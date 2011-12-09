using System.Diagnostics.Contracts;
using System.Web.UI;

namespace ServerControls.ExtensionMethods
{
	public static class PageExtensionMethods
	{
		public static void RegisterJQuery(this Page page)
		{
			Contract.Requires(page != null);

			// TODO
		}

		public static void RegisterChosen(this Page page)
		{
			Contract.Requires(page != null);

			page.RegisterJQuery();
			
			// TODO
		}
	}
}
