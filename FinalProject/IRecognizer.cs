using System;
namespace FinalProject
{
	public struct RecognizerResult {
		public string Gesture1;
		public float Confidence1;
		public string Gesture2;
		public float Confidence2;
		public string Gesture3;
		public float Confidence3;
		
		public override string ToString()
		{
			return string.Format("[RecognizerResult]: \"{0}\" with {1} confidence", Gesture1, Confidence1);
		}
	}
	
	public interface IRecognizer
	{
		string[] Gestures { get; }
		
		RecognizerResult RecognizeSingleGesture(InputGesture g);
		
		void ClearHistory();
		RecognizerResult AddNewData(JointState js);
	}
}

