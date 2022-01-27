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
        public const int MinDivisions = 1;
        public const int MaxDivisions = 6;

        [JsonIgnore]
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        private int _widthDivisions = 1;
        public int WidthDivisions
        {
            get
            {
                return (_widthDivisions = Math.Max(Math.Min(_widthDivisions, MaxDivisions), MinDivisions));
            }
            set
            {
                _widthDivisions = Math.Max(Math.Min(value, MaxDivisions), MinDivisions);
                OnPropertyChanged();
            }
        }

        private int _heightDivisions = 1;
        public int HeightDivisions 
        {
            get
            {
                return (_heightDivisions = Math.Max(Math.Min(_heightDivisions, MaxDivisions), MinDivisions));
            }
            set
            {
                if(value > 0)
                {
                    _heightDivisions = Math.Max(Math.Min(value, MaxDivisions), MinDivisions);
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
