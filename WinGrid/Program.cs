using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace WinGrid
{
    class Program
    {
        private static Form Form;
        private static List<KeyboardHook> Hooks = new List<KeyboardHook>();
        private static Dispatcher Dispatcher = Dispatcher.CurrentDispatcher;

        static void Main(string[] args)
        {
            Form = new Form();
            Form.FormBorderStyle = FormBorderStyle.None;
            Form.ShowInTaskbar = false;
            Form.Load += (s, e) => Form.Size = new Size(0, 0);
            
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Escape, Close));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Left,  () => MoveWindow(Direciton.Left,  false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Right, () => MoveWindow(Direciton.Right, false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Up,    () => MoveWindow(Direciton.Up,    false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Down,  () => MoveWindow(Direciton.Down,  false)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Left,  () => MoveWindow(Direciton.Left,  true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Right, () => MoveWindow(Direciton.Right, true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Up,    () => MoveWindow(Direciton.Up,    true)));
            Hooks.Add(GetHook(KeysEx.WinLogo | KeysEx.Alt | KeysEx.Down,  () => MoveWindow(Direciton.Down,  true)));

            Application.Run(Form);
        }

        public static void GetCurrentDisplayInformation()
        {

        }

        public static void GetNextDisplayInformation(Direciton direciton)
        {

        }

        private static PInvoke.RECT Rect = new PInvoke.RECT()
        {
            Left = 0,
            Right = 1080 / 3,
            Top = 0,
            Bottom = 1200,
        };

        public static void MoveWindow(Direciton direction, bool expand)
        {
            var handleForeground = PInvoke.GetForegroundWindow();

            var rect = new PInvoke.RECT();
            var success = PInvoke.GetWindowRect(handleForeground, ref rect);
            Console.WriteLine($"Current position = {rect.Left},{rect.Top},{rect.Right},{rect.Bottom}");
            Rect.Left = Rect.Left >= 1920 * 2 / 3 ? 0 : (Rect.Left + 1920 / 3);
            Rect.Right = (Rect.Left + 1920 / 3);
            PInvoke.SetWindowRect(handleForeground, Rect);
        }

        private static void Close()
        {
            Console.WriteLine("Closing...");

            foreach (var hook in Hooks)
            {
                hook.Disengage();
                hook.Dispose();
            }

            Form.Close();
        }

        private static KeyboardHook GetHook(KeysEx keys, Action action)
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
