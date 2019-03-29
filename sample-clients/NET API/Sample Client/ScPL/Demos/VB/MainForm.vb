'ScPl - A plotting library for .NET

'MainForm.vb
'Copyright (C) 2003 
'Paolo Pierini, Matt Howlett
'
'Redistribution and use in source and binary forms, with or without
'modification, are permitted provided that the following conditions
'are met:
'
'1. Redistributions of source code must retain the above copyright
'   notice, this list of conditions and the following disclaimer.
'
'2. Redistributions in binary form must reproduce the following text in 
'   the documentation and / or other materials provided with the 
'   distribution: 
'
'   "This product includes software developed as part of 
'   the ScPl plotting library project available from: 
'   http://www.netcontrols.org/scpl/" 
'
'------------------------------------------------------------------------
'
'THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
'IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
'OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
'IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
'INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
'NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
'DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
'THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
'(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
'THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'
'$Id: XMLAdapter.cs,v 1.4 2003/10/16 01:57:16 mef526 Exp $



Imports scpl
Imports System.Drawing.Printing

Public Class MainForm
    Inherits System.Windows.Forms.Form

    ' delegates to handle the next plot button
    Delegate Sub PlotDelegate()
    ' list of the plot routines, to be initialized in the form constructor
    Private PlotRoutines() As PlotDelegate

    Private currentPlot As Integer = 0

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        PlotRoutines = New PlotDelegate() {New PlotDelegate(AddressOf PlotWavelet), _
                       New PlotDelegate(AddressOf PlotLogAxis), _
                       New PlotDelegate(AddressOf PlotLogLog), _
               New PlotDelegate(AddressOf PlotSincFunction), _
               New PlotDelegate(AddressOf PlotGaussian), _
               New PlotDelegate(AddressOf PlotStep), _
               New PlotDelegate(AddressOf PlotLabelAxis), _
               New PlotDelegate(AddressOf PlotParticles), _
               New PlotDelegate(AddressOf DashPlot)}
        currentPlot = 0
        plotSurface.BackColor = Color.White
        Dim id As Integer = currentPlot + 1
        Label1.Text = "Plot " & id.ToString("0") & "/" & PlotRoutines.Length.ToString("0")
        PlotRoutines(currentPlot)()
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
    Friend WithEvents plotSurface As scpl.Windows.PlotSurface2D
    Friend WithEvents PrintDialog1 As System.Windows.Forms.PrintDialog
    Friend WithEvents PrintDocument1 As System.Drawing.Printing.PrintDocument
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents printButton As System.Windows.Forms.Button
    Friend WithEvents linkLabel1 As System.Windows.Forms.LinkLabel
    Friend WithEvents clear As System.Windows.Forms.Button
    Friend WithEvents quit As System.Windows.Forms.Button
    Friend WithEvents nextPlot As System.Windows.Forms.Button
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents prevPlot As System.Windows.Forms.Button
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.plotSurface = New scpl.Windows.PlotSurface2D()
        Me.PrintDialog1 = New System.Windows.Forms.PrintDialog()
        Me.PrintDocument1 = New System.Drawing.Printing.PrintDocument()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.printButton = New System.Windows.Forms.Button()
        Me.linkLabel1 = New System.Windows.Forms.LinkLabel()
        Me.clear = New System.Windows.Forms.Button()
        Me.quit = New System.Windows.Forms.Button()
        Me.nextPlot = New System.Windows.Forms.Button()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.prevPlot = New System.Windows.Forms.Button()
        Me.Panel1.SuspendLayout()
        Me.Panel2.SuspendLayout()
        Me.SuspendLayout()
        '
        'plotSurface
        '
        Me.plotSurface.AllowSelection = True
        Me.plotSurface.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.plotSurface.Dock = System.Windows.Forms.DockStyle.Fill
        Me.plotSurface.Name = "plotSurface"
        Me.plotSurface.Padding = 10
        Me.plotSurface.Size = New System.Drawing.Size(496, 318)
        Me.plotSurface.TabIndex = 0
        Me.plotSurface.Title = ""
        Me.plotSurface.TitleFont = New System.Drawing.Font("Arial", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel)
        Me.plotSurface.XAxis1 = Nothing
        Me.plotSurface.XAxis2 = Nothing
        Me.plotSurface.YAxis1 = Nothing
        Me.plotSurface.YAxis2 = Nothing
        '
        'PrintDocument1
        '
        '
        'Panel1
        '
        Me.Panel1.Controls.AddRange(New System.Windows.Forms.Control() {Me.prevPlot, Me.Label1, Me.printButton, Me.linkLabel1, Me.clear, Me.quit, Me.nextPlot})
        Me.Panel1.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.Panel1.Location = New System.Drawing.Point(0, 318)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(496, 48)
        Me.Panel1.TabIndex = 14
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(408, 24)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(72, 16)
        Me.Label1.TabIndex = 19
        Me.Label1.Text = "Label1"
        '
        'printButton
        '
        Me.printButton.Location = New System.Drawing.Point(248, 16)
        Me.printButton.Name = "printButton"
        Me.printButton.TabIndex = 18
        Me.printButton.Text = "Print"
        '
        'linkLabel1
        '
        Me.linkLabel1.Location = New System.Drawing.Point(456, 8)
        Me.linkLabel1.Name = "linkLabel1"
        Me.linkLabel1.Size = New System.Drawing.Size(32, 23)
        Me.linkLabel1.TabIndex = 17
        Me.linkLabel1.TabStop = True
        Me.linkLabel1.Text = "ScPl"
        '
        'clear
        '
        Me.clear.Location = New System.Drawing.Point(168, 16)
        Me.clear.Name = "clear"
        Me.clear.TabIndex = 16
        Me.clear.Text = "Clear"
        '
        'quit
        '
        Me.quit.Location = New System.Drawing.Point(328, 16)
        Me.quit.Name = "quit"
        Me.quit.TabIndex = 15
        Me.quit.Text = "Quit"
        '
        'nextPlot
        '
        Me.nextPlot.Location = New System.Drawing.Point(8, 16)
        Me.nextPlot.Name = "nextPlot"
        Me.nextPlot.TabIndex = 14
        Me.nextPlot.Text = "Next Plot"
        '
        'Panel2
        '
        Me.Panel2.Controls.AddRange(New System.Windows.Forms.Control() {Me.plotSurface})
        Me.Panel2.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Panel2.Name = "Panel2"
        Me.Panel2.Size = New System.Drawing.Size(496, 318)
        Me.Panel2.TabIndex = 15
        '
        'prevPlot
        '
        Me.prevPlot.Location = New System.Drawing.Point(88, 16)
        Me.prevPlot.Name = "prevPlot"
        Me.prevPlot.TabIndex = 20
        Me.prevPlot.Text = "Prev Plot"
        '
        'MainForm
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(496, 366)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.Panel2, Me.Panel1})
        Me.Name = "MainForm"
        Me.Text = "Plot Test (VB Code)"
        Me.Panel1.ResumeLayout(False)
        Me.Panel2.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

