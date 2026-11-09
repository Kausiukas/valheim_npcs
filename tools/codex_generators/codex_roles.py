# Codex Worker Roles, Cover, TOC & Troubleshooting Generators
from codex_styles import ic, slot, stone_monolith_b64, stone_tablet_b64, stone_crest_b64

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
            <h3 class="green">CODEX CHAPTERS &amp; SCHEMAS</h3>
            <ul>
              <li>
                <strong>PLATE I: MASTER PRODUCTION WEB (P. 3)</strong>
                Multi-Domain Settlement Macro-Economy &bull; Raw to Refined Flows.
              </li>
              <li>
                <strong>PART I: LUMBERJACK (P. 4) &bull; SCHEMA 1 (P. 5)</strong>
                Felling Arcs &bull; Log Bucking &bull; 3m Nursery Grid &bull; 50x Piles.
              </li>
              <li>
                <strong>PART II: MINER (P. 6) &bull; SCHEMA 2 (P. 7)</strong>
                Subsurface Quarrying &bull; -8m Bedrock &bull; Hoe Planing Ramps.
              </li>
              <li>
                <strong>PART III: FARMER (P. 8) &bull; SCHEMA 3 (P. 9)</strong>
                1.2m Crop Grid &bull; 50/50 Seed Cycle &bull; Apiary Honey Harvest.
              </li>
              <li>
                <strong>PART IV: RANCHER (P. 10) &bull; HERD TENDING</strong>
                Enclosure Feeding &bull; Selective Herd Culling &bull; Byproduct Sourcing.
              </li>
              <li>
                <strong>PART V: SMELTER (P. 11) &bull; SCHEMA 4 (P. 12)</strong>
                High-Heat Pyrolysis &bull; 1:2 Ore/Coal &bull; Ingot Vault Deposition.
              </li>
            </ul>
          </div>

          <div class="tablet">
            <h3 class="orange">EXPEDITIONS &amp; CITADEL BLUEPRINTS</h3>
            <ul>
              <li>
                <strong>PART VI: COOK (P. 13) &bull; SCHEMA 5 (P. 14)</strong>
                Spit Timers &bull; Cauldron Stews &bull; Fermenter Mead Cellar.
              </li>
              <li>
                <strong>PART VII: CRAFTSMAN (P. 15) &bull; MASON &amp; ARMORER</strong>
                Fortress Repair Patrol &bull; Zero-Cost Tool Restorations.
              </li>
              <li>
                <strong>PART VIII: FISHER (P. 16) &bull; COASTAL ANGLER</strong>
                Shoreline Casting &bull; Tidal Drift &bull; Pier Fillet Station.
              </li>
              <li>
                <strong>PART IX: HAULER (P. 17) &bull; SCHEMA 6 (P. 18)</strong>
                Ground Vacuum &bull; 1000kg Cart Freight &bull; Sign Bracket Routing.
              </li>
              <li>
                <strong>PART X: SENTRY (P. 19) &bull; SCHEMA 7 (P. 20)</strong>
                Watchtower Defense Geometry &bull; Intersecting Sightlines.
              </li>
              <li>
                <strong>BLUEPRINT ELEVATIONS (P. 21) &bull; DIAGNOSTICS (P. 22)</strong>
                Spatial Architecture &bull; 26&deg; Ramps &bull; valheimworkers.cfg.
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
      <span>PAGE 22</span>
    </div>
  </div>
