[MS-WPRN]:

Web Point-and-Print Protocol

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

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

1 / 31

Revision Summary

Date

Revision
History

Revision
Class

Comments

3/2/2007

1.0

4/3/2007

1.1

5/11/2007

1.2

New

Minor

Minor

Version 1.0 release

Version 1.1 release

Version 1.2 release

6/1/2007

1.2.1

Editorial

Changed language and formatting in the technical content.

7/3/2007

1.2.2

Editorial

Changed language and formatting in the technical content.

8/10/2007

1.2.3

Editorial

Changed language and formatting in the technical content.

9/28/2007

1.2.4

Editorial

Changed language and formatting in the technical content.

10/23/2007  2.0

Major

Converted document to unified format.

1/25/2008

2.0.1

Editorial

Changed language and formatting in the technical content.

3/14/2008

2.1

6/20/2008

2.2

7/25/2008

3.0

8/29/2008

3.1

Minor

Minor

Major

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

10/24/2008  3.1.1

Editorial

Changed language and formatting in the technical content.

12/5/2008

3.2

1/16/2009

3.3

Minor

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

2/27/2009

3.3.1

Editorial

Changed language and formatting in the technical content.

4/10/2009

3.3.2

Editorial

Changed language and formatting in the technical content.

5/22/2009

3.4

Minor

Clarified the meaning of the technical content.

7/2/2009

3.4.1

Editorial

Changed language and formatting in the technical content.

8/14/2009

3.5

9/25/2009

3.6

11/6/2009

3.7

12/18/2009  3.8

Minor

Minor

Minor

Minor

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

1/29/2010

3.8.1

Editorial

Changed language and formatting in the technical content.

3/12/2010

3.8.2

Editorial

Changed language and formatting in the technical content.

4/23/2010

3.8.3

Editorial

Changed language and formatting in the technical content.

6/4/2010

3.8.4

Editorial

Changed language and formatting in the technical content.

7/16/2010

3.9

Minor

Clarified the meaning of the technical content.

8/27/2010

3.9

None

No changes to the meaning, language, or formatting of the
technical content.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

2 / 31

Date

Revision
History

Revision
Class

Comments

10/8/2010

3.9

None

No changes to the meaning, language, or formatting of the
technical content.

11/19/2010  3.10

Minor

Clarified the meaning of the technical content.

1/7/2011

3.10

None

No changes to the meaning, language, or formatting of the
technical content.

2/11/2011

3.10

None

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

3.10

None

No changes to the meaning, language, or formatting of the
technical content.

5/6/2011

3.10

None

No changes to the meaning, language, or formatting of the
technical content.

6/17/2011

3.11

Minor

Clarified the meaning of the technical content.

9/23/2011

3.11

None

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  4.0

Major

Updated and revised the technical content.

3/30/2012

4.0

7/12/2012

4.0

10/25/2012  4.0

1/31/2013

4.0

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

5.0

Major

Updated and revised the technical content.

11/14/2013  5.0

2/13/2014

5.0

5/15/2014

5.0

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

6.0

Major

Significantly changed the technical content.

10/16/2015  6.0

7/14/2016

6.0

6/1/2017

6.0

9/15/2017

7.0

9/12/2018

8.0

None

None

None

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

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

3 / 31

Date

Revision
History

Revision
Class

Comments

4/7/2021

9.0

6/25/2021

10.0

4/23/2024

11.0

9/16/2024

11.0

Major

Major

Major

None

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

4 / 31

Table of Contents

1.1
1.2

1.2.1
1.2.2

1  Introduction ............................................................................................................ 7
Glossary ........................................................................................................... 7
References ........................................................................................................ 8
Normative References ................................................................................... 8
Informative References ................................................................................. 9
Overview .......................................................................................................... 9
Relationship to Other Protocols .......................................................................... 10
Prerequisites/Preconditions ............................................................................... 10
Applicability Statement ..................................................................................... 10
Versioning and Capability Negotiation ................................................................. 10
Vendor-Extensible Fields ................................................................................... 10
Standards Assignments ..................................................................................... 10

1.3
1.4
1.5
1.6
1.7
1.8
1.9

2.1
2.2

2.2.1
2.2.2
2.2.3
2.2.4
2.2.5
2.2.6
2.2.7

2  Messages ............................................................................................................... 12
Transport ........................................................................................................ 12
Message Syntax ............................................................................................... 12
Common Data Types ................................................................................... 12
ClientInfo Value .......................................................................................... 12
Registry Type Values ................................................................................... 13
Driver Selection Request ............................................................................. 14
Driver Selection Response ........................................................................... 14
Driver Download Request ............................................................................ 14
Driver Download Response .......................................................................... 15
BIN File Format .................................................................................... 15
UserDevMode Structure.................................................................... 15
PrnDataRoot Structure ..................................................................... 16
DAT File Format .................................................................................... 18

2.2.7.1.1
2.2.7.1.2

2.2.7.1

2.2.7.2

3.1

3.1.1
3.1.2
3.1.3
3.1.4
3.1.5
3.1.6
3.1.7

