namespace TestAPI.Models.Word
{
    public class WordDefinition
    {
        public string Word { get; set; }
        public List<Meanings> Meanings { get; set; }
    }

    public class Meanings
    {
        public string PartOfSpeech { get; set; }
        public List<Definitions> Definitions { get; set; }
    }

    public class Definitions
    {
        public string Definition { get; set; }
    }
}