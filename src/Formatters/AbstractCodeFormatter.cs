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
        public async Task<Solution> FormatAsync(
            ILogger logger, 
            Solution solution, 
            ImmutableArray<(Document, OptionSet)> formatableDocuments,
            CancellationToken cancellationToken)
        {
            var formattedDocuments = FormatFiles(logger, formatableDocuments, cancellationToken);
            return await ApplyFileChangesAsync(logger, solution, formattedDocuments);
        }

        protected abstract ImmutableArray<(Document, Task<SourceText>)> FormatFiles(
            ILogger logger, 
            ImmutableArray<(Document, OptionSet)> formatableDocuments, 
            CancellationToken cancellationToken);

        private static async Task<Solution> ApplyFileChangesAsync(
            ILogger logger,
            Solution solution, 
            ImmutableArray<(Document, Task<SourceText>)> formattedDocuments)
        {
            var formattedSolution = solution;
            var filesFormatted = 0;

            foreach (var (document, formatTask) in formattedDocuments)
            {
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
