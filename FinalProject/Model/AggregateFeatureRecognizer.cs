using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalProject
{
	public class AggregateFeatureRecognizer : IRecognizer
	{
		Dictionary<string, float[]> mWeights;
		
		public AggregateFeatureRecognizer ()
		{
			mWeights = new Dictionary<string, float[]>();
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
				return weight.CompareTo(other.weight);
			}
		}
		public RecognizerResult RecognizeSingleGesture (InputGesture g)
		{
			var results = new List<GestureWeight>();
			foreach ( var kvp in mWeights ) {
				GestureWeight gw = new GestureWeight(){name = kvp.Key};
				for ( int i = 0; i < Features.AllFeatures.SingleGestureFeatures.Count; i++ ) {
					bool fp = Features.AllFeatures.SingleGestureFeatures[i].QueryGesture(g);
					if ( fp ) gw.weight += kvp.Value[i*2];
					else gw.weight += kvp.Value[i*2+1];
				}
				results.Add(gw);
			}
			
			results.Sort();
			results.Reverse();
			float norm = (float)Features.AllFeatures.SingleGestureFeatures.Count;
			return new RecognizerResult() {
				Gesture1 = results[0].name, Confidence1 = results[0].weight / norm,
				Gesture2 = results[1].name, Confidence2 = results[1].weight / norm,
				Gesture3 = results[2].name, Confidence3 = results[2].weight / norm
			};
		}

		public void Train (IDictionary<string, IList<InputGesture>> gestures)
		{
			foreach ( var kvp in gestures ) {
				Console.WriteLine("Training gesture \"{0}\" ({1} data instances)", kvp.Key, kvp.Value.Count);
				float[] weights = new float[2*Features.AllFeatures.SingleGestureFeatures.Count];
				
				// TODO: slow and ugly...
				for ( int i = 0; i < Features.AllFeatures.SingleGestureFeatures.Count; i++ ) {
					foreach ( var ig in kvp.Value ) {
						bool fp = Features.AllFeatures.SingleGestureFeatures[i].QueryGesture(ig);
						if ( fp ) weights[i*2] += 1.0f;
						else weights[i*2+1] += 1.0f;
					}
					weights[i*2] /= (float)kvp.Value.Count;
					weights[i*2+1] /= (float)kvp.Value.Count;
				}
				
				mWeights.Add(kvp.Key, weights);
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
			mWeights = (Dictionary<string, float[]>)formatter.Deserialize(stream);
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

