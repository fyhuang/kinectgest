using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;

namespace FinalProject.Features
{
	public class HighFoot : ISingleFrameFeature {
		public bool QueryFrame(JointState js) {
			if ( js.Pos("right-foot").Y >= 0.0f )
				return true;
			else
				return false;
		}
	}
	
	public class MaxAmplitudeFeature : ISingleGestureFeature {
		string JointName;
		float Threshold;
		int ComponentIx;
		public MaxAmplitudeFeature(string jn, float threshold, int cmp) {
			JointName = jn;
			Threshold = threshold;
			ComponentIx = cmp;
		}
		public bool QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Pos(JointName).Comp(ComponentIx);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return (max - min) > Threshold;
		}
		public override string ToString () {
			return string.Format("[MaxAmplitudeFeature({0},{1:0.00},{2})]", JointName, Threshold, FinalProject.Utility.Utility.CompToChar(ComponentIx));
		}
	}
	
	public class MaxAngleAmplitudeFeature : ISingleGestureFeature {
		string JointName;
		float Threshold;
		public MaxAngleAmplitudeFeature(string jn, float threshold) {
			Threshold = threshold;
		}
		public bool QueryGesture(InputGesture ig) {
			return false;
		}
	}
	
	
	static public class AllFeatures {
		static public List<ISingleGestureFeature> SingleGestureFeatures;
		
		static AllFeatures() {
			SingleGestureFeatures = new List<ISingleGestureFeature> {
				new MaxAmplitudeFeature("right-foot", 0.1f, 1),
				new MaxAmplitudeFeature("right-foot", 0.3f, 1),
				new MaxAmplitudeFeature("right-foot", 0.5f, 1),
				new MaxAmplitudeFeature("right-foot", 1.0f, 1),
				
				new MaxAmplitudeFeature("right-palm", 0.3f, 1),
				new MaxAmplitudeFeature("right-palm", 0.5f, 1)
			};
		}
	}
}