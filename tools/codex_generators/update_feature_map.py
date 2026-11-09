import os, sys, re, json
import codex_plates

BASE_DIR = r"d:\valheim_mods"
MAP_PATH = os.path.join(BASE_DIR, "worker_feature_map.html")
SUBDIR_MAP = os.path.join(BASE_DIR, "ValheimWorkers", "worker_feature_map.html")

print("Reading base worker_feature_map.html...")
with open(MAP_PATH, "r", encoding="utf-8") as f:
    html = f.read()

# 1. Ensure SVG V-groove chisel filters are present in body
SVG_FILTERS = """
<svg width="0" height="0" style="position:absolute; display:block;">
  <defs>
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

    <!-- 3. WEATHERED RUST CHISEL (RED-OCHRE) -->
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

if 'id="v-groove-gold"' not in html:
    html = html.replace("<body>", "<body>\n" + SVG_FILTERS)
elif 'id="v-groove-rust"' not in html:
    # Insert v-groove-rust before </defs>
    rust_filter_def = """
    <!-- 3. WEATHERED RUST CHISEL (RED-OCHRE) -->
    <filter id="v-groove-rust" x="-15%" y="-15%" width="130%" height="130%" color-interpolation-filters="sRGB">
      <feGaussianBlur in="SourceAlpha" stdDeviation="0.8" result="blurAlpha"/>
      <feDiffuseLighting in="blurAlpha" surfaceScale="-1" diffuseConstant="0.7" lighting-color="#ffeedd" result="diffLight">
        <feDistantLight azimuth="190" elevation="44"/>
      </feDiffuseLighting>
      <feComposite in="diffLight" in2="SourceGraphic" operator="arithmetic" k1="1.1" k2="0.2" k3="0" k4="0" result="shadedFill"/>
      <feComposite in="shadedFill" in2="SourceAlpha" operator="in" result="clippedFill"/>
      <feOffset in="SourceAlpha" dx="1.8" dy="1.8" result="shOff"/>
      <feComposite in="SourceAlpha" in2="shOff" operator="out" result="tShadow"/>
      <feFlood flood-color="#050302" flood-opacity="0.95" result="darkFlood"/>
      <feComposite in="darkFlood" in2="tShadow" operator="in" result="carveShadow"/>
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
"""
    html = html.replace("</defs>", rust_filter_def + "\n  </defs>", 1)

# Update section-title style with Weathered Rust Chisel (red-ochre)
section_title_new = """.section-title {
    font-family: 'Norsebold', serif;
    font-size: 1.95rem;
    letter-spacing: 0.02em;
    color: #d64a2b;
    filter: url(#v-groove-rust);
    margin-bottom: 16px;
    display: flex;
    align-items: center;
    gap: 10px;
  }"""
html = re.sub(r'\.section-title\s*\{[^}]+\}', section_title_new, html)

# Update card-title style with Weathered Rust Chisel
card_title_new = """.card-title {
    font-size: 1.18rem;
    font-weight: bold;
    color: #d64a2b;
    font-family: 'Norsebold', serif;
    letter-spacing: 0.02em;
    filter: url(#v-groove-rust);
  }"""
html = re.sub(r'\.card-title\s*\{[^}]+\}', card_title_new, html)

# 2. Extract the 9 SVGs from codex_plates
def extract_svg(html_str):
    m = re.search(r'(<svg\b[^>]*>.*?</svg>)', html_str, re.DOTALL)
    return m.group(1) if m else ""

print("Generating 9 plate vector schemas from codex_plates...")
raw_plates = [
    ("plate-1", "Plate I: Macro Settlement Flow", "Macro Settlement Production Web", "Logistics", "All Workers", "docs/assets/icons/Workbench.png", codex_plates.make_plate_1(), [
        ("01", "Raw Harvesting", "Lumberjacks fell timber; Miners excavate veins down to bedrock."),
        ("02", "Camp Ingestion", "Gatherers & Hunters bring meats, berries, hides, and feathers to depot."),
        ("03", "Arterial Haulage", "Haulers pack bulk freight into wheeled carts for depot transit."),
        ("04", "Thermal Refining", "Smelters & Charcoal Kilns refine coal, copper, tin, bronze, iron & silver."),
        ("05", "Culinary & Crafts", "Cooks stew hearty meals at cauldrons; Craftsmen repair worn gear."),
        ("06", "Bastion Defense", "Sentries consume rations, patrol ramparts, and rain ballistic fire.")
    ], "Macro Settlement Production Web & Inter-Worker Handshake Topology", "Illustrates the camp-wide macro topology connecting extraction zones, central storage hubs, processing complexes, and fortified perimeter sentry outposts."),
    
    ("plate-2", "Plate II: Forestry Lifecycle", "Forestry Lifecycle & Lumberjack Flow", "Forestry", "Lumberjack", "docs/assets/icons/Stone_Axe.png", codex_plates.make_plate_2(), [
        ("01", "Arboreal Scan", "Scans 30m radius for mature beech, birch, oak, and fir trees."),
        ("02", "Precision Felling", "Cleaves base of trunk; calculates natural fall trajectory away from camp."),
        ("03", "Trunk Chunking", "Slices fallen logs and timber trunks into portable 50x wood units."),
        ("04", "Stump Eradication", "Obliterates remaining root stumps to maintain flat, pristine ground."),
        ("05", "Silvicultural Sowing", "Deposits excess cones into seed pantries and prepares replanting bag."),
        ("06", "Autonomous Reforestation", "Replants seeds at 3.5m spacing intervals to prevent stunted growth.")
    ], "Arboreal Canopy & Felling Clearance Radius Elevation", "Shows the 30-meter work radius, optimal 3.5m sapling grid distribution, and clean felling arc preventing structural damage to camp buildings."),
    
    ("plate-3", "Plate III: Mining Pipeline", "Subterranean Mining & Quarrying Pipeline", "Mining", "Miner", "docs/assets/icons/Antler_Pickaxe.png", codex_plates.make_plate_3(), [
        ("01", "Geological Survey", "Surveys terrain for copper deposits, tin nodes, and silver veins."),
        ("02", "Topsoil Stripping", "Clears overlying dirt layers down to bedrock to expose hidden node layers."),
        ("03", "Tiered Striking", "Strikes exposed node with antler, bronze, or iron pickaxe."),
        ("04", "Chunk Shattering", "Breaks fractured mineral blocks into raw ore and stone chunks."),
        ("05", "Terrain Smoothing", "Equips Hoe to level jagged quarry pits, preventing pathing traps."),
        ("06", "Ore Cart Haulage", "Transfers heavy ore stacks into wheeled carts for smeltery transit.")
    ], "Subterranean MineRock5 Vein Stripping & Trench Elevation", "Cross-section showing bedrock clearance, terraced bench extraction, and smoothed cart ramps for heavy haulage."),
    
    ("plate-4", "Plate IV: Agriculture Loop", "Agricultural Crop & Replanting Automation", "Agriculture", "Farmer", "docs/assets/icons/Cultivator.png", codex_plates.make_plate_4(), [
        ("01", "Arable Soil Tilling", "Equips Cultivator to prepare fertile dark earth patches."),
        ("02", "Seed Pantry Sourcing", "Withdraws carrot, turnip, onion, and barley seeds from agricultural chests."),
        ("03", "Proactive Sowing", "Sows seeds across tilled plots at optimal 2.0m spacing intervals."),
        ("04", "Growth Monitoring", "Monitors weed conditions and crop health during 4000-second maturation."),
        ("05", "Bountiful Harvesting", "Gathers mature vegetables and seed crops, splitting yield 2:1."),
        ("06", "Livestock Feeding", "Routes vegetable surplus into feeding pens for boars, lox, and chickens.")
    ], "Cultivated Farmland Grid & Livestock Trough Elevation", "Layout displaying 2m seed spacing, perimeter fence barrier against greydwarf pests, and adjacent livestock feeding troughs."),
    
    ("plate-5", "Plate V: Metallurgy Loops", "Metallurgical Smelting & Fuel Loops", "Smelting", "Smelter", "docs/assets/icons/Smelter.png", codex_plates.make_plate_5(), [
        ("01", "Kiln Wood Feeding", "Loads up to 25 units of raw timber into Charcoal Kiln firebox."),
        ("02", "Coal Extraction", "Extracts combusted coal from chute and stocks smelting fuel bins."),
        ("03", "Furnace Fueling", "Injects coal into blast furnace or smelter fuel hopper (2:1 ratio)."),
        ("04", "Raw Ore Feeding", "Loads copper, tin, iron, or silver ore batches into top hopper."),
        ("05", "Thermal Refining", "Monitors smelting cycle until molten slag separates into pure metal."),
        ("06", "Ingot Crate Sorting", "Transfers finished ingots into forge-adjacent labeled chests.")
    ], "Smeltery Blast Complex & Dual Hopper Ingot Elevation", "Displays bilateral kiln/smelter layout, hopper access catwalks, and forge crate staging buffer."),
    
    ("plate-6", "Plate VI: Culinary Batching", "Culinary Preparation & Cauldron Batching", "Culinary", "Cook", "docs/assets/icons/Cauldron.png", codex_plates.make_plate_6(), [
        ("01", "Larder Foraging", "Withdraws raw meats, mushrooms, berries, honey, and flour from cold pantry."),
        ("02", "Cauldron Stew Boiling", "Batches Deer Stew, Serpent Stew, and Wolf Skewer in bronze cauldron."),
        ("03", "Hearth Spit Roasting", "Skewers raw deer and boar meat onto rotisserie spits over open hearth."),
        ("04", "Fermenter Priming", "Loads honey-mead base into oak fermenting barrels for 2-day fermentation."),
        ("05", "Potion & Mead Tapping", "Extracts finished Healing, Poison Resistance, and Frost Resistance meads."),
        ("06", "Feast Table Staging", "Stocks longhouse dining tables and worker provision chests.")
    ], "Longhouse Hearth, Cauldron Rig & Spit Rotisserie Elevation", "Shows the central stone hearth, hanging cauldron chains, rotisserie spit layout, and smoke draft flue."),
    
    ("plate-7", "Plate VII: Logistics & Freight", "Logistics, Cart Freight & Chest Sorting", "Logistics", "Hauler", "docs/assets/icons/Cart.png", codex_plates.make_plate_7(), [
        ("01", "Outpost Inventory Scan", "Scans drop chests across mining and logging outposts for overflow goods."),
        ("02", "Vagon Freight Loading", "Loads heavy timber and ore crates into two-wheeled transport cart."),
        ("03", "Arterial Road Transit", "Hauls cart along stone highways and smoothed dirt paths to main compound."),
        ("04", "Sign-Tag Identification", "Reads chest sign tags ([Wood], [Stone], [Ore], [Food]) via OCR parser."),
        ("05", "Systematic Deposition", "Transfers freight into matching chests, consolidating stacks up to max capacity."),
        ("06", "50x Overflow Stacking", "Erects wood, stone, and coal resource piles when all chests are full.")
    ], "Camp Arterial Logistics, Cart Depot & Chest Array Elevation", "Depicts arterial cart roads, depot turnarounds, labeled chest arrays, and perimeter 50x overflow stockpile zones."),
    
    ("plate-8", "Plate VIII: Sentry Defense", "Sentry Defense, Ballistics & Perimeter Guard", "Combat", "Guard", "docs/assets/icons/Finewood_Bow.png", codex_plates.make_plate_8(), [
        ("01", "Rampart Patrol", "Maintains vigil atop palisades, watchtowers, and gatehouse bastions."),
        ("02", "Threat Vector Triage", "Detects hostiles at 45m; prioritizes Shamans, Brutes, and Trolls."),
        ("03", "Ballistic Bow Volleys", "Calculates target movement and parabolic gravity drop to land lead arrows."),
        ("04", "Tactical CQC Swap", "Draws iron sword and banded shield when enemies breach within 9.5 meters."),
        ("05", "Parry & Stagger Lock", "Executes timed parries, staggering attackers for 200% critical counterstrikes."),
        ("06", "Fletcher Munitions Restock", "Visits camp fletcher chests to replenish wood, fire, and needle arrows.")
    ], "Perimeter Watchtower, Palisade Rampart & Parabolic Firing Arc", "Shows watchtower elevation, 45m ballistic engagement cone, defensive ditch moat, and stake barrier placement."),
    
    ("plate-9", "Plate IX: Spatial Architecture", "Spatial Architecture & Blueprint Elevations", "Social", "All Workers", "docs/assets/icons/Round_Log.png", codex_plates.make_page_21(), [
        ("01", "Terraced Foundation", "Adapts pathfinding to tiered stone foundations and steep topography."),
        ("02", "Stair & Ramp Navigation", "Smoothly ascends 26-degree and 45-degree wooden staircases without snagging."),
        ("03", "Smoke Flue Clearance", "Avoids stagnant smoke pockets beneath gabled roofs to prevent asphyxiation."),
        ("04", "Comfort Radius Seeking", "Gathers near hearths, dragon beds, and braziers to gain rested bonus."),
        ("05", "Lintel & Door Clearance", "Traverses 2-meter doorframes and lintels with zero collision jitter."),
        ("06", "Mezzanine Operations", "Operates seamlessly across ground floors, loft lofts, and ramparts.")
    ], "Three-Story Viking Longhouse Sectional NavMesh Elevation", "Cross-section showing foundation pilings, stair ascent geometry, loft sleeping quarters, and roof smoke ridge ventilation.")
]

# Build JSON metadata and extract SVGs
schemas_data = []
schemas_svg_map = {}

for pid, short_title, full_title, domain, role, icon, raw_html, steps, bp_title, bp_desc in raw_plates:
    svg_str = extract_svg(raw_html)
    schemas_data.append({
        "id": pid,
        "shortTitle": short_title,
        "fullTitle": full_title,
        "domain": domain,
        "role": role,
        "icon": icon,
        "steps": [{"step": s[0], "title": s[1], "desc": s[2]} for s in steps],
        "bpTitle": bp_title,
        "bpDesc": bp_desc
    })
    schemas_svg_map[pid] = svg_str

# 3. CSS styles for Tab Switcher, Schemas Grid, and Modal
TAB_AND_SCHEMA_STYLES = """
  /* ==========================================================================
     MAIN TABS NAVIGATION (Stronghold Hybrid Stone Style)
     ========================================================================== */
  .tabs-nav-container {
    max-width: 1440px;
    margin: 0 auto 24px auto;
    display: flex;
    justify-content: center;
  }
  .nav-tabs {
    display: inline-flex;
    background: rgba(16, 19, 24, 0.95);
    border: 2px solid #5a4522;
    border-radius: 8px;
    padding: 6px;
    gap: 8px;
    box-shadow: inset 0 2px 6px rgba(0, 0, 0, 0.8), 0 4px 15px rgba(0, 0, 0, 0.5);
  }
  .nav-tab {
    display: flex;
    align-items: center;
    gap: 12px;
    background: transparent;
    border: 1px solid transparent;
    border-radius: 6px;
    padding: 10px 24px;
    font-family: 'Norsebold', serif;
    font-size: 1.15rem;
    color: #a89885;
    cursor: pointer;
    transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
    letter-spacing: 0.04em;
    user-select: none;
  }
  .nav-tab:hover {
    color: #f7d57f;
    background: rgba(40, 32, 20, 0.6);
    border-color: #8c6a32;
  }
  .nav-tab.active {
    background: linear-gradient(180deg, #2a2216 0%, #171a21 100%);
    border: 1px solid #FFBF1F;
    color: #FFBF1F;
    box-shadow: 0 0 16px rgba(255, 191, 31, 0.25), inset 0 1px 0 rgba(255, 230, 160, 0.3);
    filter: drop-shadow(0 2px 4px rgba(0,0,0,0.8));
  }
  .tab-glyph {
    font-size: 1.35rem;
    line-height: 1;
  }
  .tab-badge {
    background: rgba(0, 0, 0, 0.6);
    border: 1px solid #73592d;
    color: #7ad9f5;
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.78rem;
    font-weight: bold;
    padding: 2px 8px;
    border-radius: 12px;
    letter-spacing: 0.02em;
  }
  .nav-tab.active .tab-badge {
    border-color: #FFBF1F;
    color: #fff;
    background: rgba(255, 191, 31, 0.2);
  }

  /* Subtitle for Schemas View */
  .section-subtitle {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 1.05rem;
    color: #b5a490;
    margin: -8px 0 20px 0;
    line-height: 1.5;
    text-shadow: 1px 1px 2px #000;
  }

  /* Schemas Filter Toolbar */
  .schemas-toolbar {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    background: rgba(18, 22, 28, 0.85);
    border: 1px solid #4a381c;
    border-radius: 6px;
    padding: 12px 18px;
    margin-bottom: 24px;
    box-shadow: inset 0 1px 4px rgba(0, 0, 0, 0.6);
  }
  .schema-filters {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
  }
  .schema-filter-btn {
    background: #14181f;
    border: 1px solid #4d3a1f;
    color: #c4b5a0;
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-weight: bold;
    font-size: 0.85rem;
    padding: 6px 14px;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  .schema-filter-btn:hover {
    background: #252018;
    color: #f7d57f;
    border-color: #8c6a32;
  }
  .schema-filter-btn.active {
    background: #423015;
    color: #FFBF1F;
    border-color: #FFBF1F;
    box-shadow: 0 0 8px rgba(255, 191, 31, 0.3);
  }
  .schema-search-box {
    background: #0f1217;
    border: 1px solid #554020;
    border-radius: 4px;
    padding: 8px 14px;
    color: #fff;
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.92rem;
    width: 320px;
    outline: none;
    transition: border-color 0.2s ease;
  }
  .schema-search-box:focus {
    border-color: #FFBF1F;
    box-shadow: 0 0 10px rgba(255, 191, 31, 0.25);
  }

  /* Schemas 9-Card Grid */
  .schemas-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(440px, 1fr));
    gap: 24px;
    margin-bottom: 40px;
  }
  .schema-card {
    background: #15181f;
    border: 1.5px solid #4a3a22;
    border-radius: 6px;
    padding: 16px;
    display: flex;
    flex-direction: column;
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.6);
    transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
    position: relative;
    overflow: hidden;
  }
  .schema-card:hover {
    transform: translateY(-3px);
    border-color: #FFBF1F;
    box-shadow: 0 10px 24px rgba(0, 0, 0, 0.8), 0 0 14px rgba(255, 191, 31, 0.2);
  }
  .schema-card-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 12px;
  }
  .schema-card-icon {
    width: 44px;
    height: 44px;
    background: #0d1014;
    border: 1px solid #634b27;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 4px;
    flex-shrink: 0;
  }
  .schema-card-icon img {
    width: 100%;
    height: 100%;
    object-fit: contain;
  }
  .schema-card-meta {
    flex: 1;
    min-width: 0;
  }
  .schema-card-tags {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 4px;
  }
  .schema-domain-pill {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.72rem;
    font-weight: bold;
    text-transform: uppercase;
    background: rgba(56, 189, 248, 0.15);
    color: #38bdf8;
    border: 1px solid rgba(56, 189, 248, 0.4);
    padding: 2px 8px;
    border-radius: 10px;
    letter-spacing: 0.04em;
  }
  .schema-role-pill {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.72rem;
    font-weight: bold;
    color: #e0d0b8;
    background: rgba(255, 191, 31, 0.12);
    border: 1px solid rgba(255, 191, 31, 0.35);
    padding: 2px 8px;
    border-radius: 10px;
  }
  .schema-card-title {
    font-family: 'Norsebold', serif;
    color: #d64a2b;
    font-size: 1.25rem;
    letter-spacing: 0.02em;
    filter: url(#v-groove-rust);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  /* Interactive Preview Stage inside Card */
  .schema-preview-frame {
    height: 420px;
    background: #0f1217;
    border: 1px solid #332716;
    border-radius: 4px;
    position: relative;
    overflow: hidden;
    cursor: pointer;
    box-shadow: inset 0 2px 8px rgba(0,0,0,0.8);
    display: flex;
    align-items: center;
    justify-content: center;
  }
  .schema-preview-frame svg {
    width: 100%;
    height: 100%;
    object-fit: contain;
    transition: transform 0.3s ease;
    pointer-events: none;
  }
  .schema-preview-frame:hover svg {
    transform: scale(1.02);
  }
  .schema-preview-overlay {
    position: absolute;
    inset: 0;
    background: rgba(10, 13, 18, 0.4);
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: opacity 0.2s ease;
  }
  .schema-preview-frame:hover .schema-preview-overlay {
    opacity: 1;
  }
  .inspect-prompt {
    background: rgba(22, 18, 12, 0.92);
    border: 1.5px solid #FFBF1F;
    color: #FFBF1F;
    font-family: 'Norsebold', serif;
    font-size: 1.1rem;
    letter-spacing: 0.05em;
    padding: 8px 18px;
    border-radius: 4px;
    box-shadow: 0 4px 15px rgba(0,0,0,0.9), 0 0 12px rgba(255, 191, 31, 0.4);
  }

  .schema-card-summary {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.88rem;
    color: #c9bbae;
    margin: 12px 0 14px 0;
    line-height: 1.45;
    flex: 1;
  }
  .schema-card-actions {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    padding-top: 12px;
    border-top: 1px solid #2d2417;
  }
  .schema-steps-count {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.8rem;
    font-weight: bold;
    color: #7ad9f5;
    display: flex;
    align-items: center;
    gap: 6px;
  }
  .btn-inspect-schema {
    background: #2b2012;
    border: 1px solid #b8860b;
    color: #f7d57f;
    font-family: 'Norsebold', serif;
    font-size: 1rem;
    letter-spacing: 0.04em;
    padding: 6px 16px;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }
  .btn-inspect-schema:hover {
    background: #4a371c;
    border-color: #FFBF1F;
    color: #fff;
    box-shadow: 0 0 10px rgba(255, 191, 31, 0.35);
  }

  /* ==========================================================================
     FULLSCREEN HD VECTOR MODAL INSPECTOR
     ========================================================================== */
  .modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(6, 8, 12, 0.88);
    backdrop-filter: blur(10px);
    z-index: 99999;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 20px;
    animation: fadeIn 0.2s ease-out;
  }
  @keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
  }
  .modal-window {
    background: #11141a;
    border: 2px solid #b8860b;
    border-radius: 8px;
    box-shadow: 0 0 50px rgba(0, 0, 0, 0.95), 0 0 25px rgba(218, 165, 32, 0.25);
    width: 95vw;
    max-width: 1480px;
    height: 94vh;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }
  .modal-header {
    background: #171b24;
    border-bottom: 2px solid #4a381d;
    padding: 14px 22px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-shrink: 0;
  }
  .modal-title-group {
    display: flex;
    align-items: center;
    gap: 14px;
  }
  .modal-icon-frame {
    width: 48px;
    height: 48px;
    background: #0d1014;
    border: 1px solid #7a5e2f;
    border-radius: 4px;
    padding: 4px;
  }
  .modal-icon-frame img {
    width: 100%;
    height: 100%;
    object-fit: contain;
  }
  .modal-titles {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }
  .modal-title {
    font-family: 'Norsebold', serif;
    font-size: 1.65rem;
    color: #d64a2b;
    letter-spacing: 0.02em;
    filter: url(#v-groove-rust);
    line-height: 1.1;
  }
  .modal-subtitle {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.95rem;
    color: #ffa085;
    letter-spacing: 0.02em;
  }
  .modal-controls {
    display: flex;
    align-items: center;
    gap: 12px;
  }
  .modal-nav-btn {
    background: #241d13;
    border: 1px solid #6e5226;
    color: #e0d0b8;
    font-family: 'Norsebold', serif;
    font-size: 0.95rem;
    padding: 6px 14px;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  .modal-nav-btn:hover {
    background: #3d2f1c;
    border-color: #FFBF1F;
    color: #FFBF1F;
  }
  .btn-close-modal {
    background: #2a1515;
    border: 1px solid #8a2e2e;
    color: #fca5a5;
    font-size: 1.6rem;
    width: 38px;
    height: 38px;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    line-height: 1;
    transition: all 0.2s ease;
  }
  .btn-close-modal:hover {
    background: #6b1d1d;
    border-color: #ef4444;
    color: #fff;
  }

  .modal-body {
    flex: 1;
    display: flex;
    overflow: hidden;
    background: #0c0f14;
  }
  .modal-svg-stage {
    flex: 1;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16px;
    overflow: auto;
    background: radial-gradient(circle at center, #131720 0%, #090b0e 100%);
  }
  .modal-svg-stage svg {
    max-width: 100%;
    max-height: 100%;
    height: auto;
    width: auto;
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.9);
    border: 1px solid #332615;
    border-radius: 4px;
  }
  .modal-sidebar {
    width: 380px;
    background: #13161e;
    border-left: 2px solid #3d2f1b;
    padding: 20px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 20px;
    flex-shrink: 0;
  }
  .modal-section-title {
    font-family: 'Norsebold', serif;
    font-size: 1.15rem;
    color: #d64a2b;
    letter-spacing: 0.02em;
    filter: url(#v-groove-rust);
    border-bottom: 1px solid #47361d;
    padding-bottom: 6px;
    margin-bottom: 12px;
    display: flex;
    align-items: center;
    gap: 8px;
  }
  .modal-steps-list {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }
  .modal-step-item {
    background: #181d26;
    border: 1px solid #2d3542;
    border-radius: 4px;
    padding: 10px;
  }
  .modal-step-header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 4px;
  }
  .modal-step-num {
    background: #FFBF1F;
    color: #101318;
    font-family: 'Norsebold', serif;
    font-size: 0.8rem;
    font-weight: bold;
    width: 22px;
    height: 22px;
    border-radius: 3px;
    display: flex;
    align-items: center;
    justify-content: center;
  }
  .modal-step-title {
    font-family: 'Norsebold', serif;
    font-size: 0.98rem;
    color: #fff;
    letter-spacing: 0.03em;
  }
  .modal-step-desc {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.84rem;
    color: #b5a490;
    line-height: 1.4;
  }
  .modal-bp-box {
    background: #161c24;
    border: 1px solid #253347;
    border-radius: 4px;
    padding: 12px;
  }
  .modal-bp-name {
    font-family: 'Norsebold', serif;
    font-size: 1.02rem;
    color: #7ad9f5;
    margin-bottom: 6px;
    letter-spacing: 0.03em;
  }
  .modal-bp-desc {
    font-family: 'Averia Serif Libre', Georgia, serif;
    font-size: 0.85rem;
    color: #cad8e6;
    line-height: 1.42;
  }

  @media (max-width: 1080px) {
    .modal-body {
      flex-direction: column;
    }
    .modal-sidebar {
      width: 100%;
      height: 260px;
      border-left: none;
      border-top: 2px solid #3d2f1b;
    }
  }
"""

# Insert CSS into <style>
if "/* MAIN TABS NAVIGATION" not in html:
    html = html.replace("</style>", TAB_AND_SCHEMA_STYLES + "\n</style>")

# 4. Top Navigation Bar HTML
NAV_HTML = """
<div class="tabs-nav-container">
  <div class="nav-tabs">
    <button class="nav-tab active" data-tab="tab-matrix-catalog" id="btn-tab-matrix">
      <span class="tab-glyph">⚔️</span>
      <span class="tab-title">Worker Matrix &amp; Feature Catalog</span>
      <span class="tab-badge">36 Features</span>
    </button>
    <button class="nav-tab" data-tab="tab-process-schemas" id="btn-tab-schemas">
      <span class="tab-glyph">📐</span>
      <span class="tab-title">Visual Process Schemas &amp; Infographics</span>
      <span class="tab-badge">9 Vector Plates</span>
    </button>
  </div>
</div>
"""

# 5. Schemas View Content HTML
SCHEMAS_VIEW_HTML = """
<div id="tab-process-schemas" class="tab-pane" style="display: none;">
  <h2 class="section-title"><span>📐</span> Illustrated Process Schemas &amp; Blueprint Elevations</h2>
  <p class="section-subtitle">
    Pure vector schematics detailing the multi-step algorithmic state machines, tool tiers, spatial elevation blueprints, and logistic circuits driving ValheimWorkers v0.3.0.
  </p>

  <div class="schemas-toolbar">
    <div class="schema-filters" id="schema-domain-filters">
      <button class="schema-filter-btn active" data-sfilter="all">All Schemas (9)</button>
      <button class="schema-filter-btn" data-sfilter="Logistics">Logistics (2)</button>
      <button class="schema-filter-btn" data-sfilter="Forestry">Forestry (1)</button>
      <button class="schema-filter-btn" data-sfilter="Mining">Mining (1)</button>
      <button class="schema-filter-btn" data-sfilter="Agriculture">Agriculture (1)</button>
      <button class="schema-filter-btn" data-sfilter="Smelting">Smelting (1)</button>
      <button class="schema-filter-btn" data-sfilter="Culinary">Culinary (1)</button>
      <button class="schema-filter-btn" data-sfilter="Combat">Combat (1)</button>
      <button class="schema-filter-btn" data-sfilter="Social">Spatial / Social (1)</button>
    </div>
    <input type="text" id="schema-search" class="schema-search-box" placeholder="Filter schemas by step, tool, or keyword...">
  </div>

  <div class="schemas-grid" id="schemas-grid">
    <!-- Rendered dynamically by JavaScript -->
  </div>
</div>

<!-- FULLSCREEN MODAL INSPECTOR -->
<div id="schema-modal" class="modal-overlay" style="display: none;">
  <div class="modal-window">
    <div class="modal-header">
      <div class="modal-title-group">
        <div class="modal-icon-frame" id="modal-icon-frame">
          <img id="modal-icon" src="" alt="Schema Icon">
        </div>
        <div class="modal-titles">
          <div class="modal-title" id="modal-title">Plate Title</div>
          <div class="modal-subtitle" id="modal-subtitle">Plate Subtitle</div>
        </div>
      </div>
      <div class="modal-controls">
        <button class="modal-nav-btn" id="modal-prev-btn">&larr; Previous Plate</button>
        <button class="modal-nav-btn" id="modal-next-btn">Next Plate &rarr;</button>
        <button class="btn-close-modal" id="modal-close-btn" title="Close Inspector">&times;</button>
      </div>
    </div>
    <div class="modal-body">
      <div class="modal-svg-stage" id="modal-svg-stage">
        <!-- SVG injected here -->
      </div>
      <div class="modal-sidebar">
        <div>
          <div class="modal-section-title">
            <span>⚙️</span> Algorithmic State Sequence
          </div>
          <div class="modal-steps-list" id="modal-steps-list">
            <!-- Steps injected here -->
          </div>
        </div>
        <div>
          <div class="modal-section-title">
            <span>📐</span> Blueprint &amp; Spatial Elevation
          </div>
          <div class="modal-bp-box">
            <div class="modal-bp-name" id="modal-bp-name">Blueprint Name</div>
            <div class="modal-bp-desc" id="modal-bp-desc">Blueprint description text.</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</div>
"""

# Wrap existing toolbar and matrix into tab-matrix-catalog
if "<div id=\"tab-matrix-catalog\"" not in html:
    # Find </header>
    header_end = html.find("</header>")
    if header_end != -1:
        after_header = header_end + len("</header>")
        # Insert Nav
        prefix = html[:after_header] + "\n" + NAV_HTML + '\n<div id="tab-matrix-catalog" class="tab-pane">\n'
        
        # Find <footer>
        footer_start = html.find("<footer>")
        if footer_start != -1:
            matrix_catalog_content = html[after_header:footer_start]
            suffix = '\n</div>\n' + SCHEMAS_VIEW_HTML + '\n' + html[footer_start:]
            html = prefix + matrix_catalog_content + suffix

# 6. JavaScript logic for Schemas and Tabs
SCHEMAS_DATA_JSON = json.dumps(schemas_data, indent=2)
SCHEMAS_SVG_JSON = json.dumps(schemas_svg_map)

SCHEMAS_JS_DATA = f"""
const SCHEMAS_DATA = {SCHEMAS_DATA_JSON};
const SCHEMAS_SVG = {SCHEMAS_SVG_JSON};
""" + """
let currentSchemaIndex = 0;

function setupMainTabs() {
  const tabBtns = document.querySelectorAll('.nav-tab');
  tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      const targetId = btn.getAttribute('data-tab');
      tabBtns.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');

      document.querySelectorAll('.tab-pane').forEach(p => p.style.display = 'none');
      const targetPane = document.getElementById(targetId);
      if (targetPane) targetPane.style.display = 'block';

      if (targetId === 'tab-process-schemas') {
        history.replaceState(null, '', '#schemas');
      } else {
        history.replaceState(null, '', '#matrix');
      }
    });
  });

  function handleHash() {
    const urlParams = new URLSearchParams(window.location.search);
    const tabParam = (urlParams.get('tab') || '').toLowerCase();
    const hash = window.location.hash.toLowerCase();
    if (tabParam === 'schemas' || hash === '#schemas' || hash === '#infographics' || hash === '#plates') {
      const btn = document.getElementById('btn-tab-schemas');
      if (btn) btn.click();
    } else if (tabParam === 'matrix' || hash === '#matrix' || hash === '#catalog') {
      const btn = document.getElementById('btn-tab-matrix');
      if (btn) btn.click();
    }
  }
  window.addEventListener('hashchange', handleHash);
  handleHash();
}

