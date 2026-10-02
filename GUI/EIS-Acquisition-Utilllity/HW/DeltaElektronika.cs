using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;


public class DeltaElektronika
{
    // 07.06.2022 Erik Zinger
    // default communication interval: 5 Hz
    // do not use as closed loop controller
    // maximum voltage and current values must not be changed (internal scaling fails in this case)

    // default IP:      10.1.0.101
    // defult port:     8462

    // Delta Elektronika
    //Create new instance of client
    TCPClient clientDE = new TCPClient();

    //Create new timer for continous TCP message receive
    System.Timers.Timer cyclic_communication_timer = new System.Timers.Timer();

    //status changed event
    public delegate void EventMeasurementFinishedHandler();
    public event EventMeasurementFinishedHandler EventMeasurementFinished;

    // serial number string
    private string identificationString = null;

    // values
    public double voltage;
    public double voltageSWLimitMax = 70;   // default to prevent damage
    public double current;
    public double currentSWLimitMax = 24;   // defaul to prevent damage
    public bool enabled = false;

    public double voltageSetpoint;
    public double currentSetpoint;


    //Buffer queue of tcp requests
    private Queue<string> tcp_request_fifo = new Queue<string>();


    // TCP Delta Elektronika initialization
    // address - IP string format "aaa:bbb:ccc:ddd"
    // port - range 0 - 65535
    public async Task<bool> Start(string address, int port)
    {

        // Ping Host and Port
        if (!(await Task.Run(() => IsPortOpen(address, port, 5000).Result)))
        {
            MessageBox.Show("No connection to Device!", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        clientDE.create(address, port);
        clientDE.TCP_RX_Callback += RX_data;

        return true;
    }

    // Stop TCP Delta Elektronika
    public void Stop()
    {
        cyclic_communication_timer.Stop();
        clientDE.Close();
    }


    public void Send(string request)
    {
        clientDE.send(request + "\n");
    }


    private void RX_data(object sender, EventArgs e)
    {
        byte[] buff = (byte[])sender;
        string payload = Encoding.ASCII.GetString(buff);

        //double res = Convert.ToDouble(payload, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));

        // Handle incoming data here

        //Console.WriteLine("Multimeter: " + res);
    }


    //Cyclic function (timer controlled) to receive actual current and voltage values
    private void Cyclic_communication_function(object source, ElapsedEventArgs e)
    {
            //// proceed tcp_request_fifo
            //if (tcp_request_fifo.Count > 0)
            //{
            //    if (string.Equals(tcp_request_fifo.Peek(), "OUTP ON"))
            //    {
            //        enabled = true;
            //    }
            //    else
            //    {
            //        if (string.Equals(tcp_request_fifo.Peek(), "OUTP OFF"))
            //        {
            //            enabled = false;
            //        }
            //    }

            //    // remove item from queue and returns item
            //    clientDE.send_tcp(tcp_request_fifo.Dequeue());
            //}
            //else
            //{
            //    // get actual voltage measurement                
            //    voltage = GetMeasurement("MEAS:VOLT?");
            //    // get actual current measurement
            //    current = GetMeasurement("MEAS:CURR?");

            //    EventMeasurementFinished?.Invoke();
            //}
    }






    public void OutputEnable()
    {
        //tcp_request_fifo.Enqueue("OUTP ON");
        //enabled = true; // not executed at this point
        Send("OUTP ON");
    }

    public void OutputDisable()
    {
        //tcp_request_fifo.Enqueue("OUTP OFF");
        //enabled = false; // not executed at this point
        Send("OUTP OFF");
    }

    public void SetVoltage(double voltageSP)
    {
        //Send voltage if number is between 0 and 52V
        if (voltageSP >= 0 && voltageSP <= 52 && voltageSP <= voltageSWLimitMax)
        {
            voltageSetpoint = voltageSP;
            //tcp_request_fifo.Enqueue("SOUR:VOLT " + voltageSP.ToString("0.00", CultureInfo.GetCultureInfo("en-GB")));
            Send("SOUR:VOLT " + voltageSP.ToString("0.000", CultureInfo.GetCultureInfo("en-US")));
            //Console.WriteLine("Voltage set to " + voltageSP.ToString("0.00") + " V");
        }
        else
        {
            Console.WriteLine("Voltage out of range");
        }
    }

    public double GetVoltage()
    {
        return voltage;
    }

    public void SetCurrent(double currentSP)
    {
        //Send current if number is between 0 and 60A
        if (currentSP >= 0 && currentSP <= 60 && currentSP <= currentSWLimitMax)
        {
            currentSetpoint = currentSP;
            //tcp_request_fifo.Enqueue("SOUR:CURR " + currentSP.ToString("0.00", CultureInfo.GetCultureInfo("en-GB")));
            Send("SOUR:CURR " + currentSP.ToString("0.000", CultureInfo.GetCultureInfo("en-US")));
            //Console.WriteLine("Curret set to " + currentSP.ToString("0.00") + " A");
        }
        else
        {
            Console.WriteLine("Current out of range");
        }
    }

    public double GetCurrent()
    {
        return current;
    }

    public string GetIdentificationString()
    {
        return identificationString;
    }

    public bool IsOutputEnabled()
    {
        return enabled;
    }

    //private double GetMeasurement(string request)
    //{
    //    // Buffer
    //    byte[] data = new byte[265];

    //    // ignore old data
    //    clientDE.stream.FlushAsync();

    //    // Send request for actual measurement
    //    clientDE.send_tcp(request);
    //    // Get data from stream
    //    int amount_of_data = clientDE.stream.Read(data, 0, data.Length);
    //    // Encode
    //    string responsedata = Encoding.ASCII.GetString(data, 0, amount_of_data);

    //    // Return value as double with 4 decimal places
    //    return Convert.ToDouble(responsedata) / 10000.0f;
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

