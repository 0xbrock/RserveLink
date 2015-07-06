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
    internal class Rheader
    {
        /// <summary>
        /// Command
        /// </summary>
        Byte[] cmd = new Byte[4];

        /// <summary>
        /// length of the packet minus header (-16)
        /// </summary>
        Byte[] length = new Byte[4];

        /// <summary>
        /// data offset behind header (0)
        /// </summary>
        Byte[] dataOffset = new Byte[4];

        /// <summary>
        /// reserved - but must be sent so the minimal packet has 16 bytes
        /// </summary>
        Byte[] reserved = new Byte[4];

        public Rheader(RCommands Cmd, Int32 Length)
        {
            cmd = BitConverter.GetBytes((int)Cmd);
            length = BitConverter.GetBytes(Length);
        }

        public Rheader(RCommands Cmd)
        {
            cmd = BitConverter.GetBytes((int)Cmd);
        }

        public Byte[] BinaryHeader
        {
            get
            {
                Byte[] result = new Byte[16];
                Array.Copy(cmd,        0, result,  0, 4);
                Array.Copy(length,     0, result,  4, 4);
                Array.Copy(dataOffset, 0, result,  8, 4);
                Array.Copy(reserved,   0, result, 12, 4);
                return (result);
            }
        }
        
    }
}
