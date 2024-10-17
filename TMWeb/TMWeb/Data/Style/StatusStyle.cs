using DevExpress.Blazor;
using System.Drawing;

namespace TMWeb.Data.Style
{
    public class StatusStyle
    {
        public Status status { get; init; }
        public ButtonRenderStyle buttonRenderStyle { get; init; }
        public Color StyleColor { get; init; }
        public string ColorRGBString => $"RGB({StyleColor.R}, {StyleColor.G}, {StyleColor.B})";
        public string ColorHTMLString => $"#{StyleColor.R:X2}{StyleColor.G:X2}{StyleColor.B:X2}";
        public StatusStyle(Status status, ButtonRenderStyle buttonRenderStyle, Color color)
        {
            this.status = status;
            this.buttonRenderStyle = buttonRenderStyle;
            this.StyleColor = color;
        }
    }
}
