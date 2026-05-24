[MS-UPIGD]:

UPnP Device and Service Templates: Internet Gateway
Device (IGD) Extensions

Intellectual Property Rights Notice for Open Specifications Documentation

  Technical Documentation. Microsoft publishes Open Specifications documentation (“this

documentation”) for protocols, file formats, data portability, computer languages, and standards
support. Additionally, overview documents cover inter-protocol relationships and interactions.

  Copyrights. This documentation is covered by Microsoft copyrights. Regardless of any other

terms that are contained in the terms of use for the Microsoft website that hosts this
documentation, you can make copies of it in order to develop implementations of the technologies
that are described in this documentation and can distribute portions of it in your implementations
that use these technologies or in your documentation as necessary to properly document the
implementation. You can also distribute in your implementation, with or without modification, any
schemas, IDLs, or code samples that are included in the documentation. This permission also
applies to any documents that are referenced in the Open Specifications documentation.
  No Trade Secrets. Microsoft does not claim any trade secret rights in this documentation.
  Patents. Microsoft has patents that might cover your implementations of the technologies

described in the Open Specifications documentation. Neither this notice nor Microsoft's delivery of
this documentation grants any licenses under those patents or any other Microsoft patents.
However, a given Open Specifications document might be covered by the Microsoft Open
Specifications Promise or the Microsoft Community Promise. If you would prefer a written license,
or if the technologies described in this documentation are not covered by the Open Specifications
Promise or Community Promise, as applicable, patent licenses are available by contacting
iplg@microsoft.com.

  License Programs. To see all of the protocols in scope under a specific license program and the

associated patents, visit the Patent Map.

  Trademarks. The names of companies and products contained in this documentation might be
covered by trademarks or similar intellectual property rights. This notice does not grant any
licenses under those rights. For a list of Microsoft trademarks, visit
www.microsoft.com/trademarks.

  Fictitious Names. The example companies, organizations, products, domain names, email

addresses, logos, people, places, and events that are depicted in this documentation are fictitious.
No association with any real company, organization, product, domain name, email address, logo,
person, place, or event is intended or should be inferred.

Reservation of Rights. All other rights are reserved, and this notice does not grant any rights other
than as specifically described above, whether by implication, estoppel, or otherwise.

Tools. The Open Specifications documentation does not require the use of Microsoft programming
tools or programming environments in order for you to develop an implementation. If you have access
to Microsoft programming tools and environments, you are free to take advantage of them. Certain
Open Specifications documents are intended for use in conjunction with publicly available standards
specifications and network programming art and, as such, assume that the reader either is familiar
with the aforementioned material or has immediate access to it.

Support. For questions and support, please contact dochelp@microsoft.com.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 28

Revision Summary

Date

Revision
History

Revision
Class

Comments

12/16/2011  1.0

3/30/2012

1.0

7/12/2012

1.0

10/25/2012  1.0

1/31/2013

1.1

8/8/2013

2.0

11/14/2013  2.0

2/13/2014

2.0

5/15/2014

2.0

New

None

None

None

Minor

Major

None

None

None

Released new document.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

3.0

Major

Significantly changed the technical content.

10/16/2015  3.0

7/14/2016

3.0

6/1/2017

3.0

9/15/2017

4.0

9/12/2018

5.0

4/7/2021

6.0

6/25/2021

7.0

4/23/2024

8.0

None

None

None

Major

Major

Major

Major

Major

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 28

Table of Contents

1.1
1.2

1.2.1
1.2.2

1  Introduction ............................................................................................................ 4
Glossary ........................................................................................................... 4
References ........................................................................................................ 4
Normative References ................................................................................... 4
Informative References ................................................................................. 5
Overview .......................................................................................................... 5
Relationship to Protocols and Other Structures ...................................................... 5
Applicability Statement ....................................................................................... 5
Versioning and Localization ................................................................................. 5
Vendor-Extensible Fields ..................................................................................... 5

1.3
1.4
1.5
1.6
1.7

2  Structures ............................................................................................................... 6
OSInfo Service................................................................................................... 6
WANCommonInterfaceConfig Extensions ............................................................... 6
WANIPConnection Extensions .............................................................................. 8
WANPPPConnection Extensions ............................................................................ 8

2.1
2.2
2.3
2.4

3  Structure Examples ................................................................................................. 9
OSInfo .............................................................................................................. 9
WANCommonInterfaceConfig ............................................................................... 9

3.1
3.2

4  Security ................................................................................................................. 10
Security Considerations for Implementers ........................................................... 10
Index of Security Fields .................................................................................... 10

4.1
4.2

5  Appendix A: XML schema ....................................................................................... 11
WAN Common Interface Config .......................................................................... 11
WAN IP Connection .......................................................................................... 13
WAN PPP Connection ........................................................................................ 19

5.1
5.2
5.3

6  Appendix B: Product Behavior ............................................................................... 26

