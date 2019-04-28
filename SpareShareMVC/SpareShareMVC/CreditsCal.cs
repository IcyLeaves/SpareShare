using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SpareShareMVC
{
    public class CreditsCal
    {
        static public int getLevel(int exp)
        {
            if (0 <= exp && exp < 5)
            {
                return 1;
            }
            if (5 <= exp && exp < 20)
            {
                return 2;
            }
            if (20 <= exp && exp < 50)
            {
                return 3;
            }
            if (0 <= exp && exp < 100)
            {
                return 4;
            }
            if (0 <= exp && exp < 200)
            {
                return 5;
            }
            return 6;
        }
        static public int ExpInProgress(int exp)
        {
            if (0 <= exp && exp < 5)
            {
                return 5-exp;
            }
            if (5 <= exp && exp < 20)
            {
                return 20-exp;
            }
            if (20 <= exp && exp < 50)
            {
                return 50-exp;
            }
            if (0 <= exp && exp < 100)
            {
                return 100-exp;
            }
            if (0 <= exp && exp < 200)
            {
                return 200-exp;
            }
            return -1;
        }
    }
}