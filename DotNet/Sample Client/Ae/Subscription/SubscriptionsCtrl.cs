//============================================================================
// TITLE: SubscriptionsCtrl.cs
//
// CONTENTS:
// 
// A control used manage subscriptions with the servers.
//
// (c) Copyright 2002-2004 The OPC Foundation
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
// 2004/11/10 RSA   Initial implementation.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Opc;
using Opc.Ae;
using Opc.SampleClient;

namespace Opc.Ae.SampleClient
{
	/// <summary>
	/// A control used to select a valid value for any bit mask expressed as an enumeration.
	/// </summary>
	public class SubscriptionsCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ListView SubscriptionsLV;
		private System.Windows.Forms.ContextMenu PopupMenu;
		private System.Windows.Forms.MenuItem Separator01;
		private System.Windows.Forms.MenuItem AddSubscriptionMI;
		private System.Windows.Forms.MenuItem DeleteMI;
		private System.Windows.Forms.MenuItem ActiveMI;
		private System.Windows.Forms.MenuItem EditMI;
		private System.Windows.Forms.MenuItem Separator02;
		private System.Windows.Forms.MenuItem RefreshMI;
		private System.ComponentModel.IContainer components = null;

		public SubscriptionsCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			AddHeader(SubscriptionsLV, "Subscription");
			AddHeader(SubscriptionsLV, "Active");

