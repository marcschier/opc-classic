# OPC Historical Data Access 1.20 Errata Notes

Extracted from `External/Spec/historical-dataaccess-1.20-errata.zip` (`Historical DataAccess 1.20 Errata.docx`). The errata is table-heavy/free-form, so the converted text is retained here verbatim and referenced from the specification preamble instead of applying unreliable in-place replacements.

<!-- Errata notes: 7 sections retained (bounding/aggregate table corrections plus Errata 2.0 through 7.0). -->

---
![](data:image/x-wmf;base64...)

OPC Historical Data Access

Specification

Errata

Release 1.20

April 15, 2004

A document outlining the latest errata for the HDA 1.2 specification has been posted to the HDA yahoo group

<http://groups.yahoo.com/group/opc-hda/files/HDA%201.2%20Errata/OPC_HIST_Cust1.2_ERRATA.pdf>

**Modified sections of the table/specification in bold.**

2.8 Bounding values and Time Domain

**Start Time End Time dwNumValues Bounds Data Returned**

4:59 4.:59 0 Yes OPC\_S\_NODATA

4:59 4.:59 0 No OPC\_S\_NODATA

5:01 5:01 0 Yes OPC\_S\_NODATA

5:01 5:01 0 No OPC\_S\_NODATA

5:07 5:07 0 Yes OPC\_S\_NODATA

5:07 5:07 0 No OPC\_S\_NODATA

4:57 4.:59 0 Yes FIRST, 5:00

4:57 4.:59 0 No OPC\_S\_NODATA

5:01 5:02 0 Yes 5:00, 5:02

5:01 5:02 0 No OPC\_S\_NODATA

5:07 5:09 0 Yes 5:06, LAST

5:07 5:09 0 No OPC\_S\_NODATA

5:00 5:05 0 Yes 5:00, 5:02, 5:03, 5:05

5:00 5:05 0 No 5:00, 5:02, 5:03

5:01 5:04 0 Yes 5:00, 5:02, 5:03, 5:05

5:01 5:04 0 No 5:02, 5:03

5:05 5:00 0 Yes 5:05, 5:03, 5:02, 5:00

5:05 5:00 0 No 5:05, 5:03, 5:02

5:04 5:01 0 Yes 5:05, 5:03, 5:02, 5:00

5:04 5:01 0 No 5:03, 5:02

4:59 5:05 0 Yes FIRST, 5:00, 5:02, 5:03, 5:05

4:59 5:05 0 No 5:00, 5:02, 5:03

5:01 5:07 0 Yes 5:00, 5:02, 5:03, 5:05, 5:06, LAST

5:01 5:07 0 No 5:02, 5:03, 5:05, 5:06

5:00 5:05 3 Yes 5:00, 5:02, 5:03

5:00 5:05 3 No 5:00, 5:02, 5:03

5:01 5:04 3 Yes 5:00, 5:02, 5:03

5:01 5:04 3 No 5:02, 5:03

5:05 5:00 3 Yes 5:05, 5:03, 5:02

5:05 5:00 3 No 5:05, 5:03, 5:02

5:04 5:01 3 Yes 5:05, 5:03, 5:02

5:04 5:01 3 No 5:03, 5:02

4:59 5:05 3 Yes FIRST, 5:00, 5:02

4:59 5:05 3 No 5:00, 5:02, 5:03

5:01 5:07 3 Yes 5:00, 5:02, 5:03

5:01 5:07 3 No 5:02, 5:03, 5:05

5:00 NULL 3 Yes 5:00, 5:02, 5:03

5:00 NULL 3 No 5:00, 5:02, 5:03

5:00 NULL 6 Yes 5:00, 5:02, 5:03, 5:05, 5:06

5:00 NULL 6 No 5:00, 5:02, 5:03, 5:05, 5:06

NULL 5:06 3 Yes 5:06, 5:05, 5:03

NULL 5:06 3 No 5:06, 5:05, 5:03

NULL 5:06 6 Yes 5:06, 5:05, 5:03, 5:02, 5:00

NULL 5:06 6 No 5:06, 5:05, 5:03, 5:02, 5:00

**2.9.2.4 INTERPOLATIVE**

**Case 4.1 Requesting data with good bounding value.**

Start: Jan-01-2002 12:00:10 End: Jan-01-2002 12:00:20 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-02 12:00:10 10 Raw, Good 13.478 Interpolated, Good Value2 –Interpolated between values at 12:00:02 and 12:00:25

