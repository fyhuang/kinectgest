using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;
using ZedGraph;

namespace LogFileVisualizer
{
	class BitmapFigure : Form {
		Bitmap mBitmap;
		const int BorderSize = 10;
		
		public BitmapFigure(Bitmap b)
		{
			mBitmap = b;
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
	
	class ZedGraphFigure : Form {
		const int BorderSize = 10;
		
		ZedGraphControl mZG;
		public ZedGraphFigure(ZedGraphControl zg) {
			this.ClientSize = new Size(500, 400);
			
			mZG = zg;
			mZG.Parent = this;
			mZG.Location = new Point(BorderSize, BorderSize);
			mZG.Size = new Size(ClientRectangle.Width - 2*BorderSize, ClientRectangle.Height - 2*BorderSize);
			
			mZG.AxisChange();
		}
	}
	
	public class JointPlotter
	{
		static Color[] GraphColors = new[]{Color.OrangeRed, Color.PaleVioletRed, Color.Navy, Color.ForestGreen};
		
		InputGesture mGesture;
		string mJointName;
		bool mPlotAngle;
		
		public JointPlotter(InputGesture g, string jointname, bool angle)
		{
			mGesture = g;
			mJointName = jointname;
			mPlotAngle = angle;
		}
		
		public Form DisplayPlots()
		{
			if ( mPlotAngle ) {
				var aplot = BuildAnglePlot();
				var zf1 = new ZedGraphFigure(aplot);
				zf1.Visible = true;
			}
			
			var pplot = BuildPositionPlot();
			var zf2 = new ZedGraphFigure(pplot);
			zf2.Visible = true;
			return zf2;
		}
		
		ZedGraphControl BuildAnglePlot()
		{
			int jointnum = JointState.NamesToJoints[mJointName.Trim().ToLower()];
			double[] timestamps = mGesture.States.Select(x => (double)(x.Timestamp - mGesture.StartTime)).ToArray();
			
			var zg = new ZedGraphControl();
			var gp = zg.GraphPane;
			
			gp.Title.Text = String.Format("Joint \"{0}\" angle", mJointName);
			gp.XAxis.Title.Text = "Time";
			
			double[] positions = mGesture.States.Select(x => (1.0 / Math.PI * 180.0) * (double)x.RelativeAngles[jointnum]).ToArray();
			gp.AddCurve("Angle", timestamps, positions, GraphColors[3]);
			
			return zg;
		}
		
		ZedGraphControl BuildPositionPlot()
		{
			int jointnum = JointState.NamesToJoints[mJointName.Trim().ToLower()];
			double[] timestamps = mGesture.States.Select(x => (double)(x.Timestamp - mGesture.StartTime)).ToArray();
			
			var zg = new ZedGraphControl();
			var gp = zg.GraphPane;
			
			gp.Title.Text = String.Format("Joint \"{0}\" position", mJointName);
			gp.XAxis.Title.Text = "Time";
			
			for ( int i = 0; i < 3; i++ ) {
				double[] positions = mGesture.States.Select(x => (double)x.RelativeJoints[jointnum].Comp(i)).ToArray();
				gp.AddCurve(String.Format("{0}", Utility.CompToChar(i)), timestamps, positions, GraphColors[i]);
			}
			
			return zg;
		}
	}
}

