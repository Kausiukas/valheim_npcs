import os
import sys
import struct
import urllib.request
import urllib.parse
import json
import base64

ASSETS_DIR = r"d:\valheim_mods\docs\assets"
FONTS_DIR = os.path.join(ASSETS_DIR, "fonts")
ICONS_DIR = os.path.join(ASSETS_DIR, "icons")

os.makedirs(FONTS_DIR, exist_ok=True)
os.makedirs(ICONS_DIR, exist_ok=True)

# 1. Extract Norse.otf and Norsebold.otf from resources.assets
valheim_assets_path = r"D:\Steam\steamapps\common\Valheim\valheim_Data\resources.assets"
print("--- Extracting Valheim Original Fonts ---")
if os.path.exists(valheim_assets_path):
    with open(valheim_assets_path, "rb") as f:
        data = f.read()
    
    font_targets = [
        ("Norse.otf", 34316480, 43560),
        ("Norsebold.otf", 79566052, 41436)
    ]
    for fn, start, expected_sz in font_targets:
        sz = struct.unpack('<I', data[start-4:start])[0]
        sig = data[start:start+4]
        out_file = os.path.join(FONTS_DIR, fn)
        if sig == b'OTTO' and sz == expected_sz:
            with open(out_file, "wb") as out:
                out.write(data[start:start+sz])
            print(f"Extracted {fn} ({sz} bytes, sig={sig}) successfully!")
        else:
            print(f"Warning: {fn} signature mismatch! sig={sig}, sz={sz}")
else:
    print(f"Warning: {valheim_assets_path} not found.")

# 2. Download Averia Serif Libre weights from Google Fonts GitHub
print("\n--- Downloading Averia Serif Libre Font Weights ---")
averia_files = [
    "AveriaSerifLibre-Regular.ttf",
    "AveriaSerifLibre-Bold.ttf",
    "AveriaSerifLibre-Italic.ttf",
    "AveriaSerifLibre-BoldItalic.ttf"
]
for fn in averia_files:
    dst = os.path.join(FONTS_DIR, fn)
    if not os.path.exists(dst) or os.path.getsize(dst) < 10000:
        url = f"https://raw.githubusercontent.com/google/fonts/main/ofl/averiaseriflibre/{fn}"
        try:
            req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
            with urllib.request.urlopen(req, timeout=15) as resp:
                content = resp.read()
                with open(dst, "wb") as out:
                    out.write(content)
            print(f"Downloaded {fn} ({len(content)} bytes)")
        except Exception as e:
            print(f"Error downloading {fn}: {e}")
    else:
        print(f"{fn} already present ({os.path.getsize(dst)} bytes)")

# 3. Download Valheim Game Assets from Valheim Wiki (MediaWiki API)
print("\n--- Pulling Valheim Game Assets from Valheim Wiki ---")

