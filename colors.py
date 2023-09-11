from consts import PALETTES

class Colors:
    LightSquareColor = None
    DarkSquareColor = None
    BoardBackgroundColor = None
    BackgroundColor = None
    MainTextColor = None
    MainTextStrokeColor = None
    SelectedColor = None
    ButtonColor = None
    RegularTextColor = None
    BoardEdgeTextColor = None
    SquareSubscriptColor = None

    @staticmethod
    def update(config):
        Colors.LightSquareColor = PALETTES[config.GameColorPack][0]
        Colors.DarkSquareColor = PALETTES[config.GameColorPack][2]
        Colors.BoardBackgroundColor = PALETTES[config.GameColorPack][3]
        Colors.BackgroundColor = PALETTES[config.GameColorPack][1]
        Colors.MainTextColor = PALETTES[config.GameColorPack][0]
        Colors.MainTextStrokeColor = PALETTES[config.GameColorPack][3]
        Colors.SelectedColor = PALETTES[config.GameColorPack][2]
        Colors.ButtonColor = PALETTES[config.GameColorPack][1]
        Colors.RegularTextColor = PALETTES[config.GameColorPack][2]
        Colors.BoardEdgeTextColor = PALETTES[config.GameColorPack][1]
        Colors.SquareSubscriptColor = PALETTES[config.GameColorPack][3]