3  Protocol Details ..................................................................................................... 20
Client Details ................................................................................................... 20
Abstract Data Model .................................................................................... 20
Timers ...................................................................................................... 20
Initialization ............................................................................................... 20
Higher-Layer Triggered Events ..................................................................... 20
Message Processing Events and Sequencing Rules .......................................... 20
Timer Events .............................................................................................. 20
Other Local Events ...................................................................................... 20
Server Details .................................................................................................. 21
Abstract Data Model .................................................................................... 21
Timers ...................................................................................................... 21
Initialization ............................................................................................... 21
Higher-Layer Triggered Events ..................................................................... 21
Message Processing Events and Sequencing Rules .......................................... 21
Timer Events .............................................................................................. 22
Other Local Events ...................................................................................... 22

3.2.1
3.2.2
3.2.3
3.2.4
3.2.5
3.2.6
3.2.7

3.2

4.1
4.2

4  Protocol Examples ................................................................................................. 23
Client/Server Interaction ................................................................................... 23
Client/Server Messages ..................................................................................... 23
Client ........................................................................................................ 23
Server ....................................................................................................... 23

4.2.1
4.2.2

5  Security Considerations ......................................................................................... 25

6  Appendix A: Product Behavior ............................................................................... 26

7  Change Tracking .................................................................................................... 29

5 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

8  Index ..................................................................................................................... 30

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

6 / 31

1  Introduction

The Web Point-and-Print Protocol provides a way for a client to download printer driver software,
from a server in the client network, from a website, or directly by a print device. This supports the
distribution of printer drivers using standard web technologies.

This protocol is based on the Hypertext Transfer Protocol (HTTP) [RFC2616] and references
commonly used data types defined in [MS-DTYP].

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

ASCII: The American Standard Code for Information Interchange (ASCII) is an 8-bit character-
encoding scheme based on the English alphabet. ASCII codes represent text in computers,
communications equipment, and other devices that work with text. ASCII refers to a single 8-bit
ASCII character or an array of 8-bit ASCII characters with the high bit of each character set to
zero.

big-endian: Multiple-byte values that are byte-ordered with the most significant byte stored in the

memory location with the lowest address.

CAB file: See cabinet file.

cabinet file: A file that has the suffix .cab and that acts as a container for other files. It serves as
a compressed archive for a group of files. For more information, including the format of CAB
files, see [MSDN-CAB].

device driver: The software that the system uses to communicate with a device such as a display,

printer, mouse, or communications adapter. An abstraction layer that restricts access of
applications to various hardware devices on a given computer system. It is often referred to
simply as a "driver".

driver package: A collection of the files needed to successfully load a driver. This includes the

device information (.inf) file, the catalog file, and all of the binaries that are copied by the .inf
file.  Multiple drivers packaged together for deployment purposes.

fully qualified domain name (FQDN): An unambiguous domain name that gives an absolute

location in the Domain Name System's (DNS) hierarchy tree, as defined in [RFC1035] section
3.1 and [RFC2181] section 11.

Hypertext Transfer Protocol (HTTP): An application-level protocol for distributed, collaborative,
hypermedia information systems (text, graphic images, sound, video, and other multimedia
files) on the World Wide Web.

Hypertext Transfer Protocol Secure (HTTPS): An extension of HTTP that securely encrypts and

decrypts web page requests. In some older protocols, "Hypertext Transfer Protocol over Secure
Sockets Layer" is still used (Secure Sockets Layer has been deprecated). For more information,
see [SSL3] and [RFC5246].

INF file: A file that provides setup information required to set up a device, such as a list of valid
logical configurations for the device and the names of driver files associated with the device.

Internet Printing Protocol (IPP): A standard protocol for printing and for the management of

print jobs and printer settings over the Internet. It is built on the Hypertext Transfer Protocol
(HTTP).

7 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

Internet Protocol version 4 (IPv4): An Internet protocol that has 32-bit source and destination

addresses. IPv4 is the predecessor of IPv6.

little-endian: Multiple-byte values that are byte-ordered with the least significant byte stored in

the memory location with the lowest address.

NetBIOS: A particular network transport that is part of the LAN Manager protocol suite. NetBIOS

uses a broadcast communication style that was applicable to early segmented local area
networks. A protocol family including name resolution, datagram, and connection services. For
more information, see [RFC1001] and [RFC1002].

print server: A machine that hosts the print system and all its different components.

printer driver: The interface component between the operating system and the printer device. It
is responsible for processing the application data into a page description language (PDL) that
can be interpreted by the printer device.

proxy: A network node that accepts network traffic originating from one network agent and

transmits it to another network agent.

registry: A local system-defined database in which applications and system components store and
retrieve configuration data. It is a hierarchical data store with lightly typed elements that are
logically stored in tree format. Applications use the registry API to retrieve, modify, or delete
registry data. The data stored in the registry varies according to the version of the operating
system.

Unicode: A character encoding standard developed by the Unicode Consortium that represents

