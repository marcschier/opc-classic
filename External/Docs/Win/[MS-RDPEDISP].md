[MS-RDPEDISP]:

Remote Desktop Protocol: Display Update Virtual Channel
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

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1 / 20

Revision Summary

Date

Revision
History

Revision
Class

Comments

8/8/2013

1.0

11/14/2013  2.0

2/13/2014

3.0

5/15/2014

3.0

New

Major

Major

None

Released new document.

Updated and revised the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

4.0

Major

Significantly changed the technical content.

10/16/2015  4.0

None

No changes to the meaning, language, or formatting of the
technical content.

7/14/2016

4.0

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

Major

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

Significantly changed the technical content.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

2 / 20

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
Protocol Overview (Synopsis) .............................................................................. 6
Relationship to Other Protocols ............................................................................ 6
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

2.2.1

2.1
2.2

2  Messages ................................................................................................................. 8
Transport .......................................................................................................... 8
Message Syntax ................................................................................................. 8
Common Data Types ..................................................................................... 8
DISPLAYCONTROL_HEADER ..................................................................... 8
Display Control Messages .............................................................................. 8
DISPLAYCONTROL_CAPS_PDU .................................................................. 8
DISPLAYCONTROL_MONITOR_LAYOUT_PDU ............................................... 9
DISPLAYCONTROL_MONITOR_LAYOUT ............................................... 10

2.2.2.1
2.2.2.2

2.2.2.2.1

2.2.1.1

2.2.2

3.2

3.1

3.1.6
3.1.7

3.1.5.1
3.1.5.2

3.1.1
3.1.2
3.1.3
3.1.4
3.1.5

3  Protocol Details ..................................................................................................... 12
Server Details .................................................................................................. 12
Abstract Data Model .................................................................................... 12
Timers ...................................................................................................... 12
Initialization ............................................................................................... 12
Higher-Layer Triggered Events ..................................................................... 12
Processing Events and Sequencing Rules ....................................................... 12
Sending DISPLAYCONTROL_CAPS_PDU .................................................... 12
Processing DISPLAYCONTROL_MONITOR_LAYOUT_PDU ............................. 12
Timer Events .............................................................................................. 12
Other Local Events ...................................................................................... 13
Client Details ................................................................................................... 13
Abstract Data Model .................................................................................... 13
Maximum Monitor Count ........................................................................ 13
Maximum Monitor Area Factor A ............................................................. 13
Maximum Monitor Area Factor B ............................................................. 13
Timers ...................................................................................................... 13
Initialization ............................................................................................... 13
Higher-Layer Triggered Events ..................................................................... 13
Processing Events and Sequencing Rules ....................................................... 13
Processing DISPLAYCONTROL_CAPS_PDU ................................................ 13
Sending DISPLAYCONTROL_MONITOR_LAYOUT_PDU ................................. 14
Timer Events .............................................................................................. 14
Other Local Events ...................................................................................... 14

3.2.1.1
3.2.1.2
3.2.1.3

3.2.2
3.2.3
3.2.4
3.2.5

3.2.5.1
3.2.5.2

3.2.6
3.2.7

3.2.1

4  Protocol Examples ................................................................................................. 15

5  Security ................................................................................................................. 16
Security Considerations for Implementers ........................................................... 16
Index of Security Parameters ............................................................................ 16

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 17

7  Change Tracking .................................................................................................... 18

3 / 20

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8  Index ..................................................................................................................... 19

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

4 / 20

1  Introduction

This document specifies the Remote Desktop Protocol: Display Control Channel Extension to the
Remote Desktop Protocol: Basic Connectivity and Graphics Remoting, as specified in [MS-RDPBCGR]
sections 1 to 5. This control protocol is used by the client to request display configuration changes in a
remote session. Display configuration changes include the addition, removal and repositioning of
monitors, resolution updates, and orientation updates.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

ANSI character: An 8-bit Windows-1252 character set unit.

desktop scale factor: The scale factor (as a percentage) applied to Windows Desktop

Applications.

device scale factor: The scale factor as a percentage is applied to Windows Store Apps. This

value has to be calculated such that the effective maximum height of a Windows Store App is
always greater than 768 pixels, otherwise the app will not start.

dynamic virtual channel: A transport used for lossless communication between an RDP client and

a server component over a main data connection, as specified in [MS-RDPEDYC].

virtual channel: A communication channel available in a TS server session between applications

