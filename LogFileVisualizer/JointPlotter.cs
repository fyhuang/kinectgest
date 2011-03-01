using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using FinalProject;
using GoogleChartSharp;

namespace LogFileVisualizer
{
	class FigureForm : Form {
		Bitmap mBitmap;
		const int BorderSize = 10;
		
		public FigureForm(string url)
		{
			var stream = WebRequest.Create(url).GetResponse().GetResponseStream();
			mBitmap = new Bitmap(stream);
			SetupForm();
		}
		
		void SetupForm() {
			this.ClientSize = new Size(mBitmap.Width + 2*BorderSize, mBitmap.Height + 2*BorderSize);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint(e);
			var g = e.Graphics;
			g.Clear(Color.DarkSlateBlue);
			g.DrawImage(mBitmap, BorderSize, BorderSize,
			            ClientRectangle.Width - 2*BorderSize, ClientRectangle.Height - 2*BorderSize);
		}
	}
	
	public class JointPlotter
	{
		Gesture mGesture;
		string mJointName;
		
		public JointPlotter(Gesture g, string jointname)
		{
			mGesture = g;
			mJointName = jointname;
		}
		
		public void DisplayPlot()
		{
			string url = BuildPlotURL();
			Application.Run(new FigureForm(url));
		}
		
		string BuildPlotURL()
		{
			var chart = new LineChart(500, 400, LineChartType.MultiDataSet);
			int jointnum = JointState.NamesToJoints[mJointName.Trim().ToLower()];
			
			float[] data = mGesture.States.Select(x => x.RelativeJoints[jointnum].X).ToArray();
			float[] xcoords = mGesture.States.Select(x => x.Timestamp - mGesture.StartTime).ToArray();
			var datasets = new List<float[]>();
			datasets.Add(data);
			datasets.Add(xcoords);
			
			chart.SetData(datasets);
			
			var leftaxis = new ChartAxis(ChartAxisType.Left);
			leftaxis.SetRange((int)Math.Floor(data.Min()), (int)Math.Ceiling(data.Max()));
			chart.AddAxis(leftaxis);
			var botaxis = new ChartAxis(ChartAxisType.Bottom);
			botaxis.SetRange((int)Math.Floor(xcoords.Min()), (int)Math.Ceiling(xcoords.Max()));
			chart.AddAxis(botaxis);
			
			System.Console.WriteLine("Chart URL: {0}", chart.GetUrl());
			
			return chart.GetUrl();
		}
	}
}

