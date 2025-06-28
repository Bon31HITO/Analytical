using ChromatogramPlotter.Models;
using ChromatogramPlotter.Mvvm;
using ChromatogramPlotter.Services;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChromatogramPlotter.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {
        private readonly CsvParser _csvParser = new();
        private readonly SvgChromatogramPlotter _svgPlotter = new();
        private PlotOptions _plotOptions = new PlotOptions();
        private string? _loadedFilePath;

        private static readonly List<string> PlotColors = new List<string>
        {
            "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
            "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf"
        };

        // System.Drawing.Text.InstalledFontCollection の代わりに WPFネイティブのFontsクラスを使用
        public List<string> SystemFonts { get; } = Fonts.SystemFontFamilies
                                                        .Select(f => f.ToString())
                                                        .OrderBy(f => f)
                                                        .ToList();

        private string _title = "SVG Chromatogram Plotter";
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private string? _svgContent;
        public string? SvgContent { get => _svgContent; set => SetProperty(ref _svgContent, value); }

        private int _headerRowCount = 1;
        public int HeaderRowCount { get => _headerRowCount; set => SetProperty(ref _headerRowCount, value); }

        public ObservableCollection<SelectableSeriesViewModel> AvailableSeries { get; } = new();
        public ObservableCollection<PropertyItemViewModel> PropertyItems { get; } = new();

        private string _statusText = "準備完了";
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

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
                var seriesList = dataSet.Series;
                for (int i = 0; i < seriesList.Count; i++)
                {
                    var series = seriesList[i];
                    series.Color = PlotColors[i % PlotColors.Count];
                    var vm = new SelectableSeriesViewModel(series);
                    vm.PropertyChanged += OnSeriesPropertyChanged;
                    AvailableSeries.Add(vm);
                }

                Title = $"Plotter - {Path.GetFileName(_loadedFilePath)}";
                PopulateProperties();
                Replot();
                ShowStatusMessage($"{Path.GetFileName(_loadedFilePath)} を読み込みました");
            }
            catch (System.Exception ex)
            {
                ShowStatusMessage($"エラー: ファイル処理に失敗しました。 {ex.Message}");
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
                var vm = new PropertyItemViewModel(_plotOptions, prop, Replot);
                if (prop.Name == nameof(PlotOptions.FontFamily))
                {
                    vm.OptionsSource = SystemFonts;
                }
                PropertyItems.Add(vm);
            }
        }

        private void OnSeriesPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectableSeriesViewModel.IsSelected) ||
                e.PropertyName == nameof(SelectableSeriesViewModel.Color))
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
                    ShowStatusMessage($"保存しました: {saveFileDialog.FileName}");
                }
                catch (System.Exception ex)
                {
                    ShowStatusMessage($"エラー: 保存に失敗しました。 {ex.Message}");
                }
            }
        }

        private void ExecuteCopySvg(object? parameter)
        {
            if (string.IsNullOrEmpty(SvgContent)) return;
            try
            {
                var dataObject = new DataObject();

                byte[] svgBytes = Encoding.UTF8.GetBytes(SvgContent);
                var svgStream = new MemoryStream(svgBytes);
                dataObject.SetData("image/svg+xml", svgStream);

                dataObject.SetData(DataFormats.UnicodeText, SvgContent);

                Clipboard.SetDataObject(dataObject, true);
                ShowStatusMessage("画像とSVGをクリップボードにコピーしました");
            }
            catch (System.Exception ex)
            {
                ShowStatusMessage($"エラー: クリップボードへのコピーに失敗しました。 {ex.Message}");
            }
        }

        private void ExecuteExit(object? parameter) => Application.Current.Shutdown();

        private async void ShowStatusMessage(string message, int durationSeconds = 5)
        {
            StatusText = message;
            await Task.Delay(durationSeconds * 1000);
            if (StatusText == message)
            {
                StatusText = "準備完了";
            }
        }
    }
}