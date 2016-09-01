using EpocaEspecial.CustomAttributes;
using EpocaEspecial.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpocaEspecial.Model
{
    public class Stock
    {
        public Stock(string name, string index){

        }
        [NonNull]
        public virtual string Market { get; set; } // set dará excepção para valores null
        //[Max(75)]
        //[Min(73)]
        public virtual long Quote { get; set; } // set dará excepção para valores < 73
       [Min(0.325)]
        public virtual double Rate { get; set; } // set dará excepção para valores < 0,325
        [Accept("Jonas","Mitro","Cenas")]
        public virtual string Trader { get; set; } // set só aceita valores Jenny, Lily e Valery
        [Max(100)]
        [Min(2)]
        public virtual int Price { get; set; } // set dará excepção para valores < 58
                                               // dará excepção se o estado de this ou algum dos parâmetros tiver sido alterado
                                               // pela execução do método anotado -- BuildInterest
        [NoEffects]
        [Min(40)]
        public virtual double BuildInterest(Portfolio port, Store st) {
            return -1;
        }
    }
}
