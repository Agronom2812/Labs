import tkinter as tk
from tkinter import ttk

def display_table(val1, val2, val3):

    root = tk.Tk()
    root.title("3x2 Table")
    root.geometry("500x300")

    label_font = ("Helvetica", 16)

    row_labels = ["CL", "cl", "CLI"]

    values = [val1, val2, val3]

    for i in range(3):

        label = ttk.Label(root, text=row_labels[i], borderwidth=1, relief="solid", padding=10, font=label_font)
        label.grid(row=i, column=0, padx=5, pady=5, sticky="nsew")

        value_label = ttk.Label(root, text=str(values[i]), borderwidth=1, relief="solid", padding=10, font=label_font)
        value_label.grid(row=i, column=1, padx=5, pady=5, sticky="nsew")

    root.grid_columnconfigure(0, weight=1)
    root.grid_columnconfigure(1, weight=1)

    root.mainloop()
