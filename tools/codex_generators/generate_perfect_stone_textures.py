import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageFilter
from scipy.ndimage import gaussian_filter, distance_transform_edt

STONE_DIR = r"d:\valheim_mods\docs\assets\stone"
os.makedirs(STONE_DIR, exist_ok=True)
FONTS_DIR = r"d:\valheim_mods\docs\assets\fonts"

print("--- Generating Nordic Chiseled Basalt & Runestone Textures ---")

def generate_seamless_fbm(size=512, octaves=6, persistence=0.52):
    np.random.seed(1337)
    total = np.zeros((size, size), dtype=np.float32)
    amp = 1.0
    for oct in range(octaves):
        freq = 2 ** (oct + 1)
        step = size // freq
        if step < 1:
            break
        r_grid = np.random.randn(freq, freq).astype(np.float32)
        scaled = np.zeros((size, size), dtype=np.float32)
        for y in range(freq):
            for x in range(freq):
                scaled[y*step:(y+1)*step, x*step:(x+1)*step] = r_grid[y, x]
        blurred = gaussian_filter(scaled, sigma=max(1.0, step*0.65), mode='wrap')
        total += blurred * amp
        amp *= persistence
        
    total = (total - total.min()) / (total.max() - total.min() + 1e-6)
    return total

def generate_periodic_voronoi_cracks(size=512, num_pts=48):
    np.random.seed(999)
    # Generate points in [0, size)
    base_pts = np.random.randint(0, size, (num_pts, 2))
    # 3x3 periodic tiling
    all_pts = []
    for dy in [-size, 0, size]:
        for dx in [-size, 0, size]:
            all_pts.append(base_pts + np.array([dy, dx]))
    all_pts = np.vstack(all_pts)
    
    # Create grid in 3x3 space
    big_size = size * 3
    big_grid = np.zeros((big_size, big_size), dtype=bool)
    # shift to positive coords
    shifted_pts = all_pts + size
    big_grid[shifted_pts[:,0], shifted_pts[:,1]] = True
    
    # Distance transform
    _, idx = distance_transform_edt(~big_grid, return_indices=True)
    # Crop central tile
    c_idx_0 = idx[0, size:size*2, size:size*2]
    c_idx_1 = idx[1, size:size*2, size:size*2]
    
    # Edge detect
    gx = np.diff(c_idx_0, axis=0, append=c_idx_0[-1:]) != 0
    gy = np.diff(c_idx_1, axis=1, append=c_idx_1[:,-1:]) != 0
    cracks = (gx | gy).astype(np.float32)
    return gaussian_filter(cracks, sigma=0.8, mode='wrap')

H, W = 512, 512
base_fbm = generate_seamless_fbm(512, octaves=6, persistence=0.52)
cracks = generate_periodic_voronoi_cracks(512, 45)

# High frequency crystalline granite noise
np.random.seed(42)
micro_grain = np.random.normal(0, 0.12, (H, W)).astype(np.float32)
micro_grain = gaussian_filter(micro_grain, sigma=0.6, mode='wrap')

# Combine heightmap with rock clefts
heightmap = base_fbm * 0.70 + micro_grain * 0.18 - cracks * 0.15
heightmap = (heightmap - heightmap.min()) / (heightmap.max() - heightmap.min() + 1e-6)

# Directional grazing light from top-left (-1, -1, 1.8)
# Compute surface normal from gradient
gy, gx = np.gradient(heightmap * 14.0)
# Normal vector: [-gx, -gy, 1.0]
norm_len = np.sqrt(gx**2 + gy**2 + 1.0)
nx = -gx / norm_len
ny = -gy / norm_len
nz = 1.0 / norm_len

# Light vector from top-left 315 deg, elevated 45 deg
lx, ly, lz = -0.6, -0.6, 0.8
l_len = math.sqrt(lx**2 + ly**2 + lz**2)
lx, ly, lz = lx/l_len, ly/l_len, lz/l_len

