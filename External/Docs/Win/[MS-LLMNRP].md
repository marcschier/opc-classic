[MS-LLMNRP]:

Link Local Multicast Name Resolution (LLMNR) Profile

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

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 20

Revision Summary

Date

Revision
History

Revision
Class

Comments

10/24/2008  0.2

12/5/2008

1.0

1/16/2009

1.1

New

Major

Minor

Version 0.2 release

Updated and revised the technical content.

Clarified the meaning of the technical content.

2/27/2009

1.1.1

Editorial

Changed language and formatting in the technical content.

4/10/2009

2.0

Major

Updated and revised the technical content.

5/22/2009

2.0.1

Editorial

Changed language and formatting in the technical content.

7/2/2009

2.0.2

Editorial

Changed language and formatting in the technical content.

8/14/2009

2.0.3

Editorial

Changed language and formatting in the technical content.

9/25/2009

2.1

Minor

Clarified the meaning of the technical content.

11/6/2009

2.1.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  2.1.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

2.2

Minor

Clarified the meaning of the technical content.

3/12/2010

2.2.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

2.2.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

2.2.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

2.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

2.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

2.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  2.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

3.0

Major

Updated and revised the technical content.

2/11/2011

3.0

3/25/2011

3.0

5/6/2011

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

6/17/2011

3.1

Minor

Clarified the meaning of the technical content.

9/23/2011

3.1

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  4.0

Major

Updated and revised the technical content.

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 20

Date

Revision
History

Revision
Class

Comments

3/30/2012

4.0

None

No changes to the meaning, language, or formatting of the
technical content.

7/12/2012

5.0

Major

Updated and revised the technical content.

10/25/2012  5.0

1/31/2013

5.1

8/8/2013

6.0

11/14/2013  6.0

2/13/2014

6.0

5/15/2014

6.0

None

Minor

Major

None

None

None

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

7.0

Major

Significantly changed the technical content.

10/16/2015  7.0

7/14/2016

7.0

6/1/2017

7.0

9/15/2017

8.0

9/12/2018

9.0

4/7/2021

10.0

6/25/2021

11.0

4/23/2024

12.0

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

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 20

Table of Contents

1.1
1.2

1.2.1
1.2.2

1  Introduction ............................................................................................................ 5
Glossary ........................................................................................................... 5
References ........................................................................................................ 5
Normative References ................................................................................... 5
Informative References ................................................................................. 6
Overview .......................................................................................................... 6
Relationship to Other Protocols ............................................................................ 6
Prerequisites/Preconditions ................................................................................. 6
Applicability Statement ....................................................................................... 6
Versioning and Capability Negotiation ................................................................... 7
Vendor-Extensible Fields ..................................................................................... 7
Standards Assignments ....................................................................................... 7

1.3
1.4
1.5
1.6
1.7
1.8
1.9

2  Messages ................................................................................................................. 8
Transport .......................................................................................................... 8
Message Syntax ................................................................................................. 8

2.1
2.2

3.1

3.1.1
3.1.2
3.1.3
3.1.4
3.1.5
3.1.6
3.1.7

3  Protocol Details ....................................................................................................... 9
LLMNR Sender Details ......................................................................................... 9
Abstract Data Model ...................................................................................... 9
Timers ........................................................................................................ 9
Initialization ................................................................................................. 9
Higher-Layer Triggered Events ....................................................................... 9
Message Processing Events and Sequencing Rules ............................................ 9
Timer Events .............................................................................................. 10
Other Local Events ...................................................................................... 10
LLMNR Responder Details .................................................................................. 10
Abstract Data Model .................................................................................... 10
Timers ...................................................................................................... 10
Initialization ............................................................................................... 11
Higher-Layer Triggered Events ..................................................................... 11
Message Processing Events and Sequencing Rules .......................................... 11
Timer Events .............................................................................................. 11
Other Local Events ...................................................................................... 11

3.2.1
3.2.2
3.2.3
3.2.4
3.2.5
3.2.6
3.2.7

3.2

4  Protocol Examples ................................................................................................. 12