Jan-01-02 12:00:15 15 Interpolated, Good 15.652 Interpolated, Good Value2 –Interpolated between values at 12:00:02 and 12:00:25

**Case 4.2 Requesting data with good bounding value with bad data in the interval.**

Start: Jan-01-2002 12:00:35 End: Jan-01-2002 12:01:00 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-02 12:00:35 35 Interpolated, Uncertain 28.182 Interpolated, Good Value2 –Interpolated between values at 12:00:28 and 12:00:39

Jan-01-02 12:00:40 40 Interpolated, Uncertain 31.111 Interpolated, Uncertain Raw value is Bad, Value2 –Interpolated between values at 12:00:39 and 12:00:48

Jan-01-02 12:00:45 45 Interpolated, Uncertain 36.667 Interpolated, Uncertain Bounding value Bad, Value2 –Interpolated between values at 12:00:39 and 12:00:48

Jan-01-02 12:00:50 50 Raw, Good 45.000 Interpolated, Good

Jan-01-02 12:00:55 55 Interpolated, Good 51.500 Interpolated, Good

**Case 4.3 Requesting data with no good end bounding value.**

Start: Jan-01-2002 12:01:20 End: Jan-01-2002 12:01:40 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-02 12:01:20 80 Raw, Good 67.273\* Interpolated, Uncertain Uncertain values excluded.Value2 –Interpolated between values at 12:00:12 and 12:01:23

Jan-01-02 12:01:25 85 Interpolated, Good 76.667 Interpolated, Good

Jan-01-02 12:01:30 90 Raw, Good 90 Raw, Good

Jan-01-02 12:01:35 90 Interpolated, Uncertain 90 Interpolated, Uncertain Bounding value at 12:01:30, Extrapolated using stepped method

\* If Historian 2 had treated Uncertian values as Good. The value would be 70, interpolated between 12:00:17 and 12:00:23.

**Case 4.4 Requesting data with no good start bounding value.**

Start: Jan-01-2002 12:00:00 End: Jan-01-2002 12:00:20 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-2002 12:00:00 - No Data, Bad - No Data, Bad No bounding value, do not extrapolate

Jan-01-2002 12:00:05 - No Data, Bad 11.304 Interpolated, Good Value 1 - No bounding value, do not extrapolateValue2 –Interpolated between values at 12:00:02 and 12:00:25

Jan-01-2002 12:00:10 10 Raw, Good 13.478 Interpolated, Good Value2 –Interpolated between values at 12:00:02 and 12:00:25

Jan-01-2002 12:00:15 15 Interpolated, Good 15.652 Interpolated, Good Value2 –Interpolated between values at 12:00:02 and 12:00:25

**2.9.2.5 TIMEAVERAGE**

**Case 5.1 Requesting data with good bounding value.**

Start: Jan-01-2002 12:00:10 End: Jan-01-2002 12:00:20 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-2002 12:00:10 12.5 Calculated, Good 14.565 Calculated, Good Area under the line between 12:00:10 and 12:00:15 divided by interval length of 5

Jan-01-2002 12:00:15 17.5 Calculated, Good 16.739 Calculated, Good

**Case 5.2 Requesting data with good bounding value with bad data in the interval.**

Start: Jan-01-2002 12:00:35 End: Jan-01-2002 12:01:00 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-2002 12:00:35 37.5 Calculated, Uncertain 29.384 Calculated, Uncertain Value1– Interpolate values at :35 and :40 using bounds at :30 and :50Value2– Interpolate values at :35 and :40 using bounds at :28 and :48Uncertain means Bad value ignored

Jan-01-2002 12:00:40 42.5 Calculated, Uncertain 33.889 Calculated Uncertain Value1– Interpolate values at :40 and :45 using bounds at :30 and :50Value2– Interpolate values at :40 and :45 using bounds at :39 and :48Uncertain means Bad value ignored

Jan-01-2002 12:00:45 47.5 Calculated, Uncertain 40.000 Calculated Uncertain Value1– Interpolate value at :45 using bounds at :30 and :50Value2– Interpolate value at :45 using bounds at :39 and :48Interpolate value at :50 using bounds at :48 and :52Uncertain means Bad value ignored

