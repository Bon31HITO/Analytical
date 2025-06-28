using ChromatogramPlotter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChromatogramPlotter.Services
{
    class SettingsService
    {
        private readonly string _filePath;

        public SettingsService()
        {
            // 設定ファイルをユーザーごとのApplication Dataフォルダに保存する
            // 例: C:\Users\<ユーザー名>\AppData\Roaming\ChromatogramPlotter\settings.json
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, "ChromatogramPlotter");
            Directory.CreateDirectory(appFolderPath); // フォルダがなければ作成
            _filePath = Path.Combine(appFolderPath, "settings.json");
        }

        /// <summary>
        /// 設定ファイルからPlotOptionsを読み込む。ファイルが存在しない場合はデフォルト値を返す。
        /// </summary>
        public PlotOptions LoadPlotOptions()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<PlotOptions>(json) ?? new PlotOptions();
                }
            }
            catch (Exception)
            {
                // ファイルの読み込みに失敗した場合もデフォルト値を返す
            }
            return new PlotOptions(); // デフォルト値を返す
        }

        /// <summary>
        /// 現在のPlotOptionsを設定ファイルに保存する。
        /// </summary>
        public void SavePlotOptions(PlotOptions options)
        {
            var optionsJson = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(options, optionsJson);
            File.WriteAllText(_filePath, json);
        }

        /// <summary>
        /// 設定ファイルを削除してデフォルト設定に戻す。
        /// </summary>
        public void ResetPlotOptions()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }

}
