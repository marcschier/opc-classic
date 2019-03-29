# Microsoft Developer Studio Project File - Name="opc_exe" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Application" 0x0101

CFG=opc_exe - Win32 Release
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "opc_exe.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "opc_exe.mak" CFG="opc_exe - Win32 Release"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "opc_exe - Win32 Release" (based on "Win32 (x86) Application")
!MESSAGE "opc_exe - Win32 Debug" (based on "Win32 (x86) Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "opc_exe - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir ".\Release"
# PROP BASE Intermediate_Dir ".\Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "Release"
# PROP Intermediate_Dir "exe_release"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /YX /c
# ADD CPP /nologo /MTd /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /FR /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386 /out:"Release/opc_batch_exe.exe"

!ELSEIF  "$(CFG)" == "opc_exe - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir ".\opc_exe_"
# PROP BASE Intermediate_Dir ".\opc_exe_"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "exe_debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /Zi /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /YX /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /FR /YX /FD /c
# ADD BASE MTL /nologo /D "_DEBUG" /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386 /out:"Debug/opc_batch_exe.exe"

!ENDIF 

# Begin Target

# Name "opc_exe - Win32 Release"
# Name "opc_exe - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;hpj;bat;for;f90"
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

SOURCE=.\enumsamp.cpp
# End Source File
# Begin Source File

SOURCE=.\EXEMAIN.cpp
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

SOURCE=.\I_enumia.cpp
# End Source File
# Begin Source File

SOURCE=.\I_enumst.cpp
# End Source File
# Begin Source File

SOURCE=.\I_enumun.cpp
# End Source File
# Begin Source File

SOURCE=.\I_gsm.cpp
# End Source File
# Begin Source File

SOURCE=.\I_im.cpp
# End Source File
# Begin Source File

SOURCE=.\I_pf.cpp
# End Source File
# Begin Source File

SOURCE=.\I_PRM.CPP
# End Source File
# Begin Source File

SOURCE=.\i_server.cpp
# End Source File
# Begin Source File

SOURCE=.\I_sio.cpp
# End Source File
# Begin Source File

SOURCE=.\item.cpp
# End Source File
# Begin Source File

SOURCE=.\Itemutil.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_cats.c
# End Source File
# Begin Source File

SOURCE=.\opc_err.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_exe.def
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

SOURCE=.\opcfact.cpp
# End Source File
# Begin Source File

SOURCE=.\opcthrd.cpp
# End Source File
# Begin Source File

SOURCE=.\opcutil.cpp
# End Source File
# Begin Source File

SOURCE=.\reg_cat.cpp
# End Source File
# Begin Source File

SOURCE=.\regsvr.cpp
# End Source File
# Begin Source File

SOURCE=.\server.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl;fi;fd"
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
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;cnt;rtf;gif;jpg;jpeg;jpe"
# End Group
# End Target
# End Project
