# Foggy

A [Human: Fall Flat](https://store.steampowered.com/app/477160/Human_Fall_Flat/)
BepInEx plugin and an extension for speedrun timers: it contributes one extra
rule tag, `Foggy`, to **HSRTimer** or its fork **TwilightTimer** — whichever
is installed.

With the tag enabled in the timer's settings panel, every level the run enters
is played at **maximum fog density**, and the game's default `,` / `.` fog
keys are disabled for as long as the override is in scope — level play, the
loading phases between levels, and retry reloads. Leaving the run for a
menu/lobby (or unchecking the tag) restores normal fog.

## How it works

The game has a single global fog dial, `CaveRender.fogDensityMultiplier`;
`CaveRender.OnPreCull` re-derives `RenderSettings.fogDensity` from it every
rendered frame. Foggy pins that multiplier to the game's own key-clamp maximum
(`60`) whenever the rule is in scope, and re-pins it after the game's own fog
handling each frame — which is what disables the `,`/`.` keys — so the pause
menu's fog reset is reverted as well.

Both timer dependencies are soft: the plugin loads with either fork installed,
registers the `Foggy` rule with whichever timer it finds (waiting for its
registry if that timer initializes later), and stays inert if neither is
present. Under TwilightTimer's match mode, round tag sets are pushed by the
match system — include `Foggy` in the pushed set to use it in matches.

## Build

Requires the .NET SDK. Reference roots are configured in
`Directory.Build.props` (overridable via the `GAME_MANAGED`, `BEPINEX_CORE`,
`HSRTIMER_DIR` and `TWILIGHTTIMER_DIR` environment variables):

1. Build both timers first (same configuration), e.g.
   `dotnet build src/HSRTimer/HSRTimer.csproj` in the HSRTimer repository and
   `dotnet build src/TwilightTimer/TwilightTimer.csproj` in the TwilightTimer
   repository.
2. `dotnet build src/Foggy/Foggy.csproj`

## Install

Copy the built `Foggy.dll` into the game's `BepInEx/plugins/` folder alongside
HSRTimer or TwilightTimer, then enable the `Foggy` tag on the timer's
settings panel Category page (or `enabled = Foggy` in `tags.ini`).

## 中文说明

Foggy 是计时器扩展插件，可与 HSRTimer 或其分支 TwilightTimer 联动（装了哪个
就用哪个，两者扩展接口一致，仅 GUID/命名空间不同）。提供 `Foggy` 规则词条：
在计时器设置中勾选后，进入的每一关都会自动将雾浓度调至最大值，并禁用游戏
默认的 `,` / `.` 雾浓度调节按键。覆盖在关卡进行中与关卡之间的加载阶段持续
生效；退出到菜单/大厅或取消勾选后恢复正常雾浓度。TwilightTimer 比赛模式下
由赛制推送 tag 集合，将 `Foggy` 加入推送集合即可生效。
