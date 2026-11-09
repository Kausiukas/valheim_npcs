import os, base64, subprocess, pymupdf

BASE_DIR = r"d:\valheim_mods"
DOCS_DIR = os.path.join(BASE_DIR, "docs")
ASSETS_DIR = os.path.join(DOCS_DIR, "assets")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
STONE_DIR = os.path.join(ASSETS_DIR, "stone")

def file_b64(path):
    with open(path, "rb") as f:
        return base64.b64encode(f.read()).decode("utf-8")

print("Encoding assets...")
stone_monolith_b64 = file_b64(os.path.join(STONE_DIR, "stone_monolith_bg.png"))
stone_tablet_b64 = file_b64(os.path.join(STONE_DIR, "stone_tablet_bg.png"))
stone_socket_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_bg.png"))
stone_socket_eq_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_equipped_bg.png"))
stone_crest_b64 = file_b64(os.path.join(STONE_DIR, "stone_cover_crest.png"))

norse_b64 = file_b64(os.path.join(FONTS_DIR, "Norse.otf"))
norsebold_b64 = file_b64(os.path.join(FONTS_DIR, "Norsebold.otf"))
averia_regular_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Regular.ttf"))
averia_bold_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Bold.ttf"))

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

def slot(icon_name, label, equipped=False, size=46):
    eq_cls = " equipped" if equipped else ""
    return f"""<div class="valheim-slot{eq_cls}" style="width:{size}px; height:{size}px;"><img src="{ic(icon_name)}" alt="{label}"><span class="slot-label">{label}</span></div>"""

