using System;
using System.Net.Sockets;
using System.Text;

namespace telnettest
{
    // https://github.com/9swampy/Telnet
    // dotnet add package Telnet --version 0.8.3
    // dotnet add package LiteGuard --version 2.2.0
    class ManualTest
    {
        // Interpret As Command (IAC)
        public const int IAC = unchecked((int)0xFF); // 255d

        public const int WILL = unchecked((int)0xFB); // 251d - WILL (option code) - https://tools.ietf.org/html/rfc854
        public const int WONT = unchecked((int)0xFC); // 252d - WON'T (option code) - https://tools.ietf.org/html/rfc854
        public const int DO = unchecked((int)0xFD); // 253d - DO (option code) - https://tools.ietf.org/html/rfc854
        public const int DONT = unchecked((int)0xFE); // 254d - DON'T (option code) - https://tools.ietf.org/html/rfc854


        public const int TERMINAL_TYPE = unchecked((int)0x18); // 24d - https://tools.ietf.org/html/rfc884
        public const int TERMINAL_SPEED = unchecked((int)0x20);
        public const int X_DISPLAY_LOCATION = unchecked((int)0x23);
        public const int NEW_ENVIRONMENT_OPTION = unchecked((int)0x27);

        public const int ECHO = unchecked((int)0x01);
        public const int SUPPRESS_GO_AHEAD = unchecked((int)0x03);
        public const int STATUS = unchecked((int)0x05);

        public const int NEGOTIATE_ABOUT_WINDOW_SIZE = unchecked((int)0x1F);

        public const int REMOTE_FLOW_CONTROL = unchecked((int)0x21);

        public const string HOST = "192.168.0.0";
        public const int PORT = 23;
        public const string USER = "";
        public const string PASSWORD = "";

        static void Main(string[] args)
        {
            Console.WriteLine("Start app ...");

            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(HOST, PORT);

                    Console.WriteLine("Port open");

                    var networkStream = tcpClient.GetStream();

                    Console.WriteLine("CanRead: {0} DataAvailable: {1} CanWrite: {2}", networkStream.CanRead, networkStream.DataAvailable, networkStream.CanWrite);

                    // ff fd 18 ff fd 20 ff fd 23 ff fd 27

                    // IAC DO TERMINAL_TYPE
                    // IAC DO TERMINAL_SPEED
                    // IAC DO X_DISPLAY_LOCATION
                    // IAC DO NEW_ENVIRONMENT_OPTION

                    // ff fb 03 ff fd 01 ff fd 1f ff fb 05 ff fd 21

                    // WILL Suppress Go Ahead --> Do Suppress Go Ahead
                    // Do Echo --> WILL echo ff fb 01
                    // Do Negotiate About Window Size --> WONT
                    // Will Status
                    // Do Remote Flow Control

                    // ff fe 01 ff fb 01

                    // Don't Echo
                    // Will Echo

                    // Negotiate and read until there is a LOGIN request
                    var payload = "";
                    StringBuilder stringBuilder = new StringBuilder();
                    while (!payload.EndsWith("login: "))
                    {
                        payload = readAndProcess(networkStream);
                        stringBuilder.Append(payload);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(stringBuilder);

                    // specify the username
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    string command = USER + "\r\n";
                    Console.WriteLine(command);
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(command);
                    networkStream.Write(data, 0, data.Length);

                    // read until the password prompt appears
                    payload = "";
                    stringBuilder = new StringBuilder();
                    while (!payload.EndsWith("Password: "))
                    {
                        payload = readAndProcess(networkStream);
                        stringBuilder.Append(payload);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(stringBuilder);

                    // send the password
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    command = PASSWORD + "\r\n";
                    Console.WriteLine(command);
                    data = System.Text.Encoding.ASCII.GetBytes(command);
                    networkStream.Write(data, 0, data.Length);

                    // read server's answer to password
                    payload = "";
                    stringBuilder = new StringBuilder();
                    while (!payload.EndsWith("$ "))
                    {
                        payload = readAndProcess(networkStream);
                        stringBuilder.Append(payload);
                    }
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(stringBuilder);

                    // send a command and read response
                    command = "ls -la";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    var commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "whoami";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "ifconfig";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "uname -r";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "pwd";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "cd /";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "pwd";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);

                    // send a command and read response
                    command = "for i in {1..10}; do echo $i; sleep 1s; done";
                    Console.WriteLine("");
                    Console.WriteLine("CLIENT");
                    Console.WriteLine(command);
                    commandOutput = executeCommand(networkStream, command);
                    Console.WriteLine("");
                    Console.WriteLine("SERVER");
                    Console.WriteLine(commandOutput);
                }
                catch (Exception)
                {
                    Console.WriteLine("Port closed");
                }
            }

