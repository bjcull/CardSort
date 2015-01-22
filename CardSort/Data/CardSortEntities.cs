using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSort.Data
{
    public class CardSortEntities : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Strategy> Strategies { get; set; }

        public CardSortEntities()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CardSortEntities>());
        }
    }
}
