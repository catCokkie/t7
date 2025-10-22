using Godot;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    public partial class SettingsMenu : Control
    {
        private HSlider _volume;
        private HSlider _music;
        private HSlider _sfx;
        private OptionButton _windowMode;
        private OptionButton _resolution;
        private CheckBox _borderless;
        private Button _close;
        private SettingsManager _settings;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _settings = GetNodeOrNull<SettingsManager>("/root/SettingsManager");

            _volume = GetNodeOrNull<HSlider>("Center/Panel/VBox/Volume/Slider");
            _windowMode = GetNodeOrNull<OptionButton>("Center/Panel/VBox/WindowMode/Option");
            _resolution = GetNodeOrNull<OptionButton>("Center/Panel/VBox/Resolution/Option");
            _borderless = GetNodeOrNull<CheckBox>("Center/Panel/VBox/Borderless/Check");
            _music = GetNodeOrNull<HSlider>("Center/Panel/VBox/Music/Slider");
            _sfx = GetNodeOrNull<HSlider>("Center/Panel/VBox/SFX/Slider");
            _close = GetNodeOrNull<Button>("Center/Panel/VBox/Buttons/CloseButton");

            BuildOptions();
            LoadValues();

            if (_volume != null) _volume.ValueChanged += OnVolumeChanged;
            if (_windowMode != null) _windowMode.ItemSelected += OnWindowModeSelected;
            if (_resolution != null) _resolution.ItemSelected += OnResolutionSelected;
            if (_borderless != null) _borderless.Toggled += OnBorderlessToggled;
            if (_music != null) _music.ValueChanged += OnMusicChanged;
            if (_sfx != null) _sfx.ValueChanged += OnSfxChanged;
            if (_close != null) _close.Pressed += () => { Visible = false; };

            Visible = false;
        }

        private void BuildOptions()
        {
            _windowMode?.Clear();
            _windowMode?.AddItem("Windowed", (int)DisplayServer.WindowMode.Windowed);
            _windowMode?.AddItem("Fullscreen", (int)DisplayServer.WindowMode.Fullscreen);

            _resolution?.Clear();
            _resolution?.AddItem("1280 x 720", 0);
            _resolution?.AddItem("1600 x 900", 1);
            _resolution?.AddItem("1920 x 1080", 2);
        }

        private void LoadValues()
        {
            if (_settings == null) return;
            if (_volume != null) _volume.Value = _settings.MasterVolumeDb;
            if (_music != null) _music.Value = _settings.MusicVolumeDb;
            if (_sfx != null) _sfx.Value = _settings.SfxVolumeDb;
            if (_windowMode != null)
            {
                int idx = _settings.WindowMode == DisplayServer.WindowMode.Fullscreen ? 1 : 0;
                _windowMode.Select(idx);
            }
            if (_resolution != null)
            {
                var r = _settings.Resolution;
                int sel = (r.X, r.Y) switch
                {
                    (1280, 720) => 0,
                    (1600, 900) => 1,
                    (1920, 1080) => 2,
                    _ => 0
                };
                _resolution.Select(sel);
            }
            if (_borderless != null) _borderless.ButtonPressed = _settings.Borderless;
        }

        private void OnVolumeChanged(double value)
        {
            if (_settings == null) return;
            _settings.SetMasterVolumeDb((float)value);
            _settings.SaveConfig();
        }

        private void OnWindowModeSelected(long index)
        {
            if (_settings == null) return;
            var mode = index == 1 ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed;
            _settings.SetWindowMode(mode);
            _settings.SaveConfig();
        }

        private void OnResolutionSelected(long index)
        {
            if (_settings == null) return;
            Vector2I size = index switch
            {
                1 => new Vector2I(1600, 900),
                2 => new Vector2I(1920, 1080),
                _ => new Vector2I(1280, 720)
            };
            _settings.SetResolution(size);
            _settings.SaveConfig();
        }

        private void OnBorderlessToggled(bool on)
        {
            _settings?.SetBorderless(on);
            _settings?.SaveConfig();
        }

        private void OnMusicChanged(double value)
        {
            _settings?.SetMusicVolumeDb((float)value);
            _settings?.SaveConfig();
        }

        private void OnSfxChanged(double value)
        {
            _settings?.SetSfxVolumeDb((float)value);
            _settings?.SaveConfig();
        }
    }
}
