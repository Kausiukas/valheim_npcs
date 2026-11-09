# Master Builder for Etched-in-Stone Valheim Compendium
import os
import sys
import base64
import subprocess
import shutil

BASE_DIR = r"d:\valheim_mods"
ASSETS_DIR = os.path.join(BASE_DIR, "docs", "assets")
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")
STONE_DIR = os.path.join(ASSETS_DIR, "stone")

HTML_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.html")
PDF_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.pdf")
SUBDIR_HTML = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.html")
SUBDIR_PDF = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.pdf")

print("--- Loading Base64 Fonts & Stone Textures ---")
def b64(d, fn):
    p = os.path.join(d, fn)
    if not os.path.exists(p):
        print(f"Warning: {p} missing!")
        return ""
    with open(p, "rb") as f:
        return base64.b64encode(f.read()).decode("ascii")

norse_b64 = b64(FONTS_DIR, "Norse.otf")
norsebold_b64 = b64(FONTS_DIR, "Norsebold.otf")
averia_regular_b64 = b64(FONTS_DIR, "AveriaSerifLibre-Regular.ttf")
averia_bold_b64 = b64(FONTS_DIR, "AveriaSerifLibre-Bold.ttf")
averia_italic_b64 = b64(FONTS_DIR, "AveriaSerifLibre-Italic.ttf")
averia_bolditalic_b64 = b64(FONTS_DIR, "AveriaSerifLibre-BoldItalic.ttf")

stone_monolith_b64 = b64(STONE_DIR, "stone_monolith_bg.png")
stone_tablet_b64 = b64(STONE_DIR, "stone_tablet_bg.png")
stone_socket_b64 = b64(STONE_DIR, "stone_socket_bg.png")
stone_socket_eq_b64 = b64(STONE_DIR, "stone_socket_equipped_bg.png")
stone_cover_crest_b64 = b64(STONE_DIR, "stone_cover_crest.png")

def ic(name):
    return f"docs/assets/icons/{name}"

def slot(icon, label, equipped=False):
    eq_class = " equipped" if equipped else ""
    return f"""<div class="valheim-slot{eq_class}"><img src="{ic(icon)}" alt="{label}"><span class="slot-label">{label}</span></div>"""

def feat(code, name, icon):
    return f"""<span class="feature-badge"><img src="{ic(icon)}" alt="{code}"> {code} {name}</span>"""

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

