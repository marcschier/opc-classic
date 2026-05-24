[MS-UPMC]:

UPnP Device and Service Templates: Media Property and
Compatibility Extensions

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

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 30


Revision Summary

Date

Revision
History

Revision
Class

Comments

1/29/2010

0.1

Major

First Release.

3/12/2010

0.1.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

0.1.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

0.1.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

0.1.3

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

1.0

Major

Updated and revised the technical content.

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

6/17/2011

1.1

Minor

Clarified the meaning of the technical content.

9/23/2011

1.1

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  2.0

Major

Updated and revised the technical content.

3/30/2012

2.0

7/12/2012

2.0

10/25/2012  2.0

1/31/2013

2.0

8/8/2013

3.0

11/14/2013  3.1

2/13/2014

4.0

5/15/2014

4.0

None

None

None

None

Major

Minor

Major

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

5.0

Major

Significantly changed the technical content.

2 / 30

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


Date

Revision
History

Revision
Class

Comments

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

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 30


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
  - [2.1 MMPE](#21-mmpe)
    - [2.1.1 Artist Properties](#211-artist-properties)
      - [2.1.1.1 artistAlbumArtist](#2111-artistalbumartist)
      - [2.1.1.2 artistPerformer](#2112-artistperformer)
      - [2.1.1.3 artistConductor](#2113-artistconductor)
    - [2.1.2 Author Properties](#212-author-properties)
      - [2.1.2.1 authorComposer](#2121-authorcomposer)
      - [2.1.2.2 authorOriginalLyricist](#2122-authororiginallyricist)
      - [2.1.2.3 authorWriter](#2123-authorwriter)
    - [2.1.3 Ratings Properties](#213-ratings-properties)
      - [2.1.3.1 userRating](#2131-userrating)
      - [2.1.3.2 userEffectiveRating](#2132-usereffectiverating)
      - [2.1.3.3 userRatingInStars](#2133-userratinginstars)
      - [2.1.3.4 userEffectiveRatingInStars](#2134-usereffectiveratinginstars)
    - [2.1.4 serviceProvider](#214-serviceprovider)
    - [2.1.5 sourceURL](#215-sourceurl)
    - [2.1.6 year](#216-year)
    - [2.1.7 folderPath](#217-folderpath)
    - [2.1.8 fileIdentifier](#218-fileidentifier)
  - [2.2 MCEF](#22-mcef)
    - [2.2.1 Compatibility Flag Values and Behaviors](#221-compatibility-flag-values-and-behaviors)
      - [2.2.1.1 WMC_COMPAT_EXCLUDE_HTTP](#2211-wmccompatexcludehttp)
      - [2.2.1.2 WMC_COMPAT_EXCLUDE_RTSP](#2212-wmccompatexcludertsp)
      - [2.2.1.3 WMC_COMPAT_EXCLUDE_DLNA](#2213-wmccompatexcludedlna)
      - [2.2.1.4 WMC_COMPAT_EXCLUDE_DLNA_1_5](#2214-wmccompatexcludedlna15)
      - [2.2.1.5 WMC_COMPAT_EXCLUDE_PCMPARAMS](#2215-wmccompatexcludepcmparams)
      - [2.2.1.6 WMC_COMPAT_EXCLUDE_WMDRMND](#2216-wmccompatexcludewmdrmnd)
      - [2.2.1.7 WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO](#2217-wmccompatincludertspforvideo)
      - [2.2.1.8 WMC_COMPAT_EXCLUDE_WMALOSSLESS_NONTRANSCODED](#2218-wmccompatexcludewmalosslessnontranscoded)
      - [2.2.1.9 WMC_COMPAT_EXCLUDE_SEARCH](#2219-wmccompatexcludesearch)
      - [2.2.1.10 WMC_COMPAT_DO_NOT_LIMIT_RESPONSE_SIZE](#22110-wmccompatdonotlimitresponsesize)
      - [2.2.1.11 WMC_COMPAT_EXCLUDE_VIDEO_TRANSCODING](#22111-wmccompatexcludevideotranscoding)
      - [2.2.1.12 WMC_COMPAT_PLAYLIST_FAKECHILDCOUNT](#22112-wmccompatplaylistfakechildcount)
      - [2.2.1.13 WMC_COMPAT_EXCLUDE_NONPCM_AUDIO_TRANSCODING](#22113-wmccompatexcludenonpcmaudiotranscoding)
      - [2.2.1.14 WMC_COMPAT_EXCLUDE_TRANSCODING_TO_MPEG2](#22114-wmccompatexcludetranscodingtompeg2)
      - [2.2.1.15 WMC_COMPAT_EXCLUDE_RES_FILTERING](#22115-wmccompatexcluderesfiltering)
  - [2.3 MPME](#23-mpme)
    - [2.3.1 Magic Packets](#231-magic-packets)
      - [2.3.1.1 microsoft:magicPacketWakeSupported](#2311-microsoftmagicpacketwakesupported)
      - [2.3.1.2 microsoft:magicPacketSendSupported](#2312-microsoftmagicpacketsendsupported)
  - [2.4 Microsoft ProtocolInfo Extensions](#24-microsoft-protocolinfo-extensions)
    - [2.4.1 PlayToApp Extension](#241-playtoapp-extension)
- [3 Structure Examples](#3-structure-examples)
  - [3.1 MMPE Examples](#31-mmpe-examples)
    - [3.1.1 Artist Properties Tags](#311-artist-properties-tags)
    - [3.1.2 Author Properties Tags](#312-author-properties-tags)
    - [3.1.3 Ratings Properties Tags](#313-ratings-properties-tags)
    - [3.1.4 serviceProvider Property Tag](#314-serviceprovider-property-tag)
    - [3.1.5 year Property Tag](#315-year-property-tag)
    - [3.1.6 folderPath Property Tag](#316-folderpath-property-tag)
    - [3.1.7 fileIdentifier Property Tag](#317-fileidentifier-property-tag)
  - [3.2 MCEF Examples](#32-mcef-examples)
    - [3.2.1 X_DeviceCaps Example](#321-xdevicecaps-example)
  - [3.3 MPME Examples](#33-mpme-examples)
    - [3.3.1 magicPacketWakeSupported](#331-magicpacketwakesupported)
    - [3.3.2 magicPacketSendSupported](#332-magicpacketsendsupported)
  - [3.4 Microsoft ProtocolInfo Extensions Examples](#34-microsoft-protocolinfo-extensions-examples)
    - [3.4.1 PlayToApp Extension](#341-playtoapp-extension)
- [4 Security Considerations](#4-security-considerations)
- [5 Appendix A: Product Behavior](#5-appendix-a-product-behavior)
- [6 Change Tracking](#6-change-tracking)
- [7 Index](#7-index)

## 1 Introduction

This document defines the Microsoft Media Property Extensions (MMPE), the Microsoft
Compatibility Extension Flags (MCEF), the Microsoft Power Management Extensions
(MPME), and the Microsoft ProtocolInfo Extensions. These flags and extensions extend the Universal
Plug and Play (UPnP) interoperability guidelines, as specified by the UPnP Forum [UPnP] and used
by the Digital Living Network Alliance (DLNA) [DLNA].

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

### 1.1 Glossary

This document uses the following terms:

device description document (DDD): A document used to specify device information and its

capabilities, as described in the UPnP standard [UPnP].

digital item declaration language (DIDL-Lite): A subset of the Digital Item Declaration

Language (DIDL), which is an XML dialect developed within ISO/MPEG21.

Digital Living Network Alliance (DLNA): A cross-industry organization of leading consumer

electronics, computing industry, and mobile device companies, which are focused on delivering
interoperability guidelines to allow entertainment devices in the home to operate with each
other. DLNA has embraced WMM for its QoS strategy.

Digital Media Player (DMP): A device class defined in the DLNA Guidelines. A DMP is an UPnP
control point, which means that it invokes UPnP actions on UPnP devices. The DMP is not itself
a UPnP Device.

Digital Media Renderer (DMR): A Device Class defined in the DLNA Guidelines. A DMR is

UPnP Device that implements the UPnP MediaRenderer Device type.

Digital Media Server (DMS): A device class defined in the DLNA Guidelines. A DMS is an UPnP

device that implements the UPnP MediaServer device type.

DLNA guidelines: The DLNA Networked Device Interoperability Guidelines [DLNA] consist of three
volumes that provide vendors with the information required to build interoperable networked
platforms and devices for the digital home, including architecture and protocols, profiles for
media formats, and link protection.

Hypertext Transfer Protocol (HTTP): An application-level protocol for distributed, collaborative,
hypermedia information systems (text, graphic images, sound, video, and other multimedia
files) on the World Wide Web.

Microsoft Compatibility Extension Flags (MCEF): Flags provided by DMRs and DMPs to filter

specific res elements exposed to these devices.

Microsoft Media Property Extensions (MMPE): Additional metadata properties that describe a

media item that further enriches the metadata properties defined by [UPnP].

Microsoft Power Management Extensions (MPME): XML tags used to communicate the

devices support of magic packets.

Microsoft ProtocolInfo Extensions: Extensions to the ProtocolInfo syntax, defined by

Microsoft.

ProtocolInfo: As specified in [UPNPCNMGR] section 2.5.2.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 30


Real-Time Streaming Protocol (RTSP): A protocol used for transferring real-time multimedia

data (for example, audio and video) between a server and a client, as specified in [RFC2326]. It
is a streaming protocol; this means that RTSP attempts to facilitate scenarios in which the
multimedia data is being simultaneously transferred and rendered (that is, video is displayed
and audio is played).

res: Identifies a resource. A resource is typically some type of a binary asset, such as a photo,

song, video, etc. A res element contains an URI that identifies the resource.

Universal Plug and Play (UPnP): A set of computer network protocols, published by the UPnP

Forum [UPnP], that allow devices to connect seamlessly and that simplify the implementation of
networks in home (data sharing, communications, and entertainment) and corporate
environments. UPnP achieves this by defining and publishing UPnP device control protocols built
upon open, Internet-based communication standards.

UTF-8: A byte-oriented standard for encoding Unicode characters, defined in the Unicode standard.

Unless specified otherwise, this term refers to the UTF-8 encoding form specified in
[UNICODE5.0.0/2007] section 3.9.

XML: The Extensible Markup Language, as described in [XML1.0].

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

[DLNA] Digital Living Network Alliance, "The DLNA Networked Device Interoperability Guidelines",
https://spirespark.com/dlna/guidelines

Note Registration is required to download the document.

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC3629] Yergeau, F., "UTF-8, A Transformation Format of ISO 10646", STD 63, RFC 3629,
November 2003, https://www.rfc-editor.org/info/rfc3629

[RFC5234] Crocker, D., Ed., and Overell, P., "Augmented BNF for Syntax Specifications: ABNF", STD
68, RFC 5234, January 2008, https://www.rfc-editor.org/info/rfc5234

[UPnP] UPnP Forum, "Standards", http://upnp.org/sdcps-and-certification/standards/sdcps/

[XML10] World Wide Web Consortium, "Extensible Markup Language (XML) 1.0 (Third Edition)",
February 2004, http://www.w3.org/TR/2004/REC-xml-20040204/

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 30


#### 1.2.2 Informative References

[MS-DLNHND] Microsoft Corporation, "Digital Living Network Alliance (DLNA) Networked Device
Interoperability Guidelines: Microsoft Extensions".

[UPNPARCH1] UPnP Forum, "UPnP Device Architecture 1.0", October 2008,
http://www.upnp.org/specs/arch/UPnP-arch-DeviceArchitecture-v1.0.pdf

[UPNPCDS1] UPnP Forum, "ContentDirectory:1 Service Template Version 1.01", June 2002,
http://www.upnp.org/standardizeddcps/documents/ContentDirectory1.0.pdf

[UPNPCNMGR] UPnP Forum, "ConnectionManager:1 Service Template Versions 1.01", June 2002,
http://www.upnp.org/standardizeddcps/documents/ConnectionManager1.0.pdf

### 1.3 Overview

Microsoft Compatibility Extension Flags (MCEF) are used to request specific device behavior(s) to
enhance interoperability among UPnP Devices. For example, device manufacturers can develop a
UPnP capable network device in order to retrieve information about the media content that is exposed
to the network by a digital media server (DMS). Such a device is enabled to include the MCEFs in
its device description document (DDD) or User-Agent Header in order to tailor the metadata
attributes in the XML responses that are provided by the DMS during interaction for enhanced
interoperability.

Microsoft Media Property Extensions (MMPE) are used to expose several media content
properties that are defined by Microsoft. These properties are generally not expressible, or are difficult
to express, with the existing UPnP metadata attributes. In order to provide the expected query results
with the existing attributes, additional processing capabilities are required on the device. For example,
by using the additional properties defined by Microsoft, the device is relieved of the "heavy-lifting"
involved in generating similar results and is enabled to provide a richer media browsing experience to
their users.

Microsoft Power Management Extensions (MPME) are used to expose UPnP Device capabilities
for power management. For example, a UPnP Device can support sleep mode and accept wake
requests from other UPnP Devices. At the same time, a UPnP Device can support waking other UPnP
Devices.

The Microsoft ProtocolInfo Extensions are extensions to the ProtocolInfo, defined by Microsoft.
For example, a Digital Media Renderer (DMR) can use the PlayToApp extension (section 2.4.1) to
indicate that it supports a nonstandard URI scheme.

### 1.4 Relationship to Protocols and Other Structures

MMPE are extensions to the XML syntax called DIDL-Lite, which are defined in the UPnP
ContentDirectory service specification. For more information, see [UPNPCDS1] Appendix A.

MPME are extensions to the XML syntax for UPnP DDDs. The DDDs are defined in [UPNPARCH1]
section 2.1.

MMPE, MCEF, and MPME can be used in implementations of the Digital Living Network Alliance (DLNA)
Home Networked Device Interoperability Guidelines: Microsoft Extensions [MS-DLNHND].

### 1.5 Applicability Statement

The MMPE and MCEF are only applicable to implementations of the Microsoft Extensions to the
Digital Living Network Alliance (DLNA) Home Networked Device Interoperability Guidelines [MS-
DLNHND].

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 30


The MPME is only applicable to implementations of the Microsoft Extensions to the Digital Living
Network Alliance (DLNA) Home Networked Device Interoperability Guidelines.

Implementations of the MMPE and/or MCEF and/or MPME are not required in order to interoperate with
Windows. The Media Property and Compatibility Extensions enhance the Digital Living Network Alliance
(DLNA) Home Networked Device Interoperability Guidelines and the Remote Media Streaming
Initiation Protocol without breaking interoperability.

Implementers are recommended to consider whether or not the MMPE and/or MCEF and/or MPME are
applicable to their scenarios in order to provide a richer multimedia experience.

### 1.6 Versioning and Localization

The MMPE, MCEF, and MPME do not support versioning.

The MMPE, MCEF, and MPME do not explicitly address localization. However, these Microsoft
Extensions use the UTF-8 character set, as specified in [DLNA] section 7.2.5.9 and [RFC3629].

### 1.7 Vendor-Extensible Fields

None.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 30


## 2 Structures

This protocol references commonly used data types, as defined in [MS-DTYP].

### 2.1 MMPE

Each MMPE is a property that describes a media item, where all but one of the properties can be
included in a DIDL-Lite XML document. In the case of the property that is not included, the name of
the property is used as the name of an XML tag that is included within a <desc> XML tag, which itself
is included within an <item> tag that contains all of the tags associated with the media item.

The properties can also be specified as input parameters to the UPnP ContentDirectory [UPNPCDS1]
service Search and Browse actions, wherever those actions allow properties to be used. For example,
the ContentDirectory service Search action allows property names to be used to formulate a search
query.





The sourceURL property (section 2.1.5) cannot appear in DIDL-Lite

The XML tags MUST follow the syntax rules for XML defined in [XML10]. Unless otherwise
specified, any characters in the value of an XML tag MUST be escaped, using the XML escaping
mechanism defined in [XML10] section 2.4.



The syntax of each tag is specified using ABNF [RFC5234].

#### 2.1.1 Artist Properties

A group of XML properties that supports various artist roles, which is included in the multimedia
content, such as album artist, performer (also known as a contributing artist), and conductor. Support
for the artist properties enables devices to provide results for queries such as "browse by performer,
and then find albums by the selected performer".

##### 2.1.1.1 artistAlbumArtist

This property specifies the name of an artist that is listed on the album when the media item is part of
an album (i.e., a music album). For some albums, the name of the album artist can differ from the
name of the artist of individual songs.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:artistAlbumArtist"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.1.2 artistPerformer

This property specifies the name of a performing artist that is associated with the media item.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:artistPerformer"; UTF-8

PropertyValue = ALPHA; UTF-8

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 30


MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.1.3 artistConductor

This property specifies the name of a conductor (typically, a musical conductor) that is associated with
the media item.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:artistConductor"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.2 Author Properties

A group of properties that supports various roles that one or more authors can have in multimedia
content: Composer, Original Lyricist, or Writer. Support for the Author attributes enables devices to
provide results for queries such as "browse by composer, and then find genres for the selected
composer".

##### 2.1.2.1 authorComposer

Specifies the name of a composer (typically, a musical composer) that is associated with the media
item.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:authorComposer"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.2.2 authorOriginalLyricist

This property specifies the name of a person who created the original lyrics of the media item.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:authorOriginalLyricist"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.2.3 authorWriter

This property specifies the name of a person who is considered the writer of the media item.

Multiple instances of this property are allowed.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 30


The syntax of the property is defined as follows:

PropertyName = "microsoft:authorWriter"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.3 Ratings Properties

The Ratings properties support metadata that is supplied from both the user and from the metadata
provider. These properties allow devices to provide "prioritized" or "scoped" results to end users based
on their preferences. This enables a user, for example, to choose to browse only musical content that
has been rated at "5 or more stars".

There are four Ratings-related properties:





userRating and userEffectiveRating represent the user rating (if present) and an automatic rating,
respectively. The values of the userRating and userEffectiveRating properties are expressed as a
number, in the range between 0 and 99, inclusive.

userRatingInStars and userEffectiveRatingInStars represent the same information as userRating
and userEffectiveRating, respectively. The values of the userRatingInStars and
userEffectiveRatingInStars properties are expressed as a number, in the range between 0 and 5,
inclusive.

At most, one instance of each property is allowed.

##### 2.1.3.1 userRating

The syntax of the property is defined as follows:

PropertyName = "microsoft:userRating"; UTF-8

PropertyValue = 1*2DIGIT; unsigned integer ranges from 0 to 99

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.3.2 userEffectiveRating

The syntax of the property is defined as follows:

PropertyName = "microsoft:userEffectiveRating"; UTF-8

PropertyValue = 1*2DIGIT; unsigned integer ranges from 0 to 99

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

##### 2.1.3.3 userRatingInStars

The syntax of the property is defined as follows:

PropertyName = "microsoft:userRatingInStars"; UTF-8

PropertyValue = "0" / "1" / "2" / "3" / "4" / "5"; unsigned integer ranges from 0 to 5

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 30


##### 2.1.3.4 userEffectiveRatingInStars

The syntax of the property is defined as follows:

PropertyName = "microsoft:userEffectiveRatingInStars"; UTF-8

PropertyValue = "0" / "1" / "2" / "3" / "4" / "5"; unsigned integer ranges from 0 to 5

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.4 serviceProvider

The serviceProvider property represents the name of the distributor of the media item. The
serviceProvider property allows devices to return results categorized by the entity that provided the
content.

Multiple instances of this property are allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:serviceProvider"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.5 sourceURL

The sourceURL property supports the grouping of results by file system path, which enables a device
to group media files by parent folder. For example, the device can return results that enumerate all of
the pictures in the "January Ski Trip to Canada" folder.

Note  The use of the "ParentID" attribute can provide results similar to sourceURL, but only for a
container hierarchy that replicates the file system hierarchy.

At most, one instance of each property is allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:sourceURL"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.6 year

The year property enables the organization of multimedia content based solely on year, whereas the
commonly-used "dc:date" UPnP property provides the whole date. For example, use of the year
property enables a device to more easily return results for all of the pictures taken in "2006".

The syntax of the property is defined as follows:

PropertyName = "microsoft:year"; UTF-8

PropertyValue = 4DIGIT; unsigned integer, format YYYY

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 30


#### 2.1.7 folderPath

The folderPath property represents a folder path to the media item, relative to a root folder.

At most, one instance of each property is allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:folderPath"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

#### 2.1.8 fileIdentifier

The fileIdentifier property is obtained from the media file. The property is assigned by the creator of
the media file.<1>

For example, if the same file is shared by two different DMS implementations, although the value of
the "id" attribute on the "item" property can be different on each DMS, the value of the fileIdentifier
property is the same, since the value is obtained from the file itself.

At most, one instance of each property is allowed.

The syntax of the property is defined as follows:

PropertyName = "microsoft:fileIdentifier"; UTF-8

PropertyValue = ALPHA; UTF-8

MMPE = "<" PropertyName ">" PropertyValue "</" PropertyName ">";

### 2.2 MCEF

MCEFs are used to define specific behavior(s) for a particular device in order to enhance
interoperability. The flags can be provided by a DMP, or Digital Media Renderer (DMR) via its
User-Agent header, as described in [MS-DLNHND] or its DDD, as defined in [UPNPARCH1] section 2.1.
To provide the flag, the device MUST specify the Microsoft-defined microsoft:X_DeviceCaps tag within
the Device node of the DDD, as shown in section 3.2.1. The syntax to follow for this tag is specified
using ABNF [RFC5234], as follows:

CompatibilityFlag= 1*DIGIT; it is the decimal numerical representation of the bitwise-OR operation
among the hexadecimal flags used, as defined in section 2.2.1;

 <microsoft:X_DeviceCaps xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-
0"/>CompatibilityFlag</microsoft:X_DeviceCaps>

Note  The Microsoft-defined microsoft:X_DeviceCaps attribute does not change in any respect the
manner regarding how DDDs are made available to other devices in the network as, described by
[UPNPARCH1].

#### 2.2.1 Compatibility Flag Values and Behaviors

Any flag value that is not documented in this section is considered reserved and SHOULD not be used.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 30


##### 2.2.1.1 WMC_COMPAT_EXCLUDE_HTTP

This flag causes a DMS to exclude res and upnp:albumArtURI elements in the DIDL-Lite response of
the Browse or Search UPnP actions that implement the HTTP protocol in their URLs, as specified in
[UPnP].

Value: %x1

This flag affects the response of UPnP Actions: ContentDirectory:Browse,
ContentDirectory:Search, and ConnectionManager:GetProtocolInfo.

 <res duration="0:03:20.000" bitrate="24000" protocolInfo="http-
get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=015000000000000000000000000000
00" sampleFrequency="44100" bitsPerSample="16" nrAudioChannels="2"
microsoft:codec="{00000055-0000-0010-8000-00AA00389B71}" xmlns:microsoft="urn:schemas-
microsoft-com:WMPNSS-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/0_ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFG
M0M5N30uMC40.mp3</res>
 <upnp:albumArtURI dlna:profileID="JPEG_TN" xmlns:dlna="urn:schemas-dlna-org:metadata-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFGM0
M5N30uMC40.jpg?albumArt=true,formatID=13</upnp:albumArtURI>

When this flag is specified, the value of the protocolInfo attribute, as defined in [UPNPCDS1] section
6, on res elements, only specifies non-HTTP transport-dependent formats. Any res element with a
protocolInfo attribute whose value begins with an "http-get" MUST be excluded from the response. For
more information regarding the syntax of the ProtocolInfo string, see [UPNPCNMGR].

Note  This flag MUST NOT be used in combination with WMC_COMPAT_EXCLUDE_RTSP.

##### 2.2.1.2 WMC_COMPAT_EXCLUDE_RTSP

This flag causes a DMS to exclude res elements in the DIDL-Lite response of the Browse or Search
UPnP actions that implement the RTSP protocol in their URLs, as specified by [UPnP]. Elements in the
DIDL-Lite response, as demonstrated in the following syntax block, MUST be removed with the use of
this flag.

Value: %x2

This flag affects the response of UPnP Actions: ContentDirectory:Browse,
ContentDirectory:Search, and ConnectionManager:GetProtocolInfo.

 <res duration="0:03:20.000" bitrate="24000" protocolInfo="rtsp-rtp-
udp:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=10;DLNA.ORG_FLAGS=831000000000000000000000000000
00;DLNA.ORG_MAXSP=5" sampleFrequency="44100" bitsPerSample="16" nrAudioChannels="2"
microsoft:codec="{00000055-0000-0010-8000-00AA00389B71}" xmlns:microsoft="urn:schemas-
microsoft-com:WMPNSS-1-
0/">rtsp://127.0.0.1:554/WMPNSSv4/2815481477/0_ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFGM0
M5N30uMC40.mp3</res>

A consequence of constructing the DIDL-Lite response as shown in this example is that the value of
the protocolInfo attribute, as defined in [UPNPCDS1] section 6, on res elements MUST specify only
non-RTSP transport-dependent formats. Any res element with a protocolInfo attribute whose value
begins with "rtsp-rtp-udp" MUST be excluded from the response. For more information regarding the
syntax of the ProtocolInfo string, see [UPNPCNMGR].

Note  This flag MUST NOT be used in combination with WMC_COMPAT_EXCLUDE_HTTP because at
least one protocol (either HTTP or RTSP) SHOULD be enabled. This flag MUST NOT be used in
combination with WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 30


##### 2.2.1.3 WMC_COMPAT_EXCLUDE_DLNA

This flag causes a DMS to exclude all DLNA-defined attributes and parameters in the DIDL-Lite
responses given by the Browse or Search UPnP actions, as specified by [UPnP].

Value: %x4

This flag affects the response of UPnP Actions: ContentDirectory:Browse,
ContentDirectory:Search, and ConnectionManager:GetProtocolInfo.

Note  The dlna:profileID and xmlns:dlna attributes MUST NOT be included in the upnp:albumArtURI
nodes.

 <upnp:albumArtURI dlna:profileID="JPEG_SM" xmlns:dlna="urn:schemas-dlna-org:metadata-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/0_ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFG
M0M5N30uMC40.jpg?albumArt=true</upnp:albumArtURI>

The following content feature flags MUST also be removed from the protocolInfo attribute, as defined
in [UPNPCDS1] section 6, of res elements:

  DLNA.ORG_PN

  DLNA.ORG_OP

  DLNA.ORG_PS

  DLNA.ORG_CI

  DLNA.ORG_FLAGS

  DLNA.ORG_MAXSP

 <res duration="0:03:20.000" bitrate="24000" protocolInfo="http-
get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=015000000000000000000000000000
00" sampleFrequency="44100" bitsPerSample="16" nrAudioChannels="2"
microsoft:codec="{00000055-0000-0010-8000-00AA00389B71}" xmlns:microsoft="urn:schemas-
microsoft-com:WMPNSS-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/0_ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFG
M0M5N30uMC40.mp3</res>

Because some devices fall down if the string DLNA exists in the protocol response, the
WMC_COMPAT_EXCLUDE_DLNA_1_5 compatibility flag MUST be set to indicate that the specified
attributes have not been included in the upnp:albumArtURI and res nodes. Setting this compatibility
flag causes the server to not include the DLNA string as specified in the XML response.

Moreover, because DLNA support is non-existent if the WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO
flag has been set, it MUST be ignored. By setting this flag, RTSP-based URLs will be excluded from res
nodes in the case of video content.

Note  In order to preserve backwards compatibility, when the WMC_COMPAT_EXCLUDE_RTSP flag is
included within the descriptor <desc> tag, it can be escaped as described in [MS-DLNHND] section
3.4.5.1.

In the case of the UPnP action ConnectionManager:GetProtocolInfo, all format support information
MUST NOT contain protocol information that announces DLNA profiles, as specified in [DLNA].

 http-get:*:audio/x-ms-wma:DLNA.ORG_PN=WMABASE

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 30


Note  The XML tags can be escaped, as described in section 3.1 of this document, when this
compatibility flag is used.

##### 2.2.1.4 WMC_COMPAT_EXCLUDE_DLNA_1_5

This flag causes a DMS to map the DLNA Profile ID "MP3X" to "MP3", and to map the DLNA Profile IDs
"WMVSPLL_BASE" and "WMVSPML_BASE" to "WMVMED_BASE". In addition, this flag causes the DLNA
Profile ID for any media item whose DLNA Profile ID begins with "WMDRM_" to be omitted. When this
flag is used, all URLs MUST have a file extension, such as ".mp3" for MP3 content and ".pcm" for LPCM
content.

Value: %x8

This flag affects the response of UPnP Actions: ContentDirectory:Browse,
ContentDirectory:Search, and ConnectionManager:GetProtocolInfo.

 <res duration="0:03:20.000" bitrate="24000" protocolInfo="http-
get:*:audio/mpeg:DLNA.ORG_PN=MP3;DLNA.ORG_OP=01;DLNA.ORG_FLAGS=015000000000000000000000000000
00" sampleFrequency="44100" bitsPerSample="16" nrAudioChannels="2"
microsoft:codec="{00000055-0000-0010-8000-00AA00389B71}" xmlns:microsoft="urn:schemas-
microsoft-com:WMPNSS-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/1_ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFG
M0M5N30uMC40.mp3</res>

Note  The XML tags can be escaped, as described in section 3.1 of this document, when this
compatibility flag is used.

##### 2.2.1.5 WMC_COMPAT_EXCLUDE_PCMPARAMS

This flag causes a DMS to exclude sample rate and channels information from the protocolInfo
attribute in the res element for audio/L16 and audio/L8 MIME types.

Value: %x10

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

 <res duration="0:03:20.000" bitrate="176400" protocolInfo="http-
get:*:audio/L16;rate=44100;channels=2:DLNA.ORG_PN=LPCM;DLNA.ORG_OP=10;DLNA.ORG_CI=1;DLNA.ORG_
FLAGS=01500000000000000000000000000000" sampleFrequency="44100" bitsPerSample="16"
nrAudioChannels="2" microsoft:codec="{00000001-0000-0010-8000-00AA00389B71}"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-
0/">http://127.0.0.1:10243/WMPNSSv4/2815481477/ezI0OTI3N0I5LUU2MUQtNDdEMi05MTI3LTJBNjFDOTFGM0
M5N30uMC40?formatID=20</res>

##### 2.2.1.6 WMC_COMPAT_EXCLUDE_WMDRMND

This flag causes a DMS to not generate res tags where the URL requires the use of WMDRM-ND.

Value: %x20

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 30


##### 2.2.1.7 WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO

This flag causes a DMS to include res tags with RTSP-based URLs for video even if
WMC_COMPAT_EXCLUDE_RTSP and/or WMC_COMPAT_EXCLUDE_DLNA_1_5 are set.

Value: %x40

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.8 WMC_COMPAT_EXCLUDE_WMALOSSLESS_NONTRANSCODED

This flag causes a DMS to not generate res tags with URLs for non-transcoded WMA Lossless content.
This flag MUST be ignored if used in combination with WMC_COMPAT_EXCLUDE_RES_FILTERING.

Value: %x80

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.9 WMC_COMPAT_EXCLUDE_SEARCH

This flag causes the ContentDirectory:Search UPnP action to not be supported for this DMS. In order
to accomplish this action, ContentDirectory:GetSearchCapabilities MUST NOT provide any search
capabilities.

Value: %x100

This flag affects the response of UPnP Actions: ContentDirectory:Search and
ContentDirectory:GetSearchCapabilities.

##### 2.2.1.10 WMC_COMPAT_DO_NOT_LIMIT_RESPONSE_SIZE



This flag causes the DIDL-Lite responses to not be limited to 200 kB in size for these UPnP actions.

Value: %x400

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.11 WMC_COMPAT_EXCLUDE_VIDEO_TRANSCODING



This flag causes a DMS to not offer res tags with URLs that correspond to transcoded versions of
video items. This flag MUST be ignored if used in combination with
WMC_COMPAT_EXCLUDE_RES_FILTERING.

Value: %x800

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.12 WMC_COMPAT_PLAYLIST_FAKECHILDCOUNT



This flag causes a value of "1" to be returned for the child count of playlists containers. This flag is
used to counteract the reduction in performance caused by calculating the "real" child count for
playlist container.

Value: %x1000

18 / 30

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.13 WMC_COMPAT_EXCLUDE_NONPCM_AUDIO_TRANSCODING



This flag causes res tags with URLs that correspond to transcoded versions of audio items to not be
offered, unless the res tag is for an audio item transcoded into LPCM. This flag MUST be ignored if
used in combination with WMC_COMPAT_EXCLUDE_RES_FILTERING.

Value: %x2000

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.14 WMC_COMPAT_EXCLUDE_TRANSCODING_TO_MPEG2



This flag causes res tags with URLs that correspond to transcoded versions of video items into the
MPEG-2 format to not be offered. This flag MUST be ignored if used in combination with
WMC_COMPAT_EXCLUDE_RES_FILTERING.

Value: %x4000

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

##### 2.2.1.15 WMC_COMPAT_EXCLUDE_RES_FILTERING



All DIDL-Lite responses from a DMS MUST contain all applicable transcoding res elements associated
to the requested media.

Value: %x8000

This flag affects the response of UPnP Actions: ContentDirectory:Browse and
ContentDirectory:Search.

### 2.3 MPME

MPMEs are used to signal support for specific behaviors for a particular device in order to enhance
power management. The MPME XML tags can be provided by a DMR or a DMS in its DDD, as
described in [UPNPARCH1] section 2.1. This section defines the MPME XML tags.

#### 2.3.1 Magic Packets

This section defines MPME XML tags for indicating support for magic packets. For more information
about magic packets, see [MS-DLNHND] section 2.2.5.

##### 2.3.1.1 microsoft:magicPacketWakeSupported

This is an XML tag that a UPnP device provides in its DDD to announce that it can be awakened by
the use of a magic packet.<2>

For more information about the syntax of magic packet, see [MS-DLNHND] section 2.2.5.

An UPnP device MUST only provide this information if it implements a low power mode of operation.

An UPnP device that can be woken from a low power mode MUST provide the following tag in its DDD:

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 30


flag = "1" / "0"; true or false, respectively.

 <microsoft:magicPacketWakeSupported xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-
0">flag</microsoft:magicPacketWakeSupported>

##### 2.3.1.2 microsoft:magicPacketSendSupported

This is an XML tag that a UPnP device provides in its DDD to announce whether the device is able to
wake a sleeping UPnP device.<3> The following syntax shows how the element is used to announce
this capability.

flag = "1" / "0"; true or false respectively.

 <microsoft:magicPacketSendSupported xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-
0">flag</microsoft:magicPacketSendSupported>

### 2.4 Microsoft ProtocolInfo Extensions

The Microsoft ProtocolInfo Extensions extend the syntax of the ProtocolInfo syntax element, as
described in [UPNPCNMGR] section 2.5.2.

#### 2.4.1 PlayToApp Extension

The PlayToApp ProtocolInfo extension can be used by DMR devices to specify support for additional
URI schemes beyond those defined in [UPNPCNMGR] sections 5.1.1 through 5.1.4.<4>

The PlayToApp ProtocolInfo extension adheres to the extension mechanism for ProtocolInfo as defined
in [UPNPCNMGR] section 5.1.5.

The syntax of the extension is defined as follows:

CustomURIScheme = 1*( ALPHA / DIGIT / "-" )

Protocol = "microsoft.com"

Network = "*"

ContentFormat = "application/vnd.ms-playtoapp;target=" DQUOTE CustomURIScheme  DQUOTE

AdditionalInfo = "*"

PlayToApp = Protocol ":" Network ":" ContentFormat ":" AdditionalInfo

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 30


## 3 Structure Examples

### 3.1 MMPE Examples

In order to preserve backwards compatibility, XML tags inside the descriptor <desc> tag can be
escaped. For more information, refer to [MS-DLNHND] section 3.4.5.1. For example,
"<microsoft:artistAlbumArtist>" can appear as "&lt;microsoft:artistAlbumArtist&gt;" in the XML
response.

#### 3.1.1 Artist Properties Tags

The following is an example of the usage of the Artist properties in an XML response:

 <desc id="Artist"  nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:artistAlbumArtist>Album</microsoft:artistAlbumArtist>
     <microsoft:artistPerformer>Performer</microsoft:artistPerformer>
     <microsoft:artistConductor>Conductor</microsoft:artistConductor>
 </desc>

#### 3.1.2 Author Properties Tags

The following is an example of the usage of the Author properties in an XML response:

 <desc id="Author" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:authorComposer>Composer</microsoft:authorComposer>
     <microsoft:authorOriginalLyricist>Lyricist</microsoft:authorOriginalLyricist>
     <microsoft:authorWriter>Writer</microsoft:authorWriter>
 </desc>

#### 3.1.3 Ratings Properties Tags

The following is an example of the usage of the Ratings properties in an XML response:

 <desc id="UserRating" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:userRating>50</microsoft:userRating>
     <microsoft:userEffectiveRating>60</microsoft:userEffectiveRating>
     <microsoft:userRatingInStars>3</microsoft:userRatingInStars>
     <microsoft:userEffectiveRatingInStars>3</microsoft:userEffectiveRatingInStars>
 </desc>

#### 3.1.4 serviceProvider Property Tag

The following is an example of the usage of the serviceProvider property in an XML response:

 <desc id="ServiceProvider" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:serviceProvider>Service Provider</microsoft:serviceProvider>
 </desc>

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

21 / 30


#### 3.1.5 year Property Tag

The following is an example of the usage of the year property in an XML response:

 <desc id="Year" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:year>2004</microsoft:year>
 </desc>

#### 3.1.6 folderPath Property Tag

The following is an example of the usage of the folderPath property in an XML response: <5>

 <desc id="folderPath" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
    <microsoft:folderPath>Audio\Songs</microsoft:folderPath>
 </desc>

#### 3.1.7 fileIdentifier Property Tag

The following is an example of the usage of the fileIdentifier property (section 2.1.8) in an XML
response:

 <desc id="fileInfo" nameSpace="urn:schemas-microsoft-com:WMPNSS-1-0/"
xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0/">
     <microsoft:fileIdentifier>AMGt_id=T   821854;AMGp_id=P     4744;AMGa_id=R
78327;X_id={5E8A0C00-0500-11DB-89CA-0019B92A3933};XA_id={F7310100-0400-11DB-89CA-
0019B92A3933};XAP_id={88120000-0600-11DB-89CA-0019B92A3933}</microsoft:fileIdentifier>
 </desc>

### 3.2 MCEF Examples

#### 3.2.1 X_DeviceCaps Example

Device compatibility flags are specified in a DDD by using the following format:

 <device>
     <UDN>uuid:00000000-1111-2222-3333-444444444444</UDN>
     <friendlyName>Sample Renderer</friendlyName>
     <deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType>
     <manufacturer>Microsoft</manufacturer>
     ...
     <microsoft:X_DeviceCaps xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-
0">94</microsoft:X_DeviceCaps>
 </device>

Use of microsoft:X_DeviceCaps requires that the Microsoft XML namespace (urn:schemas-microsoft-
com:WMPNSS-1-0) be specified in the DDD. The value for microsoft:X_DeviceCaps is given in decimal
and represents a bitwise OR combination of the individual compatibility flag values described in section
2.2. In this example, the value of "94" for microsoft:X_DeviceCaps indicates the following
compatibility flags:

  WMC_COMPAT_EXCLUDE_RTSP

  WMC_COMPAT_EXCLUDE_DLNA

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

22 / 30


  WMC_COMPAT_EXCLUDE_DLNA_1_5

  WMC_COMPAT_EXCLUDE_PCMPARAMS

  WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO

### 3.3 MPME Examples

#### 3.3.1 magicPacketWakeSupported

The following example shows a DDD that announces that the UPnP device supports
microsoft:magicPacketWakeSupported (section 2.3.1.1). <6>

 <device>
     <UDN>uuid:00000000-1111-2222-3333-444444444444</UDN>
     <friendlyName>Sample Renderer</friendlyName>
     <deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType>
     <manufacturer>Microsoft</manufacturer>
     ...
     <microsoft:magicPacketWakeSupported
       xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0">1
       </microsoft:magicPacketWakeSupported>
 </device>

#### 3.3.2 magicPacketSendSupported

The following example shows a DDD that announces that the UPnP device supports
microsoft:magicPacketSendSupported (section 2.3.1.2). <7>

 <device>
     <UDN>uuid:00000000-1111-2222-3333-444444444444</UDN>
     <friendlyName>Sample Renderer</friendlyName>
     <deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType>
     <manufacturer>Microsoft</manufacturer>
     ...
     <microsoft:magicPacketSendSupported
       xmlns:microsoft="urn:schemas-microsoft-com:WMPNSS-1-0">1
       </microsoft:magicPacketSendSupported>
 </device>

### 3.4 Microsoft ProtocolInfo Extensions Examples

#### 3.4.1 PlayToApp Extension

The following example shows a ProtocolInfo string (section 2.4.1) that a DMR device can include in its
SinkProtocolInfo state variable ([UPNPCNMGR] section 2.2.2) to declare that it supports playing media
using the "example://" URI schemes.

 microsoft.com:*:application/vnd.ms-playtoapp;target="example":*

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

23 / 30


## 4 Security Considerations

The MMPE and MCEF do not introduce any new security considerations beyond those that already
apply to XML-based formats. Therefore, the same security considerations that pertain to the UPnP
and DLNA guidelines also apply.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

24 / 30


## 5 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

  Windows Vista operating system

  Windows 7 operating system

  Windows Home Server 2011 server software

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

<1> Section 2.1.8: The Microsoft:fileIdentifier XML tag is not supported in Windows Vista, Windows 7,
and Windows Home Server 2011.

<2> Section 2.3.1.1: The microsoft:magicPacketWakeSupported XML tag is not supported in
Windows Vista.

<3> Section 2.3.1.2: The microsoft:magicPacketSendSupported XML tag is not supported in Windows
Vista.

<4> Section 2.4.1: The PlayToApp ProtocolInfo extension is not supported in Windows Vista,
Windows 7, Windows Home Server 2011, and Windows 8.

<5> Section 3.1.6: The FolderPath XML tag is not supported in Windows Vista.

<6> Section 3.3.1: The microsoft:magicPacketWakeSupported XML tag is not supported in Windows
Vista.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

25 / 30


<7> Section 3.3.2: The microsoft:magicPacketSendSupported XML tag is not supported in Windows
Vista.

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

26 / 30


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

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

27 / 30


## 7 Index
A

Applicability 8
Artist properties 10
Artist properties tags example 21
artistAlbumArtist properties 10
artistConductor properties 11
artistPerformer properties 10
Author properties 11
Author properties tags example 21
authorComposer properties 11
authorOriginalLyricist properties 11
authorWriter properties 11

C

Change tracking 26
Common data types and fields 10
Compatibility flags - values and behaviors 14

D

Data types and fields - common 10
Details
   artist properties 10
   artistAlbumArtist properties 10
   artistConductor properties 11
   artistPerformer properties 10
   author properties 11
   authorComposer properties 11
   authorOriginalLyricist properties 11
   authorWriter properties 11
   common data types and fields 10
   compatibility flags - values and behaviors 14
   fileIdentifier property 14
   flags - values and behaviors 14
   folderPath properties 14
   magic packets 19
   MCEF property 14
   microsoft:magicPacketSendSupported tag 20
   microsoft:magicPacketWakeSupported tag 19
   MMPE property 10
   MPME property 19
   ratings properties 12
   serviceProvider properties 13
   sourceURL properties 13
   userEffectiveRating properties 12
   userEffectiveRatingInStars properties 13
   userRating properties 12
   userRatingInStars properties 12
   WMC_COMPAT_DO_NOT_LIMIT_RESPONSE_SIZE

flag 18

   WMC_COMPAT_EXCLUDE_DLNA flag 16
   WMC_COMPAT_EXCLUDE_DLNA_1_5 flag 17
   WMC_COMPAT_EXCLUDE_HTTP flag 15

WMC_COMPAT_EXCLUDE_NONPCM_AUDIO_TRA
NSCODING flag 19

   WMC_COMPAT_EXCLUDE_PCMPARAMS flag 17
   WMC_COMPAT_EXCLUDE_RES_FILTERING flag 19
   WMC_COMPAT_EXCLUDE_RTSP flag 15

   WMC_COMPAT_EXCLUDE_SEARCH flag 18

WMC_COMPAT_EXCLUDE_TRANSCODING_TO_M
PEG2 flag 19

   WMC_COMPAT_EXCLUDE_VIDEO_TRANSCODING

flag 18

WMC_COMPAT_EXCLUDE_WMALOSSLESS_NONT
RANSCODED flag 18

   WMC_COMPAT_EXCLUDE_WMDRMND flag 17
   WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO flag

18

   WMC_COMPAT_PLAYLIST_FAKECHILDCOUNT flag

18

   year properties 13

E

Examples
   artist properties tags 21
   author properties tags 21
   fileIdentifier property tag 22
   folderPath property tag 22
   magicPacketSendSupported 23
   magicPacketWakeSupported 23
   MMPE 21
   MMPE Examples 21
   ratings properties tags 21
   serviceProvider property tag 21
   X_DeviceCaps 22
   year property tag 22

F

Fields - vendor-extensible 9
fileIdentifier property 14
fileIdentifier property tag example 22
Flags
   values and behaviors 14
   WMC_COMPAT_DO_NOT_LIMIT_RESPONSE_SIZE

18

   WMC_COMPAT_EXCLUDE_DLNA 16
   WMC_COMPAT_EXCLUDE_DLNA_1_5 17
   WMC_COMPAT_EXCLUDE_HTTP 15

WMC_COMPAT_EXCLUDE_NONPCM_AUDIO_TRA
NSCODING 19

   WMC_COMPAT_EXCLUDE_PCMPARAMS 17
   WMC_COMPAT_EXCLUDE_RES_FILTERING 19
   WMC_COMPAT_EXCLUDE_RTSP 15
   WMC_COMPAT_EXCLUDE_SEARCH 18

WMC_COMPAT_EXCLUDE_TRANSCODING_TO_M
PEG2 19

   WMC_COMPAT_EXCLUDE_VIDEO_TRANSCODING

18

WMC_COMPAT_EXCLUDE_WMALOSSLESS_NONT
RANSCODED 18

   WMC_COMPAT_EXCLUDE_WMDRMND 17
   WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO 18

28 / 30

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


   WMC_COMPAT_PLAYLIST_FAKECHILDCOUNT 18
folderPath properties 14
folderPath property tag example 22

G

Glossary 6

I

Implementer - security considerations 24
Informative references 8
Introduction 6

L

Localization 9

M

Magic packets 19
magicPacketSendSupported example 23
magicPacketWakeSupported example 23
MCEF property 14
microsoft:magicPacketSendSupported tags 20
microsoft:magicPacketWakeSupported tags 19
MMPE example 21
MMPE Examples example 21
MMPE property 10
MPME property 19

N

Normative references 7

O

Overview (synopsis) 8

P

WMC_COMPAT_EXCLUDE_NONPCM_AUDIO_TRA
NSCODING 19

      WMC_COMPAT_EXCLUDE_PCMPARAMS 17
      WMC_COMPAT_EXCLUDE_RES_FILTERING 19
      WMC_COMPAT_EXCLUDE_RTSP 15
      WMC_COMPAT_EXCLUDE_SEARCH 18

WMC_COMPAT_EXCLUDE_TRANSCODING_TO_M
PEG2 19

WMC_COMPAT_EXCLUDE_VIDEO_TRANSCODIN
G 18

WMC_COMPAT_EXCLUDE_WMALOSSLESS_NONT
RANSCODED 18

      WMC_COMPAT_EXCLUDE_WMDRMND 17
      WMC_COMPAT_INCLUDE_RTSP_FOR_VIDEO 18
      WMC_COMPAT_PLAYLIST_FAKECHILDCOUNT 18
   folderPath 14
   MCEF 14
   MMPE 10
   MPME 19
   ratings 12
   serviceProvider 13
   sourceURL 13
   userEffectiveRating 12
   userEffectiveRatingInStars 13
   userRating 12
   userRatingInStars 12
   year 13
ProtocolInfo Extensions 20

R

Ratings properties 12
Ratings properties tags example 21
References 7
   informative 8
   normative 7
Relationship to protocols and other structures 8

Packets - magic 19
PlayToApp extension 20
PlayToApp extension example 23
Product behavior 25
Properties
   artist 10
   artistAlbumArtist 10
   artistConductor 11
   artistPerformer 10
   author 11
   authorComposer 11
   authorOriginalLyricist 11
   authorWriter 11
   compatibility flags - values and behaviors 14
   fileIdentifier 14
   flags
      values and behaviors 14

WMC_COMPAT_DO_NOT_LIMIT_RESPONSE_SIZ
E 18

      WMC_COMPAT_EXCLUDE_DLNA 16
      WMC_COMPAT_EXCLUDE_DLNA_1_5 17
      WMC_COMPAT_EXCLUDE_HTTP 15

S

Security - implementer considerations 24
serviceProvider properties 13
serviceProvider property tag example 21
sourceURL properties 13
Structures
   MCEF property 14
   MMPE property 10
   MPME property 19
   overview 10
   ProtocolInfo Extensions 20

T

Tags
   magic packets 19
   microsoft:magicPacketSendSupported 20
   microsoft:magicPacketWakeSupported 19
Tracking changes 26

U

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

29 / 30


userEffectiveRating properties 12
userEffectiveRatingInStars properties 13
userRating properties 12
userRatingInStars properties 12

V

Vendor-extensible fields 9
Versioning 9

X

X_DeviceCaps example 22

Y

year properties 13
year property tag example 22

[MS-UPMC] - v20240423
UPnP Device and Service Templates: Media Property and Compatibility Extensions
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

30 / 30

