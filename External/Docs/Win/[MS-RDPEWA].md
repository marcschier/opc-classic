[MS-RDPEWA]:

Remote Desktop Protocol: WebAuthn Virtual Channel
Protocol

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

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

1 / 41

Revision Summary

Date

Revision
History

Revision
Class

Comments

9/3/2022

1.0

11/8/2022  1.0

4/23/2024  2.0

3/30/2026  3.0

New

None

Major

Major

Released new document.

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Significantly changed the technical content.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

2 / 41

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
Overview .......................................................................................................... 6
Relationship to Other Protocols ............................................................................ 6
Prerequisites/Preconditions ................................................................................. 6
Applicability Statement ....................................................................................... 6
Versioning and Capability Negotiation ................................................................... 6
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

2.2.1.1
2.2.1.2
2.2.1.3

2  Messages ................................................................................................................. 8
Transport .......................................................................................................... 8
Message Syntax ................................................................................................. 8
WebAuthN_Channel Request Message ............................................................. 8
webAuthNPara Map ............................................................................... 11
CTAPCBOR_CMD_MAKE_CREDENTIAL Request ......................................... 13
CTAPCBOR_CMD_GET_ASSERTION Request ............................................. 15
WebAuthN_Channel Response Message ......................................................... 15
CTAPCBOR_RPC_COMMAND_WEB_AUTHN Response Map .......................... 16
CTAP MakeCredential Response ......................................................... 18
CTAP GetAssertion Response ............................................................ 18
CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS Response Map ................. 19
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST Response Map ..... 21

2.2.2.1.1
2.2.2.1.2

2.2.2.2
2.2.2.3

2.2.2.1

2.2.2

3.1

3  Protocol Details ..................................................................................................... 23
Client and Server Details ................................................................................... 23
Abstract Data Model .................................................................................... 23
Timers ...................................................................................................... 23
Initialization ............................................................................................... 23
Higher-Layer Triggered Events ..................................................................... 23
Message Processing Events and Sequencing Rules .......................................... 23
Timer Events .............................................................................................. 23
Other Local Events ...................................................................................... 23

3.1.1
3.1.2
3.1.3
3.1.4
3.1.5
3.1.6
3.1.7

4.1

4.3

4.2

4.1.1
4.1.2

4.2.1
4.2.2

4  Protocol Examples ................................................................................................. 24
CTAPCBOR_RPC_COMMAND_API_VERSION ......................................................... 24
Request ..................................................................................................... 24
Response ................................................................................................... 24
CTAPCBOR_RPC_COMMAND_IUVPAA .................................................................. 24
Request ..................................................................................................... 24
Response ................................................................................................... 24
CTAPCBOR_RPC_COMMAND_CANCEL_CUR_OP .................................................... 25
Request ..................................................................................................... 25
Response ................................................................................................... 25
CTAPCBOR_RPC_COMMAND_WEB_AUTHN .......................................................... 25
CTAPCBOR_CMD_MAKE_CREDENTIAL ........................................................... 25
Request ............................................................................................... 25
Response ............................................................................................. 27
CTAPCBOR_CMD_GET_ASSERTION ............................................................... 29
Request ............................................................................................... 29
Response ............................................................................................. 31
CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS ................................................. 32

4.4.2.1
4.4.2.2

4.4.1.1
4.4.1.2

4.3.1
4.3.2

4.4.2

4.4.1

4.5

4.4

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

3 / 41

4.6

4.5.1
4.5.2

4.6.1
4.6.2

Request ..................................................................................................... 32
Response ................................................................................................... 33
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST ..................................... 35
Request ..................................................................................................... 35
Response ................................................................................................... 35

5  Security ................................................................................................................. 37
Security Considerations for Implementers ........................................................... 37
Index of Security Parameters ............................................................................ 37

5.1
5.2

6  Appendix A: Product Behavior ............................................................................... 38

7  Change Tracking .................................................................................................... 39

8  Index ..................................................................................................................... 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

4 / 41

1  Introduction

The Remote Desktop Protocol (RDP): WebAuthn Virtual Channel Protocol provides a way for a user to
do WebAuthn operations over the RDP protocol. It enables a server to send webauthn request to a
client, the client can then use this request to talk to authenticators (platform as well as cross-
platform) and reply with the response.

Sections 1.5, 1.8, 1.9, 2, and 3 of this specification are normative. All other sections and examples in
this specification are informative.

1.1  Glossary

This document uses the following terms:

globally unique identifier (GUID): A term used interchangeably with universally unique

identifier (UUID) in Microsoft protocol technical documents (TDs). Interchanging the usage of
these terms does not imply or require a specific algorithm or mechanism to generate the value.
Specifically, the use of this term does not imply or require that the algorithms described in
[RFC4122] or [C706] have to be used for generating the GUID. See also universally unique
identifier (UUID).

Remote Desktop Protocol (RDP): A multi-channel protocol that allows a user to connect to a

computer running Microsoft Terminal Services (TS). RDP enables the exchange of client and
server settings and also enables negotiation of common settings to use for the duration of the
connection, so that input, graphics, and other data can be exchanged and processed between
client and server.

Transmission Control Protocol (TCP): A protocol used with the Internet Protocol (IP) to send
data in the form of message units between computers over the Internet. TCP handles keeping
track of the individual units of data (called packets) that a message is divided into for efficient
routing through the Internet.

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

[FIDO-CTAP] Brand, C., Czeskis, A., Ehrensvärd, J. et al. Eds., "Client to Authenticator Protocol
(CTAP)", https://fidoalliance.org/specs/fido-v2.0-ps-20190130/fido-client-to-authenticator-protocol-
v2.0-ps-20190130.html

[IETF-8949] Hoffman, P., "Concise Binary Object Representation (CBOR)",
https://www.ietf.org/rfc/rfc8949.txt

[MS-ERREF] Microsoft Corporation, "Windows Error Codes".

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

5 / 41

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[W3C-WebAuthPKC2] W3C, "Web Authentication: An API for accessing Public Key Credentials Level 2",
https://www.w3.org/TR/webauthn-2/

[W3C-WebAuthPKC3] W3C, "Web Authentication: An API for accessing Public Key Credentials Level 3",
https://www.w3.org/TR/webauthn-3/

1.2.2  Informative References

[MSFT-WebAuthnAPIS] Microsoft Corporation, "Microsoft WebAuthn APIs",
https://github.com/microsoft/webauthn/blob/master/webauthn.h

[MSKB-5065789] Microsoft Corporation, "September 29, 2025—KB5065789", September 2025,
https://support.microsoft.com/en-au/topic/september-29-2025-kb5065789-os-builds-26200-6725-
and-26100-6725-preview-fa03ce47-cec5-4d1c-87d0-cac4195b4b4e

1.3  Overview

The Remote Desktop Protocol: WebAuthn Virtual Channel provides a way for a user to do WebAuthn
operations over the RDP protocol.

More details about WebAuthn can be found in[W3C-WebAuthPKC3] and [W3C-WebAuthPKC2].

The WebAuthn javascript API is handled by the browser. The browser in turn calls the system-provided
APIs on Windows. The system then establishes a virtual channel to the RDP client and sends the
request to it. On the RDP client side, the request is decoded and processed by talking to available
authenticators via the Client-to-Authenticator protocol (CTAP) protocol. See [FIDO-CTAP] for more
details about the CTAP protocol.

1.4  Relationship to Other Protocols

This protocol uses the [W3C-WebAuthPKC3], [W3C-WebAuthPKC2] and [FIDO-CTAP] protocols.

1.5  Prerequisites/Preconditions

The Remote Desktop Protocol: WebAuthn Virtual Channel operates only after the dynamic virtual
channel transport is fully established.

This protocol is message-based. It assumes preservation of the packet as a whole and does not allow
for fragmentation. Additionally, it assumes that no packets are lost.

1.6  Applicability Statement

This protocol is designed to be run within the context of a Remote Desktop Protocol (RDP) virtual
channel established between a client and a server.

1.7  Versioning and Capability Negotiation

This protocol supports versioning and capability negotiation as part of the request. A client that
supports this protocol does allow this virtual channel to be opened, and a client that does not support
this protocol does not allow this virtual channel to be opened.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

6 / 41

1.8  Vendor-Extensible Fields

This protocol also uses Win32 error codes. These values are taken from the error number space as
specified in [MS-ERREF] section 2.2. Vendors SHOULD reuse those values with their indicated
meanings. Choosing any other value runs the risk of a collision in the future.

1.9  Standards Assignments

None.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

7 / 41

2  Messages

2.1  Transport

The protocol uses a channel named WebAuthN_Channel. This channel MUST be implemented using a
reliable protocol, such as TCP. Messages written to this channel are assumed to be complete and to
arrive in order.

2.2  Message Syntax

Requests and responses are encoded in Concise Binary Object Representation (CBOR) format. For
more details about CBOR, see [IETF-8949]. CBOR encoding is used because it is used in the CTAP
protocol ([FIDO-CTAP]) to access security keys. Hence clients needing to use external security keys
need to use CBOR encoding. Platform authenticators provide their own APIs to talk to use their
implementations. Overall, the WebAuthn request from the relying party is encoded along with
metadata about the operation in the request message.

This protocol has two messages, a request message and a response message. The two messages
perform different operations depending on their content. Each message is a CBOR map (see [IETF-
8949], section 3.1, “Major Types,” for a description of a map). The messages themselves, depending
on the type of request or response, will in turn contain additional maps.

The next two sections describe the request and response messages and their elements.

2.2.1  WebAuthN_Channel Request Message

The WebAuthN_Channel request message is a CBOR map using the following keys and values.

command (key type: text string (major type 3)): An unsigned integer (major type 0) indicating the

RPC command type. This field MUST be set to one of the following values:

Value

Meaning

CTAPCBOR_RPC_COMMAND_WEB_AUTHN
5

Contains both registration and assertion request
for the platform authenticator as well as security
keys.

CTAPCBOR_RPC_COMMAND_IUVPAA
6

Corresponds to the WebAuthn
IsUserVerifyingPlatformAuthenticatorAvailable API.
See [W3C-WebAuthPKC2], section 5.1.7.

CTAPCBOR_RPC_COMMAND_CANCEL_CUR_OP
7

Cancel the current webauthn request.

CTAPCBOR_RPC_COMMAND_API_VERSION

8

Get the platform authenticator API version.<1>
Callers can use the version to identify what
features are available on the OS so that caller can
decide whether or not a request can be fulfilled.

CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL

Get credential metadata information from the
platform for a relying party.<2>

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

8 / 41

Value

9

Meaning

CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST
12

Get authenticator metadata information for user
agents.<3>

 request (key type: text string (major type 3)): A byte string (major type 2) containing details about

the request. The contents vary depending on the command type.

For CTAPCBOR_RPC_COMMAND_API_VERSION, CTAPCBOR_RPC_COMMAND_IUVPAA,
CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS, and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST, this field SHOULD NOT be present.

For CTAPCBOR_RPC_COMMAND_CANCEL_CUR_OP, this field MUST contain a GUID representing
the current operation.

For CTAPCBOR_RPC_COMMAND_WEB_AUTHN, first byte MUST contain the WebAuthn command
type:

Value

Meaning

CTAPCBOR_CMD_MAKE_CREDENTIAL
0x01

This command is used to create a new credential for
an account for a relying party (registration phase).
This is done once per account.

CTAPCBOR_CMD_GET_ASSERTION
0x02

