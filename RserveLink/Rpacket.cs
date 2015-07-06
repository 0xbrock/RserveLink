#region Informations
/* RserveLink library - client interface to Rserve
 * Copyright (C) 2006 Krzysztof Miodek
 * for licensing information see LICENSE file in the original RserveLink distribution 
 * Thanks for Simon Urbanek http://rosuda.org/Rserve/ - author Rserve*/
#endregion

#region using Directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace RserveLink
{
    /// <summary>
    /// Class store packet to Rserve with message and header
    /// </summary>
    internal class Rpacket
    {        
        private Rheader header;

        private Byte[] message;
       
        public Rpacket(RCommands RCommand)
        {
            header = new Rheader(RCommand);            
        }

        public Rpacket(RCommands RCommand, Rmessage Param)
        {
            message = Param.Message;
            header = new Rheader(RCommand, Param.Message.Length);
        }

        public Rpacket(RCommands RCommand, Rmessage Param1, Rmessage Param2)
        {
        }

        public Rpacket(RCommands RCommand, Rmessage Param, Rexp Value)
        {
            /* Create array to message */
            Byte[] binValue = Value.getBinaryRepresentation();
            
            Int32 ValueLength = binValue.Length;
            Int32 ParamLength = Param.Message.Length;
            
            message = new Byte[ParamLength + ValueLength];

            header = new Rheader(RCommand, (ParamLength + ValueLength));
            Array.Copy(Param.Message, 0, message, 0, ParamLength);
            Array.Copy(binValue, 0, message, ParamLength, ValueLength);
        }


        public Byte[] BinaryPacket
        {
            get
            {
                if (message != null)
                {
                    Byte[] data = new Byte[16 + message.Length];
                    Array.Copy(header.BinaryHeader, 0, data, 0, 16);
                    Array.Copy(message, 0, data, 16, message.Length);
                    return (data);
                }
                return header.BinaryHeader;
            }
        }

        public Int32 PacketLength
        {
            get
            {
                return (message != null) ? (16 + message.Length) : 16;                
            }
        }
          
    }

    /// <summary>
    /// Class store packet with response from Rserve
    /// </summary>
    internal class RpacketResponse
    {
        /// <summary>
        /// Header of response from Rserve
        /// </summary>
        private Byte[] header = new Byte[16];

        /// <summary>
        /// Content of response from Rserve
        /// </summary>
        private Byte[] content;
        

        public RpacketResponse(Byte[] data)
        {
            Array.Copy(data, 0, header, 0, 16);

            Int32 Count = data.Length - 16;
            if (Count > 0)
            {
                content = new Byte[Count];
                Array.Copy(data, 16, content, 0, Count);
            }
        }


        public Boolean IsOk
        {
            get
            {
                if (header[0] == 1) return true;
                return false;
            }
        }

        public Boolean IsError
        {
            get
            {
                if (header[0] == 2) return true;
                return false;
            }
        }

        public Byte[] Content
        {
            get
            {
                return (content);
            }
        }

        public Int32 ContentLength
        {
            get
            {
                if (content != null) return content.Length;
                return 0;
            }
        }

        public Int32 ErrorCode
        {
            get
            {
                return ((Int32)header[3]);
            }
        }
        
    }
}
