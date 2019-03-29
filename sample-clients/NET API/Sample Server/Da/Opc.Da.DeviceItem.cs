//============================================================================
// TITLE: DeviceItem.cs
//
// CONTENTS:
// 
// A class representing a single data point in a simulated device. 
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
using System.Text;
using Opc;
using Opc.Da;

namespace Opc.Da.SampleServer
{
	/// <summary>
	/// A class representing a single data point in a simulated device. 
	/// </summary>
	public class DeviceItem
	{
		/// <summary>
		/// The unique item identifier.
		/// </summary>
		public string ItemID
		{
			get { return m_itemID; }
		}

		/// <summary>
		/// Initializes the object with the specified item id.
		/// </summary>
		public DeviceItem(string itemID)
		{
			m_itemID = itemID;
		}

		/// <summary>
		/// Initializes the object from a data value.
		/// </summary>
		public DeviceItem(string itemID, object value)
		{
			m_itemID    = itemID;

			if (value != null)
			{
				m_datatype  = value.GetType();
				m_value     = value;
				m_timestamp = DateTime.Now;
			}
		}

		/// <summary>
		/// Initializes the object from the XML stream.
		/// </summary>
		public bool Initialize(XmlTextReader reader)
		{
			// move to first child element.
			reader.ReadStartElement(ITEM);

			// read the data type.
			m_datatype = null;

			if (reader.IsStartElement())
			{
				if (reader.MoveToAttribute(TYPE, Opc.Namespace.XML_SCHEMA_INSTANCE))
				{
					m_datatype = ReadType(reader, reader.Value);
				}
			}

			// read the value.
			m_value     = ReadValue(reader, m_datatype);
			m_timestamp = DateTime.Now;

			// read properties.
			while (reader.IsStartElement())
			{
				ReadProperty(reader);
			}
			
			// read the end of the item.
			reader.ReadEndElement();

			return true;
		}

		/// <summary>
		/// Returns all available properties for the specified item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(bool returnValues)
		{
			ArrayList ids = new ArrayList();

			// add standard properties.
			ids.Add(Property.DATATYPE);
			ids.Add(Property.VALUE);
			ids.Add(Property.QUALITY);
			ids.Add(Property.TIMESTAMP);
			ids.Add(Property.ACCESSRIGHTS);
			ids.Add(Property.SCANRATE);
			ids.Add(Property.EUTYPE);
			ids.Add(Property.EUINFO);

			// add eu limits for analog items.
			if (m_euType == euType.analog)
			{
				ids.Add(Property.HIGHEU);
				ids.Add(Property.LOWEU);
			}

			// add limits for date time and decimal items.
			if (m_datatype == typeof(DateTime) || m_datatype == typeof(Decimal))
			{
				ids.Add(Property.MINIMUM_VALUE);
				ids.Add(Property.MAXIMUM_VALUE);
				ids.Add(Property.VALUE_PRECISION);
			}

			// add any additional properties.
			foreach (PropertyID id in m_properties.Keys)
			{
				ids.Add(id);
			}

			// fill in the property item ids and values.
			return GetAvailableProperties((PropertyID[])ids.ToArray(typeof(PropertyID)), returnValues);
		}

