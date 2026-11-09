# Master Builder for ValheimWorkers Stronghold Hybrid Guide Book
import os, sys, base64, subprocess, pymupdf

BASE_DIR = r"d:\valheim_mods"
DOCS_DIR = os.path.join(BASE_DIR, "docs")
ASSETS_DIR = os.path.join(DOCS_DIR, "assets")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
STONE_DIR = os.path.join(ASSETS_DIR, "stone")

HTML_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.html")
PDF_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.pdf")
SUBDIR_HTML = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.html")
SUBDIR_PDF = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.pdf")

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

print("Encoding icons...")
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
    # Fallback to general icon if name has spaces or underscores mismatch
    norm = name.lower().replace(" ", "_").replace("-", "_")
    for k in icon_cache:
        if norm in k.lower():
            return icon_cache[k]
    return ""

def slot(icon_name, label, equipped=False, size=44):
    eq_cls = " equipped" if equipped else ""
    return f"""<div class="valheim-slot{eq_cls}" style="width:{size}px; height:{size}px;"><img src="{ic(icon_name)}" alt="{label}"><span class="slot-label">{label}</span></div>"""

# Shared SVG Filter Definitions for True V-Groove Carving
SVG_FILTERS = """
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
    <!-- 1. GOLD V-GROOVE CHISEL (Titles & Cover) -->
    <filter id="v-groove-gold" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
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

    <!-- 2. CYAN RUNIC CHISEL (Work-Cycle Headers) -->
    <filter id="v-groove-cyan" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
      <feTurbulence type="fractalNoise" baseFrequency="0.12" numOctaves="2" seed="5" result="noise"/>
      <feDisplacementMap in="SourceGraphic" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap in="SourceAlpha" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>

      <feGaussianBlur in="wobbleAlpha" stdDeviation="0.7" result="b1"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="1.7" result="b2"/>
      <feComposite in="b1" in2="b2" operator="arithmetic" k1="0" k2="0.55" k3="0.45" k4="0" result="heightMap"/>

      <feDiffuseLighting in="heightMap" surfaceScale="-10" diffuseConstant="1.25" lighting-color="#e0f7ff" result="diffLight">
        <feDistantLight azimuth="315" elevation="40"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="-0.05" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>

      <feOffset in="wobbleAlpha" dx="1.4" dy="1.4" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="topLeftCrescent"/>
      <feFlood flood-color="#020408" flood-opacity="0.95" result="blackFlood"/>
      <feComposite in="blackFlood" in2="topLeftCrescent" operator="in" result="innerRimShadow"/>

      <feOffset in="wobbleAlpha" dx="1.2" dy="1.2" result="outerLipOff"/>
      <feComposite in="outerLipOff" in2="wobbleAlpha" operator="out" result="outerBottomRightLip"/>
      <feFlood flood-color="#bae6fd" flood-opacity="0.55" result="lipFlood"/>
      <feComposite in="lipFlood" in2="outerBottomRightLip" operator="in" result="outerLipHighlight"/>

      <feMerge>
        <feMergeNode in="outerLipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="innerRimShadow"/>
      </feMerge>
    </filter>

    <!-- 3. VERDIGRIS GREEN CHISEL (Capabilities) -->
    <filter id="v-groove-green" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
      <feTurbulence type="fractalNoise" baseFrequency="0.12" numOctaves="2" seed="3" result="noise"/>
      <feDisplacementMap in="SourceGraphic" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap in="SourceAlpha" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>

      <feGaussianBlur in="wobbleAlpha" stdDeviation="0.7" result="b1"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="1.7" result="b2"/>
      <feComposite in="b1" in2="b2" operator="arithmetic" k1="0" k2="0.55" k3="0.45" k4="0" result="heightMap"/>

      <feDiffuseLighting in="heightMap" surfaceScale="-10" diffuseConstant="1.25" lighting-color="#eafde6" result="diffLight">
        <feDistantLight azimuth="315" elevation="40"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="-0.05" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>

      <feOffset in="wobbleAlpha" dx="1.4" dy="1.4" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="topLeftCrescent"/>
      <feFlood flood-color="#020502" flood-opacity="0.95" result="blackFlood"/>
      <feComposite in="blackFlood" in2="topLeftCrescent" operator="in" result="innerRimShadow"/>

      <feOffset in="wobbleAlpha" dx="1.2" dy="1.2" result="outerLipOff"/>
      <feComposite in="outerLipOff" in2="wobbleAlpha" operator="out" result="outerBottomRightLip"/>
      <feFlood flood-color="#bbf7d0" flood-opacity="0.55" result="lipFlood"/>
      <feComposite in="lipFlood" in2="outerBottomRightLip" operator="in" result="outerLipHighlight"/>

      <feMerge>
        <feMergeNode in="outerLipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="innerRimShadow"/>
      </feMerge>
    </filter>

    <!-- 4. ORANGE OCHRE CHISEL (Alerts & Synergy) -->
    <filter id="v-groove-orange" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
      <feTurbulence type="fractalNoise" baseFrequency="0.12" numOctaves="2" seed="8" result="noise"/>
      <feDisplacementMap in="SourceGraphic" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap in="SourceAlpha" in2="noise" scale="1.4" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>

      <feGaussianBlur in="wobbleAlpha" stdDeviation="0.7" result="b1"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="1.7" result="b2"/>
      <feComposite in="b1" in2="b2" operator="arithmetic" k1="0" k2="0.55" k3="0.45" k4="0" result="heightMap"/>

      <feDiffuseLighting in="heightMap" surfaceScale="-10" diffuseConstant="1.25" lighting-color="#fff2e0" result="diffLight">
        <feDistantLight azimuth="315" elevation="40"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="-0.05" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>

      <feOffset in="wobbleAlpha" dx="1.4" dy="1.4" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="topLeftCrescent"/>
      <feFlood flood-color="#080402" flood-opacity="0.95" result="blackFlood"/>
      <feComposite in="blackFlood" in2="topLeftCrescent" operator="in" result="innerRimShadow"/>

      <feOffset in="wobbleAlpha" dx="1.2" dy="1.2" result="outerLipOff"/>
      <feComposite in="outerLipOff" in2="wobbleAlpha" operator="out" result="outerBottomRightLip"/>
      <feFlood flood-color="#fed7aa" flood-opacity="0.55" result="lipFlood"/>
      <feComposite in="lipFlood" in2="outerBottomRightLip" operator="in" result="outerLipHighlight"/>

      <feMerge>
        <feMergeNode in="outerLipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="innerRimShadow"/>
      </feMerge>
    </filter>
  </defs>
</svg>
"""

