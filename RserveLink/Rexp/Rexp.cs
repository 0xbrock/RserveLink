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
    /// Representation of R-eXpressions.
    /// </summary>
    public partial class Rexp
    {                
        private static Int32 getLength(ref Byte[] buffer, Int32 offset)
        {
            return
            ((buffer[offset] & 64) > 0) ? // "long" format
            ((buffer[offset + 1] & 255) | ((buffer[offset + 2] & 255) << 8) | ((buffer[offset + 3] & 255) << 16) | ((buffer[offset + 4] & 255) << 24))
            :
            ((buffer[offset + 1] & 255) | ((buffer[offset + 2] & 255) << 8) | ((buffer[offset + 3] & 255) << 16));
        }

        private Int32 setHeaders(out Byte[] buffer, Int32 dataLength, Xpression xTp)
        {
            Int32 position = 8;
            Byte[] allLength;
            Byte[] sexpLength = BitConverter.GetBytes(dataLength);
                       
            if ((dataLength + 8) > 0xfffff0)
            {
                position = 16;
                buffer = new Byte[16 + dataLength];
                buffer[0] = (byte)(DataTypes.SEXP | DataTypes.LARGE);
                allLength = BitConverter.GetBytes(dataLength + 8);            
                Array.Copy(allLength, 0, buffer, 1, 4);

                buffer[8] = (byte)(xTp | Xpression.XT_LARGE);
                Array.Copy(sexpLength, 0, buffer, 9, 4);
            }
            else
            {
                buffer = new Byte[8 + dataLength];
                buffer[0] = (byte)DataTypes.SEXP;
                allLength = BitConverter.GetBytes(dataLength + 4);
                Array.Copy(allLength, 0, buffer, 1, 3);

                buffer[4] = (byte)xTp;
                Array.Copy(sexpLength, 0, buffer, 5, 3);
            }

            return (position);
        }

        private Byte[] getBinaryStringArray()
        {
            String[] values = (String[])data;
            Int32 allLength = 0;
            Int32 size = 4;
            Byte[] message;

            foreach (String current in values) allLength += (current.Length + 1);
            while (allLength > size) size += 4;

            Int32 pos = setHeaders(out message, size, Xpression.XT_ARRAY_STR);

            Byte[] sexp = BitConverter.GetBytes(allLength);
            if (pos == 16) Array.Copy(sexp, 0, message, 9, 4);
            else Array.Copy(sexp, 0, message, 5, 3);           

            foreach (String current in values)
            {
                Byte[] tmp = ASCIIEncoding.ASCII.GetBytes(current);
                Array.Copy(tmp, 0, message, pos, tmp.Length);
                pos += tmp.Length + 1;
            }

            return message;
        }
       
        internal Byte[] getBinaryRepresentation()
        {
            switch (type)
            {
                case Xpression.XT_DOUBLE:
                    Byte[] message;
                    setHeaders(out message, 8, Xpression.XT_DOUBLE);
                    Byte[] tmp = BitConverter.GetBytes((Double)data);
                    Array.Copy(tmp, 0, message, 8, 8);
                    return message;

                case Xpression.XT_INT:
                    setHeaders(out message, 4, Xpression.XT_INT);
                    tmp = BitConverter.GetBytes((Int32)data);
                    Array.Copy(tmp, 0, message, 8, 4);
                    return message;

                case Xpression.XT_ARRAY_INT:
                    Int32[] Ivalues = (Int32[])data;
                    Byte[] value = new Byte[4];
                    Int32 pos = setHeaders(out message, Ivalues.Length * 4, Xpression.XT_ARRAY_INT);
                    
                    foreach (Int32 current in Ivalues)
                    {
                        value = BitConverter.GetBytes(current);
                        Array.Copy(value, 0, message, pos, 4);
                        pos += 4;
                    }
                    return message;

                case Xpression.XT_ARRAY_DOUBLE:
                    Double[] Dvalues = (Double[])data;
                    value = new Byte[8];
                    pos = setHeaders(out message, Dvalues.Length * 8, Xpression.XT_ARRAY_DOUBLE);

                    foreach (Double current in Dvalues)
                    {
                        value = BitConverter.GetBytes(current);
                        Array.Copy(value, 0, message, pos, 8);
                        pos += 8;
                    }
                    return (message);

                case Xpression.XT_ARRAY_STR:
                    return (getBinaryStringArray());
            }
            throw new RconnectionException("Undefined conversion from type " + type.ToString() + "to binary");
        }

                 
        /// <summary>
        /// Get content of the Rexp as List of Rexp
        /// </summary>
        /// <returns>Vector content or <code>null</code> if the Rexp is no Vector</returns>
        public List<Rexp> AsVector()
        {            
            if (data != null)
            {
                return ((type == Xpression.XT_VECTOR) ? (List<Rexp>)data : null);
            }
            return null;
        }

        /// <summary>
        /// Get content of the Rexp as <see cref="Rbool"/>
        /// </summary>
        /// <returns><see cref="Rbool"/> content or <code>null</code> if the Rexp is no logical value</returns>
        public Rbool AsBool()
        {
            if (data != null)
            {
                return ((type == Xpression.XT_BOOL) ? (Rbool)data : null);
            }
            return null;
        }

        /// <summary>
        /// Get content of the Rexp as <see cref="Rfactor"/>
        /// </summary>
        /// <returns><see cref="Rfactor"/> content or <code>null</code> if the Rexp is no factor</returns>
        public Rfactor AsFactor()
        {
            if (data != null)
            {
                return ((type == Xpression.XT_FACTOR) ? (Rfactor)data : null);
            }
            return null;
        }

        /// <summary>
        /// Get content of the Rexp as an array of integers
        /// </summary>
        /// <returns>Int32[] content or <code>null</code> if the Rexp is not a array of integers</returns>
        public Int32[] AsIntArray()
        {
            if (data != null)
            {
                if (type == Xpression.XT_ARRAY_INT) return (Int32[])data;                
                if (type == Xpression.XT_INT)
                {
                    Int32[] values = new Int32[1];
                    values[0] = (Int32)data;
                    return values;
                }
            }
            return null;
        }

        /// <summary>
        /// Get content of the Rexp as Int32 
        /// </summary>
        /// <returns>Int32 content or 0 if the Rexp is no integer</returns>
        public Int32 AsInt32()
        {
            if (data != null)
            {
                if (type == Xpression.XT_INT) return ((Int32)data);
                if (type == Xpression.XT_ARRAY_INT)
                {
                    Int32[] values = (Int32[])data;
                    if (values.Length > 0) return values[0];
                }
            }
            return 0;
        }
        
        /// <summary>
        /// Get content of the Rexp as Double 
        /// </summary>
        /// <returns>Double content or 0.0 if the Rexp is no Double</returns>
        public Double AsDouble()
        {
            if (data != null)
            {
                if (type == Xpression.XT_ARRAY_DOUBLE)
                {
                    Double[] values = (Double[])data;
                    if (values.Length > 0) return values[0];                    
                }
                if (type == Xpression.XT_DOUBLE) return ((Double)data);                
            }
            return 0.0;
        }

        /// <summary>
        /// Get content of the Rexp as an array of doubles. 
        /// Array of integers, single double and single integer are automatically 
        /// converted into such an array if necessary.
        /// </summary>
        /// <returns>Double[] content or <code>null</code> if the Rexp is not a array of doubles or integers</returns>
        public Double[] AsDoubleArray()
        {
            if (data != null)
            {
                switch (type)
                {
                    case Xpression.XT_ARRAY_DOUBLE:
                        return (Double[])data;

                    case Xpression.XT_DOUBLE:
                        Double[] Dvalues = new Double[1];
                        Dvalues[0] = (Double)data;
                        return (Dvalues);                        

                    case Xpression.XT_ARRAY_INT:
                        Int32[] values = (Int32[])data;
                        if (values.Length > 0)
                        {
                            Double[] result = new Double[values.Length];
                            for (int i = 0; i < values.Length; i++)
                            {
                                result[i] = Convert.ToDouble(values[i]);
                            }
                            return result;
                        }
                        break;

                    case Xpression.XT_INT:
                        Int32 value = this.AsInt32();
                        Dvalues = new Double[1];
                        Dvalues[0] = Convert.ToDouble(value);
                        return (Dvalues);                        
                }                
            }
            return null;
        }
                
        /// <summary>
        /// Get content of the Rexp as <see cref="Rlist"/>        
        /// </summary>
        /// <returns><see cref="Rlist"/> content or <code>null</code> if the Rexp is no list</returns>
        public Rlist AsList()
        {
            return ((type == Xpression.XT_LIST) ? (Rlist)data : null);            
        }
               
        /// <summary>
        /// Returns the content of the Rexp as a matrix of doubles (2D-array: m[rows][cols]).
        /// </summary>
        /// <returns>2D array of doubles in the form double[rows][cols] or 
        /// <code>null</code> if the contents is no 2-dimensional matrix of doubles
        /// </returns>
        /// <example>Double[,] m=RconnectionObject.Eval("matrix(c(1,2,3,4,5,6),2,3)").AsDoubleMatrix();</example>
        public Double[,] AsDoubleMatrix()
        {
            if ((type != Xpression.XT_ARRAY_DOUBLE) || (rattribute == null)
                || (rattribute.Xtype != Xpression.XT_LIST))
            {
                return null;
            }

            Rexp dim = rattribute.AsList().Head;
            if (dim == null || dim.Xtype != Xpression.XT_ARRAY_INT) return null; // we need dimension attr
            Int32[] ds = dim.AsIntArray();
            if (ds == null || ds.Length != 2) return null; // matrix must be 2-dimensional

            Int32 m = ds[0];
            Int32 n = ds[1];
            Double[,] r = new Double[m, n];
            Double[] ct = AsDoubleArray();
            if (ct == null) return null;
            // R stores matrices as matrix(c(1,2,3,4),2,2) = col1:(1,2), col2:(3,4)
            // we need to copy everything, since we create 2d array from 1d array
            Int32 k = 0;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    r[i, j] = ct[k++];
                }                
            }         
            return r;
        }
               
        /// <summary>
        /// This is alias for <see cref="AsDoubleMatrix()"/>
        /// </summary>
        /// <returns></returns>
        public Double[,] AsMatrix()
        {
            return (AsDoubleMatrix());
        }

        /// <summary>
        /// Get content of the Rexp as string 
        /// </summary>
        /// <returns>string content or <code>null</code> if the Rexp is no string</returns>
        public String AsString()
        {
            if (data != null)
            {
                return ((type == Xpression.XT_STR) ? (String)data : null);
            }
            return null;
        }

        /// <summary>
        /// Converts the Rexp value of this instance to string representation
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[" + type.ToString() + " ");

            if (rattribute != null) sb.Append("\nattr=" + rattribute.ToString() + "\n ");

            switch (type)
            {
                case Xpression.XT_DOUBLE:
                    sb.Append(((Double)data).ToString());
                    break;

                case Xpression.XT_INT:
                    sb.Append(((Int32)data).ToString());
                    break;

                case Xpression.XT_BOOL:
                    sb.Append(((Rbool)data).ToString());
                    break;

                case Xpression.XT_FACTOR:
                    sb.Append(((Rfactor)data).ToString());
                    break;

                case Xpression.XT_ARRAY_DOUBLE:
                    Double[] Dvalues = (Double[])data;
                    sb.Append("(");
                    for (int i = 0; i < Dvalues.Length; i++)
                    {
                        sb.Append(Dvalues[i].ToString());
                        if (i < (Dvalues.Length - 1)) sb.Append(", ");
                        if (i == 99)
                        {
                            sb.Append("... (" + (Dvalues.Length - 100) + " more values follow)");
                            break;
                        }
                    }
                    sb.Append(")");
                    break;

                case Xpression.XT_ARRAY_INT:
                    Int32[] Ivalues = (Int32[])data;
                    sb.Append("(");
                    for (int i = 0; i < Ivalues.Length; i++)
                    {
                        sb.Append(Ivalues[i]);
                        if (i < (Ivalues.Length - 1)) sb.Append(", ");
                        if (i == 99)
                        {
                            sb.Append("... (" + (Ivalues.Length - 100) + " more values follow)");
                            break;
                        }
                    }
                    sb.Append(")");
                    break;

                case Xpression.XT_ARRAY_BOOL:
                    Rbool[] values = (Rbool[])data;
                    sb.Append("(");
                    for (int i = 0; i < values.Length; i++)
                    {
                        sb.Append(values[i]);
                        if (i < (values.Length - 1)) sb.Append(", ");
                        if (i == 99)
                        {
                            sb.Append("... (" + (values.Length - 100) + " more values follow)");
                            break;
                        }

                    }
                    sb.Append(")");
                    break;

                case Xpression.XT_STR:
                    sb.Append("\"");
                    sb.Append((String)data);
                    sb.Append("\"");
                    break;

                case Xpression.XT_SYM:
                    sb.Append((String)data);
                    break;

                case Xpression.XT_LIST:
                case Xpression.XT_LANG:
                    Rlist l = (Rlist)data;
                    sb.Append(l.Head.ToString());
                    sb.Append(" <-> ");
                    sb.Append(l.Body.ToString());
                    break;

                case Xpression.XT_UNKNOWN:
                    sb.Append((Int32)data);
                    break;

                case Xpression.XT_VECTOR:
                    List<Rexp> Lvalues = (List<Rexp>)data;
                    sb.Append("(");
                    for (Int32 i = 0; i < Lvalues.Count; i++)
                    {
                        sb.Append(Lvalues[i].ToString());
                        if (i < (Lvalues.Count - 1)) sb.Append(", ");
                    }
                    sb.Append(")");
                    break;
            }

            sb.Append("]");
            return sb.ToString();
        }
        
    }   
}
