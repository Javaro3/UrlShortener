namespace Domains.Domains {
    public class ShortUrl {
        public int Id { get; set; }
        public string Hash { get; set; }
        public string Url { get; set; }
        public DateTime CreatingDate { get; set; }
        public int TransitionsCount { get; set; }
    }
}