//============================================================================
// TITLE: SubscriptionFiltersEditCtrl.cs
//
// CONTENTS:
// 
// A control used to edit the filters of a subscription.
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
using Opc.Ae;
using Opc.SampleClient;

namespace Opc.Ae.SampleClient
{
	/// <summary>
	/// A control used to edit the filters of a subscription.
	/// </summary>
	public class SubscriptionFiltersEditCtrl : System.Windows.Forms.UserControl, IEditObjectCtrl
	{
		private System.Windows.Forms.NumericUpDown LowSeverityCTRL;
		private System.Windows.Forms.NumericUpDown HighSeverityCTRL;
		private System.Windows.Forms.Label LowSeverityLB;
		private System.Windows.Forms.Label HighSeverityLB;
		private System.Windows.Forms.Label SimpleEventsLB;
		private System.Windows.Forms.CheckBox SimpleEventCHK;
		private System.Windows.Forms.CheckBox TrackingEventsCHK;
		private System.Windows.Forms.Label TrackingEventsLB;
		private System.Windows.Forms.CheckBox ConditionEventsCHK;
		private System.Windows.Forms.Label ConditionEventsLB;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SubscriptionFiltersEditCtrl()
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
			this.LowSeverityCTRL = new System.Windows.Forms.NumericUpDown();
			this.HighSeverityCTRL = new System.Windows.Forms.NumericUpDown();
			this.LowSeverityLB = new System.Windows.Forms.Label();
			this.HighSeverityLB = new System.Windows.Forms.Label();
			this.SimpleEventsLB = new System.Windows.Forms.Label();
			this.SimpleEventCHK = new System.Windows.Forms.CheckBox();
			this.TrackingEventsCHK = new System.Windows.Forms.CheckBox();
			this.TrackingEventsLB = new System.Windows.Forms.Label();
			this.ConditionEventsCHK = new System.Windows.Forms.CheckBox();
			this.ConditionEventsLB = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.LowSeverityCTRL)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HighSeverityCTRL)).BeginInit();
			this.SuspendLayout();
			// 
			// LowSeverityCTRL
			// 
			this.LowSeverityCTRL.Location = new System.Drawing.Point(104, 97);
			this.LowSeverityCTRL.Maximum = new System.Decimal(new int[] {
																			1000,
																			0,
																			0,
																			0});
			this.LowSeverityCTRL.Minimum = new System.Decimal(new int[] {
																			1,
																			0,
																			0,
																			0});
			this.LowSeverityCTRL.Name = "LowSeverityCTRL";
			this.LowSeverityCTRL.Size = new System.Drawing.Size(72, 20);
			this.LowSeverityCTRL.TabIndex = 23;
			this.LowSeverityCTRL.Value = new System.Decimal(new int[] {
																		  1,
																		  0,
																		  0,
																		  0});
			// 
			// HighSeverityCTRL
			// 
			this.HighSeverityCTRL.Location = new System.Drawing.Point(104, 73);
			this.HighSeverityCTRL.Maximum = new System.Decimal(new int[] {
																			 1000,
																			 0,
																			 0,
																			 0});
			this.HighSeverityCTRL.Minimum = new System.Decimal(new int[] {
																			 1,
																			 0,
																			 0,
																			 0});
			this.HighSeverityCTRL.Name = "HighSeverityCTRL";
			this.HighSeverityCTRL.Size = new System.Drawing.Size(72, 20);
			this.HighSeverityCTRL.TabIndex = 22;
			this.HighSeverityCTRL.Value = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   0});
			// 
			// LowSeverityLB
			// 
			this.LowSeverityLB.Location = new System.Drawing.Point(0, 96);
			this.LowSeverityLB.Name = "LowSeverityLB";
			this.LowSeverityLB.TabIndex = 18;
			this.LowSeverityLB.Text = "Low Severity";
			this.LowSeverityLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// HighSeverityLB
			// 
			this.HighSeverityLB.Location = new System.Drawing.Point(0, 72);
			this.HighSeverityLB.Name = "HighSeverityLB";
			this.HighSeverityLB.TabIndex = 17;
			this.HighSeverityLB.Text = "High Severity";
			this.HighSeverityLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SimpleEventsLB
			// 
			this.SimpleEventsLB.Location = new System.Drawing.Point(0, 0);
			this.SimpleEventsLB.Name = "SimpleEventsLB";
			this.SimpleEventsLB.TabIndex = 24;
			this.SimpleEventsLB.Text = "Simple Events";
			this.SimpleEventsLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SimpleEventCHK
			// 
			this.SimpleEventCHK.Location = new System.Drawing.Point(104, -1);
			this.SimpleEventCHK.Name = "SimpleEventCHK";
			this.SimpleEventCHK.Size = new System.Drawing.Size(16, 24);
			this.SimpleEventCHK.TabIndex = 25;
			// 
			// TrackingEventsCHK
			// 
			this.TrackingEventsCHK.Location = new System.Drawing.Point(104, 23);
			this.TrackingEventsCHK.Name = "TrackingEventsCHK";
			this.TrackingEventsCHK.Size = new System.Drawing.Size(16, 24);
			this.TrackingEventsCHK.TabIndex = 27;
			// 
			// TrackingEventsLB
			// 
			this.TrackingEventsLB.Location = new System.Drawing.Point(0, 24);
			this.TrackingEventsLB.Name = "TrackingEventsLB";
			this.TrackingEventsLB.TabIndex = 26;
			this.TrackingEventsLB.Text = "Tracking Events";
			this.TrackingEventsLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ConditionEventsCHK
			// 
			this.ConditionEventsCHK.Location = new System.Drawing.Point(104, 47);
			this.ConditionEventsCHK.Name = "ConditionEventsCHK";
			this.ConditionEventsCHK.Size = new System.Drawing.Size(16, 24);
			this.ConditionEventsCHK.TabIndex = 29;
			// 
			// ConditionEventsLB
			// 
			this.ConditionEventsLB.Location = new System.Drawing.Point(0, 48);
			this.ConditionEventsLB.Name = "ConditionEventsLB";
			this.ConditionEventsLB.TabIndex = 28;
			this.ConditionEventsLB.Text = "Condition Events";
			this.ConditionEventsLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// SubscriptionFiltersEditCtrl
			// 
			this.Controls.Add(this.ConditionEventsCHK);
			this.Controls.Add(this.ConditionEventsLB);
			this.Controls.Add(this.TrackingEventsCHK);
			this.Controls.Add(this.TrackingEventsLB);
			this.Controls.Add(this.SimpleEventCHK);
			this.Controls.Add(this.SimpleEventsLB);
			this.Controls.Add(this.LowSeverityCTRL);
			this.Controls.Add(this.HighSeverityCTRL);
			this.Controls.Add(this.LowSeverityLB);
			this.Controls.Add(this.HighSeverityLB);
			this.Name = "SubscriptionFiltersEditCtrl";
			this.Size = new System.Drawing.Size(176, 120);
			((System.ComponentModel.ISupportInitialize)(this.LowSeverityCTRL)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HighSeverityCTRL)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region IEditObjectCtrl Members
		/// <summary>
		/// Set all fields to default values.
		/// </summary>
		public void SetDefaults()
		{
			SimpleEventCHK.Checked     = true;
			TrackingEventsCHK.Checked  = true;
			ConditionEventsCHK.Checked = true;
			HighSeverityCTRL.Value     = 1000;
			LowSeverityCTRL.Value      = 1;
		}

		/// <summary>
		/// Copy values from control into object - throw exceptions on error.
		/// </summary>
		public object Get()
		{
			Opc.Ae.SubscriptionFilters filters = new Opc.Ae.SubscriptionFilters();

			filters.EventTypes = 0;

			if (SimpleEventCHK.Checked)     filters.EventTypes |= (int)EventType.Simple;
			if (TrackingEventsCHK.Checked)  filters.EventTypes |= (int)EventType.Tracking;
			if (ConditionEventsCHK.Checked) filters.EventTypes |= (int)EventType.Condition;

			filters.HighSeverity = (int)HighSeverityCTRL.Value;
			filters.LowSeverity  = (int)LowSeverityCTRL.Value;

			return filters;
		}
		
		/// <summary>
		/// Copy object values into controls.
		/// </summary>
		public void Set(object value)
		{
			// check for null value.
			if (value == null) { SetDefaults(); return; }

			Opc.Ae.SubscriptionFilters filters = (Opc.Ae.SubscriptionFilters)value;
			
			SimpleEventCHK.Checked     = (filters.EventTypes & (int)EventType.Simple) != 0;
			TrackingEventsCHK.Checked  = (filters.EventTypes & (int)EventType.Tracking) != 0;
			ConditionEventsCHK.Checked = (filters.EventTypes & (int)EventType.Condition) != 0;
			HighSeverityCTRL.Value     = filters.HighSeverity;
			LowSeverityCTRL.Value      = filters.LowSeverity;
		}

		/// <summary>
		/// Creates a new object.
		/// </summary>
		public object Create()
		{
			Opc.Ae.SubscriptionFilters filters = new Opc.Ae.SubscriptionFilters();
			filters.EventTypes = (int)EventType.All;
			return filters;
		}
		#endregion
	}
}
