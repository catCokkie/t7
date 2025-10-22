# 🧭 Silent Testimony 全阶段 To-Do 列表（Demo 版）

> 目标：
> 构建一个以“时间循环 + 解谜探索 + 叙事”为核心的 2D AVG 游戏 Demo  
> 使用 Godot 4.x + C#（Mono）

---

## Phase 0：项目初始化与核心架构 (Project Setup)

- [x] 创建新的 Godot 4.x (Mono) 项目  
- [x] 建立核心目录结构  
  - `Scenes/`, `Scripts/`, `Assets/`, `Resources/`, `UI/`  
- [x] 确认 `.sln` 已生成  
- [x] 设置 C# 命名空间：`SilentTestimony`
- [x] 创建全局事件总线 `GlobalEventBus.cs`  
  - 定义跨系统信号（`PlayerSpotted`, `NoiseMade`, 等）

---

## Phase 1：核心机制——时间与玩家状态 (Time & Player Vitals)

- [x] TimeManager.cs（Autoload）  
  - 实现时间流逝 `_Process(delta)`，信号：`HourChanged`, `DayChanged`
- [x] PlayerStats.cs（Autoload）  
  - 属性：`Health`, `Sanity`；方法：`ChangeHealth()` / `ChangeSanity()`；信号：`HealthChanged`, `SanityChanged`

---

## Phase 2：玩家与交互 (Player & Interaction)

### 2.1 玩家场景
- [x] `Player.tscn`（根节点 `CharacterBody2D`）
- [x] 添加 `CollisionShape2D` 与 `Sprite2D`
- [x] 脚本 `PlayerController.cs`

### 2.2 玩家控制器
- [x] `_PhysicsProcess()` 八方向移动
- [x] 潜行/行走/奔跑状态机（噪音计时器事件）

### 2.3 交互接口
- [x] `Scripts/Interfaces/IInteractable.cs`（`Interact(Node2D)`, `GetInteractPrompt()`）
- [x] `PlayerController` 集成交互检测与输入调用
- [x] 示例对象 `TestNote.tscn`（现已接入笔记阅读 UI）

### 2.4 交互检测器
- [x] 在 `Player.tscn` 添加 `Area2D` Interactor + `CollisionShape2D`
- [x] 连接 `body_entered` / `body_exited`
- [x] 最近目标优先选择逻辑
- [x] UI 提示（`InteractionPrompt`）+ 刷新修复

> 待优化：交互提示刷新策略（按需更新/更智能的优先级）

---

## Phase 3：AI 与环境 (AI & World)

- [x] 敌人场景 `EnemyAI.tscn` + `EnemyAIController.cs`
- [x] 视觉系统（`Area2D` 扇形视野 + `RayCast2D` 视线遮挡）
- [x] 状态机 FSM：`Patrolling`, `Alerted`, `Chasing`
- [x] 导航寻路（`NavigationServer2D`）
- [x] 场景管理（`SceneLoader.cs`，交互门切换场景）
  - Autoload: `SceneLoader`
  - `SceneDoor.tscn` + `SceneDoor.cs`（与出生点名 `TargetSpawnPointName`）
  - 示例：`TestLevel.tscn` ⇄ `Level2.tscn`

---

## Phase 4：物品与解谜 (Inventory & Puzzles)

- [x] `InventoryItemData.cs`（`ItemID`, `Name`, `Description`, `Icon`, `IsKeyItem`）
- [x] `InventoryManager.cs`（Autoload；`AddItem()`, `HasItem()`, `InventoryChanged`）
- [x] `LockedDoor.tscn` + `LockedDoor.cs`（检查 `HasItem(RequiredKeyItemID)`）
- [x] 物品拾取（Pickup）
  - `Scripts/World/ItemPickup.cs` + `Scenes/Props/ItemPickup.tscn`
  - 示例物品资源：`Resources/Items/TestKey.tres`
- [x] 上锁门验证（与钥匙联动）
  - `RequiredKeyItemID=key_lab` 测试通过

---

## Phase 5：叙事系统 (Narrative Systems)

- [x] 笔记阅读 UI（NoteReader）
  - `Scenes/UI/NoteReader.tscn` + `Scripts/UI/NoteReader.cs`
  - `Scripts/UI/NoteReaderManager.cs`（Autoload）
  - `Scripts/World/NotePickup.cs` + `Scenes/Props/NotePickup.tscn`
