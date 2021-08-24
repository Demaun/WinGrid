using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinGrid
{
    public class ConfigurationManager
    {
        private static readonly string ConfigPath = "config.json";
        private WinGridConfig Config;
        public ConfigurationManager()
        {
            Deserialize();
        }

        private void Validate()
        {
        }

        public void Deserialize()
        {
            try
            {
                Config = JsonConvert.DeserializeObject<WinGridConfig>(File.ReadAllText(ConfigPath));
            }
            catch
            {
                Config = new WinGridConfig();
            }
            Validate();
            Serialize();
        }

        public void Serialize()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config));
        }
    }
}