5  Security ................................................................................................................. 16
Security Considerations for Implementers ........................................................... 16
Index of Security Parameters ............................................................................ 16

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 17

7  Change Tracking .................................................................................................... 18

8  Index ..................................................................................................................... 19

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 20

1  Introduction

The Link Local Multicast Name Resolution (LLMNR) Profile describes a profile of the Link Local Multicast
Name Resolution (LLMNR) protocol, specified in [RFC4795] that does not send or respond to unicast
queries in TCP and does not support Extension Mechanisms for DNS (EDNS0) [RFC2671]. This profile
enables name resolution on the link local scenarios in which name resolution is not possible on the
conventional DNS local link, as specified in [RFC1035].

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

Transmission Control Protocol (TCP): A protocol used with the Internet Protocol (IP) to send
data in the form of message units between computers over the Internet. TCP handles keeping
track of the individual units of data (called packets) that a message is divided into for efficient
routing through the Internet.

User Datagram Protocol (UDP): The connectionless protocol within TCP/IP that corresponds to

the transport layer in the ISO/OSI reference model.

UTF-8: A byte-oriented standard for encoding Unicode characters, defined in the Unicode standard.

Unless specified otherwise, this term refers to the UTF-8 encoding form specified in
[UNICODE5.0.0/2007] section 3.9.

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

[RFC1035] Mockapetris, P., "Domain Names - Implementation and Specification", STD 13, RFC 1035,
November 1987, https://www.rfc-editor.org/info/rfc1035

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2671] Vixie, P., "Extension mechanism for DNS", RFC 2671, August 1999, https://www.rfc-
editor.org/info/rfc2671

[RFC3629] Yergeau, F., "UTF-8, A Transformation Format of ISO 10646", STD 63, RFC 3629,
November 2003, https://www.rfc-editor.org/info/rfc3629

[RFC4795] Aboba, B., Thaler, D., and Esibov, L., "Link-Local Multicast Name Resolution (LLMNR)", RFC
4795, January 2007, https://www.rfc-editor.org/info/rfc4795

5 / 20

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

[RFC768] Postel, J., "User Datagram Protocol", STD 6, RFC 768, August 1980, https://www.rfc-
editor.org/info/rfc768

[RFC793] Postel, J., Ed., "Transmission Control Protocol: DARPA Internet Program Protocol
Specification", RFC 793, September 1981, https://www.rfc-editor.org/info/rfc793

1.2.2  Informative References

[RFC2308] Andrews, M., "Negative Caching of DNS Queries (DNS NCACHE)", RFC 2308, March 1998,
https://www.rfc-editor.org/info/rfc2308

[RFC2937] Smit, C., "The Name Service Search Option for DHCP", RFC 2937, September 2000,
https://www.rfc-editor.org/info/rfc2937

[RFC3492] Costello, A., "Punycode: A Bootstring encoding of Unicode for Internationalized Domain
Names in Applications", RFC 3492, March 2003, http://www.ietf.org/rfc/rfc3492.txt

1.3  Overview

The Link Local Multicast Name Resolution (LLMNR) protocol, specified in [RFC4795] does not send or
respond to unicast queries in TCP and does not support Extension Mechanisms for DNS (EDNS0)
[RFC2671]. This profile enables name resolution on the link local scenarios in which name resolution is
not possible on the conventional DNS local link, as specified in [RFC1035].

Link Local Multicast Name Resolution queries are sent to and received on port 5355, as specified in
[RFC4795]. This profile of LLMNR differs from LLMNR defined in [RFC4795], principally in the area of
transport. Specifically, in the following areas:



[RFC4795] requires TCP, as specified in [RFC793], support, but TCP support is optional in this
profile.



[RFC4795] requires EDNS0 [RFC2671] support, but EDNS0 support is optional in this profile.

1.4  Relationship to Other Protocols

Relationship to other protocols is unchanged from [RFC4795].

Implementations of this LLMNRP profile without TCP do not preclude or prohibit [RFC4795]
implementations with TCP from operating on the same network; however, senders and responders
using this LLMNR profile cannot participate in TCP transactions.

