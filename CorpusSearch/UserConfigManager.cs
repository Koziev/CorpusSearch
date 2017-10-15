using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorpusSearch
{
    public class UserConfigManager
    {
        private Dictionary<string,object> config_properties;

        public UserConfigManager()
        {}

        private bool allow_saving = false;

        public bool AllowSaving { get { return allow_saving; } set { allow_saving = value; } }

        public void Store()
        {
            if (AllowSaving)
            {
                StoreConfig();
            }
        }

        public object this[string property_name]
        {
            get
            {
                if(config_properties==null)
                {
                    LoadConfig();
                }

                if(config_properties.ContainsKey(property_name))
                {
                    return config_properties[property_name];
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if( allow_saving)
                {
                    if (config_properties == null)
                    {
                        LoadConfig();
                    }

                    config_properties[property_name] = value;
                }
            }
        }

        private string GetConfigFilePath()
        {
            const string CONFIG_FILENAME = "CorpusSearchUserConfig.json";
            return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CONFIG_FILENAME);
        }

        private void StoreConfig()
        {
            Contract.Requires(AllowSaving);

            string config_path = GetConfigFilePath();

            log4net.ILog log = log4net.LogManager.GetLogger(typeof(UserConfigManager));
            log.InfoFormat("Writing config to {0}", config_path);

            try
            {
                using (System.IO.StreamWriter wrt = new System.IO.StreamWriter(config_path))
                {
                    string cfg_json = Newtonsoft.Json.JsonConvert.SerializeObject(config_properties);
                    wrt.Write(cfg_json);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error when writing to config file {0}: {1}", config_path, ex.Message);
                throw;
            }

            return;
        }

        public void LoadConfig()
        {
            string config_path = GetConfigFilePath();
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(UserConfigManager));
            log.InfoFormat("Reading config from {0}", config_path);

            if (System.IO.File.Exists(config_path))
            {
                try
                {
                    using (System.IO.StreamReader rdr = new System.IO.StreamReader(config_path))
                    {
                        string cfg_json = rdr.ReadToEnd();
                        config_properties = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>(cfg_json);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error when reading from config file {0}: {1}", config_path, ex.Message);
                    throw;
                }
            }

            return;
        }

    }
}
