using System;

using FinalProject;

namespace FinalProject.Features
{
	class HighFoot : ISingleFrameFeature {
		public bool QueryFrame(JointState js) {
			if ( js.Pos("right-foot").Y >= 0.0f )
				return true;
			else
				return false;
		}
	}
}