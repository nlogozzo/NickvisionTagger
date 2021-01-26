/****
* "Config.cs" - The Config class
* Copyright (C) 2021 Nicholas Logozzo
*
* This program is free software; you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation; either version 2 of the License, or
* (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, see <http://www.gnu.org/licenses/>.
****/

using System;
using System.IO;
using System.Text.Json;

namespace NickvisionTagger.Models
{
    /// <summary>
    /// The Config class
    /// </summary>
    public class Config
    {
        private static string _configDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\Nickvision\\NickvisionTagger";
        private static string _configPath = $"{_configDir}\\config.json";

        public bool IsLightTheme { get; set; }
        public string PreviousMusicFolderPath { get; set; }
        public bool IncludeSubfolders { get; set; }

        /// <summary>
        /// Constructs an empty Config object
        /// </summary>
        public Config()
        {
            IsLightTheme = true;
            PreviousMusicFolderPath = "";
            IncludeSubfolders = true;
        }

        /// <summary>
        /// Constructs a Config object
        /// </summary>
        /// <param name="isLightTheme">Whether the application is in light theme or not</param>
        /// <param name="previousMusicFolderPath">The previously opened music folder path</param>
        /// <param name="includeSubfolders">Include Subfolders</param>
        public Config(bool isLightTheme, string previousMusicFolderPath, bool includeSubfolders)
        {
            IsLightTheme = isLightTheme;
            PreviousMusicFolderPath = previousMusicFolderPath;
            IncludeSubfolders = includeSubfolders;
        }

        /// <summary>
        /// Reads the config file and constructs a Config file
        /// </summary>
        /// <returns>A constructed Config object from the saved config file. Default config if no config file found</returns>
        public static Config LoadConfig()
        {
            try
            {
                return JsonSerializer.Deserialize<Config>(File.ReadAllText(_configPath));
            }
            catch
            {
                return new Config();
            }
        }

        /// <summary>
        /// Saves a Config object into the config file
        /// </summary>
        /// <param name="config">The Config object with the saved perferences</param>
        public static void SaveConfig(Config config)
        {
            var json = JsonSerializer.Serialize(config);
            if (!Directory.Exists(_configDir))
            {
                Directory.CreateDirectory(_configDir);
            }
            File.WriteAllText(_configPath, json);
        }
    }
}
