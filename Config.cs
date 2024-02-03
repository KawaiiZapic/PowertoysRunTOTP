using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Wox.Infrastructure.Storage;

namespace Community.PowerToys.Run.Plugin.TOTP {
    public class OTPList {
        public class KeyEntry {
            public string Name = "";
            public string Key = "";
            public bool IsEncrypted = false;

        }

        public int Version = 2;
        public List<KeyEntry> Entries = new();
    }

    public static class ConfigMigratorV0 {
        private static readonly string DataDirectoryV0 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\";
        private static readonly string ConfigPathV0 = DataDirectoryV0 + "TOTPList.json";
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        public class StructV0 {
            public string Name = "";
            public string Key = "";
        }

        public class StructV1 {
            public class KeyEntry {
                    public string Name = "";
                    public string Key = "";
                    public bool IsEncrypted = false;

            }
                public int Version = 1;
                public List<KeyEntry> Entries = new();

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
                public string Name = "";
                public string Key = "";
                public bool IsEncrypted = false;

            }
            public int Version;
            public List<KeyEntry> Entries = new();
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
