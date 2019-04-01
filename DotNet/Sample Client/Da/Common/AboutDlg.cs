//============================================================================
// TITLE: AboutDlg.cs
//
// CONTENTS:
// 
// A dialog which displays information about the application.
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
// 2002/11/16 RSA   Initial implementation.

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Opc.Da.SampleClient
{
	/// <summary>
	/// A dialog which displays information about the application.
	/// </summary>
	public class AboutDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox DisclaimerTB;
		private System.Windows.Forms.PictureBox LogoPB;
		private System.Windows.Forms.Panel ButtonsPN;
		private System.Windows.Forms.Button OkBTN;
		private System.Windows.Forms.Label AppNameLB;
		private System.Windows.Forms.Label CopyrightLB;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Initializes the dialog with the specified application name.
		/// </summary>
		public AboutDlg(string applicationName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			AppNameLB.Text    = applicationName;
			DisclaimerTB.Text = "This code is provided by the OPC Foundation solely to assist in understanding and use of the appropriate OPC Specification(s) and may be used as set forth in the License Grant section of the OPC Specification. This code is provided as-is and without warranty or support of any sort and is subject to the Warranty and Liability Disclaimers which appear in the printed OPC Specification.";
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AboutDlg));
			this.DisclaimerTB = new System.Windows.Forms.TextBox();
			this.AppNameLB = new System.Windows.Forms.Label();
			this.CopyrightLB = new System.Windows.Forms.Label();
			this.LogoPB = new System.Windows.Forms.PictureBox();
			this.ButtonsPN = new System.Windows.Forms.Panel();
			this.OkBTN = new System.Windows.Forms.Button();
			this.ButtonsPN.SuspendLayout();
			this.SuspendLayout();
			// 
			// DisclaimerTB
			// 
			this.DisclaimerTB.Location = new System.Drawing.Point(8, 72);
			this.DisclaimerTB.Multiline = true;
			this.DisclaimerTB.Name = "DisclaimerTB";
			this.DisclaimerTB.ReadOnly = true;
			this.DisclaimerTB.Size = new System.Drawing.Size(488, 80);
			this.DisclaimerTB.TabIndex = 3;
			this.DisclaimerTB.Text = "";
			// 
			// AppNameLB
			// 
			this.AppNameLB.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.AppNameLB.Location = new System.Drawing.Point(160, 8);
			this.AppNameLB.Name = "AppNameLB";
			this.AppNameLB.Size = new System.Drawing.Size(328, 23);
			this.AppNameLB.TabIndex = 1;
			this.AppNameLB.Text = "OPC .NET API Sample Data Access Client";
			this.AppNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// CopyrightLB
			// 
			this.CopyrightLB.Location = new System.Drawing.Point(160, 40);
			this.CopyrightLB.Name = "CopyrightLB";
			this.CopyrightLB.Size = new System.Drawing.Size(376, 23);
			this.CopyrightLB.TabIndex = 2;
			this.CopyrightLB.Text = "(c) Copyright 2002-2003 The OPC Foundation. All Rights Reserved.";
			this.CopyrightLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// LogoPB
			// 
			this.LogoPB.Image = ((System.Drawing.Image)(resources.GetObject("LogoPB.Image")));
			this.LogoPB.Location = new System.Drawing.Point(8, 8);
			this.LogoPB.Name = "LogoPB";
			this.LogoPB.Size = new System.Drawing.Size(152, 64);
			this.LogoPB.TabIndex = 3;
			this.LogoPB.TabStop = false;
			// 
			// ButtonsPN
			// 
			this.ButtonsPN.Controls.Add(this.OkBTN);
			this.ButtonsPN.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPN.Location = new System.Drawing.Point(0, 157);
			this.ButtonsPN.Name = "ButtonsPN";
			this.ButtonsPN.Size = new System.Drawing.Size(504, 40);
			this.ButtonsPN.TabIndex = 0;
			// 
			// OkBTN
			// 
			this.OkBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.OkBTN.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkBTN.Location = new System.Drawing.Point(215, 8);
			this.OkBTN.Name = "OkBTN";
			this.OkBTN.TabIndex = 0;
			this.OkBTN.Text = "OK";
			// 
			// AboutDlg
			// 
			this.AcceptButton = this.OkBTN;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(504, 197);
			this.Controls.Add(this.ButtonsPN);
			this.Controls.Add(this.LogoPB);
			this.Controls.Add(this.CopyrightLB);
			this.Controls.Add(this.AppNameLB);
			this.Controls.Add(this.DisclaimerTB);
			this.Name = "AboutDlg";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "About";
			this.ButtonsPN.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
