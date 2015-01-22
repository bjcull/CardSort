using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSort.Data
{
    public class Strategy
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public ICollection<Category> Categories { get; set; }

        public Strategy()
        {
            Categories = new List<Category>();
        }
    }
}