almost all of the written languages of the world. The Unicode standard [UNICODE5.0.0/2007]
provides three forms (UTF-8, UTF-16, and UTF-32) and seven schemes (UTF-8, UTF-16, UTF-16
BE, UTF-16 LE, UTF-32, UTF-32 LE, and UTF-32 BE).

Uniform Resource Locator (URL): A string of characters in a standardized format that identifies

a document or resource on the World Wide Web. The format is as specified in [RFC1738].

Universal Naming Convention (UNC): A string format that specifies the location of a resource.

For more information, see [MS-DTYP] section 2.2.57.

UTF-16LE: The Unicode Transformation Format - 16-bit, Little Endian encoding scheme. It is used
to encode Unicode characters as a sequence of 16-bit codes, each encoded as two 8-bit bytes
with the least-significant byte first.

web server: A server computer that hosts websites and responds to requests from applications.

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

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

8 / 31

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[MS-RPRN] Microsoft Corporation, "Print System Remote Protocol".

[MS-RRP] Microsoft Corporation, "Windows Remote Registry Protocol".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2396] Berners-Lee, T., Fielding, R., and Masinter, L., "Uniform Resource Identifiers (URI):
Generic Syntax", RFC 2396, August 1998, https://www.rfc-editor.org/info/rfc2396

[RFC2616] Fielding, R., Gettys, J., Mogul, J., et al., "Hypertext Transfer Protocol -- HTTP/1.1", RFC
2616, June 1999, https://www.rfc-editor.org/info/rfc2616

[RFC2617] Franks, J., Hallam-Baker, P., Hostetler, J., et al., "HTTP Authentication: Basic and Digest
Access Authentication", RFC 2617, June 1999, https://www.rfc-editor.org/info/rfc2617

[RFC2818] Rescorla, E., "HTTP Over TLS", RFC 2818, May 2000, https://www.rfc-
editor.org/info/rfc2818

[RFC793] Postel, J., Ed., "Transmission Control Protocol: DARPA Internet Program Protocol
Specification", RFC 793, September 1981, https://www.rfc-editor.org/info/rfc793

1.2.2  Informative References

[MSDN-CAB] Microsoft Corporation, "Microsoft Cabinet Format", March 1997,
http://msdn.microsoft.com/en-us/library/bb417343.aspx

[MSDN-INF] Microsoft Corporation, "About INF Files", http://msdn.microsoft.com/en-
us/library/aa376858.aspx

[MSDN-PRNINF] Microsoft Corporation, "Printer INF Files", https://msdn.microsoft.com/en-
us/windows/hardware/drivers/print/printer-inf-files

[MSDN-RTV] Microsoft Corporation, "Registry Value Types", http://msdn.microsoft.com/en-
us/library/ms724884.aspx

[RFC2781] Hoffman, P., and Yergeau, F., "UTF-16, an encoding of ISO 10646", RFC 2781, February
2000, https://www.rfc-editor.org/info/rfc2781

[RFC2910] Herriot, R., Ed., Butler, S., and Moore, P., "Internet Printing Protocol/1.1: Encoding and
Transport", RFC 2910, September 2000, http://www.ietf.org/rfc/rfc2910.txt

[RFC2911] Hastings, T., Ed., Herriot, R., deBry, R., et al., "Internet Printing Protocol/1.1: Model and
Semantics", RFC 2911, September 2000, http://www.ietf.org/rfc/rfc2911.txt

[RFC5234] Crocker, D., Ed., and Overell, P., "Augmented BNF for Syntax Specifications: ABNF", STD
68, RFC 5234, January 2008, https://www.rfc-editor.org/info/rfc5234

1.3  Overview

The Web Point-and-Print Protocol provides a mechanism for clients to download printer driver
software from a server in the client network or from a website, or directly by print devices. <1>

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

9 / 31

Figure 1: Client selection and download of printer driver

A Web Point-and-Print Protocol web server maintains a list of printer drivers. A client makes a Driver
Selection Request (section 2.2.4) to obtain a printer driver of a particular type and for a particular
client configuration. If the server locates a printer driver that matches these requirements, the server
redirects the client to the location of the printer driver through the Driver Selection Response (section
2.2.5).

After a suitable printer driver has been found, the client downloads it by issuing a Driver Download
Request (section 2.2.6). The driver is supplied in a Driver Download Response (section 2.2.7).

1.4  Relationship to Other Protocols

The Web Point-and-Print Protocol is dependent on HTTP specifications [RFC2616] and [RFC2617], and
on TCP/IP specification [RFC793]. There are no protocols that are dependent on the Web Point-and-
Print Protocol.

1.5  Prerequisites/Preconditions

The Web Point-and-Print Protocol is built on HTTP [RFC2616].

Before the protocol is invoked, a client obtains both the name of a server that supports the protocol
and the name of a printer. How a client obtains these names is not addressed in this specification.

1.6  Applicability Statement

The Web Point-and-Print Protocol is applicable in environments that require distribution of printer
driver software where other mechanisms, such as the Print System Remote Protocol [MS-RPRN], are
not available.

