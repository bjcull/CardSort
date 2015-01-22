using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSort.Data
{
    public class Participant
    {
        public int Id { get; set; }
        public int UniqueId { get; set; }
        public ICollection<Category> Categories { get; set; }

        public Participant()
        {
            Categories = new List<Category>();
        }
    }
}
