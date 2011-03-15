using System;
using System.Collections.Generic;

namespace FinalProject
{
	public class SingleFrameRecognizer : IRecognizer
	{
		List<Features.IFrameFeature> mFeatures;
		
		public SingleFrameRecognizer() {
			mFeatures = new List<Features.IFrameFeature>() {
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
				if ( mFeatures[0].QueryFrame(js) > 0.0f ) {
					var res = RecognizerResult.Empty();
					res.Gesture1 = "high_kick";
					res.Confidence1 = 100.0f;
					return res;
				}
			}
			return RecognizerResult.Empty();
		}
		
		
		// Not implemented things
		public void ClearHistory() {
			throw new NotImplementedException();
		}
		
		public RecognizerResult AddNewData(JointState js) {
			throw new NotImplementedException();
		}
		
		public void Train (IDictionary<string, IList<InputGesture>> gestures)
		{
			throw new NotImplementedException ();
		}

		public void SaveModel (string filename)
		{
			throw new NotImplementedException ();
		}

		public void LoadModel (string filename)
		{
			throw new NotImplementedException ();
		}
	}
}

