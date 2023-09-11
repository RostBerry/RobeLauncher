from consts import *
from config_load import Config
from colors import *
import pygame as pg
from PIL import Image
import os

Config.load_from_txt()
FONT_CHESSBOARD_EDGE_PATH = Config.FontDict + ARIAL_BLACK
Colors.update(Config)

pg.init()
SCREEN_WIDTH = Config.ConfigData["ScreenWidth"]
SCREEN_HEIGHT = Config.ConfigData["ScreenHeight"]
screen = pg.display.set_mode((SCREEN_WIDTH, SCREEN_HEIGHT))
pg.display.set_caption('Loading')
clock = pg.time.Clock()

# AVAILABLE_IMG = Image.open(IMG_PATH + OTHER_IMG_PATH + 'available.png').resize((CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))
AVAILABLE_SURF = pg.Surface((2048, 2048), pg.SRCALPHA).convert_alpha()
AVAILABLE_SURF.fill(NOTHING)
available_width = AVAILABLE_SURF.get_width()
available_height = AVAILABLE_SURF.get_height()
pg.draw.circle(AVAILABLE_SURF, BLACK + (70,), (available_width // 2,
                                       available_height // 2), available_width // 5)

CHESS_SQUARE_SIZE = Config.ConfigData["ChessSquareSize"]
AVAILABLE_SURF = pg.transform.smoothscale(AVAILABLE_SURF, (CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))

# TAKEABLE_IMG = Image.open(IMG_PATH + OTHER_IMG_PATH + 'takeable.png').resize((CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))
TAKEABLE_SURF = pg.Surface((2048, 2048), pg.SRCALPHA).convert_alpha()
TAKEABLE_SURF.fill(BLACK + (70,))
takeable_width = TAKEABLE_SURF.get_width()
takeable_height = TAKEABLE_SURF.get_height()
pg.draw.circle(TAKEABLE_SURF, NOTHING, (takeable_width // 2,
                                             takeable_height // 2), takeable_width // 2)

TAKEABLE_SURF = pg.transform.smoothscale(TAKEABLE_SURF, (CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))

# MARK_IMG = Image.open(IMG_PATH + OTHER_IMG_PATH + 'mark.png').resize((CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))
MARK_SURF = pg.Surface((CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE), pg.SRCALPHA).convert_alpha()
MARK_SURF.fill((0, 0, 0, 0))
pg.draw.circle(MARK_SURF, (0, 200, 0), (MARK_SURF.get_width() // 2,
                                           MARK_SURF.get_height() // 2), CHESS_SQUARE_SIZE // 3, 6)

# MOVABLE_IMG_DICT = {'available': AVAILABLE_IMG,
#                     'takeable': TAKEABLE_IMG,
#                     'mark': MARK_IMG}

# pg.mixer.music.load(MUSIC_PATH + BACKGROUND_MUSIC)
# pg.mixer.music.set_volume(0.3)
# pg.mixer.music.play(-1)

func_keys = [pg.K_LCTRL, pg.K_RCTRL, pg.K_v, pg.K_RETURN, pg.K_BACKSPACE, pg.K_c]