﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Aspenlaub.Net.GitHub.CSharp.TashHost.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Aspenlaub.Net.GitHub.CSharp.TashHost.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
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
        ///   Looks up a localized string similar to Attempt to confirm status to Tash from a non-UI thread.
        /// </summary>
        internal static string ConfirmationToTashNotFromUiThread {
            get {
                return ResourceManager.GetString("ConfirmationToTashNotFromUiThread", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not confirm status to Tash (error code: {0}).
        /// </summary>
        internal static string CouldNotConfirmStatusToTash {
            get {
                return ResourceManager.GetString("CouldNotConfirmStatusToTash", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not connect to Tash.
        /// </summary>
        internal static string CouldNotConnectToTash {
            get {
                return ResourceManager.GetString("CouldNotConnectToTash", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not make Tash registration (error code: {0}).
        /// </summary>
        internal static string CouldNotMakeTashRegistration {
            get {
                return ResourceManager.GetString("CouldNotMakeTashRegistration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tash Host.
        /// </summary>
        internal static string TashHostWindowTitle {
            get {
                return ResourceManager.GetString("TashHostWindowTitle", resourceCulture);
            }
        }
    }
}
