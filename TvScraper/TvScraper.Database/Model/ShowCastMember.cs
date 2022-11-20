using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvScraper.Database.Model
{
    public class CastMember
    {
        public int Id { get; set; }
        public int ShowId { get; set; }
        public Show Show { get; set; }

        public int ActorId { get; set; }
        public Actor Actor { get; set; }  
    }
}
