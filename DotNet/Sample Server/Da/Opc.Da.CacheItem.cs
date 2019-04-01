//============================================================================
// TITLE: CacheItem.cs
//
// CONTENTS:
// 
// A class which maintains a cache for a single item.
//
// (c) Copyright 2004 The OPC Foundation
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
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using Opc;
using Opc.Da;

namespace Opc.Da.SampleServer
{
	/// <summary>
	/// A class which maintains a cache for a single item.
	/// </summary>
	public class CacheItem
	{
		/// <summary>
		/// Initializes the object with its item id and device.
		/// </summary>
		public CacheItem(string itemID, IDevice device)
		{
			if (itemID == null) throw new ArgumentNullException("itemID");
			if (device == null) throw new ArgumentNullException("device");

			m_itemID = itemID;
			m_device = device;

			m_datatype = (System.Type)ReadProperty(Property.DATATYPE);
			m_euType   = (euType)ReadProperty(Property.EUTYPE);

			if (m_euType == euType.enumerated)
			{
				m_euInfo = (string[])ReadProperty(Property.EUINFO);
			}
		}

		/// <summary>
		/// Returns all available properties for the item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(bool returnValues)
		{
			return m_device.GetAvailableProperties(m_itemID, returnValues);
		}

		/// <summary>
		/// Returns the specified properties for the item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(
			PropertyID[] propertyIDs, 
			bool         returnValues)
		{
			return m_device.GetAvailableProperties(m_itemID, propertyIDs, returnValues);
		}

		/// <summary>
		/// Reads the value of the specified item property.
		/// </summary>
		public Opc.Da.ItemValueResult Read(PropertyID propertyID)
		{
			return m_device.Read(m_itemID, propertyID);
		}


		/// <summary>
		/// Converts a value to the specified type using the specified locale.
		/// </summary>
		private object ChangeType(object source, System.Type type, string locale, bool supportsCOM)
		{
			CultureInfo culture = Thread.CurrentThread.CurrentCulture;