7  Change Tracking .................................................................................................... 27

8  Index ..................................................................................................................... 28

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 28

1  Introduction

The UPnP: Device & Service Templates: Internet Gateway Device (IGD) Extensions describe
extensions to the Universal Plug-n-Play (UPnP) device schema that describes an Internet gateway
device.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

1.1  Glossary

This document uses the following terms:

action: A command that is exposed by a service, as defined in [UPNPARCH1.1] section i.7.

computer name: The DNS or NetBIOS name.

interface alias: As defined in [RFC2863] section 6, a human-readable name that is associated

with a network interface, and configurable by a network manager.

Internet gateway device: An interconnect device between a local area network (LAN) and a wide

area network (WAN), typically providing connectivity to the Internet.

service description: A formal definition of a logical service, expressed in the UPnP Template

language and written in XML syntax. A service description is specified by a UPnP vendor by
filling in any placeholders in a UPnP Service Template (was SCPD). For more information, see
[UPNPARCH1.1] section 2.6.

MAY, SHOULD, MUST, SHOULD NOT, MUST NOT: These terms (in all caps) are used as defined
in [RFC2119]. All statements of optional behavior use either MAY, SHOULD, or SHOULD NOT.

1.2  References

Links to a document in the Microsoft Open Specifications library point to the correct section in the
most recently published version of the referenced document. However, because individual documents
in the library are not updated at the same time, the section numbers in the documents may not
match. You can confirm the correct section numbering by checking the Errata.

1.2.1  Normative References

We conduct frequent surveys of the normative references to assure their continued availability. If you
have any issue with finding a normative reference, please contact dochelp@microsoft.com. We will
assist you in finding the relevant information.

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[UPNPARCH1.1] UPnP Forum, "UPnP Device Architecture 1.1", October 2008,
http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.1.pdf

[UPNPWCIC] UPnP Forum, "WANCommonInterfaceConfig:1 Service Template Version 1.01", November
2001, http://www.upnp.org/specs/gw/UPnP-gw-WANCommonInterfaceConfig-v1-Service.pdf

[UPNPWIPC] UPnP Forum, "WANIPConnection:1 Service Template Version 1.01", November 2001,
http://www.upnp.org/specs/gw/UPnP-gw-WANIPConnection-v1-Service.pdf

[UPNPWPPC] UPnP Forum, "WANPPPConnection:1 Service Template Version 1.01", November 2001,
http://www.upnp.org/specs/gw/UPnP-gw-WANPPPConnection-v1-Service.pdf

4 / 28

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1.2.2  Informative References

[RFC2863] McCloghrie, K., and Kastenholz, F., "The Interfaces Group MIB", RFC 2863, June 2000,
https://www.rfc-editor.org/info/rfc2863

1.3  Overview

The Internet gateway device (IGD) extensions specified in this document comprise four UPnP
services:

1.  OSInfo is a new service implemented on an IGD that enables retrieval of information about the

operating system running on the IGD.

2.  The WANCommonInterfaceConfig service [UPNPWCIC] is extended to implement an action that
returns uptime information along with usage statistics. Previously this service returned only the
usage statistics, requiring the caller to use its own timestamp for rate estimation. However, using
the client's timestamp of the reception of the information introduces additional uncertainty due to
queuing and propagation delays that can vary by message. Including the IGD's uptime value in
the data instead enables more accurate rate estimation.

3.  The WANIPConnection service [UPNPWIPC] is extended to add a state variable that enables

customization of the name of the interface alias on the WAN interface of the Internet gateway
device.

4.  The WANPPPConnection service [UPNPWPPC] is extended to add a state variable that enables
customization of the name of the interface alias on the WAN interface of the Internet gateway
device.

1.4  Relationship to Protocols and Other Structures

The interface alias of the WAN interface is manageable with either the WANIPConnection service or the
WANPPPConnection service (depending on the type of WAN connection). This same state variable can
also be exposed via other mechanisms, such as the Interfaces Group MIB [RFC2863].

1.5  Applicability Statement

The IGD extensions specified in this document are applicable only to an IGD that has a single WAN
interface.

1.6  Versioning and Localization

There are no localization-dependent structures specified in this document.

1.7  Vendor-Extensible Fields

None.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 28

2  Structures

Data types used in this document are specified in [UPNPARCH1.1] section 2.5.

2.1  OSInfo Service

The OSInfo Service is a service in the Internet gateway device (IGD). Its service description is
specified as follows, in the UPnP Template Language as defined in [UPNPARCH1.1] section 2.6.

 <?xml version="1.0"?>
 <scpd xmlns="urn:schemas-upnp-org:service-1-0">
   <specVersion>
     <major>1</major>
     <minor>0</minor>
   </specVersion>
   <actionList>
    <action>
       <name>MagicOn</name>
     </action>
   </actionList>
   <serviceStateTable>
     <stateVariable>
       <name>OSMajorVersion</name>
       <dataType>i4</dataType>
     </stateVariable>
     <stateVariable>
       <name>OSMinorVersion</name>
       <dataType>i4</dataType>
     </stateVariable>
     <stateVariable>
       <name>OSBuildNumber</name>
       <dataType>i4</dataType>
     </stateVariable>
     <stateVariable>
       <name>OSMachineName</name>
       <dataType>string</dataType>
     </stateVariable>
   </serviceStateTable>
 </scpd>

