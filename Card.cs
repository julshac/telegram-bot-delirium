using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pj2
{
    class Card
    {
        public int DefaultCount { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Card(string name, int defCount, string desc)
        {
            Name = name;
            DefaultCount = defCount;
            Description = desc;
        }
    }
}