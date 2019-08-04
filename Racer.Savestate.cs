using SWE1R.Util;
using System;
using System.Collections.Generic;
using System.IO;
using Force.Crc32;

namespace SWE1R.Racer
{
    public class Savestate : DataCollection
    {
        private static readonly byte[] fileMagicWord = new byte[16] { 0x89, 0x53, 0x57, 0x45, 0x31, 0x52, 0x53, 0x41, 0x56, 0x53, 0x54, 0x41, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] fileEOFWord = new byte[8] { 0x2E, 0x44, 0x4F, 0x54, 0x44, 0x4F, 0x4E, 0x45 };
        public const string fileExt = "e1rs";
        private const byte fileVersion = 1;

        private bool initialized = false;

        public byte Vehicle
        {
            get
            {
                int i = ValueExists(DataBlock.Path.Race, (uint)Addr.Race.SelectedVehicle, 1);
                return i >= 0 ? GetValue(i) : 0xFF;
            }
        }
        public byte Track
        {
            get
            {
                int i = ValueExists(DataBlock.Path.Race, (uint)Addr.Race.SelectedTrack, 1);
                return i >= 0 ? GetValue(i) : 0xFF;
            }
        }
        public byte[] PodState
        {
            get
            {
                int i = ValueExists(DataBlock.Path.PodState, 0, (uint)Addr.PtrLen.PodState);
                return i >= 0 ? GetValue(i) : new byte[] { 0x00 };
            }
        }

        public Savestate()
        {
            data = new List<DataBlock>();
        }
        public Savestate(Racer r)
        {
            data = new List<DataBlock>();
            Init(r);
        }

        public void Init(Racer r)
        {
            GetValue(r, Addr.Race.SelectedVehicle);
            GetValue(r, Addr.Race.SelectedTrack);
            GetValue(r, Addr.Pod.Flags);
            GetValue(r, DataBlock.Path.Pod, (uint)Addr.Pod.TimeLap1, Core.DataType.None, 0x19);
            GetValue(r, DataBlock.Path.PodState, 0, Core.DataType.None, (uint)Addr.PtrLen.PodState);
            GetValue(r, Addr.Rendering.CameraMode);
            GetValue(r, Addr.Rendering.FogFlags);
            GetValue(r, DataBlock.Path.Rendering, (uint)Addr.Rendering.FogColR, Core.DataType.None, 0x10);
            initialized = true;
        }

        public void Save(Racer r)
        {
            if (CheckSaveable(r))
            {
                if (!initialized)
                    Init(r);
                else
                    Update(r);
            }
        }

