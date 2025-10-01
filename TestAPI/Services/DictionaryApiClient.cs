using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeProject;
using TestAPI.Models.Word;

namespace TestAPI.Services
{
    public class DictionaryApiClient
    {
        private readonly HttpClient client;
        public DictionaryApiClient(HttpClient client)
        {
            this.client = client;
        }

        public async Task<List<WordDefinition>> GetWordDefinitionAsync(string word)
        {
            var request = HttpClientHelper.CreateRequestMessage(HttpMethod.Get, $"{word}");
            return await HttpClientHelper.SendRequestAsync<List<WordDefinition>>(client, request);
        }
    }
}