The following action is defined in the service description above:

MagicOn: A placeholder action created solely to provide a properly formed UPnP service description.

The MagicOn action provides no functionality and always returns success.

The following state variables are defined in the service description above:

OSMajorVersion: The major version of the operating system.

OSMinorVersion: The minor version of the operating system.

OSBuildNumber: The build number of the operating system.

OSMachineName: The computer name of the Internet gateway device.

2.2  WANCommonInterfaceConfig Extensions

The WANCommonInterfaceConfig service is specified in [UPNPWCIC]. Its service description is
extended as follows, in the UPnP Template Language.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 28

 <actionList>
 <action>
     <name>X_GetICSStatistics</name>
       <argumentList>
         <argument>
           <name>TotalBytesSent</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesSent</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalBytesReceived</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesReceived</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalPacketsSent</name>
           <direction>out</direction>
           <relatedStateVariable>TotalPacketsSent</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalPacketsReceived</name>
           <direction>out</direction>
           <relatedStateVariable>TotalPacketsReceived</relatedStateVariable>
         </argument>
         <argument>
           <name>Layer1DownstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>Layer1DownstreamMaxBitRate</relatedStateVariable>
         </argument>
         <argument>
           <name>Uptime</name>
           <direction>out</direction>
           <relatedStateVariable>X_Uptime</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
 </actionList>
 <serviceStateTable>
     <stateVariable sendEvents="no">
       <name>X_PersonalFirewallEnabled</name>
       <dataType>boolean</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>X_Uptime</name>
       <dataType>ui4</dataType>
     </stateVariable>
 </serviceStateTable>

The following action is defined in the service description above:

X_GetICSStatistics: Provides the ability to retrieve uptime information for an Internet gateway

device. This action uses the following errors:

 Value

 Meaning

402

Invalid Args, as defined in [UPNPWCIC] section 2.4.1.4.

501

Action Failed, as defined in [UPNPWCIC] section 2.4.1.4.

The following state variables are defined in the service description above:

X_Uptime: The number of seconds since the IGD has started. This is the same value as the Uptime

variable in the WANIPConnection or WANPPPConnection service.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 28

X_PersonalFirewallEnabled: If the WAN interface on the Internet gateway device is protected by a

firewall, this SHOULD<1> be set to TRUE; otherwise FALSE.

2.3  WANIPConnection Extensions

The WANIPConnection service is specified in [UPNPWIPC]. Its service description is extended as
follows, in the UPnP Template Language.

   <serviceStateTable>
     <stateVariable sendEvents="yes">
       <name>X_Name</name>
       <dataType>string</dataType>
     </stateVariable>
   </serviceStateTable>

The following state variable is defined in the service description above:

X_Name: The interface alias of the external (WAN) interface of the Internet gateway device.

2.4  WANPPPConnection Extensions

The WANPPPConnection service is specified in [UPNPWPPC]. Its service description is extended as
follows, in the UPnP Template Language.

   <serviceStateTable>
     <stateVariable sendEvents="yes">
       <name>X_Name</name>
       <dataType>string</dataType>
     </stateVariable>
   </serviceStateTable>

The following state variable is defined in the service description above:

X_Name: The interface alias of the external (WAN) interface of the Internet gateway device.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 28

3  Structure Examples

3.1  OSInfo

In this example, the IGD is named "SAMPLE-IGD", and a client retrieves operating system
information. It does so as follows.

1.  The client discovers the XML schema for the OS info service as specified in [UPNPARCH1.1] section

1.

2.  The IGD responds with the OSInfo service description specified in section 2.1.

3.  The client then invokes the MagicOn action on the OSInfo service.

4.  The IGD responds with the following data.

 <?xml version="1.0"?>
 <e:propertyset xmlns:e="urn:schemas-upnp-org:event-1-0">
 <e:property><OSMajorVersion xmlns:dt="urn:schemas-microsoft-com:datatypes"
dt:dt="i4">6</OSMajorVersion></e:property>
 <e:property><OSMinorVersion xmlns:dt="urn:schemas-microsoft-com:datatypes"
dt:dt="i4">1</OSMinorVersion></e:property>
 <e:property><OSBuildNumber xmlns:dt="urn:schemas-microsoft-com:datatypes"
dt:dt="i4">7600</OSBuildNumber></e:property>
 <e:property><OSMachineName xmlns:dt="urn:schemas-microsoft-com:datatypes"
