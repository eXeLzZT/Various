using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Various.Sample
{
    public struct Item
    {
        public static Item All { get; }
        public static Item Own { get; }

        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Timestamp { get; set; }

        static Item()
        {
            All = new Item(string.Empty, "All");
            Own = new Item("NONE", "Own");
        }

        public Item(string id, string name)
        {
            Id = id;
            Name = name;
            Timestamp = DateTime.Now;
        }
    }
}
