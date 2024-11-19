using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TMWeb.Data.Script
{
    public struct ScriptDiagnosis
    {
        public LinePosition Start { get; init; }

        public int StartLine => Start.Line;
        public int StartChar => Start.Character;
        public LinePosition End { get; init; }

        public int EndLine => End.Line;

        public int EndChar => End.Character;

        public string StartPosString => $"({StartLine}, {StartChar})";
        public string Message { get; init; }
        public int Severity { get; init; }

        public ScriptDiagnosis() { }

        public ScriptDiagnosis(Diagnostic diagnostic)
        {
            Start = new LinePosition(diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1, diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1);
            End = new LinePosition(diagnostic.Location.GetLineSpan().EndLinePosition.Line + 1, diagnostic.Location.GetLineSpan().EndLinePosition.Character + 1);
            Message = diagnostic.GetMessage();
            Severity = (int)diagnostic.Severity;
        }
    }
}
