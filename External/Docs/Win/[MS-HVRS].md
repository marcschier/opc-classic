[MS-HVRS]:

Hyper-V Remote Storage Profile

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

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

1 / 10

Revision Summary

Date

Revision
History

Revision
Class

Comments

7/14/2016  1.0

3/16/2017  2.0

6/1/2017

2.0

New

Major

None

Released new document.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

9/15/2017  3.0

Major

Significantly changed the technical content.

12/1/2017  3.0

9/12/2018  4.0

4/7/2021

5.0

6/25/2021  6.0

9/16/2024  6.0

None

Major

Major

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

No changes to the meaning, language, or formatting of the
technical content.

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

2 / 10

Table of Contents

1.1
1.2

1  Introduction ............................................................................................................ 4
Glossary ........................................................................................................... 4
References ........................................................................................................ 4
Normative References ................................................................................... 4
Informative References ................................................................................. 4
Microsoft Implementations .................................................................................. 4
Standards Support Requirements ......................................................................... 5
Notation ............................................................................................................ 5

1.2.1
1.2.2

1.3
1.4
1.5

2.1
2.2

2  Standards Support Statements ................................................................................ 6
Normative Variations .......................................................................................... 6
Clarifications ..................................................................................................... 6
[MS-SMB2] Server Message Block (SMB) Protocols Version 2 and 3 .................... 6
[MS-FSA] File System Algorithms ................................................................... 6
[MS-RSVD] Remote Shared Virtual Disk Protocol .............................................. 7
[MS-SQOS] Storage Quality of Service Protocol ................................................ 8
[MS-FSRVP] File Server Remote VSS Protocol .................................................. 8
Error Handling ................................................................................................... 8
Security ............................................................................................................ 8

2.2.1
2.2.2
2.2.3
2.2.4
2.2.5

2.3
2.4

3  Change Tracking ...................................................................................................... 9

4  Index ..................................................................................................................... 10

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

3 / 10

1  Introduction

Microsoft Hyper-V supports virtual machines whose associated files are hosted on Server Message
Block (SMB) Version 3 shares. These files can include virtual machine configuration files, virtual
machine saved-state files, and virtual hard-disk files. The Hyper-V Remote Storage Profile clarifies the
level of support that Hyper-V requires from SMB Version 3 servers that host these types of files.

1.1  Glossary

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

[MS-FSA] Microsoft Corporation, "File System Algorithms".

[MS-FSCC] Microsoft Corporation, "File System Control Codes".

[MS-FSRVP] Microsoft Corporation, "File Server Remote VSS Protocol".

[MS-RSVD] Microsoft Corporation, "Remote Shared Virtual Disk Protocol".

[MS-SMB2] Microsoft Corporation, "Server Message Block (SMB) Protocol Versions 2 and 3".

[MS-SQOS] Microsoft Corporation, "Storage Quality of Service Protocol".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

1.2.2  Informative References

None.

1.3  Microsoft Implementations

  Windows 8 operating system

  Windows Server 2012 operating system

  Windows 8.1 operating system

  Windows Server 2012 R2 operating system

  Windows 10 operating system

  Windows Server 2016 operating system

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

4 / 10

  Windows Server 2019 operating system

  Windows Server 2022 operating system

  Windows 11 operating system

  Windows Server 2025 operating system

1.4  Standards Support Requirements

The conformance requirements for [MS-SMB2], [MS-FSA], [MS-RSVD], [MS-SQOS], and [MS-FSRVP]
are that all required portions of the specifications are implemented according to the specification, and
any optional portions that are included are implemented according to the specification.

1.5  Notation

The following notations are used to identify clarifications in section 2.2:

Notation

Explanation

C####

This notation identifies a clarification of ambiguity in the target specification. This includes
imprecise statements, omitted information, discrepancies, and errata. This does not include
data formatting clarifications.

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

5 / 10

2  Standards Support Statements

2.1  Normative Variations

None.

2.2  Clarifications

The following subsections identify clarifications relative to [MS-SMB2], [MS-FSA], [MS-RSVD], [MS-
SQOS] and [MS-FSRVP].

2.2.1  [MS-SMB2] Server Message Block (SMB) Protocols Version 2 and 3

C0001:

Windows 8.1 operating system, Windows Server 2012 R2 operating system, Windows 10 operating
system, Windows Server 2016 operating system, Windows Server 2019 operating system, Windows
Server 2022 operating system, Windows 11 operating system and Windows Server 2025 operating
system

The server MUST support the SMB 3.0 or higher dialect, as described in [MS-SMB2] section 1.7.

C0002:

Windows 8.1, Windows Server 2012 R2, Windows 10, Windows Server 2016, Windows Server 2019,
Windows Server 2022, Windows 11 and Windows Server 2025

The server MUST support persistent handles.

C0003:

Windows 8 operating system, Windows Server 2012 operating system, Windows 8.1, Windows Server
2012 R2, Windows 10, Windows Server 2016, Windows Server 2019, Windows Server 2022, Windows
11 and Windows Server 2025

The server MUST support the FSCTL_LMR_REQUEST_RESILIENCY command.

