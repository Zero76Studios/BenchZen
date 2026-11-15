# Bench Zen visitor: replacement character art specification

Status: specification only. No existing artwork or code is changed by this document.
Target folder for the new set: `assets/characters/visitor-02/` (visitor-01 stays untouched).

## 1. Who the character is (preserve this)

The identity reference is the approved character model: the four large standing
views (front, left, back, right) on the left half of
`assets/characters/benchzen-character-round2c/character-master-reference.png`
(also extracted as `assets/characters/visitor-01/poses/standing-*.png`).

The hooded seated image (`seated-rear-transparent.png`) is the outlier. It is a
different drawing with different clothing, hair and proportions. It is NOT the
reference and is replaced by this set.

Characteristics to keep in every frame:

| Feature | What the approved model shows |
| --- | --- |
| Person | Adult man, anonymous dark silhouette, no facial features |
| Hair | Thick, tousled, slightly wavy, medium length. Full on top and at the back, covering the tops of the ears, with a few loose tips breaking the outline |
| Build | Medium to solid. Broad shoulders, soft midsection visible in side view, sturdy legs. Not slim, not athletic-heroic |
| Posture | Relaxed and upright, head level, arms hanging loose slightly away from the body, hands open |
| Top | Plain short-sleeved crew-neck T-shirt, loose, untucked, hem at hip level, sleeves ending mid upper arm, bare forearms |
| Trousers | Straight-leg jeans, slightly bunched at the ankle |
| Footwear | Sturdy low boots or chunky casual shoes |
| Not present | No hood, jacket, hat, bag, glasses, phone or other props |

Measured proportions of the approved model (fractions of standing height H,
head top to sole):

- Head with hair: about 0.09 H wide, about 0.14 H tall. Roughly 7 heads tall overall.
- Shoulders: 0.27 H wide, starting at 0.18 H below the head top.
- Hips: about 0.22 H wide. Shirt hem at about 0.47 H below the head top.
- Side-view body depth at the chest: about 0.15 H.

These must not drift between frames. If you want the character to resemble you
more closely than the approved model does (hair shape, build, usual clothes),
change it once, in the model sheet in section 6 step 1, before any other sheet
is generated.

## 2. Style and transparency

- Near-black silhouette, about `#0c0c0e`, with only very subtle charcoal interior
  shading (shirt hem, sleeve edge, trouser folds). Crisp, clean outline.
- No rim light, glow, halo, outline stroke, cast shadow, ground line, bench,
  scenery, text, labels, numbers, grid or frame borders. (The current sheet's
  warm halo and painted benches are what made its frames unusable.)
- Background: true transparency (RGBA PNG). If the tool cannot produce real
  transparency, use flat pure white `#FFFFFF` with nothing else on it. Never a
  checkerboard pattern.
- Lighting is neutral. The scene supplies all atmosphere.

## 3. Scene geometry the art must fit

Paris background: 1672 x 941 px, origin top-left, y downward. Unchanged.

| Scene feature | Scene px |
| --- | --- |
| Bench centre (where the visitor sits) | x = 842 |
| Ground line for the visitor's soles (walking and seated) | y = 881 |
| Seat contact (underside of the seated body) | y = 812 |
| Seat height above the soles | 69 px |
| Backrest top edge | y = 738 |
| Backrest slats | y = 738-758, 761-779, 783-800 (gaps between show the body) |
| Dark seat rail in front of the upper legs | y = 814-824 |
| Bench width | x = 645-1037 |

Target on-screen standing height: **280 px** (head top to sole). This follows
from the bench: a seat 69 px above the ground suits a person about 270-290 px
tall. The seated visitor's head top then sits near y = 666 and the head and
shoulders show above the backrest.

## 4. Frame canvas and anchors (every final frame)

Every final frame is one PNG with the same canvas and the same registration.

| Item | Value |
| --- | --- |
| Canvas | 512 x 768 px, RGBA, transparent |
| Character standing height H | 600 px (head top y = 84, soles y = 684) |
| Display scale in the scene | 280 / 600 = 0.4667 |
| Ground line (soles) | y = 684 in every frame, including seated frames |
| **Foot anchor** | (256, 684) = body centre line at the ground. Maps to scene (x, 881) |
| **Hip / seating anchor** | (256, 536) = centre of the underside of the seated body. Maps to scene (842, 812) |
| Seat plane | y = 536 (148 px above the ground line) |
| Backrest top, for reference only | y = 378 |
| Margins | at least 12 px transparent on all sides |