running at the server and applications running on the TS client.

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

[MS-RDPBCGR] Microsoft Corporation, "Remote Desktop Protocol: Basic Connectivity and Graphics
Remoting".

[MS-RDPEDYC] Microsoft Corporation, "Remote Desktop Protocol: Dynamic Channel Virtual Channel
Extension".

[MS-RDPEGFX] Microsoft Corporation, "Remote Desktop Protocol: Graphics Pipeline Extension".

[MS-RDPRFX] Microsoft Corporation, "Remote Desktop Protocol: RemoteFX Codec Extension".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

5 / 20

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

1.2.2  Informative References

None.

1.3  Protocol Overview (Synopsis)

The sequence of messages exchanged by the Remote Desktop Protocol: Display Control Virtual
Channel Extension is described in the following figure. The messages exchanged in this diagram are
strictly sequential.

Figure 1: The Display Control message sequence

After the Display Control dynamic virtual channel has been opened, the server communicates
display capabilities to the client by sending the DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1)
message. This message specifies a set of parameters to which the client is required to adhere when
sending the DISPLAYCONTROL_MONITOR_LAYOUT_PDU (section 2.2.2.2) message.

To request a display configuration change on the server (such as the addition of a monitor or a new
resolution for an existing monitor), the client sends the
DISPLAYCONTROL_MONITOR_LAYOUT_PDU message to the server. If the requested
configuration is not possible, or is invalid, the server will not update the remote session with the
requested parameters.

Changes in the server-side display configuration occur out of band to the Remote Desktop Protocol:
Display Control Virtual Channel Extension. If the requested graphics configuration is valid and can be
configured on the server, then the server will either:



Initiate a Deactivation-Reactivation Sequence (as specified in [MS-RDPBCGR] section 1.3.1.3) if
the Remote Desktop Protocol: Graphics Pipeline Extension is not being used to remote session
graphics.

  Restart the graphics pipeline using the surface management commands (specified in [MS-

RDPEGFX] section 1.3) if the Remote Desktop Protocol: Graphics Pipeline Extension is being used
to remote session graphics.

The DISPLAYCONTROL_MONITOR_LAYOUT_PDU message can be sent whenever a display
configuration change is required.

1.4  Relationship to Other Protocols

The Remote Desktop Protocol: Display Control Virtual Channel Extension is embedded in a dynamic
virtual channel transport, as specified in [MS-RDPEDYC] sections 1 to 3.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

6 / 20

1.5  Prerequisites/Preconditions

The Remote Desktop Protocol: Display Control Virtual Channel Extension operates only after the
dynamic virtual channel transport is fully established. If the dynamic virtual channel transport is
terminated, the Remote Desktop Protocol: Display Control Virtual Channel Extension is also
terminated. The protocol is terminated by closing the underlying virtual channel. For details about
closing the dynamic virtual channel, refer to [MS-RDPEDYC] section 3.2.5.2.

If the RemoteFX codec ([MS-RDPRFX] sections 2.2.2 and 3.1.8) is being used to encode graphics data
from a remote session, then the Remote Desktop Protocol: Display Control Virtual Channel Extension
SHOULD NOT be used to request display configuration changes.

1.6  Applicability Statement

The Remote Desktop Protocol: Display Control Virtual Channel Extension is applicable in scenarios
where a mechanism to request display configuration changes in a remote session without
disconnecting and reconnecting is required.

1.7  Versioning and Capability Negotiation

None.

1.8  Vendor-Extensible Fields

None.

1.9  Standards Assignments

None.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

7 / 20

2  Messages

2.1  Transport

The Remote Desktop Protocol: Display Control Virtual Channel Extension is designed to operate over a
dynamic virtual channel, as specified in [MS-RDPEDYC] sections 1 to 3. The dynamic virtual channel
name is the null-terminated ANSI character string "Microsoft::Windows::RDS::DisplayControl". The
usage of channel names in the context of opening a dynamic virtual channel is specified in [MS-
RDPEDYC] section 2.2.2.1.

2.2  Message Syntax

The following sections specify the Remote Desktop Protocol: Display Virtual Channel Extension
message syntax.

2.2.1  Common Data Types

2.2.1.1  DISPLAYCONTROL_HEADER

The DISPLAYCONTROL_HEADER structure is included in all display control PDUs and specifies the
PDU type and the length of the PDU.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Type

Length

Type (4 bytes): A 32-bit unsigned integer that specifies the display control PDU type.

