# Microsoft Developer Studio Project File - Name="OPC_AE_SampleServer" - Package Owner=<4>
# Microsoft Developer Studio Generated Build File, Format Version 6.00
# ** DO NOT EDIT **

# TARGTYPE "Win32 (x86) Application" 0x0101

CFG=OPC_AE_SampleServer - Win32 Debug
!MESSAGE This is not a valid makefile. To build this project using NMAKE,
!MESSAGE use the Export Makefile command and run
!MESSAGE 
!MESSAGE NMAKE /f "OPC_AE_SampleServer.mak".
!MESSAGE 
!MESSAGE You can specify a configuration when running NMAKE
!MESSAGE by defining the macro CFG on the command line. For example:
!MESSAGE 
!MESSAGE NMAKE /f "OPC_AE_SampleServer.mak" CFG="OPC_AE_SampleServer - Win32 Debug"
!MESSAGE 
!MESSAGE Possible choices for configuration are:
!MESSAGE 
!MESSAGE "OPC_AE_SampleServer - Win32 Debug" (based on "Win32 (x86) Application")
!MESSAGE "OPC_AE_SampleServer - Win32 Release MinDependency" (based on "Win32 (x86) Application")
!MESSAGE 

# Begin Project
# PROP AllowPerConfigDependencies 0
# PROP Scc_ProjName ""
# PROP Scc_LocalPath ""
CPP=cl.exe
MTL=midl.exe
RSC=rc.exe

!IF  "$(CFG)" == "OPC_AE_SampleServer - Win32 Debug"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 1
# PROP BASE Output_Dir "Debug"
# PROP BASE Intermediate_Dir "Debug"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 1
# PROP Output_Dir "Debug"
# PROP Intermediate_Dir "Debug"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /Gm /Zi /Od /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /Yu"stdafx.h" /FD /c
# ADD CPP /nologo /MTd /W3 /Gm /GX /ZI /Od /I "..\..\Include" /D "WIN32" /D "_DEBUG" /D "_WINDOWS" /FR /Yu"stdafx.h" /FD /c
# ADD BASE MTL /nologo /D "_DEBUG" /mktyplib203 /o "NUL" /win32
# ADD MTL /nologo /D "_DEBUG" /mktyplib203 /o "NUL" /win32
# ADD BASE RSC /l 0x409 /d "_DEBUG"
# ADD RSC /l 0x409 /d "_DEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib Comctl32.lib version.lib /nologo /subsystem:windows /debug /machine:I386 /pdbtype:sept
# Begin Custom Build - Performing registration
OutDir=.\Debug
TargetPath=.\Debug\OPC_AE_SampleServer.exe
InputPath=.\Debug\OPC_AE_SampleServer.exe
SOURCE="$(InputPath)"

"$(OutDir)\regsvr32.trg" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
	"$(TargetPath)" /RegServer 
	echo regsvr32 exec. time > "$(OutDir)\regsvr32.trg" 
	echo Server registration done! 
	
# End Custom Build

!ELSEIF  "$(CFG)" == "OPC_AE_SampleServer - Win32 Release MinDependency"

# PROP BASE Use_MFC 0
# PROP BASE Use_Debug_Libraries 0
# PROP BASE Output_Dir "ReleaseMinDependency"
# PROP BASE Intermediate_Dir "ReleaseMinDependency"
# PROP BASE Target_Dir ""
# PROP Use_MFC 0
# PROP Use_Debug_Libraries 0
# PROP Output_Dir "ReleaseMinDependency"
# PROP Intermediate_Dir "ReleaseMinDependency"
# PROP Ignore_Export_Lib 0
# PROP Target_Dir ""
# ADD BASE CPP /nologo /W3 /O1 /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_ATL_STATIC_REGISTRY" /D "_ATL_MIN_CRT" /Yu"stdafx.h" /FD /c
# ADD CPP /nologo /MT /W3 /GX /O1 /I "..\..\Include" /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_ATL_STATIC_REGISTRY" /Yu"stdafx.h" /FD /c
# ADD BASE MTL /nologo /D "NDEBUG" /mktyplib203 /o "NUL" /win32
# ADD MTL /nologo /D "NDEBUG" /mktyplib203 /o "NUL" /win32
# ADD BASE RSC /l 0x409 /d "NDEBUG"
# ADD RSC /l 0x409 /d "NDEBUG"
BSC32=bscmake.exe
# ADD BASE BSC32 /nologo
# ADD BSC32 /nologo
LINK32=link.exe
# ADD BASE LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib /nologo /subsystem:windows /machine:I386
# ADD LINK32 kernel32.lib user32.lib gdi32.lib winspool.lib comdlg32.lib advapi32.lib shell32.lib ole32.lib oleaut32.lib uuid.lib odbc32.lib odbccp32.lib Comctl32.lib version.lib /nologo /subsystem:windows /machine:I386
# Begin Custom Build - Performing registration
OutDir=.\ReleaseMinDependency
TargetPath=.\ReleaseMinDependency\OPC_AE_SampleServer.exe
InputPath=.\ReleaseMinDependency\OPC_AE_SampleServer.exe
SOURCE="$(InputPath)"

