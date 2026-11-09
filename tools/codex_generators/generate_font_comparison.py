import os, subprocess
import codex_styles

OUT_HTML = r"d:\valheim_mods\docs\font_variations_comparison.html"
OUT_PNG = r"d:\valheim_mods\docs\font_variations_comparison.png"

# Same filters
from generate_font_variations import VARIATION_FILTERS

HTML_CONTENT = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>Font Variations Comparison Strip</title>
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
      padding: 24px;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
    }}

    .strip-card {{
      width: 1400px;
      background: rgba(16, 20, 26, 0.96);
      border: 1.5px solid #3c301c;
      border-radius: 6px;
      padding: 14px 20px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 20px;
      box-shadow: 0 4px 14px rgba(0, 0, 0, 0.8);
    }}
    .strip-card.highlight {{
      border-color: #d97232;
      box-shadow: 0 0 16px rgba(217, 114, 50, 0.3), 0 4px 14px rgba(0, 0, 0, 0.8);
    }}

    .label-col {{
      width: 280px;
      flex-shrink: 0;
    }}
    .label-tag {{
      display: inline-block;
      font-size: 0.72rem;
      font-weight: bold;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 2px 8px;
      border-radius: 4px;
      margin-bottom: 4px;
    }}
    .tag-current {{ background: #261e12; border: 1px solid #7a5820; color: #ebb856; }}
    .tag-rec {{ background: #3d1f0e; border: 1px solid #d97232; color: #ff9b5a; }}
    .tag-alt {{ background: #162230; border: 1px solid #38bdf8; color: #7ad9f5; }}

    .label-title {{
      font-family: 'Norsebold', serif;
      font-size: 1.15rem;
      color: #fff;
      letter-spacing: 0.03em;
    }}
    .label-sub {{
      font-size: 0.8rem;
      color: #8c7d6e;
    }}

    .stage-col {{
      flex: 1;
      background: #0f1217;
      background-image: url('{codex_styles.stone_monolith_b64}');
      background-size: 320px 320px;
      border: 1px solid #2a2216;
      border-radius: 5px;
      padding: 14px 22px;
      display: flex;
      align-items: center;
      gap: 12px;
      box-shadow: inset 0 2px 6px rgba(0, 0, 0, 0.9);
      overflow: hidden;
    }}

    .glyph {{
      font-size: 1.6rem;
      line-height: 1;
      filter: drop-shadow(0 2px 3px #000);
      flex-shrink: 0;
    }}

    /* Title Styles */
    .title-current {{
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      font-size: 1.65rem;
      letter-spacing: 0.05em;
      filter: url(#filter-current-shiny);
      white-space: nowrap;
    }}

    .title-var-a {{
      font-family: 'Norsebold', serif;
      color: #e07534;
      font-size: 1.85rem;
      letter-spacing: 0.06em;
      filter: url(#filter-bog-rust);
      white-space: nowrap;
    }}

    .title-var-b {{
      font-family: 'Norsebold', serif;
      color: #d64a2b;
      font-size: 1.85rem;
      letter-spacing: 0.06em;
      filter: url(#filter-red-ochre);
      white-space: nowrap;
    }}

    .title-var-c {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
      color: #c99355;
      font-size: 1.6rem;
      letter-spacing: 0.06em;
      filter: url(#filter-weathered-bronze);
      white-space: nowrap;
    }}

    .title-var-d {{
      font-family: 'Norsebold', serif;
      color: #3d342c;
      font-size: 1.85rem;
      letter-spacing: 0.07em;
      filter: url(#filter-hammered-iron);
      white-space: nowrap;
    }}

    .title-var-e {{
      font-family: 'Norsebold', serif;
      color: #d97232;
      font-size: 1.95rem;
      letter-spacing: 0.07em;
      white-space: nowrap;
      text-shadow: 
        -1px -2px 0px #080503,
        1px 1px 0px rgba(255, 175, 110, 0.45),
        0 3px 6px rgba(0, 0, 0, 0.95);
    }}

    .title-var-f {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      font-weight: bold;
      color: #df7738;
      font-size: 1.65rem;
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

  <!-- BASELINE -->
  <div class="strip-card">
    <div class="label-col">
      <div class="label-tag tag-current">Current Baseline</div>
      <div class="label-title">Shiny Gold V-Groove</div>
      <div class="label-sub">Norsebold &bull; #f7d57f &bull; Specular Glare</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-current">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION A -->
  <div class="strip-card highlight">
    <div class="label-col">
      <div class="label-tag tag-rec">Variation A &bull; Recommended</div>
      <div class="label-title">Bog Iron Rust (Matte Chisel)</div>
      <div class="label-sub">Norsebold &bull; #e07534 &bull; 0% Specular</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-a">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION B -->
  <div class="strip-card">
    <div class="label-col">
      <div class="label-tag tag-alt">Variation B</div>
      <div class="label-title">Red Ochre Runestone</div>
      <div class="label-sub">Norsebold &bull; #d64a2b &bull; Falun Pigment</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-b">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION C -->
  <div class="strip-card">
    <div class="label-col">
      <div class="label-tag tag-alt">Variation C &bull; In-Game Font</div>
      <div class="label-title">Weathered Bronze</div>
      <div class="label-sub">Averia Serif Libre &bull; #c99355 &bull; Thick Stems</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-c">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION D -->
  <div class="strip-card">
    <div class="label-col">
      <div class="label-tag tag-alt">Variation D</div>
      <div class="label-title">Hammered Iron + Rust Bevel</div>
      <div class="label-sub">Norsebold &bull; #3d342c Core &bull; #c96b27 Bevel</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-d">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION E -->
  <div class="strip-card highlight">
    <div class="label-col">
      <div class="label-tag tag-rec">Variation E &bull; Sharpest</div>
      <div class="label-title">Crisp Bog Rust (Direct Inset)</div>
      <div class="label-sub">Norsebold &bull; #d97232 &bull; No Filter Distortion</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-e">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

  <!-- VARIATION F -->
  <div class="strip-card">
    <div class="label-col">
      <div class="label-tag tag-alt">Variation F &bull; In-Game Sharp</div>
      <div class="label-title">Averia Deep Ochre Rust</div>
      <div class="label-sub">Averia Serif Libre &bull; #df7738 &bull; Max Legibility</div>
    </div>
    <div class="stage-col">
      <span class="glyph">📐</span>
      <span class="title-var-f">ILLUSTRATED PROCESS SCHEMAS &amp; BLUEPRINT ELEVATIONS</span>
    </div>
  </div>

</body>
</html>
"""

print(f"Writing {OUT_HTML}...")
with open(OUT_HTML, "w", encoding="utf-8") as f:
    f.write(HTML_CONTENT)

print("Rendering comparison strip...")
edge_exe = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
cmd = [
    edge_exe,
    "--headless=new",
    "--window-size=1480,1050",
    "--virtual-time-budget=4000",
    f"--screenshot={OUT_PNG}",
    f"file:///{OUT_HTML}"
]
subprocess.run(cmd, check=True)
print(f"SUCCESS: Generated {OUT_PNG}")