		/// <summary>
		/// Returns the specified properties for the specified item.
		/// </summary>
		public Opc.Da.ItemPropertyCollection GetAvailableProperties(
			PropertyID[] propertyIDs, 
			bool         returnValues)
		{
			// initialize property collection.
			ItemPropertyCollection properties = new ItemPropertyCollection();

			properties.ItemName       = m_itemID;
			properties.ItemPath       = null;
			properties.ResultID       = ResultID.S_OK;
			properties.DiagnosticInfo = null;

			// fetch information for each requested property.
			foreach (PropertyID propertyID in propertyIDs)
			{
				ItemProperty property = new ItemProperty();

				property.ID = propertyID;
                				
				// read the property value.
				if (returnValues)
				{
					ItemValueResult result = Read(propertyID);

					if (result.ResultID.Succeeded())
					{
						property.Value = result.Value;
					}

					property.ResultID       = result.ResultID;
					property.DiagnosticInfo = result.DiagnosticInfo;
				}

				// just validate the property id.
				else
				{
					property.ResultID = ValidatePropertyID(propertyID, accessRights.readWritable);
				}

				// set status if one or more errors occur.
				if (property.ResultID.Failed())
				{
					properties.ResultID = ResultID.S_FALSE;
				}

				else
				{
					// set property description.
					PropertyDescription description = PropertyDescription.Find(propertyID);

					if (description != null)
					{
						property.Description = description.Name;
						property.DataType    = description.Type;
					}

					// set property item id.
					if (propertyID.Code >= ENGINEERINGUINTS && propertyID.Code <= TIMEZONE)
					{
						property.ItemName = m_itemID + ":" + propertyID.Code.ToString();
						property.ItemPath = null;
					}
				}

				// add to collection.
				properties.Add(property);
			}

			// return collection.
			return properties;
		}

		/// <summary>
		/// Reads the value of the specified item property.
		/// </summary>
		public Opc.Da.ItemValueResult Read(PropertyID propertyID)
		{
			// initialize value and validate property.
			ItemValueResult value = new ItemValueResult();

			value.ItemName           = m_itemID;
			value.ItemPath           = null;
			value.ResultID           = ValidatePropertyID(propertyID, accessRights.readable);
			value.DiagnosticInfo     = null;
			value.Value              = null;
			value.Quality            = Quality.Bad;
			value.QualitySpecified   = false;
			value.Timestamp          = DateTime.MinValue;
			value.TimestampSpecified = false;

			if (value.ResultID.Failed())
			{
				return value;
			}

			// set default quality and timestamp (overridden when returning the item value).
			value.Quality            = Quality.Good;
			value.QualitySpecified   = true;
			value.Timestamp          = DateTime.UtcNow;
			value.TimestampSpecified = true;

			// read the property value.
			switch (propertyID.Code)
			{	
				case VALUE:
				{
					value.Value     = Opc.Convert.Clone(m_value);
					value.Quality   = m_quality;
					value.Timestamp = m_timestamp;
					break;
				}

				// standard properties.
				case DATATYPE:     { value.Value = m_datatype;     break; }
				case QUALITY:      { value.Value = m_quality;      break; }	
				case TIMESTAMP:    { value.Value = m_timestamp;    break; }
				case ACCESSRIGHTS: { value.Value = m_accessRights; break; }
				case SCANRATE:     { value.Value = m_scanRate;     break; }
				case EUTYPE:       { value.Value = m_euType;       break; }
				case EUINFO:       { value.Value = m_euInfo;       break; }
				case HIGHEU:       { value.Value = m_maxValue;     break; }
				case LOWEU:        { value.Value = m_minValue;     break; }
				
				case MINIMUM_VALUE:      
				{   
					if (m_datatype == typeof(DateTime))
					{
						value.Value = DateTime.MinValue;
						break;
					}

					if (m_datatype == typeof(Decimal))
					{
						value.Value = Decimal.MinValue;
						break;
					}

					value.Value = null;
					break;     
				}
				
				case MAXIMUM_VALUE:      
				{   
					if (m_datatype == typeof(DateTime))
					{
						value.Value = DateTime.MaxValue;
						break;
					}

					if (m_datatype == typeof(Decimal))
					{
						value.Value = Decimal.MaxValue;
						break;
					}

					value.Value = null;
					break;     
				}
				
				case VALUE_PRECISION:      
				{   
					if (m_datatype == typeof(DateTime))
					{
						value.Value = 1/(double)TimeSpan.TicksPerMillisecond;
						break;
					}

					if (m_datatype == typeof(Decimal))
					{
						value.Value = 28;
						break;
					}

					value.Value = null;
					break;     
				}

				// other defined properties.
				default:
				{
					if (!m_properties.Contains(propertyID))
					{
						value.ResultID = ResultID.Da.E_INVALID_PID;
						break;
					}

					value.Value = m_properties[propertyID];
					break;
				}
			}

			// read completed successfully.
			return value;
		}

