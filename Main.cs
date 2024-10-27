using Community.PowerToys.Run.Plugin.TOTP.localization;
using OtpNet;
using System.Runtime.InteropServices;
using System.Windows;
using Wox.Infrastructure;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.TOTP {

    public partial class Main: IPlugin, ISavable, IReloadable, IDisposable, IPluginI18n, IContextMenu {
        public string Name => Resource.plugin_name;
        public string Description => Resource.plugin_description;
        public string GetTranslatedPluginTitle() => Resource.plugin_name;
        public string GetTranslatedPluginDescription() => Resource.plugin_description;
        public static string PluginID => "2FC51DBA9F0F42108E26602486C186C1";

        private bool _disposed;

        private readonly PluginJsonStorage<AuthenticatorsList> _storage;
        private AuthenticatorsList _list;
        private string theme = "light";
        private PluginInitContext? Context { get; set; }

        public Main() {
            try {
                ConfigMigratorV0.Migrate();
            } catch { }
            try {
                ConfigMigratorV1.Migrate();
            } catch { }
            try {
                ConfigMigratorV2.Migrate();
            } catch { }

            _storage = new PluginJsonStorage<AuthenticatorsList>();
            // Enforce PT DO not delete old version config
            new StoragePowerToysVersionInfo(_storage.FilePath, 1).Close();
            _list = _storage.Load();
            _list.Authenticators.ForEach(entity => {
                if (!entity.IsEncrypted) {
                    entity.Key = EncryptKey(entity.Key);
                    entity.IsEncrypted = true;
                }
            });
            _storage.Save();
        }

        public void Init(PluginInitContext context) {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            OnThemeChanged(context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query) {
            var isSpecQuery = query.ActionKeyword != "";
            if (isSpecQuery && query.Search.StartsWith("!")) {
                return new List<Result> {
                    ScanQRCodeResultFactory(query)
                };
            }
            if (query.Search.StartsWith("otpauth://totp/")) {
                return HandleNormalOtpImport(query.Search);
            }
            if (query.Search.StartsWith("otpauth-migration://offline?")) {
                return HandleGoogleAuthImport(query.Search);
            }

            var result = new List<Result>();

            if (_list.Authenticators.Count == 0 && isSpecQuery) {
                result.Add(new Result {
                    Title = Resource.no_authenticator,
                    SubTitle = Resource.no_authenticator_tip,
                    QueryTextDisplay = query.Search,
                    IcoPath = GetIconByName("warn"),
                    Action = (e) => {
                        return false;
                    }
                });
                result.Add(ScanQRCodeResultFactory(query));
                return result;
            }

            _list.Authenticators.ForEach((authenticator) => {
                if (query.Search.Length != 0 && !StringMatcher.FuzzySearch(query.Search, authenticator.Name).Success)
                    return;
                var key = authenticator.Key;
                if (authenticator.IsEncrypted) {
                    try {
                        key = DecryptKey(key);
                    } catch (Exception) {
                        key = null;
                    }
                }
                if (key == null || !CheckKeyValid(key)) {
                    result.Add(new Result {
                        Title = string.Format(Resource.invalid_secret, authenticator.Name),
                        SubTitle = Resource.invalid_secret_tip,
                        QueryTextDisplay = authenticator.Name,
                        IcoPath = GetIconByName("warn"),
                        ContextData = authenticator,
                        Action = (e) => {
                            return false;
                        }
                    });
                } else {
                    var AuthenticatorInst = new Totp(Base32Encoding.ToBytes(key));
                    result.Add(new Result {
                        Title = string.Format(Resource.copy_to_clipboard, AuthenticatorInst.ComputeTotp(), authenticator.Name),
                        SubTitle = string.Format(Resource.copy_to_clipboard_tip, AuthenticatorInst.RemainingSeconds()),
                        IcoPath = GetIconByName("copy"),
                        QueryTextDisplay = authenticator.Name,
                        ContextData = authenticator,
                        Action = (e) => {
                            for (int i = 0; i < 10; i++) {
                                try {
                                    Clipboard.SetText(AuthenticatorInst.ComputeTotp());
                                    return true;
                                } catch (COMException) {
                                    if (i == 9) {
                                        Context!.API.ShowMsg(Resource.copy_to_clipboard_err);
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


        public void Save() {
            _storage.Save();
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

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult) {
            var result = new List<ContextMenuResult>();
            if (selectedResult.ContextData is not Authenticator)
                return result;
            var entry = (Authenticator)selectedResult.ContextData;
            result.Add(new ContextMenuResult {
                Glyph = "\xe8ac",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Title = Resource.totp_rename_title,
                Action = (e) => {
                    var dialog = new Ookii.Dialogs.WinForms.InputDialog {
                        MainInstruction = Resource.totp_rename_title,
                        Content = string.Format(Resource.totp_rename_description, entry.Name),
                        WindowTitle = "PowerToys",
                        Input = entry.Name
                    };
                    var result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && dialog.Input.Length > 0) {
                        entry.Name = dialog.Input;
                        _storage.Save();
                    }
                    return true;
                }
            });
            result.Add(new ContextMenuResult {
                Glyph = "\xe74d",
                FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                Title = Resource.totp_delete_title,
                Action = (e) => {
                    var dialog = new Ookii.Dialogs.WinForms.InputDialog() {
                        MainInstruction = Resource.totp_delete_title,
                        Content = string.Format(Resource.totp_delete_description, entry.Name),
                        WindowTitle = "PowerToys"
                    };
                    var result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK && dialog.Input == "DELETE") {
                        _list.Authenticators.Remove(entry);
                        _storage.Save();
                        Context!.API.ShowNotification(Resource.totp_delete_title, string.Format(Resource.totp_delete_done, entry.Name));
                    }
                    return true;
                }
            });
            return result;
        }
    }
}
