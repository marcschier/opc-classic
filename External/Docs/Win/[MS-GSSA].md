[MS-GSSA]:

Generic Security Service Algorithm for Secret Key
Transaction Authentication for DNS (GSS-TSIG) Protocol
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

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 17

Revision Summary

Date

Revision
History

Revision
Class

Comments

4/3/2007

1.0

5/11/2007

1.2

New

Minor

Version 1.0 release

Version 1.2 release

6/1/2007

1.2.1

Editorial

Changed language and formatting in the technical content.

7/3/2007

1.3

Minor

Clarified the meaning of the technical content.

8/10/2007

1.3.1

Editorial

Changed language and formatting in the technical content.

9/28/2007

1.3.2

Editorial

Changed language and formatting in the technical content.

10/23/2007  1.3.3

Editorial

Changed language and formatting in the technical content.

1/25/2008

1.3.4

Editorial

Changed language and formatting in the technical content.

3/14/2008

1.3.5

Editorial

Changed language and formatting in the technical content.

6/20/2008

1.3.6

Editorial

Changed language and formatting in the technical content.

7/25/2008

1.3.7

Editorial

Changed language and formatting in the technical content.

8/29/2008

1.3.8

Editorial

Changed language and formatting in the technical content.

10/24/2008  1.3.9

Editorial

Changed language and formatting in the technical content.

12/5/2008

2.0

1/16/2009

3.0

2/27/2009

4.0

Major

Major

Major

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

4/10/2009

4.0.1

Editorial

Changed language and formatting in the technical content.

5/22/2009

4.0.2

Editorial

Changed language and formatting in the technical content.

7/2/2009

4.0.3

Editorial

Changed language and formatting in the technical content.

8/14/2009

4.0.4

Editorial

Changed language and formatting in the technical content.

9/25/2009

4.0.5

Editorial

Changed language and formatting in the technical content.

11/6/2009

4.0.6

Editorial

Changed language and formatting in the technical content.

12/18/2009  4.0.7

Editorial

Changed language and formatting in the technical content.

1/29/2010

4.0.8

Editorial

Changed language and formatting in the technical content.

3/12/2010

4.0.9

Editorial

Changed language and formatting in the technical content.

4/23/2010

4.0.10

Editorial

Changed language and formatting in the technical content.

6/4/2010

4.0.11

Editorial

Changed language and formatting in the technical content.

7/16/2010

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 17

Date

Revision
History

Revision
Class

Comments

10/8/2010

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

4.0.11

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

4.1

Minor

Clarified the meaning of the technical content.

9/23/2011

4.1

12/16/2011  5.0

3/30/2012

6.0

7/12/2012

6.0

10/25/2012  6.0

1/31/2013

6.0

None

Major

Major

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

7.0

Major

Updated and revised the technical content.

11/14/2013  7.0

2/13/2014

7.0

5/15/2014

7.0

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

8.0

Major

Significantly changed the technical content.

10/16/2015  8.0

None

No changes to the meaning, language, or formatting of the
technical content.

7/14/2016

8.0

6/1/2017

9.0

9/15/2017

10.0

12/1/2017

10.0

None

Major

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 17

Date

Revision
History

Revision
Class

Comments

9/12/2018

11.0

4/7/2021

12.0

6/25/2021

13.0

4/23/2024

14.0

Major

Major

Major

Major

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 17

Table of Contents

1.1
1.2

1.2.1
1.2.2

1  Introduction ............................................................................................................ 6
Glossary ........................................................................................................... 6
References ........................................................................................................ 6
Normative References ................................................................................... 6
Informative References ................................................................................. 7
Overview .......................................................................................................... 7
Relationship to Other Protocols ............................................................................ 7
Prerequisites/Preconditions ................................................................................. 7
Applicability Statement ....................................................................................... 7
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

3  Protocol Details ....................................................................................................... 9
Common Details ................................................................................................ 9
Abstract Data Model ...................................................................................... 9
Timers ........................................................................................................ 9
Initialization ................................................................................................. 9
Higher-Layer Triggered Events ....................................................................... 9
Message Processing Events and Sequencing Rules ............................................ 9
Handling the MAC Field While Digesting DNS Messages ................................ 9
Support for the HDAC-MD5 Algorithm ........................................................ 9
Signing DNS Update Response Messages ................................................. 10
Domain Name Compression .................................................................... 10
Timer Events .............................................................................................. 10
Other Local Events ...................................................................................... 10

3.1.5.1
3.1.5.2
3.1.5.3
3.1.5.4

3.1.6
3.1.7

4  Protocol Examples ................................................................................................. 11

5  Security ................................................................................................................. 14
Security Considerations for Implementers ........................................................... 14
Index of Security Parameters ............................................................................ 14

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 15

7  Change Tracking .................................................................................................... 16

