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
    public partial class Rexp
    {
        /// <summary>
        /// Representation of R-eXpressions.
        /// </summary>
        public static Int32 parseREXP(out Rexp x, ref Byte[] buffer, Int32 offset)
        {            
            Int32 xl = getLength(ref buffer, offset);
            Boolean isLong = ((buffer[offset] & 64) != 0);
            Boolean hasAtt = ((buffer[offset] & 128) != 0);
            Xpression xt = (Xpression)(buffer[offset] & 63);

            if (isLong) offset += 4;
            offset += 4;

            Int32 eox = offset + xl;
            
            x = new Rexp();
            x.Xtype = xt;
            x.Attrib = null;
            
            Rexp xatrr;
            if (hasAtt)
            {               
                offset = parseREXP(out xatrr, ref buffer, offset);
                x.Attrib = xatrr;                
            }
            
            switch (xt)
            {
                case Xpression.XT_NULL:
                    x.Data = null;
                    return offset;

                case Xpression.XT_DOUBLE:
                    return (parseDouble(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_ARRAY_DOUBLE:
                    return (parseDoubleArray(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_INT:
                    return (parseInt(eox, offset, ref buffer, ref x));                    

                case Xpression.XT_UNKNOWN:
                    x.Data = BitConverter.ToInt32(buffer, offset);
                    offset = eox;
                    return offset;

                case Xpression.XT_CLOS:
                    return (parseClosure(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_BOOL:
                    return (parseBool(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_ARRAY_BOOL_UA:
                    return (parseBoolArrayUA(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_ARRAY_BOOL:
                    return (parseBoolArray(eox, offset, ref buffer, ref x));

                case Xpression.XT_ARRAY_INT:
                    return (parseIntArray(eox, offset, ref buffer, ref x));

                case Xpression.XT_VECTOR:
                    return (parseVector(eox, offset, ref buffer, ref x));
                    
                case Xpression.XT_STR:
                    return (parseString(eox, offset, ref buffer, ref x));

                case Xpression.XT_SYM:
                    return (parseSymbol(eox, offset, ref buffer, ref x));

                case Xpression.XT_LIST:
                case Xpression.XT_LANG:
                    return (parseList(eox, offset, ref buffer, ref x));

                default:
                    x.Data = null;
                    offset = eox;
#if WARNING
                    throw new RconnectionException("unhandeld type:" + xt.ToString());
#endif
                    return offset;
            }            
        }

        private static Int32 parseList(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Rlist rl = new Rlist();
            Rexp tmp;
            rl.Tag = null;

            offset = parseREXP(out tmp, ref buffer, offset); // CAR
            rl.Head = tmp;
            
            offset = parseREXP(out tmp, ref buffer, offset); // CDR
            rl.Body = tmp;
             
            if (offset != eox)
            {
                // if there is more data then it's presumably the TAG entry
                offset = parseREXP(out tmp, ref buffer, offset);
                rl.Tag = tmp;
                
                if (offset != eox)
                {
                    //System.out.println("Warning: list SEXP size mismatch\n");
                    offset = eox;
                }
            }
            x.Data = rl;
            return offset;
        }

        private static Int32 parseSymbol(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Rexp sym;
            offset = parseREXP(out sym, ref buffer, offset); // PRINTNAME that's all we will use
            String s = null;
            
            if (sym.Xtype == Xpression.XT_STR) s = (String)sym.Data; 
            else s = sym.ToString();
            x.Data = s; // content of a symbol is its printname string (so far)
            offset = eox;            
            return offset;
        }

        private static Int32 parseString(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {           
            Int32 i=offset;
            while (buffer[i]!=0 && i<eox) i++;
            String value = ASCIIEncoding.ASCII.GetString(buffer, offset, i - offset);
            offset = eox;
            x.Data = value;            
            return offset;        
        }

        private static Int32 parseBoolArrayUA(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Int32 Count = (eox - offset);
            x.Xtype = Xpression.XT_ARRAY_BOOL; // XT_ARRAY_BOOL_UA is only old transport type for XT_ARRAY_BOOL
            Rbool[] values = new Rbool[Count];

            for (int i = 0; i < Count; i++)
            {
                values[i] = new Rbool((RBoolValues)buffer[offset]);
                offset++;
            }
            
            x.Data = values;
            if (offset > buffer.Length) throw new Exception("bladBArrayU");
            
            return offset;
        }

        private static Int32 parseBoolArray(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Int32 Count = getLength(ref buffer, offset);
            offset += 4;
            Int32 i = 0;
            Rbool[] values = new Rbool[Count];

            while ((offset < eox) && (i < Count))
            {
                values[i] = new Rbool((RBoolValues)buffer[offset]);
                i++;
                offset++;
            }

            // skip the padding
            while ((i & 3) != 0)
            {
                i++;
                offset++;
            }
            x.Data = values;
            if (offset > buffer.Length) throw new Exception("bladBArray");
            
            return offset;
        }

        private static Int32 parseBool(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            x.Data = new Rbool((RBoolValues)buffer[offset]);
            offset++;
            if (offset != eox)
            {
                // o+3 could happen if the result was aligned (1 byte data + 3 bytes padding)
                if (eox != offset + 3) throw new RconnectionException("Warning: bool SEXP size mismatch");
                offset = eox;
            }
            if (offset > buffer.Length) throw new Exception("bladbool");
            
            return offset;
        }

        private static Int32 parseClosure(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Rexp form;
            Rexp body;
            offset = parseREXP(out form, ref buffer, offset);
            offset = parseREXP(out body, ref buffer, offset);

            if (offset != eox)
            {
#if WARNING
                throw new RconnectionException("Warning: closure SEXP size mismatch");
#endif
                offset = eox;
            }
            x.Data = body;            
            return offset;
        }

        private static Int32 parseIntArray(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Int32 Count = (eox - offset) / 4;            
            Int32[] values = new Int32[Count];
            
            for (int i = 0; i < Count; i++)
            {
                values[i] = BitConverter.ToInt32(buffer, offset);
                offset += 4;                
            }
                        
            if (offset != eox)
            {
                //throw new RconnectionException("Warning: int array SEXP size mismatch");
                offset = eox;
            }
            x.Data = values;

            if (x.Attrib != null)
            {
                if ((x.Attrib.Xtype == Xpression.XT_LIST) && (x.Attrib.Data != null) &&
                    (((Rlist)x.Attrib.Data).Head != null) &&
                    (((Rlist)x.Attrib.Data).Body != null) &&
                    (((Rlist)x.Attrib.Data).Head.Data != null) &&
                    (((Rlist)x.Attrib.Data).Body.Data != null) &&
                    (((Rlist)x.Attrib.Data).Head.Xtype == Xpression.XT_VECTOR) &&
                    (((Rlist)x.Attrib.Data).Body.Xtype == Xpression.XT_LIST) &&
                    (((Rlist)((Rlist)x.Attrib.Data).Body.Data).Head != null) &&
                    (((Rlist)((Rlist)x.Attrib.Data).Body.Data).Head.Xtype == Xpression.XT_STR) &&
                    ((String)((Rlist)((Rlist)x.Attrib.Data).Body.Data).Head.Data).Equals("factor"))
                {
                    Rfactor f = new Rfactor(values, (List<Rexp>)((Rlist)x.Attrib.Data).Head.Data);
                    x.Data = f;
                    x.Xtype = Xpression.XT_FACTOR;
                    x.Attrib = null;
                }
            }
            
            return offset;
        }

        private static Int32 parseVector(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {            
            List<Rexp> v = new List<Rexp>();
            while (offset < eox)
            {
                Rexp xx;
                offset = parseREXP(out xx, ref buffer, offset);                
                v.Add(xx);
            }

            if (offset != eox)
            {
                //System.out.println("Warning: int vector SEXP size mismatch\n");
		        offset = eox;
            }
            
            
            x.Data = v;
            // fixup for lists since they're stored as attributes of vectors
            if ((x.Attrib != null) && (x.Attrib.Xtype == Xpression.XT_LIST) &&
                (x.Attrib.Data != null))
            {
                Rlist l = new Rlist();
                l.Head = ((Rlist)x.Attrib.Data).Head;
                l.Body = new Rexp(Xpression.XT_VECTOR, v);
                x.Data = l;
                x.Xtype = Xpression.XT_LIST;
                x.Attrib = x.Attrib.Attrib;

                if (l.Head.Xtype == Xpression.XT_STR)
                {
                    List<Rexp> sv = new List<Rexp>();
                    sv.Add(l.Head);
                    l.Head = new Rexp(Xpression.XT_VECTOR, sv, l.Head.Attrib);
                    l.Head.Attrib = null;
                }
            }
             
            return offset;
        }

        private static Int32 parseInt(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            x.Data = BitConverter.ToInt32(buffer, offset);
            offset += 4;

            if (offset != eox)
            {
#if WARNING
                throw new RconnectionException("Warning: int SEXP size mismatch");
#endif
                offset = eox;
            }
            if (offset > buffer.Length) throw new Exception("bladint");
            
            return offset;
        }

        private static Int32 parseDouble(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            x.Data = BitConverter.ToDouble(buffer, offset);
            offset += 8;
            if (offset != eox)
            {
#if WARNING
                throw new RconnectionException("Warning: Double SEXP size mismatch");
#endif
                offset = eox;
            }
            if (offset > buffer.Length) throw new Exception("bladdouble");
            
            return offset;
        }

        private static Int32 parseDoubleArray(Int32 eox, Int32 offset, ref Byte[] buffer, ref Rexp x)
        {
            Int32 count = (eox - offset) / 8;
            Double[] values = new Double[count];

            for (int i = 0; i < count; i++)
            {
                values[i] = BitConverter.ToDouble(buffer, offset);
                offset += 8;                
            }
            
            if (offset != eox)
            {
#if WARNINGEXP
                throw new RconnectionException("Warning: Double array SEXP size mismatch");
#endif
                offset = eox;
            }
            x.Data = values;
            return offset;
        }
    }
}