using System.Text.Json.Nodes;
using Community.PowerToys.CmdPal.Plugin.TOTP.DataManager;
using Community.PowerToys.CmdPal.Plugin.TOTP.Localization;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Zapic.PowerToys.TOTP.Core.Data;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Pages {
    partial class RenameForm: FormContent {
        private readonly Authenticator auth;

        public RenameForm(Authenticator auth) {
            TemplateJson = Helpers.TemplateJson.Replace("""
{
    "type": "AdaptiveCard",
    "$schema": "https://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.6",
    "body": [
        {
            "type": "TextBlock",
            "text": "${rename_title}",
            "style": "heading",
            "size": "ExtraLarge",
            "wrap": true
        },
        {
            "type": "Input.Text",
            "label": "${rename_input_title}",
            "placeholder": "${original_name}",
            "id": "rename_input"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "style": "positive",
            "title": "${rename_submit}"
        }
    ]
}
""",
            new() {
                { "rename_title", Resource.totp_rename_title },
                { "rename_input_title", string.Format(Resource.totp_rename_description, auth.Name) },
                { "rename_submit", Resource.totp_rename_confirm },
                { "original_name", auth.Name }
            });
            this.auth = auth;
        }
        public override ICommandResult SubmitForm(string inputs) {
            var result = JsonNode.Parse(inputs)?.AsObject();
            if (result is null)
                return CommandResult.GoBack();
            var newName = result["rename_input"]?.AsValue().GetValue<string>();
            if (!string.IsNullOrEmpty(newName)) {
                auth.Name = newName;
                ConfigManager.Save();
            }
            return CommandResult.GoBack();
        }
    }
    internal partial class Rename(Authenticator auth): ContentPage {
        public override IContent[] GetContent() {
            return [new RenameForm(auth)];
        }
    }
}