#Region "PlotWavelet"
    Private Sub PlotWavelet()
        With plotSurface
            .Clear()
            'Create a new line plot from array data via the ArrayAdapter class.
            Dim lp As New LinePlot(New ArrayAdapter(makeDaub(256)))
            lp.Color = Color.Green

            ' Add it to the plot Surface
            .Add(lp)
            .Title = "Daubechies Wavelet"
            ' Ok, the above will produce a decent default plot, but we would like to change
            ' some of the Y Axis details. First, we'd like lots of small ticks (10) between 
            ' large tick values. Secondly, we'd like to draw a grid for the Y values. To do 
            ' this, we create a new LinearAxis (we could also use Label, Log etc). Rather than
            ' starting from scratch, we use the constructor that takes an existing axis and
            ' clones it (values in the superclass Axis only are cloned). PlotSurface2D
            ' automatically determines a suitable axis when we add plots to it (merging
            ' current requirements with old requirements), and we use this as our starting
            ' point. Because we didn't specify which Y Axis we are using when we added the 
            ' above line plot (there is one on the left - YAxis1 and one on the right - YAxis2)
            ' PlotSurface2D.Add assumed we were using YAxis1. So, we create a new axis based on
            ' YAxis1, update the details we want, then set the YAxis1 to be our updated one.
            Dim lax As New LinearAxis(Me.plotSurface.YAxis1)
            lax.NumberSmallTicks = 10
            lax.GridDetail = Axis.GridType.Fine
            Me.plotSurface.YAxis1 = lax

            ' We would also like to modify the way in which the X Axis is printed. This time,
            ' we'll just modify the relevant PlotSurface2D Axis directly. 
            Me.plotSurface.XAxis1.GridDetail = Axis.GridType.Coarse
            Me.plotSurface.XAxis1.WorldMax = 100.0F

            Me.plotSurface.PlotBackColor = Color.Beige

            ' Force a re-draw the control. 
            .Refresh()

        End With
    End Sub
