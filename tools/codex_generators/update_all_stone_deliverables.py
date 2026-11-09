import os
import sys
import base64
import shutil
import subprocess

print("=== VALHEIMWORKERS: ETCHED-IN-STONE COMPILATION ENGINE V2 ===")

BASE_DIR = r"d:\valheim_mods"
DOCS_DIR = os.path.join(BASE_DIR, "docs")
ASSETS_DIR = os.path.join(DOCS_DIR, "assets")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
STONE_DIR = os.path.join(ASSETS_DIR, "stone")

HTML_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.html")
PDF_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.pdf")
MAP_OUT = os.path.join(BASE_DIR, "worker_feature_map.html")

SUBDIR = os.path.join(BASE_DIR, "ValheimWorkers")
os.makedirs(SUBDIR, exist_ok=True)
SUBDIR_HTML = os.path.join(SUBDIR, "ValheimWorkers_Guide_Book.html")
SUBDIR_PDF = os.path.join(SUBDIR, "ValheimWorkers_Guide_Book.pdf")
SUBDIR_MAP = os.path.join(SUBDIR, "worker_feature_map.html")

def file_b64(path):
    with open(path, "rb") as f:
        return base64.b64encode(f.read()).decode("utf-8")

print("Encoding stone textures and fonts...")
stone_monolith_b64 = file_b64(os.path.join(STONE_DIR, "stone_monolith_bg.png"))
stone_tablet_b64 = file_b64(os.path.join(STONE_DIR, "stone_tablet_bg.png"))
stone_socket_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_bg.png"))
stone_socket_eq_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_equipped_bg.png"))
stone_crest_b64 = file_b64(os.path.join(STONE_DIR, "stone_cover_crest.png"))

norse_b64 = file_b64(os.path.join(FONTS_DIR, "Norse.otf"))
norsebold_b64 = file_b64(os.path.join(FONTS_DIR, "Norsebold.otf"))
averia_regular_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Regular.ttf"))
averia_bold_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Bold.ttf"))
averia_italic_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Italic.ttf"))

icon_cache = {}
for fname in os.listdir(ICONS_DIR):
    if fname.lower().endswith(".png"):
        icon_cache[fname] = "data:image/png;base64," + file_b64(os.path.join(ICONS_DIR, fname))

def ic(name):
    if name in icon_cache:
        return icon_cache[name]
    for k in icon_cache:
        if name.lower() == k.lower():
            return icon_cache[k]
    return ""

def slot(icon_name, label, equipped=False):
    eq_cls = " equipped" if equipped else ""
    return f"""<div class="valheim-slot{eq_cls}"><img src="{ic(icon_name)}" alt="{label}"><span class="slot-label">{label}</span></div>"""

def feat(f_code, f_name, icon_f):
    return f"""<span class="feature-badge"><img src="{ic(icon_f)}" alt="{f_code}"> {f_code} {f_name}</span>"""

def hugin():
    return f"""<span class="hugin-badge"><img src="{ic('Hugin.png')}" alt="Hugin"> HUGIN'S ADVICE</span>"""

def make_worker_chapter(page_num, part_roman, role_title, role_sub, role_pill, icon_main, slots_left, stations_right, panel_now, panel_routine, panel_future, panel_synergy, panel_rec, panel_warn):
    slots_left_html = "".join([slot(s[0], s[1], s[2]) for s in slots_left])
    stations_right_html = "".join([slot(s[0], s[1], False) for s in stations_right])
    
    return f"""
  <div class="page">
    <div class="content-container">
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

      <div class="worker-grid-6">
        <div class="stone-tablet panel-now">
          {panel_now}
        </div>
        <div class="stone-tablet panel-routine">
          {panel_routine}
        </div>
        <div class="stone-tablet panel-future">
          {panel_future}
        </div>
        <div class="stone-tablet panel-synergy">
          {panel_synergy}
        </div>
        <div class="stone-tablet panel-rec">
          {panel_rec}
        </div>
        <div class="stone-tablet panel-warn">
          {panel_warn}
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0 &bull; Domain Operations</span>
      <span>Page {page_num}</span>
    </div>
  </div>
"""

# Extract worker content from build_stone_guide.py
with open(os.path.join(BASE_DIR, "build_stone_guide.py"), "r", encoding="utf-8") as f:
    orig_code = f.read()

pos_w_start = orig_code.find("# 1. LUMBERJACK (Page 6)")
pos_w_end = orig_code.find("# ==================== PAGE 17: LOGISTICS MASTERY ====================")
worker_chapters_code = orig_code[pos_w_start:pos_w_end]