        public void Load(Racer r)
        {
            if (CheckLoadable(r))
            {
                foreach (DataBlock block in data)
                {
                    switch (block.PathId)
                    {
                        case DataBlock.Path.Pod:
                            r.WriteData((Addr.Pod)block.Offset, block.GetBytes());
                            break;
                        case DataBlock.Path.PodState:
                            r.WriteData((Addr.PodState)block.Offset, block.GetBytes());
                            //breaks when loading state from old session, probably some pointers being written?
                            break;
                        case DataBlock.Path.Rendering:
                            r.WriteData((Addr.Rendering)block.Offset, block.GetBytes());
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void Export(string filename)
        {
            //string filename = @"K:\Projects\swe1r\overlay\SWE1R Overlay\Format\Racer.State.WriteTest.e1rs";
            if (File.Exists(filename))
                File.Delete(filename);
            FileStream file = File.OpenWrite(filename);

            // HEADER

            uint headerCRC32 = 0;
            FileIO.WriteChunk(file, fileMagicWord, ref headerCRC32);
            byte readerVer = 1;
            byte writerVer = 1;
            FileIO.WriteChunk(file, writerVer, ref headerCRC32);
            FileIO.WriteChunk(file, readerVer, ref headerCRC32);
            FileIO.WriteChunk(file, !BitConverter.IsLittleEndian, ref headerCRC32);
            uint dataLen = 4; // pod/track bytes + final crc32
            foreach (DataBlock block in data)
                dataLen += 13 + block.DataLen; // + length of each block
            FileIO.WriteChunk(file, dataLen, ref headerCRC32);
            ushort dataOff = (ushort)(file.Position + sizeof(ushort) + sizeof(uint));
            FileIO.WriteChunk(file, dataOff, ref headerCRC32);
            file.Write(BitConverter.GetBytes(headerCRC32), 0, 4);

            // DATA

            uint dataCRC32 = 0;
            uint blockCRC32;
            foreach (DataBlock block in data)
            {
                blockCRC32 = 0;
                FileIO.WriteChunk(file, (byte)block.PathId, ref blockCRC32, ref dataCRC32);
                FileIO.WriteChunk(file, block.Offset, ref blockCRC32, ref dataCRC32);
                FileIO.WriteChunk(file, block.DataLen, ref blockCRC32, ref dataCRC32); //maybe safer to use GetBytes().Length, but it shouldnt be possible for this value to mismatch
                FileIO.WriteChunk(file, block.GetBytes(), ref blockCRC32, ref dataCRC32);
                FileIO.WriteChunk(file, blockCRC32, ref dataCRC32);
            }
            file.Write(BitConverter.GetBytes(dataCRC32), 0, 4);
            file.Write(fileEOFWord, 0, fileEOFWord.Length);

            // CLEANUP

            file.Dispose();
            file.Close();
        }

        public void Import(string filename)
        {
            //need to reimplement endianness converting, but low priority cuz there is probably not going to be variance from exporter across platforms

            //string filename = @"K:\Projects\swe1r\overlay\SWE1R Overlay\Format\Racer.State.WriteTest.e1rs";
            FileStream file = File.OpenRead(filename);
            uint headerCRC32 = 0;
            uint dataCRC32 = 0;

            // READ HEADER

            // read data
            byte[] inMagicWord = FileIO.ReadChunk(file, fileMagicWord.Length, ref headerCRC32);
            if (!Win32.ByteArrayCompare(inMagicWord, fileMagicWord))
                throw new Exception("Read Savestate: Invalid filetype.");
            byte inVerSrc = FileIO.ReadChunk(file, 0x1, ref headerCRC32)[0]; //ideal/generated-from version
            byte inVerRead = FileIO.ReadChunk(file, 0x1, ref headerCRC32)[0]; //readable version
            bool inBigEndian = Convert.ToBoolean(FileIO.ReadChunk(file, 0x1, ref headerCRC32)[0]);
            byte[] inDataLen = FileIO.ReadChunk(file, 0x4, ref headerCRC32);
            byte[] inDataOff = FileIO.ReadChunk(file, 0x2, ref headerCRC32);
            byte[] inHeaderCRC32 = FileIO.ReadChunk(file, 0x4);
            // convert to big endian if needed
            //if (inBigEndian)
            //{
            //    inDataLen = inDataLen.Reverse().ToArray();
            //    inDataOff = inDataOff.Reverse().ToArray();
            //    inHeaderCRC32 = inHeaderCRC32.Reverse().ToArray();
            //}
            // check crc32
            if (Crc32Algorithm.Append(headerCRC32, inHeaderCRC32, 0, 0x4) != 0x2144DF1C)
                throw new Exception("Read Savestate: Header invalid.");
            // check eof
            file.Seek(BitConverter.ToUInt16(inDataOff, 0) + BitConverter.ToUInt32(inDataLen, 0), SeekOrigin.Begin);
            byte[] inEOFCheck = FileIO.ReadChunk(file, fileEOFWord.Length);
            if (!Win32.ByteArrayCompare(inEOFCheck, fileEOFWord))
                throw new Exception("Read Savestate: File length invalid.");

            // READ DATA

            file.Seek(BitConverter.ToUInt16(inDataOff, 0), SeekOrigin.Begin);
            List<DataBlock> inDataBlocks = new List<DataBlock>();
            while (file.Position < BitConverter.ToUInt16(inDataOff, 0) + BitConverter.ToUInt32(inDataLen, 0) - 4)
            {
                uint blockCRC32 = 0;
                DataBlock.Path p = (DataBlock.Path)FileIO.ReadChunk(file, 0x1, ref blockCRC32, ref dataCRC32)[0];
                uint o = BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref blockCRC32, ref dataCRC32), 0);
                int l = (int)BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref blockCRC32, ref dataCRC32), 0); //typecasted to preserve original encoding from uint
                                                                                                                   //byte[] d = FileIO.ReadChunk(file, BitConverter.ToInt32((inBigEndian ? inDataBlocks.Last().length.Reverse().ToArray() : inDataBlocks.Last().length), 0), ref blockCRC32, ref dataCRC32);
                byte[] d = FileIO.ReadChunk(file, l, ref blockCRC32, ref dataCRC32);
                inDataBlocks.Add(new DataBlock(d, p, o, Core.DataType.None));
                byte[] crc32 = FileIO.ReadChunk(file, 0x4, ref dataCRC32);
                //if (inBigEndian)
                //    inDataBlocks.Last().ReverseArrays();
                if (Crc32Algorithm.Append(blockCRC32, crc32) != 0x2144DF1C)
                    throw new Exception("Read Savestate: Data block " + inDataBlocks.Count + " invalid.");
            }
            // check entire data set is valid
            byte[] inDataCRC32 = FileIO.ReadChunk(file, 0x4);
            //if (inBigEndian)
            //    inDataCRC32 = inDataCRC32.Reverse().ToArray();
            if (Crc32Algorithm.Append(dataCRC32, inDataCRC32, 0, 0x4) != 0x2144DF1C)
                throw new Exception("Read Savestate: Data invalid.");
            // check end of data is actually end of file
            inEOFCheck = FileIO.ReadChunk(file, fileEOFWord.Length);
            if (!Win32.ByteArrayCompare(inEOFCheck, fileEOFWord))
                throw new Exception("Read Savestate: File length invalid.");

            //Savestate output = new Savestate(inDataBlocks.ToArray(), inDataPod, inDataTrack);
            //need to implement a way to update data blocks with data not sourced from the game directly in order to actually use the read data

            file.Dispose();
            file.Close();

            //return output;
        }


        // CHECKING

        private bool CheckInRace(Racer r)
        {
            return r.GetData(Addr.Static.InRace) > 0;
        }

        private bool CheckRaceDataMatch(Racer r)
        {
            var ingame_track = r.GetData(Addr.Race.SelectedTrack);
            var ingame_vehicle = r.GetData(Addr.Race.SelectedVehicle);
            return (ingame_track == Track && ingame_vehicle == Vehicle);
        }

        private bool CheckLoadable(Racer r)
        {
            return CheckInRace(r) && CheckRaceDataMatch(r);
        }

        private bool CheckSaveable(Racer r)
        {
            return CheckInRace(r);
        }
    }
}