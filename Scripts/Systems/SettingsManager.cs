using Godot;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 管理并持久化设置（音量 / 窗口模式 / 分辨率）
    /// </summary>
    public partial class SettingsManager : Node
    {
        private const string ConfigPath = "user://config.json";

        public float MasterVolumeDb { get; private set; } = 0.0f; // 0 dB
        public float MusicVolumeDb { get; private set; } = 0.0f;
        public float SfxVolumeDb { get; private set; } = 0.0f;
        public DisplayServer.WindowMode WindowMode { get; private set; } = DisplayServer.WindowMode.Windowed;
        public Vector2I Resolution { get; private set; } = new Vector2I(1280, 720);
        public bool Borderless { get; private set; } = false;

        public override void _Ready()
        {
            LoadConfig();
            ApplySettings();
        }

        public void SetMasterVolumeDb(float db)
        {
            MasterVolumeDb = db;
            ApplyAudio();
        }

        public void SetMusicVolumeDb(float db)
        {
            MusicVolumeDb = db;
            ApplyAudio();
        }

        public void SetSfxVolumeDb(float db)
        {
            SfxVolumeDb = db;
            ApplyAudio();
        }

        public void SetWindowMode(DisplayServer.WindowMode mode)
        {
            WindowMode = mode;
            ApplyWindow();
        }

        public void SetResolution(Vector2I size)
        {
            Resolution = size;
            ApplyWindow();
        }

        public void SetBorderless(bool on)
        {
            Borderless = on;
            ApplyWindow();
        }

        public void ApplySettings()
        {
            ApplyAudio();
            ApplyWindow();
        }

        private void ApplyAudio()
        {
            int master = AudioServer.GetBusIndex("Master");
            if (master >= 0)
            {
                AudioServer.SetBusVolumeDb(master, MasterVolumeDb);
            }

            int music = AudioServer.GetBusIndex("BGM");
            if (music >= 0)
            {
                AudioServer.SetBusVolumeDb(music, MusicVolumeDb);
            }

            int sfx = AudioServer.GetBusIndex("SFX");
            if (sfx >= 0)
            {
                AudioServer.SetBusVolumeDb(sfx, SfxVolumeDb);
            }
        }

        private void ApplyWindow()
        {
            DisplayServer.WindowSetMode(WindowMode, 0);
            if (WindowMode == DisplayServer.WindowMode.Windowed)
            {
                DisplayServer.WindowSetSize(Resolution, 0);
            }
            DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, Borderless, 0);
        }
        
        public void SaveConfig()
        {
            var dict = new Godot.Collections.Dictionary<string, Variant>
            {
                { "master_db", MasterVolumeDb },
                { "music_db", MusicVolumeDb },
                { "sfx_db", SfxVolumeDb },
                { "window_mode", (int)WindowMode },
                { "res_w", Resolution.X },
                { "res_h", Resolution.Y },
                { "borderless", Borderless }
            };

            using var fa = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Write);
            if (fa != null)
            {
                fa.StoreString(Json.Stringify(dict));
            }
        }

        public void LoadConfig()
        {
            if (!FileAccess.FileExists(ConfigPath))
                return;

            using var fa = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
            if (fa == null) return;
            var text = fa.GetAsText();
            var result = Json.ParseString(text);
            if (result.VariantType == Variant.Type.Dictionary)
            {
                var dict = result.AsGodotDictionary();
                if (dict.ContainsKey("master_db")) MasterVolumeDb = (float)dict["master_db"].AsDouble();
                if (dict.ContainsKey("music_db")) MusicVolumeDb = (float)dict["music_db"].AsDouble();
                if (dict.ContainsKey("sfx_db")) SfxVolumeDb = (float)dict["sfx_db"].AsDouble();
                if (dict.ContainsKey("window_mode")) WindowMode = (DisplayServer.WindowMode)(int)dict["window_mode"].AsInt32();
                int w = dict.ContainsKey("res_w") ? (int)dict["res_w"].AsInt32() : Resolution.X;
                int h = dict.ContainsKey("res_h") ? (int)dict["res_h"].AsInt32() : Resolution.Y;
                Resolution = new Vector2I(w, h);
                if (dict.ContainsKey("borderless")) Borderless = dict["borderless"].AsBool();
            }
        }
    }
}
