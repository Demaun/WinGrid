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

        public WinGrid()
        {
            Configuration = new ConfigurationManager();
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, Close));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left, () => MoveWindow(Direction.Left, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right, () => MoveWindow(Direction.Right, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up, () => MoveWindow(Direction.Up, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down, () => MoveWindow(Direction.Down, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left, () => MoveWindow(Direction.Left, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direction.Right, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up, () => MoveWindow(Direction.Up, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down, () => MoveWindow(Direction.Down, true)));
        }

        public void MoveWindow(Direction direction, bool expand)
        {
            var handleForeground = PInvoke.GetForegroundWindow();

            var rect = new PInvoke.RECT();
            var success = PInvoke.GetWindowRect(handleForeground, ref rect);
            int w = rect.Right - rect.Left;
            int h = rect.Bottom - rect.Top;
            if(success)
            {
                var newSector = Configuration.GetSector(direction, new System.Drawing.Rectangle(rect.Left, rect.Top, w, h));

                if(direction == Direction.Up)
                {
                    rect.Top = newSector.Top;
                    if(!expand)
                        rect.Bottom = newSector.Bottom;
                }
                else if(direction == Direction.Down)
                {
                    rect.Bottom = newSector.Bottom;
                    if (!expand)
                        rect.Top = newSector.Top;
                }
                else if (direction == Direction.Left)
                {
                    rect.Left = newSector.Left;
                    if (!expand)
                        rect.Right = newSector.Right;
                }
                else //if (direction == Direction.Right)
                {
                    rect.Right = newSector.Right;
                    if (!expand)
                        rect.Left = newSector.Left;
                }
                PInvoke.SetWindowRect(handleForeground, rect);
            }
        }

        private void Close()
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
