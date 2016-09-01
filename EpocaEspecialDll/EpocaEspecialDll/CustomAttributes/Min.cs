using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.CustomAttributes
{
    public class Min : Attribute,IAttributes
    {
        object a;
        public Min (object v)
        {
            this.a = v;
        }

        public int CompareTo(object obj)
        {
            IComparable x = (IComparable)obj;
            if (x.CompareTo(a) < 0) throw new InvalidProgramException();
            return -1;
        }

        public void Validator(object o)
        {
            CompareTo(o);
        }


    }
}