COMMON_STYLE = f"""
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

  * {{ box-sizing: border-box; margin: 0; padding: 0; }}

  @page {{
    size: A4 portrait;
    margin: 0;
  }}

  body {{
    font-family: 'Averia Serif Libre', Georgia, serif;
    color: #ede5d8;
    background-color: #0b0d10;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }}

  .page {{
    width: 210mm;
    height: 297mm;
    page-break-after: always;
    padding: 9.5mm 10.5mm;
    background-color: #121418;
    background-image: url('data:image/png;base64,{stone_monolith_b64}');
    background-repeat: repeat;
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    border: 4.5px solid #090a0d;
    box-shadow: inset 0 0 45px rgba(0,0,0,0.92);
    overflow: hidden;
  }}

  .page::before {{
    content: "";
    position: absolute;
    inset: 3.5mm;
    border: 1.5px solid rgba(220, 185, 120, 0.28);
    pointer-events: none;
    box-shadow: inset 0 0 10px rgba(0,0,0,0.7);
  }}

  .content-container {{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
    gap: 7px;
    min-height: 0;
  }}

  /* Top Chapter Header */
  .chapter-header {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 2.5px solid #5a4220;
    padding-bottom: 4px;
  }}
  .chapter-header h2 {{
    font-family: 'Norsebold', serif;
    font-size: 19.5pt;
    color: #f7d57f;
    letter-spacing: 0.05em;
    filter: url(#v-groove-gold);
    display: flex;
    align-items: center;
    gap: 10px;
    margin: 0;
    white-space: nowrap;
  }}
  .chapter-header img.chapter-icon {{
    width: 32px;
    height: 32px;
    object-fit: contain;
    filter: drop-shadow(0 3px 6px rgba(0,0,0,0.9));
  }}
  .role-pill {{
    background: linear-gradient(135deg, #7A5116 0%, #52350C 100%);
    color: #FFF4D9;
    font-family: 'Norsebold', serif;
    font-size: 11pt;
    padding: 4px 13px;
    border-radius: 4px;
    border: 1.5px solid #DDA13B;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    box-shadow: inset 1px 1px 2px rgba(255,255,255,0.25), 0 2px 6px rgba(0,0,0,0.8);
    white-space: nowrap;
  }}

  /* Top Showcase Bar with Stone Sockets */
  .showcase-bar {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    background-color: #0f1217;
    border-top: 1.5px solid rgba(220, 185, 120, 0.25);
    border-bottom: 2px solid #06080a;
    border-radius: 4px;
    padding: 4px 10px;
    box-shadow: inset 0 2px 5px rgba(0,0,0,0.8);
  }}
  .showcase-group {{
    display: flex;
    align-items: center;
    gap: 6px;
  }}
  .showcase-title {{
    font-family: 'Norsebold', serif;
    font-size: 9.5pt;
    color: #d1b580;
    letter-spacing: 0.06em;
    margin-right: 3px;
  }}
  .valheim-slot {{
    background-color: #12151b;
    background-image: url('data:image/png;base64,{stone_socket_b64}');
    background-size: cover;
    border: 2px solid #3c3326;
    border-radius: 4px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    position: relative;
    box-shadow: inset 2px 2px 4px rgba(0,0,0,0.9), 0 2px 4px rgba(0,0,0,0.6);
  }}
  .valheim-slot.equipped {{
    background-image: url('data:image/png;base64,{stone_socket_eq_b64}');
    border-color: #3bb2d9;
    box-shadow: inset 0 0 6px rgba(59, 178, 217, 0.6), 0 0 8px rgba(59, 178, 217, 0.5);
  }}
  .valheim-slot img {{
    width: 25px;
    height: 25px;
    object-fit: contain;
    filter: drop-shadow(0 2px 3px rgba(0,0,0,0.8));
  }}
  .slot-label {{
    font-family: 'Norsebold', serif;
    font-size: 6.8pt;
    color: #e8dcc8;
    letter-spacing: 0.04em;
    position: absolute;
    bottom: 1px;
    text-shadow: 1px 1px 2px #000;
    text-transform: uppercase;
  }}

  /* Two-Column Split Stele Architecture */
  .split-container {{
    flex: 1;
    display: grid;
    grid-template-columns: 1fr 1.05fr;
    gap: 8px;
    min-height: 0;
  }}
  .col {{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 7px;
    min-height: 0;
  }}

  .tablet {{
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    background-color: #16191f;
    background-image: url('data:image/png;base64,{stone_tablet_b64}');
    background-repeat: repeat;
    border-top: 2px solid rgba(240, 215, 160, 0.35);
    border-left: 2px solid rgba(240, 215, 160, 0.25);
    border-bottom: 3px solid #06080a;
    border-right: 2.5px solid #06080a;
    border-radius: 5px;
    padding: 8px 12px;
    box-shadow: inset 2px 2px 5px rgba(0,0,0,0.88), 0 4px 10px rgba(0,0,0,0.7);
  }}

  .tablet h3 {{
    font-family: 'Norsebold', serif;
    font-size: 14pt;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    margin: 0 0 4px 0;
    display: flex;
    align-items: center;
    gap: 8px;
    border-bottom: 1.5px solid rgba(255, 230, 180, 0.2);
    padding-bottom: 3px;
  }}
  .tablet h3.gold {{ color: #f7d57f; filter: url(#v-groove-gold); }}
  .tablet h3.cyan {{ color: #7ad9f5; filter: url(#v-groove-cyan); }}
  .tablet h3.green {{ color: #8ade4c; filter: url(#v-groove-green); }}
  .tablet h3.orange {{ color: #f59e42; filter: url(#v-groove-orange); }}

  .tablet ul {{
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    flex: 1;
  }}
  .tablet li {{
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-weight: bold;
    font-size: 10pt;
    color: #ede2d0;
    line-height: 1.36;
    letter-spacing: 0.015em;
    text-shadow: -1.5px -1.5px 0px #040507, 1px 1px 0px rgba(245, 225, 185, 0.5);
    margin-bottom: 3px;
  }}
  .tablet li strong {{
    color: #fff5d4;
    font-family: 'Norsebold', serif;
    font-size: 11pt;
    letter-spacing: 0.04em;
    display: block;
    margin-bottom: 1px;
  }}

  /* Page Footer */
  .page-footer {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-top: 1.5px solid rgba(245, 225, 185, 0.25);
    padding-top: 4px;
    font-family: 'Norsebold', serif;
    font-size: 8.5pt;
    color: #9c9483;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }}
"""

print("Writing guide generator...")

def make_worker_page(page_num, part_roman, role_title, role_sub, role_pill, icon_main, loadout_slots, yield_slots, cycle_items, advice_items, cap_items, synergy_items):
    loadout_html = "".join([slot(s[0], s[1], s[2], 44) for s in loadout_slots])
    yield_html = "".join([slot(s[0], s[1], s[2], 44) for s in yield_slots])
    
    cycle_li = "".join([f"<li><strong>{item[0]}</strong>{item[1]}</li>" for item in cycle_items])
    advice_li = "".join([f"<li><strong>{item[0]}</strong>{item[1]}</li>" for item in advice_items])
    cap_li = "".join([f"<li><strong>{item[0]}</strong>{item[1]}</li>" for item in cap_items])
    synergy_li = "".join([f"<li><strong>{item[0]}</strong>{item[1]}</li>" for item in synergy_items])
    
    return f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic(icon_main)}" class="chapter-icon" alt="{role_title}"> {part_roman}: {role_title} ({role_sub})</h2>
        <span class="role-pill">{role_pill}</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">LOADOUT:</span>
          {loadout_html}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">YIELDS &amp; STATIONS:</span>
          {yield_html}
        </div>
      </div>

      <div class="split-container">
        <!-- Left Column -->
        <div class="col">
          <div class="tablet">
            <h3 class="cyan">AUTONOMOUS AI WORK-CYCLE</h3>
            <ul>
              {cycle_li}
            </ul>
          </div>

          <div class="tablet">
            <h3 class="gold">HUGIN'S ADVICE &amp; QUIRKS</h3>
            <ul>
              {advice_li}
            </ul>
          </div>
        </div>

        <!-- Right Column -->
        <div class="col">
          <div class="tablet">
            <h3 class="green">ACTIVE DOMAIN CAPABILITIES</h3>
            <ul>
              {cap_li}
            </ul>
          </div>

          <div class="tablet">
            <h3 class="orange">SETTLEMENT DOMAIN SYNERGY</h3>
            <ul>
              {synergy_li}
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>VALHEIMWORKERS v0.3.0 &bull; STRONGHOLD SPECIFICATION</span>
      <span>PAGE {page_num}</span>
    </div>
  </div>
