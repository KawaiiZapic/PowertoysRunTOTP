using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Zapic.PowerToys.TOTP.Core.Data {

    class KeyEntityV0 {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
    }

    public class KeyEntityV1 {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public bool IsEncrypted { get; set; }
    }

    public class StructV1 {
        public int Version { get; set; }
        public List<KeyEntityV1> Entries { get; set; } = new();

    }

    public class StructV3 {
        public int Version { get; set; }
        public List<KeyEntityV1> Authenticators { get; set; } = new();
    }


    public static class ConfigMigratorV0 {

        public static byte[] MigrateFromFile(string OldFilePath) {
            var FileV0 = File.Open(OldFilePath, FileMode.Open);
            var ConfigV0 = JsonSerializer.Deserialize<List<KeyEntityV0>>(FileV0) ?? throw new Exception("Config should not be null");

            var EntriesV1 = ConfigV0.Select(item =>
                new KeyEntityV1() {
                    Name = item.Name,
                    Key = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(item.Key), null, DataProtectionScope.CurrentUser)),
                    IsEncrypted = true
                }
            ).ToList();
            var ConfigV1 = new StructV1 {
                Version = 1,
                Entries = EntriesV1
            };

            FileV0.Close();
            return JsonSerializer.SerializeToUtf8Bytes(ConfigV1, new JsonSerializerOptions { WriteIndented = true });
        }

    }

    public static class ConfigMigratorV1 {
        // V1 to V2 only change the location where the data saved
        public static byte[] MigrateFromFile(string OldPath) {
            var FileV1 = File.Open(OldPath, FileMode.Open);
            var ConfigV1 = JsonSerializer.Deserialize<StructV1>(FileV1) ?? throw new Exception("Config should not be null");
            FileV1.Close();

            if (ConfigV1.Version != 1)
                throw new Exception("Config version should be 1");
            ConfigV1.Version = 2;

            return JsonSerializer.SerializeToUtf8Bytes(ConfigV1, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public static class ConfigMigratorV2 {
        public static byte[] MigrateFromFile(string OldPath) {
            var FileV2 = File.Open(OldPath, FileMode.Open);
            var ConfigV2 = JsonSerializer.Deserialize<StructV1>(FileV2) ?? throw new Exception("Config should not be null");
            FileV2.Close();

            if (ConfigV2.Version != 2)
                throw new Exception("Config version should be 2");

            var ConfigV3 = new StructV3 {
                Version = 3,
                Authenticators = ConfigV2.Entries
            };

            return JsonSerializer.SerializeToUtf8Bytes(ConfigV3, new JsonSerializerOptions { WriteIndented = true });

        }

    }
}