# Common CSS
COMMON_CSS = f"""
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
      src: url('data:font/ttf;base64,{averia_regular_b64}') format('truetype');
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
      margin: 6mm 8mm 6mm 8mm;
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
        height: 284mm;
        max-height: 284mm;
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
      background-color: #1a1e25;
      color: #EDE5D8;
      line-height: 1.32;
      font-size: 8.0pt;
    }}

    /* The Monolithic Basalt Stone Slab Page */
    .page {{
      height: 284mm;
      max-height: 284mm;
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
      page-break-before: always;
      break-before: page;
      padding: 5.5mm 8mm 4.0mm 8mm;
      overflow: hidden;
      background-color: #1a1e25;
      background-image: url('data:image/png;base64,{stone_monolith_b64}');
      background-repeat: repeat;
      background-size: 380px 380px;
      border: 3.5px solid #3c2f1c;
      outline: 1.5px solid #9a7638;
      outline-offset: -5px;
      border-radius: 6px;
      box-shadow: inset 0 0 35px rgba(0,0,0,0.88), inset 1.5px 1.5px 3px rgba(245, 215, 165, 0.25), 0 8px 30px rgba(0,0,0,0.85);
      position: relative;
    }}
    .page-first {{
      page-break-before: avoid;
      break-before: avoid;
    }}

    /* Carved Elder Futhark Rune Ribbon */
    .page::before {{
      content: "ᚠ ᚢ ᚦ ᚨ ᚱ ᚲ ᚷ ᚹ ᚺ ᚾ ᛁ ᛃ ᛈ ᛇ ᛉ ᛋ ᛏ ᛒ ᛖ ᛗ ᛚ ᛜ ᛞ ᛟ";
      display: block;
      text-align: center;
      font-family: 'Norsebold', serif;
      font-size: 7.2pt;
      color: #9a7638;
      letter-spacing: 0.38em;
      margin-bottom: 3.5px;
      text-shadow: 0 1px 2px #000, 0 0 5px rgba(212,160,23,0.3);
    }}

    .content-container {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
      gap: 5.0px;
      min-height: 0;
    }}

    .chapter-header {{
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 2px solid #5a4220;
      padding-bottom: 3.0px;
    }}
    .chapter-header h2 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 14.5pt;
      color: #f7d57f;
      letter-spacing: 0.05em;
      text-shadow: -1px -1px 0 #000, 1px 1px 0 rgba(255,240,180,0.4), 0 2px 5px rgba(0,0,0,0.9);
      display: flex;
      align-items: center;
      gap: 7px;
    }}
    .chapter-icon {{
      width: 22px;
      height: 22px;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.9));
    }}
    .role-pill {{
      background: linear-gradient(135deg, #7A5116 0%, #52350C 100%);
      color: #FFF4D9;
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 7.6pt;
      padding: 2px 8px;
      border-radius: 4px;
      border: 1px solid #DDA13B;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      box-shadow: inset 1px 1px 2px rgba(255,255,255,0.25), 0 2px 5px rgba(0,0,0,0.7);
    }}

    .page-footer {{
      border-top: 1px solid #4D371E;
      padding-top: 2.5px;
      display: flex;
      justify-content: space-between;
      font-size: 7.0pt;
      color: #A3937F;
      font-family: 'Averia Serif Libre', serif;
      margin-top: auto;
    }}

    /* Raised Stone Plinth Showcase Bar */
    .showcase-bar {{
      background-color: #181c22;
      background-image: url('data:image/png;base64,{stone_tablet_b64}');
      background-repeat: repeat;
      border-top: 1.5px solid rgba(240, 215, 160, 0.32);
      border-left: 1.5px solid rgba(240, 215, 160, 0.22);
      border-bottom: 2px solid #06080a;
      border-right: 2px solid #06080a;
      border-radius: 5px;
      padding: 4px 9px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 7px;
      box-shadow: inset 1.5px 1.5px 4px rgba(0,0,0,0.85), 0 3px 8px rgba(0,0,0,0.7);
    }}
    .showcase-group {{
      display: flex;
      align-items: center;
      gap: 6.5px;
    }}
    .showcase-title {{
      font-family: 'Norsebold', serif;
      font-size: 7.6pt;
      color: #f0be52;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin-right: 3px;
      text-shadow: -1px -1px 0px #000;
    }}
    
    /* Sunken Relic Sockets */
    .valheim-slot {{
      width: 31px;
      height: 31px;
      background: radial-gradient(circle at 65% 65%, #0e1115 0%, #161a20 100%), url('data:image/png;base64,{stone_socket_b64}') no-repeat center/cover;
      border-top: 1.5px solid #0a0d10;
      border-left: 1.5px solid #0a0d10;
      border-bottom: 1.5px solid rgba(240, 215, 160, 0.35);
      border-right: 1.5px solid rgba(240, 215, 160, 0.25);
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      box-shadow: inset 2px 2px 6px rgba(0,0,0,0.95), 0 2px 5px rgba(0,0,0,0.7);
    }}
    .valheim-slot.equipped {{
      border: 1.5px solid #38BDF8 !important;
      box-shadow: inset 0 0 8px rgba(56, 189, 248, 0.65), 0 0 8px rgba(56, 189, 248, 0.5), inset 2px 2px 5px #000 !important;
    }}
    .valheim-slot img {{
      width: 24px;
      height: 24px;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.85));
    }}
    .valheim-slot .slot-label {{
      position: absolute;
      bottom: -11.0px;
      left: 50%;
      transform: translateX(-50%);
      font-size: 5.7pt;
      color: #EDE5D8;
      white-space: nowrap;
      font-family: 'Averia Serif Libre', serif;
      text-shadow: 0 1px 2px #000, 0 0 3px #000;
    }}
    .valheim-slot.equipped::after {{
      content: "";
      position: absolute;
      top: -2px;
      right: -2px;
      width: 5px;
      height: 5px;
      background: #38BDF8;
      border-radius: 50%;
      box-shadow: 0 0 5px #38BDF8;
    }}

    /* Worker Grid 6 Tablets - Proportional Full Height Spanning */
    .worker-grid-6 {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      gap: 4.5px;
      min-height: 0;
    }}

    /* Recessed Stone Tablets */
    .stone-tablet {{
      flex: 1;
      min-height: 0;
      display: flex;
      flex-direction: column;
      justify-content: flex-start;
      background-color: #16191f;
      background-image: url('data:image/png;base64,{stone_tablet_b64}');
      background-repeat: repeat;
      border-top: 1.5px solid rgba(240, 215, 160, 0.32);
      border-left: 1.5px solid rgba(240, 215, 160, 0.22);
      border-bottom: 2.5px solid #06080a;
      border-right: 2px solid #06080a;
      border-radius: 4px;
      padding: 4.5px 9.5px;
      box-shadow: inset 1.5px 1.5px 4px rgba(0,0,0,0.85), inset -1px -1px 2px rgba(255,255,255,0.04), 0 3px 8px rgba(0,0,0,0.7);
    }}
    .stone-tablet h3 {{
      font-family: 'Norsebold', serif;
      font-size: 9.0pt;
      margin-bottom: 2.2px;
      display: flex;
      align-items: center;
      gap: 5px;
      letter-spacing: 0.04em;
      text-transform: uppercase;
    }}
    .stone-tablet p {{
      font-size: 7.9pt;
      line-height: 1.30;
      color: #ded5c5;
      margin-bottom: 2px;
      text-shadow: -1px -1px 0px rgba(0,0,0,0.9), 0.5px 0.5px 0px rgba(255, 230, 180, 0.15);
    }}
    .stone-tablet ul {{
      margin-left: 15px;
      font-size: 7.8pt;
      color: #e5ded2;
      line-height: 1.28;
    }}
    .stone-tablet li {{
      margin-bottom: 1.8px;
      text-shadow: -1px -1px 0px rgba(0,0,0,0.9), 0.5px 0.5px 0px rgba(255, 230, 180, 0.12);
    }}
    
    /* Etched Mineral Inlay Grooves */
    .panel-now h3 {{
      color: #8ADE4C;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(138, 222, 76, 0.35);
      border-bottom: 1px solid rgba(138, 222, 76, 0.3);
      padding-bottom: 2px;
    }}
    .panel-routine h3 {{
      color: #38BDF8;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(56, 189, 248, 0.35);
      border-bottom: 1px solid rgba(56, 189, 248, 0.3);
      padding-bottom: 2px;
    }}
    .panel-future h3 {{
      color: #C084FC;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(192, 132, 252, 0.35);
      border-bottom: 1px solid rgba(192, 132, 252, 0.3);
      padding-bottom: 2px;
    }}
    .panel-synergy h3 {{
      color: #22D3EE;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(34, 211, 238, 0.35);
      border-bottom: 1px solid rgba(34, 211, 238, 0.3);
      padding-bottom: 2px;
    }}
    .panel-rec h3 {{
      color: #FBBF24;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(251, 191, 36, 0.35);
      border-bottom: 1px solid rgba(251, 191, 36, 0.3);
      padding-bottom: 2px;
    }}
    .panel-warn h3 {{
      color: #F87171;
      text-shadow: -1px -1.5px 1px #000, 1px 1px 0px rgba(248, 113, 113, 0.35);
      border-bottom: 1px solid rgba(248, 113, 113, 0.3);
      padding-bottom: 2px;
    }}

    .hugin-badge {{
      display: inline-flex;
      align-items: center;
      gap: 3px;
      background: rgba(217, 119, 6, 0.28);
      border: 1px solid #D97706;
      color: #FBBF24;
      padding: 1.5px 5px;
      border-radius: 3px;
      font-size: 6.8pt;
      font-family: 'Norsebold', serif;
      margin-right: 4px;
      text-shadow: -1px -1px 0px #000;
    }}
    .hugin-badge img {{
      width: 11px;
      height: 11px;
      object-fit: contain;
    }}

    .feature-badge {{
      display: inline-flex;
      align-items: center;
      gap: 3.5px;
      background: rgba(245, 158, 11, 0.18);
      color: #FDE68A;
      border: 1px solid #B45309;
      border-radius: 3px;
      padding: 1px 5px;
      font-size: 7.2pt;
      font-family: 'Norsebold', serif;
      margin-right: 4px;
      text-shadow: -1px -1px 0px #000;
    }}
    .feature-badge img {{
      width: 12px;
      height: 12px;
      object-fit: contain;
    }}

    /* Page 1 Cover */
    .cover-body {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      align-items: center;
      text-align: center;
      padding: 7mm 4mm 3mm 4mm;
    }}
    .cover-badge {{
      display: inline-block;
      font-family: 'Norsebold', serif;
      font-size: 8.5pt;
      color: #f7d57f;
      letter-spacing: 0.14em;
      border: 1.5px solid #9a7638;
      padding: 3px 14px;
      border-radius: 4px;
      background: rgba(15, 18, 23, 0.75);
      box-shadow: inset 0 0 8px #000, 0 2px 6px rgba(0,0,0,0.8);
      margin-bottom: 3.5mm;
      text-transform: uppercase;
    }}
    .cover-title {{
      font-family: 'Norsebold', serif;
      font-size: 29pt;
      line-height: 1.05;
      color: #f7d57f;
      letter-spacing: 0.08em;
      text-shadow: -2px -2px 0 #000, 2px 2px 0 rgba(255,240,180,0.35), 0 4px 14px rgba(212,160,23,0.3);
      margin-bottom: 2.5mm;
    }}
    .cover-subtitle {{
      font-family: 'Averia Serif Libre', serif;
      font-style: italic;
      font-size: 10.5pt;
      color: #ded5c5;
      letter-spacing: 0.04em;
      text-shadow: 0 2px 4px #000;
      max-width: 90%;
      margin: 0 auto;
    }}
    .cover-medallion-box {{
      margin: 3mm 0;
      position: relative;
      display: flex;
      justify-content: center;
      align-items: center;
    }}
    .cover-medallion-img {{
      width: 215px;
      height: 215px;
      object-fit: contain;
      filter: drop-shadow(0 8px 24px rgba(0,0,0,0.9));
    }}
    .cover-pillars-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 6px;
      width: 100%;
      margin-top: 2mm;
    }}
    .cover-pillar-card {{
      background-color: #16191f;
      background-image: url('data:image/png;base64,{stone_tablet_b64}');
      background-repeat: repeat;
      border-top: 1.5px solid rgba(240, 215, 160, 0.32);
      border-left: 1.5px solid rgba(240, 215, 160, 0.22);
      border-bottom: 2.5px solid #06080a;
      border-right: 2px solid #06080a;
      border-radius: 4px;
      padding: 6.5px 10px;
      display: flex;
      align-items: center;
      gap: 10px;
      text-align: left;
      box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8), 0 3px 6px rgba(0,0,0,0.7);
    }}
    .cover-pillar-icon {{
      width: 32px;
      height: 32px;
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: radial-gradient(circle at 65% 65%, #0e1115 0%, #161a20 100%), url('data:image/png;base64,{stone_socket_b64}') no-repeat center/cover;
      border-top: 1px solid #0a0d10;
      border-left: 1px solid #0a0d10;
      border-bottom: 1px solid rgba(240, 215, 160, 0.3);
      border-right: 1px solid rgba(240, 215, 160, 0.2);
      border-radius: 4px;
      box-shadow: inset 1.5px 1.5px 4px #000;
    }}
    .cover-pillar-icon img {{
      width: 24px;
      height: 24px;
      object-fit: contain;
    }}
    .cover-pillar-text strong {{
      font-family: 'Norsebold', serif;
      font-size: 8.5pt;
      color: #f7d57f;
      display: block;
      letter-spacing: 0.04em;
      margin-bottom: 1px;
    }}
    .cover-pillar-text p {{
      font-size: 7.4pt;
      color: #ded5c5;
      line-height: 1.30;
    }}
    .cover-epigraph {{
      background-color: #16191f;
      background-image: url('data:image/png;base64,{stone_tablet_b64}');
      background-repeat: repeat;
      border-top: 1.5px solid rgba(240, 215, 160, 0.25);
      border-left: 1.5px solid rgba(240, 215, 160, 0.18);
      border-bottom: 2px solid #06080a;
      border-right: 2px solid #06080a;
      border-radius: 4px;
      padding: 6.5px 14px;
      font-style: italic;
      font-size: 8.0pt;
      color: #ded5c5;
      line-height: 1.38;
      width: 100%;
      box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8), 0 3px 6px rgba(0,0,0,0.7);
    }}
    .cover-footer {{
      font-family: 'Averia Serif Libre', serif;
      font-size: 7.0pt;
      color: #9a7638;
      letter-spacing: 0.06em;
      margin-top: 1.5mm;
    }}

    /* Page 4: 36-Feature Catalog Grid */
    .feat-catalog-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      grid-template-rows: repeat(18, 1fr);
      gap: 3.5px 7px;
      flex: 1;
      min-height: 0;
    }}
    .feat-card-comp {{
      background-color: #16191f;
      background-image: url('data:image/png;base64,{stone_tablet_b64}');
      background-repeat: repeat;
      border-top: 1.5px solid rgba(240, 215, 160, 0.28);
      border-left: 1.5px solid rgba(240, 215, 160, 0.18);
      border-bottom: 2px solid #06080a;
      border-right: 2px solid #06080a;
      border-radius: 4px;
      padding: 2.5px 6.5px;
      display: flex;
      align-items: center;
      gap: 6.5px;
      box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8), 0 2px 4px rgba(0,0,0,0.6);
    }}
    .feat-img-box {{
      width: 27px;
      height: 27px;
      flex-shrink: 0;
      background: radial-gradient(circle at 65% 65%, #0e1115 0%, #161a20 100%), url('data:image/png;base64,{stone_socket_b64}') no-repeat center/cover;
      border-top: 1px solid #0a0d10;
      border-left: 1px solid #0a0d10;
      border-bottom: 1px solid rgba(240, 215, 160, 0.3);
      border-right: 1px solid rgba(240, 215, 160, 0.2);
      border-radius: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: inset 1.5px 1.5px 4px rgba(0,0,0,0.9);
    }}
    .feat-img-box img {{
      width: 21px;
      height: 21px;
      object-fit: contain;
      filter: drop-shadow(0 1px 3px rgba(0,0,0,0.9));
    }}
    .feat-card-body {{
      flex: 1;
      min-width: 0;
    }}
    .feat-card-head {{
      display: flex;
      justify-content: space-between;
      align-items: center;
    }}
    .feat-card-head strong {{
      font-family: 'Norsebold', serif;
      font-size: 7.8pt;
      color: #f7d57f;
      letter-spacing: 0.03em;
      text-shadow: -1px -1px 0 #000;
    }}
    .feat-card-domain {{
      font-size: 6.0pt;
      color: #38bdf8;
      font-family: 'Norsebold', serif;
      text-transform: uppercase;
      background: rgba(56, 189, 248, 0.12);
      padding: 1px 4px;
      border-radius: 2px;
      border: 0.5px solid rgba(56, 189, 248, 0.3);
    }}
    .feat-card-desc {{
      font-size: 6.8pt;
      color: #ded5c5;
      line-height: 1.25;
      margin-top: 1px;
      text-shadow: -0.5px -0.5px 0 #000;
    }}

    /* Page 5: Matrix View & Tables */
    .matrix-view {{
      width: 100%;
      border-collapse: collapse;
      font-size: 7.4pt;
      margin-top: 3px;
      background: rgba(14, 17, 22, 0.85);
      border: 1.5px solid #5a4220;
    }}
    .matrix-view th, .matrix-view td {{
      border: 1px solid #3c2f1c;
      padding: 3.0px 1px;
      text-align: center;
    }}
    .matrix-view th.worker-hdr {{
      text-align: left;
      padding-left: 6px;
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      background: #1e242d;
      font-size: 7.8pt;
    }}
    .matrix-view th.feat-hdr {{
      font-family: 'Norsebold', serif;
      font-size: 5.6pt;
      color: #ded5c5;
      background: #181d24;
    }}
    .matrix-view td.w-name {{
      text-align: left;
      padding-left: 6px;
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      background: #181d24;
      font-size: 7.6pt;
    }}
    .matrix-view td.m-yes {{
      color: #4ade80;
      font-weight: bold;
      font-size: 8.5pt;
      text-shadow: 0 0 4px rgba(74, 222, 128, 0.6);
    }}
    .matrix-view td.m-no {{
      color: #3f4752;
      font-size: 7.5pt;
    }}

    table.stone-table {{
      width: 100%;
      border-collapse: collapse;
      font-size: 7.5pt;
      margin-top: 3px;
    }}
    table.stone-table th, table.stone-table td {{
      border: 1px solid #3c2f1c;
      padding: 3.5px 6px;
      text-align: left;
    }}
    table.stone-table th {{
      background: #1e242d;
      font-family: 'Norsebold', serif;
      color: #f7d57f;
      font-size: 7.8pt;
      letter-spacing: 0.03em;
    }}
    table.stone-table tr:nth-child(even) {{
      background: rgba(22, 26, 33, 0.6);
    }}
"""

