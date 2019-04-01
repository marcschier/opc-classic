//============================================================================
// TITLE: ConnectionListEditDlg.cs
//
// CONTENTS:
// 
// A dialog used to display and edit a list of DXConnection objects.
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Opc.Dx;
using Opc.SampleClient;

namespace Opc.Dx.SampleClient
{
	/// <summary>
	/// A dialog used to display and edit a list of DXConnection objects.
	/// </summary>
	public class ConnectionListEditDlg : EditObjectListDlg
	{
		private Opc.Dx.SampleClient.ConnectionEditCtrl ObjectCTRL;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConnectionListEditDlg()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			m_control = ObjectCTRL;
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ObjectCTRL = new Opc.Dx.SampleClient.ConnectionEditCtrl();
			this.SuspendLayout();
			// 
			// ObjectCTRL
			// 
			this.ObjectCTRL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ObjectCTRL.IsMask = false;
			this.ObjectCTRL.Location = new System.Drawing.Point(4, 4);
			this.ObjectCTRL.Name = "ObjectCTRL";
			this.ObjectCTRL.Size = new System.Drawing.Size(520, 456);
			this.ObjectCTRL.TabIndex = 2;
			this.ObjectCTRL.TargetServer = null;
			// 
			// ConnectionListEditDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 478);
			this.Controls.Add(this.ObjectCTRL);
			this.Name = "ConnectionListEditDlg";
			this.Text = "Edit DX Connections";
			this.Controls.SetChildIndex(this.ObjectCTRL, 0);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Prompts the user to edit the item list parameters.
		/// </summary>
		public DXConnection[] ShowDialog(
			Opc.Dx.Server  target, 
			DXConnection[] connections, 
			bool           isMask,
			bool           fixedLength)
		{
			ObjectCTRL.TargetServer  = target;
			ObjectCTRL.IsMask        = isMask;

			if (connections == null) 
			{
				connections = new DXConnection[] { (DXConnection)ObjectCTRL.Create() };
			}

			ArrayList results = base.ShowDialog((object[])connections, fixedLength);

			if (results != null && results.Count > 0)
			{
				return (DXConnection[])results.ToArray(typeof(DXConnection));
			}

			return null;
		}
	}
}
