[MC-EDMX]:

Entity Data Model for Data Services Packaging Format

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

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

1 / 15

Revision Summary

Date

Revision
History

Revision
Class

Comments

2/27/2009

0.1

Major

First Release.

4/10/2009

0.1.1

Editorial

Changed language and formatting in the technical content.

5/22/2009

0.1.2

Editorial

Changed language and formatting in the technical content.

7/2/2009

0.1.3

Editorial

Changed language and formatting in the technical content.

8/14/2009

0.1.4

Editorial

Changed language and formatting in the technical content.

9/25/2009

0.2

Minor

Clarified the meaning of the technical content.

11/6/2009

0.2.1

Editorial

Changed language and formatting in the technical content.

12/18/2009  0.2.2

Editorial

Changed language and formatting in the technical content.

1/29/2010

0.2.3

Editorial

Changed language and formatting in the technical content.

3/12/2010

1.0

Major

Updated and revised the technical content.

4/23/2010

1.0.1

Editorial

Changed language and formatting in the technical content.

6/4/2010

1.0.2

Editorial

Changed language and formatting in the technical content.

7/16/2010

2.0

Major

Updated and revised the technical content.

8/27/2010

2.0

None

No changes to the meaning, language, or formatting of the
technical content.

10/8/2010

3.0

Major

Updated and revised the technical content.

11/19/2010  3.0.1

Editorial

Changed language and formatting in the technical content.

1/7/2011

4.0

Major

Updated and revised the technical content.

2/11/2011

4.0

3/25/2011

4.0

5/6/2011

4.0

6/17/2011

4.1

9/23/2011

5.0

12/16/2011  5.1

3/30/2012

6.0

7/12/2012

6.0

10/25/2012  6.0

1/31/2013

6.0

None

None

None

Minor

Major

Minor

Major

None

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

Clarified the meaning of the technical content.

Updated and revised the technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

2 / 15

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

Date

Revision
History

Revision
Class

Comments

8/8/2013

6.0

11/14/2013  6.0

2/13/2014

6.0

5/15/2014

6.0

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

6/30/2015

7.0

Major

Significantly changed the technical content.

10/16/2015  7.0

7/14/2016

7.0

None

None

No changes to the meaning, language, or formatting of the
technical content.

No changes to the meaning, language, or formatting of the
technical content.

3/16/2017

8.0

Major

Significantly changed the technical content.

6/1/2017

8.0

None

No changes to the meaning, language, or formatting of the
technical content.

3/13/2019

9.0

Major

Significantly changed the technical content.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

3 / 15

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
Relationship to Protocols and Other Structures ...................................................... 6
Applicability Statement ....................................................................................... 6
Versioning and Localization ................................................................................. 6
Vendor-Extensible Fields ..................................................................................... 7

1.3
1.4
1.5
1.6
1.7

2  Structures ............................................................................................................... 8
edmx:Edmx ....................................................................................................... 8
edmx:DataServices ............................................................................................ 8
edmx:Reference ................................................................................................ 8
edmx:AnnotationsReference ................................................................................ 9

2.1
2.2
2.3
2.4

3  Structure Examples ............................................................................................... 11

4  Security ................................................................................................................. 12

5  Appendix A: Product Behavior ............................................................................... 13

6  Change Tracking .................................................................................................... 14

7  Index ..................................................................................................................... 15

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

4 / 15

1  Introduction

The Entity Data Model for Data Services Packaging Format (EDMX) is an XML-based file format that
serves as the packaging format for the service metadata of a data service.

Data services are specified in [MS-ODATA]. The Entity Data Model (EDM) and the EDM conceptual
schemas are specified in [MC-CSDL].

Sections 1.7 and 2 of this specification are normative. All other sections and examples in this
specification are informative.

1.1  Glossary

This document uses the following terms:

annotation: Any custom, application-specific extension that is applied to an instance of a schema
definition language through the use of custom attributes and elements that are not a part of
that schema definition language.

data service: A server-side application that implements the OData protocol for the purpose of
enabling clients to publish and edit resources. The resources exposed by data services are
described by using the EDM, as specified in [MC-CSDL].

Entity Data Model (EDM): A set of concepts that describes the structure of data, regardless of its

stored form.

schema: The set of attributes and object classes that govern the creation and update of objects.

Uniform Resource Identifier (URI): A string that identifies a resource. The URI is an addressing
mechanism defined in Internet Engineering Task Force (IETF) Uniform Resource Identifier (URI):
Generic Syntax [RFC3986].

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

[MC-CSDL] Microsoft Corporation, "Conceptual Schema Definition File Format".