Used to authenticate the user and sign the client
data using the key created previously during the
registration phase. This is also called the
authenticate phase. The command is exercised
multiple times after the registration phase.

The second and following bytes MUST contain a CBOR map corresponding to the WebAuthn
command type in the preceding table. See sections section 2.2.1.2 and section 2.2.1.3.

flags (key type: text string (major type 3)): An unsigned integer (major type 0) containing details

about the request. The value is an exclusive or (XOR) of the following values:

Value

Meaning

CTAPCLT_U2F_FLAG
0x00020000

Set to indicate the request and response will use
U2F. The provider should use the U2F device
interface instead of the CTAP interface.

CTAPCLT_DUAL_FLAG
0x00040000

Set to indicate to first try CTAP messages and
protocol. If CTAP fails, use U2F messages..

CTAPCLT_CLIENT_PIN_REQUIRED_FLAG
0x00100000

Set to force the use of a client pin for
CTAPCBOR_CMD_MAKE_CREDENTIAL.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

9 / 41

Value

Meaning

CTAPCLT_SELECT_CREDENTIAL_ALLOW_UV_FLAG
0x00008000

When set for a login get assertion, allows user
verification (UV) get assertions to select the
credential.

CTAPCLT_UV_REQUIRED_FLAG
0x00400000

Set to require user verification.

CTAPCLT_UV_PREFERRED_FLAG
0x00800000

Set to indicate user verification is preferred.

CTAPCLT_UV_NOT_REQUIRED_FLAG
0x01000000

Indicates that user verification is not required.

CTAPCLT_HMAC_SECRET_EXTENSION_FLAG
0x04000000

Set to enable the hmac-secret extension for a
CTAPCBOR_CMD_MAKE_CREDENTIAL request.

CTAPCLT_FORCE_U2F_V2_FLAG
0x08000000

Set to force the U2F version 2 interface to be used.

rpId (key type: text string (major type 3)): Text string (major type 3) representing Relying Party ID

for which credential metadata is being requested via
CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS command.

authenticatorInfoLogoRequestType (key type: text string (major type 3)): An unsigned integer

(major type 0) representing the type of authenticator logo requested.

Value

Meaning

WEBAUTHN_AUTHENTICATOR_LOGO_REQUEST_TYPE_NONE

No Logo is requested.

0

WEBAUTHN_AUTHENTICATOR_LOGO_REQUEST_TYPE_LIGHT

Light theme logo is requested.

1

WEBAUTHN_AUTHENTICATOR_LOGO_REQUEST_TYPE_DARK

Dark theme logo is requested.

2

WEBAUTHN_AUTHENTICATOR_LOGO_REQUEST_TYPE_ALL

All theme logos are requested.

3

hmacSecretSaltValues (key type: text string (major type 3)): A byte string (major type 2)

containing hmac-secret extension salt values.

10 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

thirdPartyPayment (key type: text string (major type 3)): A Boolean value (major type 7), if set
indicates that the credential is applicable for third party payment.

clientDataJSON (key type: text string (major type 3)): A byte string (major type 2) containing Client
Data JSON calculated by the user agent.

remoteWebAuthn (key type: text string (major type 3)): A Boolean value (major type 7), if set
indicates that the request is remote WebAuthn.

filterHybridTransport (key type: text string (major type 3)): A Boolean value (major type 7), if set

indicates that the hybrid transport to be filtered out.

timeout (key type: text string (major type 3)): An unsigned integer (major type 0) representing the

timeout (in milliseconds) for the operation.

transactionid (key type: text string (major type 3)): A byte array (major type 2); a GUID that is the

transaction identifier.

webAuthNPara (key type: text string (major type 3)): A CBOR map (major type 5) providing

parameters for authentication. See section 2.2.1.1 for details of the map.

2.2.1.1  webAuthNPara Map

The webAuthNPara is used in the WebAuthN_Channel request message to specify the parameters to
use for authentication. It has the following keys and values:

wnd (key type: text string (major type 3)): An unsigned integer (major type 0) that is the window

handle for the caller.

attachment (key type: text string (major type 3)): An unsigned integer (majory type 0) that

indicates the authenticator applicable to this operation.

Value

Meaning

WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY
0

Use any authenticator that can satisfy the
request conditions.

WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM
1

Use the platform authenticator to satisfy
the request conditions.<4>

WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM
2

Use the cross-platform roaming
authenticator, such as security keys or
phones, to satisfy the request conditions.

requireResident (key type: text string (major type 3)): A true or false CBOR simple value (see

[IETF-8949], section 3.3) indicating whether resident credential keys are required.

preferResident (key type: text string (major type 3)): A true or false CBOR simple value (see [IETF-

8949], section 3.3) indicating whether resident credential keys are preferred.

userVerification (key type: text string (major type 3)): An unsigned integer (major type 0) that

represents the verification requirements. This field MUST be set to one of the following values:

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

11 / 41

Value

Meaning

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY
0

User verification is not required, and any
setting is acceptable to the relying party.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED
1

User verification is required by the relying
party.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED
2

User verification is preferred by the
relying party.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED
3

User verification is discouraged by the
relying party.

 attestationPreference (key type: text string (major type 3)): An unsigned integer (major type 0)

indicating the preferred attestation method. This field MUST be set to one of the following values:

Value

Meaning

WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY
0

Use any attestation conveyance
preference.

WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_NONE
1

No preference among attestation
conveyance methods.

WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_INDIRECT
2

Prefer indirect attestation conveyance.

WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_DIRECT
3

Prefer direct attestation conveyance.

enterpriseAttestation (key type: text string (major type 3)): An unsigned integer (major type 0)

indicating the enterprise attestation to use. This field MUST be set to one of the following values:

Value

Meaning

WEBAUTHN_ENTERPRISE_ATTESTATION_NONE
0

Enterprise attestation is not requested by
the relying party.

WEBAUTHN_ENTERPRISE_ATTESTATION_VENDOR_FACILITATED
1

Enterprise attestation is requested by the
relying party and authenticator can provide
it if configured with this relying party.

WEBAUTHN_ENTERPRISE_ATTESTATION_PLATFORM_MANAGED
2

Enterprise attestation is requested by the
relying party and the platform
(OS/browser) if configured with this relying

12 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

Value

Meaning

party can allow such attestation.

cancellationId (key type: text string (major type 3)): A byte array (major type 2); a GUID that is

the cancellation identifier.

credLargeBlobOperation (key type: text string (major type 3)): An unsigned integer (major type 0)
indicating the kind of large blob operation RP wants to perform. This field MUST be set to one of
the following values:

Value

Meaning

0

1

2

3

WEBAUTHN_CRED_LARGE_BLOB_OPERATION_NONE

WEBAUTHN_CRED_LARGE_BLOB_OPERATION_GET

WEBAUTHN_CRED_LARGE_BLOB_OPERATION_SET

WEBAUTHN_CRED_LARGE_BLOB_OPERATION_DELETE

credLargeBlob (key type: text string (major type 3)): Byte String. Major Type 2. Representing Large
Blob.

largeBlobSupport (key type: text string (major type 3)): An unsigned integer (major type 0)

indicating the kind of largeBlob support RP is requesting. This field MUST be set to one of the
following values:

Value

Meaning

0

1

2

WEBAUTHN_LARGE_BLOB_SUPPORT_NONE

WEBAUTHN_LARGE_BLOB_SUPPORT_REQUIRED

WEBAUTHN_LARGE_BLOB_SUPPORT_PREFERRED

transportHint (key type: text string (major type 3)): A Text String (major type 3) indicating the

transport RP wants system to use. This field MUST be set to one of following values specified in
section 5.8.8 of [W3C-WebAuthPKC3].

2.2.1.2  CTAPCBOR_CMD_MAKE_CREDENTIAL Request

This is the CBOR map used in the WebAuthN_Channel request (section 2.2.1) for the
CTAPCBOR_CMD_MAKE_CREDENTIAL command. The map contains details needed for the request as
defined in [FIDO-CTAP], section 5.1.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

13 / 41

The map has up to seven fields indicated by numeric keys. Nearly all of the fields are themselves
CBOR maps. Some fields are optional.

Key

Field

1

2

3

4

5

6

7

Client data hash.

Relying party information.

User account information.

Algorithm preference.

Exclude list (optional).

Extensions (optional).

Options (optional).

The following is a text representation of an example request:

 {
   1: h'19EEDF1F51A140B34B316293B10C2BC0E6C53860771BB1DDA4A82E5DE94A4E7C',
   2: {
     id: "ctap.dev",
     name: "WebAuthn Test Server"
   },
   3: {
     id: h'6D696B65406578616D706C652E636F6D',
     name: "mike@example.com",
     displayName: "Mike Marlowe"
   },
   4: [
     {
       alg: -7,
       type: "public-key"
     },
     {
       alg: -257,
       type: "public-key"
     }
   ],
   5: [
     {
       id: h'1CA0E7AAAB613DA3FAC3C76366A6046E',
       type: "public-key",
       transports: 1
     },
     {
       id: h'2180DA5815A4443A91050803E93EF39A',
       type: "public-key",
       transports: 1
     }
   ],
   6: {
     credBlob: h'31323132',
     largeBlob: {
       support: "preferred"
     },
     credProtect: 2,
     largeBlobKey: true,
     minPinLength: true
   },
   7: {
     rk: true
   }

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

14 / 41

 }

See [FIDO-CTAP], section 5.1, for details.

2.2.1.3  CTAPCBOR_CMD_GET_ASSERTION Request

This is the CBOR map used in the WebAuthN_Channel request (section 2.2.1) for the
CTAPCBOR_CMD_GET_ASSERTION command. The map contains details needed for the request as
defined in [FIDO-CTAP], section 5.2.

The map has up to five fields, indicated by numeric keys. Nearly all of the fields are themselves CBOR
maps. Some fields are optional.

Key

Field

1

2

3

4

5

Relying party identifier.

Client data hash.

Credential include list (optional).

Extensions (optional).

Exclude list (optional).

The following is a text representation of an example request:

 {
   1: "ctap.dev",
   2: h'6F43BB640674BE2C2CC33AD6F0F08E450DC035F2113D1FB38A6985E32D3CB14F',
   3: [
     {
       id:
h'1C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA
861D4',
       type: "public-key"
     },
     {
       id: h'BC365B699040F8B4819833CF2F0FB4733BD424A3B043913C8CF5527799F4373A',
       type: "public-key"
     }
   ],
   4: {
     credBlob: true
   },
   5: {
     up: true
   }
 }

See [FIDO-CTAP], section 5.2, for details.

2.2.2  WebAuthN_Channel Response Message

The WebAuthN_Channel response message is a 32-bit value followed by bytes containing data
depending on the RPC command:

hresult (unsigned integer): A 32-bit HRESULT for the RPC command. See [MS-ERREF] section 2.1.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

15 / 41

response (byte string): Variable length string of bytes depending on the RPC command. The following

are the forms the response takes for a given RPC command:

RPC Command

Response Type

CTAPCBOR_RPC_COMMAND_WEB_AUTHN
5

A CBOR map (major type 5). See section 2.2.2.1.

CTAPCBOR_RPC_COMMAND_IUVPA
6

A 4-byte Boolean value in little endian format. 1
for true and 0 for false.

CTAPCBOR_RPC_COMMAND_CANCEL_CUR_OP
7

No additional bytes.

CTAPCBOR_RPC_COMMAND_API_VERSION
8

