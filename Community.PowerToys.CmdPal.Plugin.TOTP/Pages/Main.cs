// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Helpers;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Microsoft.CmdPal.Common.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages;

internal sealed partial class Main: ListPage {
    public override IconInfo Icon => Icons.MainIcon;
    public override string Title => Resource.page_totp_name;
    public override string Name => Resource.page_totp_name;
    public override string Id => typeof(Main).FullName!;
    private static List<Authenticator> AuthList {
        get => ConfigManager.Data.Authenticators;
    }

    public Main() {
        ConfigManager.OnDataChanged += () => {
            RaiseItemsChanged();
        };
    }

    static readonly ListItem EmptyListNotice = new() {
        Title = Resource.no_authenticator,
        Subtitle = Resource.no_authenticator_tip,
        Command = new Management()
    };

    static ConfirmableCommand ConfirmDeleteCommand(Authenticator a) {
        return new ConfirmableCommand {
            Command = new AnonymousCommand(() => {
                AuthList.Remove(a);
                ConfigManager.Save();
                new ToastStatusMessage(new StatusMessage {
                    Message = string.Format(Resource.totp_delete_done, a.Name),
                    State = MessageState.Success
                }).Show();
            }) { Result = CommandResult.KeepOpen(), Name = Resource.totp_delete_confirm },
            ConfirmationTitle = Resource.totp_delete_title,
            ConfirmationMessage = string.Format(Resource.totp_delete_description, a.Name)
        };
    }

    public override IListItem[] GetItems() {
        if (AuthList.Count == 0) {
            return [EmptyListNotice];
        }
        return [.. AuthList.Select<Authenticator, ListItem>((authenticator) => {
            var result = authenticator.GetResult();
            return new() {
                Title = authenticator.Name,
                Subtitle = string.Format(Resource.expired_in, result.Remain),
                Icon = Icons.Copy,
                Command = new AnonymousCommand(() => {
                    // Ensure the code is latest
                    ClipboardHelper.SetText(authenticator.GetResult().Code);
                }) {
                    Name = Resource.copy_to_cilpboard_command,
                    Icon = Icons.Copy
                },
                MoreCommands = [
                    new CommandContextItem(new Rename(authenticator)) {
                        Title = Resource.totp_rename_title,
                        Icon = Icons.Rename
                    },
                    new CommandContextItem(
                        ConfirmDeleteCommand(authenticator)
                    ) {
                        Title = Resource.totp_delete_title,
                        IsCritical = true,
                        Icon = Icons.Delete
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