Derived sizes at H = 600: head 54 px wide and 84 px tall, shoulders 162 px wide,
hips 132 px wide.

The body centre line stays at x = 256 for standing, turning, sitting and seated
frames. In walk frames x = 256 is the centre of the hips.

## 5. Frames required (18 files)

### 5.1 Walk cycle: 8 frames, facing right, one full cycle (two steps)

Step length 252 px (0.42 H). The planted foot moves back exactly 63 px per frame
relative to the hips, which lets the code move the visitor 63 canvas px (29.4
scene px) per frame with no sliding.

Foot positions are the centre of the foot, as x offset from the hip centre (256).
"A" is the far leg, "B" the near leg. Feet on the ground have soles at y = 684.

| File | Pose | Foot A | Foot B | Head top y |
| --- | --- | --- | --- | --- |
| `walk-right-01.png` | A heel strikes ahead, B on its toe behind | +126, ground | -126, ground | 90 |
| `walk-right-02.png` | Weight settles on A, B lifts and trails | +63, ground | about -100, lifted | 94 |
| `walk-right-03.png` | Passing: B swings past A, knee bent | 0, ground | about 0, lifted | 86 |
| `walk-right-04.png` | A pushes off, B reaches forward | -63, ground (heel rising) | about +70, lifted | 84 |
| `walk-right-05.png` | B heel strikes ahead, A on its toe behind | -126, ground | +126, ground | 90 |
| `walk-right-06.png` | Weight settles on B, A lifts and trails | about -100, lifted | +63, ground | 94 |
| `walk-right-07.png` | Passing: A swings past B | about 0, lifted | 0, ground | 86 |
| `walk-right-08.png` | B pushes off, A reaches forward | about +70, lifted | -63, ground (heel rising) | 84 |

Arms swing gently opposite to the legs. Relaxed stroll, not a march. Torso
upright, head level. Frames 05-08 are the same poses as 01-04 with the legs and
arms swapped.

### 5.2 Standing and turning: 3 frames

| File | Pose |
| --- | --- |
| `stand-right.png` | Standing still, facing right (side view), feet together under the hips |
| `turn-right-to-rear.png` | Three-quarter rear view: turned halfway from facing right to facing away. We see mostly the back, with the right shoulder nearer |
| `stand-rear.png` | Standing still, back to the viewer, feet centred at x = 256 +/- 60 |

### 5.3 Sitting down: 4 in-between frames, rear view

Feet stay planted at x = 256 +/- 60, soles at y = 684, in every frame from
`stand-rear` to `seated-rest`. Standing up is the same frames played in reverse.

| File | Pose | Head top y | Underside of body y |
| --- | --- | --- | --- |
| (`stand-rear.png`) | Upright | 84 | - |
| `sit-01.png` | Knees start to bend, hips ease back, slight forward lean | 115 | about 440 |
| `sit-02.png` | Half-way down, torso clearly leaning forward, arms slightly out for balance | 180 | about 490 |
| `sit-03.png` | Body touches the seat, torso still leaning forward | 240 | 536 |
| `sit-04.png` | Torso coming upright, shoulders settling | 228 | 536 |
| (`seated-rest.png`) | Settled | 223 | 536 |

### 5.4 Seated: 3 frames, rear view

| File | Pose |
| --- | --- |
| `seated-rest.png` | Sitting upright and relaxed, back to the viewer. Underside of the body at y = 536, head top at y = 223, shoulders level, upper arms at the sides with forearms resting on the thighs, knees slightly apart, lower legs vertical, feet flat at x = 256 +/- 60 with soles at y = 684. The whole body is drawn, including everything the bench will later cover |
| `seated-look-left.png` | Identical body to `seated-rest`; only the head turned about 30 degrees to the left |
| `seated-look-right.png` | Identical body; only the head turned about 30 degrees to the right |

The two look frames are optional but give a real head turn instead of the
current tilt.

## 6. Generating the art with ChatGPT

Image tools cannot hit exact pixel positions. Generate sheets, and the frames
are then sliced, scaled and registered to the anchors above in a separate step
(scale and position only, no redrawing). What the generator must get right is
the content: same character, same size on a sheet, clean silhouettes, correct
poses, complete bodies.

Ask for landscape 1536 x 1024 images with a transparent background.

**Step 1: model sheet (identity anchor).** Attach the left half of
`character-master-reference.png` (the four standing views). Prompt:

