using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;
using System.Drawing;
using Rect = PInvoke.RECT;
using System.Threading.Tasks;
using refactorsaurusrex;
using Window = System.Windows.Window;
using Button = System.Windows.Controls.Button;
using System.Text;

namespace WinGridApp
{
    public class WinGrid
    {
        public WinGrid()
        {
            _hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, () =>
            {
                if(_windows.Count > 0 || _configWindow != null)
                {
                    CloseConfigWindow();
                    ClearWindows();
                    return;
                }

                IntPtr hwnd = PInvoke.GetForegroundWindow();
                var sb = new StringBuilder(100);
                try
                {
                    var windowTextLen = PInvoke.GetWindowText(hwnd, sb, 100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }
                var windowText = sb.ToString();
                Console.WriteLine($"Window text: '{windowText}'");
                if(windowText == "" || windowText == "Search")
                {
                    return;
                }
                else if(windowText == "WinGridSelection" || _windows.Count > 0)
                {
                    ClearWindows();
                }
                _window = hwnd;
                bool first = true;
                foreach (var pair in _configuration.CurrentConfigs)
                {
                    var window = new SelectionWindow(first, pair.Value.HeightDivisions, pair.Value.WidthDivisions);
                    window.Left = pair.Key.Left;
                    window.Width = pair.Key.Width;
                    window.Top = pair.Key.Top;
                    window.Height = pair.Key.Height;
                    window.ClickDown += Window_ClickDown;
                    window.ClickUp += Window_ClickUp;
                    window.ConfigButtonClick += ShowConfigWindow;
                    window.ClickDrag += Window_ClickDrag;
                    window.PreviewMouseUp += Window_PreviewMouseUp;
                    _windows.Add(window);
                    first = false;
                    window.Show();
                }
            }));
        }

        private void Window_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_downButtons.Count > 0)
            {
                Window_ClickUp(_downButtons.First());
            }
        }

        private void Window_ClickDrag(Button obj)
        {
            if(_isMouseDown)
            {
                obj.IsEnabled = false;
                _downButtons.Add(obj);
            }
        }

        private void Window_ClickUp(Button obj)
        {
            var offset = obj.PointToScreen(new System.Windows.Point(0, 0));
            var bounds = new Rectangle((int)offset.X, (int)offset.Y, (int)obj.ActualWidth, (int)obj.ActualHeight);

            Console.WriteLine($"UP: {bounds}");
            foreach (var btn in _downButtons)
            {
                obj.IsEnabled = true;
                offset = btn.PointToScreen(new System.Windows.Point(0, 0));
                var downBounds = new Rectangle((int)offset.X, (int)offset.Y, (int)btn.ActualWidth, (int)btn.ActualHeight);

                Console.WriteLine($"DOWN: {downBounds}");
                bounds = Rectangle.Union(bounds, downBounds);
            }

            ClearWindows();
            Console.WriteLine($"BOUNDS: {bounds}");
            try
            {
                Rect oldRect = new Rect();
                Rect currentRect = new Rect();
                PInvoke.GetWindowRect(_window, ref oldRect);
                PInvoke.SetWindowRect(_window, new Rect(bounds));
                PInvoke.GetWindowRect(_window, ref currentRect);
                if (oldRect == currentRect)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            PInvoke.UnmaximizeWindow(_window);
                            for (int i = 0; i < 7; i++)
                            {
                                await Task.Delay(i * 10);
                                PInvoke.SetWindowRect(_window, new Rect(bounds));
                            }
                        }
                        catch { }
                    });
                }
            }
            catch { }
        }

        private void Window_ClickDown(Button obj)
        {
            _isMouseDown = true;
            obj.ReleaseMouseCapture();
            Window_ClickDrag(obj);
        }

        private List<KeyboardHook> _hooks = new List<KeyboardHook>();
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        private ConfigurationManager _configuration = new ConfigurationManager();
        private Window _configWindow;
        private List<Window> _windows = new List<Window>();
        private HashSet<Button> _downButtons = new HashSet<Button>();
        private bool _isMouseDown = false;
        private IntPtr _window;


        private void ShowConfigWindow()
        {
            lock (this)
            {
                if (_configWindow == null || !_configWindow.IsVisible)
                {
                    _configWindow?.Close();
                    _configWindow = new ConfigurationWindow(this, _configuration);
                    _configWindow.Show();
                    _configWindow.Activate();
                }
                else
                {
                    CloseConfigWindow();
                }

                ClearWindows();
            }
        }
        private void CloseConfigWindow()
        {
            if(_configWindow != null)
            {
                _configWindow.Close();
                _configWindow = null;
            }
        }

        private void ClearWindows()
        {
            _isMouseDown = false;
            _downButtons.Clear();
            foreach (var window in _windows)
            {
                window.Close();
            }
            _windows.Clear();
        }

        #region Config Independent
        public void Close()
        {
            Console.WriteLine("Closing...");

            foreach (var hook in _hooks)
            {
                hook.Disengage();
                hook.Dispose();
            }

            Application.Current.Shutdown();
        }

        private KeyboardHook GetHook(KeysEx keys, Action action)
        {
            var hook = new KeyboardHook();
            hook.AutoRepeat = true;
            hook.AllowPassThrough = false;
            hook.SetKeys(new KeyCombination(keys));
            hook.Pressed += (s, e) => Dispatcher.Invoke(action);
            hook.Engage();
            return hook;
        }

        #endregion
    }
}
