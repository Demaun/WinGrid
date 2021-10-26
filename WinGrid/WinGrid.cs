using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;
using System.Drawing;

namespace WinGridApp
{
    public class WinGrid
    {
        public WinGrid()
        {
            Configuration = new ConfigurationManager();
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, () =>
            {
                lock (this)
                {
                    if (ConfigWindow != null)
                    {
                        ConfigWindow = new ConfigurationWindow(this, Configuration);
                        ConfigWindow.Show();
                    }
                    else
                    {
                        ConfigWindow.Close();
                        ConfigWindow = null;
                    }
                }
            }));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left, () => MoveWindow(Direction.Left, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right, () => MoveWindow(Direction.Right, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up, () => MoveWindow(Direction.Up, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down, () => MoveWindow(Direction.Down, MoveType.Normal)));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left, () => MoveWindow(Direction.Left, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direction.Right, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up, () => MoveWindow(Direction.Up, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down, () => MoveWindow(Direction.Down, MoveType.Expand)));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Left, () => MoveWindow(Direction.Left, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direction.Right, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Up, () => MoveWindow(Direction.Up, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Down, () => MoveWindow(Direction.Down, MoveType.Contract)));
        }

        private List<KeyboardHook> Hooks = new List<KeyboardHook>();
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        private ConfigurationManager Configuration;
        private System.Windows.Window ConfigWindow;

        private const int Interval = 1;
        private static readonly Size Left = new Size(-Interval, 0);
        private static readonly Size Right = new Size(Interval, 0);
        private static readonly Size Up = new Size(0, -Interval);
        private static readonly Size Down = new Size(0, Interval);
        private static readonly Dictionary<Direction, Size> Cardinal = new Dictionary<Direction, Size>
        {
            {Direction.Left, Left },
            {Direction.Right, Right },
            {Direction.Up, Up },
            {Direction.Down, Down }
        };

        

        public void MoveWindow(Direction direction, MoveType moveType)
        {
            var handleForeground = PInvoke.GetForegroundWindow();

            var rect = new PInvoke.RECT();
            var success = PInvoke.GetWindowRect(handleForeground, ref rect);

            if (!success) return;
            
            switch (moveType)
            {
                case MoveType.Normal:
                    TranslateWindow(handleForeground, rect, direction);
                    break;
                case MoveType.Expand:
                    ExpandWindow(handleForeground, rect, direction);
                    break;
                case MoveType.Contract:
                    ContractWindow(handleForeground, rect, direction);
                    break;
            }
        }

