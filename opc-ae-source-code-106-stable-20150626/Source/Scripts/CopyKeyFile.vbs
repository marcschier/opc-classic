Option Explicit

Dim shell, fso
Dim projectPath, rootDir, srcFile, keyFile

Set shell = WScript.CreateObject("WScript.Shell")

If WScript.Arguments.Count > 0 Then
    projectPath = WScript.Arguments(0)
End If

If WScript.Arguments.Count > 1 Then
    rootDir = WScript.Arguments(1)
End If

Set fso = CreateObject("Scripting.FileSystemObject")

keyFile = rootDir & "..\Keys\OPC Key Pair.master (COM).snk"

If Not fso.FileExists(keyFile) Then
   keyFile = rootDir & "..\..\Keys\OPC Key Pair.master (COM).snk"
End If

If Not fso.FileExists(keyFile) Then
   keyFile = rootDir & "\Keys\OPC Key Pair.test.snk"
End If
  
If fso.FileExists(keyFile) Then

  Set srcFile = fso.GetFile(keyFile)
  srcFile.Copy projectPath & "OPC Key Pair.snk", True  

  WScript.Echo "Copied Key File: " & keyFile

End If