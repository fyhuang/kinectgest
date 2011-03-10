using System;
namespace FinalProject.Utility
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
	}
}

