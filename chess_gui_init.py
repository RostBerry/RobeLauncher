from gui_init import *

CHESS_PIECE_IMG_PATH = Config.ConfigData["ChessPiecesImageDict"]

header_img = Image.open(CHESS_PIECE_IMG_PATH + '2/14.png').resize((32, 32))
pg.display.set_icon(pg.image.fromstring(header_img.tobytes(),header_img.size, header_img.mode))

def __load_pieces_img():
    img_dict = {}
    folder = CHESS_PIECE_IMG_PATH + '2/'
    pieces_imgs = os.listdir(folder)

    for img in pieces_imgs:
        path = os.path.join(folder, img)
        piece_img = pg.image.fromstring((pil_image := Image.open(path).resize((CHESS_SQUARE_SIZE, CHESS_SQUARE_SIZE))).tobytes(),
                                        pil_image.size, pil_image.mode).convert_alpha()
        img_dict[int(img.replace('.png', ''))] = piece_img
    return img_dict

CHESS_PIECES_IMG_DICT = __load_pieces_img()