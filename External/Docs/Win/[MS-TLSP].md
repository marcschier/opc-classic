[MS-TLSP]:

Transport Layer Security (TLS) Profile

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

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

1 / 19


Revision Summary

Date

Revision
History

Revision
Class

Comments

10/24/2008  0.1

New

Version 0.1 release

12/5/2008

0.1.1

Editorial

Changed language and formatting in the technical content.

1/16/2009

0.1.2

Editorial

Changed language and formatting in the technical content.

2/27/2009

0.2

4/10/2009

1.0

Minor

Major

Clarified the meaning of the technical content.

Updated and revised the technical content.

5/22/2009

1.0.1

Editorial

Changed language and formatting in the technical content.

7/2/2009

1.1

Minor

Clarified the meaning of the technical content.

8/14/2009

1.1.1

Editorial

Changed language and formatting in the technical content.

9/25/2009

1.2

Minor

Clarified the meaning of the technical content.

11/6/2009

1.2.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  1.2.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

2.0

Major

Updated and revised the technical content.

3/12/2010

2.0.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

2.0.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

2.0.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

2.0.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

2.1

Minor

Clarified the meaning of the technical content.

9/23/2011

2.1

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  3.0

Major

Updated and revised the technical content.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

2 / 19


Date

Revision
History

Revision
Class

Comments

3/30/2012

3.0

7/12/2012

3.0

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

10/25/2012  4.0

Major

Updated and revised the technical content.

1/31/2013

4.0

None

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

5.0

Major

Updated and revised the technical content.

11/14/2013  5.0

None

No changes to the meaning, language, or formatting of the
technical content.

2/13/2014

5.0

5/15/2014

6.0

6/30/2015

7.0

10/16/2015  8.0

7/14/2016

9.0

3/16/2017

10.0

6/1/2017

10.0

None

Major

Major

Major

Major

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

9/15/2017

11.0

Major

Significantly changed the technical content.

12/1/2017

11.0

9/12/2018

12.0

4/7/2021

13.0

6/25/2021

14.0

10/6/2021

14.0

4/23/2024

15.0

4/13/2026

16.0

None

Major

Major

Major

None

Major

Major

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

