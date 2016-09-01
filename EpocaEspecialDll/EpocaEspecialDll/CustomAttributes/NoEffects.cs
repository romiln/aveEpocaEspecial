using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.CustomAttributes
{
    public class NoEffects : Attribute,IAttributes
    {
        public void Validator(object o)
        {
            throw new NotImplementedException();
        }
    }
}