		/// <summary>
		/// Writes the value of the specified item property.
		/// </summary>
		public Opc.IdentifiedResult Write(
			PropertyID       propertyID, 
			Opc.Da.ItemValue value)
		{
			// initialize result and validate property.
			IdentifiedResult result = new IdentifiedResult();

			result.ItemName       = m_itemID;
			result.ItemPath       = null;
			result.ResultID       = ValidatePropertyID(propertyID, accessRights.writable);
			result.DiagnosticInfo = null;

			// handle value writes.
			if (propertyID == Property.VALUE)
			{
				// copy value.
				m_value = Opc.Convert.Clone(value.Value);

				// update quality if specified.
				if (value.QualitySpecified)
				{
					m_quality = value.Quality;
				}

				// update timestamp if specified.
				if (value.TimestampSpecified)
				{
					m_timestamp = value.Timestamp;
				}

				// return results.
				return result;
			}

			// lookup property description.
			PropertyDescription property = PropertyDescription.Find(propertyID);

			if (property == null)
			{
				result.ResultID = ResultID.Da.E_INVALID_PID;
				return result;
			}

			// check datatype.
			if (!property.Type.IsInstanceOfType(value.Value))
			{
				result.ResultID = ResultID.Da.E_BADTYPE;
				return result;
			}

			// write non-value
			switch (propertyID.Code)
			{	
				// standard properties.
				case DATATYPE:      { m_datatype     = (System.Type)value.Value;                 return result; }
				case QUALITY:       { m_quality      = (Quality)value.Value;                     return result; }	
				case TIMESTAMP:     { m_timestamp    = (DateTime)value.Value;                    return result; }
				case ACCESSRIGHTS:  { m_accessRights = (accessRights)value.Value;                return result; }
				case SCANRATE:      { m_scanRate     = (float)value.Value;                       return result; }
				case EUTYPE:        { m_euType       = (euType)value.Value;                      return result; }
				case EUINFO:        { m_euInfo       = (string[])Opc.Convert.Clone(value.Value); return result; }
				case HIGHEU:        { m_maxValue     = (double)value.Value;                      return result; }
				case LOWEU:         { m_minValue     = (double)value.Value;                      return result; }
				
				// other defined properties.
				default:
				{
					if (!m_properties.Contains(propertyID))
					{
						result.ResultID = ResultID.Da.E_INVALID_PID;
						return result;
					}

					m_properties[propertyID] = Opc.Convert.Clone(value.Value);
					break;
				}
			}

			// write complete.
			return result;
		}

		/// <summary>
		/// Recursively re-calculates the value of an object.
		/// </summary>
		public void Update(long ticks, int interval)
		{
			// check if value does not change.
			if (m_period == 0)
			{
				return;
			}

			// check whether the sample rate has passed since the last change.
			long ticksPerPeriod = m_period/interval;
			long ticksPerSample = m_samplingRate/interval;

			if (ticksPerSample > 1 && ticks%ticksPerSample != 0) 
			{ 
				return;
			}

			// build simulation parameter lists.
			Parameters parameters;

			parameters.Ticks        = ticks;
			parameters.Interval     = interval;
			parameters.Period       = m_period;
			parameters.SamplingRate = m_samplingRate;
			parameters.Waveform     = m_waveform;
			parameters.MaxValue     = Double.MaxValue;
			parameters.MinValue     = Double.MinValue;

			switch (m_euType)
			{
				case euType.analog:
				{
					parameters.MaxValue = m_maxValue;
					parameters.MinValue = m_minValue;
					break;
				}

				case euType.enumerated:
				{
					parameters.Waveform = 3;
					parameters.MaxValue = m_euInfo.Length-1;
					parameters.MinValue = 0;
					break;
				}
			}

			m_value     = Update(m_value, parameters, ticks, interval);
			m_timestamp = DateTime.Now;
		}

