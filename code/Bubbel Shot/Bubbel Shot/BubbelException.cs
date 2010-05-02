using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bubbel_Shot
{
    class BubbelException : ApplicationException
    {
        public BubbelException(string exception)
            : base (exception)
        {
            
        }
    }
}