A 4-byte unsigned integer in little endian format
giving the API version.

CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS

A CBOR array containing metadata information
about the credentials.

9

CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST

A CBOR array containing authenticator metadata
information.

12

2.2.2.1  CTAPCBOR_RPC_COMMAND_WEB_AUTHN Response Map

The WebAuthN_Channel response message is a CBOR map using the following keys and values.

deviceInfo (key type text string (major type 3)): A CBOR map (major type 5) containing information

about the device. The map uses the following keys and values:

maxMsgSize (key type text string (major type 3)): An unsigned 32-bit integer (major type 0)

providing the maximum message size the authenticator can process.

maxSerializedLargeBlobArray (key type text string (major type 3)): An unsigned 32-bit integer

(major type 0) providing the maximum size serialized large blob the authenticator can
process.

providerType (key type text string (major type 3)): A text string (major type 3) representing the

provider type. Used only for information purposes. One of the following values:

Value

Meaning

CTAPHID_PROVIDER_TYPE
L"Hid"

An Hid provider.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

16 / 41

Value

Meaning

CTAPNFC_PROVIDER_TYPE
 L"Nfc"

CTAPBLE_PROVIDER_TYPE
L"Ble"

An Nfc provider.

A Ble provider.

WEBAUTHN_PLATFORM_PROVIDER_TYPE
L"Platform"

A platform provider.

providerName (key type text string (major type 3)): A text string (major type 3) representing

the provider’s name. Used only for information.

devicePath (key type text string (major type 3)): A text string (major type 3) providing the path

to the authenticator.

Manufacturer (key type text string (major type 3)): A text string (major type 3) representing the

manufacturer of the authenticator.

Product (key type text string (major type 3)): A text string (major type 3) representing the

product name of the authenticator.

aaGuid (key type text string (major type 3)): A 16 byte byte-string (major type 2) containing the

Authenticator Attestation GUID (AAGUID) for the authenticator.

residentKey (key type text string (major type 3)): A true or false CBOR simple value that

indicates whether a resident credential was created.

uvStatus (key type text string (major type 3)): An unsigned integer (major type 0) giving the

verification status of the operation. This field MUST be set to one of the following values:

Value

Meaning

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY
0

Use any verification requirement.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED
1

A verification is required.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED
2

A verification is preferred.

WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED
3

Verification is discouraged.

uvRetries (key type text string (major type 3)): An unsigned integer (major type 0) representing
the number of verification retries available for the authenticator.

credWithHmacSecretArray (key type text string (major type 3)): A byte string (major type 2)
containing hmacSecret extension response value.

17 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

thirdPartyPayment (key type: text string (major type 3)): A Boolean value (major type 7)
representing whether the credential created is applicable for third party payment.

transports (key type: text string (major type 3)): An unsigned integer (major type 0)

representing the transports supported by the credential.

Status (key type text string (major type 3)): An unsigned integer (major type 0) giving the status of

the overall operation.

Response (key type text string (major type 3)): Contains the response to the individual operation
corresponding to the individual WebAuthn command. The response consists of a single byte
indicating success or failure. A value of 0x00 indicates success. Any other value is an error. See
[FIDO-CTAP], section 6.3 for values and meaning. The single-byte code is followed by a CBOR
map containing details of the response. See section 2.2.2.1.1 and section 2.2.2.1.2 for the
response maps.

2.2.2.1.1 CTAP MakeCredential Response

This is the CBOR map used in the CTAPCBOR_RPC_COMMAND_WEB_AUTHN response map for a
response to a request to make a credential (see section 2.2.2.1).

See [FIDO-CTAP], section 6.2, for details of the map keys, values, and data types.

The following is a text representation of an example response:

 {
   1: "packed",
   2:
h'3D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB8834C500000003D8522D9F575B48668
8A9BA99FA02F35B00301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40
DDABF53D3222CEBEA861D4A50102032620012158201C1AEF38C820EE448C7BF0EE85912D0C4650F2A7B830431CBC1
3F49714D11FBA2258206A06795A704BD761068E13875D928FCDCC6C4E7F2AF4BCEC849098BCCF84F72DA368637265
64426C6F62F56B6372656450726F74656374036B686D61632D736563726574F5',
   3: {
     alg: -7,
     sig:
h'3045022100D8233DF9A938527864729037AD07DF2294F7CE764C886EF1EDC2F3AB1BD9CF6802201282A88B41A53
FB2B338CBFC4FFF713A0DAA097BA359E1E72B60D93983716E8F',
     x5c: [

h'308202D8308201C0A003020102020900FF876C2DAF7379C8300D06092A864886F70D01010B0500302E312C302A0
603550403132359756269636F2055324620526F6F742043412053657269616C203435373230303633313020170D31
34303830313030303030305A180F32303530303930343030303030305A306E310B300906035504061302534531123
010060355040A0C0959756269636F20414231223020060355040B0C1941757468656E74696361746F722041747465
73746174696F6E3127302506035504030C1E59756269636F205532462045452053657269616C20373632303837343
2333059301306072A8648CE3D020106082A8648CE3D0301070342000425F123A048283FC5796CCF887D99489FD935
C24198C4B5D8D5B2C2BFD7DD5D15AFE45B7070776567D5B5B0B23E04560B5BEA77B483B1F6491E53A3F2BEE6A39AA
38181307F3013060A2B0601040182C40A0D0104050403050506302206092B0601040182C40A020415312E332E362E
312E342E312E34313438322E312E393013060B2B0601040182E51C0201010404030205203021060B2B0601040182E
51C01010404120410D8522D9F575B486688A9BA99FA02F35B300C0603551D130101FF04023000300D06092A864886
F70D01010B0500038201010052B06949DBAAD1A64C1BA9EBC198B317EC31F9A37363BA5161B342E3A49CAD504F34E
7428BB896E9CFD28D03AD10CE325A06838E9B6C4ECB17AD40D090A16C9E7C34498332FF853B62747E8FCDF00DAE62
756E57BD40B16D677907A835C0435A2EBCE9B0B9069CA122BF9D964A73206AF74FF3C00144EBFF3DE7C7758D3147C
8C2F9FE87C12F2A9675A2046B01076361A99721871FA78FB0DE2945B579F9166C48AD2FD50C3CE56C8221A75083F6
56119394368FF17D2C920C63A09F01ED2501146B7DF1AB3970A2A32938FA9A517AF471085E160B3CA79764231746B
A6ABBA68E0D13CE259796BCD2A03AD83C74E15331328EAB438E6A4197CB12EC6FD1E388'
     ]
   },
   5: h'523FC0E9385A21D9A9A427B494014DDF3F9934838C05CD4C97BDF564FC3C8AC8'
 }

2.2.2.1.2 CTAP GetAssertion Response

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

18 / 41

This is the CBOR map used in the CTAPCBOR_RPC_COMMAND_WEB_AUTHN response map for a
response to a request to get an assertion (see section 2.2.2.1).

See [FIDO-CTAP], section 6.2, for details of the map keys, values, and data types.

The following is a text representation of an example response:

 {
   1: {
     id:
h'1C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA
861D4',
     type: "public-key"
   },
   2:
h'3D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB88348500000006A26863726564426C6
F6244313231326B686D61632D736563726574585051144A81FFFCED51BB81932851ABB84D17325C764C95C5016B5E
D0F80FA763728F7D410B39103FC502E115D7DAA326D6D0FA59A06BB20DFB4DA9AE31F1E3840D520FF29A01CD4B98D
7B555730C289673',
   3:
h'3045022033626FC0675341C2EBC5D5A049D739F517110F749685E12C3DABCBD2EB129E03022100D3CE29E77F8EB
B3A0B63D858E56ACE6B3A8702BF5113BBE7A081D352F844914F',
   4: {
     id: h'6D696B65406578616D706C652E636F6D'
   }
 }

2.2.2.2  CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS Response Map

This is a CBOR array which contains metadata about the credentials known to the platform for the
requested relying party

Individual credential metadata information is represented in a CBOR map with following fields.

Key (Unsigned Integer – CBOR Major Type 0)

Value

0

1

2

3

4

5

6

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

Version Information (Unsigned Integer.
CBOR Major Type 0).

Credential ID (Byte String. Major Type
2).

RP Entity Information.

User Entity Information.

Removable or not from the Authenticator.

Backed-up Credential or not. (Boolean.
CBOR Major Type 7).

Authenticator Name. (String. Major Type

19 / 41

Key (Unsigned Integer – CBOR Major Type 0)

7

8

9

Value

3).

Authenticator Logo.

Third Party Payment (Boolean. CBOR
Major Type 7).

Transports Supported. (Unsigned Integer.
CBOR Major Type 0).

The following is a text representation of an example response:

 [
   {
     0: 4,
     1: h'1C337E7A9F27200C98BE1E0337A501C02CBFA7215A85EA8CD9E187A1E97EFDDD',
     2: {
       id: "ctap.dev",
       name: "WebAuthn Test Server"
     },
     3: {
       id: h'626F62406578616D706C652E636F6D',
       name: "bob@example.com",
       displayName: "Bob Smith"
     },
     4: true,
     5: false,
     6: "Windows Hello",
     7:
h'3C7376672076696577426F783D22302030203339203339222066696C6C3D226E6F6E652220786D6C6E733D2
2687474703A2F2F7777772E77332E6F72672F323030302F737667223E3C726563742077696474683D22343922
206865696768743D223439222072783D2232222066696C6C3D227768697465222F3E3C6D61736B2069643D226
D61736B305F31313738315F31363230313422207374796C653D226D61736B2D747970653A6C756D696E616E63
6522206D61736B556E6974733D227573657253706163654F6E5573652220783D22382220793D2238222077696
474683D22333322206865696768743D223333223E3C7061746820643D224D3431203848385634314834315638
5A222066696C6C3D227768697465222F3E3C2F6D61736B3E3C67206D61736B3D2275726C28236D61736B305F3
1313738315F3136323031342922207472616E73666F726D3D227472616E736C617465282D352E352C2D352922
3E3C706174682066696C6C2D72756C653D226576656E6F64642220636C69702D72756C653D226576656E6F646
42220643D224D31372E3632352031362E32354331372E3632352031322E383332372032302E33393532203130
2E303632352032332E383132352031302E303632354332362E393137342031302E303632352032392E3438383
32031322E333439352032392E393332312031352E333330384333302E3533352031342E393734342033312E31
3837322031342E363932352033312E383736312031342E343937384333312E303732372031302E37383331203
2372E3736373620382032332E3831323520384331392E3235363120382031352E353632352031312E36393336
2031352E353632352031362E32354331352E353632352032302E383036342031392E323536312032342E35203
2332E383132352032342E354332342E353932312032342E352032352E333436352032342E333931392032362E
303631352032342E313839374332352E393339332032332E3632352032352E3837352032332E3033383820323
52E3837352032322E343337354332352E3837352032322E333138372032352E383737352032322E3230303420
32352E383832352032322E303832384332352E323335332032322E333132352032342E353338352032322E343
337352032332E383132352032322E343337354332302E333935322032322E343337352031372E363235203139
2E363637332031372E3632352031362E32355A4D31332E352032362E353632354832362E393738374332372E3
43237382032372E333338382032372E393939382032382E303335322032382E3636382032382E363235483133
2E354331322E3935332032382E3632352031322E343238342032382E383432342031322E303431362032392E3
23239314331312E363534382032392E363135382031312E343337352033302E313430352031312E3433373520
33302E363837354331312E343337352033322E3934382031322E333333372033342E393538332031342E32353
6322033362E343331384331362E323038322033372E393237392031392E333130342033382E39333735203233
2E383132352033382E393337354332362E323832372033382E393337352032382E333331342033382E3633333
52033302033382E313132315634302E323630324332382E323230392034302E373432362032362E3135383620
34312032332E383132352034314331392E303333342034312031352E343332342033392E393331382031332E3
03031362033382E303638384331302E353431332033362E3138333220392E3337352033332E3535323920392E

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

20 / 41

3337352033302E3638373543392E3337352032392E3539333520392E38303935392032382E353434332031302
E353833322032372E373730374331312E333536382032362E393937312031322E3430362032362E3536323520
31332E352032362E353632355A4D32382E323839362032342E34394332382E303631372032332E38343537203
2372E393337352032332E313532332032372E393337352032322E34334332372E393337352031392E30313639
2033302E373038332031362E32352033342E313235362031362E32354333372E353432382031362E323520343
02E333133312031392E303136392034302E333133312032322E34334334302E333133312032352E3138382033
382E353034312032372E353234312033362E303036362032382E333139324C33372E393433372033302E30303
4324333382E333438342033302E333536312033382E333438342033302E393833392033372E39343337203331
2E333335374C33352E313536392033332E37364C33372E393335372033362E3233344333382E3331323320333
62E353639332033382E333331392033372E313531322033372E393738382033372E353131314C33342E383134
312034302E373335324333342E343436332034312E313039392033332E383334342034312E303833332033332
E353030352034302E363738324C33322E323634362033392E313738364333322E313334332033392E30323036
2033322E303633312033382E383232322033322E303633312033382E363137344C33322E303632352033322E3
33531335632382E323538344333322E303034332032382E323337382033312E393436362032382E3231363220
33312E383839332032382E3139344333302E323133332032372E353434382032382E383930342032362E31383
7372032382E323839362032342E34395A4D33342E313235362032322E34334333352E323634372032322E3433
2033362E313838312032312E353037372033362E313838312032302E33374333362E313838312031392E32333
2332033352E323634372031382E33312033342E313235362031382E33314333322E393836352031382E333120
33322E303633312031392E323332332033322E303633312032302E33374333322E303633312032312E3530373
72033322E393836352032322E34332033342E313235362032322E34335A222066696C6C3D22626C61636B2220
66696C6C2D6F7061636974793D22302E38393536222F3E3C2F673E3C2F7376673E00',
     8: false,
     9: 16
   }

 ]