"""

def make_all_worker_role_pages():
    roles = {}
    
    # Page 4: Part I: The Lumberjack
    roles[4] = make_worker_page(
        page_num=4,
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
    )

    # Page 6: Part II: The Miner
    roles[6] = make_worker_page(
        page_num=6,
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
            ("MASON STONE SUPPLY: ", "Feeds infinite stone blocks to Craftsmen building stronghold ramparts."),
            ("HAULER ORE RUNNERS: ", "Transfers heavy ore loads from subterranean pits up to the surface.")
        ]
    )

    # Page 8: Part III: The Farmer
    roles[8] = make_worker_page(
        page_num=8,
        part_roman="PART III",
        role_title="THE FARMER",
        role_sub="CULTIVATOR &amp; APIARIST",
        role_pill="AGRICULTURE &bull; CROPS",
        icon_main="Cultivator.png",
        loadout_slots=[("Cultivator.png", "Cultivator", True), ("Carrot_Seeds.png", "Seeds", False), ("Torch.png", "Torch", False), ("Honey.png", "Honey", False)],
        yield_slots=[("Carrot.png", "Carrot", False), ("Turnip.png", "Turnip", False), ("Onion.png", "Onion", False), ("Barley.png", "Barley", False), ("Flax.png", "Flax", False), ("Honey.png", "Honey", False)],
        cycle_items=[
            ("1. SOIL CULTIVATION: ", "Tills wild grass into rich fertile loam using the Cultivator tool."),
            ("2. 1.2M GRID PLANTING: ", "Plants seeds in disciplined geometric matrices to eliminate cramped crop wither."),
            ("3. FULL-CYCLE HARVESTING: ", "Harvests fully mature crops and seed-flowers at the perfect botanical peak."),
            ("4. APIARY HONEY TENDING: ", "Empties camp beehives into honeycomb stores; monitors bee happiness.")
        ],
        advice_items=[
            ("GRID SPACING (1.2M): ", "Crops planted too close wither and die. Workers enforce minimum 1.2m spacing."),
            ("SEED RE-INVESTMENT: ", "Farmers reserve 50% of harvests for re-planting to keep farms permanently self-sustaining."),
            ("FENCE ENCLOSURE: ", "Surround agricultural plots with stakewalls to protect crops from Greydwarf trampling.")
        ],
        cap_items=[
            ("[F09] TILL SOIL CULTIVATION: ", "Converts raw terrain into fertile farmland across the full 25m boundary."),
            ("[F10] 1.2M SPACING SEEDING: ", "Plants carrots, turnips, onions, barley, and flax at calculated intervals."),
            ("[F11] MATURE VEGETABLE HARVEST: ", "Gathers ripe food without damaging adjacent growing saplings."),
            ("[F12] SEED-FLOWER ROTATION: ", "Re-seeds mature crops into flowers to guarantee infinite seed generation."),
            ("[F13] APIARY HONEY EXTRACTION: ", "Collects sweet honey combs from all beehives within the post perimeter.")
        ],
        synergy_items=[
            ("CAULDRON PANTRY CONDUIT: ", "Feeds continuous root vegetables and honey to the Cook for stamina stews."),
            ("RANCH TAME FEEDER: ", "Delivers root vegetables to boars and barley to lox enclosures automatically."),
            ("LOOM &amp; SPINNING WHEEL: ", "Supplies flax directly to Craftsmen for linen thread and padded armor.")
        ]
    )

    # Page 10: Part IV: The Rancher
    roles[10] = make_worker_page(
        page_num=10,
        part_roman="PART IV",
        role_title="THE RANCHER",
        role_sub="HERDSMAN &amp; ANIMAL TAMER",
        role_pill="LIVESTOCK &bull; BREEDING",
        icon_main="Raw_Meat.png",
        loadout_slots=[("Raw_Meat.png", "Meat", True), ("Leather_Scraps.png", "Feed", False), ("Torch.png", "Torch", False), ("Wolf_Fang.png", "Fang", False)],
        yield_slots=[("Raw_Meat.png", "Meat", False), ("Wolf_Meat.png", "Wolf Meat", False), ("Lox_Meat.png", "Lox Meat", False), ("Leather_Scraps.png", "Leather", False), ("Wolf_Pelt.png", "Pelt", False), ("Lox_Pelt.png", "Lox Pelt", False)],
        cycle_items=[
            ("1. ENCLOSURE PATROL: ", "Inspects pens for Boars, Wolves, and Lox; checks hunger and breeding status."),
            ("2. SELECTIVE TROUGH FEEDING: ", "Deposits carrots, berries, and meats to maintain active reproduction buffs."),
            ("3. SUSTAINABLE CULLING: ", "Spares breeding pairs while culling excess mature beasts for hides and meats."),
            ("4. BYPRODUCT HARVEST: ", "Gathers dropped leather scraps, pelts, wolf fangs, and trophy horns.")
        ],
        advice_items=[
            ("BREEDING MINIMUM: ", "Workers always preserve at least 2 breeding adults per enclosure to prevent herd extinction."),
            ("ENCLOSURE ROOFING: ", "Roofed pens protect domestic animals from Bat raids and drake frost breath."),
            ("FEED CONDUIT: ", "Place a chest tagged [Feed] with carrots and meat near pens for autonomous trough loading.")
        ],
        cap_items=[
            ("[F14] DOMESTIC HERD FEEDING: ", "Dispenses vegetables to boars, meat to wolves, and barley/cloudberries to lox."),
            ("[F15] BEAST TAMING ACCELERATION: ", "Soothes frightened wild animals to accelerate the domestic taming cycle."),
            ("[F16] SELECTIVE HERD CULLING: ", "Harvests surplus livestock when pen capacity exceeds sustainable limits."),
            ("[F17] LEATHER &amp; PELT SOURCING: ", "Collects hides, entrails, and fangs, routing them into tailoring chests.")
        ],
        synergy_items=[
            ("BUTCHER TO COOK PIPELINE: ", "Provides raw boar, wolf, and lox meats to the kitchen hearth."),
            ("CRAFTSMAN TANNERY: ", "Supplies constant pelts and leather scraps for armors, bows, and banners."),
            ("FARMER BYPRODUCT EXCHANGE: ", "Recycles excess crop yields into animal feed, closing the agricultural loop.")
        ]
    )

    # Page 11: Part V: The Smelter
    roles[11] = make_worker_page(
        page_num=11,
        part_roman="PART V",
        role_title="THE SMELTER",
        role_sub="FOUNDRYMAN &amp; PYROTECHNIC",
        role_pill="METALLURGY &bull; INGOTS",
        icon_main="Smelter.png",
        loadout_slots=[("Smelter.png", "Kiln", True), ("Blast_Furnace.png", "Blast", True), ("Torch.png", "Fire", False), ("Coal.png", "Fuel", False)],
        yield_slots=[("Copper.png", "Copper", False), ("Tin.png", "Tin", False), ("Bronze.png", "Bronze", False), ("Iron.png", "Iron", False), ("Silver.png", "Silver", False), ("Black_Metal.png", "Black Metal", False)],
        cycle_items=[
            ("1. CHARCOAL KILN PYROLYSIS: ", "Charges kilns with raw timber; produces black coal at high thermal efficiency."),
            ("2. SMELTER HOPPER CHARGING: ", "Feeds raw ores and coal into hoppers in strict 1:2 stoichiometric ratios."),
            ("3. BLAST FURNACE REFINING: ", "Processes Black Metal scrap and Flametal ore in high-tier blast furnaces."),
            ("4. INGOT CONSOLIDATION: ", "Extracts molten bars and deposits them into sorted foundry vaults.")
        ],
        advice_items=[
            ("STORAGE PROXIMITY: ", "Keep raw ore chests within 8m of smelters to minimize worker hopper loading travel."),
            ("COAL BUFFERING: ", "Run 2 Kilns per Smelter to guarantee furnaces never stall for lack of fuel."),
            ("CONTAINER TAGGING: ", "Tag output chests [Ingot] or [Metal] to enable automatic foundry consolidation.")
        ],
        cap_items=[
            ("[F18] CHARCOAL KILN OPERATION: ", "Continuously loads wood to produce charcoal fuel autonomously."),
            ("[F19] SMELTER FUEL CHARGING: ", "Monitors combustion chambers, keeping coal levels consistently topped off."),
            ("[F20] ORE REDUCTION &amp; SMELTING: ", "Feeds copper, tin, scrap iron, and silver ores into active smelters."),
            ("[F21] BLAST FURNACE METALLURGY: ", "Operates advanced blast furnaces for black metal and endgame alloys."),
            ("[F22] INGOT VAULT STACKING: ", "Collects cooled metal bars and routes them to secure vaults.")
        ],
        synergy_items=[
            ("MINER ORE INGESTION: ", "Consumes raw copper, tin, iron, and silver hauled directly from local quarries."),
            ("CRAFTSMAN ARMORY FORGE: ", "Feeds refined bronze, iron, and black metal bars directly into weapon forges."),
            ("LUMBERJACK FUEL TIE-IN: ", "Consumes surplus wood stacks for kiln combustion, freeing depot capacity.")
        ]
    )

    # Page 13: Part VI: The Cook
    roles[13] = make_worker_page(
        page_num=13,
        part_roman="PART VI",
        role_title="THE COOK",
        role_sub="CHEF &amp; FERMENTER",
        role_pill="PROVISIONS &bull; MEAD",
        icon_main="Cauldron.png",
        loadout_slots=[("Cauldron.png", "Cauldron", True), ("Fermenter.png", "Ferment", True), ("Torch.png", "Spit", False), ("Honey.png", "Base", False)],
        yield_slots=[("Cooked_Meat.png", "Meat", False), ("Deer_Stew.png", "Stew", False), ("Sausages.png", "Sausages", False), ("Bread.png", "Bread", False), ("Blood_Pudding.png", "Pudding", False), ("Mead_Health.png", "Health", False)],
        cycle_items=[
            ("1. HEARTH SPIT MONITORING: ", "Places meats on cooking stations; flips at 20s to prevent scorched charcoal."),
            ("2. CAULDRON STEW CRAFTING: ", "Combines vegetables, honey, and meats into high-tier stamina and HP feasts."),
            ("3. STONE OVEN BAKING: ", "Bakes bread dough, lox meat pies, and fish wraps in stone bread ovens."),
            ("4. MEAD CELLAR AGING: ", "Brews mead bases and ages them in oak fermenters for 2400 seconds.")
        ],
        advice_items=[
            ("SPIT BURN PREVENTION: ", "Worker AI monitors food timers down to the tick, pulling cooked meats immediately."),
            ("FERMENTER TIMERS: ", "Mead takes 2 Valheim days (2400s) to ferment. Keep 3 fermenters rotating continuously."),
            ("FOOD ROTATION: ", "Stock larder chests with a balance of 1 Health food, 1 Stamina food, and 1 Eitr food.")
        ],
        cap_items=[
            ("[F23] COOKING SPIT ROASTING: ", "Roasts boars, deer, and wolf meats with zero burn-loss failure rate."),
            ("[F24] CAULDRON RECIPE SIMMERING: ", "Prepares stews, soups, sausages, and serpent stews autonomously."),
            ("[F25] STONE OVEN BAKERY: ", "Bakes pies, bread loaves, and gourmet fish dishes in wood-fired ovens."),
            ("[F26] FERMENTER MEAD BARRELS: ", "Brews healing, stamina, frost resistance, and poison resistance meads."),
            ("[F27] LARDER CONSOLIDATION: ", "Sorts rations into cold storage chests tagged with [Food] and [Mead].")
        ],
        synergy_items=[
            ("SETTLEMENT BUFF KEEPER: ", "Supplies 3-course banquet rations to keep all workers operating at peak stamina."),
            ("FARMER VEGETABLE SINK: ", "Transforms raw carrots, turnips, onions, and barley into culinary dishes."),
            ("RANCHER MEAT CONSUMER: ", "Cuts raw meats into sausages, pies, and platters with zero waste.")
        ]
    )

    # Page 15: Part VII: The Craftsman
    roles[15] = make_worker_page(
        page_num=15,
        part_roman="PART VII",
        role_title="THE CRAFTSMAN",
        role_sub="MASTER BUILDER &amp; MASON",
        role_pill="CONSTRUCTION &bull; REPAIR",
        icon_main="Hammer.png",
        loadout_slots=[("Hammer.png", "Hammer", True), ("Stonecutter.png", "Mason", True), ("Forge.png", "Forge", False), ("Wood.png", "Mats", False)],
        yield_slots=[("Wood.png", "Wood", False), ("Stone.png", "Stone", False), ("Iron.png", "Nails", False), ("Tar.png", "Tar", False), ("Fine_wood.png", "Fine", False), ("Hammer.png", "Repair", False)],
        cycle_items=[
            ("1. PERIMETER DAMAGE SURVEY: ", "Patrols walls, palisades, and gates, identifying structures below 100% health."),
            ("2. ZERO-COST TIMBER REPAIRS: ", "Swaps to Hammer to repair wooden structures with zero material cost."),
            ("3. MASONRY FORTIFICATION: ", "Maintains stone bastions, trenches, and towers using the Stonecutter bench."),
            ("4. TOOL &amp; GEAR RESTORATION: ", "Repairs weapons and armor in camp chests whenever durability drops below 10%.")
        ],
        advice_items=[
            ("STATION COVERAGE: ", "Ensure Workbenches and Stonecutters cover your entire perimeter wall to allow 100% repairs."),
            ("WEATHER DECAY DEFENSE: ", "Craftsmen continually restore rain damage on un-roofed wooden beams and stairs."),
            ("CHEST REPAIR LOOP: ", "Leave damaged tools in chests tagged [Repair]; the Craftsman restores them automatically.")
        ],
        cap_items=[
            ("[F28] PERIMETER WALL MAINTENANCE: ", "Inspects and repairs damaged barricades, doors, and palisades."),
            ("[F29] ZERO-COST TIMBER RESTORATION: ", "Applies hammer repairs to wooden building components at no cost."),
            ("[F30] STONE &amp; MASONRY REPAIRS: ", "Restores stone bastions, arches, and high towers using the Stonecutter."),
            ("[F31] WORKSTATION RESTORATION: ", "Restores durability to all player and worker tools stored in repair boxes.")
        ],
        synergy_items=[
            ("UNIVERSAL TOOL INTEGRITY: ", "Restores picks, axes, bows, and hoes for all other 9 worker roles."),
            ("SENTRY TOWER FORTIFICATION: ", "Maintains guard platforms, parapets, and palisades during enemy raids."),
            ("LUMBERJACK &amp; MINER ALLIANCE: ", "Uses local timber and stone reserves to maintain impenetrable defense works.")
        ]
    )

    # Page 16: Part VIII: The Fisher
    roles[16] = make_worker_page(
        page_num=16,
        part_roman="PART VIII",
        role_title="THE FISHER",
        role_sub="COASTAL ANGLER &amp; HARVESTER",
        role_pill="MARITIME &bull; SEAFOOD",
        icon_main="Fishing_Rod.png",
        loadout_slots=[("Fishing_Rod.png", "Rod", True), ("Bait.png", "Bait", True), ("Torch.png", "Torch", False), ("Raw_Fish.png", "Catch", False)],
        yield_slots=[("Raw_Fish.png", "Fish", False), ("Cooked_Fish.png", "Cooked", False), ("Fish_Wraps.png", "Wraps", False), ("Bait.png", "Bait", False), ("Karve.png", "Dock", False), ("Serpent_Meat.png", "Serpent", False)],
        cycle_items=[
            ("1. SHORELINE RECONNAISSANCE: ", "Walks coastline edges to identify fish schools in tidal waters."),
            ("2. CASTING &amp; TENSION RETRIEVAL: ", "Casts baited lines; manages reel tension to land raw fish."),
            ("3. PIER FILLET STATION: ", "Cleans and fillets catches on dock tables, removing fish heads and entrails."),
            ("4. COLD STORAGE DEPOSITION: ", "Stores clean raw fish in barrels tagged [Fish] for the kitchen.")
        ],
        advice_items=[
            ("DOCK CONSTRUCTION: ", "Build a wooden fishing pier extending 6m into deep water for higher-tier fish species."),
            ("BAIT SOURCING: ", "Keep a chest tagged [Bait] supplied with regular and mossy fishing bait on the pier."),
            ("RAIN FISHING BONUS: ", "Fish spawn rates increase during stormy weather; fishers double their yields.")
        ],
        cap_items=[
            ("[F32] COASTAL &amp; DEEP ANGLING: ", "Casts into coastal waters to hook Perch, Pike, Trout, and Tuna."),
            ("[F33] REEL TENSION MANAGEMENT: ", "Successfully lands fish without snapping lines or running out of stamina."),
            ("[F34] DOCK FILLETING &amp; STORAGE: ", "Cleans catches into raw fish units and routes them to cold storage larders.")
        ],
        synergy_items=[
            ("CHEF'S FISH WRAP SUPPLY: ", "Supplies fish fillets to the Cook for endgame Fish Wraps (+60 stamina)."),
            ("MERCHANT COIN CYCLE: ", "Fish catches can be traded with Haldor or crafted into trophy mounts."),
            ("COASTAL SECURITY VIGIL: ", "Observes maritime approaches, warning Sentries of swimming threats.")
        ]
    )

    # Page 17: Part IX: The Hauler
    roles[17] = make_worker_page(
        page_num=17,
        part_roman="PART IX",
        role_title="THE HAULER",
        role_sub="LOGISTICS PORTER &amp; QUARTERMASTER",
        role_pill="TRANSPORT &bull; FREIGHT",
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
    )

    # Page 19: Part X: The Sentry
    roles[19] = make_worker_page(
        page_num=19,
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
    )

    return roles
