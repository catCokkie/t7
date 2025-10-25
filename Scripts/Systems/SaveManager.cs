using Godot;
using SilentTestimony.Core;
using SilentTestimony.Player;
using SilentTestimony.Systems;
using SilentTestimony.Data;
using System;
using System.Collections.Generic;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 绠€鍗曞瓨妗ｇ郴缁燂紙Autoload锛?
    /// 瀛樺偍锛氬綋鍓嶅満鏅€佺帺瀹朵綅缃€佺敓鍛姐€佺悊鏅恒€佺墿鍝両D銆佽瘉鎹甀D
    /// </summary>
    public partial class SaveManager : Node
    {
        private const string SavePath = "user://save.json";

        [Signal] public delegate void SaveCompletedEventHandler();
        [Signal] public delegate void LoadCompletedEventHandler();

        public bool IsLoading { get; private set; } = false;

        public class SaveMeta
        {
            public string ScenePath { get; set; }
            public int Day { get; set; }
            public float Hour { get; set; }
            public long UnixTime { get; set; }
            public string DayStr => $"Day {Day}";
            public string TimeStr
            {
                get
                {
                    int h = (int)Mathf.Floor(Hour);
                    int m = (int)Mathf.Floor((Hour * 60) % 60);
                    return $"{h:D2}:{m:D2}";
                }
            }
            public string LocalTimestampStr
            {
                get
                {
                    try { return DateTimeOffset.FromUnixTimeSeconds(UnixTime).LocalDateTime.ToString("yyyy-MM-dd HH:mm"); }
                    catch { return ""; }
                }
            }
        }

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

            // Metadata: in-game day/hour and real-world timestamp
            var time = GetNodeOrNull<TimeManager>("/root/TimeManager");
            int day = time?.CurrentDay ?? 1;
            float hour = time?.CurrentTimeInHours ?? 6.0f;
            long unix = DateTimeOffset.Now.ToUnixTimeSeconds();

            var dict = new Godot.Collections.Dictionary<string, Variant>
            {
                {"scene", scenePath},
                {"player_x", playerPos.X},
                {"player_y", playerPos.Y},
                {"health", health},
                {"sanity", sanity},
                {"items", itemIds},
                {"evidence", evidenceIds},
                {"day", day},
                {"hour", hour},
                {"save_time_unix", unix}
            };

            using var fa = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            if (fa != null)
            {
                fa.StoreString(Json.Stringify(dict));
            }

            EmitSignal(SignalName.SaveCompleted);
        }

        public SaveMeta GetSaveMeta()
        {
            if (!FileAccess.FileExists(SavePath)) return null;
            using var fa = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            if (fa == null) return null;
            var text = fa.GetAsText();
            var parsed = Json.ParseString(text);
            if (parsed.VariantType != Variant.Type.Dictionary) return null;
            var result = parsed.AsGodotDictionary();

            var meta = new SaveMeta();
            meta.ScenePath = result.ContainsKey("scene") ? (string)result["scene"] : "";
            meta.Day = result.ContainsKey("day") ? (int)result["day"].AsInt32() : 1;
            // hour may be stored as double; fallback to building from time_str if present in future
            meta.Hour = result.ContainsKey("hour") ? (float)result["hour"].AsDouble() : 6.0f;
            if (result.ContainsKey("save_time_unix"))
            {
                var v = result["save_time_unix"];
                long unixParsed = 0L;
                // 閬垮厤渚濊禆鐗瑰畾 AsInt64 API锛氫娇鐢?ToString 灏濊瘯瑙ｆ瀽锛屽け璐ュ垯閫€鍥?0
                if (!long.TryParse(v.ToString(), out unixParsed))
                {
                    try { unixParsed = (long)v.AsDouble(); }
                    catch { unixParsed = 0L; }
                }
                meta.UnixTime = unixParsed;
            }
            else
            {
                meta.UnixTime = 0L;
            }
            return meta;
        }

        public async void LoadGame()
        {
            IsLoading = true;
            try
            {
            if (!FileAccess.FileExists(SavePath))
            {
                GD.PushWarning("SaveManager: 灏氭棤瀛樻。鏂囦欢");
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
            int day = result.ContainsKey("day") ? (int)result["day"].AsInt32() : 1;
            float hour = result.ContainsKey("hour") ? (float)result["hour"].AsDouble() : 6.0f;

            // 鍒囨崲鍦烘櫙
            if (!string.IsNullOrEmpty(scenePath))
            {
                var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
                if (loader != null)
                {
                    loader.ChangeScene(scenePath, null);
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }

            // 鎭㈠鏃堕棿锛堝湪鍒囨崲鍒扮洰鏍囧満鏅悗璁剧疆锛屼究浜庣浉鍏崇郴缁熸敹鍒颁俊鍙凤級
            var timeMgr = GetNodeOrNull<TimeManager>("/root/TimeManager");
            timeMgr?.SetClock(day, hour, true);

            // 搴旂敤浣嶇疆
            var players = GetTree().GetNodesInGroup("Player");
            if (players.Count > 0 && players[0] is Node2D p)
            {
                p.GlobalPosition = new Vector2(px, py);
            }

            // 搴旂敤鐢熷懡鐞嗘櫤
            var stats = GetNodeOrNull<PlayerStats>("/root/PlayerStats");
            if (stats != null)
            {
                // 鐩存帴璁剧疆鍐呴儴瀛楁涓嶅彲琛岋紝璧板樊鍊?
                stats.ChangeHealth(health - stats.Health);
                stats.ChangeSanity(sanity - stats.Sanity);
            }

            // 閲嶅缓鑳屽寘锛堥€氳繃鎵弿 Resources/Items 鏍规嵁 ItemID 鍖归厤锛?
            var inv = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
            if (inv != null)
            {
                // 绠€鍗曠瓥鐣ワ細涓嶆竻绌哄凡鏈夛紝鍙ˉ榻愮己澶?
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

            // 閲嶅缓璇佹嵁锛堟壂鎻?Resources/Evidence锛?
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

            // 鍙€夛細鍦ㄥ畬鍏ㄦ仮澶嶅悗绔嬪嵆鍐欏叆涓€娆″瓨妗ｏ紝纭繚 Continue 鏂囨湰鍜屾椂闂村厓鏁版嵁鍗虫椂鏇存柊
            SaveGame();
            }
            finally
            {
                IsLoading = false;
            }
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


