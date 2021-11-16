using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EasyModbus;
using System.Configuration;

namespace KebaP30c
{
    class Program
    {
        /*
        This sample demonstrates the KEBA P30c TCPModbus support to change the configured charging current of the charging station, 
        which is helpfull if you like to charge with available PV energy and optimze for selfconsumption.

        All required settings are in the App.config so that you can define three pre-configured options for the charging current. 
        Pre-configured options are default, medium, max. Values are set in Ampere. Max and Medium options could be run by providing the /max or /med commandline argument.
        So if you like to switch to fast charging, simply run the exe with /max argument.
        
        <add key="P30DeviceIP" value="192.168.2.1"/>
        <add key="P30UserCurrentAmpereDefault" value="6"/>
        <add key="P30UserCurrentAmpereMax" value="16"/>
        <add key="P30UserCurrentAmpereMed" value="11"/>
  
        A setting of 16A equals charging with 11kw - 6A is minimum, 32A maximum, depending on the configured hardware Dip switches of your charging station.
        */
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("KebaP30c configuration tool to change User Current\n" +
                                    "run KebaP30c.exe /? for Help");
                Console.WriteLine("---------------------------------------------------");

                //process command arguments
                switch (args.Length)
                {
                    case 0:
                        //No argument provided use default configuration from App.config
                        ConfigureCurrent(1000 * Int32.Parse(ConfigurationManager.AppSettings["P30UserCurrentAmpereDefault"]));
                        break;
                    case 1:
                        //Argument detected
                        var command = args[0];
                        switch (command)
                        {
                            case "/max":
                                ConfigureCurrent(1000 * Int32.Parse(ConfigurationManager.AppSettings["P30UserCurrentAmpereMax"]));
                                break;
                            case "/med":
                                ConfigureCurrent(1000 * Int32.Parse(ConfigurationManager.AppSettings["P30UserCurrentAmpereMed"]));
                                break;
                            case "/disable":
                                ChangeStationState(false);
                                break;
                            case "/enable":
                                ChangeStationState(true);
                                break;
                            case "/report2":
                                Report2();
                                break;
                            case "/?":
                                DisplayHelp();
                                break;
                            default:
                                Console.WriteLine("Argument is not supported, use /? for help");
                                return;
                        }
                        break;
                    default:
                        //Invalid number arguments
                        Console.WriteLine("Invalid number of arguments, use /? for help");
                        return;                 
                }
                Console.Write("Press any key to continue . . . ");
                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in Main : " + e.ToString());
            }

            void ConfigureCurrent(int p30UserCurrent)
            {
                try
                {
                    Console.WriteLine("Loading User Current (A) from config = " + (p30UserCurrent / 1000).ToString());

                    //Read DeviceIP from config
                    string p30DeviceIP = ConfigurationManager.AppSettings["P30DeviceIP"];

                    //Init TCP Modbus client
                    Console.WriteLine("Load Modus library....");
                    ModbusClient modbusClient = new ModbusClient(p30DeviceIP, 502);

                    //Connect KebaP30c 
                    Console.WriteLine("Connect KebaP30c at " + p30DeviceIP.ToString());
                    modbusClient.Connect();
                    Console.WriteLine("Connected : " + modbusClient.Connected.ToString());

                    //Set max charging User current
                    Console.WriteLine("Set Charging User current (A) : " + (p30UserCurrent / 1000).ToString());
                    modbusClient.WriteSingleRegister(5004, p30UserCurrent);

                    //Read Charging state - if charging disable/enable charging station to re-initiate charing session
                    int[] chargingStateRegisters = modbusClient.ReadHoldingRegisters(1000, 2);
                    if (chargingStateRegisters[1] == 3)
                    {
                        Console.WriteLine("Charging process is active will re-initiate charging session");
                        modbusClient.WriteSingleRegister(5014, 0);
                        Console.WriteLine("Disabled charging station");
                        Console.WriteLine("Wait 30sec ... so that the car can detect the stop of the chraging session");
                        System.Threading.Thread.Sleep(30000);
                        modbusClient.WriteSingleRegister(5014, 1);
                        Console.WriteLine("Enabled charging station. Charging session should resume now.");
                    }
                    Console.WriteLine("Done...");
                    //Disconnect modbus client
                    modbusClient.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in ConfigureCurrent : " + e.ToString());
                }
            }

            void ChangeStationState(bool state)
            {
                try
                {
                    string p30DeviceIP = ConfigurationManager.AppSettings["P30DeviceIP"];
                    ModbusClient modbusClient = new ModbusClient(p30DeviceIP, 502);

                    //Connect KebaP30c 
                    Console.WriteLine("Connect KebaP30c at " + p30DeviceIP.ToString());
                    modbusClient.Connect();
                    Console.WriteLine("Connected : " + modbusClient.Connected.ToString());

                    if (state == true)
                    {
                        Console.WriteLine("Enable charging station");
                        modbusClient.WriteSingleRegister(5014, 1);
                    }
                    else
                    {
                        Console.WriteLine("Disable charging station");
                        modbusClient.WriteSingleRegister(5014, 0);
                    }
                    Console.WriteLine("Done...");
                    //Disconnect modbus client
                    modbusClient.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in ChangeStationState : " + e.ToString());
                }
            }

            void Report2()
            {
                //Query report 2 via UDP
                Console.WriteLine("Query Report2 via UDP");
                UdpClient udpClient = new UdpClient(7090);
                try
                {
                    IPAddress ip = IPAddress.Parse(ConfigurationManager.AppSettings["P30DeviceIP"]);
                    udpClient.Connect(ip, 7090);

                    // Sends a message to the host to which you have connected.
                    Byte[] sendBytes = Encoding.ASCII.GetBytes("report 2");

                    udpClient.Send(sendBytes, sendBytes.Length);

                    IPEndPoint RemoteIpEndPoint = new IPEndPoint(ip, 7090);
                    Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                    string returnData = Encoding.ASCII.GetString(receiveBytes);

                    Console.WriteLine("Report 2 Response " + returnData.ToString());

                    udpClient.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in Report2 : " + e.ToString());
                }
            }

            void DisplayHelp()
            {
                Console.WriteLine(
                    "Supported Arguments:\n" +
                    "/disable to disable charging station\n" +
                    "/enable to enable charging station\n" +
                    "/report2 to dump config via UDP\n" + 
                    "/max to set the UserCurrentAmpereMax configuration from App.config\n" +
                    "/med to set the UserCurrentAmpereMed configuration from App.config\n");
            }
        }
    }
}