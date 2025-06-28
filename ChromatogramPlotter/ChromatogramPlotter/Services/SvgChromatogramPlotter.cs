using ChromatogramPlotter.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ChromatogramPlotter.Services
{
    class SvgChromatogramPlotter
    {
        public string Plot(List<ChromatogramSeries> seriesToPlot, PlotOptions options)
        {
            if (seriesToPlot == null || !seriesToPlot.Any(s => s.Points.Any()))
                return $"<svg width='{options.Width}' height='{options.Height}' xmlns='http://www.w3.org/2000/svg' font-family='{options.FontFamily}, sans-serif'><text x='50%' y='50%' text-anchor='middle'>No data to display</text></svg>";

            var allPoints = seriesToPlot.SelectMany(s => s.Points).ToList();
            double minTime = allPoints.Min(p => p.Time);
            double maxTime = allPoints.Max(p => p.Time);
            double minIntensity = 0;
            double maxIntensity = allPoints.Any() ? allPoints.Max(p => p.Intensity) : 1;

            if (maxIntensity == 0) maxIntensity = 1;
            if (maxTime == minTime) maxTime = minTime + 1;

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?>");
            sb.AppendLine($"<svg width='{options.Width}' height='{options.Height}' xmlns='http://www.w3.org/2000/svg' font-family='{options.FontFamily}, sans-serif' stroke-linecap='round' stroke-linejoin='round'>");
            sb.AppendLine($"<rect x='0' y='0' width='{options.Width}' height='{options.Height}' fill='{options.BackgroundColor}' />");

            sb.AppendLine($"<text x='{options.Width / 2.0}' y='{options.Margin.Top / 2.0}' font-size='{options.TitleFontSize}' font-weight='bold' text-anchor='middle'>{options.Title}</text>");

            double plotWidth = options.Width - options.Margin.Left - options.Margin.Right;
            double plotHeight = options.Height - options.Margin.Top - options.Margin.Bottom;

            DrawAxesAndLabels(sb, options, plotWidth, plotHeight, minTime, maxTime, minIntensity, maxIntensity);

            sb.AppendLine($"<g id='plot-area'>");
            sb.AppendLine($"<defs><clipPath id='plotAreaClip'><rect x='{options.Margin.Left}' y='{options.Margin.Top}' width='{plotWidth}' height='{plotHeight}' /></clipPath></defs>");
            sb.AppendLine($"<g clip-path='url(#plotAreaClip)'>");
            foreach (var series in seriesToPlot)
            {
                var pointsString = new StringBuilder();
                foreach (var p in series.Points)
                {
                    double x = options.Margin.Left + (p.Time - minTime) / (maxTime - minTime) * plotWidth;
                    double y = options.Margin.Top + plotHeight - (p.Intensity - minIntensity) / (maxIntensity - minIntensity) * plotHeight;
                    pointsString.Append($"{x.ToString(CultureInfo.InvariantCulture)},{y.ToString(CultureInfo.InvariantCulture)} ");
                }
                sb.AppendLine($"<polyline points='{pointsString.ToString().Trim()}' fill='none' stroke='{series.Color}' stroke-width='{options.LineWidth.ToString(CultureInfo.InvariantCulture)}' />");
            }
            sb.AppendLine("</g>");
            sb.AppendLine("</g>");

            DrawLegend(sb, seriesToPlot, options);

            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        private void DrawLegend(StringBuilder sb, List<ChromatogramSeries> seriesList, PlotOptions options)
        {
            double legendX = options.Width - options.Margin.Right + 20;
            double legendY = options.Margin.Top;

            sb.AppendLine($"<g font-size='{options.LegendFontSize}'>");
            for (int i = 0; i < seriesList.Count; i++)
            {
                var series = seriesList[i];
                double currentY = legendY + (i * (options.LegendFontSize + 8));

                sb.AppendLine($"<line x1='{legendX - 22}' y1='{currentY + options.LegendFontSize / 2.0}' x2='{legendX - 8}' y2='{currentY + options.LegendFontSize / 2.0}' stroke='{series.Color}' stroke-width='{options.LineWidth}' />");

                double yBaseline = currentY + options.LegendFontSize / 2.0 + (options.LegendFontSize * 0.35);
                sb.AppendLine($"<text x='{legendX - 2}' y='{yBaseline.ToString(CultureInfo.InvariantCulture)}' text-anchor='start'>{series.Name}</text>");
            }
            sb.AppendLine("</g>");
        }

        private void DrawAxesAndLabels(StringBuilder sb, PlotOptions options, double plotWidth, double plotHeight, double minTime, double maxTime, double minIntensity, double maxIntensity)
        {
            var left = options.Margin.Left;
            var top = options.Margin.Top;
            var right = left + plotWidth;
            var bottom = top + plotHeight;

            if (options.ShowGridLines)
            {
                sb.AppendLine("<g id='grid-lines' stroke='#e0e0e0' stroke-width='0.5' stroke-dasharray='3,3'>");
                for (int i = 1; i < options.XAxisTickCount; i++)
                {
                    double x = left + plotWidth * i / options.XAxisTickCount;
                    sb.AppendLine($"<line x1='{x}' y1='{top}' x2='{x}' y2='{bottom}' />");
                }
                for (int i = 1; i < options.YAxisTickCount; i++)
                {
                    double y = top + (plotHeight * i / options.YAxisTickCount);
                    sb.AppendLine($"<line x1='{left}' y1='{y}' x2='{right}' y2='{y}' />");
                }
                sb.AppendLine("</g>");
            }

            sb.AppendLine($"<g id='axis-lines' stroke='black' stroke-width='{options.AxisWidth}'>");
            sb.AppendLine($"<line x1='{left}' y1='{bottom}' x2='{right}' y2='{bottom}' />");
            sb.AppendLine($"<line x1='{left}' y1='{top}' x2='{left}' y2='{bottom}' />");
            sb.AppendLine("</g>");

            sb.AppendLine($"<g id='ticks-and-labels' fill='black' font-size='{options.TickLabelFontSize}'>");
            sb.AppendLine($"<g id='x-axis' text-anchor='middle'>");
            for (int i = 0; i <= options.XAxisTickCount; i++)
            {
                double x = left + plotWidth * i / options.XAxisTickCount;
                sb.AppendLine($"<line x1='{x}' y1='{bottom}' x2='{x}' y2='{bottom + 5}' stroke='black' stroke-width='{options.AxisWidth}' />");
                sb.AppendLine($"<text x='{x}' y='{bottom + 8 + options.TickLabelFontSize}'>{minTime + (maxTime - minTime) * i / options.XAxisTickCount:G3}</text>");
            }
            sb.AppendLine("</g>");

            sb.AppendLine($"<g id='y-axis' text-anchor='end'>");
            for (int i = 0; i <= options.YAxisTickCount; i++)
            {
                double y = top + plotHeight * i / options.YAxisTickCount;
                sb.AppendLine($"<line x1='{left - 5}' y1='{y}' x2='{left}' y2='{y}' stroke='black' stroke-width='{options.AxisWidth}' />");

                double yTickBaseline = y + (options.TickLabelFontSize * 0.35);
                sb.AppendLine($"<text x='{left - 10}' y='{yTickBaseline.ToString(CultureInfo.InvariantCulture)}'>{maxIntensity - (maxIntensity - minIntensity) * i / options.YAxisTickCount:G3}</text>");
            }
            sb.AppendLine("</g>");
            sb.AppendLine("</g>");

            sb.AppendLine($"<g id='axis-labels' fill='black' font-size='{options.AxisLabelFontSize}' text-anchor='middle'>");
            double xAxisLabelY = bottom + 35 + options.TickLabelFontSize;
            sb.AppendLine($"<text x='{left + plotWidth / 2.0}' y='{xAxisLabelY}'>{options.XAxisLabel}</text>");
            double yAxisLabelX = left - 55 - options.TickLabelFontSize;
            sb.AppendLine($"<text transform='rotate(-90 {yAxisLabelX} {top + plotHeight / 2.0})' x='{yAxisLabelX}' y='{top + plotHeight / 2.0}'>{options.YAxisLabel}</text>");
            sb.AppendLine("</g>");
        }
    }
}