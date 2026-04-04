using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Helpers {

    [JsonSerializable(typeof(string))]
    public partial class TemplateJsonStringContext: JsonSerializerContext {
    }
    internal class TemplateJson {

        public static string Replace(string content, Dictionary<string, string>? kv) {
            var result = content;
            if (kv != null) {
                foreach (var pair in kv) {
                    result = result.Replace($"\"${{{pair.Key}}}\"", JsonSerializer.Serialize(pair.Value, TemplateJsonStringContext.Default.String));
                }
            }
            return result;
        }
    }
}