parts = []
parts.append(f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>ValheimWorkers v0.3.0 - The Settlement Steward's Codex</title>
  <style>
{COMMON_CSS}
  </style>
</head>
<body>
""")

# ==================== PAGE 1: COVER ====================
print("Building Page 1: Cover...")
parts.append(f"""
  <div class="page page-first">
    <div class="cover-body">
      <div class="cover-header">
        <span class="cover-badge">VALHEIMWORKERS v0.3.0 &bull; OFFICIAL COMPENDIUM</span>
        <h1 class="cover-title">THE SETTLEMENT<br>STEWARD'S CODEX</h1>
        <p class="cover-subtitle">Autonomous AI Behaviors, Spatial Memory, 36-Feature Map &amp; Domain Operations</p>
      </div>

      <div class="cover-medallion-box">
        <img src="data:image/png;base64,{stone_crest_b64}" class="cover-medallion-img" alt="ValheimWorkers Seal">
      </div>

      <div class="cover-pillars-grid">
        <div class="cover-pillar-card">
          <div class="cover-pillar-icon"><img src="{ic('Stone_Axe.png')}"></div>
          <div class="cover-pillar-text">
            <strong>AUTONOMOUS LABOR ENGINE</strong>
            <p>11 specialized roles with spatial memory, auto-repair, smart sorting &amp; unstuck hops.</p>
          </div>
        </div>
        <div class="cover-pillar-card">
          <div class="cover-pillar-icon"><img src="{ic('Sign.png')}"></div>
          <div class="cover-pillar-text">
            <strong>36 SPECIALIZED FEATURE ICONS</strong>
            <p>Master F01&ndash;F36 taxonomy categorizing harvesting, defense, refining, and social life.</p>
          </div>
        </div>
        <div class="cover-pillar-card">
          <div class="cover-pillar-icon"><img src="{ic('Cart.png')}"></div>
          <div class="cover-pillar-text">
            <strong>FULL LOGISTICS INFRASTRUCTURE</strong>
            <p>Overflow resource piles (wood, stone, coal), sign [Tag] routing &amp; Vagon cart loading.</p>
          </div>
        </div>
        <div class="cover-pillar-card">
          <div class="cover-pillar-icon"><img src="{ic('Iron_Sword.png')}"></div>
          <div class="cover-pillar-text">
            <strong>ADAPTIVE TERRAIN &amp; COMBAT</strong>
            <p>Hoe quarry leveling, ore exposure &amp; parabolic archery with tactical CQC swap.</p>
          </div>
        </div>
      </div>

      <div class="cover-epigraph">
        &ldquo;To carve a thriving hall out of the primordial wilderness of Valheim demands more than a sharp axe and strong back&mdash;it demands stewardship. May this codex guide your loyal kin as they till the soil, hollow the veins of the earth, stoke the fires of the forge, and stand resolute against the terrors of the night.&rdquo;
      </div>

      <div class="cover-footer">
        Valheim BepInEx 5.4.22+ &bull; Harmony 2.0 &bull; Authored for High-Immersion Nordic Strongholds
      </div>
    </div>
  </div>
""")

# ==================== PAGE 2: TABLE OF CONTENTS ====================
print("Building Page 2: Table of Contents & Executive Blueprint...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic('Hugin.png')}" class="chapter-icon" alt="Hugin"> TABLE OF CONTENTS &amp; EXECUTIVE BLUEPRINT</h2>
        <span class="role-pill">CODEX INDEX</span>
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 5.5px; flex: 1.2;">
        <div class="stone-tablet panel-now">
          <h3><img src="{ic('Stone_Axe.png')}" style="width: 14px; height: 14px;"> PART I: FOUNDATIONS &amp; SYSTEM ARCHITECTURE</h3>
          <ul style="list-style: none; margin-left: 0; font-size: 7.8pt;">
            <li style="display: flex; justify-content: space-between;"><span>&bull; Core Controls &amp; In-World Hotkeys</span><strong style="color: #f7d57f;">PAGE 3</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; Dual-Anchor Spatial Post Architecture</span><strong style="color: #f7d57f;">PAGE 3</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; 8&times;4 Container Storage &amp; Sorting</span><strong style="color: #f7d57f;">PAGE 3</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; 3-Slot Vitality &amp; Sustenance Rations</span><strong style="color: #f7d57f;">PAGE 3</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; Recruitable Heritages (Viking, Dvergr, Skeleton)</span><strong style="color: #f7d57f;">PAGE 3</strong></li>
          </ul>
        </div>

        <div class="stone-tablet panel-routine">
          <h3><img src="{ic('Sign.png')}" style="width: 14px; height: 14px;"> PART II: 36-FEATURE MAP &amp; SEPARATION MATRIX</h3>
          <ul style="list-style: none; margin-left: 0; font-size: 7.8pt;">
            <li style="display: flex; justify-content: space-between;"><span>&bull; The 36-Feature Icon Catalog (F01&ndash;F36)</span><strong style="color: #38bdf8;">PAGE 4</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; Master Worker &times; Feature Capability Matrix</span><strong style="color: #38bdf8;">PAGE 5</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; Domain Classification &amp; Operational Hierarchy</span><strong style="color: #38bdf8;">PAGE 5</strong></li>
          </ul>
        </div>

        <div class="stone-tablet panel-future">
          <h3><img src="{ic('Antler_Pickaxe.png')}" style="width: 14px; height: 14px;"> PART III: HARVESTING &amp; EXTRACTION ROLES</h3>
          <ul style="list-style: none; margin-left: 0; font-size: 7.8pt;">
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Lumberjack (Forester &amp; Arborist)</span><strong style="color: #c084fc;">PAGE 6</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Miner (Quarryman &amp; Excavator)</span><strong style="color: #c084fc;">PAGE 7</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Farmer (Husbandman &amp; Cultivator)</span><strong style="color: #c084fc;">PAGE 8</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Gatherer (Forager &amp; Apiarist)</span><strong style="color: #c084fc;">PAGE 9</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Hunter (Ranger &amp; Marksman)</span><strong style="color: #c084fc;">PAGE 10</strong></li>
          </ul>
        </div>

        <div class="stone-tablet panel-synergy">
          <h3><img src="{ic('Iron_Sword.png')}" style="width: 14px; height: 14px;"> PART IV: BASE PRODUCTION &amp; DEFENSE ROLES</h3>
          <ul style="list-style: none; margin-left: 0; font-size: 7.8pt;">
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Guard (Perimeter Sentinel &amp; Warden)</span><strong style="color: #22d3ee;">PAGE 11</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Smelter (Furnace Master &amp; Pyrotechnic)</span><strong style="color: #22d3ee;">PAGE 12</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Base Tender (Camp Steward &amp; Miller)</span><strong style="color: #22d3ee;">PAGE 13</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Cook (Culinary Artisan &amp; Brewer)</span><strong style="color: #22d3ee;">PAGE 14</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Hauler (Logistics Operator &amp; Packmaster)</span><strong style="color: #22d3ee;">PAGE 15</strong></li>
            <li style="display: flex; justify-content: space-between;"><span>&bull; The Craftsman (Artificer &amp; Carpenter)</span><strong style="color: #22d3ee;">PAGE 16</strong></li>
          </ul>
        </div>
      </div>

      <div class="stone-tablet panel-rec" style="flex: 1.1;">
        <h3>{hugin()} THE FIVE PILLARS OF AUTONOMOUS SETTLEMENT GOVERNANCE</h3>
        <ul>
          <li><strong>1. Secure the Perimeter:</strong> Station Guards at palisade choke points equipped with both Hunter-grade Bows and Bronze/Iron shields for tactical CQC response.</li>
          <li><strong>2. Protect Worker Vitality:</strong> Ensure 3-slot nutrition by stocking camp larders with Honey, Cooked Meats, and Jam; well-fed workers work 25% faster.</li>
          <li><strong>3. Explicit Storage Bracket Tagging:</strong> Fasten signs above strongboxes tagged with <code>[Wood]</code>, <code>[Ore]</code>, <code>[Food]</code>, <code>[Seeds]</code>, and <code>[Mead]</code> for zero-clutter routing.</li>
          <li><strong>4. Tool Proximity &amp; Integrity:</strong> Maintain an upgraded Workbench and Forge within 15m of worksites to prevent idle tool-durability walking pauses.</li>
          <li><strong>5. Plan for Material Abundance:</strong> Clear open warehouse zones for automated 50x resource piles (wood, stone, coal) when strongboxes reach full capacity.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-warn" style="flex: 0.7;">
        <h3>PART V: SETTLEMENT LOGISTICS, DIAGNOSTICS &amp; SPECIFICATIONS</h3>
        <p><strong>Page 17: Logistics Mastery:</strong> Physical Overflow Piles [F05], Bracketed Sign Tags [F32], Vagon Carts &amp; Maritime Voyages [F34].</p>
        <p><strong>Page 18: Diagnostics &amp; Telemetry:</strong> Unstuck hops, NavMesh bounds, tool tier fallbacks, multiplayer sync mutex, and F8 Telemetry.</p>
        <p><strong>Page 19: Master Settlement Roster:</strong> Full worker attribute specifications table, 50-person stronghold blueprint, and steward verification.</p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0 &bull; Blueprint</span>
      <span>Page 2</span>
    </div>
  </div>
""")