1.7  Versioning and Capability Negotiation

The Web Point-and-Print Protocol does not have versioning and does not contain locale-dependent
information.

1.8  Vendor-Extensible Fields

The Web Point-and-Print Protocol does not support vendor-extensible fields.

1.9  Standards Assignments

The Web Point-and-Print Protocol uses the following default assignments.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

10 / 31

Parameter

TCP/IP port for Hypertext Transfer Protocol (HTTP)

Value   Reference

80

[RFC2616] section
3.2.2.

TCP/IP port for Hypertext Transfer Protocol over Secure Socket Layer
(HTTPS)

443

[RFC2818] section 2.3.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

11 / 31

2  Messages

2.1  Transport

The Web Point-and-Print Protocol uses the following transports:

  HTTP over TCP/IP [RFC2616]

  HTTPS over TCP/IP [RFC2818]

2.2  Message Syntax

The Web Point-and-Print Protocol issues its requests from the client via Uniform Resource Locators
(URLs) in an HTTP GET request ([RFC2616] section 9.3) and the associated query qualifier. The
server returns a response via an HTTP response message, which returns either a Location header in a
redirect response ([RFC2616] section 14.30) or an HTTP 500 error ([RFC2616] section 10.5.1) if the
original HTTP GET request cannot be processed by the server.

2.2.1  Common Data Types

Unless otherwise noted, the following statements apply to this protocol:

  All strings consist of characters that are encoded in Unicode UTF-16LE and are null-

terminated. Each code point in a string, including null terminators, occupies 16 bits
([RFC2781] section 2.1).

  A list of strings is referred to as a MULTI_SZ structure ([MS-DTYP] section 2.3.8), in which
the characters that make up the string N+1 directly follow the terminating null character of
string N. The last string in a MULTI_SZ is terminated by two null characters.

  All parameters or members that specify the number of characters in a string or MULTI_SZ
specify the number of characters in the string, including the terminating null characters.

  All constraints that specify the maximum number of characters in a string or MULTI_SZ
specify the number of characters in the string, including the terminating null characters.

  All parameters or members that specify the number of bytes in buffers that contain a string or

MULTI_SZ specify the number of bytes in the buffer, including the terminating null
characters.

2.2.2  ClientInfo Value

There are four parameters encoded in a ClientInfo value:





The major version number of the client operating system.

The minor version number of the client operating system.

  A value specifying the type of client platform.

  A value specifying the client processor architecture.

Each parameter is an 8-bit unsigned value. For transmission on the wire, these values are packed into
32-bits to form a ClientInfo value, according to the following equation:

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

12 / 31

 ClientInfo = (major version) * (2^24) + (minor version) * (2^16) + (client platform) * (2^8)
+ (client processor architecture)

The major version number of the client operating system is dependent on the platform on which the
client is running.<2>

The minor version number of the client operating system is dependent on the platform on which the
client is running.<3>

The client platform value is dependent on the platform on which the client is running.<4>

The client processor architecture value MUST be one of the following.<5>

Value

Client processor architecture

0x00

0x01

0x02

0x03

0x06

0x09

0x05

Intel x86

MIPS

ALPHA

PPC

Itanium-based

AMD64 or Intel x64

ARM

2.2.3  Registry Type Values

The following table specifies valid registry type values for use in the dwType field of the
PrnDataRoot structure (section 2.2.7.1.2).

Registry type name/value

Description

REG_NONE

0x00000000

REG_SZ

0x00000001

REG_EXPAND_SZ

0x00000002

REG_BINARY

0x00000003

REG_DWORD

0x00000004

No value type is defined.

A string.

A string that can contain unexpanded references to environment variables,
for example, "%PATH%".

Binary data in any form.

A 32-bit number.

REG_DWORD_LITTLE_ENDIAN

A 32-bit number in little-endian format; equivalent to REG_DWORD.

0x00000004

REG_DWORD_BIG_ENDIAN

A 32-bit number in big-endian format.

0x00000005

13 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

Registry type name/value

Description

REG_LINK

0x00000006

REG_MULTI_SZ

0x00000007

Symbolic link to a registry key.

A REG_MULTI_SZ structure ([MS-RRP] section 2.2.5).

REG_RESOURCE_LIST

A device driver resource list.

0x00000008

REG_QWORD

0x0000000B

A 64-bit number.

REG_QWORD_LITTLE_ENDIAN

A 64-bit number in little-endian format; equivalent to REG_QWORD.

0x0000000B

2.2.4  Driver Selection Request

The Driver Selection Request is a standard HTTP GET request ([RFC2616] section 9.3) that sends
the printer driver selection criteria to the server in a PrinterDriverQuery. The format of the HTTP
request URL is specified by the following ABNF grammar rule [RFC5234].

 PrinterDriverQuery = [PRINT_SERVER] PRINTER_RESOURCE "?" DRIVER_QUERY
 PRINT_SERVER       = "http:" "//" host [":" port]
 PRINTER_RESOURCE   = abs_path
 DRIVER_QUERY       = "createexe" "&" CLIENT_INFO
 CLIENT_INFO        = 1*DIGIT

