using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Wox.Infrastructure.Storage;

namespace Community.PowerToys.Run.Plugin.TOTP {

    public class Authenticator {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public bool IsEncrypted { get; set; }
    }

    public class AuthenticatorsList {
        public int Version { get; set; }
        public List<Authenticator> Authenticators { get; set; } = new();
    }

    public static class ConfigMigratorV0 {
        private static readonly string DataDirectoryV0 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\";
        private static readonly string ConfigPathV0 = DataDirectoryV0 + "TOTPList.json";
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        class StructV0 {
            public string Name { get; set; } = "";
            public string Key { get; set; } = "";
        }

        class KeyEntry {
            public string Name { get; set; } = "";
            public string Key { get; set; } = "";
            public bool IsEncrypted { get; set; }
        }

        class StructV1 {
            public int Version { get; set; }
            public List<KeyEntry> Entries { get; set; } = new();

        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV0).Exists) return;

            var FileV0 = File.Open(ConfigPathV0, FileMode.Open);
            var ConfigV0 = JsonSerializer.Deserialize<List<StructV0>>(FileV0) ?? throw new Exception("Config should not be null");

            var EntriesV1 = new List<KeyEntry>();
            ConfigV0.ForEach(entry => {
                EntriesV1.Add(new() { 
                    Name = entry.Name, 
                    Key = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(entry.Key), null, DataProtectionScope.CurrentUser)),
                    IsEncrypted = true 
                });
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
            
            FileV1.Close();
            FileV0.Close();
            File.Delete(ConfigPathV0);
            Directory.Delete(DataDirectoryV0);
        }

    }
    public static class ConfigMigratorV1 {
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";


        class KeyEntry {
            public string Name { get; set; } = "";
            public string Key { get; set; } = "";
            public bool IsEncrypted { get; set; }
        }

        class OTPList {
            public int Version { get; set; }
            public List<KeyEntry> Entries { get; set; } = new();
        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV1).Exists) return;

            var FileV1 = File.Open(ConfigPathV1, FileMode.Open);
            var ConfigV1 = JsonSerializer.Deserialize<OTPList>(FileV1) ?? throw new Exception("Config should not be null");
            FileV1.Close();

            if (ConfigV1.Version != 1) return;

            var ConfigV2 = new PluginJsonStorage<OTPList>();
            new StoragePowerToysVersionInfo(ConfigV2.FilePath, 1).Close();
            var config = ConfigV2.Load();
            config.Version = 2;
            config.Entries = ConfigV1.Entries;
            ConfigV2.Save();

            File.Delete(ConfigPathV1);
            Directory.Delete(DataDirectoryV1);
        }
    }

    public static class ConfigMigratorV2 {
        private static readonly string DataDirectoryV2 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Community.PowerToys.Run.Plugin.TOTP\\";
        private static readonly string ConfigPathV2 = DataDirectoryV2 + "OTPList.json";
        private static readonly string ConfigVersionPathV2 = DataDirectoryV2 + "OTPList_version.txt";

        class Authenticator {
            public string Name { get; set; } = "";
            public string Key { get; set; } = "";
            public bool IsEncrypted { get; set; }
        }

        class AuthenticatorsList {
            public int Version { get; set; }
            public List<Authenticator> Authenticators { get; set; } = new();
        }

        class OTPList {
            public int Version { get; set; }
            public List<Authenticator> Entries { get; set; } = new();
        }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV2).Exists) return;
            
            var FileV2 = File.Open(ConfigPathV2, FileMode.Open);
            var ConfigV2 = JsonSerializer.Deserialize<OTPList>(FileV2) ?? throw new Exception("Config should not be null");
            FileV2.Close();

            if (ConfigV2.Version != 2) return;

            var ConfigV3 = new PluginJsonStorage<AuthenticatorsList>();
            new StoragePowerToysVersionInfo(ConfigV3.FilePath, 1).Close();
            var config = ConfigV3.Load();
            config.Version = 3;
            config.Authenticators = ConfigV2.Entries;
            ConfigV3.Save();

            File.Delete(ConfigPathV2);
            File.Delete(ConfigVersionPathV2);
        }

    }
}
