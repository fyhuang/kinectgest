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
		double mVariance;
		OpenTK.Vector3[] mAverage;
		
		public void Train (IEnumerable<JointState> states)
		{
			mVariance = 0.0f;
			
			var slist = states.ToList();
			mAverage = (OpenTK.Vector3[])slist[0].RelativeJoints.Clone();
			for ( int j = 0; j < mAverage.Length; j++ ) {
				mAverage[j] = new OpenTK.Vector3(slist.Average(x => x.RelativeJoints[j].X),
				                      slist.Average(x => x.RelativeJoints[j].Y),
				                      slist.Average(x => x.RelativeJoints[j].Z));
				double vc = slist.Select(x => (x.RelativeJoints[j] - mAverage[j]).LengthFast).Select(x => x*x).Sum();
				mVariance = Math.Max(vc, mVariance);
			}
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
			mVariance = (double)formatter.Deserialize(stream);
			mAverage = (OpenTK.Vector3[])formatter.Deserialize(stream);
			stream.Close();
			
			Console.WriteLine("Loaded model from {0}", filename);
		}

		public float QueryFrame (JointState js)
		{
			throw new NotImplementedException ();
		}
		
		public override string ToString ()
		{
			return string.Format ("NeutralStance");
		}
	}
}

