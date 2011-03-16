using System;
namespace FinalProject
{
	public interface ISegmenter
	{
		event EventHandler GestureSegmented;
		InputGesture LastGesture { get; }
		
		void AddState(JointState js);
	}
}