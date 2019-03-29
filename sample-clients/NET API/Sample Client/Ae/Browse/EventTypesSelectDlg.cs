//============================================================================
// TITLE: EventTypesSelectDlg.cs
//
// CONTENTS:
// 
// A dialog that displays the current status of an OPC server.
//
// (c) Copyright 2002-2003 The OPC Foundation
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
// 2002/11/16 RSA   First release.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Opc.Da;
using Opc.Ae;

namespace Opc.Ae.SampleClient
{
	/// <summary>
	/// A dialog that displays the current status of an OPC server.
	/// </summary>
	public class EventTypesSelectDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel ButtonsPN;
		private System.Windows.Forms.Button CancelBTN;
		private Opc.SampleClient.BitMaskCtrl FiltersCTRL;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EventTypesSelectDlg()
		{
			//
			// Required for Windows Form Designer support
			//
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ButtonsPN = new System.Windows.Forms.Panel();
			this.CancelBTN = new System.Windows.Forms.Button();
			this.FiltersCTRL = new Opc.SampleClient.BitMaskCtrl();
			this.ButtonsPN.SuspendLayout();
			this.SuspendLayout();
			// 
			// ButtonsPN
			// 
			this.ButtonsPN.Controls.Add(this.CancelBTN);
			this.ButtonsPN.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPN.Location = new System.Drawing.Point(0, 110);
			this.ButtonsPN.Name = "ButtonsPN";
			this.ButtonsPN.Size = new System.Drawing.Size(242, 36);
			this.ButtonsPN.TabIndex = 0;
			// 
			// CancelBTN
			// 
			this.CancelBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBTN.Location = new System.Drawing.Point(84, 8);
			this.CancelBTN.Name = "CancelBTN";
			this.CancelBTN.TabIndex = 0;
			this.CancelBTN.Text = "Close";
			// 
			// FiltersCTRL
			// 
			this.FiltersCTRL.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FiltersCTRL.Location = new System.Drawing.Point(0, 0);
			this.FiltersCTRL.Name = "FiltersCTRL";
			this.FiltersCTRL.ReadOnly = false;
			this.FiltersCTRL.Size = new System.Drawing.Size(242, 110);
			this.FiltersCTRL.TabIndex = 1;
			this.FiltersCTRL.Type = null;
			this.FiltersCTRL.Value = 0;
			// 
			// EventTypesSelectDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.CancelBTN;
			this.ClientSize = new System.Drawing.Size(242, 146);
			this.Controls.Add(this.FiltersCTRL);
			this.Controls.Add(this.ButtonsPN);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(600, 216);
			this.MinimumSize = new System.Drawing.Size(250, 180);
			this.Name = "EventTypesSelectDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Event Types";
			this.ButtonsPN.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Private Members
		#endregion

		#region Public Interface
		/// <summary>
		/// Prompts the user to select one or more event types.
		/// </summary>
		public new int ShowDialog()
		{
			FiltersCTRL.Type  = typeof(Opc.Ae.EventType);
			FiltersCTRL.Value = (int)EventType.All;

			if (base.ShowDialog() == DialogResult.OK)
			{
				return FiltersCTRL.Value;
			}

			return 0;
		}
		#endregion
	}
}
