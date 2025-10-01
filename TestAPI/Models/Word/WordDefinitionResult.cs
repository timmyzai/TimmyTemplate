using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAPI.Models.Word
{
    public class WordDefinitionResult
    {
        public string Word { get; set; }
        public bool IsCorrect { get; set; }
        public List<WordDefinition> WordDefinition { get; set; }
    }
}