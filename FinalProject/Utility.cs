using System;
namespace FinalProject
{
	public static class Utility
	{
		public static float Comp(this OpenTK.Vector3 vec, int ix) {
			if ( ix == 0 ) return vec.X;
			else if ( ix == 1 ) return vec.Y;
			else return vec.Z;
		}
		public static char CompToChar(int ix) {
			return (char)('X'+ix);
		}
		
		static public void PrintMemoryUsage() {
			Console.WriteLine("Memory usage: {0}MB", System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / (1024*1024));
		}
		
		static public double Sigmoid(double input) {
			return 1.0 / (1.0 + Math.Exp(-input));
		}
		static public float Sigmoid(float input) {
			return 1.0f / (1.0f + (float)Math.Exp(-input));
		}
	}
}

