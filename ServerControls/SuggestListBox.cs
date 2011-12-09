using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ServerControls.ExtensionMethods;

namespace ServerControls
{
	public sealed class SuggestListBox : ListBox, INamingContainer
	{
		public static string DefaultNoElementsText = "Es wurden keine Elemente gefunden: "; // yep, I know - somehow impelment sth like translatable ...

		public SuggestListBox()
		{
			this.Width = new Unit(80, UnitType.Percentage);
			this.AppendDataBoundItems = true; // have a look @ line 328
		}

		#region viewStateKeys

		private const string ViewStateKeyDefaultElementText = "DefaultElementText";
		private const string ViewStateKeyNoResultsText = "NoResultsText";
		private const string ViewStateKeyHideMandatoryIcon = "HideMandatoryIcon";

		#endregion

		#region childControls

		private readonly CustomValidator _mandatoryValidator = new CustomValidator
		{
			ID = "mV",
			Display = ValidatorDisplay.None,
			EnableClientScript = false,
			Enabled = false,
			ErrorMessage = "ErrorMessageForMandatoryFailed",
			SetFocusOnError = true
		}; // I do not want to output the validator directly - I (and you) will need a System.Web.UI.WebControls.ValidationSummary ...

		#endregion

		private IEnumerable<ListItem> ListItems
		{
			get
			{
				// as .Items are of type ListItemCollection, and this class only implements System.Collections.IEnumerable,
				// we have to do some .OfType<T>() here ... ugly but helpful
				return this.Items.OfType<ListItem>();
			}
		}

		private IEnumerable<BaseValidator> Validators
		{
			get
			{
				// I am planning to support more implicit validators - I will add them someday
				yield return this._mandatoryValidator;
			}
		}

		public string DefaultElementText
		{
			get
			{
				return this.ViewState[ViewStateKeyDefaultElementText] as string;
			}
			set
			{
				this.ViewState[ViewStateKeyDefaultElementText] = value;
			}
		}

		public string NoResultsText
		{
			get
			{
				return (this.ViewState[ViewStateKeyNoResultsText] as string) ?? DefaultNoElementsText;
			}
			set
			{
				this.ViewState[ViewStateKeyNoResultsText] = value;
			}
		}

		public bool IsMandatory
		{
			get
			{
				this.EnsureChildControls();
				return this._mandatoryValidator.Enabled;
			}
			set
			{
				this.EnsureChildControls();
				this._mandatoryValidator.Enabled = value;
			}
		}

		public bool HideMandatoryIcon
		{
			// you could take advantage of this property by checking it when you render an icon ...
			get
			{
				var obj = this.ViewState[ViewStateKeyHideMandatoryIcon];
				if (obj == null)
				{
					return false;
				}
				return (bool) obj;
			}
			set
			{
				this.ViewState[ViewStateKeyHideMandatoryIcon] = value;
			}
		}

		public string ErrorMessageForMandatoryFailed
		{
			get
			{
				this.EnsureChildControls();
				return this._mandatoryValidator.ErrorMessage;
			}
			set
			{
				this.EnsureChildControls();
				this._mandatoryValidator.ErrorMessage = value;
			}
		}

		public void ValidateMandatory(object source, ServerValidateEventArgs args)
		{
			var customValidator = (CustomValidator) source;
			var suggestDropDownList = (SuggestListBox) customValidator.Parent;

			switch (suggestDropDownList.SelectionMode)
			{
				case ListSelectionMode.Single:
				{
					var selectedValue = suggestDropDownList.GetSelectedValue<string>();
					args.IsValid = !string.IsNullOrEmpty(selectedValue);
				}
					break;
				case ListSelectionMode.Multiple:
				{
					var selectedValues = suggestDropDownList.GetSelectedValues<string>();
					args.IsValid = (from selectedValue in selectedValues
					                where !string.IsNullOrEmpty(selectedValue)
					                select selectedValue).Any();
				}
					break;
				default:
					throw new HttpException();
			}
		}

		public event Func<object, bool> DetermineEnabledStateOfListItem;

