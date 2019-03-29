//============================================================================
// TITLE: ConnectionQueryEditCtrl.cs
//
// CONTENTS:
// 
// A control used to display and edit the contents of an Item object.
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
	public class ConnectionQueryEditCtrl : System.Windows.Forms.UserControl, IEditObjectCtrl
	{
		private System.Windows.Forms.Button QueryMasksBTN;
		private System.Windows.Forms.TextBox ConnectionQueryNameTB;
		private System.Windows.Forms.Label ConnectionQueryNameLB;
		private System.Windows.Forms.Label QueryMasksLB;
		private System.Windows.Forms.TextBox BrowsePathTB;
		private System.Windows.Forms.CheckBox RecursiveCB;
		private System.Windows.Forms.Label BrowsePathLB;
		private System.Windows.Forms.Label RecursiveLB;
		private System.Windows.Forms.TextBox QueryMasksTB;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConnectionQueryEditCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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
			this.QueryMasksBTN = new System.Windows.Forms.Button();
			this.QueryMasksLB = new System.Windows.Forms.Label();
			this.BrowsePathTB = new System.Windows.Forms.TextBox();
			this.RecursiveCB = new System.Windows.Forms.CheckBox();
			this.ConnectionQueryNameTB = new System.Windows.Forms.TextBox();
			this.BrowsePathLB = new System.Windows.Forms.Label();
			this.RecursiveLB = new System.Windows.Forms.Label();
			this.ConnectionQueryNameLB = new System.Windows.Forms.Label();
			this.QueryMasksTB = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// QueryMasksBTN
			// 
			this.QueryMasksBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.QueryMasksBTN.Location = new System.Drawing.Point(184, 72);
			this.QueryMasksBTN.Name = "QueryMasksBTN";
			this.QueryMasksBTN.Size = new System.Drawing.Size(25, 20);
			this.QueryMasksBTN.TabIndex = 103;
			this.QueryMasksBTN.Text = "...";
			this.QueryMasksBTN.Click += new System.EventHandler(this.QueryMasksBTN_Click);
			// 
			// QueryMasksLB
			// 
			this.QueryMasksLB.Location = new System.Drawing.Point(0, 72);
			this.QueryMasksLB.Name = "QueryMasksLB";
			this.QueryMasksLB.Size = new System.Drawing.Size(96, 23);
			this.QueryMasksLB.TabIndex = 101;
			this.QueryMasksLB.Text = "Number of Masks";
			this.QueryMasksLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// BrowsePathTB
			// 
			this.BrowsePathTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BrowsePathTB.Location = new System.Drawing.Point(96, 24);
			this.BrowsePathTB.Name = "BrowsePathTB";
			this.BrowsePathTB.Size = new System.Drawing.Size(172, 20);
			this.BrowsePathTB.TabIndex = 90;
			this.BrowsePathTB.Text = "";
			// 
			// RecursiveCB
			// 
			this.RecursiveCB.Location = new System.Drawing.Point(96, 48);
			this.RecursiveCB.Name = "RecursiveCB";
			this.RecursiveCB.Size = new System.Drawing.Size(16, 24);
			this.RecursiveCB.TabIndex = 80;
			// 
			// ConnectionQueryNameTB
			// 
			this.ConnectionQueryNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionQueryNameTB.Location = new System.Drawing.Point(96, 0);
			this.ConnectionQueryNameTB.Name = "ConnectionQueryNameTB";
			this.ConnectionQueryNameTB.Size = new System.Drawing.Size(172, 20);
			this.ConnectionQueryNameTB.TabIndex = 77;
			this.ConnectionQueryNameTB.Text = "";
			// 
			// BrowsePathLB
			// 
			this.BrowsePathLB.Location = new System.Drawing.Point(0, 24);
			this.BrowsePathLB.Name = "BrowsePathLB";
			this.BrowsePathLB.Size = new System.Drawing.Size(96, 23);
			this.BrowsePathLB.TabIndex = 75;
			this.BrowsePathLB.Text = "Browse Path";
			this.BrowsePathLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// RecursiveLB
			// 
			this.RecursiveLB.Location = new System.Drawing.Point(0, 48);
			this.RecursiveLB.Name = "RecursiveLB";
			this.RecursiveLB.Size = new System.Drawing.Size(96, 23);
			this.RecursiveLB.TabIndex = 66;
			this.RecursiveLB.Text = "Recursive";
			this.RecursiveLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ConnectionQueryNameLB
			// 
			this.ConnectionQueryNameLB.Location = new System.Drawing.Point(0, 0);
			this.ConnectionQueryNameLB.Name = "ConnectionQueryNameLB";
			this.ConnectionQueryNameLB.Size = new System.Drawing.Size(96, 23);
			this.ConnectionQueryNameLB.TabIndex = 64;
			this.ConnectionQueryNameLB.Text = "Query Name";
			this.ConnectionQueryNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// QueryMasksTB
			// 
			this.QueryMasksTB.Location = new System.Drawing.Point(96, 72);
			this.QueryMasksTB.Name = "QueryMasksTB";
			this.QueryMasksTB.ReadOnly = true;
			this.QueryMasksTB.Size = new System.Drawing.Size(84, 20);
			this.QueryMasksTB.TabIndex = 104;
			this.QueryMasksTB.Text = "";
			// 
			// ConnectionQueryEditCtrl
			// 
			this.Controls.Add(this.QueryMasksTB);
			this.Controls.Add(this.QueryMasksBTN);
			this.Controls.Add(this.QueryMasksLB);
			this.Controls.Add(this.BrowsePathTB);
			this.Controls.Add(this.RecursiveCB);
			this.Controls.Add(this.ConnectionQueryNameTB);
			this.Controls.Add(this.BrowsePathLB);
			this.Controls.Add(this.RecursiveLB);
			this.Controls.Add(this.ConnectionQueryNameLB);
			this.Name = "ConnectionQueryEditCtrl";
			this.Size = new System.Drawing.Size(272, 96);
			this.ResumeLayout(false);

		}
		#endregion

		#region IEditObjectCtrl Interface
		/// <summary>
		/// Set all fields to default values.
		/// </summary>
		public void SetDefaults()
		{
			ConnectionQueryNameTB.Text = "";
			BrowsePathTB.Text          = "";
			RecursiveCB.Checked        = false;
			QueryMasksTB.Text          = "";
		}

		/// <summary>
		/// Copy values from control into object - throw exceptions on error.
		/// </summary>
		public object Get()
		{
			// update object.
			m_query.Name       = ConnectionQueryNameTB.Text;
			m_query.BrowsePath = BrowsePathTB.Text;
			m_query.Recursive  = RecursiveCB.Checked;

			return m_query;
		}
		
		/// <summary>
		/// Copy object values into controls.
		/// </summary>
		public void Set(object value)
		{
			m_query = null;

			// check for valid value.
			if (!typeof(Opc.Dx.DXConnectionQuery).IsInstanceOfType(value)) 
			{ 
				SetDefaults(); 
				return;
			}

			// copy value.
			m_query = new Opc.Dx.DXConnectionQuery((DXConnectionQuery)value);

			// update controls.
			ConnectionQueryNameTB.Text = m_query.Name;
			BrowsePathTB.Text          = m_query.BrowsePath;
			RecursiveCB.Checked        = m_query.Recursive;
			QueryMasksTB.Text          = m_query.Masks.Count.ToString();
		}

		/// <summary>
		/// Creates a new object.
		/// </summary>
		public object Create()
		{
			return new Opc.Dx.DXConnectionQuery();
		}
		#endregion

		#region Private Members
		private Opc.Dx.DXConnectionQuery m_query = null;
		#endregion

		private void QueryMasksBTN_Click(object sender, System.EventArgs e)
		{		
			try
			{
				// check if nothing to do.
				if (m_query == null)
				{
					return;
				}

				// prompt user.
				DXConnection[] connections = new ConnectionListEditDlg().ShowDialog(null, m_query.Masks.ToArray(), true, false);

				// check for cancel.
				if (connections == null || connections.Length == 0)
				{
					return;
				}

				// update query.
				m_query.Masks.Clear();

				foreach (DXConnection connection in connections)
				{
					m_query.Masks.Add(connection);
				}

				QueryMasksTB.Text = m_query.Masks.Count.ToString();
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}	
	}
}
