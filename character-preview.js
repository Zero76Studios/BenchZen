const MANIFEST_PATH = "assets/characters/visitor-01/manifest.json";
const ASSET_ROOT = "assets/characters/visitor-01/";
const BACKGROUND = "assets/images/paris/paris-clean.png";
const SEATED_PATH = "assets/characters/visitor-01/seated-placement.json";
const ALIGNMENT_PATH = "assets/characters/visitor-01/seat-alignment.json";

// Percentages are relative to .scene, including its letterboxed placement.
// Scale means visible body height relative to a 330px reference, not PNG width.
export function placementFor(pose, settings, dimensions) {
  const nativeScale = (330 * settings.scalePercent / 100) / pose.retainedSourceBounds[3];
  const [width, height] = dimensions;
  return {
    nativeScale,
    left: (settings.x - pose.groundAnchor[0] * nativeScale) / width * 100,
    top: (settings.y - pose.groundAnchor[1] * nativeScale) / height * 100,
    width: pose.dimensions[0] * nativeScale / width * 100,
    height: pose.dimensions[1] * nativeScale / height * 100,
  };
}

// A geometric envelope with a hip/seat anchor, never a transformed standing PNG.
export function seatedPlacementFor(guide, settings, dimensions) {
  const scale = settings.scalePercent / 100;
  return {
    left: (settings.x - guide.seatAnchor[0] * scale) / dimensions[0] * 100,
    top: (settings.y - guide.seatAnchor[1] * scale) / dimensions[1] * 100,
    width: guide.dimensions[0] * scale / dimensions[0] * 100,
    height: guide.dimensions[1] * scale / dimensions[1] * 100,
  };
}

export function seatedArtworkPlacementFor(asset, settings, dimensions) {
  const nativeScale = asset.referenceNativeScale * settings.scalePercent / 100;
  return {
    nativeScale,
    left: (settings.x - asset.seatAnchor[0] * nativeScale) / dimensions[0] * 100,
    top: (settings.y - asset.seatAnchor[1] * nativeScale) / dimensions[1] * 100,
    width: asset.dimensions[0] * nativeScale / dimensions[0] * 100,
    height: asset.dimensions[1] * nativeScale / dimensions[1] * 100,
  };
}

function setBounds(element, placement) {
  for (const key of ["left", "top", "width", "height"]) element.style[key] = `${placement[key]}%`;
}

// Measurements are independent of the placement pivot; they never reposition art.
export function alignmentFor(asset, settings, diagnostic) {
  const p = seatedArtworkPlacementFor(asset, settings, diagnostic.backgroundDimensions);
  const project = ([x, y]) => [settings.x + (x - asset.seatAnchor[0]) * p.nativeScale,
    settings.y + (y - asset.seatAnchor[1]) * p.nativeScale];
  const point = project(diagnostic.character.estimatedContourPoint);
  return {
    imageBounds: [settings.x - asset.seatAnchor[0] * p.nativeScale,
      settings.y - asset.seatAnchor[1] * p.nativeScale,
      asset.dimensions[0] * p.nativeScale, asset.dimensions[1] * p.nativeScale],
    contourPoint: point,
    contour: diagnostic.character.localContour.map(project),
    gapAboveSeatBand: Math.max(0, diagnostic.bench.seatSurfaceY[0] - point[1]),
    footBottomY: settings.y + (asset.characterBounds[1] + asset.characterBounds[3] - asset.seatAnchor[1]) * p.nativeScale,
  };
}

