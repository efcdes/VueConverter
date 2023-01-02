using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VueConverter
{
    public class FunctionInfo
    {
        public string Name { get; set; }
        public bool IsAsync { get; set; }
        public string ReturnType { get; set; }
        public List<string> Body { get; set; }
        public List<string> Parameters { get; set; }
    }
}
