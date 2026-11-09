# ValheimWorkers — Code Review

**Date:** 2026-11-05
**Scope:** all source files in `ValheimWorkers/` (~13k lines), the project/build files, the README and ROADMAP, and the telemetry artifacts.
**Method:** static review only. Nothing was built or run.

Paths are relative to `ValheimWorkers/`, and line numbers refer to the files as they were at review time.

## Summary

Single-player mostly works. Three problem areas stand out:

1. **Item duplication exploits.** The worker creates gear and ammo from nothing, and players can take it.
2. **Multiplayer ownership is ignored.** Workers and the UI change ZDOs, containers and item drops that the local client doesn't own.
3. **Developer tooling ships to every player.** That includes forced cheats, a local HTTP server, hardcoded personal paths, and disk writes every frame or two.

Severity scale:
- 🔴 Critical: exploit, data loss, or security problem
- 🟠 High: a feature is broken or the game gets stuck
- 🟡 Medium: performance or quality

---

## 🔴 Critical

### C1. Free gear and infinite item duplication
- **Where:**
  - `AI/WorkerBrain.cs:887-962` (`EnsureEquippedInInventory`)
  - `:1040-1080` (`EnsureAmmoEquipped`)
  - `:1157-1200` (`UpdateCustomizationFromCurrentEquipment`)
  - `:3168-3239` (`HandleUseOrReceiveItem`)
  - `UI/NpcLabGui.cs:566-610`
- **Problem:**
  - `EnsureEquippedInInventory` calls `m_humanoid.PickupPrefab(itemPrefab, 1, false)` whenever `Customization.Weapon/Shield/Helmet/Chest/Legs` names an item the worker doesn't have. That creates the item out of nothing.
  - **Giving a weapon or armor** to a worker only sets `Customization.Weapon = prefabName`. The item is never removed from the player, so the player keeps the original and the worker gets a new copy.
  - **Taking gear out** of the worker's inventory doesn't clear the customization slot, because `UpdateCustomizationFromCurrentEquipment` only writes names that aren't empty. The next `OnInventoryChanged` → `ApplyVisualEquipment` creates the item again. This can be farmed forever.
  - The **F8 Loadout tab** (`UpdateLiveLoadout`) gives any tier of gear for free. Players can unequip it in the grid and right-click it into their own inventory (`TakeCarriedItemToPlayer`).
  - `EnsureAmmoEquipped` gives **50 free `ArrowWood`** whenever the worker has no ammo.
  - The **hire cost is never charged.** `HireCostItem`/`HireCostAmount` are defined in `Core/WorkerCatalog.cs:26,58,90` but nothing reads them.
- **Fix:**
  - Workers only equip items that are actually in their inventory. The give action moves the real `ItemData` from the player to the worker.
  - Clear the customization slot when that gear leaves the inventory.
  - Remove item creation from the visual-sync path.
  - Charge the hire cost in `SpawnCustomWorker` (or wherever hiring happens).
  - If you want a sandbox mode, put it behind a cheat flag.

### C2. Cheats for every player; config ignored
- **Where:**
  - `Patches/ConsolePatches.cs:17`
  - `ValheimWorkersPlugin.cs:41, 66-75`
  - The F8 spawn and Proving Grounds actions
- **Problem:**
  - `Terminal.m_cheat = true` is set every time, so any client with the mod gets `devcommands` (god mode, spawn, fly) on any server, bypassing the admin check.
  - The `AutoEnableConsole` config is never read.
  - The `workers_*` commands are registered without `isCheat`.
  - F7, F8 spawning, healing and Troll spawning have no admin or host check.
- **Fix:**
  - Make the cheat override depend on the config, and default it to `false`.
  - Register the commands with `isCheat: true`.
  - Check `ZNet.instance.LocalPlayerIsAdminOrHost()` (or the equivalent) before spawn, heal, edit or test actions.