# Diffuse dot product
diffuse = np.clip(nx * lx + ny * ly + nz * lz, 0.0, 1.0)
diffuse = (diffuse - 0.4) / 0.6 # boost contrast

# 1. Monolith Background Texture
print("1. Generating enhanced stone_monolith_bg.png...")
# Authentic Nordic dark basalt / slate granite:
# Shadows: RGB(22, 25, 30)
# Midtones: RGB(36, 42, 50)
# Sunlit highlights: RGB(62, 70, 82)
# Quartz sparks: RGB(95, 105, 122)
c_shadow = np.array([22.0, 26.0, 32.0], dtype=np.float32)
c_mid = np.array([38.0, 44.0, 53.0], dtype=np.float32)
c_lit = np.array([64.0, 72.0, 84.0], dtype=np.float32)
c_quartz = np.array([105.0, 116.0, 134.0], dtype=np.float32)
c_moss = np.array([45.0, 42.0, 30.0], dtype=np.float32)

rgb_mono = np.zeros((H, W, 3), dtype=np.float32)
mask_lit = diffuse > 0.5
t_lit = np.clip((diffuse - 0.5) * 2.0, 0, 1)
t_sh = np.clip(diffuse * 2.0, 0, 1)

rgb_mono = c_shadow * (1.0 - t_sh[:, :, None]) + c_mid * t_sh[:, :, None]
rgb_mono = rgb_mono * (1.0 - t_lit[:, :, None]) + c_lit * t_lit[:, :, None]

# Add quartz sparkles
quartz_mask = np.clip((micro_grain - 0.10) * 14.0, 0, 1)
rgb_mono = rgb_mono * (1.0 - quartz_mask[:, :, None] * 0.55) + c_quartz * (quartz_mask[:, :, None] * 0.55)

# Crevice shadows
crack_sh = np.clip(cracks * 2.8, 0, 1)
rgb_mono = rgb_mono * (1.0 - crack_sh[:, :, None] * 0.45)

rgb_mono = np.clip(rgb_mono, 0, 255).astype(np.uint8)
Image.fromarray(rgb_mono).save(os.path.join(STONE_DIR, "stone_monolith_bg.png"), optimize=True)

# 2. Tablet Slab Texture (Recessed Carved Stone Slab)
print("2. Generating enhanced stone_tablet_bg.png...")
# Slightly darker for crisp typography contrast, smooth chiseled face:
c_tab_sh = np.array([18.0, 21.0, 26.0], dtype=np.float32)
c_tab_mid = np.array([30.0, 35.0, 43.0], dtype=np.float32)
c_tab_lit = np.array([50.0, 58.0, 68.0], dtype=np.float32)

rgb_tab = c_tab_sh * (1.0 - t_sh[:, :, None]) + c_tab_mid * t_sh[:, :, None]
rgb_tab = rgb_tab * (1.0 - t_lit[:, :, None]) + c_tab_lit * t_lit[:, :, None]
rgb_tab = rgb_tab * (1.0 - quartz_mask[:, :, None] * 0.35) + c_quartz * (quartz_mask[:, :, None] * 0.35)
rgb_tab = rgb_tab * (1.0 - crack_sh[:, :, None] * 0.35)

rgb_tab = np.clip(rgb_tab, 0, 255).astype(np.uint8)
Image.fromarray(rgb_tab).save(os.path.join(STONE_DIR, "stone_tablet_bg.png"), optimize=True)