			// override the current thread culture to ensure conversions happen correctly.
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
			}
			catch
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			}

			try
			{
				return ChangeType(source, type, supportsCOM);
			}

			// restore the current thread culture after conversion.
			finally
			{
				Thread.CurrentThread.CurrentCulture = culture;
			}
		}

		/// <summary>
		/// Converts a value to the specified type using either .NET or COM conversion rules.
		/// </summary>
		private object ChangeType(object source, System.Type type, bool supportsCOM)
		{
			if (source == null || type == null)
			{
				return source;
			}

			// check for an invalid req type.
			if (type == Type.ILLEGAL_TYPE)
			{
				throw new ResultIDException(ResultID.Da.E_BADTYPE);
			}

			// check for no conversion.
			if (type.IsInstanceOfType(source))
			{
				return Opc.Convert.Clone(source);
			}

			// convert the data.
			if (supportsCOM)
			{
				return ChangeTypeForCOM(source, type);
			}
			else
			{
				return ChangeType(source, type);
			}
		}

		/// <summary>
		/// Converts a value to the specified type using .NET conversion rules.
		/// </summary>
		private object ChangeType(object source, System.Type type)
		{
			try
			{
				// check for array conversion.
				if (source.GetType().IsArray && type.IsArray)
				{
					ArrayList array = new ArrayList();

					foreach (object element in (Array)source)
					{
						try
						{
							array.Add(ChangeType(element, type.GetElementType()));
						}
						catch
						{
							throw new ResultIDException(ResultID.Da.E_BADTYPE);
						}
					}

					return array.ToArray(type.GetElementType());
				}
				else if (!source.GetType().IsArray && !type.IsArray)
				{
					if (type == typeof(SByte))    { return System.Convert.ToSByte(source);    }
					if (type == typeof(Byte))     { return System.Convert.ToByte(source);     }
					if (type == typeof(Int16))    { return System.Convert.ToInt16(source);    } 
					if (type == typeof(UInt16))   { return System.Convert.ToUInt16(source);   }
					if (type == typeof(Int32))    { return System.Convert.ToInt32(source);    }
					if (type == typeof(UInt32))   { return System.Convert.ToUInt32(source);   }
					if (type == typeof(Int64))    { return System.Convert.ToInt64(source);    }
					if (type == typeof(UInt64))   { return System.Convert.ToUInt64(source);   }
					if (type == typeof(Int64))    { return System.Convert.ToInt64(source);    }
					if (type == typeof(Decimal))  { return System.Convert.ToDecimal(source);  }
					if (type == typeof(String))   { return System.Convert.ToString(source);   }
					
					if (type == typeof(Single))   
					{ 
						Single output = System.Convert.ToSingle(source);   

						if (Single.IsInfinity(output) || Single.IsNaN(output))
						{
							throw new ResultIDException(ResultID.Da.E_RANGE);
						}

						return output;
					}
					
					if (type == typeof(Double))  
					{ 
						Double output = System.Convert.ToDouble(source);   

						if (Double.IsInfinity(output) || Double.IsNaN(output))
						{
							throw new ResultIDException(ResultID.Da.E_RANGE);
						}

						return output; 
					}
					
					if (type == typeof(DateTime)) 
					{ 
						// check for conversions to date time from string.
						if (typeof(string).IsInstanceOfType(source))
						{
							string dateTime = ((string)source).Trim();

							// check for XML schema date/time format.
							if (dateTime.IndexOf('-') == 4)
							{
								try   
								{
									return System.Xml.XmlConvert.ToDateTime((string)source, XmlDateTimeSerializationMode.Utc); 
								}
								catch 
								{
									// ignore errors - try the localized version next.
								}
							}
						}

						// use default conversion. 
						return System.Convert.ToDateTime(source); 
					}
					
					if (type == typeof(Boolean))  
					{ 						
						// check for conversions to boolean from string.
						if (typeof(string).IsInstanceOfType(source))
						{
							string text = ((string)source).Trim().ToLower();

							// check for XML schema defined true values.
							if (text == "true" || text == "1")
							{
								return true;
							}

							// check for XML schema defined false values.
							if (text == "false" || text == "0")
							{
								return true;
							}
						}

						// use default conversion. 
						return System.Convert.ToBoolean(source);  
					}
				}
				else if (source.GetType().IsArray && type == typeof(string))
				{
					int count = ((Array)source).Length;

					StringBuilder buffer = new StringBuilder();

					buffer.Append("{");

					foreach (object element in (Array)source)
					{
						buffer.Append((string)ChangeType(element, typeof(string)));

						count--;

						if (count > 0)
						{
							buffer.Append(" | ");
						}
					}

					buffer.Append("}");

					return buffer.ToString();
				}

				// no conversions between scalar and array types allowed.
				throw new ResultIDException(ResultID.Da.E_BADTYPE);
			}
			catch (ResultIDException e)
			{
				throw e;
			}
			catch (Exception e)
			{
				throw new ResultIDException(ResultID.Da.E_BADTYPE, e.Message, e);
			}
		}
		
		/// <summary>
		/// Converts a value to the specified type using COM conversion rules.
		/// </summary>
		private object ChangeTypeForCOM(object source, System.Type type)
		{
			// check for conversions to date time from string.
			if (typeof(string).IsInstanceOfType(source) && type == typeof(DateTime))
			{
				try   { return System.Convert.ToDateTime((string)source); }
				catch {}
			}

			// check for conversions from date time to boolean.
			if (typeof(DateTime).IsInstanceOfType(source) && type == typeof(bool))
			{
				return !(new DateTime(1899, 12, 30, 0, 0, 0).Equals(source));
			}

			// check for conversions from float to double.
			if (typeof(float).IsInstanceOfType(source) && type == typeof(double))
			{
				return System.Convert.ToDouble((Single)source);
			}

			// check for array conversion.
			if (source.GetType().IsArray && type.IsArray)
			{
				ArrayList array = new ArrayList();

				foreach (object element in (Array)source)
				{
					try
					{
						array.Add(ChangeTypeForCOM(element, type.GetElementType()));
					}
					catch
					{
						throw new ResultIDException(new ResultID(DISP_E_OVERFLOW));
					}
				}

				return array.ToArray(type.GetElementType());
			}
			else if (!source.GetType().IsArray && !type.IsArray)
			{
				IntPtr pvargDest = Marshal.AllocCoTaskMem(16);
				IntPtr pvarSrc   = Marshal.AllocCoTaskMem(16);
				
				VariantInit(pvargDest);
				VariantInit(pvarSrc);

				Marshal.GetNativeVariantForObject(source, pvarSrc);

				try
				{
					// get vartype.
					short vt = (short)GetType(type);
	
					// change type.
					int error = VariantChangeTypeEx(
						pvargDest, 
						pvarSrc, 
						Thread.CurrentThread.CurrentCulture.LCID, 
						VARIANT_NOVALUEPROP | VARIANT_ALPHABOOL, 
						vt);

					// check error code.
					if (error != 0)
					{
						throw new ResultIDException(new ResultID(error));
					}

					// unmarshal result.
					object result = Marshal.GetObjectForNativeVariant(pvargDest);
					
					// check for invalid unsigned <=> signed conversions.
					switch ((VarEnum)vt)
					{
						case VarEnum.VT_I1:
						case VarEnum.VT_I2:
						case VarEnum.VT_I4:
						case VarEnum.VT_I8:
						case VarEnum.VT_UI1:
						case VarEnum.VT_UI2:
						case VarEnum.VT_UI4:
						case VarEnum.VT_UI8:
						{
							// ignore issue for conversions from boolean.
							if (typeof(bool).IsInstanceOfType(source))
							{	
								break;					
							}

							decimal sourceAsDecimal = 0;
							decimal resultAsDecimal = System.Convert.ToDecimal(result);

							try   { sourceAsDecimal = System.Convert.ToDecimal(source); }
							catch { sourceAsDecimal = 0; }
							
							if ((sourceAsDecimal < 0 && resultAsDecimal > 0) || (sourceAsDecimal > 0 && resultAsDecimal < 0))
							{
								throw new ResultIDException(ResultID.Da.E_RANGE);
							}

							break;
						}		
							
						case VarEnum.VT_R8:
						{						
							// fix precision problem introduced with conversion from float to double.
							if (typeof(float).IsInstanceOfType(source))
							{	
								result = System.Convert.ToDouble(source.ToString());
							}

							break;			
						}
					}

					return result;
				}
				finally
				{
					VariantClear(pvargDest);
					VariantClear(pvarSrc);

					Marshal.FreeCoTaskMem(pvargDest);
					Marshal.FreeCoTaskMem(pvarSrc);
				}
			}
			else if (source.GetType().IsArray && type == typeof(string))
			{
				int count = ((Array)source).Length;

				StringBuilder buffer = new StringBuilder();

				buffer.Append("{");

				foreach (object element in (Array)source)
				{
					buffer.Append((string)ChangeTypeForCOM(element, typeof(string)));

					count--;

					if (count > 0)
					{
						buffer.Append(" | ");
					}
				}

				buffer.Append("}");

				return buffer.ToString();
			}

			// no conversions between scalar and array types allowed.
			throw new ResultIDException(ResultID.Da.E_BADTYPE);
		}

		[DllImport("OleAut32.dll")]	
		private static extern int VariantChangeTypeEx( 
			IntPtr pvargDest,  
			IntPtr pvarSrc,  
			int    lcid,             
			ushort wFlags,  
			short  vt);

		[DllImport("oleaut32.dll")]
		private static extern void VariantInit(IntPtr pVariant);

		[DllImport("oleaut32.dll")]
		private static extern void VariantClear(IntPtr pVariant);

		private const int DISP_E_TYPEMISMATCH = -0x7FFDFFFB; // 0x80020005
		private const int DISP_E_OVERFLOW     = -0x7FFDFFF6; // 0x8002000A

		private const int VARIANT_NOVALUEPROP = 0x01;
		private const int VARIANT_ALPHABOOL   = 0x02; // For VT_BOOL to VT_BSTR conversions convert to "True"/"False" instead of

		/// <summary>
		/// Converts the system type to a VARTYPE.
		/// </summary>
		internal static VarEnum GetType(System.Type input)
		{				
			if (input == null)               return VarEnum.VT_EMPTY;
			if (input == typeof(sbyte))      return VarEnum.VT_I1;
			if (input == typeof(byte))       return VarEnum.VT_UI1;
			if (input == typeof(short))      return VarEnum.VT_I2;
			if (input == typeof(ushort))     return VarEnum.VT_UI2;
			if (input == typeof(int))        return VarEnum.VT_I4;
			if (input == typeof(uint))       return VarEnum.VT_UI4;
			if (input == typeof(long))       return VarEnum.VT_I8;
			if (input == typeof(ulong))      return VarEnum.VT_UI8;
			if (input == typeof(float))      return VarEnum.VT_R4;
			if (input == typeof(double))     return VarEnum.VT_R8;
			if (input == typeof(decimal))    return VarEnum.VT_CY;
			if (input == typeof(bool))       return VarEnum.VT_BOOL;
			if (input == typeof(DateTime))   return VarEnum.VT_DATE;
			if (input == typeof(string))     return VarEnum.VT_BSTR;
			if (input == typeof(object))     return VarEnum.VT_EMPTY;
			if (input == typeof(sbyte[]))    return VarEnum.VT_ARRAY | VarEnum.VT_I1;
			if (input == typeof(byte[]))     return VarEnum.VT_ARRAY | VarEnum.VT_UI1;
			if (input == typeof(short[]))    return VarEnum.VT_ARRAY | VarEnum.VT_I2;
			if (input == typeof(ushort[]))   return VarEnum.VT_ARRAY | VarEnum.VT_UI2;
			if (input == typeof(int[]))      return VarEnum.VT_ARRAY | VarEnum.VT_I4;
			if (input == typeof(uint[]))     return VarEnum.VT_ARRAY | VarEnum.VT_UI4;
			if (input == typeof(long[]))     return VarEnum.VT_ARRAY | VarEnum.VT_I8;
			if (input == typeof(ulong[]))    return VarEnum.VT_ARRAY | VarEnum.VT_UI8;
			if (input == typeof(float[]))    return VarEnum.VT_ARRAY | VarEnum.VT_R4;
			if (input == typeof(double[]))   return VarEnum.VT_ARRAY | VarEnum.VT_R8;
			if (input == typeof(decimal[]))  return VarEnum.VT_ARRAY | VarEnum.VT_CY;
			if (input == typeof(bool[]))     return VarEnum.VT_ARRAY | VarEnum.VT_BOOL;
			if (input == typeof(DateTime[])) return VarEnum.VT_ARRAY | VarEnum.VT_DATE;
			if (input == typeof(string[]))   return VarEnum.VT_ARRAY | VarEnum.VT_BSTR;
			if (input == typeof(object[]))   return VarEnum.VT_ARRAY | VarEnum.VT_VARIANT;
			
			return VarEnum.VT_EMPTY;
		}

		/// <summary>
		/// Reads the value from the cache and converts it to the rqeuested type.
		/// </summary>
		public ItemValueResult Read(string locale, System.Type reqType, int maxAge, bool supportsCOM)
		{
			// read value from device.
			DateTime target = DateTime.Now.AddMilliseconds((maxAge < 0)?0:-maxAge);

			if (maxAge == 0 || m_value == null || target > m_value.Timestamp)
			{
				m_value = m_device.Read(m_itemID, Property.VALUE);
				m_value.Timestamp = DateTime.Now;
			}

			ItemValueResult result = new ItemValueResult((ItemIdentifier)m_value);

			result.ResultID           = m_value.ResultID;
			result.DiagnosticInfo     = m_value.DiagnosticInfo;
			result.Quality            = m_value.Quality;
			result.QualitySpecified   = true;
			result.Timestamp          = m_value.Timestamp;
			result.TimestampSpecified = true;

			if (m_value.ResultID.Succeeded())
			{
				try
				{
					if (m_euType != euType.enumerated || reqType != typeof(string))
					{
						result.Value = ChangeType(m_value.Value, reqType, locale, supportsCOM); 
					}
					else
					{
						result.Value = m_euInfo[System.Convert.ToInt32(m_value.Value)];
					}
				}
				catch (OverflowException e)
				{
					result.ResultID = ResultID.Da.E_RANGE;
					result.DiagnosticInfo = e.Message;
				}
				catch (InvalidCastException e)
				{
					result.ResultID = ResultID.Da.E_RANGE;
					result.DiagnosticInfo = e.Message;
				}
				catch (ResultIDException e)
				{
					result.ResultID = e.Result;
					result.DiagnosticInfo = e.Message;
				}
				catch (Exception e)
				{
					result.ResultID = ResultID.Da.E_BADTYPE;
					result.DiagnosticInfo = e.Message;
				}
			}

			return result;
		}
		/// <summary>
		/// Writes a value to the device.
		/// </summary>
		public IdentifiedResult Write(string locale, ItemValue value, bool supportsCOM)
		{
			// check for invalid values.
			if (value == null || value.Value == null)
			{
				return new IdentifiedResult(m_itemID, ResultID.Da.E_BADTYPE);
			}

			ItemValue canonicalValue = new ItemValue();

			// convert non-enumerated type to canonical type.
			if (m_euType != euType.enumerated || !typeof(string).IsInstanceOfType(value.Value))
			{
				try
				{
					canonicalValue.Value = ChangeType(value.Value, m_datatype, locale, supportsCOM);
				}
				catch (OverflowException e)
				{
					return new IdentifiedResult(m_itemID, ResultID.Da.E_RANGE, e.Message);
				}
				catch (InvalidCastException e)
				{
					return new IdentifiedResult(m_itemID, ResultID.Da.E_RANGE, e.Message);
				}
				catch (ResultIDException e)
				{
					return new IdentifiedResult(m_itemID, e.Result, e.Message);
				}
				catch (Exception e)
				{
					return new IdentifiedResult(m_itemID, ResultID.Da.E_BADTYPE, e.Message);
				}
			}
			else
			{
				for (int ii = 0; ii < m_euInfo.Length; ii++)
				{
					if (m_euInfo[ii] == (string)value.Value)
					{
						canonicalValue.Value = ii;
						break;
					}
				}

				if (canonicalValue.Value == null)
				{
					return new IdentifiedResult(m_itemID, ResultID.Da.E_BADTYPE);
				}
			}

			canonicalValue.Quality            = value.Quality;
			canonicalValue.QualitySpecified   = value.QualitySpecified;
			canonicalValue.Timestamp          = value.Timestamp;
			canonicalValue.TimestampSpecified = value.TimestampSpecified;

			return m_device.Write(m_itemID, Property.VALUE, canonicalValue);		
		}
	

		/// <summary>
		/// Writes the value of the specified item property.
		/// </summary>
		public Opc.IdentifiedResult Write(
			string           itemID, 
			PropertyID       propertyID, 
			Opc.Da.ItemValue value)
		{
			return null;
		}

		/// <summary>
		/// Reads the value of the specified property.
		/// </summary>
		public object ReadProperty(PropertyID propertyID)
		{
			ItemValueResult result = m_device.Read(m_itemID, propertyID);

			if (result == null || result.ResultID.Failed())
			{
				return null;
			}

			return result.Value;
		}

		#region Private Members
		private string m_itemID = null;
		private IDevice m_device = null;
		private ItemValueResult m_value = null;
		private System.Type m_datatype = null;
		private euType m_euType = euType.noEnum;
		private string[] m_euInfo = null;
		#endregion
	}	
}
