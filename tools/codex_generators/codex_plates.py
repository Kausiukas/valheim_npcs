# Codex Vector Infographic Plates & Spatial Blueprints Suite (Refined Collision-Free Edition)
import textwrap
from codex_styles import ic

def svg_box(x, y, w, h, title, desc, icon_name, metric="", color="#55c9ea", color_name="cyan"):
    wrapped = textwrap.wrap(desc, width=27)
    lines_svg = []
    y_off = 35
    for line in wrapped[:4]:
        lines_svg.append(f'<text x="48" y="{y_off}" font-family="Averia Serif Libre" font-weight="bold" font-size="8.2" fill="#ede2d0">{line}</text>')
        y_off += 11.5
    lines_html = "\n      ".join(lines_svg)
    
    metric_html = ""
    if metric:
        metric_html = f"""
      <g transform="translate({w-105}, {h-17})">
        <rect x="0" y="0" width="98" height="13" rx="3" fill="#0a0d10" stroke="{color}" stroke-width="1"/>
        <text x="49" y="9.5" font-family="Norsebold" font-size="7" fill="{color}" text-anchor="middle" letter-spacing="0.4">{metric}</text>
      </g>
"""
    return f"""
    <!-- Node Box: {title} -->
    <g transform="translate({x}, {y})">
      <rect x="0" y="0" width="{w}" height="{h}" rx="5" fill="#13161c" stroke="#2a2318" stroke-width="1.5"/>
      <rect x="0" y="0" width="{w}" height="20" rx="4" fill="#181c24" stroke="{color}" stroke-width="1"/>
      <text x="8" y="14" font-family="Norsebold" font-size="8.8" fill="{color}" letter-spacing="0.4">{title}</text>
      
      <!-- Inset Socket -->
      <g transform="translate(8, 25)">
        <rect x="0" y="0" width="34" height="34" rx="3" fill="#0a0c10" stroke="#3c3326" stroke-width="1"/>
        <image href="{ic(icon_name)}" x="3" y="3" width="28" height="28"/>
      </g>
      
      <!-- Description Lines -->
      {lines_html}
      
      {metric_html}
    </g>
"""

def svg_conduit(d, color="#55c9ea", marker="arrow-cyan", label="", lx=0, ly=0, dashed=True):
    dash_attr = 'stroke-dasharray="4,4"' if dashed else ""
    lbl_html = ""
    if label:
        lbl_w = len(label) * 5.6 + 14
        lbl_html = f"""
      <rect x="{lx - lbl_w/2}" y="{ly - 7}" width="{lbl_w}" height="13" rx="2" fill="#090c0f" stroke="{color}" stroke-width="0.8"/>
      <text x="{lx}" y="{ly + 2.5}" font-family="Norsebold" font-size="6.5" fill="{color}" text-anchor="middle" letter-spacing="0.3">{label}</text>
"""
    return f"""
    <g>
      <path d="{d}" fill="none" stroke="{color}" stroke-width="1.8" {dash_attr} marker-end="url(#{marker})"/>
      {lbl_html}
    </g>
"""

def make_infographic_page(page_num, title, sub, pill, icon_name, svg_content, annotation_icon, annotation_text):
    return f"""
  <div class="page">
    <div class="content-container">
      <div class="chapter-header">
        <h2><img src="{ic(icon_name)}" class="chapter-icon" alt="{title}"> {title}</h2>
        <span class="role-pill">{pill}</span>
      </div>

      <div class="infographic-canvas-box">
        <svg viewBox="0 0 720 900" xmlns="http://www.w3.org/2000/svg">
          {svg_content}
        </svg>

        <div class="infographic-annotation">
          <img src="{ic(annotation_icon)}" alt="Annotation">
          <span>{annotation_text}</span>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>THE SETTLEMENT STEWARD'S CODEX</span>
      <span>PROCESS SCHEMATICS &bull; VALHEIMWORKERS v0.3.0</span>
      <span>PAGE {page_num}</span>
    </div>
  </div>
"""

# ==================== PLATE I: MASTER SETTLEMENT PRODUCTION WEB ====================

