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
	}
}

