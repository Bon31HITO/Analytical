using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ChromatogramPlotter.Services
{
    /// <summary>
    /// SVG文字列を解析し、WPFのUIElementに変換するパーサー。
    /// 属性の継承をサポートする改善版です。
    /// </summary>
    class SvgParser
    {
        public (List<UIElement> elements, Size viewboxSize) Parse(string svgContent)
        {
            var elements = new List<UIElement>();
            if (string.IsNullOrEmpty(svgContent)) return (elements, new Size());

            XDocument doc = XDocument.Parse(svgContent);
            XElement? svgNode = doc.Root;
            if (svgNode == null) return (elements, new Size());

            double width = ParseDouble(svgNode.Attribute("width")?.Value ?? "0");
            double height = ParseDouble(svgNode.Attribute("height")?.Value ?? "0");

            var initialContext = new SvgParsingContext
            {
                Fill = "black",
                Stroke = "none",
                FontSize = 12.0,
                FontFamily = svgNode.Attribute("font-family")?.Value ?? "Arial, sans-serif"
            };

            ParseNode(svgNode, initialContext, elements);

            return (elements, new Size(width, height));
        }

        private void ParseNode(XElement node, SvgParsingContext parentContext, List<UIElement> elements)
        {
            var currentContext = new SvgParsingContext(parentContext, node);

            switch (node.Name.LocalName)
            {
                case "svg":
                case "g":
                case "defs":
                    foreach (var child in node.Elements())
                    {
                        ParseNode(child, currentContext, elements);
                    }
                    break;
                case "rect":
                    elements.Add(ParseRect(node, currentContext));
                    break;
                case "line":
                    elements.Add(ParseLine(node, currentContext));
                    break;
                case "polyline":
                    elements.Add(ParsePolyline(node, currentContext));
                    break;
                case "text":
                    elements.Add(ParseText(node, currentContext));
                    break;
            }
        }

        private UIElement ParseRect(XElement node, SvgParsingContext context)
        {
            var rect = new Rectangle
            {
                Width = ParseDouble(node.Attribute("width")?.Value),
                Height = ParseDouble(node.Attribute("height")?.Value),
                Fill = ParseBrush(node.Attribute("fill")?.Value ?? context.Fill),
                RenderTransform = context.Transform
            };
            Canvas.SetLeft(rect, ParseDouble(node.Attribute("x")?.Value));
            Canvas.SetTop(rect, ParseDouble(node.Attribute("y")?.Value));
            return rect;
        }

        private UIElement ParseLine(XElement node, SvgParsingContext context)
        {
            return new Line
            {
                X1 = ParseDouble(node.Attribute("x1")?.Value),
                Y1 = ParseDouble(node.Attribute("y1")?.Value),
                X2 = ParseDouble(node.Attribute("x2")?.Value),
                Y2 = ParseDouble(node.Attribute("y2")?.Value),
                Stroke = ParseBrush(node.Attribute("stroke")?.Value ?? context.Stroke),
                StrokeThickness = ParseDouble(node.Attribute("stroke-width")?.Value ?? context.StrokeWidth.ToString(CultureInfo.InvariantCulture)),
                RenderTransform = context.Transform
            };
        }

        private UIElement ParsePolyline(XElement node, SvgParsingContext context)
        {
            var path = new Path
            {
                Stroke = ParseBrush(node.Attribute("stroke")?.Value ?? context.Stroke),
                StrokeThickness = ParseDouble(node.Attribute("stroke-width")?.Value ?? context.StrokeWidth.ToString(CultureInfo.InvariantCulture)),
                Fill = ParseBrush(node.Attribute("fill")?.Value ?? "none"),
                RenderTransform = context.Transform
            };

            var pointsAttr = node.Attribute("points")?.Value;
            if (!string.IsNullOrEmpty(pointsAttr))
            {
                var pointCollection = PointCollection.Parse(pointsAttr);
                var figure = new PathFigure { StartPoint = pointCollection.FirstOrDefault() };
                if (pointCollection.Count > 1)
                {
                    figure.Segments.Add(new PolyLineSegment(pointCollection.Skip(1), true));
                }
                path.Data = new PathGeometry { Figures = { figure } };
            }
            return path;
        }

        private UIElement ParseText(XElement node, SvgParsingContext context)
        {
            var fontSize = ParseDouble(node.Attribute("font-size")?.Value) is var fs && fs > 0 ? fs : context.FontSize;

            var textBlock = new TextBlock
            {
                Text = node.Value,
                FontSize = fontSize,
                FontFamily = new FontFamily(node.Attribute("font-family")?.Value ?? context.FontFamily),
                Foreground = ParseBrush(node.Attribute("fill")?.Value ?? context.Fill),
            };

            var canvas = new Canvas { RenderTransform = context.Transform };
            canvas.Children.Add(textBlock);

            double x = ParseDouble(node.Attribute("x")?.Value);
            double y = ParseDouble(node.Attribute("y")?.Value);

            double dyOffset = ParseRelativeUnit(node.Attribute("dy")?.Value, fontSize);

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var measuredSize = textBlock.DesiredSize;

            var textAnchor = node.Attribute("text-anchor")?.Value ?? context.TextAnchor;
            switch (textAnchor)
            {
                case "middle": Canvas.SetLeft(textBlock, x - measuredSize.Width / 2); break;
                case "end": Canvas.SetLeft(textBlock, x - measuredSize.Width); break;
                default: Canvas.SetLeft(textBlock, x); break;
            }

            double baselineApproximation = measuredSize.Height * 0.8;
            Canvas.SetTop(textBlock, y - baselineApproximation + dyOffset);

            return canvas;
        }

        private double ParseRelativeUnit(string? value, double fontSize)
        {
            if (string.IsNullOrEmpty(value)) return 0.0;
            if (value.EndsWith("em"))
            {
                if (double.TryParse(value.Replace("em", ""), CultureInfo.InvariantCulture, out double emValue))
                {
                    return emValue * fontSize;
                }
            }
            double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result);
            return result;
        }

        private static double ParseDouble(string? s)
        {
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : 0.0;
        }

        private static Brush ParseBrush(string? s)
        {
            if (string.IsNullOrEmpty(s) || s.ToLowerInvariant() == "none") return Brushes.Transparent;
            try { return (SolidColorBrush)new BrushConverter().ConvertFromString(s); }
            catch { return Brushes.Black; }
        }
    }

    internal class SvgParsingContext
    {
        public Transform Transform { get; set; } = Transform.Identity;
        public string Fill { get; set; } = "none";
        public string Stroke { get; set; } = "none";
        public double StrokeWidth { get; set; } = 1.0;
        public double FontSize { get; set; } = 12.0;
        public string FontFamily { get; set; } = "Arial";
        public string TextAnchor { get; set; } = "start";

        public SvgParsingContext() { }

        public SvgParsingContext(SvgParsingContext parent, XElement node)
        {
            Transform = parent.Transform;
            Fill = parent.Fill;
            Stroke = parent.Stroke;
            StrokeWidth = parent.StrokeWidth;
            FontSize = parent.FontSize;
            FontFamily = parent.FontFamily;
            TextAnchor = parent.TextAnchor;

            Fill = node.Attribute("fill")?.Value ?? Fill;
            Stroke = node.Attribute("stroke")?.Value ?? Stroke;
            if (double.TryParse(node.Attribute("stroke-width")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sw)) StrokeWidth = sw;
            if (double.TryParse(node.Attribute("font-size")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fs)) FontSize = fs;
            FontFamily = node.Attribute("font-family")?.Value ?? FontFamily;
            TextAnchor = node.Attribute("text-anchor")?.Value ?? TextAnchor;

            var transformAttr = node.Attribute("transform")?.Value;
            if (!string.IsNullOrEmpty(transformAttr))
            {
                var newTransform = ParseTransform(transformAttr);
                var group = new TransformGroup();
                group.Children.Add(Transform);
                group.Children.Add(newTransform);
                Transform = group;
            }
        }

        private static Transform ParseTransform(string transformString)
        {
            if (transformString.StartsWith("rotate("))
            {
                var parts = transformString.Replace("rotate(", "").Replace(")", "").Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    return new RotateTransform(double.Parse(parts[0], CultureInfo.InvariantCulture),
                                               double.Parse(parts[1], CultureInfo.InvariantCulture),
                                               double.Parse(parts[2], CultureInfo.InvariantCulture));
                }
            }
            return Transform.Identity;
        }
    }
}