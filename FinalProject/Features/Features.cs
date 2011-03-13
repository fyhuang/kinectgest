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
		JointState.JointComponent JointComponent;
		bool Directional;
		public JointAmplitude(string jn, JointState.JointComponent jc, bool d) {
			JointName = jn;
			JointComponent = jc;
			Directional = d;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Component(JointName, JointComponent);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			
			if ( Directional ) {
				bool before = ig.States.FindIndex(x => x.Component(JointName, JointComponent) == min) <
					ig.States.FindIndex(x => x.Component(JointName, JointComponent) == max);
				return (before) ? max - min : min - max;
			}
			else {
				return max - min;
			}
		}
		public override string ToString () {
			return string.Format("[JointAmplitude({0},{1})]", JointName, JointComponent.ToString());
		}
	}
	
	public class NeutralDeviation : IContinuousGestureFeature {
		string JointName;
		JointState.JointComponent JointComponent;
		public NeutralDeviation(string jn, JointState.JointComponent jc) {
			JointName = jn;
			JointComponent = jc;
		}
		public float QueryGesture(InputGesture ig) {
			float np = ig.States[0].Component(JointName, JointComponent) +
				ig.States[ig.States.Count - 1].Component(JointName, JointComponent);
			np /= 2.0f;
			float posd = ig.States.Select(x => x.Component(JointName, JointComponent)).Where(x => x >= np).Select(x => x - np).Sum(),
				  negd = ig.States.Select(x => x.Component(JointName, JointComponent)).Where(x => x < np).Select(x => x - np).Sum();
			return posd + negd;
		}
		public override string ToString () {
			return string.Format("[NeutralDeviation]");
		}
	}
	
	public class NeckAmplitude : IContinuousGestureFeature {
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.NeckPos.Y;
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return max - min;
		}
		public override string ToString () {
			return string.Format("[NeckAmplitude]");
		}
	}
	
	public class NumberCriticalPoints : IContinuousGestureFeature {
		string JointName;
		JointState.JointComponent JointComponent;
		
		public NumberCriticalPoints(string jn, JointState.JointComponent jc) {
			JointName = jn;
			JointComponent = jc;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Component(JointName, JointComponent);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			float center = (max + min) / 2.0f;
			float dist_threshold = 0.5f;
			
			var count = 0;
			foreach ( var js in ig.States ) {
				bool maxthres = (js.Component(JointName, JointComponent) - center) > (max - center) * dist_threshold;
				bool minthres = (center - js.Component(JointName, JointComponent)) > (center - min) * dist_threshold;
			}
			
			return count;
		}
	}
	
	// TODO:  measure divergence between X direction and Z direction (right flick)
	// Normalized by slope, # frames
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
				new JointAmplitude("right-foot", JointState.JointComponent.PosY, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosX, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosY, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosZ, false),
				new JointAmplitude("right-wrist", JointState.JointComponent.Angle, false),
				new NeckAmplitude(),
				
				new NeutralDeviation("right-palm", JointState.JointComponent.PosX)
				//new NumberCriticalPoints("right-palm", JointState.JointComponent.PosX)
			};
		}
	}
}