using ChromatogramPlotter.Mvvm;
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace ChromatogramPlotter.ViewModels
{
    /// <summary>
    /// プロパティグリッドの一つの行を表すViewModel。
    /// </summary>
    class PropertyItemViewModel : ObservableObject
    {
        private readonly object _instance;
        private readonly PropertyInfo _propertyInfo;
        private readonly Action _onPropertyChangedCallback;

        public string DisplayName { get; }
        public Type PropertyType => _propertyInfo.PropertyType;

        // 選択肢を持つプロパティ用のリスト (例: ComboBoxの項目)
        public IEnumerable? OptionsSource { get; set; }

        private object? _value;
        public object? Value
        {
            get => _value;
            set
            {
                // UIからの入力値(通常は文字列)を正しい型に変換しようと試みる
                try
                {
                    var convertedValue = Convert.ChangeType(value, PropertyType);
                    if (SetProperty(ref _value, convertedValue))
                    {
                        // 実際のモデルオブジェクトに値を設定
                        _propertyInfo.SetValue(_instance, _value);
                        // 変更を通知して再描画をトリガー
                        _onPropertyChangedCallback?.Invoke();
                    }
                }
                catch (Exception)
                {
                    // 変換に失敗した場合(例: 数値プロパティに文字を入力)、値を元に戻す
                    OnPropertyChanged(); // UIを元の値に強制的に戻す通知
                }
            }
        }

        public PropertyItemViewModel(object instance, PropertyInfo propertyInfo, Action onPropertyChangedCallback)
        {
            _instance = instance;
            _propertyInfo = propertyInfo;
            _onPropertyChangedCallback = onPropertyChangedCallback;

            // 属性から表示名を取得、なければプロパティ名をそのまま使用
            DisplayName = _propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? _propertyInfo.Name;
            _value = _propertyInfo.GetValue(instance);
        }
    }
}