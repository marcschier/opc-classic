[MS-SSDP]:

SSDP: Networked Home Entertainment Devices (NHED)
Extensions

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

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

1 / 15


Revision Summary

Date

Revision
History

Revision
Class

Comments

7/20/2007

0.1

9/28/2007

1.0

Major

Major

MCPP Milestone 5 Initial Availability

Updated and revised the technical content.

10/23/2007  1.0.1

Editorial

Changed language and formatting in the technical content.

11/30/2007  1.0.2

Editorial

Changed language and formatting in the technical content.

1/25/2008

1.0.3

Editorial

Changed language and formatting in the technical content.

3/14/2008

1.0.4

Editorial

Changed language and formatting in the technical content.

5/16/2008

1.0.5

Editorial

Changed language and formatting in the technical content.

6/20/2008

1.0.6

Editorial

Changed language and formatting in the technical content.

7/25/2008

1.0.7

Editorial

Changed language and formatting in the technical content.

8/29/2008

1.1

Minor

Clarified the meaning of the technical content.

10/24/2008  1.1.1

Editorial

Changed language and formatting in the technical content.

12/5/2008

1.2

Minor

Clarified the meaning of the technical content.

1/16/2009

1.2.1

Editorial

Changed language and formatting in the technical content.

2/27/2009

1.2.2

Editorial

Changed language and formatting in the technical content.

4/10/2009

1.2.3

Editorial

Changed language and formatting in the technical content.

5/22/2009

1.2.4

Editorial

Changed language and formatting in the technical content.

7/2/2009

1.2.5

Editorial

Changed language and formatting in the technical content.

8/14/2009

1.2.6

Editorial

Changed language and formatting in the technical content.

9/25/2009

1.3

11/6/2009

1.4

Minor

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

12/18/2009  1.4.1

Editorial

Changed language and formatting in the technical content.

1/29/2010

1.4.2

Editorial

Changed language and formatting in the technical content.

3/12/2010

1.4.3

Editorial

Changed language and formatting in the technical content.

4/23/2010

1.4.4

Editorial

Changed language and formatting in the technical content.

6/4/2010

1.4.5

Editorial

Changed language and formatting in the technical content.

7/16/2010

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

1.4.5

11/19/2010  1.4.5

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

2 / 15


Date

Revision
History

Revision
Class

Comments

technical content.

1/7/2011

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

1.4.5

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

1.5

Minor

Clarified the meaning of the technical content.

9/23/2011

1.5

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  2.0

Major

Updated and revised the technical content.

3/30/2012

2.0

7/12/2012

2.0

10/25/2012  2.0

1/31/2013

2.0

None

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

3.0

Major

Updated and revised the technical content.

11/14/2013  3.0

2/13/2014

3.0

5/15/2014

3.0

6/30/2015

3.0

10/16/2015  3.0

7/14/2016

3.0

6/1/2017

3.0

None

None

None

None

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