8  Index ..................................................................................................................... 17

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 17

1  Introduction

Secret Key Transaction Authentication for DNS (TSIG), as specified in [RFC2845], provides extensible
transaction level authentication for DNS. The Generic Security Service Algorithm for Secret Key
Transaction Authentication for DNS (GSS-TSIG), as specified in [RFC3645], identifies one possible
extension to TSIG based on the Generic Security Service Application Program Interface (GSS-API), as
specified in [RFC2743].

This document specifies an extension to GSS-TSIG.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

Message Authentication Code (MAC): A message authenticator computed through the use of a
symmetric key. A MAC algorithm accepts a secret key and a data buffer, and outputs a MAC.
The data and MAC can then be sent to another party, which can verify the integrity and
authenticity of the data by using the same secret key and the same MAC algorithm.

security support provider (SSP): A dynamic-link library (DLL) that implements the Security
Support Provider Interface (SSPI) by making one or more security packages available to
applications. Each security package provides mappings between an application's SSPI function
calls and an actual security model's functions. Security packages support security protocols such
as Kerberos authentication and NTLM.

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

[RFC1123] Braden, R., "Requirements for Internet Hosts - Application and Support", RFC 1123,
October 1989, https://www.rfc-editor.org/info/rfc1123

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2136] Thomson, S., Rekhter Y. and Bound, J., "Dynamic Updates in the Domain Name System
(DNS UPDATE)", RFC 2136, April 1997, https://www.rfc-editor.org/info/rfc2136

[RFC2743] Linn, J., "Generic Security Service Application Program Interface Version 2, Update 1", RFC
2743, January 2000, https://www.rfc-editor.org/info/rfc2743

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 17

[RFC2845] Vixie, P., Gudmundsson, O., Eastlake III, D., and Wellington, B., "Secret Key Transaction
Authentication for DNS (TSIG)", RFC 2845, May 2000, https://www.rfc-editor.org/info/rfc2845

[RFC2930] Eastlake III, D., "Secret Key Establishment for DNS (TKEY RR)", RFC 2930, September
2000, https://www.rfc-editor.org/info/rfc2930

[RFC3645] Kwan, S., Garg, P., Gilroy, J., Esibov, L., Westhead, J., and Hall, R., "Generic Security
Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG)", RFC 3645, October
2003, https://www.rfc-editor.org/info/rfc3645

1.2.2  Informative References

None.

1.3  Overview

Secret Key Transaction Authentication for DNS (TSIG), as specified in [RFC2845], is an extensible
protocol by which DNS messages can be authenticated and validated. The Generic Security Service
Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG), as specified in [RFC3645],
defines an algorithm for use with TSIG, which is based on the Generic Security Service Application
Program Interface, as specified in [RFC2743].

In [RFC3645] section 2.2, GSS-TSIG specifies that the final transaction key (TKEY) response indicating
successful negotiation has to be signed. In [RFC2845] section 3.4, TSIG specifies which data is to be
digested when generating or verifying the contents of a TSIG record. This protocol extension defines
an alternate method of building the digest that is used to sign the last message in the GSS-TSIG TKEY
negotiation.

1.4  Relationship to Other Protocols

This specification defines an extension to GSS-TSIG, as specified in [RFC3645]. The relationship of
GSS-TSIG to other protocols is not changed by this protocol extension.

1.5  Prerequisites/Preconditions

All prerequisites and preconditions applicable to GSS-TSIG, as specified in [RFC3645], apply to this
protocol extension.

1.6  Applicability Statement

This protocol extension does not change the way in which GSS-TSIG, as specified in [RFC3645], is
used.

1.7  Versioning and Capability Negotiation

None.

1.8  Vendor-Extensible Fields

None.

1.9  Standards Assignments

None.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 17

2  Messages

This protocol extension does not change the format of messages defined by GSS-TSIG, as specified in
[RFC3645]. The format of messages remains the same, although the contents of the TSIG record
attached to the final TKEY response in the negotiation are changed.

2.1  Transport

This protocol extension does not change the base transport used by GSS-TSIG, as specified in
[RFC3645].

2.2  Message Syntax

This document does not specify any new messages.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 17

3  Protocol Details

3.1  Common Details

3.1.1  Abstract Data Model

None.

3.1.2  Timers

None.

3.1.3  Initialization

This protocol extension does not require any initialization that is not already required by GSS-TSIG, as
specified in [RFC3645].

3.1.4  Higher-Layer Triggered Events

None.

3.1.5  Message Processing Events and Sequencing Rules

This protocol extension does not change message processing events or sequencing rules of messages
defined by GSS-TSIG, as specified in [RFC3645], beyond the changes described in the following
sections.

3.1.5.1  Handling the MAC Field While Digesting DNS Messages

