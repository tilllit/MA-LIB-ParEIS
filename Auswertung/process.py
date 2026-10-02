import subprocess
import sys
import os
import csv
import math
import cmath
from itertools import chain
import pip
from io import StringIO

def setup():
    subprocess.call([sys.executable, '-m', 'pip', '-q', 'install', '--upgrade', 'pip'])      # -m uses pip for specified python; -q disables log
    subprocess.call([sys.executable, '-m', 'pip', '-q', 'install', "-r" "requirements.txt"]) # -m uses pip for specified python; -q disables log; -r uses requirements file

#setup()

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
import scipy
from scipy.signal import butter, filtfilt, sosfiltfilt, find_peaks, lfilter
from scipy.ndimage import (
    uniform_filter1d,
    binary_closing,
    binary_opening,
    label
)


print("Numpy: ", np.__version__)
print("Pandas: ", pd.__version__)
print("Scipy: ", scipy.__version__)


# Converting Args to Settings here:
print ('argument list', sys.argv)

SampleFreq = int(sys.argv[1])
NrSamples = int(sys.argv[2])
Periods = int(sys.argv[3])
Fname = sys.argv[4]
NrCells = int(sys.argv[5])
Data_Path = sys.argv[6]
Imp_Path = sys.argv[7]

print("Freq.: ", Fname, " Freq. Sample: ", SampleFreq, " [Hz] Samles: ", NrSamples, " Periods: ", Periods, " Cells: ", NrCells)



def fit_sine_OLS(data, w0, plot=False, plot_nr=None):

    data = np.asarray(data).flatten()
    N = len(data)

    x = np.arange(N)

    w = 2 * np.pi * w0

    # Modell:
    # y = A*sin(wx) + B*cos(wx) + C + D*x
    X = np.column_stack((
        np.sin(x * w),
        np.cos(x * w),
        np.ones(N),
        x
    ))

    # OLS
    beta, _, _, _ = np.linalg.lstsq(X, data, rcond=None)

    A = beta[0]
    B = beta[1]
    C = beta[2]
    D = beta[3]

    amplitude = np.sqrt(A**2 + B**2)
    offset = C
    phase = np.arctan2(B, A)

    # Gesamter Fit inklusive Drift
    fit_data = X @ beta

    # Nur Sinus + konstanter Offset
    sine_fit = (
        A * np.sin(x * w)
        + B * np.cos(x * w)
        + C
    )

    # Geschätzter Offset-Drift
    trend = C + D * x


    # ---------------------------------------------------------
    # SNR berechnen
    # ---------------------------------------------------------

    # Reiner Sinusanteil
    signal = (
        A * np.sin(x * w)
        + B * np.cos(x * w)
    )

    # Alles, was der Fit nicht erklären kann, wird als Rauschen betrachtet
    noise = data - fit_data

    # Root Mean Square
    signal_rms = np.sqrt(np.mean(signal ** 2))
    noise_rms = np.sqrt(np.mean(noise ** 2))

    if noise_rms > 0:
        snr_db = 20 * np.log10(signal_rms / noise_rms)
    else:
        snr_db = np.inf


    if plot:
        fig = plt.figure()

        # Make plot folder
        Data_Dir = os.path.dirname(Data_Path)
        Plot_Path = os.path.join(Data_Dir, "Plots")
        os.makedirs(Plot_Path, exist_ok=True)

        plt.plot(x, data, label="Rohdaten")
        plt.plot(x, fit_data, label="OLS Fit inkl. Drift")
        plt.plot(x, sine_fit, "--", label="Sinus ohne Drift")
        plt.plot(x, trend, "--", label="Offset / Drift")

        plt.xlabel("Sample")
        plt.ylabel("Signal")
        plt.grid()
        plt.legend()

        plt.text(
            0.02,
            0.95,
            f"SNR: {snr_db:.2f} dB",
            transform=plt.gca().transAxes,
            verticalalignment="top",
            bbox=dict(boxstyle="round", alpha=0.8)
        )

        plot_name = Fname + "_OLS-Fit"
        if (plot_nr != None):
            plot_name = plot_name + str(plot_nr)

        file_path = os.path.join(
            Plot_Path,
            f"{plot_name}.png"
        )

        fig.savefig(
            file_path,
            dpi=300,
            bbox_inches="tight"
        )
        plt.show()
        plt.close(fig)

    return amplitude, offset, phase, D

