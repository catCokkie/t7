using Godot;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    public partial class MainMenu : Control
    {
        private Button _newGame;
        private Button _continue;
        private Button _settings;
        private Button _quit;
        private SettingsMenu _settingsMenu;

        public override void _Ready()
        {
            _newGame = GetNodeOrNull<Button>("Center/Panel/VBox/NewGameButton");
            _continue = GetNodeOrNull<Button>("Center/Panel/VBox/ContinueButton");
            _settings = GetNodeOrNull<Button>("Center/Panel/VBox/SettingsButton");
            _quit = GetNodeOrNull<Button>("Center/Panel/VBox/QuitButton");

            if (_newGame != null) _newGame.Pressed += OnNewGame;
            if (_continue != null) _continue.Pressed += OnContinue;
            if (_settings != null) _settings.Pressed += OnSettings;
            if (_quit != null) _quit.Pressed += OnQuit;

            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            if (saver != null)
            {
                saver.SaveCompleted += OnSaveCompleted;
            }

            UpdateContinueEnabled();
        }

        private void UpdateContinueEnabled()
        {
            bool hasSave = FileAccess.FileExists("user://save.json");
            if (_continue == null)
                return;

            _continue.Disabled = !hasSave;
            if (!hasSave)
                return;

            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            var meta = saver?.GetSaveMeta();
            if (meta != null)
            {
                string shortInGame = $"{meta.DayStr} {meta.TimeStr}";
                _continue.Text = $"Continue - {shortInGame} ({meta.LocalTimestampStr})";
            }
        }

        private void OnNewGame()
        {
            var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
            if (loader != null)
                loader.ChangeScene("res://Scenes/TestLevel.tscn", null);
            else
                GetTree().ChangeSceneToFile("res://Scenes/TestLevel.tscn");
        }

        private void OnContinue()
        {
            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            saver?.LoadGame();
        }

        private void OnSettings()
        {
            if (_settingsMenu == null)
            {
                var packed = GD.Load<PackedScene>("res://Scenes/UI/SettingsMenu.tscn");
                _settingsMenu = packed.Instantiate<SettingsMenu>();
                AddChild(_settingsMenu);
            }
            _settingsMenu.Visible = true;
        }

        private void OnQuit()
        {
            GetTree().Quit();
        }

        private void OnSaveCompleted()
        {
            // Refresh Continue label when a save completes
            UpdateContinueEnabled();
        }
    }
}