Value

Meaning

DISPLAYCONTROL_PDU_TYPE_CAPS

DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1)

0x00000005

DISPLAYCONTROL_PDU_TYPE_MONITOR_LAYOUT

0x00000002

DISPLAYCONTROL_MONITOR_LAYOUT_PDU
(section 2.2.2.2)

Length (4 bytes): A 32-bit unsigned integer that specifies the length of the display control PDU, in

bytes. This value MUST include the length of the DISPLAYCONTROL_HEADER (8 bytes).

2.2.2  Display Control Messages

2.2.2.1  DISPLAYCONTROL_CAPS_PDU

The DISPLAYCONTROL_CAPS_PDU message is a server-to-client PDU that is used to specify a set of
parameters which the client must adhere to when sending the
DISPLAYCONTROL_MONITOR_LAYOUT_PDU (section 2.2.2.2) message.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

8 / 20

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Header

...

MaxNumMonitors

MaxMonitorAreaFactorA

MaxMonitorAreaFactorB

Header (8 bytes): A DISPLAYCONTROL_HEADER (section 2.2.1.1) structure. The Type field MUST

be set to DISPLAYCONTROL_PDU_TYPE_CAPS (0x00000005).

MaxNumMonitors (4 bytes): A 32-bit unsigned integer that specifies the maximum number of

monitors supported by the server.

MaxMonitorAreaFactorA (4 bytes): A 32-bit unsigned integer that is used to specify the maximum
monitor area supported by the server. The maximum supported monitor area (in square pixels) is
given by MaxNumMonitors * MaxMonitorAreaFactorA * MaxMonitorAreaFactorB.

MaxMonitorAreaFactorB (4 bytes): A 32-bit unsigned integer that is used to specify the maximum
monitor area supported by the server. The maximum supported monitor area (in square pixels) is
given by MaxNumMonitors * MaxMonitorAreaFactorA * MaxMonitorAreaFactorB.

2.2.2.2  DISPLAYCONTROL_MONITOR_LAYOUT_PDU

The DISPLAYCONTROL_MONITOR_LAYOUT_PDU message is a client-to-server PDU that is used
to request a display configuration change on the server, such as the addition of a monitor or a new
resolution for an existing monitor. Note that the entire monitor layout MUST be included in the
Monitors field even if the configuration of only a single monitor is updated.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Header

...

MonitorLayoutSize

NumMonitors

Monitors (variable)

...

Header (8 bytes): A DISPLAYCONTROL_HEADER (section 2.2.1.1) structure. The Type field MUST

be set to DISPLAYCONTROL_PDU_TYPE_MONITOR_LAYOUT (0x00000002).

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

9 / 20

MonitorLayoutSize (4 bytes): A 32-bit unsigned integer that specifies the size, in bytes, of a single

element in the Monitors field. This field MUST be set to 40 bytes, the size of the
DISPLAYCONTROL_MONITOR_LAYOUT structure (section 2.2.2.2.1).

NumMonitors (4 bytes): A 32-bit unsigned integer that specifies the number of display monitor

definitions in the Monitors field.  The maximum number of monitor definitions allowed is specified
in the MaxNumMonitors field of the DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1)
message.

Monitors (variable): A variable-length array containing a series of

DISPLAYCONTROL_MONITOR_LAYOUT structures that specify the display monitor layout of
the client. The number of DISPLAYCONTROL_MONITOR_LAYOUT structures is specified by the
NumMonitors field. The area (in square pixels) of the layout specified by the
DISPLAYCONTROL_MONITOR_LAYOUT structures MUST NOT exceed the maximum monitor
area defined by the server in the DISPLAYCONTROL_CAPS_PDU message.

2.2.2.2.1 DISPLAYCONTROL_MONITOR_LAYOUT

The DISPLAYCONTROL_MONITOR_LAYOUT structure is used to specify the characteristics of a
monitor. The coordinates used to describe the monitor position MUST be relative to the upper-left
corner of the monitor designated as the "primary display monitor". The upper-left corner of the
primary monitor is always (0, 0).

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Flags

Left

Top

Width

Height

PhysicalWidth

PhysicalHeight

Orientation

DesktopScaleFactor

DeviceScaleFactor

Flags (4 bytes): A 32-bit unsigned integer that specifies monitor configuration flags.

Flag

Meaning

DISPLAYCONTROL_MONITOR_PRIMARY