		private bool OnDetermineEnabledStateOfListItem(object element)
		{
			var eventHandler = this.DetermineEnabledStateOfListItem;
			if (eventHandler == null)
			{
				return true;
			}
			return eventHandler(element);
		}

		#region data binding

		protected override void PerformDataBinding(IEnumerable dataSource)
		{
			base.PerformDataBinding(dataSource);

			// TODO find some better way for injection ... fear there ain't any ...

			foreach (var element in dataSource)
			{
				string elementValue;

				var dataValueField = this.DataValueField;
				if (string.IsNullOrEmpty(dataValueField))
				{
					elementValue = element.ToString();
				}
				else
				{
					elementValue = DataBinder.GetPropertyValue(element, dataValueField, null);
				}

				var matchingListItem = (from listItem in this.ListItems
				                        let listItemValue = listItem.Value
				                        where string.Equals(listItemValue, elementValue, StringComparison.CurrentCultureIgnoreCase)
				                        select listItem).FirstOrDefault();
				if (matchingListItem == null)
				{
					continue;
				}

				matchingListItem.Enabled = this.OnDetermineEnabledStateOfListItem(element);
			}
		}

		#endregion

		#region life cycle

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			this.RegisterJQuery();
			this.RegisterChosen();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();

			this.Controls.Add(this._mandatoryValidator);
			this._mandatoryValidator.ServerValidate += this.ValidateMandatory;
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			{
				// not the best place in here - but: it will not be persisted to the viewState
				var anyFailedValidators = (from baseValidator in this.Validators
				                           where !baseValidator.IsValid
				                           select baseValidator).Any();
				if (anyFailedValidators)
				{
					this.CssClass = string.Concat(this.CssClass, " ", Constants.ErrorClass);
				}
			}

			base.AddAttributesToRender(writer);

			{
				// we could also do some .data() in the client-script ...
				var defaultElementText = this.DefaultElementText;
				if (!string.IsNullOrEmpty(defaultElementText))
				{
					writer.AddAttribute("data-placeholder", defaultElementText);
				}
			}

			{
				// valid xhtml - against the original docu :)
				if (this.SelectionMode
				    == ListSelectionMode.Multiple)
				{
					writer.AddAttribute("multiple", "multiple");
				}
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			// there might be a better way, but this is just the basic registration - no need to opt this :)
			const string initializationScript = @"
<script type=""{0}"">
$(function () {{
	var $suggestListBox = $('#{1}');
	$suggestListBox.chosen({{
		'allow_single_deselect': true,
		'no_results_text': '{2}'
	}});
}});
</script>";

			var script = string.Format(initializationScript,
				Constants.ContentTypeJavaScript,
				this.ClientID,
				HttpUtility.HtmlEncode(this.NoResultsText));

			writer.WriteLine(script);

			base.RenderBeginTag(writer);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			// this is an adapted logic from the original implentation ...
			// take care of line 291 ... this does the magic with (in)active-elements
			var flag = false;
			foreach (var listItem in this.ListItems)
			{
				if (!listItem.Selected
				    && !listItem.Enabled)
				{
					continue;
				}

				writer.WriteBeginTag("option");
				if (listItem.Selected)
				{
					if (flag)
					{
						this.VerifyMultiSelect();
					}
					flag = true;
					writer.WriteAttribute("selected", "selected");
				}
				writer.WriteAttribute("value", listItem.Value, true);
				listItem.Attributes.Render(writer);
				if (this.Page != null)
				{
					this.Page.ClientScript.RegisterForEventValidation(this.UniqueID, listItem.Value);
				}
				writer.Write('>');
				HttpUtility.HtmlEncode(listItem.Text, writer);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
		}

		public override void RenderEndTag(HtmlTextWriter writer)
		{
			base.RenderEndTag(writer);

			// TODO
			//this.RenderMandatoryIcon(writer);
		}

		public override void DataBind()
		{
			// as chosen needs a plain option to render the magic "select something"

			this.Items.Clear();

			{
				var emptyItem = new ListItem();
				this.Items.Add(emptyItem);
			}

			base.DataBind();
		}

		#endregion
	}
}
