"use strict";

const experience = document.getElementById("experience");
const scene = document.getElementById("scene");
const artwork = document.getElementById("paris-artwork");
const placeholder = document.getElementById("artwork-placeholder");
const fullscreenButton = document.getElementById("fullscreen-toggle");
const fullscreenStatus = document.getElementById("fullscreen-status");

// Development tools and alternate artwork are loaded only by this explicit flag.
if (new URLSearchParams(window.location.search).get("preview") === "character") {
  artwork.dataset.src = "assets/images/paris/paris-clean.png";
  placeholder.querySelector("code").textContent = artwork.dataset.src;
  const previewStyles = document.createElement("link");
  previewStyles.rel = "stylesheet";
  previewStyles.href = "character-preview.css";
  document.head.append(previewStyles);
  import("./character-preview.js")
    .then(({ startCharacterPreview }) => startCharacterPreview({ experience, scene, artwork }))
    .catch(() => {
      const message = document.createElement("p");
      message.className = "preview-start-error";
      message.setAttribute("role", "status");
      message.textContent = "Character preview could not start. Run node tools/serve.cjs from BenchZen and open http://localhost:8000/?preview=character.";
      experience.append(message);
    });
}

// Keep missing or unreadable artwork hidden so no broken-image icon appears.
artwork.addEventListener("load", () => {
  scene.style.setProperty("--scene-ratio", artwork.naturalWidth / artwork.naturalHeight);
  artwork.hidden = false;
  placeholder.hidden = true;
});
artwork.addEventListener("error", () => {
  artwork.hidden = true;
  placeholder.hidden = false;
});
artwork.src = artwork.dataset.src;

// Some mobile browsers do not support element fullscreen. Hide the control there.
if (document.fullscreenEnabled && typeof experience.requestFullscreen === "function") {
  fullscreenButton.hidden = false;
}

fullscreenButton.addEventListener("click", async () => {
  fullscreenStatus.textContent = "";
  try {
    if (document.fullscreenElement) {
      await document.exitFullscreen();
    } else {
      await experience.requestFullscreen();
    }
  } catch {
    fullscreenStatus.textContent = "Fullscreen is unavailable. You can continue enjoying this view here.";
  }
});

// Also update when the browser exits fullscreen via Escape or its own controls.
document.addEventListener("fullscreenchange", () => {
  const isFullscreen = document.fullscreenElement === experience;
  fullscreenButton.textContent = isFullscreen ? "Exit fullscreen" : "Enter fullscreen";
  fullscreenButton.setAttribute("aria-pressed", String(isFullscreen));
});