> Using the attached character reference, draw a clean character model sheet of
> this exact same man as four full-body standing views in one row, all the same
> height, feet on the same level: front view, right side view, three-quarter
> rear view, rear view. Keep his thick tousled medium-length hair, medium-solid
> build, loose short-sleeved crew-neck T-shirt, straight-leg jeans and sturdy low
> boots exactly. Near-black silhouette with only very subtle charcoal shading
> inside, crisp outline. Transparent background. No rim light, no glow, no
> shadow, no ground line, no text, no labels, no props.

Approve this sheet before continuing. Attach it to every later request.

**Step 2: walk sheet.** Attach the approved model sheet. Prompt:

> Same man as the attached model sheet: same hair, T-shirt, jeans, boots, build
> and proportions, same silhouette style, transparent background. Draw an
> 8-frame walk cycle, side view facing right, in two rows of four, every figure
> the same height with feet on the same level in each row, evenly spaced, not
> overlapping. A relaxed stroll with gently swinging arms. Row 1: (1) front heel
> strikes the ground with legs at widest stride, (2) weight settles onto the
> front foot while the rear foot lifts, (3) passing position with the rear leg
> swinging past the standing leg, (4) the standing leg pushes off while the
> other leg reaches forward. Row 2: the same four poses with the opposite leg
> and arm leading. No rim light, glow, shadow, ground line, text or numbers.

**Step 3: turn and sit sheet.** Attach the model sheet. Prompt:

> Same man as the attached model sheet, same clothing, proportions and
> silhouette style, transparent background. Draw 7 full-body figures in one
> row, same scale, feet on the same level, evenly spaced: (1) standing, side
> view facing right; (2) three-quarter rear view, turning away from the viewer;
> (3) standing, rear view; then, all in rear view with the feet staying in the
> same place, sitting down onto an invisible seat at just below knee height:
> (4) knees starting to bend, hips easing back; (5) half-way down, leaning
> forward; (6) just seated, still leaning forward; (7) sitting upright and
> relaxed, forearms resting on thighs, feet flat. Draw the complete body in
> every figure. Do not draw the seat, a bench, a chair, shadows, text or
> numbers.

**Step 4: seated looks (optional).** Attach the model sheet and the step 3
sheet. Prompt:

> Same man, same seated rear-view pose as the last figure in the attached
> sheet, drawn three times in one row at the same scale: looking straight ahead,
> head turned about 30 degrees left, head turned about 30 degrees right. Only
> the head changes. Complete body, invisible seat, transparent background, no
> bench, shadow or text.

Regenerate any sheet in which the character changes clothing, hair, build or
size between figures, where a figure is cut off or overlaps another, or where a
bench, shadow, glow or label appears.

## 7. Files and organisation

```text
assets/characters/visitor-02/
  source/                     raw sheets exactly as generated (never edited)
    model-sheet.png
    walk-sheet.png
    turn-sit-sheet.png
    seated-looks-sheet.png
  frames/                     final registered 512 x 768 RGBA frames
    walk-right-01.png ... walk-right-08.png
    stand-right.png
    turn-right-to-rear.png
    stand-rear.png
    sit-01.png ... sit-04.png
    seated-rest.png
    seated-look-left.png
    seated-look-right.png
  manifest.json               canvas, anchors, scale, measured foot positions
  README.md                   how the frames were sliced and checked
```

Only a right-facing walk is required: the visitor enters from the left and
leaves to the right. A left-facing set (`walk-left-01..08`, `stand-left`,
`turn-left-to-rear`) can be added later with the same rules if wanted.

## 8. Acceptance checks before any code changes

Measured on the final frames, not judged by eye alone:

1. All 18 frames are 512 x 768 RGBA with a genuinely transparent background.
2. Soles at y = 684 (+/- 3 px) in every frame with a foot on the ground.
3. Standing height 600 px (+/- 2%) in `stand-right`, `stand-rear` and walk frame 03/07.
4. Head width and shoulder width within 5% across all rear-view frames, and head
   size within 5% across all frames.
5. Walk: the planted foot moves backward in every step of the cycle, by a
   similar amount each frame (target 63 px, tolerance +/- 15 px). The actual
   measured values go into the manifest and drive the movement.
6. Sit frames: feet do not move; body underside reaches y = 536 at `sit-03`.
7. Same T-shirt, jeans, boots and hairstyle in every frame.

## 9. What stays the same

The Paris background, the bench occlusion, lantern and sun glow, tower
twinkles, birds, the lifecycle (enter, walk, sit, rest, stand, leave), the
breathing and posture movement, and the `?preview=character` tools are all
kept. Once the frames pass section 8, the code change is limited to swapping
the frame tables, anchors and scale, and playing the sit frames in place of the
blend.
