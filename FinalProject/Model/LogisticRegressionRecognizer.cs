using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalProject
{
	public class LogisticRegressionRecognizer : IRecognizer
	{
		Dictionary<string, List<double>> mWeights;
		double mStepSize;
		
		public LogisticRegressionRecognizer()
		{
			mWeights = new Dictionary<string, List<double>>();
			mStepSize = 0.05;
		}
		
		public string[] Gestures {
			get {
				return mWeights.Keys.ToArray();
			}
		}
	
		struct GestureWeight : IComparable<GestureWeight> {
			public string name;
			public double weight;
			
			public int CompareTo (GestureWeight other) {
				return other.weight.CompareTo(this.weight);
			}
		}
		public RecognizerResult RecognizeSingleGesture (InputGesture g)
		{
			List<IGestureFeature> features = Features.AllFeatures.GestureFeatures;
			
			var results = new List<GestureWeight>();
			foreach ( var kvp in mWeights ) {
				GestureWeight gw = new GestureWeight(){name = kvp.Key};
				for ( int i = 0; i < features.Count; i++ ) {
					double fres = features[i].QueryGesture(g);
					gw.weight += kvp.Value[i] * fres;
				}
				gw.weight += kvp.Value[features.Count];
				gw.weight = 1.0f / (1.0f + Math.Exp(-gw.weight));
				results.Add(gw);
			}
			
			results.Sort();
			return new RecognizerResult() {
				Gesture1 = results[0].name, Confidence1 = (float)results[0].weight,
				Gesture2 = results[1].name, Confidence2 = (float)results[1].weight,
				Gesture3 = results[2].name, Confidence3 = (float)results[2].weight
			};
		}
		
		double _Sigmoid(List<double> weights, double[] feature_results) {
			double sum = 0.0;
			for ( int i = 0; i < weights.Count; i++ ) {
				sum += weights[i] * feature_results[i];
			}
			return 1.0f / (1.0f + Math.Exp(-sum));
		}
		
		double _Diff(List<double> oldWeights, List<double> weights) {
			double result = 0.0;
			for ( int i = 0; i < oldWeights.Count; i++ ) {
				result += Math.Abs(oldWeights[i] - weights[i]);
			}
			return result;
		}
		
		bool _Converged(List<double> oldWeights, List<double> weights, double threshold) {
			for ( int i = 0; i < oldWeights.Count; i++ ) {
				if ( Math.Abs(oldWeights[i] - weights[i]) > threshold ) return false;
			}
			return true;
		}

		public void Train (IDictionary<string, IList<InputGesture>> gestures)
		{
			const double threshold = 0.1;
			
			List<IGestureFeature> features = Features.AllFeatures.GestureFeatures;
			var allgestures = new List<KeyValuePair<string, InputGesture>>();
			foreach ( var kvp in gestures ) {
				foreach ( var instance in kvp.Value ) {
					allgestures.Add(new KeyValuePair<string, InputGesture>(kvp.Key, instance));
				}
			}
			
			
			var feature_results = new double[gestures.Values.Sum(x => x.Count)][];
			for ( int i = 0; i < allgestures.Count; i++ ) {
				var temp = features.Select(x => x.QueryGesture(allgestures[i].Value)).Select(x => (double)x).ToList();
				temp.Add(1.0f);
				feature_results[i] = temp.ToArray();
			}
			
			int max_iters = 0;
			
			foreach ( var kvp in gestures ) {
				mWeights[kvp.Key] = Enumerable.Range(0, features.Count + 1).Select(x => 1.0).ToList(); // The features.Count-indexed value is the "intercept"
				var oldWeights = new List<double>(features.Count + 1);
				
				var sw = new System.Diagnostics.Stopwatch(); sw.Start();
				var num_iters = 0;
				bool converged = true;
				//float diff = 0.0f;
				do {
					if ( max_iters > 0 && num_iters > 10 * max_iters ) {
						Console.WriteLine("WARNING: {0} not converging", kvp.Key);
						converged = false;
						break;
					}
					
					oldWeights.Clear();
					oldWeights.AddRange(mWeights[kvp.Key]);
					
					for ( int i = 0; i < features.Count + 1; i++ ) {
						var error_sum = 0.0;
						for ( int j = 0; j < allgestures.Count; j++ ) {
							var g = allgestures[j];
							error_sum += ( ((g.Key == kvp.Key) ? 1.0 : 0.0 ) -
						           	     _Sigmoid(mWeights[kvp.Key], feature_results[j]) ) *
								feature_results[j][i];
						}
						mWeights[kvp.Key][i] += mStepSize * error_sum;
					}
					
					num_iters++;
					//diff = _Diff(oldWeights, mWeights[kvp.Key]);
					//Console.WriteLine("Iteration {0}, diff {1}", num_iters, diff);
				} while ( !_Converged(oldWeights, mWeights[kvp.Key], threshold * mStepSize) );
				
				Console.Write("{0}: {1} iterations in {2}\n  Weights: ", kvp.Key, num_iters, sw.Elapsed);
				mWeights[kvp.Key].ForEach(x => Console.Write("{0:0.000} ", x));
				Console.WriteLine();
				
				if ( converged ) max_iters = Math.Max(num_iters, max_iters);
			}
		}

		public void SaveModel (string filename)
		{
			var stream = File.Open(filename, FileMode.Create);
			var formatter = new BinaryFormatter();
			formatter.Serialize(stream, this.GetType());
			formatter.Serialize(stream, mWeights);
			stream.Close();
		}

		public void LoadModel (string filename)
		{
			var stream = File.Open(filename, FileMode.Open);
			var formatter = new BinaryFormatter();
			var mtype = (Type)formatter.Deserialize(stream);
			if ( !mtype.Equals(this.GetType()) ) throw new InvalidDataException();
			mWeights = (Dictionary<string, List<double>>)formatter.Deserialize(stream);
			stream.Close();
		}

		public void ClearHistory ()
		{
			throw new NotImplementedException ();
		}

		public RecognizerResult AddNewData (JointState js)
		{
			throw new NotImplementedException ();
		}
	}
}

