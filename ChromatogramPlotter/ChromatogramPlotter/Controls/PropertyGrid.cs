using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ChromatogramPlotter.Controls
{
    /// <summary>
    /// オブジェクトのプロパティを一覧表示し、編集するための汎用的なコントロール。
    /// </summary>
    class PropertyGrid : Control
    {
        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        // 表示するプロパティのコレクションを受け取るための依存関係プロパティ
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable), // ViewModelのObservableCollection<PropertyItemViewModel>を受け取る
                typeof(PropertyGrid),
                new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }
}