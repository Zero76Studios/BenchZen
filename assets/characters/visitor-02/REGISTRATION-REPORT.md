# Visitor-02 — Round 2 registration diagnostic

**A complete believable walk cycle and natural sit-down transition are not currently possible from these poses alone.** All eight walk candidates and six selected rear candidates fit a 512 x 768 canvas using uniform scale and translation, but fitting does not make their gait, proportions or anchors compliant.

Baseline remains `feature/paris-living-scene` / `1fbdd6b`. Read against [character-art-spec.md](../../../docs/character-art-spec.md) and [Round 1 audit](ART-AUDIT.md). IDs W01–08 follow the walk labels; R01/R04–08 follow the rear sheet's **top row**.

## Annotated evidence

- [Source bounds and landmarks](review/round2/annotated-source.png)
- [Walk registration sheet](review/round2/registered-walk.png): eight 512 x 768 diagnostic panels
- [Rear registration sheet](review/round2/registered-rear.png): six equivalent panels
- [Measurements / transforms](review/round2/measurements.json), [comparison table](review/round2/comparison.csv)

Cyan = measured bounds; red = visible sole support-band midpoint; green = body-centred ground origin; gold = estimated shirt-hem centre / seated underside and seat guide. White rear crosses mark target feet x=196/316. Each panel has ground y=684, seat y=536 and a 12 px margin guide. Hip alignment is estimated from clothing, not an exposed joint.

## Measurements and limits

Walk: one scale **1.041667**, derived from W03's 576 px head-to-sole height. Rear: one scale **2.238806**, derived from R04's 268 px height. Each pose translates its estimated hip centre to x=256 and lowest supporting sole to y=684. No individual resizing, stretching, morphing or generated anatomy.

Walk isolation uses alpha >=128 connected components; subthreshold fringe pixels are omitted **only in diagnostic panels**, so these are not final sprites. Rear panels retain the original opaque black rectangles. Rear bounds/soles use RGB maximum >=40; central underside estimates use >=8, with thresholds 6/12 checked for sensitivity. No matte was manufactured.

| Comparison, in registered canvas px | Walk | Rear R04–08 |
| --- | --- | --- |
| Head top | 81.9–89.2; W02/W06 lack specified down-pose lowering to y=94 | 84, 137.7, 178.0, 209.4, 213.9 |
| Head envelope width | 87.5–90.6, about 3.6% spread | 98.5–107.5, about 9% spread; 5% consistency unproven |
| Projected shoulder envelope | 74.0–84.4 | 174.6–208.2 |
| Projected neck-to-shirt-hem length | 211.5–213.5 | 223.9–261.9 |
| Approximate hip–knee–ankle polyline | 228.8–258.3 | True limb length not measurable through rear seated occlusion/foreshortening |

Shoulder envelopes include sleeve/arm projection; they are not anatomical shoulder breadth. Clothing landmarks have about +/-4 source px uncertainty, hip centres +/-5, manual joint estimates +/-8–12. The leg polyline is a visible projection estimate, not evidence of changing bone length. Torso/leg differences must not be “fixed” by stretching. Rear head/body consistency needs corrected art or clearer landmarks before acceptance.

## Walk contact findings

Measured source-pixel `(x,y)` support-band points, excluding lifted feet:

| Pose | Visible support points | Assessment |
| --- | --- | --- |
| W01 | (92,618), (260.5,618) | Useful two-contact candidate |
| W02 | (474,618) | Useful loading candidate; swing foot remains behind |
| W03 | (714.5,616) | Useful stance reference; labelled passing foot remains behind hip |
| W04 | (951.5,616) | Swing foot crosses over stance boot; lacks forward reach/push-off |
| W05 | (1155.5,615), (1328.5,617) | Useful opposite-contact candidate; rear toe about 2 px higher |
| W06 | (1461,620) | Rear foot supports while forward foot is lifted; not matching down phase |
| W07 | (1833,615) | Trailing-foot pose rather than the required passing phase |
| W08 | (2039,620) | Boots overlap; lacks forward reach/push-off |

These are visible edge estimates, not force measurements or foot centres. Approximate heel-to-toe boot-centre offsets for W01's forward foot through W04 are **+131, +37, +8, -20** canvas px (target +126,+63,0,-63). Backward increments are about 94,29,28, versus 63 +/-15. Boot/hip estimates each carry +/-5 source px uncertainty; this still does not support the required cadence. W05→W06 also switches support without the expected intervening transfer. Reordering alone does not supply missing forward-reaching phases or establish opposite-leg identity.

## Rear sitting findings

R04→R08 sole-contact separation grows **86→91→105→135→167 px**. Translation cannot remove that widening. Target foot-centre separation is 120 px; contact points differ from centres, but the observed stance drift is substantial.

Estimated underside y values: **R06 509, R07 574, R08 581**, versus seat y=536. Threshold sensitivity gives R07 roughly 547–577 and R08 570–581: R08 remains at least 34 px too low. Moving it upward would lift the feet; scaling it to the seat clearance would change character scale. R01 is a plausible turn reference but is about 631 px tall at the common rear scale, requiring separate review.

## Disposition and smallest next step

**Retain as references/candidates:** W01/W05 contacts; W02/W03 stance/loading; R01 turn; R04 standing; R05/R06 early descent. None is production-approved: opaque mattes, warm lighting and unresolved proportions remain.

**Correct:** W04/W06/W07/W08 gait content; rear stance/seat geometry and head/body consistency. **Missing usable content:** forward-reaching push-off phases for both steps, a foot-locked rear first-contact/settling sequence, and optional matching seated head turns. The separately drawn stand-up row is not an exact reverse and should not fill these gaps.

Smallest practical next step: obtain **one corrected transparent rear seated-rest endpoint**, preserving identity and standing-derived scale, with soles y=684 and underside y=536. Validate it against R04 before requesting descent in-betweens. Walking then needs targeted corrected phases; runtime integration is premature.

Focused checks passed: five source hashes unchanged, 14 panels within 12 px margins, ground origins at y=684, tracked files unchanged. [Validation](review/round2/validation.json). Reproduce with `powershell -NoProfile -ExecutionPolicy Bypass -File assets/characters/visitor-02/review/round2/RunDiagnostic.ps1` in a fresh process; uses Windows System.Drawing only.

No source, visitor-01, scene, effects or lifecycle edits. No replacement artwork, final sprites, commit, push, merge or deployment. Stop at this diagnostic.
