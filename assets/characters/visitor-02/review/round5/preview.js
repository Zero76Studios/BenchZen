"use strict";
// Standalone development page: never imports production lifecycle code.
const controls = Object.fromEntries(["x","y","scale","occlusion","guides","fit"].map(id=>[id,document.getElementById(id)]));
const baselineScale = 0.3172413793103448;
const baselineSole = 662.5793103448276;
const baselineHead = 207.02068965517248;
const character = document.getElementById("character");
const viewport = document.getElementById("viewport");
const images = [...document.querySelectorAll(".scene img")];
let failed = false;
function render() {
 const x=Number(controls.x.value), y=Number(controls.y.value), percent=Number(controls.scale.value);
 if(!Number.isFinite(x)||!Number.isFinite(y)||!Number.isFinite(percent)||percent<50||percent>150){document.getElementById("status").textContent="Enter finite X/Y and a scale from 50–150%.";return;}
 const s=280/600*percent/100;
 Object.assign(character.style,{left:`${x-256*s}px`,top:`${y-536*s}px`,width:`${512*s}px`,height:`${768*s}px`});
 document.getElementById("foreground").hidden=!controls.occlusion.checked;
 document.getElementById("overlay").hidden=!controls.guides.checked;
 document.getElementById("contact").setAttribute("d",`M${x-10} ${y}h20 M${x} ${y-10}v20`);
 const sole=y+(baselineSole-536)*s;
 document.getElementById("feet").setAttribute("d",`M${x-65} ${sole}h130`);
 viewport.classList.toggle("fit",controls.fit.checked);
 viewport.style.setProperty("--fit",Math.min(1,viewport.clientWidth/1672));
 const pending=images.some(img=>!img.complete||!img.naturalWidth);
 document.getElementById("status").textContent=failed?"Image loading failed; preview is incomplete.":pending?"Loading preview images…":`Unapproved · hip (${x}, ${y}) · scale ${percent.toFixed(1)}% · estimated soles y${sole.toFixed(1)} (ground 881, gap ${(881-sole).toFixed(1)}px) · head top y${(y+(baselineHead-536)*s).toFixed(1)} · foreground ${controls.occlusion.checked?"on":"off"}`;
}
Object.values(controls).forEach(input=>input.addEventListener("input",render));
document.getElementById("reset").onclick=()=>{controls.x.value=842;controls.y.value=812;controls.scale.value=100;render();};
document.getElementById("ground").onclick=()=>{controls.x.value=842;controls.y.value=812;controls.scale.value=(69/((baselineSole-536)*280/600)*100).toFixed(3);render();};
images.forEach(img=>{img.addEventListener("load",render);img.addEventListener("error",()=>{failed=true;render();});});
window.addEventListener("resize",render);render();
