using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalProject.Features
{
	public class NeutralStance : ILearnedFrameFeature
	{
		float[] mVariance;
		float[] mAverage;
		readonly float[] mWeights;
		
		public NeutralStance() {
			mWeights = new float[] {
				0.0f,
				0.1f, // Head
				0.2f, // R shoulder
				2.0f,
				5.0f,
				0.1f,
				0.2f, // L shoulder
				2.0f,
				5.0f,
				0.1f,
				0.1f, // R hip
				1.0f,
				1.0f,
				0.1f,
				0.1f, // L hip
				1.0f,
				1.0f,
				0.1f,
				0.1f, // R pelvis
				0.1f
			};
		}

		public float QueryFrame (JointState js)
		{
			var error_sum = 0.0f;
			for ( int i = 0; i < js.RelativeAngles.Length; i++ ) {
				var error = (js.RelativeAngles[i] - mAverage[i]);
				error_sum += 1.0f - (Utility.Sigmoid(Math.Abs(error) - mVariance[i]) * mWeights[i]);
			}
			var total_error = error_sum / (float)mWeights.Length;
			Console.WriteLine("Total error: {0}", total_error);
			return total_error;
		}
		
		public void Train (IEnumerable<JointState> states)
		{
			var slist = states.ToList();
			mVariance = new float[slist[0].RelativeAngles.Length];
			mAverage = (float[])slist[0].RelativeAngles.Clone();
			for ( int j = 0; j < mAverage.Length; j++ ) {
				mAverage[j] = slist.Average(x => x.RelativeAngles[j]);
				float vc = slist.Select(x => (x.RelativeAngles[j] - mAverage[j])).Select(x => x*x).Sum();
				mVariance[j] = vc;
			}
			
			Console.Write("Variances: ");
			foreach ( var v in mVariance ) Console.Write("{0} ", v);
			Console.WriteLine();
		}

		public void SaveModel (string filename)
		{
			var stream = File.Open(filename, FileMode.Create);
			var formatter = new BinaryFormatter();
			formatter.Serialize(stream, this.GetType());
			formatter.Serialize(stream, mVariance);
			formatter.Serialize(stream, mAverage);
			stream.Close();
			
			Console.WriteLine("Saved trained model to {0}", filename);
		}

		public void LoadModel (string filename)
		{
			var stream = File.Open(filename, FileMode.Open);
			var formatter = new BinaryFormatter();
			var mtype = (Type)formatter.Deserialize(stream);
			if ( !mtype.Equals(this.GetType()) ) throw new InvalidDataException();
			mVariance = (float[])formatter.Deserialize(stream);
			mAverage = (float[])formatter.Deserialize(stream);
			stream.Close();
			
			Console.WriteLine("Loaded model from {0}", filename);
		}
		
		public override string ToString ()
		{
			return string.Format ("NeutralStance");
		}
	}
}

