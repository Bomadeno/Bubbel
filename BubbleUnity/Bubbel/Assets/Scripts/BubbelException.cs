using System;

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