2.2.2.3  CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST Response Map

This is a CBOR array which contains metadata about the authenticator known to the platform for user
agents

Individual authenticator metadata information is represented in a CBOR map with following fields.

Key (Unsigned Integer – CBOR Major Type 0)

Value

1

2

3

4

5

The following is a text representation of an example response:

 [
   {
     1: 1,

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

Version Information (Unsigned Integer.
CBOR Major Type 0).

Authenticator ID (Byte String. Major Type
2).

Authenticator Name (String. Major Type
3).

Authenticator Logo (Byte String. Major
Type 2).

IsLocked. (Boolean. CBOR Major Type 7).

21 / 41

     2: h'08987058CADC4B81B6E130DE50DCBE96',
     3: "Windows Hello",
     4:
h'3C7376672069643D224C617965725F312220786D6C6E733D22687474703A2F2F7777772E77332E6F72672F3
23030302F737667222076696577426F783D223020302032353620323536223E3C646566733E3C7374796C653E
2E636C732D317B66696C6C3A233030373864343B7374726F6B652D77696474683A3070783B7D3C2F7374796C6
53E3C2F646566733E3C7265637420636C6173733D22636C732D312220783D2232342E32352220793D2232342E
3235222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C7265637420636C61737
33D22636C732D312220783D223133332E342220793D2232342E3235222077696474683D2239382E3335222068
65696768743D2239382E3335222F3E3C7265637420636C6173733D22636C732D312220783D2232342E3235222
0793D223133332E34222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C726563
7420636C6173733D22636C732D312220783D223133332E342220793D223133332E34222077696474683D22393
82E333522206865696768743D2239382E3335222F3E3C2F7376673E',
     5: false
   }

 ]

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

22 / 41

3  Protocol Details

3.1  Client and Server Details

The protocol has two main operations: MakeCredential (section 2.2.1.2) and GetAssertion (section
2.2.1.3). The MakeCredential operation registers a credential on the authenticator for the replying
party. The GetAssertion operation authentices the user to the relying party using the authenticator.

This protocol is designed to be closer to the WebAuthn layer rather than the CTAP layer. Individual
fields in the request map to the WebAuthn options defined at the WebAuthn layer.

Once a request is received by the client, the client determines which authenticator can satisfy the
request. The process of determining which authenticator supports particular capabilities and whether it
can satisfy a request are defined in the CTAP specification. See [FIDO-CTAP], sections 4 and 5.4.

3.1.1  Abstract Data Model

None.

3.1.2  Timers

None.

3.1.3  Initialization

None.

3.1.4  Higher-Layer Triggered Events

None.

3.1.5  Message Processing Events and Sequencing Rules

None.

3.1.6  Timer Events

None.

3.1.7  Other Local Events

None.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

23 / 41

4  Protocol Examples

The following sections provide examples of requests and responses for different commands.

4.1  CTAPCBOR_RPC_COMMAND_API_VERSION

4.1.1  Request

Complete request (a CBOR map):

 A467636F6D6D616E640565666C616773006774696D656F7574006D7472616E73616374696F6E49645000000000000
000000000000000000000

Textual representation of the CBOR encoding:

 {"command": 5, "flags": 0, "timeout": 0, "transactionId":
h'00000000000000000000000000000000'}

4.1.2  Response

Full response including HRESULT and API Version:

 0x00000004

The preceding response indicates a successful call (0x00000000 as first 32 bits) and that the API
version is 4.

4.2  CTAPCBOR_RPC_COMMAND_IUVPAA

4.2.1  Request

Full request (a CBOR map):

 0xA467636F6D6D616E640665666C616773006774696D656F7574006D7472616E73616374696F6E496450000000000
00000000000000000000000

Textual representation of the CBOR encoding:

 {"command": 6, "flags": 0, "timeout": 0, "transactionId":
h'00000000000000000000000000000000'}

4.2.2  Response

Full response including HRESULT and IUVPAA:

 0x00000001

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

24 / 41

4.3  CTAPCBOR_RPC_COMMAND_CANCEL_CUR_OP

4.3.1  Request

Full request which is a CBOR map:

 A567636F6D6D616E640765666C616773006774696D656F7574006772657175657374507FDFBDDF000000000000000
0000000006D7472616E73616374696F6E49645000000000000000000000000000000000

Textual representation of the CBOR map:

 {"command": 7, "flags": 0, "timeout": 0, "request": h'7FDFBDDF000000000000000000000000',
"transactionId": h'00000000000000000000000000000000'}

4.3.2  Response

Full RPC response includes only HRESULT

 0x0000

4.4  CTAPCBOR_RPC_COMMAND_WEB_AUTHN

4.4.1  CTAPCBOR_CMD_MAKE_CREDENTIAL

4.4.1.1  Request

Full request which is a CBOR map:

A967636F6D6D616E640565666C6167731A144000006774696D656F75741A000493E06D7472616E73616374696F6E4
9645026C98F33FCD61D4DBA5B27570F8293DF677265717565737459018401A701582019EEDF1F51A140B34B316293
B10C2BC0E6C53860771BB1DDA4A82E5DE94A4E7C02A262696468637461702E646576646E616D65745765624175746
86E20546573742053657276657203A3626964506D696B65406578616D706C652E636F6D646E616D65706D696B6540
6578616D706C652E636F6D6B646973706C61794E616D656C4D696B65204D61726C6F77650482A263616C672664747
970656A7075626C69632D6B6579A263616C6739010064747970656A7075626C69632D6B65790582A3626964501CA0
E7AAAB613DA3FAC3C76366A6046E64747970656A7075626C69632D6B65796A7472616E73706F72747301A36269645
02180DA5815A4443A91050803E93EF39A64747970656A7075626C69632D6B65796A7472616E73706F7274730106A5
6863726564426C6F624431323132696C61726765426C6F62A167737570706F7274697072656665727265646B63726
56450726F746563741910036C6C61726765426C6F624B6579F56C6D696E50696E4C656E677468F507A162726BF56C
776562417574684E50617261A963776E641A000303366A6174746163686D656E74026F72657175697265526573696
4656E74F56E7072656665725265736964656E74F47075736572566572696669636174696F6E017561747465737461
74696F6E507265666572656E63650375656E74657270726973654174746573746174696F6E006E63616E63656C6C6
174696F6E496450DAED4B74000000000000000000000000706C61726765426C6F62537570706F7274027566696C74
65724879627269645472616E73706F7274F471746869726450617274795061796D656E74F46E636C69656E7444617
4614A534F4E58F17B2274797065223A22776562617574686E2E637265617465222C226368616C6C656E6765223A22
6158774C66703935524A6D5F5F366E4D753442504C7152476D42453675504D75582D3170506B6542517841222C226
F726967696E223A2268747470733A2F2F637461702E646576222C2263726F73734F726967696E223A66616C73652C
226F746865725F6B6579735F63616E5F62655F61646465645F68657265223A22646F206E6F7420636F6D706172652
0636C69656E74446174614A534F4E20616761696E737420612074656D706C6174652E205365652068747470733A2F
2F676F6F2E676C2F796162506578227D

Text representation of the request:

 {
   command: 5,
   flags: 339738624,

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

25 / 41

   timeout: 300000,
   transactionId: h'26C98F33FCD61D4DBA5B27570F8293DF',
   request:
h'01A701582019EEDF1F51A140B34B316293B10C2BC0E6C53860771BB1DDA4A82E5DE94A4E7C02A26269646863746
1702E646576646E616D6574576562417574686E20546573742053657276657203A3626964506D696B65406578616D
706C652E636F6D646E616D65706D696B65406578616D706C652E636F6D6B646973706C61794E616D656C4D696B652
04D61726C6F77650482A263616C672664747970656A7075626C69632D6B6579A263616C6739010064747970656A70
75626C69632D6B65790582A3626964501CA0E7AAAB613DA3FAC3C76366A6046E64747970656A7075626C69632D6B6
5796A7472616E73706F72747301A3626964502180DA5815A4443A91050803E93EF39A64747970656A7075626C6963
2D6B65796A7472616E73706F7274730106A56863726564426C6F624431323132696C61726765426C6F62A16773757
0706F7274697072656665727265646B6372656450726F74656374026C6C61726765426C6F624B6579F56C6D696E50
696E4C656E677468F507A162726BF5',
   webAuthNPara: {
     wnd: 197430,
     attachment: 2,
     requireResident: true,
     preferResident: false,
     userVerification: 1,
     attestationPreference: 3,
     enterpriseAttestation: 0,
     cancellationId: h'DAED4B74000000000000000000000000',
     largeBlobSupport: 2
   },
   filterHybridTransport: false,
   thirdPartyPayment: false,
   clientDataJSON:
h'7B2274797065223A22776562617574686E2E637265617465222C226368616C6C656E6765223A226158774C66703
935524A6D5F5F366E4D753442504C7152476D42453675504D75582D3170506B6542517841222C226F726967696E22
3A2268747470733A2F2F637461702E646576222C2263726F73734F726967696E223A66616C73652C226F746865725
F6B6579735F63616E5F62655F61646465645F68657265223A22646F206E6F7420636F6D7061726520636C69656E74
446174614A534F4E20616761696E737420612074656D706C6174652E205365652068747470733A2F2F676F6F2E676
C2F796162506578227D'
 }