1.5  Prerequisites/Preconditions

Prerequisites and preconditions for this profile are unchanged from [RFC4795].

1.6  Applicability Statement

The applicability of this LLMNR profile is unchanged from [RFC4795] except for the following:





This LLMNR profile is applicable only to resolving single-label names.

This LLMNR profile is not applicable to resolving all DNS record types. Specifically, only A, AAAA,
and PTR record types are required by this profile. Support for other record types is optional.

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 20

1.7  Versioning and Capability Negotiation

This profile introduces no new versioning or capability negotiation mechanisms beyond those
described in [RFC4795]. An implementation of this LLMNR profile can interoperate with an
implementation of LLMNR based on [RFC4795] but issues might arise in the following general areas
that are covered in detail in section 3 of this document:

  Sending and receiving large responses that exceed the link MTU or 512 octets.

  Situations where TCP is used.

  Querying resource records other than A, AAAA, and PTR.

1.8  Vendor-Extensible Fields

This profile does not support any vendor-extensible fields.

1.9  Standards Assignments

This profile includes no standards assignments beyond those specified in [RFC4795].

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 20

2  Messages

2.1  Transport

[RFC4795] requires support for both the User Datagram Protocol (UDP)  [RFC768] and the
Transmission Control Protocol (TCP) as transports for LLMNR messages.

An implementation of this profile MUST support UDP as a transport and MAY support TCP as a
transport.

2.2  Message Syntax

The message syntax remains unchanged from the protocol specified in [RFC4795] section 2.

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 20

3  Protocol Details

3.1  LLMNR Sender Details

LLMNR sender details are specified in [RFC4795] sections 1, 2, and 3, with differences specified as
follows.

3.1.1  Abstract Data Model

This section describes a conceptual model of possible data organization that an implementation
maintains to participate in this protocol. The described organization is provided to facilitate the
explanation of how the protocol behaves. This document does not mandate that implementations
adhere to this model as long as their external behavior is consistent with that described in this
document.

The state that needs to be maintained by a sender in this LLMNR profile is unchanged from
[RFC4795]. [RFC4795] states in section 5.4 that LLMNR implementations MUST use a distinct, isolated
cache for LLMNR on each interface. This statement is vague in terms of whether it means LLMNR
implementations MUST support caching or it means LLMNR implementations MUST keep the LLMNR
cache, if one exists, distinct from the DNS cache and isolated on a per-interface basis.
Implementations of this LLMNR profile MAY support caching. If an implementation of this LLMNR
profile performs negative caching for a name error response or lack of a response for an LLMNR query,
then it MUST do so only if there’s already a cached DNS name error entry in the DNS cache for the
name being queried. Implementations of this LLMNR profile can determine whether a negative DNS
cache entry exists, by issuing a DNS query. A response of NXDOMAIN indicates that the DNS name
does not exist and will thus result in a negative DNS cache entry. Any other response indicates that a
negative DNS cache entry does not exist [RFC2308].

3.1.2  Timers

The timers required by a sender in this LLMNR profile are unchanged from [RFC4795] except for the
following.<1>

To avoid synchronization [RFC4795] specifies in section 2.7 that the transmission of each LLMNR
query SHOULD be delayed by a time randomly selected from the interval 0 to JITTER_INTERVAL.
Implementing this behavior requires a timer. In this profile, the sender SHOULD send queries
immediately without a random delay thereby avoiding the need for such a timer.

3.1.3  Initialization

The initialization required by this LLMNR profile is unchanged from [RFC4795].

[RFC4795] section 3.1 is ambiguous as to whether support for the Name Service Search Option
(NSSO) [RFC2937] and the LLMNR Enable Option are mandatory. However, [RFC4795] includes them
only as informative references, indicating that they need not be read or understood to implement
LLMNR. As such, this profile clarifies that there are no conformance requirements with respect to those
references.

3.1.4  Higher-Layer Triggered Events

Processing of higher-layer triggered events is unchanged from [RFC4795].

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 20

3.1.5  Message Processing Events and Sequencing Rules

Except as specified in this section, the message processing and sequencing rules for an LLMNR profile
sender are unchanged from [RFC4795].

