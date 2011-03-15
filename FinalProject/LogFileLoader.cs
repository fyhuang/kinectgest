using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FinalProject
{
	public class LogFileLoader : IEnumerable<RawJointState>
	{
		string mLogFilename;
		int mNumJoints;
		public LogFileLoader(string filename)
		{
			mLogFilename = filename;
		}
		
		public IEnumerator<RawJointState> GetEnumerator() {
			//System.Console.WriteLine("Loading {0}...", mLogFilename);
			
			mNumJoints = -1;
			
			int numLoaded = 0;
			var file = new StreamReader(mLogFilename);
			while ( !file.EndOfStream ) {
				string line = file.ReadLine().Trim();
				if ( line.Length == 0 ) continue;
				if ( line[0] == '#' ) continue;
				if ( mNumJoints == -1 ) {
					// Count the number of joints
					mNumJoints = (line.Split().Length - 1) / 3;
					//System.Console.WriteLine("Number joints: {0}", mNumJoints);
				}
				
				var j = RawJointState.FromInputLine(line);
				if ( j.Joints.Length != mNumJoints ) throw new Exception("Mismatch in number of joints!");
				yield return j;
				numLoaded++;
			}
			
			System.Console.WriteLine("Loaded {0} joint states from {1}", numLoaded, mLogFilename);
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		
		public static IEnumerable<string> LogFilenames(string gesture_name, string format) {
			var output = new List<string>();
			int index = 0;
			while ( true ) {
				var filename = String.Format(format, gesture_name, index);
				if ( !File.Exists(filename) ) break;
				output.Add(filename);
				index++;
			}
			return output;
		}
		public static IEnumerable<string> LogFilenames(string gesture_name) {
			return LogFilenames(gesture_name, "gestures/track_{0}_{1:00}.log");
		}
	}
}

