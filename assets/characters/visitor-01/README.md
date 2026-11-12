# Visitor 01 — Round 2C asset preparation

Round 2F.1: `seated-rear-transparent.png` is a separate 1024 × 1536 **RGBA** repair
of the original opaque `seated-rear.png.png`, which remains untouched. Local
segmentation/edge matting passed technical checks and is integrated only into the
development preview. [Repair notes and comparisons](review/round2f1/REPAIR.md)
describe the processing and validation. [Current placement notes](SEATED-PLACEMENT.md)
retain hip/contact anchor (512, 1015) and scene position (842, 812). Final visual
approval is pending. `manifest.json` retains the original in `seatedSourceArtwork`
and describes the corrected output in `seatedArtwork`. The extraction notes below
are historical Round 2C notes; missing animation sequences remain unavailable.

Historical Round 2E: [seated placement notes](SEATED-PLACEMENT.md) and
`seated-placement.json` describe a geometry-only development study with a small
foreground bench overlay. Actual seated artwork remains unavailable. The
approved standing reference is now (950, 890), 80%, 264 px visible height;
earlier staging estimates below are historical extraction notes.

Round 2D update: these unchanged PNGs are now loaded only in the optional
`?preview=character` placement study. See the project README for controls and
copyable settings. The assessment below describes their Round 2C extraction;
the normal scene still loads no character layer and there is no animation.

Anonymous male silhouette, nearly black with subdued charcoal shading. The
approved reference's proportions and tousled hair are retained wherever the
source supplies dark character pixels. No face details, outlines, missing limbs,
or new artwork were drawn. These assets are excluded from the normal experience.

## Deliverables and readiness

- `poses/`: four standing views and eight right-facing walking stills, transparent
  PNGs at their source resolution. These are usable **isolated still assets**,
  not a production-ready animated set.
- `manifest.json`: exact sizes, source rectangles, anchors, hashes, and blocked poses.
- `review/contact-sheet.png`: labeled checkerboard review, not a sprite sheet.
- `review/validation.json`: pixel, alpha-border, source-preservation checks.
- `review/preserved-files.sha256.json`: hashes taken before preparation.
- `tools/ExtractPoses.cs`: reproducible extraction and validation using built-in
  Windows System.Drawing; no package installation or app dependency.

## Reference assessment and extraction method

The untouched master is `../benchzen-character-round2c/character-master-reference.png`,
1774 × 887 RGBA. Its alpha histogram is 616,254 fully transparent pixels,
957,011 partially transparent pixels, and 273 fully opaque pixels. A PNG alpha
channel alone does not make this a clean sprite sheet: text, grid lines, warm
halos and benches also have nonzero alpha.

There is no uniform cell grid. The four large reference views occupy roughly
x=10–530, y=108–587; the eight walking views occupy x=729–1714, y=10–178.
Source regions in the manifest are `[x, y, width, height]`, with exclusive right
and bottom edges. They avoid labels and the divider at approximately y=181.

Within each manually chosen region, keep source pixels with alpha >= 200,
maximum RGB channel <= 55, and RGB channel spread <= 25. Retain only the largest
8-connected component. Copy its original RGBA values into a tightly bounded PNG
with four transparent pixels of padding. No recoloring, alpha normalization,
resampling, inpainting, mirroring, or reconstruction occurs.

This conservative mask removes bright warm outlines and detached contamination.
It also trims light hair tips and shoe highlights; edges can look slightly hard
or uneven at native zoom, especially on the large standing shoes. It does not
recover the exact original contour beneath those highlights. Shading and slight
source translucency remain. Keep these limits visible during art review rather
than treating extraction as final rig cleanup.

Walking figures are only 157–158 pixels tall after masking, versus 456–458 for
the large standing views. Several walk drawings look similar and their limb
positions/proportions are not a verified gait cycle. Enlarging them to match the
standing views will soften detail. No animation timing or foot-lock is approved.

## Seated and missing poses