print("--- Assembling Etched-in-Stone HTML ---")
parts = []
parts.append(f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>ValheimWorkers v0.3.0 - The Settlement Steward's Codex</title>
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
      margin: 7mm 8mm 7mm 8mm;
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
        height: 283mm;
        max-height: 283mm;
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
      background-color: #0A0806;
      color: #E8DFD1;
      line-height: 1.40;
      font-size: 8.5pt;
    }}

    /* The Monolithic Basalt Stone Slab Page */
    .page {{
      height: 283mm;
      max-height: 283mm;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      page-break-before: always;
      break-before: page;
      padding: 6.5mm 8mm 4.5mm 8mm;
      overflow: hidden;
      background: linear-gradient(175deg, #1A140E 0%, #221A12 40%, #16100B 100%), url('data:image/png;base64,{stone_monolith_b64}') repeat;
      background-blend-mode: overlay;
      border: 3px solid #4D371E;
      outline: 1.5px solid #8F692E;
      outline-offset: -5px;
      border-radius: 6px;
      box-shadow: inset 0 0 28px rgba(0,0,0,0.92), inset 1.5px 1.5px 3px rgba(245, 215, 165, 0.18), 0 8px 30px rgba(0,0,0,0.85);
      position: relative;
    }}
    .page-first {{
      page-break-before: avoid;
      break-before: avoid;
    }}

    /* Carved Elder Futhark Rune Ribbon */
    .page::before {{
      content: "ᚠ   ᚦ   ᚨ   ᚱ   ᛋ   ᛏ   ᛒ   ᛖ   ᛗ   ᛚ   ᛜ   ᛞ   ᛟ";
      position: absolute;
      top: 2.5px;
      left: 14px;
      right: 14px;
      text-align: center;
      font-family: 'Norsebold', serif;
      font-size: 6.2pt;
      color: rgba(220, 175, 75, 0.45);
      letter-spacing: 6px;
      pointer-events: none;
      text-shadow: -1px -1px 0px #000, 1px 1px 0px rgba(245, 215, 165, 0.18);
    }}

    .content-container {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      min-height: 0;
    }}

    /* Chiseled Chapter Header */
    .chapter-header {{
      border-bottom: 2px solid #755220;
      padding-bottom: 3.5px;
      margin-bottom: 4.5px;
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      position: relative;
    }}
    .chapter-header h2 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 15.5pt;
      color: #FFC42B;
      letter-spacing: 0.04em;
      text-shadow: -1px -1.5px 1px #000, 1px 1.5px 1px rgba(255, 215, 130, 0.35);
      display: flex;
      align-items: center;
      gap: 6px;
    }}
    .chapter-header .chapter-icon {{
      width: 22px;
      height: 22px;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.9));
    }}
    .role-pill {{
      background: linear-gradient(135deg, #7A5116 0%, #52350C 100%);
      color: #FFF4D9;
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 7.8pt;
      padding: 2.5px 9px;
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
      margin-top: 3px;
    }}

    /* Raised Stone Plinth Showcase Bar */
    .showcase-bar {{
      background: linear-gradient(135deg, rgba(28, 20, 14, 0.95) 0%, rgba(16, 11, 7, 0.98) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1.5px solid rgba(220, 185, 135, 0.28);
      border-left: 1.5px solid rgba(220, 185, 135, 0.22);
      border-bottom: 2px solid #000;
      border-right: 2px solid #000;
      border-radius: 5px;
      padding: 4.5px 8px;
      margin-bottom: 5px;
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 6px;
      box-shadow: inset 1px 1px 4px rgba(0,0,0,0.8), 0 3px 8px rgba(0,0,0,0.65);
    }}
    .showcase-group {{
      display: flex;
      align-items: center;
      gap: 6px;
    }}
    .showcase-title {{
      font-family: 'Norsebold', serif;
      font-size: 7.6pt;
      color: #E5AC42;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin-right: 3px;
      text-shadow: -1px -1px 0px #000;
    }}
    
    /* Sunken Relic Sockets */
    .valheim-slot {{
      width: 32px;
      height: 32px;
      background: radial-gradient(circle at 65% 65%, #0B0806 0%, #15100B 100%), url('data:image/png;base64,{stone_socket_b64}') no-repeat center/cover;
      border-top: 1.5px solid #140E0A;
      border-left: 1.5px solid #140E0A;
      border-bottom: 1.5px solid rgba(225, 185, 135, 0.28);
      border-right: 1.5px solid rgba(225, 185, 135, 0.22);
      border-radius: 4px;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;
      box-shadow: inset 2px 2px 6px rgba(0,0,0,0.95), inset -1px -1px 2px rgba(255,255,255,0.05), 0 2px 5px rgba(0,0,0,0.7);
    }}
    .valheim-slot.equipped {{
      border: 1.5px solid #38BDF8 !important;
      box-shadow: inset 0 0 8px rgba(56, 189, 248, 0.65), 0 0 8px rgba(56, 189, 248, 0.5), inset 2px 2px 5px #000 !important;
    }}
    .valheim-slot img {{
      width: 25px;
      height: 25px;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px rgba(0,0,0,0.85));
    }}
    .valheim-slot .slot-label {{
      position: absolute;
      bottom: -11.5px;
      left: 50%;
      transform: translateX(-50%);
      font-size: 5.8pt;
      color: #E2D7C8;
      white-space: nowrap;
      font-family: 'Averia Serif Libre', serif;
      text-shadow: 0 1px 2px #000, 0 0 3px #000;
    }}
    .valheim-slot.equipped::after {{
      content: "";
      position: absolute;
      top: -2px;
      right: -2px;
      width: 6px;
      height: 6px;
      background: #38BDF8;
      border-radius: 50%;
      box-shadow: 0 0 5px #38BDF8;
    }}

    /* Worker Grid 6 Tablets - Full Height Spanning */
    .worker-grid-6 {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      gap: 5.5px;
      min-height: 0;
    }}

    /* Recessed Stone Tablets */
    .stone-tablet {{
      background: linear-gradient(135deg, rgba(30, 22, 16, 0.94) 0%, rgba(18, 13, 9, 0.97) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1.5px solid rgba(225, 185, 135, 0.28);
      border-left: 1.5px solid rgba(225, 185, 135, 0.20);
      border-bottom: 2px solid rgba(0, 0, 0, 0.95);
      border-right: 2px solid rgba(0, 0, 0, 0.9);
      border-radius: 5px;
      padding: 5.5px 9.5px;
      box-shadow: inset 1.5px 1.5px 4px rgba(0,0,0,0.85), inset -1px -1px 2px rgba(255,255,255,0.03), 0 3px 8px rgba(0,0,0,0.65);
    }}
    .stone-tablet h3 {{
      font-family: 'Norsebold', serif;
      font-size: 9.3pt;
      margin-bottom: 2.8px;
      display: flex;
      align-items: center;
      gap: 5px;
      letter-spacing: 0.03em;
    }}
    .stone-tablet p {{
      font-size: 8.3pt;
      line-height: 1.36;
      color: #DDD2C1;
      margin-bottom: 2.2px;
      text-shadow: -0.5px -1px 0px rgba(0,0,0,0.9);
    }}
    .stone-tablet ul {{
      margin-left: 15px;
      font-size: 8.2pt;
      color: #E6DCD0;
      line-height: 1.35;
    }}
    .stone-tablet li {{
      margin-bottom: 2.0px;
      text-shadow: -0.5px -1px 0px rgba(0,0,0,0.9);
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
      width: 11px;
      height: 11px;
      object-fit: contain;
    }}

    /* Chiseled Stone Tables */
    table {{
      width: 100%;
      border-collapse: collapse;
      margin: 4px 0;
      font-size: 7.4pt;
      background: rgba(18, 13, 9, 0.95);
      border: 1px solid #4D3921;
      border-radius: 4px;
      overflow: hidden;
      box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8);
    }}
    th, td {{
      border: 1px solid #3E2D1A;
      padding: 3.2px 5.5px;
      text-align: left;
    }}
    th {{
      background: linear-gradient(180deg, #2A1D13 0%, #1D140C 100%);
      color: #FFC42B;
      font-family: 'Norsebold', serif;
      font-weight: 700;
      font-size: 7.5pt;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      border-bottom: 2px solid #845B1C;
      text-shadow: -1px -1px 0px #000;
    }}
    tr:nth-child(even) td {{
      background: rgba(28, 20, 14, 0.6);
    }}
    td strong {{
      color: #FCD34D;
      font-family: 'Norse', serif;
      font-size: 7.6pt;
    }}

    /* Cover Page - Monumental Runestone Stele */
    .cover-page {{
      background: radial-gradient(circle at center, #261B12 0%, #150E09 60%, #0A0704 100%), url('data:image/png;base64,{stone_monolith_b64}') repeat;
      background-blend-mode: overlay;
      border: 3.5px double #C49137;
      outline: 1.5px solid #6E4F22;
      outline-offset: -7px;
      text-align: center;
      align-items: center;
      padding: 8mm 10mm 6mm 10mm;
      box-shadow: inset 0 0 35px rgba(0,0,0,0.95), 0 10px 35px rgba(0,0,0,0.9);
    }}
    .cover-container {{
      flex: 1;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      align-items: center;
      width: 100%;
      height: 100%;
    }}
    .cover-title {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 29pt;
      line-height: 1.12;
      color: #FFC42B;
      margin-top: 4px;
      margin-bottom: 3px;
      text-shadow: -1.5px -2px 0px #000, 1.5px 2px 2px rgba(255, 215, 130, 0.4), 0 0 20px rgba(235, 143, 13, 0.45);
      letter-spacing: 0.06em;
    }}
    .cover-subtitle {{
      font-family: 'Averia Serif Libre', serif;
      font-size: 11pt;
      color: #E2D7C8;
      max-width: 560px;
      margin: 0 auto;
      font-style: italic;
      text-shadow: -1px -1px 0px #000;
    }}
    .cover-divider {{
      width: 220px;
      height: 2px;
      background: linear-gradient(90deg, transparent, #DDA13B, transparent);
      margin: 6px auto;
    }}
    
    /* Circular Runestone Medallion */
    .cover-medallion-box {{
      position: relative;
      width: 170px;
      height: 170px;
      margin: 4px 0;
      display: flex;
      align-items: center;
      justify-content: center;
    }}
    .cover-medallion-bg {{
      position: absolute;
      width: 100%;
      height: 100%;
      object-fit: contain;
      filter: drop-shadow(0 8px 18px rgba(0,0,0,0.95));
    }}
    .cover-medallion-crest {{
      position: relative;
      z-index: 2;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }}
    .cover-medallion-crest img.main-shield {{
      width: 76px;
      height: 76px;
      filter: drop-shadow(0 4px 10px rgba(0,0,0,0.95)) drop-shadow(0 0 10px rgba(255, 191, 31, 0.35));
    }}
    .cover-medallion-crest img.flank-tool {{
      width: 44px;
      height: 44px;
      filter: drop-shadow(0 3px 6px rgba(0,0,0,0.9));
    }}

    .cover-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 7px;
      width: 100%;
      max-width: 610px;
      margin: 4px auto;
    }}
    .cover-card {{
      background: linear-gradient(135deg, rgba(28, 20, 14, 0.94) 0%, rgba(16, 11, 7, 0.98) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1.5px solid rgba(220, 185, 135, 0.28);
      border-left: 1.5px solid rgba(220, 185, 135, 0.22);
      border-bottom: 2px solid #000;
      border-right: 2px solid #000;
      border-radius: 5px;
      padding: 6px 10px;
      text-align: left;
      display: flex;
      align-items: center;
      gap: 9px;
      box-shadow: inset 1px 1px 4px rgba(0,0,0,0.8), 0 3px 8px rgba(0,0,0,0.65);
    }}
    .cover-card img {{
      width: 30px;
      height: 30px;
      object-fit: contain;
      filter: drop-shadow(0 2px 4px #000);
    }}
    .cover-card strong {{
      color: #FFC42B;
      font-family: 'Norsebold', serif;
      font-size: 8.5pt;
      display: block;
      margin-bottom: 2px;
      text-shadow: -1px -1px 0px #000;
    }}
    .cover-card p {{
      color: #C7BAAA;
      font-size: 7.5pt;
      line-height: 1.28;
      text-shadow: -0.5px -1px 0px rgba(0,0,0,0.9);
    }}

    .cover-oath {{
      background: linear-gradient(135deg, rgba(24, 17, 12, 0.94) 0%, rgba(14, 9, 6, 0.98) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1.5px solid rgba(220, 185, 135, 0.28);
      border-left: 1.5px solid rgba(220, 185, 135, 0.22);
      border-bottom: 2px solid #000;
      border-right: 2px solid #000;
      border-radius: 5px;
      padding: 9px 16px;
      max-width: 610px;
      margin: 4px auto;
      font-style: italic;
      color: #DFD3C2;
      font-size: 8.0pt;
      line-height: 1.40;
      position: relative;
      text-shadow: -1px -1px 0px #000;
      box-shadow: inset 1.5px 1.5px 5px rgba(0,0,0,0.9), 0 3px 8px rgba(0,0,0,0.65);
    }}

    /* TOC Grids */
    .toc-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 6px;
      margin-top: 4px;
    }}
    .toc-card {{
      background: linear-gradient(135deg, rgba(28, 20, 14, 0.95) 0%, rgba(16, 11, 7, 0.98) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1.5px solid rgba(220, 185, 135, 0.28);
      border-left: 1.5px solid rgba(220, 185, 135, 0.22);
      border-bottom: 2px solid #000;
      border-right: 2px solid #000;
      border-radius: 5px;
      padding: 5.5px 8.5px;
      box-shadow: inset 1px 1px 4px rgba(0,0,0,0.8), 0 3px 8px rgba(0,0,0,0.65);
    }}
    .toc-card h4 {{
      font-family: 'Norsebold', serif;
      font-size: 8.6pt;
      color: #FFC42B;
      border-bottom: 1px solid #5C4425;
      padding-bottom: 2px;
      margin-bottom: 3.5px;
      display: flex;
      align-items: center;
      gap: 4.5px;
      text-shadow: -1px -1px 0px #000;
    }}
    .toc-card h4 img {{
      width: 13px;
      height: 13px;
      object-fit: contain;
    }}
    .toc-list {{
      list-style: none;
      font-size: 7.5pt;
    }}
    .toc-list li {{
      display: flex;
      justify-content: space-between;
      margin-bottom: 1.8px;
      border-bottom: 1px dotted #3D2D1B;
      padding-bottom: 1.5px;
      text-shadow: -0.5px -1px 0px rgba(0,0,0,0.9);
    }}
    .toc-list li span:first-child {{
      display: flex;
      align-items: center;
      gap: 4px;
    }}
    .toc-list li span:first-child img {{
      width: 11px;
      height: 11px;
      object-fit: contain;
    }}
    .toc-list li span:last-child {{
      font-family: 'Norsebold', serif;
      color: #E5AC42;
      font-size: 7.6pt;
    }}

    /* 36-Feature Catalog Grid */
    .feat-catalog-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 3.5px 6px;
      margin-top: 3px;
    }}
    .feat-card-comp {{
      background: linear-gradient(135deg, rgba(28, 20, 14, 0.95) 0%, rgba(16, 11, 7, 0.98) 100%), url('data:image/png;base64,{stone_tablet_b64}') repeat;
      background-blend-mode: overlay;
      border-top: 1px solid rgba(220, 185, 135, 0.25);
      border-left: 1px solid rgba(220, 185, 135, 0.20);
      border-bottom: 1.5px solid #000;
      border-right: 1.5px solid #000;
      border-radius: 4px;
      padding: 2.8px 6px;
      display: flex;
      align-items: center;
      gap: 6px;
      box-shadow: inset 1px 1px 3px rgba(0,0,0,0.8), 0 2px 5px rgba(0,0,0,0.6);
    }}
    .feat-card-comp .feat-img-box {{
      width: 25px;
      height: 25px;
      background: radial-gradient(circle at 65% 65%, #0B0806 0%, #15100B 100%);
      border: 1px solid #4A351E;
      border-radius: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: inset 1px 1px 4px #000;
    }}
    .feat-card-comp .feat-img-box img {{
      width: 20px;
      height: 20px;
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
      line-height: 1.15;
    }}
    .feat-card-head strong {{
      font-family: 'Norsebold', serif;
      font-size: 7.7pt;
      color: #FFC42B;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      text-shadow: -1px -1px 0px #000;
    }}
    .feat-card-domain {{
      font-size: 5.6pt;
      color: #A3937F;
      font-family: 'Norse', serif;
      text-transform: uppercase;
      padding: 0 3px;
      background: #251B12;
      border-radius: 2px;
    }}
    .feat-card-desc {{
      font-size: 6.8pt;
      color: #CFC2B2;
      line-height: 1.18;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      text-shadow: -0.5px -1px 0px rgba(0,0,0,0.9);
    }}

    /* Separation Matrix Table */
    table.matrix-view {{
      font-size: 6.8pt;
      text-align: center;
    }}
    table.matrix-view th, table.matrix-view td {{
      padding: 2.2px 1.2px;
      border: 1px solid #3E2E1D;
    }}
    table.matrix-view th.feat-hdr {{
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      height: 64px;
      font-size: 6.3pt;
      padding: 2px 1.5px;
      background: #20170F;
      color: #E5AC42;
      font-family: 'Norse', serif;
      text-shadow: -1px -1px 0px #000;
    }}
    table.matrix-view th.worker-hdr {{
      text-align: left;
      padding-left: 6px;
      background: #20170F;
      color: #FFC42B;
      width: 82px;
      text-shadow: -1px -1px 0px #000;
    }}
    table.matrix-view td.w-name {{
      text-align: left;
      padding-left: 6px;
      font-family: 'Norsebold', serif;
      color: #F8E7C9;
      background: #17110B;
      font-size: 6.9pt;
      text-shadow: -1px -1px 0px #000;
    }}
    .m-yes {{
      color: #4ADE80;
      font-weight: bold;
      font-size: 7.6pt;
      text-shadow: 0 0 3px rgba(74, 222, 128, 0.4);
    }}
    .m-no {{
      color: #4A3724;
      font-size: 5.5pt;
    }}
  </style>
</head>
<body>
""")

# ==================== PAGE 1: COVER ====================
print("Building Page 1: Monumental Runestone Stele Cover...")
parts.append(f"""
  <div class="page page-first cover-page">
    <div class="cover-container">
      <div>
        <span class="role-pill" style="font-size: 8.5pt; padding: 3px 14px; margin-bottom: 6px;">ValheimWorkers v0.3.0 &bull; Official Compendium</span>
        <h1 class="cover-title">THE SETTLEMENT<br>STEWARD'S CODEX</h1>
        <p class="cover-subtitle">Autonomous AI Behaviors, Spatial Memory, 36-Feature Map &amp; Domain Operations</p>
      </div>
      
      <div class="cover-divider"></div>

      <!-- Carved Circular Runestone Medallion -->
      <div class="cover-medallion-box">
        <img src="data:image/png;base64,{stone_cover_crest_b64}" class="cover-medallion-bg" alt="Runestone Medallion">
        <div class="cover-medallion-crest">
          <img src="{ic("Bronze_Axe.png")}" class="flank-tool" alt="Bronze Axe">
          <img src="{ic("Banded_Shield.png")}" class="main-shield" alt="Banded Shield">
          <img src="{ic("Finewood_Bow.png")}" class="flank-tool" alt="Finewood Bow">
        </div>
      </div>

      <div class="cover-divider"></div>

      <!-- 4 Raised Stone Feature Tablets -->
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

      <!-- Engraved Steward's Oath Tablet -->
      <div class="cover-oath">
        &ldquo;To carve a thriving hall out of the primordial wilderness of Valheim demands more than a sharp axe and strong back—it demands stewardship. May this codex guide your loyal kin as they till the soil, hollow the veins of the earth, stoke the fires of the forge, and stand resolute against the terrors of the night.&rdquo;
      </div>
      
      <div style="font-size: 7.4pt; color: #9C8C78; font-family: 'Averia Serif Libre', serif; text-shadow: -1px -1px 0px #000;">
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
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Hugin.png")}" class="chapter-icon" alt="Hugin"> Table of Contents &amp; Executive Blueprint</h2>
          <span class="role-pill">Codex Index</span>
        </div>

        <div class="toc-grid">
          <div class="toc-card">
            <h4><img src="{ic("Workbench.png")}" alt="Icon"> Part I: Foundations &amp; System Architecture</h4>
            <ul class="toc-list">
              <li><span><img src="{ic("Hammer.png")}"> Core Controls &amp; In-World Hotkeys</span> <span>PAGE 3</span></li>
              <li><span><img src="{ic("Sign.png")}"> Dual-Anchor Spatial Post Architecture</span> <span>PAGE 3</span></li>
              <li><span><img src="{ic("Reinforced_Chest.png")}"> 8&times;4 Container Storage &amp; Sorting</span> <span>PAGE 3</span></li>
              <li><span><img src="{ic("Deer_Stew.png")}"> 3-Slot Vitality &amp; Sustenance Rations</span> <span>PAGE 3</span></li>
              <li><span><img src="{ic("Coins.png")}"> Recruitable Heritages (Viking, Dvergr, Skeleton)</span> <span>PAGE 3</span></li>
            </ul>
          </div>

          <div class="toc-card">
            <h4><img src="{ic("Cultivator.png")}" alt="Icon"> Part II: 36-Feature Map &amp; Separation Matrix</h4>
            <ul class="toc-list">
              <li><span><img src="{ic("Cultivator.png")}"> The 36-Feature Icon Catalog (F01–F36)</span> <span>PAGE 4</span></li>
              <li><span><img src="{ic("Workbench.png")}"> Master Worker &times; Feature Capability Matrix</span> <span>PAGE 5</span></li>
              <li><span><img src="{ic("Cart.png")}"> Domain Classification &amp; Operational Hierarchy</span> <span>PAGE 5</span></li>
            </ul>
          </div>

          <div class="toc-card">
            <h4><img src="{ic("Bronze_Axe.png")}" alt="Icon"> Part III: Harvesting &amp; Extraction Roles</h4>
            <ul class="toc-list">
              <li><span><img src="{ic("Stone_Axe.png")}"> The Lumberjack (Forester &amp; Arborist)</span> <span>PAGE 6</span></li>
              <li><span><img src="{ic("Antler_Pickaxe.png")}"> The Miner (Quarryman &amp; Excavator)</span> <span>PAGE 7</span></li>
              <li><span><img src="{ic("Cultivator.png")}"> The Farmer (Husbandman &amp; Cultivator)</span> <span>PAGE 8</span></li>
              <li><span><img src="{ic("Raspberries.png")}"> The Gatherer (Forager &amp; Apiarist)</span> <span>PAGE 9</span></li>
              <li><span><img src="{ic("Finewood_Bow.png")}"> The Hunter (Ranger &amp; Marksman)</span> <span>PAGE 10</span></li>
            </ul>
          </div>

          <div class="toc-card">
            <h4><img src="{ic("Banded_Shield.png")}" alt="Icon"> Part IV: Base Production &amp; Defense Roles</h4>
            <ul class="toc-list">
              <li><span><img src="{ic("Banded_Shield.png")}"> The Guard (Perimeter Sentinel &amp; Warden)</span> <span>PAGE 11</span></li>
              <li><span><img src="{ic("Smelter.png")}"> The Smelter (Furnace Master &amp; Pyrotechnic)</span> <span>PAGE 12</span></li>
              <li><span><img src="{ic("Windmill.png")}"> The Base Tender (Camp Steward &amp; Miller)</span> <span>PAGE 13</span></li>
              <li><span><img src="{ic("Cauldron.png")}"> The Cook (Culinary Artisan &amp; Brewer)</span> <span>PAGE 14</span></li>
              <li><span><img src="{ic("Cart.png")}"> The Hauler (Logistics Operator &amp; Packmaster)</span> <span>PAGE 15</span></li>
              <li><span><img src="{ic("Hammer.png")}"> The Craftsman (Artificer &amp; Carpenter)</span> <span>PAGE 16</span></li>
            </ul>
          </div>
        </div>
      </div>

      <div class="stone-tablet panel-rec" style="margin-top: 6px;">
        <h3>{hugin()} The Five Pillars of Autonomous Settlement Governance</h3>
        <ul style="margin-left: 15px;">
          <li><strong>1. Secure the Perimeter:</strong> Station Guards at palisade choke points equipped with both Hunter-grade Bows and Bronze/Iron shields for tactical CQC response.</li>
          <li><strong>2. Protect Worker Vitality:</strong> Ensure 3-slot nutrition by stocking camp larders with Honey, Cooked Meats, and Jam; well-fed workers work 25% faster.</li>
          <li><strong>3. Explicit Storage Bracket Tagging:</strong> Fasten signs above strongboxes tagged with <code>[Wood]</code>, <code>[Ore]</code>, <code>[Food]</code>, <code>[Seeds]</code>, and <code>[Mead]</code> for zero-clutter routing.</li>
          <li><strong>4. Tool Proximity &amp; Integrity:</strong> Maintain an upgraded Workbench and Forge within 15m of worksites to prevent idle tool-durability walking pauses.</li>
          <li><strong>5. Plan for Material Abundance:</strong> Clear open warehouse zones for automated 50x resource piles (wood, stone, coal) when strongboxes reach full capacity.</li>
        </ul>
      </div>

      <div class="stone-tablet panel-now" style="margin-top: 5px;">
        <h3>Part V: Settlement Logistics, Diagnostics &amp; Specifications</h3>
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
print("Building Page 3: Foundations & Architecture...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Workbench.png")}" class="chapter-icon" alt="Foundations"> Part I: Core Controls &amp; Foundations</h2>
        <span class="role-pill">Spatial Logic &bull; Vitality</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">Native 8&times;4 Grid (Equipped Cyan Borders):</span>
          {slot("Stone_Axe.png", "Axe", True)}
          {slot("Banded_Shield.png", "Shield", True)}
          {slot("Iron_Scale_Mail.png", "Armor", True)}
          {slot("Wood_Arrow.png", "Arrows", True)}
          {slot("Wood.png", "Cargo", False)}
          {slot("Stone.png", "Cargo", False)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">3-Slot Stomach:</span>
          {slot("Deer_Stew.png", "Stew", False)}
          {slot("Raw_Meat.png", "Meat", False)}
          {slot("Queens_Jam.png", "Jam", False)}
        </div>
      </div>

      <div class="worker-grid-6">
        <div class="stone-tablet panel-now">
          <h3>1. Direct Controls &amp; In-World Directives</h3>
          <ul>
            <li><strong>F8 (Settlement Hall UI):</strong> Opens management dashboard. Unlocks cursor, pauses player attacks/camera, and provides access to Recruitment, Roster Management, and Telemetry.</li>
            <li><strong>[E] (Tap on Worker):</strong> Instant toggle between Follow Me and Station at Post. When stationed, anchors location and resumes last assigned role.</li>
            <li><strong>[Shift + E] (Tap on Worker):</strong> Quick-cycles through all allowed roles in the world, accompanied by a speech bubble announcement.</li>
            <li><strong>[Hold E] (&gt;0.38s):</strong> Opens the worker's native 8&times;4 (32-slot) Container Inventory. Right-clicking equipment inside equips/unequips it immediately with a cyan border.</li>
            <li><strong>[Alt + Hold E] (&gt;0.38s):</strong> Opens F8 Settlement Hall focused directly onto the targeted worker.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-routine">
          <h3>2. Dual-Anchor Spatial Post Architecture</h3>
          <p>Workers maintain two distinct spatial coordinates saved permanently in their ZDO (Zone Data Object):</p>
          <ul>
            <li><strong>Camp Post (HomePosition):</strong> The domestic home base where workers deposit harvested goods into chests, withdraw fuels/raw meats, eat meals, and retreat during raids.</li>
            <li><strong>Work Post (WorkPostPosition):</strong> The remote job center where tasks like tree felling, mining, and patrols occur. Workers commute to their Work Post to perform labor, then hike back to their Camp Post when full!</li>
            <li><strong>Unstuck Hop Mechanics:</strong> If a worker remains trapped on geometry for &gt;12s, the navigation engine executes a 1.5m physics hop toward the active target.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-future">
          <h3>3. Native 8&times;4 Container Storage &amp; Auto-Repair Routine</h3>
          <ul>
            <li><strong>32-Slot Container Backing:</strong> Each worker uses a real Valheim <code>Inventory</code> container identical to a wooden chest, allowing full compatibility with inventory sorting mods.</li>
            <li><strong>Protected Gear Retention [F21]:</strong> Equipped weapons, armor, and primary tools are flagged with an immutable retention tag so workers never deposit their tools into storage chests!</li>
            <li><strong>Proactive Field Repair [F20]:</strong> When active tool durability drops below 15%, the worker pauses harvesting, routes to the nearest upgraded Workbench/Forge, and repairs all items before resuming.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-synergy">
          <h3>4. 3-Slot Sustenance &amp; Metabolic Vitality</h3>
          <ul>
            <li><strong>3-Slot Nutrition Simulation:</strong> Workers consume up to 3 distinct food items to gain maximum health, stamina, and passive health regeneration. They eat from carried rations or withdraw food from camp pantry chests.</li>
            <li><strong>Hunger Degradation:</strong> Starving workers suffer a 40% movement penalty and cannot sprint or perform heavy felling/mining strikes.</li>
            <li><strong>Sustenance Synergy:</strong> Pair Cooks with Hunters/Farmers to automatically stock <code>[Food]</code> chests with Deer Stew, Sausages, and Honey!</li>
          </ul>
        </div>

        <div class="stone-tablet panel-rec">
          <h3>5. Recruitable Heritages &amp; Racial Traits</h3>
          <ul>
            <li><strong>Viking Worker:</strong> 50 Coins cost, 150 Base HP, full custom male/female cosmetics, all 11 roles available. Well-rounded and resilient.</li>
            <li><strong>Dvergr Laborer:</strong> 75 Coins cost, 200 Base HP, spawns with Tier 2 Bronze gear, immune to minor stagger. Exceptional Miners and Smelters.</li>
            <li><strong>Skeleton Servant:</strong> 20 Bone Fragments, 80 HP, low-cost utility for Hauler, Guard, and basic chores. Immune to poison and cold weather.</li>
          </ul>
        </div>
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
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Sign.png")}" class="chapter-icon" alt="Catalog"> Part II: The 36-Feature Visual Catalog (F01–F36)</h2>
          <span class="role-pill">Capability Taxonomy</span>
        </div>
        <p style="font-size: 7.5pt; color: #C2B39E; margin-bottom: 2px;">
          Every worker behavior in ValheimWorkers v0.3.0 is indexed into a standardized 36-feature taxonomy across 8 operational domains:
        </p>
      </div>

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
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Banded_Shield.png")}" class="chapter-icon" alt="Matrix"> Part II: Worker &times; Feature Separation Matrix</h2>
          <span class="role-pill">Master Separation Grid</span>
        </div>
        <p style="font-size: 7.5pt; color: #C2B39E; margin-bottom: 2px;">
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
      </div>

      <div class="stone-tablet panel-now" style="margin-top: 4px;">
        <h3>{hugin()} UNIVERSAL CORE CAPABILITIES</h3>
        <p style="font-size: 7.6pt; line-height: 1.30;">
          <strong>Universal Features:</strong> All workers possess <strong>[F21]</strong> Protected Gear Retention, <strong>[F31]</strong> Smart Storage Sorting, <strong>[F32]</strong> Sign-Based Tag Routing, <strong>[F34]</strong> Longship Boarding, <strong>[F35]</strong> Night Torch Bearing, and <strong>[F36]</strong> Tavern Rest &amp; Seating.
        </p>
      </div>

      <div class="stone-tablet panel-routine" style="margin-top: 4px;">
        <h3>Operational Domain &amp; Hierarchy Architecture</h3>
        <table>
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
            <tr>
              <td><strong>1. Forestry &amp; Felling</strong></td>
              <td>Lumberjack</td>
              <td>Stone, Bronze, Iron Axes</td>
              <td>Wood, Core Wood, Fine Wood, Stumps</td>
              <td><code>[Wood]</code></td>
            </tr>
            <tr>
              <td><strong>2. Mining &amp; Quarrying</strong></td>
              <td>Miner</td>
              <td>Pickaxes (Antler, Bronze, Iron), Hoe</td>
              <td>Stone, Flint, Copper, Tin, Iron, Silver</td>
              <td><code>[Stone] [Ore]</code></td>
            </tr>
            <tr>
              <td><strong>3. Agriculture &amp; Fauna</strong></td>
              <td>Farmer</td>
              <td>Bronze Cultivator</td>
              <td>Carrots, Turnips, Onions, Barley, Tamed Boars</td>
              <td><code>[Seeds] [Food]</code></td>
            </tr>
            <tr>
              <td><strong>4. Wild Foraging &amp; Apiary</strong></td>
              <td>Gatherer</td>
              <td>Bare Hands, Dagger</td>
              <td>Berries, Mushrooms, Thistle, Honey, Sap Extractors</td>
              <td><code>[Berries] [Sap]</code></td>
            </tr>
            <tr>
              <td><strong>5. Wilderness Combat</strong></td>
              <td>Hunter, Guard</td>
              <td>Bows (Finewood, Huntsman), Sword, Shield</td>
              <td>Raw Meat, Deer Pelts, Feathers, Trophy Drops</td>
              <td><code>[Meat] [Pelt]</code></td>
            </tr>
            <tr>
              <td><strong>6. Smelting &amp; Refining</strong></td>
              <td>Smelter, Base Tender</td>
              <td>Fuel &amp; Ores</td>
              <td>Kiln (Coal), Smelter (Ingots), Windmill (Flour)</td>
              <td><code>[Ore] [Bars]</code></td>
            </tr>
            <tr>
              <td><strong>7. Culinary Arts</strong></td>
              <td>Cook</td>
              <td>Cooking Spit, Cauldron, Fermenter</td>
              <td>Roast Meat, Deer Stew, Sausages, Aged Meads</td>
              <td><code>[Food] [Mead]</code></td>
            </tr>
            <tr>
              <td><strong>8. Logistics &amp; Freight</strong></td>
              <td>Hauler, Craftsman</td>
              <td>Hammer, Vagon Carts, Strongboxes</td>
              <td>50x Wood/Stone Piles, Sign Routing, Cart Logistics</td>
              <td><code>[All Tags]</code></td>
            </tr>
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

# ==================== PAGES 6-16: 11 WORKER CHAPTERS ====================
print("Building Pages 6-16 (11 Worker Chapters)...")

# 1. LUMBERJACK (Page 6)
parts.append(make_worker_chapter(
    6, "Part III", "The Lumberjack", "Forester &amp; Arborist", "Forestry &bull; Timber",
    "Stone_Axe.png",
    [("Bronze_Axe.png", "Axe", True), ("Torch.png", "Torch", False), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Cargo", False)],
    [("Wood_Pile.png", "Pile"), ("Core_Wood.png", "Core"), ("Fine_Wood.png", "Fine"), ("Beech_Seeds.png", "Seeds"), ("Resin.png", "Resin")],
    f"""
    <h3>{feat("F01", "Forestry", "Stone_Axe.png")} {feat("F02", "Stump Clear", "Wood.png")} {feat("F05", "Overflow", "Wood_Pile.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F01] Selective Tree Felling:</strong> Scans within a 30m radius of Work Post for mature Beech, Birch, and Pine trees matching axe tool tier.</li>
      <li><strong>[F02] Immediate Stump &amp; Log Eradication:</strong> Prioritizes fallen log pieces and tree stumps to keep clearings pristine and prevent path obstructions.</li>
      <li><strong>[F05] Physical Overflow Piles:</strong> Constructs 50x <code>wood_pile</code> pieces on open soil when camp chests marked <code>[Wood]</code> are full.</li>
      <li><strong>[F31] Smart Sorting:</strong> Stacks regular Wood, Core Wood, and Fine Wood into existing partial stacks before creating new chest slots.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Scan &amp; Filter:</strong> Scans 30m Work Post radius for mature trees, checking equipped axe damage tier against wood hardness.</li>
      <li><strong>2. Tool Integrity Check:</strong> Verifies axe durability &gt; 15%; if depleted, routes to camp Workbench for immediate repair.</li>
      <li><strong>3. Tactical Felling:</strong> Executes felling strikes; immediately targets downed log sections and roots to eliminate hazard geometry.</li>
      <li><strong>4. Cart &amp; Chest Delivery:</strong> Transports timber to Camp Post; unloads into <code>[Wood]</code> chests or constructs 50x wood piles.</li>
      <li><strong>5. Rest &amp; Nourishment:</strong> Withdraws food from camp larders when hungry; rests near fires when darkness falls.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>[F03] Automated Tree Nursery:</strong> Proactive sapling plantation in designated reforestation perimeters away from buildings.</li>
      <li><strong>[F04] Pine Resin Tapping:</strong> Extracting cones and seeds during felling and storing them for architectural expansion.</li>
      <li><strong>Directional Wedge Felling:</strong> Felling physics prediction to prevent heavy timber from crushing nearby player fences.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Smelter Feeder:</strong> Constantly supplies the Charcoal Kiln with 25x wood batches to guarantee uninterrupted coal production.</li>
      <li><strong>Base Tender Coordination:</strong> Leaves campfire firewood stashes near living quarters for automatic sconce/hearth refueling.</li>
      <li><strong>Craftsman Handoff:</strong> Supplies fine wood and core wood directly into carpenter workshops for furniture construction.</li>
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
      <li><strong>Tree Fall Damage:</strong> Workers take minor physics damage from falling trunks; equip an Iron Helmet to mitigate felling accidents.</li>
    </ul>
    """
))

# 2. MINER (Page 7)
parts.append(make_worker_chapter(
    7, "Part IV", "The Miner", "Quarryman &amp; Excavator", "Mining &bull; Extraction",
    "Antler_Pickaxe.png",
    [("Bronze_Pickaxe.png", "Pickaxe", True), ("Hoe.png", "Hoe", True), ("Torch.png", "Torch", False), ("Stone.png", "Cargo", False)],
    [("Stone_Pile.png", "Pile"), ("Copper_Ore.png", "Copper"), ("Tin_Ore.png", "Tin"), ("Iron_Scrap.png", "Iron"), ("Flint.png", "Flint")],
    f"""
    <h3>{feat("F06", "Quarrying", "Antler_Pickaxe.png")} {feat("F07", "Deep Veins", "Copper_Ore.png")} {feat("F08", "Hoe Leveling", "Hoe.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F06] Surface Quarrying:</strong> Shatters surface rocks, loose boulders, and flint nodes with pickaxe.</li>
      <li><strong>[F07] Deep Vein Excavation:</strong> Extracts Copper, Tin, Iron Scrap, and Silver veins using tool tier verification (Antler, Bronze, Iron).</li>
      <li><strong>[F08] Even the Field (Hoe Leveling):</strong> Equips a Hoe to level and smooth jagged quarry terrain after shattering rock faces, eliminating pit traps.</li>
      <li><strong>[F09] Subterranean Vein Exposure:</strong> Lowers overlying soil layers to expose subterranean chunks of buried <code>MineRock5</code> veins.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Constructs 50x <code>stone_pile</code> pieces when camp storage chests cannot accept more stone.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Geode Prospecting:</strong> Locates rock nodes or ore veins within 25m of Work Post; evaluates pickaxe tier vs hardness.</li>
      <li><strong>2. Overburden Excavation:</strong> Chips away topsoil and surrounding bedrock to isolate full ore colliders.</li>
      <li><strong>3. Hoe Planing Pass:</strong> Swaps to Hoe to level pit bottoms, creating flat footing and ramped cart exit paths.</li>
      <li><strong>4. Ore Consolidation:</strong> Sweeps ore chunks and stone; deposits metals into <code>[Ore]</code> chests and stone into masonry piles.</li>
      <li><strong>5. Forge Repair Run:</strong> Navigates to nearest Forge when pickaxe durability drops below 10%.</li>
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
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Foundry Feeder:</strong> Direct conduit of raw copper, tin, and scrap iron to the Smelter quadrant.</li>
      <li><strong>Masonry Stockpiler:</strong> Provides vast 50x stone piles to the Craftsman for defensive stone wall construction.</li>
      <li><strong>Hauler Vagon Loading:</strong> Drops heavy ore stacks into parked carts for unified player caravan hauling.</li>
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
      <li><strong>Bedrock Limit:</strong> Cannot excavate below the world bedrock floor (-8m); miner will automatically cease downward digging.</li>
    </ul>
    """
))

# 3. FARMER (Page 8)
parts.append(make_worker_chapter(
    8, "Part V", "The Farmer", "Husbandman &amp; Cultivator", "Agriculture &bull; Sustenance",
    "Cultivator.png",
    [("Cultivator.png", "Cultivator", True), ("Rag_Tunic.png", "Tunic", True), ("Carrot_Seeds.png", "Seeds", False), ("Carrot.png", "Yield", False)],
    [("Carrot.png", "Carrot"), ("Turnip.png", "Turnip"), ("Onion.png", "Onion"), ("Barley.png", "Barley"), ("Boar.png", "Tamed")],
    f"""
    <h3>{feat("F10", "Harvesting", "Carrot.png")} {feat("F11", "Soil Prep", "Cultivator.png")} {feat("F12", "Proactive Sowing", "Carrot_Seeds.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F10] Crop Harvesting:</strong> Harvests ripe carrots, turnips, onions, barley, and flax within garden perimeter.</li>
      <li><strong>[F11] Soil Cultivation:</strong> Prepares fresh cultivated arable land with Cultivator when existing plots are fully planted.</li>
      <li><strong>[F12] Proactive Sowing (Every Seed):</strong> Withdraws vegetable/grain seeds from camp chests and proactively sows every available seed across tilled soil.</li>
      <li><strong>[F13] Automatic Seed Expansion:</strong> Maintains perpetual cycle by planting seed crops to yield 3x seeds per plant.</li>
      <li><strong>[F16] Tamed Livestock Feeding:</strong> Scans for hungry tamed Boars, Lox, Chickens, and Wolves, dropping crops or meats into their pens.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Garden Scan:</strong> Evaluates cultivated ground; identifies 100% mature crops ready for harvest.</li>
      <li><strong>2. Plucking &amp; Separation:</strong> Collects produce and seeds, keeping inventory sorted by species.</li>
      <li><strong>3. Cultivation Pass:</strong> Verifies tilled soil boundary; expands plot using Cultivator if seeds exceed prepared ground.</li>
      <li><strong>4. 1:3 Seed Propagation:</strong> Plants seed-carrots/turnips to regenerate seed stock before planting standard food crops.</li>
      <li><strong>5. Livestock Tending:</strong> Carries leftover turnips/carrots to tamed boar troughs; stores surplus in <code>[Food]</code> chests.</li>
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
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Kitchen Conduit:</strong> Supplies fresh vegetables, barley, and honey to the Cook for cauldron stews.</li>
      <li><strong>Windmill Supplier:</strong> Transfers harvested barley to the Base Tender for automated flour milling.</li>
      <li><strong>Linen Production:</strong> Supplies raw flax directly to spinning wheels for craftsman armor weaving.</li>
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
      <li><strong>Spacing Bounds:</strong> Crops require 0.5m spacing to avoid becoming unhealthy; AI automatically respects spacing constraints.</li>
    </ul>
    """
))

# 4. GATHERER (Page 9)
parts.append(make_worker_chapter(
    9, "Part VI", "The Gatherer", "Forager &amp; Apiarist", "Harvesting &bull; Foraging",
    "Raspberries.png",
    [("Rag_Tunic.png", "Tunic", True), ("Torch.png", "Torch", False), ("Raspberries.png", "Berries", False), ("Mushroom.png", "Fungi", False)],
    [("Beehive.png", "Hive"), ("Sap_Extractor.png", "Sap"), ("Honey.png", "Honey"), ("Blueberries.png", "Blueberry"), ("Thistle.png", "Thistle")],
    f"""
    <h3>{feat("F14", "Foraging", "Raspberries.png")} {feat("F15", "Apiary &amp; Sap", "Beehive.png")} {feat("F05", "Overflow", "Wood_Pile.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F14] Wild Flora Foraging:</strong> Collects wild raspberries, blueberries, yellow/red mushrooms, thistle, dandelion, and cloudberries.</li>
      <li><strong>[F15] Apiary &amp; Sap Extraction:</strong> Harvests honeycombs from camp beehives and extracts sap from Yggdrasil sap extractors.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Builds bone and resource piles when foraging containers are filled to capacity.</li>
      <li><strong>[F32] Pantry Stashing:</strong> Delivers all collected berries and honey into pantry chests marked <code>[Berries]</code> or <code>[Food]</code>.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Wild Bush Patrol:</strong> Paths along perimeter berry bushes and thistle spawners, harvesting ripe fruit.</li>
      <li><strong>2. Apiary Circuit:</strong> Visits all camp beehives, gathering accumulated honey (up to 4 honey per hive).</li>
      <li><strong>3. Sap Extractor Tap:</strong> Empties Yggdrasil sap extractors in the Mistlands or designated settlement roots.</li>
      <li><strong>4. Pantry Delivery:</strong> Delivers sweet ingredients directly to the kitchen cauldrons for mead base brewing.</li>
      <li><strong>5. Night Stash:</strong> Returns to Camp Post before midnight; equips torch if ambient lighting is low.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Domestic Berry Bush Planting:</strong> Transplanting wild raspberry and blueberry bushes directly into camp orchards.</li>
      <li><strong>Mushroom Spore Beds:</strong> Cultivating underground yellow mushroom beds in damp cellars.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Mead Brewer's Muse:</strong> Provides honey, blueberries, and thistle to the Cook for poison and frost resistance potions.</li>
      <li><strong>Base Tender Support:</strong> Provides dandelion and resin for torch refueling and yellow banner tailoring.</li>
      <li><strong>Piglet Tamer:</strong> Stocks berries in boar pens to accelerate breeding cycles.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Group 4–8 Beehives around a central courtyard pole; ensure bees remain "Happy" with open sky above.</li>
      <li>Set Work Post near a dense berry thicket for maximum yield efficiency.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Respawn Timers:</strong> Wild berries take 300 minutes (5 hours in-game) to respawn; set a wide patrol radius.</li>
      <li><strong>Bee Stings:</strong> Workers have immunity to friendly bee damage and will not flee from active hives.</li>
    </ul>
    """
))

# 5. HUNTER (Page 10)
parts.append(make_worker_chapter(
    10, "Part VII", "The Hunter", "Ranger &amp; Marksman", "Combat &bull; Hunting",
    "Finewood_Bow.png",
    [("Finewood_Bow.png", "Bow", True), ("Wood_Arrow.png", "Arrows", True), ("Rag_Tunic.png", "Tunic", True), ("Raw_Meat.png", "Pelt", False)],
    [("Deer.png", "Game"), ("Wolf.png", "Wolf"), ("Boar.png", "Boar"), ("Fire_Arrow.png", "Arrow"), ("Workbench.png", "Workbench")],
    f"""
    <h3>{feat("F17", "Wilderness Stalking", "Deer.png")} {feat("F18", "Parabolic Bow", "Finewood_Bow.png")} {feat("F20", "Workbench Repair", "Workbench.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F17] Wilderness Stalking:</strong> Sneak-stalks wild game (deer, boar, necks, birds) with reduced detection radius.</li>
      <li><strong>[F18] Hunter-Grade Archery:</strong> Long-range parabolic archery with lead shot calculations for moving targets.</li>
      <li><strong>[F20] Field Gear Maintenance:</strong> Visits camp workbench to repair worn bow and restock arrows from supply chests.</li>
      <li><strong>Trophy &amp; Hide Skinning:</strong> Drops raw meat, deer hide, feathers, and trophies into chests labeled <code>[Meat]</code> and <code>[Pelt]</code>.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Wild Game Tracking:</strong> Scans 40m radius for deer, boar, and birds; calculates downwind sneak approach.</li>
      <li><strong>2. Parabolic Lead Shot:</strong> Draws bowstring; accounts for arrow drop and target velocity, firing precision shot.</li>
      <li><strong>3. Yield Harvesting:</strong> Collects meat, hides, entrails, and feathers from fallen targets.</li>
      <li><strong>4. Arrow Restock Run:</strong> Returns to camp; pulls wooden/flint arrows from storage or crafts basic ammo.</li>
      <li><strong>5. Meat Delivery:</strong> Unloads venison and boar meat into kitchen larder chests for the Cook.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Trophy Mounting:</strong> Hanging boss and rare creature heads onto camp longhouse walls.</li>
      <li><strong>Hunting Blinds &amp; Tree Stands:</strong> Perching on elevated platforms for zero-scent hunting coverage.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Kitchen Provider:</strong> Primary supplier of raw meat, neck tails, and serpent meat to culinary stations.</li>
      <li><strong>Tannery Conduit:</strong> Delivers deer hides and wolf pelts to the Craftsman for armor and rug fabrication.</li>
      <li><strong>Guard Auxillary:</strong> Snipes hostile raid creatures from long distance before they reach the main gates.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Equip Hunter with Finewood or Huntsman Bow and at least 100 Wood or Fire Arrows.</li>
      <li>Place meat drop chest labeled <code>[Meat]</code> near the kitchen entrance for rapid handoffs.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Arrow Depletion:</strong> If out of arrows, hunter will temporarily retreat to camp to search chests for projectiles.</li>
      <li><strong>Deer Flee Radius:</strong> Deer are easily spooked; equip stealth gear (Troll armor) to increase hunting efficiency.</li>
    </ul>
    """
))

# 6. GUARD (Page 11)
parts.append(make_worker_chapter(
    11, "Part VIII", "The Guard", "Perimeter Sentinel &amp; Warden", "Defense &bull; Martial",
    "Banded_Shield.png",
    [("Huntsman_Bow.png", "Bow", True), ("Iron_Sword.png", "Sword", True), ("Banded_Shield.png", "Shield", True), ("Iron_Helmet.png", "Helm", True)],
    [("Iron_Sword.png", "Melee"), ("Fire_Arrow.png", "Bow"), ("Workbench.png", "Repair"), ("Valkyrie.png", "Horn"), ("Campfire.png", "Post")],
    f"""
    <h3>{feat("F18", "Hunter Archery", "Huntsman_Bow.png")} {feat("F19", "CQC Swap", "Iron_Sword.png")} {feat("F22", "War Horn", "Valkyrie.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F18] Average Hunter-Grade Archery:</strong> Engages approaching enemies at range with bow and arrow using lead targeting.</li>
      <li><strong>[F19] Tactical CQC Swap:</strong> Automatically draws melee weapon (sword/axe/mace) and shield when enemies enter &lt;=9.5m.</li>
      <li><strong>[F22] Raid War Horn Alert:</strong> Sounds War Horn to rally defenders when camp threat score exceeds critical threshold.</li>
      <li><strong>[F20] Autonomous Armor Repair:</strong> Visits camp workbench to maintain weapon and shield durability above 20%.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Perimeter Sentry:</strong> Patrols assigned wall parapets and gatehouse waypoints with bow drawn.</li>
      <li><strong>2. Long-Range Volley:</strong> Snipes approaching Greydwarfs, Trolls, or Skeletons with parabolic arrow shots.</li>
      <li><strong>3. Tactical Weapon Swap:</strong> Instantly stows bow and unsheathes sword and shield when enemies breach 9.5m.</li>
      <li><strong>4. Shield Block &amp; Strike:</strong> Executes parry timing against melee swings, following up with sword combos.</li>
      <li><strong>5. War Horn Alarm:</strong> Sounds settlement alarm horn if more than 3 high-tier threats attack simultaneously.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Ballista Battery Manning:</strong> Loading and operating automated defensive siege ballistas along outer curtain walls.</li>
      <li><strong>Shield Wall Formation:</strong> Locking shields alongside allied Guards for 50% reduced group damage.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Civilian Protector:</strong> Shields Farmers, Lumberjacks, and Miners from wandering forest predators.</li>
      <li><strong>Craftsman Armor Patron:</strong> Receives upgraded shields, iron helmets, and fine swords from the armory.</li>
      <li><strong>Cook Ration Priority:</strong> Consumes high-HP stews and healing meads to sustain frontline defensive integrity.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Equip Guard with both a ranged weapon (Bow + Arrows) and a melee loadout (Sword + Shield) for full CQC flexibility.</li>
      <li>Construct elevated gatehouse towers with open firing slits to grant Guards high-ground defense advantage.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Aggro Leashing:</strong> Guard will pursue enemies up to 35m before leashing back to their assigned post to prevent luring traps.</li>
      <li><strong>Troll Encounters:</strong> Guards maintain distance against log-swinging Trolls, preferring ranged arrow volleys.</li>
    </ul>
    """
))

# 7. SMELTER (Page 12)
parts.append(make_worker_chapter(
    12, "Part IX", "The Smelter", "Furnace Master &amp; Pyrotechnic", "Refining &bull; Metallurgy",
    "Smelter.png",
    [("Torch.png", "Torch", False), ("Rag_Tunic.png", "Tunic", True), ("Coal.png", "Fuel", False), ("Copper_Ore.png", "Ore", False)],
    [("Smelter.png", "Smelter"), ("Blast_Furnace.png", "Blast"), ("Charcoal_Kiln.png", "Kiln"), ("Coal_Pile.png", "Pile"), ("Bronze.png", "Ingots")],
    f"""
    <h3>{feat("F27", "Kiln Stoking", "Charcoal_Kiln.png")} {feat("F28", "Smelter Loading", "Smelter.png")} {feat("F05", "Overflow", "Coal_Pile.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F27] Charcoal Kiln Stoking:</strong> Feeds regular wood into kilns to maintain a steady stockpile of coal.</li>
      <li><strong>[F28] Smelter &amp; Blast Furnace Loading:</strong> Loads ores and coal into smelters, and extracts purified metal ingots (copper, tin, bronze, iron, silver).</li>
      <li><strong>[F05] Coal &amp; Ore Overflow Piles:</strong> Constructs 50x <code>coal_pile</code> pieces when fuel storage chests reach capacity.</li>
      <li><strong>[F32] Ingot Routing:</strong> Transports hot ingots into chests marked <code>[Ore]</code> or <code>[Bars]</code> near the main forge.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Inventory Audit:</strong> Checks nearby chests for wood, coal, and unrefined ores (copper, tin, iron, silver).</li>
      <li><strong>2. Kiln Combustion:</strong> Loads up to 25 wood into Charcoal Kilns; collects expelled coal chunks.</li>
      <li><strong>3. Furnace Stoking:</strong> Feeds 20 coal into smelter fuel hoppers; inserts up to 10 matching ore chunks.</li>
      <li><strong>4. Ingot Retrieval:</strong> Gathers purified metal bars as they drop from smelter exit spouts.</li>
      <li><strong>5. Secure Stash:</strong> Delivers ingots into forge chests; erects coal stacks in foundry yards if chests fill.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Automated Bronze Alloy Smelting:</strong> Direct 2:1 copper-to-tin blending inside specialized multi-crucible forges.</li>
      <li><strong>Flammetal Processing:</strong> Operating Ashland blast furnaces with insulated obsidian tongs.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Miner Consumer:</strong> Direct recipient of raw copper, tin, iron scrap, and silver excavated from quarries.</li>
      <li><strong>Lumberjack Fuel Patron:</strong> Consumes bulk timber harvested by loggers to keep kilns roaring continuously.</li>
      <li><strong>Craftsman Foundry Supplier:</strong> Supplies pure metal ingots directly to the blacksmith for weapon/armor smithing.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>Cluster Kilns and Smelters within 8m of each other with a central chest labeled <code>[Ore]</code> and <code>[Coal]</code>.</li>
      <li>Assign Dvergr Laborers as Smelters; their natural stagger immunity prevents forge bump interrupts.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Smoke Accumulation:</strong> Ensure smelting yards have open roofs or ventilation chimneys to prevent indoor smoke suffocation.</li>
      <li><strong>Ore Mixing:</strong> Smelters will not mix incompatible ores in the same furnace run; AI respects single-ore processing batches.</li>
    </ul>
    """
))

# 8. BASE TENDER (Page 13)
parts.append(make_worker_chapter(
    13, "Part X", "The Base Tender", "Camp Steward &amp; Miller", "Camp Maintenance &bull; Utility",
    "Hearth.png",
    [("Torch.png", "Torch", True), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Fuel", False), ("Barley.png", "Grain", False)],
    [("Windmill.png", "Mill"), ("Spinning_Wheel.png", "Wheel"), ("Hearth.png", "Hearth"), ("Barley_Flour.png", "Flour"), ("Linen_Thread.png", "Linen")],
    f"""
    <h3>{feat("F29", "Windmill", "Windmill.png")} {feat("F30", "Hearths", "Hearth.png")} {feat("F05", "Overflow", "Wood_Pile.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F29] Windmill &amp; Spinning Wheel Tending:</strong> Loads harvested Barley into Windmills to grind barley flour, and feeds Flax into Spinning Wheels for linen thread.</li>
      <li><strong>[F30] Hearth, Fireplace &amp; Sconce Refueling:</strong> Keeps campfires, stone hearths, hanging braziers, wall sconces, and standing torches burning by refueling wood, resin, and coal.</li>
      <li><strong>Camp Comfort Maintenance:</strong> Ensures resting player always enjoys maximum comfort bonus upon returning to base.</li>
      <li><strong>[F05] Overflow Stacks:</strong> Builds wood and coal piles when fuel storage is full.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Illumination Sweep:</strong> Inspects all campfires, hearths, braziers, and wall sconces within 30m radius.</li>
      <li><strong>2. Fuel Stoking Pass:</strong> Deposits resin into sconces, coal into braziers, and wood into hearths/cooking fires.</li>
      <li><strong>3. Windmill Processing:</strong> Loads up to 50 harvested barley into windmills; retrieves ground flour bags.</li>
      <li><strong>4. Spinning Wheel Tending:</strong> Loads raw flax onto spinning wheels; collects spun linen thread for armories.</li>
      <li><strong>5. Bed &amp; Hearth Tidying:</strong> Sweeps ashes and organizes residential longhouses for maximum comfort duration.</li>
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
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Farmer Conduit:</strong> Receives raw barley and flax from the fields, refining them into culinary flour and textile linen.</li>
      <li><strong>Cook Support:</strong> Delivers barley flour directly to bakery ovens for lox meat pie and bread crafting.</li>
      <li><strong>Night Watch Comfort:</strong> Keeps guard towers illuminated with bright sconces to prevent night-spawn creature surprises.</li>
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
      <li><strong>Wind Conditions:</strong> Windmill milling speed depends on local wind speed; Base Tender will queue grain during calm weather.</li>
    </ul>
    """
))

# 9. COOK (Page 14)
parts.append(make_worker_chapter(
    14, "Part XI", "The Cook", "Culinary Artisan &amp; Brewer", "Kitchen &bull; Alchemy",
    "Cauldron.png",
    [("Bronze_Axe.png", "Knife", False), ("Rag_Tunic.png", "Tunic", True), ("Honey.png", "Honey", False), ("Raw_Meat.png", "Meat", False)],
    [("Cooking_Station.png", "Spit"), ("Cauldron.png", "Cauldron"), ("Fermenter.png", "Fermenter"), ("Deer_Stew.png", "Stew"), ("Medium_Healing_Mead.png", "Mead")],
    f"""
    <h3>{feat("F23", "Spit Roasting", "Cooking_Station.png")} {feat("F24", "Cauldron Stew", "Cauldron.png")} {feat("F25", "Fermenter", "Fermenter.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F23] Spit Roasting &amp; Stew Crafting:</strong> Roasts meats on cooking spits without burning, and simmers deer stew, wolf skewers, and sausages in Cauldrons.</li>
      <li><strong>[F24] Mead Base Alchemy:</strong> Mixes honey and herbal ingredients to prepare health, stamina, poison resist, and frost resist mead bases.</li>
      <li><strong>[F25] Fermenter Tapping:</strong> Inspects camp Fermenters and automatically taps aged mead potions into pantry chests once fermentation completes.</li>
      <li><strong>[F26] Pantry Routing:</strong> Places cooked food items directly into chests marked with <code>[Food]</code>.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Pantry Audit:</strong> Scans <code>[Food]</code> chests for raw ingredients (meats, honey, berries, mushrooms, flour).</li>
      <li><strong>2. Spit Roasting:</strong> Mounts meats onto iron spits; times removal precisely upon sizzle sound to avoid burning.</li>
      <li><strong>3. Cauldron Stew Boiling:</strong> Combines ingredients into hearty recipes (Deer Stew, Sausages, Serpent Stew).</li>
      <li><strong>4. Mead Alchemy &amp; Fermentation:</strong> Brews mead bases and transfers them into empty camp Fermenters.</li>
      <li><strong>5. Fermenter Tapping:</strong> Empties finished potions (6 per keg) into pantry barrels marked <code>[Mead]</code>.</li>
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
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Worker Vitality Lifeline:</strong> Supplies the entire settlement roster with high-tier meals, maximizing worker stamina and productivity.</li>
      <li><strong>Hunter &amp; Farmer Recipient:</strong> Turns raw venison, pork, onions, and honey into long-lasting nutritious provisions.</li>
      <li><strong>Warrior Apothecary:</strong> Delivers healing and frost resistance potions into guardhouse lockers for expeditions.</li>
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
      <li><strong>Fermentation Lag:</strong> Mead takes 2 in-game days to ferment; cook checks kegs periodically without disturbing active brews.</li>
    </ul>
    """
))

# 10. HAULER (Page 15)
parts.append(make_worker_chapter(
    15, "Part XII", "The Hauler", "Logistics Operator &amp; Packmaster", "Logistics &bull; Freight",
    "Cart.png",
    [("Torch.png", "Torch", False), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Cargo", False), ("Stone.png", "Cargo", False)],
    [("Cart.png", "Vagon"), ("Reinforced_Chest.png", "Chest"), ("Sign.png", "Sign"), ("Wood_Pile.png", "Wood Stack"), ("Stone_Pile.png", "Stone Stack")],
    f"""
    <h3>{feat("F33", "Cart Loading", "Cart.png")} {feat("F05", "Overflow", "Wood_Pile.png")} {feat("F31", "Sorting", "Reinforced_Chest.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F33] Vagon Cart Loading:</strong> Identifies nearby player wheeled carts (<code>Vagon</code>) and loads bulk timber and heavy ores into cart containers.</li>
      <li><strong>[F05] Bulk Pile Construction:</strong> Solves warehouse congestion by constructing physical resource stacks when all chests are full.</li>
      <li><strong>[F31] Camp Storage Consolidation:</strong> Sweeps scattered ground drops across the yard and condenses half-empty chests into unified stacks.</li>
      <li><strong>[F32] Sign-Based Distribution:</strong> Routes items to their designated warehouse bays according to wall signs.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Settlement Floor Sweep:</strong> Scans 35m perimeter for loose ground drops, collecting wood, stone, resin, and pelts.</li>
      <li><strong>2. Overflow Mitigation:</strong> Inspects full chests; if capacity reached, erects 50x resource piles in open designated zones.</li>
      <li><strong>3. Sign Tag Routing:</strong> Redistributes misplaced items from general sorting bins into tag-matched chests.</li>
      <li><strong>4. Vagon Cart Packing:</strong> Packs heavy ores, stone, and wood into parked player transport carts (Vagon).</li>
      <li><strong>5. Dock &amp; Ship Staging:</strong> Transfers overseas freight into shoreline chests near Longship docks.</li>
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
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>The Settlement Arteries:</strong> Connects production nodes (quarries, logging camps, farms) to central warehouses.</li>
      <li><strong>Smelter Assistant:</strong> Relieves smelters from hauling coal by delivering 50x batches directly into foundry bins.</li>
      <li><strong>Player Expedition Sherpa:</strong> Fully stocks carts with expedition supplies (arrows, potions, food, wood) prior to player journeys.</li>
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
      <li><strong>Weight Encumbrance:</strong> Haulers have an expanded 450 weight capacity; if over-encumbered, they will immediately drop freight into nearest chest.</li>
      <li><strong>Cart Tipping:</strong> Park carts on flat wooden flooring or stone pavers to prevent carts from rolling away down slopes.</li>
    </ul>
    """
))

# 11. CRAFTSMAN (Page 16)
parts.append(make_worker_chapter(
    16, "Part XIII", "The Craftsman", "Artificer &amp; Carpenter", "Construction &bull; Smithing",
    "Hammer.png",
    [("Hammer.png", "Hammer", True), ("Bronze_Axe.png", "Axe", False), ("Rag_Tunic.png", "Tunic", True), ("Wood.png", "Material", False)],
    [("Workbench.png", "Bench"), ("Forge.png", "Forge"), ("Iron_Helmet.png", "Smithing"), ("Iron_Sword.png", "Weapons"), ("Fire_Arrow.png", "Fletcher")],
    f"""
    <h3>{feat("F30", "Workshop", "Workbench.png")} {feat("F20", "Repair", "Hammer.png")} {feat("F31", "Sorting", "Reinforced_Chest.png")} Active Capabilities</h3>
    <ul>
      <li><strong>[F30] Workshop Fabrication:</strong> Operates Workbenches, Forges, and Artisan Tables to craft arrows, building pieces, and camp tools.</li>
      <li><strong>[F20] Settlement-Wide Gear Repair:</strong> Serves as master blacksmith, repairing weapons, armor, and tools brought to workshops by other workers.</li>
      <li><strong>[F05] Structural Pile Organization:</strong> Maintains tidy timber and masonry yards with organized 50x resource piles.</li>
      <li><strong>[F32] Armory Organization:</strong> Categorizes weapons, shields, and armor into chests labeled <code>[Armor]</code> and <code>[Weapons]</code>.</li>
    </ul>
    """,
    f"""
    <h3>Operational AI Work-Cycle &amp; Behavior Sequence</h3>
    <ul>
      <li><strong>1. Workshop Readiness:</strong> Inspects Workbenches and Forges, ensuring valid roof coverage and structural support.</li>
      <li><strong>2. Repair Station Service:</strong> Automatically repairs damaged tools queued up by returning Miners, Lumberjacks, and Guards.</li>
      <li><strong>3. Ammunition Production:</strong> Fletches wood and fire arrows at the workbench using feathers, wood, and resin.</li>
      <li><strong>4. Tool Replenishment:</strong> Crafts replacement stone axes, antler pickaxes, and torches when tool reserves run dry.</li>
      <li><strong>5. Structural Inspection:</strong> Walks settlement perimeter, verifying stability of defensive gates and fences.</li>
    </ul>
    """,
    f"""
    <h3>Planned Future Expansions</h3>
    <ul>
      <li><strong>Automated Palisade Repair:</strong> Using hammer to repair weather and troll damage on outer settlement palisade walls.</li>
      <li><strong>Blueprint Snapping:</strong> Erecting pre-designed modular building pieces (sheds, watchtowers) from blueprint markers.</li>
    </ul>
    """,
    f"""
    <h3>Settlement Synergy &amp; Domain Handoffs</h3>
    <ul>
      <li><strong>Equipment Master:</strong> The cornerstone of settlement automation; keeps all worker tools in pristine working condition.</li>
      <li><strong>Smelter Recipient:</strong> Consumes refined bronze and iron ingots to forge weapons and shields for the Guard garrison.</li>
      <li><strong>Hunter Fletching Partner:</strong> Uses feathers brought by hunters to craft hundreds of precision hunting arrows.</li>
    </ul>
    """,
    f"""
    <h3>{hugin()} Settlement Architect Recommendations</h3>
    <ul>
      <li>House the Craftsman in a central smithy covered by a waterproof roof with an anvil and forge cooler nearby.</li>
      <li>Maintain chests marked <code>[Tools]</code>, <code>[Wood]</code>, and <code>[Ore]</code> within 5m of the primary workbench.</li>
    </ul>
    """,
    f"""
    <h3>Known Quirks &amp; Troubleshooting</h3>
    <ul>
      <li><strong>Roofing Requirement:</strong> Workbenches and Forges require roof shelter to function; AI will pause if rain exposes stations.</li>
      <li><strong>Upgrade Proximity:</strong> Workbench improvements (chopping block, adze) must be within 2m of station to unlock higher craft tiers.</li>
    </ul>
    """
))

# ==================== PAGE 17: LOGISTICS MASTERY ====================
print("Building Page 17: Logistics Mastery...")
parts.append(f"""
  <div class="page">
    <div class="content-container">
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Cart.png")}" class="chapter-icon" alt="Logistics"> Part XIV: Logistics Mastery &amp; Base Infrastructure</h2>
          <span class="role-pill">Storage &bull; Social</span>
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
            {slot("Standing_Wood_Torch.png", "Torch", False)}
            {slot("Dragon_Bed.png", "Bed", False)}
          </div>
        </div>
      </div>

      <div class="worker-grid-6">
        <div class="stone-tablet panel-now">
          <h3>1. [F05] Physical Overflow Resource Stacks</h3>
          <p>When settlement chests become completely full, workers never drop items to decay. Instead, they assemble physical resource piles on open terrain:</p>
          <ul>
            <li><strong>Wood Piles:</strong> 50x Wood &rarr; <code>wood_stack</code> &bull; 50x Fine Wood &rarr; <code>wood_fine_stack</code> &bull; 50x Core Wood &rarr; <code>wood_core_stack</code>.</li>
            <li><strong>Mineral Piles:</strong> 50x Stone &rarr; <code>stone_pile</code> &bull; 50x Coal &rarr; <code>coal_pile</code> &bull; 50x Bone Fragments &rarr; <code>bone_stack</code>.</li>
            <li><strong>Treasure Stack:</strong> 99x Coins &rarr; <code>treasure_stack</code> (for late-game wealth consolidation).</li>
            <li><strong>Ownership:</strong> Piles are registered with the local player ID, allowing players to deconstruct them instantly with a Hammer.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-routine">
          <h3>2. [F32] Sign-Based Smart Tag Routing</h3>
          <p>Workers inspect wall signs placed within 2.2m of any chest and parse tag annotations in brackets:</p>
          <ul>
            <li><strong>[Wood]:</strong> Directs regular Wood, Fine Wood, Core Wood, Yggdrasil Wood, and Bark.</li>
            <li><strong>[Stone] &amp; [Ore]:</strong> Routes rocks, flint, obsidian, copper, tin, iron scrap, and silver ores.</li>
            <li><strong>[Food] &amp; [Seeds]:</strong> Segregates cooked dishes, raw meats, honey, and agricultural seeds.</li>
            <li><strong>[Berries] &amp; [Trophy]:</strong> Stores foraged flora, wild berries, mushrooms, and hunted game trophies.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-future">
          <h3>3. [F33] Vagon Cart Automated Loading &amp; Caravan Convoys</h3>
          <ul>
            <li><strong>Wheeled Transport Integration:</strong> Haulers scan for player wheeled carts (<code>Vagon</code>) and transfer heavy ores and building timber directly into cart containers.</li>
            <li><strong>Caravan Staging:</strong> Position empty carts near your main gate; Haulers will automatically fill them with refined metals before your ocean voyages.</li>
            <li><strong>Weight Balancing:</strong> Workers distribute weight evenly to prevent cart tipping on steep switchback terrain.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-synergy">
          <h3>4. Social Life &amp; Expedition Immersion [F34–F36]</h3>
          <ul>
            <li><strong>[F34] Longship Boarding:</strong> When following player aboard a Karve or Longship, workers automatically mount passenger benches.</li>
            <li><strong>[F35] Night Torch Bearing:</strong> During nighttime (<code>EnvMan.IsNight()</code>) or inside dark dungeons, workers draw lit torches to guide paths.</li>
            <li><strong>[F36] Tavern Seating &amp; Bed Rest:</strong> During off-duty idle cycles, workers visit settlement chairs, stools, and beds to socialize and rest.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-rec">
          <h3>5. Stronghold Logistics Blueprint &amp; Yard Organization Rules</h3>
          <ul>
            <li><strong>Central Thoroughfare:</strong> Maintain a 4m-wide paved road connecting logging gates, quarry pits, and the central great hall.</li>
            <li><strong>Foundry Triangle:</strong> Keep Ore storage, Charcoal Kilns, and Smelters within an 8m radius to minimize smelting commute lag.</li>
            <li><strong>Quarry Staging:</strong> Flatten a 10&times;10m yard with a Hoe near mine pits to allow automated stone piles to erect cleanly.</li>
          </ul>
        </div>
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
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Hugin.png")}" class="chapter-icon" alt="Diagnostics"> Part XV: Diagnostics, Edge Cases &amp; Telemetry</h2>
          <span class="role-pill">Engine Stability &bull; Debug</span>
        </div>

        <div class="showcase-bar">
          <div class="showcase-group">
            <span class="showcase-title">Telemetry Systems:</span>
            {slot("Workbench.png", "NavMesh", False)}
            {slot("Hammer.png", "Repair", False)}
            {slot("Sign.png", "Mutex", False)}
          </div>
          <div class="showcase-group">
            <span class="showcase-title">Threat Matrix:</span>
            {slot("Valkyrie.png", "Horn", False)}
            {slot("Banded_Shield.png", "Guard", False)}
            {slot("Troll.png", "Threat", False)}
          </div>
        </div>
      </div>

      <div class="worker-grid-6">
        <div class="stone-tablet panel-now">
          <h3>1. Spatial Pathfinding &amp; Unstuck Teleport Mechanics</h3>
          <ul>
            <li><strong>NavMesh Agent Validation:</strong> Workers evaluate path traversability using Unity NavMesh; paths steeper than 45&deg; or crossing deep water (&gt;1.3m) are discarded.</li>
            <li><strong>Obstacle Raycasting:</strong> Frontal raycasts detect fallen trees, palisade gates, and closed doors. Workers wait 1.5s for gates or path around obstructions.</li>
            <li><strong>Stuck Vector Recovery:</strong> If position coordinates change by &lt;0.2m over 12 consecutive seconds during an active task, the engine executes an unstuck impulse jump (1.5m hop toward target).</li>
          </ul>
        </div>

        <div class="stone-tablet panel-routine">
          <h3>2. Tool Degradation &amp; Repair Routing Priority</h3>
          <ul>
            <li><strong>Durability Thresholds:</strong> When active tool durability drops below 15%, harvesting actions are aborted.</li>
            <li><strong>Station Tier Matching:</strong> Antler/stone tools route to Workbenches; bronze/iron/silver tools require an active, covered Forge.</li>
            <li><strong>Queue Preemption:</strong> If all nearby crafting stations are obstructed by other players or workers, the worker waits in a standby queue for up to 8s before retrying.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-future">
          <h3>3. Chest Contention, Multiplayer Sync &amp; ZDO Replication</h3>
          <ul>
            <li><strong>Chest Access Mutex:</strong> Valheim containers do not support simultaneous multi-client writes. ValheimWorkers implements a lightweight access lock: workers wait their turn if a chest is opened by a player.</li>
            <li><strong>ZDO State Persistence:</strong> HomePosition, WorkPosition, assigned role, stomach nourishment, and equipped gear are saved directly into the worker prefab's persistent <code>ZDO</code>.</li>
            <li><strong>Server Sync:</strong> In multiplayer dedicated servers, worker AI computes on the Zone Master client and syncs position/animation packets smoothly to all peers.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-synergy">
          <h3>4. Combat Threat Matrix &amp; Flee/Defend Thresholds</h3>
          <ul>
            <li><strong>Threat Evaluation:</strong> Enemies within 25m generate a threat score based on creature tier (Greydwarf: 20, Brute: 60, Troll: 150).</li>
            <li><strong>Non-Combatant Panic:</strong> Farmers, Cooks, and Gatherers retreat to the Camp Post when threat exceeds 40.</li>
            <li><strong>Guard Interception:</strong> Guards aggro immediately on threats &gt; 30, positioning themselves between hostiles and civilians.</li>
          </ul>
        </div>

        <div class="stone-tablet panel-rec">
          <h3>5. Console Debug Commands &amp; Telemetry Overlay</h3>
          <ul>
            <li><strong>F8 Management Console:</strong> Toggles the Settlement Hall GUI overlay to view active tasks, carry weights, and hunger levels for all workers.</li>
            <li><strong>Console Command <code>worker_debug</code>:</strong> Draws visual 3D debug rays in the world showing current path targets, work radii, and chest routing lines.</li>
            <li><strong>Console Command <code>worker_reset_all</code>:</strong> Teleports all settlement workers back to their Camp Posts and resets AI state machines in case of severe world terrain alterations.</li>
          </ul>
        </div>
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
      <div>
        <div class="chapter-header">
          <h2><img src="{ic("Valkyrie.png")}" class="chapter-icon" alt="Roster"> Part XVI: Master Settlement Roster &amp; Specifications</h2>
          <span class="role-pill">Specifications &bull; Seal</span>
        </div>

        <p style="font-size: 7.6pt; color: #C2B39E; margin-bottom: 3px;">
          Comprehensive role specification table detailing operational attributes, tool requirements, and base integration across all 11 worker classes:
        </p>

        <table>
          <thead>
            <tr>
              <th>Role Title</th>
              <th>Base HP</th>
              <th>Primary Tool</th>
              <th>Scan Radius</th>
              <th>Capacity</th>
              <th>Primary Workstation</th>
              <th>Key Output Yield</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td><strong>Lumberjack</strong></td>
              <td>150</td>
              <td>Axe (Stone/Bronze/Iron)</td>
              <td>30m</td>
              <td>300 wt</td>
              <td>Camp Workbench</td>
              <td>Wood, Fine Wood, Core Wood, 50x Piles</td>
            </tr>
            <tr>
              <td><strong>Miner</strong></td>
              <td>150</td>
              <td>Pickaxe &amp; Hoe</td>
              <td>25m</td>
              <td>350 wt</td>
              <td>Camp Forge</td>
              <td>Stone, Copper, Tin, Iron Scrap, Flint</td>
            </tr>
            <tr>
              <td><strong>Farmer</strong></td>
              <td>150</td>
              <td>Bronze Cultivator</td>
              <td>25m</td>
              <td>300 wt</td>
              <td>Cultivated Plots</td>
              <td>Carrots, Turnips, Onions, Barley, Tamed Boars</td>
            </tr>
            <tr>
              <td><strong>Gatherer</strong></td>
              <td>150</td>
              <td>Flint Knife / Hands</td>
              <td>35m</td>
              <td>250 wt</td>
              <td>Beehives &amp; Extractors</td>
              <td>Berries, Mushrooms, Thistle, Honey, Sap</td>
            </tr>
            <tr>
              <td><strong>Hunter</strong></td>
              <td>160</td>
              <td>Bow &amp; Arrows</td>
              <td>40m</td>
              <td>300 wt</td>
              <td>Perimeter Stands</td>
              <td>Raw Meat, Deer Hides, Feathers, Trophies</td>
            </tr>
            <tr>
              <td><strong>Guard</strong></td>
              <td>180</td>
              <td>Bow, Sword &amp; Shield</td>
              <td>35m</td>
              <td>300 wt</td>
              <td>Watchtowers &amp; Gates</td>
              <td>Perimeter Defense, War Horn Alerts, Trophy Drops</td>
            </tr>
            <tr>
              <td><strong>Smelter</strong></td>
              <td>150</td>
              <td>Kiln &amp; Furnace</td>
              <td>20m</td>
              <td>350 wt</td>
              <td>Smelters &amp; Blast Furnace</td>
              <td>Refined Copper, Tin, Bronze, Iron, Silver Bars</td>
            </tr>
            <tr>
              <td><strong>Base Tender</strong></td>
              <td>150</td>
              <td>Torch &amp; Fuel</td>
              <td>30m</td>
              <td>300 wt</td>
              <td>Hearths, Windmills</td>
              <td>Fireplace Refueling, Barley Flour, Linen Thread</td>
            </tr>
            <tr>
              <td><strong>Cook</strong></td>
              <td>150</td>
              <td>Spit &amp; Cauldron</td>
              <td>15m</td>
              <td>250 wt</td>
              <td>Cauldron &amp; Fermenter</td>
              <td>Roast Meats, Hearty Stews, Fermented Meads</td>
            </tr>
            <tr>
              <td><strong>Hauler</strong></td>
              <td>160</td>
              <td>Vagon Cart / Hands</td>
              <td>35m</td>
              <td>450 wt</td>
              <td>Warehouses &amp; Docks</td>
              <td>Cart Packing, Smart Chest Sorting, 50x Piles</td>
            </tr>
            <tr>
              <td><strong>Craftsman</strong></td>
              <td>150</td>
              <td>Hammer</td>
              <td>20m</td>
              <td>300 wt</td>
              <td>Upgraded Workbenches</td>
              <td>Tool Repair, Arrow Fletching, Armor Smithing</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="stone-tablet panel-routine" style="margin-top: 4px;">
        <h3>Endgame Stronghold Deployment Model (50-Person Mega-Fortress)</h3>
        <p style="font-size: 7.5pt; line-height: 1.28;">
          Recommended roster allocation to achieve 100% automated self-sufficiency in an endgame Nordic Citadel:
        </p>
        <ul style="margin-left: 15px; font-size: 7.4pt;">
          <li><strong>Resource Gathering Core (18 Workers):</strong> 5 Lumberjacks (Forester grove), 6 Miners (Deep iron/silver quarry), 4 Farmers (Terraced gardens), 3 Gatherers (Orchard &amp; sap tapping).</li>
          <li><strong>Processing &amp; Refining (12 Workers):</strong> 4 Smelters (Battery of 4 kilns &amp; 4 smelters), 3 Base Tenders (Citadel fires &amp; windmills), 3 Cooks (Brewery &amp; double cauldrons), 2 Craftsmen (Central armory).</li>
          <li><strong>Logistics &amp; Infrastructure (8 Workers):</strong> 8 Haulers (Cart convoys, yard sweepers, dock loading for longship voyages).</li>
          <li><strong>Perimeter Defense Garrison (12 Guards):</strong> 4 Gatehouse Sentinels (Bow/Sword CQC), 4 Parapet Tower Snipers (Huntsman bows), 4 Mobile Patrol Wardens (War horn alert team).</li>
        </ul>
      </div>

      <div class="stone-tablet panel-rec" style="margin-top: 4px;">
        <h3>{hugin()} Epilogue &amp; Valhalla Certification</h3>
        <p style="font-size: 7.6pt; line-height: 1.30;">
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

print("Done! Guide book compiled successfully.")
