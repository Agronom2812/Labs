import tkinter as tk
from tkinter import ttk
import math
import re


def analyze_halstead(code):
    # Основные операторы C#
    operators = {
        'if', 'else', 'for', 'while', 'do', 'switch', 'case', 'break', 'continue', 'return',
        'try', 'catch', 'finally', 'throw', 'new', 'using', 'namespace', 'class', 'struct',
        'interface', 'enum', 'void', 'int', 'double', 'float', 'bool', 'string', 'var',
        'public', 'private', 'protected', 'internal', 'static', 'readonly', 'const',
        '=', '+=', '-=', '*=', '/=', '%=', '&=', '|=', '^=', '<<=', '>>=', '=>',
        '+', '-', '*', '/', '%', '++', '--', '!', '~', '&', '|', '^', '<<', '>>',
        '==', '!=', '<', '>', '<=', '>=', '&&', '||', '??', '?.', '?', ':', '?.',
        '(', ')', '[', ']', '{', '}', '.', ',', ';', '...'
    }

    unique_operators = set()
    unique_operands = set()
    total_operators = 0
    total_operands = 0

    # Улучшенное разбиение на токены для C#
    tokens = re.findall(
        r'\w+|\+=|-=|\*=|/=|%=|&=|\|=|\^=|<<=|>>=|=>|\+\+|--|!=|<=|>=|==|&&|\|\||\?\?|\.\?|\?|:|\+|-|\*|/|%|~|&|\||\^|<<|>>|\(|\)|\[|\]|\{|\}|\.|,|;|\.\.\.',
        code
    )

    for token in tokens:
        if token in operators:
            unique_operators.add(token)
            total_operators += 1
        elif token.isidentifier() or token.isdigit() or (token.startswith('"') and token.endswith('"')) or (
            token.startswith("'") and token.endswith("'")):
            unique_operands.add(token)
            total_operands += 1

    n1 = len(unique_operators)
    n2 = len(unique_operands)
    N1 = total_operators
    N2 = total_operands

    N = N1 + N2
    n = n1 + n2
    V = N * math.log2(n) if n > 0 else 0

    # Расчет объема программы в битах
    program_size_bits = len(code.encode('utf-8')) * 8

    return {
        "n1": n1, "n2": n2, "N1": N1, "N2": N2,
        "Length (N)": N, "Vocabulary (n)": n,
        "Volume (V)": V, "Program Size (bits)": program_size_bits
    }


def show_metrics():
    code = text_input.get("1.0", tk.END)
    metrics = analyze_halstead(code)

    for row in tree.get_children():
        tree.delete(row)

    for metric, value in metrics.items():
        tree.insert("", tk.END, values=(metric, f"{value:.2f}" if isinstance(value, float) else value))


root = tk.Tk()
root.title("Halstead Metrics Analyzer for C#")
root.geometry("600x500")

tk.Label(root, text="Insert C# code here:").pack()
text_input = tk.Text(root, height=10, width=70)
text_input.pack()

tk.Button(root, text="Calculate Metrics", command=show_metrics).pack(pady=30)

tree = ttk.Treeview(root, columns=("Metric", "Value"), show="headings")
tree.heading("Metric", text="Metric")
tree.heading("Value", text="Value")
tree.pack(fill=tk.BOTH, expand=True)

default_code = """using System;

namespace Example
{
    class Program
    {
        static int Sum(int a, int b)
        {
            return a + b;
        }

        static void BubbleSort(int[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (array[j] > array[j + 1])
                    {
                        int temp = array[j];
                        array[j] = array[j + 1];
                        array[j + 1] = temp;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            int x = 0;
            int z = 1;

            if (x >= 1)
            {
                int y = 0;
            }
            else
            {
                x++;
                x = x + z;
                z += x;
            }

            int result = Sum(2, 3);
            Console.WriteLine("Result: " + result);

            int[] arr = { 5, 3, 8, 1, 2 };
            BubbleSort(arr);

            z++;
            Console.WriteLine("Sorted array:");
            foreach (var item in arr)
            {
                Console.Write(item + " ");
            }
        }
    }
}"""
text_input.insert(tk.END, default_code)

root.mainloop()
