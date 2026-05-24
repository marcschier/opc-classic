[MS-WDSMSI]:

Windows Deployment Services Multicast Session Initiation
Protocol

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

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 29

Revision Summary

Date

Revision
History

Revision
Class

Comments

4/10/2009

0.1

Major

First Release.

5/22/2009

0.1.1

Editorial

Changed language and formatting in the technical content.

7/2/2009

0.1.2

Editorial

Changed language and formatting in the technical content.

8/14/2009

0.1.3

Editorial

Changed language and formatting in the technical content.

9/25/2009

0.2

Minor

Clarified the meaning of the technical content.

11/6/2009

0.2.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  0.2.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

0.2.3

Editorial

Changed language and formatting in the technical content.

3/12/2010

0.2.4

Editorial

Changed language and formatting in the technical content.

4/23/2010

0.2.5

Editorial

Changed language and formatting in the technical content.

6/4/2010

0.2.6

Editorial

Changed language and formatting in the technical content.

7/16/2010

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

0.2.6

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

0.3

Minor

Clarified the meaning of the technical content.

9/23/2011

0.3

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  1.0

Major

Updated and revised the technical content.

3/30/2012

1.0

7/12/2012

1.1

10/25/2012  1.2

None

Minor

Minor

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 29

Date

Revision
History

Revision
Class

Comments

1/31/2013

1.3

8/8/2013

2.0

11/14/2013  2.0

Minor

Major

None

Clarified the meaning of the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

2/13/2014

2.1

Minor

Clarified the meaning of the technical content.

5/15/2014

2.1

None

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

4/23/2024

7.0

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

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 29

Table of Contents

1.1
1.2

1.3
1.4

1.2.1
1.2.2

1  Introduction ............................................................................................................ 5
Glossary ........................................................................................................... 5
References ........................................................................................................ 6
Normative References ................................................................................... 6
Informative References ................................................................................. 6
Overview .......................................................................................................... 6
Relationship to Other Protocols ............................................................................ 7
Using the WDS Control Protocol ...................................................................... 7
Using UDP ................................................................................................... 8
Prerequisites/Preconditions ................................................................................. 9
Applicability Statement ....................................................................................... 9
Versioning and Capability Negotiation ................................................................... 9
Vendor-Extensible Fields ................................................................................... 10
Standards Assignments ..................................................................................... 10

1.5
1.6
1.7
1.8
1.9

1.4.1
1.4.2

2.2.1

2.1
2.2

2  Messages ............................................................................................................... 11
Transport ........................................................................................................ 11
Message Syntax ............................................................................................... 11
WDSMC_OP_INITIATE OpCode ..................................................................... 11
Determine Client Security Mode .............................................................. 13
Determine Server Security Mode ............................................................. 14
Session Initiation Packets for UDP ................................................................. 15
Multicast Session Initiation Request Packet .............................................. 16
Multicast Session Initiation Reply Packet .................................................. 17
Multicast Session Initiation Error Packet ................................................... 17

2.2.2.1
2.2.2.2
2.2.2.3

2.2.1.1
2.2.1.2

2.2.2

3.1

3.1.1

3.1.1.1
3.1.1.2
3.1.1.3

3  Protocol Details ..................................................................................................... 19
Server Details .................................................................................................. 19
Abstract Data Model .................................................................................... 19
Registered Content Provider Configuration ............................................... 19
Registered Multicast Namespaces Configuration ........................................ 19
WDS Server Configuration ...................................................................... 19
Timers ...................................................................................................... 20
Initialization ............................................................................................... 20
Higher-Layer Triggered Events ..................................................................... 20
Message Processing Events and Sequencing Rules .......................................... 20
Supported Security Modes ...................................................................... 20
Pre-OS Client .................................................................................. 21
WDSMC_OP_INITIATE ........................................................................... 21
Over UDP ............................................................................................. 22
Timer Events .............................................................................................. 23
Other Local Events ...................................................................................... 23

3.1.2
3.1.3
3.1.4
3.1.5

3.1.5.2
3.1.5.3

3.1.6
3.1.7

3.1.5.1.1

3.1.5.1

4  Protocol Examples ................................................................................................. 24
WDS Multicast Session Initiation Protocol over WDS Control Protocol ...................... 24

4.1

5  Security ................................................................................................................. 25
Security Considerations for Implementers ........................................................... 25
Index of Security Parameters ............................................................................ 25

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 26

7  Change Tracking .................................................................................................... 27

8  Index ..................................................................................................................... 28

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 29

1  Introduction

The Multicast Session Initiation Protocol specifies communication between a client and a Windows
Deployment Services server to initiate a Multicast Session. It is a client/server protocol which specifies
two mechanisms for the client to request initiation of a Multicast Session from the server.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

Client Security Mode: Specifies the mechanism used by the client to add validation information to

each packet sent by the client to the server using the WDS Multicast Transport Protocol.

Configuration String: A Unicode string that is used by multicast namespace to instruct the

content provider about the content required to be exposed.

