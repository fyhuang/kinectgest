using System;

using FinalProject;

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
	
	public class RHPastNeck : IFrameFeature {
		public float QueryFrame(JointState js) {
			if (js.Pos("right-palm").X - js.Pos("neck").X < 0.0f)
				return 1.0f;
			else
				return 0.0f;
		}
	}
}

