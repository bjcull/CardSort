using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSort.Data
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Strategy> Strategies { get; set; }

        public Category()
        {
            Strategies = new List<Strategy>();
        }
    }
}
