using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;


public static class Python
{

    // ############## Settings ##############
    public static bool debug = true;
    public static string pyVersion = "3.9";                                                                     // Better use *3.9* / *3.10* ... no more specification to allow python from microsoft store!
    public static string pyDownload = "https://www.microsoft.com/store/productId/9P7QFQMJRFP7?ocid=pdpshare";   // Download link for specified py version (for user)


    // ############## Variables #############
    public static bool errShow = false;
    public static string errMsg = "";

    public delegate void PythonEvent(string sender);
    public static event PythonEvent RX;
    public static event PythonEvent Finished;


    public static string GetPath()
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine("where.exe python" + pyVersion);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();
        string result = cmd.StandardOutput.ReadToEnd();

        string[] lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        string path = "-";
        foreach (string line in lines) { if (System.IO.File.Exists(line)) { path = line; } }
        //Console.WriteLine("Path: " + path);
        return path;
    }

    public static bool Installed(bool message = true)
    {
        bool exists = true;

        string version = GetVersion();
        if (version == "-") { exists = false; }

        string[] splits1 = version.Split('.');
        string[] splits2 = pyVersion.Split('.');

        if (!(splits1[0] == splits2[0] && splits1[1] == splits2[1])) { exists = false; }

        if (exists == false)
        {
            exists = false;

            if (message)
            {
                Task.Run(() =>
                {
                    MessageBoxResult resultt = MessageBox.Show("Es ist keine passende Python Version installiert \r\n" +
                                                                "Python jetzt installieren?", "Warnung!",
                                                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (resultt == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(pyDownload);
                    }
                });
            }
        }
        return exists;
    }


    public static string GetVersion()
    {
        string version = "-";

        if (GetPath() == "-") { return version; }

        Process cmd = new Process();
        cmd.StartInfo.FileName = "cmd.exe";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();
        cmd.StandardInput.WriteLine("python" + pyVersion + " --version");
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        cmd.WaitForExit();
        string result = cmd.StandardOutput.ReadToEnd();

        string[] lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (string line in lines)
        {
            if (line.Length < 6) { continue; }      // wrong line
            if (line.Substring(0, 6) == "Python")   // line with version
            {
                version = line.Substring(7);        // read rest of line (version)
            }
        }
        return version;
    }


    public static async void Run(string path, string argument = "")
    {
        if (!Installed()) { return; }

        await Task.Run(() =>
        {
            //Console.WriteLine("Running in a separate thread!");

            ProcessStartInfo pycheck = new ProcessStartInfo();
            pycheck.FileName = @"python" + pyVersion + ".exe";
            string command = "-u " + path + " " + argument;
            Console.WriteLine(command);
            pycheck.Arguments = @command;
            pycheck.UseShellExecute = false;
            pycheck.RedirectStandardOutput = true;
            pycheck.RedirectStandardError = true;
            pycheck.CreateNoWindow = true;

            Process process = new Process();
            process.OutputDataReceived += Process_OutputDataReceived;
            process.ErrorDataReceived += Process_ErrorDataReceived;
            process.StartInfo = pycheck;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        });
    }





    // ##################### CALLBACKS #####################

    private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        try
        {
            if (e.Data != null)
            {
                //Console.WriteLine(e.Data.ToString());
                RX.Invoke(e.Data.ToString());

                if (e.Data.ToString().Contains("Finished .py-script successfully!"))
                {
                    Finished?.Invoke(null);
                }
            }
        }
        catch { return; }
    }

    private static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null) { return; }      // suppress useless errors
        if (e.Data.ToString().Length >= 8) { if (e.Data.ToString().Substring(0, 8) == "NoneType") { return; } }
        errMsg += e.Data.ToString();         // Add line to error message

        if (debug)
        {
            if (errShow == false)
            {
                errShow = true;

                Task.Run(() =>
                {
                    MessageBoxResult res = MessageBox.Show("Python wurde mit einem Fehler beendet! \r\nFehler anzeigen?", "Python error!", MessageBoxButton.YesNo, MessageBoxImage.Error);

                    if (res == MessageBoxResult.Yes)
                    {
                        MessageBox.Show(errMsg, "Error message", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    errMsg = "";
                    errShow = false;
                });
            }
        }
    }
}