# 3. Sunken Relic Sockets (64x64)
print("3. Generating 64x64 stone sockets...")
S = 64
socket_arr = np.zeros((S, S, 4), dtype=np.uint8)
cx_s, cy_s = 31.5, 31.5
for y in range(S):
    for x in range(S):
        dx = abs(x - cx_s)
        dy = abs(y - cy_s)
        corner = max(dx + dy - 34.0, 0.0)
        box_d = max(dx, dy) + corner * 0.55
        
        grain = (heightmap[y % H, x % W] - 0.5) * 18.0
        
        if box_d <= 23.5:
            # Cavity interior
            dist_to_edge = 23.5 - box_d
            ao = np.clip(dist_to_edge / 6.0, 0.25, 1.0)
            tl_bias = max(0.0, ((32 - x) + (32 - y)) / 45.0)
            base_cavity = 17.0 + ao * 9.0 - tl_bias * 9.0 + grain * 0.4
            base_cavity = max(9.0, min(30.0, base_cavity))
            
            socket_arr[y, x] = [
                int(base_cavity * 0.95),
                int(base_cavity * 1.05),
                int(base_cavity * 1.25),
                255
            ]
        elif box_d <= 27.5:
            # Chiseled bevel rim
            nx_s = (x - cx_s) / max(1e-3, box_d)
            ny_s = (y - cy_s) / max(1e-3, box_d)
            light_dot = -(nx_s * 0.707 + ny_s * 0.707)
            
            if light_dot > 0.15:
                # Top-left bevel highlight: bright warm granite edge
                hl = 82.0 + light_dot * 65.0 + grain
                socket_arr[y, x] = [
                    int(min(235, hl * 1.15)),
                    int(min(215, hl * 1.05)),
                    int(min(190, hl * 0.90)),
                    255
                ]
            else:
                sh = max(8.0, 20.0 + light_dot * 14.0 + grain * 0.5)
                socket_arr[y, x] = [
                    int(sh * 0.9),
                    int(sh * 0.95),
                    int(sh * 1.1),
                    255
                ]
        else:
            tab_val = 32.0 + grain
            socket_arr[y, x] = [
                int(tab_val * 0.92),
                int(tab_val * 1.0),
                int(tab_val * 1.15),
                255
            ]

Image.fromarray(socket_arr).save(os.path.join(STONE_DIR, "stone_socket_bg.png"))

# Equipped socket with glowing cyan runic ward channel
socket_eq_arr = socket_arr.copy()
for y in range(S):
    for x in range(S):
        dx = abs(x - cx_s)
        dy = abs(y - cy_s)
        corner = max(dx + dy - 34.0, 0.0)
        box_d = max(dx, dy) + corner * 0.55
        
        if 23.5 <= box_d <= 27.0:
            socket_eq_arr[y, x] = [56, 189, 248, 255]
        elif 19.5 <= box_d < 23.5:
            glow_frac = (box_d - 19.5) / 4.0
            cur = socket_arr[y, x][:3].astype(np.float32)
            cyan = np.array([56.0, 189.0, 248.0], dtype=np.float32)
            blended = cur * (1.0 - glow_frac * 0.55) + cyan * (glow_frac * 0.55)
            socket_eq_arr[y, x][:3] = np.clip(blended, 0, 255).astype(np.uint8)

Image.fromarray(socket_eq_arr).save(os.path.join(STONE_DIR, "stone_socket_equipped_bg.png"))

# 4. Circular Carved Runestone Medallion for Page 1 Cover
print("4. Crafting stone_cover_crest.png (512x512)...")
CS = 512
medallion = Image.new("RGBA", (CS, CS), (0, 0, 0, 0))
cx, cy = CS / 2.0, CS / 2.0

R_outer = 238.0
R_ring_outer = 222.0
R_ring_inner = 175.0
R_inner_plinth = 168.0