# Shared SVG Filter Definitions for True V-Groove Carving
SVG_FILTERS = """
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
    <!-- 1. GOLD V-GROOVE CHISEL (Titles) -->
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

    <!-- 2. CYAN RUNIC CHISEL (Subheaders) -->
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

    <!-- 3. VERDIGRIS GREEN CHISEL -->
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

    <!-- 4. BODY TEXT CHISEL ETCH (15pt-17pt) -->
    <filter id="v-groove-body" x="-10%" y="-10%" width="120%" height="120%" color-interpolation-filters="sRGB">
      <feTurbulence type="fractalNoise" baseFrequency="0.16" numOctaves="2" seed="9" result="noise"/>
      <feDisplacementMap in="SourceGraphic" in2="noise" scale="1.0" xChannelSelector="R" yChannelSelector="G" result="wobbleSrc"/>
      <feDisplacementMap in="SourceAlpha" in2="noise" scale="1.0" xChannelSelector="R" yChannelSelector="G" result="wobbleAlpha"/>

      <feGaussianBlur in="wobbleAlpha" stdDeviation="0.6" result="b1"/>
      <feGaussianBlur in="wobbleAlpha" stdDeviation="1.3" result="b2"/>
      <feComposite in="b1" in2="b2" operator="arithmetic" k1="0" k2="0.6" k3="0.4" k4="0" result="heightMap"/>

      <feDiffuseLighting in="heightMap" surfaceScale="-8" diffuseConstant="1.25" lighting-color="#fff8eb" result="diffLight">
        <feDistantLight azimuth="315" elevation="42"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="wobbleSrc" operator="arithmetic" k1="1.05" k2="0.25" k3="0" k4="-0.05" result="shadedFill"/>
      <feComposite in="shadedFill" in2="wobbleAlpha" operator="in" result="clippedFill"/>

      <feOffset in="wobbleAlpha" dx="1.1" dy="1.1" result="shOff"/>
      <feComposite in="wobbleAlpha" in2="shOff" operator="out" result="topLeftCrescent"/>
      <feFlood flood-color="#020305" flood-opacity="0.92" result="blackFlood"/>
      <feComposite in="blackFlood" in2="topLeftCrescent" operator="in" result="innerRimShadow"/>

      <feOffset in="wobbleAlpha" dx="1.0" dy="1.0" result="outerLipOff"/>
      <feComposite in="outerLipOff" in2="wobbleAlpha" operator="out" result="outerBottomRightLip"/>
      <feFlood flood-color="#ffefc8" flood-opacity="0.5" result="lipFlood"/>
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
      height: 284mm;
      max-height: 284mm;
      overflow: hidden;
    }}
  }}

  * {{ box-sizing: border-box; margin: 0; padding: 0; }}

  .page {{
    height: 284mm;
    max-height: 284mm;
    display: flex;
    flex-direction: column;
    justify-content: flex-start;
    padding: 6mm 8mm 5mm 8mm;
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

  /* Elder Futhark rune ribbon at top */
  .page::before {{
    content: "ᚠ ᚢ ᚦ ᚨ ᚱ ᚲ ᚷ ᚹ ᚺ ᚾ ᛁ ᛃ ᛈ ᛇ ᛉ ᛋ ᛏ ᛒ ᛖ ᛗ ᛚ ᛜ ᛞ ᛟ";
    display: block;
    text-align: center;
    font-family: 'Norsebold', serif;
    font-size: 8.0pt;
    color: #9a7638;
    letter-spacing: 0.38em;
    margin-bottom: 4px;
    text-shadow: 0 1px 2px #000, 0 0 5px rgba(212,160,23,0.3);
  }}

  .content-container {{
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 6.0px;
    min-height: 0;
  }}

  .page-footer {{
    border-top: 1px solid #4D371E;
    padding-top: 3.0px;
    display: flex;
    justify-content: space-between;
    font-size: 8.5pt;
    color: #A3937F;
    font-family: 'Norsebold', serif;
    letter-spacing: 0.05em;
    margin-top: auto;
  }}

  /* Sunken Relic Sockets (46px - Big Viking Hands) */
  .valheim-slot {{
    background: radial-gradient(circle at 65% 65%, #0e1115 0%, #161a20 100%), url('data:image/png;base64,{stone_socket_b64}') no-repeat center/cover;
    border-top: 2px solid #0a0d10;
    border-left: 2px solid #0a0d10;
    border-bottom: 2px solid rgba(240, 215, 160, 0.38);
    border-right: 2px solid rgba(240, 215, 160, 0.28);
    border-radius: 5px;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
    box-shadow: inset 2.5px 2.5px 7px rgba(0,0,0,0.95), 0 3px 6px rgba(0,0,0,0.7);
  }}
  .valheim-slot.equipped {{
    border: 2px solid #38BDF8 !important;
    box-shadow: inset 0 0 10px rgba(56, 189, 248, 0.75), 0 0 10px rgba(56, 189, 248, 0.6), inset 2px 2px 5px #000 !important;
  }}
  .valheim-slot img {{
    width: 32px;
    height: 32px;
    object-fit: contain;
    filter: drop-shadow(0 2px 4px rgba(0,0,0,0.9));
  }}
  .valheim-slot .slot-label {{
    position: absolute;
    bottom: -15.0px;
    left: 50%;
    transform: translateX(-50%);
    font-size: 8.2pt;
    color: #EDE5D8;
    white-space: nowrap;
    font-family: 'Norsebold', serif;
    letter-spacing: 0.04em;
    text-shadow: 0 1px 2px #000, 0 0 3px #000;
  }}
  .valheim-slot.equipped::after {{
    content: "";
    position: absolute;
    top: -3px;
    right: -3px;
    width: 7px;
    height: 7px;
    background: #38BDF8;
    border-radius: 50%;
    box-shadow: 0 0 6px #38BDF8;
  }}

  /* Stone Plinth Showcase Bar */
  .showcase-bar {{
    background-color: #181c22;
    background-image: url('data:image/png;base64,{stone_tablet_b64}');
    background-repeat: repeat;
    border-top: 2px solid rgba(240, 215, 160, 0.35);
    border-left: 2px solid rgba(240, 215, 160, 0.25);
    border-bottom: 2.5px solid #06080a;
    border-right: 2.5px solid #06080a;
    border-radius: 6px;
    padding: 6px 14px 18px 14px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 12px;
    box-shadow: inset 1.5px 1.5px 5px rgba(0,0,0,0.85), 0 4px 10px rgba(0,0,0,0.7);
  }}
  .showcase-group {{
    display: flex;
    align-items: center;
    gap: 10px;
  }}
  .showcase-title {{
    font-family: 'Norsebold', serif;
    font-size: 11pt;
    color: #f0be52;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-right: 6px;
    filter: url(#v-groove-gold);
  }}
"""

