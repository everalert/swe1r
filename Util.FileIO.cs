using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Force.Crc32;

namespace SWE1R.Util
{
    public class FileIO
    {
        
        //writing

        public static void WriteChunk(FileStream file, byte[] data, ref uint crc32)
        {
            file.Write(data, 0, data.Length);
            crc32 = Crc32Algorithm.Append(crc32, data);
        }
        public static void WriteChunk(FileStream file, byte data, ref uint crc32)
        {
            byte[] output = new byte[1] { data };
            file.Write(output, 0, 1);
            crc32 = Crc32Algorithm.Append(crc32, output);
        }
        public static void WriteChunk(FileStream file, dynamic data, ref uint crc32)
        {
            byte[] output = BitConverter.GetBytes(data);
            file.Write(output, 0, output.Length);
            crc32 = Crc32Algorithm.Append(crc32, output);
        }
        public static void WriteChunk(FileStream file, byte[] data, ref uint crc32a, ref uint crc32b)
        {
            file.Write(data, 0, data.Length);
            crc32a = Crc32Algorithm.Append(crc32a, data);
            crc32b = Crc32Algorithm.Append(crc32b, data);
        }
        public static void WriteChunk(FileStream file, byte data, ref uint crc32a, ref uint crc32b)
        {
            byte[] output = new byte[1] { data };
            file.Write(output, 0, 1);
            crc32a = Crc32Algorithm.Append(crc32a, output);
            crc32b = Crc32Algorithm.Append(crc32b, output);
        }
        public static void WriteChunk(FileStream file, dynamic data, ref uint crc32a, ref uint crc32b)
        {
            byte[] output = BitConverter.GetBytes(data);
            file.Write(output, 0, output.Length);
            crc32a = Crc32Algorithm.Append(crc32a, output);
            crc32b = Crc32Algorithm.Append(crc32b, output);
        }

        //reading

        public static byte[] ReadChunk(FileStream file, int length)
        {
            byte[] data = new byte[length];
            file.Read(data, 0, length);
            return data;
        }
        public static byte[] ReadChunk(FileStream file, int length, ref uint crc32)
        {
            byte[] data = new byte[length];
            file.Read(data, 0, length);
            crc32 = Crc32Algorithm.Append(crc32, data);
            return data;
        }
        public static byte[] ReadChunk(FileStream file, int length, ref uint crc32a, ref uint crc32b)
        {
            byte[] data = new byte[length];
            file.Read(data, 0, length);
            crc32a = Crc32Algorithm.Append(crc32a, data);
            crc32b = Crc32Algorithm.Append(crc32b, data);
            return data;
        }
    }
}
