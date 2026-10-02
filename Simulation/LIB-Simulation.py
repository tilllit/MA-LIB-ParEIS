import numpy as np
import math
import matplotlib.pyplot as plt


def parallel(z1, z2):
    return 1 / (1 / z1 + 1 / z2)

def parallel_many(z_list):
    z_arr = np.array(z_list)
    return 1 / np.sum(1 / z_arr, axis=0)

def capacitor_impedance(C, omega):
    return 1 / (1j * omega * C)

def inductor_impedance(L, omega):
    return 1j * omega * L

def warburg_impedance(sigma, omega):
    return sigma / np.sqrt(1j * omega)


# Li-Ion Model
def eis_li_ion_model(freq, L, Rs, Rsei, Csei, Rct, Cdl, Rw1, Cw1, Rw2, Cw2):
    omega = 2 * np.pi * freq

    Z_L = inductor_impedance(L, omega)
    Z_sei = parallel(Rsei, capacitor_impedance(Csei, omega))

    # Replaced Warburg
    Z_w1 = parallel(Rw1, capacitor_impedance(Cw1, omega))
    Z_w2 = parallel(Rw2, capacitor_impedance(Cw2, omega))

    Z_ct = parallel(Rct+Z_w1+Z_w2, capacitor_impedance(Cdl, omega))

    return Z_L + Rs + Z_sei + Z_ct

# Parallelling
def equivalent_parallel_from_reference(branch_impedances, Rsheet, Rbond, ref_idx):
    """
    Ersatzimpedanz aus Sicht der Referenzzelle ref_idx.

    Jeder andere Zellpfad bekommt zusätzlich:
        2 * distance * Rsheet
    weil auf Plus- und Minus-Seite je ein Blechwiderstand pro Schritt dazukommt.
    """
    n_cells = len(branch_impedances)
    paths = []
    Ppaths = []

    for k in range(n_cells):
        
        if (k == ref_idx):
            Ppaths.append(branch_impedances[k] - 2 * Rbond)
        else:
            distance = abs(k - ref_idx)
            Z_path = branch_impedances[k] #+ 2 * distance * Rsheet
            paths.append(Z_path)

    Ppaths.append(parallel_many(paths) + 2 * Rbond + 2* Rsheet)

    return parallel_many(Ppaths)

# =========================================================
# Frequenzbereich
# =========================================================
Fmin = 0.01     # 10mHz
Fmax = 1000.0   # 1kHz
freq = np.logspace(math.log10(Fmax), math.log10(Fmin), 1000)


# =========================================================
# Parameter
# =========================================================
n_cells = 74

Rbond = 3.5e-3    # 3,5 mOhm pro Bond-Draht
Rsheet = 0.5e-3   # 0,5 mOhm pro Blechsegment pro Seite

np.random.seed(42)


######## Original aus dem paper ########
base_params = {
    "L": 0.89e-6,
    "Rs": 0.0512,
    "Rsei": 0.00571,
    "Csei": 0.124,
    "Rct": 0.00634,
    "Cdl": 1.68,

    "Rw1": 0.00378,
    "Cw1": 222,
    "Rw2": 0.0397,
    "Cw2": 954
}

variation = {
    "L": 0.01,
    "Rs": 0.001,
    "Rsei": 0.01,
    "Csei": 0.01,
    "Rct": 0.01,
    "Cdl": 0.01,

    "Rw1": 0.01,
    "Cw1": 0.01,
    "Rw2": 0.01,
    "Cw2": 0.01
}

# 310 Cycles
base_params310 = {
    "L": 0.759e-6,
    "Rs": 0.0539,
    "Rsei": 0.00543,
    "Csei": 0.300,
    "Rct": 0.0119,
    "Cdl": 3.01,

    "Rw1": 0.00592,
    "Cw1": 530,
    "Rw2": 0.0504,
    "Cw2": 1200
}

