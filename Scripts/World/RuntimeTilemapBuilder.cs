using Godot;

namespace SilentTestimony.World
{
    /// <summary>
    /// 基于 TileMap/TileSet 的最小运行时构建器：
    /// - 生成 Ground/Collision/Decor 三层 TileMap
    /// - 动态创建一个 16×16 的最小 TileSet（使用一张纹理的 0,0 切片）
    /// - 用 Rows（字符地图）在 Ground 层布置 '#' 瓦片
    /// - 为每个 '#' 同步生成一个矩形 StaticBody2D 作为占位碰撞
    ///   （后续可在 TileSet 内设置碰撞后移除此步骤）
    /// </summary>
    public partial class RuntimeTilemapBuilder : Node2D
    {
        [Export(PropertyHint.Range, "4,128,1")] public int CellSize { get; set; } = 16;
        [Export] public Texture2D TileTexture { get; set; }
        [Export] public Godot.Collections.Array<string> Rows { get; set; } = new()
        {
            "................................................",
            "................................................",
            "................................................",
            "################################................"
        };

        private int _sourceId;

        public override void _Ready()
        {
            if (TileTexture == null)
            {
                TileTexture = GD.Load<Texture2D>("res://icon.svg");
            }

            var tileset = BuildMinimalTileSet(TileTexture, CellSize);
            var ground = MakeTileMap("Ground", tileset);
            var decor = MakeTileMap("Decor", tileset);
            var collision = MakeTileMap("Collision", tileset);

            AddChild(ground);
            AddChild(decor);
            AddChild(collision);

            var bodiesRoot = new Node2D { Name = "CollisionBodies" };
            AddChild(bodiesRoot);

            for (int r = 0; r < Rows.Count; r++)
            {
                var line = Rows[r] ?? string.Empty;
                for (int c = 0; c < line.Length; c++)
                {
                    if (line[c] == '#')
                    {
                        var cell = new Vector2I(c, r);
                        ground.SetCell(0, cell, _sourceId, new Vector2I(0, 0), 0);

                        // 占位碰撞：与 TileMap 单元对齐的静态矩形
                        var body = new StaticBody2D();
                        var shape = new CollisionShape2D();
                        var rect = new RectangleShape2D { Size = new Vector2(CellSize, CellSize) };
                        shape.Shape = rect;
                        body.Position = new Vector2(c * CellSize + CellSize * 0.5f, r * CellSize + CellSize * 0.5f);
                        body.AddChild(shape);
                        bodiesRoot.AddChild(body);
                    }
                }
            }
        }

        private TileMap MakeTileMap(string name, TileSet tileset)
        {
            var tm = new TileMap
            {
                Name = name,
                TileSet = tileset,
                CellQuadrantSize = 16,
                RenderingQuadrantSize = 16
            };
            tm.TileSet = tileset;
            tm.CellQuadrantSize = 16;
            tm.RenderingQuadrantSize = 16;
            tm.SetLayerEnabled(0, true);
            tm.SetLayerModulate(0, Colors.White);
            tm.SetLayerYSortEnabled(0, false);
            tm.SetLayerZIndex(0, 0);
            tm.SetLayerName(0, name);
            if (tm.TileSet != null)
                tm.TileSet.TileSize = new Vector2I(CellSize, CellSize);
            return tm;
        }

        private TileSet BuildMinimalTileSet(Texture2D tex, int size)
        {
            var ts = new TileSet();
            var atlas = new TileSetAtlasSource
            {
                Texture = tex,
                TextureRegionSize = new Vector2I(size, size)
            };
            _sourceId = ts.AddSource(atlas);
            // 使用 (0,0) 切片作为唯一瓦片，后续可扩展更多区域
            return ts;
        }
    }
}
