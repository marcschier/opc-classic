[MS-WMLOG]:

Windows Media Log Data Structure

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

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 52


Revision Summary

Date

Revision
History

Revision
Class

Comments

4/3/2007

0.01

7/3/2007

1.0

7/20/2007

2.0

New

Major

Major

Version 0.01 release

MLonghorn+90

Revised technical content; added example topics.

8/10/2007

2.0.1

Editorial

Changed language and formatting in the technical content.

9/28/2007

2.0.2

Editorial

Changed language and formatting in the technical content.

10/23/2007  2.0.3

Editorial

Changed language and formatting in the technical content.

11/30/2007  2.0.4

Editorial

Changed language and formatting in the technical content.

1/25/2008

2.0.5

Editorial

Changed language and formatting in the technical content.

3/14/2008

2.1

Minor

Clarified the meaning of the technical content.

5/16/2008

2.1.1

Editorial

Changed language and formatting in the technical content.

6/20/2008

2.2

7/25/2008

2.3

8/29/2008

2.4

Minor

Minor

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

10/24/2008  2.4.1

Editorial

Changed language and formatting in the technical content.

12/5/2008

3.0

1/16/2009

3.1

Major

Minor

Updated and revised the technical content.

Clarified the meaning of the technical content.

2/27/2009

3.1.1

Editorial

Changed language and formatting in the technical content.

4/10/2009

3.2

5/22/2009

4.0

Minor

Major

Clarified the meaning of the technical content.

Updated and revised the technical content.

7/2/2009

4.0.1

Editorial

Changed language and formatting in the technical content.

8/14/2009

4.0.2

Editorial

Changed language and formatting in the technical content.

9/25/2009

4.1

Minor

Clarified the meaning of the technical content.

11/6/2009

4.1.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  4.1.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

4.2

Minor

Clarified the meaning of the technical content.

3/12/2010

4.2.1

Editorial

Changed language and formatting in the technical content.

4/23/2010

4.2.2

Editorial

Changed language and formatting in the technical content.

6/4/2010

4.2.3

Editorial

Changed language and formatting in the technical content.

7/16/2010

4.2.3

8/27/2010

4.2.3

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 52


Date

Revision
History

Revision
Class

Comments

technical content.

10/8/2010

4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

1/7/2011

4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

4.2.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

4.3

Minor

Clarified the meaning of the technical content.

9/23/2011

4.3

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  5.0

Major

Updated and revised the technical content.

3/30/2012

5.0

7/12/2012

5.0

10/25/2012  5.0

1/31/2013

5.0

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

8/8/2013

6.0

Major

Updated and revised the technical content.

11/14/2013  6.0

2/13/2014

6.0

5/15/2014

6.0

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

7.0

Major

Significantly changed the technical content.

10/16/2015  7.0

7/14/2016

7.0

6/1/2017

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

9/15/2017

8.0

Major

Significantly changed the technical content.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3 / 52


Date

Revision
History

Revision
Class

Comments

9/12/2018

9.0

4/7/2021

10.0

6/25/2021

11.0

4/23/2024

12.0

Major

Major

Major