### C3. Telemetry and HTTP server ship in release builds
- **Where:** `Core/VerificationTelemetry.cs`, especially lines 168-173 (paths), ~983 (listener), 1053 (reset), 1141-1144 (CORS) and 734-772 (flush).
- **Problem:**
  - **Hardcoded personal paths:** `C:\Users\OCPC\.gemini\antigravity\brain\...`, `D:\valheim_mods\ValheimWorkers` and `D:\valheim_mods`.
  - **The listener is always on:** an HTTP server binds `127.0.0.1:18788`.
    - It sends `Access-Control-Allow-Origin: *` and `Access-Control-Allow-Private-Network: true`, so any website a player visits can read world positions, worker data and stack traces.
    - `/api/reset` accepts any method, including a plain GET.
    - The thread pool is unbounded and request lines have no length limit.
    - `Shutdown` never joins the listener thread.
  - **File writes on the main thread:** `RebuildAndFlushSnapshot` writes 2 files to up to 3 directories, synchronously, every 1–2 s.
    - It calls `FindObjectsOfType<WorkerBrain>()` while holding the lock.
    - Any exception, from any mod, sets `s_forceImmediateFlush`, so an exception spam turns into a write every frame.
  - **`RecordPass` spam:** it runs every frame from `MoveTo`, combat, `OnInventoryChanged`, `GetEffectiveResources` and OnGUI. The current `verification_telemetry.json` shows `locomotion_slope_direct_drive` with 10,605 passes and `gather_known_filtering` with 9,452. Each call takes the lock, formats strings and calls `DateTime.Now.ToString()`.
  - **Stale workers:** after logout the dashboard falls back to `s_workerSnapshots`, so workers from the previous world still show.
- **Fix:**
  - Put the whole subsystem behind `#if DEBUG` or a `DevMode` config that defaults to `false`.
  - Write under `BepInEx.Paths.ConfigPath`.
  - Build the JSON on the main thread and write it on a background thread, at about 1 Hz.
  - `RecordPass` should return early once a check has passed.
  - Remove the wildcard CORS, require a random token, and accept reset only as POST.

### C4. Dismissed or deleted workers come back
- **Where:** `AI/WorkerBrain.cs:1609`, `UI/NpcLabGui.cs:1398`, `Patches/ConsolePatches.cs:516`
- **Problem:** these use `UnityEngine.Object.Destroy(gameObject)` on a networked object. The ZDO survives, so the worker reappears when the zone reloads, and other clients never see it removed.
- **Fix:** take ownership if needed, then call `ZNetScene.instance.Destroy(gameObject)` (or `m_nview.Destroy()`).

### C5. Multiplayer ownership is ignored
- **Problem:**
  - **The worker inventory UI skips vanilla locking.** `Patches/InventoryGuiPatches.cs:16` forces `Container.IsOwner` to return `true`, and Hold-E calls `InventoryGui.instance.Show(m_container)` directly (`WorkerBrain.cs:3005`). That skips the `RequestOpen` / in-use / ownership-transfer handshake.
    - Two players can open the same worker at once.
    - A client that doesn't own the worker edits and saves its local copy, and the owner's state then overwrites it. Items get duplicated or lost.
  - **Chest transfers on chests the client doesn't own** (`chestInv.AddItem/RemoveItem`):
    - `GathererTask.cs:1264-1265, 1290-1291`
    - `FarmerTask.cs:651-652`
    - `CookTask.cs:324-326, 556-557`
    - `BaseTenderTask.cs:389-390`
    - `HaulerTask.cs:355-356`

    Deposits disappear and withdrawals duplicate. `IsInUse()` only reflects the local client.
  - **Pickups of drops the client doesn't own:** `GathererTask.cs:841`, `FarmerTask.cs:316`, `CookTask.cs:443`, `HaulerTask.cs:209`. `ZNetScene.Destroy` doesn't remove the drop for other players, so it duplicates.
  - **`WorkerBrain.Update()` has no owner check.** It runs `UpdateVitalityAndFood`, so every client eats food from its local inventory copy and heals the worker. The first-frame `ApplyVisualEquipment` also runs, and through C1 it creates items on every client.
  - **Edits on workers the client doesn't own are silently lost.** `SaveToZDO` returns early on non-owners (`WorkerBrain.cs:474`), but the GUI still shows the change as applied.
  - **`Player.m_localPlayer` is treated as "the player".**
    - In Follow (`:2434`), the worker follows whoever owns its ZDO, not the player who gave the order.
    - Threat power (`:2349`) and `ResourceCatalog.IsKnownByPlayer` behave differently depending on which client owns the worker. On a dedicated server, where there is no local player, they behave differently again.
