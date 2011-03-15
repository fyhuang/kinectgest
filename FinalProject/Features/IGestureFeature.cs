using System;
using System.Collections.Generic;

namespace FinalProject
{
	public interface IGestureFeature
	{
		float QueryGesture(InputGesture ig);
	}
	
	public interface ILearnedGestureFeature : IGestureFeature
	{
		void Train(IEnumerable<InputGesture> states);
		void SaveModel(string filename);
		void LoadModel(string filename);
	}
}