dt:dt="string">SAMPLE-IGD</OSMachineName></e:property>
 </e:propertyset>

3.2  WANCommonInterfaceConfig

In the example below, the client computes rate information across a 5-second period of time.

1.  The client invokes the WANCommonInterfaceConfig service with action X_GetICSStatistics.

2.  The IGD returns statistics, including an uptime in the X_Uptime variable.

3.  The client remembers the first set of results, and waits for 5 seconds.

4.  The client again invokes WANCommonInterfaceConfig service with action X_GetICSStatistics.

5.  The IGD returns statistics, including an uptime in the X_Uptime variable.

6.  The client then uses the two sets of information to compute rate information. For example,

incoming bandwidth usage would be (currrent TotalBytesReceived – previous TotalBytesReceived)
/ (current X_Uptime – previous X_Uptime).

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 28

4  Security

4.1  Security Considerations for Implementers

This document has the same security considerations as those defined in [UPNPARCH1.1].

Note that the OS_MachineName could be considered private information. Also operating system
version number information could be used to fingerprint the device in order to identify potential
vulnerabilities.

4.2  Index of Security Fields

None.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 28

5  Appendix A: XML schema

The full service description for the OSInfo service appears in section 2.1. The full service
descriptions for the services extended are given in the following sections.

5.1  WAN Common Interface Config

 <?xml version="1.0"?>
 <scpd xmlns="urn:schemas-upnp-org:service-1-0">
 <specVersion>
 <major>1</major>
 <minor>0</minor>
 </specVersion>
 <actionList>
     <action>
       <name>GetCommonLinkProperties</name>
       <argumentList>
         <argument>
           <name>NewWANAccessType</name>
           <direction>out</direction>
           <relatedStateVariable>WANAccessType</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLayer1UpstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>Layer1UpstreamMaxBitRate</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLayer1DownstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>Layer1DownstreamMaxBitRate</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPhysicalLinkStatus</name>
           <direction>out</direction>
           <relatedStateVariable>PhysicalLinkStatus</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetTotalBytesSent</name>
       <argumentList>
         <argument>
           <name>NewTotalBytesSent</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesSent</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetTotalBytesReceived</name>
       <argumentList>
         <argument>
           <name>NewTotalBytesReceived</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesReceived</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetTotalPacketsSent</name>
       <argumentList>
         <argument>
           <name>NewTotalPacketsSent</name>
           <direction>out</direction>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 28

           <relatedStateVariable>TotalPacketsSent</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetTotalPacketsReceived</name>
       <argumentList>
         <argument>
           <name>NewTotalPacketsReceived</name>
           <direction>out</direction>
          <relatedStateVariable>TotalPacketsReceived</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>X_GetICSStatistics</name>
       <argumentList>
         <argument>
           <name>TotalBytesSent</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesSent</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalBytesReceived</name>
           <direction>out</direction>
           <relatedStateVariable>TotalBytesReceived</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalPacketsSent</name>
           <direction>out</direction>
           <relatedStateVariable>TotalPacketsSent</relatedStateVariable>
         </argument>
         <argument>
           <name>TotalPacketsReceived</name>
           <direction>out</direction>
           <relatedStateVariable>TotalPacketsReceived</relatedStateVariable>
         </argument>
         <argument>
           <name>Layer1DownstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>Layer1DownstreamMaxBitRate</relatedStateVariable>
         </argument>
         <argument>
           <name>Uptime</name>
           <direction>out</direction>
           <relatedStateVariable>X_Uptime</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
 </actionList>
 <serviceStateTable>
 <stateVariable sendEvents="no">
 <name>WANAccessType</name>
 <dataType>string</dataType>
 <allowedValueList>
 <allowedValue>DSL</allowedValue>
 <allowedValue>POTS</allowedValue>
 <allowedValue>Cable</allowedValue>
 <allowedValue>Ethernet</allowedValue>
 <allowedValue>Other</allowedValue>
 </allowedValueList>
 </stateVariable>
 <stateVariable sendEvents="no">
 <name>Layer1UpstreamMaxBitRate</name>
 <dataType>ui4</dataType>
 </stateVariable>
 <stateVariable sendEvents="no">
 <name>Layer1DownstreamMaxBitRate</name>
 <dataType>ui4</dataType>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 28

 </stateVariable>
 <stateVariable sendEvents="yes">
 <name>PhysicalLinkStatus</name>
 <dataType>string</dataType>
       <allowedValueList>
         <allowedValue>Up</allowedValue>
         <allowedValue>Down</allowedValue>
         <allowedValue>Initializing</allowedValue>
         <allowedValue>Unavailable</allowedValue>
       </allowedValueList>
 </stateVariable>
     <stateVariable sendEvents="no">
       <name>WANAccessProvider</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>MaximumActiveConnections</name>
       <dataType>ui2</dataType>
       <allowedValueRange>
         <minimum>1</minimum>
         <maximum></maximum>
         <step>1</step>
       </allowedValueRange>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>TotalBytesSent</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>TotalBytesReceived</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>TotalPacketsSent</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>TotalPacketsReceived</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>X_PersonalFirewallEnabled</name>
       <dataType>boolean</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>X_Uptime</name>
       <dataType>ui4</dataType>
     </stateVariable>
 </serviceStateTable>
 </scpd>

