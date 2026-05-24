[MS-PBSD]:

Publication Services Data Structure

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

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 18

Revision Summary

Date

Revision
History

Revision
Class

Comments

9/23/2011

0.1

12/16/2011  0.1

3/30/2012

1.0

7/12/2012

1.0

10/25/2012  1.0

1/31/2013

2.0

8/8/2013

3.0

11/14/2013  3.0

New

None

None

None

None

Major

Major

None

Released new document

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

2/13/2014

4.0

Major

Significantly changed the technical content.

5/15/2014

4.0

None

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

5.0

Major

Significantly changed the technical content.

10/16/2015  5.0

7/14/2016

5.0

6/1/2017

5.0

9/15/2017

6.0

9/12/2018

7.0

4/7/2021

8.0

6/25/2021

9.0

4/23/2024

10.0

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

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 18

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
Relationship to Protocols and Other Structures ...................................................... 6
Applicability Statement ....................................................................................... 6
Versioning and Localization ................................................................................. 6
Vendor-Extensible Fields ..................................................................................... 6

1.3
1.4
1.5
1.6
1.7

2  Structures ............................................................................................................... 7
Namespaces ...................................................................................................... 7
Computer Information ........................................................................................ 7
Resource ........................................................................................................... 8

2.1
2.2
2.3

3  Structure Examples ................................................................................................. 9
Publication Services Data with no Listed Services ................................................... 9
Publication Services Data with Listed Resources ................................................... 10

3.1
3.2

4  Security ................................................................................................................. 14
Security Considerations for Implementers ........................................................... 14
Index Of Security Fields .................................................................................... 14

4.1
4.2

5  Appendix A: Full XML Schema ................................................................................ 15

6  Appendix B: Product Behavior ............................................................................... 16

7  Change Tracking .................................................................................................... 17

8  Index ..................................................................................................................... 18

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 18

1  Introduction

Publication Services enables computers to advertise (that is, publish) their presence and their
resources as Web services over IP-based networks. By doing so, other computers can find (that is,
discover) the Web services that have been published on the same network.

This document specifies the data, along with its structure, that computers use to describe themselves
and the resources that are being offered.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

1.1  Glossary

This document uses the following terms:

client: A computer on which the remote procedure call (RPC) client is executing.

device: Any peripheral or part of a computer system that can send or receive data.

domain name: A domain name or a NetBIOS name that identifies a domain.

endpoint: In the context of a web service, a network target to which a SOAP message can be

addressed. See [WSADDR].

NetBIOS: A particular network transport that is part of the LAN Manager protocol suite. NetBIOS

uses a broadcast communication style that was applicable to early segmented local area
networks. A protocol family including name resolution, datagram, and connection services. For
more information, see [RFC1001] and [RFC1002].

Publication Services: A Microsoft Windows Devices Profile for Web Services (DPWS) service that
enables computers to publish resources so that those resources can be discovered by other
computers on the same network.

service: A process or agent that is available on the network, offering resources or services for

clients. Examples of services include file servers, web servers, and so on.

web service: A software system designed to support interoperable machine-to-machine
interaction over a network, using XML-based standards and open transport protocols.

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

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 18

[DPWS] Chans, S., Conti, D., Schlimmer, J., et al., "Devices Profile for Web Services", February 2006,
http://specs.xmlsoap.org/ws/2006/02/devprof/devicesprofile.pdf

[MS-DPWSSN] Microsoft Corporation, "Devices Profile for Web Services (DPWS): Size Negotiation
Extension".

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[MSDN-PnP-X] Microsoft Corporation, "PnP X: Plug and Play Extensions for Windows Specification",
https://msdn.microsoft.com/en-us/windows/hardware/gg463082.aspx

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[W3C-XSD] World Wide Web Consortium, "XML Schema Part 2: Datatypes Second Edition", 28
October 2004, http://www.w3.org/TR/2004/REC-xmlschema-2-20041028

[WS-Discovery] Beatty, J., Kakivaya, G., Kemp D., et al., "Web Services Dynamic Discovery (WS-
Discovery)", April 2005, http://specs.xmlsoap.org/ws/2005/04/discovery/ws-discovery.pdf

[XMLNS] Bray, T., Hollander, D., Layman, A., et al., Eds., "Namespaces in XML 1.0 (Third Edition)",
W3C Recommendation, December 2009, https://www.w3.org/TR/2009/REC-xml-names-20091208/

