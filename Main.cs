using ManagedCommon;
using Wox.Plugin;
using Wox.Infrastructure;
using System.Web;
using System.Windows;
using OtpNet;
using PowertoysRunTOTP;

namespace PowerToysRunTOTP {

    public class Main: IPlugin {
        public static string PluginID => "2FC51DBA9F0F42108E26602486C186C1";
        private string IconCopy = "images/copy-light.png";
        private string IconAdd = "images/add-light.png";
        private string IconWarn = "images/warn-light.png";

        private PluginInitContext? Context { get; set; }
        public string Name => "TOTP";

        public string Description => "TOTP Code Generator";

        public List<Result> Query(Query query) {
            if (query.Search.StartsWith("otpauth://totp/")) {
                var list = new List<Result>();
                try {
                    var link = new Uri(query.Search);
                    var name = link.LocalPath.ToString()[1..];
                    var queries = HttpUtility.ParseQueryString(link.Query);
                    var sercet = queries.Get("secret") ?? throw new Exception();
                    list.Add(new Result {
                        Title = name,
                        SubTitle = "Add to list",
                        IcoPath = IconAdd,
                        Action = (e) => {
                            try {
                                var list = Config.LoadKeyList();
                                list.Add(new ConfigStruct.KeyEntry {
                                    Key = sercet,
                                    Name = name,
                                    IsEncrypted = false
                                });
                                Config.SaveKeyList(list);
                            } catch (Exception ex) {
                                MessageBox.Show(ex.Message + ex.StackTrace, "PowerToys TOTP Ran into error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }
                            return true;
                        }
                    });
                } catch (Exception) {
                    list.Add(new Result {
                        Title = "Invaild otpauth link",
                        SubTitle = "Check your link or try to copy it again",
                        IcoPath = IconWarn,
                        Action = (e) => {
                            return false;
                        }
                    });
                }
                return list;
            }
            if (query.Search.StartsWith("otpauth-migration://offline?")) {
                try {
                    var uri = new Uri(query.Search);
                    var queries = HttpUtility.ParseQueryString(uri.Query);
                    var payload = queries.Get("data") ?? throw new Exception();
                    var decoded = Payload.Parser.ParseFrom(Convert.FromBase64String(payload));


                    return new List<Result> {
                    new Result {
                        Title = "Add " + decoded.OtpParameters.Count() + " items to list",
                        SubTitle = "From Google Authenticator App, batch " + (decoded.BatchIndex + 1).ToString() + " / " + decoded.BatchSize,
                        IcoPath = IconAdd,
                        Action = (e) => {
                            var list = Config.LoadKeyList();
                            foreach (var item in decoded.OtpParameters) {
                                var key = Base32Encoding.ToString(item.Secret.ToByteArray());
                                var name = "";
                                if (item.Issuer.Length == 0) {
                                    name = item.Name;
                                } else if (item.Name.Length == 0) {
                                    name = item.Issuer + ": <NO NAME>";
                                } else {
                                    name = item.Issuer + ": " + item.Name;
                                }
                                list.Add(new ConfigStruct.KeyEntry{
                                    Name = name,
                                    Key = key,
                                    IsEncrypted = false
                                });
                            }
                            try {
                                Config.SaveKeyList(list);
                            } catch(Exception ex) {
                                MessageBox.Show(ex.Message + ex.StackTrace, "PowerToys TOTP Ran into error", MessageBoxButton.OK, MessageBoxImage.Exclamation);}
                            return true;
                        }
                    }
                };
                } catch (Exception) {
                    return new List<Result> {
                        new Result {
                            Title = "Invaild otpauth-migration link",
                            SubTitle = "Check your link or try to copy it again",
                            IcoPath = IconWarn,
                            Action = (e) => {
                                return false;
                            }
                        }
                    };
                }
            }
            List<ConfigStruct.KeyEntry> totpList;
            try {
                totpList = Config.LoadKeyList();
            } catch (Exception ex) {
                return new List<Result> {
                    new Result {
                        Title = ex.Message,
                        SubTitle = "Error when try to load config",
                        IcoPath = IconWarn
                    }
                };
            }
            var result = new List<Result>();

            totpList.ForEach(totp => {
                if (query.Search.Length != 0 && !StringMatcher.FuzzySearch(query.Search, totp.Name).Success)
                    return;
                var key = totp.Key;
                if (totp.IsEncrypted) {
                    key = Config.DecryptKey(key);
                }
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
            });
            if (result.Count == 0 && query.RawQuery.StartsWith(query.ActionKeyword)) {
                if (totpList.Count == 0) {
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

        public void Init(PluginInitContext context) {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            try {
                ConfigMigratorV0.Migrate();
            } catch (Exception) { }
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
    }
}