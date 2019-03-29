//============================================================================
// TITLE: ConnectionTreeCtrl.cs
//
// CONTENTS:
// 
// A tree control used to manager a set of trends acquired from an DX server.
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
// 2004/01/05 RSA   Initial implementation.

using System;
using System.Net;
using System.Xml;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Opc;
using Opc.Dx;
using Opc.SampleClient;

namespace Opc.Dx.SampleClient
{
	/// <summary>
	/// A tree control used to manager a set of trends acquired from an DX server.
	/// </summary>
	public class ConfigurationTreeCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ContextMenu PopupMenu;
		private System.Windows.Forms.TreeView ConnectionsTV;
		private System.Windows.Forms.MenuItem AddConnectionMI;
		private System.Windows.Forms.MenuItem ReloadConfigurationMI;
		private System.Windows.Forms.MenuItem ResetConfigurationMI;
		private System.Windows.Forms.MenuItem Separator02;
		private System.Windows.Forms.MenuItem ModifyQueryMI;
		private System.Windows.Forms.MenuItem Separator03;
		private System.Windows.Forms.MenuItem Separator01;
		private System.Windows.Forms.MenuItem ModifySourceServersMI;
		private System.Windows.Forms.MenuItem AddSourceServerMI;
		private System.Windows.Forms.MenuItem DeleteSourceServerMI;
		private System.Windows.Forms.MenuItem ModifySourceServerMI;
		private System.Windows.Forms.MenuItem AddQueryMI;
		private System.Windows.Forms.MenuItem ModifyConnectionsMI;
		private System.Windows.Forms.MenuItem Separator06;
		private System.Windows.Forms.MenuItem DeleteConnectionsMI;
		private System.Windows.Forms.MenuItem DeleteConnectionMI;
		private System.Windows.Forms.MenuItem Separator05;
		private System.Windows.Forms.MenuItem Separator04;
		private System.Windows.Forms.MenuItem DeleteQueryMI;
		private System.Windows.Forms.MenuItem UpdateConnectionsMI;
		private System.Windows.Forms.MenuItem ModifyConnectionMI;
		private System.Windows.Forms.MenuItem QueryConnectionsMI;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConfigurationTreeCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			ConnectionsTV.ImageList = Resources.Instance.ImageList;
			Clear();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				// release all server objects.
				Clear();

