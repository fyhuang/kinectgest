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
		
		static bool IsTraining(int ix, int cvix, int max) {
			if ( (ix + cvix) % (max / 4 + 1) == 0 ) return false;
			return true;
		}
		
		static IDictionary<string, IList<InputGesture>> LoadTrainingData(int cvix, string[] names) {
			var output = new Dictionary<string, IList<InputGesture>>();
			foreach ( var name in names ) {
				output.Add(name, new List<InputGesture>());
				int last = Enumerable.Range(1,100).First(x => !File.Exists(String.Format("gestures/track_{0}_{1:00}.log", name, x))) - 1;
				Console.WriteLine("{0} has {1} instances", name, last);
				
				for ( int i = 0; i < last; i++ ) {
					if ( !IsTraining(i, cvix, last) ) continue;
					output[name].Add(new InputGesture(new LogFileLoader(String.Format("gestures/track_{0}_{1:00}.log", name, i))));
				}
			}
			
			System.Console.WriteLine("Done loading training data");
			return output;
		}
		
		static IDictionary<string, IList<InputGesture>> LoadTestData(int cvix, string[] names) {
			var output = new Dictionary<string, IList<InputGesture>>();
			foreach ( var name in names ) {
				output.Add(name, new List<InputGesture>());
				int last = Enumerable.Range(1,100).First(x => !File.Exists(String.Format("gestures/track_{0}_{1:00}.log", name, x))) - 1;
				Console.WriteLine("{0} has {1} instances", name, last);
				
				for ( int i = 0; i < last; i++ ) {
					if ( IsTraining(i, cvix, last) ) continue;
					output[name].Add(new InputGesture(new LogFileLoader(String.Format("gestures/track_{0}_{1:00}.log", name, i))));
				}
			}
			
			System.Console.WriteLine("Done loading test data");
			return output;
		}
		
		static int GetCVIndex() {
			var file = new StreamReader(File.Open("cvindex.txt", FileMode.OpenOrCreate));
			try {
				int ix = int.Parse(file.ReadToEnd());
				return ix;
			}
			catch ( Exception ) {
				return 0;
			}
			finally {
				file.Close();
			}
		}
		
		static void SaveCVIndex(int ix) {
			var file = new StreamWriter(File.Open("cvindex.txt", FileMode.Create));
			file.Write(ix);
			file.Close();
		}
		
		enum Command { Train, TestSingle, TestRecognize, PrintGestureFeatures, BenchmarkRecognize,
			CycleCrossValidation,
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
			
			// For CV
			int cv_index = GetCVIndex();
			
			////////////////////
			// CHANGE THIS LINE TO USE A NEW RECOGNIZER
			////////////////////
			IRecognizer rec = new AggregateFeatureRecognizer();
			Console.WriteLine("Using recognizer {0}", rec.GetType().ToString());
			string model_filename = rec.GetType().ToString() + ".model";
			
			
			string filename = "gestures/track_high_kick_01.log";
			if ( args.Length > 1 ) filename = args[1];
			
			string[] trainingNames = {"high_kick", "punch", "throw", "clap", "jump", "flick_right"};

			switch ( c ) {
			case Command.Train:
				Console.WriteLine("Using cross-validation index {0}", cv_index);
				rec.Train(LoadTrainingData(cv_index, trainingNames));
				rec.SaveModel(model_filename);
				System.Console.WriteLine("Saved trained model to {0}", model_filename);
				break;
			case Command.TestSingle:
				rec.LoadModel(model_filename);
				Console.WriteLine("Loaded model from {0}", model_filename);
				var result = rec.RecognizeSingleGesture(new InputGesture(new LogFileLoader(filename)));
				Console.WriteLine(result.ToString());
				break;
			case Command.TestRecognize:
				Console.WriteLine("Using cross-validation index {0}", cv_index);
				rec.LoadModel(model_filename);
				Console.WriteLine("Loaded model from {0}", model_filename);
				
				int total = 0, correct = 0;
				var test_gestures = LoadTestData(cv_index, trainingNames);
				foreach ( var gn in test_gestures ) {
					foreach ( var tg in gn.Value ) {
						total++;
						var result2 = rec.RecognizeSingleGesture(tg);
						if ( result2.Gesture1 == gn.Key ) {
							correct++;
						}
					}
				}
				
				Console.WriteLine("\nTEST RESULTS:\n\t{0} correct / {1} total = {2}% correct",
				                  correct, total, (float)correct / (float)total * 100.0f);
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
				
			case Command.CycleCrossValidation:
				SaveCVIndex(cv_index+1);
				break;
			}
		}
	}
}

