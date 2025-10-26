using Godot;

namespace SilentTestimony.World
{
    /// <summary>
    /// 使用 TileMapLayer + TileSet 的最小关卡构建器（含字符映射）。
    /// - 生成 Ground/Decor/Collision 三个 TileMapLayer
    /// - 从 TileSet 取第一个 Atlas 源，默认使用 (0,0) 切片
    /// - 支持字符映射（示例："#:G:0,0:full"、"=:G:0,0:half"、"*:D:0,0:none"）
    /// - 用 Rows 按字符在对应图层铺设瓦片；可选择生成占位碰撞（full/half/none）
    /// </summary>
    public partial class RuntimeTilemapBuilderLayers : Node2D
    {
        [Export(PropertyHint.Range, "4,128,1")] public int CellSize { get; set; } = 16;
        [Export] public TileSet TileSet { get; set; }
        [Export] public bool GenerateRuntimeBodies { get; set; } = true;
        [Export] public Godot.Collections.Array<string> Rows { get; set; } = new()
        {
            "................................................",
            "................................................",
            "................................................",
            "################################................"
        };
        [Export] public Godot.Collections.Array<string> Mappings { get; set; } = new()
        {
            "#:G:0,0:full", // 实心砖：放 Ground，整格碰撞
            "=:G:0,0:half", // 半砖（下半格）
            "*:D:0,0:none"  // 装饰：放 Decor，无碰撞
        };

        private struct MapRule
        {
            public char Ch;
            public char LayerCode; // G=Ground, D=Decor, C=Collision
            public Vector2I Atlas;
            public string Collider; // full/half/none
        }

        public override void _Ready()
        {
            if (TileSet == null)
            {
                TileSet = GD.Load<TileSet>("res://Resources/Tiles/Minimal16.tres");
            }

            var ground = CreateLayer("Ground");
            var decor = CreateLayer("Decor");
            var collision = CreateLayer("Collision");
            AddChild(ground);
            AddChild(decor);
            AddChild(collision);

            Node2D bodiesRoot = null;
            if (GenerateRuntimeBodies)
            {
                bodiesRoot = new Node2D { Name = "CollisionBodies" };
                AddChild(bodiesRoot);
            }

            int sourceId = TileSet.GetSourceId(0);
            var rules = BuildRuleMap();
            for (int r = 0; r < Rows.Count; r++)
            {
                var line = Rows[r] ?? string.Empty;
                for (int c = 0; c < line.Length; c++)
                {
                    char ch = line[c];
                    if (!rules.TryGetValue(ch, out var rule)) continue;

                    var cell = new Vector2I(c, r);
                    var layer = SelectLayer(rule.LayerCode, ground, decor, collision);
                    layer?.SetCell(cell, sourceId, rule.Atlas, 0);

                    if (GenerateRuntimeBodies && bodiesRoot != null)
                    {
                        if (rule.Collider == "full")
                        {
                            var body = MakeRectBody(CellSize, CellSize);
                            body.Position = new Vector2(c * CellSize + CellSize * 0.5f, r * CellSize + CellSize * 0.5f);
                            bodiesRoot.AddChild(body);
                        }
                        else if (rule.Collider == "half")
                        {
                            var body = MakeRectBody(CellSize, CellSize * 0.5f);
                            // 下半格：中心向下偏移 1/4 格
                            body.Position = new Vector2(c * CellSize + CellSize * 0.5f, r * CellSize + CellSize * 0.75f);
                            bodiesRoot.AddChild(body);
                        }
                    }
                }
            }
        }

        private TileMapLayer CreateLayer(string name)
        {
            var layer = new TileMapLayer
            {
                Name = name,
                TileSet = TileSet,
                YSortEnabled = false
            };
            return layer;
        }

        private System.Collections.Generic.Dictionary<char, MapRule> BuildRuleMap()
        {
            var dict = new System.Collections.Generic.Dictionary<char, MapRule>();
            foreach (var entry in Mappings)
            {
                // 格式: "<char>:<LayerCode>:<atlasX>,<atlasY>:<collider>"
                // 示例: "#:G:0,0:full"
                if (string.IsNullOrEmpty(entry) || entry.Length < 3) continue;
                var parts = entry.Split(':');
                if (parts.Length < 4) continue;
                char ch = parts[0][0];
                char layer = string.IsNullOrEmpty(parts[1]) ? 'G' : parts[1][0];
                var xy = parts[2].Split(',');
                int ax = 0, ay = 0;
                if (xy.Length >= 2)
                {
                    int.TryParse(xy[0], out ax);
                    int.TryParse(xy[1], out ay);
                }
                string collider = parts[3];
                dict[ch] = new MapRule
                {
                    Ch = ch,
                    LayerCode = layer,
                    Atlas = new Vector2I(ax, ay),
                    Collider = collider
                };
            }
            return dict;
        }

        private TileMapLayer SelectLayer(char code, TileMapLayer ground, TileMapLayer decor, TileMapLayer collision)
        {
            switch (char.ToUpperInvariant(code))
            {
                case 'D': return decor;
                case 'C': return collision;
                default: return ground;
            }
        }

        private StaticBody2D MakeRectBody(float w, float h)
        {
            var body = new StaticBody2D();
            var shape = new CollisionShape2D();
            var rect = new RectangleShape2D { Size = new Vector2(w, h) };
            shape.Shape = rect;
            body.AddChild(shape);
            return body;
        }
    }
}