0x00000001

The monitor specified by this structure is the primary
monitor.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

10 / 20

Left (4 bytes): A 32-bit signed integer that specifies the x-coordinate of the upper-left corner of the

display monitor.

Top (4 bytes): A 32-bit signed integer that specifies the y-coordinate of the upper-left corner of the

display monitor.

Width (4 bytes): A 32-bit unsigned integer that specifies the width of the monitor in pixels. The
width MUST be greater than or equal to 200 pixels and less than or equal to 8192 pixels, and
MUST NOT be an odd value.

Height (4 bytes): A 32-bit unsigned integer that specifies the height of the monitor in pixels. The
height MUST be greater than or equal to 200 pixels and less than or equal to 8192 pixels.

PhysicalWidth (4 bytes): A 32-bit unsigned integer that specifies the physical width of the monitor,

in millimeters (mm). This value MUST be ignored if it is less than 10 mm or greater than 10,000
mm or the PhysicalHeight field is less than 10 mm or greater than 10,000 mm.

PhysicalHeight (4 bytes): A 32-bit unsigned integer that specifies the physical height of the

monitor, in millimeters. This value MUST be ignored if it is less than 10 mm or greater than
10,000 mm or PhysicalWidth is less than 10 mm or greater than 10,000 mm.

Orientation (4 bytes): A 32-bit unsigned integer that specifies the orientation of the monitor in

degrees. Valid values are 0, 90, 180 or 270. This value MUST be ignored if it is not set to one of
these values.

Value

Meaning

ORIENTATION_LANDSCAPE

The desktop is not rotated.

0

ORIENTATION_PORTRAIT

The desktop is rotated clockwise by 90 degrees.

90

ORIENTATION_LANDSCAPE_FLIPPED

The desktop is rotated clockwise by 180 degrees.

180

ORIENTATION_PORTRAIT_FLIPPED

The desktop is rotated clockwise by 270 degrees.

270

DesktopScaleFactor (4 bytes): A 32-bit, unsigned integer that specifies the desktop scale factor
of the monitor. This value MUST be ignored if it is less than 100 percent or greater than 500
percent, or if DeviceScaleFactor is not 100 percent, 140 percent, or 180 percent.

DeviceScaleFactor (4 bytes): A 32-bit, unsigned integer that specifies the device scale factor of
the monitor. This value MUST be ignored if it is not set to 100 percent, 140 percent, or 180
percent or if DesktopScaleFactor is less than 100 percent or greater than 500 percent.<1>

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

11 / 20

3  Protocol Details

3.1  Server Details

3.1.1  Abstract Data Model

This section describes a conceptual model of possible data organization that an implementation
maintains to participate in this protocol. The described organization is provided to facilitate the
explanation of how the protocol behaves. This document does not mandate that implementations
adhere to this model as long as their external behavior is consistent with that described in this
document.

Note  It is possible to implement the following conceptual data by using a variety of techniques as
long as the implementation produces external behavior that is consistent with that described in this
document.

3.1.2  Timers

None.

3.1.3  Initialization

None.

3.1.4  Higher-Layer Triggered Events

None.

3.1.5  Processing Events and Sequencing Rules

3.1.5.1  Sending DISPLAYCONTROL_CAPS_PDU

The structure and fields of the DISPLAYCONTROL_CAPS_PDU message are specified in section
2.2.2.1. The message fields MUST be populated in accordance with this description.

3.1.5.2  Processing DISPLAYCONTROL_MONITOR_LAYOUT_PDU

The structure and fields of the DISPLAYCONTROL_MONITOR_LAYOUT_PDU message are specified
in section 2.2.2.2. The server SHOULD trigger a display configuration update in the remote session
using the parameters specified by the client if all of the following conditions are true:







 All of the fields specified in the DISPLAYCONTROL_MONITOR_LAYOUT_PDU message are
valid, consistent and within range.

 None of the specified monitors overlap.

 Each monitor is adjacent to at least one other monitor (even if only at a single point).

3.1.6  Timer Events

None.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

12 / 20

3.1.7  Other Local Events

None.

3.2  Client Details

3.2.1  Abstract Data Model

This section describes a conceptual model of possible data organization that an implementation
maintains to participate in this protocol. The described organization is provided to facilitate the
explanation of how the protocol behaves. This document does not mandate that implementations
adhere to this model as long as their external behavior is consistent with that described in this
document.

