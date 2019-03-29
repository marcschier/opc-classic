This is the sample code for the OPC Batch Sample Client 2.0.

The client application is based on Al Chisolm's code for
the sample DA client but is written using MFC. Thanks Al! 

To run the client extract the file OPCBClient.exe and run.

Successfully communicating with a server requires that
the server and other necessary files are registered.
For example you might have to register the proxy/stub 
opcbc_ps.dll: 

	 REGSVR32 opcbc_ps.dll


NOTE: the IDL file included with this client is specific
	to the sample Batch Server version 2.0. made 
	available by Will Irwin of Fisher-Rosemount.
	In order for this sample client to communicate 
	with a different server you should replace the IDL
	generated source and header files and recompile.


To rebuild from source. do the following:

1.  Extract everything except the EXE file

2.  Run VC6.0 and open OPCBClient.dsw

3.  Select the build you want (Release or Debug) and 
Rebuild All.


Not Supported:
	IOPCServerPublicGroups
	IPersistFile



The sample client was built to simply exercise the various
methods found in the Interfaces defined in the Batch 
specification.

When the application starts the Connect command from the 
File menu is automatically sent which opens the Select OPC 
Server dialog box. Specify which registered OPC Batch 
server you would like to connect to in the dialog box
and begin testing.

After connecting to the selected server the client 
attempts to get interface pointers from the server. If 
the client is unable to get an interface it reports the 
information in the Debug log window.

The test menu contains a separate popup menu for each of 
the interfaces. Communicate with the server by clicking 
commands from these popup menus.

The client is implemented such that the code used to 
exercise each interface is contained within its own file. 
For example, if you were interested in the code for the 
IOPCCommon interface you could do so by reviewing the code 
within OPCCommon.cpp and OPCCommon.h.

This is the initial offering of the client, any and all 
feedback will be greatly appreciated.

