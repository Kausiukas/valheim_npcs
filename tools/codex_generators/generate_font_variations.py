import os, subprocess
import codex_styles

OUT_HTML = r"d:\valheim_mods\docs\font_variations.html"
OUT_PNG = r"d:\valheim_mods\docs\font_variations.png"

# SVG filters for the variations
VARIATION_FILTERS = """
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
    <!-- CURRENT: Shiny Gold V-Groove (for comparison) -->
    <filter id="filter-current-shiny" x="-15%" y="-15%" width="130%" height="130%">
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

    <!-- VARIATION A: MATTE BOG IRON RUST (No specular glare, warm oxidized diffuse, deep carve) -->
    <filter id="filter-bog-rust" x="-10%" y="-10%" width="125%" height="125%">
      <feGaussianBlur in="SourceAlpha" stdDeviation="1.0" result="blur"/>
      <feDiffuseLighting in="blur" surfaceScale="-7" diffuseConstant="1.2" lighting-color="#ffd5b0" result="diffLight">
        <feDistantLight azimuth="315" elevation="45"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clipped"/>
      <feOffset in="SourceAlpha" dx="1.8" dy="2.0" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#050302" flood-opacity="0.95" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      <feOffset in="SourceAlpha" dx="-0.8" dy="-0.8" result="lipOff"/>
      <feComposite in="lipOff" in2="SourceAlpha" operator="out" result="bLip"/>
      <feFlood flood-color="#e89862" flood-opacity="0.45" result="lipFlood"/>
      <feComposite in="lipFlood" in2="bLip" operator="in" result="rustLip"/>
      <feMerge>
        <feMergeNode in="rustLip"/>
        <feMergeNode in="clipped"/>
        <feMergeNode in="carveShadow"/>
      </feMerge>
    </filter>

    <!-- VARIATION B: RED OCHRE RUNESTONE (Carved granite filled with Falun red-ochre pigment) -->
    <filter id="filter-red-ochre" x="-10%" y="-10%" width="125%" height="125%">
      <feGaussianBlur in="SourceAlpha" stdDeviation="0.8" result="blur"/>
      <feDiffuseLighting in="blur" surfaceScale="-6" diffuseConstant="1.1" lighting-color="#ffe8e0" result="diffLight">
        <feDistantLight azimuth="315" elevation="45"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.05" k2="0.25" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clipped"/>
      <feOffset in="SourceAlpha" dx="2.0" dy="2.2" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#060202" flood-opacity="0.98" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      <feOffset in="SourceAlpha" dx="-0.6" dy="-0.6" result="lipOff"/>
      <feComposite in="lipOff" in2="SourceAlpha" operator="out" result="bLip"/>
      <feFlood flood-color="#ffa085" flood-opacity="0.4" result="lipFlood"/>
      <feComposite in="lipFlood" in2="bLip" operator="in" result="ochreLip"/>
      <feMerge>
        <feMergeNode in="ochreLip"/>
        <feMergeNode in="clipped"/>
        <feMergeNode in="carveShadow"/>
      </feMerge>
    </filter>

    <!-- VARIATION C: WEATHERED PATINATED BRONZE (Earthy antique bronze, matte finish) -->
    <filter id="filter-weathered-bronze" x="-10%" y="-10%" width="125%" height="125%">
      <feGaussianBlur in="SourceAlpha" stdDeviation="1.0" result="blur"/>
      <feDiffuseLighting in="blur" surfaceScale="-6" diffuseConstant="1.25" lighting-color="#ffeedd" result="diffLight">
        <feDistantLight azimuth="315" elevation="42"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.1" k2="0.18" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clipped"/>
      <feOffset in="SourceAlpha" dx="1.6" dy="1.8" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#0a0704" flood-opacity="0.95" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      <feOffset in="SourceAlpha" dx="-0.8" dy="-0.8" result="lipOff"/>
      <feComposite in="lipOff" in2="SourceAlpha" operator="out" result="bLip"/>
      <feFlood flood-color="#d6a66b" flood-opacity="0.5" result="lipFlood"/>
      <feComposite in="lipFlood" in2="bLip" operator="in" result="bronzeLip"/>
      <feMerge>
        <feMergeNode in="bronzeLip"/>
        <feMergeNode in="clipped"/>
        <feMergeNode in="carveShadow"/>
      </feMerge>
    </filter>

    <!-- VARIATION D: HAMMERED BOG IRON WITH RUST BEVEL -->
    <filter id="filter-hammered-iron" x="-10%" y="-10%" width="125%" height="125%">
      <feGaussianBlur in="SourceAlpha" stdDeviation="0.6" result="blur"/>
      <feDiffuseLighting in="blur" surfaceScale="-8" diffuseConstant="1.0" lighting-color="#dfcaa6" result="diffLight">
        <feDistantLight azimuth="315" elevation="50"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.0" k2="0.1" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clipped"/>
      <feOffset in="SourceAlpha" dx="2.0" dy="2.0" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#020202" flood-opacity="0.98" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      <feOffset in="SourceAlpha" dx="-1.0" dy="-1.0" result="lipOff"/>
      <feComposite in="lipOff" in2="SourceAlpha" operator="out" result="bLip"/>
      <feFlood flood-color="#c96b27" flood-opacity="0.8" result="rustBevel"/>
      <feComposite in="rustBevel" in2="bLip" operator="in" result="bevelLine"/>
      <feMerge>
        <feMergeNode in="bevelLine"/>
        <feMergeNode in="clipped"/>
        <feMergeNode in="carveShadow"/>
      </feMerge>
    </filter>
  </defs>
</svg>
"""