# 480 Cycles
base_params480 = {
    "L": 0.738e-6,
    "Rs": 0.0561,
    "Rsei": 0.00538,
    "Csei": 0.319,
    "Rct": 0.013,
    "Cdl": 3.2,

    "Rw1": 0.00635,
    "Cw1": 579,
    "Rw2": 0.0499,
    "Cw2": 1270
}

# 1k Cycles
base_params1000 = {
    "L": 0.573e-6,
    "Rs": 0.06141,
    "Rsei": 0.00502,
    "Csei": 0.530,
    "Rct": 0.02022,
    "Cdl": 4.85,

    "Rw1": 0.00913,
    "Cw1": 966,
    "Rw2": 0.06095,
    "Cw2": 1612
}



# region Plot1_Einzelzelle

Z_cell = eis_li_ion_model(
    freq=freq,
    L=base_params["L"],
    Rs=base_params["Rs"],
    Rsei=base_params["Rsei"],
    Csei=base_params["Csei"],
    Rct=base_params["Rct"],
    Cdl=base_params["Cdl"],

    Rw1=base_params["Rw1"],
    Cw1=base_params["Cw1"],
    Rw2=base_params["Rw2"],
    Cw2=base_params["Cw2"],
)

Z_cell310 = eis_li_ion_model(
    freq=freq,
    L=base_params310["L"],
    Rs=base_params310["Rs"],
    Rsei=base_params310["Rsei"],
    Csei=base_params310["Csei"],
    Rct=base_params310["Rct"],
    Cdl=base_params310["Cdl"],

    Rw1=base_params310["Rw1"],
    Cw1=base_params310["Cw1"],
    Rw2=base_params310["Rw2"],
    Cw2=base_params310["Cw2"],
)

Z_cell480 = eis_li_ion_model(
    freq=freq,
    L=base_params480["L"],
    Rs=base_params480["Rs"],
    Rsei=base_params480["Rsei"],
    Csei=base_params480["Csei"],
    Rct=base_params480["Rct"],
    Cdl=base_params480["Cdl"],

    Rw1=base_params480["Rw1"],
    Cw1=base_params480["Cw1"],
    Rw2=base_params480["Rw2"],
    Cw2=base_params480["Cw2"],
)

Z_cell1000 = eis_li_ion_model(
    freq=freq,
    L=base_params1000["L"],
    Rs=base_params1000["Rs"],
    Rsei=base_params1000["Rsei"],
    Csei=base_params1000["Csei"],
    Rct=base_params1000["Rct"],
    Cdl=base_params1000["Cdl"],

    Rw1=base_params1000["Rw1"],
    Cw1=base_params1000["Cw1"],
    Rw2=base_params1000["Rw2"],
    Cw2=base_params1000["Cw2"],
)



# =========================================================
# Plot 1: Einzelpfade der Zellen
# =========================================================
plt.figure("Einzelzellen", figsize=(8, 4))


plt.plot(
    Z_cell.real,
    -Z_cell.imag,
    color="green",
    label=f"0 Zyklen",
    #alpha=0.15,
    linewidth=3
)

plt.plot(
    Z_cell310.real,# - 0.002,
    -Z_cell310.imag,
    color="orange",
    label=f"310 Zyklen",
    #alpha=0.15,
    linewidth=3
)

plt.plot(
    Z_cell480.real,# - 0.0042,
    -Z_cell480.imag,
    color="red",
    label=f"480 Zyklen",
    #alpha=0.15,
    linewidth=3
)

plt.plot(
    Z_cell1000.real,# - 0.0093,
    -Z_cell1000.imag,
    color="magenta",
    label=f"1000 Zyklen",
    #alpha=0.15,
    linewidth=3
)

plt.axhline(0, color="black", linewidth=1)
plt.xlabel(r"$Z'$ [Ohm]")
plt.ylabel(r"$-Z''$ [Ohm]")
plt.title("Alterungsstufen von Einzelzellen")
plt.grid(True)
plt.legend()
plt.tight_layout()
#plt.show()

# endregion


plt.show()
