// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.Tools.Formatters
{
    internal abstract class AbstractCodeFormatter : ICodeFormatter
    {
        /// <summary>
        /// Applies formatting and returns a formatted <see cref="Solution"/>
        /// </summary>
        public async Task<Solution> FormatAsync(
            Solution solution, 
            ImmutableArray<(Document, OptionSet)> formattableDocuments,
            ILogger logger, 
            CancellationToken cancellationToken)
        {
            var formattedDocuments = FormatFiles(formattableDocuments, logger, cancellationToken);
            return await ApplyFileChangesAsync(solution, formattedDocuments, logger, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Applies formatting and returns the changed <see cref="SourceText"/> for each <see cref="Document"/>.
        /// </summary>
        protected abstract ImmutableArray<(Document, Task<SourceText>)> FormatFiles(
            ImmutableArray<(Document, OptionSet)> formattableDocuments, 
            ILogger logger, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Applies the changed <see cref="SourceText"/> for each formatted <see cref="Document"/>.
        /// </summary>
        private static async Task<Solution> ApplyFileChangesAsync(
            Solution solution, 
            ImmutableArray<(Document, Task<SourceText>)> formattedDocuments,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            var formattedSolution = solution;
            var filesFormatted = 0;

            foreach (var (document, formatTask) in formattedDocuments)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return formattedSolution;
                }

                var text = await formatTask.ConfigureAwait(false);
                if (text is null)
                {
                    continue;
                }

                formattedSolution = formattedSolution.WithDocumentText(document.Id, text);

                logger.LogInformation(Resources.Formatted_code_file_0, Path.GetFileName(document.FilePath));

                filesFormatted++;
            }

            return formattedSolution;
        }
    }
}
