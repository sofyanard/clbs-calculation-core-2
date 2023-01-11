using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace calc2
{
    internal class Helper
    {
        public EnumKolektibilitas DpdToCollectability(int dpd) 
        {
            if (dpd <= 10)
            {
                return EnumKolektibilitas.Lancar;
            }
            else if ((dpd > 10) && (dpd <= 90))
            {
                return EnumKolektibilitas.DalamPerhatianKhusus;
            }
            else if ((dpd > 90) && (dpd <= 120))
            {
                return EnumKolektibilitas.KurangLancar;
            }
            else if ((dpd > 120) && (dpd <= 180))
            {
                return EnumKolektibilitas.Diragukan;
            }
            else
            {
                return EnumKolektibilitas.Macet;
            }
        }
    }
}
