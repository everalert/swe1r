using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SWE1R.Racer
{
    [Serializable]
    public class DataCollection : ICloneable
    {
        //below is original thoughts, base generalised class now coded?
        //so far, seems to be very useful as a generalised class

        //the whole point of extrapolating data collection into a class is to instantiate each step for comparison
        //maybe make generalised class that mass updates variables based on a list of pointers,
        //and can dynamically add to the list if a custom pointer is given?
        //then derive this (overlay data collection and processing) class from that
        //in general, might need to take this approach for a robust DRR class anyway?
        //might also be able to abstract into savestate class too


        public List<DataBlock> data;

        public DataCollection()
        {
            data = new List<DataBlock>();
        }

        public DataBlock[] Data => data.ToArray();

        public dynamic GetValue(Racer racer, DataBlock.Path path, uint offset, Core.DataType type, uint length)
        {
            int index = ValueExists(path, offset, length);
            if (index < 0)
            {
                data.Add(new DataBlock(racer, path, offset, type, length));
                return data.Last().GetValue(offset, type, length);
            }
            else
            {
                return data[index].GetValue(offset, type, length);
            }
        }

        public dynamic GetValue(Racer racer, Addr.Pod datapoint)
        {
            return GetValue(racer, DataBlock.Path.Pod, (uint)datapoint, Addr.GetType(datapoint), Addr.GetLength(datapoint));
        }

        public dynamic GetValue(Racer racer, Addr.PodState datapoint)
        {
            return GetValue(racer, DataBlock.Path.PodState, (uint)datapoint, Addr.GetType(datapoint), Addr.GetLength(datapoint));
        }

        public dynamic GetValue(Racer racer, Addr.Rendering datapoint)
        {
            return GetValue(racer, DataBlock.Path.Rendering, (uint)datapoint, Addr.GetType(datapoint), Addr.GetLength(datapoint));
        }

        public dynamic GetValue(Racer racer, Addr.Race datapoint)
        {
            return GetValue(racer, DataBlock.Path.Race, (uint)datapoint, Addr.GetType(datapoint), Addr.GetLength(datapoint));
        }

        public dynamic GetValue(Racer racer, Addr.Static datapoint)
        {
            return GetValue(racer, DataBlock.Path.Static, (uint)datapoint, Addr.GetType(datapoint), Addr.GetLength(datapoint));
        }

        public dynamic GetValue(int index)
        {
            if (data.Count <= index || index < 0)
                return false;
            else
                return data[index].GetValue();
        }

        public void Update(Racer racer)
        {
            foreach (DataBlock block in data)
                block.Update(racer);
        }

        public int ValueExists(DataBlock.Path path, uint offset, uint length)
        {
            for (int z = 0; z < data.Count; z++)
                if (data[z].ContainsValue(path, offset, length))
                    return z;
            return -1;
        }

        public object Clone()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                if (GetType().IsSerializable)
                {

                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                    stream.Position = 0;
                    return formatter.Deserialize(stream);
                }
                return null;
            }
        }


        // information storage class

        [Serializable]
        public class DataBlock
        {
            private byte[] data;
            private uint dataLen = 0;
            private uint offset;
            private Path pathId;
            private Core.DataType dataType = Core.DataType.None;

            //public DataBlock() { }

            public DataBlock(Racer racer, Path path, uint off, Core.DataType type, uint len = 4)
            {
                pathId = path;
                offset = off;
                dataType = type;
                dataLen = Core.DataTypeLength(type) > 0 ? Core.DataTypeLength(type) : len;
                Update(racer);
            }

            public DataBlock(byte[] d, Path p, uint o, Core.DataType t = Core.DataType.None)
            {
                data = d;
                dataLen = (uint)d.Length;
                pathId = p;
                offset = o;
                dataType = t;
            }

            public uint DataLen => dataLen;

            public uint Offset => offset;

            public Path PathId => pathId;

            public byte[] GetBytes(uint off, uint len)
            {
                CheckDataLoaded();
                CheckLenArg(len);
                CheckContainsValue(off, len);
                byte[] output = new byte[len];
                Array.Copy(data, off - offset, output, 0, len);
                return output;
            }

            public byte[] GetBytes()
            {
                return GetBytes(offset, dataLen);
            }

            public dynamic GetValue(uint off, Core.DataType type = Core.DataType.Single, uint len = 0)
            {
                CheckDataLoaded();
                CheckLenArg(len, type);
                CheckContainsValue(off, len);
                uint sanitizedLen = Core.DataTypeLength(type) > 0 ? Core.DataTypeLength(type) : len;
                byte[] sanitizedData = new byte[sanitizedLen];
                Array.Copy(data, off - offset, sanitizedData, 0, sanitizedLen);
                switch (type)
                {
                    case Core.DataType.String:
                        return Encoding.Default.GetString(sanitizedData);
                    case Core.DataType.Byte:
                        return sanitizedData[0];
                    case Core.DataType.SByte:
                        return (sbyte)sanitizedData[0];
                    case Core.DataType.Int16:
                        return BitConverter.ToInt16(sanitizedData, 0);
                    case Core.DataType.UInt16:
                        return BitConverter.ToUInt16(sanitizedData, 0);
                    case Core.DataType.Int32:
                        return BitConverter.ToInt32(sanitizedData, 0);
                    case Core.DataType.UInt32:
                        return BitConverter.ToUInt32(sanitizedData, 0);
                    case Core.DataType.Single:
                        return BitConverter.ToSingle(sanitizedData, 0);
                    case Core.DataType.Int64:
                        return BitConverter.ToInt64(sanitizedData, 0);
                    case Core.DataType.UInt64:
                        return BitConverter.ToUInt64(sanitizedData, 0);
                    case Core.DataType.Double:
                        return BitConverter.ToDouble(sanitizedData, 0);
                    default:
                        return sanitizedData;
                }
            }

            public dynamic GetValue()
            {
                if (dataType == Core.DataType.None || dataType == Core.DataType.String)
                    return GetValue(offset, dataType, dataLen);
                return GetValue(offset, dataType);
            }

            public void Update(Racer racer)
            {
                CheckUpdateable();
                switch (pathId)
                {
                    case Path.Static:
                        data = racer.GetData((Addr.Static)offset, dataLen);
                        break;
                    case Path.Pod:
                        data = racer.GetData((Addr.Pod)offset, dataLen);
                        break;
                    case Path.PodState:
                        data = racer.GetData((Addr.PodState)offset, dataLen);
                        break;
                    case Path.Race:
                        data = racer.GetData((Addr.Race)offset, dataLen);
                        break;
                    case Path.Rendering:
                        data = racer.GetData((Addr.Rendering)offset, dataLen);
                        break;
                    default:
                        break;
                }
            }

            public bool ContainsValue(Path path, uint off, uint length)
            {
                return (pathId == path) && (offset <= off) && (offset + data.Length >= off + length);
            }

            public enum Path
            {
                None = 0,
                Static = 0,
                Pod = 1,
                PodState = 2,
                Race = 3,
                Rendering = 4
            };

            public static Path GetPathFromAddr(Addr.Pod a)
            {
                return Path.Pod;
            }

            public static Path GetPathFromAddr(Addr.PodState a)
            {
                return Path.PodState;
            }

            public static Path GetPathFromAddr(Addr.Race a)
            {
                return Path.Race;
            }

            public static Path GetPathFromAddr(Addr.Rendering a)
            {
                return Path.Rendering;
            }

            public static Path GetPathFromAddr(Addr.Static a)
            {
                return Path.Static;
            }

            // zzz

            private void CheckDataLoaded()
            {
                if (data == null)
                    throw new Exception("No data loaded.");
            }
            private void CheckLenArg(uint len)
            {
                if (len <= 0)
                    throw new Exception("Length must be greater than 0.");
            }
            private void CheckLenArg(uint len, Core.DataType type)
            {
                if ((type == Core.DataType.None || type == Core.DataType.String) && len <= 0)
                    throw new Exception("Length must be greater than 0 when type is None or String.");
            }
            private void CheckContainsValue(uint off, uint len)
            {
                if (!ContainsValue(pathId, off, len))
                    throw new Exception("Value range not in data block.");
            }
            private void CheckUpdateable()
            {
                if (pathId == null || offset == null || dataLen <= 0)
                    throw new Exception("Datapoint information not set.");
            }
        }
    }


    public class TwoFrameDataCollection
    {
        public DataCollection data = new DataCollection(), data_prev;

        public void Update(Racer r)
        {
            data_prev = (DataCollection)data.Clone();
            data.Update(r);
        }
    }
    //also add generalised multi-frame version?
}