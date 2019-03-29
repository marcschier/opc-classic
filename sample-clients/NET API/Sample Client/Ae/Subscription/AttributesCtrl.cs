//============================================================================
// TITLE: AttributesCtrl.cs
//
// CONTENTS:
// 
// A control used to select the return attributes for a subscription.
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
// 2004/11/17 RSA   Initial implementation.

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
	public class AttributesCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.TreeView AttributesTV;
		private System.Windows.Forms.GroupBox AttributesGB;
		private System.ComponentModel.IContainer components = null;

		public AttributesCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			AttributesTV.ImageList = Resources.Instance.ImageList;
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
			this.AttributesTV = new System.Windows.Forms.TreeView();
			this.AttributesGB = new System.Windows.Forms.GroupBox();
			this.AttributesGB.SuspendLayout();
			this.SuspendLayout();
			// 
			// AttributesTV
			// 
			this.AttributesTV.CheckBoxes = true;
			this.AttributesTV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesTV.ImageIndex = -1;
			this.AttributesTV.Location = new System.Drawing.Point(3, 16);
			this.AttributesTV.Name = "AttributesTV";
			this.AttributesTV.SelectedImageIndex = -1;
			this.AttributesTV.Size = new System.Drawing.Size(394, 285);
			this.AttributesTV.TabIndex = 0;
			this.AttributesTV.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.AttributesTV_AfterCheck);
			this.AttributesTV.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.AttributesTV_BeforeExpand);
			// 
			// AttributesGB
			// 
			this.AttributesGB.Controls.Add(this.AttributesTV);
			this.AttributesGB.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttributesGB.Location = new System.Drawing.Point(0, 0);
			this.AttributesGB.Name = "AttributesGB";
			this.AttributesGB.Size = new System.Drawing.Size(400, 304);
			this.AttributesGB.TabIndex = 1;
			this.AttributesGB.TabStop = false;
			this.AttributesGB.Text = "Attributes";
			// 
			// AttributesCtrl
			// 
			this.Controls.Add(this.AttributesGB);
			this.Name = "AttributesCtrl";
			this.Size = new System.Drawing.Size(400, 304);
			this.AttributesGB.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	
		#region Private Members
		private Opc.Ae.Server m_server = null;
		#endregion
		
		#region Public Interface
		/// <summary>
		/// Displays the available attributes in a heirarchy. 
		/// </summary>
		public void ShowAttributes(Opc.Ae.Server server)
		{
			m_server = server;

			AttributesTV.Nodes.Clear();

			// nothing more to do if no server provided.
			if (m_server == null)
			{
				return;
			}

			// display all event categories
			ShowEventCategories(AttributesTV.Nodes, EventType.Simple);
			ShowEventCategories(AttributesTV.Nodes, EventType.Tracking);
			ShowEventCategories(AttributesTV.Nodes, EventType.Condition);
		}

		/// <summary>
		/// Returns currently selected attributes
		/// </summary>
		public AttributeDictionary GetSelectedAttributes()
		{
			AttributeDictionary attributes = new AttributeDictionary();

			foreach (TreeNode child in AttributesTV.Nodes)
			{
				GetSelectedCategories(child, attributes);
			}

			return attributes;
		}

		/// <summary>
		/// Sets the currently selected attributes.
		/// </summary>
		public void SetSelectedAttributes(AttributeDictionary attributes)
		{
			foreach (TreeNode child in AttributesTV.Nodes)
			{
				SetSelectedCategories(child, attributes);
			}
		}

		/// <summary>
		/// Checks or unchecks all attributes for the specified category id.
		/// </summary>
		public void SelectCategory(int categoryID, bool picked)
		{
			foreach (TreeNode child in AttributesTV.Nodes)
			{
				SelectCategory(child, categoryID, picked);
			}
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Checks or unchecks all attributes for the specified category id.
		/// </summary>
		public void SelectCategory(TreeNode parent, int categoryID, bool picked)
		{
			foreach (TreeNode child in parent.Nodes)
			{
				if (!typeof(Category).IsInstanceOfType(child.Tag))
				{
					continue;
				}

				// find the matching category id.
				Category category = (Category)child.Tag;

				if (categoryID == category.ID)
				{
					// ensure node is visible if state changes.
					if (child.Checked != picked)
					{				
						// fetch attributes if a dummy mode exists.
						if (child.Nodes.Count == 1 && child.Nodes[0].Text.Length == 0)
						{
							child.Nodes.Clear();
							ShowEventAttributes(child.Nodes, category);
						}

						child.EnsureVisible();
					}

					if (picked)
					{
						child.Checked = true;
					}
					else
					{
						child.Checked = false;
					}
				}			
			}
		}

		/// <summary>
		/// Adds categories with selected attributes to dictionary.
		/// </summary>
		private void GetSelectedCategories(TreeNode parent, AttributeDictionary attributes)
		{
			foreach (TreeNode child in parent.Nodes)
			{
				if (!typeof(Category).IsInstanceOfType(child.Tag))
				{
					continue;
				}

				GetSelectedAttributes(child, (Category)child.Tag, attributes);
			}
        }

		/// <summary>
		/// Adds selected attributes to the dictionary.
		/// </summary>
		private void GetSelectedAttributes(TreeNode parent, Category category, AttributeDictionary attributes)
		{
			foreach (TreeNode child in parent.Nodes)
			{
				if (!typeof(Attribute).IsInstanceOfType(child.Tag))
				{
					continue;
				}

				if (child.Checked)
				{
					AttributeCollection collection = attributes[category.ID];

					if (collection == null)
					{
						attributes.Add(category.ID, null);
						collection = attributes[category.ID];
					}
				
					collection.Add(((Attribute)child.Tag).ID);
				}
			}
		}

		/// <summary>
		/// Updates categories with selected attributes to dictionary.
		/// </summary>
		private void SetSelectedCategories(TreeNode parent, AttributeDictionary attributes)
		{
			foreach (TreeNode child in parent.Nodes)
			{
				if (!typeof(Category).IsInstanceOfType(child.Tag))
				{
					continue;
				}

				// check if category exists.
				Category category = (Category)child.Tag;

				if (!attributes.Contains(category.ID))
				{
					child.Checked = false;
					continue;
				}

				// check if attributes need to be fetched.
				if (child.Nodes.Count == 1 && child.Nodes[0].Text.Length == 0)
				{
					child.Nodes.Clear();
					ShowEventAttributes(child.Nodes, category);
				}

				// update individual attributes.
				SetSelectedAttributes(child, category, attributes);
			}
		}

		/// <summary>
		/// Updates the selected attributes to the dictionary.
		/// </summary>
		private void SetSelectedAttributes(TreeNode parent, Category category, AttributeDictionary attributes)
		{
			foreach (TreeNode child in parent.Nodes)
			{
				if (!typeof(Attribute).IsInstanceOfType(child.Tag))
				{
					continue;
				}
				
				Attribute attribute = (Attribute)child.Tag;

				if (attributes[category.ID].Contains(attribute.ID))
				{
					child.Checked = true;
				}
				else
				{
					child.Checked = false;
				}
			}
		}

		/// <summary>
		/// Populates the tree with the event categories supported by the server.
		/// </summary>
		private void ShowEventCategories(TreeNodeCollection nodes, EventType eventType)
		{
			Category[] categories = null;

			// fetch categories.
			try
			{
				categories = m_server.QueryEventCategories((int)eventType);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				return;
			}

			// check for trivial case.
			if (categories.Length == 0)
			{
				return;
			}

			// create event type node.
			TreeNode root = new TreeNode(eventType.ToString());

			root.ImageIndex         = Resources.IMAGE_OPEN_YELLOW_FOLDER;
			root.SelectedImageIndex = Resources.IMAGE_CLOSED_YELLOW_FOLDER;
			root.Tag                = eventType;

			nodes.Add(root);

			// add categories to tree.
			foreach (Category category in categories)
			{
				// create node.
				TreeNode node = new TreeNode(category.Name);

				node.ImageIndex         = Resources.IMAGE_ENVELOPE;
				node.SelectedImageIndex = Resources.IMAGE_ENVELOPE;
				node.Tag                = category;

				// add dummy child to ensure '+' sign is visible.
				node.Nodes.Add(new TreeNode());

				// add to tree.
				root.Nodes.Add(node);
			}
		}

		/// <summary>
		/// Populates the tree with the event attributes supported by the category.
		/// </summary>
		private void ShowEventAttributes(TreeNodeCollection nodes, Category category)
		{
			Attribute[] attributes = null;

			// fetch attributes.
			try
			{
				attributes = m_server.QueryEventAttributes(category.ID);
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
				return;
			}

			// add attributes to tree.
			foreach (Attribute attribute in attributes)
			{
				// create node.
				TreeNode node = new TreeNode(attribute.Name);

				node.ImageIndex         = Resources.IMAGE_EXPLODING_BOX;
				node.SelectedImageIndex = Resources.IMAGE_EXPLODING_BOX;
				node.Tag                = attribute;

				// add to tree.
				nodes.Add(node);
			}
		}
		#endregion

		#region Event Handlers
		/// <summary>
		/// Fetches the child nodes before expanding a node.
		/// </summary>
		private void AttributesTV_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			try
			{
				// check if a dummy node is present.
				if (e.Node.Nodes.Count != 1 || e.Node.Nodes[0].Text.Length != 0)
				{
					return;
				}

				// check for category.
				if (typeof(Category).IsInstanceOfType(e.Node.Tag))
				{
					e.Node.Nodes.Clear();
					ShowEventAttributes(e.Node.Nodes, (Category)e.Node.Tag);
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		/// <summary>
		/// Changes the check state of all children to match the parent that was checked.
		/// </summary>
		private void AttributesTV_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			try
			{
				// check for event type.
				if (typeof(EventType).IsInstanceOfType(e.Node.Tag))
				{
					// check all attributes.
					foreach (TreeNode child in e.Node.Nodes)
					{
						child.Checked = e.Node.Checked;
					}

					// ensure changes are visible.
					e.Node.ExpandAll();
				}
			
					// check for category.
				else if (typeof(Category).IsInstanceOfType(e.Node.Tag))
				{					
					// fetch the attributes if necessary.
					if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text.Length != 0)
					{
						e.Node.Nodes.Clear();
						ShowEventAttributes(e.Node.Nodes, (Category)e.Node.Tag);
					}

					// check if any attributes are already checked.
					bool checkall = true;

					foreach (TreeNode child in e.Node.Nodes)
					{
						if (child.Checked)
						{
							checkall = false;
						}
					}

					// check all attributes.
					if (checkall || !e.Node.Checked)
					{
						foreach (TreeNode child in e.Node.Nodes)
						{
							child.Checked = e.Node.Checked;
						}
					}

					// ensure changes are visible.
					e.Node.ExpandAll();
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}
		#endregion
	}
}
  