1.2.2  Informative References

[MS-DPWSRP] Microsoft Corporation, "Devices Profile for Web Services (DPWS): Shared Resource
Publishing Data Structure".

[MS-HGRP] Microsoft Corporation, "HomeGroup Protocol".

[MSDN-DPWS] Microsoft Corporation, "Understanding Devices Profile for Web Services, WS-Discovery,
and SOAP-over-UDP", http://msdn.microsoft.com/en-us/library/dd179231.aspx

[MSDN-WSD] Microsoft Corporation, "Web Services on Devices", http://msdn.microsoft.com/en-
us/library/bb756908.aspx

[WSADDR] Gudgin, M., Hadley, M., and Rogers, T., "Web Services Addressing (WS-Addressing) 1.0",
W3C Recommendation, May 2006, http://www.w3.org/2005/08/addressing

1.3  Overview

The Devices Profile for Web Services (DPWS), as defined in [DPWS], specifies a well-structured
messaging model that provides basic functionality such as discovery of an endpoint [WSADDR],
metadata for that endpoint, and request/response messaging. [DPWS] specifies the role of clients,
devices, and services. Clients discover services and communicate with their endpoints. Devices are
special service endpoints that can host other services. [DPWS] defines metadata for both devices and
the services hosted by those devices. Clients can request this metadata and send requests to specific
endpoints described in the metadata.

This model fits the requirements of a computer. Computers are often set to be discoverable and
advertise metadata that describes themselves and their resources to clients on a network.
Publication Services models a computer as a DPWS device, and the resources on a computer are
modeled as Web services within the same device. The information that describes a computer and the
actual data associated with the resources are stored as part of the device metadata.

The Publication Services Data Structure document defines the organization used for the information
that describes how a computer and its resources are represented within the DPWS device metadata.

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 18

For additional explanatory information about DPWS, see [MSDN-DPWS]. For information about the
Microsoft implementation of DPWS, which is called Web Services for Devices (WSD), see [MSDN-
WSD].

1.4  Relationship to Protocols and Other Structures

The Publication Services data structure is built on [DPWS] and relies on the Microsoft
implementation of DPWS. For more information, see [MSDN-WSD].

Because the number of resources on a computer and the data associated with them can increase over
time, clients also implement the Size Negotiation Extension as specified in [MS-DPWSSN] to be able to
retrieve resources completely.<1>

The HomeGroup Protocol [MS-HGRP], which is used to create a trust relationship that facilitates the
advertising and publishing of content between machines in a home environment, and the Shell
Publishing data structure [MS-DPWSRP] use the Publication Services data structure to advertise their
content and resources.

1.5  Applicability Statement

Use of this data structure is suitable for clients if:







The client is a compliant DPWS implementation.

The client requires communication with the DPWS representation of a computer and its resources.

The client supports the Size Negotiation Extension [MS-DPWSSN] and can receive messages
longer than 32767 octets ([MS-DTYP] section 2.1.5).

Use of this protocol is suitable for service implementations if:





The service will represent itself as a DPWS-compliant computer on the network.

The service supports the Size Negotiation Extension [MS-DPWSSN] and can send messages longer
than 32767 octets.

1.6  Versioning and Localization

This data structure is not versioned and it does not explicitly support localization.

1.7  Vendor-Extensible Fields

This data structure does not define any additional vendor-extensible fields.

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 18

2  Structures

2.1  Namespaces

This document defines and references various XML namespaces using the mechanisms specified in
[XMLNS]. Even though this document associates a specific XML namespace prefix for each XML
namespace that is used, the choice of any particular XML namespace prefix is implementation-specific
and not significant for interoperability.

Prefix  Namespace URI

Reference

pub

http://schemas.microsoft.com/windows/pub/2005/07

This document

pnpx

http://schemas.microsoft.com/windows/pnpx/2005/10

[MSDN-PnP-X]

wsd

http://schemas.xmlsoap.org/ws/2005/04/discovery

[WS-Discovery]

wsdp

http://schemas.xmlsoap.org/ws/2006/02/devprof

[DPWS]

xs

http://www.w3.org/2001/XMLSchema

[W3C-XSD]

2.2  Computer Information

The Publication Services Data Structure advertises three pieces of information about a computer:

  A type indicating that the device represents a computer





The NetBIOS domain name for the computer

