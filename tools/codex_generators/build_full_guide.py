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

print("Loading Valheim authentic fonts...")
norse_b64 = get_font_b64("Norse.otf")
norsebold_b64 = get_font_b64("Norsebold.otf")
averia_reg_b64 = get_font_b64("AveriaSerifLibre-Regular.ttf")
averia_bold_b64 = get_font_b64("AveriaSerifLibre-Bold.ttf")
averia_italic_b64 = get_font_b64("AveriaSerifLibre-Italic.ttf")

# Relative icon helper
def ic(name):
    return f"docs/assets/icons/{name}"

def slot(img_name, label="", equipped=False):
    eq_class = " equipped" if equipped else ""
    lbl_html = f'<span class="slot-label">{label}</span>' if label else ""
    return f'<div class="valheim-slot{eq_class}"><img src="{ic(img_name)}" alt="{label}">{lbl_html}</div>'

def feat(code, name, img_name):
    return f'<span class="feature-badge"><img src="{ic(img_name)}" alt="{code}"> {code} {name}</span>'

def hugin_tip():
    return f'<span class="hugin-badge"><img src="{ic("Hugin.png")}" alt="Hugin"> HUGIN\'S ADVICE</span>'

print("Assembling compendium HTML...")

# We will generate the 19 pages with complete styling
with open(r"d:\valheim_mods\generate_guide_book.py", "w", encoding="utf-8") as out:
    out.write(f'''import os
import subprocess
import base64
import sys

FONTS_DIR = r"d:\\valheim_mods\\docs\\assets\\fonts"

def get_font_b64(filename):
    p = os.path.join(FONTS_DIR, filename)
    if os.path.exists(p):
        with open(p, "rb") as f:
            return base64.b64encode(f.read()).decode("ascii")
    return ""

norse_b64 = get_font_b64("Norse.otf")
norsebold_b64 = get_font_b64("Norsebold.otf")
averia_reg_b64 = get_font_b64("AveriaSerifLibre-Regular.ttf")
averia_bold_b64 = get_font_b64("AveriaSerifLibre-Bold.ttf")
averia_italic_b64 = get_font_b64("AveriaSerifLibre-Italic.ttf")

def ic(name):
    return f"docs/assets/icons/{{name}}"

def slot(img_name, label="", equipped=False):
    eq_class = " equipped" if equipped else ""
    lbl_html = f'<span class="slot-label">{{label}}</span>' if label else ""
    return f'<div class="valheim-slot{{eq_class}}"><img src="{{ic(img_name)}}" alt="{{label}}">{{lbl_html}}</div>'

def feat(code, name, img_name):
    return f'<span class="feature-badge"><img src="{{ic(img_name)}}" alt="{{code}}"> {{code}} {{name}}</span>'

def hugin():
    return f'<span class="hugin-badge"><img src="{{ic(\\'Hugin.png\\')}}" alt="Hugin"> HUGIN\\'S ADVICE</span>'

html_content = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>The Settlement Steward's Codex — Official Guide to Valheim NPC Workers</title>
  <style>
    @font-face {{
      font-family: 'Norse';
      src: url('data:font/otf;base64,{{norse_b64}}') format('opentype');
      font-weight: normal;
    }}
    @font-face {{
      font-family: 'Norsebold';
      src: url('data:font/otf;base64,{{norsebold_b64}}') format('opentype');
      font-weight: bold;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{{averia_reg_b64}}') format('truetype');
      font-weight: normal;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{{averia_bold_b64}}') format('truetype');
      font-weight: bold;
    }}
    @font-face {{
      font-family: 'Averia Serif Libre';
      src: url('data:font/ttf;base64,{{averia_italic_b64}}') format('truetype');
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
      line-height: 1.34;
      font-size: 7.8pt;
    }}

    .page {{
      height: 280mm;
      max-height: 280mm;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      page-break-before: always;
      break-before: page;
      padding: 6.5mm 8mm 5mm 8mm;
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

    /* Norse Runic Header Border */
    .page::before {{
      content: "ᚠ   ᚦ   ᚨ   ᚱ   ᛋ   ᛏ   ᛒ   ᛖ   ᛗ   ᛚ   ᛜ   ᛞ   ᛟ";
      position: absolute;
      top: 2px;
      left: 12px;
      right: 12px;
      text-align: center;
      font-family: 'Norse', serif;
      font-size: 5.8pt;
      color: rgba(200, 155, 60, 0.35);
      letter-spacing: 5px;
      pointer-events: none;
    }}

    /* Headers */
    .chapter-header {{
      border-bottom: 2px solid #846028;
      padding-bottom: 3px;
      margin-bottom: 5px;
      display: flex;
      justify-content: space-between;
      align-items: baseline;
      position: relative;
    }}
    .chapter-header h2 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 14pt;
      color: #FFBF1F;
      letter-spacing: 0.04em;
      text-shadow: 0 2px 4px rgba(0,0,0,0.8);
      display: flex;
      align-items: center;
      gap: 6px;
    }}
    .chapter-header .chapter-icon {{
      width: 20px;
      height: 20px;
      object-fit: contain;
      filter: drop-shadow(0 0 3px #EB8F0D);
    }}
    .role-pill {{
      background: #845B1C;
      color: #FFF2D1;
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 7.2pt;
      padding: 2px 7px;
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
      font-size: 6.8pt;
      color: #9C8C78;
      font-family: 'Averia Serif Libre', serif;
      margin-top: 3px;
    }}

    /* Showcase Equipment & Station Bar */
    .showcase-bar {{
      background: rgba(18, 13, 9, 0.9);
      border: 1px solid #4D3921;
      border-radius: 4px;
      padding: 3.5px 7px;
      margin-bottom: 5px;
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
      font-size: 7.1pt;
      color: #DDA13B;
      text-transform: uppercase;
      letter-spacing: 0.04em;
      margin-right: 2px;
    }}
    
    /* Authentic Valheim Inventory Slot */
    .valheim-slot {{
      width: 30px;
      height: 30px;
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
      width: 24px;
      height: 24px;
      object-fit: contain;
      filter: drop-shadow(0 1px 2px rgba(0,0,0,0.7));
    }}
    .valheim-slot .slot-label {{
      position: absolute;
      bottom: -11px;
      left: 50%;
      transform: translateX(-50%);
      font-size: 5.5pt;
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

    /* Panels */
    .panel {{
      border-radius: 4px;
      padding: 4.5px 7px;
      margin-bottom: 4px;
      border-left: 3.5px solid;
      background: rgba(22, 17, 12, 0.88);
      border-top: 1px solid rgba(75, 55, 30, 0.4);
      border-right: 1px solid rgba(75, 55, 30, 0.4);
      border-bottom: 1px solid rgba(75, 55, 30, 0.4);
      box-shadow: 0 1px 4px rgba(0,0,0,0.4);
    }}
    .panel h3 {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 8.6pt;
      margin-bottom: 2px;
      display: flex;
      align-items: center;
      gap: 5px;
      letter-spacing: 0.02em;
    }}
    .panel p {{
      font-size: 7.6pt;
      line-height: 1.32;
      color: #DDD2C1;
      margin-bottom: 2px;
    }}
    .panel ul {{
      margin-left: 14px;
      font-size: 7.5pt;
      color: #E3D9C9;
      line-height: 1.32;
    }}
    .panel li {{
      margin-bottom: 1.8px;
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
      font-size: 6.6pt;
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
      font-size: 7pt;
      font-family: 'Norsebold', serif;
      margin-right: 3px;
    }}
    .feature-badge img {{
      width: 11px;
      height: 11px;
      object-fit: contain;
    }}

    /* Tables */
    table {{
      width: 100%;
      border-collapse: collapse;
      margin: 3px 0;
      font-size: 7.1pt;
      background: rgba(18, 13, 9, 0.9);
      border: 1px solid #4D3921;
    }}
    th, td {{
      border: 1px solid #44321D;
      padding: 3px 5px;
      text-align: left;
    }}
    th {{
      background: #231911;
      color: #FFBF1F;
      font-family: 'Norsebold', serif;
      font-weight: 700;
      font-size: 7.2pt;
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
      font-size: 7.5pt;
    }}

    /* Cover Page */
    .cover-page {{
      background: radial-gradient(circle at center, #261B12 0%, #150E09 60%, #0D0805 100%);
      border: 3px double #C49137;
      outline: 1.5px solid #6E4F22;
      outline-offset: -7px;
      text-align: center;
      align-items: center;
      padding: 10mm 12mm 7mm 12mm;
    }}
    .cover-title {{
      font-family: 'Norsebold', 'Norse', serif;
      font-size: 25pt;
      line-height: 1.1;
      color: #FFBF1F;
      margin-top: 8px;
      margin-bottom: 3px;
      text-shadow: 0 3px 12px rgba(0,0,0,0.9), 0 0 16px rgba(235, 143, 13, 0.45);
      letter-spacing: 0.05em;
    }}
    .cover-subtitle {{
      font-family: 'Averia Serif Libre', serif;
      font-size: 10.2pt;
      color: #DDD2C1;
      max-width: 540px;
      margin: 0 auto 8px auto;
      font-style: italic;
    }}
    .cover-divider {{
      width: 170px;
      height: 2px;
      background: linear-gradient(90deg, transparent, #DDA13B, transparent);
      margin: 7px auto;
    }}
    .cover-crest {{
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 12px;
      margin: 8px 0;
    }}
    .cover-crest img.main-shield {{
      width: 72px;
      height: 72px;
      filter: drop-shadow(0 4px 10px rgba(0,0,0,0.9)) drop-shadow(0 0 10px rgba(255, 191, 31, 0.35));
    }}
    .cover-crest img.flank-tool {{
      width: 44px;
      height: 44px;
      filter: drop-shadow(0 3px 6px rgba(0,0,0,0.8));
    }}
    .cover-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 6px;
      max-width: 600px;
      margin: 9px auto;
      text-align: left;
    }}
    .cover-card {{
      background: rgba(22, 16, 11, 0.88);
      border: 1px solid #4D3921;
      border-radius: 4px;
      padding: 5px 9px;
      display: flex;
      align-items: center;
      gap: 8px;
    }}
    .cover-card img {{
      width: 26px;
      height: 26px;
      object-fit: contain;
      filter: drop-shadow(0 1px 3px #000);
    }}
    .cover-card strong {{
      color: #FFBF1F;
      font-family: 'Norsebold', serif;
      font-size: 8pt;
      display: block;
      margin-bottom: 1px;
    }}
    .cover-card p {{
      color: #B5A998;
      font-size: 7pt;
      line-height: 1.25;
    }}

    /* TOC Cards */
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
      padding: 5px 8px;
    }}
    .toc-card h4 {{
      font-family: 'Norsebold', serif;
      font-size: 8.3pt;
      color: #FFBF1F;
      border-bottom: 1px solid #5C4425;
      padding-bottom: 2px;
      margin-bottom: 3px;
      display: flex;
      align-items: center;
      gap: 4px;
    }}
    .toc-card h4 img {{
      width: 13px;
      height: 13px;
      object-fit: contain;
    }}
    .toc-list {{
      list-style: none;
      font-size: 7.3pt;
    }}
    .toc-list li {{
      display: flex;
      justify-content: space-between;
      margin-bottom: 1.8px;
      border-bottom: 1px dotted #3D2D1B;
      padding-bottom: 1px;
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
      color: #DDA13B;
      font-size: 7.3pt;
    }}

    /* 36-Feature Compendium Grid */
    .feat-catalog-grid {{
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 3.5px 5px;
      margin-top: 2px;
    }}
    .feat-card-comp {{
      background: rgba(20, 15, 10, 0.92);
      border: 1px solid #44321D;
      border-left: 3px solid #DDA13B;
      border-radius: 3px;
      padding: 2.5px 5px;
      display: flex;
      align-items: center;
      gap: 5px;
    }}
    .feat-card-comp .feat-img-box {{
      width: 24px;
      height: 24px;
      background: #15100B;
      border: 1px solid #3D2D1B;
      border-radius: 3px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }}
    .feat-card-comp .feat-img-box img {{
      width: 19px;
      height: 19px;
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
      font-size: 7.4pt;
      color: #FFBF1F;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }}
    .feat-card-domain {{
      font-size: 5.4pt;
      color: #9C8C78;
      font-family: 'Norse', serif;
      text-transform: uppercase;
      padding: 0 2px;
      background: #251B12;
      border-radius: 2px;
    }}
    .feat-card-desc {{
      font-size: 6.6pt;
      color: #BFB2A2;
      line-height: 1.18;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }}

    /* Matrix Table */
    table.matrix-view {{
      font-size: 6.5pt;
      text-align: center;
    }}
    table.matrix-view th, table.matrix-view td {{
      padding: 1.8px 1.2px;
      border: 1px solid #3E2E1D;
    }}
    table.matrix-view th.feat-hdr {{
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      height: 64px;
      font-size: 6.2pt;
      padding: 2px 1px;
      background: #20170F;
      color: #FFBF1F;
    }}
    table.matrix-view th.worker-hdr {{
      text-align: left;
      padding-left: 4px;
      width: 86px;
      background: #251B12;
    }}
    table.matrix-view td.w-name {{
      text-align: left;
      font-family: 'Norsebold', serif;
      padding-left: 4px;
      background: #1B140E;
      color: #FCD34D;
      font-size: 7pt;
    }}
    .m-yes {{
      color: #78C946;
      font-weight: bold;
      font-size: 7.2pt;
    }}
    .m-no {{
      color: #4A3724;
      font-size: 5pt;
    }}
  </style>
</head>
<body>
""")
''')

print("Script template created, now adding complete 19-page content generator...")
