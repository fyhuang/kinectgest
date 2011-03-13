using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;

namespace FinalProject.Features
{
	public class HighFoot : ISingleFrameFeature {
		public bool QueryFrame(JointState js) {
			if ( js.Pos("right-foot").Y >= -0.2f )
				return true;
			else
				return false;
		}
	}
	
	public class AnyFrame : ISingleGestureFeature {
		ISingleFrameFeature mFeature;
		public AnyFrame(ISingleFrameFeature f) {
			mFeature = f;
		}
		public bool QueryGesture (InputGesture ig) {
			return ig.States.Any(x => mFeature.QueryFrame(x));
		}
		public override string ToString() {
			return string.Format ("[AnyFrame({0})]", mFeature.ToString());
		}
	}
	
	public class MaxAmplitude : ISingleGestureFeature {
		string JointName;
		float Threshold;
		int ComponentIx;
		public MaxAmplitude(string jn, float threshold, int cmp) {
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
			return string.Format("[MaxAmplitude({0},{1:0.00},{2})]", JointName, Threshold, FinalProject.Utility.Utility.CompToChar(ComponentIx));
		}
	}
	
	public class MaxAngleAmplitude : ISingleGestureFeature {
		string JointName;
		float Threshold;
		public MaxAngleAmplitude(string jn, float threshold) {
			JointName = jn;
			Threshold = threshold;
		}
		public bool QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Angle(JointName);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return (max - min) > Threshold;
		}
		public override string ToString () {
			return string.Format("[MaxAngleAmplitude({0},{1:0.000}]", JointName, Threshold);
		}
	}
	
	
	#region Continuous features
	public class JointAmplitude : IContinuousGestureFeature {
		string JointName;
		int ComponentIx;
		public JointAmplitude(string jn, int cmp) {
			JointName = jn;
			ComponentIx = cmp;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Pos(JointName).Comp(ComponentIx);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return max - min;
		}
		public override string ToString () {
			return string.Format("[JointAmplitude({0},{1})]", JointName, FinalProject.Utility.Utility.CompToChar(ComponentIx));
		}
	}
	
	public class JointAngleAmplitude : IContinuousGestureFeature {
		string JointName;
		public JointAngleAmplitude(string jn) {
			JointName = jn;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Angle(JointName);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return max - min;
		}
		public override string ToString () {
			return string.Format("[JointAngleAmplitude({0}]", JointName);
		}
	}
	#endregion
	
	
	static public class AllFeatures {
		static public List<ISingleGestureFeature> SingleGestureFeatures;
		static public List<IContinuousGestureFeature> ContinuousGestureFeatures;
		
		static AllFeatures() {
			SingleGestureFeatures = new List<ISingleGestureFeature>() {
				new AnyFrame(new HighFoot())
			};
			SingleGestureFeatures.AddRange(Enumerable.Range(2,10)
			                               .Select(x => new MaxAmplitude("right-foot", 0.2f * (float)x, 1))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			SingleGestureFeatures.AddRange(Enumerable.Range(2,6)
			                               .Select(x => new MaxAmplitude("right-palm", 0.2f * (float)x, 1))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			SingleGestureFeatures.AddRange(Enumerable.Range(2,6)
			                               .Select(x => new MaxAmplitude("right-palm", 0.2f * (float)x, 0))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			SingleGestureFeatures.AddRange(Enumerable.Range(2,10)
			                               .Select(x => new MaxAngleAmplitude("right-wrist", (float)x * (float)(Math.PI / 12.0)))
			                               .Select(x => (ISingleGestureFeature)x)
			                               );
			
			
			ContinuousGestureFeatures = new List<IContinuousGestureFeature> {
				new JointAmplitude("right-foot", 1),
				new JointAmplitude("right-palm", 0),
				new JointAmplitude("right-palm", 1),
				new JointAngleAmplitude("right-wrist")
			};
		}
	}
}