[RFC2119] Bradner, S., "Key words for use in RFCs to Indicate Requirement Levels", BCP 14, RFC
2119, March 1997, https://www.rfc-editor.org/info/rfc2119

[XMLSCHEMA1] Thompson, H., Beech, D., Maloney, M., and Mendelsohn, N., Eds., "XML Schema Part
1: Structures", W3C Recommendation, May 2001, https://www.w3.org/TR/2001/REC-xmlschema-1-
20010502/

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

5 / 15

1.2.2  Informative References

[MS-NETOD] Microsoft Corporation, "Microsoft .NET Framework Protocols Overview".

[MS-ODATA] Microsoft Corporation, "Open Data Protocol (OData)".

1.3  Overview

An Entity Data Model for Data Services Packaging Format (EDMX) document is an XML-based file
format that serves as the packaging format for the service metadata of a data service.

As specified in [MS-ODATA], clients can obtain the service metadata for a data service with a URI of
the following signature.

 http://<host>/<prefix>/<service path>/$metadata

The data service returns service metadata packaged in an EDMX document. The root of an EDMX
document is an edmx:Edmx element, which contains exactly one edmx:DataServices subelement.
The edmx:DataServices subelement contains zero or more Schema subelements, which specify
Entity Data Model (EDM) conceptual schemas. These EDM conceptual schemas are annotated as
specified in [MS-ODATA].

The structure of an EDMX document resembles the following example.

 <edmx:Edmx>
   <edmx:DataServices>
     <!-- Entity Data Model Conceptual Schemas, as specified in
          [MC-CSDL] and annotated as specified in [MS-ODATA] -->
     <Schema>

     </Schema>
     <!--
        Additional Entity Data Model Conceptual Schemas as
        specified in [MC-CSDL] and annotated as specified in [MS-ODATA]
     -->
   </edmx:DataServices>
 </edmx:Edmx>

The contents of an EDMX document are determined by the data service in question and vary
depending on the data service, as specified in [MS-ODATA].

1.4  Relationship to Protocols and Other Structures

EDMX serves as the packaging format of the metadata of a data service (as specified in [MS-
ODATA]).

1.5  Applicability Statement

An EDMX document is used when clients of a data service (as specified in [MS-ODATA]) require the
metadata of the data service.

1.6  Versioning and Localization

This document specifies version 1.0 of EDMX.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

6 / 15

1.7  Vendor-Extensible Fields

An EDMX document does not contain any vendor-extensible fields, nor does it support extensibility.
However, the Entity Data Model (EDM) conceptual schemas that are packaged in an EDMX
document support an extension mechanism through the use of annotations (AnnotationAttribute
and AnnotationElement), as specified in [MC-CSDL].

Parsers of EDMX documents ignore content that is unexpected or that cannot be parsed.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

7 / 15

2  Structures

2.1  edmx:Edmx

The edmx:Edmx element defines the XML namespace for the EDMX document and contains the
edmx:DataServices subelement.

The following example uses the edmx:EDMX element.

 <edmx:Edmx Version="1.0" xmlns:edmx="http://schemas.microsoft.com/ado/2007/06/edmx">

The following rules apply to the edmx:Edmx element:

  An EDMX document MUST have exactly one edmx:Edmx element as its root element.





The Version attribute MUST be defined on the edmx:Edmx element. Version is of type
xs:string, as specified in the XML schema [XMLSCHEMA1].

The edmx:Edmx element can contain a choice of zero or more of each of the following
subelements:

  edmx:Reference

  edmx:AnnotationsReference

  Subelements in a given choice set can appear in any given order.



The edmx:Edmx element specifies exactly one edmx:DataServices subelement. This
subelement MUST appear after the edmx:Reference and edmx:AnnotationReference
subelements, if present.

2.2  edmx:DataServices

The edmx:DataServices element contains the service metadata of a data service. This service
metadata contains zero or more Entity Data Model (EDM) conceptual schemas (as specified in
[MC-CSDL]), which are annotated as specified in [MS-ODATA].

The following represents the edmx:DataServices element.

   <edmx:DataServices>

The following rule applies to the edmx:DataServices element:



The edmx:DataServices element can contain any number of Schema sublements.<1>

2.3  edmx:Reference

The edmx:Reference element is used to reference another EDMX document or an Entity Data
Model (EDM) conceptual schema.

The following examples use the edmx:Reference element.

 <edmx:Reference Url="http://www.fabrikam.com/model.edmx" />
 <edmx:Reference Url="http://www.fabrikam.com/model.csdl" />

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

8 / 15

The following rules apply to the edmx:Reference element:





