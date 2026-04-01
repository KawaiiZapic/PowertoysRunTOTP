using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace Zapic.PowerToys.TOTP.Core.Data {

    public struct TotpResult {
        public string Code = "";
        public int Remain = 0;

        public TotpResult() { }
    }

    public class Authenticator {
        public string Name { get; set; } = "";
        public string Key { get; set; } = "";
        public bool IsEncrypted { get; set; } = false;

        private Totp? _otp;

        public string GetUnencryptKey() {
            if (!IsEncrypted)
                return Key;
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Key), null, DataProtectionScope.CurrentUser));
        }

        public void SetUnencrypted(string unencrypted) {
            Key = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(unencrypted), null, DataProtectionScope.CurrentUser));
            IsEncrypted = true;
            _otp = null;
        }

        public void ForceEncypt() {
            if (IsEncrypted)
                return;
            SetUnencrypted(Key);
        }

        public TotpResult GetResult() {
            _otp ??= new Totp(Encoding.UTF8.GetBytes(GetUnencryptKey()));
            return new TotpResult {
                Code = _otp.ComputeTotp(),
                Remain = _otp.RemainingSeconds()
            };
        }
    }

    public class AuthenticatorsList {
        public int Version { get; set; }
        public List<Authenticator> Authenticators { get; set; } = new();
    }
}
