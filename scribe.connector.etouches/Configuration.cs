﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scribe.Connector.etouches
{
    public static class Configuration
    {
        static System.Configuration.Configuration config  = null;

        private static System.Configuration.Configuration getConfig()
        {
            if (config != null)
                return config;

            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Scribe.Connector.etouches.dll.config";

            config =
                  ConfigurationManager.OpenMappedExeConfiguration(
                    configFileMap, ConfigurationUserLevel.None);
            return config;

        }


        public static string RestURL
        {
            get
            {
                return getConfig().AppSettings.Settings["RestURL"].Value;
            }
        }

        public static string V2URL
        {
            get
            {
                return getConfig().AppSettings.Settings["V2URL"].Value;
            }
        }
    }
}