			SubscriptionsLV.SmallImageList = Resources.Instance.ImageList;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
		
			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SubscriptionsLV = new System.Windows.Forms.ListView();
			this.PopupMenu = new System.Windows.Forms.ContextMenu();
			this.AddSubscriptionMI = new System.Windows.Forms.MenuItem();
			this.Separator01 = new System.Windows.Forms.MenuItem();
			this.EditMI = new System.Windows.Forms.MenuItem();
			this.ActiveMI = new System.Windows.Forms.MenuItem();
			this.DeleteMI = new System.Windows.Forms.MenuItem();
			this.Separator02 = new System.Windows.Forms.MenuItem();
			this.RefreshMI = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// SubscriptionsLV
			// 
			this.SubscriptionsLV.ContextMenu = this.PopupMenu;
			this.SubscriptionsLV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubscriptionsLV.FullRowSelect = true;
			this.SubscriptionsLV.Location = new System.Drawing.Point(0, 0);
			this.SubscriptionsLV.MultiSelect = false;
			this.SubscriptionsLV.Name = "SubscriptionsLV";
			this.SubscriptionsLV.Size = new System.Drawing.Size(400, 304);
			this.SubscriptionsLV.TabIndex = 0;
			this.SubscriptionsLV.View = System.Windows.Forms.View.Details;
			this.SubscriptionsLV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SubscriptionsLV_MouseDown);
			// 
			// PopupMenu
			// 
			this.PopupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.AddSubscriptionMI,
																					  this.Separator01,
																					  this.EditMI,
																					  this.ActiveMI,
																					  this.DeleteMI,
																					  this.Separator02,
																					  this.RefreshMI});
			// 
			// AddSubscriptionMI
			// 
			this.AddSubscriptionMI.Index = 0;
			this.AddSubscriptionMI.Text = "Add Subscription...";
			this.AddSubscriptionMI.Click += new System.EventHandler(this.AddSubscriptionMI_Click);
			// 
			// Separator01
			// 
			this.Separator01.Index = 1;
			this.Separator01.Text = "-";
			// 
			// EditMI
			// 
			this.EditMI.Index = 2;
			this.EditMI.Text = "Edit...";
			this.EditMI.Click += new System.EventHandler(this.EditMI_Click);
			// 
			// ActiveMI
			// 
			this.ActiveMI.Index = 3;
			this.ActiveMI.Text = "Active";
			this.ActiveMI.Click += new System.EventHandler(this.ActiveMI_Click);
			// 
			// DeleteMI
			// 
			this.DeleteMI.Index = 4;
			this.DeleteMI.Text = "Delete";
			this.DeleteMI.Click += new System.EventHandler(this.DeleteMI_Click);
			// 
			// Separator02
			// 
			this.Separator02.Index = 5;
			this.Separator02.Text = "-";
			// 
			// RefreshMI
			// 
			this.RefreshMI.Index = 6;
			this.RefreshMI.Text = "Refresh";
			this.RefreshMI.Click += new System.EventHandler(this.RefreshMI_Click);
			// 
			// SubscriptionsCtrl
			// 
			this.Controls.Add(this.SubscriptionsLV);
			this.Name = "SubscriptionsCtrl";
			this.Size = new System.Drawing.Size(400, 304);
			this.ResumeLayout(false);

		}
		#endregion
	
		#region Private Members
		private Opc.Ae.Server m_server = null;
		private event SubscriptionActionEventHandler m_SubscriptionAction = null;
		#endregion
		
		#region Public Interface
		/// <summary>
		/// Raised when a area or source node in the tree is selected.
		/// </summary>
		public event SubscriptionActionEventHandler SubscriptionAction
		{
			add    { m_SubscriptionAction += value; }
			remove { m_SubscriptionAction += value; }
		}

		/// <summary>
		/// A delegate to receive notifications when categories are selected.
		/// </summary>
		public delegate void SubscriptionActionEventHandler(Subscription subscription, bool deleted);

		/// <summary>
		/// Displays current subscriptions for the server.
		/// </summary>
		public void ShowSubscriptions(Opc.Ae.Server server)
		{
			m_server = server;

			SubscriptionsLV.Items.Clear();

			// nothing more to do if no server provided.
			if (m_server == null)
			{
				return;
			}

			// add subscriptions.
			foreach (Subscription subscription in m_server.Subscriptions)
			{
				Add(subscription);
				
				// send notifications.
				if (subscription.Active)
				{
					if (m_SubscriptionAction != null)
					{
						m_SubscriptionAction(subscription, false);
					}
				}
			}

			// adjust columns.
			AdjustColumns(SubscriptionsLV);
		}

		/// <summary>
		/// Prompts the user to create a new subscription.
		/// </summary>
		public void AddSubscription()
		{
			try
			{
				// show properties dialog.
				Opc.Ae.Subscription subscription = new SubscriptionEditDlg().ShowDialog(m_server, null);

				if (subscription == null)
				{
					return;
				}

				// add to list.
				Add(subscription);

				// adjust columns.
				AdjustColumns(SubscriptionsLV);

				// send notifications.
				if (subscription.Active)
				{
					if (m_SubscriptionAction != null)
					{
						m_SubscriptionAction(subscription, false);
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Adds a subscription to the list.
		/// </summary>
		private void Add(Subscription subscription)
		{			
			ListViewItem item = new ListViewItem(subscription.Name, Resources.IMAGE_ENVELOPE);

			item.SubItems.Add((subscription.Active)?"Yes":"No");
			item.Tag = subscription;

			SubscriptionsLV.Items.Add(item);
		}		
		
		/// <summary>
		/// Updates a subscription in the list.
		/// </summary>
		private void Update(ListViewItem item)
		{			
			Subscription subscription = (Subscription)item.Tag;

			item.Text             = subscription.Name;
			item.SubItems[1].Text = (subscription.Active)?"Yes":"No";
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
		/// Updates the state of context menus based on the current selection.
		/// </summary>
		private void SubscriptionsLV_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// ignore left button actions.
			if (e.Button != MouseButtons.Right)	return;

			// set defaults.
			AddSubscriptionMI.Enabled = true;
			EditMI.Enabled            = false;
			ActiveMI.Enabled          = false;
			DeleteMI.Enabled          = false;

			// selects the item that was right clicked on.
			ListViewItem clickedItem = SubscriptionsLV.GetItemAt(e.X, e.Y);

			// no item clicked on - do nothing.
			if (clickedItem == null) return;

			// force selection to clicked node.
			clickedItem.Selected = true;

			// check for subscription.
			if (typeof(Subscription).IsInstanceOfType(clickedItem.Tag))
			{
				EditMI.Enabled   = true;
				ActiveMI.Enabled = true;
				DeleteMI.Enabled = true;

				ActiveMI.Checked = ((Subscription)clickedItem.Tag).Active;
				return;
			}
		}	

		/// <summary>
		/// Prompts the user to create a new subscription.
		/// </summary>
		private void AddSubscriptionMI_Click(object sender, System.EventArgs e)
		{		
			try
			{
				AddSubscription();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		/// <summary>
		/// Edits the state of a subscription.
		/// </summary>
		private void EditMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				ListViewItem item = SubscriptionsLV.SelectedItems[0];
				Opc.Ae.Subscription subscription = (Subscription)item.Tag;

				// show properties dialog.
				bool active = subscription.Active;

				new SubscriptionEditDlg().ShowDialog(m_server, subscription);

				if (subscription == null)
				{
					return;
				}

				// update list.
				Update(item);

				// send notifications.
				if (active != subscription.Active)
				{
					if (m_SubscriptionAction != null)
					{
						m_SubscriptionAction(subscription, !subscription.Active);
					}
				}

				// adjust columns.
				AdjustColumns(SubscriptionsLV);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}				
		}

		/// <summary>
		/// Toggles the active/inactive state for a subscription.
		/// </summary>
		private void ActiveMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				ListViewItem item = SubscriptionsLV.SelectedItems[0];
				Subscription subscription = (Subscription)item.Tag;

				SubscriptionState state = new SubscriptionState();
				state.Active = !ActiveMI.Checked;

				subscription.ModifyState((int)StateMask.Active, state);
				
				// toggle checkbox.
				ActiveMI.Checked = !ActiveMI.Checked;
	
				// update list.
				Update(item);

				// receive notifications.
				if (m_SubscriptionAction != null)
				{
					m_SubscriptionAction(subscription, !subscription.Active);
				}

				// adjust columns.
				AdjustColumns(SubscriptionsLV);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}				
		}

		/// <summary>
		/// Deletes a subscription.
		/// </summary>
		private void DeleteMI_Click(object sender, System.EventArgs e)
		{
			try
			{	
				ListViewItem item = SubscriptionsLV.SelectedItems[0];
				Subscription subscription = (Subscription)item.Tag;

				// send notifications.
				if (m_SubscriptionAction != null)
				{
					m_SubscriptionAction(subscription, true);
				}

				// remove from list.
                item.Remove();

                subscription.Dispose();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		/// <summary>
		/// Refreshes the current subscription.
		/// </summary>
		private void RefreshMI_Click(object sender, System.EventArgs e)
		{
			try
			{	
				ListViewItem item = SubscriptionsLV.SelectedItems[0];
				Subscription subscription = (Subscription)item.Tag;		

				subscription.Refresh();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}
		#endregion

	}
}
  