content: Identified by a unique name under a given multicast namespace. The content metadata
cannot change during the lifetime of a multicast session, and is required to allow random access
to the data.

Content Metadata: Specifies an opaque binary data that is associated with the content.

Content Provider: A module that is loaded by the server and is responsible for providing access to

the data for the content under a multicast namespace.

Endpoint GUID: Set of relevant services provided by a Service Provider are grouped together and

as a whole identified by a unique Endpoint GUID.

globally unique identifier (GUID): A term used interchangeably with universally unique

identifier (UUID) in Microsoft protocol technical documents (TDs). Interchanging the usage of
these terms does not imply or require a specific algorithm or mechanism to generate the value.
Specifically, the use of this term does not imply or require that the algorithms described in
[RFC4122] or [C706] have to be used for generating the GUID. See also universally unique
identifier (UUID).

multicast: The ability of a transport protocol, such as User Datagram Protocol (UDP), to deliver

messages to a group of recipients simultaneously without duplication of message unless the link
to recipients is split.

multicast address: A recipient that subscribes to the network address to receive packets sent

using Multicast UDP. In a multicast address scenario, a packet is sent once by the sender and is
delivered to all subscribers.

multicast namespace: Hosts multiple content that are available to clients using multicast

sessions. Identification by a unique name is required. All content under a multicast namespace
is available for transmission over multicast transmission.

Multicast Session: A session setup by the server to transmit content to multiple clients using the

WDS Multicast Application Protocol and the WDS Multicast Transport Protocol.

RSA Key: A public/private key pair generated using RSA algorithm. The private key is used to sign
the packets for the WDS Multicast Transport Protocol and the public key is used by clients to
validate the signatures.

security identifier (SID): An identifier for security principals that is used to identify an account
or a group. Conceptually, the SID is composed of an account authority portion (typically a

5 / 29

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

domain) and a smaller integer representing an identity relative to the account authority, termed
the relative identifier (RID). The SID format is specified in [MS-DTYP] section 2.4.2; a string
representation of SIDs is specified in [MS-DTYP] section 2.4.2 and [MS-AZOD] section 1.1.1.2.

Security Mode: Specifies the mechanism used by server and clients to validate the packets sent

using the WDS Multicast Transport Protocol.

Server Security Mode: Specifies the mechanism used by server to add validation information to

each packet sent by server to clients using the WDS Multicast Transport Protocol.

WDS server: A Windows Deployment Services (WDS) server that communicates with clients by

using the WDS OS Deployment Protocol to aid in deployment of an OS image on a client
machine. Clients also communicate to a WDS server to request initiation/setup of multicast
sessions for content available in multicast namespace on server.A WDS server provides an
extensible mechanism to allow service providers to provide services to clients.

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

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[MS-ERREF] Microsoft Corporation, "Windows Error Codes".

[MS-WDSC] Microsoft Corporation, "Windows Deployment Services Control Protocol".

[MS-WDSMT] Microsoft Corporation, "Windows Deployment Services Multicast Transport Protocol".

[RFC2104] Krawczyk, H., Bellare, M., and Canetti, R., "HMAC: Keyed-Hashing for Message
Authentication", RFC 2104, February 1997, https://www.rfc-editor.org/info/rfc2104

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

1.2.2  Informative References

None.

1.3  Overview

A typical interaction between client and server involves the following:

1.  The client has already obtained the following information.

  Name or IP address of server.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 29

  Name of multicast namespace.

  Name of the content in multicast namespace.

2.  The client uses the Multicast Session Initiation Protocol to request content in the multicast

namespace be set up for delivery using multicast transmission.

3.  On receiving the request, the server sets up the multicast session for the specified content in

the multicast namespace and sends the details of the multicast session to the client.

1.4  Relationship to Other Protocols

The Multicast Session Initiation Protocol specifies two mechanisms for clients to request initiation of a
multicast session. One uses the WDS Control Protocol, and the other uses UDP. Both are described in
the following subsections.

1.4.1  Using the WDS Control Protocol

The Multicast Session Initiation Protocol uses the WDS Control Protocol to send a request to the
server, which allows the user identity to be transported to the server with the request. The following
diagram illustrates the relationship of the Multicast Session Initiation Protocol and how it relates to the
WDS Control Protocol.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 29

Figure 1: Protocol relationships

1.4.2  Using UDP

The clients can use UDP as a transport to send requests to a server<1>, but this limits all requests to
being unauthenticated. The following diagram illustrates the relationship of the Multicast Session
Initiation Protocol and the UDP Protocol.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 29

Figure 2: WDSMSI relationship to UDP

1.5  Prerequisites/Preconditions

The WDS Multicast Session Initiation Protocol assumes the client has obtained the following:

1.  Name or IP address of the server.

2.  Name of the multicast namespace.

3.  Name of the content in the multicast namespace.

4.  Authentication requirements for the content.

If the server requires a user identity to control access to the content, then the client MUST use the
WDS Multicast Session Initiation Protocol over the WDS Control Protocol; otherwise the client MAY use
the Multicast Session Initiation Protocol over UDP.

