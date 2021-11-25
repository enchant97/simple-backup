using System.IO;
using SimpleBackup.Core.Configuration.Types;

// TODO add events for when config is changed in-memory
namespace SimpleBackup.Core.Configuration
{
    /// <summary>
    /// Easy access to use a shared config across program
    /// </summary>
    public static class QuickConfig
    {
        /// <summary>
        /// The currently loaded config data
        /// </summary>
        public static Types.AppConfig AppConfig { get; private set; }
        /// <summary>
        /// Read config from file (creating one if missing)
        /// </summary>
        public static void Read()
        {
            if (!File.Exists(Constants.ConfigFullPath))
            {
                Directory.CreateDirectory(Constants.UserHomePath);
                Helpers.WriteDefaults(Constants.ConfigFullPath);
            }
            Read(Constants.ConfigFullPath);
        }
        /// <summary>
        /// Read config from a specific file
        /// </summary>
        /// <param name="filename">the filename to read from</param>
        public static void Read(string filename)
        {
            AppConfig = Helpers.Read(filename);
            if (AppConfig.BackupConfigs.Count == 0)
            {
                AppConfig.BackupConfigs.Add(new BackupConfig());
            }
        }
        /// <summary>
        /// Write currently loaded config data
        /// </summary>
        public static void Write()
        {
            Directory.CreateDirectory(Constants.UserHomePath);
            Write(Constants.ConfigFullPath);
        }
        /// <summary>
        /// Write config to a specific file
        /// </summary>
        /// <param name="filename">the filename to save as</param>
        public static void Write(string filename)
        {
            Helpers.Write(filename, AppConfig);
        }
        /// <summary>
        /// Reset the config to defaults
        /// </summary>
        public static void Reset()
        {
            Directory.CreateDirectory(Constants.UserHomePath);
            Helpers.WriteDefaults(Constants.ConfigFullPath);
            Read();
        }
    }
}
