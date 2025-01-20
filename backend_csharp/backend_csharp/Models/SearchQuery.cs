namespace backend_csharp.Models
{
    public class SearchQuery
    {
        public string SearchValue {get; set;} = "";

        public string Query {get; set;} = "title;album;artist;lyrics";

        public int HitCount {get; set;} = 10;
    }
}
