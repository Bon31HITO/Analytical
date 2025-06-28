using ChromatogramPlotter.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChromatogramPlotter.Controls
{
    class PropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? BoolTemplate { get; set; }
        public DataTemplate? IntTemplate { get; set; }
        public DataTemplate? DoubleTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? FontFamilyTemplate { get; set; }
        public DataTemplate? AxisWidthTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is not PropertyItemViewModel vm) return base.SelectTemplate(item, container);

            // プロパティ名でテンプレートを振り分け
            switch (vm.DisplayName)
            {
                case "フォント":
                    return FontFamilyTemplate!;
                case "軸線の太さ":
                    return AxisWidthTemplate!;
            }

            // 型でテンプレートを振り分け
            if (vm.PropertyType == typeof(bool)) return BoolTemplate!; 
            if (vm.PropertyType == typeof(int)) return IntTemplate!;
            if (vm.PropertyType == typeof(double)) return DoubleTemplate!;
            if (vm.PropertyType == typeof(string)) return StringTemplate!;

            return base.SelectTemplate(item, container);
        }
    }
}