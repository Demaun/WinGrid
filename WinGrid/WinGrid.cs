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

namespace WinGrid
{
    public class WinGrid
    {
        private List<KeyboardHook> Hooks = new List<KeyboardHook>();
        private Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

        public WinGrid()
        {
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, Close));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left, () => MoveWindow(Direciton.Left, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right, () => MoveWindow(Direciton.Right, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up, () => MoveWindow(Direciton.Up, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down, () => MoveWindow(Direciton.Down, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left, () => MoveWindow(Direciton.Left, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direciton.Right, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up, () => MoveWindow(Direciton.Up, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down, () => MoveWindow(Direciton.Down, true)));
        }

        public string GetScreenID(Screen screen)
        {
            var input = $"{screen.DeviceName}{screen.WorkingArea.X}{screen.WorkingArea.Y}{screen.WorkingArea.Width}{screen.WorkingArea.Height}";
            using (var hasher = MD5.Create())
            {
                var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Encoding.ASCII.GetString(bytes);
            }
        }

        private PInvoke.RECT Rect = new PInvoke.RECT()
        {
            Left = 0,
            Right = 1080 / 3,
            Top = 0,
            Bottom = 1200,
        };

        public void MoveWindow(Direciton direction, bool expand)
        {
            var handleForeground = PInvoke.GetForegroundWindow();

            var rect = new PInvoke.RECT();
            var success = PInvoke.GetWindowRect(handleForeground, ref rect);
            Console.WriteLine($"Current position = {rect.Left},{rect.Top},{rect.Right},{rect.Bottom}");
            Rect.Left = Rect.Left >= 1920 * 2 / 3 ? 0 : (Rect.Left + 1920 / 3);
            Rect.Right = (Rect.Left + 1920 / 3);
            PInvoke.SetWindowRect(handleForeground, Rect);
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
