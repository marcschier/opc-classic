//============================================================================
// TITLE: BitMaskCtrl.cs
//
// CONTENTS:
// 
// A control used to select multiple values for an enumeration which is a bitmask.
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
// 2003/11/10 RSA   Initial implementation.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace Opc.SampleClient
{
	/// <summary>
	/// A control used to select a valid value for any bit mask expressed as an enumeration.
	/// </summary>
	public class BitMaskCtrl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.ListView BitMaskLV;
		private System.ComponentModel.IContainer components = null;

		public BitMaskCtrl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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
			this.BitMaskLV = new System.Windows.Forms.ListView();
			this.SuspendLayout();
			// 
			// BitMaskLV
			// 
			this.BitMaskLV.BackColor = System.Drawing.SystemColors.Window;
			this.BitMaskLV.CheckBoxes = true;
			this.BitMaskLV.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BitMaskLV.FullRowSelect = true;
			this.BitMaskLV.Location = new System.Drawing.Point(0, 0);
			this.BitMaskLV.Name = "BitMaskLV";
			this.BitMaskLV.Size = new System.Drawing.Size(400, 304);
			this.BitMaskLV.TabIndex = 0;
			this.BitMaskLV.View = System.Windows.Forms.View.List;
			// 
			// BitMaskCtrl
			// 
			this.Controls.Add(this.BitMaskLV);
			this.Name = "BitMaskCtrl";
			this.Size = new System.Drawing.Size(400, 304);
			this.ResumeLayout(false);

		}
		#endregion
	
		#region Private Members
		#endregion
		
		#region Public Interface
		/// <summary>
		/// The enumeration that defines the bit masks.
		/// </summary>
		[Browsable(false)]
		public System.Type Type
		{
			get { return (System.Type)BitMaskLV.Tag;  }
			
			set
			{
				UpdateMasks(value);
			}
		}

		/// <summary>
		/// An integer containing the current selection of bit masks.
		/// </summary>
		[Browsable(false)]
		public int Value
		{
			get { return GetSelection();  }
			set { UpdateSelection(value); }
		}

		/// <summary>
		/// Whether the value displayed by the control can be changed.
		/// </summary>
		public bool ReadOnly
		{
			get { return !BitMaskLV.Enabled;  }
			set { BitMaskLV.Enabled = !value; }
		}
		#endregion

		#region Private Methods
		/// <summary>
		/// Populates the list box from the values of an enumeration.
		/// </summary>
		private void UpdateMasks(System.Type type)
		{
			BitMaskLV.Clear();			
			BitMaskLV.Tag = type;
            
			// check if an enum type was specified 
			if (type == null)
			{
				return;
			}

			Array values = Enum.GetValues(type);

			foreach (object enumValue in values)
			{
				uint bits = System.Convert.ToUInt32(enumValue);

				// ignore values that combine multiple bits. 
				switch (bits)
				{
					case 0x0001:
					case 0x0002:
					case 0x0004:
					case 0x0008:
					case 0x0010:
					case 0x0020:
					case 0x0040:
					case 0x0080:
					case 0x0100:
					case 0x0200:
					case 0x0400:
					case 0x0800:
					case 0x1000:
					case 0x2000:
					case 0x4000:
					case 0x8000:
					{
						ListViewItem item = new ListViewItem(enumValue.ToString());
					
						item.Tag       = enumValue;				
						item.BackColor = (ReadOnly)?SystemColors.Control:SystemColors.Window;

						BitMaskLV.Items.Add(item);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Sets the checkboxes based on the bits in an integer.
		/// </summary>
		private void UpdateSelection(int value)
		{
			foreach (ListViewItem item in BitMaskLV.Items)
			{
				if ((value & System.Convert.ToInt32(item.Tag)) != 0)
				{
					item.Checked = true;
				}
				else
				{
					item.Checked = false;
				}
			}
		}

		/// <summary>
		/// Sets the bits of an integer based on the value of the checkboxes.
		/// </summary>
		private int GetSelection()
		{
			int value = 0;

			foreach (ListViewItem item in BitMaskLV.Items)
			{
				if (item.Checked)
				{
					value |= System.Convert.ToInt32(item.Tag);
				}
			}

			return value;
		}
		#endregion
	}
}
  