"$(OutDir)\regsvr32.trg" : $(SOURCE) "$(INTDIR)" "$(OUTDIR)"
	"$(TargetPath)" /RegServer 
	echo regsvr32 exec. time > "$(OutDir)\regsvr32.trg" 
	echo Server registration done! 
	
# End Custom Build

!ENDIF 

# Begin Target

# Name "OPC_AE_SampleServer - Win32 Debug"
# Name "OPC_AE_SampleServer - Win32 Release MinDependency"
# Begin Group "Source Files"

# PROP Default_Filter "cpp;c;cxx;rc;def;r;odl;idl;hpj;bat"
# Begin Source File

SOURCE=.\cathelp.cpp
# End Source File
# Begin Source File

SOURCE=.\condition.cpp
# End Source File
# Begin Source File

SOURCE=.\ConditionMap.cpp
# End Source File
# Begin Source File

SOURCE=.\global.cpp
# End Source File
# Begin Source File

SOURCE=.\MatchPattern.cpp
# End Source File
# Begin Source File

SOURCE=.\OPC_AE_SampleServer.cpp
# End Source File
# Begin Source File

SOURCE=.\OPC_AE_SampleServer.rc
# End Source File
# Begin Source File

SOURCE=.\OPCEventAreaBrowser.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCEventServer.cpp
# End Source File
# Begin Source File

SOURCE=.\OPCEventSubscriptionMgt.cpp
# End Source File
# Begin Source File

SOURCE=.\pseudoMFC.cpp
# End Source File
# Begin Source File

SOURCE=.\QueryContainers.cpp
# End Source File
# Begin Source File

SOURCE=.\ResourceVer.cpp
# End Source File
# Begin Source File

SOURCE=.\ServerStatus.cpp
# End Source File
# Begin Source File

SOURCE=.\simulation.cpp
# End Source File
# Begin Source File

SOURCE=.\StdAfx.cpp
# ADD CPP /Yc"stdafx.h"
# End Source File
# Begin Source File

SOURCE=.\ThreadPool.cpp
# End Source File
# Begin Source File

SOURCE=.\TraceStream.cpp
# End Source File
# Begin Source File

SOURCE=.\tstring.cpp
# End Source File
# End Group
# Begin Group "Header Files"

# PROP Default_Filter "h;hpp;hxx;hm;inl"
# Begin Source File

SOURCE=.\cathelp.h
# End Source File
# Begin Source File

SOURCE=.\condition.h
# End Source File
# Begin Source File

SOURCE=.\ConditionMap.h
# End Source File
# Begin Source File

SOURCE=.\FileTime.h
# End Source File
# Begin Source File

SOURCE=.\global.h
# End Source File
# Begin Source File

SOURCE=.\MatchPattern.h
# End Source File
# Begin Source File

SOURCE=.\opcae_er.h
# End Source File
# Begin Source File

SOURCE=.\opcaedef.h
# End Source File
# Begin Source File

SOURCE=.\OPCBrowseConditions.h
# End Source File
# Begin Source File

SOURCE=.\OPCEventAreaBrowser.h
# End Source File
# Begin Source File

SOURCE=.\OPCEventServer.h
# End Source File
# Begin Source File

SOURCE=.\OPCEventSubscriptionMgt.h
# End Source File
# Begin Source File

SOURCE=.\pseudoMFC.h
# End Source File
# Begin Source File

SOURCE=.\QueryContainers.h
# End Source File
# Begin Source File

SOURCE=.\Resource.h
# End Source File
# Begin Source File

SOURCE=.\ResourceVer.h
# End Source File
# Begin Source File

SOURCE=.\ServerStatus.h
# End Source File
# Begin Source File

SOURCE=.\simulation.h
# End Source File
# Begin Source File

SOURCE=.\StdAfx.h
# End Source File
# Begin Source File

SOURCE=.\ThreadPool.h
# End Source File
# Begin Source File

SOURCE=.\ThreadSafe.h
# End Source File
# Begin Source File

SOURCE=.\TraceStream.h
# End Source File
# Begin Source File

SOURCE=.\tstring.h
# End Source File
# End Group
# Begin Group "Resource Files"

# PROP Default_Filter "ico;cur;bmp;dlg;rc2;rct;bin;cnt;rtf;gif;jpg;jpeg;jpe"
# Begin Source File

SOURCE=.\OPC_AE_SampleServer.rgs
# End Source File
# Begin Source File

SOURCE=.\OPCEventServer.rgs
# End Source File
# End Group
# Begin Group "Simulation Source"

# PROP Default_Filter "xls;"
# Begin Source File

SOURCE=.\OPC_AE_SampleServer.xls
# End Source File
# End Group
# End Target
# End Project
