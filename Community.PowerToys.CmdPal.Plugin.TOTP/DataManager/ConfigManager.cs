using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Storage;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.DataManager {

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(AuthenticatorsList))]
    internal partial class SourceGenerationContext: JsonSerializerContext { }

    class ConfigManager {

        private static readonly string fileName = Path.Combine(ApplicationData.Current.LocalFolder.Path, "OTPList.json");
        private static readonly int CurrentVersion = 3;

        private static AuthenticatorsList _listInst = null!;

        public static AuthenticatorsList Data {
            get {
                if (_listInst == null) {
                    Load();
                }
                return _listInst!;
            }
        }

        public static void Load(string path) {
            if (File.Exists(path)) {
                try {
                    _listInst = JsonSerializer.Deserialize(File.ReadAllText(path), SourceGenerationContext.Default.AuthenticatorsList)!;
                } catch { }
            }
            _listInst ??= new AuthenticatorsList();
            _listInst.Version = CurrentVersion;
        }

        public static void Load() => Load(fileName);

        public static void Save() {
            File.WriteAllText(fileName, JsonSerializer.Serialize(_listInst, SourceGenerationContext.Default.AuthenticatorsList));
            OnDataChanged?.Invoke();
        }

        public static event Action? OnDataChanged;
    }
}
