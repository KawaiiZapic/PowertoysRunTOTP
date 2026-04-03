using System;
using System.Collections.Generic;
using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Helpers;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Zapic.PowerToys.TOTP.Core;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages;

internal partial class Management: DynamicListPage {
    public override IconInfo Icon => Icons.AddIcon;
    public override string Title => Resource.page_import_name;
    public override string Name => Resource.page_import_name;
    public override string Id => typeof(Management).FullName!;

    private bool inLinkImport = false;

    private static List<Authenticator> AuthList {
        get => ConfigManager.Data.Authenticators;
    }

    private string currentQuery = "";
    static ListItem HandleGoogleAuthImport(string url) {
        try {
            var parsed = Core.ParseGoogleExportLink(url);
            return new ListItem() {
                Title = string.Format(Resource.add_from_ga, parsed.Count),
                Subtitle = string.Format(Resource.add_from_ga_tip, parsed.Index + 1, parsed.BatchSize),
                Icon = Icons.Link,
                Command = new AnonymousCommand(() => {
                    ConfigManager.Data.Authenticators = [.. ConfigManager.Data.Authenticators, .. parsed.list];
                    ConfigManager.Save();
                })
            };
        } catch (Exception) {
            return new ListItem() {
                Title = Resource.invalid_ga_import_link,
                Subtitle = Resource.invalid_ga_import_link_tip,
                Icon = Icons.Warning
            };
        }
    }

    static ListItem HandleNormalOtpImport(string url) {
        try {
            var entity = Core.ParseLink(url);
            return new ListItem() {
                Title = entity.Name,
                Subtitle = Resource.add_from_otpauth_tip,
                Icon = Icons.Link,
                Command = new AnonymousCommand(() => {
                    AuthList.Add(entity);
                    ConfigManager.Save();
                }) { Name = Resource.import_command }
            };
        } catch {
            return new ListItem() {
                Title = Resource.invalid_otpauth_link,
                Subtitle = Resource.invalid_otpauth_link_tip,
                Icon = Icons.Warning
            };
        }
    }

    static readonly ListItem[] ManageCommands = [
        new ListItem(ScanQRCodeCommand) {
            Title = Resource.scan_from_screen,
            Subtitle = Resource.scan_from_screen_tip,
            Icon = Icons.QRCode
        },
        new ListItem(new NoOpCommand()) {
            Title = Resource.import_from_link,
            Subtitle = Resource.import_from_link_tip,
            Icon = Icons.Link
        },
        new ListItem(ImportFromPTRunCommand) {
            Title = Resource.add_from_ptrun_totp,
            Icon = Icons.OpenFile
        },
        new ListItem(ImportFromFileCommand) {
            Title = Resource.import_from_file,
            Subtitle = Resource.import_from_file_tip,
            Icon = Icons.Download
        },
        new ListItem(ExportToFileCommand) {
            Title = Resource.export_title,
            Subtitle = Resource.export_tip,
            Icon = Icons.Upload
        }
    ];

    public override IListItem[] GetItems() {
        if (currentQuery.StartsWith("otpauth://totp/")) {
            return [HandleNormalOtpImport(currentQuery)];
        } else if (currentQuery.StartsWith("otpauth-migration://offline?")) {
            return [HandleGoogleAuthImport(currentQuery)];
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
