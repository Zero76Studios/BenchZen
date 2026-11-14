# Bench Zen
A Zero76 Studios experiment.

**Somewhere beautiful. Nothing to do.**

## Living scene (Paris)

The normal experience (no query flag) now runs a quiet, endless visitor cycle in
`living-scene.js`, on the empty-bench artwork `assets/images/paris/paris-clean.png`:

1. The visitor walks in from the left along the gravel just beyond the bench.
2. They stop at the bench, turn to face the tower, and sit.
3. They rest for 75-120 seconds with slow breathing, an occasional small head
   movement and an occasional change of posture.
4. They stand, turn, and walk out to the right. The bench stays empty for
   12-20 seconds, then the next visit begins.

Ambient movement is limited to what the artwork already shows: the lanterns and
low sun glow gently, a few points of light twinkle on the tower, and two small
birds cross the sky every couple of minutes.

How it is put together (no new artwork, no dependencies, no build step):

- Character PNGs are shown as supplied: scaled only, never redrawn or mirrored.
  Seated placement is the documented baseline, hip (842, 812) at native scale 0.17.
- The bench in front of the visitor is copied at runtime from the background's
  own pixels (all three slats, both posts, the seat rail and the legs), so the
  visitor passes and sits behind the real bench. The seat board is a separate
  piece: in front of a walker, underneath a sitter.
- The seated PNG is layered three times with clip regions (legs, torso, head) so
  the head and upper body can move slightly while the legs stay still.
- Only transforms and opacity animate. Positions are percentages of `.scene`, so
  desktop, phone portrait/landscape, resize and fullscreen all stay aligned.
- With "reduce motion" enabled the visitor is simply seated and nothing moves.
- If any living-scene artwork fails to load, the page falls back to the original
  still image `paris-approved.png`.
- `?pace=fast` shortens the rest and the empty-bench pause for testing.
- `?preview=character` is unchanged and does not start the living scene.

Known artwork limits (the scene works around these; new art would improve them):

- **Sitting down / standing up:** the sit-down drawings in the master sheet all
  have a bench painted over the body, so they cannot be used. The transition is
  a 1.5 second blend between the rear standing pose and the seated artwork.
- **Clothing:** the standing and walking poses wear a T-shirt; the seated artwork
  wears a hooded top. The change is visible during the blend.
- **Walk cycle:** eight low-resolution frames (about 157 px tall, enlarged about
  1.7x on a full-size desktop scene) that are not an even gait; frames 5-8 are
  all wide strides. Only a right-facing walk exists, so the visitor always
  enters from the left and leaves to the right.
- **Turning:** the approach-and-turn frames were never extracted, so the turn is
  a cut between the right-facing and rear standing stills.
- **Head turns:** there are no bench-free look-around or head-rub frames. The
  head movement is a small tilt of the existing seated head, not a real turn.

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

There is no audio, weather, purchases, or
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
are used by the living scene and the development character preview.

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