GSS-TSIG, as specified in [RFC3645], specifies how the client and server exchange tokens obtained
from GSS-API calls (as specified in [RFC2743]). The tokens are contained in DNS TKEY records, as
specified in [RFC2930]. In [RFC3645] section 4.1.3, GSS-TSIG specifies that the server MUST sign the
final TKEY response in GSS-TSIG negotiation.

In [RFC2845] section 3.4.3, TSIG specifies that the request message authentication code (MAC) is
to be included in the digest when generating or validating a DNS message. However, because the final
TKEY response in the GSS-TSIG is the first DNS message in the exchange that has been signed, there
is no request MAC that can be included when performing the digest operation.

When there is no request MAC, the most obvious interpretation of [RFC2845] section 3.4.3 is that the
2-byte MAC length with a value of zero be included in the digest to indicate that no MAC data bytes
are being included in the digest. This protocol extension specifies that when building the digest for this
message, the request MAC MUST be completely omitted. In other words, the request MAC length and
request MAC data fields MUST NOT be included in the digest, so the only components of the digest will
be the DNS response message and TSIG response variables.

After GSS-TSIG negotiation is complete, the digesting of further DNS messages MUST include the
request MAC, as specified in [RFC2845] section 3.4.

3.1.5.2  Support for the HDAC-MD5 Algorithm

[RFC2845] section 2.2 specifies that TSIG MUST support the "HMAC-MD5" algorithm. GSS-API does
not explicitly define the MAC formats supported. Instead it relies on the security support provider

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 17

(SSP) that is exposed by the operating system. Implementations of this protocol extension MUST NOT
support the "HMAC-MD5.SIG-ALG.REG.INT" algorithm in [RFC2845] section 7. Implementations of this
protocol extension MUST support the "gss-tsig" algorithm, as specified in [RFC3645] section 3.1.2.

3.1.5.3  Signing DNS Update Response Messages

As described in [RFC2136] section 3.8, the DNS server MUST send a DNS update response back to the
DNS client after processing a DNS update request. If the DNS update request is signed and includes a
TSIG record, as specified in [RFC3645] and [RFC2845] section 4, then the DNS server SHOULD<1>
sign the DNS update response and include the resulting TSIG record as described in [RFC3645].

3.1.5.4  Domain Name Compression

As described in [RFC1123] section 6.1.2.4, name servers MUST use compression in responses. For the
TSIG resource record in DNS response messages, compression is not supported.

3.1.6  Timer Events

None.

3.1.7  Other Local Events

None.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 17

4  Protocol Examples

Examples that clarify the difference between a strict interpretation of the relevant RFCs and the
Microsoft implementation are included in the figures in this section.

Figure 1: Example of a protocol sequence

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 17

Figure 2: Example of Message #2 input to the GSS_GetMIC TSIG generation function

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 17

Figure 3: Example of Message #2, as it appears on the wire

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 17

5  Security

5.1  Security Considerations for Implementers

None.

5.2  Index of Security Parameters

None.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 17

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows 2000 operating system

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

<1> Section 3.1.5.3: In the Windows implementation of the DNS server, the DNS client will find that
the DNS server signed a DNS update response, as described in [RFC3645], only if the RCODE value in
the response message is zero (indicating success).

If the RCODE value in the DNS update response message is not zero (indicating failure), the DNS
client will find that the DNS server did not sign the response. Instead, the DNS server copied the DNS
update request message, changed the RCODE value to the applicable nonzero error value, and sent
that message back to the DNS client as the response. The message was otherwise unmodified; that is,
the response message contains the same signature that was in the request message.

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 17

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

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 17

   common details 9
   main 7
Overview (synopsis) 7

P

Parameters - security index 14
Preconditions 7
Prerequisites 7
Product behavior 15

R

References 6
   informative 7
   normative 6
Relationship to other protocols 7

S

Secret Key Transaction Authentication for DNS

(TSIG) described 7

Security
   implementer considerations 14
   parameter index 14
Sequencing rules 9
Standards assignments 7
Syntax 8

T

Timer events 10
Timers 9
Tracking changes 16
Transport 8
Triggered events - higher-layer 9

V

Vendor-extensible fields 7
Versioning 7

8  Index
A

Abstract data model 9
Applicability 7

C

Capability negotiation 7
Change tracking 16
Common details 9

D

Data model - abstract 9
Details - common 9

E

Examples 11

F

Fields - vendor-extensible 7

G

Glossary 6

H

Higher-layer triggered events 9

I

Implementer - security considerations 14
Index of security parameters 14
Informative references 7
Initialization 9
Introduction 6

L

Local events 10

M

Message processing 9
Messages
   overview 8
   syntax 8
   transport 8

N

Normative references 6

O

Overview

[MS-GSSA] - v20240423
Generic Security Service Algorithm for Secret Key Transaction Authentication for DNS (GSS-TSIG) Protocol
Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 17

