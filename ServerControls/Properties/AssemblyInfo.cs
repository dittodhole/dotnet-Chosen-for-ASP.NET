using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.UI;
using ServerControls;
using ServerControls.Resources.jquery;
using ServerControls.Resources.jquery.plugins.chosen;

[assembly: AssemblyTitle("ServerControls")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("ServerControls")]
[assembly: AssemblyCopyright("Copyright © Andreas Niedermair")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("b271cdf0-4bf5-457f-b27c-2fe55ddb2300")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: WebResource(JQuery.ScriptPath, Constants.ContentTypeJavaScript)]
[assembly: WebResource(Chosen.ScriptPath, Constants.ContentTypeJavaScript)]
[assembly: WebResource(Chosen.StylePath, Constants.ContentTypeStylesheet, PerformSubstitution = true)]
[assembly: WebResource(Chosen.SpritePath, Constants.ContentTypePng)]
