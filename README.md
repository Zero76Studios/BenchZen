# Bench Zen
A Zero76 Studios experiment.

**Somewhere beautiful. Nothing to do.**

## Preview locally

Open `index.html` directly in a modern desktop or mobile browser. No install,
dependencies, build step, or network connection is needed.

For a local server, if Python is already installed, run from the BenchZen folder:

```sh
python -m http.server 8000 --bind 127.0.0.1
```

Then open <http://localhost:8000>. Stop the server with Ctrl+C.

## Character placement preview (Rounds 2D–2G)

From the BenchZen folder, a dependency-free local server is also available using
an existing Node installation:

```sh
node tools/serve.cjs
```

Open **<http://localhost:8000/?preview=character>**. The server binds only to
127.0.0.1. Stop it with Ctrl+C. The Python server above also works. Character
preview needs HTTP because it loads a JavaScript module and the asset manifest;
opening the preview through `file://` is not supported. The normal experience can
still be opened directly as before.

Only the exact `?preview=character` flag loads the development panel, preview CSS,
character module and manifest. Without it, the original approved background and
minimal interface remain in use. This flag is a local development convenience,
not authentication or an access restriction.

The preview uses `assets/images/paris/paris-clean.png`, a byte-identical copy of
the originally supplied `paris-clean.png.png`. Both originals, the master character
reference, and all extracted PNGs are unchanged. No artwork was regenerated.

- Starts with **Seated character - transparent**, using the separately repaired
  `assets/characters/visitor-01/seated-rear-transparent.png` (1024 × 1536 RGBA).
  The original `seated-rear.png.png` remains untouched. Local mask/edge matting
  removed its baked-in checkerboard; no replacement character was generated.
  Scene hip anchor is (842, 812), local PNG anchor (512, 1015), with 100% meaning
  native scale 0.17. The original bench overlay keeps the character behind its
  slats and front rail. Toggle it for comparison; the geometric guide remains available.
- Switch to **Standing / walking stills** for the approved `standing-rear`
  reference at ground (950, 890), scale 80%, 264 px visible height. All four
  standing views and eight right-facing walking stills remain available.
- Adjust scale and horizontal/vertical anchor position with sliders or numeric
  fields. Guide, seated-character and standing studies retain independent settings. Reset affects only the active
  study. Standing scale is a percentage of 330 px visible height; seated scale
  uses separate baselines for the geometric guide and the repaired character image.
- Collapse the development panel for an unobstructed view. It scrolls on small
  screens. The existing fullscreen button includes both the scene and panel.
- **Show seat alignment guides** toggles the measured seat band, clothing contour,
  PNG bounds, backrest and foreground mask regions. The baseline (842, 812),
  100% is unapproved; no correction is applied.
- **Copy settings** exports schema version 5 JSON containing standing, geometric
  guide and seated-character settings, transparency status, anchors, dimensions and occlusion metadata. If clipboard
  access fails, the JSON is expanded and selected for manual copying. Settings
  can also be selected from the JSON section at any time. Refresh resets them;
  there is no settings import or persistence in this pass.

Character bounds are computed as percentages of `.scene` from the manifest's
ground anchors. The scene keeps the background's aspect ratio, so resizing,
rotation and fullscreen preserve placement, including letterboxing. The PNGs
have no added filters, outlines, shadows or effects; only display scaling occurs.

Limitations: walking options are static, lower-resolution reference extractions,
not an approved gait cycle. Conservative extraction trimmed some light contour
pixels in those original standing/walking extractions. The repaired seated image
passed technical alpha/edge checks. The shoes project about 9 px above the bench
leg contact reference, possibly a different ground depth. That alone does not
prove hovering. Contact and scale still need visual review. The geometric guide uses circles
and rectangles only; the small bench overlay covers central occupancy, not the
whole bench. The standing study has no bench occlusion. No final visual approval is claimed.
There is no animation, sound, or new runtime dependency.

See [current seated placement notes](assets/characters/visitor-01/SEATED-PLACEMENT.md)
for bench geometry, mask coordinates, proposed anchors and missing artwork.
See [transparency repair and diagnostics](assets/characters/visitor-01/review/round2f1/REPAIR.md)
for the local processing method, original/processed light/dark comparison and checks.
See [Round 2G alignment diagnosis](assets/characters/visitor-01/review/round2g/DIAGNOSIS.md)
for seat geometry, why Y=800 worsens separation, and annotated closeups.

## Approved artwork

Add the approved Eiffel Tower at dusk background at exactly:

```text
assets/images/paris/paris-approved.png
```

The approved asset is present. If it is removed or cannot be decoded, the scene
displays a quiet text placeholder with placement instructions. Refresh after
replacing the file. The empty-bench alternative is stored as
`assets/images/paris/paris-clean.png.png`; its original filename is preserved.

The full-viewport experience fits the complete image at its natural aspect ratio,
without stretching or cropping. Letterboxing is expected, especially on portrait
phones; rotating to landscape gives the artwork more room. No orientation lock
is required. Branding and controls respect mobile safe areas.

## Foundation

- `index.html`: scene structure, discreet identity, placeholder, and fullscreen control.
- `styles.css`: responsive layout and static presentation with reduced-motion support.
- `script.js`: image loading, natural aspect ratio, and the browser Fullscreen API.
- `assets/images/paris/`: approved background placement and asset notes.

The scene contains separate layers, ordered back to front: background, bench,
character, atmosphere, foreground. Outside preview mode, only the background loads content. Future
layers should use the same canvas dimensions as the approved image; keep them
inside `.scene` so they share its bounds. The interface sits above the scene.

Fullscreen appears only on browsers that support it. Use the button again or
the browser's exit action (usually Escape on desktop) to leave fullscreen. If a
request fails, a status message appears and the normal view remains usable.

This foundation includes no character animation, audio, weather, purchases, or
location switching. The concept below describes future possibilities.

## Concept
A peaceful, animated landscape experience featuring
famous landmarks and small silhouetted characters
sitting on a bench.

## Visual Style
- Landscape orientation
- Realistic scenery with graphic-novel illustration
- Crisp black silhouettes for people and animals
- Bench and character small, in lower part of scene
- Subtle natural environmental animations
- Layered ambient audio

## Characters

Round 2C asset preparation is documented in
[Visitor 01 asset notes](assets/characters/visitor-01/README.md), with a
[pose manifest](assets/characters/visitor-01/manifest.json) and
[checkerboard contact sheet](assets/characters/visitor-01/review/contact-sheet.png).
Four standing views and eight walking stills are isolated; seated idle,
sit-down and head-rub sequences remain blocked by bench occlusion. These files
are loaded only by the development character preview. No character animation is implemented.

- Man
- Woman
- Child

Only one main character at a time.

Characters walk into the scene, pause at the bench,
and sit down.

When changing characters, the current character
stands and walks away before the next arrives.

## First Location
Eiffel Tower, Paris, France.

## Development
- Web-first prototype
- Lightweight 2D animation
- No accounts or payments for prototype
- Future iOS and Android versions possible

## Guiding Principle
Powerful under the hood.
Simple on the surface.
Personality everywhere.
