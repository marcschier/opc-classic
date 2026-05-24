[MS-RTPDT]:

Real-Time Transport Protocol (RTP/RTCP): DTMF Digits,
Telephony Tones and Telephony Signals Data Extensions

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

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

1 / 16


Revision Summary

Date

Revision
History

Revision
Class

Comments

4/8/2008

0.1

New

Version 0.1 release

5/16/2008

0.1.1

Editorial

Changed language and formatting in the technical content.

6/20/2008

1.0

Major

Updated and revised the technical content.

7/25/2008

1.0.1

Editorial

Changed language and formatting in the technical content.

8/29/2008

1.0.2

Editorial

Changed language and formatting in the technical content.

10/24/2008  1.0.3

Editorial

Changed language and formatting in the technical content.

12/5/2008

1.1

1/16/2009

1.2

2/27/2009

1.3

Minor

Minor

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

4/10/2009

1.3.1

Editorial

Changed language and formatting in the technical content.

5/22/2009

1.3.2

Editorial

Changed language and formatting in the technical content.

7/2/2009

1.3.3

Editorial

Changed language and formatting in the technical content.

8/14/2009

1.3.4

Editorial

Changed language and formatting in the technical content.

9/25/2009

1.3.5

Editorial

Changed language and formatting in the technical content.

11/6/2009

1.3.6

Editorial

Changed language and formatting in the technical content.

12/18/2009  1.3.7

Editorial

Changed language and formatting in the technical content.

1/29/2010

1.4

Minor

Clarified the meaning of the technical content.

3/12/2010

1.4.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

1.4.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

1.4.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

2 / 16


Date

Revision
History

Revision
Class

Comments

5/6/2011

1.4.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

1.5

Minor

Clarified the meaning of the technical content.

9/23/2011

1.5

12/16/2011  1.5

3/30/2012

1.5

7/12/2012

1.5

10/25/2012  1.5

1/31/2013

1.5

8/8/2013

1.5

11/14/2013  1.5

2/13/2014

1.5

5/15/2014

1.5

6/30/2015

1.5

10/16/2015  1.5

7/14/2016

1.5

6/1/2017

1.5

None

None

None

None

None

None

None

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

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