- **Fix:**
  - Use the vanilla container open flow.
  - Run all simulation and inventory changes on the owner only.
  - Send UI commands to the owner with `ZRoutedRpc`.
  - For chests and drops: claim or request ownership, retry on a later tick, and only transfer while the client owns them.
  - Store the commanding player's ID in the worker's ZDO.

### C6. Base Tender destroys fuel and ore
- **Where:** `AI/BaseTenderTask.cs:441-442, 459-460, 468-469, 477-478`
- **Problem:**
  - It removes 1 item from the worker, then calls `InvokeRPC("AddFuel")` / `InvokeRPC("AddOre")`.
  - Current vanilla registers `"RPC_AddFuel"` / `"RPC_AddOre"`, and `RPC_AddOre` takes the ore name as a `string`. **Check this against your game version.** If the names are wrong, nothing is added and the item is gone.
  - Because fuel never goes up, the same target is picked again, and the tender slowly empties the chests.
- **Fix:**
  - Use the vanilla paths: `Fireplace.UseItem(humanoid, item)`, and Smelter's `OnAddOre`/`OnAddFuel` through the `m_addOreSwitch`/`m_addWoodSwitch` switches.
  - Only remove the item once the add has been accepted.

---

## 🟠 High

### H1. Worker prefabs are missing after the second world load
- **Where:** `Prefabs/WorkerPrefabManager.cs:116-121, 13`
- **Problem:** `_registeredPrefabs` is static and survives a logout to the main menu. On the next world load, the new `ZNetScene.Awake` skips every archetype because it's "already registered", so the new scene never gets the prefabs. Spawning fails and saved workers can't load.
- **Fix:** keep a `Dictionary<string, GameObject>` of the cloned prefabs, and add them to every new scene's `m_prefabs` / `m_namedPrefabs` when the scene doesn't already have them.

### H2. Workers cloned from `Player` may not be saved
- **Where:** `Prefabs/WorkerPrefabManager.cs:46-108`
- **Problem:** the vanilla `Player` prefab's `ZNetView` is normally non-persistent; this hasn't been checked against your game files. The clone never sets `m_persistent = true`. If it isn't persistent, workers aren't saved to the world file, and they're cleaned up as orphans when their owning client disconnects.
- **Fix:** set `clone.GetComponent<ZNetView>().m_persistent = true` explicitly, and check the other `ZNetView` flags.

### H3. Archetype is never stored
- **Where:** `Prefabs/WorkerPrefabManager.cs:236-295`, `AI/WorkerBrain.cs:239-241, 460-461, 483-486`
- **Problem:** `SpawnCustomWorker` never sets `brain.Archetype`, so `LoadFromZDO` and `Start` fall back to `Worker_Viking`, and that gets saved. A Skeleton Servant then cycles through all ten roles instead of its three `AllowedRoles`.
- **Fix:** look up `WorkerCatalog.Get(config.BasePrefab)` and assign it before `ApplyCustomization`. Also, `Archetype` should have a public or internal setter.

### H4. No death handling
- **Problem:** nothing handles worker death. A non-player `Humanoid` doesn't drop its inventory, so everything the player gave the worker is lost.
- **Fix:** on death (as the owner), drop the inventory or spawn a tombstone-style container.

