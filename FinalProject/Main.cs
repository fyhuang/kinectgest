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
		}
	}
}

