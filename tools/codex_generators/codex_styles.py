# Codex Styles, Base64 Textures, Fonts, and SVG Chiseled Filters
import os, base64

BASE_DIR = r"d:\valheim_mods"
DOCS_DIR = os.path.join(BASE_DIR, "docs")
ASSETS_DIR = os.path.join(DOCS_DIR, "assets")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
STONE_DIR = os.path.join(ASSETS_DIR, "stone")

def file_b64(path):
    with open(path, "rb") as f:
        return base64.b64encode(f.read()).decode("utf-8")

print("Encoding stone textures...")
stone_monolith_b64 = file_b64(os.path.join(STONE_DIR, "stone_monolith_bg.png"))
stone_tablet_b64 = file_b64(os.path.join(STONE_DIR, "stone_tablet_bg.png"))
stone_socket_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_bg.png"))
stone_socket_eq_b64 = file_b64(os.path.join(STONE_DIR, "stone_socket_equipped_bg.png"))
stone_crest_b64 = file_b64(os.path.join(STONE_DIR, "stone_cover_crest.png"))

print("Encoding typography...")
norse_b64 = file_b64(os.path.join(FONTS_DIR, "Norse.otf"))
norsebold_b64 = file_b64(os.path.join(FONTS_DIR, "Norsebold.otf"))
averia_regular_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Regular.ttf"))
averia_bold_b64 = file_b64(os.path.join(FONTS_DIR, "AveriaSerifLibre-Bold.ttf"))

print("Encoding authentic Valheim item icons...")
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
    norm = name.lower().replace(" ", "_").replace("-", "_")
    for k in icon_cache:
        if norm in k.lower():
            return icon_cache[k]
    return ""

def slot(icon_name, label, equipped=False, size=44):
    eq_cls = " equipped" if equipped else ""
    return f"""<div class="valheim-slot{eq_cls}" style="width:{size}px; height:{size}px;"><img src="{ic(icon_name)}" alt="{label}"><span class="slot-label">{label}</span></div>"""

SVG_FILTERS = """
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
    <!-- Arrow Markers for Conduits -->
    <marker id="arrow-gold" markerWidth="9" markerHeight="9" refX="7" refY="4.5" orient="auto">
      <path d="M 0 1 L 8 4.5 L 0 8 Z" fill="#ebb538"/>
    </marker>
    <marker id="arrow-cyan" markerWidth="9" markerHeight="9" refX="7" refY="4.5" orient="auto">
      <path d="M 0 1 L 8 4.5 L 0 8 Z" fill="#55c9ea"/>
    </marker>
    <marker id="arrow-green" markerWidth="9" markerHeight="9" refX="7" refY="4.5" orient="auto">
      <path d="M 0 1 L 8 4.5 L 0 8 Z" fill="#73cf42"/>
    </marker>
    <marker id="arrow-orange" markerWidth="9" markerHeight="9" refX="7" refY="4.5" orient="auto">
      <path d="M 0 1 L 8 4.5 L 0 8 Z" fill="#ea8a38"/>
    </marker>

    <!-- 1. GOLD V-GROOVE CHISEL -->
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

    <!-- 2. CYAN RUNIC CHISEL -->
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

    <!-- 4. ORANGE OCHRE CHISEL -->
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

    <!-- 5. WEATHERED RUST CHISEL (RED-OCHRE) -->
    <filter id="v-groove-rust" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
      <feGaussianBlur in="SourceAlpha" stdDeviation="0.8" result="blurAlpha"/>
      <feDiffuseLighting in="blurAlpha" surfaceScale="-1" diffuseConstant="0.7" lighting-color="#ffeedd" result="diffLight">
        <feDistantLight azimuth="190" elevation="44"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clippedFill"/>
      
      <!-- Inset Carve Shadow (Top-Left) -->
      <feOffset in="SourceAlpha" dx="1.8" dy="1.8" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#050302" flood-opacity="0.95" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
      
      <!-- Outer Rim Lip Highlight (Bottom-Right, Terracotta Ochre Lip) -->
      <feOffset in="SourceAlpha" dx="-0.9" dy="-0.9" result="lipOff"/>
      <feComposite in="lipOff" in2="SourceAlpha" operator="out" result="bLip"/>
      <feFlood flood-color="#ffa085" flood-opacity="1.0" result="lipFlood"/>
      <feComposite in="lipFlood" in2="bLip" operator="in" result="lipHighlight"/>
      
      <feMerge>
        <feMergeNode in="lipHighlight"/>
        <feMergeNode in="clippedFill"/>
        <feMergeNode in="carveShadow"/>
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
    color: #d64a2b;
    letter-spacing: 0.02em;
    filter: url(#v-groove-rust);
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

  /* Infographic Canvas Container */
  .infographic-canvas-box {{
    flex: 1;
    min-height: 0;
    background-color: #14171d;
    background-image: url('data:image/png;base64,{stone_tablet_b64}');
    border-top: 2px solid rgba(240, 215, 160, 0.35);
    border-left: 2px solid rgba(240, 215, 160, 0.25);
    border-bottom: 3px solid #06080a;
    border-right: 2.5px solid #06080a;
    border-radius: 6px;
    padding: 6px 8px;
    box-shadow: inset 2px 2px 6px rgba(0,0,0,0.9), 0 4px 10px rgba(0,0,0,0.7);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }}

  .infographic-canvas-box svg {{
    width: 100%;
    height: 100%;
    display: block;
  }}

  .infographic-annotation {{
    background-color: #0e1115;
    border-top: 1px solid rgba(220, 185, 120, 0.2);
    border-radius: 4px;
    padding: 5px 10px;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 9.2pt;
    font-weight: bold;
    color: #dcd2bf;
    margin-top: 4px;
  }}
  .infographic-annotation img {{
    width: 22px;
    height: 22px;
    object-fit: contain;
    flex-shrink: 0;
  }}
"""
