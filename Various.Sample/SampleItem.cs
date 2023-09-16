using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Various.Sample
{
    public class SampleItem
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public SampleItem(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
