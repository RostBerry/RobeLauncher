import json
from states import *

class Config:

    ConfigData = {}

    ScreenSize = None
    ChessSquareSize = None
    TicTacToeSquareSize = None
    ShogiSquareSize = None
    FPS = None
    DoFpsDrawing = None
    DoSquareSubscript = None
    DoAvailableSquareDrawing = None
    DoTerminalOutput = None
    GameModeChess = None
    GameModeShogi = None
    GameModeTTT = None
    ChessPiecesImageDict = None
    FontDict = None
    ChessPiecesPack = None
    GameColorPack = None

    @staticmethod
    def __process_value(value: str):
        if value.isnumeric():
            return int(value)
        return value.replace('"', "")

    @staticmethod
    def load_from_txt():
        with open("config.txt", "r") as config:
            lines = config.readlines()


        for line in lines:
            key, value = line.strip().split(" ")
            Config.ConfigData[key] = Config.__process_value(value)

        with open("config.json", "w") as config:
            json.dump(Config.ConfigData, config, indent=2)

        Config.update_all()

    @staticmethod
    def __reverse_process_value(value):
        if isinstance(value, str):
            return '"' + value + '"'

        return str(value)

    @staticmethod
    def load_to_txt():
        with open("config.txt", "w") as config:
            for key, value in Config.ConfigData.items():
                config.write(f"{key} {Config.__reverse_process_value(value)}\n")

    @staticmethod
    def update_all():
        Config.ScreenSize = (Config.ConfigData["ScreenWidth"], Config.ConfigData["ScreenHeight"])
        Config.ChessSquareSize = Config.ConfigData["ChessSquareSize"]
        Config.ShogiSquareSize = Config.ConfigData["ShogiSquareSize"]
        Config.TicTacToeSquareSize = Config.ConfigData["TicTacToeSquareSize"]
        Config.FPS = Config.ConfigData["FPS"]
        Config.DoSquareSubscript = Config.ConfigData["DoSquareSubscript"] == "true"
        Config.DoFpsDrawing = Config.ConfigData["DoFpsDrawing"] == "true"
        Config.DoAvailableSquareDrawing = Config.ConfigData["DoAvailableSquareDrawing"] == "true"
        Config.DoTerminalOutput = Config.ConfigData["DoTerminalOutput"] == "true"
        Config.GameModeChess = Config.ConfigData["GameModeChess"]
        Config.GameModeShogi = Config.ConfigData["GameModeShogi"]
        Config.GameModeTTT = Config.ConfigData["GameModeTicTacToe"]
        Config.ChessPiecesImageDict = Config.ConfigData["ChessPiecesImageDict"]
        Config.FontDict = Config.ConfigData["FontDict"]
        Config.ChessPiecesPack = Config.ConfigData["ChessPiecesPack"]
        Config.GameColorPack = Config.ConfigData["GameColorPack"]