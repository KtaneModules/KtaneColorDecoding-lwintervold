using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace ColorDecodingHelper
{
	public static class HelperFunctions
	{
		public static void Shuffle<T> (this IList<T> list){
			for (int i = list.Count - 1; i > 0; i--) {
				int n = UnityEngine.Random.Range (0, i);
				T c = list [n];
				list [n] = list [i];
				list [i] = c;
			}
		}
	}
}

