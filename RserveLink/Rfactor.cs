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
    /// Representation of a factor variable. In R there is no actual xpression
    /// type called "factor", instead it is coded as an int vector with a list
    /// attribute. The parser code of REXP converts such constructs directly into
    /// the RFactor objects and defines an own XT_FACTOR type. 
    /// </summary>
    public class Rfactor
    {
        /// <summary>
        /// IDs each entry corresponds to a case, ID specifies the category
        /// </summary>
        private List<Int32> id;
        
        /// <summary>
        /// values, ergo category names
        /// </summary>
        private List<String> val;
        
        /// <summary>
        /// Create a new, empty factor variable
        /// </summary>
        public Rfactor() 
        {
            id  = new List<Int32>();
            val = new List<String>(); 
        }
    
        /// <summary>
        /// Create a new factor variable, base on the supplied arrays
        /// </summary>
        /// <param name="i">array if IDs (0 ... v.length-1)</param>
        /// <param name="v">values - category names</param>
        public Rfactor(Int32[] i, String[] v) 
        {
            id  = new List<Int32>();
            val = new List<String>();

            if ((i != null) && (i.Length > 0))
            {
                foreach (Int32 current in i) id.Add(current);
            }

            if ((v != null) && (v.Length > 0))
            {
                foreach (String current in v) val.Add(current);
            }
        }

        /// <summary>
        /// Special constructor used by Rexp parser to save some re-indexing
        /// and performing automatic index conversion
        /// </summary>
        /// <param name="i">index array</param>
        /// <param name="v">vector of xpressions which should be all strings</param>
        public Rfactor(int[] i, List<Rexp> v)
        {
            id  = new List<Int32>();
            val = new List<String>();

            if ((i != null) && (i.Length > 0))
            {
                foreach (Int32 current in i) id.Add(current - 1);                
            }

            if (v != null && v.Count > 0)
            {
                foreach (Rexp current in v)
                {
                    val.Add((current == null || current.Xtype != Xpression.XT_STR) ? null : (String)current.Data);
                }               
            }
        }
        
        /// <summary>
        /// Add a new element (by name)
        /// </summary>
        /// <param name="v">value</param>
        public void Add(String v)
        {
            Int32 index = val.IndexOf(v);
            if (index < 0)
            {
                index = val.Count;
                val.Add(v);
            }
            id.Add(index);            
        }

        /// <summary>
        /// The number of caes.
        /// </summary>
        public Int32 Count
        {
            get
            {
                return (id.Count);
            }
        }

        /// <summary>
        /// Returns name for a specific ID 
	    /// </summary>
        /// <param name="i">ID</param>
        /// <returns>Name</returns>
        public String At(Int32 i)
        {
            if ((i < 0) || (i >= id.Count)) return null;
            return ((String)val[id[i]]);            
        }

        /// <summary>
        /// Converts the Rfactor value of this instance to string representation
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("{levels=(");

            if (val == null) sb.Append("null");
            else
            {
                for (int i = 0; i < val.Count; i++)
                {
                    sb.Append((i > 0) ? ",\"" : "\"");
                    sb.Append(val[i]);
                    sb.Append("\"");
                }
            }

            sb.Append("),ids=(");
            if (id == null) sb.Append("null");
            else
            {
                for (int i = 0; i < id.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(id[i]);
                }
            }
            sb.Append(")}");
            return (sb.ToString());
        }
    }
}
