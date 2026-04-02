// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Community.PowerToys.CmdPal.Plugin.TOTP;

[Guid("ea1ff7cd-e5a5-45cb-bffd-de16c34bf220")]
public sealed partial class Extension(ManualResetEvent extensionDisposedEvent): IExtension, IDisposable {
    private readonly CommandsProvider _provider = new();

    public object? GetProvider(ProviderType providerType) {
        return providerType switch {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose() => extensionDisposedEvent.Set();
}

public partial class CommandsProvider: CommandProvider {
    private readonly ICommandItem[] _commands;

    public CommandsProvider() {
        DisplayName = "TOTP";
        Icon = IconHelpers.FromRelativePath("Assets\\icon.png");
        _commands = [
            new CommandItem(new Pages.Main()) { Title = DisplayName }
        ];
    }

    public override ICommandItem[] TopLevelCommands() {
        return _commands;
    }

}
