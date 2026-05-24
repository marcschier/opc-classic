[MS-WDVSE]:

Web Distributed Authoring and Versioning (WebDAV)
Protocol: Server Extensions

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

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

1 / 27


Revision Summary

Date

Revision
History

Revision
Class

Comments

7/20/2007

0.1

9/28/2007

0.2

Major

Minor

MCPP Milestone 5 Initial Availability

Clarified the meaning of the technical content.

10/23/2007  0.2.1

Editorial

Changed language and formatting in the technical content.

11/30/2007  0.2.2

Editorial

Changed language and formatting in the technical content.

1/25/2008

0.2.3

Editorial

Changed language and formatting in the technical content.

3/14/2008

0.2.4

Editorial

Changed language and formatting in the technical content.

5/16/2008

0.2.5

Editorial

Changed language and formatting in the technical content.

6/20/2008

1.0

Major

Updated and revised the technical content.

7/25/2008

1.0.1

Editorial

Changed language and formatting in the technical content.

8/29/2008

1.0.2

Editorial

Changed language and formatting in the technical content.

10/24/2008  1.0.3

Editorial

Changed language and formatting in the technical content.

12/5/2008

1.0.4

Editorial

Changed language and formatting in the technical content.

1/16/2009

2.0

2/27/2009

3.0

Major

Major

Updated and revised the technical content.

Updated and revised the technical content.

4/10/2009

3.0.1

Editorial

Changed language and formatting in the technical content.

5/22/2009

3.0.2

Editorial

Changed language and formatting in the technical content.

7/2/2009

4.0

Major

Updated and revised the technical content.

8/14/2009

4.0.1

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

4.1.3

Editorial

Changed language and formatting in the technical content.

3/12/2010

4.1.4

Editorial

Changed language and formatting in the technical content.

4/23/2010

4.1.5

Editorial

Changed language and formatting in the technical content.

6/4/2010

4.1.6

Editorial

Changed language and formatting in the technical content.

7/16/2010

4.1.6

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

4.1.6

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

4.1.6

11/19/2010  4.1.6

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

2 / 27


Date

Revision
History

Revision
Class

Comments

technical content.

1/7/2011

5.0

Major

Updated and revised the technical content.

2/11/2011

5.0

3/25/2011

5.0

5/6/2011

5.0

6/17/2011

5.1

9/23/2011

6.0

12/16/2011  7.0

3/30/2012

7.0

None

None

None

Minor

Major

Major

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

7/12/2012

7.0

None

No changes to the meaning, language, or formatting of the
technical content.

10/25/2012  7.0

1/31/2013

7.1

8/8/2013

8.0

11/14/2013  8.1

2/13/2014

8.1

None

Minor

Major

Minor

None

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

No changes to the meaning, language, or formatting of the
technical content.

5/15/2014

8.1

None

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

9.0

Major

Significantly changed the technical content.

10/16/2015  9.0

7/14/2016

9.0

6/1/2017

9.0

9/15/2017

10.0

9/12/2018

11.0

4/7/2021

12.0

6/25/2021

13.0

4/23/2024

14.0

9/16/2024

14.0

None

None

None

Major

Major

Major

Major

Major

None

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

No changes to the meaning, language, or formatting of the

3 / 27

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024


Date

Revision
History

Revision
Class

Comments

technical content.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