The NetBIOS computer name

 <xs:simpleType name="DiscoveryTypeValues">
   <xs:restriction base="xs:QName">
     <xs:enumeration value="Computer" />
   </xs:restriction>
 </xs:simpleType>

 <xs:element name="Computer" type="xs:string" minOccurs="0" />

Element

Description

wsdp:Relationship/wsdp:Host/wsdp:Types

Indicator type for a device representation of a Publication
Services-enabled computer (pub:Computer).

wsdp:Relationship/wsdp:Host/pub:Computer  NetBIOS information for the computer.

NetBIOS information for the computer is formatted as follows:



If the computer is domain joined:

 <NetBIOS_Computer_Name>/Domain:<NetBIOS_Domain_Name>



If the computer is in a workgroup:

 <NetBIOS_Computer_Name>\Workgroup:<Workgroup_Name>

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 18

  Otherwise:

 <NetBIOS_Computer_Name>\NotJoined

Note  The pub:Computer element replaces the <any> element wildcard of the HostServiceType
complex type, which is instantiated through the <Host> element of the DPWS schema.

2.3  Resource

Each resource to be advertised by a Publication Services device host as a service consists of two
fields:



Type

  Actual data

The resource type is used as the Types (wsdp:Relationship/wsdp:Hosted/wsdp:Types) implemented
by a Host Service (see [WS-Discovery]).

The Publication Services Data Structure defines a new element to enclose the actual data associated
with the resource as follows.

 <xs:element name="Resource" type="xs:string" minOccurs="0" />

Element

Description

wsdp:Relationship/wsdp:Hosted/pub:Resource

Resource data

wsdp:Relationship/wsdp:Hosted/pub:Resource1  Resource data continued

...

...

wsdp:Relationship/wsdp:Hosted/pub:ResourceN  Resource data continued

If the data is more than 8192 octets ([MS-DTYP] section 2.1.5), it can be split into multiple Resource
elements. Clients that want to retrieve the full resource data must combine the data in the order
implied by the element names.

Note  The pub:Resource* elements replace the <any> element wildcard of the HostServiceType
complex type, which is instantiated through the <Hosted> element of the DPWS schema.

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 18

3  Structure Examples

3.1  Publication Services Data with no Listed Services

This section contains an example of the Publication Services data associated with a computer (that
is, a device). The example data indicates that there are no services being advertised by the
computer.

 <?xml version="1.0" encoding="utf-8" ?>
 <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing"
xmlns:wsx="http://schemas.xmlsoap.org/ws/2004/09/mex"
xmlns:wsdp="http://schemas.xmlsoap.org/ws/2006/02/devprof"
xmlns:pnpx="http://schemas.microsoft.com/windows/pnpx/2005/10"
xmlns:pub="http://schemas.microsoft.com/windows/pub/2005/07">
   <soap:Header>
     <wsa:To>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:To>
     <wsa:Action>http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse</wsa:Action>
     <wsa:MessageID>urn:uuid:8a07696e-8128-456c-af85-9a5008aa0446</wsa:MessageID>
     <wsa:RelatesTo>urn:uuid:bac3eeec-5488-4a27-99c9-795d0651d914</wsa:RelatesTo>
   </soap:Header>
   <soap:Body>
     <wsx:Metadata>
       <wsx:MetadataSection
Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/ThisDevice">
         <wsdp:ThisDevice>
           <wsdp:FriendlyName>Contoso Publication Service Device Host</wsdp:FriendlyName>
           <wsdp:FirmwareVersion>1.0</wsdp:FirmwareVersion>
           <wsdp:SerialNumber>20050718</wsdp:SerialNumber>
         </wsdp:ThisDevice>
       </wsx:MetadataSection>
       <wsx:MetadataSection Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/ThisModel">
         <wsdp:ThisModel>
           <wsdp:Manufacturer>Contoso, Ltd</wsdp:Manufacturer>
           <wsdp:ManufacturerUrl>http://www.contoso.com</wsdp:ManufacturerUrl>
           <wsdp:ModelName>Contoso Publication Service</wsdp:ModelName>
           <wsdp:ModelNumber>1</wsdp:ModelNumber>
           <wsdp:ModelUrl>http://www.contoso.com</wsdp:ModelUrl>
           <wsdp:PresentationUrl>http://www.contoso.com</wsdp:PresentationUrl>
           <pnpx:DeviceCategory>Computers</pnpx:DeviceCategory>
         </wsdp:ThisModel>
       </wsx:MetadataSection>
       <wsx:MetadataSection
Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/Relationship">
        <wsdp:Relationship Type="http://schemas.xmlsoap.org/ws/2006/02/devprof/host">
         <wsdp:Host>
           <wsa:EndpointReference>
             <wsa:Address>urn:uuid:95ce917d-9e37-4099-aee4-db3292041ea6</wsa:Address>
           </wsa:EndpointReference>
           <wsdp:Types>pub:Computer</wsdp:Types>
           <wsdp:ServiceId>urn:uuid:95ce917d-9e37-4099-aee4-db3292041ea6</wsdp:ServiceId>
           <pub:Computer>D-HAMILTON-1/Domain:CONTOSO_DOMAIN1</pub:Computer>
         </wsdp:Host>
       </wsdp:Relationship>
       </wsx:MetadataSection>
     </wsx:Metadata>
   </soap:Body>
 </soap:Envelope>

