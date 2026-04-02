// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Microsoft.CmdPal.Common.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.Linq;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages;

internal sealed partial class Main: ListPage {
    public override IconInfo Icon => IconHelpers.FromRelativePath("Assets\\icon.png");
    public override string Title => Resource.page_totp_name;
    public override string Name => Resource.page_totp_name;
    public override string Id => "main";
    private static List<Authenticator> AuthList {
        get => ConfigManager.Data.Authenticators;
    }

    public Main() {
        ConfigManager.OnDataChanged += () => {
            RaiseItemsChanged();
        };
    }

    public override IListItem[] GetItems() {
        if (AuthList.Count == 0) {
            return [
                new ListItem {
                    Title = Resource.no_authenticator,
                    Subtitle = Resource.no_authenticator_tip,
                    Command = new Import()
                }
            ];
        }
        return [.. AuthList.Select<Authenticator, ListItem>((authenticator) => {
            var result = authenticator.GetResult();
            return new() {
                Title = authenticator.Name,
                Subtitle = string.Format(Resource.expired_in, result.Remain),
                Icon = new IconInfo("\uE8C8"),
                Command = new AnonymousCommand(() => {
                    // Ensure the code is latest
                    new CopyTextCommand(authenticator.GetResult().Code).Invoke();
                }) {
                    Name = Resource.copy_to_cilpboard_command,
                    Icon = new IconInfo("\uE8C8")
                },
                MoreCommands = [
                    new CommandContextItem(new NoOpCommand()) {
                        Title = Resource.totp_rename_title,
                        Icon = new IconInfo("\uE8AC")
                    },
                    new CommandContextItem(
                        new ConfirmableCommand {
                            Command = new AnonymousCommand(() => {
                                AuthList.Remove(authenticator);
                                ConfigManager.Save();
                                new ToastStatusMessage(new StatusMessage {
                                    Message = string.Format(Resource.totp_delete_done, authenticator.Name),
                                    State = MessageState.Success
                                }).Show();
                            }) { Result = CommandResult.KeepOpen(), Name = Resource.totp_delete_confirm },
                            ConfirmationTitle = Resource.totp_delete_title,
                            ConfirmationMessage = string.Format(Resource.totp_delete_description, authenticator.Name)
                        }
                    ) {
                        Title = Resource.totp_delete_title,
                        IsCritical = true,
                        Icon = new IconInfo("\uE74D")
                    }
                ],
                Tags = [
                    new Tag {
                        Text = result.Code.ToString()
                    }
                ]
            };
        })];
    }
}