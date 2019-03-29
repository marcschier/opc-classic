# Microsoft Developer Studio Project File - Name="opc_dll" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Dynamic-Link Library" 0x0102

CFG=opc_dll - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "opc_dll.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "opc_dll.mak" CFG="opc_dll - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "opc_dll - Win32 Release" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE "opc_dll - Win32 Debug" (based on "Win32 (x86) Dynamic-Link Library")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "opc_dll - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "Release"
# PROP BASE Intermediate_Dir "Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "dll_Release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /YX /FD /c
# ADD CPP /nologo /MT /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /o "NUL" /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /o "NUL" /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /dll /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /dll /machine:I386 /out:"Release/opc_batch_dll.dll"

!ELSEIF  "$(CFG)" == "opc_dll - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "dll_Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /MTd /W3 /Gm /GX /Zi /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /YX /FD /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /FR /YX /FD /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /o "NUL" /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /o "NUL" /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /dll /debug /machine:I386 /pdbtype:sept
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /dll /debug /machine:I386 /out:"Debug/opc_batch_dll.dll" /pdbtype:sept

!ENDIF 

# Begin Target

# Name "opc_dll - Win32 Release"
# Name "opc_dll - Win32 Debug"
# Begin Group "SourceFiles"

# PROP Default_Filter ""
# Begin Source File

SOURCE=.\B_item.cpp
# End Source File
# Begin Source File

SOURCE=.\BatchItems.cpp
# End Source File
# Begin Source File

SOURCE=.\BatchItemSupport.cpp
# End Source File
# Begin Source File

SOURCE=.\BATCHSUMMUtil.cpp
# End Source File
# Begin Source File

SOURCE=.\Dllmain.cpp
# End Source File
# Begin Source File

SOURCE=.\enumsamp.cpp
# End Source File
# Begin Source File

SOURCE=.\Group.cpp
# End Source File
# Begin Source File

SOURCE=.\Grp_cp.cpp
# End Source File
# Begin Source File

SOURCE=.\Grp_do.cpp
# End Source File
# Begin Source File

SOURCE=.\Grp_list.cpp
# End Source File
# Begin Source File

SOURCE=.\I_asio.cpp
# End Source File
# Begin Source File

SOURCE=.\I_asio2.cpp
# End Source File
# Begin Source File

SOURCE=.\I_BatchBSAS.cpp
# End Source File
# Begin Source File

SOURCE=.\I_bes.cpp
# End Source File
# Begin Source File

SOURCE=.\I_bsvr.cpp
# End Source File
# Begin Source File

SOURCE=.\I_bsvr2.cpp
# End Source File
# Begin Source File

SOURCE=.\I_COMN.CPP
# End Source File
# Begin Source File

SOURCE=.\I_cp.cpp
# End Source File
# Begin Source File

SOURCE=.\I_cpc.cpp
# End Source File
# Begin Source File

SOURCE=.\I_do.cpp
# End Source File
# Begin Source File

SOURCE=.\I_ENUMBS.CPP
# End Source File
# Begin Source File

SOURCE=.\I_enumcp.cpp
# End Source File
# Begin Source File

SOURCE=.\I_ENUMIA.CPP
# End Source File
# Begin Source File

SOURCE=.\I_ENUMST.CPP
# End Source File
# Begin Source File

SOURCE=.\I_ENUMUN.CPP
# End Source File
# Begin Source File

SOURCE=.\I_gsm.cpp
# End Source File
# Begin Source File

SOURCE=.\I_im.cpp
# End Source File
# Begin Source File

SOURCE=.\I_PF.CPP
# End Source File
# Begin Source File

SOURCE=.\I_PRM.CPP
# End Source File
# Begin Source File

SOURCE=.\I_server.cpp
# End Source File
# Begin Source File

SOURCE=.\I_sio.cpp
# End Source File
# Begin Source File

SOURCE=.\Item.cpp
# End Source File
# Begin Source File

SOURCE=.\ITEMUTIL.CPP
# End Source File
# Begin Source File

SOURCE=.\opc_cats.c
# End Source File
# Begin Source File

SOURCE=.\opc_dll.def
# End Source File
# Begin Source File

SOURCE=.\opc_err.cpp
# End Source File
# Begin Source File

SOURCE=.\Opc_pack.cpp
# End Source File
# Begin Source File

SOURCE=.\Opcb_cats.c
# End Source File
# Begin Source File

SOURCE=.\opcbc_i.c
# End Source File
# Begin Source File

SOURCE=.\OPCBXMLData.cpp
# End Source File
# Begin Source File

SOURCE=.\opccomn_i.c
# End Source File
# Begin Source File

SOURCE=.\opcda_i.c
# End Source File
# Begin Source File

SOURCE=.\OPCFACT.CPP
# End Source File
# Begin Source File

SOURCE=.\OPCTHRD.CPP
# End Source File
# Begin Source File

SOURCE=.\OPCUTIL.CPP
# End Source File
# Begin Source File

SOURCE=.\reg_cat.cpp
# End Source File
# Begin Source File

SOURCE=.\regsvr.cpp
# End Source File
# Begin Source File

SOURCE=.\SERVER.CPP
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter ""
# Begin Source File

SOURCE=.\BatchItemSupport.h
# End Source File
# Begin Source File

SOURCE=.\CPCHELP.H
# End Source File
# Begin Source File

SOURCE=.\ENUMCP.h
# End Source File
# Begin Source File

SOURCE=.\Enumhelp.h
# End Source File
# Begin Source File

SOURCE=.\enumsamp.h
# End Source File
# Begin Source File

SOURCE=.\OPC_PACK.h
# End Source File
# Begin Source File

SOURCE=.\OPCBatchDef.h
# End Source File
# Begin Source File

SOURCE=.\OPCBatchError.h
# End Source File
# Begin Source File

SOURCE=.\OPCBatchProps.h
# End Source File
# Begin Source File

SOURCE=.\opcbc.h
# End Source File
# Begin Source File

SOURCE=.\OPCBXMLData.h
# End Source File
# Begin Source File

SOURCE=.\opccomn.h
# End Source File
# Begin Source File

SOURCE=.\opcda.h
# End Source File
# Begin Source File

SOURCE=.\opcerror.h
# End Source File
# Begin Source File

SOURCE=.\OPCFact.h
# End Source File
# Begin Source File

SOURCE=.\OPCProps.h
# End Source File
# Begin Source File

SOURCE=.\OPCTHRD.h
# End Source File
# Begin Source File

SOURCE=.\Opcxxx.h
# End Source File
# End Group
# End Target
# End Project