The content MAY have associated content metadata, which clients MUST understand in order to
consume the content. The server is responsible for transporting the content metadata from server to
clients but treats it as opaque binary data.

An agreement MUST exist between the multicast namespace and the content provider on the format
of the configuration string, which is used by the multicast namespace to instruct the content
provider to expose the appropriate content for the multicast namespace.

1.6  Applicability Statement

This protocol is applicable when a client is required to download content from a server using
multicast session, and uses the WDS Multicast Session Initiation Protocol to request that the server
set up the content for delivery over the multicast session.

1.7  Versioning and Capability Negotiation

This document covers versioning issues in the following areas.

  Supported Transports: This protocol can be implemented on top of the WDS Control Protocol and

the User Datagram Protocol (UDP).

  Security and Authentication Methods: The WDS Multicast Session Initiation Protocol over the WDS
Control Protocol supports authentication. The security requirements are specified in section 2.2.



Localization: The protocol does not support localization, and as such acts as a pass-through for all
strings.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 29

  Capability Negotiation: The protocol does explicit capability negotiations for certain Endpoint

GUID and OpCodes as specified in the following section.

Capability

Section

WDSMC_OP_INITIATE  Section 2.2.1

1.8  Vendor-Extensible Fields

The protocol does not provide any vendor-extensible fields.

This protocol uses Win32 error codes as defined in [MS-ERREF] section 2.2. Vendors SHOULD reuse
those values with their indicated meaning. Choosing any other value runs the risk of a collision in the
future.

1.9  Standards Assignments

Parameter

Value

Reference

Multicast Session Initiation Endpoint
GUID

6f13a317-3687-4b54-81a5-
504daa9062fa

 [MS-WDSC] (section 2.1.2)

Multicast Session Initiation UDP Port

5041

None.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 29

2  Messages

2.1  Transport

The Multicast Session Initiation Protocol over the WDS Control Protocol MUST use the byte-order as
specified in [MS-WDSC].

The Multicast Session Initiation Protocol over UDP MUST use network-byte-order unless noted
otherwise.

2.2  Message Syntax

WDS servers MUST support the following OpCodes under Multicast Session Initiation Endpoint
GUID, as specified in [MS-WDSC] (section 2.1.2).

OpCode

Authentication
Requirements

Description

WDSMC_OP_INITIATE

Authenticated

0x00000006

This OpCode is used by clients to request that the server set
up specified content for delivery using multicast session.

The WDS server MAY support incoming requests on UDP Port as specified in section 1.9.<2>

2.2.1  WDSMC_OP_INITIATE OpCode

The client uses this OpCode under the Multicast Session Initiation Endpoint GUID to request setup of
content for delivery using multicast session.

The request packet MUST include the following variables.

