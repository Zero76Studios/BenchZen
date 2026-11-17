# Visitor-02 — Round 4 seated candidate

**Not production-ready. Stopped at the requested reliable-extraction gate. No registered frame, bench composite or animation integration.** This is a failure of the attempted extraction to demonstrate preservation, not a claim that no other extraction method could work.

## Baseline and preservation

Verified `feature/paris-living-scene` at `ea24f84` (`Checkpoint visitor-02 artwork and animation diagnostics`). The Round 3 `SEATED-REST-REPORT.md` was already untracked; preserved it. Read the character art specification and all three preceding Visitor-02 reports.

Found `C:\Users\strid\Downloads\visitor-02-seated-candidate.png` and copied it unchanged to [source/visitor-02-seated-candidate.png](source/visitor-02-seated-candidate.png). Both SHA-256 hashes are `0B3C01490BB97568A0546B6EA27E75D976A3C2C58F915E5AF097CD7521A23BE8`. All five previous source hashes still match the original integrity manifest. No tracked files changed.

## Transparency and separate extraction attempt

The candidate is **1048 × 1501, 24-bit RGB**. All 1,573,048 pixels are opaque. The irregular white/gray checkerboard is baked into the artwork, including between the arms and torso and between the legs.

Used the built-in imagegen tool for a separate background-extraction attempt, saved as [review/round4/seated-cutout-attempt.png](review/round4/seated-cutout-attempt.png). The original is untouched. Output retains the same dimensions and removes the visible checkerboard, but contains 997,619 fully transparent pixels, 575,113 partially transparent pixels and only 316 fully opaque pixels. Example body pixels have alpha 253 rather than 255. Compared at the same coordinates, the sampled dark source pixels show changed RGB values: mean absolute channel difference 2.40; 11,431 of 125,389 samples have summed RGB difference above 15. These samples include edges, so minimum alpha 46 is not proof of an interior hole.

The attempted method did not preserve character pixels and does not establish a dependable production matte. It is retained as **unapproved evidence only**. No anatomy was deliberately reconstructed, but preservation cannot be certified from this generated edit. Do not promote this attempt to final frames.

Tool prompt: “Use case: background-extraction. Edit target: attached seated rear-view character. Remove ONLY the baked white/gray checkerboard background, including gaps between arms and torso and between legs. Output true RGBA transparency. Preserve the exact existing character pixels, outline, hair tips, clothing, anatomy, proportions, posture, lighting, and boots. Keep the original canvas and placement. Do not redraw, reconstruct, retouch, relight or invent any body parts. This is a strict non-destructive cutout for comparison, not new character generation.”

## Visual design findings

Compared with the approved master's rear view, the candidate broadly preserves tousled medium-length hair, a solid build, short-sleeved dark T-shirt, jeans and sturdy boots. The candidate has pronounced orange rim lighting along hair, shoulders, arms and boots; this remains outside the neutral, near-black silhouette specification. Jeans have conspicuous texture. The shirt/back appears long and the knees widely separated; these are visual concerns, not measured anatomical findings. The pose depicts a rear sitter with bent arms toward the thighs and visible lower jeans and boots. No obvious missing major limb is apparent, but this is not a complete edge/anatomy acceptance.

Head/hair width, anatomical shoulder width and torso proportions have **not passed the 5% consistency checks**. The comparison page displays source images at inspection sizes, not a validated common scale. Source framing is not evidence of matching proportions.

## Seat contact and preview disposition

The visible lower jeans contour is not itself proof of the anatomical support point. No verified contact landmark or uniform registration transform was established. Therefore neither hip anchor (256,536), soles y=684, 512 × 768 frame compliance, standing scale, nor actual contact at Paris (842,812) is claimed. Proper foreground occlusion remains untested. No stretching or distortion was applied to make the candidate fit.

Created a separate development-only [source-comparison page](review/round4/source-comparison.html) with the approved rear view, untouched candidate and failed cutout, plus a light/dark backdrop toggle. It does not load the lifecycle or alter the normal app. Attempting to open the page for browser inspection returned “The browser access host is unavailable”; browser rendering and shared visual review are pending. Open the HTML directly, or use the existing local server and visit `/assets/characters/visitor-02/review/round4/source-comparison.html`.

Next requirement: obtain a verified transparent export of this exact candidate, or use an extraction method demonstrably preserving its geometry and opaque interior. Then measure head/shoulder consistency, choose a standing-consistent uniform scale, identify anatomical support with explicit uncertainty, register to the hip target and check soles, and render the actual Paris bench with its existing foreground occlusion. Lighting treatment will also need review. **No visual approval has been granted.**

Measurements and hashes: [validation.json](review/round4/validation.json). Approved master, visitor-01, Paris scenery, lifecycle and animations unchanged. No commit, push, merge or deployment.
