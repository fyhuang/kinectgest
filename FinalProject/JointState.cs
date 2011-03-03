using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK;

namespace FinalProject
{
	public class JointState
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
				{"right-pelvis", 18},
				{"left-pelvis", 19},
				{"left-wrist", 8},
				{"right-palm", 5},
				{"right-hip", 10},
				{"left-hip", 14},
				{"left-palm", 9},
				{"right-knee", 11},
				{"left-knee", 15},
				{"right-ankle", 12},
				{"right-foot", 13},
				{"left-ankle", 16},
				{"left-foot", 17}
			};
			
			JointParents = new List<int>() { -1, // Neck
				0, // Head
				0, 2, 3, 4,
				0, 6, 7, 8,
				18, 10, 11, 12,
				19, 14, 15, 16,
				0, 0
			};
			
			// Sanity check
			foreach ( var nj in NamesToJoints ) {
				var c = NamesToJoints.Count(x => (x.Value == nj.Value));
				if ( c > 1 ) throw new Exception("Duplicate joints in relative joint name list!");
			}
			
			foreach ( var jp in JointParents ) {
				// TODO: check that there are no cycles here
			}
		}
		
		public float Timestamp;
		public Vector3 NeckPos;
		/// <summary>
		/// Relative joint positions from neck joint
		/// </summary>
		public Vector3[] RelativeJoints;
		/// <summary>
		/// Relative joint angles to parent joint (Vector3.UnitY if no parent)
		/// </summary>
		public float[] RelativeAngles;
		
		public Vector3 Pos(string name)
		{
			Debug.Assert(NamesToJoints.ContainsKey(name));
			return RelativeJoints[NamesToJoints[name]];
		}
		
		public float Angle(string name)
		{
			Debug.Assert(NamesToJoints.ContainsKey(name));
			return RelativeAngles[NamesToJoints[name]];
		}
		
		
		static public JointState FromRawJointState(RawJointState rjs)
		{
			JointState rel = new JointState();
			
			rel.Timestamp = rjs.Timestamp;
			rel.NeckPos = rjs.Joints[0];
			
			rel.RelativeJoints = rjs.Joints.Select(x => x - rel.NeckPos).ToArray();
			
			rel.RelativeAngles = new float[rjs.Joints.Length];
			for ( int i = 1; i < rel.RelativeJoints.Length; i++ ) {
				Vector3 thisVec = rel.RelativeJoints[i] - rel.RelativeJoints[JointParents[i]];
				Vector3 parentVec = (JointParents[JointParents[i]] == -1) ? Vector3.UnitY :
					rel.RelativeJoints[JointParents[i]] - rel.RelativeJoints[JointParents[JointParents[i]]];
				rel.RelativeAngles[i] = Vector3.CalculateAngle(thisVec, parentVec);
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