def make_plate_1():
    svg = f"""
      <defs>
        <pattern id="grid" width="30" height="30" patternUnits="userSpaceOnUse">
          <path d="M 30 0 L 0 0 0 30" fill="none" stroke="rgba(240, 215, 160, 0.04)" stroke-width="0.8"/>
        </pattern>
      </defs>
      <rect width="100%" height="100%" fill="#101318"/>
      <rect width="100%" height="100%" fill="url(#grid)"/>

      <!-- Header Banner -->
      <g transform="translate(30, 16)">
        <text x="0" y="14" font-family="Norsebold" font-size="14" fill="#f7d57f" letter-spacing="0.08em">VALHEIM SETTLEMENT MACRO-LOGISTICS &amp; PRODUCTION WEB</text>
        <text x="0" y="28" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Autonomous Inter-Domain Supply Lines &bull; Circular Resource Flow &bull; Vascular Depot</text>
      </g>

      <!-- Left Column: Extraction & Sustenance -->
      {svg_box(30, 52, 300, 88, "1. FORESTRY (LUMBERJACK)", "Fells trees, bucks timber, excises stumps and re-plants 3m nursery grid.", "Iron_Axe.png", "YIELDS: WOOD", "#73cf42", "green")}
      {svg_box(30, 152, 300, 88, "2. QUARRYING (MINER)", "Excavates copper, tin & iron veins; planes 26-degree ramps with Hoe.", "Antler_Pickaxe.png", "YIELDS: ORES", "#55c9ea", "cyan")}
      {svg_box(30, 252, 300, 88, "3. AGRICULTURE (FARMER &amp; RANCHER)", "Cultivates 1.2m crops; tends apiary hives; feeds & breeds livestock.", "Cultivator.png", "50/50 SEED CYCLE", "#73cf42", "green")}
      {svg_box(30, 352, 300, 88, "4. PROVISIONS (COOK &amp; FISHER)", "Roasts meats with 0-char timing; simmers stews; lands tidal catches.", "Cauldron.png", "3-SLOT FOOD RATIONS", "#ea8a38", "orange")}

      <!-- Central Vascular Logistics Hub (Prominent Center-Right) -->
      {svg_box(360, 152, 330, 188, "CENTRAL HAULER DEPOT &amp; CART LOGISTICS", "Central freight hub. Vacuums 25m drops, hitches 1000kg carts, and sorts to tagged chests [Wood, Ore, Ingot].", "Cart.png", "CAPACITY: 1000 KG", "#ebb538", "gold")}

      <!-- Right Column: Metallurgy, Crafting & Defense -->
      {svg_box(360, 52, 330, 88, "5. FOUNDRY (SMELTER &amp; KILN)", "Kilns burn wood to coal. Smelters ingest ores (1:2 ratio) into refined bars.", "Smelter.png", "RATIO: 1 ORE : 2 COAL", "#ea8a38", "orange")}
      {svg_box(360, 352, 330, 88, "6. STRONGHOLD DEFENSE (SENTRY &amp; CRAFTSMAN)", "Sentries maintain 40m sightlines; craftsmen repair bastions and tools at 0 cost.", "Finewood_Bow.png", "40M SIGHTLINES", "#55c9ea", "cyan")}

      <!-- Conduits with Clear Paths (Zero Node Intersections) -->
      {svg_conduit("M 330 96 L 360 96", "#ea8a38", "arrow-orange", "TIMBER TO KILN", 345, 86)}
      {svg_conduit("M 330 196 L 360 196", "#55c9ea", "arrow-cyan", "RAW ORE", 345, 186)}
      {svg_conduit("M 330 296 L 360 296", "#73cf42", "arrow-green", "CROPS &amp; MEAT", 345, 286)}
      {svg_conduit("M 330 396 L 360 396", "#ea8a38", "arrow-orange", "FOOD RATIONS", 345, 386)}

      {svg_conduit("M 525 140 L 525 152", "#ea8a38", "arrow-orange", "FUEL &amp; INGOTS", 525, 146)}
      {svg_conduit("M 525 340 L 525 352", "#55c9ea", "arrow-cyan", "ARROWS &amp; GEAR", 525, 346)}

      <!-- Lower In-Depth Specifications Stele -->
      <g transform="translate(30, 465)">
        <rect x="0" y="0" width="660" height="415" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">SETTLEMENT MACRO-LOGISTICS SPECIFICATIONS &amp; VASCULAR CONDUITS</text>
        
        <!-- Left Sub-Tablet -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="305" height="360" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="12" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">VASCULAR BASE PRINCIPLES</text>
          
          <text x="12" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 25M POSTS: Overlapping zones ensure zero freight loss.</text>
          <text x="12" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CART FREIGHT: 2-wheel wagons replace 6 walking trips.</text>
          <text x="12" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 1:2 SMELTER RATIO: 2 Coal burned per 1 Ingot smelted.</text>
          <text x="12" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 50X OVERFLOW: Automatic piles prevent chest clogs.</text>
          <text x="12" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ZERO-COST REPAIR: [Repair] chests restore gear.</text>
          <text x="12" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; DIETARY BUFFS: 3-course meals maximize stamina.</text>
          <text x="12" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ZERO MOVE PENALTY: 100% material refund on break.</text>
          <text x="12" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SENTRY VIGIL: Interlocking 40m sightlines guard camp.</text>
          <text x="12" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; VACUUM RADIUS: Clears ground debris to save FPS.</text>
          <text x="12" y="282" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CITADEL WALLS: Craftsmen restore timber defenses.</text>
          <text x="12" y="308" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; DEPOT BALANCING: Equalizes duplicate tagged chests.</text>
          <text x="12" y="334" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; SUSTAINED CYCLE: Never exhausts settlement resources.</text>
        </g>

        <!-- Right Sub-Tablet -->
        <g transform="translate(337, 38)">
          <rect x="0" y="0" width="305" height="360" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="12" y="22" font-family="Norsebold" font-size="10" fill="#73cf42">AUTONOMOUS AI SCHEDULING &amp; HARVESTS</text>

          <text x="12" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; NOCTURNAL SLEEP: Civilians sleep 21:00-06:00.</text>
          <text x="12" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 1.2M SPACING: Grid prevents crop wither permanently.</text>
          <text x="12" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 50/50 SEED CYCLE: Half of crops converted to flowers.</text>
          <text x="12" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ZERO-BURN ROASTING: Cook flips meat at 20 seconds.</text>
          <text x="12" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 40M SIGHTLINES: Elevated Sentries detect raids early.</text>
          <text x="12" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; GROUND VACUUM: Clears loose debris within 25m.</text>
          <text x="12" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; REPAIR PATROL: Craftsmen keep defenses at 100%.</text>
          <text x="12" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ROAD BUFF: Carts move 40% faster on stone paths.</text>
          <text x="12" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 26&deg; TRANSIT RAMPS: Prevents carts sliding down pits.</text>
          <text x="12" y="282" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; -8M BEDROCK CEILING: Digging halts before world floor.</text>
          <text x="12" y="308" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; 8M PROXIMITY: Chests sit &lt;= 8m from smelter hoppers.</text>
          <text x="12" y="334" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; COLD RESISTANCE: 3-course diet buffers freezing nights.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=3,
        title="SETTLEMENT MACRO PRODUCTION WEB",
        sub="VASCULAR BASE ARRAYS &amp; SUPPLY CHAINS",
        pill="CIRCULAR ECONOMY",
        icon_name="Cart.png",
        svg_content=svg,
        annotation_icon="Hugin.png",
        annotation_text="HUGIN'S LAW: Place Hauler depots directly at the nexus between Extraction (west) and Smelting (east) to eliminate transit bottlenecks."
    )

# ==================== PLATE II: FORESTRY SCHEMA 1 ====================

def make_plate_2():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#73cf42" letter-spacing="0.08em">SCHEMA 1: FORESTRY &amp; ARBOR NURSERY WORK-CYCLE</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Directional Felling &bull; 100% Stump Removal &bull; 3m Sapling Grid &bull; 50x Stacks</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: AXE TIER VALIDATION", "Scans nearby trees; checks hardness. Stone Axe for Beech/Fir; Bronze+ for Birch/Oak.", "Iron_Axe.png", "HARDNESS: 1 VS 2", "#73cf42", "green")}
      {svg_box(255, 48, 210, 95, "STEP 02: DIRECTIONAL FELLING", "Strikes trunk at oblique angles to fell trees into meadows, saving roof beams.", "Fine_wood.png", "RAYCAST CLEARANCE", "#55c9ea", "cyan")}
      {svg_box(485, 48, 210, 95, "STEP 03: LOG BUCKING PASS", "Chops fallen trunks into round wood, fine wood, and core wood chunks.", "Wood.png", "TRUNCATION PASS", "#ebb538", "gold")}

      {svg_conduit("M 235 95 L 255 95", "#73cf42", "arrow-green")}
      {svg_conduit("M 465 95 L 485 95", "#55c9ea", "arrow-cyan")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#ebb538", "arrow-gold")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: 100% STUMP CLEARING", "Stumps never regrow! Worker excises root taproot colliders to clear soil for saplings.", "Stone_Axe.png", "TAPROOT EXCISION", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: 3.0M NURSERY GRID", "Deploys Beech, Birch & Fir seeds with exact 3m equidistant spacing to prevent decay.", "Beech_Seeds.png", "3.0M CLEARANCE", "#73cf42", "green")}
      {svg_box(25, 175, 210, 95, "STEP 06: STORAGE &amp; 50X PILES", "Sorts logs into [Wood] chests. If storage is full, builds physical 50x stacks on soil.", "Wood_Pile.png", "50X OVERFLOW PREFAB", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#73cf42", "arrow-green")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">ARBORIST GEOMETRY &amp; SUSTAINABLE NURSERY BLUEPRINT</text>

        <!-- Visual Vector Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Ground line -->
          <line x1="20" y1="185" x2="614" y2="185" stroke="#73cf42" stroke-width="2.5"/>
          <text x="160" y="177" font-family="Norsebold" font-size="8.5" fill="#73cf42">NATURAL GROUND LEVEL (0.0M)</text>

          <!-- Building to protect -->
          <rect x="45" y="105" width="95" height="80" rx="2" fill="#181c24" stroke="#5a4220" stroke-width="1.5"/>
          <polygon points="40,105 92,60 145,105" fill="#2d1f10" stroke="#5a4220" stroke-width="1.5"/>
          <text x="92" y="140" font-family="Norsebold" font-size="8" fill="#d2c5b0" text-anchor="middle">CITADEL ROOF</text>
          <text x="92" y="153" font-family="Averia Serif Libre" font-weight="bold" font-size="7" fill="#888888" text-anchor="middle">12m Raycast Buffer</text>

          <!-- Safe Felling Arc -->
          <path d="M 210 185 Q 210 70 280 80" fill="none" stroke="#ea8a38" stroke-width="2" stroke-dasharray="4,4"/>
          <text x="255" y="68" font-family="Norsebold" font-size="8" fill="#ea8a38">SAFE 45&deg; FELLING ARC</text>

          <!-- Falling Tree -->
          <line x1="210" y1="185" x2="285" y2="110" stroke="#8b5a2b" stroke-width="6"/>
          <circle cx="290" cy="105" r="24" fill="#2d5a27" stroke="#3d7a37" stroke-width="1.5"/>

          <!-- Bucked Log Segments -->
          <rect x="340" y="172" width="36" height="13" rx="2" fill="#6d4a25" stroke="#3a2512" stroke-width="1"/>
          <rect x="382" y="172" width="36" height="13" rx="2" fill="#6d4a25" stroke="#3a2512" stroke-width="1"/>
          <rect x="424" y="172" width="36" height="13" rx="2" fill="#6d4a25" stroke="#3a2512" stroke-width="1"/>
          <text x="400" y="163" font-family="Norsebold" font-size="7.5" fill="#f7d57f" text-anchor="middle">LOG BUCKING CUTS</text>

          <!-- Excised Stump -->
          <rect x="485" y="181" width="22" height="4" fill="#3a2512"/>
          <path d="M 480 185 L 512 185 L 496 202 Z" fill="#ea8a38" fill-opacity="0.3" stroke="#ea8a38" stroke-width="1"/>
          <text x="496" y="214" font-family="Norsebold" font-size="7.5" fill="#ea8a38" text-anchor="middle">ROOT EXCISED</text>

          <!-- Nursery 3m Spacing -->
          <g transform="translate(540, 140)">
            <image href="{ic("Beech_Seeds.png")}" x="0" y="20" width="22" height="22"/>
            <image href="{ic("Beech_Seeds.png")}" x="50" y="20" width="22" height="22"/>
            <line x1="11" y1="15" x2="61" y2="15" stroke="#55c9ea" stroke-width="1.5" marker-end="url(#arrow-cyan)" marker-start="url(#arrow-cyan)"/>
            <text x="36" y="9" font-family="Norsebold" font-size="7.5" fill="#55c9ea" text-anchor="middle">&lt;-- 3.0M GRID --&gt;</text>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">FORESTRY LOGISTICS MATRIX</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F01 PRECISION FELLING: Trajectory raycasts check 12m for roofs.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F02 LOG BUCKING: Splits trunks into regular, core, and fine wood.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F03 STUMP ELIMINATION: Completely clears root taproot colliders.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F04 SUSTAINABLE NURSERY: 3m equidistant sapling matrix.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F05 WOOD PILE OVERFLOW: Builds physical 50x stacks when capped.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; TOOL TIER VALIDATION: Bronze Axe required for Birch &amp; Oak trees.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; ZERO LOG ROT: Freshly harvested timber never decays on soil.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; 25M WORK POST: Bounds all felling activity to designated sphere.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SEED CYCLE: Automatically recovers beech seeds &amp; cones.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#73cf42">CIRCUIT CONDUITS &amp; SYNERGIES</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SMELTER KILN FEED: Supplies raw wood directly for charcoal pyrolysis.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CRAFTSMAN TIMBER DEPOT: Delivers Core Wood for fortress log poles.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FINEWOOD ARSENAL: Delivers fine timber for Huntsman and Finewood bows.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HAULER CART LOGISTICS: High-volume lumber transferred by wagon.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FORGE REPAIR RUN: Retires axe to camp Forge when durability &lt; 10%.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; NOCTURNAL REST: Resumes cutting at 06:00 sunrise after shelter sleep.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; STUMP INTEGRITY: Never leave stumps; they block new growth.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; CONTAINER TAG: Directs round logs into chests tagged [Wood].</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; FINEWOOD TAG: Directs birch/oak timber into chests tagged [Finewood].</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=5,
        title="SCHEMA 1: FORESTRY &amp; ARBOR NURSERY",
        sub="DIRECTIONAL FELLING &bull; 3M NURSERY GRID &bull; 50X OVERFLOW",
        pill="FORESTRY PROCESS",
        icon_name="Iron_Axe.png",
        svg_content=svg,
        annotation_icon="Beech_Seeds.png",
        annotation_text="STUMP CLEARING LAW: Never leave stumps in the ground. Stumps prevent nursery saplings from taking root and permanently block terrain pathing."
    )

# ==================== PLATE III: QUARRYING SCHEMA 2 ====================

def make_plate_3():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#55c9ea" letter-spacing="0.08em">SCHEMA 2: SUBSURFACE QUARRYING &amp; HOE PLANING</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Overburden Excavation &bull; -8m Bedrock Limits &bull; 26&deg; Hoe Ramps &bull; 50x Stone Piles</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: 25M GEODE SCAN", "Scans 25m sphere around Work Post; evaluates pickaxe tier vs rock hardness.", "Antler_Pickaxe.png", "SPHERE: 25M", "#55c9ea", "cyan")}
      {svg_box(255, 48, 210, 95, "STEP 02: OVERBURDEN CLEARING", "Chips away overlying soil & crags to expose contiguous ore colliders.", "Stone.png", "BEDROCK BOUNDARY", "#73cf42", "green")}
      {svg_box(485, 48, 210, 95, "STEP 03: DEEP VEIN EXCAVATION", "Extracts raw Copper, Tin, Iron & Silver. Collects ore into cargo packs.", "Copper_Ore.png", "TIER VALIDATION", "#ebb538", "gold")}

      {svg_conduit("M 235 95 L 255 95", "#55c9ea", "arrow-cyan")}
      {svg_conduit("M 465 95 L 485 95", "#73cf42", "arrow-green")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#ebb538", "arrow-gold")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: HOE PLANING PASS", "Swaps to Hoe; levels jagged pit floor, eliminates traps, and shapes 26&deg; ramps.", "Hoe.png", "26&deg; TRANSIT RAMP", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: 10% FORGE REPAIR RUN", "Tool integrity safety trigger: when pickaxe durability &lt; 10%, routes to Forge.", "Forge.png", "RETREAT: DUR &lt;= 10%", "#55c9ea", "cyan")}
      {svg_box(25, 175, 210, 95, "STEP 06: STORAGE &amp; 50X PILES", "Sorts ores to [Ore] &amp; [Stone] chests; constructs 50x stone piles on soil if full.", "Stone_Pile.png", "50X PREFAB MASONRY", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#55c9ea", "arrow-cyan")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Cross-Section (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">SUBSURFACE QUARRY CROSS-SECTION &amp; -8M BEDROCK CEILING</text>

        <!-- Visual Cross Section Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Ground line -->
          <path d="M 20 60 L 180 60 Q 230 60 250 140 L 450 140 Q 480 140 530 60 L 614 60" fill="none" stroke="#73cf42" stroke-width="2.5"/>
          <text x="25" y="52" font-family="Norsebold" font-size="8.5" fill="#73cf42">NATURAL GROUND LEVEL (0.0M)</text>

          <!-- Overburden Excavation Zone -->
          <rect x="220" y="65" width="200" height="30" rx="3" fill="#2d2214" stroke="#5a4220" stroke-width="1"/>
          <text x="320" y="84" font-family="Norsebold" font-size="8" fill="#c7b299" text-anchor="middle">OVERBURDEN SOIL CRAGS (EXCAVATED FIRST)</text>

          <!-- Ore Mesh in Bedrock -->
          <ellipse cx="330" cy="120" rx="70" ry="16" fill="#1b382b" stroke="#3bb2d9" stroke-width="1.5"/>
          <text x="330" y="123" font-family="Norsebold" font-size="8.5" fill="#7ad9f5" text-anchor="middle">COPPER ORE COLLIDER MESH</text>

          <!-- Pit Floor Planed by Hoe -->
          <line x1="250" y1="140" x2="450" y2="140" stroke="#ebb538" stroke-width="3"/>
          <text x="350" y="158" font-family="Norsebold" font-size="8" fill="#ebb538" text-anchor="middle">LEVELED QUARRY FLOOR (-4.5M PLANED BY HOE)</text>

          <!-- 26 Degree Ramp -->
          <line x1="450" y1="140" x2="530" y2="60" stroke="#ea8a38" stroke-width="3.5"/>
          <text x="500" y="105" font-family="Norsebold" font-size="8.5" fill="#ea8a38">26&deg; CART RAMP</text>
          <text x="500" y="118" font-family="Averia Serif Libre" font-weight="bold" font-size="7" fill="#d2c5b0">Zero wheel stall</text>

          <!-- Bedrock Floor Limit -->
          <line x1="20" y1="205" x2="614" y2="205" stroke="#e04836" stroke-width="2" stroke-dasharray="6,4"/>
          <text x="25" y="198" font-family="Norsebold" font-size="8.5" fill="#e04836">WORLD BEDROCK FLOOR (-8.0M EXCAVATION CEILING)</text>
          <text x="400" y="222" font-family="Averia Serif Libre" font-weight="bold" font-size="7.5" fill="#a89a80">Miners halt downward strikes at -8m to avoid terrain clipping.</text>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">QUARRY SPECIFICATIONS &amp; LIMITS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F06 SURFACE QUARRYING: Shatters field boulders &amp; crags.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F07 DEEP VEIN MINING: Extracts Copper, Tin, Iron, and Silver.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F08 HOE PLANING: Levels pit bottoms into smooth roadways.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F05 50X STONE PILES: Automatically places stone stacks on soil.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; BEDROCK HARD CEILING: Downward mining constrained to -8m.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; PICKAXE HARDNESS: Bronze Pickaxe needed for Iron Scrap.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; 10% FORGE REPAIR: Automatically returns to Forge when damaged.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; 25M WORK POST: Spherical boundary prevents unbounded wandering.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ZERO PIT TRAPS: Planing pass removes steep vertical edges.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#ea8a38">HAULER &amp; FOUNDRY CONDUITS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 26&deg; TRANSIT GRADIENT: Steeper ramps cause carts to stall.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HAULER EXTRACTION LINK: Empties quarry chests directly.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FOUNDRY HOPPER PIPELINE: Transfers raw ore to Smelters.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; MASON STONE SUPPLY: Supplies stone blocks to Craftsmen.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; PROXIMITY RULE: Place Forge near pit to minimize walking.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SIGN BRACKETS: Tag chests [Ore] and [Stone] for auto-sort.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; ZERO COLLAPSE HAZARD: Undermining terrain never collapses.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; FLINT SOURCING: Gathers shoreline flint nodes within boundary.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; SILVER PROSPECTING: Mines mountain silver veins with Iron pick.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=7,
        title="SCHEMA 2: QUARRYING &amp; HOE PLANING",
        sub="SUBSURFACE EXCAVATION &bull; -8M BEDROCK CEILING &bull; 26&deg; RAMPS",
        pill="MINING PROCESS",
        icon_name="Antler_Pickaxe.png",
        svg_content=svg,
        annotation_icon="Hoe.png",
        annotation_text="CART RAMP LAW: Always construct 26&deg; ingress ramps into deep quarry pits using a Hoe. Steep cliff walls trap haulers and delay ore transit."
    )

# ==================== PLATE IV: AGRICULTURE SCHEMA 3 ====================

def make_plate_4():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#73cf42" letter-spacing="0.08em">SCHEMA 3: AGRICULTURAL 1.2M GRID &amp; APIARY</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">1.2m Botanical Spacing &bull; 50% Seed Reinvestment &bull; Beehive Proximity &bull; Larder Storage</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: SOIL CULTIVATION", "Converts raw grassy terrain into rich fertile loam using Cultivator.", "Cultivator.png", "SPHERE: 25M", "#73cf42", "green")}
      {svg_box(255, 48, 210, 95, "STEP 02: 1.2M GRID PLANTING", "Deploys seeds in disciplined 1.2m grid to eliminate cramped crop wither.", "Carrot_Seeds.png", "1.2M SPACING", "#55c9ea", "cyan")}
      {svg_box(485, 48, 210, 95, "STEP 03: FULL-CYCLE HARVEST", "Harvests fully mature crops and seed flowers at peak botanical ripeness.", "Carrot.png", "ZERO WITHER", "#ebb538", "gold")}

      {svg_conduit("M 235 95 L 255 95", "#73cf42", "arrow-green")}
      {svg_conduit("M 465 95 L 485 95", "#55c9ea", "arrow-cyan")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#ebb538", "arrow-gold")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: SEED ROTATION PASS", "Re-seeds 50% of harvest into seed-flowers to guarantee infinite seeds.", "Onion.png", "50/50 RATIO", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: APIARY HONEY PASS", "Empties camp beehives into honeycomb stores; monitors bee happiness.", "Honey.png", "4 HONEY / HIVE", "#73cf42", "green")}
      {svg_box(25, 175, 210, 95, "STEP 06: LARDER STORAGE", "Routes produce into [Produce] chests for kitchen culinary feasts.", "Turnip.png", "TAG: [PRODUCE]", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#73cf42", "arrow-green")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">BOTANICAL 1.2M MATRIX &amp; APIARY TERRITORY BLUEPRINT</text>

        <!-- Visual Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Tilled Loam Field -->
          <rect x="35" y="30" width="310" height="175" rx="4" fill="#1b150d" stroke="#5a4220" stroke-width="1.5"/>
          <text x="190" y="22" font-family="Norsebold" font-size="8.5" fill="#c7b299" text-anchor="middle">TILLED LOAM FIELD (1.2M BOTANICAL GRID)</text>

          <!-- 1.2m Crop Grid Points -->
          <g transform="translate(55, 45)">
            <!-- Row 1 -->
            <circle cx="20" cy="20" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Carrot.png")}" x="10" y="10" width="20" height="20"/>

            <circle cx="95" cy="20" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Carrot.png")}" x="85" y="10" width="20" height="20"/>

            <circle cx="170" cy="20" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Turnip.png")}" x="160" y="10" width="20" height="20"/>

            <circle cx="245" cy="20" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Onion.png")}" x="235" y="10" width="20" height="20"/>

            <!-- Dimension Line -->
            <line x1="20" y1="52" x2="95" y2="52" stroke="#55c9ea" stroke-width="1.5" marker-start="url(#arrow-cyan)" marker-end="url(#arrow-cyan)"/>
            <text x="57" y="47" font-family="Norsebold" font-size="7.5" fill="#55c9ea" text-anchor="middle">&lt;-- 1.2M NO-WITHER CLEARANCE --&gt;</text>

            <!-- Row 2 -->
            <circle cx="20" cy="95" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Carrot_Seeds.png")}" x="10" y="85" width="20" height="20"/>

            <circle cx="95" cy="95" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Carrot_Seeds.png")}" x="85" y="85" width="20" height="20"/>

            <circle cx="170" cy="95" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Barley.png")}" x="160" y="85" width="20" height="20"/>

            <circle cx="245" cy="95" r="14" fill="none" stroke="#73cf42" stroke-width="1" stroke-dasharray="2,2"/>
            <image href="{ic("Flax.png")}" x="235" y="85" width="20" height="20"/>
          </g>

          <!-- Beehive Radius Schematic -->
          <g transform="translate(490, 115)">
            <circle cx="0" cy="0" r="75" fill="rgba(235, 181, 56, 0.08)" stroke="#ebb538" stroke-width="1.5" stroke-dasharray="4,4"/>
            <circle cx="0" cy="0" r="24" fill="#181c24" stroke="#5a4220" stroke-width="1.5"/>
            <image href="{ic("Honey.png")}" x="-14" y="-14" width="28" height="28"/>
            <text x="0" y="36" font-family="Norsebold" font-size="8.5" fill="#ebb538" text-anchor="middle">BEEHIVE (MAX 4 HONEY)</text>
            <text x="0" y="50" font-family="Averia Serif Libre" font-weight="bold" font-size="7.5" fill="#d2c5b0" text-anchor="middle">Open sky clearance required</text>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">AGRICULTURAL SPECIFICATIONS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F09 SOIL CULTIVATION: Tills grass into fertile loam.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F10 1.2M GRID PLANTING: Eliminates cramped crop wither.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F11 HARVEST PASS: Gathers mature food without plant harm.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F12 SEED FLOWER ROTATION: 50% harvest re-seeded into flowers.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F13 APIARY HARVEST: Gathers honey from all beehives within 25m.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; BIOME CONSTRAINTS: Barley &amp; Flax require Plains soil.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; ZERO FOOD WITHER: Crops never decay once fully grown.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; CULTIVATOR REPAIR: Routes to Forge when durability &lt; 10%.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; RAIN BENEFIT: Rain accelerates botanical hydration.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#73cf42">CIRCUIT CONDUITS &amp; PANTRY LINKS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CAULDRON CONDUIT: Feeds turnips, onions &amp; honey to Cook.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; RANCH TAME FEEDER: Supplies boars with carrots &amp; lox with barley.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; TAILOR WEAVE SUPPLY: Routes raw flax to spinning wheels.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; BEE HAPPINESS LAW: Beehives require 4m clear open air radius.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; STAKEWALL PERIMETER: Enclose crops to stop boar trampling.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HAULER CONDUIT: Haulers vacuum vegetable drops into cellars.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; STORAGE TAGS: Mark chests [Produce], [Seed], and [Honey].</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; SEED CONSERVATION: Never consumes seeds for cooking recipes.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; NOCTURNAL REST: Resumes seeding at 06:00 sunrise.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=9,
        title="SCHEMA 3: AGRICULTURAL 1.2M GRID",
        sub="BOTANICAL MATRICES &bull; 50% SEED CYCLE &bull; APIARY ECOLOGY",
        pill="AGRICULTURE PROCESS",
        icon_name="Cultivator.png",
        svg_content=svg,
        annotation_icon="Carrot.png",
        annotation_text="BOTANICAL DENSITY LAW: Always space crops at least 1.2m apart. Crops planted too close immediately wither, yielding zero produce."
    )

# ==================== PLATE V: METALLURGY SCHEMA 4 ====================

def make_plate_5():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#ea8a38" letter-spacing="0.08em">SCHEMA 4: HIGH-HEAT PYROLYSIS &amp; METALLURGY</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Kiln Combustion &bull; 8m Supply Radius &bull; 1:2 Ore/Coal Stoichiometry &bull; Ingot Vaults</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: KILN PYROLYSIS", "Loads 25 wood into Charcoal Kiln; burns timber into 25 coal chunks.", "Coal.png", "25 WOOD = 25 COAL", "#ea8a38", "orange")}
      {svg_box(255, 48, 210, 95, "STEP 02: 8M CHEST PROXIMITY", "Pulls raw ores and coal from supply boxes within 8m. Eliminates delays.", "Chest.png", "PROXIMITY: &lt;= 8.0M", "#55c9ea", "cyan")}
      {svg_box(485, 48, 210, 95, "STEP 03: 1:2 STOICHIOMETRY", "Maintains combustion ratio: 1 Ore chunk : 2 Coal chunks loaded into hoppers.", "Smelter.png", "COMBUSTION BALANCE", "#ebb538", "gold")}

      {svg_conduit("M 235 95 L 255 95", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 465 95 L 485 95", "#55c9ea", "arrow-cyan")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#ebb538", "arrow-gold")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: BLAST FURNACE REFINE", "Refines high-tier Black Metal scrap & Flametal with high-temperature blast fuel.", "Blast_Furnace.png", "BLACK METAL REFINING", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: SLAG TAPPING RUN", "Collects cooling molten metal bars directly from run-off troughs into carts.", "Iron.png", "BAR EXTRACTION", "#73cf42", "green")}
      {svg_box(25, 175, 210, 95, "STEP 06: [INGOT] VAULT DEPOSIT", "Consolidates finished bronze, iron & silver bars into fortified vaults.", "Copper.png", "TAG: [INGOT]", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#73cf42", "arrow-green")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">FOUNDRY QUADRANT BLUEPRINT &amp; 8-METER INGESTION CONDUIT</text>

        <!-- Visual Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- 8m Ingestion Radius -->
          <g transform="translate(317, 118)">
            <circle cx="0" cy="0" r="95" fill="rgba(85, 201, 234, 0.06)" stroke="#55c9ea" stroke-width="1.5" stroke-dasharray="4,4"/>
            <text x="0" y="-103" font-family="Norsebold" font-size="8.5" fill="#55c9ea" text-anchor="middle">8.0M INGESTION PERIMETER (ZERO WALKING DELAY)</text>

            <!-- Central Smelter Core -->
            <rect x="-26" y="-26" width="52" height="52" rx="6" fill="#1b1e26" stroke="#ea8a38" stroke-width="2"/>
            <image href="{ic("Smelter.png")}" x="-18" y="-18" width="36" height="36"/>
            <text x="0" y="38" font-family="Norsebold" font-size="8.5" fill="#f7d57f" text-anchor="middle">SMELTER CORE</text>

            <!-- Raw Ore Chest (West) -->
            <rect x="-85" y="-16" width="32" height="32" rx="3" fill="#13161c" stroke="#5a4220" stroke-width="1"/>
            <image href="{ic("Chest.png")}" x="-80" y="-11" width="22" height="22"/>
            <text x="-69" y="28" font-family="Norsebold" font-size="7.5" fill="#55c9ea" text-anchor="middle">[ORE]</text>
            <line x1="-53" y1="0" x2="-26" y2="0" stroke="#73cf42" stroke-width="2" marker-end="url(#arrow-green)"/>

            <!-- Coal Chest (East) -->
            <rect x="53" y="-16" width="32" height="32" rx="3" fill="#13161c" stroke="#5a4220" stroke-width="1"/>
            <image href="{ic("Chest.png")}" x="58" y="-11" width="22" height="22"/>
            <text x="69" y="28" font-family="Norsebold" font-size="7.5" fill="#ea8a38" text-anchor="middle">[COAL]</text>
            <line x1="53" y1="0" x2="26" y2="0" stroke="#ea8a38" stroke-width="2" marker-end="url(#arrow-orange)"/>

            <!-- Kiln Pair (North) -->
            <rect x="-16" y="-88" width="32" height="32" rx="3" fill="#13161c" stroke="#ea8a38" stroke-width="1"/>
            <image href="{ic("Coal.png")}" x="-11" y="-83" width="22" height="22"/>
            <text x="0" y="-62" font-family="Norsebold" font-size="7.5" fill="#ea8a38" text-anchor="middle">KILN FEED</text>
            <line x1="0" y1="-56" x2="0" y2="-26" stroke="#ea8a38" stroke-width="2" marker-end="url(#arrow-orange)"/>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">METALLURGICAL BENCHMARKS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; BRONZE REFINERY: Requires 2x Copper + 1x Tin at Forge.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SCRAP IRON: Swamp Crypt mud scraps refined in Smelter.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SILVER INGOTS: Mountain veins smelted into frost-gear.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; BLACK METAL: Requires high-temp Blast Furnace stoking.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FOUNDRY PAIRING: 2 Kilns per 1 Smelter stops fuel delay.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; SMELTER CAPACITY: Max 20 Ore | Max 25 Coal | ~30s tick.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; ZERO METAL DESPAWN: Refined bars persist forever on ground.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; AUTOMATED TAPPING: Bars eject directly into cargo carts.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FUEL BALANCE: Smelter never accepts wood directly; only coal.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#ea8a38">LOGISTICS &amp; STORAGE ARCHITECTURE</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 8M RADIUS LAW: Chests outside 8m cause walking delays.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; DIRECT FORGE CONDUIT: Bars route straight to blacksmiths.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HAULER CART DROP: Raw ore wagons unload directly in yard.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; TIMBER SINK: Consumes surplus wood piles for unlimited coal.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SMOKE VENTILATION: High chimney required to stop suffocation.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; NOCTURNAL SHUTDOWN: Kilns burn down safely during night.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; TAG MATCHING: Mark output chests [Ingot] or [Metal].</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; HOPPER FILL LOGIC: Re-fills immediately when space opens.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; BLAST HOPPER: Artisan Table unlocks blast furnace automation.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=12,
        title="SCHEMA 4: METALLURGICAL PYROLYSIS",
        sub="HIGH-HEAT PYROLYSIS &bull; 8M CHEST INGESTION &bull; 1:2 STOICHIOMETRY",
        pill="METALLURGY PROCESS",
        icon_name="Smelter.png",
        svg_content=svg,
        annotation_icon="Coal.png",
        annotation_text="FOUNDRY PROXIMITY LAW: Keep raw ore and coal chests within 8m of smelter hoppers. Greater distances waste worker stamina in transit."
    )

# ==================== PLATE VI: CULINARY SCHEMA 5 ====================

def make_plate_6():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#ea8a38" letter-spacing="0.08em">SCHEMA 5: CULINARY TIMERS &amp; MEAD FERMENTATION</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Spit Timers &bull; Cauldron Simmering &bull; 2400s Mead Barrels &bull; 3-Slot Banquets</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: SPIT ROAST TIMING", "Roasts meats on open cooking spits; pulls at 20s to eliminate charcoal waste.", "Raw_Meat.png", "0-CHAR SPIT TIMER", "#ea8a38", "orange")}
      {svg_box(255, 48, 210, 95, "STEP 02: CAULDRON SIMMERING", "Combines vegetables, honey, and meats into high-tier HP and stamina stews.", "Deer_Stew.png", "CAULDRON RECIPES", "#ebb538", "gold")}
      {svg_box(485, 48, 210, 95, "STEP 03: STONE OVEN BAKING", "Bakes bread dough, lox meat pies, and fish wraps in wood-fired bread ovens.", "Serpent_Stew.png", "OVEN BAKERY", "#73cf42", "green")}

      {svg_conduit("M 235 95 L 255 95", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 465 95 L 485 95", "#ebb538", "arrow-gold")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#73cf42", "arrow-green")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: MEAD BASE BREWING", "Simmers honey, berries, and bloodbags into potent medicinal base mixtures.", "Honey.png", "CAULDRON FERMENT BASE", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: 2400S MEAD AGING", "Ferments mead bases in oak casks for 2400 seconds (2 in-game days).", "Mead_Health.png", "CYCLE: 2400 SECONDS", "#55c9ea", "cyan")}
      {svg_box(25, 175, 210, 95, "STEP 06: LARDER RATIONING", "Sorts rations into cold storage chests tagged with [Food] and [Mead].", "Sausages.png", "TAG: [FOOD] / [MEAD]", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#55c9ea", "arrow-cyan")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">KITCHEN HEARTH &amp; MEAD FERMENTATION CYCLE SPECIFICATION</text>

        <!-- Visual Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Spit Timing Dial -->
          <g transform="translate(150, 118)">
            <circle cx="0" cy="0" r="85" fill="#13161c" stroke="#5a4220" stroke-width="1.5"/>
            <!-- Raw Zone -->
            <path d="M 0 0 L 0 -85 A 85 85 0 0 1 85 0 Z" fill="rgba(224, 72, 54, 0.15)"/>
            <!-- Cooked Zone (Sweet Spot) -->
            <path d="M 0 0 L 85 0 A 85 85 0 0 1 0 85 Z" fill="rgba(115, 207, 66, 0.25)"/>
            <!-- Coal Zone (Burned) -->
            <path d="M 0 0 L 0 85 A 85 85 0 0 1 -85 0 Z" fill="rgba(30, 30, 30, 0.5)"/>

            <text x="40" y="-40" font-family="Norsebold" font-size="8" fill="#e04836">RAW (0-19S)</text>
            <text x="45" y="45" font-family="Norsebold" font-size="8" fill="#73cf42">COOKED (20S)</text>
            <text x="-45" y="45" font-family="Norsebold" font-size="8" fill="#888888">COAL (40S)</text>

            <line x1="0" y1="0" x2="62" y2="28" stroke="#ebb538" stroke-width="3.5" marker-end="url(#arrow-gold)"/>
            <circle cx="0" cy="0" r="9" fill="#ebb538"/>
            <text x="0" y="102" font-family="Norsebold" font-size="8.5" fill="#f7d57f" text-anchor="middle">SPIT FLIP AI (PULL AT 20.0S)</text>
          </g>

          <!-- Fermenter 2400s Gauge -->
          <g transform="translate(450, 118)">
            <rect x="-100" y="-65" width="200" height="130" rx="6" fill="#13161c" stroke="#55c9ea" stroke-width="1.5"/>
            <image href="{ic("Fermenter.png")}" x="-26" y="-55" width="52" height="52"/>
            <text x="0" y="16" font-family="Norsebold" font-size="9.5" fill="#55c9ea" text-anchor="middle">FERMENTER CELLAR</text>
            
            <!-- Progress Bar -->
            <rect x="-80" y="28" width="160" height="14" rx="3" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
            <rect x="-78" y="30" width="120" height="10" rx="2" fill="#55c9ea"/>
            <text x="0" y="56" font-family="Norsebold" font-size="8" fill="#d2c5b0" text-anchor="middle">2400S (2 VALHEIM DAYS) &bull; 6 POTIONS</text>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">CULINARY LAWS &amp; TIMINGS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F23 SPIT ROASTING: Pulls meats instantly at 20s (0% burn).</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F24 CAULDRON STEWS: Simmers deer, sausages, and turnip stews.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F25 STONE OVEN: Bakes bread loaves and lox meat pies.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F26 FERMENTER MEAD: Ages frost, poison, and healing meads.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F27 LARDER SORTING: Routes cooked food to [Food] boxes.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; 3-SLOT DIET LAW: 1 HP + 1 Stamina + 1 Eitr maximizes stats.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; ZERO FOOD SPOILAGE: Cooked food never rots in chest storage.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; CAULDRON EXTENSION: Level 4 Cauldron unlocks Serpent Stew.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; AUTO-LOGGING: Feeds wood to oven fire autonomously.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#ea8a38">PROVISIONS &amp; HEARTH SYNERGIES</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; RANCHER MEAT INGESTION: Consumes raw boar, wolf, and lox.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FARMER PRODUCE PIPELINE: Converts raw crops into dishes.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FISHER FILLET INTEGRATION: Uses raw fish for Fish Wraps.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SENTRY MEAD RATIONING: Supplies healing mead to watchtowers.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CHIMNEY VENTILATION: Maintain roof vents to prevent smoke.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HAULER CONDUIT: Empties oven and cauldons to central vaults.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; COLD STORAGE: Keep larders roofed for 100% rest buffs.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; FERMENTER ROTATION: Run 3 fermenters to stagger mead yields.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; NOCTURNAL SHUTDOWN: Extinguishes spit flames during night.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=14,
        title="SCHEMA 5: CULINARY TIMERS &amp; MEAD",
        sub="SPIT ROAST TIMERS &bull; CAULDRON RECIPES &bull; 2400S MEAD AGING",
        pill="CULINARY PROCESS",
        icon_name="Cauldron.png",
        svg_content=svg,
        annotation_icon="Deer_Stew.png",
        annotation_text="CHEF'S TIMER LAW: Worker AI pulls roasting meat at exactly 20.0 seconds, yielding perfectly cooked rations with 0% charcoal loss."
    )

# ==================== PLATE VII: LOGISTICS SCHEMA 6 ====================

def make_plate_7():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#ebb538" letter-spacing="0.08em">SCHEMA 6: GROUND VACUUM &amp; FREIGHT ROUTING [TAGS]</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">25m Field Vacuum &bull; 1000kg Cart Freight &bull; Sign Tag Regex &bull; Multi-Box Balancing</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: 25M GROUND VACUUM", "Sweeps settlement ground; vacuums loose wood, stone, resin & drops.", "Cart.png", "SPHERE: 25M", "#ebb538", "gold")}
      {svg_box(255, 48, 210, 95, "STEP 02: CART HITCHING &amp; LOAD", "Hitches to 2-wheeled wooden carts; loads heavy ores, metals & stones.", "Chest.png", "CAPACITY: 1000 KG", "#55c9ea", "cyan")}
      {svg_box(485, 48, 210, 95, "STEP 03: WEIGHT AUDIT PASS", "Monitors cart cargo weight to maintain 100% hauling mobility on ramps.", "Hammer.png", "MASS BALANCE", "#73cf42", "green")}

      {svg_conduit("M 235 95 L 255 95", "#ebb538", "arrow-gold")}
      {svg_conduit("M 465 95 L 485 95", "#55c9ea", "arrow-cyan")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#73cf42", "arrow-green")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: CHEST BALANCING", "Equalizes item stacks across multiple overflow boxes to prevent clogs.", "Reinforced_Chest.png", "LOAD BALANCING", "#ebb538", "gold")}
      {svg_box(255, 175, 210, 95, "STEP 05: SIGN TAG REGEX", "Reads sign tags above chests (e.g. [Wood], [Ore]); routes cargo with 100% accuracy.", "Sign.png", "REGEX: [ITEM]", "#55c9ea", "cyan")}
      {svg_box(25, 175, 210, 95, "STEP 06: STRONGHOLD VAULT ROUTE", "Delivers refined ingots, armors & food to fortified interior vaults.", "Black_Metal_Chest.png", "SECURE CITADEL", "#ea8a38", "orange")}

      {svg_conduit("M 485 222 L 465 222", "#ebb538", "arrow-gold")}
      {svg_conduit("M 255 222 L 235 222", "#55c9ea", "arrow-cyan")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ea8a38", "arrow-orange")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">FREIGHT DEPOT ARCHITECTURE &amp; CONTAINER [TAG] REGEX</text>

        <!-- Visual Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Roadway with Cart -->
          <g transform="translate(40, 155)">
            <line x1="0" y1="20" x2="554" y2="20" stroke="#73cf42" stroke-width="2.5"/>
            <text x="10" y="36" font-family="Norsebold" font-size="8" fill="#73cf42">PLANED 26&deg; TRANSIT ROADWAY (40% CART SPEED BUFF)</text>

            <!-- 2-Wheeled Cart Graphic -->
            <rect x="180" y="-38" width="85" height="38" rx="3" fill="#2d1f10" stroke="#5a4220" stroke-width="1.5"/>
            <circle cx="202" cy="4" r="14" fill="#181c24" stroke="#5a4220" stroke-width="2"/>
            <circle cx="242" cy="4" r="14" fill="#181c24" stroke="#5a4220" stroke-width="2"/>
            <line x1="265" y1="-18" x2="315" y2="-6" stroke="#8b5a2b" stroke-width="3"/>
            <image href="{ic("Cart.png")}" x="195" y="-32" width="30" height="30"/>
            <text x="222" y="-45" font-family="Norsebold" font-size="8.5" fill="#f7d57f" text-anchor="middle">TWO-WHEELED FREIGHT CART (MAX 1000 KG)</text>
          </g>

          <!-- Bracket Regex Flow Box -->
          <g transform="translate(50, 30)">
            <rect x="0" y="0" width="534" height="65" rx="4" fill="#13161c" stroke="#55c9ea" stroke-width="1"/>
            <text x="18" y="24" font-family="Norsebold" font-size="10" fill="#55c9ea">SIGN TAG REGEX ENGINE: &#91;([A-Za-z0-9_-]+)&#93;</text>
            <text x="18" y="44" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#ede2d0">Workers parse bracketed sign text above containers to route mixed inventory with 100% precision.</text>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">LOGISTICS ROUTING RULES</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F33 GROUND VACUUM: Clears all dropped items within 25m.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F34 CART HAULING: Hitches to wagons; relocates 1000kg.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F35 TAG SORTING: Case-insensitive regex bracket matching.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F36 DEPOT CONDUIT: Relocates goods to central vaults.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ROADWAY SPEED BUFF: Carts move 40% faster on stone paths.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; 2M DOORWAY CLEARANCE: Carts require 2m wide openings.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; ZERO ITEM LOSS: Dropped materials never despawn in camps.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; MULTI-CART SUPPORT: Haulers switch carts when full.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; COLLISION EVASION: Paths around obstacles cleanly.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#ebb538">CONTAINER TAG SYNTAX TABLE</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [WOOD] &amp; [FINEWOOD]: Round logs, core wood &amp; fine wood.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [ORE] &amp; [STONE]: Copper, tin, iron, silver &amp; rocks.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [PRODUCE] &amp; [SEED]: Vegetables, barley, flax &amp; seeds.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [MEAT] &amp; [FEED]: Raw cuts, neck tails &amp; animal feed.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [INGOT] &amp; [TOOLS]: Refined metal bars &amp; forged gear.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; [REPAIR]: Damaged items routed for zero-cost repairs.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; [ARROWS]: Ammunition bins for watchtower sentries.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; OVERFLOW FALLBACK: Builds 50x piles if boxes are full.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; CASE INSENSITIVE: [wood] matches [WOOD] perfectly.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=18,
        title="SCHEMA 6: GROUND VACUUM &amp; FREIGHT",
        sub="FIELD VACUUM &bull; 1000KG CART FREIGHT &bull; SIGN [TAG] REGEX",
        pill="LOGISTICS PROCESS",
        icon_name="Cart.png",
        svg_content=svg,
        annotation_icon="Sign.png",
        annotation_text="SIGN ROUTING LAW: Surround item names with square brackets e.g. [Wood], [Ore], [Ingot]. Hauler AI automatically routes cargo to matching containers."
    )

# ==================== PLATE VIII: DEFENSE SCHEMA 7 ====================

def make_plate_8():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#55c9ea" letter-spacing="0.08em">SCHEMA 7: CITADEL DEFENSE &amp; SIGHTLINES</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">40m Threat Acquisition &bull; Intersecting Sightlines &bull; Shield Wall Intercept &bull; Spoils Pass</text>
      </g>

      <!-- Row 1: Steps 1, 2, 3 (Left-to-Right) -->
      {svg_box(25, 48, 210, 95, "STEP 01: WATCHTOWER VIGIL", "Mans elevated battlements; scans wilderness with 360-degree perimeter vision.", "Finewood_Bow.png", "VIGIL: 360&deg;", "#55c9ea", "cyan")}
      {svg_box(255, 48, 210, 95, "STEP 02: 40M THREAT ACQUISITION", "Identifies Greydwarfs, Trolls & nocturnal predators up to 40m away.", "Greydwarf.png", "TARGET RANGE: 40M", "#e04836", "orange")}
      {svg_box(485, 48, 210, 95, "STEP 03: AIMED RANGED VOLLEYS", "Draws bows to suppress enemy targets with fire, flint, and needle arrows.", "Fire_Arrow.png", "ARROW SUPPRESSION", "#ebb538", "gold")}

      {svg_conduit("M 235 95 L 255 95", "#55c9ea", "arrow-cyan")}
      {svg_conduit("M 465 95 L 485 95", "#e04836", "arrow-orange")}
      
      <!-- S-Curve Conduit: Row 1 down to Row 2 -->
      {svg_conduit("M 590 143 L 590 175", "#ebb538", "arrow-gold")}

      <!-- Row 2: Steps 4, 5, 6 (Right-to-Left Snake Flow) -->
      {svg_box(485, 175, 210, 95, "STEP 04: MELEE INTERCEPT", "Swaps to blade & shield to engage breaches before monsters touch workers.", "Iron_Sword.png", "MELEE ENGAGEMENT", "#ea8a38", "orange")}
      {svg_box(255, 175, 210, 95, "STEP 05: SHIELD WALL ENGAGE", "Engages breaches with Banded Shield; draws hostile aggro away from civilians.", "Banded_Shield.png", "AGGRO INTERCEPTION", "#55c9ea", "cyan")}
      {svg_box(25, 175, 210, 95, "STEP 06: SPOILS SWEEP PASS", "Secures drops (eyes, resin, bones, trophies); routes loot to barracks.", "Bone_Fragments.png", "POST-RAID SWEEP", "#ebb538", "gold")}

      {svg_conduit("M 485 222 L 465 222", "#ea8a38", "arrow-orange")}
      {svg_conduit("M 255 222 L 235 222", "#55c9ea", "arrow-cyan")}
      
      <!-- Conduit from Step 06 into Blueprint Container -->
      {svg_conduit("M 130 270 L 130 300", "#ebb538", "arrow-gold")}

      <!-- Lower Technical Blueprint & Spatial Diagram (Height = 580px!) -->
      <g transform="translate(25, 300)">
        <rect x="0" y="0" width="670" height="580" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">CITADEL DEFENSE GEOMETRY &amp; INTERSECTING SIGHTLINES</text>

        <!-- Visual Schematic Canvas -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="235" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- Fortress Walls Layout -->
          <g transform="translate(317, 118)">
            <!-- Citadel Wall Outer Box -->
            <rect x="-150" y="-75" width="300" height="150" rx="4" fill="#13161c" stroke="#5a4220" stroke-width="2"/>
            <text x="0" y="-83" font-family="Norsebold" font-size="8.5" fill="#d2c5b0" text-anchor="middle">STRONGHOLD PALISADE &amp; STONE BASTION</text>

            <!-- 4 Corner Watchtowers -->
            <!-- NW Tower -->
            <rect x="-165" y="-90" width="32" height="32" rx="3" fill="#1b1e26" stroke="#55c9ea" stroke-width="1.5"/>
            <image href="{ic("Finewood_Bow.png")}" x="-159" y="-84" width="20" height="20"/>
            <path d="M -150 -75 L -250 -125 A 150 150 0 0 1 -190 -155 Z" fill="rgba(85, 201, 234, 0.15)"/>

            <!-- NE Tower -->
            <rect x="133" y="-90" width="32" height="32" rx="3" fill="#1b1e26" stroke="#55c9ea" stroke-width="1.5"/>
            <image href="{ic("Finewood_Bow.png")}" x="139" y="-84" width="20" height="20"/>
            <path d="M 150 -75 L 250 -125 A 150 150 0 0 0 190 -155 Z" fill="rgba(85, 201, 234, 0.15)"/>

            <!-- SW Tower -->
            <rect x="-165" y="58" width="32" height="32" rx="3" fill="#1b1e26" stroke="#55c9ea" stroke-width="1.5"/>
            <image href="{ic("Finewood_Bow.png")}" x="-159" y="64" width="20" height="20"/>
            <path d="M -150 75 L -250 125 A 150 150 0 0 0 -190 155 Z" fill="rgba(85, 201, 234, 0.15)"/>

            <!-- SE Tower -->
            <rect x="133" y="58" width="32" height="32" rx="3" fill="#1b1e26" stroke="#55c9ea" stroke-width="1.5"/>
            <image href="{ic("Finewood_Bow.png")}" x="139" y="64" width="20" height="20"/>
            <path d="M 150 75 L 250 125 A 150 150 0 0 1 190 155 Z" fill="rgba(85, 201, 234, 0.15)"/>

            <!-- Center Safe Domain -->
            <circle cx="0" cy="0" r="48" fill="rgba(115, 207, 66, 0.08)" stroke="#73cf42" stroke-width="1.5" stroke-dasharray="3,3"/>
            <text x="0" y="4" font-family="Norsebold" font-size="8.5" fill="#73cf42" text-anchor="middle">SAFE INTERIOR DOMAIN</text>
            <text x="0" y="19" font-family="Averia Serif Libre" font-weight="bold" font-size="7.5" fill="#ede2d0" text-anchor="middle">Civilian workers unhindered</text>

            <!-- 40m Sightline Arc Annotations -->
            <text x="0" y="-50" font-family="Norsebold" font-size="8" fill="#55c9ea" text-anchor="middle">INTERSECTING 40M SIGHTLINES (ZERO DEAD ZONES)</text>
          </g>
        </g>

        <!-- Specifications Stele (Height = 275px!) -->
        <g transform="translate(18, 285)">
          <!-- Left Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#55c9ea">PERIMETER DEFENSE SPECIFICATIONS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F37 PERIMETER PATROL: Walks ramparts on programmed paths.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F38 RANGED SUPPRESSION: Aimed bow volleys neutralize threats.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F39 AGGRO INTERCEPT: Banded Shield blocks protect civilians.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; F40 POST-RAID SWEEPER: Secures combat trophies &amp; monster drops.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; 40M TARGET RANGE: Acquires nocturnal predators before breaches.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; HEALTH SAFETY RETREAT: Disengages when HP &lt; 30% to heal.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; NIGHT VIGIL: Sentries remain awake through night shifts.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; ARMORY REFILL: Auto-pulls arrows from [Arrows] chests.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CITADEL WALLS: Craftsman keeps palisades at 100% health.</text>
        </g>

        <g transform="translate(342, 285)">
          <!-- Right Column -->
          <rect x="0" y="0" width="310" height="275" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="14" y="22" font-family="Norsebold" font-size="10" fill="#73cf42">LOGISTICS &amp; ARMORY CONDUITS</text>
          <text x="14" y="48" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; ARROW AMMUNITION BOXES: Auto-refills from [Arrows] chests.</text>
          <text x="14" y="74" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; CRAFTSMAN ARMORY FORGE: Receives repaired shields &amp; swords.</text>
          <text x="14" y="100" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; WALL INTEGRITY ALLIANCE: Craftsman restores gate damage.</text>
          <text x="14" y="126" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; RAID SPOILS SUPPLY: Delivers eyes and resin to kitchen &amp; forge.</text>
          <text x="14" y="152" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; PERIMETER BRAZIERS: Standing torches eliminate stealth bonuses.</text>
          <text x="14" y="178" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; COVERED TOWER ROOFS: Protects sentries from rain &amp; drakes.</text>
          <text x="14" y="204" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; CHOKEPOINTS: Channel attackers through 2m fortified gates.</text>
          <text x="14" y="230" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; LEASH RADIUS: Sentries never pursue beyond 40m perimeter.</text>
          <text x="14" y="256" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#73cf42">&bull; STAGGER MITIGATION: Banded shield absorbs troll club blows.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=20,
        title="SCHEMA 7: CITADEL DEFENSE GEOMETRY",
        sub="WATCHTOWER VIGIL &bull; 40M SIGHTLINES &bull; INTERSECTING ARCS",
        pill="DEFENSE PROCESS",
        icon_name="Finewood_Bow.png",
        svg_content=svg,
        annotation_icon="Banded_Shield.png",
        annotation_text="DEFENSE GEOMETRY LAW: Position watchtowers at diagonal stronghold corners so archer sightlines cross-cover all gate approaches without dead zones."
    )

# ==================== PAGE 21: SPATIAL BLUEPRINTS & ELEVATIONS ====================

def make_page_21():
    svg = f"""
      <rect width="100%" height="100%" fill="#101318"/>

      <!-- Header Banner -->
      <g transform="translate(25, 14)">
        <text x="0" y="14" font-family="Norsebold" font-size="13" fill="#f7d57f" letter-spacing="0.08em">SETTLEMENT SPATIAL ARCHITECTURE &amp; 3D ELEVATION BLUEPRINT</text>
        <text x="0" y="27" font-family="Averia Serif Libre" font-weight="bold" font-size="8.5" fill="#a89a80">Vertical Elevation Bounds &bull; 26&deg; Ramp Physics &bull; Navmesh Grids &bull; Comfort Radii</text>
      </g>

      <!-- Upper Visual Elevation Blueprint -->
      <g transform="translate(25, 45)">
        <rect x="0" y="0" width="670" height="350" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">THREE-TIER VERTICAL ELEVATION PROFILES &amp; CART TRACTION</text>

        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="634" height="290" rx="4" fill="#080a0e" stroke="#2a2318" stroke-width="1"/>
          
          <!-- +12.0m Watchtower Level -->
          <line x1="40" y1="50" x2="594" y2="50" stroke="#ebb538" stroke-width="2" stroke-dasharray="6,4"/>
          <text x="45" y="42" font-family="Norsebold" font-size="8.5" fill="#ebb538">+12.0M: WATCHTOWER ARCHERY PLATFORM (360&deg; SIGHTLINE TO 40M)</text>

          <!-- 0.0m Stronghold Ground Level -->
          <line x1="40" y1="140" x2="280" y2="140" stroke="#73cf42" stroke-width="3"/>
          <line x1="480" y1="140" x2="594" y2="140" stroke="#73cf42" stroke-width="3"/>
          <text x="45" y="132" font-family="Norsebold" font-size="8.5" fill="#73cf42">0.0M: STRONGHOLD GROUND LEVEL (25M WORK POST SPHERICAL DOMAIN)</text>

          <!-- 26 Degree Ramped Roadway connecting 0.0m to -4.5m -->
          <line x1="280" y1="140" x2="480" y2="230" stroke="#ea8a38" stroke-width="3.5"/>
          <text x="400" y="175" font-family="Norsebold" font-size="9" fill="#ea8a38">26&deg; HOE PLANED CART RAMP</text>
          <text x="400" y="190" font-family="Averia Serif Libre" font-weight="bold" font-size="7.5" fill="#d2c5b0">40% speed buff; zero wheel stall</text>

          <!-- -4.5m Subsurface Quarry Floor -->
          <line x1="480" y1="230" x2="594" y2="230" stroke="#55c9ea" stroke-width="2"/>
          <text x="485" y="222" font-family="Norsebold" font-size="8" fill="#55c9ea">-4.5M: PLANED QUARRY FLOOR</text>

          <!-- -8.0m World Bedrock Floor -->
          <line x1="40" y1="265" x2="594" y2="265" stroke="#e04836" stroke-width="2" stroke-dasharray="6,4"/>
          <text x="45" y="257" font-family="Norsebold" font-size="8.5" fill="#e04836">-8.0M: WORLD BEDROCK BOUNDARY (EXCAVATION CUTOFF CEILING)</text>

          <!-- Vertical Measurement Lines -->
          <line x1="30" y1="50" x2="30" y2="265" stroke="#a89676" stroke-width="1.5" marker-start="url(#arrow-gold)" marker-end="url(#arrow-gold)"/>
          <text x="25" y="160" font-family="Norsebold" font-size="7.5" fill="#a89676" transform="rotate(-90, 25, 160)" text-anchor="middle">20M TOTAL OPERATING HEIGHT</text>
        </g>
      </g>

      <!-- Lower Specifications Stele -->
      <g transform="translate(25, 415)">
        <rect x="0" y="0" width="670" height="465" rx="6" fill="#0d1015" stroke="#5a4220" stroke-width="1.5"/>
        <text x="18" y="24" font-family="Norsebold" font-size="11.5" fill="#f7d57f" letter-spacing="0.06em">NAVMESH SNAPPING &amp; SETTLEMENT SPATIAL PARAMETERS</text>

        <!-- Left Column -->
        <g transform="translate(18, 38)">
          <rect x="0" y="0" width="310" height="405" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="15" y="24" font-family="Norsebold" font-size="10" fill="#55c9ea">SPATIAL SNAPPING &amp; NAVMESH LAWS</text>
          
          <text x="15" y="52" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; 25M WORK SPHERE: Work posts radiate a 25m sphere.</text>
          <text x="15" y="78" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; 4M POST SNAPPING: Work posts snap to 4m wood grid.</text>
          <text x="15" y="104" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; 2M DOORWAYS: Doors must be 2m wide for cart clearance.</text>
          <text x="15" y="130" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; STAIRWAY RAMPS: Replace steep 45&deg; stairs with 26&deg; ramps.</text>
          <text x="15" y="156" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; 8M CHEST INGESTION: Smelter chests must sit &lt;= 8m away.</text>
          <text x="15" y="182" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; ZERO RELOCATION PENALTY: Break &amp; move posts anytime.</text>
          <text x="15" y="208" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#73cf42">&bull; NAVMESH REBUILD: Press F5 and enter worker_reset.</text>
          
          <text x="15" y="240" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; ROOF COVERAGE: 100% shelter stops rain durability rot.</text>
          <text x="15" y="266" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; WORKBENCH SPHERE: 1 Workbench covers 20m building zone.</text>
          <text x="15" y="292" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; CHIMNEY CLEARANCE: 2x2m smoke vents prevent choke.</text>
          <text x="15" y="318" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; CHOPPING BLOCK: Station upgrade required for Bronze tools.</text>
          <text x="15" y="344" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; ANVIL EXPANSION: Level 2 Forge enables Iron tool repair.</text>
          <text x="15" y="370" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ebb538">&bull; MAXIMUM WORKERS: Caps at 12 active workers per 64m zone.</text>
        </g>

        <!-- Right Column -->
        <g transform="translate(342, 38)">
          <rect x="0" y="0" width="310" height="405" rx="4" fill="#13161c" stroke="#2d251a" stroke-width="1"/>
          <text x="15" y="24" font-family="Norsebold" font-size="10" fill="#73cf42">COMFORT, SHELTER &amp; REST CYCLES</text>

          <text x="15" y="52" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; SHELTER BUFF: Roof + walls provide 100% rest recovery.</text>
          <text x="15" y="78" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; DRAGON BED: Assign workers to beds for instant stamina.</text>
          <text x="15" y="104" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; HEARTH PROXIMITY: 14m warmth radius stops freezing.</text>
          <text x="15" y="130" font-family="Averia Serif Libre" font-weight="bold" font-size="9" fill="#ede2d0">&bull; NOCTURNAL SLEEP: Civilian workers sleep 21:00 to 06:00.</text>
          <text x="15" y="156" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; SENTRY VIGIL: Sentries remain awake through night shifts.</text>
          <text x="15" y="182" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; HYPOTHERMIA IMMUNITY: 3-slot food sets negate cold status.</text>
          <text x="15" y="208" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ebb538">&bull; CAMP INTEGRITY: 1 upgraded Workbench covers 20m building.</text>

          <text x="15" y="240" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; COMFORT LEVEL 17: Dragon beds &amp; rugs yield 24min buff.</text>
          <text x="15" y="266" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; BRAZIER LIGHT: Standing iron braziers warm 10m radius.</text>
          <text x="15" y="292" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; STORM BEHAVIOR: Civilians retreat indoors during storms.</text>
          <text x="15" y="318" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; REPAIR DISENGAGE: 10% durability threshold initiates retreat.</text>
          <text x="15" y="344" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#ede2d0">&bull; FOOD AUTO-FEED: Workers consume food from larder chests.</text>
          <text x="15" y="370" font-family="Averia Serif Libre" font-weight="bold" font-size="8.8" fill="#55c9ea">&bull; HUGIN NOTIFICATIONS: F5 status prints worker diagnostics.</text>
        </g>
      </g>
"""
    return make_infographic_page(
        page_num=21,
        title="SPATIAL ARCHITECTURE &amp; BLUEPRINTS",
        sub="3D ELEVATION, RAMP GRADES &amp; NAVMESH",
        pill="SPATIAL BLUEPRINTS",
        icon_name="Hammer.png",
        svg_content=svg,
        annotation_icon="Hearth.png",
        annotation_text="BLUEPRINT LAW: Maintain strict 26&deg; slopes on all roadways and mining pit ramps. Steeper grades break cart physics and cause workers to drop hitches."
    )
