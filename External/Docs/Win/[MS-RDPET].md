[MS-RDPET]:

Remote Desktop Protocol: Telemetry Virtual Channel
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

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

1 / 15

Revision Summary

Date

Revision
History

Revision
Class

Comments

11/14/2013  1.0

2/13/2014

1.0

5/15/2014

1.0

New

None

None

Released new document.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

2.0

Major

Significantly changed the technical content.

10/16/2015  2.0

7/14/2016

2.0

6/1/2017

2.0

9/15/2017

3.0

9/12/2018

4.0

4/7/2021

5.0

6/25/2021

6.0

None

None

None

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

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

2 / 15

Table of Contents

1.1
1.2

1.2.1
1.2.2

1  Introduction ............................................................................................................ 4
Glossary ........................................................................................................... 4
References ........................................................................................................ 4
Normative References ................................................................................... 4
Informative References ................................................................................. 4
Overview .......................................................................................................... 4
Relationship to Other Protocols ............................................................................ 5
Prerequisites/Preconditions ................................................................................. 5
Applicability Statement ....................................................................................... 5
Versioning and Capability Negotiation ................................................................... 5
Vendor-Extensible Fields ..................................................................................... 5
Standards Assignments ....................................................................................... 5

1.3
1.4
1.5
1.6
1.7
1.8
1.9

2  Messages ................................................................................................................. 6
Transport .......................................................................................................... 6
Message Syntax ................................................................................................. 6
RDP_TELEMETRY_PDU ................................................................................... 6

2.1
2.2

2.2.1

3.1

3.1.5.1

3.1.6
3.1.7

3.1.1
3.1.2
3.1.3
3.1.4
3.1.5

3  Protocol Details ....................................................................................................... 8
Server Details .................................................................................................... 8
Abstract Data Model ...................................................................................... 8
Timers ........................................................................................................ 8
Initialization ................................................................................................. 8
Higher-Layer Triggered Events ....................................................................... 8
Processing Events and Sequencing Rules ......................................................... 8
Processing RDP_TELEMETRY_PDU ............................................................. 8
Timer Events ................................................................................................ 8
Other Local Events ........................................................................................ 8
Client Details ..................................................................................................... 8
Abstract Data Model ...................................................................................... 8
Timers ........................................................................................................ 8
Initialization ................................................................................................. 8
Higher-Layer Triggered Events ....................................................................... 9
Processing Events and Sequencing Rules ......................................................... 9
Sending RDP_TELEMETRY_PDU ................................................................. 9
Timer Events ................................................................................................ 9
Other Local Events ........................................................................................ 9

3.2.1
3.2.2
3.2.3
3.2.4
3.2.5

3.2.6
3.2.7

3.2.5.1

3.2

4  Protocol Examples ................................................................................................. 10

5  Security ................................................................................................................. 11
Security Considerations for Implementers ........................................................... 11
Index of Security Parameters ............................................................................ 11

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 12

7  Change Tracking .................................................................................................... 13

8  Index ..................................................................................................................... 14

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

3 / 15

1  Introduction

This document specifies the Remote Desktop Protocol: Telemetry Virtual Channel Extension to the
Remote Desktop Protocol: Basic Connectivity and Graphics Remoting, as specified in [MS-RDPBCGR].
The telemetry protocol defined in section 2.2 is used to send client performance metrics to the server,
thus providing a way to collate statistics about the quality of the RDP experience.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

ANSI character: An 8-bit Windows-1252 character set unit.

little-endian: Multiple-byte values that are byte-ordered with the least significant byte stored in

the memory location with the lowest address.

terminal server: A computer on which terminal services is running.

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

[MS-RDPBCGR] Microsoft Corporation, "Remote Desktop Protocol: Basic Connectivity and Graphics
Remoting".

[MS-RDPEDYC] Microsoft Corporation, "Remote Desktop Protocol: Dynamic Channel Virtual Channel
Extension".

[MS-RDPEGFX] Microsoft Corporation, "Remote Desktop Protocol: Graphics Pipeline Extension".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

