using SimpleBackup.Core.Configuration.Types;
using System.IO;
using System.Xml.Serialization;

namespace SimpleBackup.Core.Configuration
{
    public static class Helpers
    {
        public static void Write(string filename, AppConfig appconfig)
        {
            XmlSerializer serializer = new(typeof(AppConfig));
            using var stream = File.Open(filename, FileMode.Create);
            serializer.Serialize(stream, appconfig);
        }
        public static AppConfig Read(string filename)
        {
            XmlSerializer serializer = new(typeof(AppConfig));
            // TODO add UnknownNode and UnknownAttribute handlers
            using var stream = File.OpenRead(filename);
            return (AppConfig)serializer.Deserialize(stream);
        }
        public static void WriteDefaults(string filename)
        {
            AppConfig appConfig = new();
            Write(filename, appConfig);
        }
    }
}
