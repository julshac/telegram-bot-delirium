using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pj2
{
    class Card
    {
        public int Count { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Card(string name, int count, string desc)
        {
            Name = name;
            Count = count;
            Description = desc;
        }
    }
}