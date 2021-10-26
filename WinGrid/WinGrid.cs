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
                    if (ConfigWindow == null || !ConfigWindow.IsVisible)
                    {
                        ConfigWindow?.Close();
                        ConfigWindow = new ConfigurationWindow(this, Configuration);
                        ConfigWindow.Show();
                        ConfigWindow.Activate();
                    }
                    else
                    {
                        ConfigWindow.Close();
                        ConfigWindow = null;
                    }
                }
            }));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left,                                 () => MoveWindow(Direction.Left, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right,                                () => MoveWindow(Direction.Right, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up,                                   () => MoveWindow(Direction.Up, MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down,                                 () => MoveWindow(Direction.Down, MoveType.Normal)));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left,                    () => MoveWindow(Direction.Left, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right,                   () => MoveWindow(Direction.Right, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up,                      () => MoveWindow(Direction.Up, MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down,                    () => MoveWindow(Direction.Down, MoveType.Expand)));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Left,     () => MoveWindow(Direction.Left, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Right,    () => MoveWindow(Direction.Right, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Up,       () => MoveWindow(Direction.Up, MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Down,     () => MoveWindow(Direction.Down, MoveType.Contract)));
        }

        private List<KeyboardHook> Hooks = new List<KeyboardHook>();
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        private ConfigurationManager Configuration;
        private System.Windows.Window ConfigWindow;
        private Dictionary<IntPtr, AnimationArgs> CurrentAnimations = new Dictionary<IntPtr, AnimationArgs>();
        private Task AnimationTask;

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
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan AnimationInterval = TimeSpan.FromSeconds(1 / 120d);

        #region Moving

        public void MoveWindow(Direction direction, MoveType moveType)
        {
            Task.Run(() =>
            {
                var (ptr, start, end) = GetAnimationArgs(direction, moveType);
                
                lock (CurrentAnimations)
                {
                    if(CurrentAnimations.ContainsKey(ptr))
                    {
                        (ptr, start, end) = GetAnimationArgs(direction, moveType, ptr, CurrentAnimations[ptr].end);
                        var args = new AnimationArgs()
                        {
                            ptr = ptr,
                            t = 0,
                            start = CurrentAnimations[ptr].current,
                            end = end,
                        };
                        CurrentAnimations[ptr] = args;
                    }
                    else
                    {
                        var args = new AnimationArgs()
                        {
                            ptr = ptr,
                            t = 0,
                            start = start,
                            end = end,
                        };
                        CurrentAnimations.Add(ptr, args);
                    }
                }
                Animate();
            });
        }

        private void Animate()
        {
            lock(this)
            {
                if(AnimationTask == null)
                {
                    AnimationTask = Task.Run(async () =>
                    {
                        HashSet<IntPtr> remove = new HashSet<IntPtr>();
                        DateTime lastLoopTime = DateTime.Now - AnimationInterval;
                        while(true)
                        {
                            remove.Clear();

                            DateTime startTime = DateTime.Now;

                            lock(CurrentAnimations)
                            {
                                foreach(var pair in CurrentAnimations)
                                {
                                    var args = pair.Value;
                                    var dt = (startTime - lastLoopTime).TotalMilliseconds / AnimationDuration.TotalMilliseconds;
                                    args.t = Easing.Clamp01(pair.Value.t + (float)dt);
                                    var ts = Easing.SmoothEnd(args.t, 2, Easing.BoundType.Clamp);

                                    var x = Easing.Lerp(args.start.Left, args.end.Left, ts);
                                    var y = Easing.Lerp(args.start.Top, args.end.Top, ts);
                                    var r = Easing.Lerp(args.start.Right, args.end.Right, ts);
                                    var b = Easing.Lerp(args.start.Bottom, args.end.Bottom, ts);
                                    args.current = new Rect(x, y, r, b);

                                    PInvoke.SetWindowRect(args.ptr, args.current);
                                    if(args.t >= 1)
                                    {
                                        remove.Add(args.ptr);
                                    }
                                }

                                foreach(var ptr in remove)
                                {
                                    CurrentAnimations.Remove(ptr);
                                }

                                if (CurrentAnimations.Count == 0) break;
                            }

                            lastLoopTime = DateTime.Now;
                            var waitDuration = DateTime.Now + AnimationInterval - startTime;
                            if (waitDuration > TimeSpan.Zero)
                            {
                                await Task.Delay(waitDuration);
                            }
                            else
                            {
                                Console.WriteLine("ZeroWait");
                            }
                        }
                    }).ContinueWith(t =>
                    {
                        lock(this)
                        {
                            AnimationTask = null;
                        }
                    });
                }
            }
        }

        private (IntPtr handle, Rect current, Rect target) GetAnimationArgs(Direction direction, MoveType moveType)
        {
            var handleForeground = PInvoke.GetForegroundWindow();

            var start = new Rect();
            var success = PInvoke.GetWindowRect(handleForeground, ref start);

            if (!success) return (IntPtr.Zero, new Rect(), new Rect());

            return GetAnimationArgs(direction, moveType, handleForeground, start);
        }
        private (IntPtr handle, Rect current, Rect target) GetAnimationArgs(Direction direction, MoveType moveType, IntPtr handleForeground, Rect start)
        {
            var end = start;

            switch (moveType)
            {
                case MoveType.Normal:
                    end = TranslateWindow(start, direction);
                    break;
                case MoveType.Expand:
                    end = ExpandWindow(start, direction);
                    break;
                case MoveType.Contract:
                    end = ContractWindow(start, direction);
                    break;
            }

            return (handleForeground, start, end);
        }

        private Rect TranslateWindow(Rect rect, Direction direction)
        {

            if (!IsAligned(rect))
            {
                // If we're not ON grid, move to the enclosing sector,
                // and fill the screen in the non-given axis.
                var screen = Screen.FromPoint(rect.Center);
                var sector = GetSector(rect.Center);

                if (direction == Direction.Down || direction == Direction.Up)
                {
                    rect.Left = screen.WorkingArea.Left;
                    rect.Right = screen.WorkingArea.Right;
                    rect.Top = sector.Top;
                    rect.Bottom = sector.Bottom;
                }
                else
                {
                    rect.Top = screen.WorkingArea.Top;
                    rect.Bottom = screen.WorkingArea.Bottom;
                    rect.Left = sector.Left;
                    rect.Right = sector.Right;
                }
            }
            else
            {
                var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height), false);
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
            }

            return rect;
        }

        private Rect ExpandWindow(Rect rect, Direction direction)
        {
            var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height), false);

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

            return rect;
        }

        private Rect ContractWindow(Rect rect, Direction direction)
        {
            var newSector = GetSector(direction, new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height), true);
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

            return rect;
        }

        #endregion

        #region Queries

        public bool IsAligned(Rect rect)
        {
            var screen = Screen.FromPoint(new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2));
            var config = Configuration.Configs[screen.Bounds];
            rect.Left -= screen.WorkingArea.Left;
            rect.Right -= screen.WorkingArea.Left;
            rect.Top -= screen.WorkingArea.Top;
            rect.Bottom -= screen.WorkingArea.Top;

            const int tolerance = 2;
            int w = screen.WorkingArea.Width / config.WidthDivisions;
            int h = screen.WorkingArea.Height / config.HeightDivisions;
            return rect.Left % w < tolerance &&
                   rect.Right % w < tolerance &&
                   rect.Top % h < tolerance &&
                   rect.Bottom % h < tolerance;
        }

        /// <summary>
        /// Get the sector encapsulating a given point.
        /// </summary>
        public Rectangle GetSector(Point fromPoint)
        {
            var screen = Screen.FromPoint(fromPoint);
            var config = Configuration.Configs[screen.Bounds];
            double x, y, w, h;

            w = screen.WorkingArea.Width / (double)config.WidthDivisions;
            h = screen.WorkingArea.Height / (double)config.HeightDivisions;

            x = Math.Floor((fromPoint.X - screen.WorkingArea.Left) / (double)screen.WorkingArea.Width * config.WidthDivisions) * w + screen.WorkingArea.Left;
            y = Math.Floor((fromPoint.Y - screen.WorkingArea.Top) / (double)screen.WorkingArea.Height * config.HeightDivisions) * h + screen.WorkingArea.Top;

            return new Rectangle((int)x, (int)y, (int)Math.Ceiling(w), (int)Math.Ceiling(h));
        }

        /// <summary>
        /// Get the sector adjacent to a given (potentially misaligned) zone.
        /// </summary>
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

        #endregion

        #region Config Independent
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

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private static Point GetEdge(Rectangle rect, Direction direction, bool opposite)
        {
            Point point;

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

        private class AnimationArgs
        {
            public IntPtr ptr;
            public float t;
            public Rect start;
            public Rect current;
            public Rect end;
        }

        #endregion
    }
}
