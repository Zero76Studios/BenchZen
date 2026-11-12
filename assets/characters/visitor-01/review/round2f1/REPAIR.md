# Round 2F.1 — local transparency repair

**Technical extraction succeeded; ready for user visual review, not visually
approved.** The original `../../seated-rear.png.png` is untouched. The separate
output is `../../seated-rear-transparent.png`, 1024 × 1536, RGBA.

## Method and feasibility

The supplied RGB source has a strong separation between its nearly black
character and pale checkerboard. Known exterior samples range from RGB 246–255;
most character pixels are below RGB 30. A local mask is reliable for this image,
including the gap between the legs. This is a source-specific process, not a
general background remover.

`tools/RepairSeatedTransparency.cs` uses only built-in Windows System.Drawing:

1. Verify the exact original SHA-256 before processing; stop if it changed.
2. Establish confident pale background and a dark silhouette, then erode the
   binary silhouette by 2 px to identify solid interior. That erosion is used
   only for sampling, never as the final silhouette or a hair-trimming mask.
3. Copy the solid interior's original RGBA values. Across the narrow edge band,
   estimate soft alpha using nearby solid foreground and checkerboard colors.
   Use local foreground color to remove pale background mixed into the original
   antialiased edges. A narrow arm/body gap needs a farther background sample.
4. Stop if edge recomposition error exceeds 8 channel levels. The measured maximum
   is 4.263 / 255. Original flattened alpha cannot be recovered pixel-perfectly;
   edge alpha and edge color are estimates, confined to the matte boundary.
5. Close five isolated 1–3-pixel enclosed contour pinholes (8 pixels total) using
   neighboring alpha/color. This does not fill the open space between the legs,
   replace anatomy, or punch holes into dark fabric. The final mask has no
   enclosed transparent regions.
6. Remove 50 detached near-background speck components (120 pixels total), each
   at most 16 pixels, source max RGB >=240 and alpha <=8/255. The observed specks
   have source RGB maxima 243–245 and no dark character pixels. This removes
   faint checkerboard/edge noise without discarding dark hair or substantial
   silhouette fragments. The final character has one connected component.

There is no generative model, replacement drawing, outfit/hair change, resizing
of the production PNG, nonuniform distortion, or recoloring of the solid body.
Subtle interior charcoal shading is retained. Matting necessarily changes RGB
and alpha on the composited boundary; it does not claim every edge RGB value is
identical to the white-background source.

## Validation

- 524,260 fully opaque pixels, 14,677 partial-alpha pixels, and 1,033,927 fully
  transparent pixels; decoded dimensions 1024 × 1536 with genuine RGBA.
- All four corners and the exterior are transparent. Background between the legs
  remains transparent. No white/checkerboard-colored pixels remain in the cutout.
- All **529,626** dark source pixels (max RGB <= 80) retain nonzero coverage.
  **519,713 solid interior pixels** are copied exactly, including body shading.
- Final alpha bounds: `[185, 58, 646, 1364]`. Same original canvas and pixel origin;
  no crop or anchor shift. No enclosed transparent holes remain.
- Full-body light/dark comparison and native-resolution hair/shoe edge details
  were inspected. The complete visible hair, hood, arms/lap, legs and shoes are
  retained; no obvious checkerboard remnants, halos or jagged/missing parts were
  observed in these diagnostics. Final user approval is still pending.
- The original seated file, all existing PNGs, bench overlay and normal-mode
  HTML/CSS/JS are checked against their pre-change SHA-256 hashes.

See `extraction-metrics.json` and `validation.json` for results,
`comparison-light-dark.png` for original/processed panels on both solid colors,
and `edge-detail-light-dark.png` for full-resolution edge crops. The original's
opaque checkerboard remains visible in its comparison panels by design.

## Preview and remaining placement concern

Open `http://localhost:8000/?preview=character` after `node tools/serve.cjs`.
The default study is now **Seated character - transparent**. Standing/walking
stills and the geometric seated guide remain available with independent settings.

Initial scene hip anchor remains **(842, 812)**, local PNG anchor **(512, 1015)**,
scale setting **100%**, native PNG scale **0.17**. The unchanged central bench
overlay sits above the transparent character; no second bench is displayed.

The derived full image size remains 174.08 × 261.12 background pixels, and the
visible alpha silhouette is approximately 231.88 px tall. The source's hair/head
scale remains roughly comparable to the approved 264 px standing reference.
Shoe coverage ends around y=881.2, about 8.8 px above the ground baseline y≈890.
Scale/contact adjustment is intentionally left for visual inspection; transparency
repair does not imply approval of proportions or bench contact.

`paris-seated-review.png` is a labeled diagnostic crop of the repaired character
behind the unchanged foreground overlay. It is not loaded into the scene.

## Reproduce without touching the original or accepted output

From the BenchZen root in Windows PowerShell:

```powershell
Add-Type -AssemblyName System.Drawing
Add-Type -Path tools/RepairSeatedTransparency.cs -ReferencedAssemblies System.Drawing
[BenchZenSeatedRepair]::Run($PWD.Path)
```

This writes `review/round2f1/candidate.png` plus the comparison diagnostics. It
never overwrites the source or `seated-rear-transparent.png`. The accepted output
was promoted only after extraction checks and diagnostic inspection passed.
No new framework, package, website build step or animation was introduced.
