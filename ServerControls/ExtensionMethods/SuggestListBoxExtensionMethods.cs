using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace ServerControls.ExtensionMethods
{
	public static class SuggestListBoxExtensionMethods
	{
		public static T GetSelectedValue<T>(this SuggestListBox suggestListBox)
		{
			Contract.Requires(suggestListBox != null);

			if (suggestListBox.SelectionMode == ListSelectionMode.Multiple)
			{
				throw new HttpException();
			}

			var selectedItem = suggestListBox.SelectedItem;
			if (selectedItem == null)
			{
				return default(T);
			}
			if (string.IsNullOrEmpty(selectedItem.Value))
			{
				return default(T);
			}

			return (T) Convert.ChangeType(selectedItem.Value, typeof (T));
		}

		public static bool SetSelectedValue<T>(this SuggestListBox suggestListBox, T value)
		{
			Contract.Requires(suggestListBox != null);

			if (suggestListBox.SelectionMode == ListSelectionMode.Multiple)
			{
				throw new HttpException();
			}

			var stringValue = value.ToString();

			var listItem = (from item in suggestListBox.Items.OfType<ListItem>()
							where string.Equals(item.Value, stringValue)
							select item).FirstOrDefault();
			if (listItem == null)
			{
				return false;
			}

			suggestListBox.SelectedValue = listItem.Value;
			return true;
		}

		public static T GetSelectedText<T>(this SuggestListBox suggestListBox)
		{
			Contract.Requires(suggestListBox != null);

			if (suggestListBox.SelectionMode == ListSelectionMode.Multiple)
			{
				throw new HttpException();
			}

			var selectedItem = suggestListBox.SelectedItem;
			if (selectedItem == null)
			{
				return default(T);
			}
			if (string.IsNullOrEmpty(selectedItem.Text))
			{
				return default(T);
			}

			return (T) Convert.ChangeType(selectedItem.Text, typeof (T));
		}

		public static IEnumerable<T> GetSelectedValues<T>(this SuggestListBox suggestListBox)
		{
			Contract.Requires(suggestListBox != null);

			if (suggestListBox.SelectionMode == ListSelectionMode.Single)
			{
				throw new HttpException();
			}

			var indices = suggestListBox.GetSelectedIndices();
			if (!indices.Any())
			{
				yield break;
			}

			foreach (var index in indices)
			{
				var listItem = suggestListBox.Items[index];
				var listItemValue = listItem.Value;
				if (string.IsNullOrEmpty(listItemValue))
				{
					continue;
				}
				var listItemParsedValue = (T) Convert.ChangeType(listItemValue, typeof (T));
				yield return listItemParsedValue;
			}
		}

		public static bool SetSelectedValues<T>(this SuggestListBox suggestListBox, IEnumerable<T> values)
		{
			Contract.Requires(suggestListBox != null);
			Contract.Requires(values != null);

			if (suggestListBox.SelectionMode == ListSelectionMode.Single)
			{
				throw new HttpException();
			}

			suggestListBox.ClearSelection();

			foreach (var value in values)
			{
				var stringValue = value.ToString();
				var listItem = (from item in suggestListBox.Items.OfType<ListItem>()
								where string.Equals(item.Value, stringValue)
								select item).FirstOrDefault();
				if (listItem == null)
				{
					return false;
				}

				listItem.Selected = true;
			}

			return true;
		}

	}
}