1.2.2  Informative References

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

4 / 15

1.3  Overview

The Remote Desktop Protocol: Telemetry Virtual Channel Extension consists of a single client-to-
server PDU (section 2.2.1) sent over an RDP dynamic virtual channel (section 2.1) every time a
connection is established.

1.4  Relationship to Other Protocols

The Remote Desktop Protocol: Telemetry Virtual Channel Extension is embedded in a dynamic virtual
channel transport, as specified in [MS-RDPEDYC] sections 1 to 3.

1.5  Prerequisites/Preconditions

The Remote Desktop Protocol: Telemetry Virtual Channel Extension operates only after the dynamic
virtual channel transport is fully established. If the dynamic virtual channel transport is terminated,
the Remote Desktop Protocol: Telemetry Virtual Channel Extension is also terminated. The protocol is
terminated by closing the underlying virtual channel. For details about closing the dynamic virtual
channel, see [MS-RDPEDYC] section 3.1.5.2.

1.6  Applicability Statement

The Remote Desktop Protocol: Telemetry Virtual Channel Extension is applicable in scenarios where a
mechanism to transmit telemetry data to a terminal server is required.

1.7  Versioning and Capability Negotiation

None.

1.8  Vendor-Extensible Fields

None.

1.9  Standards Assignments

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

5 / 15

2  Messages

2.1  Transport

The Remote Desktop Protocol: Telemetry Virtual Channel Extension is designed to operate over a
dynamic virtual channel, as specified in [MS-RDPEDYC] sections 1 to 3. The dynamic virtual channel
name is the null-terminated ANSI character string "Microsoft::Windows::RDS::Telemetry". The
usage of channel names in the context of opening a dynamic virtual channel is specified in [MS-
RDPEDYC] section 2.2.2.1.

2.2  Message Syntax

The following sections specify the Remote Desktop Protocol: Telemetry Virtual Channel Extension
message syntax.

All multiple-byte fields within a message MUST be marshaled in little-endian byte order, unless
otherwise specified.

2.2.1  RDP_TELEMETRY_PDU

The RDP_TELEMETRY_PDU message is a client-to-server PDU that is used to transmit metrics with
respect to the time it took the client to complete a fully functional connection to the server.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Id

Length

PromptForCredentialsMillis

...

...

...

...

PromptForCredentialsDoneMillis

GraphicsChannelOpenedMillis

FirstGraphicsReceivedMillis

Id (1 byte): An 8-bit unsigned integer that MUST contain the value 0x01.

Length (1 byte): An 8-bit unsigned integer that specifies the length, in bytes, of the PDU. This field

MUST be set to 0x12.

PromptForCredentialsMillis (4 bytes): A 32-bit unsigned integer that specifies the difference, in

milliseconds, between the time when the connection was initiated, and the time when a
credentials prompt dialog was shown to the user. This value MUST be zero if no credentials
prompt dialog was displayed.

PromptForCredentialsDoneMillis (4 bytes): A 32-bit unsigned integer that specifies the difference,

in milliseconds, between the time when the connection was initiated, and the time when
credentials were successfully provided by the user. This value MUST be zero if no credentials
prompt dialog was displayed.

GraphicsChannelOpenedMillis (4 bytes): A 32-bit unsigned integer that specifies the difference, in
milliseconds, between the time when the connection was initiated, and the time when the Remote
Desktop Protocol: Graphics Pipeline Extension dynamic virtual channel ([MS-RDPEGFX] section
2.1) was accepted by the client.

6 / 15

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

FirstGraphicsReceivedMillis (4 bytes): A 32-bit unsigned integer that specifies the difference in

milliseconds, between the time when the connection was initiated, and the time when the first
Desktop Protocol: Graphics Pipeline Extension graphics message ([MS-RDPEGFX] section 2.2) was
received by the client.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

7 / 15

3  Protocol Details

3.1  Server Details

3.1.1  Abstract Data Model

None.

3.1.2  Timers