#Region "makeDaub"
    Public Function makeDaub(ByVal len As Integer) As Single()

        Dim daub4_h() As Single = {0.4829629F, 0.8365163F, 0.224143863F, -0.129409522F}
        Dim daub4_g() As Single = {-0.129409522F, -0.224143863F, 0.8365163F, -0.4829629F}

        Dim a(len) As Single
        a(8) = 1
        Dim t() As Single

        Dim ns As Integer = 4

        While (ns < len / 2)
            t = a.Clone()

            ns = ns * 2
            Dim i As Integer
            For i = 0 To ns * 2 - 1
                a(i) = 0.0F
            Next

            ' wavelet contribution
            For i = 0 To ns - 1
                Dim j As Integer
                For j = 0 To 3
                    a((2 * i + j) Mod (2 * ns)) = a((2 * i + j) Mod (2 * ns)) + daub4_g(j) * t(i + ns)
                Next j
            Next i

            ' smooth contribution
            For i = 0 To ns - 1
                Dim j As Integer
                For j = 0 To 3
                    a((2 * i + j) Mod (2 * ns)) = a((2 * i + j) Mod (2 * ns)) + daub4_h(j) * t(i)
                Next j
            Next i
        End While
        Return a
    End Function
#End Region
#End Region
#Region "PlotStep"
    Private Sub PlotStep()
        With plotSurface
            .Clear()
            Dim lp1 As New StepPlot(New SinusoidalAdapter(1.0F, Math.PI * 2, 0.0, 100, Math.PI * 2.0 / 200))
            lp1.Pen = New Pen(Color.Blue, 2.0)
            Dim lp2 As New StepPlot(New SinusoidalAdapter(1.0F, Math.PI * 2, Math.PI / 2, 100, Math.PI * 2.0 / 200))
            lp2.Pen = New Pen(Color.Red, 3.0)
            .Add(lp1)
            Dim fontFamily As New FontFamily("Comic Sans MS")
            Dim f As New Font(fontFamily, 16, FontStyle.Bold, GraphicsUnit.Pixel)
            .Title = "Step Plots"
            .TitleFont = f
            .Add(lp2)
            .Refresh()
        End With
    End Sub