# ==================== PAGE 3: FOUNDATIONS ====================
print("Building Page 3: Foundations...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic('Stone_Axe.png')}" class="chapter-icon" alt="Foundations"> PART I: CORE CONTROLS &amp; FOUNDATIONS</h2>
        <span class="role-pill">SPATIAL LOGIC &bull; VITALITY</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Native 8&times;4 Grid (Equipped Cyan Borders):</span>
          {slot('Stone_Axe.png', 'Axe', True)}
          {slot('Banded_Shield.png', 'Shield', True)}
          {slot('Rag_Tunic.png', 'Armor', True)}
          {slot('Wood_Arrow.png', 'Arrows', True)}
          {slot('Wood.png', 'Cargo', False)}
          {slot('Stone.png', 'Cargo', False)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">3-Slot Stomach:</span>
          {slot('Deer_Stew.png', 'Stew', False)}
          {slot('Cooked_Meat.png', 'Meat', False)}
          {slot('Queens_Jam.png', 'Jam', False)}
        </div>
      </div>

      <div class="stone-tablet panel-now">
        <h3>1. DIRECT CONTROLS &amp; IN-WORLD DIRECTIVES</h3>
        <ul>
          <li><strong>F8 (Settlement Hall UI):</strong> Opens management dashboard. Unlocks cursor, pauses player attacks/camera, and provides access to Recruitment, Roster Management, and Telemetry.</li>
          <li><strong>[E] (Tap on Worker):</strong> Instant toggle between Follow Me and Station at Post. When stationed, anchors location and resumes last assigned role.</li>
          <li><strong>[Shift + E] (Tap on Worker):</strong> Quick-cycles through all allowed roles in the world, accompanied by a speech bubble announcement.</li>
          <li><strong>[Hold E] (&gt;0.38s):</strong> Opens the worker's native 8&times;4 (32-slot) Container Inventory. Right-clicking equipment inside equips/unequips it immediately with a cyan border.</li>
          <li><strong>[Alt + Hold E] (&gt;0.38s):</strong> Opens F8 Settlement Hall focused directly onto the targeted worker.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-routine">
        <h3>2. DUAL-ANCHOR SPATIAL POST ARCHITECTURE</h3>
        <p>Workers maintain two distinct spatial coordinates saved permanently in their ZDO (Zone Data Object):</p>
        <ul>
          <li><strong>Camp Post (HomePosition):</strong> The domestic home base where workers deposit harvested goods into chests, withdraw fuels/raw meats, eat meals, and retreat during raids.</li>
          <li><strong>Work Post (WorkPostPosition):</strong> The remote job center where tasks like tree felling, mining, and patrols occur. Workers commute to their Work Post to perform labor, then hike back to their Camp Post when full!</li>
          <li><strong>Unstuck Hop Mechanics:</strong> If a worker remains trapped on geometry for &gt;12s, the navigation engine executes a 1.5m physics hop toward the active target.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-future">
        <h3>3. NATIVE 8&times;4 CONTAINER STORAGE &amp; AUTO-REPAIR ROUTINE</h3>
        <ul>
          <li><strong>32-Slot Container Backing:</strong> Each worker uses a real Valheim Inventory container identical to a wooden chest, allowing full compatibility with inventory sorting mods.</li>
          <li><strong>Protected Gear Retention [F21]:</strong> Equipped weapons, armor, and primary tools are flagged with an immutable retention tag so workers never deposit their tools into storage chests!</li>
          <li><strong>Proactive Field Repair [F20]:</strong> When active tool durability drops below 15%, the worker pauses harvesting, routes to the nearest upgraded Workbench/Forge, and repairs all items before resuming.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-synergy">
        <h3>4. 3-SLOT SUSTENANCE &amp; METABOLIC VITALITY</h3>
        <ul>
          <li><strong>3-Slot Nutrition Simulation:</strong> Workers consume up to 3 distinct food items to gain maximum health, stamina, and passive health regeneration. They eat from carried rations or withdraw food from camp pantry chests.</li>
          <li><strong>Hunger Degradation:</strong> Starving workers suffer a 40% movement penalty and cannot sprint or perform heavy felling/mining strikes.</li>
          <li><strong>Sustenance Synergy:</strong> Pair Cooks with Hunters/Farmers to automatically stock [Food] chests with Deer Stew, Sausages, and Honey!</li>
        </ul>
      </div>

      <div class="stone-tablet panel-rec">
        <h3>5. RECRUITABLE HERITAGES &amp; RACIAL TRAITS</h3>
        <ul>
          <li><strong>Viking Worker:</strong> 50 Coins cost, 150 Base HP, full custom male/female cosmetics, all 11 roles available. Well-rounded and resilient.</li>
          <li><strong>Dvergr Laborer:</strong> 75 Coins cost, 200 Base HP, spawns with Tier 2 Bronze gear, immune to minor stagger. Exceptional Miners and Smelters.</li>
          <li><strong>Skeleton Servant:</strong> 20 Bone Fragments, 80 HP, low-cost utility for Hauler, Guard, and basic chores. Immune to poison and cold weather.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0 &bull; Architecture</span>
      <span>Page 3</span>
    </div>
  </div>
""")

# ==================== PAGE 4: 36-FEATURE CATALOG ====================
print("Building Page 4: 36-Feature Catalog...")
CATALOG_ITEMS = [
  ("F01", "Forestry & Felling", "Stone_Axe.png", "Forestry", "Fells mature trees matching equipped axe tier."),
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
  ("F22", "Raid War Horn Alert", "Valkyrie.png", "Defense", "Sounds War Horn alert against high-threat hostiles."),
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

feat_cards_html = []
for code, name, icon_f, dom, desc in CATALOG_ITEMS:
    feat_cards_html.append(f"""
      <div class="feat-card-comp">
        <div class="feat-img-box"><img src="{ic(icon_f)}" alt="{code}"></div>
        <div class="feat-card-body">
          <div class="feat-card-head">
            <strong>{code} {name}</strong>
            <span class="feat-card-domain">{dom}</span>
          </div>
          <div class="feat-card-desc">{desc}</div>
        </div>
      </div>
    """)
grid_html = "".join(feat_cards_html)

parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Sign.png")}" class="chapter-icon" alt="Catalog"> Part II: The 36-Feature Visual Catalog (F01&ndash;F36)</h2>
        <span class="role-pill">Capability Taxonomy</span>
      </div>
      <p style="font-size: 7.6pt; color: #ded5c5; margin-bottom: 2px;">
        Every worker behavior in ValheimWorkers v0.3.0 is indexed into a standardized 36-feature taxonomy across 8 operational domains:
      </p>

      <div class="feat-catalog-grid">
        {grid_html}
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0 &bull; 36-Feature Map</span>
      <span>Page 4</span>
    </div>
  </div>
""")

# ==================== PAGE 5: SEPARATION MATRIX ====================
print("Building Page 5: Master Separation Matrix & Domain Hierarchy...")
F_CODES = [f"F{i:02d}" for i in range(1, 37)]

WORKER_MATRIX = [
    ("LUMBERJACK", [1, 2, 3, 4, 5, 21, 31, 32, 33, 34, 35, 36]),
    ("MINER",      [5, 6, 7, 8, 9, 21, 31, 32, 33, 34, 35, 36]),
    ("FARMER",     [5, 10, 11, 12, 13, 16, 21, 31, 32, 33, 34, 35, 36]),
    ("GATHERER",   [5, 14, 15, 21, 31, 32, 33, 34, 35, 36]),
    ("HUNTER",     [5, 17, 18, 20, 21, 31, 32, 33, 34, 35, 36]),
    ("GUARD",      [18, 19, 20, 21, 22, 31, 32, 33, 34, 35, 36]),
    ("SMELTER",    [5, 21, 27, 28, 31, 32, 33, 34, 35, 36]),
    ("BASE TENDER",[5, 21, 29, 30, 31, 32, 33, 34, 35, 36]),
    ("COOK",       [5, 21, 23, 24, 25, 26, 31, 32, 33, 34, 35, 36]),
    ("HAULER",     [5, 21, 31, 32, 33, 34, 35, 36]),
    ("CRAFTSMAN",  [5, 20, 21, 31, 32, 33, 34, 35, 36])
]

matrix_rows = []
for name, f_list in WORKER_MATRIX:
    cells = [f'<td class="w-name">{name}</td>']
    for idx in range(1, 37):
        if idx in f_list:
            cells.append('<td class="m-yes">&#10003;</td>')
        else:
            cells.append('<td class="m-no">&middot;</td>')
    matrix_rows.append(f"<tr>{''.join(cells)}</tr>")
matrix_tbody = "\n".join(matrix_rows)
matrix_th = "".join([f'<th class="feat-hdr">{fc}</th>' for fc in F_CODES])

parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Banded_Shield.png")}" class="chapter-icon" alt="Matrix"> Part II: Worker &times; Feature Separation Matrix</h2>
        <span class="role-pill">Master Separation Grid</span>
      </div>
      <p style="font-size: 7.6pt; color: #ded5c5; margin-bottom: 2px;">
        Comprehensive matrix verifying strict functional separation across all 11 worker roles and 36 capabilities:
      </p>

      <table class="matrix-view">
        <thead>
          <tr>
            <th class="worker-hdr">WORKER ROLE</th>
            {matrix_th}
          </tr>
        </thead>
        <tbody>
          {matrix_tbody}
        </tbody>
      </table>

      <div class="stone-tablet panel-now" style="margin-top: 4px; flex: 0.6;">
        <h3>{hugin()} UNIVERSAL CORE CAPABILITIES</h3>
        <p style="font-size: 7.8pt; line-height: 1.34;">
          <strong>Universal Features:</strong> All workers possess <strong>[F21]</strong> Protected Gear Retention, <strong>[F31]</strong> Smart Storage Sorting, <strong>[F32]</strong> Sign-Based Tag Routing, <strong>[F34]</strong> Longship Boarding, <strong>[F35]</strong> Night Torch Bearing, and <strong>[F36]</strong> Tavern Rest &amp; Seating.
        </p>
      </div>

      <div class="stone-tablet panel-routine" style="margin-top: 4px; flex: 1.4;">
        <h3>Operational Domain &amp; Hierarchy Architecture</h3>
        <table class="stone-table">
          <thead>
            <tr>
              <th style="width: 110px;">Domain</th>
              <th style="width: 100px;">Primary Roles</th>
              <th>Assigned Tools</th>
              <th>Core Yields &amp; Workstations</th>
              <th style="width: 80px;">Storage Tag</th>
            </tr>
          </thead>
          <tbody>
            <tr><td>1. Forestry &amp; Felling</td><td>Lumberjack</td><td>Stone, Bronze, Iron Axes</td><td>Wood, Core Wood, Fine Wood, Stumps</td><td><code>[Wood]</code></td></tr>
            <tr><td>2. Mining &amp; Quarrying</td><td>Miner</td><td>Pickaxes (Antler, Bronze, Iron), Hoe</td><td>Stone, Flint, Copper, Tin, Iron, Silver</td><td><code>[Stone] [Ore]</code></td></tr>
            <tr><td>3. Agriculture &amp; Fauna</td><td>Farmer</td><td>Bronze Cultivator</td><td>Carrots, Turnips, Onions, Barley, Tamed Boars</td><td><code>[Seeds] [Food]</code></td></tr>
            <tr><td>4. Wild Foraging &amp; Apiary</td><td>Gatherer</td><td>Bare Hands, Dagger</td><td>Berries, Mushrooms, Thistle, Honey, Sap Extractors</td><td><code>[Berries] [Sap]</code></td></tr>
            <tr><td>5. Wilderness Combat</td><td>Hunter, Guard</td><td>Bows (Finewood, Huntsman), Sword, Shield</td><td>Raw Meat, Deer Pelts, Feathers, Trophy Drops</td><td><code>[Meat] [Pelt]</code></td></tr>
            <tr><td>6. Smelting &amp; Refining</td><td>Smelter, Base Tender</td><td>Fuel &amp; Ores</td><td>Kiln (Coal), Smelter (Ingots), Windmill (Flour)</td><td><code>[Ore] [Bars]</code></td></tr>
            <tr><td>7. Culinary Arts</td><td>Cook</td><td>Cooking Spit, Cauldron, Fermenter</td><td>Roast Meat, Deer Stew, Sausages, Aged Meads</td><td><code>[Food] [Mead]</code></td></tr>
            <tr><td>8. Logistics &amp; Freight</td><td>Hauler, Craftsman</td><td>Hammer, Vagon Carts, Strongboxes</td><td>50x Wood/Stone Piles, Sign Routing, Cart Logistics</td><td><code>[All Tags]</code></td></tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 5</span>
    </div>
  </div>
""")

# ==================== PAGES 6-16: WORKER CHAPTERS ====================
print("Executing worker chapters...")
exec(worker_chapters_code, globals())

# ==================== PAGE 17: LOGISTICS MASTERY ====================
print("Building Page 17: Logistics Mastery...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Wood_Pile.png")}" class="chapter-icon" alt="Logistics"> PART XIV: LOGISTICS MASTERY &amp; BASE INFRASTRUCTURE</h2>
        <span class="role-pill">STORAGE &bull; SOCIAL</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Overflow Stacks:</span>
          {slot("Wood_Pile.png", "Wood")}
          {slot("Stone.png", "Stone")}
          {slot("Coal.png", "Coal")}
          {slot("Coins.png", "Coins")}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">Social Immersion:</span>
          {slot("Longship.png", "Ship")}
          {slot("Torch.png", "Torch")}
          {slot("Dragon_Bed.png", "Bed")}
        </div>
      </div>

      <div class="stone-tablet panel-now" style="flex: 1;">
        <h3>1. [F05] PHYSICAL OVERFLOW RESOURCE STACKS</h3>
        <p>When settlement chests become completely full, workers never drop items to decay. Instead, they assemble physical resource piles on open terrain:</p>
        <ul>
          <li><strong>Wood Piles:</strong> 50x Wood &rarr; <code>wood_stack</code> &bull; 50x Fine Wood &rarr; <code>wood_fine_stack</code> &bull; 50x Core Wood &rarr; <code>wood_core_stack</code>.</li>
          <li><strong>Mineral Piles:</strong> 50x Stone &rarr; <code>stone_pile</code> &bull; 50x Coal &rarr; <code>coal_pile</code> &bull; 50x Bone Fragments &rarr; <code>bone_stack</code>.</li>
          <li><strong>Treasure Stack:</strong> 99x Coins &rarr; <code>treasure_stack</code> (for late-game wealth consolidation).</li>
          <li><strong>Ownership:</strong> Piles are registered with the local player ID, allowing players to deconstruct them instantly with a Hammer.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-routine" style="flex: 1;">
        <h3>2. [F32] SIGN-BASED SMART TAG ROUTING</h3>
        <p>Workers inspect wall signs placed within 2.2m of any chest and parse tag annotations in brackets:</p>
        <ul>
          <li><strong>[Wood]:</strong> Directs regular Wood, Fine Wood, Core Wood, Yggdrasil Wood, and Bark.</li>
          <li><strong>[Stone] &amp; [Ore]:</strong> Routes rocks, flint, obsidian, copper, tin, iron scrap, and silver ores.</li>
          <li><strong>[Food] &amp; [Seeds]:</strong> Segregates cooked dishes, raw meats, honey, and agricultural seeds.</li>
          <li><strong>[Berries] &amp; [Trophy]:</strong> Stores foraged flora, wild berries, mushrooms, and hunted game trophies.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-future" style="flex: 1;">
        <h3>3. [F33] VAGON CART AUTOMATED LOADING &amp; CARAVAN CONVOYS</h3>
        <ul>
          <li><strong>Wheeled Transport Integration:</strong> Haulers scan for player wheeled carts (Vagon) and transfer heavy ores and building timber directly into cart containers.</li>
          <li><strong>Caravan Staging:</strong> Position empty carts near your main gate; Haulers will automatically fill them with refined metals before your ocean voyages.</li>
          <li><strong>Weight Balancing:</strong> Workers distribute weight evenly to prevent cart tipping on steep switchback terrain.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-synergy" style="flex: 1;">
        <h3>4. SOCIAL LIFE &amp; EXPEDITION IMMERSION [F34&ndash;F36]</h3>
        <ul>
          <li><strong>[F34] Longship Boarding:</strong> When following player aboard a Karve or Longship, workers automatically mount passenger benches.</li>
          <li><strong>[F35] Night Torch Bearing:</strong> During nighttime (<code>EnvMan.IsNight()</code>) or inside dark dungeons, workers draw lit torches to guide paths.</li>
          <li><strong>[F36] Tavern Seating &amp; Bed Rest:</strong> During off-duty idle cycles, workers visit settlement chairs, stools, and beds to socialize and rest.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-rec" style="flex: 1;">
        <h3>5. STRONGHOLD LOGISTICS BLUEPRINT &amp; YARD ORGANIZATION RULES</h3>
        <ul>
          <li><strong>Central Thoroughfare:</strong> Maintain a 4m-wide paved road connecting logging gates, quarry pits, and the central great hall.</li>
          <li><strong>Foundry Triangle:</strong> Keep Ore storage, Charcoal Kilns, and Smelters within an 8m radius to minimize smelting commute lag.</li>
          <li><strong>Quarry Staging:</strong> Flatten a 10&times;10m yard with a Hoe near mine pits to allow automated stone piles to erect cleanly.</li>
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

# ==================== PAGE 18: DIAGNOSTICS ====================
print("Building Page 18: Diagnostics & Telemetry...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Workbench.png")}" class="chapter-icon" alt="Diagnostics"> PART XV: DIAGNOSTICS, TELEMETRY &amp; MULTIPLAYER MUTEX</h2>
        <span class="role-pill">SYSTEM LOGIC</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">System Sensors:</span>
          {slot("Workbench.png", "Bench")}
          {slot("Forge.png", "Forge")}
          {slot("Antler_Pickaxe.png", "Tier")}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">Diagnostics Signals:</span>
          {slot("Sign.png", "ZDO")}
          {slot("Valkyrie.png", "Alert")}
          {slot("Torch.png", "Tick")}
        </div>
      </div>

      <div class="stone-tablet panel-now" style="flex: 1;">
        <h3>1. THE UNSTUCK HOP ENGINE &amp; NAVIGATION RECOVERY</h3>
        <p>Valheim's dynamic geometry can cause pathfinding bottlenecks. The worker navigation engine utilizes an autonomous 3-stage unstuck routine:</p>
        <ul>
          <li><strong>Stage 1 (Geometric Velocity Check):</strong> If worker velocity &lt; 0.15m/s for &gt;4s while pathing, the worker re-samples NavMesh rays within 3m.</li>
          <li><strong>Stage 2 (Path Inversion):</strong> Backtracks 2m along the previous valid waypoint to escape convex collision geometry.</li>
          <li><strong>Stage 3 (Physics Hop):</strong> If trapped &gt;12s, applies an upward 1.5m physics impulse combined with a directional leap toward the goal.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-routine" style="flex: 1;">
        <h3>2. NAVMESH BOUNDARIES &amp; WORK POSITION ANCHORING</h3>
        <ul>
          <li><strong>35m Radial Horizon:</strong> Workers search for job targets strictly within 35m of their Work Post, eliminating runaway wandering.</li>
          <li><strong>Water Hazard Avoidance:</strong> Workers do not path into water deeper than 1.1m unless boarding a player boat [F34].</li>
          <li><strong>Steep Cliff Clamping:</strong> Slopes &gt;45&deg; are flagged impassable; Miners use Hoes to create step cuts rather than leaping.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-future" style="flex: 1;">
        <h3>3. TOOL TIER FALLBACKS &amp; AUTOMATIC BENCH HOMING</h3>
        <ul>
          <li><strong>Hardness Verification:</strong> If a Miner encounters Iron or Obsidian without an Iron Pickaxe, the worker logs a tool-tier deficit and moves to surface stone.</li>
          <li><strong>15% Durability Trigger:</strong> Before any equipped tool shatters, the AI automatically disengages and hikes to the nearest repaired bench.</li>
          <li><strong>Station Level Check:</strong> Verifies Forge/Workbench upgrade stars match tool recipe requirements before attempting repair.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-synergy" style="flex: 1;">
        <h3>4. MULTIPLAYER SYNCHRONIZATION &amp; ZONE MUTEX</h3>
        <ul>
          <li><strong>ZDO Authority Arbitration:</strong> Worker state is backed by Valheim's authoritative ZDO engine; only the local zone owner executes AI updates.</li>
          <li><strong>Chest Access Mutex:</strong> Prevents multiple workers or players from simultaneously withdrawing from the same chest slot, preventing duplication.</li>
          <li><strong>Dedicated Server Persistence:</strong> Workers remain securely anchored at their Camp and Work posts across server restarts.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-rec" style="flex: 1;">
        <h3>5. F8 SETTLEMENT TELEMETRY DASHBOARD</h3>
        <ul>
          <li><strong>Real-Time Roster Inspector:</strong> View every worker's active HP, food timers, durability %, and current AI state machine node.</li>
          <li><strong>Target Highlighting:</strong> Click any worker in F8 to project an in-world runic beacon displaying their Work and Camp posts.</li>
          <li><strong>Global Directives:</strong> Trigger settlement-wide 'Raid Defense' or 'Return to Camp' protocols with a single steward command.</li>
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

# ==================== PAGE 19: MASTER ROSTER ====================
print("Building Page 19: Master Settlement Roster & Specifications...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Reinforced_Chest.png")}" class="chapter-icon" alt="Roster"> PART XVI: MASTER SETTLEMENT ROSTER &amp; SPECIFICATIONS</h2>
        <span class="role-pill">SPECIFICATIONS &bull; SEAL</span>
      </div>

      <div class="stone-tablet panel-now" style="flex: 2.1;">
        <h3>11-WORKER MASTER ROSTER &amp; ATTRIBUTE SPECIFICATIONS</h3>
        <p style="font-size: 7.4pt; color: #ded5c5; margin-bottom: 2px;">
          Comprehensive role specification table detailing operational attributes, tool requirements, and base integration across all 11 worker classes:
        </p>
        <table class="stone-table">
          <thead>
            <tr>
              <th>ROLE TITLE</th>
              <th style="width: 48px;">BASE HP</th>
              <th>PRIMARY TOOL</th>
              <th style="width: 60px;">SCAN RAD</th>
              <th style="width: 60px;">CAPACITY</th>
              <th>PRIMARY WORKSTATION</th>
              <th>KEY OUTPUT YIELD</th>
            </tr>
          </thead>
          <tbody>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">LUMBERJACK</td><td>150</td><td>Axe (Stone/Bronze/Iron)</td><td>30m</td><td>300 wt</td><td>Camp Workbench</td><td>Wood, Fine Wood, Core Wood, 50x Piles</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">MINER</td><td>150</td><td>Pickaxe &amp; Hoe</td><td>25m</td><td>350 wt</td><td>Camp Forge</td><td>Stone, Copper, Tin, Iron Scrap, Flint</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">FARMER</td><td>150</td><td>Bronze Cultivator</td><td>25m</td><td>300 wt</td><td>Cultivated Plots</td><td>Carrots, Turnips, Onions, Barley, Boars</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">GATHERER</td><td>150</td><td>Flint Knife / Hands</td><td>35m</td><td>250 wt</td><td>Beehives &amp; Extractors</td><td>Berries, Mushrooms, Thistle, Honey, Sap</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">HUNTER</td><td>160</td><td>Bow &amp; Arrows</td><td>40m</td><td>300 wt</td><td>Perimeter Stands</td><td>Raw Meat, Deer Hides, Feathers, Trophies</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">GUARD</td><td>180</td><td>Bow, Sword &amp; Shield</td><td>35m</td><td>300 wt</td><td>Watchtowers &amp; Gates</td><td>Perimeter Defense, War Horn Alerts, Drops</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">SMELTER</td><td>150</td><td>Kiln &amp; Furnace</td><td>20m</td><td>350 wt</td><td>Smelters &amp; Blast Furnace</td><td>Refined Copper, Tin, Bronze, Iron, Silver</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">BASE TENDER</td><td>150</td><td>Torch &amp; Fuel</td><td>30m</td><td>300 wt</td><td>Hearths, Windmills</td><td>Fireplace Refueling, Barley Flour, Thread</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">COOK</td><td>150</td><td>Spit &amp; Cauldron</td><td>15m</td><td>250 wt</td><td>Cauldron &amp; Fermenter</td><td>Roast Meats, Hearty Stews, Aged Meads</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">HAULER</td><td>160</td><td>Vagon Cart / Hands</td><td>35m</td><td>450 wt</td><td>Warehouses &amp; Docks</td><td>Cart Packing, Smart Chest Sorting, 50x Piles</td></tr>
            <tr><td style="font-family:'Norsebold',serif; color:#f7d57f;">CRAFTSMAN</td><td>150</td><td>Hammer</td><td>20m</td><td>300 wt</td><td>Upgraded Workbenches</td><td>Tool Repair, Arrow Fletching, Armor Smithing</td></tr>
          </tbody>
        </table>
      </div>

      <div class="stone-tablet panel-routine" style="flex: 1.1;">
        <h3>ENDGAME STRONGHOLD DEPLOYMENT MODEL (50-PERSON MEGA-FORTRESS)</h3>
        <p style="font-size: 7.5pt; color: #ded5c5; margin-bottom: 2px;">Recommended roster allocation to achieve 100% automated self-sufficiency in an endgame Nordic Citadel:</p>
        <ul>
          <li><strong>Resource Gathering Core (18 Workers):</strong> 5 Lumberjacks (Forester grove), 6 Miners (Deep iron/silver quarry), 4 Farmers (Terraced gardens), 3 Gatherers (Orchard &amp; sap tapping).</li>
          <li><strong>Processing &amp; Refining (12 Workers):</strong> 4 Smelters (Battery of 4 kilns &amp; 4 smelters), 3 Base Tenders (Citadel fires &amp; windmills), 3 Cooks (Brewery &amp; double cauldrons), 2 Craftsmen (Central armory).</li>
          <li><strong>Logistics &amp; Infrastructure (8 Workers):</strong> 8 Haulers (Cart convoys, yard sweepers, dock loading for longship voyages).</li>
          <li><strong>Perimeter Defense Garrison (12 Guards):</strong> 4 Gatehouse Sentinels (Bow/Sword CQC), 4 Parapet Tower Snipers (Huntsman bows), 4 Mobile Patrol Wardens (War horn alert team).</li>
        </ul>
      </div>

      <div class="stone-tablet panel-rec" style="flex: 0.8;">
        <h3>{hugin()} EPILOGUE &amp; VALHALLA CERTIFICATION</h3>
        <p style="font-size: 7.7pt; line-height: 1.34;">
          With this codex, your settlement operates as a living, breathing Nordic ecosystem. By respecting the dual-anchor spatial hierarchy, honoring the 36-feature operational matrix, and providing nourishing rations, your people will conquer the tenth realm and forge a legendary empire worthy of Odin's halls. Sk&aring;l!
        </p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0 &bull; Master Specification</span>
      <span>Page 19</span>
    </div>
  </div>
""")

