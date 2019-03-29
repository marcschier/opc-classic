//============================================================================
// TITLE: Cache.cs
//
// CONTENTS:
// 
// A shared cache of item values stored in a data access server. 
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
// 2003/03/26 RSA   Initial implementation.

using System;
using System.Xml;
using System.Net;
using System.Threading;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Resources;
using System.Reflection;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Opc;
using Opc.Da;

namespace Opc.Da.SampleServer
{
	/// <summary>
	/// An implementation of a Data Access server.
	/// </summary>
	[ComVisible(false)]
	public class  Cache : IDisposable
	{	
		#region Public Members
		/// <summary>
		/// The maximum rate at which items may be scaned.
		/// </summary>
		public const int MAX_UPDATE_RATE = 100;

		/// <summary>
		/// Returns the closest supported update rate.
		/// </summary>
		public static int AdjustUpdateRate(int updateRate)
		{
			if (updateRate%MAX_UPDATE_RATE != 0)
			{
				return (updateRate/MAX_UPDATE_RATE+1)*MAX_UPDATE_RATE;
			}

			return (updateRate > 0)?updateRate:MAX_UPDATE_RATE;
		}

		/// <summary>
		/// Initializes the object.
		/// </summary>
		public Cache(bool supportsCOM) 
		{
			m_supportsCOM = supportsCOM;
		}

		/// <summary>
		/// The set of locales supported by the server.
		/// </summary>
		public string[] SupportedLocales
		{
			get { return new string[] { "en", "fr", "de" }; }
		}
		
		/// <summary>
		/// Initializes the cache when the first server is created.
		/// </summary>
		public virtual void Initialize()
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				// create the resource manager.
				m_resourceManager  = new ResourceManager("OpcDa.Resources.Strings", Assembly.GetExecutingAssembly());

				// initialize status.
				m_status = new ServerStatus();

