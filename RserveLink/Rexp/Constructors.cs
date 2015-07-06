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
    /// <example>
    /// <code>
    /// using System;
    /// using System.Text;
    /// using RserveLink; 
    /// 
    /// namespace Rexample
    /// {
    ///    class Program
    ///    {
    ///        static void Main(string[] args)
    ///        {
    ///             Double[] values = new Double[10];
    ///             for (int i = 0; i <![CDATA[<]]> values.Length; i++)
    ///             {
    ///                 values[i] = i;
    ///             }
    ///             try
    ///             {
    ///                 /* make connection to Rserve */
    ///                 Rconnection rc = new Rconnection();
    ///                 rc.Connect();
    ///                 
    ///                 /* copy values to Rserve */
    ///                 Rexp rdata = new Rexp(values);
    ///                 rc.Assign("val", rdata);
    ///                 
    ///                 /* compute mean of values */
    ///                 Rexp response = rc.Eval("mean(val)");
    ///                 Double mean = response.AsDouble();
    ///                 rc.Disconnect();
    ///                 
    ///                 /* show result */
    ///                 Console.WriteLine("Values: ");
    ///                 foreach (Double current in values)
    ///                 {
    ///                     Console.Write(current.ToString() + " ");
    ///                 }
    ///                 Console.WriteLine("\nMean: " + mean.ToString());
    ///             }
    ///             catch (RserveLink.RconnectionException myException)
    ///             {
    ///                 Console.WriteLine(myException.Message);
    ///             }
    ///        }
    ///    }
    /// }
    /// </code>
    /// </example>   
    public partial class Rexp
    {
        /// <summary>
        /// content of the xpression 
        /// </summary>
        private Object data;

        /// <summary>
        /// Content of the xpression
        /// </summary>
        public Object Data
        {
            get
            {
                return (data);
            }
            internal set
            {
                data = value;
            }
        }
        
        /// <summary>
        /// xpression type
        /// </summary>
        private Xpression type;

        /// <summary>
        /// Xpression type
        /// </summary>
        public Xpression Xtype
        {
            get
            {
                return (type);
            }
            internal set 
            {
                type = value;
            }
        }

        /// <summary>
        /// attribute xpression or <code>null</code> if none 
        /// </summary>
        private Rexp rattribute;

        /// <summary>
        /// Attribute xpression or <code>null</code> if none
        /// </summary>
        public Rexp Attrib
        {
            get
            {
                return (rattribute);
            }
            internal set
            {
                rattribute = value;
            }
        }
                 
        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="XT">Xpression type of object</param>
        /// <param name="Xdata">Object to store</param>
        /// <param name="Att">Attribute of object</param>
        internal Rexp(Xpression XT, Object Xdata, Rexp Att)
        {
            type       = XT;
            data       = Xdata;
            rattribute = Att;
        }

        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="XT">Xpression type of object</param>
        /// <param name="Xdata">Object to store</param>
        internal Rexp(Xpression XT, Object Xdata)
        {
            type = XT;
            data = Xdata;
        }
        
        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="input">Double value</param>
        public Rexp(Double input)
        {            
            type = Xpression.XT_DOUBLE;  //set type to double
            data = input;                //copy value           
        }

        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="input">Int32 value</param>
        public Rexp(Int32 input)
        {
            type = Xpression.XT_INT;    //set type to XT_INT
            data = input;               //copy value
        }
        
        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="input">Array of Doubles values</param>
        public Rexp(Double[] input)
        {
            type = Xpression.XT_ARRAY_DOUBLE;   //set type to XT_ARRAY_DOUBLE
            data = input;                       //copy values
        }

        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="input">Array of Int32 values</param>
        public Rexp(Int32[] input)
        {
            type = Xpression.XT_ARRAY_INT;      //set type to XT_ARRAY_INT
            data = input;                       //copy values
        }

        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve 
        /// </summary>
        internal Rexp()
        {
        }
                
        /// <summary>
        /// Constructor of Rexp object which we can send to Rserve
        /// </summary>
        /// <param name="input">Array of String values</param>
        public Rexp(String[] input)
        {
            type = Xpression.XT_ARRAY_STR;      //set type to XT_ARRAY_STR
            data = input;                       //copy values
        }

    }
}