4 / 27


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
    - [2.2.1 Translate Request Header](#221-translate-request-header)
    - [2.2.2 MS-Author-Via Response Header](#222-ms-author-via-response-header)
    - [2.2.3 Depth "noroot" Request Header Extension](#223-depth-noroot-request-header-extension)
    - [2.2.4 SEARCH Method](#224-search-method)
    - [2.2.5 Deviations from RFC4918](#225-deviations-from-rfc4918)
      - [2.2.5.1 Additional "Live" Properties](#2251-additional-live-properties)
      - [2.2.5.2 Collections and Locking](#2252-collections-and-locking)
      - [2.2.5.3 Multiple Property Elements](#2253-multiple-property-elements)
- [3 Protocol Details](#3-protocol-details)
  - [3.1 WebDAV Client Details](#31-webdav-client-details)
    - [3.1.1 Abstract Data Model](#311-abstract-data-model)
    - [3.1.2 Timers](#312-timers)
    - [3.1.3 Initialization](#313-initialization)
    - [3.1.4 Higher-Layer Triggered Events](#314-higher-layer-triggered-events)
    - [3.1.5 Processing Events and Sequencing Rules](#315-processing-events-and-sequencing-rules)
      - [3.1.5.1 Translate Request Header](#3151-translate-request-header)
      - [3.1.5.2 MS-Author-Via Response Header](#3152-ms-author-via-response-header)
      - [3.1.5.3 Depth "noroot" Request Header Extension](#3153-depth-noroot-request-header-extension)
      - [3.1.5.4 SEARCH Method](#3154-search-method)
    - [3.1.6 Timer Events](#316-timer-events)
    - [3.1.7 Other Local Events](#317-other-local-events)
  - [3.2 WebDAV Server Details](#32-webdav-server-details)
    - [3.2.1 Abstract Data Model](#321-abstract-data-model)
    - [3.2.2 Timers](#322-timers)
    - [3.2.3 Initialization](#323-initialization)
    - [3.2.4 Higher-Layer Triggered Events](#324-higher-layer-triggered-events)
    - [3.2.5 Processing Events and Sequencing Rules](#325-processing-events-and-sequencing-rules)
      - [3.2.5.1 Translate Request Header](#3251-translate-request-header)
      - [3.2.5.2 MS-Author-Via Response Header](#3252-ms-author-via-response-header)
      - [3.2.5.3 Depth "noroot" Request Header Extension](#3253-depth-noroot-request-header-extension)
      - [3.2.5.4 SEARCH Method](#3254-search-method)
    - [3.2.6 Timer Events](#326-timer-events)
    - [3.2.7 Other Local Events](#327-other-local-events)
- [4 Protocol Examples](#4-protocol-examples)
  - [4.1 MS-Author-Via Response Header](#41-ms-author-via-response-header)
  - [4.2 Translate Request Header](#42-translate-request-header)
  - [4.3 Depth "noroot" Request Header Extension](#43-depth-noroot-request-header-extension)
- [5 Security](#5-security)
  - [5.1 Security Considerations for Implementers](#51-security-considerations-for-implementers)
  - [5.2 Index of Security Parameters](#52-index-of-security-parameters)
- [6 Appendix A: Product Behavior](#6-appendix-a-product-behavior)
- [7 Change Tracking](#7-change-tracking)
- [8 Index](#8-index)

## 1 Introduction

The Web Distributed Authoring and Versioning (WebDAV) Protocol defined in [RFC4918] extends the
standard Hypertext Transfer Protocol (HTTP) mechanisms defined in [RFC2068] to provide file access
and content management over the Internet. The WebDAV Protocol enables an Internet–based file
system. However, some types of files (for example, files with programmatically derived content) are
not easily managed by WebDAV and some protocol interactions are suboptimal for file system usage.

These WebDAV protocol server extensions (WDVSE) extend WebDAV by introducing new HTTP request
and response headers that both enable the file types that are not currently manageable and optimize
protocol interactions for file system clients. These extensions also introduce a new WebDAV method
that is used to send search queries to disparate search providers.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

### 1.1 Glossary

This document uses the following terms:

Augmented Backus-Naur Form (ABNF): A modified version of Backus-Naur Form (BNF),

commonly used by Internet specifications. ABNF notation balances compactness and simplicity
with reasonable representational power. ABNF differs from standard BNF in its definitions and
uses of naming rules, repetition, alternatives, order-independence, and value ranges. For more
information, see [RFC5234].

collection: A resource that contains a set of URIs that identify member resources. Use of this term

is consistent with what is specified in [RFC4918] section 5.2.

entity: Any document on a server that is accessible by using a Hypertext Transfer Protocol

(HTTP) URL.

Hypertext Transfer Protocol (HTTP): An application-level protocol for distributed, collaborative,
hypermedia information systems (text, graphic images, sound, video, and other multimedia
files) on the World Wide Web.

Hypertext Transfer Protocol Secure (HTTPS): An extension of HTTP that securely encrypts and
decrypts web page requests. In some older protocols, "Hypertext Transfer Protocol over Secure
Sockets Layer" is still used (Secure Sockets Layer has been deprecated). For more information,
see [SSL3] and [RFC5246].

locking: A mechanism that is used for overwrite protection. Locking can be applied to individual

resources or to entire collection hierarchies (see [RFC4918] sections 6 and 7).

property: A name/value pair that associates metadata with a resource. This term is used as

specified in [RFC4918] section 4.

Secure Sockets Layer (SSL): A security protocol that supports confidentiality and integrity of

messages in client and server applications that communicate over open networks. SSL supports
server and, optionally, client authentication using X.509 certificates [X509] and [RFC5280]. SSL
is superseded by Transport Layer Security (TLS). TLS version 1.0 is based on SSL version
3.0 [SSL3].

Transport Layer Security (TLS): A security protocol that supports confidentiality and integrity of
messages in client and server applications communicating over open networks. TLS supports
server and, optionally, client authentication by using X.509 certificates (as specified in [X509]).
TLS is standardized in the IETF TLS working group.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

7 / 27


Uniform Resource Locator (URL): A string of characters in a standardized format that identifies

a document or resource on the World Wide Web. The format is as specified in [RFC1738].

Web Distributed Authoring and Versioning Protocol (WebDAV): The Web Distributed

Authoring and Versioning Protocol, as described in [RFC2518] or [RFC4918].

web server: A server computer that hosts websites and responds to requests from applications.

WebDAV client: A computer that uses WebDAV, as described in [RFC2518] or [RFC4918], to

retrieve data from a WebDAV server.

WebDAV server: A computer that supports WebDAV, as described in [RFC2518] or [RFC4918],

and responds to requests from WebDAV clients.

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

[RFC2068] Fielding, R., Gettys, J., Mogul, J., et al., "Hypertext Transfer Protocol -- HTTP/1.1", RFC
2068, January 1997, https://www.rfc-editor.org/info/rfc2068

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2246] Dierks, T., and Allen, C., "The TLS Protocol Version 1.0", RFC 2246, January 1999,
https://www.rfc-editor.org/info/rfc2246

[RFC2291] Slein, J., Vitali, F., Whitehead, E., et al., "Requirements for a Distributed Authoring and
Versioning Protocol for the World Wide Web", RFC 2291, February 1998, https://www.rfc-
editor.org/info/rfc2291

[RFC2616] Fielding, R., Gettys, J., Mogul, J., et al., "Hypertext Transfer Protocol -- HTTP/1.1", RFC
2616, June 1999, https://www.rfc-editor.org/info/rfc2616

[RFC4234] Crocker, D., Ed., and Overell, P., "Augmented BNF for Syntax Specifications: ABNF", RFC
4234, October 2005, https://www.rfc-editor.org/info/rfc4234

[RFC4918] Dusseault, L, Ed., "HTTP Extensions for Web Distributed Authoring and Versioning
(WebDAV)", RFC 4918, June 2007, https://www.rfc-editor.org/info/rfc4918

#### 1.2.2 Informative References

[MSASP] Microsoft Corporation, "Active Server Pages", http://msdn.microsoft.com/en-
us/library/aa286483.aspx

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

8 / 27


[MSDN-FP] Microsoft Corporation, "FrontPage", http://msdn.microsoft.com/en-
us/library/aa167865(office.11).aspx

[RFC2518] Goland, Y., Whitehead, E., Faizi, A., et al., "HTTP Extensions for Distributed Authoring -
WebDAV", RFC 2518, February 1999, http://www.ietf.org/rfc/rfc2518.txt

[RFC2818] Rescorla, E., "HTTP Over TLS", RFC 2818, May 2000, https://www.rfc-
editor.org/info/rfc2818

### 1.3 Overview

The Web Distributed Authoring and Versioning (WebDAV) Protocol described in [RFC2518] and in
[RFC4918] is a set of methods, headers, and content types that extend the HTTP 1.1 Protocol
described in [RFC2068] and in [RFC2616]. WebDAV allows data to be written to Internet servers and
is an Internet standard for collaborative authoring.

WebDAV expands the basic support in HTTP 1.1 for content authoring by introducing additional
methods and headers to provide support for resource properties and other base functions, such as
resource locking. A resource is a network data object or service that can be identified by a URI, as
described in HTTP 1.1 specifications' section 1.3. These new capabilities make the WebDAV Protocol
suitable for basic remotely mountable file systems.

These protocol server extensions specify the following extensions to the base WebDAV Protocol.

  An HTTP method that serves as the transport vehicle for search functionality in external

processes.

  An HTTP request header that indicates whether an entity is to be returned as-is, or whether any

associated programmatic processing is to be performed and the result returned.



The MS-Author-Via Response header, which specifies to the client application what the preferred
protocol mechanism is for authoring documents in a specific namespace. The namespace is the
entire collection of items under a request URI ([RFC4918] section 5.2). For example, a client
might have multiple authoring tools available to it, each possibly supported by a different network
authoring protocol.

  An extension to the existing Depth HTTP request header that indicates whether the root of a

resource is to be included in the results.

  Additional properties in the DAV namespace that describe additional file characteristics.

### 1.4 Relationship to Other Protocols

The WebDAV Protocol: Server Extensions rely on WebDAV which, in turn, relies on HTTP 1.1
described in [RFC2616] and in [RFC2068]. These extensions can use HTTPS over Transport Layer
Security (TLS) for data protection services, as described in [RFC2818].

### 1.5 Prerequisites/Preconditions

These protocol server extensions require a WebDAV server, as defined in [RFC2291], support the
WebDAV OPTIONS command.

These protocol server extensions also require that WebDAV clients have URLs that point to WebDAV
servers. The client has to obtain the URLs through some out-of-band mechanism.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

9 / 27


### 1.6 Applicability Statement

WebDAV Protocol: Server Extensions applies in scenarios that require efficient file operations. Note
that only those extensions specific to enabling efficient file system clients are specified. These
extensions do not add any functionality. They instead help reduce the network traffic and increase the
performance of clients that use the WebDAV Protocol.

### 1.7 Versioning and Capability Negotiation

Supported Transports: WebDAV Protocol: Server Extensions use HTTP as the only transport.

Versioning: No new versioning mechanisms have been introduced beyond those that already exist in
WebDAV and HTTP.

Capability Negotiation: Negotiation of WebDAV and HTTP capabilities is via the OPTIONS method.
These server extensions extends the OPTIONS method that uses an HTTP response header to indicate
which authoring tools are to be used. For more information about authoring, see section 2.2.2.

### 1.8 Vendor-Extensible Fields

These protocol server extensions can be extended by adding new tokens to the MS-Author-Via field
value, section 2.2.2.

These protocol server extensions use HTTP status codes [RFC2068] section 10 and [RFC4918] section
11.

### 1.9 Standards Assignments

No standards body has approved or governs these server extensions or its header names, values,
methods, and deviations. This specification conforms to the form and behavior of other custom HTTP
headers.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

10 / 27


## 2 Messages

The following section describes transport requirements and the syntax of the WebDAV Protocol: Server
Extensions.

### 2.1 Transport

Messages are transported using HTTP as specified in [RFC4918] and [RFC2068].

This protocol can be used with Secure Sockets Layer (SSL) or Transport Layer Security (TLS),
as specified in [RFC2246].

Port 80 is the standard port assignment for HTTP, and port 443 is the standard port assignment for
HTTP over SSL or TLS; however, individual implementations can support other ports.

### 2.2 Message Syntax

The extension headers in this protocol conform to the form and behavior of other custom HTTP
headers, as specified in [RFC2068] section 4.2, and are consistent with the WebDAV verbs and
headers as defined in [RFC4918] sections 9 and 10.

This section specifies the following header extensions:







The Translate request header (section 2.2.1) allows the WebDAV client to request the source of
an entity.

The header field MS-Author-Via is returned as a response-header field to a client that has issued
an OPTIONS command to the server. The syntax of the OPTIONS command is specified in
[RFC2616] section 9.2. The syntax of the MS-Author-Via header is specified using the
Augmented Backus-Naur Form (ABNF), as defined in [RFC2616] section 2.1.

The "noroot (section 2.2.3)" extension to the Depth request header extends the functionality that
is specified in [RFC4918] section 10.2, allowing a WebDAV client to request the children of a
requested entity, but not to include the requested entity itself.

This section specifies the following method:



The SEARCH method (section 2.2.4) is used to transport search-related commands to external
search providers.

This section specifies the following deviations from [RFC4918] in the WebDAV server
implementation:

  WebDAV property retrieval, as specified in [RFC4918] section 9.1, can include additional "live"

property settings, as specified in section 2.2.5.1.

  WebDAV locks, as specified in [RFC4918] sections 6 and 7, might not be supported on WebDAV

collections, as specified in section 2.2.5.2 and in [RFC4918] section 7.4.



The WebDAV property setting, as specified in [RFC4918] section 9.2, can include multiple un-
nested property settings, as specified in section 2.2.5.3.

#### 2.2.1 Translate Request Header

Many resources obtained from a WebDAV server are returned exactly as-is. However, some
resources are programmatically interpreted by the web server and the result of that interpretation is
returned instead of the source representation. For instance, a request to retrieve an Active Server
Page (ASP) (as described in [MSASP]) from the web server would return the processed HTML file

11 / 27

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024


rather than the actual source of the ASP page. A resource is an entity that can be identified by a URI.
A resource is used as specified in [RFC2616] section 1.3.

In order for a WebDAV client to indicate the required representation, WebDAV Protocol: Server
Extensions introduces a new Translate request header. This header requests that the web server
perform "translation" (programmatic interpretation) of the file.

This new request header is defined as follows, using the ABNF syntax, as specified in [RFC2068]
section 2.1.

 Translate-header = "Translate" ":" Translate-value
 Translate-value = "t" / "f" / "F"

If the Translate value is "t", the web server is to process the content before returning it to the
WebDAV client; if "f" | "F", the web server is to return the unprocessed (or source) content to the
WebDAV client. The values "f" and "F" are synonymous. All other values SHOULD be ignored by the
web server. For more details about processing of this request header, see section 3.1.5.<1>

This request header MUST be supported on the GET verb and MAY be supported on other verbs.<2>

#### 2.2.2 MS-Author-Via Response Header

This extension MUST have the following format:

 MS-Author-Via = "MS-Author-Via" ": " ("MS-FP/4.0" / "MS-FP/4.0,DAV" / "DAV" / "DAV,MS-FP/4.0"
/ token ",DAV")

This header field indicates to the issuer of an HTTP OPTIONS command what protocol mechanism is
preferred for authoring documents in this particular namespace. The preference MUST be ordered so
the first mechanism listed is the one most preferred by the server.

Note  The "token" definition in the preceding grammar is specified in [RFC2616] section 2.2. Its
purpose in the grammar is to allow for future extensibility. The WebDAV Server Extensions protocol
MAY be extended by adding new tokens to the MS-Author-Via field. Clients MUST ignore tokens in this
field that they do not recognize.

The following table lists the values that are currently defined for this header.

Value

Meaning

MS-FP/4.0

The server's preferred method for authoring is the FrontPage Server Extensions.

MS-
FP/4.0,DAV

The server's protocol preference for authoring is first the FrontPage Server Extensions (for more
information, see [MSDN-FP]), and then WebDAV.

DAV

The server's preferred method for authoring is WebDAV.

DAV,MS-
FP/4.0

The server's protocol preference for authoring is first WebDAV, and then the FrontPage Server
Extensions (for more information, see [MSDN-FP]).

Token,DAV

The server's protocol preference for authoring is an extensible token, and then WebDAV.

  MS-FP/4.0 refers to Microsoft FrontPage Server protocol version 4.0 enabled on the web server.
Some applications look for this string and use that protocol to communicate with this web server.

  DAV is used to indicate that WebDAV is enabled on the WebDAV server, and a WebDAV client

MAY use WebDAV commands to communicate with this WebDAV server.

12 / 27

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024


#### 2.2.3 Depth "noroot" Request Header Extension

 This header extension is defined as follows, using the ABNF syntax defined in [RFC4234] section 2.1:

 Depth = "Depth" ": " ("0" / "1" / "infinity" / "1,noroot" / "infinity,noroot")

The following values are associated with the Depth header.

 Value

 Meaning

0

1

The command applies only to the specified resource.

The command applies to the specified resource and the next level of resources that it contains.

infinity

The command applies to the specified resource and all the resources that it contains.

1,noroot

The command applies to the next level of resources in the container but not to the container
itself.

infinity,noroot  The command applies to all the resources in the container but not to the container itself.

The noroot extension is present if the "1,noroot" value is set or if the "infinity,noroot" value is set.<3>

#### 2.2.4 SEARCH Method

This method extension allows a WebDAV client to request search results from an external search
provider. A WebDAV server SHOULD advertise support for the SEARCH method in the response to an
OPTIONS request. The SEARCH method is simply a delivery mechanism for passing queries to external
search providers.<4>

The following example shows the syntax for the SEARCH method, which conforms to the HTTP syntax
as defined in [RFC2068].

 SEARCH /[resource] HTTP/1.1
 Host: www.example.com
 Content-type: text/xml; charset="utf-8"
 Content-Length: nnn
 <?xml version="1.0" encoding="utf-8" ?>
 <D:searchrequest xmlns:D = "DAV:">
   <D:sql>
     [implementation-specific content for a search provider]
   </D:sql>
 </D:searchrequest>

#### 2.2.5 Deviations from RFC4918

##### 2.2.5.1 Additional "Live" Properties

[RFC4918] sections 4 and 15 define the "live" properties that a WebDAV server SHOULD
implement.<5>

##### 2.2.5.2 Collections and Locking

[RFC4918] sections 7 and 9 define how WebDAV servers SHOULD implement locking for
collections.<6>

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

13 / 27


##### 2.2.5.3 Multiple Property Elements

[RFC4918] section 9.2 defines the message syntax for setting properties by using the PROPPATCH
method. A property or group of properties for a resource MAY be enclosed within a single
<d:prop></d:prop> element,<7> as shown in the following example.

 PROPPATCH /[resource] HTTP/1.1
 Host: www.example.com
 Content-type: text/xml; charset="utf-8"
 Content-Length: nnn
 <?xml version="1.0" encoding="utf-8" ?>
 <d:propertyupdate xmlns:d='DAV:'>
   <d:set>
     <d:prop>
       <d:owner>Owner's Name</d:owner>
     </d:prop>
 </d:set>
 </d:propertyupdate>

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

14 / 27


## 3 Protocol Details

As specified in [RFC4918], WebDAV operates between an initiator (a WebDAV client) and a
responder (a WebDAV server). This section specifies the client and the server behaviors with respect
to the WebDAV extensions.

### 3.1 WebDAV Client Details

#### 3.1.1 Abstract Data Model

A WebDAV client SHOULD maintain a variable for each server with which it communicates that
contains the value of the MS-Author-Via response header.

#### 3.1.2 Timers

No new timers are required beyond those specified in [RFC2068].

#### 3.1.3 Initialization

Prior to general interaction with a WebDAV server, the WebDAV client SHOULD perform an
OPTIONS request to determine the current authoring implementation using the MS-Author-Via
response header.<8>

#### 3.1.4 Higher-Layer Triggered Events

No new events are triggered beyond those specified in [RFC2068].

#### 3.1.5 Processing Events and Sequencing Rules

A WebDAV client SHOULD send an OPTIONS request to the server and query the response headers
for the presence of the WebDAV server's supported features. When sending an OPTIONS request,
the WebDAV client SHOULD send the OPTIONS request before sending any other requests to the
WebDAV server. The information returned SHOULD be used by the WebDAV client to specialize
requests to this server.

##### 3.1.5.1 Translate Request Header

The WebDAV client SHOULD add the Translate request header with an "f" or "F" flag to a request if
the WebDAV client needs the source of a file without any translation; otherwise this header SHOULD
be omitted.

##### 3.1.5.2 MS-Author-Via Response Header

A WebDAV client MAY send an OPTIONS request to the server and query the response headers for
the presence of the MS-Author-Via header. The WebDAV client SHOULD use the following information
to determine the best method to send requests to the server:





The WebDAV client SHOULD NOT use WebDAV commands when communicating with a server
that responds to the OPTIONS command with only the MS-Author-Via: MS-FP/4.0 response
header.

The WebDAV client MAY use WebDAV commands when communicating with a server that responds
to the OPTIONS command with any of the following MS-Author-Via response header values:

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

15 / 27


  MS-Author-Via: DAV

  MS-Author-Via: DAV, MS-FP/4.0

  MS-Author-Via: MS-FP/4.0, DAV

  MS-Author-Via: MS-FP

  MS-Author-Via: Token,DAV

##### 3.1.5.3 Depth "noroot" Request Header Extension

The WebDAV client SHOULD add the Depth request header with a value of '1,noroot' or
'infinity,noroot' to a request if the WebDAV client needs the resources that are in the container, but
not in the container itself; otherwise this header SHOULD be omitted.<9> A resource is an entity that
can be identified by a URI. A resource is used as specified in [RFC2616] section 1.3.

##### 3.1.5.4 SEARCH Method

A WebDAV client SHOULD send a SEARCH request to a WebDAV server that advertises support for
the SEARCH method through the response to an OPTIONS request from the WebDAV client. If the
WebDAV server does not advertise support for the SEARCH method, the WebDAV client SHOULD NOT
send a SEARCH request.

#### 3.1.6 Timer Events

No new timers are required beyond those in the base protocol.

#### 3.1.7 Other Local Events

There are no new local events other than those described in the base protocol.

### 3.2 WebDAV Server Details

#### 3.2.1 Abstract Data Model

No new abstract data model is needed other than that described in the base protocol.

#### 3.2.2 Timers

No new timers are required beyond those in the base protocol.

#### 3.2.3 Initialization

No initialization is required beyond that in the base protocol.

#### 3.2.4 Higher-Layer Triggered Events

No new events are triggered beyond that in the base protocol.

#### 3.2.5 Processing Events and Sequencing Rules

A WebDAV server can advertise support for WebDAV authoring by adding the MS-Author-Via: DAV
response header to the response to the OPTIONS command. If the server does not support WebDAV

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

16 / 27


authoring, the OPTIONS response SHOULD omit the MS-Author-Via: DAV response header. A WebDAV
server MAY add the MS-Author-Via: DAV header to other responses.

##### 3.2.5.1 Translate Request Header

The WebDAV server MUST respond with the actual source of the file if the translate request header
exists with a flag value starting with "f" or "F". The WebDAV server SHOULD perform access checks
when processing a command with a Translate request header.<10>

To maintain consistency with web browsers, the default behavior if this request header is omitted is to
translate the file; omission of this header SHOULD be the same as sending Translate: t.<11>

##### 3.2.5.2 MS-Author-Via Response Header

The WebDAV server SHOULD respond with the MS-Author-Via response header when a WebDAV
client sends an OPTIONS request.<12> The WebDAV server responds with the appropriate
information when the following conditions are true:











"MS-Author-Via: DAV" The WebDAV server responds with this header when WebDAV is enabled
and the FrontPage Server Extensions protocol is not enabled.

"MS-Author-Via: MS-FP/4.0" The web server responds with this header when the FrontPage
Server Extensions protocol is enabled and WebDAV is not enabled.

"MS-Author-Via: DAV,MS-FP/4.0" The WebDAV server responds with this header when both
WebDAV and the FrontPage Server Extensions protocol are enabled, and WebDAV is preferred
over the FrontPage Server Extensions protocol.

"MS-Author-Via: MS-FP/4.0,DAV" The WebDAV server responds with this header when both the
FrontPage Server Extensions protocol and WebDAV are enabled, and the FrontPage Server
Extensions protocol is preferred over WebDAV.

"MS-Author-Via: Token,DAV" The WebDAV server responds with this header when both the
WebDAV and an extensible token are enabled.

##### 3.2.5.3 Depth "noroot" Request Header Extension

If the WebDAV server supports the Depth "noroot" request header extension, the WebDAV server
SHOULD respond appropriately depending on the WebDAV command that the WebDAV client is
sending.<13>

##### 3.2.5.4 SEARCH Method

A WebDAV server advertises support for the SEARCH method through the response to an OPTIONS
request from the WebDAV client. If a WebDAV client sends a SEARCH request, the WebDAV server
SHOULD send the request to the appropriate search provider on the server.<14>

#### 3.2.6 Timer Events

No new timers are required beyond those in the base protocol.

#### 3.2.7 Other Local Events

There are no new local events other than those described in the base protocol.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

17 / 27


## 4 Protocol Examples

This section provides examples of the protocol extensions.

### 4.1 MS-Author-Via Response Header

The following example shows an OPTIONS request from a WebDAV client and the response from the
WebDAV server that contains the MS-Author-Via response header.

Request:

 OPTIONS / HTTP/1.1
 Host: localhost
 Accept: */*

Response:

 HTTP/1.1 200 OK
 Server: Microsoft-IIS/6.0
 Date: Wed, 28 Jun 2006 00:06:21 GMT
 MS-Author-Via: DAV
 Allow: OPTIONS, TRACE, GET, HEAD, POST, LOCK, UNLOCK, MKCOL,
        PROPFIND, PROPPATCH, COPY, MOVE
 Public: OPTIONS, TRACE, GET, HEAD, POST, LOCK, UNLOCK, MKCOL,
         PROPFIND, PROPPATCH, COPY, MOVE
 MS-Author-Via: DAV
 Content-Length: 0

### 4.2 Translate Request Header

The following examples show the difference between requesting an entity and the source of an entity.
The first example is a typical HTTP GET command as issued by a browser such as Internet Explorer.

Request:

 GET /Temp/world.asp HTTP/1.1
 Translate: t
 Host: localhost
 Accept: */*

Response:

 HTTP/1.1 200 OK
 Server: Microsoft-IIS/5.1
 Date: Wed, 28 Jun 2006 00:06:21 GMT
 Content-Length: 129
 Content-Type: text/html
 Set-Cookie: ASPSESSIONIDCSSTSCQB=IEEJDPNAAIJECIOOBLMMGDJM; path=/
 Cache-control: private

 <FONT SIZE="1">Hello World</FONT><BR>

 <FONT SIZE="2">Hello World</FONT><BR>

 <FONT SIZE="3">Hello World</FONT><BR>

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

18 / 27


An authoring application might want to retrieve the source of an entity, and it would issue the same
request asking for the source of the entity as follows:

Request:

 GET /Temp/world.asp HTTP/1.1
 Translate: f
 Host: localhost
 Accept: */*

Response:

 HTTP/1.1 200 OK
 Server: Microsoft-IIS/5.1
 Date: Wed, 28 Jun 2006 00:16:34 GMT
 Content-Type: text/plain
 Content-Length: 497
 ETag: "22a87614489ac61:c02"
 Last-Modified: Wed, 28 Jun 2006 00:16:19 GMT
 Accept-Ranges: bytes
 <%
 '***********************************************
 '*     Sample ASP Code                         *
 '*                                             *
 '***********************************************

 ' Declare  loop variable.
 Dim I

 ' Loop 3 times, adjusting the
 ' font size in each loop.
 For I = 1 To 3 Step 1
   ' Output our HTML and text using the value
   ' of I as the FONT TAG's SIZE attribute.
 %>
 <FONT SIZE="<%=I%>">Hello World</FONT><BR>
 <%
 Next ' continue looping
 %>

The difference between these two requests is that the second one is requesting the source of an
entity. This is a typical example of how the Translate request header is used.

### 4.3 Depth "noroot" Request Header Extension

The following example shows a PROPFIND request from a WebDAV client and the response from the
WebDAV server that contains the Depth "noroot" Request Header Extension.

PROPFIND request with Depth: 1

 PROPFIND /dav/ HTTP/1.1
 Depth: 1
 Content-Type: text/xml
 Host: localhost
 Accept: */*
 Content-Length:104

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

19 / 27


 <?xml version="1.0"?>
 <D:propfind xmlns:D="DAV:">
   <D:prop>
     <D:displayname/>
   </D:prop>
 </D:propfind>

Response:

 HTTP/1.1 207 Multi-Status
 Date: Fri, 17 Aug 2007 20:24:20 GMT
 Server: Microsoft-IIS/6.0
 X-Powered-By: ASP.NET
 Content-Type: text/xml
 Transfer-Encoding: chunked

 <?xml version="1.0"?>
 <a:multistatus xmlns:b="urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882/"
 xmlns:c="xml:" xmlns:a="DAV:">
   <a:response>
     <a:href>http://localhost/dav/</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>dav</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
   <a:response>
     <a:href>http://localhost/dav/pagerror.gif</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>pagerror.gif</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
   <a:response>
     <a:href>http://localhost/dav/script.asp</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>script.asp</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
   <a:response>
     <a:href>http://localhost/dav/textfile.txt</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>textfile.txt</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
 </a:multistatus>

PROPFIND request with Depth: 1,noroot

 PROPFIND /dav/ HTTP/1.1
 Depth: 1,noroot
 Content-Type: text/xml
 Host: localhost
 Accept: */*\r\n

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

20 / 27


 Content-Length:104

 <?xml version="1.0"?>
 <D:propfind xmlns:D="DAV:">
   <D:prop>
     <D:displayname/>
   </D:prop>
 </D:propfind>

Response:

 HTTP/1.1 207 Multi-Status
 Date: Fri, 17 Aug 2007 20:28:15 GMT
 Server: Microsoft-IIS/6.0
 X-Powered-By: ASP.NET
 Content-Type: text/xml
 Transfer-Encoding: chunked

 <?xml version="1.0"?>
 <a:multistatus xmlns:b="urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882/"
 xmlns:c="xml:" xmlns:a="DAV:">
   <a:response>
     <a:href>http://localhost/dav/pagerror.gif</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>pagerror.gif</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
   <a:response>
     <a:href>http://localhost/dav/script.asp</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>script.asp</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
   <a:response>
     <a:href>http://localhost/dav/textfile.txt</a:href>
     <a:propstat>
       <a:status>HTTP/1.1 200 OK</a:status>
       <a:prop>
         <a:displayname>textfile.txt</a:displayname>
       </a:prop>
     </a:propstat>
   </a:response>
 </a:multistatus>

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

21 / 27


## 5 Security

### 5.1 Security Considerations for Implementers

WebDAV servers that support the Translate request header can perform access checks before
returning the source of the file, as specified in section 3.2.5.1, in order to protect any source content
(for example, database passwords).<15>

### 5.2 Index of Security Parameters

No new security parameters are required beyond those in the base protocol.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

22 / 27


## 6 Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

The terms "earlier" and "later", when used with a product version, refer to either all preceding
versions or all subsequent versions, respectively. The term "through" refers to the inclusive range of
versions. Applicable Microsoft products are listed chronologically in this section.

Windows Client

  Windows 2000 Professional operating system

  Windows XP operating system

  Windows Vista operating system

  Windows 7 operating system

  Windows 8 operating system

  Windows 8.1

  Windows 10 operating system

  Windows 11 operating system

Windows Server

  Windows 2000 Server operating system

  Windows Server 2003 operating system

  Windows Server 2003 R2 operating system

  Windows Server 2008 operating system

  Windows Server 2008 R2 operating system

  Microsoft SharePoint Foundation 2010

  Windows Server 2012 operating system

  Windows Server 2012 R2

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

23 / 27

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024


SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 2.2.1: The web server in IIS for Windows 2000 operating system, Windows XP, and
Windows based servers Windows Server 2003 and later can accept the Translate request header. This
implementation accepts "f" and "F" as FALSE. Everything else, including omission of the header, is
accepted as TRUE.

<2> Section 2.2.1:  The web server in IIS for Windows 2000, Windows XP, and Windows based
servers Windows Server 2003 and later can accept this header on all commands, while only honoring
this header on GET commands.

<3> Section 2.2.3: This request header can be supported by the WebDAV server in IIS for Windows
2000, Windows XP, and Windows Server 2003.

<4> Section 2.2.4: This method can be supported by the WebDAV server in IIS for Windows 2000,
Windows XP, and Windows Server 2003, and is used to pass queries to Windows Index Server.

<5> Section 2.2.5.1: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows based
servers Windows Server 2003 and later return the additional iscollection and ishidden properties as
"live" properties.

<6> Section 2.2.5.2: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows Server
2003 does not support locking collections.

<7> Section 2.2.5.3: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows based
servers Windows Server 2003 and later allows the use of multiple <d:prop></d:prop> elements to
encapsulate multiple properties.

<8> Section 3.1.3: The WebDAV server in Windows 2000, Windows XP, and Windows based servers
Windows Server 2003 and later can return the MS-Author-Via: DAV response header if WebDAV is
enabled on the server.

<9> Section 3.1.5.3: The Depth header field is used with several WebDAV commands. However,
Windows implementations only support the "noroot" extension on two verbs: DELETE and
PROPFIND. "1,noroot" is supported for PROPFIND and "infinity,noroot" is supported on DELETE.
Attempting to specify "noroot" on other verbs that support the Depth header field will result in a "400
Bad Request" error being returned.

<10> Section 3.2.5.1: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows
Server 2003 requires WRITE access to the file to return the source of the file.

<11> Section 3.2.5.1: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows based
servers Windows Server 2003 and later assumes the header as Translate: t by default.

<12> Section 3.2.5.2: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows based
servers Windows Server 2003 and later can return the MS-Author-Via response header in response to
an OPTIONS request.

<13> Section 3.2.5.3: The WebDAV server in IIS for Windows 2000, Windows XP, and Windows
Server 2003 supports the Depth "noroot" request header extension.

<14> Section 3.2.5.4:  The WebDAV server in IIS for Windows 2000, Windows XP, and Windows
Server 2003 supports the SEARCH method.

<15> Section 5.1:  For a Translate header value of FALSE, the WebDAV server in IIS for Windows
2000, Windows XP, and Windows Server 2003 requires WRITE access to return the source of the file.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

24 / 27


## 7 Change Tracking

No table of changes is available. The document is either new or has had no changes since its last
release.

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

25 / 27


## 8 Index
"

"Live" properties 13

[

[RFC4918] 13

A

Abstract data model
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16
Additional "Live" properties 13
Applicability 10

C

Capability negotiation 10
Change tracking 25
Client
   abstract data model 15
   higher-layer triggered events 15
   initialization 15
   other local events 16
   timer events 16
   timers 15
Collections 13

D

Data model - abstract
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16
Depth "noroot" Request header extension
   overview 13
   WebDAV client details 16
   WebDAV server details 17
Depth "noroot" Request Header Extension example

19

Depth "noroot" Request Header Extension message

13

Deviations from [RFC4918] 13

E

Examples
   Depth "noroot" Request Header Extension 19
   MS-Author-Via Response Header 18
   overview 18
   Translate Request Header 18

F

Fields - vendor-extensible 10

G

Glossary 7

H

Higher-layer triggered events
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16

I

Implementer - security considerations 22
Index of security parameters 22
Informative references 8
Initialization
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16
Introduction 7

L

Local events
   WebDAV client details 16
   WebDAV server details 17
Locking 13

M

Message processing
   WebDAV client details 15
   WebDAV server details 16
Messages
   Depth "noroot" Request Header Extension 13
   MS-Author-Via Response Header 12
   overview 11
   SEARCH Method 13
   syntax 11
   Translate Request Header 11
   transport 11
MS-Author-Via Response header
   overview 12
   WebDAV client details 15
   WebDAV server details 17
MS-Author-Via Response Header example 18
MS-Author-Via Response Header message 12
Multiple Property elements 14

N

Normative references 8

O

Other local events
   client 16
   server 17
Overview 9
Overview (synopsis) 9

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

26 / 27


P

V

Vendor-extensible fields 10
Versioning 10

W

WebDAV client details
   abstract data model 15
   Depth "noroot" Request header extension 16
   higher-layer triggered events 15
   initialization 15
   local events 16
   message processing 15
   MS-Author-Via Response header 15
   SEARCH method 16
   sequencing rules 15
   timer events 16
   timers 15
   Translate Request header 15
WebDAV server details
   abstract data model 16
   Depth "noroot" Request header extension 17
   higher-layer triggered events 16
   initialization 16
   local events 17
   message processing 16
   MS-Author-Via Response header 17
   SEARCH method 17
   sequencing rules 16
   timer events 17
   timers 16
   Translate Request header 17

Parameters - security index 22
Preconditions 9
Prerequisites 9
Product behavior 23
Protocol Details
   overview 15

R

References 8
   informative 8
   normative 8
Relationship to other protocols 9

S

SEARCH method
   overview 13
   WebDAV client details 16
   WebDAV server details 17
SEARCH Method message 13
Security
   implementer considerations 22
   parameter index 22
Sequencing rules
   WebDAV client details 15
   WebDAV server details 16
Server
   abstract data model 16
   higher-layer triggered events 16
   initialization 16
   other local events 17
   timer events 17
   timers 16
Standards assignments 10
Syntax 11

T

Timer events
   client 16
   server 17
   WebDAV client details 16
   WebDAV server details 17
Timers
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16
Tracking changes 25
Translate Request header
   example 18
   overview 11
   WebDAV client details 15
   WebDAV server details 17
Translate Request Header message 11
Transport 11
Triggered events - higher-layer
   client 15
   server 16
   WebDAV client details 15
   WebDAV server details 16

[MS-WDVSE] - v20240916
Web Distributed Authoring and Versioning (WebDAV) Protocol: Server Extensions
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

27 / 27

