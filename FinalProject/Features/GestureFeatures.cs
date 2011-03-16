using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;

namespace FinalProject.Features
{
	public class JointAmplitude : IGestureFeature {
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
	
	public class NeutralDeviation : IGestureFeature {
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
	
	public class NeckAmplitude : IGestureFeature {
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.NeckPos.Y;
			float min = ig.States.Min(f), max = ig.States.Max(f);
			return max - min;
		}
		public override string ToString () {
			return string.Format("[NeckAmplitude]");
		}
	}
	
	public class ProportionChange : IGestureFeature {
		string JointName;
		JointState.JointComponent JointComponent;
		public ProportionChange(string jn, JointState.JointComponent jc) {
			JointName = jn;
			JointComponent = jc;
		}
		public float QueryGesture(InputGesture ig) {
			float sum = 0.0f;
			for ( int i = 1; i < ig.States.Count; i++ ) {
				sum += (float)Math.Abs(ig.States[i].Component(JointName, JointComponent) - 
									   ig.States[i-1].Component(JointName, JointComponent));
			}
			return sum / (float)ig.States.Count;
		}
		public override string ToString () {
			return string.Format("[ProportionChange({0},{1})]", JointName, JointComponent);
		}
	}
	
	public class ProportionFrames : IGestureFeature {
		IFrameFeature mFeature;
		public ProportionFrames(IFrameFeature f) {
			mFeature = f;
		}
		public float QueryGesture (InputGesture ig) {
			return ig.States.Sum(x => mFeature.QueryFrame(x)) / (float)ig.States.Count;
		}
		public override string ToString() {
			return string.Format ("[ProportionFrames({0})]", mFeature.ToString());
		}
	}
	
	public class NumberCriticalPoints : IGestureFeature {
		string JN;
		JointState.JointComponent JC;
		
		public NumberCriticalPoints(string jn, JointState.JointComponent jc) {
			JN = jn;
			JC = jc;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Component(JN, JC);
			float min = ig.States.Min(f), max = ig.States.Max(f);
			float center = (max + min) / 2.0f;
			float dist_threshold = 0.5f;
			
			bool dir = (f(ig.States[1]) - f(ig.States[0])) > 0.0f;
			var count = 0;
			var sinceLast = 100;
			for ( int i = 1; i < ig.States.Count; i++ ) {
				float maxthres = (max - center) * dist_threshold;
				float minthres = (min - center) * dist_threshold;
				bool newdir = (f(ig.States[i]) - f(ig.States[i-1])) > 0.0f;
				if ( newdir != dir && sinceLast > 10 ) {
					float relpos = f(ig.States[i]) - center;
					if ( relpos > maxthres || relpos < minthres ) {
						count++;
					}
					sinceLast = 0;
				}
				dir = newdir;
				sinceLast++;
			}
			
			return (float)count / (float)ig.States.Count;
		}
	}
	
	public class DerivativeSum : IGestureFeature {
		string JN;
		JointState.JointComponent JC;
		Func<JointState, bool> Cond;
		
		public DerivativeSum(string jn, JointState.JointComponent jc, Func<JointState, bool> condition) {
			JN = jn;
			JC = jc;
			Cond = condition;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f = x => x.Component(JN, JC);
			float count = 0.0f;
			float sum = 0.0f;
			for ( int i = 1; i < ig.States.Count; i++ ) {
				var state = ig.States[i];
				if ( Cond(state) ) {
					sum += f(state) - f(ig.States[i-1]);
					count += 1.0f;
				}
			}
			
			return (count == 0.0f) ? 0.0f : sum / count;
		}
	}
	
	// TODO:  measure divergence between X direction and Z direction (right flick)
	// Normalized by slope, # frames
	public class AxisCoincidence : IGestureFeature {
		string JN;
		JointState.JointComponent JA;
		JointState.JointComponent JB;
		
		public AxisCoincidence(string jn, JointState.JointComponent ja, JointState.JointComponent jb) {
			JN = jn;
			JA = ja;
			JB = jb;
		}
		public float QueryGesture(InputGesture ig) {
			Func<JointState, float> f1 = x => x.Component(JN, JA),
									f2 = x => x.Component(JN, JB);
			
			double sum = 0.0f;
			for ( int i = 1; i < ig.States.Count; i++ ) {
				var state = ig.States[i];
				var pstate = ig.States[i-1];
				if ( f1(state) - f1(pstate) > 0.0f ) {
					sum += f2(state) - f2(pstate);
				}
				else {
					sum -= f2(state) - f2(pstate);
				}
			}
			
			return (float)(sum * 100.0) / (float)ig.States.Count;
		}
	}
	
	public class MinDistance : IGestureFeature {
		string JN1, JN2;
		public MinDistance(string j1, string j2) {
			JN1 = j1;
			JN2 = j2;
		}
		public float QueryGesture(InputGesture ig) {
			return ig.States.Select(x => (x.Pos(JN1) - x.Pos(JN2)).LengthFast).Min();
		}
	}
}