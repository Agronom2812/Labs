from pygments import lex
from pygments.lexers import CSharpLexer
from pygments.token import Token

def all_operators_parsing(code):

    tokens = lex(code, CSharpLexer())
    count = 0

    operator_values = {
        "+", "-", "*", "/", "%", "&", "|", "^", "<<", ">>", "==", "!=",
        "<", "<=", ">", ">=", "&&", "||", "!", "=", "+=", "-=", "*=",
        "/=", "%=", "&=", "|=", "^=", "<<=", ">>=", "=>", "?.", "?[",
        "??", "??=", "if", "else", "for", "foreach", "while", "do",
        "switch", "case", "break", "continue", "return", "throw", "try",
        "catch", "finally", "goto", "yield", "await"
    }

    else_flag = False

    for token_type, value in tokens:

        if value == "else":
            else_flag = True
            continue
        if else_flag and (value != " " or token_type != Token.Text.Whitespace):
            else_flag = False
            if value == "if":
                count += 1
                continue

        if token_type in Token.Operator or value in operator_values:
            count += 1

    return count