Here, "1*DIGIT" indicates "one or more occurrences of DIGIT". For the definitions of host, port,
abs_path, and DIGIT, see [RFC2616] sections 2.2 and 3.2, and [RFC2396] sections 3.2.2 and 3.3.

How the client determines the host, port, and abs_path is not specified by this protocol. Typically,
the port is either the HTTP default port "80" or another port that is configured by an administrator.

DRIVER_QUERY: The HTTP query component ([RFC2396] section 3.4), which constrains the

selection of printer driver instances for the printer resource.

PRINT_SERVER: The address of the Web Point-and-Print Protocol server, whether or not the request

is to be routed through a proxy.

PRINTER_RESOURCE: The name of the printer resource.

CLIENT_INFO: The environmental information on the client that is making the request, computed as

a ClientInfo value (section 2.2.2) and formatted as the ASCII representation of a decimal
integer.

2.2.5  Driver Selection Response

The Driver Selection Response is a standard HTTP redirect response, in the HTTP Location field,
to the URL of a file that contains the software and information necessary to install the printer driver
on the client. Specifically, the response MUST be a Redirection 302 response ([RFC2616] section
10.3.3), and the HTTP Location field MUST point to the printer driver file.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

14 / 31

2.2.6  Driver Download Request

The Driver Download Request is a standard HTTP GET request ([RFC2616] section 9.3) to the URL
specified in the Location header received in the Driver Selection Response (section 2.2.5).

2.2.7  Driver Download Response

The Driver Download Response is a standard HTTP response ([RFC2616] section 6) to the driver
download HTTP GET request (section 2.2.6). If the response return code is 200 (success), the
response entity body MUST contain the requested driver.

The HTTP redirect is to a URL with Content-Type set to application/octet-stream ([RFC2616] section
7.2.1), which specifies a cabinet file [MSDN-CAB] that has a .webpnp extension. The cabinet file
includes the following:

  Printer driver software files.

  An INF file [MSDN-INF] that is used during installation of the driver software.

  A BIN file (section 2.2.7.1) that provides the information needed to access printer

setup/configuration data.

  A DAT file (section 2.2.7.2) that provides additional print server and printer

setup/configuration data.

2.2.7.1  BIN File Format

The BIN file contains the information needed to access printer setup and configuration data. The
format of that information is shown below. All fields are in little-endian format.

File Header:

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

0x00000001

cItems

UserDevMode (variable)

...

PrnDataRoot (variable)

...

cItems (4 bytes): The number of PrnDataRoot structures (section 2.2.7.1.2) contained in the BIN

file. This value can be zero.

UserDevMode (variable): A UserDevMode structure (section 2.2.7.1.1).

PrnDataRoot (variable): A PrnDataRoot structure, which is repeated cItems times in this field.

2.2.7.1.1 UserDevMode Structure

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

15 / 31

The UserDevMode structure is used for general printer information.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

cbSize

reserved0

reserved1

reserved2

pDataOffset

cbData

Data (variable)

...

cbSize (4 bytes): The total number of bytes in this structure.

reserved0 (4 bytes): A value that MUST be set to zero and ignored on receipt.

reserved1 (4 bytes): A value that MUST be set to zero and ignored on receipt.

reserved2 (4 bytes): A value that MUST be set to zero and ignored on receipt.

pDataOffset (4 bytes): An unsigned integer that specifies the number of bytes from the start of the

UserDevMode structure to the Data field.

cbData (4 bytes): The number of bytes in the Data field.

Data (variable): Printer device settings in a _DEVMODE structure ([MS-RPRN] section 2.2.2.1).

This field MUST be padded to the next 64-bit boundary.

2.2.7.1.2 PrnDataRoot Structure

The PrnDataRoot structure describes a printer configuration value in the registry structure used by
a printer driver that is associated with a printer. This structure is repeated for a count specified by
the cItems member of the BIN file (section 2.2.7.1) header.

Printer configuration values control the behavior of the printer driver in a printer driver-specific way. A
printer driver can define an arbitrary number of printer configuration values.

Printer configuration values are uniquely identified by the following attributes:

Key: An arbitrary string defined by the printer driver associated with a printer.

ValueName: A string that contains the name of the printer configuration value.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

16 / 31

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

cbSize

dwType

KeyOffset

ValueNameOffset

pDataOffset

cbData

Key (variable)

...

ValueName (variable)

...

Data (variable)

...

cbSize (4 bytes): The total number of bytes in this PrnDataRoot.

dwType (4 bytes): A value that specifies a Registry Type value (section 2.2.3). For additional

information about registry types, see [MSDN-RTV].

KeyOffset (4 bytes): An unsigned integer that specifies the number of bytes from the start of the

PrnDataRoot to the Key field.

ValueNameOffset (4 bytes): An unsigned integer that specifies the number of bytes from the start

of the PrnDataRoot to the ValueName field.

