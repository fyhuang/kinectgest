using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using OpenTK;

namespace FinalProject
{
	/// <summary>
	/// Represents one "action" (kick, punch, jump, whatever) as a set of discrete <see cref="RelativeJointState"/>.
	/// </summary>
	public class Gesture
	{
		public float StartTime;
		public List<JointState> States;
		
		public Gesture(IEnumerable<RawJointState> states)
		{
			States = new List<JointState>();
			foreach ( var js in states ) {
				States.Add(JointState.FromRawJointState(js));
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
		public void InterpolateState(float time, ref JointState state)
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
		
		class LateralPositionEnumerable : IEnumerable<Vector3> {
			Gesture parent;
			int joint;
			public LateralPositionEnumerable(Gesture g, int j) {
				parent = g;
				joint = j;
				System.Diagnostics.Debug.Assert(j >= 0 && j < parent.States[0].RelativeJoints.Length);
			}
			
			public IEnumerator<Vector3> GetEnumerator() {
				for ( int i = 0; i < parent.States.Count; i++ ) {
					yield return parent.States[i].RelativeJoints[joint];
				}
			}
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}
		public IEnumerable<Vector3> JointPositions(string name) {
			int num = JointState.NamesToJoints[name.Trim().ToLower()];
			return new LateralPositionEnumerable(this, num);
		}
		
		
		/*
		class SurrondingEnumerable : IEnumerable<JointState> {
			int myidx;
			int numEachSide;
			Gesture parent;
			
			public SurrondingEnumerable(Gesture action, int num_per_side) {
				myidx = -num_per_side;
				numEachSide = num_per_side;
				parent = action;
			}
			
			public IEnumerator<JointState> GetEnumerator() {
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
				var newstate = JointState.CloneFrom(States[i]);
			}
		}
		*/
	}
}