"""

# ==================== PAGE 1: COVER PAGE ====================
def make_cover_page():
    return f"""
  <div class="page" style="padding: 12mm 14mm;">
    <div class="content-container" style="align-items: center; text-align: center; justify-content: space-between;">
      
      <!-- Top Rune Ribbon -->
      <div style="font-family:'Norsebold'; font-size:14pt; color:#a89676; letter-spacing:0.25em; text-shadow:0 2px 4px #000; border-bottom:1.5px solid rgba(220,185,120,0.3); width:100%; padding-bottom:6px;">
        &#5808; &#5810; &#5814; &#5816; &#5818; &#5820; &#5822; &#5824; &#5826; &#5828; &#5830; &#5832; &#5834; &#5836; &#5838; &#5840; &#5842; &#5844; &#5846; &#5848; &#5850; &#5852; &#5854; &#5808;
      </div>

      <!-- Main Monumental Title -->
      <div style="margin-top: 10px;">
        <h1 style="font-family:'Norsebold'; font-size:38pt; color:#f7d57f; letter-spacing:0.08em; filter:url(#v-groove-gold); margin-bottom:4px;">
          VALHEIM WORKERS
        </h1>
        <div style="font-family:'Norsebold'; font-size:20pt; color:#7ad9f5; letter-spacing:0.06em; filter:url(#v-groove-cyan); margin-bottom:8px;">
          THE SETTLEMENT STEWARD'S CODEX
        </div>
        <div style="font-family:'Averia Serif Libre'; font-size:11pt; font-weight:bold; color:#d6cbba; letter-spacing:0.05em; text-transform:uppercase;">
          Authoritative Domain Guide &bull; Autonomous AI Specification &bull; v0.3.0
        </div>
      </div>

      <!-- Central Sunken Relief Crest Plinth -->
      <div style="background-color:#14171d; background-image:url('data:image/png;base64,{stone_tablet_b64}'); border:3px solid #5a4220; border-radius:10px; padding:18px 24px; box-shadow:inset 3px 3px 8px rgba(0,0,0,0.9), 0 8px 20px rgba(0,0,0,0.85); display:flex; flex-direction:column; align-items:center;">
        <img src="data:image/png;base64,{stone_crest_b64}" style="width:160px; height:auto; filter:drop-shadow(0 6px 12px rgba(0,0,0,0.9)); margin-bottom:10px;" alt="Valheim Workers Crest">
        <div style="font-family:'Norsebold'; font-size:13pt; color:#f7d57f; letter-spacing:0.08em; filter:url(#v-groove-gold);">
          INSCRIBED FOR THE CHIEFTAINS OF THE TENTH REALM
        </div>
      </div>

      <!-- 10 Domain Icon Band -->
      <div style="width:100%; background-color:#0f1217; border-top:1.5px solid rgba(220,185,120,0.3); border-bottom:2px solid #06080a; border-radius:6px; padding:8px 12px; display:flex; justify-content:space-around; align-items:center;">
        {slot("Iron_Axe.png", "Lumber", True, 42)}
        {slot("Antler_Pickaxe.png", "Miner", True, 42)}
        {slot("Cultivator.png", "Farmer", True, 42)}
        {slot("Raw_Meat.png", "Rancher", True, 42)}
        {slot("Smelter.png", "Smelter", True, 42)}
        {slot("Cauldron.png", "Cook", True, 42)}
        {slot("Hammer.png", "Craftsman", True, 42)}
        {slot("Karve.png", "Fisher", True, 42)}
        {slot("Cart.png", "Hauler", True, 42)}
        {slot("Finewood_Bow.png", "Sentry", True, 42)}
      </div>

      <!-- Core Systems Architecture Stele -->
      <div style="width:100%; display:grid; grid-template-columns:1fr 1fr; gap:10px; text-align:left;">
        <div class="tablet" style="padding:10px 14px;">
          <h3 class="green" style="font-size:12pt; margin-bottom:4px;">TEN AUTONOMOUS ROLES</h3>
          <p style="font-size:9.2pt; line-height:1.4; color:#ede2d0; font-weight:bold;">
            Transform quiet wilderness into bustling strongholds. Every worker autonomously manages tool integrity, material sourcing, crafting pipelines, and defensive patrols.
          </p>
        </div>
        <div class="tablet" style="padding:10px 14px;">
          <h3 class="cyan" style="font-size:12pt; margin-bottom:4px;">ZERO-MAINTENANCE ECONOMY</h3>
          <p style="font-size:9.2pt; line-height:1.4; color:#ede2d0; font-weight:bold;">
            Featuring automated sign bracket routing [Wood], cart freight transport, 50x resource overflow stacking, and intelligent forge repair sequences.
          </p>
        </div>
      </div>

      <!-- Seal Stamp -->
      <div style="font-family:'Norsebold'; font-size:9pt; color:#857b68; letter-spacing:0.12em; text-transform:uppercase;">
        MONOLITHIC RUNESTONE EDITION &bull; VALHEIM MODDING ARCHITECTURE &bull; 2026
      </div>
    </div>
  </div>
"""

# ==================== PAGE 2: TABLE OF CONTENTS & SETTLEMENT ARCHITECTURE ====================
def make_toc_page():
    return f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Workbench.png")}" class="chapter-icon" alt="Workbench"> SETTLEMENT OPERATING ARCHITECTURE</h2>
        <span class="role-pill">CORE LAWS &bull; LOGISTICS</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">KEY FOUNDATIONS:</span>
          {slot("Workbench.png", "Bench", True, 44)}
          {slot("Forge.png", "Forge", True, 44)}
          {slot("Chest.png", "Chest", False, 44)}
          {slot("Sign.png", "Routing", False, 44)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">LOGISTICS ARRAYS:</span>
          {slot("Cart.png", "Cart", False, 44)}
          {slot("Hearth.png", "Shelter", False, 44)}
          {slot("Dragon_Bed.png", "Bed", False, 44)}
          {slot("Hugin.png", "Hugin", False, 44)}
        </div>
      </div>

      <div class="split-container">
        <!-- Left Column: Core Laws -->
        <div class="col">
          <div class="tablet">
            <h3 class="cyan">WORK POST LAWS &amp; BOUNDARIES</h3>
            <ul>
              <li>
                <strong>25-METER SPHERICAL OPERATING POST</strong>
                Each role is bounded to a 25m radius sphere centered on its Work Post totem. Workers evaluate resources, benches, and storage exclusively inside this territory.
              </li>
              <li>
                <strong>TOOL TIER VS MATERIAL HARDNESS</strong>
                Workers automatically scan local containers and their inventory for required tool tiers (e.g., Bronze Pickaxe for Iron, Fine Bow for needle arrows).
              </li>
              <li>
                <strong>10% DURABILITY REPAIR CYCLE</strong>
                When equipped tool health drops below 10%, the worker immediately halts extraction and navigates to the nearest Forge or Workbench for instant restoration.
              </li>
              <li>
                <strong>SHELTER, REST &amp; COMFORT DYNAMICS</strong>
                Workers assigned to covered beds gain stamina regeneration buffs and sleep through stormy nights, resuming labor at sunrise.
              </li>
            </ul>
          </div>

          <div class="tablet">
            <h3 class="gold">CONTAINER ROUTING SYNTAX [TAGS]</h3>
            <ul>
              <li>
                <strong>[WOOD] &amp; [FINEWOOD]</strong>
                Lumberjacks deposit chopped round logs, core wood, and fine wood into designated chests.
              </li>
              <li>
                <strong>[ORE], [COAL] &amp; [STONE]</strong>
                Miners dump raw ores and stone chunks; Smelters pull fuel and charge hoppers automatically.
              </li>
              <li>
                <strong>[PRODUCE], [SEED] &amp; [MEAT]</strong>
                Farmers sort vegetables into larders and seeds into planting bins; Ranchers route cuts to cold store.
              </li>
              <li>
                <strong>[INGOT] &amp; [TOOLS]</strong>
                Foundrymen store refined bars; Craftsmen pull metals to maintain citadel fortresses.
              </li>
            </ul>
          </div>
        </div>

        <!-- Right Column: Codex Table of Chapters -->
        <div class="col">
          <div class="tablet">
            <h3 class="green">CODEX TABLE OF CHAPTERS</h3>
            <ul>
              <li>
                <strong>PART I: THE LUMBERJACK (PAGE 3)</strong>
                Forester &bull; Precision Tree Felling &bull; Log Bucking &bull; 50x Wood Stacking.
              </li>
              <li>
                <strong>PART II: THE MINER (PAGE 4)</strong>
                Quarryman &bull; Deep Vein Excavation &bull; Hoe Planing &bull; Ore Extraction.
              </li>
              <li>
                <strong>PART III: THE FARMER (PAGE 5)</strong>
                Cultivator &bull; Soil Tilling &bull; Crop Harvesting &bull; Apiary Honey Harvest.
              </li>
              <li>
                <strong>PART IV: THE RANCHER (PAGE 6)</strong>
                Herdsman &bull; Beast Taming &bull; Enclosure Feeding &bull; Selective Herd Culling.
              </li>
              <li>
                <strong>PART V: THE SMELTER (PAGE 7)</strong>
                Foundryman &bull; Charcoal Kiln Pyrolysis &bull; Ore Smelting &bull; Blast Furnace.
              </li>
              <li>
                <strong>PART VI: THE COOK (PAGE 8)</strong>
                Chef &bull; Spit Roasting &bull; Cauldron Feasts &bull; Fermentation Mead Aging.
              </li>
            </ul>
          </div>

          <div class="tablet">
            <h3 class="orange">EXPEDITION &amp; CITADEL DOMAINS</h3>
            <ul>
              <li>
                <strong>PART VII: THE CRAFTSMAN (PAGE 9)</strong>
                Master Mason &bull; Perimeter Repair Patrol &bull; Zero-Cost Tool Restoration.
              </li>
              <li>
                <strong>PART VIII: THE FISHER (PAGE 10)</strong>
                Coastal Angler &bull; Shoreline Casting &bull; Tidal Drift &bull; Pier Fillet Station.
              </li>
              <li>
                <strong>PART IX: THE HAULER (PAGE 11)</strong>
                Logistics Porter &bull; Cart Freight &bull; Ground Vacuum &bull; Tag Routing.
              </li>
              <li>
                <strong>PART X: THE SENTRY (PAGE 12)</strong>
                Stronghold Guard &bull; Watchtower Vigil &bull; Bow Volleys &bull; Shield Intercept.
              </li>
              <li>
                <strong>PART XI: CONFIGURATION MATRIX (PAGE 13)</strong>
                Troubleshooting &bull; valheimworkers.cfg &bull; Performance &bull; Console Commands.
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>CHAPTER INDEX &bull; SYSTEM FOUNDATIONS</span>
      <span>PAGE 2</span>
    </div>
  </div>
"""