function renderSchemasGrid(schemas) {
  const grid = document.getElementById('schemas-grid');
  if (!grid) return;
  grid.innerHTML = '';

  schemas.forEach((s, idx) => {
    const card = document.createElement('div');
    card.className = 'schema-card';
    card.setAttribute('data-domain', s.domain);

    const stepsPill = s.steps.length + ' Process Steps';

    card.innerHTML = `
      <div class="schema-card-header">
        <div class="schema-card-icon">
          <img src="${s.icon}" alt="${s.shortTitle}">
        </div>
        <div class="schema-card-meta">
          <div class="schema-card-tags">
            <span class="schema-domain-pill">${s.domain}</span>
            <span class="schema-role-pill">${s.role}</span>
          </div>
          <div class="schema-card-title">${s.shortTitle}</div>
        </div>
      </div>

      <div class="schema-preview-frame" data-index="${idx}">
        ${SCHEMAS_SVG[s.id] || ''}
        <div class="schema-preview-overlay">
          <span class="inspect-prompt">🔍 Inspect HD Vector Schema</span>
        </div>
      </div>

      <p class="schema-card-summary">${s.bpDesc}</p>

      <div class="schema-card-actions">
        <div class="schema-steps-count">
          <span>⚡</span> ${stepsPill} &bull; S-Curve Flow
        </div>
        <button class="btn-inspect-schema" data-index="${idx}">
          <span>🔎</span> Inspect HD
        </button>
      </div>
    `;

    // Click on preview frame or button opens modal
    const frame = card.querySelector('.schema-preview-frame');
    const btn = card.querySelector('.btn-inspect-schema');
    frame.addEventListener('click', () => openSchemaModal(idx));
    btn.addEventListener('click', () => openSchemaModal(idx));

    grid.appendChild(card);
  });
}