5.2  WAN IP Connection

 <?xml version="1.0"?>
 <scpd xmlns="urn:schemas-upnp-org:service-1-0">
   <specVersion>
     <major>1</major>
     <minor>0</minor>
   </specVersion>
   <actionList>
     <action>
       <name>SetConnectionType</name>
       <argumentList>
         <argument>
           <name>NewConnectionType</name>
           <direction>in</direction>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 28

           <relatedStateVariable>ConnectionType</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetConnectionTypeInfo</name>
       <argumentList>
         <argument>
           <name>NewConnectionType</name>
           <direction>out</direction>
           <relatedStateVariable>ConnectionType</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPossibleConnectionTypes</name>
           <direction>out</direction>
           <relatedStateVariable>PossibleConnectionTypes</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>RequestConnection</name>
     </action>
     <action>
       <name>ForceTermination</name>
     </action>
     <action>
      <name>GetStatusInfo</name>
       <argumentList>
         <argument>
           <name>NewConnectionStatus</name>
           <direction>out</direction>
           <relatedStateVariable>ConnectionStatus</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLastConnectionError</name>
           <direction>out</direction>
           <relatedStateVariable>LastConnectionError</relatedStateVariable>
         </argument>
         <argument>
           <name>NewUptime</name>
           <direction>out</direction>
           <relatedStateVariable>Uptime</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetNATRSIPStatus</name>
       <argumentList>
         <argument>
           <name>NewRSIPAvailable</name>
           <direction>out</direction>
           <relatedStateVariable>RSIPAvailable</relatedStateVariable>
         </argument>
         <argument>
           <name>NewNATEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>NATEnabled</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetGenericPortMappingEntry</name>
       <argumentList>
         <argument>
           <name>NewPortMappingIndex</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingNumberOfEntries</relatedStateVariable>
         </argument>
         <argument>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 28

           <name>NewRemoteHost</name>
           <direction>out</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalClient</name>
           <direction>out</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>
         <argument>
           <name>NewEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetSpecificPortMappingEntry</name>
       <argumentList>
         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalClient</name>
           <direction>out</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 28

         <argument>
           <name>NewEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>AddPortMapping</name>
       <argumentList>
         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalClient</name>
           <direction>in</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>
         <argument>
           <name>NewEnabled</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>DeletePortMapping</name>
       <argumentList>
         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 28

         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetExternalIPAddress</name>
       <argumentList>
         <argument>
           <name>NewExternalIPAddress</name>
           <direction>out</direction>
         <relatedStateVariable>ExternalIPAddress</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
   </actionList>
   <serviceStateTable>
     <stateVariable sendEvents="no">
       <name>ConnectionType</name>
       <dataType>string</dataType>
       <defaultValue>Unconfigured</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>PossibleConnectionTypes</name>
       <dataType>string</dataType>
       <allowedValueList>
         <allowedValue>Unconfigured</allowedValue>
         <allowedValue>IP_Routed</allowedValue>
         <allowedValue>IP_Bridged</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>ConnectionStatus</name>
       <dataType>string</dataType>
       <defaultValue>Unconfigured</defaultValue>
       <allowedValueList>
         <allowedValue>Unconfigured</allowedValue>
       <allowedValue>Connecting</allowedValue>
       <allowedValue>Authenticating</allowedValue>
         <allowedValue>PendingDisconnect</allowedValue>
         <allowedValue>Disconnecting</allowedValue>
         <allowedValue>Disconnected</allowedValue>
         <allowedValue>Connected</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>Uptime</name>
       <dataType>ui4</dataType>
       <defaultValue>0</defaultValue>
       <allowedValueRange>
         <minimum>0</minimum>
         <maximum></maximum>
         <step>1</step>
       </allowedValueRange>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>RSIPAvailable</name>
       <dataType>boolean</dataType>
       <defaultValue>0</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="no">

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 28

       <name>NATEnabled</name>
       <dataType>boolean</dataType>
       <defaultValue>1</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>X_Name</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>LastConnectionError</name>
       <dataType>string</dataType>
       <defaultValue>ERROR_NONE</defaultValue>
       <allowedValueList>
         <allowedValue>ERROR_NONE</allowedValue>
         <allowedValue>ERROR_ISP_TIME_OUT</allowedValue>
         <allowedValue>ERROR_COMMAND_ABORTED</allowedValue>
         <allowedValue>ERROR_ NOT_ENABLED_FOR_INTERNET</allowedValue>
         <allowedValue>ERROR_BAD_PHONE_NUMBER</allowedValue>
         <allowedValue>ERROR_USER_DISCONNECT</allowedValue>
         <allowedValue>ERROR_ISP_DISCONNECT</allowedValue>
         <allowedValue>ERROR_IDLE_DISCONNECT</allowedValue>
         <allowedValue>ERROR_FORCED_DISCONNECT</allowedValue>
         <allowedValue>ERROR_SERVER_OUT_OF_RESOURCES</allowedValue>
         <allowedValue>ERROR_RESTRICTED_LOGON_HOURS</allowedValue>
         <allowedValue>ERROR_ACCOUNT_DISABLED</allowedValue>
         <allowedValue>ERROR_ACCOUNT_EXPIRED</allowedValue>
         <allowedValue>ERROR_PASSWORD_EXPIRED</allowedValue>
         <allowedValue>ERROR_AUTHENTICATION_FAILURE</allowedValue>
         <allowedValue>ERROR_NO_DIALTONE</allowedValue>
         <allowedValue>ERROR_NO_CARRIER</allowedValue>
         <allowedValue>ERROR_NO_ANSWER</allowedValue>
         <allowedValue>ERROR_LINE_BUSY</allowedValue>
         <allowedValue>ERROR_UNSUPPORTED_BITSPERSECOND</allowedValue>
         <allowedValue>ERROR_TOO_MANY_LINE_ERRORS</allowedValue>
         <allowedValue>ERROR_IP_CONFIGURATION</allowedValue>
         <allowedValue>ERROR_UNKNOWN</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>ExternalIPAddress</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>RemoteHost</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>ExternalPort</name>
       <dataType>ui2</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>InternalPort</name>
       <dataType>ui2</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingProtocol</name>
       <dataType>string</dataType>
       <allowedValueList>
         <allowedValue>TCP</allowedValue>
         <allowedValue>UDP</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>InternalClient</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingDescription</name>
       <dataType>string</dataType>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 28

     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingEnabled</name>
       <dataType>boolean</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingLeaseDuration</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>PortMappingNumberOfEntries</name>
       <dataType>ui2</dataType>
     </stateVariable>
   </serviceStateTable>
 </scpd>