#End Region
#Region "PlotLogAxis"
    Private Sub PlotLogAxis()
        plotSurface.Clear()

        Dim npt As Integer = 100

        Dim x(npt) As Single
        Dim y(npt) As Single

        Dim stp As Single = 0.1F

        Dim i As Integer
        For i = 0 To npt
            x(i) = i * stp - 5.0F
            y(i) = Math.Pow(10, x(i))
        Next i
        Dim xmin As Single = x(0)
        Dim xmax As Single = x(npt)
        Dim ymin As Single = Math.Pow(10.0, xmin)
        Dim ymax As Single = Math.Pow(10.0, xmax)
        Dim lp As New LinePlot(New ArrayAdapter(x, y))
        Dim p As New Pen(Color.Red)
        lp.Pen = p
        plotSurface.Add(lp)
        Dim loga As New LogAxis(plotSurface.YAxis1)
        loga.WorldMin = ymin
        loga.WorldMax = ymax
        loga.LinePen = p
        Dim Red As New SolidBrush(Color.Red)
        loga.LabelBrush = Red
        loga.TickTextBrush = Red
        loga.GridDetail = Axis.GridType.Fine
        '			loga.LargeTickValue=1.0F
        loga.LargeTickStep = 1.0F
        loga.Label = "10^x"
        plotSurface.YAxis1 = loga

        Dim lp1 As New LinePlot(New ArrayAdapter(x, y))
        Dim p1 As New Pen(Color.Blue)
        lp1.Pen = p1
        plotSurface.Add(lp1, XAxisPosition.Bottom, YAxisPosition.Right)
        Dim lin As New LinearAxis(plotSurface.YAxis2)
        lin.WorldMin = ymin
        lin.WorldMax = ymax
        lin.LinePen = p1
        Dim Blue As New SolidBrush(Color.Blue)
        lin.LabelBrush = Blue
        lin.TickTextBrush = Blue
        lin.GridDetail = Axis.GridType.None
        lin.Label = "10^x"
        plotSurface.YAxis2 = lin

        Dim lx As LinearAxis
        lx = plotSurface.XAxis1
        lx.WorldMin = xmin
        lx.WorldMax = xmax
        lx.GridDetail = Axis.GridType.Fine
        lx.Label = "x"

        plotSurface.Title = "Mixed Linear/Log Axes"
        plotSurface.Refresh()
    End Sub
#End Region
#Region "PlotLogLog"
    Private Sub PlotLogLog()
        ' log log plot
        plotSurface.Clear()

        Dim npt As Integer = 100
        Dim x(npt) As Single
        Dim y(npt) As Single

        Dim stp As Single = 0.1F
        ' plot a power law on the log-log scale
        Dim i As Integer
        For i = 0 To npt
            x(i) = (i + 1) * stp
            y(i) = x(i) * x(i)
        Next i

        Dim xmin As Single = x(0)
        Dim xmax As Single = x(npt - 1)
        Dim ymin As Single = y(0)
        Dim ymax As Single = y(npt - 1)

        Dim lp As New LinePlot(New ArrayAdapter(x, y))
        Dim p As New Pen(Color.Red)
        lp.Pen = p
        plotSurface.Add(lp)
        ' axes
        ' x axis
        Dim Red As New SolidBrush(Color.Red)
        Dim logax As LogAxis = New LogAxis(plotSurface.XAxis1)
        logax.WorldMin = xmin
        logax.WorldMax = xmax
        logax.LinePen = p
        logax.LabelBrush = Red
        logax.TickTextBrush = Red
        logax.GridDetail = Axis.GridType.Fine
        logax.LargeTickStep = 1.0F
        logax.Label = "x"
        plotSurface.XAxis1 = logax
        ' y axis
        Dim logay As LogAxis = New LogAxis(plotSurface.YAxis1)
        logay.WorldMin = ymin
        logay.WorldMax = ymax
        logay.LinePen = p
        logay.LabelBrush = Red
        logay.TickTextBrush = Red
        logay.GridDetail = Axis.GridType.Fine
        logay.LargeTickStep = 1.0F
        logay.Label = "x^2"
        plotSurface.YAxis1 = logay

        Dim lp1 As LinePlot = New LinePlot(New ArrayAdapter(x, y))
        Dim p1 As New Pen(Color.Blue)
        lp1.Pen = p1
        Dim Blue As New SolidBrush(Color.Blue)
        plotSurface.Add(lp1, XAxisPosition.Top, YAxisPosition.Right)
        ' axes
        ' x axis (lin)
        Dim linx As LinearAxis = DirectCast(plotSurface.XAxis2, LinearAxis)
        linx.WorldMin = xmin
        linx.WorldMax = xmax
        linx.LinePen = p1
        linx.LabelBrush = Blue
        linx.TickTextBrush = Blue
        linx.GridDetail = Axis.GridType.None
        linx.Label = "x"
        plotSurface.XAxis2 = linx
        ' y axis (lin)
        Dim liny As LinearAxis = DirectCast(plotSurface.YAxis2, LinearAxis)
        liny.WorldMin = ymin
        liny.WorldMax = ymax
        liny.LinePen = p1
        liny.LabelBrush = Blue
        liny.TickTextBrush = Blue
        liny.GridDetail = Axis.GridType.None
        liny.Label = "x^2"
        plotSurface.YAxis2 = liny

        plotSurface.Title = "x^2 plotted with log(red)/linear(blue) axes"

        plotSurface.Refresh()
    End Sub
