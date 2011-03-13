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
		Dictionary<string, List<float>> mWeights;
		float mStepSize;
		
		public LogisticRegressionRecognizer()
		{
			mWeights = new Dictionary<string, List<float>>();
			mStepSize = 0.05f;
		}
		
		public string[] Gestures {
			get {
				return mWeights.Keys.ToArray();
			}
		}
	
		struct GestureWeight : IComparable<GestureWeight> {
			public string name;
			public float weight;
			
			public int CompareTo (GestureWeight other) {
				return other.weight.CompareTo(this.weight);
			}
		}
		public RecognizerResult RecognizeSingleGesture (InputGesture g)
		{
			List<IContinuousGestureFeature> features = Features.AllFeatures.ContinuousGestureFeatures;
			
			var results = new List<GestureWeight>();
			foreach ( var kvp in mWeights ) {
				GestureWeight gw = new GestureWeight(){name = kvp.Key};
				for ( int i = 0; i < features.Count; i++ ) {
					float fres = features[i].QueryGesture(g);
					gw.weight += kvp.Value[i] * fres;
				}
				gw.weight += kvp.Value[features.Count];
				gw.weight = 1.0f / (1.0f + (float)Math.Exp(-gw.weight));
				results.Add(gw);
			}
			
			results.Sort();
			return new RecognizerResult() {
				Gesture1 = results[0].name, Confidence1 = results[0].weight,
				Gesture2 = results[1].name, Confidence2 = results[1].weight,
				Gesture3 = results[2].name, Confidence3 = results[2].weight
			};
		}
		
		float _Sigmoid(List<float> weights, float[] feature_results) {
			float sum = 0.0f;
			for ( int i = 0; i < weights.Count; i++ ) {
				sum += weights[i] * feature_results[i];
			}
			return 1.0f / (1.0f + (float)Math.Exp(-sum));
		}
		
		float _Diff(List<float> oldWeights, List<float> weights) {
			float result = 0.0f;
			for ( int i = 0; i < oldWeights.Count; i++ ) {
				result += Math.Abs(oldWeights[i] - weights[i]);
			}
			return result;
		}
		
		bool _Converged(List<float> oldWeights, List<float> weights, float threshold) {
			for ( int i = 0; i < oldWeights.Count; i++ ) {
				if ( Math.Abs(oldWeights[i] - weights[i]) > threshold ) return false;
			}
			return true;
		}

		public void Train (IDictionary<string, IList<InputGesture>> gestures)
		{
			const float threshold = 0.1f;
			
			List<IContinuousGestureFeature> features = Features.AllFeatures.ContinuousGestureFeatures;
			var allgestures = new List<KeyValuePair<string, InputGesture>>();
			foreach ( var kvp in gestures ) {
				foreach ( var instance in kvp.Value ) {
					allgestures.Add(new KeyValuePair<string, InputGesture>(kvp.Key, instance));
				}
			}
			
			
			var feature_results = new float[gestures.Values.Sum(x => x.Count)][];
			for ( int i = 0; i < allgestures.Count; i++ ) {
				var temp = features.Select(x => x.QueryGesture(allgestures[i].Value)).ToList();
				temp.Add(1.0f);
				feature_results[i] = temp.ToArray();
			}
			
			foreach ( var kvp in gestures ) {
				mWeights[kvp.Key] = Enumerable.Range(0, features.Count + 1).Select(x => 1.0f).ToList(); // The features.Count-indexed value is the "intercept"
				var oldWeights = new List<float>(features.Count + 1);
				
				var sw = new System.Diagnostics.Stopwatch(); sw.Start();
				var num_iters = 0;
				//float diff = 0.0f;
				do {
					oldWeights.Clear();
					oldWeights.AddRange(mWeights[kvp.Key]);
					
					for ( int i = 0; i < features.Count + 1; i++ ) {
						var error_sum = 0.0f;
						for ( int j = 0; j < allgestures.Count; j++ ) {
							var g = allgestures[j];
							error_sum += ( ((g.Key == kvp.Key) ? 1.0f : 0.0f ) -
						           	     _Sigmoid(mWeights[kvp.Key], feature_results[j]) ) *
								feature_results[j][i];
						}
						mWeights[kvp.Key][i] += mStepSize * error_sum;
					}
					
					num_iters++;
					//diff = _Diff(oldWeights, mWeights[kvp.Key]);
					//Console.WriteLine("Iteration {0}, diff {1}", num_iters, diff);
				} while ( !_Converged(oldWeights, mWeights[kvp.Key], threshold * mStepSize) );
				
				Console.Write("{0}: {1} iterations converged in {2}\n  Weights: ", kvp.Key, num_iters, sw.Elapsed);
				mWeights[kvp.Key].ForEach(x => Console.Write("{0} ", x));
				Console.WriteLine();
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
			mWeights = (Dictionary<string, List<float>>)formatter.Deserialize(stream);
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