Inner MakeCredential request details after the command type(0x01):

 A701582019EEDF1F51A140B34B316293B10C2BC0E6C53860771BB1DDA4A82E5DE94A4E7C02A262696468637461702
E646576646E616D6574576562417574686E20546573742053657276657203A3626964506D696B65406578616D706C
652E636F6D646E616D65706D696B65406578616D706C652E636F6D6B646973706C61794E616D656C4D696B65204D6
1726C6F77650482A263616C672664747970656A7075626C69632D6B6579A263616C6739010064747970656A707562
6C69632D6B65790582A3626964501CA0E7AAAB613DA3FAC3C76366A6046E64747970656A7075626C69632D6B65796
A7472616E73706F72747301A3626964502180DA5815A4443A91050803E93EF39A64747970656A7075626C69632D6B
65796A7472616E73706F7274730106A56863726564426C6F624431323132696C61726765426C6F62A167737570706
F7274697072656665727265646B6372656450726F74656374026C6C61726765426C6F624B6579F56C6D696E50696E
4C656E677468F507A162726BF5

Textual representation of the inner map:

 {
   1: h'19EEDF1F51A140B34B316293B10C2BC0E6C53860771BB1DDA4A82E5DE94A4E7C',
   2: {
     id: "ctap.dev",
     name: "WebAuthn Test Server"
   },
   3: {
     id: h'6D696B65406578616D706C652E636F6D',
     name: "mike@example.com",
     displayName: "Mike Marlowe"
   },
   4: [
     {
       alg: -7,
       type: "public-key"
     },
     {

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

26 / 41

       alg: -257,
       type: "public-key"
     }
   ],
   5: [
     {
       id: h'1CA0E7AAAB613DA3FAC3C76366A6046E',
       type: "public-key",
       transports: 1
     },
     {
       id: h'2180DA5815A4443A91050803E93EF39A',
       type: "public-key",
       transports: 1
     }
   ],
   6: {
     credBlob: h'31323132',
     largeBlob: {
       support: "preferred"
     },
     credProtect: 2,
     largeBlobKey: true,
     minPinLength: true
   },
   7: {
     rk: true
   }
 }

4.4.1.2  Response

Full RPC response including the HRESULT and the CBOR map:

 0x0000A36A646576696365496E666FAE6A6D61784D736753697A651904B0781B6D617853657269616C697A65644C6
1726765426C6F6241727261791904006C70726F766964657254797065634869646C70726F76696465724E616D6578
184D6963726F736F66744374617048696450726F76696465726A6465766963655061746878505C5C3F5C686964237
669645F31303530267069645F3034303223382639336334326626302630303030237B34643165353562322D663136
662D313163662D383863622D3030313131313030303033307D6C6D616E7566616374757265726659756269636F677
0726F647563746C597562694B6579204649444F66616147756964509F2D52D85B57664888A9BA99FA02F35B6B7265
736964656E744B6579F5781A63726564656E7469616C4C697374496E646578506C75734F6E6520687576537461747
57301697576526574726965730371746869726450617274795061796D656E74F46A7472616E73706F727473016673
74617475730068726573706F6E736559044100A401667061636B65640258D93D3CA4E1D7E11F604B32D0EFE18B449
B057CC736ECE0BF820A42555426DB8834C500000003D8522D9F575B486688A9BA99FA02F35B00301C1AEF38C820EE
448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA861D4A5010203262
0012158201C1AEF38C820EE448C7BF0EE85912D0C4650F2A7B830431CBC13F49714D11FBA2258206A06795A704BD7
61068E13875D928FCDCC6C4E7F2AF4BCEC849098BCCF84F72DA36863726564426C6F62F56B6372656450726F74656
374036B686D61632D736563726574F503A363616C67266373696758473045022100D8233DF9A938527864729037AD
07DF2294F7CE764C886EF1EDC2F3AB1BD9CF6802201282A88B41A53FB2B338CBFC4FFF713A0DAA097BA359E1E72B6
0D93983716E8F63783563815902DC308202D8308201C0A003020102020900FF876C2DAF7379C8300D06092A864886
F70D01010B0500302E312C302A0603550403132359756269636F2055324620526F6F742043412053657269616C203
435373230303633313020170D3134303830313030303030305A180F32303530303930343030303030305A306E310B
300906035504061302534531123010060355040A0C0959756269636F20414231223020060355040B0C19417574686
56E74696361746F72204174746573746174696F6E3127302506035504030C1E59756269636F205532462045452053
657269616C203736323038373432333059301306072A8648CE3D020106082A8648CE3D0301070342000425F123A04
8283FC5796CCF887D99489FD935C24198C4B5D8D5B2C2BFD7DD5D15AFE45B7070776567D5B5B0B23E04560B5BEA77
B483B1F6491E53A3F2BEE6A39AA38181307F3013060A2B0601040182C40A0D0104050403050506302206092B06010
40182C40A020415312E332E362E312E342E312E34313438322E312E393013060B2B0601040182E51C020101040403
0205203021060B2B0601040182E51C01010404120410D8522D9F575B486688A9BA99FA02F35B300C0603551D13010
1FF04023000300D06092A864886F70D01010B0500038201010052B06949DBAAD1A64C1BA9EBC198B317EC31F9A373
63BA5161B342E3A49CAD504F34E7428BB896E9CFD28D03AD10CE325A06838E9B6C4ECB17AD40D090A16C9E7C34498
332FF853B62747E8FCDF00DAE62756E57BD40B16D677907A835C0435A2EBCE9B0B9069CA122BF9D964A73206AF74F
F3C00144EBFF3DE7C7758D3147C8C2F9FE87C12F2A9675A2046B01076361A99721871FA78FB0DE2945B579F9166C4
8AD2FD50C3CE56C8221A75083F656119394368FF17D2C920C63A09F01ED2501146B7DF1AB3970A2A32938FA9A517A

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

27 / 41

F471085E160B3CA79764231746BA6ABBA68E0D13CE259796BCD2A03AD83C74E15331328EAB438E6A4197CB12EC6FD
1E388055820523FC0E9385A21D9A9A427B494014DDF3F9934838C05CD4C97BDF564FC3C8AC8

Textual representation of the response following the HRESULT:

 {
   deviceInfo: {
     maxMsgSize: 1200,
     maxSerializedLargeBlobArray: 1024,
     providerType: "Hid",
     providerName: "MicrosoftCtapHidProvider",
     devicePath: "\\\\?\\hid#vid_1050&pid_0402#8&93c42f&0&0000#{4d1e55b2-f16f-11cf-88cb-
001111000030}",
     manufacturer: "Yubico",
     product: "YubiKey FIDO",
     aaGuid: h'9F2D52D85B57664888A9BA99FA02F35B',
     residentKey: true,
     credentialListIndexPlusOne: -1,
     uvStatus: 1,
     uvRetries: 3,
     thirdPartyPayment: false,
     transports: 1
   },
   status: 0,
   response:
h'00A401667061636B65640258D93D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB8834C
500000003D8522D9F575B486688A9BA99FA02F35B00301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028
E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA861D4A50102032620012158201C1AEF38C820EE448C7BF0EE8
5912D0C4650F2A7B830431CBC13F49714D11FBA2258206A06795A704BD761068E13875D928FCDCC6C4E7F2AF4BCEC
849098BCCF84F72DA36863726564426C6F62F56B6372656450726F74656374036B686D61632D736563726574F503A
363616C67266373696758473045022100D8233DF9A938527864729037AD07DF2294F7CE764C886EF1EDC2F3AB1BD9
CF6802201282A88B41A53FB2B338CBFC4FFF713A0DAA097BA359E1E72B60D93983716E8F63783563815902DC30820
2D8308201C0A003020102020900FF876C2DAF7379C8300D06092A864886F70D01010B0500302E312C302A06035504
03132359756269636F2055324620526F6F742043412053657269616C203435373230303633313020170D313430383
0313030303030305A180F32303530303930343030303030305A306E310B3009060355040613025345311230100603
55040A0C0959756269636F20414231223020060355040B0C1941757468656E74696361746F7220417474657374617
4696F6E3127302506035504030C1E59756269636F205532462045452053657269616C203736323038373432333059
301306072A8648CE3D020106082A8648CE3D0301070342000425F123A048283FC5796CCF887D99489FD935C24198C
4B5D8D5B2C2BFD7DD5D15AFE45B7070776567D5B5B0B23E04560B5BEA77B483B1F6491E53A3F2BEE6A39AA3818130
7F3013060A2B0601040182C40A0D0104050403050506302206092B0601040182C40A020415312E332E362E312E342
E312E34313438322E312E393013060B2B0601040182E51C0201010404030205203021060B2B0601040182E51C0101
0404120410D8522D9F575B486688A9BA99FA02F35B300C0603551D130101FF04023000300D06092A864886F70D010
10B0500038201010052B06949DBAAD1A64C1BA9EBC198B317EC31F9A37363BA5161B342E3A49CAD504F34E7428BB8
96E9CFD28D03AD10CE325A06838E9B6C4ECB17AD40D090A16C9E7C34498332FF853B62747E8FCDF00DAE62756E57B
D40B16D677907A835C0435A2EBCE9B0B9069CA122BF9D964A73206AF74FF3C00144EBFF3DE7C7758D3147C8C2F9FE
87C12F2A9675A2046B01076361A99721871FA78FB0DE2945B579F9166C48AD2FD50C3CE56C8221A75083F65611939
4368FF17D2C920C63A09F01ED2501146B7DF1AB3970A2A32938FA9A517AF471085E160B3CA79764231746BA6ABBA6
8E0D13CE259796BCD2A03AD83C74E15331328EAB438E6A4197CB12EC6FD1E388055820523FC0E9385A21D9A9A427B
494014DDF3F9934838C05CD4C97BDF564FC3C8AC8'
 }

Inner Authenticator response details after first byte indicating success(0x00):

 A401667061636B65640258D93D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB8834C5000
00003D8522D9F575B486688A9BA99FA02F35B00301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E142
70EB5AD74D2383EE72AC40DDABF53D3222CEBEA861D4A50102032620012158201C1AEF38C820EE448C7BF0EE85912
D0C4650F2A7B830431CBC13F49714D11FBA2258206A06795A704BD761068E13875D928FCDCC6C4E7F2AF4BCEC8490
98BCCF84F72DA36863726564426C6F62F56B6372656450726F74656374036B686D61632D736563726574F503A3636
16C67266373696758473045022100D8233DF9A938527864729037AD07DF2294F7CE764C886EF1EDC2F3AB1BD9CF68
02201282A88B41A53FB2B338CBFC4FFF713A0DAA097BA359E1E72B60D93983716E8F63783563815902DC308202D83
08201C0A003020102020900FF876C2DAF7379C8300D06092A864886F70D01010B0500302E312C302A060355040313
2359756269636F2055324620526F6F742043412053657269616C203435373230303633313020170D3134303830313
030303030305A180F32303530303930343030303030305A306E310B30090603550406130253453112301006035504
0A0C0959756269636F20414231223020060355040B0C1941757468656E74696361746F72204174746573746174696
F6E3127302506035504030C1E59756269636F205532462045452053657269616C2037363230383734323330593013