parts.append("""
</body>
</html>
""")

full_html = "".join(parts)
with open(HTML_OUT, "w", encoding="utf-8") as f:
    f.write(full_html)
print(f"Generated {HTML_OUT} ({len(full_html)} bytes)")

shutil.copy(HTML_OUT, SUBDIR_HTML)
print(f"Copied to {SUBDIR_HTML}")

# Compile PDF with Edge headless
print("Compiling PDF with Edge headless...")
edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if not os.path.exists(edge_path):
    edge_path = r"C:\Program Files\Microsoft\Edge\Application\msedge.exe"

cmd = [
    edge_path,
    "--headless=new",
    "--disable-gpu",
    "--no-pdf-header-footer",
    f"--print-to-pdf={PDF_OUT}",
    HTML_OUT
]
subprocess.run(cmd, check=True)
print(f"Generated {PDF_OUT} ({os.path.getsize(PDF_OUT)} bytes)")

shutil.copy(PDF_OUT, SUBDIR_PDF)
print(f"Copied to {SUBDIR_PDF}")

# Verify Page Count with PyMuPDF
import pymupdf
doc = pymupdf.open(PDF_OUT)
num_pages = len(doc)
print(f"Verified PDF Page Count: {num_pages}")

if num_pages == 19:
    print("SUCCESS: EXACT 19-PAGE COUNT ACHIEVED!")
