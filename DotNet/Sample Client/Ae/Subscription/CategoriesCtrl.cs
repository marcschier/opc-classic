//============================================================================
// TITLE: CategoriesCtrl.cs
//
// CONTENTS:
// 
// A control used to pick the category filers for a subscription. 
//
// (c) Copyright 2003-2004 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2004/11/16 RSA   Initial implementation.

using System;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Opc.Ae;
using Opc.SampleClient;

namespace Opc.Ae.SampleClient
{
	/// <summary>
	/// A control used to edit the state of a subscription.
	/// </summary>
	public class CategoriesCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ListView CategoriesLV;
		private System.Windows.Forms.GroupBox CategoriesGB;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public CategoriesCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			AddHeader(CategoriesLV, "Name");
			AddHeader(CategoriesLV, "Event Type");
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CategoriesLV = new System.Windows.Forms.ListView();
			this.CategoriesGB = new System.Windows.Forms.GroupBox();
			this.CategoriesGB.SuspendLayout();
			this.SuspendLayout();
			// 
			// CategoriesLV
			// 
			this.CategoriesLV.CheckBoxes = true;
			this.CategoriesLV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CategoriesLV.FullRowSelect = true;
			this.CategoriesLV.Location = new System.Drawing.Point(3, 16);
			this.CategoriesLV.Name = "CategoriesLV";
			this.CategoriesLV.Size = new System.Drawing.Size(370, 181);
			this.CategoriesLV.TabIndex = 16;
			this.CategoriesLV.View = System.Windows.Forms.View.Details;
			this.CategoriesLV.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CategoriesLV_ItemCheck);
			// 
			// CategoriesGB
			// 
			this.CategoriesGB.Controls.Add(this.CategoriesLV);
			this.CategoriesGB.Dock = System.Windows.Forms.DockStyle.Top;
			this.CategoriesGB.Location = new System.Drawing.Point(0, 0);
			this.CategoriesGB.Name = "CategoriesGB";
			this.CategoriesGB.Size = new System.Drawing.Size(376, 200);
			this.CategoriesGB.TabIndex = 17;
			this.CategoriesGB.TabStop = false;
			this.CategoriesGB.Text = "Categories";
			// 
			// CategoriesCtrl
			// 
			this.Controls.Add(this.CategoriesGB);
			this.Name = "CategoriesCtrl";
			this.Size = new System.Drawing.Size(376, 200);
			this.CategoriesGB.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Members
		private Opc.Ae.Server m_server = null;
		private event CategoryCheckedEventHandler m_CategoryChecked = null;
		#endregion

		#region Public Interface
		/// <summary>
		/// Raised when a category is checked.
		/// </summary>
		public event CategoryCheckedEventHandler CategoryChecked
		{
			add    { m_CategoryChecked += value; }
			remove { m_CategoryChecked += value; }
		}

		/// <summary>
		/// A delegate to receive notifications when categories are selected.
		/// </summary>
		public delegate void CategoryCheckedEventHandler(int categoryID, bool picked);

		/// <summary>
		/// Shows the available categories in the control.
		/// </summary>
		public void ShowCategories(Opc.Ae.Server server)
		{
			if (server == null) throw new ArgumentNullException("server");

			m_server = server;
 
			ShowAvailableCategories();
		}

		/// <summary>
		/// Returns the currently selected categories.
		/// </summary>
		public int[] GetSelectedCategories()
		{
			ArrayList categories = new ArrayList();

			foreach (ListViewItem item in CategoriesLV.Items)
			{
				if (item.Checked)
				{
					categories.Add(((Category)item.Tag).ID);
				}				
			}

			return (int[])categories.ToArray(typeof(int));
		}

		/// <summary>
		/// Sets the currently selected categories.
		/// </summary>
		public void SetSelectedCategories(int[] categoryIDs)
		{
			foreach (ListViewItem item in CategoriesLV.Items)
			{
				item.Checked = false;

				Category category = (Category)item.Tag;

				for (int ii = 0; ii < categoryIDs.Length; ii++)
				{
					if (categoryIDs[ii] == category.ID)
					{
						item.Checked = true;
						break;
					}
				}		
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Displays all available categories in the list view.
		/// </summary>
		private void ShowAvailableCategories()
		{
			CategoriesLV.Items.Clear();

			ShowAvailableCategories(EventType.Simple);
			ShowAvailableCategories(EventType.Tracking);
			ShowAvailableCategories(EventType.Condition);

			CategoriesLV.Sorting = SortOrder.Ascending;
			CategoriesLV.Sort();

			AdjustColumns(CategoriesLV);
		}

		/// <summary>
		/// Displays the categories for the specified event type.
		/// </summary>
		private void ShowAvailableCategories(EventType eventType)
		{
			try
			{
				Category[] categories = m_server.QueryEventCategories((int)eventType);

				foreach (Category category in categories)
				{
					ListViewItem item = new ListViewItem(category.Name);
					
					item.SubItems.Add(eventType.ToString());
					item.Tag = category;

					CategoriesLV.Items.Add(item);
				}
			}
			catch
			{
				// ignore errors.
			}
		}

		/// <summary>
		/// Populates the list box with the categories.
		/// </summary>
		private void AddHeader(ListView listview, string name)
		{
			ColumnHeader header = new ColumnHeader();
			header.Text = name;
			listview.Columns.Add(header);
		}

		/// <summary>
		/// Adjusts the columns shown in the list view.
		/// </summary>
		private void AdjustColumns(ListView listview)
		{
			// adjust column widths.
			for (int ii = 0; ii < listview.Columns.Count; ii++)
			{
				listview.Columns[ii].Width = -2;
			}
		}
		#endregion
		
		#region Event Handlers
		/// <summary>
		/// Raises a category selected event.
		/// </summary>
		private void CategoriesLV_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
		{
			if (m_CategoryChecked != null)
			{
				try
				{
					ListViewItem item = CategoriesLV.Items[e.Index];

					if (typeof(Category).IsInstanceOfType(item.Tag))
					{
						m_CategoryChecked(((Category)item.Tag).ID, (e.NewValue == CheckState.Checked));
					}
				}
				catch
				{
					// do nothing.
				}
			}		
		}
		#endregion
	}
}