28 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

06072A8648CE3D020106082A8648CE3D0301070342000425F123A048283FC5796CCF887D99489FD935C24198C4B5D
8D5B2C2BFD7DD5D15AFE45B7070776567D5B5B0B23E04560B5BEA77B483B1F6491E53A3F2BEE6A39AA38181307F30
13060A2B0601040182C40A0D0104050403050506302206092B0601040182C40A020415312E332E362E312E342E312
E34313438322E312E393013060B2B0601040182E51C0201010404030205203021060B2B0601040182E51C01010404
120410D8522D9F575B486688A9BA99FA02F35B300C0603551D130101FF04023000300D06092A864886F70D01010B0
500038201010052B06949DBAAD1A64C1BA9EBC198B317EC31F9A37363BA5161B342E3A49CAD504F34E7428BB896E9
CFD28D03AD10CE325A06838E9B6C4ECB17AD40D090A16C9E7C34498332FF853B62747E8FCDF00DAE62756E57BD40B
16D677907A835C0435A2EBCE9B0B9069CA122BF9D964A73206AF74FF3C00144EBFF3DE7C7758D3147C8C2F9FE87C1
2F2A9675A2046B01076361A99721871FA78FB0DE2945B579F9166C48AD2FD50C3CE56C8221A75083F656119394368
FF17D2C920C63A09F01ED2501146B7DF1AB3970A2A32938FA9A517AF471085E160B3CA79764231746BA6ABBA68E0D
13CE259796BCD2A03AD83C74E15331328EAB438E6A4197CB12EC6FD1E388055820523FC0E9385A21D9A9A427B4940
14DDF3F9934838C05CD4C97BDF564FC3C8AC8

Textual representation of the inner Authenticator response:

 {
   1: "packed",
   2:
h'3D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB8834C500000003D8522D9F575B4
86688A9BA99FA02F35B00301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383
EE72AC40DDABF53D3222CEBEA861D4A50102032620012158201C1AEF38C820EE448C7BF0EE85912D0C4650F2A
7B830431CBC13F49714D11FBA2258206A06795A704BD761068E13875D928FCDCC6C4E7F2AF4BCEC849098BCCF
84F72DA36863726564426C6F62F56B6372656450726F74656374036B686D61632D736563726574F5',
   3: {
     alg: -7,
     sig:
h'3045022100D8233DF9A938527864729037AD07DF2294F7CE764C886EF1EDC2F3AB1BD9CF6802201282A88B4
1A53FB2B338CBFC4FFF713A0DAA097BA359E1E72B60D93983716E8F',
     x5c: [

h'308202D8308201C0A003020102020900FF876C2DAF7379C8300D06092A864886F70D01010B0500302E312C3
02A0603550403132359756269636F2055324620526F6F742043412053657269616C2034353732303036333130
20170D3134303830313030303030305A180F32303530303930343030303030305A306E310B300906035504061
302534531123010060355040A0C0959756269636F20414231223020060355040B0C1941757468656E74696361
746F72204174746573746174696F6E3127302506035504030C1E59756269636F2055324620454520536572696
16C203736323038373432333059301306072A8648CE3D020106082A8648CE3D0301070342000425F123A04828
3FC5796CCF887D99489FD935C24198C4B5D8D5B2C2BFD7DD5D15AFE45B7070776567D5B5B0B23E04560B5BEA7
7B483B1F6491E53A3F2BEE6A39AA38181307F3013060A2B0601040182C40A0D0104050403050506302206092B
0601040182C40A020415312E332E362E312E342E312E34313438322E312E393013060B2B0601040182E51C020
1010404030205203021060B2B0601040182E51C01010404120410D8522D9F575B486688A9BA99FA02F35B300C
0603551D130101FF04023000300D06092A864886F70D01010B0500038201010052B06949DBAAD1A64C1BA9EBC
198B317EC31F9A37363BA5161B342E3A49CAD504F34E7428BB896E9CFD28D03AD10CE325A06838E9B6C4ECB17
AD40D090A16C9E7C34498332FF853B62747E8FCDF00DAE62756E57BD40B16D677907A835C0435A2EBCE9B0B90
69CA122BF9D964A73206AF74FF3C00144EBFF3DE7C7758D3147C8C2F9FE87C12F2A9675A2046B01076361A997
21871FA78FB0DE2945B579F9166C48AD2FD50C3CE56C8221A75083F656119394368FF17D2C920C63A09F01ED2
501146B7DF1AB3970A2A32938FA9A517AF471085E160B3CA79764231746BA6ABBA68E0D13CE259796BCD2A03A
D83C74E15331328EAB438E6A4197CB12EC6FD1E388'
     ]
   },
   5: h'523FC0E9385A21D9A9A427B494014DDF3F9934838C05CD4C97BDF564FC3C8AC8'

 }

4.4.2  CTAPCBOR_CMD_GET_ASSERTION

4.4.2.1  Request

Full request, a CBOR map:

 A967636F6D6D616E640565666C6167731A004000006774696D656F75741A000493E06D7472616E73616374696F6E4
964509C6904B32DBF974E96254E9B65D0AF8E677265717565737458BF02A50168637461702E6465760258206F43BB
640674BE2C2CC33AD6F0F08E450DC035F2113D1FB38A6985E32D3CB14F0382A262696458301C1AEF38C820EE448C7

29 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA861D464747970656A7075
626C69632D6B6579A26269645820BC365B699040F8B4819833CF2F0FB4733BD424A3B043913C8CF5527799F4373A6
4747970656A7075626C69632D6B657904A16863726564426C6F62F505A1627570F56C776562417574684E50617261
A863776E641A000303366A6174746163686D656E74006F726571756972655265736964656E74F46E7072656665725
265736964656E74F47075736572566572696669636174696F6E01756174746573746174696F6E507265666572656E
63650075656E74657270726973654174746573746174696F6E006E63616E63656C6C6174696F6E4964501D33C40F0
0000000000000000000000074686D616353656372657453616C7456616C756573590101A10282A20158301C1AEF38
C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA861D402A26
5666972737458200000000000000000000000000000000000000000000000000000000000000000667365636F6E64
58200000000000000000000000000000000000000000000000000000000000000000A2015820BC365B699040F8B48
19833CF2F0FB4733BD424A3B043913C8CF5527799F4373A02A2656669727374582000000000000000000000000000
00000000000000000000000000000000000000667365636F6E6458200000000000000000000000000000000000000
0000000000000000000000000007566696C7465724879627269645472616E73706F7274F56E636C69656E74446174
614A534F4E58817B2274797065223A22776562617574686E2E676574222C226368616C6C656E6765223A2250755F3
865394D4C4F6245415459454E396841765F4F57435A7567594C706E4C6B464B46684C3478486949222C226F726967
696E223A2268747470733A2F2F637461702E646576222C2263726F73734F726967696E223A66616C73657D

Textual representation of the request:

 {
   command: 5,
   flags: 4194304,
   timeout: 300000,
   transactionId: h'9C6904B32DBF974E96254E9B65D0AF8E',
   request:
h'02A50168637461702E6465760258206F43BB640674BE2C2CC33AD6F0F08E450DC035F2113D1FB38A6985E32D3CB
14F0382A262696458301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40
DDABF53D3222CEBEA861D464747970656A7075626C69632D6B6579A26269645820BC365B699040F8B4819833CF2F0
FB4733BD424A3B043913C8CF5527799F4373A64747970656A7075626C69632D6B657904A16863726564426C6F62F5
05A1627570F5',
   webAuthNPara: {
     wnd: 197430,
     attachment: 0,
     requireResident: false,
     preferResident: false,
     userVerification: 1,
     attestationPreference: 0,
     enterpriseAttestation: 0,
     cancellationId: h'1D33C40F000000000000000000000000'
   },
   hmacSecretSaltValues:
h'A10282A20158301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDA
BF53D3222CEBEA861D402A26566697273745820000000000000000000000000000000000000000000000000000000
0000000000667365636F6E6458200000000000000000000000000000000000000000000000000000000000000000A
2015820BC365B699040F8B4819833CF2F0FB4733BD424A3B043913C8CF5527799F4373A02A2656669727374582000
00000000000000000000000000000000000000000000000000000000000000667365636F6E6458200000000000000
000000000000000000000000000000000000000000000000000',
   filterHybridTransport: true,
   clientDataJSON:
h'7B2274797065223A22776562617574686E2E676574222C226368616C6C656E6765223A2250755F3865394D4C4F6
245415459454E396841765F4F57435A7567594C706E4C6B464B46684C3478486949222C226F726967696E223A2268
747470733A2F2F637461702E646576222C2263726F73734F726967696E223A66616C73657D'
 }

Inner GetAssertion request details following the command type(0x02):

 A50168637461702E6465760258206F43BB640674BE2C2CC33AD6F0F08E450DC035F2113D1FB38A6985E32D3CB14F0
382A262696458301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDAB
F53D3222CEBEA861D464747970656A7075626C69632D6B6579A26269645820BC365B699040F8B4819833CF2F0FB47
33BD424A3B043913C8CF5527799F4373A64747970656A7075626C69632D6B657904A16863726564426C6F62F505A1
627570F5

Textual representation of the preceding GetAssertion request:

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

30 / 41

 {
   1: "ctap.dev",
   2: h'6F43BB640674BE2C2CC33AD6F0F08E450DC035F2113D1FB38A6985E32D3CB14F',
   3: [
     {
       id:
h'1C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA
861D4',
       type: "public-key"
     },
     {
       id: h'BC365B699040F8B4819833CF2F0FB4733BD424A3B043913C8CF5527799F4373A',
       type: "public-key"
     }
   ],
   4: {
     credBlob: true
   },
   5: {
     up: true
   }
 }

4.4.2.2  Response

Full response including the HRESULT and the CBOR map:

 0x0000A36A646576696365496E666FAB6A6D61784D736753697A651904B0781B6D617853657269616C697A65644C6
1726765426C6F6241727261791904006C70726F766964657254797065634869646C70726F76696465724E616D6578
184D6963726F736F66744374617048696450726F76696465726A6465766963655061746878525C5C3F5C686964237
669645F31303530267069645F30343032233726333131333036393426312630303030237B34643165353562322D66
3136662D313163662D383863622D3030313131313030303033307D6C6D616E7566616374757265726659756269636
F6770726F647563746C597562694B6579204649444F66616147756964509F2D52D85B57664888A9BA99FA02F35B78
1A63726564656E7469616C4C697374496E646578506C75734F6E65026875765374617475730169757652657472696
57303667374617475730068726573706F6E736558D100A401A26269645830FFE2DC7BB7DFD9C8C268C45DD339BB4F
187E4F33B96B2B02551FFBC264BD325BC9345A27D97F581B422370AC2C9784BB64747970656A7075626C69632D6B6
579025825E45329D03A2068D1CAF7F7BB0AE954E6B0E6259745F32F4829F750F05011F9C205000000090358483046
02210099C54C2075B88279DE38944F5492E21DA2F5E1969BCCDD99A97F39BDDD844A78022100BF091FBE45242DF48
4D7396D5304A570A78F2E5576ECC8AB24107E51968AD1C404A16269644F626F62406578616D706C652E636F6D