From the example data, the following lines make use of the Computer Information structure (section
2.2):

           <wsdp:Types>pub:Computer</wsdp:Types>

9 / 18

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

 ...
           <pub:Computer>D-HAMILTON-1/Domain:CONTOSO_DOMAIN1</pub:Computer>

3.2  Publication Services Data with Listed Resources

This section contains an example of the Publication Services data associated with a computer (that
is, a device) along with one service that is being advertised by that computer.

 <?xml version="1.0" encoding="utf-8"?>
 <soap:Envelope xmlns:soap="http://www.w3.org/2003/05/soap-envelope"
xmlns:wsa="http://schemas.xmlsoap.org/ws/2004/08/addressing"
xmlns:wsx="http://schemas.xmlsoap.org/ws/2004/09/mex"
xmlns:wsdp="http://schemas.xmlsoap.org/ws/2006/02/devprof"
xmlns:pnpx="http://schemas.microsoft.com/windows/pnpx/2005/10"
xmlns:pub="http://schemas.microsoft.com/windows/pub/2005/07">
   <soap:Header>
     <wsa:To>http://schemas.xmlsoap.org/ws/2004/08/addressing/role/anonymous</wsa:To>
     <wsa:Action>http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse</wsa:Action>
     <wsa:MessageID>urn:uuid:009c39c2-5835-4660-a4bf-53c6fd9b96fd</wsa:MessageID>
     <wsa:RelatesTo>urn:uuid:fc7cdb58-30b4-4e0d-a309-5bd7db689a0c</wsa:RelatesTo>
   </soap:Header>
   <soap:Body>
     <wsx:Metadata>
       <wsx:MetadataSection
Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/ThisDevice">
         <wsdp:ThisDevice>
           <wsdp:FriendlyName>Contoso Publication Service Device Host</wsdp:FriendlyName>
           <wsdp:FirmwareVersion>1.0</wsdp:FirmwareVersion>
           <wsdp:SerialNumber>20050718</wsdp:SerialNumber>
         </wsdp:ThisDevice>
       </wsx:MetadataSection>
       <wsx:MetadataSection Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/ThisModel">
         <wsdp:ThisModel>
           <wsdp:Manufacturer>Contoso, Ltd</wsdp:Manufacturer>
           <wsdp:ManufacturerUrl>http://www.contoso.com</wsdp:ManufacturerUrl>
           <wsdp:ModelName>Contoso Publication Service</wsdp:ModelName>
           <wsdp:ModelNumber>1</wsdp:ModelNumber>
           <wsdp:ModelUrl>http://www.contoso.com</wsdp:ModelUrl>
           <wsdp:PresentationUrl>http://www.contoso.com</wsdp:PresentationUrl>
           <pnpx:DeviceCategory>Computers</pnpx:DeviceCategory>
         </wsdp:ThisModel>
       </wsx:MetadataSection>
       <wsx:MetadataSection
