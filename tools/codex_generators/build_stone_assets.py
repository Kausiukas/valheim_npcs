import os
import math
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

STONE_DIR = r"d:\valheim_mods\docs\assets\stone"
os.makedirs(STONE_DIR, exist_ok=True)

print("--- Generating Authentic Stone & Runestone Textures ---")

def generate_perlin_noise_2d(shape, res):
    def f(t):
        return 6*t**5 - 15*t**4 + 10*t**3

    delta = (res[0] / shape[0], res[1] / shape[1])
    d = (shape[0] // res[0], shape[1] // res[1])
    grid = np.mgrid[0:res[0]:delta[0], 0:res[1]:delta[1]].transpose(1, 2, 0) % 1

    # Gradients
    angles = 2*np.pi*np.random.RandomState(42).rand(res[0]+1, res[1]+1)
    gradients = np.dstack((np.cos(angles), np.sin(angles)))
    g00 = gradients[0:-1, 0:-1].repeat(d[0], 0).repeat(d[1], 1)
    g10 = gradients[1:, 0:-1].repeat(d[0], 0).repeat(d[1], 1)
    g01 = gradients[0:-1, 1:].repeat(d[0], 0).repeat(d[1], 1)
    g11 = gradients[1:, 1:].repeat(d[0], 0).repeat(d[1], 1)

    # Ramps
    n00 = np.sum(np.dstack((grid[:,:,0], grid[:,:,1])) * g00, 2)
    n10 = np.sum(np.dstack((grid[:,:,0]-1, grid[:,:,1])) * g10, 2)
    n01 = np.sum(np.dstack((grid[:,:,0], grid[:,:,1]-1)) * g01, 2)
    n11 = np.sum(np.dstack((grid[:,:,0]-1, grid[:,:,1]-1)) * g11, 2)

    # Interpolation
    t = f(grid)
    n0 = n00*(1-t[:,:,0]) + t[:,:,0]*n10
    n1 = n01*(1-t[:,:,0]) + t[:,:,0]*n11
    return np.sqrt(2)*((1-t[:,:,1])*n0 + t[:,:,1]*n1)

def fbm(shape, octaves=4, persistence=0.5):
    h, w = shape
    res = (4, 4)
    total = np.zeros(shape)
    frequency = 1
    amplitude = 1.0
    max_amp = 0.0
    for i in range(octaves):
        r0 = res[0] * frequency
        r1 = res[1] * frequency
        # ensure divisibility
        if h % r0 != 0 or w % r1 != 0:
            break
        n = generate_perlin_noise_2d(shape, (r0, r1))
        total += n * amplitude
        max_amp += amplitude
        amplitude *= persistence
        frequency *= 2
    return total / max_amp

# 1. Monolith Background Texture (Dark Basalt / Slate)
print("1. Generating stone_monolith_bg.png...")
H, W = 512, 512
base_noise = fbm((H, W), octaves=4)
# Fine grain noise
fine_grain = np.random.RandomState(101).normal(0, 0.08, (H, W))
slate_layers = np.sin(np.linspace(0, 30, H))[:, None] * 0.05
stone_height = base_noise * 0.7 + fine_grain * 0.3 + slate_layers

# Base dark basalt palette: [22, 17, 13] to [38, 30, 23]
c_low = np.array([17, 13, 10], dtype=np.float32)
c_high = np.array([36, 28, 22], dtype=np.float32)
t = (stone_height - stone_height.min()) / (stone_height.max() - stone_height.min() + 1e-6)
t = np.clip(t, 0, 1)

rgb = c_low + t[:, :, None] * (c_high - c_low)
rgb = np.clip(rgb, 0, 255).astype(np.uint8)
img_monolith = Image.fromarray(rgb)
img_monolith.save(os.path.join(STONE_DIR, "stone_monolith_bg.png"), optimize=True)

# 2. Stone Tablet Background Texture (Slightly lighter chiseled slate)
print("2. Generating stone_tablet_bg.png...")
c_t_low = np.array([24, 18, 14], dtype=np.float32)
c_t_high = np.array([44, 34, 27], dtype=np.float32)
rgb_tablet = c_t_low + t[:, :, None] * (c_t_high - c_t_low)
rgb_tablet = np.clip(rgb_tablet, 0, 255).astype(np.uint8)
img_tablet = Image.fromarray(rgb_tablet)
img_tablet.save(os.path.join(STONE_DIR, "stone_tablet_bg.png"), optimize=True)

# 3. Stone Sockets (64x64 recessed socket textures)
print("3. Generating stone sockets...")
S = 64
socket_arr = np.zeros((S, S, 4), dtype=np.uint8)
# create recessed octagonal cavity with stone texture
for y in range(S):
    for x in range(S):
        # distance from center
        dx = abs(x - S/2)
        dy = abs(y - S/2)
        # rounded / octagonal box sdf
        r = 26
        corner = max(dx + dy - 34, 0)
        box_d = max(dx, dy) + corner*0.5
        
        # stone noise
        n = fine_grain[y % H, x % W] * 15
        
        if box_d < 24: # inside cavity
            # deep inner shadow towards top-left
            shade = max(0, (24 - box_d) / 24.0)
            tl_shadow = max(0, (24 - x) + (24 - y)) / 48.0
            val = int(14 + n - tl_shadow * 8)
            val = max(8, min(24, val))
            socket_arr[y, x] = [int(val*1.1), int(val*0.9), int(val*0.8), 255]
        elif box_d <= 27: # chiseled bevel rim
            # top-left catches light, bottom-right dark shadow
            is_tl = (x + y) < S
            if is_tl:
                val = int(58 + n*1.5) # bevel highlight
                socket_arr[y, x] = [int(val*1.15), int(val*1.0), int(val*0.85), 255]
            else:
                val = int(12 + n) # bevel shadow
                socket_arr[y, x] = [val, int(val*0.9), int(val*0.8), 255]
        else: # outer stone
            socket_arr[y, x] = [28, 22, 17, 255]

Image.fromarray(socket_arr).save(os.path.join(STONE_DIR, "stone_socket_bg.png"))

# Equipped socket with glowing cyan runic ward channel
socket_eq_arr = socket_arr.copy()
for y in range(S):
    for x in range(S):
        dx = abs(x - S/2)
        dy = abs(y - S/2)
        corner = max(dx + dy - 34, 0)
        box_d = max(dx, dy) + corner*0.5
        if 23 <= box_d <= 27:
            # glowing runic ward channel
            socket_eq_arr[y, x] = [41, 182, 246, 255]
        elif 20 <= box_d < 23:
            # cyan inner glow
            g = (box_d - 20) / 3.0
            orig = socket_arr[y, x][:3]
            cyan = np.array([41, 182, 246])
            blended = orig * (1 - g*0.6) + cyan * (g*0.6)
            socket_eq_arr[y, x][:3] = blended.astype(np.uint8)

Image.fromarray(socket_eq_arr).save(os.path.join(STONE_DIR, "stone_socket_equipped_bg.png"))

# 4. Circular Carved Runestone Medallion for Page 1 Cover
print("4. Generating stone_cover_crest.png (512x512)...")
CS = 512
medallion = Image.new("RGBA", (CS, CS), (0, 0, 0, 0))
draw = ImageDraw.Draw(medallion)

# Center and radius
cx, cy = CS // 2, CS // 2
R_outer = 230
R_ring = 195
R_inner = 160

# Draw outer weathered stone circle with beveled rim
for r in range(R_outer, R_inner, -1):
    frac = (r - R_inner) / (R_outer - R_inner)
    # stone tone
    base_col = int(26 + frac * 14)
    # directional light (top-left 45 deg)
    for a_deg in range(360):
        rad = math.radians(a_deg)
        x = int(cx + r * math.cos(rad))
        y = int(cy + r * math.sin(rad))
        if 0 <= x < CS and 0 <= y < CS:
            # dot product with light dir (-0.707, -0.707)
            dot_l = -(math.cos(rad) + math.sin(rad)) * 0.707
            col = int(base_col + dot_l * 18)
            col = max(10, min(65, col))
            medallion.putpixel((x, y), (int(col*1.15), int(col*0.95), int(col*0.8), 255))

# Draw inner dark plinth
for y in range(cy - R_inner, cy + R_inner):
    for x in range(cx - R_inner, cx + R_inner):
        dist = math.hypot(x - cx, y - cy)
        if dist <= R_inner:
            # subtle stone texture
            n = fine_grain[y % H, x % W] * 12
            # subtle center glow
            glow = (1 - dist / R_inner) * 10
            val = int(20 + n + glow)
            val = max(12, min(42, val))
            medallion.putpixel((x, y), (int(val*1.12), int(val*0.92), int(val*0.78), 255))

# Draw Elder Futhark Runic Ring in the channel
runes = ["ᚠ", "ᚢ", "ᚦ", "ᚨ", "ᚱ", "ᚲ", "ᚷ", "ᚹ", "ᚺ", "ᚾ", "ᛁ", "ᛃ", "ᛈ", "ᛇ", "ᛉ", "ᛋ", "ᛏ", "ᛒ", "ᛖ", "ᛗ", "ᛚ", "ᛜ", "ᛞ", "ᛟ"]
# We will draw circular grooved rings
draw.ellipse([cx - R_outer, cy - R_outer, cx + R_outer, cy + R_outer], outline=(92, 68, 36, 255), width=3)
draw.ellipse([cx - R_ring, cy - R_ring, cx + R_ring, cy + R_ring], outline=(168, 126, 52, 200), width=2)
draw.ellipse([cx - R_inner, cy - R_inner, cx + R_inner, cy + R_inner], outline=(68, 50, 27, 255), width=3)

# Save medallion
medallion.save(os.path.join(STONE_DIR, "stone_cover_crest.png"))
print("All stone assets generated successfully!")
