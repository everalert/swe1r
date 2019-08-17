using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
            return 1f * b / 255;
        }

        public static byte FloatToByte(float f)
        {
            return Convert.ToByte(255*Clamp(f,0f,1f));
        }

        public static string ReadNullTerminatedString(byte[] s)
        {
            int i = 0;
            while (i < s.Length && s[i] != 0)
                i++;
            return Encoding.ASCII.GetString(s, 0, i);
        }

        public static byte[] WriteNullTerminatedBytes(string s, int l)
        {
            byte[] output = new byte[l];
            byte[] input = Encoding.ASCII.GetBytes(s);
            for (int i = 0; i < l && i < input.Length; i++)
                output[i] = input[i];
            return output;
        }

        public static string SecondsToTimeString(float t, int dp = 3)
        {
            return string.Format("{0}:{1}", (int)Math.Floor(t / 60), (t % 60).ToString("00" + (dp > 0 ? ".".PadRight(dp+1,char.Parse("0")) : "")));
        }

        public static bool CheckDurationFormat(string d)
        {
            Regex r = new Regex("^[0-5]?[0-9]:[0-5]?[0-9].[0-9]{1,7}$");
            return r.IsMatch(d);
        }

        public static float TimeStringToSeconds(string t)
        {
            if (!CheckDurationFormat(t))
                throw new ArgumentException();
            string[] str = t.Split(':');
            float m = Convert.ToSingle(str[0], CultureInfo.InvariantCulture.NumberFormat);
            float s = Convert.ToSingle(str[1], CultureInfo.InvariantCulture.NumberFormat);
            return m * 60 + s;
        }

        public static bool CheckFilenameFormat(string f)
        {
            return !(f.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
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
