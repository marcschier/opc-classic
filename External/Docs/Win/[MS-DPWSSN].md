[MS-DPWSSN]:

Devices Profile for Web Services (DPWS): Size Negotiation
Extension

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

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 22


Revision Summary

Date

Revision
History

Revision
Class

Comments

12/5/2008

0.1

Major

Initial Availability.

1/16/2009

0.1.1

Editorial

Changed language and formatting in the technical content.

2/27/2009

0.1.2

Editorial

Changed language and formatting in the technical content.

4/10/2009

0.1.3

Editorial

Changed language and formatting in the technical content.

5/22/2009

0.1.4

Editorial

Changed language and formatting in the technical content.

7/2/2009

1.0

Major

Updated and revised the technical content.

8/14/2009

1.0.1

Editorial

Changed language and formatting in the technical content.

9/25/2009

1.1

Minor

Clarified the meaning of the technical content.

11/6/2009

1.1.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  2.0

1/29/2010

2.1

Major

Minor

Updated and revised the technical content.

Clarified the meaning of the technical content.

3/12/2010

2.1.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

2.1.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

2.1.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

2.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

2.2

Minor

Clarified the meaning of the technical content.

9/23/2011

2.2

12/16/2011  3.0

3/30/2012

3.0

None

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 22


Date

Revision
History

Revision
Class

Comments

technical content.

7/12/2012

3.0

10/25/2012  3.0

1/31/2013

3.0

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

4.0

Major

Updated and revised the technical content.

11/14/2013  4.0

2/13/2014

4.0

5/15/2014

4.0

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

5.0

Major

Significantly changed the technical content.

10/16/2015  5.0

None

No changes to the meaning, language, or formatting of the
technical content.

7/14/2016

5.0

6/1/2017

6.0

9/15/2017

7.0

12/1/2017

7.0

9/12/2018

8.0

4/7/2021

9.0

6/25/2021

10.0

4/23/2024

11.0

None

Major

Major

None

Major

Major

Major

Major

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 22


## Table of Contents

