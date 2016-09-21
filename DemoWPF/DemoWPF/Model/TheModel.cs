using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoWPF.Model
{
    class TheModel
    {
        Func<string, string> _converstion;

        public TheModel(Func<string, string> conversion)
        {
            _converstion = conversion;
        }

        public string ConvertText(string inputText)
        {
            return _converstion(inputText);
        }
    }
}
