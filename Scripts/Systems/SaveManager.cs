using Godot;
using SilentTestimony.Core;
using SilentTestimony.Player;
using SilentTestimony.Systems;
using SilentTestimony.Data;
using System.Collections.Generic;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 简单存档系统（Autoload）
    /// 存储：当前场景、玩家位置、生命、理智、物品ID、证据ID
    /// </summary>
    public partial class SaveManager : Node
    {
        private const string SavePath = "user://save.json";

        [Signal] public delegate void SaveCompletedEventHandler();
        [Signal] public delegate void LoadCompletedEventHandler();

        public void SaveGame()
        {
            var tree = GetTree();
            var scene = tree.CurrentScene;
            string scenePath = scene?.SceneFilePath ?? "";

            Vector2 playerPos = Vector2.Zero;
            var players = tree.GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is Node2D p)
            {
                playerPos = p.GlobalPosition;
            }

            var stats = GetNodeOrNull<PlayerStats>("/root/PlayerStats");
            int health = stats?.Health ?? 100;
            int sanity = stats?.Sanity ?? 100;

            var inv = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
            var itemIds = new Godot.Collections.Array<string>();
            if (inv != null)
            {
                foreach (var item in inv.GetAllItems())
                {
                    itemIds.Add(item.ItemID);
                }
            }

            var evMgr = GetNodeOrNull<EvidenceManager>("/root/EvidenceManager");
            var evidenceIds = new Godot.Collections.Array<string>();
            if (evMgr != null)
            {
                foreach (var e in evMgr.GetAll())
                {
                    evidenceIds.Add(e.EvidenceID);
                }
            }

            var dict = new Godot.Collections.Dictionary<string, Variant>
            {
                {"scene", scenePath},
                {"player_x", playerPos.X},
                {"player_y", playerPos.Y},
                {"health", health},
                {"sanity", sanity},
                {"items", itemIds},
                {"evidence", evidenceIds}
            };

            using var fa = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (fa != null)
            {
                fa.StoreString(Json.Stringify(dict));
            }

            EmitSignal(SignalName.SaveCompleted);
        }

        public async void LoadGame()
        {
            if (!FileAccess.FileExists(SavePath))
            {
                GD.PushWarning("SaveManager: 尚无存档文件");
                return;
            }

            using var fa = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (fa == null) return;
            var text = fa.GetAsText();
            var parsed = Json.ParseString(text);
            if (parsed.VariantType != Variant.Type.Dictionary) return;
            var result = parsed.AsGodotDictionary();

            string scenePath = result.ContainsKey("scene") ? (string)result["scene"] : "";
            float px = result.ContainsKey("player_x") ? (float)result["player_x"].AsDouble() : 0f;
            float py = result.ContainsKey("player_y") ? (float)result["player_y"].AsDouble() : 0f;
            int health = result.ContainsKey("health") ? (int)result["health"].AsInt32() : 100;
            int sanity = result.ContainsKey("sanity") ? (int)result["sanity"].AsInt32() : 100;
            var items = result.ContainsKey("items") ? result["items"].AsGodotArray() : new Godot.Collections.Array();
            var evidence = result.ContainsKey("evidence") ? result["evidence"].AsGodotArray() : new Godot.Collections.Array();

            // 切换场景
            if (!string.IsNullOrEmpty(scenePath))
            {
                var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
                if (loader != null)
                {
                    loader.ChangeScene(scenePath, null);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }

            // 应用位置
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is Node2D p)
            {
                p.GlobalPosition = new Vector2(px, py);
            }

            // 应用生命理智
            var stats = GetNodeOrNull<PlayerStats>("/root/PlayerStats");
            if (stats != null)
            {
                // 直接设置内部字段不可行，走差值
                stats.ChangeHealth(health - stats.Health);
                stats.ChangeSanity(sanity - stats.Sanity);
            }

            // 重建背包（通过扫描 Resources/Items 根据 ItemID 匹配）
            var inv = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
            if (inv != null)
            {
                // 简单策略：不清空已有，只补齐缺失
                foreach (var idVar in items)
                {
                    string id = (string)idVar;
                    if (!inv.HasItem(id))
                    {
                        var res = LoadInventoryItemById(id);
                        if (res != null) inv.AddItem(res);
                    }
                }
            }

            // 重建证据（扫描 Resources/Evidence）
            var evMgr = GetNodeOrNull<EvidenceManager>("/root/EvidenceManager");
            if (evMgr != null)
            {
                foreach (var idVar in evidence)
                {
                    string id = (string)idVar;
                    if (!evMgr.HasEvidence(id))
                    {
                        var res = LoadEvidenceById(id);
                        if (res != null) evMgr.AddEvidence(res);
                    }
                }
            }

            EmitSignal(SignalName.LoadCompleted);
        }

        private InventoryItemData LoadInventoryItemById(string id)
        {
            var dir = DirAccess.Open("res://Resources/Items");
            if (dir == null) return null;
            dir.ListDirBegin();
            for (var file = dir.GetNext(); !string.IsNullOrEmpty(file); file = dir.GetNext())
            {
                if (dir.CurrentIsDir()) continue;
                if (!file.EndsWith(".tres") && !file.EndsWith(".res")) continue;
                var path = $"res://Resources/Items/{file}";
                var res = GD.Load<InventoryItemData>(path);
                if (res != null && res.ItemID == id)
                {
                    dir.ListDirEnd();
                    return res;
                }
            }
            dir.ListDirEnd();
            return null;
        }

        private EvidenceData LoadEvidenceById(string id)
        {
            var dir = DirAccess.Open("res://Resources/Evidence");
            if (dir == null) return null;
            dir.ListDirBegin();
            for (var file = dir.GetNext(); !string.IsNullOrEmpty(file); file = dir.GetNext())
            {
                if (dir.CurrentIsDir()) continue;
                if (!file.EndsWith(".tres") && !file.EndsWith(".res")) continue;
                var path = $"res://Resources/Evidence/{file}";
                var res = GD.Load<EvidenceData>(path);
                if (res != null && res.EvidenceID == id)
                {
                    dir.ListDirEnd();
                    return res;
                }
            }
            dir.ListDirEnd();
            return null;
        }
    }
}
