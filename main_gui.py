from gui_init import *


def get_button_by_mouse_pos(self, pos):
    for button in self.all_buttons:
        if button.rect.collidepoint(pos):
            return button
    return None


def colorize_buttons(self, pos):
    for button in self.all_buttons:
        if button.rect.collidepoint(pos):
            button.second_color = Colors.SelectedColor
        else:
            button.second_color = Colors.ButtonColor
        button.update()

class Button(pg.sprite.Sprite):
    def __init__(self, size: tuple, text: str, font_size: int):
        super().__init__()
        self.size = size
        self.w, self.h = self.size
        self.border_size = BUTTON_STROKE_SIZE
        self.image = pg.Surface(self.size)
        self.image.fill(Colors.BackgroundColor)
        self.rect = pg.Rect(0, 0, self.w, self.h)
        self.second_color = PALETTES[Config.GameColorPack][3]
        pg.draw.rect(self.image, self.second_color, self.rect, self.border_size)
        self.text_str = text
        self.__text_font_size = font_size
        self.__text_font = pg.font.Font(Config.FontDict + "NotCourierSans.ttf", self.__text_font_size)
        self.__text = self.__text_font.render(text, True, Colors.RegularTextColor)
        self.image.blit(self.__text, ((self.w - self.__text.get_width()) // 2,
                                      (self.h - self.__text.get_height()) // 2))

    def update(self):
        self.image.fill(Colors.BackgroundColor)
        self.image.blit(self.__text, ((self.w - self.__text.get_width()) // 2,
                                      (self.h - self.__text.get_height()) // 2))
        pg.draw.rect(self.image, self.second_color, (0, 0, self.w, self.h), self.border_size)


class Menu:
    def __init__(self, parent_screen: pg.Surface):
        self.__screen = parent_screen
        self.__screen_size = self.__screen.get_size()
        self.is_game_called = False
        self.game = None
        self.is_options_called = False
        self.is_quit_called = False

        self.all_content = pg.Surface(self.__screen_size).convert_alpha()
        self.all_content.fill(NOTHING)
        self.all_buttons = pg.sprite.Group()
        self.__gap_between_btns = BUTTON_GAP

        pg.display.set_caption('Menu')
        self.__init_fonts()

        self.__released_button = None

        self.__draw_background()
        self.__setup_screen()
        self.__setup_main()

    def __init_fonts(self):
        self.__main_text_font_size = FONT_MAIN_TEXT_SIZE
        self.__main_text_stroke_font_size = round(self.__main_text_font_size * 1.02)
        self.__main_text_font = pg.font.Font(FONT_TEXT_PATH, self.__main_text_font_size)
        self.__main_text_stroke_font = pg.font.Font(FONT_TEXT_PATH, self.__main_text_stroke_font_size)
        self.__main_text_str_1 = 'ROBE'
        self.__main_text_str_2 = 'GAMES'
        self.__main_text_1 = self.__main_text_font.render(self.__main_text_str_1, True, Common.MAIN_COLOR)
        self.__main_text_stroke_1 = self.__main_text_stroke_font.render(self.__main_text_str_1, True,
                                                                      Common.MAIN_STROKE_COLOR)
        self.__main_text_2 = self.__main_text_font.render(self.__main_text_str_2, True, Common.MAIN_COLOR)
        self.__main_text_stroke_2 = self.__main_text_stroke_font.render(self.__main_text_str_2, True, Common.MAIN_STROKE_COLOR)

    def __draw_background(self):
        self.__background = pg.Surface(self.__screen_size)
        self.__background.fill(Common.BACKGROUND)
        self.__screen.blit(self.__background, (0, 0))

    def __setup_screen(self):
        self.all_buttons.empty()
        self.all_content.fill(NOTHING)
        main_text_stroke_1_pos = ((self.__screen_size[0] - self.__main_text_stroke_1.get_width()) // 2,
                                                          -self.__main_text_stroke_1.get_height() // 8)
        main_text_1_pos = ((self.__screen_size[0] - self.__main_text_1.get_width()) // 2,
                                                   -self.__main_text_1.get_height() // 8)
        main_text_stroke_2_pos = ((self.__screen_size[0] - self.__main_text_stroke_2.get_width()) // 2,
                                  main_text_stroke_1_pos[1] + round(self.__main_text_stroke_1.get_height() * 0.6))
        main_text_2_pos = ((self.__screen_size[0] - self.__main_text_2.get_width()) // 2,
                           main_text_1_pos[1] + round(self.__main_text_1.get_height() * 0.6))
        self.all_content.blit(self.__main_text_stroke_1, main_text_stroke_1_pos)
        self.all_content.blit(self.__main_text_1, main_text_1_pos)
        self.all_content.blit(self.__main_text_stroke_2, main_text_stroke_2_pos)
        self.all_content.blit(self.__main_text_2, main_text_2_pos)

    def __setup_main(self):
        self.__setup_screen()
        chess_button = Button(GAME_BTN_SIZE, 'chess', FONT_MAIN_BUTTONS_SIZE)
        chess_button.rect.center = self.__screen.get_rect().center
        self.all_buttons.add(chess_button)

        other_button = Button(GAME_BTN_SIZE, 'other', FONT_MAIN_BUTTONS_SIZE)
        other_button.rect.center = (chess_button.rect.center[0] + chess_button.rect.width + self.__gap_between_btns,
                                    chess_button.rect.center[1])
        self.all_buttons.add(other_button)

    def __setup_other(self):
        self.__setup_screen()
        ttt_button = Button(GAME_BTN_SIZE, 'ttt', FONT_MAIN_BUTTONS_SIZE)
        ttt_button.rect.center = self.__screen.get_rect().center
        self.all_buttons.add(ttt_button)

        back_button = Button(GAME_BTN_SIZE, 'back', FONT_MAIN_BUTTONS_SIZE)
        back_button.rect.center = (ttt_button.rect.center[0] + back_button.rect.width + self.__gap_between_btns,
                                   ttt_button.rect.center[1])
        self.all_buttons.add(back_button)

    def mouse_btn_down(self, button, pos):
        pass

    def mouse_btn_up(self, button, pos):
        if button == 1:
            self.__released_button = get_button_by_mouse_pos(self, pos)
            if self.__released_button is not None:
                released_button_text = self.__released_button.text_str
                if released_button_text in ('chess', 'shogi', 'checkers', 'ttt'):
                    self.game = released_button_text
                    self.is_game_called = True
                elif released_button_text == 'other':
                    self.__setup_other()
                elif released_button_text == 'back':
                    self.__setup_main()
            self.__released_button = None

    def mouse_motion(self, pos):
        colorize_buttons(self, pos)

    def grand_update(self):
        self.__background.fill(Common.BACKGROUND)
        self.__screen.blit(self.__background, (0, 0))
        self.__screen.blit(self.all_content, (0, 0))
        self.all_buttons.draw(self.__screen)
        pg.display.update()


class GameMode:
    def __init__(self, parent_screen: pg.Surface, game: str):
        self.__screen = parent_screen
        self.__screen_size = self.__screen.get_size()
        self.is_game_start_called = False
        self.back = False
        self.game = game

        self.all_buttons = pg.sprite.Group()
        self.all_content = pg.Surface(self.__screen_size).convert_alpha()

        self.__released_button = None

        self.__init_fonts()

        self.__draw_background()
        if self.game == 'chess':
            self.__setup_chess_screen()
        elif self.game == 'shogi':
            self.__setup_shogi_screen()
        elif self.game == 'checkers':
            self.__setup_checkers_screen()
        elif self.game == 'ttt':
            self.__setup_ttt_screen()

    def __init_fonts(self):
        self.__main_text_font_size = FONT_MAIN_TEXT_SIZE
        self.__header_text_font_size = FONT_MAIN_TEXT_SIZE // 2
        self.__main_text_stroke_font_size = round(self.__main_text_font_size * 1.02)
        self.__main_text_font = pg.font.Font(FONT_TEXT_PATH, self.__main_text_font_size)
        self.__header_text_font = pg.font.Font(FONT_TEXT_PATH, self.__header_text_font_size)
        self.__main_text_stroke_font = pg.font.Font(FONT_TEXT_PATH, self.__main_text_stroke_font_size)
        self.__main_text_str = 'GAME'
        self.__header_text_str = self.game.upper() if self.game != 'ttt' else 'TIC TAC TOE'
        self.__main_text = self.__main_text_font.render(self.__main_text_str, True, Common.MAIN_COLOR)
        self.__main_text_stroke = self.__main_text_stroke_font.render(self.__main_text_str, True,
                                                                      Common.MAIN_STROKE_COLOR)
        self.__header_text = self.__header_text_font.render(self.__header_text_str, True, Common.MAIN_STROKE_COLOR)

    def __draw_background(self):
        self.__background = pg.Surface(self.__screen_size)
        self.__background.fill(Common.BACKGROUND)
        self.__screen.blit(self.__background, (0, 0))

    def __draw_play_button(self):
        play_button = Button(MAIN_BTN_SIZE, 'PLAY', FONT_MAIN_BUTTONS_SIZE)
        play_button.rect.center = (self.__screen_size[0] // 2,
                                   self.__screen_size[1] - play_button.rect.height)
        self.all_buttons.add(play_button)

    def __setup_screen(self):
        self.all_buttons.empty()
        self.all_content.fill(NOTHING)
        main_text_stroke_pos = ((self.__screen_size[0] - self.__main_text_stroke.get_width()) // 2,
                                -self.__main_text_stroke.get_height() // 8)
        main_text_pos = ((self.__screen_size[0] - self.__main_text.get_width()) // 2,
                           -self.__main_text.get_height() // 8)
        header_text_pos = ((self.__screen_size[0] - self.__header_text.get_width()) // 2,
                           main_text_stroke_pos[1] + round(self.__main_text.get_height() * 0.8))
        self.all_content.blit(self.__main_text_stroke, main_text_stroke_pos)
        self.all_content.blit(self.__main_text, main_text_pos)
        self.all_content.blit(self.__header_text, header_text_pos)

    def __setup_chess_screen(self):
        self.__setup_screen()
        self.__draw_play_button()

    def __setup_ttt_screen(self):
        self.__setup_screen()
        self.__draw_play_button()


    def mouse_btn_down(self, button, pos):
        pass

    def mouse_btn_up(self, button, pos):
        if button == 1:
            self.__released_button = get_button_by_mouse_pos(self, pos)
            if self.__released_button is not None:
                released_btn_text = self.__released_button.text_str
                if released_btn_text == 'PLAY':
                    self.is_game_start_called = True
            self.__released_button = None

    def mouse_motion(self, pos):
        colorize_buttons(self, pos)

    def grand_update(self):
        self.__screen.blit(self.__background, (0, 0))
        self.__screen.blit(self.all_content, (0, 0))
        self.all_buttons.draw(self.__screen)
        pg.display.update()