            Console.WriteLine("App terminates.");
        }

        private static string executeCommand(NetworkStream networkStream, string command)
        {
            // convert to ASCII
            var data = System.Text.Encoding.ASCII.GetBytes(command + "\r\n");

            // write command out
            networkStream.Write(data, 0, data.Length);

            string payload = "";
            var stringBuilder = new StringBuilder();

            // read response until the command prompt shows up
            while (!payload.EndsWith("$ "))
            {
                payload = readAndProcess(networkStream);
                stringBuilder.Append(payload);
            }

            // for convenience: somehow telnet echoes the input command!
            // remove the echo from the output
            var commandOutput = stringBuilder.ToString();

            return commandOutput.Substring(command.Length);
        }

        /*
                public static string readStreamToString(NetworkStream networkStream)
                {
                    byte[] data = new byte[1024];
                    string recieved = "";

                    int size = networkStream.Read(data, 0, data.Length);
                    Console.WriteLine(ByteArrayToString(data));
                    recieved = Encoding.ASCII.GetString(data, 0, size);

                    return recieved;
                }
         
                public static string ByteArrayToString(byte[] ba)
                {
                    StringBuilder hex = new StringBuilder(ba.Length * 2);
                    foreach (byte b in ba)
                    {
                        hex.AppendFormat("{0:x2}", b);
                    }

                    return hex.ToString();
                }
        */