5.3  WAN PPP Connection

 <?xml version="1.0"?>
 <scpd xmlns="urn:schemas-upnp-org:service-1-0">
   <specVersion>
     <major>1</major>
     <minor>0</minor>
   </specVersion>
   <actionList>
     <action>
       <name>SetConnectionType</name>
       <argumentList>
         <argument>
           <name>NewConnectionType</name>
           <direction>in</direction>
           <relatedStateVariable>ConnectionType</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetConnectionTypeInfo</name>
       <argumentList>
         <argument>
           <name>NewConnectionType</name>
           <direction>out</direction>
           <relatedStateVariable>ConnectionType</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPossibleConnectionTypes</name>
           <direction>out</direction>
           <relatedStateVariable>PossibleConnectionTypes</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>RequestConnection</name>
     </action>
     <action>
       <name>ForceTermination</name>
     </action>
     <action>
      <name>GetStatusInfo</name>
       <argumentList>
         <argument>
           <name>NewConnectionStatus</name>
           <direction>out</direction>
           <relatedStateVariable>ConnectionStatus</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLastConnectionError</name>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 28

           <direction>out</direction>
           <relatedStateVariable>LastConnectionError</relatedStateVariable>
         </argument>
         <argument>
           <name>NewUptime</name>
           <direction>out</direction>
           <relatedStateVariable>Uptime</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetNATRSIPStatus</name>
       <argumentList>
         <argument>
           <name>NewRSIPAvailable</name>
           <direction>out</direction>
           <relatedStateVariable>RSIPAvailable</relatedStateVariable>
         </argument>
         <argument>
           <name>NewNATEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>NATEnabled</relatedStateVariable>
         </argument>
       </argumentList>
      </action>
      <action>
       <name>GetLinkLayerMaxBitRates</name>
       <argumentList>
         <argument>
           <name>NewUpstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>UpstreamMaxBitRate</relatedStateVariable>
         </argument>
         <argument>
           <name>NewDownstreamMaxBitRate</name>
           <direction>out</direction>
           <relatedStateVariable>DownstreamMaxBitRate</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetGenericPortMappingEntry</name>
       <argumentList>
         <argument>
           <name>NewPortMappingIndex</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingNumberOfEntries</relatedStateVariable>
         </argument>
         <argument>
           <name>NewRemoteHost</name>
           <direction>out</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 28

           <name>NewInternalClient</name>
           <direction>out</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>
         <argument>
           <name>NewEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>GetSpecificPortMappingEntry</name>
       <argumentList>
         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>out</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalClient</name>
           <direction>out</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>
         <argument>
           <name>NewEnabled</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>out</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>AddPortMapping</name>
       <argumentList>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