- [x] EvidenceData.cs（`EvidenceID`, `Title`, `Content`）
- [x] EvidenceManager.cs（Autoload，管理收集的证据）
- [x] EvidenceBoard.tscn + EvidenceBoard UI（`ItemList` 左侧列表，右侧详情）
- [x] EvidenceBoardManager.cs（Autoload，按 `O` 打开/关闭）
- [x] NotePickup 可选绑定 EvidenceData，阅读时自动登记证据
- [x] 示例证据资源：`Resources/Evidence/TestNoteEvidence.tres`

---

## Phase 6：反馈系统 (Feedback Systems)

- [x] AudioManager（脚步/交互 SFX、心跳）（Autoload）
- [x] PostProcessController（Sanity Shader：暗角/噪点）（Autoload）
- [x] Shaders：`shaders/vignette.gdshader`, `shaders/noise.gdshader`
- [x] 心跳与理智联动（在 PostProcessController 内映射 Sanity→Heartbeat 强度）
- [x] 玩家脚步音效（按走/跑分别配置，随计时器触发）

---

## Phase 7：系统功能 (System Features)

- [x] Inventory UI 覆盖层（按 `I` 打开）
  - `Scenes/UI/InventoryUI.tscn` + `Scripts/UI/InventoryUI.cs`
  - `Scripts/UI/InventoryUIManager.cs`（Autoload）
- [x] Settings（音量/分辨率/窗口模式，存 `user://config.json`）
  - `Scripts/Systems/SettingsManager.cs`（Autoload）
  - `Scenes/UI/SettingsMenu.tscn` + `Scripts/UI/SettingsMenu.cs`
- [x] SaveManager（JSON 存档：位置/场景/生命/理智/物品/证据）
  - `Scripts/Systems/SaveManager.cs`（Autoload）
- [x] PauseMenu（暂停/继续/设置/保存/退出，Esc 打开）
  - `Scenes/UI/PauseMenu.tscn` + `Scripts/UI/PauseMenu.cs`
  - `Scripts/UI/PauseMenuManager.cs`（Autoload）

---

## Phase 8：Demo 展示与打磨 (Demo Polish)

- [x] 测试关卡 `Scenes/TestLevel.tscn`（最小可交互 Demo）
  - Player、钥匙拾取、上锁门、笔记、简易地面
- [ ] 主菜单 `MainMenu.tscn`（开始/继续/退出）
- [ ] Demo 场景展示核心机制（时间/解谜/AI 追踪/Sanity 效果）
- [x] 调试工具 `DebugOverlay.tscn`（FPS、时间、背包/Evidence 数量、AI 数量、TimeScale 控制；F1 显示/隐藏；F5/F9 快存快读）
- [ ] 整合存档系统 + 设置
- [ ] 优化音效、光影、Shader 效果
- [ ] 添加 3 分钟完整可玩循环（时间 + 解谜 + 追逐 + 重置）

---

## 附录：工程规范与优化建议

- [ ] 统一命名空间：`SilentTestimony.Systems`, `SilentTestimony.Player`, `SilentTestimony.UI`
- [ ] 脚本模块划分
```
Scripts/
 ├─ Core/
 ├─ Systems/
 ├─ Player/
 ├─ AI/
 ├─ UI/
 ├─ Interfaces/
 └─ Data/
```
- [ ] 调试功能：`F1` 调试界面、`F5` 重载场景、显示 FPS/内存/AI 数量
- [ ] 时间系统调试：`Engine.time_scale` 控制速度
- [ ] 版本控制友好：数据 `.json` + `.tres`，使用 `.gdignore`
- [ ] 后续扩展：对话系统 (`DialogueManager`)、基于 Evidence/Time 的事件驱动剧情

---

## 开发周期建议（≈ 6 周）

| 周次 | 阶段 | 目标 |
|------|------|------|
| 第 1–2 周 | Phase 3 | AI + 解谜系统 |
| 第 3 周 | Phase 5 | 叙事系统基础 |
| 第 4 周 | Phase 6–7 | 音频 / 存档 / 设置 |
| 第 5–6 周 | Phase 8 | 整合打磨，形成 Demo 展示 |
