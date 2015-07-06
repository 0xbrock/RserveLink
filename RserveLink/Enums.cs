#region Informations
/* RserveLink library - client interface to Rserve
 * Copyright (C) 2006 Krzysztof Miodek
 * for licensing information see LICENSE file in the original RserveLink distribution 
 * Thanks for Simon Urbanek http://rosuda.org/Rserve/ - author Rserve*/
#endregion

using System;

namespace RserveLink
{
    /// <summary>
    /// Availiable Rserve commands 
    /// </summary>
    public enum RCommands : int
    {
        /// <summary>
        /// Login to Rserve
        /// </summary>
        login = 0x001,
        /// <summary>
        /// Evaluate without result sending
        /// </summary>
        voidEval = 0x002,
        /// <summary>
        /// Evaluate with result sending
        /// </summary>
        eval = 0x003,
        /// <summary>
        /// Shutdown Rserve
        /// </summary>
        shutdown = 0x004,
        /// <summary>
        /// Open a file on the Rserve
        /// </summary>
        openFile = 0x010,
        /// <summary>
        /// Create a file on the Rserve
        /// </summary>
        createFile = 0x011,
        /// <summary>
        /// Close a file on the Rserve
        /// </summary>
        closeFile = 0x012,
        /// <summary>
        /// Read file from Rserve
        /// </summary>
        readFile = 0x013,
        /// <summary>
        /// Write a file on the Rserve
        /// </summary>
        writeFile = 0x014,
        /// <summary>
        /// Remove a file from Rserve
        /// </summary>
        removeFile = 0x015,
        /// <summary>
        /// Set value on the Rserve (SEXP type)
        /// </summary>
        setSEXP = 0x020,
        /// <summary>
        /// Assign value on the Rserve (SEXP type)
        /// </summary>
        assignSEXP = 0x021,
        /// <summary>
        /// this commad allow clients to request bigger buffer sizes if large 
        /// data is to be transported from Rserve to the client.
        /// (incoming buffer is resized automatically)
        /// </summary>
        setBufferSize = 0x081
    }

    /// <summary>
    /// XpressionTypes
    /// </summary>
    public enum Xpression : int
    {
        /// <summary>
        /// xpression type: NULL    
        /// </summary>
        XT_NULL = 0,
        /// <summary>
        /// xpression type: integer
        /// </summary>
        XT_INT = 1,
        /// <summary>
        /// xpression type: double
        /// </summary>
        XT_DOUBLE = 2,
        /// <summary>
        /// xpression type: String
        /// </summary>
        XT_STR = 3,
        /// <summary>
        /// xpression type: language construct (currently content is same as list)
        /// </summary>
        XT_LANG = 4,
        /// <summary>
        /// xpression type: symbol (content is symbol name: String)
        /// </summary>
        XT_SYM = 5,
        /// <summary>
        /// xpression type: RBool
        /// </summary>
        XT_BOOL = 6,
        /// <summary>
        /// xpression type: Vector
        /// </summary>
        XT_VECTOR = 16,
        /// <summary>
        /// xpression type: RList
        /// </summary>
        XT_LIST = 17,
        /// <summary>
        /// xpression type: closure
        /// </summary>
        XT_CLOS = 18,
        /// <summary>
        /// xpression type: int[]
        /// </summary>
        XT_ARRAY_INT = 32,
        /// <summary>
        /// xpression type: double[]
        /// </summary>
        XT_ARRAY_DOUBLE = 33,
        /// <summary>
        /// xpression type: String[] (currently not used, Vector is used instead)
        /// </summary>
        XT_ARRAY_STR = 34,
        /// <summary>
        /// internal use only! this constant should never appear in a REXP
        /// </summary>
        XT_ARRAY_BOOL_UA = 35,
        /// <summary>
        /// xpression type: RBool[]
        /// </summary>
        XT_ARRAY_BOOL = 36,
        /// <summary>
        /// xpression type: unknown; no assumptions can be made about the content
        /// </summary>
        XT_UNKNOWN = 48,
        /// <summary>
        /// xpression type: RFactor; this XT is internally generated 
        /// (ergo is does not come from Rsrv.h) to support RFactor 
        /// class which is built from XT_ARRAY_INT
        /// </summary>
        XT_FACTOR = 127,
        /// <summary>
        /// new in 0102: if this flag is set then the length of the object
        /// is coded as 56-bit integer enlarging the header by 4 bytes 
        /// </summary>
        XT_LARGE = 64,
        /// <summary>
        /// flag; if set, the following REXP is the attribute 
        /// </summary>
        XT_HAS_ATTR = 128
    }

    /// <summary>
    /// Data types for the transport protocol (QAP1)
    /// </summary>
    public enum DataTypes : int
    {
        /// <summary>
        /// int
        /// </summary>
        INT = 1,
        /// <summary>
        /// char
        /// </summary>
        CHAR = 2,
        /// <summary>
        /// double
        /// </summary>
        DOUBLE = 3,
        /// <summary>
        /// 0 terminated string
        /// </summary>
        STRING = 4,
        /// <summary>
        /// stream of bytes (unlike DT_STRING may contain 0)
        /// </summary>
        BYTESTREAM = 5,
        /// <summary>
        /// encoded SEXP
        /// </summary>
        SEXP = 10,
        /// <summary>
        /// array of objects (i.e. first 4 bytes specify how many subsequent
        /// objects are part of the array; 0 is legitimate
        /// </summary>
        ARRAY = 11,
        /// <summary>
        /// if this flag is set then the length of the object is coded as 56-Bit
        /// integer enlarging the header by 4 bytes
        /// </summary>
        LARGE = 64
    }

    /// <summary>
    /// Logical states for Rbool data type  
    /// </summary>
    public enum RBoolValues : int
    {
        /// <summary>
        /// True
        /// </summary>
        TRUE = 1,
        /// <summary>
        /// False
        /// </summary>
        FALSE = 0,
        /// <summary>
        /// Not available
        /// </summary>
        NA = 2
    }
}