using System;
using System.Collections;
using System.Collections.Generic;

namespace FinalProject
{
	public class EnumerableFromEnumerator<T> : IEnumerable<T>
	{
		IEnumerator<T> mEnum;
		public EnumerableFromEnumerator(IEnumerator<T> en) {
			mEnum = en;
		}
		
		public IEnumerator<T> GetEnumerator() {
			return mEnum;
		}
		
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}

