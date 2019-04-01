//============================================================================
// TITLE: IDevice.cs
//
// CONTENTS:
// 
// An interface for an abstract device that contains a set of data points.
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
using System.IO;
using Opc;
using Opc.Da;

namespace Opc.Da.SampleServer
{
	/// <summary>
	/// An interface for an abstract device that contains a set of data points.
	/// </summary>
	public interface IDevice
	{
		/// <summary>
		/// Initializes the device.
		/// </summary>
		bool Initialize(Cache cache);

		/// <summary>
		/// Adds the address space for the device to the cache.
		/// </summary>
		bool BuildAddressSpace(Cache cache, string browsePath);

		/// <summary>
		/// Removes the address space for the device to the cache.
		/// </summary>
		void ClearAddressSpace(Cache cache);

		/// <summary>
		/// Returns whether item id belongs to the device's address space.
		/// </summary>
		bool IsKnownItem(string itemID);

		/// <summary>
		/// Returns all available properties for the specified item.
		/// </summary>
		Opc.Da.ItemPropertyCollection GetAvailableProperties(
			string itemID, 
			bool   returnValues);

		/// <summary>
		/// Returns the specified properties for the specified item.
		/// </summary>
		Opc.Da.ItemPropertyCollection GetAvailableProperties(
			string       itemID, 
			PropertyID[] propertyIDs, 
			bool         returnValues);

		/// <summary>
		/// Reads the value of the specified item property.
		/// </summary>
		Opc.Da.ItemValueResult Read(string itemID, PropertyID propertyID);

		/// <summary>
		/// Writes the value of the specified item property.
		/// </summary>
		Opc.IdentifiedResult Write(
			string           itemID, 
			PropertyID       propertyID, 
			Opc.Da.ItemValue value);
	}
}
