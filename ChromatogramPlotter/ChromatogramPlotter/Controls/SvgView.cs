using ChromatogramPlotter.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChromatogramPlotter.Controls
{
    class SvgView : Control
    {
        private readonly SvgParser _parser = new SvgParser();
        private Canvas? _drawingCanvas;

        // 静的コンストラクタで、このコントロールのデフォルトスタイルが
        // Themes/Generic.xaml にあることをWPFに通知します。
        static SvgView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SvgView), new FrameworkPropertyMetadata(typeof(SvgView)));
        }

        // SvgSource 依存関係プロパティの定義 (変更なし)
        public static readonly DependencyProperty SvgSourceProperty =
            DependencyProperty.Register(
                "SvgSource",
                typeof(string),
                typeof(SvgView),
                new PropertyMetadata(null, OnSvgSourceChanged));

        public string SvgSource
        {
            get { return (string)GetValue(SvgSourceProperty); }
            set { SetValue(SvgSourceProperty, value); }
        }

        private static void OnSvgSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SvgView svgView)
            {
                svgView.RenderSvg(e.NewValue as string);
            }
        }

        /// <summary>
        /// コントロールのテンプレートが適用されたときに呼び出されます。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // テンプレートから描画用のCanvasを取得します。
            // "PART_" という接頭辞は、コントロールのテンプレートに必須の要素であることを示す規約です。
            _drawingCanvas = GetTemplateChild("PART_DrawingCanvas") as Canvas;

            // テンプレートが適用された後に、再度SVGを描画します。
            RenderSvg(SvgSource);
        }

        private void RenderSvg(string? svgContent)
        {
            // OnApplyTemplateが呼ばれる前は_drawingCanvasがnullなので何もしない
            if (_drawingCanvas == null)
            {
                return;
            }

            _drawingCanvas.Children.Clear();

            if (string.IsNullOrWhiteSpace(svgContent))
            {
                _drawingCanvas.Width = 0;
                _drawingCanvas.Height = 0;
                return;
            }

            try
            {
                var (elements, viewboxSize) = _parser.Parse(svgContent);
                _drawingCanvas.Width = viewboxSize.Width;
                _drawingCanvas.Height = viewboxSize.Height;

                foreach (var element in elements)
                {
                    _drawingCanvas.Children.Add(element);
                }
            }
            catch (System.Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = $"SVG Render Error:\n{ex.Message}",
                    Foreground = Brushes.Red,
                    FontSize = 16
                };
                _drawingCanvas.Width = 400;
                _drawingCanvas.Height = 300;
                Canvas.SetLeft(errorText, 10);
                Canvas.SetTop(errorText, 10);
                _drawingCanvas.Children.Add(errorText);
            }
        }
    }
}