3 / 19


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
    - [2.2.1 Client and Server Hello Messages](#221-client-and-server-hello-messages)
    - [2.2.2 Alert Messages](#222-alert-messages)
    - [2.2.3 Extended Hello Messages](#223-extended-hello-messages)
    - [2.2.4 Certificate Messages](#224-certificate-messages)
  - [2.3 Directory Service Schema Elements](#23-directory-service-schema-elements)
- [3 Protocol Details](#3-protocol-details)
  - [3.1 Common Details](#31-common-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Higher-Layer Triggered Events](#314-higher-layer-triggered-events)
    - [3.1.5 Processing Events and Sequencing Rules](#315-processing-events-and-sequencing-rules)
      - [3.1.5.1 GSS_WrapEx() Call](#3151-gsswrapex-call)
      - [3.1.5.2 GSS_UnwrapEx() Call](#3152-gssunwrapex-call)
    - [3.1.6 Timer Events](#316-timer-events)
    - [3.1.7 Other Local Events](#317-other-local-events)
- [4 Protocol Examples](#4-protocol-examples)
- [5 Security](#5-security)
  - [5.1 Security Considerations for Implementers](#51-security-considerations-for-implementers)
  - [5.2 Index of Security Parameters](#52-index-of-security-parameters)
- [6 Appendix A: Product Behavior](#6-appendix-a-product-behavior)
- [7 Change Tracking](#7-change-tracking)
- [8 Index](#8-index)

## 1 Introduction

The Transport Layer Security (TLS) Profile specifies a restricted subset of TLS and related standards.
Support for TLS/SSL protocols is specified in [RFC8446], [RFC5246], [RFC4346], [RFC2246], and
[SSL3]. Supported TLS extensions are specified in [RFC8472], [RFC5077], [RFC7301], [RFC4366],
[RFC4681], [RFC3546], and [RFC7627]. Additional supported cryptographic curves are specified in
[RFC7748]. Hybrid ECDHE/ML-KEM groups are specified in [IETFDRAFT-TLS-ECDHE-MLKEM-04].
Cipher suites are specified in [RFC5487], [RFC5289], [RFC4492], and [RFC3268].<1>

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

ASCII: The American Standard Code for Information Interchange (ASCII) is an 8-bit character-
encoding scheme based on the English alphabet. ASCII codes represent text in computers,
communications equipment, and other devices that work with text. ASCII refers to a single 8-bit
ASCII character or an array of 8-bit ASCII characters with the high bit of each character set to
zero.

cipher: A cryptographic algorithm used to encrypt and decrypt files and messages.

Secure Sockets Layer (SSL): A security protocol that supports confidentiality and integrity of

messages in client and server applications that communicate over open networks. SSL supports
server and, optionally, client authentication using X.509 certificates [X509] and [RFC5280]. SSL
is superseded by Transport Layer Security (TLS). TLS version 1.0 is based on SSL version
3.0 [SSL3].

Transport Layer Security (TLS): A security protocol that supports confidentiality and integrity of
messages in client and server applications communicating over open networks. TLS supports
server and, optionally, client authentication by using X.509 certificates (as specified in [X509]).
TLS is standardized in the IETF TLS working group.

UTF-8: A byte-oriented standard for encoding Unicode characters, defined in the Unicode standard.

Unless specified otherwise, this term refers to the UTF-8 encoding form specified in
[UNICODE5.0.0/2007] section 3.9.

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

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

5 / 19


[IETFDRAFT-TLS-ECDHE-MLKEM-04] Kris Kwiatkowski, Panos Kampanakis, Bas Westerbaan, Douglas
Stebila, "Post-quantum hybrid ECDHE-MLKEM Key Agreement for TLSv1.3", draft-ietf-tls-ecdhe-
mlkem-04, February 2026, https://datatracker.ietf.org/doc/draft-ietf-tls-ecdhe-mlkem/

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2246] Dierks, T., and Allen, C., "The TLS Protocol Version 1.0", RFC 2246, January 1999,
https://www.rfc-editor.org/info/rfc2246

[RFC2743] Linn, J., "Generic Security Service Application Program Interface Version 2, Update 1", RFC
2743, January 2000, https://www.rfc-editor.org/info/rfc2743

[RFC3268] Chown, P., "Advanced Encryption Standard (AES) Ciphersuites for Transport Layer Security
(TLS)", RFC 3268, June 2002, https://www.rfc-editor.org/info/rfc3268

[RFC3546] Blake-Wilson, S., Nystrom, M., Hopwood, D., Mikkelsen, J., and Wright, T., "Transport
Layer Security (TLS) Extensions", RFC 3546, June 2003, https://www.rfc-editor.org/info/rfc3546

[RFC4346] Dierks, T., and Rescorla, E., "The Transport Layer Security (TLS) Protocol Version 1.1",
RFC 4346, April 2006, https://www.rfc-editor.org/info/rfc4346

[RFC4366] Blake-Wilson, S., Nystrom, M., Hopwood, D., et al., "Transport Layer Security (TLS)
Extensions", RFC 4366, April 2006, https://www.rfc-editor.org/info/rfc4366

[RFC4492] Blake-Wilson, S., Bolyard, N., Gupta, V., et al., "Elliptic Curve Cryptography (ECC) Cipher
Suites for Transport Layer Security (TLS)", RFC 4492, May 2006, https://www.rfc-
editor.org/info/rfc4492

[RFC4681] Ball, J., Medvinsky, A., and Santesson, S., "TLS User Mapping Extension", RFC 4681,
October 2006, https://www.rfc-editor.org/info/rfc4681

[RFC5077] Salowey, J., Zhou, H., Eronen, P., and Tschofenig, H., "Transport Layer Security (TLS)
Session Resumption without Server-Side State", RFC 5077, January 2008, http://www.rfc-
editor.org/rfc/rfc5077.txt

[RFC5246] Dierks, T., and Rescorla, E., "The Transport Layer Security (TLS) Protocol Version 1.2",
RFC 5246, August 2008, https://www.rfc-editor.org/info/rfc5246

[RFC5289] Rescorla, E., "TLS Elliptic Curve Cipher Suites with SHA-256/384 and AES Galois Counter
Mode (GCM)", RFC 5289, August 2008, https://www.rfc-editor.org/info/rfc5289

[RFC5487] Badra, M., "Pre-Shared Key Cipher Suites for TLS with SHA-256/384 and AES Galois
Counter Mode", RFC 5487, March 2009, https://www.rfc-editor.org/info/rfc5487

[RFC7301] Friedl, S., Popov, A., Langley, A., and Stephan, E., "Transport Layer Security (TLS)
Application-Layer Protocol Negotiation Extension", RFC 7301, July 2014, https://www.rfc-
editor.org/info/rfc7301

[RFC7627] Bhargaven, K., Delignat-Lavaud, A., Pironti, A., Paris-Rocquencourt, Inria, Langley, A., and
Ray, M., "Transport Layer Security (TLS) Session Hash and Extended Master Secret Extension", RFC
7627, September 2015, https://www.rfc-editor.org/info/rfc7627

[RFC7748] Langley, A., Hamburg, M., and Turner, S., "Elliptic Curves for Security", RFC 7748, January
2016, https://www.rfc-editor.org/info/rfc7748

[RFC8446] Rescorla, E., "The Transport Layer Security (TLS) Protocol Version 1.3", RFC 8446, August
2018, https://www.rfc-editor.org/info/rfc8446

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

6 / 19


[RFC8472] Popov, A., Ed.,  Nystroem, M., Balfanz, D.,, "Transport Layer Security (TLS) Extension for
Token Binding Protocol Negotiation", RFC 8472, October 2018, https://www.rfc-
editor.org/info/rfc8472

#### 1.2.2 Informative References

[KB4019276] Microsoft Corporation, "Update for Windows Server 2008",
https://www.catalog.update.microsoft.com/Search.aspx?q=%20KB4019276

[MSDOCS-EnableTLS1.1/2] Microsoft Corporation, "Update to add support for TLS 1.1 and TLS 1.2 in
Windows Server 2008 SP2, Windows Embedded POSReady 2009, and Windows Embedded Standard
2009", https://support.microsoft.com/en-us/topic/update-to-add-support-for-tls-1-1-and-tls-1-2-in-
windows-server-2008-sp2-windows-embedded-posready-2009-and-windows-embedded-standard-
2009-b6ab553a-fa8f-3f5e-287c-e752eb3ce5f4

[MSDOCS-SB-3081320] Microsoft Corporation, "Microsoft Security Bulletin MS15-121 – Important:
Security Update for Schannel to Address Spoofing (3081320)", Version 1.1,
https://learn.microsoft.com/en-us/security-updates/securitybulletins/2015/ms15-121

[MSDOCS-TLS-EC-Changes] Microsoft Corporation, "TLS (Schannel SSP) changes in Windows 10 and
Windows Server 2016", https://learn.microsoft.com/en-us/windows-server/security/tls/tls-schannel-
ssp-changes-in-windows-10-and-windows-server

[MSDOCS-TLS-EllipticCurves] Microsoft Corporation, "TLS Elliptic Curves in Windows 10 version 1607
and later", https://learn.microsoft.com/en-us/windows/win32/secauthn/tls-elliptic-curves-in-windows-
10-1607-and-later

[MSDOCS-TLS/SSL-CipherSuites] Microsoft Corporation, "Cipher Suites in TLS/SSL (Schannel SSP)",
https://learn.microsoft.com/en-us/windows/win32/secauthn/cipher-suites-in-schannel

[MSDOCS-TLS/SSLTables] Microsoft Corporation, "Protocols in TLS/SSL (Schannel SSP)",
https://learn.microsoft.com/en-us/windows/win32/secauthn/protocols-in-tls-ssl--schannel-ssp-

[MSKB-5083631] Microsoft Corporation, "April, 2026-KB5083631", April 2026,
https://www.catalog.update.microsoft.com/Search.aspx?q=KB5083631

[RFC5890] Klensin, J., "Internationalized Domain Names for Applications (IDNA): Definitions and
Document Framework", RFC 5890, August 2010, http://rfc-editor.org/rfc/rfc5890.txt

[RFC6066] Eastlake, D., "Transport Layer Security (TLS) Extensions: Extension Definitions", RFC
6066, January 2011, http://www.rfc-editor.org/rfc/rfc6066.txt

[SSL3] Netscape, "SSL 3.0 Specification", November 1996, https://tools.ietf.org/html/draft-ietf-tls-ssl-
version3-00

### 1.3 Overview

The TLS/SSL authentication mechanism (as specified in [RFC8446] and [RFC5246]) is used to
authenticate a server to a client with the option for mutual authentication.

### 1.4 Relationship to Other Protocols

This document is a companion to the TLS/SSL authentication standards [RFC8446] and [RFC5246].

The Transport Layer Security (TLS) Profile implements Server Name Indication (SNI) based on
[RFC4366] where HostName is in UTF-8 format. This behavior is not interoperable with SNI
implementations of [RFC6066] where HostName is a byte string using ASCII encoding without a
trailing dot to support internationalized domain names through the use of A-labels [RFC5890].

7 / 19

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026


### 1.5 Prerequisites/Preconditions

TLS/SSL authentication has the same assumptions as specified in [RFC8446] and in [RFC5246].

### 1.6 Applicability Statement

TLS/SSL authentication is used in environments where the client and server support specifications
[RFC8446] and [RFC5246].

### 1.7 Versioning and Capability Negotiation

Versioning and capability negotiation is handled as specified in [RFC8446] and [RFC5246].

### 1.8 Vendor-Extensible Fields

TLS/SSL authentication contains vendor-extensible fields as specified in [RFC8446] and [RFC5246].

### 1.9 Standards Assignments

 Parameter

 Value

 Reference

Standard TLS/SSL parameters  N/A

http://www.iana.org/assignments/tls-parameters/

TLS extension parameters

N/A

http://www.iana.org/assignments/tls-extensiontype-values/

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

8 / 19


## 2 Messages

### 2.1 Transport

TLS/SSL messages SHOULD be transported as specified in [RFC8446] and [RFC5246].

### 2.2 Message Syntax

The TLS/SSL message syntax SHOULD<2> be as specified in [RFC8446], [RFC5246], [RFC5077], and
[RFC7301].

#### 2.2.1 Client and Server Hello Messages

Cipher suites and capabilities MAY<3> be negotiated as specified in [RFC5487] and SHOULD<4><5>
be negotiated as specified in [RFC8446], [RFC7627], [RFC5246], [RFC2246], [RFC4492], and
[RFC3268].<6>

#### 2.2.2 Alert Messages

The TLS/SSL alert message behavior and formatting SHOULD<7><8> be as specified in [RFC8446]
section 6,  [RFC5246] section 7.2, [RFC2246] section 7.2, [RFC4366] section 4, and [RFC3546]
section 4.

#### 2.2.3 Extended Hello Messages

The TLS extended hello message behavior and formatting SHOULD<9> be as specified in [RFC8446]
section 4.1, [RFC5246] section 7.4.1.4, [RFC4366] sections 2.3 and 3.1, [RFC3546] section 2.3,
[RFC4681] section 2,<10> [RFC5077],<11> [RFC7301],<12> and in [RFC8472].<13>

#### 2.2.4 Certificate Messages

The TLS/SSL certificate message behavior and formatting is specified in [RFC8446] section 4.4,
[RFC5246] sections 7.4.2 and 7.4.6, [RFC2246] sections 7.4.2 and 7.4.6, and [RFC4492] sections 5.3
and 5.6.<14><15>

### 2.3 Directory Service Schema Elements

None.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

9 / 19


## 3 Protocol Details

### 3.1 Common Details

#### 3.1.1 Abstract Data Model

The abstract data model follows what is specified in [RFC8446] and [RFC5246].

#### 3.1.2 Timers

There are no timers except those specified in [RFC8446] and [RFC5246].

#### 3.1.3 Initialization

There is no protocol-specific initialization except what is specified in [RFC8446] and [RFC5246].

#### 3.1.4 Higher-Layer Triggered Events

There are no higher-layer triggered events in common to all parts of this protocol.

#### 3.1.5 Processing Events and Sequencing Rules

Message processing events and sequencing rules SHOULD<16> be as specified in [RFC8446],
[RFC5246], [RFC5077], and [RFC7301].<17> If a client receives an extension type in ServerHello that
it did not request in the associated ClientHello, it MAY<18> abort the handshake. There can be more
than one extension of the same type.

##### 3.1.5.1 GSS_WrapEx() Call

This call is an extension to GSS_Wrap ([RFC2743] section 2.3.3) that passes multiple buffers.

Inputs:







context_handle CONTEXT HANDLE

qop_req INTEGER -- 0 specifies default Quality of Protection (QOP)

input_message ORDERED LIST of:







conf_req_flag BOOLEAN

sign BOOLEAN

data OCTET STRING

Outputs:

  major_status INTEGER

  minor_status INTEGER



output_message ORDERED LIST (in same order as input_message) of:



conf_state BOOLEAN

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

10 / 19






signed BOOLEAN

data OCTET STRING



signature OCTET STRING

This call is identical to GSS_Wrap, except that it supports multiple input buffers. Schannel's binding of
GSS_WrapEx() is such that only the first input buffer will be processed, and the rest ignored.  Thus,
Schannel's binding of GSS_WrapEx() functions just as GSS_Wrap does.

##### 3.1.5.2 GSS_UnwrapEx() Call

This call is an extension to GSS_Unwrap ([RFC2743] section 2.3.4) that passes multiple buffers.

Inputs:





context_handle CONTEXT HANDLE

input_message ORDERED LIST of:







conf_state BOOLEAN

signed BOOLEAN

data OCTET STRING



signature OCTET STRING

Outputs:



qop_req INTEGER, -- 0 specifies default QOP

  major_status INTEGER

  minor_status INTEGER



output_message ORDERED LIST (in same order as input_message) of:





conf_state BOOLEAN

data OCTET STRING

This call is identical to GSS_Unwrap, except that it supports multiple input buffers. Schannel's binding
of GSS_UnwrapEx() is such that only the first input buffer will be processed and the rest ignored. Thus
Schannel's binding of GSS_UnwrapEx() functions just as GSS_Unwrap does.

#### 3.1.6 Timer Events

There are no timer events except those specified in [RFC8446] and [RFC5246].

#### 3.1.7 Other Local Events

There are no local events except those specified in [RFC8446] and [RFC5246].

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

11 / 19


## 4 Protocol Examples

Protocol examples can be found in [RFC8446] section 2, [RFC5246] section 7.3, [RFC4366] section 3,
[RFC4681] section 4, [RFC4492] section 5, and in [RFC7748] section 6.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

12 / 19


## 5 Security

### 5.1 Security Considerations for Implementers

Security considerations are specified in each standard.

### 5.2 Index of Security Parameters

 Security Parameter

 Section

 See Security Considerations for Implementers   5.1

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

13 / 19


## 6 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

The terms "earlier" and "later", when used with a product version, refer to either all preceding
versions or all subsequent versions, respectively. The term "through" refers to the inclusive range of
versions. Applicable Microsoft products are listed chronologically in this section.

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

<1> Section 1: Specification support is listed in the following table. For TLS/SSL version support
tables, see [MSDOCS-TLS/SSLTables]. For more information on support, see Elliptical Curve changes
[MSDOCS-TLS-EC-Changes], Elliptic Curves [MSDOCS-TLS-EllipticCurves], and Cipher Suites
[MSDOCS-TLS/SSL-CipherSuites].

Features

Protocols

Extensions

Groups and Cipher
Suites

Supported by

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

14 / 19


Features

Protocols

Extensions

Hybrid
ECDHE/ML-KEM
Groups for TLS
1.3

TLS 1.3,
[RFC8446]

TLS 1.3

[RFC8446]

Elliptic Curves
and Pre-Shared
Keys for TLS

TLS Extension
for Token
Binding Protocol
Negotiation

TLS Session
Resumption
without Server-
Side State

Groups and Cipher
Suites

[IETFDRAFT-TLS-
ECDHE-MLKEM-04]

Supported by

Windows 11, version 24H2 operating
system and later, Windows Server
2025 and later; see [MSKB-5083631].

Windows 11 client and later

Windows Server 2022 and later

0-RTT resumption mode is not
supported (section 2.3)

Only psk_dhe_ke key exchange mode
is supported (section 4.2.9)

[RFC7748]
(Curve25519 only)

Windows 10 v1607 operating system
and later

[RFC5487]

Windows Server 2016 and later

[RFC8472]

Windows 10 v1507 operating system
and later

Windows Server 2016 and later

Applies to TLS 1.0, TLS 1.1, and TLS
1.2

[RFC5077]

Windows 8.1 and later

Windows Server 2012 R2 and later

Applies to TLS 1.0, TLS 1.1, and TLS
1.2

[RFC5289]

Windows 8 and later

TLS 1.2

[RFC5246]

[RFC7301]
[RFC4366]

TLS 1.1

[RFC4346]

Windows Server 2012 and later

Windows Server 2008 operating
system with Service Pack 2 (SP2); see
[KB4019276]. To enable support for
TLS 1.1 and TLS 1.2, see

[MSDOCS-EnableTLS1.1/2].

Supported on every Windows version

Windows Vista and later

Windows Server 2008 and later

TLS 1.0

[RFC2246]

[RFC4681]
[RFC3546]

[RFC4492]
[RFC3268]

TLS Session
Hash and
Extended
Master Secret
Extension

SSL 3.0

[SSL3]

[RFC7627]

Supported on every Windows version

Windows Vista and later

Windows Server 2008 with SP2 and
later; see [MSDOCS-SB-3081320]

Applies to TLS 1.0, TLS 1.1, and TLS
1.2

Supported on every Windows version

Disabled by default in:

Windows 10 v1607 and later

Windows Server 2016 and later

<2> Section 2.2: [RFC5077] is not supported in Windows XP, through Windows 7 clients and Windows
Server 2003 through Windows Server 2008 R2. Only the client side of [RFC5077] is supported in
Windows 8 and Windows Server 2012.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

15 / 19


[RFC7301] is not supported by Windows XP through Windows 8 clients and Windows Server 2003
through Windows Server 2012.

<3> Section 2.2.1: DHE_PSK or RSA_PSK Key Exchange Algorithms defined in [RFC5487] are not
supported in Windows.

PSK Key Exchange Algorithm or PSK cipher suites in [RFC5487] are not supported in Windows XP
through Windows 10 v1511 operating system clients and  Windows Server 2003 through Windows
Server 2012 R2.

<4> Section 2.2.1: [RFC4492] is not supported in Windows XP and Windows Server 2003. All other
applicable Windows releases support [RFC4492], except for not allowing ECDH cipher suites where the
number of bits used in the public key algorithm is less than the number of bits used in the signing
algorithm.

<5> Section 2.2.1: Transport Layer Security (TLS) Session Hash and Extended Master Secret
Extension [RFC7627] is not supported in Windows XP through Windows 8.1 clients and Windows
Server 2003 through  Windows Server 2012 R2.

<6> Section 2.2.1: Windows accepts a unified format ClientHello message even when SSL version 2 is
disabled.

<7> Section 2.2.2: Windows has a decoupling of the network layer from the TLS/ SSL layer and thus
cannot ensure that alert messages are sent.

<8> Section 2.2.2: Sending and receiving the Certificate Status Request extension from [RFC4366]
and [RFC3546] are not supported by Windows XP and Windows Server 2003.

<9> Section 2.2.3: Sending the Server Name Indications from [RFC4366] and [RFC3546] in the
ClientHello is not supported by Windows XP and Windows Server 2003.

Sending and receiving the Server Name Indications is not supported by Windows XP through Windows
7 clients and Windows Server 2003 through Windows Server 2008 R2.

<10> Section 2.2.3: Sending and receiving the User Mapping extension by using UPN domain hint
from [RFC4681] is supported by Windows.

<11> Section 2.2.3: [RFC5077] is not supported by Windows XP through Windows 7 clients and
Windows Server 2003 through Windows Server 2008 R2. Only the client side of [RFC5077] is
supported by Windows 8 and Windows Server 2012.

<12> Section 2.2.3: [RFC7301] is not supported by Windows XP through Windows 8 clients and
Windows Server 2003, through Windows Server 2012.

<13> Section 2.2.3: Transport Layer Security (TLS) Extension for Token Binding Protocol Negotiation
[RFC8472]  is not supported by Windows XP through Windows 10 v1507 clients and Windows Server
2003 through Windows Server 2012 R2 operating system.

<14> Section 2.2.4: Windows does not require that the signing algorithm used by the issuer of a
certificate match the algorithm in the end certificate. Windows also does not require specific key usage
extension bits to be set in certificates.

<15> Section 2.2.4: Windows omits the root certificate by default when sending certificate chains.

<16> Section 3.1.5: Note the following Windows message processing:



If a session fails during bulk data transfer, Windows does not prevent attempted resumption of the
session.

  Only Windows XP and Windows Server 2003 support and process extensions within the Certificate

Status Request extension.

16 / 19

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026


  Windows does not ignore a HelloRequest received, even in the middle of a handshake.

  Windows Server 2003 does not support fragmentation of incoming messages across frames as is

allowed in [RFC5246] section 6.2.1.

<17> Section 3.1.5: [RFC7301] is not supported by Windows XP through Windows 8 clients and
Windows Server 2003 through Windows Server 2012.

<18> Section 3.1.5: Windows ignores both unrequested and duplicate extensions in both ClientHello
and ServerHello.

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

17 / 19


## 7 Change Tracking

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

1 Introduction

Added TLS 1.3 support for post-quantum hybrid ECDHE-MLKEM key
exchange, introducing X25519MLKEM768, SecP256r1MLKEM768, and
SecP384r1MLKEM1024 groups.

7 Change
Tracking

Added TLS 1.3 support for post-quantum hybrid ECDHE-MLKEM key
exchange, introducing X25519MLKEM768, SecP256r1MLKEM768, and
SecP384r1MLKEM1024 groups.

Revision
class

Major

Major

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

18 / 19


## 8 Index
A

Abstract data model 10
Alert messages 9
Alert Messages message 9
Applicability 8

C

Capability negotiation 8
Certificate messages 9
Certificate Messages message 9
Change tracking 18
Client and Server Hello Messages message 9

D

Data model - abstract 10
Directory service schema elements 9

E

Elements - directory service schema 9
Examples - overview 12
Extended Hello Messages message 9

F

Fields - vendor-extensible 8

G

Glossary 5

H

Hello messages
   client 9
   server 9
Higher-layer triggered events 10

I

Implementer - security considerations 13
Index of security parameters 13
Informative references 7
Initialization 10
Introduction 5

L

Local events 11

M

Message processing
   GSS_UnwrapEx() call 11
   GSS_WrapEx() call 10
   overview 10
Messages

[MS-TLSP] - v20260413
Transport Layer Security (TLS) Profile
Copyright © 2026 Microsoft Corporation
Release: April 13, 2026

   alert 9
   Alert Messages 9
   certificate 9
   Certificate Messages 9
   Client and Server Hello Messages 9
   Extended Hello Messages 9
   hello
      client 9
      server 9
   syntax 9
   transport 9

N

Normative references 5

O

Overview (synopsis) 7

P

Parameters - security index 13
Preconditions 7
Prerequisites 7
Product behavior 14

R

References 5
   informative 7
   normative 5
Relationship to other protocols 7

S

Schema elements - directory service 9
Security
   implementer considerations 13
   parameter index 13
Sequencing rules
   GSS_UnwrapEx() call 11
   GSS_WrapEx() call 10
   overview 10
Standards assignments 8
Syntax 9

T

Timer events 11
Timers 10
Tracking changes 18
Transport 9
Triggered events - higher-layer 10

V

Vendor-extensible fields 8
Versioning 8

19 / 19

