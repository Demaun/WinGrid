using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WinGridApp 
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionWindow : Window, INotifyPropertyChanged
    {
        public Visibility ShowConfig { get; }
        public int Columns { get; }
        public int Rows { get; }

        public event Action<Button> ClickDown;
        public event Action<Button> ClickUp;
        public event Action<Button> ClickDrag;
        public event Action ConfigButtonClick;
        public SelectionWindow(bool showConfig, int rows, int columns)
        {
            ShowConfig = showConfig ? Visibility.Visible : Visibility.Collapsed;
            Rows = rows;
            Columns = columns;
            DataContext = this;
            
            InitializeComponent();

            for(int i = 0; i < Rows * Columns; i++)
            {
                var btn = new Button();
                btn.Tag = false;
                btn.MouseMove += (s, e) => ClickDrag?.Invoke((Button)s);
                btn.PreviewMouseDown += (s, e) => ClickDown?.Invoke((Button)s);
                btn.PreviewMouseUp += (s, e) => ClickUp?.Invoke((Button)s);
                grdButtons.Children.Add(btn);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged( [CallerMemberName] string caller = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        private void ConfigButtonClicked(object sender, RoutedEventArgs e)
        {
            ConfigButtonClick?.Invoke();
        }
    }
}