        private void TranslateWindow(IntPtr handle, PInvoke.RECT rect, Direction direction)
        {
            var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.W, rect.H), false);

            if (direction == Direction.Up)
            {
                rect.Top = newSector.Top;
                rect.Bottom = newSector.Bottom;
            }
            else if (direction == Direction.Down)
            {
                rect.Bottom = newSector.Bottom;
                rect.Top = newSector.Top;
            }
            else if (direction == Direction.Left)
            {
                rect.Left = newSector.Left;
                rect.Right = newSector.Right;
            }
            else //if (direction == Direction.Right)
            {
                rect.Right = newSector.Right;
                rect.Left = newSector.Left;
            }

            PInvoke.SetWindowRect(handle, rect);
        }

        private void ExpandWindow(IntPtr handle, PInvoke.RECT rect, Direction direction)
        {
            var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.W, rect.H), false);

            if (direction == Direction.Up)
            {
                rect.Top = newSector.Top;
            }
            else if (direction == Direction.Down)
            {
                rect.Bottom = newSector.Bottom;
            }
            else if (direction == Direction.Left)
            {
                rect.Left = newSector.Left;
            }
            else //if (direction == Direction.Right)
            {
                rect.Right = newSector.Right;
            }

            PInvoke.SetWindowRect(handle, rect);
        }

        private void ContractWindow(IntPtr handle, PInvoke.RECT rect, Direction direction)
        {
            var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.W, rect.H), true);
            int minW = newSector.Width;
            int minH = newSector.Height;

            if (direction == Direction.Up)
            {
                rect.Bottom = newSector.Bottom;
                rect.Top = Math.Min(rect.Top, rect.Bottom - minH);
            }
            else if (direction == Direction.Down)
            {
                rect.Top = newSector.Top;
                rect.Bottom = Math.Max(rect.Bottom, rect.Top + minH);
            }
            else if (direction == Direction.Left)
            {
                rect.Right = newSector.Right;
                rect.Left = Math.Min(rect.Left, rect.Right - minW);
            }
            else //if (direction == Direction.Right)
            {
                rect.Left = newSector.Left;
                rect.Right = Math.Max(rect.Right, rect.Left + minW);
            }

            PInvoke.SetWindowRect(handle, rect);
        }


        public Rectangle GetSector(Direction direction, Rectangle fromSector, bool contract)
        {
            Point point = GetEdge(fromSector, direction, contract);

            if (contract)
            {
                var screen = Screen.FromPoint(point);
                var config = Configuration.Configs[screen.Bounds];
                double x, y, w, h;
                w = screen.WorkingArea.Width / (double)config.WidthDivisions;
                h = screen.WorkingArea.Height / (double)config.HeightDivisions;

                point.X += (int)(Cardinal[direction].Width * (w + Interval));
                point.Y += (int)(Cardinal[direction].Height * (h + Interval));

                screen = Screen.FromPoint(point);
                config = Configuration.Configs[screen.Bounds];

                if (!screen.WorkingArea.Contains(point))
                {
                    point = new Point(Clamp(point.X, screen.WorkingArea.Left + Interval, screen.WorkingArea.Right - Interval),
                                      Clamp(point.Y, screen.WorkingArea.Top + Interval, screen.WorkingArea.Bottom - Interval));
                }

                w = screen.WorkingArea.Width / (double)config.WidthDivisions;
                h = screen.WorkingArea.Height / (double)config.HeightDivisions;

                x = Math.Floor((point.X - screen.WorkingArea.Left) / (double)screen.WorkingArea.Width * config.WidthDivisions) * w + screen.WorkingArea.Left;
                y = Math.Floor((point.Y - screen.WorkingArea.Top) / (double)screen.WorkingArea.Height * config.HeightDivisions) * h + screen.WorkingArea.Top;

                return new Rectangle((int)x, (int)y, (int)Math.Ceiling(w), (int)Math.Ceiling(h));
            }
            else
            {
                var screen = Screen.FromPoint(point);
                do
                {
                    point = Point.Add(point, Cardinal[direction]);

                } while (Configuration.Bounds.Contains(point) && !screen.WorkingArea.Contains(point) && screen.Bounds.Contains(point));

                screen = Screen.FromPoint(point);

                if (!screen.WorkingArea.Contains(point))
                {
                    point = new Point(Clamp(point.X, screen.WorkingArea.Left + Interval, screen.WorkingArea.Right - Interval),
                                      Clamp(point.Y, screen.WorkingArea.Top + Interval, screen.WorkingArea.Bottom - Interval));
                }

                var config = Configuration.Configs[screen.Bounds];
                double x, y, w, h;
                w = screen.WorkingArea.Width / (double)config.WidthDivisions;
                h = screen.WorkingArea.Height / (double)config.HeightDivisions;
                x = Math.Floor((point.X - screen.WorkingArea.Left) / (double)screen.WorkingArea.Width * config.WidthDivisions) * w + screen.WorkingArea.Left;
                y = Math.Floor((point.Y - screen.WorkingArea.Top) / (double)screen.WorkingArea.Height * config.HeightDivisions) * h + screen.WorkingArea.Top;
                return new Rectangle((int)x, (int)y, (int)Math.Ceiling(w), (int)Math.Ceiling(h));
            }
        }

        public int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private Point GetEdge(Rectangle rect, Direction direction, bool opposite)
        {
            Point point = new Point();

            if (direction == Direction.Up)
            {
                point = new Point(rect.Left + rect.Width / 2, opposite ? rect.Bottom : rect.Top);
            }
            else if (direction == Direction.Down)
            {
                point = new Point(rect.Left + rect.Width / 2, opposite ? rect.Top : rect.Bottom);
            }
            else if (direction == Direction.Left)
            {
                point = new Point(opposite ? rect.Right : rect.Left, rect.Top + rect.Height / 2);
            }
            else
            {
                point = new Point(opposite ? rect.Left : rect.Right, rect.Top + rect.Height / 2);
            }
            return point;
        }

        public void Close()
        {
            Console.WriteLine("Closing...");

            foreach (var hook in Hooks)
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
    }
}