Textual representation of the CBOR map following the HRESULT:

 {
   deviceInfo: {
     maxMsgSize: 1200,
     maxSerializedLargeBlobArray: 1024,
     providerType: "Hid",
     providerName: "MicrosoftCtapHidProvider",
     devicePath: "\\\\?\\hid#vid_1050&pid_0402#8&93c42f&0&0000#{4d1e55b2-f16f-11cf-88cb-
001111000030}",
     manufacturer: "Yubico",
     product: "YubiKey FIDO",
     aaGuid: h'9F2D52D85B57664888A9BA99FA02F35B',
     credentialListIndexPlusOne: 1,
     credWithHmacSecretArray:
h'81A20158301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53
D3222CEBEA861D402A265666972737458200000000000000000000000000000000000000000000000000000000000
000000667365636F6E6458200000000000000000000000000000000000000000000000000000000000000000',
     uvStatus: 1,
     uvRetries: 2,
     thirdPartyPayment: false,
     transports: 0
   },
   status: 0,

31 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

   response:
h'00A401A262696458301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC4
0DDABF53D3222CEBEA861D464747970656A7075626C69632D6B65790258923D3CA4E1D7E11F604B32D0EFE18B449B
057CC736ECE0BF820A42555426DB88348500000006A26863726564426C6F6244313231326B686D61632D736563726
574585051144A81FFFCED51BB81932851ABB84D17325C764C95C5016B5ED0F80FA763728F7D410B39103FC502E115
D7DAA326D6D0FA59A06BB20DFB4DA9AE31F1E3840D520FF29A01CD4B98D7B555730C2896730358473045022033626
FC0675341C2EBC5D5A049D739F517110F749685E12C3DABCBD2EB129E03022100D3CE29E77F8EBB3A0B63D858E56A
CE6B3A8702BF5113BBE7A081D352F844914F04A1626964506D696B65406578616D706C652E636F6D'
 }

Inner Authenticator response details following the first byte indicating success(0x00):

 A401A262696458301C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDA
BF53D3222CEBEA861D464747970656A7075626C69632D6B65790258923D3CA4E1D7E11F604B32D0EFE18B449B057C
C736ECE0BF820A42555426DB88348500000006A26863726564426C6F6244313231326B686D61632D7365637265745
85051144A81FFFCED51BB81932851ABB84D17325C764C95C5016B5ED0F80FA763728F7D410B39103FC502E115D7DA
A326D6D0FA59A06BB20DFB4DA9AE31F1E3840D520FF29A01CD4B98D7B555730C2896730358473045022033626FC06
75341C2EBC5D5A049D739F517110F749685E12C3DABCBD2EB129E03022100D3CE29E77F8EBB3A0B63D858E56ACE6B
3A8702BF5113BBE7A081D352F844914F04A1626964506D696B65406578616D706C652E636F6D

Textual representation of the preceding map:

 {
   1: {
     id:
h'1C1AEF38C820EE448C7BF0EE8512BF9A813A96B73019E028E14270EB5AD74D2383EE72AC40DDABF53D3222CEBEA
861D4',
     type: "public-key"
   },
   2:
h'3D3CA4E1D7E11F604B32D0EFE18B449B057CC736ECE0BF820A42555426DB88348500000006A26863726564426C6
F6244313231326B686D61632D736563726574585051144A81FFFCED51BB81932851ABB84D17325C764C95C5016B5E
D0F80FA763728F7D410B39103FC502E115D7DAA326D6D0FA59A06BB20DFB4DA9AE31F1E3840D520FF29A01CD4B98D
7B555730C289673',
   3:
h'3045022033626FC0675341C2EBC5D5A049D739F517110F749685E12C3DABCBD2EB129E03022100D3CE29E77F8EB
B3A0B63D858E56ACE6B3A8702BF5113BBE7A081D352F844914F',
   4: {
     id: h'6D696B65406578616D706C652E636F6D'
   }
 }

4.5  CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS

4.5.1  Request

Full Request (a CBOR map):

 0xAA67636F6D6D616E640965666C616773006774696D656F7574006D7472616E73616374696F6E496450D68633AC1
123364E9709E201FD05693C7566696C7465724879627269645472616E73706F7274F4647270496468637461702E64
6576782061757468656E74696361746F72496E666F4C6F676F526571756573745479706500781F706C7567696E417
5746F66696C6C5363656E6172696F537570706F72746564F46F75765472616E73616374696F6E4964500000000000
000000000000000000000071746869726450617274795061796D656E74F4

Textual representation of the CBOR encoding:

 {
   command: 9,

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

32 / 41

   flags: 0,
   timeout: 0,
   transactionId: h'D68633AC1123364E9709E201FD05693C',
   filterHybridTransport: false,
   rpId: "ctap.dev",
   authenticatorInfoLogoRequestType: 0,
   pluginAutofillScenarioSupported: false,
   uvTransactionId: h'00000000000000000000000000000000',
   thirdPartyPayment: false
 }

4.5.2  Response

Full Response (a CBOR map):

 0x000081AA00040158201C337E7A9F27200C98BE1E0337A501C02CBFA7215A85EA8CD9E187A1E97EFDDD02A26
2696468637461702E646576646E616D6574576562417574686E20546573742053657276657203A36269644F62
6F62406578616D706C652E636F6D646E616D656F626F62406578616D706C652E636F6D6B646973706C61794E6
16D6569426F6220536D69746804F505F4066D57696E646F77732048656C6C6F075908D23C7376672076696577
426F783D22302030203339203339222066696C6C3D226E6F6E652220786D6C6E733D22687474703A2F2F77777
72E77332E6F72672F323030302F737667223E3C726563742077696474683D22343922206865696768743D2234
39222072783D2232222066696C6C3D227768697465222F3E3C6D61736B2069643D226D61736B305F313137383
15F31363230313422207374796C653D226D61736B2D747970653A6C756D696E616E636522206D61736B556E69
74733D227573657253706163654F6E5573652220783D22382220793D2238222077696474683D2233332220686
5696768743D223333223E3C7061746820643D224D34312038483856343148343156385A222066696C6C3D2277
68697465222F3E3C2F6D61736B3E3C67206D61736B3D2275726C28236D61736B305F31313738315F313632303
1342922207472616E73666F726D3D227472616E736C617465282D352E352C2D3529223E3C706174682066696C
6C2D72756C653D226576656E6F64642220636C69702D72756C653D226576656E6F64642220643D224D31372E3
632352031362E32354331372E3632352031322E383332372032302E333935322031302E303632352032332E38
3132352031302E303632354332362E393137342031302E303632352032392E343838332031322E33343935203
2392E393332312031352E333330384333302E3533352031342E393734342033312E313837322031342E363932
352033312E383736312031342E343937384333312E303732372031302E373833312032372E373637362038203
2332E3831323520384331392E3235363120382031352E353632352031312E363933362031352E353632352031
362E32354331352E353632352032302E383036342031392E323536312032342E352032332E383132352032342
E354332342E353932312032342E352032352E333436352032342E333931392032362E303631352032342E3138
39374332352E393339332032332E3632352032352E3837352032332E303338382032352E3837352032322E343
337354332352E3837352032322E333138372032352E383737352032322E323030342032352E38383235203232
2E303832384332352E323335332032322E333132352032342E353338352032322E343337352032332E3831323
52032322E343337354332302E333935322032322E343337352031372E3632352031392E363637332031372E36
32352031362E32355A4D31332E352032362E353632354832362E393738374332372E343237382032372E33333
8382032372E393939382032382E303335322032382E3636382032382E3632354831332E354331322E39353320
32382E3632352031322E343238342032382E383432342031322E303431362032392E323239314331312E36353
4382032392E363135382031312E343337352033302E313430352031312E343337352033302E36383735433131
2E343337352033322E3934382031322E333333372033342E393538332031342E323536322033362E343331384
331362E323038322033372E393237392031392E333130342033382E393337352032332E383132352033382E39
3337354332362E323832372033382E393337352032382E333331342033382E363333352033302033382E31313
2315634302E323630324332382E323230392034302E373432362032362E313538362034312032332E38313235
2034314331392E303333342034312031352E343332342033392E393331382031332E303031362033382E30363
8384331302E353431332033362E3138333220392E3337352033332E3535323920392E3337352033302E363837
3543392E3337352032392E3539333520392E38303935392032382E353434332031302E353833322032372E373
730374331312E333536382032362E393937312031322E3430362032362E353632352031332E352032362E3536
32355A4D32382E323839362032342E34394332382E303631372032332E383435372032372E393337352032332
E313532332032372E393337352032322E34334332372E393337352031392E303136392033302E373038332031
362E32352033342E313235362031362E32354333372E353432382031362E32352034302E333133312031392E3
03136392034302E333133312032322E34334334302E333133312032352E3138382033382E353034312032372E
353234312033362E303036362032382E333139324C33372E393433372033302E303034324333382E333438342
033302E333536312033382E333438342033302E393833392033372E393433372033312E333335374C33352E31
3536392033332E37364C33372E393335372033362E3233344333382E333132332033362E353639332033382E3
33331392033372E313531322033372E393738382033372E353131314C33342E383134312034302E3733353243
33342E343436332034312E313039392033332E383334342034312E303833332033332E353030352034302E363
738324C33322E323634362033392E313738364333322E313334332033392E303230362033322E303633312033
382E383232322033322E303633312033382E363137344C33322E303632352033322E333531335632382E32353
8344333322E303034332032382E323337382033312E393436362032382E323136322033312E38383933203238
2E3139344333302E323133332032372E353434382032382E383930342032362E313837372032382E323839362
032342E34395A4D33342E313235362032322E34334333352E323634372032322E34332033362E313838312032

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

33 / 41

312E353037372033362E313838312032302E33374333362E313838312031392E323332332033352E323634372
031382E33312033342E313235362031382E33314333322E393836352031382E33312033322E30363331203139
2E323332332033322E303633312032302E33374333322E303633312032312E353037372033322E39383635203
2322E34332033342E313235362032322E34335A222066696C6C3D22626C61636B222066696C6C2D6F70616369
74793D22302E38393536222F3E3C2F673E3C2F7376673E0008F40910

