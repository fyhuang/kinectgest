using System;
using System.Linq;

using FinalProject.Features;

namespace FinalProject
{
	public class DumbSegmenter : ISegmenter
	{
		/// <summary>
		/// How big a sequence of frames has to be to call it a gesture
		/// </summary>
		const int _minSegmentSize = 15;
		/// <summary>
		/// How many neutral frames in a row need to happen to end a gesture
		/// </summary>
		const int _noiseTolerance = 2;
		
		int mCurrSegmentSize;
		public DumbSegmenter ()
		{
			LastGesture = new InputGesture(null);
			mCurrSegmentSize = 0;
		}
	
		public event EventHandler GestureSegmented;
		public InputGesture LastGesture { get; private set; }
		
		bool _IsNeutralStance(JointState js) {
			// TODO: punch stance
			return AllFeatures.LearnedFrameFeatures["NeutralStance"].QueryFrame(js) > 0.7f;
		}

		public void AddState (JointState js) {
			if ( _IsNeutralStance(js) ) {
				Console.WriteLine("Neutral");
				if ( mCurrSegmentSize > _minSegmentSize ) {
					GestureSegmented(this, null);
				}
				if ( mCurrSegmentSize > 0 ) {
					LastGesture = new InputGesture(null);
				}
			}
			else {
				LastGesture.AddJointState(js);
				mCurrSegmentSize++;
			}
		}
	}
}