#End Region
#Region "PlotSincFunction"
    Private Sub PlotSincFunction()
        ' clear everything. reset fonts. remove plot components etc.
        plotSurface.Clear()

        Dim r As New Random()
        Dim a(100) As Single
        Dim b(100) As Single
        Dim mult As Single = 0.00001F
        Dim i As Integer
        For i = 0 To 99
            a(i) = (r.Next(1000) / 5000.0F - 0.1F) * mult
            If (i = 50) Then
                b(i) = mult
            Else
                b(i) = Math.Sin((i - 50.0F) / 4.0F) / ((i - 50.0F) / 4.0F)
                b(i) *= mult
            End If
            a(i) += b(i)
        Next i

        Dim pp As New PointPlot(New ArrayAdapter(a, -500.0F, 10.0F), New Marker(MarkerType.Cross1, 6, New Pen(Color.Blue, 2.0)))

        With plotSurface
            .Add(pp)

            Dim lp As New LinePlot(New ArrayAdapter(b, -500, 10))
            lp.Pen = New Pen(Color.Red, 2.0)
            .Add(lp)

            .Title = "Sinc() function"
            .YAxis1.Label = "Magnitude"
            .XAxis1.Label = "Position"
            .Refresh()
        End With
    End Sub
#End Region
#Region "PlotGaussian"
    Private Sub PlotGaussian()
        plotSurface.Clear()

        Dim r As New Random()

        Dim len As Integer = 40
        Dim a(len) As Single
        Dim b(len) As Single
        Dim i As Integer

        For i = 0 To len - 1
            a(i) = Math.Exp(-Math.Pow((i - len / 2), 2) / 50.0F)
            b(i) = a(i) + r.Next(10) / 50.0F - 0.05F
            If b(i) < 0.0F Then
                b(i) = 0.0F
            End If
        Next i
        Dim hp As New HistogramPlot(New ArrayAdapter(b))
        Dim lp As New LinePlot(New ArrayAdapter(a))
        lp.Pen = New Pen(Color.Blue, 3)
        With plotSurface
            .Add(hp)
            .Add(lp)
            .YAxis1.WorldMin = 0
            .Title = "Histogram Plot"
            .Refresh()
        End With
    End Sub
#End Region
#Region "DashPlot"
    Private Sub DashPlot()
        plotSurface.Clear()
        Const size As Integer = 80
        Dim xs(size) As Single
        Dim ys(size) As Single
        Dim i As Integer
        For i = 0 To size
            xs(i) = Math.Sin(Convert.ToSingle(i) / (size - 1) * 2.0F * Math.PI)
            ys(i) = Math.Cos(Convert.ToSingle(i) / (size - 1) * 6.0F * Math.PI)
        Next i
        Dim lp As New LinePlot(New ArrayAdapter(xs, ys))

        Dim linePen As New Pen(Color.Green, 1.0F)
        Dim pattern() As Single = {8.0F, 8.0F}
        linePen.DashPattern = pattern
        lp.Pen = linePen
        With plotSurface
            .Add(lp)
            .Title = "Dash line"
            .Refresh()
        End With
    End Sub