[RFC4795] section 2.1 requires that an LLMNR sender accept responses as large as the smaller of the
link MTU or 9194 octets. In this profile, a sender MUST accept responses as large as the maximum
UDP payload that can be carried over IPv4 or IPv6. The sender MAY use the EDNS0 [RFC2671] OPT
record to indicate the maximum UDP payload size it can accept.

When a response is received with the TC bit set, [RFC4795] section 2.1.1 recommends (but does not
require) that the LLMNR sender discard the response and resend the query over TCP. In this profile,
the sender MAY do so, but instead SHOULD simply ignore the TC bit and process the response as if
there is no truncation.

[RFC4795] specifies in section 2.7 that since it is possible for a response with the "C" bit clear to be
followed by a response with the "C" bit set, an LLMNR sender SHOULD be prepared to process
additional responses for the purposes of conflict detection, even after it has considered a query
answered. In this profile, the sender MAY process the additional responses once it considers a query
answered.

[RFC4795] section 2.9 recommends (but does not require) that the LLMNR sender include conflicting
RRs in the additional section of queries with the "C" bit set. In this profile, conflicting RRs MAY be
included in the additional section.

[RFC4795] specifies in section 2.7 that in order to avoid synchronization, the transmission of each
LLMNR query SHOULD be delayed by a time randomly selected from the interval 0 to
JITTER_INTERVAL. In this profile, the sender SHOULD send queries immediately without a random
delay.

[RFC4795] section 2.4 recommends (but does not require) that an LLMNR sender send PTR queries
using TCP unicast as opposed to UDP multicast. In this profile, the LLMNR sender MAY use unicast TCP
for PTR queries, but instead SHOULD use UDP multicast.

[RFC4795] does not specify whether names in queries are to be sent in UTF-8 [RFC3629] or Punycode
[RFC3492]. In this LLMNR profile, a sender MUST send queries in UTF-8, not Punycode.

3.1.6  Timer Events

Handling of timer events by a sender in this LLMNR profile is unchanged from [RFC4795].

3.1.7  Other Local Events

Handling of other local events by a sender in this LLMNR profile is unchanged from [RFC4795].

3.2  LLMNR Responder Details

LLMNR responder details are specified in [RFC4795] sections 2 and 4, with differences as specified
below.

3.2.1  Abstract Data Model

The state that needs to be maintained by a responder in this LLMNR profile is unchanged from
[RFC4795].

Implementations of this LLMNR profile need not have a configurable or extendable data store
containing the names to which the responder will respond.<2>

10 / 20

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3.2.2  Timers

The timers required by a responder in this LLMNR profile are unchanged from [RFC4795] except for
the following.<3>

[RFC4795] specifies in section 2.7 that in order to avoid synchronization, the transmission of each
LLMNR response SHOULD be delayed by a time randomly selected from the interval 0 to
JITTER_INTERVAL. Implementing this behavior requires a timer. In this profile, the responder SHOULD
send responses immediately without a random delay thereby avoiding the need for such a timer.

3.2.3  Initialization

The initialization required by this LLMNR profile is unchanged from [RFC4795] except for the following.

In [RFC4795], listening on TCP port 5355 is required. In this LLMNR profile, the responder MAY listen
on TCP port 5355 and MAY respond to TCP queries as specified in [RFC4795] sections 2.3 and
2.4.<4>

3.2.4  Higher-Layer Triggered Events

Processing of higher-layer triggered events is unchanged from [RFC4795].

3.2.5  Message Processing Events and Sequencing Rules

Except as specified in this section, the message processing and sequencing rules are unchanged from
[RFC4795].

[RFC4795] section 2.1 recommends (but does not require) that the responder only send UDP
responses as large as is permissible without causing fragmentation. In this profile, a responder MUST
send UDP responses with size up to the maximum UDP payload that can be carried over IPv4 or IPv6.
The LLMNR profile responder MAY honor the maximum acceptable UDP payload size indicated by an
ENDS0 OPT record in a query. If the resource records that need to be sent in the response do not all
fit in the UDP packet, then the LLMNR profile responder MUST put as many resource records as can fit
in the UDP packet and send the response without setting the TC bit.

