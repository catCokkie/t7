# Silent Testimony Demo

目标：一款以“时间循环 + 线索探索 + 理智”核心为卖点的 2D AVG Demo（Godot 4.x + C#）。

版本：0.2.0（Demo 开发中）

运行
- 引擎：Godot 4.x（Mono/C#）
- 直接用 Godot 打开仓库根目录，运行 `Scenes/MainMenu.tscn` 即可。

导出（Windows）
- 已提供 `export_presets.cfg`，目标输出：`Build/Windows/SilentTestimony.exe`。
- 在 Godot 菜单中选择：Project → Export → Windows Desktop → Export Project。

操作与热键
- 移动：W/A/S/D
- 互动：E
- 潜行：Shift
- 跑步：Ctrl（依项目配置）
- 背包：I
- 证据板：O（Esc 关闭）
- 暂停：Esc
- 调试叠加：F1（显示 FPS、时间、状态）
- 快速存档/读档：F5 / F9

当前进度（摘自 todo）
- 已完成：核心系统（时间/玩家/交互/背包/证据/设置/存档）、主菜单、暂停菜单、调试叠加、后处理（理智）
- 新增：
  - 场景切换淡入淡出（更平滑的过场表现）
  - 存档元数据（Day/Hour/时间戳），主菜单 Continue 显示详情
  - 自动存档（进入场景后；避免读档中重复触发）
  - 读档恢复游戏内时钟（Day/Hour），并立即再保存同步元数据
  - EvidenceBoard 支持 Esc 关闭
- 待办：
  - UI/UX 统一（提示文案、开关窗口的键位行为）
  - 音频总线与交互/拾取 SFX 完善
  - 性能与资源清理（目标 1080p/60fps）

已知问题
- 首次运行若无存档，主菜单 Continue 按钮禁用；产生存档后会自动显示 Day/Hour。
- 若切场景资源较大，极少数机器上淡入前可能仍见到一帧卡顿（建议后续加入加载占位界面）。

项目结构（简）
- 场景：`Scenes/`
- 脚本：`Scripts/`
- 资源：`Resources/`
- UI：`Scenes/UI/`

许可证
- Demo 阶段未指定公开许可证；如需使用或传播，请先联系作者。
