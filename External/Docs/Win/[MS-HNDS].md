[MS-HNDS]:

Host Name Data Structure Extension

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

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 13

Revision Summary

Date

Revision
History

Revision
Class

Comments

10/24/2008  0.1

12/5/2008

0.2

New

Minor

Version 0.1 release

Clarified the meaning of the technical content.

1/16/2009

0.2.1

Editorial

Changed language and formatting in the technical content.

2/27/2009

0.2.2

Editorial

Changed language and formatting in the technical content.

4/10/2009

0.2.3

Editorial

Changed language and formatting in the technical content.

5/22/2009

0.2.4

Editorial

Changed language and formatting in the technical content.

7/2/2009

0.2.5

Editorial

Changed language and formatting in the technical content.

8/14/2009

0.2.6

Editorial

Changed language and formatting in the technical content.

9/25/2009

1.0

Major

Updated and revised the technical content.

11/6/2009

1.0.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  1.0.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

1.1

Minor

Clarified the meaning of the technical content.

3/12/2010

1.1.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

1.1.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

1.1.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

1.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

1.2

Minor

Clarified the meaning of the technical content.

9/23/2011

1.2

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  2.0

Major

Updated and revised the technical content.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 13

Date

Revision
History

Revision
Class

Comments

3/30/2012

3.0

Major

Updated and revised the technical content.

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

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 13

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
Relationship to Protocols and Other Structures ...................................................... 6
Applicability Statement ....................................................................................... 6
Versioning and Localization ................................................................................. 6
Vendor-Extensible Fields ..................................................................................... 7

1.3
1.4
1.5
1.6
1.7

2  Structures ............................................................................................................... 8
Extended Host Name .......................................................................................... 8

2.1

3  Structure Examples ................................................................................................. 9

4  Security Considerations ......................................................................................... 10

5  Appendix A: Product Behavior ............................................................................... 11

6  Change Tracking .................................................................................................... 12

7  Index ..................................................................................................................... 13

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 13

1  Introduction

The Host Name Data Structure Extension Protocol specifies the extension to the allowable host names
that can be assigned to a computer.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

1.1  Glossary

This document uses the following terms:

ASCII: The American Standard Code for Information Interchange (ASCII) is an 8-bit character-
encoding scheme based on the English alphabet. ASCII codes represent text in computers,
communications equipment, and other devices that work with text. ASCII refers to a single 8-bit
ASCII character or an array of 8-bit ASCII characters with the high bit of each character set to
zero.

Augmented Backus-Naur Form (ABNF): A modified version of Backus-Naur Form (BNF),

commonly used by Internet specifications. ABNF notation balances compactness and simplicity
with reasonable representational power. ABNF differs from standard BNF in its definitions and
uses of naming rules, repetition, alternatives, order-independence, and value ranges. For more
information, see [RFC5234].

client: A computer on which the remote procedure call (RPC) client is executing.

Domain Name System (DNS): A hierarchical, distributed database that contains mappings of
domain names to various types of data, such as IP addresses. DNS enables the location of
computers and services by user-friendly names, and it also enables the discovery of other
information stored in the database.

host name: The name of a physical server, as described in [RFC952].

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

[RFC1123] Braden, R., "Requirements for Internet Hosts - Application and Support", RFC 1123,
October 1989, https://www.rfc-editor.org/info/rfc1123

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 13

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC3629] Yergeau, F., "UTF-8, A Transformation Format of ISO 10646", STD 63, RFC 3629,
November 2003, https://www.rfc-editor.org/info/rfc3629

[RFC5234] Crocker, D., Ed., and Overell, P., "Augmented BNF for Syntax Specifications: ABNF", STD
68, RFC 5234, January 2008, https://www.rfc-editor.org/info/rfc5234

[RFC952] Harrenstien, K., Stahl, M., and Feinler, E., "DOD Internet Host Table Specification", RFC
952, October 1985, https://www.rfc-editor.org/info/rfc952

1.2.2  Informative References

[ICANN] Internet Corporation for Assigned Names and Numbers, "DNS Stability: The Effect of New
Generic Top Level Domains on the Internet Domain Name System", February 2008,
http://www.icann.org/en/topics/dns-stability-draft-paper-06feb08.pdf

[MS-NBTE] Microsoft Corporation, "NetBIOS over TCP (NBT) Extensions".

[RFC1034] Mockapetris, P., "Domain Names - Concepts and Facilities", STD 13, RFC 1034, November
1987, https://www.rfc-edit.org/info/rfc1034

