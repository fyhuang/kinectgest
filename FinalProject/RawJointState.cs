using System;
using System.Text;
using OpenTK;

namespace FinalProject
{
	public class RawJointState
	{
		public float Timestamp { get; set; }
		public Vector3[] Joints;
		
		static public RawJointState FromInputLine(string line) {
			string[] words = line.Split();
			
			int num = (words.Length-1) / 3;
			var rjs = new RawJointState();
			rjs.Joints = new Vector3[num];
			
			try {
				rjs.Timestamp = float.Parse(words[0]);
				for ( int i = 0; i < num; i++ ) {
					int ri = (i*3) + 1;
					
					rjs.Joints[i].X = float.Parse(words[ri]);
					rjs.Joints[i].Y = float.Parse(words[ri+1]);
					rjs.Joints[i].Z = float.Parse(words[ri+2]);
				}
			}
			catch ( FormatException ) {
				Console.WriteLine("Input line not properly formatted: {0}", line);
				return null;
			}
			
			return rjs;
		}
		
		public AABB GetBB() {
			Vector3 min = Joints[0], max = Joints[0];
			foreach ( var v in Joints ) {
				if ( v.X < min.X ) min.X = v.X;
				if ( v.X > max.X ) max.X = v.X;
			}
			
			AABB box = new AABB();
			box.Min = min;
			box.Max = max;
			return box;
		}
		
		public override string ToString ()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0} ", Timestamp);
			foreach ( var j in Joints ) {
				sb.AppendFormat("{0} {1} {2} ", j.X, j.Y, j.Z);
			}
			return sb.ToString();
		}
	}
}

