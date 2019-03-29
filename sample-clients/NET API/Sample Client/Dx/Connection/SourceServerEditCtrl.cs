//============================================================================
// TITLE: SourceServerEditCtrl.cs
//
// CONTENTS:
// 
// A control used to display and edit the contents of an SourceServer object.
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
using System.Globalization;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Opc.Da;
using Opc.Cpx;
using Opc.SampleClient;

namespace Opc.Dx.SampleClient
{
	/// <summary>
	/// A control used to display and edit the contents of an Item object.
	/// </summary>
	public class SourceServerEditCtrl : System.Windows.Forms.UserControl, IEditObjectCtrl
	{
		private System.Windows.Forms.CheckBox DescriptionSpecifiedCB;
		private System.Windows.Forms.TextBox DescriptionTB;
		private System.Windows.Forms.CheckBox ServerNameSpecifiedCB;
		private System.Windows.Forms.TextBox ServerNameTB;
		private System.Windows.Forms.Label DescriptionLB;
		private System.Windows.Forms.CheckBox ServerUrlSpecifiedCB;
		private System.Windows.Forms.TextBox ServerUrlTB;
		private System.Windows.Forms.Label ServerUrlLB;
		private System.Windows.Forms.Label ServerNameLB;
		private System.Windows.Forms.CheckBox ServerTypeSpecifiedCB;
		private System.Windows.Forms.Label ServerTypeLB;
		private System.Windows.Forms.ComboBox ServerTypeCB;
		private System.Windows.Forms.CheckBox DefaultConnectedSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultConnectedCB;
		private System.Windows.Forms.Label DefaultConnectedLB;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SourceServerEditCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			ServerTypeCB.Items.AddRange(Opc.Dx.ServerType.Enumerate());
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
			this.ServerUrlSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.ServerUrlTB = new System.Windows.Forms.TextBox();
			this.ServerUrlLB = new System.Windows.Forms.Label();
			this.DescriptionSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DescriptionTB = new System.Windows.Forms.TextBox();
			this.DefaultConnectedSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultConnectedCB = new System.Windows.Forms.CheckBox();
			this.ServerNameSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.ServerNameTB = new System.Windows.Forms.TextBox();
			this.DescriptionLB = new System.Windows.Forms.Label();
			this.DefaultConnectedLB = new System.Windows.Forms.Label();
			this.ServerNameLB = new System.Windows.Forms.Label();
			this.ServerTypeSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.ServerTypeLB = new System.Windows.Forms.Label();
			this.ServerTypeCB = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// ServerUrlSpecifiedCB
			// 
			this.ServerUrlSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ServerUrlSpecifiedCB.Location = new System.Drawing.Point(336, 48);
			this.ServerUrlSpecifiedCB.Name = "ServerUrlSpecifiedCB";
			this.ServerUrlSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.ServerUrlSpecifiedCB.TabIndex = 110;
			this.ServerUrlSpecifiedCB.Click += new System.EventHandler(this.Specified_CheckedChanged);
			this.ServerUrlSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// ServerUrlTB
			// 
			this.ServerUrlTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ServerUrlTB.Enabled = false;
			this.ServerUrlTB.Location = new System.Drawing.Point(104, 48);
			this.ServerUrlTB.Name = "ServerUrlTB";
			this.ServerUrlTB.Size = new System.Drawing.Size(224, 20);
			this.ServerUrlTB.TabIndex = 109;
			this.ServerUrlTB.Text = "";
			// 
			// ServerUrlLB
			// 
			this.ServerUrlLB.Location = new System.Drawing.Point(0, 48);
			this.ServerUrlLB.Name = "ServerUrlLB";
			this.ServerUrlLB.Size = new System.Drawing.Size(104, 23);
			this.ServerUrlLB.TabIndex = 108;
			this.ServerUrlLB.Text = "Server URL";
			this.ServerUrlLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DescriptionSpecifiedCB
			// 
			this.DescriptionSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionSpecifiedCB.Location = new System.Drawing.Point(336, 24);
			this.DescriptionSpecifiedCB.Name = "DescriptionSpecifiedCB";
			this.DescriptionSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DescriptionSpecifiedCB.TabIndex = 92;
			this.DescriptionSpecifiedCB.Click += new System.EventHandler(this.Specified_CheckedChanged);
			this.DescriptionSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DescriptionTB
			// 
			this.DescriptionTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionTB.Enabled = false;
			this.DescriptionTB.Location = new System.Drawing.Point(104, 24);
			this.DescriptionTB.Name = "DescriptionTB";
			this.DescriptionTB.Size = new System.Drawing.Size(224, 20);
			this.DescriptionTB.TabIndex = 90;
			this.DescriptionTB.Text = "";
			// 
			// DefaultConnectedSpecifiedCB
			// 
			this.DefaultConnectedSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultConnectedSpecifiedCB.Location = new System.Drawing.Point(336, 96);
			this.DefaultConnectedSpecifiedCB.Name = "DefaultConnectedSpecifiedCB";
			this.DefaultConnectedSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultConnectedSpecifiedCB.TabIndex = 81;
			this.DefaultConnectedSpecifiedCB.Click += new System.EventHandler(this.Specified_CheckedChanged);
			this.DefaultConnectedSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultConnectedCB
			// 
			this.DefaultConnectedCB.Enabled = false;
			this.DefaultConnectedCB.Location = new System.Drawing.Point(104, 96);
			this.DefaultConnectedCB.Name = "DefaultConnectedCB";
			this.DefaultConnectedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultConnectedCB.TabIndex = 80;
			// 
			// ServerNameSpecifiedCB
			// 
			this.ServerNameSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ServerNameSpecifiedCB.Location = new System.Drawing.Point(336, 0);
			this.ServerNameSpecifiedCB.Name = "ServerNameSpecifiedCB";
			this.ServerNameSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.ServerNameSpecifiedCB.TabIndex = 78;
			this.ServerNameSpecifiedCB.Click += new System.EventHandler(this.Specified_CheckedChanged);
			this.ServerNameSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// ServerNameTB
			// 
			this.ServerNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ServerNameTB.Enabled = false;
			this.ServerNameTB.Location = new System.Drawing.Point(104, 0);
			this.ServerNameTB.Name = "ServerNameTB";
			this.ServerNameTB.Size = new System.Drawing.Size(224, 20);
			this.ServerNameTB.TabIndex = 77;
			this.ServerNameTB.Text = "";
			// 
			// DescriptionLB
			// 
			this.DescriptionLB.Location = new System.Drawing.Point(0, 24);
			this.DescriptionLB.Name = "DescriptionLB";
			this.DescriptionLB.Size = new System.Drawing.Size(104, 23);
			this.DescriptionLB.TabIndex = 75;
			this.DescriptionLB.Text = "Description";
			this.DescriptionLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultConnectedLB
			// 
			this.DefaultConnectedLB.Location = new System.Drawing.Point(0, 96);
			this.DefaultConnectedLB.Name = "DefaultConnectedLB";
			this.DefaultConnectedLB.Size = new System.Drawing.Size(104, 23);
			this.DefaultConnectedLB.TabIndex = 66;
			this.DefaultConnectedLB.Text = "Default Connected";
			this.DefaultConnectedLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ServerNameLB
			// 
			this.ServerNameLB.Location = new System.Drawing.Point(0, 0);
			this.ServerNameLB.Name = "ServerNameLB";
			this.ServerNameLB.Size = new System.Drawing.Size(104, 23);
			this.ServerNameLB.TabIndex = 64;
			this.ServerNameLB.Text = "Server Name";
			this.ServerNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ServerTypeSpecifiedCB
			// 
			this.ServerTypeSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ServerTypeSpecifiedCB.Location = new System.Drawing.Point(336, 72);
			this.ServerTypeSpecifiedCB.Name = "ServerTypeSpecifiedCB";
			this.ServerTypeSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.ServerTypeSpecifiedCB.TabIndex = 113;
			this.ServerTypeSpecifiedCB.Click += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// ServerTypeLB
			// 
			this.ServerTypeLB.Location = new System.Drawing.Point(0, 72);
			this.ServerTypeLB.Name = "ServerTypeLB";
			this.ServerTypeLB.Size = new System.Drawing.Size(104, 23);
			this.ServerTypeLB.TabIndex = 111;
			this.ServerTypeLB.Text = "Server Type";
			this.ServerTypeLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ServerTypeCB
			// 
			this.ServerTypeCB.Location = new System.Drawing.Point(104, 72);
			this.ServerTypeCB.Name = "ServerTypeCB";
			this.ServerTypeCB.Size = new System.Drawing.Size(176, 21);
			this.ServerTypeCB.TabIndex = 114;
			// 
			// SourceServerEditCtrl
			// 
			this.Controls.Add(this.ServerTypeCB);
			this.Controls.Add(this.ServerTypeSpecifiedCB);
			this.Controls.Add(this.ServerTypeLB);
			this.Controls.Add(this.ServerUrlSpecifiedCB);
			this.Controls.Add(this.ServerUrlTB);
			this.Controls.Add(this.ServerUrlLB);
			this.Controls.Add(this.DescriptionSpecifiedCB);
			this.Controls.Add(this.DescriptionTB);
			this.Controls.Add(this.DefaultConnectedSpecifiedCB);
			this.Controls.Add(this.DefaultConnectedCB);
			this.Controls.Add(this.ServerNameSpecifiedCB);
			this.Controls.Add(this.ServerNameTB);
			this.Controls.Add(this.DescriptionLB);
			this.Controls.Add(this.DefaultConnectedLB);
			this.Controls.Add(this.ServerNameLB);
			this.Name = "SourceServerEditCtrl";
			this.Size = new System.Drawing.Size(352, 120);
			this.ResumeLayout(false);

		}
		#endregion

