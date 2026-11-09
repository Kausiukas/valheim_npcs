import os
import subprocess

html_content = """<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>The Settlement Steward's Codex — Official Guide to Valheim NPC Workers</title>
  <style>
    @page {
      size: A4 portrait;
      margin: 10mm 12mm 10mm 12mm;
    }
    @media print {
      body {
        -webkit-print-color-adjust: exact !important;
        print-color-adjust: exact !important;
      }
      .page {
        page-break-before: always;
        break-before: page;
        page-break-inside: avoid;
        break-inside: avoid;
        height: 275mm;
        max-height: 275mm;
        overflow: hidden;
      }
      .page-first {
        page-break-before: auto;
        break-before: auto;
      }
    }
    
    * {
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }
    
    body {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
      background-color: #FAF8F5;
      color: #1C1917;
      line-height: 1.36;
      font-size: 8.4pt;
    }
    
    h1, h2, h3, h4 {
      font-family: "Cinzel", "Georgia", "Times New Roman", serif;
      font-weight: 700;
      color: #0F172A;
      letter-spacing: -0.01em;
    }

    .page {
      height: 275mm;
      max-height: 275mm;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      page-break-before: always;
      break-before: page;
      padding-bottom: 2mm;
      overflow: hidden;
    }
    .page-first {
      page-break-before: avoid;
      break-before: avoid;
    }

    /* Cover Page */
    .cover-page {
      background: linear-gradient(145deg, #0B0F19 0%, #171E2E 50%, #0D131F 100%);
      color: #F8FAFC;
      border: 3px double #D97706;
      border-radius: 8px;
      padding: 18mm 16mm 14mm 16mm;
      text-align: center;
      align-items: center;
    }
    .cover-badge {
      display: inline-block;
      padding: 4px 14px;
      border-radius: 20px;
      background: rgba(217, 119, 6, 0.2);
      border: 1px solid #D97706;
      color: #FBBF24;
      font-size: 9pt;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.1em;
    }
    .cover-title {
      font-size: 24pt;
      line-height: 1.15;
      color: #FFFFFF;
      margin-top: 12px;
      margin-bottom: 6px;
      text-shadow: 0 2px 10px rgba(0,0,0,0.5);
    }
    .cover-subtitle {
      font-size: 11pt;
      color: #94A3B8;
      max-width: 520px;
      margin: 0 auto 16px auto;
      font-style: italic;
    }
    .cover-divider {
      width: 140px;
      height: 2px;
      background: linear-gradient(90deg, transparent, #D97706, transparent);
      margin: 12px auto;
    }
    .cover-features {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 10px;
      max-width: 580px;
      margin: 14px auto;
      text-align: left;
    }
    .cover-feat-box {
      background: rgba(30, 41, 59, 0.7);
      border: 1px solid #334155;
      border-radius: 6px;
      padding: 8px 12px;
    }
    .cover-feat-box strong {
      color: #FBBF24;
      display: block;
      font-size: 8.5pt;
      margin-bottom: 2px;
    }
    .cover-feat-box p {
      color: #CBD5E1;
      font-size: 7.6pt;
      line-height: 1.3;
    }
    .cover-meta {
      margin-top: 14px;
      font-size: 8pt;
      color: #64748B;
    }

    /* Headers & Footers */
    .chapter-header {
      border-bottom: 2px solid #D97706;
      padding-bottom: 4px;
      margin-bottom: 8px;
      display: flex;
      justify-content: space-between;
      align-items: baseline;
    }
    .chapter-header h2 {
      font-size: 14pt;
      color: #1E293B;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .role-pill {
      background: #D97706;
      color: #FFFFFF;
      font-size: 7.5pt;
      font-weight: 700;
      padding: 2px 8px;
      border-radius: 12px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .page-footer {
      border-top: 1px solid #E2E8F0;
      padding-top: 3px;
      display: flex;
      justify-content: space-between;
      font-size: 7pt;
      color: #64748B;
      margin-top: 4px;
    }

    /* Panels */
    .panel {
      border-radius: 5px;
      padding: 7px 10px;
      margin-bottom: 7px;
      border-left: 3.5px solid;
      background: #FFFFFF;
      box-shadow: 0 1px 3px rgba(0,0,0,0.03);
    }
    .panel h3 {
      font-size: 9pt;
      margin-bottom: 3px;
      display: flex;
      align-items: center;
      gap: 5px;
    }
    .panel p {
      font-size: 8pt;
      line-height: 1.35;
      color: #334155;
      margin-bottom: 3px;
    }
    .panel ul {
      margin-left: 14px;
      font-size: 7.8pt;
      color: #334155;
      line-height: 1.35;
    }
    .panel li {
      margin-bottom: 2.5px;
    }
    .panel-now {
      border-left-color: #059669;
      background: #F0FDF4;
    }
    .panel-now h3 { color: #065F46; }
    
    .panel-future {
      border-left-color: #2563EB;
      background: #EFF6FF;
    }
    .panel-future h3 { color: #1E40AF; }

    .panel-rec {
      border-left-color: #D97706;
      background: #FFFBEB;
    }
    .panel-rec h3 { color: #92400E; }

    .panel-warn {
      border-left-color: #DC2626;
      background: #FEF2F2;
    }
    .panel-warn h3 { color: #991B1B; }

    .feature-badge {
      display: inline-flex;
      align-items: center;
      gap: 3px;
      background: #FEF3C7;
      color: #92400E;
      border: 1px solid #F59E0B;
      border-radius: 4px;
      padding: 1px 5px;
      font-size: 7.4pt;
      font-weight: 700;
      margin-right: 4px;
    }
    .domain-tag {
      font-size: 6.8pt;
      padding: 1px 4px;
      border-radius: 3px;
      background: #E2E8F0;
      color: #475569;
      font-weight: 600;
      text-transform: uppercase;
      margin-left: 4px;
    }

    /* Tables */
    table {
      width: 100%;
      border-collapse: collapse;
      margin: 6px 0;
      font-size: 7.5pt;
      background: #FFFFFF;
    }
    th, td {
      border: 1px solid #CBD5E1;
      padding: 4px 6px;
      text-align: left;
    }
    th {
      background: #1E293B;
      color: #F8FAFC;
      font-weight: 700;
      font-size: 7.3pt;
      text-transform: uppercase;
      letter-spacing: 0.03em;
    }
    tr:nth-child(even) td {
      background: #F8FAFC;
    }

    /* Catalog Grid Layout */
    .cat-compact-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 6px;
      margin-bottom: 6px;
    }
    .cat-card-mini {
      background: #FFFFFF;
      border: 1px solid #E2E8F0;
      border-radius: 4px;
      padding: 5px 8px;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      border-left: 3px solid #D97706;
    }
    .cat-card-head {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2px;
    }
    .cat-card-title {
      font-weight: 700;
      font-size: 7.9pt;
      color: #0F172A;
      display: flex;
      align-items: center;
      gap: 4px;
    }
    .cat-card-desc {
      font-size: 7.2pt;
      color: #475569;
      line-height: 1.25;
      margin-bottom: 3px;
    }
    .cat-card-workers {
      font-size: 6.8pt;
      color: #0369A1;
      font-weight: 600;
      background: #F0F9FF;
      padding: 1px 4px;
      border-radius: 3px;
      display: inline-block;
      align-self: flex-start;
    }

    /* Matrix Table */
    table.matrix-view {
      font-size: 6.8pt;
      text-align: center;
    }
    table.matrix-view th, table.matrix-view td {
      padding: 2.5px 1.5px;
      border: 1px solid #CBD5E1;
    }
    table.matrix-view th.feat-hdr {
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      height: 72px;
      font-size: 6.8pt;
      padding: 4px 1px;
    }
    table.matrix-view th.worker-hdr {
      text-align: left;
      padding-left: 6px;
      width: 95px;
    }
    table.matrix-view td.w-name {
      text-align: left;
      font-weight: 700;
      padding-left: 6px;
      background: #F1F5F9;
      color: #0F172A;
    }
    .m-yes {
      color: #059669;
      font-weight: bold;
      font-size: 8pt;
    }
    .m-no {
      color: #CBD5E1;
      font-size: 6pt;
    }

    /* TOC Grid */
    .toc-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 8px;
      margin-top: 6px;
    }
    .toc-card {
      background: #FFFFFF;
      border: 1px solid #E2E8F0;
      border-radius: 6px;
      padding: 8px 10px;
    }
    .toc-card h4 {
      font-size: 8.5pt;
      color: #D97706;
      border-bottom: 1px solid #FEF3C7;
      padding-bottom: 2px;
      margin-bottom: 5px;
    }
    .toc-list {
      list-style: none;
      font-size: 7.7pt;
    }
    .toc-list li {
      display: flex;
      justify-content: space-between;
      margin-bottom: 2.5px;
      border-bottom: 1px dotted #E2E8F0;
      padding-bottom: 1px;
    }
    .toc-list li span:last-child {
      font-weight: bold;
      color: #475569;
      font-size: 7.2pt;
    }
  </style>
</head>
<body>

  <!-- ==================== PAGE 1: COVER ==================== -->
  <div class="page page-first cover-page">
    <div style="flex: 1; display: flex; flex-direction: column; justify-content: center; align-items: center; width: 100%;">
      <span class="cover-badge">ValheimWorkers v0.3.0 &bull; Official Standard Codex</span>
      <h1 class="cover-title">THE SETTLEMENT<br>STEWARD'S CODEX</h1>
      <p class="cover-subtitle">Complete Architecture, Autonomous AI Behaviors, 36-Feature Map &amp; Domain Operations</p>
      
      <div class="cover-divider"></div>

      <div class="cover-features">
        <div class="cover-feat-box">
          <strong>Autonomous Labor Engine</strong>
          <p>11 specialized worker roles with persistent spatial post memory, auto-repair, smart sorting, and dynamic unstuck.</p>
        </div>
        <div class="cover-feat-box">
          <strong>36 Specialized Feature Icons</strong>
          <p>Master F01–F36 capability map cleanly categorizing harvesting, combat, refining, and social mechanics.</p>
        </div>
        <div class="cover-feat-box">
          <strong>Full Logistics Infrastructure</strong>
          <p>Resource pile stacking (wood, stone, coal), sign-based [Tag] routing, and player Vagon cart loading.</p>
        </div>
        <div class="cover-feat-box">
          <strong>Adaptive Terrain &amp; Combat</strong>
          <p>Quarry leveling with Hoe, subterranean ore exposure, and hunter-grade parabolic archery with tactical CQC swap.</p>
        </div>
      </div>

      <div class="cover-divider"></div>
      
      <div class="cover-meta">
        Valheim BepInEx 5.4.22+ &bull; Harmony 2.0 &bull; Authored for High-Immersion Autonomous Settlements
      </div>
    </div>
  </div>

  <!-- ==================== PAGE 2: TABLE OF CONTENTS ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Table of Contents &amp; Executive Blueprint</h2>
        <span class="role-pill">Codex Index</span>
      </div>

      <div class="toc-grid">
        <div class="toc-card">
          <h4>Part I: Foundations &amp; System Architecture</h4>
          <ul class="toc-list">
            <li><span>Core Controls &amp; In-World Hotkeys</span> <span>Page 3</span></li>
            <li><span>Dual-Anchor Spatial Post Architecture</span> <span>Page 3</span></li>
            <li><span>8&times;4 Container Storage &amp; Sorting</span> <span>Page 3</span></li>
            <li><span>3-Slot Vitality &amp; Sustenance Rations</span> <span>Page 3</span></li>
            <li><span>Recruitable Heritages (Viking, Dvergr, Skeleton)</span> <span>Page 3</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4>Part II: 36-Feature Map &amp; Separation Matrix</h4>
          <ul class="toc-list">
            <li><span>The 36-Feature Icon Catalog (F01–F36)</span> <span>Page 4</span></li>
            <li><span>Master Worker &times; Feature Capability Matrix</span> <span>Page 5</span></li>
            <li><span>Domain Classification &amp; Operational Hierarchy</span> <span>Page 5</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4>Part III: Primary Harvesting &amp; Extraction Roles</h4>
          <ul class="toc-list">
            <li><span>The Lumberjack (Forester &amp; Arborist)</span> <span>Page 6</span></li>
            <li><span>The Miner (Quarryman &amp; Excavator)</span> <span>Page 7</span></li>
            <li><span>The Farmer (Husbandman &amp; Cultivator)</span> <span>Page 8</span></li>
            <li><span>The Gatherer (Forager &amp; Apiarist)</span> <span>Page 9</span></li>
            <li><span>The Hunter (Ranger &amp; Marksman)</span> <span>Page 10</span></li>
          </ul>
        </div>

        <div class="toc-card">
          <h4>Part IV: Base Production &amp; Defense Roles</h4>
          <ul class="toc-list">
            <li><span>The Guard (Perimeter Sentinel &amp; Warden)</span> <span>Page 11</span></li>
            <li><span>The Smelter (Furnace Master &amp; Pyrotechnic)</span> <span>Page 12</span></li>
            <li><span>The Base Tender (Camp Steward &amp; Miller)</span> <span>Page 13</span></li>
            <li><span>The Cook (Culinary Artisan &amp; Brewer)</span> <span>Page 14</span></li>
            <li><span>The Hauler (Logistics Operator &amp; Packmaster)</span> <span>Page 15</span></li>
            <li><span>The Craftsman (Artificer &amp; Carpenter)</span> <span>Page 16</span></li>
          </ul>
        </div>
      </div>

      <div style="margin-top: 8px;" class="toc-card">
        <h4>Part V: Settlement Infrastructure, Diagnostics &amp; Console Reference</h4>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 8px; margin-top: 4px; font-size: 7.6pt;">
          <div><strong>Page 17:</strong> Logistics Mastery: Overflow Resource Stacks [F05], Sign Tags [F32], Carts &amp; Ships [F34]</div>
          <div><strong>Page 18:</strong> Edge Cases, Fallbacks &amp; Automated Verification Telemetry Suite</div>
          <div><strong>Page 19:</strong> Master Settlement Roster &amp; Quick-Reference Capability Comparison</div>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 2</span>
    </div>
  </div>

  <!-- ==================== PAGE 3: FOUNDATIONS ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part I: Core Controls &amp; Foundations</h2>
        <span class="role-pill">Spatial Logic &amp; Vitality</span>
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

  <!-- ==================== PAGE 4: 36-FEATURE ICON CATALOG ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part II: The 36-Feature Icon Catalog (F01–F36)</h2>
        <span class="role-pill">Capability Taxonomy</span>
      </div>

      <p style="font-size: 7.6pt; color: #475569; margin-bottom: 5px;">Every worker capability in ValheimWorkers v0.3.0 is indexed into a standardized 36-feature taxonomy across 8 operational domains:</p>

      <div class="cat-compact-grid">
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F01 🪓</span> Forestry &amp; Felling</span><span class="domain-tag">Forestry</span></div>
          <p class="cat-card-desc">Fells mature trees, birch/oak logs, and saplings matching axe tier.</p>
          <span class="cat-card-workers">Lumberjack</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F02 🪵</span> Stump Eradication</span><span class="domain-tag">Forestry</span></div>
          <p class="cat-card-desc">Prioritizes stumps and fallen logs (-15m distance bonus) for clean perimeters.</p>
          <span class="cat-card-workers">Lumberjack</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F03 🌲</span> Forest Replanting</span><span class="domain-tag">Forestry</span></div>
          <p class="cat-card-desc">Replants pine, fir, beech, and birch seeds in cleared terrain.</p>
          <span class="cat-card-workers">Lumberjack</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F04 🌿</span> Seed Extraction</span><span class="domain-tag">Forestry</span></div>
          <p class="cat-card-desc">Extracts tree seeds and cones from falling timber into camp pantries.</p>
          <span class="cat-card-workers">Lumberjack</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F05 📦</span> Overflow Pile Stacking</span><span class="domain-tag">Logistics</span></div>
          <p class="cat-card-desc">Constructs physical 20-50 resource piles (wood, stone, coal, ore) when chests full.</p>
          <span class="cat-card-workers">All Labor Roles</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F06 ⛏️</span> Surface Quarrying</span><span class="domain-tag">Mining</span></div>
          <p class="cat-card-desc">Shatters surface boulders, rocks, and flint nodes with pickaxe.</p>
          <span class="cat-card-workers">Miner</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F07 💎</span> Deep Vein Excavation</span><span class="domain-tag">Mining</span></div>
          <p class="cat-card-desc">Slices copper, tin, silver, and iron scrap deposits tier-by-tier.</p>
          <span class="cat-card-workers">Miner</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F08 🪚</span> Even the Field (Hoe)</span><span class="domain-tag">Mining</span></div>
          <p class="cat-card-desc">Uses Hoe to level jagged quarry terrain and eliminate pit obstacles.</p>
          <span class="cat-card-workers">Miner</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F09 ⛰️</span> Subterranean Vein Exposure</span><span class="domain-tag">Mining</span></div>
          <p class="cat-card-desc">Clears overlying soil down to bedrock to expose hidden MineRock5 chunks.</p>
          <span class="cat-card-workers">Miner</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F10 🌾</span> Crop Harvesting</span><span class="domain-tag">Agriculture</span></div>
          <p class="cat-card-desc">Identifies mature vegetables and grains, harvesting yields cleanly.</p>
          <span class="cat-card-workers">Farmer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F11 🚜</span> Soil Cultivation</span><span class="domain-tag">Agriculture</span></div>
          <p class="cat-card-desc">Prepares fresh arable soil patches using Cultivator when fields are full.</p>
          <span class="cat-card-workers">Farmer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F12 🌱</span> Proactive Sowing (Every Seed)</span><span class="domain-tag">Agriculture</span></div>
          <p class="cat-card-desc">Withdraws seeds from camp chests and proactively plants every seed.</p>
          <span class="cat-card-workers">Farmer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F13 🔁</span> Automatic Seed Expansion</span><span class="domain-tag">Agriculture</span></div>
          <p class="cat-card-desc">Maintains optimal 1:3 seed-to-produce ratio for perpetual farming yields.</p>
          <span class="cat-card-workers">Farmer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F14 🍓</span> Wild Plant Foraging</span><span class="domain-tag">Forestry</span></div>
          <p class="cat-card-desc">Forages wild berries, mushrooms, thistle, dandelion, and cloudberries.</p>
          <span class="cat-card-workers">Gatherer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F15 🍯</span> Apiary &amp; Sap Extraction</span><span class="domain-tag">Culinary</span></div>
          <p class="cat-card-desc">Harvests honey from beehives and extracts sap from Yggdrasil collectors.</p>
          <span class="cat-card-workers">Gatherer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F16 🐗</span> Livestock Feeding</span><span class="domain-tag">Agriculture</span></div>
          <p class="cat-card-desc">Feeds tamed boars, lox, chickens, and wolves with matching foods.</p>
          <span class="cat-card-workers">Farmer</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F17 🎯</span> Wilderness Stalking</span><span class="domain-tag">Combat</span></div>
          <p class="cat-card-desc">Sneak-stalks wild game (deer, boar, neck) with reduced alert radiuses.</p>
          <span class="cat-card-workers">Hunter</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F18 🏹</span> Hunter-Grade Archery</span><span class="domain-tag">Combat</span></div>
          <p class="cat-card-desc">Parabolic long-range archery with lead shots and high vantage elevation.</p>
          <span class="cat-card-workers">Hunter, Guard</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F19 🛡️</span> Tactical CQC Swap</span><span class="domain-tag">Combat</span></div>
          <p class="cat-card-desc">Instantly swaps to melee weapon and shield when enemy is &le;9.5m.</p>
          <span class="cat-card-workers">Guard</span>
        </div>
        <div class="cat-card-mini">
          <div class="cat-card-head"><span class="cat-card-title"><span class="feature-badge">F20 🔨</span> Field Gear Repair</span><span class="domain-tag">Logistics</span></div>
          <p class="cat-card-desc">Visits workbench to repair worn tools/weapons and crafts emergency arrows.</p>
          <span class="cat-card-workers">Hunter, Guard</span>
        </div>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 4</span>
    </div>
  </div>

  <!-- ==================== PAGE 5: CAPABILITY MATRIX ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part II: Worker &times; Feature Separation Matrix</h2>
        <span class="role-pill">Master Separation Grid</span>
      </div>

      <p style="font-size: 7.5pt; color: #475569; margin-bottom: 4px;">This matrix displays all 11 worker roles across the 36 features, illustrating distinct job separation:</p>

      <table class="matrix-view">
        <thead>
          <tr>
            <th class="worker-hdr">Worker Role</th>
            <th class="feat-hdr">F01 Tree Felling</th>
            <th class="feat-hdr">F02 Stump Clearing</th>
            <th class="feat-hdr">F03 Replanting</th>
            <th class="feat-hdr">F04 Seed Extract</th>
            <th class="feat-hdr">F05 Overflow Piles</th>
            <th class="feat-hdr">F06 Quarrying</th>
            <th class="feat-hdr">F07 Vein Mining</th>
            <th class="feat-hdr">F08 Even Field Hoe</th>
            <th class="feat-hdr">F09 Vein Exposure</th>
            <th class="feat-hdr">F10 Crop Harvest</th>
            <th class="feat-hdr">F11 Cultivation</th>
            <th class="feat-hdr">F12 Proactive Sowing</th>
            <th class="feat-hdr">F13 Seed Expand</th>
            <th class="feat-hdr">F14 Plant Forage</th>
            <th class="feat-hdr">F15 Honey &amp; Sap</th>
            <th class="feat-hdr">F16 Feed Animals</th>
            <th class="feat-hdr">F17 Game Stalk</th>
            <th class="feat-hdr">F18 Ballistic Bow</th>
            <th class="feat-hdr">F19 Tactical CQC</th>
            <th class="feat-hdr">F20 Tool Repair</th>
            <th class="feat-hdr">F21 Nav &amp; Unstuck</th>
            <th class="feat-hdr">F22 War Horn Patrol</th>
            <th class="feat-hdr">F23 Cauldron Cook</th>
            <th class="feat-hdr">F24 Mead Base</th>
            <th class="feat-hdr">F25 Ferment Mead</th>
            <th class="feat-hdr">F26 Kiln Wood</th>
            <th class="feat-hdr">F27 Smelter Ores</th>
            <th class="feat-hdr">F28 Mill &amp; Flax</th>
            <th class="feat-hdr">F29 Fuel Fires</th>
            <th class="feat-hdr">F30 Crafting</th>
            <th class="feat-hdr">F31 Smart Chest</th>
            <th class="feat-hdr">F32 Sign Tag Route</th>
            <th class="feat-hdr">F33 Vagon Cart</th>
            <th class="feat-hdr">F34 Board Ship</th>
            <th class="feat-hdr">F35 Night Torch</th>
            <th class="feat-hdr">F36 Tavern Rest</th>
          </tr>
        </thead>
        <tbody>
          <tr><td class="w-name">Lumberjack</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Miner</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Farmer</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Gatherer</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Hunter</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Guard</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Smelter</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">BaseTender</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Cook</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Hauler</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
          <tr><td class="w-name">Craftsman</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-no">&middot;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td><td class="m-yes">&check;</td></tr>
        </tbody>
      </table>

      <div class="panel panel-now" style="margin-top: 6px;">
        <h3>Domain Legend &amp; Universal Core Capabilities</h3>
        <p><strong>Universal Features:</strong> All workers possess <code>[F21] Autonomous Navigation</code>, <code>[F31] Smart Chest Sorting</code>, <code>[F32] Sign-Based Tag Routing</code>, <code>[F34] Longship Boarding</code>, <code>[F35] Night Torch Bearing</code>, and <code>[F36] Tavern Rest &amp; Seating</code>.</p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 5</span>
    </div>
  </div>

  <!-- ==================== PAGE 6: LUMBERJACK ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part III: The Lumberjack (Forester &amp; Arborist)</h2>
        <span class="role-pill">Harvesting &bull; Forestry</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F01 🪓</span> <span class="feature-badge">F02 🪵</span> <span class="feature-badge">F05 📦</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F01] 🪓 Selective Timber Felling:</strong> Scans within Work Post radius for beech, birch, oak, and fir trees matching equipped Axe tier (Stone, Flint, Bronze, Iron, Blackmetal).</li>
          <li><strong>[F02] 🪵 Stump-First Eradication Priority:</strong> Prioritizes felled tree logs and unsightly stumps with a -15m distance bonus over standing trees to keep settlement clearings clean.</li>
          <li><strong>[F05] 📦 Overflow Resource Stacking:</strong> When camp chests are full, automatically builds physical 50x <code>wood_stack</code>, <code>wood_fine_stack</code>, or <code>wood_core_stack</code> pieces.</li>
          <li><strong>[F32] 🏷️ Sign-Based Routing:</strong> Routes wood directly into chests labeled <code>[Wood]</code>, <code>[FineWood]</code>, or <code>[CoreWood]</code> before scanning general chests.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3><span class="feature-badge">F03 🌲</span> <span class="feature-badge">F04 🌿</span> Planned Future Expansions</h3>
        <ul>
          <li><strong>[F03] 🌲 Automated Tree Nursery:</strong> Proactive sapling plantation in designated reforestation plots away from buildings.</li>
          <li><strong>[F04] 🌿 Seed Extraction &amp; Pine Resin Tapping:</strong> Extracting cones and seeds during felling and storing them for architectural expansion.</li>
          <li><strong>Felling Physics Prediction:</strong> Directional tree wedging to prevent falling timber from crushing nearby player fences.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Place Work Post at the edge of a mature grove; place a chest labeled <code>[Wood]</code> near the main gate.</li>
          <li>Always supply an Axe with at least Tier 2 (Bronze) if birch or fine wood trees are within the harvesting zone.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Log Roll Distance:</strong> Rolling logs down steep inclines can pull lumberjacks beyond post radius; tether work post tightly on cliffs.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 6</span>
    </div>
  </div>

  <!-- ==================== PAGE 7: MINER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part IV: The Miner (Quarryman &amp; Excavator)</h2>
        <span class="role-pill">Harvesting &bull; Mining</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F06 ⛏️</span> <span class="feature-badge">F07 💎</span> <span class="feature-badge">F08 🪚</span> <span class="feature-badge">F09 ⛰️</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F06] ⛏️ Surface Quarrying &amp; Boulders:</strong> Breaks down surface rocks, loose stones, and flint boulders using pickaxes.</li>
          <li><strong>[F07] 💎 Deep Vein Excavation:</strong> Extracts Copper, Tin, Iron Scrap, and Silver veins using tool tier verification (Antler, Bronze, Iron).</li>
          <li><strong>[F08] 🪚 Even the Field (Hoe Leveling):</strong> Equips a Hoe to level and smooth jagged quarry terrain after shattering rock faces, eliminating pit traps.</li>
          <li><strong>[F09] ⛰️ Subterranean Vein Exposure:</strong> Lowers overlying soil layers to expose subterranean chunks of buried <code>MineRock5</code> veins.</li>
          <li><strong>[F05] 📦 Overflow Stacks:</strong> Constructs 50x <code>stone_pile</code> pieces when camp storage chests cannot accept more stone.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Deep Pit Staircarving:</strong> Auto-cutting stone steps along pit perimeters for easy cart descent.</li>
          <li><strong>Minecart Rail Hauling:</strong> Loading subterranean ore hoppers connected to surface pulleys.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Provide both a Pickaxe and a Hoe in the miner's inventory to unlock automated pit leveling.</li>
          <li>Mark chests near the foundry with <code>[Stone]</code> and <code>[Ore]</code> for automatic sign routing.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Tool Durability:</strong> Deep copper deposits rapidly deplete pickaxe durability; place an upgraded workbench near the quarry.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 7</span>
    </div>
  </div>

  <!-- ==================== PAGE 8: FARMER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part V: The Farmer (Husbandman &amp; Cultivator)</h2>
        <span class="role-pill">Agriculture &bull; Sustenance</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F10 🌾</span> <span class="feature-badge">F11 🚜</span> <span class="feature-badge">F12 🌱</span> <span class="feature-badge">F13 🔁</span> <span class="feature-badge">F16 🐗</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F10] 🌾 Crop Harvesting:</strong> Harvests ripe carrots, turnips, onions, barley, and flax within garden perimeter.</li>
          <li><strong>[F11] 🚜 Soil Cultivation:</strong> Prepares fresh cultivated arable land with Cultivator when existing plots are fully planted.</li>
          <li><strong>[F12] 🌱 Proactive Sowing (Every Seed):</strong> Withdraws vegetable/grain seeds from camp chests and proactively sows every available seed across tilled soil.</li>
          <li><strong>[F13] 🔁 Automatic Seed Expansion:</strong> Maintains perpetual cycle by planting seed crops to yield 3x seeds per plant.</li>
          <li><strong>[F16] 🐗 Tamed Livestock Feeding:</strong> Scans for hungry tamed Boars, Lox, Chickens, and Wolves, dropping crops or meats into their pens.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Livestock Breeding Cap:</strong> Culling excess animals once pen population exceeds 12 to maintain game performance.</li>
          <li><strong>Irrigation Ditches:</strong> Trench digging for accelerated crop maturation during rainy weather.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Enclose farmland with stake walls to prevent wild boars or greydwarfs from trampling tender seedlings.</li>
          <li>Label chests with <code>[Seeds]</code> and <code>[Food]</code> near garden gates for automated resupply.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Cultivator Tier:</strong> Farmer requires a Bronze Cultivator to till soil and plant crops; cannot plant without tool.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 8</span>
    </div>
  </div>

  <!-- ==================== PAGE 9: GATHERER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part VI: The Gatherer (Forager &amp; Apiarist)</h2>
        <span class="role-pill">Harvesting &bull; Foraging</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F14 🍓</span> <span class="feature-badge">F15 🍯</span> <span class="feature-badge">F05 📦</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F14] 🍓 Flora &amp; Fungi Foraging:</strong> Gathers raspberries, blueberries, cloudberries, red/yellow mushrooms, thistle, and dandelion.</li>
          <li><strong>[F15] 🍯 Apiary &amp; Sap Extraction:</strong> Harvests honeycomb from camp beehives and extracts raw Yggdrasil sap from Sap Collectors.</li>
          <li><strong>Toolless Loose Gathering:</strong> Collects loose ground stones, branches, and flint without requiring equipped axes or pickaxes.</li>
          <li><strong>[F05] 📦 Overflow Stacks:</strong> Builds resource piles when inventory is overflowing and camp chests are congested.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Mushroom Spore Cultivation:</strong> In-ground potting of glowing yellow mushrooms inside cellar basements.</li>
          <li><strong>Wisp Harvesting:</strong> Automatic collection of floating wisps from wisp traps during Mistlands nights.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Group beehives together in an open sunny courtyard and place a chest labeled <code>[Food]</code> or <code>[Berries]</code> nearby.</li>
          <li>Gatherers are excellent for early-game camp maintenance since they do not consume tool durability.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Wild Bush Regrowth:</strong> Natural berry bushes take 5 in-game hours to fruit; gatherer will idle once perimeter is cleared.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 9</span>
    </div>
  </div>

  <!-- ==================== PAGE 10: HUNTER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part VII: The Hunter (Ranger &amp; Marksman)</h2>
        <span class="role-pill">Combat &bull; Game Trapping</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F17 🎯</span> <span class="feature-badge">F18 🏹</span> <span class="feature-badge">F20 🔨</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F17] 🎯 Wilderness Stalking:</strong> Tracks deer, boar, neck, and wild fowl within work radius using stealth posture and wind evasion.</li>
          <li><strong>[F18] 🏹 Hunter-Grade Archery:</strong> Calculates target velocity, lead distance, and parabolic arc to fire precise bow shots up to 35m.</li>
          <li><strong>[F20] 🔨 Arrow Crafting &amp; Field Repair:</strong> Automatically visits workbench to repair bow durability and crafts fresh wood arrows if supply falls below 40.</li>
          <li><strong>Tamed Creature Immunity:</strong> Scans creature flags and strictly ignores tamed boars, wolves, and lox.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Trapping &amp; Snares:</strong> Setting wooden pit traps for wolves and hare in mountain/mistlands borders.</li>
          <li><strong>Scent Masking:</strong> Applying swamp muck to approach skittish deer within point-blank range.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Keep an accessible sheltered workbench near the hunter's post so they can repair bows and craft arrows uninterrupted.</li>
          <li>Mark chests with <code>[Food]</code> and <code>[Trophy]</code> for raw meat, deer hide, and trophy drop storage.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Water Fleeing:</strong> Deer wounded near shorelines may swim out into deep water; hunter will wait on shore for parabolic shot.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 10</span>
    </div>
  </div>

  <!-- ==================== PAGE 11: GUARD ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part VIII: The Guard (Perimeter Sentinel &amp; Warden)</h2>
        <span class="role-pill">Defense &bull; Security</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F18 🏹</span> <span class="feature-badge">F19 🛡️</span> <span class="feature-badge">F20 🔨</span> <span class="feature-badge">F22 📯</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F18] 🏹 Hunter-Grade Archery (Long Range):</strong> Equips bow and picks off invading greydwarfs, skeletons, and trolls at 10m–40m with lead calculations.</li>
          <li><strong>[F19] 🛡️ Tactical CQC Weapon Swap:</strong> Automatically draws melee sword/axe and raises shield when enemies close within 9.5m.</li>
          <li><strong>[F22] 📯 War Horn Alert &amp; Multi-Waypoint Patrol:</strong> Walks defensive circuits between Camp Post and Work Post; sounds War Horn alert when threat &ge; 150.</li>
          <li><strong>[F20] 🔨 Off-Combat Workbench Repair:</strong> Retreats to camp workbench after skirmishes to repair armor, weapons, and restock arrows.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Ballista Battery Manning:</strong> Loading and targeting camp ballistas against airborne drakes and gjalls.</li>
          <li><strong>Palisade Gate Locking:</strong> Closing gates during active raids and raising drawbridges.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Station Guards on elevated watchtowers or gatehouse bastions with clear sightlines over camp entrances.</li>
          <li>Provide a sturdy bow, 100+ arrows, a sword or mace, and a round/banded shield for optimal stance swapping.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Raid Mob Swarms:</strong> Trolls with tree clubs deal area-of-effect damage; keep guards elevated above club sweep radius.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 11</span>
    </div>
  </div>

  <!-- ==================== PAGE 12: SMELTER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part IX: The Smelter (Furnace Master &amp; Pyrotechnic)</h2>
        <span class="role-pill">Foundry &bull; Refining</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F26 🔥</span> <span class="feature-badge">F27 🏭</span> <span class="feature-badge">F05 📦</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F26] 🔥 Charcoal Kiln Stoking:</strong> Feeds regular wood, fine wood, or core wood into kilns to maintain uninterrupted coal reserves.</li>
          <li><strong>[F27] 🏭 Smelter &amp; Blast Furnace Refining:</strong> Transports raw copper, tin, iron, silver, and blackmetal ores into smelters, feeding fuel coal simultaneously.</li>
          <li><strong>Ingot Collection &amp; Sorting:</strong> Sweeps refined copper, bronze, iron, silver, and blackmetal ingots from smelter output chutes into designated chests.</li>
          <li><strong>[F05] 📦 Overflow Stacks:</strong> Builds <code>coal_pile</code> stacks when coal chests are filled to capacity.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Eitr Refinery Operation:</strong> Loading soft tissue and sap into eitr refineries with rad-hazard protective timing.</li>
          <li><strong>Automated Bronze Forging:</strong> Combining copper and tin directly inside forge cauldrons.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Place Smelter and Kiln input ports within 6m of chests marked <code>[Ore]</code>, <code>[Wood]</code>, and <code>[Coal]</code>.</li>
          <li>Leave 2m clear walking space in front of smelters to prevent the worker from clipping on hot output chutes.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Weight Congestion:</strong> Raw ores are heavy; smelters carry manageable batches to avoid encumbrance speed debuffs.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 12</span>
    </div>
  </div>

  <!-- ==================== PAGE 13: BASE TENDER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part X: The Base Tender (Camp Steward &amp; Miller)</h2>
        <span class="role-pill">Camp Utility &bull; Hearth</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F28 💨</span> <span class="feature-badge">F29 🕯️</span> <span class="feature-badge">F05 📦</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F28] 💨 Windmill &amp; Spinning Wheel Tending:</strong> Loads harvested Barley into Windmills to grind barley flour, and feeds Flax into Spinning Wheels for linen thread.</li>
          <li><strong>[F29] 🕯️ Hearth, Fireplace &amp; Sconce Refueling:</strong> Keeps campfires, stone hearths, hanging braziers, wall sconces, and standing torches burning by refueling wood, resin, and coal.</li>
          <li><strong>Camp Comfort Maintenance:</strong> Ensures resting player always enjoys maximum comfort bonus upon returning to base.</li>
          <li><strong>[F05] 📦 Overflow Stacks:</strong> Builds wood and coal piles when fuel storage is full.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Hot Tub &amp; Bath Stoking:</strong> Heating water in Viking cedar tubs for extended luxury rest bonuses.</li>
          <li><strong>Roof Thatching Repair:</strong> Using hammer to fix weather-decayed roof beams after heavy rainstorms.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Place a central fuel chest labeled <code>[Wood]</code> and <code>[Resin]</code> in the main courtyard.</li>
          <li>One Base Tender can maintain up to 20 fires and 4 windmills within a 25m Camp Post radius.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Resin Depletion:</strong> If base has dozens of wall sconces, resin will deplete quickly; pair Base Tender with a Gatherer.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 13</span>
    </div>
  </div>

  <!-- ==================== PAGE 14: COOK ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part XI: The Cook (Culinary Artisan &amp; Brewer)</h2>
        <span class="role-pill">Kitchen &bull; Alchemy</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F23 🍖</span> <span class="feature-badge">F24 🍯</span> <span class="feature-badge">F25 🍺</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F23] 🍖 Spit Roasting &amp; Stew Crafting:</strong> Roasts meats on cooking spits without burning, and simmers deer stew, wolf skewers, and sausages in Cauldrons.</li>
          <li><strong>[F24] 🍯 Mead Base Alchemy:</strong> Mixes honey and herbal ingredients to prepare health, stamina, poison resist, and frost resist mead bases.</li>
          <li><strong>[F25] 🍺 Fermenter Tapping:</strong> Inspects camp Fermenters and automatically taps aged mead potions into pantry chests once fermentation completes.</li>
          <li><strong>[F32] 🏷️ Pantry Routing:</strong> Places cooked food items directly into chests marked with <code>[Food]</code>.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Stone Oven Baking:</strong> Baking lox meat pies, bread loaves, and serpent pasties in stone ovens.</li>
          <li><strong>Banquet Serving:</strong> Setting prepared feasts onto longhouse feast tables for instant settlement buffing.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Place pantry chest marked <code>[Food]</code> within 3m of Cauldrons and Fermenters for rapid ingredient retrieval.</li>
          <li>Keep an active cooking fire burning under cauldrons to allow continuous recipe preparation.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Spit Burn Timing:</strong> Cook monitors roasting times; ensure path to cooking station is unobstructed so meats don't turn to charcoal.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 14</span>
    </div>
  </div>

  <!-- ==================== PAGE 15: HAULER ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part XII: The Hauler (Logistics Operator &amp; Packmaster)</h2>
        <span class="role-pill">Logistics &bull; Heavy Freight</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F33 🛒</span> <span class="feature-badge">F05 📦</span> <span class="feature-badge">F31 🗄️</span> <span class="feature-badge">F32 🏷️</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F33] 🛒 Vagon Cart Loading:</strong> Identifies nearby player wheeled carts (<code>Vagon</code>) and loads bulk timber and heavy ores into cart containers.</li>
          <li><strong>[F05] 📦 Bulk Pile Construction:</strong> Solves warehouse congestion by constructing physical resource stacks when all chests are full.</li>
          <li><strong>[F31] 🗄️ Camp Storage Consolidation:</strong> Sweeps scattered ground drops across the yard and condenses half-empty chests into unified stacks.</li>
          <li><strong>[F32] 🏷️ Sign-Based Distribution:</strong> Routes items to their designated warehouse bays according to wall signs.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Cart Towing &amp; Hauling:</strong> Physically grasping cart handles and pulling loaded wagons between distant outposts.</li>
          <li><strong>Portal Loading Relay:</strong> Loading non-teleportable metals onto docks while teleportable goods move through portals.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Park your Cart near warehouse gates; Haulers will automatically fill it with refined ingots before your mining trips.</li>
          <li>Construct wide 2m+ ramps and doorways to give Haulers ample turning radius when transporting goods.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Cart Physics:</strong> Park carts on flat leveled terrain so they don't roll away down hills while being loaded.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 15</span>
    </div>
  </div>

  <!-- ==================== PAGE 16: CRAFTSMAN ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part XIII: The Craftsman (Artificer &amp; Carpenter)</h2>
        <span class="role-pill">Workshop &bull; Manufacturing</span>
      </div>

      <div class="panel panel-now">
        <h3><span class="feature-badge">F30 🔧</span> <span class="feature-badge">F20 🔨</span> <span class="feature-badge">F31 🗄️</span> Current Core Capabilities</h3>
        <ul>
          <li><strong>[F30] 🔧 Workshop Fabrication:</strong> Assembles arrows, building pieces, and camp tools at upgraded Workbenches and Forges.</li>
          <li><strong>[F20] 🔨 Tool Durability Maintenance:</strong> Keeps settlement tool supply sharp and in working condition.</li>
          <li><strong>Material Requisition:</strong> Pulls wood, feathers, and flint from workshop storage chests to replenish arrow reserves.</li>
          <li><strong>[F32] 🏷️ Sign-Based Tag Routing:</strong> Returns fabricated items into marked output lockers.</li>
        </ul>
      </div>

      <div class="panel panel-future">
        <h3>Planned Future Expansions</h3>
        <ul>
          <li><strong>Palisade &amp; Wall Repair Rounds:</strong> Patrolling perimeter with hammer to repair raid damage to wooden stakes and gates.</li>
          <li><strong>Armor Forging:</strong> Crafting bronze and iron armor sets according to work order boards.</li>
        </ul>
      </div>

      <div class="panel panel-rec">
        <h3>Settlement Architect Recommendations</h3>
        <ul>
          <li>Build an enclosed smithy containing an upgraded Forge (with anvil, bellows, cooling bucket) and a Workbench.</li>
          <li>Supply a nearby chest labeled <code>[Wood]</code> and <code>[Flint]</code> to support high-volume arrow manufacturing.</li>
        </ul>
      </div>

      <div class="panel panel-warn">
        <h3>Known Quirks &amp; Troubleshooting</h3>
        <ul>
          <li><strong>Station Roof Requirements:</strong> Crafting stations require shelter (roof overhead) to function; build complete workshops.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 16</span>
    </div>
  </div>

  <!-- ==================== PAGE 17: LOGISTICS MASTERY ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part XIV: Logistics Mastery &amp; Base Infrastructure</h2>
        <span class="role-pill">Storage &amp; Social</span>
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
          <li><strong>[F34] ⛵ Longship Boarding:</strong> When following player aboard a Karve or Longship, workers automatically mount passenger benches.</li>
          <li><strong>[F35] 🏮 Night Torch Bearing:</strong> During nighttime (<code>EnvMan.IsNight()</code>) or inside dark dungeons, workers draw lit torches to guide paths.</li>
          <li><strong>[F36] 🪑 Tavern Seating &amp; Bed Rest:</strong> During off-duty idle cycles, workers visit settlement chairs, stools, and beds to socialize and rest.</li>
        </ul>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 17</span>
    </div>
  </div>

  <!-- ==================== PAGE 18: TROUBLESHOOTING & TELEMETRY ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Part XV: Diagnostics &amp; Console Command Reference</h2>
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

      <div class="panel panel-rec" style="margin-top: 6px;">
        <h3>Common Edge Cases &amp; Practical Solutions</h3>
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

  <!-- ==================== PAGE 19: SETTLEMENT ROSTER MATRIX ==================== -->
  <div class="page">
    <div>
      <div class="chapter-header">
        <h2>Appendix: Master Settlement Roster &amp; Specifications</h2>
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

      <div class="panel panel-rec" style="margin-top: 8px;">
        <h3>Settlement Expansion Blueprint: 10-Worker Village Formula</h3>
        <p>For an optimal self-sustaining Nordic stronghold, recruit: <strong>1 Lumberjack, 1 Miner, 1 Farmer, 1 Gatherer, 1 Hunter, 2 Guards (Gate &amp; Tower), 1 Smelter, 1 Base Tender, and 1 Cook</strong>. This configuration produces perpetual food, automated defense, warm hearths, and refined metal ingots without requiring player micromanagement.</p>
      </div>
    </div>

    <div class="page-footer">
      <span>The Settlement Steward's Codex</span>
      <span>ValheimWorkers v0.3.0</span>
      <span>Page 19</span>
    </div>
  </div>

</body>
</html>
"""

# Write HTML file
html_path = r"d:\valheim_mods\ValheimWorkers_Guide_Book.html"
with open(html_path, "w", encoding="utf-8") as f:
    f.write(html_content)

print(f"Successfully generated {html_path} ({len(html_content)} bytes)")
