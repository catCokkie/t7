using Godot;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    public partial class PauseMenu : Control
    {
        private Button _resume;
        private Button _settings;
        private Button _save;
        private Button _quit;
        private Button _returnToMain;
        private ConfirmationDialog _confirm;
        private Button _load;
        private Button _controls;
        private SettingsMenu _settingsMenu;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always; // 在暂停中依然响应
            _resume = GetNodeOrNull<Button>("Center/Panel/VBox/ResumeButton");
            _settings = GetNodeOrNull<Button>("Center/Panel/VBox/SettingsButton");
            _save = GetNodeOrNull<Button>("Center/Panel/VBox/SaveButton");
            _load = GetNodeOrNull<Button>("Center/Panel/VBox/LoadButton");
            _controls = GetNodeOrNull<Button>("Center/Panel/VBox/ControlsButton");
            _quit = GetNodeOrNull<Button>("Center/Panel/VBox/QuitButton");
            _returnToMain = GetNodeOrNull<Button>("Center/Panel/VBox/ReturnToMainButton");
            _confirm = GetNodeOrNull<ConfirmationDialog>("ConfirmDialog");

            _resume.Pressed += OnResume;
            _settings.Pressed += OnSettings;
            _save.Pressed += OnSave;
            _quit.Pressed += OnQuit;
            if (_returnToMain != null) _returnToMain.Pressed += OnReturnToMain;
            if (_confirm != null) _confirm.Confirmed += OnConfirmReturnToMain;
            if (_load != null) _load.Pressed += OnLoad;
            if (_controls != null) _controls.Pressed += OnControls;

            Visible = false;

            UpdateButtons();

            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            if (saver != null)
            {
                saver.SaveCompleted += OnSaveOrLoadCompleted;
                saver.LoadCompleted += OnSaveOrLoadCompleted;
            }
        }

        private void OnResume()
        {
            GetTree().Paused = false;
            Visible = false;
            GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Release();
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
            GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Acquire();
        }

        private void OnSave()
        {
            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            saver?.SaveGame();
        }

        private void OnLoad()
        {
            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            saver?.LoadGame();
        }

        private void OnControls()
        {
            // 预留：未来可接入按键重绑定菜单
            GD.Print("[PauseMenu] Controls menu not implemented yet.");
        }

        private void OnQuit()
        {
            GetTree().Paused = false;
            GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Release();
            GetTree().Quit();
        }

        private void OnReturnToMain()
        {
            if (_confirm != null)
            {
                _confirm.DialogText = "返回主菜单将丢失未保存进度，确定吗？";
                _confirm.PopupCentered();
            }
            else
            {
                OnConfirmReturnToMain();
            }
        }

        private void OnConfirmReturnToMain()
        {
            GetTree().Paused = false;
            GetNodeOrNull<SilentTestimony.Systems.InputGuard>("/root/InputGuard")?.Release();
            var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
            if (loader != null)
                loader.ChangeScene("res://Scenes/MainMenu.tscn", null);
            else
                GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        }

        private void UpdateButtons()
        {
            if (_load != null)
            {
                _load.Disabled = !FileAccess.FileExists("user://save.json");
            }
        }

        private void OnSaveOrLoadCompleted()
        {
            UpdateButtons();
        }
    }
}