function openSchemaModal(index) {
  const s = SCHEMAS_DATA[index];
  if (!s) return;
  currentSchemaIndex = index;

  document.getElementById('modal-icon').src = s.icon;
  document.getElementById('modal-title').textContent = s.fullTitle;
  document.getElementById('modal-subtitle').textContent = s.domain + ' • Role: ' + s.role;
  
  // Inject SVG
  const stage = document.getElementById('modal-svg-stage');
  stage.innerHTML = SCHEMAS_SVG[s.id] || '';

  // Inject steps
  const stepsList = document.getElementById('modal-steps-list');
  stepsList.innerHTML = s.steps.map(st => `
    <div class="modal-step-item">
      <div class="modal-step-header">
        <div class="modal-step-num">${st.step}</div>
        <div class="modal-step-title">${st.title}</div>
      </div>
      <div class="modal-step-desc">${st.desc}</div>
    </div>
  `).join('');

  // Blueprint info
  document.getElementById('modal-bp-name').textContent = s.bpTitle;
  document.getElementById('modal-bp-desc').textContent = s.bpDesc;

  document.getElementById('schema-modal').style.display = 'flex';
  document.body.style.overflow = 'hidden';
}

function closeSchemaModal() {
  const modal = document.getElementById('schema-modal');
  if (modal) modal.style.display = 'none';
  document.body.style.overflow = '';
}