else:
    print(f"WARNING: PAGE COUNT IS {num_pages}, TARGET IS 19!")

# Render all sample pages to verify
print("Rendering sample verification pages (Pages 1, 4, 5, 7, 17, 19)...")
for p_idx in [0, 3, 4, 6, 16, 18]:
    pix = doc[p_idx].get_pixmap(dpi=150)
    out_img = os.path.join(BASE_DIR, f"stone_p{p_idx+1}.png")
    pix.save(out_img)
    print(f"Saved {out_img}")

# ==================== UPDATE WORKER FEATURE MAP ====================
print("Updating worker_feature_map.html with authentic stone relief aesthetic...")
with open(os.path.join(BASE_DIR, "worker_feature_map.html"), "r", encoding="utf-8") as f:
    map_html = f.read()

# Replace background and styling in worker_feature_map.html
# Replace root variables
map_html = map_html.replace(
    "--bg-dark: #0A0704;",
    f"--bg-dark: #1a1e25;\n    --stone-monolith: url('data:image/png;base64,{stone_monolith_b64}');\n    --stone-tablet: url('data:image/png;base64,{stone_tablet_b64}');\n    --stone-socket: url('data:image/png;base64,{stone_socket_b64}');"
)
map_html = map_html.replace("--bg-page: #120D09;", "--bg-page: #16191f;")
map_html = map_html.replace("--bg-card: #18120C;", "--bg-card: #16191f;")
map_html = map_html.replace("--bg-hover: #261B12;", "--bg-hover: #222730;")
map_html = map_html.replace("--border-dark: #4A351D;", "--border-dark: #3c2f1c;")
map_html = map_html.replace("--border-gold: #C29D53;", "--border-gold: #9a7638;")

