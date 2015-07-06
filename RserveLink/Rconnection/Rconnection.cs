#region Informations
/* Rconnection.cs : class and methods for Rsrv client connections
 * Copyright (C) 2006-5 Krzysztof Miodek
 * Many parts of code is translated from java 
 * 
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * $Id: Rconnection.cs, v 0.1 2006/04/30 23:33:00 Miodek Exp $
 */
#endregion

#region using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
#endregion

namespace RserveLink
{    
    /// <summary>
    /// Class providing TCP/IP connection to an Rserve
    /// </summary>
    public partial class Rconnection : IDisposable
    {
        private enum authTypes
        {
            plain = 0,
            crypt = 1
        }

        private String host;

        private Int32 port;

        private Socket s;

        private Int32 rserveVersion;

        private Int32 sendedBytesCount;

        private Int32 receivedBytesCount;

        private Boolean authReq = false;

        private authTypes ARtype = authTypes.plain;

        private String key;

        private const String myID = "Rsrv0102QAP1";
        
        /// <summary>
        /// Rserve version
        /// </summary>
        public Int32 RserveVersion
        {
            get
            {
                return (rserveVersion);
            }
        }

        /// <summary>
        /// Connection state. Return <c>true</c> if this connection is alive.    
        /// </summary>
        public Boolean IsConnected
        {
            get
            {
                if (s != null) return (s.Connected);
                return (false);
            }            
        }

        /// <summary>
        /// Count of sended Bytes.
        /// </summary>
        public Int32 SendedBytesCount
        {
            get { return (sendedBytesCount); }
        }

        /// <summary>
        /// Count of received Bytes.
        /// </summary>
        public Int32 ReceivedBytesCount
        {
            get { return (receivedBytesCount); }
        }

        /// <summary>
        /// Host IP address.
        /// </summary>
        public String Host
        {
            get { return (host); }
            set { host = value; }
        }

        /// <summary>
        /// Port.
        /// </summary>
        public Int32 Port
        {
            get { return (port); }
            set { port = value; }
        }

        /// <summary>
        /// Check authentication requirement sent by server. Return <c>true</c> is 
        /// server requires authentication. In such case first command after connecting 
        /// must be login.  
        /// </summary>
        public Boolean NeedLogin
        {
            get
            {
                return (authReq);
            }
        }
                
        /// <summary>
        /// Make a new Rconnection object with specified host and port.
	    /// </summary>
        /// <param name="Host">host Name/IP</param>
        /// <param name="Port">TCP port</param>
        public Rconnection(String Host, Int32 Port)
        {
            receivedBytesCount = sendedBytesCount = 0;
            host = Host;
            port = Port;
        }

        /// <summary>
        /// Make a new Rconnection object with default port and localhost.
	    /// </summary>
        public Rconnection()
        {
            receivedBytesCount = sendedBytesCount = 0;
            host = "127.0.0.1";
            port = 6311;
        }
        
        /// <summary>
        /// Receive response from Rserve
        /// </summary>
        /// <returns>response</returns>
        internal Byte[] GetResponse()
        {
            Byte[] response = new Byte[16];                                   //Array for response from host

            try
            {
                receivedBytesCount += s.Receive(response, 16, SocketFlags.None);                    //receive 16 bytes header with response
            }
            catch (Exception ex)
            {
                throw new RconnectionException(ex.Message);
            }
            
            if (response[4] != 0)
            {
                Int32 DataLength = BitConverter.ToInt32(response, 4);

                Byte[] DataReceived = new Byte[DataLength];
                receivedBytesCount += s.Receive(DataReceived, DataLength, SocketFlags.None);

                Byte[] allData = new Byte[16 + DataLength];
                Array.Copy(response, 0, allData, 0, 16);
                Array.Copy(DataReceived, 0, allData, 16, DataLength);                
                return allData;
            }
            return response;
        }

        /// <summary>
        /// Make connection to the Rserve.
        /// </summary>
        public void Connect()
        {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    
            IPAddress address = IPAddress.Parse(host);
            Byte[] dane = new Byte[32];
            Int32 recvCount = 0;
            
            try
            {
                IPEndPoint endPoint = new IPEndPoint(address, port);
                s.Connect(endPoint);
                recvCount = s.Receive(dane, 32, SocketFlags.None);    //receive data from host
                receivedBytesCount += 32;
            }
            catch (Exception ex)
            {
                throw new RconnectionException(ex.Message);
            }

            if (recvCount != 32)
            {
                throw new RconnectionException("Handshake failed: expected 32 bytes header, got " + recvCount.ToString());
            }

            /* checking that the response from Rserve is correct */
            String receivedID = ASCIIEncoding.ASCII.GetString(dane, 0, dane.Length);
            if (String.Compare(receivedID, 0, myID, 0, 4) != 0)
            {
                throw new RconnectionException("Invalid IDstring");
            }

            if (String.Compare(receivedID, 8, myID, 8, 4) != 0)
            {
                throw new RconnectionException("Protocol not supported");
            }

            rserveVersion = Convert.ToInt32(receivedID.Substring(4, 4));

            /* authentication checking */
            Int32 pos = 16;
            while (pos <= 27)
            {
                String tmp = receivedID.Substring(pos, 4);

                if (String.Equals(tmp, "ARuc"))
                {
                    authReq = true;
                    ARtype = authTypes.crypt;
                    key = receivedID.Substring(pos + 5, 3);                    
                }

                if (String.Equals(tmp, "ARpt"))
                {
                    if (!authReq)
                    {
                        authReq = true;
                        ARtype = authTypes.plain;
                    }
                }

                pos += 4;
            }

            
        }