print("Generating Variation 1: Elder Runestone Monolith...")
# ==================== VARIATION 1 ====================
# 100% Norsebold & Norse typography, Stacked Monumental Tablets, Heavy Chisel
v1_html = f"""<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
{COMMON_STYLE}

  /* Variation 1 Specific Typography */
  body {{
    font-family: 'Norsebold', serif;
    color: #ded5c5;
  }}

  .v1-header {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 2.5px solid #5a4220;
    padding-bottom: 4px;
  }}
  .v1-header h2 {{
    font-family: 'Norsebold', serif;
    font-size: 26pt;
    color: #f7d57f;
    letter-spacing: 0.06em;
    filter: url(#v-groove-gold);
    display: flex;
    align-items: center;
    gap: 10px;
  }}
  .v1-header img.chapter-icon {{
    width: 34px;
    height: 34px;
    object-fit: contain;
    filter: drop-shadow(0 3px 6px rgba(0,0,0,0.9));
  }}
  .role-pill-v1 {{
    background: linear-gradient(135deg, #7A5116 0%, #52350C 100%);
    color: #FFF4D9;
    font-family: 'Norsebold', serif;
    font-size: 11pt;
    padding: 4px 14px;
    border-radius: 4px;
    border: 1.5px solid #DDA13B;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    box-shadow: inset 1px 1px 2px rgba(255,255,255,0.25), 0 2px 6px rgba(0,0,0,0.8);
  }}

  /* Stacked Monumental Stone Tablets */
  .v1-tablet {{
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
    padding: 8px 14px;
    box-shadow: inset 2px 2px 5px rgba(0,0,0,0.88), inset -1px -1px 2px rgba(255,255,255,0.04), 0 4px 10px rgba(0,0,0,0.7);
  }}

  .v1-tablet h3 {{
    font-family: 'Norsebold', serif;
    font-size: 15.5pt;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    margin-bottom: 4px;
    display: flex;
    align-items: center;
    gap: 8px;
    border-bottom: 1.5px solid rgba(255, 230, 180, 0.2);
    padding-bottom: 3px;
  }}
  .v1-tablet h3.gold {{ color: #f7d57f; filter: url(#v-groove-gold); }}
  .v1-tablet h3.cyan {{ color: #7ad9f5; filter: url(#v-groove-cyan); }}
  .v1-tablet h3.green {{ color: #8ade4c; filter: url(#v-groove-green); }}

  .v1-tablet ul {{
    list-style: none;
    margin-left: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    flex: 1;
  }}
  .v1-tablet li {{
    font-family: 'Norsebold', serif;
    font-size: 11.2pt;
    color: #e5dec8;
    line-height: 1.35;
    letter-spacing: 0.04em;
    filter: url(#v-groove-body);
    margin-bottom: 3px;
  }}
  .v1-tablet li strong {{
    color: #fff2c4;
    font-size: 11.8pt;
  }}
</style>
</head>
<body>
{SVG_FILTERS}
  <div class="page">
    <div class="content-container">
      <div class="v1-header">
        <h2><img src="{ic("Antler_Pickaxe.png")}" class="chapter-icon"> PART IV: THE MINER (QUARRYMAN &amp; EXCAVATOR)</h2>
        <span class="role-pill-v1">MINING &bull; EXTRACTION</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">LOADOUT:</span>
          {slot("Antler_Pickaxe.png", "Pickaxe", True, 44)}
          {slot("Hoe.png", "Hoe", True, 44)}
          {slot("Torch.png", "Torch", False, 44)}
          {slot("Stone.png", "Cargo", False, 44)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">YIELDS &amp; STATIONS:</span>
          {slot("Wood_Pile.png", "Pile", False, 44)}
          {slot("Copper_Ore.png", "Copper", False, 44)}
          {slot("Tin_Ore.png", "Tin", False, 44)}
          {slot("Iron_Scrap.png", "Iron", False, 44)}
          {slot("Flint.png", "Flint", False, 44)}
        </div>
      </div>

      <div class="v1-tablet">
        <h3 class="green">[F06] SURFACE QUARRY &bull; [F07] DEEP VEINS &bull; [F08] HOE PLANING</h3>
        <ul>
          <li><strong>[F06] SURFACE BOULDER SHATTERING:</strong> STRIKES ROCK NODES AND FLINT DEPOSITS WITH EQUIPPED PICKAXE.</li>
          <li><strong>[F07] DEEP VEIN EXCAVATION:</strong> CHIPS COPPER, TIN, SILVER, AND IRON BURIED WITHIN ROCK COLLIDERS.</li>
          <li><strong>[F08] EVEN THE FIELD (HOE LEVELING):</strong> EQUIPS A HOE TO LEVEL JAGGED QUARRY FLOORS AND ELIMINATE PIT TRAPS.</li>
          <li><strong>[F05] 50X OVERFLOW MASONRY STACKS:</strong> ERECTS PHYSICAL STONE PILES WHEN BASE STORAGE CHESTS ARE FULL.</li>
        </ul>
      </div>

      <div class="v1-tablet">
        <h3 class="cyan">AUTONOMOUS AI WORK-CYCLE &amp; BEHAVIOR SEQUENCE</h3>
        <ul>
          <li><strong>1. GEODE PROSPECTING:</strong> SCANS 25M WORK POST SPHERE; VALIDATES PICKAXE TIER AGAINST HARDNESS.</li>
          <li><strong>2. OVERBURDEN EXCAVATION:</strong> CHIPS AWAY OVERLYING SOIL AND CRAGS TO ISOLATE ORE COLLIDERS.</li>
          <li><strong>3. HOE PLANING PASS:</strong> SWAPS TO HOE TO LEVEL QUARRY BASES, CREATING SAFE PATHS FOR HAULER CARTS.</li>
          <li><strong>4. ORE CONSOLIDATION:</strong> SWEEPS ORE CHUNKS AND DEPOSITS METALS INTO [ORE] CHESTS; STACKS MASONRY PILES.</li>
          <li><strong>5. FORGE REPAIR RUN:</strong> NAVIGATES TO NEAREST FORGE WHEN PICKAXE INTEGRITY DROPS BELOW 10%.</li>
        </ul>
      </div>

      <div class="v1-tablet">
        <h3 class="gold">SETTLEMENT SYNERGY &amp; HUGIN'S TACTICAL GUIDANCE</h3>
        <ul>
          <li><strong>FOUNDRY FEEDER:</strong> SUPPLIES DIRECT RAW COPPER AND IRON ORE TO SMELTER BATTERIES FOR INGOT REFINING.</li>
          <li><strong>MASONRY ADVANCE:</strong> PROVIDES STONE PILES DIRECTLY TO CRAFTSMEN FOR CITADEL WALL CONSTRUCTION.</li>
          <li><strong>TOOL INTEGRITY RULE:</strong> MAINTAIN AN UPGRADED WORKBENCH AND FORGE NEAR THE PIT TO PREVENT TRAVEL DELAYS.</li>
          <li><strong>BEDROCK BOUNDARY:</strong> EXCAVATION CEASES SAFELY AT WORLD BEDROCK FLOOR (-8M).</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>VARIATION 1: ELDER RUNESTONE MONOLITH (100% NORSEBOLD)</span>
      <span>PAGE 7</span>
    </div>
  </div>
</body>
</html>
"""

