using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Genesis.QRCodeLib;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Zapic.PowerToys.TOTP.Core;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages {
    internal partial class Management {

        private static readonly QRDecoder decoder = new();

        static List<string> GetSetupURLFromScreen() {
            IntPtr hdc = Graphics.FromHwnd(NativeMethods.GetForegroundWindow()).GetHdc();
            var size = new Size {
                Width = NativeMethods.GetDeviceCaps(hdc, 118),
                Height = NativeMethods.GetDeviceCaps(hdc, 117)
            };
            var screenBitmap = new Bitmap(size.Width, size.Height);
            using (var g = Graphics.FromImage(screenBitmap)) {
                g.CopyFromScreen(Point.Empty, Point.Empty, size);
            }
            try {
                var data = decoder.ImageDecoder(screenBitmap);
                if (data != null) {
                    return [
                    .. data
                        .Select(record => QRCode.ByteArrayToStr(record))
                        .Where(link => link != null && link.StartsWith("otpauth://totp/"))];
                }
                return [];
            } catch {
                return [];
            }
        }

        static ICommand ScanQRCodeCommand => new AnonymousCommand(() => {
            Task.Run(async () => {
                var hwnd = NativeMethods.GetForegroundWindow();
                NativeMethods.ShowWindow(hwnd, 0);
                List<string> result = null!;
                try {
                    result = GetSetupURLFromScreen();
                } finally {
                    NativeMethods.ShowWindow(hwnd, 5);
                }
                if (result.Count > 0) {
                    result.ForEach(item => {
                        AuthList.Add(Core.ParseLink(item));
                    });
                    ConfigManager.Save();
                    new ToastStatusMessage(new StatusMessage {
                        Message = result.Count switch {
                            1 => string.Format(Resource.scan_from_screen_done_one, Core.ParseLink(result[0]).Name),
                            _ => string.Format(Resource.scan_from_screen_done_two_more, result.Count)
                        },
                        State = MessageState.Success
                    }).Show();
                } else {
                    new ToastStatusMessage(new StatusMessage {
                        Message = Resource.scan_from_screen_empty,
                        State = MessageState.Info
                    }).Show();
                }
            });
        }) {
            Name = Resource.import_command,
            Result = CommandResult.KeepOpen()
        };

        static ICommand ImportFromPTRunCommand => new AnonymousCommand(() => {
            try {
                var filePath = Path.Combine(
                    Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%"),
                    "Microsoft\\PowerToys\\PowerToys Run\\Settings\\Plugins\\Community.PowerToys.Run.Plugin.TOTP",
                    "AuthenticatorsList.json"
                );
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
        };
    }
}
