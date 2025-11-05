using Godot;

namespace SilentTestimony.World
{
    /// <summary>
    /// 全局网格/瓦片辅助方法与统一格子尺寸。
    /// 默认格子为 16×16，可按需扩展为从配置读取。
    /// </summary>
    public static class GridUtility
    {
        public const int CellSize = 16;

        public static readonly Vector2 DefaultCenterOffset = new Vector2(CellSize * 0.5f, CellSize * 0.5f);

        public static Vector2 TileToWorldPosition(Vector2I cell, float cellSize, Vector2 offset)
        {
            return new Vector2(cell.X * cellSize, cell.Y * cellSize) + offset;
        }

        public static Vector2 TileToWorldPosition(Vector2I cell, Vector2 offset)
        {
            return TileToWorldPosition(cell, CellSize, offset);
        }

        public static Vector2 TileToWorldCenter(Vector2I cell, float cellSize)
        {
            return new Vector2(cell.X * cellSize + cellSize * 0.5f, cell.Y * cellSize + cellSize * 0.5f);
        }

        public static Vector2 TileToWorldCenter(Vector2I cell)
        {
            return TileToWorldCenter(cell, CellSize);
        }

        public static Vector2I WorldToTile(Vector2 world, float cellSize)
        {
            return new Vector2I(Mathf.FloorToInt(world.X / cellSize), Mathf.FloorToInt(world.Y / cellSize));
        }

        public static Vector2I WorldToTile(Vector2 world)
        {
            return WorldToTile(world, CellSize);
        }
    }
}

