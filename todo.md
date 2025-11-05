# Silent Testimony 全阶段 To‑Do 列表（Demo 版）

> 目标：做一个以“时间循环 + 线索探索 + 理智”为核心的 2D AVG Demo（Godot 4.x + C#）。

---

## Phase 8 细目清单（Demo 打磨）

- [x] 主菜单 `Scenes/MainMenu.tscn`
  - [x] 新游戏 / 继续 / 设置 / 退出
  - [x] 继续按钮读取最近存档并显示 Day/Hour/时间戳
  - [x] 跳转 `SettingsMenu.tscn`
- [x] 流程联通
  - [x] 主菜单选项进入测试关卡 `Scenes/TestLevel.tscn`
  - [x] PauseMenu 弹确认（返回主菜单）
  - [x] SceneLoader 加入黑屏淡入/淡出（约 0.3–0.5s）
- [x] 存档系统
  - [x] 统一字段与路径：场景/位置/生命/理智/物品/证据 + Day/Hour/时间戳
  - [x] 快捷键 F5/F9；切场景自动存档（读档中避免重复）
  - [x] 主菜单继续/读取统一走 `SaveManager`
  - [x] 读档恢复游戏时间：`TimeManager.SetClock(day, hour)`
  - [ ] 设计变更：取消“季节 Season 元数据”及其在 Continue 的展示（不纳入本阶段）
 
- [x] 瓦片/网格统一（16×16）
  - [x] 新增 `GridUtility` 作为唯一格子尺寸与转换入口
  - [x] 物件脚本移除 `TileCellSize`，统一使用全局 CellSize=16
  - [x] 玩家网格移动的回退格子大小改为全局 `GridUtility.CellSize`
  - [x] `Minimal16.tres` 配置 (0,0) 基础瓦片 16×16 占位碰撞
  - [x] `RuntimeTilemapBuilderLayers.GenerateRuntimeBodies` 默认关闭（碰撞改在 TileSet 内维护）
- [ ] UI/UX 抛光
  - [ ] 交互提示文案风格统一（样式：按 E：{动作}；超 24 字省略；0.15s 淡出）
  - [x] NoteReader/EvidenceBoard 打开/关闭；Esc 可关闭
  - [ ] 拾取/确认/取消 视觉提示 + SFX（占位音效）
- [ ] 音频整理
  - [ ] Master/SFX/UI/Ambience 总线；设置菜单实时生效联动
  - [ ] 拾取/开门/阅读/确认/取消/切换等基础 SFX 覆盖
- [ ] 视觉与 Shader
  - [ ] Sanity 效果曲线微调（暗角/噪声随理智变化）
  - [ ] Vignette/Noise 参数上限与默认值校准
  - [ ] 关键节点统一风格（提示色/高亮）
- [ ] 性能与稳定性
  - [ ] 目标 1080p/60fps；无报错/缺失资源日志
  - [ ] 清理未用资源（如 `PlayerProxy.tscn` 等）
- [ ] QA 回归清单
  - [ ] 热键：E/I/O/Esc/F1/F5/F9 行为与叠加提示（含 UI 打开时输入屏蔽）
  - [ ] 拾取钥匙/开门/阅读/登记证据等常规流程（含 SFX）
  - [ ] 暂停/继续/设置/返回主菜单 等状态切换（无卡顿、无误触）
- [x] 发布与交付
  - [x] Windows 导出预设 → `Build/Windows/`
  - [x] README：操作说明、已知问题、导出步骤
  - [ ] 版本号与变更日志：`project.godot`/`README` 同步

---

## Phase 7 系统功能（已完成）

- [x] Inventory UI（`I` 打开）：`Scenes/UI/InventoryUI.tscn` + `Scripts/UI/InventoryUI.cs`
- [x] Settings：音量/分辨率/窗口模式（`user://config.json`）
- [x] SaveManager：JSON 存档（位置/状态/物品/证据）
- [x] PauseMenu：暂停/继续/设置/保存/读取/退出（Esc 打开）

---

## Phase 6 反馈系统（已完成）

- [x] AudioManager：SFX 播放与心跳强度联动
- [x] PostProcessController：Sanity Shader（暗角/噪声）
- [x] Shaders：`shaders/vignette.gdshader`, `shaders/noise.gdshader`

---

## Phase 3–5（略）
- [x] 交互/证据/物品/关卡基础流程与 UI 面板

---

## Phase 1–2 基础（已完成）

- [x] 时间系统：`TimeManager`（小时/天变更信号）
- [x] 玩家状态：`PlayerStats`（生命/理智 + 信号）

---

## Phase 0 项目初始化（已完成）

- [x] Godot 4.x (Mono) 项目与解决方案
- [x] 目录结构：`Scenes/`, `Scripts/`, `Resources/`, `UI/`
- [x] 全局事件总线 `GlobalEventBus.cs`

---

## 节奏规划（约 6 周）

| 周次 | 阶段 | 目标 |
|-----|------|------|
| 第 1–2 周 | Phase 3 | AI + 互动系统 |
| 第 3 周   | Phase 5 | 完成系统联通 |
| 第 4 周   | Phase 6 | 音频 / 存档 / 反馈 |
| 第 5–6 周 | Phase 8 | 打磨与成品 Demo |

备注：本文件对原 todo 内容进行了 Phase 8 的集中维护与勾选，历史阶段保留为简表。需要保留更细粒度清单，可在 `docs/todo_update_*.md` 下扩展。

---

## 原清单 Phase 8（勾选版）

- [x] 目录：`Scenes/MainMenu.tscn`
  - [x] 新游戏 / 继续 / 设置 / 退出
  - [x] 继续按钮在有存档时可用并显示 Day/Hour/时间戳
  - [x] 嵌入跳转 `SettingsMenu.tscn`
- [x] 流程联通
  - [x] 主菜单选择进入 `Scenes/TestLevel.tscn`
  - [x] PauseMenu 弹确认（返回主菜单）
  - [x] `SceneLoader` 加入黑幕淡入/淡出（约 0.3–0.5s）
- [x] 存档系统
  - [x] 统一存档字段与路径（含 小时/天/物品/证据/时间戳）
  - [x] 快捷键 F5/F9；切场景自动存档；读档过程中避免重复
  - [x] 主菜单继续/读取统一使用 `SaveManager`
- [ ] UI/UX 抛光
  - [x] `NoteReader`/`EvidenceBoard` 开/关；Esc 可关闭
  - [ ] 提示样式统一；拾取/确认/取消 提示 + SFX
- [x] 交付与文档
  - [x] Windows 导出预设 → `Build/Windows/`
  - [x] `README`：操作说明/已知问题/导出步骤