### H5. Farmer logic
- **Where:** `AI/FarmerTask.cs:39, 336-374, 465`
- **Problem:**
  - It calls the private `Plant.Grow()` directly. That isn't owner-checked, and it then replants on the same spot, so crops pile up and seeds get used up.
  - It replants even when `CanBePicked()` was false.
  - Anything with "Mushroom" in the name, and wild seed patches, count as crops. `ExtractCropType` falls back to `"Carrot"`, so a carrot gets planted on uncultivated ground.
  - A fallback places a mature `Pickable_X` for one seed, which is free crops.
- **Fix:**
  - Harvest only `Pickable`s and never call `Grow()`.
  - Use an explicit map from crop pickable to sapling.
  - Replant only after a successful pick, on cultivated ground (`Heightmap.IsCultivated`), with no `Plant` or `Pickable` within about 0.5 m.

### H6. Gatherer double damage and unintended hits
- **Where:** `AI/GathererTask.cs:1006-1036`
- **Problem:** `StartAttack(null)` plays a real weapon swing, and then `dest.Damage(hit)` is applied by hand on top of it. That's double damage. The real swing can also hit players, other workers or building pieces in its arc.
- **Fix:** use either the vanilla attack or a manual hit with an animation only, not both.

### H7. Cook logic
- **Where:** `AI/CookTask.cs:35, 40, 163-167, 393-456`; `AI/GathererTask.cs:182-186`
- **Problem:**
  - `FindRawMeatInInventory` returns meat the station can't take (Lox, Wolf, Hare and Chicken need the iron cooking station), so `UseItem` fails forever.
  - `"CookedFish"` should be `FishCooked`.
  - `DeerMeat` is missing from the Hunter's collection list.
  - The cook gets stuck on cooked food lying on the ground that it can't reach or pick up. The sweep step only searches 8 m around the station, and it ignores `_targetCookedDrop`.
- **Fix:**
  - Use `station.IsItemAllowed(...)` and the station's `m_conversion` lists.
  - Move to the target drop itself.
  - Add a timeout and blacklist.

### H8. BaseTender kiln detection
- **Where:** `AI/BaseTenderTask.cs:213-214, 175-189, 257`
- **Problem:**
  - Any `Smelter` with no fuel item counts as a kiln, which includes the windmill and the spinning wheel. Those get loaded with Wood.
  - A fireplace with no wood available is always picked first, so smelters are never served.
  - Only 1 unit is used per trip, even though more were withdrawn.
  - Repair accepts any tool.
- **Fix:** require a `m_conversion` entry whose input is Wood, add fair rotation between targets, and use up everything that was withdrawn.

### H9. Stuck states
- **Problem:**
  - None of the `MovingTo*` states time out, and unreachable targets are never blacklisted. A rock on a cliff, a deer that runs off with no leash (`GathererTask.cs:1144/1161`), or a chest it can't reach leaves the worker stuck forever.
  - `IsInventoryFull` counts protected gear and seeds. Gatherer and Farmer then loop between returning and depositing, running `OverlapSphere` almost every frame.
  - Hauler picks up any drop, including weapons and armor (`HaulerTask.cs:152`), which are protected and never deposited. It also takes gear players dropped on purpose.
- **Fix:**
  - Give each target a timeout, and blacklist failed targets by instance or ZDO ID for N seconds.
  - Base the "inventory full" check on items that aren't protected.
  - Hauler skips equipment.

### H10. Global `MaterialMan.Update` replacement
- **Where:** `Patches/MaterialManPatch.cs`
- **Problem:**
  - The patch replaces a vanilla rendering loop for the whole game.
  - It calls `GetMethod` + `Invoke` by reflection for every container, every frame.
  - It swallows all exceptions.
  - The NRE spam it hides is probably caused by this mod: `ApplyVisualEquipment` destroys `VisEquipment` item instances directly (`WorkerBrain.cs:803-844`).
- **Fix:** stop destroying the instances by hand and let `VisEquipment` handle slot changes. Then remove the patch. If it has to stay, cache a delegate and log at least the first exception.

