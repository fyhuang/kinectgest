using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace FinalProject
{
	public class Recognizer
	{
		IRecognizer mRec;
		string mRecFilename;
		ISegmenter mSeg;
		string mSegFilename;
		
		CrossValidation mCV;
		string[] mGestureNames;
		
		JointVisualizer mVisWindow;
		
		public Recognizer() {
			//mGestureNames = new string[] {"clap", "flick_left", "flick_right", "jump", "low_kick", "punch", "throw"};
			mGestureNames = new string[] {"clap", "flick_left", "flick_right", "high_kick", "jump", "low_kick", "punch", "throw", "wave"};
		}
		
		IDictionary<string, IList<InputGesture>> LoadData(string[] names, bool training) {
			return LoadData(names, training, "gestures/track_{0}_{1:00}.log");
		}
		IDictionary<string, IList<InputGesture>> LoadData(string[] names, bool training, string format) {
			var output = new Dictionary<string, IList<InputGesture>>();
			foreach ( var name in names ) {
				output.Add(name, new List<InputGesture>());
				var fnames = LogFileLoader.LogFilenames(name, format).ToList();
				int last = fnames.Count;
				Console.WriteLine("{0} has {1} instances", name, last);
				
				for ( int i = 0; i < last; i++ ) {
					if ( training ) {
						if ( !mCV.IsTraining(i, last) ) continue;
					}
					else {
						if ( mCV.IsTraining(i, last) ) continue;
					}
					output[name].Add(new InputGesture(new LogFileLoader(fnames[i])));
				}
			}
			
			//System.Console.WriteLine("Done loading training data");
			Utility.PrintMemoryUsage();
			return output;
		}
		
		IEnumerable<JointState> LoadFrames(string name)
		{
			return LoadData(new[]{"ns"}, true, "gestures/frames/{0}_{1:00}.log")
				.SelectMany(x => x.Value)
				.SelectMany(x => x.States);
		}
		
		void StartVisualize() {
			lock ( this ) {
				mVisWindow = new JointVisualizer();
			}
			mVisWindow.Run();
		}
		
		void LoadModels() {
			Console.WriteLine("Using cross-validation index {0}", mCV.Index);
			
			mRec.LoadModel(mRecFilename);
			
			Features.AllFeatures.LoadModels();
		}
		
		void Train() {
			Console.WriteLine("Using cross-validation index {0}", mCV.Index);
			
			Console.WriteLine("Training recognizer");
			mRec.Train(LoadData(mGestureNames, true));
			mRec.SaveModel(mRecFilename);
			
			Features.AllFeatures.LearnedFrameFeatures["NeutralStance"].Train(LoadFrames("ns"));
			Features.AllFeatures.SaveModels();
		}
		
		enum Command { Train, TestSingle, TestRecognize, PrintFeatures, TestRealtime, RunRealtime,
			BenchmarkRecognize, CycleCV, Help };
		public void Run(string[] args)
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
			mCV = new CrossValidation("cvindex.txt");
			
			////////////////////
			// CHANGE THESE LINES TO SWAP MODELS
			////////////////////
			mRec = new LogisticRegressionRecognizer();
			mSeg = new DumbSegmenter();
			
			Console.WriteLine("Using recognizer {0}", mRec.GetType().ToString());
			Console.WriteLine("Using segmenter {0}", mSeg.GetType().ToString());
			mRecFilename = "models/" + mRec.GetType().ToString() + ".rec.model";
			mSegFilename = "models/" + mSeg.GetType().ToString() + ".seg.model";
			
			
			string filename = "gestures/track_high_kick_01.log";
			if ( args.Length > 1 ) filename = args[1];
			
			InputGesture gest;
			switch ( c ) {
			case Command.Train:
				Train();
				break;
			case Command.TestSingle:
				LoadModels();
				var result = mRec.RecognizeSingleGesture(new InputGesture(new LogFileLoader(filename)));
				Console.WriteLine(result.ToString());
				break;
			case Command.TestRecognize:
				LoadModels();
				var test_gestures = LoadData(mGestureNames, false);
				int[] total = Enumerable.Range(0, test_gestures.Count).Select(x => 0).ToArray(),
					  correct = Enumerable.Range(0, test_gestures.Count).Select(x => 0).ToArray();
				int i = 0;
				Console.WriteLine();
				foreach ( var gn in test_gestures ) {
					foreach ( var tg in gn.Value ) {
						total[i]++;
						var result2 = mRec.RecognizeSingleGesture(tg);
						if ( result2.Gesture1 == gn.Key ) {
							correct[i]++;
						}
					}
					Console.WriteLine("{0}: {1} correct / {2} total = {3}% correct",
					                  gn.Key,
					                  correct[i], total[i], (float)correct[i] / (float)total[i] * 100.0f);
					i++;
				}
				
				Console.WriteLine("TEST RESULTS:\n\t{0} correct / {1} total = {2}% correct",
				                  correct.Sum(), total.Sum(), (float)correct.Sum() / (float)total.Sum() * 100.0f);
				Utility.PrintMemoryUsage();
				break;
				
			case Command.TestRealtime:
				LoadModels();
				gest = new InputGesture(new LogFileLoader(filename));
				mSeg.GestureSegmented += delegate(object sender, EventArgs e) {
					var segm = ((ISegmenter)sender).LastGesture;
					var recres = mRec.RecognizeSingleGesture(segm);
					if ( recres.Confidence1 > 0.5f ) {
						Console.WriteLine("Recognized gesture:\n{0}", recres.ToString());
					}
					else {
						Console.WriteLine("Inconclusive");
					}
				};
				
				var visthread = new System.Threading.Thread(new System.Threading.ThreadStart(this.StartVisualize));
				visthread.Start();
				while ( true ) { // Wait for window to get created
					lock ( this ) {
						if ( mVisWindow != null ) break;
					}
				}
				
				float lastTime = gest.States[0].Timestamp;
				foreach ( var frame in gest.States ) {
					mSeg.AddState(frame);
					mVisWindow.CurrState = frame;
					System.Threading.Thread.Sleep((int)((frame.Timestamp - lastTime) * 750.0f));
					lastTime = frame.Timestamp;
				}
				mSeg.Finish();
				
				visthread.Abort();
				break;
				
			case Command.PrintFeatures:
				gest = new InputGesture(new LogFileLoader(filename));
				/*foreach ( var f in Features.AllFeatures.SingleGestureFeatures ) {
					Console.WriteLine("{0}: {1}", f.ToString(), f.QueryGesture(gest));
				}*/
				foreach ( var f in Features.AllFeatures.GestureFeatures ) {
					Console.WriteLine("{0}: {1}", f.ToString(), f.QueryGesture(gest));
				}
				break;
				
			case Command.BenchmarkRecognize:
				Benchmarks.BenchmarkRecognize();
				break;
				
			case Command.CycleCV:
				mCV.Incr();
				mCV.Save();
				break;
			}
		}
		
		
		static public void Main(string[] args)
		{
			var r = new Recognizer();
			r.Run(args);
		}
	}
}

