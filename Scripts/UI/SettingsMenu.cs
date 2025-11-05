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
        private OptionButton _language;
        private CheckBox _borderless;
        private Button _close;
        private SettingsManager _settings;
        private Label _title;
        private Label _lblVolume;
        private Label _lblWindowMode;
        private Label _lblBorderless;
        private Label _lblMusic;
        private Label _lblSfx;
        private Label _lblResolution;
        private Label _lblLanguage;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _settings = GetNodeOrNull<SettingsManager>("/root/SettingsManager");

            _volume = GetNodeOrNull<HSlider>("Center/Panel/VBox/Volume/Slider");
            _windowMode = GetNodeOrNull<OptionButton>("Center/Panel/VBox/WindowMode/Option");
            _resolution = GetNodeOrNull<OptionButton>("Center/Panel/VBox/Resolution/Option");
            _language = GetNodeOrNull<OptionButton>("Center/Panel/VBox/Language/Option");
            _borderless = GetNodeOrNull<CheckBox>("Center/Panel/VBox/Borderless/Check");
            _music = GetNodeOrNull<HSlider>("Center/Panel/VBox/Music/Slider");
            _sfx = GetNodeOrNull<HSlider>("Center/Panel/VBox/SFX/Slider");
            _close = GetNodeOrNull<Button>("Center/Panel/VBox/Buttons/CloseButton");
            _title = GetNodeOrNull<Label>("Center/Panel/VBox/Title");
            _lblVolume = GetNodeOrNull<Label>("Center/Panel/VBox/Volume/Label");
            _lblWindowMode = GetNodeOrNull<Label>("Center/Panel/VBox/WindowMode/Label");
            _lblBorderless = GetNodeOrNull<Label>("Center/Panel/VBox/Borderless/Label");
            _lblMusic = GetNodeOrNull<Label>("Center/Panel/VBox/Music/Label");
            _lblSfx = GetNodeOrNull<Label>("Center/Panel/VBox/SFX/Label");
            _lblResolution = GetNodeOrNull<Label>("Center/Panel/VBox/Resolution/Label");
            _lblLanguage = GetNodeOrNull<Label>("Center/Panel/VBox/Language/Label");

            ApplyLocalization();
            BuildOptions();
            LoadValues();

            if (_volume != null) _volume.ValueChanged += OnVolumeChanged;
            if (_windowMode != null) _windowMode.ItemSelected += OnWindowModeSelected;
            if (_resolution != null) _resolution.ItemSelected += OnResolutionSelected;
            if (_borderless != null) _borderless.Toggled += OnBorderlessToggled;
            if (_music != null) _music.ValueChanged += OnMusicChanged;
            if (_sfx != null) _sfx.ValueChanged += OnSfxChanged;
            if (_language != null) _language.ItemSelected += OnLanguageSelected;
            if (_close != null) _close.Pressed += () => { Visible = false; };

            Visible = false;
        }

        private void BuildOptions()
        {
            _windowMode?.Clear();
            _windowMode?.AddItem(TranslationServer.Translate("ui.settings.windowed"), (int)DisplayServer.WindowMode.Windowed);
            _windowMode?.AddItem(TranslationServer.Translate("ui.settings.fullscreen"), (int)DisplayServer.WindowMode.Fullscreen);

            _resolution?.Clear();
            _resolution?.AddItem("1280 x 720", 0);
            _resolution?.AddItem("1600 x 900", 1);
            _resolution?.AddItem("1920 x 1080", 2);

            if (_language != null)
            {
                _language.Clear();
                _language.AddItem(TranslationServer.Translate("ui.lang.zh"), 0);
                _language.AddItem(TranslationServer.Translate("ui.lang.en"), 1);
            }
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
            if (_language != null)
            {
                string loc = _settings.Locale ?? TranslationServer.GetLocale();
                int idx = 1; // default en
                if (!string.IsNullOrEmpty(loc))
                {
                    string l = loc.ToLowerInvariant();
                    if (l.StartsWith("zh")) idx = 0;
                }
                _language.Select(idx);
            }
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

        private void OnLanguageSelected(long index)
        {
            if (_settings == null || _language == null) return;
            string locale = index == 0 ? "zh" : "en";
            _settings.SetLocale(locale);
            _settings.SaveConfig();
            ApplyLocalization();
            BuildOptions();
            LoadValues();
        }

        private void ApplyLocalization()
        {
            if (_title != null) _title.Text = TranslationServer.Translate("ui.settings.title");
            if (_lblVolume != null) _lblVolume.Text = TranslationServer.Translate("ui.settings.master_db");
            if (_lblWindowMode != null) _lblWindowMode.Text = TranslationServer.Translate("ui.settings.window_mode");
            if (_lblBorderless != null) _lblBorderless.Text = TranslationServer.Translate("ui.settings.borderless");
            if (_lblMusic != null) _lblMusic.Text = TranslationServer.Translate("ui.settings.music_db");
            if (_lblSfx != null) _lblSfx.Text = TranslationServer.Translate("ui.settings.sfx_db");
            if (_lblResolution != null) _lblResolution.Text = TranslationServer.Translate("ui.settings.resolution");
            if (_lblLanguage != null) _lblLanguage.Text = TranslationServer.Translate("ui.settings.language");
            if (_close != null)
            {
                var t = TranslationServer.Translate("ui.close");
                if (!string.IsNullOrEmpty(t)) _close.Text = t;
            }
        }
    }
}
