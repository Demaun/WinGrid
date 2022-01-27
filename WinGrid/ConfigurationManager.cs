using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IEnumerable<KeyValuePair<Rectangle, WinGridConfig>> CurrentConfigs 
        { 
            get
            {
                return Configs.Where(c => c.Value.Visibility == Visibility.Visible);
            } 
        }

        public ConfigurationManager()
        {
            Deserialize();
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
                var path = Path.GetFullPath(ConfigPath);
                Configs = JsonConvert.DeserializeObject<Dictionary<Rectangle, WinGridConfig>>(File.ReadAllText(path));
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