#End Region
#Region "PlotLabelAxis"
    Private Sub PlotLabelAxis()
        plotSurface.Clear()

        Dim xs() As Single = {13.0F, 31.0F, 27.0F, 38.0F, 24.0F, 3.0F, 2.0F}
        Dim hp As New HistogramPlot(New ArrayAdapter(xs))
        plotSurface.Add(hp)

        Dim la As New LabelAxis(plotSurface.XAxis1)
        With la
            .AddLabel("Monday", 0)
            .AddLabel("Tuesday", 1)
            .AddLabel("Wednesday", 2)
            .AddLabel("Thursday", 3)
            .AddLabel("Friday", 4)
            .AddLabel("Saturday", 5)
            .AddLabel("Sunday", 6)
        End With
        With plotSurface
            .XAxis1 = la
            .XAxis1.LargeTickSize = 0
            .YAxis1.WorldMin = 0
            .YAxis1.GridDetail = Axis.GridType.Coarse
            .YAxis1.Label = "MBytes"
            Dim lina As LinearAxis = .YAxis1
            lina.NumberSmallTicks = 1

            Dim majorGridPen As New Pen(Color.LightGray)
            Dim pattern() As Single = {1.0F, 2.0F}
            majorGridPen.DashPattern = pattern
            .MajorGridPen = majorGridPen

            .Title = "Internet useage for user johnc 09/01/03 - 09/07/03"

            .Refresh()
        End With
    End Sub
