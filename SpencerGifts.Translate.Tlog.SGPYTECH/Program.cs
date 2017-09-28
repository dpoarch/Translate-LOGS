using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpencerGifts.Translate.Tlog.SGPYTECH
{
    class Program
    {
        static void Main(string[] args)
        {
            SGPYTECH sgpytech = new SGPYTECH();
            SGPYTECHCAN sgpytechcan = new SGPYTECHCAN();
            SGGCINV0 sgg = new SGGCINV0();
            SGTLOGFORMAT sgtlog = new SGTLOGFORMAT();

            sgpytech.begin_report();
            sgpytechcan.begin_report();
            sgg.begin_report();
            sgtlog.begin_report();
        }
    }
}