		#region IEditObjectCtrl Interface
		/// <summary>
		/// Set all fields to default values.
		/// </summary>
		public void SetDefaults()
		{
			ServerNameTB.Text = "";
		    DescriptionTB.Text = "";
			ServerTypeCB.SelectedItem = null;
			ServerUrlTB.Text = "";
			DefaultConnectedCB.Checked = false;
		}

		/// <summary>
		/// Copy values from control into object - throw exceptions on error.
		/// </summary>
		public object Get()
		{
			Opc.Dx.SourceServer sourceServer = new Opc.Dx.SourceServer(m_identifier);

			// set output default values.
			sourceServer.Name = null;
			sourceServer.Description = null;
			sourceServer.ServerType = null;
			sourceServer.ServerURL = null;
			sourceServer.DefaultConnected = false;
			sourceServer.DefaultConnectedSpecified = false;

			// name
			if (ServerNameSpecifiedCB.Checked)
			{
				sourceServer.Name = ServerNameTB.Text;
			}

			// description
			if (DescriptionSpecifiedCB.Checked)
			{
				sourceServer.Description = DescriptionTB.Text;
			}

			// description
			if (DescriptionSpecifiedCB.Checked)
			{
				sourceServer.Description = DescriptionTB.Text;
			}

			// server type
			if (ServerTypeSpecifiedCB.Checked)
			{
				sourceServer.ServerType = (string)ServerTypeCB.SelectedItem;
			}

