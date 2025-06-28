using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromatogramPlotter.Models
{
    class ChromatogramDataSet
    {
        public List<ChromatogramSeries> Series { get; set; } = new List<ChromatogramSeries>();
        public List<string> HeaderLines { get; set; } = new List<string>();
    }

    class ChromatogramSeries
    {
        public string Name { get; set; } = "Unnamed";
        public List<ChromatogramPoint> Points { get; set; } = new List<ChromatogramPoint>();
        public string Color { get; set; } = "black";
    }
}
