This is the sample code for version 2.0 of the OPC Batch 
Sample Server.

The main changes from version 1.0 are:

1.  Reflects the new functionality added in v2.0 of the OPC Batch
    Specification.  This includes new models in the namespace for
    representing archived batches and master recipes and a new
    interface allowing the client to ask for a filtered batch list.

2.  Incorporates (courtesy of Dave Emerson and James Wang of Yokogawa)
    a new XML-based technique for creating the sample data.  Now users
    can modify the sample data without editing the source code, simply
    by editing an XML file.


To install the new compiled code, do the following:

1.  Extract the files opc_batch_exe.exe, opc_batch_dll.dll
and opcbc_ps.dll to a directory of your choice.  Use the RegOPCB.BAT
file, or follow steps 2-5 below to register manually:

2.  Register the proxy/stub:

			 REGSVR32 opcbc_ps.dll

3.  Register the inproc server

			 REGSVR32 opc_batch_dll.dll

4.  Register the out-of-proc server

			 opc_batch_exe /RegServer

5.  If necessary (i.e. if you have not already done it for some other
		OPC application), register the standard OPC proxy/stubs
		(OPCProxy.dll and opccomn_ps.dll) which are available from the OPC
		Foundation web site (see the "Downloads for Programmers" page
		under the "Technical Network" page).

6.  This server program uses a Microsoft XML DOM object to manipulate
    the data file. You need install Internet Explorer 5.0 (or later)
    browser in order to gain the access to the DOM object.  The code
    has been tested with versions 5.0 and 5.5 of IE.

7. The implementation kit is zipped in one file:
   OPCBSampleServer_2_0.zip. At the 'root' of this zip file are the
   executables and DLLs, plus the XML data and schema files.  There is
   also the PropertyIDTable.dat file.

	 The sub-directory Server in the ZIP file contains the source files
	 of the OPCB sample server.

8. For simplicity, the OPCB sample server used a fixed target
   directory, named C:\OPCB for data input. It is required for the
   server to read the proper files. You can change this configuration
   easily in the source file.



To rebuild from source. do the following:

1.  Extract everything from the Server directory in the ZIP except the
		DLL and EXE files to a directory of your choice.

2.  Run VC6.0 and open the workspace OPC_EXE.DSW or OPC_DLL.DSW

3.  Select the config you want (Release or Debug) and do Rebuild All.

4.  Register what you have compiled, as shown above.

The server registers itself under the component categories "OPC Batch
Server 1.0" and "OPC Batch Server 2.0" and has the ProgID
"OPC.BatchSample.1".  It also implements the standard IOPCServer (and
other Data Access) interfaces so any OPC Data Access client should be
able to connect to it.

This version of the server does not include the automation interfaces.
A separate download is available from the OPC Foundation web site to
provide support for Visual Basic clients.

This version of the sample server includes all the functionality
defined in the 2.0 version of the spec.  If the C:\OPCB directory
contains the XML files, these will be loaded to provide the sample
data.  Otherwise, a number of sample objects will be created based on
hard-coded values in the server.  These sample objects have all
'Required' and 'Optional' properties defined, although many of them
have only default values.

A lot of the functionality can be explored by any Data Access 2.0
client via the IOPCBrowseServerAddressSpace and IOPCProperties
interfaces.  Do a 'Browse To' "OPCBBatchModel" or "OPCBPhysicalModel"
or "OPCBBatchIDList" and start browsing down from there.

To try out the IOPCBatchServer, IOPCBatchServer2, IOPCEnumerationSets
and IEnumOPCBatchSummary interfaces you will need the OPC Batch Sample
Client.


Known limitations of this version:

1.  IOPCBrowseServerAddressSpace::BrowseAccessPaths is not
implemented.  This method is not required for the batch sample server.
It might be useful to a real OPC Batch server.

There is a Word file SampleServerNotes.doc which has some notes on how
the sample server is implemented, how the sample data can be extended
etc.


The following notes are taken from the file SAMPLE.TXT in Al
Chisholm's Data Access Sample Server.  They are also applicable to
this Batch Sample Server:

This set of files represents very preliminary sample code for an OPC
Batch server.  The OPC Data Access code used here is compliant with
version 2.04 of the Data Access Specification.

Disclaimer
----------
This code is provided for educational purposes only.
It is provided as-is and without support or warranty of any sort.
You are free to study this code and to use the techniques it shows in
your own implementation.

What it contains:
-----------------
A basic DLL and EXE implementation of all of the required OPC Custom 
Interfaces.
Due to the relative simplicity of the implementation it uses 'raw' OLE 
and does not use MFC.

The code uses a technique called 'tear-off' COM interfaces.
This technique has the benfit of being very efficient at runtime
and also of producding code which is very easy to read, understand
and maintain.

Technically this means that the object (the server or group) and
the interfaces it provides are implemented as completely separate 
classes. A quick look at the QueryInterface implementation of either
object clearly shows this technique.

The code and header files use 'XXX' to identify the specific
implementation. You can globally Search and Replace this if desired
to substitute a vendor specific string.

If you do use this code remember you MUST generate a unique GUID for
your server and incorporate that into the file and the OPCXXX.H
file.











