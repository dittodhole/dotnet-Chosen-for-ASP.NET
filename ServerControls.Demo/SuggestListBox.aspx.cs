using System;
using System.Web.UI;
using ServerControls.ExtensionMethods;

namespace ServerControls.Demo
{
	public partial class SuggestListBox : Page
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (!this.IsPostBack)
			{
				this.DemoSuggestListBox.DataSource = new[]
				{
					new Foo
					{
						ID = 1,
						Text = "Foo1"
					}, new Foo
					{
						ID = 2,
						Text = "Foo2"
					}, new Foo
					{
						ID = 3,
						Text = "Foo3"
					}
				};
				this.DemoSuggestListBox.DetermineEnabledStateOfListItem += element =>
				{
					var foo = (Foo) element;
					return foo.ID < 2;
				};
				this.DemoSuggestListBox.DataBind();

				this.DemoSuggestListBox.SetSelectedValue(3);
			}
		}

		#region Nested type: Foo

		public sealed class Foo
		{
			public int ID { get; set; }
			public string Text { get; set; }
		}

		#endregion
	}
}
