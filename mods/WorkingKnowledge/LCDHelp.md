# Working Knowledge LCD Help

Working Knowledge adds selectable LCD scripts for research, proficiency, identity, and display calibration. They work on normal LCD panels, transparent LCDs, cockpit displays, multi-screen blocks, and the custom Research Sci-Fi Terminal.

## Apps

### Working Knowledge Research

Shows the local viewer's research archives.

- Cycles personal research and faction research when the viewer belongs to a faction.
- Shows active and completed schematic-family counts.
- Pages through research entries automatically.
- Uses `PageTicks`, row count, text, background, and safe-area settings.

### Working Knowledge Proficiency

Shows the local viewer's personal Proficiency.

- Displays player-only hands-on skill progress.
- Shows entries above `0%` and below `100%`.
- Does not show faction Proficiency because Proficiency is not faction-shared.
- Uses `PageTicks`, row count, text, background, and safe-area settings.

### Working Knowledge Identity

Shows the local viewer's player and faction identity.

- Shows faction icon or a fallback tag block.
- Shows faction name, faction tag, and player name.
- Shows `No Faction` / `INDEPENDENT` when the viewer is not in a faction.
- Uses text, background, safe-area, `IconPaddingLeft`, and `IconSize` settings.

### Working Knowledge Calibrator

Shows a fixed diagnostic grid.

- Black background, white 10-pixel grid, and red safe-area border.
- Reads the same `Inset` settings as the other apps.
- Shows the current display name and resolved inset values.
- Keeps fixed diagnostic colors and ignores `TextColor` / `BackgroundColor`.

Use this app when tuning a cropped or off-center display.

## First Setup

1. Open a block's terminal.
2. Select one of the Working Knowledge LCD scripts for the screen you want.
3. Open the block's Custom Data.
4. Edit the generated `[WkKnLCD]` settings or add display-specific sections.

Regular LCD blocks receive only a shared default section:

```ini
[WkKnLCD]
Inset=0,0,0,0
PageTicks=180
RowsFirstPage=7
RowsOtherPages=11
LayoutScale=1
TextSize=1
// TextColor=180,255,220
// BackgroundColor=0,0,0
```

The generated Custom Data also includes a fully commented optional example for a pretend three-screen block: one Research screen, one Proficiency screen, and one Identity screen. Uncomment and edit those sections when you want per-display settings without looking up the format again.

The Research Sci-Fi Terminal receives calibrated defaults for its two custom screens:

```ini
[WkKnLCD]
Inset=0,0,0,0
PageTicks=180
RowsFirstPage=7
RowsOtherPages=11
LayoutScale=1
TextSize=1
// TextColor=180,255,220
// BackgroundColor=0,0,0

[WkKnLCD.Config1]
Display=Large Display
Inset=1%,13%,1%,13%
TextSize=1.1

[WkKnLCD.Config2]
Display=Keyboard
Inset=14,28,6,29
IconPaddingLeft=20
IconSize=1
```

Existing settings are not overwritten. If a section already exists, the mod leaves it alone.

## Section Rules

`[WkKnLCD]` is the default section for every Working Knowledge LCD app on that block.

Display-specific sections override the default section when `Display=` matches the current screen name:

```ini
[WkKnLCD.Config1]
Display=Screen 1
Inset=0,0,0,0
```

The section suffix can be anything unique. These are all valid:

```ini
[WkKnLCD.Config1]
[WkKnLCD.Screen_1]
[WkKnLCD.BedroomStatus]
```

Only `Display=` is used to match a screen. The app name is not required.

Display matching is case-insensitive and checks the surface display name first, then the surface name. Use the display names shown in Space Engineers, such as `Screen 1`, `Left`, `Center`, `Right`, `Large Display`, or `Keyboard`.

Avoid duplicate sections for the same `Display=` value. If duplicates exist, later matching sections can override earlier values.

Comments may start with `//` or `#`.

## Settings

| Setting | Used By | Meaning |
| --- | --- | --- |
| `Display` | Display-specific sections | Screen name to match, such as `Screen 1` or `Large Display`. |
| `Inset` | All apps | Safe area in `left,top,right,bottom` order. |
| `InsetLeft` | All apps | Optional single-side override. |
| `InsetTop` | All apps | Optional single-side override. |
| `InsetRight` | All apps | Optional single-side override. |
| `InsetBottom` | All apps | Optional single-side override. |
| `PageTicks` | Research, Proficiency | Ticks before changing pages. `180` is about three seconds. |
| `RowsFirstPage` | Research, Proficiency | Maximum rows on the first page, which includes the header. |
| `RowsOtherPages` | Research, Proficiency | Maximum rows on later pages without the header. |
| `LayoutScale` | Research, Proficiency, Identity | Multiplier for layout spacing, padding, row height, footer/stat boxes, and icon baseline sizing. |
| `TextSize` | Research, Proficiency, Identity | Multiplier for text size. |
| `TextColor` | Research, Proficiency, Identity | Overrides the text color. |
| `BackgroundColor` | Research, Proficiency, Identity | Overrides the drawn background color. |
| `IconPaddingLeft` | Identity | Moves the identity icon to the right or left. |
| `IconSize` | Identity | Multiplies identity icon size. |

