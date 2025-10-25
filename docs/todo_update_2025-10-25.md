# 维护更新（2025-10-25）

本更新基于当前仓库实现进度，对 Phase 8（Demo 打磨）进行状态同步与后续待办拆解。

已完成
- 场景切换淡入/淡出：`SceneLoader` 调用 `PostProcessController.FadeOut/FadeIn`（约 0.3–0.5s）。
- 存档元数据：保存 Day/Hour/时间戳；主菜单 Continue 按钮显示详情。
- 自动存档：进入新场景后保存；读档完成后保存快照以刷新 Continue。
- 读档恢复时间：`TimeManager.SetClock(day, hour)`，相关系统即时响应。
- EvidenceBoard：支持 Esc 关闭。
- 交付准备：新增 Windows 导出预设（`export_presets.cfg`，输出到 `Build/Windows/`），完善 `README.md`。

待办（建议短期完成）
- UI/UX 统一：交互提示文案风格、拾取/确认/取消 SFX。
- Shader/后处理微调：Sanity → Vignette/Noise 曲线与上限。
- 稳定性与清理：目标 1080p/60fps；清理未用资源（如 `PlayerProxy.tscn` 若确认不用）。
- QA 清单执行：热键全覆盖（E/I/O/Esc/F1/F5/F9）、暂停菜单流程、Continue 后淡入时机。
- 版本与变更日志：在 `project.godot`/`README.md` 同步版本号与简短更新记录。

说明
- 本文件用于补充 todo 现状，便于演示与交付；如需，我可以将上述“已完成”项同步勾选回 `todo.md` 的 Phase 8 列表（受编码字符影响，建议在编辑器中一起查看后提交）。