21 / 28

         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>InternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewInternalClient</name>
           <direction>in</direction>
           <relatedStateVariable>InternalClient</relatedStateVariable>
         </argument>
         <argument>
           <name>NewEnabled</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingEnabled</relatedStateVariable>
         </argument>
         <argument>
           <name>NewPortMappingDescription</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingDescription</relatedStateVariable>
         </argument>
         <argument>
           <name>NewLeaseDuration</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingLeaseDuration</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
       <name>DeletePortMapping</name>
       <argumentList>
         <argument>
           <name>NewRemoteHost</name>
           <direction>in</direction>
           <relatedStateVariable>RemoteHost</relatedStateVariable>
         </argument>
         <argument>
           <name>NewExternalPort</name>
           <direction>in</direction>
           <relatedStateVariable>ExternalPort</relatedStateVariable>
         </argument>
         <argument>
           <name>NewProtocol</name>
           <direction>in</direction>
           <relatedStateVariable>PortMappingProtocol</relatedStateVariable>
         </argument>
       </argumentList>
     </action>
     <action>
     <name>GetExternalIPAddress</name>
       <argumentList>
         <argument>
           <name>NewExternalIPAddress</name>
           <direction>out</direction>
         <relatedStateVariable>ExternalIPAddress</relatedStateVariable>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

22 / 28

         </argument>
       </argumentList>
     </action>
   </actionList>
   <serviceStateTable>
     <stateVariable sendEvents="no">
       <name>ConnectionType</name>
       <dataType>string</dataType>
       <defaultValue>Unconfigured</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>PossibleConnectionTypes</name>
       <dataType>string</dataType>
       <defaultValue>Unconfigured</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>ConnectionStatus</name>
       <dataType>string</dataType>
       <defaultValue>Unconfigured</defaultValue>
       <allowedValueList>
         <allowedValue>Unconfigured</allowedValue>
       <allowedValue>Connecting</allowedValue>
       <allowedValue>Authenticating</allowedValue>
         <allowedValue>PendingDisconnect</allowedValue>
         <allowedValue>Disconnecting</allowedValue>
         <allowedValue>Disconnected</allowedValue>
         <allowedValue>Connected</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>Uptime</name>
       <dataType>ui4</dataType>
       <defaultValue>0</defaultValue>
       <allowedValueRange>
         <minimum>0</minimum>
         <maximum></maximum>
         <step>1</step>
       </allowedValueRange>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>UpstreamMaxBitRate</name>
       <dataType>ui4</dataType>
       <defaultValue>0</defaultValue>
       <allowedValueRange>
         <minimum>0</minimum>
         <maximum></maximum>
         <step></step>
       </allowedValueRange>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>DownstreamMaxBitRate</name>
       <dataType>ui4</dataType>
       <defaultValue>0</defaultValue>
       <allowedValueRange>
         <minimum>0</minimum>
         <maximum></maximum>
         <step></step>
       </allowedValueRange>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>LastConnectionError</name>
       <dataType>string</dataType>
       <defaultValue>ERROR_NONE</defaultValue>
       <allowedValueList>
         <allowedValue>ERROR_NONE</allowedValue>
         <allowedValue>ERROR_ISP_TIME_OUT</allowedValue>
         <allowedValue>ERROR_COMMAND_ABORTED</allowedValue>
         <allowedValue>ERROR_NOT_ENABLED_FOR_INTERNET</allowedValue>
         <allowedValue>ERROR_BAD_PHONE_NUMBER</allowedValue>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

23 / 28

         <allowedValue>ERROR_USER_DISCONNECT</allowedValue>
         <allowedValue>ERROR_ISP_DISCONNECT</allowedValue>
         <allowedValue>ERROR_IDLE_DISCONNECT</allowedValue>
         <allowedValue>ERROR_FORCED_DISCONNECT</allowedValue>
         <allowedValue>ERROR_SERVER_OUT_OF_RESOURCES</allowedValue>
         <allowedValue>ERROR_RESTRICTED_LOGON_HOURS</allowedValue>
         <allowedValue>ERROR_ACCOUNT_DISABLED</allowedValue>
         <allowedValue>ERROR_ACCOUNT_EXPIRED</allowedValue>
         <allowedValue>ERROR_PASSWORD_EXPIRED</allowedValue>
         <allowedValue>ERROR_AUTHENTICATION_FAILURE</allowedValue>
         <allowedValue>ERROR_NO_DIALTONE</allowedValue>
         <allowedValue>ERROR_NO_CARRIER</allowedValue>
         <allowedValue>ERROR_NO_ANSWER</allowedValue>
         <allowedValue>ERROR_LINE_BUSY</allowedValue>
         <allowedValue>ERROR_UNSUPPORTED_BITSPERSECOND</allowedValue>
         <allowedValue>ERROR_TOO_MANY_LINE_ERRORS</allowedValue>
         <allowedValue>ERROR_IP_CONFIGURATION</allowedValue>
         <allowedValue>ERROR_UNKNOWN</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>RSIPAvailable</name>
       <dataType>boolean</dataType>
       <defaultValue>0</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>NATEnabled</name>
       <dataType>boolean</dataType>
       <defaultValue>1</defaultValue>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>X_Name</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>ExternalIPAddress</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>RemoteHost</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>ExternalPort</name>
       <dataType>ui2</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>InternalPort</name>
       <dataType>ui2</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingProtocol</name>
       <dataType>string</dataType>
       <allowedValueList>
         <allowedValue>TCP</allowedValue>
         <allowedValue>UDP</allowedValue>
       </allowedValueList>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>InternalClient</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingDescription</name>
       <dataType>string</dataType>
     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingEnabled</name>
       <dataType>boolean</dataType>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

