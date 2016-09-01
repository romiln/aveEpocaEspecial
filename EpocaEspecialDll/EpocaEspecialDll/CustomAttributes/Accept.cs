using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.CustomAttributes
{
    public class Accept : Attribute, IAttributes
    {
        object[] parameter;
        public Accept(params object [] args)
        {
            this.parameter = args;
        }
        public void Validator(object o)
        {
            if (o.Equals(null))
            {
                throw new Exception("...");
            }
            foreach(string s in parameter)
            {
                if (s.Equals(o))
                    return;
            }
            throw new Exception("...");
        }
    }
}
