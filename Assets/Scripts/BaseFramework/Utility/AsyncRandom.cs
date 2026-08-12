using System;
using System.Collections.Generic;

namespace BF.Utility
{
	public class AsyncRandom 
	{
        static int N = 0;
        static float M = 0f;

        #region Get random number/numbers
        //Get random number/numbers
        public static int Range(int min, int maxExclusuive)
        {
            N += DateTime.Now.Millisecond;
            var r = new System.Random(N);
            var b = r.Next(min, maxExclusuive);
            N += b;
            N = N > 1000000 ? N - 1000000 : N;
            return b;
        }
        public static float Range(float min, float max)
        {
            M += DateTime.Now.Millisecond;
            var r = new System.Random((int)M);
            var b = r.Next((int)(min * 2048), (int)(max * 2048));
            M += b;
            M = M > 1000000f ? M - 1000000f : M;
            return b / 2048f;
        }

        //Get random and no-repeat numbers
        public static int[] RangeMultiple(int min,int maxExclusuive,int count)
        {
            if (count > maxExclusuive - min)
            {
                return null;
            }
            int[] results = new int[count];
            for (int i = 0; i < count; i++)
            {
                int a = Range(min, maxExclusuive);
                bool isContain = false;
                for (int j = 0; j < i; j++)
                {
                    if (a == results[j])
                    {
                        isContain = true;
                        break;
                    }
                }
                if (!isContain)
                {
                    results[i] = a;
                }
                else
                {
                    i--;
                }
            }
            return results;
        }
        public static int[] RangeMultiple(int min,int maxExclusuive,int count,int minBreak)
        {
            if (count * minBreak > maxExclusuive - min || minBreak == 0)  
            {
                return null;
            }
            var results=new int[count];
            var maxCount = (maxExclusuive - min) / minBreak;
            results = RangeMultiple(0, maxCount, count);
            int randomOffset = Range(0, minBreak);
            for(int i = 0; i < count; i++)
            {
                results[i] = results[i] * minBreak + randomOffset + min;
            }
            return results;

        }
        #endregion
        #region Disrupt list/array elements
        public static T[] Disrupt<T>(T[] array)
        {
            N += DateTime.Now.Millisecond;
            var r = new System.Random((int)N);

            T temp;
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = r.Next(i + 1);
                temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
            return array;
        }
        public static List<T> Disrupt<T>(List<T> list)
        {
            N += DateTime.Now.Millisecond;
            var r = new System.Random((int)N);

            T temp;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = r.Next(i + 1);
                temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
            return list;
        }
        #endregion
        #region Get random number by possibility density(array)
        public static int GetIndex(float[] poss)
        {
            var p = Range(0, 1f);
            for (int i = poss.Length - 1; i >= 0; i--) 
            {
                if (poss[i] > 0)
                {
                    p -= poss[i];
                    if(p < 0)
                    {
                        return i;
                    }
                }
            }
            return 0;
        }
        public static int GetIndex(float[] poss, float[] last)
        {
            for(int i = 0; i < poss.Length; i++)
            {
                last[i] += poss[i];
            }
            int index = -1;
            float maxPos = -1f;
            for(int i = 0; i < poss.Length; i++)
            {
                if (last[i] > maxPos)
                {
                    maxPos = last[i];
                    index = i;
                }
            }
            last[index] -= 1;
            return index;
        }
        #endregion
    }
}