print("Generating Variation 2: Stronghold Steward Codex (Hybrid Authentic)...")
# ==================== VARIATION 2 ====================
# Norsebold Titles + Averia Serif Libre Bold Body at Large Viking Hands Scale (15pt-16pt)
# Two-Column Split Architecture
v2_html = f"""<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
{COMMON_STYLE}

  /* Variation 2 Specific Typography */
  body {{
    font-family: 'Averia Serif Libre', Georgia, serif;
    color: #ede5d8;
  }}

  .v2-header {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 2.5px solid #5a4220;
    padding-bottom: 4px;
  }}
  .v2-header h2 {{
    font-family: 'Norsebold', serif;
    font-size: 26pt;
    color: #f7d57f;
    letter-spacing: 0.06em;
    filter: url(#v-groove-gold);
    display: flex;
    align-items: center;
    gap: 10px;
  }}
  .v2-header img.chapter-icon {{
    width: 34px;
    height: 34px;
    object-fit: contain;
    filter: drop-shadow(0 3px 6px rgba(0,0,0,0.9));
  }}
  .role-pill-v2 {{
    background: linear-gradient(135deg, #7A5116 0%, #52350C 100%);
    color: #FFF4D9;
    font-family: 'Norsebold', serif;
    font-size: 11pt;
    padding: 4px 14px;
    border-radius: 4px;
    border: 1.5px solid #DDA13B;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    box-shadow: inset 1px 1px 2px rgba(255,255,255,0.25), 0 2px 6px rgba(0,0,0,0.8);
  }}

  /* Two-Column Split Stele Layout */
  .v2-split-container {{
    flex: 1;
    display: grid;
    grid-template-columns: 1fr 1.05fr;
    gap: 8px;
    min-height: 0;
  }}

  .v2-col {{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 7px;
  }}

  .v2-tablet {{
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
    padding: 10px 14px;
    box-shadow: inset 2px 2px 5px rgba(0,0,0,0.88), 0 4px 10px rgba(0,0,0,0.7);
  }}

  .v2-tablet h3 {{
    font-family: 'Norsebold', serif;
    font-size: 14.5pt;
    letter-spacing: 0.05em;
    text-transform: uppercase;
    margin-bottom: 5px;
    display: flex;
    align-items: center;
    gap: 8px;
    border-bottom: 1.5px solid rgba(255, 230, 180, 0.2);
    padding-bottom: 3px;
  }}
  .v2-tablet h3.gold {{ color: #f7d57f; filter: url(#v-groove-gold); }}
  .v2-tablet h3.cyan {{ color: #7ad9f5; filter: url(#v-groove-cyan); }}
  .v2-tablet h3.green {{ color: #8ade4c; filter: url(#v-groove-green); }}

  .v2-tablet ul {{
    list-style: none;
    margin-left: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    flex: 1;
  }}
  .v2-tablet li {{
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-weight: bold;
    font-size: 10.2pt;
    color: #ede2d0;
    line-height: 1.38;
    letter-spacing: 0.02em;
    text-shadow: -1.5px -1.5px 0px #040507, 1px 1px 0px rgba(245, 225, 185, 0.5);
    margin-bottom: 4px;
  }}
  .v2-tablet li strong {{
    color: #fff5d4;
    font-family: 'Norsebold', serif;
    font-size: 11.2pt;
    letter-spacing: 0.04em;
    display: block;
    margin-bottom: 1px;
  }}
</style>
</head>
<body>
{SVG_FILTERS}
  <div class="page">
    <div class="content-container">
      <div class="v2-header">
        <h2><img src="{ic("Antler_Pickaxe.png")}" class="chapter-icon"> PART IV: THE MINER (QUARRYMAN &amp; EXCAVATOR)</h2>
        <span class="role-pill-v2">MINING &bull; EXTRACTION</span>
      </div>

      <div class="showcase-bar">
        <div class="showcase-group">
          <span class="showcase-title">LOADOUT:</span>
          {slot("Antler_Pickaxe.png", "Pickaxe", True, 44)}
          {slot("Hoe.png", "Hoe", True, 44)}
          {slot("Torch.png", "Torch", False, 44)}
          {slot("Stone.png", "Cargo", False, 44)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">YIELDS &amp; STATIONS:</span>
          {slot("Wood_Pile.png", "Pile", False, 44)}
          {slot("Copper_Ore.png", "Copper", False, 44)}
          {slot("Tin_Ore.png", "Tin", False, 44)}
          {slot("Iron_Scrap.png", "Iron", False, 44)}
          {slot("Flint.png", "Flint", False, 44)}
        </div>
      </div>

      <div class="v2-split-container">
        <!-- Left Column -->
        <div class="v2-col">
          <div class="v2-tablet">
            <h3 class="cyan">AUTONOMOUS AI WORK-CYCLE</h3>
            <ul>
              <li>
                <strong>1. GEODE PROSPECTING</strong>
                Scans 25m sphere around Work Post; evaluates pickaxe tier vs rock hardness.
              </li>
              <li>
                <strong>2. OVERBURDEN EXCAVATION</strong>
                Chips away surface crags and topsoil to expose buried ore colliders.
              </li>
              <li>
                <strong>3. HOE PLANING PASS</strong>
                Swaps to Hoe to flatten jagged pit bottoms, creating ramped cart paths.
              </li>
              <li>
                <strong>4. FORGE REPAIR RUN</strong>
                Navigates to camp Forge when pickaxe durability drops below 10%.
              </li>
            </ul>
          </div>

          <div class="v2-tablet">
            <h3 class="gold">HUGIN'S ADVICE &amp; QUIRKS</h3>
            <ul>
              <li>
                <strong>PROXIMITY INTEGRITY</strong>
                Place an upgraded Workbench and Forge near the quarry to avoid walking delays.
              </li>
              <li>
                <strong>SIGN BRACKET ROUTING</strong>
                Mark foundry strongboxes with [Stone] and [Ore] for automated sorting.
              </li>
              <li>
                <strong>BEDROCK BOUNDARY</strong>
                Miners safely stop downward digging at the world bedrock floor (-8m).
              </li>
            </ul>
          </div>
        </div>

        <!-- Right Column -->
        <div class="v2-col">
          <div class="v2-tablet">
            <h3 class="green">ACTIVE EXTRACTION CAPABILITIES</h3>
            <ul>
              <li>
                <strong>[F06] SURFACE QUARRYING</strong>
                Shatters surface boulders, loose rocks, and flint nodes with pickaxe.
              </li>
              <li>
                <strong>[F07] DEEP VEIN EXCAVATION</strong>
                Extracts Copper, Tin, Iron Scrap, and Silver veins with tool tier checks.
              </li>
              <li>
                <strong>[F08] EVEN FIELD (HOE LEVELING)</strong>
                Equips Hoe to eliminate steep pit traps and build smooth transit roads.
              </li>
              <li>
                <strong>[F05] 50X OVERFLOW MASONRY PILES</strong>
                Constructs 50x stone_pile structures when camp storage chests are full.
              </li>
            </ul>
          </div>

          <div class="v2-tablet">
            <h3 class="gold">SETTLEMENT DOMAIN SYNERGY</h3>
            <ul>
              <li>
                <strong>FOUNDRY FEEDER</strong>
                Direct conduit of raw copper, tin, and scrap iron to the Smelter quadrant.
              </li>
              <li>
                <strong>MASONRY STOCKPILER</strong>
                Provides vast 50x stone piles to Craftsmen for defensive stone walls.
              </li>
              <li>
                <strong>VAGON CART LOADING</strong>
                Drops heavy ore stacks into parked carts for unified player hauling.
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>VARIATION 2: STRONGHOLD STEWARD CODEX (NORSEBOLD + AVERIA BOLD)</span>
      <span>PAGE 7</span>
    </div>
  </div>
</body>
</html>
"""

