using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages {
    internal partial class ExceptionHandler(Exception ex, Action onVisibile): ContentPage {
        public override string Title => "Unhandled Exception";
        public override IContent[] GetContent() {
            onVisibile.Invoke();
            return [
                new MarkdownContent() {
                     Body = $"""
                     ```
                     {ex.Message}
                     {ex.StackTrace}
                     ```
                     """
                }
            ];
        }
    }
}