Textual representation of the CBOR encoding after the HRESULT:

 [
   {
     0: 4,
     1: h'1C337E7A9F27200C98BE1E0337A501C02CBFA7215A85EA8CD9E187A1E97EFDDD',
     2: {
       id: "ctap.dev",
       name: "WebAuthn Test Server"
     },
     3: {
       id: h'626F62406578616D706C652E636F6D',
       name: "bob@example.com",
       displayName: "Bob Smith"
     },
     4: true,
     5: false,
     6: "Windows Hello",
     7:
h'3C7376672076696577426F783D22302030203339203339222066696C6C3D226E6F6E652220786D6C6E733D2
2687474703A2F2F7777772E77332E6F72672F323030302F737667223E3C726563742077696474683D22343922
206865696768743D223439222072783D2232222066696C6C3D227768697465222F3E3C6D61736B2069643D226
D61736B305F31313738315F31363230313422207374796C653D226D61736B2D747970653A6C756D696E616E63
6522206D61736B556E6974733D227573657253706163654F6E5573652220783D22382220793D2238222077696
474683D22333322206865696768743D223333223E3C7061746820643D224D3431203848385634314834315638
5A222066696C6C3D227768697465222F3E3C2F6D61736B3E3C67206D61736B3D2275726C28236D61736B305F3
1313738315F3136323031342922207472616E73666F726D3D227472616E736C617465282D352E352C2D352922
3E3C706174682066696C6C2D72756C653D226576656E6F64642220636C69702D72756C653D226576656E6F646
42220643D224D31372E3632352031362E32354331372E3632352031322E383332372032302E33393532203130
2E303632352032332E383132352031302E303632354332362E393137342031302E303632352032392E3438383
32031322E333439352032392E393332312031352E333330384333302E3533352031342E393734342033312E31
3837322031342E363932352033312E383736312031342E343937384333312E303732372031302E37383331203
2372E3736373620382032332E3831323520384331392E3235363120382031352E353632352031312E36393336
2031352E353632352031362E32354331352E353632352032302E383036342031392E323536312032342E35203
2332E383132352032342E354332342E353932312032342E352032352E333436352032342E333931392032362E
303631352032342E313839374332352E393339332032332E3632352032352E3837352032332E3033383820323
52E3837352032322E343337354332352E3837352032322E333138372032352E383737352032322E3230303420
32352E383832352032322E303832384332352E323335332032322E333132352032342E353338352032322E343
337352032332E383132352032322E343337354332302E333935322032322E343337352031372E363235203139
2E363637332031372E3632352031362E32355A4D31332E352032362E353632354832362E393738374332372E3
43237382032372E333338382032372E393939382032382E303335322032382E3636382032382E363235483133
2E354331322E3935332032382E3632352031322E343238342032382E383432342031322E303431362032392E3
23239314331312E363534382032392E363135382031312E343337352033302E313430352031312E3433373520
33302E363837354331312E343337352033322E3934382031322E333333372033342E393538332031342E32353
6322033362E343331384331362E323038322033372E393237392031392E333130342033382E39333735203233
2E383132352033382E393337354332362E323832372033382E393337352032382E333331342033382E3633333
52033302033382E313132315634302E323630324332382E323230392034302E373432362032362E3135383620
34312032332E383132352034314331392E303333342034312031352E343332342033392E393331382031332E3
03031362033382E303638384331302E353431332033362E3138333220392E3337352033332E3535323920392E
3337352033302E3638373543392E3337352032392E3539333520392E38303935392032382E353434332031302
E353833322032372E373730374331312E333536382032362E393937312031322E3430362032362E3536323520
31332E352032362E353632355A4D32382E323839362032342E34394332382E303631372032332E38343537203
2372E393337352032332E313532332032372E393337352032322E34334332372E393337352031392E30313639
2033302E373038332031362E32352033342E313235362031362E32354333372E353432382031362E323520343
02E333133312031392E303136392034302E333133312032322E34334334302E333133312032352E3138382033
382E353034312032372E353234312033362E303036362032382E333139324C33372E393433372033302E30303
4324333382E333438342033302E333536312033382E333438342033302E393833392033372E39343337203331
2E333335374C33352E313536392033332E37364C33372E393335372033362E3233344333382E3331323320333
62E353639332033382E333331392033372E313531322033372E393738382033372E353131314C33342E383134
312034302E373335324333342E343436332034312E313039392033332E383334342034312E303833332033332
E353030352034302E363738324C33322E323634362033392E313738364333322E313334332033392E30323036
2033322E303633312033382E383232322033322E303633312033382E363137344C33322E303632352033322E3

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

34 / 41

33531335632382E323538344333322E303034332032382E323337382033312E393436362032382E3231363220
33312E383839332032382E3139344333302E323133332032372E353434382032382E383930342032362E31383
7372032382E323839362032342E34395A4D33342E313235362032322E34334333352E323634372032322E3433
2033362E313838312032312E353037372033362E313838312032302E33374333362E313838312031392E32333
2332033352E323634372031382E33312033342E313235362031382E33314333322E393836352031382E333120
33322E303633312031392E323332332033322E303633312032302E33374333322E303633312032312E3530373
72033322E393836352032322E34332033342E313235362032322E34335A222066696C6C3D22626C61636B2220
66696C6C2D6F7061636974793D22302E38393536222F3E3C2F673E3C2F7376673E00',
     8: false,
     9: 16
   }

 ]

4.6  CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST

4.6.1  Request

Full Request (a CBOR map):

 A667636F6D6D616E640C65666C616773006774696D656F7574006D7472616E73616374696F6E4964500000000
0000000000000000000000000782061757468656E74696361746F72496E666F4C6F676F526571756573745479
70650171746869726450617274795061796D656E74F4

Textual representation of the CBOR encoding:

 {
   command: 12,
   flags: 0,
   timeout: 0,
   transactionId: h'00000000000000000000000000000000',
   authenticatorInfoLogoRequestType: 1,
   thirdPartyPayment: false
 }

4.6.2  Response

Full Response (a CBOR map):

 0x000081A50101025008987058CADC4B81B6E130DE50DCBE96036D57696E646F77732048656C6C6F045901AB3
C7376672069643D224C617965725F312220786D6C6E733D22687474703A2F2F7777772E77332E6F72672F3230
30302F737667222076696577426F783D223020302032353620323536223E3C646566733E3C7374796C653E2E6
36C732D317B66696C6C3A233030373864343B7374726F6B652D77696474683A3070783B7D3C2F7374796C653E
3C2F646566733E3C7265637420636C6173733D22636C732D312220783D2232342E32352220793D2232342E323
5222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C7265637420636C6173733D
22636C732D312220783D223133332E342220793D2232342E3235222077696474683D2239382E3335222068656
96768743D2239382E3335222F3E3C7265637420636C6173733D22636C732D312220783D2232342E3235222079
3D223133332E34222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C726563742
0636C6173733D22636C732D312220783D223133332E342220793D223133332E34222077696474683D2239382E
333522206865696768743D2239382E3335222F3E3C2F7376673E05F4

Textual representation of the CBOR encoding after the HRESULT:

 [
   {
     1: 1,

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

35 / 41

     2: h'08987058CADC4B81B6E130DE50DCBE96',
     3: "Windows Hello",
     4:
h'3C7376672069643D224C617965725F312220786D6C6E733D22687474703A2F2F7777772E77332E6F72672F3
23030302F737667222076696577426F783D223020302032353620323536223E3C646566733E3C7374796C653E
2E636C732D317B66696C6C3A233030373864343B7374726F6B652D77696474683A3070783B7D3C2F7374796C6
53E3C2F646566733E3C7265637420636C6173733D22636C732D312220783D2232342E32352220793D2232342E
3235222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C7265637420636C61737
33D22636C732D312220783D223133332E342220793D2232342E3235222077696474683D2239382E3335222068
65696768743D2239382E3335222F3E3C7265637420636C6173733D22636C732D312220783D2232342E3235222
0793D223133332E34222077696474683D2239382E333522206865696768743D2239382E3335222F3E3C726563
7420636C6173733D22636C732D312220783D223133332E342220793D223133332E34222077696474683D22393
82E333522206865696768743D2239382E3335222F3E3C2F7376673E',
     5: false
   }

 ]

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

36 / 41

5  Security

5.1  Security Considerations for Implementers

For information, see [W3C-WebAuthPKC3], section 13.

5.2  Index of Security Parameters

None.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

37 / 41

6  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

The terms "earlier" and "later", when used with a product version, refer to either all preceding
versions or all subsequent versions, respectively. The term "through" refers to the inclusive range of
versions. Applicable Microsoft products are listed chronologically in this section.

Windows Client

  Windows 10 v1809 operating system

  Windows 11 operating system

Windows Server

  Windows Server v1809 operating system

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

<1> Section 2.2.1:  This is the Microsoft Windows API version on Windows systems. See
WebAuthNGetApiVersionNumber in [MSFT-WebAuthnAPIS].

<2> Section 2.2.1:  Windows 11, version 24H2 operating system and later and Windows Server 2025
and later with [MSKB-5065789] support CTAPCBOR_RPC_COMMAND_GET_CREDENTIALS.

<3> Section 2.2.1:  Windows 11, version 24H2 and later and Windows Server 2025 and later with
[MSKB-5065789] support CTAPCBOR_RPC_COMMAND_GET_AUTHENTICATOR_LIST.

<4> Section 2.2.1.1:  For example, Windows Hello on Windows systems.

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

38 / 41

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

2.2.1 WebAuthN_Channel Request Message

2.2.1 WebAuthN_Channel Request Message

2.2.1 WebAuthN_Channel Request Message

2.2.1 WebAuthN_Channel Request Message

2.2.1 WebAuthN_Channel Request Message

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

2.2.1 WebAuthN_Channel Request Message

Added new fields to WebAuthN_Channel
Request Message.

2.2.1.1 webAuthNPara Map

Added new fields to webAuthNPara Map.

2.2.1.2 CTAPCBOR_CMD_MAKE_CREDENTIAL

Updated request example.

Revisi
on
class

Major

Major

Major

Major

Major

Major

Major

Major

39 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

Section

Request

Description

Revisi
on
class

2.2.1.3 CTAPCBOR_CMD_GET_ASSERTION
Request

Updated request example.

Major

2.2.2 WebAuthN_Channel Response Message

Added the
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
S and
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST RPC commands.

2.2.2.1
CTAPCBOR_RPC_COMMAND_WEB_AUTHN
Response Map

Added new fields to
CTAPCBOR_RPC_COMMAND_WEB_AUTHN
Response Map.

2.2.2.1.1 CTAP MakeCredential Response

Updated response example.

2.2.2.1.2 CTAP GetAssertion Response

Updated response example.

2.2.2.2
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
S Response Map

Added description, fields, and example
response.

2.2.2.3
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST Response Map

Added description, fields, and example
response.

4.4.1.1 Request

4.4.1.2 Response

4.4.2.1 Request

4.4.2.2 Response

Updated request sample.

Updated response sample.

Updated request sample.

Updated response sample.

4.5
CTAPCBOR_RPC_COMMAND_GET_CREDENTIAL
S

Added a new section.

4.5.1 Request

4.5.2 Response

Added request sample.

Added response sample.

4.6
CTAPCBOR_RPC_COMMAND_GET_AUTHENTICA
TOR_LIST

Added a new section.

4.6.1 Request

4.6.2 Response

Added request sample.

Added response sample.

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

Major

40 / 41

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

Preconditions 6
Prerequisites 6
Product behavior 38

R

References 5
   informative 6
   normative 5
Relationship to other protocols 6

S

Security
   implementer considerations 37
   parameter index 37
Sequencing rules
   server 23
Server
   abstract data model 23
   higher-layer triggered events 23
   initialization 23
   message processing 23
   other local events 23
   overview 23
   sequencing rules 23
   timer events 23
   timers 23
Standards assignments 7

T

Timer events
   server 23
Timers
   server 23
Tracking changes 39
Transport 8
Triggered events - higher-layer
   server 23

V

Vendor-extensible fields 7
Versioning 6

W

WebAuthN_Channel Request Message message 8
WebAuthN_Channel Response Message message 15

8  Index
A

Abstract data model
   server 23
Applicability 6

C

Capability negotiation 6
Change tracking 39

D

Data model - abstract
   server 23

F

Fields - vendor-extensible 7

G

Glossary 5

H

Higher-layer triggered events
   server 23

I

Implementer - security considerations 37
Index of security parameters 37
Informative references 6
Initialization
   server 23
Introduction 5

M

Message processing
   server 23
Messages
   transport 8
   WebAuthN_Channel Request Message 8
   WebAuthN_Channel Response Message 15

N

Normative references 5

O

Other local events
   server 23
Overview (synopsis) 6

P

Parameters - security index 37

[MS-RDPEWA] - v20260330
Remote Desktop Protocol: WebAuthn Virtual Channel Protocol
Copyright © 2026 Microsoft Corporation
Release: March 30, 2026

41 / 41

