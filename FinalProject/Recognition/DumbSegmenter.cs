using System;
namespace FinalProject
{
	public class DumbSegmenter : ISegmenter
	{
		public DumbSegmenter ()
		{
		}
	
		public event EventHandler GestureSegmented;

		public void AddState (JointState js)
		{
			throw new NotImplementedException ();
		}
	}
}