print("Generating Variation 3: Raw Basalt & Weathered Mineral Pigments...")
# ==================== VARIATION 3 ====================
# High-contrast deep chisel, boldest titles, rich mineral pigments, Norsebold + Norse uppercase
v3_html = f"""<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
{COMMON_STYLE}

  body {{
    font-family: 'Norsebold', serif;
    color: #ede2ce;
  }}

  /* High-Contrast Basalt Hero Plinth */
  .v3-hero-bar {{
    background-color: #12151b;
    background-image: url('data:image/png;base64,{stone_tablet_b64}');
    background-repeat: repeat;
    border-top: 2.5px solid rgba(255, 235, 175, 0.45);
    border-left: 2.5px solid rgba(255, 235, 175, 0.35);
    border-bottom: 3.5px solid #020305;
    border-right: 3px solid #020305;
    border-radius: 6px;
    padding: 8px 16px 20px 16px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    box-shadow: inset 2px 2px 7px rgba(0,0,0,0.95), 0 5px 14px rgba(0,0,0,0.8);
  }}

  .v3-header {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-bottom: 3px solid #6b4e22;
    padding-bottom: 4px;
  }}
  .v3-header h2 {{
    font-family: 'Norsebold', serif;
    font-size: 28pt;
    color: #fbd368;
    letter-spacing: 0.07em;
    filter: url(#v-groove-gold);
    display: flex;
    align-items: center;
    gap: 12px;
  }}
  .v3-header img.chapter-icon {{
    width: 36px;
    height: 36px;
    object-fit: contain;
    filter: drop-shadow(0 4px 8px rgba(0,0,0,0.95));
  }}

  .role-pill-v3 {{
    background: linear-gradient(135deg, #8A5C1B 0%, #4A2E07 100%);
    color: #FFF6DD;
    font-family: 'Norsebold', serif;
    font-size: 12pt;
    padding: 5px 16px;
    border-radius: 4px;
    border: 1.8px solid #F5C542;
    text-transform: uppercase;
    letter-spacing: 0.09em;
    box-shadow: inset 1.5px 1.5px 3px rgba(255,255,255,0.35), 0 3px 8px rgba(0,0,0,0.85);
  }}

  /* Tri-Panel Carved Monolith */
  .v3-tablet {{
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    background-color: #14171d;
    background-image: url('data:image/png;base64,{stone_tablet_b64}');
    background-repeat: repeat;
    border-top: 2.5px solid rgba(255, 230, 180, 0.40);
    border-left: 2.5px solid rgba(255, 230, 180, 0.30);
    border-bottom: 3.5px solid #020305;
    border-right: 3px solid #020305;
    border-radius: 6px;
    padding: 9px 16px;
    box-shadow: inset 2.5px 2.5px 6px rgba(0,0,0,0.92), 0 5px 12px rgba(0,0,0,0.75);
  }}

  .v3-tablet h3 {{
    font-family: 'Norsebold', serif;
    font-size: 16.5pt;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    margin-bottom: 4px;
    display: flex;
    align-items: center;
    gap: 8px;
    border-bottom: 2px solid rgba(255, 230, 180, 0.25);
    padding-bottom: 3px;
  }}
  .v3-tablet h3.gold {{ color: #fbd368; filter: url(#v-groove-gold); }}
  .v3-tablet h3.cyan {{ color: #6ee7b7; filter: url(#v-groove-green); }}
  .v3-tablet h3.blood {{ color: #f87171; filter: url(#v-groove-gold); }}

  .v3-tablet ul {{
    list-style: none;
    margin-left: 0;
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    flex: 1;
  }}
  .v3-tablet li {{
    font-family: 'Norse', serif;
    font-size: 12.8pt;
    color: #ede2ce;
    line-height: 1.34;
    letter-spacing: 0.05em;
    filter: url(#v-groove-body);
    margin-bottom: 3px;
  }}
  .v3-tablet li strong {{
    font-family: 'Norsebold', serif;
    color: #fef08a;
    font-size: 13.5pt;
    letter-spacing: 0.05em;
  }}
</style>
</head>
<body>
{SVG_FILTERS}
  <div class="page">
    <div class="content-container">
      <div class="v3-header">
        <h2><img src="{ic("Antler_Pickaxe.png")}" class="chapter-icon"> PART IV: THE MINER (QUARRYMAN &amp; EXCAVATOR)</h2>
        <span class="role-pill-v3">MINING &bull; EXTRACTION</span>
      </div>

      <div class="v3-hero-bar">
        <div class="showcase-group">
          <span class="showcase-title">LOADOUT:</span>
          {slot("Antler_Pickaxe.png", "Pickaxe", True, 46)}
          {slot("Hoe.png", "Hoe", True, 46)}
          {slot("Torch.png", "Torch", False, 46)}
          {slot("Stone.png", "Cargo", False, 46)}
        </div>
        <div class="showcase-group">
          <span class="showcase-title">YIELDS &amp; STATIONS:</span>
          {slot("Wood_Pile.png", "Pile", False, 46)}
          {slot("Copper_Ore.png", "Copper", False, 46)}
          {slot("Tin_Ore.png", "Tin", False, 46)}
          {slot("Iron_Scrap.png", "Iron", False, 46)}
          {slot("Flint.png", "Flint", False, 46)}
        </div>
      </div>

      <div class="v3-tablet">
        <h3 class="cyan">[F06] SURFACE BOULDERS &bull; [F07] DEEP VEINS &bull; [F08] HOE PLANING</h3>
        <ul>
          <li><strong>[F06] SURFACE QUARRYING:</strong> SHATTERS SURFACE BOULDERS AND FLINT NODES WITH EQUIPPED PICKAXE.</li>
          <li><strong>[F07] DEEP VEIN EXCAVATION:</strong> EXTRACTS COPPER, TIN, SILVER, AND IRON SCRAP VEINS USING TOOL TIER CHECKS.</li>
          <li><strong>[F08] EVEN THE FIELD (HOE LEVELING):</strong> SWAPS TO HOE TO LEVEL JAGGED QUARRY FLOORS AND ELIMINATE PIT TRAPS.</li>
          <li><strong>[F05] 50X OVERFLOW MASONRY STACKS:</strong> CONSTRUCTS 50X STONE_PILE PIECES ON SOIL WHEN CHESTS ARE FULL.</li>
        </ul>
      </div>

      <div class="v3-tablet">
        <h3 class="gold">OPERATIONAL AI WORK-CYCLE &amp; BEHAVIOR SEQUENCE</h3>
        <ul>
          <li><strong>1. GEODE PROSPECTING:</strong> SCANS 25M WORK POST RADIUS; MATCHES PICKAXE TIER VS HARDNESS.</li>
          <li><strong>2. OVERBURDEN EXCAVATION:</strong> REMOVES SURROUNDING BEDROCK TO FULLY ISOLATE VEIN COLLIDERS.</li>
          <li><strong>3. HOE PLANING PASS:</strong> LEVELS QUARRY BOTTOMS, CREATING FLAT CART RAMPS FOR BULK HAULING.</li>
          <li><strong>4. ORE CONSOLIDATION:</strong> SWEEPS CHUNKS INTO [ORE] STORAGE CHESTS; STACKS REMAINING STONE.</li>
          <li><strong>5. FORGE REPAIR RUN:</strong> NAVIGATES TO NEAREST FORGE WHEN PICKAXE DURABILITY DROPS BELOW 10%.</li>
        </ul>
      </div>

      <div class="v3-tablet">
        <h3 class="blood">SETTLEMENT INTEGRATION &amp; WORKBENCH PROXIMITY</h3>
        <ul>
          <li><strong>FOUNDRY CONDUIT:</strong> CONTINUOUSLY FEEDS RAW ORES DIRECTLY INTO THE SMELTER QUADRANT.</li>
          <li><strong>MASONRY STOCKPILER:</strong> SUPPLIES VAST STONE RESERVES TO CRAFTSMEN FOR STRONGHOLD FORTIFICATIONS.</li>
          <li><strong>TOOL INTEGRITY RULE:</strong> KEEP AN UPGRADED WORKBENCH AND FORGE NEAR THE QUARRY TO PREVENT WALKING DELAYS.</li>
          <li><strong>BEDROCK BOUNDARY:</strong> EXCAVATION SAFELY HALTS AT THE NATURAL WORLD BEDROCK FLOOR (-8M).</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>VARIATION 3: RAW BASALT &amp; WEATHERED MINERAL PIGMENTS (NORSEBOLD + NORSE)</span>
      <span>PAGE 7</span>
    </div>
  </div>
</body>
</html>
"""

