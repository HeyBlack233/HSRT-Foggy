# Foggy

A [Human: Fall Flat](https://store.steampowered.com/app/477160/Human_Fall_Flat/)
BepInEx plugin and an extension for **HSRTimer**: it contributes one extra
rule tag, `Foggy`.

With the tag enabled in HSRTimer's settings panel, every level the run enters
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

## Build

Requires the .NET SDK. Reference roots are configured in
`Directory.Build.props` (overridable via the `GAME_MANAGED`, `BEPINEX_CORE`
and `HSRTIMER_DIR` environment variables):

1. Build HSRTimer first (same configuration), e.g.
   `dotnet build src/HSRTimer/HSRTimer.csproj` in the HSRTimer repository.
2. `dotnet build src/Foggy/Foggy.csproj`

## Install

Copy the built `Foggy.dll` into the game's `BepInEx/plugins/` folder alongside
HSRTimer, then enable the `Foggy` tag on HSRTimer's settings panel Category
page (or `enabled = Foggy` in `tags.ini`).

## 中文说明

Foggy 是 HSRTimer 的扩展插件，提供 `Foggy` 规则词条：在计时器设置中勾选后，
进入的每一关都会自动将雾浓度调至最大值，并禁用游戏默认的 `,` / `.` 雾浓度
调节按键。覆盖在关卡进行中与关卡之间的加载阶段持续生效；退出到菜单/大厅
或取消勾选后恢复正常雾浓度。
