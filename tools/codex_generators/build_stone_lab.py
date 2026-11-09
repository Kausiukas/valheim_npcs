import os
import codex_styles

OUT_FILE = r"d:\valheim_mods\docs\stone_carving_lab.html"
MIRROR_FILE = r"d:\valheim_mods\ValheimWorkers\stone_carving_lab.html"

HTML_HEAD = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Viking Stone Carving &amp; Rust Typography Lab</title>
  <style>
    @font-face {{
      font-family: 'Norsebold';
      src: url('{codex_styles.norsebold_b64}') format('opentype');
      font-weight: bold;
      font-style: normal;
    }}
    @font-face {{
      font-family: 'Norse';
      src: url('{codex_styles.norse_b64}') format('opentype');
      font-weight: normal;
      font-style: normal;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('{codex_styles.averia_bold_b64}') format('truetype');
      font-weight: bold;
      font-style: normal;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre Regular';
      src: url('{codex_styles.averia_regular_b64}') format('truetype');
      font-weight: normal;
      font-style: normal;
    }}

    :root {{
      --bg-dark: #0f1217;
      --panel-bg: rgba(16, 20, 27, 0.94);
      --border-stone: #3d301d;
      --border-gold: #c28728;
      --border-rust: #d97232;
      --text-gold: #f7d57f;
      --text-rust: #e07534;
      --text-main: #ede2d0;
      --text-muted: #a89885;
      --stone-monolith-url: url('{codex_styles.stone_monolith_b64}');
      --stone-tablet-url: url('{codex_styles.stone_tablet_b64}');
    }}

    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}

    body {{
      background-color: var(--bg-dark);
      background-image: var(--stone-monolith-url);
      background-repeat: repeat;
      background-size: 512px 512px;
      color: var(--text-main);
      font-family: 'Averia Serif Libre', Georgia, serif;
      min-height: 100vh;
      display: flex;
      flex-direction: column;
      overflow-x: hidden;
    }}

    /* Top Navigation Bar */
    header.lab-top {{
      background: rgba(10, 13, 18, 0.92);
      border-bottom: 2px solid #4a381d;
      padding: 12px 28px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      backdrop-filter: blur(8px);
      position: sticky;
      top: 0;
      z-index: 100;
    }}
    .brand {{
      display: flex;
      align-items: center;
      gap: 12px;
    }}
    .brand-icon {{
      width: 34px;
      height: 34px;
      background: #1b1610;
      border: 1px solid #7a5820;
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.3rem;
    }}
    .brand-titles h1 {{
      font-family: 'Norsebold', serif;
      font-size: 1.35rem;
      color: #f7d57f;
      letter-spacing: 0.05em;
      line-height: 1.1;
    }}
    .brand-titles p {{
      font-size: 0.78rem;
      color: #8c7d6e;
    }}
    .top-actions {{
      display: flex;
      align-items: center;
      gap: 12px;
    }}
    .btn-top {{
      background: #201912;
      border: 1px solid #735323;
      color: #ede2d0;
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-size: 0.85rem;
      font-weight: bold;
      padding: 6px 14px;
      border-radius: 4px;
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      gap: 6px;
      transition: all 0.2s ease;
    }}
    .btn-top:hover {{
      background: #3b2c18;
      border-color: #d97232;
      color: #ff9b5a;
    }}
    .btn-top.btn-primary {{
      background: #3b1d0c;
      border-color: #d97232;
      color: #ff9b5a;
    }}
    .btn-top.btn-primary:hover {{
      background: #542910;
      box-shadow: 0 0 10px rgba(217, 114, 50, 0.4);
    }}

    /* Main Workspace Layout */
    .lab-workspace {{
      flex: 1;
      display: flex;
      flex-direction: column;
      padding: 24px;
      max-width: 1560px;
      width: 100%;
      margin: 0 auto;
      gap: 20px;
    }}

    /* Quick Preset Chips Row */
    .presets-bar {{
      background: rgba(14, 18, 24, 0.88);
      border: 1px solid #3c301c;
      border-radius: 8px;
      padding: 12px 18px;
      display: flex;
      align-items: center;
      gap: 10px;
      overflow-x: auto;
      box-shadow: 0 4px 12px rgba(0,0,0,0.6);
    }}
    .presets-label {{
      font-family: 'Norsebold', serif;
      font-size: 0.95rem;
      color: #e59850;
      letter-spacing: 0.04em;
      white-space: nowrap;
      margin-right: 4px;
    }}
    .preset-chip {{
      background: #171c24;
      border: 1px solid #473822;
      color: #c4b5a0;
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-size: 0.82rem;
      font-weight: bold;
      padding: 6px 14px;
      border-radius: 20px;
      cursor: pointer;
      white-space: nowrap;
      transition: all 0.2s ease;
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }}
    .preset-chip:hover {{
      background: #2b2216;
      border-color: #d97232;
      color: #f7d57f;
    }}
    .preset-chip.active {{
      background: #3d1f0e;
      border-color: #d97232;
      color: #ff9b5a;
      box-shadow: 0 0 10px rgba(217, 114, 50, 0.35);
    }}

    /* The Hero Slab Live Preview Stage */
    .preview-stage-container {{
      background: #0d1015;
      border: 2px solid #523f22;
      border-radius: 8px;
      padding: 40px 32px;
      position: relative;
      box-shadow: 0 10px 30px rgba(0,0,0,0.9), inset 0 2px 12px rgba(0,0,0,0.8);
      background-image: var(--stone-monolith-url);
      background-size: 380px 380px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 280px;
      transition: background-image 0.3s ease;
      overflow: hidden;
    }}
    .preview-stage-container::before {{
      content: '';
      position: absolute;
      inset: 0;
      background: radial-gradient(circle at center, rgba(16, 20, 26, 0.2) 0%, rgba(8, 10, 14, 0.85) 100%);
      pointer-events: none;
    }}
    .stage-badge {{
      position: absolute;
      top: 14px;
      left: 18px;
      font-size: 0.74rem;
      font-weight: bold;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      background: rgba(0, 0, 0, 0.7);
      border: 1px solid #4a3a22;
      padding: 3px 10px;
      border-radius: 4px;
      color: #8c7d6e;
    }}
    .bg-switcher {{
      position: absolute;
      top: 14px;
      right: 18px;
      display: flex;
      gap: 6px;
      z-index: 10;
    }}
    .btn-bg {{
      background: rgba(0, 0, 0, 0.7);
      border: 1px solid #4a3a22;
      color: #a89885;
      font-size: 0.75rem;
      padding: 3px 10px;
      border-radius: 4px;
      cursor: pointer;
    }}
    .btn-bg.active {{
      border-color: #d97232;
      color: #ff9b5a;
    }}

    /* The Live Inscription Display */
    .live-heading-wrapper {{
      position: relative;
      z-index: 5;
      max-width: 100%;
      text-align: center;
      padding: 20px 10px;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 16px;
    }}
    .live-glyph {{
      font-size: 2.2rem;
      line-height: 1;
      filter: drop-shadow(0 3px 5px rgba(0,0,0,0.8));
      flex-shrink: 0;
    }}
    .live-heading {{
      display: inline-block;
      line-height: 1.15;
      word-break: break-word;
      user-select: all;
    }}

    /* Sub-text / Description in Slab */
    .live-subtext {{
      position: relative;
      z-index: 5;
      max-width: 820px;
      text-align: center;
      font-size: 1.05rem;
      color: #c4b5a0;
      line-height: 1.5;
      margin-top: 6px;
      text-shadow: 1px 1px 2px #000;
    }}

    /* Comparison Split Stage */
    .split-baseline-box {{
      width: 100%;
      max-width: 1100px;
      background: rgba(14, 18, 24, 0.75);
      border: 1px solid #332717;
      border-radius: 6px;
      padding: 10px 18px;
      margin-top: 24px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 14px;
      position: relative;
      z-index: 5;
    }}
    .baseline-label {{
      font-size: 0.75rem;
      font-weight: bold;
      color: #8c7d6e;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      flex-shrink: 0;
    }}
    .baseline-text {{
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      font-size: 1.25rem;
      letter-spacing: 0.05em;
      filter: url(#v-groove-gold-baseline);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }}

    /* Bottom Controller Dock */
    .dock-wrap {{
      background: var(--panel-bg);
      border: 2px solid #4a381d;
      border-radius: 8px;
      box-shadow: 0 8px 30px rgba(0, 0, 0, 0.85);
      overflow: hidden;
      backdrop-filter: blur(12px);
    }}

    /* Dock Tabs */
    .dock-tabs {{
      display: flex;
      background: #11151c;
      border-bottom: 2px solid #3d2f1b;
      overflow-x: auto;
    }}
    .dock-tab {{
      flex: 1;
      min-width: 130px;
      background: transparent;
      border: none;
      border-right: 1px solid #2d2315;
      color: #8c7d6e;
      font-family: 'Norsebold', serif;
      font-size: 1.05rem;
      letter-spacing: 0.04em;
      padding: 12px 16px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      transition: all 0.2s ease;
    }}
    .dock-tab:hover {{
      background: rgba(40, 30, 18, 0.5);
      color: #f7d57f;
    }}
    .dock-tab.active {{
      background: #1c1813;
      color: #ff9b5a;
      border-bottom: 2px solid #d97232;
      box-shadow: inset 0 2px 0 #d97232;
    }}

    /* Tab Panels */
    .dock-panels {{
      padding: 22px 28px;
    }}
    .dock-panel {{
      display: none;
    }}
    .dock-panel.active {{
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(310px, 1fr));
      gap: 20px 28px;
      animation: fadeIn 0.2s ease-out;
    }}
    @keyframes fadeIn {{
      from {{ opacity: 0; }}
      to {{ opacity: 1; }}
    }}

    /* Control Groups */
    .ctl-group {{
      display: flex;
      flex-direction: column;
      gap: 8px;
    }}
    .ctl-header {{
      display: flex;
      justify-content: space-between;
      align-items: baseline;
    }}
    .ctl-label {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
      font-size: 0.88rem;
      color: #d1c2ab;
    }}
    .ctl-val {{
      font-size: 0.82rem;
      color: #d97232;
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
    }}
    .range-slider {{
      -webkit-appearance: none;
      appearance: none;
      width: 100%;
      height: 6px;
      background: #1c222c;
      border: 1px solid #4a3a22;
      border-radius: 3px;
      outline: none;
    }}
    .range-slider::-webkit-slider-thumb {{
      -webkit-appearance: none;
      appearance: none;
      width: 16px;
      height: 16px;
      border-radius: 50%;
      background: #d97232;
      border: 2px solid #fff;
      cursor: pointer;
      box-shadow: 0 0 6px rgba(217, 114, 50, 0.6);
      transition: transform 0.15s ease;
    }}
    .range-slider::-webkit-slider-thumb:hover {{
      transform: scale(1.2);
    }}

    /* Segmented Radio Group */
    .seg-group {{
      display: flex;
      background: #12161e;
      border: 1px solid #3c301c;
      border-radius: 4px;
      overflow: hidden;
      padding: 2px;
      gap: 2px;
    }}
    .seg-opt {{
      flex: 1;
      text-align: center;
      padding: 6px 10px;
      font-size: 0.82rem;
      font-weight: bold;
      color: #a89885;
      cursor: pointer;
      border-radius: 3px;
      user-select: none;
      transition: all 0.2s ease;
    }}
    .seg-opt.active {{
      background: #3d1f0e;
      color: #ff9b5a;
      box-shadow: 0 0 6px rgba(217, 114, 50, 0.3);
    }}

    /* Color Swatches Grid */
    .swatches-row {{
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
    }}
    .swatch-btn {{
      width: 28px;
      height: 28px;
      border-radius: 4px;
      border: 1.5px solid #473822;
      cursor: pointer;
      transition: transform 0.15s ease, border-color 0.15s ease;
    }}
    .swatch-btn:hover {{
      transform: scale(1.15);
      border-color: #fff;
    }}
    .swatch-btn.active {{
      border-color: #fff;
      box-shadow: 0 0 8px rgba(255, 255, 255, 0.5);
    }}
    .color-picker-input {{
      height: 28px;
      width: 48px;
      background: #12161e;
      border: 1px solid #473822;
      border-radius: 4px;
      padding: 2px;
      cursor: pointer;
    }}

    /* Text Input */
    .lab-text-input {{
      width: 100%;
      background: #12161e;
      border: 1px solid #473822;
      border-radius: 4px;
      padding: 8px 12px;
      color: #fff;
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-size: 0.95rem;
      outline: none;
    }}
    .lab-text-input:focus {{
      border-color: #d97232;
      box-shadow: 0 0 8px rgba(217, 114, 50, 0.3);
    }}

    /* Code Export Modal / Area */
    .code-export-box {{
      margin-top: 16px;
      background: #090c10;
      border: 1px solid #332717;
      border-radius: 6px;
      padding: 14px;
      position: relative;
    }}
    .code-export-box pre {{
      font-family: 'Consolas', monospace;
      font-size: 0.8rem;
      color: #7ad9f5;
      overflow-x: auto;
      max-height: 180px;
    }}
    .btn-copy-code {{
      position: absolute;
      top: 10px;
      right: 10px;
      background: #2b1c10;
      border: 1px solid #d97232;
      color: #ff9b5a;
      font-size: 0.78rem;
      font-weight: bold;
      padding: 4px 10px;
      border-radius: 4px;
      cursor: pointer;
    }}
    .btn-copy-code:hover {{
      background: #472612;
    }}

    /* Toast notification */
    .toast {{
      position: fixed;
      bottom: 24px;
      right: 24px;
      background: #3b1d0c;
      border: 1px solid #d97232;
      color: #fff;
      padding: 10px 18px;
      border-radius: 4px;
      font-size: 0.9rem;
      font-weight: bold;
      box-shadow: 0 4px 14px rgba(0,0,0,0.8);
      opacity: 0;
      pointer-events: none;
      transition: opacity 0.3s ease;
      z-index: 1000;
    }}
    .toast.show {{
      opacity: 1;
    }}
  </style>
</head>
<body>

<!-- Dynamic Live SVG Filters -->
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
    <!-- Baseline Gold Chisel for Comparison -->
    <filter id="v-groove-gold-baseline" x="-15%" y="-15%" width="130%" height="130%">
      <feTurbulence type="fractalNoise" baseFrequency="0.09" numOctaves="3" seed="7" result="noise"/>
      <feDisplacementMap in="SourceGraphic" in2="noise" scale="2.0" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap in="SourceAlpha" in2="noise" scale="2.0" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="0.8" result="b1"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="2.0" result="b2"/>
      <feComposite in="b1" in2="b2" operator="arithmetic" k1="0" k2="0.5" k3="0.5" k4="0" result="heightMap"/>
      <feDiffuseLighting in="heightMap" surfaceScale="-12" diffuseConstant="1.3" lighting-color="#fff4db" result="diffLight">
        <feDistantLight azimuth="315" elevation="38"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.15" k2="0.15" k3="0" k4="-0.06" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>
      <feSpecularLighting in="heightMap" surfaceScale="-12" specularConstant="0.9" specularExponent="20" lighting-color="#fff8d6" result="specLight">
        <feDistantLight azimuth="315" elevation="38"/>
      </feSpecularLighting>
      <feComposite in="specLight" in2="wobbleAlpha" operator="in" result="clippedSpec"/>
      <feOffset in="wobbleAlpha" dx="1.6" dy="1.6" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="topLeftCrescent"/>
      <feFlood flood-color="#020305" flood-opacity="0.95" result="blackFlood"/>
      <feComposite in="blackFlood" in2="topLeftCrescent" operator="in" result="innerRimShadow"/>
      <feOffset in="wobbleAlpha" dx="1.4" dy="1.4" result="outerLipOff"/>
      <feComposite in="outerLipOff" in2="wobbleAlpha" operator="out" result="outerBottomRightLip"/>
      <feFlood flood-color="#ffeab5" flood-opacity="0.65" result="lipFlood"/>
      <feComposite in="lipFlood" in2="outerBottomRightLip" operator="in" result="outerLipHighlight"/>
      <feMerge>
        <feMergeNode in="outerLipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="innerRimShadow"/>
        <feMergeNode in="clippedSpec"/>
      </feMerge>
    </filter>

    <!-- LIVE DYNAMIC CHISEL FILTER -->
    <filter id="live-chisel-filter" x="-20%" y="-20%" width="140%" height="140%" color-interpolation-filters="sRGB">
      <feTurbulence id="f-turbulence" type="fractalNoise" baseFrequency="0.09" numOctaves="2" seed="5" result="noise"/>
      <feDisplacementMap id="f-displace-src" in="SourceGraphic" in2="noise" scale="0" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap id="f-displace-alpha" in="SourceAlpha" in2="noise" scale="0" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>
      
      <feGaussianBlur id="f-blur" in="wobbleAlpha" stdDeviation="0.9" result="blurAlpha"/>
      
      <feDiffuseLighting id="f-diffuse" in="blurAlpha" surfaceScale="-8" diffuseConstant="1.2" lighting-color="#ffeedd" result="diffLight">
        <feDistantLight id="f-light-diff" azimuth="315" elevation="45"/>
      </feDiffuseLighting>
      <feComposite id="f-comp-diff" in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>
      
      <feSpecularLighting id="f-specular" in="blurAlpha" surfaceScale="-8" specularConstant="0" specularExponent="20" lighting-color="#ffffff" result="specLight">
        <feDistantLight id="f-light-spec" azimuth="315" elevation="45"/>
      </feSpecularLighting>
      <feComposite in="specLight" in2="wobbleAlpha" operator="in" result="clippedSpec"/>
      
      <!-- Inset Carve Shadow (Top-Left) -->
      <feOffset id="f-offset-sh" in="wobbleAlpha" dx="1.8" dy="1.8" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood id="f-flood-sh" flood-color="#050302" flood-opacity="0.95" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      
      <!-- Outer Rim Lip Highlight (Bottom-Right) -->
      <feOffset id="f-offset-lip" in="wobbleAlpha" dx="-0.8" dy="-0.8" result="lipOff"/>
      <feComposite in="lipOff" in2="wobbleAlpha" operator="out" result="bLip"/>
      <feFlood id="f-flood-lip" flood-color="#e89862" flood-opacity="0.5" result="lipFlood"/>
      <feComposite in="lipFlood" in2="bLip" operator="in" result="lipHighlight"/>
      
      <feMerge id="f-merge">
        <feMergeNode in="lipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="carveShadow"/>
        <feMergeNode id="f-merge-spec" in="clippedSpec"/>
      </feMerge>
    </filter>
  </defs>
</svg>

<!-- Header -->
<header class="lab-top">
  <div class="brand">
    <div class="brand-icon">⚒️</div>
    <div class="brand-titles">
      <h1>VIKING STONE CARVING &amp; RUST LAB</h1>
      <p>Interactive Typography, Relief &amp; Patina Controller &bull; ValheimWorkers Project</p>
    </div>
  </div>
  <div class="top-actions">
    <button class="btn-top" id="btn-export-css">📋 Copy CSS Rule</button>
    <button class="btn-top btn-primary" id="btn-apply-active">⚡ Set as Project Style</button>
  </div>
</header>

<!-- Main Workspace -->
<main class="lab-workspace">

  <!-- Preset Chips -->
  <div class="presets-bar">
    <span class="presets-label">⚡ STYLES:</span>
    <button class="preset-chip active" data-preset="bog-rust">🍂 Bog Iron Rust (Matte)</button>
    <button class="preset-chip" data-preset="red-ochre">🔴 Red Ochre Runestone</button>
    <button class="preset-chip" data-preset="weathered-bronze">🥉 Weathered Bronze (Averia)</button>
    <button class="preset-chip" data-preset="crisp-inset">⚔️ Crisp Inset Rust (Sharp)</button>
    <button class="preset-chip" data-preset="hammered-iron">🔨 Hammered Iron + Rust Bevel</button>
    <button class="preset-chip" data-preset="sandstone">🏛️ Weathered Sandstone</button>
    <button class="preset-chip" data-preset="charcoal-ash">🌑 Charcoal &amp; Ash</button>
    <button class="preset-chip" data-preset="shiny-baseline">✨ Current Shiny Gold (Baseline)</button>
  </div>

  <!-- Hero Preview Stage -->
  <section class="preview-stage-container" id="stageContainer">
    <span class="stage-badge">Live Basalt Slab Preview</span>
    
    <div class="bg-switcher">
      <button class="btn-bg active" data-bg="monolith">Dark Basalt</button>
      <button class="btn-bg" data-bg="tablet">Grey Granite</button>
      <button class="btn-bg" data-bg="pure">Pure Dark</button>
    </div>

    <div class="live-heading-wrapper">
      <span class="live-glyph" id="liveGlyph">📐</span>
      <h2 class="live-heading" id="liveHeading">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</h2>
    </div>

    <p class="live-subtext" id="liveSubtext">
      Pure vector schematics detailing the multi-step algorithmic state machines, tool tiers, spatial elevation blueprints, and logistic circuits driving ValheimWorkers v0.3.0.
    </p>

    <!-- Split Baseline Box -->
    <div class="split-baseline-box">
      <span class="baseline-label">Current Baseline:</span>
      <span class="baseline-text">📐 ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </section>

  <!-- Bottom Controller Dock -->
  <div class="dock-wrap">
    <div class="dock-tabs">
      <button class="dock-tab active" data-tab="tab-rust">🍂 Rust &amp; Color</button>
      <button class="dock-tab" data-tab="tab-carve">⚒️ Carving &amp; Relief</button>
      <button class="dock-tab" data-tab="tab-typo">🔤 Typography &amp; Text</button>
      <button class="dock-tab" data-tab="tab-light">☀️ Lighting &amp; Angle</button>
      <button class="dock-tab" data-tab="tab-code">💻 Export &amp; CSS</button>
    </div>

    <div class="dock-panels">
      
      <!-- Panel 1: Rust & Color -->
      <div class="dock-panel active" id="tab-rust">
        
        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Inscription Fill Color</label>
            <span class="ctl-val" id="val-fill-color">#e07534</span>
          </div>
          <div class="swatches-row">
            <button class="swatch-btn active" style="background:#e07534;" data-color="#e07534" title="Bog Iron Rust"></button>
            <button class="swatch-btn" style="background:#d97232;" data-color="#d97232" title="Warm Rust"></button>
            <button class="swatch-btn" style="background:#d64a2b;" data-color="#d64a2b" title="Red Ochre"></button>
            <button class="swatch-btn" style="background:#c99355;" data-color="#c99355" title="Weathered Bronze"></button>
            <button class="swatch-btn" style="background:#dfc49c;" data-color="#dfc49c" title="Sandstone"></button>
            <button class="swatch-btn" style="background:#3d342c;" data-color="#3d342c" title="Wrought Iron"></button>
            <button class="swatch-btn" style="background:#f7d57f;" data-color="#f7d57f" title="Gold"></button>
            <button class="swatch-btn" style="background:#38bdf8;" data-color="#38bdf8" title="Runic Cyan"></button>
            <input type="color" id="picker-fill-color" class="color-picker-input" value="#e07534">
          </div>
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Specular Shininess (0 = Pure Matte, 1.5 = Shiny Glare)</label>
            <span class="ctl-val" id="val-specular">0.00</span>
          </div>
          <input type="range" class="range-slider" id="slider-specular" min="0" max="1.5" step="0.05" value="0">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Rust Lip Highlight Color &amp; Opacity</label>
            <span class="ctl-val" id="val-lip-opacity">50%</span>
          </div>
          <div class="swatches-row">
            <input type="color" id="picker-lip-color" class="color-picker-input" value="#e89862">
            <input type="range" class="range-slider" id="slider-lip-opacity" min="0" max="1.0" step="0.05" value="0.5" style="flex:1;">
          </div>
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Carve Shadow Darkness</label>
            <span class="ctl-val" id="val-shadow-opacity">95%</span>
          </div>
          <input type="range" class="range-slider" id="slider-shadow-opacity" min="0" max="1.0" step="0.05" value="0.95">
        </div>

      </div>

      <!-- Panel 2: Carving & Relief -->
      <div class="dock-panel" id="tab-carve">
        
        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Relief Mode</label>
          </div>
          <div class="seg-group" id="seg-mode">
            <div class="seg-opt active" data-mode="filter">SVG Chiseled V-Groove</div>
            <div class="seg-opt" data-mode="crisp">Crisp Vector Inset (No Filter)</div>
          </div>
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Carve Depth (Surface Scale)</label>
            <span class="ctl-val" id="val-depth">-8</span>
          </div>
          <input type="range" class="range-slider" id="slider-depth" min="1" max="20" step="1" value="8">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Hand Wobble / Chipped Edge (Noise Scale)</label>
            <span class="ctl-val" id="val-wobble">0.0 px</span>
          </div>
          <input type="range" class="range-slider" id="slider-wobble" min="0" max="4.0" step="0.2" value="0">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Carve Shadow Offset (Depth)</label>
            <span class="ctl-val" id="val-offset">1.8 px</span>
          </div>
          <input type="range" class="range-slider" id="slider-offset" min="0.5" max="4.0" step="0.2" value="1.8">
        </div>

      </div>

      <!-- Panel 3: Typography & Text -->
      <div class="dock-panel" id="tab-typo">
        
        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Font Family</label>
          </div>
          <div class="seg-group" id="seg-font">
            <div class="seg-opt active" data-font="Norsebold">Norsebold (Viking Runes)</div>
            <div class="seg-opt" data-font="Averia Serif Libre">Averia Serif Libre (In-Game)</div>
            <div class="seg-opt" data-font="Norse">Norse (Light)</div>
          </div>
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Font Size</label>
            <span class="ctl-val" id="val-size">1.95 rem (31px)</span>
          </div>
          <input type="range" class="range-slider" id="slider-size" min="1.2" max="3.0" step="0.05" value="1.95">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Letter Spacing (Tracking)</label>
            <span class="ctl-val" id="val-spacing">0.06 em</span>
          </div>
          <input type="range" class="range-slider" id="slider-spacing" min="-0.02" max="0.20" step="0.01" value="0.06">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Test Heading Text</label>
          </div>
          <input type="text" class="lab-text-input" id="input-heading-text" value="ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS">
        </div>

      </div>

      <!-- Panel 4: Lighting & Angle -->
      <div class="dock-panel" id="tab-light">
        
        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Light Azimuth (Angle in degrees)</label>
            <span class="ctl-val" id="val-azimuth">315° (Top-Left)</span>
          </div>
          <input type="range" class="range-slider" id="slider-azimuth" min="0" max="360" step="5" value="315">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Light Elevation (Pitch in degrees)</label>
            <span class="ctl-val" id="val-elevation">45°</span>
          </div>
          <input type="range" class="range-slider" id="slider-elevation" min="15" max="80" step="1" value="45">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Diffuse Light Intensity</label>
            <span class="ctl-val" id="val-diffuse">1.20</span>
          </div>
          <input type="range" class="range-slider" id="slider-diffuse" min="0.5" max="2.0" step="0.05" value="1.20">
        </div>

        <div class="ctl-group">
          <div class="ctl-header">
            <label class="ctl-label">Light Tint Color</label>
          </div>
          <div class="swatches-row">
            <button class="swatch-btn active" style="background:#ffeedd;" data-lcolor="#ffeedd" title="Warm Daylight"></button>
            <button class="swatch-btn" style="background:#ffffff;" data-lcolor="#ffffff" title="Neutral White"></button>
            <button class="swatch-btn" style="background:#ffd5b0;" data-lcolor="#ffd5b0" title="Sunset Ember"></button>
            <button class="swatch-btn" style="background:#e0f7ff;" data-lcolor="#e0f7ff" title="Cold Nordic Mist"></button>
            <input type="color" id="picker-light-color" class="color-picker-input" value="#ffeedd">
          </div>
        </div>

      </div>

      <!-- Panel 5: Export & CSS -->
      <div class="dock-panel" id="tab-code">
        <div class="ctl-group" style="grid-column: 1 / -1;">
          <div class="ctl-header">
            <label class="ctl-label">Generated CSS &amp; SVG Filter Configuration</label>
          </div>
          <div class="code-export-box">
            <button class="btn-copy-code" id="btn-copy-box">Copy Code</button>
            <pre><code id="export-code">// CSS will update live here...</code></pre>
          </div>
        </div>
      </div>

    </div>
  </div>

</main>

<div class="toast" id="toast">Copied to clipboard!</div>
"""

JS_SCRIPT = """
<script>
// State Management
const state = {
  preset: 'bog-rust',
  mode: 'filter',
  font: 'Norsebold',
  size: 1.95,
  spacing: 0.06,
  text: 'ILLUSTRATED PROCESS SCHEMAS & BLUEPRINT ELEVATIONS',
  fillColor: '#e07534',
  specular: 0.0,
  specularExp: 20,
  depth: 8,
  wobble: 0.0,
  shadowOffset: 1.8,
  shadowOpacity: 0.95,
  lipColor: '#e89862',
  lipOpacity: 0.50,
  azimuth: 315,
  elevation: 45,
  diffuse: 1.20,
  lightColor: '#ffeedd'
};

const PRESETS = {
  'bog-rust': {
    name: 'Bog Iron Rust (Matte)',
    mode: 'filter',
    font: 'Norsebold',
    size: 1.95,
    spacing: 0.06,
    fillColor: '#e07534',
    specular: 0.0,
    depth: 7,
    wobble: 0.0,
    shadowOffset: 1.8,
    shadowOpacity: 0.95,
    lipColor: '#e89862',
    lipOpacity: 0.45,
    azimuth: 315,
    elevation: 45,
    diffuse: 1.20,
    lightColor: '#ffd5b0'
  },
  'red-ochre': {
    name: 'Red Ochre Runestone',
    mode: 'filter',
    font: 'Norsebold',
    size: 1.95,
    spacing: 0.06,
    fillColor: '#d64a2b',
    specular: 0.0,
    depth: 6,
    wobble: 0.0,
    shadowOffset: 2.0,
    shadowOpacity: 0.98,
    lipColor: '#ffa085',
    lipOpacity: 0.40,
    azimuth: 315,
    elevation: 45,
    diffuse: 1.10,
    lightColor: '#ffe8e0'
  },
  'weathered-bronze': {
    name: 'Weathered Bronze (Averia)',
    mode: 'filter',
    font: 'Averia Serif Libre',
    size: 1.70,
    spacing: 0.06,
    fillColor: '#c99355',
    specular: 0.0,
    depth: 6,
    wobble: 0.0,
    shadowOffset: 1.6,
    shadowOpacity: 0.95,
    lipColor: '#d6a66b',
    lipOpacity: 0.50,
    azimuth: 315,
    elevation: 42,
    diffuse: 1.25,
    lightColor: '#ffeedd'
  },
  'crisp-inset': {
    name: 'Crisp Inset Rust (Sharp)',
    mode: 'crisp',
    font: 'Norsebold',
    size: 2.05,
    spacing: 0.07,
    fillColor: '#d97232',
    specular: 0.0,
    depth: 8,
    wobble: 0.0,
    shadowOffset: 1.8,
    shadowOpacity: 0.95,
    lipColor: '#ffaf6e',
    lipOpacity: 0.45,
    azimuth: 315,
    elevation: 45,
    diffuse: 1.20,
    lightColor: '#ffeedd'
  },
  'hammered-iron': {
    name: 'Hammered Iron + Rust Bevel',
    mode: 'filter',
    font: 'Norsebold',
    size: 1.95,
    spacing: 0.07,
    fillColor: '#3d342c',
    specular: 0.0,
    depth: 8,
    wobble: 0.0,
    shadowOffset: 2.0,
    shadowOpacity: 0.98,
    lipColor: '#c96b27',
    lipOpacity: 0.80,
    azimuth: 315,
    elevation: 50,
    diffuse: 1.00,
    lightColor: '#dfcaa6'
  },
  'sandstone': {
    name: 'Weathered Sandstone',
    mode: 'filter',
    font: 'Averia Serif Libre',
    size: 1.75,
    spacing: 0.05,
    fillColor: '#dfc49c',
    specular: 0.0,
    depth: 7,
    wobble: 0.0,
    shadowOffset: 1.6,
    shadowOpacity: 0.92,
    lipColor: '#f5e8d3',
    lipOpacity: 0.40,
    azimuth: 315,
    elevation: 45,
    diffuse: 1.15,
    lightColor: '#fff5ea'
  },
  'charcoal-ash': {
    name: 'Charcoal & Ash',
    mode: 'filter',
    font: 'Norsebold',
    size: 1.95,
    spacing: 0.06,
    fillColor: '#2b2c30',
    specular: 0.0,
    depth: 9,
    wobble: 0.0,
    shadowOffset: 2.0,
    shadowOpacity: 0.98,
    lipColor: '#8a8e99',
    lipOpacity: 0.50,
    azimuth: 315,
    elevation: 45,
    diffuse: 1.05,
    lightColor: '#ffffff'
  },
  'shiny-baseline': {
    name: 'Current Shiny Gold',
    mode: 'filter',
    font: 'Norsebold',
    size: 1.75,
    spacing: 0.05,
    fillColor: '#f7d57f',
    specular: 0.9,
    depth: 12,
    wobble: 2.0,
    shadowOffset: 1.6,
    shadowOpacity: 0.95,
    lipColor: '#ffeab5',
    lipOpacity: 0.65,
    azimuth: 315,
    elevation: 38,
    diffuse: 1.30,
    lightColor: '#fff4db'
  }
};

// Apply active state to DOM
function updateView() {
  const heading = document.getElementById('liveHeading');
  
  // Font & text
  heading.textContent = state.text;
  heading.style.fontFamily = "'" + state.font + "', serif";
  heading.style.fontSize = state.size + "rem";
  heading.style.letterSpacing = state.spacing + "em";
  heading.style.color = state.fillColor;

  if (state.mode === 'crisp') {
    // Pure CSS Crisp Multi-layer Inset Shadow
    heading.style.filter = 'none';
    const sOff = state.shadowOffset;
    heading.style.textShadow = 
      "-" + sOff + "px -" + sOff + "px 0px rgba(5, 3, 2, " + state.shadowOpacity + "), " +
      (sOff*0.7) + "px " + (sOff*0.7) + "px 0px rgba(255, 175, 110, " + state.lipOpacity + "), " +
      "0px 4px 8px rgba(0, 0, 0, 0.9)";
  } else {
    // SVG Filter Chisel Mode
    heading.style.textShadow = 'none';
    heading.style.filter = 'url(#live-chisel-filter)';
    
    // Update SVG filter nodes
    document.getElementById('f-displace-src').setAttribute('scale', state.wobble);
    document.getElementById('f-displace-alpha').setAttribute('scale', state.wobble);
    
    const diff = document.getElementById('f-diffuse');
    diff.setAttribute('surfaceScale', "-" + state.depth);
    diff.setAttribute('diffuseConstant', state.diffuse);
    diff.setAttribute('lighting-color', state.lightColor);
    
    document.getElementById('f-light-diff').setAttribute('azimuth', state.azimuth);
    document.getElementById('f-light-diff').setAttribute('elevation', state.elevation);
    
    const spec = document.getElementById('f-specular');
    spec.setAttribute('surfaceScale', "-" + state.depth);
    spec.setAttribute('specularConstant', state.specular);
    
    document.getElementById('f-light-spec').setAttribute('azimuth', state.azimuth);
    document.getElementById('f-light-spec').setAttribute('elevation', state.elevation);
    
    // Shadow & Lip
    document.getElementById('f-offset-sh').setAttribute('dx', state.shadowOffset);
    document.getElementById('f-offset-sh').setAttribute('dy', state.shadowOffset);
    document.getElementById('f-flood-sh').setAttribute('flood-opacity', state.shadowOpacity);
    
    document.getElementById('f-offset-lip').setAttribute('dx', -state.shadowOffset * 0.5);
    document.getElementById('f-offset-lip').setAttribute('dy', -state.shadowOffset * 0.5);
    document.getElementById('f-flood-lip').setAttribute('flood-color', state.lipColor);
    document.getElementById('f-flood-lip').setAttribute('flood-opacity', state.lipOpacity);
  }

  // Update code preview
  generateCodeExport();
}

function generateCodeExport() {
  let code = '';
  if (state.mode === 'crisp') {
    code = `/* Crisp Stone Inset Style (` + state.preset + `) */
.section-title {
  font-family: '` + state.font + `', serif;
  font-size: ` + state.size + `rem;
  letter-spacing: ` + state.spacing + `em;
  color: ` + state.fillColor + `;
  text-shadow: 
    -` + state.shadowOffset + `px -` + state.shadowOffset + `px 0px rgba(5, 3, 2, ` + state.shadowOpacity + `),
    ` + (state.shadowOffset*0.7) + `px ` + (state.shadowOffset*0.7) + `px 0px rgba(255, 175, 110, ` + state.lipOpacity + `),
    0px 4px 8px rgba(0, 0, 0, 0.9);
}`;
  } else {
    code = `/* Weathered Rust Chisel (` + state.preset + `) */
.section-title {
  font-family: '` + state.font + `', serif;
  font-size: ` + state.size + `rem;
  letter-spacing: ` + state.spacing + `em;
  color: ` + state.fillColor + `;
  filter: url(#v-groove-rust);
}

/* SVG Filter Definition:
   diffuseConstant="` + state.diffuse + `" surfaceScale="-` + state.depth + `"
   specularConstant="` + state.specular + `" (Shininess: ` + (state.specular === 0 ? 'Pure Matte' : state.specular) + `)
   azimuth="` + state.azimuth + `" elevation="` + state.elevation + `"
   lipColor="` + state.lipColor + `" (Opacity: ` + state.lipOpacity + `)
*/`;
  }
  document.getElementById('export-code').textContent = code;
}

function syncControls() {
  document.getElementById('val-fill-color').textContent = state.fillColor;
  document.getElementById('picker-fill-color').value = state.fillColor;
  
  document.getElementById('slider-specular').value = state.specular;
  document.getElementById('val-specular').textContent = state.specular.toFixed(2) + (state.specular === 0 ? ' (Pure Matte)' : '');
  
  document.getElementById('picker-lip-color').value = state.lipColor;
  document.getElementById('slider-lip-opacity').value = state.lipOpacity;
  document.getElementById('val-lip-opacity').textContent = Math.round(state.lipOpacity * 100) + '%';
  
  document.getElementById('slider-shadow-opacity').value = state.shadowOpacity;
  document.getElementById('val-shadow-opacity').textContent = Math.round(state.shadowOpacity * 100) + '%';
  
  document.getElementById('slider-depth').value = state.depth;
  document.getElementById('val-depth').textContent = '-' + state.depth;
  
  document.getElementById('slider-wobble').value = state.wobble;
  document.getElementById('val-wobble').textContent = state.wobble.toFixed(1) + ' px';
  
  document.getElementById('slider-offset').value = state.shadowOffset;
  document.getElementById('val-offset').textContent = state.shadowOffset.toFixed(1) + ' px';
  
  document.getElementById('slider-size').value = state.size;
  document.getElementById('val-size').textContent = state.size + " rem (" + Math.round(state.size * 16) + "px)";
  
  document.getElementById('slider-spacing').value = state.spacing;
  document.getElementById('val-spacing').textContent = state.spacing.toFixed(2) + ' em';
  
  document.getElementById('input-heading-text').value = state.text;
  
  document.getElementById('slider-azimuth').value = state.azimuth;
  document.getElementById('val-azimuth').textContent = state.azimuth + '°';
  
  document.getElementById('slider-elevation').value = state.elevation;
  document.getElementById('val-elevation').textContent = state.elevation + '°';
  
  document.getElementById('slider-diffuse').value = state.diffuse;
  document.getElementById('val-diffuse').textContent = state.diffuse.toFixed(2);
  
  document.getElementById('picker-light-color').value = state.lightColor;

  // Segmented modes
  document.querySelectorAll('#seg-mode .seg-opt').forEach(opt => {
    opt.classList.toggle('active', opt.dataset.mode === state.mode);
  });
  document.querySelectorAll('#seg-font .seg-opt').forEach(opt => {
    opt.classList.toggle('active', opt.dataset.font === state.font);
  });

  // Preset chips
  document.querySelectorAll('.preset-chip').forEach(chip => {
    chip.classList.toggle('active', chip.dataset.preset === state.preset);
  });
}

function loadPreset(key) {
  const p = PRESETS[key];
  if (!p) return;
  state.preset = key;
  Object.assign(state, p);
  syncControls();
  updateView();
}

function showToast(msg) {
  const toast = document.getElementById('toast');
  toast.textContent = msg;
  toast.classList.add('show');
  setTimeout(() => toast.classList.remove('show'), 2200);
}

// Setup Event Listeners
function setupEvents() {
  // Preset chips
  document.querySelectorAll('.preset-chip').forEach(chip => {
    chip.addEventListener('click', () => loadPreset(chip.dataset.preset));
  });

  // Dock tab switching
  document.querySelectorAll('.dock-tab').forEach(tab => {
    tab.addEventListener('click', () => {
      document.querySelectorAll('.dock-tab').forEach(t => t.classList.remove('active'));
      document.querySelectorAll('.dock-panel').forEach(p => p.classList.remove('active'));
      tab.classList.add('active');
      document.getElementById(tab.dataset.tab).classList.add('active');
    });
  });

  // Background switcher
  document.querySelectorAll('.btn-bg').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.btn-bg').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      const stage = document.getElementById('stageContainer');
      if (btn.dataset.bg === 'monolith') {
        stage.style.backgroundImage = 'var(--stone-monolith-url)';
        stage.style.backgroundColor = '#0d1015';
      } else if (btn.dataset.bg === 'tablet') {
        stage.style.backgroundImage = 'var(--stone-tablet-url)';
        stage.style.backgroundColor = '#181b22';
      } else {
        stage.style.backgroundImage = 'none';
        stage.style.backgroundColor = '#101318';
      }
    });
  });

  // Mode segmented
  document.querySelectorAll('#seg-mode .seg-opt').forEach(opt => {
    opt.addEventListener('click', () => {
      state.mode = opt.dataset.mode;
      syncControls();
      updateView();
    });
  });

  // Font segmented
  document.querySelectorAll('#seg-font .seg-opt').forEach(opt => {
    opt.addEventListener('click', () => {
      state.font = opt.dataset.font;
      syncControls();
      updateView();
    });
  });

  // Sliders
  document.getElementById('slider-specular').addEventListener('input', (e) => {
    state.specular = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-lip-opacity').addEventListener('input', (e) => {
    state.lipOpacity = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-shadow-opacity').addEventListener('input', (e) => {
    state.shadowOpacity = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-depth').addEventListener('input', (e) => {
    state.depth = parseInt(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-wobble').addEventListener('input', (e) => {
    state.wobble = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-offset').addEventListener('input', (e) => {
    state.shadowOffset = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-size').addEventListener('input', (e) => {
    state.size = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-spacing').addEventListener('input', (e) => {
    state.spacing = parseFloat(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-azimuth').addEventListener('input', (e) => {
    state.azimuth = parseInt(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-elevation').addEventListener('input', (e) => {
    state.elevation = parseInt(e.target.value);
    syncControls();
    updateView();
  });
  document.getElementById('slider-diffuse').addEventListener('input', (e) => {
    state.diffuse = parseFloat(e.target.value);
    syncControls();
    updateView();
  });

  // Colors
  document.getElementById('picker-fill-color').addEventListener('input', (e) => {
    state.fillColor = e.target.value;
    syncControls();
    updateView();
  });
  document.getElementById('picker-lip-color').addEventListener('input', (e) => {
    state.lipColor = e.target.value;
    syncControls();
    updateView();
  });
  document.getElementById('picker-light-color').addEventListener('input', (e) => {
    state.lightColor = e.target.value;
    syncControls();
    updateView();
  });
  document.querySelectorAll('.swatch-btn[data-color]').forEach(btn => {
    btn.addEventListener('click', () => {
      state.fillColor = btn.dataset.color;
      syncControls();
      updateView();
    });
  });
  document.querySelectorAll('.swatch-btn[data-lcolor]').forEach(btn => {
    btn.addEventListener('click', () => {
      state.lightColor = btn.dataset.lcolor;
      syncControls();
      updateView();
    });
  });

  // Heading text
  document.getElementById('input-heading-text').addEventListener('input', (e) => {
    state.text = e.target.value || 'TITLE TEXT';
    updateView();
  });

  // Copy code buttons
  function copyActiveCode() {
    const text = document.getElementById('export-code').textContent;
    navigator.clipboard.writeText(text).then(() => {
      showToast('CSS copied to clipboard!');
    }).catch(() => {
      showToast('Selection copied!');
    });
  }
  document.getElementById('btn-export-css').addEventListener('click', copyActiveCode);
  document.getElementById('btn-copy-box').addEventListener('click', copyActiveCode);

  document.getElementById('btn-apply-active').addEventListener('click', () => {
    showToast("Active style ready to apply across the mod compendium!");
  });
}

window.addEventListener('DOMContentLoaded', () => {
  setupEvents();
  loadPreset('bog-rust');
});
</script>
</body>
</html>
"""

FULL_HTML = HTML_HEAD + JS_SCRIPT

print(f"Writing {OUT_FILE}...")
with open(OUT_FILE, "w", encoding="utf-8") as f:
    f.write(FULL_HTML)

print(f"Writing {MIRROR_FILE}...")
with open(MIRROR_FILE, "w", encoding="utf-8") as f:
    f.write(FULL_HTML)

print("SUCCESS: Viking Stone Carving & Rust Lab created and mirrored!")
