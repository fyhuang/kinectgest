using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace FinalProject
{
	public class Recognizer
	{
		static void BenchmarkRecognize()
		{
			var sw = new Stopwatch();
			var gest = new InputGesture(new LogFileLoader("gestures/track_high_kick_01.log"));
			var numiters = 1000;
			
			sw.Start();
			for ( int i = 0; i < numiters; i++ ) {
				foreach ( var f in Features.AllFeatures.SingleGestureFeatures ) {
					f.QueryGesture(gest);
				}
			}
			sw.Stop();
			
			Console.WriteLine("Elapsed time ({0} iters): {1}", numiters, sw.Elapsed);
			Console.WriteLine("Gestures per second: {0}", 1000.0f / (sw.ElapsedMilliseconds / (float)numiters));
		}
		
		static IDictionary<string, IList<InputGesture>> LoadTrainingData(string[] names) {
			var output = new Dictionary<string, IList<InputGesture>>();
			foreach ( var name in names ) {
				output.Add(name, new List<InputGesture>());
				int last = Enumerable.Range(1,100).Last(x => File.Exists(String.Format("gestures/track_{0}_{1:00}.log", name, x)));
				Console.WriteLine("{0} has {1} instances", name, last);
				for ( int i = 0; i < last - 2; i++ ) { // -2 is to set aside some for testing set
					output[name].Add(new InputGesture(new LogFileLoader(String.Format("gestures/track_{0}_{1:00}.log", name, i))));
				}
			}
			
			System.Console.WriteLine("Done loading training data");
			return output;
		}
		
		enum Command { Train, TestRecognize, PrintGestureFeatures, BenchmarkRecognize,
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
			IRecognizer rec = new AggregateFeatureRecognizer();
			Console.WriteLine("Using recognizer {0}", rec.GetType().ToString());
			string model_filename = rec.GetType().ToString() + ".model";
			
			
			string filename = "gestures/track_high_kick_01.log";
			if ( args.Length > 1 ) filename = args[1];

			switch ( c ) {
			case Command.Train:
				string[] trainingNames = {"high_kick", "punch", "throw", "clap", "jump", "flick_right"};
				rec.Train(LoadTrainingData(trainingNames));
				rec.SaveModel(model_filename);
				System.Console.WriteLine("Saved trained model to {0}", model_filename);
				break;
			case Command.TestRecognize:
				rec.LoadModel(model_filename);
				Console.WriteLine("Loaded model from {0}", model_filename);
				var result = rec.RecognizeSingleGesture(new InputGesture(new LogFileLoader(filename)));
				Console.WriteLine(result.ToString());
				break;
				
			case Command.PrintGestureFeatures:
				var gest = new InputGesture(new LogFileLoader(filename));
				foreach ( var f in Features.AllFeatures.SingleGestureFeatures ) {
					Console.WriteLine("{0}: {1}", f.ToString(), f.QueryGesture(gest));
				}
				break;
				
			case Command.BenchmarkRecognize:
				BenchmarkRecognize();
				break;
			}
		}
	}
}

