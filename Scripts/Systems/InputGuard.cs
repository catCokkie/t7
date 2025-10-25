using Godot;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// Autoload: 全局输入屏蔽标志。UI 叠加层打开时设置 Blocked=true，
    /// 玩家控制/交互应查询该标志以屏蔽输入。
    /// </summary>
    public partial class InputGuard : Node
    {
        private int _depth = 0;
        public bool Blocked => _depth > 0;

        public void Acquire()
        {
            _depth++;
        }

        public void Release()
        {
            if (_depth > 0) _depth--;
        }
    }
}
