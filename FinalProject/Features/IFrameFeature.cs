using System;
using System.Collections.Generic;

namespace FinalProject.Features
{
	public interface IFrameFeature
	{
		float QueryFrame(JointState js);
	}
	
	public interface ILearnedFrameFeature : IFrameFeature
	{
		void Train(IEnumerable<JointState> states);
		void SaveModel(string filename);
		void LoadModel(string filename);
	}
}