Major

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 52


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
  - [2.1 Log Data Fields](#21-log-data-fields)
    - [2.1.1 audiocodec](#211-audiocodec)
    - [2.1.2 avgbandwidth](#212-avgbandwidth)
    - [2.1.3 c-buffercount](#213-c-buffercount)
    - [2.1.4 c-cpu](#214-c-cpu)
    - [2.1.5 c-dns](#215-c-dns)
    - [2.1.6 c-hostexe](#216-c-hostexe)
    - [2.1.7 c-hostexever](#217-c-hostexever)
    - [2.1.8 c-ip](#218-c-ip)
    - [2.1.9 c-max-bandwidth](#219-c-max-bandwidth)
    - [2.1.10 c-os](#2110-c-os)
    - [2.1.11 c-osversion](#2111-c-osversion)
    - [2.1.12 c-pkts-lost-client](#2112-c-pkts-lost-client)
    - [2.1.13 c-pkts-lost-cont-net](#2113-c-pkts-lost-cont-net)
    - [2.1.14 c-pkts-lost-net](#2114-c-pkts-lost-net)
    - [2.1.15 c-pkts-received](#2115-c-pkts-received)
    - [2.1.16 c-pkts-recovered-ECC](#2116-c-pkts-recovered-ecc)
    - [2.1.17 c-pkts-recovered-resent](#2117-c-pkts-recovered-resent)
    - [2.1.18 c-playerid](#2118-c-playerid)
    - [2.1.19 c-playerlanguage](#2119-c-playerlanguage)
    - [2.1.20 c-playerversion](#2120-c-playerversion)
    - [2.1.21 c-quality](#2121-c-quality)
    - [2.1.22 c-rate](#2122-c-rate)
    - [2.1.23 c-resendreqs](#2123-c-resendreqs)
    - [2.1.24 c-starttime](#2124-c-starttime)
    - [2.1.25 c-status](#2125-c-status)
      - [2.1.25.1 Status Code 200 (No Error)](#21251-status-code-200-no-error)
      - [2.1.25.2 Status Code 210 (Client Successfully Reconnected)](#21252-status-code-210-client-successfully-reconnected)
    - [2.1.26 c-totalbuffertime](#2126-c-totalbuffertime)
    - [2.1.27 c-channelURL](#2127-c-channelurl)
    - [2.1.28 c-bytes](#2128-c-bytes)
    - [2.1.29 cs-media-name](#2129-cs-media-name)
    - [2.1.30 cs-media-role](#2130-cs-media-role)
    - [2.1.31 cs-Referer](#2131-cs-referer)
    - [2.1.32 cs-url](#2132-cs-url)
    - [2.1.33 cs-uri-stem](#2133-cs-uri-stem)
    - [2.1.34 cs-User-Agent](#2134-cs-user-agent)
    - [2.1.35 cs-user-name](#2135-cs-user-name)
    - [2.1.36 date](#2136-date)
    - [2.1.37 filelength](#2137-filelength)
    - [2.1.38 filesize](#2138-filesize)
    - [2.1.39 protocol](#2139-protocol)
    - [2.1.40 s-content-path](#2140-s-content-path)
    - [2.1.41 s-cpu-util](#2141-s-cpu-util)
    - [2.1.42 s-dns](#2142-s-dns)
    - [2.1.43 s-ip](#2143-s-ip)
    - [2.1.44 s-pkts-sent](#2144-s-pkts-sent)
    - [2.1.45 s-proxied](#2145-s-proxied)
    - [2.1.46 s-session-id](#2146-s-session-id)
    - [2.1.47 s-totalclients](#2147-s-totalclients)
    - [2.1.48 sc-bytes](#2148-sc-bytes)
    - [2.1.49 time](#2149-time)
    - [2.1.50 transport](#2150-transport)
    - [2.1.51 videocodec](#2151-videocodec)
    - [2.1.52 x-duration](#2152-x-duration)
  - [2.2 Logging Message: W3C Syntax](#22-logging-message-w3c-syntax)
    - [2.2.1 Basic Logging Syntax](#221-basic-logging-syntax)
    - [2.2.2 Extended Logging Syntax](#222-extended-logging-syntax)
  - [2.3 Logging Messages Sent to Web Servers](#23-logging-messages-sent-to-web-servers)
  - [2.4 Logging Message: XML Schema](#24-logging-message-xml-schema)
  - [2.5 Legacy Log](#25-legacy-log)
    - [2.5.1 Common Definitions](#251-common-definitions)
    - [2.5.2 Legacy Log in W3C Format](#252-legacy-log-in-w3c-format)
    - [2.5.3 Legacy Log in XML Format](#253-legacy-log-in-xml-format)
    - [2.5.4 Legacy Log Sent to a Web Server](#254-legacy-log-sent-to-a-web-server)
  - [2.6 Streaming Log](#26-streaming-log)
    - [2.6.1 Common Definitions](#261-common-definitions)
    - [2.6.2 Streaming Log Sent to Windows Media Services](#262-streaming-log-sent-to-windows-media-services)
    - [2.6.3 Streaming Log Sent to a Web Server](#263-streaming-log-sent-to-a-web-server)
  - [2.7 Rendering Log](#27-rendering-log)
    - [2.7.1 Common Definitions](#271-common-definitions)
    - [2.7.2 Rendering Log Sent to Windows Media Services](#272-rendering-log-sent-to-windows-media-services)
    - [2.7.3 Rendering Log Sent to a Web Server](#273-rendering-log-sent-to-a-web-server)
  - [2.8 Connect-Time Log](#28-connect-time-log)
- [3 Structure Examples](#3-structure-examples)
  - [3.1 Legacy Logging Message](#31-legacy-logging-message)
  - [3.2 Defining Custom Namespaces in an XML Log](#32-defining-custom-namespaces-in-an-xml-log)
  - [3.3 Example Streaming Log Messages](#33-example-streaming-log-messages)
  - [3.4 Example Rendering Log Messages](#34-example-rendering-log-messages)
  - [3.5 Example Connect-Time Log Message](#35-example-connect-time-log-message)
  - [3.6 Example Log Sent to a Web Server](#36-example-log-sent-to-a-web-server)
- [4 Security Considerations](#4-security-considerations)
- [5 Appendix A: Product Behavior](#5-appendix-a-product-behavior)
- [6 Change Tracking](#6-change-tracking)
- [7 Index](#7-index)

## 1 Introduction

This specification defines the Windows Media Log Data Structure. The Windows Media Log Data
Structure is a syntax for logging messages. The logging messages specify information about how a
client received multimedia content from a streaming server. For example, logging messages can
specify how many packets were received and how long it took for the client to receive the content.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

### 1.1 Glossary

This document uses the following terms:

Advanced Systems Format (ASF): An extensible file format that is designed to facilitate
streaming digital media data over a network. This file format is used by Windows Media.

client: The entity that has created the logging message, or an entity that receives a logging

message from a client. In the latter case, the client is a proxy.

content: Multimedia data. content is always in ASF, for example, a single ASF music file or a

single ASF video file.  Data in general. A file that an application accesses. Examples of content
include web pages and documents stored on either web servers or SMB file servers.

globally unique identifier (GUID): A term used interchangeably with universally unique

identifier (UUID) in Microsoft protocol technical documents (TDs). Interchanging the usage of
these terms does not imply or require a specific algorithm or mechanism to generate the value.
Specifically, the use of this term does not imply or require that the algorithms described in
[RFC4122] or [C706] have to be used for generating the GUID. See also universally unique
identifier (UUID).

Hypertext Transfer Protocol (HTTP): An application-level protocol for distributed, collaborative,
hypermedia information systems (text, graphic images, sound, video, and other multimedia
files) on the World Wide Web.

Internet host name: The name of a host as defined in [RFC1123] section 2.1, with the extensions

described in [MS-HNDS].

Multimedia Messaging Service (MMS): A communications protocol that is designed for

messages containing text, images, and other multimedia content that is sent between mobile
phones.

playlist: One or more content items that are streamed sequentially.

proxy: An entity that can receive logging messages from both a client and a proxy, or from a

server that is streaming on behalf of another server.

Real-Time Streaming Protocol (RTSP): A protocol used for transferring real-time multimedia

data (for example, audio and video) between a server and a client, as specified in [RFC2326]. It
is a streaming protocol; this means that RTSP attempts to facilitate scenarios in which the
multimedia data is being simultaneously transferred and rendered (that is, video is displayed
and audio is played).

server: An entity that transfers content to a client through streaming. A server might be able to do
streaming on behalf of another server; thus, a server can also be a proxy. See [MS-WMLOG]

session: The state maintained by the server when it is streaming content to a client. If a server-

side playlist is used, the same session is used for all content in the playlist.

7 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


stream: (1) A flow of data from one host to another host, or the data that flows between two

hosts.

(2) A sequence of ASF media objects ([ASF] section 5.2) that can be selected individually. For
example, if a movie has an English and a Spanish soundtrack, each can be encoded in the ASF
file as a separate stream. The video data would also be a separate stream.

streaming: The act of transferring content from a sender to a receiver.

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

[ASF] Microsoft Corporation, "Advanced Systems Format Specification", December 2004,
https://download.microsoft.com/download/7/9/0/790fecaa-f64a-4a5e-a430-
0bccdab3f1b4/ASF_Specification.doc

[MS-MSB] Microsoft Corporation, "Media Stream Broadcast (MSB) Protocol".

[MS-RTSP] Microsoft Corporation, "Real-Time Streaming Protocol (RTSP) Windows Media Extensions".

[MS-WMSP] Microsoft Corporation, "Windows Media HTTP Streaming Protocol".

[MSDN-WMMETA] Microsoft Corporation, "Windows Media Metafiles", http://msdn.microsoft.com/en-
us/library/dd564665(VS.85).aspx

[RFC1945] Berners-Lee, T., Fielding, R., and Frystyk, H., "Hypertext Transfer Protocol -- HTTP/1.0",
RFC 1945, May 1996, https://www.rfc-editor.org/info/rfc1945

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2616] Fielding, R., Gettys, J., Mogul, J., et al., "Hypertext Transfer Protocol -- HTTP/1.1", RFC
2616, June 1999, https://www.rfc-editor.org/info/rfc2616

[RFC3066] Alvestrand, H., "Tags for the Identification of Languages", BCP 47, RFC 3066, January
2001, https://www.rfc-editor.org/info/rfc3066

[RFC3629] Yergeau, F., "UTF-8, A Transformation Format of ISO 10646", STD 63, RFC 3629,
November 2003, https://www.rfc-editor.org/info/rfc3629

[RFC3986] Berners-Lee, T., Fielding, R., and Masinter, L., "Uniform Resource Identifier (URI): Generic
Syntax", STD 66, RFC 3986, January 2005, https://www.rfc-editor.org/info/rfc3986

[RFC4234] Crocker, D., Ed., and Overell, P., "Augmented BNF for Syntax Specifications: ABNF", RFC
4234, October 2005, https://www.rfc-editor.org/info/rfc4234

8 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


#### 1.2.2 Informative References

[MS-MMSP] Microsoft Corporation, "Microsoft Media Server (MMS) Protocol".

[W3C-EXLOG] World Wide Web Consortium, "Extended Log File Format", http://www.w3.org/TR/WD-
logfile.html

### 1.3 Overview

The Windows Media Log Data Structure is a syntax for logging messages. The logging messages
specify information about how a client received multimedia content from a streaming server.

### 1.4 Relationship to Protocols and Other Structures

The logging messages defined in this specification are used by the Windows Media HTTP Streaming
Protocol described in [MS-WMSP], and the Real-Time Streaming Protocol (RTSP) Windows Media
Extensions, described in [MS-RTSP]. When those two protocols are used, the logging messages
defined by this specification can be encapsulated in protocol messages specific to the streaming
protocol in use. The resulting protocol messages are sent to either Windows Media Services or to a
proxy compatible with the logging message syntax defined in this specification.

It is also possible to send logging messages to an HTTP web server. This is possible when using the
two streaming protocols mentioned earlier and when using two other streaming protocols: the
Microsoft Media Server (MMS) Protocol as described in [MS-MMSP], and the Media Stream Broadcast
(MSB) Protocol as described in [MS-MSB].

### 1.5 Applicability Statement

The syntax for logging messages defined by this specification is applicable to implementations of the
four streaming protocols mentioned in section 1.4.

### 1.6 Versioning and Localization

None.

### 1.7 Vendor-Extensible Fields

Logging messages in XML format are vendor-extensible. Any logging information added by a vendor
MUST be encoded using the "client-logging-data" syntax element specified in section 2.4.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 52


## 2 Structures

Section 2.1 defines fields that can appear in a logging message. Not all fields appear in all logging
messages, however. Section 2.2 defines the syntax of World Wide Web Consortium (W3C)-based
logging messages, and section 2.4 defines the syntax of XML-based logging messages.

Section 2.5 defines the legacy logging message type. Section 2.6 defines the Streaming Log message
type. Section 2.7 defines the Rendering Log message type. Section 2.8 defines the Connection Log
message type.

Note  These sections can also contain variants that supersede the definitions in 2.1.

The information contained in a logging message is always specific to a particular session. The extent
of a session is defined by the streaming protocol used by the server. A Rendering Log message (as
specified in section 2.7) can be sent without streaming from a server, and, in that case, a session
starts when the playback of the playlist starts and stops when the playback of the playlist stops.

Following are some common Augmented Backus-Naur Form (ABNF) constructions, as specified in
[RFC4234], that are used throughout this specification. Any ABNF syntax rules that are not defined in
[RFC4234] or in this specification are defined in [RFC1945] or [RFC2616].

 date-year    = 4DIGIT        ; "19xx" and "20xx" typical
 date-month   = 2DIGIT        ; 01 through 12
 date-day     = 2DIGIT        ; 01 through 31
 time-hour    = 2DIGIT        ; 00 through 24
 time-min     = 2DIGIT        ; 00 through 59
 time-sec     = 2DIGIT        ; 00 through 59, 60 if leap second

 ip_addr      = IPv4address | IPv6address
                  ; Defined in Appendix A of RFC3986

 ver_major    = 1*2DIGIT
 ver_minor    = 1*2DIGIT ["." 1*4DIGIT "." 1*4DIGIT]

### 2.1 Log Data Fields

#### 2.1.1 audiocodec

This field SHOULD specify a list of audio codecs used to decode the audio streams (1) accessed by
the client. Each codec MUST be listed only once regardless of the number of streams (2) decoded by
that codec.

The value for audiocodec MUST NOT exceed 256 characters in total length. If the codec name is not
available, the field MUST be set to "-".

The syntax of the audiocodec field is defined as follows.

 codec-name= 1*255VCHAR
 audiocodec=  "-" | ( codec-name *( ";" codec-name ) )

Example:

 Microsoft_Audio_Codec;Generic_MP3_Codec

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 52


#### 2.1.2 avgbandwidth

This field MUST specify the average bandwidth, in bits per second, at which the client received
content from the server (which can be a proxy), as measured by the client from the start of the
current session. This is only applicable during periods in which the server is streaming the content.
Depending on the streaming protocol used, it might be possible for the session to be in a "paused"
state in which streaming is suspended. The value for avgbandwidth does not account for the average
bandwidth during such periods in which streaming is suspended.

If the notion of an average bandwidth is not applicable, for example, because the client did not receive
any content from the server, then the field MUST be set to "-".

If the numerical value is specified, it MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the avgbandwidth field is defined as follows.

 avgbandwidth=  "-" | 1*10DIGIT

Example:

 102585

#### 2.1.3 c-buffercount

This field MUST specify the number of times the client buffered while playing the content, counted
from when the client most recently started streaming the content.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-buffercount field is defined as follows.

 c-buffercount=  1*10DIGIT

Example:

 1

#### 2.1.4 c-cpu

This field MUST specify the type of CPU used by the client computer.

The syntax of the c-cpu field is defined as follows.

 c-cpu=  1*64VCHAR

Example:

 Pentium

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 52


#### 2.1.5 c-dns

This field SHOULD be set to "-". The field MAY specify the Internet host name of the client sending
the log.<1>

The syntax of the c-dns field is defined in [RFC3986], as follows.

 c-dns= "-"
        | reg-name ;

Example:

 wmt.test.com

#### 2.1.6 c-hostexe

This field specifies the file name of the host application executed on the client. This field MUST NOT
refer to a .dll, .ocx, or other nonexecutable file.

The syntax of the c-hostexe field is defined as follows.

 c-hostexe=  *255VCHAR

Example:

 wmplayer.exe

#### 2.1.7 c-hostexever

This field MUST specify the version number of the host application running on the client.

The syntax of the c-hostexever field is defined as follows.

 c-hostexever=  ver_major "." ver_minor

Example:

 6.2.5.323

#### 2.1.8 c-ip

When a client creates a logging message, it SHOULD specify the c-ip field as "-" but MAY specify the
IP address of the client.

If a proxy is forwarding a logging message on behalf of a client, the c-ip field MUST specify the IP
address of the client. The proxy MUST replace the value of the c-ip field that was specified by the
client with the IP address of the client (as known to the proxy).

The syntax of the c-ip field is defined as follows.

12 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


 c-ip =  "-" | ip_addr

Example:

 157.100.100.100

Example:

 3ffe:2900:d005:f28b:0000:5efe:157.55.145.142

#### 2.1.9 c-max-bandwidth

This field MUST be set to "-".

The syntax of the c-max-bandwidth field is defined as follows.

 c-max-bandwidth ="-"

Example:

 -

#### 2.1.10 c-os

This field MUST specify the client's operating system.<2>

The syntax of the c-os field is defined as follows.

 OSname= "Windows_98" | "Windows_ME" | "Windows_NT"
   | "Windows_2000" | "Windows_XP" | "Windows"
   | "Windows_Server 2003"
 c-os = OSname | 1*64VCHAR

Example:

 Windows

#### 2.1.11 c-osversion

This field MUST specify the version number of the client's operating system.

The syntax of the c-osversion field is defined as follows.

 c-osversion= ver_major "." ver_minor

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

13 / 52


Example:

 6.0.0.6000

#### 2.1.12 c-pkts-lost-client

This field MUST specify the number of Advanced Systems Format (ASF) data packets ([ASF]
section 5.2) lost during transmission from server to client and not recovered at the client layer
through error correction or at the network layer by using the User Datagram Protocol (UDP) resends,
counted from when the client most recently started streaming the content.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-lost-client field is defined as follows.

 c-pkts-lost-client= 1*10DIGIT

Example:

 5

#### 2.1.13 c-pkts-lost-cont-net

This field MUST specify the largest number of ASF data packets that were lost as a consecutive span
during transmission from server to client and counted from when the client most recently started
streaming the content.

For example, if data packets numbered 1, 4, and 8 are received, and packets 2, 3, 5, 6 and 7 are lost,
then packets 2 and 3 constitute a span of two lost packets, and packets 5, 6 and 7 constitute a span
of three lost packets. In this example, the c-pkts-lost-cont-net field would be set to 3—the size of
the largest span.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-lost-cont-net field is defined as follows.

 c-pkts-lost-cont-net= 1*10DIGIT

Example:

 2

#### 2.1.14 c-pkts-lost-net

This field MUST specify the number of ASF data packets lost on the network layer, counted from when
the client most recently started streaming the content. Packets lost at the network layer can be
recovered if the client re-creates them by using forward error correction.

The numerical difference between the value of c-pkts-lost-net and the value of c-pkts-lost-client
MUST be equal to the value of c-pkts-recovered-ECC.

14 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-lost-net field is defined as follows.

 c-pkts-lost-net= 1*10DIGIT

Example:

 2

#### 2.1.15 c-pkts-received

This field MUST specify the number of ASF data packets that have been correctly received by the
client on the first attempt counted from when the client most recently started streaming the
content. (ASF data packets that were received through error correction code (ECC) recovery or UDP
resends are not included in the c-pkts-received field.)

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-received field is defined as follows.

 c-pkts-received= 1*10DIGIT

Example:

 523

#### 2.1.16 c-pkts-recovered-ECC

This field MUST specify the number of ASF data packets that were lost at the network layer but were
subsequently recovered, counted from when the client most recently started streaming the
content. The value of this field MUST be equal to the numerical difference between the value of c-
pkts-lost-net and the value of c-pkts-lost-client.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-recovered-ECC field is defined as follows.

 c-pkts-recovered-ECC= 1*10DIGIT

Example:

 1

#### 2.1.17 c-pkts-recovered-resent

This field MUST specify the number of ASF data packets that were recovered either because they were
resent through UDP or because they were received out of order.

15 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-pkts-recovered-resent field is defined as follows.

 c-pkts-recovered-resent= 1*10DIGIT

Example:

 5

#### 2.1.18 c-playerid

This field specifies a unique identifier for the client application that originated the request. The
identifier MUST be a GUID. The GUID is expressed in registry format and is not enclosed in quotation
marks, as shown by the following ABNF syntax.

If the client is configured to remain anonymous (that is, not send private information), the client
MUST set the c-playerid field as indicated by the ABNF syntax for the playid_priv syntax element as
shown in the following code example. Otherwise, c-playerid MUST use the syntax for playid_pub as
shown in the following code example. The client MUST choose a value for playid_pub randomly, and
the same value MUST be used for playid_pub in all logging messages created by the client application,
regardless of which content is streamed.

 Furthermore, multiple instances, or incarnations, of the client application MUST use the same value
for the playid_pub syntax element. However, if the client application is shared by multiple users, and
it is possible to determine a user identity or account name of the user launching the client application,
then the value for playid_pub SHOULD be different for each user identity or account name. For
example, multi-user operating systems typically have separate accounts with a distinct account name
for each user, but cellular telephones do not.

If the client uses the playid_priv syntax element, the client SHOULD choose the value for the playid
syntax element randomly; however, the client MUST use the same playid value for all logging
messages sent for the same session.

The syntax of the c-playerid field is defined as follows.

 playid= 12HEXDIG
 playid_pub = "{" 8HEXDIG "-" 4HEXDIG "-" 4HEXDIG "-"
               4HEXDIG "-" 12HEXDIG "}"
 playid_priv= "{3300AD50-2C39-46c0-AE0A-" playid "}"

 c-playerid= playid_pub / playid_priv

Example:

 {c579d042-cecc-11d1-bb31-00a0c9603954}

Example (client is anonymous):

 {3300AD50-2C39-46c0-AE0A-70b64f321a80}

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 52


#### 2.1.19 c-playerlanguage

This field MUST specify the language-country code of the client.

The syntax of the c-playerlanguage field is defined in section 2.1 of [RFC3066], as follows.

 c-playerlanguage= Language-Tag
 ;

Example:

 en-US

#### 2.1.20 c-playerversion

This field MUST specify the version number of the client.

The syntax of the c-playerversion field is defined as follows.

 c-playerversion = ver_major "." ver_minor

Example:

 7.0.1024

#### 2.1.21 c-quality

This field MUST specify the percentage of packets that were received by the client, counted from
when the client most recently started streaming the content.

If cPacketsRendered represents all packets received by the client including packets recovered by
ECC and UDP resend such that:

 cPacketsRendered  = c-pkts-received + c-pkts-recovered-ECC + c-pkts-
 recovered-resent

then the value for the c-quality field MUST be calculated as follows.

 [cPacketsRendered / (cPacketsRendered + c-pkts-lost-client)] * 100

If the denominator in the above equation evaluates to 0, c-quality MUST be specified as 100.

The syntax of the c-quality field is defined as follows.

 c-quality = 1*2DIGIT / "100"

Example:

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 52


 89

#### 2.1.22 c-rate

This field MUST specify the rate of streaming or playback as a multiplier of the normal streaming or
playback rate.

For example, a value of 1 specifies streaming or playback at the normal rate, while a value of -5
indicates rewind at a speed five times faster than real-time, and a value of 5 indicates fast-forward at
a rate five times faster than real-time.

For Legacy and Streaming Logs, c-rate MUST be the streaming rate. For Rendering logs, c-rate MUST
be the rendering (playback) rate.

The value of c-rate MUST reflect the rate that was in effect at the beginning of the period covered by
the logging message because streaming or playback might already have ended by the time the
logging message is generated.

The syntax of the c-rate field is defined as follows.

 c-rate= [ "-" ] 1*2DIGIT

Example:

 1

#### 2.1.23 c-resendreqs

This field MUST specify the number of requests made by the client to receive lost ASF data packets,
counted from when the client most recently started streaming the content. If the client is not using
UDP resend, the value of this field MUST be "-".

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-resendreqs field is defined as follows.

 c-resendreqs= "-"/ 1*10DIGIT

Example:

 5

#### 2.1.24 c-starttime

This field MUST specify the time offset, in seconds, in the content from which the client started to
render content. This represents the presentation time of the ASF data packets that the client began
rendering. For live broadcasts, the client MUST set this field to zero.

The value MUST be an integer in the range from 0 through 4,294,967,295.

18 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


The syntax of the c-starttime field is defined as follows.

 c-starttime= 1*10DIGIT

Example:

 39

#### 2.1.25 c-status

This field MUST specify a numerical code that indicates the status of the client that creates the
logging message.

The syntax of the c-status field is defined as follows:

 c-status= "200" / "210"

Example:

 200

##### 2.1.25.1 Status Code 200 (No Error)



This code indicates that the client successfully streamed and submitted the log.

##### 2.1.25.2 Status Code 210 (Client Successfully Reconnected)



This code indicates that the client disconnected and then reconnected to the server.<3>

#### 2.1.26 c-totalbuffertime

This field MUST specify the total time, in seconds, that the client spent buffering the ASF data packets
in the content, counted from when the client most recently started streaming the content. If the
client buffers the content more than once before a log is generated, c-totalbuffertime MUST be
equal to the total amount of time that the client spent buffering the ASF data packets.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-totalbuffertime field is defined as follows.

 c-totalbuffertime= 1*10DIGIT

Example:

 20

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 52


#### 2.1.27 c-channelURL

This field MUST specify the URL to the multicast station (.nsc) file, as specified in [MS-MSB], if such a
file was used by the client. Whenever an .nsc file is used, this field MUST be specified, even if the
MSB Protocol was not used to stream (2) content.

The syntax of the c-channelURL field is defined in section 4.1 of [RFC3986], as follows.

 c-channelURL  = "-"
                / URI-reference ;

Example:

 http://server/channel.nsc

#### 2.1.28 c-bytes

This field MUST specify the number of bytes received by the client from the server, counted from
when the client most recently started streaming the content.

The value for the c-bytes field MUST NOT include TCP/IP or other overhead added by the network
stack. For example, higher-level protocols such as HTTP, RTSP, and MMS can each introduce
differing amounts of overhead, resulting in different values for the same content.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the c-bytes field is defined as follows.

 c-bytes= 1*10DIGIT

Example:

 28000

#### 2.1.29 cs-media-name

The purpose of this field is to specify the file name of the content or server-side playlist entry that
was streamed or played by the client. For Legacy and Streaming Logs, the value of this field MUST
be the content or server-side playlist entry that was streamed. For Rendering Logs, it MUST be the
content or server-side playlist entry that was rendered (played).

If the server provided a Content Description, as specified in [MS-WMSP], and the Content Description
contains an entry named WMS_CONTENT_DESCRIPTION_PLAYLIST_ENTRY_URL, the value of the cs-
media-name field MUST be equal to the value of the
WMS_CONTENT_DESCRIPTION_PLAYLIST_ENTRY_URL entry.

Otherwise, if the client is using an Active Stream Redirector (.asx) file (for more information, see
[MSDN-WMMETA]), and the file specifies a logging parameter called "cs-media-name", then the value
of the cs-media-name field in the logging message MUST be equal to the value of the "cs-media-
name" logging parameter in the .asx file. See section 3.2 for an example of how this parameter is
specified in an .asx file.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 52


If none of the preceding applies, cs-media-name MUST be specified as "-".

The syntax of the cs-media-name field is defined as follows.

 cs-media-name=  *VCHAR

Examples:

 C:\wmpub\wmroot\MyAd2.asf

#### 2.1.30 cs-media-role

The purpose of this field is to specify a value that can be associated with a server-side playlist entry
to signify the role of the playlist entry. For Legacy and Streaming Logs, the value of this field MUST be
the role of the server-side playlist entry that was streamed. For Rendering Logs, it MUST be the role
of the server-side playlist entry that was rendered (played).

If the server provided a Content Description as specified in [MS-WMSP], and the Content Description
contains an entry named WMS_CONTENT_DESCRIPTION_ROLE, the value of the cs-media-role field
MUST be equal to the value of the WMS_CONTENT_DESCRIPTION_ROLE entry.

Otherwise, if the client is using an Active Stream Redirector (.asx) file as specified in [MSDN-
WMMETA], and the file specifies a logging parameter called "cs-media-role", the value of the cs-
media-role field in the logging message MUST be equal to the value of the "cs-media-role" logging
parameter in the .asx file. See section 3.2 for an example of how this parameter is specified in an .asx
file.

If none of the preceding applies, the cs-media-role MUST be specified as "-".

The syntax of the cs-media-role field is defined as follows.

 cs-media-role=  *VCHAR

Example:

 ADVERTISEMENT

#### 2.1.31 cs-Referer

This field SHOULD specify the URL to the web page that the client software application is embedded
within, except if the client software application was not embedded in a web page. If the client software
application is not embedded in a web page, but an .asx file (for more information, see [MSDN-
WMMETA]) was obtained from a web page, this field SHOULD be set to the URL to that web page.

If none of the preceding applies, this field MUST be set to "-".

The syntax of the cs-Referer field is defined in section 4.1 of [RFC3986], as follows.

 cs-Referer= "-"
            / URI-reference ;

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

21 / 52


Examples:

 http://www.adventure-works.com/default.htm

#### 2.1.32 cs-url

This field MUST specify the URL for the streaming content originally requested by the client.

Note that the value of this field can be different from the URL actually used if the server redirected
the client to a different URL, or if the client decided to use a streaming protocol that is different from
the one indicated by the URL scheme of the original URL.

When the MSB Protocol specified in [MS-MSB] is used, the "asfm" MUST be used as the URL scheme in
the cs-url field.

The syntax of the cs-url field is defined in section 4.1 of [RFC3986], as follows.

 cs-url= URI-reference;

Example 1:

 mms://www.adventure-works.com/some/content.asf

Example 2:

 asfm://239.1.2.3:9000

#### 2.1.33 cs-uri-stem

This field MUST specify the URL actually used by the client. Any query strings MUST be excluded from
the URL. (This means that the value of the cs-uri-stem field is equal to the URL actually used by the
client, truncated at the first "?" character.)

Note that the value of this field can be different from the URL originally requested by the client if the
server redirected the client to a different URL, or if the client decided to use a streaming protocol
that is different from the one indicated by the URL scheme of the original URL.

When the MSB Protocol is used, the "asfm" MUST be used as the URL scheme in the cs-uri-stem
field, as specified in [MS-MSB].

The syntax of the cs-uri-stem field is defined in section 4.1 of [RFC3986], as follows.

 cs-uri-stem= URI-reference;

Example:

 rtsp://server/test/sample.asf

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

22 / 52


#### 2.1.34 cs-User-Agent

The purpose of this field is to specify information regarding the client application that is sending the
logging message.

The cs-User-Agent field SHOULD be set to the same value that Windows Internet Explorer specifies
on the User-Agent HTTP protocol header. The field MAY be set differently as long as it adheres to the
ABNF syntax as shown in the following code example.

If a logging message is forwarded by a proxy, the cs-User-Agent field MUST begin with the string
"_via_". The original value specified by the client (which can be another proxy) on the cs-User-
Agent field SHOULD be discarded. The proxy SHOULD include a product token on the cs-User-Agent
field that specifies the brand and version of the proxy.

The syntax of the cs-User-Agent field is defined as follows.

 cs-User-Agent= [ "_via_HTTP/1.0_" ]
   1*( product; [RFC2616] section 3.8
 | comment ); [RFC2616] section 2.2

Example 1: media player embedded in Internet Explorer 6 on Windows XP operating system Service
Pack 2 (SP2):

 Mozilla/4.0_(compatible;_MSIE_6.0;_Windows_NT_5.1;_SV1)

Example 2: application based on Windows Media Format 9 Series SDK:

 Application/2.3 (WMFSDK/9.0.1234)

Example 3: proxy:

 _via_HTTP/1.0_WMCacheProxy/9.00.00.1234

#### 2.1.35 cs-user-name

This field MUST be set to "-".

The syntax of the cs-user-name field is defined as follows.

 cs-user-name= "-"

Example:

 -

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

23 / 52


#### 2.1.36 date

This field MUST specify the current date on the client when the log message is created. The time
MUST be specified in UTC.

The syntax of the date field is defined as follows.

 date= date-year "-" date-month "-" date-day

Example:

 1997-10-09

#### 2.1.37 filelength

This field MUST specify the length of the ASF file, in seconds. For a live broadcast stream (2), the
value for filelength is undefined and MUST be set to zero.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the filelength field is defined as follows.

 filelength= 1*10DIGIT

Example:

 60

#### 2.1.38 filesize

This field MUST specify the size of the ASF file, in bytes. For a live broadcast stream (2), the value
for the filesize field is undefined and MUST be set to zero.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the filesize field is defined as follows.

 filesize= 1*10DIGIT

Example:

 86000

#### 2.1.39 protocol

This field MUST specify the protocol used to stream (2) content to the client.

If the Windows Media HTTP Streaming Media Protocol was used, the value of the protocol field MUST
be "http", as specified in [MS-WMSP].

24 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


If the RTSP Windows Media Extensions was used, the value of the protocol field MUST be "rtsp", as
specified in [MS-RTSP].

If the MSB Protocol was used, the value of the protocol field MUST be "asfm", as specified in [MS-
MSB].

Note  The value for protocol can be different from the URL moniker used in the stream request.

The syntax of the protocol field is defined as follows.

 protocol= "http" / "rtsp" / "asfm"

Example:

 http

#### 2.1.40 s-content-path

This field MUST be set to "-".

The syntax of the s-content-path field is defined as follows.

 s-content-path = "-"

Example:

 -

#### 2.1.41 s-cpu-util

When a client creates a logging message, it MUST specify the s-cpu-util field as "-".

If a proxy is forwarding the logging message on behalf of a client (which can be another proxy), the
proxy MUST replace the value of the s-cpu-util field that was specified by the client with the proxy's
current CPU load, in percentage, at the time of forwarding the logging message. If the proxy uses
symmetric multiprocessing, the CPU load value MUST be calculated as the average for all processors.

When a numerical value is specified, the value MUST be an integer in the range from 0 through 100.

The syntax of the s-cpu-util field is defined as follows.

 s-cpu-util = "-" | 1*2DIGIT | "100"

Example:

 40

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

25 / 52


#### 2.1.42 s-dns

This field SHOULD specify the Internet host name of the proxy if a proxy is forwarding the logging
message on behalf of a client (which can be another proxy). The proxy MUST replace the value of the
s-dns field that was specified by the client with its own Internet host name or with "-" if the Internet
host name cannot be determined.

When a client creates a logging message, it SHOULD specify the s-dns field as "-" but MAY specify the
Internet host name of the server that the client streamed the content from.

The syntax of the s-dns field is defined in [RFC3986], as follows.

 s-dns= "-"
        | reg-name ;

Example:

 wmt.adventure-works.com

#### 2.1.43 s-ip

For Legacy and Streaming Logs, this field MUST specify the IP address of the server that the client
streamed the content from.

For Rendering Logs, the field MUST specify the IP address of the proxy if a proxy is forwarding the
logging message on behalf of a client. The proxy MUST replace the value of the s-ip field that was
specified by the client (which can be another proxy) with the IP address used by the proxy when
forwarding the Rendering Log to the server (which can be another proxy).

When a client creates a rendering log, it SHOULD specify the s-ip field as "-" but can specify the IP
address of the server that the client streamed the content from.

The syntax of the s-ip field is defined as follows.

 s-ip = "-" | ip_addr

Example:

 155.12.1.234

#### 2.1.44 s-pkts-sent

This field MUST be set to "-".

The syntax of the s-pkts-sent field is defined as follows.

 s-pkts-sent= "-"

Example:

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

26 / 52


 -

#### 2.1.45 s-proxied

This field MUST be set to "1" in a logging message that is being forwarded by a proxy. The client that
creates the logging message MUST set the field to "0", and the proxy MUST change the value to "1"
when it forwards the logging message.

The syntax of the s-proxied field is defined as follows.

 s-proxied= "0" / "1"

Example:

 1

#### 2.1.46 s-session-id

This field MUST be set to "-".

The syntax of the s-session-id field is defined as follows.

 s-session-id= "-"

Example:

 -

#### 2.1.47 s-totalclients

When a client creates a logging message, it MUST specify the s-totalclients field as "-".

If a proxy is forwarding the logging message on behalf of a client (which can be another proxy), the
proxy MUST replace the value of the s-totalclients field that was specified by the client with the total
number of clients connected to the proxy server (for all target servers combined).

When a numerical value is specified, the value MUST be an integer in the range from 0 through
4,294,967,295.

The syntax of the s-totalclients field is defined as follows.

 s-totalclients = "-" | 1*10DIGIT

Example:

 201

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

27 / 52


#### 2.1.48 sc-bytes

This field MUST be set to "-".

The syntax of the sc-bytes field is defined as follows.

 sc-bytes= "-"

Example:

 -

#### 2.1.49 time

This field MUST specify the current time on the client when the log message is created. The time
MUST be specified in UTC.

The syntax of the time field is defined as follows.

 time= time-hour ":" time-min ":" time-sec

Example:

 15:30:30

#### 2.1.50 transport

This field MUST specify the transport protocol used to receive the ASF data packets.

The syntax of the transport field is defined as follows.

 transport= "UDP" | "TCP"

Example:

 UDP

#### 2.1.51 videocodec

This field SHOULD specify a list of video codecs that are used to decode the video streams (2)
accessed by the client. Each codec MUST be listed only once, regardless of the number of streams
decoded by that codec.

The value for videocodec MUST NOT exceed 256 characters in total length. If the codec name is not
available, the field MUST be set to "-".

The syntax of the videocodec field is defined as follows.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

28 / 52


 codec-name= 1*255VCHAR
 videocodec=  "-" | ( codec-name *( ";" codec-name ) )

Example:

 Microsoft_MPEG-4_Video_Codec_V2

#### 2.1.52 x-duration

For Legacy and Rendering Log messages, this field MUST specify how much of the content has been
rendered (played) to the end user, specified in seconds. Time spent buffering data MUST NOT be
included in this value.

Playback at non-normal play speed does not affect the amount of content rendered, when expressed
in time units. For example, if the client was rewinding the content, the x-duration value can be
computed as the absolute value of the difference between the starting presentation time and ending
presentation time.

For Streaming Log messages, the x-duration field MUST specify the time it took to receive the
content, in seconds.

Fractional time amounts MUST be rounded to the nearest larger integer value.

The value MUST be an integer in the range from 0 through 4,294,967,295.

The syntax of the x-duration field is defined as follows.

 x-duration=  1*10DIGIT

Example:

 31

### 2.2 Logging Message: W3C Syntax

A W3C format logging message consists of the values of various fields, each value separated from the
next by a single space character. Logging messages that adhere to this syntax are said to use the
W3C format because the syntax is conformant with the syntax for logging entries in the Extended Log
File Format (for more information, see [W3C-EXLOG]), which is defined by W3C.

Section 2.2.1 specifies the W3C format syntax used in most logging messages. Section 2.2.2 specifies
the W3C format syntax used in certain Rendering Log messages.

The sections mentioned earlier define the ordering of the fields in the W3C format syntax but not how
the values of the fields are assigned. The rules governing the values of the individual fields depend on
the logging message in which the W3C format syntax is used. For example, the s-ip field is used as
defined in section 2.1.43 for some logging messages, while other logging messages provide an
alternate definition of the s-ip field.

All W3C format syntax MUST use the UTF-8 character set as specified in [RFC3629]. In any fields that
specify a URL, such as cs-url, the URL MUST be encoded using percent-encoding, as specified in
[RFC3986] section 2.1.

29 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


A single dash character (which is represented by U+002D and by "-" in ABNF syntax) MUST be used to
indicate that the value is empty—that is, it is either not available or not applicable.

All spaces embedded within a field value MUST be replaced by an underscore character (which is
represented by U+005F and by "_" in ABNF syntax). For example, "MPEG Layer-3" would be
transformed into "MPEG_Layer-3" in a W3C-format logging message.

Note  Transformations defined in this section are not necessarily reversible. Methods for parsing,
analyzing, or extracting information from logging messages are implementation-specific and are
outside the scope of this specification.

#### 2.2.1 Basic Logging Syntax

Most logging messages contain logging information in W3C format, adhering to the syntax specified as
follows. The logging information consists of either 44 or 47 fields.

 log_data44   = c-ip SP date SP time SP c-dns SP cs-uri-stem SP c-starttime SP
                x-duration SP c-rate SP c-status SP c-playerid SP
                c-playerversion SP c-playerlanguage SP cs-User-Agent SP
                cs-Referer SP c-hostexe SP c-hostexever SP c-os SP c-osversion SP
                c-cpu SP filelength SP filesize SP avgbandwidth SP protocol SP
                transport SP audiocodec SP videocodec SP c-channelURL SP sc-bytes SP
                c-bytes SP s-pkts-sent SP c-pkts-received SP c-pkts-lost-client SP
                c-pkts-lost-net SP c-pkts-lost-cont-net SP c-resendreqs SP
                c-pkts-recovered-ECC SP c-pkts-recovered-resent SP c-buffercount SP
                c-totalbuffertime SP c-quality SP s-ip SP s-dns SP
                s-totalclients SP s-cpu-util
                [ SP cs-url SP cs-media-name SP cs-media-role ]

#### 2.2.2 Extended Logging Syntax

Certain types of "rendering" log messages (section 2.7) contain logging information in the W3C format
defined as follows. This logging information consists of 52 fields.

 log_data52   = c-ip SP date SP time SP c-dns SP cs-uri-stem SP c-starttime SP
                x-duration SP c-rate SP c-status SP c-playerid SP
                c-playerversion SP c-playerlanguage SP cs-User-Agent SP
                cs-Referer SP c-hostexe SP c-hostexever SP c-os SP c-osversion SP
                c-cpu SP filelength SP filesize SP avgbandwidth SP protocol SP
                transport SP audiocodec SP videocodec SP c-channelURL SP sc-bytes SP
                c-bytes SP s-pkts-sent SP c-pkts-received SP c-pkts-lost-client SP
                c-pkts-lost-net SP c-pkts-lost-cont-net SP c-resendreqs SP
                c-pkts-recovered-ECC SP c-pkts-recovered-resent SP c-buffercount SP
                c-totalbuffertime SP c-quality SP s-ip SP s-dns SP
                s-totalclients SP s-cpu-util SP cs-user-name SP s-session-id SP
                s-content-path SP cs-url SP cs-media-name SP c-max-bandwidth SP
                cs-media-role SP s-proxied

### 2.3 Logging Messages Sent to Web Servers

Most of the logging messages defined in this specification can be sent to a HTTP web server. The URL
for the HTTP web server for which logging messages are submitted can be specified in an .asx file (for
more information, see [MSDN-WMMETA]). Some of the compatible streaming protocols (listed in
section 1.4) can also specify the HTTP web server URL through mechanisms that are specific to the
streaming protocol. The syntax for the logging URL is defined as follows.

 log-URL = Request-URI

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

30 / 52


The resource that is identified by log-URL MUST be capable of accepting and responding to the HTTP
GET and POST request methods described in this section; however, the methods for doing so are
implementation-specific.

Prior to sending a logging message to a web server, a client SHOULD send an HTTP GET request to the
specified web server URL to validate the URL. The logging validation request MUST adhere to the
following ABNF syntax.

 web-server-validate = "GET" SP log-URL SP HTTP-Version CRLF
 *( VCHAR /CLRF )

The web server's response MUST adhere to the following ABNF syntax.

 web-server-validate-response = HTTP-Version "200 OK" CRLF
 *( VCHAR / CRLF ) "<body><h1>"
 ( "NetShow ISAPI Log Dll" /
 ( "WMS ISAPI Log Dll/"
 1*4DIGIT "." 1*4DIGIT "." 1*4DIGIT "." 1*4DIGIT ) )
 *( VCHAR / CRLF ) "</h1>" *( VCHAR / CRLF ) </body>" *( VCHAR / CRLF )

The client SHOULD send the logging message to the web server if the server's response adheres to
the syntax for web-server-validate-response. If the client sent a request to validate the URL, and the
server's response does not adhere to the syntax for web-server-validate-response, this might mean
that the URL is invalid. In this case, the client SHOULD NOT send the logging message.

When sending the logging message, the client MUST include the logging message in the body of a
HTTP POST request.

All logging message requests that are sent to a web server MUST adhere to the following ABNF
syntax:

 web-server-request = "POST" SP log-URL SP HTTP-Version CRLF
                      *( VCHAR / CRLF )
                       web-server-log

The logging message sent in the web-server-request message body MUST adhere to the following
ABNF syntax.

 web-server-log = "MX_STATS_LogLine:" SP TAB
                  log_data44; defined in section 2.2.1

All HTTP GET and POST requests sent by the client or web server MUST be syntactically correct as per
[RFC1945] or [RFC2616]. Any header or content element not explicitly represented in one of the
preceding ABNF syntax examples MUST be ignored by the recipient.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

31 / 52


For an example of logging URL validation and the subsequent transmission of a logging message to a
web server, see section 3.6.

### 2.4 Logging Message: XML Schema

Logging messages can be represented in XML. This section defines the schema used by all logging
messages for which an XML representation has been defined with the exception of the Connect-Time
Log. The XML scheme for the Connect-Time Log is defined in section 2.8.

The XML-format log embeds W3C-format logging information inside the "Summary" XML tag.
Individual logging fields are also represented using their own XML tags.

If the entity that generates the XML-format logging message (that is, the client) has access to a
Content Description, each name/value pair in the Content Description SHOULD be encoded as shown
by the "contentdescription" syntax element in the ABNF syntax as shown in the following code
example.

The Content Description is a data structure that is provided by Windows Media Services. If no Content
Description is available to the client, the "contentdescription" syntax element MUST NOT be included
in the XML-format logging message.

If the entity that generates the XML-format logging message (that is, the client) submits additional or
custom logging information, it SHOULD be encoded as shown by the "client-logging-data" syntax
element in the following ABNF syntax. For an example illustrating submission of custom logging
information, see section 3.2.

If no additional logging information is available, the "client-logging-data" syntax element MUST NOT
be included in the XML-format logging message.

The XML-format logging syntax is defined using ABNF as shown in the following code example.
Although not explicitly shown by the syntax, linear white space, including CR LF sequences, is allowed
on each side of XML tags.

 xml-tag = 1*ALPHA
 cd-name = xml-tag
 cd-value = xml-tag
 cd-name-value-pair = "<" cd-name ">"
 cd-value
     "</" cd-name ">"

 contentdescription = "<ContentDescription>"
     *cd-name-value-pair
     "</ContentDescription>"

 client-logging-data = "<" xml-tag ">"
     *cdl-name-value-pair
     "</" xml-tag ">"

 xml-log = "<XML>"
   "<Summary>" summary-log "</Summary>"
   "<c-ip>" "0.0.0.0" "</c-ip>"
   "<date>" date "</date>"
   "<time>" time "</time>"
   "<c-dns>" c-dns "</c-dns>"
   "<cs-uri-stem>" cs-uri-stem "</cs-uri-stem>"
   "<c-starttime>" c-starttime "</c-starttime>"
   "<x-duration> x-duration "</x-duration>"
   "<c-rate>" c-rate "</c-rate>"
   "<c-status>" c-status "</c-status>"
   "<c-playerid>" c-playerid "<c-playerid>"
   "<c-playerversion>" c-playerversion "</c-playerversion>"
   "<c-playerlanguage>" c-playerlanguage "</c-playerlanguage>"
   "<cs-User-Agent>" cs-User-Agent "</cs-User-Agent>"

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

32 / 52


   "<cs-Referer>" cs-Referer "<cs-Referer>"
   "<c-hostexe>" c-hostexe "</c-hostexe>"
   "<c-hostexever>" c-hostexever "</c-hostexever>"
   "<c-os>" c-os "</c-os>"
   "<c-osversion>" c-osversion "</c-osversion>"
   "<c-cpu>" c-cpu "</c-cpu>"
   "<filelength>" filelength "</filelength>"
   "<filesize>" filesize "</filesize>"
   "<avgbandwidth>" avgbandwidth "</avgbandwidth>"
   "<protocol>" protocol "</protocol>"
   "<transport>" transport "</transport>"
   "<audiocodec>" audiocodec "</audiocodec>"
   "<videocodec>" videocodec "</videocodec>"
   "<c-channelURL>" c-channelURL "</c-channelURL>"
   "<sc-bytes>" sc-bytes "</sc-bytes>"
   "<c-bytes>" c-bytes "</c-bytes>"
   "<s-pkts-sent>" s-pkts-sent "</s-pkts-sent>"
   "<c-pkts-received>" c-pkts-received "</c-pkts-received>"
   "<c-pkts-lost-client>" c-pkts-lost-client "</c-pkts-lost-client>"
   "<c-pkts-lost-net>" c-pkts-lost-net "</c-pkts-lost-net>"
   "<c-pkts-lost-cont-net>" c-pkts-lost-cont-net "</c-pkts-lost-cont-net>"
   "<c-resendreqs>" c-resendsreqs "</c-resendreqs>"
   "<c-pkts-recovered-ECC>" c-pkts-recovered-ECC "</c-pkts-recovered-ECC>"
   "<c-pkts-recovered-resent>" c-pkts-recovered-resent "</c-pkts-recovered-resent>"
   "<c-buffercount>" c-buffercount "</c-buffercount>"
   "<c-totalbuffertime>" c-totalbuffertime "</c-totalbuffertime>"
   "<c-quality>" c-quality "</c-quality>"
   "<s-ip>" "-" "</s-ip>"
   "<s-dns>" "-" "</s-dns>"
   "<s-totalclients>" "-" "</s-totalclients>"
   "<s-cpu-util>" "-" "</s-cpu-util>"
   "<cs-url>" cs-url "</cs-url>"
   [ contentdescription ]
   *client-logging-data
 "</XML>"

The syntax only defines the ordering of the fields and the XML tag assigned to each field; it does not
define how the values of the fields are assigned. The rules governing the values of the individual fields
depend on the logging message in which the XML-format syntax is used.

The XML-format logging syntax MUST use the UTF-8 character set, as specified in [RFC3629]. In any
fields that specify a URL, such as cs-url, the URL MUST be encoded using percent-encoding, as
specified in [RFC3986] section 2.1.

A single dash character (which is represented by U+002D and by "-" in ABNF syntax) MUST be used to
indicate that the value is empty—that is, it is either not available or not applicable.

All spaces embedded within a field value MUST be replaced by an underscore character (which is
represented by U+005F and by "_" in ABNF syntax). For example, "MPEG Layer-3" would be
transformed into "MPEG_Layer-3" in a W3C-format logging message.

### 2.5 Legacy Log

The Legacy Log is also called a combination log because it contains both rendering and streaming
information. The Legacy Log can be either in W3C format or XML format. A Legacy Log can be sent
either to Windows Media Services or to a web server.

#### 2.5.1 Common Definitions

The following ABNF syntax rules applies to all variants of the Legacy Log.<4>

 s-cpu-util     = "-"

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

33 / 52


 c-ip           = "0.0.0.0"
 s-dns          = "-"

The values of the following fields MUST be assigned as defined in section 2.1:

  audiocodec

  avgbandwidth























































c-buffercount

c-channelURL

c-cpu

c-dns

c-hostexe

c-hostexever

c-os

c-osversion

c-pkts-lost-client

c-pkts-lost-cont-net

c-pkts-lost-net

c-pkts-recovered-ECC

c-pkts-recovered-resent

c-playerid

c-playerlanguage

c-playerversion

c-quality

c-rate

c-resendreqs

c-starttime

c-status

c-totalbuffertime

cs-Referer

cs-media-name

cs-uri-stem

cs-url

cs-User-Agent

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

34 / 52


  date





filelength

filesize

  protocol













cs-media-role

s-pkts-sent

s-totalclients

sc-bytes

time

transport

  videocodec

  x-duration

The Legacy Log SHOULD include the optional fields cs-url, cs-media-name, and cs-media-role.<5>

#### 2.5.2 Legacy Log in W3C Format

The ABNF syntax for a Legacy Log in W3C format that is sent to Windows Media Services is defined as
follows.

 legacy-log-W3C       = log_data44              ; defined in section 2.2.1
 s-ip                 = "-"

#### 2.5.3 Legacy Log in XML Format

The ABNF syntax for a Legacy Log in XML format that is sent to Windows Media Services is defined as
follows.<6>

 legacy-log-XML    = xml-log         ; defined in section 2.4
 summary-log       = log_data44      ; defined in section  2.2.1
 s-ip              = "-"

#### 2.5.4 Legacy Log Sent to a Web Server

The ABNF syntax for a Legacy Log that is submitted to a web server is defined as follows.

 legacy-web-server-log = web-server-log     ; defined in section 2.3

The value of the s-ip field MUST be assigned as defined in section 2.1.43.

### 2.6 Streaming Log

The Streaming Log specifies how the client received streaming data but not how the client rendered
the data. A Streaming Log can be sent either to Windows Media Services or to a web server.

35 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


#### 2.6.1 Common Definitions

The following ABNF syntax rules apply to all variants of the Streaming Log.

 audiocodec        = "-"
 c-ip              = "0.0.0.0"
 s-cpu-util        = "-"
 s-dns             = "-"
 videocodec        = "-"

The values of the following fields MUST be assigned as defined in section 2.1:

  avgbandwidth

















































c-buffercount

c-channelURL

c-cpu

c-dns

c-hostexe

c-hostexever

c-os

c-osversion

c-pkts-lost-client

c-pkts-lost-cont-net

c-pkts-lost-net

c-pkts-recovered-ECC

c-pkts-recovered-resent

c-playerid

c-playerlanguage

c-playerversion

c-quality

c-rate

c-resendreqs

c-starttime

c-status

c-totalbuffertime

cs-Referer

cs-media-name

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

36 / 52








cs-uri-stem

cs-url

cs-User-Agent

  date





filelength

filesize

  protocol













cs-media-role

s-pkts-sent

s-totalclients

sc-bytes

time

transport

  x-duration

The Streaming Log MUST include the optional fields cs-url, cs-media-name, and cs-media-role.

#### 2.6.2 Streaming Log Sent to Windows Media Services

The Streaming Log sent to Windows Media Services is in XML format and MUST adhere to the following
ABNF syntax.<7><8>

 streaming-log     = xml-log          ; defined in section  2.4
 summary-log       = log_data44       ; defined in section  2.2.1
 s-ip              = "-"

#### 2.6.3 Streaming Log Sent to a Web Server

The ABNF syntax for a Streaming Log that is submitted to a web server is defined as follows.

 streaming-web-server-log = web-server-log; defined in section 2.3

The value of the s-ip field MUST be assigned as specified in section 2.1.43.

### 2.7 Rendering Log

The Rendering Log describes playback of content by a client and is submitted to the upstream origin
server (or a configured proxy) when the client ends playback. A Rendering Log can be sent either to
Windows Media Services or to a web server.

#### 2.7.1 Common Definitions

The following ABNF syntax rules apply to all variants of the Rendering Log:

37 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


 avgbandwidth            = "-"
 c-buffercount           = "-"
 c-pkts-lost-client      = "-"
 c-pkts-lost-cont-net    = "-"
 c-pkts-lost-net         = "-"
 c-pkts-received         = "-"
 c-pkts-recovered-ECC    = "-"
 c-pkts-recovered-resent = "-"
 c-quality               = "100"
 c-resendreqs            = "-"
 c-totalbuffertime       = "-"
 protocol                = "Cache"
 transport               = "-"

The values of the following fields MUST be assigned as defined in section 2.1:

  audiocodec





































c-channelURL

c-cpu

c-hostexe

c-hostexever

c-ip

c-os

c-osversion

c-playerid

c-playerlanguage

c-playerversion

c-rate

c-starttime

c-status

cs-Referer

cs-media-name

cs-uri-stem

cs-url

cs-User-Agent

  date









filelength

filesize

s-cpu-util

s-dns

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

38 / 52












cs-media-role

s-pkts-sent

s-totalclients

sc-bytes

time

  videocodec

  x-duration

The Rendering Log MUST include the optional fields cs-url, cs-media-name, and cs-media-role.

#### 2.7.2 Rendering Log Sent to Windows Media Services

The Rendering Log sent to Windows Media Services is in XML format and MUST adhere to the following
ABNF syntax.

 rendering-log    = xml-log           ; defined in section 2.4
 summary-log      = log_data52        ; defined in section 2.2.2

The values of the following fields MUST be assigned as defined in section 2.1: c-max-bandwidth, cs-
user-name, s-content-path, s-ip, s-proxied, and s-session-id.

#### 2.7.3 Rendering Log Sent to a Web Server

The ABNF syntax for a Rendering Log that is submitted to a web server is defined as follows.

 rendering-web-server-log = web-server-log; defined in section 2.3

The value of the c-ip field MUST be assigned as defined in section 2.1.8. The value of the s-ip field
MUST be assigned as defined in section 2.1.43.

### 2.8 Connect-Time Log

The purpose of the Connect-Time Log is to specify some minimal amount of logging information about
the client. It can be useful in cases where a client starts to stream (2) some content but is
disconnected from the network before it has the opportunity to create a Streaming Log.

If a client sends a Connect-Time Log to the server at the start of the streaming session, the
Connect-Time Log ensures that the server has received at least this minimal logging information in the
case where the client subsequently is disconnected from the network.

Connect-Time Logs are not defined for web servers. Connect-Time Logs are defined only in XML
format, and the ABNF syntax is as follows.

 connect-time-log = "<XML>"
   "<Summary>"
   "</Summary>"
   "<c-dns>" c-dns "</c-dns>"
   "<c-ip>" c-ip "</c-ip>"
   "<c-os>" c-os "</c-os>"
   "<c-osversion>" c-osversion "</c-osversion>"
   "<date>" date "</date>"

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

39 / 52


   "<time>" time "</time>"
   "<c-cpu>" c-cpu "</c-cpu>"
   "<transport>" transport "</transport>"
 "</XML>"

 c-ip             = "0.0.0.0"

The values of the following fields MUST be assigned as defined in section 2.1:







c-dns

c-os

c-osversion

  date







time

c-cpu

transport

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

40 / 52


## 3 Structure Examples

### 3.1 Legacy Logging Message

 The following is an example of a legacy logging message in W3C format.

 0.0.0.0 2003-09-27 00:27:24 - http://10.194.20.175/mcast1200K 0 42 1
 200 {3300AD50-2C39-46c0-AE0A-B4C904C7848E} 9.0.0.2980 en-US
 WMFSDK/9.0.0.2980_WMPlayer/9.0.0.3008 - wmplayer.exe 9.0.0.2980
 Windows_XP 5.1.0.2600 Pentium 1801 268885194 1255347 http TCP
 Windows_Media_Audio_9 Windows_Media_Video_9 - - 6321233 - 4496 0 0 0 0
 0 0 1 0 100 - - - -

The following is an example of a legacy logging message in XML format.

 <XML>
 <Summary>0.0.0.0 2003-09-27 00:27:24 - http://10.194.20.175/mcast1200K 0 42 1 200
 {3300AD50-2C39-46c0-AE0A-B4C904C7848E} 9.0.0.2980
 en-US WMFSDK/9.0.0.2980_WMPlayer/9.0.0.3008 - wmplayer.exe 9.0.0.2980
 Windows_XP 5.1.0.2600 Pentium 1801 268885194 1255347
 http TCP Windows_Media_Audio_9 Windows_Media_Video_9
 - - 6321233 - 4496 0 0 0 0 0 0 1 0 100 - - - -
 http://10.194.20.175/mcast1200K?WMBitrate=6000000 30MinTV_1200k_1s_1s_0Q.wmv -
 </Summary>
 <c-ip>0.0.0.0</c-ip>
 <date>2003-09-27</date>
 <time>00:27:24</time>
 <c-dns>-</c-dns>
 <cs-uri-stem>http://10.194.20.175/mcast1200K</cs-uri-stem>
 <c-starttime>0</c-starttime>
 <x-duration>42</x-duration>
 <c-rate>1</c-rate>
 <c-status>200</c-status>
 <c-playerid>{3300AD50-2C39-46c0-AE0A-B4C904C7848E}</c-playerid>
 <c-playerversion>9.0.0.2980</c-playerversion>
 <c-playerlanguage>en-US</c-playerlanguage>
 <cs-User-Agent>WMFSDK/9.0.0.2980_WMPlayer/9.0.0.3008</cs-User-Agent>
 <cs-Referer>-</cs-Referer>
 <c-hostexe>wmplayer.exe</c-hostexe>
 <c-hostexever>9.0.0.2980</c-hostexever>
 <c-os>Windows_XP</c-os>
 <c-osversion>5.1.0.2600</c-osversion>
 <c-cpu>Pentium</c-cpu>
 <filelength>1801</filelength>
 <filesize>268885194</filesize>
 <avgbandwidth>1255347</avgbandwidth>
 <protocol>http</protocol>
 <transport>TCP</transport>
 <audiocodec>Windows_Media_Audio_9</audiocodec>
 <videocodec>Windows_Media_Video_9</videocodec>
 <c-channelURL>-</c-channelURL>
 <sc-bytes>-</sc-bytes>
 <c-bytes>6321233</c-bytes>
 <s-pkts-sent>-</s-pkts-sent>
 <c-pkts-received>4496</c-pkts-received>
 <c-pkts-lost-client>0</c-pkts-lost-client>
 <c-pkts-lost-net>0</c-pkts-lost-net>
 <c-pkts-lost-cont-net>0</c-pkts-lost-cont-net>
 <c-resendreqs>0</c-resendreqs>
 <c-pkts-recovered-ECC>0</c-pkts-recovered-ECC>
 <c-pkts-recovered-resent>0</c-pkts-recovered-resent>
 <c-buffercount>1</c-buffercount>

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

41 / 52


 <c-totalbuffertime>0</c-totalbuffertime>
 <c-quality>100</c-quality>
 <s-ip>-</s-ip>
 <s-dns>-</s-dns>
 <s-totalclients>-</s-totalclients>
 <s-cpu-util>-</s-cpu-util>
 <cs-url>http://10.194.20.175/mcast1200K?WMBitrate=6000000</cs-url>
 <ContentDescription>
 <WMS_CONTENT_DESCRIPTION_PLAYLIST_ENTRY_URL>30MinTV_1200k_1s_1s_0Q.wmv</WMS_CONTENT_DESCRIPTI
ON_PLAYLIST_ENTRY_URL>
 <WMS_CONTENT_DESCRIPTION_COPIED_METADATA_FROM_PLAYLIST_FILE>1</WMS_CONTENT_DESCRIPTION_COPIED
_METADATA_FROM_PLAYLIST_FILE>
 <WMS_CONTENT_DESCRIPTION_PLAYLIST_ENTRY_DURATION>1800501</WMS_CONTENT_DESCRIPTION_PLAYLIST_EN
TRY_DURATION>
 <WMS_CONTENT_DESCRIPTION_PLAYLIST_ENTRY_START_OFFSET>1450</WMS_CONTENT_DESCRIPTION_PLAYLIST_E
NTRY_START_OFFSET>
 <WMS_CONTENT_DESCRIPTION_SERVER_BRANDING_INFO>WMServer/9.0</WMS_CONTENT_DESCRIPTION_SERVER_BR
ANDING_INFO>
 </ContentDescription>
 </XML>

The following is an example of how a Legacy Log can appear as sent to a web server.

 MX_STATS_LogLine:  0.0.0.0 2000-06-14 01:18:58 -
 mmsu://server.example.com/testfile.wma 30 1 1 200 {35301A88-93D3-4F3A-
 A284-30F7A611CD23} 7.0.0.1938 en-US - - wmplayer.exe 7.0.0.1938
 Windows_2000 5.0.0.2195 Pentium 225 4551684 1528 mms UDP - - - - 29868
 - 4 0 0 0 0 0 0 0 0 100 172.29.237.102 - - -

### 3.2 Defining Custom Namespaces in an XML Log

An .asx file (for more information, see [MSDN-WMMETA]) can be used to append log data to the XML
log structure. Vendors can define any number of custom namespaces and name-value pairs in the
"client-logging-data" structure, as specified in section 2.4, following the Content Description structure.

The following example illustrates how to add the cs-media-role field (section 2.1.30) by using an
.asx file.

 <ASX version="3.0">
   <ENTRY>
     <TITLE> My Title </TITLE>
     <Author> My Author </Author>
     <PARAM name="log:cs-media-role" value="Advertisement" />
     <REF href="http://www.server.example.com/live" />
   </ENTRY>
 </ASX>

The additional and/or custom logging information is specified through the use of the PARAM element.
To use the PARAM element in this way, the NAME attribute is set to "log:" followed by a log field name
and a corresponding VALUE attribute. The log field specified in the NAME attribute is set to the value
of the VALUE attribute. If the log does not already contain a field with the specified name, it will be
added.

An XML namespace has to be defined for each custom log field specified in an .asx file. This
namespace is appended to the NAME attribute and is separated from the field name by a second colon
(":"). Because everything after the second colon is treated as a namespace, ensure that the field
name does not contain a colon.

The following example illustrates the specification of custom log fields using an .asx file.

42 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


 <ASX version="3.0">
   <ENTRY>
     <TITLE> My Title </TITLE>
     <Author> My Author </Author>
     <PARAM name="log:vendor-field1:VendorNameSpace" value="Value1" />
     <PARAM name="log:vendor-field2:VendorNameSpace" value="Value2" />
     <REF href="http://www.server.example.com/live" />
   </ENTRY>
 </ASX>

When an XML log is sent to a server for this .asx file, the new namespace is inserted after the Content
Description section, as shown in the following example (many log fields extraneous to this example
have been omitted for brevity and clarity).

 <XML>
   <Summary>0.0.0.0 2003-09-27 00:27:24 ... </Summary>
   <c-ip>0.0.0.0</c-ip>
   <date>2003-09-27</date>
   <time>00:27:24</time>
   ...
   <ContentDescription>
   ...
   </ContentDescription>
   <VendorNameSpace>
     <vendor-field1>Value1</vendor-field1>
     <vendor-field2>Value2</vendor-field2>
   </VendorNameSpace>
 </XML>

### 3.3 Example Streaming Log Messages

The following is an example of a Streaming Log in XML format.

 <XML>
 <Summary>0.0.0.0 2006-05-01 21:34:01 -
 http://server.example.com/content.wmv 4 0 1 200 {3300AD50-2C39-46c0-
 AE0A-3E0B6EFB86DC} 10.0.0.3802 en-US
 Mozilla/4.0_(compatible;_MSIE_6.0;_Windows_NT_5.1)_(WMFSDK/10.0.0.3802)
 _WMPlayer/10.0.0.4019 http://bar.microsoft.com iexplore.exe
 6.0.2900.2180 Windows_XP 5.1.0.2600 Pentium 130 638066 - http TCP - - -
 -0 - 0 0 0 0 0 0 0 0 0 100 - - - -
 http://server.example.com/content.wmv - -</Summary>
 <c-ip>0.0.0.0</c-ip>
 <date>2006-05-01</date>
 <time>21:34:01</time>
 <c-dns>-</c-dns>
 <cs-uri-stem>http://server.example.com/content.wmv</cs-uri-stem>
 <c-starttime>4</c-starttime>
 <x-duration>0</x-duration>
 <c-rate>1</c-rate>
 <c-status>200</c-status>
 <c-playerid>{3300AD50-2C39-46c0-AE0A-3E0B6EFB86DC}</c-playerid>
 <c-playerversion>10.0.0.3802</c-playerversion>
 <c-playerlanguage>en-US</c-playerlanguage>
 <cs-User-
Agent>Mozilla/4.0_(compatible;_MSIE_6.0;_Windows_NT_5.1)_(WMFSDK/10.0.0.3802)_WMPlayer/10.0.0
.4019</cs-User-Agent>
 <cs-Referer>http://bar.microsoft.com</cs-Referer>
 <c-hostexe>iexplore.exe</c-hostexe>

43 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


 <c-hostexever>6.0.2900.2180</c-hostexever>
 <c-os>Windows_XP</c-os>
 <c-osversion>5.1.0.2600</c-osversion>
 <c-cpu>Pentium</c-cpu>
 <filelength>130</filelength>
 <filesize>638066</filesize>
 <avgbandwidth>-</avgbandwidth>
 <protocol>http</protocol>
 <transport>TCP</transport>
 <audiocodec>-</audiocodec>
 <videocodec>-</videocodec>
 <c-channelURL>-</c-channelURL>
 <sc-bytes>-</sc-bytes>
 <c-bytes>0</c-bytes>
 <s-pkts-sent>-</s-pkts-sent>
 <c-pkts-received>0</c-pkts-received>
 <c-pkts-lost-client>0</c-pkts-lost-client>
 <c-pkts-lost-net>0</c-pkts-lost-net>
 <c-pkts-lost-cont-net>0</c-pkts-lost-cont-net>
 <c-resendreqs>0</c-resendreqs>
 <c-pkts-recovered-ECC>0</c-pkts-recovered-ECC>
 <c-pkts-recovered-resent>0</c-pkts-recovered-resent>
 <c-buffercount>0</c-buffercount>
 <c-totalbuffertime>0</c-totalbuffertime>
 <c-quality>100</c-quality>
 <s-ip>-</s-ip>
 <s-dns>-</s-dns>
 <s-totalclients>-</s-totalclients>
 <s-cpu-util>-</s-cpu-util>
 <cs-url>http://server.example.com/content.wmv</cs-url>
 </XML>

The following is an example of how a Streaming Log can appear as sent to a web server.

 MX_STATS_LogLine:  0.0.0.0 2000-06-14 01:18:58 -
 mmsu://server.example.com/testfile.wma 30 1 1 200 {35301A88-93D3-4F3A-
 A284-30F7A611CD23} 7.0.0.1938 en-US - - wmplayer.exe 7.0.0.1938
 Windows_2000 5.0.0.2195 Pentium 225 4551684 1528 mms UDP - - - - 29868
 - 4 0 0 0 0 0 0 0 0 100 172.29.237.102 - - -
 mmsu://server.example.com/testfile.wma - -

### 3.4 Example Rendering Log Messages

The following is an example of a Rendering Log in XML format.

 <XML>
 <Summary>0.0.0.0 2006-05-01 21:34:01 -
 http://server.example.com/content.wmv 4 0 1 200 {3300AD50-2C39-46c0-
 AE0A-3E0B6EFB86DC} 10.0.0.3802 en-US
 Mozilla/4.0_(compatible;_MSIE_6.0;_Windows_NT_5.1)_(WMFSDK/10.0.0.3802)
 _WMPlayer/10.0.0.4019 http://bar.microsoft.com iexplore.exe
 6.0.2900.2180 Windows_XP 5.1.0.2600 Pentium 130 638066 - Cache -
 Windows_Media_Audio_9 Windows_Media_Video_9 - - 0 - - - - - - - - - -
 100 - - - - - - - http://server.example.com/content.wmv - - - 0
 </Summary>
 <c-ip>0.0.0.0</c-ip>
 <date>2006-05-01</date>
 <time>21:34:01</time>
 <c-dns>-</c-dns>
 <cs-uri-stem>http://server.example.com/content.wmv</cs-uri-stem>
 <c-starttime>4</c-starttime>
 <x-duration>0</x-duration>
 <c-rate>1</c-rate>

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

44 / 52


 <c-status>200</c-status>
 <c-playerid>{3300AD50-2C39-46c0-AE0A-3E0B6EFB86DC}</c-playerid>
 <c-playerversion>10.0.0.3802</c-playerversion>
 <c-playerlanguage>en-US</c-playerlanguage>
 <cs-User-Agent>Mozilla/4.0_(compatible;_MSIE_6.0;_Windows_NT_5.1)
 _(WMFSDK/10.0.0.3802)_WMPlayer/10.0.0.4019</cs-User-Agent>
 <cs-Referer>http://bar.microsoft.com</cs-Referer>
 <c-hostexe>iexplore.exe</c-hostexe>
 <c-hostexever>6.0.2900.2180</c-hostexever>
 <c-os>Windows_XP</c-os>
 <c-osversion>5.1.0.2600</c-osversion>
 <c-cpu>Pentium</c-cpu>
 <filelength>130</filelength>
 <filesize>638066</filesize>
 <avgbandwidth>-</avgbandwidth>
 <protocol>Cache</protocol>
 <transport>-</transport>
 <audiocodec>Windows_Media_Audio_9</audiocodec>
 <videocodec>Windows_Media_Video_9</videocodec>
 <c-channelURL>-</c-channelURL>
 <sc-bytes>-</sc-bytes>
 <c-bytes>0</c-bytes>
 <s-pkts-sent>-</s-pkts-sent>
 <c-pkts-received>-</c-pkts-received>
 <c-pkts-lost-client>-</c-pkts-lost-client>
 <c-pkts-lost-net>-</c-pkts-lost-net>
 <c-pkts-lost-cont-net>-</c-pkts-lost-cont-net>
 <c-resendreqs>-</c-resendreqs>
 <c-pkts-recovered-ECC>-</c-pkts-recovered-ECC>
 <c-pkts-recovered-resent>-</c-pkts-recovered-resent>
 <c-buffercount>-</c-buffercount>
 <c-totalbuffertime>-</c-totalbuffertime>
 <c-quality>100</c-quality>
 <s-ip>-</s-ip>
 <s-dns>-</s-dns>
 <s-totalclients>-</s-totalclients>
 <s-cpu-util>-</s-cpu-util>
 <cs-url>http://server.example.com/content.wmv</cs-url>
 </XML>

The following is an example of how a Rendering Log can appear as sent to a web server.

 MX_STATS_LogLine:  0.0.0.0 2000-06-14 01:18:58 -
 mms://server.example.com/test.wma 30 1 1 200 {35301A88-93D3-4F3A-A284-
 30F7A611CD23} 7.0.0.1938 en-US - - wmplayer.exe 7.0.0.1938 Windows_2000
 5.0.0.2195 Pentium 225 4551684 1528 Cache - - - - - 29868 - - - - - - -
 - - - 100 - - - - mms://server.example.com/test.wma - -

### 3.5 Example Connect-Time Log Message

The following is an example of a Connect-Time Log message in XML format.

 <XML>
 <Summary></Summary>
 <c-dns>-</c-dns>
 <c-ip>0.0.0.0</c-ip>
 <c-os>Windows</c-os>
 <c-osversion>6.0.0.6000</c-osversion>
 <date>2006-08-30</date>
 <time>13:18:44</time>
 <c-cpu>Pentium</c-cpu>
 <transport>TCP</transport>
 </XML>

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

45 / 52


### 3.6 Example Log Sent to a Web Server

 The following is an example of a client validating a logging URL and subsequently transmitting a
logging message to the web server.

 GET /scripts/wmsiislog.dll HTTP/1.1
 User-Agent: NSPlayer
 Host: WebServer:8080
 Connection: Keep-Alive
 Cache-Control: no-cache

 HTTP/1.1 200 OK
 Connection: close
 Date: Wed, 27 Jun 2007 02:54:23 GMT
 Server: Microsoft-IIS/6.0
 Content-Type: text/html

 <head><title>WMS ISAPI Log Dll/9.00.00.3372</title></head>
 <body><h1>WMS ISAPI Log Dll/9.00.00.3372</h1></body>

 POST /scripts/wmsiislog.dll HTTP/1.1
 Content-Type: text/plain;charset=UTF-8
 User-Agent: NSPlayer
 Host: WebServer:8080
 Content-Length: 424
 Connection: Keep-Alive
 Cache-Control: no-cache

 MX_STATS_LogLine: .0.0.0.0 2007-06-27 02:52:39 - asfm://239.192.50.29:30864 0 39 1 200
 {3300AD50-2C39-46c0-AE0A-0572F2EA5330} 10.0.0.4054 en-US
 WMFSDK/10.0.0.4054_WMPlayer/10.0.0.4036 - wmplayer.exe 10.0.0.3802 Windows_XP 5.1.0.2600
 Pentium 229 10413011 411536 asfm UDP Windows_Media_Audio_9.2 -
 http://WebServer:8080/multicast.nsc - 2170350 - 182 0 0 0 0 0 0 1 3 100 239.192.50.29 - -
 - http://WebServer:8080/multicast.nsc - -
 HTTP/1.1 200 OK
 Server: Microsoft-IIS/6.0
 Date: Wed, 27 Jun 2007 02:54:23 GMT
 Connection: close

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

46 / 52


## 4 Security Considerations

A server that receives a logging message validates the syntax of the fields. For example, the server
checks that logging fields that are supposed to contain numerical data really do so, and that no invalid
characters, such as control characters, are present. Invalid fields or characters could cause any tools
that process the logging information to malfunction.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

47 / 52


## 5 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

The terms "earlier" and "later", when used with a product version, refer to either all preceding
versions or all subsequent versions, respectively. The term "through" refers to the inclusive range of
versions. Applicable Microsoft products are listed chronologically in this section.

Windows Client

  Windows NT operating system

  Windows 2000 Professional operating system

  Windows XP operating system

  Windows Vista operating system

  Windows 7 operating system

  Windows 8 operating system

  Windows 8.1 operating system

  Windows 10 operating system

  Windows 11 operating system

Windows Server

  Windows NT

  Windows 2000 Server operating system

  Windows Server 2003 operating system

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

48 / 52

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024


SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 2.1.5: Windows Media Player 6.4 specifies the Internet host name in the c-dns field.

<2> Section 2.1.10: On Windows Vista and Windows 7, c-os is set to "Windows".

<3> Section 2.1.25.2: Windows Media Player 6.4, Windows Media Format 7.0 SDK, Windows Media
Format 7.1 SDK, and Windows Media Player for Windows XP never specify status code 210.

<4> Section 2.5.1: Windows Media Player 6.4 specifies its own IP address in the c-ip field. Windows
Media Format 7.0 SDK, Windows Media Format 7.1 SDK, and Windows Media Player for Windows XP
specify their own IP address in the c-ip field depending on the current setting of a configuration value
in the user interface.

<5> Section 2.5.1: Windows Media Player 6.4, Windows Media Format 7.0 SDK, Windows Media
Format 7.1 SDK, and Windows Media Player for Windows XP never include the three optional fields.

<6> Section 2.5.3: Windows Media Format 9 Series SDK, Windows Media Format 9.5 SDK, Windows
Vista, and Windows 7 do not include the "contentdescription" and "client-logging-data" syntax
elements in the XML-format logging message when using RTSP [MS-RTSP].

<7> Section 2.6.2: Windows Media Format 9 Series SDK, Windows Media Format 9.5 SDK, Windows
Vista and later do not include the "contentdescription" and "client-logging-data" syntax elements in
the XML-format logging message when using RTSP [MS-RTSP].

<8> Section 2.6.2: When going through a proxy, Windows Media Services on Windows Server 2003
operating system with Service Pack 1 (SP1), Windows Server 2008, and Windows Server 2008 R2
duplicate the "<c-channelURL>", "</c-channelURL>", "<cs-media-role>", and "</cs-media-role>"
tags. For each tag that is duplicated, the duplicate tag immediately follows the original tag.

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

49 / 52


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

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

50 / 52


## 7 Index
A

Applicability 9
audiocodec 10
avgbandwidth 11

B

Basic logging syntax 30

C

c-buffercount 11
c-bytes 20
c-channelURL 20
c-cpu 11
c-dns 12
Change tracking 50
c-hostexe 12
c-hostexever 12
c-ip (section 2.1.8 12, section 2.1.9 13)
Common data types and fields 10
Connect-time log 39
c-os 13
c-osversion 13
c-pkts-lost-client 14
c-pkts-lost-cont-net 14
c-pkts-lost-net 14
c-pkts-received 15
c-pkts-recovered-ECC 15
c-pkts-recovered-resent 15
c-playerid 16
c-playerlanguage 17
c-playerversion 17
c-quality 17
c-rate 18
c-resendreqs 18
cs-media-name 20
cs-media-role 21
cs-Referer 21
c-starttime 18
c-status 19
cs-uri-stem 22
cs-url 22
cs-User-Agent 23
cs-user-name 23
c-totalbuffertime 19

D

Data types and fields - common 10
date 24
Defining Custom Namespaces in an XML Log

example 42

Details
   common data types and fields 10

E

Example Connect-Time Log Message example 45
Example Log Sent to a Web Server example 46

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

Example Rendering Log Messages example 44
Example Streaming Log Messages example 43
Examples
   Defining Custom Namespaces in an XML Log 42
   Example Connect-Time Log Message 45
   Example Log Sent to a Web Server 46
   Example Rendering Log Messages 44
   Example Streaming Log Messages 43
   Legacy Logging Message 41
   legacy logging message example 41
Extended logging syntax 30

F

Fields - vendor-extensible 9
filelength 24
filesize 24

G

Glossary 7

I

Implementer - security considerations 47
Informative references 9
Introduction 7

L

Legacy log
   common definitions 33
   overview 33
   sent to web server 35
   W3C format 35
   XML format 35
Legacy Logging Message example 41
Localization 9
Log data fields 10
Logging message - W3C syntax 29
Logging message - WXML schema 32
Logging message sent to web servers 30

N

Normative references 8

O

Overview (synopsis) 9

P

Product behavior 48
protocol 24

R

References 8
   informative 9
   normative 8

51 / 52


Relationship to other protocols 9
Relationship to protocols and other structures 9
Rendering log
   common definitions 37
   overview 37
   sent to web server 39
   sent to Windows Media Services 39

S

sc-bytes 28
s-content-path 25
s-cpu-util 25
s-dns 26
Security 47
Security - implementer considerations 47
s-ip 26
s-pkts-sent 26
s-proxied 27
s-session-id 27
Status Code 200 (No Error) 19
Status Code 210 (Client Successfully Reconnected)

19

s-totalclients 27
Streaming log
   common definitions 36
   overview 35
   sent to web server 37
   sent to Windows Media Services 37
Structures
   connect-time log 39
   legacy log 33
   log data fields 10
   logging message - W3C syntax 29
   logging message - XML schema 32
   logging message sent to web servers 30
   overview 10
   rendering log 37
   streaming log 35

T

time 28
Tracking changes 50
transport 28

V

Vendor-extensible fields 9
Versioning 9
videocodec 28

X

x-duration 29

[MS-WMLOG] - v20240423
Windows Media Log Data Structure
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

52 / 52

