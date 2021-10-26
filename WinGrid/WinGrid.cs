using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace WinGridApp
{
    public class WinGrid
    {
        private List<KeyboardHook> Hooks = new List<KeyboardHook>();
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;
        private ConfigurationManager Configuration;
        private Window ConfigWindow;

        public WinGrid()
        {
            Configuration = new ConfigurationManager();
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, ()=>
            {
                lock(this)
                {
                    if(ConfigWindow != null)
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

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left,                              () => MoveWindow(Direction.Left,   MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right,                             () => MoveWindow(Direction.Right,  MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up,                                () => MoveWindow(Direction.Up,     MoveType.Normal)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down,                              () => MoveWindow(Direction.Down,   MoveType.Normal)));
            
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left,                 () => MoveWindow(Direction.Left,   MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right,                () => MoveWindow(Direction.Right,  MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up,                   () => MoveWindow(Direction.Up,     MoveType.Expand)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down,                 () => MoveWindow(Direction.Down,   MoveType.Expand)));

            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Left,  () => MoveWindow(Direction.Left,   MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direction.Right,  MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Up,    () => MoveWindow(Direction.Up,     MoveType.Contract)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Shift | KeysEx.Alt | KeysEx.Down,  () => MoveWindow(Direction.Down,   MoveType.Contract)));
        }

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
            var newSector = Configuration.GetSector(direction, new System.Drawing.Rectangle(rect.Left, rect.Top, rect.W, rect.H), false);

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
            var newSector = Configuration.GetSector(direction, new System.Drawing.Rectangle(rect.Left, rect.Top, rect.W, rect.H), false);

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
            var newSector = Configuration.GetSector(direction, new System.Drawing.Rectangle(rect.Left, rect.Top, rect.W, rect.H), true);
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
