using System.Collections.Generic;
using System.Text.Json;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Helpers {
    internal class TemplateJson {

        public static string Replace(string content, Dictionary<string, string>? kv) {
            var result = content;
            if (kv != null) {
                foreach (var pair in kv) {
#pragma warning disable IL2026, IL3050
                    result = result.Replace($"\"${{{pair.Key}}}\"", JsonSerializer.Serialize(pair.Value));
#pragma warning restore IL2026, IL3050
                }
            }
            return result;
        }
    }
}
