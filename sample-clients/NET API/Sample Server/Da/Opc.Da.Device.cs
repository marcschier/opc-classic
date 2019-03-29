//============================================================================
// TITLE: Device.cs
//
// CONTENTS:
// 
// A class that represents a device with a set of simulated data points.
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
// 2004/03/26 RSA   Initial implementation.

using System;
using System.Xml;
using System.Net;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.Runtime.InteropServices;
using System.IO;
using Opc;
using Opc.Da;

namespace Opc.Da.SampleServer
{
	/// <summary>
	/// A class that represents a device with a set of simulated data points.
	/// </summary>
	[ComVisible(false)]
	public class  Device : IDevice, IDisposable
	{	
		/// <summary>
		/// Initializes the object with default values.
		/// </summary>
		public Device() {}

		/// <summary>
		/// Initializes the object from the XML stream.
		/// </summary>
		public bool Initialize(Cache cache)
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Device");

				// always use 'en-US' for conversions but save the default culture.
				CultureInfo culture = Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

				string configFile = null; 

				try
				{
					// get configuration file name from application settings.
					configFile = (string)(new AppSettingsReader().GetValue("Opc.Da.SampleServer.Device", typeof(string)));
					
					// check for relative file path.
					int index = configFile.IndexOf(":");

					if (index == -1)
					{
						string filePath = Assembly.GetExecutingAssembly().CodeBase;

						index = filePath.LastIndexOf("://");

						if (index != -1)
						{
							filePath =  filePath.Substring(index+4);
						}

						for (int ii = 0; ii < 2; ii++)
						{
							index = filePath.LastIndexOf("/");

							if (index != -1)
							{
								filePath = filePath.Substring(0, index);
							}
						}

						configFile = filePath + "/" + configFile;
					}
				}
				catch
				{
					// get configuration file name from process name.
					configFile = Process.GetCurrentProcess().MainModule.FileName;

					int index = configFile.LastIndexOf(".");

					if (index != -1)
					{
						configFile = configFile.Substring(0, index);
					}

					configFile += ".device.xml";
				}

