using ManagedCommon;
using Wox.Plugin;
using Wox.Infrastructure;
using System.Web;
using System.Windows;
using OtpNet;
using Wox.Infrastructure.Storage;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace Community.PowerToys.Run.Plugin.TOTP {

    public class Main: IPlugin, ISavable, IReloadable, IDisposable {
        public static string PluginID => "2FC51DBA9F0F42108E26602486C186C1";
        private string IconCopy = "images/copy-light.png";
        private string IconAdd = "images/add-light.png";
        private string IconWarn = "images/warn-light.png";

        private PluginInitContext? Context { get; set; }
        public string Name => "TOTP";

        public string Description => "TOTP Code Generator";
        private bool _disposed;

        private PluginJsonStorage<OTPList> _storage;
        private OTPList _list;

        public Main() {
            _storage = new PluginJsonStorage<OTPList>();
            try {
                ConfigMigratorV0.Migrate();
            } catch (Exception) { }
            try {
                ConfigMigratorV1.Migrate();
            } catch (Exception) { }
            // Enforce PT DO not delete old version config
            new StoragePowerToysVersionInfo(_storage.FilePath, 1).Close();
            _list = _storage.Load();
            _list.Entries.ForEach(totp => {
                if (!totp.IsEncrypted) { 
                    totp.Key = EncryptKey(totp.Key);
                    totp.IsEncrypted = true;
                }
            });
            _storage.Save();
        }

        public List<Result> Query(Query query) {
            if (query.Search.StartsWith("otpauth://totp/")) {
                return HandleNormalOtpImport(query.Search);
            }
            if (query.Search.StartsWith("otpauth-migration://offline?")) {
                return HandleGoogleAuthImport(query.Search);
            }

            var result = new List<Result>();

            _list.Entries.ForEach(totp => {
                if (query.Search.Length != 0 && !StringMatcher.FuzzySearch(query.Search, totp.Name).Success)
                    return;
                var key = totp.Key;
                if (totp.IsEncrypted) {
                    key = DecryptKey(key);
                }
                if (!CheckKeyValid(key)) {
                    result.Add(new Result {
                        Title = totp.Name + ": Invalid OTP secret",
                        SubTitle = "This OTP contains a invalid OTP secret that can't be decode as base32 data",
                        IcoPath = IconWarn,
                        Action = (e) => {
                            return false;
                        }
                    });
                } else {
                    var totpInst = new Totp(Base32Encoding.ToBytes(key));
                    result.Add(new Result {
                        Title = totpInst.ComputeTotp() + " - " + totp.Name,
                        SubTitle = "Copy to clipboard - Expired in " + totpInst.RemainingSeconds().ToString() + "s",
                        IcoPath = IconCopy,
                        Action = (e) => {
                            Clipboard.SetText(totpInst.ComputeTotp());
                            return true;
                        }
                    });
                }
            });
            if (result.Count == 0 && query.RawQuery.StartsWith(query.ActionKeyword)) {
                if (_list.Entries.Count == 0) {
                    result.Add(new Result {
                        Title = "No TOTP found in config",
                        SubTitle = "Add TOTP to plugin by paste your setup link(otpauth://) first",
                        IcoPath = IconWarn,
                        Action = (e) => {
                            return false;
                        }
                    });
                } else {
                    result.Add(new Result {
                        Title = "No matching result",
                        SubTitle = "Leave it blank to show all items",
                        IcoPath = IconWarn,
                        Action = (e) => {
                            return false;
                        }
                    });
                }
            }
            return result;
        }
        public List<Result> HandleGoogleAuthImport(string url) {
            try {
                var uri = new Uri(url);
                var queries = HttpUtility.ParseQueryString(uri.Query);
                var payload = queries.Get("data") ?? throw new Exception();
                var decoded = Payload.Parser.ParseFrom(Convert.FromBase64String(payload));

                return new List<Result> {
                    new() {
                        Title = "Add " + decoded.OtpParameters.Count + " items to list",
                        SubTitle = "From Google Authenticator App, batch " + (decoded.BatchIndex + 1).ToString() + " / " + decoded.BatchSize,
                        IcoPath = IconAdd,
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
                                _list.Entries.Add(new OTPList.KeyEntry{
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
                return new List<Result> {
                        new () {
                            Title = "Invalid otpauth-migration link",
                            SubTitle = "Check your link or try to copy it again",
                            IcoPath = IconWarn,
                            Action = (e) => {
                                return false;
                            }
                        }
                    };
            }
        }
        public List<Result> HandleNormalOtpImport(string url) {
            var list = new List<Result>();
            try {
                var link = new Uri(url);
                var name = link.LocalPath.ToString()[1..];
                var queries = HttpUtility.ParseQueryString(link.Query);
                var sercet = queries.Get("secret") ?? throw new Exception();
                if (!CheckKeyValid(sercet)) {
                    list.Add(new Result {
                        Title = "Invalid OTP secret",
                        SubTitle = "This link contains a invalid OTP secret that can't be decode as base32 data",
                        IcoPath = IconWarn,
                        Action = (e) => {
                            return false;
                        }
                    });
                   
                } else {
                    list.Add(new Result {
                        Title = name,
                        SubTitle = "Add to list",
                        IcoPath = IconAdd,
                        Action = (e) => {
                            _list.Entries.Add(new OTPList.KeyEntry {
                                Key = EncryptKey(sercet),
                                Name = name,
                                IsEncrypted = true
                            });
                            _storage.Save();
                            return true;
                        }
                    });
                }
            } catch (Exception) {
                list.Add(new Result {
                    Title = "Invalid otpauth link",
                    SubTitle = "Check your link or try to copy it again",
                    IcoPath = IconWarn,
                    Action = (e) => {
                        return false;
                    }
                });
            }
            return list;
        }

        public static bool CheckKeyValid(string key) {
            try {
                Base32Encoding.ToBytes(key);
                return true;
            } catch (Exception) {
                return false;
            }
        }

        public void Init(PluginInitContext context) {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        private void UpdateIconPath(Theme theme) {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite) {
                IconCopy = "images/copy-light.png";
                IconAdd = "images/add-light.png";
                IconWarn = "images/warn-light.png";
            } else {
                IconCopy = "images/copy-dark.png";
                IconAdd = "images/add-dark.png";
                IconWarn = "images/warn-dark.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) {
            UpdateIconPath(newTheme);
        }

        public void Save() {
            _storage.Save();
        }

        public static string DecryptKey(string encrypted) {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encrypted), null, DataProtectionScope.CurrentUser));
        }

        public static string EncryptKey(string unencrypted) {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(unencrypted), null, DataProtectionScope.CurrentUser));
        }

        public void ReloadData() {
            _list = _storage.Load();
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed && disposing) {
                if (Context != null && Context.API != null) {
                    Context.API.ThemeChanged -= OnThemeChanged;
                }

                _disposed = true;
            }
        }

        public Control CreateSettingPanel() {
            throw new NotImplementedException();
        }
    }
}