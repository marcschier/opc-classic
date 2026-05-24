[MS-BGPP]:

Border Gateway Protocol (BGP) Profile

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

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 12


Revision Summary

Date

Revision History  Revision Class  Comments

7/9/2020

1.0

4/7/2021

2.0

4/23/2024  3.0

New

Major

Major

Released new document.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 12


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
  - [3.1 Gateway Details](#31-gateway-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Higher-Layer Triggered Events](#314-higher-layer-triggered-events)
    - [3.1.5 Message Processing Events and Sequencing Rules](#315-message-processing-events-and-sequencing-rules)
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

The Border Gateway Protocol (BGP) is an inter-domain routing protocol. The primary function of a BGP
speaking system is to exchange network reachability information with other BGP systems. BGP
reduces the need for manual route configuration on routers because it is a dynamic routing protocol.

In implementations of this profile, BGP is predominantly used to automatically learn routes between
sites that are connected over site-to-site Virtual Private Networks (VPN). This document clarifies the
differences between the profile and the published BGP standard.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

BGP speaker: A router that implements the Border Gateway Protocol (BGP).

Border Gateway Protocol (BGP): An inter-autonomous system routing protocol designed for

TCP/IP routing.

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

[RFC1997] R. Chandra, P. Traina, "BGP Communities Attribute", RFC 1997, August 1996,
https://www.rfc-editor.org/info/rfc1997

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2385] Heffernan, A., "Protection of BGP Sessions via the TCP MD5 Signature Option", RFC2385,
https://www.rfc-editor.org/info/rfc2385

[RFC4271] Rekhter, Y., Li, T., Hares, S., Eds., "A Border Gateway Protocol 4 (BGP-4)", RFC 4271,
https://www.rfc-editor.org/info/rfc4271

#### 1.2.2 Informative References

[MSDOCS-BGP] Microsoft Corporation, "Border Gateway Protocol (BGP)",
https://learn.microsoft.com/en-us/windows-server/remote/remote-access/bgp/border-gateway-
protocol-bgp

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 12


### 1.3 Overview

The Border Gateway Protocol (BGP) Profile does not implement some of the MUST clauses called
out in BGP RFCs. For more details see section 2.2 and section 3.1.5. For more information on BGP see
[MSDOCS-BGP].

### 1.4 Relationship to Other Protocols

The Border Gateway Protocol (BGP) Profile is a subset of the Border Gateway Protocol 4 (BGP-4)
specified in [RFC4271]. BGP Profile relies on BGP Communities Attributes specified in [RFC1997] with
some omissions.

### 1.5 Prerequisites/Preconditions

This profile assumes that an administrator only configures the use of this profile in an applicable
environment.

### 1.6 Applicability Statement

Since this profile is not compliant with the standard BGP-4 requirements specified in [RFC4271], it is
not applicable for use on the global Internet.

### 1.7 Versioning and Capability Negotiation

This profile does not provide any way to discover whether a peer supports this profile.

### 1.8 Vendor-Extensible Fields

None.

### 1.9 Standards Assignments

None.

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

5 / 12


## 2 Messages

### 2.1 Transport

There are no transport deviations from the BGP-4 specification [RFC4271].

### 2.2 Message Syntax

Message syntax is based on the BGP-4 specification [RFC4271].

The deviations from the BGP Communities Attribute specification [RFC1997] are:

The RFC section on "Well-known Communities" states, "The following communities have global
significance and their operations shall be implemented in any community-attribute-aware BGP
speaker": NO_EXPORT (0xFFFFFF01), NO_ADVERTISE (0xFFFFFF02), and
NO_EXPORT_SUBCONFED (0xFFFFFF03). These community operations are not implemented.

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 12


## 3 Protocol Details

### 3.1 Gateway Details

BGP Profile applies to the server side as specified in [RFC4271].

#### 3.1.1 Abstract Data Model

No changes from [RFC4271].

#### 3.1.2 Timers

No changes from [RFC4271].

#### 3.1.3 Initialization

No changes from [RFC4271].

#### 3.1.4 Higher-Layer Triggered Events

No changes from [RFC4271].

#### 3.1.5 Message Processing Events and Sequencing Rules

The deviations from the Border Gateway Protocol 4 (BGP-4) specification [RFC4271] are:

1.  The RFC section on "Security Considerations" states that implementations MUST support TCP MD5

([RFC2385]) for authentication. Authentication is not implemented.

2.  The RFC states, "As part of Phase 3 of the route selection process, the BGP speaker has updated

its Adj-RIBs-Out. All newly installed routes and all newly unfeasible routes for which there is no
replacement route SHALL be advertised to its peers by means of an UPDATE message." This is not
implemented. An External Border Gateway Protocol (EBGP) or (eBGP) learned route will not be re-
advertised to another eBGP peer even if that’s the best route from the decision process.

#### 3.1.6 Timer Events

No changes from [RFC4271].

#### 3.1.7 Other Local Events

No changes from [RFC4271].

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 12


## 4 Protocol Examples

Protocol examples of Boarder Gateway message formats can be found in [RFC4271] section 4.

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 12


## 5 Security

### 5.1 Security Considerations for Implementers

The "Security Considerations" section in specification [RFC4271] states that implementations MUST
support TCP MD5 ([RFC2385]) for authentication. Authentication is not implemented.

### 5.2 Index of Security Parameters

None.

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 12


## 6 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows Server 2012 R2 operating system

  Windows Server 2016 operating system

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

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 12


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

6 Appendix A: Product
Behavior

Added Windows Server 2025 to the list of applicable
products.

Revision
class

Major

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 12


Tracking changes 11
Transport 6

V

Vendor-extensible fields 5
Versioning 5

## 8 Index
A

Applicability 5

C

Capability negotiation 5
Change tracking 11

F

Fields - vendor-extensible 5

G

Glossary 4

I

Implementer - security considerations 9
Index of security parameters 9
Informative references 4
Introduction 4

M

Messages
   transport 6

N

Normative references 4

O

Overview (synopsis) 5

P

Parameters - security index 9
Preconditions 5
Prerequisites 5
Product behavior 10

R

References 4
   informative 4
   normative 4
Relationship to other protocols 5

S

Security
   implementer considerations 9
   parameter index 9
Standards assignments 5

T

[MS-BGPP] - v20240423
Border Gateway Protocol (BGP) Profile
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 12

