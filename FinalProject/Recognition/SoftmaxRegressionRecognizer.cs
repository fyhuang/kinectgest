using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FinalProject
{
    using LabeledGesture = Tuple<string, InputGesture>;

    class SoftmaxRegressionRecognizer : IRecognizer
    {
        Dictionary<string, List<double>> mWeights;
        double mStepSize;
        double mConvergenceThreshold;

        public SoftmaxRegressionRecognizer()
        {
            // the last double in each list of weights is the intercept weight
            // so all lists are length (n+1) where n is the number of features
            mWeights = new Dictionary<string, List<double>>();
            mStepSize = 0.1;
            mConvergenceThreshold = 0.2;
        }

        double _Sorta_sigmoid(List<double> weights, float[] feature_results)
        {
            double sum = 0.0f;
            for (int i = 0; i < weights.Count; i++)
                sum += weights[i] * feature_results[i];
            return Math.Exp(sum);
        }

        bool _Completely_converged(Dictionary<string, List<double>> oldWeights,
                                   Dictionary<string, List<double>> newWeights,
                                   double threshold)
        {
            double error_sum = _Sum_weight_diff(oldWeights, newWeights);
            if (error_sum > mStepSize * mConvergenceThreshold)
                return false;
            return true;
        }

        double _Sum_weight_diff(Dictionary<string, List<double>> oldWeights,
                                Dictionary<string, List<double>> newWeights)
        {
            double error_sum = 0;
            foreach (var kvp in oldWeights)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                    error_sum += Math.Abs(oldWeights[kvp.Key][i] - newWeights[kvp.Key][i]);
            }
            return error_sum;
        }

        #region IRecognizer Members

        public string[] Gestures
        {
            get { throw new NotImplementedException(); }
        }

        class GestureWeight : IComparable<GestureWeight> {
			public string name;
			public double weight;
			
			public int CompareTo (GestureWeight other) {
				return other.weight.CompareTo(this.weight);
			}
		}

        public RecognizerResult RecognizeSingleGesture(InputGesture g)
        {
            List<IGestureFeature> features = Features.AllFeatures.GestureFeatures;
			
			var results = new List<GestureWeight>();
            foreach (var kvp in mWeights)
            {
                GestureWeight gw = new GestureWeight() { name = kvp.Key };
                gw.weight = kvp.Value[features.Count];
                for (int i = 0; i < features.Count; i++)
                {
                    float fres = features[i].QueryGesture(g);
                    gw.weight += kvp.Value[i] * fres;
                }
                gw.weight = Math.Exp(gw.weight);
                results.Add(gw);
            }
            double total_weight = 0;
            for (int i = 0; i < results.Count; i++)
                total_weight += results[i].weight;
            for (int i = 0; i < results.Count; i++)
                results[i].weight = 1.0 * results[i].weight / total_weight;

            results.Sort();
            return new RecognizerResult()
            {
                Gesture1 = results[0].name, Confidence1 = (float)results[0].weight,
                Gesture2 = results[1].name, Confidence2 = (float)results[1].weight,
                Gesture3 = results[2].name, Confidence3 = (float)results[2].weight
            };
        }

        public void ClearHistory()
        {
            throw new NotImplementedException();
        }

        public RecognizerResult AddNewData(JointState js)
        {
            throw new NotImplementedException();
        }

        public void Train(IDictionary<string, IList<InputGesture>> gestures)
        {
            // also serves as output array
            // list of tuples in the form of (class label, gesture sequence)
            var allgestures = new List<LabeledGesture>();
            foreach (var kvp in gestures)
            {
                foreach (var instance in kvp.Value)
                {
                    allgestures.Add(new Tuple<string, InputGesture>(kvp.Key, instance));
                }
            }

            // initialize input matrix
            // first dimension is each gesture file, second is index of features
            var feature_results = new float[allgestures.Count][];
            for (int i = 0; i < allgestures.Count; i++)
            {
                var temp = Features.AllFeatures.GestureFeatureResults(allgestures[i].Item2).ToList();
                temp.Add(1.0f);
                feature_results[i] = temp.ToArray();
            }

            foreach (var kvp in gestures)
                mWeights[kvp.Key] = Enumerable.Range(0, feature_results[0].Length).Select(x => 0.0).ToList();
            var oldWeights = new Dictionary<string, List<double>>();
            foreach (var kvp in gestures)
                oldWeights[kvp.Key] = Enumerable.Range(0, feature_results[0].Length).Select(x => 0.0).ToList();

            var sw = new System.Diagnostics.Stopwatch(); sw.Start();
            int num_iters = 0;
            do {
                // make oldWeights the same as mWeights before we update mWeights
                foreach ( var kvp in mWeights ) {
                    oldWeights[kvp.Key].Clear();
                    oldWeights[kvp.Key].AddRange(mWeights[kvp.Key]);
                }

                for (int inputgest_i = 0; inputgest_i < allgestures.Count; inputgest_i++)
                {
                    var class_weights = new Dictionary<string, double>();
                    // here kvp.Key specifies the gesture class label
                    foreach (var kvp in gestures)
                    {
                        double weight = 0;
                        // iterate through each feature
                        for (int feat_i = 0; feat_i < feature_results[inputgest_i].Length; feat_i++)
                            weight += 1.0 * feature_results[inputgest_i][feat_i] * mWeights[kvp.Key][feat_i];
                        class_weights[kvp.Key] = Math.Exp(weight);
                    }
                    double total_weight = 0;
                    foreach (var kvp in class_weights)
                        total_weight += kvp.Value;

                    // now time to update the parameters
                    // kvp.Key is gesture class label
                    foreach (var kvp in gestures)
                    {
                        double indicator = 0;
                        if (allgestures[inputgest_i].Item1 == kvp.Key) indicator = 1.0;
                        double error = indicator - class_weights[kvp.Key] / total_weight;
                        for (int feat_i = 0; feat_i < feature_results[inputgest_i].Length; feat_i++)
                            mWeights[kvp.Key][feat_i] += error * feature_results[inputgest_i][feat_i];
                    }
                }
                num_iters++;

                if (num_iters % 100 == 0)
                {
                    double error_sum = _Sum_weight_diff(oldWeights, mWeights);
                    Console.Write("{0} iterations in {1}\n Sum of weight differences: ", num_iters, sw.Elapsed);
                    Console.Write("{0:0.000}\n", error_sum);
                }
            } while (!_Completely_converged(oldWeights, mWeights, mConvergenceThreshold * mStepSize));

        }

        public void SaveModel(string filename)
        {
            var stream = File.Open(filename, FileMode.Create);
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this.GetType());
            formatter.Serialize(stream, mWeights);
            stream.Close();

            Console.WriteLine("Saved trained model to {0}", filename);
        }

        public void LoadModel(string filename)
        {
            var stream = File.Open(filename, FileMode.Open);
            var formatter = new BinaryFormatter();
            var mtype = (Type)formatter.Deserialize(stream);
            if (!mtype.Equals(this.GetType())) throw new InvalidDataException();
            mWeights = (Dictionary<string, List<double>>)formatter.Deserialize(stream);
            stream.Close();

            Console.WriteLine("Loaded model from {0}", filename);
        }

        #endregion
    }
}
