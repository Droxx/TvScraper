using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TvScraper.Database.Model
{
    public class Actor
    {
        public int Id { get; set; }
        public int TvMazeId { get; set; }
        public string Name { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public ICollection<CastMember> Shows { get; set; }
    }
}
