using System;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleBackup.Core.Configuration.Types
{
    [XmlRoot("Configuration", IsNullable = true)]
    public class AppConfig
    {
        public bool ShowHelp = true;
        public int DefaultConfigI = 0;
        public BackupConfig[] BackupConfigs = { new BackupConfig() };
        public string[] ExcludedFilenames = { @".DS_Store", @"^[Tt]humbs.db$" };
    }
    public class BackupConfig
    {
        [XmlAttribute]
        public string Name = "default";
        public string DestinationPath = "";
        public string[] IncludedPaths = { };
        public string[] ExcludedPaths = { };
        public int VersionsToKeep = 2;
        public Constants.BackupType BackupType = Constants.BackupType.FOLDER;
        public DateTime LastBackup;
    }
}