        /// <summary>
        /// Reads from the telnet connection.
        /// Will negotiate and send negotiation responses if it encounters IAC codes.!--
        /// Everything that does not start with an IAC code is read and returned.
        /// </summary>
        public static string readAndProcess(NetworkStream networkStream)
        {
            byte[] readdata = new byte[1024];
            byte[] writedata = new byte[1024];
            Array.Clear(writedata, 0, writedata.Length);

            int bytesRead = networkStream.Read(readdata, 0, readdata.Length);
            int bytesWritten = 0;

            string result = "";

            int i = 0;
            while (i < bytesRead)
            {
                if (IAC == readdata[i])
                {
                    i++;
                    if (DO == readdata[i])
                    {
                        i++;
                        if (TERMINAL_TYPE == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO TERMINAL_TYPE");

                            // Sender is willing to receive terminal type information in a
                            // subsequent sub-negotiation

                            // for every DO respond WONT
                            //byte[] response = new byte[] { IAC, WONT, TERMINAL_TYPE };
                            //networkStream.Write(response, 0, response.Length);

                            //byte[] response = new byte[] { IAC, WILL, TERMINAL_TYPE };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = TERMINAL_TYPE;
                            bytesWritten++;
                        }
                        else if (TERMINAL_SPEED == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO TERMINAL_SPEED");

                            //byte[] response = new byte[] { IAC, WONT, TERMINAL_SPEED };
                            //byte[] response = new byte[] { IAC, WILL, TERMINAL_SPEED };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = TERMINAL_SPEED;
                            bytesWritten++;
                        }
                        else if (X_DISPLAY_LOCATION == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO X_DISPLAY_LOCATION");

                            //byte[] response = new byte[] { IAC, WONT, X_DISPLAY_LOCATION };
                            //byte[] response = new byte[] { IAC, WILL, X_DISPLAY_LOCATION };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = X_DISPLAY_LOCATION;
                            bytesWritten++;
                        }
                        else if (NEW_ENVIRONMENT_OPTION == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO NEW_ENVIRONMENT_OPTION");

                            //byte[] response = new byte[] { IAC, WONT, NEW_ENVIRONMENT_OPTION };
                            //byte[] response = new byte[] { IAC, WILL, NEW_ENVIRONMENT_OPTION };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = NEW_ENVIRONMENT_OPTION;
                            bytesWritten++;
                        }
                        else if (ECHO == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO ECHO");

                            //byte[] response = new byte[] { IAC, WONT, NEW_ENVIRONMENT_OPTION };
                            //byte[] response = new byte[] { IAC, WILL, NEW_ENVIRONMENT_OPTION };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = ECHO;
                            bytesWritten++;
                        }
                        else if (NEGOTIATE_ABOUT_WINDOW_SIZE == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO NEGOTIATE_ABOUT_WINDOW_SIZE");

                            //byte[] response = new byte[] { IAC, WONT, NEW_ENVIRONMENT_OPTION };
                            //byte[] response = new byte[] { IAC, WILL, NEW_ENVIRONMENT_OPTION };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = NEGOTIATE_ABOUT_WINDOW_SIZE;
                            bytesWritten++;
                        }
                        else if (REMOTE_FLOW_CONTROL == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC DO REMOTE_FLOW_CONTROL");

                            //byte[] response = new byte[] { IAC, WONT, NEW_ENVIRONMENT_OPTION };
                            //byte[] response = new byte[] { IAC, WILL, NEW_ENVIRONMENT_OPTION };
                            //networkStream.Write(response, 0, response.Length);

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = WILL;
                            writedata[bytesWritten] = WONT;
                            bytesWritten++;
                            writedata[bytesWritten] = REMOTE_FLOW_CONTROL;
                            bytesWritten++;
                        }
                    }
                    else if (WILL == readdata[i])
                    {
                        i++;
                        if (SUPPRESS_GO_AHEAD == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC WILL SUPPRESS_GO_AHEAD");

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            writedata[bytesWritten] = DO;
                            //writedata[bytesWritten] = DONT;
                            bytesWritten++;
                            writedata[bytesWritten] = SUPPRESS_GO_AHEAD;
                            bytesWritten++;
                        }
                        else if (STATUS == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC WILL STATUS");

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = DO;
                            writedata[bytesWritten] = DONT;
                            bytesWritten++;
                            writedata[bytesWritten] = STATUS;
                            bytesWritten++;
                        }
                        else if (ECHO == readdata[i])
                        {
                            i++;
                            Console.WriteLine("IAC WILL ECHO");

                            writedata[bytesWritten] = IAC;
                            bytesWritten++;
                            //writedata[bytesWritten] = DO;
                            writedata[bytesWritten] = DONT;
                            bytesWritten++;
                            writedata[bytesWritten] = ECHO;
                            bytesWritten++;
                        }
                    }
                }
                else
                {
                    // regular data
                    result = Encoding.ASCII.GetString(readdata, i, bytesRead - i);
                    i += bytesRead - i;
                }
            }

            // for some reason, the data has to be sent on block
            // sending bytes individually causes the telnet server to go unresponsive
            if (bytesWritten > 0)
            {
                networkStream.Write(writedata, 0, bytesWritten);
            }

            return result;
        }
    }
}

// http://danzig.jct.ac.il/tcp-ip-lab/ibm-tutorial/3376c42.html
// https://networkengineering.stackexchange.com/questions/26424/how-do-the-telnet-requests-do-and-will-differ

