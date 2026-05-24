[MS-XOPP]:

XML-binary Optimized Packaging (XOP) Profile

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

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 14

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

Initial Availability

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

0.1.5

Editorial

Changed language and formatting in the technical content.

8/14/2009

0.1.6

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

12/16/2011  1.0

3/30/2012

1.0

None

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 14

Date

Revision
History

Revision
Class

Comments

technical content.

7/12/2012

1.0

10/25/2012  1.0

1/31/2013

1.0

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

1.1

Minor

Clarified the meaning of the technical content.

11/14/2013  1.1

2/13/2014

1.1

5/15/2014

1.1

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

4/23/2024

7.0

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

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 14

Table of Contents

1.3

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
MIME Parts Ordering in Multipart/Related XOP Package Extension ....................... 6
Relationship to Other Protocols ............................................................................ 6
Prerequisites/Preconditions ................................................................................. 6
Applicability Statement ....................................................................................... 6
Versioning and Capability Negotiation ................................................................... 6
Vendor-Extensible Fields ..................................................................................... 7
Standards Assignments ....................................................................................... 7

1.4
1.5
1.6
1.7
1.8
1.9

1.3.1

2  Messages ................................................................................................................. 8
Transport .......................................................................................................... 8
Message Syntax ................................................................................................. 8
Ordering of the MIME Parts in XOP Packages .................................................... 8

2.1
2.2

2.2.1

3  Protocol Details ....................................................................................................... 9

4  Protocol Examples ................................................................................................. 10
MIME Multipart/Related XOP Package Ordering .................................................... 10

4.1

5  Security ................................................................................................................. 11
Security Considerations for Implementers ........................................................... 11
Index of Security Parameters ............................................................................ 11

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 12

7  Change Tracking .................................................................................................... 13

8  Index ..................................................................................................................... 14

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 14

1  Introduction

XML-binary Optimized Packaging (XOP), as specified in [XML-XOP], defines a method for the
efficient serialization of XML Information Sets (XML Infosets) that have certain types of content.
The XML-binary Optimized Packaging (XOP) Profile extends XOP to allow for the creation of more
efficient implementations that process XML Infosets. This document, [MS-XOPP], describes the
serialization rules for XML Infosets as MIME Multipart/Related XOP packages but does not specify
how these XML Infosets are transmitted between network nodes.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

Multipurpose Internet Mail Extensions (MIME): A set of extensions that redefines and

expands support for various types of content in email messages, as described in [RFC2045],
[RFC2046], and [RFC2047].

SOAP message: An XML document consisting of a mandatory SOAP envelope, an optional SOAP
header, and a mandatory SOAP body. See [SOAP1.2-1/2007] section 5 for more information.

streaming: The act of processing a part of an XML Infoset without requiring that the entire XML

Infoset be available.

XML Information Set (Infoset): An abstract data set that provides a consistent set of definitions

for use in specifications that refer to the information in a well-formed XML document, as
described in [XMLINFOSET].

XML-binary Optimized Packaging (XOP): The packaging convention that efficiently serializes

XML Infosets that contain certain types of content, as described in [XML-XOP].

XOP Information Set (XOP Infoset): An XML Infoset in which optimized content has been
removed and replaced by <xop:Include> SOAP element information items, as described in
[XML-XOP].

XOP package: A package that offers an alternate serialization of an XML Infoset and that contains
the XOP document and any optimized content from the original XML Infoset, as described in
[XML-XOP].

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

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 14

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2387] Levinson, E., "The MIME Multipart/Related Content-type", RFC 2387, August 1998,
https://www.rfc-editor.org/info/rfc2387

[XML-XOP] Gudgin, M., Mendelsohn, N., Nottingham, M., and Ruellan, H., "XML-binary Optimized
Packaging", January 25, 2005, http://www.w3.org/TR/2005/REC-xop10-20050125

1.2.2  Informative References

[SOAP-MTOM] Gudgin, M., Medelsohn, N., Nottingham, M., and Ruellan, H., "SOAP Message
Transmission Optimization Mechanism", W3C Recommendation, 25 January 2005,
http://www.w3.org/TR/2005/REC-soap12-mtom-20050125/

1.3  Overview

The XML-binary Optimized Packaging (XOP) Profile provides extensions that enable more efficient
implementations of [XML-XOP] to be built by requiring certain ordering of the Multipurpose Internet
Mail Extensions (MIME) XML Information Set (XML Infoset) parts in the XOP package.

1.3.1  MIME Parts Ordering in Multipart/Related XOP Package Extension

The standard XOP implementation, as specified in [XML-XOP] section 4.1, is not allowed to consider
the ordering of MIME parts to be significant to XOP processing or to the construction of the XOP
Information Set (XOP Infoset) for MIME Multipart/Related packaging. The XML-binary Optimized
Packaging (XOP) Profile extends the MIME Multipart/Related packaging mechanism specified in [XML-
XOP] to allow for the ordering of the MIME parts, as described in section 2.2.1. These extensions
enable the creation of more efficient implementations for processing an XML Infoset packaged in
MIME Multipart/Related XOP packages when streaming.

