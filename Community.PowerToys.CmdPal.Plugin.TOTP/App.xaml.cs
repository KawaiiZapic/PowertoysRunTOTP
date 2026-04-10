using Microsoft.UI.Xaml;

namespace Community.PowerToys.CmdPal.Plugin.TOTP {
    public partial class App: Application {
        private Window? _window;
        public App() {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            _window = new UI.MainWindow();
            _window.Activate();
        }
    }
}
