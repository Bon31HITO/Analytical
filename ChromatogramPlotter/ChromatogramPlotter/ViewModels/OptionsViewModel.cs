using ChromatogramPlotter.Models;
using ChromatogramPlotter.Mvvm;

namespace ChromatogramPlotter.ViewModels
{
    class OptionsViewModel : ObservableObject
    {
        private double _lineWidth;
        public double LineWidth { get => _lineWidth; set => SetProperty(ref _lineWidth, value); }

        private int _xAxisTickCount;
        public int XAxisTickCount { get => _xAxisTickCount; set => SetProperty(ref _xAxisTickCount, value); }

        private int _yAxisTickCount;
        public int YAxisTickCount { get => _yAxisTickCount; set => SetProperty(ref _yAxisTickCount, value); }

        private int _legendFontSize;
        public int LegendFontSize { get => _legendFontSize; set => SetProperty(ref _legendFontSize, value); }

        public void LoadFromModel(PlotOptions options)
        {
            LineWidth = options.LineWidth;
            XAxisTickCount = options.XAxisTickCount;
            YAxisTickCount = options.YAxisTickCount;
            LegendFontSize = options.LegendFontSize;
        }

        public void ApplyToModel(PlotOptions options)
        {
            options.LineWidth = LineWidth;
            options.XAxisTickCount = XAxisTickCount;
            options.YAxisTickCount = YAxisTickCount;
            options.LegendFontSize = LegendFontSize;
        }
    }
}