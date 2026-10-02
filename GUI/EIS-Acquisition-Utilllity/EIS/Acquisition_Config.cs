using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Acquisition_Config
{

    public struct SIN_Config
    {
        public string Name { get; set; }

        public byte E_Load_Speed { get; set; }
        public UInt16 ARR { get; set; }
        public UInt16 PSC { get; set; }
        public byte Periods { get; set; }
        public UInt32 Srelax { get; set; }
        public UInt16 LUT { get; set; }
        public UInt16 DAC_Amplitude { get; set; }
        public UInt16 DAC_Offset { get; set; }

        public UInt16 Samples { get; set; }
        public UInt32 Fsample { get; set; }
        public UInt32 Fserial { get; set; }
        public byte NumCards { get; set; }

        public SIN_Config(string name, byte e_load_speed, UInt16 arr, UInt16 psc, byte periods, UInt16 s_relax, UInt16 lut, UInt16 dac_amp, UInt16 dac_offset, UInt16 samples, UInt32 f_sample, UInt32 f_serial= 25_000_000, byte num_cards = 3)
        {
            Name = name;
            E_Load_Speed = e_load_speed;
            ARR = arr;
            PSC = psc;
            Periods = periods;
            LUT = lut;
            DAC_Amplitude = dac_amp;
            DAC_Offset = dac_offset;
            Samples = samples;
            Fsample = f_sample;
            Fserial = f_serial;
            NumCards = num_cards;
            Srelax = s_relax;
        }
    }


    // Fast E-Load (2.2nF)  ???
    public static SIN_Config Conf_3000_000mHz = new SIN_Config(name: "3kHz", e_load_speed: 2, arr: 2666, psc: 0, periods: 40, lut: 18, dac_amp: 100, dac_offset: 280, samples: 2000, f_sample: 120000, s_relax: 30);

    public static SIN_Config Conf_2400_000mHz = new SIN_Config(name: "2.4kHz", e_load_speed: 2, arr: 3332, psc: 0, periods: 30, lut: 18, dac_amp: 100, dac_offset: 280, samples: 2000, f_sample: 120000, s_relax: 30);

    public static SIN_Config Conf_2000_000mHz = new SIN_Config(name: "2kHz", e_load_speed: 2, arr: 3999, psc: 0, periods: 30, lut: 18, dac_amp: 100, dac_offset: 280, samples: 2000, f_sample: 120000, s_relax: 30);

    public static SIN_Config Conf_1500_000mHz = new SIN_Config(name: "1.5kHz", e_load_speed: 2, arr: 2665, psc: 1, periods: 30, lut: 18, dac_amp: 100, dac_offset: 280, samples: 3000, f_sample: 120000, s_relax: 30);

    
    // Mid E-Load (10nF)
    public static SIN_Config Conf_1000_000mHz = new SIN_Config(name: "1kHz", e_load_speed: 2, arr: 3999, psc: 0, periods: 20, lut: 36, dac_amp: 100, dac_offset: 280, samples: 2000, f_sample: 80000, s_relax: 30);

    public static SIN_Config Conf_900_000mHz = new SIN_Config(name: "900Hz", e_load_speed: 2, arr: 1110, psc: 3, periods: 20, lut: 36, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 64000, s_relax: 30);

    public static SIN_Config Conf_800_000mHz = new SIN_Config(name: "800Hz", e_load_speed: 2, arr: 4999, psc: 0, periods: 20, lut: 36, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 64000, s_relax: 30);

    public static SIN_Config Conf_700_000mHz = new SIN_Config(name: "700Hz", e_load_speed: 2, arr: 2855, psc: 1, periods: 20, lut: 36, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 56000, s_relax: 30);

    public static SIN_Config Conf_600_000mHz = new SIN_Config(name: "600Hz", e_load_speed: 2, arr: 1110, psc: 2, periods: 20, lut: 72, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 48000, s_relax: 30);

    public static SIN_Config Conf_500_000mHz = new SIN_Config(name: "500Hz", e_load_speed: 2, arr: 1999, psc: 0, periods: 20, lut: 144, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 40000, s_relax: 50);

    public static SIN_Config Conf_400_000mHz = new SIN_Config(name: "400Hz", e_load_speed: 2, arr: 2499, psc: 0, periods: 20, lut: 144, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 32000, s_relax: 50);

    public static SIN_Config Conf_300_000mHz = new SIN_Config(name: "300Hz", e_load_speed: 2, arr: 1110, psc: 2, periods: 20, lut: 144, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 24000, s_relax: 50);

    public static SIN_Config Conf_200_000mHz = new SIN_Config(name: "200Hz", e_load_speed: 2, arr: 4999, psc: 0, periods: 20, lut: 144, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 16000, s_relax: 50);

    public static SIN_Config Conf_100_000mHz = new SIN_Config(name: "100Hz", e_load_speed: 2, arr: 2879, psc: 0, periods: 20, lut: 500, dac_amp: 80, dac_offset: 280, samples: 2000, f_sample: 8000, s_relax: 50);

    // Add more?

    public static SIN_Config Conf_50_000mHz = new SIN_Config(name: "50Hz", e_load_speed: 1, arr: 2879, psc: 0, periods: 20, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 4000, s_relax: 90);

    public static SIN_Config Conf_20_000mHz = new SIN_Config(name: "20Hz", e_load_speed: 1, arr: 7199, psc: 0, periods: 20, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 1600, s_relax: 90);

    public static SIN_Config Conf_10_000mHz = new SIN_Config(name: "10Hz", e_load_speed: 1, arr: 14399, psc: 0, periods: 20, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 800, s_relax: 120);

    public static SIN_Config Conf_5_000mHz = new SIN_Config(name: "5Hz", e_load_speed: 1, arr: 28799, psc: 0, periods: 20, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 400, s_relax: 120);

    public static SIN_Config Conf_2_000mHz = new SIN_Config(name: "2Hz", e_load_speed: 1, arr: 35999, psc: 1, periods: 20, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 160, s_relax: 120);

    public static SIN_Config Conf_1_500mHz = new SIN_Config(name: "1.5Hz", e_load_speed: 1, arr: 47999, psc: 1, periods: 10, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 200, s_relax: 180);

    public static SIN_Config Conf_1_000mHz = new SIN_Config(name: "1Hz", e_load_speed: 1, arr: 47999, psc: 2, periods: 10, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 160, s_relax: 180);


    // Real Slow here!

    public static SIN_Config Conf_900mHz = new SIN_Config(name: "900mHz", e_load_speed: 1, arr: 39999, psc: 3, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 300, s_relax: 240);

    public static SIN_Config Conf_800mHz = new SIN_Config(name: "800mHz", e_load_speed: 1, arr: 59999, psc: 2, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 300, s_relax: 240);

    public static SIN_Config Conf_700mHz = new SIN_Config(name: "700mHz", e_load_speed: 1, arr: 41141, psc: 4, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 200, s_relax: 240);

    public static SIN_Config Conf_600mHz = new SIN_Config(name: "600mHz", e_load_speed: 1, arr: 59999, psc: 3, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 200, s_relax: 240);

    public static SIN_Config Conf_500mHz = new SIN_Config(name: "500mHz", e_load_speed: 1, arr: 57599, psc: 4, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 160, s_relax: 300);

    public static SIN_Config Conf_200mHz = new SIN_Config(name: "200mHz", e_load_speed: 1, arr: 59999, psc: 11, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 80, s_relax: 300);

    public static SIN_Config Conf_100mHz = new SIN_Config(name: "100mHz", e_load_speed: 1, arr: 59999, psc: 23, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 40, s_relax: 300);

    public static SIN_Config Conf_50mHz = new SIN_Config(name: "50mHz", e_load_speed: 1, arr: 63999, psc: 44, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 20, s_relax: 300);

    public static SIN_Config Conf_10mHz = new SIN_Config(name: "10mHz", e_load_speed: 1, arr: 63999, psc: 224, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 4, s_relax: 300);

    public static SIN_Config Conf_5mHz = new SIN_Config(name: "5mHz", e_load_speed: 1, arr: 63999, psc: 449, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 2000, f_sample: 2, s_relax: 300);

    public static SIN_Config Conf_1mHz = new SIN_Config(name: "1mHz", e_load_speed: 1, arr: 63999, psc: 2249, periods: 5, lut: 1000, dac_amp: 40, dac_offset: 280, samples: 5000, f_sample: 1, s_relax: 300);


    public static List<SIN_Config> TB_Sweeps = new List<SIN_Config>()
    {
        Conf_3000_000mHz, Conf_2400_000mHz, Conf_2000_000mHz, Conf_1500_000mHz,
        Conf_1000_000mHz, Conf_900_000mHz, Conf_800_000mHz, Conf_700_000mHz, Conf_600_000mHz, Conf_500_000mHz, Conf_400_000mHz, Conf_300_000mHz, Conf_200_000mHz, Conf_100_000mHz,
        Conf_50_000mHz, Conf_20_000mHz, Conf_10_000mHz, Conf_5_000mHz, Conf_2_000mHz, Conf_1_500mHz, Conf_1_000mHz,
        Conf_900mHz, Conf_800mHz, Conf_700mHz, Conf_600mHz, Conf_500mHz, Conf_200mHz, Conf_100mHz, Conf_50mHz, Conf_10mHz, Conf_5mHz, Conf_1mHz
    };











    // Mid E-Load (10nF)
    public static SIN_Config MSX_Conf_3000_000mHz = new SIN_Config(name: "3kHz", e_load_speed: 2, arr: 2666, psc: 0, periods: 40, lut: 18, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 120000, s_relax: 30, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_2000_000mHz = new SIN_Config(name: "2kHz", e_load_speed: 2, arr: 3999, psc: 0, periods: 30, lut: 18, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 120000, s_relax: 30, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_1000_000mHz = new SIN_Config(name: "1kHz", e_load_speed: 2, arr: 3999, psc: 0, periods: 20, lut: 36, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 80000, s_relax: 30, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_500_000mHz = new SIN_Config(name: "500Hz", e_load_speed: 2, arr: 1999, psc: 0, periods: 20, lut: 144, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 40000, s_relax: 50, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_200_000mHz = new SIN_Config(name: "200Hz", e_load_speed: 2, arr: 4999, psc: 0, periods: 20, lut: 144, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 16000, s_relax: 50, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_100_000mHz = new SIN_Config(name: "100Hz", e_load_speed: 2, arr: 2879, psc: 0, periods: 20, lut: 500, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 8000, s_relax: 50, num_cards: 26, f_serial: 15_000_000);


    // Slow E-Load (220nF)

    public static SIN_Config MSX_Conf_50_000mHz = new SIN_Config(name: "50Hz", e_load_speed: 1, arr: 2879, psc: 0, periods: 20, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 4000, s_relax: 90, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_20_000mHz = new SIN_Config(name: "20Hz", e_load_speed: 1, arr: 7199, psc: 0, periods: 20, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 1600, s_relax: 90, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_10_000mHz = new SIN_Config(name: "10Hz", e_load_speed: 1, arr: 14399, psc: 0, periods: 20, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 800, s_relax: 120, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_5_000mHz = new SIN_Config(name: "5Hz", e_load_speed: 1, arr: 28799, psc: 0, periods: 20, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 400, s_relax: 120, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_2_000mHz = new SIN_Config(name: "2Hz", e_load_speed: 1, arr: 35999, psc: 1, periods: 20, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 160, s_relax: 120, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_1_000mHz = new SIN_Config(name: "1Hz", e_load_speed: 1, arr: 47999, psc: 2, periods: 10, lut: 1000, dac_amp: 240, dac_offset: 480, samples: 2000, f_sample: 160, s_relax: 180, num_cards: 26, f_serial: 15_000_000);


    public static SIN_Config MSX_Conf_500mHz = new SIN_Config(name: "500mHz", e_load_speed: 1, arr: 57599, psc: 4, periods: 5, lut: 1000, dac_amp: 200, dac_offset: 420, samples: 2000, f_sample: 160, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_200mHz = new SIN_Config(name: "200mHz", e_load_speed: 1, arr: 59999, psc: 11, periods: 5, lut: 1000, dac_amp: 200, dac_offset: 420, samples: 2000, f_sample: 80, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_100mHz = new SIN_Config(name: "100mHz", e_load_speed: 1, arr: 59999, psc: 23, periods: 5, lut: 1000, dac_amp: 200, dac_offset: 420, samples: 2000, f_sample: 40, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_50mHz = new SIN_Config(name: "50mHz", e_load_speed: 1, arr: 63999, psc: 44, periods: 5, lut: 1000, dac_amp: 140, dac_offset: 360, samples: 2000, f_sample: 20, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_10mHz = new SIN_Config(name: "10mHz", e_load_speed: 1, arr: 63999, psc: 224, periods: 5, lut: 1000, dac_amp: 140, dac_offset: 360, samples: 2000, f_sample: 4, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_5mHz = new SIN_Config(name: "5mHz", e_load_speed: 1, arr: 63999, psc: 449, periods: 5, lut: 1000, dac_amp: 140, dac_offset: 320, samples: 2000, f_sample: 2, s_relax: 300, num_cards: 26, f_serial: 15_000_000);

    public static SIN_Config MSX_Conf_1mHz = new SIN_Config(name: "1mHz", e_load_speed: 1, arr: 63999, psc: 2249, periods: 5, lut: 1000, dac_amp: 140, dac_offset: 320, samples: 5000, f_sample: 1, s_relax: 300, num_cards: 26, f_serial: 15_000_000);


    public static List<SIN_Config> MSX_Sweeps = new List<SIN_Config>()
    {
        MSX_Conf_3000_000mHz, MSX_Conf_2000_000mHz, MSX_Conf_1000_000mHz,
        MSX_Conf_500_000mHz, MSX_Conf_200_000mHz, MSX_Conf_100_000mHz,
        MSX_Conf_50_000mHz, MSX_Conf_20_000mHz, MSX_Conf_10_000mHz, MSX_Conf_5_000mHz, MSX_Conf_2_000mHz, MSX_Conf_1_000mHz,
        MSX_Conf_500mHz, MSX_Conf_200mHz, MSX_Conf_100mHz,
        MSX_Conf_50mHz, MSX_Conf_10mHz, MSX_Conf_5mHz, MSX_Conf_1mHz

    };

}

