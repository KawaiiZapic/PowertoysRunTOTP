using ManagedCommon;
using Wox.Plugin;
using Wox.Infrastructure;
using System.Web;
using System.Windows;
using OtpNet;
using Wox.Infrastructure.Storage;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using Community.PowerToys.Run.Plugin.TOTP.localization;

namespace Community.PowerToys.Run.Plugin.TOTP {

    public class Main: IPlugin, ISavable, IReloadable, IDisposable, IPluginI18n {
        public string Name => Resource.plugin_name;
        public string Description => Resource.plugin_description;
        public string GetTranslatedPluginTitle() => Resource.plugin_name;
        public string GetTranslatedPluginDescription() => Resource.plugin_description;
        public static string PluginID => "2FC51DBA9F0F42108E26602486C186C1";

        private string theme = "light";

        private PluginInitContext? Context { get; set; }

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


            if (_list.Entries.Count == 0 && query.RawQuery.StartsWith(query.ActionKeyword)) {
                result.Add(new Result {
                    Title = Resource.no_authenticator,
                    SubTitle = Resource.no_authenticator_tip,
                    IcoPath = GetIconByName("warn"),
                    Action = (e) => {
                        return false;
                    }
                });
                return result;
            }

            _list.Entries.ForEach(totp => {
                if (query.Search.Length != 0 && !StringMatcher.FuzzySearch(query.Search, totp.Name).Success)
                    return;
                var key = totp.Key;
                if (totp.IsEncrypted) {
                    key = DecryptKey(key);
                }
                if (!CheckKeyValid(key)) {
                    result.Add(new Result {
                        Title = string.Format(Resource.invalid_secret, totp.Name),
                        SubTitle = Resource.invalid_secret_tip,
                        IcoPath = GetIconByName("copy"),
                        Action = (e) => {
                            return false;
                        }
                    });
                } else {
                    var totpInst = new Totp(Base32Encoding.ToBytes(key));
                    result.Add(new Result {
                        Title = string.Format(Resource.copy_to_clipboard, totpInst.ComputeTotp(), totp.Name),
                        SubTitle = string.Format(Resource.copy_to_clipboard_tip, totpInst.RemainingSeconds()),
                        IcoPath = GetIconByName("copy"),
                        Action = (e) => {
                            for (int i = 0; i < 10; i++) {
                                try {
                                    Clipboard.SetText(totpInst.ComputeTotp());
                                    return true;
                                } catch (COMException) {
                                    if (i == 9) {
                                        MessageBox.Show(Resource.copy_to_clipboard_err);
                                        return true;
                                    }
                                    Thread.Sleep(100);
                                }
                            }
                            return true;
                        }
                    });
                }
            });
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
                        Title = string.Format(Resource.add_from_ga, decoded.OtpParameters.Count),
                        SubTitle = string.Format(Resource.add_from_ga_tip, decoded.BatchIndex + 1, decoded.BatchSize),
                        IcoPath = GetIconByName("add"),
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
                            Title = Resource.invalid_ga_import_link,
                            SubTitle = Resource.invalid_ga_import_link_tip,
                            IcoPath = GetIconByName("warn"),
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
                        Title = string.Format(Resource.invalid_secret, name),
                        SubTitle = Resource.invalid_secret_tip,
                        IcoPath = GetIconByName("warn"),
                        Action = (e) => {
                            return false;
                        }
                    });
                   
                } else {
                    list.Add(new Result {
                        Title = name,
                        SubTitle = Resource.add_from_otpauth_tip,
                        IcoPath = GetIconByName("add"),
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
                    Title = Resource.invalid_otpauth_link,
                    SubTitle = Resource.invalid_otpauth_link_tip,
                    IcoPath = GetIconByName("warn"),
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
                this.theme = "light";
            } else {
                this.theme = "dark";
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

        private string GetIconByName(string name) {
            return "images/" + name + "-" + theme + ".png";
        }
    }
}