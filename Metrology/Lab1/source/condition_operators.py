from pygments import lex
from pygments.lexers import CSharpLexer
from pygments.token import Token

def condition_operators(code):

    tokens = lex(code, CSharpLexer())
    count = 0

    operator_values = {
        "if", "else", "for", "foreach", "while", "do", "case"
    }


    else_flag = False

    do_flag = False


    for token_type, value in tokens:

        if value == "else":
            else_flag = True
            continue
        if else_flag and (value != " " or token_type != Token.Text.Whitespace):
            else_flag = False
            if value == "if":
                count += 1
                continue

        if value == "do":
            do_flag = True
            continue
        if do_flag and value == "while":
            do_flag = False
            continue

        if value in operator_values:
            count += 1

    return count

