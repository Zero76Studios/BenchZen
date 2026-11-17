# Round 7 — bench perspective check

**The fixed placement is visually plausible; the boots can read as resting on pavement farther from the viewer than the near bench feet. Grounding is not conclusive. Retain X842 / Y812 / 100% for visual review; this check does not justify correcting the artwork.**

Branch verified: `feature/paris-living-scene`. Read the character specification and Round 6 report; inspected the original empty-bench scene and reused the unchanged Round 6 100% composite.

| Evidence | Finding |
| --- | --- |
| Measured/transformed boot contours | Left (812.09,871.07); right (870.50,871.07). Round 6 alpha-based measurements, scene pixels. |
| Documented geometry | Contact target y812; ground reference y881. The latter is not a visible pavement edge. |
| Visible bench/pavement features | Near bench-foot tips appear around y883–890; seat-board depth is roughly 14 px (y800–814). These are visual readings of painted outlines, not surveyed 3D coordinates. Pavement continues behind the near feet and beneath both boots. |
| Illustrative local ground estimate at the boots | **y868–880**, representative line y874. This assumes the sitter's boots project 7–19 px farther away than near bench-foot contact around y887, an offset of the same order as visible seat depth. It is a perspective scenario, not a fitted or independently measured plane; other depths remain possible. |

The sole level y871.07 falls inside that illustrative band, about 2.93 px above its representative line. Thus the 9.93 px difference from y881 is **not evidence of a physical air gap by itself**. A horizontal image line at one depth cannot serve every point on the pavement. The bench's broadly level crosswise orientation supports using a roughly horizontal local guide between the boots; their actual depth remains unknown.

Perspective can reasonably explain the apparent float **if the feet are farther away than the near bench supports**. The stylized seat/leg geometry does not uniquely establish that depth or a vanishing point. Boot contact shadows are not clearly identifiable, so the soles retain a slightly suspended appearance in close-up. The jeans-underside anchor is still not verified anatomical support. No reliable metric leg-length correction follows from this evidence.

**Recommendation:** keep the current placement and inspect grounding at normal scene size before requesting new art. Do not enlarge, lower, or lengthen the character merely to reach y881. If subsequent review establishes the same ground depth as y881 and still finds floating feet, revisit the seated pose's leg projection separately. The specification remains unchanged; this is a visual plausibility assessment, not production approval.

[Annotated close-up](bench-perspective-closeup.png): existing 100% scene, uniform 4× inspection crop, measured soles in red, assumed local-depth band in cyan, reference y881 in gray. No artwork, placement, scale, scenery, animation or lifecycle changes. Prior Visitor-02 files verified unchanged by hashes; diagnostic files only. No commit, push, merge or deployment.