**No seated idle, sitting transition, or head-rub PNG is supplied.** All four
idle/look views and all six head-rub views contain benches across the lower
torso, hips, arms/hands or legs. Deleting the benches would leave gaps. Visible
heads and shoulders alone cannot serve as complete seated poses. All six
head-rub frames remain blocked, including apparently neutral start/end frames.
The six sit-down frames also cannot form a clean transition sequence from this
reference. Their frame references and reasons are recorded in the manifest.

The six approach/turn drawings were not prepared in this pass; the large rear
standing view supplies the reliable rear still. A left-facing walking sequence
was not supplied or synthesized by mirroring.

## Paris bench alignment (preparation only)

The actual empty-bench asset is **`../../images/paris/paris-clean.png.png`**,
1672 × 941. There is no file named exactly `paris-clean.png`. The filename is
preserved. `paris-approved.png` has the same dimensions but already contains a
seated visitor. The live HTML still references that approved image; do not overlay
another visitor on it. Neither background nor the scene code was changed.

Coordinates below are manual visual estimates in the clean image's original
pixel space, top-left origin, +x right, +y down (allow roughly ±8 px):

| Feature | Image-space estimate |
| --- | --- |
| Entire bench bounds | x=645–1037, y=735–891 |
| Backrest top at center | (842, 738) |
| Seat contact / future hip anchor | (842, 812) |
| Ground baseline | y=890 |
| Future seated feet center | (842, 885) |
| Suggested standing arrival beside bench | (1060, 890) |

Each exported pose has a local `groundAnchor: [x,y]` in PNG pixel-edge
coordinates. Its y is the bottom edge of the lowest retained character pixel,
before the four-pixel padding; x is a manually estimated projection of the body
axis, not the crop midpoint or a measured center of gravity. These are still
placement anchors, not validated per-foot animation contacts. A future seated
pose needs a distinct hip/seat anchor; none can honestly be assigned here.

Start a standing scale review at about **330 px visible body height** in the
1672 × 941 background (adjust within roughly 320–340 px after art review). This
is a staging estimate based on bench proportions, not a calibrated human size.
For a pose whose retained bounds have height `H`, use `s = 330 / H`; e.g. the rear
standing still has H=458, giving s≈0.721. A walking still with H=157 needs s≈2.102,
which exposes its lower source resolution.

For desired image-space ground anchor `(X,Y)` and PNG local anchor `(ax,ay)`:

```text
left = X - s * ax
top  = Y - s * ay
width  = pngWidth * s
height = pngHeight * s
```

All future poses belong inside the existing `[data-layer="character"]` within
`.scene`. Convert image coordinates using `k = displayedSceneWidth / 1672`;
multiply left, top, width, and height by k. The scene uses the background's natural
aspect ratio and contains it without cropping. Measure relative to `.scene`,
not the viewport, so letterboxing cannot shift anchors. Do not size each crop to
the same width: pose widths vary.

A full seated visitor must be occluded by the existing backrest/slats where
appropriate, with legs visible below. Because the clean bench is baked into the
background, a future exact bench occlusion mask or matching foreground cutout is
needed. The reserved empty bench layer does not provide one. No new bench or
occlusion layer was made in this pass.

## Checks and reproduction

On Windows PowerShell, from the BenchZen root, these commands use only the
installed .NET drawing assembly (no Python, npm, or build step for the website):

```powershell
Add-Type -AssemblyName System.Drawing
Add-Type -Path assets/characters/visitor-01/tools/ExtractPoses.cs -ReferencedAssemblies System.Drawing
$poses = [BenchZenPoseExtraction]::Run($PWD.Path)
[BenchZenPoseExtraction]::Validate($PWD.Path, $poses)
```

This rewrites only the 12 derived PNGs and the review contact sheet. Source
regions are explicit in the tool. The committed-to-disk manifest is documentation,
not a runtime dependency. Run pixel checks and re-review the contact sheet if the
algorithm changes; update manifest records if output dimensions change.

Next: review these stills at intended scene scale, then obtain a consistent,
bench-free seated/transition/head-rub pose set with complete obscured anatomy.
Match the same hair, proportions and dark style. Approve scale and occlusion
before any animation work.
