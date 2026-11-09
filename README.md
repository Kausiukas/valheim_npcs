<div align="center">

# ⚔️ VALHEIM WORKERS ⚔️
### The Settlement Steward's Autonomous AI Engine & Domain Codex

[![Valheim](https://img.shields.io/badge/Valheim-v0.218.21+-e2b76e?style=for-the-badge&logo=steam&logoColor=white)](https://store.steampowered.com/app/892970/Valheim/)
[![BepInEx](https://img.shields.io/badge/BepInEx-5.4.2202+-c8683e?style=for-the-badge)](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
[![Version](https://img.shields.io/badge/Release-v0.3.0-55c9ea?style=for-the-badge)](https://github.com/Kausiukas/valheim_npcs/releases)
[![Lineages](https://img.shields.io/badge/Lineages-Vikings%20%E2%80%A2%20Dvergr%20%E2%80%A2%20Skeletons-38b000?style=for-the-badge)](#-asymmetric-lineages-relics--progression)
[![Roles](https://img.shields.io/badge/Autonomous%20Roles-11%20Specialists-f39c12?style=for-the-badge)](#-the-10-specialist-domains--visual-codex)

<br/>

<img src="docs/pages/page_01.png" alt="ValheimWorkers Settlement Steward's Codex" width="750" style="border-radius: 8px; box-shadow: 0 10px 30px rgba(0,0,0,0.6);"/>

<p align="center">
  <b>Transform quiet wilderness into bustling, self-sustaining strongholds.</b><br/>
  Featuring autonomous tool maintenance, smart Viking self-defense, multi-resource logistics, 3 asymmetric lineages, and etched runestone documentation.
</p>

[🎮 Features](#-key-features) • [📜 Visual Codex](#-the-10-specialist-domains--visual-codex) • [💀 Lineages & Relics](#-asymmetric-lineages-relics--progression) • [🖥️ In-Game UI](#-in-game-user-interfaces) • [⌨️ Controls](#-controls--hotkeys) • [🛠️ Build Guide](#️-recompiling--deploying)

---

</div>

## 🌐 Interactive Web Suite & Documentation Portal

This repository includes a full interactive documentation suite that can be launched directly in your browser:

* **[📖 The Settlement Steward's Codex (HTML Edition)](docs/codex/ValheimWorkers_Guide_Book.html)** — Full 22-page runestone compendium.
* **[📜 Codex Printable PDF (14.8 MB)](docs/codex/ValheimWorkers_Guide_Book.pdf)** — High-res vector blueprints and schematics.
* **[🗺️ Interactive 36-Feature Capability Matrix](docs/interactive/worker_feature_map.html)** — Live searchable matrix with 9 vector diagrams.
* **[🛡️ NPC Character Sheet & Skill Tree Prototype](docs/interactive/npc_character_menu_preview.html)** — Live interactive UI prototype.
* **[🧪 In-Game Manual QA Checklist & Telemetry](docs/interactive/manual_verification_checklist.html)** — Verification suite and test telemetry.
* **[🏛️ Technical Architecture Specification](docs/ARCHITECTURE.md)** — Deep dive into AI state machines, ZDO keys, and Harmony patches.
* **[🗺️ Phased Development Roadmap](docs/ROADMAP.md)** — Milestones v0.1.0 through v1.0.0 and Definitions of Done.

---

## 🎮 Key Features

* **3 Asymmetric Lineages:**
  * 🛡️ **Human Vikings:** 4 settlement tiers from Meadows Lost Souls to Mead-Hall Einherjar, grinding Task XP across a 4-Branch Skill Tree (*Woodsman, Hearth, Ranger, Shield-Wall*).
  * 💀 **Skeleton Servants:** Unlocked via Skarde's Sacrificial Dagger and Valka's Blood Chalice. Absorbs combat marrow blood to evolve across Forms I–IV (*Thrall $\rightarrow$ Sinew-Woven $\rightarrow$ Draugr-Grafted $\rightarrow$ Crimson Revenant Lord*). Immune to pride decay.
  * ⛏️ **Dvergr Laborers:** Rescued drudges with high baseline carry weight and mining speed, certified through gold coins, refined bronze, and gems via the Luxury Patronage tree.
* **11 Autonomous Specialist Roles:** `Lumberjack`, `Miner`, `Farmer`, `Rancher`, `Smelter`, `Cook`, `Craftsman`, `Fisher`, `Hauler`, `Sentry/Guard`, and `Base Tender`.
* **Smart Viking Self-Defense & Threat Evaluation:** Workers never stand idle while attacked. They evaluate enemy power vs. friendly allies within 18m, auto-equip combat gear to slay manageable foes, or sound tactical retreats from apex monsters (Trolls, Abominations, Bears).
* **Autonomous Covered Workbench Repair Loop:** When tool durability degrades below 35%, workers path to the nearest covered Workbench/Forge, restore durability to 100%, and resume chores.
* **Sign-Bracket Logistics & Smart Sorting:** Tag chests with `[Wood]`, `[Ore]`, `[Produce]`, or `[Seed]`. Workers prioritize existing matching stacks before occupying empty slots and construct physical 50x resource piles when storage overflows.
* **Live World Map (`M`) & Minimap Pins:** Dynamic pins display real-time positions, names, assigned roles, and combat status tags (`[Retreating!]`, `[Missing Tool]`).

---

## 📜 The 10 Specialist Domains & Visual Codex

The settlement economy operates on a circular resource web where raw harvests flow seamlessly into refining, crafting, and defense:

<div align="center">
  <img src="docs/pages/page_03.png" alt="Settlement Macro Production Web" width="49%"/>
  <img src="docs/pages/page_02.png" alt="Settlement Operating Architecture" width="49%"/>
</div>

<details>
<summary><b>🌲 Forestry & Arborist (Part I • Lumberjack) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_04.png" alt="Lumberjack Role Specification" width="49%"/>
  <img src="docs/pages/page_05.png" alt="Forestry Schema 1" width="49%"/>
</div>

* **Precision Felling:** Raycasts clear felling arcs into open meadows, preventing timber damage to citadel roofs.
* **100% Stump Removal:** Excises root taproots so new nursery saplings can root on clean ground.
* **Sustainable Arbor Nursery:** Replants Beech, Birch, and Fir cones at an exact 3.0m equidistant grid.
</details>

<details>
<summary><b>⛏️ Subsurface Quarrying (Part II • Miner) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_06.png" alt="Miner Role Specification" width="49%"/>
  <img src="docs/pages/page_07.png" alt="Quarrying Schema 2" width="49%"/>
</div>

* **Terraced Bench Clearance:** Excavates surface boulders, copper veins, and subterranean deposits.
* **26° Transit Ramps:** Planes smooth paths with the Hoe so heavy ore carts can ascend without rollback.
* **Bedrock Safety Floor:** Digging automatically halts 0.5m above the world bedrock floor.
</details>

<details>
<summary><b>🌾 Agriculture & Botanical Nursery (Part III • Farmer) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_08.png" alt="Farmer Role Specification" width="49%"/>
  <img src="docs/pages/page_09.png" alt="Agriculture Schema 3" width="49%"/>
</div>

* **1.2m Crop Grid Spacing:** Eliminates crop overcrowding and prevents plant withering.
* **50/50 Seed Cycle:** Replants half of harvested crops back into seed flowers to guarantee perpetual abundance.
* **Tamed Beast Nutrition:** Proactively feeds hungry tamed Boars, Wolves, and Lox from camp fodder reserves.
</details>

<details>
<summary><b>🥩 Animal Husbandry & Ranching (Part IV • Rancher) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_10.png" alt="Rancher Role Specification" width="60%"/>
</div>

* **Herd Tending:** Maintains pen fences, collects dropped feathers and hides, and harvests honey from beehives within 30m.
</details>

<details>
<summary><b>🔥 Metallurgy & Smelting (Part V • Smelter) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_11.png" alt="Smelter Role Specification" width="49%"/>
  <img src="docs/pages/page_12.png" alt="Metallurgy Schema 4" width="49%"/>
</div>

* **Charcoal Pyrolysis:** Keeps Kilns continuously loaded while respecting a configurable minimum wood reserve.
* **1:2 Stoichiometric Charging:** Balances ore hoppers and coal combustion chambers without stalling.
* **Ingot Consolidation:** Extracts refined copper, tin, bronze, iron, and silver bars into secure foundry vaults.
</details>

<details>
<summary><b>🍲 Culinary & Fermentation (Part VI • Cook) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_13.png" alt="Cook Role Specification" width="49%"/>
  <img src="docs/pages/page_14.png" alt="Culinary Schema 5" width="49%"/>
</div>

* **Zero-Burn Spits:** Mounts meats on roasting spits and pulls cooked food at 20 seconds before it turns to coal.
* **Cauldron Stews & Ovens:** Bakes hearty settlement pies, breads, and stews from pantry larders.
* **Fermenter Cellar:** Taps aged mead casks and routes resistance meads into medical chests.
</details>

<details>
<summary><b>🔨 Fortress Architecture & Armory (Part VII • Craftsman) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_15.png" alt="Craftsman Role Specification" width="60%"/>
</div>

* **Zero-Cost Tool Restoration:** Services damaged tools for all settlement workers at covered workbenches.
* **Fortress Repair Patrol:** Re-hammers weather-damaged roofs and breached palisades back to 100% HP.
</details>

<details>
<summary><b>📦 Vascular Freight & Hauling (Part IX • Hauler) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_17.png" alt="Hauler Role Specification" width="49%"/>
  <img src="docs/pages/page_18.png" alt="Logistics Schema 6" width="49%"/>
</div>

* **25m Ground Vacuum:** Sweeps loose ground debris and resource drops to maintain camp cleanliness and high FPS.
* **1000kg Cart Freight:** Pulls loaded 2-wheel wagons along stone paths, replacing up to 6 manual walking trips.
</details>

<details>
<summary><b>🏹 Citadel Defense & Sentry Patrol (Part X • Sentry) — [Click to Expand Blueprint]</b></summary>
<br/>

<div align="center">
  <img src="docs/pages/page_19.png" alt="Sentry Role Specification" width="49%"/>
  <img src="docs/pages/page_20.png" alt="Defense Schema 7" width="49%"/>
</div>

* **40m Elevated Sightlines:** Stationed on watchtowers to spot Greydwarf raids and troll incursions early.
* **Ballistic Arc Archery:** Calculates arrow gravity drop and lead timing against moving targets.
</details>

---

## 💀 Asymmetric Lineages, Relics & Progression

```mermaid
flowchart TD
    Start["Survival World Start\n(Humans: Tier 1 Available | Skeletons & Dvergr: Locked 🔒)"]

    subgraph SKELETON ["💀 Skeleton Worker System"]
        T1["⚰️ Barrow of Skarde\n(Burial Crypt)"] --> Dagger["🗡️ Sacrificial Dagger\n(One-shots tamed beasts)"]
        Dagger --> Sacrifice["Boar-over-Bones Sacrifice\n(Drop 10x Bones + Kill Boar)"]
        Sacrifice --> SkelThrall["Raise Form I: Rattling Thrall"]
        
        T2["🩸 Crypt of Valka\n(Stone Ship Barrow)"] --> Chalice["🏆 Blood Chalice of Valka\n(Stores up to 100 Blood)"]
        Chalice --> ThrowBlood["Feed Marrow Blood to Workers"]
        
        CombatFeed["Combat Blood-Feeding\n(+0.5 Blood on hit + 2 HP heal\n+2..6 Blood on kill)"] --> MarrowRes["Marrow Blood Reservoir"]
        MarrowRes --> EvolForms["Evolve Forms I - IV\n(Sinew-Woven -> Draugr-Grafted -> Revenant Lord)"]
    end

    subgraph HUMAN ["🛡️ Human (Viking) System"]
        LostSoul["Tier 1: Lost Soul\n(Campfire offering)"] --> GrindXP["Task XP Grinding\n(+5 to +25 XP per chore)"]
        GrindXP --> HumanTree["4-Branch Skill Tree\n(Woodsman, Hearth, Ranger, Shield-Wall)"]
        
        RuinRescue["Tier 2: Rescued Homesteader"] --> FarmCook["Unlocks Lumberjack, Farmer, Cook"]
        StandingStone["Tier 3: Standing Stone Drengr"] --> HunterMiner["Unlocks Ballistic Hunter & Miner"]
        Einherjar["Tier 4: Mead-Hall Einherjar"] --> GuardMaster["Unlocks Raid Guard & Quartermaster"]
    end

    subgraph DVERGR ["⛏️ Dvergr Guild System"]
        Beacon["Tier 1: Stranded Drudge\n(Repair Broken Survey Beacon)"] --> Drudge["Heavy Hauler & Stone Collector"]
        Brewing["Tier 2: Deep Prospector\n(Brew Fermenter Mead + Bronze)"] --> Miner["Unlocks Master Ore Miner"]
        ThreeFires["Tier 3: Guild Forge-Smith\n(Active Kiln + Smelter + Forge)"] --> SmelterSmith["Unlocks Smelting Master"]
        TrollCave["Tier 4: Ward-Sentinel\n(Recover Guild Seal from Troll Cave)"] --> Sentinel["Unlocks Armored Ward-Sentinel"]
        
        Luxury["Luxury Patronage Tree\n(Coins, Refined Ores, Amber, Rubies)"] --> Certs["Master Guild Certifications\n& 'Wealthy & Smug' Pride Immunity"]
    end
```

---

## 🖥️ In-Game User Interfaces

ValheimWorkers provides two distinct interfaces:

### 1. The `F8` Settlement Hall Manager
* **Dual-Corner Responsive Resizing:** Drag either the **bottom-left (`◣`)** or **bottom-right (`◢`)** gold corner grips, or window edges, to resize without losing mouse grip.
* **Top Bar Presets:** Quick-switch between `Compact` (780x580), `Standard` (980x720), and `MaxFit` (fullscreen fit) alongside `A-` / `A+` font scaling.
* **Tab 1 (Hire & Customize):** Name, body model (`Viking Male`, `Viking Female`, `Dvergr`, `Skeleton`), hair, beard, audio voice profiles, starting gear, and initial role orders.
* **Tab 2 (Active Roster & Map Tracking):** Live list of all active realm workers, current status, distance, health, and a **"Locate on Map (M)"** button.
* **Tab 3 (Proving Grounds & Sandbox):** 1-click test scenarios (`Blood Sacrifice`, `Lumberjack`, `Mining`, `Troll Threat & Retreat`, `Cooking`, `Farming`, `Smelter`).

### 2. Player-Style Character Sheet & Skills Drawer (`Hold [E]`)
* **Hold `[E]` on any worker:** Opens the native Valheim `8x4` container grid alongside the **Worker Vitals Drawer**.
* **Visual Paperdoll:** 8 dedicated slots (Helmet, Chest, Legs, Main Hand, Shield, Cape, Bow, Ammo). Right-click items to equip/unequip with native cyan frames.
* **Valheim Activity Skills (`0–100`):** Tracks *Woodcutting, Mining, Melee Combat, Blocking, Archery, Farming, Cooking, and Athletics*.
* **Heritage Skill Tree Graph:** Interactive visual node graph for Human, Skeleton, and Dvergr advancement.

---

## ⌨️ Controls & Hotkeys

| Key | Action |
| :--- | :--- |
| **`F8`** | **Toggle NPC Lab & Settlement Manager UI** (unlocks cursor, pauses weapon attacks) |
| **`F7`** | Quick-spawn default worker in front of player |
| **`F5`** | Open Valheim developer console |
| **Aim at Worker + `[E]`** | Toggle between **Follow Me** and **Stay at Post** |
| **Aim at Worker + `[Shift + E]`** | Cycle worker role in-world with speech bubbles |
| **Aim at Worker + `[Hold E]`** | Open native Valheim `8x4` container inventory grid + Vitals Drawer |
| **Aim at Worker + `[Alt + Hold E]`** | Open `F8` Settlement Hall inspector directly for this worker |

---

## 🛠️ Recompiling & Deploying

The project uses Visual Studio 2022's `MSBuild.exe` and targets `.NET Framework 4.7.2`:

```powershell
powershell.exe -ExecutionPolicy Bypass -File "build.ps1"
```

The build script automatically detects your Visual Studio installation, compiles the solution, and deploys the plugin DLL to your active r2modman profile:
```
%APPDATA%\r2modmanPlus-local\Valheim\profiles\Default\BepInEx\plugins\ValheimWorkers\ValheimWorkers.dll
```

---

## 📄 License & Attribution

* Built with [BepInEx](https://github.com/BepInEx/BepInEx) and [Harmony](https://github.com/pardeike/Harmony).
* Developed for Valheim by **Kausiukas**.
