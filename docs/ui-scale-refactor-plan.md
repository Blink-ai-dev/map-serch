# UI Global Scale Refactor Plan

## Goals
- Single source of truth for UI scale (window size + UI elements).
- Prevent partial scaling (e.g., list width only) and ensure proportional resize across window, panels, lists, overlays, tooltips, and map UI.
- Minimize regressions by isolating changes and adding guarded fallbacks.

## Current Findings (source review)
- `App.xaml.cs` holds scaling logic (`ApplyGlobalScale`) using AppWindow resize; base size cached once but shared with UI size changes spread across multiple view-specific resources.
- `MapPage.xaml`: UI scale buttons call `MapViewModel` commands, which call `App.ApplyGlobalScale`. Layout itself no longer uses Scale, but size resources are still scattered.
- `SettingsViewModel`: manages InfoOverlay/Tooltip/DetailPanel sizes via Preferences, but map tooltip (HTML) still uses hardcoded sizes/colors; no central scale resource for general UI.
- `map.html`: tooltip, markers, circle styles are hardcoded; no scale parameter from .NET.
- List cards: fixed paddings/heights; no scale-aware resources.
- Null-guards in `ApplyGlobalScale` added; AppWindow used on Windows, fallback to Window size assignment.

## Scope & Impact Areas
- **Scaling state**: App.xaml.cs, new shared service, Preferences keys.
- **UI resources**: central dictionary for padding/font/icon sizes and panel widths.
- **Views**: MapPage, SettingsPage, Detail Panel, Info Overlay, CollectionView item templates.
- **Interop**: map.html tooltip size/highlight size; IMapService/MapService to pass scale.
- **Platform**: Windows AppWindow resize; ensure guards when AppWindow unavailable.

## Risks (functional regressions to avoid)
- Window resize not applied (AppWindow null) → ensure fallback and logging.
- Map tooltip/circle/marker highlight not scaling → update JS and interop calls.
- List item layouts clipping at small scales → use minimum sizes + resource-based paddings.
- Detail panel/Info overlay/Buttons overflow at large scales → cap max scale and resource clamps.
- Preferences compatibility: new scale settings should default safely.

## Plan (phased)
### Phase 1: Centralize Scale State
- Add `ScaleService` (singleton):
  - Properties: `CurrentScale`, `BaseWidth/Height` (cached once), `MinScale=0.9`, `MaxScale=1.3` (tunable).
  - Methods: `SetScale(step)`, `Reset()`, `ApplyWindowSize()`.
  - Events: `ScaleChanged` for UI resources update.
- Move `ApplyGlobalScale` logic from `App.xaml.cs` into `ScaleService`; App only initializes service and wires logging.

### Phase 2: Resource-Driven UI Sizes
- Create `Resources/Styles/UiScale.xaml` with dynamic resources:
  - `CardPadding`, `IconSize`, `HeadingSize`, `BodySize`, `PanelWidth`, `OverlayPadding`, `TooltipFontSize`, etc.
- On `ScaleChanged`, update these resources (clamped) so all views inherit without per-view logic.

### Phase 3: Wire Views to Resources
- MapPage.xaml:
  - List item padding/height from resources (no hardcoded numbers).
  - InfoOverlay/DetailPanel widths, paddings from resources.
  - +/- buttons call `ScaleService` (through VM), remove residual per-view scaling.
- SettingsPage.xaml:
  - Remove any legacy font-size controls; keep size options mapping to resource presets if still needed (info overlay / tooltip / detail panel) or align with global scale if business logic allows.

### Phase 4: Map Interop
- Extend `IMapService.ApplyMapSettingsAsync` to accept `uiScale` (or derived sizes).
- In `map.html`, apply scale to:
  - Tooltip padding/font/min-width.
  - Marker highlight size multiplier.
  - Circle stroke width/opacity if needed.
- Ensure defaults are used when scale not provided (backward compatible).

### Phase 5: Safeguards & Fallbacks
- Clamp scale every change; ignore redundant updates.
- If AppWindow unavailable, log and skip resize (avoid crashes).
- Maintain minimum sizes for list items/panels to avoid clipping at small scales.

### Phase 6: Testing
- Manual matrix: scale 0.9 / 1.0 / 1.1 / 1.3
  - Window size changes proportionally.
  - Left panel/list widths scale proportionally; no text clipping.
  - Info overlay size scales; counts readable.
  - Detail panel width and content readable at all scales.
  - Map tooltip & marker highlight scale accordingly.
- Regression checks: navigation between tabs, detail panel open/close, import/export flows untouched.

## Files to Touch (expected)
- New: `Services/ScaleService.cs`, `Resources/Styles/UiScale.xaml`.
- Update: `App.xaml.cs`, `App.xaml`, `MauiProgram.cs` (DI registration), `MapViewModel.cs` (use service), `SettingsViewModel.cs` (if keeping local sizes, align with service or simplify), `MapPage.xaml`, `SettingsPage.xaml`, `map.html`, `IMapService.cs`, `MapService.cs`.

## Rollback Strategy
- Keep old scaling code behind a feature flag in `ScaleService` (fallback to AppWindow resize only).
- If JS scaling causes issues, guard by defaulting to scale 1.0 until stabilized.


