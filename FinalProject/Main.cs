using System;
namespace FinalProject
{
	public class Recognizer
	{
		enum Command { Train, TestRecognize, PrintGestureFeatures,
			Help };
		static public void Main(string[] args)
		{
			Command c = Recognizer.Command.Help;
			if ( args.Length > 0 ) {
				c = (Command)Enum.Parse(typeof(Recognizer.Command), args[0]);
			}
			if ( c == Recognizer.Command.Help ) {
				Console.WriteLine("Usage:\n\tFinalProject.exe command [filename]\n\nCommands:");
				foreach ( var cm in Enum.GetNames(typeof(Recognizer.Command)) ) Console.WriteLine(cm);
				return;
			}
			
			////////////////////
			// CHANGE THIS LINE TO USE A NEW RECOGNIZER
			////////////////////
			IRecognizer rec = new SingleFrameRecognizer();
			
			
			string filename = "gestures/track_high_kick_01.log";
			if ( args.Length > 1 ) filename = args[1];

			switch ( c ) {
			case Command.TestRecognize:
				var result = rec.RecognizeSingleGesture(new InputGesture(new LogFileLoader(filename)));
				Console.WriteLine(result.ToString());
				break;
				
			case Command.PrintGestureFeatures:
				var gest = new InputGesture(new LogFileLoader(filename));
				foreach ( var f in Features.AllFeatures.SingleGestureFeatures ) {
					Console.WriteLine("{0}: {1}", f.ToString(), f.QueryGesture(gest));
				}
				break;
			}
		}
	}
}