Jan-01-2002 12:00:50 52.5 Calculated, Good 49.450 Calculated, Good Value1– Interpolate value at :55 using bounds at :50 and 01:00Value2– Interpolate value at :50 using bounds at :48 and :52Interpolate value at :55 using bounds at :52 and :01:12

Jan-01-2002 12:00:55 57.5 Calculated, Good 52.750 Calculated, Good Value1– Interpolate value at :55 using bounds at :50 and 01:00Value2– Interpolate value at :50 using bounds at :48 and :52Interpolate value at :55 using bounds at :52 and :01:12

**Case 5.3 Requesting data with no good end bounding value.**

Start: Jan-01-2002 12:01:20 End: Jan-01-2002 12:01:40 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-2002 12:01:20 82.5 Calculated, Good 70.515 Calculated Uncertain Value1– Interpolate value at :25 using bounds at :20 and :30Value2– Interpolate value at :20 using bounds at :12 and :23 (Uncertain value at :17 is ignored by this historian)Interpolate value at :25 using bounds at :23 and :26

Jan-01-2002 12:01:25 87.5 Calculated, Good 83.667 Calculated, Good Value1– Interpolate value at :25 using bounds at :20 and :30Value2– Interpolate value at :25 using bounds at :23 and :26

Jan-01-2002 12:01:30 90\* Calculated, Uncertain 90\* Calculated, Uncertain Extrapolate value at :35 using value at :30

Jan-01-2002 12:01:35 90\* Calculated, Uncertain 90\* Calculated, Uncertain Extrapolate values at :35 and :40 using value at :30

\* Stepped extrapolation is used at the boundary. Servers may opt to extrapolate data based on the previous slope.

**Case 5.4 Requesting data with no good start bounding value.**

Start: Jan-01-2002 12:00:00 End: Jan-01-2002 12:00:20 Interval: 00:00:05

Timestamp Historian 1 Historian 2 Notes

Value Quality Value Quality

Jan-01-2002 12:00:00 0 No Data, Bad 10.652 Partial, Uncertain Value1-No bounding value, do not extrapolate. No data in the intervalValue2- Interpolate value at :05 using bounds at :02 and :25Use partial interval :02 to :05, with interval of 3.

Jan-01-2002 12:00:05 0 No Data, Bad 12.391 Calculated, Good Value1-No bounding value, do not extrapolate. No data in the intervalValue2- Interpolate values at :05 and 10 using bounds at :02 and :25

Jan-01-2002 12:00:10 12.5 Calculated, Good 14.565 Calculated, Good Value

**2.9.2.7. AVERAGE**

**2.9.2.11. MINIMUM ACTUAL TIME**

**2.9.2.12. MINIMUM**

**2.9.2.13. MAXIMUM ACTUAL TIME**

**2.9.2.14. MAXIMUM**

Replaced all Bad, No Data values to be ‘-‘ instead of 0.

**2.9.2.15. START**

Case 15.2 Requesting data with good bounding value with bad data in the interval.

Start: Jan-01-2002 12:00:35 End: Jan-01-2002 12:01:00 Interval: 00:00:05

Timestamp Historian 1 Notes

Value Quality

Jan-01-2002 12:00:35 - No Data, Bad

Jan-01-2002 12:00:40 40 Raw, Bad Raw value (If Bad values are stored)

Jan-01-2002 12:00:45 - No Data, Bad

Jan-01-2002 12:00:50 50 Raw, Good

Jan-01-2002 12:00:55 - No Data, Bad

Start: Jan-01-2002 12:00:35 End: Jan-01-2002 12:01:00 Interval: 00:00:05

Timestamp Historian 2 Notes

Value Quality

Jan-01-2002 12:00:39 30 Raw, Good First raw in :35-:40 at :39

Jan-01-2002 12:00:42 40 Raw, Bad Raw value (If Bad values are stored)

Jan-01-2002 12:00:48 40 Raw, Good First raw in :45-:50 at :48

Jan-01-2002 12:00:52 50 Raw, Good First raw in :50-:55 at :52

Jan-01-2002 12:00:55 - No Data, Bad

Case 15.3 Partial Intervals.

Start: Jan-01-2002 12:00:05 End: Jan-01-2002 12:00:35 Interval: 00:00:16

Timestamp Historian 1 Notes

Value Quality

Jan-01-2002 12:00:10 10 Raw, Good First raw in :05-:21 at :10

Jan-01-2002 12:00:30 30 Partial, Good First raw in :21-:35 at :30

