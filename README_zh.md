# Foggy

《人类一败涂地》（*Human: Fall Flat*）的 BepInEx 插件 —— 是
**[HSRTimer](https://github.com/TwilightCup/HSRTimer)** 的扩展插件：为计时器提供
一个额外规则词条 `Foggy`。同时兼容 HSRTimer 的分支 **TwilightTimer**，装了哪个
就用哪个。

在计时器设置中勾选本词条后，进入的每一关都会自动将雾浓度调至**最大值**，并禁用
游戏默认的 `,` / `.` 雾浓度调节按键。覆盖在关卡进行中、关卡之间的加载阶段与重试
重载期间持续生效；退出到菜单/大厅或取消勾选后恢复正常雾浓度。

> **English documentation**: [README.md](README.md)

---

## 安装

将构建产物 `Foggy.dll` 与 HSRTimer 或 TwilightTimer 一起放入游戏的
`BepInEx/plugins/` 目录，然后在计时器设置面板的 Category 页勾选 `Foggy` 词条
（或在 `tags.ini` 中写 `enabled = Foggy`）。

TwilightTimer 比赛模式下由赛制推送 tag 集合，将 `Foggy` 加入推送集合即可生效。

## 机制

游戏的全局雾开关只有一个：`CaveRender.fogDensityMultiplier`；
`CaveRender.OnPreCull` 每渲染帧用它重算 `RenderSettings.fogDensity`。Foggy 在
规则生效范围内将该倍率钉在游戏自带按键调节的上限（`60`），并在游戏自身雾逻辑
每帧运行后重新钉住 —— 这同时废掉了 `,`/`.` 按键的作用，暂停菜单的雾重置也会被
立即复原。

对两个计时器的依赖均为软依赖：装了任一分支即可加载，向检测到的计时器注册
`Foggy` 规则（若其初始化较晚会等待就绪后再注册）；两者都未安装时保持惰性。

## 构建

需要 .NET SDK。引用路径在 `Directory.Build.props` 中配置（可用环境变量
`GAME_MANAGED`、`BEPINEX_CORE`、`HSRTIMER_DIR`、`TWILIGHTTIMER_DIR` 覆盖）：

1. 先构建两个计时器（相同 Configuration）：在 HSRTimer 仓库执行
   `dotnet build src/HSRTimer/HSRTimer.csproj`，在 TwilightTimer 仓库执行
   `dotnet build src/TwilightTimer/TwilightTimer.csproj`。
2. `dotnet build src/Foggy/Foggy.csproj`