24 / 28

     </stateVariable>
     <stateVariable sendEvents="no">
       <name>PortMappingLeaseDuration</name>
       <dataType>ui4</dataType>
     </stateVariable>
     <stateVariable sendEvents="yes">
       <name>PortMappingNumberOfEntries</name>
       <dataType>ui2</dataType>
     </stateVariable>
   </serviceStateTable>
 </scpd>

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

25 / 28

6  Appendix B: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows XP operating system

  Windows Server 2003 operating system

  Windows Vista operating system

  Windows Server 2008 operating system

  Windows 7 operating system

  Windows Server 2008 R2 operating system

  Windows 8 operating system

  Windows Server 2012 operating system

  Windows 8.1 operating system

  Windows Server 2012 R2 operating system

  Windows 10 operating system

  Windows Server 2016 operating system

  Windows Server 2019 operating system

  Windows Server 2022 operating system

  Windows 11 operating system

  Windows Server 2025 operating system

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 2.2: Windows XP and Windows Server 2003 set X_PersonalFirewallEnabled to TRUE, if
the Internet connection firewall is enabled on the WAN interface of the gateway device. If any other
firewall is used, X_PersonalFirewallEnabled is set to FALSE. All other versions of Windows always set
X_PersonalFirewallEnabled to FALSE.

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

26 / 28

7  Change Tracking

This section identifies changes that were made to this document since the last release. Changes are
classified as Major, Minor, or None.

The revision class Major means that the technical content in the document was significantly revised.
Major changes affect protocol interoperability or implementation. Examples of major changes are:

  A document revision that incorporates changes to interoperability requirements.
  A document revision that captures changes to protocol functionality.

The revision class Minor means that the meaning of the technical content was clarified. Minor changes
do not affect protocol interoperability or implementation. Examples of minor changes are updates to
clarify ambiguity at the sentence, paragraph, or table level.

The revision class None means that no new technical changes were introduced. Minor editorial and
formatting changes may have been made, but the relevant technical content is identical to the last
released version.

The changes made to this document are listed in the following table. For more information, please
contact dochelp@microsoft.com.

Section

Description

6 Appendix B: Product
Behavior

Added Windows Server 2025 to the list of applicable
products.

Revision
class

Major

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

27 / 28

8  Index
A

Applicability 5

C

Change tracking 27
Common data types and fields 6

D

Data types and fields - common 6
Details
   common data types and fields 6

E

Examples
   OSInfo 9
   WANCommonInterfaceConfig 9
Extensions
   WANCommonInterfaceConfig 6
   WANIPConnection 8
   WANPPPConnection 8

F

Field index - security 10
Fields - security index 10
Fields - vendor-extensible 5
Full XML schema
   overview 11
   WAN Common Interface Config 11
   WAN IP Connection 13
   WAN PPP Connection 19

G

Glossary 4

I

Implementer - security considerations 10
Index of security fields 10
Informative references 5
Introduction 4

L

Localization 5

N

Normative references 4

O

OSInfo example 9
OSInfo service 6
Overview (synopsis) 5

P

Product behavior 26

R

References 4
   informative 5
   normative 4
Relationship to protocols and other structures 5

S

Security
   field index 10
   implementer considerations 10
Services
   OSInfo 6
   WANCommonInterfaceConfig 6
   WANIPConnection 8
   WANPPPConnection 8
Structures
   OSInfo service 6
   overview 6
   WANCommonInterfaceConfig service 6
   WANIPConnection service 8
   WANPPPConnection service 8

T

Tracking changes 27

V

Vendor-extensible fields 5
Versioning 5

W

WAN Common Interface Config schema 11
WAN IP Connection schema 13
WAN PPP Connection schema 19
WANCommonInterfaceConfig example 9
WANCommonInterfaceConfig service 6
WANIPConnection service 8
WANPPPConnection service 8

X

XML schema
   overview 11
   WAN Common Interface Config 11
   WAN IP Connection 13
   WAN PPP Connection 19

[MS-UPIGD] - v20240423
UPnP Device and Service Templates: Internet Gateway Device (IGD) Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

28 / 28

