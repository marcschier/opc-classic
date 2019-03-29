/*
ScPl - A plotting library for .NET

MainForm.cs
Copyright (C) 2003 
Matt Howlett, Paolo Pierini

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
   
2. Redistributions in binary form must reproduce the following text in 
   the documentation and / or other materials provided with the 
   distribution: 
   
   "This product includes software developed as part of 
   the ScPl plotting library project available from: 
   http://www.netcontrols.org/scpl/" 

------------------------------------------------------------------------

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

$Id: XMLAdapter.cs,v 1.4 2003/10/16 01:57:16 mef526 Exp $

*/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Printing;
using scpl;

namespace TestScPl
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		// delegates to handle the next plot button
		private delegate void PlotDelegate();
        // list of the plot routines, to be initialized in the form constructor
		private PlotDelegate [] PlotRoutines;

		// Note that a scpl.Windows.PlotSurface2D class
		// is used here. This has exactly the same 
		// functionality as the scpl.PlotSurface2D 
		// class, except that it is derived from Forms.UserControl
		// and automatically paints itself in a windows.forms
		// application. Windows.PlotSurface2D can also paint itself
		// to other arbitrary Drawing.Graphics drawing surfaces
		// using the Draw method. (see printing later).
		private scpl.Windows.PlotSurface2D plotSurface;

		private System.Windows.Forms.Button quit;
		private System.Windows.Forms.Button clear;
		private System.Windows.Forms.Button nextPlot;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button printButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private PrintDocument printDocument_;
		private PageSettings pgSettings_; 

		private int currentPlot = 0;
		private System.Windows.Forms.Button prevPlot;
		private System.Windows.Forms.Label label1;

		#region PlotWavelet
		public void PlotWavelet()
		{	
			this.plotSurface.Clear();

			// Create a new line plot from array data via the ArrayAdapter class.
			LinePlot lp = new LinePlot( new ArrayAdapter( makeDaub(256) ) );
			lp.Color = Color.Green;

			// And add it to the plot surface
			this.plotSurface.Add( lp );
			this.plotSurface.Title = "Reversed / Upside down Daubechies Wavelet";

			// Ok, the above will produce a decent default plot, but we would like to change
			// some of the Y Axis details. First, we'd like lots of small ticks (10) between 
			// large tick values. Secondly, we'd like to draw a grid for the Y values. To do 
			// this, we create a new LinearAxis (we could also use Label, Log etc). Rather than
			// starting from scratch, we use the constructor that takes an existing axis and
			// clones it (values in the superclass Axis only are cloned). PlotSurface2D
			// automatically determines a suitable axis when we add plots to it (merging
			// current requirements with old requirements), and we use this as our starting
			// point. Because we didn't specify which Y Axis we are using when we added the 
			// above line plot (there is one on the left - YAxis1 and one on the right - YAxis2)
			// PlotSurface2D.Add assumed we were using YAxis1. So, we create a new axis based on
			// YAxis1, update the details we want, then set the YAxis1 to be our updated one.
			LinearAxis myAxis = new LinearAxis( this.plotSurface.YAxis1 );
			myAxis.NumberSmallTicks = 10;
			myAxis.GridDetail = Axis.GridType.Fine;
			this.plotSurface.YAxis1 = myAxis;
	
			// We would also like to modify the way in which the X Axis is printed. This time,
			// we'll just modify the relevant PlotSurface2D Axis directly. 
			this.plotSurface.XAxis1.GridDetail = Axis.GridType.Coarse;
			this.plotSurface.XAxis1.WorldMax = 100.0f;
		
			this.plotSurface.PlotBackColor = Color.Beige;
			this.plotSurface.XAxis1.Reversed = true;
			this.plotSurface.YAxis1.Reversed = true;
		
			// Force a re-draw the control. 
			this.plotSurface.Refresh();
		}
		#region makeDaub
		public float[] makeDaub( int len )
		{
			float[] daub4_h = 
			{ 0.482962913145f, 0.836516303737f, 0.224143868042f, -0.129409522551f };

			float[] daub4_g = 
			{ -0.129409522551f, -0.224143868042f, 0.836516303737f, -0.482962913145f };

			float[] a = new float[len];
			a[8] = 1.0f;
			float[] t;

			int ns = 4;  // number smooth
			while ( ns < len/2 ) 
			{
				t = (float[])a.Clone();

				ns *= 2;

				for ( int i=0; i<(ns*2); ++i ) 
				{
					a[i] = 0.0f;
				}

				// wavelet contribution
				for ( int i=0; i<ns; ++i ) 
				{
					for ( int j=0; j<4; ++j ) 
					{
						a[(2*i+j)%(2*ns)] += daub4_g[j] * t[i+ns];
					}
				}
				// smooth contribution
				for ( int i=0; i<ns; ++i ) 
				{
					for ( int j=0; j<4; ++j ) 
					{
						a[(2*i+j)%(2*ns)] += daub4_h[j]*t[i];
					}
				}
			}
			return a;
		}
		#endregion
		#endregion 
		#region PlotStep
		private void PlotStep()
		{
			plotSurface.Clear();
			StepPlot lp1 = new StepPlot( 
				new SinusoidalAdapter( 1.0, Math.PI * 2, 0.0, 100, (float)Math.PI * 2.0F / 200 ) );
			lp1.Label = "Sin";
			lp1.Pen = new Pen( Color.Blue, 2.0f );
			plotSurface.Add( lp1 );

			StepPlot lp2 = new StepPlot(
				new SinusoidalAdapter(1.0, Math.PI * 2, Math.PI / 2, 100, (float)Math.PI * 2.0F / 200) );
			lp2.Label = "Cos";
			lp2.Pen = new Pen( Color.Red, 3.0f );
			plotSurface.Add( lp2 );

			FontFamily fontFamily = new FontFamily("Comic Sans MS");
			Font f = new Font( fontFamily, 16, FontStyle.Bold, GraphicsUnit.Pixel );
            plotSurface.Title = "Step Plots";
			plotSurface.TitleFont = f;

			plotSurface.ShowLegend = true;

			plotSurface.Refresh();
		}
		#endregion
		#region PlotLogAxis
		public void PlotLogAxis()
		{
			plotSurface.Clear();

			int npt=101;
			float [] x = new float[npt];
			float [] y = new float[npt];

			float step=0.1F;
			for (int i=0; i<npt;i++)
			{
				x[i]=i*step-5.0F;
				y[i]=(float)Math.Pow(10.0,x[i]);
			}
			float xmin = x[0];
			float xmax = x[npt-1];
			float ymin = (float)Math.Pow(10.0,xmin);
			float ymax = (float)Math.Pow(10.0,xmax);

			LinePlot lp=new LinePlot(new ArrayAdapter(x,y));
			Pen p=new Pen(Color.Red);
			lp.Pen=p;
			plotSurface.Add(lp);
			LogAxis loga= new LogAxis(plotSurface.YAxis1);
			loga.WorldMin=ymin;
			loga.WorldMax=ymax;
			loga.LinePen=p;
			SolidBrush Red=new SolidBrush(Color.Red);
			loga.LabelBrush=Red;
			loga.TickTextBrush=Red;
			loga.GridDetail=Axis.GridType.Fine;
//			loga.LargeTickValue=1.0F;
			loga.LargeTickStep=1.0F;
			loga.Label="10^x";
			plotSurface.YAxis1=loga;

			LinePlot lp1=new LinePlot(new ArrayAdapter(x,y));
			Pen p1=new Pen(Color.Blue);
			lp1.Pen=p1;
			plotSurface.Add(lp1,XAxisPosition.Bottom,YAxisPosition.Right);
			LinearAxis lin= new LinearAxis(plotSurface.YAxis2);
			lin.WorldMin=ymin;
			lin.WorldMax=ymax;
			lin.LinePen=p1;
			SolidBrush Blue=new SolidBrush(Color.Blue);
			lin.LabelBrush=Blue;
			lin.TickTextBrush=Blue;
			lin.GridDetail=Axis.GridType.None;
			lin.Label="10^x";
			plotSurface.YAxis2=lin;
 
			LinearAxis lx=(LinearAxis)plotSurface.XAxis1;
			lx.WorldMin=xmin;
			lx.WorldMax=xmax;
			lx.GridDetail=Axis.GridType.Fine;
			lx.Label="x";

			plotSurface.Title="Mixed Linear/Log Axes";

			plotSurface.Refresh();
		}
		#endregion
		#region PlotLogLog
		public void PlotLogLog()
		{
			// log log plot
			plotSurface.Clear();

			int npt=101;
			float [] x = new float[npt];
			float [] y = new float[npt];

			float step=0.1F;
			// plot a power law on the log-log scale
			for (int i=0; i<npt;i++)
			{
				x[i]=(i+1)*step;
				y[i]=x[i]*x[i];
			}
			float xmin=x[0];
			float xmax=x[npt-1];
			float ymin=y[0];
			float ymax=y[npt-1];

			LinePlot lp=new LinePlot(new ArrayAdapter(x,y));
			Pen p=new Pen(Color.Red);
			lp.Pen=p;
			plotSurface.Add(lp);
			// axes
			// x axis
			SolidBrush Red=new SolidBrush(Color.Red);
			LogAxis logax= new LogAxis(plotSurface.XAxis1);
			logax.WorldMin=xmin;
			logax.WorldMax=xmax;
			logax.LinePen=p;
			logax.LabelBrush=Red;
			logax.TickTextBrush=Red;
			logax.GridDetail=Axis.GridType.Fine;
			logax.LargeTickStep=1.0F;
			logax.Label="x";
			plotSurface.XAxis1=logax;
			// y axis
			LogAxis logay= new LogAxis(plotSurface.YAxis1);
			logay.WorldMin=ymin;
			logay.WorldMax=ymax;
			logay.LinePen=p;
			logay.LabelBrush=Red;
			logay.TickTextBrush=Red;
			logay.GridDetail=Axis.GridType.Fine;
			logay.LargeTickStep=1.0F;
			logay.Label="x^2";
			plotSurface.YAxis1=logay;

            LinePlot lp1=new LinePlot(new ArrayAdapter(x,y));
			Pen p1=new Pen(Color.Blue);
			lp1.Pen=p1;
			SolidBrush Blue=new SolidBrush(Color.Blue);
			plotSurface.Add(lp1,XAxisPosition.Top,YAxisPosition.Right);
			// axes
			// x axis (lin)
			LinearAxis linx=(LinearAxis) plotSurface.XAxis2;
			linx.WorldMin=xmin;
			linx.WorldMax=xmax;
			linx.LinePen=p1;
			linx.LabelBrush=Blue;
			linx.TickTextBrush=Blue;
			linx.GridDetail=Axis.GridType.None;
			linx.Label="x";
			plotSurface.XAxis2=linx;
			// y axis (lin)
			LinearAxis liny= (LinearAxis) plotSurface.YAxis2;
			liny.WorldMin=ymin;
			liny.WorldMax=ymax;
			liny.LinePen=p1;
			liny.LabelBrush=Blue;
			liny.TickTextBrush=Blue;
			liny.GridDetail=Axis.GridType.None;
			liny.Label="x^2";
			plotSurface.YAxis2=liny;

			plotSurface.Title="x^2 plotted with log(red)/linear(blue) axes";

			plotSurface.Refresh();
		}
		#endregion

		#region PlotSincFunction
		private void PlotSincFunction() 
		{
			plotSurface.Clear(); // clear everything. reset fonts. remove plot components etc.

			System.Random r = new Random();
			double[] a = new double[100];
			double[] b = new double[100];
			double mult = 0.00001f;
			for( int i=0; i<100; ++i )  
			{
				a[i] = ((double)r.Next(1000)/5000.0f-0.1f)*mult;
				if (i == 50 ) { b[i] = 1.0f*mult; } 
				else
				{
					b[i] = (double)Math.Sin((((double)i-50.0f)/4.0f))/(((double)i-50.0f)/4.0f);
					b[i] *= mult;
				}
				a[i] += b[i];
			}
		
			Marker m= new Marker(MarkerType.Cross1,6,new Pen(Color.Blue,2.0F));
			PointPlot pp = new PointPlot( new ArrayAdapter( a, -500.0f, 10.0f ) , m ); 
			pp.Label = "Random";
			plotSurface.Add(pp); 

			LinePlot lp = new LinePlot( new ArrayAdapter( b, -500.0f, 10.0f ) );
			lp.Pen = new Pen(Color.Red,2.0F);
			plotSurface.Add(lp);

			plotSurface.Title = "Sinc Function";
			plotSurface.YAxis1.Label = "Magnitude";
			plotSurface.XAxis1.Label = "Position";

			plotSurface.ShowLegend = true;
			plotSurface.LegendAttachTo( XAxisPosition.Top, YAxisPosition.Left );
			plotSurface.LegendXOffset = 15.0f;
			plotSurface.LegendYOffset = 15.0f;
			plotSurface.VerticalEdgeLegendPlacement = scpl.Legend.Placement.Inside;
			plotSurface.HorizontalEdgeLegendPlacement = scpl.Legend.Placement.Inside;

			plotSurface.Refresh();
		}
		#endregion
		#region PlotGaussian
		public void PlotGaussian()
		{
			plotSurface.Clear();
	
			System.Random r = new Random();
			
			int len = 40;
			double[] a = new double[len];
			double[] b = new double[len];

			for (int i=0; i<len; ++i) 
			{
				int j = len-1-i;
				a[i] = (double)Math.Exp(-(double)(i-len/2)*(double)(i-len/2)/50.0f);
				b[i] = a[i] + (r.Next(10)/50.0f)-0.05f;
				if (b[i] < 0.0f) 
				{
					b[i] = 0;
				}
			}

			HistogramPlot sp = new HistogramPlot( new ArrayAdapter(b) );
			LinePlot lp = new LinePlot( new ArrayAdapter(a) );
			lp.Pen = new Pen(Color.Blue,3.0F);
			plotSurface.Add(sp);
			plotSurface.Add(lp);
			plotSurface.YAxis1.WorldMin = 0.0f;
			plotSurface.Title = "Histogram Plot";
			plotSurface.Refresh();
		}
		#endregion
		#region DashPlot
		public void DashPlot()
		{
			plotSurface.Clear();
			const int size=500;
			float [] xs=new float [size];
			float [] ys=new float [size];
			for (int i=0; i<size; i++)
			{
				xs[i]=(float)Math.Sin((double)i/(double)(size-1)*2.0*Math.PI);
				ys[i]=(float)Math.Cos((double)i/(double)(size-1)*6.0*Math.PI);
			}
			LinePlot lp=new LinePlot(new ArrayAdapter(xs,ys));
			Pen linePen=new Pen(Color.Green,1.0F);
			float [] pattern=new float[] {8.0F, 8.0F};
			linePen.DashPattern=pattern;
			lp.Pen=linePen;
			plotSurface.Add(lp);
			plotSurface.Title="Dash line";
			plotSurface.Refresh();
		}
		#endregion
		#region PlotLabelAxis
		public void PlotLabelAxis()
		{
			plotSurface.Clear();

			float[] xs = {13.0f, 31.0f, 27.0f, 38.0f, 24.0f, 3.0f, 2.0f };
			HistogramPlot hp = new HistogramPlot( new ArrayAdapter(xs) );
			plotSurface.Add(hp);

			LabelAxis la = new LabelAxis( plotSurface.XAxis1 );
			la.AddLabel( "Monday", 0.0f );
			la.AddLabel( "Tuesday", 1.0f );
			la.AddLabel( "Wednesday", 2.0f );
			la.AddLabel( "Thursday", 3.0f );
			la.AddLabel( "Friday", 4.0f );
			la.AddLabel( "Saturday", 5.0f );
			la.AddLabel( "Sunday", 6.0f );
			plotSurface.XAxis1 = la;
			plotSurface.XAxis1.LargeTickSize = 0.0f;
			plotSurface.YAxis1.WorldMin = 0.0f;
			plotSurface.YAxis1.GridDetail = Axis.GridType.Coarse;
			plotSurface.YAxis1.Label = "MBytes";
			((LinearAxis)plotSurface.YAxis1).NumberSmallTicks = 1;
			
			Pen majorGridPen = new Pen( Color.LightGray );
			float[] pattern = {1.0f,2.0f};
			majorGridPen.DashPattern = pattern;
			plotSurface.MajorGridPen = majorGridPen;

			plotSurface.Title = "Internet useage for user johnc 09/01/03 - 09/07/03";

			plotSurface.Refresh();
		}
		#endregion
		#region PlotParticles
		public void PlotParticles()
		{
			plotSurface.Clear();

			// in this example we synthetize a particle distribution
			// in the x-x' phase space and plot it, with the rms Twiss
			// ellipse and desnity distribution
			const int Particle_Number=5000;
			float [] x = new float[Particle_Number];
			float [] y = new float[Particle_Number];
			// Twiss parameters for the beam ellipse
			// 5 mm mrad max emittance, 1 mm beta function
			float alpha, beta, gamma, emit;
			alpha=-2.0F;
			beta=1.0F;
			gamma = (1.0F + alpha * alpha) / beta;
			emit = 4.0F;

			float da, xmax, xpmax;
			da = -alpha / gamma;
			xmax = (float)Math.Sqrt(emit / gamma);
			xpmax = (float)Math.Sqrt(emit * gamma);

			Random rand = new Random();

			// cheap randomizer on the unit circle
			for (int i = 0; i<Particle_Number; i++)
			{
				float r;
				do
				{
					x[i] = (float)(2.0F * rand.NextDouble() - 1.0F);
					y[i] = (float)(2.0F * rand.NextDouble() - 1.0F);
					r = (float)Math.Sqrt(x[i] * x[i] + y[i] * y[i]);
				} while (r>1.0F);
			}

			// transform to the tilted twiss ellipse
			for (int i =0; i<Particle_Number;i++)
			{
				y[i] *= xpmax;
				x[i] = x[i] * xmax + y[i] * da;
			}
			plotSurface.Title = "Beam Horizontal Phase Space and Twiss ellipse";

			PointPlot pp=new PointPlot(new ArrayAdapter(x, y), new Marker(MarkerType.FilledCircle ,4, new Pen(Color.Blue)));
			plotSurface.Add(pp, XAxisPosition.Bottom, YAxisPosition.Left);

			// set axes
			LinearAxis lx=(LinearAxis) plotSurface.XAxis1;
			lx.GridDetail = Axis.GridType.Fine;
			lx.Label = "Position - x [mm]";
			lx.NumberSmallTicks = 9;
			LinearAxis ly = (LinearAxis) plotSurface.YAxis1;
			ly.GridDetail = Axis.GridType.Fine;
			ly.Label = "Divergence - x' [mrad]";
			ly.NumberSmallTicks = 9;
			
			// Draws the rms Twiss ellipse computed from the random data
			float [] xeli=new float [40];
			float [] yeli=new float [40];

			float a_rms, b_rms, g_rms, e_rms;

			Twiss(x, y, out a_rms, out b_rms, out g_rms, out e_rms);
			TwissEllipse(a_rms, b_rms, g_rms, e_rms, ref xeli, ref yeli);

			LinePlot lp = new LinePlot(new ArrayAdapter(xeli, yeli));
			plotSurface.Add(lp, XAxisPosition.Bottom, YAxisPosition.Left);
			lp.Pen = new Pen(Color.Red, 2.0F);
			// Draws the ellipse containing 100% of the particles
			// for a uniform distribution in 2D the area is 4 times the rms
			float [] xeli2 = new float [40];
			float [] yeli2 = new float [40];
			TwissEllipse(a_rms, b_rms, g_rms, 4.0F * e_rms, ref xeli2, ref yeli2);

			LinePlot lp2 = new LinePlot(new ArrayAdapter(xeli2, yeli2));
			plotSurface.Add(lp2, XAxisPosition.Bottom, YAxisPosition.Left);
			Pen p2 = new Pen(Color.Red, 2.0F);
			float [] pattern = {5.0F, 40.0F};
			p2.DashPattern = pattern;
			lp2.Pen = p2;

			// now bin the particle position to create beam density histogram
			float range, min, max;
			min = (float)lx.WorldMin;
			max = (float)lx.WorldMax;
			range = max - min;

			const int Nbin = 30;
			float dx = range / Nbin;
			float [] xbin= new float[Nbin+1];
			float [] xh = new float[Nbin];

			for (int j=0; j<=Nbin;j++)
			{
				xbin[j] = min + j * range;
				if (j < Nbin) xh[j] = 0.0F;
			}
			for (int i =0; i<Particle_Number; i++)
			{
				if (x[i] >= min && x[i] <= max)
				{
					int j;
					j = Convert.ToInt32(Nbin * (x[i] - min) / range);
					xh[j] += 1;
				}
			}
			StepPlot sp= new StepPlot(new ArrayAdapter(xh, min, range / Nbin));
			sp.Center = true;
			plotSurface.Add(sp, XAxisPosition.Bottom, YAxisPosition.Right);
			// axis formatting
			LinearAxis ly2 = (LinearAxis)plotSurface.YAxis2;
			ly2.WorldMin = 0.0F;
			ly2.GridDetail = Axis.GridType.None;
			ly2.Label = "Beam Density [a.u.]";
			ly2.NumberSmallTicks = 9;
			sp.Pen = new Pen(Color.Green, 2);

			// Finally, refreshes the plot
			plotSurface.Refresh();
		}

	    // Fill the array containing the rms twiss ellipse data points
		// ellipse is g*x^2+a*x*y+b*y^2=e
		private void TwissEllipse(float a, float b, float g, float e, ref float [] x, ref float [] y)
		{
			float rot, sr, cr, brot;
			if (a==0) 
			{
				rot=0;
			}
			else
			{
				rot=(float)(.5*Math.Atan(2.0 * a / (g - b)));
			}
			sr = (float)Math.Sin(rot);
			cr = (float)Math.Cos(rot);
			brot = g * sr * sr - 2.0F * a * sr * cr + b * cr * cr;
			int npt=x.Length;
			float theta;
		
			for (int i=0; i<npt;i++)
			{
				float xr,yr;
				theta = i * 2.0F * (float)Math.PI / (npt-1);
				xr = (float)(Math.Sqrt(e * brot) * Math.Cos(theta));
				yr = (float)(Math.Sqrt(e / brot) * Math.Sin(theta));
				x[i] = xr * cr - yr * sr;
				y[i] = xr * sr + yr * cr;
			}
		}
		// Evaluates the rms Twiss parameters from the particle coordinates
		private void Twiss(float [] x, float [] y, out float a, out float b, out float g, out float e)
		{
			float xave, xsqave, yave, ysqave, xyave;
			float sigmaxsq, sigmaysq, sigmaxy;
			int Npoints= x.Length;
			xave = 0;
			yave = 0;
			for (int i=0; i<Npoints;i++)
			{
				xave += x[i];
				yave += y[i];
			}
			xave /= Npoints;
			yave /= Npoints;
			xsqave = 0;
			ysqave = 0;
			xyave = 0;
			for (int i=0;i<Npoints;i++)
			{
				xsqave += x[i] * x[i];
				ysqave += y[i] * y[i];
				xyave += x[i] * y[i];
			}
			xsqave /= Npoints;
			ysqave /= Npoints;
			xyave /= Npoints;
			sigmaxsq = xsqave - xave * xave;
			sigmaysq = ysqave - yave * yave;
			sigmaxy = xyave - xave * yave;
			// Now evaluates rms Twiss parameters
			e = (float)Math.Sqrt(sigmaxsq * sigmaysq - sigmaxy * sigmaxy);
			a = -sigmaxy / e;
			b = sigmaxsq / e;
			g = (1.0F + a * a) / b;
		}
		#endregion

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// List here the plot routines that you want to be accessed
			PlotRoutines = new PlotDelegate [] { new PlotDelegate(PlotWavelet), 
												   new PlotDelegate(PlotLogAxis),
												   new PlotDelegate(PlotLogLog),
												   new PlotDelegate(PlotSincFunction), 
												   new PlotDelegate(PlotGaussian),
												   new PlotDelegate(PlotStep), 
												   new PlotDelegate(PlotLabelAxis),
												   new PlotDelegate(PlotParticles), 
												   new PlotDelegate(DashPlot)};

			this.Resize += new System.EventHandler(this.ResizeHandler);
			currentPlot = 0;
			plotSurface.BackColor = Color.White;

			// set up printer
			pgSettings_ = new PageSettings();
			printDocument_ = new PrintDocument();
			//printDocument_.DefaultPageSettings = pgSettings_; // not 100% necessary.
			printDocument_.PrintPage += new PrintPageEventHandler(pd_PrintPage);
            int id=currentPlot+1;
			label1.Text="Plot " + id.ToString("0") + "/" + PlotRoutines.Length.ToString("0");
			PlotRoutines[0]();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.plotSurface = new scpl.Windows.PlotSurface2D();
			this.quit = new System.Windows.Forms.Button();
			this.nextPlot = new System.Windows.Forms.Button();
			this.clear = new System.Windows.Forms.Button();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.printButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.prevPlot = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// plotSurface
			// 
			this.plotSurface.AllowSelection = true;
			this.plotSurface.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.plotSurface.Name = "plotSurface";
			this.plotSurface.Padding = 10;
			this.plotSurface.Size = new System.Drawing.Size(496, 320);
			this.plotSurface.TabIndex = 8;
			this.plotSurface.Title = "";
			this.plotSurface.TitleFont = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
			this.plotSurface.XAxis1 = null;
			this.plotSurface.XAxis2 = null;
			this.plotSurface.YAxis1 = null;
			this.plotSurface.YAxis2 = null;
			// 
			// quit
			// 
			this.quit.Location = new System.Drawing.Point(328, 336);
			this.quit.Name = "quit";
			this.quit.TabIndex = 3;
			this.quit.Text = "Quit";
			this.quit.Click += new System.EventHandler(this.quit_Click);
			// 
			// nextPlot
			// 
			this.nextPlot.Location = new System.Drawing.Point(8, 336);
			this.nextPlot.Name = "nextPlot";
			this.nextPlot.TabIndex = 4;
			this.nextPlot.Text = "Next Plot";
			this.nextPlot.Click += new System.EventHandler(this.nextPlot_Click);
			// 
			// clear
			// 
			this.clear.Location = new System.Drawing.Point(168, 336);
			this.clear.Name = "clear";
			this.clear.TabIndex = 5;
			this.clear.Text = "Clear";
			this.clear.Click += new System.EventHandler(this.clear_Click);
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(456, 328);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(32, 23);
			this.linkLabel1.TabIndex = 7;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "ScPl";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// printButton
			// 
			this.printButton.Location = new System.Drawing.Point(248, 336);
			this.printButton.Name = "printButton";
			this.printButton.TabIndex = 9;
			this.printButton.Text = "Print";
			this.printButton.Click += new System.EventHandler(this.printButton_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(408, 344);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 10;
			this.label1.Text = "label1";
			// 
			// prevPlot
			// 
			this.prevPlot.Location = new System.Drawing.Point(88, 336);
			this.prevPlot.Name = "prevPlot";
			this.prevPlot.TabIndex = 12;
			this.prevPlot.Text = "Prev Plot";
			this.prevPlot.Click += new System.EventHandler(this.prevPlot_Click);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(496, 366);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.prevPlot,
																		  this.label1,
																		  this.printButton,
																		  this.linkLabel1,
																		  this.clear,
																		  this.nextPlot,
																		  this.quit,
																		  this.plotSurface});
			this.Name = "MainForm";
			this.Text = "Plot Test";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

		}
		#endregion


		// The PrintPage event is raised for each page to be printed.
		private void pd_PrintPage(object sender, PrintPageEventArgs ev) 
		{
			// ok - the windows.forms PlotSurface2D control can also be 
			// rendered to other Graphics surfaces. Here we output to a
			// printer. 
			plotSurface.Draw(ev.Graphics, ev.MarginBounds);
			ev.HasMorePages = false;
		}

		private void quit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}

		#region ResizeHandler
		private void ResizeHandler(object sender, System.EventArgs e)
		{
			plotSurface.Width = this.Width - 8;
			plotSurface.Height = this.Height - 80;
			linkLabel1.Left = this.Width - 48;
			linkLabel1.Top = this.Height - 72;
			nextPlot.Top = this.Height - 64;
			prevPlot.Top = this.Height - 64;
			clear.Top = this.Height - 64;
			printButton.Top = this.Height - 64;
			quit.Top = this.Height - 64;
			label1.Top=this.Height-56;
		}
		#endregion

		private void nextPlot_Click(object sender, System.EventArgs e)
		{
			currentPlot++;
			if( currentPlot == PlotRoutines.Length ) currentPlot = 0;
			int id=currentPlot+1;
			label1.Text = "Plot " + id.ToString("0") + "/" + PlotRoutines.Length.ToString("0");
			PlotRoutines[currentPlot]();
		}

		private void clear_Click(object sender, System.EventArgs e)
		{
			// sets up new plot.
			plotSurface.Clear();
			// paints the window.
			plotSurface.Refresh();
		}

		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("www.netcontrols.org/scpl");
		}

		private void printButton_Click(object sender, System.EventArgs e)
		{
			PrintDialog dlg = new PrintDialog();
			dlg.Document = printDocument_;
			if (dlg.ShowDialog() == DialogResult.OK) 
			{
				try
				{
					printDocument_.Print();
				}
				catch
				{
					Console.WriteLine( "caught\n" );
				}
			}	
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			System.DateTime dt = DateTime.Now;
			System.Console.WriteLine( "{0}", dt.Ticks.ToString() );	
			Application.Run(new MainForm());
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
		}

		private void prevPlot_Click(object sender, System.EventArgs e)
		{
			currentPlot--;
			if( currentPlot == -1 ) currentPlot = PlotRoutines.Length-1;
			int id=currentPlot+1;
			label1.Text = "Plot " + id.ToString("0") + "/" + PlotRoutines.Length.ToString("0");
			PlotRoutines[currentPlot]();
	
		}
	}
}