3 / 16


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
    - [2.2.1 DTMF Telephony Event](#221-dtmf-telephony-event)
- [3 Protocol Details](#3-protocol-details)
  - [3.1 Common Details](#31-common-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Higher-Layer Triggered Events](#314-higher-layer-triggered-events)
    - [3.1.5 Message Processing Events and Sequencing Rules](#315-message-processing-events-and-sequencing-rules)
    - [3.1.6 Timer Events](#316-timer-events)
    - [3.1.7 Other Local Events](#317-other-local-events)
  - [3.2 Receiver Details](#32-receiver-details)
    - [3.2.1 Abstract Data Model](#321-abstract-data-model)
    - [3.2.2 Timers](#322-timers)
    - [3.2.3 Initialization](#323-initialization)
    - [3.2.4 Higher-Layer Triggered Events](#324-higher-layer-triggered-events)
    - [3.2.5 Message Processing Events and Sequencing Rules](#325-message-processing-events-and-sequencing-rules)
    - [3.2.6 Timer Events](#326-timer-events)
    - [3.2.7 Other Local Events](#327-other-local-events)
  - [3.3 Sender Details](#33-sender-details)
    - [3.3.1 Abstract Data Model](#331-abstract-data-model)
    - [3.3.2 Timers](#332-timers)
    - [3.3.3 Initialization](#333-initialization)
    - [3.3.4 Higher-Layer Triggered Events](#334-higher-layer-triggered-events)
    - [3.3.5 Message Processing Events and Sequencing Rules](#335-message-processing-events-and-sequencing-rules)
    - [3.3.6 Timer Events](#336-timer-events)
    - [3.3.7 Other Local Events](#337-other-local-events)
- [4 Protocol Examples](#4-protocol-examples)
- [5 Security](#5-security)
  - [5.1 Security Considerations for Implementers](#51-security-considerations-for-implementers)
  - [5.2 Index of Security Parameters](#52-index-of-security-parameters)
- [6 Appendix A: Product Behavior](#6-appendix-a-product-behavior)
- [7 Change Tracking](#7-change-tracking)
- [8 Index](#8-index)

## 1 Introduction

The Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones, and Telephony Signals
Data Extensions Protocol (RTPDT) is an extension to [RFC4733]. RTPDT describes the payload format
needed to carry DTMF digits, tones, and signals in RTP packets over a network transport.

Any behavior not explicitly defined in this document means the behavior defined in [RFC4733] must
be used.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

dual-tone multi-frequency (DTMF): In telephony systems, a signaling system in which each

digit is associated with two specific frequencies. This system typically is associated with touch-
tone keypads for telephones.

Real-Time Transport Protocol (RTP): A network transport protocol that provides end-to-end

transport functions that are suitable for applications that transmit real-time data, such as audio
and video, as described in [RFC3550].

Session Description Protocol (SDP): A protocol that is used for session announcement, session
invitation, and other forms of multimedia session initiation. For more information see [MS-SDP]
and [RFC3264].

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

[MS-RTPME] Microsoft Corporation, "Real-Time Transport Protocol (RTP/RTCP): Microsoft Extensions".

[MS-RTPRAD] Microsoft Corporation, "Real-Time Transport Protocol (RTP/RTCP): Redundant Audio
Data Extensions".

[MS-SDP] Microsoft Corporation, "Session Description Protocol (SDP) Extensions".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC4733] Schulzrinne, H., and Taylor, T., "RTP Payload for DTMF Digits, Telephony Tones and
Telephony Signals", RFC 4733, December 2006, http://www.ietf.org/rfc/rfc4733.txt

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

5 / 16


#### 1.2.2 Informative References

None.

### 1.3 Overview

The RTP/RTCP: DTMF Digits, Telephony Tones, and Telephony Signals Data Extensions protocol
describes a mechanism for transmission of in-band and out-of-band telephony digits, tones, and
signals. It is an extension to [RFC4733].

The RTPDT protocol is limited to telephony signals using out-of-band transmission. The in-band
transmission of digits and tones is not supported by this protocol.

### 1.4 Relationship to Other Protocols

This protocol relies on RTP as specified in [MS-RTPME] as its transport mechanism. This protocol can
be used to communicate signaling DTMF telephony events between clients and gateways using the
RTP payload.

### 1.5 Prerequisites/Preconditions

This protocol is a payload of RTP; therefore, a valid RTP session has to be established between a
client and a gateway.

Furthermore, because of the dynamic payload typing of the telephony events, out-of-band negotiation
is required to bind the payload type of the RTP payload to the telephony events. This is done using the
Session Description Protocol [MS-SDP].

### 1.6 Applicability Statement

This protocol is applicable wherever telephony digits, tones, or signals need to be sent or consumed
either by remote clients or through gateways.

### 1.7 Versioning and Capability Negotiation

  Supported Transports: This protocol is sent using the RTP transport mechanism [MS-RTPME].



Protocol Versions: This protocol, as a format of an RTP payload, does not provide for versioning
information within the scope of the protocol itself. However, as a part of the RTP payload, any
versioning information about the RTP level will apply.

  Security and Authentication Methods: This specification does not describe any security or

authentication methods. Security and authentication are dependent on the security method,
authentication method, or both methods used by the RTP version 2 protocol.



 Localization: None.

### 1.8 Vendor-Extensible Fields

None.

### 1.9 Standards Assignments

None.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

6 / 16


## 2 Messages

### 2.1 Transport

The RTP/RTCP: DTMF Digits, Telephony Tones and Telephony Signals Data Extensions protocol MUST
be sent using RTP as specified in [MS-RTPME] as its transport. This protocol assumes that a
successful RTP session has been established with valid payload information.

The Session Description Protocol (SDP) [MS-SDP] MUST be used to negotiate the payload type
information.

### 2.2 Message Syntax

The structure and syntax of the RTP/RTCP: DTMF Digits, Telephony Tones and Telephony Signals Data
Extensions protocol is defined in [RFC4733] section 2.3.

#### 2.2.1 DTMF Telephony Event

The DTMF Telephony Event format is described in [RFC4733] section 2.3.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

7 / 16


## 3 Protocol Details

The RTP/RTCP: DTMF Digits, Telephony Tones, and Telephony Signals Extensions protocol conforms
more to the "sender-receiver" paradigm than the classic "client-server" paradigm. More specifically, it
is appropriate to discuss in terms of the receiver of the telephony signals and the sender of the
telephony signals.

This specification covers the common details between the sender and receiver. It then provides the
specifics of the sender and receiver details.

### 3.1 Common Details

In [RFC4733], out-of-band negotiation of telephony signal information is required to establish a
session. During this negotiation, both payload types and the clock rate of the telephony signals are
negotiated as described in [RFC4733] section 2.5.1.1 using SDP for out-of-band negotiation. While
dynamic payload type binding is required, both the sender and receiver of message blocks conforming
to RTPDT MUST fix the telephony signaling information at 8,000 Hertz. Dynamic negotiation of the
clock frequency of the DTMF payload MUST NOT be used.

[RFC4733] allows a "zero" duration in the payload of an RTP packet for state events. Endpoints using
RTPDT MUST NOT send telephony events with a "zero" duration. Telephony events include the state
and nonstate events.

All event duration values MUST NOT exceed the maximum duration expressible in the duration field of
the payload format as described in [RFC4733] section 2.3.5.

Redundancy support as described in [MS-RTPRAD] MUST NOT be used. Integrity for the payload is not
defined by this specification; see [MS-RTPRAD] section 2.2 for payload integrity information.

Multiple payload type binding for different telephony events MUST NOT be used. There MUST be only
one telephony event binding for a payload type. The payload type binding MUST be symmetrical. This
means the receive payload type and send payload type MUST be the same. Asymmetrical payload
type information MUST NOT be used.

RTPDT supports only the telephony event. An in-band telephony tone transmission MUST NOT be
used.

All clock frequencies for DTMF signals, tones, and digits MUST be fixed at 8,000 Hertz.

#### 3.1.1 Abstract Data Model

None.

#### 3.1.2 Timers

None.

#### 3.1.3 Initialization

None.

#### 3.1.4 Higher-Layer Triggered Events

None.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

8 / 16


#### 3.1.5 Message Processing Events and Sequencing Rules

There are no sequence rules or processing event requirements for this protocol above that described
in [RFC4733].

#### 3.1.6 Timer Events

None.

#### 3.1.7 Other Local Events

None.

### 3.2 Receiver Details

Redundant payload support as described in [MS-RTPRAD] MUST NOT be used.

Multiple events per the RTP block MUST NOT be used.

#### 3.2.1 Abstract Data Model

None.

#### 3.2.2 Timers

None.

#### 3.2.3 Initialization

None.

#### 3.2.4 Higher-Layer Triggered Events

None.

#### 3.2.5 Message Processing Events and Sequencing Rules

There are no sequence rules or processing event requirements for this protocol above that described
in [RFC4733].

#### 3.2.6 Timer Events

None.

#### 3.2.7 Other Local Events

None.

### 3.3 Sender Details

Implementation for this protocol MUST NOT generate redundant blocks as described in [MS-RTPRAD].

The sender MUST NOT pack multiple DTMF payloads into a single RTP packet.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

9 / 16


The sender MUST NOT generate a DTMF event whose duration exceeds the maximum expressible
duration as specified in [RFC4733] section 2.3.5.

The sender MUST NOT generate a DTMF event payload with a zero duration.

#### 3.3.1 Abstract Data Model

None.

#### 3.3.2 Timers

None.

#### 3.3.3 Initialization

None.

#### 3.3.4 Higher-Layer Triggered Events

None.

#### 3.3.5 Message Processing Events and Sequencing Rules

There are no sequence rules or processing event requirements for this protocol above that described
in [RFC4733].

#### 3.3.6 Timer Events

None.

#### 3.3.7 Other Local Events

None.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

10 / 16


## 4 Protocol Examples

The following is an example of the SDP negotiation for the DTMF events.

 m=audio 51712 RTP/AVP 114 111 112 115 116 4 8 0 97 101
 ...
 a=rtpmap:114 x-msrta/16000
 a=fmtp:114 bitrate=29000
 a=rtpmap:111 SIREN/16000
 a=fmtp:111 bitrate=16000
 a=rtpmap:112 G7221/16000
 a=fmtp:112 bitrate=24000
 a=rtpmap:115 x-msrta/8000
 a=fmtp:115 bitrate=11800
 a=rtpmap:116 AAL2-G726-32/8000
 a=rtpmap:4 G723/8000
 a=rtpmap:8 PCMA/8000
 a=rtpmap:0 PCMU/8000
 a=rtpmap:97 RED/8000
 a=rtpmap:101 telephone-event/8000
 a=fmtp:101 0-16

The preceding sample uses the default DTMF payload type for Microsoft Office Communicator
(PT=101). This would result in the following payload being generated (and expected).

  0                   1                   2                   3
  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |V=2|P|X|  CC   |M|     PT      |       sequence number         |
 |   | | |       |0|    101      |                               |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |                           timestamp                           |
 |                                                               |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |           synchronization source (SSRC) identifier            |
 |                                                               |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
 |     event     |E R| volume    |          duration             |
 |       1       |1 0|    20     |             1760              |
 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

11 / 16


## 5 Security

### 5.1 Security Considerations for Implementers

There are no additional protocol considerations beyond those described in [RFC4733].

### 5.2 Index of Security Parameters

No security parameters are used by this protocol.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

12 / 16


## 6 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows 2000 operating system

  Windows XP operating system

  Windows Server 2003 operating system

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

13 / 16


## 7 Change Tracking

No table of changes is available. The document is either new or has had no changes since its last
release.

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

14 / 16


## 8 Index
A

Abstract data model
   receiver (section 3.1.1 8, section 3.2.1 9)
   sender (section 3.1.1 8, section 3.3.1 10)
Applicability 6

C

Capability negotiation 6
Change tracking 14

D

Data model - abstract
   receiver (section 3.1.1 8, section 3.2.1 9)
   sender (section 3.1.1 8, section 3.3.1 10)
DTMF Telephony Event format 7
DTMF Telephony Event message 7

E

   syntax
      DTMF Telephony Event format 7
      overview 7
   transport 7

N

Normative references 5

O

Overview (synopsis) 6

P

Parameters - security index 12
Preconditions 6
Prerequisites 6
Product behavior 13
Protocol Details
   overview 8

Examples - overview 11

R

F

Receiver
   abstract data model (section 3.1.1 8, section 3.2.1

Fields - vendor-extensible 6

9)

G

Glossary 5

H

Higher-layer triggered events
   receiver (section 3.1.4 8, section 3.2.4 9)
   sender (section 3.1.4 8, section 3.3.4 10)

I

Implementer - security considerations 12
Index of security parameters 12
Informative references 6
Initialization
   receiver (section 3.1.3 8, section 3.2.3 9)
   sender (section 3.1.3 8, section 3.3.3 10)
Introduction 5

L

Local events
   receiver (section 3.1.7 9, section 3.2.7 9)
   sender (section 3.1.7 9, section 3.3.7 10)

M

Message processing
   receiver (section 3.1.5 9, section 3.2.5 9)
   sender (section 3.1.5 9, section 3.3.5 10)
Messages
   DTMF Telephony Event 7

   higher-layer triggered events (section 3.1.4 8,

section 3.2.4 9)

   initialization (section 3.1.3 8, section 3.2.3 9)
   local events (section 3.1.7 9, section 3.2.7 9)
   message processing (section 3.1.5 9, section 3.2.5

9)

   overview (section 3.1 8, section 3.2 9)
   sequencing rules (section 3.1.5 9, section 3.2.5 9)
   timer events (section 3.1.6 9, section 3.2.6 9)
   timers (section 3.1.2 8, section 3.2.2 9)
References 5
   informative 6
   normative 5
Relationship to other protocols 6

S

Security
   implementer considerations 12
   parameter index 12
Sender
   abstract data model (section 3.1.1 8, section 3.3.1

10)

   higher-layer triggered events (section 3.1.4 8,

section 3.3.4 10)

   initialization (section 3.1.3 8, section 3.3.3 10)
   local events (section 3.1.7 9, section 3.3.7 10)
   message processing (section 3.1.5 9, section 3.3.5

10)

   overview (section 3.1 8, section 3.3 9)
   sequencing rules (section 3.1.5 9, section 3.3.5

10)

   timer events (section 3.1.6 9, section 3.3.6 10)
   timers (section 3.1.2 8, section 3.3.2 10)
Sequencing rules

15 / 16

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017


   receiver (section 3.1.5 9, section 3.2.5 9)
   sender (section 3.1.5 9, section 3.3.5 10)
Standards assignments 6
Syntax
   DTMF Telephony Event format 7
   overview 7

T

Timer events
   receiver (section 3.1.6 9, section 3.2.6 9)
   sender (section 3.1.6 9, section 3.3.6 10)
Timers
   receiver (section 3.1.2 8, section 3.2.2 9)
   sender (section 3.1.2 8, section 3.3.2 10)
Tracking changes 14
Transport 7
Triggered events - higher-layer
   receiver (section 3.1.4 8, section 3.2.4 9)
   sender (section 3.1.4 8, section 3.3.4 10)

V

Vendor-extensible fields 6
Versioning 6

[MS-RTPDT] - v20170601
Real-Time Transport Protocol (RTP/RTCP): DTMF Digits, Telephony Tones and Telephony Signals Data Extensions
Copyright © 2017 Microsoft Corporation
Release: June 1, 2017

16 / 16