				m_status.VendorInfo     = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyDescriptionAttribute))).Description;
				m_status.ProductVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
				m_status.ServerState    = serverState.running;
				m_status.StatusInfo     = serverState.running.ToString();
				m_status.StartTime      = DateTime.Now;
				m_status.LastUpdateTime = DateTime.MinValue;
				m_status.CurrentTime    = DateTime.Now;

				// start the cache thread.
				ThreadPool.QueueUserWorkItem(new WaitCallback(OnUpdate));
			}
		}

		/// <summary>
		/// Returns a localized string with the specified name.
		/// </summary>
		public string GetString(string name, string locale)
		{
			// create a culture object.
			CultureInfo culture = null;
			
			try   { culture = new CultureInfo(locale); }
			catch {	culture = new CultureInfo(""); }

			// lookup resource string.
			try   { return m_resourceManager.GetString(name, culture); }
			catch {	return name; }
		}

		/// <summary>
		/// Adds a reference to the cache.
		/// </summary>
		public int AddRef()
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");
				return ++m_refs;
			}
		}

		/// <summary>
		/// Removes a reference to the cache.
		/// </summary>
		public int Release()
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");
				return --m_refs;
			}
		}

		/// <summary>
		/// Copies the current status into object passed in.
		/// </summary>
		public Opc.Da.ServerStatus GetStatus()
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				Opc.Da.ServerStatus status = new ServerStatus();

				status.VendorInfo     = m_status.VendorInfo;
				status.ProductVersion = m_status.ProductVersion;
				status.ServerState    = m_status.ServerState;
				status.StatusInfo     = m_status.StatusInfo;
				status.StartTime      = m_status.StartTime;
				status.LastUpdateTime = m_status.LastUpdateTime;
				status.CurrentTime    = DateTime.Now;

				return status;
			}
		}

		/// <summary>
		/// Returns the set of elements in the address space that meet the specified criteria.
		/// </summary>
		public Opc.Da.BrowseElement[] Browse(
			ItemIdentifier            itemID,
			BrowseFilters             filters,
			ref Opc.Da.BrowsePosition position)
		{	
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				BrowseElement element = m_addressSpace;
	
				// find desired element.
				string browsePath = (itemID != null)?itemID.ItemName:null;

				if (browsePath != null && browsePath.Length > 0)
				{
					element = m_addressSpace.Find(browsePath);

					if (element == null)
					{
						// check if browsing a property item.
						PropertyID property = Property.VALUE;

						string rootID = LookupProperty(browsePath, out property);

						if (rootID != null)
						{
							element = m_addressSpace.Find(rootID);
						}

						if (element == null)
						{
							throw new ResultIDException(ResultID.Da.E_UNKNOWN_ITEM_NAME);
						}

						// property items never contain children.
						return new Opc.Da.BrowseElement[0];
					}
				}

				// check if no elements exist.
				if (element.Count == 0)
				{
					return new Opc.Da.BrowseElement[0];
				}

				// determine start position.
				int start = 0;

				if (position != null)
				{
					start = ((BrowsePosition)position).Index;
					position.Dispose();
					position = null;
				}

				// process child nodes.
				ArrayList results = new ArrayList();

				for (int ii = start; ii < element.Count; ii++)
				{
					BrowseElement child = element.Child(ii);

					// exclude elements without children.
					if (filters.BrowseFilter == browseFilter.branch && child.Count == 0) 
					{
						continue;
					}

					// check if an element is an item.
					CacheItem item = (CacheItem)m_items[child.ItemID];

					// exclude elements which are not items.
					if (filters.BrowseFilter == browseFilter.item && item == null)
					{
						continue;
					}

					// apply name filter (using the SQL LIKE operator).
					if (filters.ElementNameFilter != null && filters.ElementNameFilter.Length > 0)
					{
						if (!Opc.Convert.Match(child.Name, filters.ElementNameFilter, true))
						{
							continue;
						}
					}

					// add element to list of results.
					Opc.Da.BrowseElement result = new Opc.Da.BrowseElement();

					result.Name        = child.Name;
					result.ItemName    = child.ItemID;
					result.ItemPath    = null;
					result.IsItem      = item != null;
					result.HasChildren = child.Count > 0;
					result.Properties  = null;

					// add properties to results.
					if (filters.ReturnAllProperties || filters.PropertyIDs != null)
					{
						result.Properties = GetProperties(item, (filters.ReturnAllProperties)?null:filters.PropertyIDs, filters.ReturnPropertyValues);
					}

					results.Add(result);

					// check if max elements exceeded.
					if (filters.MaxElementsReturned > 0 && results.Count >= filters.MaxElementsReturned)
					{
						if (ii+1 < element.Count)
						{
							position = new BrowsePosition(itemID, filters);
							((BrowsePosition)position).Index = ii+1;
						}

						break;
					}
				}
				

				// return results.
				return (Opc.Da.BrowseElement[])results.ToArray(typeof(Opc.Da.BrowseElement));
			}
		}

		/// <summary>
		/// Fetches the specified properties for an item.
		/// </summary>
		public ItemPropertyCollection GetProperties(
			ItemIdentifier itemID,
			PropertyID[]   propertyIDs,
			bool           returnValues)
		{
			if (itemID == null) throw new ArgumentNullException("itemID");

			lock (this)
			{	
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				ItemPropertyCollection properties = new ItemPropertyCollection();

				properties.ItemName       = itemID.ItemName;
				properties.ItemPath       = itemID.ItemPath;
				properties.ResultID       = ResultID.S_OK;
				properties.DiagnosticInfo = null;

				if (itemID.ItemName == null || itemID.ItemName.Length == 0)
				{
					properties.ResultID = ResultID.Da.E_INVALID_ITEM_NAME;
					return properties;
				}

				// find the item.
				CacheItem item = (CacheItem)m_items[itemID.ItemName];

				if (item == null)
				{
					properties.ResultID = ResultID.Da.E_UNKNOWN_ITEM_NAME;
					return properties;
				}
			
				// get the properties.
				if (propertyIDs == null)
				{
					properties = item.GetAvailableProperties(returnValues);
				}
				else
				{
					properties = item.GetAvailableProperties(propertyIDs, returnValues);
				}

				// return result.
				return properties;
			}
		}

		/// <summary>
		/// Adds an item without a link to the address space.
		/// </summary>
		public bool AddItem(string itemID, IDevice device)
		{
			lock (this)
			{
				// validate item id.
				if (itemID == null || itemID.Length == 0)
				{
					return false;
				}

				// check if item already exists.
				if (m_items.Contains(itemID))
				{
					return false;
				}

				// create new item and index by item id.
				m_items[itemID] = new CacheItem(itemID, device);
		
				return true;
			}
		}

		/// <summary>
		/// Removes an item from address space.
		/// </summary>
		public bool RemoveItem(string itemID)
		{
			lock (this)
			{
				// validate item id.
				if (itemID == null || itemID.Length == 0)
				{
					return false;
				}

				// check if item already exists.
				if (!m_items.Contains(itemID))
				{
					return false;
				}

				// remove item.
				m_items.Remove(itemID);

				return true;
			}
		}

		/// <summary>
		/// Adds an link and an item with the same id to the address space.
		/// </summary>
		public bool AddItemAndLink(string browsePath, IDevice device)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// check if item does not exists.
				if (m_items.Contains(browsePath))
				{
					return false;
				}

				// create the browse element.
				BrowseElement element = m_addressSpace.Insert(browsePath);

				if (element == null)
				{
					return false;
				}

				// create new item and index by item id.
				m_items[element.ItemID] = new CacheItem(browsePath, device);
				return true;
			}
		}

		/// <summary>
		/// Adds an link with an item id to the address space.
		/// </summary>
		public bool AddItemAndLink(string browsePath, string itemID, IDevice device)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// check if item does not exists.
				if (!m_items.Contains(itemID))
				{
					return false;
				}

				// create the browse element.
				BrowseElement element = m_addressSpace.Insert(browsePath, itemID);

				if (element == null)
				{
					return false;
				}

				// create new item and index by item id.
				m_items[element.ItemID] = new CacheItem(itemID, device);
				return true;
			}
		}
    
		/// <summary>
		/// Removes an link and an item with the same id to the address space.
		/// </summary>
		public bool RemoveItemAndLink(string browsePath)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// find the browse element.
				BrowseElement element = m_addressSpace.Find(browsePath);

				if (element != null)
				{
					// remove item.
					m_items.Remove(element.ItemID);

					// remove element.
					element.Remove();
				}

				return true;
			}
		}
    
		/// <summary>
		/// Adds an link without an item to the address space.
		/// </summary>
		public bool AddLink(string browsePath)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// create the browse element.
				BrowseElement element = m_addressSpace.Insert(browsePath);

				if (element == null)
				{
					return false;
				}

				return true;
			}
		}
    
		/// <summary>
		/// Removes an link (but not the item) from the address space.
		/// </summary>
		public bool RemoveLink(string browsePath)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// create the browse element.
				BrowseElement element = m_addressSpace.Find(browsePath);

				if (element != null)
				{
					element.Remove();
					return true;
				}

				return false;
			}
		}
    
		/// <summary>
		/// Removes an link (but not the item) from the address space only if it has no children.
		/// </summary>
		public bool RemoveEmptyLink(string browsePath)
		{
			lock (this)
			{
				// validate browse path.
				if (browsePath == null || browsePath.Length == 0)
				{
					return false;
				}

				// create the browse element.
				BrowseElement element = m_addressSpace.Find(browsePath);

				if (element != null)
				{
					if (element.Count == 0)
					{
						element.Remove();
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Checks whether the item id is valid.
		/// </summary>
		public bool IsValidItem(string itemID)
		{
			lock (this)
			{
				return (LookupItem(itemID) != null);
			}
		}

		/// <summary>
		/// Reads the value from the cache.
		/// </summary>
		public ItemValueResult Read(string itemID, string locale, System.Type reqType, int maxAge)
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				if (itemID == null || itemID.Length == 0)
				{
					return new ItemValueResult(itemID, ResultID.Da.E_INVALID_ITEM_NAME);
				}

				CacheItem item = LookupItem(itemID);
				
				if (item == null)
				{
					return new ItemValueResult(itemID, ResultID.Da.E_UNKNOWN_ITEM_NAME);
				}

				return item.Read(locale, reqType, maxAge, m_supportsCOM);
			}
		}

		/// <summary>
		/// Reads the value of the specified property.
		/// </summary>
		public object ReadProperty(string itemID, PropertyID propertyID)
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				CacheItem item = LookupItem(itemID);
				
				if (item == null)
				{
					return null;
				}

				return item.ReadProperty(propertyID);
			}
		}

		/// <summary>
		/// Writes a value to the device.
		/// </summary>
		public IdentifiedResult Write(string itemID, string locale, ItemValue value)
		{
			lock (this)
			{
				if (m_disposed) throw new ObjectDisposedException("Opc.Da.Cache");

				if (itemID == null || itemID.Length == 0)
				{
					return new IdentifiedResult(itemID, ResultID.Da.E_INVALID_ITEM_NAME);
				}

				CacheItem item = LookupItem(itemID);
				
				if (item == null)
				{
					return new IdentifiedResult(itemID, ResultID.Da.E_UNKNOWN_ITEM_NAME);
				}

				return item.Write(locale, value, m_supportsCOM);
			}
		}

		/// <summary>
		/// Adds a subscription to the list of active subscriptions.
		/// </summary>
		public void ActivateSubscription(Subscription subscription)
		{
			lock (this)
			{
				// ensure duplicates are not added.
				for (int ii = 0; ii < m_subscriptions.Count; ii++)
				{
					if (subscription == m_subscriptions[ii])
					{
						return;
					}
				}

				m_subscriptions.Add(subscription);
			}
		}

		/// <summary>
		/// Removes a subscription to the list of active subscriptions.
		/// </summary>
		public void DeactivateSubscription(Subscription subscription)
		{
			lock (this)
			{
				for (int ii = 0; ii < m_subscriptions.Count; ii++)
				{
					if (subscription == m_subscriptions[ii])
					{
						m_subscriptions.RemoveAt(ii);
						return;
					}
				}
			}
		}
		#endregion

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

		#region Private Members
		private int m_refs = 0;
		private bool m_disposed = false;
		private ResourceManager m_resourceManager = null;
		private Opc.Da.ServerStatus m_status = null;
		private Hashtable m_items = new Hashtable();
		private BrowseElement m_addressSpace = new BrowseElement(null, null);
		private ArrayList m_subscriptions = new ArrayList();
		private bool m_supportsCOM = false;

		/// <summary>
		/// Splits an item identfier into its component parts.
		/// </summary>
		private bool ParseItemID(string itemID, out string baseItemID, out PropertyID propertyID)
		{
			// set default values.
			baseItemID = itemID;
			propertyID = Property.VALUE;

			// validate item id.
			if (itemID == null || itemID.Length == 0)
			{
				return false;
			}

			// check for a property id qualifier.
			int index = itemID.LastIndexOf(":");

			if (index == -1)
			{
				return true;
			}

			// extract base item id.
			baseItemID = itemID.Substring(0, index);

			// parse property id.
			try
			{
				int code = System.Convert.ToInt32(itemID.Substring(index+1));
				propertyID = new PropertyID(code);
			}
			catch
			{
				return false;
			}

			// item syntax is valid.
			return true;
		}

		/// <summary>
		/// Fetches the specified properties for an item.
		/// </summary>
		private Opc.Da.ItemProperty[] GetProperties(
			CacheItem    item,
			PropertyID[] propertyIDs,
			bool         returnValues)
		{
			// check for trivial case.
			if (item == null)
			{
				return null;
			}

			// fetch properties.
			ItemPropertyCollection properties = null;
			
			if (propertyIDs == null)
			{
				properties = item.GetAvailableProperties(returnValues);
			}
			else
			{
				properties = item.GetAvailableProperties(propertyIDs, returnValues);
			}

			// convert collection to array.
			return (Opc.Da.ItemProperty[])properties.ToArray(typeof(Opc.Da.ItemProperty));
		}

		/// <summary>
		/// Looks up the cache item for the specified item id.
		/// </summary>
		private CacheItem LookupItem(string itemID)
		{
			int index = itemID.LastIndexOf(":");

			if (index == -1)
			{
				return (CacheItem)m_items[itemID];
			}

			return (CacheItem)m_items[itemID.Substring(0, index)];
		}

		/// <summary>
		/// Extracts the property id for the specified item id.
		/// </summary>
		private string LookupProperty(string itemID, out PropertyID propertyID)
		{
			propertyID = Property.VALUE;

			// look for property delimiter.
			int index = itemID.LastIndexOf(":");

			if (index == -1)
			{
				return itemID;
			}

			// lookup property id.
			try
			{
				propertyID = new PropertyID(System.Convert.ToInt32(itemID.Substring(index+1)));
			}
			catch
			{
				return null;
			}

			// return root item id.
			return itemID.Substring(0, index);
		}

		/// <summary>
		/// Periodically updates the cache for all active subscriptions.
		/// </summary>
		private void OnUpdate(object stateInfo) 
		{
			int  delay = 0;
			long ticks = 0;

			do
			{
				// sleep until next update.
				Thread.Sleep((delay > 0 && delay < MAX_UPDATE_RATE)?MAX_UPDATE_RATE - delay:MAX_UPDATE_RATE);

				delay = Environment.TickCount;

				// build list of subscriptions to update.
				ArrayList subscriptions = new ArrayList();

				lock (this)
				{					
					// check if object has been disposed.
					if (m_disposed)
					{
						break;
					}

					foreach (Subscription subscription in m_subscriptions)
					{
						subscriptions.Add(subscription);
					}
				}
				
				// update each subscription
				foreach (Subscription subscription in subscriptions)
				{
					try
					{
						subscription.Update(ticks, MAX_UPDATE_RATE);
					}
					catch (Exception e)
					{
						string message = e.Message;
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