# ==================== PAGE 13: CONFIGURATION & TROUBLESHOOTING ====================
def make_troubleshooting_page():
    return f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic("Hugin.png")}" class="chapter-icon" alt="Hugin"> PART XI: CONFIGURATION &amp; DIAGNOSTICS</h2>
        <span class="role-pill">MATRIX &bull; VALHEIMWORKERS.CFG</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">ADMIN CONTROLS:</span>
          {slot("Hammer.png", "Config", True, 44)}
          {slot("Sign.png", "Regex", True, 44)}
          {slot("Workbench.png", "Mesh", False, 44)}
          {slot("Forge.png", "Durability", False, 44)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">DIAGNOSTIC STATUS:</span>
          {slot("Chest.png", "Storage", False, 44)}
          {slot("Hearth.png", "Warmth", False, 44)}
          {slot("Cart.png", "Pathing", False, 44)}
          {slot("Hugin.png", "Hugin", False, 44)}
        </div>
      </div>

      <div class="split-container">
        <!-- Left Column: Troubleshooting -->
        <div class="col">
          <div class="tablet">
            <h3 class="cyan">TACTICAL AI TROUBLESHOOTING</h3>
            <ul>
              <li>
                <strong>WORKER STUCK IN PLACE (PATHING DEADLOCK)</strong>
                Use a Hoe to smooth abrupt cliff edges. Ensure doorways are at least 2m wide and stairs have no steep obstructions.
              </li>
              <li>
                <strong>WORKER IDLE ("NO WORK AVAILABLE")</strong>
                Confirm target nodes (rocks, crops, trees) are within the 25m sphere and required tool tiers are available in chests.
              </li>
              <li>
                <strong>CONTAINER FULL DEADLOCK</strong>
                Workers halt extraction if matching chests are full. Enable 50x resource piles or add extra chests tagged with [Tag].
              </li>
              <li>
                <strong>WORKBENCH LEVEL INSUFFICIENT</strong>
                High-tier tool repairs require upgraded station levels. Place Anvils, Coolers, and Chopping Blocks nearby.
              </li>
            </ul>
          </div>

          <div class="tablet">
            <h3 class="gold">HUGIN'S GOLDEN RULES OF BASE DESIGN</h3>
            <ul>
              <li>
                <strong>CENTRAL LOGISTICS HUB</strong>
                Place Hauler depots in the geographic center of your base to minimize transfer distance between quadrants.
              </li>
              <li>
                <strong>RAMPED ROADWAYS</strong>
                Pave dirt roads into flat stone or leveled soil paths; carts move 40% faster and rarely lose traction on ramps.
              </li>
              <li>
                <strong>OVERLAPPING WATCHTOWERS</strong>
                Build guard towers at perimeter corners so Sentry bow sightlines intersect, eliminating all dead zones.
              </li>
            </ul>
          </div>
        </div>

        <!-- Right Column: Config Matrix -->
        <div class="col">
          <div class="tablet">
            <h3 class="green">CONFIGURATION MATRIX (valheimworkers.cfg)</h3>
            <ul>
              <li>
                <strong>ScanIntervalSeconds = 2.5</strong>
                Evaluation tick frequency in seconds. Lower values increase responsiveness; higher values optimize potato CPUs.
              </li>
              <li>
                <strong>MaxWorkersPerZone = 12</strong>
                Caps total concurrent autonomous workers permitted inside a single active 64m world zone to protect server FPS.
              </li>
              <li>
                <strong>PathingMeshDetail = High</strong>
                Controls navmesh resolution around player structures, doors, and complex terrain modifications.
              </li>
              <li>
                <strong>AutoDepositOverflow = True</strong>
                Allows Lumberjacks and Miners to erect physical 50x wood and stone piles when storage boxes are capped.
              </li>
              <li>
                <strong>ToolDurabilityThreshold = 0.10</strong>
                Percentage (10%) at which workers disengage from tasks to seek repair at a Workbench or Forge.
              </li>
            </ul>
          </div>

          <div class="tablet">
            <h3 class="orange">CONSOLE COMMANDS (F5)</h3>
            <ul>
              <li>
                <strong>worker_debug on/off</strong>
                Toggles visual debug rendering of the 25m operating sphere, task raycasts, and pathing nodes.
              </li>
              <li>
                <strong>worker_reset</strong>
                Forces all nearby workers to drop current tasks, re-scan inventory, and re-bind to valid work posts.
              </li>
              <li>
                <strong>worker_status</strong>
                Prints a full diagnostic report of nearby worker inventories, stamina, and current AI states to the console.
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>VALHEIMWORKERS v0.3.0 &bull; CONFIGURATION MATRIX</span>
      <span>PAGE 13</span>
    </div>
  </div>
"""

print("Building pages array...")
pages = []

# Page 1: Cover
pages.append(make_cover_page())

# Page 2: TOC & Principles
pages.append(make_toc_page())

# Page 3: Part I: The Lumberjack
pages.append(make_worker_page(
    page_num=3,
    part_roman="PART I",
    role_title="THE LUMBERJACK",
    role_sub="FORESTER &amp; ARBORIST",
    role_pill="FORESTRY &bull; TIMBER",
    icon_main="Iron_Axe.png",
    loadout_slots=[("Iron_Axe.png", "Axe", True), ("Stone_Axe.png", "Flint", False), ("Torch.png", "Torch", False), ("Wood.png", "Cargo", False)],
    yield_slots=[("Wood.png", "Wood", False), ("Fine_wood.png", "Fine", False), ("Core_wood.png", "Core", False), ("Beech_Seeds.png", "Beech", False), ("Birch_Seeds.png", "Birch", False), ("Wood_Pile.png", "Pile", False)],
    cycle_items=[
        ("1. ARBORIST PROSPECTING: ", "Surveys the 25m station boundary, validating equipped axe tier against wood hardness (Birch/Oak need Bronze+)."),
        ("2. DIRECTIONAL TREE FELLING: ", "Strikes trunks at ergonomic angles to fell timber into open ground, preventing collateral damage to base roofs."),
        ("3. LOG BUCKING &amp; STUMP CLEARING: ", "Bucks fallen trunks into portable timber chunks and thoroughly excises tree stumps to clear ground."),
        ("4. NURSERY REPLANTING GRID: ", "Deploys Beech seeds, Birch seeds, and Fir cones at 3m spacing to establish perpetual sustainable harvest groves.")
    ],
    advice_items=[
        ("STUMP INTEGRITY: ", "Stumps never regrow. Workers ensure 100% stump clearing so new saplings can be rooted on flat ground."),
        ("LOG COLLISION DEFENSE: ", "Felling massive trees near buildings can damage timber beams; workers path around walls before cutting."),
        ("AXE TIER VALIDATION: ", "Stone axes cannot fell Fine Wood (Birch/Oak). Equip at least a Bronze Axe for advanced biomes.")
    ],
    cap_items=[
        ("[F01] PRECISION TREE FELLING: ", "Shatters Beech, Birch, Fir, Pine, and Oak trunks using appropriate tool tiers."),
        ("[F02] TIMBER LOG BUCKING: ", "Splits downed logs into regular Wood, Core Wood, and Fine Wood bundles."),
        ("[F03] STUMP REMOVAL PASS: ", "Chops underground stump colliders clean from soil to maximize settlement building area."),
        ("[F04] AUTOMATED ARBOR NURSERY: ", "Re-plants tree seeds in aligned grid patterns to regenerate timber supply within the base."),
        ("[F05] 50X WOOD STACK OVERFLOW: ", "Constructs 50x wood_pile structures automatically when settlement storage boxes are full.")
    ],
    synergy_items=[
        ("SMELTER CHARCOAL PIPELINE: ", "Feeds continuous timber stacks directly to Kilns for unlimited smelting fuel."),
        ("CRAFTSMAN TIMBER DEPOT: ", "Delivers structural Core Wood and Fine Wood directly to the Carpenter for citadel defense."),
        ("HAULER WAGON TRANSFER: ", "Drops heavy lumber stacks into parked wagons for mass player transit.")
    ]
))

# Page 4: Part II: The Miner
pages.append(make_worker_page(
    page_num=4,
    part_roman="PART II",
    role_title="THE MINER",
    role_sub="QUARRYMAN &amp; EXCAVATOR",
    role_pill="MINING &bull; EXTRACTION",
    icon_main="Antler_Pickaxe.png",
    loadout_slots=[("Antler_Pickaxe.png", "Pickaxe", True), ("Hoe.png", "Hoe", True), ("Torch.png", "Torch", False), ("Stone.png", "Cargo", False)],
    yield_slots=[("Stone.png", "Stone", False), ("Copper_Ore.png", "Copper", False), ("Tin_Ore.png", "Tin", False), ("Iron_Scrap.png", "Iron", False), ("Silver_Ore.png", "Silver", False), ("Stone_Pile.png", "Pile", False)],
    cycle_items=[
        ("1. GEODE PROSPECTING: ", "Scans 25m sphere around Work Post; evaluates pickaxe tier vs rock hardness."),
        ("2. OVERBURDEN EXCAVATION: ", "Chips away surface crags and topsoil to expose buried ore colliders."),
        ("3. HOE PLANING PASS: ", "Swaps to Hoe to flatten jagged pit bottoms, creating ramped cart paths."),
        ("4. FORGE REPAIR RUN: ", "Navigates to camp Forge when pickaxe durability drops below 10%.")
    ],
    advice_items=[
        ("PROXIMITY INTEGRITY: ", "Place an upgraded Workbench and Forge near the quarry to avoid walking delays."),
        ("SIGN BRACKET ROUTING: ", "Mark foundry strongboxes with [Stone] and [Ore] for automated sorting."),
        ("BEDROCK BOUNDARY: ", "Miners safely stop downward digging at the world bedrock floor (-8m).")
    ],
    cap_items=[
        ("[F06] SURFACE QUARRYING: ", "Shatters surface boulders, loose rocks, and flint nodes with pickaxe."),
        ("[F07] DEEP VEIN EXCAVATION: ", "Extracts Copper, Tin, Iron Scrap, and Silver veins with tool tier checks."),
        ("[F08] EVEN FIELD (HOE LEVELING): ", "Equips Hoe to eliminate steep pit traps and build smooth transit roads."),
        ("[F05] 50X OVERFLOW MASONRY PILES: ", "Constructs 50x stone_pile structures when camp storage chests are full.")
    ],
    synergy_items=[
        ("FOUNDRY FEEDER: ", "Direct conduit of raw copper, tin, and scrap iron to the Smelter quadrant."),
        ("MASONRY STOCKPILER: ", "Provides vast 50x stone piles to Craftsmen for defensive stone walls."),
        ("VAGON CART LOADING: ", "Drops heavy ore stacks into parked carts for unified player hauling.")
    ]
))

# Page 5: Part III: The Farmer
pages.append(make_worker_page(
    page_num=5,
    part_roman="PART III",
    role_title="THE FARMER",
    role_sub="HARVESTER &amp; CULTIVATOR",
    role_pill="AGRICULTURE &bull; CROPS",
    icon_main="Cultivator.png",
    loadout_slots=[("Cultivator.png", "Cultivator", True), ("Hoe.png", "Hoe", False), ("Torch.png", "Torch", False), ("Carrot.png", "Seeds", False)],
    yield_slots=[("Carrot.png", "Carrot", False), ("Turnip.png", "Turnip", False), ("Onion.png", "Onion", False), ("Barley.png", "Barley", False), ("Flax.png", "Flax", False), ("Honey.png", "Honey", False)],
    cycle_items=[
        ("1. SOIL TILLING PASS: ", "Equips Cultivator to convert wild grass within 25m into aerated arable soil."),
        ("2. MATURITY SCANNING: ", "Inspects crop growth stages; bypasses immature sprouts to harvest 100% ripe vegetables."),
        ("3. GRID REPLANTING ROTATION: ", "Maintains 50/50 seed-to-crop ratio, planting seed crops to ensure permanent replenishment."),
        ("4. LARDER &amp; APIARY SWEEP: ", "Collects honey from beehives and deposits vegetables into tagged pantry bins.")
    ],
    advice_items=[
        ("SPACING ACCURACY: ", "AI enforces 1.2m crop grid spacing so vegetables never rot from overcrowding."),
        ("BEEHIVE AIRSPACE: ", "Keep hives on fence posts with open sky so the 'Bees are Happy' state remains active."),
        ("BIOME RESTRICTIONS: ", "Barley and Flax only grow in the Plains biome; set Plains farm outpost work posts.")
    ],
    cap_items=[
        ("[F09] PRECISION HARVESTING: ", "Picks ripe Carrots, Turnips, Onions, Barley, and Flax without damaging immature nodes."),
        ("[F10] PERPETUAL SEED CYCLE: ", "Calculates seed reserves to auto-replant seed flowers before filling cooking larders."),
        ("[F11] SOIL TILLING &amp; CULTIVATION: ", "Tills virgin earth and maintains dark soil quality within agricultural plots."),
        ("[F12] APIARY HONEY EXTRACTION: ", "Extracts comb honey from hives every morning without agitating worker bees."),
        ("[F13] WILD BERRY &amp; HERB FORAGING: ", "Harvests wild Raspberries, Blueberries, Cloudberries, and Dandelions in range.")
    ],
    synergy_items=[
        ("COOK'S CAULDRON LARDER: ", "Feeds steady crops and honey directly into brewing cauldrons and feast pots."),
        ("RANCHER FEED TROUGH CONDUIT: ", "Transfers root crops to animal enclosures to trigger continuous taming and breeding."),
        ("WEAVER FLAX SUPPLY: ", "Supplies raw Flax stalks directly to Craftsman spinning wheels for Linen Thread.")
    ]
))

# Page 6: Part IV: The Rancher
pages.append(make_worker_page(
    page_num=6,
    part_roman="PART IV",
    role_title="THE RANCHER",
    role_sub="BEAST TAMER &amp; HERDSMAN",
    role_pill="HUSBANDRY &bull; LIVESTOCK",
    icon_main="Raw_Meat.png",
    loadout_slots=[("Iron_Sword.png", "Knife", True), ("Torch.png", "Torch", False), ("Carrot.png", "Feed", False), ("Raw_Meat.png", "Meat", False)],
    yield_slots=[("Raw_Meat.png", "Meat", False), ("Leather_Scraps.png", "Leather", False), ("Bone_Fragments.png", "Bone", False), ("Boar.png", "Boar", False), ("Wolf.png", "Wolf", False), ("Lox.png", "Lox", False)],
    cycle_items=[
        ("1. ENCLOSURE PATROL: ", "Inspects fencing, gates, and feeding zones within 25m for wild and tamed beasts."),
        ("2. TROUGH FEEDING: ", "Distributes root crops to Boars, raw meat to Wolves, and Cloudberries/Barley to Lox herds."),
        ("3. ACCLIMATION &amp; TAMING: ", "Monitors calming status; ensures wild beasts remain unspotted while acclimating."),
        ("4. SELECTIVE HERD CULLING: ", "Harvests mature surplus animals beyond the set breeding limit while preserving pairs.")
    ],
    advice_items=[
        ("STAR TIER PRESERVATION: ", "The Rancher prioritizes breeding 1-Star and 2-Star stock, never culling highest-tier breeders."),
        ("FIRE SCARE MITIGATION: ", "Keep torches and campfires away from boar pens to prevent panic that halts taming."),
        ("BREEDING CEILING: ", "Set enclosure capacity caps in config to prevent herd overcrowding and server FPS drop.")
    ],
    cap_items=[
        ("[F14] WILD BEAST ACCLIMATION: ", "Tames wild boars, wolves, and lox using stealth approach and steady feed drops."),
        ("[F15] BREEDING &amp; NURSERY CARE: ", "Maintains food in pens to sustain continuous hearts and newborn litters."),
        ("[F16] SELECTIVE HERD CULLING: ", "Equips butcher knife to cull non-starred surplus adults when herd exceeds limit."),
        ("[F17] LEATHER &amp; PELT REAPING: ", "Collects hides, entrails, and raw meats directly into cold-store storage bins.")
    ],
    synergy_items=[
        ("ARMORER LEATHER PIPELINE: ", "Supplies continuous leather scraps and pelts for high-tier armor crafting."),
        ("CHEF'S COLD-STORE SUPPLY: ", "Provides prime raw meats directly into the Cook's butchery lockers."),
        ("WAR PACK BREEDING: ", "Breeds high-damage 2-star wolves for Stronghold perimeter sentry defense.")
    ]
))

# Page 7: Part V: The Smelter
pages.append(make_worker_page(
    page_num=7,
    part_roman="PART V",
    role_title="THE SMELTER",
    role_sub="FOUNDRYMAN &amp; CHARCOAL BURNER",
    role_pill="METALLURGY &bull; INGOTS",
    icon_main="Smelter.png",
    loadout_slots=[("Hammer.png", "Tongs", True), ("Hoe.png", "Hoe", False), ("Torch.png", "Torch", False), ("Coal.png", "Fuel", False)],
    yield_slots=[("Charcoal_Kiln.png", "Kiln", False), ("Smelter.png", "Smelter", False), ("Blast_Furnace.png", "Blast", False), ("Coal.png", "Coal", False), ("Copper.png", "Copper", False), ("Iron.png", "Iron", False)],
    cycle_items=[
        ("1. KILN PYROLYSIS: ", "Feeds round wood and pine into Kilns; converts raw timber into dense charcoal chunks."),
        ("2. FOUNDRY FUEL FEEDING: ", "Loads 25x coal into Smelters and Blast Furnaces to maintain optimal refining heat."),
        ("3. ORE CHARGING PASS: ", "Feeds raw Copper, Tin, Iron, and Silver into smelter hoppers in batch ratios."),
        ("4. SLAG TAPPING &amp; INGOT STACKING: ", "Gathers cooling bars from run-off troughs and stocks categorized ingot chests.")
    ],
    advice_items=[
        ("CHEST INGESTION RANGE: ", "Place raw ore and wood boxes within 8m of smelters for rapid cycle turnaround."),
        ("BLAST FURNACE BIOME REQUIREMENT: ", "Blast furnaces require Artisan Tables (Dragon Tear) for Black Metal and Flametal."),
        ("COAL BALANCING RULE: ", "Dedicate at least two Charcoal Kilns per active Smelter to prevent fuel starvation.")
    ],
    cap_items=[
        ("[F18] KILN TIMBER CONVERSION: ", "Fires up to 25 wood logs per kiln cycle for constant coal generation."),
        ("[F19] TIER-1 SMELTER OPERATION: ", "Refines raw Copper Ore, Tin Ore, and Iron Scrap into pristine forged metal bars."),
        ("[F20] BLAST FURNACE METALLURGY: ", "Smelts high-tier Black Metal Scrap with precision fuel consumption."),
        ("[F21] AUTOMATED INGOT CONSOLIDATION: ", "Transfers hot bars into labeled [Ingot] strongboxes without player oversight.")
    ],
    synergy_items=[
        ("MINER ORE INGESTION: ", "Pulls directly from Miner ore drop-boxes, forming an autonomous metallurgy loop."),
        ("CRAFTSMAN FORGE SUPPLY: ", "Stocks refined Bronze, Iron, and Silver directly into weaponsmithing lockers."),
        ("HAULER DOCKING STATION: ", "Prepares bulk metal bars for cart loading and transport to coastal ports.")
    ]
))

# Page 8: Part VI: The Cook
pages.append(make_worker_page(
    page_num=8,
    part_roman="PART VI",
    role_title="THE COOK",
    role_sub="CHEF &amp; MEAD BREWER",
    role_pill="CULINARY &bull; POTIONS",
    icon_main="Cauldron.png",
    loadout_slots=[("Iron_Sword.png", "Ladle", True), ("Campfire.png", "Spit", False), ("Honey.png", "Honey", False), ("Deer_Stew.png", "Stew", False)],
    yield_slots=[("Cooking_Station.png", "Station", False), ("Iron_Cooking_Station.png", "Iron Spit", False), ("Cauldron.png", "Cauldron", False), ("Fermenter.png", "Fermenter", False), ("Deer_Stew.png", "Stew", False), ("Sausages.png", "Sausage", False)],
    cycle_items=[
        ("1. PANTRY INGREDIENT REQUISITION: ", "Pulls meats from cold-store and crops from larders matching known recipes."),
        ("2. OPEN-FLAME SPIT ROASTING: ", "Places raw meat on spit roasters; flips and removes cuts precisely before burning."),
        ("3. CAULDRON BATCH STEWING: ", "Simmers Deer Stew, Sausages, Serpent Stew, and Turnip Soup in massive kettle batches."),
        ("4. MEAD MUST BREWING &amp; AGING: ", "Boils honey bases and loads fermenters to age Healing, Stamina, and Frost meads.")
    ],
    advice_items=[
        ("ZERO CHAR LOGIC: ", "The Cook's AI guarantees 100% perfect timing, producing zero burnt coal from meats."),
        ("CAULDRON LEVELING: ", "Install Spice Racks, Butcher Tables, and Pots to unlock end-game recipes for the Cook."),
        ("RATION DISTRIBUTOR: ", "Place a [Food] chest near the main hall entrance for workers and players to claim rations.")
    ],
    cap_items=[
        ("[F22] SPIT ROAST TIMING: ", "Roasts deer meat, boar meat, and serpent cuts with zero burning or wastage."),
        ("[F23] CAULDRON GOURMET BREWING: ", "Prepares complex multi-ingredient stews and hearty soups on cauldron fires."),
        ("[F24] FERMENTATION MEAD CASKS: ", "Fills barrel fermenters with medicinal mead musts, harvesting aged potion casks."),
        ("[F25] SETTLEMENT RATION ALLOCATION: ", "Stocks player dining tables with balanced Health and Stamina food sets.")
    ],
    synergy_items=[
        ("FARMER VEGETABLE PIPELINE: ", "Draws carrots, onions, and turnips directly from harvest bins for soup bases."),
        ("RANCHER MEAT HARVEST CONDUIT: ", "Transforms fresh slaughter cuts into high-stamina provisions within minutes."),
        ("SENTRY VIGOR BUFFS: ", "Feeds defensive guards hearty stews to maximize HP and poise against troll raids.")
    ]
))

# Page 9: Part VII: The Craftsman
pages.append(make_worker_page(
    page_num=9,
    part_roman="PART VII",
    role_title="THE CRAFTSMAN",
    role_sub="ARMORER, WEAPONSMITH &amp; MASON",
    role_pill="CRAFTING &bull; FORTIFICATION",
    icon_main="Hammer.png",
    loadout_slots=[("Hammer.png", "Hammer", True), ("Hoe.png", "Hoe", False), ("Torch.png", "Torch", False), ("Workbench.png", "Bench", False)],
    yield_slots=[("Workbench.png", "Bench", False), ("Forge.png", "Forge", False), ("Iron_Helmet.png", "Armor", False), ("Iron_Sword.png", "Sword", False), ("Banded_Shield.png", "Shield", False), ("Stone_Pile.png", "Masonry", False)],
    cycle_items=[
        ("1. PERIMETER DAMAGE AUDIT: ", "Patrols palisades, roof beams, and gates to detect weather decay or combat damage."),
        ("2. STRUCTURAL MALLET PASS: ", "Equips Hammer to restore walls, stairs, and doors back to 100% maximum durability."),
        ("3. TOOL RESTORATION RUN: ", "Checks settlement tool racks; restores broken pickaxes, axes, and bows for workers."),
        ("4. ADVANCED REINFORCEMENT: ", "Upgrades wooden palisades to stone walls and constructs reinforced citadel battlements.")
    ],
    advice_items=[
        ("WORKBENCH COVERAGE: ", "Workers require an active Workbench or Stonecutter radius to execute hammer repairs."),
        ("FREE TOOL REPAIR: ", "Repairs cost zero resources in Valheim; keep crafting tables close to speed up cycles."),
        ("RAIN DECAY PREVENTION: ", "The Craftsman automatically repairs rain-exposed roofless beams to stop rot at 50% HP.")
    ],
    cap_items=[
        ("[F26] ARCHITECTURAL REPAIR PATROL: ", "Scans 25m perimeter; repairs rain rot and enemy siege damage on buildings."),
        ("[F27] ZERO-COST TOOL RESTORATION: ", "Services all dull worker equipment at Workbenches and Forges autonomously."),
        ("[F28] DEFENSIVE CITADEL REINFORCEMENT: ", "Erects sharp stake spikes, defensive barricades, and arrow slits."),
        ("[F29] MATERIAL PROCESSING &amp; CUTTING: ", "Operates the Stonecutter and Spinning Wheel to craft stone bricks and linen.")
    ],
    synergy_items=[
        ("FOUNDRY METAL FLOW: ", "Uses fresh ingots to craft replacement armor and upgraded weapons for Sentries."),
        ("LUMBERJACK REPAIR CONDUIT: ", "Maintains 100% axe durability so timber clearing runs never halt for repairs."),
        ("MINER STONE SUPPLY: ", "Consumes 50x stone piles to build impenetrable high-tier granite fortresses.")
    ]
))

# Page 10: Part VIII: The Fisher
pages.append(make_worker_page(
    page_num=10,
    part_roman="PART VIII",
    role_title="THE FISHER",
    role_sub="ANGLER &amp; COASTAL FORAGER",
    role_pill="MARITIME &bull; SEAFOOD",
    icon_main="Karve.png",
    loadout_slots=[("Iron_Sword.png", "Harpoon", True), ("Torch.png", "Torch", False), ("Coins.png", "Bait", False), ("Karve.png", "Boat", False)],
    yield_slots=[("Neck.png", "Neck", False), ("Raw_Meat.png", "Fish", False), ("Amber.png", "Amber", False), ("Ruby.png", "Ruby", False), ("Coins.png", "Coins", False), ("Karve.png", "Tide", False)],
    cycle_items=[
        ("1. COASTAL SHORELINE SURVEY: ", "Patrols ocean shorelines, docks, and rocky shallows scanning for active fish shoals."),
        ("2. CASTING &amp; REEL TIMING: ", "Casts line into target eddies; applies stamina-efficient reel tension on strikes."),
        ("3. TIDAL FORAGING RUN: ", "Gathers shoreline flotsam, washed-up seaweed, flint nodes, and trapped tidal creatures."),
        ("4. DOCK CATCH FILLETING: ", "Transfers coastal bounty to pier fillet tables, producing clean raw fish portions.")
    ],
    advice_items=[
        ("PIER CONSTRUCTION: ", "Build an extended wooden pier out over deep water to give the Fisher access to ocean shoals."),
        ("BAIT REQUIREMENTS: ", "Stock [Bait] chests near the dock with specialized trader bait tiers for deep-water catches."),
        ("STORM SURGE YIELDS: ", "Ocean fish strand themselves during high-tide sea storms; tidal yields triple in stormy weather.")
    ],
    cap_items=[
        ("[F30] SHORELINE &amp; DOCK ANGLING: ", "Hooks Perch, Pike, Trollfish, and Coral Cod from coastal waters and wharves."),
        ("[F31] INTERTIDAL DRIFT FORAGING: ", "Sweeps tidal shallows for washed-up oysters, flint, amber beads, and flotsam."),
        ("[F32] CATCH FILLETING &amp; PRESERVATION: ", "Processes whole ocean fish into standardized Raw Fish meat packs for cooking.")
    ],
    synergy_items=[
        ("COOK'S WRAP INGREDIENTS: ", "Supplies endless Raw Fish portions for high-tier Plains Fish Wraps."),
        ("TRADER WEALTH ACCUMULATION: ", "Pans amber and coins from shoreline surf to fund trader bait and eggs."),
        ("HAULER DOCK INTEGRATION: ", "Docks cart at pier head for automated fish transfer directly to stronghold kitchens.")
    ]
))

# Page 11: Part IX: The Hauler
pages.append(make_worker_page(
    page_num=11,
    part_roman="PART IX",
    role_title="THE HAULER",
    role_sub="LOGISTICS PORTER &amp; CART DRIVER",
    role_pill="LOGISTICS &bull; FREIGHT",
    icon_main="Cart.png",
    loadout_slots=[("Cart.png", "Cart", True), ("Hammer.png", "Hammer", False), ("Torch.png", "Torch", False), ("Chest.png", "Pack", False)],
    yield_slots=[("Cart.png", "Cart", False), ("Chest.png", "Chest", False), ("Reinforced_Chest.png", "Iron Chest", False), ("Black_Metal_Chest.png", "Vault", False), ("Sign.png", "Sign", False), ("Wood_Pile.png", "Yard", False)],
    cycle_items=[
        ("1. FIELD VACUUM SWEEP: ", "Scans base grounds; vacuums dropped timber chunks, loose rocks, and stray monster loot."),
        ("2. CART HITCHING &amp; LOADING: ", "Hitches to nearby wooden carts, filling mobile beds with heavy ores and stones."),
        ("3. SIGN BRACKET LOGISTICS: ", "Paths through depots, sorting gathered stacks into chests marked with [Item] tags."),
        ("4. SUPPLY DEPOT BALANCING: ", "Equalizes inventory stacks across multiple overflow boxes to prevent bin clogs.")
    ],
    advice_items=[
        ("ROADWAY PLANING: ", "Use a Hoe to flatten 2m wide transit roads; haulers move 40% faster on stone paths."),
        ("STAIRWAY RAMPS: ", "Haulers cannot pull carts up standard steep stairs; build 26-degree roof or wood ramps."),
        ("SIGN SYNTAX PRECISION: ", "Brackets must match item names: [Wood], [Stone], [Ore], [Coal], [Produce], [Meat].")
    ],
    cap_items=[
        ("[F33] SETTLEMENT GROUND VACUUM: ", "Sweeps all loose dropped items within 25m, preventing visual clutter and lag."),
        ("[F34] TWO-WHEELED CART HAULING: ", "Hitches to and maneuvers heavy carts to transport up to 1000kg of raw materials."),
        ("[F35] BRACKET CONTAINER ROUTING: ", "Interprets sign tags to sort mixed items into designated containers with 100% accuracy."),
        ("[F36] INTER-DOMAIN DEPOT CONDUIT: ", "Relocates processed goods from production sheds to central stronghold vaults.")
    ],
    synergy_items=[
        ("THE VASCULAR BASE SYSTEM: ", "Connects all 9 production domains, freeing civilian workers from walking travel."),
        ("MINER CART INTEGRATION: ", "Empties mining pit chests directly, keeping excavators continuously swinging pickaxes."),
        ("FOUNDRY FEEDER LINK: ", "Transfers fuel and ores from distant yards directly into smelter hopper bins.")
    ]
))

# Page 12: Part X: The Sentry
pages.append(make_worker_page(
    page_num=12,
    part_roman="PART X",
    role_title="THE SENTRY",
    role_sub="STRONGHOLD GUARD &amp; PATROL",
    role_pill="SECURITY &bull; DEFENSE",
    icon_main="Finewood_Bow.png",
    loadout_slots=[("Finewood_Bow.png", "Bow", True), ("Iron_Sword.png", "Sword", True), ("Banded_Shield.png", "Shield", True), ("Fire_Arrow.png", "Arrows", False)],
    yield_slots=[("Greydwarf.png", "Greydwarf", False), ("Skeleton.png", "Skeleton", False), ("Troll.png", "Troll", False), ("Bone_Fragments.png", "Bones", False), ("Resin.png", "Resin", False), ("Valkyrie.png", "Honor", False)],
    cycle_items=[
        ("1. WATCHTOWER SENTRY VIGIL: ", "Mans high defensive battlements, keeping 360-degree vision over outer wilderness."),
        ("2. THREAT RECOGNITION: ", "Identifies Greydwarf scouts, Trolls, and nocturnal predators up to 40m away."),
        ("3. RANGED HARASSMENT ARCS: ", "Draws bows to suppress enemy targets with fire, flint, and needle arrow volleys."),
        ("4. PERIMETER MELEE INTERCEPT: ", "Swaps to shield and blade to engage breaches before monsters can touch civilian workers.")
    ],
    advice_items=[
        ("ELEVATED POSTS: ", "Station Sentries in covered watchtowers so they gain height advantage and stay sheltered from rain."),
        ("ARROW AMMUNITION BINS: ", "Place a [Arrows] chest in the guard tower to provide infinite autonomous quiver refills."),
        ("LIGHTING ADVANTAGE: ", "Place standing torches along perimeter fences to expose nocturnal attackers to sentry fire.")
    ],
    cap_items=[
        ("[F37] PERIMETER PATROL &amp; VIGIL: ", "Walks ramparts and gatehouses on programmed defensive patrol paths."),
        ("[F38] TACTICAL RANGED SUPPRESSION: ", "Fires aimed bow volleys to neutralize incoming raid forces before they reach gates."),
        ("[F39] AGGRO INTERCEPTION &amp; SHIELD WALL: ", "Draws hostile focus away from civilian workers using aggressive melee blocking."),
        ("[F40] POST-RAID DROP SWEEPING: ", "Secures combat spoils (eyes, resin, bones, trophies) and dumps them in barracks.")
    ],
    synergy_items=[
        ("CIVILIAN WORKER DEFENSE: ", "Guarantees Farmers, Lumberjacks, and Miners can work without fear of random ambushes."),
        ("CRAFTSMAN ARMORY TIE-IN: ", "Receives freshly forged armor, bows, and shields directly from the Stronghold armorer."),
        ("RAID SPOILS SUPPLY: ", "Provides constant Grey Dwarf eyes and trophies to Cook and Craftsman for advanced recipes.")
    ]
))

# Page 13: Part XI: Troubleshooting & Config
pages.append(make_troubleshooting_page())

print(f"Total pages assembled: {len(pages)}")

# Assemble Full HTML Document
full_html = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>ValheimWorkers v0.3.0 - The Settlement Steward's Codex</title>
  <style>
{COMMON_STYLE}
  </style>
</head>
<body>
{SVG_FILTERS}
{"".join(pages)}
</body>
</html>
"""

print(f"Writing {HTML_OUT}...")
with open(HTML_OUT, "w", encoding="utf-8") as f:
    f.write(full_html)

# Also write to ValheimWorkers subdirectory
os.makedirs(os.path.dirname(SUBDIR_HTML), exist_ok=True)
with open(SUBDIR_HTML, "w", encoding="utf-8") as f:
    f.write(full_html)

print("Compiling PDF with Edge...")
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
print(f"Successfully compiled {PDF_OUT}!")

# Copy PDF to ValheimWorkers subdir
import shutil
shutil.copy2(PDF_OUT, SUBDIR_PDF)
print(f"Mirrored PDF to {SUBDIR_PDF}")

# Verify page count and render PNGs with pymupdf
doc = pymupdf.open(PDF_OUT)
print(f"PDF verified: {len(doc)} pages generated.")

# Render each page to PNG in docs/pages/
PAGES_PNG_DIR = os.path.join(DOCS_DIR, "pages")
os.makedirs(PAGES_PNG_DIR, exist_ok=True)

for i, page in enumerate(doc):
    pix = page.get_pixmap(dpi=150)
    png_path = os.path.join(PAGES_PNG_DIR, f"page_{i+1:02d}.png")
    pix.save(png_path)
    print(f"Rendered Page {i+1} -> {png_path}")

print("Master Guide Book successfully compiled and verified!")
