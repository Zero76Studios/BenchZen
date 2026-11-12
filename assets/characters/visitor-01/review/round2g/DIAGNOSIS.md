# Round 2G — seat alignment diagnosis

**No seating correction or visual approval is applied.** Reloading the preview
restores X=842, Y=812, scale=100% for diagnosis. Neither Y=812 nor the rejected
Y=800 is approved.

Open `http://localhost:8000/?preview=character` after `node tools/serve.cjs`.
**Show seat alignment guides** is initially enabled for the seated character;
it can be toggled independently of the original bench occlusion checkbox. All
standing/walking and geometry-study controls remain. Normal mode is unchanged.
Copy settings now uses schema version 5, including diagnostic measurements.

## Actual bench and character measurements

Coordinates use the 1672 × 941 background pixel space, +y down. Board limits are
visual estimates with roughly ±2 px uncertainty, not calibrated 3D geometry.

| Feature | Measured / estimated position |
| --- | --- |
| Three-slat backrest | x≈645–1037, y≈737–800 |
| Warm horizontal upper seat surface | x≈670–1013, **y≈806–816** |
| Dark front seat rail | **y≈816–824** |
| Original foreground overlay box | x=690–1000, y=737–824, transparent between four copied strips |
| Full character PNG bounds at baseline | x=754.96, y=639.45, width=174.08, height=261.12 |
| Independently scanned central lower clothing edge | local pixel-edge **(512, 1015)**, scene **(842, 812)** |
| Neighboring central lower clothing contour | local y=1009–1018; scene **y=810.98–812.51** |
| Lowest shoe projection | y≈881.19 |

The clothing contour was rescanned from actual PNG alpha >=128, in local y=950–1080
across x=370–650. It was not inferred from the old pivot. The source crop shows a
visible hem/underside between the legs, **not the anatomical hip joint or hidden
loaded support patch**. The old (512, 1015) pivot matches that central image edge;
it is still only a provisional image-placement pivot.

## Why Y=800 was worse

```text
imageTop = pivotY - 1015 * 0.17
observedContourY = pivotY + (independentlyMeasuredLocalY - 1015) * 0.17
```

Reducing Y from 812 to 800 raises the entire image **12 px**. The central clothing
edge then projects to y=800, **6 px above** the rear boundary of the seat region
at y=806. Neighboring contour is around y=799–801. That is a genuine projected
separation from the board. This rejected comparison is a diagnostic image only,
not the saved/runtime default.

## What reads as a gap at Y=812

At baseline the measured clothing underside is **inside** the y=806–816 seat
surface band. There is no positive central clothing-to-seat-band air gap in these
measurements. This does not prove the pose looks naturally seated; it separates
geometric contact from the perceptual depth/contact concern.

About **4 px of exposed seat surface** remain between the central hem at y=812
and the dark front rail beginning at y=816. The warm board strip can read as
separation when support depth is unclear, but those pixels belong to the seat,
not empty scenery. The front rail then passes across the upper legs. Mask-on/off
closeups retain the same contour and seat strip: masking changes occlusion, not
character position. No contact shadow or new effect is added to conceal this.

The earlier 9 px foot comparison also needs qualification: y≈890 was measured at
**bench-leg** ground contacts. It is not a calibrated contact line for the visitor
at a potentially different depth. Shoes at y≈881 therefore do not independently
prove that the backside hovers above the seat.

## Cause assessment

| Candidate | Finding |
| --- | --- |
| Position | Confirmed cause of the worse Y=800 trial. At Y=812, the central contour projects into the board region. |
| Anchor | A real clothing-edge coordinate, but not a verified physical support anchor. No crop/scale arithmetic error found. |
| Proportions / perspective | Scale 0.17 and artwork remain unchanged. Depth cues may differ, but physical incompatibility has not been established from these images. |
| Foreground mask | Correctly fixed to existing slats and front rail. It does not cover the central contour at y=812; front strip starts at y=816. It can affect the impression but does not generate a central geometric gap. |

## Can position alone fix it? Recommendation awaiting approval

Downward translation fixes the **confirmed** Y=800 separation; that trial is
restored to the requested baseline. Another translation alone cannot yet be
promised to resolve the Y=812 seating impression, and the data do not establish
that the supplied pose cannot physically fit.

Inspect the baseline with guides off and mask toggled first. If deeper overlap
is desired, the next proposed comparison would be a small **downward trial toward
Y=814–816**, keeping the same artwork and scale. The measured hem would remain
within the board/front-edge region. **That trial has not been applied or rendered
as a new candidate.** No anchor, proportions, mask or artwork correction is made.
Do not move upward to Y=800 or stretch the pose to force a fit.

If natural contact still cannot read within the board band, obtain an
artist-defined weight-bearing contact region / matching bench perspective before
changing the anchor or pose. Do not force shoes and hem onto unrelated screen
lines. Wait for visual approval before attempting a correction.

## Artifacts and verification

- `baseline-alignment-guides.png`: close-up with PNG bounds, backrest, seat band,
  measured contour and existing foreground mask regions.
- `seat-contact-closeups.png`: baseline mask on/off and explicitly rejected Y=800.
- `source-contact-inspection.png`: independently measured source hem.
- `bench-geometry-inspection.png`: original bench enlarged with coordinate lines.
- `measurements.json`: independently scanned source contour and projected gaps.
- `preserved-files.sha256.json`: pre-change PNG and normal-mode code hashes.
- `validation.json`: syntax, controllers, toggles, projection, preservation and HTTP.

Colors: cyan seat band; red clothing contact estimate; amber whole PNG bounds
(including transparent margins); purple mask box/opaque bands; white backrest.
These are temporary development guides, not character artwork.

`tools/SeatAlignmentDiagnostic.cs` reproduces the source scan and three diagnostic
composites with built-in System.Drawing. It writes only Round 2G review files.
The viewer reads `../../seat-alignment.json` for independent point measurements.
No new framework, dependency, animation, commit, push or deployment is involved.
