# Visitor-02 art audit — Round 1

**Verdict: useful reference and extraction candidates; insufficient for a consistent, natural walk/sit preview as supplied.**

Baseline verified: `feature/paris-living-scene` at `1fbdd6b08d7152fa65dbd89eb6ac8fbf79326d71`, initially clean. Authority: [character-art-spec.md](../../../docs/character-art-spec.md), especially sections 4, 5 and 8.

## Import

Inspected `C:\Users\strid\Downloads\benchzen_character_art_handoff.zip` before extracting: one directory, exactly five PNGs, no other files. Original filenames and bytes preserved in `source/`; all five SHA-256 hashes match the archive entries. See [integrity/alpha measurements](review/source-integrity.json) and [contact sheet](review/contact-sheet.png). No final frames were created.

## The five sources

| Source | Dimensions / layout | Transparency and usability |
| --- | --- | --- |
| `original-reference.png` | 1774 x 887; four large standing views plus small action rows at different scales | Real RGBA transparency, extensive partial alpha, halo residue, labels/rules. Four large views are the specified identity reference. Small action figures have baked benches/occluded bodies; unsuitable as replacement sprites. |
| `approved-master-character.png` | 1536 x 1024; front, left, rear, right standing views | Fully opaque RGB brown vignette/ground shading. Useful identity and standing reference; no three-quarter view. Requires separate matte/style work before sprite use. |
| `walk-cycle-candidate.png` | 2172 x 724; eight labelled figures in one row | Real RGBA transparency; most body pixels near-opaque. Best diagnostic extraction candidate: complete, separate bodies, no bench. Gait sequence needs correction. |
| `rear-sit-motion-reference.png` | 2172 x 724; two rows of eight, with approach/departure steps, headers/rules | Fully opaque RGB black. Complete bodies, no bench; useful turn/descent/rest candidates. Feet and seat geometry do not yet meet the specification. |
| `side-sit-motion-reference.png` | 2172 x 724; two rows of eight, headers/rules | Fully opaque RGB black. Useful lean/contact/settle reference; side views cannot fill required rear-view frames. |

Both opaque backgrounds fail the transparent-or-pure-white source requirement. Simple black removal would damage dark clothing/hair. All sheets retain warm rim lighting and detailed skin/clothing rather than the neutral near-black silhouette style.

## Layout, identity and continuity

- **Registration:** none is a final 512 x 768 frame with foot `(256,684)` and seated hip `(256,536)` anchors. Uniform scaling/translation can register valid poses; it cannot repair their anatomy or motion.
- **Walk spacing:** alpha >=128 components, excluding labels at y>=625, measure 572–579 px tall (about 1.2% variation), 157–329 px wide, with lowest pixels at y=615–620. Horizontal bounds overlap for figures 1/2 and 6/7 although bodies are disconnected. Equal 271.5 px cells would clip limbs: use component masks. [Measured bounds](review/walk-component-bounds.txt) use coordinates relative to a crop starting at source y=35; these are silhouette bounds, not measured sole/hip anchors.
- **Identity:** broadly consistent tousled hair, solid build, short-sleeved T-shirt, jeans and boots; no hood/props. Apparent shoulder breadth, hair outlines and torso shape vary, particularly between the independently drawn sit/stand rows. Registered head/shoulder widths have not established the required 5% tolerance.
- **Walk poses:** count labels left to right. 1/5 are contact candidates; 2/3 have similar trailing-leg poses. 4 lacks clear forward swing-foot reach. 6, labelled down, is another broad stride with a raised forward foot; 7 resembles down/trailing rather than passing; 8 again lacks clear forward reach. Opposite-leg symmetry, arm continuity, planted-foot progression of 63 +/-15 canvas px and the 8-to-1 loop are unproven. Labels are not reliable gait evidence.
- **Rear sit/stand:** top-row 1 is a three-quarter candidate, 4 standing, 5/6 descent, 7/8 contact/rest. Feet widen from narrow standing to wide seated stance, defeating fixed-foot continuity. 7/8 are similarly upright: a distinct forward-leaning first-contact pose followed by settling is unclear. Bottom-row standing poses are separately drawn, not exact reversals; mixing rows would introduce jumps.
- **Seat height warning:** visually, rear top-row 4 is about 270 px tall; row-1 pose 8 has about 50–55 px underside-to-sole clearance, roughly 0.19–0.20 H versus required 148/600 = 0.247 H. That suggests 52–57 scene px instead of 69. Approximate only: dark matte makes the contour ambiguous. [Seated detail](review/rear-seated-detail.png).

## Missing or unusable frames

All **18 registered deliverables** are absent. Source coverage:

| Required files | Status |
| --- | --- |
| `walk-right-01..08` | Eight drawings, but valid alternating down/passing/up phases are not established; especially 04/06/07/08 need correction or validated reassignment. |
| `stand-right`, `turn-right-to-rear`, `stand-rear` | Candidates exist; require clean alpha, consistent style and registration. |
| `sit-01..04` | Descent candidates exist; complete rear-view, foot-locked bend/lean/contact/settle sequence is missing or unusable as supplied. |
| `seated-rest` | Rear top-row 8/bottom-row 1 are candidates pending matte, stance, seat-height and proportion checks. |
| `seated-look-left/right` | Missing matching rear seated poses with head-only turns. Optional for an initial preview. |

## Smallest next step

Run an **offline registration/contact test**: isolate the eight walk candidates, apply one common uniform scale and hip registration, and measure both feet through the cycle. Test only rear top-row 4/8 against ground/seat guides at one standing-derived scale. This establishes exactly which poses survive before processing an entire set.

Then obtain corrected deficient walk phases and a transparent, foot-locked rear descent/contact/settle sequence at the correct seat height. Keep the identity; defer optional head looks. Do not bridge gaps with stretching, crossfades or morphing. Runtime integration waits for the spec's acceptance checks.

Visitor-01, Paris artwork/effects and animation lifecycle unchanged. No replacement artwork generated; no commit, push, merge or deploy.