pDataOffset (4 bytes): An unsigned integer that specifies the number of bytes from the start of the

PrnDataRoot to the Data field.

cbData (4 bytes): The number of bytes in the Data field.

Key (variable): A string that contains the name of the key that corresponds to the printer

configuration value identified by ValueName.

This field MUST be padded to the next 64-bit boundary.

ValueName (variable): A string that contains the name of a printer configuration value for the

printer driver.

This field MUST be padded to the next 64-bit boundary.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

17 / 31

Data (variable): A field containing data for the printer configuration value identified by ValueName.
The data contained in this field MUST be interpreted according to the registry value type that is
specified by the dwType field.

This field MUST be padded to the next 64-bit boundary.

2.2.7.2  DAT File Format

The DAT file contains options for installing a printer driver on the client machine. The following
restrictions apply to DAT files:





The name of the DAT file is "cab_ipp.dat".

The DAT file is a text file, in Unicode UTF-16LE character encoding.

DAT file options are specified as follows:

 /if
 /x
 /Q<PackageList>
 /b<PrinterBaseName>
 /f<InfName>
 /r<PrinterPortName>
 /m<DriverName>
 /n<UncName>
 /a<BinName>
 /q

Note  These options are presented in the form of a list for readability. This is not to specify that a
newline character is required to separate each option in a DAT file.

Option parameters are defined as follows:

PackageList: A list of CAB file names containing driver packages included in the .webpnp file. The

individual CAB file names are separated by ";".

PrinterBaseName: "\\http://<ServerName>\<PrinterName>", if the HTTP transport is being used;

or "\\https://<ServerName>\<PrinterName>", if the HTTPS transport is being used.

ServerName: The NetBIOS name, fully qualified domain name (FQDN), or IPv4 address.

PrinterName: The name of the printer.

InfName: The name of the INF file that is included in the .webpnp file.

PrinterPortName: The URL of the printer.

DriverName: The name of the printer driver.

UncName: The Universal Naming Convention (UNC) path of the server.

BinName: The name of the BIN file (section 2.2.7.1) that is included in the .webpnp file.

The following syntax rules apply to DAT file options and parameters:



The individual options can appear in any order.

  All options MUST be specified, except "/x", "/q" and "/Q".



The individual options MUST be separated by white space, which includes the UTF-16LE
character codes 0x0020, 0x000D, and 0x000A, and combinations thereof.

18 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

  White space between the switch of an option (for example, "/r") and its parameter (for

example, the PrinterPortName) MUST be accepted.

  A parameter MUST be surrounded by quotes if it contains white space.

  Quotes around a parameter that does not contain white space MUST be accepted.



The absence of quotes around a parameter that does not contain white space MUST be
accepted.



The option "/if" conveys no meaning in the protocol.

The following syntax rules apply to the "/x", "/q" and "/Q" options specifically:













Either the two options "/x" and "/q" together, or the single option "/Q", MUST be specified.

If "/x" and "/q" are specified "/Q" MUST NOT be specified.

If "/Q" is specified "/x" and "/q" MUST NOT be specified.

The options "/x" and "/q" together indicate the installation of a printer driver. The name and
location of the printer driver MUST be specified with the parameters of other options.

The option "/Q" specifies the installation of a driver package.

"/Q" MUST NOT be specified if the major version parameter of the ClientInfo value (section
2.2.2) in the request is less than 6.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

19 / 31

3  Protocol Details

3.1  Client Details

3.1.1  Abstract Data Model

None.

3.1.2  Timers

None.

3.1.3  Initialization

None.

3.1.4  Higher-Layer Triggered Events

A client invocation is the result of local client application activity. The client application specifies the
driver selection parameters. The syntax and semantics of these parameters are specified in section
2.2.4. No other higher-layer triggered events are defined.

3.1.5  Message Processing Events and Sequencing Rules

The Web Point-and-Print Protocol client is preconfigured with the URL of a Web Point-and-Print
Protocol server. To initiate the protocol, the client sends a Driver Selection Request (section 2.2.4)
message to the web server with which it has been configured.

If the server returns an HTTP status other than 302 (found), the client MUST return an error code to
the invoking application.

If the server returns an HTTP redirect code of 302, the client parses the received HTTP response to
retrieve the Location header. If a Location header is not found, the client MUST return an error code
to the invoking application. Otherwise, the client sends a Driver Download Request message
(section 2.2.6) to the server identified in the Location header.

If the HTTP response returned to the client has a status of 200 (success), the entity body received in
the HTTP response is returned to the invoking application as the printer driver package. If the HTTP
response is anything other than success, the client MUST return an error code to the invoking client
application.

3.1.6  Timer Events

None.

3.1.7  Other Local Events

No additional local events are used on the client beyond the events maintained in the underlying
HTTP.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

20 / 31

3.2  Server Details

3.2.1  Abstract Data Model

This section describes a conceptual model of a possible data organization that an implementation
might need to maintain to participate in this protocol. The organization described in this section is
provided to facilitate the explanation of how the protocol behaves. This specification does not mandate
that implementations adhere to this model as long as their external behavior is consistent with that
described in this specification.