The LLMNR profile responder MUST respond to queries for resource record types of A, AAAA, PTR, and
ANY. The LLMNR profile responder MAY respond to queries for other resource record types, but instead
SHOULD silently discard queries for other resource record types. In response to a query with resource
record type of ANY, the LLMNR profile responder MUST return any eligible A and AAAA resource
records per [RFC4795] section 2.6 and MAY return other types of resource records.

The LLMNR profile responder MUST respond to queries for names encoded in UTF-8 format [RFC3629]
and MAY respond to queries for internationalized names converted to Punycode [RFC3492].

[RFC4795] section 4.2 specifies that an LLMNR responder SHOULD log name conflicts detected as a
result of uniqueness verification. A responder in this LLMNR profile MAY log name conflicts.

3.2.6  Timer Events

Handling of timer events by a responder in this LLMNR profile is unchanged from [RFC4795].

3.2.7  Other Local Events

Handling of other local events by a responder in this LLMNR profile is unchanged from [RFC4795].

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 20

4  Protocol Examples

The following example illustrates an LLMNR query for AAAA resource records for a host name that
starts with a non-ASCII character (represented in UTF-8 encoding) and the corresponding response,
which contains multiple AAAA resource records that make the response larger than the 512-octet UDP
payload limit observed by DNS:

UDP/IPv6 packet containing the AAAA LLMNR query for host name "çest":

 - Ipv6:
     Versions: IPv6, Internet Protocol, DSCP 0
     PayloadLength: 31 (0x1F)
     NextProtocol: 17(0x11)
     HopLimit: 1 (0x1)
     SourceAddress: FE80:0:0:0:D9F6:CE2E:4875:AB03
     DestinationAddress: FF02:0:0:0:0:0:1:3
 - Udp:
     SourcePort: 62925(0xf5cd)
     DestinationPort: 5355(0x14eb)
     TotalLength: 31 (0x1F)
     Checksum: 37373 (0x91FD)
 - Llmnr:
     QueryIdentifier: 35893 (0x8C35)
   - Flags:
      QR:       (0...............) Query
      OPCode:   (.0000...........) Standard
      C:        (.....0..........)
      TC:       (......0.........)
      T:        (.......0........)
      Reserved: (........0000....)
      RCode:    (............0000) Success
     QuestionCount: 1 (0x1)
     AnswerCount: 0 (0x0)
     NameServerCount: 0 (0x0)
     AdditionalCount: 0 (0x0)
   - QRecord:
      QuestionName: çest (0x05 0xC3 0xA7 0x65 0x73 0x74 0x00)
      QuestionType: AAAA, 28(0x1c)
      QuestionClass: Internet, 1(0x1)

UDP/IPv6 packet containing the LLMNR response, which includes 25 AAAA resource records. In the
following example, all 25 IP addresses belong to interfaces on the same host and are thus not in
conflict.

 - Ipv6:
   - Versions: IPv6, Internet Protocol, DSCP 0
     PayloadLength: 736 (0x2E0)
     NextProtocol: 17(0x11)
     HopLimit: 64 (0x40)
     SourceAddress: FE80:0:0:0:0:0:0:100
     DestinationAddress: FE80:0:0:0:D9F6:CE2E:4875:AB03
 - Udp:
     SourcePort: 5355(0x14eb)
     DestinationPort: 62925(0xf5cd)
     TotalLength: 736 (0x2E0)
     Checksum: 9332 (0x2474)
 - Llmnr:
     QueryIdentifier: 35893 (0x8C35)
   - Flags:
      QR:       (1...............) Response

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 20

      OPCode:   (.0000...........) Standard
      C:        (.....0..........)
      TC:       (......0.........)
      T:        (.......0........)
      Reserved: (........0000....)
      RCode:    (............0000) Success
     QuestionCount: 1 (0x1)
     AnswerCount: 25 (0x19)
     NameServerCount: 0 (0x0)
     AdditionalCount: 0 (0x0)
   - QRecord:
      QuestionName: çest (0x05 0xC3 0xA7 0x65 0x73 0x74 0x00)
      QuestionType: AAAA, 28(0x1c)
      QuestionClass: Internet, 1(0x1)
   - ARecord:
      ResourceName: çest (0x05 0xC3 0xA7 0x65 0x73 0x74 0x00)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: 2001:4898:1B:5:709F:3CF3:698E:AB15
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: 2002:9D3B:1DF3:8:709F:3CF3:698E:AB15
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FEC0:0:0:8:709F:3CF3:698E:AB15
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:100
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:101
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:102
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:103
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 20

      IPv6Address: FE80:0:0:0:0:0:0:104
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:105
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:106
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:107
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:108
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:109
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:110
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:111
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:112
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:113
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 20

      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:114
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:115
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:116
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:117
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:118
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:119
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:0:0:0:120
   - ARecord:
      ResourceName: çest (0xC0 0x17)
      ResourceType: AAAA, 28(0x1c)
      ResourceClass: Internet, 1(0x1)
      TimeToLive: 30 (0x1E)
      ResourceDataLength: 16 (0x10)
      IPv6Address: FE80:0:0:0:709F:3CF3:698E:AB15

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 20

