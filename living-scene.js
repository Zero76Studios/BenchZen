"use strict";

// Bench Zen living scene: one visitor enters, walks to the bench, sits, rests,
// stands and leaves, with quiet ambient movement around them.
//
// Everything is drawn from the supplied artwork. Character PNGs are displayed
// as-is (scaled, never redrawn or mirrored). The bench that passes in front of
// the visitor is copied at runtime from the same background pixels.
// Coordinates are background pixels (1672 x 941, top-left origin) and are
// converted to percentages of .scene, so placement survives any resize.
(function () {
  const W = 1672;
  const H = 941;
  const BACKGROUND = "assets/images/paris/paris-clean.png";
  const CHARACTER_ROOT = "assets/characters/visitor-01/";

  // From assets/characters/visitor-01/manifest.json. Kept inline so the normal
  // experience still works when index.html is opened directly from disk.
  // [file, width, height, groundAnchorX, groundAnchorY, retainedHeight]
  const POSES = {
    "standing-rear": ["poses/standing-rear.png", 146, 466, 71, 462, 458],
    "standing-right": ["poses/standing-right.png", 90, 464, 46, 460, 456],
    "walk-1": ["poses/walking-right-01.png", 100, 165, 51, 161, 157],
    "walk-2": ["poses/walking-right-02.png", 90, 165, 53, 161, 157],
    "walk-3": ["poses/walking-right-03.png", 66, 165, 44, 161, 157],
    "walk-4": ["poses/walking-right-04.png", 40, 165, 20, 161, 157],
    "walk-5": ["poses/walking-right-05.png", 97, 165, 48, 161, 157],
    "walk-6": ["poses/walking-right-06.png", 100, 166, 55, 162, 158],
    "walk-7": ["poses/walking-right-07.png", 106, 166, 56, 162, 158],
    "walk-8": ["poses/walking-right-08.png", 100, 166, 56, 162, 158],
  };
  const WALK_CYCLE = ["walk-1", "walk-2", "walk-3", "walk-4", "walk-5", "walk-6", "walk-7", "walk-8"];
  const PASSING_FRAME = 3; // walk-4: upright, legs together. Blends best with standing.

  // Approved standing reference: 80% of 330 px = 264 px visible height.
  const VISIBLE_HEIGHT = 264;

  // Seated artwork and its placement (seated-placement.json baseline).
  const SEATED = {
    file: "seated-rear-transparent.png",
    width: 1024, height: 1536,
    anchorX: 512, anchorY: 1015, // lower clothing edge resting on the seat
    footY: 1422,                 // bottom of the shoes
    headTopY: 58,
    scale: 0.17,
    x: 842, y: 812,
  };

  // The visitor walks along the strip of gravel just beyond the bench (between
  // the bench and the lawn kerb), so the bench passes in front of them.
  const ROUTE = {
    groundY: 881,     // same ground line as the seated shoes
    enterX: -70,
    benchX: 842,
    exitX: W + 70,
    speed: 168,       // background px per second: an unhurried stroll
    stride: 28,       // background px travelled per walk frame (keeps feet planted)
  };

  // Bench pieces measured from paris-clean.png: [x, y, width, height].
  const BENCH = {
    region: [640, 736, 404, 160],
    solid: [
      [645, 738, 393, 20],  // top backrest slat
      [645, 761, 393, 18],  // middle slat
      [645, 783, 393, 17],  // lower slat
      [646, 814, 392, 10],  // dark rail along the near edge of the seat
    ],
    // Narrow uprights and legs: only their dark pixels are kept.
    dark: [
      [662, 738, 19, 76],   // left backrest post
      [1004, 738, 17, 76],  // right backrest post
      [660, 824, 36, 70],   // left legs
      [990, 824, 44, 70],   // right legs
    ],
    // The seat board. In front of someone walking behind the bench, but
    // underneath someone sitting on it, so it is a separate switchable piece.
    seat: [668, 800, 348, 14],
    darkThreshold: 78,
  };

  const TIMING = {
    firstArrival: [1500, 2500],
    rest: [75000, 120000],
    emptyBench: [12000, 20000],
    sit: 1500,
    stand: 1700,
  };
  const FAST_TIMING = { firstArrival: [400, 600], rest: [14000, 16000], emptyBench: [2500, 3500], sit: 1500, stand: 1700 };

  const pctX = value => `${value / W * 100}%`;
  const pctY = value => `${value / H * 100}%`;
  const between = ([low, high]) => low + Math.random() * (high - low);
  const clamp01 = value => Math.max(0, Math.min(1, value));
  const ease = t => t * t * (3 - 2 * t);
  const wait = ms => new Promise(resolve => setTimeout(resolve, ms));

  function loadImage(src) {
    return new Promise((resolve, reject) => {
      const image = new Image();
      image.decoding = "async";
      image.onload = () => resolve(image);
      image.onerror = () => reject(new Error(`Missing artwork: ${src}`));
      image.src = src;
    });
  }

  // Load every piece before anything changes on screen. If one is missing the
  // caller keeps the original static artwork instead.
  async function prepare() {
    const poseIds = Object.keys(POSES);
    const [background, seated, ...poseImages] = await Promise.all([
      loadImage(BACKGROUND),
      loadImage(CHARACTER_ROOT + SEATED.file),
      ...poseIds.map(id => loadImage(CHARACTER_ROOT + POSES[id][0])),
    ]);
    if (background.naturalWidth !== W || background.naturalHeight !== H) throw new Error("Unexpected background size");
    if (seated.naturalWidth !== SEATED.width || seated.naturalHeight !== SEATED.height) throw new Error("Unexpected seated artwork size");
    poseIds.forEach((id, index) => {
      if (poseImages[index].naturalWidth !== POSES[id][1] || poseImages[index].naturalHeight !== POSES[id][2]) {
        throw new Error(`Unexpected pose size: ${id}`);
      }
    });
    return { background };
  }

  function place(element, x, y, width, height) {
    element.style.left = pctX(x);
    element.style.top = pctY(y);
    element.style.width = pctX(width);
    element.style.height = pctY(height);
  }

  // Copy bench pixels from the background into a transparent canvas that sits
  // in front of the visitor. Nothing is drawn by hand.
  function benchCanvas(background, solidRects, darkRects) {
    const [rx, ry, rw, rh] = BENCH.region;
    const canvas = document.createElement("canvas");
    canvas.width = rw;
    canvas.height = rh;
    canvas.className = "bench-occluder";
    place(canvas, rx, ry, rw, rh);
    const context = canvas.getContext("2d");
    for (const [x, y, w, h] of darkRects) {
      context.drawImage(background, x, y, w, h, x - rx, y - ry, w, h);
      try {
        const pixels = context.getImageData(x - rx, y - ry, w, h);
        const data = pixels.data;
        for (let i = 0; i < data.length; i += 4) {
          if (Math.max(data[i], data[i + 1], data[i + 2]) >= BENCH.darkThreshold) data[i + 3] = 0;
        }
        context.putImageData(pixels, x - rx, y - ry);
      } catch {
        // Opened straight from disk: pixels cannot be read, so the whole
        // rectangle stays. Slightly less exact around the legs, still correct.
      }
    }
    for (const [x, y, w, h] of solidRects) context.drawImage(background, x, y, w, h, x - rx, y - ry, w, h);
    return canvas;
  }

  function buildVisitor(characterLayer) {
    // The visitor element covers the scene, with the ground point at its
    // top-left corner. Moving it is then a single percentage translate.
    const visitor = document.createElement("div");
    visitor.className = "visitor";
    const frames = {};
    for (const [id, [file, width, height, anchorX, anchorY, retained]] of Object.entries(POSES)) {
      const scale = VISIBLE_HEIGHT / retained;
      // The extracted stills are slightly see-through (source alpha 200-254).
      // Stacking the same PNG three times makes the silhouette solid without
      // editing or redrawing any pixels.
      const image = document.createElement("div");
      image.className = "visitor__pose";
      const url = `url("${CHARACTER_ROOT + file}")`;
      image.style.backgroundImage = `${url}, ${url}, ${url}`;
      place(image, -anchorX * scale, -anchorY * scale, width * scale, height * scale);
      frames[id] = image;
      visitor.append(image);
    }

    // Seated artwork, split into three stacked copies of the same PNG so the
    // head and upper body can move a little while the legs stay planted.
    const seated = document.createElement("div");
    seated.className = "seated";
    const s = SEATED.scale;
    place(seated, SEATED.x - SEATED.anchorX * s, SEATED.y - SEATED.anchorY * s, SEATED.width * s, SEATED.height * s);
    seated.style.transformOrigin = `50% ${SEATED.footY / SEATED.height * 100}%`;
    const part = className => {
      const image = new Image();
      image.className = `seated__part ${className}`;
      image.alt = "";
      image.draggable = false;
      image.src = CHARACTER_ROOT + SEATED.file;
      return image;
    };
    const upper = document.createElement("div");
    upper.className = "seated__upper";
    const breath = document.createElement("div");
    breath.className = "seated__breath";
    const head = part("seated__head");
    breath.append(part("seated__torso"), head);
    upper.append(breath);
    seated.append(part("seated__legs"), upper);

    characterLayer.append(visitor, seated);
    return { visitor, frames, seated, upper, head };
  }

  function buildAtmosphere(layer) {
    const glow = (className, x, y, radius, delay) => {
      const element = document.createElement("span");
      element.className = `glow ${className}`;
      place(element, x - radius, y - radius, radius * 2, radius * 2);
      element.style.animationDelay = `${delay}s`;
      layer.append(element);
    };
    // Lanterns and the low sun already painted in the artwork.
    glow("glow--lamp", 160, 283, 92, 0);
    glow("glow--lamp", 358, 578, 36, -3.1);
    glow("glow--lamp", 1607, 573, 36, -5.4);
    glow("glow--lamp", 594, 637, 18, -1.7);
    glow("glow--lamp", 1413, 628, 18, -6.6);
    glow("glow--sun", 467, 551, 80, 0);

    // A few pin-point twinkles on the tower's existing lights.
    const sparkles = [[1079, 70], [1074, 120], [1084, 170], [1072, 225], [1088, 280], [1066, 335], [1094, 360],
      [1052, 400], [1110, 420], [1040, 470], [1122, 485], [1012, 540], [1150, 545], [985, 590], [1178, 592],
      [962, 628], [1200, 630]];
    sparkles.forEach(([x, y], index) => {
      const dot = document.createElement("span");
      dot.className = "sparkle";
      dot.style.left = pctX(x);
      dot.style.top = pctY(y);
      dot.style.animationDuration = `${4.2 + (index * 1.37) % 3.4}s`;
      dot.style.animationDelay = `${-((index * 2.9) % 7)}s`;
      layer.append(dot);
    });

    // Two distant birds drift across now and then, as in the approved artwork.
    const flock = document.createElement("div");
    flock.className = "flock";
    for (const [x, y, delay] of [[0, 0, 0], [36, 22, -0.4]]) {
      const bird = document.createElementNS("http://www.w3.org/2000/svg", "svg");
      bird.setAttribute("viewBox", "0 0 24 10");
      bird.setAttribute("class", "flock__bird");
      bird.style.left = pctX(x);
      bird.style.top = pctY(y);
      bird.style.animationDelay = `${delay}s`;
      const wings = document.createElementNS("http://www.w3.org/2000/svg", "path");
      wings.setAttribute("d", "M1 7 Q7 0 12 6 Q17 0 23 7 Q17 3.5 12 8 Q7 3.5 1 7 Z");
      bird.append(wings);
      flock.append(bird);
    }
    layer.append(flock);
  }

  function start({ scene, background }) {
    const params = new URLSearchParams(window.location.search);
    const timing = params.get("pace") === "fast" ? FAST_TIMING : TIMING;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    const { visitor, frames, seated, upper, head } = buildVisitor(scene.querySelector('[data-layer="character"]'));
    const foreground = scene.querySelector('[data-layer="foreground"]');
    const bench = benchCanvas(background, BENCH.solid, BENCH.dark);
    const seatBoard = benchCanvas(background, [BENCH.seat], []);
    foreground.append(seatBoard, bench);
    buildAtmosphere(scene.querySelector('[data-layer="atmosphere"]'));
    scene.classList.add("scene--living");

    const state = { x: ROUTE.enterX, squash: 1, pose: null };
    const setStage = stage => { scene.dataset.visitor = stage; };

    function render() {
      visitor.style.transform = `translate3d(${state.x / W * 100}%, ${ROUTE.groundY / H * 100}%, 0) scaleY(${state.squash})`;
    }
    function setPose(id) {
      if (state.pose === id) return;
      if (state.pose) frames[state.pose].classList.remove("is-shown");
      if (id) frames[id].classList.add("is-shown");
      state.pose = id;
    }
    const nextFrame = () => new Promise(resolve => requestAnimationFrame(resolve));

    async function tween(duration, step) {
      const begin = await nextFrame();
      for (;;) {
        const t = clamp01((await nextFrame() - begin) / duration);
        step(t);
        if (t >= 1) return;
      }
    }

    // Walk frames advance with distance, not time, so feet stay planted even
    // while the visitor slows down or sets off.
    async function walk(from, to, { endFrame = null, startFrame = 0, easeIn = false, easeOut = false } = {}) {
      const total = Math.abs(to - from);
      const direction = Math.sign(to - from);
      const steps = Math.floor(total / ROUTE.stride);
      const offset = endFrame === null ? startFrame : (((endFrame - steps) % 8) + 8) % 8;
      let travelled = 0;
      let last = await nextFrame();
      while (travelled < total) {
        const now = await nextFrame();
        const dt = Math.min(100, now - last) / 1000; // a hidden tab resumes where it stopped
        last = now;
        let speed = ROUTE.speed;
        if (easeIn) speed *= 0.4 + 0.6 * clamp01(travelled / 60);
        if (easeOut) speed *= 0.4 + 0.6 * clamp01((total - travelled) / 80);
        travelled = Math.min(total, travelled + speed * dt);
        state.x = from + direction * travelled;
        setPose(WALK_CYCLE[(offset + Math.floor(travelled / ROUTE.stride)) % 8]);
        render();
      }
    }

    // No sit-down drawings exist without a bench painted over them, so sitting
    // is a short blend between the rear standing pose and the seated artwork.
    // Both are stretched slightly so heads meet and feet stay on the ground.
    const seatedVisible = (SEATED.footY - SEATED.headTopY) * SEATED.scale;
    const standingSquash = seatedVisible / VISIBLE_HEIGHT;
    const seatedStretch = VISIBLE_HEIGHT / seatedVisible;

    function blend(amount) { // 0 = standing, 1 = seated
      const e = ease(amount);
      state.squash = 1 + (standingSquash - 1) * e;
      seated.style.transform = `scaleY(${seatedStretch + (1 - seatedStretch) * e})`;
      seated.style.opacity = clamp01((amount - 0.36) / 0.2);
      visitor.style.opacity = 1 - clamp01((amount - 0.5) / 0.2);
      seatBoard.hidden = amount > 0.5;
      render();
    }

    // Idle life while resting. Breathing is a CSS loop; these are occasional.
    let resting = false;
    const timers = new Set();
    function later(ms, action) {
      const id = setTimeout(() => { timers.delete(id); if (resting) action(); }, ms);
      timers.add(id);
    }
    function headMoves() {
      later(between([9000, 22000]), () => {
        const turn = (Math.random() < 0.5 ? -1 : 1) * between([1.4, 3.2]);
        head.style.setProperty("--head-turn", `${turn}deg`);
        head.style.setProperty("--head-shift", `${turn * 0.12}%`);
        later(between([3500, 9000]), () => {
          head.style.setProperty("--head-turn", "0deg");
          head.style.setProperty("--head-shift", "0%");
          headMoves();
        });
      });
    }
    function postureShifts() {
      later(between([18000, 40000]), () => {
        upper.style.setProperty("--lean", `${(Math.random() < 0.5 ? -1 : 1) * between([0.35, 0.8])}deg`);
        upper.style.setProperty("--settle", `${between([0, 0.25])}%`);
        postureShifts();
      });
    }
    function startResting() {
      resting = true;
      seated.classList.add("is-resting");
      headMoves();
      postureShifts();
    }
    async function stopResting() {
      resting = false;
      timers.forEach(clearTimeout);
      timers.clear();
      for (const [element, name, value] of [[head, "--head-turn", "0deg"], [head, "--head-shift", "0%"],
        [upper, "--lean", "0deg"], [upper, "--settle", "0%"]]) element.style.setProperty(name, value);
      await wait(1800); // let the head and shoulders return before standing
      seated.classList.remove("is-resting");
    }

    async function visit() {
      setStage("entering");
      visitor.style.opacity = 1;
      seated.style.opacity = 0;
      seatBoard.hidden = false;
      state.squash = 1;
      await walk(ROUTE.enterX, ROUTE.benchX, { endFrame: PASSING_FRAME, easeOut: true });
      setPose("standing-right");
      await wait(450);
      setPose("standing-rear");
      await wait(900);

      setStage("sitting");
      await tween(timing.sit, blend);
      setStage("resting");
      startResting();
      await wait(between(timing.rest));
      await stopResting();

      setStage("standing");
      await tween(timing.stand, t => blend(1 - t));
      await wait(1100);
      setPose("standing-right");
      await wait(500);

      setStage("leaving");
      await walk(ROUTE.benchX, ROUTE.exitX, { startFrame: PASSING_FRAME + 1, easeIn: true });
      setPose(null);
      setStage("away");
    }

    if (reduceMotion) {
      // Still scene: the visitor is simply sitting, as in the approved artwork.
      visitor.style.opacity = 0;
      seated.style.opacity = 1;
      seatBoard.hidden = true;
      setStage("resting");
      return;
    }

    (async () => {
      setStage("away");
      seated.style.opacity = 0;
      await wait(between(timing.firstArrival));
      for (;;) {
        await visit();
        await wait(between(timing.emptyBench));
      }
    })();
  }

  window.BenchZenLivingScene = { prepare, start };
})();