/* https://tools.ietf.org/html/rfc854
NAME               CODE              MEANING

      SE                  240    End of subnegotiation parameters.
      NOP                 241    No operation.
      Data Mark           242    The data stream portion of a Synch.
                                 This should always be accompanied
                                 by a TCP Urgent notification.
      Break               243    NVT character BRK.
      Interrupt Process   244    The function IP.
      Abort output        245    The function AO.
      Are You There       246    The function AYT.
      Erase character     247    The function EC.
      Erase Line          248    The function EL.
      Go ahead            249    The GA signal.
      SB                  250    Indicates that what follows is
                                 subnegotiation of the indicated
                                 option.
      WILL (option code)  251    Indicates the desire to begin
                                 performing, or confirmation that
                                 you are now performing, the
                                 indicated option.
      WON'T (option code) 252    Indicates the refusal to perform,
                                 or continue performing, the
                                 indicated option.
      DO (option code)    253    Indicates the request that the
                                 other party perform, or
                                 confirmation that you are expecting
                                 the other party to perform, the
                                 indicated option.
      DON'T (option code) 254    Indicates the demand that the
                                 other party stop performing,
                                 or confirmation that you are no
                                 longer expecting the other party
                                 to perform, the indicated option.
      IAC                 255    Data Byte 255.
       */

/*
https://forum.arduino.cc/index.php?topic=111730.0
https://tools.ietf.org/html/rfc884
https://stackoverflow.com/questions/40066726/handling-telnet-negotiation

PACKET CONTENTS DESCRIPTION                   HEX VALUES

CLIENT--
Command: Will Negotiate About Window Size     ff fb 1f
Command: Will Terminal Speed                  ff fb 20
Command: Will Terminal Type                   ff fb 18
Command: Will New Environment Option          ff fb 27
Command: Do Echo                              ff fd 01
Command: Will Suppress Go Ahead               ff fb 03
Command: Do Suppress Go Ahead                 ff fd 03

SERVER--
Command: Do Terminal Type                     ff fd 18
Command: Do Terminal Speed                    ff fd 20
Command: Do X Display Location                ff fd 23
Command: Do New Environment Option            ff fd 27

CLIENT--
Command: Won't X Display Location             ff fc 23

SERVER--
Command: Do Negotiate About Window Size       ff fd 1f
Command: Will Echo                            ff fb 01
Command: Do Suppress Go Ahead                 ff fd 03
Command: Will Suppress Go Ahead               ff fb 03
Suboption Begin: Terminal Speed               ff fa 20
Option data                                   01
Command: Suboption End                        ff f0
Suboption Begin: New Environment Option       ff fa 27
Option data                                   01
Command: Suboption End                        ff f0
Suboption Begin: Terminal Type                ff fa 18
Send your Terminal Type                       01
Command: Suboption End                        ff f0

CLIENT--
Suboption Begin: Negotiate About Window Size  ff fa 1f
Width: 80                                     00 50
Height: 24                                    00 18
Command: Suboption End                        ff f0

CLIENT--
Suboption Begin: Terminal Speed               ff fa 20
Option data                                   00 33 38 34 30 30 2c 33 38 34 30 30
Command: Suboption End                        ff f0

CLIENT--
Suboption Begin: New Environment Option       ff fa 27
Option data                                   00
Command: Suboption End                        ff f0

CLIENT--
Suboption Begin: Terminal Type                ff fa 18
Here's my Terminal Type                       00
Value: XTERM                                  58 54 45 52 4d
Command: Suboption End                        ff f0

SERVER--
Command: Do Echo                              ff fd 01
Command: Will Status                          ff fb 05
Command: Do Remote Flow Control               ff fd 21

CLIENT--
Command: Won't Echo                           ff fc 01

CLIENT--
Command: Don't Status                         ff fe 05

CLIENT--
Command: Won't Remote Flow Control            ff fc 21

SERVER--
Data: login:                                  6c 6f 67 69 6e 3a 20

 */
