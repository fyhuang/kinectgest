using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FinalProject;

namespace LogFileVisualizer
{
	class LogFileVisualizer
	{
		enum Command { ViewGesture, PlotJoint };
		
		static string GetJointName() {
			System.Console.WriteLine("Which joint to graph?");
			
			string jn = null;
			while ( jn == null ) {
				jn = System.Console.ReadLine();
				if ( !JointState.NamesToJoints.ContainsKey(jn) ) {
					System.Console.WriteLine("Joint with name {0} doesn't exist, enter another:", jn);
					jn = null;
				}
			}
			
			return jn;
		}
		
		public static void Main (string[] args)
		{
			string filename = "gestures/track_high_kick_00.log";
			Command cmd = LogFileVisualizer.Command.ViewGesture;
			if ( args.Length > 0 ) {
				filename = args[0];
				if ( args.Length > 1 ) {
					cmd = (LogFileVisualizer.Command)Enum.Parse(typeof(LogFileVisualizer.Command), args[1]);
				}
			}
			
			string whichJoint;
			switch ( cmd ) {
			case Command.ViewGesture:
				using ( var vw = new GestureJointVisualizer(filename) ) {
					vw.Run(30.0);
				}
				break;
			case Command.PlotJoint:
				whichJoint = GetJointName();
				var gest1 = new InputGesture(new LogFileLoader(filename));
				var jp = new JointPlotter(gest1, whichJoint);
				jp.DisplayPlots();
				break;
			}
		}
	}
}

