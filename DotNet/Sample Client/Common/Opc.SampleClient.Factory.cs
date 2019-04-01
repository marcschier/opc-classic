//============================================================================
// TITLE: Opc.SampleClient.Factory.cs
//
// CONTENTS:
// 
// A interface and a class used to instantiate server objects.
//
// (c) Copyright 2003-2004 The OPC Foundation
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
// 2003/08/18 RSA   Initial implementation.

using System;
using System.Xml;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;

namespace Opc.SampleClient
{
	/// <summary>
	/// Defines utility functions used to instantiate servers.
	/// </summary>
	public class Factory
	{		
		/// <summary>
		/// Creates a server object for the specified URL.
		/// </summary>
		public static Opc.Server GetServerForURL(Opc.URL url)
		{
			if (url == null) throw new ArgumentNullException("url");

			Opc.Server server = null;

			// create an unconnected server object for XML based servers.
			if (url.Scheme == UrlScheme.HTTP)
			{
				server = new Opc.Da.Server(new OpcXml.Factory(), url);
			}

			// create an unconnected server object for COM based servers.
			else
			{
				// DA
				if (url.Scheme == UrlScheme.DA)  
				{ 
					server = new Opc.Da.Server(new OpcCom.Factory(), url); 
				}

				// AE
				else if (url.Scheme == UrlScheme.AE)  
				{ 
					server = new Opc.Ae.Server(new OpcCom.Factory(), url); 
				}

				// HDA
				else if (url.Scheme == UrlScheme.HDA) 
				{ 
					server = new Opc.Hda.Server(new OpcCom.Factory(), url); 
				}

				// DX
				else if (url.Scheme == UrlScheme.DX) 
				{ 
					server = new Opc.Dx.Server(new OpcCom.Factory(), url); 
				}

				#if (UA)
				// UA
				else if (url.Scheme == UrlScheme.UA_TCP || url.Scheme == UrlScheme.UA_HTTP)  
				{ 
					server = new Opc.Ua.Server(new OpcXml.Factory(), url); 
				}
				#endif

				// Other specifications not supported yet.
				else
				{
					throw new NotSupportedException(url.Scheme);
				}
			}

			return server;
		}
	}
}
