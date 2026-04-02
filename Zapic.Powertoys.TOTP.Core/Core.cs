using OtpNet;
using System.Web;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Zapic.PowerToys.TOTP.Core {
    public class Core {
        class SecretInvalidException: Exception { }

        public class ParseGAExportResult {
            public int Index;
            public int BatchSize;
            public int Count;
            public IEnumerable<Authenticator> list = null!;
        }
        static bool CheckKeyValid(string key) {
            try {
                Base32Encoding.ToBytes(key);
                return true;
            } catch (Exception) {
                return false;
            }
        }
        public static Authenticator ParseLink(string link) {
            var uri = new Uri(link);
            if (uri.Scheme != "otpauth") {
                throw new Exception();
            }
            var name = uri.LocalPath.ToString()[1..];
            if (string.IsNullOrEmpty(name)) {
                name = "<NO NAME>";
            }
            var queries = HttpUtility.ParseQueryString(uri.Query);
            var secret = queries.Get("secret") ?? throw new Exception();
            if (!CheckKeyValid(secret)) {
                throw new SecretInvalidException();
            }
            var result = new Authenticator() {
                Name = name
            };
            result.SetUnencrypted(secret);

            return result;
        }

        public static ParseGAExportResult ParseGoogleExportLink(string link) {
            var uri = new Uri(link);
            if (uri.Scheme != "otpauth-migration") {
                throw new Exception();
            }
            var queries = HttpUtility.ParseQueryString(uri.Query);
            var payload = queries.Get("data") ?? throw new Exception();
            var decoded = Payload.Parser.ParseFrom(Convert.FromBase64String(payload));
            var list = decoded.OtpParameters.Select(item => {
                var key = Base32Encoding.ToString(item.Secret.ToByteArray());
                var name = item.Issuer;
                if (item.Name.Length > 0) {
                    if (name.Length > 0) {
                        name += ": " + item.Name;
                    } else {
                        name = item.Name;
                    }
                } else {
                    if (name.Length > 0) {
                        name += ": <NO NAME>";
                    } else {
                        name = "<NO NAME>";
                    }
                }
                var result = new Authenticator() {
                    Name = name
                };
                result.SetUnencrypted(key);
                return result;
            });
            return new ParseGAExportResult {
                list = list,
                Index = decoded.BatchIndex,
                BatchSize = decoded.BatchSize,
                Count = decoded.OtpParameters.Count

            };
        }
    }
}
