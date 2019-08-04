using System;
using System.Collections.Generic;

namespace SWE1R.Util
{
    public static class Helper
    {
        public static List<String> ArrayToStrList(Array input)
        {
            var output = new List<String>();
            foreach (float item in input)
                output.Add(item.ToString());
            return output;
        }

        public static string[] FormatTimesArray( float[] input, string format )
        {
            var output = new string[input.Length];
            for (var i = 0; i < input.Length; i++)
                output[i] = TimeSpan.FromSeconds(input[i]).ToString(format);
            return output;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        public static float ByteToFloat(byte b)
        {
            return b / 255;
        }


        public enum DataTypes
        {
            //unsigned = no. of bits, signed = bits-1, fractional = bits+1
            SByte = 7,
            Byte = 8,
            Int16 = 15,
            UInt16 = 16,
            Int32 = 31,
            UInt32 = 32,
            Single = 33,
            Int64 = 63,
            UInt64 = 64,
            Double = 65,
            Decimal = 129
        };
    }
}
