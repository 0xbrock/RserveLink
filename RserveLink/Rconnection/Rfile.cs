#region Informations
/* RserveLink library - client interface to Rserve
 * Copyright (C) 2006 Krzysztof Miodek
 * for licensing information see LICENSE file in the original RserveLink distribution 
 * Thanks for Simon Urbanek http://rosuda.org/Rserve/ - author Rserve*/
#endregion

#region using Directives
using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
#endregion

namespace RserveLink
{
    /// <summary> 
    /// </summary>
    /// <example>
    /// This example show to order files on the Rserve.
    /// <code>
    /// using System;
    /// using System.Collections.Generic;
    /// using System.Text;
    /// using System.IO;
    /// using RserveLink;
    ///
    /// namespace rservelinktest
    /// {
    ///    class Program
    ///    {
    ///        static void Main()
    ///        {
    ///            /* create connection with Rserve */
    ///            Rconnection MyConnection = new Rconnection();
    ///            MyConnection.Connect();
    ///
    ///            /* create file, write text and close */
    ///            MyConnection.CreateFile("example1.txt");
    ///            MyConnection.WriteFile("This is example1 text");
    ///            MyConnection.CloseFile();
    ///
    ///            /* create file on our side */
    ///            FileStream fs = File.Create("example2.txt");
    ///            fs.Write(ASCIIEncoding.ASCII.GetBytes("This is example2 text"), 0, 21);
    ///            fs.Close();
    ///
    ///            /* open file and copy to the Rserve */
    ///            fs = File.Open("example2.txt", FileMode.Open);
    ///            MyConnection.CreateFile("example2.txt");
    ///            MyConnection.PacketSize = 4;        //max packet length 16 (header) + 4 (data) bytes 
    ///            MyConnection.WriteFile((Stream)fs);
    ///            MyConnection.CloseFile();
    ///            fs.Close();
    ///
    ///            /* read files from Rserve */
    ///            String response;
    ///            MyConnection.OpenFile("example1.txt");
    ///            MyConnection.ReadFile(32, out response);
    ///            MyConnection.CloseFile();
    ///            Console.WriteLine("example1.txt: " + response);
    ///
    ///            MyConnection.OpenFile("example2.txt");
    ///            MyConnection.ReadFile(32, out response);
    ///            MyConnection.CloseFile();
    ///            Console.WriteLine("example2.txt: " + response);
    ///
    ///            MyConnection.Disconnect();
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>
    public partial class Rconnection
    {
        /// <summary>
        /// Max packet length without header
        /// </summary>
        private Int32 packetSize = 128;

        /// <summary>
        /// Max length (in bytes) of data which will be send to Rserve, when we use
        /// WriteFile(Stream) method. This is only data length, all packet consist of
        /// data and header (always 16 bytes).
        /// </summary>
        public Int32 PacketSize
        {
            get
            {
                return (packetSize);
            }
            set
            {
                packetSize = value;
            }
        }

        /// <summary>
        /// Open a file on the Rserve for reading
        /// </summary>
        /// <param name="FileName">
        /// File name should not contain any path delimiters, 
        /// since Rserve may restrict the access to local working directory.
        /// </param>
        public void OpenFile(String FileName)
        {
            Rmessage msg = new Rmessage(FileName);
            Rpacket packet = new Rpacket(RCommands.openFile, msg);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }

        /// <summary>
        /// Create a file on the Rserve for writing
        /// </summary>
        /// <param name="FileName">
        /// File name should not contain any path delimiters, 
        /// since Rserve may restrict the access to local working directory.
        /// </param>
        public void CreateFile(String FileName)
        {
            Rmessage msg = new Rmessage(FileName);
            Rpacket packet = new Rpacket(RCommands.createFile, msg);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }
        
        /// <summary>
        /// Remove a file on the Rserve
        /// </summary>
        /// <param name="FileName">
        /// File name should not contain any path delimiters, since 
        /// Rserve may restrict the access to local working directory.
        /// </param>
        public void RemoveFile(String FileName)
        {
            Rmessage msg = new Rmessage(FileName);
            Rpacket packet = new Rpacket(RCommands.removeFile, msg);

            s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }

        /// <summary>
        /// Close file on the Rserve         
        /// </summary>
        public void CloseFile()
        {
            Rpacket packet = new Rpacket(RCommands.closeFile);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }
                
        /// <summary>
        /// Method write string value to the remote file
        /// </summary>
        /// <param name="input">value to send</param>
        public void WriteFile(String input)
        {
            Rmessage msg = new Rmessage(ASCIIEncoding.ASCII.GetBytes(input));
            Rpacket packet = new Rpacket(RCommands.writeFile, msg);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }
                
        /// <summary>
        /// Method write string value to the remote file
        /// </summary>
        /// <param name="input">value to send</param>
        public void WriteFile(Stream input)
        {
            while (input.Position != input.Length)
            {
                Byte[] data;
            
                if ((input.Length - input.Position) > packetSize) data = new Byte[packetSize];
                else data = new Byte[input.Length - input.Position];

                input.Read(data, 0, data.Length);
                Rmessage msg = new Rmessage(data);
                Rpacket packet = new Rpacket(RCommands.writeFile, msg);

                sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
                RpacketResponse response = new RpacketResponse(this.GetResponse());

                if (response.IsError)
                {
                    throw new RconnectionException(response.ErrorCode);
                }
            }            
        }

        /// <summary>
        /// Reads specified number of bytes (or less) from the remote file.
        /// </summary>
        /// <param name="size">maximal number of bytes to read</param>
        /// <param name="Value">String value to store the read and ASCII decoded bytes</param>
        public void ReadFile(Int32 size, out String Value)
        {
            Rmessage msg = new Rmessage(size);
            Rpacket packet = new Rpacket(RCommands.readFile, msg);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsOk)
            {
                Value = ASCIIEncoding.ASCII.GetString(response.Content, 0, response.Content.Length * 2);
            }
            else
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }

        /// <summary>
        /// Reads specified number of bytes (or less) from the remote file.
	    /// </summary>
        /// <param name="size">maximal number of bytes to read</param>
        /// <param name="data">buffer to store the read bytes</param>
        public void ReadFile(Int32 size, out Byte[] data)
        {
            Rmessage msg = new Rmessage(size);
            Rpacket packet = new Rpacket(RCommands.readFile, msg);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsOk)
            {
                data = response.Content;
            }
            else
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }
    }
}