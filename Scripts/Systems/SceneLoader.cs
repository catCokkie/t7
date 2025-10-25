using Godot;
using SilentTestimony.UI;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 简单的场景加载与出生点管理（Autoload）
    /// </summary>
    public partial class SceneLoader : Node
    {
        private string _pendingSpawnName;

        public async void ChangeScene(string scenePath, string spawnPointName = null)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                GD.PushWarning("SceneLoader.ChangeScene: scenePath 为空");
                return;
            }

            _pendingSpawnName = spawnPointName;

            var fader = GetNodeOrNull<PostProcessController>("/root/PostProcessController");
            if (fader != null)
            {
                await fader.FadeOut(0.3f);
            }

            var err = GetTree().ChangeSceneToFile(scenePath);
            if (err != Error.Ok)
            {
                GD.PushError($"SceneLoader: 切换场景失败: {scenePath}, 错误: {err}");
                return;
            }

            // 等待一帧，确保新场景树稳定
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            ApplySpawnPoint();

            if (fader != null)
            {
                await fader.FadeIn(0.3f);
            }

            // Autosave upon entering a new scene, unless we're currently loading a save
            var saver = GetNodeOrNull<SaveManager>("/root/SaveManager");
            if (saver != null && !saver.IsLoading)
            {
                saver.SaveGame();
            }
        }

        private void ApplySpawnPoint()
        {
            if (string.IsNullOrEmpty(_pendingSpawnName))
                return;

            var current = GetTree().CurrentScene;
            if (current == null)
                return;

            // 查找出生点节点（按名称）
            Node2D spawn = FindNodeByName<Node2D>(current, _pendingSpawnName);

            // 查找玩家（按组）并设置位置
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && spawn != null)
            {
                if (players[0] is Node2D playerNode)
                {
                    playerNode.GlobalPosition = spawn.GlobalPosition;
                }
            }

            _pendingSpawnName = null;
        }

        private T FindNodeByName<T>(Node root, string name) where T : class
        {
            if (root == null) return null;
            if (root.Name == name && root is T tRoot)
                return tRoot;

            foreach (Node child in root.GetChildren())
            {
                var found = FindNodeByName<T>(child, name);
                if (found != null) return found;
            }
            return null;
        }

        public override void _Process(double delta)
        {
            // no-op: ensure Node processes if needed in the future
        }
    }
}

