/* Liturgy — mock interactions. No dependencies, no build step. */
(function () {
  "use strict";

  var R_LABELS = ["Request", "Receive", "Review", "Render", "Rejoice"];
  var reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* Build the canonical-hours 5R dial: 5 arc segments, `filled` of them lit. */
  function buildDial(el) {
    var filled = parseInt(el.getAttribute("data-filled") || "0", 10);
    var size = 120, r = 48, cx = 60, cy = 60;
    var circ = 2 * Math.PI * r;
    var seg = circ / 5;
    var gap = 6; // px gap between segments

    var svg =
      '<svg viewBox="0 0 ' + size + " " + size + '" role="img" aria-label="' +
      filled + ' of 5 movements complete: ' +
      R_LABELS.slice(0, filled).join(", ") + '">';
    svg += '<circle class="dial__track" cx="' + cx + '" cy="' + cy + '" r="' + r + '"/>';
    for (var i = 0; i < 5; i++) {
      var color = i < filled ? "var(--lime)" : (i === filled ? "var(--develop)" : "var(--ink-line)");
      var dash = (seg - gap) + " " + (circ - seg + gap);
      var offset = -i * seg;
      svg +=
        '<circle class="dial__seg" cx="' + cx + '" cy="' + cy + '" r="' + r +
        '" stroke="' + color + '" stroke-dasharray="' + dash +
        '" stroke-dashoffset="' + offset + '"/>';
    }
    svg += "</svg>";
    svg +=
      '<div class="dial__center"><div class="dial__count">' + filled +
      '<span style="color:var(--paper-45);font-size:1rem">/5</span></div>' +
      '<div class="dial__label">' + (el.getAttribute("data-label") || "5R loop") + "</div></div>";
    el.innerHTML = svg;
  }

  function initDials() {
    document.querySelectorAll(".dial").forEach(buildDial);
  }

  /* Lightweight, purely visual: clicking a movement's "Log" button in the 5R
     ritual advances the dial + pips so the mock feels alive. */
  function initRitual() {
    var page = document.querySelector("[data-ritual]");
    if (!page) return;
    page.addEventListener("click", function (e) {
      var btn = e.target.closest("[data-log]");
      if (!btn) return;
      var idx = parseInt(btn.getAttribute("data-log"), 10);
      var next = idx + 1;
      // update movements
      page.querySelectorAll(".movement").forEach(function (m, i) {
        m.classList.toggle("is-done", i <= idx);
        m.classList.toggle("is-current", i === next);
        m.classList.toggle("is-locked", i > next);
      });
      // update dial
      var dial = page.querySelector(".dial");
      if (dial) { dial.setAttribute("data-filled", String(next)); buildDial(dial); }
      // update rlist + pipstrips if present
      page.querySelectorAll(".rlist__item, .pipstrip__pip").forEach(function (n, i) {
        // handled per-container below
      });
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initDials();
    initRitual();
    // stamp current year anywhere requested
    document.querySelectorAll("[data-year]").forEach(function (n) {
      n.textContent = "2026";
    });
  });
})();
