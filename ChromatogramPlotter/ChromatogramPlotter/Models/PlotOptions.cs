using System.ComponentModel;

namespace ChromatogramPlotter.Models
{
    class PlotOptions
    {
        [DisplayName("外観 - グリッド線を表示")]
        public bool ShowGridLines { get; set; } = true;

        [DisplayName("フォント - グラフ全体のフォント")]
        public string FontFamily { get; set; } = "Meiryo UI";

        [DisplayName("フォント - タイトル")]
        public int TitleFontSize { get; set; } = 16;

        [DisplayName("フォント - 軸ラベル")]
        public int AxisLabelFontSize { get; set; } = 16;

        [DisplayName("フォント - 目盛りラベル")]
        public int TickLabelFontSize { get; set; } = 16;

        [DisplayName("フォント - 凡例")]
        public int LegendFontSize { get; set; } = 16;

        [DisplayName("線 - プロット線")]
        public double LineWidth { get; set; } = 1.5;

        [DisplayName("線 - 軸線")]
        public double AxisWidth { get; set; } = 1.5;

        [DisplayName("軸 - X軸の目盛り数")]
        public int XAxisTickCount { get; set; } = 5;

        [DisplayName("軸 - Y軸の目盛り数")]
        public int YAxisTickCount { get; set; } = 5;

        [Browsable(false)]
        public int Width { get; set; } = 800;
        [Browsable(false)]
        public int Height { get; set; } = 480;

        [Browsable(false)]
        public Margin Margin { get; set; } = new Margin(60, 120, 90, 100); // 左と下のマージンを拡大

        [Browsable(false)]
        public string Title { get; set; } = "Chromatogram";

        [Browsable(false)]
        public string XAxisLabel { get; set; } = "Time (min)";

        [Browsable(false)]
        public string YAxisLabel { get; set; } = "Intensity";

        [Browsable(false)]
        public string BackgroundColor { get; set; } = "transparent";

        public PlotOptions Clone()
        {
            return (PlotOptions)this.MemberwiseClone();
        }
    }
}