using Microsoft.UI.Xaml;

namespace Community.PowerToys.CmdPal.Plugin.TOTP.UI {
    public sealed partial class MainWindow: Window {
        public MainWindow() {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }
    }
}
