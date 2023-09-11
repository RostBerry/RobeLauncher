from chess_engine import *
from chess_gui_init import *

class ChessGUI:
    def __init__(self, parent_screen: pg.Surface, game_mode):
        self.__screen = parent_screen
        self.__screen_size = self.__screen.get_size()
        self.__square_size = CHESS_SQUARE_SIZE
        self.engine = ChessEngine(game_mode)
        self.__square_count = self.engine.square_count

        self.__square_subscription = SQUARE_SUBSCRIPT
        self.__do_available_square_drawing = AVAILABLE_SQUARE_DRAWING
        self.all_pieces = {}
        self.all_available_squares = {}  #All squares to draw when piece clicked

        self.__lmb_pressed_square_id = None
        self.__lmb_released_square_id = None
        self.__picked_piece = None
        self.__piece_choice = None
        self.__piece_to_promote = None

        self.recorded_game_move = 0
        self.is_recorded_game_done = False
        self.move_played = None
        self.__time_for_bot_move = TIME_FOR_CHESS_BOT_MOVE

        pg.display.set_caption('Chess Session')

        self.back = False

        self.__init_event()
        self.__init_fonts()
        self.__draw_background()
        self.__setup_chessboard()
        self.__setup_pieces()
        self.__time = 0

    def __init_fonts(self):
        self.__chessboard_font_size = FONT_CHESSBOARD_SIZE
        self.__chessboard_font = pg.font.Font(FONT_CHESSBOARD_PATH, self.__chessboard_font_size)
        self.__square_subscript_font_size = SQUARE_SUBSCRIPT_FONT_SIZE
        self.__square_subscript_font = pg.font.Font(SQUARE_SUBSCRIPT_FONT_PATH, self.__square_subscript_font_size)
        self.__square_subscript_font_color = Common.SQUARE_SUBSCRIPT_COLOR

    def __init_event(self):
        self.move_played = False
        if self.engine.gamemode == 'PvP':
            self.both_sides = True
            self.do_bot_play = False
            self.do_bot_response = False
            self.color = None
        elif self.engine.gamemode == 'PvB':
            self.both_sides = False
            self.do_bot_play = False
            self.do_bot_response = True
            self.color = True
        elif self.engine.gamemode == 'BvP':
            self.both_sides = False
            self.do_bot_play = True
            self.do_bot_response = False
            self.color = False
        elif self.engine.gamemode == 'BvB':
            self.both_sides = False
            self.do_bot_play = True
            self.do_bot_response = True
            self.color = None

    def __draw_background(self):
        self.__background = pg.Surface(self.__screen_size)
        self.__background.fill(Common.BACKGROUND)
        self.__screen.blit(self.__background, (0, 0))

    def __setup_chessboard(self):
        self.__chessboard_width = self.__square_count * self.__square_size
        self.__letters_row, self.__num_column = self.__create_nums_and_letters()
        self.__letters_row_height = self.__letters_row.get_height()
        self.__num_column_width = self.__num_column.get_width()

        self.all_squares = self.__create_all_squares()

        self.__chessboard = pg.Surface((2 * self.__num_column_width + self.__chessboard_width,
                                        2 * self.__letters_row_height + self.__chessboard_width))
        self.__chessboard.fill(Common.BOARD_BACKGROUND_COLOR)
        self.__chessboard.blit(self.__num_column, (0, self.__letters_row_height))
        self.__chessboard.blit(self.__num_column, (self.__num_column_width + self.__chessboard_width,
                                                   self.__letters_row_height))
        self.__chessboard.blit(self.__letters_row, (self.__num_column_width, 0))
        self.__chessboard.blit(self.__letters_row, (self.__num_column_width,
                                                    self.__letters_row_height + self.__chessboard_width))

        self.chessboard_rect = self.__chessboard.get_rect()
        self.chessboard_rect.x += (self.__screen_size[0] - self.chessboard_rect.width) // 2
        self.chessboard_rect.y += (self.__screen_size[1] - self.chessboard_rect.height) // 2
        self.__chessboard_pos = (self.chessboard_rect.x, self.chessboard_rect.y)

        square_pos_offset = (self.chessboard_rect.x + self.__num_column_width,
                             self.chessboard_rect.y + self.__letters_row_height)
        for square in self.all_squares.values():
            square.rect.x += square_pos_offset[0]
            square.rect.y += square_pos_offset[1]


    def __create_nums_and_letters(self):
        row = pg.Surface((self.__square_count * self.__square_size, self.__square_size // 2)).convert_alpha()
        row_height = row.get_height()
        column = pg.Surface((self.__square_size // 2, self.__square_count * self.__square_size)).convert_alpha()
        column_width = column.get_width()
        row.fill(NOTHING)
        column.fill(NOTHING)
        for i in range(self.__square_count):
            letter = self.__chessboard_font.render(LETTERS[i].upper(),
                                                   True,
                                                   Common.BOARD_TEXT_COLOR)
            number = self.__chessboard_font.render(str(self.__square_count - i), True,
                                                   Common.BOARD_TEXT_COLOR)

            row.blit(letter,
                     (i * self.__square_size + (self.__square_size - letter.get_width()) // 2,
                     (row_height - letter.get_height()) // 2
            ))

            column.blit(number,
                        ((column_width - number.get_width()) // 2,
                        i * self.__square_size + (self.__square_size - number.get_height()) // 2
            ))

        return row, column

    def __create_all_squares(self):
        all_squares = {}
        square_color = False

        i = 1
        for square_id in self.engine.all_squares:
            all_squares[square_id] = ChessSquareSprite(square_color,
                                                       self.__square_size,
                                                       square_id,
                                                       self.engine.from_square_id(square_id),
                                                       self.__square_subscript_font,
                                                       self.__square_subscript_font_color,
                                                       self.__square_subscription)
            square_color ^= True
            square_color = square_color ^ True if i % 8 == 0 else square_color
            i += 1
        return all_squares

    def __setup_pieces(self):
        self.all_pieces = {}
        for square_id in self.engine.all_pieces:
            piece = PieceSprite(self.engine.all_pieces[square_id].name)
            piece.rect = self.all_squares[square_id].rect.copy()
            self.all_pieces[square_id] = piece

    def make_move(self, move, to_invert_move_played: bool = True):
        # start_time = time.perf_counter()
        move_done_successfully = True
        try:
            self.engine.make_move(move)
            if to_invert_move_played:
                self.move_played ^= True
        except InvalidPiecePromotionError:
            self.__picked_piece.rect.center = self.all_squares[self.__lmb_pressed_square_id].rect.center
            move_done_successfully = False
            print('promotion error')
        except InvalidColorError:
            self.__picked_piece.rect.center = self.all_squares[self.__lmb_pressed_square_id].rect.center
            print('invalid color')
            move_done_successfully = False
        except InvalidEndPosError:
            self.__picked_piece.rect.center = self.all_squares[self.__lmb_pressed_square_id].rect.center
            print('invalid end pos')
            move_done_successfully = False
        except InvalidStartPosError:
            self.__picked_piece.rect.center = self.all_squares[self.__lmb_pressed_square_id].rect.center
            print('invalid start pos')
            move_done_successfully = False
        except Check:
            self.engine.print_board()
            print('Check')
            if to_invert_move_played:
                self.move_played ^= True
        except WhiteWon:
            self.engine.print_board()
            print('White Won')
        except BlackWon:
            self.engine.print_board()
            print('Black Won')
        except Stalemate:
            self.engine.print_board()
            print('Stalemate')
        except RepetitionOfMoves:
            self.engine.print_board()
            print('Draw by repetition of moves')
        except InsufficientMaterial:
            self.engine.print_board()
            print('Draw by insufficient material for a win')
        except FiftyMovesRule:
            self.engine.print_board()
            print('Draw by fifty moves rule')

        if move_done_successfully:
            self.__setup_pieces()
            # print(self.engine.bot.get_perft(self.engine.turn))
            # print(self.engine.w_pinned_squares_y, self.engine.w_pinned_squares_x, self.engine.w_squares_in_check, self.engine.b_pinned_squares_y, self.engine.b_pinned_squares_x, self.engine.b_squares_in_check)
            self.recorded_game_move += 1
        # print(time.perf_counter() - start_time)


    def make_move_from_uci(self, uci_move):
        try:
            self.engine.input(f'go uci {uci_move}')
        except (InvalidColorError, InvalidEndPosError, InvalidPiecePromotionError):
            print('huyuhen')
            pg.quit()
        except Check:
            print('Check')
        except WhiteWon:
            print('White Won')
        except BlackWon:
            print('Black Won')
        except Stalemate:
            print('Stalemate')
        except RepetitionOfMoves:
            print('Draw by repetition of moves')
        except InsufficientMaterial:
            print('Draw by insufficient material for a win')
        except FiftyMovesRule:
            print('Draw by fifty moves rule')

        self.__setup_pieces()
        self.recorded_game_move += 1

    def play_event(self):
        time_spent = self.__time
        skip_time_check = True
        if 0.01 > time_spent % self.__time_for_bot_move > 0.0 or skip_time_check:
            if not self.move_played:
                if self.do_bot_play:
                    if self.engine.game_status == 'playing':
                        self.grand_update()
                        self.make_move(self.engine.bot.get_best_move()[0], False)
                        self.__setup_pieces()
                        self.move_played = True
            else:
                if self.do_bot_response:
                    if self.engine.game_status == 'playing':
                        self.grand_update()
                        self.make_move(self.engine.bot.get_best_move()[0], False)
                        self.__setup_pieces()
                        self.move_played = False

    def __clear_pressed_buffer(self):
        self.__picked_piece = None
        self.__lmb_pressed_square_id = None
        self.__lmb_released_square_id = None
        self.all_available_squares = {}
        self.__piece_choice = None
        self.__piece_to_promote = None

    def __return_picked_piece_to_home(self):
        if self.__picked_piece is not None:
            self.__picked_piece.rect.center = self.all_squares[self.__lmb_pressed_square_id].rect.center
            self.__clear_pressed_buffer()

    def __get_square_id_by_mouse_pos(self, pos):
        for square_id in self.all_squares:
            if self.all_squares[square_id].rect.collidepoint(pos):
                return square_id
        return None

    def __get_promotion_piece_by_mouse_pos(self, pos):
        for promotion_piece in self.__piece_choice.all_promotion_pieces:
            if promotion_piece.rect.collidepoint(pos):
                return promotion_piece.name
        return None

    def mouse_btn_down(self, button, pos):
        if self.__piece_choice is None:
            if button == 1:
                self.__lmb_pressed_square_id = self.__get_square_id_by_mouse_pos(pos)
        if button == 1:
            if self.__piece_choice is not None:
                pass
            elif self.__lmb_pressed_square_id in self.all_pieces:
                self.__picked_piece = self.all_pieces[self.__lmb_pressed_square_id]
                self.__picked_piece.rect.center = pos
                color_condition = (True if self.both_sides else self.engine.turn == self.color)
                if self.engine.all_pieces[self.__lmb_pressed_square_id].color == self.engine.turn and color_condition:
                    if self.__do_available_square_drawing:
                        if self.engine.game_status == 'playing':
                            self.__draw_available_squares()
        elif button == 3:
            self.__return_picked_piece_to_home()

    def mouse_btn_up(self, button, pos):
        if self.__piece_choice is None:
            color_condition = (True if self.both_sides else self.engine.turn == self.color)
            if self.engine.game_status == 'playing' and color_condition:
                self.__lmb_released_square_id = self.__get_square_id_by_mouse_pos(pos)
        if button == 1:
            if self.__piece_choice is not None:
                self.__piece_to_promote = self.__get_promotion_piece_by_mouse_pos(pos)
                if self.__piece_to_promote is not None:
                    self.__piece_choice = None
                    self.make_move((self.__lmb_pressed_square_id, self.__lmb_released_square_id + (self.__piece_to_promote,)))
                    self.__clear_pressed_buffer()
            elif self.__lmb_released_square_id in self.all_squares:
                if self.__lmb_released_square_id != self.__lmb_pressed_square_id:
                    if self.__picked_piece is not None:
                        self.__picked_piece.rect.center = self.all_squares[self.__lmb_released_square_id].rect.center
                        can_promote = False
                        if (self.__picked_piece.name in PAWN
                            and self.__lmb_released_square_id[1] == (8 if self.engine.turn else 1)):
                            for available_square in self.engine.get_available_squares(self.__lmb_pressed_square_id):
                                if self.__lmb_released_square_id == available_square[:-1]:
                                    can_promote = True
                                    break
                            if can_promote:
                                color = self.engine.all_pieces[self.__lmb_pressed_square_id].color
                                self.__piece_choice = PieceChoice(self.__square_size, color)
                                if color:
                                    self.__piece_choice.rect.topleft = self.all_squares[self.__lmb_released_square_id].rect.topleft
                                else:
                                    self.__piece_choice.rect.bottomleft = self.all_squares[self.__lmb_released_square_id].rect.bottomleft
                                self.__piece_choice.renew_promotion_pieces_rects()

                        if not can_promote:
                            self.make_move((self.__lmb_pressed_square_id, self.__lmb_released_square_id + (self.__piece_to_promote,)))
                            self.__clear_pressed_buffer()
                else:
                    self.__return_picked_piece_to_home()
            else:
                self.__return_picked_piece_to_home()



    def mouse_motion(self, pos):
        if self.__piece_choice is not None:
            for background in self.__piece_choice.all_promotion_pieces_backgrounds:
                if background.rect.collidepoint(pos):
                    background.image.fill(Common.PIECE_CHOOSING_BACKGROUND_SELECTED_COLOR)
                else:
                    background.image.fill(Common.PIECE_CHOOSING_BACKGROUND_COLOR)
            self.__piece_choice.image.fill(Common.PIECE_CHOOSING_BACKGROUND_COLOR)
            for i, promotion_piece in enumerate(self.__piece_choice.all_promotion_pieces):
                promotion_piece_pos = (promotion_piece.rect.x - self.__piece_choice.rect.x,
                                       promotion_piece.rect.y - self.__piece_choice.rect.y)
                promotion_piece_background_pos = (promotion_piece_pos[0] + self.__piece_choice.promotion_piece_background_frame_border // 2,
                                                  promotion_piece_pos[1] + self.__piece_choice.promotion_piece_background_frame_border // 2)
                self.__piece_choice.image.blit(self.__piece_choice.all_promotion_pieces_backgrounds[i].image, promotion_piece_background_pos)
                self.__piece_choice.image.blit(promotion_piece.image, promotion_piece_pos)
        elif self.__picked_piece is not None:
            self.__picked_piece.rect.center = pos

    def keyboard_btn_down(self, event):
        if self.is_recorded_game_done:
            if event.key == pg.K_LEFT:
                self.engine.input('<')
                self.__setup_pieces()

            if event.key == pg.K_RIGHT:
                self.engine.input('>')
                self.__setup_pieces()

    def __draw_available_squares(self):
        for available_square in self.engine.get_available_squares(self.__lmb_pressed_square_id):
            available_square_mark = AvailableSquareSprite(True if available_square[:-1] in self.all_pieces else False)
            available_square_mark.rect = self.all_squares[available_square[:-1]].rect.copy()
            self.all_available_squares[available_square[:-1]] = available_square_mark

    def grand_update(self):
        self.__screen.blit(self.__background, (0, 0))
        self.__screen.blit(self.__chessboard, self.__chessboard_pos)
        for square in self.all_squares.values():
            self.__screen.blit(square.image, square.rect.topleft)
        for available_square in self.all_available_squares.values():
            self.__screen.blit(available_square.image, available_square.rect.topleft)
        for piece in self.all_pieces.values():
            if piece != self.__picked_piece:
                self.__screen.blit(piece.image, piece.rect.topleft)
        if self.__picked_piece is not None:
            self.__screen.blit(self.__picked_piece.image, self.__picked_piece.rect.topleft)
        if self.__piece_choice is not None:
            self.__screen.blit(self.__piece_choice.image, self.__piece_choice.rect.topleft)
        pg.display.update()
        self.__time += 1/FPS


class ChessSquareSprite(pg.sprite.Sprite):
    def __init__(self, color: bool, size: int, square_id: tuple, name: str,
                 font: pg.font.Font, font_color: tuple, to_subscript: bool):
        super().__init__()
        x, y = square_id
        self.color = Common.SQUARE_COLORS[color]
        self.square_name = name
        self.font = font
        self.font_color = font_color
        self.image = pg.Surface((size, size))
        self.image.fill(self.color)
        self.to_subscript = to_subscript
        self.rect = pg.Rect((x - 1) * size, size * (8 - y), size, size)
        if self.to_subscript:
            subscription = self.font.render(self.square_name, True, self.font_color)
            self.image.blit(subscription,
                            ((self.rect.width - subscription.get_width()) // 2,
                             (self.rect.height - subscription.get_height()) // 2))


class PieceSprite(pg.sprite.Sprite):
    def __init__(self, name: str):
        super().__init__()
        self.image = CHESS_PIECES_IMG_DICT[name]
        self.name = name


class AvailableSquareSprite(pg.sprite.Sprite):
    def __init__(self, square_type: bool):
        super().__init__()
        self.image = TAKEABLE_SURF if square_type else AVAILABLE_SURF


class PieceChoice(pg.sprite.Sprite):
    def __init__(self, size: int, color: bool):
        super().__init__()
        self.size = size
        self.color = color
        self.image = pg.Surface((self.size, self.size * len(PIECES_TO_PROMOTE)))
        self.image.fill(Common.PIECE_CHOOSING_BACKGROUND_COLOR)
        self.rect = pg.Rect(0, 0, self.size, self.size * len(PIECES_TO_PROMOTE))
        self.all_promotion_pieces = []
        self.all_promotion_pieces_backgrounds = []
        self.promotion_piece_background_frame_border = PROMOTION_PIECE_BACKGROUND_FRAME_BORDER
        for i, piece_name in enumerate(PIECES_TO_PROMOTE):
            piece_to_choose = PieceToChoose(self.size, piece_name if self.color else piece_name.lower())
            piece_background = PieceToChooseBackground(self.size - self.promotion_piece_background_frame_border)
            if self.color:
                pos = (self.rect.x, self.rect.y + i * self.size)
                piece_to_choose.rect.topleft = pos
            else:
                pos = (self.rect.x, self.rect.y + self.rect.height - (i + 1) * self.size)
                piece_to_choose.rect.topleft = pos
            piece_background.rect.topleft = (piece_to_choose.rect.topleft[0] + self.promotion_piece_background_frame_border // 2,
                                             piece_to_choose.rect.topleft[1] + self.promotion_piece_background_frame_border // 2)
            self.all_promotion_pieces.append(piece_to_choose)
            self.all_promotion_pieces_backgrounds.append(piece_background)
            self.image.blit(piece_background.image, pos)
            self.image.blit(piece_to_choose.image, pos)

    def renew_promotion_pieces_rects(self):
        for i, promotion_piece in enumerate(self.all_promotion_pieces):
            if self.color:
                promotion_piece.rect.topleft = (self.rect.x, self.rect.y + i * self.size)
            else:
                promotion_piece.rect.topleft = (self.rect.x, self.rect.y + self.rect.height - (i + 1) * self.size)
            piece_background = self.all_promotion_pieces_backgrounds[i]
            piece_background.rect.topleft = (promotion_piece.rect.topleft[0] + self.promotion_piece_background_frame_border // 2,
                                             promotion_piece.rect.topleft[1] + self.promotion_piece_background_frame_border // 2)


class PieceToChoose(pg.sprite.Sprite):
    def __init__(self, size, name):
        super().__init__()
        self.size = size
        self.name = name
        self.image = CHESS_PIECES_IMG_DICT[self.name].convert_alpha()
        self.rect = pg.Rect(0, 0, self.size, self.size)


class PieceToChooseBackground(pg.sprite.Sprite):
    def __init__(self, size):
        super().__init__()
        self.size = size
        self.image = pg.Surface((size, size))
        self.image.fill(Common.PIECE_CHOOSING_BACKGROUND_COLOR)
        self.rect = pg.Rect(0, 0, self.size, self.size)