The Url attribute MUST be defined on the edmx:Reference element. Url is of type xs:anyURI,
as specified in the XML schema [XMLSCHEMA1]. Url specifies a URI that resolves to the
referenced EDMX document or to the EDM conceptual schema. Url MUST be an absolute URL.

If edmx:Reference is defined in an EDMX document, processors incorporate the referenced
EDMX document or the EDM conceptual schema.

2.4  edmx:AnnotationsReference

The edmx:AnnotationsReference element is used to reference annotations (as specified in [MC-
CSDL]) specified in another EDMX document or in an Entity Data Model (EDM) conceptual schema.

The following examples use the edmx:AnnotationsReference element.

 <edmx:AnnotationsReference Url="http://fabrikam.com/Annotations.edmx">
    <edmx:Include TermNamespace="Com.Fabrikam.Model" Qualifier="Phone" />
 </edmx:AnnotationsReference>

 <edmx:AnnotationsReference Url="http://fabrikam.com/Annotations.edmx">
    <edmx:Include TermNamespace="Com.Fabrikam.Model" />
 </edmx:AnnotationsReference>
 <edmx:AnnotationsReference Url="http://fabrikam.com/Annotations.edmx">
    <edmx:Include  Qualifier="Phone" />
 </edmx:AnnotationsReference>

 <edmx:AnnotationsReference Url="http://fabrikam.com/Annotations.edmx">
    <edmx:Include />
 </edmx:AnnotationsReference>

The following rules apply to the edmx:AnnotationsReference element:

















The Url attribute MUST be defined on the edmx:AnnotationsReference element. Url is of type
xs:anyURI, as specified in the XML schema ([XMLSCHEMA1]). Url specifies a URI that resolves
to the referenced EDMX document or to the EDM conceptual schema that contains annotations.
Url MUST be an absolute URL.

The edmx:AnnotationsReference element MUST contain one or more edmx:Include
subelements. edmx:Include is used to define the external annotations that are specified in the
referenced EDMX document or in the EDM conceptual schema.

If the edmx:AnnotationsReference element is defined in an EDMX document, processors MAY
ignore the edmx:AnnotationsReference element.

If processors do not ignore the edmx:AnnotationsReference element, processors MUST
incorporate only the Annotations elements (as specified in [MC-CSDL]) and ignore all other EDM
conceptual schema elements (as specified in [MC-CSDL]).

The TermNamespace attribute MAY be defined on the edmx:Include subelement.
TermNamespace is of type xs:string and indicates which annotations are to be included.

The Qualifier attribute MAY be defined on the edmx:Include subelement. Qualifier is of type
xs:string and indicates which annotations are to be included.

If the Qualifier attribute is specified as an empty string, it is considered to be not specified.

If only the TermNamespace attribute is defined on the edmx:Include subelement,
edmx:AnnotationsReference includes all annotations that apply terms that are in the specified
TermNamespace, regardless of the Qualifier.

9 / 15

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019







If both TermNamespace and Qualifier attributes are defined on the edmx:Include subelement,
edmx:AnnotationsReference includes all annotations that apply terms that are in the specified
TermNamespace and have the specified Qualifier.

If only the Qualifier attribute is defined on the edmx:Include subelement,
edmx:AnnotationsReference includes all annotations that apply terms that have the specified
Qualifier, regardless of the namespace of the terms.

If neither the TermNamespace nor the Qualifier attribute is defined on the edmx:Include
subelement, edmx:AnnotationsReference includes all annotations.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

10 / 15

3  Structure Examples

The following is an example of the service metadata returned by a data service. The edmx:Edmx
and edmx:DataServices elements are specified in sections 2.1 and 2.2 of this document. All other
XML elements are specified in [MC-CSDL] and [MS-ODATA].

 <edmx:Edmx Version="1.0" xmlns:edmx="http://schemas.microsoft.com/ado/2007/06/edmx">
   <edmx:DataServices>
     <Schema Namespace="NorthwindModel"
 xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices"
 xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"
 xmlns="http://schemas.microsoft.com/ado/2006/04/edm">
       <EntityContainer Name="NorthwindEntities" m:IsDefaultEntityContainer="true">
         <EntitySet Name="OrderDetails" EntityType="NorthwindModel.OrderDetail" />
         <EntitySet Name="Orders" EntityType="NorthwindModel.Order" />
         <AssociationSet Name="OrderDetails_Orders"
