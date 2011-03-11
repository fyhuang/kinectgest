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
			JointName = jn;
			Threshold = threshold;
		}
		public bool QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Angle(JointName);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return (max - min) > Threshold;
		}
		public override string ToString () {
			return string.Format("[MaxAngleAmplitudeFeature({0},{1:0.000}]", JointName, Threshold);
		}
	}
	
	
	static public class AllFeatures {
		static public List<ISingleGestureFeature> SingleGestureFeatures;
		
		static AllFeatures() {
			SingleGestureFeatures = new List<ISingleGestureFeature> {
			};
			
			SingleGestureFeatures.AddRange(Enumerable.Range(1,4)
			                               .Select(x => new MaxAmplitudeFeature("right-foot", 0.1f * (float)x, 1))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			SingleGestureFeatures.AddRange(Enumerable.Range(2,4)
			                               .Select(x => new MaxAmplitudeFeature("right-palm", 0.1f * (float)x, 1))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			SingleGestureFeatures.AddRange(Enumerable.Range(1,8)
			                               .Select(x => new MaxAngleAmplitudeFeature("right-wrist", (float)x * (float)(Math.PI / 12.0)))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
		}
	}
}