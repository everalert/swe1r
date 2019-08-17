using SWE1R.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SWE1R.Racer
{
    public static class Save
    {
        private const string name_default = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const byte time_count = 0x64, name_length = 0x20, profile_count = 4;
        private const float time_default = 3599.99f;  // 0xD7FF6045
        private readonly static byte[] magicword = new byte[4] { 0x03, 0x00, 0x01, 0x00 };

        public static int MaxNameLength => (int)name_length;
        public static float DefaultTime => time_default;
        public static string DefaultName => name_default;

        //SAVE.sav structure

        //0x00 byte[4], magic number
        //0x04 byte[0x50], file block
        //-> 0x00 string[0x20], filename
        //-> 0x20-0x21 = ??? (0x21 = 1?)
        //-> 0x22 byte, file slot number (0-3)
        //-> 0x23=??
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

        public class ProfileSave
        {
            public const int ProfileLength = 0x50,
                NameLocation = 0x00,
                NameLength = 0x20,
                SelectedVehicleLocation = 0x24,
                TracksLocation = 0x25,
                PlacingsLocation = 0x2A,
                VehiclesLocation = 0x34,
                TrugutsLocation = 0x38,
                MoneyLocation = TrugutsLocation,
                DroidsLocation = 0x40,
                UpgradeLevelsLocation = 0x41,
                UpgradeHealthsLocation = 0x48;
            public const string Extension = "sav";

            private byte[] _rawdata;
            private string _playername;
            private Vehicle _selectedvehicle;
            private Circuit[] _circuitstatus;
            private VehicleUnlockFlags _availablevehicles;
            private int _money;
            private byte _droids;
            private Upgrade[] _upgradestatus;

            private bool data_imported = false;

            public byte[] Bytes
            {
                get
                {
                    byte[] data = OriginalBytes;
                    Array.Copy(Helper.WriteNullTerminatedBytes(_playername, NameLength), 0, data, NameLocation, NameLength);
                    Array.Copy(BitConverter.GetBytes(_money), 0, data, MoneyLocation, 4);
                    Array.Copy(BitConverter.GetBytes((int)_availablevehicles), 0, data, VehiclesLocation, 4);
                    data[SelectedVehicleLocation] = (byte)_selectedvehicle;
                    data[DroidsLocation] = _droids;
                    for (int i = 0; i < _circuitstatus.Length; i++)
                    {
                        data[TracksLocation + i] = _circuitstatus[i].Tracks;
                        Array.Copy(BitConverter.GetBytes(_circuitstatus[i].Placements), 0, data, PlacingsLocation + i * 2, 2);
                    }
                    for (int i = 0; i < _upgradestatus.Length; i++)
                    {
                        data[UpgradeLevelsLocation + i] = _upgradestatus[i].Level;
                        data[UpgradeHealthsLocation + i] = _upgradestatus[i].Health;
                    }
                    return data;
                }
            }

            public byte[] OriginalBytes => _rawdata.ToArray();

            public Circuit AmateurCircuit => _circuitstatus[0];

            public Circuit SemiProCircuit => _circuitstatus[1];

            public Circuit GalacticCircuit => _circuitstatus[2];

            public Circuit InvitationalCircuit => _circuitstatus[3];

            public string Player
            {
                get { return _playername == name_default ? "NoName" : _playername; }
                set { _playername = value.Substring(0, Math.Min(value.Length, name_length)); }
            }

            public Vehicle SelectedVehicle {
                get { return _selectedvehicle; }
                set { _selectedvehicle = value; }
            }

            public int Truguts
            {
                get { return _money; }
                set { _money = value; }
            }

            public byte PitDroids
            {
                get { return _droids; }
                set { _droids = Helper.Clamp(value, (byte)1, (byte)7); }
            }

            public Upgrade[] Upgrades => _upgradestatus;

            public VehicleUnlockFlags AvailableVehicles
            {
                get { return _availablevehicles; }
                set { _availablevehicles = value; }
            }

            public TrackUnlockFlags AvailableTracks
            {
                get
                {
                    TrackUnlockFlags output = 0;
                    for (int i = 0; i < _circuitstatus.Length; i++)
                        output |= (TrackUnlockFlags)((uint)_circuitstatus[i].Tracks<<i*8);
                    return output;
                }
                set
                {
                    byte[] input = BitConverter.GetBytes((int)value);
                    for (int i = 0; i < input.Length; i++)
                        _circuitstatus[i].Tracks = input[i];
                }
            }

            public TrackPlacing GetPlacement(TrackUnlockFlags t)
            {
                int tInt = (int)t, c = 0;
                byte i = 0;
                while (tInt >> c * 8 > byte.MaxValue)
                    c++;
                while (tInt >> c * 8 + i > 1)
                    i++;
                return _circuitstatus[c].GetPlacement(i);
            }

            public void SetPlacement(TrackUnlockFlags t, TrackPlacing p)
            {
                int tInt = (int)t, c = 0;
                byte i = 0;
                while (tInt >> c * 8 > byte.MaxValue)
                    c++;
                while (tInt >> c * 8 + i > 1)
                    i++;
                _circuitstatus[c].SetPlacement(i, p);
            }

            private ProfileSave() { }

            public ProfileSave(string filename)
            {
                Import(filename);
            }


            public void Import(string filename)
            {
                using (FileStream file = File.OpenRead(filename))
                {
                    if (file.Length != 0x54 || !Win32.ByteArrayCompare(magicword, FileIO.ReadChunk(file, 4)))
                        throw new InvalidDataException();

                    ProfileSave profile = this;
                    ReadData(FileIO.ReadChunk(file, 0x50), ref profile);
                }
            }

            public void Export(string filename)
            {
                if (!data_imported)
                    throw new InvalidDataException();

                if (!Helper.CheckFilenameFormat(Path.GetFileName(filename)))
                    throw new FormatException("File name contains invalid characters.");

                if (File.Exists(filename)) { /*check to confirm? or do it before calling this?*/ }

                using (FileStream file = File.Create(filename))
                {
                    FileIO.WriteChunk(file, magicword);
                    byte[] data = Bytes;
                    //write correct filename/playername combination without interfering with profile data
                    if (Path.GetFileNameWithoutExtension(filename) != _playername)
                        Array.Copy(Helper.WriteNullTerminatedBytes(Path.GetFileNameWithoutExtension(filename), NameLength), 0, data, NameLocation, NameLength);
                    FileIO.WriteChunk(file, data);
                }
            }

            public static ProfileSave ReadData(byte[] data)
            {
                if (data.Length != 0x50)
                    throw new InvalidDataException();
                ProfileSave profile = new ProfileSave();
                ReadData(data, ref profile);
                return profile;
            }

            private static void ReadData(byte[] data, ref ProfileSave profile)
            {
                profile._rawdata = data;
                profile._playername = Helper.ReadNullTerminatedString(new ArraySegment<byte>(data, 0x0, 0x20).ToArray());
                profile._selectedvehicle = (Vehicle)data[0x24];
                profile._money = BitConverter.ToInt32(data, 0x38);
                profile._droids = data[0x40];
                profile._availablevehicles = (VehicleUnlockFlags)BitConverter.ToUInt32(data, 0x34);
                profile._circuitstatus = new Circuit[4];
                for (var z = 0; z < 4; z++)
                    profile._circuitstatus[z] = new Circuit(data[0x25 + z], BitConverter.ToUInt16(data, 0x2A + z * 2));
                profile._upgradestatus = new Upgrade[7];
                for (var z = 0; z < 7; z++)
                    profile._upgradestatus[z] = new Upgrade(data[0x41 + z], data[0x48 + z]);

                profile.data_imported = true;
            }

            public class Upgrade
            {
                private byte _level;
                private byte _health;

                public Upgrade(byte l, byte h)
                {
                    _level = l;
                    _health = h;
                }

                public byte Level
                {
                    get { return _level; }
                    set { _level = Helper.Clamp(value, (byte)0, (byte)5); }
                }

                public byte Health
                {
                    get { return _health; }
                    set { _health = value; }
                }
            }

            public class Circuit
            {
                public byte Tracks;
                public ushort Placements;

                public Circuit(byte t, ushort p)
                {
                    Tracks = t;
                    Placements = p;
                }

                public TrackPlacing GetPlacement(byte t)
                {
                    return (TrackPlacing)(((0b11 << t*2) & Placements) >> t*2);
                }

                public void SetPlacement(byte t, TrackPlacing p)
                {
                    Placements &= (ushort)~(0b11 << t * 2);
                    Placements |= (ushort)((int)p << t * 2);
                }
            }
        }


        //tgfd.dat structure

        //0x000 byte[4], magic word
        //-> 03 00 01 00

        //0x004 0x14 length data block?
        //-> 0x00-0x04 = always 9A 1C A4 07 01 ????
        //-> 0x05 byte, sfx volume
        //-> 0x06 byte, music/cs volume
        //-> 0x07 = ?
        //-> 0x08 = ? dflt 03, does change, upgrade unlocks?
        //-> 0x09-0x0A = ?
        //-> 0x0C uint32(flags), freeplay track unlocks, default 0x07030100
        //-> 0x10 uint32(flags), freeplay pod unlocks, default 0x012E0200

        //0x018 byte[4][0x50], file blocks
        //-> same format as profile save data
        //-> game itself might not even use slots 2+? seems to just raw copy/save bytes to and from memory when manipulating the file without validating the data block
        //-> also there was another location in memory that i forgot that seemed to have like 20+ profile slots

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

        public class GameSave
        {
            private const int FileLength = 0xFD8, DataLocation = 0x004, SettingsLocation = 0x004, ProfilesLocation = 0x018, TimesLocation = 0x158, NamesLocation = 0x2E8, VehiclesLocation = 0xF68;
            private bool data_imported = false;
            public const string Extension = "dat";

            private byte[] _rawdata;
            private GeneralSettings _settings;
            private RaceTime[] _times;
            private ProfileSave[] _profiles;

            public byte[] Bytes {
                get {
                    byte[] data = OriginalBytes;
                    Array.Copy(_settings.Bytes, 0, data, SettingsLocation - DataLocation, GeneralSettings.SettingsLength);
                    for (int i = 0; i < _profiles.Length; i++)
                        Array.Copy(_profiles[i].Bytes, 0, data, ProfilesLocation - DataLocation + i * ProfileSave.ProfileLength, ProfileSave.ProfileLength);
                    for (int i = 0; i < time_count; i++)
                    {
                        Array.Copy(BitConverter.GetBytes(_times[i].Time < 0 ? DefaultTime : _times[i].Time), 0, data, TimesLocation - DataLocation + i * 4, 4);
                        Array.Copy(Helper.WriteNullTerminatedBytes(_times[i].Name, MaxNameLength), 0, data, NamesLocation - DataLocation + i * MaxNameLength, MaxNameLength);
                        data[VehiclesLocation - DataLocation + i] = (byte)_times[i].Vehicle;
                    }
                    return data;
                }
            }

            public byte[] OriginalBytes => _rawdata.ToArray();

            public RaceTime[] Times => _times;
            public RaceTime[] ImmutableTimes => _times.ToArray();
            public ProfileSave[] Profiles => _profiles;
            public GeneralSettings GeneralSettings => _settings;

            public void Import(string filename)
            {
                using (FileStream file = File.OpenRead(filename))
                {

                    if (file.Length != FileLength || !Win32.ByteArrayCompare(magicword, FileIO.ReadChunk(file, 4)))
                        throw new Exception("Invalid save file.");

                    byte[] d;
                    byte[] data = FileIO.ReadChunk(file, FileLength-DataLocation);
                    _rawdata = data;

                    //Settings
                    d = new byte[GeneralSettings.SettingsLength];
                    Array.Copy(data, SettingsLocation - DataLocation, d, 0, GeneralSettings.SettingsLength);
                    _settings = new GeneralSettings(d);

                    // profiles
                    _profiles = new ProfileSave[profile_count];
                    for (var i = 0; i < profile_count; i++)
                    {
                        d = new byte[ProfileSave.ProfileLength];
                        Array.Copy(data, ProfilesLocation - DataLocation + i * ProfileSave.ProfileLength, d, 0, ProfileSave.ProfileLength);
                        _profiles[i] = ProfileSave.ReadData(d);
                    }

                    _times = new RaceTime[time_count];
                    for (int i = 0; i < time_count; i++)
                    {
                        RaceTime t = new RaceTime();
                        t._track = (Track)Math.Floor((decimal)i % 50 / 2);
                        float time = BitConverter.ToSingle(data, TimesLocation - DataLocation + i * 4);
                        t._time = time >= DefaultTime ? -1 : time;
                        t.Name = Helper.ReadNullTerminatedString(new ArraySegment<byte>(data, NamesLocation - DataLocation + i * MaxNameLength, MaxNameLength).ToArray());
                        t._vehicle = (Vehicle)data[VehiclesLocation - DataLocation + i];
                        t._mirror = i % 2 == 1;
                        t._timetype = i >= 50 ? RaceTimeType.BestLap : RaceTimeType.ThreeLap;
                        _times[i] = t;
                    }

                    data_imported = true;
                }
            }

            public void Export(string filename)
            {
                if (!data_imported)
                    throw new InvalidDataException();

                if (!Helper.CheckFilenameFormat(Path.GetFileName(filename)))
                    throw new FormatException("File name contains invalid characters.");

                if (File.Exists(filename)) { /*check to confirm? or do it before calling this?*/ }

                using (FileStream file = File.Create(filename))
                {
                    FileIO.WriteChunk(file, magicword);
                    FileIO.WriteChunk(file, Bytes);
                }
            }
        }

        public struct RaceTime
        {
            public Track _track;
            public float _time;
            public bool _mirror;
            public RaceTimeType _timetype;
            public Vehicle _vehicle;
            private string _name;

            public float Time
            {
                get { return _time; }
                set { _time = value >= time_default || value < 0 ? -1 : value; }
            }

            public string TimeString => _time < 0 ? "NoTime" : Helper.SecondsToTimeString(_time);

            public string FullTimeString => _time < 0 ? "NoTime" : Helper.SecondsToTimeString(_time, 7);

            public string Name
            {
                get { return _name == name_default ? "NoName" : _name; }
                set { _name = value.Substring(0, Math.Min(value.Length, name_length)); }
            }

            public Vehicle Vehicle
            {
                get { return _vehicle; }
                set { _vehicle = value; }
            }

            public string VehicleName => Value.Vehicle.Name[(byte)_vehicle];

            public Track Track => _track;

            public string TrackName => Value.Track.Name[(byte)_track];

            public string Summary => _track + Environment.NewLine + TimeString + Environment.NewLine + Name;

            public bool Mirror => _mirror;

            public RaceTimeType TimeType => _timetype;

            public static bool TimeIsValid(float t)
            {
                return t >= 0 && t < time_default;
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

        public class GeneralSettings
        {
            public const int SettingsLength = 0x14,
                VolSFXLocation = 0x05,
                VolMusicLocation = 0x06,
                VehiclesLocation = 0x10,
                TracksLocation = 0x0C;

            private byte[] _rawdata;
            private byte _volsfx, _volmusic;
            private VehicleUnlockFlags _vehicles;
            private TrackUnlockFlags _tracks;

            public byte[] Bytes
            {
                get
                {
                    byte[] data = OriginalBytes;
                    data[VolSFXLocation] = _volsfx;
                    data[VolMusicLocation] = _volmusic;
                    Array.Copy(BitConverter.GetBytes((int)_tracks), 0, data, TracksLocation, 4);
                    Array.Copy(BitConverter.GetBytes((int)_vehicles), 0, data, VehiclesLocation, 4);
                    return data;
                }
            }

            public byte[] OriginalBytes => _rawdata.ToArray();

            public float VolSFX
            {
                get { return Helper.ByteToFloat(_volsfx); }
                set { _volsfx = Helper.FloatToByte(value); }
            }

            public float VolMusic
            {
                get { return Helper.ByteToFloat(_volmusic); }
                set { _volmusic = Helper.FloatToByte(value); }
            }

            public VehicleUnlockFlags AvailableVehicles
            {
                get { return _vehicles; }
                set { _vehicles = value; }
            }

            public TrackUnlockFlags AvailableTracks
            {
                get { return _tracks; }
                set { _tracks = value; }
            }

            private GeneralSettings() { }

            public GeneralSettings(byte[] d)
            {
                GeneralSettings s = this;
                ReadData(d, ref s);
            }

            public static GeneralSettings ReadData(byte[] d)
            {
                GeneralSettings s = new GeneralSettings();
                ReadData(d, ref s);
                return s;
            }

            public static void ReadData(byte[] d, ref GeneralSettings s)
            {
                if (d.Length != SettingsLength)
                    throw new InvalidDataException();

                s._rawdata = d;
                s._volsfx = d[VolSFXLocation];
                s._volmusic = d[VolMusicLocation];
                s._vehicles = (VehicleUnlockFlags)BitConverter.ToUInt32(d, VehiclesLocation);
                s._tracks = (TrackUnlockFlags)BitConverter.ToUInt32(d, TracksLocation);
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

        public enum Vehicle : byte
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

        [Flags]
        public enum TrackUnlockFlags
        {
            TheBoontaTrainingCourse = 0x1,
            MonGazzaSpeedway = 0x2,
            BeedosWildRide = 0x4,
            AquilarisClassic = 0x8,
            Malastare100 = 0x10,
            Vengeance = 0x20,
            SpiceMineRun = 0x40,
            AmateurCircuitDone = 0x80,
            SunkenCity = 0x100,
            HowlerGorge = 0x200,
            DugDerby = 0x400,
            ScrappersRun = 0x800,
            ZuggaChallenge = 0x1000,
            BarooCoast = 0x2000,
            BumpysBreakers = 0x4000,
            SemiProCircuitDone = 0x8000,
            Executioner = 0x10000,
            SebulbasLegacy = 0x20000,
            GrabvineGateway = 0x40000,
            AndobiMountainRun = 0x80000,
            DethrosRevenge = 0x100000,
            FireMountainRally = 0x200000,
            TheBoontaClassic = 0x400000,
            GalacticCircuitDone = 0x800000,
            AndoPrimeCentrum = 0x1000000,
            Abyss = 0x2000000,
            TheGauntlet = 0x4000000,
            Inferno = 0x8000000
        };

        public enum TrackPlacing
        {
            First = 0b11,
            Second = 0b10,
            Third = 0b01,
            FourthOrNone = 0b00
        }

        public static Dictionary<TrackPlacing, string> TrackPlacingText = new Dictionary<TrackPlacing, string>()
        {
            {TrackPlacing.First, "1st" },
            {TrackPlacing.Second, "2nd" },
            {TrackPlacing.Third, "3rd" },
            {TrackPlacing.FourthOrNone, "4th or None" }
        };
    }
}