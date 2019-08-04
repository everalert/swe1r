using Path = SWE1R.Racer.DataCollection.DataBlock.Path;

namespace SWE1R.Racer
{
    public class GameState
    {
        // eventually, rename this to State and move current State class to Savestate

        private uint text_base = (uint)Addr.Static.Text01;
        private uint text_len = Addr.GetLength(Addr.Static.Text01);

        private DataCollection data = new DataCollection(), data_prev;

        public void Update(Racer r)
        {
            data_prev = (DataCollection)data.Clone();
            data.Update(r);
        }

        public Id State(Racer r)
        {
            if (data.GetValue(r, Addr.Static.InRace) == 1)
                return Id.InRace;

            if (data.GetValue(r, Addr.Static.SceneId) == 60)
                return Id.VehicleSelect;

            if ((data.GetValue(r, Addr.Static.SceneId) == 260) ||
                (data.GetValue(r, Path.Static, text_base + text_len * 2, Core.DataType.String, text_len).Substring(5, 6) == "Mirror") ||
                (data.GetValue(r, Path.Static, text_base + text_len * 3, Core.DataType.String, text_len).Substring(5, 10) == "Start Race") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(4, 7) == "Results"))
                return Id.TrackSelect;

            if (data.GetValue(r, (Addr.Static)text_base).Substring(5, 24) == "Single Player Tournament")
                return Id.Title;

            if (data.GetValue(r, (Addr.Static)text_base).Substring(3, 14) == "Current Player")
                return Id.FileSelect;

            if ((data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "VIDEO SETTINGS" && data.GetValue(r, (Addr.Static)(text_base + text_len)).Substring(5, 14) == "AUDIO SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "VIDEO SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "AUDIO SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 17) == "JOYSTICK SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "MOUSE SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base + text_len, Core.DataType.String, text_len).Substring(5, 17) == "KEYBOARD SETTINGS" && data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 22) == "Show Reserved Settings") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 13) == "RESERVED KEYS") ||
                (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 23) == "FORCE FEEDBACK SETTINGS") ||
                (data.GetValue(r, Path.Static, text_base + text_len, Core.DataType.String, text_len).Substring(5, 18) == "LOAD/SAVE SETTINGS" && data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 6) == "Cancel"))
                return Id.Settings;

            return Id.Unknown;
        }

        public Id DeepState(Racer r)
        {
            // in-race
            if (data.GetValue(r, Addr.Static.InRace) == 1)
            {
                if (data.GetValue(r, Addr.Static.PauseState) > 0)
                    return Id.RacePaused;
                if ((data.GetValue(r, Addr.Pod.Flags) & (1 << 0)) == 0 && (data.GetValue(r, Addr.Pod.Flags) & (1 << 1)) == 0)
                    return Id.RaceStarting;
                if ((data.GetValue(r, Addr.Pod.Flags) & (1 << 0)) != 0 && (data.GetValue(r, Addr.Pod.Flags) & (1 << 1)) != 0)
                    return Id.RaceEnded;
                return Id.InRace;
            }

            // vehicle selection
            if (data.GetValue(r, Addr.Static.SceneId) == 60)
            {
                if (data.GetValue(r, Path.Static, text_base + text_len * 2, Core.DataType.String, text_len).Substring(5, 18) != "Vehicle Statistics")
                    return Id.VehicleSelectAnim;
                return Id.VehicleSelect;
            }

            // track selection
            if (data.GetValue(r, Addr.Static.SceneId) == 260)
            {
                if (data.GetValue(r, Path.Static, text_base + text_len * 2, Core.DataType.String, text_len).Substring(5, 6) == "Mirror")
                    return Id.TrackSettings;
                if (data.GetValue(r, Path.Static, text_base + text_len * 3, Core.DataType.String, text_len).Substring(5, 10) == "Start Race")
                    return Id.TrackReady;
                return Id.TrackSelect;
            }
            if (data.GetValue(r, Path.Static, text_base + text_len * 2, Core.DataType.String, text_len).Substring(5, 6) == "Mirror")
                return Id.TrackSettings;
            if (data.GetValue(r, Path.Static, text_base + text_len * 3, Core.DataType.String, text_len).Substring(5, 10) == "Start Race")
                return Id.TrackReady;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(4, 7) == "Results")
                return Id.TrackResults;

            // main menu
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 24) == "Single Player Tournament")
                return Id.Title;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(3, 14) == "Current Player")
                return Id.FileSelect;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "VIDEO SETTINGS" && data.GetValue(r, (Addr.Static)(text_base + text_len)).Substring(5, 14) == "AUDIO SETTINGS")
                return Id.Settings;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "VIDEO SETTINGS")
                return Id.SettingsVideo;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "AUDIO SETTINGS")
                return Id.SettingsAudio;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 17) == "JOYSTICK SETTINGS")
                return Id.SettingsJoystick;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 14) == "MOUSE SETTINGS")
                return Id.SettingsMouse;
            if (data.GetValue(r, Path.Static, text_base + text_len, Core.DataType.String, text_len).Substring(5, 17) == "KEYBOARD SETTINGS" && data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 22) == "Show Reserved Settings")
                return Id.SettingsKeyboard;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 13) == "RESERVED KEYS")
                return Id.SettingsKeyboardReservedKeys;
            if (data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 23) == "FORCE FEEDBACK SETTINGS")
                return Id.SettingsForceFeedback;
            if (data.GetValue(r, Path.Static, text_base + text_len, Core.DataType.String, text_len).Substring(5, 18) == "LOAD/SAVE SETTINGS" && data.GetValue(r, Path.Static, text_base, Core.DataType.String, text_len).Substring(5, 6) == "Cancel")
                return Id.SettingsLoadSave;

            // default
            return Id.Unknown;
        }

        public bool EnterOrLeaveRace(Racer r)
        {
            int i = data_prev.ValueExists(Path.Static, (uint)Addr.Static.InRace, Addr.GetLength(Addr.Static.InRace));
            bool prev = (i < 0) ? false : data_prev.GetValue(i) > 0;
            bool now = data.GetValue(r, Addr.Static.InRace) > 0;
            return now ^ prev;
        }

        public enum Id
        {
            Unknown,
            Title,
            FileSelect,
            VehicleSelect,
            VehicleSelectAnimFile,
            VehicleSelectAnimTrack,
            VehicleSelectAnim,
            TrackSelect,
            TrackSelectAmateur,
            TrackSelectSemipro,
            TrackSelectGalactic,
            TrackSelectInvitational,
            TrackSettings,
            TrackReady,
            TrackResults,
            InRace,
            RaceStarting,
            RacePaused,
            RaceEnded,
            Settings,
            SettingsVideo,
            SettingsAudio,
            SettingsJoystick,
            SettingsMouse,
            SettingsKeyboard,
            SettingsKeyboardReservedKeys,
            SettingsForceFeedback,
            SettingsLoadSave,
            Loading
        }
    }
}