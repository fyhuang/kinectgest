using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject;

namespace FinalProject.Features
{
	static public class AllFeatures {
		static public List<IGestureFeature> GestureFeatures;
		static public Dictionary<string, ILearnedFrameFeature> LearnedFrameFeatures;
		
		static AllFeatures() {
			GestureFeatures = new List<IGestureFeature> {
				new JointAmplitude("right-palm", JointState.JointComponent.PosX, false),
				new JointAmplitude("right-palm", JointState.JointComponent.PosY, false),
				//new JointAmplitude("right-palm", JointState.JointComponent.PosZ, false),
				//new JointAmplitude("left-palm", JointState.JointComponent.PosX, false),
				
				new JointAmplitude("right-wrist", JointState.JointComponent.Angle, false),
				//new ProportionChange("right-wrist", JointState.JointComponent.Angle),
				//new ProportionChange("left-wrist", JointState.JointComponent.Angle),
				
				new JointAmplitude("right-foot", JointState.JointComponent.PosY, false)
				//new ProportionChange("right-foot", JointState.JointComponent.Angle),
				//new MinDistance("left-palm", "right-palm"),
				//new MaxDistance("left-palm", "right-palm"),
				
				//new NeckAmplitude(),
				//new ProportionFrames(new HighFoot()),
				//new ProportionFrames(new RHPastNeck()),
				//new ProportionFrames(new RHandForward()),
				
				/*new NeutralDeviation("right-palm", JointState.JointComponent.PosX),
				new NumberCriticalPoints("right-palm", JointState.JointComponent.PosX),
				new DerivativeSum("right-palm", JointState.JointComponent.PosX, x => x.Component("right-palm", JointState.JointComponent.PosZ) > 0.15f),
				new AxisCoincidence("right-palm", JointState.JointComponent.PosX, JointState.JointComponent.PosZ)*/
			};
			
			// Auxiliary features (enable at your own peril!)
			/*var other_joints = new[]{"right-shoulder", "left-shoulder", "right-elbow", "left-elbow", "right-knee", "left-knee"};
			foreach ( var j in other_joints ) {
				GestureFeatures.Add(new JointAmplitude(j, JointState.JointComponent.PosX, false));
				GestureFeatures.Add(new JointAmplitude(j, JointState.JointComponent.PosY, false));
				GestureFeatures.Add(new JointAmplitude(j, JointState.JointComponent.PosZ, false));
				GestureFeatures.Add(new JointAmplitude(j, JointState.JointComponent.Angle, false));
				foreach ( var k in other_joints ) {
					if ( j != k ) {
						GestureFeatures.Add(new MinDistance(j, k));
						GestureFeatures.Add(new MaxDistance(j, k));
					}
				}
			}*/
			
			
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
