import os
import subprocess
import base64
import sys

FONTS_DIR = r"d:\valheim_mods\docs\assets\fonts"
ICONS_DIR = r"d:\valheim_mods\docs\assets\icons"

def get_font_b64(filename):
    p = os.path.join(FONTS_DIR, filename)
    if os.path.exists(p):
        with open(p, "rb") as f:
            return base64.b64encode(f.read()).decode("ascii")
    return ""

print("Loading Valheim fonts...")
norse_b64 = get_font_b64("Norse.otf")
norsebold_b64 = get_font_b64("Norsebold.otf")
averia_reg_b64 = get_font_b64("AveriaSerifLibre-Regular.ttf")
averia_bold_b64 = get_font_b64("AveriaSerifLibre-Bold.ttf")
averia_italic_b64 = get_font_b64("AveriaSerifLibre-Italic.ttf")

def ic(name):
    return f"docs/assets/icons/{name}"

def slot(img_name, label="", equipped=False):
    eq_class = " equipped" if equipped else ""
    lbl_html = f'<span class="slot-label">{label}</span>' if label else ""
    return f'<div class="valheim-slot{eq_class}"><img src="{ic(img_name)}" alt="{label}">{lbl_html}</div>'

def feat(code, name, img_name):
    return f'<span class="feature-badge"><img src="{ic(img_name)}" alt="{code}"> {code} {name}</span>'

def hugin():
    return f'<span class="hugin-badge"><img src="{ic("Hugin.png")}" alt="Hugin"> HUGIN\'S ADVICE</span>'

parts = []