1.4  Relationship to Other Protocols

The XML-binary Optimized Packaging (XOP) Profile is an extension of [XML-XOP]. The extensions
specified in this document [MS-XOPP] do not introduce any new protocol relationships beyond those
specified in [XML-XOP] Appendix A.

1.5  Prerequisites/Preconditions

There are no prerequisites or preconditions beyond those specified in [XML-XOP] Appendix A.

1.6  Applicability Statement

The MIME Parts Ordering in Multipart/Related XOP Package Extension specified in section 1.3.1 is
applicable when an XOP Infoset packaged in a MIME Multipart/Related XOP package is processed in
streaming fashion.

These extensions are not applicable to XOP packaging mechanisms other than MIME and those that do
not specify their own packaging mechanism.

If broad interoperability with implementations strictly compliant with [XML-XOP] is desired, these
extensions might not be a suitable choice.

1.7  Versioning and Capability Negotiation

There is no versioning or capability negotiation beyond that specified in [XML-XOP].

6 / 14

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1.8  Vendor-Extensible Fields

There are no vendor-extensible fields beyond those specified in [XML-XOP].

1.9  Standards Assignments

There are no standards assignments beyond those specified in [XML-XOP].

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 14

2  Messages

2.1  Transport

This specification defines only serialization rules for XOP packages and does not define how XOP
packages are transmitted on the network. As such, it does not have a transport.

2.2  Message Syntax

Except as specified in section 2.2.1, the syntax used for specifying MIME Multipart/Related XOP
packaging is as specified in [XML-XOP] section 3, [XML-XOP] section 4.1, and [RFC2387].

2.2.1  Ordering of the MIME Parts in XOP Packages

The extensions provided by the XML–binary Optimized Packaging (XOP) Profile override the following
text located in [XML-XOP] section 4.1:

"Except for purposes of determining the root MIME part, as specified by [RFC2387], ordering of MIME
parts MUST NOT be considered significant to XOP processing or to the construction of the XOP
Infoset."

In streaming mode negotiated through a process that is out of band to this protocol, the root MIME
part MUST appear first in a MIME Multipart/Related XOP package, and the subsequent MIME parts
MUST appear in the order in which they appear in the corresponding XML Infoset.<1>

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 14

3  Protocol Details

The XML-binary Optimized Packaging (XOP) Profile does not introduce any new protocol roles or
change any existing protocol roles that are defined in [XML-XOP].

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 14

4  Protocol Examples

4.1  MIME Multipart/Related XOP Package Ordering

The XML-binary Optimized Packaging (XOP) Profile does not introduce any new protocol roles or
change any existing protocol roles that are defined in [XML-XOP]. Examples of how MIME
Multipart/Related XOP packages are ordered are provided in [XML-XOP] section 1.2.

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 14

5  Security

5.1  Security Considerations for Implementers

Security considerations are the same as those specified in [XML-XOP] section 6.

5.2  Index of Security Parameters

None.

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 14

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

Windows Releases

  Windows XP operating system Service Pack 2 (SP2)

  Windows Server 2003 operating system with Service Pack 1 (SP1)

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

<1> Section 2.2.1: The Windows Web Services API element provides a buffered and streaming
programming model for exchanging and processing SOAP messages encoded as specified in [SOAP-
MTOM] between network nodes. When the Windows Web Services API's streaming programming
model is used to create SOAP messages, it follows the MIME parts ordering requirements specified in
section 2.2.1.

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 14

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

Added Windows Server 2025  to the list of applicable
products.

Revision
class

Major

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 14

   informative 6
   normative 5
Relationship to other protocols 6

S

Security
   implementer considerations 11
   parameter index 11
Standards assignments 7
Syntax 8

T

Tracking changes 13
Transport 8

V

Vendor-extensible fields 7
Versioning 6

8  Index
A

Applicability 6

C

Capability negotiation 6
Change tracking 13

E

Examples - MIME multipart/related XOP package

ordering 10

F

Fields - vendor-extensible 7

G

Glossary 5

I

Implementer - security considerations 11
Index of security parameters 11
Informative references 6
Introduction 5

M

Messages
   syntax 8
   transport 8
MIME multipart/related XOP package ordering

example 10

MIME parts - ordering of
   in multipart/related XOP package extension 6
   in XOP packages 8

N

Normative references 5

O

Overview (synopsis) 6

P

Parameters - security index 11
Preconditions 6
Prerequisites 6
Product behavior 12
Protocol Details 9
   overview 9

R

References 5

[MS-XOPP] - v20240423
XML-binary Optimized Packaging (XOP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 14

