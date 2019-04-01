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
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /YX /c
# ADD CPP /nologo /MTd /W3 /GX /O2 /D "NDEBUG" /D "WIN32" /D "_WINDOWS" /D "_WIN32_DCOM" /YX /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386

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
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /Zi /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /YX /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /D "_DEBUG" /D "WIN32" /D "_WINDOWS" /D "_WIN32_DCOM" /FR /YX /FD /c
# ADD BASE MTL /nologo /D "_DEBUG" /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386

!ENDIF 

# Begin Target

# Name "opc_exe - Win32 Release"
# Name "opc_exe - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;hpj;bat;for;f90"
# Begin Source File

SOURCE=.\AccessCheck.cpp
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

SOURCE=.\I_bsas.cpp
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

SOURCE=.\I_Security.cpp
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

SOURCE=.\opc_exe.def
# End Source File
# Begin Source File

SOURCE=.\Opc_pack.cpp
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

SOURCE=.\PrivateSecurity.CPP
# End Source File
# Begin Source File

SOURCE=.\reg_cat.cpp
# End Source File
# Begin Source File

SOURCE=.\regsvr.cpp
# End Source File
# Begin Source File

SOURCE=.\SecurityDescriptor.cpp
# End Source File
# Begin Source File

SOURCE=.\server.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl;fi;fd"
# Begin Source File

SOURCE=.\AccessCheck.h
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

SOURCE=.\OPC_PACK.h
# End Source File
# Begin Source File

SOURCE=.\opcerror.h
# End Source File
# Begin Source File

SOURCE=.\OPCFact.h
# End Source File
# Begin Source File

SOURCE=.\OPCTHRD.h
# End Source File
# Begin Source File

SOURCE=.\Opcxxx.h
# End Source File
# Begin Source File

SOURCE=.\PrivateSecurity.H
# End Source File
# Begin Source File

SOURCE=.\SecurityDescriptor.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;cnt;rtf;gif;jpg;jpeg;jpe"
# End Group
# Begin Group "Interfaces"

# PROP Default_Filter "*.idl"
# Begin Source File

SOURCE=.\OPCCOMN.IDL

!IF  "$(CFG)" == "opc_exe - Win32 Release"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\OPCCOMN.IDL
InputName=OPCCOMN

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ELSEIF  "$(CFG)" == "opc_exe - Win32 Debug"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\OPCCOMN.IDL
InputName=OPCCOMN

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ENDIF 

# End Source File
# Begin Source File

SOURCE=.\OPCDA.IDL

!IF  "$(CFG)" == "opc_exe - Win32 Release"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\OPCDA.IDL
InputName=OPCDA

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ELSEIF  "$(CFG)" == "opc_exe - Win32 Debug"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\OPCDA.IDL
InputName=OPCDA

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ENDIF 

# End Source File
# Begin Source File

SOURCE=.\opcSec.idl

!IF  "$(CFG)" == "opc_exe - Win32 Release"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\opcSec.idl
InputName=opcSec

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ELSEIF  "$(CFG)" == "opc_exe - Win32 Debug"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Build Step on $(InputPath)
InputPath=.\opcSec.idl
InputName=opcSec

BuildCmds= \
	midl /Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName).tlb" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ENDIF 

# End Source File
# End Group
# Begin Source File

SOURCE=.\ReadMe.htm
# PROP Exclude_From_Build 1
# End Source File
# End Target
# End Project
