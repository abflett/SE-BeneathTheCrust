# Working Knowledge Roadmap Notes

These notes track near-term Working Knowledge priorities from playtesting and feedback. They complement the shorter release-gate checklist in [Working Knowledge release roadmap](working_knowledge_release_roadmap.md).

## 0.9.x - Bugfix And Compatibility Testing

### Feedback / Chat Message Fixes

- Continue testing recent feedback and progress reporting fixes in normal play.
- Review chat message frequency during discovery, research, and Proficiency updates.
- Confirm that existing config options properly reduce or disable progress and info message spam.
- Improve config documentation where needed.

### Modded Block Compatibility Testing

- Test how normal modded blocks behave with Working Knowledge.
- Confirm whether modded blocks are currently buildable as already researched and full Proficiency.
- Verify that block mods do not unintentionally become locked, blocked, or partially restricted by the progression system.
- Document current behavior clearly so players know whether configuration is required.

## 0.10.0 - Feedback-Driven Feature Update

### HUD Progress Display

- Investigate replacing or supplementing chat progress messages with a cleaner HUD-style display.
- Prototype recent progress bars for discovery, research, or Proficiency updates.
- Ideally show only the most recent few progress entries.
- Have progress bars fade out after a few seconds of no updates.
- Keep chat messages available as a fallback or optional mode.
- Evaluate whether this can be done without heavy dependencies.
- Investigate frameworks such as Rich HUD, but avoid adding required dependencies unless the benefit is worth it.

### Custom Modded Block Integration

- Add a way for admins or players to assign custom modded blocks into Working Knowledge schematic groups.
- Allow modded blocks to become part of the normal research and Proficiency system instead of always acting as already unlocked.
- Consider config-driven definitions for custom block groups.
- Make the system safe for servers and modpacks where many block mods are installed.

### Automation Mod Attribution

- Investigate compatibility with automation mods such as Nanobot Build and Repair System.
- Determine whether automated welding and grinding actions expose a player owner, builder, controller, or usable attribution source.
- Check whether Working Knowledge can safely grant research and Proficiency from automated actions.
- Add a fallback attribution option if possible.
- If reliable attribution is not possible, document the limitation clearly.

## General Goals

- Keep Working Knowledge usable without requiring players to edit config.
- Use config mainly for tuning, server balancing, and advanced customization.
- Avoid hard dependencies on companion mods unless absolutely necessary.
- Prioritize compatibility with common modpacks.
- Continue using player feedback to guide quality-of-life improvements.
