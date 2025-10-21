# 🧭 Silent Testimony — 全阶段 To-Do 列表（Demo 版）

> **目标：**  
> 构建一个以“时间循环 + 解谜探索 + 叙事”为核心的 2D AVG 游戏 Demo  
> 使用 **Godot 4.x + C#（Mono）**

---

## ✅ Phase 0：项目初始化与核心架构 (Project Setup)

- [x] 创建新的 **Godot 4.x (Mono)** 项目  
- [x] 建立核心目录结构  
  - `Scenes/`, `Scripts/`, `Assets/`, `Resources/`, `UI/`  
- [x] 确认 `.sln` 已生成  
- [x] 设置 C# 命名空间：`SilentTestimony`
- [x] 创建全局事件总线 `GlobalEventBus.cs`  
  - 定义跨系统信号（`PlayerSpotted`, `NoiseMade`, 等）

---

## ⚙️ Phase 1：核心机制 – 时间与玩家状态 (Time & Player Vitals)

- [x] **TimeManager.cs**
  - 全局单例注册
  - 实现时间流逝 (`_Process(delta)`)
  - 发出信号：`HourChanged`, `DayChanged`
- [x] **PlayerStats.cs**
  - 全局单例注册
  - 定义属性：`Health`, `Sanity`
  - 创建 `ChangeHealth()` / `ChangeSanity()`
  - 发出信号：`HealthChanged`, `SanityChanged`

---

## 🧍 Phase 2：玩家与交互 (Player & Interaction)

### 🎮 2.1 玩家场景
- [x] 创建 `Player.tscn`（根节点 `CharacterBody2D`）
- [x] 添加 `CollisionShape2D` 与 `Sprite2D`  
- [x] 添加脚本 `PlayerController.cs`

### 🕹 2.2 玩家控制器
- [x] 实现 `_PhysicsProcess()` 八方向移动
- [x] 添加潜行状态机  
  - 枚举 `PlayerState { Walking, Sneaking }`  
  - 潜行时速度降低

### 🧩 2.3 交互接口 (当前进行)
- [x] 创建 `Scripts/Interfaces/IInteractable.cs`  
  - `void Interact(Node2D interactor)`  
  - `string GetInteractPrompt()`  
- [x] 在 `PlayerController` 中集成交互检测逻辑  
  - 添加列表 `_nearbyInteractables`  
  - 检测输入 `InputEvent` → 调用 `Interact()`
- [x] 创建示例对象 `TestNote.tscn` 实现接口  
  - 测试输出 `"按 [E] 阅读笔记"`

### 🧭 2.4 交互检测器 (下一步)
- [x] 在 `Player.tscn` 添加 `Area2D` 命名为 `Interactor`  
- [x] 添加检测范围 `CollisionShape2D`  
- [x] 在 `PlayerController` 绑定 `body_entered` / `body_exited` 信号  
- [x] 实现距离最近优先的交互逻辑  
- [x] 添加 UI 提示组件（InteractionPrompt）

> **当前实现要点：** 接口定义统一调用入口，交互器根据最近目标判定触发 `Interact()`，并驱动 UI 提示在范围内实时更新。  

- [ ] 优化交互提示刷新策略（例如按需更新或更智能的提示优先级）。  

---

## 🧠 Phase 3：AI 与环境 (AI & World)

- [ ] **EnemyAI.tscn**
  - 基础 `CharacterBody2D` + `EnemyAIController.cs`
- [ ] 实现视觉系统
  - 添加 `Area2D`（扇形视野）与 `RayCast2D`
  - 视野内检测玩家是否被墙遮挡
- [ ] 实现状态机 FSM
  - `Patrolling`, `Alerted`, `Chasing`
- [ ] 导航寻路
  - 使用 `NavigationServer2D.GetPath()`
- [ ] 场景管理
  - 创建 `SceneLoader.cs`
  - 创建可交互门 `Door.tscn`（调用 `ChangeScene()`）

---

## 🔐 Phase 4：物品与解谜 (Inventory & Puzzles)

- [ ] **InventoryItemData.cs**
  - 定义：`ItemID`, `Name`, `Description`, `Icon`, `IsKeyItem`
- [ ] **InventoryManager.cs**
  - 列表存储物品  
  - `AddItem()`, `HasItem()`  
  - 发出信号：`InventoryChanged`
- [ ] **LockedDoor.tscn**
  - 实现 `IInteractable`
  - 检查 `InventoryManager.HasItem(RequiredKeyItemID)`
  - 若匹配则开门

---

## 📜 Phase 5：叙事系统 (Narrative Systems)

- [ ] **EvidenceData.cs**
  - 定义：`EvidenceID`, `Title`, `Content`
- [ ] **EvidenceManager.cs**
  - 管理收集的证据
- [ ] **EvidenceBoard.tscn**
  - 通过 `GridContainer` 或 `GraphEdit` 展示证据关系

---

## 💫 Phase 6：反馈系统 (Feedback Systems)

- [ ] **AudioManager.cs**
  - `PlaySFX(AudioStream sfx, Vector2 pos)`  
  - `PlayHeartbeat(float intensity)`
- [ ] **PostProcessController.cs**
  - 动态调整理智Shader参数（暗角、噪点）
- [ ] 创建 Shader 文件：
  - `vignette.gdshader`
  - `noise.gdshader`

---

## ⚙️ Phase 7：系统功能 (System Features)

- [ ] **SettingsMenu.tscn**
  - 音量滑条、分辨率、窗口模式
  - 保存配置至 `user://config.json`
- [ ] **SaveManager.cs**
  - 结构化 JSON 存档（位置、场景、生命、理智、物品）
- [ ] **PauseMenu.tscn**
  - 暂停、继续、设置、退出选项

---

## 🎬 Phase 8：Demo 展示与打磨 (Demo Polish)

- [ ] 主菜单 (`MainMenu.tscn`)  
  - 开始游戏 / 继续 / 退出
- [ ] Demo 关卡场景展示核心机制  
  - 时间变化 / 解谜 / AI 追踪 / Sanity 效果
- [ ] 调试工具 `DebugOverlay.tscn`  
  - FPS、AI 状态、时间、TimeScale
- [ ] 整合存档系统 + 设置  
- [ ] 优化音效、光影、Shader 效果  
- [ ] 添加 3 分钟完整可玩循环（时间 → 解谜 → 追逐 → 重置）

---

## 🧩 附录：工程规范与优化建议

- [ ] **统一命名空间**  
  `SilentTestimony.Systems`, `SilentTestimony.Player`, `SilentTestimony.UI`
- [ ] **脚本模块划分**
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
- [ ] **调试功能**
  - `F1` 显示调试界面  
  - `F5` 重载场景  
  - 显示 FPS、内存、AI 数量  
- [ ] **时间系统调试**  
  - 使用 `Engine.time_scale` 控制游戏速度  
- [ ] **版本控制友好性**  
  - 数据文件采用 `.json` + `.tres` 混合方式  
  - 使用 `.gdignore` 避免冲突  
- [ ] **后续扩展**  
  - 加入对话系统 (`DialogueManager`)  
  - 加入事件驱动剧情（基于 Evidence / Time）

---

## 📅 开发周期建议（约 6 周）

| 周次 | 阶段 | 目标 |
|------|------|------|
| 第 1–2 周 | Phase 3–4 | AI + 解谜系统 |
| 第 3 周 | Phase 5 | 叙事系统基础 |
| 第 4 周 | Phase 6–7 | 音频 / 存档 / 设置 |
| 第 5–6 周 | Phase 8 | 整合打磨，形成 Demo 展示版 |