# Update body background
map_html = map_html.replace(
    "background-color: var(--bg-dark);",
    "background-color: #1a1e25;\n    background-image: var(--stone-monolith);\n    background-repeat: repeat;\n    background-size: 380px 380px;"
)

# Update header
map_html = map_html.replace(
    "background: radial-gradient(circle at center, #241910 0%, #120D09 100%);",
    "background-color: #16191f;\n    background-image: var(--stone-tablet);\n    background-repeat: repeat;\n    border-top: 1.5px solid rgba(240, 215, 160, 0.35);\n    border-left: 1.5px solid rgba(240, 215, 160, 0.25);\n    border-bottom: 2.5px solid #06080a;\n    border-right: 2px solid #06080a;\n    box-shadow: inset 1.5px 1.5px 4px rgba(0,0,0,0.85), 0 8px 24px rgba(0,0,0,0.8);"
)

# Update catalog-card
map_html = map_html.replace(
    "background: var(--bg-card);\n    border: 1px solid var(--border-dark);\n    border-left: 4px solid var(--border-gold);",
    "background-color: #16191f;\n    background-image: var(--stone-tablet);\n    background-repeat: repeat;\n    border-top: 1.5px solid rgba(240, 215, 160, 0.30);\n    border-left: 3.5px solid #d4a017;\n    border-bottom: 2.5px solid #06080a;\n    border-right: 2px solid #06080a;\n    box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8), 0 4px 12px rgba(0,0,0,0.6);"
)

# Update card-icon to use sunken stone socket
map_html = map_html.replace(
    "background: #110B07;\n    border: 1.5px solid var(--border-dark);",
    "background-image: var(--stone-socket);\n    background-size: cover;\n    border-top: 1px solid #0a0d10;\n    border-left: 1px solid #0a0d10;\n    border-bottom: 1.5px solid rgba(240, 215, 160, 0.35);\n    border-right: 1.5px solid rgba(240, 215, 160, 0.25);"
)

# Save updated worker_feature_map.html
with open(MAP_OUT, "w", encoding="utf-8") as f:
    f.write(map_html)
print(f"Generated {MAP_OUT} ({len(map_html)} bytes)")

shutil.copy(MAP_OUT, SUBDIR_MAP)
print(f"Copied to {SUBDIR_MAP}")

print("=== ALL ETCHED-IN-STONE DELIVERABLES SUCCESSFULLY UPDATED! ===")
