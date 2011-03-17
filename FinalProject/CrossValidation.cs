using System;
using System.IO;

namespace FinalProject
{
	public class CrossValidation
	{
		string mFilename;
		public int Index { get; private set; }
		
		public CrossValidation(string filename) {
			mFilename = filename;
			
			var file = new StreamReader(File.Open(mFilename, FileMode.OpenOrCreate));
			try {
				Index = int.Parse(file.ReadToEnd());
			}
			catch ( Exception ) {
				Index = 0;
			}
			finally {
				file.Close();
			}
		}
		
		public void Incr() {
			Index++;
			Console.WriteLine("CV index now {0}", Index);
		}
		public void Save() {
			var file = new StreamWriter(File.Open(mFilename, FileMode.Create));
			file.Write(Index);
			file.Close();
		}
		
		public bool IsTraining(int ix, int max) {
			if ( (ix + Index) % 4 == 0 ) return false;
			return true;
		}
	}
}

