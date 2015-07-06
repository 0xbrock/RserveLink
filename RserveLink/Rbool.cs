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
    /// Implementation of tri-state logical data type in R.The three 
    /// states are True, False and Not available. To obtain truly boolean
    /// value, you'll need to use <see cref="isTRUE"/> or <see cref="isFALSE"/> 
    /// since there is no canonical representation of RBool in boolean.
    /// </summary>
    public class Rbool
    {
        /// <summary>
        /// Actual value
        /// 1 - true
        /// 0 - false
        /// 2 - na
        /// </summary>
        private Int32 value;

        /// <summary>
        /// Actual value
        /// 1 - true
        /// 0 - false
        /// 2 - na
        /// </summary>        
        public Int32 Value
        {
            get
            {
                return (value);
            }
        }
        
        /// <summary>
        /// Constructor of Rbool object
        /// </summary>
        /// <param name="input"></param>
        public Rbool(RBoolValues input)
        {
            value = (int)input;
        }
        
        /// <summary>
        /// Constructor of Rbool object
        /// </summary>
        /// <param name="input">Boolean value</param>
        public Rbool(Boolean input)
        {
            if (input) value = 1;
            else value = 0;
        }

        /// <summary>
        /// Return <c>true</c> if Rbool value is "Not available" else return <c>false</c>.
        /// </summary>
        /// <returns>Boolean value</returns>
        public Boolean isNA()
        {
            return (value == (int)RBoolValues.NA);
        }

        /// <summary>
        /// Return <c>true</c> if Rbool value is "true" else return <c>false</c>.
        /// </summary>
        /// <returns>Boolean value</returns>
        public Boolean isTRUE()
        {
            return (value == (int)RBoolValues.TRUE);
        }

        /// <summary>
        /// Return <c>true</c> if Rbool value is "false" else return <c>false</c>.
        /// </summary>
        /// <returns></returns>
        public Boolean isFALSE()
        {
            return (value == (int)RBoolValues.FALSE);
        }

        /// <summary>
        /// Converts the Rbool value of this instance to string representation
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            return ((RBoolValues)value).ToString();
        }                
    }
}