			// server url
			if (ServerUrlSpecifiedCB.Checked)
			{
				sourceServer.ServerURL = ServerUrlTB.Text;
			}

			// default connected
			if (DefaultConnectedSpecifiedCB.Checked)
			{
				sourceServer.DefaultConnected = DefaultConnectedCB.Checked;
				sourceServer.DefaultConnectedSpecified = true;
			}

			return sourceServer;
		}
		
		/// <summary>
		/// Copy object values into controls.
		/// </summary>
		public void Set(object value)
		{
			// check for valid value.
			if (!typeof(Opc.Dx.SourceServer).IsInstanceOfType(value)) 
			{ 
				SetDefaults(); 
				return;
			}

			Opc.Dx.SourceServer sourceServer = (Opc.Dx.SourceServer)value;

			// save item identifier (including client and server handles).
			m_identifier = new Opc.Dx.ItemIdentifier(sourceServer);

			ServerNameTB.Text          = sourceServer.Name;
			DescriptionTB.Text         = sourceServer.Description;
			ServerTypeCB.SelectedItem  = sourceServer.ServerType;
			ServerUrlTB.Text           = sourceServer.ServerURL;
			DefaultConnectedCB.Checked = sourceServer.DefaultConnected;

			ServerNameSpecifiedCB.Checked       = true;
			DescriptionSpecifiedCB.Checked      = true;
			ServerTypeSpecifiedCB.Checked       = true;
			ServerUrlSpecifiedCB.Checked        = true;
			DefaultConnectedSpecifiedCB.Checked = true;
		}

		/// <summary>
		/// Creates a new object.
		/// </summary>
		public object Create()
		{
			Opc.Dx.SourceServer sourceServer = new Opc.Dx.SourceServer();

			return sourceServer;
		}
		#endregion

		#region Private Members
		private Opc.Dx.ItemIdentifier m_identifier = null;
		#endregion

		/// <summary>
		/// Toggles the enabled state of controls based on the specified check boxes.
		/// </summary>
		private void Specified_CheckedChanged(object sender, System.EventArgs e)
		{			
			ServerNameTB.Enabled = ServerNameSpecifiedCB.Checked;
			DescriptionTB.Enabled = DescriptionSpecifiedCB.Checked;
			ServerTypeCB.Enabled = DescriptionSpecifiedCB.Checked;
			ServerUrlTB.Enabled = DescriptionSpecifiedCB.Checked;
			DefaultConnectedCB.Enabled = DefaultConnectedSpecifiedCB.Checked;
		}
	}
}
