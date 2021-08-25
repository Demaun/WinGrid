using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinGridApp
{
    public class WinGridConfig: INotifyPropertyChanged
    {
        [JsonIgnore]
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        private int _widthDivisions = 1;
        public int WidthDivisions
        {
            get
            {
                return _widthDivisions > 0 ? _widthDivisions : (_widthDivisions = 1);
            }
            set
            {
                if (value > 0)
                {
                    _widthDivisions = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _heightDivisions = 1;
        public int HeightDivisions 
        {
            get
            {
                return _heightDivisions > 0 ? _heightDivisions : (_heightDivisions = 1);
            }
            set
            {
                if(value > 0)
                {
                    _heightDivisions = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public WinGridConfig(int widthDivisions, int heightDivisions)
        {
            WidthDivisions = widthDivisions;
            HeightDivisions = heightDivisions;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
