using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SimpleBackup.Core.Configuration.Types
{
    [XmlRoot("Configuration", IsNullable = true)]
    public class AppConfig
    {
        public bool ShowHelp = true;
        public int DefaultConfigI = 0;
        public List<BackupConfig> BackupConfigs;
        public string[] ExcludedFilenames = { @".DS_Store", @"^[Tt]humbs.db$" };
    }
    public class BackupConfig
    {
        [XmlAttribute]
        public string Name = "default";
        public string DestinationPath = "";
        public List<string> IncludedPaths = new();
        public List<string> ExcludedPaths = new();
        public int VersionsToKeep = 2;
        public Constants.BackupType BackupType = Constants.BackupType.FOLDER;
        public DateTime LastBackup;
        public override string ToString()
        {
            return Name;
        }
    }
}
