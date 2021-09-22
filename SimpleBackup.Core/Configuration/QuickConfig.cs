using System.IO;

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
        public static Types.AppConfig AppConfig;
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
            AppConfig = Helpers.Read(Constants.ConfigFullPath);
        }
        /// <summary>
        /// Write currently loaded config data
        /// </summary>
        public static void Write()
        {
            Directory.CreateDirectory(Constants.UserHomePath);
            Helpers.Write(Constants.ConfigFullPath, AppConfig);
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
