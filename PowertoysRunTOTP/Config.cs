using Wox.Infrastructure.Storage;
using Core = Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.Run.Plugin.TOTP {
    public static class ConfigMigratorV0 {
        private static readonly string DataDirectoryV0 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\";
        private static readonly string ConfigPathV0 = DataDirectoryV0 + "TOTPList.json";
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV0).Exists)
                return;

            var ConfigV1 = Core.ConfigMigratorV0.MigrateFromFile(ConfigPathV0);

            if (!new FileInfo(DataDirectoryV1).Exists) {
                Directory.CreateDirectory(DataDirectoryV1);
            }
            File.WriteAllBytes(ConfigPathV1, ConfigV1);

            File.Delete(ConfigPathV0);
            Directory.Delete(DataDirectoryV0);
        }

    }
    public static class ConfigMigratorV1 {
        private static readonly string DataDirectoryV1 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\TOTP\\";
        private static readonly string ConfigPathV1 = DataDirectoryV1 + "Config.json";

        // PluginJsonStorage<T> use reflection to determine the name of config file
        // rename namespace or type name will cause file path changed
        class OTPList { }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV1).Exists)
                return;

            var ConfigV2 = Core.ConfigMigratorV1.MigrateFromFile(ConfigPathV1);

            var ConfigV2Storage = new PluginJsonStorage<OTPList>();
            new StoragePowerToysVersionInfo(ConfigV2Storage.FilePath, 1).Close();

            File.WriteAllBytes(ConfigV2Storage.FilePath, ConfigV2);

            File.Delete(ConfigPathV1);
            Directory.Delete(DataDirectoryV1);
        }
    }

    public static class ConfigMigratorV2 {
        private static readonly string DataDirectoryV2 = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Community.PowerToys.Run.Plugin.TOTP\\";
        private static readonly string ConfigPathV2 = DataDirectoryV2 + "OTPList.json";
        private static readonly string ConfigVersionPathV2 = DataDirectoryV2 + "OTPList_version.txt";

        // PluginJsonStorage<T> use reflection to determine the name of config file
        // rename namespace or type name will cause file path changed
        class AuthenticatorsList { }

        public static void Migrate() {
            if (!new FileInfo(ConfigPathV2).Exists)
                return;

            var ConfigV3 = Core.ConfigMigratorV2.MigrateFromFile(ConfigPathV2);

            var ConfigV3Storage = new PluginJsonStorage<AuthenticatorsList>();
            new StoragePowerToysVersionInfo(ConfigV3Storage.FilePath, 1).Close();

            File.WriteAllBytes(ConfigV3Storage.FilePath, ConfigV3);

            File.Delete(ConfigPathV2);
            File.Delete(ConfigVersionPathV2);
        }

    }
}
