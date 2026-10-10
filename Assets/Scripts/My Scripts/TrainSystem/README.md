# 西部三站火车系统

已配置在 `Assets/Scenes/Game.unity` 的 `Western Train System` 对象上。
直接进入 Play Mode 即可运行，不需要再手动挂脚本。

## 运行方式

- 使用 Western 素材中的车头、煤车和一节客车厢，保留原有轨道与三个站台。
- 初始位于黑水镇站，立即倒车出站；整列车清过道岔后停稳 2 秒，再向前进入环线。
- 随后按北站 → 东站 → 黑水镇的顺序停靠，反复循环。
- 每次到站停稳后随机等待 60–120 个游戏秒，再出发。暂停游戏时计时也暂停；慢动作会同步影响火车。
- 停车点以客车厢对齐站台为准。车厢分别沿轨道取样转弯，车头的独立车轮会随行驶方向转动。
- 正常最高速度 8 米/秒，倒车最高速度 3 米/秒；进出站有加减速。

## 调整

选中 `Western Train System`，在 `Train Controller` 中修改：
`Cruise Speed`、`Reversing Speed`、`Acceleration`、`Braking`、`Minimum Dwell`、`Maximum Dwell`。
`Clearance Distance` 是倒车退出后车头越过道岔的距离，必须保证整车已经清过交叉点。

移动已有轨道或站台后，可以在编辑模式执行：
`Tools > Train System > Rebuild Route From Existing Tracks`，然后保存场景。
该工具针对当前“一个支线 + 一个环线 + 三个站台”的布局生成路径；
不支持任意多个道岔，遇到不符合条件的连接会报错。

不要直接移动整个 `Western Train System` 对象，否则路径会与轨道模型分离。
源码和模型均不依赖 MCP 才能运行；MCP 只用于配置与检查编辑器。

## 验证与恢复

`Western.Trains.Tests` 中的 EditMode 测试覆盖两圈三站顺序、停靠时长、
倒车换向时的位置连续性、不同时间步长和零时间步长。

添加火车前的场景副本保存在：
`.utmp/train-system/Game.before-train.unity`。
该目录属于生成缓存，不纳入版本控制。
