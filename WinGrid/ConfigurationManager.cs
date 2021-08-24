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
        private Dictionary<Rectangle, WinGridConfig> Configs;

        public Rectangle Bounds = new Rectangle();
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

        public Rectangle GetSector(Direction direction, Rectangle fromSector)
        {
            Point point = GetEdge(fromSector, direction);


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

        public int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        private Point GetEdge(Rectangle rect, Direction direction)
        {
            Point point = new Point();

            if (direction == Direction.Up)
            {
                point = new Point(rect.Left + rect.Width / 2, rect.Top);
            }
            else if (direction == Direction.Down)
            {
                point = new Point(rect.Left + rect.Width / 2, rect.Bottom);
            }
            else if (direction == Direction.Left)
            {
                point = new Point(rect.Left, rect.Top + rect.Height / 2);
            }
            else
            {
                point = new Point(rect.Right, rect.Top + rect.Height / 2);
            }
            return point;
        }

        private void Validate()
        {
            foreach(var screen in Screen.AllScreens)
            {
                if(!Configs.ContainsKey(screen.Bounds))
                {
                    Configs.Add(screen.Bounds, new WinGridConfig(1, 1));
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
