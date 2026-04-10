// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace Community.PowerToys.CmdPal.Plugin.TOTP;

public class Program {
    [MTAThread]
    public static void Main(string[] args) {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer") {
            ComServer server = new();

            ManualResetEvent extensionDisposedEvent = new(false);

            // We are instantiating an extension instance once above, and returning it every time the callback in RegisterExtension below is called.
            // This makes sure that only one instance of SampleExtension is alive, which is returned every time the host asks for the IExtension object.
            // If you want to instantiate a new instance each time the host asks, create the new instance inside the delegate.
            Extension extensionInstance = new(extensionDisposedEvent);
            server.RegisterClass<Extension, IExtension>(() => extensionInstance);
            server.Start();

            // This will make the main thread wait until the event is signalled by the extension class.
            // Since we have single instance of the extension object, we exit as soon as it is disposed.
            extensionDisposedEvent.WaitOne();
            server.Stop();
            server.UnsafeDispose();
        } else {
            Task.Run(LaunchUI).Wait();
        }
    }

    [GeneratedCodeAttribute("Microsoft.UI.Xaml.Markup.Compiler", " 3.0.0.2602")]
    [DebuggerNonUserCodeAttribute()]
    [STAThread]
    static void LaunchUI() {
        NativeMethods.XamlCheckProcessRequirements();

        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start((p) => {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            new App();
        });
    }
}