		#region Private Members
		private const string ITEM          = "Item";
		private const string TYPE          = "type";
		private const string PROPERTY      = "Property";
		private const string PROPERTY_ID   = "PropertyID";
		private const string ARRAY_OF      = "ArrayOf";
		private const string PERIOD        = "Period";
		private const string SAMPLING_RATE = "SamplingRate";
		private const string WAVEFORM      = "Waveform";

		private const int DATATYPE         = 1;
		private const int VALUE            = 2;
		private const int QUALITY          = 3;
		private const int TIMESTAMP        = 4;
		private const int ACCESSRIGHTS     = 5;
		private const int SCANRATE         = 6;
		private const int EUTYPE           = 7;
		private const int EUINFO           = 8;
		private const int ENGINEERINGUINTS = 100;
		private const int HIGHEU           = 102;
		private const int LOWEU            = 103;
		private const int TIMEZONE         = 108;
		private const int MINIMUM_VALUE    = 109;
		private const int MAXIMUM_VALUE    = 110;
		private const int VALUE_PRECISION  = 111;

		private string m_itemID = null;
		private System.Type m_datatype = null;
		private object m_value = null;
		private Quality m_quality = Quality.Good;
		private DateTime m_timestamp = DateTime.MinValue;
		private accessRights m_accessRights = accessRights.readWritable;
		private float m_scanRate = 0;
		private euType m_euType = euType.noEnum;
		private string[] m_euInfo = null;
		private double m_maxValue = Double.MaxValue;
		private double m_minValue = Double.MinValue;
		private int m_period = 0;
		private int m_samplingRate = 0;
		private int m_waveform = 1;
		private Hashtable m_properties = new Hashtable();

		/// <summary>
		/// Reads a value from the XML stream.
		/// </summary>
		private object ReadValue(XmlTextReader reader, System.Type datatype)
		{
			// read the datatype attribute.
			if (datatype == null || datatype == typeof(object))
			{
				if (reader.MoveToAttribute(TYPE, Opc.Namespace.XML_SCHEMA_INSTANCE))
				{
					datatype = ReadType(reader, reader.Value);
				}
	
				// default datatype is object.
				else
				{
					datatype = (reader.IsStartElement())?typeof(object[]):typeof(object);
				}
			}

			// get the period.
			if (reader.MoveToAttribute(PERIOD))
			{
				m_period = (int)Opc.Convert.ChangeType(reader.Value, typeof(int));
			}

			// get the sampling rate.
			if (reader.MoveToAttribute(SAMPLING_RATE))
			{
				m_samplingRate = (int)Opc.Convert.ChangeType(reader.Value, typeof(int));
			}

			// get the waveform.
			if (reader.MoveToAttribute(WAVEFORM))
			{
				m_waveform = (int)Opc.Convert.ChangeType(reader.Value, typeof(int));
			}

			reader.ReadStartElement();

			// read a simple value.
			object value = null;

			if (!datatype.IsArray)
			{
				value = Opc.Convert.ChangeType(reader.ReadString(), datatype);
			}
			
				// read an array value.
			else
			{
				ArrayList values = new ArrayList();

				while (reader.IsStartElement())
				{
					values.Add(ReadValue(reader, datatype.GetElementType()));
				}

				value = values.ToArray(datatype.GetElementType());
			}
			
			// read end element.
			reader.ReadEndElement();

			// return value.
			return value;
		}

