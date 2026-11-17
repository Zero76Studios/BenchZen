# Visitor-02 Round 6 — seat and foot alignment

**103% does not resolve the floating boots. With the intended contact held at (842,812), each boot remains 8.16 scene pixels above the documented ground line, versus 9.93 pixels at 100%. Do not lower the whole body to hide this discrepancy.** No placement or art approval is implied.

Verified branch `feature/paris-living-scene`, commit `ea24f84`. Round 3–5 files were already uncommitted and remain untouched. Read the art specification, Round 5 measurements, registration generator, preview HTML and display-transform code. New files are confined to this Round 6 diagnostic directory.

## Coordinate chain and fixed geometry

All results below use the existing **1672 × 941 scene coordinate system**, origin top-left, y down. Scale percentages mean the Round 5 preview's 100% and 103%, not native image size.

- Source candidate: `source/visitor-02-seated-transparent.png`, 1048 × 1501.
- Round 5 registration: uniform source-to-frame scale `0.3172413793103448`.
- Source clothing-edge anchor `(524,1046)` maps to frame `(256,536)`, then scene `(842,812)` at both display scales.
- Display source-to-scene scale: `0.3172413793103448 × 280/600 × percentage/100`, or `0.1480459770` at 100% and `0.1524873563` at 103%.
- Bench seat/contact target **y812**; standing/seated ground target **y881**; required projected clearance **69 scene px**.
- Existing bench seat board spans y800–814; foreground rail y814–824. These are distinct drawn surfaces, not a calibration proving an anatomical support plane.
- The spec's 148 canvas-pixel clearance maps to 69.067 scene pixels at 280/600. Its 0.067-pixel rounding difference cannot account for the observed gap.

For any source point `(u,v)`, the analytical prediction is `sceneX = 842 + (u−524) × scale`, `sceneY = 812 + (v−1046) × scale`. The diagnostic renders the existing Round 5 registered PNG with the existing display transform; it does not regenerate or alter the character.

## Comparison table

Units are scene pixels. L/R mean screen-left/right. Reported coordinates use two decimal places for comparison, not a claim of subpixel anatomical accuracy.

| Quantity | 100% | 103% | Evidence type |
| --- | ---: | ---: | --- |
| Head/hair top y | 658.48 | 653.87 | Alpha contour + transform |
| Head/hair width | 38.64 | 39.80 | Alpha envelope within head ROI |
| Head/hair height | ~42.49 ±1.18 | ~43.76 ±1.22 | Estimated hair base; not anatomical skull height |
| Upper shoulder envelope width | 61.14 | 62.98 | Alpha envelope at source y398–402; not joint breadth |
| Intended hip/contact anchor | (842,812) | (842,812) | Fixed placement input |
| Seat target y | 812 | 812 | Documented geometry |
| Left boot sole representative point | (812.09,871.07) | (811.20,872.84) | Alpha sole edge + transform |
| Right boot sole representative point | (870.50,871.07) | (871.35,872.84) | Alpha sole edge + transform |
| Left sole → y881 gap | **9.93** | **8.16** | Derived difference |
| Right sole → y881 gap | **9.93** | **8.16** | Derived difference |
| Contact → sole vertical reach | 59.07 | 60.84 | Derived difference |

The 3% increase adds only **1.77 px** to the contact-to-sole reach, raises the head top **4.61 px**, widens the head **1.16 px**, and widens the measured shoulder envelope **1.83 px**. It changes the upper body that already looks seated while leaving most of the gap.

## Measurements, estimates and limitations

**Measured pixel evidence:** alpha threshold >=128, using source ROIs: head x350–699/y0–299; shoulder x200–849/y398–402; left boot x150–419/y1300–1489; right boot x620–879/y1300–1489. Both sole bottom boundaries are source **y1445**, defined as the lowest included pixel plus one. Sole x is the midpoint of the occupied range in the bottom four pixel rows, not a foot-joint location or centre of pressure. Thresholds 32/128 give y1445; threshold 224 gives y1444, a 0.15–0.16 scene-pixel change. This is much smaller than either gap.

The existing registered frame's left and right boot bottom boundaries both measure **y663**, versus the source-derived prediction y662.5793. The difference is about **0.20 scene px** at 100% and **0.20 px** at 103%; resampling does not explain an 8–10 px discrepancy. Final screen rasterisation and anti-aliasing can add subpixel differences. The quoted gaps are analytical source-contour predictions.