3 / 15


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
  - [2.2 Message Syntax](#22-message-syntax)
- [3 Protocol Details](#3-protocol-details)
  - [3.1 Device Details](#31-device-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Higher-Layer Triggered Events](#314-higher-layer-triggered-events)
    - [3.1.5 Message Processing Events and Sequencing Rules](#315-message-processing-events-and-sequencing-rules)
    - [3.1.6 Timer Events](#316-timer-events)
    - [3.1.7 Other Local Events](#317-other-local-events)
  - [3.2 Control Point Details](#32-control-point-details)
    - [3.2.1 Abstract Data Model](#321-abstract-data-model)
    - [3.2.2 Timers](#322-timers)
    - [3.2.3 Initialization](#323-initialization)
    - [3.2.4 Higher-Layer Triggered Events](#324-higher-layer-triggered-events)
    - [3.2.5 Message Processing Events and Sequencing Rules](#325-message-processing-events-and-sequencing-rules)
    - [3.2.6 Timer Events](#326-timer-events)
    - [3.2.7 Other Local Events](#327-other-local-events)
- [4 Protocol Examples](#4-protocol-examples)
- [5 Security](#5-security)
  - [5.1 Security Considerations for Implementers](#51-security-considerations-for-implementers)
  - [5.2 Index of Security Parameters](#52-index-of-security-parameters)
- [6 Appendix A: Product Behavior](#6-appendix-a-product-behavior)
- [7 Change Tracking](#7-change-tracking)
- [8 Index](#8-index)

## 1 Introduction

The SSDP: Networked Home Entertainment Devices (NHED) Extensions are a set of extensions to the
Simple Service Discovery Protocol (SSDP), as specified in [UPNPARCH1], and are used to detect
devices on a home network. In this specification, the SSDP: Networked Home Entertainment Devices
(NHED) Extensions are referred to as SSDPE.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

Uniform Resource Identifier (URI): A string that identifies a resource. The URI is an addressing
mechanism defined in Internet Engineering Task Force (IETF) Uniform Resource Identifier (URI):
Generic Syntax [RFC3986].

Universal Plug and Play (UPnP): A set of computer network protocols, published by the UPnP

Forum [UPnP], that allow devices to connect seamlessly and that simplify the implementation of
networks in home (data sharing, communications, and entertainment) and corporate
environments. UPnP achieves this by defining and publishing UPnP device control protocols built
upon open, Internet-based communication standards.

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

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC3986] Berners-Lee, T., Fielding, R., and Masinter, L., "Uniform Resource Identifier (URI): Generic
Syntax", STD 66, RFC 3986, January 2005, https://www.rfc-editor.org/info/rfc3986

[UPNPARCH1] UPnP Forum, "UPnP Device Architecture 1.0", October 2008,
http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf

[UPnP] UPnP Forum, "Standards", http://upnp.org/sdcps-and-certification/standards/sdcps/

#### 1.2.2 Informative References

[SSDP1] Goland, Yaron Y., Cai, T., Leach, P., Gu, Y., and Albright, S., "Simple Service Discovery
Protocol (SSDP)", 1999, http://tools.ietf.org/html/draft-cai-ssdp-v1-03

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

5 / 15


### 1.3 Overview

SSDP (as specified in [UPNPARCH1]) is used to detect Universal Plug and Play (as specified in
[UPnP]) devices on a network. SSDP is maintained by the UPnP Forum and is published by the UPnP
Implementers Corporation.

The SSDP: Networked Home Entertainment Devices (NHED) Extensions, also known as SSDPE,
provide a mechanism for a control point to discover a device on the network without requiring the
device to implement a complete SSDP stack. SSDP is simplified by removing the requirement for a
description document (substituted with device-specific information in an Alternate Location (AL)
header in each announcement) and by removing the need for a multicast listener (substituted with
frequent periodic announcements).

### 1.4 Relationship to Other Protocols

The SSDP: Networked Home Entertainment Devices (NHED) Extensions depend on protocols described
in section 1.1 of [UPNPARCH1], specifically:

  HTTP (Multicast over UDP) (HTTPMU)

  Universal Datagram Protocol (UDP)



Internet Protocol (IP)

### 1.5 Prerequisites/Preconditions

The SSDP: Networked Home Entertainment Devices (NHED) Extensions have no additional
prerequisites/preconditions beyond what is required for SSDP, as specified in [UPNPARCH1].

### 1.6 Applicability Statement

The SSDP: Networked Home Entertainment Devices (NHED) Extensions provide a mechanism for a
control point to discover a device on the network without requiring the device to implement a
complete SSDP stack.

### 1.7 Versioning and Capability Negotiation

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

### 1.8 Vendor-Extensible Fields

The AL header (as specified in [SSDP1]) in the ssdp:alive message contains a bracketed list of URIs
(as specified in [RFC3986]). The vendor can extend that list with any URIs that comply with the rules
specified in [RFC3986].

### 1.9 Standards Assignments

There are no standards assignments other than what is specified in [UPNPARCH1].

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

6 / 15


## 2 Messages

### 2.1 Transport

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

### 2.2 Message Syntax

The SSDP: Networked Home Entertainment Devices (NHED) Extensions MUST follow the Simple
Service Discovery Protocol discovery advertisement messages syntax, as specified in [UPNPARCH1]
section 1.1, with the following exceptions:





The LOCATION header MUST contain the single character "*".

The AL header (as specified by [SSDP1]) is required and MUST contain a list of URIs ([RFC3986]),
with each URI framed by the characters "<" and ">".

The SSDP: Networked Home Entertainment Devices (NHED) Extensions SHOULD NOT implement the
Simple Service Discovery Protocol discovery search messages syntax as specified in [UPNPARCH1]
section 1.2.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

7 / 15


## 3 Protocol Details

### 3.1 Device Details

SSDP (as specified in [UPNPARCH1]) is used for device discovery between control points and devices.
On the device, specific messages are multicast.

#### 3.1.1 Abstract Data Model

No abstract data model is required.

#### 3.1.2 Timers

Because the SSDP: Networked Home Entertainment Devices (NHED) Extensions are implemented such
that the traditional SSDP search does not exist on the device, the device SHOULD send ssdp:alive
messages on a periodic basis that is more frequent than the Simple Service Discovery Protocol default.

#### 3.1.3 Initialization

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.1.4 Higher-Layer Triggered Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.1.5 Message Processing Events and Sequencing Rules

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.1.6 Timer Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.1.7 Other Local Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

### 3.2 Control Point Details

 SSDP is used for device discovery between control points (as specified in [UPNPARCH1]) and devices
(as specified in [UPNPARCH1]). The control point listens for multicast messages from the device.

#### 3.2.1 Abstract Data Model

No abstract data model is required.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

8 / 15


#### 3.2.2 Timers

No timers are required.

#### 3.2.3 Initialization

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.2.4 Higher-Layer Triggered Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.2.5 Message Processing Events and Sequencing Rules

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.2.6 Timer Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

#### 3.2.7 Other Local Events

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

9 / 15


## 4 Protocol Examples

A new device is set up and plugged in to the home network for the first time. When it is turned on, the
device first sends over port 1900 a UDP multicast message of ssdp:byebye, and then, immediately
afterward, a message of ssdp:alive. The ssdp:byebye message is sent just before the ssdp:alive
message to ensure cancellation of any previously sent ssdp:alive message.

The following examples could be used for a particular Microsoft Xbox 360 device.

 NOTIFY * HTTP/1.1
 HOST:239.255.255.250:1900
 NT:urn:schemas-microsoft-com:nhed:presence:1
 NTS:ssdp:byebye
 LOCATION:*
 USN:uuid:00000000-0000-0000-0200-00125A8A0960::urn:schemas-microsoft-
 com:nhed:presence:1

 NOTIFY * HTTP/1.1
 HOST:239.255.255.250:1900
 NT:urn:schemas-microsoft-com:nhed:presence:1
 NTS:ssdp:alive
 LOCATION:*
 CACHE-CONTROL:max-age=4
 AL:<urn:schemas-microsoft-com:nhed:attributes?type=X02&firmwarever=
 5766.0&udn=uuid:10000000-0000-0000-0200-00125A8A0960>
 USN:uuid:00000000-0000-0000-0200-00125A8A0960::urn:schemas-microsoft-
 com:nhed:presence:1
 SERVER:dashboard/1.0 UpnP/1.0 xbox/2.0

Note  In these examples, the NT, USN, and AL header values are placeholders to be replaced by
application-specific values.

The device continues to resend these messages every 5 seconds until a control point that is listening
on the network for this ssdp:alive message responds with the appropriate behavior. The contract
between the control point and the device for the expected control point response behavior is outside
the scope of this protocol. However, as an example, a device could listen on an agreed-upon TCP port,
and when the control point connects to this TCP port (in response to having received a device
ssdp:alive message) the device could assume it has been discovered and cease sending the SSDP
messages.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

10 / 15


## 5 Security

### 5.1 Security Considerations for Implementers

The SSDP: Networked Home Entertainment Devices (NHED) Extensions do not specify anything
beyond what is specified by [UPNPARCH1].

### 5.2 Index of Security Parameters

There are no security parameters for the SSDP: Networked Home Entertainment Devices (NHED)
Extensions.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

11 / 15


## 6 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows Vista operating system

  Windows 7 operating system

  Windows 8 operating system

  Windows 8.1 operating system

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

12 / 15


## 7 Change Tracking

No table of changes is available. The document is either new or has had no changes since its last
release.

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

13 / 15


## 8 Index
A

Abstract data model
   control point 8
   device 8
Applicability 6

C

Capability negotiation 6
Change tracking 13
Control point
   abstract data model 8
   higher-layer triggered events 9
   initialization 9
   local events 9
   message processing 9
   overview 8
   sequencing rules 9
   timer events 9
   timers 9

D

Data model - abstract
   control point 8
   device 8
Device
   abstract data model 8
   higher-layer triggered events 8
   initialization 8
   local events 8
   message processing 8
   overview 8
   sequencing rules 8
   timer events 8
   timers 8

E

Examples - overview 10

F

Fields - vendor-extensible 6

G

Glossary 5

H

Higher-layer triggered events
   control point 9
   device 8

I

Implementer - security considerations 11
Index of security parameters 11
Informative references 5
Initialization

   control point 9
   device 8
Introduction 5

L

Local events
   control point 9
   device 8

M

Message processing
   control point 9
   device 8
Messages
   syntax 7
   transport 7

N

Normative references 5

O

Overview (synopsis) 6

P

Parameters - security index 11
Preconditions 6
Prerequisites 6
Product behavior 12

R

References 5
   informative 5
   normative 5
Relationship to other protocols 6

S

Security
   implementer considerations 11
   parameter index 11
Sequencing rules
   control point 9
   device 8
Standards assignments 6
Syntax 7

T

Timer events
   control point 9
   device 8
Timers
   control point 9
   device 8
Tracking changes 13
Transport 7

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

14 / 15


Triggered events - higher-layer
   control point 9
   device 8

V

Vendor-extensible fields 6
Versioning 6

[MS-SSDP] - v20170601
SSDP: Networked Home Entertainment Devices (NHED) Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

15 / 15

