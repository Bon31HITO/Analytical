using ChromatogramPlotter.Models;
using ChromatogramPlotter.Mvvm;
using ChromatogramPlotter.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ChromatogramPlotter.ViewModels
{
    class SelectableSeriesViewModel : ObservableObject
    {
        public ChromatogramSeries Model { get; }
        private bool _isSelected;
        public string Name => Model.Name;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        public SelectableSeriesViewModel(ChromatogramSeries model)
        {
            Model = model;
            _isSelected = true;
        }
    }

    class MainWindowViewModel : ObservableObject
    {
        private readonly CsvParser _csvParser = new();
        private readonly SvgChromatogramPlotter _svgPlotter = new();
        private PlotOptions _plotOptions = new PlotOptions();
        private string? _loadedFilePath;

        private string _title = "SVG Chromatogram Plotter";
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private string? _svgContent;
        public string? SvgContent { get => _svgContent; set => SetProperty(ref _svgContent, value); }

        private int _headerRowCount = 1;
        public int HeaderRowCount { get => _headerRowCount; set => SetProperty(ref _headerRowCount, value); }

        public ObservableCollection<SelectableSeriesViewModel> AvailableSeries { get; } = new();
        public ObservableCollection<PropertyItemViewModel> PropertyItems { get; } = new();

        public ICommand OpenFileCommand { get; }
        public ICommand SaveSvgCommand { get; }
        public ICommand CopySvgCommand { get; }
        public ICommand ExitCommand { get; }

        public MainWindowViewModel()
        {
            OpenFileCommand = new RelayCommand(ExecuteOpenFile);
            SaveSvgCommand = new RelayCommand(ExecuteSaveSvg, CanExecuteSaveOrCopy);
            CopySvgCommand = new RelayCommand(ExecuteCopySvg, CanExecuteSaveOrCopy);
            ExitCommand = new RelayCommand(ExecuteExit);
        }

        private void ExecuteOpenFile(object? parameter)
        {
            var openFileDialog = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv", Title = "CSVファイルを選択" };
            if (openFileDialog.ShowDialog() == true)
            {
                _loadedFilePath = openFileDialog.FileName;
                LoadAndPlotData();
            }
        }

        private void LoadAndPlotData()
        {
            if (string.IsNullOrEmpty(_loadedFilePath)) return;
            try
            {
                var dataSet = _csvParser.ParseMultiSeries(_loadedFilePath, HeaderRowCount);
                AvailableSeries.Clear();
                foreach (var series in dataSet.Series)
                {
                    var vm = new SelectableSeriesViewModel(series);
                    vm.PropertyChanged += OnSeriesSelectionChanged;
                    AvailableSeries.Add(vm);
                }
                Title = $"Plotter - {Path.GetFileName(_loadedFilePath)}";
                PopulateProperties();
                Replot();
            }
            catch (System.Exception ex)
            {
                ShowError($"ファイル処理エラー: {ex.Message}");
                ClearData();
            }
        }

        private void PopulateProperties()
        {
            PropertyItems.Clear();
            var properties = typeof(PlotOptions).GetProperties()
                .Where(p => p.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? true);
            foreach (var prop in properties)
            {
                PropertyItems.Add(new PropertyItemViewModel(_plotOptions, prop, Replot));
            }
        }

        private void OnSeriesSelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableSeriesViewModel.IsSelected))
            {
                Replot();
            }
        }

        private void Replot()
        {
            var selectedModels = AvailableSeries.Where(vm => vm.IsSelected).Select(vm => vm.Model).ToList();
            _plotOptions.Title = Path.GetFileNameWithoutExtension(_loadedFilePath ?? "Chromatogram");
            SvgContent = _svgPlotter.Plot(selectedModels, _plotOptions);
        }

        private void ClearData()
        {
            AvailableSeries.Clear();
            PropertyItems.Clear();
            SvgContent = null;
            Title = "SVG Chromatogram Plotter";
            _loadedFilePath = null;
        }

        private bool CanExecuteSaveOrCopy(object? parameter) => !string.IsNullOrEmpty(SvgContent);

        private void ExecuteSaveSvg(object? parameter)
        {
            var saveFileDialog = new SaveFileDialog { Filter = "SVG Files (*.svg)|*.svg", FileName = "chromatogram.svg" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, SvgContent!);
                    MessageBox.Show("SVGファイルを保存しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    ShowError($"保存エラー: {ex.Message}");
                }
            }
        }

        private void ExecuteCopySvg(object? parameter)
        {
            if (string.IsNullOrEmpty(SvgContent)) return;

            try
            {
                var dataObject = new DataObject();

                // --- 1. image/svg+xml 形式 (Inkscape, Illustratorなど向け) ---
                // usingブロックを使わず、ストリームをDataObjectに委ねる
                byte[] svgBytes = Encoding.UTF8.GetBytes(SvgContent);
                var svgStream = new MemoryStream(svgBytes);
                dataObject.SetData("image/svg+xml", svgStream);

                // --- 2. UnicodeText 形式 (メモ帳など、フォールバック用) ---
                dataObject.SetData(DataFormats.UnicodeText, SvgContent);

                // 作成したDataObjectをクリップボードに設定
                Clipboard.SetDataObject(dataObject, true);

                MessageBox.Show("SVG画像、ビットマップ画像、テキストをクリップボードにコピーしました。", "コピー完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                ShowError($"クリップボードへのコピーに失敗しました: {ex.Message}");
            }
        }

        private void ExecuteExit(object? parameter) => Application.Current.Shutdown();

        private void ShowError(string message)
        {
            MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}