# Enhanced Stamina System (Exhaustion)

A realistic stamina system for Space Engineers, building on the original *Stamina* mod by keyspace and *Exhaustion* by The Screw Up Team.

This mod introduces a fully configurable stamina mechanic that encourages automation and thoughtful survival gameplay.

## Features

- 🔋 **Stamina usage** for movement (walking/running), work (welding/grinding/drilling), and driving.
- 🧍‍♂️ **Stamina recovery** when sitting, laying, or resting.
- 🛠️ Integrated with **Text HUD API** for an immersive custom HUD panel.
- 🥤 Compatible with **"Eat. Drink. Sleep. Repeat!"** – restore stamina by eating or drinking.
- 🖥️ Easy in-game configuration:
  - Toggle stamina usage for Movement, Work, and Driving
  - Adjust stamina costs and regeneration rates
- 🌤️ Automatically hides the stamina HUD when the vanilla HUD is hidden.

## Setup & Configuration

This mod uses the **Text HUD API** (Workshop ID: `758597413`) to provide a menu and HUD overlay.  
Make sure this dependency is enabled!

### Stamina Costs
- `CostLow`: e.g., walking
- `CostMedium`: e.g., light work or driving
- `CostHigh`: e.g., sprinting or heavy tool use

### Stamina Gains
- `GainLow`: passive recovery
- `GainMedium`: resting actions like sitting
- `GainHigh`: laying down or eating/drinking

You can customize all these values using the **Text HUD mod menu** in-game.

### Advanced Configuration

For power users, additional settings are available in the mod's **configuration file**.  
These options allow finer control over stamina mechanics, HUD positioning, visibility behavior, stamina drain multipliers, and more.

Instructions for accessing and editing the config file are provided in the mod folder.

## Compatibility

✅ Works in both singleplayer and multiplayer  
✅ Compatible with other mods (no block overrides)  
✅ Non-intrusive and script-based

## License

All files not otherwise marked are licensed under [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/)  
Authored and extended by The Screw Up Team with improvements by [Your Name or Team Name].

This mod is based on:

- [keyspace's Stamina mod](https://github.com/keyspace/Stamina/blob/master/Data/Scripts/Stamina/Hud.cs)
- Networking code originally by [THDigi](https://github.com/THDigi/SE-ModScript-Examples/tree/738e02fdddfbd03de4018829784b5ccb1f6cf251/Data/Scripts/Examples/Example_NetworkProtobuf)
- [Text HUD API](https://steamcommunity.com/sharedfiles/filedetails/?id=758597413) by DraygoKorvan (MIT License)  
  Earlier versions available [on GitHub](https://github.com/DraygoKorvan/HUDApi)

See [LICENSE.txt](LICENSE.txt) for full licensing details.
