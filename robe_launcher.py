from chess_gui import *
# from ttt_gui import *
# from shogi_gui import *

class RobeLauncher:
    def __init__(self):
        self.chess = None
        self.shogi = None
        self.checkers = None
        self.ttt = None
        self.options = None
        self.game_mode = None
        self.menu = Menu(screen)

        self.recorded_chess_game = None
        self.recorded_ttt_game = None

        self.start_loop()

    def start_loop(self):
        run = True
        try:
            while run:
                for event in pg.event.get():
                    if event.type == pg.QUIT:
                        run = False

                    if event.type == pg.MOUSEBUTTONDOWN:
                        if self.menu is not None:
                            self.menu.mouse_btn_down(event.button, event.pos)
                        elif self.options is not None:
                            self.options.mouse_btn_down(event.button, event.pos)
                        elif self.game_mode is not None:
                            self.game_mode.mouse_btn_down(event.button, event.pos)
                        elif self.chess is not None:
                            self.chess.mouse_btn_down(event.button, event.pos)
                        elif self.ttt is not None:
                            self.ttt.mouse_btn_down(event.button, event.pos)

                    if event.type == pg.MOUSEBUTTONUP:
                        if self.menu is not None:
                            self.menu.mouse_btn_up(event.button, event.pos)
                        elif self.options is not None:
                            pass
                        elif self.game_mode is not None:
                            self.game_mode.mouse_btn_up(event.button, event.pos)
                        elif self.chess is not None:
                            self.chess.mouse_btn_up(event.button, event.pos)
                        elif self.ttt is not None:
                            self.ttt.mouse_btn_up(event.button, event.pos)

                    if event.type == pg.MOUSEMOTION:
                        if self.menu is not None:
                            self.menu.mouse_motion(event.pos)
                        elif self.options is not None:
                            self.options.mouse_motion(event.pos)
                        elif self.game_mode is not None:
                            self.game_mode.mouse_motion(event.pos)
                        elif self.chess is not None:
                            self.chess.mouse_motion(event.pos)
                        elif self.ttt is not None:
                            self.ttt.mouse_motion(event.pos)

                    if event.type == pg.KEYDOWN:
                        if self.menu is not None:
                            self.menu.keyboard_btn_down(event)
                        elif self.options is not None:
                            self.options.keyboard_btn_down(event)
                        elif self.chess is not None:
                            self.chess.keyboard_btn_down(event)

                    if event.type == pg.KEYUP:
                        pass

                    if self.menu is not None:

                        if self.menu.is_game_called:
                            self.game_mode = GameMode(screen, self.menu.game)
                            self.menu = None

                        elif self.menu.is_options_called:
                            self.menu = None
                            self.options = Options(screen)

                        elif self.menu.is_quit_called:
                            run = False

                    elif self.options is not None:

                        if self.options.back:
                            self.options = None
                            self.menu = Menu(screen)

                    elif self.chess is not None:

                        if self.chess.back:
                            self.chess = None
                            self.menu = Menu(screen)

                    elif self.game_mode is not None:

                        if self.game_mode.back:
                            self.game_mode = None
                            self.menu = Menu(screen)

                        if self.game_mode.is_game_start_called:
                            if self.game_mode.game == 'chess':
                                self.chess = ChessGUI(screen, GAME_MODE_CHESS)
                                if self.recorded_chess_game is None:
                                    self.chess.is_recorded_game_done = True
                            elif self.game_mode.game == 'shogi':
                                pass
                            elif self.game_mode.game == 'checkers':
                                pass
                            elif self.game_mode.game == 'ttt':
                                self.ttt = TTTGUI(screen, GAME_MODE_TTT)
                            self.game_mode = None

                if self.menu is not None:
                    self.menu.grand_update()

                elif self.game_mode is not None:
                    self.game_mode.grand_update()

                elif self.chess is not None:
                    if self.recorded_chess_game is not None:
                        if self.chess.recorded_game_move < len(self.recorded_chess_game):
                            if not self.chess.is_recorded_game_done:
                                self.chess.make_move_from_uci(self.recorded_chess_game[self.chess.recorded_game_move])
                        else:
                            self.chess.is_recorded_game_done = True
                    else:
                        self.chess.play_event()
                    self.chess.grand_update()
                    if self.recorded_chess_game is not None or GAME_MODE_CHESS == 'BvB':
                        if self.chess.recorded_game_move == 1:
                            time.sleep(0.5)

                elif self.ttt is not None:
                    if self.recorded_ttt_game is not None:
                        if self.ttt.recorded_game_move < len(self.recorded_ttt_game):
                            if not self.ttt.is_recorded_game_done:
                                self.ttt.make_move_from_rec_game(self.recorded_ttt_game[self.ttt.recorded_game_move])
                        else:
                            self.ttt.is_recorded_game_done = True
                    else:
                        self.ttt.play_event()
                    self.ttt.grand_update()
                    if self.ttt.recorded_game_move == 1:
                        time.sleep(0.5)
                clock.tick(FPS)
        except KeyboardInterrupt:
            pass
        pg.quit()