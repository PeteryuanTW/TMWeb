using Microsoft.CodeAnalysis.Text;
using OmniSharp.Models;

namespace TMWeb.Services
{
    internal static class SourceTextExtensions
    {
        public static int GetTextPosition(this SourceText sourceText, Request request)
        {
            return sourceText.Lines.GetPosition(new LinePosition(request.Line, request.Column));
        } 
    }
}
