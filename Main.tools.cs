using Community.PowerToys.Run.Plugin.TOTP.localization;
using Genesis.QRCodeLib;
using ManagedCommon;
using OtpNet;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Wox.Plugin;
using Point = System.Drawing.Point;

namespace Community.PowerToys.Run.Plugin.TOTP {
    class SecretInvalidException: Exception { }

    public partial class Main {

        static Authenticator ParseOTPLink(string url) {
            var link = new Uri(url);
            var name = link.LocalPath.ToString()[1..];
            var queries = HttpUtility.ParseQueryString(link.Query);
            var secret = queries.Get("secret") ?? throw new Exception();
            if (!CheckKeyValid(secret)) {
                throw new SecretInvalidException();
            }
            return new() {
                Key = EncryptKey(secret),
                Name = name,
                IsEncrypted = true
            };
        }

        static bool CheckKeyValid(string key) {
            try {
                Base32Encoding.ToBytes(key);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        static string DecryptKey(string encrypted) {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser));
        }

        static string EncryptKey(string unencrypted) {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(unencrypted), null, DataProtectionScope.CurrentUser));
        }

        string GetIconByName(string name) {
            return "images/" + name + "-" + theme + ".png";
        }

        static List<string> GetSetupURLFromScreen() {
            var result = new List<string>();
            var screenSize = System.Windows.Forms.Screen.GetBounds(Point.Empty);
            var screenBitmap = new Bitmap(screenSize.Width, screenSize.Height);
            using (var g = Graphics.FromImage(screenBitmap)) {
                g.CopyFromScreen(Point.Empty, Point.Empty, screenSize.Size);
            }
            var decoder = new QRDecoder();
            var data = decoder.ImageDecoder(screenBitmap);
            if (data != null) {
                foreach (var item in data) {
                    var link = QRDecoder.ByteArrayToStr(item);
                    if (link != null && link.StartsWith("otpauth://totp/")) {
                        result.Add(link);
                    }
                }
            }
            return result;
        }

        Result ScanQRCodeResultFactory(Query query) {
            return new() {
                Title = Resource.scan_from_screen,
                SubTitle = Resource.scan_from_screen_tip,
                IcoPath = GetIconByName("scan"),
                QueryTextDisplay = query.Search,
                Action = (e) => {
                    Task.Run(() => {
                        Task.Delay(500);
                        var list = GetSetupURLFromScreen();
                        int success = 0;
                        string name = "";
                        foreach (var link in list) {
                            try {
                                var entry = ParseOTPLink(link);
                                name = entry.Name;
                                _list.Authenticators.Add(entry);
                                success++;
                            } catch (Exception) {
                            }
                        }
                        if (success > 1) {
                            Context!.API.ShowNotification(Resource.scan_from_screen_done_title, string.Format(Resource.scan_from_screen_done_two_more, success));
                        } else if (success == 1) {
                            Context!.API.ShowNotification(Resource.scan_from_screen_done_title, string.Format(Resource.scan_from_screen_done_one, name));
                        } else {
                            Context!.API.ShowNotification(Resource.scan_from_screen_done_title, Resource.scan_from_screen_empty);
                        }
                    });
                    return true;
                }
            };
        }

        List<Result> HandleGoogleAuthImport(string url) {
            try {
                var uri = new Uri(url);
                var queries = HttpUtility.ParseQueryString(uri.Query);
                var payload = queries.Get("data") ?? throw new Exception();
                var decoded = Payload.Parser.ParseFrom(Convert.FromBase64String(payload));

                return new() {
                    new() {
                        Title = string.Format(Resource.add_from_ga, decoded.OtpParameters.Count),
                        SubTitle = string.Format(Resource.add_from_ga_tip, decoded.BatchIndex + 1, decoded.BatchSize),
                        IcoPath = GetIconByName("add"),
                        QueryTextDisplay = url,
                        Action = (e) => {
                            foreach (var item in decoded.OtpParameters) {
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
                                _list.Authenticators.Add(new () {
                                    Name = name,
                                    Key = EncryptKey(key),
                                    IsEncrypted = true
                                });
                                _storage.Save();
                            }
                            return true;
                        }
                    }
                };
            } catch (Exception) {
                return new() {
                    new () {
                        Title = Resource.invalid_ga_import_link,
                        SubTitle = Resource.invalid_ga_import_link_tip,
                        QueryTextDisplay = url,
                        IcoPath = GetIconByName("warn"),
                        Action = (e) => {
                            return false;
                        }
                    }
                };
            }
        }

        List<Result> HandleNormalOtpImport(string url) {
            try {
                var entry = ParseOTPLink(url);
                return new() {
                    new () {
                        Title = entry.Name,
                        SubTitle = Resource.add_from_otpauth_tip,
                        IcoPath = GetIconByName("add"),
                        QueryTextDisplay = url,
                        Action = (e) => {
                            _list.Authenticators.Add(entry);
                            _storage.Save();
                                return true;
                        }
                    }
                };
            } catch (SecretInvalidException) {
                return new() {
                    new() {
                        Title = Resource.invalid_secret,
                        SubTitle = Resource.invalid_secret_tip,
                        QueryTextDisplay = url,
                        IcoPath = GetIconByName("warn"),
                        Action = (e) => {
                            return false;
                        }
                    }
                };
            } catch (Exception) {
                return new() {
                    new() {
                        Title = Resource.invalid_otpauth_link,
                        SubTitle = Resource.invalid_otpauth_link_tip,
                        QueryTextDisplay = url,
                        IcoPath = GetIconByName("warn"),
                        Action = (e) => {
                            return false;
                        }
                    }
                };
            }
        }

        void OnThemeChanged(Theme currentTheme, Theme newTheme) {
            OnThemeChanged(newTheme);
        }

        void OnThemeChanged(Theme newTheme) {
            if (newTheme == Theme.Light || newTheme == Theme.HighContrastWhite) {
                this.theme = "light";
            } else {
                this.theme = "dark";
            }
        }
    }
}
