using System;
namespace FinalProject
{
	public interface ISegmenter
	{
		event EventHandler GestureSegmented;
		void AddState(JointState js);
	}
}