5  Security

5.1  Security Considerations for Implementers

Security considerations for this profile of LLMNR are unchanged from [RFC4795].

5.2  Index of Security Parameters

None.

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 20

6  Appendix A: Product Behavior

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

<1> Section 3.1.2: Windows Vista, Windows Server 2008, Windows 7, and Windows Server 2008 R2
operating system use a retry time of 100 ms and a wait time of 200 ms. Otherwise Windows uses a
retry time of 410 ms and a wait time of 410 ms.

<2> Section 3.2.1: Applicable Windows releases do not have an extendable or configurable data
store. The LLMNR responder will respond only to the computer's host name. Therefore, the Windows
releases of this LLMNR profile cannot be configured to respond to arbitrary names.

<3> Section 3.2.2: Windows Vista, Windows Server 2008, Windows 7, and Windows Server 2008 R2
use a retry time of 100 ms and a wait time of 200 ms. Otherwise, Windows uses a retry time of 410
ms and a wait time of 410 ms.

<4> Section 3.2.3: Windows does not listen to LLMNR queries on any TCP port, including 5355.

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 20

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

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 20

8  Index
A

Abstract data model
   responder 10
   sender 9
Applicability 6

C

Capability negotiation 7
Change tracking 18

D

Data model - abstract
   responder 10
   sender 9

E

Examples - overview 12

F

Fields - vendor-extensible 7

G

Glossary 5

H

Higher-layer triggered events
   responder 11
   sender 9

I

Implementer - security considerations 16
Index of security parameters 16
Informative references 6
Initialization
   responder 11
   sender 9
Introduction 5

L

Local events
   responder 11
   sender 10

M

Message processing
   responder 11
   sender 9
Messages
   syntax 8
   transport 8

N

Normative references 5

O

Overview (synopsis) 6

P

Parameters - security index 16
Preconditions 6
Prerequisites 6
Product behavior 17

R

References 5
   informative 6
   normative 5
Relationship to other protocols 6
Responder
   abstract data model 10
   higher-layer triggered events 11
   initialization 11
   local events 11
   message processing 11
   overview 10
   sequencing rules 11
   timer events 11
   timers 10

S

Security
   implementer considerations 16
   parameter index 16
Sender
   abstract data model 9
   higher-layer triggered events 9
   initialization 9
   local events 10
   message processing 9
   overview 9
   sequencing rules 9
   timer events 10
   timers 9
Sequencing rules
   responder 11
   sender 9
Standards assignments 7
Syntax 8

T

Timer events
   responder 11
   sender 10
Timers
   responder 10

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 20

   sender 9
Tracking changes 18
Transport 8
Triggered events - higher-layer
   responder 11
   sender 9

V

Vendor-extensible fields 7
Versioning 7

[MS-LLMNRP] - v20240423
Link Local Multicast Name Resolution (LLMNR) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 20