Association="NorthwindModel.OrderDetails_Orders">
           <End Role="Orders" EntitySet="Orders" />
           <End Role="OrderDetails" EntitySet="OrderDetails" />
         </AssociationSet>
       </EntityContainer>
       <EntityType Name="OrderDetail">
         <Key>
           <PropertyRef Name="OrderID" />
           <PropertyRef Name="ProductID" />
         </Key>
         <Property Name="Discount" Type="Edm.Single" Nullable="false" />
         <Property Name="OrderID" Type="Edm.Int32" Nullable="false" />
         <Property Name="ProductID" Type="Edm.Int32" Nullable="false" />
         <Property Name="Quantity" Type="Edm.Int16" Nullable="false" />
         <Property Name="UnitPrice" Type="Edm.Decimal" Nullable="false" Precision="19"
Scale="4" />
         <NavigationProperty Name="Order" Relationship="NorthwindModel.OrderDetails_Orders"
FromRole="OrderDetails" ToRole="Orders" />
       </EntityType>
       <EntityType Name="Order">
         <Key>
           <PropertyRef Name="OrderID" />
         </Key>
         <Property Name="CustomerID" Type="Edm.String" Nullable="true" MaxLength="5"
Unicode="true" FixedLength="true" />
         <Property Name="OrderDate" Type="Edm.DateTime" Nullable="true" />
         <Property Name="OrderID" Type="Edm.Int32" Nullable="false" />
         <Property Name="ShipAddress" Type="Edm.String" Nullable="true" MaxLength="60"
Unicode="true" FixedLength="false" />
         <NavigationProperty Name="OrderDetails"
Relationship="NorthwindModel.OrderDetails_Orders" FromRole="Orders" ToRole="OrderDetails" />
       </EntityType>
       <Association Name="OrderDetails_Orders">
         <End Role="Orders" Type="NorthwindModel.Order" Multiplicity="1" />
         <End Role="OrderDetails" Type="NorthwindModel.OrderDetail" Multiplicity="*" />
         <ReferentialConstraint>
           <Principal Role="Orders">
             <PropertyRef Name="OrderID" />
           </Principal>
           <Dependent Role="OrderDetails">
             <PropertyRef Name="OrderID" />
           </Dependent>
         </ReferentialConstraint>
       </Association>
     </Schema>
   </edmx:DataServices>
 </edmx:Edmx>

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

11 / 15

4  Security

None.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

12 / 15

5  Appendix A: Product Behavior

The information in this specification is applicable to the following Microsoft products or supplemental
software. References to product versions include updates to those products.

This document specifies version-specific details in the Microsoft .NET Framework. For information
about which versions of .NET Framework are available in each released Windows product or as
supplemental software, see [MS-NETOD] section 4.

  Microsoft .NET Framework 3.5 Service Pack 1 (SP1)

  Microsoft .NET Framework 4.0

  Microsoft .NET Framework 4.5

  Microsoft .NET Framework 4.6

  Microsoft .NET Framework 4.7

  Microsoft .NET Framework 4.8

Exceptions, if any, are noted in this section. If an update version, service pack or Knowledge Base
(KB) number appears with a product name, the behavior changed in that update. The new behavior
also applies to subsequent updates unless otherwise specified. If a product edition appears with the
product version, behavior is different in that product edition.

Unless otherwise specified, any statement of optional behavior in this specification that is prescribed
using the terms "SHOULD" or "SHOULD NOT" implies product behavior in accordance with the
SHOULD or SHOULD NOT prescription. Unless otherwise specified, the term "MAY" implies that the
product does not follow the prescription.

<1> Section 2.2: Microsoft implementations always have at least one Schema subelement.

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

13 / 15

6  Change Tracking

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

Added .NET Framework 4.8 to the list of applicable
products.

Revision
class

Major

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

14 / 15

References 5
   informative 6
   normative 5
Relationship to protocols and other structures 6

S

Structures
   edmx:AnnotationsReference 9
   edmx:DataServices 8
   edmx:Edmx 8
   edmx:Reference 8

T

Tracking changes 14

V

Vendor-extensible fields 7
Versioning 6

7  Index
A

Applicability 6

C

Change tracking 14

D

Details
   edmx:AnnotationsReference element 9
   edmx:DataServices element 8
   edmx:Edmx element 8
   edmx:Reference element 8

E

edmx:AnnotationsReference element 9
edmx:DataServices element 8
edmx:Edmx element 8
edmx:Referenceelement 8
Elements
   edmx:AnnotationsReference 9
   edmx:DataServices 8
   edmx:Edmx 8
   edmx:Reference 8
Examples 11

F

Fields - vendor-extensible 7

G

Glossary 5

I

Informative references 6
Introduction 5

L

Localization 6

N

Normative references 5

O

Overview (synopsis) 6

P

Product behavior 13

R

[MC-EDMX] - v20190313
Entity Data Model for Data Services Packaging Format
Copyright © 2019 Microsoft Corporation
Release: March 13, 2019

15 / 15