Note  It is possible to implement the following conceptual data by using a variety of techniques as
long as the implementation produces external behavior that is consistent with that described in this
document.

3.2.1.1  Maximum Monitor Count

The Maximum Monitor Count store contains the contents of the MaxNumMonitors field of the
DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1) message.

3.2.1.2  Maximum Monitor Area Factor A

The Maximum Monitor Area Factor A store contains the contents of the MaxMonitorAreaFactorA
field of the DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1) message.

3.2.1.3  Maximum Monitor Area Factor B

The Maximum Monitor Area Factor B store contains the contents of the MaxMonitorAreaFactorB
field of the DISPLAYCONTROL_CAPS_PDU (section 2.2.2.1) message.

3.2.2  Timers

None.

3.2.3  Initialization

None.

3.2.4  Higher-Layer Triggered Events

None.

3.2.5  Processing Events and Sequencing Rules

3.2.5.1  Processing DISPLAYCONTROL_CAPS_PDU

The structure and fields of the DISPLAYCONTROL_CAPS_PDU message are specified in section
2.2.2.1. Upon receiving the DISPLAYCONTROL_CAPS_PDU message, the client MUST store the
contents of the MaxNumMonitors, MaxMonitorAreaFactorA and MaxMonitorAreaFactorB fields
in the Maximum Monitor Count (section 3.2.1.1), Maximum Monitor Area Factor A (section
3.2.1.2), and Maximum Monitor Area Factor B (section 3.2.1.3) ADM elements respectively.

13 / 20

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

3.2.5.2  Sending DISPLAYCONTROL_MONITOR_LAYOUT_PDU

The structure and fields of the DISPLAYCONTROL_MONITOR_LAYOUT_PDU message are specified
in section 2.2.2.2. The message fields MUST be populated in accordance with that description. The
maximum area of the monitor layout MUST NOT exceed product of the Maximum Monitor Count
(section 3.2.1.1), Maximum Monitor Area Factor A (section 3.2.1.2), and Maximum Monitor
Area Factor B (section 3.2.1.3) ADM elements.

3.2.6  Timer Events

None.

3.2.7  Other Local Events

None.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

14 / 20

4  Protocol Examples

None.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

15 / 20

5  Security

5.1  Security Considerations for Implementers

None.

5.2  Index of Security Parameters

None.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

16 / 20

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

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

<1> Section 2.2.2.2.1:  The DeviceScaleFactor field is processed only in Windows 8.1.

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

17 / 20

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

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

18 / 20

8  Index
A

Abstract data model
   client 13
   server 12
Applicability 7

C

Capability negotiation 7
Change tracking 18
Client
   abstract data model 13
   higher-layer triggered events 13
   initialization 13
   other local events 14
   timer events 14
   timers 13
Client – abstract data model 13

D

Data model - abstract
   client 13
   server 12
DISPLAYCONTROL_CAPS_PDU packet 8
DISPLAYCONTROL_HEADER packet 8
DISPLAYCONTROL_MONITOR_LAYOUT packet

(section 2.2.2.2 9, section 2.2.2.2.1 10)

E

Examples - overview 15

F

Fields - vendor-extensible 7

G

Glossary 5

H

Higher-layer triggered events
   client 13
   server 12

I

Implementer - security considerations 16
Index of security parameters 16
Informative references 6
Initialization
   client 13
   server 12
Introduction 5

M

Messages
   syntax 8
   transport 8

N

Normative references 5

O

Other local events
   client 14
   server 13
Overview (synopsis) 6

P

Parameters - security index 16
Preconditions 7
Prerequisites 7
Product behavior 17

R

References 5
   informative 6
   normative 5
Relationship to other protocols 6

S

Security
   implementer considerations 16
   parameter index 16
Security considerations 16
Server
   abstract data model 12
   higher-layer triggered events 12
   initialization 12
   other local events 13
   timer events 12
   timers 12
Server – abstract data model 12
Standards assignments 7

T

Timer events
   client 14
   server 12
Timers
   client 13
   server 12
Tracking changes 18
Transport 8
Transport messages 8
Triggered events - higher-layer
   client 13
   server 12

V

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

19 / 20

Vendor-extensible fields 7
Versioning 7

[MS-RDPEDISP] - v20240423
Remote Desktop Protocol: Display Update Virtual Channel Extension
Copyright © 2024 Microsoft Corporation
Release: April 23, 2024

20 / 20

