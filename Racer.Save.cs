using SWE1R.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SWE1R.Racer
{
    public class Save
    {
        private const string gamePath = @"Z:\GOG\STAR WARS Racer",
            nameDef = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const byte timeLen = 0x64,
            nameLen = 0x20;
        private const float timeDef = 3599.99f;  // 0xD7FF6045
        private readonly static byte[] magicword = new byte[4] { 0x03, 0x00, 0x01, 0x00 };

        //SAVE.sav structure

        //0x00 byte[4], magic number
        //0x04 byte[0x50], file block
        //-> 0x00 string[0x20], most recent loaded filename?
        //-> 0x20-0x23 = ??? (0x21 = 1?, 0x22 = file slot?)
        //-> 0x24 byte, selected vehicle
        //-> 0x25 byte(flags), amateur circuit track unlocks, default 01
        //-> 0x26 byte(flags), semi-pro circuit track unlocks, default 01
        //-> 0x27 byte(flags), galactic circuit track unlocks, default 01
        //-> 0x28 byte(flags), invitational circuit track unlocks, default 00
        //-> 0x2A ushort(flags), amateur circuit placements (2bit/track)
        //-> 0x2C ushort(flags), semi-pro circuit placements (2bit/track)
        //-> 0x2E ushort(flags), galactic circuit placements (2bit/track)
        //-> 0x30 ushort(flags), invitational circuit placements (2bit/track)
        //-> 0x32-0x33 = ???
        //-> 0x34 uint32(flags), vehicle unlocks (bits), default 0x012E0200
        //-> 0x38 long(signed), truguts
        //-> 0x3C-0x3F = ??? (0x3C has data)
        //-> 0x40 byte, pit droids
        //-> 0x41 7byte, upgrade levels
        //-> 0x48 7byte, upgrade healths
        //-> 0x4F byte, always 0x00?

        public class Profile
        {
            public string Name;
            public Vehicle SelectedVehicle;
            public ProfileCircuit[] Circuits;
            public VehicleUnlockFlags Vehicles;
            public long Truguts;
            public byte PitDroids;
            public ProfileUpgrade[] Upgrades;

            public Profile() { }

            public Profile(string filename)
            {
                ReadFile(filename);
            }


            public void ReadFile(string filename)
            {
                using (FileStream file = File.OpenRead(filename))
                {
                    if (file.Length != 0x54 || !Win32.ByteArrayCompare(magicword, FileIO.ReadChunk(file, 4)))
                        throw new Exception("Invalid profile file.");

                    byte[] data = FileIO.ReadChunk(file, 0x50);

                    Profile profile = this;
                    InterpretData(data, ref profile);
                }
            }

            public static Profile ReadData(byte[] data)
            {
                if (data.Length != 0x50)
                    throw new Exception("Invalid data length.");
                Profile profile = new Profile();
                InterpretData(data, ref profile);
                return profile;
            }

            private static void InterpretData(byte[] data, ref Profile profile)
            {
                profile.Name = BitConverter.ToString(data, 0x0, 0x20);
                profile.SelectedVehicle = (Vehicle)data[0x24];
                profile.Truguts = BitConverter.ToInt32(data, 0x38);
                profile.PitDroids = data[0x40];
                profile.Vehicles = (VehicleUnlockFlags)BitConverter.ToUInt32(data, 0x34);
                profile.Circuits = new ProfileCircuit[4];
                for (var z = 0; z < 4; z++)
                    profile.Circuits[z] = new ProfileCircuit
                    {
                        Tracks = data[0x25 + z],
                        Placements = BitConverter.ToUInt16(data, 0x2A + z * 2)
                    };
                profile.Upgrades = new ProfileUpgrade[7];
                for (var z = 0; z < 7; z++)
                    profile.Upgrades[z] = new ProfileUpgrade
                    {
                        Level = data[0x41 + z],
                        Health = data[0x48 + z]
                    };
            }

            public string PrintString()
            {
                string output = "";
                output += Name + Environment.NewLine;
                output += Value.Vehicle.Name[(byte)SelectedVehicle] + Environment.NewLine;
                output += Truguts + " Truguts" + Environment.NewLine;
                output += PitDroids + " Pit Droids" + Environment.NewLine;
                output += Vehicles.ToString() + Environment.NewLine;
                for (byte a = 0; a < 4; a++)
                    switch (a)
                    {
                        case 0:
                            output += "AM:  " + ((CircuitAmateurFlags)Circuits[a].Tracks).ToString() + Environment.NewLine;
                            break;
                        case 1:
                            output += "SP:  " + ((CircuitSemiproFlags)Circuits[a].Tracks).ToString() + Environment.NewLine;
                            break;
                        case 2:
                            output += "GAL: " + ((CircuitGalacticFlags)Circuits[a].Tracks).ToString() + Environment.NewLine;
                            break;
                        case 3:
                            output += "INV: " + ((CircuitInvitationalFlags)Circuits[a].Tracks).ToString() + Environment.NewLine;
                            break;
                    }
                for (byte a = 0; a < 7; a++)
                    output += Value.Upgrade.Name[a] + ": Level " + Upgrades[a].Level + " (" + Helper.ByteToFloat(Upgrades[a].Health).ToString("0.0%") + " HP)" + Environment.NewLine;
                return output;
            }
        }

        public struct ProfileUpgrade
        {
            public byte Level;
            public byte Health;
        }
        public struct ProfileCircuit
        {
            public byte Tracks;
            public ushort Placements;
        }


        //tgfd.dat structure

        //0x000 byte[4], magic word
        //-> 03 00 01 00

        //0x004 0x14 length data block?
        //-> structure = ???
        //-> seems to have track/vehicle unlock stuff, maybe freeplay related

        //0x018 byte[4][0x50], file blocks
        //-> same format as profile save data

        //0x158 float[0x64], race times
        //-- 3lap times for all tracks in track id order
        //-- for each track entry, regular then mirrored mode times
        //-- same pattern repeats for 1lap times
        //-- default 3599.99 (0xD7FF6045) for no saved time
        //0x2E8 string[0x64], time names
        //-- same pattern for tracks as race times
        //-- 0x20 length strings
        //-- default AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA for no saved time, otherwise name padded with 0x00
        //0xF68 byte[0x64], time vehicles
        //-- same pattern for tracks as race times
        //-- defaults to track favourite for no saved time

        //0xFCC byte[0xC], EOF padding?
        //-- 0x00 only

        //game ignores name and pod if time is default

        //total length 0x0FD8

        public class Game
        {
            private List<RaceTimeSet> Times;
            private List<Profile> Players;

            public void ReadFile(string filename = gamePath + @"\data\player\tgfd.dat")
            {
                FileStream file = File.OpenRead(filename);

                if (file.Length != 0xFD8 || !Win32.ByteArrayCompare(magicword, FileIO.ReadChunk(file, 4)))
                    throw new Exception("Invalid save file.");

                byte[] data = FileIO.ReadChunk(file, 0xFD4);

                Times = new List<RaceTimeSet>();
                Players = new List<Profile>();

                // profiles
                for (var i = 0; i < 4; i++)
                {
                    byte[] d = new byte[0x50];
                    Array.Copy(data, 0x14 + i * 0x50, d, 0, 0x50);
                    Players.Add(Profile.ReadData(d));
                    Console.WriteLine("PROFILE " + i + ":");
                    Console.WriteLine(Players.Last().PrintString());
                }

                // times
                Console.WriteLine("".PadRight(30) + "3-Lap".PadRight(12) + "1-Lap".PadRight(12) + "3-Lap (M)".PadRight(12) + "1-Lap (M)".PadRight(12));
                for (byte trk = 0; trk < 25; trk++)
                {
                    RaceTimeSet set = new RaceTimeSet();
                    for (byte typ = 0; typ < 2; typ++)
                    {
                        for (byte mir = 0; mir < 2; mir++)
                        {
                            RaceTime time = new RaceTime();
                            time.Track = (Track)trk;
                            float t = BitConverter.ToSingle(data, 0x154 + (trk + typ * 25) * 8 + mir * 4);
                            time.Time = t == timeDef ? -1 : t;
                            time.Name = BitConverter.ToString(data, 0x2E4 + (trk + typ * 25) * 0x40 + mir * 0x20);
                            time.Vehicle = (Vehicle)data[0xF64 + (trk + typ * 25) * 2 + mir * 1];
                            time.Mirror = (mir % 2 == 1);
                            time.TimeType = typ > 0 ? RaceTimeType.BestLap : RaceTimeType.ThreeLap;
                            set.Times[mir * 2 + typ * 1] = time;
                        }
                    }
                    Console.WriteLine(Value.Track.Name[trk].PadRight(30) + set.Times[0].TimeString().PadRight(12) + set.Times[1].TimeString().PadRight(12) + set.Times[2].TimeString().PadRight(12) + set.Times[3].TimeString().PadRight(12));
                    Times.Add(set);
                }

                file.Dispose();
                file.Close();
            }
        }

        public struct RaceTime
        {
            public Track Track;
            public float Time;
            public bool Mirror;
            public RaceTimeType TimeType;
            public Vehicle Vehicle;
            public string Name;

            public string TimeString()
            {
                return (Time < 0 ? "NoTime" : TimeSpan.FromSeconds(Time).ToString("m\\:ss\\.fff"));
            }

            public string GetString()
            {
                return ((byte)Track).ToString("00") + "   " + (Time < 0 ? "NoTime  " : TimeSpan.FromSeconds(Time).ToString("m\\:ss\\.fff")) + "   " + TimeType + "   " + Mirror;
            }
        }

        public class RaceTimeSet
        {
            public RaceTime[] Times;

            public RaceTimeSet()
            {
                Times = new RaceTime[4];
            }
        }

        public enum RaceTimeType
        {
            ThreeLap = 0,
            BestLap = 1,
            Lap1 = 2,
            Lap2 = 3,
            Lap3 = 4,
            Lap4 = 5,
            Lap5 = 6
        }

        [Flags]
        public enum VehicleUnlockFlags
        {
            None = 0x000000,
            AnakinSkywalker = 0x000001,
            TeemtoPagalies = 0x000002,
            Sebulba = 0x000004,
            RattsTyerell = 0x000008,
            AldarBeedo = 0x000010,
            Mawhonic = 0x000020,
            ArkBumpyRoose = 0x000040,
            WanSandage = 0x000080,
            MarsGuo = 0x000100,
            EbeEndocott = 0x000200,
            DudBolt = 0x000400,
            Gasgano = 0x000800,
            CleggHoldfast = 0x001000,
            ElanMak = 0x002000,
            NevaKee = 0x004000,
            BozzieBaranta = 0x008000,
            BolesRoor = 0x010000,
            OdyMandrell = 0x020000,
            FudSang = 0x040000,
            BenQuadinaros = 0x080000,
            SlideParamita = 0x100000,
            ToyDampner = 0x200000,
            BullseyeNavior = 0x400000
        };

        public enum Vehicle
        {
            AnakinSkywalker = 0x00,
            TeemtoPagalies = 0x01,
            Sebulba = 0x02,
            RattsTyerell = 0x03,
            AldarBeedo = 0x04,
            Mawhonic = 0x05,
            ArkBumpyRoose = 0x06,
            WanSandage = 0x07,
            MarsGuo = 0x08,
            EbeEndocott = 0x09,
            DudBolt = 0x0A,
            Gasgano = 0x0B,
            CleggHoldfast = 0x0C,
            ElanMak = 0x0D,
            NevaKee = 0x0E,
            BozzieBaranta = 0x0F,
            BolesRoor = 0x10,
            OdyMandrell = 0x11,
            FudSang = 0x12,
            BenQuadinaros = 0x13,
            SlideParamita = 0x14,
            ToyDampner = 0x15,
            BullseyeNavior = 0x16
        };

        [Flags]
        public enum CircuitAmateurFlags
        {
            None = 0x00,
            TheBoontaTrainingCourse = 0x01,
            MonGazzaSpeedway = 0x02,
            BeedosWildRide = 0x04,
            AquilarisClassic = 0x08,
            Malastare100 = 0x10,
            Vengeance = 0x20,
            SpiceMineRun = 0x40,
            TournamentDone = 0x80
        };

        [Flags]
        public enum CircuitSemiproFlags
        {
            None = 0x00,
            SunkenCity = 0x01,
            HowlerGorge = 0x02,
            DugDerby = 0x04,
            ScrappersRun = 0x08,
            ZuggaChallenge = 0x10,
            BarooCoast = 0x20,
            BumpysBreakers = 0x40,
            TournamentDone = 0x80
        };

        [Flags]
        public enum CircuitGalacticFlags
        {
            None = 0x00,
            Executioner = 0x01,
            SebulbasLegacy = 0x02,
            GrabvineGateway = 0x04,
            AndobiMountainRun = 0x08,
            DethrosRevenge = 0x10,
            FireMountainRally = 0x20,
            TheBoontaClassic = 0x40,
            TournamentDone = 0x80
        };

        [Flags]
        public enum CircuitInvitationalFlags
        {
            None = 0x00,
            AndoPrimeCentrum = 0x01,
            Abyss = 0x02,
            TheGauntlet = 0x04,
            Inferno = 0x08
        };

        public enum Track
        {
            TheBoontaTrainingCourse = 0x00,
            MonGazzaSpeedway = 0x10,
            BeedosWildRide = 0x02,
            AquilarisClassic = 0x06,
            Malastare100 = 0x16,
            Vengeance = 0x13,
            SpiceMineRun = 0x11,
            SunkenCity = 0x07,
            HowlerGorge = 0x03,
            DugDerby = 0x17,
            ScrappersRun = 0x09,
            ZuggaChallenge = 0x12,
            BarooCoast = 0x0C,
            BumpysBreakers = 0x08,
            Executioner = 0x14,
            SebulbasLegacy = 0x18,
            GrabvineGateway = 0x0D,
            AndobiMountainRun = 0x04,
            DethrosRevenge = 0x0A,
            FireMountainRally = 0x0E,
            TheBoontaClassic = 0x01,
            AndoPrimeCentrum = 0x05,
            Abyss = 0x0B,
            TheGauntlet = 0x15,
            Inferno = 0x0F
        };

        public enum TrackPlacing
        {
            First = 0x3,
            Second = 0x2,
            Third = 0x1,
            Fourth = 0x0,
            None = 0x0
        }
    }
}