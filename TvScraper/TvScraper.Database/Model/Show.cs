using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvScraper.Database.Model
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<CastMember> Cast { get; set; }   
    }
}
