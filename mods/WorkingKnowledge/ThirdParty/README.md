# Third-Party Components

Working Knowledge bundles the official **Rich HUD Framework Full Client 1.3.0.0** source so its scripts can communicate with the separately loaded Rich HUD Master Workshop mod.

- Project: <https://github.com/ZachHembree/RichHudFramework.Client>
- Source release: `1.3.0.0`
- Bundled paths: `Data/Scripts/WorkingKnowledge/Integration/RichHudFramework/Client` and `Shared`
- License: MIT; see `RichHudFramework.LICENSE.txt`

Local Full Client adjustment: `Shared/UI/HUD/HudElements/ClickableHudElements/TextBox.cs` suppresses its "Open Chat to Enable Text Editing" warning when input has already been explicitly opened through `OpenInput()`. The Working Knowledge field wrapper uses that public API on focus and closes input on blur; the framework's remaining input behavior is unchanged.

The bundled client is an integration bridge, not the Rich HUD Master implementation. Players and servers still need the Rich HUD Master Workshop dependency (`1965654081`) loaded for the overlay and settings menu to render.

`Integration/RichHud/WkTextHudSettingsLauncher.cs` optionally connects to the public Text HUD API menu protocol when that mod (`758597413`) is already loaded. It adds a player-menu launcher only; no Text HUD rendering client is bundled and no additional required dependency is introduced. The protocol IDs and callbacks were checked against the installed Text HUD API `HudAPIv2.cs` menu classes. Upstream project: <https://github.com/DraygoKorvan/HUDApi>.
