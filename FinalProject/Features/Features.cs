using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;

namespace FinalProject.Features
{
	public class HighFoot : IFrameFeature {
		public float QueryFrame(JointState js) {
			if ( js.Pos("right-foot").Y >= -0.2f )
				return 1.0f;
			else
				return 0.0f;
		}
	}
	
	public class HandsTogether : IFrameFeature {
		public float QueryFrame(JointState js) {
			if ((js.Pos("right-palm") - js.Pos("left-palm")).Length < 0.2f)
				return 1.0f;
			else
				return 0.0f;
		}
	}
	
	
	
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
				//bool maxthres = (js.Component(JN, JointComponent) - center) > (max - center) * dist_threshold;
				//bool minthres = (center - js.Component(JN, JointComponent)) > (center - min) * dist_threshold;
				bool newdir = (f(ig.States[i]) - f(ig.States[i-1])) > 0.0f;
				if ( newdir != dir && sinceLast > 10 ) {
					count++;
					sinceLast = 0;
				}
				dir = newdir;
				sinceLast++;
			}
			
			return (float)count / (float)ig.States.Count;
		}
	}
	
	// TODO:  measure divergence between X direction and Z direction (right flick)
	// Normalized by slope, # frames
	// TODO: measure distance between left hand and right hand
	
	
	static public class AllFeatures {
		static public List<IGestureFeature> GestureFeatures;
		
		static AllFeatures() {
			GestureFeatures = new List<IGestureFeature> {
				new JointAmplitude("right-palm", JointState.JointComponent.PosX, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosY, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosZ, false),
				
				new JointAmplitude("right-wrist", JointState.JointComponent.Angle, false),
				new ProportionChange("right-wrist", JointState.JointComponent.Angle),
				
				new JointAmplitude("right-foot", JointState.JointComponent.PosY, false),
				
				new NeckAmplitude(),
				new ProportionFrames(new HighFoot()),
				new ProportionFrames(new HandsTogether()),
				
				//new NeutralDeviation("right-wrist", JointState.JointComponent.PosX),
				new NumberCriticalPoints("right-palm", JointState.JointComponent.PosX)
			};
		}
		
		static public IEnumerable<float> GestureFeatureResults(InputGesture ig) {
			return GestureFeatures.Select(x => x.QueryGesture(ig));
		}
	}
}