The Web Point-and-Print Protocol depends on an abstract data model that maintains printers. The
print server behaves as if it hosted the following objects in the specified hierarchy.

List of Printers: Each printer represents a physical print device or a number of homogeneous

physical devices that are installed on the print server. Each printer object maintains the following
data elements:

  A name that uniquely identifies the printer.

  A reference to a printer driver object for the printer.



Per-user printer device settings in a _DEVMODE structure ([MS-RPRN] section 2.2.2.1).

List of Drivers: Each printer driver represents the software component responsible for converting

print content submitted by applications into device-specific commands. Each printer driver object
maintains the following data elements:

  A name that uniquely identifies the printer driver.

  A list of well-known modules (that is, rendering module, configuration module, and data

module).

  Additionally, each printer driver object maintains the following optional data elements:

  A list of dependent files.



Information on the printer driver manufacturer, printer driver time stamp, and version.

The abstract data model can relate each printer to a single printer driver ([MS-RPRN] section 3.1.1).

3.2.2  Timers

None.

3.2.3  Initialization

The server listens for HTTP and HTTPS requests.

3.2.4  Higher-Layer Triggered Events

None.

3.2.5  Message Processing Events and Sequencing Rules

The client initiates the Web Point-and-Print Protocol by sending a Driver Selection Request message
(section 2.2.4) to the server. On receiving this message, the server validates parameters as follows:



The PRINTER_RESOURCE parameter is a valid absolute path of a printer resource.

21 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024



The CLIENT_INFO section of the URL is a supported ClientInfo value (section 2.2.2) value.

If parameter validation fails, the server MUST fail the operation immediately, returning HTTP error
500 (Internal Server Error). If parameter validation succeeds, the server composes a response to the
client as follows:





The server uses the received CLIENT_INFO parameter to determine the location of the
appropriate printer driver. The mechanism is implementation specific. If a matching driver is
found, the server creates a Driver Selection Response (section 2.2.5), replying to the client
with an HTTP 302 redirect.<6>

If a matching driver cannot be found, the server fails the operation, replying to the client with
HTTP error 500 (internal server error).

Processing of a Driver Download Request (section 2.2.6) follows the rules specified for GET
processing ([RFC2616] section 9.3). The format of the data returned in the response is specified in the
Driver Download Response (section 2.2.7).

3.2.6  Timer Events

None.

3.2.7  Other Local Events

No additional local events are used on the client beyond the events maintained in the underlying
HTTP.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

22 / 31

4  Protocol Examples

4.1  Client/Server Interaction

The following diagram illustrates the interaction sequence to obtain a printer driver using the Web
Point-and-Print Protocol.

Figure 2: Obtaining a printer driver

4.2  Client/Server Messages

4.2.1  Client

The following text is a sample client Driver Selection Request (section 2.2.4), depicted in the
Client/Server Interaction figure in section 4.1. An HTTP GET request is used for sending printer
driver selection criteria to the server.

 GET /printers/printerModelXXX/.printer?createexe&83952128

4.2.2  Server

The following text is a sample server Driver Selection Response (section 2.2.5) to a client Driver
Selection Request (section 2.2.4), as depicted in the Client/Server Interaction figure in section
4.1. The server performs an HTTP Redirection 302 response to the URL of a file that contains the
requested printer driver and installation software.

 HTTP/1.1 302 Object Moved
 Content-Length: 176
 Content-Type: text/html
 Location: http://1.1.1.2/printers/PrtCabs/85CCX862.webpnp
 Server: Microsoft-IIS/6.0
 X-Powered-By: ASP.NET
 Date: Wed, 12 Jul 2006 21:44:42 GMT
 Connection: close

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

23 / 31

 <head><title>Document Moved</title></head>
 <body><h1>Object Moved</h1>This document may be found
    <a HREF="http://1.1.1.2/printers/PrtCabs/85CCX862.webpnp">here</a>
 </body>
 <END OF ENTITY>

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

24 / 31

5  Security Considerations

The Web Point-and-Print Protocol does not provide any facilities for mutual authentication, so server
identities cannot be verified.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

25 / 31

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

The terms "earlier" and "later", when used with a product version, refer to either all preceding
versions or all subsequent versions, respectively. The term "through" refers to the inclusive range of
versions. Applicable Microsoft products are listed chronologically in this section.

  Windows 2000 operating system

  Windows 2000 Server operating system

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

<1> Section 1.3: The Web Point-and-Print Protocol is used when the Print System Remote Protocol
(for more information, see [MS-RPRN]) is not available due to policy restrictions, policy restrictions on
the use of RPC, or other restrictions on printing. When the Web Point-and-Print Protocol is used to
install the printer drivers, the print spooler uses the Internet Printing Protocol (IPP) [RFC2910]
[RFC2911] to communicate with the actual print device.

26 / 31

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

<2> Section 2.2.2: Windows clients use one of the following major version values.

Value   Major version