def find_interval(data, sampling_frequency, periods, plot = False):
    """Bestimmt ein geeignetes Intervall für die Sinusapproximation."""

    data = np.asarray(data, dtype=float)

    if len(data) < 10 or periods < 2:
        return False, None, None, None, None, None

    # Erwartete Periodendauer grob abschätzen
    expected_period = len(data) / periods

    # Signal tiefpassfiltern
    lowpass = sosfiltfilt(
        butter(1, 0.3, btype="low", output="sos"),
        data
    )

    # Maxima erkennen
    signal_range = (
        np.percentile(lowpass, 95)
        - np.percentile(lowpass, 5)
    )

    peaks, _ = find_peaks(
        lowpass,
        distance=max(1, int(0.45 * expected_period)),
        prominence=0.08 * signal_range
    )

    if len(peaks) < 2:
        return False, None, None, None, None, None

    # Längste Folge mit regelmäßigen Peak-Abständen auswählen
    distances = np.diff(peaks)
    typical_distance = np.median(distances)

    irregular = (
        (distances < 0.65 * typical_distance)
        | (distances > 1.35 * typical_distance)
    )

    peak_groups = np.split(
        peaks,
        np.where(irregular)[0] + 1
    )

    peaks = max(peak_groups, key=len)

    # Randperioden ausschließen
    if periods > 10:
        discard_start, discard_end = 4, 2
    elif periods > 5:
        discard_start, discard_end = 3, 1
    else:
        discard_start, discard_end = 1, 0

    if len(peaks) - discard_start - discard_end < 2:
        return False, None, None, None, None, None

    if discard_end > 0:
        used_peaks = peaks[discard_start:-discard_end]
    else:
        used_peaks = peaks[discard_start:]

    # 6. Periodendauer, Frequenz und Intervall berechnen
    period_samples = float(np.mean(np.diff(used_peaks)))
    frequency = sampling_frequency / period_samples

    interval_start = int(used_peaks[0])
    interval_end = int(used_peaks[-1] + 1)

    # ---------------------------------------------------------
    # Plot
    # ---------------------------------------------------------
    
    if plot:
        fig = plt.figure(
            "Find interval"
        )

        fig.clear()

        ax = fig.add_subplot(111)

        ax.plot(
            data,
            label="Org"
        )

        ax.plot(
            lowpass,
            label="LP"
        )

        if len(peaks) > 0:
            ax.plot(
                peaks,
                lowpass[peaks],
                "x",
                label="erkannte Peaks"
            )

        # Erkannter aktiver Sinusbereich
        ax.axvspan(
            interval_start,
            interval_end,
            alpha=0.12,
            label="erkannter Sinusbereich"
        )

        ax.plot(
            used_peaks,
            lowpass[used_peaks],
            "o",
            label="verwendete Peaks"
        )

        ax.axvline(
            interval_start,
            linestyle="--",
            label="Intervall Start"
        )

        ax.axvline(
            interval_end,
            linestyle="--",
            label="Intervall Ende"
        )

        ax.set_title(
            f"Frequenz: {frequency:.3f} Hz | "
            f"Periode: {period_samples:.2f} Samples | "
            f"Peaks: {len(used_peaks)} | "
            #f"Prominence: {min_prominence:.6g}"
        )

        ax.set_xlabel("Sample")
        ax.set_ylabel("Amplitude")
        ax.legend()
        fig.tight_layout()

        # Make plot folder
        Data_Dir = os.path.dirname(Data_Path)
        Plot_Path = os.path.join(Data_Dir, "Plots")
        os.makedirs(Plot_Path, exist_ok=True)

        plot_name = Fname + "_Interval"
        
        file_path = os.path.join(
            Plot_Path,
            f"{plot_name}.png"
        )

        fig.savefig(
            file_path,
            dpi=300,
            bbox_inches="tight"
        )
        plt.show()
        plt.close(fig)

    return (
        True,
        peaks.tolist(),
        interval_start,
        interval_end,
        frequency,
        period_samples
    )


def main() -> None:
    #Data_Path = "X:\Desktop\\15-07-26\\10Hz.csv"
    #Data_Path = "X:\Desktop\\Acquisition_PHY_DATA_1.csv"
    data = pd.read_csv(Data_Path, header=0, sep=';')

    Voltage = np.array(data["Voltage [V]"])
    
    datavalid, peaks, int_start, int_end, Freq, p_samples = find_interval(Voltage, SampleFreq, Periods, True)

    if not datavalid:
        print("Fehler: Es konnte kein gültiges Sinusintervall gefunden werden.")
        return

    print(f"Ermittelte Frequenz: {Freq:.6f} Hz")
    print(f"Samples pro Periode: {p_samples:.6f}")

    Voltage = Voltage[int_start:int_end]    # Correcting Interval
    amplitude, offset, phase, drift = fit_sine_OLS(Voltage, 1/p_samples, True)

    phase = phase - math.pi / 2
    U_cplx = amplitude * complex(math.cos(phase), math.sin(phase))

    print(U_cplx)


    Zcomp = []
    new_line = Fname
    
    # Loop over Cells
    for i in range(NrCells):
        Xcurrent = np.array(data.iloc[:, (i+2)])
        Xcurrent = Xcurrent[int_start:int_end]    # Correcting Interval
        amplitude, offset, phase, drift = fit_sine_OLS(Xcurrent, 1/p_samples, False, i+1)
        phase = phase - math.pi / 2
        I_cplx = amplitude * complex(math.cos(phase), math.sin(phase))

        # safe from div. by zero!
        if (I_cplx == complex(0,0)):
            Zcomp.append(complex(0,0))
        else:
            Zcomp.append(U_cplx / I_cplx)

        print(Zcomp[i])

    with open(Imp_Path, mode="a", newline="", encoding="utf-8") as datei:
        writer = csv.writer(datei, delimiter=";")
        writer.writerow([new_line] + Zcomp)

    print("Finished .py-script successfully!")
    #plt.show()



if __name__ == "__main__":
    main()