[RFC1035] Mockapetris, P., "Domain Names - Implementation and Specification", STD 13, RFC 1035,
November 1987, https://www.rfc-editor.org/info/rfc1035

[RFC2181] Elz, R., and Bush, R., "Clarifications to the DNS Specification", RFC 2181, July 1997,
https://www.rfc-editor.org/info/rfc2181

[RFC3493] Gilligan, R., Thomson, S., Bound, J., McCann, J., and Stevens, W., "Basic Socket Interface
Extensions for IPv6", RFC 3493, February 2003, https://www.rfc-editor.org/info/rfc3493

1.3  Overview

A host name is a string assigned to a computer to identify itself and to differentiate itself from other
hosts on the network. The syntax for a host name was first defined in [RFC952] and was subsequently
updated in [RFC1123] section 2.1.

This document extends that syntax to allow underscores and non-ASCII characters.

1.4  Relationship to Protocols and Other Structures

Various protocols use host names in their own protocols and it is the responsibility of those protocols
to state whether they use the standard host name syntax, or this extended syntax.

One protocol worth noting is the DNS protocol [RFC1034] [RFC1035] [RFC2181], which does not
depend on host names in any way. The DNS protocol uses DNS names, which allow binary labels, and
hence inherently supports host names as well as names that would not be legal host names.

Note  This document does not apply to NetBIOS names, which are instead discussed in [MS-NBTE].

1.5  Applicability Statement

A computer is typically configured with a host name which is used to uniquely identify that computer.
That is, hosts can identify one another through the host names.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 13

1.6  Versioning and Localization

There is no versioning or localization support in this structure.

1.7  Vendor-Extensible Fields

The host name structure does not contain any vendor-extensible fields.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 13

2  Structures

2.1  Extended Host Name

The extended host name syntax is a UTF-8 [RFC3629] string specified by the following ABNF
[RFC5234].

 hname = name *("." name)
 name = 1*63let-dig-hyp-und
 let-dig-hyp-und = ALPHA / DIGIT / UTF8-2 / UTF8-3 / UTF8-4 / "-" / "_"

where UTF8-2, UTF8-3, and UTF8-4 are as specified in [RFC3629] section 4. In addition, the entire
extended host name MUST be at most 255 bytes long.

An implementation MAY<1> disallow a string where a substring constructed from the "name" rule
does not contain at least one non-DIGIT character.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 13

3  Structure Examples

The following strings are all examples of extended host names.

 "my_computer.contoso.com"
 "my_computer"
 "_123"
  "0x123"
 "-"
 "-._.-._.-"

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 13

4  Security Considerations

Because the string "0x123" is a valid extended host name, there might be security issues depending
on how client software interprets such strings. For example, as discussed in [ICANN], the inet_addr()
method of the classic sockets Application Programming Interface (API) will interpret these strings as
string representations of an IP address, and as discussed in [RFC3493] section 6.1, the getaddrinfo()
method of the sockets API will perform a simple conversion of strings accepted by inet_addr(), instead
of trying to resolve the name using any type of name resolution service. This could redirect the client
software to an address other than an address registered for that host name. As such, great care needs
to be taken before using an extended host name that could be interpreted as a hexadecimal number.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 13

5  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows NT operating system

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

<1> Section 2.1: Windows NT, Windows 2000, Windows XP, Windows Server 2003, Windows Vista,
Windows Server 2008, Windows 7, and Windows Server 2008 R2 operating system follow this
behavior.

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 13

6  Change Tracking

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

5 Appendix A: Product
Behavior

Added Windows Server 2025 to the list of applicable
products.

Revision
class

Major

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 13

7  Index
A

Applicability 6

C

Change tracking 12

D

Details - extended host name 8

E

Example 9
Examples 9
Extended host name 8

F

Fields - vendor-extensible 7

G

Glossary 5

H

Host names - extended 8

I

Implementer - security considerations 10
Informative references 6
Introduction 5

L

Localization 6

N

Normative references 5

O

Overview (synopsis) 6

P

Product behavior 11

R

References 5
   informative 6
   normative 5
Relationship to protocols and other structures 6

S

[MS-HNDS] - v20240423
Host Name Data Structure Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

Security 10
Security - implementer considerations 10
Structures - extended host name 8

T

Tracking changes 12

V

Vendor-extensible fields 7
Versioning 6

13 / 13

