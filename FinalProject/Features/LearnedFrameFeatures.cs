using System;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using OpenTK;

namespace FinalProject.Features
{
	public class NeutralStance : ILearnedFrameFeature
	{
		float[] mVariance;
		Vector3[] mAverage;
		readonly float[] mWeights;
		
		public NeutralStance() {
			mWeights = new float[] {
				0.1f, // Neck
				0.1f, // Head
				1.0f, // R shoulder
				3.0f,
				4.0f,
				5.0f,
				1.0f, // L shoulder
				3.0f,
				4.0f,
				5.0f,
				1.0f, // R hip
				2.0f,
				3.0f,
				4.0f,
				1.0f, // L hip
				2.0f,
				3.0f,
				4.0f,
				0.1f, // R pelvis
				0.1f
			};
		}
		
		Vector3 Parent(JointState js, int i) {
			if ( i >= 3 && i <= 5 ) return js.Pos("right-shoulder");
			else if ( i >= 7 && i <= 9 ) return js.Pos("left-shoulder");
			else if ( i >= 11 && i <= 13 ) return js.Pos("right-hip");
			else if ( i >= 15 && i <= 17 ) return js.Pos("left-hip");
			return js.NeckPos;
		}

		public float QueryFrame (JointState js)
		{
			/*var error_sum = 0.0f;
			for ( int i = 0; i < js.RelativeJoints.Length; i++ ) {
				var reljoint = (js.RelativeJoints[i] - Parent(js, i));
				reljoint.NormalizeFast();
				var error = (reljoint - mAverage[i]).LengthFast;
				if ( error < mVariance[i] ) error_sum += 0.0f;
				else error_sum += mWeights[i] * (error - mVariance[i]);
				//error_sum += (1.0f - Utility.Sigmoid(error.LengthSq * mWeights[i] - mVariance[i] - 2.0f)) * mWeights[i];
			}
			var total_error = 1.0f - error_sum / mWeights.Sum();
			Console.WriteLine("Total error: {0}", total_error);
			return total_error;*/

			var error_sum = 0.0f;
			for ( int i = 0; i < js.RelativeJoints.Length; i++ ) {
				var reljoint = (js.RelativeJoints[i] - Parent(js, i));
				reljoint.NormalizeFast();
				var error = (reljoint - mAverage[i]).LengthFast / 2.0f;
				error_sum += (float)Math.Pow(error, 1.0f/mWeights[i]);
			}
			var conf = 1.0f - error_sum / mVariance.Sum() / mWeights.Sum();
			//Console.WriteLine("Confidence: {0}", conf);
			return conf;
		}
		
		public void Train (IEnumerable<JointState> states)
		{
			var slist = states.ToList();
			mVariance = new float[slist[0].RelativeJoints.Length];
			mAverage = (Vector3[])slist[0].RelativeJoints.Clone();
			for ( int j = 0; j < mAverage.Length; j++ ) {
				var relposes = slist.Select(x => x.RelativeJoints[j] - Parent(x, j))
					.Select(y => y / y.LengthFast).ToList();
				mAverage[j] = new Vector3();
				relposes.ForEach(x => mAverage[j] += x);
				mAverage[j] /= (float)relposes.Count;
				
				float vc = relposes.Select(x => (x - mAverage[j]).LengthFast).Select(x => x*x).Sum();
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
			mAverage = (Vector3[])formatter.Deserialize(stream);
			stream.Close();
			
			Console.WriteLine("Loaded model from {0}", filename);
		}
		
		public override string ToString ()
		{
			return string.Format ("NeutralStance");
		}
	}
}