		/// <summary>
		/// Reads an item property from an XML stream.
		/// </summary>
		private void ReadProperty(XmlTextReader reader)
		{
			// look up the property id.
			PropertyDescription property = null;

			if (reader.MoveToAttribute(PROPERTY_ID))
			{
				try
				{
					property = PropertyDescription.Find(new PropertyID(((int)System.Convert.ToInt32(reader.Value))));
				}
				catch
				{
					property = null;
				}
			}

			// ignore invalid properties.
			if (property == null)
			{
				return;
			}

			// read value.
			object value = ReadValue(reader, property.Type);

			switch (property.ID.Code)
			{
				case QUALITY:      { m_quality      = (Quality)value;         break; } 
				case TIMESTAMP:    { m_timestamp    = (DateTime)value;        break; }  
				case ACCESSRIGHTS: { m_accessRights = (accessRights)value;    break; }  
				case SCANRATE:     { m_scanRate     = (float)value;           break; }  
				case EUTYPE:       { m_euType       = (euType)((int)value+1); break; }  
				case EUINFO:       { m_euInfo       = (string[])value;        break; }  
				case HIGHEU:       { m_maxValue     = (double)value;          break; }  
				case LOWEU:        { m_minValue     = (double)value;          break; }  
				
				default:                                          
				{ 
					m_properties[property.ID] = value;  
					break;    
				}
			}
		}

		/// <summary>
		/// Reads the data type from an XML stream.
		/// </summary>
		private System.Type ReadType(XmlTextReader reader, string type)
		{
			bool   array         = false;
			string parsedType    = type;
			string typeNamespace = null;

			// extract namespace.
			int index = parsedType.IndexOf(":");

			if (index != -1)
			{
				typeNamespace = reader.LookupNamespace(parsedType.Substring(0, index));
				parsedType    = parsedType.Substring(index+1);
			}

			// remove array prefix.
			if (type.StartsWith(ARRAY_OF))
			{
				parsedType = type.Substring(ARRAY_OF.Length, 1).ToLower() + type.Substring(ARRAY_OF.Length+1);
				array      = true;
			}

			// check for standard types.
			if (parsedType == "byte")          return (array)?typeof(sbyte[]):typeof(sbyte);
			if (parsedType == "unsignedByte")  return (array)?typeof(byte[]):typeof(byte);
			if (parsedType == "short")         return (array)?typeof(short[]):typeof(short);
			if (parsedType == "unsignedShort") return (array)?typeof(ushort[]):typeof(ushort);
			if (parsedType == "int")           return (array)?typeof(int[]):typeof(int);
			if (parsedType == "unsignedInt")   return (array)?typeof(uint[]):typeof(uint);
			if (parsedType == "long")          return (array)?typeof(long[]):typeof(long);
			if (parsedType == "unsignedLong")  return (array)?typeof(ulong[]):typeof(ulong);
			if (parsedType == "float")         return (array)?typeof(float[]):typeof(float);
			if (parsedType == "double")        return (array)?typeof(double[]):typeof(double);
			if (parsedType == "decimal")       return (array)?typeof(decimal[]):typeof(decimal);
			if (parsedType == "string")        return (array)?typeof(string[]):typeof(string);
			if (parsedType == "boolean")       return (array)?typeof(bool[]):typeof(bool);
			if (parsedType == "dateTime")      return (array)?typeof(DateTime[]):typeof(DateTime);
			if (parsedType == "anyType")       return (array)?typeof(object[]):typeof(object);
			if (parsedType == "hexBinary")     return (array)?typeof(string[]):typeof(string);

			// unsupported xml schema defined data type.
			if (typeNamespace == Opc.Namespace.XML_SCHEMA)
			{
				return null;
			}

			// must be complex type.
			return typeof(object[]);
		}