Unknown keys are ignored by apps that do not use them, so one display section can safely contain settings for multiple apps.

## Insets

`Inset` accepts pixels or percentages:

```ini
Inset=14,28,6,29
Inset=1%,13%,1%,13%
InsetLeft=10
InsetTop=5%
```

Order is always:

```text
left,top,right,bottom
```

Use pixels for fine tuning. Use percentages when a display may have different resolutions or aspect ratios.

## Colors

`TextColor` and `BackgroundColor` are optional. If omitted, the app uses the block's normal LCD text and background colors.

Supported color formats:

```ini
TextColor=180,255,220
TextColor=#B4FFDC
BackgroundColor=0,0,0
BackgroundColor=Black
```

Named colors:

- `Black`
- `White`
- `Red`
- `Green`
- `Blue`

Alpha values are not supported. Transparent LCD panels use vanilla brightness behavior, where black or near-black backgrounds are the transparent-looking option. For transparent LCD panels, use:

```ini
BackgroundColor=0,0,0
```

or a very dark color:

```ini
BackgroundColor=3,3,3
```

## Common Examples

### Transparent LCD Status Screen

```ini
[WkKnLCD]
Inset=0,0,0,0
PageTicks=180
RowsFirstPage=7
RowsOtherPages=11
LayoutScale=1
TextSize=1
TextColor=180,255,220
BackgroundColor=0,0,0
```

### Fast Paging Research Display

```ini
[WkKnLCD]
PageTicks=90
RowsFirstPage=6
RowsOtherPages=10
```

### Multi-Screen Billboard

```ini
[WkKnLCD]
TextColor=#B4FFDC
BackgroundColor=0,0,0

[WkKnLCD.Left]
Display=Left
Inset=0,0,0,0

[WkKnLCD.Center]
Display=Center
Inset=8,0,8,0
TextSize=1.15

[WkKnLCD.Right]
Display=Right
Inset=0,0,0,0
PageTicks=240
```

### Research Sci-Fi Terminal

```ini
[WkKnLCD]
Inset=0,0,0,0
PageTicks=180
RowsFirstPage=7
RowsOtherPages=11
LayoutScale=1
TextSize=1

[WkKnLCD.Config1]
Display=Large Display
Inset=1%,13%,1%,13%
TextSize=1.1

[WkKnLCD.Config2]
Display=Keyboard
Inset=14,28,6,29
IconPaddingLeft=20
IconSize=1
```

## Calibration Workflow

1. Select `Working Knowledge Calibrator` on the target display.
2. Look for the red border.
3. If the red border is cropped on the left, increase `InsetLeft` or the first `Inset` value.
4. If it is cropped on the top, increase `InsetTop` or the second `Inset` value.
5. If it is cropped on the right, increase `InsetRight` or the third `Inset` value.
6. If it is cropped on the bottom, increase `InsetBottom` or the fourth `Inset` value.
7. Switch back to `Working Knowledge Research`, `Working Knowledge Proficiency`, or `Working Knowledge Identity`.

The Calibrator shows the display name in the lower-left area. Use that name in `Display=...` for per-screen settings.

## Troubleshooting

### My settings do not apply

- Confirm the selected LCD script is one of the Working Knowledge apps.
- Confirm the section header starts with `[WkKnLCD]`.
- For per-screen settings, confirm `Display=` exactly matches the display name shown by the Calibrator.
- Make sure the setting is inside the correct section.

### My transparent LCD has a solid background

Add this to the matching section:

```ini
BackgroundColor=0,0,0
```

If it still reads too solid, keep the value close to black. Space Engineers transparent LCDs do not use alpha from script colors.

### My text is too large or too small

To change only text size, adjust:

```ini
TextSize=0.9
```

or:

```ini
TextSize=1.15
```

To change the whole app layout size, adjust:

```ini
LayoutScale=0.9
```

or:

```ini
LayoutScale=1.1
```

`LayoutScale` is useful when the app mostly fits but the padding, rows, footer, or stat boxes need a small global nudge. It accepts `0.5` to `1.5`.

### The Identity icon is off-center

Adjust:

```ini
IconPaddingLeft=20
IconSize=1
```

Negative `IconPaddingLeft` moves the icon left. Positive values move it right.
