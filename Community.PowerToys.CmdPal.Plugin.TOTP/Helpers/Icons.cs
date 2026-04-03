using Microsoft.CommandPalette.Extensions.Toolkit;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.Helpers {
    internal static class Icons {
        public static IconInfo MainIcon = IconHelpers.FromRelativePath("Assets\\icon.png");
        public static IconInfo AddIcon = IconHelpers.FromRelativePath("Assets\\add.png");
        public static IconInfo Copy = new("\uE8C8");
        public static IconInfo Rename = new("\uE8AC");
        public static IconInfo Delete = new("\uE74D");
        public static IconInfo QRCode = new("\uED14");
        public static IconInfo Link = new("\uE71B");
        public static IconInfo OpenIn = new("\uE8E5");
        public static IconInfo Warning = new("\uE7BA");

    }
}