Dialect="http://schemas.xmlsoap.org/ws/2006/02/devprof/Relationship">
         <wsdp:Relationship Type="http://schemas.xmlsoap.org/ws/2006/02/devprof/host">
           <wsdp:Host>
             <wsa:EndpointReference>
               <wsa:Address>urn:uuid:95ce917d-9e37-4099-aee4-db3292041ea6</wsa:Address>
             </wsa:EndpointReference>
             <wsdp:Types>pub:Computer</wsdp:Types>
             <wsdp:ServiceId>urn:uuid:95ce917d-9e37-4099-aee4-db3292041ea6</wsdp:ServiceId>
             <pub:Computer>D-HAMILTON-1/Domain:CONTOSO_DOMAIN1</pub:Computer>
           </wsdp:Host>
           <wsdp:Hosted>
             <wsa:EndpointReference>
               <wsa:Address>urn:uuid:95ce917d-9e37-4099-aee4-db3292041ea6</wsa:Address>
             </wsa:EndpointReference>
             <wsdp:Types>pub:ShellPublishing</wsdp:Types>

<wsdp:ServiceId>fdid:Provider%5CMicrosoft.Base.Publication/Publication%5CExplorer/C692EF15-
7F02-42E9-9FA9-D5F313749B04_S-1-5-21-738895527-329673414-497557404-1000</wsdp:ServiceId>

<pub:Resource>m9CAAwzP41GbgYXZyNXav5WPiEjLwICIl52YvRWaudWPiUFVG1COi8jPNoAPwlmPNoAIgwTdzVmczZU
asV2cEV2cjJXawRXav5mPNoAIgACI88GI15WPic2br1WZuJCIh1jIn92atVmbiAyc9IyQ2kjMFZUM10yNGBjMtQjMFlTL
5YUQ50CR1Y0MxMzN0kjQwQzXT1SMtUTLyETL3MDO4kTN1IzNtMjM5YzNzQTM00CN5cTN1cDNwQTLxADMwICIv4TDKACIg
ACPpxmPNoAIgACIgACPp5TDKACIgACIgACI8AnPcV1clJ3ccd2br1WZuxVQwBHRhRXYcJ1bh1WaudGXNl2Yy92cvZGdcd

10 / 18

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

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

11 / 18

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

jJjYyFzVaVnQBFkNBF0QBFVQBZ3Nyl3KrJnWLdja31GcDFUQBdGaBFUQBFUQnFUQBFUQBFUQBFUQBFUQBFUQBFUQB5mQ3
JWQzdUQ0JUUaFENHFUQBdmRBlkRBhXQBFUQBFUQLdDR21mSCFkQCh0YFZ0RkhmQBBVQnFUQFF0d3sSczBVNh1WercncaF
XQBFUQFpUQBFUQBFUSBFUQBFUQBFUQBFUQBFUQBFUQBFUURFUQIF0dCFkUBV0RBBjQRlVQBFUQXF0ZVFUREFUQBFUQBd2
cQZ3MGVUQJFjYoFzVhVHZHFEOBF0QBFVQBZ3Nyl3KrJnWLdDV11GcDFUQBd2aBFUQBFUQnFUQBFUQBFUQBFUQBFUQBFUQ
BFUQBNlQ3JWQFdUQ0JUUhFENHFkbCFUQBllQBllQR1UQBFUQBF0ZxsSRQBVVBFFVK50aVBlTsZGeBFUQBJUQDFUUBFkd3
IXersmcad1NUhDOvNUQBF0drFUQBFUQBdWQBFUQBFUQBFUQBFUQBFUQBFUQBFkTCFVYB10RBlnQ3JWQNhUQ2J0ZaFUUIF
UQBF0RBlkRBhXQBFUQBFUQLdDR51mQCFEWs1mYrljMkpnQBBVQnFUQFF0d3sSczBVNh1Werc2caFXQBFUQRpUQBFUQBFU
SBFUQBFUQBFUQBFUQBFUQBFUQBF0dWF0aHFUdCFkWBhzRBNjQ3NWQBFUQXFUQXFUREFUQBFUQBlFdQlneUVUQ3V1UDpkV
RNVNY1UQBFUUBdWQBVUQ3dzKxNHUDJWbxsSSQBVcBFUQBFFMFFUQBFUQNFUQBFUQBFUQBFUQBFUQBFUQBFUQBRVQrdUQp
J0ZjFURHFUeCFVYBV1RBpnQBFUQnJUQZN0ZNFUV0cUQBdWMrkEUQdWQBVlSOVkVWpEbmhHNDRlSKVUQBdHSBlUQBJUQ4U
ndLdja31mW0BVe6p2SBFUQBJ0TCFUQBFUQFFUQBFUQBFUQBFUQBlkRBFUQBFUQBZUQwJ0dZFUUIFUMCd2YBV1RBpnQnxU
Q3dUQwJ0ZZFUSIFEaCd2YBtGSBRXQRJWQNhUQBFUQRFUTIF0bCFlWBd3RBNnQ31UQJRUQ1FUQaF0dHF0cCFETBBzQBpXQ
B5UQVRUQ1EUUOFUQBF0YBFUQBFUQBFUQBFUQBFUQBFUQBdmQBFUQNFUQBF0SXFUQBFUQBFUQBNmMiJXMXpVdoJzYshXba
FUQBFUQBFUaDBjcoNFO1kmTktUc4R2ZHljZZ12Z3E3Sw5EMrdGSS5WQhVWRJhzT1sCavFUOh9WRmVXWU5WahNGSvJ1LI1
mS0UXcTFGROpENSBjSn1</pub:Resource>

