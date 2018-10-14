using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("GalleryServer.Business")]
[assembly: AssemblyDescription("This class library contains business layer functionality for supporting Gallery Server, a web application for sharing photos, videos, audio files, and other media assets over the web.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tech Info Systems, LLC")]
[assembly: AssemblyProduct("Gallery Server")]
[assembly: AssemblyCopyright("Copyright © Tech Info Systems, LLC 2018")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: CLSCompliant(false)] // Logic for EXIF properties uses unsigned variables, which are not CLS compliant

[assembly: ComVisible(false)]

// Define the locale that maps to the default resources. When a browser specifies the given 
// locale, this attribute is used to go directly to the Invariant/default resource. This
// provides a small performance improvement. Do not change this unless the default resource
// contents are changed to represent a different locale.
[assembly: System.Resources.NeutralResourcesLanguage("en-US")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f0680b85-97fc-4cd9-8468-a32779db5a83")]

[assembly: AssemblyVersion("4.4.3.*")]
[assembly: System.Resources.SatelliteContractVersion("1.0.0.0")]