Start: Jan-01-2002 12:00:05 End: Jan-01-2002 12:00:35 Interval: 00:00:16

Timestamp Historian 2 Notes

Value Quality

Jan-01-2002 12:00:05 - No Data, Bad No raw data in :05-:21

Jan-01-2002 12:00:25 20 Raw, Good First raw in :21-:35 at :25

**2.9.2.16. END**

Case 16.3 Partial Intervals.

Start: Jan-01-2002 12:00:05 End: Jan-01-2002 12:00:35 Interval: 00:00:16

Timestamp Historian 1 Notes

Value Quality

Jan-01-2002 12:00:10 10 Raw, Good Last raw in :05-:21 at :10

Jan-01-2002 12:00:30 30 Partial, Good Last raw in :21-:35 at :30

Start: Jan-01-2002 12:00:05 End: Jan-01-2002 12:00:35 Interval: 00:00:16

Timestamp Historian 2 Notes

Value Quality

Jan-01-2002 12:00:21 - No Data, Bad No raw data in :05-:21

Jan-01-2002 12:00:28 25 Raw, Good Last raw in :21-:35 at :28

**Errata 2.0 Server should use the item status code of 'S\_NODATA' forRead(Advise)Processed(WithUpdate), ReadAtTime**

If the client specifies a range where no data exists for the ReadProcessedand ReadAtTime then the server returns an array of values with theOPCHDA\_NODATA quality set with an item status code of 'S\_OK'. This behavioris inconsistent with ReadRaw which returns no values and an item status codeof 'S\_NODATA'. The server should set the status code to 'S\_NODATA' forReadProcessed and ReadAtTime when the server knows that no data for the itemexists within the time. This status code would be in addition to returningall of item values with OPCHDA\_NODATA quality.

Portions of the specifications updated:

================================================

4.4.3.3. IOPCHDA\_SyncRead::ReadAtTime

ppError Codes

Return Code Description

S\_OK The item was read successfully.

OPC\_S\_NODATA No data was found in the specified time range.

OPC\_E\_BADRIGHTS Insufficient rights for this operation.

OPC\_E\_INVALIDHANDLE The handle is invalid.

OPC\_S\_NODATA No data was found for the item.

E\_FAIL The item read was unsuccessful.

Comments

The order of the values and qualities returned shall match the order of the time stamps supplied in the request.

When no value exists for a specified timestamp, a value shall be interpolated from the surrounding values to represent the value at the specified timestamp. The interpolation will follow the same rules as the standard Intpolated aggregate as outlined in Section 2.9 If the value can not be interpolated when no data exists for a given Item in any subinterval in the time domain, the server shall return OPC\_S\_NODATA in the ppErrors array for that Item.

==========================================================

4.5.1.3. IOPCHDA\_AsyncRead::ReadProcessed

ppError Codes

Return Code Description

S\_OK The item was read successfully.

OPC\_S\_NODATA No data was found in the specified time range.

OPC\_E\_BADRIGHTS Insufficient rights for this operation.

OPC\_E\_INVALIDHANDLE The handle is invalid.

==========================================================

4.5.1.3. IOPCHDA\_AsyncRead::ReadProcessed

ppError Codes

Return Code Description

S\_OK The item was read successfully.

OPC\_S\_NODATA No data was found in the specified time range.

OPC\_E\_BADRIGHTS Insufficient rights for this operation.

OPC\_E\_INVALIDHANDLE The handle is invalid.

==========================================================

4.5.1.4. IOPCHDA\_AsyncRead::AdviseProcessed

ppError Codes

Return Code Description

S\_OK The item was read successfully.

OPC\_S\_NODATA No data was found in the specified time range.

OPC\_E\_BADRIGHTS Insufficient rights for this operation.

OPC\_E\_INVALIDHANDLE The handle is invalid.

**Errata 3.0 Optional fields in MODIFIEDITEM are not allowed.**

Section 5.3.6 indicates that certain fields in the MODIFIEDITEM struct maybe set to NULL. This is illegal in DCOM. All members of that structure mustbe valid arrays with a length equal to dwNumValues.

Portions of the specifications updated:

5.3.6. OPCHDA\_MODIFIEDITEM

Member Description

hClient The client provided handle for this item

dwNumValues Count of the number of data items returned for the item.

pftTimeStamps UTC TimeStamps for this item’s values.

