using System;
using System.Collections.Generic;
using System.Text;

namespace ATP.SocketSearch
{
    class SocketSearchInfo
    {
        internal bool IsCurStartWith(string curBalise, string startWord)
        {
            return curBalise.StartsWith(startWord);
        }

        internal bool Is0_5Maendlink(string curBalise, string Maendlink)
        {
            if (IsCurStartWith(curBalise, "Z"))
            {
                return curBalise.Substring(0, 4) == Maendlink;
            }
            return curBalise.Substring(0,5)== Maendlink;
        }

        internal bool IsCurBaliseEmpty(string curBalise)
        {
            return string.IsNullOrEmpty(curBalise);
        }


    }
}
