using System;
using System.Text;
using System.Collections.Generic;

namespace FinalProject
{
	public struct RecognizerResult {
		public string Gesture1;
		public float Confidence1;
		public string Gesture2;
		public float Confidence2;
		public string Gesture3;
		public float Confidence3;
		
		static public RecognizerResult Empty() {
			var rr = new RecognizerResult();
			rr.Gesture1 = rr.Gesture2 = rr.Gesture3 = "";
			rr.Confidence1 = rr.Confidence2 = rr.Confidence3 = 0.0f;
			return rr;
		}
		
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append("[RecognizerResult]:\n");
			sb.Append(string.Format("\t1. \"{0}\" with {1} confidence\n", Gesture1, Confidence1));
			sb.Append(string.Format("\t2. \"{0}\" with {1} confidence\n", Gesture2, Confidence2));
			sb.Append(string.Format("\t3. \"{0}\" with {1} confidence", Gesture3, Confidence3));
			return sb.ToString();
		}
	}
	
	public interface IRecognizer
	{
		string[] Gestures { get; }
		
		RecognizerResult RecognizeSingleGesture(InputGesture g);
		
		void ClearHistory();
		RecognizerResult AddNewData(JointState js);
		
		
		void Train(IDictionary<string, IList<InputGesture>> gestures);
		void SaveModel(string filename);
		void LoadModel(string filename);
	}
}

