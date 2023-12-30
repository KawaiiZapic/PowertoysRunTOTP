using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Wox.Infrastructure.Storage;

namespace Community.PowerToys.Run.Plugin.TOTP {
    public class OTPList {
        public class KeyEntry {
            public string Name { get; set; }
            public string Key { get; set; }
            public bool IsEncrypted { get; set; }

        }

        public int Version { get; set; }
        public List<KeyEntry> Entries { get; set; }
    }


    public static class Config {

        public static List<OTPList.KeyEntry> LoadKeyList() {
            var nc = new PluginJsonStorage<OTPList>();
            var config = nc.Load();
            if (config.Entries == null) {
                config.Version = 2;
                config.Entries = new List<OTPList.KeyEntry>();
                nc.Save();
            }
            return config.Entries;
        }

        public static void SaveKeyList(List<OTPList.KeyEntry> list) {
            var nc = new PluginJsonStorage<OTPList>();
            var config = nc.Load();
            foreach (var entry in list) {
                if (entry.IsEncrypted != true) {
                    entry.Key = EncryptKey(entry.Key);
                    entry.IsEncrypted = true;
                }
            }
            config.Entries = list;
            nc.Save();
        }

        public static string DecryptKey(string encrypted) {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser));
        }

        public static string EncryptKey(string unencrypted) {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(unencrypted), null, DataProtectionScope.CurrentUser));
        }
    }


    public static class ConfigMigratorV0 {
        private static readonly string DataDirectoryV0 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\";
        private static readonly string ConfigPathV0 = DataDirectoryV0 + "TOTPList.json";
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        public class StructV0 {
            public string Name { get; set; }
            public string Key { get; set; }

        }

        public class StructV1 {
            public class KeyEntry {
                public string Name { get; set; }
                public string Key { get; set; }
                public bool IsEncrypted { get; set; }

            }
            public int Version { get; set; }
            public List<KeyEntry> Entries { get; set; }

        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV0).Exists)
                return;
            var FileV0 = File.Open(ConfigPathV0, FileMode.Open);
            var ConfigV0 = JsonSerializer.Deserialize<List<StructV0>>(FileV0) ?? throw new Exception("Config should not be null");
            var EntriesV1 = new List<StructV1.KeyEntry>();
            ConfigV0.ForEach(entry => {
                EntriesV1.Add(new StructV1.KeyEntry { Name = entry.Name, Key = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(entry.Key), null, DataProtectionScope.CurrentUser)), IsEncrypted = true });
            });
            var ConfigV1 = new StructV1 {
                Version = 1,
                Entries = EntriesV1
            };
            if (!new FileInfo(DataDirectoryV1).Exists) {
                Directory.CreateDirectory(DataDirectoryV1);
            }
            var FileV1 = File.OpenWrite(ConfigPathV1);
            JsonSerializer.Serialize(FileV1, ConfigV1, new JsonSerializerOptions { WriteIndented = true });

            FileV0.Dispose();
            File.Delete(ConfigPathV0);
            Directory.Delete(DataDirectoryV0);
        }

    }
    public static class ConfigMigratorV1 {
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        public class OTPList {
            public class KeyEntry {
                public string Name { get; set; }
                public string Key { get; set; }
                public bool IsEncrypted { get; set; }

            }

            public int Version { get; set; }
            public List<KeyEntry> Entries { get; set; }
        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV1).Exists)
                return;

            var FileV1 = File.Open(ConfigPathV1, FileMode.Open);
            var ConfigV1 = JsonSerializer.Deserialize<OTPList>(FileV1) ?? throw new Exception("Config should not be null");

            var ConfigV2 = new PluginJsonStorage<OTPList>();
            var config = ConfigV2.Load();
            config.Version = 2;
            config.Entries = ConfigV1.Entries;
            ConfigV2.Save();

            FileV1.Close();
            File.Delete(ConfigPathV1);
            Directory.Delete(DataDirectoryV1);
        }

    }
}
