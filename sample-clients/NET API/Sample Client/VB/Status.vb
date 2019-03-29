'============================================================================
' TITLE: Status.vb
'
' CONTENTS:
' 
' Defines the server status form for an OPC C# sample application.
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

Public Class Status
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
    Friend WithEvents ButtonsPN As System.Windows.Forms.Panel
    Friend WithEvents CancelBTN As System.Windows.Forms.Button
    Friend WithEvents StartTimeTB As System.Windows.Forms.TextBox
    Friend WithEvents LastUpdateTimeTB As System.Windows.Forms.TextBox
    Friend WithEvents StartTimeLB As System.Windows.Forms.Label
    Friend WithEvents VersionLB As System.Windows.Forms.Label
    Friend WithEvents VersionTB As System.Windows.Forms.TextBox
    Friend WithEvents ServerStateLB As System.Windows.Forms.Label
    Friend WithEvents ServerStateTB As System.Windows.Forms.TextBox
    Friend WithEvents VendorInfoLB As System.Windows.Forms.Label
    Friend WithEvents CurrentTimeLB As System.Windows.Forms.Label
    Friend WithEvents LastUpdateTimeLB As System.Windows.Forms.Label
    Friend WithEvents VendorInfoTB As System.Windows.Forms.TextBox
    Friend WithEvents CurrentTimeTB As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.ButtonsPN = New System.Windows.Forms.Panel
        Me.CancelBTN = New System.Windows.Forms.Button
        Me.StartTimeTB = New System.Windows.Forms.TextBox
        Me.LastUpdateTimeTB = New System.Windows.Forms.TextBox
        Me.StartTimeLB = New System.Windows.Forms.Label
        Me.VersionLB = New System.Windows.Forms.Label
        Me.VersionTB = New System.Windows.Forms.TextBox
        Me.ServerStateLB = New System.Windows.Forms.Label
        Me.ServerStateTB = New System.Windows.Forms.TextBox
        Me.VendorInfoLB = New System.Windows.Forms.Label
        Me.CurrentTimeLB = New System.Windows.Forms.Label
        Me.LastUpdateTimeLB = New System.Windows.Forms.Label
        Me.VendorInfoTB = New System.Windows.Forms.TextBox
        Me.CurrentTimeTB = New System.Windows.Forms.TextBox
        Me.ButtonsPN.SuspendLayout()
        Me.SuspendLayout()
        '
        'ButtonsPN
        '
        Me.ButtonsPN.Controls.Add(Me.CancelBTN)
        Me.ButtonsPN.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.ButtonsPN.Location = New System.Drawing.Point(0, 146)
        Me.ButtonsPN.Name = "ButtonsPN"
        Me.ButtonsPN.Size = New System.Drawing.Size(416, 36)
        Me.ButtonsPN.TabIndex = 17
        '
        'CancelBTN
        '
        Me.CancelBTN.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CancelBTN.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelBTN.Location = New System.Drawing.Point(171, 8)
        Me.CancelBTN.Name = "CancelBTN"
        Me.CancelBTN.TabIndex = 0
        Me.CancelBTN.Text = "Close"
        '
        'StartTimeTB
        '
        Me.StartTimeTB.Location = New System.Drawing.Point(120, 76)
        Me.StartTimeTB.Name = "StartTimeTB"
        Me.StartTimeTB.Size = New System.Drawing.Size(136, 20)
        Me.StartTimeTB.TabIndex = 29
        Me.StartTimeTB.Text = ""
        '
        'LastUpdateTimeTB
        '
        Me.LastUpdateTimeTB.Location = New System.Drawing.Point(120, 124)
        Me.LastUpdateTimeTB.Name = "LastUpdateTimeTB"
        Me.LastUpdateTimeTB.Size = New System.Drawing.Size(136, 20)
        Me.LastUpdateTimeTB.TabIndex = 33
        Me.LastUpdateTimeTB.Text = ""
        '
        'StartTimeLB
        '
        Me.StartTimeLB.Location = New System.Drawing.Point(8, 76)
        Me.StartTimeLB.Name = "StartTimeLB"
        Me.StartTimeLB.Size = New System.Drawing.Size(104, 23)
        Me.StartTimeLB.TabIndex = 28
        Me.StartTimeLB.Text = "Start Time"
        Me.StartTimeLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'VersionLB
        '
        Me.VersionLB.Location = New System.Drawing.Point(8, 28)
        Me.VersionLB.Name = "VersionLB"
        Me.VersionLB.Size = New System.Drawing.Size(104, 23)
        Me.VersionLB.TabIndex = 20
        Me.VersionLB.Text = "Version"
        Me.VersionLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'VersionTB
        '
        Me.VersionTB.Location = New System.Drawing.Point(120, 28)
        Me.VersionTB.Name = "VersionTB"
        Me.VersionTB.Size = New System.Drawing.Size(216, 20)
        Me.VersionTB.TabIndex = 21
        Me.VersionTB.Text = ""
        '
        'ServerStateLB
        '
        Me.ServerStateLB.Location = New System.Drawing.Point(8, 52)
        Me.ServerStateLB.Name = "ServerStateLB"
        Me.ServerStateLB.Size = New System.Drawing.Size(104, 23)
        Me.ServerStateLB.TabIndex = 22
        Me.ServerStateLB.Text = "State"
        Me.ServerStateLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'ServerStateTB
        '
        Me.ServerStateTB.Location = New System.Drawing.Point(120, 52)
        Me.ServerStateTB.Name = "ServerStateTB"
        Me.ServerStateTB.Size = New System.Drawing.Size(216, 20)
        Me.ServerStateTB.TabIndex = 23
        Me.ServerStateTB.Text = ""
        '
        'VendorInfoLB
        '
        Me.VendorInfoLB.Location = New System.Drawing.Point(8, 4)
        Me.VendorInfoLB.Name = "VendorInfoLB"
        Me.VendorInfoLB.Size = New System.Drawing.Size(104, 23)
        Me.VendorInfoLB.TabIndex = 18
        Me.VendorInfoLB.Text = "Vendor Info"
        Me.VendorInfoLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'CurrentTimeLB
        '
        Me.CurrentTimeLB.Location = New System.Drawing.Point(8, 100)
        Me.CurrentTimeLB.Name = "CurrentTimeLB"
        Me.CurrentTimeLB.Size = New System.Drawing.Size(104, 23)
        Me.CurrentTimeLB.TabIndex = 30
        Me.CurrentTimeLB.Text = "Current Time"
        Me.CurrentTimeLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'LastUpdateTimeLB
        '
        Me.LastUpdateTimeLB.Location = New System.Drawing.Point(8, 124)
        Me.LastUpdateTimeLB.Name = "LastUpdateTimeLB"
        Me.LastUpdateTimeLB.Size = New System.Drawing.Size(104, 23)
        Me.LastUpdateTimeLB.TabIndex = 32
        Me.LastUpdateTimeLB.Text = "Last Update Time"
        Me.LastUpdateTimeLB.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'VendorInfoTB
        '
        Me.VendorInfoTB.Location = New System.Drawing.Point(120, 4)
        Me.VendorInfoTB.Name = "VendorInfoTB"
        Me.VendorInfoTB.Size = New System.Drawing.Size(292, 20)
        Me.VendorInfoTB.TabIndex = 19
        Me.VendorInfoTB.Text = ""
        '
        'CurrentTimeTB
        '
        Me.CurrentTimeTB.Location = New System.Drawing.Point(120, 100)
        Me.CurrentTimeTB.Name = "CurrentTimeTB"
        Me.CurrentTimeTB.Size = New System.Drawing.Size(136, 20)
        Me.CurrentTimeTB.TabIndex = 31
        Me.CurrentTimeTB.Text = ""
        '
        'Status
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(416, 182)
        Me.Controls.Add(Me.ButtonsPN)
        Me.Controls.Add(Me.StartTimeTB)
        Me.Controls.Add(Me.LastUpdateTimeTB)
        Me.Controls.Add(Me.VersionTB)
        Me.Controls.Add(Me.ServerStateTB)
        Me.Controls.Add(Me.VendorInfoTB)
        Me.Controls.Add(Me.CurrentTimeTB)
        Me.Controls.Add(Me.StartTimeLB)
        Me.Controls.Add(Me.VersionLB)
        Me.Controls.Add(Me.ServerStateLB)
        Me.Controls.Add(Me.VendorInfoLB)
        Me.Controls.Add(Me.CurrentTimeLB)
        Me.Controls.Add(Me.LastUpdateTimeLB)
        Me.Name = "Status"
        Me.Text = "Status"
        Me.ButtonsPN.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Public Sub ShowStatus(ByVal status As Opc.Da.ServerStatus)

        VendorInfoTB.Text() = status.VendorInfo
        VersionTB.Text() = status.ProductVersion
        ServerStateTB.Text = status.ServerState
        StartTimeTB.Text() = Opc.Convert.ToString(status.StartTime)
        CurrentTimeTB.Text() = Opc.Convert.ToString(status.CurrentTime)
        LastUpdateTimeTB.Text() = Opc.Convert.ToString(status.LastUpdateTime)

        ShowDialog()

    End Sub

End Class
