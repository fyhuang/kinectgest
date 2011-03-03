using System;
namespace FinalProject
{
	public class Recognizer
	{
		enum Command { Train, TestRecognize, Help };
		static public void Main(string[] args)
		{
			Command c = Recognizer.Command.TestRecognize;
			if ( args.Length > 0 ) {
				c = (Command)Enum.Parse(typeof(Recognizer.Command), args[0]);
				if ( c == Recognizer.Command.Help ) {
					foreach ( var cm in Enum.GetNames(typeof(Recognizer.Command)) ) Console.WriteLine(cm);
					return;
				}
			}
			
			////////////////////
			// CHANGE THIS LINE TO USE A NEW RECOGNIZER
			////////////////////
			IRecognizer rec = new SingleFrameRecognizer();
			
			
			switch ( c ) {
			case Command.TestRecognize:
				string filename = "gestures/track_high_kick_01.log";
				if ( args.Length > 1 ) filename = args[1];
				var result = rec.RecognizeSingleGesture(new InputGesture(new LogFileLoader(filename)));
				Console.WriteLine(result.ToString());
				break;
			}
		}
	}
}

