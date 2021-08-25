using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace WinGridApp
{
    public class ConfigurationManager
    {
        private static readonly string ConfigPath = "config.json";
        public Dictionary<Rectangle, WinGridConfig> Configs { get; private set; }

        public Rectangle Bounds { get; set; } = new Rectangle();
        private const int Interval = 1;
        private static readonly Size Left =  new Size(-Interval, 0);
        private static readonly Size Right = new Size(Interval, 0);
        private static readonly Size Up =    new Size(0, -Interval);
        private static readonly Size Down =  new Size(0, Interval);
        private static readonly Dictionary<Direction, Size> Cardinal = new Dictionary<Direction, Size>
        {
            {Direction.Left, Left },
            {Direction.Right, Right },
            {Direction.Up, Up },
            {Direction.Down, Down }
        };


        public ConfigurationManager()
        {
            Deserialize();
        }

        public Rectangle GetSector(Direction direction, Rectangle fromSector, bool contract)
        {
            Point point = GetEdge(fromSector, direction, contract);

            if(contract)
            {
                var screen = Screen.FromPoint(point);
                var config = Configs[screen.Bounds];
                double x, y, w, h;
                w = screen.WorkingArea.Width / (double)config.WidthDivisions;
                h = screen.WorkingArea.Height / (double)config.HeightDivisions;

                point.X += (int)(Cardinal[direction].Width * (w + Interval));
                point.Y += (int)(Cardinal[direction].Height * (h + Interval));

                screen = Screen.FromPoint(point);
                config = Configs[screen.Bounds];

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

                } while (Bounds.Contains(point) && !screen.WorkingArea.Contains(point) && screen.Bounds.Contains(point));
            
                screen = Screen.FromPoint(point);

                if(!screen.WorkingArea.Contains(point))
                {
                    point = new Point(Clamp(point.X, screen.WorkingArea.Left + Interval, screen.WorkingArea.Right - Interval),
                                      Clamp(point.Y, screen.WorkingArea.Top + Interval, screen.WorkingArea.Bottom - Interval));
                }

                var config = Configs[screen.Bounds];
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

        private void Validate()
        {
            Bounds = new Rectangle();
            foreach(var screen in Screen.AllScreens)
            {
                if(!Configs.ContainsKey(screen.Bounds))
                {
                    var config = new WinGridConfig(1, 1);
                    Configs.Add(screen.Bounds, config);
                    config.Visibility = Visibility.Visible;
                }
                else
                {
                    Configs[screen.Bounds].Visibility = Visibility.Visible;
                }

                Bounds = Rectangle.Union(Bounds, screen.WorkingArea);
            }
        }

        public void Deserialize()
        {
            try
            {
                Configs = JsonConvert.DeserializeObject<Dictionary<Rectangle, WinGridConfig>>(File.ReadAllText(ConfigPath));
            }
            catch
            {
                Configs = new Dictionary<Rectangle, WinGridConfig>();
            }
            Validate();
            Serialize();
        }

        public void Serialize()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Configs, Formatting.Indented));
        }
    }
}