		/// <summary>
		/// Checks if the specified property is valid for the specifed access type.
		/// </summary>
		private ResultID ValidatePropertyID(PropertyID propertyID, accessRights accessRequired)
		{
			switch (propertyID.Code)
			{
				// check access rights for value properties.
				case VALUE:
				case QUALITY:
				case TIMESTAMP:
				{
					if (m_accessRights != accessRights.readWritable && m_accessRights != accessRequired)
					{
						switch (accessRequired)
						{
							case accessRights.readable: { return ResultID.Da.E_WRITEONLY; }
							case accessRights.writable: { return ResultID.Da.E_READONLY;  }
						}
					}

					break;
				}

				// no checks required for intrinsic properties.
				case DATATYPE: 
				case ACCESSRIGHTS: 
				case SCANRATE:  
				case EUTYPE: 
				case EUINFO:
				{
					break;
				}

				// eu limits only valid for analog items.
				case HIGHEU:
				case LOWEU:
				{
					if (m_euType != euType.analog)
					{
						return ResultID.Da.E_INVALID_PID;
					}

					break;
				}

				// data type limits properties are always read only.
				case MINIMUM_VALUE: 
				case MAXIMUM_VALUE: 
				case VALUE_PRECISION:  
				{
					if (accessRequired == accessRights.writable)
					{ 
						return ResultID.Da.E_READONLY;
					}

					break;
				}

				// lookup any addition property.
				default:
				{
					if (!m_properties.Contains(propertyID))
					{
						return ResultID.Da.E_INVALID_PID;
					}

					break;
				}
			}

			// property is valid.
			return ResultID.S_OK;
		}

		/// <summary>
		/// Contains parameters used in the similation.
		/// </summary>
		struct Parameters
		{
			public long   Ticks;
			public int    Interval;
			public int    Period;
			public int    SamplingRate;
			public int    Waveform;
			public double MaxValue;
			public double MinValue;
		};

		/// <summary>
		/// Recursively re-calculates the value of an object.
		/// </summary>
		private object Update(object value, Parameters parameters, long ticks, int interval)
		{
			// handle array types.
			if (value != null && value.GetType().IsArray)
			{
				ArrayList elements = new ArrayList(((Array)value).Length);

				foreach (object element in (Array)value)
				{
					elements.Add(Update(element, parameters, ticks, interval));
				}

				return elements.ToArray(value.GetType().GetElementType());
			}

			// handle scalar types.
			if (typeof(sbyte).IsInstanceOfType(value))    return Calculate((sbyte)value, parameters);
			if (typeof(byte).IsInstanceOfType(value))     return Calculate((byte)value, parameters);
			if (typeof(short).IsInstanceOfType(value))    return Calculate((short)value, parameters);
			if (typeof(ushort).IsInstanceOfType(value))   return Calculate((ushort)value, parameters);
			if (typeof(int).IsInstanceOfType(value))      return Calculate((int)value, parameters);
			if (typeof(uint).IsInstanceOfType(value))     return Calculate((uint)value, parameters);
			if (typeof(long).IsInstanceOfType(value))     return Calculate((long)value, parameters);
			if (typeof(ulong).IsInstanceOfType(value))    return Calculate((ulong)value, parameters);
			if (typeof(float).IsInstanceOfType(value))    return Calculate((float)value, parameters);
			if (typeof(double).IsInstanceOfType(value))   return Calculate((double)value, parameters);
			if (typeof(decimal).IsInstanceOfType(value))  return Calculate((decimal)value, parameters);
			if (typeof(bool).IsInstanceOfType(value))     return Calculate((bool)value, parameters);
			if (typeof(string).IsInstanceOfType(value))   return Calculate((string)value, parameters);
			if (typeof(DateTime).IsInstanceOfType(value)) return Calculate((DateTime)value, parameters);

			// no calculate for unknown type.
			return value;
		}

