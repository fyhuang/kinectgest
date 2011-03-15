using System;
using System.Diagnostics;

namespace FinalProject
{
	public static class Benchmarks
	{
		static public void BenchmarkRecognize()
		{
			var sw = new Stopwatch();
			var gest = new InputGesture(new LogFileLoader("gestures/track_high_kick_01.log"));
			var numiters = 1000;
			
			sw.Start();
			for ( int i = 0; i < numiters; i++ ) {
				foreach ( var f in Features.AllFeatures.GestureFeatures ) {
					f.QueryGesture(gest);
				}
			}
			sw.Stop();
			
			Console.WriteLine("Elapsed time ({0} iters): {1}", numiters, sw.Elapsed);
			Console.WriteLine("Gestures per second: {0}", 1000.0f / (sw.ElapsedMilliseconds / (float)numiters));
		}
	}
}

