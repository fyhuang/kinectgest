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
			return AllFeatures.LearnedFrameFeatures["NeutralStance"].QueryFrame(js) > 0.8f;
		}
		
		void _CheckIfSegmented() {
			if ( mCurrSegmentSize > _minSegmentSize ) {
				Console.WriteLine("Segmented gesture");
				GestureSegmented(this, null);
			}
		}

		public void AddState (JointState js) {
			if ( _IsNeutralStance(js) ) {
				_CheckIfSegmented();
				if ( mCurrSegmentSize > 0 ) {
					LastGesture = new InputGesture(null);
				}
				mCurrSegmentSize = 0;
			}
			else {
				LastGesture.AddJointState(js);
				mCurrSegmentSize++;
			}
		}
		
		public void Finish() {
			_CheckIfSegmented();
		}
	}
}

