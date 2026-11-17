# Visitor-02 — Round 3 seated-rest review

**STOP: existing artwork cannot supply a production-quality, spec-compliant seated-rest asset through clean extraction, uniform scale and translation alone.** No final PNG or Paris bench composite was created. The explicit stop condition takes precedence over those downstream deliverables.

Baseline verified on 2026-11-16: `feature/paris-living-scene`, commit `ea24f84` (`Checkpoint visitor-02 artwork and animation diagnostics`), clean working tree before this report.

Read the character art specification, Round 1 audit and Round 2 registration report. Visually inspected the approved master, the complete rear sitting source (both rows), and the registered rear diagnostic. Rechecked all five original source SHA-256 hashes against `review/source-integrity.json`; all match. Numerical findings below reuse the existing Round 2 measurements, not a new extraction or anatomical measurement.

## Best candidate and extraction decision

Top-row pose 8 (`R08`, seated idle) is the best semantic rest candidate: upright rear view, tousled hair, short-sleeved T-shirt, jeans, boots, arms bent toward thighs. Top-row 7 is a contact candidate. Bottom-row 1 is a separately drawn seated start, not a proven interchangeable endpoint. The small poses in the original reference contain benches and obscured body areas; the side sheet does not provide the required rear view.

The rear sheet is 2172 × 724 RGB, with an opaque black background and no alpha. Dark hair, shirt and jeans approach the background colour; a black-removal threshold cannot establish a reliable complete contour. Low thresholds retain background/fringe, while higher thresholds risk holes and damaged clothing. Warm rim lighting also remains inconsistent with the neutral silhouette specification. This does not prove every contour is unrecoverable with manual work, but clean extraction preserving the full body is not established, and geometry independently rules out the candidate.

## Standing scale and bench contact

At the common rear scale `600 / 268 = 2.238806`, derived from standing R04, with soles registered at canvas y=684:

| Check | R08 | Required |
| --- | --- | --- |
| Estimated underside | (256, 581.0) | (256, 536) |
| Underside-to-ground clearance | 103.0 canvas px | 148 canvas px |
| Head top | y=213.9 | about y=223 |
| Head envelope width | 107.5 canvas px | within 5% of standing reference |
| Projected shoulder envelope | 174.6 canvas px | consistent with standing reference |

R04's measured head envelope is 98.5 px and shoulder envelope 194.8 px. R08's head is about 9.1% wider; its projected shoulder envelope is about 10.3% narrower. Sleeve/arm projection affects shoulder measurements, so this is a continuity warning rather than a direct anatomical breadth measurement. Hair and clothing match broadly, but proportion compliance is not established.

The R08 underside estimate varies from y=569.8 to 581.0 across the existing threshold checks, still 33.8–45.0 px below the seat. R07 also fails: approximately y=547–577, rather than 536, and is not the relaxed endpoint. Landmark uncertainty and opaque contours prevent claiming exact anatomical contact.

At scene scale 280/600, R08's estimated underside lands at about scene y=833 with soles at 881; the Paris seat is y=812. That is about 21 scene px below the seat (at least about 16 px across the threshold checks). Hip-aligning instead would lift the soles about 21 scene px above the ground. A uniform enlargement to supply the missing clearance would increase scale roughly 44%, contradicting standing consistency. Moving or stretching individual body parts is disallowed.

The Paris backrest slats (y=738–758, 761–779, 783–800) and foreground seat rail (y=814–824) could conceal parts of the defect; they cannot correct body contact or supply a valid matte. No composite was made because an opaque crop would fail the background requirement and a speculative matte would imply an unvalidated asset. Correct foreground occlusion and natural contact therefore remain unvalidated.

## Result and required next artwork

Created this report only. Source integrity and repository baseline checks passed; production transparency, simultaneous hip/sole registration, proportion consistency and bench contact did not pass or remain unproven. Existing evidence: [registered rear comparison](review/round2/registered-rear.png), [measurements](review/round2/measurements.json).

A corrected complete rear seated-rest drawing is needed, preserving the approved identity and standing-derived scale, with real transparency, hip underside (256,536), soles y=684, and no bench or lighting halo. Do not reconstruct hidden anatomy from these sources. That corrected drawing will require visual approval of identity, complete anatomy/clothing, hair and shoulder proportions, followed by a Paris composite checking contact and foreground occlusion. No production seated asset is ready for approval in this round.

Original images, visitor-01, Paris environment and animation lifecycle are unchanged. No transitions or walking work, commit, push, merge or deployment performed.
