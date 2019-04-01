# Microsoft Developer Studio Project File - Name="opctest" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Console Application" 0x0103

CFG=opctest - Win32 Release
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "opctest.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "opctest.mak" CFG="opctest - Win32 Release"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "opctest - Win32 Release" (based on "Win32 (x86) Console Application")
!MESSAGE "opctest - Win32 Debug" (based on "Win32 (x86) Console Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
RSC=rc.exe

!IF  "$(CFG)" == "opctest - Win32 Release"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir ".\Release"
# PROP BASE Intermediate_Dir ".\Release"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir ".\Release"
# PROP Intermediate_Dir ".\Release"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /GX /O2 /D "WIN32" /D "NDEBUG" /D "_CONSOLE" /YX /c
# ADD CPP /nologo /MT /W3 /GX /O2 /D "NDEBUG" /D "WIN32" /D "_CONSOLE" /D "_WIN32_DCOM" /YX /FD /c
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /subsystem:console /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /subsystem:console /machine:I386
# SUBTRACT LINK32 /nodefaultlib

!ELSEIF  "$(CFG)" == "opctest - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir ".\Debug"
# PROP BASE Intermediate_Dir ".\Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir ".\Debug"
# PROP Intermediate_Dir ".\Debug"
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /GX /Zi /Od /D "WIN32" /D "_DEBUG" /D "_CONSOLE" /YX /c
# ADD CPP /nologo /MT /W3 /Gm /GX /ZI /Od /D "_DEBUG" /D "WIN32" /D "_CONSOLE" /D "_WIN32_DCOM" /YX /FD /c
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /subsystem:console /debug /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib /nologo /subsystem:console /debug /machine:I386
# SUBTRACT LINK32 /nodefaultlib

!ENDIF 

# Begin Target

# Name "opctest - Win32 Release"
# Name "opctest - Win32 Debug"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;hpj;bat;for;f90"
# Begin Source File

SOURCE=.\get_cats.cpp
# End Source File
# Begin Source File

SOURCE=.\Opc_bas.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_cats.c
# End Source File
# Begin Source File

SOURCE=.\opc_comn.cpp
# End Source File
# Begin Source File

SOURCE=.\Opc_cpt.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_data.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_gsm.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_imgt.cpp
# End Source File
# Begin Source File

SOURCE=.\Opc_pack.cpp
# End Source File
# Begin Source File

SOURCE=.\Opc_prm.cpp
# End Source File
# Begin Source File

SOURCE=.\opc_scpc.cpp
# End Source File
# Begin Source File

SOURCE=.\opccomn_i.c
# End Source File
# Begin Source File

SOURCE=.\opcda_i.c
# End Source File
# Begin Source File

SOURCE=.\opctest.cpp
# End Source File
# Begin Source File

SOURCE=.\wcsutil.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl;fi;fd"
# Begin Source File

SOURCE=.\Opc_pack.h
# End Source File
# Begin Source File

SOURCE=.\wcsutil.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;cnt;rtf;gif;jpg;jpeg;jpe"
# End Group
# Begin Group "Interfaces"

# PROP Default_Filter "*.idl"
# Begin Source File

SOURCE=.\opcSec.idl

!IF  "$(CFG)" == "opctest - Win32 Release"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Step on $(InputPath)
InputPath=.\opcSec.idl
InputName=opcSec

BuildCmds= \
	midl -Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ELSEIF  "$(CFG)" == "opctest - Win32 Debug"

# PROP Ignore_Default_Tool 1
# Begin Custom Build - Performing MIDL Step on $(InputPath)
InputPath=.\opcSec.idl
InputName=opcSec

BuildCmds= \
	midl -Oicf -h $(InputName).h -iid $(InputName)_i.c $(InputName).idl

"$(InputName).h" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)

"$(InputName)_i.c" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
   $(BuildCmds)
# End Custom Build

!ENDIF 

# End Source File
# End Group
# End Target
# End Project
