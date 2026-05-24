[MS-ADA1]:

Active Directory Schema Attributes A-L

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

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

1 / 140


Revision Summary

Date

Revision
History

Revision
Class

Comments

2/22/2007

0.01

6/1/2007

1.0

New

Major

Version 0.01 release

Updated and revised the technical content.

7/3/2007

1.0.1

Editorial

Changed language and formatting in the technical content.

7/20/2007

1.0.2

Editorial

Changed language and formatting in the technical content.

8/10/2007

1.0.3

Editorial

Changed language and formatting in the technical content.

9/28/2007

1.0.4

Editorial

Changed language and formatting in the technical content.

10/23/2007  1.0.5

Editorial

Changed language and formatting in the technical content.

11/30/2007  2.0

1/25/2008

3.0

3/14/2008

3.1

Major

Major

Minor

Updated and revised the technical content.

Updated and revised the technical content.

Clarified status of several attributes.

5/16/2008

3.1.1

Editorial

Changed language and formatting in the technical content.

6/20/2008

3.1.2

Editorial

Changed language and formatting in the technical content.

7/25/2008

4.0

8/29/2008

5.0

10/24/2008  6.0

12/5/2008

7.0

Major

Major

Major

Major

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

1/16/2009

7.0.1

Editorial

Changed language and formatting in the technical content.

2/27/2009

7.0.2

Editorial

Changed language and formatting in the technical content.

4/10/2009

8.0

5/22/2009

8.1

7/2/2009

8.2

Major

Minor

Minor

Updated and revised the technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

8/14/2009

8.2.1

Editorial

Changed language and formatting in the technical content.

9/25/2009

8.2.2

Editorial

Changed language and formatting in the technical content.

11/6/2009

8.3

12/18/2009  9.0

1/29/2010

10.0

3/12/2010

11.0

4/23/2010

12.0

6/4/2010

13.0

7/16/2010

13.1

Minor

Major

Major

Major

Major

Major

Minor

Clarified the meaning of the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

2 / 140


Date

Revision
History

Revision
Class

Comments

8/27/2010

14.0

10/8/2010

15.0

11/19/2010  16.0

1/7/2011

17.0

2/11/2011

18.0

3/25/2011

18.0

5/6/2011

18.1

6/17/2011

18.2

9/23/2011

18.2

Major

Major

Major

Major

Major

None

Minor

Minor

None

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Clarified the meaning of the technical content.

No changes to the meaning, language, or formatting of the
technical content.

12/16/2011  19.0

Major

Updated and revised the technical content.

3/30/2012

19.0

7/12/2012

20.0

10/25/2012  20.1

1/31/2013

20.1

None

Major

Minor

None

No changes to the meaning, language, or formatting of the
technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

No changes to the meaning, language, or formatting of the
technical content.

8/8/2013

20.2

Minor

Clarified the meaning of the technical content.

11/14/2013  20.2

None

No changes to the meaning, language, or formatting of the
technical content.

2/13/2014

20.2

None

No changes to the meaning, language, or formatting of the
technical content.

5/15/2014

20.2

None

No changes to the meaning, language, or formatting of the
technical content.

6/30/2015

20.3

Minor

Clarified the meaning of the technical content.

10/16/2015  20.3

None

No changes to the meaning, language, or formatting of the
technical content.

7/14/2016

20.3

None

No changes to the meaning, language, or formatting of the
technical content.

6/1/2017

20.3

9/15/2017

21.0

9/12/2018

21.1

None

Major

Minor

No changes to the meaning, language, or formatting of the
technical content.

Significantly changed the technical content.

Clarified the meaning of the technical content.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

3 / 140


## Table of Contents

