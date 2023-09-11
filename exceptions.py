class InvalidColorError(BaseException):
    pass
class InvalidStartPosError(BaseException):
    pass
class InvalidEndPosError(BaseException):
    pass
class InvalidPiecePromotionError(BaseException):
    pass
class Check(BaseException):
    pass
class WhiteWon(BaseException):
    pass
class BlackWon(BaseException):
    pass
class Stalemate(BaseException):
    pass
class Draw(BaseException):
    pass
class FiftyMovesRule(BaseException):
    pass
class RepetitionOfMoves(BaseException):
    pass
class InsufficientMaterial(BaseException):
    pass
class InvalidInputError(BaseException):
    pass
class ConfigLoadError(BaseException):
    pass