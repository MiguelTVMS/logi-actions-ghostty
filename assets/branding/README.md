# Icon style

Icons use simple, flat outlines. Glyph SVGs have transparent backgrounds and hollow shapes, without filled mascot bodies, gradients, or shadows. The plugin tile and button templates supply the blue background separately. The original retro ghost is drawn independently; no Ghostty or Pac-Man artwork is copied.

The standalone `ghost.svg` has a transparent background. The plugin tile uses blue `#149CFF` with a white outlined ghost, matching the action defaults. `plugin-icon.svg` is its editable source; `src/package/metadata/Icon256x256.png` is the required transparent 256 × 256 Logitech plugin icon, with the mark inside the centered 192 × 192 safe area.

Render the PNG on macOS with:

```sh
swift tools/branding/render-plugin-icon.swift assets/branding/plugin-icon.svg src/package/metadata/Icon256x256.png
```

Each action has a monochrome black SVG in `src/package/actionsymbols/`, named after its full C# class name. This is Logitech's required format for recolorable picker symbols. Matching `.ict` button templates embed the same SVG geometry with explicit white contours and default to a blue background with white outlines. The embedded button SVG is white before host tinting; the required monochrome picker SVG stays black. Users can recolor them through Logitech's icon editor. User overrides take precedence; use **Reset icon to default** to see changed packaged defaults.

`./repo icons` requires an outlined SVG and matching template for every action. The full quality gate includes this check. Keep embedded SVG bytes synchronized when changing a symbol.

The earlier shaded raster ghost has been replaced by this vector design.

Picker symbols encode their outline thickness as filled compound path contours, as installed Logitech symbols do. The interiors and background remain transparent. This avoids relying on inherited SVG stroke attributes during host recoloring.

Settings action artwork comes from Tabler Icons (`file-settings` and `refresh`), under the MIT license. Original sources and pinned upstream revision are in `assets/icons/tabler/`; the distributed license is in `src/package/THIRD-PARTY-NOTICES.txt`. Prefer this library for future action icons. Default button glyphs occupy a centered 60% area for compact placement.