# Write out the three variation HTML files
v1_path = os.path.join(BASE_DIR, "variation_1_elder_runestone.html")
v2_path = os.path.join(BASE_DIR, "variation_2_stronghold_hybrid.html")
v3_path = os.path.join(BASE_DIR, "variation_3_raw_basalt_pigments.html")

with open(v1_path, "w", encoding="utf-8") as f: f.write(v1_html)
with open(v2_path, "w", encoding="utf-8") as f: f.write(v2_html)
with open(v3_path, "w", encoding="utf-8") as f: f.write(v3_html)

print("Saved HTML variations!")

# Compile each to PDF and render high-res PNG
edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if not os.path.exists(edge_path):
    edge_path = r"C:\Program Files\Microsoft\Edge\Application\msedge.exe"

variations = [
    ("variation_1_elder_runestone", v1_path),
    ("variation_2_stronghold_hybrid", v2_path),
    ("variation_3_raw_basalt_pigments", v3_path),
]

for name, html_file in variations:
    pdf_file = os.path.join(BASE_DIR, f"{name}.pdf")
    png_file = os.path.join(BASE_DIR, f"{name}.png")
    print(f"Compiling {name}...")
    cmd = [
        edge_path,
        "--headless=new",
        "--disable-gpu",
        "--no-pdf-header-footer",
        f"--print-to-pdf={pdf_file}",
        html_file
    ]
    subprocess.run(cmd, check=True)
    doc = pymupdf.open(pdf_file)
    page = doc[0]
    pix = page.get_pixmap(dpi=150)
    pix.save(png_file)
    print(f"Saved {png_file}!")

print("All 3 variations successfully generated and rendered!")
