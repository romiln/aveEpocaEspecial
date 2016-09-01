using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.CustomAttributes
{
    public class NonNull : Attribute,IAttributes
    {
        public NonNull()
        {

        }

        public void Validator(object o)
        {
            if(o.Equals(null))
                throw new InvalidOperationException();
        }
    }
}
