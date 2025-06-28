using ChromatogramPlotter.Models;
using ChromatogramPlotter.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ChromatogramPlotter.ViewModels
{
    class SelectableSeriesViewModel : ObservableObject
    {
        // ユーザーが選択可能な色のパレット
        private static readonly List<string> PredefinedColors = new List<string>
        {
            "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
            "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf"
        };

        public ChromatogramSeries Model { get; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public string Name => Model.Name;

        // 色プロパティをViewに公開し、変更を通知できるようにする
        public string Color
        {
            get => Model.Color;
            set
            {
                if (Model.Color != value)
                {
                    Model.Color = value;
                    OnPropertyChanged(); // UIの色表示を更新
                }
            }
        }

        public ICommand ChangeColorCommand { get; }

        public SelectableSeriesViewModel(ChromatogramSeries model)
        {
            Model = model;
            _isSelected = true;
            ChangeColorCommand = new RelayCommand(ExecuteChangeColor);
        }

        private void ExecuteChangeColor(object? parameter)
        {
            int currentIndex = PredefinedColors.IndexOf(this.Color);
            // 次の色を選択（見つからなかった場合は最初から）。リストの最後に達したら先頭に戻る。
            int nextIndex = (currentIndex + 1) % PredefinedColors.Count;
            this.Color = PredefinedColors[nextIndex];
        }
    }
}