<pub:Resource1>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

12 / 18

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

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
1EyG4k8XYLqP6fA3oWIAy</pub:Resource1>

<pub:Resource2>WCfU3BofYTXgsXNr3ikT3JZcccgdhbCNEYS/CuMIHi5cKGLCIPDcSg7P3VRyx1VzDOW170YhGD7Pa+
NJDFeuGO73NiEaCtTljULnyYlqtQokSdeYJJJGqs6g6nd8iLGQ0857nG/PMxvCzdirzSa1hxK</pub:Resource2>
           </wsdp:Hosted>
         </wsdp:Relationship>
       </wsx:MetadataSection>
     </wsx:Metadata>
   </soap:Body>
 </soap:Envelope>

From the example data, the following lines make use of the Computer Information structure (section
2.2):

           <wsdp:Types>pub:Computer</wsdp:Types>
 ...
           <pub:Computer>D-HAMILTON-1/Domain:CONTOSO_DOMAIN1</pub:Computer>

Also from the example data, the Resource structure (section 2.3) is demonstrated in the lines that
start with "<pub:Resource>", "<pub:Resource1>", and "<pub:Resource2>".

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 18

4  Security

4.1  Security Considerations for Implementers

Publication Services does not provide any security to protect the data that is being advertised or to
ensure its authenticity. Applications that need to secure their advertised resources typically implement
their security policy on top of Publication Service data structures.

4.2  Index Of Security Fields

None.

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 18

5  Appendix A: Full XML Schema

For ease of implementation, the following is the full XML schema for this protocol.

 <?xml version="1.0" encoding="UTF-8"?>
 <xs:schema
   targetNamespace="http://schemas.microsoft.com/windows/pub/2005/07"
   xmlns:pub="http://schemas.microsoft.com/windows/pub/2005/07"
   xmlns:xs="http://www.w3.org/2001/XMLSchema"
   elementFormDefault="qualified"
   blockDefault="#all" >

   <xs:simpleType name="DiscoveryTypeValues">
     <xs:restriction base="xs:QName">
       <xs:enumeration value="pub:Computer"/>
     </xs:restriction>
   </xs:simpleType>

   <xs:element name="Computer" type="xs:string" minOccurs="0" />

   <xs:element name="Resource" type="xs:string" minOccurs="0" />

 </xs:schema>

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 18

6  Appendix B: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

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

  Windows Server operating system

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

<1> Section 1.4:  On Windows Vista and Windows Server 2008, the maximum metadata size that the
device representation of a computer can be is 32767 octets ([MS-DTYP] section 2.1.5). This is because
the extension that is documented in [MS-DPWSSN] is not supported on these operating systems.

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 18

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

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 18

8  Index
A

Applicability 6

C

Change tracking 17

E

T

Tracking changes 17

V

Vendor-extensible fields 6
Versioning 6

Examples
   Publication Services Data with Listed Resources 10
   Publication Services Data with no Listed Services 9

X

XML schema 15

F

Fields - vendor-extensible 6
Full XML schema 15

G

Glossary 4

I

Implementer - security considerations 14
Informative references 5
Introduction 4

L

Localization 6

N

Normative references 4

O

Overview (synopsis) 5

P

Product behavior 16
Publication Services Data with Listed Resources

example 10

Publication Services Data with no Listed Services

example 9

R

References 4
   informative 5
   normative 4
Relationship to protocols and other structures 6

S

Security
   implementer considerations 14

[MS-PBSD] - v20240423
Publication Services Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 18

