#region Informations
/* RserveLink library - client interface to Rserve
 * Copyright (C) 2006 Krzysztof Miodek
 * for licensing information see LICENSE file in the original RserveLink distribution 
 * Thanks for Simon Urbanek http://rosuda.org/Rserve/ - author Rserve*/
#endregion

#region using Directives
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
#endregion

namespace RserveLink
{
    /// <summary>
    /// Implementation of R-lists<br/>
    /// The point is that the parser tries to interpret lists to be of the form entry=value,
    /// where entry is stored in the "head" part, and value is stored in the "body" part.
    /// Then using <see cref="At(String)"/> it is possible to fetch "body" for a specific "head".
    /// The terminology used is partly from hash fields - "keys" are the elements in "head"
    /// and values are in "body" <see cref="Keys"/>.
    /// On the other hand, R uses lists to store complex internal structures, which are not
    /// parsed according to the structure - in that case "head" and "body" have to be evaluated
    /// separately according to their meaning in that context.
    /// </summary>
    public class Rlist
    {        
        /// <summary>
        /// Xpression containing head (corresponds to CAR)
        /// </summary>
        private Rexp head;

        /// <summary>
        /// Xpression containing body (corresponds to CDR)
        /// </summary>
        private Rexp body;

        /// <summary>
        /// Xpression containing tag
        /// </summary>
        private Rexp tag;
                
        /// <summary>
        /// List from head xpression
        /// </summary>
        Rexp[] h;

        /// <summary>
        /// List from body xpression
        /// </summary>
        Rexp[] b;

        /// <summary>
        /// Contructs an empty list
        /// </summary>
        public Rlist()
        {
            head = body = tag = null;
        }

        /// <summary>
        /// Constructs an initialized list
        /// </summary>
        /// <param name="Head">Head xpression</param>
        /// <param name="Body">Body xpression</param>
        public Rlist(Rexp Head, Rexp Body)
        {
            head = Head;
            body = Body;
            tag  = null;
        }
                
        /// <summary>
        /// Head xpression (CAR)
        /// </summary>
        public Rexp Head 
        {
            get
            {
                return (head);
            }
            set
            {
                head = value;
            }
        }
                
        /// <summary>
        /// Body xpression (CDR)
        /// </summary>
        public Rexp Body
        {
            get
            {
                return (body);
            }
            set
            {
                body = value;
            }
        }
                
        /// <summary>
        /// Tag xpression
        /// </summary>
        public Rexp Tag
        {
            get
            {
                return (tag);
            }
            set
            {
                tag = value;
            }
        }
                 
        /// <summary>
        /// function that updates cached lists
        /// </summary>
        /// <returns>true if both expressions are vectors and of the same length</returns>
        private Boolean updateList()
        {
            if (head == null || body == null || (head.Xtype != Xpression.XT_VECTOR)
                || (body.Xtype != Xpression.XT_VECTOR))
            {
                return false;
            }

            Rexp[] dataHead = (Rexp[])head.Data;
            Rexp[] dataBody = (Rexp[])body.Data;

            if (dataHead.Length != dataBody.Length) return false;

            h = dataHead;
            b = dataBody;
            
            return true;
        }

        /// <summary>
        /// Get element at the specified position
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>xpression at the index or null if list is not standartized or index out of bounds</returns>
        public Rexp At(Int32 i)
        {
            if (!updateList()) return null;
            return (i >= 0 && i < b.Length) ? b[i] : null;
        }
    
        /// <summary>
        /// Get xpression given a key
        /// </summary>
        /// <param name="v">key</param>
        /// <returns>xpression which corresponds to the given key or null 
        /// if list is not standartized or key not found</returns>
        public Rexp At(String v)
        {
            if (!updateList()) return null;
            for (int i = 0; i < h.Length; i++)
            {
                Rexp r = h[i];
                if ((r != null) && (r.Xtype == Xpression.XT_STR) &&
                    (((String)r.Data).Equals(v)))
                {
                    return b[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all keys of the list
        /// </summary>
        /// <returns>array containing all keys or null if list is not standartized</returns>
        public String[] Keys()
        {
            if (!updateList()) return null;
            String[] k = new String[h.Length];
            
            for (int i = 0; i < h.Length; i++)
            {
                if (h[i] == null || h[i].Xtype != Xpression.XT_STR)
                {
                    k[i] = null;
                }
                else
                {
                    if (h[i].Data != null) k[i] = (String)h[i].Data;
                    else k[i] = null;
                }                 
            }
            return k;
        }
    }
}