---

## 🟡 Medium

### M1. Performance
- **`Physics.OverlapSphere` + `GetComponentInParent` runs every frame** in:
  - `HaulerTask.cs:94` (`ShouldDepositNow`) and `:228`
  - `CookTask.cs:285`
  - `BaseTenderTask.cs:326`
  - `FarmerTask.cs:576`
  - `GathererTask.cs:1169`
  - `GathererTask.cs:785` (`GetComponentsInChildren` while moving)
- **`FindNearestMainBaseAnchor`** (`WorkerBrain.cs:2027`) does a 100 m `OverlapSphere` and returns the *first* match, not the nearest.
- **`Traverse.Create(...)` reflection** runs on every equipment access (`WorkerBrain.cs`, throughout) and per station on every scan (Cook:184-193, Farmer:208, Gatherer:496).
- **`MoveTo` raycasts** have no layer mask, so small drops and characters trigger hop-assist jumps.
- **Fix:**
  - Use `OverlapSphereNonAlloc` with layer masks.
  - Build a shared scan cache per work area, refreshed every 1–2 s.
  - Use cached `AccessTools.FieldRef` delegates or a publicized `assembly_valheim`.

### M2. Log spam
- `LogAction` only skips a message identical to the previous one. Messages that contain distances log every frame, for example:
  - `GuardTask.cs:119`
  - `BaseTenderTask.cs:296`
  - `HaulerTask.cs:180`
  - `WorkerBrain.cs:1789, 2442`
- `VerboseConsoleLog` defaults to `true`.
- **Fix:** log when the state changes, not every tick. Make verbose logging a config option that defaults to off.

### M3. IMGUI issues
- **Layout errors:** `NpcLabGui.cs:861, 989, 1870` `return` from inside `BeginHorizontal` or scroll groups, which throws mismatched-layout errors.
- **Spam on every UI event:**
  - The radius slider calls `SetWorkRadius` → `SaveToZDO` + `StartCommandTrace` on every event (`:894-898, 1833-1837`).
  - The name `TextField` saves on every keystroke (`:1121-1127`).
- **Textures:** `ValheimGuiTheme.cs:154-286` doesn't set `HideFlags.HideAndDontSave`, so `UnloadUnusedAssets` can destroy them. `EnsureInitialized` only checks one texture.
- **Glyphs:** emoji and symbol glyphs (🗗 🗖 ⛶ 🔒 🔄 ⚠ ✓) probably render as boxes in Valheim's fonts.
- **Fix:** keep OnGUI read-only, queue actions and apply them in `Update`, and apply edits on mouse-up or after a short delay.

### M4. Customization not applied
- `SpeedModifier` is saved per worker but never applied. Only the prefab's `m_speed` is scaled, not `m_runSpeed` or `m_walkSpeed`.
- Assigning `m_hitEffects` and the other effect lists shares the template prefab's `EffectList` instances, which is fine as long as nothing changes them.

### M5. Container and creature checks
- `GetComponentInParent<Container>()` also matches tombstones, carts and ships, and it ignores `CheckAccess` on private chests.
- Hunting doesn't check `IsTamed()`, so tamed animals can be killed.
- Using `??` on a `UnityEngine.Object` skips Unity's null check (Farmer:629, Cook:534, Hauler:275, InteractionPatches).
- `m_health <= 0` checks (`GathererTask.cs:1056-1062`) are dead code, because `m_health` is max health.

### M6. Patch hygiene
- `DurabilityPatches` already blocks `DrainEquipedItemDurability`, so the Attack and Block postfixes probably add nothing.
- `StoreGui.IsVisible → true` is a fragile way to block input.
- `InventoryGuiUpdateContainerPatch` forces `m_autoCloseDistance = 4f` on every container, which overrides other mods.
- `WorkerBrain.OnDisable` checks `this == null`, which can never be true inside `OnDisable`.

