﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Community.PowerToys.Run.Plugin.TOTP.localization {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Community.PowerToys.Run.Plugin.TOTP.localization.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Add {0} authenticators 的本地化字符串。
        /// </summary>
        internal static string add_from_ga {
            get {
                return ResourceManager.GetString("add_from_ga", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 From Google Authenticator App, batch {0} / {1} 的本地化字符串。
        /// </summary>
        internal static string add_from_ga_tip {
            get {
                return ResourceManager.GetString("add_from_ga_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Add new authenticator 的本地化字符串。
        /// </summary>
        internal static string add_from_otpauth_tip {
            get {
                return ResourceManager.GetString("add_from_otpauth_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0} - {1} 的本地化字符串。
        /// </summary>
        internal static string copy_to_clipboard {
            get {
                return ResourceManager.GetString("copy_to_clipboard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Something is using clipboard, code can&apos;t be copied, please try again 的本地化字符串。
        /// </summary>
        internal static string copy_to_clipboard_err {
            get {
                return ResourceManager.GetString("copy_to_clipboard_err", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Copy to clipboard - Expired in {0}s 的本地化字符串。
        /// </summary>
        internal static string copy_to_clipboard_tip {
            get {
                return ResourceManager.GetString("copy_to_clipboard_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Invalid otpauth-migration link 的本地化字符串。
        /// </summary>
        internal static string invalid_ga_import_link {
            get {
                return ResourceManager.GetString("invalid_ga_import_link", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to scan QRCode and copy it again 的本地化字符串。
        /// </summary>
        internal static string invalid_ga_import_link_tip {
            get {
                return ResourceManager.GetString("invalid_ga_import_link_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Invalid otpauth link 的本地化字符串。
        /// </summary>
        internal static string invalid_otpauth_link {
            get {
                return ResourceManager.GetString("invalid_otpauth_link", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Try to scan QRCode and copy it again 的本地化字符串。
        /// </summary>
        internal static string invalid_otpauth_link_tip {
            get {
                return ResourceManager.GetString("invalid_otpauth_link_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0} - Invalid authenticator 的本地化字符串。
        /// </summary>
        internal static string invalid_secret {
            get {
                return ResourceManager.GetString("invalid_secret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 This authenticator contains a invalid secret that can&apos;t be decode as base32 data 的本地化字符串。
        /// </summary>
        internal static string invalid_secret_tip {
            get {
                return ResourceManager.GetString("invalid_secret_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 No authenticator 的本地化字符串。
        /// </summary>
        internal static string no_authenticator {
            get {
                return ResourceManager.GetString("no_authenticator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Add your first authenticator by paste your setup link(otpauth://) or Google Authenticator(otpauth-migration://) 的本地化字符串。
        /// </summary>
        internal static string no_authenticator_tip {
            get {
                return ResourceManager.GetString("no_authenticator_tip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Copy Time-based one time password to clipboard 的本地化字符串。
        /// </summary>
        internal static string plugin_description {
            get {
                return ResourceManager.GetString("plugin_description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 TOTP 的本地化字符串。
        /// </summary>
        internal static string plugin_name {
            get {
                return ResourceManager.GetString("plugin_name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Delete {0}? Please make sure you have disable 2FA verify on the website, otherwise you will be no longer able to access your account.
        ///Type &quot;DELETE&quot; to confrim delete. 的本地化字符串。
        /// </summary>
        internal static string totp_delete_description {
            get {
                return ResourceManager.GetString("totp_delete_description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Authenticator {0} has been deleted. 的本地化字符串。
        /// </summary>
        internal static string totp_delete_done {
            get {
                return ResourceManager.GetString("totp_delete_done", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Delete authenticator 的本地化字符串。
        /// </summary>
        internal static string totp_delete_title {
            get {
                return ResourceManager.GetString("totp_delete_title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Rename {0} to: 的本地化字符串。
        /// </summary>
        internal static string totp_rename_description {
            get {
                return ResourceManager.GetString("totp_rename_description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Rename authenticator 的本地化字符串。
        /// </summary>
        internal static string totp_rename_title {
            get {
                return ResourceManager.GetString("totp_rename_title", resourceCulture);
            }
        }
    }
}