function setupSchemaEvents() {
  // Modal close handlers
  const closeBtn = document.getElementById('modal-close-btn');
  const modal = document.getElementById('schema-modal');
  if (closeBtn) closeBtn.addEventListener('click', closeSchemaModal);
  if (modal) {
    modal.addEventListener('click', (e) => {
      if (e.target === modal) closeSchemaModal();
    });
  }
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') closeSchemaModal();
  });

  // Modal Next / Prev
  const prevBtn = document.getElementById('modal-prev-btn');
  const nextBtn = document.getElementById('modal-next-btn');
  if (prevBtn) {
    prevBtn.addEventListener('click', () => {
      currentSchemaIndex = (currentSchemaIndex - 1 + SCHEMAS_DATA.length) % SCHEMAS_DATA.length;
      openSchemaModal(currentSchemaIndex);
    });
  }
  if (nextBtn) {
    nextBtn.addEventListener('click', () => {
      currentSchemaIndex = (currentSchemaIndex + 1) % SCHEMAS_DATA.length;
      openSchemaModal(currentSchemaIndex);
    });
  }

  // Domain Filter Buttons
  const filterBtns = document.querySelectorAll('.schema-filter-btn');
  filterBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      filterBtns.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      applySchemaFilters();
    });
  });

  // Search Input
  const searchInput = document.getElementById('schema-search');
  if (searchInput) {
    searchInput.addEventListener('input', applySchemaFilters);
  }
}

