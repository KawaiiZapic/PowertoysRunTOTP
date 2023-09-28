using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace PowertoysRunTOTP
{
    public class ConfigStruct
    {
        public class KeyEntry
        {
            public string Name { get; set; }
            public string Key { get; set; }
            public bool IsEncrypted { get; set; }

        }

        public int Version { get; set; }
        public List<KeyEntry> Entries { get; set; }
    }


    public static class Config {
        private static readonly string DataDirectory = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPath = DataDirectory + "Config.json";
        private static readonly int Version = 1;

        public static ConfigStruct LoadConfig() {
            var fileInfo = new FileInfo(ConfigPath);
            if (!fileInfo.Exists)
            {
                if (!fileInfo.Directory!.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory!.FullName);
                }
                var newFile = File.Create(ConfigPath);
                var options = new JsonSerializerOptions { WriteIndented = true };
                var EmptyConfig = new ConfigStruct
                {
                    Entries = new List<ConfigStruct.KeyEntry>(),
                    Version = Version,
                };
                JsonSerializer.Serialize(newFile, EmptyConfig, options);
                newFile.Dispose();
            }
            var file = File.OpenRead(ConfigPath);
            var result = JsonSerializer.Deserialize<ConfigStruct>(file);
            if (result == null)
            {
                throw new Exception("Failed to load config: Result is null");
            }
            file.Dispose();
            return result;
        }

        public static List<ConfigStruct.KeyEntry> LoadKeyList()
        {
            return LoadConfig().Entries;
        }

        public static void SaveConfig(ConfigStruct config)
        {
            var fileInfo = new FileInfo(ConfigPath);
            if (!fileInfo.Exists)
            {
                if (!fileInfo.Directory!.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory!.FullName);
                }
            }
            var file = File.Open(ConfigPath, FileMode.Create);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(file, config, options);
        }

        public static void SaveKeyList(List<ConfigStruct.KeyEntry> list)
        {
            var config = LoadConfig();
            foreach ( var entry in list )
            {
                if (entry.IsEncrypted != true) {
                    entry.Key = Config.EncryptKey(entry.Key);
                    entry.IsEncrypted = true;
                }
            }
            config.Entries = list;
            SaveConfig(config);
        }

        public static string DecryptKey(string encrypted) {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser));
        }

        public static string EncryptKey(string unencrypted)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(unencrypted), null, DataProtectionScope.CurrentUser));
        }
    }


    public static class ConfigMigratorV0 {
        private static readonly string DataDirectoryV0 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\";
        private static readonly string ConfigPathV0 = DataDirectoryV0 + "TOTPList.json";
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        public class StructV0
        {
            public String Name { get; set; }
            public String Key { get; set; }

        }

        public class StructV1
        {
            public class KeyEntry
            {
                public string Name { get; set; }
                public string Key { get; set; }
                public bool IsEncrypted { get; set; }

            }
            public int Version { get; set; }
            public List<KeyEntry> Entries { get; set; }
            
        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV0).Exists) return;
            var FileV0 = File.Open(ConfigPathV0, FileMode.Open);
            var ConfigV0 = JsonSerializer.Deserialize<List<StructV0>>(FileV0) ?? throw new Exception("Config should not be null");
            var EntriesV1 = new List<StructV1.KeyEntry>();
            ConfigV0.ForEach(entry => { 
                EntriesV1.Add(new StructV1.KeyEntry { Name = entry.Name, Key = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(entry.Key), null, DataProtectionScope.CurrentUser)), IsEncrypted = true });
            });
            var ConfigV1 = new StructV1
            {
                Version = 1,
                Entries = EntriesV1
            };
            if (!new FileInfo(DataDirectoryV1).Exists)
            {
                Directory.CreateDirectory(DataDirectoryV1);
            }
            var FileV1 = File.OpenWrite(ConfigPathV1);
            JsonSerializer.Serialize(FileV1, ConfigV1, new JsonSerializerOptions { WriteIndented = true });

            FileV0.Dispose();
            File.Delete(ConfigPathV0);
            Directory.Delete(DataDirectoryV0);
        }

    }
}
