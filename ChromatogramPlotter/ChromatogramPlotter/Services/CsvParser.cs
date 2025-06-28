using ChromatogramPlotter.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ChromatogramPlotter.Services
{
    class CsvParser
    {
        public ChromatogramDataSet ParseMultiSeries(string filePath, int headerRowCount)
        {
            var dataSet = new ChromatogramDataSet();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length <= headerRowCount)
            {
                return dataSet;
            }

            dataSet.HeaderLines.AddRange(lines.Take(headerRowCount));
            string[] seriesNames = dataSet.HeaderLines.LastOrDefault()?.Split(',') ?? new string[0];
            var dataLines = lines.Skip(headerRowCount).ToList();
            if (!dataLines.Any())
            {
                return dataSet;
            }

            int columnCount = dataLines.First(l => !string.IsNullOrWhiteSpace(l)).Split(',').Length;
            int seriesCount = columnCount / 2;

            for (int i = 0; i < seriesCount; i++)
            {
                dataSet.Series.Add(new ChromatogramSeries
                {
                    Name = (i * 2 < seriesNames.Length) ? seriesNames[i * 2].Trim() : $"Series {i + 1}",
                });
            }

            foreach (var line in dataLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(',');
                for (int i = 0; i < seriesCount; i++)
                {
                    int timeIndex = i * 2;
                    int intensityIndex = i * 2 + 1;

                    if (intensityIndex < values.Length &&
                        double.TryParse(values[timeIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out double time) &&
                        double.TryParse(values[intensityIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out double intensity))
                    {
                        dataSet.Series[i].Points.Add(new ChromatogramPoint { Time = time, Intensity = intensity });
                    }
                }
            }
            dataSet.Series.RemoveAll(s => !s.Points.Any());
            return dataSet;
        }
    }
}