# Mapping of canonical filename to MediaWiki title search
IMAGE_FILES = [
    # Tools
    "Stone_Axe.png", "Bronze_Axe.png", "Iron_Axe.png", "Blackmetal_Axe.png",
    "Antler_Pickaxe.png", "Bronze_Pickaxe.png", "Iron_Pickaxe.png",
    "Cultivator.png", "Hoe.png", "Hammer.png", "Torch.png",
    # Weapons & Armor
    "Finewood_Bow.png", "Huntsman_Bow.png", "Draugr_Fang.png",
    "Wood_Arrow.png", "Fire_Arrow.png", "Iron_Sword.png", "Silver_Sword.png",
    "Banded_Shield.png", "Black_Metal_Shield.png",
    "Iron_Helmet.png", "Iron_Scale_Mail.png", "Rag_Tunic.png",
    # Wood & Trees
    "Wood.png", "Fine_Wood.png", "Core_Wood.png", "Resin.png",
    "Beech_Seeds.png", "Birch_Seeds.png", "Pine_Cone.png",
    "Wood_Pile.png", "Core_Wood_Stack.png",
    # Ores & Metals
    "Stone.png", "Flint.png", "Copper_Ore.png", "Tin_Ore.png",
    "Iron_Scrap.png", "Silver_Ore.png", "Coal.png",
    "Stone_Pile.png", "Coal_Pile.png",
    "Copper.png", "Bronze.png", "Iron.png", "Silver.png", "Black_Metal.png",
    # Crops & Foraging
    "Carrot.png", "Carrot_Seeds.png", "Turnip.png", "Turnip_Seeds.png",
    "Onion.png", "Onion_Seeds.png", "Barley.png", "Flax.png",
    "Raspberries.png", "Blueberries.png", "Cloudberries.png",
    "Mushroom.png", "Yellow_Mushroom.png", "Thistle.png", "Dandelion.png",
    "Honey.png", "Sap.png",
    # Food & Meads
    "Raw_Meat.png", "Cooked_Meat.png", "Boar_Jerky.png",
    "Deer_Stew.png", "Sausages.png", "Queens_Jam.png", "Serpent_Stew.png",
    "Barley_Flour.png", "Linen_Thread.png",
    "Medium_Healing_Mead.png", "Tasty_Mead.png", "Frost_Resistance_Mead.png",
    # Stations & Structures
    "Workbench.png", "Forge.png", "Smelter.png", "Charcoal_Kiln.png", "Blast_Furnace.png",
    "Windmill.png", "Spinning_Wheel.png", "Cooking_Station.png", "Iron_Cooking_Station.png",
    "Cauldron.png", "Fermenter.png", "Beehive.png", "Sap_Extractor.png",
    # Logistics, Storage, Ships & Bases
    "Cart.png", "Piece_Chest_Wood.png", "Reinforced_Chest.png", "Black_Metal_Chest.png",
    "Sign.png", "Karve.png", "Longship.png",
    "Campfire.png", "Hearth.png", "Standing_Wood_Torch.png", "Sconce.png",
    "Dragon_Bed.png", "Raven_Throne.png",
    # Treasures & Currency
    "Coins.png", "Coin_Pile.png", "Bone_Fragments.png", "Amber.png", "Ruby.png",
    # Creatures & NPC references
    "Boar.png", "Deer.png", "Neck.png", "Wolf.png", "Lox.png", "Chicken.png",
    "Greydwarf.png", "Skeleton.png", "Troll.png", "Hugin.png", "Valkyrie.png"
]

def fetch_wiki_image_urls(filenames):
    # Query in batches of 30
    results = {}
    batch_size = 25
    for i in range(0, len(filenames), batch_size):
        batch = filenames[i:i+batch_size]
        titles = "|".join([f"File:{fn}" for fn in batch])
        url = f"https://valheim.fandom.com/api.php?action=query&titles={urllib.parse.quote(titles)}&prop=imageinfo&iiprop=url&format=json"
        try:
            req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'})
            with urllib.request.urlopen(req, timeout=15) as resp:
                data = json.loads(resp.read().decode('utf-8'))
                pages = data.get('query', {}).get('pages', {})
                for pid, pinfo in pages.items():
                    title = pinfo.get('title', '')
                    clean_name = title.replace("File:", "").replace(" ", "_")
                    imageinfo = pinfo.get('imageinfo', [])
                    if imageinfo:
                        img_url = imageinfo[0].get('url', '')
                        results[clean_name.lower()] = (clean_name, img_url)
        except Exception as ex:
            print(f"Error querying batch: {ex}")
    return results

print(f"Querying Wiki API for {len(IMAGE_FILES)} assets...")
url_map = fetch_wiki_image_urls(IMAGE_FILES)
print(f"Resolved {len(url_map)} image URLs from Wiki.")

downloaded_count = 0
for fn in IMAGE_FILES:
    key = fn.lower().replace(" ", "_")
    dst_fn = fn.replace(" ", "_")
    dst_path = os.path.join(ICONS_DIR, dst_fn)
    
    if os.path.exists(dst_path) and os.path.getsize(dst_path) > 500:
        continue
    
    if key in url_map:
        _, img_url = url_map[key]
        try:
            req = urllib.request.Request(img_url, headers={'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)'})
            with urllib.request.urlopen(req, timeout=15) as r:
                data = r.read()
                with open(dst_path, "wb") as out:
                    out.write(data)
            downloaded_count += 1
            print(f"Downloaded: {dst_fn} ({len(data)} bytes)")
        except Exception as e:
            print(f"Failed to download {dst_fn} from {img_url}: {e}")
    else:
        print(f"Not found in Wiki API: {fn}")

print(f"\nDone! Newly downloaded: {downloaded_count} images.")
print(f"Total icons in directory: {len(os.listdir(ICONS_DIR))}")