#End Region
#Region "PlotParticles"
    Private Sub PlotParticles()
        plotSurface.Clear()

        Const Particle_Number = 5000
        Dim x(Particle_Number) As Single
        Dim y(Particle_Number) As Single
        ' Twiss parameters for the beam ellipse
        ' 5 mm mrad max emittance, 1 mm beta function
        Dim alpha, beta, gamma, emit As Single
        alpha = -2.0F
        beta = 1.0F
        gamma = (1.0 + alpha * alpha) / beta
        emit = 4.0F
        Dim da, xmax, xpmax As Single
        da = -alpha / gamma
        xmax = Math.Sqrt(emit / gamma)
        xpmax = Math.Sqrt(emit * gamma)
        '
        Dim i As Integer
        Dim rand As New Random()

        ' cheap randomizer on the unit circle
        For i = 0 To Particle_Number
            Dim r As Single
            Do
                x(i) = 2.0F * rand.NextDouble - 1.0F
                y(i) = 2.0F * rand.NextDouble - 1.0F
                r = Math.Sqrt(x(i) * x(i) + y(i) * y(i))
            Loop While r > 1.0F
        Next i
        ' transform to the tilted twiss ellipse
        For i = 0 To Particle_Number
            y(i) *= xpmax
            x(i) = x(i) * xmax + y(i) * da
        Next

        plotSurface.Title = "Beam Horizontal Phase Space and Twiss ellipse"

        Dim pp As New PointPlot(New ArrayAdapter(x, y), New Marker(MarkerType.FilledCircle, 4, New Pen(Color.Blue)))
        plotSurface.Add(pp, XAxisPosition.Bottom, YAxisPosition.Left)

        ' set axes
        Dim lx As LinearAxis = plotSurface.XAxis1
        lx.GridDetail = Axis.GridType.Fine
        lx.Label = "Position - x [mm]"
        lx.NumberSmallTicks = 9
        Dim ly As LinearAxis = plotSurface.YAxis1
        ly.GridDetail = Axis.GridType.Fine
        ly.Label = "Divergence - x' [mrad]"
        ly.NumberSmallTicks = 9

        ' Draws the rms Twiss ellipse computed from the random data
        Dim xeli(40), yeli(40) As Single
        Dim a_rms, b_rms, g_rms, e_rms As Single
        Twiss(x, y, a_rms, b_rms, g_rms, e_rms)
        TwissEllipse(a_rms, b_rms, g_rms, e_rms, xeli, yeli)
        Dim lp As New LinePlot(New ArrayAdapter(xeli, yeli))
        plotSurface.Add(lp, XAxisPosition.Bottom, YAxisPosition.Left)
        lp.Pen = New Pen(Color.Red, 2.0)
        ' Draws the ellipse containing 100% of the particles
        ' for a uniform distribution in 2D the area is 4 times the rms
        Dim xeli2(40), yeli2(40) As Single
        TwissEllipse(a_rms, b_rms, g_rms, 4.0 * e_rms, xeli2, yeli2)
        Dim lp2 As New LinePlot(New ArrayAdapter(xeli2, yeli2))
        plotSurface.Add(lp2, XAxisPosition.Bottom, YAxisPosition.Left)
        Dim p2 As New Pen(Color.Red, 2.0)
        Dim pattern() As Single = {5.0, 40.0}
        p2.DashPattern = pattern
        lp2.Pen = p2

        ' now bin the particle position to create beam density histogram
        Dim range, min, max As Single
        min = lx.WorldMin
        max = lx.WorldMax
        range = max - min
        Const Nbin = 30
        Dim dx As Single = range / Nbin
        Dim xbin(Nbin) As Single
        Dim xh(Nbin - 1) As Single
        Dim j As Integer
        For j = 0 To Nbin
            xbin(j) = min + j * range
            If j < Nbin Then xh(j) = 0.0
        Next j
        For i = 0 To Particle_Number
            If x(i) >= min AndAlso x(i) <= max Then
                j = Convert.ToInt32(Nbin * (x(i) - min) / range)
                xh(j) += 1
            End If
        Next i
        Dim sp As New StepPlot(New ArrayAdapter(xh, min, range / Nbin))
        sp.Center = True
        plotSurface.Add(sp, XAxisPosition.Bottom, YAxisPosition.Right)
        ' axis formatting
        Dim ly2 As LinearAxis = plotSurface.YAxis2
        ly2.WorldMin = 0.0F
        ly2.GridDetail = Axis.GridType.None
        ly2.Label = "Beam Density [a.u.]"
        ly2.NumberSmallTicks = 9
        sp.Pen = New Pen(Color.Green, 2)

        ' Finally, refreshes the plot
        plotSurface.Refresh()
    End Sub
    ' Fill the array containing the rms twiss ellipse data points
    ' ellipse is g*x^2+a*x*y+b*y^2=e
    Private Sub TwissEllipse(ByVal a As Single, ByVal b As Single, ByVal g As Single, ByVal e As Single, ByRef x() As Single, ByRef y() As Single)
        Dim rot, sr, cr, brot As Single
        If a = 0 Then
            rot = 0
        Else
            rot = 0.5 * Math.Atan(2.0 * a / (g - b))
        End If
        sr = Math.Sin(rot)
        cr = Math.Cos(rot)
        brot = g * sr * sr - 2.0 * a * sr * cr + b * cr * cr
        Dim npt As Integer = x.Length - 1
        Dim i As Integer
        Dim theta As Single
        For i = 0 To npt
            Dim xr, yr As Single
            theta = i * 2.0 * Math.PI / npt
            xr = Math.Sqrt(e * brot) * Math.Cos(theta)
            yr = Math.Sqrt(e / brot) * Math.Sin(theta)
            x(i) = xr * cr - yr * sr
            y(i) = xr * sr + yr * cr
        Next i
    End Sub
    ' Evaluates the rms Twiss parameters from the particle coordinates
    Private Sub Twiss(ByVal x() As Single, ByVal y() As Single, ByRef a As Single, ByRef b As Single, ByRef g As Single, ByRef e As Single)
        Dim xave, xsqave, yave, ysqave, xyave As Single
        Dim sigmaxsq, sigmaysq, sigmaxy As Single
        Dim Npoints As Integer = x.Length - 1
        xave = 0
        yave = 0
        Dim i As Integer
        For i = 0 To Npoints
            xave += x(i)
            yave += y(i)
        Next
        xave /= Npoints
        yave /= Npoints
        xsqave = 0
        ysqave = 0
        xyave = 0
        For i = 0 To Npoints
            xsqave += x(i) * x(i)
            ysqave += y(i) * y(i)
            xyave += x(i) * y(i)
        Next
        xsqave /= Npoints
        ysqave /= Npoints
        xyave /= Npoints
        sigmaxsq = xsqave - xave * xave
        sigmaysq = ysqave - yave * yave
        sigmaxy = xyave - xave * yave
        ' Now evaluates rms Twiss parameters
        e = Math.Sqrt(sigmaxsq * sigmaysq - sigmaxy * sigmaxy)
        a = -sigmaxy / e
        b = sigmaxsq / e
        g = (1.0F + a * a) / b
    End Sub