Namespace (WDSCPL_VAR_WSTRING as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set
to the name of the multicast namespace.

Content (WDSCPL_VAR_WSTRING as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set to
the name of content under the multicast namespace.

Client (WDSCPL_VAR_WSTRING as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set to the
machine name of the client. The maximum character length for the machine name MUST NOT
exceed 16 characters including the null character.

The request packet MAY include the following variables.

Cap (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): Specifies a bitwise value

of the following flags:<3>

Flag

Description

WDSMC_CLIENT_CAP_CHECKSUM

0x00000001

Specifies that clients support checksum handling for packets using
the WDS Multicast Transport Protocol.

WDSMC_CLIENT_CAP_IPV6

0x00000002

Specifies that clients support the IPv6 protocol and is capable of
receiving multicast packets using the IPv6 protocol for the WDS
Multicast Transport Protocol.

WDSMC_CLIENT_CAP_BOOT_DEVICE

Specifies that the client is operating in a pre-OS environment.

0x00000004

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 29

The reply packet MUST include the following variables.

TpMcAddress.Port (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be

set to the UDP port being used by multicast session to send packets using multicast.

TpMcAddress.Address (WDSCPL_VAR_BLOB as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST
be set to the multicast IP address being used by the multicast session and MUST be specified in
network byte order.

For a multicast session using an IPv4 multicast address, this variable MUST be set to 4 bytes
specifying the IPv4 multicast address.

For a multicast session using an IPv6 multicast address, this variable MUST be set to 16 bytes
specifying the IPv6 multicast address.

TpUniAddress.Port (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be

set to the same value as specified for the TpMcAddress.Port variable.

TpUniAddress.Address (WDSCPL_VAR_BLOB as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST
be set to the IP address of the network interface card being used by multicast session on the
server and MUST be specified in network byte order.

For a multicast session using an IPv4 address, this variable MUST be set to 4 bytes specifying the
IPv4 address.

For a multicast session using an IPv6 address, this variable MUST be set to 16 bytes specifying the
IPv6 address.

SessionId (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set to a

numeric value that uniquely identifies the multicast session on the server.

ContentSize (WDSCPL_VAR_ULONG64 as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set

to the total size of the content, in bytes.

BlockSize (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): content is divided
into equal-sized blocks of data by WDS Multicast Transport Protocol. This variable specifies the
size of each block in bytes. The last block of data for content MAY be smaller in size because the
total size of content MAY NOT be equally divisible by the BlockSize.

TotalBlocks (WDSCPL_VAR_ULONG64 as specified in [MS-WDSC] section 2.2.1.3.2.1): MUST be set

to the total number of blocks that the content has been divided into.

ContentMetadata (WDSCPL_VAR_BLOB as specified in [MS-WDSC] section 2.2.1.3.2.1): Specifies

any metadata associated with the content. If the content does not have any associated metadata,
then this variable MUST be set to zero length.

The reply packet MAY include the following variables.

SymKey (WDSCPL_VAR_BLOB as specified in [MS-WDSC] section 2.2.1.3.2.1): Specifies the shared
cryptographic key to use to compute and/or validate the hash of the packets using the Hash
Message Authentication Code (HMAC) algorithm ([RFC2104]) specified by the HMACAlgId
variable for WDS Multicast Transport Protocol [MS-WDSMT].

When this variable is specified, HashAlgId and HMACAlgId variables MUST be specified as well.

Section 2.2.1.1 specifies the rules to determine the client security mode and section 2.2.1.2
specifies the rules to determine the server security mode.

SignKey (WDSCPL_VAR_BLOB as specified in [MS-WDSC] section 2.2.1.3.2.1): Specifies the public

RSA key to use to validate the signature of packets sent by server.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 29

Section 2.2.1.1 specifies the rules to determine the client security mode and section 2.2.1.2
specifies the rules to determine the server security mode.

HashAlgId (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): Specifies the

Hashing algorithm to use to compute the hash for packets.

HMACAlgId (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): HMAC algorithm
to use to compute the HMAC hash for the packets.

SecMode (WDSCPL_VAR_ULONG as specified in [MS-WDSC] section 2.2.1.3.2.1): The value for

variable has the following format:<4>

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Client Security Mode

Server Security Mode

When this variable is specified in the reply packet, it controls the mechanism used by the client
and server to validate packets before further processing. The values for client security mode and
server security mode MUST be set to one of the following for each.

Security Mode

Description

WDSMCTP_SEC_NONE

0x0000

WDSMCTP_SEC_HASH

0x0001

Specifies that packets MUST not include any security/validation
information.

Specifies that packets MUST use the specified hash algorithm for the
packet.

WDSMCTP_SEC_SIGN

Specifies that packet MUST be signed.

0x0002

WDSMCTP_SEC_CHECKSUM

Specifies that packet MUST include the checksum for the packet.

0x0003

UserSid (WDSCPL_VAR_BLOB): MUST be set to the security identifier, as specified in [MS-DTYP]

section 2.4.2, of the user.<5>

2.2.1.1  Determine Client Security Mode

The following flowchart specifies the logic to be followed by the client to determine the client security
mode for the WDS Multicast Transport Protocol, as specified in [MS-WDSMT].

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 29

Figure 3: Client security mode flowchart

2.2.1.2  Determine Server Security Mode

The following flowchart specifies the logic to be followed by the client to determine the server
security mode for WDS Multicast Transport Protocol, as specified in [MS-WDSMT].

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 29

Figure 4: Server security mode flowchart

2.2.2  Session Initiation Packets for UDP

The WDS Multicast Session Initiation Protocol over UDP uses a single packet format. The packet
format supports options and depending on the type of packet, a different set of options are specified
in the packet.

The format for all packets is as follows.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

OpCode

OptionsCount

OptionId

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 29

...

OptionLength

OptionValue (variable)

...

OpCode (1 byte): MUST be set to the type of packet as specified below.

OpCode

Meaning

WDSMCSE_OP_REQUEST

0x01

Multicast Session Initiation Request Packet. Section 2.2.2.1 specifies options
that MUST be specified.

WDSMCSE_OP_REPLY

0x02

Multicast Session Initiation Reply Packet. Section 2.2.2.2 specifies options that
MUST be specified.

OptionsCount (2 bytes): MUST be set to the number of Options specified in the packet.

The set of these three fields, OptionId, OptionLength, and OptionValue, are used to specify
value for each option.

OptionId (2 bytes): Specifies a numeric value that uniquely identifies the option.

OptionLength (2 bytes): Specifies the length, in bytes, for the value of the option.

OptionValue (variable): Specifies the value for the option. The length for this field is specified by

the OptionLength field.

2.2.2.1  Multicast Session Initiation Request Packet

This packet is sent by the client to server on the UDP port specified in section 1.9. This packet is used
to request the server to set up specified content for delivery using multicast session.

The format for this packet is specified in section 2.2.2. The request packet MUST include the Options
specified below.

Option Id

Description

WDSMCSE_OPT_NAMESPACE

Specifies the name for multicast namespace.

0x0601

The value MUST be a Unicode string with the individual characters of the string
specified in little-endian format. The last character of the Unicode string MUST
be a null character.

WDSMCSE_OPT_CONTENT

Specifies the name for content under a multicast namespace.

0x0602

The value MUST be a Unicode string with individual characters of the string
specified in little-endian format. The last character for the value MUST be a
null character.

WDSMCSE_OPT_MAC_ADDRESS

0x050C

Specifies the MAC address of the network interface card being used by the
client to communicate with server.

The request packet MAY specify the following Options:<6>

Option Id

Description

WDSMCSE_OPT_IPV6_CAPABLE

0x010D

A single-byte value that MUST be set to 1 if the client is capable of receiving
multicast packets using the IPv6 protocol; otherwise the value MUST be set to
zero.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 29

If a request packet does not specify WDSMCSE_OPT_IPV6_CAPABLE option, the server MUST assume
that the client is not capable of receiving IPv6 multicast packets.

2.2.2.2  Multicast Session Initiation Reply Packet

The server sends this packet in response to Multicast Session Initiation Request Packet when the
requested content has been set up for delivery using multicast session. The reply packet MUST be
sent using the UDP port specified in section 1.9 by the server. If an error occurs that prevents the
server from setting up the content for delivery using multicast session, then the server sends the reply
packet as specified in section 2.2.2.3.

The format for this packet is specified in section 2.2.2. The packet MUST include the options specified
below.

Option Id

Description

WDSMCSE_OPT_MULTICAST_ADDR

Specifies the multicast IP address for the multicast session.

0x0503

The value MUST specify a 4-byte address for an IPv4 multicast address,
and a 16-byte address for an IPv6 multicast address.

WDSMCSE_OPT_SERVER_ADDR

0x0504

Specifies the IP address of the network interface card being used by the
multicast session.

The value MUST specify a 4-byte address for an IPv4 address and a 16-
byte address for an IPv6 address.

WDSMCSE_OPT_MULTICAST_PORT

0x0205

MUST be set to the UDP port being used by the multicast session to
transmit packets to the multicast address specified by the
WDSMCSE_OPT_MULTICAST_ADDR field.

The value MUST be a 2-byte unsigned numeric value.

WDSMCSE_OPT_SERVER_PORT

0x0206

MUST be set to the same value as the
WDSMCSE_OPT_MULTICAST_PORT field.

WDSMCSE_OPT_CONTENT_SIZE

Specifies the total size, in bytes, for the content.

0x0407

The value is a 64-bit unsigned numeric value.

WDSMCSE_OPT_BLOCK_SIZE

0x0309

Content is divided into equal blocks of data by the WDS Multicast Transport
Protocol. This variable specifies the size of each block in bytes. The last
block of data for content MAY be smaller in size because the size of content
MAY NOT be fully divisible by the WDSMCSE_OPT_BLOCK_SIZE field.

The value is a 32-bit unsigned numeric value.

WDSMCSE_OPT_TOTAL_BLOCKS

0x0408

MUST be set to the total number of blocks the content has been divided
into.

The value is a 64-bit unsigned numeric value.

WDSMCSE_OPT_SESSION_ID

0x030A

MUST be set to a numeric value that uniquely identifies the multicast
session on the server.

The value is a 32-bit unsigned numeric value.

2.2.2.3  Multicast Session Initiation Error Packet

This packet is sent by the server in response to the Multicast Session Initiation Request Packet if an
error occurs that prevents the server from setting up the Multicast Session for the requested
content.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 29

The format for this packet is specified in section 2.2.2. The request packet MUST include the options
specified below.

Option Id

Description

WDSMCSE_OPT_ERROR

0x030B

MUST be set to the Win32 error code that prevented the server from setting up the
Multicast Session.

The value MUST be a 32 bit numeric value.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 29

3  Protocol Details

3.1  Server Details

This section specifies the WDS Deployment Protocol behavior for WDS server.

3.1.1  Abstract Data Model

This section describes a conceptual model of possible data organization that an implementation
maintains to participate in this protocol. The described organization is provided to facilitate the
explanation of how the protocol behaves. This document does not mandate that implementations
adhere to this model as long as their external behavior is consistent with that described in this
document.

Registered content providers: Specifies a list of content providers registered with the WDS server.
Each registered content provider has configuration data associated with it, as specified in section
3.1.1.1.

Registered multicast namespaces: Specifies the collection of multicast namespaces that are
registered on the server and are available for clients. Each Registered multicast namespace has
configuration data associated with it, as specified in section 3.1.1.2.

WDS serverconfiguration: Configuration information for the server, in persistent storage, in the form
of (name, value) pairs. The list of configuration parameters are specified in section 3.1.1.3.

3.1.1.1  Registered Content Provider Configuration

The following properties are stored for each registered content provider in persistent storage.

Name: Specifies a unique name for the content provider.

ModulePath: Specifies the path to the module for the content provider.

AllowUnauthenticated: A Boolean value which, when set to TRUE (0x00000001), specifies that the

content provider allows unauthenticated clients to request content using the WDS Multicast
Session Initiation Protocol over UDP.

3.1.1.2  Registered Multicast Namespaces Configuration

The following properties are stored in persistent storage for each registered multicast namespace.

Name: Specifies a unique name for the multicast namespace.

ContentProvider: Specifies the name for the content provider that will be providing information

and data for contents available using the multicast namespace.

ConfigurationString: Specifies a configuration string that instructs the content provider about the

types of content to make available for the multicast namespace.

3.1.1.3  WDS Server Configuration

The following properties are stored for WDS server configuration.

AllowUDP: A Boolean value that, when set to TRUE (0x00000001), specifies that the server MUST
listen for WDS Multicast Session Initiation Protocol packets on the UDP port (section 1.9).

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 29

ServerSecurityMode: A numeric value that specifies the server security mode (section 2.2.1).

Section 3.1.5.1 specifies the list of supported security mode.

ClientSecurityMode: A numeric value that specifies the client security mode (section 2.2.1).

Section 3.1.5.1 specifies the list of supported security modes.

SignKey: An RSA public/private key pair that is used by the server if the ServerSecurityMode field

is set to WDSMCTP_SEC_SIGN.

HashKey: A cryptographic key that is used by the server when ServerSecurityMode is set to

WDSMCTP_SEC_HASH or WDSMCTP_SEC_SIGN. This key is also used by the client when the
ClientSecurityMode field is set to WDSMCTP_SEC_HASH.

HashAlgId: A numeric value that specifies the Cryptographic Hash algorithm to use if security mode

is set to WDSMCTP_SEC_HASH or WDSMCTP_SEC_SIGN.

HMACAlgId: A numeric value that specifies the Cryptographic HMAC algorithm to use if security

mode is set to WDSMCTP_SEC_HASH.

3.1.2  Timers

None.

3.1.3  Initialization

On initialization, the WDS server MUST register a Multicast Session Initiation Endpoint GUID as
specified in section 1.9. If AllowUDP (section 3.1.1.2) is set to TRUE (0x00000001), the server MUST
also listen for incoming packets on the UDP port specified in section 1.9.

The server MUST read information for all registered content providers, along with the associated
registered content provider configuration (section 3.1.1.1) for each, and MUST initialize each content
provider.

In order to initialize each multicast namespace, the server MUST follow the steps below.

1.  Read the collection of registered multicast namespaces and associated registered multicast

namespace configuration (section 3.1.1.2) for each.

2.  Validate that the ContentProvider (section 3.1.1.2) exists and is initialized.

3.  Provide the ConfigurationString (section 3.1.1.2) to the content provider so appropriate content

can be made available for the multicast namespace.

The server MUST also validate that the security modes specified by the ServerSecurityMode field
and the ClientSecurityMode field are valid as specified in section 3.1.5.1.

3.1.4  Higher-Layer Triggered Events

None.

3.1.5  Message Processing Events and Sequencing Rules

3.1.5.1  Supported Security Modes

The server MUST support the following combination of security modes.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 29

Pre-OS Client  Server Security Mode

Client Security Mode

Yes

No

No

No

No

WDSMCTP_SEC_CHECKSUM  WDSMCTP_SEC_CHECKSUM

WDSMCTP_SEC_SIGN

WDSMCTP_SEC_HASH

WDSMCTP_SEC_HASH

WDSMCTP_SEC_HASH

WDSMCTP_SEC_CHECKSUM  WDSMCTP_SEC_CHECKSUM

WDSMCTP_SEC_NONE

WDSMCTP_SEC_NONE

3.1.5.1.1 Pre-OS Client

The server MUST assume that the client is running in a pre-OS environment if a request packet is
received using the UDP port.

For requests received using the WDS Control Protocol, as specified in [MS-WDSC], the server MUST
assume that the client is running in pre-OS environment if the Cap variable specifies the
WDSMC_CLIENT_CAP_BOOT_DEVICE flag.

3.1.5.2  WDSMC_OP_INITIATE

This OpCode is used by clients to request set up of content under a multicast namespace for
delivery using multicast session.

The server MUST follow the steps in the following section for setting up the content for delivery using
multicast session.

1.  MUST match the value of Namespace variable to the Name property (section 3.1.1.1) of

registered multicast namespaces.

2.  MUST query the content provider identified by ContentProvider property (section 3.1.1.2) for the

content specified by the Content variable in the request packet to validate that client has access
to content.

If the request packet includes the Cap variable, and it specifies WDSMC_CLIENT_CAP_IPV6, then if
the server is capable of setting up multicast session for IPv6, the server MUST setup the multicast
session using IPv6; otherwise, the server MUST set up the multicast session using IPv4.

If the client is running in a pre-OS environment (section 3.1.5.1.1), the server MUST set both the
server and client security modes to WDSMCTP_SEC_CHECKSUM for the multicast session.

If the client is not running in a pre-OS environment, the server MUST set the security modes as
specified for the ServerSecurityMode and ClientSecurityMode fields (section 3.1.1.3) for the
multicast session. The server MUST provide both the SignKey and HashKey fields to the multicast
session if required by security modes.

The server MUST query the multicast session and add the following variables to the reply packet.

TpMcAddress.Port,TpMcAddress.Address, TpUniAddress.Port, TpUniAddress.Address,
SessionId, BlockSize, TotalBlocks, ContentSize

The server MUST query the associated content provider for any metadata associated with the content,
and add it to the reply packet using the ContentMetadata variable. If no metadata exists for the
content, the server MUST not add the ContentMetadata variable to the reply packet.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

21 / 29

The server MUST validate that when server or client security mode is set to
WDSMCTP_SEC_CHECKSUM, the request packet MUST include the Cap variable. Cap MUST specify
the WDSMC_CLIENT_CAP_CHECKSUM.

If the server security mode being used by the multicast session is WDSMCTP_SEC_SIGN, then the
server MUST:





add the SignKey field (section 3.1.1.3) to the reply packet, using the value of the SignKey
variable

add the SymKey field (section 3.1.1.3) to the reply packet, using the value of the SymKey
variable.

If the security mode (either client or server) being used by multicast session is WDSMCTP_SEC_HASH,
then the server MUST add the following to the reply packet:







the HashKey field (section 3.1.1.3) to the reply packet using the SymKey variable.

the HashAlgId field (section 3.1.1.3) to the reply packet using the HashAlgId variable.

the HMACAlgId field (section 3.1.1.3) to the reply packet using the HMACAlgId variable.

The server MUST construct the security mode (section 2.2.1) and add it to the reply packet using the
SecMode variable.

The server MUST get the user security identifier and add it to the reply packet using the UserSid
variable.

3.1.5.3  Over UDP

The Multicast Session Initiation Request Packet is received by the server on the UDP port specified in
section 1.9.

The server MUST validate that the request packet specifies WDSMCSE_OPT_NAMESPACE,
WDSMCSE_OPT_CONTENT and WDSMCSE_OPT_MAC_ADDRESS options.

The server:

MUST match the value of the Namespace variable to the Name property (section 3.1.1.1) of
registered multicast namespaces.

MUST verify that AllowUnauthenticated (section 3.1.1.1) is set to TRUE (0x00000001).

MUST query the content provider identified by the ContentProvider property (section 3.1.1.2) for the
content specified by the Content variable in the request packet, in order to validate that the client is
allowed access to content.

If the request packet specifies WDSMCSE_OPT_IPV6_CAPABLE, and it is set to 1, and the server is
capable of setting up multicast session for IPv6, then the server MUST set up the multicast session
using IPv6; otherwise the server MUST set up the multicast session using IPv4.

The server MUST set the server and client security modes to WDSMCTP_SEC_CHECKSUM.

The server MUST query the multicast session and add the following options to the reply packet:

WDSMCSE_OPT_MULTICAST_ADDR, WDSMCSE_OPT_MULTICAST_PORT,
WDSMCSE_OPT_SERVER_ADDR, WDSMCSE_OPT_SERVER_PORT, WDSMCSE_OPT_CONTENT_SIZE,
WDSMCSE_OPT_TOTAL_BLOCKS, WDSMCSE_OPT_BLOCK_SIZE, WDSMCSE_OPT_SESSION_ID.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

22 / 29

3.1.6  Timer Events

When using the WDS Multicast Session Initiation Protocol over UDP, the client MUST wait for 1 second
for the reply from the server before sending the request packet again.

3.1.7  Other Local Events

None.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

23 / 29

4  Protocol Examples

4.1  WDS Multicast Session Initiation Protocol over WDS Control Protocol

The Request Packet includes following variables.

Namespace (WDSCPL_VAR_WSTRING): "WDS:default/install.wim/1"

Content (WDSCPL_VAR_WSTRING): "install.wim"

Client (WDSCPL_VAR_WSTRING): "TestMachine"

Cap (WDSCPL_VAR_ULONG): 0x00000003 (WDSMC_CLIENT_CAP_CHECKSUM |
WDSMC_CLIENT_CAP_IPV6)

The reply packet includes the following variables.

TpMcAddress.Port (WDSCPL_VAR_ULONG): 0x0000FA84

TpMcAddress.Address (WDSCPL_VAR_BLOB): EF00006F

TpUniAddress.Port (WDSCPL_VAR_ULONG): 0x0000FA84

TpUniAddress.Address (WDSCPL_VAR_BLOB): C0A800C8

ContentSize (WDSCPL_VAR_ULONG64): 0x00000000EF8B56EC

TotalBlocks (WDSCPL_VAR_ULONG64): 0x6FB00

BlockSize (WDSCPL_VAR_ULONG): 0x00002251

SessionId (WDSCPL_VAR_ULONG): 0x6D19EE7E

SymKey (WDSCPL_VAR_BLOB):
0802000003660000180000002F15F82AE0683EF79E6D62A70BDC519D2A3246E0FDB354E9

SymKey (WDSCPL_VAR_BLOB):
0802000003660000180000002F15F82AE0683EF79E6D62A70BDC519D2A3246E0FDB354E9

UserSid (WDSCPL_VAR_BLOB):
0105000000000005150000006BE79ECE8F2C9599DC2F39DCF4010000

HMACAlgId (WDSCPL_VAR_ULONG): 0x00008009

HashAlgId (WDSCPL_VAR_ULONG): 0x0000800C

SecMode (WDSCPL_VAR_ULONG): 0x00010001

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

24 / 29

5  Security

5.1  Security Considerations for Implementers

None.

5.2  Index of Security Parameters

Security Parameter

Section

Endpoint GUID, OpCodes, and Security  Section 2.2

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

25 / 29

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows Server 2008 operating system

  Windows Server 2008 R2 operating system

  Windows Server 2012 operating system

  Windows Server 2012 R2 operating system

  Windows Server 2016 operating system

  Windows Server operating system

  Windows Server 2019 operating system

  Windows Server 2022 operating system

  Windows Server 2025 operating system

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 1.4.2:  Windows Server 2008 R2 operating system supports the WDS Multicast Session
Initiation Protocol using UDP.

<2> Section 2.2: Windows Server 2008 R2, Windows Server 2012, and Windows Server 2012 R2
listen for incoming requests on the UDP port.

<3> Section 2.2.1: Windows 7 operating system, Windows 8 operating system, and Windows 8.1
operating system send the Cap variable.

<4> Section 2.2.1: Windows Server 2008 R2, Windows Server 2012, and Windows Server 2012 R2
send the SecMode variable.

<5> Section 2.2.1: Windows Server 2008 R2, Windows Server 2012, and Windows Server 2012 R2
send the UserSid variable in the reply packet.

<6> Section 2.2.2.1: Windows clients do not send this Option.

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

26 / 29

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

6 Appendix A: Product
Behavior

Added Windows Server 2025 to the list of applicable
products.

Revision
class

Major

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

27 / 29

8  Index
A

Abstract data model
   overview 19
   registered content provider configuration 19
   registered multicast namespaces configuration 19
   server 19
   WDS Server configuration 19
Applicability 9

C

Capability negotiation 9
Change tracking 27
Client - determining security mode 13

D

Data model - abstract
   overview 19
   registered content provider configuration 19
   registered multicast namespaces configuration 19
   server 19
   WDS Server configuration 19

E

Examples - WDS Multicast Session Initiation Protocol

over WDS Control Protocol 24

F

Fields - vendor-extensible 10

G

Glossary 5

H

Higher-layer triggered events 20
   server 20

I

Implementer - security considerations 25
Index of security parameters 25
Informative references 6
Initialization 20
   server 20
Introduction 5

L

Local events 23

M

Message processing
   supported security modes 20

   UDP Protocol 22
   WDSMC_OP_INITIATE OpCode 21
Messages
   Session Initiation Packets for UDP 15
   syntax
      overview 11
      WDSMC_OP_INITIATE OpCode 11
   transport 11
   WDSMC_OP_INITIATE OpCode 11
Multicast Session Initiation Error Packet 17
Multicast Session Initiation Reply Packet 17
Multicast Session Initiation Request Packet 16

N

Normative references 6

O

Other local events
   server 23
Overview (synopsis) 6

P

Parameters - security index 25
Preconditions 9
Prerequisites 9
Product behavior 26

R

References 6
   informative 6
   normative 6
Relationship to other protocols 7
   overview 7
   UDP Protocol 8
   WDS Control Protocol 7

S

Security
   determining client mode 13
   determining server mode 14
   implementer considerations 25
   parameter index 25
   supported modes
      overview 20
      pre-OS client 21
Sequencing rules
   supported security modes 20
   UDP Protocol 22
   WDSMC_OP_INITIATE OpCode 21
Server
   abstract data model 19
   determining security mode 14
   higher-layer triggered events 20
   initialization 20
   other local events 23
   overview 19

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

28 / 29

   timer events 23
   timers 20
Session Initiation Packets for UDP message 15
Session_Initiation_Packets_for_UDP packet 15
Standards assignments 10
Syntax
   overview 11
   WDSMC_OP_INITIATE OpCode 11

T

Timer events 23
   server 23
Timers 20
   server 20
Tracking changes 27
Transport 11
Triggered events - higher-layer 20
   server 20

U

UDP Protocol
   message processing 22
   session initiation packets for
      Multicast Session Initiation Error Packet 17
      Multicast Session Initiation Reply Packet 17
      Multicast Session Initiation Request Packet 16
   using 8

V

Vendor-extensible fields 10
Versioning 9

W

WDS Control Protocol 7
WDSMC_OP_INITIATE OpCode 21
   determining client security mode 13
   determining server security mode 14
   overview 11
WDSMC_OP_INITIATE OpCode message 11

[MS-WDSMSI] - v20240423
Windows Deployment Services Multicast Session Initiation Protocol
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

29 / 29

