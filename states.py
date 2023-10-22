from enum import Enum

class GameMode(Enum):
    PvP = 0
    PvB = 1
    BvP = 2
    BvB = 3

class GameStatus(Enum):
    Running = 0
    WhiteWon = 1
    BlackWon = 2
    Draw = 3

class ColorPack(Enum):
    Cyan = 0
    Gray = 1
    Pink = 2
    Orange = 3
    Green = 4