using System;

namespace FinalProject.Features
{
	public interface ISingleFrameFeature
	{
		bool QueryFrame(JointState js);
	}
}

