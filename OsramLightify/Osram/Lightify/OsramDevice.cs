using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osram.Lightify
{
    public class OsramDevice
    {
        public string Name { get; set; }
        public bool State { get; set; }
        public int Level { get; set; }
        public int ColorTemp { get; set; }
    }
}