**Visual estimate:** hair base source y296 ±8, used with measured alpha top y9 for approximate head-with-hair height. The Round 5 head ROI ended at y299; its 291-pixel bounding height was ROI-limited and should not be called a measured anatomical head height. The shoulder stripe is reproducible clothing-outline evidence, not an anatomical shoulder width. No skeleton or bone lengths were inferred.

**Placement assumption:** source `(524,1046)` is the bottom of the central visible jeans contour. Checking x500–540 at five-pixel intervals finds the same y1046 boundary throughout. It is a stable image landmark, **not verified anatomical seat contact**. The seat and foreground rail hide the relevant support region; this single rear projection cannot prove the pelvis support plane. A correct-looking upper body is visual evidence, not proof of anatomical contact.

## What causes the apparent float?

| Possible cause | Finding |
| --- | --- |
| Anchor arithmetic/placement | The source → frame → scene math keeps the specified clothing-edge anchor fixed. No transform error found. Anatomical misidentification remains possible. |
| Incorrect bench/ground geometry | The implementation uses the documented y812/y881 values. No numeric mismatch found. However, these targets do not independently establish where a seated boot lies in the painted ground plane. |
| Character leg proportions or seated pose | At the head-matched scale, the visible clothing-edge-to-sole projection is 59.07 px, rather than the required 69 px. This projected reach is insufficient **if** y881 is correct for the sitter. It does not prove short bones; thigh angle, knee/ankle pose, clothing and foreshortening affect the result. |
| Perspective/depth mismatch | Plausible but unproven. A sitter's feet can lie at a different depth from the walking line and bench legs; in that case their image y need not equal 881. The painting and rear-view character have no calibrated camera/ground-plane data that would distinguish this from a pose discrepancy. |

The established cause is a **mismatch between the chosen seated projection and the prescribed seat-to-ground clearance**. The evidence cannot uniquely assign that mismatch to anatomy, camera perspective, or an unverified support landmark.

Moving the whole sitter down 9.93 px at 100% would move the jeans anchor to y821.93, inside/below the front rail rather than y812. At 103%, closing the 8.16 px gap would put it at y820.16. Neither is a justified fix. Raising the anchor to the rear seat-board edge would worsen the foot gap. Re-anchoring alone cannot change the actual separation of visible clothing underside and boots.

Uniformly scaling until the documented y881 is reached requires about **116.81%** of the Round 5 baseline, not 103%. That would enlarge all upper-body dimensions by 16.81% and raise head top to approximately y632.67. This is a tradeoff calculation, not a recommended placement.

## Smallest justified next action

**Keep the 100% baseline and fixed seat anchor while reviewing the intended seated foot depth.** Use the annotated comparison to confirm the support plane and whether y881 should apply to the sitter's soles. A seated-depth-specific ground target is the smallest potential configuration correction, but only if the scene perspective supports it; do not redefine it merely to erase the gap.

If review confirms that the hips are correctly supported and both seated boots must land at y881, there is **no global position/scale correction that preserves the currently accepted upper-body appearance**. The next art task would need a seated pose with about 9.93 scene px more projected contact-to-sole reach at the existing upper-body scale, preserving head, shoulders and clothing identity. This is not an instruction to stretch the legs; it requires resolving the pose/perspective with the artist. No artwork change was attempted here. 103% may be compared visually but should not be presented as a foot-alignment fix.

## Diagnostic outputs and verification

- [Annotated 100% / 103% comparison](alignment-100-vs-103.png): uniform 2× crops, scene coordinates unchanged.
- [100% full scene](scene-100.png), [103% full scene](scene-103.png), and annotated full-scene variants.
- [Machine-readable measurements](measurements.json), [validation](validation.json), [preservation baseline](preservation-before.json).
- Reproduce in a fresh PowerShell process: `powershell -NoProfile -ExecutionPolicy Bypass -File assets/characters/visitor-02/review/round6/RunDiagnostic.ps1`.

Foreground bench pixels come from the existing Round 5 overlay. Original assets, visitor-01, scenery, existing diagnostics and lifecycle code are unchanged. No animations, transitions, commits, pushes, merges or deployment. This diagnostic does not grant visual approval.
