using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGrid
{
    public class WinGridConfig
    {
        public Dictionary<string, (int WidthDivisions, int HeightDivisions)> Configurations = new Dictionary<string, (int WidthDivisions, int HeightDivisions)>();
    }
}