		private sbyte Calculate(sbyte value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)SByte.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)SByte.MinValue);

			return (sbyte)Calculate((decimal)value, parameters);
		}

		private byte Calculate(byte value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)Byte.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)Byte.MinValue);

			return (byte)Calculate((decimal)value, parameters);
		}

		private short Calculate(short value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)Int16.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)Int16.MinValue);

			return (short)Calculate((decimal)value, parameters);
		}

		private ushort Calculate(ushort value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)UInt16.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)UInt16.MinValue);

			return (ushort)Calculate((decimal)value, parameters);
		}

		private int Calculate(int value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)Int32.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)Int32.MinValue);

			return (int)Calculate((decimal)value, parameters);
		}

		private uint Calculate(uint value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)UInt32.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)UInt32.MinValue);

			return (uint)Calculate((decimal)value, parameters);
		}

		private long Calculate(long value, Parameters parameters)
		{				
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)Int64.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)Int64.MinValue);

			return (long)Calculate((decimal)value, parameters);
		}

		private ulong Calculate(ulong value, Parameters parameters)
		{				
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)UInt64.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)UInt64.MinValue);

			return (ulong)Calculate((decimal)value, parameters);
		}
	
		private float Calculate(float value, Parameters parameters)
		{	
			parameters.MaxValue = Math.Min(parameters.MaxValue, (double)Single.MaxValue);
			parameters.MinValue = Math.Max(parameters.MinValue, (double)Single.MinValue);

			return (float)Calculate((double)value, parameters);
		}

		private double Calculate(double value, Parameters parameters)
		{	
			// determine upper and lower bounds.
			double max = (parameters.MaxValue == Double.MaxValue)?Int64.MaxValue:(double)parameters.MaxValue;
			double min = (parameters.MinValue == Double.MinValue)?Int64.MinValue:(double)parameters.MinValue;

			double range = max - min;
			double delta = range*(((double)parameters.SamplingRate)/((double)parameters.Period));

			switch (parameters.Waveform)
			{
				case 1:
				{
					if (max <= value + delta)
					{
						return min;
					}

					return value + delta;
				}

				case 2:
				{
					double fraction = ((double)((parameters.Ticks*parameters.Interval)%parameters.Period))/parameters.Period;
		
					return min + (double)(range*(Math.Sin(2*Math.PI*fraction)+1.0)/2.0);
				}

				case 3:
				{
					if (value == (double)parameters.MaxValue)
					{
						return 0;
					}

					return value + 1;
				}
			}

			// value does not change.
			return value;
		}

		private decimal Calculate(decimal value, Parameters parameters)
		{	
			// determine upper and lower bounds.
			decimal max = (parameters.MaxValue == Double.MaxValue)?Decimal.MaxValue:(decimal)parameters.MaxValue;
			decimal min = (parameters.MinValue == Double.MinValue)?Decimal.MinValue:(decimal)parameters.MinValue;

			decimal range = 0;
			
			try   { range = max - min;          }
			catch { range = Decimal.MaxValue/2; }

			decimal delta = range*(((decimal)parameters.SamplingRate)/((decimal)parameters.Period));

			switch (parameters.Waveform)
			{
				case 1:
				{
					if (max <= value + delta)
					{
						return min;
					}

					return value + delta;
				}

				case 2:
				{
					double fraction = ((double)((parameters.Ticks*parameters.Interval)%parameters.Period))/parameters.Period;

					return min + (decimal)(range*((decimal)((Math.Sin(2*Math.PI*fraction)+1.0)/2.0)));
				}

				case 3:
				{
					if (value == (decimal)parameters.MaxValue)
					{
						return 0;
					}

					return value + 1;
				}
			}

			// value does not change.
			return value;
		}
	
		private DateTime Calculate(DateTime value, Parameters parameters)
		{	
			return new DateTime(Calculate(value.Ticks, parameters));
		}

		private bool Calculate(bool value, Parameters parameters)
		{	
			return !value;
		}

		private string Calculate(string value, Parameters parameters)
		{
			double fraction = ((double)((parameters.Ticks*parameters.Interval)%parameters.Period))/parameters.Period;

			int seed = (int)(((double)Int16.MaxValue)*fraction);

			if (value != null && seed == 0)
			{
				foreach (char ch in value)
				{
					seed += (int)Char.GetNumericValue(ch);
				}
			}

			Random random = new Random(seed);

			int length = (int)(((random.NextDouble()+1)*80)/2);

			StringBuilder buffer = new StringBuilder(length);

			for (int ii = 0; ii < length; ii++)
			{
				buffer.Append(System.Convert.ToChar((byte)(((random.NextDouble()+1)*94)/2)+32));
			}

			return buffer.ToString();
		}
		#endregion
	}
}
