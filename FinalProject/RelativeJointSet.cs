using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using OpenTK;

namespace FinalProject
{
	public struct RelativeJointState
	{
		static public Dictionary<string, int> NamesToJoints;
		static RelativeJointState()
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
			
			// Sanity check
			foreach ( var nj in NamesToJoints ) {
				var c = NamesToJoints.Count(x => (x.Value == nj.Value));
				if ( c > 1 ) throw new Exception("Duplicate joints in relative joint name list!");
			}
		}
		
		public float Timestamp;
		public Vector3 NeckPos;
		public Vector3[] RelativeJoints;
		
		static public RelativeJointState FromRawJointState(RawJointState rjs)
		{
			RelativeJointState rel = new RelativeJointState();
			
			rel.Timestamp = rjs.Timestamp;
			rel.NeckPos = rjs.Joints[0];
			rel.RelativeJoints = new Vector3[rjs.Joints.Length];
			for ( int i = 0; i < rjs.Joints.Length - 1; i++ ) {
				rel.RelativeJoints[i] = rjs.Joints[i+1] - rel.NeckPos;
			}
			return rel;
		}
		
		static public RelativeJointState CloneFrom(RelativeJointState rjs)
		{
			RelativeJointState rel = new RelativeJointState();
			rel.Timestamp = rjs.Timestamp;
			rel.NeckPos = rjs.NeckPos;
			rel.RelativeJoints = new Vector3[rjs.RelativeJoints.Length];
			rjs.RelativeJoints.CopyTo(rel.RelativeJoints, 0);
			return rel;
		}
	}
	
	/// <summary>
	/// Represents one "action" (kick, punch, jump, whatever) as a set of discrete <see cref="RelativeJointState"/>.
	/// </summary>
	public class RelativeAction
	{
		public float StartTime;
		public List<RelativeJointState> States;
		
		public RelativeAction(IEnumerable<RawJointState> states)
		{
			States = new List<RelativeJointState>();
			foreach ( var js in states ) {
				States.Add(RelativeJointState.FromRawJointState(js));
			}
			StartTime = States[0].Timestamp;
		}
		
		public float TotalTime {
			get {
				return States[States.Count-1].Timestamp - StartTime;
			}
		}
		
		/// <summary>
		/// Modifies the <see cref="RelativeJointState"/> pointed to by state to contain interpolated joint information at a particular timestep.
		/// IMPORTANT: the array pointed to by state.Joints WILL be modified!
		/// </summary>
		/// <param name="time">
		/// The time, from the beginning of the animation, to get the joint state at.
		/// </param>
		/// <param name="state">
		/// Where to write the results to. IMPORTANT: the array pointed to by state.Joints WILL be modified!
		/// </param>
		public void InterpolateState(float time, ref RelativeJointState state)
		{
			if ( time <= 0.0f ) {
				States[0].RelativeJoints.CopyTo(state.RelativeJoints, 0);
			}
			else if ( time >= States[States.Count-1].Timestamp - StartTime ) {
				States[States.Count-1].RelativeJoints.CopyTo(state.RelativeJoints, 0);
			}
			else {
				int state1idx = States.FindLastIndex(x => ((x.Timestamp - StartTime) <= time));
				var state1 = States[state1idx];
				var state2 = States[state1idx+1];
				float weight2 = (time - (state1.Timestamp - StartTime)) /
				                (state2.Timestamp - state1.Timestamp),
					weight1 = (1.0f - weight2);
				
				for ( int i = 0; i < state.RelativeJoints.Length; i++ ) {
					state.RelativeJoints[i] = weight1 * state1.RelativeJoints[i] + weight2 * state2.RelativeJoints[i];
				}
			}
		}
		
		
		class SurrondingEnumerable : IEnumerable<RelativeJointState> {
			int myidx;
			int numEachSide;
			RelativeAction parent;
			
			public SurrondingEnumerable(RelativeAction action, int num_per_side) {
				myidx = -num_per_side;
				numEachSide = num_per_side;
				parent = action;
			}
			
			public IEnumerator<RelativeJointState> GetEnumerator() {
				while ( myidx <= numEachSide ) {
					if ( myidx < 0 ) yield return parent.States[0];
					else if ( myidx >= parent.States.Count ) yield return parent.States[parent.States.Count-1];
					else yield return parent.States[myidx];
					
					myidx++;
				}
			}
			
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}
		
		public void RemoveNoise()
		{
			for ( int i = 0; i < States.Count; i++ ) {
				var newstate = RelativeJointState.CloneFrom(States[i]);
			}
		}
	}
}

