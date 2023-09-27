using ManagedCommon;
using Wox.Plugin;
using System.IO;
using System.Text.Json;
using System.Web;
using System.Windows;
using OtpNet;

namespace PowerToysRunTOTP
{
    public class Struct
    {
        public String Name { get; set; }
        public String Key { get; set; }
    }

    public class Main : IPlugin
    {

        private string ConfigPath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Zapic.Plugin.TOTP\\TOTPList.json";
        private string IconCopy { get; set; }
        private string IconAdd { get; set; }
        private string IconWarn { get; set; }

        private PluginInitContext? Context { get; set; }
        public string Name => "TOTP";

        public string Description => "TOTP Code Generator";

        public List<Result> Query(Query query)
        {
            if (query.Search.StartsWith("otpauth://totp/")) {
                var list = new List<Result>();
                try {
                    var link = new Uri(query.Search);
                    var name = link.LocalPath.ToString().Substring(1);
                    var queries = HttpUtility.ParseQueryString(link.Query);
                    var sercet = queries.Get("secret") ?? throw new Exception();
                    list.Add(new Result {
                        Title = name,
                        SubTitle = "Add to list",
                        IcoPath = IconAdd,
                        Action = (e) => {
                            var list = loadConfig();
                            list.Add(new Struct {
                                Key = sercet,
                                Name = name
                            });
                            saveConfig(list);
                            return true;
                        }
                    });
                } catch (Exception) { }
                return list;
            }
            if (query.Search.StartsWith("otpauth-migration://offline?")) {
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
                            var list = loadConfig();
                            foreach (var item in decoded.OtpParameters) {
                                var key = Base32Encoding.ToString(item.Secret.ToByteArray());
                                if (list.Find(it => it.Key.Equals(key)) != null) continue;
                                var name = "";
                                if (item.Issuer.Length == 0) {
                                    name = item.Name;
                                } else if (item.Name.Length == 0) {
                                    name = item.Issuer + ": <NO NAME>";
                                } else {
                                    name = item.Issuer + ": " + item.Name;
                                }
                                list.Add(new Struct{ 
                                    Name = name,
                                    Key = key
                                });
                            }
                            saveConfig(list);
                            return true;
                        }
                    }
                };
            }
            List<Struct> totpList;
            try {
                totpList = loadConfig();
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
            
            totpList.ForEach(totp =>
            {
                if (query.Search.Length != 0 && !totp.Name.ToLower().Contains(query.Search.ToLower())) return;
                var totpInst = new Totp(Base32Encoding.ToBytes(totp.Key));
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
                if (totpList.Count == 0)
                {
                    result.Add(new Result
                    {
                        Title = "No TOTP found in config",
                        SubTitle = "Add TOTP to plugin by paste your setup link(totp://) first",
                        IcoPath = IconWarn,
                        Action = (e) =>
                        {
                            return false;
                        }
                    });
                }
                else {
                    result.Add(new Result
                    {
                        Title = "No matching result",
                        SubTitle = "Leave it blank to show all items",
                        IcoPath = IconWarn,
                        Action = (e) =>
                        {
                            return false;
                        }
                    });
                }
            }
            return result;
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        private void UpdateIconPath(Theme theme)
        {
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

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private List<Struct> loadConfig() {
            var fileInfo = new FileInfo(ConfigPath);
            if (!fileInfo.Exists)
            {
                if (!fileInfo.Directory!.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory!.FullName);
                }
                var newFile = File.Create(ConfigPath);
                var options = new JsonSerializerOptions { WriteIndented = true };
                JsonSerializer.Serialize(newFile, new List<Struct>(), options);
                newFile.Dispose();
            }
            var file = File.OpenRead(ConfigPath);
            var result = JsonSerializer.Deserialize<List<Struct>>(file);
            if (result == null)
            {
                throw new Exception("Failed to load config: Result is null");
            }
            file.Dispose();
            return result;
        }

        private void saveConfig(List<Struct> list)
        {
            var fileInfo = new FileInfo(ConfigPath);
            if (!fileInfo.Exists)
            {
                if (!fileInfo.Directory!.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory!.FullName);
                }
            }
            var file = File.Open(ConfigPath, FileMode.Create);
            var options = new JsonSerializerOptions { WriteIndented = true };
            JsonSerializer.Serialize(file, list, options);
            file.Dispose();
        }
    }
}