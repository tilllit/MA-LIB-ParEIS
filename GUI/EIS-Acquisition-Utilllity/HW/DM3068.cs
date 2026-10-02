using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


public class DM3068
{


    TCPClient clientMM = new TCPClient();

    public delegate void EventMeasurementFinishedHandler();
    public event EventMeasurementFinishedHandler EventMeasurementFinished;

    public delegate void BlobInEvent();
    public event BlobInEvent new_blob;

    Timer cyclic_communication_timer = new Timer();


    public delegate void MeasureEvent(double val);
    public event MeasureEvent NewData;


    public double Voltage;
    public Func<double> parameter;

    public async Task<bool> Start(string address, int port)
    {
        // Ping Host and Port
        if (!(await Task.Run(() => IsPortOpen(address, port, 5000).Result)))
        {
            MessageBox.Show("No connection to Device!", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }


        clientMM.create(address, port);
        clientMM.TCP_RX_Callback += RX_data;
        Send("CMDSET RIGOL");

        return true;

        //cyclic_communication_timer.Interval = 200;
        //cyclic_communication_timer.Elapsed += GetFunction;
        //cyclic_communication_timer.Start();
    }


    public void Stop()
    {
        cyclic_communication_timer.Stop();
        clientMM.TCP_RX_Callback -= RX_data;
        clientMM.Close();
    }



    private void RX_data(object sender, EventArgs e)
    {
        byte[] buff = (byte[])sender;
        string payload = Encoding.ASCII.GetString(buff);

        double res = Convert.ToDouble(payload, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

        NewData?.Invoke(res);

        // Handle incoming data here

        //Console.WriteLine("Multimeter: " + res);
    }


    public void GetFunction(object source, ElapsedEventArgs e)
    {
        cyclic_communication_timer.Stop();
        try
        {
            Voltage = GetVoltageDC();
            //GetVoltageAC();
            //GetCurrentDC();
            //GetCurrentAC();
            //GetResistance();
            //GetFrequency();
            //GetPeriod();
            //GetDiode();
            //GetCapacitance();
            //GetContinuity();


            // Invoke blob update event
            new_blob?.Invoke();
        }
        catch { }
        cyclic_communication_timer.Start();
    }





    public double Send(string request, bool response = false)
    {
        // Buffer
        byte[] data = new byte[265];

        // ignore old data
        //clientDE.stream.FlushAsync();

        // Send request for actual measurement
        clientMM.send(request + "\n");

        return 0.0;

        //if (response)
        //{
        //    // Get data from stream
        //    int amount_of_data = clientDE.client.stream.Read(data, 0, data.Length);
        //    // Encode
        //    string responsedata = Encoding.ASCII.GetString(data, 0, amount_of_data);

        //    Debug.WriteLine(responsedata);


        //    double res = Convert.ToDouble(responsedata, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
        //    Debug.WriteLine(res);
        //    return res;
        //}
        //else
        //{
        //    return 0;
        //}

    }



    public double GetVoltageDC()
    {
        string message = ":MEASure:VOLTage:DC?";
        double res = Send(message, true);
        return res;
    }

    public double GetVoltageAC()
    {
        string message = ":MEASure:VOLTage:AC?";
        double res = Send(message, true);
        return res;
    }

    public double GetCurrentDC()
    {
        string message = ":MEASure:CURRent:DC?";
        double res = Send(message, true);
        return res;
    }

    public double GetCurrentAC()
    {
        string message = ":MEASure:CURRent:AC?";
        double res = Send(message, true);
        return res;
    }

    public double GetResistance()
    {
        string message = ":MEASure:RESistance?";
        double res = Send(message, true);
        return res;
    }

    public double GetFrequency()
    {
        string message = ":MEASure:FREQuency?";
        double res = Send(message, true);
        return res;
    }

    public double GetPeriod()
    {
        string message = ":MEASure:PERiod?";
        double res = Send(message, true);
        return res;
    }

    public double GetDiode()
    {
        string message = ":MEASure:DIODe?";
        double res = Send(message, true);
        return res;
    }

    public double GetCapacitance()
    {
        string message = ":MEASure:CAPacitance?";
        double res = Send(message, true);
        return res;
    }

    // Durchgangsprüfung
    public double GetContinuity()
    {
        string message = ":MEASure:CONTinuity?";
        double res = Send(message, true);
        return res;
    }

    public double Measure(Func<double> Type)
    {
        return Type();
    }

    private void Measure(object sender, ElapsedEventArgs e)
    {
        parameter();
    }

    public void StartBlob(Func<double> Type)
    {
        parameter = Type;
        cyclic_communication_timer.Interval = 200;
        cyclic_communication_timer.Elapsed += Measure;
        cyclic_communication_timer.Start();
    }

    public void StopBlob()
    {
        cyclic_communication_timer.Elapsed -= Measure;
        cyclic_communication_timer.Stop();
    }

    public void BEEP()
    {
        string message = "SYSTem:BEEPer";
        Send(message);
    }

    //public bool GetError()
    //{
    //    // Buffer
    //    byte[] data = new byte[265];

    //    // ignore old data
    //    clientDE.stream.FlushAsync();

    //    string message = "SYSTem:ERRor?";

    //    try
    //    {
    //        // Send request for actual measurement
    //        clientDE.send_tcp(message);

    //        // Get data from stream
    //        int amount_of_data = clientDE.stream.Read(data, 0, data.Length);
    //        // Encode
    //        string responsedata = Encoding.ASCII.GetString(data, 0, amount_of_data);
    //        var splits = responsedata.Split(',');

    //        Debug.WriteLine(responsedata);

    //        if (splits[0] == "1") { return true; }
    //        else { return false; }
    //    }
    //    catch { return true; }
    //}






    // this works to check for open port
    private static async Task<bool> IsPortOpen(string host, int port, int timeoutMs = 2000)
    {
        try
        {
            var TCPclient = new TcpClient();
            var connectTask = TCPclient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeoutMs);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            // Wenn ConnectTask fertig ist UND erfolgreich war:
            if (completedTask == connectTask)
            {
                // Abwarten, ob dabei wirklich eine Verbindung zustande kam (durch await)
                await connectTask;
                return true;
            }
            else
            {
                // Timeout
                return false;
            }
        }
        catch (SocketException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }



}








//CMDSET AGILENT

//DISPlay 1
//DISPlay 0

//MEASure:VOLTage: DC?
//MEASure:VOLTage: AC?





//CMDSET RIGOL

//SYSTem:BEEPer
//SYSTem:BEEPer: STATe?
//SYSTem:BEEPer: STATe 0
//SYSTem: BEEPer: STATe 1

//SYSTem: ERRor ?



//: MEASure:VOLTage: DC ?
//: MEASure:VOLTage: AC ?

//: MEASure:CURRent: DC ?
//: MEASure:CURRent: AC ?

//: MEASure:RESistance ?

//: MEASure:FREQuency ?

//: MEASure:PERiod ?

//: MEASure:DIODe ?

//: MEASure:CAPacitance ?

//: MEASure:CONTinuity ?
