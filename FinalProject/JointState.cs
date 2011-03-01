using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using OpenTK;

namespace FinalProject
{
	public struct JointState
	{
		static public Dictionary<string, int> NamesToJoints;
		static public List<int> JointParents;
		static JointState()
		{
			NamesToJoints = new Dictionary<string, int>() {
				{"neck", 0},
				{"head", 1},
				{"right-shoulder", 2},
				{"left-shoulder", 6},
				{"right-elbow", 3},
				{"left-elbow", 7},
				{"right-wrist", 4},
				{"right-pe", 18},
				{"left-pe", 19},
				{"left-wrist", 8},
				{"right-p", 5},
				{"right-h", 10},
				{"left-h", 14},
				{"left-p", 9},
				{"right-knee", 11},
				{"left-knee", 15},
				{"right-ankle", 12},
				{"right-foot", 13}
				// Missing: left ankle, left foot
			};
			
			JointParents = new List<int>() { -1, // Neck
				0, 0, 2, 3, 4,
				0, 6, 7, 8,
				-2, 10, 11, 12,
				-2, 14, 15, 16,
				-2, -2
			};
			
			// Sanity check
			foreach ( var nj in NamesToJoints ) {
				var c = NamesToJoints.Count(x => (x.Value == nj.Value));
				if ( c > 1 ) throw new Exception("Duplicate joints in relative joint name list!");
			}
		}
		
		public float Timestamp;
		public Vector3 NeckPos;
		public Vector3[] RelativeJoints;
		public Quaternion[] RelativeAngles;
		
		static public JointState FromRawJointState(RawJointState rjs)
		{
			JointState rel = new JointState();
			
			rel.Timestamp = rjs.Timestamp;
			rel.NeckPos = rjs.Joints[0];
			rel.RelativeJoints = new Vector3[rjs.Joints.Length];
			for ( int i = 0; i < rjs.Joints.Length - 1; i++ ) {
				rel.RelativeJoints[i] = rjs.Joints[i+1] - rel.NeckPos;
			}
			return rel;
		}
		
		static public JointState CloneFrom(JointState rjs)
		{
			JointState rel = new JointState();
			rel.Timestamp = rjs.Timestamp;
			rel.NeckPos = rjs.NeckPos;
			rel.RelativeJoints = new Vector3[rjs.RelativeJoints.Length];
			rjs.RelativeJoints.CopyTo(rel.RelativeJoints, 0);
			return rel;
		}
	}
}