				try
				{
					// open the configuration file.
					XmlTextReader reader = new XmlTextReader(File.OpenRead(configFile));

					// read address space.
					reader.ReadStartElement(SERVER);

					// skip self registration element.
					if (reader.IsStartElement(SELF_REG_INFO))
					{
						reader.Skip();
					}

					// read top level elements.
					while (reader.IsStartElement(BROWSE_ELEMENT))
					{					
						ReadElement(reader, null);
					}

					reader.ReadEndElement();

					// build address space in cache.
					BuildAddressSpace(cache, null);

					// close the configuration file.
					reader.Close();

					// start the device simulation.					
					ThreadPool.QueueUserWorkItem(new WaitCallback(OnUpdate));

					return true;
				}
				catch (Exception e)
				{
					string message = e.Message;
					return false;
				}
				finally
				{
					Thread.CurrentThread.CurrentCulture = culture;
				}
			}
		}

		#region IDisposable Members
		/// <summary>
		/// Stops all threads and releases all resources.
		/// </summary>
		public void Dispose()
		{
			lock (this)
			{
				m_disposed = true;
			}
		}
		#endregion
		
		#region IDevice Members
		/// <summary>
		/// Adds the address space for the device to the cache.
		/// </summary>
		public bool BuildAddressSpace(Cache cache, string browsePath)
		{
			lock (this)
			{
				foreach (DeviceItem item in m_itemList)
				{
					cache.AddItemAndLink(item.ItemID, this);
				}

				return true;
			}
		}

		/// <summary>
		/// Removes the address space for the device to the cache.
		/// </summary>
		public void ClearAddressSpace(Cache cache)
		{
			lock (this)
			{
				foreach (DeviceItem item in m_itemList)
				{
					cache.RemoveItemAndLink(item.ItemID);
				}
			}
		}

		/// <summary>
		/// Returns whether item id belongs to the device's address space.
		/// </summary>
		public bool IsKnownItem(string itemID)
		{
			lock (this)
			{
				return m_items.Contains(itemID);
			}
		}

		/// <summary>
		/// Returns all available properties for the specified item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(
			string itemID, 
			bool   returnValues)
		{
			lock (this)
			{
				// initialize result.
				ItemPropertyCollection properties = new ItemPropertyCollection();

				properties.ItemName       = itemID;
				properties.ItemPath       = null;
				properties.ResultID       = ResultID.S_OK;
				properties.DiagnosticInfo = null;

				// lookup item.
				DeviceItem item = (DeviceItem)m_items[itemID];

				if (item == null)
				{
					properties.ResultID = ResultID.Da.E_UNKNOWN_ITEM_NAME;
					return properties;
				}

				// fetch properties.
				return item.GetAvailableProperties(returnValues);
			}
		}

		/// <summary>
		/// Returns the specified properties for the specified item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(
			string       itemID, 
			PropertyID[] propertyIDs, 
			bool         returnValues)
		{
			lock (this)
			{
				// initialize result.
				ItemPropertyCollection properties = new ItemPropertyCollection();

				properties.ItemName       = itemID;
				properties.ItemPath       = null;
				properties.ResultID       = ResultID.S_OK;
				properties.DiagnosticInfo = null;

				// lookup item.
				DeviceItem item = (DeviceItem)m_items[itemID];

				if (item == null)
				{
					properties.ResultID = ResultID.Da.E_UNKNOWN_ITEM_NAME;
					return properties;
				}

				// fetch properties.
				return item.GetAvailableProperties(propertyIDs, returnValues);
			}
		}

		/// <summary>
		/// Reads the value of the specified item property.
		/// </summary>
		public Opc.Da.ItemValueResult Read(string itemID, PropertyID propertyID)
		{
			lock (this)
			{
				// initialize result.
				ItemValueResult result = new ItemValueResult();

				result.ItemName       = itemID;
				result.ItemPath       = null;
				result.ResultID       = ResultID.S_OK;
				result.DiagnosticInfo = null;

				// lookup item.
				DeviceItem item = (DeviceItem)m_items[itemID];

				if (item == null)
				{
					result.ResultID = ResultID.Da.E_UNKNOWN_ITEM_NAME;
					return result;
				}

				// read value.
				return item.Read(propertyID);
			}
		}

		/// <summary>
		/// Writes the value of the specified item property.
		/// </summary>
		public Opc.IdentifiedResult Write(
			string           itemID, 
			PropertyID       propertyID, 
			Opc.Da.ItemValue value)
		{
			lock (this)
			{
				// lookup item.
				DeviceItem item = (DeviceItem)m_items[itemID];

				if (item == null)
				{
					return new IdentifiedResult(itemID, ResultID.Da.E_UNKNOWN_ITEM_NAME);
				}

				// write value.
				return item.Write(propertyID, value);
			}
		}
		#endregion

		#region Private Members
		private const string SERVER         = "Server";
		private const string BROWSE_ELEMENT = "BrowseElement";
		private const string ITEM           = "Item";
		private const string CHILDREN       = "Children";
		private const string ELEMENT_NAME   = "ElementName";
		private const string SELF_REG_INFO  = "SelfRegInfo";

		private bool m_disposed = false;
		private Hashtable m_items = new Hashtable();
		private ArrayList m_itemList = new ArrayList();

		/// <summary>
		/// Reads a browse element from the XML stream.
		/// </summary>
		private void ReadElement(XmlTextReader reader, string browsePath)
		{
			string itemID = (browsePath == null)?"":browsePath + "/";

			if (reader.MoveToAttribute(ELEMENT_NAME))
			{
				itemID += reader.Value;
			}

			reader.ReadStartElement(BROWSE_ELEMENT);

			// create item.
			if (reader.IsStartElement(ITEM))
			{
				DeviceItem item = new DeviceItem(itemID);

				if (item.Initialize(reader))
				{
					m_items[itemID] = item; 
					m_itemList.Add(item);
				}
			}

			// read child elements.
			if (reader.IsStartElement(CHILDREN))
			{
				reader.ReadStartElement();

				while (reader.IsStartElement(BROWSE_ELEMENT))
				{
					ReadElement(reader, itemID);
				}
				
				reader.ReadEndElement();
			}

			reader.ReadEndElement();
		}

		/// <summary>
		/// Periodically updates the values of all simulated data points.
		/// </summary>
		private void OnUpdate(object stateInfo) 
		{			
			int  delay = 0;
			long ticks = 0;

			do
			{
				// sleep until next update.
				Thread.Sleep((delay > 0 && delay < Cache.MAX_UPDATE_RATE)?Cache.MAX_UPDATE_RATE - delay:Cache.MAX_UPDATE_RATE);

				delay = Environment.TickCount;

				// update each subscription.
				lock (this)
				{
					// check if object has been disposed.
					if (m_disposed)
					{
						break;
					}

					// process each item.
					foreach (DeviceItem item in m_items.Values)
					{				
						try
						{
							item.Update(ticks, Cache.MAX_UPDATE_RATE);											
						}
						catch (Exception e)
						{
							string message = e.Message;
						}
					}
				}
				
				delay = Environment.TickCount - delay;

				// increment tick count.
				ticks++;
			}
			while (true);
		}
		#endregion
	}
}