0x05  Windows 2000 Server, Windows XP, or Windows Server 2003

0x06  Windows Vista through Windows 8.1 and Windows Server 2008 through Windows Server 2012 R2

operating system

0x0A  Windows 10 and later and Windows Server 2016 and later

<3> Section 2.2.2: Windows clients use one of the following minor version values.

Value   Minor version

0x00  Windows 2000 Server, Windows Vista, Windows Server 2008, Windows 10, Windows Server 2016,

Windows Server 2019, Windows 11, Windows Server 2025

0x01  Windows XP, Windows 7, or Windows Server 2008 R2

0x02  Windows Server 2003, Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2

<4> Section 2.2.2: Windows clients use one of the following client platform values.

Value   Client platform

0x01  Windows 95 operating system, Windows 98 operating system, or Windows Millennium Edition operating

system.

0x02  Windows 2000, Windows XP and later, Windows Server 2003 and later

Windows Vista and later and Windows Server 2008 and later:

  A client platform value of 0x01 is invalid.

  All other client platform values are treated as 0x02.

Windows 2000, Windows 2000 Server, Windows XP, and Windows Server 2003:

  All client platform values different from 0x01 or 0x02 are treated as 0x02.



If client platform is 0x01, the client processor architecture value will be ignored and will
always be treated as Intel x86.

<5> Section 2.2.2: Windows client processor architecture values:

Not supported on Windows 2000 or Windows 2000 Server:

  AMD64

Not supported on Windows Server 2003 and later and Windows Vista and later:

  MIPS

  ALPHA



PPC

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

27 / 31

Not supported on Windows 2000, Windows 2000 Server, Windows Server 2003, Windows Vista,
Windows Server 2008, Windows 7, or Windows Server 2008 R2 operating system:

  ARM

<6> Section 3.2.5: The Windows implementation extracts information from printer driver INF files to
determine the appropriate drivers to return to the client. For information on printer driver INF files,
see [MSDN-PRNINF].

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

28 / 31

7  Change Tracking

No table of changes is available. The document is either new or has had no changes since its last
release.

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

29 / 31

8  Index
A

Abstract data model
   client 20
   server 21
Applicability 10

B

BIN file format 15
BIN_File_Format packet 15

C

Capability negotiation 10
Change tracking 29
Client
   abstract data model 20
   client/server interaction example 23
   client/server messages example 23
   example 23
   higher-layer triggered events 20
   initialization 20
   local events 20
   message processing 20
   other local events 20
   sequencing rules 20
   timer events 20
   timers 20
ClientInfo 12
ClientInfo Value message 12
Common Data Types message 12

D

DAT file format 18
Data model - abstract
   client 20
   server 21
Data types 12
Driver download request 14
Driver Download Request message 14
Driver download response 15
Driver Download Response message 15
Driver selection request 14
Driver Selection Request message 14
Driver selection response 14
Driver Selection Response message 14

E

Examples
   client/server interaction 23
   client/server messages 23

F

Fields - vendor-extensible 10
File formats
   BIN 15
   DAT 18

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

G

Glossary 7

H

Higher-layer triggered events
   client 20
   server 21

I

Implementer - security considerations 25
Informative references 9
Initialization
   client 20
   server 21
Introduction 7

L

Local events
   client 20
   server 22

M

Message processing
   client 20
   server 21
Messages
   ClientInfo Value 12
   Common Data Types 12
   Driver Download Request 14
   Driver Download Response 15
   Driver Selection Request 14
   Driver Selection Response 14
   Registry Type Values 13
   syntax 12
   transport 12

N

Normative references 8

O

Other local events
   client 20
   server 22
Overview (synopsis) 9

P

Preconditions 10
Prerequisites 10
PrnDataRoot packet 16
Product behavior 26

R

30 / 31

References 8
   informative 9
   normative 8
REG_BINARY 13
REG_DWORD 13
REG_DWORD_BIG_ENDIAN 13
REG_DWORD_LITTLE_ENDIAN 13
REG_EXPAND_SZ 13
REG_LINK 13
REG_MULTI_SZ 13
REG_NONE 13
REG_QWORD 13
REG_QWORD_LITTLE_ENDIAN 13
REG_RESOURCE_LIST 13
REG_SZ 13
Registry Type Values message 13
Relationship to other protocols 10

S

Security - implementer considerations 25
Sequencing rules
   client 20
   server 21
Server
   abstract data model 21
   client/server interaction example 23
   client/server messages example 23
   example 23
   higher-layer triggered events 21
   initialization 21
   local events 22
   message processing 21
   other local events 22
   sequencing rules 21
   timer events 22
   timers 21
Standards assignments 10
Syntax 12

T

Timer events
   client 20
   server 22
Timers
   client 20
   server 21
Tracking changes 29
Transport 12
Triggered events - higher-layer
   client 20
   server 21

U

UserDevMode packet 15

V

Vendor-extensible fields 10
Versioning 10

[MS-WPRN] - v20240916
Web Point-and-Print Protocol
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

31 / 31