				if (components != null)
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
			this.ConnectionsTV = new System.Windows.Forms.TreeView();
			this.PopupMenu = new System.Windows.Forms.ContextMenu();
			this.ReloadConfigurationMI = new System.Windows.Forms.MenuItem();
			this.ResetConfigurationMI = new System.Windows.Forms.MenuItem();
			this.Separator01 = new System.Windows.Forms.MenuItem();
			this.AddSourceServerMI = new System.Windows.Forms.MenuItem();
			this.ModifySourceServersMI = new System.Windows.Forms.MenuItem();
			this.Separator02 = new System.Windows.Forms.MenuItem();
			this.ModifySourceServerMI = new System.Windows.Forms.MenuItem();
			this.DeleteSourceServerMI = new System.Windows.Forms.MenuItem();
			this.Separator03 = new System.Windows.Forms.MenuItem();
			this.AddConnectionMI = new System.Windows.Forms.MenuItem();
			this.ModifyConnectionsMI = new System.Windows.Forms.MenuItem();
			this.Separator04 = new System.Windows.Forms.MenuItem();
			this.AddQueryMI = new System.Windows.Forms.MenuItem();
			this.ModifyQueryMI = new System.Windows.Forms.MenuItem();
			this.QueryConnectionsMI = new System.Windows.Forms.MenuItem();
			this.DeleteQueryMI = new System.Windows.Forms.MenuItem();
			this.Separator05 = new System.Windows.Forms.MenuItem();
			this.UpdateConnectionsMI = new System.Windows.Forms.MenuItem();
			this.DeleteConnectionsMI = new System.Windows.Forms.MenuItem();
			this.Separator06 = new System.Windows.Forms.MenuItem();
			this.ModifyConnectionMI = new System.Windows.Forms.MenuItem();
			this.DeleteConnectionMI = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// ConnectionsTV
			// 
			this.ConnectionsTV.ContextMenu = this.PopupMenu;
			this.ConnectionsTV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConnectionsTV.ImageIndex = -1;
			this.ConnectionsTV.Location = new System.Drawing.Point(0, 0);
			this.ConnectionsTV.Name = "ConnectionsTV";
			this.ConnectionsTV.SelectedImageIndex = -1;
			this.ConnectionsTV.Size = new System.Drawing.Size(400, 400);
			this.ConnectionsTV.TabIndex = 0;
			this.ConnectionsTV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ConnectionsTV_MouseDown);
			this.ConnectionsTV.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ConnectionsTV_AfterSelect);
			// 
			// PopupMenu
			// 
			this.PopupMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.ReloadConfigurationMI,
																					  this.ResetConfigurationMI,
																					  this.Separator01,
																					  this.AddSourceServerMI,
																					  this.ModifySourceServersMI,
																					  this.Separator02,
																					  this.ModifySourceServerMI,
																					  this.DeleteSourceServerMI,
																					  this.Separator03,
																					  this.AddConnectionMI,
																					  this.ModifyConnectionsMI,
																					  this.Separator04,
																					  this.ModifyConnectionMI,
																					  this.DeleteConnectionMI,
																					  this.Separator06,
																					  this.AddQueryMI,
																					  this.ModifyQueryMI,
																					  this.DeleteQueryMI,
																					  this.Separator05,
																					  this.QueryConnectionsMI,
																					  this.UpdateConnectionsMI,
																					  this.DeleteConnectionsMI});
			// 
			// ReloadConfigurationMI
			// 
			this.ReloadConfigurationMI.Index = 0;
			this.ReloadConfigurationMI.Text = "Reload Configuration";
			this.ReloadConfigurationMI.Click += new System.EventHandler(this.ReloadConfigurationMI_Click);
			// 
			// ResetConfigurationMI
			// 
			this.ResetConfigurationMI.Index = 1;
			this.ResetConfigurationMI.Text = "Reset Configuration...";
			this.ResetConfigurationMI.Click += new System.EventHandler(this.ResetConfigurationMI_Click);
			// 
			// Separator01
			// 
			this.Separator01.Index = 2;
			this.Separator01.Text = "-";
			// 
			// AddSourceServerMI
			// 
			this.AddSourceServerMI.Index = 3;
			this.AddSourceServerMI.Text = "Add Source Server...";
			this.AddSourceServerMI.Click += new System.EventHandler(this.AddSourceServerMI_Click);
			// 
			// ModifySourceServersMI
			// 
			this.ModifySourceServersMI.Index = 4;
			this.ModifySourceServersMI.Text = "Modify Source Servers...";
			this.ModifySourceServersMI.Click += new System.EventHandler(this.ModifySourceServersMI_Click);
			// 
			// Separator02
			// 
			this.Separator02.Index = 5;
			this.Separator02.Text = "-";
			// 
			// ModifySourceServerMI
			// 
			this.ModifySourceServerMI.Index = 6;
			this.ModifySourceServerMI.Text = "Modify Source Server...";
			this.ModifySourceServerMI.Click += new System.EventHandler(this.ModifySourceServerMI_Click);
			// 
			// DeleteSourceServerMI
			// 
			this.DeleteSourceServerMI.Index = 7;
			this.DeleteSourceServerMI.Text = "Delete Source Server";
			this.DeleteSourceServerMI.Click += new System.EventHandler(this.DeleteSourceServerMI_Click);
			// 
			// Separator03
			// 
			this.Separator03.Index = 8;
			this.Separator03.Text = "-";
			// 
			// AddConnectionMI
			// 
			this.AddConnectionMI.Index = 9;
			this.AddConnectionMI.Text = "Add Connection...";
			this.AddConnectionMI.Click += new System.EventHandler(this.AddConnectionMI_Click);
			// 
			// ModifyConnectionsMI
			// 
			this.ModifyConnectionsMI.Index = 10;
			this.ModifyConnectionsMI.Text = "Modify Connections...";
			this.ModifyConnectionsMI.Click += new System.EventHandler(this.ModifyConnectionsMI_Click);
			// 
			// Separator04
			// 
			this.Separator04.Index = 11;
			this.Separator04.Text = "-";
			// 
			// AddQueryMI
			// 
			this.AddQueryMI.Index = 15;
			this.AddQueryMI.Text = "Add Query...";
			this.AddQueryMI.Click += new System.EventHandler(this.AddQueryMI_Click);
			// 
			// ModifyQueryMI
			// 
			this.ModifyQueryMI.Index = 16;
			this.ModifyQueryMI.Text = "Modify Query...";
			this.ModifyQueryMI.Click += new System.EventHandler(this.ModifyQueryMI_Click);
			// 
			// QueryConnectionsMI
			// 
			this.QueryConnectionsMI.Index = 19;
			this.QueryConnectionsMI.Text = "Query Connections";
			this.QueryConnectionsMI.Click += new System.EventHandler(this.QueryConnectionsMI_Click);
			// 
			// DeleteQueryMI
			// 
			this.DeleteQueryMI.Index = 17;
			this.DeleteQueryMI.Text = "Delete Query";
			this.DeleteQueryMI.Click += new System.EventHandler(this.DeleteQueryMI_Click);
			// 
			// Separator05
			// 
			this.Separator05.Index = 18;
			this.Separator05.Text = "-";
			// 
			// UpdateConnectionsMI
			// 
			this.UpdateConnectionsMI.Index = 20;
			this.UpdateConnectionsMI.Text = "Update Connections...";
			this.UpdateConnectionsMI.Click += new System.EventHandler(this.UpdateConnectionsMI_Click);
			// 
			// DeleteConnectionsMI
			// 
			this.DeleteConnectionsMI.Index = 21;
			this.DeleteConnectionsMI.Text = "Delete Connections...";
			this.DeleteConnectionsMI.Click += new System.EventHandler(this.DeleteConnectionsMI_Click);
			// 
			// Separator06
			// 
			this.Separator06.Index = 14;
			this.Separator06.Text = "-";
			// 
			// ModifyConnectionMI
			// 
			this.ModifyConnectionMI.Index = 12;
			this.ModifyConnectionMI.Text = "Modify Connection...";
			this.ModifyConnectionMI.Click += new System.EventHandler(this.ModifyConnectionMI_Click);
			// 
			// DeleteConnectionMI
			// 
			this.DeleteConnectionMI.Index = 13;
			this.DeleteConnectionMI.Text = "Delete Connection";
			this.DeleteConnectionMI.Click += new System.EventHandler(this.DeleteConnectionMI_Click);
			// 
			// ConfigurationTreeCtrl
			// 
			this.Controls.Add(this.ConnectionsTV);
			this.Name = "ConfigurationTreeCtrl";
			this.Size = new System.Drawing.Size(400, 400);
			this.ResumeLayout(false);

		}
		#endregion
				
		#region Public Interface
		/// <summary>
		/// The set of possible node types in a configuration.
		/// </summary>
		public enum NodeType
		{
			Server,
			SourceServersFolder,
			ConnectionsFolder,
			QueriesFolder,
			SourceServer,
			Connection,
			Query
		}

		/// <summary>
		/// The set of actions that could cause events to be raised.
		/// </summary>
		public enum NodeAction
		{
			Selected,
			Picked,
			Added,
			Modified,
			Deleted
		}

		/// <summary>
		/// The arguments passed to the configuration event handler.
		/// </summary>
		public class ConfigurationEventArgs : EventArgs
		{
			/// <summary>
			/// Identifies the action that caused the event to be raised.
			/// </summary>
			public NodeAction Action
			{
				get { return m_action; }
			}

			/// <summary>
			/// Identifies the type of the node associated with the event.
			/// </summary>
			public NodeType NodeType
			{
				get { return m_nodeType; }
			}

			/// <summary>
			/// The object associated with the node.
			/// </summary>
			public object NodeData
			{
				get { return m_nodeData; }
			}

			/// <summary>
			/// Intializes the object with an action, node type and node data.
			/// </summary>
			public ConfigurationEventArgs(NodeAction action, NodeType nodeType, object nodeData)
			{
				m_action   = action;
				m_nodeType = nodeType;
				m_nodeData = nodeData;
			}
  
			#region Private Members
			private NodeAction m_action   = NodeAction.Selected;
			private NodeType   m_nodeType = NodeType.Server;
			private object     m_nodeData = null;
			#endregion
		}

		/// <summary>
		/// A delegate used to receive notifications of change to the configuration.
		/// </summary>
		public delegate void ConfigurationEventHandler(object sender, ConfigurationEventArgs e);

		/// <summary>
		/// An event that is raised whenever the user completes some action on a node.
		/// </summary>
		public event ConfigurationEventHandler ConfigurationEvent
		{
			add    { m_configurationEvent += value; }
			remove { m_configurationEvent += value; }
		}

		/// <summary>
		/// Initializes the control with the specified DX server.
		/// </summary>
		public void Initialize(Opc.Dx.Server server)
		{
			m_target = server;

			Clear();

			// nothing more to do server is null.
			if (server == null)
			{
				return;
			}

			LoadConfiguration(server);
		}

		/// <summary>
		/// Removes all nodes and releases all resources.
		/// </summary>
		public void Clear()
		{		
			ConnectionsTV.Nodes.Clear();
		}
		#endregion

		#region Private Members
		private Opc.Dx.Server m_target = null;
		private event ConfigurationEventHandler m_configurationEvent = null;

		/// <summary>
		/// A class to store context information for a tree node.
		/// </summary>
		private class NodeTag
		{
			public NodeType NodeType = NodeType.Server;
			public object   NodeData = null;

			public NodeTag(NodeType nodeType, object nodeData)
			{
				NodeType = nodeType;
				NodeData = nodeData;
			}
		}

		/// <summary>
		/// Reads the current configuration from the server and builds the tree.
		/// </summary>
		private void LoadConfiguration(Opc.Dx.Server server)
		{
			ConnectionsTV.Nodes.Clear();

			// create root node for server.
			TreeNode node = new TreeNode(server.Name);

			node.ImageIndex         = Resources.IMAGE_LOCAL_SERVER;
			node.SelectedImageIndex = Resources.IMAGE_LOCAL_SERVER;
			
			node.Tag = new NodeTag(NodeType.Server, server);

			ConnectionsTV.Nodes.Add(node);

			// load source servers.
			LoadSourceServers(node, server);

			// load connections.
			LoadConnections(node, server);

			// load queries.
			LoadQueries(node, server);
		}

		/// <summary>
		/// Reads the source servers from the DX server and builds the tree.
		/// </summary>
		private void LoadSourceServers(TreeNode parent, Opc.Dx.Server server)
		{
			// create folder node for source servers.
			TreeNode folder = new TreeNode("Source Servers");

			folder.ImageIndex         = Resources.IMAGE_CLOSED_YELLOW_FOLDER;
			folder.SelectedImageIndex = Resources.IMAGE_OPEN_YELLOW_FOLDER;
			
			folder.Tag = new NodeTag(NodeType.SourceServersFolder, null);
			
			parent.Nodes.Add(folder);

			folder.EnsureVisible();

			try
			{
				// fetch source servers.
				Opc.Dx.SourceServer[] sourceServers = server.GetSourceServers();

				if (sourceServers != null)
				{
					// add source servers to tree.
					foreach (Opc.Dx.SourceServer sourceServer in sourceServers)
					{
						AddSourceServer(folder, sourceServer);
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Updates the set of source servers.
		/// </summary>
		private void UpdateSourceServers(Opc.Dx.Server server)
		{
			// find root folder for source servers.
			TreeNode folder = FindFolder(NodeType.SourceServersFolder);
			
			folder.Nodes.Clear();

			// add connections to tree.
			foreach (Opc.Dx.SourceServer sourceServer in server.SourceServers)
			{
				AddSourceServer(folder, sourceServer);
			}
		}

		/// <summary>
		/// Adds a source server to the tree.
		/// </summary>
		private void AddSourceServer(TreeNode parent, Opc.Dx.SourceServer sourceServer)
		{
			TreeNode node = new TreeNode(sourceServer.Name);

			node.ImageIndex         = Resources.IMAGE_LOCAL_SERVER;
			node.SelectedImageIndex = Resources.IMAGE_LOCAL_SERVER;
			
			node.Tag = new NodeTag(NodeType.SourceServer, sourceServer);
			
			parent.Nodes.Add(node);
		}
		
		/// <summary>
		/// Reads the connections from the DX server and builds the tree.
		/// </summary>
		private void LoadConnections(TreeNode parent, Opc.Dx.Server server)
		{
			// create the root folder node for connections.
			 TreeNode folder = new TreeNode("DX Connections");

			 folder.ImageIndex         = Resources.IMAGE_CLOSED_YELLOW_FOLDER;
			 folder.SelectedImageIndex = Resources.IMAGE_OPEN_YELLOW_FOLDER;
			
			 folder.Tag = new NodeTag(NodeType.ConnectionsFolder, null);
			
			 parent.Nodes.Add(folder);

			 folder.EnsureVisible();

			 try
			 {
				 ResultID[] errors = null;

				 // fetch connections.
				 Opc.Dx.DXConnection[] connections = server.QueryDXConnections(
					 null,
					 null,
					 true,
					 out errors);

				 if (connections != null)
				 {
					 // add connections to tree.
					 foreach (Opc.Dx.DXConnection connection in connections)
					 {
						 AddConnection(folder, connection, false);
					 }
				 }
			 }
			 catch (Exception e)
			 {
				 MessageBox.Show(e.Message);
			 }
		}

		/// <summary>
		/// Adds the connections from the DX server and builds the tree.
		/// </summary>
		private void UpdateConnections(Opc.Dx.Server server)
		{
			// find root folder for connections.
			TreeNode folder = FindFolder(NodeType.ConnectionsFolder);

			folder.Nodes.Clear();
			
			try
			{
				ResultID[] errors = null;

				// fetch connections.
				Opc.Dx.DXConnection[] connections = server.QueryDXConnections(
					null,
					null,
					true,
					out errors);

				if (connections != null)
				{
					// add connections to tree.
					foreach (Opc.Dx.DXConnection connection in connections)
					{
						AddConnection(folder, connection, false);
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message);
			}
		}

		/// <summary>
		/// Adds a DX connection to the tree.
		/// </summary>
		private void AddConnection(TreeNode parent, Opc.Dx.DXConnection connection, bool ignoreBrowsePath)
		{
			// add connection at root if no browse paths specified.
			if (ignoreBrowsePath || connection.BrowsePaths.Count == 0)
			{
				TreeNode node = new TreeNode(connection.Name);

				node.ImageIndex         = Resources.IMAGE_YELLOW_SCROLL;
				node.SelectedImageIndex = Resources.IMAGE_YELLOW_SCROLL;
			
				node.Tag = new NodeTag(NodeType.Connection, connection);
			
				parent.Nodes.Add(node);
				return;
			}

			// add connect to the tree once for each browse path specified.
			foreach (string browsePath in connection.BrowsePaths)
			{
				string folder  = browsePath;
				string subpath = "";

				TreeNode folderNode = parent;

				// build the folders required for the browse path.
				do
				{
					int index = folder.IndexOf("/");

					if (index != -1)
					{
						subpath = folder.Substring(index+1);
						folder  = folder.Substring(0, index);
					}
					else
					{
						subpath = "";
					}

					if (folder.Length > 0)
					{
						// check if the folder already exists.
						bool found = false;

						foreach (TreeNode child in folderNode.Nodes)
						{
							if (child.Text == folder)
							{
								folderNode = child;
								found = true;
								break;
							}
						}

						// create a new folder.
						if (!found)
						{								
							TreeNode child = new TreeNode(folder);

							child.ImageIndex         = Resources.IMAGE_CLOSED_BLUE_FOLDER;
							child.SelectedImageIndex = Resources.IMAGE_OPEN_BLUE_FOLDER;
			
							child.Tag = new NodeTag(NodeType.ConnectionsFolder, browsePath);
			
							folderNode.Nodes.Add(child);
							
							folderNode = child;
						}
					}

					folder = subpath;
				}
				while (subpath.Length > 0);

				// add connection to folder referenced by the browse path.
				TreeNode node = new TreeNode(connection.Name);

				node.ImageIndex         = Resources.IMAGE_YELLOW_SCROLL;
				node.SelectedImageIndex = Resources.IMAGE_YELLOW_SCROLL;
			
				node.Tag = new NodeTag(NodeType.Connection, connection);
			
				folderNode.Nodes.Add(node);
			}
		}

		/// <summary>
		/// Creates a folder to contain connection queries.
		/// </summary>
		private void LoadQueries(TreeNode parent, Opc.Dx.Server server)
		{
			TreeNode folder = new TreeNode("Queries");

			folder.ImageIndex         = Resources.IMAGE_CLOSED_YELLOW_FOLDER;
			folder.SelectedImageIndex = Resources.IMAGE_OPEN_YELLOW_FOLDER;
			
			folder.Tag = new NodeTag(NodeType.QueriesFolder, null);
			
			parent.Nodes.Add(folder);

			folder.EnsureVisible();

			// add saved queries.
			UpdateQueries(server);
		}

		/// <summary>
		/// Adds the connection queries from the DX server and builds the tree.
		/// </summary>
		private void UpdateQueries(Opc.Dx.Server server)
		{
			// find root folder for queries.
			TreeNode folder = FindFolder(NodeType.QueriesFolder);

			folder.Nodes.Clear();
			
			// add queries to tree.
			foreach (Opc.Dx.DXConnectionQuery query in server.Queries)
			{
				TreeNode node = new TreeNode(query.Name);

				node.ImageIndex         = Resources.IMAGE_LIST_BOX;
				node.SelectedImageIndex = Resources.IMAGE_LIST_BOX;
			
				node.Tag = new NodeTag(NodeType.Query, query);
			
				folder.Nodes.Add(node);
			}

			folder.Expand();
		}

		/// <summary>
		/// Finds the root folder with the specified node type.
		/// </summary>
		private TreeNode FindFolder(NodeType nodeType)
		{
			foreach (TreeNode root in ConnectionsTV.Nodes)
			{
				foreach (TreeNode child in root.Nodes)
				{
					if (typeof(NodeTag).IsInstanceOfType(child.Tag))
					{
						if (((NodeTag)child.Tag).NodeType == nodeType)
						{
							return child;
						}
					}
				}
			}

			return null;
		}
		
		/// <summary>
		/// Displays any errors in the error array.
		/// </summary>
		private void ShowErrors(ResultID[] errors)
		{
			if (errors != null)
			{
				// compile a string with all errors.
				StringBuilder message = new StringBuilder();

				for (int ii = 0; ii < errors.Length; ii++)
				{
					if (errors[ii].Failed())
					{
						if (message.Length > 0)
						{
							message.Append("\r\n");
						}

						message.AppendFormat("[{0}]\t{1}", ii, errors[ii]);
					}
				}

				// display errors.
				if (message.Length > 0)
				{
					MessageBox.Show(message.ToString(), "Show DX Connection Mask Errors", MessageBoxButtons.OK);
				}
			}
		}

		/// <summary>
		/// Displays the response.
		/// </summary>
		private void ShowResponse(GeneralResponse response)
		{
			if (response != null)
			{
				// compile a string with all errors.
				StringBuilder message = new StringBuilder();

				for (int ii = 0; ii < response.Count; ii++)
				{
					if (message.Length > 0)
					{
						message.Append("\r\n");
					}

					message.AppendFormat("{0}\t[{1}]", response[ii].ItemName, response[ii].ResultID);
				}

				// display errors.
				if (message.Length > 0)
				{
					MessageBox.Show(message.ToString(), "DX General Response", MessageBoxButtons.OK);
				}
			}
		}
		#endregion

		#region Windows Control Event Handlers
		/// <summary>
		/// Updates the state of context menus based on the current selection.
		/// </summary>
		private void ConnectionsTV_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try
			{
				// ignore left button actions.
				if (e.Button != MouseButtons.Right)	return;

				// selects the item that was right clicked on.
				TreeNode clickedNode = ConnectionsTV.GetNodeAt(e.X, e.Y);

				// no item clicked on - do nothing.
				if (clickedNode == null) return;

				// force selection to clicked node.
				ConnectionsTV.SelectedNode = clickedNode;

				// disable everything.
				ReloadConfigurationMI.Enabled = false;
				ResetConfigurationMI.Enabled  = false;
				AddSourceServerMI.Enabled     = false;
				ModifySourceServersMI.Enabled = false;
				ModifySourceServerMI.Enabled  = false;
				DeleteSourceServerMI.Enabled  = false;
				AddQueryMI.Enabled            = false;
				ModifyQueryMI.Enabled         = false;
				DeleteQueryMI.Enabled         = false;
				AddConnectionMI.Enabled       = false;
				ModifyConnectionsMI.Enabled   = false;
				ModifyConnectionMI.Enabled    = false;
				DeleteConnectionMI.Enabled    = false;
				QueryConnectionsMI.Enabled    = false;
				UpdateConnectionsMI.Enabled   = false;
				DeleteConnectionsMI.Enabled   = false;

				// check for a valid node tag.
				if (!typeof(NodeTag).IsInstanceOfType(clickedNode.Tag))
				{
					return;
				}

				NodeTag tag = (NodeTag)clickedNode.Tag;

				// enable menu according to the current node type.
				switch (tag.NodeType)
				{
					case NodeType.Server:
					{
						ReloadConfigurationMI.Enabled = true;
						ResetConfigurationMI.Enabled  = true;
						break;
					}

					case NodeType.SourceServersFolder:
					{
						AddSourceServerMI.Enabled     = true;
						ModifySourceServersMI.Enabled = true;
						break;
					}

					case NodeType.SourceServer:
					{
						ModifySourceServerMI.Enabled = true;
						DeleteSourceServerMI.Enabled = true;
						break;
					}

					case NodeType.ConnectionsFolder:
					{
						AddConnectionMI.Enabled     = true;
						ModifyConnectionsMI.Enabled = true;
						break;
					}

					case NodeType.Connection:
					{
						ModifyConnectionMI.Enabled = true;
						DeleteConnectionMI.Enabled = true;
						break;
					}

					case NodeType.QueriesFolder:
					{
						AddQueryMI.Enabled = true;
						break;
					}

					case NodeType.Query:
					{
						ModifyQueryMI.Enabled       = true;
						DeleteQueryMI.Enabled       = true;
						QueryConnectionsMI.Enabled  = true;
						UpdateConnectionsMI.Enabled = true;
						DeleteConnectionsMI.Enabled = true;
						break;
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}	

		/// <summary>
		/// Fires events indicating trends or items have been selected.  
		/// </summary>
		private void ConnectionsTV_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			try
			{
				// selects the item that was right clicked on.
				TreeNode clickedNode = e.Node;

				// check for a valid node tag.
				if (!typeof(NodeTag).IsInstanceOfType(clickedNode.Tag))
				{
					return;
				}

				NodeTag tag = (NodeTag)clickedNode.Tag;

				// raise event.
				if (m_configurationEvent != null)
				{
					m_configurationEvent(this, new ConfigurationEventArgs(NodeAction.Selected, tag.NodeType, tag.NodeData));
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		private void ReloadConfigurationMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Server)
				{
					return;
				}

				// reload configuration.
				LoadConfiguration(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		private void ResetConfigurationMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Server)
				{
					return;
				}

				// reload configuration.
				m_target.ResetConfiguration(null);

				// reload configuration.
				LoadConfiguration(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void AddSourceServerMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.SourceServersFolder)
				{
					return;
				}

				SourceServer[] sourceServers = new SourceServer[1];
				sourceServers[0] = new SourceServer();

				ArrayList sourceServersToAdd = new ArrayList();

				do 
				{
					// prompt user.
					sourceServers = new SourceServerListEditDlg().ShowDialog(sourceServers, false);

					// check for cancel.
					if (sourceServers == null || sourceServers.Length == 0)
					{
						return;
					}

					// update server.
					GeneralResponse response = m_target.AddSourceServers(sourceServers);

					if (response == null || response.Count != sourceServers.Length)
					{
						throw new ResultIDException(ResultID.E_FAIL);
					}			

					// check for item level errors.
					sourceServersToAdd.Clear();

					StringBuilder message = new StringBuilder();

					for (int ii = 0; ii < sourceServers.Length; ii++)
					{
						if (response[ii].ResultID.Failed())
						{
							if (message.Length > 0)
							{
								message.Append("\r\n");
							}

							message.Append(sourceServers[ii].Name);
							message.AppendFormat("\t{0}", response[ii].ResultID);
						}
						else
						{
							sourceServers[ii].ItemName = response[ii].ItemName;
							sourceServers[ii].ItemPath = response[ii].ItemPath;
							sourceServers[ii].Version  = response[ii].Version;

							sourceServersToAdd.Add(sourceServers[ii]);
						}
					}

					// display errors.
					if (message.Length == 0)
					{
						break;
					}
					
					// prompt with errors.
					if (MessageBox.Show(message.ToString(), "Add DX Source Server", MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						break;
					}

					// delete source servers that were added successfully.
					if (sourceServersToAdd.Count > 0)
					{
						m_target.DeleteSourceServers((SourceServer[])sourceServersToAdd.ToArray(typeof(SourceServer)));
					}
				}
				while (sourceServers != null);

				// update source servers tree.
				UpdateSourceServers(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		private void ModifySourceServersMI_Click(object sender, System.EventArgs e)
		{
			try
			{	// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.SourceServersFolder)
				{
					return;
				}

				// find connections in folder.
				ArrayList sourceServerList = new ArrayList();
				
				foreach (TreeNode child in node.Nodes)
				{
					tag = (NodeTag)child.Tag;

					if (tag.NodeType == NodeType.SourceServer)
					{
						sourceServerList.Add(tag.NodeData);
					}

				}

				// convert list to an array.
				SourceServer[] sourceServers = (SourceServer[])sourceServerList.ToArray(typeof(SourceServer));

				if (sourceServers.Length == 0)
				{
					return;
				}

				do 
				{
					// prompt user.
					sourceServers = new SourceServerListEditDlg().ShowDialog(sourceServers, true);

					// check for cancel.
					if (sourceServers == null || sourceServers.Length == 0)
					{
						return;
					}

					// update server.
					GeneralResponse response = m_target.ModifySourceServers(sourceServers);

					if (response == null || response.Count != sourceServers.Length)
					{
						throw new ResultIDException(ResultID.E_FAIL);
					}			

					// check for item level errors.
					StringBuilder message = new StringBuilder();

					for (int ii = 0; ii < sourceServers.Length; ii++)
					{
						if (response[ii].ResultID.Failed())
						{
							if (message.Length > 0)
							{
								message.Append("\r\n");
							}

							message.Append(sourceServers[ii].Name);
							message.AppendFormat("\t{0}", response[ii].ResultID);
						}
					}

					// display errors.
					if (message.Length == 0)
					{
						break;
					}
					
					// prompt with errors.
					if (MessageBox.Show(message.ToString(), "Modify DX Source Server", MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						break;
					}
				}
				while (sourceServers != null);

				// update the source server tree.
				UpdateSourceServers(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		private void ModifySourceServerMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.SourceServer)
				{
					return;
				}

				// prompt user.
				SourceServer[] sourceServers = new SourceServer[] { (SourceServer)tag.NodeData };

				sourceServers = new SourceServerListEditDlg().ShowDialog(sourceServers, true);

				// check for cancel.
				if (sourceServers == null || sourceServers.Length == 0)
				{
					return;
				}

				// update server.
				GeneralResponse response = m_target.ModifySourceServers(sourceServers);

				if (response == null || response.Count != 1)
				{
					throw new ResultIDException(ResultID.E_FAIL);
				}			

				// check for error.
				if (response[0].ResultID.Failed())
				{
					throw new ResultIDException(response[0].ResultID);
				}
				
				// update the source server tree.
				UpdateSourceServers(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void DeleteSourceServerMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.SourceServer)
				{
					return;
				}

				// prompt user.
				SourceServer[] sourceServers = new SourceServer[1];
				
				sourceServers[0]          = new SourceServer();
				sourceServers[0].ItemName = ((SourceServer)tag.NodeData).ItemName;
				sourceServers[0].ItemPath = ((SourceServer)tag.NodeData).ItemPath;
				sourceServers[0].Version  = ((SourceServer)tag.NodeData).Version;

				// update server.
				GeneralResponse response = m_target.DeleteSourceServers(sourceServers);

				if (response == null || response.Count != 1)
				{
					throw new ResultIDException(ResultID.E_FAIL);
				}			

				// check for error.
				if (response[0].ResultID.Failed())
				{
					throw new ResultIDException(response[0].ResultID);
				}
				
				// update the source server tree.
				UpdateSourceServers(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void AddConnectionMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.ConnectionsFolder)
				{
					return;
				}

				DXConnection[] connections = new DXConnection[1];

				connections[0] = new DXConnection();
				connections[0].BrowsePaths.Add((tag.NodeData != null)?tag.NodeData:"");

				ArrayList connectionsToAdd = new ArrayList();

				do 
				{
					// prompt user.
					connections = new ConnectionListEditDlg().ShowDialog(m_target, connections, false, false);

					// check for cancel.
					if (connections == null || connections.Length == 0)
					{
						return;
					}

					// update the source server tree.
					UpdateSourceServers(m_target);

					// update server.
					GeneralResponse response = m_target.AddDXConnections(connections);

					if (response == null || response.Count != connections.Length)
					{
						throw new ResultIDException(ResultID.E_FAIL);
					}			

					// check for item level errors.
					connectionsToAdd.Clear();

					StringBuilder message = new StringBuilder();

					for (int ii = 0; ii < connections.Length; ii++)
					{
						if (response[ii].ResultID.Failed())
						{
							if (message.Length > 0)
							{
								message.Append("\r\n");
							}

							message.Append(connections[ii].Name);
							message.AppendFormat("\t{0}", response[ii].ResultID);
						}
						else
						{
							connections[ii].ItemName = response[ii].ItemName;
							connections[ii].ItemPath = response[ii].ItemPath;
							connections[ii].Version  = response[ii].Version;

							connectionsToAdd.Add(connections[ii]);
						}
					}

					// display errors.
					if (message.Length == 0)
					{
						break;
					}
					
					// prompt with errors.
					if (MessageBox.Show(message.ToString(), "Add DX Connections", MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						break;
					}

					// delete connections that were added successfully.
					ResultID[] errors = null;

					m_target.DeleteDXConnections(
						null, 
						(DXConnection[])connectionsToAdd.ToArray(typeof(DXConnection)), 
						false, 
						out errors);
				}
				while (connections != null);

				// update connections tree.
				UpdateConnections(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void ModifyConnectionsMI_Click(object sender, System.EventArgs e)
		{
			try
			{	// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.ConnectionsFolder)
				{
					return;
				}

				// find connections in folder.
				ArrayList connectionList = new ArrayList();
				
				foreach (TreeNode child in node.Nodes)
				{
					tag = (NodeTag)child.Tag;

					if (tag.NodeType == NodeType.Connection)
					{
						connectionList.Add(tag.NodeData);
					}

				}

				// convert list to an array.
				DXConnection[] connections = (DXConnection[])connectionList.ToArray(typeof(DXConnection));

				if (connections.Length == 0)
				{
					return;
				}

				do 
				{
					// prompt user.
					connections = new ConnectionListEditDlg().ShowDialog(m_target, connections, false, true);

					// check for cancel.
					if (connections == null || connections.Length == 0)
					{
						return;
					}

					// update the source server tree.
					UpdateSourceServers(m_target);

					// update server.
					GeneralResponse response = m_target.ModifyDXConnections(connections);

					if (response == null || response.Count != connections.Length)
					{
						throw new ResultIDException(ResultID.E_FAIL);
					}			

					// check for item level errors.
					StringBuilder message = new StringBuilder();

					for (int ii = 0; ii < connections.Length; ii++)
					{
						if (response[ii].ResultID.Failed())
						{
							if (message.Length > 0)
							{
								message.Append("\r\n");
							}

							message.Append(connections[ii].Name);
							message.AppendFormat("\t{0}", response[ii].ResultID);
						}
					}

					// display errors.
					if (message.Length == 0)
					{
						break;
					}
					
					// prompt with errors.
					if (MessageBox.Show(message.ToString(), "Modify DX Connections", MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						break;
					}
				}
				while (connections != null);

				// update connections tree.
				UpdateConnections(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void AddQueryMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.QueriesFolder)
				{
					return;
				}

				// create default query.
				DXConnectionQuery[] queries = new DXConnectionQuery[1];
				queries[0] = new DXConnectionQuery();

				// prompt user.
				queries = new ConnectionQueryListEditDlg().ShowDialog(queries, true);

				// check for cancel.
				if (queries == null || queries.Length != 1)
				{
					return;
				}

				// update server.
				m_target.Queries.Add(queries[0]);

				// update queries tree.
				UpdateQueries(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		private void ModifyQueryMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Query)
				{
					return;
				}

				// create query to modify.
				DXConnectionQuery[] queries = new DXConnectionQuery[1];
				queries[0] = (DXConnectionQuery)tag.NodeData;

				// prompt user.
				queries = new ConnectionQueryListEditDlg().ShowDialog(queries, true);

				// check for cancel.
				if (queries == null || queries.Length != 1)
				{
					return;
				}

				// remove existing query.
				m_target.Queries.Remove(tag.NodeData);

				// add updated query.
				m_target.Queries.Add(queries[0]);

				// update queries tree.
				UpdateQueries(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}	
		}

		private void DeleteQueryMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Query)
				{
					return;
				}

				// remove query.
				m_target.Queries.Remove(tag.NodeData);

				// update queries tree.
				UpdateQueries(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void ModifyConnectionMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Connection)
				{
					return;
				}

				// prompt user.
				DXConnection[] connections = new DXConnection[] { (DXConnection)tag.NodeData };

				connections = new ConnectionListEditDlg().ShowDialog(m_target, connections, false, true);

				// check for cancel.
				if (connections == null || connections.Length == 0)
				{
					return;
				}

				// update server.
				GeneralResponse response = m_target.ModifyDXConnections(connections);

				if (response == null || response.Count != 1)
				{
					throw new ResultIDException(ResultID.E_FAIL);
				}			

				// check for error.
				if (response[0].ResultID.Failed())
				{
					throw new ResultIDException(response[0].ResultID);
				}
				
				// update connections tree.
				UpdateConnections(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}

		private void DeleteConnectionMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Connection)
				{
					return;
				}

				// prompt user.
				DXConnection[] connections = new DXConnection[1];
				
				connections[0]          = new DXConnection();
				connections[0].ItemName = ((DXConnection)tag.NodeData).ItemName;
				connections[0].ItemPath = ((DXConnection)tag.NodeData).ItemPath;
				connections[0].Version  = ((DXConnection)tag.NodeData).Version;

				// update server.
				ResultID[] errors = null;

				GeneralResponse response = m_target.DeleteDXConnections(
					null,
					connections,
					true,
					out errors);

				// show errors.
				ShowErrors(errors);

				if (response == null || response.Count != 1)
				{
					throw new ResultIDException(ResultID.E_FAIL);
				}			

				// check for error.
				if (response[0].ResultID.Failed())
				{
					throw new ResultIDException(response[0].ResultID);
				}
				
				// update connections tree.
				UpdateConnections(m_target);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}	
		}

		private void QueryConnectionsMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Query)
				{
					return;
				}

				DXConnectionQuery query = (DXConnectionQuery)tag.NodeData;

				// run query.
				ResultID[] errors = null;

				DXConnection[] connections = query.Query(m_target, out errors);

				// show errors.
				ShowErrors(errors);

				// update node.
				node.Nodes.Clear();

				if (connections != null)
				{
					foreach (DXConnection connection in connections)
					{
						AddConnection(node, connection, true);
					}
				}		
		
				node.Expand();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		private void UpdateConnectionsMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Query)
				{
					return;
				}

				DXConnection[] connections = new DXConnection[1];
				connections[0] = new DXConnection();

				// prompt user.
				connections = new ConnectionListEditDlg().ShowDialog(m_target, connections, true, true);

				// check for cancel.
				if (connections == null || connections.Length == 0)
				{
					return;
				}

				// update the source server tree.
				UpdateSourceServers(m_target);

				// run update.
				DXConnectionQuery query = (DXConnectionQuery)tag.NodeData;

				ResultID[] errors = null;

				GeneralResponse response = query.Update(m_target, connections[0], out errors);

				// show errors.
				ShowErrors(errors);

				// show response.
				ShowResponse(response);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}			
		}

		private void DeleteConnectionsMI_Click(object sender, System.EventArgs e)
		{
			try
			{
				// check for valid selection.
				TreeNode node = ConnectionsTV.SelectedNode;

				if (node == null || !typeof(NodeTag).IsInstanceOfType(node.Tag))
				{
					return;
				}

				// check selection that is the correct type.
				NodeTag tag = (NodeTag)node.Tag;

				if (tag.NodeType != NodeType.Query)
				{
					return;
				}

				// run update.
				DXConnectionQuery query = (DXConnectionQuery)tag.NodeData;

				ResultID[] errors = null;

				GeneralResponse response = query.Delete(m_target, out errors);

				// show errors.
				ShowErrors(errors);

				// show response.
				ShowResponse(response);
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}		
		}
		#endregion
	}
}
