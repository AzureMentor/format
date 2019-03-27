// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.Tools.Formatters
{
    internal sealed class WhitespaceFormatter : DocumentFormatter
    {
        protected override async Task<SourceText> FormatFileAsync(
            Document document,
            OptionSet options,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            logger.LogTrace(Resources.Formatting_code_file_0, Path.GetFileName(document.FilePath));

            var formattedDocument = await Formatter.FormatAsync(document, options, cancellationToken).ConfigureAwait(false);
            var formattedSourceText = await formattedDocument.GetTextAsync(cancellationToken).ConfigureAwait(false);
            if (formattedSourceText.ContentEquals(await document.GetTextAsync(cancellationToken).ConfigureAwait(false)))
            {
                // Avoid touching files that didn't actually change
                return null;
            }

            return formattedSourceText;
        }
    }
}
