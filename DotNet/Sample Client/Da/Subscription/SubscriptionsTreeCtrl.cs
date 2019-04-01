//============================================================================
// TITLE: SubscriptionsTreeCtrl.cs
//
// CONTENTS:
// 
// A tree control used to navigate and manipulate a set of subscriptions for a DA server.
//
// (c) Copyright 2003 The OPC Foundation
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
// 2003/06/11 RSA   Initial implementation.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Opc.Da;
using Opc.SampleClient;

namespace Opc.Da.SampleClient
{
	/// <summary>
	/// Delegate to receive subscription created/deleted events.
	/// </summary>
	public delegate void SubscriptionModifiedCallback(Subscription subscription, bool deleted);
	
	/// <summary>
	/// A tree control used to navigate and manipulate a set of subscriptions for a DA server.
	/// </summary>
	public class SubscriptionsTreeCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TreeView SubscriptionTV;
		private System.Windows.Forms.ContextMenu Server_PopupMenu;
		private System.Windows.Forms.ContextMenu Subscription_PopupMenu;
		private System.Windows.Forms.MenuItem SubscriptionDeleteMI;
		private System.Windows.Forms.MenuItem SubscriptionAddItemsMI;
		private System.Windows.Forms.MenuItem SubscriptionEditItemsMI;
		private System.Windows.Forms.MenuItem SubscriptionEditMI;
		private System.Windows.Forms.MenuItem SubscriptionReadMI;
		private System.Windows.Forms.MenuItem SubscriptionWriteMI;
		private System.Windows.Forms.MenuItem SubscriptionRefreshMI;
		private System.Windows.Forms.MenuItem SubscriptionActiveMI;
		private System.Windows.Forms.MenuItem SubscriptionEnabledMI;
		private System.Windows.Forms.MenuItem SeparatorS1;
		private System.Windows.Forms.MenuItem SeparatorS2;
		private System.Windows.Forms.MenuItem ServerViewStatusMI;
		private System.Windows.Forms.MenuItem ServerReadItemsMI;
		private System.Windows.Forms.MenuItem ServerWriteItemsMI;
		private System.Windows.Forms.MenuItem SubscriptionCreateMI;
		private System.Windows.Forms.ContextMenu Item_PopupMenu;
		private System.Windows.Forms.MenuItem ItemEditMI;
		private System.Windows.Forms.MenuItem ItemRemoveMI;
		private System.Windows.Forms.MenuItem ServerDisconnectMI;
		private System.Windows.Forms.MenuItem ServerBrowseItemsMI;
		private System.Windows.Forms.MenuItem Separator02;
		private System.Windows.Forms.MenuItem Separator01;
		private System.Windows.Forms.MenuItem SeparatorI1;
		private System.Windows.Forms.MenuItem ItemActiveMI;
		private System.Windows.Forms.MenuItem SubscriptionAsyncReadMI;
		private System.Windows.Forms.MenuItem SubscriptionAsyncWriteMI;
		private System.Windows.Forms.MenuItem SeparatorS3;
		private System.Windows.Forms.MenuItem EditOptionsMI;
		private System.Windows.Forms.MenuItem SubscriptionEditOptionsMI;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SubscriptionsTreeCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			SubscriptionTV.ImageList = Resources.Instance.ImageList;
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
			this.SubscriptionTV = new System.Windows.Forms.TreeView();
			this.Server_PopupMenu = new System.Windows.Forms.ContextMenu();
			this.ServerViewStatusMI = new System.Windows.Forms.MenuItem();
			this.EditOptionsMI = new System.Windows.Forms.MenuItem();
			this.ServerDisconnectMI = new System.Windows.Forms.MenuItem();
			this.Separator01 = new System.Windows.Forms.MenuItem();
			this.ServerBrowseItemsMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionCreateMI = new System.Windows.Forms.MenuItem();
			this.Separator02 = new System.Windows.Forms.MenuItem();
			this.ServerReadItemsMI = new System.Windows.Forms.MenuItem();
			this.ServerWriteItemsMI = new System.Windows.Forms.MenuItem();
			this.Subscription_PopupMenu = new System.Windows.Forms.ContextMenu();
			this.SubscriptionEditMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionDeleteMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionAddItemsMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionEditItemsMI = new System.Windows.Forms.MenuItem();
			this.SeparatorS1 = new System.Windows.Forms.MenuItem();
			this.SubscriptionActiveMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionEnabledMI = new System.Windows.Forms.MenuItem();
			this.SeparatorS2 = new System.Windows.Forms.MenuItem();
			this.SubscriptionReadMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionWriteMI = new System.Windows.Forms.MenuItem();
			this.SeparatorS3 = new System.Windows.Forms.MenuItem();
			this.SubscriptionAsyncReadMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionAsyncWriteMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionRefreshMI = new System.Windows.Forms.MenuItem();
			this.Item_PopupMenu = new System.Windows.Forms.ContextMenu();
			this.ItemEditMI = new System.Windows.Forms.MenuItem();
			this.ItemRemoveMI = new System.Windows.Forms.MenuItem();
			this.SeparatorI1 = new System.Windows.Forms.MenuItem();
			this.ItemActiveMI = new System.Windows.Forms.MenuItem();
			this.SubscriptionEditOptionsMI = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// SubscriptionTV
			// 
			this.SubscriptionTV.ContextMenu = this.Server_PopupMenu;
			this.SubscriptionTV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubscriptionTV.ImageIndex = -1;
			this.SubscriptionTV.Location = new System.Drawing.Point(0, 0);
			this.SubscriptionTV.Name = "SubscriptionTV";
			this.SubscriptionTV.SelectedImageIndex = -1;
			this.SubscriptionTV.Size = new System.Drawing.Size(360, 400);
			this.SubscriptionTV.TabIndex = 0;
			this.SubscriptionTV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SubscriptionTV_MouseDown);
			// 
			// Server_PopupMenu
			// 
			this.Server_PopupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							 this.ServerViewStatusMI,
																							 this.EditOptionsMI,
																							 this.ServerDisconnectMI,
																							 this.Separator01,
																							 this.ServerBrowseItemsMI,
																							 this.SubscriptionCreateMI,
																							 this.Separator02,
																							 this.ServerReadItemsMI,
																							 this.ServerWriteItemsMI});
			// 
			// ServerViewStatusMI
			// 
			this.ServerViewStatusMI.Index = 0;
			this.ServerViewStatusMI.Text = "&View Status...";
			this.ServerViewStatusMI.Click += new System.EventHandler(this.ServerViewStatusMI_Click);
			// 
			// EditOptionsMI
			// 
			this.EditOptionsMI.Index = 1;
			this.EditOptionsMI.Text = "Edit &Options...";
			this.EditOptionsMI.Click += new System.EventHandler(this.EditOptionsMI_Click);
			// 
			// ServerDisconnectMI
			// 
			this.ServerDisconnectMI.Index = 2;
			this.ServerDisconnectMI.Text = "&Disconnect";
			this.ServerDisconnectMI.Click += new System.EventHandler(this.ServerDisconnectMI_Click);
			// 
			// Separator01
			// 
			this.Separator01.Index = 3;
			this.Separator01.Text = "-";
			// 
			// ServerBrowseItemsMI
			// 
			this.ServerBrowseItemsMI.Index = 4;
			this.ServerBrowseItemsMI.Text = "&Browse Items...";
			this.ServerBrowseItemsMI.Click += new System.EventHandler(this.ServerBrowseItemsMI_Click);
			// 
			// SubscriptionCreateMI
			// 
			this.SubscriptionCreateMI.Index = 5;
			this.SubscriptionCreateMI.Text = "&Create Subscription...";
			this.SubscriptionCreateMI.Click += new System.EventHandler(this.CreateSubscriptionMI_Click);
			// 
			// Separator02
			// 
			this.Separator02.Index = 6;
			this.Separator02.Text = "-";
			// 
			// ServerReadItemsMI
			// 
			this.ServerReadItemsMI.Index = 7;
			this.ServerReadItemsMI.Text = "&Read...";
			this.ServerReadItemsMI.Click += new System.EventHandler(this.ServerReadItemsMI_Click);
			// 
			// ServerWriteItemsMI
			// 
			this.ServerWriteItemsMI.Index = 8;
			this.ServerWriteItemsMI.Text = "&Write...";
			this.ServerWriteItemsMI.Click += new System.EventHandler(this.ServerWriteItemsMI_Click);
			// 
			// Subscription_PopupMenu
			// 
			this.Subscription_PopupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								   this.SubscriptionEditMI,
																								   this.SubscriptionEditOptionsMI,
																								   this.SubscriptionDeleteMI,
																								   this.SubscriptionAddItemsMI,
																								   this.SubscriptionEditItemsMI,
																								   this.SeparatorS1,
																								   this.SubscriptionActiveMI,
																								   this.SubscriptionEnabledMI,
																								   this.SeparatorS2,
																								   this.SubscriptionReadMI,
																								   this.SubscriptionWriteMI,
																								   this.SeparatorS3,
																								   this.SubscriptionAsyncReadMI,
																								   this.SubscriptionAsyncWriteMI,
																								   this.SubscriptionRefreshMI});
			// 
			// SubscriptionEditMI
			// 
			this.SubscriptionEditMI.Index = 0;
			this.SubscriptionEditMI.Text = "&Edit State...";
			this.SubscriptionEditMI.Click += new System.EventHandler(this.SubscriptionEditMI_Click);
			// 
			// SubscriptionDeleteMI
			// 
			this.SubscriptionDeleteMI.Index = 2;
			this.SubscriptionDeleteMI.Text = "&Delete";
			this.SubscriptionDeleteMI.Click += new System.EventHandler(this.SubscriptionDeleteMI_Click);
			// 
			// SubscriptionAddItemsMI
			// 
			this.SubscriptionAddItemsMI.Index = 3;
			this.SubscriptionAddItemsMI.Text = "&Add Items..";
			this.SubscriptionAddItemsMI.Click += new System.EventHandler(this.SubscriptionAddItemsMI_Click);
			// 
			// SubscriptionEditItemsMI
			// 
			this.SubscriptionEditItemsMI.Index = 4;
			this.SubscriptionEditItemsMI.Text = "E&dit Items...";
			this.SubscriptionEditItemsMI.Click += new System.EventHandler(this.SubscriptionEditItemsMI_Click);
			// 
			// SeparatorS1
			// 
			this.SeparatorS1.Index = 5;
			this.SeparatorS1.Text = "-";
			// 
			// SubscriptionActiveMI
			// 
			this.SubscriptionActiveMI.Index = 6;
			this.SubscriptionActiveMI.Text = "Acti&ve";
			this.SubscriptionActiveMI.Click += new System.EventHandler(this.SubscriptionActiveMI_Click);
			// 
			// SubscriptionEnabledMI
			// 
			this.SubscriptionEnabledMI.Index = 7;
			this.SubscriptionEnabledMI.Text = "E&nabled";
			this.SubscriptionEnabledMI.Click += new System.EventHandler(this.SubscriptionEnabledMI_Click);
			// 
			// SeparatorS2
			// 
			this.SeparatorS2.Index = 8;
			this.SeparatorS2.Text = "-";
			// 
			// SubscriptionReadMI
			// 
			this.SubscriptionReadMI.Index = 9;
			this.SubscriptionReadMI.Text = "&Read...";
			this.SubscriptionReadMI.Click += new System.EventHandler(this.SubscriptionReadMI_Click);
			// 
			// SubscriptionWriteMI
			// 
			this.SubscriptionWriteMI.Index = 10;
			this.SubscriptionWriteMI.Text = "&Write...";
			this.SubscriptionWriteMI.Click += new System.EventHandler(this.SubscriptionWriteMI_Click);
			// 
			// SeparatorS3
			// 
			this.SeparatorS3.Index = 11;
			this.SeparatorS3.Text = "-";
			// 
			// SubscriptionAsyncReadMI
			// 
			this.SubscriptionAsyncReadMI.Index = 12;
			this.SubscriptionAsyncReadMI.Text = "Async Read...";
			this.SubscriptionAsyncReadMI.Click += new System.EventHandler(this.SubscriptionAsyncReadMI_Click);
			// 
			// SubscriptionAsyncWriteMI
			// 
			this.SubscriptionAsyncWriteMI.Index = 13;
			this.SubscriptionAsyncWriteMI.Text = "Async Write...";
			this.SubscriptionAsyncWriteMI.Click += new System.EventHandler(this.SubscriptionAsyncWriteMI_Click);
			// 
			// SubscriptionRefreshMI
			// 
			this.SubscriptionRefreshMI.Index = 14;
			this.SubscriptionRefreshMI.Text = "Refre&sh";
			this.SubscriptionRefreshMI.Click += new System.EventHandler(this.SubscriptionRefreshMI_Click);
			// 
			// Item_PopupMenu
			// 
			this.Item_PopupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						   this.ItemEditMI,
																						   this.ItemRemoveMI,
																						   this.SeparatorI1,
																						   this.ItemActiveMI});
			// 
			// ItemEditMI
			// 
			this.ItemEditMI.Index = 0;
			this.ItemEditMI.Text = "&Edit...";
			this.ItemEditMI.Click += new System.EventHandler(this.ItemEditMI_Click);
			// 
			// ItemRemoveMI
			// 
			this.ItemRemoveMI.Index = 1;
			this.ItemRemoveMI.Text = "&Delete";
			this.ItemRemoveMI.Click += new System.EventHandler(this.ItemRemoveMI_Click);
			// 
			// SeparatorI1
			// 
			this.SeparatorI1.Index = 2;
			this.SeparatorI1.Text = "-";
			// 
			// ItemActiveMI
			// 
			this.ItemActiveMI.Index = 3;
			this.ItemActiveMI.Text = "&Active";
			this.ItemActiveMI.Click += new System.EventHandler(this.ItemActiveMI_Click);
			// 
			// SubscriptionEditOptionsMI
			// 
			this.SubscriptionEditOptionsMI.Index = 1;
			this.SubscriptionEditOptionsMI.Text = "Edit &Options...";
			this.SubscriptionEditOptionsMI.Click += new System.EventHandler(this.SubscriptionEditOptionsMI_Click);
			// 
			// SubscriptionsTreeCtrl
			// 
			this.Controls.Add(this.SubscriptionTV);
			this.Name = "SubscriptionsTreeCtrl";
			this.Size = new System.Drawing.Size(360, 400);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The underlying tree view. 
		/// </summary>
		public TreeView View {get{return SubscriptionTV;}}

		/// <summary>
		/// Raised when a subscrition is created/deleted.
		/// </summary>
		public event SubscriptionModifiedCallback SubscriptionModified = null;

		/// <summary>
		/// Initializes the control with the specified server.
		/// </summary>
		public void Initialize(Opc.Da.Server server)
		{
			// clear the tree view.
			SubscriptionTV.Nodes.Clear();

			// check if nothing to do.
			if (server == null) return;

			// connect to server if not already.
			if (!server.IsConnected)
			{
				server.Connect(null);
			}

			// add the root node.
			TreeNode node = new TreeNode(server.Name);
			node.ImageIndex = node.SelectedImageIndex = Resources.IMAGE_LOCAL_SERVER;
			node.Tag = server;

			SubscriptionTV.Nodes.Add(node);

			// add any existing subscriptions.
			foreach (Subscription subscription in server.Subscriptions)
			{
				AddSubscription(node, subscription);

				if (SubscriptionModified != null)
				{
					SubscriptionModified(subscription, false);
				}
			}

			// expand root node.
			node.Expand();
		}

		/// <summary>
		/// Initializes the control with the specified server.
		/// </summary>
		public void Initialize(Subscription subscription)
		{
			if (subscription == null) throw new ArgumentNullException("subscription");

			AddSubscription(null, subscription);
		}

		/// <summary>
		/// Disconnects from the server.
		/// </summary>
		public void Disconnect()
		{
			foreach (TreeNode node in SubscriptionTV.Nodes)
			{
				Disconnect(node, (Server)node.Tag);
			}
		}

		/// <summary>
		/// Displays the server status in a model dialog.
		/// </summary>
		private void ViewStatus(Server server)
		{
			new ServerStatusDlg().ShowDialog(server);	
		}

		/// <summary>
		/// Disconnects from the server and clears all objects.
		/// </summary>
		private void Disconnect(TreeNode node, Server server)
		{
			// remove all subscriptions.
			foreach (TreeNode child in node.Nodes)
			{
				if (IsSubscriptionNode(child))
				{
					if (SubscriptionModified != null)
					{
						SubscriptionModified((Subscription)child.Tag, true);
					}

					Subscription subscription = (Subscription)child.Tag;
					server.CancelSubscription(subscription);
					subscription.Dispose();				
				}
			}

			// disconnect server.
			server.Disconnect();

			// remove tree from view.
			node.Remove();
		}
		
		/// <summary>
		/// Displays the address space of the server in a modal dialog.
		/// </summary>
		private void BrowseItems(Opc.Da.Server server)
		{
			new BrowseItemsDlg().Initialize(server);
		}

		/// <summary>
		/// Prompts the user to select a set of items and reads them from the server.
		/// </summary>
		private void ReadItems(Opc.Da.Server server)
		{
			new ReadItemsDlg().ShowDialog(server);	
		}

		/// <summary>
		/// Prompts the user to specify a set of item values and writes them to the server.
		/// </summary>
		private void WriteItems(Opc.Da.Server server)
		{
			new WriteItemsDlg().ShowDialog(server);	
		}

		/// <summary>
		/// Prompts the user to create a new subscription.
		/// </summary>
		private void CreateSubscription(TreeNode node, Opc.Da.Server server)
		{
			Subscription subscription = new SubscriptionCreateDlg().ShowDialog(server);

			if (subscription == null)
			{
				return;
			}

			AddSubscription(node, subscription);
			node.Expand();

			if (SubscriptionModified != null)
			{
				SubscriptionModified(subscription, false);
			}
		}

		/// <summary>
		/// Deletes the specified subscription.
		/// </summary>
		private void DeleteSubscription(TreeNode node, Subscription subscription)
		{
			if (SubscriptionModified != null)
			{
				SubscriptionModified(subscription, true);
			}

			try
			{
				Server server = subscription.Server;
				server.CancelSubscription(subscription);
				subscription.Dispose();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}

			node.Remove();
		}		
		
		/// <summary>
		/// Edit the specified subscription.
		/// </summary>
		private void EditSubscription(TreeNode node, Subscription subscription)
		{
			try
			{
				SubscriptionState state = new SubscriptionListEditDlg().ShowDialog(subscription.Server, subscription.State);

				if (state == null)
				{
					return;
				}

				subscription.ModifyState((int)StateMask.All, state);

				node.Text = subscription.Name;

				if (SubscriptionModified != null)
				{
					SubscriptionModified(subscription, !state.Active);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Toggles the active state of a subscription.
		/// </summary>
		private void SetActive(Subscription subscription, bool active)
		{
			try
			{
				SubscriptionState state = new SubscriptionState();
				state.Active = active;
				subscription.ModifyState((int)StateMask.Active, state);

				if (SubscriptionModified != null)
				{
					SubscriptionModified(subscription, !state.Active);
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Toggles the enabled state of a subscription.
		/// </summary>
		private void SetEnabled(Subscription subscription, bool enabled)
		{
			try
			{
				subscription.SetEnabled(enabled);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Prompts the user to select a set of items and reads them from the server.
		/// </summary>
		private void ReadItems(Subscription subscription, bool synchronous)
		{
			new ReadItemsDlg().ShowDialog(subscription, synchronous);	
		}

		/// <summary>
		/// Prompts the user to specify a set of item values and writes them to the server.
		/// </summary>
		private void WriteItems(Subscription subscription, bool synchronous)
		{
			new WriteItemsDlg().ShowDialog(subscription, synchronous);	
		}

		/// <summary>
		/// Refreshes the items of a subscription.
		/// </summary>
		private void Refresh(Subscription subscription)
		{
			try
			{
				subscription.Refresh();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Adds items to the specified subscription.
		/// </summary>
		private void AddItems(TreeNode node, Subscription subscription)
		{
			ItemResult[] items = new SubscriptionAddItemsDlg().ShowDialog(subscription);

			// update tree with new items.
			if (items != null)
			{
				foreach (ItemResult item in items) 
				{
					if (item.ResultID.Succeeded())
					{
						AddItem(node, item);
					}
				}

				node.Expand();
			}
		}

		/// <summary>
		/// Edits items in the specified subscription.
		/// </summary>
		private void EditItems(TreeNode node, Subscription subscription)
		{
			try
			{
				// save item ids to detect item id changes.
				ItemIdentifier[] itemIDs = new ItemIdentifier[subscription.Items.Length];

				for (int ii = 0; ii < itemIDs.Length; ii++)
				{
					itemIDs[ii] = new ItemIdentifier(subscription.Items[ii]);
				}

				// edit the items.
				Item[] items = new ItemListEditDlg().ShowDialog(subscription.Items, false, false);

				if (items == null)
				{
					return;
				}

				// separate the items in lists depending on whether item id changed.
				ArrayList insertItems = new ArrayList();
				ArrayList updateItems = new ArrayList();
				ArrayList deleteItems = new ArrayList();

				for (int ii = 0; ii < itemIDs.Length; ii++)
				{
					if (items[ii].Key != itemIDs[ii].Key)
					{
						insertItems.Add(items[ii]);
						deleteItems.Add(itemIDs[ii]);
					}
					else
					{
						updateItems.Add(items[ii]);
					}
				}

				// update existing items.
				if (updateItems.Count > 0)
				{
					subscription.ModifyItems((int)StateMask.All, (Item[])updateItems.ToArray(typeof(Item)));
				}

				// insert new items.
				if (insertItems.Count > 0)
				{
					subscription.AddItems((Item[])insertItems.ToArray(typeof(Item)));
				}

				// delete old items.
				if (deleteItems.Count > 0)
				{
					subscription.RemoveItems((ItemIdentifier[])deleteItems.ToArray(typeof(ItemIdentifier)));
				}

				// update tree.
				node.Nodes.Clear();
				
				foreach (Item item in subscription.Items)
				{ 
					AddItem(node, item); 
				}
				
				node.Expand();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Edits items in the specified subscription.
		/// </summary>
		private void EditItem(TreeNode node, Subscription subscription, Item item)
		{
			try
			{
				// save existing item id.
				ItemIdentifier itemID = new ItemIdentifier(item);

				Item[] items = new ItemListEditDlg().ShowDialog(new Item[] { item }, false, false);

				if (items == null)
				{
					return;
				}

				// modify an existing item.
				if (itemID.Key == items[0].Key)
				{
					subscription.ModifyItems((int)StateMask.All, items);
				}

				// add/remove item because the item id changed.
				else
				{
					items = subscription.AddItems(items);
					subscription.RemoveItems(new ItemIdentifier[] {itemID});
				}

				node.Text = items[0].ItemName;
				node.Tag  = items[0];

			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Remove an item in the specified subscription.
		/// </summary>
		private void RemoveItem(TreeNode node, Subscription subscription, Item item)
		{
			try
			{
				subscription.RemoveItems(new ItemIdentifier[] { item });
				node.Remove();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}
	
		/// <summary>
		/// Toggles the active state of a subscription.
		/// </summary>
		private void SetActive(TreeNode node, Subscription subscription, Item item, bool active)
		{
			try
			{			
				item.Active = active;
				item.ActiveSpecified = true;

				Item[] items = subscription.ModifyItems((int)StateMask.Active, new Item[] { item });

				if (items != null)
				{
					node.Tag = items[0];
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Checks if the current node is a server node.
		/// </summary>
		private bool IsServerNode(TreeNode node)
		{
			if (node == null || node.Tag == null) return false;
		
			return typeof(Server).IsInstanceOfType(node.Tag);
		}

		/// <summary>
		/// Checks if the current node is a subscription node.
		/// </summary>
		private bool IsSubscriptionNode(TreeNode node)
		{
			if (node == null || node.Tag == null) return false;
			return (node.Tag.GetType().GetInterface(typeof(ISubscription).FullName) != null);
		}

		/// <summary>
		/// Checks if the current node is an item node.
		/// </summary>
		private bool IsItemNode(TreeNode node)
		{
			if (node == null || node.Tag == null) return false;
			return (node.Tag.GetType() == typeof(Item) || node.Tag.GetType() == typeof(ItemResult));
		}

		/// <summary>
		/// Adds a subscription into the tree.
		/// </summary>
		private void AddSubscription(TreeNode parent, Subscription subscription)
		{
			TreeNode child = new TreeNode(subscription.Name);
			child.ImageIndex = child.SelectedImageIndex = Resources.IMAGE_ENVELOPE;
			child.Tag = subscription;

			foreach (Item item in subscription.Items)
			{
				AddItem(child, item);
			}

			if (parent != null)	
			{ 
				parent.Nodes.Add(child); 
			}
			else                
			{ 
				SubscriptionTV.Nodes.Add(child); 
				child.Expand(); 
			}
		}
		
		/// <summary>
		/// Adds an item into the tree.
		/// </summary>
		private void AddItem(TreeNode parent, Item item)
		{
			TreeNode child = new TreeNode(item.ItemName);
			child.ImageIndex = child.SelectedImageIndex = Resources.IMAGE_YELLOW_SCROLL;
			child.Tag = item;

			parent.Nodes.Add(child);
		}
		
		/// <summary>
		/// Removes a subscription an releases all resources.
		/// </summary>
		private void RemoveSubscriptionMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				if (node.Parent != null)
				{
					try
					{
						Server server = (Server)node.Parent.Tag;
						server.CancelSubscription((Subscription)node.Tag);
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}

					node.Remove();
				}
			}
		}

		/// <summary>
		/// Updates the state of context menu. 
		/// </summary>
		private void SubscriptionTV_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// ignore left button actions.
			if (e.Button != MouseButtons.Right)	return;

			// selects the item that was right clicked on.
			TreeNode clickedNode = SubscriptionTV.GetNodeAt(e.X, e.Y);

			// no item clicked on - do nothing.
			if (clickedNode == null) return;

			// force selection to clicked node.
			SubscriptionTV.SelectedNode = clickedNode;

			// disable everything.
			ServerViewStatusMI.Enabled      = false;
			ServerBrowseItemsMI.Enabled     = false;
			ServerReadItemsMI.Enabled       = false;
			ServerWriteItemsMI.Enabled      = false;
			ServerDisconnectMI.Enabled      = false;
			SubscriptionCreateMI.Enabled    = false;
			SubscriptionDeleteMI.Enabled    = false;
			SubscriptionAddItemsMI.Enabled  = false;
			SubscriptionEditItemsMI.Enabled = false;
			SubscriptionEditMI.Enabled      = false;
			SubscriptionReadMI.Enabled      = false;
			SubscriptionWriteMI.Enabled     = false;
			SubscriptionRefreshMI.Enabled   = false;
			SubscriptionActiveMI.Enabled    = false;
			SubscriptionEnabledMI.Enabled   = false;
			ItemEditMI.Enabled              = false;
			ItemRemoveMI.Enabled            = false;

			if (IsServerNode(clickedNode))
			{
				SubscriptionTV.ContextMenu = Server_PopupMenu;

				ServerViewStatusMI.Enabled   = true;
				ServerBrowseItemsMI.Enabled  = true;
				ServerReadItemsMI.Enabled    = true;
				ServerWriteItemsMI.Enabled   = true;
				ServerDisconnectMI.Enabled   = true;
				SubscriptionCreateMI.Enabled = true;

				return;
			}

			if (IsSubscriptionNode(clickedNode))
			{
				SubscriptionTV.ContextMenu = Subscription_PopupMenu;

				SubscriptionEditMI.Enabled      = true;
				SubscriptionDeleteMI.Enabled    = clickedNode.Parent != null;
				SubscriptionAddItemsMI.Enabled  = true;
				SubscriptionEditItemsMI.Enabled = clickedNode.Parent != null;
				SubscriptionReadMI.Enabled      = clickedNode.Parent != null;
				SubscriptionWriteMI.Enabled     = clickedNode.Parent != null;
				SubscriptionRefreshMI.Enabled   = clickedNode.Parent != null;
				SubscriptionActiveMI.Enabled    = clickedNode.Parent != null;
				SubscriptionEnabledMI.Enabled   = clickedNode.Parent != null;

				SubscriptionActiveMI.Checked    = ((Subscription)clickedNode.Tag).Active;
				SubscriptionEnabledMI.Checked   = ((Subscription)clickedNode.Tag).Enabled;
				return;
			}

			if (IsItemNode(clickedNode))
			{
				SubscriptionTV.ContextMenu = Item_PopupMenu;

				Item item = (Item)clickedNode.Tag;

				ItemEditMI.Enabled   = true;
				ItemRemoveMI.Enabled = true;
				ItemActiveMI.Checked = item.Active;
				return;
			}
		}

		/// <summary>
		/// Displays the server status in a modal dialog.
		/// </summary>
		private void ServerViewStatusMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				ViewStatus((Server)node.Tag);
			}
		}

		/// <summary>
		/// Disconnects and removes a server from the control.
		/// </summary>
		private void ServerDisconnectMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				Disconnect(node, (Server)node.Tag);
			}
		}

		/// <summary>
		/// Displays the address space of the server in a modal dialog.
		/// </summary>
		private void ServerBrowseItemsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				BrowseItems((Opc.Da.Server)node.Tag);
			}
		}

		/// <summary>
		/// Creates a new subscription and adds it to the tree.
		/// </summary>
		private void CreateSubscriptionMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				CreateSubscription(node, (Opc.Da.Server)node.Tag);
			}
		}

		/// <summary>
		/// Reads a set of items for a server.
		/// </summary>
		private void ServerReadItemsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				ReadItems((Opc.Da.Server)node.Tag);
			}
		}

		/// <summary>
		/// Writes a set of items to the server.
		/// </summary>
		private void ServerWriteItemsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				WriteItems((Opc.Da.Server)node.Tag);
			}
		}

		/// <summary>
		/// Edits the state of a subscription.
		/// </summary>
		private void SubscriptionEditMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				EditSubscription(node, (Subscription)node.Tag);
			}
		}

		/// <summary>
		/// Removes a subscription.
		/// </summary>
		private void SubscriptionDeleteMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				DeleteSubscription(node, (Subscription)node.Tag);
			}
		}

		/// <summary>
		/// Adds items to a subscription.
		/// </summary>
		private void SubscriptionAddItemsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				AddItems(node, (Subscription)node.Tag);
			}
		}

		/// <summary>
		/// Edits items in a subscription.
		/// </summary>
		private void SubscriptionEditItemsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				EditItems(node, (Subscription)node.Tag);
			}
		}

		/// <summary>
		/// Toggles the active state of a subscription.
		/// </summary>
		private void SubscriptionActiveMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				SubscriptionActiveMI.Checked = !SubscriptionActiveMI.Checked;
				SetActive((Subscription)node.Tag, SubscriptionActiveMI.Checked);
			}
		}

		/// <summary>
		/// Toggles the enabled state of a subscription.
		/// </summary>
		private void SubscriptionEnabledMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				SubscriptionEnabledMI.Checked = !SubscriptionEnabledMI.Checked;
				SetEnabled((Subscription)node.Tag, SubscriptionEnabledMI.Checked);
			}
		}

		/// <summary>
		/// Reads a set of items belonging to a subscription.
		/// </summary>
		private void SubscriptionReadMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				ReadItems((Subscription)node.Tag, true);
			}
		}

		/// <summary>
		/// Writes a set of items belonging to a subscription.
		/// </summary>
		private void SubscriptionWriteMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				WriteItems((Subscription)node.Tag, true);
			}
		}

		/// <summary>
		/// Reads a set of items belonging to a subscription.
		/// </summary>
		private void SubscriptionAsyncReadMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				ReadItems((Subscription)node.Tag, false);
			}
		}

		/// <summary>
		/// Writes a set of items belonging to a subscription.
		/// </summary>
		private void SubscriptionAsyncWriteMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				WriteItems((Subscription)node.Tag, false);
			}
		}

		/// <summary>
		/// Refreshes the items of a subscription.
		/// </summary>
		private void SubscriptionRefreshMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				Refresh((Subscription)node.Tag);
			}
		}

		/// <summary>
		/// Edits a single item.
		/// </summary>
		private void ItemEditMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsItemNode(node))
			{
				EditItem(node, (Subscription)node.Parent.Tag, (Item)node.Tag);
			}
		}

		/// <summary>
		/// Removes a single item.
		/// </summary>
		private void ItemRemoveMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsItemNode(node))
			{
				RemoveItem(node, (Subscription)node.Parent.Tag, (Item)node.Tag);
			}
		}

		/// <summary>
		/// Toggles the active state of a single item.
		/// </summary>
		private void ItemActiveMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsItemNode(node))
			{
				ItemActiveMI.Checked = !ItemActiveMI.Checked;
				SetActive(node, (Subscription)node.Parent.Tag, (Item)node.Tag, ItemActiveMI.Checked);
			}
		}

		/// <summary>
		/// Called when the Server | Edit Options menu item is clicked.
		/// </summary>
		private void EditOptionsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsServerNode(node))
			{
				new OptionsEditDlg().ShowDialog((Server)node.Tag);
			}
		}

		/// <summary>
		/// Called when the Subscription | Edit Options menu item is clicked.
		/// </summary>
		private void SubscriptionEditOptionsMI_Click(object sender, System.EventArgs e)
		{
			TreeNode node = SubscriptionTV.SelectedNode;

			if (IsSubscriptionNode(node))
			{
				new OptionsEditDlg().ShowDialog((Subscription)node.Tag);
			}
		} 
	}
}
