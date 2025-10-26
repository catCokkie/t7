using Godot;

namespace SilentTestimony.World
{
    /// <summary>
    /// 用字符地图在运行时生成基于像素格子的关卡（占位实现）。
    /// 使用 16×16 等尺寸的砖块场景来拼装碰撞/可视地面。
    /// </summary>
    public partial class GridLevelBuilder : Node2D
    {
        [Export(PropertyHint.Range, "4,128,1")] public int CellSize { get; set; } = 16;
        [Export] public string TileScenePath { get; set; } = "res://Scenes/Tiles/Solid16.tscn";
        [Export] public Godot.Collections.Array<string> Rows { get; set; } = new()
        {
            "................................................",
            "................................................",
            "................................................",
            "################################................"
        };

        public override void _Ready()
        {
            var packed = GD.Load<PackedScene>(TileScenePath);
            if (packed == null)
            {
                GD.PushError($"GridLevelBuilder: 无法加载砖块场景 {TileScenePath}");
                return;
            }

            for (int r = 0; r < Rows.Count; r++)
            {
                var line = Rows[r] ?? string.Empty;
                for (int c = 0; c < line.Length; c++)
                {
                    char ch = line[c];
                    if (ch == '#' )
                    {
                        var tile = packed.Instantiate<Node2D>();
                        AddChild(tile);
                        tile.Position = new Vector2(c * CellSize + CellSize * 0.5f, r * CellSize + CellSize * 0.5f);
                    }
                }
            }
        }
    }
}

