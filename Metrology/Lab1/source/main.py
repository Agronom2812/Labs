import os

from all_operators_parcing import all_operators_parsing
from calc_max_depth import calc_max_depth
from condition_operators import condition_operators
from main_window import display_table


def main():
    file_path = os.path.abspath("Jilb_metr/Jilb_metr/Program.cs")
    with open(file_path, "r", encoding="utf-8") as f:
        code = f.read()

    max_depth = calc_max_depth(code)
    all_operators = all_operators_parsing(code)
    condition = condition_operators(code)

    CL = condition
    if all_operators == 0:
        cl = -1
    else :
        cl = float(CL / all_operators)
    CLI = max_depth

    display_table(CL, cl, CLI)

    print(max_depth, all_operators, condition)


if __name__ == "__main__":
    main()

