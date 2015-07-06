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
    /// Represents errors that occur during Rserve connection
    /// </summary>
    public class RconnectionException : Exception
    {
        private String message;

        private Int32 errorCode;

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return (message);
            }
        }

        /// <summary>
        /// Gets a error code.
        /// </summary>
        public Int32 ErrorCode
        {
            get { return (errorCode); }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Message">Message about error</param>
        public RconnectionException(String Message)
        {
            message = Message;            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ErrorCode">Error code</param>
        public RconnectionException(Int32 ErrorCode)
        {
            errorCode = ErrorCode;
            switch (ErrorCode)
            {                
                case 65:
                    message = "Authentification failure";
                    break;
                case 66:
                    message = "Connection closed or broken packet killed it";
                    break;
                case 67:
                    message = "Unsupported/invalid command";
                    break;
                case 68:
                    message = "Some parameters are invalid";
                    break;
                case 69:
                    message = "R-error occured, usually followed by connection shutdown";
                    break;
                case 70:
                    message = "I/O error";
                    break;
                case 71:
                    message = "attempt to perform fileRead/Write on closed file";
                    break;
                case 72:
                    message = "the server deosn;t allow the user to issue the specified command";
                    break;
                case 73:
                    message = "unsupported command";
                    break;
                case 74:
                    message = "unknown command";
                    break;
                case 75:
                    message = "incoming packet is too big";
                    break;
                case 76:
                    message = "the requested object is too big to be transported in that way";
                    break;
                case 77:
                    message = "out of memory";
                    break;
                case 80:
                    message = "session is still busy";
                    break;
                case 81:
                    message = "unable to detach seesion";
                    break;
            }
        }

    }
}
