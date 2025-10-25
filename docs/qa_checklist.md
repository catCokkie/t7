# 快速 QA 清单

## 主菜单 / Continue
- 首次启动 Continue 禁用；产生存档后启用
- Continue 文案：`Continue - Day X HH:MM (yyyy-MM-dd HH:mm)`

## 快速存读档 F5/F9
- 位置、Day/Hour 恢复正确；无错误日志

## 自动存档
- 进入新场景后自动保存；读档流程中不重复触发

## 场景切换淡入淡出
- 新游戏/读档/返回主菜单：黑屏淡出→切换→淡入，无闪屏

## 时间恢复（读档）
- `TimeManager` 恢复 Day/Hour；相关系统响应（后处理/心跳）

## 热键与交互
- W/A/S/D、Shift、Ctrl、E、Esc：行为一致
- I、O 面板开/关；Esc 关闭；面板打开时屏蔽多余输入
- F1 调试叠加；F5/F9 快速存读

## 暂停菜单
- Resume/Settings/Save/Load/ReturnToMain/Quit 正常
- 无存档时 Load 灰显；存/读后按钮状态刷新

## UI/UX 一致性
- 交互提示：样式统一（按 E：{动作}）、离开范围后 0.15s 淡出

## 设置与音频
- Master/BGM/SFX 实时生效；重启后从 `user://config.json` 恢复

## 性能与日志
- 1080p/60fps 目标；无 Missing Resource/报错

## 导出版本
- `Build/Windows/SilentTestimony.exe` 行为与编辑器一致
