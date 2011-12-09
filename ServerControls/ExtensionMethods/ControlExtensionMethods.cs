using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ServerControls.Resources.jquery;
using ServerControls.Resources.jquery.plugins.chosen;

namespace ServerControls.ExtensionMethods
{
	public static class ControlExtensionMethods
	{
		private static readonly Type TypeOfControlExtensionMethods = typeof (ControlExtensionMethods);

		public static void RegisterJQuery<TControl>(this TControl control)
			where TControl : Control
		{
			control.RegisterScriptWebResource("jquery", JQuery.ScriptPath);
		}

		public static void RegisterChosen<TControl>(this TControl control)
			where TControl : Control
		{
			control.RegisterScriptWebResource("chosen", Chosen.ScriptPath);
			control.RegisterStyleWebResource(Chosen.StylePath);
		}

		internal static void RegisterScriptWebResource<TControl>(this TControl control, string key, string path)
			where TControl : Control
		{
			Contract.Requires(control != null);
			Contract.Requires(!String.IsNullOrEmpty(key));
			Contract.Requires(!String.IsNullOrEmpty(path));

			var page = control.Page;
			var clientScriptManager = page.ClientScript;
			if (clientScriptManager.IsClientScriptIncludeRegistered(key))
			{
				return;
			}

			var webResourcePath = page.GetWebResourcePath(path);
			clientScriptManager.RegisterClientScriptInclude(key, webResourcePath);
		}

		internal static void RegisterStyleWebResource<TControl>(this TControl control, params string[] paths)
			where TControl : Control
		{
			Contract.Requires(control != null);
			Contract.Requires(paths != null);

			var page = control.Page;
			var header = page.Header;
			if (header == null)
			{
				return;
			}
			var htmlLinkControls = header.Controls.OfType<HtmlLink>().ToList();

			foreach (var path in paths)
			{
				var webResourcePath = page.GetWebResourcePath(path);
				if (htmlLinkControls.Any(htmlLinkControl => string.Equals(htmlLinkControl.Href, webResourcePath)))
				{
					continue;
				}

				var cssLink = new HtmlLink();
				cssLink.Attributes.Add("href", webResourcePath);
				cssLink.Attributes.Add("type", Constants.ContentTypeStylesheet);
				cssLink.Attributes.Add("rel", "stylesheet");
				header.Controls.Add(cssLink);
			}
		}

		internal static string GetWebResourcePath(this Page page, string path)
		{
			Contract.Requires(page != null);
			Contract.Requires(!String.IsNullOrEmpty(path));

			Contract.Ensures(!String.IsNullOrEmpty(Contract.Result<string>()));

			var clientScriptManager = page.ClientScript;

			return clientScriptManager.GetWebResourceUrl(TypeOfControlExtensionMethods, path);
		}
	}
}
