[MS-ERREF]:

Windows Error Codes

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

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

1 / 497


Revision Summary

Date

Revision
History

Revision
Class

Comments

2/14/2008

5.0.3

Editorial

Changed language and formatting in the technical content.

3/14/2008

5.1

Minor

Clarified the meaning of the technical content.

5/16/2008

5.1.1

Editorial

Changed language and formatting in the technical content.

6/20/2008

5.2

Minor

Clarified the meaning of the technical content.

7/25/2008

5.2.1

Editorial

Changed language and formatting in the technical content.

8/29/2008

5.2.2

Editorial

Changed language and formatting in the technical content.

10/24/2008  5.2.3

Editorial

Changed language and formatting in the technical content.

12/5/2008

5.3

Minor

Clarified the meaning of the technical content.

1/16/2009

5.3.1

Editorial

Editorial Update.

2/27/2009

5.3.2

Editorial

Changed language and formatting in the technical content.

4/10/2009

6.0

5/22/2009

7.0

7/2/2009

7.1

8/14/2009

8.0

9/25/2009

8.1

11/6/2009

8.2

12/18/2009  9.0

Major

Major

Minor

Major

Minor

Minor

Major

Updated and revised the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

1/29/2010

9.0.1

Editorial

Changed language and formatting in the technical content.

3/12/2010

9.0.2

Editorial

Changed language and formatting in the technical content.

4/23/2010

9.0.3

Editorial

Changed language and formatting in the technical content.

6/4/2010

9.0.4

Editorial

Changed language and formatting in the technical content.

7/16/2010

9.0.4

None

No changes to the meaning, language, or formatting of the
technical content.

8/27/2010

9.0.4

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

9.0.4

11/19/2010  10.0

1/7/2011

11.0

2/11/2011

11.0

None

Major

Major

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

3/25/2011

12.0

Major

Updated and revised the technical content.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

2 / 497


Date

Revision
History

Revision
Class

Comments

5/6/2011

12.0

6/17/2011

12.1

9/23/2011

12.2

12/16/2011  13.0

3/30/2012

13.1

7/12/2012

13.1

None

Minor

Minor

Major

Minor

None

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

No changes to the meaning, language, or formatting of the
technical content.

10/25/2012  13.1

None

No changes to the meaning, language, or formatting of the
technical content.

1/31/2013

13.1

8/8/2013

14.0

11/14/2013  14.1

2/13/2014

14.2

5/15/2014

14.2

None

Major

Minor

Minor

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

15.0

Major

Significantly changed the technical content.

10/16/2015  15.0

7/14/2016

16.0

6/1/2017

17.0

9/15/2017

18.0

12/1/2017

18.0

9/12/2018

19.0

4/7/2021

20.0

6/25/2021

21.0

11/16/2021  22.0

11/19/2024  23.0

None

Major

Major

Major

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

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

3 / 497


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
  - [1.7 Vendor Extensible Fields](#17-vendor-extensible-fields)
- [2 Structures](#2-structures)
  - [2.1 HRESULT](#21-hresult)
    - [2.1.1 HRESULT Values](#211-hresult-values)
    - [2.1.2 HRESULT From WIN32 Error Code Macro](#212-hresult-from-win32-error-code-macro)
  - [2.2 Win32 Error Codes](#22-win32-error-codes)
  - [2.3 NTSTATUS](#23-ntstatus)
    - [2.3.1 NTSTATUS Values](#231-ntstatus-values)
  - [2.4 LDAP Error to Win32 Error Mapping](#24-ldap-error-to-win32-error-mapping)
- [3 Structure Example](#3-structure-example)
- [4 Security Considerations](#4-security-considerations)
- [5 Appendix A: Product Behavior](#5-appendix-a-product-behavior)
- [6 Change Tracking](#6-change-tracking)
- [7 Index](#7-index)

## 1 Introduction

The Windows Error Codes document is a companion reference to the protocol specifications. It
documents the common usage details for those HRESULT values, Win32 error codes, and NTSTATUS
values that are referenced by specifications in the protocol documentation set.

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

### 1.1 Glossary

This document uses the following terms:

HRESULT: An integer value that indicates the result or status of an operation. A particular

HRESULT can have different meanings depending on the protocol using it. See [MS-ERREF]
section 2.1 and specific protocol documents for further details.

terminal server: A computer on which terminal services is running.

terminal services (TS): A service on a server computer that allows delivery of applications, or

the desktop itself, to various computing devices. When a user runs an application on a terminal
server, the application execution takes place on the server computer and only keyboard,
mouse, and display information is transmitted over the network. Each user sees only his or her
individual session, which is managed transparently by the server operating system and is
independent of any other client session.

universally unique identifier (UUID): A 128-bit value. UUIDs can be used for multiple

purposes, from tagging objects with an extremely short lifetime, to reliably identifying very
persistent objects in cross-process communication such as client and server interfaces, manager
entry-point vectors, and RPC objects. UUIDs are highly likely to be unique. UUIDs are also
known as globally unique identifiers (GUIDs) and these terms are used interchangeably in the
Microsoft protocol technical documents (TDs). Interchanging the usage of these terms does not
imply or require a specific algorithm or mechanism to generate the UUID. Specifically, the use of
this term does not imply or require that the algorithms described in [RFC4122] or [C706] has to
be used for generating the UUID.

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

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[RFC2251] Wahl, M., Howes, T., and Kille, S., "Lightweight Directory Access Protocol (v3)", RFC 2251,
December 1997, https://www.rfc-editor.org/info/rfc2251

5 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


#### 1.2.2 Informative References

None.

### 1.3 Overview

If a protocol returns HRESULTs, the protocol uses HRESULTs, as specified in section 2.1.

If a protocol uses Win32 error codes, these values are taken from the Windows error number space,
as specified in section 2.2.

If a protocol uses NTSTATUS values, these values are specified in section 2.3.

### 1.4 Relationship to Protocols and Other Structures

The structures documented in this specification do not depend on any other structures or protocols.

The structures in this document are returned by many protocols.

### 1.5 Applicability Statement

The data types specified in this document are applicable for use in any protocol that needs to include a
discrete set of error codes.

### 1.6 Versioning and Localization

The structures in the Windows Error Codes require no versioning or localization information.

### 1.7 Vendor Extensible Fields

HRESULTs: Vendors can choose their own values, as long as the C bit (0x20000000) is set, indicating
it is a customer code. The structures documented in this specification have no vendor-extensible
fields.

Win32 Error Codes: Vendors can only reuse these values with their indicated meanings.  Choosing any
other value runs the risk of a collision in the future.

NTSTATUS: Vendors can choose their own values for this field, as long as the C bit (0x20000000) is
set, indicating it is a customer code.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

6 / 497


## 2 Structures

### 2.1 HRESULT

The HRESULT numbering space is vendor-extensible. Vendors can supply their own values for this
field, as long as the C bit (0x20000000) is set, indicating it is a customer code.

The HRESULT numbering space has the following internal structure. Any protocol that uses NTSTATUS
values on the wire is responsible for stating the order in which the bytes are placed on the wire.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

S  R  C  N  X

Facility

Code

S (1 bit): Severity. If set, indicates a failure result. If clear, indicates a success result.

R (1 bit): Reserved. If the N bit is clear, this bit MUST be set to 0. If the N bit is set, this bit is

defined by the NTSTATUS numbering space (as specified in section 2.3).

C (1 bit): Customer. This bit specifies if the value is customer-defined or Microsoft-defined. The bit is

set for customer-defined values and clear for Microsoft-defined values.<1>

N (1 bit): If set, indicates that the error code is an NTSTATUS value (as specified in section 2.3),

except that this bit is set.

X (1 bit):  Reserved.  SHOULD be set to 0. <2>

Facility (11 bits): An indicator of the source of the error. New facilities are occasionally added by

Microsoft.

The following table lists the currently defined facility codes:

Value

FACILITY_NULL

0

FACILITY_RPC

1

Meaning

The default facility code.

The source of the error code is an RPC subsystem.

FACILITY_DISPATCH

The source of the error code is a COM Dispatch.

2

FACILITY_STORAGE

The source of the error code is OLE Storage.

3

FACILITY_ITF

4

FACILITY_WIN32

7

The source of the error code is COM/OLE Interface management.

This region is reserved to map undecorated error codes into
HRESULTs.

FACILITY_WINDOWS

The source of the error code is the Windows subsystem.

8

FACILITY_SECURITY

The source of the error code is the Security API layer.

7 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Value

9

FACILITY_SSPI

9

Meaning

The source of the error code is the Security API layer.

FACILITY_CONTROL

The source of the error code is the control mechanism.

10

FACILITY_CERT

11

The source of the error code is a certificate client or server?

FACILITY_INTERNET

The source of the error code is Wininet related.

12

FACILITY_MEDIASERVER

The source of the error code is the Windows Media Server.

13

FACILITY_MSMQ

14

The source of the error code is the Microsoft Message Queue.

FACILITY_SETUPAPI

The source of the error code is the Setup API.

15

FACILITY_SCARD

16

The source of the error code is the Smart-card subsystem.

FACILITY_COMPLUS

The source of the error code is COM+.

17

FACILITY_AAF

18

FACILITY_URT

19

FACILITY_ACS

20

FACILITY_DPLAY

21

FACILITY_UMI

22

FACILITY_SXS

23

The source of the error code is the Microsoft agent.

The source of the error code is .NET CLR.

The source of the error code is the audit collection service.

The source of the error code is Direct Play.

The source of the error code is the ubiquitous memoryintrospection
service.

The source of the error code is Side-by-side servicing.

FACILITY_WINDOWS_CE

The error code is specific to Windows CE.

24

FACILITY_HTTP

25

The source of the error code is HTTP support.

FACILITY_USERMODE_COMMONLOG

The source of the error code is common Logging support.

26

FACILITY_USERMODE_FILTER_MANAGER  The source of the error code is the user mode filter manager.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

8 / 497


Value

31

Meaning

FACILITY_BACKGROUNDCOPY

The source of the error code is background copy control

32

FACILITY_CONFIGURATION

The source of the error code is configuration services.

33

FACILITY_STATE_MANAGEMENT

The source of the error code is state management services.

34

FACILITY_METADIRECTORY

The source of the error code is the Microsoft Identity Server.

35

FACILITY_WINDOWSUPDATE

The source of the error code is a Windows update.

36

FACILITY_DIRECTORYSERVICE

The source of the error code is Active Directory.

37

FACILITY_GRAPHICS

The source of the error code is the graphics drivers.

38

FACILITY_SHELL

39

The source of the error code is the user Shell.

FACILITY_TPM_SERVICES

The source of the error code is the Trusted Platform Module services.

40

FACILITY_TPM_SOFTWARE

41

FACILITY_PLA

48

FACILITY_FVE

49

FACILITY_FWP

50

The source of the error code is the Trusted Platform Module
applications.

The source of the error code is Performance Logs and Alerts

The source of the error code is Full volume encryption.

he source of the error code is the Firewall Platform.

FACILITY_WINRM

The source of the error code is the Windows Resource Manager.

51

FACILITY_NDIS

52

The source of the error code is the Network Driver Interface.

FACILITY_USERMODE_HYPERVISOR

The source of the error code is the Usermode Hypervisor components.

53

FACILITY_CMI

54

The source of the error code is the Configuration Management
Infrastructure.

FACILITY_USERMODE_VIRTUALIZATION

55

The source of the error code is the user mode virtualization
subsystem.

FACILITY_USERMODE_VOLMGR

The source of the error code is  the user mode volume manager

9 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Value

56

FACILITY_BCD

57

Meaning

The source of the error code is the Boot Configuration Database.

FACILITY_USERMODE_VHD

The source of the error code is user mode virtual hard disk support.

58

FACILITY_SDIAG

60

The source of the error code is System Diagnostics.

FACILITY_WEBSERVICES

The source of the error code is the Web Services.

61

FACILITY_WINDOWS_DEFENDER

The source of the error code is a Windows Defender component.

80

FACILITY_OPC

81

The source of the error code is the open connectivity service.

Code (2 bytes): The remainder of the error code.

#### 2.1.1 HRESULT Values

Combining the fields of an HRESULT into a single, 32-bit numbering space, the following HRESULT
values are defined, in addition to those derived from NTSTATUS values (section 2.3.1) and Win32
error codes (section 2.2). This document provides the common usage details of the HRESULTs;
individual protocol specifications provide expanded or modified definitions.

Most values also have a default message defined, which can be used to map the value to a human-
readable text message; when this is done, the HRESULT value is also known as a message identifier.

Note: In the following descriptions, a percentage sign (%) followed by one or more alphanumeric
characters (for example, "%1" or "%hs") indicates a variable that is replaced by text at the time the
value is returned.

Return value/code

0x00030200

STG_S_CONVERTED

0x00030201

STG_S_BLOCK

0x00030202

STG_S_RETRYNOW

0x00030203

STG_S_MONITORING

0x00030204

STG_S_MULTIPLEOPENS

0x00030205

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The underlying file was
converted to compound file
format.

The storage operation should
block until more data is
available.

The storage operation should
retry immediately.

The notified event sink will not
influence the storage operation.

Multiple opens prevent
consolidated (commit
succeeded).

Consolidation of the storage file

10 / 497


Return value/code

STG_S_CONSOLIDATIONFAILED

0x00030206

STG_S_CANNOTCONSOLIDATE

0x00040000

OLE_S_USEREG

0x00040001

OLE_S_STATIC

0x00040002

OLE_S_MAC_CLIPFORMAT

0x00040100

DRAGDROP_S_DROP

0x00040101

DRAGDROP_S_CANCEL

0x00040102

DRAGDROP_S_USEDEFAULTCURSORS

0x00040130

DATA_S_SAMEFORMATETC

0x00040140

VIEW_S_ALREADY_FROZEN

0x00040170

CACHE_S_FORMATETC_NOTSUPPORTED

0x00040171

CACHE_S_SAMECACHE

0x00040172

CACHE_S_SOMECACHES_NOTUPDATED

0x00040180

OLEOBJ_S_INVALIDVERB

0x00040181

OLEOBJ_S_CANNOT_DOVERB_NOW

0x00040182

OLEOBJ_S_INVALIDHWND

0x000401A0

INPLACE_S_TRUNCATED

0x000401C0

CONVERT10_S_NO_PRESENTATION

0x000401E2

MK_S_REDUCED_TO_SELF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

failed (commit succeeded).

Consolidation of the storage file
is inappropriate (commit
succeeded).

Use the registry database to
provide the requested
information.

Success, but static.

Macintosh clipboard format.

Successful drop took place.

Drag-drop operation canceled.

Use the default cursor.

Data has same FORMATETC.

View is already frozen.

FORMATETC not supported.

Same cache.

Some caches are not updated.

Invalid verb for OLE object.

Verb number is valid but verb
cannot be done now.

Invalid window handle passed.

Message is too long; some of it
had to be truncated before
displaying.

Unable to convert OLESTREAM
to IStorage.

Moniker reduced to itself.

11 / 497


Return value/code

Description

0x000401E4

MK_S_ME

0x000401E5

MK_S_HIM

0x000401E6

MK_S_US

0x000401E7

MK_S_MONIKERALREADYREGISTERED

0x00040200

EVENT_S_SOME_SUBSCRIBERS_FAILED

0x00040202

EVENT_S_NOSUBSCRIBERS

0x00041300

SCHED_S_TASK_READY

0x00041301

SCHED_S_TASK_RUNNING

0x00041302

SCHED_S_TASK_DISABLED

0x00041303

SCHED_S_TASK_HAS_NOT_RUN

0x00041304

SCHED_S_TASK_NO_MORE_RUNS

0x00041305

SCHED_S_TASK_NOT_SCHEDULED

0x00041306

SCHED_S_TASK_TERMINATED

0x00041307

SCHED_S_TASK_NO_VALID_TRIGGERS

0x00041308

SCHED_S_EVENT_TRIGGER

0x0004131B

SCHED_S_SOME_TRIGGERS_FAILED

0x0004131C

SCHED_S_BATCH_LOGON_PROBLEM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Common prefix is this moniker.

Common prefix is input
moniker.

Common prefix is both
monikers.

Moniker is already registered in
running object table.

An event was able to invoke
some, but not all, of the
subscribers.

An event was delivered, but
there were no subscribers.

The task is ready to run at its
next scheduled time.

The task is currently running.

The task will not run at the
scheduled times because it has
been disabled.

The task has not yet run.

There are no more runs
scheduled for this task.

One or more of the properties
that are needed to run this task
on a schedule have not been
set.

The last run of the task was
terminated by the user.

Either the task has no triggers,
or the existing triggers are
disabled or not set.

Event triggers do not have set
run times.

The task is registered, but not
all specified triggers will start
the task.

The task is registered, but it
might fail to start. Batch logon
privilege needs to be enabled
for the task principal.

12 / 497


Return value/code

0x0004D000

XACT_S_ASYNC

0x0004D002

XACT_S_READONLY

0x0004D003

XACT_S_SOMENORETAIN

0x0004D004

XACT_S_OKINFORM

0x0004D005

XACT_S_MADECHANGESCONTENT

0x0004D006

XACT_S_MADECHANGESINFORM

0x0004D007

XACT_S_ALLNORETAIN

0x0004D008

XACT_S_ABORTING

0x0004D009

XACT_S_SINGLEPHASE

0x0004D00A

XACT_S_LOCALLY_OK

0x0004D010

XACT_S_LASTRESOURCEMANAGER

0x00080012

CO_S_NOTALLINTERFACES

0x00080013

CO_S_MACHINENAMENOTFOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An asynchronous operation was
specified. The operation has
begun, but its outcome is not
known yet.

The method call succeeded
because the transaction was
read-only.

The transaction was
successfully aborted. However,
this is a coordinated
transaction, and a number of
enlisted resources were aborted
outright because they could not
support abort-retaining
semantics.

No changes were made during
this call, but the sink wants
another chance to look if any
other sinks make further
changes.

The sink is content and wants
the transaction to proceed.
Changes were made to one or
more resources during this call.

The sink is for the moment and
wants the transaction to
proceed, but if other changes
are made following this return
by other event sinks, this sink
wants another chance to look.

The transaction was
successfully aborted. However,
the abort was nonretaining.

An abort operation was already
in progress.

The resource manager has
performed a single-phase
commit of the transaction.

The local transaction has not
aborted.

The resource manager has
requested to be the coordinator
(last resource manager) for the
transaction.

Not all the requested interfaces
were available.

The specified machine name
was not found in the cache.

13 / 497


Return value/code

0x00090312

SEC_I_CONTINUE_NEEDED

0x00090313

SEC_I_COMPLETE_NEEDED

0x00090314

SEC_I_COMPLETE_AND_CONTINUE

0x00090315

SEC_I_LOCAL_LOGON

0x00090317

SEC_I_CONTEXT_EXPIRED

0x00090320

SEC_I_INCOMPLETE_CREDENTIALS

0x00090321

SEC_I_RENEGOTIATE

0x00090323

SEC_I_NO_LSA_CONTEXT

0x0009035C

SEC_I_SIGNATURE_NEEDED

0x00091012

CRYPT_I_NEW_PROTECTION_REQUIRED

0x000D0000

NS_S_CALLPENDING

0x000D0001

NS_S_CALLABORTED

0x000D0002

NS_S_STREAM_TRUNCATED

0x000D0BC8

NS_S_REBUFFERING

0x000D0BC9

NS_S_DEGRADING_QUALITY

0x000D0BDB

NS_S_TRANSCRYPTOR_EOF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The function completed
successfully, but it must be
called again to complete the
context.

The function completed
successfully, but
CompleteToken must be called.

The function completed
successfully, but both
CompleteToken and this
function must be called to
complete the context.

The logon was completed, but
no network authority was
available. The logon was made
using locally known information.

The context has expired and
can no longer be used.

The credentials supplied were
not complete and could not be
verified. Additional information
can be returned from the
context.

The context data must be
renegotiated with the peer.

There is no LSA mode context
associated with this context.

A signature operation must be
performed before the user can
authenticate.

The protected data needs to be
reprotected.

The requested operation is
pending completion.

The requested operation was
aborted by the client.

The stream was purposefully
stopped before completion.

The requested operation has
caused the source to rebuffer.

The requested operation has
caused the source to degrade
codec quality.

The transcryptor object has
reached end of file.

14 / 497


Return value/code

0x000D0FE8

NS_S_WMP_UI_VERSIONMISMATCH

0x000D0FE9

NS_S_WMP_EXCEPTION

0x000D1040

NS_S_WMP_LOADED_GIF_IMAGE

0x000D1041

NS_S_WMP_LOADED_PNG_IMAGE

0x000D1042

NS_S_WMP_LOADED_BMP_IMAGE

0x000D1043

NS_S_WMP_LOADED_JPG_IMAGE

0x000D104F

NS_S_WMG_FORCE_DROP_FRAME

0x000D105F

NS_S_WMR_ALREADYRENDERED

0x000D1060

NS_S_WMR_PINTYPEPARTIALMATCH

0x000D1061

NS_S_WMR_PINTYPEFULLMATCH

0x000D1066

NS_S_WMG_ADVISE_DROP_FRAME

0x000D1067

NS_S_WMG_ADVISE_DROP_TO_KEYFRAME

0x000D10DB

NS_S_NEED_TO_BUY_BURN_RIGHTS

0x000D10FE

NS_S_WMPCORE_PLAYLISTCLEARABORT

0x000D10FF

NS_S_WMPCORE_PLAYLISTREMOVEITEMABORT

0x000D1102

NS_S_WMPCORE_PLAYLIST_CREATION_PENDING

0x000D1103

NS_S_WMPCORE_MEDIA_VALIDATION_PENDING

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An upgrade is needed for the
theme manager to correctly
show this skin. Skin reports
version: %.1f.

An error occurred in one of the
UI components.

Successfully loaded a GIF file.

Successfully loaded a PNG file.

Successfully loaded a BMP file.

Successfully loaded a JPG file.

Drop this frame.

The specified stream has
already been rendered.

The specified type partially
matches this pin type.

The specified type fully matches
this pin type.

The timestamp is late compared
to the current render position.
Advise dropping this frame.

The timestamp is severely late
compared to the current render
position. Advise dropping
everything up to the next key
frame.

No burn rights. You will be
prompted to buy burn rights
when you try to burn this file to
an audio CD.

Failed to clear playlist because
it was aborted by user.

Failed to remove item in the
playlist since it was aborted by
user.

Playlist is being generated
asynchronously.

Validation of the media is
pending.

15 / 497


Return value/code

0x000D1104

NS_S_WMPCORE_PLAYLIST_REPEAT_SECONDARY_SEGMENTS_IGNORED

0x000D1105

NS_S_WMPCORE_COMMAND_NOT_AVAILABLE

0x000D1106

NS_S_WMPCORE_PLAYLIST_NAME_AUTO_GENERATED

0x000D1107

NS_S_WMPCORE_PLAYLIST_IMPORT_MISSING_ITEMS

0x000D1108

NS_S_WMPCORE_PLAYLIST_COLLAPSED_TO_SINGLE_MEDIA

0x000D1109

NS_S_WMPCORE_MEDIA_CHILD_PLAYLIST_OPEN_PENDING

0x000D110A

NS_S_WMPCORE_MORE_NODES_AVAIABLE

0x000D1135

NS_S_WMPBR_SUCCESS

0x000D1136

NS_S_WMPBR_PARTIALSUCCESS

0x000D1144

NS_S_WMPEFFECT_TRANSPARENT

0x000D1145

NS_S_WMPEFFECT_OPAQUE

0x000D114E

NS_S_OPERATION_PENDING

0x000D1359

NS_S_TRACK_BUY_REQUIRES_ALBUM_PURCHASE

0x000D135E

NS_S_NAVIGATION_COMPLETE_WITH_ERRORS

0x000D1361

NS_S_TRACK_ALREADY_DOWNLOADED

0x000D1519

NS_S_PUBLISHING_POINT_STARTED_WITH_FAILED_SINKS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Encountered more than one
Repeat block during ASX
processing.

Current state of WMP disallows
calling this method or property.

Name for the playlist has been
auto generated.

The imported playlist does not
contain all items from the
original.

The M3U playlist has been
ignored because it only contains
one item.

The open for the child playlist
associated with this media is
pending.

More nodes support the
interface requested, but the
array for returning them is full.

Backup or Restore successful!.

Transfer complete with
limitations.

Request to the effects control to
change transparency status to
transparent.

Request to the effects control to
change transparency status to
opaque.

The requested application pane
is performing an operation and
will not be released.

The file is only available for
purchase when you buy the
entire album.

There were problems
completing the requested
navigation. There are identifiers
missing in the catalog.

Track already downloaded.

The publishing point
successfully started, but one or
more of the requested data
writer plug-ins failed.

16 / 497


Return value/code

0x000D2726

NS_S_DRM_LICENSE_ACQUIRED

0x000D2727

NS_S_DRM_INDIVIDUALIZED

0x000D2746

NS_S_DRM_MONITOR_CANCELLED

0x000D2747

NS_S_DRM_ACQUIRE_CANCELLED

0x000D276E

NS_S_DRM_BURNABLE_TRACK

0x000D276F

NS_S_DRM_BURNABLE_TRACK_WITH_PLAYLIST_RESTRICTION

0x000D27DE

NS_S_DRM_NEEDS_INDIVIDUALIZATION

0x000D2AF8

NS_S_REBOOT_RECOMMENDED

0x000D2AF9

NS_S_REBOOT_REQUIRED

0x000D2F09

NS_S_EOSRECEDING

0x000D2F0D

NS_S_CHANGENOTICE

0x001F0001

ERROR_FLT_IO_COMPLETE

0x00262307

ERROR_GRAPHICS_MODE_NOT_PINNED

0x0026231E

ERROR_GRAPHICS_NO_PREFERRED_MODE

0x0026234B

ERROR_GRAPHICS_DATASET_IS_EMPTY

0x0026234C

ERROR_GRAPHICS_NO_MORE_ELEMENTS_IN_DATASET

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Status message: The license
was acquired.

Status message: The security
upgrade has been completed.

Status message: License
monitoring has been canceled.

Status message: License
acquisition has been canceled.

The track is burnable and had
no playlist burn limit.

The track is burnable but has a
playlist burn limit.

A security upgrade is required
to perform the operation on this
media file.

Installation was successful;
however, some file cleanup is
not complete. For best results,
restart your computer.

Installation was successful;
however, some file cleanup is
not complete. To continue, you
must restart your computer.

EOS hit during rewinding.

Internal.

The IO was completed by a
filter.

No mode is pinned on the
specified VidPN source or
target.

Specified mode set does not
specify preference for one of its
modes.

Specified data set (for example,
mode set, frequency range set,
descriptor set, and topology) is
empty.

Specified data set (for example,
mode set, frequency range set,
descriptor set, and topology)
does not contain any more
elements.

17 / 497


Return value/code

0x00262351

ERROR_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATION_NOT_PIN
NED

0x00300100

PLA_S_PROPERTY_IGNORED

0x00340001

ERROR_NDIS_INDICATION_REQUIRED

0x0DEAD100

TRK_S_OUT_OF_SYNC

0x0DEAD102

TRK_VOLUME_NOT_FOUND

0x0DEAD103

TRK_VOLUME_NOT_OWNED

0x0DEAD107

TRK_S_NOTIFICATION_QUOTA_EXCEEDED

0x400D004F

NS_I_TIGER_START

0x400D0051

NS_I_CUB_START

0x400D0052

NS_I_CUB_RUNNING

0x400D0054

NS_I_DISK_START

0x400D0056

NS_I_DISK_REBUILD_STARTED

0x400D0057

NS_I_DISK_REBUILD_FINISHED

0x400D0058

NS_I_DISK_REBUILD_ABORTED

0x400D0059

NS_I_LIMIT_FUNNELS

0x400D005A

NS_I_START_DISK

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Specified content
transformation is not pinned on
the specified VidPN present
path.

Property value will be ignored.

The request will be completed
later by a Network Driver
Interface Specification (NDIS)
status indication.

The VolumeSequenceNumber of
a MOVE_NOTIFICATION request
is incorrect.

The VolumeID in a request was
not found in the server's
ServerVolumeTable.

A notification was sent to the
LnkSvrMessage method, but the
RequestMachine for the request
was not the VolumeOwner for a
VolumeID in the request.

The server received a
MOVE_NOTIFICATION request,
but the FileTable size limit has
already been reached.

The Title Server %1 is running.

Content Server %1 (%2) is
starting.

Content Server %1 (%2) is
running.

Disk %1 ( %2 ) on Content
Server %3, is running.

Started rebuilding disk %1 ( %2
) on Content Server %3.

Finished rebuilding disk %1 (
%2 ) on Content Server %3.

Aborted rebuilding disk %1 (
%2 ) on Content Server %3.

A NetShow administrator at
network location %1 set the
data stream limit to %2
streams.

A NetShow administrator at
network location %1 started

18 / 497


Return value/code

0x400D005B

NS_I_STOP_DISK

0x400D005C

NS_I_STOP_CUB

0x400D005D

NS_I_KILL_USERSESSION

0x400D005E

NS_I_KILL_CONNECTION

0x400D005F

NS_I_REBUILD_DISK

0x400D0069

MCMADM_I_NO_EVENTS

0x400D006E

NS_I_LOGGING_FAILED

0x400D0070

NS_I_LIMIT_BANDWIDTH

0x400D0191

NS_I_CUB_UNFAIL_LINK

0x400D0193

NS_I_RESTRIPE_START

0x400D0194

NS_I_RESTRIPE_DONE

0x400D0196

NS_I_RESTRIPE_DISK_OUT

0x400D0197

NS_I_RESTRIPE_CUB_OUT

0x400D0198

NS_I_DISK_STOP

0x400D14BE

NS_I_PLAYLIST_CHANGE_RECEDING

0x400D2EFF

NS_I_RECONNECTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

disk %2.

A NetShow administrator at
network location %1 stopped
disk %2.

A NetShow administrator at
network location %1 stopped
Content Server %2.

A NetShow administrator at
network location %1 aborted
user session %2 from the
system.

A NetShow administrator at
network location %1 aborted
obsolete connection %2 from
the system.

A NetShow administrator at
network location %1 started
rebuilding disk %2.

Event initialization failed, there
will be no MCM events.

The logging operation failed.

A NetShow administrator at
network location %1 set the
maximum bandwidth limit to
%2 bps.

Content Server %1 (%2) has
established its link to Content
Server %3.

Restripe operation has started.

Restripe operation has
completed.

Content disk %1 (%2) on
Content Server %3 has been
restriped out.

Content server %1 (%2) has
been restriped out.

Disk %1 ( %2 ) on Content
Server %3, has been offlined.

The playlist change occurred
while receding.

The client is reconnected.

19 / 497


Return value/code

0x400D2F01

NS_I_NOLOG_STOP

0x400D2F03

NS_I_EXISTING_PACKETIZER

0x400D2F04

NS_I_MANUAL_PROXY

0x40262009

ERROR_GRAPHICS_DRIVER_MISMATCH

0x4026242F

ERROR_GRAPHICS_UNKNOWN_CHILD_STATUS

0x40262437

ERROR_GRAPHICS_LEADLINK_START_DEFERRED

0x40262439

ERROR_GRAPHICS_POLLING_TOO_FREQUENTLY

0x4026243A

ERROR_GRAPHICS_START_DEFERRED

0x8000000A

E_PENDING

0x80004001

E_NOTIMPL

0x80004002

E_NOINTERFACE

0x80004003

E_POINTER

0x80004004

E_ABORT

0x80004005

E_FAIL

0x80004006

CO_E_INIT_TLS

0x80004007

CO_E_INIT_SHARED_ALLOCATOR

0x80004008

CO_E_INIT_MEMORY_ALLOCATOR

0x80004009

CO_E_INIT_CLASS_CACHE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Forcing a switch to a pending
header on start.

There is already an existing
packetizer plugin for the
stream.

The proxy setting is manual.

The kernel driver detected a
version mismatch between it
and the user mode driver.

Child device presence was not
reliably detected.

Starting the lead-link adapter
has been deferred temporarily.

The display adapter is being
polled for children too
frequently at the same polling
level.

Starting the adapter has been
deferred temporarily.

The data necessary to complete
this operation is not yet
available.

Not implemented.

No such interface supported.

Invalid pointer.

Operation aborted.

Unspecified error.

Thread local storage failure.

Get shared memory allocator
failure.

Get memory allocator failure.

Unable to initialize class cache.

20 / 497


Return value/code

0x8000400A

CO_E_INIT_RPC_CHANNEL

0x8000400B

CO_E_INIT_TLS_SET_CHANNEL_CONTROL

0x8000400C

CO_E_INIT_TLS_CHANNEL_CONTROL

0x8000400D

CO_E_INIT_UNACCEPTED_USER_ALLOCATOR

0x8000400E

CO_E_INIT_SCM_MUTEX_EXISTS

0x8000400F

CO_E_INIT_SCM_FILE_MAPPING_EXISTS

0x80004010

CO_E_INIT_SCM_MAP_VIEW_OF_FILE

0x80004011

CO_E_INIT_SCM_EXEC_FAILURE

0x80004012

CO_E_INIT_ONLY_SINGLE_THREADED

0x80004013

CO_E_CANT_REMOTE

0x80004014

CO_E_BAD_SERVER_NAME

0x80004015

CO_E_WRONG_SERVER_IDENTITY

0x80004016

CO_E_OLE1DDE_DISABLED

0x80004017

CO_E_RUNAS_SYNTAX

0x80004018

CO_E_CREATEPROCESS_FAILURE

0x80004019

CO_E_RUNAS_CREATEPROCESS_FAILURE

0x8000401A

CO_E_RUNAS_LOGON_FAILURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unable to initialize remote
procedure call (RPC) services.

Cannot set thread local storage
channel control.

Could not allocate thread local
storage channel control.

The user-supplied memory
allocator is unacceptable.

The OLE service mutex already
exists.

The OLE service file mapping
already exists.

Unable to map view of file for
OLE service.

Failure attempting to launch
OLE service.

There was an attempt to call
CoInitialize a second time while
single-threaded.

A Remote activation was
necessary but was not allowed.

A Remote activation was
necessary, but the server name
provided was invalid.

The class is configured to run as
a security ID different from the
caller.

Use of OLE1 services requiring
Dynamic Data Exchange (DDE)
Windows is disabled.

A RunAs specification must be
<domain name>\<user name>
or simply <user name>.

The server process could not be
started. The path name might
be incorrect.

The server process could not be
started as the configured
identity. The path name might
be incorrect or unavailable.

The server process could not be
started because the configured
identity is incorrect. Check the
user name and password.

21 / 497


Return value/code

0x8000401B

CO_E_LAUNCH_PERMSSION_DENIED

0x8000401C

CO_E_START_SERVICE_FAILURE

0x8000401D

CO_E_REMOTE_COMMUNICATION_FAILURE

0x8000401E

CO_E_SERVER_START_TIMEOUT

0x8000401F

CO_E_CLSREG_INCONSISTENT

0x80004020

CO_E_IIDREG_INCONSISTENT

0x80004021

CO_E_NOT_SUPPORTED

0x80004022

CO_E_RELOAD_DLL

0x80004023

CO_E_MSI_ERROR

0x80004024

CO_E_ATTEMPT_TO_CREATE_OUTSIDE_CLIENT_CONTEXT

0x80004025

CO_E_SERVER_PAUSED

0x80004026

CO_E_SERVER_NOT_PAUSED

0x80004027

CO_E_CLASS_DISABLED

0x80004028

CO_E_CLRNOTAVAILABLE

0x80004029

CO_E_ASYNC_WORK_REJECTED

0x8000402A

CO_E_SERVER_INIT_TIMEOUT

0x8000402B

CO_E_NO_SECCTX_IN_ACTIVATE

0x80004030

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The client is not allowed to
launch this server.

The service providing this
server could not be started.

This computer was unable to
communicate with the computer
providing the server.

The server did not respond after
being launched.

The registration information for
this server is inconsistent or
incomplete.

The registration information for
this interface is inconsistent or
incomplete.

The operation attempted is not
supported.

A DLL must be loaded.

A Microsoft Software Installer
error was encountered.

The specified activation could
not occur in the client context
as specified.

Activations on the server are
paused.

Activations on the server are
not paused.

The component or application
containing the component has
been disabled.

The common language runtime
is not available.

The thread-pool rejected the
submitted asynchronous work.

The server started, but it did
not finish initializing in a timely
fashion.

Unable to complete the call
because there is no COM+
security context inside
IObjectControl.Activate.

The provided tracker

22 / 497


Return value/code

CO_E_TRACKER_CONFIG

0x80004031

CO_E_THREADPOOL_CONFIG

0x80004032

CO_E_SXS_CONFIG

0x80004033

CO_E_MALFORMED_SPN

0x8000FFFF

E_UNEXPECTED

0x80010001

RPC_E_CALL_REJECTED

0x80010002

RPC_E_CALL_CANCELED

0x80010003

RPC_E_CANTPOST_INSENDCALL

0x80010004

RPC_E_CANTCALLOUT_INASYNCCALL

0x80010005

RPC_E_CANTCALLOUT_INEXTERNALCALL

0x80010006

RPC_E_CONNECTION_TERMINATED

0x80010007

RPC_E_SERVER_DIED

0x80010008

RPC_E_CLIENT_DIED

0x80010009

RPC_E_INVALID_DATAPACKET

0x8001000A

RPC_E_CANTTRANSMIT_CALL

0x8001000B

RPC_E_CLIENT_CANTMARSHAL_DATA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

configuration is invalid.

The provided thread pool
configuration is invalid.

The provided side-by-side
configuration is invalid.

The server principal name
(SPN) obtained during security
negotiation is malformed.

Catastrophic failure.

Call was rejected by callee.

Call was canceled by the
message filter.

The caller is dispatching an
intertask SendMessage call and
cannot call out via PostMessage.

The caller is dispatching an
asynchronous call and cannot
make an outgoing call on behalf
of this call.

It is illegal to call out while
inside message filter.

The connection terminated or is
in a bogus state and can no
longer be used. Other
connections are still valid.

The callee (the server, not the
server application) is not
available and disappeared; all
connections are invalid. The call
might have executed.

The caller (client) disappeared
while the callee (server) was
processing a call.

The data packet with the
marshaled parameter data is
incorrect.

The call was not transmitted
properly; the message queue
was full and was not emptied
after yielding.

The client RPC caller cannot
marshal the parameter data due
to errors (such as low memory).

23 / 497


Return value/code

0x8001000C

RPC_E_CLIENT_CANTUNMARSHAL_DATA

0x8001000D

RPC_E_SERVER_CANTMARSHAL_DATA

0x8001000E

RPC_E_SERVER_CANTUNMARSHAL_DATA

0x8001000F

RPC_E_INVALID_DATA

0x80010010

RPC_E_INVALID_PARAMETER

0x80010011

RPC_E_CANTCALLOUT_AGAIN

0x80010012

RPC_E_SERVER_DIED_DNE

0x80010100

RPC_E_SYS_CALL_FAILED

0x80010101

RPC_E_OUT_OF_RESOURCES

0x80010102

RPC_E_ATTEMPTED_MULTITHREAD

0x80010103

RPC_E_NOT_REGISTERED

0x80010104

RPC_E_FAULT

0x80010105

RPC_E_SERVERFAULT

0x80010106

RPC_E_CHANGED_MODE

0x80010107

RPC_E_INVALIDMETHOD

0x80010108

RPC_E_DISCONNECTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The client RPC caller cannot
unmarshal the return data due
to errors (such as low memory).

The server RPC callee cannot
marshal the return data due to
errors (such as low memory).

The server RPC callee cannot
unmarshal the parameter data
due to errors (such as low
memory).

Received data is invalid. The
data might be server or client
data.

A particular parameter is invalid
and cannot be (un)marshaled.

There is no second outgoing call
on same channel in DDE
conversation.

The callee (the server, not the
server application) is not
available and disappeared; all
connections are invalid. The call
did not execute.

System call failed.

Could not allocate some
required resource (such as
memory or events)

Attempted to make calls on
more than one thread in single-
threaded mode.

The requested interface is not
registered on the server object.

RPC could not call the server or
could not return the results of
calling the server.

The server threw an exception.

Cannot change thread mode
after it is set.

The method called does not
exist on the server.

The object invoked has
disconnected from its clients.

24 / 497


Return value/code

Description

0x80010109

RPC_E_RETRY

0x8001010A

RPC_E_SERVERCALL_RETRYLATER

0x8001010B

RPC_E_SERVERCALL_REJECTED

0x8001010C

RPC_E_INVALID_CALLDATA

0x8001010D

RPC_E_CANTCALLOUT_ININPUTSYNCCALL

0x8001010E

RPC_E_WRONG_THREAD

0x8001010F

RPC_E_THREAD_NOT_INIT

0x80010110

RPC_E_VERSION_MISMATCH

0x80010111

RPC_E_INVALID_HEADER

0x80010112

RPC_E_INVALID_EXTENSION

0x80010113

RPC_E_INVALID_IPID

0x80010114

RPC_E_INVALID_OBJECT

0x80010115

RPC_S_CALLPENDING

0x80010116

RPC_S_WAITONTIMER

0x80010117

RPC_E_CALL_COMPLETE

0x80010118

RPC_E_UNSECURE_CALL

0x80010119

RPC_E_TOO_LATE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The object invoked chose not to
process the call now. Try again
later.

The message filter indicated
that the application is busy.

The message filter rejected the
call.

A call control interface was
called with invalid data.

An outgoing call cannot be
made because the application is
dispatching an input-
synchronous call.

The application called an
interface that was marshaled
for a different thread.

CoInitialize has not been called
on the current thread.

The version of OLE on the client
and server machines does not
match.

OLE received a packet with an
invalid header.

OLE received a packet with an
invalid extension.

The requested object or
interface does not exist.

The requested object does not
exist.

OLE has sent a request and is
waiting for a reply.

OLE is waiting before retrying a
request.

Call context cannot be accessed
after call completed.

Impersonate on unsecure calls
is not supported.

Security must be initialized
before any interfaces are
marshaled or unmarshaled. It
cannot be changed after
initialized.

25 / 497


Return value/code

0x8001011A

RPC_E_NO_GOOD_SECURITY_PACKAGES

0x8001011B

RPC_E_ACCESS_DENIED

0x8001011C

RPC_E_REMOTE_DISABLED

0x8001011D

RPC_E_INVALID_OBJREF

0x8001011E

RPC_E_NO_CONTEXT

0x8001011F

RPC_E_TIMEOUT

0x80010120

RPC_E_NO_SYNC

0x80010121

RPC_E_FULLSIC_REQUIRED

0x80010122

RPC_E_INVALID_STD_NAME

0x80010123

CO_E_FAILEDTOIMPERSONATE

0x80010124

CO_E_FAILEDTOGETSECCTX

0x80010125

CO_E_FAILEDTOOPENTHREADTOKEN

0x80010126

CO_E_FAILEDTOGETTOKENINFO

0x80010127

CO_E_TRUSTEEDOESNTMATCHCLIENT

0x80010128

CO_E_FAILEDTOQUERYCLIENTBLANKET

0x80010129

CO_E_FAILEDTOSETDACL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

No security packages are
installed on this machine, the
user is not logged on, or there
are no compatible security
packages between the client
and server.

Access is denied.

Remote calls are not allowed for
this process.

The marshaled interface data
packet (OBJREF) has an invalid
or unknown format.

No context is associated with
this call. This happens for some
custom marshaled calls and on
the client side of the call.

This operation returned because
the time-out period expired.

There are no synchronize
objects to wait on.

Full subject issuer chain Secure
Sockets Layer (SSL) principal
name expected from the server.

Principal name is not a valid
Microsoft standard (msstd)
name.

Unable to impersonate DCOM
client.

Unable to obtain server's
security context.

Unable to open the access
token of the current thread.

Unable to obtain user
information from an access
token.

The client who called
IAccessControl::IsAccessPermitt
ed was not the trustee provided
to the method.

Unable to obtain the client's
security blanket.

Unable to set a discretionary
access control list (ACL) into a
security descriptor.

26 / 497


Return value/code

0x8001012A

CO_E_ACCESSCHECKFAILED

0x8001012B

CO_E_NETACCESSAPIFAILED

0x8001012C

CO_E_WRONGTRUSTEENAMESYNTAX

0x8001012D

CO_E_INVALIDSID

0x8001012E

CO_E_CONVERSIONFAILED

0x8001012F

CO_E_NOMATCHINGSIDFOUND

0x80010130

CO_E_LOOKUPACCSIDFAILED

0x80010131

CO_E_NOMATCHINGNAMEFOUND

0x80010132

CO_E_LOOKUPACCNAMEFAILED

0x80010133

CO_E_SETSERLHNDLFAILED

0x80010134

CO_E_FAILEDTOGETWINDIR

0x80010135

CO_E_PATHTOOLONG

0x80010136

CO_E_FAILEDTOGENUUID

0x80010137

CO_E_FAILEDTOCREATEFILE

0x80010138

CO_E_FAILEDTOCLOSEHANDLE

0x80010139

CO_E_EXCEEDSYSACLLIMIT

0x8001013A

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system function
AccessCheck returned false.

Either NetAccessDel or
NetAccessAdd returned an error
code.

One of the trustee strings
provided by the user did not
conform to the
<Domain>\<Name> syntax
and it was not the *" string".

One of the security identifiers
provided by the user was
invalid.

Unable to convert a wide
character trustee string to a
multiple-byte trustee string.

Unable to find a security
identifier that corresponds to a
trustee string provided by the
user.

The system function
LookupAccountSID failed.

Unable to find a trustee name
that corresponds to a security
identifier provided by the user.

The system function
LookupAccountName failed.

Unable to set or reset a
serialization handle.

Unable to obtain the Windows
directory.

Path too long.

Unable to generate a UUID.

Unable to create file.

Unable to close a serialization
handle or a file handle.

The number of access control
entries (ACEs) in an ACL
exceeds the system limit.

Not all the DENY_ACCESS ACEs
are arranged in front of the

27 / 497


Return value/code

CO_E_ACESINWRONGORDER

0x8001013B

CO_E_INCOMPATIBLESTREAMVERSION

0x8001013C

CO_E_FAILEDTOOPENPROCESSTOKEN

0x8001013D

CO_E_DECODEFAILED

0x8001013F

CO_E_ACNOTINITIALIZED

0x80010140

CO_E_CANCEL_DISABLED

0x8001FFFF

RPC_E_UNEXPECTED

0x80020001

DISP_E_UNKNOWNINTERFACE

0x80020003

DISP_E_MEMBERNOTFOUND

0x80020004

DISP_E_PARAMNOTFOUND

0x80020005

DISP_E_TYPEMISMATCH

0x80020006

DISP_E_UNKNOWNNAME

0x80020007

DISP_E_NONAMEDARGS

0x80020008

DISP_E_BADVARTYPE

0x80020009

DISP_E_EXCEPTION

0x8002000A

DISP_E_OVERFLOW

0x8002000B

DISP_E_BADINDEX

0x8002000C

DISP_E_UNKNOWNLCID

0x8002000D

DISP_E_ARRAYISLOCKED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

GRANT_ACCESS ACEs in the
stream.

The version of ACL format in
the stream is not supported by
this implementation of
IAccessControl.

Unable to open the access
token of the server process.

Unable to decode the ACL in the
stream provided by the user.

The COM IAccessControl object
is not initialized.

Call Cancellation is disabled.

An internal error occurred.

Unknown interface.

Member not found.

Parameter not found.

Type mismatch.

Unknown name.

No named arguments.

Bad variable type.

Exception occurred.

Out of present range.

Invalid index.

Unknown language.

Memory is locked.

28 / 497


Return value/code

0x8002000E

DISP_E_BADPARAMCOUNT

0x8002000F

DISP_E_PARAMNOTOPTIONAL

0x80020010

DISP_E_BADCALLEE

0x80020011

DISP_E_NOTACOLLECTION

0x80020012

DISP_E_DIVBYZERO

0x80020013

DISP_E_BUFFERTOOSMALL

0x80028016

TYPE_E_BUFFERTOOSMALL

0x80028017

TYPE_E_FIELDNOTFOUND

0x80028018

TYPE_E_INVDATAREAD

0x80028019

TYPE_E_UNSUPFORMAT

0x8002801C

TYPE_E_REGISTRYACCESS

0x8002801D

TYPE_E_LIBNOTREGISTERED

0x80028027

TYPE_E_UNDEFINEDTYPE

0x80028028

TYPE_E_QUALIFIEDNAMEDISALLOWED

0x80028029

TYPE_E_INVALIDSTATE

0x8002802A

TYPE_E_WRONGTYPEKIND

0x8002802B

TYPE_E_ELEMENTNOTFOUND

0x8002802C

TYPE_E_AMBIGUOUSNAME

0x8002802D

TYPE_E_NAMECONFLICT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Invalid number of parameters.

Parameter not optional.

Invalid callee.

Does not support a collection.

Division by zero.

Buffer too small.

Buffer too small.

Field name not defined in the
record.

Old format or invalid type
library.

Old format or invalid type
library.

Error accessing the OLE
registry.

Library not registered.

Bound to unknown type.

Qualified name disallowed.

Invalid forward reference, or
reference to uncompiled type.

Type mismatch.

Element not found.

Ambiguous name.

Name already exists in the
library.

29 / 497


Return value/code

0x8002802E

TYPE_E_UNKNOWNLCID

0x8002802F

TYPE_E_DLLFUNCTIONNOTFOUND

0x800288BD

TYPE_E_BADMODULEKIND

0x800288C5

TYPE_E_SIZETOOBIG

0x800288C6

TYPE_E_DUPLICATEID

0x800288CF

TYPE_E_INVALIDID

0x80028CA0

TYPE_E_TYPEMISMATCH

0x80028CA1

TYPE_E_OUTOFBOUNDS

0x80028CA2

TYPE_E_IOERROR

0x80028CA3

TYPE_E_CANTCREATETMPFILE

0x80029C4A

TYPE_E_CANTLOADLIBRARY

0x80029C83

TYPE_E_INCONSISTENTPROPFUNCS

0x80029C84

TYPE_E_CIRCULARTYPE

0x80030001

STG_E_INVALIDFUNCTION

0x80030002

STG_E_FILENOTFOUND

0x80030003

STG_E_PATHNOTFOUND

0x80030004

STG_E_TOOMANYOPENFILES

0x80030005

STG_E_ACCESSDENIED

0x80030006

STG_E_INVALIDHANDLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unknown language code
identifier (LCID).

Function not defined in specified
DLL.

Wrong module kind for the
operation.

Size cannot exceed 64 KB.

Duplicate ID in inheritance
hierarchy.

Incorrect inheritance depth in
standard OLE hmember.

Type mismatch.

Invalid number of arguments.

I/O error.

Error creating unique .tmp file.

Error loading type library or
DLL.

Inconsistent property functions.

Circular dependency between
types and modules.

Unable to perform requested
operation.

%1 could not be found.

The path %1 could not be
found.

There are insufficient resources
to open another file.

Access denied.

Attempted an operation on an
invalid object.

30 / 497


Return value/code

0x80030008

STG_E_INSUFFICIENTMEMORY

0x80030009

STG_E_INVALIDPOINTER

0x80030012

STG_E_NOMOREFILES

0x80030013

STG_E_DISKISWRITEPROTECTED

0x80030019

STG_E_SEEKERROR

0x8003001D

STG_E_WRITEFAULT

0x8003001E

STG_E_READFAULT

0x80030020

STG_E_SHAREVIOLATION

0x80030021

STG_E_LOCKVIOLATION

0x80030050

STG_E_FILEALREADYEXISTS

0x80030057

STG_E_INVALIDPARAMETER

0x80030070

STG_E_MEDIUMFULL

0x800300F0

STG_E_PROPSETMISMATCHED

0x800300FA

STG_E_ABNORMALAPIEXIT

0x800300FB

STG_E_INVALIDHEADER

0x800300FC

STG_E_INVALIDNAME

0x800300FD

STG_E_UNKNOWN

0x800300FE

STG_E_UNIMPLEMENTEDFUNCTION

0x800300FF

STG_E_INVALIDFLAG

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There is insufficient memory
available to complete operation.

Invalid pointer error.

There are no more entries to
return.

Disk is write-protected.

An error occurred during a seek
operation.

A disk error occurred during a
write operation.

A disk error occurred during a
read operation.

A share violation has occurred.

A lock violation has occurred.

%1 already exists.

Invalid parameter error.

There is insufficient disk space
to complete operation.

Illegal write of non-simple
property to simple property set.

An application programming
interface (API) call exited
abnormally.

The file %1 is not a valid
compound file.

The name %1 is not valid.

An unexpected error occurred.

That function is not
implemented.

Invalid flag error.

31 / 497


Return value/code

0x80030100

STG_E_INUSE

0x80030101

STG_E_NOTCURRENT

0x80030102

STG_E_REVERTED

0x80030103

STG_E_CANTSAVE

0x80030104

STG_E_OLDFORMAT

0x80030105

STG_E_OLDDLL

0x80030106

STG_E_SHAREREQUIRED

0x80030107

STG_E_NOTFILEBASEDSTORAGE

0x80030108

STG_E_EXTANTMARSHALLINGS

0x80030109

STG_E_DOCFILECORRUPT

0x80030110

STG_E_BADBASEADDRESS

0x80030111

STG_E_DOCFILETOOLARGE

0x80030112

STG_E_NOTSIMPLEFORMAT

0x80030201

STG_E_INCOMPLETE

0x80030202

STG_E_TERMINATED

0x80030305

STG_E_STATUS_COPY_PROTECTION_FAILURE

0x80030306

STG_E_CSS_AUTHENTICATION_FAILURE

0x80030307

STG_E_CSS_KEY_NOT_PRESENT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Attempted to use an object that
is busy.

The storage has been changed
since the last commit.

Attempted to use an object that
has ceased to exist.

Cannot save.

The compound file %1 was
produced with an incompatible
version of storage.

The compound file %1 was
produced with a newer version
of storage.

Share.exe or equivalent is
required for operation.

Illegal operation called on non-
file based storage.

Illegal operation called on
object with extant marshalings.

The docfile has been corrupted.

OLE32.DLL has been loaded at
the wrong address.

The compound file is too large
for the current implementation.

The compound file was not
created with the STGM_SIMPLE
flag.

The file download was aborted
abnormally. The file is
incomplete.

The file download has been
terminated.

Generic Copy Protection Error.

Copy Protection Error—DVD
CSS Authentication failed.

Copy Protection Error—The
given sector does not have a
valid CSS key.

32 / 497


Return value/code

0x80030308

STG_E_CSS_KEY_NOT_ESTABLISHED

0x80030309

STG_E_CSS_SCRAMBLED_SECTOR

0x8003030A

STG_E_CSS_REGION_MISMATCH

0x8003030B

STG_E_RESETS_EXHAUSTED

0x80040000

OLE_E_OLEVERB

0x80040001

OLE_E_ADVF

0x80040002

OLE_E_ENUM_NOMORE

0x80040003

OLE_E_ADVISENOTSUPPORTED

0x80040004

OLE_E_NOCONNECTION

0x80040005

OLE_E_NOTRUNNING

0x80040006

OLE_E_NOCACHE

0x80040007

OLE_E_BLANK

0x80040008

OLE_E_CLASSDIFF

0x80040009

OLE_E_CANT_GETMONIKER

0x8004000A

OLE_E_CANT_BINDTOSOURCE

0x8004000B

OLE_E_STATIC

0x8004000C

OLE_E_PROMPTSAVECANCELLED

0x8004000D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Copy Protection Error—DVD
session key not established.

Copy Protection Error—The read
failed because the sector is
encrypted.

Copy Protection Error—The
current DVD's region does not
correspond to the region setting
of the drive.

Copy Protection Error—The
drive's region setting might be
permanent or the number of
user resets has been
exhausted.

Invalid OLEVERB structure.

Invalid advise flags.

Cannot enumerate any more
because the associated data is
missing.

This implementation does not
take advises.

There is no connection for this
connection ID.

Need to run the object to
perform this operation.

There is no cache to operate on.

Uninitialized object.

Linked object's source class has
changed.

Not able to get the moniker of
the object.

Not able to bind to the source.

Object is static; operation not
allowed.

User canceled out of the Save
dialog box.

Invalid rectangle.

33 / 497


Return value/code

OLE_E_INVALIDRECT

0x8004000E

OLE_E_WRONGCOMPOBJ

0x8004000F

OLE_E_INVALIDHWND

0x80040010

OLE_E_NOT_INPLACEACTIVE

0x80040011

OLE_E_CANTCONVERT

0x80040012

OLE_E_NOSTORAGE

0x80040064

DV_E_FORMATETC

0x80040065

DV_E_DVTARGETDEVICE

0x80040066

DV_E_STGMEDIUM

0x80040067

DV_E_STATDATA

0x80040068

DV_E_LINDEX

0x80040069

DV_E_TYMED

0x8004006A

DV_E_CLIPFORMAT

0x8004006B

DV_E_DVASPECT

0x8004006C

DV_E_DVTARGETDEVICE_SIZE

0x8004006D

DV_E_NOIVIEWOBJECT

0x80040100

DRAGDROP_E_NOTREGISTERED

0x80040101

DRAGDROP_E_ALREADYREGISTERED

0x80040102

DRAGDROP_E_INVALIDHWND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

compobj.dll is too old for the
ole2.dll initialized.

Invalid window handle.

Object is not in any of the
inplace active states.

Not able to convert object.

Not able to perform the
operation because object is not
given storage yet.

Invalid FORMATETC structure.

Invalid DVTARGETDEVICE
structure.

Invalid STDGMEDIUM structure.

Invalid STATDATA structure.

Invalid lindex.

Invalid TYMED structure.

Invalid clipboard format.

Invalid aspects.

The tdSize parameter of the
DVTARGETDEVICE structure is
invalid.

Object does not support
IViewObject interface.

Trying to revoke a drop target
that has not been registered.

This window has already been
registered as a drop target.

Invalid window handle.

34 / 497


Return value/code

0x80040110

CLASS_E_NOAGGREGATION

0x80040111

CLASS_E_CLASSNOTAVAILABLE

0x80040112

CLASS_E_NOTLICENSED

0x80040140

VIEW_E_DRAW

0x80040150

REGDB_E_READREGDB

0x80040151

REGDB_E_WRITEREGDB

0x80040152

REGDB_E_KEYMISSING

0x80040153

REGDB_E_INVALIDVALUE

0x80040154

REGDB_E_CLASSNOTREG

0x80040155

REGDB_E_IIDNOTREG

0x80040156

REGDB_E_BADTHREADINGMODEL

0x80040160

CAT_E_CATIDNOEXIST

0x80040161

CAT_E_NODESCRIPTION

0x80040164

CS_E_PACKAGE_NOTFOUND

0x80040165

CS_E_NOT_DELETABLE

0x80040166

CS_E_CLASS_NOTFOUND

0x80040167

CS_E_INVALID_VERSION

0x80040168

CS_E_NO_CLASSSTORE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Class does not support
aggregation (or class object is
remote).

ClassFactory cannot supply
requested class.

Class is not licensed for use.

Error drawing view.

Could not read key from
registry.

Could not write key to registry.

Could not find the key in the
registry.

Invalid value for registry.

Class not registered.

Interface not registered.

Threading model entry is not
valid.

CATID does not exist.

Description not found.

No package in the software
installation data in Active
Directory meets this criteria.

Deleting this will break the
referential integrity of the
software installation data in
Active Directory.

The CLSID was not found in the
software installation data in
Active Directory.

The software installation data in
Active Directory is corrupt.

There is no software installation
data in Active Directory.

35 / 497


Return value/code

0x80040169

CS_E_OBJECT_NOTFOUND

0x8004016A

CS_E_OBJECT_ALREADY_EXISTS

0x8004016B

CS_E_INVALID_PATH

0x8004016C

CS_E_NETWORK_ERROR

0x8004016D

CS_E_ADMIN_LIMIT_EXCEEDED

0x8004016E

CS_E_SCHEMA_MISMATCH

0x8004016F

CS_E_INTERNAL_ERROR

0x80040170

CACHE_E_NOCACHE_UPDATED

0x80040180

OLEOBJ_E_NOVERBS

0x80040181

OLEOBJ_E_INVALIDVERB

0x800401A0

INPLACE_E_NOTUNDOABLE

0x800401A1

INPLACE_E_NOTOOLSPACE

0x800401C0

CONVERT10_E_OLESTREAM_GET

0x800401C1

CONVERT10_E_OLESTREAM_PUT

0x800401C2

CONVERT10_E_OLESTREAM_FMT

0x800401C3

CONVERT10_E_OLESTREAM_BITMAP_TO_DIB

0x800401C4

CONVERT10_E_STG_FMT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There is no software installation
data object in Active Directory.

The software installation data
object in Active Directory
already exists.

The path to the software
installation data in Active
Directory is not correct.

A network error interrupted the
operation.

The size of this object exceeds
the maximum size set by the
administrator.

The schema for the software
installation data in Active
Directory does not match the
required schema.

An error occurred in the
software installation data in
Active Directory.

Cache not updated.

No verbs for OLE object.

Invalid verb for OLE object.

Undo is not available.

Space for tools is not available.

OLESTREAM Get method failed.

OLESTREAM Put method failed.

Contents of the OLESTREAM not
in correct format.

There was an error in a
Windows GDI call while
converting the bitmap to a
device-independent bitmap
(DIB).

Contents of the IStorage not in
correct format.

36 / 497


Return value/code

0x800401C5

CONVERT10_E_STG_NO_STD_STREAM

0x800401C6

CONVERT10_E_STG_DIB_TO_BITMAP

0x800401D0

CLIPBRD_E_CANT_OPEN

0x800401D1

CLIPBRD_E_CANT_EMPTY

0x800401D2

CLIPBRD_E_CANT_SET

0x800401D3

CLIPBRD_E_BAD_DATA

0x800401D4

CLIPBRD_E_CANT_CLOSE

0x800401E0

MK_E_CONNECTMANUALLY

0x800401E1

MK_E_EXCEEDEDDEADLINE

0x800401E2

MK_E_NEEDGENERIC

0x800401E3

MK_E_UNAVAILABLE

0x800401E4

MK_E_SYNTAX

0x800401E5

MK_E_NOOBJECT

0x800401E6

MK_E_INVALIDEXTENSION

0x800401E7

MK_E_INTERMEDIATEINTERFACENOTSUPPORTED

0x800401E8

MK_E_NOTBINDABLE

0x800401E9

MK_E_NOTBOUND

0x800401EA

MK_E_CANTOPENFILE

0x800401EB

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Contents of IStorage is missing
one of the standard streams.

There was an error in a
Windows Graphics Device
Interface (GDI) call while
converting the DIB to a bitmap.

OpenClipboard failed.

EmptyClipboard failed.

SetClipboard failed.

Data on clipboard is invalid.

CloseClipboard failed.

Moniker needs to be connected
manually.

Operation exceeded deadline.

Moniker needs to be generic.

Operation unavailable.

Invalid syntax.

No object for moniker.

Bad extension for file.

Intermediate operation failed.

Moniker is not bindable.

Moniker is not bound.

Moniker cannot open file.

User input required for

37 / 497


Return value/code

MK_E_MUSTBOTHERUSER

0x800401EC

MK_E_NOINVERSE

0x800401ED

MK_E_NOSTORAGE

0x800401EE

MK_E_NOPREFIX

0x800401EF

MK_E_ENUMERATION_FAILED

0x800401F0

CO_E_NOTINITIALIZED

0x800401F1

CO_E_ALREADYINITIALIZED

0x800401F2

CO_E_CANTDETERMINECLASS

0x800401F3

CO_E_CLASSSTRING

0x800401F4

CO_E_IIDSTRING

0x800401F5

CO_E_APPNOTFOUND

0x800401F6

CO_E_APPSINGLEUSE

0x800401F7

CO_E_ERRORINAPP

0x800401F8

CO_E_DLLNOTFOUND

0x800401F9

CO_E_ERRORINDLL

0x800401FA

CO_E_WRONGOSFORAPP

0x800401FB

CO_E_OBJNOTREG

0x800401FC

CO_E_OBJISREG

0x800401FD

CO_E_OBJNOTCONNECTED

0x800401FE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

operation to succeed.

Moniker class has no inverse.

Moniker does not refer to
storage.

No common prefix.

Moniker could not be
enumerated.

CoInitialize has not been called.

CoInitialize has already been
called.

Class of object cannot be
determined.

Invalid class string.

Invalid interface string.

Application not found.

Application cannot be run more
than once.

Some error in application.

DLL for class not found.

Error in the DLL.

Wrong operating system or
operating system version for
application.

Object is not registered.

Object is already registered.

Object is not connected to
server.

Application was launched, but it

38 / 497


Return value/code

CO_E_APPDIDNTREG

0x800401FF

CO_E_RELEASED

0x80040201

EVENT_E_ALL_SUBSCRIBERS_FAILED

0x80040203

EVENT_E_QUERYSYNTAX

0x80040204

EVENT_E_QUERYFIELD

0x80040205

EVENT_E_INTERNALEXCEPTION

0x80040206

EVENT_E_INTERNALERROR

0x80040207

EVENT_E_INVALID_PER_USER_SID

0x80040208

EVENT_E_USER_EXCEPTION

0x80040209

EVENT_E_TOO_MANY_METHODS

0x8004020A

EVENT_E_MISSING_EVENTCLASS

0x8004020B

EVENT_E_NOT_ALL_REMOVED

0x8004020C

EVENT_E_COMPLUS_NOT_INSTALLED

0x8004020D

EVENT_E_CANT_MODIFY_OR_DELETE_UNCONFIGURED_OBJECT

0x8004020E

EVENT_E_CANT_MODIFY_OR_DELETE_CONFIGURED_OBJECT

0x8004020F

EVENT_E_INVALID_EVENT_CLASS_PARTITION

0x80040210

EVENT_E_PER_USER_SID_NOT_LOGGED_ON

0x80041309

SCHED_E_TRIGGER_NOT_FOUND

0x8004130A

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

did not register a class factory.

Object has been released.

An event was unable to invoke
any of the subscribers.

A syntax error occurred trying
to evaluate a query string.

An invalid field name was used
in a query string.

An unexpected exception was
raised.

An unexpected internal error
was detected.

The owner security identifier
(SID) on a per-user subscription
does not exist.

A user-supplied component or
subscriber raised an exception.

An interface has too many
methods to fire events from.

A subscription cannot be stored
unless its event class already
exists.

Not all the objects requested
could be removed.

COM+ is required for this
operation, but it is not installed.

Cannot modify or delete an
object that was not added using
the COM+ Administrative SDK.

Cannot modify or delete an
object that was added using the
COM+ Administrative SDK.

The event class for this
subscription is in an invalid
partition.

The owner of the PerUser
subscription is not logged on to
the system specified.

Trigger not found.

One or more of the properties

39 / 497


Return value/code

SCHED_E_TASK_NOT_READY

0x8004130B

SCHED_E_TASK_NOT_RUNNING

0x8004130C

SCHED_E_SERVICE_NOT_INSTALLED

0x8004130D

SCHED_E_CANNOT_OPEN_TASK

0x8004130E

SCHED_E_INVALID_TASK

0x8004130F

SCHED_E_ACCOUNT_INFORMATION_NOT_SET

0x80041310

SCHED_E_ACCOUNT_NAME_NOT_FOUND

0x80041311

SCHED_E_ACCOUNT_DBASE_CORRUPT

0x80041312

SCHED_E_NO_SECURITY_SERVICES

0x80041313

SCHED_E_UNKNOWN_OBJECT_VERSION

0x80041314

SCHED_E_UNSUPPORTED_ACCOUNT_OPTION

0x80041315

SCHED_E_SERVICE_NOT_RUNNING

0x80041316

SCHED_E_UNEXPECTEDNODE

0x80041317

SCHED_E_NAMESPACE

0x80041318

SCHED_E_INVALIDVALUE

0x80041319

SCHED_E_MISSINGNODE

0x8004131A

SCHED_E_MALFORMEDXML

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

that are needed to run this task
have not been set.

There is no running instance of
the task.

The Task Scheduler service is
not installed on this computer.

The task object could not be
opened.

The object is either an invalid
task object or is not a task
object.

No account information could be
found in the Task Scheduler
security database for the task
indicated.

Unable to establish existence of
the account specified.

Corruption was detected in the
Task Scheduler security
database; the database has
been reset.

Task Scheduler security
services are available only on
Windows NT operating system.

The task object version is either
unsupported or invalid.

The task has been configured
with an unsupported
combination of account settings
and run-time options.

The Task Scheduler service is
not running.

The task XML contains an
unexpected node.

The task XML contains an
element or attribute from an
unexpected namespace.

The task XML contains a value
that is incorrectly formatted or
out of range.

The task XML is missing a
required element or attribute.

The task XML is malformed.

40 / 497


Return value/code

0x8004131D

SCHED_E_TOO_MANY_NODES

0x8004131E

SCHED_E_PAST_END_BOUNDARY

0x8004131F

SCHED_E_ALREADY_RUNNING

0x80041320

SCHED_E_USER_NOT_LOGGED_ON

0x80041321

SCHED_E_INVALID_TASK_HASH

0x80041322

SCHED_E_SERVICE_NOT_AVAILABLE

0x80041323

SCHED_E_SERVICE_TOO_BUSY

0x80041324

SCHED_E_TASK_ATTEMPTED

0x8004D000

XACT_E_ALREADYOTHERSINGLEPHASE

0x8004D001

XACT_E_CANTRETAIN

0x8004D002

XACT_E_COMMITFAILED

0x8004D003

XACT_E_COMMITPREVENTED

0x8004D004

XACT_E_HEURISTICABORT

0x8004D005

XACT_E_HEURISTICCOMMIT

0x8004D006

XACT_E_HEURISTICDAMAGE

0x8004D007

XACT_E_HEURISTICDANGER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The task XML contains too
many nodes of the same type.

The task cannot be started after
the trigger's end boundary.

An instance of this task is
already running.

The task will not run because
the user is not logged on.

The task image is corrupt or has
been tampered with.

The Task Scheduler service is
not available.

The Task Scheduler service is
too busy to handle your
request. Try again later.

The Task Scheduler service
attempted to run the task, but
the task did not run due to one
of the constraints in the task
definition.

Another single phase resource
manager has already been
enlisted in this transaction.

A retaining commit or abort is
not supported.

The transaction failed to commit
for an unknown reason. The
transaction was aborted.

Cannot call commit on this
transaction object because the
calling application did not
initiate the transaction.

Instead of committing, the
resource heuristically aborted.

Instead of aborting, the
resource heuristically
committed.

Some of the states of the
resource were committed while
others were aborted, likely
because of heuristic decisions.

Some of the states of the
resource might have been
committed while others were
aborted, likely because of

41 / 497


Return value/code

0x8004D008

XACT_E_ISOLATIONLEVEL

0x8004D009

XACT_E_NOASYNC

0x8004D00A

XACT_E_NOENLIST

0x8004D00B

XACT_E_NOISORETAIN

0x8004D00C

XACT_E_NORESOURCE

0x8004D00D

XACT_E_NOTCURRENT

0x8004D00E

XACT_E_NOTRANSACTION

0x8004D00F

XACT_E_NOTSUPPORTED

0x8004D010

XACT_E_UNKNOWNRMGRID

0x8004D011

XACT_E_WRONGSTATE

0x8004D012

XACT_E_WRONGUOW

0x8004D013

XACT_E_XTIONEXISTS

0x8004D014

XACT_E_NOIMPORTOBJECT

0x8004D015

XACT_E_INVALIDCOOKIE

0x8004D016

XACT_E_INDOUBT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

heuristic decisions.

The requested isolation level is
not valid or supported.

The transaction manager does
not support an asynchronous
operation for this method.

Unable to enlist in the
transaction.

The requested semantics of
retention of isolation across
retaining commit and abort
boundaries cannot be supported
by this transaction
implementation, or isoFlags
was not equal to 0.

There is no resource presently
associated with this enlistment.

The transaction failed to commit
due to the failure of optimistic
concurrency control in at least
one of the resource managers.

The transaction has already
been implicitly or explicitly
committed or aborted.

An invalid combination of flags
was specified.

The resource manager ID is not
associated with this transaction
or the transaction manager.

This method was called in the
wrong state.

The indicated unit of work does
not match the unit of work
expected by the resource
manager.

An enlistment in a transaction
already exists.

An import object for the
transaction could not be found.

The transaction cookie is
invalid.

The transaction status is in
doubt. A communication failure
occurred, or a transaction
manager or resource manager

42 / 497


Return value/code

0x8004D017

XACT_E_NOTIMEOUT

0x8004D018

XACT_E_ALREADYINPROGRESS

0x8004D019

XACT_E_ABORTED

0x8004D01A

XACT_E_LOGFULL

0x8004D01B

XACT_E_TMNOTAVAILABLE

0x8004D01C

XACT_E_CONNECTION_DOWN

0x8004D01D

XACT_E_CONNECTION_DENIED

0x8004D01E

XACT_E_REENLISTTIMEOUT

0x8004D01F

XACT_E_TIP_CONNECT_FAILED

0x8004D020

XACT_E_TIP_PROTOCOL_ERROR

0x8004D021

XACT_E_TIP_PULL_FAILED

0x8004D022

XACT_E_DEST_TMNOTAVAILABLE

0x8004D023

XACT_E_TIP_DISABLED

0x8004D024

XACT_E_NETWORK_TX_DISABLED

0x8004D025

XACT_E_PARTNER_NETWORK_TX_DISABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

has failed.

A time-out was specified, but
time-outs are not supported.

The requested operation is
already in progress for the
transaction.

The transaction has already
been aborted.

The Transaction Manager
returned a log full error.

The transaction manager is not
available.

A connection with the
transaction manager was lost.

A request to establish a
connection with the transaction
manager was denied.

Resource manager reenlistment
to determine transaction status
timed out.

The transaction manager failed
to establish a connection with
another Transaction Internet
Protocol (TIP) transaction
manager.

The transaction manager
encountered a protocol error
with another TIP transaction
manager.

The transaction manager could
not propagate a transaction
from another TIP transaction
manager.

The transaction manager on the
destination machine is not
available.

The transaction manager has
disabled its support for TIP.

The transaction manager has
disabled its support for remote
or network transactions.

The partner transaction
manager has disabled its
support for remote or network
transactions.

43 / 497


Return value/code

0x8004D026

XACT_E_XA_TX_DISABLED

0x8004D027

XACT_E_UNABLE_TO_READ_DTC_CONFIG

0x8004D028

XACT_E_UNABLE_TO_LOAD_DTC_PROXY

0x8004D029

XACT_E_ABORTING

0x8004D080

XACT_E_CLERKNOTFOUND

0x8004D081

XACT_E_CLERKEXISTS

0x8004D082

XACT_E_RECOVERYINPROGRESS

0x8004D083

XACT_E_TRANSACTIONCLOSED

0x8004D084

XACT_E_INVALIDLSN

0x8004D085

XACT_E_REPLAYREQUEST

0x8004D100

XACT_E_CONNECTION_REQUEST_DENIED

0x8004D101

XACT_E_TOOMANY_ENLISTMENTS

0x8004D102

XACT_E_DUPLICATE_GUID

0x8004D103

XACT_E_NOTSINGLEPHASE

0x8004D104

XACT_E_RECOVERYALREADYDONE

0x8004D105

XACT_E_PROTOCOL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The transaction manager has
disabled its support for XA
transactions.

Microsoft Distributed
Transaction Coordinator
(MSDTC) was unable to read its
configuration information.

MSDTC was unable to load the
DTC proxy DLL.

The local transaction has
aborted.

The specified CRM clerk was not
found. It might have completed
before it could be held.

The specified CRM clerk does
not exist.

Recovery of the CRM log file is
still in progress.

The transaction has completed,
and the log records have been
discarded from the log file. They
are no longer available.

lsnToRead is outside of the
current limits of the log

The COM+ Compensating
Resource Manager has records
it wishes to replay.

The request to connect to the
specified transaction
coordinator was denied.

The maximum number of
enlistments for the specified
transaction has been reached.

A resource manager with the
same identifier is already
registered with the specified
transaction coordinator.

The prepare request given was
not eligible for single-phase
optimizations.

RecoveryComplete has already
been called for the given
resource manager.

The interface call made was
incorrect for the current state of
the protocol.

44 / 497


Return value/code

0x8004D106

XACT_E_RM_FAILURE

0x8004D107

XACT_E_RECOVERY_FAILED

0x8004D108

XACT_E_LU_NOT_FOUND

0x8004D109

XACT_E_DUPLICATE_LU

0x8004D10A

XACT_E_LU_NOT_CONNECTED

0x8004D10B

XACT_E_DUPLICATE_TRANSID

0x8004D10C

XACT_E_LU_BUSY

0x8004D10D

XACT_E_LU_NO_RECOVERY_PROCESS

0x8004D10E

XACT_E_LU_DOWN

0x8004D10F

XACT_E_LU_RECOVERING

0x8004D110

XACT_E_LU_RECOVERY_MISMATCH

0x8004D111

XACT_E_RM_UNAVAILABLE

0x8004E002

CONTEXT_E_ABORTED

0x8004E003

CONTEXT_E_ABORTING

0x8004E004

CONTEXT_E_NOCONTEXT

0x8004E005

CONTEXT_E_WOULD_DEADLOCK

0x8004E006

CONTEXT_E_SYNCH_TIMEOUT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The xa_open call failed for the
XA resource.

The xa_recover call failed for
the XA resource.

The logical unit of work
specified cannot be found.

The specified logical unit of
work already exists.

Subordinate creation failed. The
specified logical unit of work
was not connected.

A transaction with the given
identifier already exists.

The resource is in use.

The LU Recovery process is
down.

The remote session was lost.

The resource is currently
recovering.

There was a mismatch in
driving recovery.

An error occurred with the XA
resource.

The root transaction wanted to
commit, but the transaction
aborted.

The COM+ component on which
the method call was made has a
transaction that has already
aborted or is in the process of
aborting.

There is no Microsoft
Transaction Server (MTS) object
context.

The component is configured to
use synchronization, and this
method call would cause a
deadlock to occur.

The component is configured to
use synchronization, and a
thread has timed out waiting to

45 / 497


Return value/code

0x8004E007

CONTEXT_E_OLDREF

0x8004E00C

CONTEXT_E_ROLENOTFOUND

0x8004E00F

CONTEXT_E_TMNOTAVAILABLE

0x8004E021

CO_E_ACTIVATIONFAILED

0x8004E022

CO_E_ACTIVATIONFAILED_EVENTLOGGED

0x8004E023

CO_E_ACTIVATIONFAILED_CATALOGERROR

0x8004E024

CO_E_ACTIVATIONFAILED_TIMEOUT

0x8004E025

CO_E_INITIALIZATIONFAILED

0x8004E026

CONTEXT_E_NOJIT

0x8004E027

CONTEXT_E_NOTRANSACTION

0x8004E028

CO_E_THREADINGMODEL_CHANGED

0x8004E029

CO_E_NOIISINTRINSICS

0x8004E02A

CO_E_NOCOOKIES

0x8004E02B

CO_E_DBERROR

0x8004E02C

CO_E_NOTPOOLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

enter the context.

You made a method call on a
COM+ component that has a
transaction that has already
committed or aborted.

The specified role was not
configured for the application.

COM+ was unable to talk to the
MSDTC.

An unexpected error occurred
during COM+ activation.

COM+ activation failed. Check
the event log for more
information.

COM+ activation failed due to a
catalog or configuration error.

COM+ activation failed because
the activation could not be
completed in the specified
amount of time.

COM+ activation failed because
an initialization function failed.
Check the event log for more
information.

The requested operation
requires that just-in-time (JIT)
be in the current context, and it
is not.

The requested operation
requires that the current
context have a transaction, and
it does not.

The components threading
model has changed after install
into a COM+ application. Re-
install component.

Internet Information Services
(IIS) intrinsics not available.
Start your work with IIS.

An attempt to write a cookie
failed.

An attempt to use a database
generated a database-specific
error.

The COM+ component you
created must use object pooling

46 / 497


Return value/code

0x8004E02D

CO_E_NOTCONSTRUCTED

0x8004E02E

CO_E_NOSYNCHRONIZATION

0x8004E02F

CO_E_ISOLEVELMISMATCH

0x8004E030

CO_E_CALL_OUT_OF_TX_SCOPE_NOT_ALLOWED

0x8004E031

CO_E_EXIT_TRANSACTION_SCOPE_NOT_CALLED

0x80070005

E_ACCESSDENIED

0x8007000E

E_OUTOFMEMORY

0x80070032

ERROR_NOT_SUPPORTED

0x80070057

E_INVALIDARG

0x80070070

ERROR_DISK_FULL

0x80080001

CO_E_CLASS_CREATE_FAILED

0x80080002

CO_E_SCM_ERROR

0x80080003

CO_E_SCM_RPC_FAILURE

0x80080004

CO_E_BAD_PATH

0x80080005

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

to work.

The COM+ component you
created must use object
construction to work correctly.

The COM+ component requires
synchronization, and it is not
configured for it.

The TxIsolation Level property
for the COM+ component being
created is stronger than the
TxIsolationLevel for the root.

The component attempted to
make a cross-context call
between invocations of
EnterTransactionScope and
ExitTransactionScope. This is
not allowed. Cross-context calls
cannot be made while inside a
transaction scope.

The component made a call to
EnterTransactionScope, but did
not make a corresponding call
to ExitTransactionScope before
returning.

General access denied error.

The server does not have
enough memory for the new
channel.

The server cannot support a
client request for a dynamic
virtual channel.

One or more arguments are
invalid.

There is not enough space on
the disk.

Attempt to create a class object
failed.

OLE service could not bind
object.

RPC communication failed with
OLE service.

Bad path to object.

Server execution failed.

47 / 497


Return value/code

CO_E_SERVER_EXEC_FAILURE

0x80080006

CO_E_OBJSRV_RPC_FAILURE

0x80080007

MK_E_NO_NORMALIZED

0x80080008

CO_E_SERVER_STOPPING

0x80080009

MEM_E_INVALID_ROOT

0x80080010

MEM_E_INVALID_LINK

0x80080011

MEM_E_INVALID_SIZE

0x80080015

CO_E_MISSING_DISPLAYNAME

0x80080016

CO_E_RUNAS_VALUE_MUST_BE_AAA

0x80080017

CO_E_ELEVATION_DISABLED

0x80090001

NTE_BAD_UID

0x80090002

NTE_BAD_HASH

0x80090003

NTE_BAD_KEY

0x80090004

NTE_BAD_LEN

0x80090005

NTE_BAD_DATA

0x80090006

NTE_BAD_SIGNATURE

0x80090007

NTE_BAD_VER

0x80090008

NTE_BAD_ALGID

0x80090009

NTE_BAD_FLAGS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

OLE service could not
communicate with the object
server.

Moniker path could not be
normalized.

Object server is stopping when
OLE service contacts it.

An invalid root block pointer
was specified.

An allocation chain contained an
invalid link pointer.

The requested allocation size
was too large.

The activation requires a display
name to be present under the
class identifier (CLSID) key.

The activation requires that the
RunAs value for the application
is Activate As Activator.

The class is not configured to
support elevated activation.

Bad UID.

Bad hash.

Bad key.

Bad length.

Bad data.

Invalid signature.

Bad version of provider.

Invalid algorithm specified.

Invalid flags specified.

48 / 497


Return value/code

0x8009000A

NTE_BAD_TYPE

0x8009000B

NTE_BAD_KEY_STATE

0x8009000C

NTE_BAD_HASH_STATE

0x8009000D

NTE_NO_KEY

0x8009000E

NTE_NO_MEMORY

0x8009000F

NTE_EXISTS

0x80090010

NTE_PERM

0x80090011

NTE_NOT_FOUND

0x80090012

NTE_DOUBLE_ENCRYPT

0x80090013

NTE_BAD_PROVIDER

0x80090014

NTE_BAD_PROV_TYPE

0x80090015

NTE_BAD_PUBLIC_KEY

0x80090016

NTE_BAD_KEYSET

0x80090017

NTE_PROV_TYPE_NOT_DEF

0x80090018

NTE_PROV_TYPE_ENTRY_BAD

0x80090019

NTE_KEYSET_NOT_DEF

0x8009001A

NTE_KEYSET_ENTRY_BAD

0x8009001B

NTE_PROV_TYPE_NO_MATCH

0x8009001C

NTE_SIGNATURE_FILE_BAD

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Invalid type specified.

Key not valid for use in
specified state.

Hash not valid for use in
specified state.

Key does not exist.

Insufficient memory available
for the operation.

Object already exists.

Access denied.

Object was not found.

Data already encrypted.

Invalid provider specified.

Invalid provider type specified.

Provider's public key is invalid.

Key set does not exist.

Provider type not defined.

The provider type, as
registered, is invalid.

The key set is not defined.

The key set, as registered, is
invalid.

Provider type does not match
registered value.

The digital signature file is
corrupt.

49 / 497


Return value/code

0x8009001D

NTE_PROVIDER_DLL_FAIL

0x8009001E

NTE_PROV_DLL_NOT_FOUND

0x8009001F

NTE_BAD_KEYSET_PARAM

0x80090020

NTE_FAIL

0x80090021

NTE_SYS_ERR

0x80090022

NTE_SILENT_CONTEXT

0x80090023

NTE_TOKEN_KEYSET_STORAGE_FULL

0x80090024

NTE_TEMPORARY_PROFILE

0x80090025

NTE_FIXEDPARAMETER

0x80090026

NTE_INVALID_HANDLE

0x80090027

NTE_INVALID_PARAMETER

0x80090028

NTE_BUFFER_TOO_SMALL

0x80090029

NTE_NOT_SUPPORTED

0x8009002A

NTE_NO_MORE_ITEMS

0x8009002B

NTE_BUFFERS_OVERLAP

0x8009002C

NTE_DECRYPTION_FAILURE

0x8009002D

NTE_INTERNAL_ERROR

0x8009002E

NTE_UI_REQUIRED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Provider DLL failed to initialize
correctly.

Provider DLL could not be
found.

The keyset parameter is invalid.

An internal error occurred.

A base error occurred.

Provider could not perform the
action because the context was
acquired as silent.

The security token does not
have storage space available for
an additional container.

The profile for the user is a
temporary profile.

The key parameters could not
be set because the
configuration service provider
(CSP) uses fixed parameters.

The supplied handle is invalid.

The parameter is incorrect.

The buffer supplied to a
function was too small.

The requested operation is not
supported.

No more data is available.

The supplied buffers overlap
incorrectly.

The specified data could not be
decrypted.

An internal consistency check
failed.

This operation requires input
from the user.

50 / 497


Return value/code

0x8009002F

NTE_HMAC_NOT_SUPPORTED

0x80090300

SEC_E_INSUFFICIENT_MEMORY

0x80090301

SEC_E_INVALID_HANDLE

0x80090302

SEC_E_UNSUPPORTED_FUNCTION

0x80090303

SEC_E_TARGET_UNKNOWN

0x80090304

SEC_E_INTERNAL_ERROR

0x80090305

SEC_E_SECPKG_NOT_FOUND

0x80090306

SEC_E_NOT_OWNER

0x80090307

SEC_E_CANNOT_INSTALL

0x80090308

SEC_E_INVALID_TOKEN

0x80090309

SEC_E_CANNOT_PACK

0x8009030A

SEC_E_QOP_NOT_SUPPORTED

0x8009030B

SEC_E_NO_IMPERSONATION

0x8009030C

SEC_E_LOGON_DENIED

0x8009030D

SEC_E_UNKNOWN_CREDENTIALS

0x8009030E

SEC_E_NO_CREDENTIALS

0x8009030F

SEC_E_MESSAGE_ALTERED

0x80090310

SEC_E_OUT_OF_SEQUENCE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The cryptographic provider does
not support Hash Message
Authentication Code (HMAC).

Not enough memory is available
to complete this request.

The handle specified is invalid.

The function requested is not
supported.

The specified target is unknown
or unreachable.

The Local Security Authority
(LSA) cannot be contacted.

The requested security package
does not exist.

The caller is not the owner of
the desired credentials.

The security package failed to
initialize and cannot be
installed.

The token supplied to the
function is invalid.

The security package is not able
to marshal the logon buffer, so
the logon attempt has failed.

The per-message quality of
protection is not supported by
the security package.

The security context does not
allow impersonation of the
client.

The logon attempt failed.

The credentials supplied to the
package were not recognized.

No credentials are available in
the security package.

The message or signature
supplied for verification has
been altered.

The message supplied for
verification is out of sequence.

51 / 497


Return value/code

0x80090311

SEC_E_NO_AUTHENTICATING_AUTHORITY

0x80090316

SEC_E_BAD_PKGID

0x80090317

SEC_E_CONTEXT_EXPIRED

0x80090318

SEC_E_INCOMPLETE_MESSAGE

0x80090320

SEC_E_INCOMPLETE_CREDENTIALS

0x80090321

SEC_E_BUFFER_TOO_SMALL

0x80090322

SEC_E_WRONG_PRINCIPAL

0x80090324

SEC_E_TIME_SKEW

0x80090325

SEC_E_UNTRUSTED_ROOT

0x80090326

SEC_E_ILLEGAL_MESSAGE

0x80090327

SEC_E_CERT_UNKNOWN

0x80090328

SEC_E_CERT_EXPIRED

0x80090329

SEC_E_ENCRYPT_FAILURE

0x80090330

SEC_E_DECRYPT_FAILURE

0x80090331

SEC_E_ALGORITHM_MISMATCH

0x80090332

SEC_E_SECURITY_QOS_FAILED

0x80090333

SEC_E_UNFINISHED_CONTEXT_DELETED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

No authority could be contacted
for authentication.

The requested security package
does not exist.

The context has expired and
can no longer be used.

The supplied message is
incomplete. The signature was
not verified.

The credentials supplied were
not complete and could not be
verified. The context could not
be initialized.

The buffers supplied to a
function was too small.

The target principal name is
incorrect.

The clocks on the client and
server machines are skewed.

The certificate chain was issued
by an authority that is not
trusted.

The message received was
unexpected or badly formatted.

An unknown error occurred
while processing the certificate.

The received certificate has
expired.

The specified data could not be
encrypted.

The specified data could not be
decrypted.

The client and server cannot
communicate because they do
not possess a common
algorithm.

The security context could not
be established due to a failure
in the requested quality of
service (for example, mutual
authentication or delegation).

A security context was deleted
before the context was

52 / 497


Return value/code

0x80090334

SEC_E_NO_TGT_REPLY

0x80090335

SEC_E_NO_IP_ADDRESSES

0x80090336

SEC_E_WRONG_CREDENTIAL_HANDLE

0x80090337

SEC_E_CRYPTO_SYSTEM_INVALID

0x80090338

SEC_E_MAX_REFERRALS_EXCEEDED

0x80090339

SEC_E_MUST_BE_KDC

0x8009033A

SEC_E_STRONG_CRYPTO_NOT_SUPPORTED

0x8009033B

SEC_E_TOO_MANY_PRINCIPALS

0x8009033C

SEC_E_NO_PA_DATA

0x8009033D

SEC_E_PKINIT_NAME_MISMATCH

0x8009033E

SEC_E_SMARTCARD_LOGON_REQUIRED

0x8009033F

SEC_E_SHUTDOWN_IN_PROGRESS

0x80090340

SEC_E_KDC_INVALID_REQUEST

0x80090341

SEC_E_KDC_UNABLE_TO_REFER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

completed. This is considered a
logon failure.

The client is trying to negotiate
a context and the server
requires user-to-user but did
not send a ticket granting ticket
(TGT) reply.

Unable to accomplish the
requested task because the
local machine does not have an
IP addresses.

The supplied credential handle
does not match the credential
associated with the security
context.

The cryptographic system or
checksum function is invalid
because a required function is
unavailable.

The number of maximum ticket
referrals has been exceeded.

The local machine must be a
Kerberos domain controller
(KDC), and it is not.

The other end of the security
negotiation requires strong
cryptographics, but it is not
supported on the local machine.

The KDC reply contained more
than one principal name.

Expected to find PA data for a
hint of what etype to use, but it
was not found.

The client certificate does not
contain a valid user principal
name (UPN), or does not match
the client name in the logon
request. Contact your
administrator.

Smart card logon is required
and was not used.

A system shutdown is in
progress.

An invalid request was sent to
the KDC.

The KDC was unable to
generate a referral for the

53 / 497


Return value/code

0x80090342

SEC_E_KDC_UNKNOWN_ETYPE

0x80090343

SEC_E_UNSUPPORTED_PREAUTH

0x80090345

SEC_E_DELEGATION_REQUIRED

0x80090346

SEC_E_BAD_BINDINGS

0x80090347

SEC_E_MULTIPLE_ACCOUNTS

0x80090348

SEC_E_NO_KERB_KEY

0x80090349

SEC_E_CERT_WRONG_USAGE

0x80090350

SEC_E_DOWNGRADE_DETECTED

0x80090351

SEC_E_SMARTCARD_CERT_REVOKED

0x80090352

SEC_E_ISSUING_CA_UNTRUSTED

0x80090353

SEC_E_REVOCATION_OFFLINE_C

0x80090354

SEC_E_PKINIT_CLIENT_FAILURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

service requested.

The encryption type requested
is not supported by the KDC.

An unsupported pre-
authentication mechanism was
presented to the Kerberos
package.

The requested operation cannot
be completed. The computer
must be trusted for delegation,
and the current user account
must be configured to allow
delegation.

Client's supplied Security
Support Provider Interface
(SSPI) channel bindings were
incorrect.

The received certificate was
mapped to multiple accounts.

No Kerberos key was found.

The certificate is not valid for
the requested usage.

The system detected a possible
attempt to compromise
security. Ensure that you can
contact the server that
authenticated you.

The smart card certificate used
for authentication has been
revoked. Contact your system
administrator. The event log
might contain additional
information.

An untrusted certification
authority (CA) was detected
while processing the smart card
certificate used for
authentication. Contact your
system administrator.

The revocation status of the
smart card certificate used for
authentication could not be
determined. Contact your
system administrator.

The smart card certificate used
for authentication was not
trusted. Contact your system
administrator.

54 / 497


Return value/code

0x80090355

SEC_E_SMARTCARD_CERT_EXPIRED

0x80090356

SEC_E_NO_S4U_PROT_SUPPORT

0x80090357

SEC_E_CROSSREALM_DELEGATION_FAILURE

0x80090358

SEC_E_REVOCATION_OFFLINE_KDC

0x80090359

SEC_E_ISSUING_CA_UNTRUSTED_KDC

0x8009035A

SEC_E_KDC_CERT_EXPIRED

0x8009035B

SEC_E_KDC_CERT_REVOKED

0x8009035D

SEC_E_INVALID_PARAMETER

0x8009035E

SEC_E_DELEGATION_POLICY

0x8009035F

SEC_E_POLICY_NLTM_ONLY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The smart card certificate used
for authentication has expired.
Contact your system
administrator.

The Kerberos subsystem
encountered an error. A service
for user protocol requests was
made against a domain
controller that does not support
services for users.

An attempt was made by this
server to make a Kerberos-
constrained delegation request
for a target outside the server's
realm. This is not supported and
indicates a misconfiguration on
this server's allowed-to-
delegate-to list. Contact your
administrator.

The revocation status of the
domain controller certificate
used for smart card
authentication could not be
determined. The system event
log contains additional
information. Contact your
system administrator.

An untrusted CA was detected
while processing the domain
controller certificate used for
authentication. The system
event log contains additional
information. Contact your
system administrator.

The domain controller certificate
used for smart card logon has
expired. Contact your system
administrator with the contents
of your system event log.

The domain controller certificate
used for smart card logon has
been revoked. Contact your
system administrator with the
contents of your system event
log.

One or more of the parameters
passed to the function were
invalid.

The client policy does not allow
credential delegation to the
target server.

The client policy does not allow
credential delegation to the
target server with NLTM only

55 / 497


Return value/code

0x80091001

CRYPT_E_MSG_ERROR

0x80091002

CRYPT_E_UNKNOWN_ALGO

0x80091003

CRYPT_E_OID_FORMAT

0x80091004

CRYPT_E_INVALID_MSG_TYPE

0x80091005

CRYPT_E_UNEXPECTED_ENCODING

0x80091006

CRYPT_E_AUTH_ATTR_MISSING

0x80091007

CRYPT_E_HASH_VALUE

0x80091008

CRYPT_E_INVALID_INDEX

0x80091009

CRYPT_E_ALREADY_DECRYPTED

0x8009100A

CRYPT_E_NOT_DECRYPTED

0x8009100B

CRYPT_E_RECIPIENT_NOT_FOUND

0x8009100C

CRYPT_E_CONTROL_TYPE

0x8009100D

CRYPT_E_ISSUER_SERIALNUMBER

0x8009100E

CRYPT_E_SIGNER_NOT_FOUND

0x8009100F

CRYPT_E_ATTRIBUTES_MISSING

0x80091010

CRYPT_E_STREAM_MSG_NOT_READY

0x80091011

CRYPT_E_STREAM_INSUFFICIENT_DATA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

authentication.

An error occurred while
performing an operation on a
cryptographic message.

Unknown cryptographic
algorithm.

The object identifier is poorly
formatted.

Invalid cryptographic message
type.

Unexpected cryptographic
message encoding.

The cryptographic message
does not contain an expected
authenticated attribute.

The hash value is not correct.

The index value is not valid.

The content of the
cryptographic message has
already been decrypted.

The content of the
cryptographic message has not
been decrypted yet.

The enveloped-data message
does not contain the specified
recipient.

Invalid control type.

Invalid issuer or serial number.

Cannot find the original signer.

The cryptographic message
does not contain all of the
requested attributes.

The streamed cryptographic
message is not ready to return
data.

The streamed cryptographic
message requires more data to
complete the decode operation.

56 / 497


Return value/code

0x80092001

CRYPT_E_BAD_LEN

0x80092002

CRYPT_E_BAD_ENCODE

0x80092003

CRYPT_E_FILE_ERROR

0x80092004

CRYPT_E_NOT_FOUND

0x80092005

CRYPT_E_EXISTS

0x80092006

CRYPT_E_NO_PROVIDER

0x80092007

CRYPT_E_SELF_SIGNED

0x80092008

CRYPT_E_DELETED_PREV

0x80092009

CRYPT_E_NO_MATCH

0x8009200A

CRYPT_E_UNEXPECTED_MSG_TYPE

0x8009200B

CRYPT_E_NO_KEY_PROPERTY

0x8009200C

CRYPT_E_NO_DECRYPT_CERT

0x8009200D

CRYPT_E_BAD_MSG

0x8009200E

CRYPT_E_NO_SIGNER

0x8009200F

CRYPT_E_PENDING_CLOSE

0x80092010

CRYPT_E_REVOKED

0x80092011

CRYPT_E_NO_REVOCATION_DLL

0x80092012

CRYPT_E_NO_REVOCATION_CHECK

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The length specified for the
output data was insufficient.

An error occurred during the
encode or decode operation.

An error occurred while reading
or writing to a file.

Cannot find object or property.

The object or property already
exists.

No provider was specified for
the store or object.

The specified certificate is self-
signed.

The previous certificate or
certificate revocation list (CRL)
context was deleted.

Cannot find the requested
object.

The certificate does not have a
property that references a
private key.

Cannot find the certificate and
private key for decryption.

Cannot find the certificate and
private key to use for
decryption.

Not a cryptographic message or
the cryptographic message is
not formatted correctly.

The signed cryptographic
message does not have a signer
for the specified signer index.

Final closure is pending until
additional frees or closes.

The certificate is revoked.

No DLL or exported function
was found to verify revocation.

The revocation function was
unable to check revocation for
the certificate.

57 / 497


Return value/code

0x80092013

CRYPT_E_REVOCATION_OFFLINE

0x80092014

CRYPT_E_NOT_IN_REVOCATION_DATABASE

0x80092020

CRYPT_E_INVALID_NUMERIC_STRING

0x80092021

CRYPT_E_INVALID_PRINTABLE_STRING

0x80092022

CRYPT_E_INVALID_IA5_STRING

0x80092023

CRYPT_E_INVALID_X500_STRING

0x80092024

CRYPT_E_NOT_CHAR_STRING

0x80092025

CRYPT_E_FILERESIZED

0x80092026

CRYPT_E_SECURITY_SETTINGS

0x80092027

CRYPT_E_NO_VERIFY_USAGE_DLL

0x80092028

CRYPT_E_NO_VERIFY_USAGE_CHECK

0x80092029

CRYPT_E_VERIFY_USAGE_OFFLINE

0x8009202A

CRYPT_E_NOT_IN_CTL

0x8009202B

CRYPT_E_NO_TRUSTED_SIGNER

0x8009202C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The revocation function was
unable to check revocation
because the revocation server
was offline.

The certificate is not in the
revocation server's database.

The string contains a non-
numeric character.

The string contains a
nonprintable character.

The string contains a character
not in the 7-bit ASCII character
set.

The string contains an invalid
X500 name attribute key, object
identifier (OID), value, or
delimiter.

The dwValueType for the
CERT_NAME_VALUE is not one
of the character strings. Most
likely it is either a
CERT_RDN_ENCODED_BLOB or
CERT_TDN_OCTED_STRING.

The Put operation cannot
continue. The file needs to be
resized. However, there is
already a signature present. A
complete signing operation
must be done.

The cryptographic operation
failed due to a local security
option setting.

No DLL or exported function
was found to verify subject
usage.

The called function was unable
to perform a usage check on
the subject.

The called function was unable
to complete the usage check
because the server was offline.

The subject was not found in a
certificate trust list (CTL).

None of the signers of the
cryptographic message or
certificate trust list is trusted.

The public key's algorithm

58 / 497


Return value/code

CRYPT_E_MISSING_PUBKEY_PARA

0x80093000

CRYPT_E_OSS_ERROR

0x80093001

OSS_MORE_BUF

0x80093002

OSS_NEGATIVE_UINTEGER

0x80093003

OSS_PDU_RANGE

0x80093004

OSS_MORE_INPUT

0x80093005

OSS_DATA_ERROR

0x80093006

OSS_BAD_ARG

0x80093007

OSS_BAD_VERSION

0x80093008

OSS_OUT_MEMORY

0x80093009

OSS_PDU_MISMATCH

0x8009300A

OSS_LIMITED

0x8009300B

OSS_BAD_PTR

0x8009300C

OSS_BAD_TIME

0x8009300D

OSS_INDEFINITE_NOT_SUPPORTED

0x8009300E

OSS_MEM_ERROR

0x8009300F

OSS_BAD_TABLE

0x80093010

OSS_TOO_LONG

0x80093011

OSS_CONSTRAINT_VIOLATED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

parameters are missing.

OSS Certificate encode/decode
error code base.

OSS ASN.1 Error: Output Buffer
is too small.

OSS ASN.1 Error: Signed
integer is encoded as a
unsigned integer.

OSS ASN.1 Error: Unknown
ASN.1 data type.

OSS ASN.1 Error: Output buffer
is too small; the decoded data
has been truncated.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid
argument.

OSS ASN.1 Error:
Encode/Decode version
mismatch.

OSS ASN.1 Error: Out of
memory.

OSS ASN.1 Error:
Encode/Decode error.

OSS ASN.1 Error: Internal
error.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Unsupported
BER indefinite-length encoding.

OSS ASN.1 Error: Access
violation.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

59 / 497


Return value/code

0x80093012

OSS_FATAL_ERROR

0x80093013

OSS_ACCESS_SERIALIZATION_ERROR

0x80093014

OSS_NULL_TBL

0x80093015

OSS_NULL_FCN

0x80093016

OSS_BAD_ENCRULES

0x80093017

OSS_UNAVAIL_ENCRULES

0x80093018

OSS_CANT_OPEN_TRACE_WINDOW

0x80093019

OSS_UNIMPLEMENTED

0x8009301A

OSS_OID_DLL_NOT_LINKED

0x8009301B

OSS_CANT_OPEN_TRACE_FILE

0x8009301C

OSS_TRACE_FILE_ALREADY_OPEN

0x8009301D

OSS_TABLE_MISMATCH

0x8009301E

OSS_TYPE_NOT_SUPPORTED

0x8009301F

OSS_REAL_DLL_NOT_LINKED

0x80093020

OSS_REAL_CODE_NOT_LINKED

0x80093021

OSS_OUT_OF_RANGE

0x80093022

OSS_COPIER_DLL_NOT_LINKED

0x80093023

OSS_CONSTRAINT_DLL_NOT_LINKED

0x80093024

OSS_COMPARATOR_DLL_NOT_LINKED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

OSS ASN.1 Error: Internal
error.

OSS ASN.1 Error:
Multithreading conflict.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error:
Encode/Decode function not
implemented.

OSS ASN.1 Error: Trace file
error.

OSS ASN.1 Error: Function not
implemented.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Trace file
error.

OSS ASN.1 Error: Trace file
error.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Invalid data.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

60 / 497


Return value/code

0x80093025

OSS_COMPARATOR_CODE_NOT_LINKED

0x80093026

OSS_MEM_MGR_DLL_NOT_LINKED

0x80093027

OSS_PDV_DLL_NOT_LINKED

0x80093028

OSS_PDV_CODE_NOT_LINKED

0x80093029

OSS_API_DLL_NOT_LINKED

0x8009302A

OSS_BERDER_DLL_NOT_LINKED

0x8009302B

OSS_PER_DLL_NOT_LINKED

0x8009302C

OSS_OPEN_TYPE_ERROR

0x8009302D

OSS_MUTEX_NOT_CREATED

0x8009302E

OSS_CANT_CLOSE_TRACE_FILE

0x80093100

CRYPT_E_ASN1_ERROR

0x80093101

CRYPT_E_ASN1_INTERNAL

0x80093102

CRYPT_E_ASN1_EOD

0x80093103

CRYPT_E_ASN1_CORRUPT

0x80093104

CRYPT_E_ASN1_LARGE

0x80093105

CRYPT_E_ASN1_CONSTRAINT

0x80093106

CRYPT_E_ASN1_MEMORY

0x80093107

CRYPT_E_ASN1_OVERFLOW

0x80093108

CRYPT_E_ASN1_BADPDU

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: Program link
error.

OSS ASN.1 Error: System
resource error.

OSS ASN.1 Error: Trace file
error.

ASN1 Certificate encode/decode
error code base.

ASN1 internal encode or decode
error.

ASN1 unexpected end of data.

ASN1 corrupted data.

ASN1 value too large.

ASN1 constraint violated.

ASN1 out of memory.

ASN1 buffer overflow.

ASN1 function not supported for
this protocol data unit (PDU).

61 / 497


Return value/code

0x80093109

CRYPT_E_ASN1_BADARGS

0x8009310A

CRYPT_E_ASN1_BADREAL

0x8009310B

CRYPT_E_ASN1_BADTAG

0x8009310C

CRYPT_E_ASN1_CHOICE

0x8009310D

CRYPT_E_ASN1_RULE

0x8009310E

CRYPT_E_ASN1_UTF8

0x80093133

CRYPT_E_ASN1_PDU_TYPE

0x80093134

CRYPT_E_ASN1_NYI

0x80093201

CRYPT_E_ASN1_EXTENDED

0x80093202

CRYPT_E_ASN1_NOEOD

0x80094001

CERTSRV_E_BAD_REQUESTSUBJECT

0x80094002

CERTSRV_E_NO_REQUEST

0x80094003

CERTSRV_E_BAD_REQUESTSTATUS

0x80094004

CERTSRV_E_PROPERTY_EMPTY

0x80094005

CERTSRV_E_INVALID_CA_CERTIFICATE

0x80094006

CERTSRV_E_SERVER_SUSPENDED

0x80094007

CERTSRV_E_ENCODING_LENGTH

0x80094008

CERTSRV_E_ROLECONFLICT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

ASN1 bad arguments to
function call.

ASN1 bad real value.

ASN1 bad tag value met.

ASN1 bad choice value.

ASN1 bad encoding rule.

ASN1 bad Unicode (UTF8).

ASN1 bad PDU type.

ASN1 not yet implemented.

ASN1 skipped unknown
extensions.

ASN1 end of data expected.

The request subject name is
invalid or too long.

The request does not exist.

The request's current status
does not allow this operation.

The requested property value is
empty.

The CA's certificate contains
invalid data.

Certificate service has been
suspended for a database
restore operation.

The certificate contains an
encoded length that is
potentially incompatible with
older enrollment software.

The operation is denied. The
user has multiple roles
assigned, and the CA is
configured to enforce role

62 / 497


Return value/code

0x80094009

CERTSRV_E_RESTRICTEDOFFICER

0x8009400A

CERTSRV_E_KEY_ARCHIVAL_NOT_CONFIGURED

0x8009400B

CERTSRV_E_NO_VALID_KRA

0x8009400C

CERTSRV_E_BAD_REQUEST_KEY_ARCHIVAL

0x8009400D

CERTSRV_E_NO_CAADMIN_DEFINED

0x8009400E

CERTSRV_E_BAD_RENEWAL_CERT_ATTRIBUTE

0x8009400F

CERTSRV_E_NO_DB_SESSIONS

0x80094010

CERTSRV_E_ALIGNMENT_FAULT

0x80094011

CERTSRV_E_ENROLL_DENIED

0x80094012

CERTSRV_E_TEMPLATE_DENIED

0x80094013

CERTSRV_E_DOWNLEVEL_DC_SSL_OR_UPGRADE

0x80094800

CERTSRV_E_UNSUPPORTED_CERT_TYPE

0x80094801

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

separation.

The operation is denied. It can
only be performed by a
certificate manager that is
allowed to manage certificates
for the current requester.

Cannot archive private key. The
CA is not configured for key
archival.

Cannot archive private key. The
CA could not verify one or more
key recovery certificates.

The request is incorrectly
formatted. The encrypted
private key must be in an
unauthenticated attribute in an
outermost signature.

At least one security principal
must have the permission to
manage this CA.

The request contains an invalid
renewal certificate attribute.

An attempt was made to open a
CA database session, but there
are already too many active
sessions. The server needs to
be configured to allow
additional sessions.

A memory reference caused a
data alignment fault.

The permissions on this CA do
not allow the current user to
enroll for certificates.

The permissions on the
certificate template do not allow
the current user to enroll for
this type of certificate.

The contacted domain controller
cannot support signed
Lightweight Directory Access
Protocol (LDAP) traffic. Update
the domain controller or
configure Certificate Services to
use SSL for Active Directory
access.

The requested certificate
template is not supported by
this CA.

The request contains no

63 / 497


Return value/code

CERTSRV_E_NO_CERT_TYPE

0x80094802

CERTSRV_E_TEMPLATE_CONFLICT

0x80094803

CERTSRV_E_SUBJECT_ALT_NAME_REQUIRED

0x80094804

CERTSRV_E_ARCHIVED_KEY_REQUIRED

0x80094805

CERTSRV_E_SMIME_REQUIRED

0x80094806

CERTSRV_E_BAD_RENEWAL_SUBJECT

0x80094807

CERTSRV_E_BAD_TEMPLATE_VERSION

0x80094808

CERTSRV_E_TEMPLATE_POLICY_REQUIRED

0x80094809

CERTSRV_E_SIGNATURE_POLICY_REQUIRED

0x8009480A

CERTSRV_E_SIGNATURE_COUNT

0x8009480B

CERTSRV_E_SIGNATURE_REJECTED

0x8009480C

CERTSRV_E_ISSUANCE_POLICY_REQUIRED

0x8009480D

CERTSRV_E_SUBJECT_UPN_REQUIRED

0x8009480E

CERTSRV_E_SUBJECT_DIRECTORY_GUID_REQUIRED

0x8009480F

CERTSRV_E_SUBJECT_DNS_REQUIRED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

certificate template information.

The request contains conflicting
template information.

The request is missing a
required Subject Alternate
name extension.

The request is missing a
required private key for archival
by the server.

The request is missing a
required SMIME capabilities
extension.

The request was made on
behalf of a subject other than
the caller. The certificate
template must be configured to
require at least one signature to
authorize the request.

The request template version is
newer than the supported
template version.

The template is missing a
required signature policy
attribute.

The request is missing required
signature policy information.

The request is missing one or
more required signatures.

One or more signatures did not
include the required application
or issuance policies. The
request is missing one or more
required valid signatures.

The request is missing one or
more required signature
issuance policies.

The UPN is unavailable and
cannot be added to the Subject
Alternate name.

The Active Directory GUID is
unavailable and cannot be
added to the Subject Alternate
name.

The Domain Name System
(DNS) name is unavailable and
cannot be added to the Subject
Alternate name.

64 / 497


Return value/code

0x80094810

CERTSRV_E_ARCHIVED_KEY_UNEXPECTED

0x80094811

CERTSRV_E_KEY_LENGTH

0x80094812

CERTSRV_E_SUBJECT_EMAIL_REQUIRED

0x80094813

CERTSRV_E_UNKNOWN_CERT_TYPE

0x80094814

CERTSRV_E_CERT_TYPE_OVERLAP

0x80094815

CERTSRV_E_TOO_MANY_SIGNATURES

0x80094816

CERTSRV_E_RENEWAL_BAD_PUBLIC_KEY

0x80094817

CERTSRV_E_INVALID_EK

0x8009481A

CERTSRV_E_KEY_ATTESTATION

0x80095000

XENROLL_E_KEY_NOT_EXPORTABLE

0x80095001

XENROLL_E_CANNOT_ADD_ROOT_CERT

0x80095002

XENROLL_E_RESPONSE_KA_HASH_NOT_FOUND

0x80095003

XENROLL_E_RESPONSE_UNEXPECTED_KA_HASH

0x80095004

XENROLL_E_RESPONSE_KA_HASH_MISMATCH

0x80095005

XENROLL_E_KEYSPEC_SMIME_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The request includes a private
key for archival by the server,
but key archival is not enabled
for the specified certificate
template.

The public key does not meet
the minimum size required by
the specified certificate
template.

The email name is unavailable
and cannot be added to the
Subject or Subject Alternate
name.

One or more certificate
templates to be enabled on this
CA could not be found.

The certificate template renewal
period is longer than the
certificate validity period. The
template should be reconfigured
or the CA certificate renewed.

The certificate template
requires too many return
authorization (RA) signatures.
Only one RA signature is
allowed.

The key used in a renewal
request does not match one of
the certificates being renewed.

The endorsement key certificate
is not valid.

Key attestation did not succeed.

The key is not exportable.

You cannot add the root CA
certificate into your local store.

The key archival hash attribute
was not found in the response.

An unexpected key archival
hash attribute was found in the
response.

There is a key archival hash
mismatch between the request
and the response.

Signing certificate cannot
include SMIME extension.

65 / 497


Return value/code

0x80096001

TRUST_E_SYSTEM_ERROR

0x80096002

TRUST_E_NO_SIGNER_CERT

0x80096003

TRUST_E_COUNTER_SIGNER

0x80096004

TRUST_E_CERT_SIGNATURE

0x80096005

TRUST_E_TIME_STAMP

0x80096010

TRUST_E_BAD_DIGEST

0x80096019

TRUST_E_BASIC_CONSTRAINTS

0x8009601E

TRUST_E_FINANCIAL_CRITERIA

0x80097001

MSSIPOTF_E_OUTOFMEMRANGE

0x80097002

MSSIPOTF_E_CANTGETOBJECT

0x80097003

MSSIPOTF_E_NOHEADTABLE

0x80097004

MSSIPOTF_E_BAD_MAGICNUMBER

0x80097005

MSSIPOTF_E_BAD_OFFSET_TABLE

0x80097006

MSSIPOTF_E_TABLE_TAGORDER

0x80097007

MSSIPOTF_E_TABLE_LONGWORD

0x80097008

MSSIPOTF_E_BAD_FIRST_TABLE_PLACEMENT

0x80097009

MSSIPOTF_E_TABLES_OVERLAP

0x8009700A

MSSIPOTF_E_TABLE_PADBYTES

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A system-level error occurred
while verifying trust.

The certificate for the signer of
the message is invalid or not
found.

One of the counter signatures
was invalid.

The signature of the certificate
cannot be verified.

The time-stamp signature or
certificate could not be verified
or is malformed.

The digital signature of the
object did not verify.

A certificate's basic constraint
extension has not been
observed.

The certificate does not meet or
contain the Authenticode
financial extensions.

Tried to reference a part of the
file outside the proper range.

Could not retrieve an object
from the file.

Could not find the head table in
the file.

The magic number in the head
table is incorrect.

The offset table has incorrect
values.

Duplicate table tags or the tags
are out of alphabetical order.

A table does not start on a long
word boundary.

First table does not appear after
header information.

Two or more tables overlap.

Too many pad bytes between
tables, or pad bytes are not 0.

66 / 497


Return value/code

0x8009700B

MSSIPOTF_E_FILETOOSMALL

0x8009700C

MSSIPOTF_E_TABLE_CHECKSUM

0x8009700D

MSSIPOTF_E_FILE_CHECKSUM

0x80097010

MSSIPOTF_E_FAILED_POLICY

0x80097011

MSSIPOTF_E_FAILED_HINTS_CHECK

0x80097012

MSSIPOTF_E_NOT_OPENTYPE

0x80097013

MSSIPOTF_E_FILE

0x80097014

MSSIPOTF_E_CRYPT

0x80097015

MSSIPOTF_E_BADVERSION

0x80097016

MSSIPOTF_E_DSIG_STRUCTURE

0x80097017

MSSIPOTF_E_PCONST_CHECK

0x80097018

MSSIPOTF_E_STRUCTURE

0x80097019

ERROR_CRED_REQUIRES_CONFIRMATION

0x800B0001

TRUST_E_PROVIDER_UNKNOWN

0x800B0002

TRUST_E_ACTION_UNKNOWN

0x800B0003

TRUST_E_SUBJECT_FORM_UNKNOWN

0x800B0004

TRUST_E_SUBJECT_NOT_TRUSTED

0x800B0005

DIGSIG_E_ENCODE

0x800B0006

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

File is too small to contain the
last table.

A table checksum is incorrect.

The file checksum is incorrect.

The signature does not have the
correct attributes for the policy.

The file did not pass the hints
check.

The file is not an OpenType file.

Failed on a file operation (such
as open, map, read, or write).

A call to a CryptoAPI function
failed.

There is a bad version number
in the file.

The structure of the DSIG table
is incorrect.

A check failed in a partially
constant table.

Some kind of structural error.

The requested credential
requires confirmation.

Unknown trust provider.

The trust verification action
specified is not supported by
the specified trust provider.

The form specified for the
subject is not one supported or
known by the specified trust
provider.

The subject is not trusted for
the specified action.

Error due to problem in ASN.1
encoding process.

Error due to problem in ASN.1

67 / 497


Return value/code

DIGSIG_E_DECODE

0x800B0007

DIGSIG_E_EXTENSIBILITY

0x800B0008

DIGSIG_E_CRYPTO

0x800B0009

PERSIST_E_SIZEDEFINITE

0x800B000A

PERSIST_E_SIZEINDEFINITE

0x800B000B

PERSIST_E_NOTSELFSIZING

0x800B0100

TRUST_E_NOSIGNATURE

0x800B0101

CERT_E_EXPIRED

0x800B0102

CERT_E_VALIDITYPERIODNESTING

0x800B0103

CERT_E_ROLE

0x800B0104

CERT_E_PATHLENCONST

0x800B0105

CERT_E_CRITICAL

0x800B0106

CERT_E_PURPOSE

0x800B0107

CERT_E_ISSUERCHAINING

0x800B0108

CERT_E_MALFORMED

0x800B0109

CERT_E_UNTRUSTEDROOT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

decoding process.

Reading/writing extensions
where attributes are
appropriate, and vice versa.

Unspecified cryptographic
failure.

The size of the data could not
be determined.

The size of the indefinite-sized
data could not be determined.

This object does not read and
write self-sizing data.

No signature was present in the
subject.

A required certificate is not
within its validity period when
verifying against the current
system clock or the time stamp
in the signed file.

The validity periods of the
certification chain do not nest
correctly.

A certificate that can only be
used as an end entity is being
used as a CA or vice versa.

A path length constraint in the
certification chain has been
violated.

A certificate contains an
unknown extension that is
marked "critical".

A certificate is being used for a
purpose other than the ones
specified by its CA.

A parent of a given certificate
did not issue that child
certificate.

A certificate is missing or has
an empty value for an
important field, such as a
subject or issuer name.

A certificate chain processed,
but terminated in a root
certificate that is not trusted by
the trust provider.

68 / 497


Return value/code

0x800B010A

CERT_E_CHAINING

0x800B010B

TRUST_E_FAIL

0x800B010C

CERT_E_REVOKED

0x800B010D

CERT_E_UNTRUSTEDTESTROOT

0x800B010E

CERT_E_REVOCATION_FAILURE

0x800B010F

CERT_E_CN_NO_MATCH

0x800B0110

CERT_E_WRONG_USAGE

0x800B0111

TRUST_E_EXPLICIT_DISTRUST

0x800B0112

CERT_E_UNTRUSTEDCA

0x800B0113

CERT_E_INVALID_POLICY

0x800B0114

CERT_E_INVALID_NAME

0x800D0003

NS_W_SERVER_BANDWIDTH_LIMIT

0x800D0004

NS_W_FILE_BANDWIDTH_LIMIT

0x800D0060

NS_W_UNKNOWN_EVENT

0x800D0199

NS_I_CATATONIC_FAILURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A certificate chain could not be
built to a trusted root authority.

Generic trust failure.

A certificate was explicitly
revoked by its issuer. If the
certificate is Microsoft Windows
PCA 2010, then the driver was
signed by a certificate no longer
recognized by Windows.<3>

The certification path
terminates with the test root
that is not trusted with the
current policy settings.

The revocation process could
not continue—the certificates
could not be checked.

The certificate's CN name does
not match the passed value.

The certificate is not valid for
the requested usage.

The certificate was explicitly
marked as untrusted by the
user.

A certification chain processed
correctly, but one of the CA
certificates is not trusted by the
policy provider.

The certificate has invalid
policy.

The certificate has an invalid
name. The name is not included
in the permitted list or is
explicitly excluded.

The maximum filebitrate value
specified is greater than the
server's configured maximum
bandwidth.

The maximum bandwidth value
specified is less than the
maximum filebitrate.

Unknown %1 event
encountered.

Disk %1 ( %2 ) on Content
Server %3, will be failed
because it is catatonic.

69 / 497


Return value/code

0x800D019A

NS_I_CATATONIC_AUTO_UNFAIL

0x800F0000

SPAPI_E_EXPECTED_SECTION_NAME

0x800F0001

SPAPI_E_BAD_SECTION_NAME_LINE

0x800F0002

SPAPI_E_SECTION_NAME_TOO_LONG

0x800F0003

SPAPI_E_GENERAL_SYNTAX

0x800F0100

SPAPI_E_WRONG_INF_STYLE

0x800F0101

SPAPI_E_SECTION_NOT_FOUND

0x800F0102

SPAPI_E_LINE_NOT_FOUND

0x800F0103

SPAPI_E_NO_BACKUP

0x800F0200

SPAPI_E_NO_ASSOCIATED_CLASS

0x800F0201

SPAPI_E_CLASS_MISMATCH

0x800F0202

SPAPI_E_DUPLICATE_FOUND

0x800F0203

SPAPI_E_NO_DRIVER_SELECTED

0x800F0204

SPAPI_E_KEY_DOES_NOT_EXIST

0x800F0205

SPAPI_E_INVALID_DEVINST_NAME

0x800F0206

SPAPI_E_INVALID_CLASS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Disk %1 ( %2 ) on Content
Server %3, auto online from
catatonic state.

A non-empty line was
encountered in the INF before
the start of a section.

A section name marker in the
information file (INF) is not
complete or does not exist on a
line by itself.

An INF section was encountered
whose name exceeds the
maximum section name length.

The syntax of the INF is invalid.

The style of the INF is different
than what was requested.

The required section was not
found in the INF.

The required line was not found
in the INF.

The files affected by the
installation of this file queue
have not been backed up for
uninstall.

The INF or the device
information set or element does
not have an associated install
class.

The INF or the device
information set or element does
not match the specified install
class.

An existing device was found
that is a duplicate of the device
being manually installed.

There is no driver selected for
the device information set or
element.

The requested device registry
key does not exist.

The device instance name is
invalid.

The install class is not present
or is invalid.

70 / 497


Return value/code

0x800F0207

SPAPI_E_DEVINST_ALREADY_EXISTS

0x800F0208

SPAPI_E_DEVINFO_NOT_REGISTERED

0x800F0209

SPAPI_E_INVALID_REG_PROPERTY

0x800F020A

SPAPI_E_NO_INF

0x800F020B

SPAPI_E_NO_SUCH_DEVINST

0x800F020C

SPAPI_E_CANT_LOAD_CLASS_ICON

0x800F020D

SPAPI_E_INVALID_CLASS_INSTALLER

0x800F020E

SPAPI_E_DI_DO_DEFAULT

0x800F020F

SPAPI_E_DI_NOFILECOPY

0x800F0210

SPAPI_E_INVALID_HWPROFILE

0x800F0211

SPAPI_E_NO_DEVICE_SELECTED

0x800F0212

SPAPI_E_DEVINFO_LIST_LOCKED

0x800F0213

SPAPI_E_DEVINFO_DATA_LOCKED

0x800F0214

SPAPI_E_DI_BAD_PATH

0x800F0215

SPAPI_E_NO_CLASSINSTALL_PARAMS

0x800F0216

SPAPI_E_FILEQUEUE_LOCKED

0x800F0217

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The device instance cannot be
created because it already
exists.

The operation cannot be
performed on a device
information element that has
not been registered.

The device property code is
invalid.

The INF from which a driver list
is to be built does not exist.

The device instance does not
exist in the hardware tree.

The icon representing this
install class cannot be loaded.

The class installer registry entry
is invalid.

The class installer has indicated
that the default action should
be performed for this
installation request.

The operation does not require
any files to be copied.

The specified hardware profile
does not exist.

There is no device information
element currently selected for
this device information set.

The operation cannot be
performed because the device
information set is locked.

The operation cannot be
performed because the device
information element is locked.

The specified path does not
contain any applicable device
INFs.

No class installer parameters
have been set for the device
information set or element.

The operation cannot be
performed because the file
queue is locked.

A service installation section in

71 / 497


Return value/code

SPAPI_E_BAD_SERVICE_INSTALLSECT

0x800F0218

SPAPI_E_NO_CLASS_DRIVER_LIST

0x800F0219

SPAPI_E_NO_ASSOCIATED_SERVICE

0x800F021A

SPAPI_E_NO_DEFAULT_DEVICE_INTERFACE

0x800F021B

SPAPI_E_DEVICE_INTERFACE_ACTIVE

0x800F021C

SPAPI_E_DEVICE_INTERFACE_REMOVED

0x800F021D

SPAPI_E_BAD_INTERFACE_INSTALLSECT

0x800F021E

SPAPI_E_NO_SUCH_INTERFACE_CLASS

0x800F021F

SPAPI_E_INVALID_REFERENCE_STRING

0x800F0220

SPAPI_E_INVALID_MACHINENAME

0x800F0221

SPAPI_E_REMOTE_COMM_FAILURE

0x800F0222

SPAPI_E_MACHINE_UNAVAILABLE

0x800F0223

SPAPI_E_NO_CONFIGMGR_SERVICES

0x800F0224

SPAPI_E_INVALID_PROPPAGE_PROVIDER

0x800F0225

SPAPI_E_NO_SUCH_DEVICE_INTERFACE

0x800F0226

SPAPI_E_DI_POSTPROCESSING_REQUIRED

0x800F0227

SPAPI_E_INVALID_COINSTALLER

0x800F0228

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

this INF is invalid.

There is no class driver list for
the device information element.

The installation failed because a
function driver was not specified
for this device instance.

There is presently no default
device interface designated for
this interface class.

The operation cannot be
performed because the device
interface is currently active.

The operation cannot be
performed because the device
interface has been removed
from the system.

An interface installation section
in this INF is invalid.

This interface class does not
exist in the system.

The reference string supplied
for this interface device is
invalid.

The specified machine name
does not conform to Universal
Naming Convention (UNCs).

A general remote
communication error occurred.

The machine selected for
remote communication is not
available at this time.

The Plug and Play service is not
available on the remote
machine.

The property page provider
registry entry is invalid.

The requested device interface
is not present in the system.

The device's co-installer has
additional work to perform after
installation is complete.

The device's co-installer is
invalid.

There are no compatible drivers

72 / 497


Return value/code

SPAPI_E_NO_COMPAT_DRIVERS

0x800F0229

SPAPI_E_NO_DEVICE_ICON

0x800F022A

SPAPI_E_INVALID_INF_LOGCONFIG

0x800F022B

SPAPI_E_DI_DONT_INSTALL

0x800F022C

SPAPI_E_INVALID_FILTER_DRIVER

0x800F022D

SPAPI_E_NON_WINDOWS_NT_DRIVER

0x800F022E

SPAPI_E_NON_WINDOWS_DRIVER

0x800F022F

SPAPI_E_NO_CATALOG_FOR_OEM_INF

0x800F0230

SPAPI_E_DEVINSTALL_QUEUE_NONNATIVE

0x800F0231

SPAPI_E_NOT_DISABLEABLE

0x800F0232

SPAPI_E_CANT_REMOVE_DEVINST

0x800F0233

SPAPI_E_INVALID_TARGET

0x800F0234

SPAPI_E_DRIVER_NONNATIVE

0x800F0235

SPAPI_E_IN_WOW64

0x800F0236

SPAPI_E_SET_SYSTEM_RESTORE_POINT

0x800F0237

SPAPI_E_INCORRECTLY_COPIED_INF

0x800F0238

SPAPI_E_SCE_DISABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

for this device.

There is no icon that represents
this device or device type.

A logical configuration specified
in this INF is invalid.

The class installer has denied
the request to install or upgrade
this device.

One of the filter drivers installed
for this device is invalid.

The driver selected for this
device does not support
Windows XP operating system.

The driver selected for this
device does not support
Windows.

The third-party INF does not
contain digital signature
information.

An invalid attempt was made to
use a device installation file
queue for verification of digital
signatures relative to other
platforms.

The device cannot be disabled.

The device could not be
dynamically removed.

Cannot copy to specified target.

Driver is not intended for this
platform.

Operation not allowed in
WOW64.

The operation involving
unsigned file copying was rolled
back, so that a system restore
point could be set.

An INF was copied into the
Windows INF directory in an
improper manner.

The Security Configuration
Editor (SCE) APIs have been
disabled on this embedded

73 / 497


Return value/code

0x800F0239

SPAPI_E_UNKNOWN_EXCEPTION

0x800F023A

SPAPI_E_PNP_REGISTRY_ERROR

0x800F023B

SPAPI_E_REMOTE_REQUEST_UNSUPPORTED

0x800F023C

SPAPI_E_NOT_AN_INSTALLED_OEM_INF

0x800F023D

SPAPI_E_INF_IN_USE_BY_DEVICES

0x800F023E

SPAPI_E_DI_FUNCTION_OBSOLETE

0x800F023F

SPAPI_E_NO_AUTHENTICODE_CATALOG

0x800F0240

SPAPI_E_AUTHENTICODE_DISALLOWED

0x800F0241

SPAPI_E_AUTHENTICODE_TRUSTED_PUBLISHER

0x800F0242

SPAPI_E_AUTHENTICODE_TRUST_NOT_ESTABLISHED

0x800F0243

SPAPI_E_AUTHENTICODE_PUBLISHER_NOT_TRUSTED

0x800F0244

SPAPI_E_SIGNATURE_OSATTRIBUTE_MISMATCH

0x800F0245

SPAPI_E_ONLY_VALIDATE_VIA_AUTHENTICODE

0x800F0246

SPAPI_E_DEVICE_INSTALLER_NOT_READY

0x800F0247

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

product.

An unknown exception was
encountered.

A problem was encountered
when accessing the Plug and
Play registry database.

The requested operation is not
supported for a remote
machine.

The specified file is not an
installed original equipment
manufacturer (OEM) INF.

One or more devices are
presently installed using the
specified INF.

The requested device install
operation is obsolete.

A file could not be verified
because it does not have an
associated catalog signed via
Authenticode.

Authenticode signature
verification is not supported for
the specified INF.

The INF was signed with an
Authenticode catalog from a
trusted publisher.

The publisher of an
Authenticode-signed catalog
has not yet been established as
trusted.

The publisher of an
Authenticode-signed catalog
was not established as trusted.

The software was tested for
compliance with Windows logo
requirements on a different
version of Windows and might
not be compatible with this
version.

The file can be validated only by
a catalog signed via
Authenticode.

One of the installers for this
device cannot perform the
installation at this time.

A problem was encountered
while attempting to add the

74 / 497


Return value/code

SPAPI_E_DRIVER_STORE_ADD_FAILED

0x800F0248

SPAPI_E_DEVICE_INSTALL_BLOCKED

0x800F0249

SPAPI_E_DRIVER_INSTALL_BLOCKED

0x800F024A

SPAPI_E_WRONG_INF_TYPE

0x800F024B

SPAPI_E_FILE_HASH_NOT_IN_CATALOG

0x800F024C

SPAPI_E_DRIVER_STORE_DELETE_FAILED

0x800F0300

SPAPI_E_UNRECOVERABLE_STACK_OVERFLOW

0x800F1000

SPAPI_E_ERROR_NOT_INSTALLED

0x80100001

SCARD_F_INTERNAL_ERROR

0x80100002

SCARD_E_CANCELLED

0x80100003

SCARD_E_INVALID_HANDLE

0x80100004

SCARD_E_INVALID_PARAMETER

0x80100005

SCARD_E_INVALID_TARGET

0x80100006

SCARD_E_NO_MEMORY

0x80100007

SCARD_F_WAITED_TOO_LONG

0x80100008

SCARD_E_INSUFFICIENT_BUFFER

0x80100009

SCARD_E_UNKNOWN_READER

0x8010000A

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

driver to the store.

The installation of this device is
forbidden by system policy.
Contact your system
administrator.

The installation of this driver is
forbidden by system policy.
Contact your system
administrator.

The specified INF is the wrong
type for this operation.

The hash for the file is not
present in the specified catalog
file. The file is likely corrupt or
the victim of tampering.

A problem was encountered
while attempting to delete the
driver from the store.

An unrecoverable stack
overflow was encountered.

No installed components were
detected.

An internal consistency check
failed.

The action was canceled by an
SCardCancel request.

The supplied handle was invalid.

One or more of the supplied
parameters could not be
properly interpreted.

Registry startup information is
missing or invalid.

Not enough memory available
to complete this command.

An internal consistency timer
has expired.

The data buffer to receive
returned data is too small for
the returned data.

The specified reader name is
not recognized.

The user-specified time-out

75 / 497


Return value/code

SCARD_E_TIMEOUT

0x8010000B

SCARD_E_SHARING_VIOLATION

0x8010000C

SCARD_E_NO_SMARTCARD

0x8010000D

SCARD_E_UNKNOWN_CARD

0x8010000E

SCARD_E_CANT_DISPOSE

0x8010000F

SCARD_E_PROTO_MISMATCH

0x80100010

SCARD_E_NOT_READY

0x80100011

SCARD_E_INVALID_VALUE

0x80100012

SCARD_E_SYSTEM_CANCELLED

0x80100013

SCARD_F_COMM_ERROR

0x80100014

SCARD_F_UNKNOWN_ERROR

0x80100015

SCARD_E_INVALID_ATR

0x80100016

SCARD_E_NOT_TRANSACTED

0x80100017

SCARD_E_READER_UNAVAILABLE

0x80100018

SCARD_P_SHUTDOWN

0x80100019

SCARD_E_PCI_TOO_SMALL

0x8010001A

SCARD_E_READER_UNSUPPORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

value has expired.

The smart card cannot be
accessed because of other
connections outstanding.

The operation requires a smart
card, but no smart card is
currently in the device.

The specified smart card name
is not recognized.

The system could not dispose of
the media in the requested
manner.

The requested protocols are
incompatible with the protocol
currently in use with the smart
card.

The reader or smart card is not
ready to accept commands.

One or more of the supplied
parameters values could not be
properly interpreted.

The action was canceled by the
system, presumably to log off
or shut down.

An internal communications
error has been detected.

An internal error has been
detected, but the source is
unknown.

An automatic terminal
recognition (ATR) obtained from
the registry is not a valid ATR
string.

An attempt was made to end a
nonexistent transaction.

The specified reader is not
currently available for use.

The operation has been aborted
to allow the server application
to exit.

The peripheral component
interconnect (PCI) Receive
buffer was too small.

The reader driver does not meet
minimal requirements for

76 / 497


Return value/code

0x8010001B

SCARD_E_DUPLICATE_READER

0x8010001C

SCARD_E_CARD_UNSUPPORTED

0x8010001D

SCARD_E_NO_SERVICE

0x8010001E

SCARD_E_SERVICE_STOPPED

0x8010001F

SCARD_E_UNEXPECTED

0x80100020

SCARD_E_ICC_INSTALLATION

0x80100021

SCARD_E_ICC_CREATEORDER

0x80100022

SCARD_E_UNSUPPORTED_FEATURE

0x80100023

SCARD_E_DIR_NOT_FOUND

0x80100024

SCARD_E_FILE_NOT_FOUND

0x80100025

SCARD_E_NO_DIR

0x80100026

SCARD_E_NO_FILE

0x80100027

SCARD_E_NO_ACCESS

0x80100028

SCARD_E_WRITE_TOO_MANY

0x80100029

SCARD_E_BAD_SEEK

0x8010002A

SCARD_E_INVALID_CHV

0x8010002B

SCARD_E_UNKNOWN_RES_MNG

0x8010002C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

support.

The reader driver did not
produce a unique reader name.

The smart card does not meet
minimal requirements for
support.

The smart card resource
manager is not running.

The smart card resource
manager has shut down.

An unexpected card error has
occurred.

No primary provider can be
found for the smart card.

The requested order of object
creation is not supported.

This smart card does not
support the requested feature.

The identified directory does not
exist in the smart card.

The identified file does not exist
in the smart card.

The supplied path does not
represent a smart card
directory.

The supplied path does not
represent a smart card file.

Access is denied to this file.

The smart card does not have
enough memory to store the
information.

There was an error trying to set
the smart card file object
pointer.

The supplied PIN is incorrect.

An unrecognized error code was
returned from a layered
component.

The requested certificate does

77 / 497


Return value/code

SCARD_E_NO_SUCH_CERTIFICATE

0x8010002D

SCARD_E_CERTIFICATE_UNAVAILABLE

0x8010002E

SCARD_E_NO_READERS_AVAILABLE

0x8010002F

SCARD_E_COMM_DATA_LOST

0x80100030

SCARD_E_NO_KEY_CONTAINER

0x80100031

SCARD_E_SERVER_TOO_BUSY

0x80100065

SCARD_W_UNSUPPORTED_CARD

0x80100066

SCARD_W_UNRESPONSIVE_CARD

0x80100067

SCARD_W_UNPOWERED_CARD

0x80100068

SCARD_W_RESET_CARD

0x80100069

SCARD_W_REMOVED_CARD

0x8010006A

SCARD_W_SECURITY_VIOLATION

0x8010006B

SCARD_W_WRONG_CHV

0x8010006C

SCARD_W_CHV_BLOCKED

0x8010006D

SCARD_W_EOF

0x8010006E

SCARD_W_CANCELLED_BY_USER

0x8010006F

SCARD_W_CARD_NOT_AUTHENTICATED

0x80110401

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

not exist.

The requested certificate could
not be obtained.

Cannot find a smart card
reader.

A communications error with
the smart card has been
detected. Retry the operation.

The requested key container
does not exist on the smart
card.

The smart card resource
manager is too busy to
complete this operation.

The reader cannot communicate
with the smart card, due to ATR
configuration conflicts.

The smart card is not
responding to a reset.

Power has been removed from
the smart card, so that further
communication is not possible.

The smart card has been reset,
so any shared state information
is invalid.

The smart card has been
removed, so that further
communication is not possible.

Access was denied because of a
security violation.

The card cannot be accessed
because the wrong PIN was
presented.

The card cannot be accessed
because the maximum number
of PIN entry attempts has been
reached.

The end of the smart card file
has been reached.

The action was canceled by the
user.

No PIN was presented to the
smart card.

Errors occurred accessing one

78 / 497


Return value/code

COMADMIN_E_OBJECTERRORS

0x80110402

COMADMIN_E_OBJECTINVALID

0x80110403

COMADMIN_E_KEYMISSING

0x80110404

COMADMIN_E_ALREADYINSTALLED

0x80110407

COMADMIN_E_APP_FILE_WRITEFAIL

0x80110408

COMADMIN_E_APP_FILE_READFAIL

0x80110409

COMADMIN_E_APP_FILE_VERSION

0x8011040A

COMADMIN_E_BADPATH

0x8011040B

COMADMIN_E_APPLICATIONEXISTS

0x8011040C

COMADMIN_E_ROLEEXISTS

0x8011040D

COMADMIN_E_CANTCOPYFILE

0x8011040F

COMADMIN_E_NOUSER

0x80110410

COMADMIN_E_INVALIDUSERIDS

0x80110411

COMADMIN_E_NOREGISTRYCLSID

0x80110412

COMADMIN_E_BADREGISTRYPROGID

0x80110413

COMADMIN_E_AUTHENTICATIONLEVEL

0x80110414

COMADMIN_E_USERPASSWDNOTVALID

0x80110418

COMADMIN_E_CLSIDORIIDMISMATCH

0x80110419

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

or more objects—the ErrorInfo
collection contains more detail.

One or more of the object's
properties are missing or
invalid.

The object was not found in the
catalog.

The object is already registered.

An error occurred writing to the
application file.

An error occurred reading the
application file.

Invalid version number in
application file.

The file path is invalid.

The application is already
installed.

The role already exists.

An error occurred copying the
file.

One or more users are not
valid.

One or more users in the
application file are not valid.

The component's CLSID is
missing or corrupt.

The component's programmatic
ID is missing or corrupt.

Unable to set required
authentication level for update
request.

The identity or password set on
the application is not valid.

Application file CLSIDs or
instance identifiers (IIDs) do
not match corresponding DLLs.

Interface information is either
missing or changed.

79 / 497


Return value/code

COMADMIN_E_REMOTEINTERFACE

0x8011041A

COMADMIN_E_DLLREGISTERSERVER

0x8011041B

COMADMIN_E_NOSERVERSHARE

0x8011041D

COMADMIN_E_DLLLOADFAILED

0x8011041E

COMADMIN_E_BADREGISTRYLIBID

0x8011041F

COMADMIN_E_APPDIRNOTFOUND

0x80110423

COMADMIN_E_REGISTRARFAILED

0x80110424

COMADMIN_E_COMPFILE_DOESNOTEXIST

0x80110425

COMADMIN_E_COMPFILE_LOADDLLFAIL

0x80110426

COMADMIN_E_COMPFILE_GETCLASSOBJ

0x80110427

COMADMIN_E_COMPFILE_CLASSNOTAVAIL

0x80110428

COMADMIN_E_COMPFILE_BADTLB

0x80110429

COMADMIN_E_COMPFILE_NOTINSTALLABLE

0x8011042A

COMADMIN_E_NOTCHANGEABLE

0x8011042B

COMADMIN_E_NOTDELETEABLE

0x8011042C

COMADMIN_E_SESSION

0x8011042D

COMADMIN_E_COMP_MOVE_LOCKED

0x8011042E

COMADMIN_E_COMP_MOVE_BAD_DEST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

DllRegisterServer failed on
component install.

No server file share available.

DLL could not be loaded.

The registered TypeLib ID is not
valid.

Application install directory not
found.

Errors occurred while in the
component registrar.

The file does not exist.

The DLL could not be loaded.

GetClassObject failed in the
DLL.

The DLL does not support the
components listed in the
TypeLib.

The TypeLib could not be
loaded.

The file does not contain
components or component
information.

Changes to this object and its
subobjects have been disabled.

The delete function has been
disabled for this object.

The server catalog version is
not supported.

The component move was
disallowed because the source
or destination application is
either a system application or
currently locked against
changes.

The component move failed
because the destination
application no longer exists.

80 / 497


Return value/code

0x80110430

COMADMIN_E_REGISTERTLB

0x80110433

COMADMIN_E_SYSTEMAPP

0x80110434

COMADMIN_E_COMPFILE_NOREGISTRAR

0x80110435

COMADMIN_E_COREQCOMPINSTALLED

0x80110436

COMADMIN_E_SERVICENOTINSTALLED

0x80110437

COMADMIN_E_PROPERTYSAVEFAILED

0x80110438

COMADMIN_E_OBJECTEXISTS

0x80110439

COMADMIN_E_COMPONENTEXISTS

0x8011043B

COMADMIN_E_REGFILE_CORRUPT

0x8011043C

COMADMIN_E_PROPERTY_OVERFLOW

0x8011043E

COMADMIN_E_NOTINREGISTRY

0x8011043F

COMADMIN_E_OBJECTNOTPOOLABLE

0x80110446

COMADMIN_E_APPLID_MATCHES_CLSID

0x80110447

COMADMIN_E_ROLE_DOES_NOT_EXIST

0x80110448

COMADMIN_E_START_APP_NEEDS_COMPONENTS

0x80110449

COMADMIN_E_REQUIRES_DIFFERENT_PLATFORM

0x8011044A

COMADMIN_E_CAN_NOT_EXPORT_APP_PROXY

0x8011044B

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system was unable to
register the TypeLib.

This operation cannot be
performed on the system
application.

The component registrar
referenced in this file is not
available.

A component in the same DLL is
already installed.

The service is not installed.

One or more property settings
are either invalid or in conflict
with each other.

The object you are attempting
to add or rename already
exists.

The component already exists.

The registration file is corrupt.

The property value is too large.

Object was not found in
registry.

This object cannot be pooled.

A CLSID with the same GUID as
the new application ID is
already installed on this
machine.

A role assigned to a component,
interface, or method did not
exist in the application.

You must have components in
an application to start the
application.

This operation is not enabled on
this platform.

Application proxy is not
exportable.

Failed to start application

81 / 497


Return value/code

COMADMIN_E_CAN_NOT_START_APP

0x8011044C

COMADMIN_E_CAN_NOT_EXPORT_SYS_APP

0x8011044D

COMADMIN_E_CANT_SUBSCRIBE_TO_COMPONENT

0x8011044E

COMADMIN_E_EVENTCLASS_CANT_BE_SUBSCRIBER

0x8011044F

COMADMIN_E_LIB_APP_PROXY_INCOMPATIBLE

0x80110450

COMADMIN_E_BASE_PARTITION_ONLY

0x80110451

COMADMIN_E_START_APP_DISABLED

0x80110457

COMADMIN_E_CAT_DUPLICATE_PARTITION_NAME

0x80110458

COMADMIN_E_CAT_INVALID_PARTITION_NAME

0x80110459

COMADMIN_E_CAT_PARTITION_IN_USE

0x8011045A

COMADMIN_E_FILE_PARTITION_DUPLICATE_FILES

0x8011045B

COMADMIN_E_CAT_IMPORTED_COMPONENTS_NOT_ALLOWED

0x8011045C

COMADMIN_E_AMBIGUOUS_APPLICATION_NAME

0x8011045D

COMADMIN_E_AMBIGUOUS_PARTITION_NAME

0x80110472

COMADMIN_E_REGDB_NOTINITIALIZED

0x80110473

COMADMIN_E_REGDB_NOTOPEN

0x80110474

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

because it is either a library
application or an application
proxy.

System application is not
exportable.

Cannot subscribe to this
component (the component
might have been imported).

An event class cannot also be a
subscriber component.

Library applications and
application proxies are
incompatible.

This function is valid for the
base partition only.

You cannot start an application
that has been disabled.

The specified partition name is
already in use on this computer.

The specified partition name is
invalid. Check that the name
contains at least one visible
character.

The partition cannot be deleted
because it is the default
partition for one or more users.

The partition cannot be
exported because one or more
components in the partition
have the same file name.

Applications that contain one or
more imported components
cannot be installed into a
nonbase partition.

The application name is not
unique and cannot be resolved
to an application ID.

The partition name is not
unique and cannot be resolved
to a partition ID.

The COM+ registry database
has not been initialized.

The COM+ registry database is
not open.

The COM+ registry database

82 / 497


Return value/code

COMADMIN_E_REGDB_SYSTEMERR

0x80110475

COMADMIN_E_REGDB_ALREADYRUNNING

0x80110480

COMADMIN_E_MIG_VERSIONNOTSUPPORTED

0x80110481

COMADMIN_E_MIG_SCHEMANOTFOUND

0x80110482

COMADMIN_E_CAT_BITNESSMISMATCH

0x80110483

COMADMIN_E_CAT_UNACCEPTABLEBITNESS

0x80110484

COMADMIN_E_CAT_WRONGAPPBITNESS

0x80110485

COMADMIN_E_CAT_PAUSE_RESUME_NOT_SUPPORTED

0x80110486

COMADMIN_E_CAT_SERVERFAULT

0x80110600

COMQC_E_APPLICATION_NOT_QUEUED

0x80110601

COMQC_E_NO_QUEUEABLE_INTERFACES

0x80110602

COMQC_E_QUEUING_SERVICE_NOT_AVAILABLE

0x80110603

COMQC_E_NO_IPERSISTSTREAM

0x80110604

COMQC_E_BAD_MESSAGE

0x80110605

COMQC_E_UNAUTHENTICATED

0x80110606

COMQC_E_UNTRUSTED_ENQUEUER

0x80110701

MSDTC_E_DUPLICATE_RESOURCE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

detected a system error.

The COM+ registry database is
already running.

This version of the COM+
registry database cannot be
migrated.

The schema version to be
migrated could not be found in
the COM+ registry database.

There was a type mismatch
between binaries.

A binary of unknown or invalid
type was provided.

There was a type mismatch
between a binary and an
application.

The application cannot be
paused or resumed.

The COM+ catalog server threw
an exception during execution.

Only COM+ applications marked
"queued" can be invoked using
the "queue" moniker.

At least one interface must be
marked "queued" to create a
queued component instance
with the "queue" moniker.

Message Queuing is required for
the requested operation and is
not installed.

Unable to marshal an interface
that does not support
IPersistStream.

The message is improperly
formatted or was damaged in
transit.

An unauthenticated message
was received by an application
that accepts only authenticated
messages.

The message was requeued or
moved by a user not in the QC
Trusted User "role".

Cannot create a duplicate
resource of type Distributed

83 / 497


Return value/code

0x80110808

COMADMIN_E_OBJECT_PARENT_MISSING

0x80110809

COMADMIN_E_OBJECT_DOES_NOT_EXIST

0x8011080A

COMADMIN_E_APP_NOT_RUNNING

0x8011080B

COMADMIN_E_INVALID_PARTITION

0x8011080D

COMADMIN_E_SVCAPP_NOT_POOLABLE_OR_RECYCLABLE

0x8011080E

COMADMIN_E_USER_IN_SET

0x8011080F

COMADMIN_E_CANTRECYCLELIBRARYAPPS

0x80110811

COMADMIN_E_CANTRECYCLESERVICEAPPS

0x80110812

COMADMIN_E_PROCESSALREADYRECYCLED

0x80110813

COMADMIN_E_PAUSEDPROCESSMAYNOTBERECYCLED

0x80110814

COMADMIN_E_CANTMAKEINPROCSERVICE

0x80110815

COMADMIN_E_PROGIDINUSEBYCLSID

0x80110816

COMADMIN_E_DEFAULT_PARTITION_NOT_IN_SET

0x80110817

COMADMIN_E_RECYCLEDPROCESSMAYNOTBEPAUSED

0x80110818

COMADMIN_E_PARTITION_ACCESSDENIED

0x80110819

COMADMIN_E_PARTITION_MSI_ONLY

0x8011081A

COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_1_0_FORMAT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Transaction Coordinator.

One of the objects being
inserted or updated does not
belong to a valid parent
collection.

One of the specified objects
cannot be found.

The specified application is not
currently running.

The partitions specified are not
valid.

COM+ applications that run as
Windows NT service cannot be
pooled or recycled.

One or more users are already
assigned to a local partition set.

Library applications cannot be
recycled.

Applications running as
Windows NT services cannot be
recycled.

The process has already been
recycled.

A paused process cannot be
recycled.

Library applications cannot be
Windows NT services.

The ProgID provided to the
copy operation is invalid. The
ProgID is in use by another
registered CLSID.

The partition specified as the
default is not a member of the
partition set.

A recycled process cannot be
paused.

Access to the specified partition
is denied.

Only application files (*.msi
files) can be installed into
partitions.

Applications containing one or
more legacy components cannot

84 / 497


Return value/code

0x8011081B

COMADMIN_E_LEGACYCOMPS_NOT_ALLOWED_IN_NONBASE_PARTITIONS

0x8011081C

COMADMIN_E_COMP_MOVE_SOURCE

0x8011081D

COMADMIN_E_COMP_MOVE_DEST

0x8011081E

COMADMIN_E_COMP_MOVE_PRIVATE

0x8011081F

COMADMIN_E_BASEPARTITION_REQUIRED_IN_SET

0x80110820

COMADMIN_E_CANNOT_ALIAS_EVENTCLASS

0x80110821

COMADMIN_E_PRIVATE_ACCESSDENIED

0x80110822

COMADMIN_E_SAFERINVALID

0x80110823

COMADMIN_E_REGISTRY_ACCESSDENIED

0x80110824

COMADMIN_E_PARTITIONS_DISABLED

0x801F0001

ERROR_FLT_NO_HANDLER_DEFINED

0x801F0002

ERROR_FLT_CONTEXT_ALREADY_DEFINED

0x801F0003

ERROR_FLT_INVALID_ASYNCHRONOUS_REQUEST

0x801F0004

ERROR_FLT_DISALLOW_FAST_IO

0x801F0005

ERROR_FLT_INVALID_NAME_REQUEST

0x801F0006

ERROR_FLT_NOT_SAFE_TO_POST_OPERATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

be exported to 1.0 format.

Legacy components cannot
exist in nonbase partitions.

A component cannot be moved
(or copied) from the System
Application, an application
proxy, or a nonchangeable
application.

A component cannot be moved
(or copied) to the System
Application, an application
proxy or a nonchangeable
application.

A private component cannot be
moved (or copied) to a library
application or to the base
partition.

The Base Application Partition
exists in all partition sets and
cannot be removed.

Alas, Event Class components
cannot be aliased.

Access is denied because the
component is private.

The specified SAFER level is
invalid.

The specified user cannot write
to the system registry.

COM+ partitions are currently
disabled.

A handler was not defined by
the filter for this operation.

A context is already defined for
this object.

Asynchronous requests are not
valid for this operation.

Disallow the Fast IO path for
this operation.

An invalid name request was
made. The name requested
cannot be retrieved at this time.

Posting this operation to a
worker thread for further
processing is not safe at this

85 / 497


Return value/code

0x801F0007

ERROR_FLT_NOT_INITIALIZED

0x801F0008

ERROR_FLT_FILTER_NOT_READY

0x801F0009

ERROR_FLT_POST_OPERATION_CLEANUP

0x801F000A

ERROR_FLT_INTERNAL_ERROR

0x801F000B

ERROR_FLT_DELETING_OBJECT

0x801F000C

ERROR_FLT_MUST_BE_NONPAGED_POOL

0x801F000D

ERROR_FLT_DUPLICATE_ENTRY

0x801F000E

ERROR_FLT_CBDQ_DISABLED

0x801F000F

ERROR_FLT_DO_NOT_ATTACH

0x801F0010

ERROR_FLT_DO_NOT_DETACH

0x801F0011

ERROR_FLT_INSTANCE_ALTITUDE_COLLISION

0x801F0012

ERROR_FLT_INSTANCE_NAME_COLLISION

0x801F0013

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

time because it could lead to a
system deadlock.

The Filter Manager was not
initialized when a filter tried to
register. Be sure that the Filter
Manager is being loaded as a
driver.

The filter is not ready for
attachment to volumes because
it has not finished initializing
(FltStartFiltering has not been
called).

The filter must clean up any
operation-specific context at
this time because it is being
removed from the system
before the operation is
completed by the lower drivers.

The Filter Manager had an
internal error from which it
cannot recover; therefore, the
operation has been failed. This
is usually the result of a filter
returning an invalid value from
a preoperation callback.

The object specified for this
action is in the process of being
deleted; therefore, the action
requested cannot be completed
at this time.

Nonpaged pool must be used
for this type of context.

A duplicate handler definition
has been provided for an
operation.

The callback data queue has
been disabled.

Do not attach the filter to the
volume at this time.

Do not detach the filter from
the volume at this time.

An instance already exists at
this altitude on the volume
specified.

An instance already exists with
this name on the volume
specified.

The system could not find the
filter specified.

86 / 497


Return value/code

ERROR_FLT_FILTER_NOT_FOUND

0x801F0014

ERROR_FLT_VOLUME_NOT_FOUND

0x801F0015

ERROR_FLT_INSTANCE_NOT_FOUND

0x801F0016

ERROR_FLT_CONTEXT_ALLOCATION_NOT_FOUND

0x801F0017

ERROR_FLT_INVALID_CONTEXT_REGISTRATION

0x801F0018

ERROR_FLT_NAME_CACHE_MISS

0x801F0019

ERROR_FLT_NO_DEVICE_OBJECT

0x801F001A

ERROR_FLT_VOLUME_ALREADY_MOUNTED

0x801F001B

ERROR_FLT_ALREADY_ENLISTED

0x801F001C

ERROR_FLT_CONTEXT_ALREADY_LINKED

0x801F0020

ERROR_FLT_NO_WAITER_FOR_REPLY

0x80260001

ERROR_HUNG_DISPLAY_DRIVER_THREAD

0x80261001

ERROR_MONITOR_NO_DESCRIPTOR

0x80261002

ERROR_MONITOR_UNKNOWN_DESCRIPTOR_FORMAT

0x80263001

DWM_E_COMPOSITIONDISABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system could not find the
volume specified.

The system could not find the
instance specified.

No registered context allocation
definition was found for the
given request.

An invalid parameter was
specified during context
registration.

The name requested was not
found in the Filter Manager
name cache and could not be
retrieved from the file system.

The requested device object
does not exist for the given
volume.

The specified volume is already
mounted.

The specified Transaction
Context is already enlisted in a
transaction.

The specified context is already
attached to another object.

No waiter is present for the
filter's reply to this message.

{Display Driver Stopped
Responding} The %hs display
driver has stopped working
normally. Save your work and
reboot the system to restore full
display functionality. The next
time you reboot the machine a
dialog will be displayed giving
you a chance to report this
failure to Microsoft.

Monitor descriptor could not be
obtained.

Format of the obtained monitor
descriptor is not supported by
this release.

{Desktop Composition is
Disabled} The operation could
not be completed because
desktop composition is disabled.

87 / 497


Return value/code

0x80263002

DWM_E_REMOTING_NOT_SUPPORTED

0x80263003

DWM_E_NO_REDIRECTION_SURFACE_AVAILABLE

0x80263004

DWM_E_NOT_QUEUING_PRESENTS

0x80280000

TPM_E_ERROR_MASK

0x80280001

TPM_E_AUTHFAIL

0x80280002

TPM_E_BADINDEX

0x80280003

TPM_E_BAD_PARAMETER

0x80280004

TPM_E_AUDITFAILURE

0x80280005

TPM_E_CLEAR_DISABLED

0x80280006

TPM_E_DEACTIVATED

0x80280007

TPM_E_DISABLED

0x80280008

TPM_E_DISABLED_CMD

0x80280009

TPM_E_FAIL

0x8028000A

TPM_E_BAD_ORDINAL

0x8028000B

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Some Desktop Composition
APIs Are Not Supported While
Remoting} Some desktop
composition APIs are not
supported while remoting. The
operation is not supported while
running in a remote session.

{No DWM Redirection Surface is
Available} The Desktop Window
Manager (DWM) was unable to
provide a redirection surface to
complete the DirectX present.

{DWM Is Not Queuing Presents
for the Specified Window} The
window specified is not
currently using queued
presents.

This is an error mask to convert
Trusted Platform Module (TPM)
hardware errors to Win32
errors.

Authentication failed.

The index to a Platform
Configuration Register (PCR),
DIR, or other register is
incorrect.

One or more parameters are
bad.

An operation completed
successfully but the auditing of
that operation failed.

The clear disable flag is set and
all clear operations now require
physical access.

The TPM is deactivated.

The TPM is disabled.

The target command has been
disabled.

The operation failed.

The ordinal was unknown or
inconsistent.

The ability to install an owner is

88 / 497


Return value/code

TPM_E_INSTALL_DISABLED

0x8028000C

TPM_E_INVALID_KEYHANDLE

0x8028000D

TPM_E_KEYNOTFOUND

0x8028000E

TPM_E_INAPPROPRIATE_ENC

0x8028000F

TPM_E_MIGRATEFAIL

0x80280010

TPM_E_INVALID_PCR_INFO

0x80280011

TPM_E_NOSPACE

0x80280012

TPM_E_NOSRK

0x80280013

TPM_E_NOTSEALED_BLOB

0x80280014

TPM_E_OWNER_SET

0x80280015

TPM_E_RESOURCES

0x80280016

TPM_E_SHORTRANDOM

0x80280017

TPM_E_SIZE

0x80280018

TPM_E_WRONGPCRVAL

0x80280019

TPM_E_BAD_PARAM_SIZE

0x8028001A

TPM_E_SHA_THREAD

0x8028001B

TPM_E_SHA_ERROR

0x8028001C

TPM_E_FAILEDSELFTEST

0x8028001D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

disabled.

The key handle cannot be
interpreted.

The key handle points to an
invalid key.

Unacceptable encryption
scheme.

Migration authorization failed.

PCR information could not be
interpreted.

No room to load key.

There is no storage root key
(SRK) set.

An encrypted blob is invalid or
was not created by this TPM.

There is already an owner.

The TPM has insufficient internal
resources to perform the
requested action.

A random string was too short.

The TPM does not have the
space to perform the operation.

The named PCR value does not
match the current PCR value.

The paramSize argument to the
command has the incorrect
value.

There is no existing SHA-1
thread.

The calculation is unable to
proceed because the existing
SHA-1 thread has already
encountered an error.

Self-test has failed and the TPM
has shut down.

The authorization for the second
key in a two-key function failed

89 / 497


Return value/code

TPM_E_AUTH2FAIL

0x8028001E

TPM_E_BADTAG

0x8028001F

TPM_E_IOERROR

0x80280020

TPM_E_ENCRYPT_ERROR

0x80280021

TPM_E_DECRYPT_ERROR

0x80280022

TPM_E_INVALID_AUTHHANDLE

0x80280023

TPM_E_NO_ENDORSEMENT

0x80280024

TPM_E_INVALID_KEYUSAGE

0x80280025

TPM_E_WRONG_ENTITYTYPE

0x80280026

TPM_E_INVALID_POSTINIT

0x80280027

TPM_E_INAPPROPRIATE_SIG

0x80280028

TPM_E_BAD_KEY_PROPERTY

0x80280029

TPM_E_BAD_MIGRATION

0x8028002A

TPM_E_BAD_SCHEME

0x8028002B

TPM_E_BAD_DATASIZE

0x8028002C

TPM_E_BAD_MODE

0x8028002D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

authorization.

The tag value sent to for a
command is invalid.

An I/O error occurred
transmitting information to the
TPM.

The encryption process had a
problem.

The decryption process did not
complete.

An invalid handle was used.

The TPM does not have an
endorsement key (EK) installed.

The usage of a key is not
allowed.

The submitted entity type is not
allowed.

The command was received in
the wrong sequence relative to
TPM_Init and a subsequent
TPM_Startup.

Signed data cannot include
additional DER information.

The key properties in
TPM_KEY_PARMs are not
supported by this TPM.

The migration properties of this
key are incorrect.

The signature or encryption
scheme for this key is incorrect
or not permitted in this
situation.

The size of the data (or blob)
parameter is bad or inconsistent
with the referenced key.

A mode parameter is bad, such
as capArea or subCapArea for
TPM_GetCapability,
physicalPresence parameter for
TPM_PhysicalPresence, or
migrationType for
TPM_CreateMigrationBlob.

Either the physicalPresence or

90 / 497


Return value/code

TPM_E_BAD_PRESENCE

0x8028002E

TPM_E_BAD_VERSION

0x8028002F

TPM_E_NO_WRAP_TRANSPORT

0x80280030

TPM_E_AUDITFAIL_UNSUCCESSFUL

0x80280031

TPM_E_AUDITFAIL_SUCCESSFUL

0x80280032

TPM_E_NOTRESETABLE

0x80280033

TPM_E_NOTLOCAL

0x80280034

TPM_E_BAD_TYPE

0x80280035

TPM_E_INVALID_RESOURCE

0x80280036

TPM_E_NOTFIPS

0x80280037

TPM_E_INVALID_FAMILY

0x80280038

TPM_E_NO_NV_PERMISSION

0x80280039

TPM_E_REQUIRES_SIGN

0x8028003A

TPM_E_KEY_NOTSUPPORTED

0x8028003B

TPM_E_AUTH_CONFLICT

0x8028003C

TPM_E_AREA_LOCKED

0x8028003D

TPM_E_BAD_LOCALITY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

physicalPresenceLock bits have
the wrong value.

The TPM cannot perform this
version of the capability.

The TPM does not allow for
wrapped transport sessions.

TPM audit construction failed
and the underlying command
was returning a failure code
also.

TPM audit construction failed
and the underlying command
was returning success.

Attempt to reset a PCR that
does not have the resettable
attribute.

Attempt to reset a PCR register
that requires locality and the
locality modifier not part of
command transport.

Make identity blob not properly
typed.

When saving context identified
resource type does not match
actual resource.

The TPM is attempting to
execute a command only
available when in Federal
Information Processing
Standards (FIPS) mode.

The command is attempting to
use an invalid family ID.

The permission to manipulate
the NV storage is not available.

The operation requires a signed
command.

Wrong operation to load an NV
key.

NV_LoadKey blob requires both
owner and blob authorization.

The NV area is locked and not
writable.

The locality is incorrect for the
attempted operation.

91 / 497


Return value/code

0x8028003E

TPM_E_READ_ONLY

0x8028003F

TPM_E_PER_NOWRITE

0x80280040

TPM_E_FAMILYCOUNT

0x80280041

TPM_E_WRITE_LOCKED

0x80280042

TPM_E_BAD_ATTRIBUTES

0x80280043

TPM_E_INVALID_STRUCTURE

0x80280044

TPM_E_KEY_OWNER_CONTROL

0x80280045

TPM_E_BAD_COUNTER

0x80280046

TPM_E_NOT_FULLWRITE

0x80280047

TPM_E_CONTEXT_GAP

0x80280048

TPM_E_MAXNVWRITES

0x80280049

TPM_E_NOOPERATOR

0x8028004A

TPM_E_RESOURCEMISSING

0x8028004B

TPM_E_DELEGATE_LOCK

0x8028004C

TPM_E_DELEGATE_FAMILY

0x8028004D

TPM_E_DELEGATE_ADMIN

0x8028004E

TPM_E_TRANSPORT_NOTEXCLUSIVE

0x8028004F

TPM_E_OWNER_CONTROL

0x80280050

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The NV area is read-only and
cannot be written to.

There is no protection on the
write to the NV area.

The family count value does not
match.

The NV area has already been
written to.

The NV area attributes conflict.

The structure tag and version
are invalid or inconsistent.

The key is under control of the
TPM owner and can only be
evicted by the TPM owner.

The counter handle is incorrect.

The write is not a complete
write of the area.

The gap between saved context
counts is too large.

The maximum number of NV
writes without an owner has
been exceeded.

No operator AuthData value is
set.

The resource pointed to by
context is not loaded.

The delegate administration is
locked.

Attempt to manage a family
other then the delegated family.

Delegation table management
not enabled.

There was a command executed
outside an exclusive transport
session.

Attempt to context save an
owner evict controlled key.

The DAA command has no

92 / 497


Return value/code

TPM_E_DAA_RESOURCES

0x80280051

TPM_E_DAA_INPUT_DATA0

0x80280052

TPM_E_DAA_INPUT_DATA1

0x80280053

TPM_E_DAA_ISSUER_SETTINGS

0x80280054

TPM_E_DAA_TPM_SETTINGS

0x80280055

TPM_E_DAA_STAGE

0x80280056

TPM_E_DAA_ISSUER_VALIDITY

0x80280057

TPM_E_DAA_WRONG_W

0x80280058

TPM_E_BAD_HANDLE

0x80280059

TPM_E_BAD_DELEGATE

0x8028005A

TPM_E_BADCONTEXT

0x8028005B

TPM_E_TOOMANYCONTEXTS

0x8028005C

TPM_E_MA_TICKET_SIGNATURE

0x8028005D

TPM_E_MA_DESTINATION

0x8028005E

TPM_E_MA_SOURCE

0x8028005F

TPM_E_MA_AUTHORITY

0x80280061

TPM_E_PERMANENTEK

0x80280062

TPM_E_BAD_SIGNATURE

0x80280063

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

resources available to execute
the command.

The consistency check on DAA
parameter inputData0 has
failed.

The consistency check on DAA
parameter inputData1 has
failed.

The consistency check on
DAA_issuerSettings has failed.

The consistency check on
DAA_tpmSpecific has failed.

The atomic process indicated by
the submitted DAA command is
not the expected process.

The issuer's validity check has
detected an inconsistency.

The consistency check on w has
failed.

The handle is incorrect.

Delegation is not correct.

The context blob is invalid.

Too many contexts held by the
TPM.

Migration authority signature
validation failure.

Migration destination not
authenticated.

Migration source incorrect.

Incorrect migration authority.

Attempt to revoke the EK and
the EK is not revocable.

Bad signature of CMK ticket.

There is no room in the context
list for additional contexts.

93 / 497


Return value/code

TPM_E_NOCONTEXTSPACE

0x80280400

TPM_E_COMMAND_BLOCKED

0x80280401

TPM_E_INVALID_HANDLE

0x80280402

TPM_E_DUPLICATE_VHANDLE

0x80280403

TPM_E_EMBEDDED_COMMAND_BLOCKED

0x80280404

TPM_E_EMBEDDED_COMMAND_UNSUPPORTED

0x80280800

TPM_E_RETRY

0x80280801

TPM_E_NEEDS_SELFTEST

0x80280802

TPM_E_DOING_SELFTEST

0x80280803

TPM_E_DEFEND_LOCK_RUNNING

0x80284001

TBS_E_INTERNAL_ERROR

0x80284002

TBS_E_BAD_PARAMETER

0x80284003

TBS_E_INVALID_OUTPUT_POINTER

0x80284004

TBS_E_INVALID_CONTEXT

0x80284005

TBS_E_INSUFFICIENT_BUFFER

0x80284006

TBS_E_IOERROR

0x80284007

TBS_E_INVALID_CONTEXT_PARAM

0x80284008

TBS_E_SERVICE_NOT_RUNNING

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The command was blocked.

The specified handle was not
found.

The TPM returned a duplicate
handle and the command needs
to be resubmitted.

The command within the
transport was blocked.

The command within the
transport is not supported.

The TPM is too busy to respond
to the command immediately,
but the command could be
resubmitted at a later time.

SelfTestFull has not been run.

The TPM is currently executing
a full self-test.

The TPM is defending against
dictionary attacks and is in a
time-out period.

An internal software error has
been detected.

One or more input parameters
are bad.

A specified output pointer is
bad.

The specified context handle
does not refer to a valid
context.

A specified output buffer is too
small.

An error occurred while
communicating with the TPM.

One or more context
parameters are invalid.

The TPM Base Services (TBS) is
not running and could not be
started.

94 / 497


Return value/code

0x80284009

TBS_E_TOO_MANY_TBS_CONTEXTS

0x8028400A

TBS_E_TOO_MANY_RESOURCES

0x8028400B

TBS_E_SERVICE_START_PENDING

0x8028400C

TBS_E_PPI_NOT_SUPPORTED

0x8028400D

TBS_E_COMMAND_CANCELED

0x8028400E

TBS_E_BUFFER_TOO_LARGE

0x80290100

TPMAPI_E_INVALID_STATE

0x80290101

TPMAPI_E_NOT_ENOUGH_DATA

0x80290102

TPMAPI_E_TOO_MUCH_DATA

0x80290103

TPMAPI_E_INVALID_OUTPUT_POINTER

0x80290104

TPMAPI_E_INVALID_PARAMETER

0x80290105

TPMAPI_E_OUT_OF_MEMORY

0x80290106

TPMAPI_E_BUFFER_TOO_SMALL

0x80290107

TPMAPI_E_INTERNAL_ERROR

0x80290108

TPMAPI_E_ACCESS_DENIED

0x80290109

TPMAPI_E_AUTHORIZATION_FAILED

0x8029010A

TPMAPI_E_INVALID_CONTEXT_HANDLE

0x8029010B

TPMAPI_E_TBS_COMMUNICATION_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A new context could not be
created because there are too
many open contexts.

A new virtual resource could not
be created because there are
too many open virtual
resources.

The TBS service has been
started but is not yet running.

The physical presence interface
is not supported.

The command was canceled.

The input or output buffer is too
large.

The command buffer is not in
the correct state.

The command buffer does not
contain enough data to satisfy
the request.

The command buffer cannot
contain any more data.

One or more output parameters
was null or invalid.

One or more input parameters
are invalid.

Not enough memory was
available to satisfy the request.

The specified buffer was too
small.

An internal error was detected.

The caller does not have the
appropriate rights to perform
the requested operation.

The specified authorization
information was invalid.

The specified context handle
was not valid.

An error occurred while
communicating with the TBS.

95 / 497


Return value/code

0x8029010C

TPMAPI_E_TPM_COMMAND_ERROR

0x8029010D

TPMAPI_E_MESSAGE_TOO_LARGE

0x8029010E

TPMAPI_E_INVALID_ENCODING

0x8029010F

TPMAPI_E_INVALID_KEY_SIZE

0x80290110

TPMAPI_E_ENCRYPTION_FAILED

0x80290111

TPMAPI_E_INVALID_KEY_PARAMS

0x80290112

TPMAPI_E_INVALID_MIGRATION_AUTHORIZATION_BLOB

0x80290113

TPMAPI_E_INVALID_PCR_INDEX

0x80290114

TPMAPI_E_INVALID_DELEGATE_BLOB

0x80290115

TPMAPI_E_INVALID_CONTEXT_PARAMS

0x80290116

TPMAPI_E_INVALID_KEY_BLOB

0x80290117

TPMAPI_E_INVALID_PCR_DATA

0x80290118

TPMAPI_E_INVALID_OWNER_AUTH

0x80290200

TBSIMP_E_BUFFER_TOO_SMALL

0x80290201

TBSIMP_E_CLEANUP_FAILED

0x80290202

TBSIMP_E_INVALID_CONTEXT_HANDLE

0x80290203

TBSIMP_E_INVALID_CONTEXT_PARAM

0x80290204

TBSIMP_E_TPM_ERROR

0x80290205

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The TPM returned an
unexpected result.

The message was too large for
the encoding scheme.

The encoding in the binary large
object (BLOB) was not
recognized.

The key size is not valid.

The encryption operation failed.

The key parameters structure
was not valid.

The requested supplied data
does not appear to be a valid
migration authorization BLOB.

The specified PCR index was
invalid.

The data given does not appear
to be a valid delegate BLOB.

One or more of the specified
context parameters was not
valid.

The data given does not appear
to be a valid key BLOB.

The specified PCR data was
invalid.

The format of the owner
authorization data was invalid.

The specified buffer was too
small.

The context could not be
cleaned up.

The specified context handle is
invalid.

An invalid context parameter
was specified.

An error occurred while
communicating with the TPM.

No entry with the specified key

96 / 497


Return value/code

TBSIMP_E_HASH_BAD_KEY

0x80290206

TBSIMP_E_DUPLICATE_VHANDLE

0x80290207

TBSIMP_E_INVALID_OUTPUT_POINTER

0x80290208

TBSIMP_E_INVALID_PARAMETER

0x80290209

TBSIMP_E_RPC_INIT_FAILED

0x8029020A

TBSIMP_E_SCHEDULER_NOT_RUNNING

0x8029020B

TBSIMP_E_COMMAND_CANCELED

0x8029020C

TBSIMP_E_OUT_OF_MEMORY

0x8029020D

TBSIMP_E_LIST_NO_MORE_ITEMS

0x8029020E

TBSIMP_E_LIST_NOT_FOUND

0x8029020F

TBSIMP_E_NOT_ENOUGH_SPACE

0x80290210

TBSIMP_E_NOT_ENOUGH_TPM_CONTEXTS

0x80290211

TBSIMP_E_COMMAND_FAILED

0x80290212

TBSIMP_E_UNKNOWN_ORDINAL

0x80290213

TBSIMP_E_RESOURCE_EXPIRED

0x80290214

TBSIMP_E_INVALID_RESOURCE

0x80290215

TBSIMP_E_NOTHING_TO_UNLOAD

0x80290216

TBSIMP_E_HASH_TABLE_FULL

0x80290217

TBSIMP_E_TOO_MANY_TBS_CONTEXTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

was found.

The specified virtual handle
matches a virtual handle
already in use.

The pointer to the returned
handle location was null or
invalid.

One or more parameters are
invalid.

The RPC subsystem could not
be initialized.

The TBS scheduler is not
running.

The command was canceled.

There was not enough memory
to fulfill the request.

The specified list is empty, or
the iteration has reached the
end of the list.

The specified item was not
found in the list.

The TPM does not have enough
space to load the requested
resource.

There are too many TPM
contexts in use.

The TPM command failed.

The TBS does not recognize the
specified ordinal.

The requested resource is no
longer available.

The resource type did not
match.

No resources can be unloaded.

No new entries can be added to
the hash table.

A new TBS context could not be
created because there are too

97 / 497


Return value/code

0x80290218

TBSIMP_E_TOO_MANY_RESOURCES

0x80290219

TBSIMP_E_PPI_NOT_SUPPORTED

0x8029021A

TBSIMP_E_TPM_INCOMPATIBLE

0x80290300

TPM_E_PPI_ACPI_FAILURE

0x80290301

TPM_E_PPI_USER_ABORT

0x80290302

TPM_E_PPI_BIOS_FAILURE

0x80290303

TPM_E_PPI_NOT_SUPPORTED

0x80300002

PLA_E_DCS_NOT_FOUND

0x80300045

PLA_E_TOO_MANY_FOLDERS

0x80300070

PLA_E_NO_MIN_DISK

0x803000AA

PLA_E_DCS_IN_USE

0x803000B7

PLA_E_DCS_ALREADY_EXISTS

0x80300101

PLA_E_PROPERTY_CONFLICT

0x80300102

PLA_E_DCS_SINGLETON_REQUIRED

0x80300103

PLA_E_CREDENTIALS_REQUIRED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

many open contexts.

A new virtual resource could not
be created because there are
too many open virtual
resources.

The physical presence interface
is not supported.

TBS is not compatible with the
version of TPM found on the
system.

A general error was detected
when attempting to acquire the
BIOS response to a physical
presence command.

The user failed to confirm the
TPM operation request.

The BIOS failure prevented the
successful execution of the
requested TPM operation (for
example, invalid TPM operation
request, BIOS communication
error with the TPM).

The BIOS does not support the
physical presence interface.

A Data Collector Set was not
found.

Unable to start Data Collector
Set because there are too many
folders.

Not enough free disk space to
start Data Collector Set.

Data Collector Set is in use.

Data Collector Set already
exists.

Property value conflict.

The current configuration for
this Data Collector Set requires
that it contain exactly one Data
Collector.

A user account is required to
commit the current Data
Collector Set properties.

98 / 497


Return value/code

0x80300104

PLA_E_DCS_NOT_RUNNING

0x80300105

PLA_E_CONFLICT_INCL_EXCL_API

0x80300106

PLA_E_NETWORK_EXE_NOT_VALID

0x80300107

PLA_E_EXE_ALREADY_CONFIGURED

0x80300108

PLA_E_EXE_PATH_NOT_VALID

0x80300109

PLA_E_DC_ALREADY_EXISTS

0x8030010A

PLA_E_DCS_START_WAIT_TIMEOUT

0x8030010B

PLA_E_DC_START_WAIT_TIMEOUT

0x8030010C

PLA_E_REPORT_WAIT_TIMEOUT

0x8030010D

PLA_E_NO_DUPLICATES

0x8030010E

PLA_E_EXE_FULL_PATH_REQUIRED

0x8030010F

PLA_E_INVALID_SESSION_NAME

0x80300110

PLA_E_PLA_CHANNEL_NOT_ENABLED

0x80300111

PLA_E_TASKSCHED_CHANNEL_NOT_ENABLED

0x80310000

FVE_E_LOCKED_VOLUME

0x80310001

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Data Collector Set is not
running.

A conflict was detected in the
list of include and exclude APIs.
Do not specify the same API in
both the include list and the
exclude list.

The executable path specified
refers to a network share or
UNC path.

The executable path specified is
already configured for API
tracing.

The executable path specified
does not exist. Verify that the
specified path is correct.

Data Collector already exists.

The wait for the Data Collector
Set start notification has timed
out.

The wait for the Data Collector
to start has timed out.

The wait for the report
generation tool to finish has
timed out.

Duplicate items are not allowed.

When specifying the executable
to trace, you must specify a full
path to the executable and not
just a file name.

The session name provided is
invalid.

The Event Log channel
Microsoft-Windows-Diagnosis-
PLA/Operational must be
enabled to perform this
operation.

The Event Log channel
Microsoft-Windows-
TaskScheduler must be enabled
to perform this operation.

The volume must be unlocked
before it can be used.

The volume is fully decrypted

99 / 497


Return value/code

FVE_E_NOT_ENCRYPTED

0x80310002

FVE_E_NO_TPM_BIOS

0x80310003

FVE_E_NO_MBR_METRIC

0x80310004

FVE_E_NO_BOOTSECTOR_METRIC

0x80310005

FVE_E_NO_BOOTMGR_METRIC

0x80310006

FVE_E_WRONG_BOOTMGR

0x80310007

FVE_E_SECURE_KEY_REQUIRED

0x80310008

FVE_E_NOT_ACTIVATED

0x80310009

FVE_E_ACTION_NOT_ALLOWED

0x8031000A

FVE_E_AD_SCHEMA_NOT_INSTALLED

0x8031000B

FVE_E_AD_INVALID_DATATYPE

0x8031000C

FVE_E_AD_INVALID_DATASIZE

0x8031000D

FVE_E_AD_NO_VALUES

0x8031000E

FVE_E_AD_ATTR_NOT_SET

0x8031000F

FVE_E_AD_GUID_NOT_FOUND

0x80310010

FVE_E_BAD_INFORMATION

0x80310011

FVE_E_TOO_SMALL

0x80310012

FVE_E_SYSTEM_VOLUME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

and no key is available.

The firmware does not support
using a TPM during boot.

The firmware does not use a
TPM to perform initial program
load (IPL) measurement.

The master boot record (MBR)
is not TPM-aware.

The BOOTMGR is not being
measured by the TPM.

The BOOTMGR component does
not perform expected TPM
measurements.

No secure key protection
mechanism has been defined.

This volume has not been
provisioned for encryption.

Requested action was denied by
the full-volume encryption
(FVE) control engine.

The Active Directory forest does
not contain the required
attributes and classes to host
FVE or TPM information.

The type of data obtained from
Active Directory was not
expected.

The size of the data obtained
from Active Directory was not
expected.

The attribute read from Active
Directory has no (zero) values.

The attribute was not set.

The specified GUID could not be
found.

The control block for the
encrypted volume is not valid.

Not enough free space
remaining on volume to allow
encryption.

The volume cannot be
encrypted because it is required

100 / 497


Return value/code

0x80310013

FVE_E_FAILED_WRONG_FS

0x80310014

FVE_E_FAILED_BAD_FS

0x80310015

FVE_E_NOT_SUPPORTED

0x80310016

FVE_E_BAD_DATA

0x80310017

FVE_E_VOLUME_NOT_BOUND

0x80310018

FVE_E_TPM_NOT_OWNED

0x80310019

FVE_E_NOT_DATA_VOLUME

0x8031001A

FVE_E_AD_INSUFFICIENT_BUFFER

0x8031001B

FVE_E_CONV_READ

0x8031001C

FVE_E_CONV_WRITE

0x8031001D

FVE_E_KEY_REQUIRED

0x8031001E

FVE_E_CLUSTERING_NOT_SUPPORTED

0x8031001F

FVE_E_VOLUME_BOUND_ALREADY

0x80310020

FVE_E_OS_NOT_PROTECTED

0x80310021

FVE_E_PROTECTION_DISABLED

0x80310022

FVE_E_RECOVERY_KEY_REQUIRED

0x80310023

FVE_E_FOREIGN_VOLUME

0x80310024

FVE_E_OVERLAPPED_UPDATE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

to boot the operating system.

The volume cannot be
encrypted because the file
system is not supported.

The file system is inconsistent.
Run CHKDSK.

This volume cannot be
encrypted.

Data supplied is malformed.

Volume is not bound to the
system.

TPM must be owned before a
volume can be bound to it.

The volume specified is not a
data volume.

The buffer supplied to a
function was insufficient to
contain the returned data.

A read operation failed while
converting the volume.

A write operation failed while
converting the volume.

One or more key protection
mechanisms are required for
this volume.

Cluster configurations are not
supported.

The volume is already bound to
the system.

The boot OS volume is not
being protected via FVE.

All protection mechanisms are
effectively disabled (clear key
exists).

A recovery key protection
mechanism is required.

This volume cannot be bound to
a TPM.

The control block for the
encrypted volume was updated

101 / 497


Return value/code

0x80310025

FVE_E_TPM_SRK_AUTH_NOT_ZERO

0x80310026

FVE_E_FAILED_SECTOR_SIZE

0x80310027

FVE_E_FAILED_AUTHENTICATION

0x80310028

FVE_E_NOT_OS_VOLUME

0x80310029

FVE_E_AUTOUNLOCK_ENABLED

0x8031002A

FVE_E_WRONG_BOOTSECTOR

0x8031002B

FVE_E_WRONG_SYSTEM_FS

0x8031002C

FVE_E_POLICY_PASSWORD_REQUIRED

0x8031002D

FVE_E_CANNOT_SET_FVEK_ENCRYPTED

0x8031002E

FVE_E_CANNOT_ENCRYPT_NO_KEY

0x80310030

FVE_E_BOOTABLE_CDDVD

0x80310031

FVE_E_PROTECTOR_EXISTS

0x80310032

FVE_E_RELATIVE_PATH

0x80320001

FWP_E_CALLOUT_NOT_FOUND

0x80320002

FWP_E_CONDITION_NOT_FOUND

0x80320003

FWP_E_FILTER_NOT_FOUND

0x80320004

FWP_E_LAYER_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

by another thread. Try again.

The SRK authentication of the
TPM is not zero and, therefore,
is not compatible.

The volume encryption
algorithm cannot be used on
this sector size.

BitLocker recovery
authentication failed.

The volume specified is not the
boot OS volume.

Auto-unlock information for
data volumes is present on the
boot OS volume.

The system partition boot
sector does not perform TPM
measurements.

The system partition file system
must be NTFS.

Group policy requires a
recovery password before
encryption can begin.

The volume encryption
algorithm and key cannot be set
on an encrypted volume.

A key must be specified before
encryption can begin.

A bootable CD/DVD is in the
system. Remove the CD/DVD
and reboot the system.

An instance of this key
protector already exists on the
volume.

The file cannot be saved to a
relative path.

The callout does not exist.

The filter condition does not
exist.

The filter does not exist.

The layer does not exist.

102 / 497


Return value/code

0x80320005

FWP_E_PROVIDER_NOT_FOUND

0x80320006

FWP_E_PROVIDER_CONTEXT_NOT_FOUND

0x80320007

FWP_E_SUBLAYER_NOT_FOUND

0x80320008

FWP_E_NOT_FOUND

0x80320009

FWP_E_ALREADY_EXISTS

0x8032000A

FWP_E_IN_USE

0x8032000B

FWP_E_DYNAMIC_SESSION_IN_PROGRESS

0x8032000C

FWP_E_WRONG_SESSION

0x8032000D

FWP_E_NO_TXN_IN_PROGRESS

0x8032000E

FWP_E_TXN_IN_PROGRESS

0x8032000F

FWP_E_TXN_ABORTED

0x80320010

FWP_E_SESSION_ABORTED

0x80320011

FWP_E_INCOMPATIBLE_TXN

0x80320012

FWP_E_TIMEOUT

0x80320013

FWP_E_NET_EVENTS_DISABLED

0x80320014

FWP_E_INCOMPATIBLE_LAYER

0x80320015

FWP_E_KM_CLIENTS_ONLY

0x80320016

FWP_E_LIFETIME_MISMATCH

0x80320017

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The provider does not exist.

The provider context does not
exist.

The sublayer does not exist.

The object does not exist.

An object with that GUID or
LUID already exists.

The object is referenced by
other objects and, therefore,
cannot be deleted.

The call is not allowed from
within a dynamic session.

The call was made from the
wrong session and, therefore,
cannot be completed.

The call must be made from
within an explicit transaction.

The call is not allowed from
within an explicit transaction.

The explicit transaction has
been forcibly canceled.

The session has been canceled.

The call is not allowed from
within a read-only transaction.

The call timed out while waiting
to acquire the transaction lock.

Collection of network diagnostic
events is disabled.

The operation is not supported
by the specified layer.

The call is allowed for kernel-
mode callers only.

The call tried to associate two
objects with incompatible
lifetimes.

The object is built in and,

103 / 497


Return value/code

FWP_E_BUILTIN_OBJECT

0x80320018

FWP_E_TOO_MANY_BOOTTIME_FILTERS

0x80320019

FWP_E_NOTIFICATION_DROPPED

0x8032001A

FWP_E_TRAFFIC_MISMATCH

0x8032001B

FWP_E_INCOMPATIBLE_SA_STATE

0x8032001C

FWP_E_NULL_POINTER

0x8032001D

FWP_E_INVALID_ENUMERATOR

0x8032001E

FWP_E_INVALID_FLAGS

0x8032001F

FWP_E_INVALID_NET_MASK

0x80320020

FWP_E_INVALID_RANGE

0x80320021

FWP_E_INVALID_INTERVAL

0x80320022

FWP_E_ZERO_LENGTH_ARRAY

0x80320023

FWP_E_NULL_DISPLAY_NAME

0x80320024

FWP_E_INVALID_ACTION_TYPE

0x80320025

FWP_E_INVALID_WEIGHT

0x80320026

FWP_E_MATCH_TYPE_MISMATCH

0x80320027

FWP_E_TYPE_MISMATCH

0x80320028

FWP_E_OUT_OF_BOUNDS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

therefore, cannot be deleted.

The maximum number of boot-
time filters has been reached.

A notification could not be
delivered because a message
queue is at its maximum
capacity.

The traffic parameters do not
match those for the security
association context.

The call is not allowed for the
current security association
state.

A required pointer is null.

An enumerator is not valid.

The flags field contains an
invalid value.

A network mask is not valid.

An FWP_RANGE is not valid.

The time interval is not valid.

An array that must contain at
least one element that is zero-
length.

The displayData.name field
cannot be null.

The action type is not one of
the allowed action types for a
filter.

The filter weight is not valid.

A filter condition contains a
match type that is not
compatible with the operands.

An FWP_VALUE or
FWPM_CONDITION_VALUE is of
the wrong type.

An integer value is outside the
allowed range.

104 / 497


Return value/code

0x80320029

FWP_E_RESERVED

0x8032002A

FWP_E_DUPLICATE_CONDITION

0x8032002B

FWP_E_DUPLICATE_KEYMOD

0x8032002C

FWP_E_ACTION_INCOMPATIBLE_WITH_LAYER

0x8032002D

FWP_E_ACTION_INCOMPATIBLE_WITH_SUBLAYER

0x8032002E

FWP_E_CONTEXT_INCOMPATIBLE_WITH_LAYER

0x8032002F

FWP_E_CONTEXT_INCOMPATIBLE_WITH_CALLOUT

0x80320030

FWP_E_INCOMPATIBLE_AUTH_METHOD

0x80320031

FWP_E_INCOMPATIBLE_DH_GROUP

0x80320032

FWP_E_EM_NOT_SUPPORTED

0x80320033

FWP_E_NEVER_MATCH

0x80320034

FWP_E_PROVIDER_CONTEXT_MISMATCH

0x80320035

FWP_E_INVALID_PARAMETER

0x80320036

FWP_E_TOO_MANY_SUBLAYERS

0x80320037

FWP_E_CALLOUT_NOTIFICATION_FAILED

0x80320038

FWP_E_INCOMPATIBLE_AUTH_CONFIG

0x80320039

FWP_E_INCOMPATIBLE_CIPHER_CONFIG

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A reserved field is nonzero.

A filter cannot contain multiple
conditions operating on a single
field.

A policy cannot contain the
same keying module more than
once.

The action type is not
compatible with the layer.

The action type is not
compatible with the sublayer.

The raw context or the provider
context is not compatible with
the layer.

The raw context or the provider
context is not compatible with
the callout.

The authentication method is
not compatible with the policy
type.

The Diffie-Hellman group is not
compatible with the policy type.

An Internet Key Exchange (IKE)
policy cannot contain an
Extended Mode policy.

The enumeration template or
subscription will never match
any objects.

The provider context is of the
wrong type.

The parameter is incorrect.

The maximum number of
sublayers has been reached.

The notification function for a
callout returned an error.

The IPsec authentication
configuration is not compatible
with the authentication type.

The IPsec cipher configuration is
not compatible with the cipher
type.

105 / 497


Return value/code

0x80340002

ERROR_NDIS_INTERFACE_CLOSING

0x80340004

ERROR_NDIS_BAD_VERSION

0x80340005

ERROR_NDIS_BAD_CHARACTERISTICS

0x80340006

ERROR_NDIS_ADAPTER_NOT_FOUND

0x80340007

ERROR_NDIS_OPEN_FAILED

0x80340008

ERROR_NDIS_DEVICE_FAILED

0x80340009

ERROR_NDIS_MULTICAST_FULL

0x8034000A

ERROR_NDIS_MULTICAST_EXISTS

0x8034000B

ERROR_NDIS_MULTICAST_NOT_FOUND

0x8034000C

ERROR_NDIS_REQUEST_ABORTED

0x8034000D

ERROR_NDIS_RESET_IN_PROGRESS

0x8034000F

ERROR_NDIS_INVALID_PACKET

0x80340010

ERROR_NDIS_INVALID_DEVICE_REQUEST

0x80340011

ERROR_NDIS_ADAPTER_NOT_READY

0x80340014

ERROR_NDIS_INVALID_LENGTH

0x80340015

ERROR_NDIS_INVALID_DATA

0x80340016

ERROR_NDIS_BUFFER_TOO_SHORT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The binding to the network
interface is being closed.

An invalid version was specified.

An invalid characteristics table
was used.

Failed to find the network
interface, or the network
interface is not ready.

Failed to open the network
interface.

The network interface has
encountered an internal
unrecoverable failure.

The multicast list on the
network interface is full.

An attempt was made to add a
duplicate multicast address to
the list.

At attempt was made to remove
a multicast address that was
never added.

The network interface aborted
the request.

The network interface cannot
process the request because it
is being reset.

An attempt was made to send
an invalid packet on a network
interface.

The specified request is not a
valid operation for the target
device.

The network interface is not
ready to complete this
operation.

The length of the buffer
submitted for this operation is
not valid.

The data used for this operation
is not valid.

The length of the buffer
submitted for this operation is
too small.

106 / 497


Return value/code

0x80340017

ERROR_NDIS_INVALID_OID

0x80340018

ERROR_NDIS_ADAPTER_REMOVED

0x80340019

ERROR_NDIS_UNSUPPORTED_MEDIA

0x8034001A

ERROR_NDIS_GROUP_ADDRESS_IN_USE

0x8034001B

ERROR_NDIS_FILE_NOT_FOUND

0x8034001C

ERROR_NDIS_ERROR_READING_FILE

0x8034001D

ERROR_NDIS_ALREADY_MAPPED

0x8034001E

ERROR_NDIS_RESOURCE_CONFLICT

0x8034001F

ERROR_NDIS_MEDIA_DISCONNECTED

0x80340022

ERROR_NDIS_INVALID_ADDRESS

0x8034002A

ERROR_NDIS_PAUSED

0x8034002B

ERROR_NDIS_INTERFACE_NOT_FOUND

0x8034002C

ERROR_NDIS_UNSUPPORTED_REVISION

0x8034002D

ERROR_NDIS_INVALID_PORT

0x8034002E

ERROR_NDIS_INVALID_PORT_STATE

0x803400BB

ERROR_NDIS_NOT_SUPPORTED

0x80342000

ERROR_NDIS_DOT11_AUTO_CONFIG_ENABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The network interface does not
support this OID.

The network interface has been
removed.

The network interface does not
support this media type.

An attempt was made to
remove a token ring group
address that is in use by other
components.

An attempt was made to map a
file that cannot be found.

An error occurred while the
NDIS tried to map the file.

An attempt was made to map a
file that is already mapped.

An attempt to allocate a
hardware resource failed
because the resource is used by
another component.

The I/O operation failed
because network media is
disconnected or the wireless
access point is out of range.

The network address used in
the request is invalid.

The offload operation on the
network interface has been
paused.

The network interface was not
found.

The revision number specified in
the structure is not supported.

The specified port does not
exist on this network interface.

The current state of the
specified port on this network
interface does not support the
requested operation.

The network interface does not
support this request.

The wireless local area network
(LAN) interface is in auto-

107 / 497


Return value/code

0x80342001

ERROR_NDIS_DOT11_MEDIA_IN_USE

0x80342002

ERROR_NDIS_DOT11_POWER_STATE_INVALID

0x8DEAD01B

TRK_E_NOT_FOUND

0x8DEAD01C

TRK_E_VOLUME_QUOTA_EXCEEDED

0x8DEAD01E

TRK_SERVER_TOO_BUSY

0xC0090001

ERROR_AUDITING_DISABLED

0xC0090002

ERROR_ALL_SIDS_FILTERED

0xC0090003

ERROR_BIZRULES_NOT_ENABLED

0xC00D0005

NS_E_NOCONNECTION

0xC00D0006

NS_E_CANNOTCONNECT

0xC00D0007

NS_E_CANNOTDESTROYTITLE

0xC00D0008

NS_E_CANNOTRENAMETITLE

0xC00D0009

NS_E_CANNOTOFFLINEDISK

0xC00D000A

NS_E_CANNOTONLINEDISK

0xC00D000B

NS_E_NOREGISTEREDWALKER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

configuration mode and does
not support the requested
parameter change operation.

The wireless LAN interface is
busy and cannot perform the
requested operation.

The wireless LAN interface is
shutting down and does not
support the requested
operation.

A requested object was not
found.

The server received a
CREATE_VOLUME subrequest of
a SYNC_VOLUMES request, but
the ServerVolumeTable size
limit for the RequestMachine
has already been reached.

The server is busy, and the
client should retry the request
at a later time.

The specified event is currently
not being audited.

The SID filtering operation
removed all SIDs.

Business rule scripts are
disabled for the calling
application.

There is no connection
established with the Windows
Media server. The operation
failed.

Unable to establish a connection
to the server.

Unable to destroy the title.

Unable to rename the title.

Unable to offline disk.

Unable to online disk.

There is no file parser
registered for this type of file.

108 / 497


Return value/code

0xC00D000C

NS_E_NOFUNNEL

0xC00D000D

NS_E_NO_LOCALPLAY

0xC00D000E

NS_E_NETWORK_BUSY

0xC00D000F

NS_E_TOO_MANY_SESS

0xC00D0010

NS_E_ALREADY_CONNECTED

0xC00D0011

NS_E_INVALID_INDEX

0xC00D0012

NS_E_PROTOCOL_MISMATCH

0xC00D0013

NS_E_TIMEOUT

0xC00D0014

NS_E_NET_WRITE

0xC00D0015

NS_E_NET_READ

0xC00D0016

NS_E_DISK_WRITE

0xC00D0017

NS_E_DISK_READ

0xC00D0018

NS_E_FILE_WRITE

0xC00D0019

NS_E_FILE_READ

0xC00D001A

NS_E_FILE_NOT_FOUND

0xC00D001B

NS_E_FILE_EXISTS

0xC00D001C

NS_E_INVALID_NAME

0xC00D001D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There is no data connection
established.

Failed to load the local play
DLL.

The network is busy.

The server session limit was
exceeded.

The network connection already
exists.

Index %1 is invalid.

There is no protocol or protocol
version supported by both the
client and the server.

The server, a computer set up
to offer multimedia content to
other computers, could not
handle your request for
multimedia content in a timely
manner. Please try again later.

Error writing to the network.

Error reading from the network.

Error writing to a disk.

Error reading from a disk.

Error writing to a file.

Error reading from a file.

The system cannot find the file
specified.

The file already exists.

The file name, directory name,
or volume label syntax is
incorrect.

Failed to open a file.

109 / 497


Return value/code

NS_E_FILE_OPEN_FAILED

0xC00D001E

NS_E_FILE_ALLOCATION_FAILED

0xC00D001F

NS_E_FILE_INIT_FAILED

0xC00D0020

NS_E_FILE_PLAY_FAILED

0xC00D0021

NS_E_SET_DISK_UID_FAILED

0xC00D0022

NS_E_INDUCED

0xC00D0023

NS_E_CCLINK_DOWN

0xC00D0024

NS_E_INTERNAL

0xC00D0025

NS_E_BUSY

0xC00D0026

NS_E_UNRECOGNIZED_STREAM_TYPE

0xC00D0027

NS_E_NETWORK_SERVICE_FAILURE

0xC00D0028

NS_E_NETWORK_RESOURCE_FAILURE

0xC00D0029

NS_E_CONNECTION_FAILURE

0xC00D002A

NS_E_SHUTDOWN

0xC00D002B

NS_E_INVALID_REQUEST

0xC00D002C

NS_E_INSUFFICIENT_BANDWIDTH

0xC00D002D

NS_E_NOT_REBUILDING

0xC00D002E

NS_E_LATE_OPERATION

0xC00D002F

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unable to allocate a file.

Unable to initialize a file.

Unable to play a file.

Could not set the disk UID.

An error was induced for testing
purposes.

Two Content Servers failed to
communicate.

An unknown error occurred.

The requested resource is in
use.

The specified protocol is not
recognized. Be sure that the file
name and syntax, such as
slashes, are correct for the
protocol.

The network service provider
failed.

An attempt to acquire a
network resource failed.

The network connection has
failed.

The session is being terminated
locally.

The request is invalid in the
current state.

There is insufficient bandwidth
available to fulfill the request.

The disk is not rebuilding.

An operation requested for a
particular time could not be
carried out on schedule.

Invalid or corrupt data was

110 / 497


Return value/code

NS_E_INVALID_DATA

0xC00D0030

NS_E_FILE_BANDWIDTH_LIMIT

0xC00D0031

NS_E_OPEN_FILE_LIMIT

0xC00D0032

NS_E_BAD_CONTROL_DATA

0xC00D0033

NS_E_NO_STREAM

0xC00D0034

NS_E_STREAM_END

0xC00D0035

NS_E_SERVER_NOT_FOUND

0xC00D0036

NS_E_DUPLICATE_NAME

0xC00D0037

NS_E_DUPLICATE_ADDRESS

0xC00D0038

NS_E_BAD_MULTICAST_ADDRESS

0xC00D0039

NS_E_BAD_ADAPTER_ADDRESS

0xC00D003A

NS_E_BAD_DELIVERY_MODE

0xC00D003B

NS_E_INVALID_CHANNEL

0xC00D003C

NS_E_INVALID_STREAM

0xC00D003D

NS_E_INVALID_ARCHIVE

0xC00D003E

NS_E_NOTITLES

0xC00D003F

NS_E_INVALID_CLIENT

0xC00D0040

NS_E_INVALID_BLACKHOLE_ADDRESS

0xC00D0041

NS_E_INCOMPATIBLE_FORMAT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

encountered.

The bandwidth required to
stream a file is higher than the
maximum file bandwidth
allowed on the server.

The client cannot have any
more files open simultaneously.

The server received invalid data
from the client on the control
connection.

There is no stream available.

There is no more data in the
stream.

The specified server could not
be found.

The specified name is already in
use.

The specified address is already
in use.

The specified address is not a
valid multicast address.

The specified adapter address is
invalid.

The specified delivery mode is
invalid.

The specified station does not
exist.

The specified stream does not
exist.

The specified archive could not
be opened.

The system cannot find any
titles on the server.

The system cannot find the
client specified.

The Blackhole Address is not
initialized.

The station does not support
the stream format.

111 / 497


Return value/code

0xC00D0042

NS_E_INVALID_KEY

0xC00D0043

NS_E_INVALID_PORT

0xC00D0044

NS_E_INVALID_TTL

0xC00D0045

NS_E_STRIDE_REFUSED

0xC00D0046

NS_E_MMSAUTOSERVER_CANTFINDWALKER

0xC00D0047

NS_E_MAX_BITRATE

0xC00D0048

NS_E_LOGFILEPERIOD

0xC00D0049

NS_E_MAX_CLIENTS

0xC00D004A

NS_E_LOG_FILE_SIZE

0xC00D004B

NS_E_MAX_FILERATE

0xC00D004C

NS_E_WALKER_UNKNOWN

0xC00D004D

NS_E_WALKER_SERVER

0xC00D004E

NS_E_WALKER_USAGE

0xC00D0050

NS_E_TIGER_FAIL

0xC00D0053

NS_E_CUB_FAIL

0xC00D0055

NS_E_DISK_FAIL

0xC00D0060

NS_E_MAX_FUNNELS_ALERT

0xC00D0061

NS_E_ALLOCATE_FILE_FAIL

0xC00D0062

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified key is not valid.

The specified port is not valid.

The specified TTL is not valid.

The request to fast forward or
rewind could not be fulfilled.

Unable to load the appropriate
file parser.

Cannot exceed the maximum
bandwidth limit.

Invalid value for LogFilePeriod.

Cannot exceed the maximum
client limit.

The maximum log file size has
been reached.

Cannot exceed the maximum
file rate.

Unknown file type.

The specified file, %1, cannot
be loaded onto the specified
server, %2.

There was a usage error with
file parser.

The Title Server %1 has failed.

Content Server %1 (%2) has
failed.

Disk %1 ( %2 ) on Content
Server %3, has failed.

The NetShow data stream limit
of %1 streams was reached.

The NetShow Video Server was
unable to allocate a %1 block
file named %2.

A Content Server was unable to
page a block.

112 / 497


Return value/code

NS_E_PAGING_ERROR

0xC00D0063

NS_E_BAD_BLOCK0_VERSION

0xC00D0064

NS_E_BAD_DISK_UID

0xC00D0065

NS_E_BAD_FSMAJOR_VERSION

0xC00D0066

NS_E_BAD_STAMPNUMBER

0xC00D0067

NS_E_PARTIALLY_REBUILT_DISK

0xC00D0068

NS_E_ENACTPLAN_GIVEUP

0xC00D006A

MCMADM_E_REGKEY_NOT_FOUND

0xC00D006B

NS_E_NO_FORMATS

0xC00D006C

NS_E_NO_REFERENCES

0xC00D006D

NS_E_WAVE_OPEN

0xC00D006F

NS_E_CANNOTCONNECTEVENTS

0xC00D0071

NS_E_NO_DEVICE

0xC00D0072

NS_E_NO_SPECIFIED_DEVICE

0xC00D00C8

NS_E_MONITOR_GIVEUP

0xC00D00C9

NS_E_REMIRRORED_DISK

0xC00D00CA

NS_E_INSUFFICIENT_DATA

0xC00D00CB

NS_E_ASSERT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Disk %1 has unrecognized
control block version %2.

Disk %1 has incorrect uid %2.

Disk %1 has unsupported file
system major version %2.

Disk %1 has bad stamp number
in control block.

Disk %1 is partially
reconstructed.

EnactPlan gives up.

The key was not found in the
registry.

The publishing point cannot be
started because the server does
not have the appropriate
stream formats. Use the
Multicast Announcement Wizard
to create a new announcement
for this publishing point.

No reference URLs were found
in an ASX file.

Error opening wave device, the
device might be in use.

Unable to establish a connection
to the NetShow event monitor
service.

No device driver is present on
the system.

No specified device driver is
present.

Netshow Events Monitor is not
operational and has been
disconnected.

Disk %1 is remirrored.

Insufficient data found.

1 failed in file %2 line %3.

113 / 497


Return value/code

0xC00D00CC

NS_E_BAD_ADAPTER_NAME

0xC00D00CD

NS_E_NOT_LICENSED

0xC00D00CE

NS_E_NO_SERVER_CONTACT

0xC00D00CF

NS_E_TOO_MANY_TITLES

0xC00D00D0

NS_E_TITLE_SIZE_EXCEEDED

0xC00D00D1

NS_E_UDP_DISABLED

0xC00D00D2

NS_E_TCP_DISABLED

0xC00D00D3

NS_E_HTTP_DISABLED

0xC00D00D4

NS_E_LICENSE_EXPIRED

0xC00D00D5

NS_E_TITLE_BITRATE

0xC00D00D6

NS_E_EMPTY_PROGRAM_NAME

0xC00D00D7

NS_E_MISSING_CHANNEL

0xC00D00D8

NS_E_NO_CHANNELS

0xC00D00D9

NS_E_INVALID_INDEX2

0xC00D0190

NS_E_CUB_FAIL_LINK

0xC00D0192

NS_E_BAD_CUB_UID

0xC00D0195

NS_E_GLITCH_MODE

0xC00D019B

NS_E_NO_MEDIA_PROTOCOL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified adapter name is
invalid.

The application is not licensed
for this feature.

Unable to contact the server.

Maximum number of titles
exceeded.

Maximum size of a title
exceeded.

UDP protocol not enabled. Not
trying %1!ls!.

TCP protocol not enabled. Not
trying %1!ls!.

HTTP protocol not enabled. Not
trying %1!ls!.

The product license has expired.

Source file exceeds the per title
maximum bitrate. See NetShow
Theater documentation for
more information.

The program name cannot be
empty.

Station %1 does not exist.

You need to define at least one
station before this operation can
complete.

The index specified is invalid.

Content Server %1 (%2) has
failed its link to Content Server
%3.

Content Server %1 (%2) has
incorrect uid %3.

Server unreliable because
multiple components failed.

Content Server %1 (%2) is
unable to communicate with the
Media System Network Protocol.

114 / 497


Return value/code

0xC00D07F1

NS_E_NOTHING_TO_DO

0xC00D07F2

NS_E_NO_MULTICAST

0xC00D0BB8

NS_E_INVALID_INPUT_FORMAT

0xC00D0BB9

NS_E_MSAUDIO_NOT_INSTALLED

0xC00D0BBA

NS_E_UNEXPECTED_MSAUDIO_ERROR

0xC00D0BBB

NS_E_INVALID_OUTPUT_FORMAT

0xC00D0BBC

NS_E_NOT_CONFIGURED

0xC00D0BBD

NS_E_PROTECTED_CONTENT

0xC00D0BBE

NS_E_LICENSE_REQUIRED

0xC00D0BBF

NS_E_TAMPERED_CONTENT

0xC00D0BC0

NS_E_LICENSE_OUTOFDATE

0xC00D0BC1

NS_E_LICENSE_INCORRECT_RIGHTS

0xC00D0BC2

NS_E_AUDIO_CODEC_NOT_INSTALLED

0xC00D0BC3

NS_E_AUDIO_CODEC_ERROR

0xC00D0BC4

NS_E_VIDEO_CODEC_NOT_INSTALLED

0xC00D0BC5

NS_E_VIDEO_CODEC_ERROR

0xC00D0BC6

NS_E_INVALIDPROFILE

0xC00D0BC7

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Nothing to do.

Not receiving data from the
server.

The input media format is
invalid.

The MSAudio codec is not
installed on this system.

An unexpected error occurred
with the MSAudio codec.

The output media format is
invalid.

The object must be fully
configured before audio
samples can be processed.

You need a license to perform
the requested operation on this
media file.

You need a license to perform
the requested operation on this
media file.

This media file is corrupted or
invalid. Contact the content
provider for a new file.

The license for this media file
has expired. Get a new license
or contact the content provider
for further assistance.

You are not allowed to open this
file. Contact the content
provider for further assistance.

The requested audio codec is
not installed on this system.

An unexpected error occurred
with the audio codec.

The requested video codec is
not installed on this system.

An unexpected error occurred
with the video codec.

The Profile is invalid.

A new version of the SDK is
needed to play the requested

115 / 497


Return value/code

NS_E_INCOMPATIBLE_VERSION

0xC00D0BCA

NS_E_OFFLINE_MODE

0xC00D0BCB

NS_E_NOT_CONNECTED

0xC00D0BCC

NS_E_TOO_MUCH_DATA

0xC00D0BCD

NS_E_UNSUPPORTED_PROPERTY

0xC00D0BCE

NS_E_8BIT_WAVE_UNSUPPORTED

0xC00D0BCF

NS_E_NO_MORE_SAMPLES

0xC00D0BD0

NS_E_INVALID_SAMPLING_RATE

0xC00D0BD1

NS_E_MAX_PACKET_SIZE_TOO_SMALL

0xC00D0BD2

NS_E_LATE_PACKET

0xC00D0BD3

NS_E_DUPLICATE_PACKET

0xC00D0BD4

NS_E_SDK_BUFFERTOOSMALL

0xC00D0BD5

NS_E_INVALID_NUM_PASSES

0xC00D0BD6

NS_E_ATTRIBUTE_READ_ONLY

0xC00D0BD7

NS_E_ATTRIBUTE_NOT_ALLOWED

0xC00D0BD8

NS_E_INVALID_EDL

0xC00D0BD9

NS_E_DATA_UNIT_EXTENSION_TOO_LARGE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

content.

The requested URL is not
available in offline mode.

The requested URL cannot be
accessed because there is no
network connection.

The encoding process was
unable to keep up with the
amount of supplied data.

The given property is not
supported.

Windows Media Player cannot
copy the files to the CD because
they are 8-bit. Convert the files
to 16-bit, 44-kHz stereo files by
using Sound Recorder or
another audio-processing
program, and then try again.

There are no more samples in
the current range.

The given sampling rate is
invalid.

The given maximum packet size
is too small to accommodate
this profile.)

The packet arrived too late to
be of use.

The packet is a duplicate of one
received before.

Supplied buffer is too small.

The wrong number of
preprocessing passes was used
for the stream's output type.

An attempt was made to add,
modify, or delete a read only
attribute.

An attempt was made to add
attribute that is not allowed for
the given media type.

The EDL provided is invalid.

The Data Unit Extension data
was too large to be used.

116 / 497


Return value/code

0xC00D0BDA

NS_E_CODEC_DMO_ERROR

0xC00D0BDC

NS_E_FEATURE_DISABLED_BY_GROUP_POLICY

0xC00D0BDD

NS_E_FEATURE_DISABLED_IN_SKU

0xC00D0FA0

NS_E_NO_CD

0xC00D0FA1

NS_E_CANT_READ_DIGITAL

0xC00D0FA2

NS_E_DEVICE_DISCONNECTED

0xC00D0FA3

NS_E_DEVICE_NOT_SUPPORT_FORMAT

0xC00D0FA4

NS_E_SLOW_READ_DIGITAL

0xC00D0FA5

NS_E_MIXER_INVALID_LINE

0xC00D0FA6

NS_E_MIXER_INVALID_CONTROL

0xC00D0FA7

NS_E_MIXER_INVALID_VALUE

0xC00D0FA8

NS_E_MIXER_UNKNOWN_MMRESULT

0xC00D0FA9

NS_E_USER_STOP

0xC00D0FAA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An unexpected error occurred
with a DMO codec.

This feature has been disabled
by group policy.

This feature is disabled in this
SKU.

There is no CD in the CD drive.
Insert a CD, and then try again.

Windows Media Player could not
use digital playback to play the
CD. To switch to analog
playback, on the Tools menu,
click Options, and then click the
Devices tab. Double-click the
CD drive, and then in the
Playback area, click Analog. For
additional assistance, click Web
Help.

Windows Media Player no longer
detects a connected portable
device. Reconnect your portable
device, and then try
synchronizing the file again.

Windows Media Player cannot
play the file. The portable
device does not support the
specified file type.

Windows Media Player could not
use digital playback to play the
CD. The Player has
automatically switched the CD
drive to analog playback. To
switch back to digital CD
playback, use the Devices tab.
For additional assistance, click
Web Help.

An invalid line error occurred in
the mixer.

An invalid control error occurred
in the mixer.

An invalid value error occurred
in the mixer.

An unrecognized MMRESULT
occurred in the mixer.

User has stopped the operation.

Windows Media Player cannot

117 / 497


Return value/code

NS_E_MP3_FORMAT_NOT_FOUND

0xC00D0FAB

NS_E_CD_READ_ERROR_NO_CORRECTION

0xC00D0FAC

NS_E_CD_READ_ERROR

0xC00D0FAD

NS_E_CD_SLOW_COPY

0xC00D0FAE

NS_E_CD_COPYTO_CD

0xC00D0FAF

NS_E_MIXER_NODRIVER

0xC00D0FB0

NS_E_REDBOOK_ENABLED_WHILE_COPYING

0xC00D0FB1

NS_E_CD_REFRESH

0xC00D0FB2

NS_E_CD_DRIVER_PROBLEM

0xC00D0FB3

NS_E_WONT_DO_DIGITAL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

rip the track because a
compatible MP3 encoder is not
installed on your computer.
Install a compatible MP3
encoder or choose a different
format to rip to (such as
Windows Media Audio).

Windows Media Player cannot
read the CD. The disc might be
dirty or damaged. Turn on error
correction, and then try again.

Windows Media Player cannot
read the CD. The disc might be
dirty or damaged or the CD
drive might be malfunctioning.

For best performance, do not
play CD tracks while ripping
them.

It is not possible to directly
burn tracks from one CD to
another CD. You must first rip
the tracks from the CD to your
computer, and then burn the
files to a blank CD.

Could not open a sound mixer
driver.

Windows Media Player cannot
rip tracks from the CD correctly
because the CD drive settings in
Device Manager do not match
the CD drive settings in the
Player.

Windows Media Player is busy
reading the CD.

Windows Media Player could not
use digital playback to play the
CD. The Player has
automatically switched the CD
drive to analog playback. To
switch back to digital CD
playback, use the Devices tab.
For additional assistance, click
Web Help.

Windows Media Player could not
use digital playback to play the
CD. The Player has
automatically switched the CD
drive to analog playback. To
switch back to digital CD
playback, use the Devices tab.
For additional assistance, click
Web Help.

118 / 497


Return value/code

0xC00D0FB4

NS_E_WMPXML_NOERROR

0xC00D0FB5

NS_E_WMPXML_ENDOFDATA

0xC00D0FB6

NS_E_WMPXML_PARSEERROR

0xC00D0FB7

NS_E_WMPXML_ATTRIBUTENOTFOUND

0xC00D0FB8

NS_E_WMPXML_PINOTFOUND

0xC00D0FB9

NS_E_WMPXML_EMPTYDOC

0xC00D0FBA

NS_E_WMP_PATH_ALREADY_IN_LIBRARY

0xC00D0FBE

NS_E_WMP_FILESCANALREADYSTARTED

0xC00D0FBF

NS_E_WMP_HME_INVALIDOBJECTID

0xC00D0FC0

NS_E_WMP_MF_CODE_EXPIRED

0xC00D0FC1

NS_E_WMP_HME_NOTSEARCHABLEFORITEMS

0xC00D0FC7

NS_E_WMP_ADDTOLIBRARY_FAILED

0xC00D0FC8

NS_E_WMP_WINDOWSAPIFAILURE

0xC00D0FC9

NS_E_WMP_RECORDING_NOT_ALLOWED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A call was made to
GetParseError on the XML
parser but there was no error to
retrieve.

The XML Parser ran out of data
while parsing.

A generic parse error occurred
in the XML parser but no
information is available.

A call get GetNamedAttribute or
GetNamedAttributeIndex on the
XML parser resulted in the index
not being found.

A call was made go
GetNamedPI on the XML parser,
but the requested Processing
Instruction was not found.

Persist was called on the XML
parser, but the parser has no
data to persist.

This file path is already in the
library.

Windows Media Player is
already searching for files to
add to your library. Wait for the
current process to finish before
attempting to search again.

Windows Media Player is unable
to find the media you are
looking for.

A component of Windows Media
Player is out-of-date. If you are
running a pre-release version of
Windows, try upgrading to a
more recent version.

This container does not support
search on items.

Windows Media Player
encountered a problem while
adding one or more files to the
library. For additional
assistance, click Web Help.

A Windows API call failed but no
error information was available.

This file does not have burn
rights. If you obtained this file
from an online store, go to the
online store to get burn rights.

119 / 497


Return value/code

0xC00D0FCA

NS_E_DEVICE_NOT_READY

0xC00D0FCB

NS_E_DAMAGED_FILE

0xC00D0FCC

NS_E_MPDB_GENERIC

0xC00D0FCD

NS_E_FILE_FAILED_CHECKS

0xC00D0FCE

NS_E_MEDIA_LIBRARY_FAILED

0xC00D0FCF

NS_E_SHARING_VIOLATION

0xC00D0FD0

NS_E_NO_ERROR_STRING_FOUND

0xC00D0FD1

NS_E_WMPOCX_NO_REMOTE_CORE

0xC00D0FD2

NS_E_WMPOCX_NO_ACTIVE_CORE

0xC00D0FD3

NS_E_WMPOCX_NOT_RUNNING_REMOTELY

0xC00D0FD4

NS_E_WMPOCX_NO_REMOTE_WINDOW

0xC00D0FD5

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player no longer
detects a connected portable
device. Reconnect your portable
device, and then try to sync the
file again.

Windows Media Player cannot
play the file because it is
corrupted.

Windows Media Player
encountered an error while
attempting to access
information in the library. Try
restarting the Player.

The file cannot be added to the
library because it is smaller
than the "Skip files smaller
than" setting. To add the file,
change the setting on the
Library tab. For additional
assistance, click Web Help.

Windows Media Player cannot
create the library. You must be
logged on as an administrator
or a member of the
Administrators group to install
the Player. For more
information, contact your
system administrator.

The file is already in use. Close
other programs that might be
using the file, or stop playing
the file, and then try again.

Windows Media Player has
encountered an unknown error.

The Windows Media Player
ActiveX control cannot connect
to remote media services, but
will continue with local media
services.

The requested method or
property is not available
because the Windows Media
Player ActiveX control has not
been properly activated.

The Windows Media Player
ActiveX control is not running in
remote mode.

An error occurred while trying
to get the remote Windows
Media Player window.

Windows Media Player has

120 / 497


Return value/code

Description

NS_E_WMPOCX_ERRORMANAGERNOTAVAILABLE

encountered an unknown error.

0xC00D0FD6

NS_E_PLUGIN_NOTSHUTDOWN

0xC00D0FD7

NS_E_WMP_CANNOT_FIND_FOLDER

0xC00D0FD8

NS_E_WMP_STREAMING_RECORDING_NOT_ALLOWED

0xC00D0FD9

NS_E_WMP_PLUGINDLL_NOTFOUND

0xC00D0FDA

NS_E_NEED_TO_ASK_USER

0xC00D0FDB

NS_E_WMPOCX_PLAYER_NOT_DOCKED

0xC00D0FDC

NS_E_WMP_EXTERNAL_NOTREADY

0xC00D0FDD

NS_E_WMP_MLS_STALE_DATA

0xC00D0FDE

NS_E_WMP_UI_SUBCONTROLSNOTSUPPORTED

0xC00D0FDF

NS_E_WMP_UI_VERSIONMISMATCH

0xC00D0FE0

NS_E_WMP_UI_NOTATHEMEFILE

0xC00D0FE1

NS_E_WMP_UI_SUBELEMENTNOTFOUND

0xC00D0FE2

NS_E_WMP_UI_VERSIONPARSE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Windows Media Player was not
closed properly. A damaged or
incompatible plug-in might have
caused the problem to occur. As
a precaution, all optional plug-
ins have been disabled.

Windows Media Player cannot
find the specified path. Verify
that the path is typed correctly.
If it is, the path does not exist
in the specified location, or the
computer where the path is
located is not available.

Windows Media Player cannot
save a file that is being
streamed.

Windows Media Player cannot
find the selected plug-in. The
Player will try to remove it from
the menu. To use this plug-in,
install it again.

Action requires input from the
user.

The Windows Media Player
ActiveX control must be in a
docked state for this action to
be performed.

The Windows Media Player
external object is not ready.

Windows Media Player cannot
perform the requested action.
Your computer's time and date
might not be set correctly.

The control (%s) does not
support creation of sub-
controls, yet (%d) sub-controls
have been specified.

Version mismatch: (%.1f
required, %.1f found).

The layout manager was given
valid XML that wasn't a theme
file.

The %s subelement could not
be found on the %s object.

An error occurred parsing the
version tag. Valid version tags
are of the form: <?wmp
version='1.0'?>.

121 / 497


Return value/code

0xC00D0FE3

NS_E_WMP_UI_VIEWIDNOTFOUND

0xC00D0FE4

NS_E_WMP_UI_PASSTHROUGH

0xC00D0FE5

NS_E_WMP_UI_OBJECTNOTFOUND

0xC00D0FE6

NS_E_WMP_UI_SECONDHANDLER

0xC00D0FE7

NS_E_WMP_UI_NOSKININZIP

0xC00D0FEA

NS_E_WMP_URLDOWNLOADFAILED

0xC00D0FEB

NS_E_WMPOCX_UNABLE_TO_LOAD_SKIN

0xC00D0FEC

NS_E_WMP_INVALID_SKIN

0xC00D0FED

NS_E_WMP_SENDMAILFAILED

0xC00D0FEE

NS_E_WMP_LOCKEDINSKINMODE

0xC00D0FEF

NS_E_WMP_FAILED_TO_SAVE_FILE

0xC00D0FF0

NS_E_WMP_SAVEAS_READONLY

0xC00D0FF1

NS_E_WMP_FAILED_TO_SAVE_PLAYLIST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The view specified in for the
'currentViewID' property (%s)
was not found in this theme file.

This error used internally for hit
testing.

Attributes were specified for the
%s object, but the object was
not available to send them to.

The %s event already has a
handler, the second handler
was ignored.

No .wms file found in skin
archive.

Windows Media Player
encountered a problem while
downloading the file. For
additional assistance, click Web
Help.

The Windows Media Player
ActiveX control cannot load the
requested uiMode and cannot
roll back to the existing uiMode.

Windows Media Player
encountered a problem with the
skin file. The skin file might not
be valid.

Windows Media Player cannot
send the link because your
email program is not
responding. Verify that your
email program is configured
properly, and then try again.
For more information about
email, see Windows Help.

Windows Media Player cannot
switch to full mode because
your computer administrator
has locked this skin.

Windows Media Player
encountered a problem while
saving the file. For additional
assistance, click Web Help.

Windows Media Player cannot
overwrite a read-only file. Try
using a different file name.

Windows Media Player
encountered a problem while
creating or saving the playlist.
For additional assistance, click
Web Help.

122 / 497


Return value/code

0xC00D0FF2

NS_E_WMP_FAILED_TO_OPEN_WMD

0xC00D0FF3

NS_E_WMP_CANT_PLAY_PROTECTED

0xC00D0FF4

NS_E_SHARING_STATE_OUT_OF_SYNC

0xC00D0FFA

NS_E_WMPOCX_REMOTE_PLAYER_ALREADY_RUNNING

0xC00D1004

NS_E_WMP_RBC_JPGMAPPINGIMAGE

0xC00D1005

NS_E_WMP_JPGTRANSPARENCY

0xC00D1009

NS_E_WMP_INVALID_MAX_VAL

0xC00D100A

NS_E_WMP_INVALID_MIN_VAL

0xC00D100E

NS_E_WMP_CS_JPGPOSITIONIMAGE

0xC00D100F

NS_E_WMP_CS_NOTEVENLYDIVISIBLE

0xC00D1018

NS_E_WMPZIP_NOTAZIPFILE

0xC00D1019

NS_E_WMPZIP_CORRUPT

0xC00D101A

NS_E_WMPZIP_FILENOTFOUND

0xC00D1022

NS_E_WMP_IMAGE_FILETYPE_UNSUPPORTED

0xC00D1023

NS_E_WMP_IMAGE_INVALID_FORMAT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
open the Windows Media
Download file. The file might be
damaged.

The file cannot be added to the
library because it is a protected
DVR-MS file. This content
cannot be played back by
Windows Media Player.

Media sharing has been turned
off because a required Windows
setting or component has
changed. For additional
assistance, click Web Help.

Exclusive Services launch failed
because the Windows Media
Player is already running.

JPG Images are not
recommended for use as a
mappingImage.

JPG Images are not
recommended when using a
transparencyColor.

The Max property cannot be
less than Min property.

The Min property cannot be
greater than Max property.

JPG Images are not
recommended for use as a
positionImage.

The (%s) image's size is not
evenly divisible by the
positionImage's size.

The ZIP reader opened a file
and its signature did not match
that of the ZIP files.

The ZIP reader has detected
that the file is corrupted.

GetFileStream, SaveToFile, or
SaveTemp file was called on the
ZIP reader with a file name that
was not found in the ZIP file.

Image type not supported.

Image file might be corrupt.

123 / 497


Return value/code

0xC00D1024

NS_E_WMP_GIF_UNEXPECTED_ENDOFFILE

0xC00D1025

NS_E_WMP_GIF_INVALID_FORMAT

0xC00D1026

NS_E_WMP_GIF_BAD_VERSION_NUMBER

0xC00D1027

NS_E_WMP_GIF_NO_IMAGE_IN_FILE

0xC00D1028

NS_E_WMP_PNG_INVALIDFORMAT

0xC00D1029

NS_E_WMP_PNG_UNSUPPORTED_BITDEPTH

0xC00D102A

NS_E_WMP_PNG_UNSUPPORTED_COMPRESSION

0xC00D102B

NS_E_WMP_PNG_UNSUPPORTED_FILTER

0xC00D102C

NS_E_WMP_PNG_UNSUPPORTED_INTERLACE

0xC00D102D

NS_E_WMP_PNG_UNSUPPORTED_BAD_CRC

0xC00D102E

NS_E_WMP_BMP_INVALID_BITMASK

0xC00D102F

NS_E_WMP_BMP_TOPDOWN_DIB_UNSUPPORTED

0xC00D1030

NS_E_WMP_BMP_BITMAP_NOT_CREATED

0xC00D1031

NS_E_WMP_BMP_COMPRESSION_UNSUPPORTED

0xC00D1032

NS_E_WMP_BMP_INVALID_FORMAT

0xC00D1033

NS_E_WMP_JPG_JERR_ARITHCODING_NOTIMPL

0xC00D1034

NS_E_WMP_JPG_INVALID_FORMAT

0xC00D1035

NS_E_WMP_JPG_BAD_DCTSIZE

0xC00D1036

NS_E_WMP_JPG_BAD_VERSION_NUMBER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unexpected end of file. GIF file
might be corrupt.

Invalid GIF file.

Invalid GIF version. Only 87a or
89a supported.

No images found in GIF file.

Invalid PNG image file format.

PNG bitdepth not supported.

Compression format defined in
PNG file not supported,

Filter method defined in PNG file
not supported.

Interlace method defined in
PNG file not supported.

Bad CRC in PNG file.

Invalid bitmask in BMP file.

Topdown DIB not supported.

Bitmap could not be created.

Compression format defined in
BMP not supported.

Invalid Bitmap format.

JPEG Arithmetic coding not
supported.

Invalid JPEG format.

Invalid JPEG format.

Internal version error.
Unexpected JPEG library
version.

124 / 497


Return value/code

0xC00D1037

NS_E_WMP_JPG_BAD_PRECISION

0xC00D1038

NS_E_WMP_JPG_CCIR601_NOTIMPL

0xC00D1039

NS_E_WMP_JPG_NO_IMAGE_IN_FILE

0xC00D103A

NS_E_WMP_JPG_READ_ERROR

0xC00D103B

NS_E_WMP_JPG_FRACT_SAMPLE_NOTIMPL

0xC00D103C

NS_E_WMP_JPG_IMAGE_TOO_BIG

0xC00D103D

NS_E_WMP_JPG_UNEXPECTED_ENDOFFILE

0xC00D103E

NS_E_WMP_JPG_SOF_UNSUPPORTED

0xC00D103F

NS_E_WMP_JPG_UNKNOWN_MARKER

0xC00D1044

NS_E_WMP_FAILED_TO_OPEN_IMAGE

0xC00D1049

NS_E_WMP_DAI_SONGTOOSHORT

0xC00D104A

NS_E_WMG_RATEUNAVAILABLE

0xC00D104B

NS_E_WMG_PLUGINUNAVAILABLE

0xC00D104C

NS_E_WMG_CANNOTQUEUE

0xC00D104D

NS_E_WMG_PREROLLLICENSEACQUISITIONNOTALLOWED

0xC00D104E

NS_E_WMG_UNEXPECTEDPREROLLSTATUS

0xC00D1051

NS_E_WMG_INVALID_COPP_CERTIFICATE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Internal JPEG Library error.
Unsupported JPEG data
precision.

JPEG CCIR601 not supported.

No image found in JPEG file.

Could not read JPEG file.

JPEG Fractional sampling not
supported.

JPEG image too large. Maximum
image size supported is 65500
X 65500.

Unexpected end of file reached
in JPEG file.

Unsupported JPEG SOF marker
found.

Unknown JPEG marker found.

Windows Media Player cannot
display the picture file. The
player either does not support
the picture type or the picture is
corrupted.

Windows Media Player cannot
compute a Digital Audio Id for
the song. It is too short.

Windows Media Player cannot
play the file at the requested
speed.

The rendering or digital signal
processing plug-in cannot be
instantiated.

The file cannot be queued for
seamless playback.

Windows Media Player cannot
download media usage rights
for a file in the playlist.

Windows Media Player
encountered an error while
trying to queue a file.

Windows Media Player cannot
play the protected file. The

125 / 497


Return value/code

0xC00D1052

NS_E_WMG_COPP_SECURITY_INVALID

0xC00D1053

NS_E_WMG_COPP_UNSUPPORTED

0xC00D1054

NS_E_WMG_INVALIDSTATE

0xC00D1055

NS_E_WMG_SINKALREADYEXISTS

0xC00D1056

NS_E_WMG_NOSDKINTERFACE

0xC00D1057

NS_E_WMG_NOTALLOUTPUTSRENDERED

0xC00D1058

NS_E_WMG_FILETRANSFERNOTALLOWED

0xC00D1059

NS_E_WMR_UNSUPPORTEDSTREAM

0xC00D105A

NS_E_WMR_PINNOTFOUND

0xC00D105B

NS_E_WMR_WAITINGONFORMATSWITCH

0xC00D105C

NS_E_WMR_NOSOURCEFILTER

0xC00D105D

NS_E_WMR_PINTYPENOMATCH

0xC00D105E

NS_E_WMR_NOCALLBACKAVAILABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Player cannot verify that the
connection to your video card is
secure. Try installing an
updated device driver for your
video card.

Windows Media Player cannot
play the protected file. The
Player detected that the
connection to your hardware
might not be secure.

Windows Media Player output
link protection is unsupported
on this system.

Operation attempted in an
invalid graph state.

A renderer cannot be inserted in
a stream while one already
exists.

The Windows Media SDK
interface needed to complete
the operation does not exist at
this time.

Windows Media Player cannot
play a portion of the file
because it requires a codec that
either could not be downloaded
or that is not supported by the
Player.

File transfer streams are not
allowed in the standalone
Player.

Windows Media Player cannot
play the file. The Player does
not support the format you are
trying to play.

An operation was attempted on
a pin that does not exist in the
DirectShow filter graph.

Specified operation cannot be
completed while waiting for a
media format change from the
SDK.

Specified operation cannot be
completed because the source
filter does not exist.

The specified type does not
match this pin.

The WMR Source Filter does not
have a callback available.

126 / 497


Return value/code

0xC00D1062

NS_E_WMR_SAMPLEPROPERTYNOTSET

0xC00D1063

NS_E_WMR_CANNOT_RENDER_BINARY_STREAM

0xC00D1064

NS_E_WMG_LICENSE_TAMPERED

0xC00D1065

NS_E_WMR_WILLNOT_RENDER_BINARY_STREAM

0xC00D1068

NS_E_WMX_UNRECOGNIZED_PLAYLIST_FORMAT

0xC00D1069

NS_E_ASX_INVALIDFORMAT

0xC00D106A

NS_E_ASX_INVALIDVERSION

0xC00D106B

NS_E_ASX_INVALID_REPEAT_BLOCK

0xC00D106C

NS_E_ASX_NOTHING_TO_WRITE

0xC00D106D

NS_E_URLLIST_INVALIDFORMAT

0xC00D106E

NS_E_WMX_ATTRIBUTE_DOES_NOT_EXIST

0xC00D106F

NS_E_WMX_ATTRIBUTE_ALREADY_EXISTS

0xC00D1070

NS_E_WMX_ATTRIBUTE_UNRETRIEVABLE

0xC00D1071

NS_E_WMX_ITEM_DOES_NOT_EXIST

0xC00D1072

NS_E_WMX_ITEM_TYPE_ILLEGAL

0xC00D1073

NS_E_WMX_ITEM_UNSETTABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified property has not
been set on this sample.

A plug-in is required to correctly
play the file. To determine if the
plug-in is available to download,
click Web Help.

Windows Media Player cannot
play the file because your
media usage rights are
corrupted. If you previously
backed up your media usage
rights, try restoring them.

Windows Media Player cannot
play protected files that contain
binary streams.

Windows Media Player cannot
play the playlist because it is
not valid.

Windows Media Player cannot
play the playlist because it is
not valid.

A later version of Windows
Media Player might be required
to play this playlist.

The format of a REPEAT loop
within the current playlist file is
not valid.

Windows Media Player cannot
save the playlist because it does
not contain any items.

Windows Media Player cannot
play the playlist because it is
not valid.

The specified attribute does not
exist.

The specified attribute already
exists.

Cannot retrieve the specified
attribute.

The specified item does not
exist in the current playlist.

Items of the specified type
cannot be created within the
current playlist.

The specified item cannot be set
in the current playlist.

127 / 497


Return value/code

0xC00D1074

NS_E_WMX_PLAYLIST_EMPTY

0xC00D1075

NS_E_MLS_SMARTPLAYLIST_FILTER_NOT_REGISTERED

0xC00D1076

NS_E_WMX_INVALID_FORMAT_OVER_NESTING

0xC00D107C

NS_E_WMPCORE_NOSOURCEURLSTRING

0xC00D107D

NS_E_WMPCORE_COCREATEFAILEDFORGITOBJECT

0xC00D107E

NS_E_WMPCORE_FAILEDTOGETMARSHALLEDEVENTHANDLERINTERFACE

0xC00D107F

NS_E_WMPCORE_BUFFERTOOSMALL

0xC00D1080

NS_E_WMPCORE_UNAVAILABLE

0xC00D1081

NS_E_WMPCORE_INVALIDPLAYLISTMODE

0xC00D1086

NS_E_WMPCORE_ITEMNOTINPLAYLIST

0xC00D1087

NS_E_WMPCORE_PLAYLISTEMPTY

0xC00D1088

NS_E_WMPCORE_NOBROWSER

0xC00D1089

NS_E_WMPCORE_UNRECOGNIZED_MEDIA_URL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
perform the requested action
because the playlist does not
contain any items.

The specified auto playlist
contains a filter type that is
either not valid or is not
installed on this computer.

Windows Media Player cannot
play the file because the
associated playlist contains too
many nested playlists.

Windows Media Player cannot
find the file. Verify that the path
is typed correctly. If it is, the
file might not exist in the
specified location, or the
computer where the file is
stored might not be available.

Failed to create the Global
Interface Table.

Failed to get the marshaled
graph event handler interface.

Buffer is too small for copying
media type.

The current state of the Player
does not allow this operation.

The playlist manager does not
understand the current play
mode (for example, shuffle or
normal).

Windows Media Player cannot
play the file because it is not in
the current playlist.

There are no items in the
playlist. Add items to the
playlist, and then try again.

The web page cannot be
displayed because no web
browser is installed on your
computer.

Windows Media Player cannot
find the specified file. Verify the
path is typed correctly. If it is,
the file does not exist in the
specified location, or the
computer where the file is
stored is not available.

128 / 497


Return value/code

0xC00D108A

NS_E_WMPCORE_GRAPH_NOT_IN_LIST

0xC00D108B

NS_E_WMPCORE_PLAYLIST_EMPTY_OR_SINGLE_MEDIA

0xC00D108C

NS_E_WMPCORE_ERRORSINKNOTREGISTERED

0xC00D108D

NS_E_WMPCORE_ERRORMANAGERNOTAVAILABLE

0xC00D108E

NS_E_WMPCORE_WEBHELPFAILED

0xC00D108F

NS_E_WMPCORE_MEDIA_ERROR_RESUME_FAILED

0xC00D1090

NS_E_WMPCORE_NO_REF_IN_ENTRY

0xC00D1091

NS_E_WMPCORE_WMX_LIST_ATTRIBUTE_NAME_EMPTY

0xC00D1092

NS_E_WMPCORE_WMX_LIST_ATTRIBUTE_NAME_ILLEGAL

0xC00D1093

NS_E_WMPCORE_WMX_LIST_ATTRIBUTE_VALUE_EMPTY

0xC00D1094

NS_E_WMPCORE_WMX_LIST_ATTRIBUTE_VALUE_ILLEGAL

0xC00D1095

NS_E_WMPCORE_WMX_LIST_ITEM_ATTRIBUTE_NAME_EMPTY

0xC00D1096

NS_E_WMPCORE_WMX_LIST_ITEM_ATTRIBUTE_NAME_ILLEGAL

0xC00D1097

NS_E_WMPCORE_WMX_LIST_ITEM_ATTRIBUTE_VALUE_EMPTY

0xC00D1098

NS_E_WMPCORE_LIST_ENTRY_NO_REF

0xC00D1099

NS_E_WMPCORE_MISNAMED_FILE

0xC00D109A

NS_E_WMPCORE_CODEC_NOT_TRUSTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Graph with the specified URL
was not found in the prerolled
graph list.

Windows Media Player cannot
perform the requested
operation because there is only
one item in the playlist.

An error sink was never
registered for the calling object.

The error manager is not
available to respond to errors.

The Web Help URL cannot be
opened.

Could not resume playing next
item in playlist.

Windows Media Player cannot
play the file because the
associated playlist does not
contain any items or the playlist
is not valid.

An empty string for playlist
attribute name was found.

A playlist attribute name that is
not valid was found.

An empty string for a playlist
attribute value was found.

An illegal value for a playlist
attribute was found.

An empty string for a playlist
item attribute name was found.

An illegal value for a playlist
item attribute name was found.

An illegal value for a playlist
item attribute was found.

The playlist does not contain
any items.

Windows Media Player cannot
play the file. The file is either
corrupted or the Player does not
support the format you are
trying to play.

The codec downloaded for this
file does not appear to be

129 / 497


Return value/code

0xC00D109B

NS_E_WMPCORE_CODEC_NOT_FOUND

0xC00D109C

NS_E_WMPCORE_CODEC_DOWNLOAD_NOT_ALLOWED

0xC00D109D

NS_E_WMPCORE_ERROR_DOWNLOADING_PLAYLIST

0xC00D109E

NS_E_WMPCORE_FAILED_TO_BUILD_PLAYLIST

0xC00D109F

NS_E_WMPCORE_PLAYLIST_ITEM_ALTERNATE_NONE

0xC00D10A0

NS_E_WMPCORE_PLAYLIST_ITEM_ALTERNATE_EXHAUSTED

0xC00D10A1

NS_E_WMPCORE_PLAYLIST_ITEM_ALTERNATE_NAME_NOT_FOUND

0xC00D10A2

NS_E_WMPCORE_PLAYLIST_ITEM_ALTERNATE_MORPH_FAILED

0xC00D10A3

NS_E_WMPCORE_PLAYLIST_ITEM_ALTERNATE_INIT_FAILED

0xC00D10A4

NS_E_WMPCORE_MEDIA_ALTERNATE_REF_EMPTY

0xC00D10A5

NS_E_WMPCORE_PLAYLIST_NO_EVENT_NAME

0xC00D10A6

NS_E_WMPCORE_PLAYLIST_EVENT_ATTRIBUTE_ABSENT

0xC00D10A7

NS_E_WMPCORE_PLAYLIST_EVENT_EMPTY

0xC00D10A8

NS_E_WMPCORE_PLAYLIST_STACK_EMPTY

0xC00D10A9

NS_E_WMPCORE_CURRENT_MEDIA_NOT_ACTIVE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

properly signed, so it cannot be
installed.

Windows Media Player cannot
play the file. One or more
codecs required to play the file
could not be found.

Windows Media Player cannot
play the file because a required
codec is not installed on your
computer. To try downloading
the codec, turn on the
"Download codecs
automatically" option.

Windows Media Player
encountered a problem while
downloading the playlist. For
additional assistance, click Web
Help.

Failed to build the playlist.

Playlist has no alternates to
switch into.

No more playlist alternates
available to switch to.

Could not find the name of the
alternate playlist to switch into.

Failed to switch to an alternate
for this media.

Failed to initialize an alternate
for the media.

No URL specified for the roll
over Refs in the playlist file.

Encountered a playlist with no
name.

A required attribute in the event
block of the playlist was not
found.

No items were found in the
event block of the playlist.

No playlist was found while
returning from a nested playlist.

The media item is not active
currently.

130 / 497


Return value/code

0xC00D10AB

NS_E_WMPCORE_USER_CANCEL

0xC00D10AC

NS_E_WMPCORE_PLAYLIST_REPEAT_EMPTY

0xC00D10AD

NS_E_WMPCORE_PLAYLIST_REPEAT_START_MEDIA_NONE

0xC00D10AE

NS_E_WMPCORE_PLAYLIST_REPEAT_END_MEDIA_NONE

0xC00D10AF

NS_E_WMPCORE_INVALID_PLAYLIST_URL

0xC00D10B0

NS_E_WMPCORE_MISMATCHED_RUNTIME

0xC00D10B1

NS_E_WMPCORE_PLAYLIST_IMPORT_FAILED_NO_ITEMS

0xC00D10B2

NS_E_WMPCORE_VIDEO_TRANSFORM_FILTER_INSERTION

0xC00D10B3

NS_E_WMPCORE_MEDIA_UNAVAILABLE

0xC00D10B4

NS_E_WMPCORE_WMX_ENTRYREF_NO_REF

0xC00D10B5

NS_E_WMPCORE_NO_PLAYABLE_MEDIA_IN_PLAYLIST

0xC00D10B6

NS_E_WMPCORE_PLAYLIST_EMPTY_NESTED_PLAYLIST_SKIPPED_ITEMS

0xC00D10B7

NS_E_WMPCORE_BUSY

0xC00D10B8

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
perform the requested action
because you chose to cancel it.

Windows Media Player
encountered a problem with the
playlist. The format of the
playlist is not valid.

Media object corresponding to
start of a playlist repeat block
was not found.

Media object corresponding to
the end of a playlist repeat
block was not found.

The playlist URL supplied to the
playlist manager is not valid.

Windows Media Player cannot
play the file because it is
corrupted.

Windows Media Player cannot
add the playlist to the library
because the playlist does not
contain any items.

An error has occurred that could
prevent the changing of the
video contrast on this media.

Windows Media Player cannot
play the file. If the file is located
on the Internet, connect to the
Internet. If the file is located on
a removable storage card,
insert the storage card.

The playlist contains an
ENTRYREF for which no href
was parsed. Check the syntax
of playlist file.

Windows Media Player cannot
play any items in the playlist.
To find information about the
problem, click the Now Playing
tab, and then click the icon next
to each file in the List pane.

Windows Media Player cannot
play some or all of the items in
the playlist because the playlist
is nested.

Windows Media Player cannot
play the file at this time. Try
again later.

There is no child playlist

131 / 497


Return value/code

NS_E_WMPCORE_MEDIA_CHILD_PLAYLIST_UNAVAILABLE

0xC00D10B9

NS_E_WMPCORE_MEDIA_NO_CHILD_PLAYLIST

0xC00D10BA

NS_E_WMPCORE_FILE_NOT_FOUND

0xC00D10BB

NS_E_WMPCORE_TEMP_FILE_NOT_FOUND

0xC00D10BC

NS_E_WMDM_REVOKED

0xC00D10BD

NS_E_DDRAW_GENERIC

0xC00D10BE

NS_E_DISPLAY_MODE_CHANGE_FAILED

0xC00D10BF

NS_E_PLAYLIST_CONTAINS_ERRORS

0xC00D10C0

NS_E_CHANGING_PROXY_NAME

0xC00D10C1

NS_E_CHANGING_PROXY_PORT

0xC00D10C2

NS_E_CHANGING_PROXY_EXCEPTIONLIST

0xC00D10C3

NS_E_CHANGING_PROXYBYPASS

0xC00D10C4

NS_E_CHANGING_PROXY_PROTOCOL_NOT_FOUND

0xC00D10C5

NS_E_GRAPH_NOAUDIOLANGUAGE

0xC00D10C6

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

available for this media item at
this time.

There is no child playlist for this
media item.

Windows Media Player cannot
find the file. The link from the
item in the library to its
associated digital media file
might be broken. To fix the
problem, try repairing the link
or removing the item from the
library.

The temporary file was not
found.

Windows Media Player cannot
sync the file because the device
needs to be updated.

Windows Media Player cannot
play the video because there is
a problem with your video card.

Windows Media Player failed to
change the screen mode for
full-screen video playback.

Windows Media Player cannot
play one or more files. For
additional information, right-
click an item that cannot be
played, and then click Error
Details.

Cannot change the proxy name
if the proxy setting is not set to
custom.

Cannot change the proxy port if
the proxy setting is not set to
custom.

Cannot change the proxy
exception list if the proxy
setting is not set to custom.

Cannot change the proxy
bypass flag if the proxy setting
is not set to custom.

Cannot find the specified
protocol.

Cannot change the language
settings. Either the graph has
no audio or the audio only
supports one language.

The graph has no audio

132 / 497


Return value/code

NS_E_GRAPH_NOAUDIOLANGUAGESELECTED

0xC00D10C7

NS_E_CORECD_NOTAMEDIACD

0xC00D10C8

NS_E_WMPCORE_MEDIA_URL_TOO_LONG

0xC00D10C9

NS_E_WMPFLASH_CANT_FIND_COM_SERVER

0xC00D10CA

NS_E_WMPFLASH_INCOMPATIBLEVERSION

0xC00D10CB

NS_E_WMPOCXGRAPH_IE_DISALLOWS_ACTIVEX_CONTROLS

0xC00D10CC

NS_E_NEED_CORE_REFERENCE

0xC00D10CD

NS_E_MEDIACD_READ_ERROR

0xC00D10CE

NS_E_IE_DISALLOWS_ACTIVEX_CONTROLS

0xC00D10CF

NS_E_FLASH_PLAYBACK_NOT_ALLOWED

0xC00D10D0

NS_E_UNABLE_TO_CREATE_RIP_LOCATION

0xC00D10D1

NS_E_WMPCORE_SOME_CODECS_MISSING

0xC00D10D2

NS_E_WMP_RIP_FAILED

0xC00D10D3

NS_E_WMP_FAILED_TO_RIP_TRACK

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

language selected.

This is not a media CD.

Windows Media Player cannot
play the file because the URL is
too long.

To play the selected item, you
must install the Macromedia
Flash Player. To download the
Macromedia Flash Player, go to
the Adobe website.

To play the selected item, you
must install a later version of
the Macromedia Flash Player.
To download the Macromedia
Flash Player, go to the Adobe
website.

Windows Media Player cannot
play the file because your
Internet security settings
prohibit the use of ActiveX
controls.

The use of this method requires
an existing reference to the
Player object.

Windows Media Player cannot
play the CD. The disc might be
dirty or damaged.

Windows Media Player cannot
play the file because your
Internet security settings
prohibit the use of ActiveX
controls.

Flash playback has been turned
off in Windows Media Player.

Windows Media Player cannot
rip the CD because a valid rip
location cannot be created.

Windows Media Player cannot
play the file because a required
codec is not installed on your
computer.

Windows Media Player cannot
rip one or more tracks from the
CD.

Windows Media Player
encountered a problem while
ripping the track from the CD.
For additional assistance, click

133 / 497


Return value/code

0xC00D10D4

NS_E_WMP_ERASE_FAILED

0xC00D10D5

NS_E_WMP_FORMAT_FAILED

0xC00D10D6

NS_E_WMP_CANNOT_BURN_NON_LOCAL_FILE

0xC00D10D7

NS_E_WMP_FILE_TYPE_CANNOT_BURN_TO_AUDIO_CD

0xC00D10D8

NS_E_WMP_FILE_DOES_NOT_FIT_ON_CD

0xC00D10D9

NS_E_WMP_FILE_NO_DURATION

0xC00D10DA

NS_E_PDA_FAILED_TO_BURN

0xC00D10DC

NS_E_FAILED_DOWNLOAD_ABORT_BURN

0xC00D10DD

NS_E_WMPCORE_DEVICE_DRIVERS_MISSING

0xC00D1126

NS_E_WMPIM_USEROFFLINE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Web Help.

Windows Media Player
encountered a problem while
erasing the disc. For additional
assistance, click Web Help.

Windows Media Player
encountered a problem while
formatting the device. For
additional assistance, click Web
Help.

This file cannot be burned to a
CD because it is not located on
your computer.

It is not possible to burn this
file type to an audio CD.
Windows Media Player can burn
the following file types to an
audio CD: WMA, MP3, or WAV.

This file is too large to fit on a
disc.

It is not possible to determine if
this file can fit on a disc
because Windows Media Player
cannot detect the length of the
file. Playing the file before
burning might enable the Player
to detect the file length.

Windows Media Player
encountered a problem while
burning the file to the disc. For
additional assistance, click Web
Help.

Windows Media Player cannot
burn the audio CD because
some items in the list that you
chose to buy could not be
downloaded from the online
store.

Windows Media Player cannot
play the file. Try using Windows
Update or Device Manager to
update the device drivers for
your audio and video cards. For
information about using
Windows Update or Device
Manager, see Windows Help.

Windows Media Player has
detected that you are not
connected to the Internet.
Connect to the Internet, and
then try again.

134 / 497


Return value/code

0xC00D1127

NS_E_WMPIM_USERCANCELED

0xC00D1128

NS_E_WMPIM_DIALUPFAILED

0xC00D1129

NS_E_WINSOCK_ERROR_STRING

0xC00D1130

NS_E_WMPBR_NOLISTENER

0xC00D1131

NS_E_WMPBR_BACKUPCANCEL

0xC00D1132

NS_E_WMPBR_RESTORECANCEL

0xC00D1133

NS_E_WMPBR_ERRORWITHURL

0xC00D1134

NS_E_WMPBR_NAMECOLLISION

0xC00D1137

NS_E_WMPBR_DRIVE_INVALID

0xC00D1138

NS_E_WMPBR_BACKUPRESTOREFAILED

0xC00D1158

NS_E_WMP_CONVERT_FILE_FAILED

0xC00D1159

NS_E_WMP_CONVERT_NO_RIGHTS_ERRORURL

0xC00D115A

NS_E_WMP_CONVERT_NO_RIGHTS_NOERRORURL

0xC00D115B

NS_E_WMP_CONVERT_FILE_CORRUPT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The attempt to connect to the
Internet was canceled.

The attempt to connect to the
Internet failed.

Windows Media Player has
encountered an unknown
network error.

No window is currently listening
to Backup and Restore events.

Your media usage rights were
not backed up because the
backup was canceled.

Your media usage rights were
not restored because the
restoration was canceled.

An error occurred while backing
up or restoring your media
usage rights. A required web
page cannot be displayed.

Your media usage rights were
not backed up because the
backup was canceled.

Windows Media Player cannot
restore your media usage rights
from the specified location.
Choose another location, and
then try again.

Windows Media Player cannot
backup or restore your media
usage rights.

Windows Media Player cannot
add the file to the library.

Windows Media Player cannot
add the file to the library
because the content provider
prohibits it. For assistance,
contact the company that
provided the file.

Windows Media Player cannot
add the file to the library
because the content provider
prohibits it. For assistance,
contact the company that
provided the file.

Windows Media Player cannot
add the file to the library. The
file might not be valid.

135 / 497


Return value/code

0xC00D115C

NS_E_WMP_CONVERT_PLUGIN_UNAVAILABLE_ERRORURL

0xC00D115D

NS_E_WMP_CONVERT_PLUGIN_UNAVAILABLE_NOERRORURL

0xC00D115E

NS_E_WMP_CONVERT_PLUGIN_UNKNOWN_FILE_OWNER

0xC00D1160

NS_E_DVD_DISC_COPY_PROTECT_OUTPUT_NS

0xC00D1161

NS_E_DVD_DISC_COPY_PROTECT_OUTPUT_FAILED

0xC00D1162

NS_E_DVD_NO_SUBPICTURE_STREAM

0xC00D1163

NS_E_DVD_COPY_PROTECT

0xC00D1164

NS_E_DVD_AUTHORING_PROBLEM

0xC00D1165

NS_E_DVD_INVALID_DISC_REGION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
add the file to the library. The
plug-in required to add the file
is not installed properly. For
assistance, click Web Help to
display the website of the
company that provided the file.

Windows Media Player cannot
add the file to the library. The
plug-in required to add the file
is not installed properly. For
assistance, contact the
company that provided the file.

Windows Media Player cannot
add the file to the library. The
plug-in required to add the file
is not installed properly. For
assistance, contact the
company that provided the file.

Windows Media Player cannot
play this DVD. Try installing an
updated driver for your video
card or obtaining a newer video
card.

This DVD's resolution exceeds
the maximum allowed by your
component video outputs. Try
reducing your screen resolution
to 640 x 480, or turn off analog
component outputs and use a
VGA connection to your
monitor.

Windows Media Player cannot
display subtitles or highlights in
DVD menus. Reinstall the DVD
decoder or contact the DVD
drive manufacturer to obtain an
updated decoder.

Windows Media Player cannot
play this DVD because there is
a problem with digital copy
protection between your DVD
drive, decoder, and video card.
Try installing an updated driver
for your video card.

Windows Media Player cannot
play the DVD. The disc was
created in a manner that the
Player does not support.

Windows Media Player cannot
play the DVD because the disc
prohibits playback in your
region of the world. You must
obtain a disc that is intended for

136 / 497


Return value/code

0xC00D1166

NS_E_DVD_COMPATIBLE_VIDEO_CARD

0xC00D1167

NS_E_DVD_MACROVISION

0xC00D1168

NS_E_DVD_SYSTEM_DECODER_REGION

0xC00D1169

NS_E_DVD_DISC_DECODER_REGION

0xC00D116A

NS_E_DVD_NO_VIDEO_STREAM

0xC00D116B

NS_E_DVD_NO_AUDIO_STREAM

0xC00D116C

NS_E_DVD_GRAPH_BUILDING

0xC00D116D

NS_E_DVD_NO_DECODER

0xC00D116E

NS_E_DVD_PARENTAL

0xC00D116F

NS_E_DVD_CANNOT_JUMP

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

your geographic region.

Windows Media Player cannot
play the DVD because your
video card does not support
DVD playback.

Windows Media Player cannot
play this DVD because it is not
possible to turn on analog copy
protection on the output
display. Try installing an
updated driver for your video
card.

Windows Media Player cannot
play the DVD because the
region assigned to your DVD
drive does not match the region
assigned to your DVD decoder.

Windows Media Player cannot
play the DVD because the disc
prohibits playback in your
region of the world. You must
obtain a disc that is intended for
your geographic region.

Windows Media Player cannot
play DVD video. You might need
to adjust your Windows display
settings. Open display settings
in Control Panel, and then try
lowering your screen resolution
and color quality settings.

Windows Media Player cannot
play DVD audio. Verify that
your sound card is set up
correctly, and then try again.

Windows Media Player cannot
play DVD video. Close any open
files and quit any other
programs, and then try again. If
the problem persists, restart
your computer.

Windows Media Player cannot
play the DVD because a
compatible DVD decoder is not
installed on your computer.

Windows Media Player cannot
play the scene because it has a
parental rating higher than the
rating that you are authorized
to view.

Windows Media Player cannot
skip to the requested location
on the DVD.

137 / 497


Return value/code

0xC00D1170

NS_E_DVD_DEVICE_CONTENTION

0xC00D1171

NS_E_DVD_NO_VIDEO_MEMORY

0xC00D1172

NS_E_DVD_CANNOT_COPY_PROTECTED

0xC00D1173

NS_E_DVD_REQUIRED_PROPERTY_NOT_SET

0xC00D1174

NS_E_DVD_INVALID_TITLE_CHAPTER

0xC00D1176

NS_E_NO_CD_BURNER

0xC00D1177

NS_E_DEVICE_IS_NOT_READY

0xC00D1178

NS_E_PDA_UNSUPPORTED_FORMAT

0xC00D1179

NS_E_NO_PDA

0xC00D117A

NS_E_PDA_UNSPECIFIED_ERROR

0xC00D117B

NS_E_MEMSTORAGE_BAD_DATA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
play the DVD because it is
currently in use by another
program. Quit the other
program that is using the DVD,
and then try again.

Windows Media Player cannot
play DVD video. You might need
to adjust your Windows display
settings. Open display settings
in Control Panel, and then try
lowering your screen resolution
and color quality settings.

Windows Media Player cannot
rip the DVD because it is copy
protected.

One of more of the required
properties has not been set.

The specified title and/or
chapter number does not exist
on this DVD.

Windows Media Player cannot
burn the files because the
Player cannot find a burner. If
the burner is connected
properly, try using Windows
Update to install the latest
device driver.

Windows Media Player does not
detect storage media in the
selected device. Insert storage
media into the device, and then
try again.

Windows Media Player cannot
sync this file. The Player might
not support the file type.

Windows Media Player does not
detect a portable device.
Connect your portable device,
and then try again.

Windows Media Player
encountered an error while
communicating with the device.
The storage card on the device
might be full, the device might
be turned off, or the device
might not allow playlists or
folders to be created on it.

Windows Media Player
encountered an error while
burning a CD.

138 / 497


Return value/code

0xC00D117C

NS_E_PDA_FAIL_SELECT_DEVICE

0xC00D117D

NS_E_PDA_FAIL_READ_WAVE_FILE

0xC00D117E

NS_E_IMAPI_LOSSOFSTREAMING

0xC00D117F

NS_E_PDA_DEVICE_FULL

0xC00D1180

NS_E_FAIL_LAUNCH_ROXIO_PLUGIN

0xC00D1181

NS_E_PDA_DEVICE_FULL_IN_SESSION

0xC00D1182

NS_E_IMAPI_MEDIUM_INVALIDTYPE

0xC00D1183

NS_E_PDA_MANUALDEVICE

0xC00D1184

NS_E_PDA_PARTNERSHIPNOTEXIST

0xC00D1185

NS_E_PDA_CANNOT_CREATE_ADDITIONAL_SYNC_RELATIONSHIP

0xC00D1186

NS_E_PDA_NO_TRANSCODE_OF_DRM

0xC00D1187

NS_E_PDA_TRANSCODECACHEFULL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player
encountered an error while
communicating with a portable
device or CD drive.

Windows Media Player cannot
open the WAV file.

Windows Media Player failed to
burn all the files to the CD.
Select a slower recording speed,
and then try again.

There is not enough storage
space on the portable device to
complete this operation. Delete
some unneeded files on the
portable device, and then try
again.

Windows Media Player cannot
burn the files. Verify that your
burner is connected properly,
and then try again. If the
problem persists, reinstall the
Player.

Windows Media Player did not
sync some files to the device
because there is not enough
storage space on the device.

The disc in the burner is not
valid. Insert a blank disc into
the burner, and then try again.

Windows Media Player cannot
perform the requested action
because the device does not
support sync.

To perform the requested
action, you must first set up
sync with the device.

You have already created sync
partnerships with 16 devices.
To create a new sync
partnership, you must first end
an existing partnership.

Windows Media Player cannot
sync the file because protected
files cannot be converted to the
required quality level or file
format.

The folder that stores converted
files is full. Either empty the
folder or increase its size, and
then try again.

139 / 497


Return value/code

0xC00D1188

NS_E_PDA_TOO_MANY_FILE_COLLISIONS

0xC00D1189

NS_E_PDA_CANNOT_TRANSCODE

0xC00D118A

NS_E_PDA_TOO_MANY_FILES_IN_DIRECTORY

0xC00D118B

NS_E_PROCESSINGSHOWSYNCWIZARD

0xC00D118C

NS_E_PDA_TRANSCODE_NOT_PERMITTED

0xC00D118D

NS_E_PDA_INITIALIZINGDEVICES

0xC00D118E

NS_E_PDA_OBSOLETE_SP

0xC00D118F

NS_E_PDA_TITLE_COLLISION

0xC00D1190

NS_E_PDA_DEVICESUPPORTDISABLED

0xC00D1191

NS_E_PDA_NO_LONGER_AVAILABLE

0xC00D1192

NS_E_PDA_ENCODER_NOT_RESPONDING

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There are too many files with
the same name in the folder on
the device. Change the file
name or sync to a different
folder.

Windows Media Player cannot
convert the file to the format
required by the device.

You have reached the maximum
number of files your device
allows in a folder. If your device
supports playback from
subfolders, try creating
subfolders on the device and
storing some files in them.

Windows Media Player is
already trying to start the
Device Setup Wizard.

Windows Media Player cannot
convert this file format. If an
updated version of the codec
used to compress this file is
available, install it and then try
to sync the file again.

Windows Media Player is busy
setting up devices. Try again
later.

Your device is using an
outdated driver that is no
longer supported by Windows
Media Player. For additional
assistance, click Web Help.

Windows Media Player cannot
sync the file because a file with
the same name already exists
on the device. Change the file
name or try to sync the file to a
different folder.

Automatic and manual sync
have been turned off
temporarily. To sync to a
device, restart Windows Media
Player.

This device is not available.
Connect the device to the
computer, and then try again.

Windows Media Player cannot
sync the file because an error
occurred while converting the
file to another quality level or
format. If the problem persists,
remove the file from the list of

140 / 497


Return value/code

0xC00D1193

NS_E_PDA_CANNOT_SYNC_FROM_LOCATION

0xC00D1194

NS_E_WMP_PROTOCOL_PROBLEM

0xC00D1195

NS_E_WMP_NO_DISK_SPACE

0xC00D1196

NS_E_WMP_LOGON_FAILURE

0xC00D1197

NS_E_WMP_CANNOT_FIND_FILE

0xC00D1198

NS_E_WMP_SERVER_INACCESSIBLE

0xC00D1199

NS_E_WMP_UNSUPPORTED_FORMAT

0xC00D119A

NS_E_WMP_DSHOW_UNSUPPORTED_FORMAT

0xC00D119B

NS_E_WMP_PLAYLIST_EXISTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

files to sync.

Windows Media Player cannot
sync the file to your device. The
file might be stored in a location
that is not supported. Copy the
file from its current location to
your hard disk, add it to your
library, and then try to sync the
file again.

Windows Media Player cannot
open the specified URL. Verify
that the Player is configured to
use all available protocols, and
then try again.

Windows Media Player cannot
perform the requested action
because there is not enough
storage space on your
computer. Delete some
unneeded files on your hard
disk, and then try again.

The server denied access to the
file. Verify that you are using
the correct user name and
password.

Windows Media Player cannot
find the file. If you are trying to
play, burn, or sync an item that
is in your library, the item
might point to a file that has
been moved, renamed, or
deleted.

Windows Media Player cannot
connect to the server. The
server name might not be
correct, the server might not be
available, or your proxy settings
might not be correct.

Windows Media Player cannot
play the file. The Player might
not support the file type or
might not support the codec
that was used to compress the
file.

Windows Media Player cannot
play the file. The Player might
not support the file type or a
required codec might not be
installed on your computer.

Windows Media Player cannot
create the playlist because the
name already exists. Type a
different playlist name.

141 / 497


Return value/code

0xC00D119C

NS_E_WMP_NONMEDIA_FILES

0xC00D119D

NS_E_WMP_INVALID_ASX

0xC00D119E

NS_E_WMP_ALREADY_IN_USE

0xC00D119F

NS_E_WMP_IMAPI_FAILURE

0xC00D11A0

NS_E_WMP_WMDM_FAILURE

0xC00D11A1

NS_E_WMP_CODEC_NEEDED_WITH_4CC

0xC00D11A2

NS_E_WMP_CODEC_NEEDED_WITH_FORMATTAG

0xC00D11A3

NS_E_WMP_MSSAP_NOT_AVAILABLE

0xC00D11A4

NS_E_WMP_WMDM_INTERFACEDEAD

0xC00D11A5

NS_E_WMP_WMDM_NOTCERTIFIED

0xC00D11A6

NS_E_WMP_WMDM_LICENSE_NOTEXIST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
delete the playlist because it
contains items that are not
digital media files. Any digital
media files in the playlist were
deleted.

The playlist cannot be opened
because it is stored in a shared
folder on another computer. If
possible, move the playlist to
the playlists folder on your
computer.

Windows Media Player is
already in use. Stop playing any
items, close all Player dialog
boxes, and then try again.

Windows Media Player
encountered an error while
burning. Verify that the burner
is connected properly and that
the disc is clean and not
damaged.

Windows Media Player has
encountered an unknown error
with your portable device.
Reconnect your portable device,
and then try again.

A codec is required to play this
file. To determine if this codec
is available to download from
the web, click Web Help.

An audio codec is needed to
play this file. To determine if
this codec is available to
download from the web, click
Web Help.

To play the file, you must install
the latest Windows service
pack. To install the service pack
from the Windows Update
website, click Web Help.

Windows Media Player no longer
detects a portable device.
Reconnect your portable device,
and then try again.

Windows Media Player cannot
sync the file because the
portable device does not
support protected files.

This file does not have sync
rights. If you obtained this file
from an online store, go to the

142 / 497


Return value/code

0xC00D11A7

NS_E_WMP_WMDM_LICENSE_EXPIRED

0xC00D11A8

NS_E_WMP_WMDM_BUSY

0xC00D11A9

NS_E_WMP_WMDM_NORIGHTS

0xC00D11AA

NS_E_WMP_WMDM_INCORRECT_RIGHTS

0xC00D11AB

NS_E_WMP_IMAPI_GENERIC

0xC00D11AD

NS_E_WMP_IMAPI_DEVICE_NOTPRESENT

0xC00D11AE

NS_E_WMP_IMAPI_DEVICE_BUSY

0xC00D11AF

NS_E_WMP_IMAPI_LOSS_OF_STREAMING

0xC00D11B0

NS_E_WMP_SERVER_UNAVAILABLE

0xC00D11B1

NS_E_WMP_FILE_OPEN_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

online store to get sync rights.

Windows Media Player cannot
sync the file because the sync
rights have expired. Go to the
content provider's online store
to get new sync rights.

The portable device is already in
use. Wait until the current task
finishes or quit other programs
that might be using the portable
device, and then try again.

Windows Media Player cannot
sync the file because the
content provider or device
prohibits it. You might be able
to resolve this problem by going
to the content provider's online
store to get sync rights.

The content provider has not
granted you the right to sync
this file. Go to the content
provider's online store to get
sync rights.

Windows Media Player cannot
burn the files to the CD. Verify
that the disc is clean and not
damaged. If necessary, select a
slower recording speed or try a
different brand of blank discs.

Windows Media Player cannot
burn the files. Verify that the
burner is connected properly,
and then try again.

Windows Media Player cannot
burn the files. Verify that the
burner is connected properly
and that the disc is clean and
not damaged. If the burner is
already in use, wait until the
current task finishes or quit
other programs that might be
using the burner.

Windows Media Player cannot
burn the files to the CD.

Windows Media Player cannot
play the file. The server might
not be available or there might
be a problem with your network
or firewall settings.

Windows Media Player
encountered a problem while
playing the file. For additional

143 / 497


Return value/code

0xC00D11B2

NS_E_WMP_VERIFY_ONLINE

0xC00D11B3

NS_E_WMP_SERVER_NOT_RESPONDING

0xC00D11B4

NS_E_WMP_DRM_CORRUPT_BACKUP

0xC00D11B5

NS_E_WMP_DRM_LICENSE_SERVER_UNAVAILABLE

0xC00D11B6

NS_E_WMP_NETWORK_FIREWALL

0xC00D11B7

NS_E_WMP_NO_REMOVABLE_MEDIA

0xC00D11B8

NS_E_WMP_PROXY_CONNECT_TIMEOUT

0xC00D11B9

NS_E_WMP_NEED_UPGRADE

0xC00D11BA

NS_E_WMP_AUDIO_HW_PROBLEM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

assistance, click Web Help.

Windows Media Player must
connect to the Internet to verify
the file's media usage rights.
Connect to the Internet, and
then try again.

Windows Media Player cannot
play the file because a network
error occurred. The server
might not be available. Verify
that you are connected to the
network and that your proxy
settings are correct.

Windows Media Player cannot
restore your media usage rights
because it could not find any
backed up rights on your
computer.

Windows Media Player cannot
download media usage rights
because the server is not
available (for example, the
server might be busy or not
online).

Windows Media Player cannot
play the file. A network firewall
might be preventing the Player
from opening the file by using
the UDP transport protocol. If
you typed a URL in the Open
URL dialog box, try using a
different transport protocol (for
example, "http:").

Insert the removable media,
and then try again.

Windows Media Player cannot
play the file because the proxy
server is not responding. The
proxy server might be
temporarily unavailable or your
Player proxy settings might not
be valid.

To play the file, you might need
to install a later version of
Windows Media Player. On the
Help menu, click Check for
Updates, and then follow the
instructions. For additional
assistance, click Web Help.

Windows Media Player cannot
play the file because there is a
problem with your sound
device. There might not be a
sound device installed on your

144 / 497


Return value/code

0xC00D11BB

NS_E_WMP_INVALID_PROTOCOL

0xC00D11BC

NS_E_WMP_INVALID_LIBRARY_ADD

0xC00D11BD

NS_E_WMP_MMS_NOT_SUPPORTED

0xC00D11BE

NS_E_WMP_NO_PROTOCOLS_SELECTED

0xC00D11BF

NS_E_WMP_GOFULLSCREEN_FAILED

0xC00D11C0

NS_E_WMP_NETWORK_ERROR

0xC00D11C1

NS_E_WMP_CONNECT_TIMEOUT

0xC00D11C2

NS_E_WMP_MULTICAST_DISABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

computer, it might be in use by
another program, or it might
not be functioning properly.

Windows Media Player cannot
play the file because the
specified protocol is not
supported. If you typed a URL
in the Open URL dialog box, try
using a different transport
protocol (for example, "http:"
or "rtsp:").

Windows Media Player cannot
add the file to the library
because the file format is not
supported.

Windows Media Player cannot
play the file because the
specified protocol is not
supported. If you typed a URL
in the Open URL dialog box, try
using a different transport
protocol (for example, "mms:").

Windows Media Player cannot
play the file because there are
no streaming protocols
selected. Select one or more
protocols, and then try again.

Windows Media Player cannot
switch to Full Screen. You might
need to adjust your Windows
display settings. Open display
settings in Control Panel, and
then try setting Hardware
acceleration to Full.

Windows Media Player cannot
play the file because a network
error occurred. The server
might not be available (for
example, the server is busy or
not online) or you might not be
connected to the network.

Windows Media Player cannot
play the file because the server
is not responding. Verify that
you are connected to the
network, and then try again
later.

Windows Media Player cannot
play the file because the
multicast protocol is not
enabled. On the Tools menu,
click Options, click the Network
tab, and then select the
Multicast check box. For
additional assistance, click Web

145 / 497


Return value/code

0xC00D11C3

NS_E_WMP_SERVER_DNS_TIMEOUT

0xC00D11C4

NS_E_WMP_PROXY_NOT_FOUND

0xC00D11C5

NS_E_WMP_TAMPERED_CONTENT

0xC00D11C6

NS_E_WMP_OUTOFMEMORY

0xC00D11C7

NS_E_WMP_AUDIO_CODEC_NOT_INSTALLED

0xC00D11C8

NS_E_WMP_VIDEO_CODEC_NOT_INSTALLED

0xC00D11C9

NS_E_WMP_IMAPI_DEVICE_INVALIDTYPE

0xC00D11CA

NS_E_WMP_DRM_DRIVER_AUTH_FAILURE

0xC00D11CB

NS_E_WMP_NETWORK_RESOURCE_FAILURE

0xC00D11CC

NS_E_WMP_UPGRADE_APPLICATION

0xC00D11CD

NS_E_WMP_UNKNOWN_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Help.

Windows Media Player cannot
play the file because a network
problem occurred. Verify that
you are connected to the
network, and then try again
later.

Windows Media Player cannot
play the file because the
network proxy server cannot be
found. Verify that your proxy
settings are correct, and then
try again.

Windows Media Player cannot
play the file because it is
corrupted.

Your computer is running low
on memory. Quit other
programs, and then try again.

Windows Media Player cannot
play, burn, rip, or sync the file
because a required audio codec
is not installed on your
computer.

Windows Media Player cannot
play the file because the
required video codec is not
installed on your computer.

Windows Media Player cannot
burn the files. If the burner is
busy, wait for the current task
to finish. If necessary, verify
that the burner is connected
properly and that you have
installed the latest device
driver.

Windows Media Player cannot
play the protected file because
there is a problem with your
sound device. Try installing a
new device driver or use a
different sound device.

Windows Media Player
encountered a network error.
Restart the Player.

Windows Media Player is not
installed properly. Reinstall the
Player.

Windows Media Player
encountered an unknown error.
For additional assistance, click
Web Help.

146 / 497


Return value/code

0xC00D11CE

NS_E_WMP_INVALID_KEY

0xC00D11CF

NS_E_WMP_CD_ANOTHER_USER

0xC00D11D0

NS_E_WMP_DRM_NEEDS_AUTHORIZATION

0xC00D11D1

NS_E_WMP_BAD_DRIVER

0xC00D11D2

NS_E_WMP_ACCESS_DENIED

0xC00D11D3

NS_E_WMP_LICENSE_RESTRICTS

0xC00D11D4

NS_E_WMP_INVALID_REQUEST

0xC00D11D5

NS_E_WMP_CD_STASH_NO_SPACE

0xC00D11D6

NS_E_WMP_DRM_NEW_HARDWARE

0xC00D11D7

NS_E_WMP_DRM_INVALID_SIG

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player cannot
play the file because the
required codec is not valid.

The CD drive is in use by
another user. Wait for the task
to complete, and then try again.

Windows Media Player cannot
play, sync, or burn the
protected file because a
problem occurred with the
Windows Media Digital Rights
Management (DRM) system.
You might need to connect to
the Internet to update your
DRM components. For additional
assistance, click Web Help.

Windows Media Player cannot
play the file because there
might be a problem with your
sound or video device. Try
installing an updated device
driver.

Windows Media Player cannot
access the file. The file might be
in use, you might not have
access to the computer where
the file is stored, or your proxy
settings might not be correct.

The content provider prohibits
this action. Go to the content
provider's online store to get
new media usage rights.

Windows Media Player cannot
perform the requested action at
this time.

Windows Media Player cannot
burn the files because there is
not enough free disk space to
store the temporary files.
Delete some unneeded files on
your hard disk, and then try
again.

Your media usage rights have
become corrupted or are no
longer valid. This might happen
if you have replaced hardware
components in your computer.

The required Windows Media
Digital Rights Management
(DRM) component cannot be
validated. You might be able
resolve the problem by
reinstalling the Player.

147 / 497


Return value/code

0xC00D11D8

NS_E_WMP_DRM_CANNOT_RESTORE

0xC00D11D9

NS_E_WMP_BURN_DISC_OVERFLOW

0xC00D11DA

NS_E_WMP_DRM_GENERIC_LICENSE_FAILURE

0xC00D11DB

NS_E_WMP_DRM_NO_SECURE_CLOCK

0xC00D11DC

NS_E_WMP_DRM_NO_RIGHTS

0xC00D11DD

NS_E_WMP_DRM_INDIV_FAILED

0xC00D11DE

NS_E_WMP_SERVER_NONEWCONNECTIONS

0xC00D11DF

NS_E_WMP_MULTIPLE_ERROR_IN_PLAYLIST

0xC00D11E0

NS_E_WMP_IMAPI2_ERASE_FAIL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

You have exceeded your restore
limit for the day. Try restoring
your media usage rights
tomorrow.

Some files might not fit on the
CD. The required space cannot
be calculated accurately
because some files might be
missing duration information.
To ensure the calculation is
accurate, play the files that are
missing duration information.

Windows Media Player cannot
verify the file's media usage
rights. If you obtained this file
from an online store, go to the
online store to get the
necessary rights.

It is not possible to sync
because this device's internal
clock is not set correctly. To set
the clock, select the option to
set the device clock on the
Privacy tab of the Options
dialog box, connect to the
Internet, and then sync the
device again. For additional
assistance, click Web Help.

Windows Media Player cannot
play, burn, rip, or sync the
protected file because you do
not have the appropriate rights.

Windows Media Player
encountered an error during
upgrade.

Windows Media Player cannot
connect to the server because it
is not accepting any new
connections. This could be
because it has reached its
maximum connection limit.
Please try again later.

A number of queued files
cannot be played. To find
information about the problem,
click the Now Playing tab, and
then click the icon next to each
file in the List pane.

Windows Media Player
encountered an error while
erasing the rewritable CD or
DVD. Verify that the CD or DVD
burner is connected properly
and that the disc is clean and

148 / 497


Return value/code

0xC00D11E1

NS_E_WMP_IMAPI2_ERASE_DEVICE_BUSY

0xC00D11E2

NS_E_WMP_DRM_COMPONENT_FAILURE

0xC00D11E3

NS_E_WMP_DRM_NO_DEVICE_CERT

0xC00D11E4

NS_E_WMP_SERVER_SECURITY_ERROR

0xC00D11E5

NS_E_WMP_AUDIO_DEVICE_LOST

0xC00D11E6

NS_E_WMP_IMAPI_MEDIA_INCOMPATIBLE

0xC00D11EE

NS_E_SYNCWIZ_DEVICE_FULL

0xC00D11EF

NS_E_SYNCWIZ_CANNOT_CHANGE_SETTINGS

0xC00D11F0

NS_E_TRANSCODE_DELETECACHEERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

not damaged.

Windows Media Player cannot
erase the rewritable CD or DVD.
Verify that the CD or DVD
burner is connected properly
and that the disc is clean and
not damaged. If the burner is
already in use, wait until the
current task finishes or quit
other programs that might be
using the burner.

A Windows Media Digital Rights
Management (DRM) component
encountered a problem. If you
are trying to use a file that you
obtained from an online store,
try going to the online store and
getting the appropriate usage
rights.

It is not possible to obtain
device's certificate. Please
contact the device manufacturer
for a firmware update or for
other steps to resolve this
problem.

Windows Media Player
encountered an error when
connecting to the server. The
security information from the
server could not be validated.

An audio device was
disconnected or reconfigured.
Verify that the audio device is
connected, and then try to play
the item again.

Windows Media Player could not
complete burning because the
disc is not compatible with your
drive. Try inserting a different
kind of recordable media or use
a disc that supports a write
speed that is compatible with
your drive.

Windows Media Player cannot
save the sync settings because
your device is full. Delete some
unneeded files on your device
and then try again.

It is not possible to change sync
settings at this time. Try again
later.

Windows Media Player cannot
delete these files currently. If
the Player is synchronizing, wait

149 / 497


Return value/code

0xC00D11F8

NS_E_CD_NO_BUFFERS_READ

0xC00D11F9

NS_E_CD_EMPTY_TRACK_QUEUE

0xC00D11FA

NS_E_CD_NO_READER

0xC00D11FB

NS_E_CD_ISRC_INVALID

0xC00D11FC

NS_E_CD_MEDIA_CATALOG_NUMBER_INVALID

0xC00D11FD

NS_E_SLOW_READ_DIGITAL_WITH_ERRORCORRECTION

0xC00D11FE

NS_E_CD_SPEEDDETECT_NOT_ENOUGH_READS

0xC00D11FF

NS_E_CD_QUEUEING_DISABLED

0xC00D1202

NS_E_WMP_DRM_ACQUIRING_LICENSE

0xC00D1203

NS_E_WMP_DRM_LICENSE_EXPIRED

0xC00D1204

NS_E_WMP_DRM_LICENSE_NOTACQUIRED

0xC00D1205

NS_E_WMP_DRM_LICENSE_NOTENABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

until it is complete and then try
again.

Windows Media Player could not
use digital mode to read the
CD. The Player has
automatically switched the CD
drive to analog mode. To switch
back to digital mode, use the
Devices tab. For additional
assistance, click Web Help.

No CD track was specified for
playback.

The CD filter was not able to
create the CD reader.

Invalid ISRC code.

Invalid Media Catalog Number.

Windows Media Player cannot
play audio CDs correctly
because the CD drive is slow
and error correction is turned
on. To increase performance,
turn off playback error
correction for this drive.

Windows Media Player cannot
estimate the CD drive's
playback speed because the CD
track is too short.

Cannot queue the CD track
because queuing is not enabled.

Windows Media Player cannot
download additional media
usage rights until the current
download is complete.

The media usage rights for this
file have expired or are no
longer valid. If you obtained the
file from an online store, sign in
to the store, and then try again.

Windows Media Player cannot
download the media usage
rights for the file. If you
obtained the file from an online
store, sign in to the store, and
then try again.

The media usage rights for this
file are not yet valid. To see
when they will become valid,
right-click the file in the library,

150 / 497


Return value/code

0xC00D1206

NS_E_WMP_DRM_LICENSE_UNUSABLE

0xC00D1207

NS_E_WMP_DRM_LICENSE_CONTENT_REVOKED

0xC00D1208

NS_E_WMP_DRM_LICENSE_NOSAP

0xC00D1209

NS_E_WMP_DRM_UNABLE_TO_ACQUIRE_LICENSE

0xC00D120A

NS_E_WMP_LICENSE_REQUIRED

0xC00D120B

NS_E_WMP_PROTECTED_CONTENT

0xC00D122A

NS_E_WMP_POLICY_VALUE_NOT_CONFIGURED

0xC00D1234

NS_E_PDA_CANNOT_SYNC_FROM_INTERNET

0xC00D1235

NS_E_PDA_CANNOT_SYNC_INVALID_PLAYLIST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

click Properties, and then click
the Media Usage Rights tab.

The media usage rights for this
file are not valid. If you
obtained this file from an online
store, contact the store for
assistance.

The content provider has
revoked the media usage rights
for this file. If you obtained this
file from an online store, ask
the store if a new version of the
file is available.

The media usage rights for this
file require a feature that is not
supported in your current
version of Windows Media
Player or your current version
of Windows. Try installing the
latest version of the Player. If
you obtained this file from an
online store, contact the store
for further assistance.

Windows Media Player cannot
download media usage rights at
this time. Try again later.

Windows Media Player cannot
play, burn, or sync the file
because the media usage rights
are missing. If you obtained the
file from an online store, sign in
to the store, and then try again.

Windows Media Player cannot
play, burn, or sync the file
because the media usage rights
are missing. If you obtained the
file from an online store, sign in
to the store, and then try again.

Windows Media Player cannot
read a policy. This can occur
when the policy does not exist
in the registry or when the
registry cannot be read.

Windows Media Player cannot
sync content streamed directly
from the Internet. If possible,
download the file to your
computer, and then try to sync
the file.

This playlist is not valid or is
corrupted. Create a new playlist
using Windows Media Player,
then sync the new playlist
instead.

151 / 497


Return value/code

0xC00D1236

NS_E_PDA_FAILED_TO_SYNCHRONIZE_FILE

0xC00D1237

NS_E_PDA_SYNC_FAILED

0xC00D1238

NS_E_PDA_DELETE_FAILED

0xC00D1239

NS_E_PDA_FAILED_TO_RETRIEVE_FILE

0xC00D123A

NS_E_PDA_DEVICE_NOT_RESPONDING

0xC00D123B

NS_E_PDA_FAILED_TO_TRANSCODE_PHOTO

0xC00D123C

NS_E_PDA_FAILED_TO_ENCRYPT_TRANSCODED_FILE

0xC00D123D

NS_E_PDA_CANNOT_TRANSCODE_TO_AUDIO

0xC00D123E

NS_E_PDA_CANNOT_TRANSCODE_TO_VIDEO

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows Media Player
encountered a problem while
synchronizing the file to the
device. For additional
assistance, click Web Help.

Windows Media Player
encountered an error while
synchronizing to the device.

Windows Media Player cannot
delete a file from the device.

Windows Media Player cannot
copy a file from the device to
your library.

Windows Media Player cannot
communicate with the device
because the device is not
responding. Try reconnecting
the device, resetting the device,
or contacting the device
manufacturer for updated
firmware.

Windows Media Player cannot
sync the picture to the device
because a problem occurred
while converting the file to
another quality level or format.
The original file might be
damaged or corrupted.

Windows Media Player cannot
convert the file. The file might
have been encrypted by the
Encrypted File System (EFS).
Try decrypting the file first and
then synchronizing it. For
information about how to
decrypt a file, see Windows
Help and Support.

Your device requires that this
file be converted in order to
play on the device. However,
the device either does not
support playing audio, or
Windows Media Player cannot
convert the file to an audio
format that is supported by the
device.

Your device requires that this
file be converted in order to
play on the device. However,
the device either does not
support playing video, or
Windows Media Player cannot
convert the file to a video
format that is supported by the

152 / 497


Return value/code

0xC00D123F

NS_E_PDA_CANNOT_TRANSCODE_TO_IMAGE

0xC00D1240

NS_E_PDA_RETRIEVED_FILE_FILENAME_TOO_LONG

0xC00D1241

NS_E_PDA_CEWMDM_DRM_ERROR

0xC00D1242

NS_E_INCOMPLETE_PLAYLIST

0xC00D1243

NS_E_PDA_SYNC_RUNNING

0xC00D1244

NS_E_PDA_SYNC_LOGIN_ERROR

0xC00D1245

NS_E_PDA_TRANSCODE_CODEC_NOT_FOUND

0xC00D1246

NS_E_CANNOT_SYNC_DRM_TO_NON_JANUS_DEVICE

0xC00D1247

NS_E_CANNOT_SYNC_PREVIOUS_SYNC_RUNNING

0xC00D125C

NS_E_WMP_HWND_NOTFOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

device.

Your device requires that this
file be converted in order to
play on the device. However,
the device either does not
support displaying pictures, or
Windows Media Player cannot
convert the file to a picture
format that is supported by the
device.

Windows Media Player cannot
sync the file to your computer
because the file name is too
long. Try renaming the file on
the device.

Windows Media Player cannot
sync the file because the device
is not responding. This typically
occurs when there is a problem
with the device firmware. For
additional assistance, click Web
Help.

Incomplete playlist.

It is not possible to perform the
requested action because sync
is in progress. You can either
stop sync or wait for it to
complete, and then try again.

Windows Media Player cannot
sync the subscription content
because you are not signed in
to the online store that provided
it. Sign in to the online store,
and then try again.

Windows Media Player cannot
convert the file to the format
required by the device. One or
more codecs required to convert
the file could not be found.

It is not possible to sync
subscription files to this device.

Your device is operating slowly
or is not responding. Until the
device responds, it is not
possible to sync again. To
return the device to normal
operation, try disconnecting it
from the computer or resetting
it.

The Windows Media Player
download manager cannot

153 / 497


Return value/code

0xC00D125D

NS_E_BKGDOWNLOAD_WRONG_NO_FILES

0xC00D125E

NS_E_BKGDOWNLOAD_COMPLETECANCELLEDJOB

0xC00D125F

NS_E_BKGDOWNLOAD_CANCELCOMPLETEDJOB

0xC00D1260

NS_E_BKGDOWNLOAD_NOJOBPOINTER

0xC00D1261

NS_E_BKGDOWNLOAD_INVALIDJOBSIGNATURE

0xC00D1262

NS_E_BKGDOWNLOAD_FAILED_TO_CREATE_TEMPFILE

0xC00D1263

NS_E_BKGDOWNLOAD_PLUGIN_FAILEDINITIALIZE

0xC00D1264

NS_E_BKGDOWNLOAD_PLUGIN_FAILEDTOMOVEFILE

0xC00D1265

NS_E_BKGDOWNLOAD_CALLFUNCFAILED

0xC00D1266

NS_E_BKGDOWNLOAD_CALLFUNCTIMEOUT

0xC00D1267

NS_E_BKGDOWNLOAD_CALLFUNCENDED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

function properly because the
Player main window cannot be
found. Try restarting the Player.

Windows Media Player
encountered a download that
has the wrong number of files.
This might occur if another
program is trying to create jobs
with the same signature as the
Player.

Windows Media Player tried to
complete a download that was
already canceled. The file will
not be available.

Windows Media Player tried to
cancel a download that was
already completed. The file will
not be removed.

Windows Media Player is trying
to access a download that is not
valid.

This download was not created
by Windows Media Player.

The Windows Media Player
download manager cannot
create a temporary file name.
This might occur if the path is
not valid or if the disk is full.

The Windows Media Player
download manager plug-in
cannot start. This might occur if
the system is out of resources.

The Windows Media Player
download manager cannot
move the file.

The Windows Media Player
download manager cannot
perform a task because the
system has no resources to
allocate.

The Windows Media Player
download manager cannot
perform a task because the task
took too long to run.

The Windows Media Player
download manager cannot
perform a task because the
Player is terminating the
service. The task will be
recovered when the Player
restarts.

154 / 497


Return value/code

0xC00D1268

NS_E_BKGDOWNLOAD_WMDUNPACKFAILED

0xC00D1269

NS_E_BKGDOWNLOAD_FAILEDINITIALIZE

0xC00D126A

NS_E_INTERFACE_NOT_REGISTERED_IN_GIT

0xC00D126B

NS_E_BKGDOWNLOAD_INVALID_FILE_NAME

0xC00D128E

NS_E_IMAGE_DOWNLOAD_FAILED

0xC00D12C0

NS_E_WMP_UDRM_NOUSERLIST

0xC00D12C1

NS_E_WMP_DRM_NOT_ACQUIRING

0xC00D12F2

NS_E_WMP_BSTR_TOO_LONG

0xC00D12FC

NS_E_WMP_AUTOPLAY_INVALID_STATE

0xC00D1306

NS_E_WMP_COMPONENT_REVOKED

0xC00D1324

NS_E_CURL_NOTSAFE

0xC00D1325

NS_E_CURL_INVALIDCHAR

0xC00D1326

NS_E_CURL_INVALIDHOSTNAME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The Windows Media Player
download manager cannot
expand a WMD file. The file will
be deleted and the operation
will not be completed
successfully.

The Windows Media Player
download manager cannot
start. This might occur if the
system is out of resources.

Windows Media Player cannot
access a required functionality.
This might occur if the wrong
system files or Player DLLs are
loaded.

Windows Media Player cannot
get the file name of the
requested download. The
requested download will be
canceled.

Windows Media Player
encountered an error while
downloading an image.

Windows Media Player cannot
update your media usage rights
because the Player cannot
verify the list of activated users
of this computer.

Windows Media Player is trying
to acquire media usage rights
for a file that is no longer being
used. Rights acquisition will
stop.

The parameter is not valid.

The state is not valid for this
request.

Windows Media Player cannot
play this file until you complete
the software component
upgrade. After the component
has been upgraded, try to play
the file again.

The URL is not safe for the
operation specified.

The URL contains one or more
characters that are not valid.

The URL contains a host name
that is not valid.

155 / 497


Return value/code

0xC00D1327

NS_E_CURL_INVALIDPATH

0xC00D1328

NS_E_CURL_INVALIDSCHEME

0xC00D1329

NS_E_CURL_INVALIDURL

0xC00D132B

NS_E_CURL_CANTWALK

0xC00D132C

NS_E_CURL_INVALIDPORT

0xC00D132D

NS_E_CURLHELPER_NOTADIRECTORY

0xC00D132E

NS_E_CURLHELPER_NOTAFILE

0xC00D132F

NS_E_CURL_CANTDECODE

0xC00D1330

NS_E_CURLHELPER_NOTRELATIVE

0xC00D1331

NS_E_CURL_INVALIDBUFFERSIZE

0xC00D1356

NS_E_SUBSCRIPTIONSERVICE_PLAYBACK_DISALLOWED

0xC00D1357

NS_E_CANNOT_BUY_OR_DOWNLOAD_FROM_MULTIPLE_SERVICES

0xC00D1358

NS_E_CANNOT_BUY_OR_DOWNLOAD_CONTENT

0xC00D135A

NS_E_NOT_CONTENT_PARTNER_TRACK

0xC00D135B

NS_E_TRACK_DOWNLOAD_REQUIRES_ALBUM_PURCHASE

0xC00D135C

NS_E_TRACK_DOWNLOAD_REQUIRES_PURCHASE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The URL contains a path that is
not valid.

The URL contains a scheme that
is not valid.

The URL is not valid.

Windows Media Player cannot
play the file. If you clicked a
link on a web page, the link
might not be valid.

The URL port is not valid.

The URL is not a directory.

The URL is not a file.

The URL contains characters
that cannot be decoded. The
URL might be truncated or
incomplete.

The specified URL is not a
relative URL.

The buffer is smaller than the
size specified.

The content provider has not
granted you the right to play
this file. Go to the content
provider's online store to get
play rights.

Windows Media Player cannot
purchase or download content
from multiple online stores.

The file cannot be purchased or
downloaded. The file might not
be available from the online
store.

The provider of this file cannot
be identified.

The file is only available for
download when you buy the
entire album.

You must buy the file before
you can download it.

156 / 497


Return value/code

0xC00D135D

NS_E_TRACK_PURCHASE_MAXIMUM_EXCEEDED

0xC00D135F

NS_E_SUBSCRIPTIONSERVICE_LOGIN_FAILED

0xC00D1360

NS_E_SUBSCRIPTIONSERVICE_DOWNLOAD_TIMEOUT

0xC00D1362

NS_E_CONTENT_PARTNER_STILL_INITIALIZING

0xC00D1363

NS_E_OPEN_CONTAINING_FOLDER_FAILED

0xC00D136A

NS_E_ADVANCEDEDIT_TOO_MANY_PICTURES

0xC00D1388

NS_E_REDIRECT

0xC00D1389

NS_E_STALE_PRESENTATION

0xC00D138A

NS_E_NAMESPACE_WRONG_PERSIST

0xC00D138B

NS_E_NAMESPACE_WRONG_TYPE

0xC00D138C

NS_E_NAMESPACE_NODE_CONFLICT

0xC00D138D

NS_E_NAMESPACE_NODE_NOT_FOUND

0xC00D138E

NS_E_NAMESPACE_BUFFER_TOO_SMALL

0xC00D138F

NS_E_NAMESPACE_TOO_MANY_CALLBACKS

0xC00D1390

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

You have exceeded the
maximum number of files that
can be purchased in a single
transaction.

Windows Media Player cannot
sign in to the online store.
Verify that you are using the
correct user name and
password. If the problem
persists, the store might be
temporarily unavailable.

Windows Media Player cannot
download this item because the
server is not responding. The
server might be temporarily
unavailable or the Internet
connection might be lost.

Content Partner still initializing.

The folder could not be opened.
The folder might have been
moved or deleted.

Windows Media Player could not
add all of the images to the file
because the images exceeded
the 7 megabyte (MB) limit.

The client redirected to another
server.

The streaming media
description is no longer current.

It is not possible to create a
persistent namespace node
under a transient parent node.

It is not possible to store a
value in a namespace node that
has a different value type.

It is not possible to remove the
root namespace node.

The specified namespace node
could not be found.

The buffer supplied to hold
namespace node string is too
small.

The callback list on a
namespace node is at the
maximum size.

It is not possible to register an

157 / 497


Return value/code

NS_E_NAMESPACE_DUPLICATE_CALLBACK

0xC00D1391

NS_E_NAMESPACE_CALLBACK_NOT_FOUND

0xC00D1392

NS_E_NAMESPACE_NAME_TOO_LONG

0xC00D1393

NS_E_NAMESPACE_DUPLICATE_NAME

0xC00D1394

NS_E_NAMESPACE_EMPTY_NAME

0xC00D1395

NS_E_NAMESPACE_INDEX_TOO_LARGE

0xC00D1396

NS_E_NAMESPACE_BAD_NAME

0xC00D1397

NS_E_NAMESPACE_WRONG_SECURITY

0xC00D13EC

NS_E_CACHE_ARCHIVE_CONFLICT

0xC00D13ED

NS_E_CACHE_ORIGIN_SERVER_NOT_FOUND

0xC00D13EE

NS_E_CACHE_ORIGIN_SERVER_TIMEOUT

0xC00D13EF

NS_E_CACHE_NOT_BROADCAST

0xC00D13F0

NS_E_CACHE_CANNOT_BE_CACHED

0xC00D13F1

NS_E_CACHE_NOT_MODIFIED

0xC00D1450

NS_E_CANNOT_REMOVE_PUBLISHING_POINT

0xC00D1451

NS_E_CANNOT_REMOVE_PLUGIN

0xC00D1452

NS_E_WRONG_PUBLISHING_POINT_TYPE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

already-registered callback on a
namespace node.

Cannot find the callback in the
namespace when attempting to
remove the callback.

The namespace node name
exceeds the allowed maximum
length.

Cannot create a namespace
node that already exists.

The namespace node name
cannot be a null string.

Finding a child namespace node
by index failed because the
index exceeded the number of
children.

The namespace node name is
invalid.

It is not possible to store a
value in a namespace node that
has a different security type.

The archive request conflicts
with other requests in progress.

The specified origin server
cannot be found.

The specified origin server is
not responding.

The internal code for HTTP
status code 412 Precondition
Failed due to not broadcast
type.

The internal code for HTTP
status code 403 Forbidden due
to not cacheable.

The internal code for HTTP
status code 304 Not Modified.

It is not possible to remove a
cache or proxy publishing point.

It is not possible to remove the
last instance of a type of plug-
in.

Cache and proxy publishing
points do not support this
property or method.

158 / 497


Return value/code

0xC00D1453

NS_E_UNSUPPORTED_LOAD_TYPE

0xC00D1454

NS_E_INVALID_PLUGIN_LOAD_TYPE_CONFIGURATION

0xC00D1455

NS_E_INVALID_PUBLISHING_POINT_NAME

0xC00D1456

NS_E_TOO_MANY_MULTICAST_SINKS

0xC00D1457

NS_E_PUBLISHING_POINT_INVALID_REQUEST_WHILE_STARTED

0xC00D1458

NS_E_MULTICAST_PLUGIN_NOT_ENABLED

0xC00D1459

NS_E_INVALID_OPERATING_SYSTEM_VERSION

0xC00D145A

NS_E_PUBLISHING_POINT_REMOVED

0xC00D145B

NS_E_INVALID_PUSH_PUBLISHING_POINT_START_REQUEST

0xC00D145C

NS_E_UNSUPPORTED_LANGUAGE

0xC00D145D

NS_E_WRONG_OS_VERSION

0xC00D145E

NS_E_PUBLISHING_POINT_STOPPED

0xC00D14B4

NS_E_PLAYLIST_ENTRY_ALREADY_PLAYING

0xC00D14B5

NS_E_EMPTY_PLAYLIST

0xC00D14B6

NS_E_PLAYLIST_PARSE_FAILURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The plug-in does not support
the specified load type.

The plug-in does not support
any load types. The plug-in
must support at least one load
type.

The publishing point name is
invalid.

Only one multicast data writer
plug-in can be enabled for a
publishing point.

The requested operation cannot
be completed while the
publishing point is started.

A multicast data writer plug-in
must be enabled in order for
this operation to be completed.

This feature requires Windows
Server 2003, Enterprise Edition.

The requested operation cannot
be completed because the
specified publishing point has
been removed.

Push publishing points are
started when the encoder starts
pushing the stream. This
publishing point cannot be
started by the server
administrator.

The specified language is not
supported.

Windows Media Services will
only run on Windows Server
2003, Standard Edition and
Windows Server 2003,
Enterprise Edition.

The operation cannot be
completed because the
publishing point has been
stopped.

The playlist entry is already
playing.

The playlist or directory you are
requesting does not contain
content.

The server was unable to parse
the requested playlist file.

159 / 497


Return value/code

0xC00D14B7

NS_E_PLAYLIST_UNSUPPORTED_ENTRY

0xC00D14B8

NS_E_PLAYLIST_ENTRY_NOT_IN_PLAYLIST

0xC00D14B9

NS_E_PLAYLIST_ENTRY_SEEK

0xC00D14BA

NS_E_PLAYLIST_RECURSIVE_PLAYLISTS

0xC00D14BB

NS_E_PLAYLIST_TOO_MANY_NESTED_PLAYLISTS

0xC00D14BC

NS_E_PLAYLIST_SHUTDOWN

0xC00D14BD

NS_E_PLAYLIST_END_RECEDING

0xC00D1518

NS_E_DATAPATH_NO_SINK

0xC00D151A

NS_E_INVALID_PUSH_TEMPLATE

0xC00D151B

NS_E_INVALID_PUSH_PUBLISHING_POINT

0xC00D151C

NS_E_CRITICAL_ERROR

0xC00D151D

NS_E_NO_NEW_CONNECTIONS

0xC00D151E

NS_E_WSX_INVALID_VERSION

0xC00D151F

NS_E_HEADER_MISMATCH

0xC00D1520

NS_E_PUSH_DUPLICATE_PUBLISHING_POINT_NAME

0xC00D157C

NS_E_NO_SCRIPT_ENGINE

0xC00D157D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The requested operation is not
supported for this type of
playlist entry.

Cannot jump to a playlist entry
that is not inserted in the
playlist.

Cannot seek to the desired
playlist entry.

Cannot play recursive playlist.

The number of nested playlists
exceeded the limit the server
can handle.

Cannot execute the requested
operation because the playlist
has been shut down by the
Media Server.

The playlist has ended while
receding.

The data path does not have an
associated data writer plug-in.

The specified push template is
invalid.

The specified push publishing
point is invalid.

The requested operation cannot
be performed because the
server or publishing point is in a
critical error state.

The content cannot be played
because the server is not
currently accepting connections.
Try connecting at a later time.

The version of this playlist is not
supported by the server.

The command does not apply to
the current media header user
by a server component.

The specified publishing point
name is already in use.

There is no script engine
available for this file.

The plug-in has reported an
error. See the Troubleshooting

160 / 497


Return value/code

NS_E_PLUGIN_ERROR_REPORTED

0xC00D157E

NS_E_SOURCE_PLUGIN_NOT_FOUND

0xC00D157F

NS_E_PLAYLIST_PLUGIN_NOT_FOUND

0xC00D1580

NS_E_DATA_SOURCE_ENUMERATION_NOT_SUPPORTED

0xC00D1581

NS_E_MEDIA_PARSER_INVALID_FORMAT

0xC00D1582

NS_E_SCRIPT_DEBUGGER_NOT_INSTALLED

0xC00D1583

NS_E_FEATURE_REQUIRES_ENTERPRISE_SERVER

0xC00D1584

NS_E_WIZARD_RUNNING

0xC00D1585

NS_E_INVALID_LOG_URL

0xC00D1586

NS_E_INVALID_MTU_RANGE

0xC00D1587

NS_E_INVALID_PLAY_STATISTICS

0xC00D1588

NS_E_LOG_NEED_TO_BE_SKIPPED

0xC00D1589

NS_E_HTTP_TEXT_DATACONTAINER_SIZE_LIMIT_EXCEEDED

0xC00D158A

NS_E_PORT_IN_USE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

tab or the NT Application Event
Log for details.

No enabled data source plug-in
is available to access the
requested content.

No enabled playlist parser plug-
in is available to access the
requested content.

The data source plug-in does
not support enumeration.

The server cannot stream the
selected file because it is either
damaged or corrupt. Select a
different file.

The plug-in cannot be enabled
because a compatible script
debugger is not installed on this
system. Install a script
debugger, or disable the script
debugger option on the general
tab of the plug-in's properties
page and try again.

The plug-in cannot be loaded
because it requires Windows
Server 2003, Enterprise Edition.

Another wizard is currently
running. Please close the other
wizard or wait until it finishes
before attempting to run this
wizard again.

Invalid log URL. Multicast
logging URL must look like
"http://servername/isapibacken
d.dll".

Invalid MTU specified. The valid
range for maximum packet size
is between 36 and 65507 bytes.

Invalid play statistics for
logging.

The log needs to be skipped.

The size of the data exceeded
the limit the WMS HTTP
Download Data Source plugin
can handle.

One usage of each socket
address (protocol/network
address/port) is permitted.
Verify that other services or

161 / 497


Return value/code

0xC00D158B

NS_E_PORT_IN_USE_HTTP

0xC00D158C

NS_E_HTTP_TEXT_DATACONTAINER_INVALID_SERVER_RESPONSE

0xC00D158D

NS_E_ARCHIVE_REACH_QUOTA

0xC00D158E

NS_E_ARCHIVE_ABORT_DUE_TO_BCAST

0xC00D158F

NS_E_ARCHIVE_GAP_DETECTED

0xC00D1590

NS_E_AUTHORIZATION_FILE_NOT_FOUND

0xC00D1B58

NS_E_BAD_MARKIN

0xC00D1B59

NS_E_BAD_MARKOUT

0xC00D1B5A

NS_E_NOMATCHING_MEDIASOURCE

0xC00D1B5B

NS_E_UNSUPPORTED_SOURCETYPE

0xC00D1B5C

NS_E_TOO_MANY_AUDIO

0xC00D1B5D

NS_E_TOO_MANY_VIDEO

0xC00D1B5E

NS_E_NOMATCHING_ELEMENT

0xC00D1B5F

NS_E_MISMATCHED_MEDIACONTENT

0xC00D1B60

NS_E_CANNOT_DELETE_ACTIVE_SOURCEGROUP

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

applications are not attempting
to use the same port and then
try to enable the plug-in again.

One usage of each socket
address (protocol/network
address/port) is permitted.
Verify that other services (such
as IIS) or applications are not
attempting to use the same
port and then try to enable the
plug-in again.

The WMS HTTP Download Data
Source plugin was unable to
receive the remote server's
response.

The archive plug-in has reached
its quota.

The archive plug-in aborted
because the source was from
broadcast.

The archive plug-in detected an
interrupt in the source.

The system cannot find the file
specified.

The mark-in time should be
greater than 0 and less than the
mark-out time.

The mark-out time should be
greater than the mark-in time
and less than the file duration.

No matching media type is
found in the source %1.

The specified source type is not
supported.

It is not possible to specify
more than one audio input.

It is not possible to specify
more than two video inputs.

No matching element is found
in the list.

The profile's media types must
match the media types defined
for the session.

It is not possible to remove an
active source while encoding.

162 / 497


Return value/code

0xC00D1B61

NS_E_AUDIODEVICE_BUSY

0xC00D1B62

NS_E_AUDIODEVICE_UNEXPECTED

0xC00D1B63

NS_E_AUDIODEVICE_BADFORMAT

0xC00D1B64

NS_E_VIDEODEVICE_BUSY

0xC00D1B65

NS_E_VIDEODEVICE_UNEXPECTED

0xC00D1B66

NS_E_INVALIDCALL_WHILE_ENCODER_RUNNING

0xC00D1B67

NS_E_NO_PROFILE_IN_SOURCEGROUP

0xC00D1B68

NS_E_VIDEODRIVER_UNSTABLE

0xC00D1B69

NS_E_VIDCAPSTARTFAILED

0xC00D1B6A

NS_E_VIDSOURCECOMPRESSION

0xC00D1B6B

NS_E_VIDSOURCESIZE

0xC00D1B6C

NS_E_ICMQUERYFORMAT

0xC00D1B6D

NS_E_VIDCAPCREATEWINDOW

0xC00D1B6E

NS_E_VIDCAPDRVINUSE

0xC00D1B6F

NS_E_NO_MEDIAFORMAT_IN_SOURCE

0xC00D1B70

NS_E_NO_VALID_OUTPUT_STREAM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

It is not possible to open the
specified audio capture device
because it is currently in use.

It is not possible to open the
specified audio capture device
because an unexpected error
has occurred.

The audio capture device does
not support the specified audio
format.

It is not possible to open the
specified video capture device
because it is currently in use.

It is not possible to open the
specified video capture device
because an unexpected error
has occurred.

This operation is not allowed
while encoding.

No profile is set for the source.

The video capture driver
returned an unrecoverable
error. It is now in an unstable
state.

It was not possible to start the
video device.

The video source does not
support the requested output
format or color depth.

The video source does not
support the requested capture
size.

It was not possible to obtain
output information from the
video compressor.

It was not possible to create a
video capture window.

There is already a stream active
on this video device.

No media format is set in
source.

Cannot find a valid output
stream from the source.

163 / 497


Return value/code

0xC00D1B71

NS_E_NO_VALID_SOURCE_PLUGIN

0xC00D1B72

NS_E_NO_ACTIVE_SOURCEGROUP

0xC00D1B73

NS_E_NO_SCRIPT_STREAM

0xC00D1B74

NS_E_INVALIDCALL_WHILE_ARCHIVAL_RUNNING

0xC00D1B75

NS_E_INVALIDPACKETSIZE

0xC00D1B76

NS_E_PLUGIN_CLSID_INVALID

0xC00D1B77

NS_E_UNSUPPORTED_ARCHIVETYPE

0xC00D1B78

NS_E_UNSUPPORTED_ARCHIVEOPERATION

0xC00D1B79

NS_E_ARCHIVE_FILENAME_NOTSET

0xC00D1B7A

NS_E_SOURCEGROUP_NOTPREPARED

0xC00D1B7B

NS_E_PROFILE_MISMATCH

0xC00D1B7C

NS_E_INCORRECTCLIPSETTINGS

0xC00D1B7D

NS_E_NOSTATSAVAILABLE

0xC00D1B7E

NS_E_NOTARCHIVING

0xC00D1B7F

NS_E_INVALIDCALL_WHILE_ENCODER_STOPPED

0xC00D1B80

NS_E_NOSOURCEGROUPS

0xC00D1B81

NS_E_INVALIDINPUTFPS

0xC00D1B82

NS_E_NO_DATAVIEW_SUPPORT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

It was not possible to find a
valid source plug-in for the
specified source.

No source is currently active.

No script stream is set in the
current source.

This operation is not allowed
while archiving.

The setting for the maximum
packet size is not valid.

The plug-in CLSID specified is
not valid.

This archive type is not
supported.

This archive operation is not
supported.

The local archive file name was
not set.

The source is not yet prepared.

Profiles on the sources do not
match.

The specified crop values are
not valid.

No statistics are available at
this time.

The encoder is not archiving.

This operation is only allowed
during encoding.

This SourceGroupCollection
doesn't contain any
SourceGroups.

This source does not have a
frame rate of 30 fps. Therefore,
it is not possible to apply the
inverse telecine filter to the
source.

It is not possible to display your
source or output video in the

164 / 497


Return value/code

0xC00D1B83

NS_E_CODEC_UNAVAILABLE

0xC00D1B84

NS_E_ARCHIVE_SAME_AS_INPUT

0xC00D1B85

NS_E_SOURCE_NOTSPECIFIED

0xC00D1B86

NS_E_NO_REALTIME_TIMECOMPRESSION

0xC00D1B87

NS_E_UNSUPPORTED_ENCODER_DEVICE

0xC00D1B88

NS_E_UNEXPECTED_DISPLAY_SETTINGS

0xC00D1B89

NS_E_NO_AUDIODATA

0xC00D1B8A

NS_E_INPUTSOURCE_PROBLEM

0xC00D1B8B

NS_E_WME_VERSION_MISMATCH

0xC00D1B8C

NS_E_NO_REALTIME_PREPROCESS

0xC00D1B8D

NS_E_NO_REPEAT_PREPROCESS

0xC00D1B8E

NS_E_CANNOT_PAUSE_LIVEBROADCAST

0xC00D1B8F

NS_E_DRM_PROFILE_NOT_SET

0xC00D1B90

NS_E_DUPLICATE_DRMPROFILE

0xC00D1B91

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Video panel.

One or more codecs required to
open this content could not be
found.

The archive file has the same
name as an input file. Change
one of the names before
continuing.

The source has not been set up
completely.

It is not possible to apply time
compression to a broadcast
session.

It is not possible to open this
device.

It is not possible to start
encoding because the display
size or color has changed since
the current session was defined.
Restore the previous settings or
create a new session.

No audio data has been
received for several seconds.
Check the audio source and
restart the encoder.

One or all of the specified
sources are not working
properly. Check that the
sources are configured
correctly.

The supplied configuration file is
not supported by this version of
the encoder.

It is not possible to use image
preprocessing with live
encoding.

It is not possible to use two-
pass encoding when the source
is set to loop.

It is not possible to pause
encoding during a broadcast.

A DRM profile has not been set
for the current session.

The profile ID is already used
by a DRM profile. Specify a
different profile ID.

The setting of the selected

165 / 497


Return value/code

NS_E_INVALID_DEVICE

0xC00D1B92

NS_E_SPEECHEDL_ON_NON_MIXEDMODE

0xC00D1B93

NS_E_DRM_PASSWORD_TOO_LONG

0xC00D1B94

NS_E_DEVCONTROL_FAILED_SEEK

0xC00D1B95

NS_E_INTERLACE_REQUIRE_SAMESIZE

0xC00D1B96

NS_E_TOO_MANY_DEVICECONTROL

0xC00D1B97

NS_E_NO_MULTIPASS_FOR_LIVEDEVICE

0xC00D1B98

NS_E_MISSING_AUDIENCE

0xC00D1B99

NS_E_AUDIENCE_CONTENTTYPE_MISMATCH

0xC00D1B9A

NS_E_MISSING_SOURCE_INDEX

0xC00D1B9B

NS_E_NUM_LANGUAGE_MISMATCH

0xC00D1B9C

NS_E_LANGUAGE_MISMATCH

0xC00D1B9D

NS_E_VBRMODE_MISMATCH

0xC00D1B9E

NS_E_INVALID_INPUT_AUDIENCE_INDEX

0xC00D1B9F

NS_E_INVALID_INPUT_LANGUAGE

0xC00D1BA0

NS_E_INVALID_INPUT_STREAM

0xC00D1BA1

NS_E_EXPECT_MONO_WAV_INPUT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

device does not support control
for playing back tapes.

You must specify a mixed voice
and audio mode in order to use
an optimization definition file.

The specified password is too
long. Type a password with
fewer than 8 characters.

It is not possible to seek to the
specified mark-in point.

When you choose to maintain
the interlacing in your video,
the output video size must
match the input video size.

Only one device control plug-in
can control a device.

You must also enable storing
content to hard disk temporarily
in order to use two-pass
encoding with the input device.

An audience is missing from the
output stream configuration.

All audiences in the output tree
must have the same content
type.

A source index is missing from
the output stream
configuration.

The same source index in
different audiences should have
the same number of languages.

The same source index in
different audiences should have
the same languages.

The same source index in
different audiences should use
the same VBR encoding mode.

The bit rate index specified is
not valid.

The specified language is not
valid.

The specified source type is not
valid.

The source must be a mono
channel .wav file.

166 / 497


Return value/code

0xC00D1BA2

NS_E_INPUT_WAVFORMAT_MISMATCH

0xC00D1BA3

NS_E_RECORDQ_DISK_FULL

0xC00D1BA4

NS_E_NO_PAL_INVERSE_TELECINE

0xC00D1BA5

NS_E_ACTIVE_SG_DEVICE_DISCONNECTED

0xC00D1BA6

NS_E_ACTIVE_SG_DEVICE_CONTROL_DISCONNECTED

0xC00D1BA7

NS_E_NO_FRAMES_SUBMITTED_TO_ANALYZER

0xC00D1BA8

NS_E_INPUT_DOESNOT_SUPPORT_SMPTE

0xC00D1BA9

NS_E_NO_SMPTE_WITH_MULTIPLE_SOURCEGROUPS

0xC00D1BAA

NS_E_BAD_CONTENTEDL

0xC00D1BAB

NS_E_INTERLACEMODE_MISMATCH

0xC00D1BAC

NS_E_NONSQUAREPIXELMODE_MISMATCH

0xC00D1BAD

NS_E_SMPTEMODE_MISMATCH

0xC00D1BAE

NS_E_END_OF_TAPE

0xC00D1BAF

NS_E_NO_MEDIA_IN_AUDIENCE

0xC00D1BB0

NS_E_NO_AUDIENCES

0xC00D1BB1

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

All the source .wav files must
have the same format.

The hard disk being used for
temporary storage of content
has reached the minimum
allowed disk space. Create more
space on the hard disk and
restart encoding.

It is not possible to apply the
inverse telecine feature to PAL
content.

A capture device in the current
active source is no longer
available.

A device used in the current
active source for device control
is no longer available.

No frames have been submitted
to the analyzer for analysis.

The source video does not
support time codes.

It is not possible to generate a
time code when there are
multiple sources in a session.

The voice codec optimization
definition file cannot be found
or is corrupted.

The same source index in
different audiences should have
the same interlace mode.

The same source index in
different audiences should have
the same nonsquare pixel
mode.

The same source index in
different audiences should have
the same time code mode.

Either the end of the tape has
been reached or there is no
tape. Check the device and
tape.

No audio or video input has
been specified.

The profile must contain a bit
rate.

You must specify at least one

167 / 497


Return value/code

NS_E_NO_AUDIO_COMPAT

0xC00D1BB2

NS_E_INVALID_VBR_COMPAT

0xC00D1BB3

NS_E_NO_PROFILE_NAME

0xC00D1BB4

NS_E_INVALID_VBR_WITH_UNCOMP

0xC00D1BB5

NS_E_MULTIPLE_VBR_AUDIENCES

0xC00D1BB6

NS_E_UNCOMP_COMP_COMBINATION

0xC00D1BB7

NS_E_MULTIPLE_AUDIO_CODECS

0xC00D1BB8

NS_E_MULTIPLE_AUDIO_FORMATS

0xC00D1BB9

NS_E_AUDIO_BITRATE_STEPDOWN

0xC00D1BBA

NS_E_INVALID_AUDIO_PEAKRATE

0xC00D1BBB

NS_E_INVALID_AUDIO_PEAKRATE_2

0xC00D1BBC

NS_E_INVALID_AUDIO_BUFFERMAX

0xC00D1BBD

NS_E_MULTIPLE_VIDEO_CODECS

0xC00D1BBE

NS_E_MULTIPLE_VIDEO_SIZES

0xC00D1BBF

NS_E_INVALID_VIDEO_BITRATE

0xC00D1BC0

NS_E_VIDEO_BITRATE_STEPDOWN

0xC00D1BC1

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

audio stream to be compatible
with Windows Media Player 7.1.

Using a VBR encoding mode is
not compatible with Windows
Media Player 7.1.

You must specify a profile
name.

It is not possible to use a VBR
encoding mode with
uncompressed audio or video.

It is not possible to use MBR
encoding with VBR encoding.

It is not possible to mix
uncompressed and compressed
content in a session.

All audiences must use the
same audio codec.

All audiences should use the
same audio format to be
compatible with Windows Media
Player 7.1.

The audio bit rate for an
audience with a higher total bit
rate must be greater than one
with a lower total bit rate.

The audio peak bit rate setting
is not valid.

The audio peak bit rate setting
must be greater than the audio
bit rate setting.

The setting for the maximum
buffer size for audio is not valid.

All audiences must use the
same video codec.

All audiences should use the
same video size to be
compatible with Windows Media
Player 7.1.

The video bit rate setting is not
valid.

The video bit rate for an
audience with a higher total bit
rate must be greater than one
with a lower total bit rate.

The video peak bit rate setting

168 / 497


Return value/code

NS_E_INVALID_VIDEO_PEAKRATE

0xC00D1BC2

NS_E_INVALID_VIDEO_PEAKRATE_2

0xC00D1BC3

NS_E_INVALID_VIDEO_WIDTH

0xC00D1BC4

NS_E_INVALID_VIDEO_HEIGHT

0xC00D1BC5

NS_E_INVALID_VIDEO_FPS

0xC00D1BC6

NS_E_INVALID_VIDEO_KEYFRAME

0xC00D1BC7

NS_E_INVALID_VIDEO_IQUALITY

0xC00D1BC8

NS_E_INVALID_VIDEO_CQUALITY

0xC00D1BC9

NS_E_INVALID_VIDEO_BUFFER

0xC00D1BCA

NS_E_INVALID_VIDEO_BUFFERMAX

0xC00D1BCB

NS_E_INVALID_VIDEO_BUFFERMAX_2

0xC00D1BCC

NS_E_INVALID_VIDEO_WIDTH_ALIGN

0xC00D1BCD

NS_E_INVALID_VIDEO_HEIGHT_ALIGN

0xC00D1BCE

NS_E_MULTIPLE_SCRIPT_BITRATES

0xC00D1BCF

NS_E_INVALID_SCRIPT_BITRATE

0xC00D1BD0

NS_E_MULTIPLE_FILE_BITRATES

0xC00D1BD1

NS_E_INVALID_FILE_BITRATE

0xC00D1BD2

NS_E_SAME_AS_INPUT_COMBINATION

0xC00D1BD3

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

is not valid.

The video peak bit rate setting
must be greater than the video
bit rate setting.

The video width setting is not
valid.

The video height setting is not
valid.

The video frame rate setting is
not valid.

The video key frame setting is
not valid.

The video image quality setting
is not valid.

The video codec quality setting
is not valid.

The video buffer setting is not
valid.

The setting for the maximum
buffer size for video is not valid.

The value of the video
maximum buffer size setting
must be greater than the video
buffer size setting.

The alignment of the video
width is not valid.

The alignment of the video
height is not valid.

All bit rates must have the
same script bit rate.

The script bit rate specified is
not valid.

All bit rates must have the
same file transfer bit rate.

The file transfer bit rate is not
valid.

All audiences in a profile should
either be same as input or have
video width and height
specified.

This source type does not

169 / 497


Return value/code

NS_E_SOURCE_CANNOT_LOOP

0xC00D1BD4

NS_E_INVALID_FOLDDOWN_COEFFICIENTS

0xC00D1BD5

NS_E_DRMPROFILE_NOTFOUND

0xC00D1BD6

NS_E_INVALID_TIMECODE

0xC00D1BD7

NS_E_NO_AUDIO_TIMECOMPRESSION

0xC00D1BD8

NS_E_NO_TWOPASS_TIMECOMPRESSION

0xC00D1BD9

NS_E_TIMECODE_REQUIRES_VIDEOSTREAM

0xC00D1BDA

NS_E_NO_MBR_WITH_TIMECODE

0xC00D1BDB

NS_E_INVALID_INTERLACEMODE

0xC00D1BDC

NS_E_INVALID_INTERLACE_COMPAT

0xC00D1BDD

NS_E_INVALID_NONSQUAREPIXEL_COMPAT

0xC00D1BDE

NS_E_INVALID_SOURCE_WITH_DEVICE_CONTROL

0xC00D1BDF

NS_E_CANNOT_GENERATE_BROADCAST_INFO_FOR_QUALITYVBR

0xC00D1BE0

NS_E_EXCEED_MAX_DRM_PROFILE_LIMIT

0xC00D1BE1

NS_E_DEVICECONTROL_UNSTABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

support looping.

The fold-down value needs to
be between -144 and 0.

The specified DRM profile does
not exist in the system.

The specified time code is not
valid.

It is not possible to apply time
compression to a video-only
session.

It is not possible to apply time
compression to a session that is
using two-pass encoding.

It is not possible to generate a
time code for an audio-only
session.

It is not possible to generate a
time code when you are
encoding content at multiple bit
rates.

The video codec selected does
not support maintaining
interlacing in video.

Maintaining interlacing in video
is not compatible with Windows
Media Player 7.1.

Allowing nonsquare pixel output
is not compatible with Windows
Media Player 7.1.

Only capture devices can be
used with device control.

It is not possible to generate
the stream format file if you are
using quality-based VBR
encoding for the audio or video
stream. Instead use the
Windows Media file generated
after encoding to create the
announcement file.

It is not possible to create a
DRM profile because the
maximum number of profiles
has been reached. You must
delete some DRM profiles before
creating new ones.

The device is in an unstable
state. Check that the device is
functioning properly and a tape

170 / 497


Return value/code

0xC00D1BE2

NS_E_INVALID_PIXEL_ASPECT_RATIO

0xC00D1BE3

NS_E_AUDIENCE__LANGUAGE_CONTENTTYPE_MISMATCH

0xC00D1BE4

NS_E_INVALID_PROFILE_CONTENTTYPE

0xC00D1BE5

NS_E_TRANSFORM_PLUGIN_NOT_FOUND

0xC00D1BE6

NS_E_TRANSFORM_PLUGIN_INVALID

0xC00D1BE7

NS_E_EDL_REQUIRED_FOR_DEVICE_MULTIPASS

0xC00D1BE8

NS_E_INVALID_VIDEO_WIDTH_FOR_INTERLACED_ENCODING

0xC00D1BE9

NS_E_MARKIN_UNSUPPORTED

0xC00D2711

NS_E_DRM_INVALID_APPLICATION

0xC00D2712

NS_E_DRM_LICENSE_STORE_ERROR

0xC00D2713

NS_E_DRM_SECURE_STORE_ERROR

0xC00D2714

NS_E_DRM_LICENSE_STORE_SAVE_ERROR

0xC00D2715

NS_E_DRM_SECURE_STORE_UNLOCK_ERROR

0xC00D2716

NS_E_DRM_INVALID_CONTENT

0xC00D2717

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

is in place.

The pixel aspect ratio value
must be between 1 and 255.

All streams with different
languages in the same audience
must have same properties.

The profile must contain at least
one audio or video stream.

The transform plug-in could not
be found.

The transform plug-in is not
valid. It might be damaged or
you might not have the required
permissions to access the plug-
in.

To use two-pass encoding, you
must enable device control and
setup an edit decision list (EDL)
that has at least one entry.

When you choose to maintain
the interlacing in your video,
the output video size must be a
multiple of 4.

Markin/Markout is unsupported
with this source type.

A problem has occurred in the
Digital Rights Management
component. Contact product
support for this application.

License storage is not working.
Contact Microsoft product
support.

Secure storage is not working.
Contact Microsoft product
support.

License acquisition did not
work. Acquire a new license or
contact the content provider for
further assistance.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The media file is corrupted.
Contact the content provider to
get a new file.

The license is corrupted.

171 / 497


Return value/code

NS_E_DRM_UNABLE_TO_OPEN_LICENSE

0xC00D2718

NS_E_DRM_INVALID_LICENSE

0xC00D2719

NS_E_DRM_INVALID_MACHINE

0xC00D271B

NS_E_DRM_ENUM_LICENSE_FAILED

0xC00D271C

NS_E_DRM_INVALID_LICENSE_REQUEST

0xC00D271D

NS_E_DRM_UNABLE_TO_INITIALIZE

0xC00D271E

NS_E_DRM_UNABLE_TO_ACQUIRE_LICENSE

0xC00D271F

NS_E_DRM_INVALID_LICENSE_ACQUIRED

0xC00D2720

NS_E_DRM_NO_RIGHTS

0xC00D2721

NS_E_DRM_KEY_ERROR

0xC00D2722

NS_E_DRM_ENCRYPT_ERROR

0xC00D2723

NS_E_DRM_DECRYPT_ERROR

0xC00D2725

NS_E_DRM_LICENSE_INVALID_XML

0xC00D2728

NS_E_DRM_NEEDS_INDIVIDUALIZATION

0xC00D2729

NS_E_DRM_ALREADY_INDIVIDUALIZED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Acquire a new license.

The license is corrupted or
invalid. Acquire a new license

Licenses cannot be copied from
one computer to another. Use
License Management to transfer
licenses, or get a new license
for the media file.

License storage is not working.
Contact Microsoft product
support.

The media file is corrupted.
Contact the content provider to
get a new file.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The license could not be
acquired. Try again later.

License acquisition did not
work. Acquire a new license or
contact the content provider for
further assistance.

The requested operation cannot
be performed on this file.

The requested action cannot be
performed because a problem
occurred with the Windows
Media Digital Rights
Management (DRM)
components on your computer.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The media file is corrupted.
Contact the content provider to
get a new file.

The license is corrupted.
Acquire a new license.

A security upgrade is required
to perform the operation on this
media file.

You already have the latest
security components. No
upgrade is necessary at this
time.

172 / 497


Return value/code

0xC00D272A

NS_E_DRM_ACTION_NOT_QUERIED

0xC00D272B

NS_E_DRM_ACQUIRING_LICENSE

0xC00D272C

NS_E_DRM_INDIVIDUALIZING

0xC00D272D

NS_E_BACKUP_RESTORE_FAILURE

0xC00D272E

NS_E_BACKUP_RESTORE_BAD_REQUEST_ID

0xC00D272F

NS_E_DRM_PARAMETERS_MISMATCHED

0xC00D2730

NS_E_DRM_UNABLE_TO_CREATE_LICENSE_OBJECT

0xC00D2731

NS_E_DRM_UNABLE_TO_CREATE_INDI_OBJECT

0xC00D2732

NS_E_DRM_UNABLE_TO_CREATE_ENCRYPT_OBJECT

0xC00D2733

NS_E_DRM_UNABLE_TO_CREATE_DECRYPT_OBJECT

0xC00D2734

NS_E_DRM_UNABLE_TO_CREATE_PROPERTIES_OBJECT

0xC00D2735

NS_E_DRM_UNABLE_TO_CREATE_BACKUP_OBJECT

0xC00D2736

NS_E_DRM_INDIVIDUALIZE_ERROR

0xC00D2737

NS_E_DRM_LICENSE_OPEN_ERROR

0xC00D2738

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The application cannot perform
this action. Contact product
support for this application.

You cannot begin a new license
acquisition process until the
current one has been
completed.

You cannot begin a new security
upgrade until the current one
has been completed.

Failure in Backup-Restore.

Bad Request ID in Backup-
Restore.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A license cannot be created for
this media file. Reinstall the
application.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The security upgrade failed. Try
again later.

License storage is not working.
Contact Microsoft product
support.

License storage is not working.
Contact Microsoft product

173 / 497


Return value/code

NS_E_DRM_LICENSE_CLOSE_ERROR

0xC00D2739

NS_E_DRM_GET_LICENSE_ERROR

0xC00D273A

NS_E_DRM_QUERY_ERROR

0xC00D273B

NS_E_DRM_REPORT_ERROR

0xC00D273C

NS_E_DRM_GET_LICENSESTRING_ERROR

0xC00D273D

NS_E_DRM_GET_CONTENTSTRING_ERROR

0xC00D273E

NS_E_DRM_MONITOR_ERROR

0xC00D273F

NS_E_DRM_UNABLE_TO_SET_PARAMETER

0xC00D2740

NS_E_DRM_INVALID_APPDATA

0xC00D2741

NS_E_DRM_INVALID_APPDATA_VERSION

0xC00D2742

NS_E_DRM_BACKUP_EXISTS

0xC00D2743

NS_E_DRM_BACKUP_CORRUPT

0xC00D2744

NS_E_DRM_BACKUPRESTORE_BUSY

0xC00D2745

NS_E_BACKUP_RESTORE_BAD_DATA

0xC00D2748

NS_E_DRM_LICENSE_UNUSABLE

0xC00D2749

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

support.

License storage is not working.
Contact Microsoft product
support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact product
support for this application.

License storage is not working.
Contact Microsoft product
support.

The media file is corrupted.
Contact the content provider to
get a new file.

A problem has occurred in the
Digital Rights Management
component. Try again later.

The application has made an
invalid call to the Digital Rights
Management component.
Contact product support for this
application.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact product
support for this application.

Licenses are already backed up
in this location.

One or more backed-up licenses
are missing or corrupt.

You cannot begin a new backup
process until the current
process has been completed.

Bad Data sent to Backup-
Restore.

The license is invalid. Contact
the content provider for further
assistance.

A required property was not set

174 / 497


Return value/code

NS_E_DRM_INVALID_PROPERTY

0xC00D274A

NS_E_DRM_SECURE_STORE_NOT_FOUND

0xC00D274B

NS_E_DRM_CACHED_CONTENT_ERROR

0xC00D274C

NS_E_DRM_INDIVIDUALIZATION_INCOMPLETE

0xC00D274D

NS_E_DRM_DRIVER_AUTH_FAILURE

0xC00D274E

NS_E_DRM_NEED_UPGRADE_MSSAP

0xC00D274F

NS_E_DRM_REOPEN_CONTENT

0xC00D2750

NS_E_DRM_DRIVER_DIGIOUT_FAILURE

0xC00D2751

NS_E_DRM_INVALID_SECURESTORE_PASSWORD

0xC00D2752

NS_E_DRM_APPCERT_REVOKED

0xC00D2753

NS_E_DRM_RESTORE_FRAUD

0xC00D2754

NS_E_DRM_HARDWARE_INCONSISTENT

0xC00D2755

NS_E_DRM_SDMI_TRIGGER

0xC00D2756

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

by the application. Contact
product support for this
application.

A problem has occurred in the
Digital Rights Management
component of this application.
Try to acquire a license again.

A license cannot be found for
this media file. Use License
Management to transfer a
license for this file from the
original computer, or acquire a
new license.

A problem occurred during the
security upgrade. Try again
later.

Certified driver components are
required to play this media file.
Contact Windows Update to see
whether updated drivers are
available for your hardware.

One or more of the Secure
Audio Path components were
not found or an entry point in
those components was not
found.

Status message: Reopen the
file.

Certain driver functionality is
required to play this media file.
Contact Windows Update to see
whether updated drivers are
available for your hardware.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

You cannot restore your
license(s).

The licenses for your media files
are corrupted. Contact Microsoft
product support.

To transfer this media file, you
must upgrade the application.

You cannot make any more

175 / 497


Return value/code

NS_E_DRM_SDMI_NOMORECOPIES

0xC00D2757

NS_E_DRM_UNABLE_TO_CREATE_HEADER_OBJECT

0xC00D2758

NS_E_DRM_UNABLE_TO_CREATE_KEYS_OBJECT

0xC00D2759

NS_E_DRM_LICENSE_NOTACQUIRED

0xC00D275A

NS_E_DRM_UNABLE_TO_CREATE_CODING_OBJECT

0xC00D275B

NS_E_DRM_UNABLE_TO_CREATE_STATE_DATA_OBJECT

0xC00D275C

NS_E_DRM_BUFFER_TOO_SMALL

0xC00D275D

NS_E_DRM_UNSUPPORTED_PROPERTY

0xC00D275E

NS_E_DRM_ERROR_BAD_NET_RESP

0xC00D275F

NS_E_DRM_STORE_NOTALLSTORED

0xC00D2760

NS_E_DRM_SECURITY_COMPONENT_SIGNATURE_INVALID

0xC00D2761

NS_E_DRM_INVALID_DATA

0xC00D2762

NS_E_DRM_POLICY_DISABLE_ONLINE

0xC00D2763

NS_E_DRM_UNABLE_TO_CREATE_AUTHENTICATION_OBJECT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

copies of this media file.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

Unable to obtain license.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The buffer supplied is not
sufficient.

The property requested is not
supported.

The specified server cannot
perform the requested
operation.

Some of the licenses could not
be stored.

The Digital Rights Management
security upgrade component
could not be validated. Contact
Microsoft product support.

Invalid or corrupt data was
encountered.

The Windows Media Digital
Rights Management system
cannot perform the requested
action because your computer
or network administrator has
enabled the group policy
Prevent Windows Media DRM
Internet Access. For assistance,
contact your administrator.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

176 / 497


Return value/code

0xC00D2764

NS_E_DRM_NOT_CONFIGURED

0xC00D2765

NS_E_DRM_DEVICE_ACTIVATION_CANCELED

0xC00D2766

NS_E_BACKUP_RESTORE_TOO_MANY_RESETS

0xC00D2767

NS_E_DRM_DEBUGGING_NOT_ALLOWED

0xC00D2768

NS_E_DRM_OPERATION_CANCELED

0xC00D2769

NS_E_DRM_RESTRICTIONS_NOT_RETRIEVED

0xC00D276A

NS_E_DRM_UNABLE_TO_CREATE_PLAYLIST_OBJECT

0xC00D276B

NS_E_DRM_UNABLE_TO_CREATE_PLAYLIST_BURN_OBJECT

0xC00D276C

NS_E_DRM_UNABLE_TO_CREATE_DEVICE_REGISTRATION_OBJECT

0xC00D276D

NS_E_DRM_UNABLE_TO_CREATE_METERING_OBJECT

0xC00D2770

NS_E_DRM_TRACK_EXCEEDED_PLAYLIST_RESTICTION

0xC00D2771

NS_E_DRM_TRACK_EXCEEDED_TRACKBURN_RESTRICTION

0xC00D2772

NS_E_DRM_UNABLE_TO_GET_DEVICE_CERT

0xC00D2773

NS_E_DRM_UNABLE_TO_GET_SECURE_CLOCK

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Not all of the necessary
properties for DRM have been
set.

The portable device does not
have the security required to
copy protected files to it. To
obtain the additional security,
try to copy the file to your
portable device again. When a
message appears, click OK.

Too many resets in Backup-
Restore.

Running this process under a
debugger while using DRM
content is not allowed.

The user canceled the DRM
operation.

The license you are using has
assocaited output restrictions.
This license is unusable until
these restrictions are queried.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The specified track has
exceeded it's specified playlist
burn limit in this playlist.

The specified track has
exceeded it's track burn limit.

A problem has occurred in
obtaining the device's
certificate. Contact Microsoft
product support.

A problem has occurred in
obtaining the device's secure
clock. Contact Microsoft product

177 / 497


Return value/code

0xC00D2774

NS_E_DRM_UNABLE_TO_SET_SECURE_CLOCK

0xC00D2775

NS_E_DRM_UNABLE_TO_GET_SECURE_CLOCK_FROM_SERVER

0xC00D2776

NS_E_DRM_POLICY_METERING_DISABLED

0xC00D2777

NS_E_DRM_TRANSFER_CHAINED_LICENSES_UNSUPPORTED

0xC00D2778

NS_E_DRM_SDK_VERSIONMISMATCH

0xC00D2779

NS_E_DRM_LIC_NEEDS_DEVICE_CLOCK_SET

0xC00D277A

NS_E_LICENSE_HEADER_MISSING_URL

0xC00D277B

NS_E_DEVICE_NOT_WMDRM_DEVICE

0xC00D277C

NS_E_DRM_INVALID_APPCERT

0xC00D277D

NS_E_DRM_PROTOCOL_FORCEFUL_TERMINATION_ON_PETITION

0xC00D277E

NS_E_DRM_PROTOCOL_FORCEFUL_TERMINATION_ON_CHALLENGE

0xC00D277F

NS_E_DRM_CHECKPOINT_FAILED

0xC00D2780

NS_E_DRM_BB_UNABLE_TO_INITIALIZE

0xC00D2781

NS_E_DRM_UNABLE_TO_LOAD_HARDWARE_ID

0xC00D2782

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

support.

A problem has occurred in
setting the device's secure
clock. Contact Microsoft product
support.

A problem has occurred in
obtaining the secure clock from
server. Contact Microsoft
product support.

This content requires the
metering policy to be enabled.

Transfer of chained licenses
unsupported.

The Digital Rights Management
component is not installed
properly. Reinstall the Player.

The file could not be transferred
because the device clock is not
set.

The content header is missing
an acquisition URL.

The current attached device
does not support WMDRM.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

The client application has been
forcefully terminated during a
DRM petition.

The client application has been
forcefully terminated during a
DRM challenge.

Secure storage protection error.
Restore your licenses from a
previous backup and try again.

A problem has occurred in the
Digital Rights Management root
of trust. Contact Microsoft
product support.

A problem has occurred in
retrieving the Digital Rights
Management machine
identification. Contact Microsoft
product support.

A problem has occurred in
opening the Digital Rights

178 / 497


Return value/code

NS_E_DRM_UNABLE_TO_OPEN_DATA_STORE

0xC00D2783

NS_E_DRM_DATASTORE_CORRUPT

0xC00D2784

NS_E_DRM_UNABLE_TO_CREATE_INMEMORYSTORE_OBJECT

0xC00D2785

NS_E_DRM_STUBLIB_REQUIRED

0xC00D2786

NS_E_DRM_UNABLE_TO_CREATE_CERTIFICATE_OBJECT

0xC00D2787

NS_E_DRM_MIGRATION_TARGET_NOT_ONLINE

0xC00D2788

NS_E_DRM_INVALID_MIGRATION_IMAGE

0xC00D2789

NS_E_DRM_MIGRATION_TARGET_STATES_CORRUPTED

0xC00D278A

NS_E_DRM_MIGRATION_IMPORTER_NOT_AVAILABLE

0xC00D278B

NS_DRM_E_MIGRATION_UPGRADE_WITH_DIFF_SID

0xC00D278C

NS_DRM_E_MIGRATION_SOURCE_MACHINE_IN_USE

0xC00D278D

NS_DRM_E_MIGRATION_TARGET_MACHINE_LESS_THAN_LH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Management data storage file.
Contact Microsoft product.

The Digital Rights Management
data storage is not functioning
properly. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A secured library is required to
access the requested
functionality.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component during license
migration. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component during license
migration. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component during license
migration. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component during license
migration. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component during license
migration. Contact Microsoft
product support.

The Digital Rights Management
component is in use during
license migration. Contact
Microsoft product support.

Licenses are being migrated to
a machine running XP or
downlevel OS. This operation
can only be performed on
Windows Vista or a later OS.
Contact Microsoft product

179 / 497


Return value/code

0xC00D278E

NS_DRM_E_MIGRATION_IMAGE_ALREADY_EXISTS

0xC00D278F

NS_E_DRM_HARDWAREID_MISMATCH

0xC00D2790

NS_E_INVALID_DRMV2CLT_STUBLIB

0xC00D2791

NS_E_DRM_MIGRATION_INVALID_LEGACYV2_DATA

0xC00D2792

NS_E_DRM_MIGRATION_LICENSE_ALREADY_EXISTS

0xC00D2793

NS_E_DRM_MIGRATION_INVALID_LEGACYV2_SST_PASSWORD

0xC00D2794

NS_E_DRM_MIGRATION_NOT_SUPPORTED

0xC00D2795

NS_E_DRM_UNABLE_TO_CREATE_MIGRATION_IMPORTER_OBJECT

0xC00D2796

NS_E_DRM_CHECKPOINT_MISMATCH

0xC00D2797

NS_E_DRM_CHECKPOINT_CORRUPT

0xC00D2798

NS_E_REG_FLUSH_FAILURE

0xC00D2799

NS_E_HDS_KEY_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

support.

Migration Image already exists.
Contact Microsoft product
support.

The requested action cannot be
performed because a hardware
configuration change has been
detected by the Windows Media
Digital Rights Management
(DRM) components on your
computer.

The wrong stublib has been
linked to an application or DLL
using drmv2clt.dll.

The legacy V2 data being
imported is invalid.

The license being imported
already exists.

The password of the Legacy V2
SST entry being imported is
incorrect.

Migration is not supported by
the plugin.

A migration importer cannot be
created for this media file.
Reinstall the application.

The requested action cannot be
performed because a problem
occurred with the Windows
Media Digital Rights
Management (DRM)
components on your computer.

The requested action cannot be
performed because a problem
occurred with the Windows
Media Digital Rights
Management (DRM)
components on your computer.

The requested action cannot be
performed because a problem
occurred with the Windows
Media Digital Rights
Management (DRM)
components on your computer.

The requested action cannot be
performed because a problem
occurred with the Windows
Media Digital Rights
Management (DRM)
components on your computer.

180 / 497


Return value/code

0xC00D279A

NS_E_DRM_MIGRATION_OPERATION_CANCELLED

0xC00D279B

NS_E_DRM_MIGRATION_OBJECT_IN_USE

0xC00D279C

NS_E_DRM_MALFORMED_CONTENT_HEADER

0xC00D27D8

NS_E_DRM_LICENSE_EXPIRED

0xC00D27D9

NS_E_DRM_LICENSE_NOTENABLED

0xC00D27DA

NS_E_DRM_LICENSE_APPSECLOW

0xC00D27DB

NS_E_DRM_STORE_NEEDINDI

0xC00D27DC

NS_E_DRM_STORE_NOTALLOWED

0xC00D27DD

NS_E_DRM_LICENSE_APP_NOTALLOWED

0xC00D27DF

NS_E_DRM_LICENSE_CERT_EXPIRED

0xC00D27E0

NS_E_DRM_LICENSE_SECLOW

0xC00D27E1

NS_E_DRM_LICENSE_CONTENT_REVOKED

0xC00D27E2

NS_E_DRM_DEVICE_NOT_REGISTERED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Migration was canceled by the
user.

Migration object is already in
use and cannot be called until
the current operation
completes.

The content header does not
comply with DRM requirements
and cannot be used.

The license for this file has
expired and is no longer valid.
Contact your content provider
for further assistance.

The license for this file is not
valid yet, but will be at a future
date.

The license for this file requires
a higher level of security than
the player you are currently
using has. Try using a different
player or download a newer
version of your current player.

The license cannot be stored as
it requires security upgrade of
Digital Rights Management
component.

Your machine does not meet
the requirements for storing the
license.

The license for this file requires
an upgraded version of your
player or a different player.

The license server's certificate
expired. Make sure your system
clock is set correctly. Contact
your content provider for
further assistance.

The license for this file requires
a higher level of security than
the player you are currently
using has. Try using a different
player or download a newer
version of your current player.

The content owner for the
license you just acquired is no
longer supporting their content.
Contact the content owner for a
newer version of the content.

The content owner for the
license you just acquired

181 / 497


Return value/code

0xC00D280A

NS_E_DRM_LICENSE_NOSAP

0xC00D280B

NS_E_DRM_LICENSE_NOSVP

0xC00D280C

NS_E_DRM_LICENSE_NOWDM

0xC00D280D

NS_E_DRM_LICENSE_NOTRUSTEDCODEC

0xC00D280E

NS_E_DRM_SOURCEID_NOT_SUPPORTED

0xC00D283D

NS_E_DRM_NEEDS_UPGRADE_TEMPFILE

0xC00D283E

NS_E_DRM_NEED_UPGRADE_PD

0xC00D283F

NS_E_DRM_SIGNATURE_FAILURE

0xC00D2840

NS_E_DRM_LICENSE_SERVER_INFO_MISSING

0xC00D2841

NS_E_DRM_BUSY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

requires your device to register
to the current machine.

The license for this file requires
a feature that is not supported
in your current player or
operating system. You can try
with newer version of your
current player or contact your
content provider for further
assistance.

The license for this file requires
a feature that is not supported
in your current player or
operating system. You can try
with newer version of your
current player or contact your
content provider for further
assistance.

The license for this file requires
Windows Driver Model (WDM)
audio drivers. Contact your
sound card manufacturer for
further assistance.

The license for this file requires
a higher level of security than
the player you are currently
using has. Try using a different
player or download a newer
version of your current player.

The license for this file is not
supported by your current
player. You can try with newer
version of your current player
or contact your content provider
for further assistance.

An updated version of your
media player is required to play
the selected content.

A new version of the Digital
Rights Management component
is required. Contact product
support for this application to
get the latest version.

Failed to either create or verify
the content header.

Could not read the necessary
information from the system
registry.

The DRM subsystem is currently
locked by another application or
user. Try again later.

182 / 497


Return value/code

0xC00D2842

NS_E_DRM_PD_TOO_MANY_DEVICES

0xC00D2843

NS_E_DRM_INDIV_FRAUD

0xC00D2844

NS_E_DRM_INDIV_NO_CABS

0xC00D2845

NS_E_DRM_INDIV_SERVICE_UNAVAILABLE

0xC00D2846

NS_E_DRM_RESTORE_SERVICE_UNAVAILABLE

0xC00D2847

NS_E_DRM_CLIENT_CODE_EXPIRED

0xC00D2848

NS_E_DRM_NO_UPLINK_LICENSE

0xC00D2849

NS_E_DRM_INVALID_KID

0xC00D284A

NS_E_DRM_LICENSE_INITIALIZATION_ERROR

0xC00D284C

NS_E_DRM_CHAIN_TOO_LONG

0xC00D284D

NS_E_DRM_UNSUPPORTED_ALGORITHM

0xC00D284E

NS_E_DRM_LICENSE_DELETION_ERROR

0xC00D28A0

NS_E_DRM_INVALID_CERTIFICATE

0xC00D28A1

NS_E_DRM_CERTIFICATE_REVOKED

0xC00D28A2

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There are too many target
devices registered on the
portable media.

The security upgrade cannot be
completed because the allowed
number of daily upgrades has
been exceeded. Try again
tomorrow.

The security upgrade cannot be
completed because the server is
unable to perform the
operation. Try again later.

The security upgrade cannot be
performed because the server is
not available. Try again later.

Windows Media Player cannot
restore your licenses because
the server is not available. Try
again later.

Windows Media Player cannot
play the protected file. Verify
that your computer's date is set
correctly. If it is correct, on the
Help menu, click Check for
Player Updates to install the
latest version of the Player.

The chained license cannot be
created because the referenced
uplink license does not exist.

The specified KID is invalid.

License initialization did not
work. Contact Microsoft product
support.

The uplink license of a chained
license cannot itself be a
chained license.

The specified encryption
algorithm is unsupported.

License deletion did not work.
Contact Microsoft product
support.

The client's certificate is
corrupted or the signature
cannot be verified.

The client's certificate has been
revoked.

There is no license available for

183 / 497


Return value/code

NS_E_DRM_LICENSE_UNAVAILABLE

0xC00D28A3

NS_E_DRM_DEVICE_LIMIT_REACHED

0xC00D28A4

NS_E_DRM_UNABLE_TO_VERIFY_PROXIMITY

0xC00D28A5

NS_E_DRM_MUST_REGISTER

0xC00D28A6

NS_E_DRM_MUST_APPROVE

0xC00D28A7

NS_E_DRM_MUST_REVALIDATE

0xC00D28A8

NS_E_DRM_INVALID_PROXIMITY_RESPONSE

0xC00D28A9

NS_E_DRM_INVALID_SESSION

0xC00D28AA

NS_E_DRM_DEVICE_NOT_OPEN

0xC00D28AB

NS_E_DRM_DEVICE_ALREADY_REGISTERED

0xC00D28AC

NS_E_DRM_UNSUPPORTED_PROTOCOL_VERSION

0xC00D28AD

NS_E_DRM_UNSUPPORTED_ACTION

0xC00D28AE

NS_E_DRM_CERTIFICATE_SECURITY_LEVEL_INADEQUATE

0xC00D28AF

NS_E_DRM_UNABLE_TO_OPEN_PORT

0xC00D28B0

NS_E_DRM_BAD_REQUEST

0xC00D28B1

NS_E_DRM_INVALID_CRL

0xC00D28B2

NS_E_DRM_ATTRIBUTE_TOO_LONG

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the requested action.

The maximum number of
devices in use has been
reached. Unable to open
additional devices.

The proximity detection
procedure could not confirm
that the receiver is near the
transmitter in the network.

The client must be registered
before executing the intended
operation.

The client must be approved
before executing the intended
operation.

The client must be revalidated
before executing the intended
operation.

The response to the proximity
detection challenge is invalid.

The requested session is invalid.

The device must be opened
before it can be used to receive
content.

Device registration failed
because the device is already
registered.

Unsupported WMDRM-ND
protocol version.

The requested action is not
supported.

The certificate does not have an
adequate security level for the
requested action.

Unable to open the specified
port for receiving Proximity
messages.

The message format is invalid.

The Certificate Revocation List
is invalid or corrupted.

The length of the attribute
name or value is too long.

184 / 497


Return value/code

0xC00D28B3

NS_E_DRM_EXPIRED_LICENSEBLOB

0xC00D28B4

NS_E_DRM_INVALID_LICENSEBLOB

0xC00D28B5

NS_E_DRM_INCLUSION_LIST_REQUIRED

0xC00D28B6

NS_E_DRM_DRMV2CLT_REVOKED

0xC00D28B7

NS_E_DRM_RIV_TOO_SMALL

0xC00D2904

NS_E_OUTPUT_PROTECTION_LEVEL_UNSUPPORTED

0xC00D2905

NS_E_COMPRESSED_DIGITAL_VIDEO_PROTECTION_LEVEL_UNSUPPORTED

0xC00D2906

NS_E_UNCOMPRESSED_DIGITAL_VIDEO_PROTECTION_LEVEL_UNSUPPORTED

0xC00D2907

NS_E_ANALOG_VIDEO_PROTECTION_LEVEL_UNSUPPORTED

0xC00D2908

NS_E_COMPRESSED_DIGITAL_AUDIO_PROTECTION_LEVEL_UNSUPPORTED

0xC00D2909

NS_E_UNCOMPRESSED_DIGITAL_AUDIO_PROTECTION_LEVEL_UNSUPPORTED

0xC00D290A

NS_E_OUTPUT_PROTECTION_SCHEME_UNSUPPORTED

0xC00D2AFA

NS_E_REBOOT_RECOMMENDED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The license blob passed in the
cardea request is expired.

The license blob passed in the
cardea request is invalid.
Contact Microsoft product
support.

The requested operation cannot
be performed because the
license does not contain an
inclusion list.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

A problem has occurred in the
Digital Rights Management
component. Contact Microsoft
product support.

Windows Media Player does not
support the level of output
protection required by the
content.

Windows Media Player does not
support the level of protection
required for compressed digital
video.

Windows Media Player does not
support the level of protection
required for uncompressed
digital video.

Windows Media Player does not
support the level of protection
required for analog video.

Windows Media Player does not
support the level of protection
required for compressed digital
audio.

Windows Media Player does not
support the level of protection
required for uncompressed
digital audio.

Windows Media Player does not
support the scheme of output
protection required by the
content.

Installation was not successful
and some file cleanup is not
complete. For best results,
restart your computer.

185 / 497


Return value/code

0xC00D2AFB

NS_E_REBOOT_REQUIRED

0xC00D2AFC

NS_E_SETUP_INCOMPLETE

0xC00D2AFD

NS_E_SETUP_DRM_MIGRATION_FAILED

0xC00D2AFE

NS_E_SETUP_IGNORABLE_FAILURE

0xC00D2AFF

NS_E_SETUP_DRM_MIGRATION_FAILED_AND_IGNORABLE_FAILURE

0xC00D2B00

NS_E_SETUP_BLOCKED

0xC00D2EE0

NS_E_UNKNOWN_PROTOCOL

0xC00D2EE1

NS_E_REDIRECT_TO_PROXY

0xC00D2EE2

NS_E_INTERNAL_SERVER_ERROR

0xC00D2EE3

NS_E_BAD_REQUEST

0xC00D2EE4

NS_E_ERROR_FROM_PROXY

0xC00D2EE5

NS_E_PROXY_TIMEOUT

0xC00D2EE6

NS_E_SERVER_UNAVAILABLE

0xC00D2EE7

NS_E_REFUSED_BY_SERVER

0xC00D2EE8

NS_E_INCOMPATIBLE_SERVER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Installation was not successful.
To continue, you must restart
your computer.

Installation was not successful.

Setup cannot migrate the
Windows Media Digital Rights
Management (DRM)
components.

Some skin or playlist
components cannot be installed.

Setup cannot migrate the
Windows Media Digital Rights
Management (DRM)
components. In addition, some
skin or playlist components
cannot be installed.

Installation is blocked because
your computer does not meet
one or more of the setup
requirements.

The specified protocol is not
supported.

The client is redirected to a
proxy server.

The server encountered an
unexpected condition which
prevented it from fulfilling the
request.

The request could not be
understood by the server.

The proxy experienced an error
while attempting to contact the
media server.

The proxy did not receive a
timely response while
attempting to contact the media
server.

The server is currently unable
to handle the request due to a
temporary overloading or
maintenance of the server.

The server is refusing to fulfill
the requested operation.

The server is not a compatible
streaming media server.

186 / 497


Return value/code

0xC00D2EE9

NS_E_MULTICAST_DISABLED

0xC00D2EEA

NS_E_INVALID_REDIRECT

0xC00D2EEB

NS_E_ALL_PROTOCOLS_DISABLED

0xC00D2EEC

NS_E_MSBD_NO_LONGER_SUPPORTED

0xC00D2EED

NS_E_PROXY_NOT_FOUND

0xC00D2EEE

NS_E_CANNOT_CONNECT_TO_PROXY

0xC00D2EEF

NS_E_SERVER_DNS_TIMEOUT

0xC00D2EF0

NS_E_PROXY_DNS_TIMEOUT

0xC00D2EF1

NS_E_CLOSED_ON_SUSPEND

0xC00D2EF2

NS_E_CANNOT_READ_PLAYLIST_FROM_MEDIASERVER

0xC00D2EF3

NS_E_SESSION_NOT_FOUND

0xC00D2EF4

NS_E_REQUIRE_STREAMING_CLIENT

0xC00D2EF5

NS_E_PLAYLIST_ENTRY_HAS_CHANGED

0xC00D2EF6

NS_E_PROXY_ACCESSDENIED

0xC00D2EF7

NS_E_PROXY_SOURCE_ACCESSDENIED

0xC00D2EF8

NS_E_NETWORK_SINK_WRITE

0xC00D2EF9

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The content cannot be streamed
because the Multicast protocol
has been disabled.

The server redirected the player
to an invalid location.

The content cannot be streamed
because all protocols have been
disabled.

The MSBD protocol is no longer
supported. Please use HTTP to
connect to the Windows Media
stream.

The proxy server could not be
located. Please check your
proxy server configuration.

Unable to establish a connection
to the proxy server. Please
check your proxy server
configuration.

Unable to locate the media
server. The operation timed
out.

Unable to locate the proxy
server. The operation timed
out.

Media closed because Windows
was shut down.

Unable to read the contents of a
playlist file from a media server.

Session not found.

Content requires a streaming
media client.

A command applies to a
previous playlist entry.

The proxy server is denying
access. The username and/or
password might be incorrect.

The proxy could not provide
valid authentication credentials
to the media server.

The network sink failed to write
data to the network.

Packets are not being received
from the server. The packets

187 / 497


Return value/code

NS_E_FIREWALL

0xC00D2EFA

NS_E_MMS_NOT_SUPPORTED

0xC00D2EFB

NS_E_SERVER_ACCESSDENIED

0xC00D2EFC

NS_E_RESOURCE_GONE

0xC00D2EFD

NS_E_NO_EXISTING_PACKETIZER

0xC00D2EFE

NS_E_BAD_SYNTAX_IN_SERVER_RESPONSE

0xC00D2F00

NS_E_RESET_SOCKET_CONNECTION

0xC00D2F02

NS_E_TOO_MANY_HOPS

0xC00D2F05

NS_E_TOO_MUCH_DATA_FROM_SERVER

0xC00D2F06

NS_E_CONNECT_TIMEOUT

0xC00D2F07

NS_E_PROXY_CONNECT_TIMEOUT

0xC00D2F08

NS_E_SESSION_INVALID

0xC00D2F0A

NS_E_PACKETSINK_UNKNOWN_FEC_STREAM

0xC00D2F0B

NS_E_PUSH_CANNOTCONNECT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

might be blocked by a filtering
device, such as a network
firewall.

The MMS protocol is not
supported. Please use HTTP or
RTSP to connect to the Windows
Media stream.

The Windows Media server is
denying access. The username
and/or password might be
incorrect.

The Publishing Point or file on
the Windows Media Server is no
longer available.

There is no existing packetizer
plugin for a stream.

The response from the media
server could not be understood.
This might be caused by an
incompatible proxy server or
media server.

The Windows Media Server
reset the network connection.

The request could not reach the
media server (too many hops).

The server is sending too much
data. The connection has been
terminated.

It was not possible to establish
a connection to the media
server in a timely manner. The
media server might be down for
maintenance, or it might be
necessary to use a proxy server
to access this media server.

It was not possible to establish
a connection to the proxy
server in a timely manner.
Please check your proxy server
configuration.

Session not found.

Unknown packet sink stream.

Unable to establish a connection
to the server. Ensure Windows
Media Services is started and
the HTTP Server control

188 / 497


Return value/code

0xC00D2F0C

NS_E_INCOMPATIBLE_PUSH_SERVER

0xC00D32C8

NS_E_END_OF_PLAYLIST

0xC00D32C9

NS_E_USE_FILE_SOURCE

0xC00D32CA

NS_E_PROPERTY_NOT_FOUND

0xC00D32CC

NS_E_PROPERTY_READ_ONLY

0xC00D32CD

NS_E_TABLE_KEY_NOT_FOUND

0xC00D32CF

NS_E_INVALID_QUERY_OPERATOR

0xC00D32D0

NS_E_INVALID_QUERY_PROPERTY

0xC00D32D2

NS_E_PROPERTY_NOT_SUPPORTED

0xC00D32D4

NS_E_SCHEMA_CLASSIFY_FAILURE

0xC00D32D5

NS_E_METADATA_FORMAT_NOT_SUPPORTED

0xC00D32D6

NS_E_METADATA_NO_EDITING_CAPABILITY

0xC00D32D7

NS_E_METADATA_CANNOT_SET_LOCALE

0xC00D32D8

NS_E_METADATA_LANGUAGE_NOT_SUPORTED

0xC00D32D9

NS_E_METADATA_NO_RFC1766_NAME_FOR_LOCALE

0xC00D32DA

NS_E_METADATA_NOT_AVAILABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

protocol is properly enabled.

The Server service that received
the HTTP push request is not a
compatible version of Windows
Media Services (WMS). This
error might indicate the push
request was received by IIS
instead of WMS. Ensure WMS is
started and has the HTTP
Server control protocol properly
enabled and try again.

The playlist has reached its end.

Use file source.

The property was not found.

The property is read only.

The table key was not found.

Invalid query operator.

Invalid query property.

The property is not supported.

Schema classification failure.

The metadata format is not
supported.

Cannot edit the metadata.

Cannot set the locale id.

The language is not supported
in the format.

There is no RFC1766 name
translation for the supplied
locale id.

The metadata (or metadata
item) is not available.

189 / 497


Return value/code

0xC00D32DB

NS_E_METADATA_CACHE_DATA_NOT_AVAILABLE

0xC00D32DC

NS_E_METADATA_INVALID_DOCUMENT_TYPE

0xC00D32DD

NS_E_METADATA_IDENTIFIER_NOT_AVAILABLE

0xC00D32DE

NS_E_METADATA_CANNOT_RETRIEVE_FROM_OFFLINE_CACHE

0xC0261003

ERROR_MONITOR_INVALID_DESCRIPTOR_CHECKSUM

0xC0261004

ERROR_MONITOR_INVALID_STANDARD_TIMING_BLOCK

0xC0261005

ERROR_MONITOR_WMI_DATABLOCK_REGISTRATION_FAILED

0xC0261006

ERROR_MONITOR_INVALID_SERIAL_NUMBER_MONDSC_BLOCK

0xC0261007

ERROR_MONITOR_INVALID_USER_FRIENDLY_MONDSC_BLOCK

0xC0261008

ERROR_MONITOR_NO_MORE_DESCRIPTOR_DATA

0xC0261009

ERROR_MONITOR_INVALID_DETAILED_TIMING_BLOCK

0xC0262000

ERROR_GRAPHICS_NOT_EXCLUSIVE_MODE_OWNER

0xC0262001

ERROR_GRAPHICS_INSUFFICIENT_DMA_BUFFER

0xC0262002

ERROR_GRAPHICS_INVALID_DISPLAY_ADAPTER

0xC0262003

ERROR_GRAPHICS_ADAPTER_WAS_RESET

0xC0262004

ERROR_GRAPHICS_INVALID_DRIVER_MODEL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The cached metadata (or
metadata item) is not available.

The metadata document is
invalid.

The metadata content identifier
is not available.

Cannot retrieve metadata from
the offline metadata cache.

Checksum of the obtained
monitor descriptor is invalid.

Monitor descriptor contains an
invalid standard timing block.

Windows Management
Instrumentation (WMI) data
block registration failed for one
of the MSMonitorClass WMI
subclasses.

Provided monitor descriptor
block is either corrupted or does
not contain the monitor's
detailed serial number.

Provided monitor descriptor
block is either corrupted or does
not contain the monitor's user-
friendly name.

There is no monitor descriptor
data at the specified (offset,
size) region.

Monitor descriptor contains an
invalid detailed timing block.

Exclusive mode ownership is
needed to create unmanaged
primary allocation.

The driver needs more direct
memory access (DMA) buffer
space to complete the
requested operation.

Specified display adapter handle
is invalid.

Specified display adapter and all
of its state has been reset.

The driver stack does not match
the expected driver model.

190 / 497


Return value/code

0xC0262005

ERROR_GRAPHICS_PRESENT_MODE_CHANGED

0xC0262006

ERROR_GRAPHICS_PRESENT_OCCLUDED

0xC0262007

ERROR_GRAPHICS_PRESENT_DENIED

0xC0262008

ERROR_GRAPHICS_CANNOTCOLORCONVERT

0xC0262100

ERROR_GRAPHICS_NO_VIDEO_MEMORY

0xC0262101

ERROR_GRAPHICS_CANT_LOCK_MEMORY

0xC0262102

ERROR_GRAPHICS_ALLOCATION_BUSY

0xC0262103

ERROR_GRAPHICS_TOO_MANY_REFERENCES

0xC0262104

ERROR_GRAPHICS_TRY_AGAIN_LATER

0xC0262105

ERROR_GRAPHICS_TRY_AGAIN_NOW

0xC0262106

ERROR_GRAPHICS_ALLOCATION_INVALID

0xC0262107

ERROR_GRAPHICS_UNSWIZZLING_APERTURE_UNAVAILABLE

0xC0262108

ERROR_GRAPHICS_UNSWIZZLING_APERTURE_UNSUPPORTED

0xC0262109

ERROR_GRAPHICS_CANT_EVICT_PINNED_ALLOCATION

0xC0262110

ERROR_GRAPHICS_INVALID_ALLOCATION_USAGE

0xC0262111

ERROR_GRAPHICS_CANT_RENDER_LOCKED_ALLOCATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Present happened but ended up
into the changed desktop mode.

Nothing to present due to
desktop occlusion.

Not able to present due to
denial of desktop access.

Not able to present with color
conversion.

Not enough video memory
available to complete the
operation.

Could not probe and lock the
underlying memory of an
allocation.

The allocation is currently busy.

An object being referenced has
reach the maximum reference
count already and cannot be
referenced further.

A problem could not be solved
due to some currently existing
condition. The problem should
be tried again later.

A problem could not be solved
due to some currently existing
condition. The problem should
be tried again immediately.

The allocation is invalid.

No more unswizzling apertures
are currently available.

The current allocation cannot be
unswizzled by an aperture.

The request failed because a
pinned allocation cannot be
evicted.

The allocation cannot be used
from its current segment
location for the specified
operation.

A locked allocation cannot be
used in the current command
buffer.

191 / 497


Return value/code

0xC0262112

ERROR_GRAPHICS_ALLOCATION_CLOSED

0xC0262113

ERROR_GRAPHICS_INVALID_ALLOCATION_INSTANCE

0xC0262114

ERROR_GRAPHICS_INVALID_ALLOCATION_HANDLE

0xC0262115

ERROR_GRAPHICS_WRONG_ALLOCATION_DEVICE

0xC0262116

ERROR_GRAPHICS_ALLOCATION_CONTENT_LOST

0xC0262200

ERROR_GRAPHICS_GPU_EXCEPTION_ON_DEVICE

0xC0262300

ERROR_GRAPHICS_INVALID_VIDPN_TOPOLOGY

0xC0262301

ERROR_GRAPHICS_VIDPN_TOPOLOGY_NOT_SUPPORTED

0xC0262302

ERROR_GRAPHICS_VIDPN_TOPOLOGY_CURRENTLY_NOT_SUPPORTED

0xC0262303

ERROR_GRAPHICS_INVALID_VIDPN

0xC0262304

ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE

0xC0262305

ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET

0xC0262306

ERROR_GRAPHICS_VIDPN_MODALITY_NOT_SUPPORTED

0xC0262308

ERROR_GRAPHICS_INVALID_VIDPN_SOURCEMODESET

0xC0262309

ERROR_GRAPHICS_INVALID_VIDPN_TARGETMODESET

0xC026230A

ERROR_GRAPHICS_INVALID_FREQUENCY

0xC026230B

ERROR_GRAPHICS_INVALID_ACTIVE_REGION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The allocation being referenced
has been closed permanently.

An invalid allocation instance is
being referenced.

An invalid allocation handle is
being referenced.

The allocation being referenced
does not belong to the current
device.

The specified allocation lost its
content.

Graphics processing unit (GPU)
exception is detected on the
given device. The device is not
able to be scheduled.

Specified video present network
(VidPN) topology is invalid.

Specified VidPN topology is valid
but is not supported by this
model of the display adapter.

Specified VidPN topology is valid
but is not supported by the
display adapter at this time,
due to current allocation of its
resources.

Specified VidPN handle is
invalid.

Specified video present source
is invalid.

Specified video present target is
invalid.

Specified VidPN modality is not
supported (for example, at least
two of the pinned modes are
not cofunctional).

Specified VidPN source mode
set is invalid.

Specified VidPN target mode set
is invalid.

Specified video signal frequency
is invalid.

Specified video signal active
region is invalid.

192 / 497


Return value/code

0xC026230C

ERROR_GRAPHICS_INVALID_TOTAL_REGION

0xC0262310

ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE_MODE

0xC0262311

ERROR_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET_MODE

0xC0262312

ERROR_GRAPHICS_PINNED_MODE_MUST_REMAIN_IN_SET

0xC0262313

ERROR_GRAPHICS_PATH_ALREADY_IN_TOPOLOGY

0xC0262314

ERROR_GRAPHICS_MODE_ALREADY_IN_MODESET

0xC0262315

ERROR_GRAPHICS_INVALID_VIDEOPRESENTSOURCESET

0xC0262316

ERROR_GRAPHICS_INVALID_VIDEOPRESENTTARGETSET

0xC0262317

ERROR_GRAPHICS_SOURCE_ALREADY_IN_SET

0xC0262318

ERROR_GRAPHICS_TARGET_ALREADY_IN_SET

0xC0262319

ERROR_GRAPHICS_INVALID_VIDPN_PRESENT_PATH

0xC026231A

ERROR_GRAPHICS_NO_RECOMMENDED_VIDPN_TOPOLOGY

0xC026231B

ERROR_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGESET

0xC026231C

ERROR_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGE

0xC026231D

ERROR_GRAPHICS_FREQUENCYRANGE_NOT_IN_SET

0xC026231F

ERROR_GRAPHICS_FREQUENCYRANGE_ALREADY_IN_SET

0xC0262320

ERROR_GRAPHICS_STALE_MODESET

0xC0262321

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Specified video signal total
region is invalid.

Specified video present source
mode is invalid.

Specified video present target
mode is invalid.

Pinned mode must remain in
the set on VidPN's cofunctional
modality enumeration.

Specified video present path is
already in the VidPN topology.

Specified mode is already in the
mode set.

Specified video present source
set is invalid.

Specified video present target
set is invalid.

Specified video present source
is already in the video present
source set.

Specified video present target is
already in the video present
target set.

Specified VidPN present path is
invalid.

Miniport has no
recommendation for
augmentation of the specified
VidPN topology.

Specified monitor frequency
range set is invalid.

Specified monitor frequency
range is invalid.

Specified frequency range is not
in the specified monitor
frequency range set.

Specified frequency range is
already in the specified monitor
frequency range set.

Specified mode set is stale.
Reacquire the new mode set.

Specified monitor source mode

193 / 497


Return value/code

ERROR_GRAPHICS_INVALID_MONITOR_SOURCEMODESET

0xC0262322

ERROR_GRAPHICS_INVALID_MONITOR_SOURCE_MODE

0xC0262323

ERROR_GRAPHICS_NO_RECOMMENDED_FUNCTIONAL_VIDPN

0xC0262324

ERROR_GRAPHICS_MODE_ID_MUST_BE_UNIQUE

0xC0262325

ERROR_GRAPHICS_EMPTY_ADAPTER_MONITOR_MODE_SUPPORT_INTERSECT
ION

0xC0262326

ERROR_GRAPHICS_VIDEO_PRESENT_TARGETS_LESS_THAN_SOURCES

0xC0262327

ERROR_GRAPHICS_PATH_NOT_IN_TOPOLOGY

0xC0262328

ERROR_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_SOURCE

0xC0262329

ERROR_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_TARGET

0xC026232A

ERROR_GRAPHICS_INVALID_MONITORDESCRIPTORSET

0xC026232B

ERROR_GRAPHICS_INVALID_MONITORDESCRIPTOR

Description

set is invalid.

Specified monitor source mode
is invalid.

Miniport does not have any
recommendation regarding the
request to provide a functional
VidPN given the current display
adapter configuration.

ID of the specified mode is
already used by another mode
in the set.

System failed to determine a
mode that is supported by both
the display adapter and the
monitor connected to it.

Number of video present
targets must be greater than or
equal to the number of video
present sources.

Specified present path is not in
the VidPN topology.

Display adapter must have at
least one video present source.

Display adapter must have at
least one video present target.

Specified monitor descriptor set
is invalid.

Specified monitor descriptor is
invalid.

0xC026232C

ERROR_GRAPHICS_MONITORDESCRIPTOR_NOT_IN_SET

Specified descriptor is not in the
specified monitor descriptor set.

0xC026232D

ERROR_GRAPHICS_MONITORDESCRIPTOR_ALREADY_IN_SET

0xC026232E

ERROR_GRAPHICS_MONITORDESCRIPTOR_ID_MUST_BE_UNIQUE

0xC026232F

ERROR_GRAPHICS_INVALID_VIDPN_TARGET_SUBSET_TYPE

0xC0262330

ERROR_GRAPHICS_RESOURCES_NOT_RELATED

0xC0262331

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Specified descriptor is already
in the specified monitor
descriptor set.

ID of the specified monitor
descriptor is already used by
another descriptor in the set.

Specified video present target
subset type is invalid.

Two or more of the specified
resources are not related to
each other, as defined by the
interface semantics.

ID of the specified video
present source is already used

194 / 497


Return value/code

Description

ERROR_GRAPHICS_SOURCE_ID_MUST_BE_UNIQUE

by another source in the set.

0xC0262332

ERROR_GRAPHICS_TARGET_ID_MUST_BE_UNIQUE

0xC0262333

ERROR_GRAPHICS_NO_AVAILABLE_VIDPN_TARGET

0xC0262334

ERROR_GRAPHICS_MONITOR_COULD_NOT_BE_ASSOCIATED_WITH_ADAPTER

0xC0262335

ERROR_GRAPHICS_NO_VIDPNMGR

0xC0262336

ERROR_GRAPHICS_NO_ACTIVE_VIDPN

0xC0262337

ERROR_GRAPHICS_STALE_VIDPN_TOPOLOGY

0xC0262338

ERROR_GRAPHICS_MONITOR_NOT_CONNECTED

0xC0262339

ERROR_GRAPHICS_SOURCE_NOT_IN_TOPOLOGY

0xC026233A

ERROR_GRAPHICS_INVALID_PRIMARYSURFACE_SIZE

0xC026233B

ERROR_GRAPHICS_INVALID_VISIBLEREGION_SIZE

0xC026233C

ERROR_GRAPHICS_INVALID_STRIDE

0xC026233D

ERROR_GRAPHICS_INVALID_PIXELFORMAT

0xC026233E

ERROR_GRAPHICS_INVALID_COLORBASIS

0xC026233F

ERROR_GRAPHICS_INVALID_PIXELVALUEACCESSMODE

0xC0262340

ERROR_GRAPHICS_TARGET_NOT_IN_TOPOLOGY

0xC0262341

ERROR_GRAPHICS_NO_DISPLAY_MODE_MANAGEMENT_SUPPORT

0xC0262342

ERROR_GRAPHICS_VIDPN_SOURCE_IN_USE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

ID of the specified video
present target is already used
by another target in the set.

Specified VidPN source cannot
be used because there is no
available VidPN target to
connect it to.

Newly arrived monitor could not
be associated with a display
adapter.

Display adapter in question
does not have an associated
VidPN manager.

VidPN manager of the display
adapter in question does not
have an active VidPN.

Specified VidPN topology is
stale. Re-acquire the new
topology.

There is no monitor connected
on the specified video present
target.

Specified source is not part of
the specified VidPN topology.

Specified primary surface size is
invalid.

Specified visible region size is
invalid.

Specified stride is invalid.

Specified pixel format is invalid.

Specified color basis is invalid.

Specified pixel value access
mode is invalid.

Specified target is not part of
the specified VidPN topology.

Failed to acquire display mode
management interface.

Specified VidPN source is
already owned by a display

195 / 497


Return value/code

0xC0262343

ERROR_GRAPHICS_CANT_ACCESS_ACTIVE_VIDPN

0xC0262344

ERROR_GRAPHICS_INVALID_PATH_IMPORTANCE_ORDINAL

0xC0262345

ERROR_GRAPHICS_INVALID_PATH_CONTENT_GEOMETRY_TRANSFORMATION

0xC0262346

ERROR_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATION_NOT_SU
PPORTED

0xC0262347

ERROR_GRAPHICS_INVALID_GAMMA_RAMP

0xC0262348

ERROR_GRAPHICS_GAMMA_RAMP_NOT_SUPPORTED

0xC0262349

ERROR_GRAPHICS_MULTISAMPLING_NOT_SUPPORTED

0xC026234A

ERROR_GRAPHICS_MODE_NOT_IN_MODESET

0xC026234D

ERROR_GRAPHICS_INVALID_VIDPN_TOPOLOGY_RECOMMENDATION_REASON

0xC026234E

ERROR_GRAPHICS_INVALID_PATH_CONTENT_TYPE

0xC026234F

ERROR_GRAPHICS_INVALID_COPYPROTECTION_TYPE

0xC0262350

ERROR_GRAPHICS_UNASSIGNED_MODESET_ALREADY_EXISTS

0xC0262352

ERROR_GRAPHICS_INVALID_SCANLINE_ORDERING

0xC0262353

ERROR_GRAPHICS_TOPOLOGY_CHANGES_NOT_ALLOWED

0xC0262354

ERROR_GRAPHICS_NO_AVAILABLE_IMPORTANCE_ORDINALS

0xC0262355

ERROR_GRAPHICS_INCOMPATIBLE_PRIVATE_FORMAT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

mode manager (DMM) client
and cannot be used until that
client releases it.

Specified VidPN is active and
cannot be accessed.

Specified VidPN present path
importance ordinal is invalid.

Specified VidPN present path
content geometry
transformation is invalid.

Specified content geometry
transformation is not supported
on the respective VidPN present
path.

Specified gamma ramp is
invalid.

Specified gamma ramp is not
supported on the respective
VidPN present path.

Multisampling is not supported
on the respective VidPN present
path.

Specified mode is not in the
specified mode set.

Specified VidPN topology
recommendation reason is
invalid.

Specified VidPN present path
content type is invalid.

Specified VidPN present path
copy protection type is invalid.

No more than one unassigned
mode set can exist at any given
time for a given VidPN source or
target.

The specified scan line ordering
type is invalid.

Topology changes are not
allowed for the specified VidPN.

All available importance ordinals
are already used in the
specified topology.

Specified primary surface has a
different private format
attribute than the current

196 / 497


Return value/code

0xC0262356

ERROR_GRAPHICS_INVALID_MODE_PRUNING_ALGORITHM

0xC0262400

ERROR_GRAPHICS_SPECIFIED_CHILD_ALREADY_CONNECTED

0xC0262401

ERROR_GRAPHICS_CHILD_DESCRIPTOR_NOT_SUPPORTED

0xC0262430

ERROR_GRAPHICS_NOT_A_LINKED_ADAPTER

0xC0262431

ERROR_GRAPHICS_LEADLINK_NOT_ENUMERATED

0xC0262432

ERROR_GRAPHICS_CHAINLINKS_NOT_ENUMERATED

0xC0262433

ERROR_GRAPHICS_ADAPTER_CHAIN_NOT_READY

0xC0262434

ERROR_GRAPHICS_CHAINLINKS_NOT_STARTED

0xC0262435

ERROR_GRAPHICS_CHAINLINKS_NOT_POWERED_ON

0xC0262436

ERROR_GRAPHICS_INCONSISTENT_DEVICE_LINK_STATE

0xC0262438

ERROR_GRAPHICS_NOT_POST_DEVICE_DRIVER

0xC0262500

ERROR_GRAPHICS_OPM_NOT_SUPPORTED

0xC0262501

ERROR_GRAPHICS_COPP_NOT_SUPPORTED

0xC0262502

ERROR_GRAPHICS_UAB_NOT_SUPPORTED

0xC0262503

ERROR_GRAPHICS_OPM_INVALID_ENCRYPTED_PARAMETERS

0xC0262504

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

primary surface.

Specified mode pruning
algorithm is invalid.

Specified display adapter child
device already has an external
device connected to it.

The display adapter child device
does not support reporting a
descriptor.

The display adapter is not
linked to any other adapters.

Lead adapter in a linked
configuration was not
enumerated yet.

Some chain adapters in a linked
configuration were not
enumerated yet.

The chain of linked adapters is
not ready to start because of an
unknown failure.

An attempt was made to start a
lead link display adapter when
the chain links were not started
yet.

An attempt was made to turn
on a lead link display adapter
when the chain links were
turned off.

The adapter link was found to
be in an inconsistent state. Not
all adapters are in an expected
PNP or power state.

The driver trying to start is not
the same as the driver for the
posted display adapter.

The driver does not support
Output Protection Manager
(OPM).

The driver does not support
Certified Output Protection
Protocol (COPP).

The driver does not support a
user-accessible bus (UAB).

The specified encrypted
parameters are invalid.

An array passed to a function

197 / 497


Return value/code

ERROR_GRAPHICS_OPM_PARAMETER_ARRAY_TOO_SMALL

0xC0262505

ERROR_GRAPHICS_OPM_NO_VIDEO_OUTPUTS_EXIST

0xC0262506

ERROR_GRAPHICS_PVP_NO_DISPLAY_DEVICE_CORRESPONDS_TO_NAME

0xC0262507

ERROR_GRAPHICS_PVP_DISPLAY_DEVICE_NOT_ATTACHED_TO_DESKTOP

0xC0262508

ERROR_GRAPHICS_PVP_MIRRORING_DEVICES_NOT_SUPPORTED

0xC026250A

ERROR_GRAPHICS_OPM_INVALID_POINTER

0xC026250B

ERROR_GRAPHICS_OPM_INTERNAL_ERROR

0xC026250C

ERROR_GRAPHICS_OPM_INVALID_HANDLE

0xC026250D

ERROR_GRAPHICS_PVP_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE

0xC026250E

ERROR_GRAPHICS_PVP_INVALID_CERTIFICATE_LENGTH

0xC026250F

ERROR_GRAPHICS_OPM_SPANNING_MODE_ENABLED

0xC0262510

ERROR_GRAPHICS_OPM_THEATER_MODE_ENABLED

0xC0262511

ERROR_GRAPHICS_PVP_HFS_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

cannot hold all of the data that
the function wants to put in it.

The GDI display device passed
to this function does not have
any active video outputs.

The protected video path (PVP)
cannot find an actual GDI
display device that corresponds
to the passed-in GDI display
device name.

This function failed because the
GDI display device passed to it
was not attached to the
Windows desktop.

The PVP does not support
mirroring display devices
because they do not have video
outputs.

The function failed because an
invalid pointer parameter was
passed to it. A pointer
parameter is invalid if it is null,
it points to an invalid address, it
points to a kernel mode
address, or it is not correctly
aligned.

An internal error caused this
operation to fail.

The function failed because the
caller passed in an invalid OPM
user mode handle.

This function failed because the
GDI device passed to it did not
have any monitors associated
with it.

A certificate could not be
returned because the certificate
buffer passed to the function
was too small.

A video output could not be
created because the frame
buffer is in spanning mode.

A video output could not be
created because the frame
buffer is in theater mode.

The function call failed because
the display adapter's hardware
functionality scan failed to
validate the graphics hardware.

198 / 497


Return value/code

0xC0262512

ERROR_GRAPHICS_OPM_INVALID_SRM

0xC0262513

ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_HDCP

0xC0262514

ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_ACP

0xC0262515

ERROR_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_CGMSA

0xC0262516

ERROR_GRAPHICS_OPM_HDCP_SRM_NEVER_SET

0xC0262517

ERROR_GRAPHICS_OPM_RESOLUTION_TOO_HIGH

0xC0262518

ERROR_GRAPHICS_OPM_ALL_HDCP_HARDWARE_ALREADY_IN_USE

0xC0262519

ERROR_GRAPHICS_OPM_VIDEO_OUTPUT_NO_LONGER_EXISTS

0xC026251A

ERROR_GRAPHICS_OPM_SESSION_TYPE_CHANGE_IN_PROGRESS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The High-Bandwidth Digital
Content Protection (HDCP)
System Renewability Message
(SRM) passed to this function
did not comply with section 5 of
the HDCP 1.1 specification.

The video output cannot enable
the HDCP system because it
does not support it.

The video output cannot enable
analog copy protection because
it does not support it.

The video output cannot enable
the Content Generation
Management System Analog
(CGMS-A) protection technology
because it does not support it.

IOPMVideoOutput's
GetInformation() method
cannot return the version of the
SRM being used because the
application never successfully
passed an SRM to the video
output.

IOPMVideoOutput's Configure()
method cannot enable the
specified output protection
technology because the output's
screen resolution is too high.

IOPMVideoOutput's Configure()
method cannot enable HDCP
because the display adapter's
HDCP hardware is already being
used by other physical outputs.

The operating system
asynchronously destroyed this
OPM video output because the
operating system's state
changed. This error typically
occurs because the monitor
physical device object (PDO)
associated with this video
output was removed, the
monitor PDO associated with
this video output was stopped,
the video output's session
became a nonconsole session or
the video output's desktop
became an inactive desktop.

IOPMVideoOutput's methods
cannot be called when a session
is changing its type. There are
currently three types of
sessions: console, disconnected
and remote (remote desktop

199 / 497


Return value/code

0xC0262580

ERROR_GRAPHICS_I2C_NOT_SUPPORTED

0xC0262581

ERROR_GRAPHICS_I2C_DEVICE_DOES_NOT_EXIST

0xC0262582

ERROR_GRAPHICS_I2C_ERROR_TRANSMITTING_DATA

0xC0262583

ERROR_GRAPHICS_I2C_ERROR_RECEIVING_DATA

0xC0262584

ERROR_GRAPHICS_DDCCI_VCP_NOT_SUPPORTED

0xC0262585

ERROR_GRAPHICS_DDCCI_INVALID_DATA

0xC0262586

ERROR_GRAPHICS_DDCCI_MONITOR_RETURNED_INVALID_TIMING_STATUS_
BYTE

0xC0262587

ERROR_GRAPHICS_MCA_INVALID_CAPABILITIES_STRING

0xC0262588

ERROR_GRAPHICS_MCA_INTERNAL_ERROR

0xC0262589

ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_COMMAND

0xC026258A

ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_LENGTH

0xC026258B

ERROR_GRAPHICS_DDCCI_INVALID_MESSAGE_CHECKSUM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

protocol [RDP] or Independent
Computing Architecture [ICA]).

The monitor connected to the
specified video output does not
have an I2C bus.

No device on the I2C bus has
the specified address.

An error occurred while
transmitting data to the device
on the I2C bus.

An error occurred while
receiving data from the device
on the I2C bus.

The monitor does not support
the specified Virtual Control
Panel (VCP) code.

The data received from the
monitor is invalid.

A function call failed because a
monitor returned an invalid
Timing Status byte when the
operating system used the
Display Data Channel Command
Interface (DDC/CI) Get Timing
Report and Timing Message
command to get a timing report
from a monitor.

The monitor returned a DDC/CI
capabilities string that did not
comply with the ACCESS.bus
3.0, DDC/CI 1.1 or MCCS 2
Revision 1 specification.

An internal Monitor
Configuration API error
occurred.

An operation failed because a
DDC/CI message had an invalid
value in its command field.

This error occurred because a
DDC/CI message length field
contained an invalid value.

This error occurred because the
value in a DDC/CI message
checksum field did not match
the message's computed
checksum value. This error
implies that the data was
corrupted while it was being
transmitted from a monitor to a
computer.

200 / 497


Return value/code

0xC02625D6

ERROR_GRAPHICS_PMEA_INVALID_MONITOR

0xC02625D7

ERROR_GRAPHICS_PMEA_INVALID_D3D_DEVICE

0xC02625D8

ERROR_GRAPHICS_DDCCI_CURRENT_CURRENT_VALUE_GREATER_THAN_MAX
IMUM_VALUE

0xC02625D9

ERROR_GRAPHICS_MCA_INVALID_VCP_VERSION

0xC02625DA

ERROR_GRAPHICS_MCA_MONITOR_VIOLATES_MCCS_SPECIFICATION

0xC02625DB

ERROR_GRAPHICS_MCA_MCCS_VERSION_MISMATCH

0xC02625DC

ERROR_GRAPHICS_MCA_UNSUPPORTED_MCCS_VERSION

0xC02625DE

ERROR_GRAPHICS_MCA_INVALID_TECHNOLOGY_TYPE_RETURNED

0xC02625DF

ERROR_GRAPHICS_MCA_UNSUPPORTED_COLOR_TEMPERATURE

Description

The HMONITOR no longer
exists, is not attached to the
desktop, or corresponds to a
mirroring device.

The Direct3D (D3D) device's
GDI display device no longer
exists, is not attached to the
desktop, or is a mirroring
display device.

A continuous VCP code's current
value is greater than its
maximum value. This error code
indicates that a monitor
returned an invalid value.

The monitor's VCP Version
(0xDF) VCP code returned an
invalid version value.

The monitor does not comply
with the Monitor Control
Command Set (MCCS)
specification it claims to
support.

The MCCS version in a
monitor's mccs_ver capability
does not match the MCCS
version the monitor reports
when the VCP Version (0xDF)
VCP code is used.

The Monitor Configuration API
only works with monitors that
support the MCCS 1.0
specification, the MCCS 2.0
specification, or the MCCS 2.0
Revision 1 specification.

The monitor returned an invalid
monitor technology type. CRT,
plasma, and LCD (TFT) are
examples of monitor technology
types. This error implies that
the monitor violated the MCCS
2.0 or MCCS 2.0 Revision 1
specification.

The
SetMonitorColorTemperature()
caller passed a color
temperature to it that the
current monitor did not support.
CRT, plasma, and LCD (TFT) are
examples of monitor technology
types. This error implies that
the monitor violated the MCCS
2.0 or MCCS 2.0 Revision 1
specification.

201 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC02625E0

ERROR_GRAPHICS_ONLY_CONSOLE_SESSION_SUPPORTED

Description

This function can be used only if
a program is running in the
local console session. It cannot
be used if the program is
running on a remote desktop
session or on a terminal
server session.

#### 2.1.2 HRESULT From WIN32 Error Code Macro

The HRESULT From WIN32 Error Code Macro converts a Win32 error code to an HRESULT using the
pattern 0x8007XXXX, where XXXX is the first two bytes of the Win32 hex value 0x0000XXXX.

The macro is as follows:

 #define FACILITY_WIN32 0x0007

 #define __HRESULT_FROM_WIN32(x) ((HRESULT)(x) <= 0 ? ((HRESULT)(x)) : ((HRESULT) (((x) &
0x0000FFFF) | (FACILITY_WIN32 << 16) | 0x80000000)))

### 2.2 Win32 Error Codes

All Win32 error codes MUST be in the range 0x0000 to 0xFFFF, although Win32 error codes can be
used both in 16-bit fields (such as within the HRESULT type specified in section 2.1) as well as 32-bit
fields. Most values also have a default message defined, which can be used to map the value to a
human-readable text message; when this is done, the Win32 error code is also known as a message
identifier.

The following table specifies the values and corresponding meanings of the Win32 error codes.
Vendors SHOULD NOT assign other meanings to these values, to avoid the risk of a collision in the
future.

This document provides the common usage details of the Win32 error codes; individual protocol
specifications provide expanded or modified definitions.

Note  In the following descriptions, a percentage sign followed by one or more alphanumeric
characters (for example, "%1" or "%hs") indicates a variable that will be replaced by text at the time
the value is returned.

Win32 error codes

0x00000000

ERROR_SUCCESS

0x00000000

NERR_Success

0x00000001

ERROR_INVALID_FUNCTION

0x00000002

ERROR_FILE_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The operation completed
successfully.

The operation completed
successfully.

Incorrect function.

The system cannot find the file
specified.

202 / 497


Win32 error codes

0x00000003

ERROR_PATH_NOT_FOUND

0x00000004

ERROR_TOO_MANY_OPEN_FILES

0x00000005

ERROR_ACCESS_DENIED

0x00000006

ERROR_INVALID_HANDLE

0x00000007

ERROR_ARENA_TRASHED

0x00000008

ERROR_NOT_ENOUGH_MEMORY

0x00000009

ERROR_INVALID_BLOCK

0x0000000A

ERROR_BAD_ENVIRONMENT

0x0000000B

ERROR_BAD_FORMAT

0x0000000C

ERROR_INVALID_ACCESS

0x0000000D

ERROR_INVALID_DATA

0x0000000E

ERROR_OUTOFMEMORY

0x0000000F

ERROR_INVALID_DRIVE

0x00000010

ERROR_CURRENT_DIRECTORY

0x00000011

ERROR_NOT_SAME_DEVICE

0x00000012

ERROR_NO_MORE_FILES

0x00000013

ERROR_WRITE_PROTECT

0x00000014

ERROR_BAD_UNIT

0x00000015

ERROR_NOT_READY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system cannot find the path
specified.

The system cannot open the file.

Access is denied.

The handle is invalid.

The storage control blocks were
destroyed.

Not enough storage is available
to process this command.

The storage control block
address is invalid.

The environment is incorrect.

An attempt was made to load a
program with an incorrect
format.

The access code is invalid.

The data is invalid.

Not enough storage is available
to complete this operation.

The system cannot find the drive
specified.

The directory cannot be
removed.

The system cannot move the file
to a different disk drive.

There are no more files.

The media is write-protected.

The system cannot find the
device specified.

The device is not ready.

203 / 497


Win32 error codes

0x00000016

ERROR_BAD_COMMAND

0x00000017

ERROR_CRC

0x00000018

ERROR_BAD_LENGTH

0x00000019

ERROR_SEEK

0x0000001A

ERROR_NOT_DOS_DISK

0x0000001B

ERROR_SECTOR_NOT_FOUND

0x0000001C

ERROR_OUT_OF_PAPER

0x0000001D

ERROR_WRITE_FAULT

0x0000001E

ERROR_READ_FAULT

0x0000001F

ERROR_GEN_FAILURE

0x00000020

ERROR_SHARING_VIOLATION

0x00000021

ERROR_LOCK_VIOLATION

0x00000022

ERROR_WRONG_DISK

0x00000024

ERROR_SHARING_BUFFER_EXCEEDED

0x00000026

ERROR_HANDLE_EOF

0x00000027

ERROR_HANDLE_DISK_FULL

0x00000032

ERROR_NOT_SUPPORTED

0x00000033

ERROR_REM_NOT_LIST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The device does not recognize
the command.

Data error (cyclic redundancy
check).

The program issued a command
but the command length is
incorrect.

The drive cannot locate a specific
area or track on the disk.

The specified disk cannot be
accessed.

The drive cannot find the sector
requested.

The printer is out of paper.

The system cannot write to the
specified device.

The system cannot read from the
specified device.

A device attached to the system
is not functioning.

The process cannot access the
file because it is being used by
another process.

The process cannot access the
file because another process has
locked a portion of the file.

The wrong disk is in the drive.
Insert %2 (Volume Serial
Number: %3) into drive %1.

Too many files opened for
sharing.

Reached the end of the file.

The disk is full.

The request is not supported.

Windows cannot find the network
path. Verify that the network
path is correct and the
destination computer is not busy

204 / 497


Win32 error codes

0x00000034

ERROR_DUP_NAME

0x00000035

ERROR_BAD_NETPATH

0x00000036

ERROR_NETWORK_BUSY

0x00000037

ERROR_DEV_NOT_EXIST

0x00000038

ERROR_TOO_MANY_CMDS

0x00000039

ERROR_ADAP_HDW_ERR

0x0000003A

ERROR_BAD_NET_RESP

0x0000003B

ERROR_UNEXP_NET_ERR

0x0000003C

ERROR_BAD_REM_ADAP

0x0000003D

ERROR_PRINTQ_FULL

0x0000003E

ERROR_NO_SPOOL_SPACE

0x0000003F

ERROR_PRINT_CANCELLED

0x00000040

ERROR_NETNAME_DELETED

0x00000041

ERROR_NETWORK_ACCESS_DENIED

0x00000042

ERROR_BAD_DEV_TYPE

0x00000043

ERROR_BAD_NET_NAME

0x00000044

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

or turned off. If Windows still
cannot find the network path,
contact your network
administrator.

You were not connected because
a duplicate name exists on the
network. Go to System in
Control Panel to change the
computer name, and then try
again.

The network path was not found.

The network is busy.

The specified network resource
or device is no longer available.

The network BIOS command
limit has been reached.

A network adapter hardware
error occurred.

The specified server cannot
perform the requested operation.

An unexpected network error
occurred.

The remote adapter is not
compatible.

The print queue is full.

Space to store the file waiting to
be printed is not available on the
server.

Your file waiting to be printed
was deleted.

The specified network name is
no longer available.

Network access is denied.

The network resource type is not
correct.

The network name cannot be
found.

The name limit for the local

205 / 497


Win32 error codes

ERROR_TOO_MANY_NAMES

0x00000045

ERROR_TOO_MANY_SESS

0x00000046

ERROR_SHARING_PAUSED

0x00000047

ERROR_REQ_NOT_ACCEP

0x00000048

ERROR_REDIR_PAUSED

0x00000050

ERROR_FILE_EXISTS

0x00000052

ERROR_CANNOT_MAKE

0x00000053

ERROR_FAIL_I24

0x00000054

ERROR_OUT_OF_STRUCTURES

0x00000055

ERROR_ALREADY_ASSIGNED

0x00000056

ERROR_INVALID_PASSWORD

0x00000057

ERROR_INVALID_PARAMETER

0x00000058

ERROR_NET_WRITE_FAULT

0x00000059

ERROR_NO_PROC_SLOTS

0x00000064

ERROR_TOO_MANY_SEMAPHORES

0x00000065

ERROR_EXCL_SEM_ALREADY_OWNED

0x00000066

ERROR_SEM_IS_SET

0x00000067

ERROR_TOO_MANY_SEM_REQUESTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

computer network adapter card
was exceeded.

The network BIOS session limit
was exceeded.

The remote server has been
paused or is in the process of
being started.

No more connections can be
made to this remote computer at
this time because the computer
has accepted the maximum
number of connections.

The specified printer or disk
device has been paused.

The file exists.

The directory or file cannot be
created.

Fail on INT 24.

Storage to process this request
is not available.

The local device name is already
in use.

The specified network password
is not correct.

The parameter is incorrect.

A write fault occurred on the
network.

The system cannot start another
process at this time.

Cannot create another system
semaphore.

The exclusive semaphore is
owned by another process.

The semaphore is set and cannot
be closed.

The semaphore cannot be set
again.

206 / 497


Win32 error codes

0x00000068

ERROR_INVALID_AT_INTERRUPT_TIME

0x00000069

ERROR_SEM_OWNER_DIED

0x0000006A

ERROR_SEM_USER_LIMIT

0x0000006B

ERROR_DISK_CHANGE

0x0000006C

ERROR_DRIVE_LOCKED

0x0000006D

ERROR_BROKEN_PIPE

0x0000006E

ERROR_OPEN_FAILED

0x0000006F

ERROR_BUFFER_OVERFLOW

0x00000070

ERROR_DISK_FULL

0x00000071

ERROR_NO_MORE_SEARCH_HANDLES

0x00000072

ERROR_INVALID_TARGET_HANDLE

0x00000075

ERROR_INVALID_CATEGORY

0x00000076

ERROR_INVALID_VERIFY_SWITCH

0x00000077

ERROR_BAD_DRIVER_LEVEL

0x00000078

ERROR_CALL_NOT_IMPLEMENTED

0x00000079

ERROR_SEM_TIMEOUT

0x0000007A

ERROR_INSUFFICIENT_BUFFER

0x0000007B

ERROR_INVALID_NAME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Cannot request exclusive
semaphores at interrupt time.

The previous ownership of this
semaphore has ended.

Insert the disk for drive %1.

The program stopped because
an alternate disk was not
inserted.

The disk is in use or locked by
another process.

The pipe has been ended.

The system cannot open the
device or file specified.

The file name is too long.

There is not enough space on
the disk.

No more internal file identifiers
are available.

The target internal file identifier
is incorrect.

The Input Output Control
(IOCTL) call made by the
application program is not
correct.

The verify-on-write switch
parameter value is not correct.

The system does not support the
command requested.

This function is not supported on
this system.

The semaphore time-out period
has expired.

The data area passed to a
system call is too small.

The file name, directory name,
or volume label syntax is
incorrect.

207 / 497


Win32 error codes

0x0000007C

ERROR_INVALID_LEVEL

0x0000007D

ERROR_NO_VOLUME_LABEL

0x0000007E

ERROR_MOD_NOT_FOUND

0x0000007F

ERROR_PROC_NOT_FOUND

0x00000080

ERROR_WAIT_NO_CHILDREN

0x00000081

ERROR_CHILD_NOT_COMPLETE

0x00000082

ERROR_DIRECT_ACCESS_HANDLE

0x00000083

ERROR_NEGATIVE_SEEK

0x00000084

ERROR_SEEK_ON_DEVICE

0x00000085

ERROR_IS_JOIN_TARGET

0x00000086

ERROR_IS_JOINED

0x00000087

ERROR_IS_SUBSTED

0x00000088

ERROR_NOT_JOINED

0x00000089

ERROR_NOT_SUBSTED

0x0000008A

ERROR_JOIN_TO_JOIN

0x0000008B

ERROR_SUBST_TO_SUBST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system call level is not
correct.

The disk has no volume label.

The specified module could not
be found.

The specified procedure could
not be found.

There are no child processes to
wait for.

The %1 application cannot be
run in Win32 mode.

Attempt to use a file handle to
an open disk partition for an
operation other than raw disk
I/O.

An attempt was made to move
the file pointer before the
beginning of the file.

The file pointer cannot be set on
the specified device or file.

A JOIN or SUBST command
cannot be used for a drive that
contains previously joined
drives.

An attempt was made to use a
JOIN or SUBST command on a
drive that has already been
joined.

An attempt was made to use a
JOIN or SUBST command on a
drive that has already been
substituted.

The system tried to delete the
JOIN of a drive that is not
joined.

The system tried to delete the
substitution of a drive that is not
substituted.

The system tried to join a drive
to a directory on a joined drive.

The system tried to substitute a
drive to a directory on a
substituted drive.

208 / 497


Win32 error codes

0x0000008C

ERROR_JOIN_TO_SUBST

0x0000008D

ERROR_SUBST_TO_JOIN

0x0000008E

ERROR_BUSY_DRIVE

0x0000008F

ERROR_SAME_DRIVE

0x00000090

ERROR_DIR_NOT_ROOT

0x00000091

ERROR_DIR_NOT_EMPTY

0x00000092

ERROR_IS_SUBST_PATH

0x00000093

ERROR_IS_JOIN_PATH

0x00000094

ERROR_PATH_BUSY

0x00000095

ERROR_IS_SUBST_TARGET

0x00000096

ERROR_SYSTEM_TRACE

0x00000097

ERROR_INVALID_EVENT_COUNT

0x00000098

ERROR_TOO_MANY_MUXWAITERS

0x00000099

ERROR_INVALID_LIST_FORMAT

0x0000009A

ERROR_LABEL_TOO_LONG

0x0000009B

ERROR_TOO_MANY_TCBS

0x0000009C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The system tried to join a drive
to a directory on a substituted
drive.

The system tried to SUBST a
drive to a directory on a joined
drive.

The system cannot perform a
JOIN or SUBST at this time.

The system cannot join or
substitute a drive to or for a
directory on the same drive.

The directory is not a
subdirectory of the root
directory.

The directory is not empty.

The path specified is being used
in a substitute.

Not enough resources are
available to process this
command.

The path specified cannot be
used at this time.

An attempt was made to join or
substitute a drive for which a
directory on the drive is the
target of a previous substitute.

System trace information was
not specified in your
CONFIG.SYS file, or tracing is
disallowed.

The number of specified
semaphore events for
DosMuxSemWait is not correct.

DosMuxSemWait did not
execute; too many semaphores
are already set.

The DosMuxSemWait list is not
correct.

The volume label you entered
exceeds the label character limit
of the destination file system.

Cannot create another thread.

The recipient process has

209 / 497


Win32 error codes

ERROR_SIGNAL_REFUSED

0x0000009D

ERROR_DISCARDED

0x0000009E

ERROR_NOT_LOCKED

0x0000009F

ERROR_BAD_THREADID_ADDR

0x000000A0

ERROR_BAD_ARGUMENTS

0x000000A1

ERROR_BAD_PATHNAME

0x000000A2

ERROR_SIGNAL_PENDING

0x000000A4

ERROR_MAX_THRDS_REACHED

0x000000A7

ERROR_LOCK_FAILED

0x000000AA

ERROR_BUSY

0x000000AD

ERROR_CANCEL_VIOLATION

0x000000AE

ERROR_ATOMIC_LOCKS_NOT_SUPPORTED

0x000000B4

ERROR_INVALID_SEGMENT_NUMBER

0x000000B6

ERROR_INVALID_ORDINAL

0x000000B7

ERROR_ALREADY_EXISTS

0x000000BA

ERROR_INVALID_FLAG_NUMBER

0x000000BB

ERROR_SEM_NOT_FOUND

0x000000BC

ERROR_INVALID_STARTING_CODESEG

0x000000BD

ERROR_INVALID_STACKSEG

0x000000BE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

refused the signal.

The segment is already
discarded and cannot be locked.

The segment is already
unlocked.

The address for the thread ID is
not correct.

One or more arguments are not
correct.

The specified path is invalid.

A signal is already pending.

No more threads can be created
in the system.

Unable to lock a region of a file.

The requested resource is in use.

A lock request was not
outstanding for the supplied
cancel region.

The file system does not support
atomic changes to the lock type.

The system detected a segment
number that was not correct.

The operating system cannot run
%1.

Cannot create a file when that
file already exists.

The flag passed is not correct.

The specified system semaphore
name was not found.

The operating system cannot run
%1.

The operating system cannot run
%1.

The operating system cannot run

210 / 497


Win32 error codes

ERROR_INVALID_MODULETYPE

0x000000BF

ERROR_INVALID_EXE_SIGNATURE

0x000000C0

ERROR_EXE_MARKED_INVALID

0x000000C1

ERROR_BAD_EXE_FORMAT

0x000000C2

ERROR_ITERATED_DATA_EXCEEDS_64k

0x000000C3

ERROR_INVALID_MINALLOCSIZE

0x000000C4

ERROR_DYNLINK_FROM_INVALID_RING

0x000000C5

ERROR_IOPL_NOT_ENABLED

0x000000C6

ERROR_INVALID_SEGDPL

0x000000C7

ERROR_AUTODATASEG_EXCEEDS_64k

0x000000C8

ERROR_RING2SEG_MUST_BE_MOVABLE

0x000000C9

ERROR_RELOC_CHAIN_XEEDS_SEGLIM

0x000000CA

ERROR_INFLOOP_IN_RELOC_CHAIN

0x000000CB

ERROR_ENVVAR_NOT_FOUND

0x000000CD

ERROR_NO_SIGNAL_SENT

0x000000CE

ERROR_FILENAME_EXCED_RANGE

0x000000CF

ERROR_RING2_STACK_IN_USE

0x000000D0

ERROR_META_EXPANSION_TOO_LONG

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

%1.

Cannot run %1 in Win32 mode.

The operating system cannot run
%1.

%1 is not a valid Win32
application.

The operating system cannot run
%1.

The operating system cannot run
%1.

The operating system cannot run
this application program.

The operating system is not
presently configured to run this
application.

The operating system cannot run
%1.

The operating system cannot run
this application program.

The code segment cannot be
greater than or equal to 64 KB.

The operating system cannot run
%1.

The operating system cannot run
%1.

The system could not find the
environment option that was
entered.

No process in the command
subtree has a signal handler.

The file name or extension is too
long.

The ring 2 stack is in use.

The asterisk (*) or question
mark (?) global file name
characters are entered
incorrectly, or too many global
file name characters are
specified.

211 / 497


Win32 error codes

0x000000D1

ERROR_INVALID_SIGNAL_NUMBER

0x000000D2

ERROR_THREAD_1_INACTIVE

0x000000D4

ERROR_LOCKED

0x000000D6

ERROR_TOO_MANY_MODULES

0x000000D7

ERROR_NESTING_NOT_ALLOWED

0x000000D8

ERROR_EXE_MACHINE_TYPE_MISMATCH

0x000000D9

ERROR_EXE_CANNOT_MODIFY_SIGNED_BINARY

0x000000DA

ERROR_EXE_CANNOT_MODIFY_STRONG_SIGNED_BINARY

0x000000DC

ERROR_FILE_CHECKED_OUT

0x000000DD

ERROR_CHECKOUT_REQUIRED

0x000000DE

ERROR_BAD_FILE_TYPE

0x000000DF

ERROR_FILE_TOO_LARGE

0x000000E0

ERROR_FORMS_AUTH_REQUIRED

0x000000E1

ERROR_VIRUS_INFECTED

0x000000E2

ERROR_VIRUS_DELETED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The signal being posted is not
correct.

The signal handler cannot be set.

The segment is locked and
cannot be reallocated.

Too many dynamic-link modules
are attached to this program or
dynamic-link module.

Cannot nest calls to LoadModule.

This version of %1 is not
compatible with the version of
Windows you're running. Check
your computer's system
information to see whether you
need an x86 (32-bit) or x64 (64-
bit) version of the program, and
then contact the software
publisher.

The image file %1 is signed,
unable to modify.

The image file %1 is strong
signed, unable to modify.

This file is checked out or locked
for editing by another user.

The file must be checked out
before saving changes.

The file type being saved or
retrieved has been blocked.

The file size exceeds the limit
allowed and cannot be saved.

Access denied. Before opening
files in this location, you must
first browse to the website and
select the option to sign in
automatically.

Operation did not complete
successfully because the file
contains a virus.

This file contains a virus and
cannot be opened. Due to the
nature of this virus, the file has
been removed from this location.

212 / 497


Win32 error codes

0x000000E5

ERROR_PIPE_LOCAL

0x000000E6

ERROR_BAD_PIPE

0x000000E7

ERROR_PIPE_BUSY

0x000000E8

ERROR_NO_DATA

0x000000E9

ERROR_PIPE_NOT_CONNECTED

0x000000EA

ERROR_MORE_DATA

0x000000F0

ERROR_VC_DISCONNECTED

0x000000FE

ERROR_INVALID_EA_NAME

0x000000FF

ERROR_EA_LIST_INCONSISTENT

0x00000102

WAIT_TIMEOUT

0x00000103

ERROR_NO_MORE_ITEMS

0x0000010A

ERROR_CANNOT_COPY

0x0000010B

ERROR_DIRECTORY

0x00000113

ERROR_EAS_DIDNT_FIT

0x00000114

ERROR_EA_FILE_CORRUPT

0x00000115

ERROR_EA_TABLE_FULL

0x00000116

ERROR_INVALID_EA_HANDLE

0x0000011A

ERROR_EAS_NOT_SUPPORTED

0x00000120

ERROR_NOT_OWNER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The pipe is local.

The pipe state is invalid.

All pipe instances are busy.

The pipe is being closed.

No process is on the other end of
the pipe.

More data is available.

The session was canceled.

The specified extended attribute
name was invalid.

The extended attributes are
inconsistent.

The wait operation timed out.

No more data is available.

The copy functions cannot be
used.

The directory name is invalid.

The extended attributes did not
fit in the buffer.

The extended attribute file on
the mounted file system is
corrupt.

The extended attribute table file
is full.

The specified extended attribute
handle is invalid.

The mounted file system does
not support extended attributes.

Attempt to release mutex not
owned by caller.

213 / 497


Win32 error codes

0x0000012A

ERROR_TOO_MANY_POSTS

0x0000012B

ERROR_PARTIAL_COPY

0x0000012C

ERROR_OPLOCK_NOT_GRANTED

0x0000012D

ERROR_INVALID_OPLOCK_PROTOCOL

0x0000012E

ERROR_DISK_TOO_FRAGMENTED

0x0000012F

ERROR_DELETE_PENDING

0x0000013D

ERROR_MR_MID_NOT_FOUND

0x0000013E

ERROR_SCOPE_NOT_FOUND

0x0000015E

ERROR_FAIL_NOACTION_REBOOT

0x0000015F

ERROR_FAIL_SHUTDOWN

0x00000160

ERROR_FAIL_RESTART

0x00000161

ERROR_MAX_SESSIONS_REACHED

0x00000190

ERROR_THREAD_MODE_ALREADY_BACKGROUND

0x00000191

ERROR_THREAD_MODE_NOT_BACKGROUND

0x00000192

ERROR_PROCESS_MODE_ALREADY_BACKGROUND

0x00000193

ERROR_PROCESS_MODE_NOT_BACKGROUND

0x000001E7

ERROR_INVALID_ADDRESS

0x000001F4

ERROR_USER_PROFILE_LOAD

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Too many posts were made to a
semaphore.

Only part of a
ReadProcessMemory or
WriteProcessMemory request
was completed.

The oplock request is denied.

An invalid oplock
acknowledgment was received
by the system.

The volume is too fragmented to
complete this operation.

The file cannot be opened
because it is in the process of
being deleted.

The system cannot find message
text for message number 0x%1
in the message file for %2.

The scope specified was not
found.

No action was taken because a
system reboot is required.

The shutdown operation failed.

The restart operation failed.

The maximum number of
sessions has been reached.

The thread is already in
background processing mode.

The thread is not in background
processing mode.

The process is already in
background processing mode.

The process is not in background
processing mode.

Attempt to access invalid
address.

User profile cannot be loaded.

214 / 497


Win32 error codes

0x00000216

ERROR_ARITHMETIC_OVERFLOW

0x00000217

ERROR_PIPE_CONNECTED

0x00000218

ERROR_PIPE_LISTENING

0x00000219

ERROR_VERIFIER_STOP

0x0000021A

ERROR_ABIOS_ERROR

0x0000021B

ERROR_WX86_WARNING

0x0000021C

ERROR_WX86_ERROR

0x0000021D

ERROR_TIMER_NOT_CANCELED

0x0000021E

ERROR_UNWIND

0x0000021F

ERROR_BAD_STACK

0x00000220

ERROR_INVALID_UNWIND_TARGET

0x00000221

ERROR_INVALID_PORT_ATTRIBUTES

0x00000222

ERROR_PORT_MESSAGE_TOO_LONG

0x00000223

ERROR_INVALID_QUOTA_LOWER

0x00000224

ERROR_DEVICE_ALREADY_ATTACHED

0x00000225

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Arithmetic result exceeded 32
bits.

There is a process on the other
end of the pipe.

Waiting for a process to open the
other end of the pipe.

Application verifier has found an
error in the current process.

An error occurred in the ABIOS
subsystem.

A warning occurred in the WX86
subsystem.

An error occurred in the WX86
subsystem.

An attempt was made to cancel
or set a timer that has an
associated asynchronous
procedure call (APC) and the
subject thread is not the thread
that originally set the timer with
an associated APC routine.

Unwind exception code.

An invalid or unaligned stack was
encountered during an unwind
operation.

An invalid unwind target was
encountered during an unwind
operation.

Invalid object attributes specified
to NtCreatePort or invalid port
attributes specified to
NtConnectPort.

Length of message passed to
NtRequestPort or
NtRequestWaitReplyPort was
longer than the maximum
message allowed by the port.

An attempt was made to lower a
quota limit below the current
usage.

An attempt was made to attach
to a device that was already
attached to another device.

An attempt was made to execute

215 / 497


Win32 error codes

ERROR_INSTRUCTION_MISALIGNMENT

0x00000226

ERROR_PROFILING_NOT_STARTED

0x00000227

ERROR_PROFILING_NOT_STOPPED

0x00000228

ERROR_COULD_NOT_INTERPRET

0x00000229

ERROR_PROFILING_AT_LIMIT

0x0000022A

ERROR_CANT_WAIT

0x0000022B

ERROR_CANT_TERMINATE_SELF

0x0000022C

ERROR_UNEXPECTED_MM_CREATE_ERR

0x0000022D

ERROR_UNEXPECTED_MM_MAP_ERROR

0x0000022E

ERROR_UNEXPECTED_MM_EXTEND_ERR

0x0000022F

ERROR_BAD_FUNCTION_TABLE

0x00000230

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

an instruction at an unaligned
address, and the host system
does not support unaligned
instruction references.

Profiling not started.

Profiling not stopped.

The passed ACL did not contain
the minimum required
information.

The number of active profiling
objects is at the maximum and
no more can be started.

Used to indicate that an
operation cannot continue
without blocking for I/O.

Indicates that a thread
attempted to terminate itself by
default (called
NtTerminateThread with NULL)
and it was the last thread in the
current process.

If an MM error is returned that is
not defined in the standard FsRtl
filter, it is converted to one of
the following errors that is
guaranteed to be in the filter. In
this case, information is lost;
however, the filter correctly
handles the exception.

If an MM error is returned that is
not defined in the standard FsRtl
filter, it is converted to one of
the following errors that is
guaranteed to be in the filter. In
this case, information is lost;
however, the filter correctly
handles the exception.

If an MM error is returned that is
not defined in the standard FsRtl
filter, it is converted to one of
the following errors that is
guaranteed to be in the filter. In
this case, information is lost;
however, the filter correctly
handles the exception.

A malformed function table was
encountered during an unwind
operation.

Indicates that an attempt was

216 / 497


Win32 error codes

ERROR_NO_GUID_TRANSLATION

0x00000231

ERROR_INVALID_LDT_SIZE

0x00000233

ERROR_INVALID_LDT_OFFSET

0x00000234

ERROR_INVALID_LDT_DESCRIPTOR

0x00000235

ERROR_TOO_MANY_THREADS

0x00000236

ERROR_THREAD_NOT_IN_PROCESS

0x00000237

ERROR_PAGEFILE_QUOTA_EXCEEDED

0x00000238

ERROR_LOGON_SERVER_CONFLICT

0x00000239

ERROR_SYNCHRONIZATION_REQUIRED

0x0000023A

ERROR_NET_OPEN_FAILED

0x0000023B

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

made to assign protection to a
file system file or directory and
one of the SIDs in the security
descriptor could not be
translated into a GUID that could
be stored by the file system. This
causes the protection attempt to
fail, which might cause a file
creation attempt to fail.

Indicates that an attempt was
made to grow a local domain
table (LDT) by setting its size, or
that the size was not an even
number of selectors.

Indicates that the starting value
for the LDT information was not
an integral multiple of the
selector size.

Indicates that the user supplied
an invalid descriptor when trying
to set up LDT descriptors.

Indicates a process has too
many threads to perform the
requested action. For example,
assignment of a primary token
can be performed only when a
process has zero or one threads.

An attempt was made to operate
on a thread within a specific
process, but the thread specified
is not in the process specified.

Page file quota was exceeded.

The Netlogon service cannot
start because another Netlogon
service running in the domain
conflicts with the specified role.

On applicable Windows Server
releases, the Security Accounts
Manager (SAM) database is
significantly out of
synchronization with the copy on
the domain controller. A
complete synchronization is
required.

The NtCreateFile API failed. This
error should never be returned
to an application, it is a place
holder for the Windows LAN
Manager Redirector to use in its
internal error mapping routines.

{Privilege Failed} The I/O
permissions for the process

217 / 497


Win32 error codes

ERROR_IO_PRIVILEGE_FAILED

0x0000023C

ERROR_CONTROL_C_EXIT

0x0000023D

ERROR_MISSING_SYSTEMFILE

0x0000023E

ERROR_UNHANDLED_EXCEPTION

0x0000023F

ERROR_APP_INIT_FAILURE

0x00000240

ERROR_PAGEFILE_CREATE_FAILED

0x00000241

ERROR_INVALID_IMAGE_HASH

0x00000242

ERROR_NO_PAGEFILE

0x00000243

ERROR_ILLEGAL_FLOAT_CONTEXT

0x00000244

ERROR_NO_EVENT_PAIR

0x00000245

ERROR_DOMAIN_CTRLR_CONFIG_ERROR

0x00000246

ERROR_ILLEGAL_CHARACTER

0x00000247

ERROR_UNDEFINED_CHARACTER

0x00000248

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

could not be changed.

{Application Exit by CTRL+C}
The application terminated as a
result of a CTRL+C.

{Missing System File} The
required system file %hs is bad
or missing.

{Application Error} The
exception %s (0x%08lx)
occurred in the application at
location 0x%08lx.

{Application Error} The
application failed to initialize
properly (0x%lx). Click OK to
terminate the application.

{Unable to Create Paging File}
The creation of the paging file
%hs failed (%lx). The requested
size was %ld.

The hash for the image cannot
be found in the system catalogs.
The image is likely corrupt or the
victim of tampering.

{No Paging File Specified} No
paging file was specified in the
system configuration.

{EXCEPTION} A real-mode
application issued a floating-
point instruction, and floating-
point hardware is not present.

An event pair synchronization
operation was performed using
the thread-specific client/server
event pair object, but no event
pair object was associated with
the thread.

A domain server has an incorrect
configuration.

An illegal character was
encountered. For a multibyte
character set, this includes a
lead byte without a succeeding
trail byte. For the Unicode
character set, this includes the
characters 0xFFFF and 0xFFFE.

The Unicode character is not
defined in the Unicode character
set installed on the system.

The paging file cannot be

218 / 497


Win32 error codes

ERROR_FLOPPY_VOLUME

0x00000249

ERROR_BIOS_FAILED_TO_CONNECT_INTERRUPT

0x0000024A

ERROR_BACKUP_CONTROLLER

0x0000024B

ERROR_MUTANT_LIMIT_EXCEEDED

0x0000024C

ERROR_FS_DRIVER_REQUIRED

0x0000024D

ERROR_CANNOT_LOAD_REGISTRY_FILE

0x0000024E

ERROR_DEBUG_ATTACH_FAILED

0x0000024F

ERROR_SYSTEM_PROCESS_TERMINATED

0x00000250

ERROR_DATA_NOT_ACCEPTED

0x00000251

ERROR_VDM_HARD_ERROR

0x00000252

ERROR_DRIVER_CANCEL_TIMEOUT

0x00000253

ERROR_REPLY_MESSAGE_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

created on a floppy disk.

The system bios failed to
connect a system interrupt to
the device or bus for which the
device is connected.

This operation is only allowed for
the primary domain controller
(PDC) of the domain.

An attempt was made to acquire
a mutant such that its maximum
count would have been
exceeded.

A volume has been accessed for
which a file system driver is
required that has not yet been
loaded.

{Registry File Failure} The
registry cannot load the hive
(file): %hs or its log or alternate.
It is corrupt, absent, or not
writable.

{Unexpected Failure in
DebugActiveProcess} An
unexpected failure occurred
while processing a
DebugActiveProcess API request.
Choosing OK will terminate the
process, and choosing Cancel will
ignore the error.

{Fatal System Error} The %hs
system process terminated
unexpectedly with a status of
0x%08x (0x%08x 0x%08x). The
system has been shut down.

{Data Not Accepted} The
transport driver interface (TDI)
client could not handle the data
received during an indication.

The NT Virtual DOS Machine
(NTVDM) encountered a hard
error.

{Cancel Timeout} The driver
%hs failed to complete a
canceled I/O request in the
allotted time.

{Reply Message Mismatch} An
attempt was made to reply to a
local procedure call (LPC)
message, but the thread
specified by the client ID in the
message was not waiting on that
message.

219 / 497


Win32 error codes

0x00000254

ERROR_LOST_WRITEBEHIND_DATA

0x00000255

ERROR_CLIENT_SERVER_PARAMETERS_INVALID

0x00000256

ERROR_NOT_TINY_STREAM

0x00000257

ERROR_STACK_OVERFLOW_READ

0x00000258

ERROR_CONVERT_TO_LARGE

0x00000259

ERROR_FOUND_OUT_OF_SCOPE

0x0000025A

ERROR_ALLOCATE_BUCKET

0x0000025B

ERROR_MARSHALL_OVERFLOW

0x0000025C

ERROR_INVALID_VARIANT

0x0000025D

ERROR_BAD_COMPRESSION_BUFFER

0x0000025E

ERROR_AUDIT_FAILED

0x0000025F

ERROR_TIMER_RESOLUTION_NOT_SET

0x00000260

ERROR_INSUFFICIENT_LOGON_INFO

0x00000261

ERROR_BAD_DLL_ENTRYPOINT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Delayed Write Failed} Windows
was unable to save all the data
for the file %hs. The data has
been lost. This error might be
caused by a failure of your
computer hardware or network
connection. Try to save this file
elsewhere.

The parameters passed to the
server in the client/server shared
memory window were invalid.
Too much data might have been
put in the shared memory
window.

The stream is not a tiny stream.

The request must be handled by
the stack overflow code.

Internal OFS status codes
indicating how an allocation
operation is handled. Either it is
retried after the containing
onode is moved or the extent
stream is converted to a large
stream.

The attempt to find the object
found an object matching by ID
on the volume but it is out of the
scope of the handle used for the
operation.

The bucket array must be grown.
Retry transaction after doing so.

The user/kernel marshaling
buffer has overflowed.

The supplied variant structure
contains invalid data.

The specified buffer contains ill-
formed data.

{Audit Failed} An attempt to
generate a security audit failed.

The timer resolution was not
previously set by the current
process.

There is insufficient account
information to log you on.

{Invalid DLL Entrypoint} The
dynamic link library %hs is not

220 / 497


Win32 error codes

0x00000262

ERROR_BAD_SERVICE_ENTRYPOINT

0x00000263

ERROR_IP_ADDRESS_CONFLICT1

0x00000264

ERROR_IP_ADDRESS_CONFLICT2

0x00000265

ERROR_REGISTRY_QUOTA_LIMIT

0x00000266

ERROR_NO_CALLBACK_ACTIVE

0x00000267

ERROR_PWD_TOO_SHORT

0x00000268

ERROR_PWD_TOO_RECENT

0x00000269

ERROR_PWD_HISTORY_CONFLICT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

written correctly. The stack
pointer has been left in an
inconsistent state. The entry
point should be declared as
WINAPI or STDCALL. Select YES
to fail the DLL load. Select NO to
continue execution. Selecting NO
can cause the application to
operate incorrectly.

{Invalid Service Callback
Entrypoint} The %hs service is
not written correctly. The stack
pointer has been left in an
inconsistent state. The callback
entry point should be declared as
WINAPI or STDCALL. Selecting
OK will cause the service to
continue operation. However,
the service process might
operate incorrectly.

There is an IP address conflict
with another system on the
network.

There is an IP address conflict
with another system on the
network.

{Low On Registry Space} The
system has reached the
maximum size allowed for the
system part of the registry.
Additional storage requests will
be ignored.

A callback return system service
cannot be executed when no
callback is active.

The password provided is too
short to meet the policy of your
user account. Choose a longer
password.

The policy of your user account
does not allow you to change
passwords too frequently. This is
done to prevent users from
changing back to a familiar, but
potentially discovered, password.
If you feel your password has
been compromised, contact your
administrator immediately to
have a new one assigned.

You have attempted to change
your password to one that you
have used in the past. The policy
of your user account does not
allow this. Select a password
that you have not previously

221 / 497


Win32 error codes

0x0000026A

ERROR_UNSUPPORTED_COMPRESSION

0x0000026B

ERROR_INVALID_HW_PROFILE

0x0000026C

ERROR_INVALID_PLUGPLAY_DEVICE_PATH

0x0000026D

ERROR_QUOTA_LIST_INCONSISTENT

0x0000026E

ERROR_EVALUATION_EXPIRATION

0x0000026F

ERROR_ILLEGAL_DLL_RELOCATION

0x00000270

ERROR_DLL_INIT_FAILED_LOGOFF

0x00000271

ERROR_VALIDATE_CONTINUE

0x00000272

ERROR_NO_MORE_MATCHES

0x00000273

ERROR_RANGE_LIST_CONFLICT

0x00000274

ERROR_SERVER_SID_MISMATCH

0x00000275

ERROR_CANT_ENABLE_DENY_ONLY

0x00000276

ERROR_FLOAT_MULTIPLE_FAULTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

used.

The specified compression
format is unsupported.

The specified hardware profile
configuration is invalid.

The specified Plug and Play
registry device path is invalid.

The specified quota list is
internally inconsistent with its
descriptor.

{Windows Evaluation
Notification} The evaluation
period for this installation of
Windows has expired. This
system will shut down in 1 hour.
To restore access to this
installation of Windows, upgrade
this installation using a licensed
distribution of this product.

{Illegal System DLL Relocation}
The system DLL %hs was
relocated in memory. The
application will not run properly.
The relocation occurred because
the DLL %hs occupied an
address range reserved for
Windows system DLLs. The
vendor supplying the DLL should
be contacted for a new DLL.

{DLL Initialization Failed} The
application failed to initialize
because the window station is
shutting down.

The validation process needs to
continue on to the next step.

There are no more matches for
the current index enumeration.

The range could not be added to
the range list because of a
conflict.

The server process is running
under a SID different than that
required by the client.

A group marked use for deny
only cannot be enabled.

{EXCEPTION} Multiple floating
point faults.

222 / 497


Win32 error codes

0x00000277

ERROR_FLOAT_MULTIPLE_TRAPS

0x00000278

ERROR_NOINTERFACE

0x00000279

ERROR_DRIVER_FAILED_SLEEP

0x0000027A

ERROR_CORRUPT_SYSTEM_FILE

0x0000027B

ERROR_COMMITMENT_MINIMUM

0x0000027C

ERROR_PNP_RESTART_ENUMERATION

0x0000027D

ERROR_SYSTEM_IMAGE_BAD_SIGNATURE

0x0000027E

ERROR_PNP_REBOOT_REQUIRED

0x0000027F

ERROR_INSUFFICIENT_POWER

0x00000281

ERROR_SYSTEM_SHUTDOWN

0x00000282

ERROR_PORT_NOT_SET

0x00000283

ERROR_DS_VERSION_CHECK_FAILURE

0x00000284

ERROR_RANGE_NOT_FOUND

0x00000286

ERROR_NOT_SAFE_MODE_DRIVER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{EXCEPTION} Multiple floating
point traps.

The requested interface is not
supported.

{System Standby Failed} The
driver %hs does not support
standby mode. Updating this
driver might allow the system to
go to standby mode.

The system file %1 has become
corrupt and has been replaced.

{Virtual Memory Minimum Too
Low} Your system is low on
virtual memory. Windows is
increasing the size of your virtual
memory paging file. During this
process, memory requests for
some applications might be
denied. For more information,
see Help.

A device was removed so
enumeration must be restarted.

{Fatal System Error} The
system image %s is not properly
signed. The file has been
replaced with the signed file. The
system has been shut down.

Device will not start without a
reboot.

There is not enough power to
complete the requested
operation.

The system is in the process of
shutting down.

An attempt to remove a process
DebugPort was made, but a port
was not already associated with
the process.

This version of Windows is not
compatible with the behavior
version of directory forest,
domain, or domain controller.

The specified range could not be
found in the range list.

The driver was not loaded
because the system is booting
into safe mode.

223 / 497


Win32 error codes

0x00000287

ERROR_FAILED_DRIVER_ENTRY

0x00000288

ERROR_DEVICE_ENUMERATION_ERROR

0x00000289

ERROR_MOUNT_POINT_NOT_RESOLVED

0x0000028A

ERROR_INVALID_DEVICE_OBJECT_PARAMETER

0x0000028B

ERROR_MCA_OCCURED

0x0000028C

ERROR_DRIVER_DATABASE_ERROR

0x0000028D

ERROR_SYSTEM_HIVE_TOO_LARGE

0x0000028E

ERROR_DRIVER_FAILED_PRIOR_UNLOAD

0x0000028F

ERROR_VOLSNAP_PREPARE_HIBERNATE

0x00000290

ERROR_HIBERNATION_FAILURE

0x00000299

ERROR_FILE_SYSTEM_LIMITATION

0x0000029C

ERROR_ASSERTION_FAILURE

0x0000029D

ERROR_ACPI_ERROR

0x0000029E

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The driver was not loaded
because it failed its initialization
call.

The device encountered an error
while applying power or reading
the device configuration. This
might be caused by a failure of
your hardware or by a poor
connection.

The create operation failed
because the name contained at
least one mount point that
resolves to a volume to which
the specified device object is not
attached.

The device object parameter is
either not a valid device object
or is not attached to the volume
specified by the file name.

A machine check error has
occurred. Check the system
event log for additional
information.

There was an error [%2]
processing the driver database.

The system hive size has
exceeded its limit.

The driver could not be loaded
because a previous version of
the driver is still in memory.

{Volume Shadow Copy Service}
Wait while the Volume Shadow
Copy Service prepares volume
%hs for hibernation.

The system has failed to
hibernate (the error code is
%hs). Hibernation will be
disabled until the system is
restarted.

The requested operation could
not be completed due to a file
system limitation.

An assertion failure has
occurred.

An error occurred in the
Advanced Configuration and
Power Interface (ACPI)
subsystem.

WOW assertion error.

224 / 497


Win32 error codes

ERROR_WOW_ASSERTION

0x0000029F

ERROR_PNP_BAD_MPS_TABLE

0x000002A0

ERROR_PNP_TRANSLATION_FAILED

0x000002A1

ERROR_PNP_IRQ_TRANSLATION_FAILED

0x000002A2

ERROR_PNP_INVALID_ID

0x000002A3

ERROR_WAKE_SYSTEM_DEBUGGER

0x000002A4

ERROR_HANDLES_CLOSED

0x000002A5

ERROR_EXTRANEOUS_INFORMATION

0x000002A6

ERROR_RXACT_COMMIT_NECESSARY

0x000002A7

ERROR_MEDIA_CHECK

0x000002A8

ERROR_GUID_SUBSTITUTION_MADE

0x000002A9

ERROR_STOPPED_ON_SYMLINK

0x000002AA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A device is missing in the system
BIOS MultiProcessor
Specification (MPS) table. This
device will not be used. Contact
your system vendor for system
BIOS update.

A translator failed to translate
resources.

An interrupt request (IRQ)
translator failed to translate
resources.

Driver %2 returned invalid ID for
a child device (%3).

{Kernel Debugger Awakened}
the system debugger was
awakened by an interrupt.

{Handles Closed} Handles to
objects have been automatically
closed because of the requested
operation.

{Too Much Information} The
specified ACL contained more
information than was expected.

This warning level status
indicates that the transaction
state already exists for the
registry subtree, but that a
transaction commit was
previously aborted. The commit
has NOT been completed, but it
has not been rolled back either
(so it can still be committed if
desired).

{Media Changed} The media
might have changed.

{GUID Substitution} During the
translation of a GUID to a
Windows SID, no
administratively defined GUID
prefix was found. A substitute
prefix was used, which will not
compromise system security.
However, this might provide
more restrictive access than
intended.

The create operation stopped
after reaching a symbolic link.

A long jump has been executed.

225 / 497


Win32 error codes

ERROR_LONGJUMP

0x000002AB

ERROR_PLUGPLAY_QUERY_VETOED

0x000002AC

ERROR_UNWIND_CONSOLIDATE

0x000002AD

ERROR_REGISTRY_HIVE_RECOVERED

0x000002AE

ERROR_DLL_MIGHT_BE_INSECURE

0x000002AF

ERROR_DLL_MIGHT_BE_INCOMPATIBLE

0x000002B0

ERROR_DBG_EXCEPTION_NOT_HANDLED

0x000002B1

ERROR_DBG_REPLY_LATER

0x000002B2

ERROR_DBG_UNABLE_TO_PROVIDE_HANDLE

0x000002B3

ERROR_DBG_TERMINATE_THREAD

0x000002B4

ERROR_DBG_TERMINATE_PROCESS

0x000002B5

ERROR_DBG_CONTROL_C

0x000002B6

ERROR_DBG_PRINTEXCEPTION_C

0x000002B7

ERROR_DBG_RIPEXCEPTION

0x000002B8

ERROR_DBG_CONTROL_BREAK

0x000002B9

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The Plug and Play query
operation was not successful.

A frame consolidation has been
executed.

{Registry Hive Recovered}
Registry hive (file): %hs was
corrupted and it has been
recovered. Some data might
have been lost.

The application is attempting to
run executable code from the
module %hs. This might be
insecure. An alternative, %hs, is
available. Should the application
use the secure module %hs?

The application is loading
executable code from the
module %hs. This is secure, but
might be incompatible with
previous releases of the
operating system. An
alternative, %hs, is available.
Should the application use the
secure module %hs?

Debugger did not handle the
exception.

Debugger will reply later.

Debugger cannot provide handle.

Debugger terminated thread.

Debugger terminated process.

Debugger got control C.

Debugger printed exception on
control C.

Debugger received Routing
Information Protocol (RIP)
exception.

Debugger received control break.

Debugger command

226 / 497


Win32 error codes

ERROR_DBG_COMMAND_EXCEPTION

0x000002BA

ERROR_OBJECT_NAME_EXISTS

0x000002BB

ERROR_THREAD_WAS_SUSPENDED

0x000002BC

ERROR_IMAGE_NOT_AT_BASE

0x000002BD

ERROR_RXACT_STATE_CREATED

0x000002BE

ERROR_SEGMENT_NOTIFICATION

0x000002BF

ERROR_BAD_CURRENT_DIRECTORY

0x000002C0

ERROR_FT_READ_RECOVERY_FROM_BACKUP

0x000002C1

ERROR_FT_WRITE_RECOVERY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

communication exception.

{Object Exists} An attempt was
made to create an object and the
object name already existed.

{Thread Suspended} A thread
termination occurred while the
thread was suspended. The
thread was resumed and
termination proceeded.

{Image Relocated} An image file
could not be mapped at the
address specified in the image
file. Local fixes must be
performed on this image.

This informational level status
indicates that a specified registry
subtree transaction state did not
yet exist and had to be created.

{Segment Load} A virtual DOS
machine (VDM) is loading,
unloading, or moving an MS-
DOS or Win16 program segment
image. An exception is raised so
a debugger can load, unload, or
track symbols and breakpoints
within these 16-bit segments.

{Invalid Current Directory} The
process cannot switch to the
startup current directory %hs.
Select OK to set current
directory to %hs, or select
CANCEL to exit.

{Redundant Read} To satisfy a
read request, the NT fault-
tolerant file system successfully
read the requested data from a
redundant copy. This was done
because the file system
encountered a failure on a
member of the fault-tolerant
volume, but it was unable to
reassign the failing area of the
device.

{Redundant Write} To satisfy a
write request, the Windows NT
fault-tolerant file system
successfully wrote a redundant
copy of the information. This was
done because the file system
encountered a failure on a
member of the fault-tolerant
volume, but it was not able to
reassign the failing area of the
device.

227 / 497


Win32 error codes

0x000002C2

ERROR_IMAGE_MACHINE_TYPE_MISMATCH

0x000002C3

ERROR_RECEIVE_PARTIAL

0x000002C4

ERROR_RECEIVE_EXPEDITED

0x000002C5

ERROR_RECEIVE_PARTIAL_EXPEDITED

0x000002C6

ERROR_EVENT_DONE

0x000002C7

ERROR_EVENT_PENDING

0x000002C8

ERROR_CHECKING_FILE_SYSTEM

0x000002C9

ERROR_FATAL_APP_EXIT

0x000002CA

ERROR_PREDEFINED_HANDLE

0x000002CB

ERROR_WAS_UNLOCKED

0x000002CD

ERROR_WAS_LOCKED

0x000002CF

ERROR_ALREADY_WIN32

0x000002D0

ERROR_IMAGE_MACHINE_TYPE_MISMATCH_EXE

0x000002D1

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Machine Type Mismatch} The
image file %hs is valid, but is for
a machine type other than the
current machine. Select OK to
continue, or CANCEL to fail the
DLL load.

{Partial Data Received} The
network transport returned
partial data to its client. The
remaining data will be sent later.

{Expedited Data Received} The
network transport returned data
to its client that was marked as
expedited by the remote system.

{Partial Expedited Data
Received} The network transport
returned partial data to its client
and this data was marked as
expedited by the remote system.
The remaining data will be sent
later.

{TDI Event Done} The TDI
indication has completed
successfully.

{TDI Event Pending} The TDI
indication has entered the
pending state.

Checking file system on %wZ.

{Fatal Application Exit} %hs.

The specified registry key is
referenced by a predefined
handle.

{Page Unlocked} The page
protection of a locked page was
changed to 'No Access' and the
page was unlocked from memory
and from the process.

{Page Locked} One of the pages
to lock was already locked.

The value already corresponds
with a Win 32 error code.

{Machine Type Mismatch} The
image file %hs is valid, but is for
a machine type other than the
current machine.

A yield execution was performed
and no thread was available to

228 / 497


Win32 error codes

ERROR_NO_YIELD_PERFORMED

0x000002D2

ERROR_TIMER_RESUME_IGNORED

0x000002D3

ERROR_ARBITRATION_UNHANDLED

0x000002D4

ERROR_CARDBUS_NOT_SUPPORTED

0x000002D5

ERROR_MP_PROCESSOR_MISMATCH

0x000002D6

ERROR_HIBERNATED

0x000002D7

ERROR_RESUME_HIBERNATION

0x000002D8

ERROR_FIRMWARE_UPDATED

0x000002D9

ERROR_DRIVERS_LEAKING_LOCKED_PAGES

0x000002DA

ERROR_WAKE_SYSTEM

0x000002DF

ERROR_ABANDONED_WAIT_0

0x000002E4

ERROR_ELEVATION_REQUIRED

0x000002E5

ERROR_REPARSE

0x000002E6

ERROR_OPLOCK_BREAK_IN_PROGRESS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

run.

The resume flag to a timer API
was ignored.

The arbiter has deferred
arbitration of these resources to
its parent.

The inserted CardBus device
cannot be started because of a
configuration error on %hs"."

The CPUs in this multiprocessor
system are not all the same
revision level. To use all
processors the operating system
restricts itself to the features of
the least capable processor in
the system. If problems occur
with this system, contact the
CPU manufacturer to see if this
mix of processors is supported.

The system was put into
hibernation.

The system was resumed from
hibernation.

Windows has detected that the
system firmware (BIOS) was
updated (previous firmware date
= %2, current firmware date
%3).

A device driver is leaking locked
I/O pages, causing system
degradation. The system has
automatically enabled a tracking
code to try and catch the culprit.

The system has awoken.

The call failed because the
handle associated with it was
closed.

The requested operation requires
elevation.

A reparse should be performed
by the object manager because
the name of the file resulted in a
symbolic link.

An open/create operation
completed while an oplock break
is underway.

229 / 497


Win32 error codes

0x000002E7

ERROR_VOLUME_MOUNTED

0x000002E8

ERROR_RXACT_COMMITTED

0x000002E9

ERROR_NOTIFY_CLEANUP

0x000002EA

ERROR_PRIMARY_TRANSPORT_CONNECT_FAILED

0x000002EB

ERROR_PAGE_FAULT_TRANSITION

0x000002EC

ERROR_PAGE_FAULT_DEMAND_ZERO

0x000002ED

ERROR_PAGE_FAULT_COPY_ON_WRITE

0x000002EE

ERROR_PAGE_FAULT_GUARD_PAGE

0x000002EF

ERROR_PAGE_FAULT_PAGING_FILE

0x000002F0

ERROR_CACHE_PAGE_LOCKED

0x000002F1

ERROR_CRASH_DUMP

0x000002F2

ERROR_BUFFER_ALL_ZEROS

0x000002F3

ERROR_REPARSE_OBJECT

0x000002F4

ERROR_RESOURCE_REQUIREMENTS_CHANGED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A new volume has been mounted
by a file system.

This success level status
indicates that the transaction
state already exists for the
registry subtree, but that a
transaction commit was
previously aborted. The commit
has now been completed.

This indicates that a notify
change request has been
completed due to closing the
handle which made the notify
change request.

{Connect Failure on Primary
Transport} An attempt was
made to connect to the remote
server %hs on the primary
transport, but the connection
failed. The computer was able to
connect on a secondary
transport.

Page fault was a transition fault.

Page fault was a demand zero
fault.

Page fault was a demand zero
fault.

Page fault was a demand zero
fault.

Page fault was satisfied by
reading from a secondary
storage device.

Cached page was locked during
operation.

Crash dump exists in paging file.

Specified buffer contains all
zeros.

A reparse should be performed
by the object manager because
the name of the file resulted in a
symbolic link.

The device has succeeded a
query-stop and its resource
requirements have changed.

230 / 497


Win32 error codes

0x000002F5

ERROR_TRANSLATION_COMPLETE

0x000002F6

ERROR_NOTHING_TO_TERMINATE

0x000002F7

ERROR_PROCESS_NOT_IN_JOB

0x000002F8

ERROR_PROCESS_IN_JOB

0x000002F9

ERROR_VOLSNAP_HIBERNATE_READY

0x000002FA

ERROR_FSFILTER_OP_COMPLETED_SUCCESSFULLY

0x000002FB

ERROR_INTERRUPT_VECTOR_ALREADY_CONNECTED

0x000002FC

ERROR_INTERRUPT_STILL_CONNECTED

0x000002FD

ERROR_WAIT_FOR_OPLOCK

0x000002FE

ERROR_DBG_EXCEPTION_HANDLED

0x000002FF

ERROR_DBG_CONTINUE

0x00000300

ERROR_CALLBACK_POP_STACK

0x00000301

ERROR_COMPRESSION_DISABLED

0x00000302

ERROR_CANTFETCHBACKWARDS

0x00000303

ERROR_CANTSCROLLBACKWARDS

0x00000304

ERROR_ROWSNOTRELEASED

0x00000305

ERROR_BAD_ACCESSOR_FLAGS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The translator has translated
these resources into the global
space and no further translations
should be performed.

A process being terminated has
no threads to terminate.

The specified process is not part
of a job.

The specified process is part of a
job.

{Volume Shadow Copy Service}
The system is now ready for
hibernation.

A file system or file system filter
driver has successfully
completed an FsFilter operation.

The specified interrupt vector
was already connected.

The specified interrupt vector is
still connected.

An operation is blocked waiting
for an oplock.

Debugger handled exception.

Debugger continued.

An exception occurred in a user
mode callback and the kernel
callback frame should be
removed.

Compression is disabled for this
volume.

The data provider cannot fetch
backward through a result set.

The data provider cannot scroll
backward through a result set.

The data provider requires that
previously fetched data is
released before asking for more
data.

The data provider was not able
to interpret the flags set for a
column binding in an accessor.

231 / 497


Win32 error codes

0x00000306

ERROR_ERRORS_ENCOUNTERED

0x00000307

ERROR_NOT_CAPABLE

0x00000308

ERROR_REQUEST_OUT_OF_SEQUENCE

0x00000309

ERROR_VERSION_PARSE_ERROR

0x0000030A

ERROR_BADSTARTPOSITION

0x0000030B

ERROR_MEMORY_HARDWARE

0x0000030C

ERROR_DISK_REPAIR_DISABLED

0x0000030D

ERROR_INSUFFICIENT_RESOURCE_FOR_SPECIFIED_SHARED_SECTION_SIZE

0x0000030E

ERROR_SYSTEM_POWERSTATE_TRANSITION

0x0000030F

ERROR_SYSTEM_POWERSTATE_COMPLEX_TRANSITION

0x00000310

ERROR_MCA_EXCEPTION

0x00000311

ERROR_ACCESS_AUDIT_BY_POLICY

0x00000312

ERROR_ACCESS_DISABLED_NO_SAFER_UI_BY_POLICY

0x00000313

ERROR_ABANDON_HIBERFILE

0x00000314

ERROR_LOST_WRITEBEHIND_DATA_NETWORK_DISCONNECTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

One or more errors occurred
while processing the request.

The implementation is not
capable of performing the
request.

The client of a component
requested an operation that is
not valid given the state of the
component instance.

A version number could not be
parsed.

The iterator's start position is
invalid.

The hardware has reported an
uncorrectable memory error.

The attempted operation
required self-healing to be
enabled.

The Desktop heap encountered
an error while allocating session
memory. There is more
information in the system event
log.

The system power state is
transitioning from %2 to %3.

The system power state is
transitioning from %2 to %3 but
could enter %4.

A thread is getting dispatched
with MCA EXCEPTION because of
MCA.

Access to %1 is monitored by
policy rule %2.

Access to %1 has been restricted
by your administrator by policy
rule %2.

A valid hibernation file has been
invalidated and should be
abandoned.

{Delayed Write Failed} Windows
was unable to save all the data
for the file %hs; the data has
been lost. This error can be
caused by network connectivity
issues. Try to save this file
elsewhere.

232 / 497


Win32 error codes

0x00000315

ERROR_LOST_WRITEBEHIND_DATA_NETWORK_SERVER_ERROR

0x00000316

ERROR_LOST_WRITEBEHIND_DATA_LOCAL_DISK_ERROR

0x000003E2

ERROR_EA_ACCESS_DENIED

0x000003E3

ERROR_OPERATION_ABORTED

0x000003E4

ERROR_IO_INCOMPLETE

0x000003E5

ERROR_IO_PENDING

0x000003E6

ERROR_NOACCESS

0x000003E7

ERROR_SWAPERROR

0x000003E9

ERROR_STACK_OVERFLOW

0x000003EA

ERROR_INVALID_MESSAGE

0x000003EB

ERROR_CAN_NOT_COMPLETE

0x000003EC

ERROR_INVALID_FLAGS

0x000003ED

ERROR_UNRECOGNIZED_VOLUME

0x000003EE

ERROR_FILE_INVALID

0x000003EF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Delayed Write Failed} Windows
was unable to save all the data
for the file %hs; the data has
been lost. This error was
returned by the server on which
the file exists. Try to save this
file elsewhere.

{Delayed Write Failed} Windows
was unable to save all the data
for the file %hs; the data has
been lost. This error can be
caused if the device has been
removed or the media is write-
protected.

Access to the extended attribute
was denied.

The I/O operation has been
aborted because of either a
thread exit or an application
request.

Overlapped I/O event is not in a
signaled state.

Overlapped I/O operation is in
progress.

Invalid access to memory
location.

Error performing in-page
operation.

Recursion too deep; the stack
overflowed.

The window cannot act on the
sent message.

Cannot complete this function.

Invalid flags.

The volume does not contain a
recognized file system. Be sure
that all required file system
drivers are loaded and that the
volume is not corrupted.

The volume for a file has been
externally altered so that the
opened file is no longer valid.

The requested operation cannot
be performed in full-screen

233 / 497


Win32 error codes

ERROR_FULLSCREEN_MODE

0x000003F0

ERROR_NO_TOKEN

0x000003F1

ERROR_BADDB

0x000003F2

ERROR_BADKEY

0x000003F3

ERROR_CANTOPEN

0x000003F4

ERROR_CANTREAD

0x000003F5

ERROR_CANTWRITE

0x000003F6

ERROR_REGISTRY_RECOVERED

0x000003F7

ERROR_REGISTRY_CORRUPT

0x000003F8

ERROR_REGISTRY_IO_FAILED

0x000003F9

ERROR_NOT_REGISTRY_FILE

0x000003FA

ERROR_KEY_DELETED

0x000003FB

ERROR_NO_LOG_SPACE

0x000003FC

ERROR_KEY_HAS_CHILDREN

0x000003FD

ERROR_CHILD_MUST_BE_VOLATILE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

mode.

An attempt was made to
reference a token that does not
exist.

The configuration registry
database is corrupt.

The configuration registry key is
invalid.

The configuration registry key
could not be opened.

The configuration registry key
could not be read.

The configuration registry key
could not be written.

One of the files in the registry
database had to be recovered by
use of a log or alternate copy.
The recovery was successful.

The registry is corrupted. The
structure of one of the files
containing registry data is
corrupted, or the system's
memory image of the file is
corrupted, or the file could not
be recovered because the
alternate copy or log was absent
or corrupted.

An I/O operation initiated by the
registry failed and cannot be
recovered. The registry could not
read in, write out, or flush one of
the files that contain the
system's image of the registry.

The system attempted to load or
restore a file into the registry,
but the specified file is not in a
registry file format.

Illegal operation attempted on a
registry key that has been
marked for deletion.

System could not allocate the
required space in a registry log.

Cannot create a symbolic link in
a registry key that already has
subkeys or values.

Cannot create a stable subkey
under a volatile parent key.

234 / 497


Win32 error codes

0x000003FE

ERROR_NOTIFY_ENUM_DIR

0x0000041B

ERROR_DEPENDENT_SERVICES_RUNNING

0x0000041C

ERROR_INVALID_SERVICE_CONTROL

0x0000041D

ERROR_SERVICE_REQUEST_TIMEOUT

0x0000041E

ERROR_SERVICE_NO_THREAD

0x0000041F

ERROR_SERVICE_DATABASE_LOCKED

0x00000420

ERROR_SERVICE_ALREADY_RUNNING

0x00000421

ERROR_INVALID_SERVICE_ACCOUNT

0x00000422

ERROR_SERVICE_DISABLED

0x00000423

ERROR_CIRCULAR_DEPENDENCY

0x00000424

ERROR_SERVICE_DOES_NOT_EXIST

0x00000425

ERROR_SERVICE_CANNOT_ACCEPT_CTRL

0x00000426

ERROR_SERVICE_NOT_ACTIVE

0x00000427

ERROR_FAILED_SERVICE_CONTROLLER_CONNECT

0x00000428

ERROR_EXCEPTION_IN_SERVICE

0x00000429

ERROR_DATABASE_DOES_NOT_EXIST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A notify change request is being
completed and the information is
not being returned in the caller's
buffer. The caller now needs to
enumerate the files to find the
changes.

A stop control has been sent to a
service that other running
services are dependent on.

The requested control is not
valid for this service.

The service did not respond to
the start or control request in a
timely fashion.

A thread could not be created for
the service.

The service database is locked.

An instance of the service is
already running.

The account name is invalid or
does not exist, or the password
is invalid for the account name
specified.

The service cannot be started,
either because it is disabled or
because it has no enabled
devices associated with it.

Circular service dependency was
specified.

The specified service does not
exist as an installed service.

The service cannot accept
control messages at this time.

The service has not been
started.

The service process could not
connect to the service controller.

An exception occurred in the
service when handling the
control request.

The database specified does not
exist.

235 / 497


Win32 error codes

0x0000042A

ERROR_SERVICE_SPECIFIC_ERROR

0x0000042B

ERROR_PROCESS_ABORTED

0x0000042C

ERROR_SERVICE_DEPENDENCY_FAIL

0x0000042D

ERROR_SERVICE_LOGON_FAILED

0x0000042E

ERROR_SERVICE_START_HANG

0x0000042F

ERROR_INVALID_SERVICE_LOCK

0x00000430

ERROR_SERVICE_MARKED_FOR_DELETE

0x00000431

ERROR_SERVICE_EXISTS

0x00000432

ERROR_ALREADY_RUNNING_LKG

0x00000433

ERROR_SERVICE_DEPENDENCY_DELETED

0x00000434

ERROR_BOOT_ALREADY_ACCEPTED

0x00000435

ERROR_SERVICE_NEVER_STARTED

0x00000436

ERROR_DUPLICATE_SERVICE_NAME

0x00000437

ERROR_DIFFERENT_SERVICE_ACCOUNT

0x00000438

ERROR_CANNOT_DETECT_DRIVER_FAILURE

0x00000439

ERROR_CANNOT_DETECT_PROCESS_ABORT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The service has returned a
service-specific error code.

The process terminated
unexpectedly.

The dependency service or group
failed to start.

The service did not start due to a
logon failure.

After starting, the service
stopped responding in a start-
pending state.

The specified service database
lock is invalid.

The specified service has been
marked for deletion.

The specified service already
exists.

The system is currently running
with the last-known-good
configuration.

The dependency service does not
exist or has been marked for
deletion.

The current boot has already
been accepted for use as the
last-known-good control set.

No attempts to start the service
have been made since the last
boot.

The name is already in use as
either a service name or a
service display name.

The account specified for this
service is different from the
account specified for other
services running in the same
process.

Failure actions can only be set
for Win32 services, not for
drivers.

This service runs in the same
process as the service control
manager. Therefore, the service
control manager cannot take
action if this service's process

236 / 497


Win32 error codes

0x0000043A

ERROR_NO_RECOVERY_PROGRAM

0x0000043B

ERROR_SERVICE_NOT_IN_EXE

0x0000043C

ERROR_NOT_SAFEBOOT_SERVICE

0x0000044C

ERROR_END_OF_MEDIA

0x0000044D

ERROR_FILEMARK_DETECTED

0x0000044E

ERROR_BEGINNING_OF_MEDIA

0x0000044F

ERROR_SETMARK_DETECTED

0x00000450

ERROR_NO_DATA_DETECTED

0x00000451

ERROR_PARTITION_FAILURE

0x00000452

ERROR_INVALID_BLOCK_LENGTH

0x00000453

ERROR_DEVICE_NOT_PARTITIONED

0x00000454

ERROR_UNABLE_TO_LOCK_MEDIA

0x00000455

ERROR_UNABLE_TO_UNLOAD_MEDIA

0x00000456

ERROR_MEDIA_CHANGED

0x00000457

ERROR_BUS_RESET

0x00000458

ERROR_NO_MEDIA_IN_DRIVE

0x00000459

ERROR_NO_UNICODE_TRANSLATION

0x0000045A

ERROR_DLL_INIT_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

terminates unexpectedly.

No recovery program has been
configured for this service.

The executable program that this
service is configured to run in
does not implement the service.

This service cannot be started in
Safe Mode.

The physical end of the tape has
been reached.

A tape access reached a
filemark.

The beginning of the tape or a
partition was encountered.

A tape access reached the end of
a set of files.

No more data is on the tape.

Tape could not be partitioned.

When accessing a new tape of a
multivolume partition, the
current block size is incorrect.

Tape partition information could
not be found when loading a
tape.

Unable to lock the media eject
mechanism.

Unable to unload the media.

The media in the drive might
have changed.

The I/O bus was reset.

No media in drive.

No mapping for the Unicode
character exists in the target
multibyte code page.

A DLL initialization routine failed.

237 / 497


Win32 error codes

0x0000045B

ERROR_SHUTDOWN_IN_PROGRESS

0x0000045C

ERROR_NO_SHUTDOWN_IN_PROGRESS

0x0000045D

ERROR_IO_DEVICE

0x0000045E

ERROR_SERIAL_NO_DEVICE

0x0000045F

ERROR_IRQ_BUSY

0x00000460

ERROR_MORE_WRITES

0x00000461

ERROR_COUNTER_TIMEOUT

0x00000462

ERROR_FLOPPY_ID_MARK_NOT_FOUND

0x00000463

ERROR_FLOPPY_WRONG_CYLINDER

0x00000464

ERROR_FLOPPY_UNKNOWN_ERROR

0x00000465

ERROR_FLOPPY_BAD_REGISTERS

0x00000466

ERROR_DISK_RECALIBRATE_FAILED

0x00000467

ERROR_DISK_OPERATION_FAILED

0x00000468

ERROR_DISK_RESET_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A system shutdown is in
progress.

Unable to abort the system
shutdown because no shutdown
was in progress.

The request could not be
performed because of an I/O
device error.

No serial device was successfully
initialized. The serial driver will
unload.

Unable to open a device that was
sharing an IRQ with other
devices. At least one other
device that uses that IRQ was
already opened.

A serial I/O operation was
completed by another write to
the serial port. (The
IOCTL_SERIAL_XOFF_COUNTER
reached zero.)

A serial I/O operation completed
because the time-out period
expired. (The
IOCTL_SERIAL_XOFF_COUNTER
did not reach zero.)

No ID address mark was found
on the floppy disk.

Mismatch between the floppy
disk sector ID field and the
floppy disk controller track
address.

The floppy disk controller
reported an error that is not
recognized by the floppy disk
driver.

The floppy disk controller
returned inconsistent results in
its registers.

While accessing the hard disk, a
recalibrate operation failed, even
after retries.

While accessing the hard disk, a
disk operation failed even after
retries.

While accessing the hard disk, a
disk controller reset was needed,
but that also failed.

238 / 497


Win32 error codes

0x00000469

ERROR_EOM_OVERFLOW

0x0000046A

ERROR_NOT_ENOUGH_SERVER_MEMORY

0x0000046B

ERROR_POSSIBLE_DEADLOCK

0x0000046C

ERROR_MAPPED_ALIGNMENT

0x00000474

ERROR_SET_POWER_STATE_VETOED

0x00000475

ERROR_SET_POWER_STATE_FAILED

0x00000476

ERROR_TOO_MANY_LINKS

0x0000047E

ERROR_OLD_WIN_VERSION

0x0000047F

ERROR_APP_WRONG_OS

0x00000480

ERROR_SINGLE_INSTANCE_APP

0x00000481

ERROR_RMODE_APP

0x00000482

ERROR_INVALID_DLL

0x00000483

ERROR_NO_ASSOCIATION

0x00000484

ERROR_DDE_FAIL

0x00000485

ERROR_DLL_NOT_FOUND

0x00000486

ERROR_NO_MORE_USER_HANDLES

0x00000487

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Physical end of tape
encountered.

Not enough server storage is
available to process this
command.

A potential deadlock condition
has been detected.

The base address or the file
offset specified does not have
the proper alignment.

An attempt to change the
system power state was vetoed
by another application or driver.

The system BIOS failed an
attempt to change the system
power state.

An attempt was made to create
more links on a file than the file
system supports.

The specified program requires a
newer version of Windows.

The specified program is not a
Windows or MS-DOS program.

Cannot start more than one
instance of the specified
program.

The specified program was
written for an earlier version of
Windows.

One of the library files needed to
run this application is damaged.

No application is associated with
the specified file for this
operation.

An error occurred in sending the
command to the application.

One of the library files needed to
run this application cannot be
found.

The current process has used all
of its system allowance of
handles for Windows manager
objects.

The message can be used only
with synchronous operations.

239 / 497


Win32 error codes

ERROR_MESSAGE_SYNC_ONLY

0x00000488

ERROR_SOURCE_ELEMENT_EMPTY

0x00000489

ERROR_DESTINATION_ELEMENT_FULL

0x0000048A

ERROR_ILLEGAL_ELEMENT_ADDRESS

0x0000048B

ERROR_MAGAZINE_NOT_PRESENT

0x0000048C

ERROR_DEVICE_REINITIALIZATION_NEEDED

0x0000048D

ERROR_DEVICE_REQUIRES_CLEANING

0x0000048E

ERROR_DEVICE_DOOR_OPEN

0x0000048F

ERROR_DEVICE_NOT_CONNECTED

0x00000490

ERROR_NOT_FOUND

0x00000491

ERROR_NO_MATCH

0x00000492

ERROR_SET_NOT_FOUND

0x00000493

ERROR_POINT_NOT_FOUND

0x00000494

ERROR_NO_TRACKING_SERVICE

0x00000495

ERROR_NO_VOLUME_ID

0x00000497

ERROR_UNABLE_TO_REMOVE_REPLACED

0x00000498

ERROR_UNABLE_TO_MOVE_REPLACEMENT

0x00000499

ERROR_UNABLE_TO_MOVE_REPLACEMENT_2

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The indicated source element
has no media.

The indicated destination
element already contains media.

The indicated element does not
exist.

The indicated element is part of
a magazine that is not present.

The indicated device requires re-
initialization due to hardware
errors.

The device has indicated that
cleaning is required before
further operations are
attempted.

The device has indicated that its
door is open.

The device is not connected.

Element not found.

There was no match for the
specified key in the index.

The property set specified does
not exist on the object.

The point passed to
GetMouseMovePoints is not in
the buffer.

The tracking (workstation)
service is not running.

The volume ID could not be
found.

Unable to remove the file to be
replaced.

Unable to move the replacement
file to the file to be replaced. The
file to be replaced has retained
its original name.

Unable to move the replacement
file to the file to be replaced. The
file to be replaced has been
renamed using the backup

240 / 497


Win32 error codes

0x0000049A

ERROR_JOURNAL_DELETE_IN_PROGRESS

0x0000049B

ERROR_JOURNAL_NOT_ACTIVE

0x0000049C

ERROR_POTENTIAL_FILE_FOUND

0x0000049D

ERROR_JOURNAL_ENTRY_DELETED

0x000004A6

ERROR_SHUTDOWN_IS_SCHEDULED

0x000004A7

ERROR_SHUTDOWN_USERS_LOGGED_ON

0x000004B0

ERROR_BAD_DEVICE

0x000004B1

ERROR_CONNECTION_UNAVAIL

0x000004B2

ERROR_DEVICE_ALREADY_REMEMBERED

0x000004B3

ERROR_NO_NET_OR_BAD_PATH

0x000004B4

ERROR_BAD_PROVIDER

0x000004B5

ERROR_CANNOT_OPEN_PROFILE

0x000004B6

ERROR_BAD_PROFILE

0x000004B7

ERROR_NOT_CONTAINER

0x000004B8

ERROR_EXTENDED_ERROR

0x000004B9

ERROR_INVALID_GROUPNAME

0x000004BA

ERROR_INVALID_COMPUTERNAME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

name.

The volume change journal is
being deleted.

The volume change journal is not
active.

A file was found, but it might not
be the correct file.

The journal entry has been
deleted from the journal.

A system shutdown has already
been scheduled.

The system shutdown cannot be
initiated because there are other
users logged on to the computer.

The specified device name is
invalid.

The device is not currently
connected but it is a
remembered connection.

The local device name has a
remembered connection to
another network resource.

The network path was either
typed incorrectly, does not exist,
or the network provider is not
currently available. Try retyping
the path or contact your network
administrator.

The specified network provider
name is invalid.

Unable to open the network
connection profile.

The network connection profile is
corrupted.

Cannot enumerate a
noncontainer.

An extended error has occurred.

The format of the specified group
name is invalid.

The format of the specified
computer name is invalid.

241 / 497


Win32 error codes

0x000004BB

ERROR_INVALID_EVENTNAME

0x000004BC

ERROR_INVALID_DOMAINNAME

0x000004BD

ERROR_INVALID_SERVICENAME

0x000004BE

ERROR_INVALID_NETNAME

0x000004BF

ERROR_INVALID_SHARENAME

0x000004C0

ERROR_INVALID_PASSWORDNAME

0x000004C1

ERROR_INVALID_MESSAGENAME

0x000004C2

ERROR_INVALID_MESSAGEDEST

0x000004C3

ERROR_SESSION_CREDENTIAL_CONFLICT

0x000004C4

ERROR_REMOTE_SESSION_LIMIT_EXCEEDED

0x000004C5

ERROR_DUP_DOMAINNAME

0x000004C6

ERROR_NO_NETWORK

0x000004C7

ERROR_CANCELLED

0x000004C8

ERROR_USER_MAPPED_FILE

0x000004C9

ERROR_CONNECTION_REFUSED

0x000004CA

ERROR_GRACEFUL_DISCONNECT

0x000004CB

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The format of the specified event
name is invalid.

The format of the specified
domain name is invalid.

The format of the specified
service name is invalid.

The format of the specified
network name is invalid.

The format of the specified share
name is invalid.

The format of the specified
password is invalid.

The format of the specified
message name is invalid.

The format of the specified
message destination is invalid.

Multiple connections to a server
or shared resource by the same
user, using more than one user
name, are not allowed.
Disconnect all previous
connections to the server or
shared resource and try again.

An attempt was made to
establish a session to a network
server, but there are already too
many sessions established to
that server.

The workgroup or domain name
is already in use by another
computer on the network.

The network is not present or
not started.

The operation was canceled by
the user.

The requested operation cannot
be performed on a file with a
user-mapped section open.

The remote system refused the
network connection.

The network connection was
gracefully closed.

The network transport endpoint

242 / 497


Win32 error codes

ERROR_ADDRESS_ALREADY_ASSOCIATED

0x000004CC

ERROR_ADDRESS_NOT_ASSOCIATED

0x000004CD

ERROR_CONNECTION_INVALID

0x000004CE

ERROR_CONNECTION_ACTIVE

0x000004CF

ERROR_NETWORK_UNREACHABLE

0x000004D0

ERROR_HOST_UNREACHABLE

0x000004D1

ERROR_PROTOCOL_UNREACHABLE

0x000004D2

ERROR_PORT_UNREACHABLE

0x000004D3

ERROR_REQUEST_ABORTED

0x000004D4

ERROR_CONNECTION_ABORTED

0x000004D5

ERROR_RETRY

0x000004D6

ERROR_CONNECTION_COUNT_LIMIT

0x000004D7

ERROR_LOGIN_TIME_RESTRICTION

0x000004D8

ERROR_LOGIN_WKSTA_RESTRICTION

0x000004D9

ERROR_INCORRECT_ADDRESS

0x000004DA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

already has an address
associated with it.

An address has not yet been
associated with the network
endpoint.

An operation was attempted on a
nonexistent network connection.

An invalid operation was
attempted on an active network
connection.

The network location cannot be
reached. For information about
network troubleshooting, see
Windows Help.

The network location cannot be
reached. For information about
network troubleshooting, see
Windows Help.

The network location cannot be
reached. For information about
network troubleshooting, see
Windows Help.

No service is operating at the
destination network endpoint on
the remote system.

The request was aborted.

The network connection was
aborted by the local system.

The operation could not be
completed. A retry should be
performed.

A connection to the server could
not be made because the limit
on the number of concurrent
connections for this account has
been reached.

Attempting to log on during an
unauthorized time of day for this
account.

The account is not authorized to
log on from this station.

The network address could not
be used for the operation
requested.

The service is already registered.

243 / 497


Win32 error codes

ERROR_ALREADY_REGISTERED

0x000004DB

ERROR_SERVICE_NOT_FOUND

0x000004DC

ERROR_NOT_AUTHENTICATED

0x000004DD

ERROR_NOT_LOGGED_ON

0x000004DE

ERROR_CONTINUE

0x000004DF

ERROR_ALREADY_INITIALIZED

0x000004E0

ERROR_NO_MORE_DEVICES

0x000004E1

ERROR_NO_SUCH_SITE

0x000004E2

ERROR_DOMAIN_CONTROLLER_EXISTS

0x000004E3

ERROR_ONLY_IF_CONNECTED

0x000004E4

ERROR_OVERRIDE_NOCHANGES

0x000004E5

ERROR_BAD_USER_PROFILE

0x000004E6

ERROR_NOT_SUPPORTED_ON_SBS

0x000004E7

ERROR_SERVER_SHUTDOWN_IN_PROGRESS

0x000004E8

ERROR_HOST_DOWN

0x000004E9

ERROR_NON_ACCOUNT_SID

0x000004EA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified service does not
exist.

The operation being requested
was not performed because the
user has not been authenticated.

The operation being requested
was not performed because the
user has not logged on to the
network. The specified service
does not exist.

Continue with work in progress.

An attempt was made to perform
an initialization operation when
initialization has already been
completed.

No more local devices.

The specified site does not exist.

A domain controller with the
specified name already exists.

This operation is supported only
when you are connected to the
server.

The group policy framework
should call the extension even if
there are no changes.

The specified user does not have
a valid profile.

This operation is not supported
on a computer running Windows
Server 2003 operating system
for Small Business Server.

The server machine is shutting
down.

The remote system is not
available. For information about
network troubleshooting, see
Windows Help.

The security identifier provided is
not from an account domain.

The security identifier provided
does not have a domain

244 / 497


Win32 error codes

ERROR_NON_DOMAIN_SID

0x000004EB

ERROR_APPHELP_BLOCK

0x000004EC

ERROR_ACCESS_DISABLED_BY_POLICY

0x000004ED

ERROR_REG_NAT_CONSUMPTION

0x000004EE

ERROR_CSCSHARE_OFFLINE

0x000004EF

ERROR_PKINIT_FAILURE

0x000004F0

ERROR_SMARTCARD_SUBSYSTEM_FAILURE

0x000004F1

ERROR_DOWNGRADE_DETECTED

0x000004F7

ERROR_MACHINE_LOCKED

0x000004F9

ERROR_CALLBACK_SUPPLIED_INVALID_DATA

0x000004FA

ERROR_SYNC_FOREGROUND_REFRESH_REQUIRED

0x000004FB

ERROR_DRIVER_BLOCKED

0x000004FC

ERROR_INVALID_IMPORT_OF_NON_DLL

0x000004FD

ERROR_ACCESS_DISABLED_WEBBLADE

0x000004FE

ERROR_ACCESS_DISABLED_WEBBLADE_TAMPER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

component.

AppHelp dialog canceled, thus
preventing the application from
starting.

This program is blocked by
Group Policy. For more
information, contact your system
administrator.

A program attempt to use an
invalid register value. Normally
caused by an uninitialized
register. This error is Itanium
specific.

The share is currently offline or
does not exist.

The Kerberos protocol
encountered an error while
validating the KDC certificate
during smartcard logon. There is
more information in the system
event log.

The Kerberos protocol
encountered an error while
attempting to utilize the
smartcard subsystem.

The system detected a possible
attempt to compromise security.
Ensure that you can contact the
server that authenticated you.

The machine is locked and
cannot be shut down without the
force option.

An application-defined callback
gave invalid data when called.

The Group Policy framework
should call the extension in the
synchronous foreground policy
refresh.

This driver has been blocked
from loading.

A DLL referenced a module that
was neither a DLL nor the
process's executable image.

Windows cannot open this
program because it has been
disabled.

Windows cannot open this
program because the license

245 / 497


Win32 error codes

0x000004FF

ERROR_RECOVERY_FAILURE

0x00000500

ERROR_ALREADY_FIBER

0x00000501

ERROR_ALREADY_THREAD

0x00000502

ERROR_STACK_BUFFER_OVERRUN

0x00000503

ERROR_PARAMETER_QUOTA_EXCEEDED

0x00000504

ERROR_DEBUGGER_INACTIVE

0x00000505

ERROR_DELAY_LOAD_FAILED

0x00000506

ERROR_VDM_DISALLOWED

0x00000507

ERROR_UNIDENTIFIED_ERROR

0x00000508

ERROR_INVALID_CRUNTIME_PARAMETER

0x00000509

ERROR_BEYOND_VDL

0x0000050A

ERROR_INCOMPATIBLE_SERVICE_SID_TYPE

0x0000050B

ERROR_DRIVER_PROCESS_TERMINATED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

enforcement system has been
tampered with or become
corrupted.

A transaction recover failed.

The current thread has already
been converted to a fiber.

The current thread has already
been converted from a fiber.

The system detected an overrun
of a stack-based buffer in this
application. This overrun could
potentially allow a malicious user
to gain control of this
application.

Data present in one of the
parameters is more than the
function can operate on.

An attempt to perform an
operation on a debug object
failed because the object is in
the process of being deleted.

An attempt to delay-load a .dll or
get a function address in a
delay-loaded .dll failed.

%1 is a 16-bit application. You
do not have permissions to
execute 16-bit applications.
Check your permissions with
your system administrator.

Insufficient information exists to
identify the cause of failure.

The parameter passed to a C
runtime function is incorrect.

The operation occurred beyond
the valid data length of the file.

The service start failed because
one or more services in the
same process have an
incompatible service SID type
setting. A service with a
restricted service SID type can
only coexist in the same process
with other services with a
restricted SID type.

The process hosting the driver
for this device has been
terminated.

246 / 497


Win32 error codes

0x0000050C

ERROR_IMPLEMENTATION_LIMIT

0x0000050D

ERROR_PROCESS_IS_PROTECTED

0x0000050E

ERROR_SERVICE_NOTIFY_CLIENT_LAGGING

0x0000050F

ERROR_DISK_QUOTA_EXCEEDED

0x00000510

ERROR_CONTENT_BLOCKED

0x00000511

ERROR_INCOMPATIBLE_SERVICE_PRIVILEGE

0x00000513

ERROR_INVALID_LABEL

0x00000514

ERROR_NOT_ALL_ASSIGNED

0x00000515

ERROR_SOME_NOT_MAPPED

0x00000516

ERROR_NO_QUOTAS_FOR_ACCOUNT

0x00000517

ERROR_LOCAL_USER_SESSION_KEY

0x00000518

ERROR_NULL_LM_PASSWORD

0x00000519

ERROR_UNKNOWN_REVISION

0x0000051A

ERROR_REVISION_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An operation attempted to
exceed an implementation-
defined limit.

Either the target process, or the
target thread's containing
process, is a protected process.

The service notification client is
lagging too far behind the
current state of services in the
machine.

An operation failed because the
storage quota was exceeded.

An operation failed because the
content was blocked.

A privilege that the service
requires to function properly
does not exist in the service
account configuration. The
Services Microsoft Management
Console (MMC) snap-in
(Services.msc) and the Local
Security Settings MMC snap-in
(Secpol.msc) can be used to
view the service configuration
and the account configuration.

Indicates a particular SID cannot
be assigned as the label of an
object.

Not all privileges or groups
referenced are assigned to the
caller.

Some mapping between account
names and SIDs was not done.

No system quota limits are
specifically set for this account.

No encryption key is available. A
well-known encryption key was
returned.

The password is too complex to
be converted to a LAN Manager
password. The LAN Manager
password returned is a null
string.

The revision level is unknown.

Indicates two revision levels are
incompatible.

247 / 497


Win32 error codes

0x0000051B

ERROR_INVALID_OWNER

 0x0000051C

ERROR_INVALID_PRIMARY_GROUP

0x0000051D

ERROR_NO_IMPERSONATION_TOKEN

0x0000051E

ERROR_CANT_DISABLE_MANDATORY

0x0000051F

ERROR_NO_LOGON_SERVERS

0x00000520

ERROR_NO_SUCH_LOGON_SESSION

0x00000521

ERROR_NO_SUCH_PRIVILEGE

0x00000522

ERROR_PRIVILEGE_NOT_HELD

0x00000523

ERROR_INVALID_ACCOUNT_NAME

0x00000524

ERROR_USER_EXISTS

0x00000525

ERROR_NO_SUCH_USER

0x00000526

ERROR_GROUP_EXISTS

0x00000527

ERROR_NO_SUCH_GROUP

0x00000528

ERROR_MEMBER_IN_GROUP

0x00000529

ERROR_MEMBER_NOT_IN_GROUP

0x0000052A

ERROR_LAST_ADMIN

0x0000052B

ERROR_WRONG_PASSWORD

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

This SID cannot be assigned as
the owner of this object.

This SID cannot be assigned as
the primary group of an object.

An attempt has been made to
operate on an impersonation
token by a thread that is not
currently impersonating a client.

The group cannot be disabled.

There are currently no logon
servers available to service the
logon request.

A specified logon session does
not exist. It might already have
been terminated.

A specified privilege does not
exist.

A required privilege is not held
by the client.

The name provided is not a
properly formed account name.

The specified account already
exists.

The specified account does not
exist.

The specified group already
exists.

The specified group does not
exist.

Either the specified user account
is already a member of the
specified group, or the specified
group cannot be deleted because
it contains a member.

The specified user account is not
a member of the specified group
account.

The last remaining
administration account cannot be
disabled or deleted.

Unable to update the password.
The value provided as the

248 / 497


Win32 error codes

0x0000052C

ERROR_ILL_FORMED_PASSWORD

0x0000052D

ERROR_PASSWORD_RESTRICTION

0x0000052E

ERROR_LOGON_FAILURE

0x0000052F

ERROR_ACCOUNT_RESTRICTION

0x00000530

ERROR_INVALID_LOGON_HOURS

0x00000531

ERROR_INVALID_WORKSTATION

0x00000532

ERROR_PASSWORD_EXPIRED

0x00000533

ERROR_ACCOUNT_DISABLED

0x00000534

ERROR_NONE_MAPPED

0x00000535

ERROR_TOO_MANY_LUIDS_REQUESTED

0x00000536

ERROR_LUIDS_EXHAUSTED

0x00000537

ERROR_INVALID_SUB_AUTHORITY

0x00000538

ERROR_INVALID_ACL

0x00000539

ERROR_INVALID_SID

0x0000053A

ERROR_INVALID_SECURITY_DESCR

0x0000053C

ERROR_BAD_INHERITANCE_ACL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

current password is incorrect.

Unable to update the password.
The value provided for the new
password contains values that
are not allowed in passwords.

Unable to update the password.
The value provided for the new
password does not meet the
length, complexity, or history
requirements of the domain.

Logon failure: Unknown user
name or bad password.

Logon failure: User account
restriction. Possible reasons are
blank passwords not allowed,
logon hour restrictions, or a
policy restriction has been
enforced.

Logon failure: Account logon
time restriction violation.

Logon failure: User not allowed
to log on to this computer.

Logon failure: The specified
account password has expired.

Logon failure: Account currently
disabled.

No mapping between account
names and SIDs was done.

Too many local user identifiers
(LUIDs) were requested at one
time.

No more LUIDs are available.

The sub-authority part of an SID
is invalid for this particular use.

The ACL structure is invalid.

The SID structure is invalid.

The security descriptor structure
is invalid.

The inherited ACL or ACE could
not be built.

249 / 497


Win32 error codes

0x0000053D

ERROR_SERVER_DISABLED

0x0000053E

ERROR_SERVER_NOT_DISABLED

0x0000053F

ERROR_INVALID_ID_AUTHORITY

0x00000540

ERROR_ALLOTTED_SPACE_EXCEEDED

0x00000541

ERROR_INVALID_GROUP_ATTRIBUTES

0x00000542

ERROR_BAD_IMPERSONATION_LEVEL

0x00000543

ERROR_CANT_OPEN_ANONYMOUS

0x00000544

ERROR_BAD_VALIDATION_CLASS

0x00000545

ERROR_BAD_TOKEN_TYPE

0x00000546

ERROR_NO_SECURITY_ON_OBJECT

0x00000547

ERROR_CANT_ACCESS_DOMAIN_INFO

0x00000548

ERROR_INVALID_SERVER_STATE

0x00000549

ERROR_INVALID_DOMAIN_STATE

0x0000054A

ERROR_INVALID_DOMAIN_ROLE

0x0000054B

ERROR_NO_SUCH_DOMAIN

0x0000054C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The server is currently disabled.

The server is currently enabled.

The value provided was an
invalid value for an identifier
authority.

No more memory is available for
security information updates.

The specified attributes are
invalid, or incompatible with the
attributes for the group as a
whole.

Either a required impersonation
level was not provided, or the
provided impersonation level is
invalid.

Cannot open an anonymous level
security token.

The validation information class
requested was invalid.

The type of the token is
inappropriate for its attempted
use.

Unable to perform a security
operation on an object that has
no associated security.

Configuration information could
not be read from the domain
controller, either because the
machine is unavailable, or access
has been denied.

The SAM or local security
authority (LSA) server was in the
wrong state to perform the
security operation.

The domain was in the wrong
state to perform the security
operation.

This operation is only allowed for
the PDC of the domain.

The specified domain either does
not exist or could not be
contacted.

The specified domain already
exists.

250 / 497


Win32 error codes

ERROR_DOMAIN_EXISTS

0x0000054D

ERROR_DOMAIN_LIMIT_EXCEEDED

0x0000054E

ERROR_INTERNAL_DB_CORRUPTION

0x0000054F

ERROR_INTERNAL_ERROR

0x00000550

ERROR_GENERIC_NOT_MAPPED

0x00000551

ERROR_BAD_DESCRIPTOR_FORMAT

0x00000552

ERROR_NOT_LOGON_PROCESS

0x00000553

ERROR_LOGON_SESSION_EXISTS

0x00000554

ERROR_NO_SUCH_PACKAGE

0x00000555

ERROR_BAD_LOGON_SESSION_STATE

0x00000556

ERROR_LOGON_SESSION_COLLISION

0x00000557

ERROR_INVALID_LOGON_TYPE

0x00000558

ERROR_CANNOT_IMPERSONATE

0x00000559

ERROR_RXACT_INVALID_STATE

0x0000055A

ERROR_RXACT_COMMIT_FAILURE

0x0000055B

ERROR_SPECIAL_ACCOUNT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An attempt was made to exceed
the limit on the number of
domains per server.

Unable to complete the
requested operation because of
either a catastrophic media
failure or a data structure
corruption on the disk.

An internal error occurred.

Generic access types were
contained in an access mask that
should already be mapped to
nongeneric types.

A security descriptor is not in the
right format (absolute or self-
relative).

The requested action is
restricted for use by logon
processes only. The calling
process has not registered as a
logon process.

Cannot start a new logon session
with an ID that is already in use.

A specified authentication
package is unknown.

The logon session is not in a
state that is consistent with the
requested operation.

The logon session ID is already
in use.

A logon request contained an
invalid logon type value.

Unable to impersonate using a
named pipe until data has been
read from that pipe.

The transaction state of a
registry subtree is incompatible
with the requested operation.

An internal security database
corruption has been
encountered.

Cannot perform this operation on
built-in accounts.

251 / 497


Win32 error codes

0x0000055C

ERROR_SPECIAL_GROUP

0x0000055D

ERROR_SPECIAL_USER

0x0000055E

ERROR_MEMBERS_PRIMARY_GROUP

0x0000055F

ERROR_TOKEN_ALREADY_IN_USE

0x00000560

ERROR_NO_SUCH_ALIAS

0x00000561

ERROR_MEMBER_NOT_IN_ALIAS

0x00000562

ERROR_MEMBER_IN_ALIAS

0x00000563

ERROR_ALIAS_EXISTS

0x00000564

ERROR_LOGON_NOT_GRANTED

0x00000565

ERROR_TOO_MANY_SECRETS

0x00000566

ERROR_SECRET_TOO_LONG

0x00000567

ERROR_INTERNAL_DB_ERROR

0x00000568

ERROR_TOO_MANY_CONTEXT_IDS

0x00000569

ERROR_LOGON_TYPE_NOT_GRANTED

0x0000056A

ERROR_NT_CROSS_ENCRYPTION_REQUIRED

0x0000056B

ERROR_NO_SUCH_MEMBER

0x0000056C

ERROR_INVALID_MEMBER

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Cannot perform this operation on
this built-in special group.

Cannot perform this operation on
this built-in special user.

The user cannot be removed
from a group because the group
is currently the user's primary
group.

The token is already in use as a
primary token.

The specified local group does
not exist.

The specified account name is
not a member of the group.

The specified account name is
already a member of the group.

The specified local group already
exists.

Logon failure: The user has not
been granted the requested
logon type at this computer.

The maximum number of secrets
that can be stored in a single
system has been exceeded.

The length of a secret exceeds
the maximum length allowed.

The local security authority
database contains an internal
inconsistency.

During a logon attempt, the
user's security context
accumulated too many SIDs.

Logon failure: The user has not
been granted the requested
logon type at this computer.

A cross-encrypted password is
necessary to change a user
password.

A member could not be added to
or removed from the local group
because the member does not
exist.

A new member could not be
added to a local group because

252 / 497


Win32 error codes

0x0000056D

ERROR_TOO_MANY_SIDS

0x0000056E

ERROR_LM_CROSS_ENCRYPTION_REQUIRED

0x0000056F

ERROR_NO_INHERITANCE

0x00000570

ERROR_FILE_CORRUPT

0x00000571

ERROR_DISK_CORRUPT

0x00000572

ERROR_NO_USER_SESSION_KEY

0x00000573

ERROR_LICENSE_QUOTA_EXCEEDED

0x00000574

ERROR_WRONG_TARGET_NAME

0x00000575

ERROR_MUTUAL_AUTH_FAILED

0x00000576

ERROR_TIME_SKEW

0x00000577

ERROR_CURRENT_DOMAIN_NOT_ALLOWED

0x00000578

ERROR_INVALID_WINDOW_HANDLE

0x00000579

ERROR_INVALID_MENU_HANDLE

0x0000057A

ERROR_INVALID_CURSOR_HANDLE

0x0000057B

ERROR_INVALID_ACCEL_HANDLE

Description

the member has the wrong
account type.

Too many SIDs have been
specified.

A cross-encrypted password is
necessary to change this user
password.

Indicates an ACL contains no
inheritable components.

The file or directory is corrupted
and unreadable.

The disk structure is corrupted
and unreadable.

There is no user session key for
the specified logon session.

The service being accessed is
licensed for a particular number
of connections. No more
connections can be made to the
service at this time because the
service has accepted the
maximum number of
connections.

Logon failure: The target account
name is incorrect.

Mutual authentication failed. The
server's password is out of date
at the domain controller.

There is a time and/or date
difference between the client and
server.

This operation cannot be
performed on the current
domain.

Invalid window handle.

Invalid menu handle.

Invalid cursor handle.

Invalid accelerator table handle.

0x0000057C

Invalid hook handle.

253 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Win32 error codes

ERROR_INVALID_HOOK_HANDLE

0x0000057D

ERROR_INVALID_DWP_HANDLE

0x0000057E

ERROR_TLW_WITH_WSCHILD

0x0000057F

ERROR_CANNOT_FIND_WND_CLASS

0x00000580

ERROR_WINDOW_OF_OTHER_THREAD

0x00000581

ERROR_HOTKEY_ALREADY_REGISTERED

0x00000582

ERROR_CLASS_ALREADY_EXISTS

0x00000583

ERROR_CLASS_DOES_NOT_EXIST

0x00000584

ERROR_CLASS_HAS_WINDOWS

0x00000585

ERROR_INVALID_INDEX

0x00000586

ERROR_INVALID_ICON_HANDLE

0x00000587

ERROR_PRIVATE_DIALOG_INDEX

0x00000588

ERROR_LISTBOX_ID_NOT_FOUND

0x00000589

ERROR_NO_WILDCARD_CHARACTERS

0x0000058A

ERROR_CLIPBOARD_NOT_OPEN

0x0000058B

ERROR_HOTKEY_NOT_REGISTERED

0x0000058C

ERROR_WINDOW_NOT_DIALOG

0x0000058D

ERROR_CONTROL_ID_NOT_FOUND

0x0000058E

ERROR_INVALID_COMBOBOX_MESSAGE

0x0000058F

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Invalid handle to a multiple-
window position structure.

Cannot create a top-level child
window.

Cannot find window class.

Invalid window; it belongs to
other thread.

Hot key is already registered.

Class already exists.

Class does not exist.

Class still has open windows.

Invalid index.

Invalid icon handle.

Using private DIALOG window
words.

The list box identifier was not
found.

No wildcards were found.

Thread does not have a clipboard
open.

Hot key is not registered.

The window is not a valid dialog
window.

Control ID not found.

Invalid message for a combo box
because it does not have an edit
control.

The window is not a combo box.

254 / 497


Win32 error codes

ERROR_WINDOW_NOT_COMBOBOX

0x00000590

ERROR_INVALID_EDIT_HEIGHT

0x00000591

ERROR_DC_NOT_FOUND

0x00000592

ERROR_INVALID_HOOK_FILTER

0x00000593

ERROR_INVALID_FILTER_PROC

0x00000594

ERROR_HOOK_NEEDS_HMOD

0x00000595

ERROR_GLOBAL_ONLY_HOOK

0x00000596

ERROR_JOURNAL_HOOK_SET

0x00000597

ERROR_HOOK_NOT_INSTALLED

0x00000598

ERROR_INVALID_LB_MESSAGE

0x00000599

ERROR_SETCOUNT_ON_BAD_LB

0x0000059A

ERROR_LB_WITHOUT_TABSTOPS

0x0000059B

ERROR_DESTROY_OBJECT_OF_OTHER_THREAD

0x0000059C

ERROR_CHILD_WINDOW_MENU

0x0000059D

ERROR_NO_SYSTEM_MENU

0x0000059E

ERROR_INVALID_MSGBOX_STYLE

0x0000059F

ERROR_INVALID_SPI_VALUE

0x000005A0

ERROR_SCREEN_ALREADY_LOCKED

0x000005A1

ERROR_HWNDS_HAVE_DIFF_PARENT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Height must be less than 256.

Invalid device context (DC)
handle.

Invalid hook procedure type.

Invalid hook procedure.

Cannot set nonlocal hook without
a module handle.

This hook procedure can only be
set globally.

The journal hook procedure is
already installed.

The hook procedure is not
installed.

Invalid message for single-
selection list box.

LB_SETCOUNT sent to non-lazy
list box.

This list box does not support
tab stops.

Cannot destroy object created by
another thread.

Child windows cannot have
menus.

The window does not have a
system menu.

Invalid message box style.

Invalid system-wide (SPI_*)
parameter.

Screen already locked.

All handles to windows in a
multiple-window position
structure must have the same
parent.

255 / 497


Win32 error codes

0x000005A2

ERROR_NOT_CHILD_WINDOW

0x000005A3

ERROR_INVALID_GW_COMMAND

0x000005A4

ERROR_INVALID_THREAD_ID

0x000005A5

ERROR_NON_MDICHILD_WINDOW

0x000005A6

ERROR_POPUP_ALREADY_ACTIVE

0x000005A7

ERROR_NO_SCROLLBARS

0x000005A8

ERROR_INVALID_SCROLLBAR_RANGE

0x000005A9

ERROR_INVALID_SHOWWIN_COMMAND

0x000005AA

ERROR_NO_SYSTEM_RESOURCES

0x000005AB

ERROR_NONPAGED_SYSTEM_RESOURCES

0x000005AC

ERROR_PAGED_SYSTEM_RESOURCES

0x000005AD

ERROR_WORKING_SET_QUOTA

0x000005AE

ERROR_PAGEFILE_QUOTA

0x000005AF

ERROR_COMMITMENT_LIMIT

0x000005B0

ERROR_MENU_ITEM_NOT_FOUND

0x000005B1

ERROR_INVALID_KEYBOARD_HANDLE

0x000005B2

ERROR_HOOK_TYPE_NOT_ALLOWED

Description

The window is not a child
window.

Invalid GW_* command.

Invalid thread identifier.

Cannot process a message from
a window that is not a multiple
document interface (MDI)
window.

Pop-up menu already active.

The window does not have scroll
bars.

Scroll bar range cannot be
greater than MAXLONG.

Cannot show or remove the
window in the way specified.

Insufficient system resources
exist to complete the requested
service.

Insufficient system resources
exist to complete the requested
service.

Insufficient system resources
exist to complete the requested
service.

Insufficient quota to complete
the requested service.

Insufficient quota to complete
the requested service.

The paging file is too small for
this operation to complete.

A menu item was not found.

Invalid keyboard layout handle.

Hook type not allowed.

0x000005B3

ERROR_REQUIRES_INTERACTIVE_WINDOWSTATION

This operation requires an
interactive window station.

256 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Win32 error codes

0x000005B4

ERROR_TIMEOUT

0x000005B5

ERROR_INVALID_MONITOR_HANDLE

0x000005B6

ERROR_INCORRECT_SIZE

0x000005B7

ERROR_SYMLINK_CLASS_DISABLED

0x000005B8

ERROR_SYMLINK_NOT_SUPPORTED

0x000005DC

ERROR_EVENTLOG_FILE_CORRUPT

0x000005DD

ERROR_EVENTLOG_CANT_START

0x000005DE

ERROR_LOG_FILE_FULL

0x000005DF

ERROR_EVENTLOG_FILE_CHANGED

0x0000060E

ERROR_INVALID_TASK_NAME

0x0000060F

ERROR_INVALID_TASK_INDEX

0x00000610

ERROR_THREAD_ALREADY_IN_TASK

0x00000641

ERROR_INSTALL_SERVICE_FAILURE

0x00000642

ERROR_INSTALL_USEREXIT

0x00000643

ERROR_INSTALL_FAILURE

0x00000644

ERROR_INSTALL_SUSPEND

0x00000645

ERROR_UNKNOWN_PRODUCT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

This operation returned because
the time-out period expired.

Invalid monitor handle.

Incorrect size argument.

The symbolic link cannot be
followed because its type is
disabled.

This application does not support
the current operation on
symbolic links.

The event log file is corrupted.

No event log file could be
opened, so the event logging
service did not start.

The event log file is full.

The event log file has changed
between read operations.

The specified task name is
invalid.

The specified task index is
invalid.

The specified thread is already
joining a task.

The Windows Installer service
could not be accessed. This can
occur if the Windows Installer is
not correctly installed. Contact
your support personnel for
assistance.

User canceled installation.

Fatal error during installation.

Installation suspended,
incomplete.

This action is valid only for
products that are currently
installed.

257 / 497


Win32 error codes

0x00000646

ERROR_UNKNOWN_FEATURE

0x00000647

ERROR_UNKNOWN_COMPONENT

0x00000648

ERROR_UNKNOWN_PROPERTY

0x00000649

ERROR_INVALID_HANDLE_STATE

0x0000064A

ERROR_BAD_CONFIGURATION

0x0000064B

ERROR_INDEX_ABSENT

0x0000064C

ERROR_INSTALL_SOURCE_ABSENT

0x0000064D

ERROR_INSTALL_PACKAGE_VERSION

0x0000064E

ERROR_PRODUCT_UNINSTALLED

0x0000064F

ERROR_BAD_QUERY_SYNTAX

0x00000650

ERROR_INVALID_FIELD

0x00000651

ERROR_DEVICE_REMOVED

0x00000652

ERROR_INSTALL_ALREADY_RUNNING

0x00000653

ERROR_INSTALL_PACKAGE_OPEN_FAILED

0x00000654

ERROR_INSTALL_PACKAGE_INVALID

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Feature ID not registered.

Component ID not registered.

Unknown property.

Handle is in an invalid state.

The configuration data for this
product is corrupt. Contact your
support personnel.

Component qualifier not present.

The installation source for this
product is not available. Verify
that the source exists and that
you can access it.

This installation package cannot
be installed by the Windows
Installer service. You must install
a Windows service pack that
contains a newer version of the
Windows Installer service.

Product is uninstalled.

SQL query syntax invalid or
unsupported.

Record field does not exist.

The device has been removed.

Another installation is already in
progress. Complete that
installation before proceeding
with this install.

This installation package could
not be opened. Verify that the
package exists and that you can
access it, or contact the
application vendor to verify that
this is a valid Windows Installer
package.

This installation package could
not be opened. Contact the
application vendor to verify that
this is a valid Windows Installer

258 / 497


Win32 error codes

0x00000655

ERROR_INSTALL_UI_FAILURE

0x00000656

ERROR_INSTALL_LOG_FAILURE

0x00000657

ERROR_INSTALL_LANGUAGE_UNSUPPORTED

0x00000658

ERROR_INSTALL_TRANSFORM_FAILURE

0x00000659

ERROR_INSTALL_PACKAGE_REJECTED

0x0000065A

ERROR_FUNCTION_NOT_CALLED

0x0000065B

ERROR_FUNCTION_FAILED

0x0000065C

ERROR_INVALID_TABLE

0x0000065D

ERROR_DATATYPE_MISMATCH

0x0000065E

ERROR_UNSUPPORTED_TYPE

0x0000065F

ERROR_CREATE_FAILED

0x00000660

ERROR_INSTALL_TEMP_UNWRITABLE

0x00000661

ERROR_INSTALL_PLATFORM_UNSUPPORTED

0x00000662

ERROR_INSTALL_NOTUSED

0x00000663

ERROR_PATCH_PACKAGE_OPEN_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

package.

There was an error starting the
Windows Installer service user
interface. Contact your support
personnel.

Error opening installation log file.
Verify that the specified log file
location exists and that you can
write to it.

The language of this installation
package is not supported by
your system.

Error applying transforms. Verify
that the specified transform
paths are valid.

This installation is forbidden by
system policy. Contact your
system administrator.

Function could not be executed.

Function failed during execution.

Invalid or unknown table
specified.

Data supplied is of wrong type.

Data of this type is not
supported.

The Windows Installer service
failed to start. Contact your
support personnel.

The Temp folder is on a drive
that is full or is inaccessible. Free
up space on the drive or verify
that you have write permission
on the Temp folder.

This installation package is not
supported by this processor
type. Contact your product
vendor.

Component not used on this
computer.

This update package could not
be opened. Verify that the
update package exists and that
you can access it, or contact the

259 / 497


Win32 error codes

0x00000664

ERROR_PATCH_PACKAGE_INVALID

0x00000665

ERROR_PATCH_PACKAGE_UNSUPPORTED

0x00000666

ERROR_PRODUCT_VERSION

0x00000667

ERROR_INVALID_COMMAND_LINE

0x00000668

ERROR_INSTALL_REMOTE_DISALLOWED

0x00000669

ERROR_SUCCESS_REBOOT_INITIATED

0x0000066A

ERROR_PATCH_TARGET_NOT_FOUND

0x0000066B

ERROR_PATCH_PACKAGE_REJECTED

0x0000066C

ERROR_INSTALL_TRANSFORM_REJECTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

application vendor to verify that
this is a valid Windows Installer
update package.

This update package could not
be opened. Contact the
application vendor to verify that
this is a valid Windows Installer
update package.

This update package cannot be
processed by the Windows
Installer service. You must install
a Windows service pack that
contains a newer version of the
Windows Installer service.

Another version of this product is
already installed. Installation of
this version cannot continue. To
configure or remove the existing
version of this product, use
Add/Remove Programs in Control
Panel.

Invalid command-line argument.
Consult the Windows Installer
SDK for detailed command line
help.

Only administrators have
permission to add, remove, or
configure server software during
a Terminal Services remote
session. If you want to install or
configure software on the server,
contact your network
administrator.

The requested operation
completed successfully. The
system will be restarted so the
changes can take effect.

The upgrade cannot be installed
by the Windows Installer service
because the program to be
upgraded might be missing, or
the upgrade might update a
different version of the program.
Verify that the program to be
upgraded exists on your
computer and that you have the
correct upgrade.

The update package is not
permitted by a software
restriction policy.

One or more customizations are
not permitted by a software
restriction policy.

260 / 497


Win32 error codes

0x0000066D

ERROR_INSTALL_REMOTE_PROHIBITED

0x0000066E

ERROR_PATCH_REMOVAL_UNSUPPORTED

0x0000066F

ERROR_UNKNOWN_PATCH

0x00000670

ERROR_PATCH_NO_SEQUENCE

0x00000671

ERROR_PATCH_REMOVAL_DISALLOWED

0x00000672

ERROR_INVALID_PATCH_XML

0x00000673

ERROR_PATCH_MANAGED_ADVERTISED_PRODUCT

0x00000674

ERROR_INSTALL_SERVICE_SAFEBOOT

0x000006A4

RPC_S_INVALID_STRING_BINDING

0x000006A5

RPC_S_WRONG_KIND_OF_BINDING

0x000006A6

RPC_S_INVALID_BINDING

0x000006A7

RPC_S_PROTSEQ_NOT_SUPPORTED

0x000006A8

RPC_S_INVALID_RPC_PROTSEQ

0x000006A9

RPC_S_INVALID_STRING_UUID

0x000006AA

RPC_S_INVALID_ENDPOINT_FORMAT

0x000006AB

RPC_S_INVALID_NET_ADDR

0x000006AC

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The Windows Installer does not
permit installation from a
Remote Desktop Connection.

Uninstallation of the update
package is not supported.

The update is not applied to this
product.

No valid sequence could be
found for the set of updates.

Update removal was disallowed
by policy.

The XML update data is invalid.

Windows Installer does not
permit updating of managed
advertised products. At least one
feature of the product must be
installed before applying the
update.

The Windows Installer service is
not accessible in Safe Mode. Try
again when your computer is not
in Safe Mode or you can use
System Restore to return your
machine to a previous good
state.

The string binding is invalid.

The binding handle is not the
correct type.

The binding handle is invalid.

The RPC protocol sequence is not
supported.

The RPC protocol sequence is
invalid.

The string UUID is invalid.

The endpoint format is invalid.

The network address is invalid.

No endpoint was found.

261 / 497


Win32 error codes

RPC_S_NO_ENDPOINT_FOUND

0x000006AD

RPC_S_INVALID_TIMEOUT

0x000006AE

RPC_S_OBJECT_NOT_FOUND

0x000006AF

RPC_S_ALREADY_REGISTERED

0x000006B0

RPC_S_TYPE_ALREADY_REGISTERED

0x000006B1

RPC_S_ALREADY_LISTENING

0x000006B2

RPC_S_NO_PROTSEQS_REGISTERED

0x000006B3

RPC_S_NOT_LISTENING

0x000006B4

RPC_S_UNKNOWN_MGR_TYPE

0x000006B5

RPC_S_UNKNOWN_IF

0x000006B6

RPC_S_NO_BINDINGS

0x000006B7

RPC_S_NO_PROTSEQS

0x000006B8

RPC_S_CANT_CREATE_ENDPOINT

0x000006B9

RPC_S_OUT_OF_RESOURCES

0x000006BA

RPC_S_SERVER_UNAVAILABLE

0x000006BB

RPC_S_SERVER_TOO_BUSY

0x000006BC

RPC_S_INVALID_NETWORK_OPTIONS

0x000006BD

RPC_S_NO_CALL_ACTIVE

0x000006BE

RPC_S_CALL_FAILED

0x000006BF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The time-out value is invalid.

The object UUID) was not found.

The object UUID) has already
been registered.

The type UUID has already been
registered.

The RPC server is already
listening.

No protocol sequences have
been registered.

The RPC server is not listening.

The manager type is unknown.

The interface is unknown.

There are no bindings.

There are no protocol sequences.

The endpoint cannot be created.

Not enough resources are
available to complete this
operation.

The RPC server is unavailable.

The RPC server is too busy to
complete this operation.

The network options are invalid.

There are no RPCs active on this
thread.

The RPC failed.

The RPC failed and did not

262 / 497


Win32 error codes

RPC_S_CALL_FAILED_DNE

0x000006C0

RPC_S_PROTOCOL_ERROR

0x000006C1

RPC_S_PROXY_ACCESS_DENIED

0x000006C2

RPC_S_UNSUPPORTED_TRANS_SYN

0x000006C4

RPC_S_UNSUPPORTED_TYPE

0x000006C5

RPC_S_INVALID_TAG

0x000006C6

RPC_S_INVALID_BOUND

0x000006C7

RPC_S_NO_ENTRY_NAME

0x000006C8

RPC_S_INVALID_NAME_SYNTAX

0x000006C9

RPC_S_UNSUPPORTED_NAME_SYNTAX

0x000006CB

RPC_S_UUID_NO_ADDRESS

0x000006CC

RPC_S_DUPLICATE_ENDPOINT

0x000006CD

RPC_S_UNKNOWN_AUTHN_TYPE

0x000006CE

RPC_S_MAX_CALLS_TOO_SMALL

0x000006CF

RPC_S_STRING_TOO_LONG

0x000006D0

RPC_S_PROTSEQ_NOT_FOUND

0x000006D1

RPC_S_PROCNUM_OUT_OF_RANGE

0x000006D2

RPC_S_BINDING_HAS_NO_AUTH

0x000006D3

RPC_S_UNKNOWN_AUTHN_SERVICE

0x000006D4

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

execute.

An RPC protocol error occurred.

Access to the HTTP proxy is
denied.

The transfer syntax is not
supported by the RPC server.

The UUID type is not supported.

The tag is invalid.

The array bounds are invalid.

The binding does not contain an
entry name.

The name syntax is invalid.

The name syntax is not
supported.

No network address is available
to use to construct a UUID.

The endpoint is a duplicate.

The authentication type is
unknown.

The maximum number of calls is
too small.

The string is too long.

The RPC protocol sequence was
not found.

The procedure number is out of
range.

The binding does not contain any
authentication information.

The authentication service is
unknown.

The authentication level is

263 / 497


Win32 error codes

RPC_S_UNKNOWN_AUTHN_LEVEL

0x000006D5

RPC_S_INVALID_AUTH_IDENTITY

0x000006D6

RPC_S_UNKNOWN_AUTHZ_SERVICE

0x000006D7

EPT_S_INVALID_ENTRY

0x000006D8

EPT_S_CANT_PERFORM_OP

0x000006D9

EPT_S_NOT_REGISTERED

0x000006DA

RPC_S_NOTHING_TO_EXPORT

0x000006DB

RPC_S_INCOMPLETE_NAME

0x000006DC

RPC_S_INVALID_VERS_OPTION

0x000006DD

RPC_S_NO_MORE_MEMBERS

0x000006DE

RPC_S_NOT_ALL_OBJS_UNEXPORTED

0x000006DF

RPC_S_INTERFACE_NOT_FOUND

0x000006E0

RPC_S_ENTRY_ALREADY_EXISTS

0x000006E1

RPC_S_ENTRY_NOT_FOUND

0x000006E2

RPC_S_NAME_SERVICE_UNAVAILABLE

0x000006E3

RPC_S_INVALID_NAF_ID

0x000006E4

RPC_S_CANNOT_SUPPORT

0x000006E5

RPC_S_NO_CONTEXT_AVAILABLE

0x000006E6

RPC_S_INTERNAL_ERROR

0x000006E7

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

unknown.

The security context is invalid.

The authorization service is
unknown.

The entry is invalid.

The server endpoint cannot
perform the operation.

There are no more endpoints
available from the endpoint
mapper.

No interfaces have been
exported.

The entry name is incomplete.

The version option is invalid.

There are no more members.

There is nothing to unexport.

The interface was not found.

The entry already exists.

The entry is not found.

The name service is unavailable.

The network address family is
invalid.

The requested operation is not
supported.

No security context is available
to allow impersonation.

An internal error occurred in an
RPC.

The RPC server attempted an

264 / 497


Win32 error codes

RPC_S_ZERO_DIVIDE

0x000006E8

RPC_S_ADDRESS_ERROR

0x000006E9

RPC_S_FP_DIV_ZERO

0x000006EA

RPC_S_FP_UNDERFLOW

0x000006EB

RPC_S_FP_OVERFLOW

0x000006EC

RPC_X_NO_MORE_ENTRIES

0x000006ED

RPC_X_SS_CHAR_TRANS_OPEN_FAIL

0x000006EE

RPC_X_SS_CHAR_TRANS_SHORT_FILE

0x000006EF

RPC_X_SS_IN_NULL_CONTEXT

0x000006F1

RPC_X_SS_CONTEXT_DAMAGED

0x000006F2

RPC_X_SS_HANDLES_MISMATCH

0x000006F3

RPC_X_SS_CANNOT_GET_CALL_HANDLE

0x000006F4

RPC_X_NULL_REF_POINTER

0x000006F5

RPC_X_ENUM_VALUE_OUT_OF_RANGE

0x000006F6

RPC_X_BYTE_COUNT_TOO_SMALL

0x000006F7

RPC_X_BAD_STUB_DATA

0x000006F8

ERROR_INVALID_USER_BUFFER

0x000006F9

ERROR_UNRECOGNIZED_MEDIA

0x000006FA

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

integer division by zero.

An addressing error occurred in
the RPC server.

A floating-point operation at the
RPC server caused a division by
zero.

A floating-point underflow
occurred at the RPC server.

A floating-point overflow
occurred at the RPC server.

The list of RPC servers available
for the binding of auto handles
has been exhausted.

Unable to open the character
translation table file.

The file containing the character
translation table has fewer than
512 bytes.

A null context handle was passed
from the client to the host during
an RPC.

The context handle changed
during an RPC.

The binding handles passed to
an RPC do not match.

The stub is unable to get the
RPC handle.

A null reference pointer was
passed to the stub.

The enumeration value is out of
range.

The byte count is too small.

The stub received bad data.

The supplied user buffer is not
valid for the requested
operation.

The disk media is not
recognized. It might not be
formatted.

The workstation does not have a

265 / 497


Win32 error codes

ERROR_NO_TRUST_LSA_SECRET

0x000006FB

ERROR_NO_TRUST_SAM_ACCOUNT

0x000006FC

ERROR_TRUSTED_DOMAIN_FAILURE

0x000006FD

ERROR_TRUSTED_RELATIONSHIP_FAILURE

0x000006FE

ERROR_TRUST_FAILURE

0x000006FF

RPC_S_CALL_IN_PROGRESS

0x00000700

ERROR_NETLOGON_NOT_STARTED

0x00000701

ERROR_ACCOUNT_EXPIRED

0x00000702

ERROR_REDIRECTOR_HAS_OPEN_HANDLES

0x00000703

ERROR_PRINTER_DRIVER_ALREADY_INSTALLED

0x00000704

ERROR_UNKNOWN_PORT

0x00000705

ERROR_UNKNOWN_PRINTER_DRIVER

0x00000706

ERROR_UNKNOWN_PRINTPROCESSOR

0x00000707

ERROR_INVALID_SEPARATOR_FILE

0x00000708

ERROR_INVALID_PRIORITY

0x00000709

ERROR_INVALID_PRINTER_NAME

0x0000070A

ERROR_PRINTER_ALREADY_EXISTS

0x0000070B

ERROR_INVALID_PRINTER_COMMAND

Description

trust secret.

The security database on the
server does not have a computer
account for this workstation trust
relationship.

The trust relationship between
the primary domain and the
trusted domain failed.

The trust relationship between
this workstation and the primary
domain failed.

The network logon failed.

An RPC is already in progress for
this thread.

An attempt was made to log on,
but the network logon service
was not started.

The user's account has expired.

The redirector is in use and
cannot be unloaded.

The specified printer driver is
already installed.

The specified port is unknown.

The printer driver is unknown.

The print processor is unknown.

The specified separator file is
invalid.

The specified priority is invalid.

The printer name is invalid.

The printer already exists.

The printer command is invalid.

0x0000070C

The specified data type is invalid.

266 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Win32 error codes

ERROR_INVALID_DATATYPE

0x0000070D

ERROR_INVALID_ENVIRONMENT

0x0000070E

RPC_S_NO_MORE_BINDINGS

0x0000070F

ERROR_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT

0x00000710

ERROR_NOLOGON_WORKSTATION_TRUST_ACCOUNT

0x00000711

ERROR_NOLOGON_SERVER_TRUST_ACCOUNT

0x00000712

ERROR_DOMAIN_TRUST_INCONSISTENT

0x00000713

ERROR_SERVER_HAS_OPEN_HANDLES

0x00000714

ERROR_RESOURCE_DATA_NOT_FOUND

0x00000715

ERROR_RESOURCE_TYPE_NOT_FOUND

0x00000716

ERROR_RESOURCE_NAME_NOT_FOUND

0x00000717

ERROR_RESOURCE_LANG_NOT_FOUND

0x00000718

ERROR_NOT_ENOUGH_QUOTA

0x00000719

RPC_S_NO_INTERFACES

0x0000071A

RPC_S_CALL_CANCELLED

0x0000071B

RPC_S_BINDING_INCOMPLETE

0x0000071C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The environment specified is
invalid.

There are no more bindings.

The account used is an
interdomain trust account. Use
your global user account or local
user account to access this
server.

The account used is a computer
account. Use your global user
account or local user account to
access this server.

The account used is a server
trust account. Use your global
user account or local user
account to access this server.

The name or SID of the domain
specified is inconsistent with the
trust information for that
domain.

The server is in use and cannot
be unloaded.

The specified image file did not
contain a resource section.

The specified resource type
cannot be found in the image
file.

The specified resource name
cannot be found in the image
file.

The specified resource language
ID cannot be found in the image
file.

Not enough quota is available to
process this command.

No interfaces have been
registered.

The RPC was canceled.

The binding handle does not
contain all the required
information.

A communications failure

267 / 497


Win32 error codes

RPC_S_COMM_FAILURE

0x0000071D

RPC_S_UNSUPPORTED_AUTHN_LEVEL

0x0000071E

RPC_S_NO_PRINC_NAME

0x0000071F

RPC_S_NOT_RPC_ERROR

0x00000720

RPC_S_UUID_LOCAL_ONLY

0x00000721

RPC_S_SEC_PKG_ERROR

0x00000722

RPC_S_NOT_CANCELLED

0x00000723

RPC_X_INVALID_ES_ACTION

0x00000724

RPC_X_WRONG_ES_VERSION

0x00000725

RPC_X_WRONG_STUB_VERSION

0x00000726

RPC_X_INVALID_PIPE_OBJECT

0x00000727

RPC_X_WRONG_PIPE_ORDER

0x00000728

RPC_X_WRONG_PIPE_VERSION

0x0000076A

RPC_S_GROUP_MEMBER_NOT_FOUND

0x0000076B

EPT_S_CANT_CREATE

0x0000076C

RPC_S_INVALID_OBJECT

0x0000076D

ERROR_INVALID_TIME

0x0000076E

ERROR_INVALID_FORM_NAME

0x0000076F

ERROR_INVALID_FORM_SIZE

0x00000770

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

occurred during an RPC.

The requested authentication
level is not supported.

No principal name is registered.

The error specified is not a valid
Windows RPC error code.

A UUID that is valid only on this
computer has been allocated.

A security package-specific error
occurred.

The thread is not canceled.

Invalid operation on the
encoding/decoding handle.

Incompatible version of the
serializing package.

Incompatible version of the RPC
stub.

The RPC pipe object is invalid or
corrupted.

An invalid operation was
attempted on an RPC pipe
object.

Unsupported RPC pipe version.

The group member was not
found.

The endpoint mapper database
entry could not be created.

The object UUID is the nil UUID.

The specified time is invalid.

The specified form name is
invalid.

The specified form size is invalid.

The specified printer handle is

268 / 497


Win32 error codes

ERROR_ALREADY_WAITING

0x00000771

ERROR_PRINTER_DELETED

0x00000772

ERROR_INVALID_PRINTER_STATE

0x00000773

ERROR_PASSWORD_MUST_CHANGE

0x00000774

ERROR_DOMAIN_CONTROLLER_NOT_FOUND

0x00000775

ERROR_ACCOUNT_LOCKED_OUT

0x00000776

OR_INVALID_OXID

0x00000777

OR_INVALID_OID

0x00000778

OR_INVALID_SET

0x00000779

RPC_S_SEND_INCOMPLETE

0x0000077A

RPC_S_INVALID_ASYNC_HANDLE

0x0000077B

RPC_S_INVALID_ASYNC_CALL

0x0000077C

RPC_X_PIPE_CLOSED

0x0000077D

RPC_X_PIPE_DISCIPLINE_ERROR

0x0000077E

RPC_X_PIPE_EMPTY

0x0000077F

ERROR_NO_SITENAME

0x00000780

ERROR_CANT_ACCESS_FILE

0x00000781

ERROR_CANT_RESOLVE_FILENAME

0x00000782

RPC_S_ENTRY_TYPE_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

already being waited on.

The specified printer has been
deleted.

The state of the printer is
invalid.

The user's password must be
changed before logging on the
first time.

Could not find the domain
controller for this domain.

The referenced account is
currently locked out and cannot
be logged on to.

The object exporter specified
was not found.

The object specified was not
found.

The object set specified was not
found.

Some data remains to be sent in
the request buffer.

Invalid asynchronous RPC
handle.

Invalid asynchronous RPC call
handle for this operation.

The RPC pipe object has already
been closed.

The RPC call completed before all
pipes were processed.

No more data is available from
the RPC pipe.

No site name is available for this
machine.

The file cannot be accessed by
the system.

The name of the file cannot be
resolved by the system.

The entry is not of the expected
type.

269 / 497


Win32 error codes

0x00000783

RPC_S_NOT_ALL_OBJS_EXPORTED

0x00000784

RPC_S_INTERFACE_NOT_EXPORTED

0x00000785

RPC_S_PROFILE_NOT_ADDED

0x00000786

RPC_S_PRF_ELT_NOT_ADDED

0x00000787

RPC_S_PRF_ELT_NOT_REMOVED

0x00000788

RPC_S_GRP_ELT_NOT_ADDED

0x00000789

RPC_S_GRP_ELT_NOT_REMOVED

0x0000078A

ERROR_KM_DRIVER_BLOCKED

0x0000078B

ERROR_CONTEXT_EXPIRED

0x0000078C

ERROR_PER_USER_TRUST_QUOTA_EXCEEDED

0x0000078D

ERROR_ALL_USER_TRUST_QUOTA_EXCEEDED

0x0000078E

ERROR_USER_DELETE_TRUST_QUOTA_EXCEEDED

0x0000078F

ERROR_AUTHENTICATION_FIREWALL_FAILED

0x00000790

ERROR_REMOTE_PRINT_CONNECTIONS_BLOCKED

0x000007D0

ERROR_INVALID_PIXEL_FORMAT

0x000007D1

ERROR_BAD_DRIVER

0x000007D2

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Not all object UUIDs could be
exported to the specified entry.

The interface could not be
exported to the specified entry.

The specified profile entry could
not be added.

The specified profile element
could not be added.

The specified profile element
could not be removed.

The group element could not be
added.

The group element could not be
removed.

The printer driver is not
compatible with a policy enabled
on your computer that blocks
Windows NT 4.0 operating
system drivers.

The context has expired and can
no longer be used.

The current user's delegated
trust creation quota has been
exceeded.

The total delegated trust
creation quota has been
exceeded.

The current user's delegated
trust deletion quota has been
exceeded.

Logon failure: The machine you
are logging on to is protected by
an authentication firewall. The
specified account is not allowed
to authenticate to the machine.

Remote connections to the Print
Spooler are blocked by a policy
set on your machine.

The pixel format is invalid.

The specified driver is invalid.

The window style or class
attribute is invalid for this

270 / 497


Win32 error codes

ERROR_INVALID_WINDOW_STYLE

0x000007D3

ERROR_METAFILE_NOT_SUPPORTED

0x000007D4

ERROR_TRANSFORM_NOT_SUPPORTED

0x000007D5

ERROR_CLIPPING_NOT_SUPPORTED

0x000007DA

ERROR_INVALID_CMM

0x000007DB

ERROR_INVALID_PROFILE

0x000007DC

ERROR_TAG_NOT_FOUND

0x000007DD

ERROR_TAG_NOT_PRESENT

0x000007DE

ERROR_DUPLICATE_TAG

0x000007DF

ERROR_PROFILE_NOT_ASSOCIATED_WITH_DEVICE

0x000007E0

ERROR_PROFILE_NOT_FOUND

0x000007E1

ERROR_INVALID_COLORSPACE

0x000007E2

ERROR_ICM_NOT_ENABLED

0x000007E3

ERROR_DELETING_ICM_XFORM

0x000007E4

ERROR_INVALID_TRANSFORM

0x000007E5

ERROR_COLORSPACE_MISMATCH

0x000007E6

ERROR_INVALID_COLORINDEX

0x000007E7

ERROR_PROFILE_DOES_NOT_MATCH_DEVICE

0x00000836

NERR_NetNotStarted

0x00000837

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

operation.

The requested metafile operation
is not supported.

The requested transformation
operation is not supported.

The requested clipping operation
is not supported.

The specified color management
module is invalid.

The specified color profile is
invalid.

The specified tag was not found.

A required tag is not present.

The specified tag is already
present.

The specified color profile is not
associated with any device.

The specified color profile was
not found.

The specified color space is
invalid.

Image Color Management is not
enabled.

There was an error while
deleting the color transform.

The specified color transform is
invalid.

The specified transform does not
match the bitmap's color space.

The specified named color index
is not present in the profile.

The specified profile is intended
for a device of a different type
than the specified device.

The workstation driver is not
installed.

The server could not be located.

271 / 497


Win32 error codes

NERR_UnknownServer

0x00000838

NERR_ShareMem

0x00000839

NERR_NoNetworkResource

0x0000083A

NERR_RemoteOnly

0x0000083B

NERR_DevNotRedirected

0x0000083C

ERROR_CONNECTED_OTHER_PASSWORD

0x0000083D

ERROR_CONNECTED_OTHER_PASSWORD_DEFAULT

0x00000842

NERR_ServerNotStarted

0x00000843

NERR_ItemNotFound

0x00000844

NERR_UnknownDevDir

0x00000845

NERR_RedirectedPath

0x00000846

NERR_DuplicateShare

0x00000847

NERR_NoRoom

0x00000849

NERR_TooManyItems

0x0000084A

NERR_InvalidMaxUsers

0x0000084B

NERR_BufTooSmall

0x0000084F

NERR_RemoteErr

0x00000853

NERR_LanmanIniError

0x00000858

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An internal error occurred. The
network cannot access a shared
memory segment.

A network resource shortage
occurred.

This operation is not supported
on workstations.

The device is not connected.

The network connection was
made successfully, but the user
had to be prompted for a
password other than the one
originally specified.

The network connection was
made successfully using default
credentials.

The Server service is not started.

The queue is empty.

The device or directory does not
exist.

The operation is invalid on a
redirected resource.

The name has already been
shared.

The server is currently out of the
requested resource.

Requested addition of items
exceeds the maximum allowed.

The Peer service supports only
two simultaneous users.

The API return buffer is too
small.

A remote API error occurred.

An error occurred when opening
or reading the configuration file.

A general network error

272 / 497


Win32 error codes

NERR_NetworkError

0x00000859

NERR_WkstaInconsistentState

0x0000085A

NERR_WkstaNotStarted

0x0000085B

NERR_BrowserNotStarted

0x0000085C

NERR_InternalError

0x0000085D

NERR_BadTransactConfig

0x0000085E

NERR_InvalidAPI

0x0000085F

NERR_BadEventName

0x00000860

NERR_DupNameReboot

0x00000862

NERR_CfgCompNotFound

0x00000863

NERR_CfgParamNotFound

0x00000865

NERR_LineTooLong

0x00000866

NERR_QNotFound

0x00000867

NERR_JobNotFound

0x00000868

NERR_DestNotFound

0x00000869

NERR_DestExists

0x0000086A

NERR_QExists

0x0000086B

NERR_QNoRoom

0x0000086C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

occurred.

The Workstation service is in an
inconsistent state. Restart the
computer before restarting the
Workstation service.

The Workstation service has not
been started.

The requested information is not
available.

An internal error occurred.

The server is not configured for
transactions.

The requested API is not
supported on the remote server.

The event name is invalid.

The computer name already
exists on the network. Change it
and reboot the computer.

The specified component could
not be found in the configuration
information.

The specified parameter could
not be found in the configuration
information.

A line in the configuration file is
too long.

The printer does not exist.

The print job does not exist.

The printer destination cannot be
found.

The printer destination already
exists.

The print queue already exists.

No more printers can be added.

No more print jobs can be

273 / 497


Win32 error codes

NERR_JobNoRoom

0x0000086D

NERR_DestNoRoom

0x0000086E

NERR_DestIdle

0x0000086F

NERR_DestInvalidOp

0x00000870

NERR_ProcNoRespond

0x00000871

NERR_SpoolerNotLoaded

0x00000872

NERR_DestInvalidState

0x00000873

NERR_QinvalidState

0x00000874

NERR_JobInvalidState

0x00000875

NERR_SpoolNoMemory

0x00000876

NERR_DriverNotFound

0x00000877

NERR_DataTypeInvalid

0x00000878

NERR_ProcNotFound

0x00000884

NERR_ServiceTableLocked

0x00000885

NERR_ServiceTableFull

0x00000886

NERR_ServiceInstalled

0x00000887

NERR_ServiceEntryLocked

0x00000888

NERR_ServiceNotInstalled

0x00000889

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

added.

No more printer destinations can
be added.

This printer destination is idle
and cannot accept control
operations.

This printer destination request
contains an invalid control
function.

The print processor is not
responding.

The spooler is not running.

This operation cannot be
performed on the print
destination in its current state.

This operation cannot be
performed on the print queue in
its current state.

This operation cannot be
performed on the print job in its
current state.

A spooler memory allocation
failure occurred.

The device driver does not exist.

The data type is not supported
by the print processor.

The print processor is not
installed.

The service database is locked.

The service table is full.

The requested service has
already been started.

The service does not respond to
control actions.

The service has not been
started.

The service name is invalid.

274 / 497


Win32 error codes

NERR_BadServiceName

0x0000088A

NERR_ServiceCtlTimeout

0x0000088B

NERR_ServiceCtlBusy

0x0000088C

NERR_BadServiceProgName

0x0000088D

NERR_ServiceNotCtrl

0x0000088E

NERR_ServiceKillProc

0x0000088F

NERR_ServiceCtlNotValid

0x00000890

NERR_NotInDispatchTbl

0x00000891

NERR_BadControlRecv

0x00000892

NERR_ServiceNotStarting

0x00000898

NERR_AlreadyLoggedOn

0x00000899

NERR_NotLoggedOn

0x0000089A

NERR_BadUsername

0x0000089B

NERR_BadPassword

0x0000089C

NERR_UnableToAddName_W

0x0000089D

NERR_UnableToAddName_F

0x0000089E

NERR_UnableToDelName_W

0x0000089F

NERR_UnableToDelName_F

0x000008A1

NERR_LogonsPaused

0x000008A2

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The service is not responding to
the control function.

The service control is busy.

The configuration file contains an
invalid service program name.

The service could not be
controlled in its present state.

The service ended abnormally.

The requested pause or stop is
not valid for this service.

The service control dispatcher
could not find the service name
in the dispatch table.

The service control dispatcher
pipe read failed.

A thread for the new service
could not be created.

This workstation is already
logged on to the LAN.

The workstation is not logged on
to the LAN.

The user name or group name
parameter is invalid.

The password parameter is
invalid.

The logon processor did not add
the message alias.

The logon processor did not add
the message alias.

The logoff processor did not
delete the message alias.

The logoff processor did not
delete the message alias.

Network logons are paused.

A centralized logon server

275 / 497


Win32 error codes

NERR_LogonServerConflict

0x000008A3

NERR_LogonNoUserPath

0x000008A4

NERR_LogonScriptError

0x000008A6

NERR_StandaloneLogon

0x000008A7

NERR_LogonServerNotFound

0x000008A8

NERR_LogonDomainExists

0x000008A9

NERR_NonValidatedLogon

0x000008AB

NERR_ACFNotFound

0x000008AC

NERR_GroupNotFound

0x000008AD

NERR_UserNotFound

0x000008AE

NERR_ResourceNotFound

0x000008AF

NERR_GroupExists

0x000008B0

NERR_UserExists

0x000008B1

NERR_ResourceExists

0x000008B2

NERR_NotPrimary

0x000008B3

NERR_ACFNotLoaded

0x000008B4

NERR_ACFNoRoom

0x000008B5

NERR_ACFFileIOFail

0x000008B6

NERR_ACFTooManyLists

0x000008B7

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

conflict occurred.

The server is configured without
a valid user path.

An error occurred while loading
or running the logon script.

The logon server was not
specified. The computer will be
logged on as STANDALONE.

The logon server could not be
found.

There is already a logon domain
for this computer.

The logon server could not
validate the logon.

The security database could not
be found.

The group name could not be
found.

The user name could not be
found.

The resource name could not be
found.

The group already exists.

The user account already exists.

The resource permission list
already exists.

This operation is allowed only on
the PDC of the domain.

The security database has not
been started.

There are too many names in
the user accounts database.

A disk I/O failure occurred.

The limit of 64 entries per
resource was exceeded.

Deleting a user with a session is

276 / 497


Win32 error codes

NERR_UserLogon

0x000008B8

NERR_ACFNoParent

0x000008B9

NERR_CanNotGrowSegment

0x000008BA

NERR_SpeGroupOp

0x000008BB

NERR_NotInCache

0x000008BC

NERR_UserInGroup

0x000008BD

NERR_UserNotInGroup

0x000008BE

NERR_AccountUndefined

0x000008BF

NERR_AccountExpired

0x000008C0

NERR_InvalidWorkstation

0x000008C1

NERR_InvalidLogonHours

0x000008C2

NERR_PasswordExpired

0x000008C3

NERR_PasswordCantChange

0x000008C4

NERR_PasswordHistConflict

0x000008C5

NERR_PasswordTooShort

0x000008C6

NERR_PasswordTooRecent

0x000008C7

NERR_InvalidDatabase

0x000008C8

NERR_DatabaseUpToDate

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

not allowed.

The parent directory could not
be located.

Unable to add to the security
database session cache
segment.

This operation is not allowed on
this special group.

This user is not cached in the
user accounts database session
cache.

The user already belongs to this
group.

The user does not belong to this
group.

This user account is undefined.

This user account has expired.

The user is not allowed to log on
from this workstation.

The user is not allowed to log on
at this time.

The password of this user has
expired.

The password of this user cannot
change.

This password cannot be used
now.

The password does not meet the
password policy requirements.
Check the minimum password
length, password complexity,
and password history
requirements.

The password of this user is too
recent to change.

The security database is
corrupted.

No updates are necessary to this
replicant network or local
security database.

277 / 497


Win32 error codes

0x000008C9

NERR_SyncRequired

0x000008CA

NERR_UseNotFound

0x000008CB

NERR_BadAsgType

0x000008CC

NERR_DeviceIsShared

0x000008DE

NERR_NoComputerName

0x000008DF

NERR_MsgAlreadyStarted

0x000008E0

NERR_MsgInitFailed

0x000008E1

NERR_NameNotFound

0x000008E2

NERR_AlreadyForwarded

0x000008E3

NERR_AddForwarded

0x000008E4

NERR_AlreadyExists

0x000008E5

NERR_TooManyNames

0x000008E6

NERR_DelComputerName

0x000008E7

NERR_LocalForward

0x000008E8

NERR_GrpMsgProcessor

0x000008E9

NERR_PausedRemote

0x000008EA

NERR_BadReceive

0x000008EB

NERR_NameInUse

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

This replicant database is
outdated; synchronization is
required.

The network connection could
not be found.

This asg_type is invalid.

This device is currently being
shared.

The computer name could not be
added as a message alias. The
name might already exist on the
network.

The Messenger service is already
started.

The Messenger service failed to
start.

The message alias could not be
found on the network.

This message alias has already
been forwarded.

This message alias has been
added but is still forwarded.

This message alias already exists
locally.

The maximum number of added
message aliases has been
exceeded.

The computer name could not be
deleted.

Messages cannot be forwarded
back to the same workstation.

An error occurred in the domain
message processor.

The message was sent, but the
recipient has paused the
Messenger service.

The message was sent but not
received.

The message alias is currently in
use. Try again later.

278 / 497


Win32 error codes

0x000008EC

NERR_MsgNotStarted

0x000008ED

NERR_NotLocalName

0x000008EE

NERR_NoForwardName

0x000008EF

NERR_RemoteFull

0x000008F0

NERR_NameNotForwarded

0x000008F1

NERR_TruncatedBroadcast

0x000008F6

NERR_InvalidDevice

0x000008F7

NERR_WriteFault

0x000008F9

NERR_DuplicateName

0x000008FA

NERR_DeleteLater

0x000008FB

NERR_IncompleteDel

0x000008FC

NERR_MultipleNets

0x00000906

NERR_NetNameNotFound

0x00000907

NERR_DeviceNotShared

0x00000908

NERR_ClientNameNotFound

0x0000090A

NERR_FileIdNotFound

0x0000090B

NERR_ExecFailure

0x0000090C

NERR_TmpFile

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The Messenger service has not
been started.

The name is not on the local
computer.

The forwarded message alias
could not be found on the
network.

The message alias table on the
remote station is full.

Messages for this alias are not
currently being forwarded.

The broadcast message was
truncated.

This is an invalid device name.

A write fault occurred.

A duplicate message alias exists
on the network.

This message alias will be
deleted later.

The message alias was not
successfully deleted from all
networks.

This operation is not supported
on computers with multiple
networks.

This shared resource does not
exist.

This device is not shared.

A session does not exist with
that computer name.

There is not an open file with
that identification number.

A failure occurred when
executing a remote
administration command.

A failure occurred when opening
a remote temporary file.

279 / 497


Win32 error codes

0x0000090D

NERR_TooMuchData

0x0000090E

NERR_DeviceShareConflict

0x0000090F

NERR_BrowserTableIncomplete

0x00000910

NERR_NotLocalDomain

0x00000911

NERR_IsDfsShare

0x0000091B

NERR_DevInvalidOpCode

0x0000091C

NERR_DevNotFound

0x0000091D

NERR_DevNotOpen

0x0000091E

NERR_BadQueueDevString

0x0000091F

NERR_BadQueuePriority

0x00000921

NERR_NoCommDevs

0x00000922

NERR_QueueNotFound

0x00000924

NERR_BadDevString

0x00000925

NERR_BadDev

0x00000926

NERR_InUseBySpooler

0x00000927

NERR_CommDevInUse

0x0000092F

NERR_InvalidComputer

0x00000932

NERR_MaxLenExceeded

0x00000934

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The data returned from a remote
administration command has
been truncated to 64 KB.

This device cannot be shared as
both a spooled and a nonspooled
resource.

The information in the list of
servers might be incorrect.

The computer is not active in
this domain.

The share must be removed
from the Distributed File System
(DFS) before it can be deleted.

The operation is invalid for this
device.

This device cannot be shared.

This device was not open.

This device name list is invalid.

The queue priority is invalid.

There are no shared
communication devices.

The queue you specified does
not exist.

This list of devices is invalid.

The requested device is invalid.

This device is already in use by
the spooler.

This device is already in use as a
communication device.

This computer name is invalid.

The string and prefix specified
are too long.

This path component is invalid.

280 / 497


Win32 error codes

NERR_BadComponent

0x00000935

NERR_CantType

0x0000093A

NERR_TooManyEntries

0x00000942

NERR_ProfileFileTooBig

0x00000943

NERR_ProfileOffset

0x00000944

NERR_ProfileCleanup

0x00000945

NERR_ProfileUnknownCmd

0x00000946

NERR_ProfileLoadErr

0x00000947

NERR_ProfileSaveErr

0x00000949

NERR_LogOverflow

0x0000094A

NERR_LogFileChanged

0x0000094B

NERR_LogFileCorrupt

0x0000094C

NERR_SourceIsDir

0x0000094D

NERR_BadSource

0x0000094E

NERR_BadDest

0x0000094F

NERR_DifferentServers

0x00000951

NERR_RunSrvPaused

0x00000955

NERR_ErrCommRunSrv

0x00000957

NERR_ErrorExecingGhost

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Could not determine the type of
input.

The buffer for types is not big
enough.

Profile files cannot exceed 64 KB.

The start offset is out of range.

The system cannot delete
current connections to network
resources.

The system was unable to parse
the command line in this file.

An error occurred while loading
the profile file.

Errors occurred while saving the
profile file. The profile was
partially saved.

Log file %1 is full.

This log file has changed
between reads.

Log file %1 is corrupt.

The source path cannot be a
directory.

The source path is illegal.

The destination path is illegal.

The source and destination paths
are on different servers.

The Run server you requested is
paused.

An error occurred when
communicating with a Run
server.

An error occurred when starting
a background process.

281 / 497


Win32 error codes

0x00000958

NERR_ShareNotFound

0x00000960

NERR_InvalidLana

0x00000961

NERR_OpenFiles

0x00000962

NERR_ActiveConns

0x00000963

NERR_BadPasswordCore

0x00000964

NERR_DevInUse

0x00000965

NERR_LocalDrive

0x0000097E

NERR_AlertExists

0x0000097F

NERR_TooManyAlerts

0x00000980

NERR_NoSuchAlert

0x00000981

NERR_BadRecipient

0x00000982

NERR_AcctLimitExceeded

0x00000988

NERR_InvalidLogSeek

0x00000992

NERR_BadUasConfig

0x00000993

NERR_InvalidUASOp

0x00000994

NERR_LastAdmin

0x00000995

NERR_DCNotFound

0x00000996

NERR_LogonTrackingError

0x00000997

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The shared resource you are
connected to could not be found.

The LAN adapter number is
invalid.

There are open files on the
connection.

Active connections still exist.

This share name or password is
invalid.

The device is being accessed by
an active process.

The drive letter is in use locally.

The specified client is already
registered for the specified
event.

The alert table is full.

An invalid or nonexistent alert
name was raised.

The alert recipient is invalid.

A user's session with this server
has been deleted.

The log file does not contain the
requested record number.

The user accounts database is
not configured correctly.

This operation is not permitted
when the Net Logon service is
running.

This operation is not allowed on
the last administrative account.

Could not find the domain
controller for this domain.

Could not set logon information
for this user.

The Net Logon service has not
been started.

282 / 497


Win32 error codes

NERR_NetlogonNotStarted

0x00000998

NERR_CanNotGrowUASFile

0x00000999

NERR_TimeDiffAtDC

0x0000099A

NERR_PasswordMismatch

0x0000099C

NERR_NoSuchServer

0x0000099D

NERR_NoSuchSession

0x0000099E

NERR_NoSuchConnection

0x0000099F

NERR_TooManyServers

0x000009A0

NERR_TooManySessions

0x000009A1

NERR_TooManyConnections

0x000009A2

NERR_TooManyFiles

0x000009A3

NERR_NoAlternateServers

0x000009A6

NERR_TryDownLevel

0x000009B0

NERR_UPSDriverNotStarted

0x000009B1

NERR_UPSInvalidConfig

0x000009B2

NERR_UPSInvalidCommPort

0x000009B3

NERR_UPSSignalAsserted

0x000009B4

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unable to add to the user
accounts database.

This server's clock is not
synchronized with the PDC's
clock.

A password mismatch has been
detected.

The server identification does
not specify a valid server.

The session identification does
not specify a valid session.

The connection identification
does not specify a valid
connection.

There is no space for another
entry in the table of available
servers.

The server has reached the
maximum number of sessions it
supports.

The server has reached the
maximum number of
connections it supports.

The server cannot open more
files because it has reached its
maximum number.

There are no alternate servers
registered on this server.

Try the down-level (remote
admin protocol) version of API
instead.

The uninterruptible power supply
(UPS) driver could not be
accessed by the UPS service.

The UPS service is not
configured correctly.

The UPS service could not access
the specified Comm Port.

The UPS indicated a line fail or
low battery situation. Service not
started.

The UPS service failed to
perform a system shut down.

283 / 497


Win32 error codes

NERR_UPSShutdownFailed

0x000009C4

NERR_BadDosRetCode

0x000009C5

NERR_ProgNeedsExtraMem

0x000009C6

NERR_BadDosFunction

0x000009C7

NERR_RemoteBootFailed

0x000009C8

NERR_BadFileCheckSum

0x000009C9

NERR_NoRplBootSystem

0x000009CA

NERR_RplLoadrNetBiosErr

0x000009CB

NERR_RplLoadrDiskErr

0x000009CC

NERR_ImageParamErr

0x000009CD

NERR_TooManyImageParams

0x000009CE

NERR_NonDosFloppyUsed

0x000009CF

NERR_RplBootRestart

0x000009D0

NERR_RplSrvrCallFailed

0x000009D1

NERR_CantConnectRplSrvr

0x000009D2

NERR_CantOpenImageFile

0x000009D3

NERR_CallingRplSrvr

0x000009D4

NERR_StartingRplBoot

0x000009D5

NERR_RplBootServiceTerm

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The program below returned an
MS-DOS error code.

The program below needs more
memory.

The program below called an
unsupported MS-DOS function.

The workstation failed to boot.

The file below is corrupt.

No loader is specified in the
boot-block definition file.

NetBIOS returned an error: The
network control blocks (NCBs)
and Server Message Block (SMB)
are dumped above.

A disk I/O error occurred.

Image parameter substitution
failed.

Too many image parameters
cross disk sector boundaries.

The image was not generated
from an MS-DOS disk formatted
with /S.

Remote boot will be restarted
later.

The call to the Remoteboot
server failed.

Cannot connect to the
Remoteboot server.

Cannot open image file on the
Remoteboot server.

Connecting to the Remoteboot
server.

Connecting to the Remoteboot
server.

Remote boot service was
stopped, check the error log for

284 / 497


Win32 error codes

0x000009D6

NERR_RplBootStartFailed

0x000009D7

NERR_RPL_CONNECTED

0x000009F6

NERR_BrowserConfiguredToNotRun

0x00000A32

NERR_RplNoAdaptersStarted

0x00000A33

NERR_RplBadRegistry

0x00000A34

NERR_RplBadDatabase

0x00000A35

NERR_RplRplfilesShare

0x00000A36

NERR_RplNotRplServer

0x00000A37

NERR_RplCannotEnum

0x00000A38

NERR_RplWkstaInfoCorrupted

0x00000A39

NERR_RplWkstaNotFound

0x00000A3A

NERR_RplWkstaNameUnavailable

0x00000A3B

NERR_RplProfileInfoCorrupted

0x00000A3C

NERR_RplProfileNotFound

0x00000A3D

NERR_RplProfileNameUnavailable

0x00000A3E

NERR_RplProfileNotEmpty

0x00000A3F

NERR_RplConfigInfoCorrupted

0x00000A40

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the cause of the problem.

Remote boot startup failed;
check the error log for the cause
of the problem.

A second connection to a
Remoteboot resource is not
allowed.

The browser service was
configured with
MaintainServerList=No.

Service failed to start because
none of the network adapters
started with this service.

Service failed to start due to bad
startup information in the
registry.

Service failed to start because its
database is absent or corrupt.

Service failed to start because
the RPLFILES share is absent.

Service failed to start because
the RPLUSER group is absent.

Cannot enumerate service
records.

Workstation record information
has been corrupted.

Workstation record was not
found.

Workstation name is in use by
some other workstation.

Profile record information has
been corrupted.

Profile record was not found.

Profile name is in use by some
other profile.

There are workstations using this
profile.

Configuration record information
has been corrupted.

Configuration record was not

285 / 497


Win32 error codes

NERR_RplConfigNotFound

0x00000A41

NERR_RplAdapterInfoCorrupted

0x00000A42

NERR_RplInternal

0x00000A43

NERR_RplVendorInfoCorrupted

0x00000A44

NERR_RplBootInfoCorrupted

0x00000A45

NERR_RplWkstaNeedsUserAcct

0x00000A46

NERR_RplNeedsRPLUSERAcct

0x00000A47

NERR_RplBootNotFound

0x00000A48

NERR_RplIncompatibleProfile

0x00000A49

NERR_RplAdapterNameUnavailable

0x00000A4A

NERR_RplConfigNotEmpty

0x00000A4B

NERR_RplBootInUse

0x00000A4C

NERR_RplBackupDatabase

0x00000A4D

NERR_RplAdapterNotFound

0x00000A4E

NERR_RplVendorNotFound

0x00000A4F

NERR_RplVendorNameUnavailable

0x00000A50

NERR_RplBootNameUnavailable

0x00000A51

NERR_RplConfigNameUnavailable

0x00000A64

NERR_DfsInternalCorruption

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

found.

Adapter ID record information
has been corrupted.

An internal service error has
occurred.

Vendor ID record information
has been corrupted.

Boot block record information
has been corrupted.

The user account for this
workstation record is missing.

The RPLUSER local group could
not be found.

Boot block record was not found.

Chosen profile is incompatible
with this workstation.

Chosen network adapter ID is in
use by some other workstation.

There are profiles using this
configuration.

There are workstations, profiles,
or configurations using this boot
block.

Service failed to back up the
Remoteboot database.

Adapter record was not found.

Vendor record was not found.

Vendor name is in use by some
other vendor record.

The boot name or vendor ID is in
use by some other boot block
record.

The configuration name is in use
by some other configuration.

The internal database
maintained by the DFS service is
corrupt.

286 / 497


Win32 error codes

0x00000A65

NERR_DfsVolumeDataCorrupt

0x00000A66

NERR_DfsNoSuchVolume

0x00000A67

NERR_DfsVolumeAlreadyExists

0x00000A68

NERR_DfsAlreadyShared

0x00000A69

NERR_DfsNoSuchShare

0x00000A6A

NERR_DfsNotALeafVolume

0x00000A6B

NERR_DfsLeafVolume

0x00000A6C

NERR_DfsVolumeHasMultipleServers

0x00000A6D

NERR_DfsCantCreateJunctionPoint

0x00000A6E

NERR_DfsServerNotDfsAware

0x00000A6F

NERR_DfsBadRenamePath

0x00000A70

NERR_DfsVolumeIsOffline

0x00000A71

NERR_DfsNoSuchServer

0x00000A72

NERR_DfsCyclicalName

0x00000A73

NERR_DfsNotSupportedInServerDfs

0x00000A74

NERR_DfsDuplicateService

0x00000A75

NERR_DfsCantRemoveLastServerShare

0x00000A76

NERR_DfsVolumeIsInterDfs

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

One of the records in the internal
DFS database is corrupt.

There is no DFS name whose
entry path matches the input
entry path.

A root or link with the given
name already exists.

The server share specified is
already shared in the DFS.

The indicated server share does
not support the indicated DFS
namespace.

The operation is not valid in this
portion of the namespace.

The operation is not valid in this
portion of the namespace.

The operation is ambiguous
because the link has multiple
servers.

Unable to create a link.

The server is not DFS-aware.

The specified rename target path
is invalid.

The specified DFS link is offline.

The specified server is not a
server for this link.

A cycle in the DFS name was
detected.

The operation is not supported
on a server-based DFS.

This link is already supported by
the specified server share.

Cannot remove the last server
share supporting this root or
link.

The operation is not supported
for an inter-DFS link.

287 / 497


Win32 error codes

0x00000A77

NERR_DfsInconsistent

0x00000A78

NERR_DfsServerUpgraded

0x00000A79

NERR_DfsDataIsIdentical

0x00000A7A

NERR_DfsCantRemoveDfsRoot

0x00000A7B

NERR_DfsChildOrParentInDfs

0x00000A82

NERR_DfsInternalError

0x00000A83

NERR_SetupAlreadyJoined

0x00000A84

NERR_SetupNotJoined

0x00000A85

NERR_SetupDomainController

0x00000A86

NERR_DefaultJoinRequired

0x00000A87

NERR_InvalidWorkgroupName

0x00000A88

NERR_NameUsesIncompatibleCodePage

0x00000A89

NERR_ComputerAccountNotFound

0x00000A8A

NERR_PersonalSku

0x00000A8D

NERR_PasswordMustChange

0x00000A8E

NERR_AccountLockedOut

0x00000A8F

NERR_PasswordTooLong

0x00000A90

NERR_PasswordNotComplexEnough

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The internal state of the DFS
Service has become inconsistent.

The DFS Service has been
installed on the specified server.

The DFS data being reconciled is
identical.

The DFS root cannot be deleted.
Uninstall DFS if required.

A child or parent directory of the
share is already in a DFS.

DFS internal error.

This machine is already joined to
a domain.

This machine is not currently
joined to a domain.

This machine is a domain
controller and cannot be
unjoined from a domain.

The destination domain
controller does not support
creating machine accounts in
organizational units (OUs).

The specified workgroup name is
invalid.

The specified computer name is
incompatible with the default
language used on the domain
controller.

The specified computer account
could not be found.

This version of Windows cannot
be joined to a domain.

The password must change at
the next logon.

The account is locked out.

The password is too long.

The password does not meet the
complexity policy.

288 / 497


Win32 error codes

0x00000A91

NERR_PasswordFilterError

0x00000BB8

ERROR_UNKNOWN_PRINT_MONITOR

0x00000BB9

ERROR_PRINTER_DRIVER_IN_USE

0x00000BBA

ERROR_SPOOL_FILE_NOT_FOUND

0x00000BBB

ERROR_SPL_NO_STARTDOC

0x00000BBC

ERROR_SPL_NO_ADDJOB

0x00000BBD

ERROR_PRINT_PROCESSOR_ALREADY_INSTALLED

0x00000BBE

ERROR_PRINT_MONITOR_ALREADY_INSTALLED

0x00000BBF

ERROR_INVALID_PRINT_MONITOR

0x00000BC0

ERROR_PRINT_MONITOR_IN_USE

0x00000BC1

ERROR_PRINTER_HAS_JOBS_QUEUED

0x00000BC2

ERROR_SUCCESS_REBOOT_REQUIRED

0x00000BC3

ERROR_SUCCESS_RESTART_REQUIRED

0x00000BC4

ERROR_PRINTER_NOT_FOUND

0x00000BC5

ERROR_PRINTER_DRIVER_WARNED

0x00000BC6

ERROR_PRINTER_DRIVER_BLOCKED

0x00000BC7

ERROR_PRINTER_DRIVER_PACKAGE_IN_USE

0x00000BC8

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The password does not meet the
requirements of the password
filter DLLs.

The specified print monitor is
unknown.

The specified printer driver is
currently in use.

The spool file was not found.

A StartDocPrinter call was not
issued.

An AddJob call was not issued.

The specified print processor has
already been installed.

The specified print monitor has
already been installed.

The specified print monitor does
not have the required functions.

The specified print monitor is
currently in use.

The requested operation is not
allowed when there are jobs
queued to the printer.

The requested operation is
successful. Changes will not be
effective until the system is
rebooted.

The requested operation is
successful. Changes will not be
effective until the service is
restarted.

No printers were found.

The printer driver is known to be
unreliable.

The printer driver is known to
harm the system.

The specified printer driver
package is currently in use.

Unable to find a core driver
package that is required by the

289 / 497


Win32 error codes

Description

ERROR_CORE_DRIVER_PACKAGE_NOT_FOUND

printer driver package.

0x00000BC9

ERROR_FAIL_REBOOT_REQUIRED

0x00000BCA

ERROR_FAIL_REBOOT_INITIATED

0x00000BCB
ERROR_PRINTER_DRIVER_DOWNLOAD_NEEDED

0x00000BCE

ERROR_PRINTER_NOT_SHAREABLE

0x00000F6E

ERROR_IO_REISSUE_AS_CACHED

0x00000FA0

ERROR_WINS_INTERNAL

0x00000FA1

ERROR_CAN_NOT_DEL_LOCAL_WINS

0x00000FA2

ERROR_STATIC_INIT

0x00000FA3

ERROR_INC_BACKUP

0x00000FA4

ERROR_FULL_BACKUP

0x00000FA5

ERROR_REC_NON_EXISTENT

0x00000FA6

ERROR_RPL_NOT_ALLOWED

0x00000FD2

PEERDIST_ERROR_CONTENTINFO_VERSION_UNSUPPORTED

0x00000FD3

PEERDIST_ERROR_CANNOT_PARSE_CONTENTINFO

0x00000FD4

PEERDIST_ERROR_MISSING_DATA

0x00000FD5

PEERDIST_ERROR_NO_MORE

0x00000FD6

PEERDIST_ERROR_NOT_INITIALIZED

0x00000FD7

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The requested operation failed. A
system reboot is required to roll
back changes made.

The requested operation failed. A
system reboot has been initiated
to roll back changes made.

The specified printer driver was
not found on the system and
needs to be downloaded.

The specified printer cannot be
shared.

Reissue the given operation as a
cached I/O operation.

Windows Internet Name Service
(WINS) encountered an error
while processing the command.

The local WINS cannot be
deleted.

The importation from the file
failed.

The backup failed. Was a full
backup done before?

The backup failed. Check the
directory to which you are
backing the database.

The name does not exist in the
WINS database.

Replication with a nonconfigured
partner is not allowed.

The version of the supplied
content information is not
supported.

The supplied content information
is malformed.

The requested data cannot be
found in local or peer caches.

No more data is available or
required.

The supplied object has not been
initialized.

The supplied object has already

290 / 497


Win32 error codes

PEERDIST_ERROR_ALREADY_INITIALIZED

0x00000FD8

PEERDIST_ERROR_SHUTDOWN_IN_PROGRESS

0x00000FD9

PEERDIST_ERROR_INVALIDATED

0x00000FDA

PEERDIST_ERROR_ALREADY_EXISTS

0x00000FDB

PEERDIST_ERROR_OPERATION_NOTFOUND

0x00000FDC

PEERDIST_ERROR_ALREADY_COMPLETED

0x00000FDD

PEERDIST_ERROR_OUT_OF_BOUNDS

0x00000FDE

PEERDIST_ERROR_VERSION_UNSUPPORTED

0x00000FDF

PEERDIST_ERROR_INVALID_CONFIGURATION

0x00000FE0

PEERDIST_ERROR_NOT_LICENSED

0x00000FE1

PEERDIST_ERROR_SERVICE_UNAVAILABLE

0x00001004

ERROR_DHCP_ADDRESS_CONFLICT

0x00001068

ERROR_WMI_GUID_NOT_FOUND

0x00001069

ERROR_WMI_INSTANCE_NOT_FOUND

0x0000106A

ERROR_WMI_ITEMID_NOT_FOUND

0x0000106B

ERROR_WMI_TRY_AGAIN

0x0000106C

ERROR_WMI_DP_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

been initialized.

A shutdown operation is already
in progress.

The supplied object has already
been invalidated.

An element already exists and
was not replaced.

Cannot cancel the requested
operation as it has already been
completed.

Cannot perform the requested
operation because it has already
been carried out.

An operation accessed data
beyond the bounds of valid data.

The requested version is not
supported.

A configuration value is invalid.

The SKU is not licensed.

PeerDist Service is still
initializing and will be available
shortly.

The Dynamic Host Configuration
Protocol (DHCP) client has
obtained an IP address that is
already in use on the network.
The local interface will be
disabled until the DHCP client
can obtain a new address.

The GUID passed was not
recognized as valid by a WMI
data provider.

The instance name passed was
not recognized as valid by a WMI
data provider.

The data item ID passed was not
recognized as valid by a WMI
data provider.

The WMI request could not be
completed and should be retried.

The WMI data provider could not
be located.

291 / 497


Win32 error codes

0x0000106D

ERROR_WMI_UNRESOLVED_INSTANCE_REF

0x0000106E

ERROR_WMI_ALREADY_ENABLED

0x0000106F

ERROR_WMI_GUID_DISCONNECTED

0x00001070

ERROR_WMI_SERVER_UNAVAILABLE

0x00001071

ERROR_WMI_DP_FAILED

0x00001072

ERROR_WMI_INVALID_MOF

0x00001073

ERROR_WMI_INVALID_REGINFO

0x00001074

ERROR_WMI_ALREADY_DISABLED

0x00001075

ERROR_WMI_READ_ONLY

0x00001076

ERROR_WMI_SET_FAILURE

0x000010CC

ERROR_INVALID_MEDIA

0x000010CD

ERROR_INVALID_LIBRARY

0x000010CE

ERROR_INVALID_MEDIA_POOL

0x000010CF

ERROR_DRIVE_MEDIA_MISMATCH

0x000010D0

ERROR_MEDIA_OFFLINE

0x000010D1

ERROR_LIBRARY_OFFLINE

0x000010D2

ERROR_EMPTY

0x000010D3

ERROR_NOT_EMPTY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The WMI data provider
references an instance set that
has not been registered.

The WMI data block or event
notification has already been
enabled.

The WMI data block is no longer
available.

The WMI data service is not
available.

The WMI data provider failed to
carry out the request.

The WMI Managed Object
Format (MOF) information is not
valid.

The WMI registration information
is not valid.

The WMI data block or event
notification has already been
disabled.

The WMI data item or data block
is read-only.

The WMI data item or data block
could not be changed.

The media identifier does not
represent a valid medium.

The library identifier does not
represent a valid library.

The media pool identifier does
not represent a valid media pool.

The drive and medium are not
compatible, or they exist in
different libraries.

The medium currently exists in
an offline library and must be
online to perform this operation.

The operation cannot be
performed on an offline library.

The library, drive, or media pool
is empty.

The library, drive, or media pool
must be empty to perform this

292 / 497


Win32 error codes

0x000010D4

ERROR_MEDIA_UNAVAILABLE

0x000010D5

ERROR_RESOURCE_DISABLED

0x000010D6

ERROR_INVALID_CLEANER

0x000010D7

ERROR_UNABLE_TO_CLEAN

0x000010D8

ERROR_OBJECT_NOT_FOUND

0x000010D9

ERROR_DATABASE_FAILURE

0x000010DA

ERROR_DATABASE_FULL

0x000010DB

ERROR_MEDIA_INCOMPATIBLE

0x000010DC

ERROR_RESOURCE_NOT_PRESENT

0x000010DD

ERROR_INVALID_OPERATION

0x000010DE

ERROR_MEDIA_NOT_AVAILABLE

0x000010DF

ERROR_DEVICE_NOT_AVAILABLE

0x000010E0

ERROR_REQUEST_REFUSED

0x000010E1

ERROR_INVALID_DRIVE_OBJECT

0x000010E2

ERROR_LIBRARY_FULL

0x000010E3

ERROR_MEDIUM_NOT_ACCESSIBLE

0x000010E4

ERROR_UNABLE_TO_LOAD_MEDIUM

0x000010E5

ERROR_UNABLE_TO_INVENTORY_DRIVE

0x000010E6

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

operation.

No media is currently available in
this media pool or library.

A resource required for this
operation is disabled.

The media identifier does not
represent a valid cleaner.

The drive cannot be cleaned or
does not support cleaning.

The object identifier does not
represent a valid object.

Unable to read from or write to
the database.

The database is full.

The medium is not compatible
with the device or media pool.

The resource required for this
operation does not exist.

The operation identifier is not
valid.

The media is not mounted or
ready for use.

The device is not ready for use.

The operator or administrator
has refused the request.

The drive identifier does not
represent a valid drive.

Library is full. No slot is available
for use.

The transport cannot access the
medium.

Unable to load the medium into
the drive.

Unable to retrieve the drive
status.

Unable to retrieve the slot

293 / 497


Win32 error codes

ERROR_UNABLE_TO_INVENTORY_SLOT

0x000010E7

ERROR_UNABLE_TO_INVENTORY_TRANSPORT

0x000010E8

ERROR_TRANSPORT_FULL

0x000010E9

ERROR_CONTROLLING_IEPORT

0x000010EA

ERROR_UNABLE_TO_EJECT_MOUNTED_MEDIA

0x000010EB

ERROR_CLEANER_SLOT_SET

0x000010EC

ERROR_CLEANER_SLOT_NOT_SET

0x000010ED

ERROR_CLEANER_CARTRIDGE_SPENT

0x000010EE

ERROR_UNEXPECTED_OMID

0x000010EF

ERROR_CANT_DELETE_LAST_ITEM

0x000010F0

ERROR_MESSAGE_EXCEEDS_MAX_SIZE

0x000010F1

ERROR_VOLUME_CONTAINS_SYS_FILES

0x000010F2

ERROR_INDIGENOUS_TYPE

0x000010F3

ERROR_NO_SUPPORTING_DRIVES

0x000010F4

ERROR_CLEANER_CARTRIDGE_INSTALLED

0x000010F5

ERROR_IEPORT_FULL

0x000010FE

ERROR_FILE_OFFLINE

0x000010FF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

status.

Unable to retrieve status about
the transport.

Cannot use the transport
because it is already in use.

Unable to open or close the
inject/eject port.

Unable to eject the medium
because it is in a drive.

A cleaner slot is already
reserved.

A cleaner slot is not reserved.

The cleaner cartridge has
performed the maximum number
of drive cleanings.

Unexpected on-medium
identifier.

The last remaining item in this
group or resource cannot be
deleted.

The message provided exceeds
the maximum size allowed for
this parameter.

The volume contains system or
paging files.

The media type cannot be
removed from this library
because at least one drive in the
library reports it can support this
media type.

This offline media cannot be
mounted on this system because
no enabled drives are present
that can be used.

A cleaner cartridge is present in
the tape library.

Cannot use the IEport because it
is not empty.

The remote storage service was
not able to recall the file.

The remote storage service is
not operational at this time.

294 / 497


Win32 error codes

Description

ERROR_REMOTE_STORAGE_NOT_ACTIVE

0x00001100

ERROR_REMOTE_STORAGE_MEDIA_ERROR

0x00001126

ERROR_NOT_A_REPARSE_POINT

0x00001127

ERROR_REPARSE_ATTRIBUTE_CONFLICT

0x00001128

ERROR_INVALID_REPARSE_DATA

0x00001129

ERROR_REPARSE_TAG_INVALID

0x0000112A

ERROR_REPARSE_TAG_MISMATCH

0x00001194

ERROR_VOLUME_NOT_SIS_ENABLED

0x00001389

ERROR_DEPENDENT_RESOURCE_EXISTS

0x0000138A

ERROR_DEPENDENCY_NOT_FOUND

0x0000138B

ERROR_DEPENDENCY_ALREADY_EXISTS

0x0000138C

ERROR_RESOURCE_NOT_ONLINE

0x0000138D

ERROR_HOST_NODE_NOT_AVAILABLE

0x0000138E

ERROR_RESOURCE_NOT_AVAILABLE

0x0000138F

ERROR_RESOURCE_NOT_FOUND

0x00001390

ERROR_SHUTDOWN_CLUSTER

0x00001391

ERROR_CANT_EVICT_ACTIVE_NODE

0x00001392

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The remote storage service
encountered a media error.

The file or directory is not a
reparse point.

The reparse point attribute
cannot be set because it conflicts
with an existing attribute.

The data present in the reparse
point buffer is invalid.

The tag present in the reparse
point buffer is invalid.

There is a mismatch between the
tag specified in the request and
the tag present in the reparse
point.

Single Instance Storage (SIS) is
not available on this volume.

The operation cannot be
completed because other
resources depend on this
resource.

The cluster resource dependency
cannot be found.

The cluster resource cannot be
made dependent on the specified
resource because it is already
dependent.

The cluster resource is not
online.

A cluster node is not available
for this operation.

The cluster resource is not
available.

The cluster resource could not be
found.

The cluster is being shut down.

A cluster node cannot be evicted
from the cluster unless the node
is down or it is the last node.

The object already exists.

295 / 497


Win32 error codes

ERROR_OBJECT_ALREADY_EXISTS

0x00001393

ERROR_OBJECT_IN_LIST

0x00001394

ERROR_GROUP_NOT_AVAILABLE

0x00001395

ERROR_GROUP_NOT_FOUND

0x00001396

ERROR_GROUP_NOT_ONLINE

0x00001397

ERROR_HOST_NODE_NOT_RESOURCE_OWNER

0x00001398

ERROR_HOST_NODE_NOT_GROUP_OWNER

0x00001399

ERROR_RESMON_CREATE_FAILED

0x0000139A

ERROR_RESMON_ONLINE_FAILED

0x0000139B

ERROR_RESOURCE_ONLINE

0x0000139C

ERROR_QUORUM_RESOURCE

0x0000139D

ERROR_NOT_QUORUM_CAPABLE

0x0000139E

ERROR_CLUSTER_SHUTTING_DOWN

0x0000139F

ERROR_INVALID_STATE

0x000013A0

ERROR_RESOURCE_PROPERTIES_STORED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The object is already in the list.

The cluster group is not available
for any new requests.

The cluster group could not be
found.

The operation could not be
completed because the cluster
group is not online.

The operation failed because
either the specified cluster node
is not the owner of the resource,
or the node is not a possible
owner of the resource.

The operation failed because
either the specified cluster node
is not the owner of the group, or
the node is not a possible owner
of the group.

The cluster resource could not be
created in the specified resource
monitor.

The cluster resource could not be
brought online by the resource
monitor.

The operation could not be
completed because the cluster
resource is online.

The cluster resource could not be
deleted or brought offline
because it is the quorum
resource.

The cluster could not make the
specified resource a quorum
resource because it is not
capable of being a quorum
resource.

The cluster software is shutting
down.

The group or resource is not in
the correct state to perform the
requested operation.

The properties were stored but
not all changes will take effect
until the next time the resource
is brought online.

296 / 497


Win32 error codes

0x000013A1

ERROR_NOT_QUORUM_CLASS

0x000013A2

ERROR_CORE_RESOURCE

0x000013A3

ERROR_QUORUM_RESOURCE_ONLINE_FAILED

0x000013A4

ERROR_QUORUMLOG_OPEN_FAILED

0x000013A5

ERROR_CLUSTERLOG_CORRUPT

0x000013A6

ERROR_CLUSTERLOG_RECORD_EXCEEDS_MAXSIZE

0x000013A7

ERROR_CLUSTERLOG_EXCEEDS_MAXSIZE

0x000013A8

ERROR_CLUSTERLOG_CHKPOINT_NOT_FOUND

0x000013A9

ERROR_CLUSTERLOG_NOT_ENOUGH_SPACE

0x000013AA

ERROR_QUORUM_OWNER_ALIVE

0x000013AB

ERROR_NETWORK_NOT_AVAILABLE

0x000013AC

ERROR_NODE_NOT_AVAILABLE

0x000013AD

ERROR_ALL_NODES_NOT_AVAILABLE

0x000013AE

ERROR_RESOURCE_FAILED

0x000013AF

ERROR_CLUSTER_INVALID_NODE

0x000013B0

ERROR_CLUSTER_NODE_EXISTS

0x000013B1

ERROR_CLUSTER_JOIN_IN_PROGRESS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The cluster could not make the
specified resource a quorum
resource because it does not
belong to a shared storage class.

The cluster resource could not be
deleted because it is a core
resource.

The quorum resource failed to
come online.

The quorum log could not be
created or mounted successfully.

The cluster log is corrupt.

The record could not be written
to the cluster log because it
exceeds the maximum size.

The cluster log exceeds its
maximum size.

No checkpoint record was found
in the cluster log.

The minimum required disk
space needed for logging is not
available.

The cluster node failed to take
control of the quorum resource
because the resource is owned
by another active node.

A cluster network is not available
for this operation.

A cluster node is not available
for this operation.

All cluster nodes must be
running to perform this
operation.

A cluster resource failed.

The cluster node is not valid.

The cluster node already exists.

A node is in the process of
joining the cluster.

297 / 497


Win32 error codes

0x000013B2

ERROR_CLUSTER_NODE_NOT_FOUND

0x000013B3

ERROR_CLUSTER_LOCAL_NODE_NOT_FOUND

0x000013B4

ERROR_CLUSTER_NETWORK_EXISTS

0x000013B5

ERROR_CLUSTER_NETWORK_NOT_FOUND

0x000013B6

ERROR_CLUSTER_NETINTERFACE_EXISTS

0x000013B7

ERROR_CLUSTER_NETINTERFACE_NOT_FOUND

0x000013B8

ERROR_CLUSTER_INVALID_REQUEST

0x000013B9

ERROR_CLUSTER_INVALID_NETWORK_PROVIDER

0x000013BA

ERROR_CLUSTER_NODE_DOWN

0x000013BB

ERROR_CLUSTER_NODE_UNREACHABLE

0x000013BC

ERROR_CLUSTER_NODE_NOT_MEMBER

0x000013BD

ERROR_CLUSTER_JOIN_NOT_IN_PROGRESS

0x000013BE

ERROR_CLUSTER_INVALID_NETWORK

0x000013C0

ERROR_CLUSTER_NODE_UP

0x000013C1

ERROR_CLUSTER_IPADDR_IN_USE

0x000013C2

ERROR_CLUSTER_NODE_NOT_PAUSED

0x000013C3

ERROR_CLUSTER_NO_SECURITY_CONTEXT

0x000013C4

ERROR_CLUSTER_NETWORK_NOT_INTERNAL

0x000013C5

ERROR_CLUSTER_NODE_ALREADY_UP

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The cluster node was not found.

The cluster local node
information was not found.

The cluster network already
exists.

The cluster network was not
found.

The cluster network interface
already exists.

The cluster network interface
was not found.

The cluster request is not valid
for this object.

The cluster network provider is
not valid.

The cluster node is down.

The cluster node is not
reachable.

The cluster node is not a
member of the cluster.

A cluster join operation is not in
progress.

The cluster network is not valid.

The cluster node is up.

The cluster IP address is already
in use.

The cluster node is not paused.

No cluster security context is
available.

The cluster network is not
configured for internal cluster
communication.

The cluster node is already up.

298 / 497


Win32 error codes

0x000013C6

ERROR_CLUSTER_NODE_ALREADY_DOWN

0x000013C7

ERROR_CLUSTER_NETWORK_ALREADY_ONLINE

0x000013C8

ERROR_CLUSTER_NETWORK_ALREADY_OFFLINE

0x000013C9

ERROR_CLUSTER_NODE_ALREADY_MEMBER

0x000013CA

ERROR_CLUSTER_LAST_INTERNAL_NETWORK

0x000013CB

ERROR_CLUSTER_NETWORK_HAS_DEPENDENTS

0x000013CC

ERROR_INVALID_OPERATION_ON_QUORUM

0x000013CD

ERROR_DEPENDENCY_NOT_ALLOWED

0x000013CE

ERROR_CLUSTER_NODE_PAUSED

0x000013CF

ERROR_NODE_CANT_HOST_RESOURCE

0x000013D0

ERROR_CLUSTER_NODE_NOT_READY

0x000013D1

ERROR_CLUSTER_NODE_SHUTTING_DOWN

0x000013D2

ERROR_CLUSTER_JOIN_ABORTED

0x000013D3

ERROR_CLUSTER_INCOMPATIBLE_VERSIONS

0x000013D4

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The cluster node is already
down.

The cluster network is already
online.

The cluster network is already
offline.

The cluster node is already a
member of the cluster.

The cluster network is the only
one configured for internal
cluster communication between
two or more active cluster
nodes. The internal
communication capability cannot
be removed from the network.

One or more cluster resources
depend on the network to
provide service to clients. The
client access capability cannot be
removed from the network.

This operation cannot be
performed on the cluster
resource because it is the
quorum resource. This quorum
resource cannot be brought
offline and its possible owners
list cannot be modified.

The cluster quorum resource is
not allowed to have any
dependencies.

The cluster node is paused.

The cluster resource cannot be
brought online. The owner node
cannot run this resource.

The cluster node is not ready to
perform the requested operation.

The cluster node is shutting
down.

The cluster join operation was
aborted.

The cluster join operation failed
due to incompatible software
versions between the joining
node and its sponsor.

This resource cannot be created

299 / 497


Win32 error codes

ERROR_CLUSTER_MAXNUM_OF_RESOURCES_EXCEEDED

0x000013D5

ERROR_CLUSTER_SYSTEM_CONFIG_CHANGED

0x000013D6

ERROR_CLUSTER_RESOURCE_TYPE_NOT_FOUND

0x000013D7

ERROR_CLUSTER_RESTYPE_NOT_SUPPORTED

0x000013D8

ERROR_CLUSTER_RESNAME_NOT_FOUND

0x000013D9

ERROR_CLUSTER_NO_RPC_PACKAGES_REGISTERED

0x000013DA

ERROR_CLUSTER_OWNER_NOT_IN_PREFLIST

0x000013DB

ERROR_CLUSTER_DATABASE_SEQMISMATCH

0x000013DC

ERROR_RESMON_INVALID_STATE

0x000013DD

ERROR_CLUSTER_GUM_NOT_LOCKER

0x000013DE

ERROR_QUORUM_DISK_NOT_FOUND

0x000013DF

ERROR_DATABASE_BACKUP_CORRUPT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

because the cluster has reached
the limit on the number of
resources it can monitor.

The system configuration
changed during the cluster join
or form operation. The join or
form operation was aborted.

The specified resource type was
not found.

The specified node does not
support a resource of this type.
This might be due to version
inconsistencies or due to the
absence of the resource DLL on
this node.

The specified resource name is
not supported by this resource
DLL. This might be due to a bad
(or changed) name supplied to
the resource DLL.

No authentication package could
be registered with the RPC
server.

You cannot bring the group
online because the owner of the
group is not in the preferred list
for the group. To change the
owner node for the group, move
the group.

The join operation failed because
the cluster database sequence
number has changed or is
incompatible with the locker
node. This can happen during a
join operation if the cluster
database was changing during
the join.

The resource monitor will not
allow the fail operation to be
performed while the resource is
in its current state. This can
happen if the resource is in a
pending state.

A non-locker code received a
request to reserve the lock for
making global updates.

The quorum disk could not be
located by the cluster service.

The backed-up cluster database
is possibly corrupt.

300 / 497


Win32 error codes

0x000013E0

ERROR_CLUSTER_NODE_ALREADY_HAS_DFS_ROOT

0x000013E1

ERROR_RESOURCE_PROPERTY_UNCHANGEABLE

0x00001702

ERROR_CLUSTER_MEMBERSHIP_INVALID_STATE

0x00001703

ERROR_CLUSTER_QUORUMLOG_NOT_FOUND

0x00001704

ERROR_CLUSTER_MEMBERSHIP_HALT

0x00001705

ERROR_CLUSTER_INSTANCE_ID_MISMATCH

0x00001706

ERROR_CLUSTER_NETWORK_NOT_FOUND_FOR_IP

0x00001707

ERROR_CLUSTER_PROPERTY_DATA_TYPE_MISMATCH

0x00001708

ERROR_CLUSTER_EVICT_WITHOUT_CLEANUP

0x00001709

ERROR_CLUSTER_PARAMETER_MISMATCH

0x0000170A

ERROR_NODE_CANNOT_BE_CLUSTERED

0x0000170B

ERROR_CLUSTER_WRONG_OS_VERSION

0x0000170C

ERROR_CLUSTER_CANT_CREATE_DUP_CLUSTER_NAME

0x0000170D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A DFS root already exists in this
cluster node.

An attempt to modify a resource
property failed because it
conflicts with another existing
property.

An operation was attempted that
is incompatible with the current
membership state of the node.

The quorum resource does not
contain the quorum log.

The membership engine
requested shutdown of the
cluster service on this node.

The join operation failed because
the cluster instance ID of the
joining node does not match the
cluster instance ID of the
sponsor node.

A matching cluster network for
the specified IP address could
not be found.

The actual data type of the
property did not match the
expected data type of the
property.

The cluster node was evicted
from the cluster successfully, but
the node was not cleaned up. To
determine what clean-up steps
failed and how to recover, see
the Failover Clustering
application event log using Event
Viewer.

Two or more parameter values
specified for a resource's
properties are in conflict.

This computer cannot be made a
member of a cluster.

This computer cannot be made a
member of a cluster because it
does not have the correct
version of Windows installed.

A cluster cannot be created with
the specified cluster name
because that cluster name is
already in use. Specify a
different name for the cluster.

The cluster configuration action

301 / 497


Win32 error codes

Description

ERROR_CLUSCFG_ALREADY_COMMITTED

has already been committed.

0x0000170E

ERROR_CLUSCFG_ROLLBACK_FAILED

0x0000170F

ERROR_CLUSCFG_SYSTEM_DISK_DRIVE_LETTER_CONFLICT

0x00001710

ERROR_CLUSTER_OLD_VERSION

0x00001711

ERROR_CLUSTER_MISMATCHED_COMPUTER_ACCT_NAME

0x00001712

ERROR_CLUSTER_NO_NET_ADAPTERS

0x00001713

ERROR_CLUSTER_POISONED

0x00001714

ERROR_CLUSTER_GROUP_MOVING

0x00001715

ERROR_CLUSTER_RESOURCE_TYPE_BUSY

0x00001716

ERROR_RESOURCE_CALL_TIMED_OUT

0x00001717

ERROR_INVALID_CLUSTER_IPV6_ADDRESS

0x00001718

ERROR_CLUSTER_INTERNAL_INVALID_FUNCTION

0x00001719

ERROR_CLUSTER_PARAMETER_OUT_OF_BOUNDS

0x0000171A

ERROR_CLUSTER_PARTIAL_SEND

0x0000171B

ERROR_CLUSTER_REGISTRY_INVALID_FUNCTION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The cluster configuration action
could not be rolled back.

The drive letter assigned to a
system disk on one node
conflicted with the drive letter
assigned to a disk on another
node.

One or more nodes in the cluster
are running a version of
Windows that does not support
this operation.

The name of the corresponding
computer account does not
match the network name for this
resource.

No network adapters are
available.

The cluster node has been
poisoned.

The group is unable to accept
the request because it is moving
to another node.

The resource type cannot accept
the request because it is too
busy performing another
operation.

The call to the cluster resource
DLL timed out.

The address is not valid for an
IPv6 Address resource. A global
IPv6 address is required, and it
must match a cluster network.
Compatibility addresses are not
permitted.

An internal cluster error
occurred. A call to an invalid
function was attempted.

A parameter value is out of
acceptable range.

A network error occurred while
sending data to another node in
the cluster. The number of bytes
transmitted was less than
required.

An invalid cluster registry
operation was attempted.

302 / 497


Win32 error codes

0x0000171C

ERROR_CLUSTER_INVALID_STRING_TERMINATION

0x0000171D

ERROR_CLUSTER_INVALID_STRING_FORMAT

0x0000171E

ERROR_CLUSTER_DATABASE_TRANSACTION_IN_PROGRESS

0x0000171F

ERROR_CLUSTER_DATABASE_TRANSACTION_NOT_IN_PROGRESS

0x00001720

ERROR_CLUSTER_NULL_DATA

0x00001721

ERROR_CLUSTER_PARTIAL_READ

0x00001722

ERROR_CLUSTER_PARTIAL_WRITE

0x00001723

ERROR_CLUSTER_CANT_DESERIALIZE_DATA

0x00001724

ERROR_DEPENDENT_RESOURCE_PROPERTY_CONFLICT

0x00001725

ERROR_CLUSTER_NO_QUORUM

0x00001726

ERROR_CLUSTER_INVALID_IPV6_NETWORK

0x00001727

ERROR_CLUSTER_INVALID_IPV6_TUNNEL_NETWORK

0x00001728

ERROR_QUORUM_NOT_ALLOWED_IN_THIS_GROUP

0x00001770

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An input string of characters is
not properly terminated.

An input string of characters is
not in a valid format for the data
it represents.

An internal cluster error
occurred. A cluster database
transaction was attempted while
a transaction was already in
progress.

An internal cluster error
occurred. There was an attempt
to commit a cluster database
transaction while no transaction
was in progress.

An internal cluster error
occurred. Data was not properly
initialized.

An error occurred while reading
from a stream of data. An
unexpected number of bytes was
returned.

An error occurred while writing
to a stream of data. The required
number of bytes could not be
written.

An error occurred while
deserializing a stream of cluster
data.

One or more property values for
this resource are in conflict with
one or more property values
associated with its dependent
resources.

A quorum of cluster nodes was
not present to form a cluster.

The cluster network is not valid
for an IPv6 address resource, or
it does not match the configured
address.

The cluster network is not valid
for an IPv6 tunnel resource.
Check the configuration of the IP
Address resource on which the
IPv6 tunnel resource depends.

Quorum resource cannot reside
in the available storage group.

The specified file could not be
encrypted.

303 / 497


Win32 error codes

ERROR_ENCRYPTION_FAILED

0x00001771

ERROR_DECRYPTION_FAILED

0x00001772

ERROR_FILE_ENCRYPTED

0x00001773

ERROR_NO_RECOVERY_POLICY

0x00001774

ERROR_NO_EFS

0x00001775

ERROR_WRONG_EFS

0x00001776

ERROR_NO_USER_KEYS

0x00001777

ERROR_FILE_NOT_ENCRYPTED

0x00001778

ERROR_NOT_EXPORT_FORMAT

0x00001779

ERROR_FILE_READ_ONLY

0x0000177A

ERROR_DIR_EFS_DISALLOWED

0x0000177B

ERROR_EFS_SERVER_NOT_TRUSTED

0x0000177C

ERROR_BAD_RECOVERY_POLICY

0x0000177D

ERROR_EFS_ALG_BLOB_TOO_BIG

0x0000177E

ERROR_VOLUME_NOT_SUPPORT_EFS

0x0000177F

ERROR_EFS_DISABLED

0x00001780

ERROR_EFS_VERSION_NOT_SUPPORT

0x00001781

ERROR_CS_ENCRYPTION_INVALID_SERVER_RESPONSE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified file could not be
decrypted.

The specified file is encrypted
and the user does not have the
ability to decrypt it.

There is no valid encryption
recovery policy configured for
this system.

The required encryption driver is
not loaded for this system.

The file was encrypted with a
different encryption driver than
is currently loaded.

There are no Encrypting File
System (EFS) keys defined for
the user.

The specified file is not
encrypted.

The specified file is not in the
defined EFS export format.

The specified file is read-only.

The directory has been disabled
for encryption.

The server is not trusted for
remote encryption operation.

Recovery policy configured for
this system contains invalid
recovery certificate.

The encryption algorithm used
on the source file needs a bigger
key buffer than the one on the
destination file.

The disk partition does not
support file encryption.

This machine is disabled for file
encryption.

A newer system is required to
decrypt this encrypted file.

The remote server sent an
invalid response for a file being
opened with client-side

304 / 497


Win32 error codes

0x00001782

ERROR_CS_ENCRYPTION_UNSUPPORTED_SERVER

0x00001783

ERROR_CS_ENCRYPTION_EXISTING_ENCRYPTED_FILE

0x00001784

ERROR_CS_ENCRYPTION_NEW_ENCRYPTED_FILE

0x00001785

ERROR_CS_ENCRYPTION_FILE_NOT_CSE

0x000017E6

ERROR_NO_BROWSER_SERVERS_FOUND

0x00001838

SCHED_E_SERVICE_NOT_LOCALSYSTEM

0x000019C8

ERROR_LOG_SECTOR_INVALID

0x000019C9

ERROR_LOG_SECTOR_PARITY_INVALID

0x000019CA

ERROR_LOG_SECTOR_REMAPPED

0x000019CB

ERROR_LOG_BLOCK_INCOMPLETE

0x000019CC

ERROR_LOG_INVALID_RANGE

0x000019CD

ERROR_LOG_BLOCKS_EXHAUSTED

0x000019CE

ERROR_LOG_READ_CONTEXT_INVALID

0x000019CF

ERROR_LOG_RESTART_INVALID

0x000019D0

ERROR_LOG_BLOCK_VERSION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

encryption.

Client-side encryption is not
supported by the remote server
even though it claims to support
it.

File is encrypted and should be
opened in client-side encryption
mode.

A new encrypted file is being
created and a $EFS needs to be
provided.

The SMB client requested a
client-side extension (CSE) file
system control (FSCTL) on a
non-CSE file.

The list of servers for this
workgroup is not currently
available

The Task Scheduler service must
be configured to run in the
System account to function
properly. Individual tasks can be
configured to run in other
accounts.

The log service encountered an
invalid log sector.

The log service encountered a
log sector with invalid block
parity.

The log service encountered a
remapped log sector.

The log service encountered a
partial or incomplete log block.

The log service encountered an
attempt to access data outside
the active log range.

The log service user marshaling
buffers are exhausted.

The log service encountered an
attempt to read from a
marshaling area with an invalid
read context.

The log service encountered an
invalid log restart area.

The log service encountered an
invalid log block version.

305 / 497


Win32 error codes

0x000019D1

ERROR_LOG_BLOCK_INVALID

0x000019D2

ERROR_LOG_READ_MODE_INVALID

0x000019D3

ERROR_LOG_NO_RESTART

0x000019D4

ERROR_LOG_METADATA_CORRUPT

0x000019D5

ERROR_LOG_METADATA_INVALID

0x000019D6

ERROR_LOG_METADATA_INCONSISTENT

0x000019D7

ERROR_LOG_RESERVATION_INVALID

0x000019D8

ERROR_LOG_CANT_DELETE

0x000019D9

ERROR_LOG_CONTAINER_LIMIT_EXCEEDED

0x000019DA

ERROR_LOG_START_OF_LOG

0x000019DB

ERROR_LOG_POLICY_ALREADY_INSTALLED

0x000019DC

ERROR_LOG_POLICY_NOT_INSTALLED

0x000019DD

ERROR_LOG_POLICY_INVALID

0x000019DE

ERROR_LOG_POLICY_CONFLICT

0x000019DF

ERROR_LOG_PINNED_ARCHIVE_TAIL

0x000019E0

ERROR_LOG_RECORD_NONEXISTENT

0x000019E1

ERROR_LOG_RECORDS_RESERVED_INVALID

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The log service encountered an
invalid log block.

The log service encountered an
attempt to read the log with an
invalid read mode.

The log service encountered a
log stream with no restart area.

The log service encountered a
corrupted metadata file.

The log service encountered a
metadata file that could not be
created by the log file system.

The log service encountered a
metadata file with inconsistent
data.

The log service encountered an
attempt to erroneous allocate or
dispose reservation space.

The log service cannot delete a
log file or file system container.

The log service has reached the
maximum allowable containers
allocated to a log file.

The log service has attempted to
read or write backward past the
start of the log.

The log policy could not be
installed because a policy of the
same type is already present.

The log policy in question was
not installed at the time of the
request.

The installed set of policies on
the log is invalid.

A policy on the log in question
prevented the operation from
completing.

Log space cannot be reclaimed
because the log is pinned by the
archive tail.

The log record is not a record in
the log file.

The number of reserved log
records or the adjustment of the
number of reserved log records

306 / 497


Win32 error codes

0x000019E2

ERROR_LOG_SPACE_RESERVED_INVALID

0x000019E3

ERROR_LOG_TAIL_INVALID

0x000019E4

ERROR_LOG_FULL

0x000019E5

ERROR_COULD_NOT_RESIZE_LOG

0x000019E6

ERROR_LOG_MULTIPLEXED

0x000019E7

ERROR_LOG_DEDICATED

0x000019E8

ERROR_LOG_ARCHIVE_NOT_IN_PROGRESS

0x000019E9

ERROR_LOG_ARCHIVE_IN_PROGRESS

0x000019EA

ERROR_LOG_EPHEMERAL

0x000019EB

ERROR_LOG_NOT_ENOUGH_CONTAINERS

0x000019EC

ERROR_LOG_CLIENT_ALREADY_REGISTERED

0x000019ED

ERROR_LOG_CLIENT_NOT_REGISTERED

0x000019EE

ERROR_LOG_FULL_HANDLER_IN_PROGRESS

0x000019EF

ERROR_LOG_CONTAINER_READ_FAILED

0x000019F0

ERROR_LOG_CONTAINER_WRITE_FAILED

0x000019F1

ERROR_LOG_CONTAINER_OPEN_FAILED

0x000019F2

ERROR_LOG_CONTAINER_STATE_INVALID

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

is invalid.

The reserved log space or the
adjustment of the log space is
invalid.

A new or existing archive tail or
base of the active log is invalid.

The log space is exhausted.

The log could not be set to the
requested size.

The log is multiplexed; no direct
writes to the physical log are
allowed.

The operation failed because the
log is a dedicated log.

The operation requires an
archive context.

Log archival is in progress.

The operation requires a non-
ephemeral log, but the log is
ephemeral.

The log must have at least two
containers before it can be read
from or written to.

A log client has already
registered on the stream.

A log client has not been
registered on the stream.

A request has already been
made to handle the log full
condition.

The log service encountered an
error when attempting to read
from a log container.

The log service encountered an
error when attempting to write
to a log container.

The log service encountered an
error when attempting to open a
log container.

The log service encountered an
invalid container state when

307 / 497


Win32 error codes

0x000019F3

ERROR_LOG_STATE_INVALID

0x000019F4

ERROR_LOG_PINNED

0x000019F5

ERROR_LOG_METADATA_FLUSH_FAILED

0x000019F6

ERROR_LOG_INCONSISTENT_SECURITY

0x000019F7

ERROR_LOG_APPENDED_FLUSH_FAILED

0x000019F8

ERROR_LOG_PINNED_RESERVATION

0x00001A2C

ERROR_INVALID_TRANSACTION

0x00001A2D

ERROR_TRANSACTION_NOT_ACTIVE

0x00001A2E

ERROR_TRANSACTION_REQUEST_NOT_VALID

0x00001A2F

ERROR_TRANSACTION_NOT_REQUESTED

0x00001A30

ERROR_TRANSACTION_ALREADY_ABORTED

0x00001A31

ERROR_TRANSACTION_ALREADY_COMMITTED

0x00001A32

ERROR_TM_INITIALIZATION_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

attempting a requested action.

The log service is not in the
correct state to perform a
requested action.

The log space cannot be
reclaimed because the log is
pinned.

The log metadata flush failed.

Security on the log and its
containers is inconsistent.

Records were appended to the
log or reservation changes were
made, but the log could not be
flushed.

The log is pinned due to
reservation consuming most of
the log space. Free some
reserved records to make space
available.

The transaction handle
associated with this operation is
not valid.

The requested operation was
made in the context of a
transaction that is no longer
active.

The requested operation is not
valid on the transaction object in
its current state.

The caller has called a response
API, but the response is not
expected because the
transaction manager did not
issue the corresponding request
to the caller.

It is too late to perform the
requested operation because the
transaction has already been
aborted.

It is too late to perform the
requested operation because the
transaction has already been
committed.

The transaction manager was
unable to be successfully
initialized. Transacted operations
are not supported.

308 / 497


Win32 error codes

0x00001A33

ERROR_RESOURCEMANAGER_READ_ONLY

0x00001A34

ERROR_TRANSACTION_NOT_JOINED

0x00001A35

ERROR_TRANSACTION_SUPERIOR_EXISTS

0x00001A36

ERROR_CRM_PROTOCOL_ALREADY_EXISTS

0x00001A37

ERROR_TRANSACTION_PROPAGATION_FAILED

0x00001A38

ERROR_CRM_PROTOCOL_NOT_FOUND

0x00001A39

ERROR_TRANSACTION_INVALID_MARSHALL_BUFFER

0x00001A3A

ERROR_CURRENT_TRANSACTION_NOT_VALID

0x00001A3B

ERROR_TRANSACTION_NOT_FOUND

0x00001A3C

ERROR_RESOURCEMANAGER_NOT_FOUND

0x00001A3D

ERROR_ENLISTMENT_NOT_FOUND

0x00001A3E

ERROR_TRANSACTIONMANAGER_NOT_FOUND

0x00001A3F

ERROR_TRANSACTIONMANAGER_NOT_ONLINE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified resource manager
made no changes or updates to
the resource under this
transaction.

The resource manager has
attempted to prepare a
transaction that it has not
successfully joined.

The transaction object already
has a superior enlistment, and
the caller attempted an
operation that would have
created a new superior. Only a
single superior enlistment is
allowed.

The resource manager tried to
register a protocol that already
exists.

The attempt to propagate the
transaction failed.

The requested propagation
protocol was not registered as a
CRM.

The buffer passed in to
PushTransaction or
PullTransaction is not in a valid
format.

The current transaction context
associated with the thread is not
a valid handle to a transaction
object.

The specified transaction object
could not be opened because it
was not found.

The specified resource manager
object could not be opened
because it was not found.

The specified enlistment object
could not be opened because it
was not found.

The specified transaction
manager object could not be
opened because it was not
found.

The specified resource manager
was unable to create an
enlistment because its
associated transaction manager
is not online.

309 / 497


Win32 error codes

0x00001A40

ERROR_TRANSACTIONMANAGER_RECOVERY_NAME_COLLISION

0x00001A90

ERROR_TRANSACTIONAL_CONFLICT

0x00001A91

ERROR_RM_NOT_ACTIVE

0x00001A92

ERROR_RM_METADATA_CORRUPT

0x00001A93

ERROR_DIRECTORY_NOT_RM

0x00001A95

ERROR_TRANSACTIONS_UNSUPPORTED_REMOTE

0x00001A96

ERROR_LOG_RESIZE_INVALID_SIZE

0x00001A97

ERROR_OBJECT_NO_LONGER_EXISTS

0x00001A98

ERROR_STREAM_MINIVERSION_NOT_FOUND

0x00001A99

ERROR_STREAM_MINIVERSION_NOT_VALID

0x00001A9A

ERROR_MINIVERSION_INACCESSIBLE_FROM_SPECIFIED_TRANSACTION

0x00001A9B

ERROR_CANT_OPEN_MINIVERSION_WITH_MODIFY_INTENT

0x00001A9C

ERROR_CANT_CREATE_MORE_STREAM_MINIVERSIONS

0x00001A9E

ERROR_REMOTE_FILE_VERSION_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified transaction
manager was unable to create
the objects contained in its log
file in the ObjectB namespace.
Therefore, the transaction
manager was unable to recover.

The function attempted to use a
name that is reserved for use by
another transaction.

Transaction support within the
specified file system resource
manager is not started or was
shut down due to an error.

The metadata of the resource
manager has been corrupted.
The resource manager will not
function.

The specified directory does not
contain a resource manager.

The remote server or share does
not support transacted file
operations.

The requested log size is invalid.

The object (file, stream, link)
corresponding to the handle has
been deleted by a transaction
savepoint rollback.

The specified file miniversion
was not found for this transacted
file open.

The specified file miniversion
was found but has been
invalidated. The most likely
cause is a transaction savepoint
rollback.

A miniversion can only be
opened in the context of the
transaction that created it.

It is not possible to open a
miniversion with modify access.

It is not possible to create any
more miniversions for this
stream.

The remote server sent
mismatching version numbers or
FID for a file opened with
transactions.

310 / 497


Win32 error codes

0x00001A9F

ERROR_HANDLE_NO_LONGER_VALID

0x00001AA0

ERROR_NO_TXF_METADATA

0x00001AA1

ERROR_LOG_CORRUPTION_DETECTED

0x00001AA2

ERROR_CANT_RECOVER_WITH_HANDLE_OPEN

0x00001AA3

ERROR_RM_DISCONNECTED

0x00001AA4

ERROR_ENLISTMENT_NOT_SUPERIOR

0x00001AA5

ERROR_RECOVERY_NOT_NEEDED

0x00001AA6

ERROR_RM_ALREADY_STARTED

0x00001AA7

ERROR_FILE_IDENTITY_NOT_PERSISTENT

0x00001AA8

ERROR_CANT_BREAK_TRANSACTIONAL_DEPENDENCY

0x00001AA9

ERROR_CANT_CROSS_RM_BOUNDARY

0x00001AAA

ERROR_TXF_DIR_NOT_EMPTY

0x00001AAB

ERROR_INDOUBT_TRANSACTIONS_EXIST

0x00001AAC

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The handle has been invalidated
by a transaction. The most likely
cause is the presence of memory
mapping on a file, or an open
handle when the transaction
ended or rolled back to
savepoint.

There is no transaction metadata
on the file.

The log data is corrupt.

The file cannot be recovered
because a handle is still open on
it.

The transaction outcome is
unavailable because the resource
manager responsible for it is
disconnected.

The request was rejected
because the enlistment in
question is not a superior
enlistment.

The transactional resource
manager is already consistent.
Recovery is not needed.

The transactional resource
manager has already been
started.

The file cannot be opened in a
transaction because its identity
depends on the outcome of an
unresolved transaction.

The operation cannot be
performed because another
transaction is depending on the
fact that this property will not
change.

The operation would involve a
single file with two transactional
resource managers and is
therefore not allowed.

The $Txf directory must be
empty for this operation to
succeed.

The operation would leave a
transactional resource manager
in an inconsistent state and is,
therefore, not allowed.

The operation could not be

311 / 497


Win32 error codes

ERROR_TM_VOLATILE

0x00001AAD

ERROR_ROLLBACK_TIMER_EXPIRED

0x00001AAE

ERROR_TXF_ATTRIBUTE_CORRUPT

0x00001AAF

ERROR_EFS_NOT_ALLOWED_IN_TRANSACTION

0x00001AB0

ERROR_TRANSACTIONAL_OPEN_NOT_ALLOWED

0x00001AB1

ERROR_LOG_GROWTH_FAILED

0x00001AB2

ERROR_TRANSACTED_MAPPING_UNSUPPORTED_REMOTE

0x00001AB3

ERROR_TXF_METADATA_ALREADY_PRESENT

0x00001AB4

ERROR_TRANSACTION_SCOPE_CALLBACKS_NOT_SET

0x00001AB5

ERROR_TRANSACTION_REQUIRED_PROMOTION

0x00001AB6

ERROR_CANNOT_EXECUTE_FILE_IN_TRANSACTION

0x00001AB7

ERROR_TRANSACTIONS_NOT_FROZEN

0x00001AB8

ERROR_TRANSACTION_FREEZE_IN_PROGRESS

0x00001AB9

ERROR_NOT_SNAPSHOT_VOLUME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

completed because the
transaction manager does not
have a log.

A rollback could not be
scheduled because a previously
scheduled rollback has already
been executed or is queued for
execution.

The transactional metadata
attribute on the file or directory
is corrupt and unreadable.

The encryption operation could
not be completed because a
transaction is active.

This object is not allowed to be
opened in a transaction.

An attempt to create space in
the transactional resource
manager's log failed. The failure
status has been recorded in the
event log.

Memory mapping (creating a
mapped section) to a remote file
under a transaction is not
supported.

Transaction metadata is already
present on this file and cannot
be superseded.

A transaction scope could not be
entered because the scope
handler has not been initialized.

Promotion was required to allow
the resource manager to enlist,
but the transaction was set to
disallow it.

This file is open for modification
in an unresolved transaction and
can be opened for execution only
by a transacted reader.

The request to thaw frozen
transactions was ignored
because transactions were not
previously frozen.

Transactions cannot be frozen
because a freeze is already in
progress.

The target volume is not a
snapshot volume. This operation
is only valid on a volume

312 / 497


Win32 error codes

0x00001ABA

ERROR_NO_SAVEPOINT_WITH_OPEN_FILES

0x00001ABB

ERROR_DATA_LOST_REPAIR

0x00001ABC

ERROR_SPARSE_NOT_ALLOWED_IN_TRANSACTION

0x00001ABD

ERROR_TM_IDENTITY_MISMATCH

0x00001ABE

ERROR_FLOATED_SECTION

0x00001ABF

ERROR_CANNOT_ACCEPT_TRANSACTED_WORK

0x00001AC0

ERROR_CANNOT_ABORT_TRANSACTIONS

0x00001B59

ERROR_CTX_WINSTATION_NAME_INVALID

0x00001B5A

ERROR_CTX_INVALID_PD

0x00001B5B

ERROR_CTX_PD_NOT_FOUND

0x00001B5C

ERROR_CTX_WD_NOT_FOUND

0x00001B5D

ERROR_CTX_CANNOT_MAKE_EVENTLOG_ENTRY

0x00001B5E

ERROR_CTX_SERVICE_NAME_COLLISION

0x00001B5F

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

mounted as a snapshot.

The savepoint operation failed
because files are open on the
transaction. This is not
permitted.

Windows has discovered
corruption in a file, and that file
has since been repaired. Data
loss might have occurred.

The sparse operation could not
be completed because a
transaction is active on the file.

The call to create a transaction
manager object failed because
the Tm Identity stored in the
logfile does not match the Tm
Identity that was passed in as an
argument.

I/O was attempted on a section
object that has been floated as a
result of a transaction ending.
There is no valid data.

The transactional resource
manager cannot currently accept
transacted work due to a
transient condition, such as low
resources.

The transactional resource
manager had too many
transactions outstanding that
could not be aborted. The
transactional resource manager
has been shut down.

The specified session name is
invalid.

The specified protocol driver is
invalid.

The specified protocol driver was
not found in the system path.

The specified terminal
connection driver was not found
in the system path.

A registry key for event logging
could not be created for this
session.

A service with the same name
already exists on the system.

A close operation is pending on

313 / 497


Win32 error codes

ERROR_CTX_CLOSE_PENDING

0x00001B60

ERROR_CTX_NO_OUTBUF

0x00001B61

ERROR_CTX_MODEM_INF_NOT_FOUND

0x00001B62

ERROR_CTX_INVALID_MODEMNAME

0x00001B63

ERROR_CTX_MODEM_RESPONSE_ERROR

0x00001B64

ERROR_CTX_MODEM_RESPONSE_TIMEOUT

0x00001B65

ERROR_CTX_MODEM_RESPONSE_NO_CARRIER

0x00001B66

ERROR_CTX_MODEM_RESPONSE_NO_DIALTONE

0x00001B67

ERROR_CTX_MODEM_RESPONSE_BUSY

0x00001B68

ERROR_CTX_MODEM_RESPONSE_VOICE

0x00001B69

ERROR_CTX_TD_ERROR

0x00001B6E

ERROR_CTX_WINSTATION_NOT_FOUND

0x00001B6F

ERROR_CTX_WINSTATION_ALREADY_EXISTS

0x00001B70

ERROR_CTX_WINSTATION_BUSY

0x00001B71

ERROR_CTX_BAD_VIDEO_MODE

0x00001B7B

ERROR_CTX_GRAPHICS_INVALID

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the session.

There are no free output buffers
available.

The MODEM.INF file was not
found.

The modem name was not found
in the MODEM.INF file.

The modem did not accept the
command sent to it. Verify that
the configured modem name
matches the attached modem.

The modem did not respond to
the command sent to it. Verify
that the modem is properly
cabled and turned on.

Carrier detect has failed or
carrier has been dropped due to
disconnect.

Dial tone not detected within the
required time. Verify that the
phone cable is properly attached
and functional.

Busy signal detected at remote
site on callback.

Voice detected at remote site on
callback.

Transport driver error.

The specified session cannot be
found.

The specified session name is
already in use.

The requested operation cannot
be completed because the
terminal connection is currently
busy processing a connect,
disconnect, reset, or delete
operation.

An attempt has been made to
connect to a session whose video
mode is not supported by the
current client.

The application attempted to
enable DOS graphics mode. DOS
graphics mode is not supported.

314 / 497


Win32 error codes

0x00001B7D

ERROR_CTX_LOGON_DISABLED

0x00001B7E

ERROR_CTX_NOT_CONSOLE

0x00001B80

ERROR_CTX_CLIENT_QUERY_TIMEOUT

0x00001B81

ERROR_CTX_CONSOLE_DISCONNECT

0x00001B82

ERROR_CTX_CONSOLE_CONNECT

0x00001B84

ERROR_CTX_SHADOW_DENIED

0x00001B85

ERROR_CTX_WINSTATION_ACCESS_DENIED

0x00001B89

ERROR_CTX_INVALID_WD

0x00001B8A

ERROR_CTX_SHADOW_INVALID

0x00001B8B

ERROR_CTX_SHADOW_DISABLED

0x00001B8C

ERROR_CTX_CLIENT_LICENSE_IN_USE

0x00001B8D

ERROR_CTX_CLIENT_LICENSE_NOT_SET

0x00001B8E

ERROR_CTX_LICENSE_NOT_AVAILABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Your interactive logon privilege
has been disabled. Contact your
administrator.

The requested operation can be
performed only on the system
console. This is most often the
result of a driver or system DLL
requiring direct console access.

The client failed to respond to
the server connect message.

Disconnecting the console
session is not supported.

Reconnecting a disconnected
session to the console is not
supported.

The request to control another
session remotely was denied.

The requested session access is
denied.

The specified terminal
connection driver is invalid.

The requested session cannot be
controlled remotely. This might
be because the session is
disconnected or does not
currently have a user logged on.

The requested session is not
configured to allow remote
control.

Your request to connect to this
terminal server has been
rejected. Your terminal server
client license number is currently
being used by another user. Call
your system administrator to
obtain a unique license number.

Your request to connect to this
terminal server has been
rejected. Your terminal server
client license number has not
been entered for this copy of the
terminal server client. Contact
your system administrator.

The number of connections to
this computer is limited and all
connections are in use right now.
Try connecting later or contact
your system administrator.

315 / 497


Win32 error codes

0x00001B8F

ERROR_CTX_LICENSE_CLIENT_INVALID

0x00001B90

ERROR_CTX_LICENSE_EXPIRED

0x00001B91

ERROR_CTX_SHADOW_NOT_RUNNING

0x00001B92

ERROR_CTX_SHADOW_ENDED_BY_MODE_CHANGE

0x00001B93

ERROR_ACTIVATION_COUNT_EXCEEDED

0x00001B94

ERROR_CTX_WINSTATIONS_DISABLED

0x00001B95

ERROR_CTX_ENCRYPTION_LEVEL_REQUIRED

0x00001B96

ERROR_CTX_SESSION_IN_USE

0x00001B97

ERROR_CTX_NO_FORCE_LOGOFF

0x00001B98

ERROR_CTX_ACCOUNT_RESTRICTION

0x00001B99

ERROR_RDP_PROTOCOL_ERROR

0x00001B9A

ERROR_CTX_CDM_CONNECT

0x00001B9B

ERROR_CTX_CDM_DISCONNECT

0x00001B9C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The client you are using is not
licensed to use this system. Your
logon request is denied.

The system license has expired.
Your logon request is denied.

Remote control could not be
terminated because the specified
session is not currently being
remotely controlled.

The remote control of the
console was terminated because
the display mode was changed.
Changing the display mode in a
remote control session is not
supported.

Activation has already been reset
the maximum number of times
for this installation. Your
activation timer will not be
cleared.

Remote logons are currently
disabled.

You do not have the proper
encryption level to access this
session.

The user %s\\%s is currently
logged on to this computer. Only
the current user or an
administrator can log on to this
computer.

The user %s\\%s is already
logged on to the console of this
computer. You do not have
permission to log in at this time.
To resolve this issue, contact
%s\\%s and have them log off.

Unable to log you on because of
an account restriction.

The RDP component %2
detected an error in the protocol
stream and has disconnected the
client.

The Client Drive Mapping Service
has connected on terminal
connection.

The Client Drive Mapping Service
has disconnected on terminal
connection.

The terminal server security

316 / 497


Win32 error codes

ERROR_CTX_SECURITY_LAYER_ERROR

0x00001B9D

ERROR_TS_INCOMPATIBLE_SESSIONS

0x00001F41

FRS_ERR_INVALID_API_SEQUENCE

0x00001F42

FRS_ERR_STARTING_SERVICE

0x00001F43

FRS_ERR_STOPPING_SERVICE

0x00001F44

FRS_ERR_INTERNAL_API

0x00001F45

FRS_ERR_INTERNAL

0x00001F46

FRS_ERR_SERVICE_COMM

0x00001F47

FRS_ERR_INSUFFICIENT_PRIV

0x00001F48

FRS_ERR_AUTHENTICATION

0x00001F49

FRS_ERR_PARENT_INSUFFICIENT_PRIV

0x00001F4A

FRS_ERR_PARENT_AUTHENTICATION

0x00001F4B

FRS_ERR_CHILD_TO_PARENT_COMM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

layer detected an error in the
protocol stream and has
disconnected the client.

The target session is
incompatible with the current
session.

The file replication service API
was called incorrectly.

The file replication service
cannot be started.

The file replication service
cannot be stopped.

The file replication service API
terminated the request. The
event log might contain more
information.

The file replication service
terminated the request. The
event log might contain more
information.

The file replication service
cannot be contacted. The event
log might contain more
information.

The file replication service
cannot satisfy the request
because the user has insufficient
privileges. The event log might
contain more information.

The file replication service
cannot satisfy the request
because authenticated RPC is not
available. The event log might
contain more information.

The file replication service
cannot satisfy the request
because the user has insufficient
privileges on the domain
controller. The event log might
contain more information.

The file replication service
cannot satisfy the request
because authenticated RPC is not
available on the domain
controller. The event log might
contain more information.

The file replication service
cannot communicate with the file
replication service on the domain
controller. The event log might

317 / 497


Win32 error codes

0x00001F4C

FRS_ERR_PARENT_TO_CHILD_COMM

0x00001F4D

FRS_ERR_SYSVOL_POPULATE

0x00001F4E

FRS_ERR_SYSVOL_POPULATE_TIMEOUT

0x00001F4F

FRS_ERR_SYSVOL_IS_BUSY

0x00001F50

FRS_ERR_SYSVOL_DEMOTE

0x00001F51

FRS_ERR_INVALID_SERVICE_PARAMETER

0x00002008

ERROR_DS_NOT_INSTALLED

0x00002009

ERROR_DS_MEMBERSHIP_EVALUATED_LOCALLY

0x0000200A

ERROR_DS_NO_ATTRIBUTE_OR_VALUE

0x0000200B

ERROR_DS_INVALID_ATTRIBUTE_SYNTAX

0x0000200C

ERROR_DS_ATTRIBUTE_TYPE_UNDEFINED

0x0000200D

ERROR_DS_ATTRIBUTE_OR_VALUE_EXISTS

0x0000200E

ERROR_DS_BUSY

0x0000200F

ERROR_DS_UNAVAILABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

contain more information.

The file replication service on the
domain controller cannot
communicate with the file
replication service on this
computer. The event log might
contain more information.

The file replication service
cannot populate the system
volume because of an internal
error. The event log might
contain more information.

The file replication service
cannot populate the system
volume because of an internal
time-out. The event log might
contain more information.

The file replication service
cannot process the request. The
system volume is busy with a
previous request.

The file replication service
cannot stop replicating the
system volume because of an
internal error. The event log
might contain more information.

The file replication service
detected an invalid parameter.

An error occurred while installing
the directory service. For more
information, see the event log.

The directory service evaluated
group memberships locally.

The specified directory service
attribute or value does not exist.

The attribute syntax specified to
the directory service is invalid.

The attribute type specified to
the directory service is not
defined.

The specified directory service
attribute or value already exists.

The directory service is busy.

The directory service is
unavailable.

318 / 497


Win32 error codes

0x00002010

ERROR_DS_NO_RIDS_ALLOCATED

0x00002011

ERROR_DS_NO_MORE_RIDS

0x00002012

ERROR_DS_INCORRECT_ROLE_OWNER

0x00002013

ERROR_DS_RIDMGR_INIT_ERROR

0x00002014

ERROR_DS_OBJ_CLASS_VIOLATION

0x00002015

ERROR_DS_CANT_ON_NON_LEAF

0x00002016

ERROR_DS_CANT_ON_RDN

0x00002017

ERROR_DS_CANT_MOD_OBJ_CLASS

0x00002018

ERROR_DS_CROSS_DOM_MOVE_ERROR

0x00002019

ERROR_DS_GC_NOT_AVAILABLE

0x0000201A

ERROR_SHARED_POLICY

0x0000201B

ERROR_POLICY_OBJECT_NOT_FOUND

0x0000201C

ERROR_POLICY_ONLY_IN_DS

0x0000201D

ERROR_PROMOTION_ACTIVE

0x0000201E

ERROR_NO_PROMOTION_ACTIVE

0x00002020

ERROR_DS_OPERATIONS_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The directory service was unable
to allocate a relative identifier.

The directory service has
exhausted the pool of relative
identifiers.

The requested operation could
not be performed because the
directory service is not the
master for that type of
operation.

The directory service was unable
to initialize the subsystem that
allocates relative identifiers.

The requested operation did not
satisfy one or more constraints
associated with the class of the
object.

The directory service can
perform the requested operation
only on a leaf object.

The directory service cannot
perform the requested operation
on the relative distinguished
name (RDN) attribute of an
object.

The directory service detected an
attempt to modify the object
class of an object.

The requested cross-domain
move operation could not be
performed.

Unable to contact the global
catalog (GC) server.

The policy object is shared and
can only be modified at the root.

The policy object does not exist.

The requested policy information
is only in the directory service.

A domain controller promotion is
currently active.

A domain controller promotion is
not currently active.

An operations error occurred.

319 / 497


Win32 error codes

0x00002021

ERROR_DS_PROTOCOL_ERROR

0x00002022

ERROR_DS_TIMELIMIT_EXCEEDED

0x00002023

ERROR_DS_SIZELIMIT_EXCEEDED

0x00002024

ERROR_DS_ADMIN_LIMIT_EXCEEDED

0x00002025

ERROR_DS_COMPARE_FALSE

0x00002026

ERROR_DS_COMPARE_TRUE

0x00002027

ERROR_DS_AUTH_METHOD_NOT_SUPPORTED

0x00002028

ERROR_DS_STRONG_AUTH_REQUIRED

0x00002029

ERROR_DS_INAPPROPRIATE_AUTH

0x0000202A

ERROR_DS_AUTH_UNKNOWN

0x0000202B

ERROR_DS_REFERRAL

0x0000202C

ERROR_DS_UNAVAILABLE_CRIT_EXTENSION

0x0000202D

ERROR_DS_CONFIDENTIALITY_REQUIRED

0x0000202E

ERROR_DS_INAPPROPRIATE_MATCHING

0x0000202F

ERROR_DS_CONSTRAINT_VIOLATION

0x00002030

ERROR_DS_NO_SUCH_OBJECT

0x00002031

ERROR_DS_ALIAS_PROBLEM

0x00002032

ERROR_DS_INVALID_DN_SYNTAX

0x00002033

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A protocol error occurred.

The time limit for this request
was exceeded.

The size limit for this request
was exceeded.

The administrative limit for this
request was exceeded.

The compare response was false.

The compare response was true.

The requested authentication
method is not supported by the
server.

A more secure authentication
method is required for this
server.

Inappropriate authentication.

The authentication mechanism is
unknown.

A referral was returned from the
server.

The server does not support the
requested critical extension.

This request requires a secure
connection.

Inappropriate matching.

A constraint violation occurred.

There is no such object on the
server.

There is an alias problem.

An invalid dn syntax has been
specified.

The object is a leaf object.

320 / 497


Win32 error codes

ERROR_DS_IS_LEAF

0x00002034

ERROR_DS_ALIAS_DEREF_PROBLEM

0x00002035

ERROR_DS_UNWILLING_TO_PERFORM

0x00002036

ERROR_DS_LOOP_DETECT

0x00002037

ERROR_DS_NAMING_VIOLATION

0x00002038

ERROR_DS_OBJECT_RESULTS_TOO_LARGE

0x00002039

ERROR_DS_AFFECTS_MULTIPLE_DSAS

0x0000203A

ERROR_DS_SERVER_DOWN

0x0000203B

ERROR_DS_LOCAL_ERROR

0x0000203C

ERROR_DS_ENCODING_ERROR

0x0000203D

ERROR_DS_DECODING_ERROR

0x0000203E

ERROR_DS_FILTER_UNKNOWN

0x0000203F

ERROR_DS_PARAM_ERROR

0x00002040

ERROR_DS_NOT_SUPPORTED

0x00002041

ERROR_DS_NO_RESULTS_RETURNED

0x00002042

ERROR_DS_CONTROL_NOT_FOUND

0x00002043

ERROR_DS_CLIENT_LOOP

0x00002044

ERROR_DS_REFERRAL_LIMIT_EXCEEDED

0x00002045

ERROR_DS_SORT_CONTROL_MISSING

0x00002046

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There is an alias dereferencing
problem.

The server is unwilling to process
the request.

A loop has been detected.

There is a naming violation.

The result set is too large.

The operation affects multiple
DSAs.

The server is not operational.

A local error has occurred.

An encoding error has occurred.

A decoding error has occurred.

The search filter cannot be
recognized.

One or more parameters are
illegal.

The specified method is not
supported.

No results were returned.

The specified control is not
supported by the server.

A referral loop was detected by
the client.

The preset referral limit was
exceeded.

The search requires a SORT
control.

The search results exceed the

321 / 497


Win32 error codes

ERROR_DS_OFFSET_RANGE_ERROR

0x0000206D

ERROR_DS_ROOT_MUST_BE_NC

0x0000206E

ERROR_DS_ADD_REPLICA_INHIBITED

0x0000206F

ERROR_DS_ATT_NOT_DEF_IN_SCHEMA

0x00002070

ERROR_DS_MAX_OBJ_SIZE_EXCEEDED

0x00002071

ERROR_DS_OBJ_STRING_NAME_EXISTS

0x00002072

ERROR_DS_NO_RDN_DEFINED_IN_SCHEMA

0x00002073

ERROR_DS_RDN_DOESNT_MATCH_SCHEMA

0x00002074

ERROR_DS_NO_REQUESTED_ATTS_FOUND

0x00002075

ERROR_DS_USER_BUFFER_TO_SMALL

0x00002076

ERROR_DS_ATT_IS_NOT_ON_OBJ

0x00002077

ERROR_DS_ILLEGAL_MOD_OPERATION

0x00002078

ERROR_DS_OBJ_TOO_LARGE

0x00002079

ERROR_DS_BAD_INSTANCE_TYPE

0x0000207A

ERROR_DS_MASTERDSA_REQUIRED

0x0000207B

ERROR_DS_OBJECT_CLASS_REQUIRED

0x0000207C

ERROR_DS_MISSING_REQUIRED_ATT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

offset range specified.

The root object must be the
head of a naming context. The
root object cannot have an
instantiated parent.

The add replica operation cannot
be performed. The naming
context must be writable to
create the replica.

A reference to an attribute that
is not defined in the schema
occurred.

The maximum size of an object
has been exceeded.

An attempt was made to add an
object to the directory with a
name that is already in use.

An attempt was made to add an
object of a class that does not
have an RDN defined in the
schema.

An attempt was made to add an
object using an RDN that is not
the RDN defined in the schema.

None of the requested attributes
were found on the objects.

The user buffer is too small.

The attribute specified in the
operation is not present on the
object.

Illegal modify operation. Some
aspect of the modification is not
permitted.

The specified object is too large.

The specified instance type is not
valid.

The operation must be
performed at a master DSA.

The object class attribute must
be specified.

A required attribute is missing.

322 / 497


Win32 error codes

0x0000207D

ERROR_DS_ATT_NOT_DEF_FOR_CLASS

0x0000207E

ERROR_DS_ATT_ALREADY_EXISTS

0x00002080

ERROR_DS_CANT_ADD_ATT_VALUES

0x00002081

ERROR_DS_SINGLE_VALUE_CONSTRAINT

0x00002082

ERROR_DS_RANGE_CONSTRAINT

0x00002083

ERROR_DS_ATT_VAL_ALREADY_EXISTS

0x00002084

ERROR_DS_CANT_REM_MISSING_ATT

0x00002085

ERROR_DS_CANT_REM_MISSING_ATT_VAL

0x00002086

ERROR_DS_ROOT_CANT_BE_SUBREF

0x00002087

ERROR_DS_NO_CHAINING

0x00002088

ERROR_DS_NO_CHAINED_EVAL

0x00002089

ERROR_DS_NO_PARENT_OBJECT

0x0000208A

ERROR_DS_PARENT_IS_AN_ALIAS

0x0000208B

ERROR_DS_CANT_MIX_MASTER_AND_REPS

0x0000208C

ERROR_DS_CHILDREN_EXIST

0x0000208D

ERROR_DS_OBJ_NOT_FOUND

0x0000208E

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An attempt was made to modify
an object to include an attribute
that is not legal for its class.

The specified attribute is already
present on the object.

The specified attribute is not
present, or has no values.

Multiple values were specified for
an attribute that can have only
one value.

A value for the attribute was not
in the acceptable range of
values.

The specified value already
exists.

The attribute cannot be removed
because it is not present on the
object.

The attribute value cannot be
removed because it is not
present on the object.

The specified root object cannot
be a subreference.

Chaining is not permitted.

Chained evaluation is not
permitted.

The operation could not be
performed because the object's
parent is either uninstantiated or
deleted.

Having a parent that is an alias
is not permitted. Aliases are leaf
objects.

The object and parent must be
of the same type, either both
masters or both replicas.

The operation cannot be
performed because child objects
exist. This operation can only be
performed on a leaf object.

Directory object not found.

The aliased object is missing.

323 / 497


Win32 error codes

ERROR_DS_ALIASED_OBJ_MISSING

0x0000208F

ERROR_DS_BAD_NAME_SYNTAX

0x00002090

ERROR_DS_ALIAS_POINTS_TO_ALIAS

0x00002091

ERROR_DS_CANT_DEREF_ALIAS

0x00002092

ERROR_DS_OUT_OF_SCOPE

0x00002093

ERROR_DS_OBJECT_BEING_REMOVED

0x00002094

ERROR_DS_CANT_DELETE_DSA_OBJ

0x00002095

ERROR_DS_GENERIC_ERROR

0x00002096

ERROR_DS_DSA_MUST_BE_INT_MASTER

0x00002097

ERROR_DS_CLASS_NOT_DSA

0x00002098

ERROR_DS_INSUFF_ACCESS_RIGHTS

0x00002099

ERROR_DS_ILLEGAL_SUPERIOR

0x0000209A

ERROR_DS_ATTRIBUTE_OWNED_BY_SAM

0x0000209B

ERROR_DS_NAME_TOO_MANY_PARTS

0x0000209C

ERROR_DS_NAME_TOO_LONG

0x0000209D

ERROR_DS_NAME_VALUE_TOO_LONG

0x0000209E

ERROR_DS_NAME_UNPARSEABLE

0x0000209F

ERROR_DS_NAME_TYPE_UNKNOWN

0x000020A0

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The object name has bad syntax.

An alias is not permitted to refer
to another alias.

The alias cannot be
dereferenced.

The operation is out of scope.

The operation cannot continue
because the object is in the
process of being removed.

The DSA object cannot be
deleted.

A directory service error has
occurred.

The operation can only be
performed on an internal master
DSA object.

The object must be of class DSA.

Insufficient access rights to
perform the operation.

The object cannot be added
because the parent is not on the
list of possible superiors.

Access to the attribute is not
permitted because the attribute
is owned by the SAM.

The name has too many parts.

The name is too long.

The name value is too long.

The directory service
encountered an error parsing a
name.

The directory service cannot get
the attribute type for a name.

The name does not identify an
object; the name identifies a

324 / 497


Win32 error codes

ERROR_DS_NOT_AN_OBJECT

0x000020A1

ERROR_DS_SEC_DESC_TOO_SHORT

0x000020A2

ERROR_DS_SEC_DESC_INVALID

0x000020A3

ERROR_DS_NO_DELETED_NAME

0x000020A4

ERROR_DS_SUBREF_MUST_HAVE_PARENT

0x000020A5

ERROR_DS_NCNAME_MUST_BE_NC

0x000020A6

ERROR_DS_CANT_ADD_SYSTEM_ONLY

0x000020A7

ERROR_DS_CLASS_MUST_BE_CONCRETE

0x000020A8

ERROR_DS_INVALID_DMD

0x000020A9

ERROR_DS_OBJ_GUID_EXISTS

0x000020AA

ERROR_DS_NOT_ON_BACKLINK

0x000020AB

ERROR_DS_NO_CROSSREF_FOR_NC

0x000020AC

ERROR_DS_SHUTTING_DOWN

0x000020AD

ERROR_DS_UNKNOWN_OPERATION

0x000020AE

ERROR_DS_INVALID_ROLE_OWNER

0x000020AF

ERROR_DS_COULDNT_CONTACT_FSMO

0x000020B0

ERROR_DS_CROSS_NC_DN_RENAME

0x000020B1

ERROR_DS_CANT_MOD_SYSTEM_ONLY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

phantom.

The security descriptor is too
short.

The security descriptor is invalid.

Failed to create name for deleted
object.

The parent of a new
subreference must exist.

The object must be a naming
context.

It is not permitted to add an
attribute that is owned by the
system.

The class of the object must be
structural; you cannot
instantiate an abstract class.

The schema object could not be
found.

A local object with this GUID
(dead or alive) already exists.

The operation cannot be
performed on a back link.

The cross-reference for the
specified naming context could
not be found.

The operation could not be
performed because the directory
service is shutting down.

The directory service request is
invalid.

The role owner attribute could
not be read.

The requested Flexible Single
Master Operations (FSMO)
operation failed. The current
FSMO holder could not be
contacted.

Modification of a distinguished
name across a naming context is
not permitted.

The attribute cannot be modified
because it is owned by the

325 / 497


Win32 error codes

0x000020B2

ERROR_DS_REPLICATOR_ONLY

0x000020B3

ERROR_DS_OBJ_CLASS_NOT_DEFINED

0x000020B4

ERROR_DS_OBJ_CLASS_NOT_SUBCLASS

0x000020B5

ERROR_DS_NAME_REFERENCE_INVALID

0x000020B6

ERROR_DS_CROSS_REF_EXISTS

0x000020B7

ERROR_DS_CANT_DEL_MASTER_CROSSREF

0x000020B8

ERROR_DS_SUBTREE_NOTIFY_NOT_NC_HEAD

0x000020B9

ERROR_DS_NOTIFY_FILTER_TOO_COMPLEX

0x000020BA

ERROR_DS_DUP_RDN

0x000020BB

ERROR_DS_DUP_OID

0x000020BC

ERROR_DS_DUP_MAPI_ID

0x000020BD

ERROR_DS_DUP_SCHEMA_ID_GUID

0x000020BE

ERROR_DS_DUP_LDAP_DISPLAY_NAME

0x000020BF

ERROR_DS_SEMANTIC_ATT_TEST

0x000020C0

ERROR_DS_SYNTAX_MISMATCH

0x000020C1

ERROR_DS_EXISTS_IN_MUST_HAVE

0x000020C2

ERROR_DS_EXISTS_IN_MAY_HAVE

0x000020C3

ERROR_DS_NONEXISTENT_MAY_HAVE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

system.

Only the replicator can perform
this function.

The specified class is not
defined.

The specified class is not a
subclass.

The name reference is invalid.

A cross-reference already exists.

It is not permitted to delete a
master cross-reference.

Subtree notifications are only
supported on naming context
(NC) heads.

Notification filter is too complex.

Schema update failed: Duplicate
RDN.

Schema update failed: Duplicate
OID.

Schema update failed: Duplicate
Message Application
Programming Interface (MAPI)
identifier.

Schema update failed: Duplicate
schema ID GUID.

Schema update failed: Duplicate
LDAP display name.

Schema update failed: Range-
Lower less than Range-Upper.

Schema update failed: Syntax
mismatch.

Schema deletion failed: Attribute
is used in the Must-Contain list.

Schema deletion failed: Attribute
is used in the May-Contain list.

Schema update failed: Attribute
in May-Contain list does not

326 / 497


Win32 error codes

0x000020C4

ERROR_DS_NONEXISTENT_MUST_HAVE

0x000020C5

ERROR_DS_AUX_CLS_TEST_FAIL

0x000020C6

ERROR_DS_NONEXISTENT_POSS_SUP

0x000020C7

ERROR_DS_SUB_CLS_TEST_FAIL

0x000020C8

ERROR_DS_BAD_RDN_ATT_ID_SYNTAX

0x000020C9

ERROR_DS_EXISTS_IN_AUX_CLS

0x000020CA

ERROR_DS_EXISTS_IN_SUB_CLS

0x000020CB

ERROR_DS_EXISTS_IN_POSS_SUP

0x000020CC

ERROR_DS_RECALCSCHEMA_FAILED

0x000020CD

ERROR_DS_TREE_DELETE_NOT_FINISHED

0x000020CE

ERROR_DS_CANT_DELETE

0x000020CF

ERROR_DS_ATT_SCHEMA_REQ_ID

0x000020D0

ERROR_DS_BAD_ATT_SCHEMA_SYNTAX

0x000020D1

ERROR_DS_CANT_CACHE_ATT

0x000020D2

ERROR_DS_CANT_CACHE_CLASS

0x000020D3

ERROR_DS_CANT_REMOVE_ATT_CACHE

0x000020D4

ERROR_DS_CANT_REMOVE_CLASS_CACHE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

exist.

Schema update failed: Attribute
in the Must-Contain list does not
exist.

Schema update failed: Class in
the Aux Class list does not exist
or is not an auxiliary class.

Schema update failed: Class in
the Poss-Superiors list does not
exist.

Schema update failed: Class in
the subclass of the list does not
exist or does not satisfy
hierarchy rules.

Schema update failed: Rdn-Att-
Id has wrong syntax.

Schema deletion failed: Class is
used as an auxiliary class.

Schema deletion failed: Class is
used as a subclass.

Schema deletion failed: Class is
used as a Poss-Superior.

Schema update failed in
recalculating validation cache.

The tree deletion is not finished.
The request must be made again
to continue deleting the tree.

The requested delete operation
could not be performed.

Cannot read the governs class
identifier for the schema record.

The attribute schema has bad
syntax.

The attribute could not be
cached.

The class could not be cached.

The attribute could not be
removed from the cache.

The class could not be removed
from the cache.

327 / 497


Win32 error codes

0x000020D5

ERROR_DS_CANT_RETRIEVE_DN

0x000020D6

ERROR_DS_MISSING_SUPREF

0x000020D7

ERROR_DS_CANT_RETRIEVE_INSTANCE

0x000020D8

ERROR_DS_CODE_INCONSISTENCY

0x000020D9

ERROR_DS_DATABASE_ERROR

0x000020DA

ERROR_DS_GOVERNSID_MISSING

0x000020DB

ERROR_DS_MISSING_EXPECTED_ATT

0x000020DC

ERROR_DS_NCNAME_MISSING_CR_REF

0x000020DD

ERROR_DS_SECURITY_CHECKING_ERROR

0x000020DE

ERROR_DS_SCHEMA_NOT_LOADED

0x000020DF

ERROR_DS_SCHEMA_ALLOC_FAILED

0x000020E0

ERROR_DS_ATT_SCHEMA_REQ_SYNTAX

0x000020E1

ERROR_DS_GCVERIFY_ERROR

0x000020E2

ERROR_DS_DRA_SCHEMA_MISMATCH

0x000020E3

ERROR_DS_CANT_FIND_DSA_OBJ

0x000020E4

ERROR_DS_CANT_FIND_EXPECTED_NC

0x000020E5

ERROR_DS_CANT_FIND_NC_IN_CACHE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The distinguished name attribute
could not be read.

No superior reference has been
configured for the directory
service. The directory service is,
therefore, unable to issue
referrals to objects outside this
forest.

The instance type attribute could
not be retrieved.

An internal error has occurred.

A database error has occurred.

The governsID attribute is
missing.

An expected attribute is missing.

The specified naming context is
missing a cross-reference.

A security checking error has
occurred.

The schema is not loaded.

Schema allocation failed. Check
if the machine is running low on
memory.

Failed to obtain the required
syntax for the attribute schema.

The GC verification failed. The
GC is not available or does not
support the operation. Some
part of the directory is currently
not available.

The replication operation failed
because of a schema mismatch
between the servers involved.

The DSA object could not be
found.

The naming context could not be
found.

The naming context could not be
found in the cache.

328 / 497


Win32 error codes

0x000020E6

ERROR_DS_CANT_RETRIEVE_CHILD

0x000020E7

ERROR_DS_SECURITY_ILLEGAL_MODIFY

0x000020E8

ERROR_DS_CANT_REPLACE_HIDDEN_REC

0x000020E9

ERROR_DS_BAD_HIERARCHY_FILE

0x000020EA

ERROR_DS_BUILD_HIERARCHY_TABLE_FAILED

0x000020EB

ERROR_DS_CONFIG_PARAM_MISSING

0x000020EC

ERROR_DS_COUNTING_AB_INDICES_FAILED

0x000020ED

ERROR_DS_HIERARCHY_TABLE_MALLOC_FAILED

0x000020EE

ERROR_DS_INTERNAL_FAILURE

0x000020EF

ERROR_DS_UNKNOWN_ERROR

0x000020F0

ERROR_DS_ROOT_REQUIRES_CLASS_TOP

0x000020F1

ERROR_DS_REFUSING_FSMO_ROLES

0x000020F2

ERROR_DS_MISSING_FSMO_SETTINGS

0x000020F3

ERROR_DS_UNABLE_TO_SURRENDER_ROLES

0x000020F4

ERROR_DS_DRA_GENERIC

0x000020F5

ERROR_DS_DRA_INVALID_PARAMETER

0x000020F6

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The child object could not be
retrieved.

The modification was not
permitted for security reasons.

The operation cannot replace the
hidden record.

The hierarchy file is invalid.

The attempt to build the
hierarchy table failed.

The directory configuration
parameter is missing from the
registry.

The attempt to count the
address book indices failed.

The allocation of the hierarchy
table failed.

The directory service
encountered an internal failure.

The directory service
encountered an unknown failure.

A root object requires a class of
"top".

This directory server is shutting
down, and cannot take
ownership of new floating single-
master operation roles.

The directory service is missing
mandatory configuration
information and is unable to
determine the ownership of
floating single-master operation
roles.

The directory service was unable
to transfer ownership of one or
more floating single-master
operation roles to other servers.

The replication operation failed.

An invalid parameter was
specified for this replication
operation.

The directory service is too busy

329 / 497


Win32 error codes

ERROR_DS_DRA_BUSY

0x000020F7

ERROR_DS_DRA_BAD_DN

0x000020F8

ERROR_DS_DRA_BAD_NC

0x000020F9

ERROR_DS_DRA_DN_EXISTS

0x000020FA

ERROR_DS_DRA_INTERNAL_ERROR

0x000020FB

ERROR_DS_DRA_INCONSISTENT_DIT

0x000020FC

ERROR_DS_DRA_CONNECTION_FAILED

0x000020FD

ERROR_DS_DRA_BAD_INSTANCE_TYPE

0x000020FE

ERROR_DS_DRA_OUT_OF_MEM

0x000020FF

ERROR_DS_DRA_MAIL_PROBLEM

0x00002100

ERROR_DS_DRA_REF_ALREADY_EXISTS

0x00002101

ERROR_DS_DRA_REF_NOT_FOUND

0x00002102

ERROR_DS_DRA_OBJ_IS_REP_SOURCE

0x00002103

ERROR_DS_DRA_DB_ERROR

0x00002104

ERROR_DS_DRA_NO_REPLICA

0x00002105

ERROR_DS_DRA_ACCESS_DENIED

0x00002106

ERROR_DS_DRA_NOT_SUPPORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

to complete the replication
operation at this time.

The DN specified for this
replication operation is invalid.

The naming context specified for
this replication operation is
invalid.

The DN specified for this
replication operation already
exists.

The replication system
encountered an internal error.

The replication operation
encountered a database
inconsistency.

The server specified for this
replication operation could not
be contacted.

The replication operation
encountered an object with an
invalid instance type.

The replication operation failed
to allocate memory.

The replication operation
encountered an error with the
mail system.

The replication reference
information for the target server
already exists.

The replication reference
information for the target server
does not exist.

The naming context cannot be
removed because it is replicated
to another server.

The replication operation
encountered a database error.

The naming context is in the
process of being removed or is
not replicated from the specified
server.

Replication access was denied.

The requested operation is not
supported by this version of the

330 / 497


Win32 error codes

0x00002107

ERROR_DS_DRA_RPC_CANCELLED

0x00002108

ERROR_DS_DRA_SOURCE_DISABLED

0x00002109

ERROR_DS_DRA_SINK_DISABLED

0x0000210A

ERROR_DS_DRA_NAME_COLLISION

0x0000210B

ERROR_DS_DRA_SOURCE_REINSTALLED

0x0000210C

ERROR_DS_DRA_MISSING_PARENT

0x0000210D

ERROR_DS_DRA_PREEMPTED

0x0000210E

ERROR_DS_DRA_ABANDON_SYNC

0x0000210F

ERROR_DS_DRA_SHUTDOWN

0x00002110

ERROR_DS_DRA_INCOMPATIBLE_PARTIAL_SET

0x00002111

ERROR_DS_DRA_SOURCE_IS_PARTIAL_REPLICA

0x00002112

ERROR_DS_DRA_EXTN_CONNECTION_FAILED

0x00002113

ERROR_DS_INSTALL_SCHEMA_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

directory service.

The replication RPC was
canceled.

The source server is currently
rejecting replication requests.

The destination server is
currently rejecting replication
requests.

The replication operation failed
due to a collision of object
names.

The replication source has been
reinstalled.

The replication operation failed
because a required parent object
is missing.

The replication operation was
preempted.

The replication synchronization
attempt was abandoned because
of a lack of updates.

The replication operation was
terminated because the system
is shutting down.

A synchronization attempt failed
because the destination DC is
currently waiting to synchronize
new partial attributes from the
source. This condition is normal
if a recent schema change
modified the partial attribute set.
The destination partial attribute
set is not a subset of the source
partial attribute set.

The replication synchronization
attempt failed because a master
replica attempted to sync from a
partial replica.

The server specified for this
replication operation was
contacted, but that server was
unable to contact an additional
server needed to complete the
operation.

The version of the directory
service schema of the source
forest is not compatible with the
version of the directory service
on this computer.

331 / 497


Win32 error codes

0x00002114

ERROR_DS_DUP_LINK_ID

0x00002115

ERROR_DS_NAME_ERROR_RESOLVING

0x00002116

ERROR_DS_NAME_ERROR_NOT_FOUND

0x00002117

ERROR_DS_NAME_ERROR_NOT_UNIQUE

0x00002118

ERROR_DS_NAME_ERROR_NO_MAPPING

0x00002119

ERROR_DS_NAME_ERROR_DOMAIN_ONLY

0x0000211A

ERROR_DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING

0x0000211B

ERROR_DS_CONSTRUCTED_ATT_MOD

0x0000211C

ERROR_DS_WRONG_OM_OBJ_CLASS

0x0000211D

ERROR_DS_DRA_REPL_PENDING

0x0000211E

ERROR_DS_DS_REQUIRED

0x0000211F

ERROR_DS_INVALID_LDAP_DISPLAY_NAME

0x00002120

ERROR_DS_NON_BASE_SEARCH

0x00002121

ERROR_DS_CANT_RETRIEVE_ATTS

0x00002122

ERROR_DS_BACKLINK_WITHOUT_LINK

0x00002123

ERROR_DS_EPOCH_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Schema update failed: An
attribute with the same link
identifier already exists.

Name translation: Generic
processing error.

Name translation: Could not find
the name or insufficient right to
see name.

Name translation: Input name
mapped to more than one output
name.

Name translation: The input
name was found but not the
associated output format.

Name translation: Unable to
resolve completely, only the
domain was found.

Name translation: Unable to
perform purely syntactical
mapping at the client without
going out to the wire.

Modification of a constructed
attribute is not allowed.

The OM-Object-Class specified is
incorrect for an attribute with
the specified syntax.

The replication request has been
posted; waiting for a reply.

The requested operation requires
a directory service, and none
was available.

The LDAP display name of the
class or attribute contains non-
ASCII characters.

The requested search operation
is only supported for base
searches.

The search failed to retrieve
attributes from the database.

The schema update operation
tried to add a backward link
attribute that has no
corresponding forward link.

The source and destination of a
cross-domain move do not agree
on the object's epoch number.
Either the source or the

332 / 497


Win32 error codes

0x00002124

ERROR_DS_SRC_NAME_MISMATCH

0x00002125

ERROR_DS_SRC_AND_DST_NC_IDENTICAL

0x00002126

ERROR_DS_DST_NC_MISMATCH

0x00002127

ERROR_DS_NOT_AUTHORITIVE_FOR_DST_NC

0x00002128

ERROR_DS_SRC_GUID_MISMATCH

0x00002129

ERROR_DS_CANT_MOVE_DELETED_OBJECT

0x0000212A

ERROR_DS_PDC_OPERATION_IN_PROGRESS

0x0000212B

ERROR_DS_CROSS_DOMAIN_CLEANUP_REQD

0x0000212C

ERROR_DS_ILLEGAL_XDOM_MOVE_OPERATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

destination does not have the
latest version of the object.

The source and destination of a
cross-domain move do not agree
on the object's current name.
Either the source or the
destination does not have the
latest version of the object.

The source and destination for
the cross-domain move
operation are identical. The
caller should use a local move
operation instead of a cross-
domain move operation.

The source and destination for a
cross-domain move do not agree
on the naming contexts in the
forest. Either the source or the
destination does not have the
latest version of the Partitions
container.

The destination of a cross-
domain move is not authoritative
for the destination naming
context.

The source and destination of a
cross-domain move do not agree
on the identity of the source
object. Either the source or the
destination does not have the
latest version of the source
object.

The object being moved across
domains is already known to be
deleted by the destination
server. The source server does
not have the latest version of the
source object.

Another operation that requires
exclusive access to the PDC
FSMO is already in progress.

A cross-domain move operation
failed because two versions of
the moved object exist—one
each in the source and
destination domains. The
destination object needs to be
removed to restore the system
to a consistent state.

This object cannot be moved
across domain boundaries either
because cross-domain moves for
this class are not allowed, or the
object has some special

333 / 497


Win32 error codes

0x0000212D

ERROR_DS_CANT_WITH_ACCT_GROUP_MEMBERSHPS

0x0000212E

ERROR_DS_NC_MUST_HAVE_NC_PARENT

0x0000212F

ERROR_DS_CR_IMPOSSIBLE_TO_VALIDATE

0x00002130

ERROR_DS_DST_DOMAIN_NOT_NATIVE

0x00002131

ERROR_DS_MISSING_INFRASTRUCTURE_CONTAINER

0x00002132

ERROR_DS_CANT_MOVE_ACCOUNT_GROUP

0x00002133

ERROR_DS_CANT_MOVE_RESOURCE_GROUP

0x00002134

ERROR_DS_INVALID_SEARCH_FLAG

0x00002135

ERROR_DS_NO_TREE_DELETE_ABOVE_NC

0x00002136

ERROR_DS_COULDNT_LOCK_TREE_FOR_DELETE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

characteristics, for example, a
trust account or a restricted
relative identifier (RID), that
prevent its move.

Cannot move objects with
memberships across domain
boundaries because, once
moved, this violates the
membership conditions of the
account group. Remove the
object from any account group
memberships and retry.

A naming context head must be
the immediate child of another
naming context head, not of an
interior node.

The directory cannot validate the
proposed naming context name
because it does not hold a
replica of the naming context
above the proposed naming
context. Ensure that the domain
naming master role is held by a
server that is configured as a GC
server, and that the server is up-
to-date with its replication
partners. (Applies only to
Windows 2000 operating system
domain naming masters.)

Destination domain must be in
native mode.

The operation cannot be
performed because the server
does not have an infrastructure
container in the domain of
interest.

Cross-domain moves of
nonempty account groups is not
allowed.

Cross-domain moves of
nonempty resource groups is not
allowed.

The search flags for the attribute
are invalid. The ambiguous name
resolution (ANR) bit is valid only
on attributes of Unicode or
Teletex strings.

Tree deletions starting at an
object that has an NC head as a
descendant are not allowed.

The directory service failed to
lock a tree in preparation for a
tree deletion because the tree

334 / 497


Win32 error codes

0x00002137

ERROR_DS_COULDNT_IDENTIFY_OBJECTS_FOR_TREE_DELETE

0x00002138

ERROR_DS_SAM_INIT_FAILURE

0x00002139

ERROR_DS_SENSITIVE_GROUP_VIOLATION

0x0000213A

ERROR_DS_CANT_MOD_PRIMARYGROUPID

0x0000213B

ERROR_DS_ILLEGAL_BASE_SCHEMA_MOD

0x0000213C

ERROR_DS_NONSAFE_SCHEMA_CHANGE

0x0000213D

ERROR_DS_SCHEMA_UPDATE_DISALLOWED

0x0000213E

ERROR_DS_CANT_CREATE_UNDER_SCHEMA

0x0000213F

ERROR_DS_INSTALL_NO_SRC_SCH_VERSION

0x00002140

ERROR_DS_INSTALL_NO_SCH_VERSION_IN_INIFILE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

was in use.

The directory service failed to
identify the list of objects to
delete while attempting a tree
deletion.

SAM initialization failed because
of the following error: %1. Error
Status: 0x%2. Click OK to shut
down the system and reboot into
Directory Services Restore Mode.
Check the event log for detailed
information.

Only an administrator can
modify the membership list of an
administrative group.

Cannot change the primary
group ID of a domain controller
account.

An attempt was made to modify
the base schema.

Adding a new mandatory
attribute to an existing class,
deleting a mandatory attribute
from an existing class, or adding
an optional attribute to the
special class Top that is not a
backlink attribute (directly or
through inheritance, for
example, by adding or deleting
an auxiliary class) is not allowed.

Schema update is not allowed on
this DC because the DC is not
the schema FSMO role owner.

An object of this class cannot be
created under the schema
container. You can only create
Attribute-Schema and Class-
Schema objects under the
schema container.

The replica or child install failed
to get the objectVersion
attribute on the schema
container on the source DC.
Either the attribute is missing on
the schema container or the
credentials supplied do not have
permission to read it.

The replica or child install failed
to read the objectVersion
attribute in the SCHEMA section
of the file schema.ini in the
System32 directory.

335 / 497


Win32 error codes

0x00002141

ERROR_DS_INVALID_GROUP_TYPE

0x00002142

ERROR_DS_NO_NEST_GLOBALGROUP_IN_MIXEDDOMAIN

0x00002143

ERROR_DS_NO_NEST_LOCALGROUP_IN_MIXEDDOMAIN

0x00002144

ERROR_DS_GLOBAL_CANT_HAVE_LOCAL_MEMBER

0x00002145

ERROR_DS_GLOBAL_CANT_HAVE_UNIVERSAL_MEMBER

0x00002146

ERROR_DS_UNIVERSAL_CANT_HAVE_LOCAL_MEMBER

0x00002147

ERROR_DS_GLOBAL_CANT_HAVE_CROSSDOMAIN_MEMBER

0x00002148

ERROR_DS_LOCAL_CANT_HAVE_CROSSDOMAIN_LOCAL_MEMBER

0x00002149

ERROR_DS_HAVE_PRIMARY_MEMBERS

0x0000214A

ERROR_DS_STRING_SD_CONVERSION_FAILED

0x0000214B

ERROR_DS_NAMING_MASTER_GC

0x0000214C

ERROR_DS_DNS_LOOKUP_FAILURE

0x0000214D

ERROR_DS_COULDNT_UPDATE_SPNS

0x0000214E

ERROR_DS_CANT_RETRIEVE_SD

0x0000214F

ERROR_DS_KEY_NOT_UNIQUE

0x00002150

ERROR_DS_WRONG_LINKED_ATT_SYNTAX

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified group type is
invalid.

You cannot nest global groups in
a mixed domain if the group is
security-enabled.

You cannot nest local groups in a
mixed domain if the group is
security-enabled.

A global group cannot have a
local group as a member.

A global group cannot have a
universal group as a member.

A universal group cannot have a
local group as a member.

A global group cannot have a
cross-domain member.

A local group cannot have
another cross domain local group
as a member.

A group with primary members
cannot change to a security-
disabled group.

The schema cache load failed to
convert the string default
security descriptor (SD) on a
class-schema object.

Only DSAs configured to be GC
servers should be allowed to
hold the domain naming master
FSMO role. (Applies only to
Windows 2000 servers.)

The DSA operation is unable to
proceed because of a DNS
lookup failure.

While processing a change to the
DNS host name for an object,
the SPN values could not be kept
in sync.

The Security Descriptor attribute
could not be read.

The object requested was not
found, but an object with that
key was found.

The syntax of the linked
attribute being added is
incorrect. Forward links can only

336 / 497


Win32 error codes

0x00002151

ERROR_DS_SAM_NEED_BOOTKEY_PASSWORD

0x00002152

ERROR_DS_SAM_NEED_BOOTKEY_FLOPPY

0x00002153

ERROR_DS_CANT_START

0x00002154

ERROR_DS_INIT_FAILURE

0x00002155

ERROR_DS_NO_PKT_PRIVACY_ON_CONNECTION

0x00002156

ERROR_DS_SOURCE_DOMAIN_IN_FOREST

0x00002157

ERROR_DS_DESTINATION_DOMAIN_NOT_IN_FOREST

0x00002158

ERROR_DS_DESTINATION_AUDITING_NOT_ENABLED

0x00002159

ERROR_DS_CANT_FIND_DC_FOR_SRC_DOMAIN

0x0000215A

ERROR_DS_SRC_OBJ_NOT_GROUP_OR_USER

0x0000215B

ERROR_DS_SRC_SID_EXISTS_IN_FOREST

0x0000215C

ERROR_DS_SRC_AND_DST_OBJECT_CLASS_MISMATCH

0x0000215D

ERROR_SAM_INIT_FAILURE

0x0000215E

ERROR_DS_DRA_SCHEMA_INFO_SHIP

0x0000215F

ERROR_DS_DRA_SCHEMA_CONFLICT

0x00002160

ERROR_DS_DRA_EARLIER_SCHEMA_CONFLICT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

have syntax 2.5.5.1, 2.5.5.7,
and 2.5.5.14, and backlinks can
only have syntax 2.5.5.1.

SAM needs to get the boot
password.

SAM needs to get the boot key
from the floppy disk.

Directory Service cannot start.

Directory Services could not
start.

The connection between client
and server requires packet
privacy or better.

The source domain cannot be in
the same forest as the
destination.

The destination domain MUST be
in the forest.

The operation requires that
destination domain auditing be
enabled.

The operation could not locate a
DC for the source domain.

The source object must be a
group or user.

The source object's SID already
exists in the destination forest.

The source and destination
object must be of the same type.

SAM initialization failed because
of the following error: %1. Error
Status: 0x%2. Click OK to shut
down the system and reboot into
Safe Mode. Check the event log
for detailed information.

Schema information could not be
included in the replication
request.

The replication operation could
not be completed due to a
schema incompatibility.

The replication operation could
not be completed due to a

337 / 497


Win32 error codes

0x00002161

ERROR_DS_DRA_OBJ_NC_MISMATCH

0x00002162

ERROR_DS_NC_STILL_HAS_DSAS

0x00002163

ERROR_DS_GC_REQUIRED

0x00002164

ERROR_DS_LOCAL_MEMBER_OF_LOCAL_ONLY

0x00002165

ERROR_DS_NO_FPO_IN_UNIVERSAL_GROUPS

0x00002166

ERROR_DS_CANT_ADD_TO_GC

0x00002167

ERROR_DS_NO_CHECKPOINT_WITH_PDC

0x00002168

ERROR_DS_SOURCE_AUDITING_NOT_ENABLED

0x00002169

ERROR_DS_CANT_CREATE_IN_NONDOMAIN_NC

0x0000216A

ERROR_DS_INVALID_NAME_FOR_SPN

0x0000216B

ERROR_DS_FILTER_USES_CONTRUCTED_ATTRS

0x0000216C

ERROR_DS_UNICODEPWD_NOT_IN_QUOTES

0x0000216D

ERROR_DS_MACHINE_ACCOUNT_QUOTA_EXCEEDED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

previous schema incompatibility.

The replication update could not
be applied because either the
source or the destination has not
yet received information
regarding a recent cross-domain
move operation.

The requested domain could not
be deleted because there exist
domain controllers that still host
this domain.

The requested operation can be
performed only on a GC server.

A local group can only be a
member of other local groups in
the same domain.

Foreign security principals
cannot be members of universal
groups.

The attribute is not allowed to be
replicated to the GC because of
security reasons.

The checkpoint with the PDC
could not be taken because too
many modifications are currently
being processed.

The operation requires that
source domain auditing be
enabled.

Security principal objects can
only be created inside domain
naming contexts.

An SPN could not be constructed
because the provided host name
is not in the necessary format.

A filter was passed that uses
constructed attributes.

The unicodePwd attribute value
must be enclosed in quotation
marks.

Your computer could not be
joined to the domain. You have
exceeded the maximum number
of computer accounts you are
allowed to create in this domain.
Contact your system
administrator to have this limit
reset or increased.

338 / 497


Win32 error codes

0x0000216E

ERROR_DS_MUST_BE_RUN_ON_DST_DC

0x0000216F

ERROR_DS_SRC_DC_MUST_BE_SP4_OR_GREATER

0x00002170

ERROR_DS_CANT_TREE_DELETE_CRITICAL_OBJ

0x00002171

ERROR_DS_INIT_FAILURE_CONSOLE

0x00002172

ERROR_DS_SAM_INIT_FAILURE_CONSOLE

0x00002173

ERROR_DS_FOREST_VERSION_TOO_HIGH

0x00002174

ERROR_DS_DOMAIN_VERSION_TOO_HIGH

0x00002175

ERROR_DS_FOREST_VERSION_TOO_LOW

0x00002176

ERROR_DS_DOMAIN_VERSION_TOO_LOW

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

For security reasons, the
operation must be run on the
destination DC.

For security reasons, the source
DC must be NT4SP4 or greater.

Critical directory service system
objects cannot be deleted during
tree deletion operations. The
tree deletion might have been
partially performed.

Directory Services could not
start because of the following
error: %1. Error Status: 0x%2.
Click OK to shut down the
system. You can use the
Recovery Console to further
diagnose the system.

SAM initialization failed because
of the following error: %1. Error
Status: 0x%2. Click OK to shut
down the system. You can use
the Recovery Console to further
diagnose the system.

The version of the operating
system installed is incompatible
with the current forest functional
level. You must upgrade to a
new version of the operating
system before this server can
become a domain controller in
this forest.

The version of the operating
system installed is incompatible
with the current domain
functional level. You must
upgrade to a new version of the
operating system before this
server can become a domain
controller in this domain.

The version of the operating
system installed on this server
no longer supports the current
forest functional level. You must
raise the forest functional level
before this server can become a
domain controller in this forest.

The version of the operating
system installed on this server
no longer supports the current
domain functional level. You
must raise the domain functional
level before this server can
become a domain controller in
this domain.

339 / 497


Win32 error codes

0x00002177

ERROR_DS_INCOMPATIBLE_VERSION

0x00002178

ERROR_DS_LOW_DSA_VERSION

0x00002179

ERROR_DS_NO_BEHAVIOR_VERSION_IN_MIXEDDOMAIN

0x0000217A

ERROR_DS_NOT_SUPPORTED_SORT_ORDER

0x0000217B

ERROR_DS_NAME_NOT_UNIQUE

0x0000217C

ERROR_DS_MACHINE_ACCOUNT_CREATED_PRENT4

0x0000217D

ERROR_DS_OUT_OF_VERSION_STORE

0x0000217E

ERROR_DS_INCOMPATIBLE_CONTROLS_USED

0x0000217F

ERROR_DS_NO_REF_DOMAIN

0x00002180

ERROR_DS_RESERVED_LINK_ID

0x00002181

ERROR_DS_LINK_ID_NOT_AVAILABLE

0x00002182

ERROR_DS_AG_CANT_HAVE_UNIVERSAL_MEMBER

0x00002183

ERROR_DS_MODIFYDN_DISALLOWED_BY_INSTANCE_TYPE

0x00002184

ERROR_DS_NO_OBJECT_MOVE_IN_SCHEMA_NC

0x00002185

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The version of the operating
system installed on this server is
incompatible with the functional
level of the domain or forest.

The functional level of the
domain (or forest) cannot be
raised to the requested value
because one or more domain
controllers in the domain (or
forest) are at a lower,
incompatible functional level.

The forest functional level cannot
be raised to the requested value
because one or more domains
are still in mixed-domain mode.
All domains in the forest must be
in native mode for you to raise
the forest functional level.

The sort order requested is not
supported.

The requested name already
exists as a unique identifier.

The machine account was
created before Windows NT 4.0.
The account needs to be re-
created.

The database is out of version
store.

Unable to continue operation
because multiple conflicting
controls were used.

Unable to find a valid security
descriptor reference domain for
this partition.

Schema update failed: The link
identifier is reserved.

Schema update failed: There are
no link identifiers available.

An account group cannot have a
universal group as a member.

Rename or move operations on
naming context heads or read-
only objects are not allowed.

Move operations on objects in
the schema naming context are
not allowed.

A system flag has been set on

340 / 497


Win32 error codes

ERROR_DS_MODIFYDN_DISALLOWED_BY_FLAG

0x00002186

ERROR_DS_MODIFYDN_WRONG_GRANDPARENT

0x00002187

ERROR_DS_NAME_ERROR_TRUST_REFERRAL

0x00002188

ERROR_NOT_SUPPORTED_ON_STANDARD_SERVER

0x00002189

ERROR_DS_CANT_ACCESS_REMOTE_PART_OF_AD

0x0000218A

ERROR_DS_CR_IMPOSSIBLE_TO_VALIDATE_V2

0x0000218B

ERROR_DS_THREAD_LIMIT_EXCEEDED

0x0000218C

ERROR_DS_NOT_CLOSEST

0x0000218D

ERROR_DS_CANT_DERIVE_SPN_WITHOUT_SERVER_REF

0x0000218E

ERROR_DS_SINGLE_USER_MODE_FAILED

0x0000218F

ERROR_DS_NTDSCRIPT_SYNTAX_ERROR

0x00002190

ERROR_DS_NTDSCRIPT_PROCESS_ERROR

0x00002191

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the object that does not allow
the object to be moved or
renamed.

This object is not allowed to
change its grandparent
container. Moves are not
forbidden on this object, but are
restricted to sibling containers.

Unable to resolve completely; a
referral to another forest was
generated.

The requested action is not
supported on a standard server.

Could not access a partition of
the directory service located on a
remote server. Make sure at
least one server is running for
the partition in question.

The directory cannot validate the
proposed naming context (or
partition) name because it does
not hold a replica, nor can it
contact a replica of the naming
context above the proposed
naming context. Ensure that the
parent naming context is
properly registered in the DNS,
and at least one replica of this
naming context is reachable by
the domain naming master.

The thread limit for this request
was exceeded.

The GC server is not in the
closest site.

The directory service cannot
derive an SPN with which to
mutually authenticate the target
server because the
corresponding server object in
the local DS database has no
serverReference attribute.

The directory service failed to
enter single-user mode.

The directory service cannot
parse the script because of a
syntax error.

The directory service cannot
process the script because of an
error.

The directory service cannot

341 / 497


Win32 error codes

ERROR_DS_DIFFERENT_REPL_EPOCHS

0x00002192

ERROR_DS_DRS_EXTENSIONS_CHANGED

0x00002193

ERROR_DS_REPLICA_SET_CHANGE_NOT_ALLOWED_ON_DISABLED_CR

0x00002194

ERROR_DS_NO_MSDS_INTID

0x00002195

ERROR_DS_DUP_MSDS_INTID

0x00002196

ERROR_DS_EXISTS_IN_RDNATTID

0x00002197

ERROR_DS_AUTHORIZATION_FAILED

0x00002198

ERROR_DS_INVALID_SCRIPT

0x00002199

ERROR_DS_REMOTE_CROSSREF_OP_FAILED

0x0000219A

ERROR_DS_CROSS_REF_BUSY

0x0000219B

ERROR_DS_CANT_DERIVE_SPN_FOR_DELETED_DOMAIN

0x0000219C

ERROR_DS_CANT_DEMOTE_WITH_WRITEABLE_NC

0x0000219D

ERROR_DS_DUPLICATE_ID_FOUND

0x0000219E

ERROR_DS_INSUFFICIENT_ATTR_TO_CREATE_OBJECT

0x0000219F

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

perform the requested operation
because the servers involved are
of different replication epochs
(which is usually related to a
domain rename that is in
progress).

The directory service binding
must be renegotiated due to a
change in the server extensions
information.

The operation is not allowed on a
disabled cross-reference.

Schema update failed: No values
for msDS-IntId are available.

Schema update failed: Duplicate
msDS-IntId. Retry the operation.

Schema deletion failed: Attribute
is used in rDNAttID.

The directory service failed to
authorize the request.

The directory service cannot
process the script because it is
invalid.

The remote create cross-
reference operation failed on the
domain naming master FSMO.
The operation's error is in the
extended data.

A cross-reference is in use locally
with the same name.

The directory service cannot
derive an SPN with which to
mutually authenticate the target
server because the server's
domain has been deleted from
the forest.

Writable NCs prevent this DC
from demoting.

The requested object has a
nonunique identifier and cannot
be retrieved.

Insufficient attributes were given
to create an object. This object
might not exist because it might
have been deleted and the
garbage already collected.

The group cannot be converted

342 / 497


Win32 error codes

ERROR_DS_GROUP_CONVERSION_ERROR

0x000021A0

ERROR_DS_CANT_MOVE_APP_BASIC_GROUP

0x000021A1

ERROR_DS_CANT_MOVE_APP_QUERY_GROUP

0x000021A2

ERROR_DS_ROLE_NOT_VERIFIED

0x000021A3

ERROR_DS_WKO_CONTAINER_CANNOT_BE_SPECIAL

0x000021A4

ERROR_DS_DOMAIN_RENAME_IN_PROGRESS

0x000021A5

ERROR_DS_EXISTING_AD_CHILD_NC

0x000021A6

ERROR_DS_REPL_LIFETIME_EXCEEDED

0x000021A7

ERROR_DS_DISALLOWED_IN_SYSTEM_CONTAINER

0x000021A8

ERROR_DS_LDAP_SEND_QUEUE_FULL

0x000021A9

ERROR_DS_DRA_OUT_SCHEDULE_WINDOW

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

due to attribute restrictions on
the requested group type.

Cross-domain moves of
nonempty basic application
groups is not allowed.

Cross-domain moves of
nonempty query-based
application groups is not
allowed.

The FSMO role ownership could
not be verified because its
directory partition did not
replicate successfully with at
least one replication partner.

The target container for a
redirection of a well-known
object container cannot already
be a special container.

The directory service cannot
perform the requested operation
because a domain rename
operation is in progress.

The directory service detected a
child partition below the
requested partition name. The
partition hierarchy must be
created in a top down method.

The directory service cannot
replicate with this server
because the time since the last
replication with this server has
exceeded the tombstone
lifetime.

The requested operation is not
allowed on an object under the
system container.

The LDAP server's network send
queue has filled up because the
client is not processing the
results of its requests fast
enough. No more requests will
be processed until the client
catches up. If the client does not
catch up then it will be
disconnected.

The scheduled replication did not
take place because the system
was too busy to execute the
request within the schedule
window. The replication queue is
overloaded. Consider reducing
the number of partners or
decreasing the scheduled

343 / 497


Win32 error codes

0x000021AA

ERROR_DS_POLICY_NOT_KNOWN

0x000021AB

ERROR_NO_SITE_SETTINGS_OBJECT

0x000021AC

ERROR_NO_SECRETS

0x000021AD

ERROR_NO_WRITABLE_DC_FOUND

0x000021AE

ERROR_DS_NO_SERVER_OBJECT

0x000021AF

ERROR_DS_NO_NTDSA_OBJECT

0x000021B0

ERROR_DS_NON_ASQ_SEARCH

0x000021B1

ERROR_DS_AUDIT_FAILURE

0x000021B2

ERROR_DS_INVALID_SEARCH_FLAG_SUBTREE

0x000021B3

ERROR_DS_INVALID_SEARCH_FLAG_TUPLE

0x000021BF

ERROR_DS_DRA_RECYCLED_TARGET

0x000021C2

ERROR_DS_HIGH_DSA_VERSION

0x000021C7

ERROR_DS_SPN_VALUE_NOT_UNIQUE_IN_FOREST

0x000021C8

ERROR_DS_UPN_VALUE_NOT_UNIQUE_IN_FOREST

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

replication frequency.

At this time, it cannot be
determined if the branch
replication policy is available on
the hub domain controller. Retry
at a later time to account for
replication latencies.

The site settings object for the
specified site does not exist.

The local account store does not
contain secret material for the
specified account.

Could not find a writable domain
controller in the domain.

The server object for the domain
controller does not exist.

The NTDS Settings object for the
domain controller does not exist.

The requested search operation
is not supported for attribute
scoped query (ASQ) searches.

A required audit event could not
be generated for the operation.

The search flags for the attribute
are invalid. The subtree index bit
is valid only on single-valued
attributes.

The search flags for the attribute
are invalid. The tuple index bit is
valid only on attributes of
Unicode strings.

The replication operation failed
because the target object
referenced by a link value is
recycled.

The functional level of the
domain (or forest) cannot be
lowered to the requested value.

The operation failed because the
SPN value provided for
addition/modification is not
unique forest-wide.

The operation failed because the
UPN value provided for
addition/modification is not
unique forest-wide.

344 / 497


Win32 error codes

0x00002329

DNS_ERROR_RCODE_FORMAT_ERROR

0x0000232A

DNS_ERROR_RCODE_SERVER_FAILURE

0x0000232B

DNS_ERROR_RCODE_NAME_ERROR

0x0000232C

DNS_ERROR_RCODE_NOT_IMPLEMENTED

0x0000232D

DNS_ERROR_RCODE_REFUSED

0x0000232E

DNS_ERROR_RCODE_YXDOMAIN

0x0000232F

DNS_ERROR_RCODE_YXRRSET

0x00002330

DNS_ERROR_RCODE_NXRRSET

0x00002331

DNS_ERROR_RCODE_NOTAUTH

0x00002332

DNS_ERROR_RCODE_NOTZONE

0x00002338

DNS_ERROR_RCODE_BADSIG

0x00002339

DNS_ERROR_RCODE_BADKEY

0x0000233A

DNS_ERROR_RCODE_BADTIME

0x0000251D

DNS_INFO_NO_RECORDS

0x0000251E

DNS_ERROR_BAD_PACKET

0x0000251F

DNS_ERROR_NO_PACKET

0x00002520

DNS_ERROR_RCODE

0x00002521

DNS_ERROR_UNSECURE_PACKET

0x0000254F

DNS_ERROR_INVALID_TYPE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

DNS server unable to interpret
format.

DNS server failure.

DNS name does not exist.

DNS request not supported by
name server.

DNS operation refused.

DNS name that should not exist,
does exist.

DNS resource record (RR) set
that should not exist, does exist.

DNS RR set that should to exist,
does not exist.

DNS server not authoritative for
zone.

DNS name in update or prereq is
not in zone.

DNS signature failed to verify.

DNS bad key.

DNS signature validity expired.

No records found for given DNS
query.

Bad DNS packet.

No DNS packet.

DNS error, check rcode.

Unsecured DNS packet.

Invalid DNS type.

345 / 497


Win32 error codes

0x00002550

DNS_ERROR_INVALID_IP_ADDRESS

0x00002551

DNS_ERROR_INVALID_PROPERTY

0x00002552

DNS_ERROR_TRY_AGAIN_LATER

0x00002553

DNS_ERROR_NOT_UNIQUE

0x00002554

DNS_ERROR_NON_RFC_NAME

0x00002555

DNS_STATUS_FQDN

0x00002556

DNS_STATUS_DOTTED_NAME

0x00002557

DNS_STATUS_SINGLE_PART_NAME

0x00002558

DNS_ERROR_INVALID_NAME_CHAR

0x00002559

DNS_ERROR_NUMERIC_NAME

0x0000255A

DNS_ERROR_NOT_ALLOWED_ON_ROOT_SERVER

0x0000255B

DNS_ERROR_NOT_ALLOWED_UNDER_DELEGATION

0x0000255C

DNS_ERROR_CANNOT_FIND_ROOT_HINTS

0x0000255D

DNS_ERROR_INCONSISTENT_ROOT_HINTS

0x0000255E

DNS_ERROR_DWORD_VALUE_TOO_SMALL

0x0000255F

DNS_ERROR_DWORD_VALUE_TOO_LARGE

0x00002560

DNS_ERROR_BACKGROUND_LOADING

0x00002561

DNS_ERROR_NOT_ALLOWED_ON_RODC

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Invalid IP address.

Invalid property.

Try DNS operation again later.

Record for given name and type
is not unique.

DNS name does not comply with
RFC specifications.

DNS name is a fully qualified
DNS name.

DNS name is dotted (multilabel).

DNS name is a single-part name.

DNS name contains an invalid
character.

DNS name is entirely numeric.

The operation requested is not
permitted on a DNS root server.

The record could not be created
because this part of the DNS
namespace has been delegated
to another server.

The DNS server could not find a
set of root hints.

The DNS server found root hints
but they were not consistent
across all adapters.

The specified value is too small
for this parameter.

The specified value is too large
for this parameter.

This operation is not allowed
while the DNS server is loading
zones in the background. Try
again later.

The operation requested is not
permitted on against a DNS
server running on a read-only

346 / 497


Win32 error codes

0x00002581

DNS_ERROR_ZONE_DOES_NOT_EXIST

0x00002582

DNS_ERROR_NO_ZONE_INFO

0x00002583

DNS_ERROR_INVALID_ZONE_OPERATION

0x00002584

DNS_ERROR_ZONE_CONFIGURATION_ERROR

0x00002585

DNS_ERROR_ZONE_HAS_NO_SOA_RECORD

0x00002586

DNS_ERROR_ZONE_HAS_NO_NS_RECORDS

0x00002587

DNS_ERROR_ZONE_LOCKED

0x00002588

DNS_ERROR_ZONE_CREATION_FAILED

0x00002589

DNS_ERROR_ZONE_ALREADY_EXISTS

0x0000258A

DNS_ERROR_AUTOZONE_ALREADY_EXISTS

0x0000258B

DNS_ERROR_INVALID_ZONE_TYPE

0x0000258C

DNS_ERROR_SECONDARY_REQUIRES_MASTER_IP

0x0000258D

DNS_ERROR_ZONE_NOT_SECONDARY

0x0000258E

DNS_ERROR_NEED_SECONDARY_ADDRESSES

0x0000258F

DNS_ERROR_WINS_INIT_FAILED

0x00002590

DNS_ERROR_NEED_WINS_SERVERS

0x00002591

DNS_ERROR_NBSTAT_INIT_FAILED

0x00002592

DNS_ERROR_SOA_DELETE_INVALID

Description

DC.

DNS zone does not exist.

DNS zone information not
available.

Invalid operation for DNS zone.

Invalid DNS zone configuration.

DNS zone has no start of
authority (SOA) record.

DNS zone has no Name Server
(NS) record.

DNS zone is locked.

DNS zone creation failed.

DNS zone already exists.

DNS automatic zone already
exists.

Invalid DNS zone type.

Secondary DNS zone requires
master IP address.

DNS zone not secondary.

Need secondary IP address.

WINS initialization failed.

Need WINS servers.

NBTSTAT initialization call failed.

Invalid delete of SOA.

0x00002593

A conditional forwarding zone

347 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Win32 error codes

Description

DNS_ERROR_FORWARDER_ALREADY_EXISTS

already exists for that name.

0x00002594

DNS_ERROR_ZONE_REQUIRES_MASTER_IP

0x00002595

DNS_ERROR_ZONE_IS_SHUTDOWN

0x000025B3

DNS_ERROR_PRIMARY_REQUIRES_DATAFILE

0x000025B4

DNS_ERROR_INVALID_DATAFILE_NAME

0x000025B5

DNS_ERROR_DATAFILE_OPEN_FAILURE

0x000025B6

DNS_ERROR_FILE_WRITEBACK_FAILED

0x000025B7

DNS_ERROR_DATAFILE_PARSING

0x000025E5

DNS_ERROR_RECORD_DOES_NOT_EXIST

0x000025E6

DNS_ERROR_RECORD_FORMAT

0x000025E7

DNS_ERROR_NODE_CREATION_FAILED

0x000025E8

DNS_ERROR_UNKNOWN_RECORD_TYPE

0x000025E9

DNS_ERROR_RECORD_TIMED_OUT

0x000025EA

DNS_ERROR_NAME_NOT_IN_ZONE

0x000025EB

DNS_ERROR_CNAME_LOOP

0x000025EC

DNS_ERROR_NODE_IS_CNAME

0x000025ED

DNS_ERROR_CNAME_COLLISION

0x000025EE

DNS_ERROR_RECORD_ONLY_AT_ZONE_ROOT

0x000025EF

DNS_ERROR_RECORD_ALREADY_EXISTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

This zone must be configured
with one or more master DNS
server IP addresses.

The operation cannot be
performed because this zone is
shut down.

The primary DNS zone requires a
data file.

Invalid data file name for the
DNS zone.

Failed to open the data file for
the DNS zone.

Failed to write the data file for
the DNS zone.

Failure while reading datafile for
DNS zone.

DNS record does not exist.

DNS record format error.

Node creation failure in DNS.

Unknown DNS record type.

DNS record timed out.

Name not in DNS zone.

CNAME loop detected.

Node is a CNAME DNS record.

A CNAME record already exists
for the given name.

Record is only at DNS zone root.

DNS record already exists.

348 / 497


Win32 error codes

0x000025F0

DNS_ERROR_SECONDARY_DATA

0x000025F1

DNS_ERROR_NO_CREATE_CACHE_DATA

0x000025F2

DNS_ERROR_NAME_DOES_NOT_EXIST

0x000025F3

DNS_WARNING_PTR_CREATE_FAILED

0x000025F4

DNS_WARNING_DOMAIN_UNDELETED

0x000025F5

DNS_ERROR_DS_UNAVAILABLE

0x000025F6

DNS_ERROR_DS_ZONE_ALREADY_EXISTS

0x000025F7

DNS_ERROR_NO_BOOTFILE_IF_DS_ZONE

0x00002617

DNS_INFO_AXFR_COMPLETE

0x00002618

DNS_ERROR_AXFR

0x00002619

DNS_INFO_ADDED_LOCAL_WINS

0x00002649

DNS_STATUS_CONTINUE_NEEDED

0x0000267B

DNS_ERROR_NO_TCPIP

0x0000267C

DNS_ERROR_NO_DNS_SERVERS

0x000026AD

DNS_ERROR_DP_DOES_NOT_EXIST

0x000026AE

DNS_ERROR_DP_ALREADY_EXISTS

0x000026AF

DNS_ERROR_DP_NOT_ENLISTED

0x000026B0

DNS_ERROR_DP_ALREADY_ENLISTED

0x000026B1

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Secondary DNS zone data error.

Could not create DNS cache
data.

DNS name does not exist.

Could not create pointer (PTR)
record.

DNS domain was undeleted.

The directory service is
unavailable.

DNS zone already exists in the
directory service.

DNS server not creating or
reading the boot file for the
directory service integrated DNS
zone.

DNS AXFR (zone transfer)
complete.

DNS zone transfer failed.

Added local WINS server.

Secure update call needs to
continue update request.

TCP/IP network protocol not
installed.

No DNS servers configured for
local system.

The specified directory partition
does not exist.

The specified directory partition
already exists.

This DNS server is not enlisted in
the specified directory partition.

This DNS server is already
enlisted in the specified directory
partition.

The directory partition is not

349 / 497


Win32 error codes

DNS_ERROR_DP_NOT_AVAILABLE

0x000026B2

DNS_ERROR_DP_FSMO_ERROR

0x00002714

WSAEINTR

0x00002719

WSAEBADF

0x0000271D

WSAEACCES

0x0000271E

WSAEFAULT

0x00002726

WSAEINVAL

0x00002728

WSAEMFILE

0x00002733

WSAEWOULDBLOCK

0x00002734

WSAEINPROGRESS

0x00002735

WSAEALREADY

0x00002736

WSAENOTSOCK

0x00002737

WSAEDESTADDRREQ

0x00002738

WSAEMSGSIZE

0x00002739

WSAEPROTOTYPE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

available at this time. Wait a few
minutes and try again.

The application directory
partition operation failed. The
domain controller holding the
domain naming master role is
down or unable to service the
request or is not running
Windows Server 2003.

A blocking operation was
interrupted by a call to
WSACancelBlockingCall.

The file handle supplied is not
valid.

An attempt was made to access
a socket in a way forbidden by
its access permissions.

The system detected an invalid
pointer address in attempting to
use a pointer argument in a call.

An invalid argument was
supplied.

Too many open sockets.

A nonblocking socket operation
could not be completed
immediately.

A blocking operation is currently
executing.

An operation was attempted on a
nonblocking socket that already
had an operation in progress.

An operation was attempted on
something that is not a socket.

A required address was omitted
from an operation on a socket.

A message sent on a datagram
socket was larger than the
internal message buffer or some
other network limit, or the buffer
used to receive a datagram into
was smaller than the datagram
itself.

A protocol was specified in the
socket function call that does not
support the semantics of the
socket type requested.

350 / 497


Win32 error codes

0x0000273A

WSAENOPROTOOPT

0x0000273B

WSAEPROTONOSUPPORT

0x0000273C

WSAESOCKTNOSUPPORT

0x0000273D

WSAEOPNOTSUPP

0x0000273E

WSAEPFNOSUPPORT

0x0000273F

WSAEAFNOSUPPORT

0x00002740

WSAEADDRINUSE

0x00002741

WSAEADDRNOTAVAIL

0x00002742

WSAENETDOWN

0x00002743

WSAENETUNREACH

0x00002744

WSAENETRESET

0x00002745

WSAECONNABORTED

0x00002746

WSAECONNRESET

0x00002747

WSAENOBUFS

0x00002748

WSAEISCONN

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An unknown, invalid, or
unsupported option or level was
specified in a getsockopt or
setsockopt call.

The requested protocol has not
been configured into the system,
or no implementation for it
exists.

The support for the specified
socket type does not exist in this
address family.

The attempted operation is not
supported for the type of object
referenced.

The protocol family has not been
configured into the system or no
implementation for it exists.

An address incompatible with the
requested protocol was used.

Only one usage of each socket
address (protocol/network
address/port) is normally
permitted.

The requested address is not
valid in its context.

A socket operation encountered
a dead network.

A socket operation was
attempted to an unreachable
network.

The connection has been broken
due to keep-alive activity
detecting a failure while the
operation was in progress.

An established connection was
aborted by the software in your
host machine.

An existing connection was
forcibly closed by the remote
host.

An operation on a socket could
not be performed because the
system lacked sufficient buffer
space or because a queue was
full.

A connect request was made on
an already connected socket.

351 / 497


Win32 error codes

0x00002749

WSAENOTCONN

0x0000274A

WSAESHUTDOWN

0x0000274B

WSAETOOMANYREFS

0x0000274C

WSAETIMEDOUT

0x0000274D

WSAECONNREFUSED

0x0000274E

WSAELOOP

0x0000274F

WSAENAMETOOLONG

0x00002750

WSAEHOSTDOWN

0x00002751

WSAEHOSTUNREACH

0x00002752

WSAENOTEMPTY

0x00002753

WSAEPROCLIM

0x00002754

WSAEUSERS

0x00002755

WSAEDQUOT

0x00002756

WSAESTALE

0x00002757

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A request to send or receive data
was disallowed because the
socket is not connected and
(when sending on a datagram
socket using a sendto call) no
address was supplied.

A request to send or receive data
was disallowed because the
socket had already been shut
down in that direction with a
previous shutdown call.

Too many references to a kernel
object.

A connection attempt failed
because the connected party did
not properly respond after a
period of time, or the established
connection failed because the
connected host failed to respond.

No connection could be made
because the target machine
actively refused it.

Cannot translate name.

Name or name component was
too long.

A socket operation failed
because the destination host was
down.

A socket operation was
attempted to an unreachable
host.

Cannot remove a directory that
is not empty.

A Windows Sockets
implementation might have a
limit on the number of
applications that can use it
simultaneously.

Ran out of quota.

Ran out of disk quota.

File handle reference is no longer
available.

Item is not available locally.

352 / 497


Win32 error codes

WSAEREMOTE

0x0000276B

WSASYSNOTREADY

0x0000276C

WSAVERNOTSUPPORTED

0x0000276D

WSANOTINITIALISED

0x00002775

WSAEDISCON

0x00002776

WSAENOMORE

0x00002777

WSAECANCELLED

0x00002778

WSAEINVALIDPROCTABLE

0x00002779

WSAEINVALIDPROVIDER

0x0000277A

WSAEPROVIDERFAILEDINIT

0x0000277B

WSASYSCALLFAILURE

0x0000277C

WSASERVICE_NOT_FOUND

0x0000277D

WSATYPE_NOT_FOUND

0x0000277E

WSA_E_NO_MORE

0x0000277F

WSA_E_CANCELLED

0x00002780

WSAEREFUSED

0x00002AF9

WSAHOST_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

WSAStartup cannot function at
this time because the underlying
system it uses to provide
network services is currently
unavailable.

The Windows Sockets version
requested is not supported.

Either the application has not
called WSAStartup, or
WSAStartup failed.

Returned by WSARecv or
WSARecvFrom to indicate that
the remote party has initiated a
graceful shutdown sequence.

No more results can be returned
by WSALookupServiceNext.

A call to WSALookupServiceEnd
was made while this call was still
processing. The call has been
canceled.

The procedure call table is
invalid.

The requested service provider is
invalid.

The requested service provider
could not be loaded or initialized.

A system call that should never
fail has failed.

No such service is known. The
service cannot be found in the
specified namespace.

The specified class was not
found.

No more results can be returned
by WSALookupServiceNext.

A call to WSALookupServiceEnd
was made while this call was still
processing. The call has been
canceled.

A database query failed because
it was actively refused.

No such host is known.

353 / 497


Win32 error codes

0x00002AFA

WSATRY_AGAIN

0x00002AFB

WSANO_RECOVERY

0x00002AFC

WSANO_DATA

0x00002AFD

WSA_QOS_RECEIVERS

0x00002AFE

WSA_QOS_SENDERS

0x00002AFF

WSA_QOS_NO_SENDERS

0x00002B00

WSA_QOS_NO_RECEIVERS

0x00002B01

WSA_QOS_REQUEST_CONFIRMED

0x00002B02

WSA_QOS_ADMISSION_FAILURE

0x00002B03

WSA_QOS_POLICY_FAILURE

0x00002B04

WSA_QOS_BAD_STYLE

0x00002B05

WSA_QOS_BAD_OBJECT

0x00002B06

WSA_QOS_TRAFFIC_CTRL_ERROR

0x00002B07

WSA_QOS_GENERIC_ERROR

0x00002B08

WSA_QOS_ESERVICETYPE

0x00002B09

WSA_QOS_EFLOWSPEC

0x00002B0A

WSA_QOS_EPROVSPECBUF

0x00002B0B

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

This is usually a temporary error
during host name resolution and
means that the local server did
not receive a response from an
authoritative server.

A nonrecoverable error occurred
during a database lookup.

The requested name is valid, but
no data of the requested type
was found.

At least one reserve has arrived.

At least one path has arrived.

There are no senders.

There are no receivers.

Reserve has been confirmed.

Error due to lack of resources.

Rejected for administrative
reasons—bad credentials.

Unknown or conflicting style.

There is a problem with some
part of the filterspec or provider-
specific buffer in general.

There is a problem with some
part of the flowspec.

General quality of serve (QOS)
error.

An invalid or unrecognized
service type was found in the
flowspec.

An invalid or inconsistent
flowspec was found in the QOS
structure.

Invalid QOS provider-specific
buffer.

An invalid QOS filter style was

354 / 497


Win32 error codes

WSA_QOS_EFILTERSTYLE

0x00002B0C

WSA_QOS_EFILTERTYPE

0x00002B0D

WSA_QOS_EFILTERCOUNT

0x00002B0E

WSA_QOS_EOBJLENGTH

0x00002B0F

WSA_QOS_EFLOWCOUNT

0x00002B10

WSA_QOS_EUNKOWNPSOBJ

0x00002B11

WSA_QOS_EPOLICYOBJ

0x00002B12

WSA_QOS_EFLOWDESC

0x00002B13

WSA_QOS_EPSFLOWSPEC

0x00002B14

WSA_QOS_EPSFILTERSPEC

0x00002B15

WSA_QOS_ESDMODEOBJ

0x00002B16

WSA_QOS_ESHAPERATEOBJ

0x00002B17

WSA_QOS_RESERVED_PETYPE

0x000032C8

ERROR_IPSEC_QM_POLICY_EXISTS

0x000032C9

ERROR_IPSEC_QM_POLICY_NOT_FOUND

0x000032CA

ERROR_IPSEC_QM_POLICY_IN_USE

0x000032CB

ERROR_IPSEC_MM_POLICY_EXISTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

used.

An invalid QOS filter type was
used.

An incorrect number of QOS
FILTERSPECs were specified in
the FLOWDESCRIPTOR.

An object with an invalid
ObjectLength field was
specified in the QOS provider-
specific buffer.

An incorrect number of flow
descriptors was specified in the
QOS structure.

An unrecognized object was
found in the QOS provider-
specific buffer.

An invalid policy object was
found in the QOS provider-
specific buffer.

An invalid QOS flow descriptor
was found in the flow descriptor
list.

An invalid or inconsistent
flowspec was found in the QOS
provider-specific buffer.

An invalid FILTERSPEC was
found in the QOS provider-
specific buffer.

An invalid shape discard mode
object was found in the QOS
provider-specific buffer.

An invalid shaping rate object
was found in the QOS provider-
specific buffer.

A reserved policy element was
found in the QOS provider-
specific buffer.

The specified quick mode policy
already exists.

The specified quick mode policy
was not found.

The specified quick mode policy
is being used.

The specified main mode policy
already exists.

355 / 497


Win32 error codes

0x000032CC

ERROR_IPSEC_MM_POLICY_NOT_FOUND

0x000032CD

ERROR_IPSEC_MM_POLICY_IN_USE

0x000032CE

ERROR_IPSEC_MM_FILTER_EXISTS

0x000032CF

ERROR_IPSEC_MM_FILTER_NOT_FOUND

0x000032D0

ERROR_IPSEC_TRANSPORT_FILTER_EXISTS

0x000032D1

ERROR_IPSEC_TRANSPORT_FILTER_NOT_FOUND

0x000032D2

ERROR_IPSEC_MM_AUTH_EXISTS

0x000032D3

ERROR_IPSEC_MM_AUTH_NOT_FOUND

0x000032D4

ERROR_IPSEC_MM_AUTH_IN_USE

0x000032D5

ERROR_IPSEC_DEFAULT_MM_POLICY_NOT_FOUND

0x000032D6

ERROR_IPSEC_DEFAULT_MM_AUTH_NOT_FOUND

0x000032D7

ERROR_IPSEC_DEFAULT_QM_POLICY_NOT_FOUND

0x000032D8

ERROR_IPSEC_TUNNEL_FILTER_EXISTS

0x000032D9

ERROR_IPSEC_TUNNEL_FILTER_NOT_FOUND

0x000032DA

ERROR_IPSEC_MM_FILTER_PENDING_DELETION

0x000032DB

ERROR_IPSEC_TRANSPORT_FILTER_ENDING_DELETION

0x000032DC

ERROR_IPSEC_TUNNEL_FILTER_PENDING_DELETION

0x000032DD

ERROR_IPSEC_MM_POLICY_PENDING_ELETION

0x000032DE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified main mode policy
was not found.

The specified main mode policy
is being used.

The specified main mode filter
already exists.

The specified main mode filter
was not found.

The specified transport mode
filter already exists.

The specified transport mode
filter does not exist.

The specified main mode
authentication list exists.

The specified main mode
authentication list was not
found.

The specified main mode
authentication list is being used.

The specified default main mode
policy was not found.

The specified default main mode
authentication list was not
found.

The specified default quick mode
policy was not found.

The specified tunnel mode filter
exists.

The specified tunnel mode filter
was not found.

The main mode filter is pending
deletion.

The transport filter is pending
deletion.

The tunnel filter is pending
deletion.

The main mode policy is pending
deletion.

The main mode authentication
bundle is pending deletion.

356 / 497


Win32 error codes

Description

ERROR_IPSEC_MM_AUTH_PENDING_DELETION

0x000032DF

ERROR_IPSEC_QM_POLICY_PENDING_DELETION

0x000032E0

WARNING_IPSEC_MM_POLICY_PRUNED

0x000032E1

WARNING_IPSEC_QM_POLICY_PRUNED

0x000035E8

ERROR_IPSEC_IKE_NEG_STATUS_BEGIN

0x000035E9

ERROR_IPSEC_IKE_AUTH_FAIL

0x000035EA

ERROR_IPSEC_IKE_ATTRIB_FAIL

0x000035EB

ERROR_IPSEC_IKE_NEGOTIATION_PENDING

0x000035EC

ERROR_IPSEC_IKE_GENERAL_PROCESSING_ERROR

0x000035ED

ERROR_IPSEC_IKE_TIMED_OUT

0x000035EE

ERROR_IPSEC_IKE_NO_CERT

0x000035EF

ERROR_IPSEC_IKE_SA_DELETED

0x000035F0

ERROR_IPSEC_IKE_SA_REAPED

0x000035F1

ERROR_IPSEC_IKE_MM_ACQUIRE_DROP

0x000035F2

ERROR_IPSEC_IKE_QM_ACQUIRE_DROP

0x000035F3

ERROR_IPSEC_IKE_QUEUE_DROP_MM

0x000035F4

ERROR_IPSEC_IKE_QUEUE_DROP_NO_MM

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The quick mode policy is pending
deletion.

The main mode policy was
successfully added, but some of
the requested offers are not
supported.

The quick mode policy was
successfully added, but some of
the requested offers are not
supported.

Starts the list of frequencies of
various IKE Win32 error codes
encountered during negotiations.

The IKE authentication
credentials are unacceptable.

The IKE security attributes are
unacceptable.

The IKE negotiation is in
progress.

General processing error.

Negotiation timed out.

The IKE failed to find a valid
machine certificate. Contact your
network security administrator
about installing a valid certificate
in the appropriate certificate
store.

The IKE security association (SA)
was deleted by a peer before it
was completely established.

The IKE SA was deleted before it
was completely established.

The negotiation request sat in
the queue too long.

The negotiation request sat in
the queue too long.

The negotiation request sat in
the queue too long.

The negotiation request sat in
the queue too long.

357 / 497


Win32 error codes

0x000035F5

ERROR_IPSEC_IKE_DROP_NO_RESPONSE

0x000035F6

ERROR_IPSEC_IKE_MM_DELAY_DROP

0x000035F7

ERROR_IPSEC_IKE_QM_DELAY_DROP

0x000035F8

ERROR_IPSEC_IKE_ERROR

0x000035F9

ERROR_IPSEC_IKE_CRL_FAILED

0x000035FA

ERROR_IPSEC_IKE_INVALID_KEY_USAGE

0x000035FB

ERROR_IPSEC_IKE_INVALID_CERT_TYPE

0x000035FC

ERROR_IPSEC_IKE_NO_PRIVATE_KEY

0x000035FE

ERROR_IPSEC_IKE_DH_FAIL

0x00003600

ERROR_IPSEC_IKE_INVALID_HEADER

0x00003601

ERROR_IPSEC_IKE_NO_POLICY

0x00003602

ERROR_IPSEC_IKE_INVALID_SIGNATURE

0x00003603

ERROR_IPSEC_IKE_KERBEROS_ERROR

0x00003604

ERROR_IPSEC_IKE_NO_PUBLIC_KEY

0x00003605

ERROR_IPSEC_IKE_PROCESS_ERR

0x00003606

ERROR_IPSEC_IKE_PROCESS_ERR_SA

0x00003607

ERROR_IPSEC_IKE_PROCESS_ERR_PROP

0x00003608

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

There was no response from a
peer.

The negotiation took too long.

The negotiation took too long.

An unknown error occurred.

The certificate revocation check
failed.

Invalid certificate key usage.

Invalid certificate type.

The IKE negotiation failed
because the machine certificate
used does not have a private
key. IPsec certificates require a
private key. Contact your
network security administrator
about a certificate that has a
private key.

There was a failure in the Diffie-
Hellman computation.

Invalid header.

No policy configured.

Failed to verify signature.

Failed to authenticate using
Kerberos.

The peer's certificate did not
have a public key.

Error processing the error
payload.

Error processing the SA payload.

Error processing the proposal
payload.

Error processing the transform

358 / 497


Win32 error codes

ERROR_IPSEC_IKE_PROCESS_ERR_TRANS

0x00003609

ERROR_IPSEC_IKE_PROCESS_ERR_KE

0x0000360A

ERROR_IPSEC_IKE_PROCESS_ERR_ID

0x0000360B

ERROR_IPSEC_IKE_PROCESS_ERR_CERT

0x0000360C

ERROR_IPSEC_IKE_PROCESS_ERR_CERT_REQ

0x0000360D

ERROR_IPSEC_IKE_PROCESS_ERR_HASH

0x0000360E

ERROR_IPSEC_IKE_PROCESS_ERR_SIG

0x0000360F

ERROR_IPSEC_IKE_PROCESS_ERR_NONCE

0x00003610

ERROR_IPSEC_IKE_PROCESS_ERR_NOTIFY

0x00003611

ERROR_IPSEC_IKE_PROCESS_ERR_DELETE

0x00003612

ERROR_IPSEC_IKE_PROCESS_ERR_VENDOR

0x00003613

ERROR_IPSEC_IKE_INVALID_PAYLOAD

0x00003614

ERROR_IPSEC_IKE_LOAD_SOFT_SA

0x00003615

ERROR_IPSEC_IKE_SOFT_SA_TORN_DOWN

0x00003616

ERROR_IPSEC_IKE_INVALID_COOKIE

0x00003617

ERROR_IPSEC_IKE_NO_PEER_CERT

0x00003618

ERROR_IPSEC_IKE_PEER_CRL_FAILED

0x00003619

ERROR_IPSEC_IKE_POLICY_CHANGE

0x0000361A

ERROR_IPSEC_IKE_NO_MM_POLICY

0x0000361B

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

payload.

Error processing the key
exchange payload.

Error processing the ID payload.

Error processing the certification
payload.

Error processing the certificate
request payload.

Error processing the hash
payload.

Error processing the signature
payload.

Error processing the nonce
payload.

Error processing the notify
payload.

Error processing the delete
payload.

Error processing the VendorId
payload.

Invalid payload received.

Soft SA loaded.

Soft SA torn down.

Invalid cookie received.

Peer failed to send valid machine
certificate.

Certification revocation check of
peer's certificate failed.

New policy invalidated SAs
formed with the old policy.

There is no available main mode
IKE policy.

Failed to enabled trusted

359 / 497


Win32 error codes

ERROR_IPSEC_IKE_NOTCBPRIV

0x0000361C

ERROR_IPSEC_IKE_SECLOADFAIL

0x0000361D

ERROR_IPSEC_IKE_FAILSSPINIT

0x0000361E

ERROR_IPSEC_IKE_FAILQUERYSSP

0x0000361F

ERROR_IPSEC_IKE_SRVACQFAIL

0x00003620

ERROR_IPSEC_IKE_SRVQUERYCRED

0x00003621

ERROR_IPSEC_IKE_GETSPIFAIL

0x00003622

ERROR_IPSEC_IKE_INVALID_FILTER

0x00003623

ERROR_IPSEC_IKE_OUT_OF_MEMORY

0x00003624

ERROR_IPSEC_IKE_ADD_UPDATE_KEY_FAILED

0x00003625

ERROR_IPSEC_IKE_INVALID_POLICY

0x00003626

ERROR_IPSEC_IKE_UNKNOWN_DOI

0x00003627

ERROR_IPSEC_IKE_INVALID_SITUATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

computer base (TCB) privilege.

Failed to load SECURITY.DLL.

Failed to obtain the security
function table dispatch address
from the SSPI.

Failed to query the Kerberos
package to obtain the max token
size.

Failed to obtain the Kerberos
server credentials for the
Internet Security Association and
Key Management Protocol
(ISAKMP)/ERROR_IPSEC_IKE
service. Kerberos authentication
will not function. The most likely
reason for this is lack of domain
membership. This is normal if
your computer is a member of a
workgroup.

Failed to determine the SSPI
principal name for
ISAKMP/ERROR_IPSEC_IKE
service
(QueryCredentialsAttributes).

Failed to obtain a new service
provider interface (SPI) for the
inbound SA from the IPsec
driver. The most common cause
for this is that the driver does
not have the correct filter. Check
your policy to verify the filters.

Given filter is invalid.

Memory allocation failed.

Failed to add an SA to the IPSec
driver. The most common cause
for this is if the IKE negotiation
took too long to complete. If the
problem persists, reduce the
load on the faulting machine.

Invalid policy.

Invalid digital object identifier
(DOI).

Invalid situation.

360 / 497


Win32 error codes

0x00003628

ERROR_IPSEC_IKE_DH_FAILURE

0x00003629

ERROR_IPSEC_IKE_INVALID_GROUP

0x0000362A

ERROR_IPSEC_IKE_ENCRYPT

0x0000362B

ERROR_IPSEC_IKE_DECRYPT

0x0000362C

ERROR_IPSEC_IKE_POLICY_MATCH

0x0000362D

ERROR_IPSEC_IKE_UNSUPPORTED_ID

0x0000362E

ERROR_IPSEC_IKE_INVALID_HASH

0x0000362F

ERROR_IPSEC_IKE_INVALID_HASH_ALG

0x00003630

ERROR_IPSEC_IKE_INVALID_HASH_SIZE

0x00003631

ERROR_IPSEC_IKE_INVALID_ENCRYPT_ALG

0x00003632

ERROR_IPSEC_IKE_INVALID_AUTH_ALG

0x00003633

ERROR_IPSEC_IKE_INVALID_SIG

0x00003634

ERROR_IPSEC_IKE_LOAD_FAILED

0x00003635

ERROR_IPSEC_IKE_RPC_DELETE

0x00003636

ERROR_IPSEC_IKE_BENIGN_REINIT

0x00003637

ERROR_IPSEC_IKE_INVALID_RESPONDER_LIFETIME_NOTIFY

0x00003639

ERROR_IPSEC_IKE_INVALID_CERT_KEYLEN

0x0000363A

ERROR_IPSEC_IKE_MM_LIMIT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Diffie-Hellman failure.

Invalid Diffie-Hellman group.

Error encrypting payload.

Error decrypting payload.

Policy match error.

Unsupported ID.

Hash verification failed.

Invalid hash algorithm.

Invalid hash size.

Invalid encryption algorithm.

Invalid authentication algorithm.

Invalid certificate signature.

Load failed.

Deleted by using an RPC call.

A temporary state was created
to perform reinitialization. This is
not a real failure.

The lifetime value received in the
Responder Lifetime Notify is
below the Windows 2000
configured minimum value. Fix
the policy on the peer machine.

Key length in the certificate is
too small for configured security
requirements.

Maximum number of established
MM SAs to peer exceeded.

361 / 497


Win32 error codes

0x0000363B

ERROR_IPSEC_IKE_NEGOTIATION_DISABLED

0x0000363C

ERROR_IPSEC_IKE_QM_LIMIT

0x0000363D

ERROR_IPSEC_IKE_MM_EXPIRED

0x0000363E

ERROR_IPSEC_IKE_PEER_MM_ASSUMED_INVALID

0x0000363F

ERROR_IPSEC_IKE_CERT_CHAIN_POLICY_MISMATCH

0x00003640

ERROR_IPSEC_IKE_UNEXPECTED_MESSAGE_ID

0x00003641

ERROR_IPSEC_IKE_INVALID_UMATTS

0x00003642

ERROR_IPSEC_IKE_DOS_COOKIE_SENT

0x00003643

ERROR_IPSEC_IKE_SHUTTING_DOWN

0x00003644

ERROR_IPSEC_IKE_CGA_AUTH_FAILED

0x00003645

ERROR_IPSEC_IKE_PROCESS_ERR_NATOA

0x00003646

ERROR_IPSEC_IKE_INVALID_MM_FOR_QM

0x00003647

ERROR_IPSEC_IKE_QM_EXPIRED

0x00003648

ERROR_IPSEC_IKE_TOO_MANY_FILTERS

0x00003649

ERROR_IPSEC_IKE_NEG_STATUS_END

0x000036B0

ERROR_SXS_SECTION_NOT_FOUND

0x000036B1

ERROR_SXS_CANT_GEN_ACTCTX

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The IKE received a policy that
disables negotiation.

Reached maximum quick mode
limit for the main mode. New
main mode will be started.

Main mode SA lifetime expired or
the peer sent a main mode
delete.

Main mode SA assumed to be
invalid because peer stopped
responding.

Certificate does not chain to a
trusted root in IPsec policy.

Received unexpected message
ID.

Received invalid AuthIP user
mode attributes.

Sent DOS cookie notify to
initiator.

The IKE service is shutting down.

Could not verify the binding
between the color graphics
adapter (CGA) address and the
certificate.

Error processing the NatOA
payload.

The parameters of the main
mode are invalid for this quick
mode.

The quick mode SA was expired
by the IPsec driver.

Too many dynamically added
IKEEXT filters were detected.

Ends the list of frequencies of
various IKE Win32 error codes
encountered during negotiations.

The requested section was not
present in the activation context.

The application has failed to
start because its side-by-side
configuration is incorrect. See
the application event log for

362 / 497


Win32 error codes

0x000036B2

ERROR_SXS_INVALID_ACTCTXDATA_FORMAT

0x000036B3

ERROR_SXS_ASSEMBLY_NOT_FOUND

0x000036B4

ERROR_SXS_MANIFEST_FORMAT_ERROR

0x000036B5

ERROR_SXS_MANIFEST_PARSE_ERROR

0x000036B6

ERROR_SXS_ACTIVATION_CONTEXT_DISABLED

0x000036B7

ERROR_SXS_KEY_NOT_FOUND

0x000036B8

ERROR_SXS_VERSION_CONFLICT

0x000036B9

ERROR_SXS_WRONG_SECTION_TYPE

0x000036BA

ERROR_SXS_THREAD_QUERIES_DISABLED

0x000036BB

ERROR_SXS_PROCESS_DEFAULT_ALREADY_SET

0x000036BC

ERROR_SXS_UNKNOWN_ENCODING_GROUP

0x000036BD

ERROR_SXS_UNKNOWN_ENCODING

0x000036BE

ERROR_SXS_INVALID_XML_NAMESPACE_URI

0x000036BF

ERROR_SXS_ROOT_MANIFEST_DEPENDENCY_OT_INSTALLED

0x000036C0

ERROR_SXS_LEAF_MANIFEST_DEPENDENCY_NOT_INSTALLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

more detail.

The application binding data
format is invalid.

The referenced assembly is not
installed on your system.

The manifest file does not begin
with the required tag and format
information.

The manifest file contains one or
more syntax errors.

The application attempted to
activate a disabled activation
context.

The requested lookup key was
not found in any active
activation context.

A component version required by
the application conflicts with
another active component
version.

The type requested activation
context section does not match
the query API used.

Lack of system resources has
required isolated activation to be
disabled for the current thread of
execution.

An attempt to set the process
default activation context failed
because the process default
activation context was already
set.

The encoding group identifier
specified is not recognized.

The encoding requested is not
recognized.

The manifest contains a
reference to an invalid URI.

The application manifest
contains a reference to a
dependent assembly that is not
installed.

The manifest for an assembly
used by the application has a
reference to a dependent
assembly that is not installed.

363 / 497


Win32 error codes

0x000036C1

ERROR_SXS_INVALID_ASSEMBLY_IDENTITY_ATTRIBUTE

0x000036C2

ERROR_SXS_MANIFEST_MISSING_REQUIRED_DEFAULT_NAMESPACE

0x000036C3

ERROR_SXS_MANIFEST_INVALID_REQUIRED_DEFAULT_NAMESPACE

0x000036C4

ERROR_SXS_PRIVATE_MANIFEST_CROSS_PATH_WITH_REPARSE_POINT

0x000036C5

ERROR_SXS_DUPLICATE_DLL_NAME

0x000036C6

ERROR_SXS_DUPLICATE_WINDOWCLASS_NAME

0x000036C7

ERROR_SXS_DUPLICATE_CLSID

0x000036C8

ERROR_SXS_DUPLICATE_IID

0x000036C9

ERROR_SXS_DUPLICATE_TLBID

0x000036CA

ERROR_SXS_DUPLICATE_PROGID

0x000036CB

ERROR_SXS_DUPLICATE_ASSEMBLY_NAME

0x000036CC

ERROR_SXS_FILE_HASH_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The manifest contains an
attribute for the assembly
identity that is not valid.

The manifest is missing the
required default namespace
specification on the assembly
element.

The manifest has a default
namespace specified on the
assembly element but its value
is not urn:schemas-microsoft-
com:asm.v1"."

The private manifest probed has
crossed the reparse-point-
associated path.

Two or more components
referenced directly or indirectly
by the application manifest have
files by the same name.

Two or more components
referenced directly or indirectly
by the application manifest have
window classes with the same
name.

Two or more components
referenced directly or indirectly
by the application manifest have
the same COM server CLSIDs.

Two or more components
referenced directly or indirectly
by the application manifest have
proxies for the same COM
interface IIDs.

Two or more components
referenced directly or indirectly
by the application manifest have
the same COM type library
TLBIDs.

Two or more components
referenced directly or indirectly
by the application manifest have
the same COM ProgIDs.

Two or more components
referenced directly or indirectly
by the application manifest are
different versions of the same
component, which is not
permitted.

A component's file does not
match the verification
information present in the

364 / 497


Win32 error codes

0x000036CD

ERROR_SXS_POLICY_PARSE_ERROR

0x000036CE

ERROR_SXS_XML_E_MISSINGQUOTE

0x000036CF

ERROR_SXS_XML_E_COMMENTSYNTAX

0x000036D0

ERROR_SXS_XML_E_BADSTARTNAMECHAR

0x000036D1

ERROR_SXS_XML_E_BADNAMECHAR

0x000036D2

ERROR_SXS_XML_E_BADCHARINSTRING

0x000036D3

ERROR_SXS_XML_E_XMLDECLSYNTAX

0x000036D4

ERROR_SXS_XML_E_BADCHARDATA

0x000036D5

ERROR_SXS_XML_E_MISSINGWHITESPACE

0x000036D6

ERROR_SXS_XML_E_EXPECTINGTAGEND

0x000036D7

ERROR_SXS_XML_E_MISSINGSEMICOLON

0x000036D8

ERROR_SXS_XML_E_UNBALANCEDPAREN

0x000036D9

ERROR_SXS_XML_E_INTERNALERROR

0x000036DA

ERROR_SXS_XML_E_UNEXPECTED_WHITESPACE

0x000036DB

ERROR_SXS_XML_E_INCOMPLETE_ENCODING

0x000036DC

ERROR_SXS_XML_E_MISSING_PAREN

0x000036DD

ERROR_SXS_XML_E_EXPECTINGCLOSEQUOTE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

component manifest.

The policy manifest contains one
or more syntax errors.

Manifest Parse Error: A string
literal was expected, but no
opening quotation mark was
found.

Manifest Parse Error: Incorrect
syntax was used in a comment.

Manifest Parse Error: A name
started with an invalid character.

Manifest Parse Error: A name
contained an invalid character.

Manifest Parse Error: A string
literal contained an invalid
character.

Manifest Parse Error: Invalid
syntax for an XML declaration.

Manifest Parse Error: An Invalid
character was found in text
content.

Manifest Parse Error: Required
white space was missing.

Manifest Parse Error: The angle
bracket (>) character was
expected.

Manifest Parse Error: A
semicolon (;) was expected.

Manifest Parse Error: Unbalanced
parentheses.

Manifest Parse Error: Internal
error.

Manifest Parse Error: Whitespace
is not allowed at this location.

Manifest Parse Error: End of file
reached in invalid state for
current encoding.

Manifest Parse Error: Missing
parenthesis.

Manifest Parse Error: A single (')
or double (") quotation mark is
missing.

365 / 497


Win32 error codes

0x000036DE

ERROR_SXS_XML_E_MULTIPLE_COLONS

0x000036DF

ERROR_SXS_XML_E_INVALID_DECIMAL

0x000036E0

ERROR_SXS_XML_E_INVALID_HEXIDECIMAL

0x000036E1

ERROR_SXS_XML_E_INVALID_UNICODE

0x000036E2

ERROR_SXS_XML_E_WHITESPACEORQUESTIONMARK

0x000036E3

ERROR_SXS_XML_E_UNEXPECTEDENDTAG

0x000036E4

ERROR_SXS_XML_E_UNCLOSEDTAG

0x000036E5

ERROR_SXS_XML_E_DUPLICATEATTRIBUTE

0x000036E6

ERROR_SXS_XML_E_MULTIPLEROOTS

0x000036E7

ERROR_SXS_XML_E_INVALIDATROOTLEVEL

0x000036E8

ERROR_SXS_XML_E_BADXMLDECL

0x000036E9

ERROR_SXS_XML_E_MISSINGROOT

0x000036EA

ERROR_SXS_XML_E_UNEXPECTEDEOF

0x000036EB

ERROR_SXS_XML_E_BADPEREFINSUBSET

0x000036EC

ERROR_SXS_XML_E_UNCLOSEDSTARTTAG

0x000036ED

ERROR_SXS_XML_E_UNCLOSEDENDTAG

0x000036EE

ERROR_SXS_XML_E_UNCLOSEDSTRING

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Manifest Parse Error: Multiple
colons are not allowed in a
name.

Manifest Parse Error: Invalid
character for decimal digit.

Manifest Parse Error: Invalid
character for hexadecimal digit.

Manifest Parse Error: Invalid
Unicode character value for this
platform.

Manifest Parse Error: Expecting
whitespace or question mark (?).

Manifest Parse Error: End tag
was not expected at this
location.

Manifest Parse Error: The
following tags were not closed:
%1.

Manifest Parse Error: Duplicate
attribute.

Manifest Parse Error: Only one
top-level element is allowed in
an XML document.

Manifest Parse Error: Invalid at
the top level of the document.

Manifest Parse Error: Invalid
XML declaration.

Manifest Parse Error: XML
document must have a top-level
element.

Manifest Parse Error:
Unexpected end of file.

Manifest Parse Error: Parameter
entities cannot be used inside
markup declarations in an
internal subset.

Manifest Parse Error: Element
was not closed.

Manifest Parse Error: End
element was missing the angle
bracket (>) character.

Manifest Parse Error: A string
literal was not closed.

366 / 497


Win32 error codes

0x000036EF

ERROR_SXS_XML_E_UNCLOSEDCOMMENT

0x000036F0

ERROR_SXS_XML_E_UNCLOSEDDECL

0x000036F1

ERROR_SXS_XML_E_UNCLOSEDCDATA

0x000036F2

ERROR_SXS_XML_E_RESERVEDNAMESPACE

0x000036F3

ERROR_SXS_XML_E_INVALIDENCODING

0x000036F4

ERROR_SXS_XML_E_INVALIDSWITCH

0x000036F5

ERROR_SXS_XML_E_BADXMLCASE

0x000036F6

ERROR_SXS_XML_E_INVALID_STANDALONE

0x000036F7

ERROR_SXS_XML_E_UNEXPECTED_STANDALONE

0x000036F8

ERROR_SXS_XML_E_INVALID_VERSION

0x000036F9

ERROR_SXS_XML_E_MISSINGEQUALS

0x000036FA

ERROR_SXS_PROTECTION_RECOVERY_FAILED

0x000036FB

ERROR_SXS_PROTECTION_PUBLIC_KEY_OO_SHORT

0x000036FC

ERROR_SXS_PROTECTION_CATALOG_NOT_VALID

0x000036FD

ERROR_SXS_UNTRANSLATABLE_HRESULT

0x000036FE

ERROR_SXS_PROTECTION_CATALOG_FILE_MISSING

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Manifest Parse Error: A comment
was not closed.

Manifest Parse Error: A
declaration was not closed.

Manifest Parse Error: A CDATA
section was not closed.

Manifest Parse Error: The
namespace prefix is not allowed
to start with the reserved string
xml"."

Manifest Parse Error: System
does not support the specified
encoding.

Manifest Parse Error: Switch
from current encoding to
specified encoding not
supported.

Manifest Parse Error: The name
"xml" is reserved and must be
lowercase.

Manifest Parse Error: The stand-
alone attribute must have the
value "yes" or "no".

Manifest Parse Error: The stand-
alone attribute cannot be used in
external entities.

Manifest Parse Error: Invalid
version number.

Manifest Parse Error: Missing
equal sign (=) between the
attribute and the attribute value.

Assembly Protection Error:
Unable to recover the specified
assembly.

Assembly Protection Error: The
public key for an assembly was
too short to be allowed.

Assembly Protection Error: The
catalog for an assembly is not
valid, or does not match the
assembly's manifest.

An HRESULT could not be
translated to a corresponding
Win32 error code.

Assembly Protection Error: The
catalog for an assembly is

367 / 497


Win32 error codes

0x000036FF

ERROR_SXS_MISSING_ASSEMBLY_IDENTITY_ATTRIBUTE

0x00003700

ERROR_SXS_INVALID_ASSEMBLY_IDENTITY_ATTRIBUTE_NAME

0x00003701

ERROR_SXS_ASSEMBLY_MISSING

0x00003702

ERROR_SXS_CORRUPT_ACTIVATION_STACK

0x00003703

ERROR_SXS_CORRUPTION

0x00003704

ERROR_SXS_EARLY_DEACTIVATION

0x00003705

ERROR_SXS_INVALID_DEACTIVATION

0x00003706

ERROR_SXS_MULTIPLE_DEACTIVATION

0x00003707

ERROR_SXS_PROCESS_TERMINATION_REQUESTED

0x00003708

ERROR_SXS_RELEASE_ACTIVATION_ONTEXT

0x00003709

ERROR_SXS_SYSTEM_DEFAULT_ACTIVATION_CONTEXT_EMPTY

0x0000370A

ERROR_SXS_INVALID_IDENTITY_ATTRIBUTE_VALUE

0x0000370B

ERROR_SXS_INVALID_IDENTITY_ATTRIBUTE_NAME

0x0000370C

ERROR_SXS_IDENTITY_DUPLICATE_ATTRIBUTE

0x0000370D

ERROR_SXS_IDENTITY_PARSE_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

missing.

The supplied assembly identity is
missing one or more attributes
that must be present in this
context.

The supplied assembly identity
has one or more attribute names
that contain characters not
permitted in XML names.

The referenced assembly could
not be found.

The activation context activation
stack for the running thread of
execution is corrupt.

The application isolation
metadata for this process or
thread has become corrupt.

The activation context being
deactivated is not the most
recently activated one.

The activation context being
deactivated is not active for the
current thread of execution.

The activation context being
deactivated has already been
deactivated.

A component used by the
isolation facility has requested to
terminate the process.

A kernel mode component is
releasing a reference on an
activation context.

The activation context of the
system default assembly could
not be generated.

The value of an attribute in an
identity is not within the legal
range.

The name of an attribute in an
identity is not within the legal
range.

An identity contains two
definitions for the same
attribute.

The identity string is malformed.
This might be due to a trailing
comma, more than two unnamed
attributes, a missing attribute

368 / 497


Win32 error codes

0x0000370E

ERROR_MALFORMED_SUBSTITUTION_STRING

0x0000370F

ERROR_SXS_INCORRECT_PUBLIC_KEY_OKEN

0x00003710

ERROR_UNMAPPED_SUBSTITUTION_STRING

0x00003711

ERROR_SXS_ASSEMBLY_NOT_LOCKED

0x00003712

ERROR_SXS_COMPONENT_STORE_CORRUPT

0x00003713

ERROR_ADVANCED_INSTALLER_FAILED

0x00003714

ERROR_XML_ENCODING_MISMATCH

0x00003715

ERROR_SXS_MANIFEST_IDENTITY_SAME_BUT_CONTENTS_DIFFERENT

0x00003716

ERROR_SXS_IDENTITIES_DIFFERENT

0x00003717

ERROR_SXS_ASSEMBLY_IS_NOT_A_DEPLOYMENT

0x00003718

ERROR_SXS_FILE_NOT_PART_OF_ASSEMBLY

0x00003719

ERROR_SXS_MANIFEST_TOO_BIG

0x0000371A

ERROR_SXS_SETTING_NOT_REGISTERED

0x0000371B

ERROR_SXS_TRANSACTION_CLOSURE_INCOMPLETE

0x00003A98

ERROR_EVT_INVALID_CHANNEL_PATH

0x00003A99

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

name, or a missing attribute
value.

A string containing localized
substitutable content was
malformed. Either a dollar sign
($) was followed by something
other than a left parenthesis or
another dollar sign, or a
substitution's right parenthesis
was not found.

The public key token does not
correspond to the public key
specified.

A substitution string had no
mapping.

The component must be locked
before making the request.

The component store has been
corrupted.

An advanced installer failed
during setup or servicing.

The character encoding in the
XML declaration did not match
the encoding used in the
document.

The identities of the manifests
are identical, but the contents
are different.

The component identities are
different.

The assembly is not a
deployment.

The file is not a part of the
assembly.

The size of the manifest exceeds
the maximum allowed.

The setting is not registered.

One or more required members
of the transaction are not
present.

The specified channel path is
invalid.

The specified query is invalid.

369 / 497


Win32 error codes

ERROR_EVT_INVALID_QUERY

0x00003A9A

ERROR_EVT_PUBLISHER_METADATA_NOT_FOUND

0x00003A9B

ERROR_EVT_EVENT_TEMPLATE_NOT_FOUND

0x00003A9C

ERROR_EVT_INVALID_PUBLISHER_NAME

0x00003A9D

ERROR_EVT_INVALID_EVENT_DATA

0x00003A9F

ERROR_EVT_CHANNEL_NOT_FOUND

0x00003AA0

ERROR_EVT_MALFORMED_XML_TEXT

0x00003AA1

ERROR_EVT_SUBSCRIPTION_TO_DIRECT_CHANNEL

0x00003AA2

ERROR_EVT_CONFIGURATION_ERROR

0x00003AA3

ERROR_EVT_QUERY_RESULT_STALE

0x00003AA4

ERROR_EVT_QUERY_RESULT_INVALID_POSITION

0x00003AA5

ERROR_EVT_NON_VALIDATING_MSXML

0x00003AA6

ERROR_EVT_FILTER_ALREADYSCOPED

0x00003AA7

ERROR_EVT_FILTER_NOTELTSET

0x00003AA8

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The publisher metadata cannot
be found in the resource.

The template for an event
definition cannot be found in the
resource (error = %1).

The specified publisher name is
invalid.

The event data raised by the
publisher is not compatible with
the event template definition in
the publisher's manifest.

The specified channel could not
be found. Check channel
configuration.

The specified XML text was not
well-formed. See extended error
for more details.

The caller is trying to subscribe
to a direct channel which is not
allowed. The events for a direct
channel go directly to a log file
and cannot be subscribed to.

Configuration error.

The query result is stale or
invalid. This might be due to the
log being cleared or rolling over
after the query result was
created. Users should handle this
code by releasing the query
result object and reissuing the
query.

Query result is currently at an
invalid position.

Registered Microsoft XML
(MSXML) does not support
validation.

An expression can only be
followed by a change-of-scope
operation if it itself evaluates to
a node set and is not already
part of some other change-of-
scope operation.

Cannot perform a step operation
from a term that does not
represent an element set.

Left side arguments to binary

370 / 497


Win32 error codes

ERROR_EVT_FILTER_INVARG

0x00003AA9

ERROR_EVT_FILTER_INVTEST

0x00003AAA

ERROR_EVT_FILTER_INVTYPE

0x00003AAB

ERROR_EVT_FILTER_PARSEERR

0x00003AAC

ERROR_EVT_FILTER_UNSUPPORTEDOP

0x00003AAD

ERROR_EVT_FILTER_UNEXPECTEDTOKEN

0x00003AAE

ERROR_EVT_INVALID_OPERATION_OVER_ENABLED_DIRECT_CHANNEL

0x00003AAF

ERROR_EVT_INVALID_CHANNEL_PROPERTY_VALUE

0x00003AB0

ERROR_EVT_INVALID_PUBLISHER_PROPERTY_VALUE

0x00003AB1

ERROR_EVT_CHANNEL_CANNOT_ACTIVATE

0x00003AB2

ERROR_EVT_FILTER_TOO_COMPLEX

0x00003AB3

ERROR_EVT_MESSAGE_NOT_FOUND

0x00003AB4

ERROR_EVT_MESSAGE_ID_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

operators must be either
attributes, nodes, or variables
and right side arguments must
be constants.

A step operation must involve
either a node test or, in the case
of a predicate, an algebraic
expression against which to test
each node in the node set
identified by the preceding node
set can be evaluated.

This data type is currently
unsupported.

A syntax error occurred at
position %1!d!

This operator is unsupported by
this implementation of the filter.

The token encountered was
unexpected.

The requested operation cannot
be performed over an enabled
direct channel. The channel must
first be disabled before
performing the requested
operation.

Channel property %1!s! contains
an invalid value. The value has
an invalid type, is outside the
valid range, cannot be updated,
or is not supported by this type
of channel.

Publisher property %1!s!
contains an invalid value. The
value has an invalid type, is
outside the valid range, cannot
be updated, or is not supported
by this type of publisher.

The channel fails to activate.

The xpath expression exceeded
supported complexity. Simplify it
or split it into two or more
simple expressions.

The message resource is present
but the message is not found in
the string or message table.

The message ID for the desired
message could not be found.

371 / 497


Win32 error codes

0x00003AB5

ERROR_EVT_UNRESOLVED_VALUE_INSERT

0x00003AB6

ERROR_EVT_UNRESOLVED_PARAMETER_INSERT

0x00003AB7

ERROR_EVT_MAX_INSERTS_REACHED

0x00003AB8

ERROR_EVT_EVENT_DEFINITION_NOT_OUND

0x00003AB9

ERROR_EVT_MESSAGE_LOCALE_NOT_FOUND

0x00003ABA

ERROR_EVT_VERSION_TOO_OLD

0x00003ABB

ERROR_EVT_VERSION_TOO_NEW

0x00003ABC

ERROR_EVT_CANNOT_OPEN_CHANNEL_OF_QUERY

0x00003ABD

ERROR_EVT_PUBLISHER_DISABLED

0x00003AE8

ERROR_EC_SUBSCRIPTION_CANNOT_ACTIVATE

0x00003AE9

ERROR_EC_LOG_DISABLED

0x00003AFC

ERROR_MUI_FILE_NOT_FOUND

0x00003AFD

ERROR_MUI_INVALID_FILE

0x00003AFE

ERROR_MUI_INVALID_RC_CONFIG

0x00003AFF

ERROR_MUI_INVALID_LOCALE_NAME

0x00003B00

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The substitution string for the
insert index (%1) could not be
found.

The description string for the
parameter reference (%1) could
not be found.

The maximum number of
replacements has been reached.

The event definition could not be
found for the event ID (%1).

The locale-specific resource for
the desired message is not
present.

The resource is too old to be
compatible.

The resource is too new to be
compatible.

The channel at index %1 of the
query cannot be opened.

The publisher has been disabled
and its resource is not available.
This usually occurs when the
publisher is in the process of
being uninstalled or upgraded.

The subscription fails to activate.

The log of the subscription is in a
disabled state and events cannot
be forwarded to it. The log must
first be enabled before the
subscription can be activated.

The resource loader failed to find
the Multilingual User Interface
(MUI) file.

The resource loader failed to
load the MUI file because the file
failed to pass validation.

The release candidate (RC)
manifest is corrupted with
garbage data, is an unsupported
version, or is missing a required
item.

The RC manifest has an invalid
culture name.

The RC Manifest has an invalid

372 / 497


Win32 error codes

Description

ERROR_MUI_INVALID_ULTIMATEFALLBACK_NAME

ultimate fallback name.

0x00003B01

ERROR_MUI_FILE_NOT_LOADED

0x00003B02

ERROR_RESOURCE_ENUM_USER_STOP

0x00003B03

ERROR_MUI_INTLSETTINGS_UILANG_NOT_INSTALLED

0x00003B04

ERROR_MUI_INTLSETTINGS_INVALID_LOCALE_NAME

0x00003B60

ERROR_MCA_INVALID_CAPABILITIES_STRING

0x00003B61

ERROR_MCA_INVALID_VCP_VERSION

0x00003B62

ERROR_MCA_MONITOR_VIOLATES_MCCS_SPECIFICATION

0x00003B63

ERROR_MCA_MCCS_VERSION_MISMATCH

0x00003B64

ERROR_MCA_UNSUPPORTED_MCCS_VERSION

0x00003B65

ERROR_MCA_INTERNAL_ERROR

0x00003B66

ERROR_MCA_INVALID_TECHNOLOGY_TYPE_RETURNED

0x00003B67

ERROR_MCA_UNSUPPORTED_COLOR_TEMPERATURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The resource loader cache does
not have a loaded MUI entry.

The user stopped resource
enumeration.

User interface language
installation failed.

Locale installation failed.

The monitor returned a DDC/CI
capabilities string that did not
comply with the ACCESS.bus
3.0, DDC/CI 1.1, or MCCS 2
Revision 1 specification.

The monitor's VCP version
(0xDF) VCP code returned an
invalid version value.

The monitor does not comply
with the MCCS specification it
claims to support.

The MCCS version in a monitor's
mccs_ver capability does not
match the MCCS version the
monitor reports when the VCP
version (0xDF) VCP code is used.

The monitor configuration API
works only with monitors that
support the MCCS 1.0, MCCS
2.0, or MCCS 2.0 Revision 1
specifications.

An internal monitor configuration
API error occurred.

The monitor returned an invalid
monitor technology type. CRT,
plasma, and LCD (TFT) are
examples of monitor technology
types. This error implies that the
monitor violated the MCCS 2.0
or MCCS 2.0 Revision 1
specification.

The
SetMonitorColorTemperature()
caller passed a color
temperature to it that the
current monitor did not support.
CRT, plasma, and LCD (TFT) are
examples of monitor technology
types. This error implies that the
monitor violated the MCCS 2.0
or MCCS 2.0 Revision 1

373 / 497


Win32 error codes

0x00003B92

ERROR_AMBIGUOUS_SYSTEM_DEVICE

0x00003BC3

ERROR_SYSTEM_DEVICE_NOT_FOUND

### 2.3 NTSTATUS

Description

specification.

The requested system device
cannot be identified due to
multiple indistinguishable
devices potentially matching the
identification criteria.

The requested system device
cannot be found.

Values are 32 bit and are laid out as follows. The following diagram is independent of endianness; that
is, the diagram is shown in host byte order and merely shows the layout of the numbering space.

Any protocol that uses NTSTATUS values on the wire is responsible for stating the order that the bytes
are placed on the wire.

0  1  2  3  4  5  6  7  8  9

1
0  1  2  3  4  5  6  7  8  9

2
0  1  2  3  4  5  6  7  8  9

3
0  1

Sev  C  N

Facility

Code

Sev (2 bits): Severity. Severity codes are as follows.

Value

Meaning

STATUS_SEVERITY_SUCCESS

Success

0x0

STATUS_SEVERITY_INFORMATIONAL

Informational

0x1

STATUS_SEVERITY_WARNING

Warning

0x2

STATUS_SEVERITY_ERROR

Error

0x3

C (1 bit): Customer. This specifies if the value is customer- or Microsoft-defined. This bit is set for

customer-defined values and clear for Microsoft-defined values.<4>

N (1 bit): Reserved. MUST be set to 0 so that it is possible to map an NTSTATUS value to an

equivalent HRESULT value, as specified in section 2.1, by setting this bit.

Facility (12 bits): A value that, together with the C bit, indicates the numbering space to use for the

Code field.<5>

Code (2 bytes): The remainder of the error code. Vendors SHOULD reuse the values in the following
table with their indicated meaning or define their own values with the C bit set. Choosing any
other value with the C bit clear runs the risk of a collision in the future.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

374 / 497


#### 2.3.1 NTSTATUS Values

By combining the NTSTATUS into a single 32-bit numbering space, the following NTSTATUS values are
defined. Most values also have a defined default message that can be used to map the value to a
human-readable text message. When this is done, the NTSTATUS value is also known as a message
identifier.

This document provides the common usage details of the NTSTATUS values; individual protocol
specifications provide expanded or modified definitions when needed.

In the following descriptions, a percentage sign that is followed by one or more alphanumeric
characters (for example, "%1" or "%hs") indicates a variable that is replaced by text at the time the
value is returned.

Return value/code

0x00000000

STATUS_SUCCESS

0x00000000

STATUS_WAIT_0

0x00000001

STATUS_WAIT_1

0x00000002

STATUS_WAIT_2

0x00000003

STATUS_WAIT_3

0x0000003F

STATUS_WAIT_63

0x00000080

STATUS_ABANDONED

0x00000080

STATUS_ABANDONED_WAIT_0

0x000000BF

STATUS_ABANDONED_WAIT_63

0x000000C0

STATUS_USER_APC

0x00000101

STATUS_ALERTED

0x00000102

STATUS_TIMEOUT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The operation completed successfully.

The caller specified WaitAny for WaitType
and one of the dispatcher objects in the
Object array has been set to the signaled
state.

The caller specified WaitAny for WaitType
and one of the dispatcher objects in the
Object array has been set to the signaled
state.

The caller specified WaitAny for WaitType
and one of the dispatcher objects in the
Object array has been set to the signaled
state.

The caller specified WaitAny for WaitType
and one of the dispatcher objects in the
Object array has been set to the signaled
state.

The caller specified WaitAny for WaitType
and one of the dispatcher objects in the
Object array has been set to the signaled
state.

The caller attempted to wait for a mutex
that has been abandoned.

The caller attempted to wait for a mutex
that has been abandoned.

The caller attempted to wait for a mutex
that has been abandoned.

A user-mode APC was delivered before
the given Interval expired.

The delay completed because the thread
was alerted.

The given Timeout interval expired.

375 / 497


Return value/code

0x00000103

STATUS_PENDING

0x00000104

STATUS_REPARSE

0x00000105

STATUS_MORE_ENTRIES

0x00000106

STATUS_NOT_ALL_ASSIGNED

0x00000107

STATUS_SOME_NOT_MAPPED

0x00000108

STATUS_OPLOCK_BREAK_IN_PROGRESS

0x00000109

STATUS_VOLUME_MOUNTED

0x0000010A

STATUS_RXACT_COMMITTED

0x0000010B

STATUS_NOTIFY_CLEANUP

0x0000010C

STATUS_NOTIFY_ENUM_DIR

0x0000010D

STATUS_NO_QUOTAS_FOR_ACCOUNT

0x0000010E

STATUS_PRIMARY_TRANSPORT_CONNECT_FAILED

0x00000110

STATUS_PAGE_FAULT_TRANSITION

0x00000111

STATUS_PAGE_FAULT_DEMAND_ZERO

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The operation that was requested is
pending completion.

A reparse should be performed by the
Object Manager because the name of the
file resulted in a symbolic link.

Returned by enumeration APIs to indicate
more information is available to
successive calls.

Indicates not all privileges or groups that
are referenced are assigned to the caller.
This allows, for example, all privileges to
be disabled without having to know
exactly which privileges are assigned.

Some of the information to be translated
has not been translated.

An open/create operation completed
while an opportunistic lock (oplock) break
is underway.

A new volume has been mounted by a
file system.

This success level status indicates that
the transaction state already exists for
the registry subtree but that a
transaction commit was previously
aborted. The commit has now been
completed.

Indicates that a notify change request
has been completed due to closing the
handle that made the notify change
request.

Indicates that a notify change request is
being completed and that the information
is not being returned in the caller's
buffer. The caller now needs to
enumerate the files to find the changes.

{No Quotas} No system quota limits are
specifically set for this account.

{Connect Failure on Primary Transport}
An attempt was made to connect to the
remote server %hs on the primary
transport, but the connection failed. The
computer WAS able to connect on a
secondary transport.

The page fault was a transition fault.

The page fault was a demand zero fault.

376 / 497


Return value/code

0x00000112

STATUS_PAGE_FAULT_COPY_ON_WRITE

0x00000113

STATUS_PAGE_FAULT_GUARD_PAGE

0x00000114

STATUS_PAGE_FAULT_PAGING_FILE

0x00000115

STATUS_CACHE_PAGE_LOCKED

0x00000116

STATUS_CRASH_DUMP

0x00000117

STATUS_BUFFER_ALL_ZEROS

0x00000118

STATUS_REPARSE_OBJECT

0x00000119

STATUS_RESOURCE_REQUIREMENTS_CHANGED

0x00000120

STATUS_TRANSLATION_COMPLETE

0x00000121

STATUS_DS_MEMBERSHIP_EVALUATED_LOCALLY

0x00000122

STATUS_NOTHING_TO_TERMINATE

0x00000123

STATUS_PROCESS_NOT_IN_JOB

0x00000124

STATUS_PROCESS_IN_JOB

0x00000125

STATUS_VOLSNAP_HIBERNATE_READY

0x00000126

STATUS_FSFILTER_OP_COMPLETED_SUCCESSFULLY

0x00000127

STATUS_INTERRUPT_VECTOR_ALREADY_CONNECTED

0x00000128

STATUS_INTERRUPT_STILL_CONNECTED

0x00000129

STATUS_PROCESS_CLONED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The page fault was a demand zero fault.

The page fault was a demand zero fault.

The page fault was satisfied by reading
from a secondary storage device.

The cached page was locked during
operation.

The crash dump exists in a paging file.

The specified buffer contains all zeros.

A reparse should be performed by the
Object Manager because the name of the
file resulted in a symbolic link.

The device has succeeded a query-stop
and its resource requirements have
changed.

The translator has translated these
resources into the global space and no
additional translations should be
performed.

The directory service evaluated group
memberships locally, because it was
unable to contact a global catalog server.

A process being terminated has no
threads to terminate.

The specified process is not part of a job.

The specified process is part of a job.

{Volume Shadow Copy Service} The
system is now ready for hibernation.

A file system or file system filter driver
has successfully completed an FsFilter
operation.

The specified interrupt vector was
already connected.

The specified interrupt vector is still
connected.

The current process is a cloned process.

377 / 497


Return value/code

0x0000012A

STATUS_FILE_LOCKED_WITH_ONLY_READERS

0x0000012B

STATUS_FILE_LOCKED_WITH_WRITERS

0x00000202

STATUS_RESOURCEMANAGER_READ_ONLY

0x00000367

STATUS_WAIT_FOR_OPLOCK

0x00010001

DBG_EXCEPTION_HANDLED

0x00010002

DBG_CONTINUE

0x001C0001

STATUS_FLT_IO_COMPLETE

0xC0000467

STATUS_FILE_NOT_AVAILABLE

0xC0000480

STATUS_SHARE_UNAVAILABLE

0xC0000721

STATUS_CALLBACK_RETURNED_THREAD_AFFINITY

0x40000000

STATUS_OBJECT_NAME_EXISTS

0x40000001

STATUS_THREAD_WAS_SUSPENDED

0x40000002

STATUS_WORKING_SET_LIMIT_RANGE

0x40000003

STATUS_IMAGE_NOT_AT_BASE

0x40000004

STATUS_RXACT_STATE_CREATED

0x40000005

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The file was locked and all users of the
file can only read.

The file was locked and at least one user
of the file can write.

The specified ResourceManager made no
changes or updates to the resource under
this transaction.

An operation is blocked and waiting for
an oplock.

Debugger handled the exception.

The debugger continued.

The IO was completed by a filter.

The file is temporarily unavailable.

The share is temporarily unavailable.

A threadpool worker thread entered a
callback at thread affinity %p and exited
at affinity %p.

This is unexpected, indicating that the
callback missed restoring the priority.

{Object Exists} An attempt was made to
create an object but the object name
already exists.

{Thread Suspended} A thread
termination occurred while the thread
was suspended. The thread resumed, and
termination proceeded.

{Working Set Range Error} An attempt
was made to set the working set
minimum or maximum to values that are
outside the allowable range.

{Image Relocated} An image file could
not be mapped at the address that is
specified in the image file. Local fixes
must be performed on this image.

This informational level status indicates
that a specified registry subtree
transaction state did not yet exist and
had to be created.

{Segment Load} A virtual DOS machine
(VDM) is loading, unloading, or moving

378 / 497


Return value/code

STATUS_SEGMENT_NOTIFICATION

0x40000006

STATUS_LOCAL_USER_SESSION_KEY

0x40000007

STATUS_BAD_CURRENT_DIRECTORY

0x40000008

STATUS_SERIAL_MORE_WRITES

0x40000009

STATUS_REGISTRY_RECOVERED

0x4000000A

STATUS_FT_READ_RECOVERY_FROM_BACKUP

0x4000000B

STATUS_FT_WRITE_RECOVERY

0x4000000C

STATUS_SERIAL_COUNTER_TIMEOUT

0x4000000D

STATUS_NULL_LM_PASSWORD

0x4000000E

STATUS_IMAGE_MACHINE_TYPE_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

an MS-DOS or Win16 program segment
image. An exception is raised so that a
debugger can load, unload, or track
symbols and breakpoints within these 16-
bit segments.

{Local Session Key} A user session key
was requested for a local remote
procedure call (RPC) connection. The
session key that is returned is a constant
value and not unique to this connection.

{Invalid Current Directory} The process
cannot switch to the startup current
directory %hs. Select OK to set the
current directory to %hs, or select
CANCEL to exit.

{Serial IOCTL Complete} A serial I/O
operation was completed by another
write to a serial port. (The
IOCTL_SERIAL_XOFF_COUNTER reached
zero.)

{Registry Recovery} One of the files that
contains the system registry data had to
be recovered by using a log or alternate
copy. The recovery was successful.

{Redundant Read} To satisfy a read
request, the Windows NT fault-tolerant
file system successfully read the
requested data from a redundant copy.
This was done because the file system
encountered a failure on a member of the
fault-tolerant volume but was unable to
reassign the failing area of the device.

{Redundant Write} To satisfy a write
request, the Windows NT fault-tolerant
file system successfully wrote a
redundant copy of the information. This
was done because the file system
encountered a failure on a member of the
fault-tolerant volume but was unable to
reassign the failing area of the device.

{Serial IOCTL Timeout} A serial I/O
operation completed because the time-
out period expired. (The
IOCTL_SERIAL_XOFF_COUNTER had not
reached zero.)

{Password Too Complex} The Windows
password is too complex to be converted
to a LAN Manager password. The LAN
Manager password that returned is a
NULL string.

{Machine Type Mismatch} The image file
%hs is valid but is for a machine type
other than the current machine. Select
OK to continue, or CANCEL to fail the DLL

379 / 497


Return value/code

0x4000000F

STATUS_RECEIVE_PARTIAL

0x40000010

STATUS_RECEIVE_EXPEDITED

0x40000011

STATUS_RECEIVE_PARTIAL_EXPEDITED

0x40000012

STATUS_EVENT_DONE

0x40000013

STATUS_EVENT_PENDING

0x40000014

STATUS_CHECKING_FILE_SYSTEM

0x40000015

STATUS_FATAL_APP_EXIT

0x40000016

STATUS_PREDEFINED_HANDLE

0x40000017

STATUS_WAS_UNLOCKED

0x40000018

STATUS_SERVICE_NOTIFICATION

0x40000019

STATUS_WAS_LOCKED

0x4000001A

STATUS_LOG_HARD_ERROR

0x4000001B

STATUS_ALREADY_WIN32

0x4000001C

STATUS_WX86_UNSIMULATE

0x4000001D

STATUS_WX86_CONTINUE

0x4000001E

STATUS_WX86_SINGLE_STEP

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

load.

{Partial Data Received} The network
transport returned partial data to its
client. The remaining data will be sent
later.

{Expedited Data Received} The network
transport returned data to its client that
was marked as expedited by the remote
system.

{Partial Expedited Data Received} The
network transport returned partial data
to its client and this data was marked as
expedited by the remote system. The
remaining data will be sent later.

{TDI Event Done} The TDI indication has
completed successfully.

{TDI Event Pending} The TDI indication
has entered the pending state.

Checking file system on %wZ.

{Fatal Application Exit} %hs

The specified registry key is referenced
by a predefined handle.

{Page Unlocked} The page protection of
a locked page was changed to 'No Access'
and the page was unlocked from memory
and from the process.

%hs

{Page Locked} One of the pages to lock
was already locked.

Application popup: %1 : %2

A Win32 process already exists.

An exception status code that is used by
the Win32 x86 emulation subsystem.

An exception status code that is used by
the Win32 x86 emulation subsystem.

An exception status code that is used by
the Win32 x86 emulation subsystem.

380 / 497


Return value/code

0x4000001F

STATUS_WX86_BREAKPOINT

0x40000020

STATUS_WX86_EXCEPTION_CONTINUE

0x40000021

STATUS_WX86_EXCEPTION_LASTCHANCE

0x40000022

STATUS_WX86_EXCEPTION_CHAIN

0x40000023

STATUS_IMAGE_MACHINE_TYPE_MISMATCH_EXE

0x40000024

STATUS_NO_YIELD_PERFORMED

0x40000025

STATUS_TIMER_RESUME_IGNORED

0x40000026

STATUS_ARBITRATION_UNHANDLED

0x40000027

STATUS_CARDBUS_NOT_SUPPORTED

0x40000028

STATUS_WX86_CREATEWX86TIB

0x40000029

STATUS_MP_PROCESSOR_MISMATCH

0x4000002A

STATUS_HIBERNATED

0x4000002B

STATUS_RESUME_HIBERNATION

0x4000002C

STATUS_FIRMWARE_UPDATED

0x4000002D

STATUS_DRIVERS_LEAKING_LOCKED_PAGES

0x4000002E

STATUS_MESSAGE_RETRIEVED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An exception status code that is used by
the Win32 x86 emulation subsystem.

An exception status code that is used by
the Win32 x86 emulation subsystem.

An exception status code that is used by
the Win32 x86 emulation subsystem.

An exception status code that is used by
the Win32 x86 emulation subsystem.

{Machine Type Mismatch} The image file
%hs is valid but is for a machine type
other than the current machine.

A yield execution was performed and no
thread was available to run.

The resume flag to a timer API was
ignored.

The arbiter has deferred arbitration of
these resources to its parent.

The device has detected a CardBus card
in its slot.

An exception status code that is used by
the Win32 x86 emulation subsystem.

The CPUs in this multiprocessor system
are not all the same revision level. To use
all processors, the operating system
restricts itself to the features of the least
capable processor in the system. If
problems occur with this system, contact
the CPU manufacturer to see if this mix
of processors is supported.

The system was put into hibernation.

The system was resumed from
hibernation.

Windows has detected that the system
firmware (BIOS) was updated [previous
firmware date = %2, current firmware
date %3].

A device driver is leaking locked I/O
pages and is causing system degradation.
The system has automatically enabled
the tracking code to try and catch the
culprit.

The ALPC message being canceled has
already been retrieved from the queue on

381 / 497


Return value/code

0x4000002F

STATUS_SYSTEM_POWERSTATE_TRANSITION

0x40000030

STATUS_ALPC_CHECK_COMPLETION_LIST

0x40000031

STATUS_SYSTEM_POWERSTATE_COMPLEX_TRANSITION

0x40000032

STATUS_ACCESS_AUDIT_BY_POLICY

0x40000033

STATUS_ABANDON_HIBERFILE

0x40000034

STATUS_BIZRULES_NOT_ENABLED

0x40000294

STATUS_WAKE_SYSTEM

0x40000370

STATUS_DS_SHUTTING_DOWN

0x40010001

DBG_REPLY_LATER

0x40010002

DBG_UNABLE_TO_PROVIDE_HANDLE

0x40010003

DBG_TERMINATE_THREAD

0x40010004

DBG_TERMINATE_PROCESS

0x40010005

DBG_CONTROL_C

0x40010006

DBG_PRINTEXCEPTION_C

0x40010007

DBG_RIPEXCEPTION

0x40010008

DBG_CONTROL_BREAK

0x40010009

DBG_COMMAND_EXCEPTION

0x40020056

RPC_NT_UUID_LOCAL_ONLY

0x400200AF

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the other side.

The system power state is transitioning
from %2 to %3.

The receive operation was successful.
Check the ALPC completion list for the
received message.

The system power state is transitioning
from %2 to %3 but could enter %4.

Access to %1 is monitored by policy rule
%2.

A valid hibernation file has been
invalidated and should be abandoned.

Business rule scripts are disabled for the
calling application.

The system has awoken.

The directory service is shutting down.

Debugger will reply later.

Debugger cannot provide a handle.

Debugger terminated the thread.

Debugger terminated the process.

Debugger obtained control of C.

Debugger printed an exception on control
C.

Debugger received a RIP exception.

Debugger received a control break.

Debugger command communication
exception.

A UUID that is valid only on this
computer has been allocated.

Some data remains to be sent in the

382 / 497


Return value/code

RPC_NT_SEND_INCOMPLETE

0x400A0004

STATUS_CTX_CDM_CONNECT

0x400A0005

STATUS_CTX_CDM_DISCONNECT

0x4015000D

STATUS_SXS_RELEASE_ACTIVATION_CONTEXT

0x40190034

STATUS_RECOVERY_NOT_NEEDED

0x40190035

STATUS_RM_ALREADY_STARTED

0x401A000C

STATUS_LOG_NO_RESTART

0x401B00EC

STATUS_VIDEO_DRIVER_DEBUG_REPORT_REQUEST

0x401E000A

STATUS_GRAPHICS_PARTIAL_DATA_POPULATED

0x401E0117

STATUS_GRAPHICS_DRIVER_MISMATCH

0x401E0307

STATUS_GRAPHICS_MODE_NOT_PINNED

0x401E031E

STATUS_GRAPHICS_NO_PREFERRED_MODE

0x401E034B

STATUS_GRAPHICS_DATASET_IS_EMPTY

0x401E034C

STATUS_GRAPHICS_NO_MORE_ELEMENTS_IN_DATASET

0x401E0351

STATUS_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATIO

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

request buffer.

The Client Drive Mapping Service has
connected on Terminal Connection.

The Client Drive Mapping Service has
disconnected on Terminal Connection.

A kernel mode component is releasing a
reference on an activation context.

The transactional resource manager is
already consistent. Recovery is not
needed.

The transactional resource manager has
already been started.

The log service encountered a log stream
with no restart area.

{Display Driver Recovered From Failure}
The %hs display driver has detected a
failure and recovered from it. Some
graphical operations might have failed.
The next time you restart the machine, a
dialog box appears, giving you an
opportunity to upload data about this
failure to Microsoft.

The specified buffer is not big enough to
contain the entire requested dataset.
Partial data is populated up to the size of
the buffer.

The caller needs to provide a buffer of
the size as specified in the partially
populated buffer's content (interface
specific).

The kernel driver detected a version
mismatch between it and the user mode
driver.

No mode is pinned on the specified VidPN
source/target.

The specified mode set does not specify a
preference for one of its modes.

The specified dataset (for example, mode
set, frequency range set, descriptor set,
or topology) is empty.

The specified dataset (for example, mode
set, frequency range set, descriptor set,
or topology) does not contain any more
elements.

The specified content transformation is
not pinned on the specified VidPN present

383 / 497


Return value/code

N_NOT_PINNED

0x401E042F

STATUS_GRAPHICS_UNKNOWN_CHILD_STATUS

0x401E0437

STATUS_GRAPHICS_LEADLINK_START_DEFERRED

0x401E0439

STATUS_GRAPHICS_POLLING_TOO_FREQUENTLY

0x401E043A

STATUS_GRAPHICS_START_DEFERRED

0x40230001

STATUS_NDIS_INDICATION_REQUIRED

0x80000001

STATUS_GUARD_PAGE_VIOLATION

0x80000002

STATUS_DATATYPE_MISALIGNMENT

0x80000003

STATUS_BREAKPOINT

0x80000004

STATUS_SINGLE_STEP

0x80000005

STATUS_BUFFER_OVERFLOW

0x80000006

STATUS_NO_MORE_FILES

0x80000007

STATUS_WAKE_SYSTEM_DEBUGGER

0x8000000A

STATUS_HANDLES_CLOSED

0x8000000B

STATUS_NO_INHERITANCE

0x8000000C

STATUS_GUID_SUBSTITUTION_MADE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

path.

The child device presence was not
reliably detected.

Starting the lead adapter in a linked
configuration has been temporarily
deferred.

The display adapter is being polled for
children too frequently at the same
polling level.

Starting the adapter has been
temporarily deferred.

The request will be completed later by an
NDIS status indication.

{EXCEPTION} Guard Page Exception A
page of memory that marks the end of a
data structure, such as a stack or an
array, has been accessed.

{EXCEPTION} Alignment Fault A data
type misalignment was detected in a load
or store instruction.

{EXCEPTION} Breakpoint A breakpoint
has been reached.

{EXCEPTION} Single Step A single step
or trace operation has just been
completed.

{Buffer Overflow} The data was too large
to fit into the specified buffer.

{No More Files} No more files were found
which match the file specification.

{Kernel Debugger Awakened} The
system debugger was awakened by an
interrupt.

{Handles Closed} Handles to objects
have been automatically closed because
of the requested operation.

{Non-Inheritable ACL} An access control
list (ACL) contains no components that
can be inherited.

{GUID Substitution} During the
translation of a globally unique identifier
(GUID) to a Windows security ID (SID),
no administratively defined GUID prefix
was found. A substitute prefix was used,
which will not compromise system
security. However, this might provide a
more restrictive access than intended.

384 / 497


Return value/code

0x8000000D

STATUS_PARTIAL_COPY

0x8000000E

STATUS_DEVICE_PAPER_EMPTY

0x8000000F

STATUS_DEVICE_POWERED_OFF

0x80000010

STATUS_DEVICE_OFF_LINE

0x80000011

STATUS_DEVICE_BUSY

0x80000012

STATUS_NO_MORE_EAS

0x80000013

STATUS_INVALID_EA_NAME

0x80000014

STATUS_EA_LIST_INCONSISTENT

0x80000015

STATUS_INVALID_EA_FLAG

0x80000016

STATUS_VERIFY_REQUIRED

0x80000017

STATUS_EXTRANEOUS_INFORMATION

0x80000018

STATUS_RXACT_COMMIT_NECESSARY

0x8000001A

STATUS_NO_MORE_ENTRIES

0x8000001B

STATUS_FILEMARK_DETECTED

0x8000001C

STATUS_MEDIA_CHANGED

0x8000001D

STATUS_BUS_RESET

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Because of protection conflicts, not all
the requested bytes could be copied.

{Out of Paper} The printer is out of
paper.

{Device Power Is Off} The printer power
has been turned off.

{Device Offline} The printer has been
taken offline.

{Device Busy} The device is currently
busy.

{No More EAs} No more extended
attributes (EAs) were found for the file.

{Illegal EA} The specified extended
attribute (EA) name contains at least one
illegal character.

{Inconsistent EA List} The extended
attribute (EA) list is inconsistent.

{Invalid EA Flag} An invalid extended
attribute (EA) flag was set.

{Verifying Disk} The media has changed
and a verify operation is in progress;
therefore, no reads or writes can be
performed to the device, except those
that are used in the verify operation.

{Too Much Information} The specified
access control list (ACL) contained more
information than was expected.

This warning level status indicates that
the transaction state already exists for
the registry subtree, but that a
transaction commit was previously
aborted. The commit has NOT been
completed but has not been rolled back
either; therefore, it can still be
committed, if needed.

{No More Entries} No more entries are
available from an enumeration operation.

{Filemark Found} A filemark was
detected.

{Media Changed} The media has
changed.

{I/O Bus Reset} An I/O bus reset was
detected.

385 / 497


Return value/code

0x8000001E

STATUS_END_OF_MEDIA

0x8000001F

STATUS_BEGINNING_OF_MEDIA

0x80000020

STATUS_MEDIA_CHECK

0x80000021

STATUS_SETMARK_DETECTED

0x80000022

STATUS_NO_DATA_DETECTED

0x80000023

STATUS_REDIRECTOR_HAS_OPEN_HANDLES

0x80000024

STATUS_SERVER_HAS_OPEN_HANDLES

0x80000025

STATUS_ALREADY_DISCONNECTED

0x80000026

STATUS_LONGJUMP

0x80000027

STATUS_CLEANER_CARTRIDGE_INSTALLED

0x80000028

STATUS_PLUGPLAY_QUERY_VETOED

0x80000029

STATUS_UNWIND_CONSOLIDATE

0x8000002A

STATUS_REGISTRY_HIVE_RECOVERED

0x8000002B

STATUS_DLL_MIGHT_BE_INSECURE

0x8000002C

STATUS_DLL_MIGHT_BE_INCOMPATIBLE

0x8000002D

STATUS_STOPPED_ON_SYMLINK

Description

{End of Media} The end of the media was
encountered.

The beginning of a tape or partition has
been detected.

{Media Changed} The media might have
changed.

A tape access reached a set mark.

During a tape access, the end of the data
written is reached.

The redirector is in use and cannot be
unloaded.

The server is in use and cannot be
unloaded.

The specified connection has already
been disconnected.

A long jump has been executed.

A cleaner cartridge is present in the tape
library.

The Plug and Play query operation was
not successful.

A frame consolidation has been executed.

{Registry Hive Recovered} The registry
hive (file): %hs was corrupted and it has
been recovered. Some data might have
been lost.

The application is attempting to run
executable code from the module %hs.
This might be insecure. An alternative,
%hs, is available. Should the application
use the secure module %hs?

The application is loading executable
code from the module %hs. This is
secure but might be incompatible with
previous releases of the operating
system. An alternative, %hs, is available.
Should the application use the secure
module %hs?

The create operation stopped after
reaching a symbolic link.

0x80000288

The device has indicated that cleaning is

386 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_DEVICE_REQUIRES_CLEANING

Description

necessary.

0x80000289

STATUS_DEVICE_DOOR_OPEN

0x80000803

STATUS_DATA_LOST_REPAIR

0x80010001

DBG_EXCEPTION_NOT_HANDLED

0x80130001

STATUS_CLUSTER_NODE_ALREADY_UP

0x80130002

STATUS_CLUSTER_NODE_ALREADY_DOWN

The device has indicated that its door is
open. Further operations require it closed
and secured.

Windows discovered a corruption in the
file %hs. This file has now been repaired.
Check if any data in the file was lost
because of the corruption.

Debugger did not handle the exception.

The cluster node is already up.

The cluster node is already down.

0x80130003

The cluster network is already online.

STATUS_CLUSTER_NETWORK_ALREADY_ONLINE

0x80130004

The cluster network is already offline.

STATUS_CLUSTER_NETWORK_ALREADY_OFFLINE

0x80130005

STATUS_CLUSTER_NODE_ALREADY_MEMBER

0x80190009

STATUS_COULD_NOT_RESIZE_LOG

0x80190029

STATUS_NO_TXF_METADATA

0x80190031

STATUS_CANT_RECOVER_WITH_HANDLE_OPEN

0x80190041

STATUS_TXF_METADATA_ALREADY_PRESENT

0x80190042

STATUS_TRANSACTION_SCOPE_CALLBACKS_NOT_SET

0x801B00EB

STATUS_VIDEO_HUNG_DISPLAY_DRIVER_THREAD_RECOVERED

0x801C0001

STATUS_FLT_BUFFER_TOO_SMALL

0x80210001

STATUS_FVE_PARTIAL_METADATA

0x80210002

STATUS_FVE_TRANSIENT_STATE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The cluster node is already a member of
the cluster.

The log could not be set to the requested
size.

There is no transaction metadata on the
file.

The file cannot be recovered because
there is a handle still open on it.

Transaction metadata is already present
on this file and cannot be superseded.

A transaction scope could not be entered
because the scope handler has not been
initialized.

{Display Driver Stopped Responding and
recovered} The %hs display driver has
stopped working normally. The recovery
had been performed.

{Buffer too small} The buffer is too small
to contain the entry. No information has
been written to the buffer.

Volume metadata read or write is
incomplete.

BitLocker encryption keys were ignored
because the volume was in a transient
state.

387 / 497


Return value/code

0xC0000001

STATUS_UNSUCCESSFUL

0xC0000002

STATUS_NOT_IMPLEMENTED

0xC0000003

STATUS_INVALID_INFO_CLASS

0xC0000004

STATUS_INFO_LENGTH_MISMATCH

0xC0000005

STATUS_ACCESS_VIOLATION

0xC0000006

STATUS_IN_PAGE_ERROR

0xC0000007

STATUS_PAGEFILE_QUOTA

0xC0000008

STATUS_INVALID_HANDLE

0xC0000009

STATUS_BAD_INITIAL_STACK

0xC000000A

STATUS_BAD_INITIAL_PC

0xC000000B

STATUS_INVALID_CID

0xC000000C

STATUS_TIMER_NOT_CANCELED

0xC000000D

STATUS_INVALID_PARAMETER

0xC000000E

STATUS_NO_SUCH_DEVICE

0xC000000F

STATUS_NO_SUCH_FILE

0xC0000010

STATUS_INVALID_DEVICE_REQUEST

0xC0000011

STATUS_END_OF_FILE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Operation Failed} The requested
operation was unsuccessful.

{Not Implemented} The requested
operation is not implemented.

{Invalid Parameter} The specified
information class is not a valid
information class for the specified object.

The specified information record length
does not match the length that is
required for the specified information
class.

The instruction at 0x%08lx referenced
memory at 0x%08lx. The memory could
not be %s.

The instruction at 0x%08lx referenced
memory at 0x%08lx. The required data
was not placed into memory because of
an I/O error status of 0x%08lx.

The page file quota for the process has
been exhausted.

An invalid HANDLE was specified.

An invalid initial stack was specified in a
call to NtCreateThread.

An invalid initial start address was
specified in a call to NtCreateThread.

An invalid client ID was specified.

An attempt was made to cancel or set a
timer that has an associated APC and the
specified thread is not the thread that
originally set the timer with an associated
APC routine.

An invalid parameter was passed to a
service or function.

A device that does not exist was
specified.

{File Not Found} The file %hs does not
exist.

The specified request is not a valid
operation for the target device.

The end-of-file marker has been reached.
There is no valid data in the file beyond

388 / 497


Return value/code

0xC0000012

STATUS_WRONG_VOLUME

0xC0000013

STATUS_NO_MEDIA_IN_DEVICE

0xC0000014

STATUS_UNRECOGNIZED_MEDIA

0xC0000015

STATUS_NONEXISTENT_SECTOR

0xC0000016

STATUS_MORE_PROCESSING_REQUIRED

0xC0000017

STATUS_NO_MEMORY

0xC0000018

STATUS_CONFLICTING_ADDRESSES

0xC0000019

STATUS_NOT_MAPPED_VIEW

0xC000001A

STATUS_UNABLE_TO_FREE_VM

0xC000001B

STATUS_UNABLE_TO_DELETE_SECTION

0xC000001C

STATUS_INVALID_SYSTEM_SERVICE

0xC000001D

STATUS_ILLEGAL_INSTRUCTION

0xC000001E

STATUS_INVALID_LOCK_SEQUENCE

0xC000001F

STATUS_INVALID_VIEW_SIZE

0xC0000020

STATUS_INVALID_FILE_FOR_SECTION

0xC0000021

STATUS_ALREADY_COMMITTED

Description

this marker.

{Wrong Volume} The wrong volume is in
the drive. Insert volume %hs into drive
%hs.

{No Disk} There is no disk in the drive.
Insert a disk into drive %hs.

{Unknown Disk Format} The disk in drive
%hs is not formatted properly. Check the
disk, and reformat it, if needed.

{Sector Not Found} The specified sector
does not exist.

{Still Busy} The specified I/O request
packet (IRP) cannot be disposed of
because the I/O operation is not
complete.

{Not Enough Quota} Not enough virtual
memory or paging file quota is available
to complete the specified operation.

{Conflicting Address Range} The
specified address range conflicts with the
address space.

The address range to unmap is not a
mapped view.

The virtual memory cannot be freed.

The specified section cannot be deleted.

An invalid system service was specified in
a system service call.

{EXCEPTION} Illegal Instruction An
attempt was made to execute an illegal
instruction.

{Invalid Lock Sequence} An attempt was
made to execute an invalid lock
sequence.

{Invalid Mapping} An attempt was made
to create a view for a section that is
bigger than the section.

{Bad File} The attributes of the specified
mapping file for a section of memory
cannot be read.

{Already Committed} The specified
address range is already committed.

0xC0000022

{Access Denied} A process has requested

389 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_ACCESS_DENIED

0xC0000023

STATUS_BUFFER_TOO_SMALL

0xC0000024

STATUS_OBJECT_TYPE_MISMATCH

0xC0000025

STATUS_NONCONTINUABLE_EXCEPTION

0xC0000026

STATUS_INVALID_DISPOSITION

0xC0000027

STATUS_UNWIND

0xC0000028

STATUS_BAD_STACK

0xC0000029

STATUS_INVALID_UNWIND_TARGET

0xC000002A

STATUS_NOT_LOCKED

0xC000002B

STATUS_PARITY_ERROR

0xC000002C

STATUS_UNABLE_TO_DECOMMIT_VM

0xC000002D

STATUS_NOT_COMMITTED

0xC000002E

STATUS_INVALID_PORT_ATTRIBUTES

0xC000002F

STATUS_PORT_MESSAGE_TOO_LONG

0xC0000030

STATUS_INVALID_PARAMETER_MIX

0xC0000031

STATUS_INVALID_QUOTA_LOWER

0xC0000032

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

access to an object but has not been
granted those access rights.

{Buffer Too Small} The buffer is too
small to contain the entry. No
information has been written to the
buffer.

{Wrong Type} There is a mismatch
between the type of object that is
required by the requested operation and
the type of object that is specified in the
request.

{EXCEPTION} Cannot Continue Windows
cannot continue from this exception.

An invalid exception disposition was
returned by an exception handler.

Unwind exception code.

An invalid or unaligned stack was
encountered during an unwind operation.

An invalid unwind target was
encountered during an unwind operation.

An attempt was made to unlock a page of
memory that was not locked.

A device parity error on an I/O operation.

An attempt was made to decommit
uncommitted virtual memory.

An attempt was made to change the
attributes on memory that has not been
committed.

Invalid object attributes specified to
NtCreatePort or invalid port attributes
specified to NtConnectPort.

The length of the message that was
passed to NtRequestPort or
NtRequestWaitReplyPort is longer than
the maximum message that is allowed by
the port.

An invalid combination of parameters was
specified.

An attempt was made to lower a quota
limit below the current usage.

{Corrupt Disk} The file system structure
on the disk is corrupt and unusable. Run

390 / 497


Return value/code

Description

STATUS_DISK_CORRUPT_ERROR

the Chkdsk utility on the volume %hs.

0xC0000033

STATUS_OBJECT_NAME_INVALID

0xC0000034

STATUS_OBJECT_NAME_NOT_FOUND

0xC0000035

STATUS_OBJECT_NAME_COLLISION

0xC0000037

STATUS_PORT_DISCONNECTED

0xC0000038

STATUS_DEVICE_ALREADY_ATTACHED

0xC0000039

STATUS_OBJECT_PATH_INVALID

0xC000003A

STATUS_OBJECT_PATH_NOT_FOUND

0xC000003B

STATUS_OBJECT_PATH_SYNTAX_BAD

0xC000003C

STATUS_DATA_OVERRUN

0xC000003D

STATUS_DATA_LATE_ERROR

0xC000003E

STATUS_DATA_ERROR

0xC000003F

STATUS_CRC_ERROR

0xC0000040

STATUS_SECTION_TOO_BIG

0xC0000041

STATUS_PORT_CONNECTION_REFUSED

0xC0000042

STATUS_INVALID_PORT_HANDLE

0xC0000043

STATUS_SHARING_VIOLATION

0xC0000044

STATUS_QUOTA_EXCEEDED

0xC0000045

STATUS_INVALID_PAGE_PROTECTION

The object name is invalid.

The object name is not found.

The object name already exists.

An attempt was made to send a message
to a disconnected communication port.

An attempt was made to attach to a
device that was already attached to
another device.

The object path component was not a
directory object.

{Path Not Found} The path %hs does not
exist.

The object path component was not a
directory object.

{Data Overrun} A data overrun error
occurred.

{Data Late} A data late error occurred.

{Data Error} An error occurred in reading
or writing data.

{Bad CRC} A cyclic redundancy check
(CRC) checksum error occurred.

{Section Too Large} The specified section
is too big to map the file.

The NtConnectPort request is refused.

The type of port handle is invalid for the
operation that is requested.

A file cannot be opened because the
share access flags are incompatible.

Insufficient quota exists to complete the
operation.

The specified page protection was not
valid.

0xC0000046

An attempt to release a mutant object

391 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_MUTANT_NOT_OWNED

0xC0000047

STATUS_SEMAPHORE_LIMIT_EXCEEDED

0xC0000048

STATUS_PORT_ALREADY_SET

0xC0000049

STATUS_SECTION_NOT_IMAGE

0xC000004A

STATUS_SUSPEND_COUNT_EXCEEDED

0xC000004B

STATUS_THREAD_IS_TERMINATING

0xC000004C

STATUS_BAD_WORKING_SET_LIMIT

0xC000004D

STATUS_INCOMPATIBLE_FILE_MAP

0xC000004E

STATUS_SECTION_PROTECTION

0xC000004F

STATUS_EAS_NOT_SUPPORTED

0xC0000050

STATUS_EA_TOO_LARGE

0xC0000051

STATUS_NONEXISTENT_EA_ENTRY

0xC0000052

STATUS_NO_EAS_ON_FILE

0xC0000053

STATUS_EA_CORRUPT_ERROR

0xC0000054

STATUS_FILE_LOCK_CONFLICT

0xC0000055

STATUS_LOCK_NOT_GRANTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

was made by a thread that was not the
owner of the mutant object.

An attempt was made to release a
semaphore such that its maximum count
would have been exceeded.

An attempt was made to set the
DebugPort or ExceptionPort of a process,
but a port already exists in the process,
or an attempt was made to set the
CompletionPort of a file but a port was
already set in the file, or an attempt was
made to set the associated completion
port of an ALPC port but it is already set.

An attempt was made to query image
information on a section that does not
map an image.

An attempt was made to suspend a
thread whose suspend count was at its
maximum.

An attempt was made to suspend a
thread that has begun termination.

An attempt was made to set the working
set limit to an invalid value (for example,
the minimum greater than maximum).

A section was created to map a file that
is not compatible with an already existing
section that maps the same file.

A view to a section specifies a protection
that is incompatible with the protection of
the initial view.

An operation involving EAs failed because
the file system does not support EAs.

An EA operation failed because the EA set
is too large.

An EA operation failed because the name
or EA index is invalid.

The file for which EAs were requested has
no EAs.

The EA is corrupt and cannot be read.

A requested read/write cannot be granted
due to a conflicting file lock.

A requested file lock cannot be granted
due to other existing locks.

392 / 497


Return value/code

0xC0000056

STATUS_DELETE_PENDING

0xC0000057

STATUS_CTL_FILE_NOT_SUPPORTED

0xC0000058

STATUS_UNKNOWN_REVISION

0xC0000059

STATUS_REVISION_MISMATCH

0xC000005A

STATUS_INVALID_OWNER

0xC000005B

STATUS_INVALID_PRIMARY_GROUP

0xC000005C

STATUS_NO_IMPERSONATION_TOKEN

0xC000005D

STATUS_CANT_DISABLE_MANDATORY

0xC000005E

STATUS_NO_LOGON_SERVERS

0xC000005F

STATUS_NO_SUCH_LOGON_SESSION

0xC0000060

STATUS_NO_SUCH_PRIVILEGE

0xC0000061

STATUS_PRIVILEGE_NOT_HELD

0xC0000062

STATUS_INVALID_ACCOUNT_NAME

0xC0000063

STATUS_USER_EXISTS

0xC0000064

STATUS_NO_SUCH_USER

0xC0000065

STATUS_GROUP_EXISTS

0xC0000066

STATUS_NO_SUCH_GROUP

0xC0000067

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

A non-close operation has been
requested of a file object that has a
delete pending.

An attempt was made to set the control
attribute on a file. This attribute is not
supported in the destination file system.

Indicates a revision number that was
encountered or specified is not one that
is known by the service. It might be a
more recent revision than the service is
aware of.

Indicates that two revision levels are
incompatible.

Indicates a particular security ID cannot
be assigned as the owner of an object.

Indicates a particular security ID cannot
be assigned as the primary group of an
object.

An attempt has been made to operate on
an impersonation token by a thread that
is not currently impersonating a client.

A mandatory group cannot be disabled.

No logon servers are currently available
to service the logon request.

A specified logon session does not exist.
It might already have been terminated.

A specified privilege does not exist.

A required privilege is not held by the
client.

The name provided is not a properly
formed account name.

The specified account already exists.

The specified account does not exist.

The specified group already exists.

The specified group does not exist.

The specified user account is already in

393 / 497


Return value/code

STATUS_MEMBER_IN_GROUP

0xC0000068

STATUS_MEMBER_NOT_IN_GROUP

0xC0000069

STATUS_LAST_ADMIN

0xC000006A

STATUS_WRONG_PASSWORD

0xC000006B

STATUS_ILL_FORMED_PASSWORD

0xC000006C

STATUS_PASSWORD_RESTRICTION

0xC000006D

STATUS_LOGON_FAILURE

0xC000006E

STATUS_ACCOUNT_RESTRICTION

0xC000006F

STATUS_INVALID_LOGON_HOURS

0xC0000070

STATUS_INVALID_WORKSTATION

0xC0000071

STATUS_PASSWORD_EXPIRED

0xC0000072

STATUS_ACCOUNT_DISABLED

0xC0000073

STATUS_NONE_MAPPED

0xC0000074

STATUS_TOO_MANY_LUIDS_REQUESTED

0xC0000075

STATUS_LUIDS_EXHAUSTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

the specified group account. Also used to
indicate a group cannot be deleted
because it contains a member.

The specified user account is not a
member of the specified group account.

Indicates the requested operation would
disable or delete the last remaining
administration account. This is not
allowed to prevent creating a situation in
which the system cannot be
administrated.

When trying to update a password, this
return status indicates that the value
provided as the current password is not
correct.

When trying to update a password, this
return status indicates that the value
provided for the new password contains
values that are not allowed in passwords.

When trying to update a password, this
status indicates that some password
update rule has been violated. For
example, the password might not meet
length criteria.

The attempted logon is invalid. This is
either due to a bad username or
authentication information.

Indicates a referenced user name and
authentication information are valid, but
some user account restriction has
prevented successful authentication
(such as time-of-day restrictions).

The user account has time restrictions
and cannot be logged onto at this time.

The user account is restricted so that it
cannot be used to log on from the source
workstation.

The user account password has expired.

The referenced account is currently
disabled and cannot be logged on to.

None of the information to be translated
has been translated.

The number of LUIDs requested cannot
be allocated with a single allocation.

Indicates there are no more LUIDs to
allocate.

394 / 497


Return value/code

0xC0000076

STATUS_INVALID_SUB_AUTHORITY

0xC0000077

STATUS_INVALID_ACL

0xC0000078

STATUS_INVALID_SID

0xC0000079

STATUS_INVALID_SECURITY_DESCR

0xC000007A

STATUS_PROCEDURE_NOT_FOUND

0xC000007B

STATUS_INVALID_IMAGE_FORMAT

0xC000007C

STATUS_NO_TOKEN

0xC000007D

STATUS_BAD_INHERITANCE_ACL

0xC000007E

STATUS_RANGE_NOT_LOCKED

0xC000007F

STATUS_DISK_FULL

0xC0000080

STATUS_SERVER_DISABLED

0xC0000081

STATUS_SERVER_NOT_DISABLED

0xC0000082

STATUS_TOO_MANY_GUIDS_REQUESTED

0xC0000083

STATUS_GUIDS_EXHAUSTED

0xC0000084

STATUS_INVALID_ID_AUTHORITY

Description

Indicates the sub-authority value is
invalid for the particular use.

Indicates the ACL structure is not valid.

Indicates the SID structure is not valid.

Indicates the SECURITY_DESCRIPTOR
structure is not valid.

Indicates the specified procedure address
cannot be found in the DLL.

{Bad Image} %hs is either not designed
to run on Windows or it contains an
error. Try installing the program again
using the original installation media or
contact your system administrator or the
software vendor for support.

An attempt was made to reference a
token that does not exist. This is typically
done by referencing the token that is
associated with a thread when the thread
is not impersonating a client.

Indicates that an attempt to build either
an inherited ACL or ACE was not
successful. This can be caused by a
number of things. One of the more
probable causes is the replacement of a
CreatorId with a SID that did not fit into
the ACE or ACL.

The range specified in NtUnlockFile was
not locked.

An operation failed because the disk was
full.

The GUID allocation server is disabled at
the moment.

The GUID allocation server is enabled at
the moment.

Too many GUIDs were requested from
the allocation server at once.

The GUIDs could not be allocated
because the Authority Agent was
exhausted.

The value provided was an invalid value
for an identifier authority.

0xC0000085

No more authority agent values are

395 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_AGENTS_EXHAUSTED

0xC0000086

STATUS_INVALID_VOLUME_LABEL

0xC0000087

STATUS_SECTION_NOT_EXTENDED

0xC0000088

STATUS_NOT_MAPPED_DATA

0xC0000089

STATUS_RESOURCE_DATA_NOT_FOUND

0xC000008A

STATUS_RESOURCE_TYPE_NOT_FOUND

0xC000008B

STATUS_RESOURCE_NAME_NOT_FOUND

0xC000008C

STATUS_ARRAY_BOUNDS_EXCEEDED

0xC000008D

STATUS_FLOAT_DENORMAL_OPERAND

0xC000008E

STATUS_FLOAT_DIVIDE_BY_ZERO

0xC000008F

STATUS_FLOAT_INEXACT_RESULT

0xC0000090

STATUS_FLOAT_INVALID_OPERATION

0xC0000091

STATUS_FLOAT_OVERFLOW

0xC0000092

STATUS_FLOAT_STACK_CHECK

0xC0000093

STATUS_FLOAT_UNDERFLOW

0xC0000094

STATUS_INTEGER_DIVIDE_BY_ZERO

0xC0000095

STATUS_INTEGER_OVERFLOW

0xC0000096

STATUS_PRIVILEGED_INSTRUCTION

0xC0000097

STATUS_TOO_MANY_PAGING_FILES

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

available for the particular identifier
authority value.

An invalid volume label has been
specified.

A mapped section could not be extended.

Specified section to flush does not map a
data file.

Indicates the specified image file did not
contain a resource section.

Indicates the specified resource type
cannot be found in the image file.

Indicates the specified resource name
cannot be found in the image file.

{EXCEPTION} Array bounds exceeded.

{EXCEPTION} Floating-point denormal
operand.

{EXCEPTION} Floating-point division by
zero.

{EXCEPTION} Floating-point inexact
result.

{EXCEPTION} Floating-point invalid
operation.

{EXCEPTION} Floating-point overflow.

{EXCEPTION} Floating-point stack check.

{EXCEPTION} Floating-point underflow.

{EXCEPTION} Integer division by zero.

{EXCEPTION} Integer overflow.

{EXCEPTION} Privileged instruction.

An attempt was made to install more
paging files than the system supports.

396 / 497


Return value/code

0xC0000098

STATUS_FILE_INVALID

0xC0000099

STATUS_ALLOTTED_SPACE_EXCEEDED

0xC000009A

STATUS_INSUFFICIENT_RESOURCES

0xC000009B

STATUS_DFS_EXIT_PATH_FOUND

0xC000009C

STATUS_DEVICE_DATA_ERROR

0xC000009D

STATUS_DEVICE_NOT_CONNECTED

0xC000009F

STATUS_FREE_VM_NOT_AT_BASE

0xC00000A0

STATUS_MEMORY_NOT_ALLOCATED

0xC00000A1

STATUS_WORKING_SET_QUOTA

0xC00000A2

STATUS_MEDIA_WRITE_PROTECTED

0xC00000A3

STATUS_DEVICE_NOT_READY

0xC00000A4

STATUS_INVALID_GROUP_ATTRIBUTES

0xC00000A5

STATUS_BAD_IMPERSONATION_LEVEL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The volume for a file has been externally
altered such that the opened file is no
longer valid.

When a block of memory is allotted for
future updates, such as the memory
allocated to hold discretionary access
control and primary group information,
successive updates might exceed the
amount of memory originally allotted.
Because a quota might already have
been charged to several processes that
have handles to the object, it is not
reasonable to alter the size of the
allocated memory. Instead, a request
that requires more memory than has
been allotted must fail and the
STATUS_ALLOTTED_SPACE_EXCEEDED
error returned.

Insufficient system resources exist to
complete the API.

An attempt has been made to open a
DFS exit path control file.

There are bad blocks (sectors) on the
hard disk.

There is bad cabling, non-termination, or
the controller is not able to obtain access
to the hard disk.

Virtual memory cannot be freed because
the base address is not the base of the
region and a region size of zero was
specified.

An attempt was made to free virtual
memory that is not allocated.

The working set is not big enough to
allow the requested pages to be locked.

{Write Protect Error} The disk cannot be
written to because it is write-protected.
Remove the write protection from the
volume %hs in drive %hs.

{Drive Not Ready} The drive is not ready
for use; its door might be open. Check
drive %hs and make sure that a disk is
inserted and that the drive door is closed.

The specified attributes are invalid or are
incompatible with the attributes for the
group as a whole.

A specified impersonation level is invalid.
Also used to indicate that a required
impersonation level was not provided.

397 / 497


Return value/code

0xC00000A6

STATUS_CANT_OPEN_ANONYMOUS

0xC00000A7

STATUS_BAD_VALIDATION_CLASS

0xC00000A8

STATUS_BAD_TOKEN_TYPE

0xC00000A9

STATUS_BAD_MASTER_BOOT_RECORD

0xC00000AA

STATUS_INSTRUCTION_MISALIGNMENT

0xC00000AB

STATUS_INSTANCE_NOT_AVAILABLE

0xC00000AC

STATUS_PIPE_NOT_AVAILABLE

0xC00000AD

STATUS_INVALID_PIPE_STATE

0xC00000AE

STATUS_PIPE_BUSY

0xC00000AF

STATUS_ILLEGAL_FUNCTION

0xC00000B0

STATUS_PIPE_DISCONNECTED

0xC00000B1

STATUS_PIPE_CLOSING

0xC00000B2

STATUS_PIPE_CONNECTED

0xC00000B3

STATUS_PIPE_LISTENING

0xC00000B4

STATUS_INVALID_READ_MODE

0xC00000B5

STATUS_IO_TIMEOUT

0xC00000B6

STATUS_FILE_FORCED_CLOSED

Description

An attempt was made to open an
anonymous-level token. Anonymous
tokens cannot be opened.

The validation information class
requested was invalid.

The type of a token object is
inappropriate for its attempted use.

The type of a token object is
inappropriate for its attempted use.

An attempt was made to execute an
instruction at an unaligned address and
the host system does not support
unaligned instruction references.

The maximum named pipe instance count
has been reached.

An instance of a named pipe cannot be
found in the listening state.

The named pipe is not in the connected
or closing state.

The specified pipe is set to complete
operations and there are current I/O
operations queued so that it cannot be
changed to queue operations.

The specified handle is not open to the
server end of the named pipe.

The specified named pipe is in the
disconnected state.

The specified named pipe is in the closing
state.

The specified named pipe is in the
connected state.

The specified named pipe is in the
listening state.

The specified named pipe is not in
message mode.

{Device Timeout} The specified I/O
operation on %hs was not completed
before the time-out period expired.

The specified file has been closed by
another process.

0xC00000B7

Profiling is not started.

398 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_PROFILING_NOT_STARTED

0xC00000B8

STATUS_PROFILING_NOT_STOPPED

0xC00000B9

STATUS_COULD_NOT_INTERPRET

0xC00000BA

STATUS_FILE_IS_A_DIRECTORY

0xC00000BB

STATUS_NOT_SUPPORTED

0xC00000BC

STATUS_REMOTE_NOT_LISTENING

0xC00000BD

STATUS_DUPLICATE_NAME

0xC00000BE

STATUS_BAD_NETWORK_PATH

0xC00000BF

STATUS_NETWORK_BUSY

0xC00000C0

STATUS_DEVICE_DOES_NOT_EXIST

0xC00000C1

STATUS_TOO_MANY_COMMANDS

0xC00000C2

STATUS_ADAPTER_HARDWARE_ERROR

0xC00000C3

STATUS_INVALID_NETWORK_RESPONSE

0xC00000C4

STATUS_UNEXPECTED_NETWORK_ERROR

0xC00000C5

STATUS_BAD_REMOTE_ADAPTER

0xC00000C6

STATUS_PRINT_QUEUE_FULL

0xC00000C7

STATUS_NO_SPOOL_SPACE

0xC00000C8

STATUS_PRINT_CANCELLED

0xC00000C9

STATUS_NETWORK_NAME_DELETED

Profiling is not stopped.

The passed ACL did not contain the
minimum required information.

The file that was specified as a target is a
directory, and the caller specified that it
could be anything but a directory.

The request is not supported.

This remote computer is not listening.

A duplicate name exists on the network.

The network path cannot be located.

The network is busy.

This device does not exist.

The network BIOS command limit has
been reached.

An I/O adapter hardware error has
occurred.

The network responded incorrectly.

An unexpected network error occurred.

The remote adapter is not compatible.

The print queue is full.

Space to store the file that is waiting to
be printed is not available on the server.

The requested print file has been
canceled.

The network name was deleted.

0xC00000CA

Network access is denied.

399 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_NETWORK_ACCESS_DENIED

0xC00000CB

STATUS_BAD_DEVICE_TYPE

0xC00000CC

STATUS_BAD_NETWORK_NAME

0xC00000CD

STATUS_TOO_MANY_NAMES

0xC00000CE

STATUS_TOO_MANY_SESSIONS

0xC00000CF

STATUS_SHARING_PAUSED

0xC00000D0

STATUS_REQUEST_NOT_ACCEPTED

0xC00000D1

STATUS_REDIRECTOR_PAUSED

0xC00000D2

STATUS_NET_WRITE_FAULT

0xC00000D3

STATUS_PROFILING_AT_LIMIT

0xC00000D4

STATUS_NOT_SAME_DEVICE

0xC00000D5

STATUS_FILE_RENAMED

0xC00000D6

STATUS_VIRTUAL_CIRCUIT_CLOSED

0xC00000D7

STATUS_NO_SECURITY_ON_OBJECT

0xC00000D8

STATUS_CANT_WAIT

0xC00000D9

STATUS_PIPE_EMPTY

0xC00000DA

STATUS_CANT_ACCESS_DOMAIN_INFO

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

{Incorrect Network Resource Type} The
specified device type (LPT, for example)
conflicts with the actual device type on
the remote resource.

{Network Name Not Found} The
specified share name cannot be found on
the remote server.

The name limit for the network adapter
card of the local computer was exceeded.

The network BIOS session limit was
exceeded.

File sharing has been temporarily paused.

No more connections can be made to this
remote computer at this time because
the computer has already accepted the
maximum number of connections.

Print or disk redirection is temporarily
paused.

A network data fault occurred.

The number of active profiling objects is
at the maximum and no more can be
started.

{Incorrect Volume} The destination file
of a rename request is located on a
different device than the source of the
rename request.

The specified file has been renamed and
thus cannot be modified.

{Network Request Timeout} The session
with a remote server has been
disconnected because the time-out
interval for a request has expired.

Indicates an attempt was made to
operate on the security of an object that
does not have security associated with it.

Used to indicate that an operation cannot
continue without blocking for I/O.

Used to indicate that a read operation
was done on an empty pipe.

Configuration information could not be
read from the domain controller, either
because the machine is unavailable or

400 / 497


Return value/code

0xC00000DB

STATUS_CANT_TERMINATE_SELF

0xC00000DC

STATUS_INVALID_SERVER_STATE

0xC00000DD

STATUS_INVALID_DOMAIN_STATE

0xC00000DE

STATUS_INVALID_DOMAIN_ROLE

0xC00000DF

STATUS_NO_SUCH_DOMAIN

0xC00000E0

STATUS_DOMAIN_EXISTS

0xC00000E1

STATUS_DOMAIN_LIMIT_EXCEEDED

0xC00000E2

STATUS_OPLOCK_NOT_GRANTED

0xC00000E3

STATUS_INVALID_OPLOCK_PROTOCOL

0xC00000E4

STATUS_INTERNAL_DB_CORRUPTION

0xC00000E5

STATUS_INTERNAL_ERROR

0xC00000E6

STATUS_GENERIC_NOT_MAPPED

0xC00000E7

STATUS_BAD_DESCRIPTOR_FORMAT

0xC00000E8

STATUS_INVALID_USER_BUFFER

0xC00000E9

STATUS_UNEXPECTED_IO_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

access has been denied.

Indicates that a thread attempted to
terminate itself by default (called
NtTerminateThread with NULL) and it was
the last thread in the current process.

Indicates the Sam Server was in the
wrong state to perform the desired
operation.

Indicates the domain was in the wrong
state to perform the desired operation.

This operation is only allowed for the
primary domain controller of the domain.

The specified domain did not exist.

The specified domain already exists.

An attempt was made to exceed the limit
on the number of domains per server for
this release.

An error status returned when the
opportunistic lock (oplock) request is
denied.

An error status returned when an invalid
opportunistic lock (oplock)
acknowledgment is received by a file
system.

This error indicates that the requested
operation cannot be completed due to a
catastrophic media failure or an on-disk
data structure corruption.

An internal error occurred.

Indicates generic access types were
contained in an access mask which
should already be mapped to non-generic
access types.

Indicates a security descriptor is not in
the necessary format (absolute or self-
relative).

An access to a user buffer failed at an
expected point in time. This code is
defined because the caller does not want
to accept STATUS_ACCESS_VIOLATION
in its filter.

If an I/O error that is not defined in the
standard FsRtl filter is returned, it is
converted to the following error, which is

401 / 497


Return value/code

Description

0xC00000EA

STATUS_UNEXPECTED_MM_CREATE_ERR

0xC00000EB

STATUS_UNEXPECTED_MM_MAP_ERROR

0xC00000EC

STATUS_UNEXPECTED_MM_EXTEND_ERR

0xC00000ED

STATUS_NOT_LOGON_PROCESS

0xC00000EE

STATUS_LOGON_SESSION_EXISTS

0xC00000EF

STATUS_INVALID_PARAMETER_1

0xC00000F0

STATUS_INVALID_PARAMETER_2

0xC00000F1

STATUS_INVALID_PARAMETER_3

0xC00000F2

STATUS_INVALID_PARAMETER_4

0xC00000F3

STATUS_INVALID_PARAMETER_5

0xC00000F4

STATUS_INVALID_PARAMETER_6

0xC00000F5

STATUS_INVALID_PARAMETER_7

0xC00000F6

STATUS_INVALID_PARAMETER_8

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

guaranteed to be in the filter. In this
case, information is lost; however, the
filter correctly handles the exception.

If an MM error that is not defined in the
standard FsRtl filter is returned, it is
converted to one of the following errors,
which are guaranteed to be in the filter.
In this case, information is lost; however,
the filter correctly handles the exception.

If an MM error that is not defined in the
standard FsRtl filter is returned, it is
converted to one of the following errors,
which are guaranteed to be in the filter.
In this case, information is lost; however,
the filter correctly handles the exception.

If an MM error that is not defined in the
standard FsRtl filter is returned, it is
converted to one of the following errors,
which are guaranteed to be in the filter.
In this case, information is lost; however,
the filter correctly handles the exception.

The requested action is restricted for use
by logon processes only. The calling
process has not registered as a logon
process.

An attempt has been made to start a new
session manager or LSA logon session by
using an ID that is already in use.

An invalid parameter was passed to a
service or function as the first argument.

An invalid parameter was passed to a
service or function as the second
argument.

An invalid parameter was passed to a
service or function as the third argument.

An invalid parameter was passed to a
service or function as the fourth
argument.

An invalid parameter was passed to a
service or function as the fifth argument.

An invalid parameter was passed to a
service or function as the sixth argument.

An invalid parameter was passed to a
service or function as the seventh
argument.

An invalid parameter was passed to a
service or function as the eighth
argument.

402 / 497


Return value/code

0xC00000F7

STATUS_INVALID_PARAMETER_9

0xC00000F8

STATUS_INVALID_PARAMETER_10

0xC00000F9

STATUS_INVALID_PARAMETER_11

0xC00000FA

STATUS_INVALID_PARAMETER_12

0xC00000FB

STATUS_REDIRECTOR_NOT_STARTED

0xC00000FC

STATUS_REDIRECTOR_STARTED

0xC00000FD

STATUS_STACK_OVERFLOW

0xC00000FE

STATUS_NO_SUCH_PACKAGE

0xC00000FF

STATUS_BAD_FUNCTION_TABLE

0xC0000100

STATUS_VARIABLE_NOT_FOUND

0xC0000101

STATUS_DIRECTORY_NOT_EMPTY

0xC0000102

STATUS_FILE_CORRUPT_ERROR

0xC0000103

STATUS_NOT_A_DIRECTORY

0xC0000104

STATUS_BAD_LOGON_SESSION_STATE

0xC0000105

STATUS_LOGON_SESSION_COLLISION

0xC0000106

STATUS_NAME_TOO_LONG

0xC0000107

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An invalid parameter was passed to a
service or function as the ninth
argument.

An invalid parameter was passed to a
service or function as the tenth
argument.

An invalid parameter was passed to a
service or function as the eleventh
argument.

An invalid parameter was passed to a
service or function as the twelfth
argument.

An attempt was made to access a
network file, but the network software
was not yet started.

An attempt was made to start the
redirector, but the redirector has already
been started.

A new guard page for the stack cannot be
created.

A specified authentication package is
unknown.

A malformed function table was
encountered during an unwind operation.

Indicates the specified environment
variable name was not found in the
specified environment block.

Indicates that the directory trying to be
deleted is not empty.

{Corrupt File} The file or directory %hs is
corrupt and unreadable. Run the Chkdsk
utility.

A requested opened file is not a
directory.

The logon session is not in a state that is
consistent with the requested operation.

An internal LSA error has occurred. An
authentication package has requested the
creation of a logon session but the ID of
an already existing logon session has
been specified.

A specified name string is too long for its
intended use.

The user attempted to force close the
files on a redirected drive, but there were

403 / 497


Return value/code

STATUS_FILES_OPEN

0xC0000108

STATUS_CONNECTION_IN_USE

0xC0000109

STATUS_MESSAGE_NOT_FOUND

0xC000010A

STATUS_PROCESS_IS_TERMINATING

0xC000010B

STATUS_INVALID_LOGON_TYPE

0xC000010C

STATUS_NO_GUID_TRANSLATION

0xC000010D

STATUS_CANNOT_IMPERSONATE

0xC000010E

STATUS_IMAGE_ALREADY_LOADED

0xC0000117

STATUS_NO_LDT

0xC0000118

STATUS_INVALID_LDT_SIZE

0xC0000119

STATUS_INVALID_LDT_OFFSET

0xC000011A

STATUS_INVALID_LDT_DESCRIPTOR

0xC000011B

STATUS_INVALID_IMAGE_NE_FORMAT

0xC000011C

STATUS_RXACT_INVALID_STATE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

opened files on the drive, and the user
did not specify a sufficient level of force.

The user attempted to force close the
files on a redirected drive, but there were
opened directories on the drive, and the
user did not specify a sufficient level of
force.

RtlFindMessage could not locate the
requested message ID in the message
table resource.

An attempt was made to duplicate an
object handle into or out of an exiting
process.

Indicates an invalid value has been
provided for the LogonType requested.

Indicates that an attempt was made to
assign protection to a file system file or
directory and one of the SIDs in the
security descriptor could not be
translated into a GUID that could be
stored by the file system. This causes the
protection attempt to fail, which might
cause a file creation attempt to fail.

Indicates that an attempt has been made
to impersonate via a named pipe that has
not yet been read from.

Indicates that the specified image is
already loaded.

Indicates that an attempt was made to
change the size of the LDT for a process
that has no LDT.

Indicates that an attempt was made to
grow an LDT by setting its size, or that
the size was not an even number of
selectors.

Indicates that the starting value for the
LDT information was not an integral
multiple of the selector size.

Indicates that the user supplied an
invalid descriptor when trying to set up
LDT descriptors.

The specified image file did not have the
correct format. It appears to be NE
format.

Indicates that the transaction state of a
registry subtree is incompatible with the
requested operation. For example, a
request has been made to start a new
transaction with one already in progress,

404 / 497


Return value/code

Description

0xC000011D

STATUS_RXACT_COMMIT_FAILURE

0xC000011E

STATUS_MAPPED_FILE_SIZE_ZERO

0xC000011F

STATUS_TOO_MANY_OPENED_FILES

0xC0000120

STATUS_CANCELLED

0xC0000121

STATUS_CANNOT_DELETE

0xC0000122

STATUS_INVALID_COMPUTER_NAME

0xC0000123

STATUS_FILE_DELETED

0xC0000124

STATUS_SPECIAL_ACCOUNT

0xC0000125

STATUS_SPECIAL_GROUP

0xC0000126

STATUS_SPECIAL_USER

0xC0000127

STATUS_MEMBERS_PRIMARY_GROUP

0xC0000128

STATUS_FILE_CLOSED

0xC0000129

STATUS_TOO_MANY_THREADS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

or a request has been made to apply a
transaction when one is not currently in
progress.

Indicates an error has occurred during a
registry transaction commit. The
database has been left in an unknown,
but probably inconsistent, state. The
state of the registry transaction is left as
COMMITTING.

An attempt was made to map a file of
size zero with the maximum size
specified as zero.

Too many files are opened on a remote
server. This error should only be returned
by the Windows redirector on a remote
drive.

The I/O request was canceled.

An attempt has been made to remove a
file or directory that cannot be deleted.

Indicates a name that was specified as a
remote computer name is syntactically
invalid.

An I/O request other than close was
performed on a file after it was deleted,
which can only happen to a request that
did not complete before the last handle
was closed via NtClose.

Indicates an operation that is
incompatible with built-in accounts has
been attempted on a built-in (special)
SAM account. For example, built-in
accounts cannot be deleted.

The operation requested cannot be
performed on the specified group
because it is a built-in special group.

The operation requested cannot be
performed on the specified user because
it is a built-in special user.

Indicates a member cannot be removed
from a group because the group is
currently the member's primary group.

An I/O request other than close and
several other special case operations was
attempted using a file object that had
already been closed.

Indicates a process has too many threads
to perform the requested action. For
example, assignment of a primary token

405 / 497


Return value/code

Description

0xC000012A

STATUS_THREAD_NOT_IN_PROCESS

0xC000012B

STATUS_TOKEN_ALREADY_IN_USE

0xC000012C

STATUS_PAGEFILE_QUOTA_EXCEEDED

0xC000012D

STATUS_COMMITMENT_LIMIT

0xC000012E

STATUS_INVALID_IMAGE_LE_FORMAT

0xC000012F

STATUS_INVALID_IMAGE_NOT_MZ

0xC0000130

STATUS_INVALID_IMAGE_PROTECT

0xC0000131

STATUS_INVALID_IMAGE_WIN_16

0xC0000132

STATUS_LOGON_SERVER_CONFLICT

0xC0000133

STATUS_TIME_DIFFERENCE_AT_DC

0xC0000134

STATUS_SYNCHRONIZATION_REQUIRED

0xC0000135

STATUS_DLL_NOT_FOUND

0xC0000136

STATUS_OPEN_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

can be performed only when a process
has zero or one threads.

An attempt was made to operate on a
thread within a specific process, but the
specified thread is not in the specified
process.

An attempt was made to establish a
token for use as a primary token but the
token is already in use. A token can only
be the primary token of one process at a
time.

The page file quota was exceeded.

{Out of Virtual Memory} Your system is
low on virtual memory. To ensure that
Windows runs correctly, increase the size
of your virtual memory paging file. For
more information, see Help.

The specified image file did not have the
correct format: it appears to be LE
format.

The specified image file did not have the
correct format: it did not have an initial
MZ.

The specified image file did not have the
correct format: it did not have a proper
e_lfarlc in the MZ header.

The specified image file did not have the
correct format: it appears to be a 16-bit
Windows image.

The Netlogon service cannot start
because another Netlogon service
running in the domain conflicts with the
specified role.

The time at the primary domain
controller is different from the time at the
backup domain controller or member
server by too large an amount.

On applicable Windows Server releases,
the SAM database is significantly out of
synchronization with the copy on the
domain controller. A complete
synchronization is required.

{Unable To Locate Component} This
application has failed to start because
%hs was not found. Reinstalling the
application might fix this problem.

The NtCreateFile API failed. This error
should never be returned to an
application; it is a place holder for the

406 / 497


Return value/code

Description

0xC0000137

STATUS_IO_PRIVILEGE_FAILED

0xC0000138

STATUS_ORDINAL_NOT_FOUND

0xC0000139

STATUS_ENTRYPOINT_NOT_FOUND

0xC000013A

STATUS_CONTROL_C_EXIT

0xC000013B

STATUS_LOCAL_DISCONNECT

0xC000013C

STATUS_REMOTE_DISCONNECT

0xC000013D

STATUS_REMOTE_RESOURCES

0xC000013E

STATUS_LINK_FAILED

0xC000013F

STATUS_LINK_TIMEOUT

0xC0000140

STATUS_INVALID_CONNECTION

0xC0000141

STATUS_INVALID_ADDRESS

0xC0000142

STATUS_DLL_INIT_FAILED

0xC0000143

STATUS_MISSING_SYSTEMFILE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Windows LAN Manager Redirector to use
in its internal error-mapping routines.

{Privilege Failed} The I/O permissions for
the process could not be changed.

{Ordinal Not Found} The ordinal %ld
could not be located in the dynamic link
library %hs.

{Entry Point Not Found} The procedure
entry point %hs could not be located in
the dynamic link library %hs.

{Application Exit by CTRL+C} The
application terminated as a result of a
CTRL+C.

{Virtual Circuit Closed} The network
transport on your computer has closed a
network connection. There might or
might not be I/O requests outstanding.

{Virtual Circuit Closed} The network
transport on a remote computer has
closed a network connection. There might
or might not be I/O requests
outstanding.

{Insufficient Resources on Remote
Computer} The remote computer has
insufficient resources to complete the
network request. For example, the
remote computer might not have enough
available memory to carry out the
request at this time.

{Virtual Circuit Closed} An existing
connection (virtual circuit) has been
broken at the remote computer. There is
probably something wrong with the
network software protocol or the network
hardware on the remote computer.

{Virtual Circuit Closed} The network
transport on your computer has closed a
network connection because it had to
wait too long for a response from the
remote computer.

The connection handle that was given to
the transport was invalid.

The address handle that was given to the
transport was invalid.

{DLL Initialization Failed} Initialization of
the dynamic link library %hs failed. The
process is terminating abnormally.

{Missing System File} The required
system file %hs is bad or missing.

407 / 497


Return value/code

0xC0000144

STATUS_UNHANDLED_EXCEPTION

0xC0000145

STATUS_APP_INIT_FAILURE

0xC0000146

STATUS_PAGEFILE_CREATE_FAILED

0xC0000147

STATUS_NO_PAGEFILE

0xC0000148

STATUS_INVALID_LEVEL

0xC0000149

STATUS_WRONG_PASSWORD_CORE

0xC000014A

STATUS_ILLEGAL_FLOAT_CONTEXT

0xC000014B

STATUS_PIPE_BROKEN

0xC000014C

STATUS_REGISTRY_CORRUPT

0xC000014D

STATUS_REGISTRY_IO_FAILED

0xC000014E

STATUS_NO_EVENT_PAIR

0xC000014F

STATUS_UNRECOGNIZED_VOLUME

0xC0000150

STATUS_SERIAL_NO_DEVICE_INITED

0xC0000151

STATUS_NO_SUCH_ALIAS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Application Error} The exception %s
(0x%08lx) occurred in the application at
location 0x%08lx.

{Application Error} The application failed
to initialize properly (0x%lx). Click OK to
terminate the application.

{Unable to Create Paging File} The
creation of the paging file %hs failed
(%lx). The requested size was %ld.

{No Paging File Specified} No paging file
was specified in the system configuration.

{Incorrect System Call Level} An invalid
level was passed into the specified
system call.

{Incorrect Password to LAN Manager
Server} You specified an incorrect
password to a LAN Manager 2.x or MS-
NET server.

{EXCEPTION} A real-mode application
issued a floating-point instruction and
floating-point hardware is not present.

The pipe operation has failed because the
other end of the pipe has been closed.

{The Registry Is Corrupt} The structure
of one of the files that contains registry
data is corrupt; the image of the file in
memory is corrupt; or the file could not
be recovered because the alternate copy
or log was absent or corrupt.

An I/O operation initiated by the Registry
failed and cannot be recovered. The
registry could not read in, write out, or
flush one of the files that contain the
system's image of the registry.

An event pair synchronization operation
was performed using the thread-specific
client/server event pair object, but no
event pair object was associated with the
thread.

The volume does not contain a
recognized file system. Be sure that all
required file system drivers are loaded
and that the volume is not corrupt.

No serial device was successfully
initialized. The serial driver will unload.

The specified local group does not exist.

408 / 497


Return value/code

0xC0000152

STATUS_MEMBER_NOT_IN_ALIAS

0xC0000153

STATUS_MEMBER_IN_ALIAS

0xC0000154

STATUS_ALIAS_EXISTS

0xC0000155

STATUS_LOGON_NOT_GRANTED

0xC0000156

STATUS_TOO_MANY_SECRETS

0xC0000157

STATUS_SECRET_TOO_LONG

0xC0000158

STATUS_INTERNAL_DB_ERROR

0xC0000159

STATUS_FULLSCREEN_MODE

0xC000015A

STATUS_TOO_MANY_CONTEXT_IDS

0xC000015B

STATUS_LOGON_TYPE_NOT_GRANTED

0xC000015C

STATUS_NOT_REGISTRY_FILE

0xC000015D

STATUS_NT_CROSS_ENCRYPTION_REQUIRED

0xC000015E

STATUS_DOMAIN_CTRLR_CONFIG_ERROR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified account name is not a
member of the group.

The specified account name is already a
member of the group.

The specified local group already exists.

A requested type of logon (for example,
interactive, network, and service) is not
granted by the local security policy of the
target system. Ask the system
administrator to grant the necessary
form of logon.

The maximum number of secrets that
can be stored in a single system was
exceeded. The length and number of
secrets is limited to satisfy U.S. State
Department export restrictions.

The length of a secret exceeds the
maximum allowable length. The length
and number of secrets is limited to
satisfy U.S. State Department export
restrictions.

The local security authority (LSA)
database contains an internal
inconsistency.

The requested operation cannot be
performed in full-screen mode.

During a logon attempt, the user's
security context accumulated too many
security IDs. This is a very unusual
situation. Remove the user from some
global or local groups to reduce the
number of security IDs to incorporate
into the security context.

A user has requested a type of logon (for
example, interactive or network) that has
not been granted. An administrator has
control over who can logon interactively
and through the network.

The system has attempted to load or
restore a file into the registry, and the
specified file is not in the format of a
registry file.

An attempt was made to change a user
password in the security account
manager without providing the necessary
Windows cross-encrypted password.

A domain server has an incorrect
configuration.

409 / 497


Return value/code

Description

0xC000015F

STATUS_FT_MISSING_MEMBER

0xC0000160

STATUS_ILL_FORMED_SERVICE_ENTRY

0xC0000161

STATUS_ILLEGAL_CHARACTER

0xC0000162

STATUS_UNMAPPABLE_CHARACTER

0xC0000163

STATUS_UNDEFINED_CHARACTER

0xC0000164

STATUS_FLOPPY_VOLUME

0xC0000165

STATUS_FLOPPY_ID_MARK_NOT_FOUND

0xC0000166

STATUS_FLOPPY_WRONG_CYLINDER

0xC0000167

STATUS_FLOPPY_UNKNOWN_ERROR

0xC0000168

STATUS_FLOPPY_BAD_REGISTERS

0xC0000169

STATUS_DISK_RECALIBRATE_FAILED

0xC000016A

STATUS_DISK_OPERATION_FAILED

0xC000016B

STATUS_DISK_RESET_FAILED

0xC000016C

STATUS_SHARED_IRQ_BUSY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

An attempt was made to explicitly access
the secondary copy of information via a
device control to the fault tolerance
driver and the secondary copy is not
present in the system.

A configuration registry node that
represents a driver service entry was ill-
formed and did not contain the required
value entries.

An illegal character was encountered. For
a multibyte character set, this includes a
lead byte without a succeeding trail byte.
For the Unicode character set this
includes the characters 0xFFFF and
0xFFFE.

No mapping for the Unicode character
exists in the target multibyte code page.

The Unicode character is not defined in
the Unicode character set that is installed
on the system.

The paging file cannot be created on a
floppy disk.

{Floppy Disk Error} While accessing a
floppy disk, an ID address mark was not
found.

{Floppy Disk Error} While accessing a
floppy disk, the track address from the
sector ID field was found to be different
from the track address that is maintained
by the controller.

{Floppy Disk Error} The floppy disk
controller reported an error that is not
recognized by the floppy disk driver.

{Floppy Disk Error} While accessing a
floppy-disk, the controller returned
inconsistent results via its registers.

{Hard Disk Error} While accessing the
hard disk, a recalibrate operation failed,
even after retries.

{Hard Disk Error} While accessing the
hard disk, a disk operation failed even
after retries.

{Hard Disk Error} While accessing the
hard disk, a disk controller reset was
needed, but even that failed.

An attempt was made to open a device
that was sharing an interrupt request
(IRQ) with other devices. At least one
other device that uses that IRQ was

410 / 497


Return value/code

Description

0xC000016D

STATUS_FT_ORPHANING

0xC000016E

STATUS_BIOS_FAILED_TO_CONNECT_INTERRUPT

0xC0000172

STATUS_PARTITION_FAILURE

0xC0000173

STATUS_INVALID_BLOCK_LENGTH

0xC0000174

STATUS_DEVICE_NOT_PARTITIONED

0xC0000175

STATUS_UNABLE_TO_LOCK_MEDIA

0xC0000176

STATUS_UNABLE_TO_UNLOAD_MEDIA

0xC0000177

STATUS_EOM_OVERFLOW

0xC0000178

STATUS_NO_MEDIA

0xC000017A

STATUS_NO_SUCH_MEMBER

0xC000017B

STATUS_INVALID_MEMBER

0xC000017C

STATUS_KEY_DELETED

0xC000017D

STATUS_NO_LOG_SPACE

0xC000017E

STATUS_TOO_MANY_SIDS

0xC000017F

STATUS_LM_CROSS_ENCRYPTION_REQUIRED

0xC0000180

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

already opened. Two concurrent opens of
devices that share an IRQ and only work
via interrupts is not supported for the
particular bus type that the devices use.

{FT Orphaning} A disk that is part of a
fault-tolerant volume can no longer be
accessed.

The basic input/output system (BIOS)
failed to connect a system interrupt to
the device or bus for which the device is
connected.

The tape could not be partitioned.

When accessing a new tape of a multi-
volume partition, the current blocksize is
incorrect.

The tape partition information could not
be found when loading a tape.

An attempt to lock the eject media
mechanism failed.

An attempt to unload media failed.

The physical end of tape was detected.

{No Media} There is no media in the
drive. Insert media into drive %hs.

A member could not be added to or
removed from the local group because
the member does not exist.

A new member could not be added to a
local group because the member has the
wrong account type.

An illegal operation was attempted on a
registry key that has been marked for
deletion.

The system could not allocate the
required space in a registry log.

Too many SIDs have been specified.

An attempt was made to change a user
password in the security account
manager without providing the necessary
LM cross-encrypted password.

An attempt was made to create a
symbolic link in a registry key that

411 / 497


Return value/code

STATUS_KEY_HAS_CHILDREN

0xC0000181

STATUS_CHILD_MUST_BE_VOLATILE

0xC0000182

STATUS_DEVICE_CONFIGURATION_ERROR

0xC0000183

STATUS_DRIVER_INTERNAL_ERROR

0xC0000184

STATUS_INVALID_DEVICE_STATE

0xC0000185

STATUS_IO_DEVICE_ERROR

0xC0000186

STATUS_DEVICE_PROTOCOL_ERROR

0xC0000187

STATUS_BACKUP_CONTROLLER

0xC0000188

STATUS_LOG_FILE_FULL

0xC0000189

STATUS_TOO_LATE

0xC000018A

STATUS_NO_TRUST_LSA_SECRET

0xC000018B

STATUS_NO_TRUST_SAM_ACCOUNT

0xC000018C

STATUS_TRUSTED_DOMAIN_FAILURE

0xC000018D

STATUS_TRUSTED_RELATIONSHIP_FAILURE

0xC000018E

STATUS_EVENTLOG_FILE_CORRUPT

0xC000018F

STATUS_EVENTLOG_CANT_START

0xC0000190

STATUS_TRUST_FAILURE

0xC0000191

STATUS_MUTANT_LIMIT_EXCEEDED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

already has subkeys or values.

An attempt was made to create a stable
subkey under a volatile parent key.

The I/O device is configured incorrectly
or the configuration parameters to the
driver are incorrect.

An error was detected between two
drivers or within an I/O driver.

The device is not in a valid state to
perform this request.

The I/O device reported an I/O error.

A protocol error was detected between
the driver and the device.

This operation is only allowed for the
primary domain controller of the domain.

The log file space is insufficient to
support this operation.

A write operation was attempted to a
volume after it was dismounted.

The workstation does not have a trust
secret for the primary domain in the local
LSA database.

On applicable Windows Server releases,
the SAM database does not have a
computer account for this workstation
trust relationship.

The logon request failed because the
trust relationship between the primary
domain and the trusted domain failed.

The logon request failed because the
trust relationship between this
workstation and the primary domain
failed.

The Eventlog log file is corrupt.

No Eventlog log file could be opened. The
Eventlog service did not start.

The network logon failed. This might be
because the validation authority cannot
be reached.

An attempt was made to acquire a
mutant such that its maximum count

412 / 497


Return value/code

0xC0000192

STATUS_NETLOGON_NOT_STARTED

0xC0000193

STATUS_ACCOUNT_EXPIRED

0xC0000194

STATUS_POSSIBLE_DEADLOCK

0xC0000195

STATUS_NETWORK_CREDENTIAL_CONFLICT

0xC0000196

STATUS_REMOTE_SESSION_LIMIT

0xC0000197

STATUS_EVENTLOG_FILE_CHANGED

0xC0000198

STATUS_NOLOGON_INTERDOMAIN_TRUST_ACCOUNT

0xC0000199

STATUS_NOLOGON_WORKSTATION_TRUST_ACCOUNT

0xC000019A

STATUS_NOLOGON_SERVER_TRUST_ACCOUNT

0xC000019B

STATUS_DOMAIN_TRUST_INCONSISTENT

0xC000019C

STATUS_FS_DRIVER_REQUIRED

0xC000019D

STATUS_IMAGE_ALREADY_LOADED_AS_DLL

0xC000019E

STATUS_INCOMPATIBLE_WITH_GLOBAL_SHORT_NAME_REGISTRY_
SETTING

Description

would have been exceeded.

An attempt was made to logon, but the
NetLogon service was not started.

The user account has expired.

{EXCEPTION} Possible deadlock
condition.

Multiple connections to a server or shared
resource by the same user, using more
than one user name, are not allowed.
Disconnect all previous connections to
the server or shared resource and try
again.

An attempt was made to establish a
session to a network server, but there
are already too many sessions
established to that server.

The log file has changed between reads.

The account used is an interdomain trust
account. Use your global user account or
local user account to access this server.

The account used is a computer account.
Use your global user account or local user
account to access this server.

The account used is a server trust
account. Use your global user account or
local user account to access this server.

The name or SID of the specified domain
is inconsistent with the trust information
for that domain.

A volume has been accessed for which a
file system driver is required that has not
yet been loaded.

Indicates that the specified image is
already loaded as a DLL.

Short name settings cannot be changed
on this volume due to the global registry
setting.

0xC000019F

STATUS_SHORT_NAMES_NOT_ENABLED_ON_VOLUME

Short names are not enabled on this
volume.

0xC00001A0

STATUS_SECURITY_STREAM_IS_INCONSISTENT

0xC00001A1

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The security stream for the given volume
is in an inconsistent state. Please run
CHKDSK on the volume.

A requested file lock operation cannot be

413 / 497


Return value/code

Description

STATUS_INVALID_LOCK_RANGE

processed due to an invalid byte range.

0xC00001A2

STATUS_INVALID_ACE_CONDITION

0xC00001A3

STATUS_IMAGE_SUBSYSTEM_NOT_PRESENT

0xC00001A4

STATUS_NOTIFICATION_GUID_ALREADY_DEFINED

0xC0000201

STATUS_NETWORK_OPEN_RESTRICTION

0xC0000202

STATUS_NO_USER_SESSION_KEY

0xC0000203

STATUS_USER_SESSION_DELETED

0xC0000204

STATUS_RESOURCE_LANG_NOT_FOUND

0xC0000205

STATUS_INSUFF_SERVER_RESOURCES

0xC0000206

STATUS_INVALID_BUFFER_SIZE

0xC0000207

STATUS_INVALID_ADDRESS_COMPONENT

0xC0000208

STATUS_INVALID_ADDRESS_WILDCARD

0xC0000209

STATUS_TOO_MANY_ADDRESSES

0xC000020A

STATUS_ADDRESS_ALREADY_EXISTS

0xC000020B

STATUS_ADDRESS_CLOSED

0xC000020C

STATUS_CONNECTION_DISCONNECTED

0xC000020D

STATUS_CONNECTION_RESET

0xC000020E

STATUS_TOO_MANY_NODES

0xC000020F

STATUS_TRANSACTION_ABORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The specified access control entry (ACE)
contains an invalid condition.

The subsystem needed to support the
image type is not present.

The specified file already has a
notification GUID associated with it.

A remote open failed because the
network open restrictions were not
satisfied.

There is no user session key for the
specified logon session.

The remote user session has been
deleted.

Indicates the specified resource language
ID cannot be found in the image file.

Insufficient server resources exist to
complete the request.

The size of the buffer is invalid for the
specified operation.

The transport rejected the specified
network address as invalid.

The transport rejected the specified
network address due to invalid use of a
wildcard.

The transport address could not be
opened because all the available
addresses are in use.

The transport address could not be
opened because it already exists.

The transport address is now closed.

The transport connection is now
disconnected.

The transport connection has been reset.

The transport cannot dynamically acquire
any more nodes.

The transport aborted a pending
transaction.

414 / 497


Return value/code

0xC0000210

STATUS_TRANSACTION_TIMED_OUT

0xC0000211

STATUS_TRANSACTION_NO_RELEASE

0xC0000212

STATUS_TRANSACTION_NO_MATCH

0xC0000213

STATUS_TRANSACTION_RESPONDED

0xC0000214

STATUS_TRANSACTION_INVALID_ID

0xC0000215

STATUS_TRANSACTION_INVALID_TYPE

0xC0000216

STATUS_NOT_SERVER_SESSION

0xC0000217

STATUS_NOT_CLIENT_SESSION

0xC0000218

STATUS_CANNOT_LOAD_REGISTRY_FILE

0xC0000219

STATUS_DEBUG_ATTACH_FAILED

0xC000021A

STATUS_SYSTEM_PROCESS_TERMINATED

0xC000021B

STATUS_DATA_NOT_ACCEPTED

0xC000021C

STATUS_NO_BROWSER_SERVERS_FOUND

0xC000021D

STATUS_VDM_HARD_ERROR

0xC000021E

STATUS_DRIVER_CANCEL_TIMEOUT

0xC000021F

STATUS_REPLY_MESSAGE_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The transport timed out a request that is
waiting for a response.

The transport did not receive a release
for a pending response.

The transport did not find a transaction
that matches the specific token.

The transport had previously responded
to a transaction request.

The transport does not recognize the
specified transaction request ID.

The transport does not recognize the
specified transaction request type.

The transport can only process the
specified request on the server side of a
session.

The transport can only process the
specified request on the client side of a
session.

{Registry File Failure} The registry
cannot load the hive (file): %hs or its log
or alternate. It is corrupt, absent, or not
writable.

{Unexpected Failure in
DebugActiveProcess} An unexpected
failure occurred while processing a
DebugActiveProcess API request.
Choosing OK will terminate the process,
and choosing Cancel will ignore the error.

{Fatal System Error} The %hs system
process terminated unexpectedly with a
status of 0x%08x (0x%08x 0x%08x).
The system has been shut down.

{Data Not Accepted} The TDI client could
not handle the data received during an
indication.

{Unable to Retrieve Browser Server List}
The list of servers for this workgroup is
not currently available.

NTVDM encountered a hard error.

{Cancel Timeout} The driver %hs failed
to complete a canceled I/O request in the
allotted time.

{Reply Message Mismatch} An attempt
was made to reply to an LPC message,
but the thread specified by the client ID

415 / 497


Return value/code

Description

0xC0000220

STATUS_MAPPED_ALIGNMENT

0xC0000221

STATUS_IMAGE_CHECKSUM_MISMATCH

0xC0000222

STATUS_LOST_WRITEBEHIND_DATA

0xC0000223

STATUS_CLIENT_SERVER_PARAMETERS_INVALID

0xC0000224

STATUS_PASSWORD_MUST_CHANGE

0xC0000225

STATUS_NOT_FOUND

0xC0000226

STATUS_NOT_TINY_STREAM

0xC0000227

STATUS_RECOVERY_FAILURE

0xC0000228

STATUS_STACK_OVERFLOW_READ

0xC0000229

STATUS_FAIL_CHECK

0xC000022A

STATUS_DUPLICATE_OBJECTID

0xC000022B

STATUS_OBJECTID_EXISTS

0xC000022C

STATUS_CONVERT_TO_LARGE

0xC000022D

STATUS_RETRY

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

in the message was not waiting on that
message.

{Mapped View Alignment Incorrect} An
attempt was made to map a view of a
file, but either the specified base address
or the offset into the file were not aligned
on the proper allocation granularity.

{Bad Image Checksum} The image %hs
is possibly corrupt. The header checksum
does not match the computed checksum.

{Delayed Write Failed} Windows was
unable to save all the data for the file
%hs. The data has been lost. This error
might be caused by a failure of your
computer hardware or network
connection. Try to save this file
elsewhere.

The parameters passed to the server in
the client/server shared memory window
were invalid. Too much data might have
been put in the shared memory window.

The user password must be changed
before logging on the first time.

The object was not found.

The stream is not a tiny stream.

A transaction recovery failed.

The request must be handled by the
stack overflow code.

A consistency check failed.

The attempt to insert the ID in the index
failed because the ID is already in the
index.

The attempt to set the object ID failed
because the object already has an ID.

Internal OFS status codes indicating how
an allocation operation is handled. Either
it is retried after the containing oNode is
moved or the extent stream is converted
to a large stream.

The request needs to be retried.

416 / 497


Return value/code

0xC000022E

STATUS_FOUND_OUT_OF_SCOPE

0xC000022F

STATUS_ALLOCATE_BUCKET

0xC0000230

STATUS_PROPSET_NOT_FOUND

0xC0000231

STATUS_MARSHALL_OVERFLOW

0xC0000232

STATUS_INVALID_VARIANT

0xC0000233

STATUS_DOMAIN_CONTROLLER_NOT_FOUND

0xC0000234

STATUS_ACCOUNT_LOCKED_OUT

0xC0000235

STATUS_HANDLE_NOT_CLOSABLE

0xC0000236

STATUS_CONNECTION_REFUSED

0xC0000237

STATUS_GRACEFUL_DISCONNECT

0xC0000238

STATUS_ADDRESS_ALREADY_ASSOCIATED

0xC0000239

STATUS_ADDRESS_NOT_ASSOCIATED

0xC000023A

STATUS_CONNECTION_INVALID

0xC000023B

STATUS_CONNECTION_ACTIVE

0xC000023C

STATUS_NETWORK_UNREACHABLE

0xC000023D

STATUS_HOST_UNREACHABLE

0xC000023E

STATUS_PROTOCOL_UNREACHABLE

0xC000023F

STATUS_PORT_UNREACHABLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The attempt to find the object found an
object on the volume that matches by
ID; however, it is out of the scope of the
handle that is used for the operation.

The bucket array must be grown. Retry
the transaction after doing so.

The specified property set does not exist
on the object.

The user/kernel marshaling buffer has
overflowed.

The supplied variant structure contains
invalid data.

A domain controller for this domain was
not found.

The user account has been automatically
locked because too many invalid logon
attempts or password change attempts
have been requested.

NtClose was called on a handle that was
protected from close via
NtSetInformationObject.

The transport-connection attempt was
refused by the remote system.

The transport connection was gracefully
closed.

The transport endpoint already has an
address associated with it.

An address has not yet been associated
with the transport endpoint.

An operation was attempted on a
nonexistent transport connection.

An invalid operation was attempted on an
active transport connection.

The remote network is not reachable by
the transport.

The remote system is not reachable by
the transport.

The remote system does not support the
transport protocol.

No service is operating at the destination
port of the transport on the remote

417 / 497


Return value/code

0xC0000240

STATUS_REQUEST_ABORTED

0xC0000241

STATUS_CONNECTION_ABORTED

0xC0000242

STATUS_BAD_COMPRESSION_BUFFER

0xC0000243

STATUS_USER_MAPPED_FILE

0xC0000244

STATUS_AUDIT_FAILED

0xC0000245

STATUS_TIMER_RESOLUTION_NOT_SET

0xC0000246

STATUS_CONNECTION_COUNT_LIMIT

0xC0000247

STATUS_LOGIN_TIME_RESTRICTION

0xC0000248

STATUS_LOGIN_WKSTA_RESTRICTION

0xC0000249

STATUS_IMAGE_MP_UP_MISMATCH

0xC0000250

STATUS_INSUFFICIENT_LOGON_INFO

0xC0000251

STATUS_BAD_DLL_ENTRYPOINT

0xC0000252

STATUS_BAD_SERVICE_ENTRYPOINT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

system.

The request was aborted.

The transport connection was aborted by
the local system.

The specified buffer contains ill-formed
data.

The requested operation cannot be
performed on a file with a user mapped
section open.

{Audit Failed} An attempt to generate a
security audit failed.

The timer resolution was not previously
set by the current process.

A connection to the server could not be
made because the limit on the number of
concurrent connections for this account
has been reached.

Attempting to log on during an
unauthorized time of day for this
account.

The account is not authorized to log on
from this station.

{UP/MP Image Mismatch} The image
%hs has been modified for use on a
uniprocessor system, but you are running
it on a multiprocessor machine. Reinstall
the image file.

There is insufficient account information
to log you on.

{Invalid DLL Entrypoint} The dynamic
link library %hs is not written correctly.
The stack pointer has been left in an
inconsistent state. The entry point should
be declared as WINAPI or STDCALL.
Select YES to fail the DLL load. Select NO
to continue execution. Selecting NO
might cause the application to operate
incorrectly.

{Invalid Service Callback Entrypoint} The
%hs service is not written correctly. The
stack pointer has been left in an
inconsistent state. The callback entry
point should be declared as WINAPI or
STDCALL. Selecting OK will cause the
service to continue operation. However,
the service process might operate
incorrectly.

418 / 497


Return value/code

0xC0000253

STATUS_LPC_REPLY_LOST

0xC0000254

STATUS_IP_ADDRESS_CONFLICT1

0xC0000255

STATUS_IP_ADDRESS_CONFLICT2

0xC0000256

STATUS_REGISTRY_QUOTA_LIMIT

0xC0000257

STATUS_PATH_NOT_COVERED

0xC0000258

STATUS_NO_CALLBACK_ACTIVE

0xC0000259

STATUS_LICENSE_QUOTA_EXCEEDED

0xC000025A

STATUS_PWD_TOO_SHORT

0xC000025B

STATUS_PWD_TOO_RECENT

0xC000025C

STATUS_PWD_HISTORY_CONFLICT

0xC000025E

STATUS_PLUGPLAY_NO_DEVICE

0xC000025F

STATUS_UNSUPPORTED_COMPRESSION

0xC0000260

STATUS_INVALID_HW_PROFILE

0xC0000261

STATUS_INVALID_PLUGPLAY_DEVICE_PATH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The server received the messages but did
not send a reply.

There is an IP address conflict with
another system on the network.

There is an IP address conflict with
another system on the network.

{Low On Registry Space} The system has
reached the maximum size that is
allowed for the system part of the
registry. Additional storage requests will
be ignored.

The contacted server does not support
the indicated part of the DFS namespace.

A callback return system service cannot
be executed when no callback is active.

The service being accessed is licensed for
a particular number of connections. No
more connections can be made to the
service at this time because the service
has already accepted the maximum
number of connections.

The password provided is too short to
meet the policy of your user account.
Choose a longer password.

The policy of your user account does not
allow you to change passwords too
frequently. This is done to prevent users
from changing back to a familiar, but
potentially discovered, password. If you
feel your password has been
compromised, contact your administrator
immediately to have a new one assigned.

You have attempted to change your
password to one that you have used in
the past. The policy of your user account
does not allow this. Select a password
that you have not previously used.

You have attempted to load a legacy
device driver while its device instance
had been disabled.

The specified compression format is
unsupported.

The specified hardware profile
configuration is invalid.

The specified Plug and Play registry
device path is invalid.

419 / 497


Return value/code

0xC0000262

STATUS_DRIVER_ORDINAL_NOT_FOUND

0xC0000263

STATUS_DRIVER_ENTRYPOINT_NOT_FOUND

0xC0000264

STATUS_RESOURCE_NOT_OWNED

0xC0000265

STATUS_TOO_MANY_LINKS

0xC0000266

STATUS_QUOTA_LIST_INCONSISTENT

0xC0000267

STATUS_FILE_IS_OFFLINE

0xC0000268

STATUS_EVALUATION_EXPIRATION

0xC0000269

STATUS_ILLEGAL_DLL_RELOCATION

0xC000026A

STATUS_LICENSE_VIOLATION

0xC000026B

STATUS_DLL_INIT_FAILED_LOGOFF

0xC000026C

STATUS_DRIVER_UNABLE_TO_LOAD

0xC000026D

STATUS_DFS_UNAVAILABLE

0xC000026E

STATUS_VOLUME_DISMOUNTED

Description

{Driver Entry Point Not Found} The %hs
device driver could not locate the ordinal
%ld in driver %hs.

{Driver Entry Point Not Found} The %hs
device driver could not locate the entry
point %hs in driver %hs.

{Application Error} The application
attempted to release a resource it did not
own. Click OK to terminate the
application.

An attempt was made to create more
links on a file than the file system
supports.

The specified quota list is internally
inconsistent with its descriptor.

The specified file has been relocated to
offline storage.

{Windows Evaluation Notification} The
evaluation period for this installation of
Windows has expired. This system will
shutdown in 1 hour. To restore access to
this installation of Windows, upgrade this
installation by using a licensed
distribution of this product.

{Illegal System DLL Relocation} The
system DLL %hs was relocated in
memory. The application will not run
properly. The relocation occurred because
the DLL %hs occupied an address range
that is reserved for Windows system
DLLs. The vendor supplying the DLL
should be contacted for a new DLL.

{License Violation} The system has
detected tampering with your registered
product type. This is a violation of your
software license. Tampering with the
product type is not permitted.

{DLL Initialization Failed} The application
failed to initialize because the window
station is shutting down.

{Unable to Load Device Driver} %hs
device driver could not be loaded. Error
Status was 0x%x.

DFS is unavailable on the contacted
server.

An operation was attempted to a volume
after it was dismounted.

0xC000026F

An internal error occurred in the Win32

420 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_WX86_INTERNAL_ERROR

x86 emulation subsystem.

0xC0000270

STATUS_WX86_FLOAT_STACK_CHECK

0xC0000271

STATUS_VALIDATE_CONTINUE

0xC0000272

STATUS_NO_MATCH

0xC0000273

STATUS_NO_MORE_MATCHES

0xC0000275

STATUS_NOT_A_REPARSE_POINT

0xC0000276

STATUS_IO_REPARSE_TAG_INVALID

0xC0000277

STATUS_IO_REPARSE_TAG_MISMATCH

0xC0000278

STATUS_IO_REPARSE_DATA_INVALID

0xC0000279

STATUS_IO_REPARSE_TAG_NOT_HANDLED

0xC0000280

STATUS_REPARSE_POINT_NOT_RESOLVED

0xC0000281

STATUS_DIRECTORY_IS_A_REPARSE_POINT

0xC0000282

STATUS_RANGE_LIST_CONFLICT

0xC0000283

STATUS_SOURCE_ELEMENT_EMPTY

0xC0000284

STATUS_DESTINATION_ELEMENT_FULL

0xC0000285

STATUS_ILLEGAL_ELEMENT_ADDRESS

0xC0000286

STATUS_MAGAZINE_NOT_PRESENT

0xC0000287

STATUS_REINITIALIZATION_NEEDED

0xC000028A

STATUS_ENCRYPTION_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Win32 x86 emulation subsystem floating-
point stack check.

The validation process needs to continue
on to the next step.

There was no match for the specified key
in the index.

There are no more matches for the
current index enumeration.

The NTFS file or directory is not a reparse
point.

The Windows I/O reparse tag passed for
the NTFS reparse point is invalid.

The Windows I/O reparse tag does not
match the one that is in the NTFS reparse
point.

The user data passed for the NTFS
reparse point is invalid.

The layered file system driver for this I/O
tag did not handle it when needed.

The NTFS symbolic link could not be
resolved even though the initial file name
is valid.

The NTFS directory is a reparse point.

The range could not be added to the
range list because of a conflict.

The specified medium changer source
element contains no media.

The specified medium changer
destination element already contains
media.

The specified medium changer element
does not exist.

The specified element is contained in a
magazine that is no longer present.

The device requires re-initialization due
to hardware errors.

The file encryption attempt failed.

421 / 497


Return value/code

Description

0xC000028B

STATUS_DECRYPTION_FAILED

0xC000028C

STATUS_RANGE_NOT_FOUND

0xC000028D

STATUS_NO_RECOVERY_POLICY

0xC000028E

STATUS_NO_EFS

0xC000028F

STATUS_WRONG_EFS

0xC0000290

STATUS_NO_USER_KEYS

0xC0000291

STATUS_FILE_NOT_ENCRYPTED

0xC0000292

STATUS_NOT_EXPORT_FORMAT

0xC0000293

STATUS_FILE_ENCRYPTED

0xC0000295

STATUS_WMI_GUID_NOT_FOUND

0xC0000296

STATUS_WMI_INSTANCE_NOT_FOUND

0xC0000297

STATUS_WMI_ITEMID_NOT_FOUND

0xC0000298

STATUS_WMI_TRY_AGAIN

0xC0000299

STATUS_SHARED_POLICY

0xC000029A

STATUS_POLICY_OBJECT_NOT_FOUND

0xC000029B

STATUS_POLICY_ONLY_IN_DS

0xC000029C

STATUS_VOLUME_NOT_UPGRADED

The file decryption attempt failed.

The specified range could not be found in
the range list.

There is no encryption recovery policy
configured for this system.

The required encryption driver is not
loaded for this system.

The file was encrypted with a different
encryption driver than is currently
loaded.

There are no EFS keys defined for the
user.

The specified file is not encrypted.

The specified file is not in the defined EFS
export format.

The specified file is encrypted and the
user does not have the ability to decrypt
it.

The GUID passed was not recognized as
valid by a WMI data provider.

The instance name passed was not
recognized as valid by a WMI data
provider.

The data item ID passed was not
recognized as valid by a WMI data
provider.

The WMI request could not be completed
and should be retried.

The policy object is shared and can only
be modified at the root.

The policy object does not exist when it
should.

The requested policy information only
lives in the Ds.

The volume must be upgraded to enable
this feature.

0xC000029D

STATUS_REMOTE_STORAGE_NOT_ACTIVE

The remote storage service is not
operational at this time.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

422 / 497


Return value/code

0xC000029E

STATUS_REMOTE_STORAGE_MEDIA_ERROR

0xC000029F

STATUS_NO_TRACKING_SERVICE

0xC00002A0

STATUS_SERVER_SID_MISMATCH

0xC00002A1

STATUS_DS_NO_ATTRIBUTE_OR_VALUE

0xC00002A2

STATUS_DS_INVALID_ATTRIBUTE_SYNTAX

0xC00002A3

STATUS_DS_ATTRIBUTE_TYPE_UNDEFINED

0xC00002A4

STATUS_DS_ATTRIBUTE_OR_VALUE_EXISTS

0xC00002A5

STATUS_DS_BUSY

0xC00002A6

STATUS_DS_UNAVAILABLE

0xC00002A7

STATUS_DS_NO_RIDS_ALLOCATED

0xC00002A8

STATUS_DS_NO_MORE_RIDS

0xC00002A9

STATUS_DS_INCORRECT_ROLE_OWNER

0xC00002AA

STATUS_DS_RIDMGR_INIT_ERROR

0xC00002AB

STATUS_DS_OBJ_CLASS_VIOLATION

0xC00002AC

STATUS_DS_CANT_ON_NON_LEAF

0xC00002AD

STATUS_DS_CANT_ON_RDN

0xC00002AE

STATUS_DS_CANT_MOD_OBJ_CLASS

Description

The remote storage service encountered
a media error.

The tracking (workstation) service is not
running.

The server process is running under a
SID that is different from the SID that is
required by client.

The specified directory service attribute
or value does not exist.

The attribute syntax specified to the
directory service is invalid.

The attribute type specified to the
directory service is not defined.

The specified directory service attribute
or value already exists.

The directory service is busy.

The directory service is unavailable.

The directory service was unable to
allocate a relative identifier.

The directory service has exhausted the
pool of relative identifiers.

The requested operation could not be
performed because the directory service
is not the master for that type of
operation.

The directory service was unable to
initialize the subsystem that allocates
relative identifiers.

The requested operation did not satisfy
one or more constraints that are
associated with the class of the object.

The directory service can perform the
requested operation only on a leaf object.

The directory service cannot perform the
requested operation on the Relatively
Defined Name (RDN) attribute of an
object.

The directory service detected an attempt
to modify the object class of an object.

0xC00002AF

An error occurred while performing a

423 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_DS_CROSS_DOM_MOVE_FAILED

cross domain move operation.

0xC00002B0

STATUS_DS_GC_NOT_AVAILABLE

0xC00002B1

STATUS_DIRECTORY_SERVICE_REQUIRED

0xC00002B2

STATUS_REPARSE_ATTRIBUTE_CONFLICT

0xC00002B3

STATUS_CANT_ENABLE_DENY_ONLY

0xC00002B4

STATUS_FLOAT_MULTIPLE_FAULTS

0xC00002B5

STATUS_FLOAT_MULTIPLE_TRAPS

0xC00002B6

STATUS_DEVICE_REMOVED

0xC00002B7

STATUS_JOURNAL_DELETE_IN_PROGRESS

0xC00002B8

STATUS_JOURNAL_NOT_ACTIVE

0xC00002B9

STATUS_NOINTERFACE

0xC00002C1

STATUS_DS_ADMIN_LIMIT_EXCEEDED

0xC00002C2

STATUS_DRIVER_FAILED_SLEEP

0xC00002C3

STATUS_MUTUAL_AUTHENTICATION_FAILED

0xC00002C4

STATUS_CORRUPT_SYSTEM_FILE

0xC00002C5

STATUS_DATATYPE_MISALIGNMENT_ERROR

0xC00002C6

STATUS_WMI_READ_ONLY

0xC00002C7

STATUS_WMI_SET_FAILURE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Unable to contact the global catalog
server.

The requested operation requires a
directory service, and none was
available.

The reparse attribute cannot be set
because it is incompatible with an
existing attribute.

A group marked "use for deny only"
cannot be enabled.

{EXCEPTION} Multiple floating-point
faults.

{EXCEPTION} Multiple floating-point
traps.

The device has been removed.

The volume change journal is being
deleted.

The volume change journal is not active.

The requested interface is not supported.

A directory service resource limit has
been exceeded.

{System Standby Failed} The driver %hs
does not support standby mode.
Updating this driver allows the system to
go to standby mode.

Mutual Authentication failed. The server
password is out of date at the domain
controller.

The system file %1 has become corrupt
and has been replaced.

{EXCEPTION} Alignment Error A data
type misalignment error was detected in
a load or store instruction.

The WMI data item or data block is read-
only.

The WMI data item or data block could
not be changed.

424 / 497


Return value/code

0xC00002C8

STATUS_COMMITMENT_MINIMUM

0xC00002C9

STATUS_REG_NAT_CONSUMPTION

0xC00002CA

STATUS_TRANSPORT_FULL

0xC00002CB

STATUS_DS_SAM_INIT_FAILURE

0xC00002CC

STATUS_ONLY_IF_CONNECTED

0xC00002CD

STATUS_DS_SENSITIVE_GROUP_VIOLATION

0xC00002CE

STATUS_PNP_RESTART_ENUMERATION

0xC00002CF

STATUS_JOURNAL_ENTRY_DELETED

0xC00002D0

STATUS_DS_CANT_MOD_PRIMARYGROUPID

0xC00002D1

STATUS_SYSTEM_IMAGE_BAD_SIGNATURE

0xC00002D2

STATUS_PNP_REBOOT_REQUIRED

0xC00002D3

STATUS_POWER_STATE_INVALID

0xC00002D4

STATUS_DS_INVALID_GROUP_TYPE

0xC00002D5

STATUS_DS_NO_NEST_GLOBALGROUP_IN_MIXEDDOMAIN

0xC00002D6

STATUS_DS_NO_NEST_LOCALGROUP_IN_MIXEDDOMAIN

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

{Virtual Memory Minimum Too Low} Your
system is low on virtual memory.
Windows is increasing the size of your
virtual memory paging file. During this
process, memory requests for some
applications might be denied. For more
information, see Help.

{EXCEPTION} Register NaT consumption
faults. A NaT value is consumed on a
non-speculative instruction.

The transport element of the medium
changer contains media, which is causing
the operation to fail.

Security Accounts Manager initialization
failed because of the following error: %hs
Error Status: 0x%x. Click OK to shut
down this system and restart in Directory
Services Restore Mode. Check the event
log for more detailed information.

This operation is supported only when
you are connected to the server.

Only an administrator can modify the
membership list of an administrative
group.

A device was removed so enumeration
must be restarted.

The journal entry has been deleted from
the journal.

Cannot change the primary group ID of a
domain controller account.

{Fatal System Error} The system image
%s is not properly signed. The file has
been replaced with the signed file. The
system has been shut down.

The device will not start without a reboot.

The power state of the current device
cannot support this request.

The specified group type is invalid.

In a mixed domain, no nesting of a global
group if the group is security enabled.

In a mixed domain, cannot nest local
groups with other local groups, if the
group is security enabled.

425 / 497


Return value/code

0xC00002D7

STATUS_DS_GLOBAL_CANT_HAVE_LOCAL_MEMBER

0xC00002D8

STATUS_DS_GLOBAL_CANT_HAVE_UNIVERSAL_MEMBER

0xC00002D9

STATUS_DS_UNIVERSAL_CANT_HAVE_LOCAL_MEMBER

0xC00002DA

STATUS_DS_GLOBAL_CANT_HAVE_CROSSDOMAIN_MEMBER

0xC00002DB

STATUS_DS_LOCAL_CANT_HAVE_CROSSDOMAIN_LOCAL_MEMBER

0xC00002DC

STATUS_DS_HAVE_PRIMARY_MEMBERS

0xC00002DD

STATUS_WMI_NOT_SUPPORTED

0xC00002DE

STATUS_INSUFFICIENT_POWER

0xC00002DF

STATUS_SAM_NEED_BOOTKEY_PASSWORD

0xC00002E0

STATUS_SAM_NEED_BOOTKEY_FLOPPY

0xC00002E1

STATUS_DS_CANT_START

0xC00002E2

STATUS_DS_INIT_FAILURE

0xC00002E3

STATUS_SAM_INIT_FAILURE

0xC00002E4

STATUS_DS_GC_REQUIRED

0xC00002E5

STATUS_DS_LOCAL_MEMBER_OF_LOCAL_ONLY

0xC00002E6

STATUS_DS_NO_FPO_IN_UNIVERSAL_GROUPS

Description

A global group cannot have a local group
as a member.

A global group cannot have a universal
group as a member.

A universal group cannot have a local
group as a member.

A global group cannot have a cross-
domain member.

A local group cannot have another cross-
domain local group as a member.

Cannot change to a security-disabled
group because primary members are in
this group.

The WMI operation is not supported by
the data block or method.

There is not enough power to complete
the requested operation.

The Security Accounts Manager needs to
get the boot password.

The Security Accounts Manager needs to
get the boot key from the floppy disk.

The directory service cannot start.

The directory service could not start
because of the following error: %hs Error
Status: 0x%x. Click OK to shut down this
system and restart in Directory Services
Restore Mode. Check the event log for
more detailed information.

The Security Accounts Manager
initialization failed because of the
following error: %hs Error Status: 0x%x.
Click OK to shut down this system and
restart in Safe Mode. Check the event log
for more detailed information.

The requested operation can be
performed only on a global catalog
server.

A local group can only be a member of
other local groups in the same domain.

Foreign security principals cannot be
members of universal groups.

0xC00002E7

Your computer could not be joined to the

426 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_DS_MACHINE_ACCOUNT_QUOTA_EXCEEDED

0xC00002E9

STATUS_CURRENT_DOMAIN_NOT_ALLOWED

0xC00002EA

STATUS_CANNOT_MAKE

0xC00002EB

STATUS_SYSTEM_SHUTDOWN

0xC00002EC

STATUS_DS_INIT_FAILURE_CONSOLE

0xC00002ED

STATUS_DS_SAM_INIT_FAILURE_CONSOLE

0xC00002EE

STATUS_UNFINISHED_CONTEXT_DELETED

0xC00002EF

STATUS_NO_TGT_REPLY

0xC00002F0

STATUS_OBJECTID_NOT_FOUND

0xC00002F1

STATUS_NO_IP_ADDRESSES

0xC00002F2

STATUS_WRONG_CREDENTIAL_HANDLE

0xC00002F3

STATUS_CRYPTO_SYSTEM_INVALID

0xC00002F4

STATUS_MAX_REFERRALS_EXCEEDED

0xC00002F5

STATUS_MUST_BE_KDC

0xC00002F6

STATUS_STRONG_CRYPTO_NOT_SUPPORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

domain. You have exceeded the
maximum number of computer accounts
you are allowed to create in this domain.
Contact your system administrator to
have this limit reset or increased.

This operation cannot be performed on
the current domain.

The directory or file cannot be created.

The system is in the process of shutting
down.

Directory Services could not start
because of the following error: %hs Error
Status: 0x%x. Click OK to shut down the
system. You can use the recovery
console to diagnose the system further.

Security Accounts Manager initialization
failed because of the following error: %hs
Error Status: 0x%x. Click OK to shut
down the system. You can use the
recovery console to diagnose the system
further.

A security context was deleted before the
context was completed. This is
considered a logon failure.

The client is trying to negotiate a context
and the server requires user-to-user but
did not send a TGT reply.

An object ID was not found in the file.

Unable to accomplish the requested task
because the local machine does not have
any IP addresses.

The supplied credential handle does not
match the credential that is associated
with the security context.

The crypto system or checksum function
is invalid because a required function is
unavailable.

The number of maximum ticket referrals
has been exceeded.

The local machine must be a Kerberos
KDC (domain controller) and it is not.

The other end of the security negotiation
requires strong crypto but it is not
supported on the local machine.

427 / 497


Return value/code

0xC00002F7

STATUS_TOO_MANY_PRINCIPALS

0xC00002F8

STATUS_NO_PA_DATA

0xC00002F9

STATUS_PKINIT_NAME_MISMATCH

0xC00002FA

STATUS_SMARTCARD_LOGON_REQUIRED

0xC00002FB

STATUS_KDC_INVALID_REQUEST

0xC00002FC

STATUS_KDC_UNABLE_TO_REFER

0xC00002FD

STATUS_KDC_UNKNOWN_ETYPE

0xC00002FE

STATUS_SHUTDOWN_IN_PROGRESS

Description

The KDC reply contained more than one
principal name.

Expected to find PA data for a hint of
what etype to use, but it was not found.

The client certificate does not contain a
valid UPN, or does not match the client
name in the logon request. Contact your
administrator.

Smart card logon is required and was not
used.

An invalid request was sent to the KDC.

The KDC was unable to generate a
referral for the service requested.

The encryption type requested is not
supported by the KDC.

A system shutdown is in progress.

0xC00002FF

The server machine is shutting down.

STATUS_SERVER_SHUTDOWN_IN_PROGRESS

0xC0000300

STATUS_NOT_SUPPORTED_ON_SBS

0xC0000301

STATUS_WMI_GUID_DISCONNECTED

0xC0000302

STATUS_WMI_ALREADY_DISABLED

0xC0000303

STATUS_WMI_ALREADY_ENABLED

0xC0000304

STATUS_MFT_TOO_FRAGMENTED

0xC0000305

STATUS_COPY_PROTECTION_FAILURE

0xC0000306

STATUS_CSS_AUTHENTICATION_FAILURE

0xC0000307

STATUS_CSS_KEY_NOT_PRESENT

0xC0000308

STATUS_CSS_KEY_NOT_ESTABLISHED

This operation is not supported on a
computer running Windows Server 2003
for Small Business Server.

The WMI GUID is no longer available.

Collection or events for the WMI GUID is
already disabled.

Collection or events for the WMI GUID is
already enabled.

The master file table on the volume is too
fragmented to complete this operation.

Copy protection failure.

Copy protection error—DVD CSS
Authentication failed.

Copy protection error—The specified
sector does not contain a valid key.

Copy protection error—DVD session key
not established.

0xC0000309

Copy protection error—The read failed

428 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_CSS_SCRAMBLED_SECTOR

because the sector is encrypted.

0xC000030A

STATUS_CSS_REGION_MISMATCH

0xC000030B

STATUS_CSS_RESETS_EXHAUSTED

0xC0000320

STATUS_PKINIT_FAILURE

0xC0000321

STATUS_SMARTCARD_SUBSYSTEM_FAILURE

0xC0000322

STATUS_NO_KERB_KEY

0xC0000350

STATUS_HOST_DOWN

0xC0000351

STATUS_UNSUPPORTED_PREAUTH

0xC0000352

STATUS_EFS_ALG_BLOB_TOO_BIG

0xC0000353

STATUS_PORT_NOT_SET

0xC0000354

STATUS_DEBUGGER_INACTIVE

0xC0000355

STATUS_DS_VERSION_CHECK_FAILURE

0xC0000356

STATUS_AUDITING_DISABLED

0xC0000357

STATUS_PRENT4_MACHINE_ACCOUNT

Copy protection error—The region of the
specified DVD does not correspond to the
region setting of the drive.

Copy protection error—The region setting
of the drive might be permanent.

The Kerberos protocol encountered an
error while validating the KDC certificate
during smart card logon. There is more
information in the system event log.

The Kerberos protocol encountered an
error while attempting to use the smart
card subsystem.

The target server does not have
acceptable Kerberos credentials.

The transport determined that the
remote system is down.

An unsupported pre-authentication
mechanism was presented to the
Kerberos package.

The encryption algorithm that is used on
the source file needs a bigger key buffer
than the one that is used on the
destination file.

An attempt to remove a processes
DebugPort was made, but a port was not
already associated with the process.

An attempt to do an operation on a
debug port failed because the port is in
the process of being deleted.

This version of Windows is not compatible
with the behavior version of the directory
forest, domain, or domain controller.

The specified event is currently not being
audited.

The machine account was created prior to
Windows NT 4.0. The account needs to
be recreated.

0xC0000358

STATUS_DS_AG_CANT_HAVE_UNIVERSAL_MEMBER

An account group cannot have a
universal group as a member.

0xC0000359

STATUS_INVALID_IMAGE_WIN_32

0xC000035A

STATUS_INVALID_IMAGE_WIN_64

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The specified image file did not have the
correct format; it appears to be a 32-bit
Windows image.

The specified image file did not have the
correct format; it appears to be a 64-bit

429 / 497


Return value/code

0xC000035B

STATUS_BAD_BINDINGS

0xC000035C

STATUS_NETWORK_SESSION_EXPIRED

0xC000035D

STATUS_APPHELP_BLOCK

0xC000035E

STATUS_ALL_SIDS_FILTERED

0xC000035F

STATUS_NOT_SAFE_MODE_DRIVER

0xC0000361

STATUS_ACCESS_DISABLED_BY_POLICY_DEFAULT

0xC0000362

STATUS_ACCESS_DISABLED_BY_POLICY_PATH

0xC0000363

STATUS_ACCESS_DISABLED_BY_POLICY_PUBLISHER

0xC0000364

STATUS_ACCESS_DISABLED_BY_POLICY_OTHER

0xC0000365

STATUS_FAILED_DRIVER_ENTRY

0xC0000366

STATUS_DEVICE_ENUMERATION_ERROR

0xC0000368

STATUS_MOUNT_POINT_NOT_RESOLVED

0xC0000369

STATUS_INVALID_DEVICE_OBJECT_PARAMETER

0xC000036A

STATUS_MCA_OCCURED

0xC000036B

STATUS_DRIVER_BLOCKED_CRITICAL

0xC000036C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Windows image.

The client's supplied SSPI channel
bindings were incorrect.

The client session has expired; so the
client must re-authenticate to continue
accessing the remote resources.

The AppHelp dialog box canceled; thus
preventing the application from starting.

The SID filtering operation removed all
SIDs.

The driver was not loaded because the
system is starting in safe mode.

Access to %1 has been restricted by your
Administrator by the default software
restriction policy level.

Access to %1 has been restricted by your
Administrator by location with policy rule
%2 placed on path %3.

Access to %1 has been restricted by your
Administrator by software publisher
policy.

Access to %1 has been restricted by your
Administrator by policy rule %2.

The driver was not loaded because it
failed its initialization call.

The device encountered an error while
applying power or reading the device
configuration. This might be caused by a
failure of your hardware or by a poor
connection.

The create operation failed because the
name contained at least one mount point
that resolves to a volume to which the
specified device object is not attached.

The device object parameter is either not
a valid device object or is not attached to
the volume that is specified by the file
name.

A machine check error has occurred.
Check the system event log for additional
information.

Driver %2 has been blocked from
loading.

Driver %2 has been blocked from
loading.

430 / 497


Return value/code

STATUS_DRIVER_BLOCKED

0xC000036D

STATUS_DRIVER_DATABASE_ERROR

0xC000036E

STATUS_SYSTEM_HIVE_TOO_LARGE

0xC000036F

STATUS_INVALID_IMPORT_OF_NON_DLL

0xC0000371

STATUS_NO_SECRETS

0xC0000372

STATUS_ACCESS_DISABLED_NO_SAFER_UI_BY_POLICY

0xC0000373

STATUS_FAILED_STACK_SWITCH

0xC0000374

STATUS_HEAP_CORRUPTION

0xC0000380

STATUS_SMARTCARD_WRONG_PIN

0xC0000381

STATUS_SMARTCARD_CARD_BLOCKED

Description

There was error [%2] processing the
driver database.

System hive size has exceeded its limit.

A dynamic link library (DLL) referenced a
module that was neither a DLL nor the
process's executable image.

The local account store does not contain
secret material for the specified account.

Access to %1 has been restricted by your
Administrator by policy rule %2.

The system was not able to allocate
enough memory to perform a stack
switch.

A heap has been corrupted.

An incorrect PIN was presented to the
smart card.

The smart card is blocked.

0xC0000382

No PIN was presented to the smart card.

STATUS_SMARTCARD_CARD_NOT_AUTHENTICATED

0xC0000383

STATUS_SMARTCARD_NO_CARD

0xC0000384

STATUS_SMARTCARD_NO_KEY_CONTAINER

0xC0000385

STATUS_SMARTCARD_NO_CERTIFICATE

0xC0000386

STATUS_SMARTCARD_NO_KEYSET

0xC0000387

STATUS_SMARTCARD_IO_ERROR

0xC0000388

STATUS_DOWNGRADE_DETECTED

0xC0000389

STATUS_SMARTCARD_CERT_REVOKED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

No smart card is available.

The requested key container does not
exist on the smart card.

The requested certificate does not exist
on the smart card.

The requested keyset does not exist.

A communication error with the smart
card has been detected.

The system detected a possible attempt
to compromise security. Ensure that you
can contact the server that authenticated
you.

The smart card certificate used for
authentication has been revoked. Contact
your system administrator. There might
be additional information in the event
log.

431 / 497


Return value/code

0xC000038A

STATUS_ISSUING_CA_UNTRUSTED

0xC000038B

STATUS_REVOCATION_OFFLINE_C

0xC000038C

STATUS_PKINIT_CLIENT_FAILURE

0xC000038D

STATUS_SMARTCARD_CERT_EXPIRED

0xC000038E

STATUS_DRIVER_FAILED_PRIOR_UNLOAD

0xC000038F

STATUS_SMARTCARD_SILENT_CONTEXT

0xC0000401

STATUS_PER_USER_TRUST_QUOTA_EXCEEDED

0xC0000402

STATUS_ALL_USER_TRUST_QUOTA_EXCEEDED

Description

An untrusted certificate authority was
detected while processing the smart card
certificate that is used for authentication.
Contact your system administrator.

The revocation status of the smart card
certificate that is used for authentication
could not be determined. Contact your
system administrator.

The smart card certificate used for
authentication was not trusted. Contact
your system administrator.

The smart card certificate used for
authentication has expired. Contact your
system administrator.

The driver could not be loaded because a
previous version of the driver is still in
memory.

The smart card provider could not
perform the action because the context
was acquired as silent.

The delegated trust creation quota of the
current user has been exceeded.

The total delegated trust creation quota
has been exceeded.

0xC0000403

STATUS_USER_DELETE_TRUST_QUOTA_EXCEEDED

The delegated trust deletion quota of the
current user has been exceeded.

0xC0000404

STATUS_DS_NAME_NOT_UNIQUE

0xC0000405

STATUS_DS_DUPLICATE_ID_FOUND

0xC0000406

STATUS_DS_GROUP_CONVERSION_ERROR

0xC0000407

STATUS_VOLSNAP_PREPARE_HIBERNATE

0xC0000408

STATUS_USER2USER_REQUIRED

0xC0000409

STATUS_STACK_BUFFER_OVERRUN

0xC000040A

STATUS_NO_S4U_PROT_SUPPORT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The requested name already exists as a
unique identifier.

The requested object has a non-unique
identifier and cannot be retrieved.

The group cannot be converted due to
attribute restrictions on the requested
group type.

{Volume Shadow Copy Service} Wait
while the Volume Shadow Copy Service
prepares volume %hs for hibernation.

Kerberos sub-protocol User2User is
required.

The system detected an overrun of a
stack-based buffer in this application.
This overrun could potentially allow a
malicious user to gain control of this
application.

The Kerberos subsystem encountered an
error. A service for user protocol request
was made against a domain controller

432 / 497


Return value/code

Description

0xC000040B

STATUS_CROSSREALM_DELEGATION_FAILURE

0xC000040C

STATUS_REVOCATION_OFFLINE_KDC

0xC000040D

STATUS_ISSUING_CA_UNTRUSTED_KDC

0xC000040E

STATUS_KDC_CERT_EXPIRED

0xC000040F

STATUS_KDC_CERT_REVOKED

0xC0000410

STATUS_PARAMETER_QUOTA_EXCEEDED

0xC0000411

STATUS_HIBERNATION_FAILURE

0xC0000412

STATUS_DELAY_LOAD_FAILED

0xC0000413

STATUS_AUTHENTICATION_FIREWALL_FAILED

0xC0000414

STATUS_VDM_DISALLOWED

0xC0000415

STATUS_HUNG_DISPLAY_DRIVER_THREAD

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

which does not support service for user.

An attempt was made by this server to
make a Kerberos constrained delegation
request for a target that is outside the
server realm. This action is not supported
and the resulting error indicates a
misconfiguration on the allowed-to-
delegate-to list for this server. Contact
your administrator.

The revocation status of the domain
controller certificate used for smart card
authentication could not be determined.
There is additional information in the
system event log. Contact your system
administrator.

An untrusted certificate authority was
detected while processing the domain
controller certificate used for
authentication. There is additional
information in the system event log.
Contact your system administrator.

The domain controller certificate used for
smart card logon has expired. Contact
your system administrator with the
contents of your system event log.

The domain controller certificate used for
smart card logon has been revoked.
Contact your system administrator with
the contents of your system event log.

Data present in one of the parameters is
more than the function can operate on.

The system has failed to hibernate (The
error code is %hs). Hibernation will be
disabled until the system is restarted.

An attempt to delay-load a .dll or get a
function address in a delay-loaded .dll
failed.

Logon Failure: The machine you are
logging onto is protected by an
authentication firewall. The specified
account is not allowed to authenticate to
the machine.

%hs is a 16-bit application. You do not
have permissions to execute 16-bit
applications. Check your permissions with
your system administrator.

{Display Driver Stopped Responding}
The %hs display driver has stopped
working normally. Save your work and
reboot the system to restore full display
functionality. The next time you reboot
the machine a dialog will be displayed

433 / 497


Return value/code

Description

0xC0000416

STATUS_INSUFFICIENT_RESOURCE_FOR_SPECIFIED_SHARED_SECT
ION_SIZE

0xC0000417

STATUS_INVALID_CRUNTIME_PARAMETER

0xC0000418

STATUS_NTLM_BLOCKED

0xC0000419

STATUS_DS_SRC_SID_EXISTS_IN_FOREST

giving you a chance to report this failure
to Microsoft.

The Desktop heap encountered an error
while allocating session memory. There is
more information in the system event
log.

An invalid parameter was passed to a C
runtime function.

The authentication failed because NTLM
was blocked.

The source object's SID already exists in
destination forest.

0xC000041A

STATUS_DS_DOMAIN_NAME_EXISTS_IN_FOREST

The domain name of the trusted domain
already exists in the forest.

0xC000041B

STATUS_DS_FLAT_NAME_EXISTS_IN_FOREST

0xC000041C

STATUS_INVALID_USER_PRINCIPAL_NAME

0xC0000420

STATUS_ASSERTION_FAILURE

0xC0000421

STATUS_VERIFIER_STOP

0xC0000423

STATUS_CALLBACK_POP_STACK

0xC0000424

STATUS_INCOMPATIBLE_DRIVER_BLOCKED

0xC0000425

STATUS_HIVE_UNLOADED

0xC0000426

STATUS_COMPRESSION_DISABLED

0xC0000427

STATUS_FILE_SYSTEM_LIMITATION

0xC0000428

STATUS_INVALID_IMAGE_HASH

0xC0000429

STATUS_NOT_CAPABLE

0xC000042A

STATUS_REQUEST_OUT_OF_SEQUENCE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The flat name of the trusted domain
already exists in the forest.

The User Principal Name (UPN) is invalid.

There has been an assertion failure.

Application verifier has found an error in
the current process.

A user mode unwind is in progress.

%2 has been blocked from loading due to
incompatibility with this system. Contact
your software vendor for a compatible
version of the driver.

Illegal operation attempted on a registry
key which has already been unloaded.

Compression is disabled for this volume.

The requested operation could not be
completed due to a file system limitation.

The hash for image %hs cannot be found
in the system catalogs. The image is
likely corrupt or the victim of tampering.

The implementation is not capable of
performing the request.

The requested operation is out of order
with respect to other operations.

434 / 497


Return value/code

0xC000042B

STATUS_IMPLEMENTATION_LIMIT

0xC000042C

STATUS_ELEVATION_REQUIRED

0xC000042D

STATUS_NO_SECURITY_CONTEXT

0xC000042E

STATUS_PKU2U_CERT_FAILURE

0xC0000432

STATUS_BEYOND_VDL

0xC0000433

STATUS_ENCOUNTERED_WRITE_IN_PROGRESS

0xC0000434

STATUS_PTE_CHANGED

0xC0000435

STATUS_PURGE_FAILED

0xC0000440

STATUS_CRED_REQUIRES_CONFIRMATION

0xC0000441

STATUS_CS_ENCRYPTION_INVALID_SERVER_RESPONSE

0xC0000442

STATUS_CS_ENCRYPTION_UNSUPPORTED_SERVER

Description

An operation attempted to exceed an
implementation-defined limit.

The requested operation requires
elevation.

The required security context does not
exist.

The PKU2U protocol encountered an error
while attempting to utilize the associated
certificates.

The operation was attempted beyond the
valid data length of the file.

The attempted write operation
encountered a write already in progress
for some portion of the range.

The page fault mappings changed in the
middle of processing a fault so the
operation must be retried.

The attempt to purge this file from
memory failed to purge some or all the
data from memory.

The requested credential requires
confirmation.

The remote server sent an invalid
response for a file being opened with
Client Side Encryption.

Client Side Encryption is not supported
by the remote server even though it
claims to support it.

0xC0000443

STATUS_CS_ENCRYPTION_EXISTING_ENCRYPTED_FILE

File is encrypted and should be opened in
Client Side Encryption mode.

0xC0000444

STATUS_CS_ENCRYPTION_NEW_ENCRYPTED_FILE

A new encrypted file is being created and
a $EFS needs to be provided.

0xC0000445

STATUS_CS_ENCRYPTION_FILE_NOT_CSE

0xC0000446

STATUS_INVALID_LABEL

0xC0000450

STATUS_DRIVER_PROCESS_TERMINATED

0xC0000451

STATUS_AMBIGUOUS_SYSTEM_DEVICE

0xC0000452

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The SMB client requested a CSE FSCTL
on a non-CSE file.

Indicates a particular Security ID cannot
be assigned as the label of an object.

The process hosting the driver for this
device has terminated.

The requested system device cannot be
identified due to multiple
indistinguishable devices potentially
matching the identification criteria.

The requested system device cannot be

435 / 497


Return value/code

STATUS_SYSTEM_DEVICE_NOT_FOUND

0xC0000453

STATUS_RESTART_BOOT_APPLICATION

0xC0000454

STATUS_INSUFFICIENT_NVRAM_RESOURCES

0xC0000460

STATUS_NO_RANGES_PROCESSED

0xC0000463

STATUS_DEVICE_FEATURE_NOT_SUPPORTED

0xC0000464

STATUS_DEVICE_UNREACHABLE

0xC0000465

STATUS_INVALID_TOKEN

0xC0000466

STATUS_SERVER_UNAVAILABLE

0xC0000500

STATUS_INVALID_TASK_NAME

0xC0000501

STATUS_INVALID_TASK_INDEX

0xC0000502

STATUS_THREAD_ALREADY_IN_TASK

0xC0000503

STATUS_CALLBACK_BYPASS

0xC0000602

STATUS_FAIL_FAST_EXCEPTION

0xC0000603

STATUS_IMAGE_CERT_REVOKED

0xC0000700

STATUS_PORT_CLOSED

0xC0000701

STATUS_MESSAGE_LOST

0xC0000702

STATUS_INVALID_MESSAGE

0xC0000703

STATUS_REQUEST_CANCELED

0xC0000704

STATUS_RECURSIVE_DISPATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

found.

This boot application must be restarted.

Insufficient NVRAM resources exist to
complete the API.  A reboot might be
required.

No ranges for the specified operation
were able to be processed.

The storage device does not support
Offload Write.

Data cannot be moved because the
source device cannot communicate with
the destination device.

The token representing the data is invalid
or expired.

The file server is temporarily unavailable.

The specified task name is invalid.

The specified task index is invalid.

The specified thread is already joining a
task.

A callback has requested to bypass native
code.

A fail fast exception occurred. Exception
handlers will not be invoked and the
process will be terminated immediately.

Windows cannot verify the digital
signature for this file. The signing
certificate for this file has been revoked.

The ALPC port is closed.

The ALPC message requested is no longer
available.

The ALPC message supplied is invalid.

The ALPC message has been canceled.

Invalid recursive dispatch attempt.

436 / 497


Return value/code

0xC0000705

STATUS_LPC_RECEIVE_BUFFER_EXPECTED

0xC0000706

STATUS_LPC_INVALID_CONNECTION_USAGE

0xC0000707

STATUS_LPC_REQUESTS_NOT_ALLOWED

0xC0000708

STATUS_RESOURCE_IN_USE

0xC0000709

STATUS_HARDWARE_MEMORY_ERROR

0xC000070A

STATUS_THREADPOOL_HANDLE_EXCEPTION

0xC000070B

STATUS_THREADPOOL_SET_EVENT_ON_COMPLETION_FAILED

0xC000070C

STATUS_THREADPOOL_RELEASE_SEMAPHORE_ON_COMPLETION_FA
ILED

0xC000070D

STATUS_THREADPOOL_RELEASE_MUTEX_ON_COMPLETION_FAILED

0xC000070E

STATUS_THREADPOOL_FREE_LIBRARY_ON_COMPLETION_FAILED

0xC000070F

STATUS_THREADPOOL_RELEASED_DURING_OPERATION

0xC0000710

STATUS_CALLBACK_RETURNED_WHILE_IMPERSONATING

0xC0000711

STATUS_APC_RETURNED_WHILE_IMPERSONATING

0xC0000712

STATUS_PROCESS_IS_PROTECTED

0xC0000713

STATUS_MCA_EXCEPTION

0xC0000714

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

No receive buffer has been supplied in a
synchronous request.

The connection port is used in an invalid
context.

The ALPC port does not accept new
request messages.

The resource requested is already in use.

The hardware has reported an
uncorrectable memory error.

Status 0x%08x was returned, waiting on
handle 0x%x for wait 0x%p, in waiter
0x%p.

After a callback to 0x%p(0x%p), a
completion call to Set event(0x%p) failed
with status 0x%08x.

After a callback to 0x%p(0x%p), a
completion call to
ReleaseSemaphore(0x%p, %d) failed
with status 0x%08x.

After a callback to 0x%p(0x%p), a
completion call to ReleaseMutex(%p)
failed with status 0x%08x.

After a callback to 0x%p(0x%p), a
completion call to FreeLibrary(%p) failed
with status 0x%08x.

The thread pool 0x%p was released while
a thread was posting a callback to
0x%p(0x%p) to it.

A thread pool worker thread is
impersonating a client, after a callback to
0x%p(0x%p). This is unexpected,
indicating that the callback is missing a
call to revert the impersonation.

A thread pool worker thread is
impersonating a client, after executing an
APC. This is unexpected, indicating that
the APC is missing a call to revert the
impersonation.

Either the target process, or the target
thread's containing process, is a
protected process.

A thread is getting dispatched with MCA
EXCEPTION because of MCA.

The client certificate account mapping is
not unique.

437 / 497


Return value/code

Description

STATUS_CERTIFICATE_MAPPING_NOT_UNIQUE

0xC0000715

STATUS_SYMLINK_CLASS_DISABLED

0xC0000716

STATUS_INVALID_IDN_NORMALIZATION

0xC0000717

STATUS_NO_UNICODE_TRANSLATION

0xC0000718

STATUS_ALREADY_REGISTERED

0xC0000719

STATUS_CONTEXT_MISMATCH

0xC000071A

STATUS_PORT_ALREADY_HAS_COMPLETION_LIST

0xC000071B

STATUS_CALLBACK_RETURNED_THREAD_PRIORITY

0xC000071C

STATUS_INVALID_THREAD

0xC000071D

STATUS_CALLBACK_RETURNED_TRANSACTION

0xC000071E

STATUS_CALLBACK_RETURNED_LDR_LOCK

0xC000071F

STATUS_CALLBACK_RETURNED_LANG

0xC0000720

STATUS_CALLBACK_RETURNED_PRI_BACK

0xC0000800

STATUS_DISK_REPAIR_DISABLED

0xC0000801

STATUS_DS_DOMAIN_RENAME_IN_PROGRESS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The symbolic link cannot be followed
because its type is disabled.

Indicates that the specified string is not
valid for IDN normalization.

No mapping for the Unicode character
exists in the target multi-byte code page.

The provided callback is already
registered.

The provided context did not match the
target.

The specified port already has a
completion list.

A threadpool worker thread entered a
callback at thread base priority 0x%x and
exited at priority 0x%x.

This is unexpected, indicating that the
callback missed restoring the priority.

An invalid thread, handle %p, is specified
for this operation. Possibly, a threadpool
worker thread was specified.

A threadpool worker thread entered a
callback, which left transaction state.

This is unexpected, indicating that the
callback missed clearing the transaction.

A threadpool worker thread entered a
callback, which left the loader lock held.

This is unexpected, indicating that the
callback missed releasing the lock.

A threadpool worker thread entered a
callback, which left with preferred
languages set.

This is unexpected, indicating that the
callback missed clearing them.

A threadpool worker thread entered a
callback, which left with background
priorities set.

This is unexpected, indicating that the
callback missed restoring the original
priorities.

The attempted operation required self
healing to be enabled.

The directory service cannot perform the
requested operation because a domain
rename operation is in progress.

438 / 497


Return value/code

0xC0000802

STATUS_DISK_QUOTA_EXCEEDED

0xC0000804

STATUS_CONTENT_BLOCKED

0xC0000805

STATUS_BAD_CLUSTERS

0xC0000806

STATUS_VOLUME_DIRTY

0xC0000901

STATUS_FILE_CHECKED_OUT

0xC0000902

STATUS_CHECKOUT_REQUIRED

0xC0000903

STATUS_BAD_FILE_TYPE

0xC0000904

STATUS_FILE_TOO_LARGE

0xC0000905

STATUS_FORMS_AUTH_REQUIRED

0xC0000906

STATUS_VIRUS_INFECTED

0xC0000907

STATUS_VIRUS_DELETED

0xC0000908

STATUS_BAD_MCFG_TABLE

0xC000090B

STATUS_BAD_DATA

0xC0000909

STATUS_CANNOT_BREAK_OPLOCK

0xC0009898

STATUS_WOW_ASSERTION

0xC000A000

STATUS_INVALID_SIGNATURE

0xC000A001

STATUS_HMAC_NOT_SUPPORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An operation failed because the storage
quota was exceeded.

An operation failed because the content
was blocked.

The operation could not be completed
due to bad clusters on disk.

The operation could not be completed
because the volume is dirty. Please run
the Chkdsk utility and try again.

This file is checked out or locked for
editing by another user.

The file must be checked out before
saving changes.

The file type being saved or retrieved has
been blocked.

The file size exceeds the limit allowed
and cannot be saved.

Access Denied. Before opening files in
this location, you must first browse to the
e.g. site and select the option to log on
automatically.

The operation did not complete
successfully because the file contains a
virus.

This file contains a virus and cannot be
opened. Due to the nature of this virus,
the file has been removed from this
location.

The resources required for this device
conflict with the MCFG table.

Bad data.

The operation did not complete
successfully because it would cause an
oplock to be broken. The caller has
requested that existing oplocks not be
broken.

WOW Assertion Error.

The cryptographic signature is invalid.

The cryptographic provider does not
support HMAC.

439 / 497


Return value/code

0xC000A002

STATUS_AUTH_TAG_MISMATCH

0xC000A010

STATUS_IPSEC_QUEUE_OVERFLOW

0xC000A011

STATUS_ND_QUEUE_OVERFLOW

0xC000A012

STATUS_HOPLIMIT_EXCEEDED

0xC000A013

STATUS_PROTOCOL_NOT_SUPPORTED

0xC000A080

STATUS_LOST_WRITEBEHIND_DATA_NETWORK_DISCONNECTED

0xC000A081

STATUS_LOST_WRITEBEHIND_DATA_NETWORK_SERVER_ERROR

0xC000A082

STATUS_LOST_WRITEBEHIND_DATA_LOCAL_DISK_ERROR

0xC000A083

STATUS_XML_PARSE_ERROR

0xC000A084

STATUS_XMLDSIG_ERROR

0xC000A085

STATUS_WRONG_COMPARTMENT

0xC000A086

STATUS_AUTHIP_FAILURE

0xC000A087

STATUS_DS_OID_MAPPED_GROUP_CANT_HAVE_MEMBERS

0xC000A088

STATUS_DS_OID_NOT_FOUND

0xC000A100

STATUS_HASH_NOT_SUPPORTED

0xC000A101

STATUS_HASH_NOT_PRESENT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The computed authentication tag did not
match the input authentication tag.

The IPsec queue overflowed.

The neighbor discovery queue
overflowed.

An Internet Control Message Protocol
(ICMP) hop limit exceeded error was
received.

The protocol is not installed on the local
machine.

{Delayed Write Failed} Windows was
unable to save all the data for the file
%hs; the data has been lost. This error
might be caused by network connectivity
issues. Try to save this file elsewhere.

{Delayed Write Failed} Windows was
unable to save all the data for the file
%hs; the data has been lost. This error
was returned by the server on which the
file exists. Try to save this file elsewhere.

{Delayed Write Failed} Windows was
unable to save all the data for the file
%hs; the data has been lost. This error
might be caused if the device has been
removed or the media is write-protected.

Windows was unable to parse the
requested XML data.

An error was encountered while
processing an XML digital signature.

This indicates that the caller made the
connection request in the wrong routing
compartment.

This indicates that there was an AuthIP
failure when attempting to connect to the
remote host.

OID mapped groups cannot have
members.

The specified OID cannot be found.

Hash generation for the specified version
and hash type is not enabled on server.

The hash requests is not present or not
up to date with the current file contents.

440 / 497


Return value/code

0xC000A2A1

STATUS_OFFLOAD_READ_FLT_NOT_SUPPORTED

0xC000A2A2

STATUS_OFFLOAD_WRITE_FLT_NOT_SUPPORTED

Description

A file system filter on the server has not
opted in for Offload Read support.

A file system filter on the server has not
opted in for Offload Write support.

0xC000A2A3

STATUS_OFFLOAD_READ_FILE_NOT_SUPPORTED

Offload read operations cannot be
performed on:









Compressed files

Sparse files

Encrypted files

File system metadata files

0xC000A2A4

STATUS_OFFLOAD_WRITE_FILE_NOT_SUPPORTED

Offload write operations cannot be
performed on:









Compressed files

Sparse files

Encrypted files

File system metadata files

The debugger did not perform a state
change.

The debugger found that the application
is not idle.

The string binding is invalid.

The binding handle is not the correct
type.

The binding handle is invalid.

The RPC protocol sequence is not
supported.

The RPC protocol sequence is invalid.

The string UUID is invalid.

The endpoint format is invalid.

The network address is invalid.

441 / 497

0xC0010001

DBG_NO_STATE_CHANGE

0xC0010002

DBG_APP_NOT_IDLE

0xC0020001

RPC_NT_INVALID_STRING_BINDING

0xC0020002

RPC_NT_WRONG_KIND_OF_BINDING

0xC0020003

RPC_NT_INVALID_BINDING

0xC0020004

RPC_NT_PROTSEQ_NOT_SUPPORTED

0xC0020005

RPC_NT_INVALID_RPC_PROTSEQ

0xC0020006

RPC_NT_INVALID_STRING_UUID

0xC0020007

RPC_NT_INVALID_ENDPOINT_FORMAT

0xC0020008

RPC_NT_INVALID_NET_ADDR

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC0020009

RPC_NT_NO_ENDPOINT_FOUND

0xC002000A

RPC_NT_INVALID_TIMEOUT

0xC002000B

RPC_NT_OBJECT_NOT_FOUND

0xC002000C

RPC_NT_ALREADY_REGISTERED

0xC002000D

RPC_NT_TYPE_ALREADY_REGISTERED

0xC002000E

RPC_NT_ALREADY_LISTENING

0xC002000F

RPC_NT_NO_PROTSEQS_REGISTERED

0xC0020010

RPC_NT_NOT_LISTENING

0xC0020011

RPC_NT_UNKNOWN_MGR_TYPE

0xC0020012

RPC_NT_UNKNOWN_IF

0xC0020013

RPC_NT_NO_BINDINGS

0xC0020014

RPC_NT_NO_PROTSEQS

0xC0020015

RPC_NT_CANT_CREATE_ENDPOINT

0xC0020016

RPC_NT_OUT_OF_RESOURCES

0xC0020017

RPC_NT_SERVER_UNAVAILABLE

0xC0020018

RPC_NT_SERVER_TOO_BUSY

0xC0020019

RPC_NT_INVALID_NETWORK_OPTIONS

0xC002001A

RPC_NT_NO_CALL_ACTIVE

0xC002001B

RPC_NT_CALL_FAILED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

No endpoint was found.

The time-out value is invalid.

The object UUID was not found.

The object UUID has already been
registered.

The type UUID has already been
registered.

The RPC server is already listening.

No protocol sequences have been
registered.

The RPC server is not listening.

The manager type is unknown.

The interface is unknown.

There are no bindings.

There are no protocol sequences.

The endpoint cannot be created.

Insufficient resources are available to
complete this operation.

The RPC server is unavailable.

The RPC server is too busy to complete
this operation.

The network options are invalid.

No RPCs are active on this thread.

The RPC failed.

442 / 497


Return value/code

0xC002001C

RPC_NT_CALL_FAILED_DNE

0xC002001D

RPC_NT_PROTOCOL_ERROR

0xC002001F

RPC_NT_UNSUPPORTED_TRANS_SYN

0xC0020021

RPC_NT_UNSUPPORTED_TYPE

0xC0020022

RPC_NT_INVALID_TAG

0xC0020023

RPC_NT_INVALID_BOUND

0xC0020024

RPC_NT_NO_ENTRY_NAME

0xC0020025

RPC_NT_INVALID_NAME_SYNTAX

0xC0020026

RPC_NT_UNSUPPORTED_NAME_SYNTAX

0xC0020028

RPC_NT_UUID_NO_ADDRESS

0xC0020029

RPC_NT_DUPLICATE_ENDPOINT

0xC002002A

RPC_NT_UNKNOWN_AUTHN_TYPE

0xC002002B

RPC_NT_MAX_CALLS_TOO_SMALL

0xC002002C

RPC_NT_STRING_TOO_LONG

0xC002002D

RPC_NT_PROTSEQ_NOT_FOUND

0xC002002E

RPC_NT_PROCNUM_OUT_OF_RANGE

0xC002002F

RPC_NT_BINDING_HAS_NO_AUTH

0xC0020030

RPC_NT_UNKNOWN_AUTHN_SERVICE

0xC0020031

RPC_NT_UNKNOWN_AUTHN_LEVEL

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The RPC failed and did not execute.

An RPC protocol error occurred.

The RPC server does not support the
transfer syntax.

The type UUID is not supported.

The tag is invalid.

The array bounds are invalid.

The binding does not contain an entry
name.

The name syntax is invalid.

The name syntax is not supported.

No network address is available to
construct a UUID.

The endpoint is a duplicate.

The authentication type is unknown.

The maximum number of calls is too
small.

The string is too long.

The RPC protocol sequence was not
found.

The procedure number is out of range.

The binding does not contain any
authentication information.

The authentication service is unknown.

The authentication level is unknown.

443 / 497


Return value/code

0xC0020032

RPC_NT_INVALID_AUTH_IDENTITY

0xC0020033

RPC_NT_UNKNOWN_AUTHZ_SERVICE

0xC0020034

EPT_NT_INVALID_ENTRY

0xC0020035

EPT_NT_CANT_PERFORM_OP

0xC0020036

EPT_NT_NOT_REGISTERED

0xC0020037

RPC_NT_NOTHING_TO_EXPORT

0xC0020038

RPC_NT_INCOMPLETE_NAME

0xC0020039

RPC_NT_INVALID_VERS_OPTION

0xC002003A

RPC_NT_NO_MORE_MEMBERS

0xC002003B

RPC_NT_NOT_ALL_OBJS_UNEXPORTED

0xC002003C

RPC_NT_INTERFACE_NOT_FOUND

0xC002003D

RPC_NT_ENTRY_ALREADY_EXISTS

0xC002003E

RPC_NT_ENTRY_NOT_FOUND

0xC002003F

RPC_NT_NAME_SERVICE_UNAVAILABLE

0xC0020040

RPC_NT_INVALID_NAF_ID

0xC0020041

RPC_NT_CANNOT_SUPPORT

0xC0020042

RPC_NT_NO_CONTEXT_AVAILABLE

0xC0020043

RPC_NT_INTERNAL_ERROR

0xC0020044

RPC_NT_ZERO_DIVIDE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The security context is invalid.

The authorization service is unknown.

The entry is invalid.

The operation cannot be performed.

No more endpoints are available from the
endpoint mapper.

No interfaces have been exported.

The entry name is incomplete.

The version option is invalid.

There are no more members.

There is nothing to unexport.

The interface was not found.

The entry already exists.

The entry was not found.

The name service is unavailable.

The network address family is invalid.

The requested operation is not
supported.

No security context is available to allow
impersonation.

An internal error occurred in the RPC.

The RPC server attempted to divide an
integer by zero.

444 / 497


Return value/code

0xC0020045

RPC_NT_ADDRESS_ERROR

0xC0020046

RPC_NT_FP_DIV_ZERO

0xC0020047

RPC_NT_FP_UNDERFLOW

0xC0020048

RPC_NT_FP_OVERFLOW

0xC0020049

RPC_NT_CALL_IN_PROGRESS

0xC002004A

RPC_NT_NO_MORE_BINDINGS

0xC002004B

RPC_NT_GROUP_MEMBER_NOT_FOUND

0xC002004C

EPT_NT_CANT_CREATE

0xC002004D

RPC_NT_INVALID_OBJECT

0xC002004F

RPC_NT_NO_INTERFACES

0xC0020050

RPC_NT_CALL_CANCELLED

0xC0020051

RPC_NT_BINDING_INCOMPLETE

0xC0020052

RPC_NT_COMM_FAILURE

0xC0020053

RPC_NT_UNSUPPORTED_AUTHN_LEVEL

0xC0020054

RPC_NT_NO_PRINC_NAME

0xC0020055

RPC_NT_NOT_RPC_ERROR

0xC0020057

RPC_NT_SEC_PKG_ERROR

0xC0020058

RPC_NT_NOT_CANCELLED

0xC0020062

RPC_NT_INVALID_ASYNC_HANDLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

An addressing error occurred in the RPC
server.

A floating point operation at the RPC
server caused a divide by zero.

A floating point underflow occurred at the
RPC server.

A floating point overflow occurred at the
RPC server.

An RPC is already in progress for this
thread.

There are no more bindings.

The group member was not found.

The endpoint mapper database entry
could not be created.

The object UUID is the nil UUID.

No interfaces have been registered.

The RPC was canceled.

The binding handle does not contain all
the required information.

A communications failure occurred during
an RPC.

The requested authentication level is not
supported.

No principal name was registered.

The error specified is not a valid Windows
RPC error code.

A security package-specific error
occurred.

The thread was not canceled.

Invalid asynchronous RPC handle.

445 / 497


Return value/code

0xC0020063

RPC_NT_INVALID_ASYNC_CALL

0xC0020064

RPC_NT_PROXY_ACCESS_DENIED

0xC0030001

RPC_NT_NO_MORE_ENTRIES

0xC0030002

RPC_NT_SS_CHAR_TRANS_OPEN_FAIL

0xC0030003

RPC_NT_SS_CHAR_TRANS_SHORT_FILE

0xC0030004

RPC_NT_SS_IN_NULL_CONTEXT

0xC0030005

RPC_NT_SS_CONTEXT_MISMATCH

0xC0030006

RPC_NT_SS_CONTEXT_DAMAGED

0xC0030007

RPC_NT_SS_HANDLES_MISMATCH

0xC0030008

RPC_NT_SS_CANNOT_GET_CALL_HANDLE

0xC0030009

RPC_NT_NULL_REF_POINTER

0xC003000A

RPC_NT_ENUM_VALUE_OUT_OF_RANGE

0xC003000B

RPC_NT_BYTE_COUNT_TOO_SMALL

0xC003000C

RPC_NT_BAD_STUB_DATA

0xC0030059

RPC_NT_INVALID_ES_ACTION

0xC003005A

RPC_NT_WRONG_ES_VERSION

0xC003005B

RPC_NT_WRONG_STUB_VERSION

0xC003005C

RPC_NT_INVALID_PIPE_OBJECT

0xC003005D

RPC_NT_INVALID_PIPE_OPERATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Invalid asynchronous RPC call handle for
this operation.

Access to the HTTP proxy is denied.

The list of RPC servers available for auto-
handle binding has been exhausted.

The file designated by
DCERPCCHARTRANS cannot be opened.

The file containing the character
translation table has fewer than 512
bytes.

A null context handle is passed as an [in]
parameter.

The context handle does not match any
known context handles.

The context handle changed during a call.

The binding handles passed to an RPC do
not match.

The stub is unable to get the call handle.

A null reference pointer was passed to
the stub.

The enumeration value is out of range.

The byte count is too small.

The stub received bad data.

Invalid operation on the
encoding/decoding handle.

Incompatible version of the serializing
package.

Incompatible version of the RPC stub.

The RPC pipe object is invalid or corrupt.

An invalid operation was attempted on an
RPC pipe object.

446 / 497


Return value/code

0xC003005E

RPC_NT_WRONG_PIPE_VERSION

0xC003005F

RPC_NT_PIPE_CLOSED

0xC0030060

RPC_NT_PIPE_DISCIPLINE_ERROR

0xC0030061

RPC_NT_PIPE_EMPTY

0xC0040035

STATUS_PNP_BAD_MPS_TABLE

0xC0040036

STATUS_PNP_TRANSLATION_FAILED

0xC0040037

STATUS_PNP_IRQ_TRANSLATION_FAILED

0xC0040038

STATUS_PNP_INVALID_ID

0xC0040039

STATUS_IO_REISSUE_AS_CACHED

0xC00A0001

STATUS_CTX_WINSTATION_NAME_INVALID

0xC00A0002

STATUS_CTX_INVALID_PD

0xC00A0003

STATUS_CTX_PD_NOT_FOUND

0xC00A0006

STATUS_CTX_CLOSE_PENDING

0xC00A0007

STATUS_CTX_NO_OUTBUF

0xC00A0008

STATUS_CTX_MODEM_INF_NOT_FOUND

0xC00A0009

STATUS_CTX_INVALID_MODEMNAME

0xC00A000A

STATUS_CTX_RESPONSE_ERROR

0xC00A000B

STATUS_CTX_MODEM_RESPONSE_TIMEOUT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Unsupported RPC pipe version.

The RPC pipe object has already been
closed.

The RPC call completed before all pipes
were processed.

No more data is available from the RPC
pipe.

A device is missing in the system BIOS
MPS table. This device will not be used.
Contact your system vendor for a system
BIOS update.

A translator failed to translate resources.

An IRQ translator failed to translate
resources.

Driver %2 returned an invalid ID for a
child device (%3).

Reissue the given operation as a cached
I/O operation

Session name %1 is invalid.

The protocol driver %1 is invalid.

The protocol driver %1 was not found in
the system path.

A close operation is pending on the
terminal connection.

No free output buffers are available.

The MODEM.INF file was not found.

The modem (%1) was not found in the
MODEM.INF file.

The modem did not accept the command
sent to it. Verify that the configured
modem name matches the attached
modem.

The modem did not respond to the
command sent to it. Verify that the
modem cable is properly attached and

447 / 497


Return value/code

0xC00A000C

STATUS_CTX_MODEM_RESPONSE_NO_CARRIER

0xC00A000D

STATUS_CTX_MODEM_RESPONSE_NO_DIALTONE

0xC00A000E

STATUS_CTX_MODEM_RESPONSE_BUSY

0xC00A000F

STATUS_CTX_MODEM_RESPONSE_VOICE

0xC00A0010

STATUS_CTX_TD_ERROR

0xC00A0012

STATUS_CTX_LICENSE_CLIENT_INVALID

0xC00A0013

STATUS_CTX_LICENSE_NOT_AVAILABLE

0xC00A0014

STATUS_CTX_LICENSE_EXPIRED

0xC00A0015

STATUS_CTX_WINSTATION_NOT_FOUND

Description

the modem is turned on.

Carrier detection has failed or the carrier
has been dropped due to disconnection.

A dial tone was not detected within the
required time. Verify that the phone
cable is properly attached and functional.

A busy signal was detected at a remote
site on callback.

A voice was detected at a remote site on
callback.

Transport driver error.

The client you are using is not licensed to
use this system. Your logon request is
denied.

The system has reached its licensed
logon limit. Try again later.

The system license has expired. Your
logon request is denied.

The specified session cannot be found.

0xC00A0016

STATUS_CTX_WINSTATION_NAME_COLLISION

The specified session name is already in
use.

0xC00A0017

STATUS_CTX_WINSTATION_BUSY

0xC00A0018

STATUS_CTX_BAD_VIDEO_MODE

0xC00A0022

STATUS_CTX_GRAPHICS_INVALID

0xC00A0024

STATUS_CTX_NOT_CONSOLE

0xC00A0026

STATUS_CTX_CLIENT_QUERY_TIMEOUT

0xC00A0027

STATUS_CTX_CONSOLE_DISCONNECT

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The requested operation cannot be
completed because the terminal
connection is currently processing a
connect, disconnect, reset, or delete
operation.

An attempt has been made to connect to
a session whose video mode is not
supported by the current client.

The application attempted to enable DOS
graphics mode. DOS graphics mode is
not supported.

The requested operation can be
performed only on the system console.
This is most often the result of a driver or
system DLL requiring direct console
access.

The client failed to respond to the server
connect message.

Disconnecting the console session is not
supported.

448 / 497


Return value/code

0xC00A0028

STATUS_CTX_CONSOLE_CONNECT

0xC00A002A

STATUS_CTX_SHADOW_DENIED

0xC00A002B

STATUS_CTX_WINSTATION_ACCESS_DENIED

0xC00A002E

STATUS_CTX_INVALID_WD

0xC00A002F

STATUS_CTX_WD_NOT_FOUND

0xC00A0030

STATUS_CTX_SHADOW_INVALID

0xC00A0031

STATUS_CTX_SHADOW_DISABLED

0xC00A0032

STATUS_RDP_PROTOCOL_ERROR

0xC00A0033

STATUS_CTX_CLIENT_LICENSE_NOT_SET

0xC00A0034

STATUS_CTX_CLIENT_LICENSE_IN_USE

0xC00A0035

STATUS_CTX_SHADOW_ENDED_BY_MODE_CHANGE

0xC00A0036

STATUS_CTX_SHADOW_NOT_RUNNING

0xC00A0037

STATUS_CTX_LOGON_DISABLED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

Reconnecting a disconnected session to
the console is not supported.

The request to control another session
remotely was denied.

A process has requested access to a
session, but has not been granted those
access rights.

The terminal connection driver %1 is
invalid.

The terminal connection driver %1 was
not found in the system path.

The requested session cannot be
controlled remotely. You cannot control
your own session, a session that is trying
to control your session, a session that
has no user logged on, or other sessions
from the console.

The requested session is not configured
to allow remote control.

The RDP protocol component %2
detected an error in the protocol stream
and has disconnected the client.

Your request to connect to this terminal
server has been rejected. Your terminal
server client license number has not been
entered for this copy of the terminal
client. Contact your system administrator
for help in entering a valid, unique
license number for this terminal server
client. Click OK to continue.

Your request to connect to this terminal
server has been rejected. Your terminal
server client license number is currently
being used by another user. Contact your
system administrator to obtain a new
copy of the terminal server client with a
valid, unique license number. Click OK to
continue.

The remote control of the console was
terminated because the display mode
was changed. Changing the display mode
in a remote control session is not
supported.

Remote control could not be terminated
because the specified session is not
currently being remotely controlled.

Your interactive logon privilege has been
disabled. Contact your system
administrator.

449 / 497


Return value/code

0xC00A0038

STATUS_CTX_SECURITY_LAYER_ERROR

0xC00A0039

STATUS_TS_INCOMPATIBLE_SESSIONS

0xC00B0001

STATUS_MUI_FILE_NOT_FOUND

0xC00B0002

STATUS_MUI_INVALID_FILE

0xC00B0003

STATUS_MUI_INVALID_RC_CONFIG

0xC00B0004

STATUS_MUI_INVALID_LOCALE_NAME

Description

The terminal server security layer
detected an error in the protocol stream
and has disconnected the client.

The target session is incompatible with
the current session.

The resource loader failed to find an MUI
file.

The resource loader failed to load an MUI
file because the file failed to pass
validation.

The RC manifest is corrupted with
garbage data, is an unsupported version,
or is missing a required item.

The RC manifest has an invalid culture
name.

0xC00B0005

STATUS_MUI_INVALID_ULTIMATEFALLBACK_NAME

The RC manifest has and invalid ultimate
fallback name.

0xC00B0006

STATUS_MUI_FILE_NOT_LOADED

0xC00B0007

STATUS_RESOURCE_ENUM_USER_STOP

0xC0130001

STATUS_CLUSTER_INVALID_NODE

0xC0130002

STATUS_CLUSTER_NODE_EXISTS

0xC0130003

STATUS_CLUSTER_JOIN_IN_PROGRESS

0xC0130004

STATUS_CLUSTER_NODE_NOT_FOUND

0xC0130005

STATUS_CLUSTER_LOCAL_NODE_NOT_FOUND

0xC0130006

STATUS_CLUSTER_NETWORK_EXISTS

0xC0130007

STATUS_CLUSTER_NETWORK_NOT_FOUND

0xC0130008

STATUS_CLUSTER_NETINTERFACE_EXISTS

0xC0130009

STATUS_CLUSTER_NETINTERFACE_NOT_FOUND

The resource loader cache does not have
a loaded MUI entry.

The user stopped resource enumeration.

The cluster node is not valid.

The cluster node already exists.

A node is in the process of joining the
cluster.

The cluster node was not found.

The cluster local node information was
not found.

The cluster network already exists.

The cluster network was not found.

The cluster network interface already
exists.

The cluster network interface was not
found.

0xC013000A

The cluster request is not valid for this

450 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_CLUSTER_INVALID_REQUEST

Description

object.

0xC013000B

The cluster network provider is not valid.

STATUS_CLUSTER_INVALID_NETWORK_PROVIDER

0xC013000C

STATUS_CLUSTER_NODE_DOWN

0xC013000D

STATUS_CLUSTER_NODE_UNREACHABLE

0xC013000E

STATUS_CLUSTER_NODE_NOT_MEMBER

0xC013000F

STATUS_CLUSTER_JOIN_NOT_IN_PROGRESS

0xC0130010

STATUS_CLUSTER_INVALID_NETWORK

0xC0130011

STATUS_CLUSTER_NO_NET_ADAPTERS

0xC0130012

STATUS_CLUSTER_NODE_UP

0xC0130013

STATUS_CLUSTER_NODE_PAUSED

0xC0130014

STATUS_CLUSTER_NODE_NOT_PAUSED

0xC0130015

STATUS_CLUSTER_NO_SECURITY_CONTEXT

0xC0130016

STATUS_CLUSTER_NETWORK_NOT_INTERNAL

0xC0130017

STATUS_CLUSTER_POISONED

0xC0140001

STATUS_ACPI_INVALID_OPCODE

0xC0140002

STATUS_ACPI_STACK_OVERFLOW

0xC0140003

STATUS_ACPI_ASSERT_FAILED

0xC0140004

STATUS_ACPI_INVALID_INDEX

0xC0140005

STATUS_ACPI_INVALID_ARGUMENT

The cluster node is down.

The cluster node is not reachable.

The cluster node is not a member of the
cluster.

A cluster join operation is not in
progress.

The cluster network is not valid.

No network adapters are available.

The cluster node is up.

The cluster node is paused.

The cluster node is not paused.

No cluster security context is available.

The cluster network is not configured for
internal cluster communication.

The cluster node has been poisoned.

An attempt was made to run an invalid
AML opcode.

The AML interpreter stack has
overflowed.

An inconsistent state has occurred.

An attempt was made to access an array
outside its bounds.

A required argument was not specified.

0xC0140006

A fatal error has occurred.

451 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_ACPI_FATAL

0xC0140007

STATUS_ACPI_INVALID_SUPERNAME

0xC0140008

STATUS_ACPI_INVALID_ARGTYPE

0xC0140009

STATUS_ACPI_INVALID_OBJTYPE

0xC014000A

STATUS_ACPI_INVALID_TARGETTYPE

Description

An invalid SuperName was specified.

An argument with an incorrect type was
specified.

An object with an incorrect type was
specified.

A target with an incorrect type was
specified.

0xC014000B

STATUS_ACPI_INCORRECT_ARGUMENT_COUNT

An incorrect number of arguments was
specified.

0xC014000C

STATUS_ACPI_ADDRESS_NOT_MAPPED

0xC014000D

STATUS_ACPI_INVALID_EVENTTYPE

0xC014000E

STATUS_ACPI_HANDLER_COLLISION

0xC014000F

STATUS_ACPI_INVALID_DATA

0xC0140010

STATUS_ACPI_INVALID_REGION

0xC0140011

STATUS_ACPI_INVALID_ACCESS_SIZE

0xC0140012

STATUS_ACPI_ACQUIRE_GLOBAL_LOCK

0xC0140013

STATUS_ACPI_ALREADY_INITIALIZED

0xC0140014

STATUS_ACPI_NOT_INITIALIZED

0xC0140015

STATUS_ACPI_INVALID_MUTEX_LEVEL

0xC0140016

STATUS_ACPI_MUTEX_NOT_OWNED

0xC0140017

STATUS_ACPI_MUTEX_NOT_OWNER

0xC0140018

STATUS_ACPI_RS_ACCESS

0xC0140019

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

An address failed to translate.

An incorrect event type was specified.

A handler for the target already exists.

Invalid data for the target was specified.

An invalid region for the target was
specified.

An attempt was made to access a field
outside the defined range.

The global system lock could not be
acquired.

An attempt was made to reinitialize the
ACPI subsystem.

The ACPI subsystem has not been
initialized.

An incorrect mutex was specified.

The mutex is not currently owned.

An attempt was made to access the
mutex by a process that was not the
owner.

An error occurred during an access to
region space.

An attempt was made to use an incorrect

452 / 497


Return value/code

STATUS_ACPI_INVALID_TABLE

0xC0140020

STATUS_ACPI_REG_HANDLER_FAILED

0xC0140021

STATUS_ACPI_POWER_REQUEST_FAILED

0xC0150001

STATUS_SXS_SECTION_NOT_FOUND

0xC0150002

STATUS_SXS_CANT_GEN_ACTCTX

0xC0150003

STATUS_SXS_INVALID_ACTCTXDATA_FORMAT

0xC0150004

STATUS_SXS_ASSEMBLY_NOT_FOUND

0xC0150005

STATUS_SXS_MANIFEST_FORMAT_ERROR

0xC0150006

STATUS_SXS_MANIFEST_PARSE_ERROR

0xC0150007

STATUS_SXS_ACTIVATION_CONTEXT_DISABLED

0xC0150008

STATUS_SXS_KEY_NOT_FOUND

0xC0150009

STATUS_SXS_VERSION_CONFLICT

0xC015000A

STATUS_SXS_WRONG_SECTION_TYPE

0xC015000B

STATUS_SXS_THREAD_QUERIES_DISABLED

0xC015000C

STATUS_SXS_ASSEMBLY_MISSING

0xC015000E

STATUS_SXS_PROCESS_DEFAULT_ALREADY_SET

0xC015000F

STATUS_SXS_EARLY_DEACTIVATION

0xC0150010

STATUS_SXS_INVALID_DEACTIVATION

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

table.

The registration of an ACPI event failed.

An ACPI power object failed to transition
state.

The requested section is not present in
the activation context.

Windows was unble to process the
application binding information. Refer to
the system event log for further
information.

The application binding data format is
invalid.

The referenced assembly is not installed
on the system.

The manifest file does not begin with the
required tag and format information.

The manifest file contains one or more
syntax errors.

The application attempted to activate a
disabled activation context.

The requested lookup key was not found
in any active activation context.

A component version required by the
application conflicts with another
component version that is already active.

The type requested activation context
section does not match the query API
used.

Lack of system resources has required
isolated activation to be disabled for the
current thread of execution.

The referenced assembly could not be
found.

An attempt to set the process default
activation context failed because the
process default activation context was
already set.

The activation context being deactivated
is not the most recently activated one.

The activation context being deactivated
is not active for the current thread of
execution.

453 / 497


Return value/code

0xC0150011

STATUS_SXS_MULTIPLE_DEACTIVATION

0xC0150012

STATUS_SXS_SYSTEM_DEFAULT_ACTIVATION_CONTEXT_EMPTY

0xC0150013

STATUS_SXS_PROCESS_TERMINATION_REQUESTED

0xC0150014

STATUS_SXS_CORRUPT_ACTIVATION_STACK

0xC0150015

STATUS_SXS_CORRUPTION

0xC0150016

STATUS_SXS_INVALID_IDENTITY_ATTRIBUTE_VALUE

Description

The activation context being deactivated
has already been deactivated.

The activation context of the system
default assembly could not be generated.

A component used by the isolation facility
has requested that the process be
terminated.

The activation context activation stack for
the running thread of execution is
corrupt.

The application isolation metadata for
this process or thread has become
corrupt.

The value of an attribute in an identity is
not within the legal range.

0xC0150017

STATUS_SXS_INVALID_IDENTITY_ATTRIBUTE_NAME

The name of an attribute in an identity is
not within the legal range.

0xC0150018

STATUS_SXS_IDENTITY_DUPLICATE_ATTRIBUTE

An identity contains two definitions for
the same attribute.

0xC0150019

STATUS_SXS_IDENTITY_PARSE_ERROR

0xC015001A

STATUS_SXS_COMPONENT_STORE_CORRUPT

0xC015001B

STATUS_SXS_FILE_HASH_MISMATCH

0xC015001C

STATUS_SXS_MANIFEST_IDENTITY_SAME_BUT_CONTENTS_DIFFER
ENT

0xC015001D

STATUS_SXS_IDENTITIES_DIFFERENT

The identity string is malformed. This
might be due to a trailing comma, more
than two unnamed attributes, a missing
attribute name, or a missing attribute
value.

The component store has become
corrupted.

A component's file does not match the
verification information present in the
component manifest.

The identities of the manifests are
identical, but their contents are different.

The component identities are different.

0xC015001E

The assembly is not a deployment.

STATUS_SXS_ASSEMBLY_IS_NOT_A_DEPLOYMENT

0xC015001F

The file is not a part of the assembly.

STATUS_SXS_FILE_NOT_PART_OF_ASSEMBLY

0xC0150020

STATUS_ADVANCED_INSTALLER_FAILED

0xC0150021

STATUS_XML_ENCODING_MISMATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

An advanced installer failed during setup
or servicing.

The character encoding in the XML
declaration did not match the encoding
used in the document.

454 / 497


Return value/code

0xC0150022

STATUS_SXS_MANIFEST_TOO_BIG

0xC0150023

STATUS_SXS_SETTING_NOT_REGISTERED

Description

The size of the manifest exceeds the
maximum allowed.

The setting is not registered.

0xC0150024

STATUS_SXS_TRANSACTION_CLOSURE_INCOMPLETE

One or more required transaction
members are not present.

0xC0150025

STATUS_SMI_PRIMITIVE_INSTALLER_FAILED

0xC0150026

STATUS_GENERIC_COMMAND_FAILED

0xC0150027

STATUS_SXS_FILE_HASH_MISSING

0xC0190001

STATUS_TRANSACTIONAL_CONFLICT

0xC0190002

STATUS_INVALID_TRANSACTION

0xC0190003

STATUS_TRANSACTION_NOT_ACTIVE

0xC0190004

STATUS_TM_INITIALIZATION_FAILED

0xC0190005

STATUS_RM_NOT_ACTIVE

0xC0190006

STATUS_RM_METADATA_CORRUPT

0xC0190007

STATUS_TRANSACTION_NOT_JOINED

0xC0190008

STATUS_DIRECTORY_NOT_RM

0xC019000A

STATUS_TRANSACTIONS_UNSUPPORTED_REMOTE

0xC019000B

STATUS_LOG_RESIZE_INVALID_SIZE

0xC019000C

STATUS_REMOTE_FILE_VERSION_MISMATCH

0xC019000F

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The SMI primitive installer failed during
setup or servicing.

A generic command executable returned
a result that indicates failure.

A component is missing file verification
information in its manifest.

The function attempted to use a name
that is reserved for use by another
transaction.

The transaction handle associated with
this operation is invalid.

The requested operation was made in the
context of a transaction that is no longer
active.

The transaction manager was unable to
be successfully initialized. Transacted
operations are not supported.

Transaction support within the specified
file system resource manager was not
started or was shut down due to an error.

The metadata of the resource manager
has been corrupted. The resource
manager will not function.

The resource manager attempted to
prepare a transaction that it has not
successfully joined.

The specified directory does not contain a
file system resource manager.

The remote server or share does not
support transacted file operations.

The requested log size for the file system
resource manager is invalid.

The remote server sent mismatching
version number or Fid for a file opened
with transactions.

The resource manager tried to register a
protocol that already exists.

455 / 497


Return value/code

Description

STATUS_CRM_PROTOCOL_ALREADY_EXISTS

0xC0190010

STATUS_TRANSACTION_PROPAGATION_FAILED

0xC0190011

STATUS_CRM_PROTOCOL_NOT_FOUND

0xC0190012

STATUS_TRANSACTION_SUPERIOR_EXISTS

0xC0190013

STATUS_TRANSACTION_REQUEST_NOT_VALID

0xC0190014

STATUS_TRANSACTION_NOT_REQUESTED

0xC0190015

STATUS_TRANSACTION_ALREADY_ABORTED

0xC0190016

STATUS_TRANSACTION_ALREADY_COMMITTED

0xC0190017

STATUS_TRANSACTION_INVALID_MARSHALL_BUFFER

0xC0190018

STATUS_CURRENT_TRANSACTION_NOT_VALID

0xC0190019

STATUS_LOG_GROWTH_FAILED

0xC0190021

STATUS_OBJECT_NO_LONGER_EXISTS

0xC0190022

STATUS_STREAM_MINIVERSION_NOT_FOUND

0xC0190023

STATUS_STREAM_MINIVERSION_NOT_VALID

0xC0190024

STATUS_MINIVERSION_INACCESSIBLE_FROM_SPECIFIED_TRANSAC
TION

The attempt to propagate the transaction
failed.

The requested propagation protocol was
not registered as a CRM.

The transaction object already has a
superior enlistment, and the caller
attempted an operation that would have
created a new superior. Only a single
superior enlistment is allowed.

The requested operation is not valid on
the transaction object in its current state.

The caller has called a response API, but
the response is not expected because the
transaction manager did not issue the
corresponding request to the caller.

It is too late to perform the requested
operation, because the transaction has
already been aborted.

It is too late to perform the requested
operation, because the transaction has
already been committed.

The buffer passed in to
NtPushTransaction or NtPullTransaction is
not in a valid format.

The current transaction context
associated with the thread is not a valid
handle to a transaction object.

An attempt to create space in the
transactional resource manager's log
failed. The failure status has been
recorded in the event log.

The object (file, stream, or link) that
corresponds to the handle has been
deleted by a transaction savepoint
rollback.

The specified file miniversion was not
found for this transacted file open.

The specified file miniversion was found
but has been invalidated. The most likely
cause is a transaction savepoint rollback.

A miniversion can be opened only in the
context of the transaction that created it.

0xC0190025

STATUS_CANT_OPEN_MINIVERSION_WITH_MODIFY_INTENT

It is not possible to open a miniversion
with modify access.

456 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC0190026

STATUS_CANT_CREATE_MORE_STREAM_MINIVERSIONS

0xC0190028

STATUS_HANDLE_NO_LONGER_VALID

0xC0190030

STATUS_LOG_CORRUPTION_DETECTED

0xC0190032

STATUS_RM_DISCONNECTED

0xC0190033

STATUS_ENLISTMENT_NOT_SUPERIOR

0xC0190036

STATUS_FILE_IDENTITY_NOT_PERSISTENT

0xC0190037

STATUS_CANT_BREAK_TRANSACTIONAL_DEPENDENCY

0xC0190038

STATUS_CANT_CROSS_RM_BOUNDARY

0xC0190039

STATUS_TXF_DIR_NOT_EMPTY

0xC019003A

STATUS_INDOUBT_TRANSACTIONS_EXIST

0xC019003B

STATUS_TM_VOLATILE

0xC019003C

STATUS_ROLLBACK_TIMER_EXPIRED

0xC019003D

STATUS_TXF_ATTRIBUTE_CORRUPT

0xC019003E

STATUS_EFS_NOT_ALLOWED_IN_TRANSACTION

0xC019003F

STATUS_TRANSACTIONAL_OPEN_NOT_ALLOWED

0xC0190040

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

It is not possible to create any more
miniversions for this stream.

The handle has been invalidated by a
transaction. The most likely cause is the
presence of memory mapping on a file or
an open handle when the transaction
ended or rolled back to savepoint.

The log data is corrupt.

The transaction outcome is unavailable
because the resource manager
responsible for it is disconnected.

The request was rejected because the
enlistment in question is not a superior
enlistment.

The file cannot be opened in a
transaction because its identity depends
on the outcome of an unresolved
transaction.

The operation cannot be performed
because another transaction is depending
on this property not changing.

The operation would involve a single file
with two transactional resource managers
and is, therefore, not allowed.

The $Txf directory must be empty for this
operation to succeed.

The operation would leave a transactional
resource manager in an inconsistent
state and is therefore not allowed.

The operation could not be completed
because the transaction manager does
not have a log.

A rollback could not be scheduled
because a previously scheduled rollback
has already executed or been queued for
execution.

The transactional metadata attribute on
the file or directory %hs is corrupt and
unreadable.

The encryption operation could not be
completed because a transaction is
active.

This object is not allowed to be opened in
a transaction.

Memory mapping (creating a mapped
section) a remote file under a transaction

457 / 497


Return value/code

Description

STATUS_TRANSACTED_MAPPING_UNSUPPORTED_REMOTE

is not supported.

0xC0190043

STATUS_TRANSACTION_REQUIRED_PROMOTION

0xC0190044

STATUS_CANNOT_EXECUTE_FILE_IN_TRANSACTION

0xC0190045

STATUS_TRANSACTIONS_NOT_FROZEN

0xC0190046

STATUS_TRANSACTION_FREEZE_IN_PROGRESS

0xC0190047

STATUS_NOT_SNAPSHOT_VOLUME

0xC0190048

STATUS_NO_SAVEPOINT_WITH_OPEN_FILES

0xC0190049

STATUS_SPARSE_NOT_ALLOWED_IN_TRANSACTION

0xC019004A

STATUS_TM_IDENTITY_MISMATCH

0xC019004B

STATUS_FLOATED_SECTION

0xC019004C

STATUS_CANNOT_ACCEPT_TRANSACTED_WORK

0xC019004D

STATUS_CANNOT_ABORT_TRANSACTIONS

0xC019004E

STATUS_TRANSACTION_NOT_FOUND

0xC019004F

STATUS_RESOURCEMANAGER_NOT_FOUND

0xC0190050

STATUS_ENLISTMENT_NOT_FOUND

Promotion was required to allow the
resource manager to enlist, but the
transaction was set to disallow it.

This file is open for modification in an
unresolved transaction and can be
opened for execute only by a transacted
reader.

The request to thaw frozen transactions
was ignored because transactions were
not previously frozen.

Transactions cannot be frozen because a
freeze is already in progress.

The target volume is not a snapshot
volume. This operation is valid only on a
volume mounted as a snapshot.

The savepoint operation failed because
files are open on the transaction, which is
not permitted.

The sparse operation could not be
completed because a transaction is active
on the file.

The call to create a transaction manager
object failed because the Tm Identity
that is stored in the log file does not
match the Tm Identity that was passed in
as an argument.

I/O was attempted on a section object
that has been floated as a result of a
transaction ending. There is no valid
data.

The transactional resource manager
cannot currently accept transacted work
due to a transient condition, such as low
resources.

The transactional resource manager had
too many transactions outstanding that
could not be aborted. The transactional
resource manager has been shut down.

The specified transaction was unable to
be opened because it was not found.

The specified resource manager was
unable to be opened because it was not
found.

The specified enlistment was unable to be
opened because it was not found.

0xC0190051

The specified transaction manager was

458 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_TRANSACTIONMANAGER_NOT_FOUND

0xC0190052

STATUS_TRANSACTIONMANAGER_NOT_ONLINE

0xC0190053

STATUS_TRANSACTIONMANAGER_RECOVERY_NAME_COLLISION

0xC0190054

STATUS_TRANSACTION_NOT_ROOT

0xC0190055

STATUS_TRANSACTION_OBJECT_EXPIRED

0xC0190056

STATUS_COMPRESSION_NOT_ALLOWED_IN_TRANSACTION

0xC0190057

STATUS_TRANSACTION_RESPONSE_NOT_ENLISTED

0xC0190058

STATUS_TRANSACTION_RECORD_TOO_LONG

0xC0190059

STATUS_NO_LINK_TRACKING_IN_TRANSACTION

0xC019005A

STATUS_OPERATION_NOT_SUPPORTED_IN_TRANSACTION

0xC019005B

STATUS_TRANSACTION_INTEGRITY_VIOLATED

0xC0190060

STATUS_EXPIRED_HANDLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

unable to be opened because it was not
found.

The specified resource manager was
unable to create an enlistment because
its associated transaction manager is not
online.

The specified transaction manager was
unable to create the objects contained in
its log file in the Ob namespace.
Therefore, the transaction manager was
unable to recover.

The call to create a superior enlistment
on this transaction object could not be
completed because the transaction object
specified for the enlistment is a
subordinate branch of the transaction.
Only the root of the transaction can be
enlisted as a superior.

Because the associated transaction
manager or resource manager has been
closed, the handle is no longer valid.

The compression operation could not be
completed because a transaction is active
on the file.

The specified operation could not be
performed on this superior enlistment
because the enlistment was not created
with the corresponding completion
response in the NotificationMask.

The specified operation could not be
performed because the record to be
logged was too long. This can occur
because either there are too many
enlistments on this transaction or the
combined RecoveryInformation being
logged on behalf of those enlistments is
too long.

The link-tracking operation could not be
completed because a transaction is
active.

This operation cannot be performed in a
transaction.

The kernel transaction manager had to
abort or forget the transaction because it
blocked forward progress.

The handle is no longer properly
associated with its transaction.  It might
have been opened in a transactional
resource manager that was subsequently
forced to restart.  Please close the handle
and open a new one.

459 / 497


Return value/code

0xC0190061

STATUS_TRANSACTION_NOT_ENLISTED

0xC01A0001

STATUS_LOG_SECTOR_INVALID

0xC01A0002

STATUS_LOG_SECTOR_PARITY_INVALID

0xC01A0003

STATUS_LOG_SECTOR_REMAPPED

0xC01A0004

STATUS_LOG_BLOCK_INCOMPLETE

0xC01A0005

STATUS_LOG_INVALID_RANGE

0xC01A0006

STATUS_LOG_BLOCKS_EXHAUSTED

0xC01A0007

STATUS_LOG_READ_CONTEXT_INVALID

0xC01A0008

STATUS_LOG_RESTART_INVALID

0xC01A0009

STATUS_LOG_BLOCK_VERSION

0xC01A000A

STATUS_LOG_BLOCK_INVALID

0xC01A000B

STATUS_LOG_READ_MODE_INVALID

0xC01A000D

STATUS_LOG_METADATA_CORRUPT

0xC01A000E

STATUS_LOG_METADATA_INVALID

0xC01A000F

STATUS_LOG_METADATA_INCONSISTENT

0xC01A0010

STATUS_LOG_RESERVATION_INVALID

0xC01A0011

STATUS_LOG_CANT_DELETE

0xC01A0012

STATUS_LOG_CONTAINER_LIMIT_EXCEEDED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The specified operation could not be
performed because the resource manager
is not enlisted in the transaction.

The log service found an invalid log
sector.

The log service encountered a log sector
with invalid block parity.

The log service encountered a remapped
log sector.

The log service encountered a partial or
incomplete log block.

The log service encountered an attempt
to access data outside the active log
range.

The log service user-log marshaling
buffers are exhausted.

The log service encountered an attempt
to read from a marshaling area with an
invalid read context.

The log service encountered an invalid
log restart area.

The log service encountered an invalid
log block version.

The log service encountered an invalid
log block.

The log service encountered an attempt
to read the log with an invalid read
mode.

The log service encountered a corrupted
metadata file.

The log service encountered a metadata
file that could not be created by the log
file system.

The log service encountered a metadata
file with inconsistent data.

The log service encountered an attempt
to erroneously allocate or dispose
reservation space.

The log service cannot delete the log file
or the file system container.

The log service has reached the
maximum allowable containers allocated

460 / 497


Return value/code

0xC01A0013

STATUS_LOG_START_OF_LOG

0xC01A0014

STATUS_LOG_POLICY_ALREADY_INSTALLED

0xC01A0015

STATUS_LOG_POLICY_NOT_INSTALLED

0xC01A0016

STATUS_LOG_POLICY_INVALID

0xC01A0017

STATUS_LOG_POLICY_CONFLICT

0xC01A0018

STATUS_LOG_PINNED_ARCHIVE_TAIL

0xC01A0019

STATUS_LOG_RECORD_NONEXISTENT

0xC01A001A

STATUS_LOG_RECORDS_RESERVED_INVALID

0xC01A001B

STATUS_LOG_SPACE_RESERVED_INVALID

0xC01A001C

STATUS_LOG_TAIL_INVALID

0xC01A001D

STATUS_LOG_FULL

0xC01A001E

STATUS_LOG_MULTIPLEXED

0xC01A001F

STATUS_LOG_DEDICATED

0xC01A0020

STATUS_LOG_ARCHIVE_NOT_IN_PROGRESS

0xC01A0021

STATUS_LOG_ARCHIVE_IN_PROGRESS

0xC01A0022

STATUS_LOG_EPHEMERAL

0xC01A0023

STATUS_LOG_NOT_ENOUGH_CONTAINERS

0xC01A0024

STATUS_LOG_CLIENT_ALREADY_REGISTERED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

to a log file.

The log service has attempted to read or
write backward past the start of the log.

The log policy could not be installed
because a policy of the same type is
already present.

The log policy in question was not
installed at the time of the request.

The installed set of policies on the log is
invalid.

A policy on the log in question prevented
the operation from completing.

The log space cannot be reclaimed
because the log is pinned by the archive
tail.

The log record is not a record in the log
file.

The number of reserved log records or
the adjustment of the number of
reserved log records is invalid.

The reserved log space or the adjustment
of the log space is invalid.

A new or existing archive tail or the base
of the active log is invalid.

The log space is exhausted.

The log is multiplexed; no direct writes to
the physical log are allowed.

The operation failed because the log is
dedicated.

The operation requires an archive
context.

Log archival is in progress.

The operation requires a nonephemeral
log, but the log is ephemeral.

The log must have at least two containers
before it can be read from or written to.

A log client has already registered on the
stream.

461 / 497


Return value/code

0xC01A0025

STATUS_LOG_CLIENT_NOT_REGISTERED

0xC01A0026

STATUS_LOG_FULL_HANDLER_IN_PROGRESS

0xC01A0027

STATUS_LOG_CONTAINER_READ_FAILED

0xC01A0028

STATUS_LOG_CONTAINER_WRITE_FAILED

0xC01A0029

STATUS_LOG_CONTAINER_OPEN_FAILED

0xC01A002A

STATUS_LOG_CONTAINER_STATE_INVALID

0xC01A002B

STATUS_LOG_STATE_INVALID

0xC01A002C

STATUS_LOG_PINNED

0xC01A002D

STATUS_LOG_METADATA_FLUSH_FAILED

0xC01A002E

STATUS_LOG_INCONSISTENT_SECURITY

0xC01A002F

STATUS_LOG_APPENDED_FLUSH_FAILED

0xC01A0030

STATUS_LOG_PINNED_RESERVATION

0xC01B00EA

STATUS_VIDEO_HUNG_DISPLAY_DRIVER_THREAD

0xC01C0001

STATUS_FLT_NO_HANDLER_DEFINED

0xC01C0002

STATUS_FLT_CONTEXT_ALREADY_DEFINED

Description

A log client has not been registered on
the stream.

A request has already been made to
handle the log full condition.

The log service encountered an error
when attempting to read from a log
container.

The log service encountered an error
when attempting to write to a log
container.

The log service encountered an error
when attempting to open a log container.

The log service encountered an invalid
container state when attempting a
requested action.

The log service is not in the correct state
to perform a requested action.

The log space cannot be reclaimed
because the log is pinned.

The log metadata flush failed.

Security on the log and its containers is
inconsistent.

Records were appended to the log or
reservation changes were made, but the
log could not be flushed.

The log is pinned due to reservation
consuming most of the log space. Free
some reserved records to make space
available.

{Display Driver Stopped Responding}
The %hs display driver has stopped
working normally. Save your work and
reboot the system to restore full display
functionality. The next time you reboot
the computer, a dialog box will allow you
to upload data about this failure to
Microsoft.

A handler was not defined by the filter for
this operation.

A context is already defined for this
object.

0xC01C0003

STATUS_FLT_INVALID_ASYNCHRONOUS_REQUEST

Asynchronous requests are not valid for
this operation.

462 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01C0004

STATUS_FLT_DISALLOW_FAST_IO

0xC01C0005

STATUS_FLT_INVALID_NAME_REQUEST

0xC01C0006

STATUS_FLT_NOT_SAFE_TO_POST_OPERATION

0xC01C0007

STATUS_FLT_NOT_INITIALIZED

0xC01C0008

STATUS_FLT_FILTER_NOT_READY

0xC01C0009

STATUS_FLT_POST_OPERATION_CLEANUP

0xC01C000A

STATUS_FLT_INTERNAL_ERROR

0xC01C000B

STATUS_FLT_DELETING_OBJECT

0xC01C000C

STATUS_FLT_MUST_BE_NONPAGED_POOL

0xC01C000D

STATUS_FLT_DUPLICATE_ENTRY

0xC01C000E

STATUS_FLT_CBDQ_DISABLED

0xC01C000F

STATUS_FLT_DO_NOT_ATTACH

0xC01C0010

STATUS_FLT_DO_NOT_DETACH

Description

This is an internal error code used by the
filter manager to determine if a fast I/O
operation should be forced down the
input/output request packet (IRP) path.
Minifilters should never return this value.

An invalid name request was made. The
name requested cannot be retrieved at
this time.

Posting this operation to a worker thread
for further processing is not safe at this
time because it could lead to a system
deadlock.

The Filter Manager was not initialized
when a filter tried to register. Make sure
that the Filter Manager is loaded as a
driver.

The filter is not ready for attachment to
volumes because it has not finished
initializing (FltStartFiltering has not been
called).

The filter must clean up any operation-
specific context at this time because it is
being removed from the system before
the operation is completed by the lower
drivers.

The Filter Manager had an internal error
from which it cannot recover; therefore,
the operation has failed. This is usually
the result of a filter returning an invalid
value from a pre-operation callback.

The object specified for this action is in
the process of being deleted; therefore,
the action requested cannot be
completed at this time.

A nonpaged pool must be used for this
type of context.

A duplicate handler definition has been
provided for an operation.

The callback data queue has been
disabled.

Do not attach the filter to the volume at
this time.

Do not detach the filter from the volume
at this time.

0xC01C0011

STATUS_FLT_INSTANCE_ALTITUDE_COLLISION

An instance already exists at this altitude
on the volume specified.

0xC01C0012

An instance already exists with this name

463 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_FLT_INSTANCE_NAME_COLLISION

on the volume specified.

0xC01C0013

STATUS_FLT_FILTER_NOT_FOUND

0xC01C0014

STATUS_FLT_VOLUME_NOT_FOUND

0xC01C0015

STATUS_FLT_INSTANCE_NOT_FOUND

The system could not find the filter
specified.

The system could not find the volume
specified.

The system could not find the instance
specified.

0xC01C0016

STATUS_FLT_CONTEXT_ALLOCATION_NOT_FOUND

No registered context allocation definition
was found for the given request.

0xC01C0017

STATUS_FLT_INVALID_CONTEXT_REGISTRATION

An invalid parameter was specified during
context registration.

0xC01C0018

STATUS_FLT_NAME_CACHE_MISS

0xC01C0019

STATUS_FLT_NO_DEVICE_OBJECT

0xC01C001A

STATUS_FLT_VOLUME_ALREADY_MOUNTED

0xC01C001B

STATUS_FLT_ALREADY_ENLISTED

0xC01C001C

STATUS_FLT_CONTEXT_ALREADY_LINKED

0xC01C0020

STATUS_FLT_NO_WAITER_FOR_REPLY

0xC01D0001

STATUS_MONITOR_NO_DESCRIPTOR

The name requested was not found in the
Filter Manager name cache and could not
be retrieved from the file system.

The requested device object does not
exist for the given volume.

The specified volume is already mounted.

The specified transaction context is
already enlisted in a transaction.

The specified context is already attached
to another object.

No waiter is present for the filter's reply
to this message.

A monitor descriptor could not be
obtained.

0xC01D0002

STATUS_MONITOR_UNKNOWN_DESCRIPTOR_FORMAT

This release does not support the format
of the obtained monitor descriptor.

0xC01D0003

STATUS_MONITOR_INVALID_DESCRIPTOR_CHECKSUM

The checksum of the obtained monitor
descriptor is invalid.

0xC01D0004

STATUS_MONITOR_INVALID_STANDARD_TIMING_BLOCK

The monitor descriptor contains an
invalid standard timing block.

0xC01D0005

STATUS_MONITOR_WMI_DATABLOCK_REGISTRATION_FAILED

0xC01D0006

STATUS_MONITOR_INVALID_SERIAL_NUMBER_MONDSC_BLOCK

0xC01D0007

STATUS_MONITOR_INVALID_USER_FRIENDLY_MONDSC_BLOCK

WMI data-block registration failed for one
of the MSMonitorClass WMI subclasses.

The provided monitor descriptor block is
either corrupted or does not contain the
monitor's detailed serial number.

The provided monitor descriptor block is
either corrupted or does not contain the
monitor's user-friendly name.

464 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01D0008

STATUS_MONITOR_NO_MORE_DESCRIPTOR_DATA

0xC01D0009

STATUS_MONITOR_INVALID_DETAILED_TIMING_BLOCK

0xC01D000A

STATUS_MONITOR_INVALID_MANUFACTURE_DATE

Description

There is no monitor descriptor data at the
specified (offset or size) region.

The monitor descriptor contains an
invalid detailed timing block.

Monitor descriptor contains invalid
manufacture date.

0xC01E0000

STATUS_GRAPHICS_NOT_EXCLUSIVE_MODE_OWNER

Exclusive mode ownership is needed to
create an unmanaged primary allocation.

0xC01E0001

STATUS_GRAPHICS_INSUFFICIENT_DMA_BUFFER

0xC01E0002

STATUS_GRAPHICS_INVALID_DISPLAY_ADAPTER

0xC01E0003

STATUS_GRAPHICS_ADAPTER_WAS_RESET

0xC01E0004

STATUS_GRAPHICS_INVALID_DRIVER_MODEL

The driver needs more DMA buffer space
to complete the requested operation.

The specified display adapter handle is
invalid.

The specified display adapter and all of
its state have been reset.

The driver stack does not match the
expected driver model.

0xC01E0005

STATUS_GRAPHICS_PRESENT_MODE_CHANGED

Present happened but ended up into the
changed desktop mode.

0xC01E0006

STATUS_GRAPHICS_PRESENT_OCCLUDED

0xC01E0007

STATUS_GRAPHICS_PRESENT_DENIED

Nothing to present due to desktop
occlusion.

Not able to present due to denial of
desktop access.

0xC01E0008

Not able to present with color conversion.

STATUS_GRAPHICS_CANNOTCOLORCONVERT

0xC01E000B

STATUS_GRAPHICS_PRESENT_REDIRECTION_DISABLED

0xC01E000C

STATUS_GRAPHICS_PRESENT_UNOCCLUDED

0xC01E0100

STATUS_GRAPHICS_NO_VIDEO_MEMORY

0xC01E0101

STATUS_GRAPHICS_CANT_LOCK_MEMORY

0xC01E0102

STATUS_GRAPHICS_ALLOCATION_BUSY

0xC01E0103

STATUS_GRAPHICS_TOO_MANY_REFERENCES

0xC01E0104

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Present redirection is disabled (desktop
windowing management subsystem is
off).

Previous exclusive VidPn source owner
has released its ownership

Not enough video memory is available to
complete the operation.

Could not probe and lock the underlying
memory of an allocation.

The allocation is currently busy.

An object being referenced has already
reached the maximum reference count
and cannot be referenced further.

A problem could not be solved due to an
existing condition. Try again later.

465 / 497


Return value/code

Description

STATUS_GRAPHICS_TRY_AGAIN_LATER

0xC01E0105

STATUS_GRAPHICS_TRY_AGAIN_NOW

0xC01E0106

STATUS_GRAPHICS_ALLOCATION_INVALID

0xC01E0107

STATUS_GRAPHICS_UNSWIZZLING_APERTURE_UNAVAILABLE

0xC01E0108

STATUS_GRAPHICS_UNSWIZZLING_APERTURE_UNSUPPORTED

0xC01E0109

STATUS_GRAPHICS_CANT_EVICT_PINNED_ALLOCATION

0xC01E0110

STATUS_GRAPHICS_INVALID_ALLOCATION_USAGE

0xC01E0111

STATUS_GRAPHICS_CANT_RENDER_LOCKED_ALLOCATION

0xC01E0112

STATUS_GRAPHICS_ALLOCATION_CLOSED

0xC01E0113

STATUS_GRAPHICS_INVALID_ALLOCATION_INSTANCE

0xC01E0114

STATUS_GRAPHICS_INVALID_ALLOCATION_HANDLE

A problem could not be solved due to an
existing condition. Try again now.

The allocation is invalid.

No more unswizzling apertures are
currently available.

The current allocation cannot be
unswizzled by an aperture.

The request failed because a pinned
allocation cannot be evicted.

The allocation cannot be used from its
current segment location for the specified
operation.

A locked allocation cannot be used in the
current command buffer.

The allocation being referenced has been
closed permanently.

An invalid allocation instance is being
referenced.

An invalid allocation handle is being
referenced.

0xC01E0115

STATUS_GRAPHICS_WRONG_ALLOCATION_DEVICE

The allocation being referenced does not
belong to the current device.

0xC01E0116

The specified allocation lost its content.

STATUS_GRAPHICS_ALLOCATION_CONTENT_LOST

0xC01E0200

STATUS_GRAPHICS_GPU_EXCEPTION_ON_DEVICE

A GPU exception was detected on the
given device. The device cannot be
scheduled.

0xC01E0300

The specified VidPN topology is invalid.

STATUS_GRAPHICS_INVALID_VIDPN_TOPOLOGY

0xC01E0301

STATUS_GRAPHICS_VIDPN_TOPOLOGY_NOT_SUPPORTED

0xC01E0302

STATUS_GRAPHICS_VIDPN_TOPOLOGY_CURRENTLY_NOT_SUPPORT
ED

0xC01E0303

STATUS_GRAPHICS_INVALID_VIDPN

0xC01E0304

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The specified VidPN topology is valid but
is not supported by this model of the
display adapter.

The specified VidPN topology is valid but
is not currently supported by the display
adapter due to allocation of its resources.

The specified VidPN handle is invalid.

The specified video present source is
invalid.

466 / 497


Return value/code

Description

STATUS_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE

0xC01E0305

STATUS_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET

The specified video present target is
invalid.

0xC01E0306

STATUS_GRAPHICS_VIDPN_MODALITY_NOT_SUPPORTED

0xC01E0308

STATUS_GRAPHICS_INVALID_VIDPN_SOURCEMODESET

0xC01E0309

STATUS_GRAPHICS_INVALID_VIDPN_TARGETMODESET

0xC01E030A

STATUS_GRAPHICS_INVALID_FREQUENCY

0xC01E030B

STATUS_GRAPHICS_INVALID_ACTIVE_REGION

0xC01E030C

STATUS_GRAPHICS_INVALID_TOTAL_REGION

0xC01E0310

STATUS_GRAPHICS_INVALID_VIDEO_PRESENT_SOURCE_MODE

0xC01E0311

STATUS_GRAPHICS_INVALID_VIDEO_PRESENT_TARGET_MODE

0xC01E0312

STATUS_GRAPHICS_PINNED_MODE_MUST_REMAIN_IN_SET

0xC01E0313

STATUS_GRAPHICS_PATH_ALREADY_IN_TOPOLOGY

0xC01E0314

STATUS_GRAPHICS_MODE_ALREADY_IN_MODESET

0xC01E0315

STATUS_GRAPHICS_INVALID_VIDEOPRESENTSOURCESET

0xC01E0316

STATUS_GRAPHICS_INVALID_VIDEOPRESENTTARGETSET

0xC01E0317

STATUS_GRAPHICS_SOURCE_ALREADY_IN_SET

0xC01E0318

STATUS_GRAPHICS_TARGET_ALREADY_IN_SET

The specified VidPN modality is not
supported (for example, at least two of
the pinned modes are not co-functional).

The specified VidPN source mode set is
invalid.

The specified VidPN target mode set is
invalid.

The specified video signal frequency is
invalid.

The specified video signal active region is
invalid.

The specified video signal total region is
invalid.

The specified video present source mode
is invalid.

The specified video present target mode
is invalid.

The pinned mode must remain in the set
on the VidPN's co-functional modality
enumeration.

The specified video present path is
already in the VidPN's topology.

The specified mode is already in the
mode set.

The specified video present source set is
invalid.

The specified video present target set is
invalid.

The specified video present source is
already in the video present source set.

The specified video present target is
already in the video present target set.

0xC01E0319

STATUS_GRAPHICS_INVALID_VIDPN_PRESENT_PATH

The specified VidPN present path is
invalid.

0xC01E031A

STATUS_GRAPHICS_NO_RECOMMENDED_VIDPN_TOPOLOGY

The miniport has no recommendation for
augmenting the specified VidPN's
topology.

467 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01E031B

STATUS_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGESET

0xC01E031C

STATUS_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGE

0xC01E031D

STATUS_GRAPHICS_FREQUENCYRANGE_NOT_IN_SET

0xC01E031F

STATUS_GRAPHICS_FREQUENCYRANGE_ALREADY_IN_SET

0xC01E0320

STATUS_GRAPHICS_STALE_MODESET

0xC01E0321

STATUS_GRAPHICS_INVALID_MONITOR_SOURCEMODESET

Description

The specified monitor frequency range
set is invalid.

The specified monitor frequency range is
invalid.

The specified frequency range is not in
the specified monitor frequency range
set.

The specified frequency range is already
in the specified monitor frequency range
set.

The specified mode set is stale. Reacquire
the new mode set.

The specified monitor source mode set is
invalid.

0xC01E0322

STATUS_GRAPHICS_INVALID_MONITOR_SOURCE_MODE

The specified monitor source mode is
invalid.

0xC01E0323

STATUS_GRAPHICS_NO_RECOMMENDED_FUNCTIONAL_VIDPN

The miniport does not have a
recommendation regarding the request to
provide a functional VidPN given the
current display adapter configuration.

0xC01E0324

STATUS_GRAPHICS_MODE_ID_MUST_BE_UNIQUE

The ID of the specified mode is being
used by another mode in the set.

0xC01E0325

STATUS_GRAPHICS_EMPTY_ADAPTER_MONITOR_MODE_SUPPORT_I
NTERSECTION

0xC01E0326

STATUS_GRAPHICS_VIDEO_PRESENT_TARGETS_LESS_THAN_SOUR
CES

0xC01E0327

STATUS_GRAPHICS_PATH_NOT_IN_TOPOLOGY

0xC01E0328

STATUS_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_SOURC
E

The system failed to determine a mode
that is supported by both the display
adapter and the monitor connected to it.

The number of video present targets
must be greater than or equal to the
number of video present sources.

The specified present path is not in the
VidPN's topology.

The display adapter must have at least
one video present source.

0xC01E0329

STATUS_GRAPHICS_ADAPTER_MUST_HAVE_AT_LEAST_ONE_TARGE
T

The display adapter must have at least
one video present target.

0xC01E032A

STATUS_GRAPHICS_INVALID_MONITORDESCRIPTORSET

The specified monitor descriptor set is
invalid.

0xC01E032B

STATUS_GRAPHICS_INVALID_MONITORDESCRIPTOR

0xC01E032C

STATUS_GRAPHICS_MONITORDESCRIPTOR_NOT_IN_SET

The specified monitor descriptor is
invalid.

The specified descriptor is not in the
specified monitor descriptor set.

468 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01E032D

STATUS_GRAPHICS_MONITORDESCRIPTOR_ALREADY_IN_SET

0xC01E032E

STATUS_GRAPHICS_MONITORDESCRIPTOR_ID_MUST_BE_UNIQUE

0xC01E032F

STATUS_GRAPHICS_INVALID_VIDPN_TARGET_SUBSET_TYPE

0xC01E0330

STATUS_GRAPHICS_RESOURCES_NOT_RELATED

0xC01E0331

STATUS_GRAPHICS_SOURCE_ID_MUST_BE_UNIQUE

0xC01E0332

STATUS_GRAPHICS_TARGET_ID_MUST_BE_UNIQUE

0xC01E0333

STATUS_GRAPHICS_NO_AVAILABLE_VIDPN_TARGET

0xC01E0334

STATUS_GRAPHICS_MONITOR_COULD_NOT_BE_ASSOCIATED_WITH
_ADAPTER

0xC01E0335

STATUS_GRAPHICS_NO_VIDPNMGR

0xC01E0336

STATUS_GRAPHICS_NO_ACTIVE_VIDPN

0xC01E0337

STATUS_GRAPHICS_STALE_VIDPN_TOPOLOGY

Description

The specified descriptor is already in the
specified monitor descriptor set.

The ID of the specified monitor descriptor
is being used by another descriptor in the
set.

The specified video present target subset
type is invalid.

Two or more of the specified resources
are not related to each other, as defined
by the interface semantics.

The ID of the specified video present
source is being used by another source in
the set.

The ID of the specified video present
target is being used by another target in
the set.

The specified VidPN source cannot be
used because there is no available VidPN
target to connect it to.

The newly arrived monitor could not be
associated with a display adapter.

The particular display adapter does not
have an associated VidPN manager.

The VidPN manager of the particular
display adapter does not have an active
VidPN.

The specified VidPN topology is stale;
obtain the new topology.

0xC01E0338

STATUS_GRAPHICS_MONITOR_NOT_CONNECTED

No monitor is connected on the specified
video present target.

0xC01E0339

STATUS_GRAPHICS_SOURCE_NOT_IN_TOPOLOGY

The specified source is not part of the
specified VidPN's topology.

0xC01E033A

STATUS_GRAPHICS_INVALID_PRIMARYSURFACE_SIZE

The specified primary surface size is
invalid.

0xC01E033B

The specified visible region size is invalid.

STATUS_GRAPHICS_INVALID_VISIBLEREGION_SIZE

0xC01E033C

STATUS_GRAPHICS_INVALID_STRIDE

0xC01E033D

STATUS_GRAPHICS_INVALID_PIXELFORMAT

The specified stride is invalid.

The specified pixel format is invalid.

0xC01E033E

The specified color basis is invalid.

469 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

STATUS_GRAPHICS_INVALID_COLORBASIS

0xC01E033F

STATUS_GRAPHICS_INVALID_PIXELVALUEACCESSMODE

The specified pixel value access mode is
invalid.

0xC01E0340

STATUS_GRAPHICS_TARGET_NOT_IN_TOPOLOGY

The specified target is not part of the
specified VidPN's topology.

0xC01E0341

STATUS_GRAPHICS_NO_DISPLAY_MODE_MANAGEMENT_SUPPORT

0xC01E0342

STATUS_GRAPHICS_VIDPN_SOURCE_IN_USE

0xC01E0343

STATUS_GRAPHICS_CANT_ACCESS_ACTIVE_VIDPN

0xC01E0344

STATUS_GRAPHICS_INVALID_PATH_IMPORTANCE_ORDINAL

0xC01E0345

STATUS_GRAPHICS_INVALID_PATH_CONTENT_GEOMETRY_TRANSF
ORMATION

0xC01E0346

STATUS_GRAPHICS_PATH_CONTENT_GEOMETRY_TRANSFORMATIO
N_NOT_SUPPORTED

Failed to acquire the display mode
management interface.

The specified VidPN source is already
owned by a DMM client and cannot be
used until that client releases it.

The specified VidPN is active and cannot
be accessed.

The specified VidPN's present path
importance ordinal is invalid.

The specified VidPN's present path
content geometry transformation is
invalid.

The specified content geometry
transformation is not supported on the
respective VidPN present path.

0xC01E0347

The specified gamma ramp is invalid.

STATUS_GRAPHICS_INVALID_GAMMA_RAMP

0xC01E0348

STATUS_GRAPHICS_GAMMA_RAMP_NOT_SUPPORTED

0xC01E0349

STATUS_GRAPHICS_MULTISAMPLING_NOT_SUPPORTED

The specified gamma ramp is not
supported on the respective VidPN
present path.

Multisampling is not supported on the
respective VidPN present path.

0xC01E034A

STATUS_GRAPHICS_MODE_NOT_IN_MODESET

The specified mode is not in the specified
mode set.

0xC01E034D

STATUS_GRAPHICS_INVALID_VIDPN_TOPOLOGY_RECOMMENDATIO
N_REASON

The specified VidPN topology
recommendation reason is invalid.

0xC01E034E

STATUS_GRAPHICS_INVALID_PATH_CONTENT_TYPE

The specified VidPN present path content
type is invalid.

0xC01E034F

STATUS_GRAPHICS_INVALID_COPYPROTECTION_TYPE

The specified VidPN present path copy
protection type is invalid.

0xC01E0350

STATUS_GRAPHICS_UNASSIGNED_MODESET_ALREADY_EXISTS

0xC01E0352

STATUS_GRAPHICS_INVALID_SCANLINE_ORDERING

Only one unassigned mode set can exist
at any one time for a particular VidPN
source or target.

The specified scan line ordering type is
invalid.

470 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01E0353

STATUS_GRAPHICS_TOPOLOGY_CHANGES_NOT_ALLOWED

0xC01E0354

STATUS_GRAPHICS_NO_AVAILABLE_IMPORTANCE_ORDINALS

0xC01E0355

STATUS_GRAPHICS_INCOMPATIBLE_PRIVATE_FORMAT

0xC01E0356

STATUS_GRAPHICS_INVALID_MODE_PRUNING_ALGORITHM

0xC01E0357

STATUS_GRAPHICS_INVALID_MONITOR_CAPABILITY_ORIGIN

0xC01E0358

STATUS_GRAPHICS_INVALID_MONITOR_FREQUENCYRANGE_CONST
RAINT

Description

The topology changes are not allowed for
the specified VidPN.

All available importance ordinals are
being used in the specified topology.

The specified primary surface has a
different private-format attribute than
the current primary surface.

The specified mode-pruning algorithm is
invalid.

The specified monitor-capability origin is
invalid.

The specified monitor-frequency range
constraint is invalid.

0xC01E0359

STATUS_GRAPHICS_MAX_NUM_PATHS_REACHED

The maximum supported number of
present paths has been reached.

0xC01E035A

STATUS_GRAPHICS_CANCEL_VIDPN_TOPOLOGY_AUGMENTATION

0xC01E035B

STATUS_GRAPHICS_INVALID_CLIENT_TYPE

0xC01E035C

STATUS_GRAPHICS_CLIENTVIDPN_NOT_SET

0xC01E0400

STATUS_GRAPHICS_SPECIFIED_CHILD_ALREADY_CONNECTED

0xC01E0401

STATUS_GRAPHICS_CHILD_DESCRIPTOR_NOT_SUPPORTED

The miniport requested that
augmentation be canceled for the
specified source of the specified VidPN's
topology.

The specified client type was not
recognized.

The client VidPN is not set on this adapter
(for example, no user mode-initiated
mode changes have taken place on this
adapter).

The specified display adapter child device
already has an external device connected
to it.

The display adapter child device does not
support reporting a descriptor.

0xC01E0430

STATUS_GRAPHICS_NOT_A_LINKED_ADAPTER

The display adapter is not linked to any
other adapters.

0xC01E0431

STATUS_GRAPHICS_LEADLINK_NOT_ENUMERATED

The lead adapter in a linked configuration
was not enumerated yet.

0xC01E0432

STATUS_GRAPHICS_CHAINLINKS_NOT_ENUMERATED

Some chain adapters in a linked
configuration have not yet been
enumerated.

0xC01E0433

STATUS_GRAPHICS_ADAPTER_CHAIN_NOT_READY

The chain of linked adapters is not ready
to start because of an unknown failure.

0xC01E0434

STATUS_GRAPHICS_CHAINLINKS_NOT_STARTED

An attempt was made to start a lead link
display adapter when the chain links had
not yet started.

471 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

0xC01E0435

STATUS_GRAPHICS_CHAINLINKS_NOT_POWERED_ON

0xC01E0436

STATUS_GRAPHICS_INCONSISTENT_DEVICE_LINK_STATE

0xC01E0438

STATUS_GRAPHICS_NOT_POST_DEVICE_DRIVER

0xC01E043B

STATUS_GRAPHICS_ADAPTER_ACCESS_NOT_EXCLUDED

0xC01E0500

STATUS_GRAPHICS_OPM_NOT_SUPPORTED

Description

An attempt was made to turn on a lead
link display adapter when the chain links
were turned off.

The adapter link was found in an
inconsistent state. Not all adapters are in
an expected PNP/power state.

The driver trying to start is not the same
as the driver for the posted display
adapter.

An operation is being attempted that
requires the display adapter to be in a
quiescent state.

The driver does not support OPM.

0xC01E0501

The driver does not support COPP.

STATUS_GRAPHICS_COPP_NOT_SUPPORTED

0xC01E0502

STATUS_GRAPHICS_UAB_NOT_SUPPORTED

0xC01E0503

STATUS_GRAPHICS_OPM_INVALID_ENCRYPTED_PARAMETERS

0xC01E0504

STATUS_GRAPHICS_OPM_PARAMETER_ARRAY_TOO_SMALL

0xC01E0505

STATUS_GRAPHICS_OPM_NO_PROTECTED_OUTPUTS_EXIST

0xC01E0506

STATUS_GRAPHICS_PVP_NO_DISPLAY_DEVICE_CORRESPONDS_TO
_NAME

0xC01E0507

STATUS_GRAPHICS_PVP_DISPLAY_DEVICE_NOT_ATTACHED_TO_DE
SKTOP

0xC01E0508

STATUS_GRAPHICS_PVP_MIRRORING_DEVICES_NOT_SUPPORTED

0xC01E050A

STATUS_GRAPHICS_OPM_INVALID_POINTER

0xC01E050B

STATUS_GRAPHICS_OPM_INTERNAL_ERROR

0xC01E050C

STATUS_GRAPHICS_OPM_INVALID_HANDLE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The driver does not support UAB.

The specified encrypted parameters are
invalid.

An array passed to a function cannot hold
all of the data that the function wants to
put in it.

The GDI display device passed to this
function does not have any active
protected outputs.

The PVP cannot find an actual GDI
display device that corresponds to the
passed-in GDI display device name.

This function failed because the GDI
display device passed to it was not
attached to the Windows desktop.

The PVP does not support mirroring
display devices because they do not have
any protected outputs.

The function failed because an invalid
pointer parameter was passed to it. A
pointer parameter is invalid if it is null, is
not correctly aligned, or it points to an
invalid address or a kernel mode address.

An internal error caused an operation to
fail.

The function failed because the caller
passed in an invalid OPM user-mode
handle.

472 / 497


Return value/code

0xC01E050D

STATUS_GRAPHICS_PVP_NO_MONITORS_CORRESPOND_TO_DISPLA
Y_DEVICE

0xC01E050E

STATUS_GRAPHICS_PVP_INVALID_CERTIFICATE_LENGTH

0xC01E050F

STATUS_GRAPHICS_OPM_SPANNING_MODE_ENABLED

0xC01E0510

STATUS_GRAPHICS_OPM_THEATER_MODE_ENABLED

0xC01E0511

STATUS_GRAPHICS_PVP_HFS_FAILED

0xC01E0512

STATUS_GRAPHICS_OPM_INVALID_SRM

0xC01E0513

STATUS_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_HDCP

0xC01E0514

STATUS_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_ACP

0xC01E0515

STATUS_GRAPHICS_OPM_OUTPUT_DOES_NOT_SUPPORT_CGMSA

0xC01E0516

STATUS_GRAPHICS_OPM_HDCP_SRM_NEVER_SET

0xC01E0517

STATUS_GRAPHICS_OPM_RESOLUTION_TOO_HIGH

0xC01E0518

STATUS_GRAPHICS_OPM_ALL_HDCP_HARDWARE_ALREADY_IN_USE

0xC01E051A

STATUS_GRAPHICS_OPM_PROTECTED_OUTPUT_NO_LONGER_EXIST
S

Description

This function failed because the GDI
device passed to it did not have any
monitors associated with it.

A certificate could not be returned
because the certificate buffer passed to
the function was too small.

DxgkDdiOpmCreateProtectedOutput()
could not create a protected output
because the video present yarget is in
spanning mode.

DxgkDdiOpmCreateProtectedOutput()
could not create a protected output
because the video present target is in
theater mode.

The function call failed because the
display adapter's hardware functionality
scan (HFS) failed to validate the graphics
hardware.

The HDCP SRM passed to this function
did not comply with section 5 of the
HDCP 1.1 specification.

The protected output cannot enable the
HDCP system because it does not support
it.

The protected output cannot enable
analog copy protection because it does
not support it.

The protected output cannot enable the
CGMS-A protection technology because it
does not support it.

DxgkDdiOPMGetInformation() cannot
return the version of the SRM being used
because the application never
successfully passed an SRM to the
protected output.

DxgkDdiOPMConfigureProtectedOutput()
cannot enable the specified output
protection technology because the
output's screen resolution is too high.

DxgkDdiOPMConfigureProtectedOutput()
cannot enable HDCP because other
physical outputs are using the display
adapter's HDCP hardware.

The operating system asynchronously
destroyed this OPM-protected output
because the operating system state
changed. This error typically occurs
because the monitor PDO associated with
this protected output was removed or
stopped, the protected output's session

473 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

Description

0xC01E051B

STATUS_GRAPHICS_OPM_SESSION_TYPE_CHANGE_IN_PROGRESS

0xC01E051C

STATUS_GRAPHICS_OPM_PROTECTED_OUTPUT_DOES_NOT_HAVE_
COPP_SEMANTICS

0xC01E051D

STATUS_GRAPHICS_OPM_INVALID_INFORMATION_REQUEST

0xC01E051E

STATUS_GRAPHICS_OPM_DRIVER_INTERNAL_ERROR

0xC01E051F

STATUS_GRAPHICS_OPM_PROTECTED_OUTPUT_DOES_NOT_HAVE_
OPM_SEMANTICS

0xC01E0520

STATUS_GRAPHICS_OPM_SIGNALING_NOT_SUPPORTED

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

became a nonconsole session, or the
protected output's desktop became
inactive.

OPM functions cannot be called when a
session is changing its type. Three types
of sessions currently exist: console,
disconnected, and remote (RDP or ICA).

The
DxgkDdiOPMGetCOPPCompatibleInformat
ion, DxgkDdiOPMGetInformation, or
DxgkDdiOPMConfigureProtectedOutput
function failed. This error is returned only
if a protected output has OPM semantics.

DxgkDdiOPMGetCOPPCompatibleInformat
ion always returns this error if a
protected output has OPM semantics.

DxgkDdiOPMGetInformation returns this
error code if the caller requested COPP-
specific information.

DxgkDdiOPMConfigureProtectedOutput
returns this error when the caller tries to
use a COPP-specific command.

The DxgkDdiOPMGetInformation and
DxgkDdiOPMGetCOPPCompatibleInformat
ion functions return this error code if the
passed-in sequence number is not the
expected sequence number or the
passed-in OMAC value is invalid.

The function failed because an
unexpected error occurred inside a
display driver.

The
DxgkDdiOPMGetCOPPCompatibleInformat
ion, DxgkDdiOPMGetInformation, or
DxgkDdiOPMConfigureProtectedOutput
function failed. This error is returned only
if a protected output has COPP
semantics.

DxgkDdiOPMGetCOPPCompatibleInformat
ion returns this error code if the caller
requested OPM-specific information.

DxgkDdiOPMGetInformation always
returns this error if a protected output
has COPP semantics.

DxgkDdiOPMConfigureProtectedOutput
returns this error when the caller tries to
use an OPM-specific command.

The
DxgkDdiOPMGetCOPPCompatibleInformat
ion and
DxgkDdiOPMConfigureProtectedOutput
functions return this error if the display
driver does not support the
DXGKMDT_OPM_GET_ACP_AND_CGMSA_
SIGNALING and

474 / 497


Return value/code

Description

0xC01E0521

STATUS_GRAPHICS_OPM_INVALID_CONFIGURATION_REQUEST

0xC01E0580

STATUS_GRAPHICS_I2C_NOT_SUPPORTED

DXGKMDT_OPM_SET_ACP_AND_CGMSA_
SIGNALING GUIDs.

The
DxgkDdiOPMConfigureProtectedOutput
function returns this error code if the
passed-in sequence number is not the
expected sequence number or the
passed-in OMAC value is invalid.

The monitor connected to the specified
video output does not have an I2C bus.

0xC01E0581

STATUS_GRAPHICS_I2C_DEVICE_DOES_NOT_EXIST

No device on the I2C bus has the
specified address.

0xC01E0582

STATUS_GRAPHICS_I2C_ERROR_TRANSMITTING_DATA

An error occurred while transmitting data
to the device on the I2C bus.

0xC01E0583

STATUS_GRAPHICS_I2C_ERROR_RECEIVING_DATA

An error occurred while receiving data
from the device on the I2C bus.

0xC01E0584

STATUS_GRAPHICS_DDCCI_VCP_NOT_SUPPORTED

The monitor does not support the
specified VCP code.

0xC01E0585

STATUS_GRAPHICS_DDCCI_INVALID_DATA

0xC01E0586

STATUS_GRAPHICS_DDCCI_MONITOR_RETURNED_INVALID_TIMING
_STATUS_BYTE

0xC01E0587

STATUS_GRAPHICS_DDCCI_INVALID_CAPABILITIES_STRING

0xC01E0588

STATUS_GRAPHICS_MCA_INTERNAL_ERROR

0xC01E0589

STATUS_GRAPHICS_DDCCI_INVALID_MESSAGE_COMMAND

0xC01E058A

STATUS_GRAPHICS_DDCCI_INVALID_MESSAGE_LENGTH

0xC01E058B

STATUS_GRAPHICS_DDCCI_INVALID_MESSAGE_CHECKSUM

0xC01E058C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The data received from the monitor is
invalid.

A function call failed because a monitor
returned an invalid timing status byte
when the operating system used the
DDC/CI get timing report and timing
message command to get a timing report
from a monitor.

A monitor returned a DDC/CI capabilities
string that did not comply with the
ACCESS.bus 3.0, DDC/CI 1.1, or MCCS 2
Revision 1 specification.

An internal error caused an operation to
fail.

An operation failed because a DDC/CI
message had an invalid value in its
command field.

This error occurred because a DDC/CI
message had an invalid value in its
length field.

This error occurred because the value in
a DDC/CI message's checksum field did
not match the message's computed
checksum value. This error implies that
the data was corrupted while it was being
transmitted from a monitor to a
computer.

This function failed because an invalid
monitor handle was passed to it.

475 / 497


Return value/code

Description

STATUS_GRAPHICS_INVALID_PHYSICAL_MONITOR_HANDLE

0xC01E058D

STATUS_GRAPHICS_MONITOR_NO_LONGER_EXISTS

0xC01E05E0

STATUS_GRAPHICS_ONLY_CONSOLE_SESSION_SUPPORTED

0xC01E05E1

STATUS_GRAPHICS_NO_DISPLAY_DEVICE_CORRESPONDS_TO_NAM
E

0xC01E05E2

STATUS_GRAPHICS_DISPLAY_DEVICE_NOT_ATTACHED_TO_DESKTO
P

0xC01E05E3

STATUS_GRAPHICS_MIRRORING_DEVICES_NOT_SUPPORTED

0xC01E05E4

STATUS_GRAPHICS_INVALID_POINTER

0xC01E05E5

STATUS_GRAPHICS_NO_MONITORS_CORRESPOND_TO_DISPLAY_DE
VICE

0xC01E05E6

STATUS_GRAPHICS_PARAMETER_ARRAY_TOO_SMALL

0xC01E05E7

STATUS_GRAPHICS_INTERNAL_ERROR

0xC01E05E8

STATUS_GRAPHICS_SESSION_TYPE_CHANGE_IN_PROGRESS

0xC0210000

STATUS_FVE_LOCKED_VOLUME

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The operating system asynchronously
destroyed the monitor that corresponds
to this handle because the operating
system's state changed. This error
typically occurs because the monitor PDO
associated with this handle was removed
or stopped, or a display mode change
occurred. A display mode change occurs
when Windows sends a
WM_DISPLAYCHANGE message to
applications.

This function can be used only if a
program is running in the local console
session. It cannot be used if a program is
running on a remote desktop session or
on a terminal server session.

This function cannot find an actual GDI
display device that corresponds to the
specified GDI display device name.

The function failed because the specified
GDI display device was not attached to
the Windows desktop.

This function does not support GDI
mirroring display devices because GDI
mirroring display devices do not have any
physical monitors associated with them.

The function failed because an invalid
pointer parameter was passed to it. A
pointer parameter is invalid if it is null, is
not correctly aligned, or points to an
invalid address or to a kernel mode
address.

This function failed because the GDI
device passed to it did not have a
monitor associated with it.

An array passed to the function cannot
hold all of the data that the function must
copy into the array.

An internal error caused an operation to
fail.

The function failed because the current
session is changing its type. This function
cannot be called when the current
session is changing its type. Three types
of sessions currently exist: console,
disconnected, and remote (RDP or ICA).

The volume must be unlocked before it
can be used.

476 / 497


Return value/code

Description

0xC0210001

STATUS_FVE_NOT_ENCRYPTED

0xC0210002

STATUS_FVE_BAD_INFORMATION

0xC0210003

STATUS_FVE_TOO_SMALL

0xC0210004

STATUS_FVE_FAILED_WRONG_FS

0xC0210005

STATUS_FVE_FAILED_BAD_FS

0xC0210006

STATUS_FVE_FS_NOT_EXTENDED

0xC0210007

STATUS_FVE_FS_MOUNTED

0xC0210008

STATUS_FVE_NO_LICENSE

0xC0210009

STATUS_FVE_ACTION_NOT_ALLOWED

0xC021000A

STATUS_FVE_BAD_DATA

0xC021000B

STATUS_FVE_VOLUME_NOT_BOUND

0xC021000C

STATUS_FVE_NOT_DATA_VOLUME

0xC021000D

STATUS_FVE_CONV_READ_ERROR

0xC021000E

STATUS_FVE_CONV_WRITE_ERROR

0xC021000F

STATUS_FVE_OVERLAPPED_UPDATE

0xC0210010

STATUS_FVE_FAILED_SECTOR_SIZE

0xC0210011

STATUS_FVE_FAILED_AUTHENTICATION

0xC0210012

STATUS_FVE_NOT_OS_VOLUME

0xC0210013

STATUS_FVE_KEYFILE_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The volume is fully decrypted and no key
is available.

The control block for the encrypted
volume is not valid.

Not enough free space remains on the
volume to allow encryption.

The partition cannot be encrypted
because the file system is not supported.

The file system is inconsistent. Run the
Check Disk utility.

The file system does not extend to the
end of the volume.

This operation cannot be performed while
a file system is mounted on the volume.

BitLocker Drive Encryption is not included
with this version of Windows.

The requested action was denied by the
FVE control engine.

The data supplied is malformed.

The volume is not bound to the system.

The volume specified is not a data
volume.

A read operation failed while converting
the volume.

A write operation failed while converting
the volume.

The control block for the encrypted
volume was updated by another thread.
Try again.

The volume encryption algorithm cannot
be used on this sector size.

BitLocker recovery authentication failed.

The volume specified is not the boot
operating system volume.

The BitLocker startup key or recovery
password could not be read from external

477 / 497


Return value/code

0xC0210014

STATUS_FVE_KEYFILE_INVALID

0xC0210015

STATUS_FVE_KEYFILE_NO_VMK

0xC0210016

STATUS_FVE_TPM_DISABLED

0xC0210017

STATUS_FVE_TPM_SRK_AUTH_NOT_ZERO

0xC0210018

STATUS_FVE_TPM_INVALID_PCR

0xC0210019

STATUS_FVE_TPM_NO_VMK

0xC021001A

STATUS_FVE_PIN_INVALID

0xC021001B

STATUS_FVE_AUTH_INVALID_APPLICATION

0xC021001C

STATUS_FVE_AUTH_INVALID_CONFIG

0xC021001D

STATUS_FVE_DEBUGGER_ENABLED

0xC021001E

STATUS_FVE_DRY_RUN_FAILED

0xC021001F

STATUS_FVE_BAD_METADATA_POINTER

0xC0210020

STATUS_FVE_OLD_METADATA_COPY

0xC0210021

STATUS_FVE_REBOOT_REQUIRED

0xC0210022

STATUS_FVE_RAW_ACCESS

0xC0210023

STATUS_FVE_RAW_BLOCKED

0xC0210026

STATUS_FVE_NO_FEATURE_LICENSE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

media.

The BitLocker startup key or recovery
password file is corrupt or invalid.

The BitLocker encryption key could not
be obtained from the startup key or the
recovery password.

The TPM is disabled.

The authorization data for the SRK of the
TPM is not zero.

The system boot information changed or
the TPM locked out access to BitLocker
encryption keys until the computer is
restarted.

The BitLocker encryption key could not
be obtained from the TPM.

The BitLocker encryption key could not
be obtained from the TPM and PIN.

A boot application hash does not match
the hash computed when BitLocker was
turned on.

The Boot Configuration Data (BCD)
settings are not supported or have
changed because BitLocker was enabled.

Boot debugging is enabled. Run Windows
Boot Configuration Data Store Editor
(bcdedit.exe) to turn it off.

The BitLocker encryption key could not
be obtained.

The metadata disk region pointer is
incorrect.

The backup copy of the metadata is out
of date.

No action was taken because a system
restart is required.

No action was taken because BitLocker
Drive Encryption is in RAW access mode.

BitLocker Drive Encryption cannot enter
RAW access mode for this volume.

This feature of BitLocker Drive Encryption
is not included with this version of
Windows.

478 / 497


Return value/code

0xC0210027

STATUS_FVE_POLICY_USER_DISABLE_RDV_NOT_ALLOWED

0xC0210028

STATUS_FVE_CONV_RECOVERY_FAILED

0xC0210029

STATUS_FVE_VIRTUALIZED_SPACE_TOO_BIG

0xC0210030

STATUS_FVE_VOLUME_TOO_SMALL

0xC0220001

STATUS_FWP_CALLOUT_NOT_FOUND

0xC0220002

STATUS_FWP_CONDITION_NOT_FOUND

0xC0220003

STATUS_FWP_FILTER_NOT_FOUND

0xC0220004

STATUS_FWP_LAYER_NOT_FOUND

0xC0220005

STATUS_FWP_PROVIDER_NOT_FOUND

Description

Group policy does not permit turning off
BitLocker Drive Encryption on roaming
data volumes.

Bitlocker Drive Encryption failed to
recover from aborted conversion. This
could be due to either all conversion logs
being corrupted or the media being write-
protected.

The requested virtualization size is too
big.

The drive is too small to be protected
using BitLocker Drive Encryption.

The callout does not exist.

The filter condition does not exist.

The filter does not exist.

The layer does not exist.

The provider does not exist.

0xC0220006

The provider context does not exist.

STATUS_FWP_PROVIDER_CONTEXT_NOT_FOUND

0xC0220007

STATUS_FWP_SUBLAYER_NOT_FOUND

0xC0220008

STATUS_FWP_NOT_FOUND

0xC0220009

STATUS_FWP_ALREADY_EXISTS

0xC022000A

STATUS_FWP_IN_USE

0xC022000B

STATUS_FWP_DYNAMIC_SESSION_IN_PROGRESS

0xC022000C

STATUS_FWP_WRONG_SESSION

0xC022000D

STATUS_FWP_NO_TXN_IN_PROGRESS

0xC022000E

STATUS_FWP_TXN_IN_PROGRESS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The sublayer does not exist.

The object does not exist.

An object with that GUID or LUID already
exists.

The object is referenced by other objects
and cannot be deleted.

The call is not allowed from within a
dynamic session.

The call was made from the wrong
session and cannot be completed.

The call must be made from within an
explicit transaction.

The call is not allowed from within an
explicit transaction.

479 / 497


Return value/code

Description

0xC022000F

STATUS_FWP_TXN_ABORTED

0xC0220010

STATUS_FWP_SESSION_ABORTED

0xC0220011

STATUS_FWP_INCOMPATIBLE_TXN

0xC0220012

STATUS_FWP_TIMEOUT

0xC0220013

STATUS_FWP_NET_EVENTS_DISABLED

0xC0220014

STATUS_FWP_INCOMPATIBLE_LAYER

0xC0220015

STATUS_FWP_KM_CLIENTS_ONLY

0xC0220016

STATUS_FWP_LIFETIME_MISMATCH

0xC0220017

STATUS_FWP_BUILTIN_OBJECT

0xC0220018

STATUS_FWP_TOO_MANY_BOOTTIME_FILTERS

0xC0220018

STATUS_FWP_TOO_MANY_CALLOUTS

0xC0220019

STATUS_FWP_NOTIFICATION_DROPPED

0xC022001A

STATUS_FWP_TRAFFIC_MISMATCH

0xC022001B

STATUS_FWP_INCOMPATIBLE_SA_STATE

0xC022001C

STATUS_FWP_NULL_POINTER

0xC022001D

STATUS_FWP_INVALID_ENUMERATOR

0xC022001E

STATUS_FWP_INVALID_FLAGS

0xC022001F

STATUS_FWP_INVALID_NET_MASK

0xC0220020

STATUS_FWP_INVALID_RANGE

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The explicit transaction has been forcibly
canceled.

The session has been canceled.

The call is not allowed from within a
read-only transaction.

The call timed out while waiting to
acquire the transaction lock.

The collection of network diagnostic
events is disabled.

The operation is not supported by the
specified layer.

The call is allowed for kernel-mode
callers only.

The call tried to associate two objects
with incompatible lifetimes.

The object is built-in and cannot be
deleted.

The maximum number of boot-time
filters has been reached.

The maximum number of callouts has
been reached.

A notification could not be delivered
because a message queue has reached
maximum capacity.

The traffic parameters do not match
those for the security association context.

The call is not allowed for the current
security association state.

A required pointer is null.

An enumerator is not valid.

The flags field contains an invalid value.

A network mask is not valid.

An FWP_RANGE is not valid.

480 / 497


Return value/code

0xC0220021

STATUS_FWP_INVALID_INTERVAL

0xC0220022

STATUS_FWP_ZERO_LENGTH_ARRAY

0xC0220023

STATUS_FWP_NULL_DISPLAY_NAME

0xC0220024

STATUS_FWP_INVALID_ACTION_TYPE

0xC0220025

STATUS_FWP_INVALID_WEIGHT

0xC0220026

STATUS_FWP_MATCH_TYPE_MISMATCH

0xC0220027

STATUS_FWP_TYPE_MISMATCH

0xC0220028

STATUS_FWP_OUT_OF_BOUNDS

0xC0220029

STATUS_FWP_RESERVED

0xC022002A

STATUS_FWP_DUPLICATE_CONDITION

0xC022002B

STATUS_FWP_DUPLICATE_KEYMOD

0xC022002C

STATUS_FWP_ACTION_INCOMPATIBLE_WITH_LAYER

0xC022002D

STATUS_FWP_ACTION_INCOMPATIBLE_WITH_SUBLAYER

0xC022002E

STATUS_FWP_CONTEXT_INCOMPATIBLE_WITH_LAYER

0xC022002F

STATUS_FWP_CONTEXT_INCOMPATIBLE_WITH_CALLOUT

0xC0220030

STATUS_FWP_INCOMPATIBLE_AUTH_METHOD

0xC0220031

STATUS_FWP_INCOMPATIBLE_DH_GROUP

0xC0220032

STATUS_FWP_EM_NOT_SUPPORTED

0xC0220033

STATUS_FWP_NEVER_MATCH

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

The time interval is not valid.

An array that must contain at least one
element has a zero length.

The displayData.name field cannot be
null.

The action type is not one of the allowed
action types for a filter.

The filter weight is not valid.

A filter condition contains a match type
that is not compatible with the operands.

An FWP_VALUE or
FWPM_CONDITION_VALUE is of the
wrong type.

An integer value is outside the allowed
range.

A reserved field is nonzero.

A filter cannot contain multiple conditions
operating on a single field.

A policy cannot contain the same keying
module more than once.

The action type is not compatible with
the layer.

The action type is not compatible with
the sublayer.

The raw context or the provider context
is not compatible with the layer.

The raw context or the provider context
is not compatible with the callout.

The authentication method is not
compatible with the policy type.

The Diffie-Hellman group is not
compatible with the policy type.

An IKE policy cannot contain an Extended
Mode policy.

The enumeration template or subscription
will never match any objects.

481 / 497


Return value/code

0xC0220034

STATUS_FWP_PROVIDER_CONTEXT_MISMATCH

0xC0220035

STATUS_FWP_INVALID_PARAMETER

0xC0220036

STATUS_FWP_TOO_MANY_SUBLAYERS

Description

The provider context is of the wrong
type.

The parameter is incorrect.

The maximum number of sublayers has
been reached.

0xC0220037

STATUS_FWP_CALLOUT_NOTIFICATION_FAILED

The notification function for a callout
returned an error.

0xC0220038

STATUS_FWP_INCOMPATIBLE_AUTH_CONFIG

0xC0220039

STATUS_FWP_INCOMPATIBLE_CIPHER_CONFIG

0xC022003C

STATUS_FWP_DUPLICATE_AUTH_METHOD

0xC0220100

STATUS_FWP_TCPIP_NOT_READY

0xC0220101

STATUS_FWP_INJECT_HANDLE_CLOSING

0xC0220102

STATUS_FWP_INJECT_HANDLE_STALE

0xC0220103

STATUS_FWP_CANNOT_PEND

0xC0230002

STATUS_NDIS_CLOSING

0xC0230004

STATUS_NDIS_BAD_VERSION

0xC0230005

STATUS_NDIS_BAD_CHARACTERISTICS

0xC0230006

STATUS_NDIS_ADAPTER_NOT_FOUND

0xC0230007

STATUS_NDIS_OPEN_FAILED

0xC0230008

STATUS_NDIS_DEVICE_FAILED

0xC0230009

STATUS_NDIS_MULTICAST_FULL

0xC023000A

STATUS_NDIS_MULTICAST_EXISTS

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The IPsec authentication configuration is
not compatible with the authentication
type.

The IPsec cipher configuration is not
compatible with the cipher type.

A policy cannot contain the same auth
method more than once.

The TCP/IP stack is not ready.

The injection handle is being closed by
another thread.

The injection handle is stale.

The classify cannot be pended.

The binding to the network interface is
being closed.

An invalid version was specified.

An invalid characteristics table was used.

Failed to find the network interface or the
network interface is not ready.

Failed to open the network interface.

The network interface has encountered
an internal unrecoverable failure.

The multicast list on the network
interface is full.

An attempt was made to add a duplicate
multicast address to the list.

482 / 497


Return value/code

0xC023000B

STATUS_NDIS_MULTICAST_NOT_FOUND

0xC023000C

STATUS_NDIS_REQUEST_ABORTED

0xC023000D

STATUS_NDIS_RESET_IN_PROGRESS

0xC023000F

STATUS_NDIS_INVALID_PACKET

0xC0230010

STATUS_NDIS_INVALID_DEVICE_REQUEST

0xC0230011

STATUS_NDIS_ADAPTER_NOT_READY

0xC0230014

STATUS_NDIS_INVALID_LENGTH

0xC0230015

STATUS_NDIS_INVALID_DATA

0xC0230016

STATUS_NDIS_BUFFER_TOO_SHORT

0xC0230017

STATUS_NDIS_INVALID_OID

0xC0230018

STATUS_NDIS_ADAPTER_REMOVED

0xC0230019

STATUS_NDIS_UNSUPPORTED_MEDIA

0xC023001A

STATUS_NDIS_GROUP_ADDRESS_IN_USE

0xC023001B

STATUS_NDIS_FILE_NOT_FOUND

0xC023001C

STATUS_NDIS_ERROR_READING_FILE

0xC023001D

STATUS_NDIS_ALREADY_MAPPED

0xC023001E

STATUS_NDIS_RESOURCE_CONFLICT

0xC023001F

STATUS_NDIS_MEDIA_DISCONNECTED

Description

At attempt was made to remove a
multicast address that was never added.

The network interface aborted the
request.

The network interface cannot process the
request because it is being reset.

An attempt was made to send an invalid
packet on a network interface.

The specified request is not a valid
operation for the target device.

The network interface is not ready to
complete this operation.

The length of the buffer submitted for
this operation is not valid.

The data used for this operation is not
valid.

The length of the submitted buffer for
this operation is too small.

The network interface does not support
this object identifier.

The network interface has been removed.

The network interface does not support
this media type.

An attempt was made to remove a token
ring group address that is in use by other
components.

An attempt was made to map a file that
cannot be found.

An error occurred while NDIS tried to
map the file.

An attempt was made to map a file that
is already mapped.

An attempt to allocate a hardware
resource failed because the resource is
used by another component.

The I/O operation failed because the
network media is disconnected or the
wireless access point is out of range.

0xC0230022

The network address used in the request

483 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Return value/code

STATUS_NDIS_INVALID_ADDRESS

0xC023002A

STATUS_NDIS_PAUSED

0xC023002B

STATUS_NDIS_INTERFACE_NOT_FOUND

0xC023002C

STATUS_NDIS_UNSUPPORTED_REVISION

0xC023002D

STATUS_NDIS_INVALID_PORT

0xC023002E

STATUS_NDIS_INVALID_PORT_STATE

0xC023002F

STATUS_NDIS_LOW_POWER_STATE

0xC02300BB

STATUS_NDIS_NOT_SUPPORTED

0xC023100F

STATUS_NDIS_OFFLOAD_POLICY

0xC0231012

STATUS_NDIS_OFFLOAD_CONNECTION_REJECTED

0xC0231013

STATUS_NDIS_OFFLOAD_PATH_REJECTED

0xC0232000

STATUS_NDIS_DOT11_AUTO_CONFIG_ENABLED

0xC0232001

STATUS_NDIS_DOT11_MEDIA_IN_USE

0xC0232002

STATUS_NDIS_DOT11_POWER_STATE_INVALID

Description

is invalid.

The offload operation on the network
interface has been paused.

The network interface was not found.

The revision number specified in the
structure is not supported.

The specified port does not exist on this
network interface.

The current state of the specified port on
this network interface does not support
the requested operation.

The miniport adapter is in a lower power
state.

The network interface does not support
this request.

The TCP connection is not offloadable
because of a local policy setting.

The TCP connection is not offloadable by
the Chimney offload target.

The IP Path object is not in an offloadable
state.

The wireless LAN interface is in auto-
configuration mode and does not support
the requested parameter change
operation.

The wireless LAN interface is busy and
cannot perform the requested operation.

The wireless LAN interface is power down
and does not support the requested
operation.

0xC0232003

The list of wake on LAN patterns is full.

STATUS_NDIS_PM_WOL_PATTERN_LIST_FULL

0xC0232004

STATUS_NDIS_PM_PROTOCOL_OFFLOAD_LIST_FULL

The list of low power protocol offloads is
full.

0xC0360001

STATUS_IPSEC_BAD_SPI

0xC0360002

STATUS_IPSEC_SA_LIFETIME_EXPIRED

0xC0360003

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

The SPI in the packet does not match a
valid IPsec SA.

The packet was received on an IPsec SA
whose lifetime has expired.

The packet was received on an IPsec SA
that does not match the packet

484 / 497


Return value/code

STATUS_IPSEC_WRONG_SA

0xC0360004

STATUS_IPSEC_REPLAY_CHECK_FAILED

0xC0360005

STATUS_IPSEC_INVALID_PACKET

0xC0360006

STATUS_IPSEC_INTEGRITY_CHECK_FAILED

0xC0360007

STATUS_IPSEC_CLEAR_TEXT_DROP

0xC0360008

STATUS_IPSEC_AUTH_FIREWALL_DROP

0xC0360009

STATUS_IPSEC_THROTTLE_DROP

0xC0368000

STATUS_IPSEC_DOSP_BLOCK

0xC0368001

STATUS_IPSEC_DOSP_RECEIVED_MULTICAST

0xC0368002

STATUS_IPSEC_DOSP_INVALID_PACKET

0xC0368003

STATUS_IPSEC_DOSP_STATE_LOOKUP_FAILED

0xC0368004

STATUS_IPSEC_DOSP_MAX_ENTRIES

0xC0368005

STATUS_IPSEC_DOSP_KEYMOD_NOT_ALLOWED

0xC0368006

STATUS_IPSEC_DOSP_MAX_PER_IP_RATELIMIT_QUEUES

0xC038005B

STATUS_VOLMGR_MIRROR_NOT_SUPPORTED

0xC038005C

STATUS_VOLMGR_RAID5_NOT_SUPPORTED

0xC03A0014

STATUS_VIRTDISK_PROVIDER_NOT_FOUND

0xC03A0015

STATUS_VIRTDISK_NOT_VIRTUAL_DISK

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Description

characteristics.

The packet sequence number replay
check failed.

The IPsec header and/or trailer in the
packet is invalid.

The IPsec integrity check failed.

IPsec dropped a clear text packet.

IPsec dropped an incoming ESP packet in
authenticated firewall mode.  This drop is
benign.

IPsec dropped a packet due to DOS
throttle.

IPsec Dos Protection matched an explicit
block rule.

IPsec Dos Protection received an IPsec
specific multicast packet which is not
allowed.

IPsec Dos Protection received an
incorrectly formatted packet.

IPsec Dos Protection failed to lookup
state.

IPsec Dos Protection failed to create state
because there are already maximum
number of entries allowed by policy.

IPsec Dos Protection received an IPsec
negotiation packet for a keying module
which is not allowed by policy.

IPsec Dos Protection failed to create per
internal IP ratelimit queue because there
is already maximum number of queues
allowed by policy.

The system does not support mirrored
volumes.

The system does not support RAID-5
volumes.

A virtual disk support provider for the
specified file was not found.

The specified disk is not a virtual disk.

485 / 497


Return value/code

0xC03A0016

STATUS_VHD_PARENT_VHD_ACCESS_DENIED

0xC03A0017

STATUS_VHD_CHILD_PARENT_SIZE_MISMATCH

0xC03A0018

STATUS_VHD_DIFFERENCING_CHAIN_CYCLE_DETECTED

0xC03A0019

STATUS_VHD_DIFFERENCING_CHAIN_ERROR_IN_PARENT

0xC05D0000

STATUS_SMB_NO_PREAUTH_INTEGRITY_HASH_OVERLAP

0xC05D0001

STATUS_SMB_BAD_CLUSTER_DIALECT

Description

The chain of virtual hard disks is
inaccessible. The process has not been
granted access rights to the parent
virtual hard disk for the differencing disk.

The chain of virtual hard disks is
corrupted. There is a mismatch in the
virtual sizes of the parent virtual hard
disk and differencing disk.

The chain of virtual hard disks is
corrupted. A differencing disk is indicated
in its own parent chain.

The chain of virtual hard disks is
inaccessible. There was an error opening
a virtual hard disk further up the chain.

Returned in response to a client negotiate
request when the server does not
support any of the hash algorithms in the
request.

The current cluster functional level does
not support this SMB dialect.

### 2.4 LDAP Error to Win32 Error Mapping

Windows contains an implementation of the LDAP resultCode ([RFC2251] section 4.1.10) which is
used by higher-layer protocols to interpret the results of an LDAP operation.

Each LDAP error value is also mapped to the closest Win32 error value, for use by the higher-layer
protocols. This mapping is as shown in the following table:

Valu
e:
Deci
mal

Value:
Hexadec
imal
represe
ntation

LDAPResult.re
sultCode: RFC
1777

LDAPResult.re
sultCode: RFC
2251

Windows: Ldap Error
(LDAP_RETCODE
from winldap.w)

Windows: Win32 error
(from
LdapMapErrorToWin32 /
winmain\ds\ds\src\lda
p\client\util.cxx)

0

1

2

3

4

5

6

success

success

LDAP_SUCCESS

NO_ERROR

0x0

0x1

operationsError

operationsError

LDAP_OPERATIONS_ER
ROR

LDAP_PROTOCOL_ERRO
R

ERROR_OPEN_FAILED

ERROR_INVALID_LEVEL

0x2

protocolError

protocolError

0x3

0x4

timeLimitExceed
ed

timeLimitExceed
ed

LDAP_TIMELIMIT_EXCE
EDED

ERROR_TIMEOUT

sizeLimitExceed
ed

sizeLimitExceed
ed

LDAP_SIZELIMIT_EXCE
EDED

ERROR_MORE_DATA

0x5

compareFalse

compareFalse

LDAP_COMPARE_FALSE

ERROR_DS_GENERIC_ERR
OR

0x6

compareTrue

compareTrue

LDAP_COMPARE_TRUE

ERROR_DS_GENERIC_ERR

486 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Valu
e:
Deci
mal

Value:
Hexadec
imal
represe
ntation

LDAPResult.re
sultCode: RFC
1777

LDAPResult.re
sultCode: RFC
2251

Windows: Ldap Error
(LDAP_RETCODE
from winldap.w)

Windows: Win32 error
(from
LdapMapErrorToWin32 /
winmain\ds\ds\src\lda
p\client\util.cxx)

OR

7

8

9

0x7

0x8

0x9

10

11

0xA

0xB

12

0xC

13

0xD

14

0xE

authMethodNot
Supported

authMethodNot
Supported

LDAP_AUTH_METHOD_
NOT_SUPPORTED

ERROR_ACCESS_DENIED

strongAuthRequ
ired

strongAuthRequi
red

LDAP_STRONG_AUTH_
REQUIRED

ERROR_ACCESS_DENIED

9 reserved

LDAP_REFERRAL_V2,
LDAP_PARTIAL_RESULT
S

ERROR_MORE_DATA

referral

LDAP_REFERRAL

adminLimitExce
eded

LDAP_ADMIN_LIMIT_EX
CEEDED

ERROR_NOT_ENOUGH_QU
OTA

unavailableCritic
alExtension

LDAP_UNAVAILABLE_C
RIT_EXTENSION

ERROR_CAN_NOT_COMPLE
TE

confidentialityRe
quired

LDAP_CONFIDENTIALIT
Y_REQUIRED

saslBindInProgr
ess

LDAP_SASL_BIND_IN_P
ROGRESS

15

16

0xF

0x10

noSuchAttribute

noSuchAttribute

LDAP_NO_SUCH_ATTRI
BUTE

ERROR_INVALID_PARAMET
ER

17

0x11

undefinedAttrib
uteType

undefinedAttrib
uteType

LDAP_UNDEFINED_TYP
E

ERROR_DS_GENERIC_ERR
OR

18

0x12

inappropriateMa
tching

inappropriateMa
tching

LDAP_INAPPROPRIATE_
MATCHING

ERROR_INVALID_PARAMET
ER

19

0x13

constraintViolati
on

constraintViolati
on

LDAP_CONSTRAINT_VI
OLATION

ERROR_INVALID_PARAMET
ER

20

0x14

attributeOrValu
eExists

attributeOrValue
Exists

LDAP_ATTRIBUTE_OR_
VALUE_EXISTS

ERROR_ALREADY_EXISTS

21

0x15

invalidAttribute
Syntax

invalidAttributeS
yntax

LDAP_INVALID_SYNTAX  ERROR_INVALID_NAME

22

23

24

25

26

27

28

0x16

0x17

0x18

0x19

0x1A

0x1B

0x1C

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

487 / 497


Valu
e:
Deci
mal

29

30

31

32

LDAPResult.re
sultCode: RFC
1777

LDAPResult.re
sultCode: RFC
2251

Windows: Ldap Error
(LDAP_RETCODE
from winldap.w)

Windows: Win32 error
(from
LdapMapErrorToWin32 /
winmain\ds\ds\src\lda
p\client\util.cxx)

Value:
Hexadec
imal
represe
ntation

0x1D

0x1E

0x1F

0x20

noSuchObject

noSuchObject

LDAP_NO_SUCH_OBJEC
T

ERROR_FILE_NOT_FOUND

33

0x21

aliasProblem

aliasProblem

LDAP_ALIAS_PROBLEM

ERROR_DS_GENERIC_ERR
OR

34

0x22

invalidDNSyntax

invalidDNSyntax

LDAP_INVALID_DN_SY
NTAX

ERROR_INVALID_PARAMET
ER

35

0x23

isLeaf

35 reserved for
undefined isLeaf

LDAP_IS_LEAF

ERROR_DS_GENERIC_ERR
OR

36

0x24

aliasDereferenci
ngProblem

aliasDereferenci
ngProblem

LDAP_ALIAS_DEREF_PR
OBLEM

ERROR_DS_GENERIC_ERR
OR

37

38

39

40

41

42

43

44

45

46

47

48

0x25

0x26

0x27

0x28

0x29

0x2A

0x2B

0x2C

0x2D

0x2E

0x2F

0x30

37-47 unused

inappropriateAu
thentication

inappropriateAu
thentication

LDAP_INAPPROPRIATE_
AUTH

ERROR_ACCESS_DENIED

49

0x31

invalidCredentia
ls

invalidCredentia
ls

LDAP_INVALID_CREDE
NTIALS

ERROR_LOGON_FAILURE

50

0x32

insufficientAcces
sRights

insufficientAcces
sRights

LDAP_INSUFFICIENT_RI
GHTS

ERROR_ACCESS_DENIED

51

52

53

0x33

busy

busy

LDAP_BUSY

ERROR_BUSY

0x34

unavailable

unavailable

LDAP_UNAVAILABLE

ERROR_DEV_NOT_EXIST

0x35

unwillingToPerfo
rm

unwillingToPerfo
rm

LDAP_UNWILLING_TO_
PERFORM

ERROR_CAN_NOT_COMPLE
TE

488 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


Valu
e:
Deci
mal

Value:
Hexadec
imal
represe
ntation

LDAPResult.re
sultCode: RFC
1777

LDAPResult.re
sultCode: RFC
2251

Windows: Ldap Error
(LDAP_RETCODE
from winldap.w)

54

0x36

loopDetect

loopDetect

LDAP_LOOP_DETECT

Windows: Win32 error
(from
LdapMapErrorToWin32 /
winmain\ds\ds\src\lda
p\client\util.cxx)

ERROR_DS_GENERIC_ERR
OR

55-63 unused

55

56

57

58

59

60

0x37

0x38

0x39

0x3A

0x3B

0x3C

61

0x3D

LDAP_SORT_CONTROL_
MISSING

ERROR_DS_SORT_CONTR
OL_MISSING

LDAP_OFFSET_RANGE_
ERROR

ERROR_DS_OFFSET_RANG
E_ERROR

62

63

64

0x3E

0x3F

0x40

namingViolation

namingViolation

LDAP_NAMING_VIOLATI
ON

ERROR_INVALID_PARAMET
ER

65

0x41

objectClassViola
tion

objectClassViola
tion

LDAP_OBJECT_CLASS_
VIOLATION

ERROR_INVALID_PARAMET
ER

66

0x42

notAllowedOnNo
nLeaf

notAllowedOnNo
nLeaf

LDAP_NOT_ALLOWED_
ON_NONLEAF

ERROR_CAN_NOT_COMPLE
TE

67

0x43

notAllowedOnR
DN

notAllowedOnR
DN

LDAP_NOT_ALLOWED_
ON_RDN

ERROR_ACCESS_DENIED

68

0x44

entryAlreadyExi
sts

entryAlreadyExi
sts

LDAP_ALREADY_EXISTS  ERROR_ALREADY_EXISTS

69

0x45

objectClassMods
Prohibited

objectClassMods
Prohibited

LDAP_NO_OBJECT_CLA
SS_MODS

ERROR_ACCESS_DENIED

70

0x46

71

0x47

72

73

74

75

76

0x48

0x49

0x4A

0x4B

0x4C

77

0x4D

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

70 reserved for
CLDAP

LDAP_RESULTS_TOO_L
ARGE

ERROR_INSUFFICIENT_BU
FFER

affectsMultipleD
SAs

LDAP_AFFECTS_MULTIP
LE_DSAS

ERROR_CAN_NOT_COMPLE
TE

72-79 unused

LDAP_VIRTUAL_LIST_VI
EW_ERROR

489 / 497


Valu
e:
Deci
mal

78

79

80

LDAPResult.re
sultCode: RFC
1777

LDAPResult.re
sultCode: RFC
2251

Windows: Ldap Error
(LDAP_RETCODE
from winldap.w)

Windows: Win32 error
(from
LdapMapErrorToWin32 /
winmain\ds\ds\src\lda
p\client\util.cxx)

Value:
Hexadec
imal
represe
ntation

0x4E

0x4F

0x50

other

other

LDAP_OTHER

ERROR_DS_GENERIC_ERR
OR

81

0x51

82

0x52

83

0x53

84

0x54

85

0x55

86

0x56

87

0x57

88

0x58

89

0x59

90

0x5A

91

0x5B

92

0x5C

93

0x5D

94

0x5E

95

0x5F

96

97

0x60

0x61

98

0x62

81-90 reserved
for APIs

LDAP_SERVER_DOWN

ERROR_BAD_NET_RESP

LDAP_LOCAL_ERROR

LDAP_ENCODING_ERRO
R

LDAP_DECODING_ERR
OR

LDAP_TIMEOUT

ERROR_DS_GENERIC_ERR
OR

ERROR_UNEXP_NET_ERR

ERROR_UNEXP_NET_ERR

ERROR_SERVICE_REQUES
T_TIMEOUT

LDAP_AUTH_UNKNOWN  ERROR_WRONG_PASSWOR
D

LDAP_FILTER_ERROR

ERROR_INVALID_PARAMET
ER

LDAP_USER_CANCELLE
D

ERROR_CANCELLED

LDAP_PARAM_ERROR

ERROR_INVALID_PARAMET
ER

LDAP_NO_MEMORY

ERROR_NOT_ENOUGH_ME
MORY

LDAP_CONNECT_ERRO
R

ERROR_CONNECTION_REF
USED

LDAP_NOT_SUPPORTED  ERROR_CAN_NOT_COMPLE

TE

LDAP_NO_RESULTS_RE
TURNED

ERROR_MORE_DATA

LDAP_CONTROL_NOT_F
OUND

ERROR_NOT_FOUND

LDAP_MORE_RESULTS_
TO_RETURN

ERROR_MORE_DATA

LDAP_CLIENT_LOOP

LDAP_REFERRAL_LIMIT
_EXCEEDED

490 / 497

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024


## 3 Structure Example

 There are no structure examples.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

491 / 497


## 4 Security Considerations

These structures require no security considerations beyond those of the protocols that utilize them.

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

492 / 497


## 5 Appendix A: Product Behavior

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

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 2.1: All HRESULT values used by Microsoft have the C bit clear.

<2> Section 2.1: The following HRESULT codes have the X bit set to 1:

Value

Name

0x0DEAD100  TRK_S_OUT_OF_SYNC

0x0DEAD102  TRK_VOLUME_NOT_FOUND

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

493 / 497


Value

Name

0x0DEAD103  TRK_VOLUME_NOT_OWNED

0x0DEAD107  TRK_S_NOTIFICATION_QUOTA_EXCEEDED

0x8DEAD01B  TRK_E_NOT_FOUND

0x8DEAD01C  TRK_E_VOLUME_QUOTA_EXCEEDED

0x8DEAD01E  TRK_SERVER_TOO_BUSY

<3> Section 2.1.1:  If you are the driver owner, please contact Windows Hardware Developer
Support.

<4> Section 2.3: All NTSTATUS values that are used by Microsoft have the C bit clear.

<5> Section 2.3: Windows defines the following NTSTATUS facility values when the C bit is clear.

Name

FACILITY_DEBUGGER

FACILITY_RPC_RUNTIME

FACILITY_RPC_STUBS

FACILITY_IO_ERROR_CODE

FACILITY_NTWIN32

FACILITY_NTSSPI

Value

0x001

0x002

0x003

0x004

0x007

0x009

FACILITY_TERMINAL_SERVER

0x00A

FACILTIY_MUI_ERROR_CODE

0x00B

FACILITY_USB_ERROR_CODE

0x010

FACILITY_HID_ERROR_CODE

0x011

FACILITY_FIREWIRE_ERROR_CODE  0x012

FACILITY_CLUSTER_ERROR_CODE

0x013

FACILITY_ACPI_ERROR_CODE

0x014

FACILITY_SXS_ERROR_CODE

0x015

FACILITY_TRANSACTION

FACILITY_COMMONLOG

FACILITY_VIDEO

0x019

0x01A

0x01B

FACILITY_FILTER_MANAGER

0x01C

FACILITY_MONITOR

0x01D

FACILITY_GRAPHICS_KERNEL

0x01E

FACILITY_DRIVER_FRAMEWORK

0x020

FACILITY_FVE_ERROR_CODE

0x021

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

494 / 497


Name

Value

FACILITY_FWP_ERROR_CODE

0x022

FACILITY_NDIS_ERROR_CODE

0x023

FACILITY_HYPERVISOR

FACILITY_IPSEC

FACILITY_MAXIMUM_VALUE

0x035

0x036

0x037

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

495 / 497


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

2.3.1
NTSTATUS
Values

11820 : Added STATUS_SMB_BAD_CLUSTER_DIALECT and
STATUS_SMB_NO_PREAUTH_INTEGRITY_HASH_OVERLAP to the table of
NTSTATUS values.

Revision
class

Major

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

496 / 497


## 7 Index
A

Applicability 6

C

Change tracking 500

E

Examples 495

F

Fields - vendor extensible 6
Fields - vendor-extensible 6

G

Glossary 5

H

HRESULTs packet 7

I

Implementer - security considerations 496
Informative references 6
Introduction 5

L

Localization 6

N

Normative references 5
NTSTATUS packet 377

O

Overview 6
Overview (synopsis) 6

P

Product behavior 497

R

References 5
   informative 6
   normative 5
Relationship to protocols and other structures 6
Relationships
   other protocols 6
   other structures 6

S

[MS-ERREF] - v20241119
Windows Error Codes
Copyright © 2024 Microsoft Corporation
Release: November 19, 2024

Security - considerations 496
Security - implementer considerations 496
Structures 7

T

Tracking changes 500

V

Vendor extensible fields 6
Vendor-extensible fields 6
Versioning 6

497 / 497

