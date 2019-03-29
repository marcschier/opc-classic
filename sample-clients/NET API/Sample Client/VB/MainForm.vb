'============================================================================
' TITLE: MainForm.vb
'
' CONTENTS:
' 
' Defines the main form for an OPC VB sample application.
'
' (c) Copyright 2002 The OPC Foundation
' ALL RIGHTS RESERVED.
'
' DISCLAIMER:
'  This code is provided by the OPC Foundation solely to assist in 
'  understanding and use of the appropriate OPC Specification(s) and may be 
'  used as set forth in the License Grant section of the OPC Specification.
'  This code is provided as-is and without warranty or support of any sort
'  and is subject to the Warranty and Liability Disclaimers which appear
'  in the printed OPC Specification.
'
' MODIFICATION LOG:
'
' Date       By    Notes
' ---------- ---   -----
' 2002/11/16 RSA   First release.

Imports Opc
Imports Opc.Da

Public Class MainForm

    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents File As System.Windows.Forms.MainMenu
    Friend WithEvents FileMI As System.Windows.Forms.MenuItem
    Friend WithEvents ListServersMI As System.Windows.Forms.MenuItem
    Friend WithEvents ServersLB As System.Windows.Forms.ListBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.File = New System.Windows.Forms.MainMenu()
        Me.FileMI = New System.Windows.Forms.MenuItem()
        Me.ListServersMI = New System.Windows.Forms.MenuItem()
        Me.ServersLB = New System.Windows.Forms.ListBox()
        Me.SuspendLayout()
        '
        'File
        '
        Me.File.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.FileMI})
        '
        'FileMI
        '
        Me.FileMI.Index = 0
        Me.FileMI.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.ListServersMI})
        Me.FileMI.Text = "File"
        '
        'ListServersMI
        '
        Me.ListServersMI.Index = 0
        Me.ListServersMI.Text = "List Servers..."
        '
        'ServersLB
        '
        Me.ServersLB.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ServersLB.Name = "ServersLB"
        Me.ServersLB.Size = New System.Drawing.Size(292, 264)
        Me.ServersLB.TabIndex = 0
        '
        'MainForm
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(292, 273)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.ServersLB})
        Me.Menu = Me.File
        Me.Name = "MainForm"
        Me.Text = "OPC Sample VB Application"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub ListServersMI_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListServersMI.Click

        Try
            'Connects to OpcEnum and gets a list of servers on the local machine.
            Dim enumerator As OpcCom.ServerEnumerator = New OpcCom.ServerEnumerator

            m_serverList = enumerator.GetAvailableServers(Opc.Specification.COM_DA_20)

            'Updates the list box with all known servers.
            Dim entry As Opc.Server

            For Each entry In m_serverList
                ServersLB.Items.Add(entry.Name)
            Next entry

        Catch exception As Exception

            MessageBox.Show(exception.Message)

        End Try

    End Sub

    Private Sub ServersLB_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles ServersLB.DoubleClick

        Dim server As Opc.Da.Server = Nothing

        Try

            Dim index As Int32 = ServersLB.SelectedIndex

            If index <> -1 Then

                'Connect to the server.
                server = m_serverList(index)
                server.Connect()

                'Fetch the server status.
                Dim status As Opc.Da.ServerStatus = server.GetStatus()

                'Show the status in a dialog.
                Dim dialog As Status = New Status
                dialog.ShowStatus(status)

            End If

        Catch exception As Exception

            MessageBox.Show(exception.Message)

        End Try

        If Not server Is Nothing Then
            'Releases the server (MUST be called to release the underlying COM object).
            server.Dispose()
        End If

    End Sub

    Private m_serverList() As Opc.Server

End Class
