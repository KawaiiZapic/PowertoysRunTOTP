using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Genesis.QRCodeLib;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Zapic.PowerToys.TOTP.Core;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages;

internal partial class Import: DynamicListPage {
    public override IconInfo Icon => IconHelpers.FromRelativePath("Assets\\add.png");
    public override string Title => Resource.page_import_name;
    public override string Name => Resource.page_import_name;

    public override string Id => "import";

    private static readonly QRDecoder decoder = new();

    private bool inLinkImport = false;

    private static List<Authenticator> AuthList {
        get => ConfigManager.Data.Authenticators;
    }

    private string currentQuery = "";
    static List<string> GetSetupURLFromScreen() {
        var result = new List<string>();
        // TODO: get acutually screen size
        var size = new Size {
            Width = 8192,
            Height = 8192
        };
        var screenBitmap = new Bitmap(size.Width, size.Height);
        using (var g = Graphics.FromImage(screenBitmap)) {
            g.CopyFromScreen(Point.Empty, Point.Empty, size);
        }
        try {
            var data = decoder.ImageDecoder(screenBitmap);
            if (data != null) {
                foreach (var item in data) {
                    var link = QRCode.ByteArrayToStr(item);
                    if (link != null && link.StartsWith("otpauth://totp/")) {
                        result.Add(link);
                    }
                }
            }
            return result;
        } catch {
            return [];
        }
    }

    static IListItem[] HandleGoogleAuthImport(string url) {
        try {
            var parsed = Core.ParseGoogleExportLink(url);
            return [
                    new ListItem() {
                        Title = string.Format(Resource.add_from_ga, parsed.Count),
                        Subtitle = string.Format(Resource.add_from_ga_tip, parsed.Index + 1, parsed.BatchSize),
                        Icon = new IconInfo("\uE71B"),
                        Command = new AnonymousCommand(() => {
                            ConfigManager.Data.Authenticators = [.. ConfigManager.Data.Authenticators, .. parsed.list];
                            ConfigManager.Save();
                        })
                    }
                ];
        } catch (Exception) {
            return [
                    new ListItem() {
                        Title = Resource.invalid_ga_import_link,
                        Subtitle = Resource.invalid_ga_import_link_tip,
                        Icon = new IconInfo("\uE7BA")
                    }
                ];
        }
    }

    static IListItem[] HandleNormalOtpImport(string url) {
        try {
            var entity = Core.ParseLink(url);
            return [
                    new ListItem() {
                        Title = entity.Name,
                        Subtitle = Resource.add_from_otpauth_tip,
                        Icon = new IconInfo("\uE71B"),
                        Command = new AnonymousCommand(() => {
                            AuthList.Add(entity);
                            ConfigManager.Save();
                        }) { Name = Resource.import_command }
                    }
                ];
        } catch {
            return [
                    new ListItem() {
                        Title = Resource.invalid_otpauth_link,
                        Subtitle = Resource.invalid_otpauth_link_tip,
                        Icon = new IconInfo("\uE7BA")
                    }
            ];
        }
    }

    static IListItem[] ManageCommands => [
        new ListItem(new AnonymousCommand(() => {
            Task.Run(async () => {
                await Task.Delay(500);
                List<string> result = GetSetupURLFromScreen();
                if (result.Count > 0) {
                     result.ForEach(item => {
                         AuthList.Add(Core.ParseLink(item));
                     });
                    ConfigManager.Save();
                }
                new ToastContentBuilder()
                     .AddText(Resource.scan_from_screen_done_title)
                     .AddText(result.Count switch {
                         0 => Resource.scan_from_screen_empty,
                         1 => string.Format(Resource.scan_from_screen_done_one, Core.ParseLink(result[0]).Name),
                         _ => string.Format(Resource.scan_from_screen_done_two_more, result.Count)
                     })
                     .Show();
            });
        }) {
            Name = Resource.import_command
        }) {
            Title = Resource.scan_from_screen,
            Subtitle = Resource.scan_from_screen_tip,
            Icon = new IconInfo("\uED14")
        },
        new ListItem(new NoOpCommand()) {
            Title = Resource.import_from_link,
            Subtitle = Resource.import_from_link_tip,
            Icon = new IconInfo("\uE71B")
        },
        new ListItem(new AnonymousCommand(() => {
            try {
                var filePath = Path.Combine(Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%") + "\\Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Community.PowerToys.Run.Plugin.TOTP\\", "AuthenticatorsList.json");
                ConfigManager.Load(filePath);
                ConfigManager.Save();
                new ToastStatusMessage(new StatusMessage {
                    Message = string.Format(Resource.import_done_tip, AuthList.Count),
                    State = MessageState.Success
                }).Show();
            } catch {
                new ToastStatusMessage(new StatusMessage {
                    Message = Resource.import_failed_title,
                    State = MessageState.Error
                }).Show();
            }
        }) {
            Result = CommandResult.GoBack(),
            Name = Resource.import_command
        }) {
            Title = Resource.add_from_ptrun_totp,
            Icon = new IconInfo("\uE8E5")
        }
    ];

    public override IListItem[] GetItems() {
        if (currentQuery.StartsWith("otpauth://totp/")) {
            return HandleNormalOtpImport(currentQuery);
        } else if (currentQuery.StartsWith("otpauth-migration://offline?")) {
            return HandleGoogleAuthImport(currentQuery);
        } else {
            return ManageCommands;
        }
    }

    public override void UpdateSearchText(string _, string newSearch) {
        currentQuery = newSearch;
        if (newSearch.StartsWith("otpauth://totp/") || newSearch.StartsWith("otpauth-migration://offline?")) {
            inLinkImport = true;
            RaiseItemsChanged();
        } else if (inLinkImport) {
            inLinkImport = false;
            RaiseItemsChanged();
        }
    }
}
