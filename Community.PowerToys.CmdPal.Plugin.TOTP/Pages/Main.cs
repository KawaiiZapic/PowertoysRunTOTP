// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Genesis.QRCodeLib;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Zapic.PowerToys.TOTP.Core;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages;

internal sealed partial class Main: DynamicListPage {

    private static List<Authenticator> AuthList {
        get => ConfigManager.Data.Authenticators;
    }

    public Main() {
        Icon = IconHelpers.FromRelativePaths("Assets\\icon-light.png", "Assets\\icon-dark.png");
        Title = "TOTP";
        Name = Resource.page_totp_open;
    }

    private string currentQuery = "";

    static IListItem[] ToAuthenticatorResultList(IEnumerable<Authenticator> list) {
        return [.. list.Select<Authenticator, ListItem>((authenticator) => {
            var result = authenticator.GetResult();
            return new() {
                Title = authenticator.Name,
                Subtitle = string.Format(Resource.expired_in, result.Remain),
                Icon = new IconInfo("\uE8C8"),
                Command = new AnonymousCommand(() => {
                    new CopyTextCommand(authenticator.GetResult().Code).Invoke();
                }) {
                    Name = Resource.copy_to_cilpboard_command
                },
                Tags = [
                    new Tag {
                        Text = result.Code.ToString()
                    }
                ]
            };
        })];
    }


    IListItem[] HandleGoogleAuthImport(string url) {
        try {
            var parsed = Core.ParseGoogleExportLink(url);
            return [
                    new ListItem() {
                        Title = string.Format(Resource.add_from_ga, parsed.Count),
                        Subtitle = string.Format(Resource.add_from_ga_tip, parsed.Index + 1, parsed.BatchSize),
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

    IListItem[] HandleNormalOtpImport(string url) {
        try {
            var entity = Core.ParseLink(url);
            return [
                    new ListItem() {
                        Title = entity.Name,
                        Subtitle = Resource.add_from_otpauth_tip,
                        Command = new AnonymousCommand(() => {
                            AuthList.Add(entity);
                            ConfigManager.Save();
                        })
                    }
                ];
        } catch (Exception) {
            return [
                    new ListItem() {
                        Title = Resource.invalid_otpauth_link,
                        Subtitle = Resource.invalid_otpauth_link_tip,
                        Icon = new IconInfo("\uE7BA")
                    }
                ];
        }
    }

    IListItem[] ManageCommands => [
        new ListItem(new AnonymousCommand(() => {
            var result = GetSetupURLFromScreen();
            result.ForEach(item => {
                AuthList.Add(Core.ParseLink(item));
            });
            ConfigManager.Save();
        })) {
            Title = Resource.scan_from_screen,
            Subtitle = Resource.scan_from_screen_tip,
            Icon = new IconInfo("\uED14")
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
            Result = CommandResult.KeepOpen()
        }) {
            Title = Resource.add_from_ptrun_totp,
            Icon = new IconInfo("\uE8E5")
        }
    ];

    public override IListItem[] GetItems() {
        if (currentQuery.StartsWith("!")) {
            return ManageCommands;
        } else if (currentQuery.StartsWith("otpauth://totp/")) {
            return HandleNormalOtpImport(currentQuery);
        } else if (currentQuery.StartsWith("otpauth-migration://offline?")) {
            return HandleGoogleAuthImport(currentQuery);
        } else {
            if (string.IsNullOrEmpty(currentQuery)) {
                return ToAuthenticatorResultList(AuthList);
            }
            return ToAuthenticatorResultList(
                AuthList
                .Select(item => (FuzzyStringMatcher.ScoreFuzzy(currentQuery, item.Name), item))
                .Where(item => item.Item1 > 0)
                .OrderByDescending(item => item.Item1)
                .Select(item => item.item)
            );
        }
    }

    static List<string> GetSetupURLFromScreen() {
        var result = new List<string>();
        // TODO: Get display size by something else
        var size = new Size {
            Width = 1920,
            Height = 1080
        };
        var screenBitmap = new Bitmap(size.Width, size.Height);
        using (var g = Graphics.FromImage(screenBitmap)) {
            g.CopyFromScreen(Point.Empty, Point.Empty, size);
        }
        var decoder = new QRDecoder();
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
    }

    public override void UpdateSearchText(string oldSearch, string newSearch) {
        currentQuery = newSearch;
        RaiseItemsChanged();
    }
}