from pygments import lex
from pygments.lexers import CSharpLexer
from pygments.token import Token


def countIf(words: list, start, finish) -> int:
    i = start
    max_depth = 0
    curr_depth = 0
    while i < finish:
        if words[i] == "if" or words[i] == "for" or words[i] == "else" or words[i] == "while" or words[i] == "do":
            if words[i] != "else":
                curr_depth = 1

            j = i + 1
            balance = 1
            while balance != 0:
                j += 1
                if words[j] == "{":
                    balance += 1
                elif words[j] == "}":
                    balance -= 1
            curr_depth += countIf(words, i + 2, j)
            if curr_depth > max_depth:
                max_depth = curr_depth
            i = j

        if words[i] == "switch":
            j = i + 1
            balance = 1
            while balance != 0:
                j += 1
                if words[j] == "{":
                    balance += 1
                elif words[j] == "}":
                    balance -= 1
            curr_depth += countSwitch(words, i + 2, j)
            if curr_depth > max_depth:
                max_depth = curr_depth
            i = j

        i += 1
    return max_depth


def countSwitch(words: list, start, finish) -> int:
    i = start
    max_depth = 0
    curr_depth = 0
    case_depth = 0
    while i < finish:
        if words[i] == "case":
            j = i
            case_depth += 1
            was_case = False
            while not was_case and j < finish:
                j += 1
                if words[j] == "case" or words[j] == "default":
                    was_case = True
                elif words[j] == "switch":
                    k = j + 1
                    balance = 1
                    while balance != 0:
                        k += 1
                        if words[k] == "{":
                            balance += 1
                        elif words[k] == "}":
                            balance -= 1
                    curr_depth += countSwitch(words, j + 2, k)
                    if curr_depth > max_depth:
                        max_depth = curr_depth
                    j = k

            curr_depth = countIf(words, i + 1, j) + case_depth
            if curr_depth > max_depth:
                max_depth = curr_depth
            i = j - 1

        elif words[i] == "default":
            curr_depth = countIf(words, i + 1, finish) + case_depth
            if curr_depth > max_depth:
                max_depth = curr_depth
            i = finish - 1

        i += 1
    return max_depth


def replaceIf(words: list, start, finish):
    i = start
    start_of_if = start
    was_elseif = False
    while i < finish or was_elseif:
        if i < finish and (words[i] == "if" and not was_elseif or words[i] == "elseif" or words[i] == "else"):
            if words[i] == "if":
                start_of_if = i
            if words[i] == "elseif":
                was_elseif = True

            j = i + 1
            balance = 1
            while balance != 0:
                j += 1
                if words[j] == "{":
                    balance += 1
                elif words[j] == "}":
                    balance -= 1
            replaceIf(words, i + 2, j)
            i = j

        elif was_elseif:
            was_elseif = False
            end_if = i
            j = start_of_if
            while j < end_if:
                if words[j] == "elseif" or words[j] == "if":
                    if words[j] == "elseif":
                        words[j] = "if"
                    k = j + 1
                    balance = 1
                    while balance != 0:
                        k += 1
                        if words[k] == "{":
                            balance += 1
                        elif words[k] == "}":
                            balance -= 1
                    if words[k + 1] == "else":
                        j = k + 1
                    else:
                        words.insert(end_if, "}")
                        words.pop(k)
                        j = k
                else:
                    break

        i += 1


def calc_max_depth(code):
    tokens = lex(code, CSharpLexer())
    max_depth = -1
    cur = -1

    operator_values = {
        "if", "for", "case", "switch", "else", "default", "while", "do"
    }
    words = []
    opened_brace_count = 0
    closed_brace_count = 0

    else_flag = False
    wait_for_brace = False
    do_flag = False

    for token_type, value in tokens:
        if value == "else":
            else_flag = True
            wait_for_brace = True
            continue
        if else_flag and (value != " " or token_type != Token.Text.Whitespace):
            else_flag = False
            if value == "if":
                words.append("elseif")
                continue
            else:
                words.append("else")

        if value == "do":
            do_flag = True
            words.append(value)
            wait_for_brace = True
            continue
        if do_flag and value == "while":
            words.append(value)
            do_flag = False
            continue

        if value in operator_values:
            words.append(value)
            wait_for_brace = True
        if value == "{" and wait_for_brace:
            words.append(value)
            opened_brace_count += 1
            wait_for_brace = False
        if value == "}" and opened_brace_count > closed_brace_count:
            words.append(value)
            closed_brace_count += 1

    replaceIf(words, 0, len(words))

    return countIf(words, 0, len(words)) - 1
