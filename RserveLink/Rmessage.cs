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
using System.IO;
#endregion

namespace RserveLink
{    
    internal class Rmessage
    {
        private DataTypes Type;

        private Byte[] content;
               
        public Rmessage(String data)
        {
            Type = DataTypes.STRING;

            Byte[] input = ASCIIEncoding.ASCII.GetBytes(data);

            Int32 PacketLength = 4;
            while ((input.Length + 1) > PacketLength) PacketLength += 4;
            
            content = new Byte[4 + PacketLength];
            Array.Copy(input, 0, content, 4, input.Length);

            Byte[] length = BitConverter.GetBytes(PacketLength);
            Array.Copy(length, 0, content, 1, 3);
            content[0] = (byte)Type;
        }

        public Rmessage(Byte[] data)
        {
            Type = DataTypes.BYTESTREAM;
            Int32 position = 4;

            Int32 PacketLength = 4;
            while (data.Length > PacketLength) PacketLength += 4;

            Byte[] length = BitConverter.GetBytes(PacketLength);
            if (PacketLength > 0xffffff)
            {
                Type = (DataTypes.BYTESTREAM | DataTypes.LARGE);
                content = new Byte[8 + PacketLength];
                Array.Copy(length, 0, content, 1, 4);
                position = 8;
            }
            else
            {
                content = new Byte[4 + PacketLength];
                Array.Copy(length, 0, content, 1, 3);                
            }
            Array.Copy(data, 0, content, position, data.Length);
            content[0] = (byte)Type;         
        }

        public Rmessage(Byte data)
        {
            Type = DataTypes.BYTESTREAM;//setting type (Bitstream) in object field
            content = new Byte[8];      //create array with message and header of parameter 
            content[0] = 5;             //setting type (Bitstream)
            content[1] = 1;             //setting length (1 byte)
            content[4] = data;          //copying data to send
        }
        
        public Rmessage(Int32 data)
        {
            Type = DataTypes.INT;

            content = new Byte[8];
            Byte[] val = BitConverter.GetBytes(data);
            Byte[] btype = BitConverter.GetBytes((int)Type);

            Array.Copy(btype, 0, content, 0, 4);
            Array.Copy(val, 0, content, 4, 4);
            content[1] = 4;
        }

                
        public Byte[] Message
        {
            get
            {
                return content;
            }
        }

        public Int32 Length
        {
            get
            {
                return (content.Length);
            }
        }

        public DataTypes Dtype
        {
            get
            {
                return (Type);
            }
        }
    }
       
}
