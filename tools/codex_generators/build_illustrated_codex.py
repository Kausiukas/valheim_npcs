# Master 22-Page Illustrated Codex & Infographics Suite Builder
import os, sys, subprocess, shutil, pymupdf
from codex_styles import COMMON_STYLE, SVG_FILTERS, BASE_DIR, DOCS_DIR
from codex_roles import make_cover_page, make_toc_page, make_troubleshooting_page, make_all_worker_role_pages
from codex_plates import (
    make_plate_1, make_plate_2, make_plate_3, make_plate_4,
    make_plate_5, make_plate_6, make_plate_7, make_plate_8,
    make_page_21
)

HTML_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.html")
PDF_OUT = os.path.join(BASE_DIR, "ValheimWorkers_Guide_Book.pdf")
SUBDIR_HTML = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.html")
SUBDIR_PDF = os.path.join(BASE_DIR, "ValheimWorkers", "ValheimWorkers_Guide_Book.pdf")

print("Generating 10 worker role pages...")
roles = make_all_worker_role_pages()

print("Assembling 22-page Master Codex...")
pages = [
    make_cover_page(),           # Page 1: Monument Cover
    make_toc_page(),             # Page 2: Table of Contents & Core Foundations
    make_plate_1(),              # Page 3: Plate I: Macro Production Web
    roles[4],                    # Page 4: Part I: Lumberjack
    make_plate_2(),              # Page 5: Plate II: Forestry Schema 1
    roles[6],                    # Page 6: Part II: Miner
    make_plate_3(),              # Page 7: Plate III: Quarrying Schema 2
    roles[8],                    # Page 8: Part III: Farmer
    make_plate_4(),              # Page 9: Plate IV: Agriculture Schema 3
    roles[10],                   # Page 10: Part IV: Rancher
    roles[11],                   # Page 11: Part V: Smelter
    make_plate_5(),              # Page 12: Plate V: Metallurgy Schema 4
    roles[13],                   # Page 13: Part VI: Cook
    make_plate_6(),              # Page 14: Plate VI: Culinary Schema 5
    roles[15],                   # Page 15: Part VII: Craftsman
    roles[16],                   # Page 16: Part VIII: Fisher
    roles[17],                   # Page 17: Part IX: Hauler
    make_plate_7(),              # Page 18: Plate VII: Logistics Schema 6
    roles[19],                   # Page 19: Part X: Sentry
    make_plate_8(),              # Page 20: Plate VIII: Defense Schema 7
    make_page_21(),              # Page 21: Spatial Architecture & Blueprint Elevations
    make_troubleshooting_page()  # Page 22: Troubleshooting & Configuration Matrix
]

print(f"Total pages assembled: {len(pages)}")
assert len(pages) == 22, f"Expected 22 pages, but assembled {len(pages)}"

full_html = f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>ValheimWorkers v0.3.0 - The Settlement Steward's Codex</title>
  <style>
{COMMON_STYLE}
  </style>
</head>
<body>
{SVG_FILTERS}
{"".join(pages)}
</body>
</html>
"""

print(f"Writing {HTML_OUT}...")
with open(HTML_OUT, "w", encoding="utf-8") as f:
    f.write(full_html)

os.makedirs(os.path.dirname(SUBDIR_HTML), exist_ok=True)
with open(SUBDIR_HTML, "w", encoding="utf-8") as f:
    f.write(full_html)

print("Compiling PDF with Microsoft Edge...")
edge_path = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if not os.path.exists(edge_path):
    edge_path = r"C:\Program Files\Microsoft\Edge\Application\msedge.exe"

cmd = [
    edge_path,
    "--headless=new",
    "--disable-gpu",
    "--no-pdf-header-footer",
    f"--print-to-pdf={PDF_OUT}",
    HTML_OUT
]
subprocess.run(cmd, check=True)
print(f"Successfully compiled {PDF_OUT}!")

shutil.copy2(PDF_OUT, SUBDIR_PDF)
print(f"Mirrored PDF to {SUBDIR_PDF}")

print("Verifying PDF and rendering high-resolution PNG pages...")
doc = pymupdf.open(PDF_OUT)
print(f"PDF verified: {len(doc)} pages generated.")
assert len(doc) == 22, f"PDF page count mismatch: expected 22, got {len(doc)}"

PAGES_PNG_DIR = os.path.join(DOCS_DIR, "pages")
os.makedirs(PAGES_PNG_DIR, exist_ok=True)

for i, page in enumerate(doc):
    pix = page.get_pixmap(dpi=150)
    png_path = os.path.join(PAGES_PNG_DIR, f"page_{i+1:02d}.png")
    pix.save(png_path)
    print(f"Rendered Page {i+1:02d} -> {png_path}")

print("Master 22-Page Codex successfully compiled and verified!")