for y in range(CS):
    for x in range(CS):
        dist = math.hypot(x - cx, y - cy)
        if dist > R_outer + 2:
            continue
            
        grain = (heightmap[y % H, x % W] - 0.5) * 22.0
        angle = math.atan2(y - cy, x - cx)
        dir_dot = -(math.cos(angle) * 0.707 + math.sin(angle) * 0.707)
        
        if dist > R_outer - 4:
            if dir_dot > 0:
                val = 48.0 + dir_dot * 75.0 + grain
                medallion.putpixel((x, y), (int(min(230, val*1.15)), int(min(210, val*1.05)), int(min(185, val*0.9)), 255))
            else:
                val = max(10.0, 24.0 + dir_dot * 15.0 + grain)
                medallion.putpixel((x, y), (int(val*0.9), int(val*0.95), int(val*1.1), 255))
        elif dist >= R_ring_outer:
            val = 40.0 + grain
            medallion.putpixel((x, y), (int(val*0.95), int(val*1.0), int(val*1.12), 255))
        elif dist > R_ring_inner:
            depth_ao = math.sin((dist - R_ring_inner) / (R_ring_outer - R_ring_inner) * math.pi)
            val = 22.0 - depth_ao * 7.0 + grain * 0.7
            medallion.putpixel((x, y), (int(val*0.92), int(val*0.98), int(val*1.15), 255))
        elif dist >= R_inner_plinth:
            if dir_dot > 0:
                val = 48.0 + dir_dot * 65.0 + grain
                medallion.putpixel((x, y), (int(min(220, val*1.12)), int(min(205, val*1.02)), int(min(180, val*0.88)), 255))
            else:
                val = max(10.0, 22.0 + dir_dot * 12.0 + grain)
                medallion.putpixel((x, y), (int(val*0.9), int(val*0.95), int(val*1.1), 255))
        else:
            radial_wave = math.sin(dist * 0.2) * 5.0
            val = 32.0 + grain + radial_wave
            medallion.putpixel((x, y), (int(val*0.95), int(val*1.02), int(val*1.18), 255))

draw = ImageDraw.Draw(medallion)
runes = ["ᚠ", "ᚢ", "ᚦ", "ᚨ", "ᚱ", "ᚲ", "ᚷ", "ᚹ", "ᚺ", "ᚾ", "ᛁ", "ᛃ", "ᛈ", "ᛇ", "ᛉ", "ᛋ", "ᛏ", "ᛒ", "ᛖ", "ᛗ", "ᛚ", "ᛜ", "ᛞ", "ᛟ"]
R_rune = (R_ring_outer + R_ring_inner) / 2.0

norse_font = None
for f_name in ["Norsebold.otf", "Norse.otf"]:
    p = os.path.join(FONTS_DIR, f_name)
    if os.path.exists(p):
        try:
            norse_font = ImageFont.truetype(p, 25)
            break
        except Exception:
            pass

if norse_font is None:
    try:
        norse_font = ImageFont.truetype("segoeui.ttf", 22)
    except Exception:
        norse_font = ImageFont.load_default()

num_runes = len(runes)
for i, rune in enumerate(runes):
    theta = i * (2.0 * math.pi / num_runes) - math.pi / 2.0
    rx = cx + R_rune * math.cos(theta)
    ry = cy + R_rune * math.sin(theta)
    
    draw.text((rx + 1.2, ry + 1.5), rune, fill=(10, 8, 6, 230), font=norse_font, anchor="mm")
    draw.text((rx - 1.0, ry - 1.0), rune, fill=(245, 215, 130, 180), font=norse_font, anchor="mm")
    draw.text((rx, ry), rune, fill=(218, 170, 70, 255), font=norse_font, anchor="mm")

draw.ellipse([cx - R_outer, cy - R_outer, cx + R_outer, cy + R_outer], outline=(215, 175, 95, 210), width=2)
draw.ellipse([cx - R_ring_outer, cy - R_ring_outer, cx + R_ring_outer, cy + R_ring_outer], outline=(25, 20, 15, 240), width=2)
draw.ellipse([cx - R_ring_inner, cy - R_ring_inner, cx + R_ring_inner, cy + R_ring_inner], outline=(215, 175, 95, 190), width=2)
draw.ellipse([cx - R_inner_plinth, cy - R_inner_plinth, cx + R_inner_plinth, cy + R_inner_plinth], outline=(25, 20, 15, 240), width=2)

medallion.save(os.path.join(STONE_DIR, "stone_cover_crest.png"))
print("Done generating updated stone assets!")
