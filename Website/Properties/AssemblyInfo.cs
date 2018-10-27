using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("GalleryServer.Web")]
[assembly: AssemblyDescription("Gallery Server is a web application using for sharing photos, videos, audio files, and other media assets.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tech Info Systems, LLC")]
[assembly: AssemblyProduct("Gallery Server")]
[assembly: AssemblyCopyright("Copyright © Tech Info Systems, LLC 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Assembly is not CLS compliant because there are web pages whose code behind class is
// named _default. Apparently the underscore is not allowed.
[assembly: CLSCompliant(false)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Define the locale that maps to the default resources. When a browser specifies the given 
// locale, this attribute is used to go directly to the Invariant/default resource. This
// provides a small performance improvement. Do not change this unless the default resource
// contents are changed to represent a different locale.
[assembly: System.Resources.NeutralResourcesLanguage("en-US")]

[assembly: System.Web.UI.WebResource("GalleryServer.Web.App_GlobalResources.gs-ftr-logo.png", "image/png")]

[assembly: AssemblyVersion("4.5.0.*")]
[assembly: System.Resources.SatelliteContractVersion("1.0.0.0")]
