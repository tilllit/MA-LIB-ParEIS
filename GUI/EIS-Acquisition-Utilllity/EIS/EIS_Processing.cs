using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class EIS_Processing
{

    public static string ImpedanceFilePath = "";


    public static void Process_Single_Frequency(Acquisition_Config.SIN_Config config, int NrCells, string Freq_Data_Path, string Imp_Data_Path)
    {
        string defaultPath = AppDomain.CurrentDomain.BaseDirectory;

        string Fname = config.Name.Replace(" ", "");

        string arguments = "";
        arguments += " " + config.Fsample.ToString();
        arguments += " " + config.Samples.ToString();
        arguments += " " + config.Periods.ToString();
        arguments += " " + Fname;
        arguments += " " + NrCells;
        arguments += " " + Freq_Data_Path;
        arguments += " " + Imp_Data_Path;

        Python.Run(@defaultPath + "Python\\process.py", arguments);
    }









}

