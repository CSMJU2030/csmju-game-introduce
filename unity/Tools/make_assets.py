from PIL import Image, ImageDraw
from pathlib import Path
root=Path('Assets/Resources')
char=root/'Characters/ManaSeed'
# Composite free Mana Seed base + outfit + hair + hat sheets, retaining source 64px frame grid.
layers=[Image.open(char/f'{n}.png').convert('RGBA') for n in ('base','outfit','hair','hat')]
out=Image.new('RGBA',(384,256))
# Bottom four rows of the 8x8 sheet are the four directional walk cycles; 6 frames each.
for d, source_row in enumerate((4, 7, 6, 5)):  # down, left, right, up
    for f in range(6):
        x=f*64; y=source_row*64
        cell=Image.new('RGBA',(64,64))
        for layer in layers: cell.alpha_composite(layer.crop((x,y,x+64,y+64)))
        out.alpha_composite(cell,(f*64,d*64))
out.save(char/'character_walk.png')

# Compose Kenmi's licensed tiles at native pixel size; do not resample artwork.
import random
src=Path(__file__).resolve().parent.parent/'ArtSources/Kenmi'
def asset(name): return Image.open(src/name).convert('RGBA')
grass=asset('Tiles/Grass_Middle.png'); path=asset('Tiles/Path_Tile.png'); water=asset('Tiles/Water_Tile.png'); farm=asset('Tiles/FarmLand_Tile.png')
tree=asset('Outdoor decoration/Oak_Tree.png'); house=asset('Outdoor decoration/House_1_Wood_Base_Blue.png'); decor=asset('Outdoor decoration/Outdoor_Decor_Free.png'); bridge=asset('Outdoor decoration/Bridge_Wood.png')
slugs=['welcome_zone','curriculum_hall','software_lab','ai_data_cave','iot_garden']
for index,slug in enumerate(slugs):
    rng=random.Random(341+index); im=Image.new('RGBA',(448,352))
    def stamp(sprite,x,y): im.alpha_composite(sprite,(x,y))
    def patch(sheet,x,y,cols,rows):
        # Nine-slice top-left 3x3 tiles from Kenmi's autotile sheet.
        for j in range(rows):
            sy=0 if j==0 else 32 if j==rows-1 else 16
            for i in range(cols):
                sx=0 if i==0 else 32 if i==cols-1 else 16
                stamp(sheet.crop((sx,sy,sx+16,sy+16)),x+i*16,y+j*16)
    for y in range(0,352,16):
        for x in range(0,448,16): stamp(grass,x,y)
    for _ in range(100):
        x,y=rng.randrange(28)*16,rng.randrange(22)*16
        stamp(decor.crop((rng.choice([0,16,32]),0,48,16)).crop((0,0,16,16)),x,y)
    # Continuous east-west walkway meets at zone boundaries, clear around every NPC.
    patch(path,-16,224,30,5)
    patch(path,128,128,6,11)
    if index==0:
        patch(path,64,160,19,5); stamp(house,160,40)
        for x,y in [(16,8),(336,20),(0,112),(368,120),(64,272),(320,278)]: stamp(tree,x,y)
        stamp(decor.crop((80,64,112,128)),284,170)
    elif index==1:
        stamp(house,64,48);stamp(house,256,48);patch(path,96,176,16,4)
        for x,y in [(0,16),(176,0),(360,0),(360,272)]:stamp(tree,x,y)
        for x in range(184,264,16): stamp(decor.crop((0,128,16,144)),x,185)
    elif index==2:
        stamp(house,64,32);stamp(house,240,64);patch(path,112,176,14,4)
        for x,y in [(0,0),(336,0),(368,126),(0,278)]:stamp(tree,x,y)
        for x in (80,112,304): stamp(asset('Outdoor decoration/Chest.png'),x,208)
    elif index==3:
        patch(water,224,48,12,10);stamp(house,48,48)
        # A wooden crossing over the data pond.
        for x in range(240,400,32):stamp(bridge.crop((0,0,32,16)),x,128)
        for x,y in [(0,0),(144,0),(352,0),(344,275)]:stamp(tree,x,y)
    else:
        stamp(house,32,32)
        for x in (176,256,336):
            patch(farm,x,80,4,6)
            for y in (96,128):
                for xx in (x+16,x+32):stamp(decor.crop((48,32,64,48)),xx,y)
        patch(water,16,176,6,3)
        for x,y in [(352,0),(96,278)]:stamp(tree,x,y)
    # Border foliage keeps the travel corridor readable.
    for x in range(0,448,32):
        stamp(decor.crop((0,16,16,32)),x,336)
    im.convert('RGB').save(root/'Art'/f'{slug}.png',optimize=True)
