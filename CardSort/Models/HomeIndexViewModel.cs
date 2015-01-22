using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardSort.Data;

namespace CardSort.Models
{
    public class HomeIndexViewModel
    {
        public IList<ParticipantItemViewModel> Participants { get; set; }

        public HomeIndexViewModel()
        {
            Participants = new List<ParticipantItemViewModel>();
        }
    }
}
