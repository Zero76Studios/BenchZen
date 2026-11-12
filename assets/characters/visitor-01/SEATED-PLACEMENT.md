# Round 2G seated placement - diagnostic baseline

**Current placement is unapproved.** X=842, Y=812, 100% is restored for diagnosis; Y=800 is rejected. [Round 2G diagnosis and closeups](review/round2g/DIAGNOSIS.md) independently measure the clothing contour and seat surface. The preview has toggleable alignment guides; no new correction is applied.

**Technical transparency repair passed. Final character appearance and placement
are ready for user visual review, not visually approved.**

The original `seated-rear.png.png` is unchanged, 1024 × 1536 opaque RGB with a
baked-in checkerboard. The separate `seated-rear-transparent.png` has genuine
RGBA transparency on the same 1024 × 1536 canvas. It was derived locally by
segmentation and edge matting, not by generating or redrawing the character.
See [repair method, validation and diagnostics](review/round2f1/REPAIR.md).

## Preview

Run `node tools/serve.cjs` from BenchZen, then open
`http://localhost:8000/?preview=character`.

- **Seated character - transparent** is now the default, using the repaired PNG.
- **Seated geometric guide** remains available as a separate geometry study.
- **Standing / walking stills** retains all four standing views and eight walking
  stills. Reset restores the approved standing reference below.

The three studies retain independent scale/x/y settings during the page session.
Reset affects only the active study. Copy settings exports schema version 5 JSON
with all states, anchors, scale and transparency/occlusion status. Clipboard
failure reveals/selects the JSON for manual copying. Refresh restores defaults.
No changes apply outside the development query flag, and no animation exists.

## Coordinates and proportions

All scene coordinates use the original 1672 × 941 clean-background pixel space;
local anchors use the PNG's top-left pixel-edge origin.

| Study | Scene anchor | Local anchor | Initial scale |
| --- | --- | --- | --- |
| Approved standing rear | Ground (950, 890) | (71, 462) in 146 × 466 PNG | 80% of 330 = 264 px visible height |
| Repaired seated character | Hip/contact (842, 812) | (512, 1015) in 1024 × 1536 PNG | 100% = native PNG scale 0.17 |
| Geometric seated guide | Hip (842, 812) | (60, 140) in 120 × 218 guide | 100% of guide size |

The local pivot is at the visible lower clothing contour between the legs.
Source pixel inspection across x=400–620 places that edge around y=1014–1017.
An independent Round 2G alpha scan confirms this coordinate as a clothing edge,
but it is not a verified physical support point or anatomical hip joint. Repair
did not crop, rescale or shift the PNG.

Final alpha bounds are `[185, 58, 646, 1364]`. At native scale 0.17:

- Full PNG size: 174.08 × 261.12 background pixels; visible alpha height ≈231.88 px.
- Image top-left: (754.96, 639.45).
- Hair/head size remains roughly comparable to the approved standing reference.
  The hooded seated shoulders are broader than the standing T-shirt silhouette.
- Lower shoe coverage ends near y=881.2, about 8.8 px above the bench-leg
  contact reference near y=890. These may be different ground depths; that
  difference alone does not prove hovering. No scale/contact correction is applied.

For seated scale control `p`:

```text
nativeScale = 0.17 * p / 100
left   = hipX - 512  * nativeScale
top    = hipY - 1015 * nativeScale
width  = 1024 * nativeScale
height = 1536 * nativeScale
```

Placement becomes percentages of the same `.scene` as the background. Native
aspect ratio and image-space anchors preserve alignment during resizing, mobile
rotation, fullscreen and letterboxing. No nonuniform stretch, outline, shadow,
filter or opacity trick is applied to the repaired image.

## Unchanged bench occlusion

The complete bench is baked into `paris-clean.png`. Approximate bounds are
x=645–1037, y=735–891; center x≈842; backrest top y≈737; seat surface y≈806–816;
front rail y≈816–824; ground y≈890.

The Round 2E `../../images/paris/bench-preview/bench-center-occlusion.png` is unchanged:
310 × 87 transparent pixels, fixed at (690, 737), source rectangle
`[690, 737, 310, 87]`, exclusive right/bottom. It copies only the central slats and
front rail:

| Part | x interval | y interval (exclusive end) |
| --- | --- | --- |
| Top backrest slat | 690–1000 | 737–757 |
| Middle slat | 690–1000 | 761–778 |
| Lower slat | 690–1000 | 783–800 |
| Front seat rail | 690–1000 | 816–824 |

The 19,220 copied pixels match the original background exactly, with 7,750
transparent gap pixels. No second bench, posts or legs are drawn. The repaired
character occupies the character layer, below the foreground overlay. Scenery
and character are now visible through the slat gaps without a checkerboard.

Toggle occlusion to compare. The overlay stays fixed as the character moves or
changes scale, and is absent from the standing study. It covers central occupancy
only, not bench arms or hands positioned in front of the backrest. Off-bench
placement triggers a warning. Fractional-size mask edges and limb/seat contacts
still need browser review before approval.

## Checks and artifacts

`manifest.json` retains the unchanged original asset assessment as
`seatedSourceArtwork` and the repaired file, hashes, alpha counts, bounds and
anchor as `seatedArtwork`. The original 12 standing/walking pose records and
animation-readiness limits are unchanged. `seated-placement.json` records the
current independent defaults and continuing placement-review requirements.

`review/round2f1/` contains extraction metrics, source-preservation hashes,
validation results, original/processed comparisons on light and dark backgrounds,
native-resolution hair/shoe crops and a Paris bench diagnostic composite.
Earlier Round 2E/2F diagnostic files are historical and unchanged.

The repair validates all dark source pixels retained, exact solid-interior RGB,
transparent corners/background, partial alpha at edges, no enclosed transparent
holes, and unchanged original artwork. Standing/walking controls, independent
settings/reset, copy fallback, normal mode and positioning math are checked.

Remaining work is user visual review of the repaired silhouette, head/body scale,
seat contact, feet and occlusion edges. Walking, sit-down transitions, head rubbing
and other animations are still out of scope. No final visual approval is claimed.
