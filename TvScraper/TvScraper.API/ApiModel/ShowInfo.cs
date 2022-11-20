namespace TvScraper.API.ApiModel
{
    public class ShowInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<CastInfo> Cast { get; set; }
    }
}
