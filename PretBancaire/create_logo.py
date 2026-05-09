import sys
import os
import shutil

try:
    from PIL import Image
except ImportError:
    os.system('pip install Pillow')
    from PIL import Image

src = r'C:\Users\alijuvance\.gemini\antigravity\brain\2ec4a18f-8bb6-4ad9-a06d-688a75ffefa0\banking_app_logo_1778320921080.png'
dest_dir = r'd:\Nouveau dossier\Pret-bancaire\PretBancaire\Resources'

if not os.path.exists(dest_dir):
    os.makedirs(dest_dir)

dest_png = os.path.join(dest_dir, 'logo.png')
dest_ico = os.path.join(dest_dir, 'logo.ico')

shutil.copyfile(src, dest_png)

img = Image.open(src)
img.save(dest_ico, format='ICO', sizes=[(256, 256), (128, 128), (64, 64), (32, 32), (16, 16)])

print('Logo successfully copied and converted to ICO!')