- [1 Introduction](#1-introduction)
  - [1.1 References](#11-references)
- [2 Attributes](#2-attributes)
  - [2.1 Attribute accountExpires](#21-attribute-accountexpires)
  - [2.2 Attribute accountNameHistory](#22-attribute-accountnamehistory)
  - [2.3 Attribute aCSAggregateTokenRatePerUser](#23-attribute-acsaggregatetokenrateperuser)
  - [2.4 Attribute aCSAllocableRSVPBandwidth](#24-attribute-acsallocablersvpbandwidth)
  - [2.5 Attribute aCSCacheTimeout](#25-attribute-acscachetimeout)
  - [2.6 Attribute aCSDirection](#26-attribute-acsdirection)
  - [2.7 Attribute aCSDSBMDeadTime](#27-attribute-acsdsbmdeadtime)
  - [2.8 Attribute aCSDSBMPriority](#28-attribute-acsdsbmpriority)
  - [2.9 Attribute aCSDSBMRefresh](#29-attribute-acsdsbmrefresh)
  - [2.10 Attribute aCSEnableACSService](#210-attribute-acsenableacsservice)
  - [2.11 Attribute aCSEnableRSVPAccounting](#211-attribute-acsenablersvpaccounting)
  - [2.12 Attribute aCSEnableRSVPMessageLogging](#212-attribute-acsenablersvpmessagelogging)
  - [2.13 Attribute aCSEventLogLevel](#213-attribute-acseventloglevel)
  - [2.14 Attribute aCSIdentityName](#214-attribute-acsidentityname)
  - [2.15 Attribute aCSMaxAggregatePeakRatePerUser](#215-attribute-acsmaxaggregatepeakrateperuser)
  - [2.16 Attribute aCSMaxDurationPerFlow](#216-attribute-acsmaxdurationperflow)
  - [2.17 Attribute aCSMaximumSDUSize](#217-attribute-acsmaximumsdusize)
  - [2.18 Attribute aCSMaxNoOfAccountFiles](#218-attribute-acsmaxnoofaccountfiles)
  - [2.19 Attribute aCSMaxNoOfLogFiles](#219-attribute-acsmaxnooflogfiles)
  - [2.20 Attribute aCSMaxPeakBandwidth](#220-attribute-acsmaxpeakbandwidth)
  - [2.21 Attribute aCSMaxPeakBandwidthPerFlow](#221-attribute-acsmaxpeakbandwidthperflow)
  - [2.22 Attribute aCSMaxSizeOfRSVPAccountFile](#222-attribute-acsmaxsizeofrsvpaccountfile)
  - [2.23 Attribute aCSMaxSizeOfRSVPLogFile](#223-attribute-acsmaxsizeofrsvplogfile)
  - [2.24 Attribute aCSMaxTokenBucketPerFlow](#224-attribute-acsmaxtokenbucketperflow)
  - [2.25 Attribute aCSMaxTokenRatePerFlow](#225-attribute-acsmaxtokenrateperflow)
  - [2.26 Attribute aCSMinimumDelayVariation](#226-attribute-acsminimumdelayvariation)
  - [2.27 Attribute aCSMinimumLatency](#227-attribute-acsminimumlatency)
  - [2.28 Attribute aCSMinimumPolicedSize](#228-attribute-acsminimumpolicedsize)
  - [2.29 Attribute aCSNonReservedMaxSDUSize](#229-attribute-acsnonreservedmaxsdusize)
  - [2.30 Attribute aCSNonReservedMinPolicedSize](#230-attribute-acsnonreservedminpolicedsize)
  - [2.31 Attribute aCSNonReservedPeakRate](#231-attribute-acsnonreservedpeakrate)
  - [2.32 Attribute aCSNonReservedTokenSize](#232-attribute-acsnonreservedtokensize)
  - [2.33 Attribute aCSNonReservedTxLimit](#233-attribute-acsnonreservedtxlimit)
  - [2.34 Attribute aCSNonReservedTxSize](#234-attribute-acsnonreservedtxsize)
  - [2.35 Attribute aCSPermissionBits](#235-attribute-acspermissionbits)
  - [2.36 Attribute aCSPolicyName](#236-attribute-acspolicyname)
  - [2.37 Attribute aCSPriority](#237-attribute-acspriority)
  - [2.38 Attribute aCSRSVPAccountFilesLocation](#238-attribute-acsrsvpaccountfileslocation)
  - [2.39 Attribute aCSRSVPLogFilesLocation](#239-attribute-acsrsvplogfileslocation)
  - [2.40 Attribute aCSServerList](#240-attribute-acsserverlist)
  - [2.41 Attribute aCSServiceType](#241-attribute-acsservicetype)
  - [2.42 Attribute aCSTimeOfDay](#242-attribute-acstimeofday)
  - [2.43 Attribute aCSTotalNoOfFlows](#243-attribute-acstotalnoofflows)
  - [2.44 Attribute additionalTrustedServiceNames](#244-attribute-additionaltrustedservicenames)
  - [2.45 Attribute addressBookRoots](#245-attribute-addressbookroots)
  - [2.46 Attribute addressBookRoots2](#246-attribute-addressbookroots2)
  - [2.47 Attribute addressEntryDisplayTable](#247-attribute-addressentrydisplaytable)
  - [2.48 Attribute addressEntryDisplayTableMSDOS](#248-attribute-addressentrydisplaytablemsdos)
  - [2.49 Attribute addressSyntax](#249-attribute-addresssyntax)
  - [2.50 Attribute addressType](#250-attribute-addresstype)
  - [2.51 Attribute adminContextMenu](#251-attribute-admincontextmenu)
  - [2.52 Attribute adminCount](#252-attribute-admincount)
  - [2.53 Attribute adminDescription](#253-attribute-admindescription)
  - [2.54 Attribute adminDisplayName](#254-attribute-admindisplayname)
  - [2.55 Attribute adminMultiselectPropertyPages](#255-attribute-adminmultiselectpropertypages)
  - [2.56 Attribute adminPropertyPages](#256-attribute-adminpropertypages)
  - [2.57 Attribute allowedAttributes](#257-attribute-allowedattributes)
  - [2.58 Attribute allowedAttributesEffective](#258-attribute-allowedattributeseffective)
  - [2.59 Attribute allowedChildClasses](#259-attribute-allowedchildclasses)
  - [2.60 Attribute allowedChildClassesEffective](#260-attribute-allowedchildclasseseffective)
  - [2.61 Attribute altSecurityIdentities](#261-attribute-altsecurityidentities)
  - [2.62 Attribute aNR](#262-attribute-anr)
  - [2.63 Attribute applicationName](#263-attribute-applicationname)
  - [2.64 Attribute appliesTo](#264-attribute-appliesto)
  - [2.65 Attribute appSchemaVersion](#265-attribute-appschemaversion)
  - [2.66 Attribute assetNumber](#266-attribute-assetnumber)
  - [2.67 Attribute assistant](#267-attribute-assistant)
  - [2.68 Attribute associatedDomain](#268-attribute-associateddomain)
  - [2.69 Attribute associatedName](#269-attribute-associatedname)
  - [2.70 Attribute assocNTAccount](#270-attribute-assocntaccount)
  - [2.71 Attribute attributeCertificateAttribute](#271-attribute-attributecertificateattribute)
  - [2.72 Attribute attributeDisplayNames](#272-attribute-attributedisplaynames)
  - [2.73 Attribute attributeID](#273-attribute-attributeid)
  - [2.74 Attribute attributeSecurityGUID](#274-attribute-attributesecurityguid)
  - [2.75 Attribute attributeSyntax](#275-attribute-attributesyntax)
  - [2.76 Attribute attributeTypes](#276-attribute-attributetypes)
  - [2.77 Attribute audio](#277-attribute-audio)
  - [2.78 Attribute auditingPolicy](#278-attribute-auditingpolicy)
  - [2.79 Attribute authenticationOptions](#279-attribute-authenticationoptions)
  - [2.80 Attribute authorityRevocationList](#280-attribute-authorityrevocationlist)
  - [2.81 Attribute auxiliaryClass](#281-attribute-auxiliaryclass)
  - [2.82 Attribute badPasswordTime](#282-attribute-badpasswordtime)
  - [2.83 Attribute badPwdCount](#283-attribute-badpwdcount)
  - [2.84 Attribute birthLocation](#284-attribute-birthlocation)
  - [2.85 Attribute bootFile](#285-attribute-bootfile)
  - [2.86 Attribute bootParameter](#286-attribute-bootparameter)
  - [2.87 Attribute bridgeheadServerListBL](#287-attribute-bridgeheadserverlistbl)
  - [2.88 Attribute bridgeheadTransportList](#288-attribute-bridgeheadtransportlist)
  - [2.89 Attribute buildingName](#289-attribute-buildingname)
  - [2.90 Attribute builtinCreationTime](#290-attribute-builtincreationtime)
  - [2.91 Attribute builtinModifiedCount](#291-attribute-builtinmodifiedcount)
  - [2.92 Attribute businessCategory](#292-attribute-businesscategory)
  - [2.93 Attribute bytesPerMinute](#293-attribute-bytesperminute)
  - [2.94 Attribute c](#294-attribute-c)
  - [2.95 Attribute cACertificate](#295-attribute-cacertificate)
  - [2.96 Attribute cACertificateDN](#296-attribute-cacertificatedn)
  - [2.97 Attribute cAConnect](#297-attribute-caconnect)
  - [2.98 Attribute canonicalName](#298-attribute-canonicalname)
  - [2.99 Attribute canUpgradeScript](#299-attribute-canupgradescript)
  - [2.100 Attribute carLicense](#2100-attribute-carlicense)
  - [2.101 Attribute catalogs](#2101-attribute-catalogs)
  - [2.102 Attribute categories](#2102-attribute-categories)
  - [2.103 Attribute categoryId](#2103-attribute-categoryid)
  - [2.104 Attribute cAUsages](#2104-attribute-causages)
  - [2.105 Attribute cAWEBURL](#2105-attribute-caweburl)
  - [2.106 Attribute certificateAuthorityObject](#2106-attribute-certificateauthorityobject)
  - [2.107 Attribute certificateRevocationList](#2107-attribute-certificaterevocationlist)
  - [2.108 Attribute certificateTemplates](#2108-attribute-certificatetemplates)
  - [2.109 Attribute classDisplayName](#2109-attribute-classdisplayname)
  - [2.110 Attribute cn](#2110-attribute-cn)
  - [2.111 Attribute co](#2111-attribute-co)
  - [2.112 Attribute codePage](#2112-attribute-codepage)
  - [2.113 Attribute cOMClassID](#2113-attribute-comclassid)
  - [2.114 Attribute cOMCLSID](#2114-attribute-comclsid)
  - [2.115 Attribute cOMInterfaceID](#2115-attribute-cominterfaceid)
  - [2.116 Attribute comment](#2116-attribute-comment)
  - [2.117 Attribute cOMOtherProgId](#2117-attribute-comotherprogid)
  - [2.118 Attribute company](#2118-attribute-company)
  - [2.119 Attribute cOMProgID](#2119-attribute-comprogid)
  - [2.120 Attribute cOMTreatAsClassId](#2120-attribute-comtreatasclassid)
  - [2.121 Attribute cOMTypelibId](#2121-attribute-comtypelibid)
  - [2.122 Attribute cOMUniqueLIBID](#2122-attribute-comuniquelibid)
  - [2.123 Attribute contentIndexingAllowed](#2123-attribute-contentindexingallowed)
  - [2.124 Attribute contextMenu](#2124-attribute-contextmenu)
  - [2.125 Attribute controlAccessRights](#2125-attribute-controlaccessrights)
  - [2.126 Attribute cost](#2126-attribute-cost)
  - [2.127 Attribute countryCode](#2127-attribute-countrycode)
  - [2.128 Attribute createDialog](#2128-attribute-createdialog)
  - [2.129 Attribute createTimeStamp](#2129-attribute-createtimestamp)
  - [2.130 Attribute createWizardExt](#2130-attribute-createwizardext)
  - [2.131 Attribute creationTime](#2131-attribute-creationtime)
  - [2.132 Attribute creationWizard](#2132-attribute-creationwizard)
  - [2.133 Attribute creator](#2133-attribute-creator)
  - [2.134 Attribute cRLObject](#2134-attribute-crlobject)
  - [2.135 Attribute cRLPartitionedRevocationList](#2135-attribute-crlpartitionedrevocationlist)
  - [2.136 Attribute crossCertificatePair](#2136-attribute-crosscertificatepair)
  - [2.137 Attribute currentLocation](#2137-attribute-currentlocation)
  - [2.138 Attribute currentParentCA](#2138-attribute-currentparentca)
  - [2.139 Attribute currentValue](#2139-attribute-currentvalue)
  - [2.140 Attribute currMachineId](#2140-attribute-currmachineid)
  - [2.141 Attribute dBCSPwd](#2141-attribute-dbcspwd)
  - [2.142 Attribute dc](#2142-attribute-dc)
  - [2.143 Attribute defaultClassStore](#2143-attribute-defaultclassstore)
  - [2.144 Attribute defaultGroup](#2144-attribute-defaultgroup)
  - [2.145 Attribute defaultHidingValue](#2145-attribute-defaulthidingvalue)
  - [2.146 Attribute defaultLocalPolicyObject](#2146-attribute-defaultlocalpolicyobject)
  - [2.147 Attribute defaultObjectCategory](#2147-attribute-defaultobjectcategory)
  - [2.148 Attribute defaultPriority](#2148-attribute-defaultpriority)
  - [2.149 Attribute defaultSecurityDescriptor](#2149-attribute-defaultsecuritydescriptor)
  - [2.150 Attribute deltaRevocationList](#2150-attribute-deltarevocationlist)
  - [2.151 Attribute department](#2151-attribute-department)
  - [2.152 Attribute departmentNumber](#2152-attribute-departmentnumber)
  - [2.153 Attribute description](#2153-attribute-description)
  - [2.154 Attribute desktopProfile](#2154-attribute-desktopprofile)
  - [2.155 Attribute destinationIndicator](#2155-attribute-destinationindicator)
  - [2.156 Attribute dhcpClasses](#2156-attribute-dhcpclasses)
  - [2.157 Attribute dhcpFlags](#2157-attribute-dhcpflags)
  - [2.158 Attribute dhcpIdentification](#2158-attribute-dhcpidentification)
  - [2.159 Attribute dhcpMask](#2159-attribute-dhcpmask)
  - [2.160 Attribute dhcpMaxKey](#2160-attribute-dhcpmaxkey)
  - [2.161 Attribute dhcpObjDescription](#2161-attribute-dhcpobjdescription)
  - [2.162 Attribute dhcpObjName](#2162-attribute-dhcpobjname)
  - [2.163 Attribute dhcpOptions](#2163-attribute-dhcpoptions)
  - [2.164 Attribute dhcpProperties](#2164-attribute-dhcpproperties)
  - [2.165 Attribute dhcpRanges](#2165-attribute-dhcpranges)
  - [2.166 Attribute dhcpReservations](#2166-attribute-dhcpreservations)
  - [2.167 Attribute dhcpServers](#2167-attribute-dhcpservers)
  - [2.168 Attribute dhcpSites](#2168-attribute-dhcpsites)
  - [2.169 Attribute dhcpState](#2169-attribute-dhcpstate)
  - [2.170 Attribute dhcpSubnets](#2170-attribute-dhcpsubnets)
  - [2.171 Attribute dhcpType](#2171-attribute-dhcptype)
  - [2.172 Attribute dhcpUniqueKey](#2172-attribute-dhcpuniquekey)
  - [2.173 Attribute dhcpUpdateTime](#2173-attribute-dhcpupdatetime)
  - [2.174 Attribute directReports](#2174-attribute-directreports)
  - [2.175 Attribute displayName](#2175-attribute-displayname)
  - [2.176 Attribute displayNamePrintable](#2176-attribute-displaynameprintable)
  - [2.177 Attribute distinguishedName](#2177-attribute-distinguishedname)
  - [2.178 Attribute dITContentRules](#2178-attribute-ditcontentrules)
  - [2.179 Attribute division](#2179-attribute-division)
  - [2.180 Attribute dMDLocation](#2180-attribute-dmdlocation)
  - [2.181 Attribute dmdName](#2181-attribute-dmdname)
  - [2.182 Attribute dNReferenceUpdate](#2182-attribute-dnreferenceupdate)
  - [2.183 Attribute dnsAllowDynamic](#2183-attribute-dnsallowdynamic)
  - [2.184 Attribute dnsAllowXFR](#2184-attribute-dnsallowxfr)
  - [2.185 Attribute dNSHostName](#2185-attribute-dnshostname)
  - [2.186 Attribute dnsNotifySecondaries](#2186-attribute-dnsnotifysecondaries)
  - [2.187 Attribute dNSProperty](#2187-attribute-dnsproperty)
  - [2.188 Attribute dnsRecord](#2188-attribute-dnsrecord)
  - [2.189 Attribute dnsRoot](#2189-attribute-dnsroot)
  - [2.190 Attribute dnsSecureSecondaries](#2190-attribute-dnssecuresecondaries)
  - [2.191 Attribute dNSTombstoned](#2191-attribute-dnstombstoned)
  - [2.192 Attribute documentAuthor](#2192-attribute-documentauthor)
  - [2.193 Attribute documentIdentifier](#2193-attribute-documentidentifier)
  - [2.194 Attribute documentLocation](#2194-attribute-documentlocation)
  - [2.195 Attribute documentPublisher](#2195-attribute-documentpublisher)
  - [2.196 Attribute documentTitle](#2196-attribute-documenttitle)
  - [2.197 Attribute documentVersion](#2197-attribute-documentversion)
  - [2.198 Attribute domainCAs](#2198-attribute-domaincas)
  - [2.199 Attribute domainCrossRef](#2199-attribute-domaincrossref)
  - [2.200 Attribute domainID](#2200-attribute-domainid)
  - [2.201 Attribute domainIdentifier](#2201-attribute-domainidentifier)
  - [2.202 Attribute domainPolicyObject](#2202-attribute-domainpolicyobject)
  - [2.203 Attribute domainPolicyReference](#2203-attribute-domainpolicyreference)
  - [2.204 Attribute domainReplica](#2204-attribute-domainreplica)
  - [2.205 Attribute domainWidePolicy](#2205-attribute-domainwidepolicy)
  - [2.206 Attribute drink](#2206-attribute-drink)
  - [2.207 Attribute driverName](#2207-attribute-drivername)
  - [2.208 Attribute driverVersion](#2208-attribute-driverversion)
  - [2.209 Attribute dSASignature](#2209-attribute-dsasignature)
  - [2.210 Attribute dSCorePropagationData](#2210-attribute-dscorepropagationdata)
  - [2.211 Attribute dSHeuristics](#2211-attribute-dsheuristics)
  - [2.212 Attribute dSUIAdminMaximum](#2212-attribute-dsuiadminmaximum)
  - [2.213 Attribute dSUIAdminNotification](#2213-attribute-dsuiadminnotification)
  - [2.214 Attribute dSUIShellMaximum](#2214-attribute-dsuishellmaximum)
  - [2.215 Attribute dynamicLDAPServer](#2215-attribute-dynamicldapserver)
  - [2.216 Attribute eFSPolicy](#2216-attribute-efspolicy)
  - [2.217 Attribute employeeID](#2217-attribute-employeeid)
  - [2.218 Attribute employeeNumber](#2218-attribute-employeenumber)
  - [2.219 Attribute employeeType](#2219-attribute-employeetype)
  - [2.220 Attribute Enabled](#2220-attribute-enabled)
  - [2.221 Attribute enabledConnection](#2221-attribute-enabledconnection)
  - [2.222 Attribute enrollmentProviders](#2222-attribute-enrollmentproviders)
  - [2.223 Attribute entryTTL](#2223-attribute-entryttl)
  - [2.224 Attribute extendedAttributeInfo](#2224-attribute-extendedattributeinfo)
  - [2.225 Attribute extendedCharsAllowed](#2225-attribute-extendedcharsallowed)
  - [2.226 Attribute extendedClassInfo](#2226-attribute-extendedclassinfo)
  - [2.227 Attribute extensionName](#2227-attribute-extensionname)
  - [2.228 Attribute extraColumns](#2228-attribute-extracolumns)
  - [2.229 Attribute facsimileTelephoneNumber](#2229-attribute-facsimiletelephonenumber)
  - [2.230 Attribute fileExtPriority](#2230-attribute-fileextpriority)
  - [2.231 Attribute flags](#2231-attribute-flags)
  - [2.232 Attribute flatName](#2232-attribute-flatname)
  - [2.233 Attribute forceLogoff](#2233-attribute-forcelogoff)
  - [2.234 Attribute foreignIdentifier](#2234-attribute-foreignidentifier)
  - [2.235 Attribute friendlyNames](#2235-attribute-friendlynames)
  - [2.236 Attribute fromEntry](#2236-attribute-fromentry)
  - [2.237 Attribute fromServer](#2237-attribute-fromserver)
  - [2.238 Attribute frsComputerReference](#2238-attribute-frscomputerreference)
  - [2.239 Attribute frsComputerReferenceBL](#2239-attribute-frscomputerreferencebl)
  - [2.240 Attribute fRSControlDataCreation](#2240-attribute-frscontroldatacreation)
  - [2.241 Attribute fRSControlInboundBacklog](#2241-attribute-frscontrolinboundbacklog)
  - [2.242 Attribute fRSControlOutboundBacklog](#2242-attribute-frscontroloutboundbacklog)
  - [2.243 Attribute fRSDirectoryFilter](#2243-attribute-frsdirectoryfilter)
  - [2.244 Attribute fRSDSPoll](#2244-attribute-frsdspoll)
  - [2.245 Attribute fRSExtensions](#2245-attribute-frsextensions)
  - [2.246 Attribute fRSFaultCondition](#2246-attribute-frsfaultcondition)
  - [2.247 Attribute fRSFileFilter](#2247-attribute-frsfilefilter)
  - [2.248 Attribute fRSFlags](#2248-attribute-frsflags)
  - [2.249 Attribute fRSLevelLimit](#2249-attribute-frslevellimit)
  - [2.250 Attribute fRSMemberReference](#2250-attribute-frsmemberreference)
  - [2.251 Attribute fRSMemberReferenceBL](#2251-attribute-frsmemberreferencebl)
  - [2.252 Attribute fRSPartnerAuthLevel](#2252-attribute-frspartnerauthlevel)
  - [2.253 Attribute fRSPrimaryMember](#2253-attribute-frsprimarymember)
  - [2.254 Attribute fRSReplicaSetGUID](#2254-attribute-frsreplicasetguid)
  - [2.255 Attribute fRSReplicaSetType](#2255-attribute-frsreplicasettype)
  - [2.256 Attribute fRSRootPath](#2256-attribute-frsrootpath)
  - [2.257 Attribute fRSRootSecurity](#2257-attribute-frsrootsecurity)
  - [2.258 Attribute fRSServiceCommand](#2258-attribute-frsservicecommand)
  - [2.259 Attribute fRSServiceCommandStatus](#2259-attribute-frsservicecommandstatus)
  - [2.260 Attribute fRSStagingPath](#2260-attribute-frsstagingpath)
  - [2.261 Attribute fRSTimeLastCommand](#2261-attribute-frstimelastcommand)
  - [2.262 Attribute fRSTimeLastConfigChange](#2262-attribute-frstimelastconfigchange)
  - [2.263 Attribute fRSUpdateTimeout](#2263-attribute-frsupdatetimeout)
  - [2.264 Attribute fRSVersion](#2264-attribute-frsversion)
  - [2.265 Attribute fRSVersionGUID](#2265-attribute-frsversionguid)
  - [2.266 Attribute fRSWorkingPath](#2266-attribute-frsworkingpath)
  - [2.267 Attribute fSMORoleOwner](#2267-attribute-fsmoroleowner)
  - [2.268 Attribute garbageCollPeriod](#2268-attribute-garbagecollperiod)
  - [2.269 Attribute gecos](#2269-attribute-gecos)
  - [2.270 Attribute generatedConnection](#2270-attribute-generatedconnection)
  - [2.271 Attribute generationQualifier](#2271-attribute-generationqualifier)
  - [2.272 Attribute gidNumber](#2272-attribute-gidnumber)
  - [2.273 Attribute givenName](#2273-attribute-givenname)
  - [2.274 Attribute globalAddressList](#2274-attribute-globaladdresslist)
  - [2.275 Attribute globalAddressList2](#2275-attribute-globaladdresslist2)
  - [2.276 Attribute governsID](#2276-attribute-governsid)
  - [2.277 Attribute gPCFileSysPath](#2277-attribute-gpcfilesyspath)
  - [2.278 Attribute gPCFunctionalityVersion](#2278-attribute-gpcfunctionalityversion)
  - [2.279 Attribute gPCMachineExtensionNames](#2279-attribute-gpcmachineextensionnames)
  - [2.280 Attribute gPCUserExtensionNames](#2280-attribute-gpcuserextensionnames)
  - [2.281 Attribute gPCWQLFilter](#2281-attribute-gpcwqlfilter)
  - [2.282 Attribute gPLink](#2282-attribute-gplink)
  - [2.283 Attribute gPOptions](#2283-attribute-gpoptions)
  - [2.284 Attribute groupAttributes](#2284-attribute-groupattributes)
  - [2.285 Attribute groupMembershipSAM](#2285-attribute-groupmembershipsam)
  - [2.286 Attribute groupPriority](#2286-attribute-grouppriority)
  - [2.287 Attribute groupsToIgnore](#2287-attribute-groupstoignore)
  - [2.288 Attribute groupType](#2288-attribute-grouptype)
  - [2.289 Attribute hasMasterNCs](#2289-attribute-hasmasterncs)
  - [2.290 Attribute hasPartialReplicaNCs](#2290-attribute-haspartialreplicancs)
  - [2.291 Attribute helpData16](#2291-attribute-helpdata16)
  - [2.292 Attribute helpData32](#2292-attribute-helpdata32)
  - [2.293 Attribute helpFileName](#2293-attribute-helpfilename)
  - [2.294 Attribute hideFromAB](#2294-attribute-hidefromab)
  - [2.295 Attribute homeDirectory](#2295-attribute-homedirectory)
  - [2.296 Attribute homeDrive](#2296-attribute-homedrive)
  - [2.297 Attribute homePhone](#2297-attribute-homephone)
  - [2.298 Attribute homePostalAddress](#2298-attribute-homepostaladdress)
  - [2.299 Attribute host](#2299-attribute-host)
  - [2.300 Attribute houseIdentifier](#2300-attribute-houseidentifier)
  - [2.301 Attribute iconPath](#2301-attribute-iconpath)
  - [2.302 Attribute implementedCategories](#2302-attribute-implementedcategories)
  - [2.303 Attribute indexedScopes](#2303-attribute-indexedscopes)
  - [2.304 Attribute info](#2304-attribute-info)
  - [2.305 Attribute initialAuthIncoming](#2305-attribute-initialauthincoming)
  - [2.306 Attribute initialAuthOutgoing](#2306-attribute-initialauthoutgoing)
  - [2.307 Attribute initials](#2307-attribute-initials)
  - [2.308 Attribute installUiLevel](#2308-attribute-installuilevel)
  - [2.309 Attribute instanceType](#2309-attribute-instancetype)
  - [2.310 Attribute internationalISDNNumber](#2310-attribute-internationalisdnnumber)
  - [2.311 Attribute interSiteTopologyFailover](#2311-attribute-intersitetopologyfailover)
  - [2.312 Attribute interSiteTopologyGenerator](#2312-attribute-intersitetopologygenerator)
  - [2.313 Attribute interSiteTopologyRenew](#2313-attribute-intersitetopologyrenew)
  - [2.314 Attribute invocationId](#2314-attribute-invocationid)
  - [2.315 Attribute ipHostNumber](#2315-attribute-iphostnumber)
  - [2.316 Attribute ipNetmaskNumber](#2316-attribute-ipnetmasknumber)
  - [2.317 Attribute ipNetworkNumber](#2317-attribute-ipnetworknumber)
  - [2.318 Attribute ipPhone](#2318-attribute-ipphone)
  - [2.319 Attribute ipProtocolNumber](#2319-attribute-ipprotocolnumber)
  - [2.320 Attribute ipsecData](#2320-attribute-ipsecdata)
  - [2.321 Attribute ipsecDataType](#2321-attribute-ipsecdatatype)
  - [2.322 Attribute ipsecFilterReference](#2322-attribute-ipsecfilterreference)
  - [2.323 Attribute ipsecID](#2323-attribute-ipsecid)
  - [2.324 Attribute ipsecISAKMPReference](#2324-attribute-ipsecisakmpreference)
  - [2.325 Attribute ipsecName](#2325-attribute-ipsecname)
  - [2.326 Attribute iPSECNegotiationPolicyAction](#2326-attribute-ipsecnegotiationpolicyaction)
  - [2.327 Attribute ipsecNegotiationPolicyReference](#2327-attribute-ipsecnegotiationpolicyreference)
  - [2.328 Attribute iPSECNegotiationPolicyType](#2328-attribute-ipsecnegotiationpolicytype)
  - [2.329 Attribute ipsecNFAReference](#2329-attribute-ipsecnfareference)
  - [2.330 Attribute ipsecOwnersReference](#2330-attribute-ipsecownersreference)
  - [2.331 Attribute ipsecPolicyReference](#2331-attribute-ipsecpolicyreference)
  - [2.332 Attribute ipServicePort](#2332-attribute-ipserviceport)
  - [2.333 Attribute ipServiceProtocol](#2333-attribute-ipserviceprotocol)
  - [2.334 Attribute isCriticalSystemObject](#2334-attribute-iscriticalsystemobject)
  - [2.335 Attribute isDefunct](#2335-attribute-isdefunct)
  - [2.336 Attribute isDeleted](#2336-attribute-isdeleted)
  - [2.337 Attribute isEphemeral](#2337-attribute-isephemeral)
  - [2.338 Attribute isMemberOfPartialAttributeSet](#2338-attribute-ismemberofpartialattributeset)
  - [2.339 Attribute isPrivilegeHolder](#2339-attribute-isprivilegeholder)
  - [2.340 Attribute isRecycled](#2340-attribute-isrecycled)
  - [2.341 Attribute isSingleValued](#2341-attribute-issinglevalued)
  - [2.342 Attribute jpegPhoto](#2342-attribute-jpegphoto)
  - [2.343 Attribute keywords](#2343-attribute-keywords)
  - [2.344 Attribute knowledgeInformation](#2344-attribute-knowledgeinformation)
  - [2.345 Attribute l](#2345-attribute-l)
  - [2.346 Attribute labeledURI](#2346-attribute-labeleduri)
  - [2.347 Attribute lastBackupRestorationTime](#2347-attribute-lastbackuprestorationtime)
  - [2.348 Attribute lastContentIndexed](#2348-attribute-lastcontentindexed)
  - [2.349 Attribute lastKnownParent](#2349-attribute-lastknownparent)
  - [2.350 Attribute lastLogoff](#2350-attribute-lastlogoff)
  - [2.351 Attribute lastLogon](#2351-attribute-lastlogon)
  - [2.352 Attribute lastLogonTimestamp](#2352-attribute-lastlogontimestamp)
  - [2.353 Attribute lastSetTime](#2353-attribute-lastsettime)
  - [2.354 Attribute lastUpdateSequence](#2354-attribute-lastupdatesequence)
  - [2.355 Attribute lDAPAdminLimits](#2355-attribute-ldapadminlimits)
  - [2.356 Attribute lDAPDisplayName](#2356-attribute-ldapdisplayname)
  - [2.357 Attribute lDAPIPDenyList](#2357-attribute-ldapipdenylist)
  - [2.358 Attribute lSACreationTime](#2358-attribute-lsacreationtime)
  - [2.359 Attribute lSAModifiedCount](#2359-attribute-lsamodifiedcount)
  - [2.360 Attribute legacyExchangeDN](#2360-attribute-legacyexchangedn)
  - [2.361 Attribute linkID](#2361-attribute-linkid)
  - [2.362 Attribute linkTrackSecret](#2362-attribute-linktracksecret)
  - [2.363 Attribute lmPwdHistory](#2363-attribute-lmpwdhistory)
  - [2.364 Attribute localeID](#2364-attribute-localeid)
  - [2.365 Attribute localizationDisplayId](#2365-attribute-localizationdisplayid)
  - [2.366 Attribute localizedDescription](#2366-attribute-localizeddescription)
  - [2.367 Attribute localPolicyFlags](#2367-attribute-localpolicyflags)
  - [2.368 Attribute localPolicyReference](#2368-attribute-localpolicyreference)
  - [2.369 Attribute location](#2369-attribute-location)
  - [2.370 Attribute lockoutDuration](#2370-attribute-lockoutduration)
  - [2.371 Attribute lockOutObservationWindow](#2371-attribute-lockoutobservationwindow)
  - [2.372 Attribute lockoutThreshold](#2372-attribute-lockoutthreshold)
  - [2.373 Attribute lockoutTime](#2373-attribute-lockouttime)
  - [2.374 Attribute loginShell](#2374-attribute-loginshell)
  - [2.375 Attribute logonCount](#2375-attribute-logoncount)
  - [2.376 Attribute logonHours](#2376-attribute-logonhours)
  - [2.377 Attribute logonWorkstation](#2377-attribute-logonworkstation)
- [3 Change Tracking](#3-change-tracking)
- [4 Index](#4-index)

## 1 Introduction

Active Directory Schema Attributes A-L contains a partial list of the objects that exist in the Active
Directory schema for Active Directory Domain Services (AD DS); it contains schema objects of
type "attribute" whose names start with the letters A through L. Active Directory and all associated
terms and concepts are described in [MS-ADTS].

Note: This document is not intended to stand on its own; it is intended to act as an appendix to the
Active Directory Technical Specification. For details about the Active Directory schema, see [MS-ADTS]
section 3.1.1.2 (Active Directory Schema).

Note: The object definitions in this document are also available for download in LDAP Data
Interchange Format (LDIF) at the following location: [MSFT-ADSCHEMA].

Note: The object definitions in this document contain information about the product in which the
objects were first implemented in the Active Directory schema. Unless otherwise specified, objects
continue to be available in the Active Directory schema in all subsequent versions of the product
according to the list of products in [MS-ADTS] section 1.

### 1.1 References

[JFIF] Hamilton, E., "JPEG File Interchange Format, Version 1.02", September 1992,
http://www.w3.org/Graphics/JPEG/jfif.txt

[MS-ADA3] Microsoft Corporation, "Active Directory Schema Attributes N-Z".

[MS-ADOD] Microsoft Corporation, "Active Directory Protocols Overview".

[MS-ADTS] Microsoft Corporation, "Active Directory Technical Specification".

[MS-DTYP] Microsoft Corporation, "Windows Data Types".

[MS-GPIPSEC] Microsoft Corporation, "Group Policy: IP Security (IPsec) Protocol Extension".

[MS-GPOL] Microsoft Corporation, "Group Policy: Core Protocol".

[MS-LSAD] Microsoft Corporation, "Local Security Authority (Domain Policy) Remote Protocol".

[MS-SAMR] Microsoft Corporation, "Security Account Manager (SAM) Remote Protocol (Client-to-
Server)".

[MSDN-ACL] Microsoft Corporation, "ACL structure", http://msdn.microsoft.com/en-
us/library/aa374931.aspx

[MSDN-CP] Microsoft Corporation, "Code Page Identifiers", https://learn.microsoft.com/en-
us/windows/desktop/Intl/code-page-identifiers

[MSDN-ExtUserIntDirObj] Microsoft Corporation, "Extending the User Interface for Directory Objects",
http://msdn.microsoft.com/en-us/library/ms676902.aspx

[MSDN-GroupType] Microsoft Corporation, "Group-Type", http://msdn.microsoft.com/en-
us/library/ms675935.aspx

[MSFT-ADSCHEMA] Microsoft Corporation, "Combined Active Directory Schema Classes and Attributes
for Windows Server", December 2013, https://www.microsoft.com/en-
us/download/details.aspx?id=23782

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

11 / 140


[RFC2251] Wahl, M., Howes, T., and Kille, S., "Lightweight Directory Access Protocol (v3)", RFC 2251,
December 1997, https://www.rfc-editor.org/info/rfc2251

[RFC2307] Howard, L., "An Approach for Using LDAP as a Network Information Service", RFC 2307,
March 1998, https://www.rfc-editor.org/info/rfc2307

[RFC2849] Good, G., "The LDAP Data Interchange Format (LDIF) - Technical Specification", RFC 2849,
June 2000, https://www.rfc-editor.org/info/rfc2849

[X500] ITU-T, "Information Technology - Open Systems Interconnection - The Directory: Overview of
Concepts, Models and Services", Recommendation X.500, August 2005, http://www.itu.int/rec/T-REC-
X.500-200508-S/en

Note There is a charge to download the specification.

[X509] ITU-T, "Information Technology - Open Systems Interconnection - The Directory: Public-Key
and Attribute Certificate Frameworks", Recommendation X.509, August 2005,
http://www.itu.int/rec/T-REC-X.509/en

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

12 / 140


## 2 Attributes

The following sections specify attributes in the Active Directory schema whose names start with the
letters A through L.

These sections normatively specify the schema definition of each attribute and version-specific
behavior of those schema definitions (such as when the attribute was added to the schema).
Additionally, as an aid to the reader some of the sections include informative notes about how the
attribute can be used.

Note: Lines of text in the attribute definitions that are excessively long have been "folded" in
accordance with [RFC2849] Note 2.

### 2.1 Attribute accountExpires

This attribute specifies the date the account expires. This value represents the number of 100-
nanosecond intervals since January 1, 1601, Coordinated Universal Time (Greenwich Mean Time). A
value of 0 or 0x7FFFFFFFFFFFFFFF (9223372036854775807) indicates that the account never expires.

 cn: Account-Expires
 ldapDisplayName: accountExpires
 attributeId: 1.2.840.113556.1.4.159
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967915-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 4c164200-20c0-11d0-a768-00aa006e0529
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server operating system.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008 operating
system.

### 2.2 Attribute accountNameHistory

This attribute specifies the length of time the account has been active.

 cn: Account-Name-History
 ldapDisplayName: accountNameHistory
 attributeId: 1.2.840.113556.1.4.1307
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 031952ec-3b72-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.3 Attribute aCSAggregateTokenRatePerUser

This attribute specifies the maximum quality of service token rate for any user for all flows.

13 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: ACS-Aggregate-Token-Rate-Per-User
 ldapDisplayName: aCSAggregateTokenRatePerUser
 attributeId: 1.2.840.113556.1.4.760
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f56127d-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.4 Attribute aCSAllocableRSVPBandwidth

This attribute specifies the maximum bandwidth that can be reserved.

 cn: ACS-Allocable-RSVP-Bandwidth
 ldapDisplayName: aCSAllocableRSVPBandwidth
 attributeId: 1.2.840.113556.1.4.766
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f561283-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.5 Attribute aCSCacheTimeout

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Cache-Timeout
 ldapDisplayName: aCSCacheTimeout
 attributeId: 1.2.840.113556.1.4.779
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb355a1-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.6 Attribute aCSDirection

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Direction
 ldapDisplayName: aCSDirection
 attributeId: 1.2.840.113556.1.4.757
 attributeSyntax: 2.5.5.9
 omSyntax: 2

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

14 / 140


 isSingleValued: TRUE
 schemaIdGuid: 7f56127a-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.7 Attribute aCSDSBMDeadTime

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-DSBM-DeadTime
 ldapDisplayName: aCSDSBMDeadTime
 attributeId: 1.2.840.113556.1.4.778
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb355a0-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.8 Attribute aCSDSBMPriority

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-DSBM-Priority
 ldapDisplayName: aCSDSBMPriority
 attributeId: 1.2.840.113556.1.4.776
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559e-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.9 Attribute aCSDSBMRefresh

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-DSBM-Refresh
 ldapDisplayName: aCSDSBMRefresh
 attributeId: 1.2.840.113556.1.4.777
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559f-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

15 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.10 Attribute aCSEnableACSService

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Enable-ACS-Service
 ldapDisplayName: aCSEnableACSService
 attributeId: 1.2.840.113556.1.4.770
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 7f561287-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.11 Attribute aCSEnableRSVPAccounting

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Enable-RSVP-Accounting
 ldapDisplayName: aCSEnableRSVPAccounting
 attributeId: 1.2.840.113556.1.4.899
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: f072230e-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.12 Attribute aCSEnableRSVPMessageLogging

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Enable-RSVP-Message-Logging
 ldapDisplayName: aCSEnableRSVPMessageLogging
 attributeId: 1.2.840.113556.1.4.768
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 7f561285-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

16 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.13 Attribute aCSEventLogLevel

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Event-Log-Level
 ldapDisplayName: aCSEventLogLevel
 attributeId: 1.2.840.113556.1.4.769
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f561286-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.14 Attribute aCSIdentityName

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Identity-Name
 ldapDisplayName: aCSIdentityName
 attributeId: 1.2.840.113556.1.4.784
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: dab029b6-ddf7-11d1-90a5-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.15 Attribute aCSMaxAggregatePeakRatePerUser

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Aggregate-Peak-Rate-Per-User
 ldapDisplayName: aCSMaxAggregatePeakRatePerUser
 attributeId: 1.2.840.113556.1.4.897
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: f072230c-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.16 Attribute aCSMaxDurationPerFlow

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

17 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: ACS-Max-Duration-Per-Flow
 ldapDisplayName: aCSMaxDurationPerFlow
 attributeId: 1.2.840.113556.1.4.761
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f56127e-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.17 Attribute aCSMaximumSDUSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Maximum-SDU-Size
 ldapDisplayName: aCSMaximumSDUSize
 attributeId: 1.2.840.113556.1.4.1314
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 87a2d8f9-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.18 Attribute aCSMaxNoOfAccountFiles

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-No-Of-Account-Files
 ldapDisplayName: aCSMaxNoOfAccountFiles
 attributeId: 1.2.840.113556.1.4.901
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: f0722310-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.19 Attribute aCSMaxNoOfLogFiles

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-No-Of-Log-Files
 ldapDisplayName: aCSMaxNoOfLogFiles
 attributeId: 1.2.840.113556.1.4.774
 attributeSyntax: 2.5.5.9

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

18 / 140


 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559c-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.20 Attribute aCSMaxPeakBandwidth

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Peak-Bandwidth
 ldapDisplayName: aCSMaxPeakBandwidth
 attributeId: 1.2.840.113556.1.4.767
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f561284-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.21 Attribute aCSMaxPeakBandwidthPerFlow

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Peak-Bandwidth-Per-Flow
 ldapDisplayName: aCSMaxPeakBandwidthPerFlow
 attributeId: 1.2.840.113556.1.4.759
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f56127c-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.22 Attribute aCSMaxSizeOfRSVPAccountFile

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Size-Of-RSVP-Account-File
 ldapDisplayName: aCSMaxSizeOfRSVPAccountFile
 attributeId: 1.2.840.113556.1.4.902
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: f0722311-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

19 / 140


 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.23 Attribute aCSMaxSizeOfRSVPLogFile

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Size-Of-RSVP-Log-File
 ldapDisplayName: aCSMaxSizeOfRSVPLogFile
 attributeId: 1.2.840.113556.1.4.775
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559d-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.24 Attribute aCSMaxTokenBucketPerFlow

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Token-Bucket-Per-Flow
 ldapDisplayName: aCSMaxTokenBucketPerFlow
 attributeId: 1.2.840.113556.1.4.1313
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 81f6e0df-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.25 Attribute aCSMaxTokenRatePerFlow

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Max-Token-Rate-Per-Flow
 ldapDisplayName: aCSMaxTokenRatePerFlow
 attributeId: 1.2.840.113556.1.4.758
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f56127b-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

20 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.26 Attribute aCSMinimumDelayVariation

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Minimum-Delay-Variation
 ldapDisplayName: aCSMinimumDelayVariation
 attributeId: 1.2.840.113556.1.4.1317
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 9c65329b-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.27 Attribute aCSMinimumLatency

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Minimum-Latency
 ldapDisplayName: aCSMinimumLatency
 attributeId: 1.2.840.113556.1.4.1316
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 9517fefb-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.28 Attribute aCSMinimumPolicedSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Minimum-Policed-Size
 ldapDisplayName: aCSMinimumPolicedSize
 attributeId: 1.2.840.113556.1.4.1315
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 8d0e7195-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

21 / 140


### 2.29 Attribute aCSNonReservedMaxSDUSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Non-Reserved-Max-SDU-Size
 ldapDisplayName: aCSNonReservedMaxSDUSize
 attributeId: 1.2.840.113556.1.4.1320
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: aec2cfe3-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.30 Attribute aCSNonReservedMinPolicedSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Non-Reserved-Min-Policed-Size
 ldapDisplayName: aCSNonReservedMinPolicedSize
 attributeId: 1.2.840.113556.1.4.1321
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: b6873917-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.31 Attribute aCSNonReservedPeakRate

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Non-Reserved-Peak-Rate
 ldapDisplayName: aCSNonReservedPeakRate
 attributeId: 1.2.840.113556.1.4.1318
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: a331a73f-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.32 Attribute aCSNonReservedTokenSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

22 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: ACS-Non-Reserved-Token-Size
 ldapDisplayName: aCSNonReservedTokenSize
 attributeId: 1.2.840.113556.1.4.1319
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: a916d7c9-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.33 Attribute aCSNonReservedTxLimit

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Non-Reserved-Tx-Limit
 ldapDisplayName: aCSNonReservedTxLimit
 attributeId: 1.2.840.113556.1.4.780
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 1cb355a2-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.34 Attribute aCSNonReservedTxSize

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Non-Reserved-Tx-Size
 ldapDisplayName: aCSNonReservedTxSize
 attributeId: 1.2.840.113556.1.4.898
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: f072230d-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.35 Attribute aCSPermissionBits

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Permission-Bits
 ldapDisplayName: aCSPermissionBits
 attributeId: 1.2.840.113556.1.4.765
 attributeSyntax: 2.5.5.16

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

23 / 140


 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 7f561282-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.36 Attribute aCSPolicyName

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Policy-Name
 ldapDisplayName: aCSPolicyName
 attributeId: 1.2.840.113556.1.4.772
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559a-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.37 Attribute aCSPriority

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Priority
 ldapDisplayName: aCSPriority
 attributeId: 1.2.840.113556.1.4.764
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f561281-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.38 Attribute aCSRSVPAccountFilesLocation

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-RSVP-Account-Files-Location
 ldapDisplayName: aCSRSVPAccountFilesLocation
 attributeId: 1.2.840.113556.1.4.900
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f072230f-aef5-11d1-bdcf-0000f80367c1
 systemOnly: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

24 / 140


 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.39 Attribute aCSRSVPLogFilesLocation

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-RSVP-Log-Files-Location
 ldapDisplayName: aCSRSVPLogFilesLocation
 attributeId: 1.2.840.113556.1.4.773
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1cb3559b-56d0-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.40 Attribute aCSServerList

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Server-List
 ldapDisplayName: aCSServerList
 attributeId: 1.2.840.113556.1.4.1312
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7cbd59a5-3b90-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.41 Attribute aCSServiceType

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Service-Type
 ldapDisplayName: aCSServiceType
 attributeId: 1.2.840.113556.1.4.762
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f56127f-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

25 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.42 Attribute aCSTimeOfDay

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Time-Of-Day
 ldapDisplayName: aCSTimeOfDay
 attributeId: 1.2.840.113556.1.4.756
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 7f561279-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.43 Attribute aCSTotalNoOfFlows

This attribute is not necessary for Active Directory to function. The protocol does not define a format
beyond that required by the schema.

 cn: ACS-Total-No-Of-Flows
 ldapDisplayName: aCSTotalNoOfFlows
 attributeId: 1.2.840.113556.1.4.763
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f561280-5301-11d1-a9c5-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.44 Attribute additionalTrustedServiceNames

This attribute specifies a list of services in the domain that can be trusted. This attribute is not
necessary for Active Directory to function. The protocol does not define a format beyond that required
by the schema.

 cn: Additional-Trusted-Service-Names
 ldapDisplayName: additionalTrustedServiceNames
 attributeId: 1.2.840.113556.1.4.889
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 032160be-9824-11d1-aec0-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

26 / 140


### 2.45 Attribute addressBookRoots

This attribute is used by Microsoft Exchange Server and is not necessary for Active Directory
functioning. It specifies the trees of address book containers to appear in the Messaging Application
Programming Interface (MAPI) address book.

 cn: Address-Book-Roots
 ldapDisplayName: addressBookRoots
 attributeId: 1.2.840.113556.1.4.1244
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: f70b6e48-06f4-11d2-aa53-00c04fd7d83a
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.46 Attribute addressBookRoots2

This attribute is used by Exchange Server and is not necessary for Active Directory functioning. It
specifies the trees of address book containers to appear in the MAPI address book. Similar to
addressBookRoots, it differs by being a linked attribute.

 cn: Address-Book-Roots2
 ldapDisplayName: addressBookRoots2
 attributeId: 1.2.840.113556.1.4.2046
 attributeSyntax: 2.5.5.1
 linkID: 2122
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 508ca374-a511-4e4e-9f4f-856f61a6b7e4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows Server 2008.

### 2.47 Attribute addressEntryDisplayTable

This attribute is used by Exchange Server and is not necessary for Active Directory functioning. It
specifies the display table for an address entry.

 cn: Address-Entry-Display-Table
 ldapDisplayName: addressEntryDisplayTable
 attributeId: 1.2.840.113556.1.2.324
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5fd42461-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

27 / 140


 rangeUpper: 32768
 mapiID: 32791
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.48 Attribute addressEntryDisplayTableMSDOS

This attribute is used by Exchange Server and is not necessary for Active Directory functioning. It
specifies the MAPI display table for an address entry for an MS-DOS client.

 cn: Address-Entry-Display-Table-MSDOS
 ldapDisplayName: addressEntryDisplayTableMSDOS
 attributeId: 1.2.840.113556.1.2.400
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5fd42462-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32768
 mapiID: 32839
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.49 Attribute addressSyntax

This attribute is used by Exchange Server and is not necessary for Active Directory functioning. It
specifies a grammar for encoding the display table properties as a string.

 cn: Address-Syntax
 ldapDisplayName: addressSyntax
 attributeId: 1.2.840.113556.1.2.255
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5fd42463-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 4096
 mapiID: 32792
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

28 / 140


### 2.50 Attribute addressType

This attribute is used by Exchange Server and is not necessary for Active Directory functioning. It
specifies a character string describing the format of the user's address. Address types map to address
formats. That is, by looking at a recipient's address type, client applications can determine how to
format an address appropriate for the recipient.

 cn: Address-Type
 ldapDisplayName: addressType
 attributeId: 1.2.840.113556.1.2.350
 attributeSyntax: 2.5.5.4
 omSyntax: 20
 isSingleValued: TRUE
 schemaIdGuid: 5fd42464-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32
 mapiID: 32840
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.51 Attribute adminContextMenu

This attribute specifies the order number and GUID of the context menu to be used on administration
screens. GUID is defined in [MS-DTYP] section 2.3.4.

 cn: Admin-Context-Menu
 ldapDisplayName: adminContextMenu
 attributeId: 1.2.840.113556.1.4.614
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 553fd038-f32e-11d0-b0bc-00c04fd8dca6
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.52 Attribute adminCount

This attribute specifies that a given object has had its access control lists (ACLs) changed to a more
secure value by the Active Directory system [MS-ADOD] because it is a member of one of the
administrative groups, either directly or transitively. For more information on the ACL structure, see
[MSDN-ACL].

 cn: Admin-Count
 ldapDisplayName: adminCount
 attributeId: 1.2.840.113556.1.4.150
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf967918-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

29 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.53 Attribute adminDescription

This attribute specifies the description displayed on administration screens.

 cn: Admin-Description
 ldapDisplayName: adminDescription
 attributeId: 1.2.840.113556.1.2.226
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967919-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 1024
 attributeSecurityGuid: 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
 mapiID: 32842
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.54 Attribute adminDisplayName

This attribute specifies the name to be displayed on administration screens.

 cn: Admin-Display-Name
 ldapDisplayName: adminDisplayName
 attributeId: 1.2.840.113556.1.2.194
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96791a-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256
 mapiID: 32843
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.55 Attribute adminMultiselectPropertyPages

This attribute specifies the GUID of a Component Object Model (COM) object that implements
multiselect property pages for the Active Directory Users and Computers snap-in.

 cn: Admin-Multiselect-Property-Pages
 ldapDisplayName: adminMultiselectPropertyPages

30 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 attributeId: 1.2.840.113556.1.4.1690
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 18f9b67d-5ac6-4b3b-97db-d0a406afb7ba
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows Server 2003 operating system.

### 2.56 Attribute adminPropertyPages

This attribute specifies the GUID of the property pages for an object to be displayed on Active
Directory administration screens. For more information, see the document, "Extending the User
Interface for Directory Objects" [MSDN-ExtUserIntDirObj].

 cn: Admin-Property-Pages
 ldapDisplayName: adminPropertyPages
 attributeId: 1.2.840.113556.1.4.562
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 52458038-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.57 Attribute allowedAttributes

This attribute specifies attributes that will be permitted to be assigned to a class.

 cn: Allowed-Attributes
 ldapDisplayName: allowedAttributes
 attributeId: 1.2.840.113556.1.4.913
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad940-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.58 Attribute allowedAttributesEffective

This attribute specifies a list of attributes that can be modified on the object.

 cn: Allowed-Attributes-Effective
 ldapDisplayName: allowedAttributesEffective

31 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 attributeId: 1.2.840.113556.1.4.914
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad941-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.59 Attribute allowedChildClasses

This attribute specifies classes that can be contained by a class.

 cn: Allowed-Child-Classes
 ldapDisplayName: allowedChildClasses
 attributeId: 1.2.840.113556.1.4.911
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad942-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.60 Attribute allowedChildClassesEffective

This attribute specifies a list of classes that can be modified.

 cn: Allowed-Child-Classes-Effective
 ldapDisplayName: allowedChildClassesEffective
 attributeId: 1.2.840.113556.1.4.912
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad943-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

32 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.61 Attribute altSecurityIdentities

This attribute specifies a given user mapping for [X509] certificates or external Kerberos user
accounts for the purpose of authentication.

 cn: Alt-Security-Identities
 ldapDisplayName: altSecurityIdentities
 attributeId: 1.2.840.113556.1.4.867
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 00fbf30c-91fe-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: fATTINDEX
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.62 Attribute aNR

This attribute specifies whether ambiguous name resolution is to be used when choosing between
objects.

 cn: ANR
 ldapDisplayName: aNR
 attributeId: 1.2.840.113556.1.4.1208
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 45b01500-c419-11d1-bbc9-0080c76670c0
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.63 Attribute applicationName

This attribute is used to store the name of the application.

 cn: Application-Name
 ldapDisplayName: applicationName
 attributeId: 1.2.840.113556.1.4.218
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: dd712226-10e4-11d0-a05f-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 64

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

33 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.64 Attribute appliesTo

This attribute specifies the list of object classes that an extended right applies to. For more
information on Active Directory object classes, see [MS-ADTS].

 cn: Applies-To
 ldapDisplayName: appliesTo
 attributeId: 1.2.840.113556.1.4.341
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 8297931d-86d3-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.65 Attribute appSchemaVersion

This attribute specifies the schema version of the directory. It is used to provide correct behavior
across schema changes. For more information on the schema, see [MS-ADTS] section 3.1.1.2.

 cn: App-Schema-Version
 ldapDisplayName: appSchemaVersion
 attributeId: 1.2.840.113556.1.4.848
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 96a7dd65-9118-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.66 Attribute assetNumber

This attribute is used to store the tracking number of the object.

 cn: Asset-Number
 ldapDisplayName: assetNumber
 attributeId: 1.2.840.113556.1.4.283
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: ba305f75-47e3-11d0-a1a6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

34 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.67 Attribute assistant

This attribute can be used to store the distinguished name of an administrative assistant for a user.

 cn: Assistant
 ldapDisplayName: assistant
 attributeId: 1.2.840.113556.1.4.652
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 0296c11c-40da-11d1-a9c0-0000f80367c1
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.68 Attribute associatedDomain

The associatedDomain attribute type specifies a fully qualified domain name (FQDN) (2) ([MS-ADTS]
section 1.1) associated with an object.

 cn: associatedDomain
 ldapDisplayName: associatedDomain
 attributeId: 0.9.2342.19200300.100.1.37
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: FALSE
 schemaIdGuid: 3320fc38-c379-4c17-a510-1bdf6133c5da
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.69 Attribute associatedName

The associatedName attribute type specifies an entry in the directory associated with a DNS domain.

 cn: associatedName
 ldapDisplayName: associatedName
 attributeId: 0.9.2342.19200300.100.1.38
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: f7fbfc45-85ab-42a4-a435-780e62f7858b
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows Server 2003.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

35 / 140


### 2.70 Attribute assocNTAccount

The Windows NT operating system account that applies to this object.

 cn: Assoc-NT-Account
 ldapDisplayName: assocNTAccount
 attributeId: 1.2.840.113556.1.4.1213
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 398f63c0-ca60-11d1-bbd1-0000f81f10c0
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.71 Attribute attributeCertificateAttribute

A digitally signed or certified identity and set of attributes. Used to bind authorization information to
an identity.

 cn: attributeCertificateAttribute
 ldapDisplayName: attributeCertificateAttribute
 attributeId: 2.5.4.58
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: fa4693bb-7bc2-4cb9-81a8-c99c43b7905e
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.72 Attribute attributeDisplayNames

The name to be displayed for this object.

 cn: Attribute-Display-Names
 ldapDisplayName: attributeDisplayNames
 attributeId: 1.2.840.113556.1.4.748
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: cb843f80-48d9-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.73 Attribute attributeID

This attribute specifies the unique X.500 object identifier (OID) for identifying an attribute. For more
information, see [X500].

 cn: Attribute-ID
 ldapDisplayName: attributeID
 attributeId: 1.2.840.113556.1.2.30

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

36 / 140


 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: TRUE
 schemaIdGuid: bf967922-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.74 Attribute attributeSecurityGUID

This attribute specifies the GUID used to apply security credentials to a set of objects.

 cn: Attribute-Security-GUID
 ldapDisplayName: attributeSecurityGUID
 attributeId: 1.2.840.113556.1.4.149
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf967924-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.75 Attribute attributeSyntax

This attribute specifies the OID for the syntax for this attribute.

 cn: Attribute-Syntax
 ldapDisplayName: attributeSyntax
 attributeId: 1.2.840.113556.1.2.32
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: TRUE
 schemaIdGuid: bf967925-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.76 Attribute attributeTypes

A multivalued property containing strings that represent each attribute in the schema.

37 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Attribute-Types
 ldapDisplayName: attributeTypes
 attributeId: 2.5.21.5
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad944-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.77 Attribute audio

This attribute can be used to store audio.

 cn: audio
 ldapDisplayName: audio
 attributeId: 0.9.2342.19200300.100.1.55
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: d0e1d224-e1a0-42ce-a2da-793ba5244f35
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 250000
 showInAdvancedViewOnly: FALSE

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.78 Attribute auditingPolicy

This attribute specifies the auditing policy for the local policy.

 cn: Auditing-Policy
 ldapDisplayName: auditingPolicy
 attributeId: 1.2.840.113556.1.4.202
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 6da8a4fe-0e52-11d0-a286-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.79 Attribute authenticationOptions

This attribute specifies the authentication options used in the Active Directory Service Interface (ADSI)
to bind to directory services objects.

38 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Authentication-Options
 ldapDisplayName: authenticationOptions
 attributeId: 1.2.840.113556.1.4.11
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf967928-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.80 Attribute authorityRevocationList

Cross-certificate, certificate revocation list.

 cn: Authority-Revocation-List
 ldapDisplayName: authorityRevocationList
 attributeId: 2.5.4.38
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 1677578d-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10485760
 mapiID: 32806
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

### 2.81 Attribute auxiliaryClass

This attribute specifies the list of auxiliary classes to be associated with this class.

 cn: Auxiliary-Class
 ldapDisplayName: auxiliaryClass
 attributeId: 1.2.840.113556.1.2.351
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: FALSE
 schemaIdGuid: bf96792c-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

39 / 140


### 2.82 Attribute badPasswordTime

This attribute specifies the last time and date that an attempt to log on to this account was made with
an invalid password. This value is stored as a large integer that represents the number of 100
nanosecond intervals since January 1, 1601 (UTC). A value of zero means that the last invalid
password time is unknown.

 cn: Bad-Password-Time
 ldapDisplayName: badPasswordTime
 attributeId: 1.2.840.113556.1.4.49
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf96792d-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.83 Attribute badPwdCount

This attribute specifies the number of times the user tried to log on to the account by using an
incorrect password. A value of 0 indicates that the value is unknown.

 cn: Bad-Pwd-Count
 ldapDisplayName: badPwdCount
 attributeId: 1.2.840.113556.1.4.12
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf96792e-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.84 Attribute birthLocation

This attribute specifies the location of a system object, such as a file, at the time that it was originally
created.

 cn: Birth-Location
 ldapDisplayName: birthLocation
 attributeId: 1.2.840.113556.1.4.332
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 1f0075f9-7e40-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 32

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

40 / 140


 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.85 Attribute bootFile

This attribute specifies the boot image name.

 cn: BootFile
 ldapDisplayName: bootFile
 attributeId: 1.3.6.1.1.1.1.24
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: FALSE
 schemaIdGuid: e3f3cb4e-0f20-42eb-9703-d2ff26e52667
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10240

Version-Specific Behavior: First implemented on Windows Server 2003 R2 operating system.

### 2.86 Attribute bootParameter

This attribute specifies the rpc.bootparameter.

 cn: BootParameter
 ldapDisplayName: bootParameter
 attributeId: 1.3.6.1.1.1.1.23
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: FALSE
 schemaIdGuid: d72a0750-8c7c-416e-8714-e65f11e908be
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10240

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.87 Attribute bridgeheadServerListBL

This attribute is the back link attribute of bridgeheadServerList and contains the list of servers that are
bridgeheads for Active Directory replication.

 cn: Bridgehead-Server-List-BL
 ldapDisplayName: bridgeheadServerListBL
 attributeId: 1.2.840.113556.1.4.820
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: d50c2cdb-8951-11d1-aebc-0000f80367c1
 systemOnly: TRUE
 searchFlags: 0
 linkID: 99
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

41 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.88 Attribute bridgeheadTransportList

This attribute specifies the replication transports for which this server is an Active Directory
bridgehead DC.

 cn: Bridgehead-Transport-List
 ldapDisplayName: bridgeheadTransportList
 attributeId: 1.2.840.113556.1.4.819
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: d50c2cda-8951-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 linkID: 98
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.89 Attribute buildingName

This attribute specifies the name of the building where an organization or organizational unit is based.

 cn: buildingName
 ldapDisplayName: buildingName
 attributeId: 0.9.2342.19200300.100.1.48
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: f87fa54b-b2c5-4fd7-88c0-daccb21d93c5
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.90 Attribute builtinCreationTime

This attribute is used to support replication to Windows NT 4.0 operating system domains.

 cn: Builtin-Creation-Time
 ldapDisplayName: builtinCreationTime
 attributeId: 1.2.840.113556.1.4.13
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf96792f-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

42 / 140


### 2.91 Attribute builtinModifiedCount

This attribute is used to support replication to Windows NT 4.0 domains.

 cn: Builtin-Modified-Count
 ldapDisplayName: builtinModifiedCount
 attributeId: 1.2.840.113556.1.4.14
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967930-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.92 Attribute businessCategory

This attribute specifies descriptive text on an organizational unit.

 cn: Business-Category
 ldapDisplayName: businessCategory
 attributeId: 2.5.4.15
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf967931-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 128
 mapiID: 32855
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.93 Attribute bytesPerMinute

This attribute specifies the printer data transfer rate.

 cn: Bytes-Per-Minute
 ldapDisplayName: bytesPerMinute
 attributeId: 1.2.840.113556.1.4.284
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: ba305f76-47e3-11d0-a1a6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.94 Attribute c

This attribute specifies the country/region in the address of the user. The country/region is
represented as the two-character country code based on [ISO-3166].

43 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Country-Name
 ldapDisplayName: c
 attributeId: 2.5.4.6
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967945-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 1
 rangeUpper: 3
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 32873
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.95 Attribute cACertificate

This attribute specifies certificates of trusted certificate authorities (CAs).

 cn: CA-Certificate
 ldapDisplayName: cACertificate
 attributeId: 2.5.4.37
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: bf967932-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32768
 mapiID: 32771
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.96 Attribute cACertificateDN

This attribute specifies the full distinguished name from the certificate authority (CA) certificate.

 cn: CA-Certificate-DN
 ldapDisplayName: cACertificateDN
 attributeId: 1.2.840.113556.1.4.697
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2740-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

44 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.97 Attribute cAConnect

This attribute specifies the connection string for binding to a CA.

 cn: CA-Connect
 ldapDisplayName: cAConnect
 attributeId: 1.2.840.113556.1.4.687
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2735-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.98 Attribute canonicalName

This attribute specifies the name of the object in canonical format;
myserver2.fabrikam.com/users/jeffsmith is an example of a distinguished name in canonical format.

This is a constructed attribute. The results returned are identical to those returned by the following
Active Directory function: DsCrackNames(NULL, DS_NAME_FLAG_SYNTACTICAL_ONLY,
DS_FQDN_1779_NAME, DS_CANONICAL_NAME, ...).

 cn: Canonical-Name
 ldapDisplayName: canonicalName
 attributeId: 1.2.840.113556.1.4.916
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad945-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.99 Attribute canUpgradeScript

This attribute specifies the list of application packages that can be upgraded by this application
package or that can upgrade this application package.

 cn: Can-Upgrade-Script
 ldapDisplayName: canUpgradeScript
 attributeId: 1.2.840.113556.1.4.815
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: d9e18314-8939-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

45 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.100 Attribute carLicense

This attribute can be used to store a vehicle license or registration plate.

 cn: carLicense
 ldapDisplayName: carLicense
 attributeId: 2.16.840.1.113730.3.1.1
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: d4159c92-957d-4a87-8a67-8d2934e01649
 systemOnly: FALSE
 searchFlags: 0
 showInAdvancedViewOnly: FALSE

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.101 Attribute catalogs

This attribute specifies the list of catalogs indexing storage on a given computer.

 cn: Catalogs
 ldapDisplayName: catalogs
 attributeId: 1.2.840.113556.1.4.675
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7bfdcb81-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.102 Attribute categories

This attribute specifies a list of category IDs (GUIDs) for categories that apply to this application.

 cn: Categories
 ldapDisplayName: categories
 attributeId: 1.2.840.113556.1.4.672
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7bfdcb7e-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.103 Attribute categoryId

This attribute specifies the ID for a component category.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

46 / 140


 cn: Category-Id
 ldapDisplayName: categoryId
 attributeId: 1.2.840.113556.1.4.322
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 7d6c0e94-7e20-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.104 Attribute cAUsages

This attribute specifies the list of OID/cryptographic service provider (CSP) name concatenations.

 cn: CA-Usages
 ldapDisplayName: cAUsages
 attributeId: 1.2.840.113556.1.4.690
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 963d2738-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.105 Attribute cAWEBURL

This attribute specifies the URL for an HTTP connection to a CA.

 cn: CA-WEB-URL
 ldapDisplayName: cAWEBURL
 attributeId: 1.2.840.113556.1.4.688
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2736-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.106 Attribute certificateAuthorityObject

This attribute specifies a reference to the CA associated with a certificate revocation list (CRL)
distribution point.

 cn: Certificate-Authority-Object
 ldapDisplayName: certificateAuthorityObject
 attributeId: 1.2.840.113556.1.4.684
 attributeSyntax: 2.5.5.1

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

47 / 140


 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 963d2732-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

On Windows 2000 Server, rangeUpper is not defined.

### 2.107 Attribute certificateRevocationList

This attribute represents a list of certificates that have been revoked.

 cn: Certificate-Revocation-List
 ldapDisplayName: certificateRevocationList
 attributeId: 2.5.4.39
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 1677579f-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10485760
 mapiID: 32790
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

### 2.108 Attribute certificateTemplates

This attribute contains information for a certificate issued by a certificate server.

 cn: Certificate-Templates
 ldapDisplayName: certificateTemplates
 attributeId: 1.2.840.113556.1.4.823
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 2a39c5b1-8960-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.109 Attribute classDisplayName

This attribute specifies the object name to be displayed on dialogs.

 cn: Class-Display-Name
 ldapDisplayName: classDisplayName
 attributeId: 1.2.840.113556.1.4.610

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

48 / 140


 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 548e1c22-dea6-11d0-b010-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.110 Attribute cn

This attribute specifies the name that represents an object. It is used to perform searches.

 cn: Common-Name
 ldapDisplayName: cn
 attributeId: 2.5.4.3
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96793f-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14863
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.111 Attribute co

This attribute specifies the country/region in which the user is located.

 cn: Text-Country
 ldapDisplayName: co
 attributeId: 1.2.840.113556.1.2.131
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ffa7-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 1
 rangeUpper: 128
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14886
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

49 / 140


### 2.112 Attribute codePage

This attribute specifies the code page for the user's language of choice. The space of values is the
Microsoft code page designation. For more information, see [MSDN-CP].

 cn: Code-Page
 ldapDisplayName: codePage
 attributeId: 1.2.840.113556.1.4.16
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf967938-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 0
 rangeUpper: 65535
 attributeSecurityGuid: 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeLower and rangeUpper are not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.113 Attribute cOMClassID

This attribute specifies the list of ClassIDs implemented in this application package.

 cn: COM-ClassID
 ldapDisplayName: cOMClassID
 attributeId: 1.2.840.113556.1.4.19
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf96793b-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.114 Attribute cOMCLSID

This attribute specifies the GUID associated with this object class.

 cn: COM-CLSID
 ldapDisplayName: cOMCLSID
 attributeId: 1.2.840.113556.1.4.249
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 281416d9-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

50 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.115 Attribute cOMInterfaceID

This attribute specifies the list of interfaces implemented in this application package.

 cn: COM-InterfaceID
 ldapDisplayName: cOMInterfaceID
 attributeId: 1.2.840.113556.1.4.20
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf96793c-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.116 Attribute comment

This attribute can be used to store a comment for a user.

 cn: User-Comment
 ldapDisplayName: comment
 attributeId: 1.2.840.113556.1.4.156
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967a6a-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.117 Attribute cOMOtherProgId

This attribute specifies the list of other program ID strings for the host class.

 cn: COM-Other-Prog-Id
 ldapDisplayName: cOMOtherProgId
 attributeId: 1.2.840.113556.1.4.253
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 281416dd-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

51 / 140


### 2.118 Attribute company

This attribute can be used to store a company name for a user.

 cn: Company
 ldapDisplayName: company
 attributeId: 1.2.840.113556.1.2.146
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ff88-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14870
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.119 Attribute cOMProgID

This attribute specifies the list of COM object program IDs implemented in this application package.

 cn: COM-ProgID
 ldapDisplayName: cOMProgID
 attributeId: 1.2.840.113556.1.4.21
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf96793d-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.120 Attribute cOMTreatAsClassId

This attribute specifies the Treat-As string GUID class identifier (CLSID) for the host class.

 cn: COM-Treat-As-Class-Id
 ldapDisplayName: cOMTreatAsClassId
 attributeId: 1.2.840.113556.1.4.251
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 281416db-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

52 / 140


### 2.121 Attribute cOMTypelibId

This attribute specifies the list of type library IDs contained in this application package.

 cn: COM-Typelib-Id
 ldapDisplayName: cOMTypelibId
 attributeId: 1.2.840.113556.1.4.254
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 281416de-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.122 Attribute cOMUniqueLIBID

This attribute specifies a single-valued string GUID LIBID for a type library.

 cn: COM-Unique-LIBID
 ldapDisplayName: cOMUniqueLIBID
 attributeId: 1.2.840.113556.1.4.250
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 281416da-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 36
 rangeUpper: 36
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.123 Attribute contentIndexingAllowed

Indicates whether the volume object can be content indexed.

 cn: Content-Indexing-Allowed
 ldapDisplayName: contentIndexingAllowed
 attributeId: 1.2.840.113556.1.4.24
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf967943-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.124 Attribute contextMenu

This attribute specifies the order number and GUID of the context menu to be used for an object.

53 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Context-Menu
 ldapDisplayName: contextMenu
 attributeId: 1.2.840.113556.1.4.499
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 4d8601ee-ac85-11d0-afe3-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.125 Attribute controlAccessRights

This attribute is used by DS Security to determine which users can perform specific operations on the
host object.

 cn: Control-Access-Rights
 ldapDisplayName: controlAccessRights
 attributeId: 1.2.840.113556.1.4.200
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 6da8a4fc-0e52-11d0-a286-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.126 Attribute cost

This attribute contains the relative cost for routing messages through a particular site connector.

 cn: Cost
 ldapDisplayName: cost
 attributeId: 1.2.840.113556.1.2.135
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf967944-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 mapiID: 32872
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.127 Attribute countryCode

This attribute specifies the country code for the user's language of choice.

 cn: Country-Code

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

54 / 140


 ldapDisplayName: countryCode
 attributeId: 1.2.840.113556.1.4.25
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 5fd42471-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 0
 rangeUpper: 65535
 attributeSecurityGuid: 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeLower and rangeUpper are not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.128 Attribute createDialog

This attribute specifies the GUID of the dialog for creating an associated object.

 cn: Create-Dialog
 ldapDisplayName: createDialog
 attributeId: 1.2.840.113556.1.4.810
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2b09958a-8931-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.129 Attribute createTimeStamp

This attribute specifies the date this object was created. This value is replicated.

 cn: Create-Time-Stamp
 ldapDisplayName: createTimeStamp
 attributeId: 2.5.18.1
 attributeSyntax: 2.5.5.11
 omSyntax: 24
 isSingleValued: TRUE
 schemaIdGuid: 2df90d73-009f-11d2-aa4c-00c04fd7d83a
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

55 / 140


### 2.130 Attribute createWizardExt

The GUID of wizard extensions for creating an associated object.

 cn: Create-Wizard-Ext
 ldapDisplayName: createWizardExt
 attributeId: 1.2.840.113556.1.4.812
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 2b09958b-8931-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.131 Attribute creationTime

This attribute specifies the date and time that the object was created.

 cn: Creation-Time
 ldapDisplayName: creationTime
 attributeId: 1.2.840.113556.1.4.26
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967946-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.132 Attribute creationWizard

This attribute specifies the wizard to activate when creating objects of this class.

 cn: Creation-Wizard
 ldapDisplayName: creationWizard
 attributeId: 1.2.840.113556.1.4.498
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 4d8601ed-ac85-11d0-afe3-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.133 Attribute creator

This attribute specifies the person who created the object.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

56 / 140


 cn: Creator
 ldapDisplayName: creator
 attributeId: 1.2.840.113556.1.4.679
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 7bfdcb85-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.134 Attribute cRLObject

This attribute specifies the reference to the CRL object associated with a CA.

 cn: CRL-Object
 ldapDisplayName: cRLObject
 attributeId: 1.2.840.113556.1.4.689
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 963d2737-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.135 Attribute cRLPartitionedRevocationList

This attribute specifies the public key infrastructure–revocation lists.

 cn: CRL-Partitioned-Revocation-List
 ldapDisplayName: cRLPartitionedRevocationList
 attributeId: 1.2.840.113556.1.4.683
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 963d2731-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10485760
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

### 2.136 Attribute crossCertificatePair

This attribute specifies the version 3 (v3) cross-certificate.

 cn: Cross-Certificate-Pair
 ldapDisplayName: crossCertificatePair
 attributeId: 2.5.4.40

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

57 / 140


 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 167757b2-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 32768
 mapiID: 32805
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

### 2.137 Attribute currentLocation

This attribute specifies the computer location for an object that has moved.

 cn: Current-Location
 ldapDisplayName: currentLocation
 attributeId: 1.2.840.113556.1.4.335
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 1f0075fc-7e40-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 32
 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.138 Attribute currentParentCA

This attribute specifies a reference to the CAs that issued the current certificates for a CA.

 cn: Current-Parent-CA
 ldapDisplayName: currentParentCA
 attributeId: 1.2.840.113556.1.4.696
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 963d273f-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.139 Attribute currentValue

This attribute is used to store the new value of a secret object. Secret objects are specified in [MS-
LSAD] section 3.1.1.4. The format of the value of this attribute is outside the scope of the state
model, and values stored in this attribute cannot be retrieved via the Lightweight Directory Access
Protocol (LDAP). Instead, secret objects are retrieved and written as specified in [MS-LSAD] section
3.1.1.4.

58 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Current-Value
 ldapDisplayName: currentValue
 attributeId: 1.2.840.113556.1.4.27
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf967947-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.140 Attribute currMachineId

This attribute specifies the ID of the machine where a Link-Track-Vol-Entry object is located.

 cn: Curr-Machine-Id
 ldapDisplayName: currMachineId
 attributeId: 1.2.840.113556.1.4.337
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 1f0075fe-7e40-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.141 Attribute dBCSPwd

This attribute specifies the account's LAN Manager password.

For more information, see [MS-SAMR] section 3.1.1.8.6.

 cn: DBCS-Pwd
 ldapDisplayName: dBCSPwd
 attributeId: 1.2.840.113556.1.4.55
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf96799c-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

59 / 140


### 2.142 Attribute dc

This attribute specifies the naming attribute for domain and DNS objects. Usually displayed as
dc=DomainName.

 cn: Domain-Component
 ldapDisplayName: dc
 attributeId: 0.9.2342.19200300.100.1.25
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 19195a55-6da0-11d0-afd3-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 255
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.143 Attribute defaultClassStore

This attribute specifies the default Class Store for a given user.

 cn: Default-Class-Store
 ldapDisplayName: defaultClassStore
 attributeId: 1.2.840.113556.1.4.213
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: bf967948-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.144 Attribute defaultGroup

This attribute specifies the group to which this object is assigned when it is created.

 cn: Default-Group
 ldapDisplayName: defaultGroup
 attributeId: 1.2.840.113556.1.4.480
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 720bc4e2-a54a-11d0-afdf-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

60 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.145 Attribute defaultHidingValue

This attribute specifies a Boolean value that specifies the default setting of the
showInAdvancedViewOnly property of new instances of this class.

 cn: Default-Hiding-Value
 ldapDisplayName: defaultHidingValue
 attributeId: 1.2.840.113556.1.4.518
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: b7b13116-b82e-11d0-afee-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.146 Attribute defaultLocalPolicyObject

This attribute specifies a reference to a policy object that defines the local policy for the host object.

 cn: Default-Local-Policy-Object
 ldapDisplayName: defaultLocalPolicyObject
 attributeId: 1.2.840.113556.1.4.57
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: bf96799f-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.147 Attribute defaultObjectCategory

This attribute specifies the value to use for the objectCategory attribute (see [MS-ADA3] section 2.39)
if one is not specified on object instantiation. For more information on the defaultObjectCategory
attribute, see [MS-ADTS] section 3.1.1.2.4.8.

 cn: Default-Object-Category
 ldapDisplayName: defaultObjectCategory
 attributeId: 1.2.840.113556.1.4.783
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 26d97367-6070-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

61 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.148 Attribute defaultPriority

The default priority (for example, of a process or a print job).

 cn: Default-Priority
 ldapDisplayName: defaultPriority
 attributeId: 1.2.840.113556.1.4.232
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 281416c8-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.149 Attribute defaultSecurityDescriptor

This attribute specifies the security descriptor to be assigned to the object when it is created.

 cn: Default-Security-Descriptor
 ldapDisplayName: defaultSecurityDescriptor
 attributeId: 1.2.840.113556.1.4.224
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 807a6d30-1669-11d0-a064-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32767
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.150 Attribute deltaRevocationList

This list contains certificates revoked since the last delta update.

 cn: Delta-Revocation-List
 ldapDisplayName: deltaRevocationList
 attributeId: 2.5.4.53
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 167757b5-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10485760
 mapiID: 35910

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

62 / 140


In Windows 2000 Server, rangeUpper is not defined.

### 2.151 Attribute department

This attribute contains the name of the user's department.

 cn: Department
 ldapDisplayName: department
 attributeId: 1.2.840.113556.1.2.141
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96794f-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14872
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.152 Attribute departmentNumber

This attribute can be used to store a department number within an organization.

 cn: departmentNumber
 ldapDisplayName: departmentNumber
 attributeId: 2.16.840.1.113730.3.1.2
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: be9ef6ee-cbc7-4f22-b27b-96967e7ee585
 systemOnly: FALSE
 searchFlags: 0
 showInAdvancedViewOnly: FALSE

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.153 Attribute description

This attribute specifies the description to display for an object.

 cn: Description
 ldapDisplayName: description
 attributeId: 2.5.4.13
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf967950-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 1024
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 32879
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

63 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.154 Attribute desktopProfile

This attribute specifies the location of the desktop profile for a user or group of users.

 cn: Desktop-Profile
 ldapDisplayName: desktopProfile
 attributeId: 1.2.840.113556.1.4.346
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: eea65906-8ac6-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.155 Attribute destinationIndicator

This is part of the [X500] specification.

 cn: Destination-Indicator
 ldapDisplayName: destinationIndicator
 attributeId: 2.5.4.27
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: bf967951-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 128
 mapiID: 32880
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.156 Attribute dhcpClasses

 cn: dhcp-Classes
 ldapDisplayName: dhcpClasses
 attributeId: 1.2.840.113556.1.4.715
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 963d2750-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.157 Attribute dhcpFlags

 cn: dhcp-Flags

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

64 / 140


 ldapDisplayName: dhcpFlags
 attributeId: 1.2.840.113556.1.4.700
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 963d2741-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.158 Attribute dhcpIdentification

 cn: dhcp-Identification
 ldapDisplayName: dhcpIdentification
 attributeId: 1.2.840.113556.1.4.701
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2742-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.159 Attribute dhcpMask

 cn: dhcp-Mask
 ldapDisplayName: dhcpMask
 attributeId: 1.2.840.113556.1.4.706
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d2747-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.160 Attribute dhcpMaxKey

 cn: dhcp-MaxKey
 ldapDisplayName: dhcpMaxKey
 attributeId: 1.2.840.113556.1.4.719
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 963d2754-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

65 / 140


### 2.161 Attribute dhcpObjDescription

 cn: dhcp-Obj-Description
 ldapDisplayName: dhcpObjDescription
 attributeId: 1.2.840.113556.1.4.703
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2744-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.162 Attribute dhcpObjName

 cn: dhcp-Obj-Name
 ldapDisplayName: dhcpObjName
 attributeId: 1.2.840.113556.1.4.702
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 963d2743-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.163 Attribute dhcpOptions

 cn: dhcp-Options
 ldapDisplayName: dhcpOptions
 attributeId: 1.2.840.113556.1.4.714
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 963d274f-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.164 Attribute dhcpProperties

 cn: dhcp-Properties
 ldapDisplayName: dhcpProperties
 attributeId: 1.2.840.113556.1.4.718
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 963d2753-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

66 / 140


### 2.165 Attribute dhcpRanges

 cn: dhcp-Ranges
 ldapDisplayName: dhcpRanges
 attributeId: 1.2.840.113556.1.4.707
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d2748-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.166 Attribute dhcpReservations

 cn: dhcp-Reservations
 ldapDisplayName: dhcpReservations
 attributeId: 1.2.840.113556.1.4.709
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d274a-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.167 Attribute dhcpServers

This attribute contains a list of servers authorized in the enterprise. This attribute is sent by the
Dynamic Host Configuration Protocol (DHCP) server and can contain either the name of the server or
its IP address.

 cn: dhcp-Servers
 ldapDisplayName: dhcpServers
 attributeId: 1.2.840.113556.1.4.704
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d2745-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 extendedCharsAllowed: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, extendedCharsAllowed is not defined.

### 2.168 Attribute dhcpSites

 cn: dhcp-Sites
 ldapDisplayName: dhcpSites
 attributeId: 1.2.840.113556.1.4.708
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

67 / 140


 schemaIdGuid: 963d2749-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.169 Attribute dhcpState

 cn: dhcp-State
 ldapDisplayName: dhcpState
 attributeId: 1.2.840.113556.1.4.717
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d2752-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.170 Attribute dhcpSubnets

 cn: dhcp-Subnets
 ldapDisplayName: dhcpSubnets
 attributeId: 1.2.840.113556.1.4.705
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: FALSE
 schemaIdGuid: 963d2746-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.171 Attribute dhcpType

The type of DHCP server. This attribute is sent by the DHCP server during authorization and is
currently being set to 0.

 cn: dhcp-Type
 ldapDisplayName: dhcpType
 attributeId: 1.2.840.113556.1.4.699
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 963d273b-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

68 / 140


### 2.172 Attribute dhcpUniqueKey

 cn: dhcp-Unique-Key
 ldapDisplayName: dhcpUniqueKey
 attributeId: 1.2.840.113556.1.4.698
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 963d273a-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.173 Attribute dhcpUpdateTime

 cn: dhcp-Update-Time
 ldapDisplayName: dhcpUpdateTime
 attributeId: 1.2.840.113556.1.4.720
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 963d2755-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.174 Attribute directReports

This attribute contains the list of users who directly report to a user. The users listed as reports are
those who have their property-manager property set to this user. Each item in the list is a linked
reference to the object that represents the corresponding user.

 cn: Reports
 ldapDisplayName: directReports
 attributeId: 1.2.840.113556.1.2.436
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: bf967a1c-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 linkID: 43
 mapiID: 32782
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.175 Attribute displayName

This attribute specifies the display name for an object, usually the combination of the user's first
name, middle initial, and last name.

 cn: Display-Name

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

69 / 140


 ldapDisplayName: displayName
 attributeId: 1.2.840.113556.1.2.13
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967953-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fANR | fATTINDEX
 rangeLower: 0
 rangeUpper: 256
 attributeSecurityGuid: 59ba2f42-79a2-11d0-9020-00c04fc2d3cf
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.176 Attribute displayNamePrintable

This attribute specifies the printable display name for an object, usually the combination of the user's
first name, middle initial, and last name.

 cn: Display-Name-Printable
 ldapDisplayName: displayNamePrintable
 attributeId: 1.2.840.113556.1.2.353
 attributeSyntax: 2.5.5.5
 omSyntax: 19
 isSingleValued: TRUE
 schemaIdGuid: bf967954-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14847
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.177 Attribute distinguishedName

This attribute is the same as the distinguished name for an object and is included on all objects in
Active Directory. It is also used by Exchange Server. See [MS-ADTS] section 3.1.1.1.4 for more
information.

 cn: Obj-Dist-Name
 ldapDisplayName: distinguishedName
 attributeId: 2.5.4.49
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: bf9679e4-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

70 / 140


 mapiID: 32828
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.178 Attribute dITContentRules

This attribute specifies the permissible content of entries of a particular structural object class via the
identification of an optional set of auxiliary object classes, as well as mandatory, optional, and
precluded attributes. Collective attributes shall be included in DIT-Content-Rules, as specified in
[RFC2251] section 3.2.1.

 cn: DIT-Content-Rules
 ldapDisplayName: dITContentRules
 attributeId: 2.5.21.2
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad946-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.179 Attribute division

This attribute can be used to store the name of a division for a user.

 cn: Division
 ldapDisplayName: division
 attributeId: 1.2.840.113556.1.4.261
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: fe6136a0-2073-11d0-a9c2-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 0
 rangeUpper: 256
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.180 Attribute dMDLocation

This attribute specifies the distinguished name to the schema partition.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

71 / 140


 cn: DMD-Location
 ldapDisplayName: dMDLocation
 attributeId: 1.2.840.113556.1.2.36
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: f0f8ff8b-1191-11d0-a060-00aa006c33ed
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.181 Attribute dmdName

This attribute specifies a name used to identify the schema partition.

 cn: DMD-Name
 ldapDisplayName: dmdName
 attributeId: 1.2.840.113556.1.2.598
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 167757b9-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 1024
 mapiID: 35926
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.182 Attribute dNReferenceUpdate

If an object is renamed, this attribute is used to track all the previous and current names assigned to
the object so that linked objects can still find it.

 cn: DN-Reference-Update
 ldapDisplayName: dNReferenceUpdate
 attributeId: 1.2.840.113556.1.4.1242
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 2df90d86-009f-11d2-aa4c-00c04fd7d83a
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

72 / 140


### 2.183 Attribute dnsAllowDynamic

 cn: Dns-Allow-Dynamic
 ldapDisplayName: dnsAllowDynamic
 attributeId: 1.2.840.113556.1.4.378
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: e0fa1e65-9b45-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.184 Attribute dnsAllowXFR

 cn: Dns-Allow-XFR
 ldapDisplayName: dnsAllowXFR
 attributeId: 1.2.840.113556.1.4.379
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: e0fa1e66-9b45-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.185 Attribute dNSHostName

This attribute specifies the name of a computer as registered in DNS.

 cn: DNS-Host-Name
 ldapDisplayName: dNSHostName
 attributeId: 1.2.840.113556.1.4.619
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 72e39547-7b18-11d1-adef-00c04fd8d5cd
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 attributeSecurityGuid: 72e39547-7b18-11d1-adef-00c04fd8d5cd
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, attributeSecurityGuid is not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.186 Attribute dnsNotifySecondaries

 cn: Dns-Notify-Secondaries
 ldapDisplayName: dnsNotifySecondaries

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

73 / 140


 attributeId: 1.2.840.113556.1.4.381
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: FALSE
 schemaIdGuid: e0fa1e68-9b45-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.187 Attribute dNSProperty

Written onto dnsZone objects. This attribute is used to store zone properties in BLOB format.

 cn: DNS-Property
 ldapDisplayName: dNSProperty
 attributeId: 1.2.840.113556.1.4.1306
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 675a15fe-3b70-11d2-90cc-00c04fd91ab1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.188 Attribute dnsRecord

This attribute is written onto dnsNode objects. Used to store DNS resource record definitions in BLOB
format.

 cn: Dns-Record
 ldapDisplayName: dnsRecord
 attributeId: 1.2.840.113556.1.4.382
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: e0fa1e69-9b45-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.189 Attribute dnsRoot

The uppermost fully qualified domain name (FQDN) (1) ([MS-ADTS] section 1.1) assigned to a domain
naming context. This is set on a crossRef object and is used, among other things, for referral
generation. A search through an entire domain tree is initiated at the Dns-Root object. This attribute
can be multivalued, in which case multiple referrals are generated.

 cn: Dns-Root
 ldapDisplayName: dnsRoot
 attributeId: 1.2.840.113556.1.4.28
 attributeSyntax: 2.5.5.12
 omSyntax: 64

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

74 / 140


 isSingleValued: FALSE
 schemaIdGuid: bf967959-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 1
 rangeUpper: 255
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.190 Attribute dnsSecureSecondaries

 cn: Dns-Secure-Secondaries
 ldapDisplayName: dnsSecureSecondaries
 attributeId: 1.2.840.113556.1.4.380
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: FALSE
 schemaIdGuid: e0fa1e67-9b45-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.191 Attribute dNSTombstoned

Set to TRUE if this object has been tombstoned. This attribute exists to make searching for
tombstoned records easier and faster.

Tombstoned objects are objects that have been deleted but not yet removed from the directory. When
the value is missing or FALSE, the DNS node is active. When the value is TRUE, the DNS node has
been logically deleted, but the dnsNode object is kept alive to avoid excess replication traffic and to
replicate node deletions between DNS servers.

 cn: DNS-Tombstoned
 ldapDisplayName: dNSTombstoned
 attributeId: 1.2.840.113556.1.4.1414
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: d5eb2eb7-be4e-463b-a214-634a44d7392e
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.192 Attribute documentAuthor

 cn: documentAuthor
 ldapDisplayName: documentAuthor
 attributeId: 0.9.2342.19200300.100.1.14
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

75 / 140


 isSingleValued: FALSE
 schemaIdGuid: f18a8e19-af5f-4478-b096-6f35c27eb83f
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.193 Attribute documentIdentifier

 cn: documentIdentifier
 ldapDisplayName: documentIdentifier
 attributeId: 0.9.2342.19200300.100.1.11
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 0b21ce82-ff63-46d9-90fb-c8b9f24e97b9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.194 Attribute documentLocation

 cn: documentLocation
 ldapDisplayName: documentLocation
 attributeId: 0.9.2342.19200300.100.1.15
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: b958b14e-ac6d-4ec4-8892-be70b69f7281
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.195 Attribute documentPublisher

This attribute specifies the person and/or organization that published a document.

 cn: documentPublisher
 ldapDisplayName: documentPublisher
 attributeId: 0.9.2342.19200300.100.1.56
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 170f09d7-eb69-448a-9a30-f1afecfd32d7
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

76 / 140


### 2.196 Attribute documentTitle

 cn: documentTitle
 ldapDisplayName: documentTitle
 attributeId: 0.9.2342.19200300.100.1.12
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: de265a9c-ff2c-47b9-91dc-6e6fe2c43062
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.197 Attribute documentVersion

 cn: documentVersion
 ldapDisplayName: documentVersion
 attributeId: 0.9.2342.19200300.100.1.13
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 94b3a8a9-d613-4cec-9aad-5fbcc1046b43
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.198 Attribute domainCAs

The Domain-Certificate-Authorities attribute contains a list of certificate authorities for a given
domain.

 cn: Domain-Certificate-Authorities
 ldapDisplayName: domainCAs
 attributeId: 1.2.840.113556.1.4.668
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 7bfdcb7a-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.199 Attribute domainCrossRef

This is a reference from a trusted domain object to the cross-reference object of the trusted domain.

 cn: Domain-Cross-Ref
 ldapDisplayName: domainCrossRef
 attributeId: 1.2.840.113556.1.4.472
 attributeSyntax: 2.5.5.1
 omSyntax: 127

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

77 / 140


 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: b000ea7b-a086-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.200 Attribute domainID

Reference to a domain associated with a CA.

 cn: Domain-ID
 ldapDisplayName: domainID
 attributeId: 1.2.840.113556.1.4.686
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 963d2734-48be-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.201 Attribute domainIdentifier

The domain security identifier (SID), as defined in [MS-DTYP] section 2.4.2, identifying the domain.
The SID can be represented in any of the three formats described in that section, depending on the
type of protocol being used to carry the information.

 cn: Domain-Identifier
 ldapDisplayName: domainIdentifier
 attributeId: 1.2.840.113556.1.4.755
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 7f561278-5301-11d1-a9c5-0000f80367c1
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.202 Attribute domainPolicyObject

Reference to the policy object defining the Local Security Authority (LSA) policy for the host domain.

 cn: Domain-Policy-Object
 ldapDisplayName: domainPolicyObject
 attributeId: 1.2.840.113556.1.4.32
 attributeSyntax: 2.5.5.1
 omSyntax: 127

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

78 / 140


 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: bf96795d-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.203 Attribute domainPolicyReference

The distinguished name of a domain policy object that a policy object copies from.

 cn: Domain-Policy-Reference
 ldapDisplayName: domainPolicyReference
 attributeId: 1.2.840.113556.1.4.422
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 80a67e2a-9f22-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: a29b89fe-c7e8-11d0-9bae-00c04fd92ef5
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.204 Attribute domainReplica

A Unicode string attribute that gives the NetBIOS name of the PDC at the time of upgrade from
Windows NT 4.0, otherwise the default value is NULL.

 cn: Domain-Replica
 ldapDisplayName: domainReplica
 attributeId: 1.2.840.113556.1.4.158
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96795e-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32767
 attributeSecurityGuid: b8119fd0-04f6-4762-ab7a-4986c76b3f9a
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, attributeSecurityGuid is not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.205 Attribute domainWidePolicy

This attribute is for user-extensible policy to be replicated to the clients.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

79 / 140


 cn: Domain-Wide-Policy
 ldapDisplayName: domainWidePolicy
 attributeId: 1.2.840.113556.1.4.421
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 80a67e29-9f22-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: a29b89fd-c7e8-11d0-9bae-00c04fd92ef5
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.206 Attribute drink

 cn: drink
 ldapDisplayName: drink
 attributeId: 0.9.2342.19200300.100.1.5
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 1a1aa5b5-262e-4df6-af04-2cf6b0d80048
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.207 Attribute driverName

 cn: Driver-Name
 ldapDisplayName: driverName
 attributeId: 1.2.840.113556.1.4.229
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 281416c5-1968-11d0-a28f-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.208 Attribute driverVersion

 cn: Driver-Version
 ldapDisplayName: driverVersion
 attributeId: 1.2.840.113556.1.4.276
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: ba305f6e-47e3-11d0-a1a6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

80 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.209 Attribute dSASignature

The DSA-Signature of an object is the Invocation-ID of the last directory to modify the object.

 cn: DSA-Signature
 ldapDisplayName: dSASignature
 attributeId: 1.2.840.113556.1.2.74
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 167757bc-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 mapiID: 32887
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.210 Attribute dSCorePropagationData

This attribute is for internal use only.

 cn: DS-Core-Propagation-Data
 ldapDisplayName: dSCorePropagationData
 attributeId: 1.2.840.113556.1.4.1357
 attributeSyntax: 2.5.5.11
 omSyntax: 24
 isSingleValued: FALSE
 schemaIdGuid: d167aa4b-8b08-11d2-9939-0000f87a57d4
 systemOnly: TRUE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.211 Attribute dSHeuristics

This attribute contains global settings for the entire forest. For more information on global settings,
see [MS-ADTS].

 cn: DS-Heuristics
 ldapDisplayName: dSHeuristics
 attributeId: 1.2.840.113556.1.2.212
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ff86-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

81 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.212 Attribute dSUIAdminMaximum

This is the default maximum number of objects that will be shown in a container by the administration
user interface (UI).

 cn: DS-UI-Admin-Maximum
 ldapDisplayName: dSUIAdminMaximum
 attributeId: 1.2.840.113556.1.4.1344
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: ee8d0ae0-6f91-11d2-9905-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.213 Attribute dSUIAdminNotification

This attribute specifies a list of the GUIDs of COM objects that support a callback interface that
DSAdmin calls when an action has occurred on an object through the UI.

 cn: DS-UI-Admin-Notification
 ldapDisplayName: dSUIAdminNotification
 attributeId: 1.2.840.113556.1.4.1343
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: f6ea0a94-6f91-11d2-9905-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.214 Attribute dSUIShellMaximum

This attribute specifies the default maximum number of objects that will be shown in a container by
the shell UI.

 cn: DS-UI-Shell-Maximum
 ldapDisplayName: dSUIShellMaximum
 attributeId: 1.2.840.113556.1.4.1345
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: fcca766a-6f91-11d2-9905-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

82 / 140


### 2.215 Attribute dynamicLDAPServer

This attribute specifies the fully qualified domain name (FQDN) (1) ([MS-ADTS] section 1.1) of the
server-handling dynamic properties for this account.

 cn: Dynamic-LDAP-Server
 ldapDisplayName: dynamicLDAPServer
 attributeId: 1.2.840.113556.1.4.537
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 52458021-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.216 Attribute eFSPolicy

The Encrypting File System (EFS) Policy.

 cn: EFSPolicy
 ldapDisplayName: eFSPolicy
 attributeId: 1.2.840.113556.1.4.268
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 8e4eb2ec-4712-11d0-a1a0-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: a29b89fd-c7e8-11d0-9bae-00c04fd92ef5
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.217 Attribute employeeID

This attribute specifies the ID of an employee.

 cn: Employee-ID
 ldapDisplayName: employeeID
 attributeId: 1.2.840.113556.1.4.35
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967962-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

83 / 140


### 2.218 Attribute employeeNumber

This attribute specifies the number assigned to an employee other than the ID.

 cn: Employee-Number
 ldapDisplayName: employeeNumber
 attributeId: 1.2.840.113556.1.2.610
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: a8df73ef-c5ea-11d1-bbcb-0080c76670c0
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 512
 mapiID: 35943

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, the following attributes are defined differently.

 systemFlags: FLAG_SCHEMA_BASE_OBJECT

### 2.219 Attribute employeeType

This attribute specifies the job category for an employee.

 cn: Employee-Type
 ldapDisplayName: employeeType
 attributeId: 1.2.840.113556.1.2.613
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: a8df73f0-c5ea-11d1-bbcb-0080c76670c0
 systemOnly: FALSE
 searchFlags: fCOPY
 rangeLower: 1
 rangeUpper: 256
 mapiID: 35945

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, the following attributes are defined differently.

 systemFlags: FLAG_SCHEMA_BASE_OBJECT

### 2.220 Attribute Enabled

This attribute is used to signify whether a given crossRef is enabled.

 cn: Enabled
 ldapDisplayName: Enabled
 attributeId: 1.2.840.113556.1.2.557
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: a8df73f2-c5ea-11d1-bbcb-0080c76670c0
 systemOnly: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

84 / 140


 searchFlags: 0
 mapiID: 35873
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.221 Attribute enabledConnection

This attribute specifies whether a connection is available for use.

 cn: Enabled-Connection
 ldapDisplayName: enabledConnection
 attributeId: 1.2.840.113556.1.4.36
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf967963-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.222 Attribute enrollmentProviders

Public key infrastructure (PKI) certificate templates.

 cn: Enrollment-Providers
 ldapDisplayName: enrollmentProviders
 attributeId: 1.2.840.113556.1.4.825
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a39c5b3-8960-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.223 Attribute entryTTL

This operational attribute is maintained by the server and appears to be present in every dynamic
entry. The attribute is not present when the entry does not contain the dynamicObject object class.
The value of this attribute is the time in seconds that the entry will continue to exist before
disappearing from the directory.

In the absence of intervening "refresh" operations, the values returned by reading the attribute in two
successive searches are guaranteed to be nonincreasing. The smallest permissible value is 0,
indicating that the entry can disappear without warning. The attribute is marked NO-USER-
MODIFICATION because it can be changed only by using the refresh operation.

85 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Entry-TTL
 ldapDisplayName: entryTTL
 attributeId: 1.3.6.1.4.1.1466.101.119.3
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: d213decc-d81a-4384-aac2-dcfcfd631cf8
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 31557600
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.224 Attribute extendedAttributeInfo

This attribute specifies a multivalued property containing strings that represent additional information
for each attribute.

 cn: Extended-Attribute-Info
 ldapDisplayName: extendedAttributeInfo
 attributeId: 1.2.840.113556.1.4.909
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad947-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.225 Attribute extendedCharsAllowed

This attribute specifies whether extended characters are allowed in the value of this attribute. Applies
only to IA5, numeric, printable, and teletex string attributes.

 cn: Extended-Chars-Allowed
 ldapDisplayName: extendedCharsAllowed
 attributeId: 1.2.840.113556.1.2.380
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf967966-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 mapiID: 32935
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, the following attributes are defined differently.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

86 / 140


 systemOnly: TRUE

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.226 Attribute extendedClassInfo

This attribute specifies a multivalued property containing strings that represent additional information
for each class. Each value contains the governsID, lDAPDisplayName, and schemaIDGUID of the class.

 cn: Extended-Class-Info
 ldapDisplayName: extendedClassInfo
 attributeId: 1.2.840.113556.1.4.908
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad948-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.227 Attribute extensionName

This attribute specifies the name of a property page used to extend the UI of a directory object.

 cn: Extension-Name
 ldapDisplayName: extensionName
 attributeId: 1.2.840.113556.1.2.227
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf967972-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 255
 mapiID: 32937
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.228 Attribute extraColumns

This is a multivalued attribute whose value(s) consist of a 5 tuple: (attribute name), (column title),
(default visibility (0,1)), (column width (-1 for auto width)), and 0 (reserved for future use). This
value is used by the Active Directory Users and Computers console.

 cn: Extra-Columns
 ldapDisplayName: extraColumns
 attributeId: 1.2.840.113556.1.4.1687
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

87 / 140


 schemaIdGuid: d24e2846-1dd9-4bcf-99d7-a6227cc86da7
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.229 Attribute facsimileTelephoneNumber

Contains the telephone number of the user's business fax machine.

 cn: Facsimile-Telephone-Number
 ldapDisplayName: facsimileTelephoneNumber
 attributeId: 2.5.4.23
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967974-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 14883
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.230 Attribute fileExtPriority

This attribute specifies a list of file extensions in an application package and their associated priorities.

 cn: File-Ext-Priority
 ldapDisplayName: fileExtPriority
 attributeId: 1.2.840.113556.1.4.816
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: d9e18315-8939-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.231 Attribute flags

To be used by the object to store bit information.

 cn: Flags
 ldapDisplayName: flags
 attributeId: 1.2.840.113556.1.4.38
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf967976-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

88 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.232 Attribute flatName

For Windows NT domains, the flat name is the NetBIOS name. For links with non–Windows NT
domains, the flat name is the identifying name of that domain or it is NULL.

 cn: Flat-Name
 ldapDisplayName: flatName
 attributeId: 1.2.840.113556.1.4.511
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: b7b13117-b82e-11d0-afee-0000f80367c1
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.233 Attribute forceLogoff

This attribute is used in computing the kickoff time. Logoff time minus Force Log Off equals kickoff
time.

 cn: Force-Logoff
 ldapDisplayName: forceLogoff
 attributeId: 1.2.840.113556.1.4.39
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967977-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: b8119fd0-04f6-4762-ab7a-4986c76b3f9a
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, attributeSecurityGuid is not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.234 Attribute foreignIdentifier

This attribute specifies the security properties used by a foreign system.

 cn: Foreign-Identifier
 ldapDisplayName: foreignIdentifier
 attributeId: 1.2.840.113556.1.4.356
 attributeSyntax: 2.5.5.10
 omSyntax: 4

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

89 / 140


 isSingleValued: TRUE
 schemaIdGuid: 3e97891e-8c01-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.235 Attribute friendlyNames

This attribute specifies a list of default friendly name definitions supported by a catalog.

 cn: Friendly-Names
 ldapDisplayName: friendlyNames
 attributeId: 1.2.840.113556.1.4.682
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7bfdcb88-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.236 Attribute fromEntry

This is a constructed attribute that is TRUE if the object is writable, and FALSE if it is read-only (for
example, a global catalog replica instance).

 cn: From-Entry
 ldapDisplayName: fromEntry
 attributeId: 1.2.840.113556.1.4.910
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: FALSE
 schemaIdGuid: 9a7ad949-ca53-11d1-bbd0-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_IS_CONSTRUCTED |
  FLAG_DOMAIN_DISALLOW_RENAME
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.237 Attribute fromServer

This attribute specifies the distinguished name of the replication source server.

 cn: From-Server
 ldapDisplayName: fromServer
 attributeId: 1.2.840.113556.1.4.40
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

90 / 140


 schemaIdGuid: bf967979-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.238 Attribute frsComputerReference

This File Replication service (FRS) attribute contains a reference to a replica set member's computer
object.

 cn: Frs-Computer-Reference
 ldapDisplayName: frsComputerReference
 attributeId: 1.2.840.113556.1.4.869
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 2a132578-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 linkID: 102
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.239 Attribute frsComputerReferenceBL

This FRS attribute is a back link attribute of Attribute frsComputerReference and contains a reference
to replica sets to which this computer belongs.

 cn: Frs-Computer-Reference-BL
 ldapDisplayName: frsComputerReferenceBL
 attributeId: 1.2.840.113556.1.4.870
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 2a132579-9373-11d1-aebc-0000f80367c1
 systemOnly: TRUE
 searchFlags: 0
 linkID: 103
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.240 Attribute fRSControlDataCreation

This FRS attribute contains a Warning/Error level pair for file data creation (megabyte (MB) per
second).

 cn: FRS-Control-Data-Creation

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

91 / 140


 ldapDisplayName: fRSControlDataCreation
 attributeId: 1.2.840.113556.1.4.871
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a13257a-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.241 Attribute fRSControlInboundBacklog

This FRS attribute contains a Warning/Error level pair for inbound backlog (number of files).

 cn: FRS-Control-Inbound-Backlog
 ldapDisplayName: fRSControlInboundBacklog
 attributeId: 1.2.840.113556.1.4.872
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a13257b-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.242 Attribute fRSControlOutboundBacklog

This FRS attribute contains a Warning/Error level pair for outbound backlog (number of files).

 cn: FRS-Control-Outbound-Backlog
 ldapDisplayName: fRSControlOutboundBacklog
 attributeId: 1.2.840.113556.1.4.873
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a13257c-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.243 Attribute fRSDirectoryFilter

This FRS attribute contains a list of directories excluded from file replication (for example, the "temp"
directory or the "obj" directory).

 cn: FRS-Directory-Filter

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

92 / 140


 ldapDisplayName: fRSDirectoryFilter
 attributeId: 1.2.840.113556.1.4.484
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1be8f171-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.244 Attribute fRSDSPoll

This FRS attribute contains the DS polling interval for the file replication engine.

 cn: FRS-DS-Poll
 ldapDisplayName: fRSDSPoll
 attributeId: 1.2.840.113556.1.4.490
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1be8f177-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.245 Attribute fRSExtensions

This FRS attribute contains binary data used by file replication.

 cn: FRS-Extensions
 ldapDisplayName: fRSExtensions
 attributeId: 1.2.840.113556.1.4.536
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 52458020-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 65536
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.246 Attribute fRSFaultCondition

This FRS attribute contains the fault condition for a member.

 cn: FRS-Fault-Condition
 ldapDisplayName: fRSFaultCondition
 attributeId: 1.2.840.113556.1.4.491
 attributeSyntax: 2.5.5.12
 omSyntax: 64

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

93 / 140


 isSingleValued: TRUE
 schemaIdGuid: 1be8f178-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.247 Attribute fRSFileFilter

This FRS attribute contains the list of file extensions excluded from file replication.

 cn: FRS-File-Filter
 ldapDisplayName: fRSFileFilter
 attributeId: 1.2.840.113556.1.4.483
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1be8f170-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.248 Attribute fRSFlags

This FRS attribute contains the FRS option flags.

 cn: FRS-Flags
 ldapDisplayName: fRSFlags
 attributeId: 1.2.840.113556.1.4.874
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 2a13257d-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.249 Attribute fRSLevelLimit

This FRS attribute contains the limit depth of the directory tree to replicate for file replication.

 cn: FRS-Level-Limit
 ldapDisplayName: fRSLevelLimit
 attributeId: 1.2.840.113556.1.4.534
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 5245801e-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

94 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.250 Attribute fRSMemberReference

This FRS attribute contains a reference to the member object for this subscriber.

 cn: FRS-Member-Reference
 ldapDisplayName: fRSMemberReference
 attributeId: 1.2.840.113556.1.4.875
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 2a13257e-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 linkID: 104
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.251 Attribute fRSMemberReferenceBL

This FRS attribute is the back link attribute of Attribute fRSMemberReference and contains a reference
to subscriber objects for this member.

 cn: FRS-Member-Reference-BL
 ldapDisplayName: fRSMemberReferenceBL
 attributeId: 1.2.840.113556.1.4.876
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 2a13257f-9373-11d1-aebc-0000f80367c1
 systemOnly: TRUE
 searchFlags: 0
 linkID: 105
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.252 Attribute fRSPartnerAuthLevel

This FRS attribute contains the remote procedure call (RPC) security level.

 cn: FRS-Partner-Auth-Level
 ldapDisplayName: fRSPartnerAuthLevel
 attributeId: 1.2.840.113556.1.4.877
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 2a132580-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

95 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.253 Attribute fRSPrimaryMember

This FRS attribute contains a reference to the primary member of a replica set.

 cn: FRS-Primary-Member
 ldapDisplayName: fRSPrimaryMember
 attributeId: 1.2.840.113556.1.4.878
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 2a132581-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 linkID: 106
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.254 Attribute fRSReplicaSetGUID

This FRS attribute contains a GUID that identifies an FRS replica set.

 cn: FRS-Replica-Set-GUID
 ldapDisplayName: fRSReplicaSetGUID
 attributeId: 1.2.840.113556.1.4.533
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5245801a-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.255 Attribute fRSReplicaSetType

This FRS attribute contains a code that indicates whether this is a system volume (SYSVOL) replica
set, a distributed file system (DFS) replica set, or other replica set.

 cn: FRS-Replica-Set-Type
 ldapDisplayName: fRSReplicaSetType
 attributeId: 1.2.840.113556.1.4.31
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 26d9736b-6070-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

96 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.256 Attribute fRSRootPath

This FRS attribute contains a path to the root of the replicated file system tree.

 cn: FRS-Root-Path
 ldapDisplayName: fRSRootPath
 attributeId: 1.2.840.113556.1.4.487
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1be8f174-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.257 Attribute fRSRootSecurity

This FRS attribute contains a security descriptor of the replica set root for file replication.

 cn: FRS-Root-Security
 ldapDisplayName: fRSRootSecurity
 attributeId: 1.2.840.113556.1.4.535
 attributeSyntax: 2.5.5.15
 omSyntax: 66
 isSingleValued: TRUE
 schemaIdGuid: 5245801f-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 65535
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.258 Attribute fRSServiceCommand

This FRS attribute contains a Unicode string that an administrator can set to pass a command to every
replica set member.

 cn: FRS-Service-Command
 ldapDisplayName: fRSServiceCommand
 attributeId: 1.2.840.113556.1.4.500
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: ddac0cee-af8f-11d0-afeb-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 512
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

97 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.259 Attribute fRSServiceCommandStatus

This FRS attribute contains the response from the last command issued to a member.

 cn: FRS-Service-Command-Status
 ldapDisplayName: fRSServiceCommandStatus
 attributeId: 1.2.840.113556.1.4.879
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a132582-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 512
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.260 Attribute fRSStagingPath

This FRS attribute contains a path to the file replication staging area.

 cn: FRS-Staging-Path
 ldapDisplayName: fRSStagingPath
 attributeId: 1.2.840.113556.1.4.488
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1be8f175-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.261 Attribute fRSTimeLastCommand

This FRS attribute contains the time in which the last command was executed.

 cn: FRS-Time-Last-Command
 ldapDisplayName: fRSTimeLastCommand
 attributeId: 1.2.840.113556.1.4.880
 attributeSyntax: 2.5.5.11
 omSyntax: 23
 isSingleValued: TRUE
 schemaIdGuid: 2a132583-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.262 Attribute fRSTimeLastConfigChange

This FRS attribute contains the time in which the last configuration change was accepted.

98 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: FRS-Time-Last-Config-Change
 ldapDisplayName: fRSTimeLastConfigChange
 attributeId: 1.2.840.113556.1.4.881
 attributeSyntax: 2.5.5.11
 omSyntax: 23
 isSingleValued: TRUE
 schemaIdGuid: 2a132584-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.263 Attribute fRSUpdateTimeout

This FRS attribute contains the maximum time, in minutes, to wait to complete an update before
giving up.

 cn: FRS-Update-Timeout
 ldapDisplayName: fRSUpdateTimeout
 attributeId: 1.2.840.113556.1.4.485
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 1be8f172-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.264 Attribute fRSVersion

This FRS attribute contains the version number and build date.

 cn: FRS-Version
 ldapDisplayName: fRSVersion
 attributeId: 1.2.840.113556.1.4.882
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 2a132585-9373-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 32
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.265 Attribute fRSVersionGUID

If this FRS attribute is present, changing its value indicates that a configuration change has been
made on this replica set.

 cn: FRS-Version-GUID
 ldapDisplayName: fRSVersionGUID
 attributeId: 1.2.840.113556.1.4.43

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

99 / 140


 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 26d9736c-6070-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.266 Attribute fRSWorkingPath

This FRS attribute contains the path to the file replication database.

 cn: FRS-Working-Path
 ldapDisplayName: fRSWorkingPath
 attributeId: 1.2.840.113556.1.4.486
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 1be8f173-a9ff-11d0-afe2-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.267 Attribute fSMORoleOwner

The fSMORoleOwner attribute stores the distinguished name of a DSA object as described in [MS-
ADTS] section 3.1.1.1.11 (FSMO Roles).

 cn: FSMO-Role-Owner
 ldapDisplayName: fSMORoleOwner
 attributeId: 1.2.840.113556.1.4.369
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 66171887-8f3c-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.268 Attribute garbageCollPeriod

This attribute is located on the CN=Directory Service,CN=Windows
NT,CN=Services,CN=Configuration,... object. It represents the period, in hours, between DS garbage
collection runs.

100 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Garbage-Coll-Period
 ldapDisplayName: garbageCollPeriod
 attributeId: 1.2.840.113556.1.2.301
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 5fd424a1-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 mapiID: 32943
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.269 Attribute gecos

The GECOS field (the common name, as specified in [RFC2307] section 3).

 cn: Gecos
 ldapDisplayName: gecos
 attributeId: 1.3.6.1.1.1.1.2
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: TRUE
 schemaIdGuid: a3e03f1f-1d55-4253-a0af-30c2a784e46e
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 10240

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.270 Attribute generatedConnection

Set to TRUE if this connection was created by autotopology generation.

 cn: Generated-Connection
 ldapDisplayName: generatedConnection
 attributeId: 1.2.840.113556.1.4.41
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf96797a-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.271 Attribute generationQualifier

Indicates a person's generation; for example, junior (Jr.) or II.

 cn: Generation-Qualifier
 ldapDisplayName: generationQualifier
 attributeId: 2.5.4.44

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

101 / 140


 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 16775804-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 64
 mapiID: 35923
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.272 Attribute gidNumber

An integer uniquely identifying a group in an administrative domain, as specified in [RFC2307].

 cn: GidNumber
 ldapDisplayName: gidNumber
 attributeId: 1.3.6.1.1.1.1.1
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: c5b95f0c-ec9e-41c4-849c-b46597ed6696
 systemOnly: FALSE
 searchFlags: fATTINDEX

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.273 Attribute givenName

Contains the given name (first name) of the user.

 cn: Given-Name
 ldapDisplayName: givenName
 attributeId: 2.5.4.42
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ff8e-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fANR | fATTINDEX
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14854
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.274 Attribute globalAddressList

This attribute is used on an Exchange Server container to store the distinguished name of a newly
created global address list (GAL). Once this attribute has at least one entry, the implementer can
enable MAPI clients to use a GAL.

102 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


 cn: Global-Address-List
 ldapDisplayName: globalAddressList
 attributeId: 1.2.840.113556.1.4.1245
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: f754c748-06f4-11d2-aa53-00c04fd7d83a
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.275 Attribute globalAddressList2

This attribute is used on an Exchange Server container to store the distinguished name of a newly
created GAL. Once this attribute has at least one entry, the implementer can enable MAPI clients to
use a GAL. Similar to globalAddressList, it differs by being a linked attribute.

 cn: Global-Address-List2
 ldapDisplayName: globalAddressList2
 attributeId: 1.2.840.113556.1.4.2047
 attributeSyntax: 2.5.5.1
 linkID: 2124
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 4898f63d-4112-477c-8826-3ca00bd8277d
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows Server 2008.

### 2.276 Attribute governsID

This attribute specifies the unique object ID of the class defined by this class-schema object.

 cn: Governs-ID
 ldapDisplayName: governsID
 attributeId: 1.2.840.113556.1.2.22
 attributeSyntax: 2.5.5.2
 omSyntax: 6
 isSingleValued: TRUE
 schemaIdGuid: bf96797d-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

103 / 140


### 2.277 Attribute gPCFileSysPath

This attribute specifies the Universal Naming Convention (UNC) path to the Group Policy Object
template located in the system volume (SYSVOL).

 cn: GPC-File-Sys-Path
 ldapDisplayName: gPCFileSysPath
 attributeId: 1.2.840.113556.1.4.894
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f30e3bc1-9ff0-11d1-b603-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.278 Attribute gPCFunctionalityVersion

This attribute specifies the version of the Group Policy Object Editor that created this object.

 cn: GPC-Functionality-Version
 ldapDisplayName: gPCFunctionalityVersion
 attributeId: 1.2.840.113556.1.4.893
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: f30e3bc0-9ff0-11d1-b603-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.279 Attribute gPCMachineExtensionNames

This attribute is used by the Group Policy Object (GPO) for machine policies.

 cn: GPC-Machine-Extension-Names
 ldapDisplayName: gPCMachineExtensionNames
 attributeId: 1.2.840.113556.1.4.1348
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 32ff8ecc-783f-11d2-9916-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.280 Attribute gPCUserExtensionNames

This attribute is used by the GPO for user policies.

 cn: GPC-User-Extension-Names
 ldapDisplayName: gPCUserExtensionNames

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

104 / 140


 attributeId: 1.2.840.113556.1.4.1349
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 42a75fc6-783f-11d2-9916-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.281 Attribute gPCWQLFilter

This attribute is used to store a string that contains a GUID for the filter and a Windows Management
Instrumentation (WMI) namespace path.

 cn: GPC-WQL-Filter
 ldapDisplayName: gPCWQLFilter
 attributeId: 1.2.840.113556.1.4.1694
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 7bd4c7a6-1add-4436-8c04-3999a880154c
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.282 Attribute gPLink

This attribute specifies a value used by the Group Policy Core Protocol. See [MS-GPOL] section 2.2.2.

 cn: GP-Link
 ldapDisplayName: gPLink
 attributeId: 1.2.840.113556.1.4.891
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f30e3bbe-9ff0-11d1-b603-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.283 Attribute gPOptions

Options that affect all Group Policy associated with the object hosting this property.

 cn: GP-Options
 ldapDisplayName: gPOptions
 attributeId: 1.2.840.113556.1.4.892
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: f30e3bbf-9ff0-11d1-b603-0000f80367c1

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

105 / 140


 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.284 Attribute groupAttributes

 cn: Group-Attributes
 ldapDisplayName: groupAttributes
 attributeId: 1.2.840.113556.1.4.152
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf96797e-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.285 Attribute groupMembershipSAM

Windows NT security. Down-level Windows NT support.

 cn: Group-Membership-SAM
 ldapDisplayName: groupMembershipSAM
 attributeId: 1.2.840.113556.1.4.166
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf967980-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.286 Attribute groupPriority

 cn: Group-Priority
 ldapDisplayName: groupPriority
 attributeId: 1.2.840.113556.1.4.345
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: eea65905-8ac6-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.287 Attribute groupsToIgnore

 cn: Groups-to-Ignore
 ldapDisplayName: groupsToIgnore
 attributeId: 1.2.840.113556.1.4.344

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

106 / 140


 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: eea65904-8ac6-11d0-afda-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.288 Attribute groupType

This attribute contains a set of flags that define the type and scope of a group object. For more
information about the possible values for this attribute, see the Remarks section of [MSDN-
GroupType].

 cn: Group-Type
 ldapDisplayName: groupType
 attributeId: 1.2.840.113556.1.4.750
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 9a9a021e-4a5b-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: fPRESERVEONDELETE | fATTINDEX
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.289 Attribute hasMasterNCs

This attribute contains the distinguished names of naming contexts. Forward link for the Mastered-By
attribute.

 cn: Has-Master-NCs
 ldapDisplayName: hasMasterNCs
 attributeId: 1.2.840.113556.1.2.14
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: bf967982-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 linkID: 76
 mapiID: 32950
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

107 / 140


### 2.290 Attribute hasPartialReplicaNCs

Sibling to Has-Master-NCs. The Has-Partial-Replica-NCs attribute reflects the distinguished name for
all other-domain NCs that have been replicated into a global catalog (GC).

 cn: Has-Partial-Replica-NCs
 ldapDisplayName: hasPartialReplicaNCs
 attributeId: 1.2.840.113556.1.2.15
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: bf967981-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 linkID: 74
 mapiID: 32949
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.291 Attribute helpData16

This attribute was used for the Win16 Help file format for Exchange Server 4.0. It is not used for any
other versions of Exchange Server.

 cn: Help-Data16
 ldapDisplayName: helpData16
 attributeId: 1.2.840.113556.1.2.402
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5fd424a7-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32768
 mapiID: 32826
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.292 Attribute helpData32

This attribute was used for the Win32 Help file format for Exchange Server 4.0. It is not used for any
other versions of Exchange Server.

 cn: Help-Data32
 ldapDisplayName: helpData32
 attributeId: 1.2.840.113556.1.2.9
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 5fd424a8-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

108 / 140


 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32768
 mapiID: 32784
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.293 Attribute helpFileName

This attribute was used for Exchange Server 4.0. It contained the name that is used for the file when
the provider downloaded Help data to a client computer. It is not used for any other versions of
Exchange Server.

 cn: Help-File-Name
 ldapDisplayName: helpFileName
 attributeId: 1.2.840.113556.1.2.327
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 5fd424a9-1262-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 13
 mapiID: 32827
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.294 Attribute hideFromAB

This is a defunct attribute and is not to be used.

 cn: Hide-From-AB
 ldapDisplayName: hideFromAB
 attributeId: 1.2.840.113556.1.4.1780
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: ec05b750-a977-4efe-8e8d-ba6c1a6e33a8
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.295 Attribute homeDirectory

This attribute specifies the home directory for the account. This value can be a null string.

 cn: Home-Directory
 ldapDisplayName: homeDirectory

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

109 / 140


 attributeId: 1.2.840.113556.1.4.44
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967985-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.296 Attribute homeDrive

This attribute specifies the drive letter to which to map the UNC path specified by homeDirectory. The
drive letter is specified in the form "<DriveLetter>:" where <DriveLetter> is the letter of the drive to
map. The <DriveLetter> is a single, uppercase letter and the colon (:) is required.

 cn: Home-Drive
 ldapDisplayName: homeDrive
 attributeId: 1.2.840.113556.1.4.45
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf967986-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.297 Attribute homePhone

The user's main home telephone number.

 cn: Phone-Home-Primary
 ldapDisplayName: homePhone
 attributeId: 0.9.2342.19200300.100.1.20
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ffa1-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 64
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 14857
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

110 / 140


In Windows 2000 Server, the following attributes are defined differently.

 systemFlags: 0

### 2.298 Attribute homePostalAddress

This attribute specifies a user's home address.

 cn: Address-Home
 ldapDisplayName: homePostalAddress
 attributeId: 1.2.840.113556.1.2.617
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 16775781-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 4096
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 14941

Version-Specific Behavior: First implemented on Windows 2000 Server.

On Windows 2000 Server, the following attribute is defined differently.

 systemFlags: FLAG_SCHEMA_BASE_OBJECT

### 2.299 Attribute host

This attribute type specifies a host computer.

 cn: host
 ldapDisplayName: host
 attributeId: 0.9.2342.19200300.100.1.9
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 6043df71-fa48-46cf-ab7c-cbd54644b22d
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 256

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.300 Attribute houseIdentifier

The houseIdentifier attribute specifies a linguistic construct used to identify a particular building; for
example, a house number or house name relative to a street, avenue, town, or city.

 cn: houseIdentifier
 ldapDisplayName: houseIdentifier
 attributeId: 2.5.4.51
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

111 / 140


 schemaIdGuid: a45398b7-c44a-4eb6-82d3-13c10946dbfe
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 32768

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.301 Attribute iconPath

This attribute specifies the source for loading an icon.

 cn: Icon-Path
 ldapDisplayName: iconPath
 attributeId: 1.2.840.113556.1.4.219
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: f0f8ff83-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 2048
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.302 Attribute implementedCategories

This attribute specifies a list of component category IDs that this object implements.

 cn: Implemented-Categories
 ldapDisplayName: implementedCategories
 attributeId: 1.2.840.113556.1.4.320
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 7d6c0e92-7e20-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 16
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.303 Attribute indexedScopes

This attribute specifies the list of indexed directory scopes (for example, C:\ or D:\).

 cn: IndexedScopes
 ldapDisplayName: indexedScopes
 attributeId: 1.2.840.113556.1.4.681
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7bfdcb87-4807-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

112 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.304 Attribute info

This attribute specifies the user's comments. This string can be a null string.

 cn: Comment
 ldapDisplayName: info
 attributeId: 1.2.840.113556.1.2.81
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96793e-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 1024
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 12292
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.305 Attribute initialAuthIncoming

This attribute is not necessary for Active Directory functioning, and this protocol does not define a
format beyond that required by the schema.

 cn: Initial-Auth-Incoming
 ldapDisplayName: initialAuthIncoming
 attributeId: 1.2.840.113556.1.4.539
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 52458023-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.306 Attribute initialAuthOutgoing

This attribute is used to contain information about an initial outgoing authentication sent by the
authentication server for this domain to the client that requested authentication. The server that uses
this attribute receives the authorization from the authentication server and sends it to the client.

 cn: Initial-Auth-Outgoing
 ldapDisplayName: initialAuthOutgoing
 attributeId: 1.2.840.113556.1.4.540
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

113 / 140


 schemaIdGuid: 52458024-ca6a-11d0-afff-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.307 Attribute initials

This attribute contains the initials for parts of the user's full name. This can be used as the middle
initial in the Windows address book.

 cn: Initials
 ldapDisplayName: initials
 attributeId: 2.5.4.43
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: f0f8ff90-1191-11d0-a060-00aa006c33ed
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 6
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 mapiID: 14858
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.308 Attribute installUiLevel

This attribute specifies information for the type (level) of installation that is used for the user
interface. Possible installation levels are as follows: 2 INSTALLUILEVEL_NONE (silent installation), 3
INSTALLUILEVEL_BASIC (simple installation with error handling), 4 INSTALLUILEVEL_REDUCED
(authored UI, wizard dialogs suppressed), and 5 INSTALLUILEVEL_FULL (authored UI with wizards,
progress, and errors).

 cn: Install-Ui-Level
 ldapDisplayName: installUiLevel
 attributeId: 1.2.840.113556.1.4.847
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: 96a7dd64-9118-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.309 Attribute instanceType

A bit field that dictates how the object is instantiated on a particular server. The value of this attribute
can differ on different replicas even if the replicas are in sync. This attribute can be zero or a
combination of one or more of the following bit flags.

114 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


Bit flag

Meaning

0x00000001  The head of naming context.

0x00000002  This replica is not instantiated.

0x00000004  The object is writable on this directory.

0x00000008  The naming context above this one on this directory is held.

0x00000010  The naming context is being constructed for the first time via replication.

0x00000020  The naming context is being removed from the local directory system agent (DSA).

 cn: Instance-Type
 ldapDisplayName: instanceType
 attributeId: 1.2.840.113556.1.2.1
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf96798c-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 mapiID: 32957
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.310 Attribute internationalISDNNumber

This attribute specifies an international ISDN number associated with an object.

 cn: International-ISDN-Number
 ldapDisplayName: internationalISDNNumber
 attributeId: 2.5.4.25
 attributeSyntax: 2.5.5.6
 omSyntax: 18
 isSingleValued: FALSE
 schemaIdGuid: bf96798d-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 1
 rangeUpper: 16
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 32958
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.311 Attribute interSiteTopologyFailover

This attribute indicates how much time can transpire since the last keep-alive message for the
intersite topology generator to be considered dead.

 cn: Inter-Site-Topology-Failover
 ldapDisplayName: interSiteTopologyFailover

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

115 / 140


 attributeId: 1.2.840.113556.1.4.1248
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: b7c69e60-2cc7-11d2-854e-00a0c983f608
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

Note: This attribute is used for the server-to-server replication implementation only; the meaning is
not significant to Windows clients.

### 2.312 Attribute interSiteTopologyGenerator

This attribute specifies support failover for the machine designated as the one that runs Knowledge
Consistency Checker (KCC) intersite topology generation in a given site.

 cn: Inter-Site-Topology-Generator
 ldapDisplayName: interSiteTopologyGenerator
 attributeId: 1.2.840.113556.1.4.1246
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: b7c69e5e-2cc7-11d2-854e-00a0c983f608
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

Note: This attribute is used for the server-to-server replication implementation only; the meaning is
not significant to Windows clients.

### 2.313 Attribute interSiteTopologyRenew

This attribute indicates how often the intersite topology generator updates the keep-alive message
that is sent to domain controllers that are contained in the same site.

 cn: Inter-Site-Topology-Renew
 ldapDisplayName: interSiteTopologyRenew
 attributeId: 1.2.840.113556.1.4.1247
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: b7c69e5f-2cc7-11d2-854e-00a0c983f608
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

116 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

Note: This attribute is used for the server-to-server replication implementation only; the meaning is
not significant to Windows clients.

### 2.314 Attribute invocationId

This attribute is used to uniquely identify the specific version of the directory database associated with
a domain controller.

 cn: Invocation-Id
 ldapDisplayName: invocationId
 attributeId: 1.2.840.113556.1.2.115
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf96798e-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: fATTINDEX
 mapiID: 32959
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, the following attributes are defined differently.

 searchFlags: 0

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.315 Attribute ipHostNumber

This attribute specifies the IP address as a dotted decimal, omitting leading zeros.

 cn: IpHostNumber
 ldapDisplayName: ipHostNumber
 attributeId: 1.3.6.1.1.1.1.19
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: FALSE
 schemaIdGuid: de8bb721-85dc-4fde-b687-9657688e667e
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 128

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.316 Attribute ipNetmaskNumber

This attribute specifies the IP netmask as a dotted decimal, omitting leading zeros.

 cn: IpNetmaskNumber
 ldapDisplayName: ipNetmaskNumber
 attributeId: 1.3.6.1.1.1.1.21

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

117 / 140


 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: TRUE
 schemaIdGuid: 6ff64fcd-462e-4f62-b44a-9a5347659eb9
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 128

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.317 Attribute ipNetworkNumber

This attribute specifies the IP network as a dotted decimal, omitting leading zeros.

 cn: IpNetworkNumber
 ldapDisplayName: ipNetworkNumber
 attributeId: 1.3.6.1.1.1.1.20
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: TRUE
 schemaIdGuid: 4e3854f4-3087-42a4-a813-bb0c528958d3
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 128

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.318 Attribute ipPhone

This attribute specifies the TCP/IP address for the telephone. It is used by telephony.

 cn: Phone-Ip-Primary
 ldapDisplayName: ipPhone
 attributeId: 1.2.840.113556.1.4.721
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 4d146e4a-48d4-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 64
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

### 2.319 Attribute ipProtocolNumber

This attribute is part of the protocols map and stores the unique number that identifies the protocol.

 cn: IpProtocolNumber
 ldapDisplayName: ipProtocolNumber
 attributeId: 1.3.6.1.1.1.1.17
 attributeSyntax: 2.5.5.9
 omSyntax: 2

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

118 / 140


 isSingleValued: TRUE
 schemaIdGuid: ebf5c6eb-0e2d-4415-9670-1081993b4211
 systemOnly: FALSE
 searchFlags: 0

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.320 Attribute ipsecData

The Ipsec-Data attribute is for internal use only.

 cn: Ipsec-Data
 ldapDisplayName: ipsecData
 attributeId: 1.2.840.113556.1.4.623
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: b40ff81f-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.321 Attribute ipsecDataType

The Ipsec-Data-Type attribute is for internal use only.

 cn: Ipsec-Data-Type
 ldapDisplayName: ipsecDataType
 attributeId: 1.2.840.113556.1.4.622
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: b40ff81e-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.322 Attribute ipsecFilterReference

The Ipsec-Filter-Reference attribute.

 cn: Ipsec-Filter-Reference
 ldapDisplayName: ipsecFilterReference
 attributeId: 1.2.840.113556.1.4.629
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: b40ff823-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

119 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.323 Attribute ipsecID

The Ipsec-ID attribute.

 cn: Ipsec-ID
 ldapDisplayName: ipsecID
 attributeId: 1.2.840.113556.1.4.621
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: b40ff81d-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.324 Attribute ipsecISAKMPReference

The Ipsec-ISAKMP-Reference attribute (see [MS-GPIPSEC] section 2.2.1.1.1).

 cn: Ipsec-ISAKMP-Reference
 ldapDisplayName: ipsecISAKMPReference
 attributeId: 1.2.840.113556.1.4.626
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: b40ff820-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.325 Attribute ipsecName

The Ipsec-Name attribute.

 cn: Ipsec-Name
 ldapDisplayName: ipsecName
 attributeId: 1.2.840.113556.1.4.620
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: b40ff81c-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.326 Attribute iPSECNegotiationPolicyAction

The IPSEC-Negotiation-Policy-Action attribute.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

120 / 140


 cn: IPSEC-Negotiation-Policy-Action
 ldapDisplayName: iPSECNegotiationPolicyAction
 attributeId: 1.2.840.113556.1.4.888
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 07383075-91df-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.327 Attribute ipsecNegotiationPolicyReference

The Ipsec-Negotiation-Policy-Reference attribute.

 cn: Ipsec-Negotiation-Policy-Reference
 ldapDisplayName: ipsecNegotiationPolicyReference
 attributeId: 1.2.840.113556.1.4.628
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: b40ff822-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.328 Attribute iPSECNegotiationPolicyType

The IPSEC-Negotiation-Policy-Type attribute.

 cn: IPSEC-Negotiation-Policy-Type
 ldapDisplayName: iPSECNegotiationPolicyType
 attributeId: 1.2.840.113556.1.4.887
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 07383074-91df-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.329 Attribute ipsecNFAReference

The Ipsec-NFA-Reference attribute (see [MS-GPIPSEC] section 2.2.1.1.1).

 cn: Ipsec-NFA-Reference
 ldapDisplayName: ipsecNFAReference
 attributeId: 1.2.840.113556.1.4.627
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

121 / 140


 schemaIdGuid: b40ff821-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.330 Attribute ipsecOwnersReference

The Ipsec-Owners-Reference attribute.

 cn: Ipsec-Owners-Reference
 ldapDisplayName: ipsecOwnersReference
 attributeId: 1.2.840.113556.1.4.624
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: b40ff824-427a-11d1-a9c2-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.331 Attribute ipsecPolicyReference

The Ipsec-Policy-Reference attribute.

 cn: Ipsec-Policy-Reference
 ldapDisplayName: ipsecPolicyReference
 attributeId: 1.2.840.113556.1.4.517
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: b7b13118-b82e-11d0-afee-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.332 Attribute ipServicePort

This is a part of the services map and contains the port at which the UNIX service is available.

 cn: IpServicePort
 ldapDisplayName: ipServicePort
 attributeId: 1.3.6.1.1.1.1.15
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: ff2daebf-f463-495a-8405-3e483641eaa2
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

122 / 140


Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.333 Attribute ipServiceProtocol

This is a part of the services map and stores the protocol number for a UNIX service.

 cn: IpServiceProtocol
 ldapDisplayName: ipServiceProtocol
 attributeId: 1.3.6.1.1.1.1.16
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: FALSE
 schemaIdGuid: cd96ec0b-1ed6-43b4-b26b-f170b645883f
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 1024

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.334 Attribute isCriticalSystemObject

If TRUE, the object hosting this attribute is replicated during installation of a new replica.

 cn: Is-Critical-System-Object
 ldapDisplayName: isCriticalSystemObject
 attributeId: 1.2.840.113556.1.4.868
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 00fbf30d-91fe-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.335 Attribute isDefunct

If TRUE, the class or attribute is no longer usable. Old versions of this object can exist, but new ones
cannot be created.

 cn: Is-Defunct
 ldapDisplayName: isDefunct
 attributeId: 1.2.840.113556.1.4.661
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 28630ebe-41d5-11d1-a9c1-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

123 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.336 Attribute isDeleted

If TRUE, this object has been marked for deletion and will be removed from the Active Directory
system [MS-ADOD].

 cn: Is-Deleted
 ldapDisplayName: isDeleted
 attributeId: 1.2.840.113556.1.2.48
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf96798f-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 mapiID: 32960
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.337 Attribute isEphemeral

 cn: Is-Ephemeral
 ldapDisplayName: isEphemeral
 attributeId: 1.2.840.113556.1.4.1212
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: f4c453f0-c5f1-11d1-bbcb-0080c76670c0
 systemOnly: TRUE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.338 Attribute isMemberOfPartialAttributeSet

If TRUE, this attribute is replicated to the global catalog.

 cn: Is-Member-Of-Partial-Attribute-Set
 ldapDisplayName: isMemberOfPartialAttributeSet
 attributeId: 1.2.840.113556.1.4.639
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 19405b9d-3cfa-11d1-a9c0-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

124 / 140


### 2.339 Attribute isPrivilegeHolder

This attribute specifies a back link to privileges held by a given principal.

 cn: Is-Privilege-Holder
 ldapDisplayName: isPrivilegeHolder
 attributeId: 1.2.840.113556.1.4.638
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: FALSE
 schemaIdGuid: 19405b9c-3cfa-11d1-a9c0-0000f80367c1
 systemOnly: TRUE
 searchFlags: 0
 linkID: 71
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.340 Attribute isRecycled

If TRUE, this object has been marked for permanent deletion. Additionally, if the Recycle Bin optional
feature is enabled, the value TRUE marks an object that cannot be undeleted. It will be removed from
the Active Directory system [MS-ADOD].

 cn: Is-Recycled
 ldapDisplayName: isRecycled
 attributeId: 1.2.840.113556.1.4.2058
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: 8fb59256-55f1-444b-aacb-f5b482fe3459
 systemOnly: TRUE
 searchFlags: fPRESERVEONDELETE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 isMemberOfPartialAttributeSet: TRUE
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows Server 2008 R2 operating system.

### 2.341 Attribute isSingleValued

If TRUE, this attribute can only store one value.

 cn: Is-Single-Valued
 ldapDisplayName: isSingleValued
 attributeId: 1.2.840.113556.1.2.33
 attributeSyntax: 2.5.5.8
 omSyntax: 1
 isSingleValued: TRUE
 schemaIdGuid: bf967992-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 mapiID: 32961
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

125 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.342 Attribute jpegPhoto

Used to store one or more images of a person by using the JPEG File Interchange Format, as specified
in [JFIF].

 cn: jpegPhoto
 ldapDisplayName: jpegPhoto
 attributeId: 0.9.2342.19200300.100.1.60
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: bac80572-09c4-4fa9-9ae6-7628d7adbe0e
 systemOnly: FALSE
 searchFlags: 0
 showInAdvancedViewOnly: FALSE

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.343 Attribute keywords

This attribute specifies a list of keywords that can be used to locate a given connection point.

 cn: Keywords
 ldapDisplayName: keywords
 attributeId: 1.2.840.113556.1.4.48
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: bf967993-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 1
 rangeUpper: 256
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.344 Attribute knowledgeInformation

This attribute specifies a human-readable accumulated description of knowledge that is mastered by a
specific DSA.

 cn: Knowledge-Information
 ldapDisplayName: knowledgeInformation
 attributeId: 2.5.4.2
 attributeSyntax: 2.5.5.4
 omSyntax: 20
 isSingleValued: FALSE
 schemaIdGuid: 1677581f-47f3-11d1-a9c3-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 mapiID: 32963

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

126 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.345 Attribute l

This attribute represents the name of a locality, such as a town or city.

 cn: Locality-Name
 ldapDisplayName: l
 attributeId: 2.5.4.7
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf9679a2-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY | fATTINDEX
 rangeLower: 1
 rangeUpper: 128
 attributeSecurityGuid: 77b5b886-944a-11d1-aebd-0000f80367c1
 mapiID: 14887
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT |
  FLAG_ATTR_REQ_PARTIAL_SET_MEMBER
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.346 Attribute labeledURI

This attribute specifies a Uniform Resource Identifier (URI) followed by a label. The label is used to
describe the resource to which the URI points, and it is intended as a friendly name.

 cn: labeledURI
 ldapDisplayName: labeledURI
 attributeId: 1.3.6.1.4.1.250.1.57
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: c569bb46-c680-44bc-a273-e6c227d71b45
 systemOnly: FALSE
 searchFlags: 0
 showInAdvancedViewOnly: FALSE

Version-Specific Behavior: First implemented on Windows Server 2003.

### 2.347 Attribute lastBackupRestorationTime

This attribute specifies when the last system restore occurred.

 cn: Last-Backup-Restoration-Time
 ldapDisplayName: lastBackupRestorationTime
 attributeId: 1.2.840.113556.1.4.519
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 1fbb0be8-ba63-11d0-afef-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

127 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.348 Attribute lastContentIndexed

This attribute specifies the time this volume was last content-indexed.

 cn: Last-Content-Indexed
 ldapDisplayName: lastContentIndexed
 attributeId: 1.2.840.113556.1.4.50
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967995-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.349 Attribute lastKnownParent

This attribute specifies the distinguished name of the last known parent of an orphaned or deleted
object.

 cn: Last-Known-Parent
 ldapDisplayName: lastKnownParent
 attributeId: 1.2.840.113556.1.4.781
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 52ab8670-5709-11d1-a9c6-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.350 Attribute lastLogoff

This attribute specifies the last time the user logged off. This value is stored as a large integer that
represents the number of 100 nanosecond intervals since January 1, 1601 (UTC). A value of zero
means that the last logoff time is unknown.

 cn: Last-Logoff
 ldapDisplayName: lastLogoff
 attributeId: 1.2.840.113556.1.4.51
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967996-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

128 / 140


 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.351 Attribute lastLogon

This attribute specifies the last time the user logged on. This value is stored as a large integer that
represents the number of 100 nanosecond intervals since January 1, 1601 (UTC). A value of zero
means that the last logon time is unknown.

 cn: Last-Logon
 ldapDisplayName: lastLogon
 attributeId: 1.2.840.113556.1.4.52
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967997-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.352 Attribute lastLogonTimestamp

This is the time that the user last logged on to the domain. Whenever a user logs on, the value of this
attribute is read from the DC. If msDS-LogonTimeSyncInterval is nonzero, and the value is older than
(current_time - msDS-LogonTimeSyncInterval), the value is updated with the current time. The initial
update, after the domain functional level is raised to DS_BEHAVIOR_WIN2003 or higher, is calculated
as 14 days minus a random percentage of 5 days.

Note: This attribute is present on objects only when the domain functional level is
DS_BEHAVIOR_WIN2003 or higher.

 cn: Last-Logon-Timestamp
 ldapDisplayName: lastLogonTimestamp
 attributeId: 1.2.840.113556.1.4.1696
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: c0e20a04-0e5a-4ff3-9482-5efeaecd7060
 systemOnly: FALSE
 searchFlags: fATTINDEX
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows Server 2003.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

129 / 140


The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.353 Attribute lastSetTime

This attribute specifies the last time the secret was changed.

 cn: Last-Set-Time
 ldapDisplayName: lastSetTime
 attributeId: 1.2.840.113556.1.4.53
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf967998-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.354 Attribute lastUpdateSequence

This attribute specifies the update sequence number for the last item in the class store that was
changed.

 cn: Last-Update-Sequence
 ldapDisplayName: lastUpdateSequence
 attributeId: 1.2.840.113556.1.4.330
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 7d6c0e9c-7e20-11d0-afd6-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.355 Attribute lDAPAdminLimits

This attribute contains a set of attribute-value pairs defining LDAP server administrative limits.

 cn: LDAP-Admin-Limits
 ldapDisplayName: lDAPAdminLimits
 attributeId: 1.2.840.113556.1.4.843
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: 7359a352-90f7-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

130 / 140

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018


### 2.356 Attribute lDAPDisplayName

This attribute specifies the name used by LDAP clients, such as the ADSI LDAP provider, to read and
write the attribute by using the LDAP protocol.

 cn: LDAP-Display-Name
 ldapDisplayName: lDAPDisplayName
 attributeId: 1.2.840.113556.1.2.460
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: bf96799a-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fPRESERVEONDELETE | fATTINDEX
 rangeLower: 1
 rangeUpper: 256
 mapiID: 33137
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.357 Attribute lDAPIPDenyList

This attribute holds a list of binary IP addresses that are denied access to an LDAP server.

 cn: LDAP-IPDeny-List
 ldapDisplayName: lDAPIPDenyList
 attributeId: 1.2.840.113556.1.4.844
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: 7359a353-90f7-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.358 Attribute lSACreationTime

This attribute is used to support replication to Windows NT 4.0 domains.

 cn: LSA-Creation-Time
 ldapDisplayName: lSACreationTime
 attributeId: 1.2.840.113556.1.4.66
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf9679ad-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

131 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.359 Attribute lSAModifiedCount

This attribute is used to support replication to Windows NT 4.0 domains.

 cn: LSA-Modified-Count
 ldapDisplayName: lSAModifiedCount
 attributeId: 1.2.840.113556.1.4.67
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf9679ae-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.360 Attribute legacyExchangeDN

This attribute specifies the distinguished name previously used by Exchange Server.

 cn: Legacy-Exchange-DN
 ldapDisplayName: legacyExchangeDN
 attributeId: 1.2.840.113556.1.4.655
 attributeSyntax: 2.5.5.4
 omSyntax: 20
 isSingleValued: TRUE
 schemaIdGuid: 28630ebc-41d5-11d1-a9c1-0000f80367c1
 systemOnly: FALSE
 searchFlags: fPRESERVEONDELETE| fANR | fATTINDEX
 attributeSecurityGuid: e48d0154-bcf8-11d1-8702-00c04fb96050
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.361 Attribute linkID

This attribute specifies an integer that indicates that the attribute is a linked attribute. An even integer
is a forward link and an odd integer is a back link.

 cn: Link-ID
 ldapDisplayName: linkID
 attributeId: 1.2.840.113556.1.2.50
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf96799b-0de6-11d0-a285-00aa003049e2
 systemOnly: TRUE
 searchFlags: 0
 mapiID: 32965
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

132 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.362 Attribute linkTrackSecret

This attribute specifies a link to a secret key that allows an encrypted file to be translated into plain
text.

 cn: Link-Track-Secret
 ldapDisplayName: linkTrackSecret
 attributeId: 1.2.840.113556.1.4.269
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: 2ae80fe2-47b4-11d0-a1a4-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 rangeLower: 0
 rangeUpper: 16
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.363 Attribute lmPwdHistory

The password history of the user in LAN Manager (LM) one-way format (OWF). The LM OWF is used
for compatibility with LAN Manager 2.x clients, Windows 95 operating system, and Windows 98
operating system.

For more information about usage, refer to [MS-SAMR] sections 3.1.1.6 and 3.1.1.9.1.

 cn: Lm-Pwd-History
 ldapDisplayName: lmPwdHistory
 attributeId: 1.2.840.113556.1.4.160
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: FALSE
 schemaIdGuid: bf96799d-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.364 Attribute localeID

This attribute specifies a list of locale IDs supported by this application. A locale ID represents a
geographic location; for example, a country/region, a city, or a county.

 cn: Locale-ID
 ldapDisplayName: localeID
 attributeId: 1.2.840.113556.1.4.58
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: FALSE

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

133 / 140


 schemaIdGuid: bf9679a1-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.365 Attribute localizationDisplayId

This attribute is used to index the Extrts.mc file to get the localized displayName for the objects, for
UI purposes.

 cn: Localization-Display-Id
 ldapDisplayName: localizationDisplayId
 attributeId: 1.2.840.113556.1.4.1353
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: a746f0d1-78d0-11d2-9916-0000f87a57d4
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.366 Attribute localizedDescription

This attribute specifies the localization ID and display name for an object.

 cn: Localized-Description
 ldapDisplayName: localizedDescription
 attributeId: 1.2.840.113556.1.4.817
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: FALSE
 schemaIdGuid: d9e18316-8939-11d1-aebc-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.367 Attribute localPolicyFlags

This attribute specifies flags that determine where a machine gets its policy (Local-Policy-Reference).

 cn: Local-Policy-Flags
 ldapDisplayName: localPolicyFlags
 attributeId: 1.2.840.113556.1.4.56
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf96799e-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

134 / 140


Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.368 Attribute localPolicyReference

This attribute specifies the distinguished name of a local policy object that a policy object copies from.

 cn: Local-Policy-Reference
 ldapDisplayName: localPolicyReference
 attributeId: 1.2.840.113556.1.4.457
 attributeSyntax: 2.5.5.1
 omSyntax: 127
 omObjectClass: 1.3.12.2.1011.28.0.714
 isSingleValued: TRUE
 schemaIdGuid: 80a67e4d-9f22-11d0-afdd-00c04fd930c9
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: a29b8a01-c7e8-11d0-9bae-00c04fd92ef5
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.369 Attribute location

This attribute specifies the user's location, such as an office number.

 cn: Location
 ldapDisplayName: location
 attributeId: 1.2.840.113556.1.4.222
 attributeSyntax: 2.5.5.12
 omSyntax: 64
 isSingleValued: TRUE
 schemaIdGuid: 09dcb79f-165f-11d0-a064-00aa006c33ed
 systemOnly: FALSE
 searchFlags: fATTINDEX
 rangeLower: 0
 rangeUpper: 1024
 isMemberOfPartialAttributeSet: TRUE
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

### 2.370 Attribute lockoutDuration

This attribute specifies the amount of time an account is locked due to the Lockout-Threshold being
exceeded. This value is stored as a large integer. It represents the negative of the number of 100
nanosecond intervals that elapse, from the time the Lockout-Threshold is exceeded, before the
account is unlocked.

 cn: Lockout-Duration
 ldapDisplayName: lockoutDuration
 attributeId: 1.2.840.113556.1.4.60
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf9679a5-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: c7407360-20bf-11d0-a768-00aa006e0529

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

135 / 140


 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

In Windows 2000 Server, rangeUpper is not defined.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.371 Attribute lockOutObservationWindow

This attribute specifies the waiting period after which the Lockout Threshold (section 2.372) is reset.
The valid values are <None> and 00:00:00:01 through the Lockout Duration (section 2.370) value.

 cn: Lock-Out-Observation-Window
 ldapDisplayName: lockOutObservationWindow
 attributeId: 1.2.840.113556.1.4.61
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: bf9679a4-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: c7407360-20bf-11d0-a768-00aa006e0529
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.372 Attribute lockoutThreshold

This attribute specifies the number of invalid logon attempts that are permitted before the account is
locked out.

 cn: Lockout-Threshold
 ldapDisplayName: lockoutThreshold
 attributeId: 1.2.840.113556.1.4.73
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf9679a6-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 65535
 attributeSecurityGuid: c7407360-20bf-11d0-a768-00aa006e0529
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

136 / 140


### 2.373 Attribute lockoutTime

This attribute specifies the date and time (UTC) that this account was locked out. This value is stored
as a large integer that represents the number of 100 nanosecond intervals since January 1, 1601
(UTC). A value of zero means that the account is not currently locked out.

 cn: Lockout-Time
 ldapDisplayName: lockoutTime
 attributeId: 1.2.840.113556.1.4.662
 attributeSyntax: 2.5.5.16
 omSyntax: 65
 isSingleValued: TRUE
 schemaIdGuid: 28630ebf-41d5-11d1-a9c1-0000f80367c1
 systemOnly: FALSE
 searchFlags: 0
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.374 Attribute loginShell

This attribute specifies the path to the logon shell. For more information, see [RFC2307] section 2.2.

 cn: LoginShell
 ldapDisplayName: loginShell
 attributeId: 1.3.6.1.1.1.1.4
 attributeSyntax: 2.5.5.5
 omSyntax: 22
 isSingleValued: TRUE
 schemaIdGuid: a553d12c-3231-4c5e-8adf-8d189697721e
 systemOnly: FALSE
 searchFlags: 0
 rangeUpper: 1024

Version-Specific Behavior: First implemented on Windows Server 2003 R2.

### 2.375 Attribute logonCount

This attribute specifies the number of times that the account has successfully logged on. A value of 0
indicates that the value is unknown.

 cn: Logon-Count
 ldapDisplayName: logonCount
 attributeId: 1.2.840.113556.1.4.169
 attributeSyntax: 2.5.5.9
 omSyntax: 2
 isSingleValued: TRUE
 schemaIdGuid: bf9679aa-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: 0
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT | FLAG_ATTR_NOT_REPLICATED
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

137 / 140


The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.376 Attribute logonHours

This attribute specifies the hours that the user is allowed to log on to the domain.

 cn: Logon-Hours
 ldapDisplayName: logonHours
 attributeId: 1.2.840.113556.1.4.64
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf9679ab-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT
 schemaFlagsEx: FLAG_ATTR_IS_CRITICAL

Version-Specific Behavior: First implemented on Windows 2000 Server.

The schemaFlagsEx attribute was added to this attribute definition in Windows Server 2008.

### 2.377 Attribute logonWorkstation

For more information, see the userWorkstations attribute in [MS-ADA3].

 cn: Logon-Workstation
 ldapDisplayName: logonWorkstation
 attributeId: 1.2.840.113556.1.4.65
 attributeSyntax: 2.5.5.10
 omSyntax: 4
 isSingleValued: TRUE
 schemaIdGuid: bf9679ac-0de6-11d0-a285-00aa003049e2
 systemOnly: FALSE
 searchFlags: fCOPY
 attributeSecurityGuid: 5f202010-79a5-11d0-9020-00c04fc2d4cf
 systemFlags: FLAG_SCHEMA_BASE_OBJECT

Version-Specific Behavior: First implemented on Windows 2000 Server.

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

138 / 140


## 3 Change Tracking

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

Revision class

1 Introduction  Added a reference to [MS-ADTS] for the list of applicable products.  Minor

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

139 / 140


## 4 Index
A

Active Directory attributes beginning with A - L 13
Attributes beginning with A - L 13

C

Change tracking 139

I

Introduction 11

S

Schema attributes - Active Directory 13

T

Tracking changes 139

[MS-ADA1] - v20180912
Active Directory Schema Attributes A-L
Copyright © 2018 Microsoft Corporation
Release: September 12, 2018

140 / 140