2.2.2  [MS-FSA] File System Algorithms

C0004:

Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2, Windows 10, Windows
Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and Windows Server 2025

If the server doesn't support the FSCTL_OFFLOAD_READ and FSCTL_OFFLOAD_WRITE commands, as
specified in [MS-FSA] sections 2.1.5.10.20 and 2.1.5.10.21, then any small computer system interface
(SCSI) ODX commands initiated by the virtual machine operating system fail.

If the server supports the FSCTL_OFFLOAD_READ and FSCTL_OFFLOAD_WRITE commands, then
Hyper-V can issue these commands to optimize the performance of virtual disk creation, merge,
compaction, and mirroring operations.

C0005:

Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2, Windows 10, Windows
Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and Windows Server 2025

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

6 / 10

If the server supports the FSCTL_SET_ZERO_DATA command, as specified in [MS-FSA] section
2.1.5.10.39, then Hyper-V can issue this command to optimize the performance of virtual-disk-
creation operations.

C0006:

Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2, Windows 10, Windows
Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and Windows Server 2025

If the server doesn't support the FSCTL_FILE_LEVEL_TRIM command, as specified in [MS-FSA] section
2.1.5.10.6:

  Support for the SCSI UNMAP command is still advertised to the virtual machine operating system

in the response to SCSI INQUIRY commands.

  SCSI UNMAP requests initiated by the virtual machine operating system are still completed

successfully to the virtual machine operating system, but they have no side effects (they are not
translated into corresponding file system requests to release space allocated to the files backing
the virtual disks attached to the virtual machine).

C0007:

Windows 10, Windows Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and
Windows Server 2025

If the server advertises the FILE_SUPPORTS_BLOCK_REFCOUNTING flag for a given Open (as defined
in FILE_FS_ATTRIBUTE_INFORMATION), the server MUST support the
FSCTL_DUPLICATE_EXTENTS_TO_FILE command, as specified in [MS-FSA] section 2.1.5.10.4.

C0008:

Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2, Windows 10, Windows
Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and Windows Server 2025

Hyper-V doesn't support virtual disk files with any of the following flags set, as specified in [MS-FSCC]
section 2.6:







FILE_ATTRIBUTE_COMPRESSED

FILE_ATTRIBUTE_ENCRYPTED

FILE_ATTRIBUTE_SPARSE_FILE

C0009:

Windows 10 v1703 operating system, Windows Server 2019, Windows Server 2022, Windows 11 and
Windows Server 2025

If the server advertises the FILE_SUPPORTS_SPARSE_VDL flag for a given Open (as specified in [MS-
FSCC] section 2.5.1), Hyper-V will support virtual disk files with the FILE_ATTRIBUTE_SPARSE_FILE
flag set, as specified in [MS-FSCC] section 2.6.

2.2.3  [MS-RSVD] Remote Shared Virtual Disk Protocol

C0010:

Windows 8.1, Windows Server 2012 R2, Windows 10, Windows Server 2016, Windows Server 2019,
Windows Server 2022, Windows 11 and Windows Server 2025

If the server doesn't support the Remote Shared Virtual Disk Protocol [MS-RSVD]:

7 / 10

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024





The operation of configuring a Hyper-V virtual machine to be attached to a remote shared virtual
disk fails.

The operation of starting a Hyper-V virtual machine configured to be attached to a remote shared
virtual disk fails.

If the server doesn't support version 2 of the Remote Shared Virtual Disk Protocol [MS-RSVD]:



The backup operation of a remote shared virtual disk, including checkpoints and resilient change
tracking, fails.



The replica operation of a remote shared virtual disk fails.

  Resizing a remote shared virtual disk fails.

2.2.4  [MS-SQOS] Storage Quality of Service Protocol

C0011:

Windows 10, Windows Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and
Windows Server 2025

If the server doesn't support the Storage Quality of Service Protocol [MS-SQOS], then any storage
Quality of Service policies associated with the virtual disks of a Hyper-V virtual machine are ignored
and never enforced. However, this doesn't prevent the virtual machine from successfully starting.

2.2.5  [MS-FSRVP] File Server Remote VSS Protocol

C0012:

Windows 8, Windows Server 2012, Windows 8.1, Windows Server 2012 R2, Windows 10, Windows
Server 2016, Windows Server 2019, Windows Server 2022, Windows 11 and Windows Server 2025

If the server doesn't support the File Server Remote VSS Protocol [MS-FSRVP]:



The operation of a Hyper-V host–initiated backup of a virtual machine attached to a remote shared
virtual disk fails.

2.3  Error Handling

None.

2.4  Security

None.

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

8 / 10

3  Change Tracking

No table of changes is available. The document is either new or has had no changes since its last
release.

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

9 / 10

4  Index
C

Change tracking 9

G

Glossary 4

I

Informative references 4
Introduction 4

N

Normative references 4

R

References
   informative 4
   normative 4

T

Tracking changes 9

[MS-HVRS] - v20240916
Hyper-V Remote Storage Profile
Copyright © 2024 Microsoft Corporation
Release: September 16, 2024

10 / 10