function applySchemaFilters() {
  const activeBtn = document.querySelector('.schema-filter-btn.active');
  const selectedDomain = activeBtn ? activeBtn.getAttribute('data-sfilter') : 'all';
  const query = (document.getElementById('schema-search').value || '').toLowerCase().trim();

  const filtered = SCHEMAS_DATA.filter(s => {
    const matchesDomain = (selectedDomain === 'all') || (s.domain.toLowerCase() === selectedDomain.toLowerCase());
    const matchesQuery = !query ||
      s.shortTitle.toLowerCase().includes(query) ||
      s.fullTitle.toLowerCase().includes(query) ||
      s.domain.toLowerCase().includes(query) ||
      s.role.toLowerCase().includes(query) ||
      s.bpTitle.toLowerCase().includes(query) ||
      s.bpDesc.toLowerCase().includes(query) ||
      s.steps.some(st => st.title.toLowerCase().includes(query) || st.desc.toLowerCase().includes(query));
    return matchesDomain && matchesQuery;
  });

  renderSchemasGrid(filtered);
}

"""

# Append JS functions to <script>
if "function setupMainTabs" not in html:
    # Insert right before document.addEventListener("DOMContentLoaded", init);
    target = 'document.addEventListener("DOMContentLoaded", init);'
    replacement = SCHEMAS_JS_DATA + """
function initSchemasSuite() {
  setupMainTabs();
  renderSchemasGrid(SCHEMAS_DATA);
  setupSchemaEvents();
}

const originalInit = init;
init = function() {
  if (typeof originalInit === 'function') originalInit();
  initSchemasSuite();
};

document.addEventListener("DOMContentLoaded", init);
"""
    if target in html:
        html = html.replace(target, replacement)

# Save to main file
print(f"Writing updated {MAP_PATH}...")
with open(MAP_PATH, "w", encoding="utf-8") as f:
    f.write(html)

print(f"Writing updated {SUBDIR_MAP}...")
with open(SUBDIR_MAP, "w", encoding="utf-8") as f:
    f.write(html)

print("SUCCESS: Both worker_feature_map.html files updated with complete 9-Plate Visual Process Schemas suite and HD Modal Inspector!")