- [1 Introduction](#1-introduction)
  - [1.1 Glossary](#11-glossary)
  - [1.2 References](#12-references)
    - [1.2.1 Normative References](#121-normative-references)
    - [1.2.2 Informative References](#122-informative-references)
  - [1.3 Overview](#13-overview)
  - [1.4 Relationship to Other Protocols](#14-relationship-to-other-protocols)
  - [1.5 Prerequisites/Preconditions](#15-prerequisitespreconditions)
  - [1.6 Applicability Statement](#16-applicability-statement)
  - [1.7 Versioning and Capability Negotiation](#17-versioning-and-capability-negotiation)
  - [1.8 Vendor-Extensible Fields](#18-vendor-extensible-fields)
  - [1.9 Standards Assignments](#19-standards-assignments)
- [2 Messages](#2-messages)
  - [2.1 Transport](#21-transport)
  - [2.2 Common Message Syntax](#22-common-message-syntax)
    - [2.2.1 Namespaces](#221-namespaces)
    - [2.2.2 Messages](#222-messages)
    - [2.2.3 Elements](#223-elements)
      - [2.2.3.1 lms:LargeMetadataSupport](#2231-lmslargemetadatasupport)
    - [2.2.4 Complex Types](#224-complex-types)
    - [2.2.5 Simple Types](#225-simple-types)
    - [2.2.6 Attributes](#226-attributes)
    - [2.2.7 Groups](#227-groups)
    - [2.2.8 Attribute Groups](#228-attribute-groups)
- [3 Protocol Details](#3-protocol-details)
  - [3.1 Server Details](#31-server-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Message Processing Events and Sequencing Rules](#314-message-processing-events-and-sequencing-rules)
    - [3.1.5 Timer Events](#315-timer-events)
    - [3.1.6 Other Local Events](#316-other-local-events)
  - [3.2 Client Details](#32-client-details)
    - [3.2.1 Abstract Data Model](#321-abstract-data-model)
    - [3.2.2 Timers](#322-timers)
    - [3.2.3 Initialization](#323-initialization)
    - [3.2.4 Message Processing Events and Sequencing Rules](#324-message-processing-events-and-sequencing-rules)
    - [3.2.5 Timer Events](#325-timer-events)
    - [3.2.6 Other Local Events](#326-other-local-events)
- [4 Protocol Examples](#4-protocol-examples)
  - [4.1 Request From a Client Using This Protocol Extension](#41-request-from-a-client-using-this-protocol-extension)
  - [4.2 Request From a Client Without This Protocol Extension](#42-request-from-a-client-without-this-protocol-extension)
  - [4.3 Response Message from DPWS](#43-response-message-from-dpws)
  - [4.4 Using the lms:LargeMetadataSupport Element](#44-using-the-lmslargemetadatasupport-element)
- [5 Security](#5-security)
  - [5.1 Security Considerations for Implementers](#51-security-considerations-for-implementers)
  - [5.2 Index of Security Parameters](#52-index-of-security-parameters)
- [6 Appendix A: Full WSDL](#6-appendix-a-full-wsdl)
- [7 Appendix B: Product Behavior](#7-appendix-b-product-behavior)
- [8 Change Tracking](#8-change-tracking)
- [9 Index](#9-index)

## 1 Introduction

This document specifies an extension to the Devices Profile for Web Services (DPWS) to allow the
negotiation of message sizes between a client and a service for a specific message transaction. This
extension to an existing protocol does not define new operations, but instead defines XML Schema
that may be added to existing messages to allow clients and services to properly configure
themselves.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

client: The sending endpoint of a web services request message, and receiver of any resulting web

services response message.

device: The Devices Profile for Web Services (DPWS) term for a special instance of a service that

is discoverable and contains other services with metadata describing those services.

endpoint: In the context of a web service, a network target to which a SOAP message can be

addressed. See [WSADDR].

service: The receiving endpoint of a web services request message, and sender of any resulting

web services response message.

SOAP action: The HTTP request header field used to indicate the intent of the SOAP request, using

a URI value. See [SOAP1.1] section 6.1.1 for more information.

SOAP header: A mechanism for implementing extensions to a SOAP message in a decentralized
manner without prior agreement between the communicating parties. See [SOAP1.2-1/2007]
section 5.2 for more information.

SOAP message: An XML document consisting of a mandatory SOAP envelope, an optional SOAP
header, and a mandatory SOAP body. See [SOAP1.2-1/2007] section 5 for more information.

web service: A unit of application logic that provides data and services to other applications and

can be called by using standard Internet transport protocols such as HTTP, Simple Mail Transfer
Protocol (SMTP), or File Transfer Protocol (FTP). Web services can perform functions that range
from simple requests to complicated business processes.

Web Services Description Language (WSDL): An XML format for describing network services

as a set of endpoints that operate on messages that contain either document-oriented or
procedure-oriented information. The operations and messages are described abstractly and are
bound to a concrete network protocol and message format in order to define an endpoint.
Related concrete endpoints are combined into abstract endpoints, which describe a network
service. WSDL is extensible, which allows the description of endpoints and their messages
regardless of the message formats or network protocols that are used.

XML namespace: A collection of names that is used to identify elements, types, and attributes in
XML documents identified in a URI reference [RFC3986]. A combination of XML namespace and
local name allows XML documents to use elements, types, and attributes that have the same
names but come from different sources. For more information, see [XMLNS-2ED].

XML Schema (XSD): A language that defines the elements, attributes, namespaces, and data

types for XML documents as defined by [XMLSCHEMA1/2] and [XMLSCHEMA2/2] standards. An
XML schema uses XML syntax for its language.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 22


MAY, SHOULD, MUST, SHOULD NOT, MUST NOT: These terms (in all caps) are used as defined
in [RFC2119]. All statements of optional behavior use either MAY, SHOULD, or SHOULD NOT.

### 1.2 References

Links to a document in the Microsoft Open Specifications library point to the correct section in the
most recently published version of the referenced document. However, because individual documents
in the library are not updated at the same time, the section numbers in the documents may not
match. You can confirm the correct section numbering by checking the Errata.

#### 1.2.1 Normative References

We conduct frequent surveys of the normative references to assure their continued availability. If you
have any issue with finding a normative reference, please contact dochelp@microsoft.com. We will
assist you in finding the relevant information.

[DPWS] Chans, S., Conti, D., Schlimmer, J., et al., "Devices Profile for Web Services", February 2006,
http://specs.xmlsoap.org/ws/2006/02/devprof/devicesprofile.pdf

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[SOAP1.2-1/2003] Gudgin, M., Hadley, M., Mendelsohn, N., et al., "SOAP Version 1.2 Part 1:
Messaging Framework", W3C Recommendation, June 2003, http://www.w3.org/TR/2003/REC-soap12-
part1-20030624

[WSAddressing] Box, D., et al., "Web Services Addressing (WS-Addressing)", August 2004,
http://www.w3.org/Submission/ws-addressing/

[WSDL] Christensen, E., Curbera, F., Meredith, G., and Weerawarana, S., "Web Services Description
Language (WSDL) 1.1", W3C Note, March 2001, https://www.w3.org/TR/2001/NOTE-wsdl-20010315

[WSMETA] Ballinger, K., Bissett, B., Box, D., et al., "Web Services Metadata Exchange (WS-
MetadataExchange)", Version 1.1, August 2006, http://specs.xmlsoap.org/ws/2004/09/mex/WS-
MetadataExchange.pdf

[XMLNS] Bray, T., Hollander, D., Layman, A., et al., Eds., "Namespaces in XML 1.0 (Third Edition)",
W3C Recommendation, December 2009, https://www.w3.org/TR/2009/REC-xml-names-20091208/

[XMLSCHEMA1] Thompson, H., Beech, D., Maloney, M., and Mendelsohn, N., Eds., "XML Schema Part
1: Structures", W3C Recommendation, May 2001, https://www.w3.org/TR/2001/REC-xmlschema-1-
20010502/

[XMLSCHEMA2] Biron, P.V., Ed. and Malhotra, A., Ed., "XML Schema Part 2: Datatypes", W3C
Recommendation, May 2001, https://www.w3.org/TR/2001/REC-xmlschema-2-20010502/

#### 1.2.2 Informative References

None.

### 1.3 Overview

Devices Profile for Web Services (DPWS) specifies a device friendly, well-structured messaging model
providing basic functionality such as discovery of an endpoint, metadata for that endpoint, and
request/response messaging. This model is built using core Web Services specifications as building
blocks, and assembled with explanatory text. DPWS identifies the roles of clients, which discover
device endpoints and communicate with devices and services; devices, which can be discoverable

7 / 22

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


service endpoints that host other services, and the services hosted within the device. Additionally,
DPWS defines metadata for both devices and the service endpoints hosted by devices.

This model maps to the requirements of modern home computers in most cases. Home computers are
often set to be discoverable and provide metadata describing themselves and their endpoints (such as
file shares) and resources to clients on a network.

DPWS has one key restriction, in that it recommends clients and services limit their messages to
32,767 octets in length (See [DPWS] Appendix I, R0003, and R0026).

Windows leverages the DPWS model for describing home computers and their metadata. In DPWS
terminology, a home computer is a device, and services and resources available from the home
computer are described by services in metadata.

The metadata provided might be quite large, beyond the size originally envisioned by DPWS for
resource constrained devices. This large size is always due to metadata describing resource or
endpoints hosted within the device. This document describes an extension to DPWS that allows a
DPWS-based client and service to negotiate a larger acceptable message size.

A client that is capable of accepting large message responses can send a request with the large
message support indicator. The service, upon receiving a request, looks for the large message support
indicator and:





If the indicator is present, the service constructs a response which includes all configured
metadata and endpoints available from the home computer.

If the indicator is not present, the service determines what the size of the response will be, and
either:





If the size of the response is less than or equal to the 32,767 octet limit defined in [DPWS],
the response is constructed and sent to the client.

If the size of the response is more than the 32,767 octet limit defined in [DPWS], the response
is changed to contain only metadata describing the computer, and is then sent.

The logic in this protocol is structured in this way to help preserve network behavior with clients that
do not support the large message support indicator. These clients will still see the existence and basic
description of the device, but will not see all of the associated resources and services, as these clients
expect services to manage their metadata to within the 32,767 octet limit.

### 1.4 Relationship to Other Protocols

This extension is built on DPWS and relies on DPWS functionality to work. This extension is designed
to allow DPWS compliant implementations to negotiate larger message sizes, as supported by
standard Web Services; that is, normal Web Services not governed by DPWS do not need such a
protocol extension.

This extension does not define new SOAP messages or patterns, but instead defines XML Schema
that can be added to existing schema extensibility points.

### 1.5 Prerequisites/Preconditions

Both the client and the device in this exchange must be DPWS compliant.

The client must accept SOAP messages larger than 32,767 octets.

Services must be capable of sending as much data as they can prepare.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 22


### 1.6 Applicability Statement

Use of this protocol is appropriate for client implementations if:







The client is a DPWS compliant implementation.

 The client intends to communicate with the DPWS representation of a computer.

The client supports receiving DPWS response messages larger than 32,767 octets in length.

Use of this protocol is appropriate for service implementations if:





The service intends to represent itself as a DPWS compliant computer on the network.

The service supports sending messages larger than 32,767 in length.

### 1.7 Versioning and Capability Negotiation

This protocol extension covers a single version of the extension as specified in section 2.1. There are
no previous versions. There are no special transport requirements or other restrictions placed on
DPWS resulting from use of this extension.

### 1.8 Vendor-Extensible Fields

The schema for this protocol extension does define additional vendor extensions.

### 1.9 Standards Assignments

There are no specific standards assignments for this protocol extension.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 22


## 2 Messages

### 2.1 Transport

Use of this protocol extension requires support for the underlying specifications:

  Clients and devices implementing this protocol extension MUST be compliant with DPWS as

specified in [DPWS].

  DPWS-compliant endpoints MUST be fully compatible with the February 2006 revision of DPWS.

DPWS compliance ensures clients and device support the consistent set of protocol versions.

This protocol extension is limited to these existing messages:

  Devices implementing this protocol extension MUST support receiving the extension in the SOAP

Header of a SOAP Message with the SOAP Action
http://schemas.xmlsoap.org/ws/2004/09/transfer/Get

  Devices implementing this protocol extension MUST support sending a SOAP Message as a

response with a length greater than 32,767 octets.

  Clients implementing this protocol extension MUST support sending the extension in the SOAP

Header of a SOAP Message with the SOAP Action
http://schemas/xmlsoap.org/ws/2004/09/transfer/Get

  Clients implementing this protocol extension MUST support receiving a SOAP Message as a

response with a length greater than 32,767 octets.

Not all SOAP implementations properly support SOAP Header extensions. This support is required for
use of this extension.

This extension does not define a finite boundary for the size of the SOAP Message response.

### 2.2 Common Message Syntax

This section contains common definitions used by this protocol. The syntax of the definitions uses XML
Schema as defined in [XMLSCHEMA1] and [XMLSCHEMA2] and Web Services Description
Language (WSDL) as defined in [WSDL].

#### 2.2.1 Namespaces

This specification defines and references various XML namespaces using the mechanisms specified in
[XMLNS]. Although this specification associates a specific XML namespace prefix for each XML
namespace that is used, the choice of any particular XML namespace prefix is implementation-specific
and not significant for interoperability.

Prefix  Namespace URI

 s

http://www.w3.org/2003/05/soap-envelope

Reference

[SOAP1.2-1/2003]

wsa

http://schemas.xmlsoap.org/ws/2004/08/addressing

[WSAddressing]

wsx

http://schemas.xmlsoap.org/ws/2004/09/mex

[WSMETA]

lms

http://schemas.microsoft.com/windows/dpws/LargeMetadataSupport/2007/08  This document

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 22


#### 2.2.2 Messages

This protocol does not define any messages.

#### 2.2.3 Elements

The following table summarizes the set of common XML Schema element definitions defined by this
specification. XML Schema element definitions that are specific to a particular operation are described
with the operation.

Element

Description

lms:LargeMetadataSupport

Indicator for support of large metadata messages.

##### 2.2.3.1 lms:LargeMetadataSupport

The lms:LargeMetadataSupport element is used to indicate client side support for large metadata
responses. The normative form for this element is as follows.

 <lms:LargeMetadataSupport/>

The element is not extensible and does not support attributes.

The schema for this element is as follows.

 <xs:element name='LargeMetadataSupport'>
   <xs:complexType>
   <xs:sequence />
   </xs:complexType>
 </xs:element>

This element MUST NOT be used outside of the top level of the SOAP header in a SOAP Message. If
the element is used outside of the top level of the SOAP header where it is defined as semantically
meaningful, then it will be ignored. Standard exceptions to this apply (for example, if the header is
inserted in a location that violates the schema).This element MUST NOT be used in SOAP headers
where the wsa:Action is not http://schemas.xmlsoap.org/ws/2004/09/transfer/Get. If the element is
used in a SOAP header when the action is not http://schemas.xmlsoap.org/ws/2004/09/transfer/Get
and the service understands the action, then the element will be ignored.

#### 2.2.4 Complex Types

This specification does not define any common XML Schema complex type definitions.

#### 2.2.5 Simple Types

This specification does not define any common XML Schema simple type definitions.

#### 2.2.6 Attributes

This specification does not define any common XML Schema attribute definitions.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 22


#### 2.2.7 Groups

This specification does not define any common XML Schema group definitions.

#### 2.2.8 Attribute Groups

This specification does not define any common XML Schema attribute group definitions.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 22


## 3 Protocol Details

This protocol extension governs the behavior of a device's use of the data model specified in [DPWS]
depending on whether a client supports large metadata responses.

### 3.1 Server Details

#### 3.1.1 Abstract Data Model

This protocol extension does not describe a separate data model.

Devices that support this protocol extension MUST support the data model described in [DPWS]
section 5.

Data specified in [DPWS] section 5.2 is sent in part or in full depending on client support for large
metadata responses (as indicated by the presence of the <lms:LargeMetadataSupport> element as
defined in section 2.2.3.1) and the size of the metadata response.

#### 3.1.2 Timers

This protocol extension requires no new timers.

#### 3.1.3 Initialization

This protocol extension requires no new initialization.

#### 3.1.4 Message Processing Events and Sequencing Rules

If the data specified in [DPWS] section 5.2 is sent with a full response exceeding 32,767 octets in
length and the client supports large metadata responses, the full response

  MUST be compliant with [DPWS] section 3, except R0003 and R0026,

  MUST be compliant with [DPWS] section 5, and

  MUST be sent in its entirety.

If the data specified in [DPWS] section 5.2 is sent in part due to a full response exceeding 32,767
octets in length and the client not supporting large metadata responses, the
wsdp:Relationship/wsdp:Host element and its children MUST be included in the response, and the
wsdp:Relationship/wsdp:Hosted element MAY be included, to the extent that its inclusion does not
cause the response to exceed 32,767 octets in length.

This specification does not define any new Web Services Description Language (WSDL)
operations.

#### 3.1.5 Timer Events

This protocol extension defines no timer events.

#### 3.1.6 Other Local Events

This protocol extension defines no local events and is not subject to any new local events.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 22


### 3.2 Client Details

#### 3.2.1 Abstract Data Model

This protocol extension does not describe a separate data model.

Clients that support this protocol extension MUST support the data model described in [DPWS] section
5.

The client side of this protocol extension is simply a pass-through. Calls made by the higher-layer
protocol or application are passed directly to the transport, and the results returned by the transport
are passed directly back to the higher-layer protocol or application.

Clients supporting large metadata responses MUST include the <lms:LargerMetadataSupport>
element as defined in section 2.2.3.1.

#### 3.2.2 Timers

This protocol extension requires no new timers.

#### 3.2.3 Initialization

This protocol extension requires no new initialization.

#### 3.2.4 Message Processing Events and Sequencing Rules

This protocol extension describes no separate message processing events and sequencing rules.

This specification does not define any new WSDL operations.

#### 3.2.5 Timer Events

This protocol extension defines no timer events.

#### 3.2.6 Other Local Events

This protocol extension defines no local events and is not subject to any new local events.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 22


## 4 Protocol Examples

### 4.1 Request From a Client Using This Protocol Extension

The following is an abbreviated example request from a client using this protocol extension:

 <s:Envelope …>
   <s:Header …>
     <wsa:Action>
       http://schemas.xmlsoap.org/ws/2004/09/transfer/Get
     </wsa:Action>
     <wsa:MessageID>xs:anyURI</wsa:MessageID>
     <wsa:To>xs:anyURI</wsa:To>
     <lms:LargeMetadataSupport/>
   </s:Header>
   <s:Body/>
 </s:Envelope>

### 4.2 Request From a Client Without This Protocol Extension

The following is an abbreviated example request from a client without this protocol extension:

 <s:Envelope …>
   <s:Header …>
     <wsa:Action>
       http://schemas.xmlsoap.org/ws/2004/09/transfer/Get
     </wsa:Action>
     <wsa:MessageID>xs:anyURI</wsa:MessageID>
     <wsa:To>xs:anyURI</wsa:To>
   </s:Header>
   <s:Body/>
 </s:Envelope>

### 4.3 Response Message from DPWS

The following is an abbreviated example response message from DPWS (see [DPWS] section 5.1).

 <s:Envelope …>
   <s:Header …>
     <wsa:Action>
       http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse
     </wsa:Action>
     <wsa:RelatesTo>xs:RequestURI</wsa:RelatesTo>
     <wsa:MessageID>xs:anyURI</wsa:MessageID>
     <wsa:To>http://schemas.xmlsoap.org/ws/2004/08/adressing/role/anonymous</wsa:To>
   </s:Header>
   <s:Body …>
     <wsx:Metadata>
      ...
     </wsx:Metadata>
   </s:Body>
 </s:Envelope>

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 22


### 4.4 Using the lms:LargeMetadataSupport Element

A usage example of the lms:LargeMetadataSupport element is as follows.

 <s:Envelope …>
   <s:Header …>
     <wsa:Action>
       http://schemas.xmlsoap.org/ws/2004/09/transfer/Get
     </wsa:Action>
     <wsa:MessageID>xs:anyURI</wsa:MessageID>
     <wsa:To>xs:anyURI</wsa:To>
     <lms:LargeMetadataSupport/>
   </s:Header>
   <s:Body ...>
      ...
   </s:Body>
 </s:Envelope>

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 22


## 5 Security

### 5.1 Security Considerations for Implementers

Clients indicating they support large metadata response can use this in a DDoS attack on services .
The nature of the attack is to have a large number of distributed clients issue requests indicating they
support large responses. Assuming TCP is in use, the clients would then slow their processing of data
and cause TCP backoffs to slow data transmission to 100 bytes/second. The large number of clients
would result in a significant number of active connections and potentially a large amount of in memory
state on the service. It is not clear that this attack is significantly more damaging than having the
clients aggressively download, which would instead exhaust bandwidth but would have similar external
consequences.

Similarly, clients indicating they support large metadata responses can end up receiving a large
response at a very slow rate, and might be impacted by the same in-memory state concerns as
services.

### 5.2 Index of Security Parameters

There are no security parameters for this protocol extension.

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 22


## 6 Appendix A: Full WSDL

This protocol extension does not define a WSDL. It extends the WSDL defined and referenced by
[DPWS]. The following XSD contains a definition of the lms:LargeMetadataSupport element (section
2.2.3.1).

 <?xml version="1.0" encoding="UTF-8"?>
 <xs:schema
 targetNamespace="http://schemas.microsoft.com/windows/dpws/LargeMetadataSupport/2007/08"
 xmlns:lms="http://schemas.microsoft.com/windows/dpws/LargeMetadataSupport/2007/08"
xmlns:xs="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified">

     <xs:element name='LargeMetadataSupport'>
         <xs:complexType>
         <xs:sequence />
         </xs:complexType>
     </xs:element>
 </xs:schema>

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 22


## 7 Appendix B: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

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

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 22


## 8 Change Tracking

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

7 Appendix B: Product
Behavior

Added Windows Server 2025 to the list of applicable
products.

Revision
class

Major

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 22


## 9 Index
A

Abstract data model
   client 14
   server 13
Abstract data model - server 13
Applicability 9
Attribute groups 12
Attributes 11

C

Capability negotiation 9
Change tracking 20
Client
   abstract data model 14
   initialization 14
   local events 14
   message processing 14
   sequencing rules 14
   timer events 14
   timers 14
Common message syntax
   attribute groups 12
   attributes 11
   complex types 11
   elements 11
   groups 12
   messages 11
   namespaces 10
   overview 10
   simple types 11
Complex types 11

D

Data model - abstract 13
   client 14
   server 13

E

Elements
   lms:LargeMetadataSupport 11
Events
   local - client 14
   local - server 13
   timer - client 14
   timer - server 13
Examples
   lms:LargeMetadataSupport element 15
   request from a client using this protocol extension

15

   request from a client without this protocol

extension 15

   response message - from [DPWS] 15

F

Fields - vendor-extensible 9
Full WSDL 18

G

Glossary 6
Groups 12

I

Implementer - security considerations 17
Index of security parameters 17
Informative references 7
Initialization
   client 14
   server 13
Initialization - server 13
Introduction 6

L

lms:LargeMetadataSupport element 11
lms:LargeMetadataSupport element example 15
Local events
   client 14
   server 13
Local events - server 13

M

Message processing
   client 14
   server 13
Message processing - server 13
Messages
   attribute groups 12
   attributes 11
   complex types 11
   elements 11
   enumerated 11
   groups 12
   lms:LargeMetadataSupport element 11
   namespaces 10
   simple types 11
   syntax 10
      attribute groups 12
      attributes 11
      complex types 11
      elements 11
      groups 12
      messages 11
      namespaces 10
      overview 10
      simple types 11
   transport 10

N

Namespaces 10
Normative references 7

O

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

21 / 22


Overview (synopsis) 7

P

Parameters - security index 17
Preconditions 8
Prerequisites 8
Product behavior 19
Protocol Details
   overview 13

R

References 7
   informative 7
   normative 7
Relationship to other protocols 8
Request from a client using this protocol extension

example 15

Request from a client without this protocol extension

example 15

Response message example 15

Timers - server 13
Tracking changes 20
Transport 10
Types
   complex 11
   simple 11

V

Vendor-extensible fields 9
Versioning 9

W

WSDL 18

S

Security
   implementer considerations 17
   parameter index 17
Sequencing rules
   client 14
   server 13
Sequencing rules - server 13
Server
   abstract data model 13
   initialization 13
   local events 13
   message processing 13
   sequencing rules 13
   timer events 13
   timers 13
Simple types 11
Standards assignments 9
Syntax
   attribute groups 12
   attributes 11
   complex types 11
   elements
      lms:LargeMetadataSupport 11
      overview 11
   groups 12
   messages 11
   messages - overview 10
   namespaces 10
   overview 10
   simple types 11

T

Timer events
   client 14
   server 13
Timer events - server 13
Timers
   client 14
   server 13

[MS-DPWSSN] - v20240423
Devices Profile for Web Services (DPWS): Size Negotiation Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

22 / 22

