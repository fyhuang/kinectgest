using System;
using System.Collections.Generic;

namespace FinalProject
{
	public class SingleFrameRecognizer : IRecognizer
	{
		List<Features.ISingleFrameFeature> mFeatures;
		
		public SingleFrameRecognizer() {
			mFeatures = new List<Features.ISingleFrameFeature>() {
				new Features.HighFoot()
			};
		}
		
		public string[] Gestures {
			get {
				return new string[]{"high-kick"};
			}
		}
		
		public RecognizerResult RecognizeSingleGesture(InputGesture g) {
			foreach ( var js in g.States ) {
				if ( mFeatures[0].QueryFrame(js) ) {
					return new RecognizerResult(){
						Gesture1 = "high-kick",
						Confidence1 = 100.0f
					};
				}
			}
			return RecognizerResult.Empty();
		}
		
		public void ClearHistory() {
			throw new NotImplementedException();
		}
		
		public RecognizerResult AddNewData(JointState js) {
			throw new NotImplementedException();
		}
	}
}

