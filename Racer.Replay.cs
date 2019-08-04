using Force.Crc32;
using SWE1R.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SWE1R.Racer
{
    public class Replay
    {
        private static readonly byte[] file_word_magic = new byte[16] { 0x89, 0x53, 0x57, 0x45, 0x31, 0x52, 0x4D, 0x4F, 0x56, 0x52, 0x45, 0x43, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] file_word_eof = new byte[8] { 0x2E, 0x44, 0x4F, 0x54, 0x44, 0x4F, 0x4E, 0x45 };
        public const string fileExt = "e1rm";
        private const byte file_version = 1;

        private List<DataCollection> data = new List<DataCollection>();
        public DataCollection[] Data => data.ToArray();

        private byte meta_track = 0xFF, meta_vehicle = 0xFF;
        private byte[] meta_player, meta_upgrade_level, meta_upgrade_health;

        private bool initialized = false; // only true when first frame fully written

        private string string_processor;

        // CONTROL

        private void Init(Racer r)
        {
            if (CheckInRace(r) && data.Count <= 0)
            {
                // get race info
                meta_track = r.GetData(Addr.Race.SelectedTrack);
                meta_vehicle = r.GetData(Addr.Race.SelectedVehicle);
                meta_player = r.GetData(Addr.Static.SaveFile01, 0x20);
                meta_upgrade_level = r.GetData(Addr.Static.SaveFile01 + 0x41, 0x7);
                meta_upgrade_health = r.GetData(Addr.Static.SaveFile01 + 0x48, 0x7);

                // write first frame
                // index 0 used as canonical data set; referred to for file output and serves as base for new frames
                DataCollection first = new DataCollection();
                first.GetValue(r, Addr.Pod.TimeTotal);
                first.GetValue(r, Addr.Pod.Lap);
                first.GetValue(r, Addr.Pod.Flags);
                first.GetValue(r, Addr.PodState.Flags1);
                first.GetValue(r, Addr.PodState.Flags2);
                first.GetValue(r, Addr.PodState.X);
                first.GetValue(r, Addr.PodState.Y);
                first.GetValue(r, Addr.PodState.Z);
                first.GetValue(r, Addr.PodState.Vector3D_1A);
                first.GetValue(r, Addr.PodState.Vector3D_1B);
                first.GetValue(r, Addr.PodState.Vector3D_1C);
                first.GetValue(r, Addr.PodState.Vector3D_2A);
                first.GetValue(r, Addr.PodState.Vector3D_2B);
                first.GetValue(r, Addr.PodState.Vector3D_2C);
                first.GetValue(r, Addr.PodState.LapCompletion);
                first.GetValue(r, Addr.PodState.Speed);
                first.GetValue(r, Addr.PodState.Heat);
                first.GetValue(r, Addr.PodState.EngineDamageTL);
                first.GetValue(r, Addr.PodState.EngineDamageML);
                first.GetValue(r, Addr.PodState.EngineDamageBL);
                first.GetValue(r, Addr.PodState.EngineDamageTR);
                first.GetValue(r, Addr.PodState.EngineDamageMR);
                first.GetValue(r, Addr.PodState.EngineDamageBR);
                /* also add inputs at some point; need to figure out how to conflate all the input methods */
                //also - LightningPirate: I'd say just the main stuff like what pod, upgrades, upgrade health, and track and then xyz, time, current lap, progress, speed, boost, thrust, pitch, brake, turning, repair, slide, tilt, deaths, engine stuff (health, fire, temp)
                //also also - 3d transformation data, animation id, animation frame
                //so far mostly included except animation stuff (addresses not known) and input stuff
                data.Add(first);
                initialized = true;
            }
        }

        public void Update(Racer r)
        {
            if (!CheckUpdateable(r))
                Init(r);
            else
            {
                DataCollection next = (DataCollection)data[0].Clone();
                next.Update(r);
                float timestamp = next.GetValue(r, Addr.Pod.TimeTotal);
                bool timestamp_exists = false;
                for (int i = 0; i < data.Count; i++)
                {
                    float timestamp_frame = data[i].GetValue(r, Addr.Pod.TimeTotal);
                    if (timestamp_frame > timestamp)
                        data.RemoveAt(i);
                    if (timestamp_frame == timestamp)
                        timestamp_exists = true;
                }
                if (!timestamp_exists)
                    data.Add(next);
            }
        }

        public void Reset(Racer r)
        {
            data.Clear();
            initialized = false;
            Init(r);
        }


        // FILE HANDLING

        public void Export(string filename)
        {
            uint len_head = 0x27;
            uint len_meta = 0x30;
            uint len_def = FrameItemCount * 0x9;
            uint len_body = FrameCount * FrameDataSize;
            uint len_data = len_meta + len_def + len_body;
            uint off_meta = len_head;
            uint off_def = off_meta + len_meta;
            uint off_body = off_def + len_def;

            CheckExportable(true);

            if (File.Exists(filename))
                File.Delete(filename);

            using (FileStream file = File.OpenWrite(filename))
            {
                uint CRC32 = 0;

                // header
                FileIO.WriteChunk(file, file_word_magic, ref CRC32);
                byte writerVer = file_version;
                FileIO.WriteChunk(file, writerVer, ref CRC32);
                byte readerVer = file_version;
                FileIO.WriteChunk(file, readerVer, ref CRC32);
                FileIO.WriteChunk(file, !BitConverter.IsLittleEndian, ref CRC32);
                FileIO.WriteChunk(file, off_meta, ref CRC32);
                FileIO.WriteChunk(file, off_def, ref CRC32);
                FileIO.WriteChunk(file, off_body, ref CRC32);
                FileIO.WriteChunk(file, len_data, ref CRC32);
                FileIO.WriteChunk(file, CRC32, ref CRC32);

                // meta
                FileIO.WriteChunk(file, meta_player, ref CRC32);
                FileIO.WriteChunk(file, meta_track, ref CRC32);
                FileIO.WriteChunk(file, meta_vehicle, ref CRC32);
                FileIO.WriteChunk(file, meta_upgrade_level, ref CRC32);
                FileIO.WriteChunk(file, meta_upgrade_health, ref CRC32);

                // def
                foreach (DataCollection.DataBlock block in data[0].Data)
                {
                    FileIO.WriteChunk(file, (byte)block.PathId, ref CRC32);
                    FileIO.WriteChunk(file, block.Offset, ref CRC32);
                    FileIO.WriteChunk(file, block.DataLen, ref CRC32);
                }

                // body
                foreach (DataCollection frame in data)
                    foreach (DataCollection.DataBlock block in frame.Data)
                        FileIO.WriteChunk(file, block.GetBytes(), ref CRC32);

                // footer
                FileIO.WriteChunk(file, CRC32, ref CRC32);
                file.Write(file_word_eof, 0, file_word_eof.Length);
            }
        }

        public void Import(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();

            using (FileStream file = File.OpenRead(filename))
            {
                uint hCRC32 = 0, fCRC32 = 0;
                byte[] CRC32_check;

                // header
                byte[] word_magic = FileIO.ReadChunk(file, file_word_magic.Length, ref hCRC32, ref fCRC32);
                if (!Win32.ByteArrayCompare(word_magic, file_word_magic))
                    throw new InvalidDataException();
                byte ver_write = FileIO.ReadChunk(file, 0x1, ref hCRC32, ref fCRC32)[0];
                byte ver_read = FileIO.ReadChunk(file, 0x1, ref hCRC32, ref fCRC32)[0];
                byte endianness = FileIO.ReadChunk(file, 0x1, ref hCRC32, ref fCRC32)[0];
                uint off_meta = BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref hCRC32, ref fCRC32), 0);
                uint off_def = BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref hCRC32, ref fCRC32), 0);
                uint off_body = BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref hCRC32, ref fCRC32), 0);
                uint len_data = BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref hCRC32, ref fCRC32), 0);
                CRC32_check = FileIO.ReadChunk(file, 0x4, ref fCRC32);
                if (Crc32Algorithm.Append(hCRC32, CRC32_check) != 0x2144DF1C)
                    throw new InvalidDataException();

                // meta
                byte[] in_meta_player = FileIO.ReadChunk(file, 0x20, ref fCRC32);
                byte in_meta_track = FileIO.ReadChunk(file, 0x1, ref fCRC32)[0];
                byte in_meta_vehicle = FileIO.ReadChunk(file, 0x1, ref fCRC32)[0];
                byte[] in_meta_upgrade_level = FileIO.ReadChunk(file, 0x7, ref fCRC32);
                byte[] in_meta_upgrade_health = FileIO.ReadChunk(file, 0x7, ref fCRC32);

                // def
                List<DataCollection.DataBlock.Path> in_path = new List<DataCollection.DataBlock.Path>();
                List<uint> in_offset = new List<uint>();
                List<uint> in_length = new List<uint>();
                while (file.Position < off_body)
                {
                    in_path.Add((DataCollection.DataBlock.Path)FileIO.ReadChunk(file, 0x1, ref fCRC32)[0]);
                    in_offset.Add(BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref fCRC32), 0));
                    in_length.Add(BitConverter.ToUInt32(FileIO.ReadChunk(file, 0x4, ref fCRC32), 0));
                }

                // body
                List<DataCollection> in_frame = new List<DataCollection>();
                while (file.Position < off_meta + len_data)
                {
                    DataCollection frame = new DataCollection();
                    for (int i = 0; i < in_path.Count; i++)
                    {
                        byte[] in_data = FileIO.ReadChunk(file, (int)in_length[i], ref fCRC32);
                        DataCollection.DataBlock block = new DataCollection.DataBlock(in_data, in_path[i], in_offset[i]);
                        frame.data.Add(block);
                    }
                    in_frame.Add(frame);
                }

                // footer
                CRC32_check = FileIO.ReadChunk(file, 0x4);
                if (Crc32Algorithm.Append(fCRC32, CRC32_check) != 0x2144DF1C)
                    throw new InvalidDataException();
                byte[] word_eof = FileIO.ReadChunk(file, file_word_eof.Length);
                if (!Win32.ByteArrayCompare(word_eof, file_word_eof))
                    throw new InvalidDataException();

                // output
                meta_track = in_meta_track;
                meta_vehicle = in_meta_vehicle;
                meta_player = in_meta_player;
                meta_upgrade_level = in_meta_upgrade_level;
                meta_upgrade_health = in_meta_upgrade_health;
                data.Clear();
                foreach (DataCollection frame in in_frame)
                    data.Add(frame);
                initialized = true;
            }
        }

        private uint FrameItemCount => data.Count() > 0 ? (uint)data[0].Data.Count() : 0;

        private uint FrameCount => (uint)data.Count();

        private uint FrameDataSize
        {
            get
            {
                if (FrameItemCount == 0)
                    return 0;
                else
                {
                    uint bytes = 0;
                    foreach (DataCollection.DataBlock block in data[0].data)
                        bytes += block.DataLen;
                    return bytes;
                }
            }
        }


        // DATA PROCESSING

        public byte Vehicle => meta_vehicle;

        public string VehicleName => Value.Vehicle.Name.TryGetValue(Vehicle, out string_processor) != false ? string_processor : "-";

        public byte Track => meta_track;

        public string TrackName => Value.Track.Name.TryGetValue(Track, out string_processor) != false ? string_processor : "-";

        /* need to implement generalised functions to cut down the following but cbf atm lol */

        public Vector3 Position(int i)
        {
            if (data.Count() <= i)
                throw new IndexOutOfRangeException();
            Addr.PodState xOff = Addr.PodState.X, yOff = Addr.PodState.Y, zOff = Addr.PodState.Z;
            int xI = data[i].ValueExists(DataCollection.DataBlock.Path.PodState, (uint)xOff, Addr.GetLength(xOff));
            int yI = data[i].ValueExists(DataCollection.DataBlock.Path.PodState, (uint)yOff, Addr.GetLength(yOff));
            int zI = data[i].ValueExists(DataCollection.DataBlock.Path.PodState, (uint)zOff, Addr.GetLength(zOff));
            if (!(xI >= 0 && yI >= 0 && zI >= 0))
                throw new Exception("Position not found in data set.");
            float X = data[i].data[xI].GetValue((uint)xOff, Addr.GetType(xOff));
            float Y = data[i].data[yI].GetValue((uint)yOff, Addr.GetType(yOff));
            float Z = data[i].data[zI].GetValue((uint)zOff, Addr.GetType(zOff));
            return new Vector3(X, Y, Z);
        }

        public Vector3 PositionMin
        {
            get
            {
                Vector3 output = new Vector3(float.MaxValue);
                for (int i = 0; i < data.Count(); i++)
                    output = Vector3.Min(output, Position(i));
                return output;
            }
        }

        public Vector3 PositionMax
        {
            get
            {
                Vector3 output = new Vector3(float.MinValue);
                for (int i = 0; i < data.Count(); i++)
                    output = Vector3.Max(output, Position(i));
                return output;
            }
        }

        public Vector3 PositionRange => PositionMin.X > PositionMax.X ? Vector3.Zero : PositionMax - PositionMin;


        // CHECKING

        private bool CheckInRace(Racer r)
        {
            return r.GetData(Addr.Static.InRace) > 0;
        }

        private bool CheckRacePaused(Racer r)
        {
            return r.GetData(Addr.Static.PauseState) > 0;
        }

        private bool CheckUpdateable(Racer r)
        {
            return CheckInRace(r) && initialized;
        }

        public bool CheckExportable(bool t = false)
        {
            if (data.Count() <= 0)
                if (t)
                    throw new Exception("No replay data to export.");
                else
                    return false;
            return true;
        }
    }
}