#End Region

    ' Handler for the Click event of the Clear Button
    Private Sub clear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles clear.Click
        ' sets up a new plot
        plotSurface.Clear()
        ' repaints the control
        plotSurface.Refresh()
    End Sub

    ' Handler for the Click event of the LinkLabel 
    Private Sub linkLabel1_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles linkLabel1.LinkClicked
        System.Diagnostics.Process.Start("http://www.netcontrols.org/scpl")
    End Sub

    ' Handler for the Click event of the Print Button
    Private Sub printButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles printButton.Click
        ' Instances a PrintDocument and PageSettings object
        PrintDocument1 = New PrintDocument()
        Dim pgSettings_ As New PageSettings()
        PrintDialog1.Document = PrintDocument1
        If PrintDialog1.ShowDialog() = DialogResult.OK Then
            Try
                PrintDocument1.Print()
            Catch
                Console.WriteLine("Error Printing")
            End Try
        End If
    End Sub

    ' Handler for the PrintPage event 
    ' Printing from the control is easy, drop a printdocument object on the page, 
    ' select it and the PrintPage event on the VS list boxes on the top of the code editor
    ' and fill the handler routine with the code to print the object. 
    ' A straightforward routine is implemented below
    Private Sub PrintDocument1_PrintPage(ByVal sender As Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDocument1.PrintPage
        ' The control can be rendered to any Graphics surface, here we use the Graphics
        ' object referenced by the PrintPage event (i.e. the printer graphics surface).
        plotSurface.Draw(e.Graphics, e.MarginBounds)
        e.HasMorePages = False
    End Sub

    ' Handler for the Click event of the quit Button
    Private Sub quit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles quit.Click
        Application.Exit()
    End Sub

    ' Handler for the Click event of the nextplot Button
    Private Sub nextPlot_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles nextPlot.Click
        currentPlot += 1
        If currentPlot = PlotRoutines.GetUpperBound(0) + 1 Then currentPlot = 0
        Dim id As Integer = currentPlot + 1
        Label1.Text = "Plot " & id.ToString("0") & "/" & PlotRoutines.Length.ToString("0")
        PlotRoutines(currentPlot)()
    End Sub
    Private Sub prevPlot_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles prevPlot.Click
        currentPlot -= 1
        If currentPlot = PlotRoutines.GetLowerBound(0) - 1 Then currentPlot = PlotRoutines.GetUpperBound(0)
        Dim id As Integer = currentPlot + 1
        Label1.Text = "Plot " & id.ToString("0") & "/" & PlotRoutines.Length.ToString("0")
        PlotRoutines(currentPlot)()
    End Sub

    Private Sub Panel1_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Panel1.Paint

    End Sub

End Class