# --- STYLES & HEADER ---
parts.append(f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>The Settlement Steward's Codex — Official Guide to Valheim NPC Workers</title>
  <style>
    @font-face {{
      font-family: 'Norse';
      src: url('data:font/otf;base64,{norse_b64}') format('opentype');
      font-weight: normal;
    }}
    @font-face {{
      font-family: 'Norsebold';
      src: url('data:font/otf;base64,{norsebold_b64}') format('opentype');
      font-weight: bold;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{averia_reg_b64}') format('truetype');
      font-weight: normal;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{averia_bold_b64}') format('truetype');
      font-weight: bold;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{averia_italic_b64}') format('truetype');
      font-weight: normal;
      font-style: italic;
    }}

    @page {{
      size: A4 portrait;
      margin: 8mm 9mm 8mm 9mm;
    }}
    @media print {{
      body {{
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
      }}
      .page {{
        page-break-before: always;
        break-before: page;
        page-break-inside: avoid;
        break-inside: avoid;
        height: 280mm;
        max-height: 280mm;
        overflow: hidden;
      }}
      .page-first {{
        page-break-before: auto;
        break-before: auto;
      }}
    }}
    
    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}
    
    body {{
      font-family: 'Averia Serif Libre', Georgia, serif;
      background-color: #0E0A07;
      color: #EFE6D8;
      line-height: 1.33;
      font-size: 7.7pt;
    }}

    .page {{
      height: 280mm;
      max-height: 280mm;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      page-break-before: always;
      break-before: page;
      padding: 6mm 7.5mm 4.5mm 7.5mm;
      overflow: hidden;
      background: linear-gradient(175deg, #18120C 0%, #1F1710 40%, #150E09 100%);
      border: 2px solid #5C4424;
      outline: 1px solid #A87E34;
      outline-offset: -4px;
      border-radius: 4px;
      box-shadow: inset 0 0 16px rgba(0,0,0,0.85);
      position: relative;
    }}
    .page-first {{
      page-break-before: avoid;
      break-before: avoid;
    }}

    .page::before {{
      content: "ᚠ   ᚦ   ᚨ   ᚱ   ᛋ   ᛏ   ᛒ   ᛖ   ᛗ   ᛚ   ᛜ   ᛞ   ᛟ";
      position: absolute;
      top: 2px;
      left: 12px;
      right: 12px;
      text-align: center;
      font-family: 'Norse', serif;
      font-size: 5.6pt;
      color: rgba(200, 155, 60, 0.35);
      letter-spacing: 5px;
      pointer-events: none;
    }}

    .chapter-header {{
      border-bottom: 2px solid #846028;
      padding-bottom: 2.5px;
      margin-bottom: 4.5px;
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      position: relative;
    }}
    .chapter-header h2 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 13.8pt;
      color: #FFBF1F;
      letter-spacing: 0.04em;
      text-shadow: 0 2px 4px rgba(0,0,0,0.8);
      display: flex;
      align-items: center;
      gap: 5px;
    }}
    .chapter-header .chapter-icon {{
      width: 19px;
      height: 19px;
      object-fit: contain;
      filter: drop-shadow(0 0 3px #EB8F0D);
    }}
    .role-pill {{
      background: #845B1C;
      color: #FFF2D1;
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 7.1pt;
      padding: 1.5px 6.5px;
      border-radius: 3px;
      border: 1px solid #DDA13B;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      box-shadow: 0 1px 3px rgba(0,0,0,0.5);
    }}

    .page-footer {{
      border-top: 1px solid #4A3720;
      padding-top: 2px;
      display: flex;
      justify-content: space-between;
      font-size: 6.6pt;
      color: #9C8C78;
      font-family: 'Averia Serif Libre', serif;
      margin-top: 2.5px;
    }}

    .showcase-bar {{
      background: rgba(18, 13, 9, 0.9);
      border: 1px solid #4D3921;
      border-radius: 4px;
      padding: 3px 6px;
      margin-bottom: 4.5px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 6px;
      box-shadow: inset 0 0 8px rgba(0,0,0,0.6);
    }}
    .showcase-group {{
      display: flex;
      align-items: center;
      gap: 5px;
    }}
    .showcase-title {{
      font-family: 'Norsebold', serif;
      font-size: 7pt;
      color: #DDA13B;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin-right: 2px;
    }}
    
    .valheim-slot {{
      width: 29px;
      height: 29px;
      background: #17120E;
      border: 1.5px solid #443525;
      border-radius: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      box-shadow: inset 0 0 4px #000;
    }}
    .valheim-slot.equipped {{
      border-color: #29B6F6;
      box-shadow: inset 0 0 4px #000, 0 0 5px rgba(41, 182, 246, 0.55);
    }}
    .valheim-slot img {{
      width: 23px;
      height: 23px;
      object-fit: contain;
      filter: drop-shadow(0 1px 2px rgba(0,0,0,0.7));
    }}
    .valheim-slot .slot-label {{
      position: absolute;
      bottom: -11px;
      left: 50%;
      transform: translateX(-50%);
      font-size: 5.4pt;
      color: #E2D7C8;
      white-space: nowrap;
      font-family: 'Averia Serif Libre', serif;
      text-shadow: 0 1px 2px #000;
    }}
    .valheim-slot.equipped::after {{
      content: "";
      position: absolute;
      top: -2px;
      right: -2px;
      width: 4px;
      height: 4px;
      background: #29B6F6;
      border-radius: 50%;
    }}

    .panel {{
      border-radius: 4px;
      padding: 4px 6.5px;
      margin-bottom: 3.8px;
      border-left: 3.5px solid;
      background: rgba(22, 17, 12, 0.88);
      border-top: 1px solid rgba(75, 55, 30, 0.4);
      border-right: 1px solid rgba(75, 55, 30, 0.4);
      border-bottom: 1px solid rgba(75, 55, 30, 0.4);
      box-shadow: 0 1px 4px rgba(0,0,0,0.4);
    }}
    .panel h3 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 8.4pt;
      margin-bottom: 2px;
      display: flex;
      align-items: center;
      gap: 4px;
      letter-spacing: 0.02em;
    }}
    .panel p {{
      font-size: 7.5pt;
      line-height: 1.31;
      color: #DDD2C1;
      margin-bottom: 1.8px;
    }}
    .panel ul {{
      margin-left: 13px;
      font-size: 7.4pt;
      color: #E3D9C9;
      line-height: 1.31;
    }}
    .panel li {{
      margin-bottom: 1.6px;
    }}
    
    .panel-now {{
      border-left-color: #5FA834;
      background: linear-gradient(90deg, rgba(32, 48, 20, 0.45) 0%, rgba(20, 15, 10, 0.85) 100%);
    }}
    .panel-now h3 {{ color: #78C946; }}
    
    .panel-future {{
      border-left-color: #0284C7;
      background: linear-gradient(90deg, rgba(14, 40, 64, 0.45) 0%, rgba(20, 15, 10, 0.85) 100%);
    }}
    .panel-future h3 {{ color: #38BDF8; }}

    .panel-rec {{
      border-left-color: #D97706;
      background: linear-gradient(90deg, rgba(55, 34, 12, 0.45) 0%, rgba(20, 15, 10, 0.85) 100%);
    }}
    .panel-rec h3 {{ color: #FBBF24; }}

    .panel-warn {{
      border-left-color: #DC2626;
      background: linear-gradient(90deg, rgba(60, 20, 20, 0.45) 0%, rgba(20, 15, 10, 0.85) 100%);
    }}
    .panel-warn h3 {{ color: #F87171; }}

    .hugin-badge {{
      display: inline-flex;
      align-items: center;
      gap: 3px;
      background: rgba(217, 119, 6, 0.25);
      border: 1px solid #D97706;
      color: #FBBF24;
      padding: 1px 4px;
      border-radius: 3px;
      font-size: 6.5pt;
      font-family: 'Norsebold', serif;
      margin-right: 3px;
    }}
    .hugin-badge img {{
      width: 10px;
      height: 10px;
      object-fit: contain;
    }}

    .feature-badge {{
      display: inline-flex;
      align-items: center;
      gap: 3px;
      background: rgba(245, 158, 11, 0.15);
      color: #FCD34D;
      border: 1px solid #B45309;
      border-radius: 3px;
      padding: 0px 4px;
      font-size: 6.9pt;
      font-family: 'Norsebold', serif;
      margin-right: 3px;
    }}
    .feature-badge img {{
      width: 10px;
      height: 10px;
      object-fit: contain;
    }}

    table {{
      width: 100%;
      border-collapse: collapse;
      margin: 3px 0;
      font-size: 7pt;
      background: rgba(18, 13, 9, 0.9);
      border: 1px solid #4D3921;
    }}
    th, td {{
      border: 1px solid #44321D;
      padding: 2.8px 4.5px;
      text-align: left;
    }}
    th {{
      background: #231911;
      color: #FFBF1F;
      font-family: 'Norsebold', serif;
      font-weight: 700;
      font-size: 7.1pt;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      border-bottom: 2px solid #845B1C;
    }}
    tr:nth-child(even) td {{
      background: rgba(26, 19, 13, 0.5);
    }}
    td strong {{
      color: #FCD34D;
      font-family: 'Norse', serif;
      font-size: 7.4pt;
    }}

    .cover-page {{
      background: radial-gradient(circle at center, #261B12 0%, #150E09 60%, #0D0805 100%);
      border: 3px double #C49137;
      outline: 1.5px solid #6E4F22;
      outline-offset: -7px;
      text-align: center;
      align-items: center;
      padding: 9mm 11mm 6mm 11mm;
    }}
    .cover-title {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 24pt;
      line-height: 1.1;
      color: #FFBF1F;
      margin-top: 7px;
      margin-bottom: 3px;
      text-shadow: 0 3px 12px rgba(0,0,0,0.9), 0 0 16px rgba(235, 143, 13, 0.45);
      letter-spacing: 0.05em;
    }}
    .cover-subtitle {{
      font-family: 'Averia Serif Libre', serif;
      font-size: 10pt;
      color: #DDD2C1;
      max-width: 540px;
      margin: 0 auto 7px auto;
      font-style: italic;
    }}
    .cover-divider {{
      width: 170px;
      height: 2px;
      background: linear-gradient(90deg, transparent, #DDA13B, transparent);
      margin: 6px auto;
    }}
    .cover-crest {{
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 12px;
      margin: 7px 0;
    }}
    .cover-crest img.main-shield {{
      width: 70px;
      height: 70px;
      filter: drop-shadow(0 4px 10px rgba(0,0,0,0.9)) drop-shadow(0 0 10px rgba(255, 191, 31, 0.35));
    }}
    .cover-crest img.flank-tool {{
      width: 42px;
      height: 42px;
      filter: drop-shadow(0 3px 6px rgba(0,0,0,0.8));
    }}
    .cover-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 5.5px;
      max-width: 600px;
      margin: 8px auto;
      text-align: left;
    }}
    .cover-card {{
      background: rgba(22, 16, 11, 0.88);
      border: 1px solid #4D3921;
      border-radius: 4px;
      padding: 5px 8px;
      display: flex;
      align-items: center;
      gap: 7px;
    }}
    .cover-card img {{
      width: 25px;
      height: 25px;
      object-fit: contain;
      filter: drop-shadow(0 1px 3px #000);
    }}
    .cover-card strong {{
      color: #FFBF1F;
      font-family: 'Norsebold', serif;
      font-size: 7.9pt;
      display: block;
      margin-bottom: 1px;
    }}
    .cover-card p {{
      color: #B5A998;
      font-size: 6.9pt;
      line-height: 1.23;
    }}

    .toc-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 5px;
      margin-top: 3px;
    }}
    .toc-card {{
      background: rgba(20, 15, 10, 0.9);
      border: 1px solid #4B3722;
      border-radius: 4px;
      padding: 4.5px 7.5px;
    }}
    .toc-card h4 {{
      font-family: 'Norsebold', serif;
      font-size: 8.2pt;
      color: #FFBF1F;
      border-bottom: 1px solid #5C4425;
      padding-bottom: 2px;
      margin-bottom: 3px;
      display: flex;
      align-items: center;
      gap: 4px;
    }}
    .toc-card h4 img {{
      width: 12px;
      height: 12px;
      object-fit: contain;
    }}
    .toc-list {{
      list-style: none;
      font-size: 7.2pt;
    }}
    .toc-list li {{
      display: flex;
      justify-content: space-between;
      margin-bottom: 1.6px;
      border-bottom: 1px dotted #3D2D1B;
      padding-bottom: 1px;
    }}
    .toc-list li span:first-child {{
      display: flex;
      align-items: center;
      gap: 3.5px;
    }}
    .toc-list li span:first-child img {{
      width: 10px;
      height: 10px;
      object-fit: contain;
    }}
    .toc-list li span:last-child {{
      font-family: 'Norsebold', serif;
      color: #DDA13B;
      font-size: 7.2pt;
    }}

    .feat-catalog-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 3px 5px;
      margin-top: 2px;
    }}
    .feat-card-comp {{
      background: rgba(20, 15, 10, 0.92);
      border: 1px solid #44321D;
      border-left: 3px solid #DDA13B;
      border-radius: 3px;
      padding: 2.2px 5px;
      display: flex;
      align-items: center;
      gap: 5px;
    }}
    .feat-card-comp .feat-img-box {{
      width: 23px;
      height: 23px;
      background: #15100B;
      border: 1px solid #3D2D1B;
      border-radius: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }}
    .feat-card-comp .feat-img-box img {{
      width: 18px;
      height: 18px;
      object-fit: contain;
    }}
    .feat-card-body {{
      flex: 1;
      min-width: 0;
    }}
    .feat-card-head {{
      display: flex;
      justify-content: space-between;
      align-items: center;
      line-height: 1.1;
    }}
    .feat-card-head strong {{
      font-family: 'Norsebold', serif;
      font-size: 7.3pt;
      color: #FFBF1F;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }}
    .feat-card-domain {{
      font-size: 5.3pt;
      color: #9C8C78;
      font-family: 'Norse', serif;
      text-transform: uppercase;
      padding: 0 2px;
      background: #251B12;
      border-radius: 2px;
    }}
    .feat-card-desc {{
      font-size: 6.5pt;
      color: #BFB2A2;
      line-height: 1.16;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }}

    table.matrix-view {{
      font-size: 6.4pt;
      text-align: center;
    }}
    table.matrix-view th, table.matrix-view td {{
      padding: 1.6px 1px;
      border: 1px solid #3E2E1D;
    }}
    table.matrix-view th.feat-hdr {{
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      height: 62px;
      font-size: 6.1pt;
      padding: 2px 1px;
      background: #20170F;
      color: #FFBF1F;
    }}
    table.matrix-view th.worker-hdr {{
      text-align: left;
      padding-left: 4px;
      width: 84px;
      background: #251B12;
    }}
    table.matrix-view td.w-name {{
      text-align: left;
      font-family: 'Norsebold', serif;
      padding-left: 4px;
      background: #1B140E;
      color: #FCD34D;
      font-size: 6.9pt;
    }}
    .m-yes {{
      color: #78C946;
      font-weight: bold;
      font-size: 7.1pt;
    }}
    .m-no {{
      color: #4A3724;
      font-size: 5pt;
    }}
  </style>
</head>
<body>
""")

# ==================== PAGE 1: COVER ====================
parts.append(f"""
  <div class="page page-first cover-page">
    <div style="flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%;">
      <span class="role-pill" style="font-size: 8pt; padding: 3px 12px; margin-bottom: 4px;">ValheimWorkers v0.3.0 &bull; Official Compendium</span>
      <h1 class="cover-title">THE SETTLEMENT<br>STEWARD'S CODEX</h1>
      <p class="cover-subtitle">Autonomous AI Behaviors, Spatial Memory, 36-Feature Map &amp; Domain Operations</p>
      
      <div class="cover-divider"></div>

      <div class="cover-crest">
        <img src="{ic("Bronze_Axe.png")}" class="flank-tool" alt="Bronze Axe">
        <img src="{ic("Banded_Shield.png")}" class="main-shield" alt="Banded Shield">
        <img src="{ic("Finewood_Bow.png")}" class="flank-tool" alt="Finewood Bow">
      </div>

      <div class="cover-divider"></div>

      <div class="cover-grid">
        <div class="cover-card">
          <img src="{ic("Workbench.png")}" alt="Workbench">
          <div>
            <strong>Autonomous Labor Engine</strong>
            <p>11 specialized roles with spatial memory, auto-repair, smart sorting &amp; unstuck hops.</p>
          </div>
        </div>
        <div class="cover-card">
          <img src="{ic("Cultivator.png")}" alt="Cultivator">
          <div>
            <strong>36 Specialized Feature Icons</strong>
            <p>Master F01–F36 taxonomy categorizing harvesting, defense, refining, and social life.</p>
          </div>
        </div>
        <div class="cover-card">
          <img src="{ic("Cart.png")}" alt="Cart">
          <div>
            <strong>Full Logistics Infrastructure</strong>
            <p>Overflow resource piles (wood, stone, coal), sign [Tag] routing &amp; Vagon cart loading.</p>
          </div>
        </div>
        <div class="cover-card">
          <img src="{ic("Iron_Sword.png")}" alt="Sword">
          <div>
            <strong>Adaptive Terrain &amp; Combat</strong>
            <p>Hoe quarry leveling, ore exposure &amp; parabolic archery with tactical CQC swap.</p>
          </div>
        </div>
      </div>

      <div class="cover-divider"></div>
      
      <div style="margin-top: 6px; font-size: 7.2pt; color: #8C7B68; font-family: 'Averia Serif Libre', serif;">
        Valheim BepInEx 5.4.22+ &bull; Harmony 2.0 &bull; Authored for High-Immersion Nordic Strongholds
      </div>
    </div>
  </div>
""")

# ==================== PAGE 2: TABLE OF CONTENTS ====================
parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Hugin.png")}" class="chapter-icon" alt="Hugin"> Table of Contents &amp; Executive Blueprint</h2>
        <span class="role-pill">Codex Index</span>
      </div>

      <div class="toc-grid">
        <div class="toc-card">
          <h4><img src="{ic("Workbench.png")}" alt="Icon"> Part I: Foundations &amp; System Architecture</h4>
          <ul class="toc-list">
            <li><span><img src="{ic("Hammer.png")}" alt="Icon"> Core Controls &amp; In-World Hotkeys</span> <span>Page 3</span></li>
            <li><span><img src="{ic("Sign.png")}" alt="Icon"> Dual-Anchor Spatial Post Architecture</span> <span>Page 3</span></li>
            <li><span><img src="{ic("Reinforced_Chest.png")}" alt="Icon"> 8&times;4 Container Storage &amp; Sorting</span> <span>Page 3</span></li>
            <li><span><img src="{ic("Deer_Stew.png")}" alt="Icon"> 3-Slot Vitality &amp; Sustenance Rations</span> <span>Page 3</span></li>
            <li><span><img src="{ic("Coins.png")}" alt="Icon"> Recruitable Heritages (Viking, Dvergr, Skeleton)</span> <span>Page 3</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4><img src="{ic("Bronze_Axe.png")}" alt="Icon"> Part II: 36-Feature Map &amp; Separation Matrix</h4>
          <ul class="toc-list">
            <li><span><img src="{ic("Cultivator.png")}" alt="Icon"> The 36-Feature Icon Catalog (F01–F36)</span> <span>Page 4</span></li>
            <li><span><img src="{ic("Smelter.png")}" alt="Icon"> Master Worker &times; Feature Capability Matrix</span> <span>Page 5</span></li>
            <li><span><img src="{ic("Hearth.png")}" alt="Icon"> Domain Classification &amp; Operational Hierarchy</span> <span>Page 5</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4><img src="{ic("Stone_Axe.png")}" alt="Icon"> Part III: Harvesting &amp; Extraction Roles</h4>
          <ul class="toc-list">
            <li><span><img src="{ic("Bronze_Axe.png")}" alt="Icon"> The Lumberjack (Forester &amp; Arborist)</span> <span>Page 6</span></li>
            <li><span><img src="{ic("Antler_Pickaxe.png")}" alt="Icon"> The Miner (Quarryman &amp; Excavator)</span> <span>Page 7</span></li>
            <li><span><img src="{ic("Cultivator.png")}" alt="Icon"> The Farmer (Husbandman &amp; Cultivator)</span> <span>Page 8</span></li>
            <li><span><img src="{ic("Raspberries.png")}" alt="Icon"> The Gatherer (Forager &amp; Apiarist)</span> <span>Page 9</span></li>
            <li><span><img src="{ic("Finewood_Bow.png")}" alt="Icon"> The Hunter (Ranger &amp; Marksman)</span> <span>Page 10</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4><img src="{ic("Iron_Sword.png")}" alt="Icon"> Part IV: Base Production &amp; Defense Roles</h4>
          <ul class="toc-list">
            <li><span><img src="{ic("Banded_Shield.png")}" alt="Icon"> The Guard (Perimeter Sentinel &amp; Warden)</span> <span>Page 11</span></li>
            <li><span><img src="{ic("Smelter.png")}" alt="Icon"> The Smelter (Furnace Master &amp; Pyrotechnic)</span> <span>Page 12</span></li>
            <li><span><img src="{ic("Windmill.png")}" alt="Icon"> The Base Tender (Camp Steward &amp; Miller)</span> <span>Page 13</span></li>
            <li><span><img src="{ic("Cauldron.png")}" alt="Icon"> The Cook (Culinary Artisan &amp; Brewer)</span> <span>Page 14</span></li>
            <li><span><img src="{ic("Cart.png")}" alt="Icon"> The Hauler (Logistics Operator &amp; Packmaster)</span> <span>Page 15</span></li>
            <li><span><img src="{ic("Hammer.png")}" alt="Icon"> The Craftsman (Artificer &amp; Carpenter)</span> <span>Page 16</span></li>
          </ul>
        </div>
      </div>

      <div style="margin-top: 6px;" class="toc-card">
        <h4><img src="{ic("Longship.png")}" alt="Icon"> Part V: Settlement Logistics, Diagnostics &amp; Specifications</h4>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 6px; margin-top: 3px; font-size: 7.2pt;">
          <div><strong style="color: #FCD34D;">Page 17:</strong> Logistics Mastery: Overflow Piles [F05], Sign Tags [F32], Carts &amp; Ships [F34]</div>
          <div><strong style="color: #FCD34D;">Page 18:</strong> Diagnostics, Edge Cases &amp; Automated Verification Telemetry Suite</div>
          <div style="grid-column: span 2;"><strong style="color: #FCD34D;">Page 19:</strong> Master Settlement Roster &amp; Comparative Capability Matrix</div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 2</span>
    </div>
  </div>
""")

# ==================== PAGE 3: FOUNDATIONS ====================
parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Workbench.png")}" class="chapter-icon" alt="Workbench"> Part I: Core Controls &amp; Foundations</h2>
        <span class="role-pill">Spatial Logic &amp; Vitality</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Native 8&times;4 Grid (Equipped Cyan Borders):</span>
          {slot("Bronze_Axe.png", "Axe", True)}
          {slot("Banded_Shield.png", "Shield", True)}
          {slot("Iron_Scale_Mail.png", "Armor", True)}
          {slot("Wood_Arrow.png", "Arrows", True)}
          {slot("Wood.png", "Cargo", False)}
          {slot("Stone.png", "Cargo", False)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">3-Slot Stomach:</span>
          {slot("Serpent_Stew.png", "Stew", False)}
          {slot("Sausages.png", "Meat", False)}
          {slot("Queens_Jam.png", "Jam", False)}
        </div>
      </div>

      <div class="panel panel-rec">
        <h3>1. Direct Controls &amp; In-World Directives</h3>
        <ul>
          <li><strong>F8 (Settlement Hall UI):</strong> Opens management dashboard. Unlocks cursor, pauses player attacks/camera, and provides access to Recruitment, Roster Management, and Telemetry.</li>
          <li><strong>[E] (Tap on Worker):</strong> Instant toggle between <strong>Follow Me</strong> and <strong>Station at Post</strong>. When stationed, anchors location and resumes last assigned role.</li>
          <li><strong>[Shift + E] (Tap on Worker):</strong> Quick-cycles through all allowed roles in the world, accompanied by a speech bubble announcement.</li>
          <li><strong>[Hold E] (&gt;0.38s):</strong> Opens the worker's native <strong>8&times;4 (32-slot) Container Inventory</strong>. Right-clicking equipment inside equips/unequips it immediately with a cyan border.</li>
          <li><strong>[Alt + Hold E] (&gt;0.38s):</strong> Opens F8 Settlement Hall focused directly onto the targeted worker.</li>
        </ul>
      </div>

      <div class="panel panel-now">
        <h3>2. Dual-Anchor Spatial Post Architecture</h3>
        <p>Workers maintain two distinct spatial coordinates saved permanently in their ZDO (Zone Data Object):</p>
        <ul>
          <li><strong>Camp Post (<code>HomePosition</code>):</strong> The domestic home base where workers deposit harvested goods into chests, withdraw fuels/raw meats, eat meals, and retreat during raids.</li>
          <li><strong>Work Post (<code>WorkPostPosition</code>):</strong> The remote job center where tasks like tree felling, mining, and patrols occur. Workers commute to their Work Post to perform labor, then hike back to their Camp Post when full!</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>3. 3-Slot Nourishment &amp; Recruitable Heritages</h3>
        <ul>
          <li><strong>3-Slot Stomach:</strong> Workers consume up to 3 distinct food items to gain maximum health, stamina, and passive health regeneration. They eat from camp chests or carried rations.</li>
          <li><strong>Viking Worker:</strong> 50 Coins cost, 150 Base HP, full custom male/female cosmetics, all 11 roles available.</li>
          <li><strong>Dvergr Laborer:</strong> 75 Coins cost, 200 Base HP, spawns with Tier 2 Bronze gear, immune to minor stagger.</li>
          <li><strong>Skeleton Servant:</strong> 20 Bone Fragments, 80 HP, low-cost utility for Hauler, Guard, and basic chores.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 3</span>
    </div>
  </div>
""")

# ==================== PAGE 4: 36-FEATURE ICON CATALOG ====================
features_list = [
    ("F01", "Forestry & Felling", "Bronze_Axe.png", "Forestry", "Fells mature trees matching equipped axe tier."),
    ("F02", "Stump Eradication", "Wood.png", "Forestry", "Prioritizes stumps and fallen logs for clean clearings."),
    ("F03", "Forest Replanting", "Beech_Seeds.png", "Forestry", "Replants pine, fir, beech, and birch in perimeter plots."),
    ("F04", "Seed Extraction", "Pine_Cone.png", "Forestry", "Extracts tree seeds and cones from falling timber."),
    ("F05", "Overflow Pile Stacking", "Wood_Pile.png", "Logistics", "Constructs 50x wood, stone, and coal resource stacks."),
    ("F06", "Surface Quarrying", "Antler_Pickaxe.png", "Mining", "Shatters surface boulders and flint nodes with pickaxe."),
    ("F07", "Deep Vein Excavation", "Copper_Ore.png", "Mining", "Excavates copper, tin, silver, and iron scrap deposits."),
    ("F08", "Even Field (Hoe)", "Hoe.png", "Mining", "Levels jagged quarry floors to eliminate pitfalls."),
    ("F09", "Subterranean Veins", "Stone.png", "Mining", "Lowers overlying soil down to bedrock to expose veins."),
    ("F10", "Crop Harvesting", "Carrot.png", "Agriculture", "Harvests ripe vegetables and grains without damage."),
    ("F11", "Soil Cultivation", "Cultivator.png", "Agriculture", "Cultivates fresh garden plots with Cultivator."),
    ("F12", "Proactive Sowing", "Carrot_Seeds.png", "Agriculture", "Pulls seeds from chests and proactively plants every plot."),
    ("F13", "Seed Expansion", "Turnip_Seeds.png", "Agriculture", "Maintains 1:3 seed-to-produce perpetual crop cycles."),
    ("F14", "Wild Plant Foraging", "Raspberries.png", "Forestry", "Forages raspberries, blueberries, mushrooms, and thistle."),
    ("F15", "Apiary & Sap Harvest", "Beehive.png", "Culinary", "Harvests beehives and taps Yggdrasil sap extractors."),
    ("F16", "Livestock Feeding", "Boar.png", "Agriculture", "Feeds tamed boars, lox, and wolves with matching foods."),
    ("F17", "Wilderness Stalking", "Deer.png", "Combat", "Sneak-stalks wild game with reduced detection radius."),
    ("F18", "Hunter-Grade Archery", "Finewood_Bow.png", "Combat", "Parabolic long-range archery with lead shot calculations."),
    ("F19", "Tactical CQC Swap", "Iron_Sword.png", "Combat", "Swaps to melee sword and shield when enemy is <=9.5m."),
    ("F20", "Field Gear Repair", "Workbench.png", "Logistics", "Visits workbench to repair worn tools and craft arrows."),
    ("F21", "Protected Gear Check", "Banded_Shield.png", "Logistics", "Prevents active tools and weapons from chest deposit."),
    ("F22", "Raid War Horn Alert", "Troll.png", "Defense", "Sounds War Horn alert against high-threat hostiles."),
    ("F23", "Spit Roasting", "Cooking_Station.png", "Culinary", "Roasts meats on cooking spits without burning into coal."),
    ("F24", "Cauldron Recipe Stews", "Cauldron.png", "Culinary", "Brews deer stew, wolf skewers, and sausages in cauldrons."),
    ("F25", "Fermenter Tapping", "Fermenter.png", "Culinary", "Taps finished mead potions into pantry chests."),
    ("F26", "Pantry Food Sorting", "Deer_Stew.png", "Culinary", "Routes cooked meals into designated [Food] chests."),
    ("F27", "Charcoal Kiln Stoking", "Charcoal_Kiln.png", "Refining", "Feeds wood into kilns to maintain steady coal reserves."),
    ("F28", "Smelter Ore Refining", "Smelter.png", "Refining", "Loads ores and coal into smelters and collects ingots."),
    ("F29", "Windmill Grain Milling", "Windmill.png", "Refining", "Feeds Barley into Windmills to produce Barley Flour."),
    ("F30", "Hearth Fire Stoking", "Hearth.png", "Utility", "Refuels campfires, hearths, sconces, and braziers."),
    ("F31", "Smart Chest Sorting", "Reinforced_Chest.png", "Logistics", "Merges incoming items into matching chest stacks."),
    ("F32", "Sign-Based Tag Routing", "Sign.png", "Logistics", "Parses [Wood], [Ore], [Food] bracketed sign tags."),
    ("F33", "Vagon Cart Loading", "Cart.png", "Logistics", "Loads bulky timber and ore into wheeled player carts."),
    ("F34", "Longship Boarding", "Longship.png", "Social", "Boards player boats and anchors safely during voyages."),
    ("F35", "Night Torch Bearing", "Torch.png", "Social", "Equips and lights torches during darkness and night."),
    ("F36", "Tavern Rest & Seating", "Dragon_Bed.png", "Social", "Rests in chairs, benches, and beds when off duty.")
]

feat_cards_html = ""
for code, name, icon_fn, domain, desc in features_list:
    feat_cards_html += f"""
        <div class="feat-card-comp">
          <div class="feat-img-box"><img src="{ic(icon_fn)}" alt="{code}"></div>
          <div class="feat-card-body">
            <div class="feat-card-head">
              <strong>{code} {name}</strong>
              <span class="feat-card-domain">{domain}</span>
            </div>
            <div class="feat-card-desc">{desc}</div>
          </div>
        </div>
    """

parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Sign.png")}" class="chapter-icon" alt="Sign"> Part II: The 36-Feature Visual Catalog (F01–F36)</h2>
        <span class="role-pill">Capability Taxonomy</span>
      </div>

      <p style="font-size: 7.2pt; color: #B5A998; margin-bottom: 3px;">Every worker behavior in ValheimWorkers v0.3.0 is indexed into a standardized 36-feature taxonomy across 8 operational domains:</p>

      <div class="feat-catalog-grid">
        {feat_cards_html}
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 4</span>
    </div>
  </div>
""")

# ==================== PAGE 5: CAPABILITY MATRIX ====================
matrix_rows = [
    ("Lumberjack", [True,True,True,True,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,False, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Miner",      [False,False,False,False,True, True,True,True,True, False,False,False,False, False,False,False, False,False,False,False, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Farmer",     [False,False,False,False,True, False,False,False,False, True,True,True,True, False,False,True, False,False,False,False, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Gatherer",   [False,False,False,False,True, False,False,False,False, False,False,False,False, True,True,False, False,False,False,False, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Hunter",     [False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, True,True,False,True, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Guard",      [False,False,False,False,False, False,False,False,False, False,False,False,False, False,False,False, False,True,True,True, True,True, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Smelter",    [False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,False, True,False, False,False,False,False, True,True,False,False, True,True,True,True,True,True]),
    ("Base Tender",[False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,False, True,False, False,False,False,False, False,False,True,True, True,True,True,True,True,True]),
    ("Cook",       [False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,False, True,False, True,True,True,True, False,False,False,False, True,True,True,True,True,True]),
    ("Hauler",     [False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,False, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
    ("Craftsman",  [False,False,False,False,True, False,False,False,False, False,False,False,False, False,False,False, False,False,False,True, True,False, False,False,False,False, False,False,False,False, True,True,True,True,True,True]),
]

matrix_tbody = ""
for role_name, bools in matrix_rows:
    tds = ""
    for b in bools:
        tds += '<td class="m-yes">&#10004;</td>' if b else '<td class="m-no">&bull;</td>'
    matrix_tbody += f'<tr><td class="w-name">{role_name}</td>{tds}</tr>'

feat_headers = "".join([f'<th class="feat-hdr">{code}</th>' for code, _, _, _, _ in features_list])

parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Banded_Shield.png")}" class="chapter-icon" alt="Shield"> Part II: Worker &times; Feature Separation Matrix</h2>
        <span class="role-pill">Master Separation Grid</span>
      </div>

      <p style="font-size: 7.1pt; color: #B5A998; margin-bottom: 3px;">Comprehensive matrix verifying strict functional separation across all 11 worker roles and 36 capabilities:</p>

      <table class="matrix-view">
        <thead>
          <tr>
            <th class="worker-hdr">Worker Role</th>
            {feat_headers}
          </tr>
        </thead>
        <tbody>
          {matrix_tbody}
        </tbody>
      </table>

      <div class="panel panel-now" style="margin-top: 4px;">
        <h3><img src="{ic("Hearth.png")}" style="width: 13px; height: 13px; object-fit: contain;" alt="Hearth"> Universal Core Capabilities</h3>
        <p><strong>Universal Features:</strong> All workers possess <code>[F21] Protected Gear Retention</code>, <code>[F31] Smart Storage Sorting</code>, <code>[F32] Sign-Based Tag Routing</code>, <code>[F34] Longship Boarding</code>, <code>[F35] Night Torch Bearing</code>, and <code>[F36] Tavern Rest &amp; Seating</code>.</p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 5</span>
    </div>
  </div>
""")

# ==================== CHAPTER TEMPLATE HELPER ====================
def make_worker_chapter(page_num, part_roman, role_title, role_sub, role_pill, icon_main, slots_left, stations_right, panel_now, panel_future, panel_rec, panel_warn):
    slots_left_html = "".join([slot(s[0], s[1], s[2]) for s in slots_left])
    stations_right_html = "".join([slot(s[0], s[1], False) for s in stations_right])
    
    return f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic(icon_main)}" class="chapter-icon" alt="{role_title}"> {part_roman}: {role_title} ({role_sub})</h2>
        <span class="role-pill">{role_pill}</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Recommended Loadout:</span>
          {slots_left_html}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">Interactive Stations &amp; Yields:</span>
          {stations_right_html}
        </div>
      </div>

      <div class="panel panel-now">
        {panel_now}
      </div>

      <div class="panel panel-future">
        {panel_future}
      </div>

      <div class="panel panel-rec">
        {panel_rec}
      </div>

      <div class="panel panel-warn">
        {panel_warn}
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page {page_num}</span>
    </div>
  </div>
"""

# ==================== PAGE 6: LUMBERJACK ====================
parts.append(make_worker_chapter(
    6, "Part III", "The Lumberjack", "Forester &amp; Arborist", "Forestry &bull; Harvesting",
    "Bronze_Axe.png",
    [("Bronze_Axe.png", "Axe", True), ("Rag_Tunic.png", "Tunic", True), ("Torch.png", "Torch", False), ("Wood.png", "Cargo", False)],
    [("Wood_Pile.png", "Pile"), ("Fine_Wood.png", "Fine"), ("Core_Wood.png", "Core"), ("Beech_Seeds.png", "Seeds"), ("Resin.png", "Resin")],
    f"""
    <h3>{feat("F01", "Timber Felling", "Bronze_Axe.png")} {feat("F02", "Stump Priority", "Wood.png")} {feat("F05", "Overflow Piles", "Wood_Pile.png")} Current Core Capabilities</h3>
    <ul>
      <li><strong>[F01] Selective Timber Felling:</strong> Scans within Work Post radius for beech, birch, oak, and fir trees matching equipped Axe tier (Stone, Flint, Bronze, Iron, Blackmetal).</li>
      <li><strong>[F02] Stump-First Eradication Priority:</strong> Prioritizes sectioning felled logs and eradicating stumps with a -15m distance bonus over standing trees to keep settlement clearings clean.</li>
      <li><strong>[F05] Overflow Resource Stacking:</strong> When camp chests are full, automatically builds physical 50x <code>wood_stack</code>, <code>wood_fine_stack</code>, or <code>wood_core_stack</code> pieces.</li>
      <li><strong>[F32] Sign-Based Routing:</strong> Routes timber directly into chests labeled <code>[Wood]</code>, <code>[FineWood]</code>, or <code>[CoreWood]</code> before general storage.</li>
    </ul>
    """,
    f"""
    <h3>{feat("F03", "Nursery", "Beech_Seeds.png")} {feat("F04", "Cones", "Pine_Cone.png")} Planned Future Expansions</h3>
    <ul>
      <li><strong>[F03] Automated Tree Nursery:</strong> Proactive sapling plantation in designated reforestation perimeters away from buildings.</li>
      <li><strong>[F04] Seed Extraction &amp; Pine Resin Tapping:</strong> Extracting cones and seeds during felling and storing them for architectural expansion.</li>
      <li><strong>Directional Wedge Felling:</strong> Felling physics prediction to prevent heavy timber from crushing nearby player fences.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Place Work Post at the boundary of a mature grove; position a chest labeled <code>[Wood]</code> near the main logging gate.</li>
      <li>Equip at least a Bronze Axe if birch or fine wood trees are within radius to prevent tool-tier warning pauses.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Rolling Logs:</strong> Downhill logs can roll far on steep slopes; tether work post tightly on cliffs to avoid extended hauling trips.</li>
    </ul>
    """
))

# ==================== PAGE 7: MINER ====================
parts.append(make_worker_chapter(
    7, "Part IV", "The Miner", "Quarryman &amp; Excavator", "Mining &bull; Extraction",
    "Antler_Pickaxe.png",
    [("Bronze_Pickaxe.png", "Pickaxe", True), ("Hoe.png", "Hoe", True), ("Torch.png", "Torch", False), ("Stone.png", "Cargo", False)],
    [("Stone_Pile.png", "Pile"), ("Copper_Ore.png", "Copper"), ("Tin_Ore.png", "Tin"), ("Iron_Scrap.png", "Iron"), ("Flint.png", "Flint")],
    f"""
    <h3>{feat("F06", "Quarrying", "Antler_Pickaxe.png")} {feat("F07", "Deep Veins", "Copper_Ore.png")} {feat("F08", "Hoe Leveling", "Hoe.png")} {feat("F09", "Overburden", "Stone.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F06] Surface Quarrying:</strong> Shatters surface rocks, loose boulders, and flint nodes with pickaxe.</li>
      <li><strong>[F07] Deep Vein Excavation:</strong> Extracts Copper, Tin, Iron Scrap, and Silver veins using tool tier verification (Antler, Bronze, Iron).</li>
      <li><strong>[F08] Even the Field (Hoe Leveling):</strong> Equips a Hoe to level and smooth jagged quarry terrain after shattering rock faces, eliminating pit traps.</li>
      <li><strong>[F09] Subterranean Vein Exposure:</strong> Lowers overlying soil layers to expose subterranean chunks of buried <code>MineRock5</code> veins.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Constructs 50x <code>stone_pile</code> pieces when camp storage chests cannot accept more stone.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Perimeter Staircarving:</strong> Auto-cutting stone steps along pit perimeters for easy cart descent.</li>
      <li><strong>Minecart Rail Hoppers:</strong> Loading subterranean ore hoppers connected to surface pulley stations.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Provide both a Pickaxe and a Hoe in the miner's inventory to unlock automated pit leveling.</li>
      <li>Mark chests near the foundry with <code>[Stone]</code> and <code>[Ore]</code> for automatic sign routing.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Tool Durability:</strong> Deep copper deposits rapidly deplete pickaxe durability; place an upgraded workbench near the quarry.</li>
    </ul>
    """
))

# ==================== PAGE 8: FARMER ====================
parts.append(make_worker_chapter(
    8, "Part V", "The Farmer", "Husbandman &amp; Cultivator", "Agriculture &bull; Sustenance",
    "Cultivator.png",
    [("Cultivator.png", "Cultivator", True), ("Rag_Tunic.png", "Tunic", True), ("Carrot_Seeds.png", "Seeds", False), ("Carrot.png", "Yield", False)],
    [("Carrot.png", "Carrot"), ("Turnip.png", "Turnip"), ("Onion.png", "Onion"), ("Barley.png", "Barley"), ("Boar.png", "Tamed")],
    f"""
    <h3>{feat("F10", "Harvesting", "Carrot.png")} {feat("F11", "Soil Prep", "Cultivator.png")} {feat("F12", "Proactive Sowing", "Carrot_Seeds.png")} {feat("F16", "Feeding", "Boar.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F10] Crop Harvesting:</strong> Harvests ripe carrots, turnips, onions, barley, and flax within garden perimeter.</li>
      <li><strong>[F11] Soil Cultivation:</strong> Prepares fresh cultivated arable land with Cultivator when existing plots are fully planted.</li>
      <li><strong>[F12] Proactive Sowing (Every Seed):</strong> Withdraws vegetable/grain seeds from camp chests and proactively sows every available seed across tilled soil.</li>
      <li><strong>[F13] Automatic Seed Expansion:</strong> Maintains perpetual cycle by planting seed crops to yield 3x seeds per plant.</li>
      <li><strong>[F16] Tamed Livestock Feeding:</strong> Scans for hungry tamed Boars, Lox, Chickens, and Wolves, dropping crops or meats into their pens.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Livestock Breeding Cap:</strong> Culling excess animals once pen population exceeds 12 to maintain game performance.</li>
      <li><strong>Irrigation Ditches:</strong> Trench digging for accelerated crop maturation during rainy weather.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Enclose farmland with stake walls to prevent wild boars or greydwarfs from trampling tender seedlings.</li>
      <li>Label chests with <code>[Seeds]</code> and <code>[Food]</code> near garden gates for automated resupply.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Cultivator Tier:</strong> Farmer requires a Bronze Cultivator to till soil and plant crops; cannot plant without tool.</li>
    </ul>
    """
))

# ==================== PAGE 9: GATHERER ====================
parts.append(make_worker_chapter(
    9, "Part VI", "The Gatherer", "Forager &amp; Apiarist", "Harvesting &bull; Foraging",
    "Raspberries.png",
    [("Torch.png", "Torch", False), ("Rag_Tunic.png", "Tunic", True), ("Honey.png", "Honey", False), ("Raspberries.png", "Berries", False)],
    [("Beehive.png", "Hive"), ("Sap_Extractor.png", "Sap Ext"), ("Blueberries.png", "Berry"), ("Mushroom.png", "Shroom"), ("Thistle.png", "Thistle")],
    f"""
    <h3>{feat("F14", "Foraging", "Raspberries.png")} {feat("F15", "Apiary", "Beehive.png")} {feat("F05", "Overflow", "Wood_Pile.png")} Current Core Capabilities</h3>
    <ul>
      <li><strong>[F14] Flora &amp; Fungi Foraging:</strong> Gathers raspberries, blueberries, cloudberries, red/yellow mushrooms, thistle, and dandelion.</li>
      <li><strong>[F15] Apiary &amp; Sap Extraction:</strong> Harvests honeycomb from camp beehives and extracts raw Yggdrasil sap from Sap Collectors.</li>
      <li><strong>Toolless Loose Gathering:</strong> Collects loose ground stones, branches, and flint without requiring equipped axes or pickaxes.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Builds resource piles when inventory is overflowing and camp chests are congested.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Mushroom Spore Cultivation:</strong> In-ground potting of glowing yellow mushrooms inside cellar basements.</li>
      <li><strong>Wisp Harvesting:</strong> Automatic collection of floating wisps from wisp traps during Mistlands nights.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Group beehives together in an open sunny courtyard and place a chest labeled <code>[Food]</code> or <code>[Berries]</code> nearby.</li>
      <li>Gatherers are excellent for early-game camp maintenance since they do not consume tool durability.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Wild Bush Regrowth:</strong> Natural berry bushes take 5 in-game hours to fruit; gatherer will idle once perimeter is cleared.</li>
    </ul>
    """
))

# ==================== PAGE 10: HUNTER ====================
parts.append(make_worker_chapter(
    10, "Part VII", "The Hunter", "Ranger &amp; Marksman", "Combat &bull; Game Trapping",
    "Finewood_Bow.png",
    [("Finewood_Bow.png", "Bow", True), ("Wood_Arrow.png", "Arrows", True), ("Rag_Tunic.png", "Tunic", True), ("Deer_Stew.png", "Ration", False)],
    [("Deer.png", "Prey"), ("Raw_Meat.png", "Meat"), ("Boar_Jerky.png", "Hide"), ("Workbench.png", "Bench"), ("Campfire.png", "Hearth")],
    f"""
    <h3>{feat("F17", "Stalking", "Deer.png")} {feat("F18", "Archery", "Finewood_Bow.png")} {feat("F20", "Repair", "Workbench.png")} Current Core Capabilities</h3>
    <ul>
      <li><strong>[F17] Wilderness Stalking:</strong> Tracks deer, boar, neck, and wild fowl within work radius using stealth posture and wind evasion.</li>
      <li><strong>[F18] Hunter-Grade Archery:</strong> Calculates target velocity, lead distance, and parabolic arc to fire precise bow shots up to 35m.</li>
      <li><strong>[F20] Arrow Crafting &amp; Field Repair:</strong> Automatically visits workbench to repair bow durability and crafts fresh wood arrows if supply falls below 40.</li>
      <li><strong>Tamed Creature Immunity:</strong> Scans creature flags and strictly ignores tamed boars, wolves, and lox.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Trapping &amp; Snares:</strong> Setting wooden pit traps for wolves and hare in mountain/mistlands borders.</li>
      <li><strong>Scent Masking:</strong> Applying swamp muck to approach skittish deer within point-blank range.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Keep an accessible sheltered workbench near the hunter's post so they can repair bows and craft arrows uninterrupted.</li>
      <li>Mark chests with <code>[Food]</code> and <code>[Trophy]</code> for raw meat, deer hide, and trophy drop storage.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Water Fleeing:</strong> Deer wounded near shorelines may swim out into deep water; hunter will wait on shore for parabolic shot.</li>
    </ul>
    """
))

# ==================== PAGE 11: GUARD ====================
parts.append(make_worker_chapter(
    11, "Part VIII", "The Guard", "Perimeter Sentinel &amp; Warden", "Defense &bull; Security",
    "Banded_Shield.png",
    [("Huntsman_Bow.png", "Bow", False), ("Iron_Sword.png", "Sword", True), ("Banded_Shield.png", "Shield", True), ("Iron_Helmet.png", "Helm", True)],
    [("Troll.png", "Warhorn"), ("Workbench.png", "Repair"), ("Greydwarf.png", "Foe"), ("Standing_Wood_Torch.png", "Beacon"), ("Campfire.png", "Rally")],
    f"""
    <h3>{feat("F18", "Archery", "Finewood_Bow.png")} {feat("F19", "CQC Swap", "Iron_Sword.png")} {feat("F20", "Repair", "Workbench.png")} {feat("F22", "War Horn", "Troll.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F18] Hunter-Grade Archery (Long Range):</strong> Equips bow and picks off invading greydwarfs, skeletons, and trolls at 10m–40m with lead calculations.</li>
      <li><strong>[F19] Tactical CQC Weapon Swap:</strong> Automatically draws melee sword/axe and raises shield when enemies close within 9.5m.</li>
      <li><strong>[F22] War Horn Alert &amp; Multi-Waypoint Patrol:</strong> Walks defensive circuits between Camp Post and Work Post; sounds War Horn alert when threat &ge; 150.</li>
      <li><strong>[F20] Off-Combat Workbench Repair:</strong> Retreats to camp workbench after skirmishes to repair armor, weapons, and restock arrows.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Ballista Battery Manning:</strong> Loading and targeting camp ballistas against airborne drakes and gjalls.</li>
      <li><strong>Palisade Gate Locking:</strong> Closing gates during active raids and raising drawbridges.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Station Guards on elevated watchtowers or gatehouse bastions with clear sightlines over camp entrances.</li>
      <li>Provide a sturdy bow, 100+ arrows, a sword or mace, and a round/banded shield for optimal stance swapping.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Raid Mob Swarms:</strong> Trolls with tree clubs deal area-of-effect damage; keep guards elevated above club sweep radius.</li>
    </ul>
    """
))

# ==================== PAGE 12: SMELTER ====================
parts.append(make_worker_chapter(
    12, "Part IX", "The Smelter", "Furnace Master &amp; Pyrotechnic", "Foundry &bull; Refining",
    "Smelter.png",
    [("Hammer.png", "Hammer", False), ("Rag_Tunic.png", "Tunic", True), ("Coal.png", "Coal", False), ("Copper_Ore.png", "Ore", False)],
    [("Smelter.png", "Furnace"), ("Charcoal_Kiln.png", "Kiln"), ("Blast_Furnace.png", "Blast"), ("Copper.png", "Ingot"), ("Coal_Pile.png", "Pile")],
    f"""
    <h3>{feat("F27", "Kiln", "Charcoal_Kiln.png")} {feat("F28", "Smelting", "Smelter.png")} {feat("F05", "Overflow", "Coal_Pile.png")} Current Core Capabilities</h3>
    <ul>
      <li><strong>[F27] Charcoal Kiln Stoking:</strong> Feeds regular wood, fine wood, or core wood into kilns to maintain uninterrupted coal reserves.</li>
      <li><strong>[F28] Smelter &amp; Blast Furnace Refining:</strong> Transports raw copper, tin, iron, silver, and blackmetal ores into smelters, feeding fuel coal simultaneously.</li>
      <li><strong>Ingot Collection &amp; Sorting:</strong> Sweeps refined copper, bronze, iron, silver, and blackmetal ingots from smelter output chutes into designated chests.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Builds <code>coal_pile</code> stacks when coal chests are filled to capacity.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Eitr Refinery Operation:</strong> Loading soft tissue and sap into eitr refineries with rad-hazard protective timing.</li>
      <li><strong>Automated Bronze Forging:</strong> Combining copper and tin directly inside forge cauldrons.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Place Smelter and Kiln input ports within 6m of chests marked <code>[Ore]</code>, <code>[Wood]</code>, and <code>[Coal]</code>.</li>
      <li>Leave 2m clear walking space in front of smelters to prevent the worker from clipping on hot output chutes.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Weight Congestion:</strong> Raw ores are heavy; smelters carry manageable batches to avoid encumbrance speed debuffs.</li>
    </ul>
    """
))

# ==================== PAGE 13: BASE TENDER ====================
parts.append(make_worker_chapter(
    13, "Part X", "The Base Tender", "Camp Steward &amp; Miller", "Camp Utility &bull; Hearth",
    "Windmill.png",
    [("Torch.png", "Torch", True), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Fuel", False), ("Barley.png", "Grain", False)],
    [("Windmill.png", "Mill"), ("Spinning_Wheel.png", "Wheel"), ("Hearth.png", "Hearth"), ("Barley_Flour.png", "Flour"), ("Linen_Thread.png", "Linen")],
    f"""
    <h3>{feat("F29", "Windmill", "Windmill.png")} {feat("F30", "Hearths", "Hearth.png")} {feat("F05", "Overflow", "Wood_Pile.png")} Current Core Capabilities</h3>
    <ul>
      <li><strong>[F29] Windmill &amp; Spinning Wheel Tending:</strong> Loads harvested Barley into Windmills to grind barley flour, and feeds Flax into Spinning Wheels for linen thread.</li>
      <li><strong>[F30] Hearth, Fireplace &amp; Sconce Refueling:</strong> Keeps campfires, stone hearths, hanging braziers, wall sconces, and standing torches burning by refueling wood, resin, and coal.</li>
      <li><strong>Camp Comfort Maintenance:</strong> Ensures resting player always enjoys maximum comfort bonus upon returning to base.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Builds wood and coal piles when fuel storage is full.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Hot Tub &amp; Bath Stoking:</strong> Heating water in Viking cedar tubs for extended luxury rest bonuses.</li>
      <li><strong>Roof Thatching Repair:</strong> Using hammer to fix weather-decayed roof beams after heavy rainstorms.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Place a central fuel chest labeled <code>[Wood]</code> and <code>[Resin]</code> in the main courtyard.</li>
      <li>One Base Tender can maintain up to 20 fires and 4 windmills within a 25m Camp Post radius.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Resin Depletion:</strong> If base has dozens of wall sconces, resin will deplete quickly; pair Base Tender with a Gatherer.</li>
    </ul>
    """
))

# ==================== PAGE 14: COOK ====================
parts.append(make_worker_chapter(
    14, "Part XI", "The Cook", "Culinary Artisan &amp; Brewer", "Kitchen &bull; Alchemy",
    "Cauldron.png",
    [("KnifeFlint.png" if os.path.exists(os.path.join(ICONS_DIR, "KnifeFlint.png")) else "Bronze_Axe.png", "Knife", False), ("Rag_Tunic.png", "Tunic", True), ("Honey.png", "Honey", False), ("Raw_Meat.png", "Meat", False)],
    [("Cooking_Station.png", "Spit"), ("Cauldron.png", "Cauldron"), ("Fermenter.png", "Fermenter"), ("Deer_Stew.png", "Stew"), ("Medium_Healing_Mead.png", "Mead")],
    f"""
    <h3>{feat("F23", "Spit Roasting", "Cooking_Station.png")} {feat("F24", "Cauldron Stew", "Cauldron.png")} {feat("F25", "Fermenter", "Fermenter.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F23] Spit Roasting &amp; Stew Crafting:</strong> Roasts meats on cooking spits without burning, and simmers deer stew, wolf skewers, and sausages in Cauldrons.</li>
      <li><strong>[F24] Mead Base Alchemy:</strong> Mixes honey and herbal ingredients to prepare health, stamina, poison resist, and frost resist mead bases.</li>
      <li><strong>[F25] Fermenter Tapping:</strong> Inspects camp Fermenters and automatically taps aged mead potions into pantry chests once fermentation completes.</li>
      <li><strong>[F32] Pantry Routing:</strong> Places cooked food items directly into chests marked with <code>[Food]</code>.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Stone Oven Baking:</strong> Baking lox meat pies, bread loaves, and serpent pasties in stone ovens.</li>
      <li><strong>Banquet Serving:</strong> Setting prepared feasts onto longhouse feast tables for instant settlement buffing.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Place pantry chest marked <code>[Food]</code> within 3m of Cauldrons and Fermenters for rapid ingredient retrieval.</li>
      <li>Keep an active cooking fire burning under cauldrons to allow continuous recipe preparation.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Spit Burn Timing:</strong> Cook monitors roasting times; ensure path to cooking station is unobstructed so meats don't turn to charcoal.</li>
    </ul>
    """
))

# ==================== PAGE 15: HAULER ====================
parts.append(make_worker_chapter(
    15, "Part XII", "The Hauler", "Logistics Operator &amp; Packmaster", "Logistics &bull; Freight",
    "Cart.png",
    [("Torch.png", "Torch", False), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Cargo", False), ("Stone.png", "Cargo", False)],
    [("Cart.png", "Vagon"), ("Reinforced_Chest.png", "Chest"), ("Sign.png", "Sign"), ("Wood_Pile.png", "Wood Stack"), ("Stone_Pile.png", "Stone Stack")],
    f"""
    <h3>{feat("F33", "Cart Loading", "Cart.png")} {feat("F05", "Overflow", "Wood_Pile.png")} {feat("F31", "Sorting", "Reinforced_Chest.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F33] Vagon Cart Loading:</strong> Identifies nearby player wheeled carts (<code>Vagon</code>) and loads bulk timber and heavy ores into cart containers.</li>
      <li><strong>[F05] Bulk Pile Construction:</strong> Solves warehouse congestion by constructing physical resource stacks when all chests are full.</li>
      <li><strong>[F31] Camp Storage Consolidation:</strong> Sweeps scattered ground drops across the yard and condenses half-empty chests into unified stacks.</li>
      <li><strong>[F32] Sign-Based Distribution:</strong> Routes items to their designated warehouse bays according to wall signs.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Cart Towing &amp; Hauling:</strong> Physically grasping cart handles and pulling loaded wagons between distant outposts.</li>
      <li><strong>Portal Loading Relay:</strong> Loading non-teleportable metals onto docks while teleportable goods move through portals.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Park your Cart near warehouse gates; Haulers will automatically fill it with refined ingots before your mining trips.</li>
      <li>Construct wide 2m+ ramps and doorways to give Haulers ample turning radius when transporting goods.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Cart Physics:</strong> Park carts on flat leveled terrain so they don't roll away down hills while being loaded.</li>
    </ul>
    """
))

# ==================== PAGE 16: CRAFTSMAN ====================
parts.append(make_worker_chapter(
    16, "Part XIII", "The Craftsman", "Artificer &amp; Carpenter", "Workshop &bull; Manufacturing",
    "Hammer.png",
    [("Hammer.png", "Hammer", True), ("Rag_Tunic.png", "Tunic", True), ("Wood_Arrow.png", "Arrow", False), ("Wood.png", "Wood", False)],
    [("Workbench.png", "Bench"), ("Forge.png", "Forge"), ("Reinforced_Chest.png", "Locker"), ("Fire_Arrow.png", "Fire Arrow"), ("Sign.png", "Sign")],
    f"""
    <h3>{feat("F30", "Workshop", "Workbench.png")} {feat("F20", "Tool Repair", "Hammer.png")} {feat("F31", "Storage", "Reinforced_Chest.png")} Current Capabilities</h3>
    <ul>
      <li><strong>[F30] Workshop Fabrication:</strong> Assembles arrows, building pieces, and camp tools at upgraded Workbenches and Forges.</li>
      <li><strong>[F20] Tool Durability Maintenance:</strong> Keeps settlement tool supply sharp and in working condition.</li>
      <li><strong>Material Requisition:</strong> Pulls wood, feathers, and flint from workshop storage chests to replenish arrow reserves.</li>
      <li><strong>[F32] Sign-Based Tag Routing:</strong> Returns fabricated items into marked output lockers.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Palisade &amp; Wall Repair Rounds:</strong> Patrolling perimeter with hammer to repair raid damage to wooden stakes and gates.</li>
      <li><strong>Armor Forging:</strong> Crafting bronze and iron armor sets according to work order boards.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Build an enclosed smithy containing an upgraded Forge (with anvil, bellows, cooling bucket) and a Workbench.</li>
      <li>Supply a nearby chest labeled <code>[Wood]</code> and <code>[Flint]</code> to support high-volume arrow manufacturing.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Station Roof Requirements:</strong> Crafting stations require shelter (roof overhead) to function; build complete workshops.</li>
    </ul>
    """
))

# ==================== PAGE 17: LOGISTICS MASTERY ====================
parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Cart.png")}" class="chapter-icon" alt="Cart"> Part XIV: Logistics Mastery &amp; Base Infrastructure</h2>
        <span class="role-pill">Storage &amp; Social</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Overflow Stacks:</span>
          {slot("Wood_Pile.png", "Wood", False)}
          {slot("Stone_Pile.png", "Stone", False)}
          {slot("Coal_Pile.png", "Coal", False)}
          {slot("Coin_Pile.png", "Coins", False)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">Social Immersion:</span>
          {slot("Longship.png", "Ship", False)}
          {slot("Torch.png", "Torch", False)}
          {slot("Dragon_Bed.png", "Bed", False)}
        </div>
      </div>

      <div class="panel panel-rec">
        <h3>1. [F05] Physical Overflow Resource Stacks</h3>
        <p>When settlement chests become completely full, workers never drop items to decay. Instead, they assemble physical resource piles on open terrain:</p>
        <ul>
          <li><strong>Wood Piles:</strong> 50x Wood &rarr; <code>wood_stack</code> &bull; 50x Fine Wood &rarr; <code>wood_fine_stack</code> &bull; 50x Core Wood &rarr; <code>wood_core_stack</code>.</li>
          <li><strong>Mineral Piles:</strong> 50x Stone &rarr; <code>stone_pile</code> &bull; 50x Coal &rarr; <code>coal_pile</code> &bull; 50x Bone Fragments &rarr; <code>bone_stack</code>.</li>
          <li><strong>Treasure Stack:</strong> 99x Coins &rarr; <code>treasure_stack</code> (for late-game wealth consolidation).</li>
          <li><strong>Ownership:</strong> Piles are registered with the local player ID, allowing players to deconstruct them instantly with a Hammer.</li>
        </ul>
      </div>

      <div class="panel panel-now">
        <h3>2. [F32] Sign-Based Smart Tag Routing</h3>
        <p>Workers inspect wall signs placed within 2.2m of any chest and parse tag annotations in brackets:</p>
        <ul>
          <li><strong>[Wood]:</strong> Directs regular Wood, Fine Wood, Core Wood, Yggdrasil Wood, and Bark.</li>
          <li><strong>[Stone] &amp; [Ore]:</strong> Routes rocks, flint, obsidian, copper, tin, iron scrap, and silver ores.</li>
          <li><strong>[Food] &amp; [Seeds]:</strong> Segregates cooked dishes, raw meats, honey, and agricultural seeds.</li>
          <li><strong>[Berries] &amp; [Trophy]:</strong> Stores foraged flora, wild berries, mushrooms, and hunted game trophies.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>3. Social Life &amp; Expedition Immersion [F34–F36]</h3>
        <ul>
          <li><strong>[F34] Longship Boarding:</strong> When following player aboard a Karve or Longship, workers automatically mount passenger benches.</li>
          <li><strong>[F35] Night Torch Bearing:</strong> During nighttime (<code>EnvMan.IsNight()</code>) or inside dark dungeons, workers draw lit torches to guide paths.</li>
          <li><strong>[F36] Tavern Seating &amp; Bed Rest:</strong> During off-duty idle cycles, workers visit settlement chairs, stools, and beds to socialize and rest.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 17</span>
    </div>
  </div>
""")

# ==================== PAGE 18: TROUBLESHOOTING & TELEMETRY ====================
parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Hugin.png")}" class="chapter-icon" alt="Hugin"> Part XV: Diagnostics &amp; Console Command Reference</h2>
        <span class="role-pill">F5 Console &bull; Telemetry</span>
      </div>

      <div class="panel panel-warn">
        <h3>Automated Verification Telemetry Engine</h3>
        <p>ValheimWorkers embeds an internal flight recorder tracking 100+ mission parameters in real time. Use F5 console commands to inspect status:</p>
      </div>

      <table>
        <thead>
          <tr>
            <th>Command</th>
            <th>Context</th>
            <th>Operational Effect</th>
          </tr>
        </thead>
        <tbody>
          <tr><td><code>workers_help</code></td><td>F5 Console</td><td>Prints complete list of worker debug, spawning, and diagnostic commands.</td></tr>
          <tr><td><code>workers_verify_status</code></td><td>F5 Console</td><td>Displays real-time pass/fail rates for mining, tree felling, archery, and farming tests.</td></tr>
          <tr><td><code>workers_stats</code></td><td>F5 Console</td><td>Lists all active workers, current roles, health, carried weight, and coordinates.</td></tr>
          <tr><td><code>workers_trace [name]</code></td><td>F5 Console</td><td>Prints the last 10 AI state decisions, path targets, and inventory actions for a worker.</td></tr>
          <tr><td><code>workers_respawn [name]</code></td><td>F5 Console</td><td>Teleports worker to their Camp Post, resets AI state machine, and recovers from stuck loops.</td></tr>
          <tr><td><code>workers_clear</code></td><td>F5 Console</td><td>Dismisses all active workers across the world and removes their minimap pins.</td></tr>
          <tr><td><code>workers_devmode [on|off]</code></td><td>F5 Console</td><td>Toggles DevMode (free hiring, Proving Grounds scenarios, discovery bypass).</td></tr>
          <tr><td><code>workers_test_hunter</code></td><td>F5 Console</td><td>1-click spawns Hunter with bow, wild deer/boar targets, and loot chest.</td></tr>
          <tr><td><code>workers_test_farmer</code></td><td>F5 Console</td><td>1-click spawns Farmer, cultivator, carrot crops, and seed storage.</td></tr>
          <tr><td><code>workers_test_guard</code></td><td>F5 Console</td><td>1-click spawns Guard with bow/sword/shield and hostile Greydwarf for combat verification.</td></tr>
        </tbody>
      </table>

      <div class="panel panel-rec" style="margin-top: 5px;">
        <h3>{hugin()} Common Edge Cases &amp; Practical Solutions</h3>
        <ul>
          <li><strong>Worker Standing Idle at Post:</strong> Verify that resources exist within work radius and that the worker possesses the required tool tier.</li>
          <li><strong>Chests Overflowing:</strong> Check that clear ground exists within 6m of Camp Post so workers can build physical resource stacks.</li>
          <li><strong>Worker Won't Fight:</strong> Check that worker is assigned to <strong>Guard</strong> role and has weapons equipped in their inventory.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 18</span>
    </div>
  </div>
""")

# ==================== PAGE 19: SETTLEMENT ROSTER MATRIX ====================
parts.append(f"""
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2><img src="{ic("Coins.png")}" class="chapter-icon" alt="Coins"> Appendix: Master Settlement Roster &amp; Specifications</h2>
        <span class="role-pill">Comparative Roster</span>
      </div>

      <table>
        <thead>
          <tr>
            <th>Role</th>
            <th>Primary Tool</th>
            <th>Primary Goods</th>
            <th>Work Post Need</th>
            <th>Camp Post Need</th>
            <th>Key Features</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td><strong>Lumberjack</strong></td>
            <td>Axe (Tier 1–4)</td>
            <td>Wood, Fine Wood, Core Wood</td>
            <td>Forest / Timber Grove</td>
            <td>[Wood] Chest / Lumber Piles</td>
            <td>F01, F02, F03, F04, F05, F32</td>
          </tr>
          <tr>
            <td><strong>Miner</strong></td>
            <td>Pickaxe + Hoe</td>
            <td>Stone, Flint, Ores, Scrap</td>
            <td>Quarry / Ore Deposit</td>
            <td>[Stone] / [Ore] Chests</td>
            <td>F05, F06, F07, F08, F09, F32</td>
          </tr>
          <tr>
            <td><strong>Farmer</strong></td>
            <td>Cultivator (Bronze)</td>
            <td>Carrots, Turnips, Barley, Flax</td>
            <td>Tilled Farmland Plots</td>
            <td>[Seeds] / [Food] Chests</td>
            <td>F10, F11, F12, F13, F16, F32</td>
          </tr>
          <tr>
            <td><strong>Gatherer</strong></td>
            <td>None (Bare Hands)</td>
            <td>Berries, Mushrooms, Sap, Honey</td>
            <td>Meadow / Berry Bushes</td>
            <td>[Berries] / [Food] Chests</td>
            <td>F05, F14, F15, F31, F32</td>
          </tr>
          <tr>
            <td><strong>Hunter</strong></td>
            <td>Bow &amp; Arrows</td>
            <td>Raw Meat, Hides, Feathers, Trophies</td>
            <td>Game Trail / Wilderness</td>
            <td>Workbench + [Food] Chest</td>
            <td>F17, F18, F20, F31, F32</td>
          </tr>
          <tr>
            <td><strong>Guard</strong></td>
            <td>Bow + Sword + Shield</td>
            <td>Hostile Drops, Pelts, Trophies</td>
            <td>Watchtower / Gate Post</td>
            <td>Workbench + Armory Chest</td>
            <td>F18, F19, F20, F22, F32</td>
          </tr>
          <tr>
            <td><strong>Smelter</strong></td>
            <td>None (Heavy Hands)</td>
            <td>Metal Ingots, Refined Coal</td>
            <td>Foundry / Blast Furnace</td>
            <td>[Ore], [Coal] &amp; [Wood] Chests</td>
            <td>F05, F26, F27, F31, F32</td>
          </tr>
          <tr>
            <td><strong>Base Tender</strong></td>
            <td>None (Kindling Grip)</td>
            <td>Flour, Linen Thread, Warmth</td>
            <td>Campfires &amp; Windmills</td>
            <td>[Wood], [Resin] &amp; [Coal] Chests</td>
            <td>F05, F28, F29, F31, F32</td>
          </tr>
          <tr>
            <td><strong>Cook</strong></td>
            <td>None (Ladle &amp; Tongs)</td>
            <td>Cooked Meals, Stews, Aged Meads</td>
            <td>Cauldron &amp; Fermenters</td>
            <td>[Food] Pantry Chest</td>
            <td>F05, F23, F24, F25, F32</td>
          </tr>
          <tr>
            <td><strong>Hauler</strong></td>
            <td>None (Packmaster)</td>
            <td>Bulk Freight &amp; Cart Logistics</td>
            <td>Remote Outpost / Quarry</td>
            <td>Vagon Cart + Central Depot</td>
            <td>F05, F31, F32, F33, F34</td>
          </tr>
          <tr>
            <td><strong>Craftsman</strong></td>
            <td>Hammer &amp; Chisel</td>
            <td>Arrows, Building Parts, Tools</td>
            <td>Upgraded Forge / Workshop</td>
            <td>[Wood] &amp; [Metal] Chests</td>
            <td>F20, F30, F31, F32</td>
          </tr>
        </tbody>
      </table>

      <div class="panel panel-rec" style="margin-top: 6px;">
        <h3>{hugin()} Settlement Expansion Blueprint: 10-Worker Village Formula</h3>
        <p>For an optimal self-sustaining Nordic stronghold, recruit: <strong>1 Lumberjack, 1 Miner, 1 Farmer, 1 Gatherer, 1 Hunter, 2 Guards (Gate &amp; Tower), 1 Smelter, 1 Base Tender, and 1 Cook</strong>. This configuration produces perpetual food, automated defense, warm hearths, and refined metal ingots without requiring player micromanagement.</p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 19</span>
    </div>
  </div>
""")

parts.append("""
</body>
</html>
""")

full_html = "".join(parts)

html_path = r"d:\valheim_mods\ValheimWorkers_Guide_Book.html"
with open(html_path, "w", encoding="utf-8") as f:
    f.write(full_html)
print(f"Generated {html_path} ({len(full_html)} bytes)")

# Also copy to ValheimWorkers project folder
project_html_path = r"d:\valheim_mods\ValheimWorkers\ValheimWorkers_Guide_Book.html"
with open(project_html_path, "w", encoding="utf-8") as f:
    f.write(full_html)
print(f"Copied to {project_html_path}")

# Render to PDF using Edge headless
pdf_path = r"d:\valheim_mods\ValheimWorkers_Guide_Book.pdf"
edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if os.path.exists(edge_path):
    print("Compiling PDF with Edge headless...")
    cmd = [
        edge_path,
        "--headless=new",
        "--disable-gpu",
        "--no-pdf-header-footer",
        f"--print-to-pdf={pdf_path}",
        f"file:///{html_path.replace(os.sep, '/')}"
    ]
    res = subprocess.run(cmd, capture_output=True)
    if os.path.exists(pdf_path):
        print(f"Generated {pdf_path} ({os.path.getsize(pdf_path)} bytes)")
        # Copy PDF to project folder
        project_pdf_path = r"d:\valheim_mods\ValheimWorkers\ValheimWorkers_Guide_Book.pdf"
        with open(pdf_path, "rb") as src, open(project_pdf_path, "wb") as dst:
            dst.write(src.read())
        print(f"Copied to {project_pdf_path}")
    else:
        print("Error: PDF was not generated.")
else:
    print(f"Warning: Edge executable not found at {edge_path}")
