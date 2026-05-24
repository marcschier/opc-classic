[MS-PROPSTORE]:

Property Store Binary File Format

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

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 14


Revision Summary

Date

Revision
History

Revision
Class

7/16/2010

1.0

8/27/2010

1.0

10/8/2010

1.0

11/19/2010  1.0

1/7/2011

1.0

2/11/2011

1.0

3/25/2011

1.0

5/6/2011

1.0

New

None

None

None

None

None

None

None

Comments

First Release.

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

6/17/2011

1.1

Minor

Clarified the meaning of the technical content.

9/23/2011

1.1

12/16/2011  1.1

3/30/2012

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

7/12/2012

1.2

Minor

Clarified the meaning of the technical content.

10/25/2012  1.2

1/31/2013

1.2

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

2.0

Major

Updated and revised the technical content.

11/14/2013  2.0

2/13/2014

2.0

5/15/2014

2.0

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

3.0

Major

Significantly changed the technical content.

10/16/2015  3.0

7/14/2016

3.0

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
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

6/1/2017

3.0

9/15/2017

4.0

9/12/2018

5.0

4/7/2021

6.0

6/25/2021

7.0

4/23/2024

8.0

None

Major

Major

Major

Major

Major

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 14


## Table of Contents

- [1 Introduction](#1-introduction)
  - [1.1 Glossary](#11-glossary)
  - [1.2 References](#12-references)
    - [1.2.1 Normative References](#121-normative-references)
    - [1.2.2 Informative References](#122-informative-references)
  - [1.3 Overview](#13-overview)
  - [1.4 Relationship to Protocols and Other Structures](#14-relationship-to-protocols-and-other-structures)
  - [1.5 Applicability Statement](#15-applicability-statement)
  - [1.6 Versioning and Localization](#16-versioning-and-localization)
  - [1.7 Vendor-Extensible Fields](#17-vendor-extensible-fields)
- [2 Structures](#2-structures)
  - [2.1 Serialized Property Store](#21-serialized-property-store)
  - [2.2 Serialized Property Storage](#22-serialized-property-storage)
  - [2.3 Serialized Property Value](#23-serialized-property-value)
    - [2.3.1 Serialized Property Value (String Name)](#231-serialized-property-value-string-name)
    - [2.3.2 Serialized Property Value (Integer Name)](#232-serialized-property-value-integer-name)
- [3 Structure Examples](#3-structure-examples)
- [4 Security Considerations](#4-security-considerations)
- [5 Appendix A: Product Behavior](#5-appendix-a-product-behavior)
- [6 Change Tracking](#6-change-tracking)
- [7 Index](#7-index)

## 1 Introduction

This document specifies the Microsoft Property Store Binary File Format. This file format is a
persistence format for a set of properties.  Implementers can use this file format to store a set of
properties in a file or within another structure.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

### 1.1 Glossary

This document uses the following terms:

globally unique identifier (GUID): A term used interchangeably with universally unique

identifier (UUID) in Microsoft protocol technical documents (TDs). Interchanging the usage of
these terms does not imply or require a specific algorithm or mechanism to generate the value.
Specifically, the use of this term does not imply or require that the algorithms described in
[RFC4122] or [C706] have to be used for generating the GUID. See also universally unique
identifier (UUID).

little-endian: Multiple-byte values that are byte-ordered with the least significant byte stored in

the memory location with the lowest address.

Unicode: A character encoding standard developed by the Unicode Consortium that represents

almost all of the written languages of the world. The Unicode standard [UNICODE5.0.0/2007]
provides three forms (UTF-8, UTF-16, and UTF-32) and seven schemes (UTF-8, UTF-16, UTF-16
BE, UTF-16 LE, UTF-32, UTF-32 LE, and UTF-32 BE).

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

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[MS-OLEPS] Microsoft Corporation, "Object Linking and Embedding (OLE) Property Set Data
Structures".

[MS-SHLLINK] Microsoft Corporation, "Shell Link (.LNK) Binary File Format".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

#### 1.2.2 Informative References

None.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 14


### 1.3 Overview

This structure provides a compact way to serialize one or more property sets.  Each property set
consists of a property set identifier and one or more property values.  Each property value consists of
a unique property name and an associated value.  Each property name can be either an unsigned
integer or, in the case of a special property set identifier, a Unicode string.

This structure does not specify the semantics of properties or the assignment of property set
identifiers or property names.

Data in this file format is stored in little-endian format.

### 1.4 Relationship to Protocols and Other Structures

This structure is used by the Shell Link (.LNK) Binary File Format, as specified in [MS-SHLLINK].

### 1.5 Applicability Statement

This document specifies a persistence format for one or more sets of property identifiers and
associated property values.  This persistence format is applicable when each property set can be
identified by a globally unique identifier (GUID), and when each property within a property set can
be identified by an unsigned integer or a Unicode string name and can be persisted as a
TypedPropertyValue structure, as specified in [MS-OLEPS] section 2.15.

### 1.6 Versioning and Localization

None.

### 1.7 Vendor-Extensible Fields

Implementers are free to define new Format IDs within the Serialized Property Storage structure, as
defined in section 2.2, and to define new property identifiers within a Serialized Property Value
structure, as defined in section 2.3.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 14


## 2 Structures

This document references commonly used data types as defined in [MS-DTYP].

Unless otherwise qualified, instances of GUID in this section refer to [MS-DTYP] section 2.3.4.

### 2.1 Serialized Property Store

The Property Store Binary File Format is a sequence of Serialized Property Storage structures. The
sequence MUST be terminated by a Serialized Property Storage structure that specifies 0x00000000
for the Storage Size field.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Store Size

Serialized Property Storage (variable)

...

Store Size (4 bytes): An unsigned integer that specifies the total size, in bytes, of this structure,

excluding the size of this field.

Serialized Property Storage (variable): A sequence of one or more Serialized Property Storage

structures, as specified in section 2.2.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Serialized Property Storage - 1

...

Serialized Property Storage - N

Serialized Property Storage with Storage Size value of 0x00000000

### 2.2 Serialized Property Storage

The Serialized Property Storage structure is a sequence of Serialized Property Value structures.  The
sequence MUST be terminated by a Serialized Property Value structure that specifies 0x00000000 for
the Value Size field.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Storage Size

Version

7 / 14

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


Format ID

...

...

...

Serialized Property Value (variable)

...

Storage Size (4 bytes): An unsigned integer that specifies the total size, in bytes, of this structure.

It MUST be 0x00000000 if this is the last Serialized Property Storage in the enclosing Serialized
Property Store.

Version (4 bytes): Has to be equal to 0x53505331.

Format ID (16 bytes): A GUID that specifies the semantics and expected usage of the properties

contained in this Serialized Property Storage structure. It MUST be unique in the set of serialized
property storage structures.

Serialized Property Value (variable): A sequence of one or more property values.  If the Format

ID field is equal to the GUID {D5CDD505-2E9C-101B-9397-08002B2CF9AE}, then all values in
the sequence MUST be Serialized Property Value (String Name) structures, as specified in section
2.3.1; otherwise, all values MUST be Serialized Property Value (Integer Name) structures, as
specified in section 2.3.2. The last Serialized Property Value in the sequence MUST specify
0x00000 for the Value Size.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Serialized Property Value - 1

...

Serialized Property Value - N

Serialized Property Value with Value Size of 0x00000000

### 2.3 Serialized Property Value

There are two types of Serialized Property Value structures: Serialized Property Value (String Name)
structures and Serialized Property Value (Integer Name) structures.

#### 2.3.1 Serialized Property Value (String Name)

The Serialized Property Value (String Name) structure specifies a single property within a Serialized
Property Storage structure, where the property is identified by a unique Unicode string.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 14


0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Value Size

Name Size

Reserved

Name (variable)

...

Value (variable)

...

Value Size (4 bytes): An unsigned integer that specifies the total size, in bytes, of this structure. It

MUST be 0x00000000 if this is the last The Serialized Property Value in the enclosing Serialized
Property Storage structure.

Name Size (4 bytes): An unsigned integer that specifies the size, in bytes, of the Name field,

including the null-terminating character.

Reserved (1 byte): Has to be 0x00.

Name (variable): A null-terminated Unicode string that specifies the identity of the property. It has

to be unique within the enclosing Serialized Property Storage structure.

Value (variable): A TypedPropertyValue structure, as specified in [MS-OLEPS] section 2.15.

#### 2.3.2 Serialized Property Value (Integer Name)

The Serialized Property Value (Integer Name) structure specifies a single property within a Serialized
Property Storage structure, where the property is identified by a unique unsigned integer.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Reserved

Value Size

Id

...

Value (variable)

Value Size (4 bytes): An unsigned integer that specifies the total size, in bytes, of this structure. It

MUST be 0x00000000 if this is the last Serialized Property Value in the enclosing Serialized
Property Storage structure.

Id (4 bytes): An unsigned integer that specifies the identity of the property. It MUST be unique

within the enclosing Serialized Property Storage structure.

Reserved (1 byte): MUST be 0x00.

Value (variable): A TypedPropertyValue structure, as specified in [MS-OLEPS] section 2.15.

9 / 14

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


## 3 Structure Examples

None.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 14


## 4 Security Considerations

None.

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 14


## 5 Appendix A: Product Behavior

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

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 14


## 6 Change Tracking

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

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 14


## 7 Index
A

Applicability 6

C

Change tracking 13
Common data types and fields 7

D

Data types and fields - common 7
Details
   common data types and fields 7

E

Examples 10

F

Fields - vendor-extensible 6

G

Glossary 5

I

Implementer - security considerations 11
Informative references 5
Introduction 5

L

Localization 6

N

Normative references 5

O

Overview (synopsis) 6

P

Product behavior 12

R

References 5
   informative 5
   normative 5
Relationship to other protocols 6
Relationship to protocols and other structures 6

S

Security - implementer considerations 11

[MS-PROPSTORE] - v20240423
Property Store Binary File Format
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

Serialized Property Value structures 8
Serialized_Property_Storage packet 7
Serialized_Property_Store packet 7
Serialized_Property_Value_Integer_Name packet 9
Serialized_Property_Value_String_Name packet 8
Structures 7
   overview 7

T

Tracking changes 13

V

Vendor-extensible fields 6
Versioning 6

14 / 14