### M7. Smaller items
- Withdrawals use `AddItem(m_dropPrefab, n)`, which creates new items and loses quality and variant. A null prefab throws after the remove has already happened (`BaseTenderTask.cs:390`).
- `GathererTask.cs:1230` searches around `HomePosition` even when the worker has no home.
- Speech bubbles (`Chat.SetNpcText`) only show on the owner's client.
- `IsBowName` matches by substring (`"Bow"`). Use `m_itemType` instead.

### M8. Docs drift (README / ROADMAP)

| Claim in docs | What the code does |
| --- | --- |
| Radius 5–100 m | 5–150 m |
| "Unlock All (Sandbox)" toggle | Doesn't exist |
| Foraging includes Dandelion / Cloudberry | Not in the UI |
| Hire orders include `Follow` | Not offered; Farmer and Cook are offered but not documented |
| Roster shows health | It doesn't |
| Troll spawns 16 m away | 22 m |
| ROADMAP: "Current v0.1.0", Farming and Cooking unchecked | Plugin is 0.3.0, and Farmer and Cook are implemented |

The docs also don't mention the HTTP bridge, the disk writes, the forced devcommands, or hire costs.

---

## Checked and ruled out

- **Carried items do survive a reload.** `EnsureInventoryLink` points the Humanoid's `m_inventory` at the `Container`'s inventory, and `Container` saves it to the ZDO. The multiplayer caveats in C5 still apply.
- **No thread-safety bug in the HTTP threads.** They only touch cached JSON under `s_lock` and never call the Unity API. The flags read on the main thread should still be `volatile`.

---

## Recommendations

1. **Use version control.** The workspace isn't a git repository. Commit before starting these fixes.
2. **Split developer tooling from the release build.** Telemetry, Proving Grounds, `workers_test_*`, the cheat override and verbose logging go behind `#if DEBUG` or a `DevMode` config that defaults to off. The release DLL then has no listener, no file writes and no cheats.
3. **One world-interaction layer.** Every chest transfer, pickup, fuel or ore load and planting goes through one helper that:
   - checks ownership, or claims it and retries;
   - adds to the destination, confirms, and only then removes from the source;
   - moves real `ItemData`, so quality and variant are kept;
   - returns success or failure.

   That fixes C5, C6 and most item loss.
4. **Owner-authoritative simulation and RPC commands.** The AI, vitality and inventory changes run only on the owner. UI and console commands go to the owner via `ZRoutedRpc`, behind a single host or admin check.
5. **Real economy.** Charge the hire cost. Workers only use gear the player supplies, and nothing is created implicitly.
6. **Break up `WorkerBrain.cs` (3.2k lines).** Split it into Equipment, Combat/Threat, Navigation/Unstuck, Persistence and Vitality components.
7. **Shared reservations, blacklists and scan cache.**
   - A static map of target ID → claiming worker, with expiry, so several workers don't race for the same target.
   - The same structure records unreachable targets.
   - Each work area gets one scan cache, refreshed every 1–2 s, instead of each worker calling `OverlapSphere` every frame.
8. **Use vanilla data instead of name matching:**
   - `CookingStation.m_conversion`
   - `Smelter.m_conversion`
   - `Fireplace.m_fuelItem`
   - `Pickable.m_itemPrefab`
   - Plant → sapling links
   - `CharacterDrop`
9. **Network compatibility.** Add a version handshake (for example Jotunn's `NetworkCompatibility`, or a custom RPC) so that clients without the mod, or with a mismatched version, can't join and hit missing prefabs.
10. **Two-client test plan.** Spawn, give gear, take gear, dismiss, reload the world, return to the main menu and load again, let the owning client leave, and open the same worker from two clients. This covers most of the issues above. Add these steps to `manual_verification_checklist.html`.

### Suggested fix order
1. C1 (duplication)
2. C2 + C3 (cheats and telemetry)
3. C4 (dismiss)
4. H1 + H2 + H3 (prefab registration and persistence)
5. C5 + C6 (ownership and Base Tender)
6. H5–H9 (task logic)
7. Medium items
