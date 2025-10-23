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

            _newGame.Pressed += OnNewGame;
            _continue.Pressed += OnContinue;
            _settings.Pressed += OnSettings;
            _quit.Pressed += OnQuit;

            UpdateContinueEnabled();
        }

        private void UpdateContinueEnabled()
        {
            bool hasSave = FileAccess.FileExists("user://save.json");
            if (_continue != null)
            {
                _continue.Disabled = !hasSave;
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
            if (saver != null)
            {
                saver.LoadGame();
            }
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
    }
}