pdwQualities The qualities of the data for this item.

pvDataValues The values for the item.

pftModificationTime The time the modification was made. Support for this field is optional

pEditType The modification type for the item.

szUser The name of the user that made the modification. Support for this field is optional.

Errata 4.0 ReadModified needs the OPC\_E\_MAXEXCEEDED error.

A client could specify a time domain that exceeds the server defined limitsfor a call.

Portions of the specifications updated:

==========================================================

4.4.3.4. IOPCHDA\_SyncRead::ReadModified

HRESULT Return Codes

Return Code Description

S\_OK The function was successful.

S\_FALSE The function was partially successful. See the ppErrors to determine what happened.

OPC\_E\_MAXEXCEEDED The maximum number of values returnable by the server was exceeded.

E\_INVALIDARG An Invalid parameter was passed.

E\_NOTIMPL This server does not support this function.

E\_FAIL The function was unsuccessful.

==========================================================

4.5.1.5. IOPCHDA\_AsyncRead::ReadModified

HRESULT Return Codes

Return Code Description

S\_OK The function was successful.

S\_FALSE The function was partially successful. See the ppErrors to determine what happened.

OPC\_E\_MAXEXCEEDED The maximum number of values returnable by the server was exceeded.

E\_NOTIMPL This server does not support this function.

E\_INVALIDARG An invalid parameter was passed.

E\_FAIL The function was unsuccessful.

**Errata 5.0 ReadAttribute needs to defined how to handle NODATA and NOBOUND cases.**

Portions of the specifications updated:

==========================================================

4.4.1.1. IOPCHDA\_Server::GetItemAttributes

An attribute may be added to an item after it is created, therefore, it is possible to request a history for an attribute prior to the time when there was any data. There are two cases:

a) The client specifies a time range where the start time is earlier than the first historical value for an attribute but the end time is after that value. In this case, the server should return a value with the quality of NODATA and a timestamp of StartTime.

b) The client specifies a time range where the both the start and end time are earlier than the first historical value. In this case,

In both cases, the server should return a single value with a quality of NODATA and a timestamp of StartTime. The HRESULT associated with the attribute should be S\_OK.

**Errata 6.0 InsertAnnotations needs to clarify use of timestamps parameters.**

The InsertAnnotations call associates THREE different timestamps with eachannotation value, however, only two of these timestamps can have anymeaning. The spec needs to indicate which of the three timestamps should beignored by the server.It makes most sense to ftTimestamps array of ANNOTATION structure since thisvalue is specified by the ftTimeStamps argument of the function call.However, the client must still provide a valid array for ftTimestamps inorder to keep DCOM happy.

Portions of the specifications updated:

==========================================================

4.4.5.3. IOPCHDA\_SyncAnnotations:: Insert

NOTE: When using IOPCHDA\_SyncAnnotations:: Insert the ftTimestamps array of the OPCHDA\_ANNOTATION sturcutre is redundant. The client must still provide a valid array for this ftTimestamps array as per DCOM rules, however this array will be ignored by the server..

==========================================================

4.5.3.3. IOPCHDA\_AsyncAnnotations::Insert

NOTE: When using IOPCHDA\_SyncAnnotations:: Insert the ftTimestamps array of the OPCHDA\_ANNOTATION sturcutre is redundant. The client must still provide a valid array for this ftTimestamps array as per DCOM rules, however this array will be ignored by the server..

**Errata 7.0 Rules regarding PARTIAL quality aggregates need to be clarified.**

The PARTIAL quality should only be used for aggregates that would otherwisereturn a quality of CALCULATED. Aggregates that return a quality of RAWshould not return the PARTIAL quality. In addition, the server should returnPARTIAL for any interval that begins before the first useable value in thehistorian. Note that the first useable value for an item may be differentthan the first value for an item. This distinction needs to be made clear.

Portions of the specifications updated:

==========================================================

2.9.1.3. Quality

In some cases the time domain of the request is not evenly divisible by the resample interval. If the last subinterval computed is not a complete subinterval, the last aggregate returned shall be based upon that incomplete subinterval, and the quality of the aggregate shall be OPCHDA\_PARTIAL .The PARTIAL quality should only be used for aggregates that would otherwise return a quality of CALCULATED. Aggregates that return a quality of RAW should not return the PARTIAL quality. In addition, the server should return PARTIAL for any interval that begins before the first good useable value in the historian.
