using SWE1R.Util;
using System;
using System.Collections.Generic;

namespace SWE1R.Racer
{
    readonly public struct Value
    {
        readonly public struct Vehicle
        {
            public const byte AnakinSkywalker = 0x00,
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
                BullseyeNavior = 0x16;

            readonly public static Dictionary<byte, string> Name = new Dictionary<byte, string>()
                {
                    { 0,  "Anakin Skywalker" },
                    { 1,  "Teemto Pagalies" },
                    { 2,  "Sebulba" },
                    { 3,  "Ratts Tyerell" },
                    { 4,  "Aldar Beedo" },
                    { 5,  "Mawhonic" },
                    { 6,  "Ark 'Bumpy' Roose" },
                    { 7,  "Wan Sandage" },
                    { 8,  "Mars Guo" },
                    { 9,  "Ebe Endocott" },
                    { 10, "Dud Bolt" },
                    { 11, "Gasgano" },
                    { 12, "Clegg Holdfast" },
                    { 13, "Elan Mak" },
                    { 14, "Neva Kee" },
                    { 15, "Bozzie Baranta" },
                    { 16, "Boles Roor" },
                    { 17, "Ody Mandrell" },
                    { 18, "Fud Sang" },
                    { 19, "Ben Quadinaros" },
                    { 20, "Slide Paramita" },
                    { 21, "Toy Dampner" },
                    { 22, "'Bullseye' Navior" }
                };

            readonly public static Dictionary<byte, VehicleStats> Stats = new Dictionary<byte, VehicleStats>()
                {
                    {0,new VehicleStats(0.5f,300f,110f,3f,490f,30f,60f,200f,13f,9f,4.99f,0.4f,50f,0.6f,5f)},
                    {1,new VehicleStats(0.5f,260f,90f,1.7f,479f,30f,80f,195f,14f,6f,4.99f,0.43f,50f,0.5f,8f)},
                    {2,new VehicleStats(0.38f,228f,95f,3.2f,600f,38f,50f,185f,9f,2f,4.99f,0.19f,80f,0.3f,7f)},
                    {3,new VehicleStats(0.35f,238f,90f,4f,520f,33f,80f,300f,12f,5f,4.99f,0.3f,55f,0.45f,7f)},
                    {4,new VehicleStats(0.3f,250f,85f,4f,517f,35f,85f,360f,10.5f,4.5f,4.99f,0.25f,68f,0.4f,7f)},
                    {5,new VehicleStats(0.36f,224f,100f,3.75f,480f,41f,80f,350f,13f,7f,4.99f,0.2f,60f,0.48f,7f)},
                    {6,new VehicleStats(0.3f,202f,85f,1f,485f,30f,80f,210f,10.5f,6.5f,6.5f,0.1f,70f,0.25f,6f)},
                    {7,new VehicleStats(0.8f,294f,95f,1.9f,480f,25f,70f,180f,9f,3f,7f,0.19f,60f,0.5f,7f)},
                    {8,new VehicleStats(0.6f,288f,100f,2.3f,540f,30f,85f,315f,7.5f,2.1f,6f,0.35f,70f,0.5f,10f)},
                    {9,new VehicleStats(0.6f,294f,100f,2.5f,500f,40f,70f,190f,15.2f,11f,4.99f,0.45f,45f,0.7f,4.8f)},
                    {10,new VehicleStats(0.54f,215f,80f,3f,505f,35f,90f,230f,8.6f,2.5f,4.99f,0.2f,70f,0.35f,5.5f)},
                    {11,new VehicleStats(0.43f,238f,82f,3.3f,510f,43f,83f,310f,12.5f,1.7f,4.99f,0.4f,63f,0.43f,4.2f)},
                    {12,new VehicleStats(0.5f,252f,89f,1.75f,495f,45f,80f,303f,11.5f,5f,6f,0.31f,55f,0.43f,7f)},
                    {13,new VehicleStats(0.3f,224f,95f,3.75f,480f,40f,70f,360f,10f,2.5f,4.99f,0.3f,53f,0.5f,6f)},
                    {14,new VehicleStats(0.8f,230f,115f,1f,480f,30f,70f,280f,11.5f,3.3f,4.99f,0.32f,55f,0.6f,7f)},
                    {15,new VehicleStats(0.33f,294f,90f,2.1f,485f,42f,83f,275f,11.8f,3.5f,4.99f,0.3f,60f,0.55f,7f)},
                    {16,new VehicleStats(0.3f,280f,83f,2.85f,590f,35f,85f,390f,9.5f,2.7f,4.99f,0.18f,62f,0.3f,7f)},
                    {17,new VehicleStats(0.45f,238f,90f,1.8f,475f,30f,65f,240f,11f,4.4f,5f,0.4f,57f,0.6f,5.2f)},
                    {18,new VehicleStats(0.35f,245f,90f,2.85f,490f,30f,75f,250f,12f,6.5f,4.99f,0.39f,53f,0.55f,7f)},
                    {19,new VehicleStats(0.45f,203f,89f,3f,575f,40f,95f,400f,8f,2f,4.99f,0.28f,73f,0.45f,7f)},
                    {20,new VehicleStats(0.43f,297f,120f,1.95f,475f,30f,80f,200f,16f,12f,4.99f,0.63f,40f,0.8f,7f)},
                    {21,new VehicleStats(0.5f,270f,86f,1.75f,485f,25f,70f,240f,12.5f,10f,4.99f,0.5f,40f,0.65f,7f)},
                    {22,new VehicleStats(0.7f,322f,120f,1.8f,480f,25f,70f,300f,15f,11f,4.99f,0.55f,45f,0.77f,7f)}
                };
        }

        public struct VehicleStats
        {
            public const byte AntiSkid = 0,
                TurnResponse = 1,
                MaxTurnRate = 2,
                Acceleration = 3,
                MaxSpeed = 4,
                AirBrakeInv = 5,
                DecelInv = 6,
                BoostThrust = 7,
                HeatRate = 8,
                CoolRate = 9,
                HoverHeight = 10,
                RepairRate = 11,
                BumpMass = 12,
                DmgImmunity = 13,
                ISectRadius = 14;
            readonly public Dictionary<byte, float> stats;

            public VehicleStats(float AS, float TR, float MTR, float A, float MS,
                float ABI, float DI, float BT, float HR, float CR,
                float HH, float RR, float BM, float DmI, float ISR)
            {
                stats = new Dictionary<byte, float>(15);
                stats[AntiSkid] = Math.Max(AS, 0);
                stats[TurnResponse] = Math.Max(TR, 0);
                stats[MaxTurnRate] = Math.Max(MTR, 0);
                stats[Acceleration] = Math.Max(A, 0);
                stats[MaxSpeed] = Math.Max(MS, 0);
                stats[AirBrakeInv] = Math.Max(ABI, 0);
                stats[DecelInv] = Math.Max(DI, 0);
                stats[BoostThrust] = Math.Max(BT, 0);
                stats[HeatRate] = Math.Max(HR, 0);
                stats[CoolRate] = Math.Max(CR, 0);
                stats[HoverHeight] = Math.Max(HH, 0);
                stats[RepairRate] = Math.Max(RR, 0);
                stats[BumpMass] = Math.Max(BM, 0);
                stats[DmgImmunity] = Math.Max(DmI, 0);
                stats[ISectRadius] = Math.Max(ISR, 0);
            }

            public float Stat(byte stat)
            {
                byte s = Helper.Clamp(stat, (byte)0, (byte)14);
                return stats[s];
            }

            public float CalculateUpgradedStat(byte stat, byte level, byte health = 0xFF)
            {
                byte s = Helper.Clamp(stat, (byte)0, (byte)14);
                float bs = stats[s];
                if (Upgrade.MapFromStat.ContainsKey(s))
                {
                    UpgradeStats u = Upgrade.Stats[Upgrade.MapFromStat[s]];
                    return u.CalculateStat(bs, level, health);
                }
                else
                    return bs;
            }
        }

        readonly public struct Upgrade
        {
            public const byte AntiSkid = 0x0,
                TurnResponse = 0x1,
                Acceleration = 0x2,
                MaxSpeed = 0x3,
                AirBrakeInv = 0x4,
                CoolRate = 0x5,
                RepairRate = 0x6;

            readonly public static Dictionary<byte, string> Name = new Dictionary<byte, string>()
                {
                    {AntiSkid,     "Anti Skid" },
                    {TurnResponse, "Turn Response" },
                    {Acceleration, "Acceleration" },
                    {MaxSpeed,     "Max Speed" },
                    {AirBrakeInv,  "Air Brake Inv" },
                    {CoolRate,     "Cool Rate" },
                    {RepairRate,   "Repair Rate" }
                };

            readonly public static Dictionary<byte, UpgradeStats> Stats = new Dictionary<byte, UpgradeStats>()
                {
                    {AntiSkid,     new UpgradeStats(0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.01f, 1f) },
                    {TurnResponse, new UpgradeStats(116, 242, 348, 464, 578, 50, 1000) },
                    {Acceleration, new UpgradeStats(0.14f, 0.28f, 0.42f, 0.56f, 0.70f, 0.1f, 5f, 1) },
                    {MaxSpeed,     new UpgradeStats(40, 80, 120, 160, 200, 450, 650) },
                    {AirBrakeInv,  new UpgradeStats(0.08f, 0.17f, 0.26f, 0.35f, 0.44f, 1, 1000, 1) },
                    {CoolRate,     new UpgradeStats(1.6f, 3.2f, 4.8f, 6.4f, 8.0f, 1, 20) },
                    {RepairRate,   new UpgradeStats(0.1f, 0.2f, 0.3f, 0.4f, 0.45f, 0, 1) }
                };

            readonly public static Dictionary<byte, byte> MapToStat = new Dictionary<byte, byte>()
                {
                    {AntiSkid,     VehicleStats.AntiSkid },
                    {TurnResponse, VehicleStats.TurnResponse },
                    {Acceleration, VehicleStats.Acceleration },
                    {MaxSpeed,     VehicleStats.MaxSpeed },
                    {AirBrakeInv,  VehicleStats.AirBrakeInv },
                    {CoolRate,     VehicleStats.CoolRate },
                    {RepairRate,   VehicleStats.RepairRate }
                };

            readonly public static Dictionary<byte, byte> MapFromStat = new Dictionary<byte, byte>()
                {
                    {VehicleStats.AntiSkid,     AntiSkid },
                    {VehicleStats.TurnResponse, TurnResponse },
                    {VehicleStats.Acceleration, Acceleration },
                    {VehicleStats.MaxSpeed,     MaxSpeed },
                    {VehicleStats.AirBrakeInv,  AirBrakeInv },
                    {VehicleStats.CoolRate,     CoolRate },
                    {VehicleStats.RepairRate,   RepairRate }
                };
        }

        public struct UpgradeStats
        {
            readonly private float[] stat;
            readonly public float Min, Max;
            readonly private byte type;

            public UpgradeStats(float lv1, float lv2, float lv3, float lv4, float lv5, float mn, float mx, byte t = 0)
            {
                Min = mn;
                Max = mx;
                stat = new float[5] {
                        (t == 0) ? Math.Max(lv1, 0) : Helper.Clamp(lv1, 0, 1),
                        (t == 0) ? Math.Max(lv2, 0) : Helper.Clamp(lv2, 0, 1),
                        (t == 0) ? Math.Max(lv3, 0) : Helper.Clamp(lv3, 0, 1),
                        (t == 0) ? Math.Max(lv4, 0) : Helper.Clamp(lv4, 0, 1),
                        (t == 0) ? Math.Max(lv5, 0) : Helper.Clamp(lv5, 0, 1)
                    };
                type = t;
            }

            public float Modifier(byte level)
            {
                byte lvl = Helper.Clamp((byte)(level - 1), (byte)0, (byte)4);
                return stat[lvl];
            }

            public float CalculateStat(float bs, byte l, byte h = 0xFF)
            {
                byte lvl = Helper.Clamp((byte)(l - 1), (byte)0, (byte)4);
                float hp = Helper.ByteToFloat(h);
                if (type == 0)
                    return Helper.Clamp((hp * stat[lvl] + bs), Min, Max);
                else
                    return Helper.Clamp(((1 - hp) * stat[lvl] - (stat[lvl] - 1) * bs), Min, Max);
            }
        }

        readonly public struct Track
        {
            public const byte TheBoontaTrainingCourse = 0x00,
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
                Inferno = 0x0F;

            readonly public static Dictionary<byte, string> Name = new Dictionary<byte, string>()
                {
                    { 0,  "The Boonta Training Course" }, // Amateur
                    { 16, "Mon Gazza Speedway" },
                    { 2,  "Beedo's Wild Ride" },
                    { 6,  "Aquilaris Classic" },
                    { 22, "Malastare 100" },
                    { 19, "Vengeance" },
                    { 17, "Spice Mine Run" },
                    { 7,  "Sunken City" },                // Semi-Pro
                    { 3,  "Howler Gorge" },
                    { 23, "Dug Derby" },
                    { 9,  "Scrapper's Run" },
                    { 18, "Zugga Challenge" },
                    { 12, "Baroo Coast" },
                    { 8,  "Bumpy's Breakers" },
                    { 20, "Executioner" },                // Galactic
                    { 24, "Sebulba's Legacy" },
                    { 13, "Grabvine Gateway" },
                    { 4,  "Andobi Mountain Run" },
                    { 10, "Dethro's Revenge" },
                    { 14, "Fire Mountain Rally" },
                    { 1,  "The Boonta Classic" },
                    { 5,  "Ando Prime Centrum" },         // Invitational
                    { 11, "Abyss" },
                    { 21, "The Gauntlet" },
                    { 15, "Inferno" }
                };

            readonly public static Dictionary<byte, byte> Favorite = new Dictionary<byte, byte>()
                {
                    { 0,  Vehicle.Sebulba }, // Amateur
                    { 16, Vehicle.TeemtoPagalies },
                    { 2,  Vehicle.AldarBeedo },
                    { 6,  Vehicle.CleggHoldfast },
                    { 22, Vehicle.DudBolt },
                    { 19, Vehicle.FudSang },
                    { 17, Vehicle.MarsGuo },
                    { 7,  Vehicle.BullseyeNavior },                // Semi-Pro
                    { 3,  Vehicle.RattsTyerell },
                    { 23, Vehicle.ElanMak },
                    { 9,  Vehicle.WanSandage },
                    { 18, Vehicle.BolesRoor },
                    { 12, Vehicle.NevaKee },
                    { 8,  Vehicle.ArkBumpyRoose },
                    { 20, Vehicle.ToyDampner },                // Galactic
                    { 24, Vehicle.Sebulba },
                    { 13, Vehicle.AnakinSkywalker },
                    { 4,  Vehicle.Mawhonic },
                    { 10, Vehicle.OdyMandrell },
                    { 14, Vehicle.EbeEndocott },
                    { 1,  Vehicle.Sebulba },
                    { 5,  Vehicle.SlideParamita },         // Invitational
                    { 11, Vehicle.BozzieBaranta },
                    { 21, Vehicle.Gasgano },
                    { 15, Vehicle.BenQuadinaros }
                };
        }

        readonly public struct World
        {
            public const byte Tatooine = 0,
                AndoPrime = 1,
                Aquilaris = 2,
                OrdIbanna = 3,
                Baroonda = 4,
                MonGazza = 5,
                OovoIV = 6,
                Malastare = 7;

            readonly public static Dictionary<byte, string> Name = new Dictionary<byte, string>()
            {
                // based on track order, need to verify (don't have world addr yet)
                { 0,  "Tatooine" },
                { 1,  "Ando Prime" },
                { 2,  "Aquilaris" },
                { 3,  "Ord Ibanna" },
                { 4,  "Baroonda" },
                { 5,  "Mon Gazza" },
                { 6,  "Oovo IV" },
                { 7,  "Malastare" }
            };
        }
    }
}