HTML_CONTENT = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>ValheimWorkers Header Font & Rust Variations</title>
  <style>
    @font-face {{
      font-family: 'Norsebold';
      src: url('{codex_styles.norsebold_b64}') format('opentype');
      font-weight: bold;
      font-style: normal;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('{codex_styles.averia_bold_b64}') format('truetype');
      font-weight: bold;
      font-style: normal;
    }}

    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}

    body {{
      background-color: #12151b;
      background-image: url('{codex_styles.stone_monolith_b64}');
      background-repeat: repeat;
      background-size: 512px 512px;
      color: #ede2d0;
      font-family: 'Averia Serif Libre', Georgia, serif;
      padding: 40px;
      display: flex;
      flex-direction: column;
      align-items: center;
    }}

    .header-banner {{
      max-width: 1400px;
      width: 100%;
      text-align: center;
      margin-bottom: 36px;
      padding-bottom: 24px;
      border-bottom: 2px solid #4a381d;
    }}
    .header-banner h1 {{
      font-family: 'Norsebold', serif;
      font-size: 2.2rem;
      color: #e59850;
      letter-spacing: 0.08em;
      margin-bottom: 8px;
      text-shadow: 2px 2px 4px #000;
    }}
    .header-banner p {{
      color: #a89885;
      font-size: 1.05rem;
    }}

    .variations-container {{
      max-width: 1400px;
      width: 100%;
      display: flex;
      flex-direction: column;
      gap: 28px;
    }}

    .variation-card {{
      background: rgba(18, 22, 28, 0.94);
      border: 1.5px solid #3c301c;
      border-radius: 8px;
      padding: 24px 30px;
      box-shadow: 0 6px 20px rgba(0, 0, 0, 0.8), inset 0 1px 0 rgba(255, 255, 255, 0.04);
      position: relative;
    }}
    .variation-card.recommended {{
      border-color: #d97232;
      box-shadow: 0 0 20px rgba(217, 114, 50, 0.25), 0 6px 20px rgba(0, 0, 0, 0.8);
    }}

    .badge-tag {{
      display: inline-block;
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-size: 0.75rem;
      font-weight: bold;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 3px 10px;
      border-radius: 4px;
      margin-bottom: 12px;
    }}
    .badge-current {{
      background: #2a2010;
      border: 1px solid #7a5820;
      color: #e5b95c;
    }}
    .badge-recommended {{
      background: #3d1f0e;
      border: 1px solid #d97232;
      color: #ff9b5a;
    }}
    .badge-alternative {{
      background: #1b2633;
      border: 1px solid #38bdf8;
      color: #7ad9f5;
    }}

    .var-meta {{
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      margin-bottom: 14px;
    }}
    .var-name {{
      font-family: 'Norsebold', serif;
      font-size: 1.3rem;
      color: #fff;
      letter-spacing: 0.04em;
    }}
    .var-font-info {{
      font-size: 0.88rem;
      color: #8c7d6e;
    }}

    /* Title Stage: Displays the title exactly on the dark stone background */
    .title-stage {{
      background: #101318;
      background-image: url('{codex_styles.stone_monolith_b64}');
      background-size: 320px 320px;
      border: 1px solid #2a2216;
      border-radius: 6px;
      padding: 22px 28px;
      margin-bottom: 14px;
      display: flex;
      align-items: center;
      gap: 14px;
      box-shadow: inset 0 2px 8px rgba(0, 0, 0, 0.9);
    }}
    .glyph-icon {{
      font-size: 1.85rem;
      line-height: 1;
      filter: drop-shadow(0 2px 4px #000);
    }}

    .var-desc {{
      font-size: 0.92rem;
      color: #b5a490;
      line-height: 1.5;
    }}
    .var-desc strong {{
      color: #e8d8c3;
    }}

    /* --------------------------------------------------------------------------
       SPECIFIC VARIATION STYLES
       -------------------------------------------------------------------------- */

    /* 0. CURRENT SHINY (As seen in screenshot) */
    .title-current {{
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      font-size: 1.75rem;
      letter-spacing: 0.05em;
      filter: url(#filter-current-shiny);
      white-space: nowrap;
    }}

    /* 1. VARIATION A: BOG IRON RUST (Norsebold, Matte Oxidized Orange-Brown, Chiseled Carve) */
    .title-var-a {{
      font-family: 'Norsebold', serif;
      color: #e07534;
      font-size: 1.95rem;
      letter-spacing: 0.06em;
      filter: url(#filter-bog-rust);
      white-space: nowrap;
    }}

    /* 2. VARIATION B: RED OCHRE RUNESTONE (Norsebold, Historical Scandinavian Falun Red Pigment) */
    .title-var-b {{
      font-family: 'Norsebold', serif;
      color: #d64a2b;
      font-size: 1.95rem;
      letter-spacing: 0.06em;
      filter: url(#filter-red-ochre);
      white-space: nowrap;
    }}

    /* 3. VARIATION C: WEATHERED ANTIQUE BRONZE (Averia Serif Libre Bold - Valheim UI Native) */
    .title-var-c {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
      color: #c99355;
      font-size: 1.7rem;
      letter-spacing: 0.06em;
      filter: url(#filter-weathered-bronze);
      white-space: nowrap;
    }}

    /* 4. VARIATION D: HAMMERED BOG IRON WITH RUST BEVEL (Norsebold, Dark Charcoal Core + Rust Lip) */
    .title-var-d {{
      font-family: 'Norsebold', serif;
      color: #3d342c;
      font-size: 1.95rem;
      letter-spacing: 0.07em;
      filter: url(#filter-hammered-iron);
      white-space: nowrap;
    }}

    /* 5. VARIATION E: PURE CSS WEATHERED RUST CHISEL (No SVG filter distortion, 100% crisp on all displays) */
    .title-var-e {{
      font-family: 'Norsebold', serif;
      color: #d97232;
      font-size: 2.05rem;
      letter-spacing: 0.07em;
      white-space: nowrap;
      text-shadow: 
        -1px -2px 0px #080503,
        1px 1px 0px rgba(255, 175, 110, 0.4),
        0 3px 6px rgba(0, 0, 0, 0.95);
    }}

    /* 6. VARIATION F: AVERIA SERIF LIBRE BOLD - DEEP OCHRE RUST (Native Valheim UI Font with pure CSS carve) */
    .title-var-f {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
      color: #df7738;
      font-size: 1.75rem;
      letter-spacing: 0.05em;
      white-space: nowrap;
      text-shadow: 
        -1px -2px 1px #060302,
        1px 1px 0px rgba(255, 190, 130, 0.45),
        0 4px 8px rgba(0, 0, 0, 0.9);
    }}
  </style>
</head>
<body>

{VARIATION_FILTERS}

<div class="header-banner">
  <h1>SECTION TITLE TYPOGRAPHY &amp; RUST VARIATIONS</h1>
  <p>Comparing 6 non-shiny, weathered rust &amp; iron styles against the current shiny gold baseline</p>
</div>

<div class="variations-container">

  <!-- CURRENT BASELINE -->
  <div class="variation-card">
    <div class="badge-tag badge-current">Current Baseline (For Comparison)</div>
    <div class="var-meta">
      <div class="var-name">Current: Shiny Gold V-Groove Chisel</div>
      <div class="var-font-info">Font: Norsebold (1.75rem) &bull; Color: #f7d57f &bull; Specular: High (#fff8d6)</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-current">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Critique:</strong> High specular reflection and harsh white highlights create a glittering, noisy foil effect. Thin rune strokes get thinned and fragmented by displacement turbulence.
    </div>
  </div>

  <!-- VARIATION A -->
  <div class="variation-card recommended">
    <div class="badge-tag badge-recommended">Variation A &bull; Recommended</div>
    <div class="var-meta">
      <div class="var-name">Viking Bog Iron Rust (Matte Oxidized Chisel)</div>
      <div class="var-font-info">Font: Norsebold (1.95rem) &bull; Color: #e07534 (Bog Iron) &bull; Specular: 0% (Pure Matte)</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-a">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> 100% glare-free matte relief with a warm oxidized bog-iron palette. Uses gentle diffuse lighting from top-left, deep charcoal V-carve shadows, and an earthy rust-colored stone rim. Eliminates all metallic glitz while preserving the authentic runic font.
    </div>
  </div>

  <!-- VARIATION B -->
  <div class="variation-card">
    <div class="badge-tag badge-alternative">Variation B</div>
    <div class="var-meta">
      <div class="var-name">Red Ochre Runestone Carving (Falun Pigment)</div>
      <div class="var-font-info">Font: Norsebold (1.95rem) &bull; Color: #d64a2b (Iron Oxide Ochre) &bull; Style: Runestone Infill</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-b">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> Historically authentic Scandinavian runestone aesthetic. Historical Viking runestones carved into granite were filled with red iron-oxide ochre paint to make the runes stand out against the grey stone. Exceptional contrast and zero shine.
    </div>
  </div>

  <!-- VARIATION C -->
  <div class="variation-card">
    <div class="badge-tag badge-alternative">Variation C</div>
    <div class="var-meta">
      <div class="var-name">Weathered Antique Bronze (Valheim In-Game UI Font)</div>
      <div class="var-font-info">Font: Averia Serif Libre Bold (1.7rem) &bull; Color: #c99355 &bull; Style: Hand-Carved Serif</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-c">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> Employs <em>Averia Serif Libre Bold</em> (Valheim's native in-game menu and UI font). Its thicker, organic, hand-crafted serif stems provide superior legibility and readability over Norsebold, rendered with a matte patinated bronze and warm rust chisel.
    </div>
  </div>

  <!-- VARIATION D -->
  <div class="variation-card">
    <div class="badge-tag badge-alternative">Variation D</div>
    <div class="var-meta">
      <div class="var-name">Hammered Bog Iron with Rust Bevel</div>
      <div class="var-font-info">Font: Norsebold (1.95rem) &bull; Core: #3d342c (Dark Iron) &bull; Rim: #c96b27 (Rust Bevel)</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-d">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> Dual-tone wrought iron look. Dark forged iron core seated deep into the rock tablet, outlined with a bright burnt-orange rust bevel along the bottom-right lip.
    </div>
  </div>

  <!-- VARIATION E -->
  <div class="variation-card recommended">
    <div class="badge-tag badge-recommended">Variation E &bull; High Crispness Alternative</div>
    <div class="var-meta">
      <div class="var-name">Crisp Bog Rust (Razor-Sharp Vector Relief)</div>
      <div class="var-font-info">Font: Norsebold (2.05rem) &bull; Color: #d97232 &bull; Relief: Dual-Layer Inset Shadow</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-e">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> Completely bypasses SVG displacement map noise so letters retain 100% solid, crisp glyph silhouettes. Creates an incised stone carving illusion via deep soot top-left under-shadow and a soft warm rust bottom-right lip.
    </div>
  </div>

  <!-- VARIATION F -->
  <div class="variation-card">
    <div class="badge-tag badge-alternative">Variation F</div>
    <div class="var-meta">
      <div class="var-name">Averia Serif Libre Bold - Deep Ochre Rust</div>
      <div class="var-font-info">Font: Averia Serif Libre Bold (1.75rem) &bull; Color: #df7738 &bull; Relief: Crisp Stone Inset</div>
    </div>
    <div class="title-stage">
      <span class="glyph-icon">📐</span>
      <span class="title-var-f">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
    <div class="var-desc">
      <strong>Design:</strong> Combines the authentic Valheim in-game font with the crisp bog rust relief. Maximum readability, robust stem thickness, and zero pixel degradation at any zoom level.
    </div>
  </div>

</div>

</body>
</html>
"""

print(f"Writing {OUT_HTML}...")
with open(OUT_HTML, "w", encoding="utf-8") as f:
    f.write(HTML_CONTENT)

print("Rendering high-resolution screenshot with Edge headless...")
edge_exe = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
cmd = [
    edge_exe,
    "--headless=new",
    "--window-size=1600,2400",
    "--virtual-time-budget=4000",
    f"--screenshot={OUT_PNG}",
    f"file:///{OUT_HTML}"
]
subprocess.run(cmd, check=True)
print(f"SUCCESS: Generated {OUT_PNG}")
