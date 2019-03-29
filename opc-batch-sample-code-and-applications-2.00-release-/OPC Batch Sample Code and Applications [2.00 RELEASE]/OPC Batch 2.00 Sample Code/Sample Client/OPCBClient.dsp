# Microsoft Developer Studio Project File - Name="OPCBClient" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Application" 0x0101

CFG=OPCBClient - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "OPCBClient.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "OPCBClient.mak" CFG="OPCBClient - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "OPCBClient - Win32 Release" (based on "Win32 (x86) Application")
!MESSAGE "OPCBClient - Win32 Debug" (based on "Win32 (x86) Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "OPCBClient - Win32 Release"

# PROP BASE Use_MFC 5
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 5
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "./Release"
# PROP Intermediate_Dir "./Release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /Yu"stdafx.h" /FD /c
# ADD CPP /nologo /MT /W3 /GX /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_MBCS" /Yu"stdafx.h" /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /subsystem:windows /machine:I386
# ADD LINK32 /nologo /subsystem:windows /machine:I386

!ELSEIF  "$(CFG)" == "OPCBClient - Win32 Debug"

# PROP BASE Use_MFC 5
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 5
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "./Debug"
# PROP Intermediate_Dir "./Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MTd /W3 /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /Yu"stdafx.h" /FD /GZ /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /D "_MBCS" /FR /Yu"stdafx.h" /FD /GZ /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept
# ADD LINK32 /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept

!ENDIF 

# Begin Target

# Name "OPCBClient - Win32 Release"
# Name "OPCBClient - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\ConnectionPointContainer.cpp
# End Source File
# Begin Source File

SOURCE=.\DiagBrowser.cpp
# End Source File
# Begin Source File

SOURCE=.\DiagSelectServer.cpp
# End Source File
# Begin Source File

SOURCE=.\EditDlg.cpp
# End Source File
# Begin Source File

SOURCE=.\EnumOPCBatchSummary.cpp
# End Source File
# Begin Source File

SOURCE=.\FilterDialog.cpp
# End Source File
# Begin Source File

SOURCE=.\IClientShutDown.cpp
# End Source File
# Begin Source File

SOURCE=.\MainFrm.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCBatchServer.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCBatchServer2.cpp
# End Source File
# Begin Source File

SOURCE=.\opcbc_i.c
# SUBTRACT CPP /YX /Yc /Yu
# End Source File
# Begin Source File

SOURCE=.\OPCBClient.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCBClient.rc
# End Source File
# Begin Source File

SOURCE=.\OPCBClientDoc.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCBClientView.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCBrowseServerAddressSpace.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCCommon.cpp
# End Source File
# Begin Source File

SOURCE=.\opccomn_i.c
# SUBTRACT CPP /YX /Yc /Yu
# End Source File
# Begin Source File

SOURCE=.\OPCCPC.cpp
# End Source File
# Begin Source File

SOURCE=.\opcda_i.c
# SUBTRACT CPP /YX /Yc /Yu
# End Source File
# Begin Source File

SOURCE=.\OPCEnumerationSets.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCItemProperties.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCServer.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCServerPublicGroups.cpp
# End Source File
# Begin Source File

SOURCE=.\ParamsResults.cpp
# End Source File
# Begin Source File

SOURCE=.\ParamsResultsDlg.cpp
# End Source File
# Begin Source File

SOURCE=.\PersistFile.cpp
# End Source File
# Begin Source File

SOURCE=.\Server.cpp
# End Source File
# Begin Source File

SOURCE=.\StdAfx.cpp
# ADD CPP /Yc"stdafx.h"
# End Source File
# Begin Source File

SOURCE=.\Unknown.cpp
# End Source File
# Begin Source File

SOURCE=.\Utility.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\ConnectionPointContainer.h
# End Source File
# Begin Source File

SOURCE=.\DiagBrowser.h
# End Source File
# Begin Source File

SOURCE=.\DiagSelectServer.h
# End Source File
# Begin Source File

SOURCE=.\EditDlg.h
# End Source File
# Begin Source File

SOURCE=.\EnumOPCBatchSummary.h
# End Source File
# Begin Source File

SOURCE=.\FilterDialog.h
# End Source File
# Begin Source File

SOURCE=.\IClientShutDown.h
# End Source File
# Begin Source File

SOURCE=.\MainFrm.h
# End Source File
# Begin Source File

SOURCE=.\opc_batch.h
# End Source File
# Begin Source File

SOURCE=.\OPCBatchServer.h
# End Source File
# Begin Source File

SOURCE=.\OPCBatchServer2.h
# End Source File
# Begin Source File

SOURCE=.\OPCBClient.h
# End Source File
# Begin Source File

SOURCE=.\OPCBClientDoc.h
# End Source File
# Begin Source File

SOURCE=.\OPCBClientView.h
# End Source File
# Begin Source File

SOURCE=.\OPCBrowseServerAddressSpace.h
# End Source File
# Begin Source File

SOURCE=.\OPCCommon.h
# End Source File
# Begin Source File

SOURCE=.\OPCCPC.h
# End Source File
# Begin Source File

SOURCE=.\OPCEnumerationSets.h
# End Source File
# Begin Source File

SOURCE=.\OPCItemProperties.h
# End Source File
# Begin Source File

SOURCE=.\OPCServer.h
# End Source File
# Begin Source File

SOURCE=.\OPCServerPublicGroups.h
# End Source File
# Begin Source File

SOURCE=.\PersistFile.h
# End Source File
# Begin Source File

SOURCE=.\Resource.h
# End Source File
# Begin Source File

SOURCE=.\Server.h
# End Source File
# Begin Source File

SOURCE=.\StdAfx.h
# End Source File
# Begin Source File

SOURCE=.\Unknown.h
# End Source File
# Begin Source File

SOURCE=.\Utility.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;rgs;gif;jpg;jpeg;jpe"
# Begin Source File

SOURCE=.\OPCBClient.ico
# End Source File
# Begin Source File

SOURCE=.\res\OPCBClient.rc2
# End Source File
# Begin Source File

SOURCE=.\OPCBClientDoc.ico
# End Source File
# End Group
# Begin Source File

SOURCE=.\ReadMe.txt
# End Source File
# End Target
# End Project