None.

3.1.3  Initialization

None.

3.1.4  Higher-Layer Triggered Events

None.

3.1.5  Processing Events and Sequencing Rules

3.1.5.1  Processing RDP_TELEMETRY_PDU

The structure and fields of the RDP_TELEMETRY_PDU message are specified in section 2.2.1.
Processing of this message is optional. Upon receiving this message, the server SHOULD log an event
with the connection information contained in the PromptForCredentialsMillis,
PromptForCredentialsDoneMillis, GraphicsChannelOpenedMillis and
FirstGraphicsReceivedMillis fields.

3.1.6  Timer Events

None.

3.1.7  Other Local Events

None.

3.2  Client Details

3.2.1  Abstract Data Model

None.

3.2.2  Timers

None.

3.2.3  Initialization

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

8 / 15

3.2.4  Higher-Layer Triggered Events

None.

3.2.5  Processing Events and Sequencing Rules

3.2.5.1  Sending RDP_TELEMETRY_PDU

The structure and fields of the RDP_TELEMETRY_PDU message are specified in section 2.2.1. The
message fields MUST be populated in accordance with this description. Transmission of the
RDP_TELEMETRY_PDU message to the server is optional.

3.2.6  Timer Events

None.

3.2.7  Other Local Events

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

9 / 15

4  Protocol Examples

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

10 / 15

5  Security

5.1  Security Considerations for Implementers

None.

5.2  Index of Security Parameters

None.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

11 / 15

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows 8.1 operating system

  Windows Server 2012 R2 operating system

  Windows 10 operating system

  Windows Server 2016 operating system

  Windows Server 2019 operating system

  Windows Server 2022 operating system

  Windows 11 operating system

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms SHOULD or SHOULD NOT implies product behavior in accordance with the SHOULD or
SHOULD NOT prescription. Unless otherwise specified, the term MAY implies that the product does not
follow the prescription.

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

12 / 15

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

Revision class

6 Appendix A: Product Behavior  Updated for this version of Windows Client.  Major

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

13 / 15

8  Index
A

Abstract data model
   client 8
   server 8
Applicability 5

C

Capability negotiation 5
Change tracking 13
Client
   abstract data model 8
   higher-layer triggered events 9
   initialization 8
   local events 9
   other local events 9
   RDP_TELEMETRY_PDU message 9
   timer events 9
   timers 8

D

Data model - abstract
   client 8
   server 8

F

Fields - vendor-extensible 5

G

Glossary 4

H

Higher-layer triggered events
   client 9
   server 8

I

Implementer - security considerations 11
Index of security parameters 11
Informative references 4
Initialization
   client 8
   server 8
Introduction 4

M

Messages
   RDP_TELEMETRY_PDU 6
   syntax 6
   transport 6

N

Normative references 4

O

Other local events
   client 9
   server 8
Overview (synopsis) 4

P

Parameters - security index 11
Preconditions 5
Prerequisites 5
Product behavior 12
Protocol examples 10

R

RDP_TELEMETRY_PDU message 6
RDP_TELEMETRY_PDU message - client 9
RDP_TELEMETRY_PDU message - server 8
RDP_TELEMETRY_PDU packet 6
References 4
   informative 4
   normative 4
Relationship to other protocols 5

S

Security
   implementer considerations 11
   parameter index 11
Server
   abstract data model 8
   higher-layer triggered events 8
   initialization 8
   local events 8
   other local events 8
   RDP_TELEMETRY_PDU message 8
   timer events 8
   timers 8
Standards assignments 5

T

Timer events
   client 9
   server 8
Timer events - server 8
Timers
   client 8
   server 8
Tracking changes 13
Transport 6
Triggered events - higher-layer
   client 9
   server 8

V

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

14 / 15

Vendor-extensible fields 5
Versioning 5

[MS-RDPET] - v20210625
Remote Desktop Protocol: Telemetry Virtual Channel Extension
Copyright © 2021 Microsoft Corporation
Release: June 25, 2021

15 / 15

