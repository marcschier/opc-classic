//============================================================================
// TITLE: ItemEditCtrl.cs
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
	public class ConnectionEditCtrl : System.Windows.Forms.UserControl, IEditObjectCtrl
	{
		private System.Windows.Forms.CheckBox EnableSubstituteValueCB;
		private System.Windows.Forms.CheckBox KeywordSpecifiedCB;
		private System.Windows.Forms.TextBox KeywordTB;
		private System.Windows.Forms.Label KeywordLB;
		private System.Windows.Forms.CheckBox SubstituteValueSpecifiedCB;
		private System.Windows.Forms.Button SourceServerBTN;
		private System.Windows.Forms.CheckBox VendorDataSpecifiedCB;
		private System.Windows.Forms.TextBox VendorDataTB;
		private System.Windows.Forms.CheckBox UpdateRateSpecifiedCB;
		private System.Windows.Forms.CheckBox DeadbandSpecifiedCB;
		private System.Windows.Forms.CheckBox DescriptionSpecifiedCB;
		private System.Windows.Forms.TextBox DescriptionTB;
		private System.Windows.Forms.NumericUpDown DeadbandCTRL;
		private System.Windows.Forms.NumericUpDown UpdateRateCTRL;
		private System.Windows.Forms.CheckBox EnableSubstituteValueSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultOverriddenCB;
		private System.Windows.Forms.CheckBox DefaultOverriddenSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultOverrideValueSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultTargetItemConnectedSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultTargetItemConnectedCB;
		private System.Windows.Forms.CheckBox DefaultSourceItemConnectedSpecifiedCB;
		private System.Windows.Forms.CheckBox DefaultSourceItemConnectedCB;
		private System.Windows.Forms.CheckBox BrowsePathsSpecifiedCB;
		private System.Windows.Forms.CheckBox ConnectionNameSpecifiedCB;
		private System.Windows.Forms.TextBox ConnectionNameTB;
		private System.Windows.Forms.Label VendorDataLB;
		private System.Windows.Forms.Label DescriptionLB;
		private System.Windows.Forms.Label DeadbandLB;
		private System.Windows.Forms.Label UpdateRateLB;
		private System.Windows.Forms.Label EnableSubstituteValueLB;
		private System.Windows.Forms.Label SubstituteValueLB;
		private System.Windows.Forms.Label DefaultOverrideValueLB;
		private System.Windows.Forms.Label DefaultOverriddenLB;
		private System.Windows.Forms.Label DefaultTargetItemConnectedLB;
		private System.Windows.Forms.Label DefaultSourceItemConnectedLB;
		private System.Windows.Forms.Label BrowsePathsLB;
		private System.Windows.Forms.Label ConnectionNameLB;
		private Opc.SampleClient.ValueCtrl DefaultOverrideValueCTRL;
		private Opc.SampleClient.ValueCtrl SubstituteValueCTRL;
		private Opc.SampleClient.ValueCtrl BrowsePathsCTRL;
		private System.Windows.Forms.Label TargetItemNameLB;
		private System.Windows.Forms.Label SourceServerNameLB;
		private System.Windows.Forms.NumericUpDown SourceItemQueueSizeCTRL;
		private System.Windows.Forms.Label SourceItemQueueSizeLB;
		private System.Windows.Forms.CheckBox TargetItemNameSpecifiedCB;
		private System.Windows.Forms.CheckBox SourceServerNameSpecifiedCB;
		private System.Windows.Forms.CheckBox SourceItemQueueSizeSpecifiedCB;
		private System.Windows.Forms.Button TargetItemBTN;
		private System.Windows.Forms.TextBox TargetItemNameTB;
		private System.Windows.Forms.TextBox TargetItemPathTB;
		private System.Windows.Forms.Label TargetItemPathLB;
		private System.Windows.Forms.CheckBox TargetItemPathSpecifiedCB;
		private System.Windows.Forms.TextBox SourceItemPathTB;
		private System.Windows.Forms.Label SourceItemPathLB;
		private System.Windows.Forms.CheckBox SourceItemPathSpecifiedCB;
		private System.Windows.Forms.Button SourceItemBTN;
		private System.Windows.Forms.TextBox SourceItemNameTB;
		private System.Windows.Forms.Label SourceItemNameLB;
		private System.Windows.Forms.CheckBox SourceItemNameSpecifiedCB;
		private System.Windows.Forms.ComboBox SourceServerNameCB;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConnectionEditCtrl()
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
			this.EnableSubstituteValueCB = new System.Windows.Forms.CheckBox();
			this.TargetItemNameLB = new System.Windows.Forms.Label();
			this.KeywordSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.KeywordTB = new System.Windows.Forms.TextBox();
			this.KeywordLB = new System.Windows.Forms.Label();
			this.SubstituteValueSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceServerBTN = new System.Windows.Forms.Button();
			this.SourceServerNameLB = new System.Windows.Forms.Label();
			this.VendorDataSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceItemQueueSizeCTRL = new System.Windows.Forms.NumericUpDown();
			this.SourceItemQueueSizeLB = new System.Windows.Forms.Label();
			this.VendorDataTB = new System.Windows.Forms.TextBox();
			this.TargetItemNameSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceServerNameSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.UpdateRateSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DeadbandSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DescriptionSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceItemQueueSizeSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DescriptionTB = new System.Windows.Forms.TextBox();
			this.DeadbandCTRL = new System.Windows.Forms.NumericUpDown();
			this.UpdateRateCTRL = new System.Windows.Forms.NumericUpDown();
			this.EnableSubstituteValueSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultOverriddenCB = new System.Windows.Forms.CheckBox();
			this.DefaultOverriddenSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultOverrideValueSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultTargetItemConnectedSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultTargetItemConnectedCB = new System.Windows.Forms.CheckBox();
			this.DefaultSourceItemConnectedSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.DefaultSourceItemConnectedCB = new System.Windows.Forms.CheckBox();
			this.BrowsePathsSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.ConnectionNameSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.ConnectionNameTB = new System.Windows.Forms.TextBox();
			this.VendorDataLB = new System.Windows.Forms.Label();
			this.DescriptionLB = new System.Windows.Forms.Label();
			this.DeadbandLB = new System.Windows.Forms.Label();
			this.UpdateRateLB = new System.Windows.Forms.Label();
			this.EnableSubstituteValueLB = new System.Windows.Forms.Label();
			this.SubstituteValueLB = new System.Windows.Forms.Label();
			this.DefaultOverrideValueLB = new System.Windows.Forms.Label();
			this.DefaultOverriddenLB = new System.Windows.Forms.Label();
			this.DefaultTargetItemConnectedLB = new System.Windows.Forms.Label();
			this.DefaultSourceItemConnectedLB = new System.Windows.Forms.Label();
			this.BrowsePathsLB = new System.Windows.Forms.Label();
			this.ConnectionNameLB = new System.Windows.Forms.Label();
			this.DefaultOverrideValueCTRL = new Opc.SampleClient.ValueCtrl();
			this.SubstituteValueCTRL = new Opc.SampleClient.ValueCtrl();
			this.BrowsePathsCTRL = new Opc.SampleClient.ValueCtrl();
			this.TargetItemBTN = new System.Windows.Forms.Button();
			this.TargetItemNameTB = new System.Windows.Forms.TextBox();
			this.TargetItemPathTB = new System.Windows.Forms.TextBox();
			this.TargetItemPathLB = new System.Windows.Forms.Label();
			this.TargetItemPathSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceItemPathTB = new System.Windows.Forms.TextBox();
			this.SourceItemPathLB = new System.Windows.Forms.Label();
			this.SourceItemPathSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceItemBTN = new System.Windows.Forms.Button();
			this.SourceItemNameTB = new System.Windows.Forms.TextBox();
			this.SourceItemNameLB = new System.Windows.Forms.Label();
			this.SourceItemNameSpecifiedCB = new System.Windows.Forms.CheckBox();
			this.SourceServerNameCB = new System.Windows.Forms.ComboBox();
			((System.ComponentModel.ISupportInitialize)(this.SourceItemQueueSizeCTRL)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DeadbandCTRL)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UpdateRateCTRL)).BeginInit();
			this.SuspendLayout();
			// 
			// EnableSubstituteValueCB
			// 
			this.EnableSubstituteValueCB.Enabled = false;
			this.EnableSubstituteValueCB.Location = new System.Drawing.Point(144, 216);
			this.EnableSubstituteValueCB.Name = "EnableSubstituteValueCB";
			this.EnableSubstituteValueCB.Size = new System.Drawing.Size(16, 24);
			this.EnableSubstituteValueCB.TabIndex = 112;
			// 
			// TargetItemNameLB
			// 
			this.TargetItemNameLB.Location = new System.Drawing.Point(0, 240);
			this.TargetItemNameLB.Name = "TargetItemNameLB";
			this.TargetItemNameLB.Size = new System.Drawing.Size(144, 23);
			this.TargetItemNameLB.TabIndex = 111;
			this.TargetItemNameLB.Text = "Target Item Name";
			this.TargetItemNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// KeywordSpecifiedCB
			// 
			this.KeywordSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.KeywordSpecifiedCB.Location = new System.Drawing.Point(504, 72);
			this.KeywordSpecifiedCB.Name = "KeywordSpecifiedCB";
			this.KeywordSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.KeywordSpecifiedCB.TabIndex = 110;
			this.KeywordSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// KeywordTB
			// 
			this.KeywordTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.KeywordTB.Enabled = false;
			this.KeywordTB.Location = new System.Drawing.Point(144, 72);
			this.KeywordTB.Name = "KeywordTB";
			this.KeywordTB.Size = new System.Drawing.Size(352, 20);
			this.KeywordTB.TabIndex = 109;
			this.KeywordTB.Text = "";
			// 
			// KeywordLB
			// 
			this.KeywordLB.Location = new System.Drawing.Point(0, 72);
			this.KeywordLB.Name = "KeywordLB";
			this.KeywordLB.Size = new System.Drawing.Size(144, 23);
			this.KeywordLB.TabIndex = 108;
			this.KeywordLB.Text = "Keyword";
			this.KeywordLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SubstituteValueSpecifiedCB
			// 
			this.SubstituteValueSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SubstituteValueSpecifiedCB.Location = new System.Drawing.Point(504, 192);
			this.SubstituteValueSpecifiedCB.Name = "SubstituteValueSpecifiedCB";
			this.SubstituteValueSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.SubstituteValueSpecifiedCB.TabIndex = 107;
			this.SubstituteValueSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceServerBTN
			// 
			this.SourceServerBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceServerBTN.Enabled = false;
			this.SourceServerBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SourceServerBTN.Location = new System.Drawing.Point(472, 288);
			this.SourceServerBTN.Name = "SourceServerBTN";
			this.SourceServerBTN.Size = new System.Drawing.Size(25, 20);
			this.SourceServerBTN.TabIndex = 103;
			this.SourceServerBTN.Text = "...";
			this.SourceServerBTN.Click += new System.EventHandler(this.SourceServerBTN_Click);
			// 
			// SourceServerNameLB
			// 
			this.SourceServerNameLB.Location = new System.Drawing.Point(0, 288);
			this.SourceServerNameLB.Name = "SourceServerNameLB";
			this.SourceServerNameLB.Size = new System.Drawing.Size(144, 23);
			this.SourceServerNameLB.TabIndex = 101;
			this.SourceServerNameLB.Text = "Source Server";
			this.SourceServerNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// VendorDataSpecifiedCB
			// 
			this.VendorDataSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.VendorDataSpecifiedCB.Location = new System.Drawing.Point(504, 432);
			this.VendorDataSpecifiedCB.Name = "VendorDataSpecifiedCB";
			this.VendorDataSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.VendorDataSpecifiedCB.TabIndex = 100;
			this.VendorDataSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceItemQueueSizeCTRL
			// 
			this.SourceItemQueueSizeCTRL.Enabled = false;
			this.SourceItemQueueSizeCTRL.Location = new System.Drawing.Point(144, 360);
			this.SourceItemQueueSizeCTRL.Maximum = new System.Decimal(new int[] {
																					32767,
																					0,
																					0,
																					0});
			this.SourceItemQueueSizeCTRL.Name = "SourceItemQueueSizeCTRL";
			this.SourceItemQueueSizeCTRL.Size = new System.Drawing.Size(80, 20);
			this.SourceItemQueueSizeCTRL.TabIndex = 99;
			// 
			// SourceItemQueueSizeLB
			// 
			this.SourceItemQueueSizeLB.Location = new System.Drawing.Point(0, 360);
			this.SourceItemQueueSizeLB.Name = "SourceItemQueueSizeLB";
			this.SourceItemQueueSizeLB.Size = new System.Drawing.Size(144, 23);
			this.SourceItemQueueSizeLB.TabIndex = 98;
			this.SourceItemQueueSizeLB.Text = "Queue Size";
			this.SourceItemQueueSizeLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// VendorDataTB
			// 
			this.VendorDataTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.VendorDataTB.Enabled = false;
			this.VendorDataTB.Location = new System.Drawing.Point(144, 432);
			this.VendorDataTB.Name = "VendorDataTB";
			this.VendorDataTB.Size = new System.Drawing.Size(352, 20);
			this.VendorDataTB.TabIndex = 97;
			this.VendorDataTB.Text = "";
			// 
			// TargetItemNameSpecifiedCB
			// 
			this.TargetItemNameSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TargetItemNameSpecifiedCB.Location = new System.Drawing.Point(504, 240);
			this.TargetItemNameSpecifiedCB.Name = "TargetItemNameSpecifiedCB";
			this.TargetItemNameSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.TargetItemNameSpecifiedCB.TabIndex = 96;
			this.TargetItemNameSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceServerNameSpecifiedCB
			// 
			this.SourceServerNameSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceServerNameSpecifiedCB.Location = new System.Drawing.Point(504, 288);
			this.SourceServerNameSpecifiedCB.Name = "SourceServerNameSpecifiedCB";
			this.SourceServerNameSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.SourceServerNameSpecifiedCB.TabIndex = 95;
			this.SourceServerNameSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// UpdateRateSpecifiedCB
			// 
			this.UpdateRateSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateRateSpecifiedCB.Location = new System.Drawing.Point(504, 384);
			this.UpdateRateSpecifiedCB.Name = "UpdateRateSpecifiedCB";
			this.UpdateRateSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.UpdateRateSpecifiedCB.TabIndex = 94;
			this.UpdateRateSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DeadbandSpecifiedCB
			// 
			this.DeadbandSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DeadbandSpecifiedCB.Location = new System.Drawing.Point(504, 408);
			this.DeadbandSpecifiedCB.Name = "DeadbandSpecifiedCB";
			this.DeadbandSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DeadbandSpecifiedCB.TabIndex = 93;
			this.DeadbandSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DescriptionSpecifiedCB
			// 
			this.DescriptionSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionSpecifiedCB.Location = new System.Drawing.Point(504, 48);
			this.DescriptionSpecifiedCB.Name = "DescriptionSpecifiedCB";
			this.DescriptionSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DescriptionSpecifiedCB.TabIndex = 92;
			this.DescriptionSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceItemQueueSizeSpecifiedCB
			// 
			this.SourceItemQueueSizeSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemQueueSizeSpecifiedCB.Location = new System.Drawing.Point(504, 360);
			this.SourceItemQueueSizeSpecifiedCB.Name = "SourceItemQueueSizeSpecifiedCB";
			this.SourceItemQueueSizeSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.SourceItemQueueSizeSpecifiedCB.TabIndex = 91;
			this.SourceItemQueueSizeSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DescriptionTB
			// 
			this.DescriptionTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DescriptionTB.Enabled = false;
			this.DescriptionTB.Location = new System.Drawing.Point(144, 48);
			this.DescriptionTB.Name = "DescriptionTB";
			this.DescriptionTB.Size = new System.Drawing.Size(352, 20);
			this.DescriptionTB.TabIndex = 90;
			this.DescriptionTB.Text = "";
			// 
			// DeadbandCTRL
			// 
			this.DeadbandCTRL.Enabled = false;
			this.DeadbandCTRL.Location = new System.Drawing.Point(144, 408);
			this.DeadbandCTRL.Name = "DeadbandCTRL";
			this.DeadbandCTRL.Size = new System.Drawing.Size(80, 20);
			this.DeadbandCTRL.TabIndex = 89;
			// 
			// UpdateRateCTRL
			// 
			this.UpdateRateCTRL.Enabled = false;
			this.UpdateRateCTRL.Location = new System.Drawing.Point(144, 384);
			this.UpdateRateCTRL.Maximum = new System.Decimal(new int[] {
																		   2147483647,
																		   0,
																		   0,
																		   0});
			this.UpdateRateCTRL.Name = "UpdateRateCTRL";
			this.UpdateRateCTRL.Size = new System.Drawing.Size(80, 20);
			this.UpdateRateCTRL.TabIndex = 88;
			// 
			// EnableSubstituteValueSpecifiedCB
			// 
			this.EnableSubstituteValueSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EnableSubstituteValueSpecifiedCB.Location = new System.Drawing.Point(504, 216);
			this.EnableSubstituteValueSpecifiedCB.Name = "EnableSubstituteValueSpecifiedCB";
			this.EnableSubstituteValueSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.EnableSubstituteValueSpecifiedCB.TabIndex = 87;
			this.EnableSubstituteValueSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultOverriddenCB
			// 
			this.DefaultOverriddenCB.Enabled = false;
			this.DefaultOverriddenCB.Location = new System.Drawing.Point(144, 144);
			this.DefaultOverriddenCB.Name = "DefaultOverriddenCB";
			this.DefaultOverriddenCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultOverriddenCB.TabIndex = 86;
			// 
			// DefaultOverriddenSpecifiedCB
			// 
			this.DefaultOverriddenSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultOverriddenSpecifiedCB.Location = new System.Drawing.Point(504, 144);
			this.DefaultOverriddenSpecifiedCB.Name = "DefaultOverriddenSpecifiedCB";
			this.DefaultOverriddenSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultOverriddenSpecifiedCB.TabIndex = 85;
			this.DefaultOverriddenSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultOverrideValueSpecifiedCB
			// 
			this.DefaultOverrideValueSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultOverrideValueSpecifiedCB.Location = new System.Drawing.Point(504, 168);
			this.DefaultOverrideValueSpecifiedCB.Name = "DefaultOverrideValueSpecifiedCB";
			this.DefaultOverrideValueSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultOverrideValueSpecifiedCB.TabIndex = 84;
			this.DefaultOverrideValueSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultTargetItemConnectedSpecifiedCB
			// 
			this.DefaultTargetItemConnectedSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultTargetItemConnectedSpecifiedCB.Location = new System.Drawing.Point(504, 120);
			this.DefaultTargetItemConnectedSpecifiedCB.Name = "DefaultTargetItemConnectedSpecifiedCB";
			this.DefaultTargetItemConnectedSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultTargetItemConnectedSpecifiedCB.TabIndex = 83;
			this.DefaultTargetItemConnectedSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultTargetItemConnectedCB
			// 
			this.DefaultTargetItemConnectedCB.Enabled = false;
			this.DefaultTargetItemConnectedCB.Location = new System.Drawing.Point(144, 120);
			this.DefaultTargetItemConnectedCB.Name = "DefaultTargetItemConnectedCB";
			this.DefaultTargetItemConnectedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultTargetItemConnectedCB.TabIndex = 82;
			// 
			// DefaultSourceItemConnectedSpecifiedCB
			// 
			this.DefaultSourceItemConnectedSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultSourceItemConnectedSpecifiedCB.Location = new System.Drawing.Point(504, 96);
			this.DefaultSourceItemConnectedSpecifiedCB.Name = "DefaultSourceItemConnectedSpecifiedCB";
			this.DefaultSourceItemConnectedSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultSourceItemConnectedSpecifiedCB.TabIndex = 81;
			this.DefaultSourceItemConnectedSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// DefaultSourceItemConnectedCB
			// 
			this.DefaultSourceItemConnectedCB.Enabled = false;
			this.DefaultSourceItemConnectedCB.Location = new System.Drawing.Point(144, 96);
			this.DefaultSourceItemConnectedCB.Name = "DefaultSourceItemConnectedCB";
			this.DefaultSourceItemConnectedCB.Size = new System.Drawing.Size(16, 24);
			this.DefaultSourceItemConnectedCB.TabIndex = 80;
			// 
			// BrowsePathsSpecifiedCB
			// 
			this.BrowsePathsSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BrowsePathsSpecifiedCB.Location = new System.Drawing.Point(504, 24);
			this.BrowsePathsSpecifiedCB.Name = "BrowsePathsSpecifiedCB";
			this.BrowsePathsSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.BrowsePathsSpecifiedCB.TabIndex = 79;
			this.BrowsePathsSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// ConnectionNameSpecifiedCB
			// 
			this.ConnectionNameSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionNameSpecifiedCB.Location = new System.Drawing.Point(504, 0);
			this.ConnectionNameSpecifiedCB.Name = "ConnectionNameSpecifiedCB";
			this.ConnectionNameSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.ConnectionNameSpecifiedCB.TabIndex = 78;
			this.ConnectionNameSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// ConnectionNameTB
			// 
			this.ConnectionNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectionNameTB.Enabled = false;
			this.ConnectionNameTB.Location = new System.Drawing.Point(144, 0);
			this.ConnectionNameTB.Name = "ConnectionNameTB";
			this.ConnectionNameTB.Size = new System.Drawing.Size(352, 20);
			this.ConnectionNameTB.TabIndex = 77;
			this.ConnectionNameTB.Text = "";
			// 
			// VendorDataLB
			// 
			this.VendorDataLB.Location = new System.Drawing.Point(0, 432);
			this.VendorDataLB.Name = "VendorDataLB";
			this.VendorDataLB.Size = new System.Drawing.Size(144, 23);
			this.VendorDataLB.TabIndex = 76;
			this.VendorDataLB.Text = "Vendor Data";
			this.VendorDataLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DescriptionLB
			// 
			this.DescriptionLB.Location = new System.Drawing.Point(0, 48);
			this.DescriptionLB.Name = "DescriptionLB";
			this.DescriptionLB.Size = new System.Drawing.Size(144, 23);
			this.DescriptionLB.TabIndex = 75;
			this.DescriptionLB.Text = "Description";
			this.DescriptionLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DeadbandLB
			// 
			this.DeadbandLB.Location = new System.Drawing.Point(0, 408);
			this.DeadbandLB.Name = "DeadbandLB";
			this.DeadbandLB.Size = new System.Drawing.Size(144, 23);
			this.DeadbandLB.TabIndex = 74;
			this.DeadbandLB.Text = "Deadband";
			this.DeadbandLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// UpdateRateLB
			// 
			this.UpdateRateLB.Location = new System.Drawing.Point(0, 384);
			this.UpdateRateLB.Name = "UpdateRateLB";
			this.UpdateRateLB.Size = new System.Drawing.Size(144, 23);
			this.UpdateRateLB.TabIndex = 73;
			this.UpdateRateLB.Text = "Update Rate";
			this.UpdateRateLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// EnableSubstituteValueLB
			// 
			this.EnableSubstituteValueLB.Location = new System.Drawing.Point(0, 216);
			this.EnableSubstituteValueLB.Name = "EnableSubstituteValueLB";
			this.EnableSubstituteValueLB.Size = new System.Drawing.Size(144, 23);
			this.EnableSubstituteValueLB.TabIndex = 71;
			this.EnableSubstituteValueLB.Text = "Enable Substitute Value";
			this.EnableSubstituteValueLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SubstituteValueLB
			// 
			this.SubstituteValueLB.Location = new System.Drawing.Point(0, 192);
			this.SubstituteValueLB.Name = "SubstituteValueLB";
			this.SubstituteValueLB.Size = new System.Drawing.Size(144, 23);
			this.SubstituteValueLB.TabIndex = 70;
			this.SubstituteValueLB.Text = "Substitute Value";
			this.SubstituteValueLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultOverrideValueLB
			// 
			this.DefaultOverrideValueLB.Location = new System.Drawing.Point(0, 168);
			this.DefaultOverrideValueLB.Name = "DefaultOverrideValueLB";
			this.DefaultOverrideValueLB.Size = new System.Drawing.Size(144, 23);
			this.DefaultOverrideValueLB.TabIndex = 69;
			this.DefaultOverrideValueLB.Text = "Default Override Value";
			this.DefaultOverrideValueLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultOverriddenLB
			// 
			this.DefaultOverriddenLB.Location = new System.Drawing.Point(0, 144);
			this.DefaultOverriddenLB.Name = "DefaultOverriddenLB";
			this.DefaultOverriddenLB.Size = new System.Drawing.Size(144, 23);
			this.DefaultOverriddenLB.TabIndex = 68;
			this.DefaultOverriddenLB.Text = "Default Overridden";
			this.DefaultOverriddenLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultTargetItemConnectedLB
			// 
			this.DefaultTargetItemConnectedLB.Location = new System.Drawing.Point(0, 120);
			this.DefaultTargetItemConnectedLB.Name = "DefaultTargetItemConnectedLB";
			this.DefaultTargetItemConnectedLB.Size = new System.Drawing.Size(144, 23);
			this.DefaultTargetItemConnectedLB.TabIndex = 67;
			this.DefaultTargetItemConnectedLB.Text = "Default Target Connected";
			this.DefaultTargetItemConnectedLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultSourceItemConnectedLB
			// 
			this.DefaultSourceItemConnectedLB.Location = new System.Drawing.Point(0, 96);
			this.DefaultSourceItemConnectedLB.Name = "DefaultSourceItemConnectedLB";
			this.DefaultSourceItemConnectedLB.Size = new System.Drawing.Size(144, 23);
			this.DefaultSourceItemConnectedLB.TabIndex = 66;
			this.DefaultSourceItemConnectedLB.Text = "Default Source Connected";
			this.DefaultSourceItemConnectedLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// BrowsePathsLB
			// 
			this.BrowsePathsLB.Location = new System.Drawing.Point(0, 24);
			this.BrowsePathsLB.Name = "BrowsePathsLB";
			this.BrowsePathsLB.Size = new System.Drawing.Size(144, 23);
			this.BrowsePathsLB.TabIndex = 65;
			this.BrowsePathsLB.Text = "Browse Paths";
			this.BrowsePathsLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ConnectionNameLB
			// 
			this.ConnectionNameLB.Location = new System.Drawing.Point(0, 0);
			this.ConnectionNameLB.Name = "ConnectionNameLB";
			this.ConnectionNameLB.Size = new System.Drawing.Size(144, 23);
			this.ConnectionNameLB.TabIndex = 64;
			this.ConnectionNameLB.Text = "Connection Name";
			this.ConnectionNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// DefaultOverrideValueCTRL
			// 
			this.DefaultOverrideValueCTRL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultOverrideValueCTRL.Enabled = false;
			this.DefaultOverrideValueCTRL.ItemID = null;
			this.DefaultOverrideValueCTRL.Location = new System.Drawing.Point(144, 168);
			this.DefaultOverrideValueCTRL.Name = "DefaultOverrideValueCTRL";
			this.DefaultOverrideValueCTRL.Size = new System.Drawing.Size(352, 20);
			this.DefaultOverrideValueCTRL.TabIndex = 113;
			this.DefaultOverrideValueCTRL.Value = null;
			// 
			// SubstituteValueCTRL
			// 
			this.SubstituteValueCTRL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.SubstituteValueCTRL.Enabled = false;
			this.SubstituteValueCTRL.ItemID = null;
			this.SubstituteValueCTRL.Location = new System.Drawing.Point(144, 192);
			this.SubstituteValueCTRL.Name = "SubstituteValueCTRL";
			this.SubstituteValueCTRL.Size = new System.Drawing.Size(352, 20);
			this.SubstituteValueCTRL.TabIndex = 114;
			this.SubstituteValueCTRL.Value = null;
			// 
			// BrowsePathsCTRL
			// 
			this.BrowsePathsCTRL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BrowsePathsCTRL.Enabled = false;
			this.BrowsePathsCTRL.ItemID = null;
			this.BrowsePathsCTRL.Location = new System.Drawing.Point(144, 24);
			this.BrowsePathsCTRL.Name = "BrowsePathsCTRL";
			this.BrowsePathsCTRL.Size = new System.Drawing.Size(352, 20);
			this.BrowsePathsCTRL.TabIndex = 115;
			this.BrowsePathsCTRL.Value = null;
			// 
			// TargetItemBTN
			// 
			this.TargetItemBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TargetItemBTN.Enabled = false;
			this.TargetItemBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.TargetItemBTN.Location = new System.Drawing.Point(472, 240);
			this.TargetItemBTN.Name = "TargetItemBTN";
			this.TargetItemBTN.Size = new System.Drawing.Size(25, 20);
			this.TargetItemBTN.TabIndex = 117;
			this.TargetItemBTN.Text = "...";
			this.TargetItemBTN.Click += new System.EventHandler(this.TargetItemBTN_Click);
			// 
			// TargetItemNameTB
			// 
			this.TargetItemNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.TargetItemNameTB.Enabled = false;
			this.TargetItemNameTB.Location = new System.Drawing.Point(144, 240);
			this.TargetItemNameTB.Name = "TargetItemNameTB";
			this.TargetItemNameTB.Size = new System.Drawing.Size(324, 20);
			this.TargetItemNameTB.TabIndex = 116;
			this.TargetItemNameTB.Text = "";
			// 
			// TargetItemPathTB
			// 
			this.TargetItemPathTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.TargetItemPathTB.Enabled = false;
			this.TargetItemPathTB.Location = new System.Drawing.Point(144, 264);
			this.TargetItemPathTB.Name = "TargetItemPathTB";
			this.TargetItemPathTB.Size = new System.Drawing.Size(324, 20);
			this.TargetItemPathTB.TabIndex = 120;
			this.TargetItemPathTB.Text = "";
			// 
			// TargetItemPathLB
			// 
			this.TargetItemPathLB.Location = new System.Drawing.Point(0, 264);
			this.TargetItemPathLB.Name = "TargetItemPathLB";
			this.TargetItemPathLB.Size = new System.Drawing.Size(144, 23);
			this.TargetItemPathLB.TabIndex = 119;
			this.TargetItemPathLB.Text = "Target Item Path";
			this.TargetItemPathLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TargetItemPathSpecifiedCB
			// 
			this.TargetItemPathSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.TargetItemPathSpecifiedCB.Location = new System.Drawing.Point(504, 264);
			this.TargetItemPathSpecifiedCB.Name = "TargetItemPathSpecifiedCB";
			this.TargetItemPathSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.TargetItemPathSpecifiedCB.TabIndex = 118;
			this.TargetItemPathSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceItemPathTB
			// 
			this.SourceItemPathTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemPathTB.Enabled = false;
			this.SourceItemPathTB.Location = new System.Drawing.Point(144, 336);
			this.SourceItemPathTB.Name = "SourceItemPathTB";
			this.SourceItemPathTB.Size = new System.Drawing.Size(324, 20);
			this.SourceItemPathTB.TabIndex = 127;
			this.SourceItemPathTB.Text = "";
			// 
			// SourceItemPathLB
			// 
			this.SourceItemPathLB.Location = new System.Drawing.Point(0, 336);
			this.SourceItemPathLB.Name = "SourceItemPathLB";
			this.SourceItemPathLB.Size = new System.Drawing.Size(144, 23);
			this.SourceItemPathLB.TabIndex = 126;
			this.SourceItemPathLB.Text = "Source Item Path";
			this.SourceItemPathLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SourceItemPathSpecifiedCB
			// 
			this.SourceItemPathSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemPathSpecifiedCB.Location = new System.Drawing.Point(504, 336);
			this.SourceItemPathSpecifiedCB.Name = "SourceItemPathSpecifiedCB";
			this.SourceItemPathSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.SourceItemPathSpecifiedCB.TabIndex = 125;
			this.SourceItemPathSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceItemBTN
			// 
			this.SourceItemBTN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemBTN.Enabled = false;
			this.SourceItemBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.SourceItemBTN.Location = new System.Drawing.Point(472, 312);
			this.SourceItemBTN.Name = "SourceItemBTN";
			this.SourceItemBTN.Size = new System.Drawing.Size(25, 20);
			this.SourceItemBTN.TabIndex = 124;
			this.SourceItemBTN.Text = "...";
			this.SourceItemBTN.Click += new System.EventHandler(this.SourceItemBTN_Click);
			// 
			// SourceItemNameTB
			// 
			this.SourceItemNameTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemNameTB.Enabled = false;
			this.SourceItemNameTB.Location = new System.Drawing.Point(144, 312);
			this.SourceItemNameTB.Name = "SourceItemNameTB";
			this.SourceItemNameTB.Size = new System.Drawing.Size(324, 20);
			this.SourceItemNameTB.TabIndex = 123;
			this.SourceItemNameTB.Text = "";
			// 
			// SourceItemNameLB
			// 
			this.SourceItemNameLB.Location = new System.Drawing.Point(0, 312);
			this.SourceItemNameLB.Name = "SourceItemNameLB";
			this.SourceItemNameLB.Size = new System.Drawing.Size(144, 23);
			this.SourceItemNameLB.TabIndex = 122;
			this.SourceItemNameLB.Text = "Source Item Name";
			this.SourceItemNameLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SourceItemNameSpecifiedCB
			// 
			this.SourceItemNameSpecifiedCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SourceItemNameSpecifiedCB.Location = new System.Drawing.Point(504, 312);
			this.SourceItemNameSpecifiedCB.Name = "SourceItemNameSpecifiedCB";
			this.SourceItemNameSpecifiedCB.Size = new System.Drawing.Size(16, 24);
			this.SourceItemNameSpecifiedCB.TabIndex = 121;
			this.SourceItemNameSpecifiedCB.CheckedChanged += new System.EventHandler(this.Specified_CheckedChanged);
			// 
			// SourceServerNameCB
			// 
			this.SourceServerNameCB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.SourceServerNameCB.Location = new System.Drawing.Point(144, 288);
			this.SourceServerNameCB.Name = "SourceServerNameCB";
			this.SourceServerNameCB.Size = new System.Drawing.Size(324, 21);
			this.SourceServerNameCB.TabIndex = 128;
			// 
			// ConnectionEditCtrl
			// 
			this.Controls.Add(this.SourceServerNameCB);
			this.Controls.Add(this.SourceItemPathTB);
			this.Controls.Add(this.SourceItemPathLB);
			this.Controls.Add(this.SourceItemPathSpecifiedCB);
			this.Controls.Add(this.SourceItemBTN);
			this.Controls.Add(this.SourceItemNameTB);
			this.Controls.Add(this.SourceItemNameLB);
			this.Controls.Add(this.SourceItemNameSpecifiedCB);
			this.Controls.Add(this.TargetItemPathTB);
			this.Controls.Add(this.TargetItemPathLB);
			this.Controls.Add(this.TargetItemPathSpecifiedCB);
			this.Controls.Add(this.TargetItemBTN);
			this.Controls.Add(this.TargetItemNameTB);
			this.Controls.Add(this.BrowsePathsCTRL);
			this.Controls.Add(this.SubstituteValueCTRL);
			this.Controls.Add(this.DefaultOverrideValueCTRL);
			this.Controls.Add(this.EnableSubstituteValueCB);
			this.Controls.Add(this.TargetItemNameLB);
			this.Controls.Add(this.KeywordSpecifiedCB);
			this.Controls.Add(this.KeywordTB);
			this.Controls.Add(this.KeywordLB);
			this.Controls.Add(this.SubstituteValueSpecifiedCB);
			this.Controls.Add(this.SourceServerBTN);
			this.Controls.Add(this.SourceServerNameLB);
			this.Controls.Add(this.VendorDataSpecifiedCB);
			this.Controls.Add(this.SourceItemQueueSizeCTRL);
			this.Controls.Add(this.SourceItemQueueSizeLB);
			this.Controls.Add(this.VendorDataTB);
			this.Controls.Add(this.TargetItemNameSpecifiedCB);
			this.Controls.Add(this.SourceServerNameSpecifiedCB);
			this.Controls.Add(this.UpdateRateSpecifiedCB);
			this.Controls.Add(this.DeadbandSpecifiedCB);
			this.Controls.Add(this.DescriptionSpecifiedCB);
			this.Controls.Add(this.SourceItemQueueSizeSpecifiedCB);
			this.Controls.Add(this.DescriptionTB);
			this.Controls.Add(this.DeadbandCTRL);
			this.Controls.Add(this.UpdateRateCTRL);
			this.Controls.Add(this.EnableSubstituteValueSpecifiedCB);
			this.Controls.Add(this.DefaultOverriddenCB);
			this.Controls.Add(this.DefaultOverriddenSpecifiedCB);
			this.Controls.Add(this.DefaultOverrideValueSpecifiedCB);
			this.Controls.Add(this.DefaultTargetItemConnectedSpecifiedCB);
			this.Controls.Add(this.DefaultTargetItemConnectedCB);
			this.Controls.Add(this.DefaultSourceItemConnectedSpecifiedCB);
			this.Controls.Add(this.DefaultSourceItemConnectedCB);
			this.Controls.Add(this.BrowsePathsSpecifiedCB);
			this.Controls.Add(this.ConnectionNameSpecifiedCB);
			this.Controls.Add(this.ConnectionNameTB);
			this.Controls.Add(this.VendorDataLB);
			this.Controls.Add(this.DescriptionLB);
			this.Controls.Add(this.DeadbandLB);
			this.Controls.Add(this.UpdateRateLB);
			this.Controls.Add(this.EnableSubstituteValueLB);
			this.Controls.Add(this.SubstituteValueLB);
			this.Controls.Add(this.DefaultOverrideValueLB);
			this.Controls.Add(this.DefaultOverriddenLB);
			this.Controls.Add(this.DefaultTargetItemConnectedLB);
			this.Controls.Add(this.DefaultSourceItemConnectedLB);
			this.Controls.Add(this.BrowsePathsLB);
			this.Controls.Add(this.ConnectionNameLB);
			this.Name = "ConnectionEditCtrl";
			this.Size = new System.Drawing.Size(520, 456);
			((System.ComponentModel.ISupportInitialize)(this.SourceItemQueueSizeCTRL)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DeadbandCTRL)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UpdateRateCTRL)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Public Interface
		/// <summary>
		/// The target server.
		/// </summary>
		public Opc.Dx.Server TargetServer
		{
			get	 { return m_server;  }
			
			set	
			{ 
				m_server = value; 

				SourceServerNameCB.Items.Clear();

				if (m_server != null)
				{
					foreach (SourceServer server in m_server.SourceServers)
					{
						SourceServerNameCB.Items.Add(server.Name);
					}
				}
			}
		}

		/// <summary>
		/// Whether the control is editing a 'mask' or an 'instance'
		/// </summary>
		public bool IsMask
		{
			get	 { return m_isMask;  }
			
			set	
			{ 
				m_isMask = value; 

				ConnectionNameSpecifiedCB.Checked = !m_isMask;
				BrowsePathsSpecifiedCB.Checked = !m_isMask;
				DescriptionSpecifiedCB.Checked = !m_isMask;
				KeywordSpecifiedCB.Checked = !m_isMask;
				DefaultSourceItemConnectedSpecifiedCB.Checked = !m_isMask;
				DefaultTargetItemConnectedSpecifiedCB.Checked = !m_isMask;
				DefaultOverriddenSpecifiedCB.Checked = !m_isMask;
				DefaultOverrideValueSpecifiedCB.Checked = !m_isMask;
				SubstituteValueSpecifiedCB.Checked = !m_isMask;
				EnableSubstituteValueSpecifiedCB.Checked = !m_isMask;
				TargetItemPathSpecifiedCB.Checked = !m_isMask;
				TargetItemNameSpecifiedCB.Checked = !m_isMask;
				SourceServerNameSpecifiedCB.Checked = !m_isMask;
				SourceItemPathSpecifiedCB.Checked = !m_isMask;
				SourceItemNameSpecifiedCB.Checked = !m_isMask;
				SourceItemQueueSizeSpecifiedCB.Checked = !m_isMask;
				UpdateRateSpecifiedCB.Checked = !m_isMask;
				DeadbandSpecifiedCB.Checked = !m_isMask;
				VendorDataSpecifiedCB.Checked = !m_isMask;
			}
		}
		#endregion

		#region IEditObjectCtrl Interface
		/// <summary>
		/// Set all fields to default values.
		/// </summary>
		public void SetDefaults()
		{
			ConnectionNameTB.Text = "";
			BrowsePathsCTRL.Value = new string[] { "/" };
		    DescriptionTB.Text = "";
			KeywordTB.Text = "";
			DefaultSourceItemConnectedCB.Checked = false;
			DefaultTargetItemConnectedCB.Checked = false;
			DefaultOverriddenCB.Checked = false;
			DefaultOverrideValueCTRL.Value = null;
			SubstituteValueCTRL.Value = null;
			EnableSubstituteValueCB.Checked = false;
			TargetItemPathTB.Text = "";
			TargetItemNameTB.Text = "";
			SourceServerNameCB.Text = "";
			SourceItemPathTB.Text = "";
			SourceItemNameTB.Text = "";
			SourceItemQueueSizeCTRL.Value = 0;
			UpdateRateCTRL.Value = 0;
			DeadbandCTRL.Value = 0;
			VendorDataTB.Text = "";
		}

		/// <summary>
		/// Copy values from control into object - throw exceptions on error.
		/// </summary>
		public object Get()
		{
			Opc.Dx.DXConnection connection = new Opc.Dx.DXConnection(m_identifier);

			// set output default values.
			connection.BrowsePaths.Clear();
			connection.Name = null;
			connection.Description = null;
			connection.Keyword = null;
			connection.DefaultSourceItemConnected = false;
			connection.DefaultSourceItemConnectedSpecified = false;
			connection.DefaultTargetItemConnected = false;
			connection.DefaultTargetItemConnectedSpecified = false;
			connection.DefaultOverridden = false;
			connection.DefaultOverriddenSpecified = false;
			connection.DefaultOverrideValue = null;
			connection.SubstituteValue = null;
			connection.EnableSubstituteValue = false;
			connection.EnableSubstituteValueSpecified = false;
			connection.TargetItemPath = null;
			connection.TargetItemName = null;
			connection.SourceServerName = null;
			connection.SourceItemPath = null;
			connection.SourceItemName = null;
			connection.SourceItemQueueSize = 0;
			connection.SourceItemQueueSizeSpecified = false;
			connection.UpdateRate = 0;
			connection.UpdateRateSpecified = false;
			connection.Deadband = 0;
			connection.DeadbandSpecified = false;
			connection.VendorData = null;

			// name
			if (ConnectionNameSpecifiedCB.Checked)
			{
				connection.Name = ConnectionNameTB.Text;
			}

			// browse paths
			if (BrowsePathsSpecifiedCB.Checked)
			{
				object browsePaths = BrowsePathsCTRL.Value;

				if (typeof(string[]).IsInstanceOfType(browsePaths))
				{
					connection.BrowsePaths.AddRange((string[])browsePaths);
				}
			}

			// description
			if (DescriptionSpecifiedCB.Checked)
			{
				connection.Description = DescriptionTB.Text;
			}

			// keyword
			if (KeywordSpecifiedCB.Checked)
			{
				connection.Keyword = KeywordTB.Text;
			}

			// default source item connected
			if (DefaultSourceItemConnectedSpecifiedCB.Checked)
			{
				connection.DefaultSourceItemConnected = DefaultSourceItemConnectedCB.Checked;
				connection.DefaultSourceItemConnectedSpecified = true;
			}

			// default target item connected
			if (DefaultTargetItemConnectedSpecifiedCB.Checked)
			{
				connection.DefaultTargetItemConnected = DefaultTargetItemConnectedCB.Checked;
				connection.DefaultTargetItemConnectedSpecified = true;
			}

			// default overridden
			if (DefaultOverriddenSpecifiedCB.Checked)
			{
				connection.DefaultOverridden = DefaultOverriddenCB.Checked;
				connection.DefaultOverriddenSpecified = true;
			}

			// default override value
			if (DefaultOverrideValueSpecifiedCB.Checked)
			{
				connection.DefaultOverrideValue = DefaultOverrideValueCTRL.Value;
			}

			// substitute value
			if (SubstituteValueSpecifiedCB.Checked)
			{
				connection.SubstituteValue = SubstituteValueCTRL.Value;
			}

			// enable substitute value
			if (EnableSubstituteValueSpecifiedCB.Checked)
			{
				connection.EnableSubstituteValue = EnableSubstituteValueCB.Checked;
				connection.EnableSubstituteValueSpecified = true;
			}

			// target item name
			if (TargetItemNameSpecifiedCB.Checked)
			{
				connection.TargetItemName = TargetItemNameTB.Text;
			}

			// target item path
			if (TargetItemPathSpecifiedCB.Checked)
			{
				connection.TargetItemPath = TargetItemPathTB.Text;
			}

			// source server name
			if (SourceServerNameSpecifiedCB.Checked)
			{
				connection.SourceServerName = (string)SourceServerNameCB.Items[SourceServerNameCB.SelectedIndex];
			}

			// source item name
			if (SourceItemNameSpecifiedCB.Checked)
			{
				connection.SourceItemName = SourceItemNameTB.Text;
			}

			// source item path
			if (TargetItemPathSpecifiedCB.Checked)
			{
				connection.SourceItemPath = SourceItemPathTB.Text;
			}

			// source item queue size
			if (SourceItemQueueSizeSpecifiedCB.Checked)
			{
				connection.SourceItemQueueSize = (int)SourceItemQueueSizeCTRL.Value;
				connection.SourceItemQueueSizeSpecified = true;
			}

			// update rate
			if (UpdateRateSpecifiedCB.Checked)
			{
				connection.UpdateRate = (int)UpdateRateCTRL.Value;
				connection.UpdateRateSpecified = true;
			}

			// deadband
			if (DeadbandSpecifiedCB.Checked)
			{
				connection.Deadband = (float)DeadbandCTRL.Value;
				connection.DeadbandSpecified = true;
			}

			// vendor data
			if (VendorDataSpecifiedCB.Checked)
			{
				connection.VendorData = VendorDataTB.Text;
			}

			return connection;
		}
		
		/// <summary>
		/// Copy object values into controls.
		/// </summary>
		public void Set(object value)
		{
			// check for valid value.
			if (!typeof(Opc.Dx.DXConnection).IsInstanceOfType(value)) 
			{ 
				SetDefaults(); 
				return;
			}

			Opc.Dx.DXConnection connection = (Opc.Dx.DXConnection)value;

			// save item identifier (including client and server handles).
			m_identifier = new Opc.Dx.ItemIdentifier(connection);

			ConnectionNameTB.Text = connection.Name;
			BrowsePathsCTRL.Value = connection.BrowsePaths.ToArray();
			DescriptionTB.Text = connection.Description;
			KeywordTB.Text = connection.Keyword;
			DefaultSourceItemConnectedCB.Checked = connection.DefaultSourceItemConnected;
			DefaultTargetItemConnectedCB.Checked = connection.DefaultTargetItemConnected;
			DefaultOverriddenCB.Checked = connection.DefaultOverridden;
			DefaultOverrideValueCTRL.Value = connection.DefaultOverrideValue;			
			SubstituteValueCTRL.Value = connection.SubstituteValue;
			EnableSubstituteValueCB.Checked = connection.EnableSubstituteValue;
			TargetItemNameTB.Text = connection.TargetItemName;
			TargetItemPathTB.Text = connection.TargetItemPath;
			SourceServerNameCB.SelectedIndex = SourceServerNameCB.FindStringExact(connection.SourceServerName);
			SourceItemNameTB.Text = connection.SourceItemName;
			SourceItemPathTB.Text = connection.SourceItemPath;
			SourceItemQueueSizeCTRL.Value = connection.SourceItemQueueSize;		
			UpdateRateCTRL.Value = connection.UpdateRate;	
			DeadbandCTRL.Value = (decimal)connection.Deadband;
			VendorDataTB.Text = connection.VendorData;

			ConnectionNameSpecifiedCB.Checked = connection.Name != null;
			BrowsePathsSpecifiedCB.Checked = connection.BrowsePaths.Count > 0;
			DescriptionSpecifiedCB.Checked = connection.Description != null;
			KeywordSpecifiedCB.Checked = connection.Keyword != null;
			DefaultSourceItemConnectedSpecifiedCB.Checked = connection.DefaultSourceItemConnectedSpecified;
			DefaultTargetItemConnectedSpecifiedCB.Checked = connection.DefaultTargetItemConnectedSpecified;			
			DefaultOverriddenSpecifiedCB.Checked = connection.DefaultOverriddenSpecified;
			DefaultOverrideValueSpecifiedCB.Checked = connection.DefaultOverrideValue != null;
			SubstituteValueSpecifiedCB.Checked = connection.SubstituteValue != null;
			EnableSubstituteValueSpecifiedCB.Checked = connection.EnableSubstituteValueSpecified;
			TargetItemNameSpecifiedCB.Checked = connection.TargetItemName != null;
			TargetItemPathSpecifiedCB.Checked = connection.TargetItemPath != null;
			SourceServerNameSpecifiedCB.Checked = connection.SourceServerName != null;
			SourceItemNameSpecifiedCB.Checked = connection.SourceItemName != null;
			SourceItemPathSpecifiedCB.Checked = connection.SourceItemPath != null;
			SourceItemQueueSizeSpecifiedCB.Checked = connection.SourceItemQueueSizeSpecified;
			UpdateRateSpecifiedCB.Checked = connection.UpdateRateSpecified;
			DeadbandSpecifiedCB.Checked = connection.DeadbandSpecified;
			VendorDataSpecifiedCB.Checked = connection.VendorData != null;
			
			if (!m_isMask && connection.Name == null)
			{
				ConnectionNameSpecifiedCB.Checked = true;
				BrowsePathsSpecifiedCB.Checked = true;
				DescriptionSpecifiedCB.Checked = true;
				KeywordSpecifiedCB.Checked = true;
				DefaultSourceItemConnectedSpecifiedCB.Checked = true;
				DefaultTargetItemConnectedSpecifiedCB.Checked = true;	
				DefaultOverriddenSpecifiedCB.Checked = true;
				DefaultOverrideValueSpecifiedCB.Checked = true;
				SubstituteValueSpecifiedCB.Checked = true;
				EnableSubstituteValueSpecifiedCB.Checked = true;
				TargetItemNameSpecifiedCB.Checked = true;
				TargetItemPathSpecifiedCB.Checked = true;
				SourceServerNameSpecifiedCB.Checked = true;
				SourceItemNameSpecifiedCB.Checked = true;
				SourceItemPathSpecifiedCB.Checked = true;
				SourceItemQueueSizeSpecifiedCB.Checked = true;
				UpdateRateSpecifiedCB.Checked = true;
				DeadbandSpecifiedCB.Checked = true;
				VendorDataSpecifiedCB.Checked = true;
			}
		}

		/// <summary>
		/// Creates a new object.
		/// </summary>
		public object Create()
		{
			Opc.Dx.DXConnection connection = new Opc.Dx.DXConnection();

			if (!m_isMask)
			{
				connection.BrowsePaths.Add("/");
			}

			return connection;
		}
		#endregion

		#region Private Members
		private Opc.Dx.ItemIdentifier m_identifier = null;
		private bool m_isMask = false;
		private Opc.Dx.Server m_server = null;
		#endregion

		/// <summary>
		/// Toggles the enabled state of controls based on the specified check boxes.
		/// </summary>
		private void Specified_CheckedChanged(object sender, System.EventArgs e)
		{			
			ConnectionNameTB.Enabled = ConnectionNameSpecifiedCB.Checked;
			BrowsePathsCTRL.Enabled = BrowsePathsSpecifiedCB.Checked;
			DescriptionTB.Enabled = DescriptionSpecifiedCB.Checked;
			KeywordTB.Enabled = KeywordSpecifiedCB.Checked;
			DefaultSourceItemConnectedCB.Enabled = DefaultSourceItemConnectedSpecifiedCB.Checked;		
			DefaultTargetItemConnectedCB.Enabled = DefaultTargetItemConnectedSpecifiedCB.Checked;
			DefaultOverriddenCB.Enabled = DefaultOverriddenSpecifiedCB.Checked;
			DefaultOverrideValueCTRL.Enabled = DefaultOverrideValueSpecifiedCB.Checked;			
			SubstituteValueCTRL.Enabled = SubstituteValueSpecifiedCB.Checked;
			EnableSubstituteValueCB.Enabled = EnableSubstituteValueSpecifiedCB.Checked;
			TargetItemNameTB.Enabled = TargetItemNameSpecifiedCB.Checked;
			TargetItemBTN.Enabled = TargetItemNameSpecifiedCB.Checked;
			TargetItemPathTB.Enabled = TargetItemPathSpecifiedCB.Checked;
			SourceServerNameCB.Enabled = SourceServerNameSpecifiedCB.Checked;
			SourceServerBTN.Enabled = SourceServerNameSpecifiedCB.Checked;
			SourceItemNameTB.Enabled = SourceItemNameSpecifiedCB.Checked;
			SourceItemBTN.Enabled = SourceItemNameSpecifiedCB.Checked;
			SourceItemPathTB.Enabled = SourceItemPathSpecifiedCB.Checked;
			SourceItemQueueSizeCTRL.Enabled = SourceItemQueueSizeSpecifiedCB.Checked;			
			UpdateRateCTRL.Enabled = UpdateRateSpecifiedCB.Checked;			
			DeadbandCTRL.Enabled = DeadbandSpecifiedCB.Checked;
			VendorDataTB.Enabled = VendorDataSpecifiedCB.Checked;
		}

		private void SourceServerBTN_Click(object sender, System.EventArgs e)
		{		
			try
			{
				// select new source server.
				Opc.Da.Server server = (Opc.Da.Server)new SelectServerDlg().ShowDialog(Opc.Specification.COM_DA_30);

				if (server == null)
				{
					return;
				}

				string serverName = server.Name.Replace('/', ' ');
				
				// check if it already exists.
				foreach (SourceServer currentServer in m_server.SourceServers)
				{
					if (currentServer.Name == serverName)
					{						
						SourceServerNameCB.SelectedIndex = SourceServerNameCB.FindStringExact(serverName);
						return;
					}
				}

				// create new source server.
				Opc.Dx.SourceServer sourceServer = new SourceServer();

				sourceServer.Name                      = serverName;
				sourceServer.Description               = null;
				sourceServer.ServerType                = Opc.Dx.ServerType.COM_DA205;
				sourceServer.ServerURL                 = server.Url.ToString();
				sourceServer.DefaultConnected          = false;
				sourceServer.DefaultConnectedSpecified = true;

				// add new server.
				m_server.AddSourceServer(sourceServer);

				// update controls.
				int index = SourceServerNameCB.Items.Add(sourceServer.Name);
				SourceServerNameCB.SelectedIndex = index;
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}	

		/// <summary>
		/// Prompts a user to select an item in the source server's address space.
		/// </summary>
		private void SourceItemBTN_Click(object sender, System.EventArgs e)
		{		
			try
			{
				// specify a source server.
				if (SourceServerNameCB.SelectedIndex == -1)
				{
					SourceServerBTN_Click(sender, e);
				}

				// give up if no source server choosen.
				if (SourceServerNameCB.SelectedIndex == -1)
				{
					return;
				}

				// look up source server object.
				string serverName = (string)SourceServerNameCB.Items[SourceServerNameCB.SelectedIndex];

				SourceServer sourceServer = m_server.SourceServers[serverName];

				if (sourceServer == null)
				{
					return;
				}

				// create DA wrapper for source server.
				Opc.Da.Server server = (Opc.Da.Server)Opc.SampleClient.Factory.GetServerForURL(new URL(sourceServer.ServerURL));

				// select item in server.
				Opc.ItemIdentifier itemID = new BrowseItemsDlg().ShowDialog(server);

				if (itemID != null)
				{
					SourceItemNameTB.Text             = itemID.ItemName;
					SourceItemPathTB.Text             = itemID.ItemPath;
					SourceItemPathSpecifiedCB.Checked = itemID.ItemPath != null;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}

		/// <summary>
		/// Prompts a user to select an item in the target server's address space.
		/// </summary>
		private void TargetItemBTN_Click(object sender, System.EventArgs e)
		{	
			try
			{
				Opc.ItemIdentifier itemID = new BrowseItemsDlg().ShowDialog(m_server);

				if (itemID != null)
				{
					TargetItemNameTB.Text             = itemID.ItemName;
					TargetItemPathTB.Text             = itemID.ItemPath;
					TargetItemPathSpecifiedCB.Checked = itemID.ItemPath != null;

					ItemPropertyCollection[] properties = m_server.GetProperties(
						new Opc.ItemIdentifier[] { itemID },
						new PropertyID[] { Property.DATATYPE },
						true);

					if (properties != null && properties.Length == 1 && properties[0].Count == 1)
					{
						object datatype = properties[0][0].Value;

						if (typeof(System.Type).IsInstanceOfType(datatype))
						{
							try
							{
								DefaultOverrideValueCTRL.Value = Opc.Convert.ChangeType(DefaultOverrideValueCTRL.Value, (System.Type)datatype);
							}
							catch
							{
							}

							try
							{
								SubstituteValueCTRL.Value = Opc.Convert.ChangeType(SubstituteValueCTRL.Value, (System.Type)datatype);
							}
							catch
							{
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
			}
		}
	}
}