export async function startCharacterPreview({ experience, scene, artwork }) {
  const panel = document.createElement("details");
  panel.className = "character-preview";
  panel.open = true;
  panel.innerHTML = `
    <summary>Character preview <span>development only</span></summary>
    <div class="preview-controls">
      <p>Static placement study - collapse this panel for a clear view.</p>
      <fieldset disabled>
        <legend>Visitor 01 placement</legend>
        <label for="preview-study">Study</label>
        <select id="preview-study">
          <option value="seated">Seated geometric guide</option>
          <option value="seated-artwork">Seated character - transparent</option>
          <option value="standing">Standing / walking stills</option>
        </select>
        <p class="preview-study-note"></p>
        <div class="preview-pose-field">
          <label for="preview-pose">Pose</label>
          <select id="preview-pose"></select>
        </div>
        <div class="preview-field">
          <label for="preview-scale">Scale (%)</label>
          <input id="preview-scale-number" aria-label="Scale percent for active study, exact value" type="number" min="25" max="180" step="1" value="100">
          <input id="preview-scale" type="range" min="25" max="180" step="1" value="100">
        </div>
        <div class="preview-field">
          <label for="preview-x">Horizontal (px)</label>
          <input id="preview-x-number" aria-label="Horizontal position for active study, exact pixels" type="number" min="0" max="1672" step="1" value="842">
          <input id="preview-x" type="range" min="0" max="1672" step="1" value="842">
        </div>
        <div class="preview-field">
          <label for="preview-y" id="preview-y-label">Seat / hip (px)</label>
          <input id="preview-y-number" aria-label="Vertical anchor position for active study, exact pixels" type="number" min="0" max="941" step="1" value="812">
          <input id="preview-y" type="range" min="0" max="941" step="1" value="812">
        </div>
        <label class="preview-occlusion-field"><input id="preview-occlusion" type="checkbox" checked> Bench slat / front rail occlusion</label>
        <label class="preview-alignment-controls"><input id="preview-alignment" type="checkbox" checked> Show seat alignment guides</label>
        <p class="preview-alignment-legend" hidden><span> Cyan: seat surface</span> / <span>red: clothing contact estimate</span> / <span>amber: PNG bounds</span> / <span>purple: foreground mask</span>. White: backrest.</p>
        <p class="preview-alignment-readout" hidden></p>
        <p class="preview-measurement"></p>
        <p class="preview-placement-warning" role="status"></p>
        <div class="preview-actions">
          <button type="button" id="preview-reset">Reset this study</button>
          <button type="button" id="preview-copy">Copy settings</button>
        </div>
        <details class="preview-json">
          <summary>Settings JSON / manual copy</summary>
          <textarea aria-label="Current preview settings JSON" readonly spellcheck="false"></textarea>
        </details>
      </fieldset>
      <p class="preview-load-status" role="status">Loading character manifest…</p>
      <p class="preview-copy-status" role="status"></p>
      <p class="preview-seated-source-status" role="status"></p>
      <p>No animation. Repaired seated transparency passed technical checks; character appearance and placement still need your visual review.</p>
      <a href="./">Return to normal experience</a>
    </div>`;
  experience.append(panel);
  const find = selector => panel.querySelector(selector);
  const loadStatus = find(".preview-load-status");
  const copyStatus = find(".preview-copy-status");
  let manifest, seatedConfig, diagnostic;
  try {
    [manifest, seatedConfig, diagnostic] = await Promise.all([MANIFEST_PATH, SEATED_PATH, ALIGNMENT_PATH].map(async path => {
      try {
        const response = await fetch(path);
        if (!response.ok) throw new Error("Placement metadata unavailable");
        return await response.json();
      } catch (error) {
        // A missing optional diagnostic must not break existing pose controls.
        if (path === ALIGNMENT_PATH) return null;
        throw error;
      }
    }));
    if (manifest.poses.length !== 12 || manifest.parisAlignment.dimensions.join() !== "1672,941") {
      throw new Error("Unexpected asset manifest");
    }
  } catch {
    loadStatus.textContent = "Cannot load character placement metadata. Serve BenchZen locally with node tools/serve.cjs and reopen /?preview=character.";
    return;
  }

  const poses = manifest.poses;
  const dimensions = manifest.parisAlignment.dimensions;
  const seatedAsset = manifest.seatedArtwork;
  const defaults = { standing: seatedConfig.approvedStanding, seated: seatedConfig.proposedSeated,
    "seated-artwork": seatedConfig.proposedArtworkSeated };
  const settings = Object.fromEntries(Object.entries(defaults).map(([key, value]) => [key, { ...value }]));
  let study = "seated-artwork";
  const currentSettings = () => settings[study];
  const selector = find("#preview-pose");
  for (const pose of poses) {
    const option = document.createElement("option");
    option.value = pose.id;
    option.textContent = pose.id.replaceAll("-", " ");
    selector.append(option);
  }
  selector.value = settings.standing.pose;
  find("fieldset").disabled = false;
  const layer = scene.querySelector('[data-layer="character"]');
  layer.classList.add("preview-character-layer");
  const standingLayer = document.createElement("div");
  standingLayer.className = "scene__layer";
  const guide = document.createElement("div");
  guide.className = "preview-seated-guide";
  guide.hidden = true;
  const geometry = document.createElementNS("http://www.w3.org/2000/svg", "svg");
  geometry.setAttribute("viewBox", `0 0 ${seatedConfig.guide.dimensions.join(" ")}`);
  geometry.setAttribute("fill", seatedConfig.guide.fill);
  geometry.setAttribute("aria-hidden", "true");
  for (const shape of seatedConfig.guide.shapes) {
    const primitive = document.createElementNS("http://www.w3.org/2000/svg", shape.type);
    for (const [key, value] of Object.entries(shape)) if (key !== "type") primitive.setAttribute(key, value);
    geometry.append(primitive);
  }
  const guideLabel = document.createElement("span");
  guideLabel.className = "preview-guide-label";
  guideLabel.textContent = "SEATED GUIDE - NO ARTWORK";
  guide.append(geometry, guideLabel);
  const seatedSource = document.createElement("div");
  seatedSource.className = "preview-seated-source";
  seatedSource.hidden = true;
  const seatedLabel = document.createElement("span");
  seatedLabel.className = "preview-guide-label";
  seatedLabel.textContent = "SEATED CHARACTER - REVIEW";
  const seatedImage = new Image();
  seatedImage.className = "preview-seated-image";
  seatedImage.alt = "";
  seatedImage.draggable = false;
  let seatedImageReady = false;
  seatedSource.append(seatedImage, seatedLabel);
  layer.replaceChildren(standingLayer, guide, seatedSource);

  const overlay = new Image();
  overlay.className = "preview-bench-occlusion";
  overlay.alt = "";
  overlay.hidden = true;
  let overlayReady = false;
  const [ox, oy, ow, oh] = seatedConfig.occlusion.sourceRect;
  setBounds(overlay, { left: ox / dimensions[0] * 100, top: oy / dimensions[1] * 100,
    width: ow / dimensions[0] * 100, height: oh / dimensions[1] * 100 });
  scene.querySelector('[data-layer="foreground"]').append(overlay);
  let displayedImage = null;
  let imageReady = false;
  let requestId = 0;
  const textarea = find("textarea");
  const occlusionToggle = find("#preview-occlusion");
  const alignmentToggle = find("#preview-alignment");
  alignmentToggle.disabled = !diagnostic;
  const svgNS = "http://www.w3.org/2000/svg";
  const alignmentSVG = document.createElementNS(svgNS, "svg");
  alignmentSVG.classList.add("preview-seat-alignment");
  alignmentSVG.setAttribute("viewBox", `0 0 ${dimensions.join(" ")}`);
  alignmentSVG.setAttribute("aria-hidden", "true");
  alignmentSVG.setAttribute("hidden", "");
  scene.append(alignmentSVG); // Above the existing foreground overlay, inside .scene.
  const shape = (type, attributes, parent = alignmentSVG) => {
    const element = document.createElementNS(svgNS, type);
    for (const [key, value] of Object.entries(attributes)) element.setAttribute(key, value);
    parent.append(element);
    return element;
  };
  let boundsGuide, contourGuide, pointGuide, diagnosticLabel;
  if (diagnostic) {
    const [bx, by, bw, bh] = diagnostic.bench.backrestBounds;
    shape("rect", { x: bx, y: by, width: bw, height: bh, fill: "none", stroke: "#ffffff", "stroke-dasharray": "4 4", "stroke-width": 1 });
    const [mx, my, mw, mh] = diagnostic.occlusion.bounds;
    shape("rect", { x: mx, y: my, width: mw, height: mh, fill: "none", stroke: "#d491ff", "stroke-dasharray": "6 3", "stroke-width": 1 });
    for (const [top, bottom] of diagnostic.occlusion.opaqueYBands) {
      shape("rect", { x: mx, y: top, width: mw, height: bottom - top, fill: "#d491ff", "fill-opacity": 0.12, stroke: "#d491ff", "stroke-width": 0.5 });
    }
    shape("polygon", { points: diagnostic.bench.seatSurfacePolygon.map(p => p.join(",")).join(" "), fill: "#54d6f4", "fill-opacity": 0.22, stroke: "#54d6f4", "stroke-width": 1 });
    boundsGuide = shape("rect", { fill: "none", stroke: "#ffc857", "stroke-dasharray": "5 3", "stroke-width": 1 });
    contourGuide = shape("polyline", { fill: "none", stroke: "#ff7373", "stroke-width": 1.5 });
    pointGuide = shape("circle", { r: 4, fill: "#11151c", stroke: "#ff7373", "stroke-width": 2 });
    diagnosticLabel = shape("text", { fill: "#ffc857", "font-size": 12, "font-family": "sans-serif" });
    diagnosticLabel.textContent = "PNG bounds / diagnostic only";
  }

  function renderAlignment(backgroundReady) {
    const visible = Boolean(diagnostic && study === "seated-artwork" && alignmentToggle.checked && backgroundReady && seatedImageReady);
    alignmentSVG.toggleAttribute("hidden", !visible);
    find(".preview-alignment-legend").hidden = !visible;
    find(".preview-alignment-readout").hidden = !visible;
    if (!diagnostic) return null;
    const result = alignmentFor(seatedAsset, settings["seated-artwork"], diagnostic);
    const [x, y, width, height] = result.imageBounds;
    for (const [key, value] of Object.entries({ x, y, width, height })) boundsGuide.setAttribute(key, value);
    contourGuide.setAttribute("points", result.contour.map(p => p.join(",")).join(" "));
    pointGuide.setAttribute("cx", result.contourPoint[0]);
    pointGuide.setAttribute("cy", result.contourPoint[1]);
    diagnosticLabel.setAttribute("x", x);
    diagnosticLabel.setAttribute("y", y - 6);
    const cy = result.contourPoint[1];
    const [top, bottom] = diagnostic.bench.seatSurfaceY;
    const relation = cy < top ? `${(top - cy).toFixed(1)}px above the seat band.`
      : cy > bottom ? `${(cy - bottom).toFixed(1)}px below the seat band.`
        : "Projects inside the seat band; no measured central air gap.";
    find(".preview-alignment-readout").textContent = `Clothing underside y=${cy.toFixed(2)}; bench surface y=${top}-${bottom}. ${relation} This is a silhouette estimate, not a verified weight-bearing joint.`;
    return result;
  }

  function render() {
    const pose = poses.find(item => item.id === settings.standing.pose);
    const placement = placementFor(pose, settings.standing, dimensions);
    const backgroundReady = !artwork.hidden && artwork.naturalWidth === dimensions[0] && artwork.naturalHeight === dimensions[1];
    if (displayedImage) {
      setBounds(displayedImage, placement);
      displayedImage.hidden = study !== "standing" || !imageReady || !backgroundReady;
    }
    setBounds(guide, seatedPlacementFor(seatedConfig.guide, settings.seated, dimensions));
    guide.hidden = study !== "seated" || !backgroundReady;
    const sourcePlacement = seatedArtworkPlacementFor(seatedAsset, settings["seated-artwork"], dimensions);
    setBounds(seatedSource, sourcePlacement);
    seatedSource.hidden = study !== "seated-artwork" || !backgroundReady || !seatedImageReady;
    overlay.hidden = study === "standing" || !backgroundReady || !overlayReady || !occlusionToggle.checked;
    const alignment = renderAlignment(backgroundReady);
    const active = currentSettings();
    const seatedScale = active.scalePercent / 100;
    const torsoLeft = study === "seated-artwork"
      ? active.x + (seatedAsset.characterBounds[0] - seatedAsset.seatAnchor[0]) * sourcePlacement.nativeScale
      : active.x - 38 * seatedScale;
    const torsoRight = study === "seated-artwork"
      ? torsoLeft + seatedAsset.characterBounds[2] * sourcePlacement.nativeScale : active.x + 38 * seatedScale;
    const warnings = [];
    if (study !== "standing") {
      if (study === "seated-artwork") warnings.push("Diagnostic baseline only; no placement is approved. Reducing Y to 800 moves upward. No seating correction is applied.");
      if (torsoLeft < ox || torsoRight > ox + ow || Math.abs(active.y - defaults[study].y) > 8) {
        warnings.push("Off-bench placement: central overlay coverage or proposed seat height exceeded.");
      }
      if (!overlayReady) warnings.push("Bench overlay unavailable or still loading; occlusion is not demonstrated.");
      else if (!occlusionToggle.checked) warnings.push("Occlusion disabled for comparison.");
    }
    find(".preview-placement-warning").textContent = warnings.join(" ");
    textarea.value = JSON.stringify({
      schemaVersion: 5,
      activeStudy: study,
      characterId: manifest.characterId,
      manifest: MANIFEST_PATH,
      background: BACKGROUND,
      backgroundDimensions: dimensions,
      coordinateSystem: "background pixels, top-left origin",
      standing: {
        pose: settings.standing.pose, image: ASSET_ROOT + pose.file, imageDimensions: pose.dimensions,
        groundPosition: { x: settings.standing.x, y: settings.standing.y },
        localGroundAnchor: pose.groundAnchor, scalePercent: settings.standing.scalePercent,
        visibleBodyHeight: 330 * settings.standing.scalePercent / 100, nativeImageScale: placement.nativeScale,
      },
      seated: {
        status: "geometric-guide-only", actualArtworkAvailable: true, actualArtworkUsable: true,
        seatPosition: { x: settings.seated.x, y: settings.seated.y },
        scalePercent: settings.seated.scalePercent, guideDimensions: seatedConfig.guide.dimensions,
        localSeatAnchor: seatedConfig.guide.seatAnchor, definition: SEATED_PATH,
      },
      seatedArtwork: {
        status: seatedAsset.status, productionReady: false, transparencyGenuine: seatedAsset.transparency.genuine,
        image: ASSET_ROOT + seatedAsset.file, imageDimensions: seatedAsset.dimensions,
        seatPosition: { x: settings["seated-artwork"].x, y: settings["seated-artwork"].y },
        scalePercent: settings["seated-artwork"].scalePercent, nativeImageScale: sourcePlacement.nativeScale,
        localSeatAnchor: seatedAsset.seatAnchor, anchorStatus: seatedAsset.anchorStatus,
      },
      occlusion: { ...seatedConfig.occlusion, enabled: occlusionToggle.checked, assetLoaded: overlayReady },
      alignmentDiagnostic: alignment ? { enabled: alignmentToggle.checked, definition: ALIGNMENT_PATH,
        estimatedClothingContour: alignment.contourPoint, gapAboveSeatBand: alignment.gapAboveSeatBand,
        physicalSupportPointVerified: false, placementCorrectionApplied: false } : null,
    }, null, 2);
    find(".preview-measurement").textContent = study === "standing"
      ? `${330 * settings.standing.scalePercent / 100}px visible height - ground (${settings.standing.x}, ${settings.standing.y})`
      : study === "seated" ? `Geometry only - hip (${active.x}, ${active.y}); ${active.scalePercent}% guide scale`
        : `Seated character - hip (${active.x}, ${active.y}); ${active.scalePercent}% (native scale ${sourcePlacement.nativeScale.toFixed(4)})`;
    copyStatus.textContent = "";
  }

  function loadPose() {
    const pose = poses.find(item => item.id === settings.standing.pose);
    const thisRequest = ++requestId;
    imageReady = false;
    if (displayedImage) displayedImage.hidden = true;
    loadStatus.textContent = "Loading pose…";
    const image = new Image();
    image.className = "preview-character";
    image.alt = ""; // Decorative scene layer; selected pose is named by the control.
    image.draggable = false;
    image.hidden = true;
    image.onload = () => {
      if (thisRequest !== requestId) return;
      if (image.naturalWidth !== pose.dimensions[0] || image.naturalHeight !== pose.dimensions[1]) {
        loadStatus.textContent = "Pose dimensions differ from the manifest. Check the original extracted PNG.";
        return;
      }
      displayedImage = image;
      imageReady = true;
      standingLayer.replaceChildren(image);
      loadStatus.textContent = "";
      render();
    };
    image.onerror = () => {
      if (thisRequest !== requestId) return;
      loadStatus.textContent = "Pose image unavailable. Choose another pose or check assets/characters/visitor-01/poses/.";
    };
    image.src = ASSET_ROOT + pose.file;
    render();
  }

  const fields = [["scale", "scalePercent"], ["x", "x"], ["y", "y"]];
  for (const [id, key] of fields) {
    const range = find(`#preview-${id}`);
    const number = find(`#preview-${id}-number`);
    for (const input of [range, number]) {
      input.addEventListener("input", () => {
        if (input.value === "" || !Number.isFinite(input.valueAsNumber)) return;
        const value = Math.round(Math.max(Number(input.min), Math.min(Number(input.max), input.valueAsNumber)));
        currentSettings()[key] = value;
        range.value = value;
        if (input === range) number.value = value;
        render();
      });
      input.addEventListener("change", () => { number.value = currentSettings()[key]; });
    }
  }
  selector.addEventListener("change", () => {
    settings.standing.pose = selector.value;
    loadPose();
  });
  find("#preview-reset").addEventListener("click", () => {
    Object.assign(currentSettings(), defaults[study]);
    if (study !== "standing") occlusionToggle.checked = true;
    syncControls();
    if (study === "standing") loadPose();
    else render();
  });
  function syncControls() {
    find("#preview-study").value = study;
    selector.value = settings.standing.pose;
    selector.disabled = study !== "standing";
    find(".preview-pose-field").hidden = study !== "standing";
    find(".preview-occlusion-field").hidden = study === "standing";
    find(".preview-alignment-controls").hidden = study !== "seated-artwork";
    find("#preview-y-label").textContent = study === "seated-artwork" ? "Pivot Y (px)" : study === "seated" ? "Seat / hip (px)" : "Ground (px)";
    find(".preview-study-note").textContent = study === "seated"
      ? "Geometric guide only. The repaired transparent character is available in the seated character study. 100% is the proposed guide size."
      : study === "seated-artwork" ? "Provisional (842, 812), 100% = native scale 0.17. The pivot is not a verified anatomical support point. Toggle alignment guides and bench occlusion to inspect; no correction is applied."
        : "Approved reference: rear standing at (950, 890), 80%, 264px visible height. Walking choices are stills.";
    for (const [id, key] of fields) {
      find(`#preview-${id}`).value = currentSettings()[key];
      find(`#preview-${id}-number`).value = currentSettings()[key];
    }
  }
  find("#preview-study").addEventListener("change", () => {
    study = find("#preview-study").value;
    syncControls();
    render();
  });
  occlusionToggle.addEventListener("change", render);
  alignmentToggle.addEventListener("change", render);
  find("#preview-copy").addEventListener("click", async () => {
    try {
      await navigator.clipboard.writeText(textarea.value);
      copyStatus.textContent = "Settings copied.";
    } catch {
      find(".preview-json").open = true;
      textarea.focus();
      textarea.select();
      copyStatus.textContent = "Clipboard unavailable. The settings are selected below; copy them manually.";
    }
  });
  artwork.addEventListener("load", render);
  artwork.addEventListener("error", render);
  overlay.onload = () => {
    overlayReady = overlay.naturalWidth === ow && overlay.naturalHeight === oh;
    render();
  };
  overlay.onerror = () => { overlayReady = false; render(); };
  overlay.src = seatedConfig.occlusion.file;
  seatedImage.onload = () => {
    seatedImageReady = seatedImage.naturalWidth === seatedAsset.dimensions[0] && seatedImage.naturalHeight === seatedAsset.dimensions[1];
    find(".preview-seated-source-status").textContent = seatedImageReady ? "" : "Seated character dimensions differ from its manifest. Check the repaired file.";
    render();
  };
  seatedImage.onerror = () => {
    seatedImageReady = false;
    find(".preview-seated-source-status").textContent = "Seated character unavailable; geometric guide and standing stills remain available.";
    render();
  };
  seatedImage.src = ASSET_ROOT + seatedAsset.file;
  syncControls();
  loadPose();
}
