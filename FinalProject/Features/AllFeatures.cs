using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;
using FinalProject.Utility;

namespace FinalProject.Features
{
	static public class AllFeatures {
		static public List<IGestureFeature> GestureFeatures;
		static public Dictionary<string, ILearnedFrameFeature> LearnedFrameFeatures;
		
		static AllFeatures() {
			GestureFeatures = new List<IGestureFeature> {
				//new JointAmplitude("right-palm", JointState.JointComponent.PosX, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosY, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosZ, false),
				
				//new JointAmplitude("right-wrist", JointState.JointComponent.Angle, false),
				new ProportionChange("right-wrist", JointState.JointComponent.Angle),
				
				new JointAmplitude("right-foot", JointState.JointComponent.PosY, false),
				new ProportionChange("right-foot", JointState.JointComponent.Angle),
				
				new NeckAmplitude(),
				new ProportionFrames(new HighFoot()),
				new ProportionFrames(new HandsTogether()),
				new ProportionFrames(new RHPastNeck()),
				
				//new NeutralDeviation("right-palm", JointState.JointComponent.PosX),
				new NumberCriticalPoints("right-palm", JointState.JointComponent.PosX),
				new DerivativeSum("right-palm", JointState.JointComponent.PosX, x => x.Component("right-palm", JointState.JointComponent.PosZ) > 0.15f),
				new AxisCoincidence("right-palm", JointState.JointComponent.PosX, JointState.JointComponent.PosZ)
			};
			
			
			LearnedFrameFeatures = new Dictionary<string, ILearnedFrameFeature>() {
				{"NeutralStance", new NeutralStance()}
			};
		}
		
		static public void LoadModels() {
			foreach ( var f in LearnedFrameFeatures ) {
				f.Value.LoadModel(String.Format("models/{0}.fmodel", f.Value.ToString()));
			}
		}
		static public void SaveModels() {
			foreach ( var f in LearnedFrameFeatures ) {
				f.Value.SaveModel(String.Format("models/{0}.fmodel", f.Value.ToString()));
			}
		}
		
		static public IEnumerable<float> GestureFeatureResults(InputGesture ig) {
			return GestureFeatures.Select(x => x.QueryGesture(ig));
		}
	}
}