        /// <summary>
        /// Didconnect with Rserve.
        /// </summary>
        public void Disconnect()
        {
            s.Close();
        }
        
        /// <summary>
        /// Assign a content of a REXP to a symbol in R. The symbol is created if 
        /// it doesn't exist already.
        /// </summary>
        /// <param name="symbol">
        /// Symbol name.It is the responsibility of the user to make sure that the symbol 
        /// name is valid in R. In fact R will always create the symbol, but it may not 
        /// be accessible (examples: "bar\nfoo" or "bar$foo").
        /// </param>
        /// <param name="input">Contents</param>
        public void Assign(String symbol, Rexp input)
        {
            Rmessage Rsymbol = new Rmessage(symbol);
            Rpacket packet = new Rpacket(RCommands.assignSEXP, Rsymbol, input);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }
                     
        /// <summary>
        /// Evaluates the given command without retriving the result
        /// </summary>
        /// <param name="Cmd">command</param>
        public void VoidEval(String Cmd)
        {
            Rmessage msg = new Rmessage(Cmd);
            Rpacket packet = new Rpacket(RCommands.voidEval, msg);

            s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(this.GetResponse());

            if (response.IsError)
            {
                throw new RconnectionException(response.ErrorCode);
            }
        }

        public void Dispose()
        {
            s.Close();
        }

        private Rexp parseEvalResponse(RpacketResponse packet)
        {
            Int32 rxo = 0;
            Byte[] data = packet.Content;
            
            if (rserveVersion > 100)
            { 
                /* since 0101 eval responds correctly by using DT_SEXP type/len header which is 4 bytes long */
                rxo = 4;
                
                /* we should check parameter type (should be DT_SEXP) and fail if it's not */
                if (data[0] != (byte)DataTypes.SEXP && data[0] != ((byte)DataTypes.SEXP | (byte)DataTypes.LARGE))
                {
                    throw new RconnectionException("Error while processing eval output: SEXP (type " + 
                        DataTypes.SEXP.ToString() + ") expected but found result type " + 
                        data[0].ToString() + ".");
                }

                if (data[0] == ((byte)DataTypes.SEXP | (byte)DataTypes.LARGE))
                {
                    rxo = 8; // large data need skip of 8 bytes
                }
                /* warning: we are not checking or using the length - we assume that only the one SEXP is returned. This is true for the current CMD_eval implementation, but may not be in the future. */
            }
            Rexp rx = new Rexp();
            if (data.Length > rxo)
            {                
                Rexp.parseREXP(out rx, ref data, rxo);
            }
            return rx;            
        }
                
        /// <summary>Evaluates the given command and retrieves the result</summary>
        /// <param name="Cmd">command</param>
        /// <returns>R-xpression</returns>        
        public Rexp Eval(String Cmd)
        {
            Rmessage message = new Rmessage(Cmd);                                   //Make message(command with header)
            Rpacket packet = new Rpacket(RCommands.eval, message);

            sendedBytesCount += s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
            RpacketResponse response = new RpacketResponse(GetResponse());

            if (response.IsOk && (response.ContentLength > 0))
            {
                return (parseEvalResponse(response));
            }
            else throw new RconnectionException("Eval failed");           
        }
        
        ///** login using supplied user/pwd. Note that login must be the first
	    ///command if used
	    ///@param user username
	    ///@param pwd password
	    ///@return returns <code>true</code> on success */
        public void login(String user, String pwd) 
        {
            if(authReq)
            {
                //if(!s.Connected);not connected
                if (ARtype == authTypes.crypt)
                {


                    String command = user + "\n" + UnixCrypt.crypt(pwd, key);
                    Rmessage msg = new Rmessage(command);
                    Rpacket packet = new Rpacket(RCommands.login, msg);

                    s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
                    RpacketResponse response = new RpacketResponse(this.GetResponse());

                    if (response.IsError)
                    {
                        throw new RconnectionException(response.ErrorCode);
                        //try {s.close();} catch (Exception e) {};
                        //is=null; os=null; s=null; connected=false;

                    }  
                }
                else
                {
                    String command = user + "\n" + pwd;
                    Rmessage msg = new Rmessage(command);
                    Rpacket packet = new Rpacket(RCommands.login, msg);

                    s.Send(packet.BinaryPacket, packet.PacketLength, SocketFlags.None);
                    RpacketResponse response = new RpacketResponse(this.GetResponse());

                    if (response.IsError)
                    {
                        throw new RconnectionException(response.ErrorCode);
                        //try {s.close();} catch (Exception e) {};
                        //is=null; os=null; s=null; connected=false;

                    }     
                }
			}
		}
		
    }
}
