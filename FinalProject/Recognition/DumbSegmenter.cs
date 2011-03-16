using System;
using System.Linq;
using System.Collections.Generic;

using FinalProject.Features;

namespace FinalProject
{
	public class DumbSegmenter : ISegmenter
	{
		/// <summary>
		/// How big a sequence of frames has to be to call it a gesture
		/// </summary>
		const int _minSegmentSize = 8;
		/// <summary>
		/// How many neutral frames in a row need to happen to end a gesture
		/// </summary>
		const int _noiseTolerance = 2;
		/// <summary>
		/// How many neutral frames to include before each gesture
		/// </summary>
		const int _numBufferFrames = 4;
		
		int mCurrSegmentSize;
		Queue<JointState> mBuffer;
		List<JointState> mCurrGesture;
		
		public DumbSegmenter ()
		{
			LastGesture = null;
			mCurrSegmentSize = 0;
			mBuffer = new Queue<JointState>();
			mCurrGesture = new List<JointState>();
		}
	
		public event EventHandler GestureSegmented;
		public InputGesture LastGesture { get; private set; }
		
		void _AddToBuffer(JointState js) {
			mBuffer.Enqueue(js);
			if ( mBuffer.Count > _numBufferFrames ) mBuffer.Dequeue();
		}
		
		bool _IsNeutralStance(JointState js) {
			// TODO: punch stance
			return AllFeatures.LearnedFrameFeatures["NeutralStance"].QueryFrame(js) > 0.85f;
		}
		
		void _CheckIfSegmented() {
			if ( mCurrSegmentSize > _minSegmentSize ) {
				Console.WriteLine("Segmented gesture");
				LastGesture = InputGesture.FromJointStates(mCurrGesture);
				GestureSegmented(this, null);
			}
		}

		public void AddState (JointState js) {
			if ( _IsNeutralStance(js) ) {
				_CheckIfSegmented();
				_AddToBuffer(js);
				if ( mCurrSegmentSize > 0 ) {
					mCurrGesture.Clear();
				}
				mCurrSegmentSize = 0;
			}
			else {
				mCurrGesture.AddRange(mBuffer);
				mBuffer.Clear();
				mCurrGesture.Add(js);
				mCurrSegmentSize++;
			}
		}
		
		public void Finish() {
			_CheckIfSegmented();
		}
	}
}

