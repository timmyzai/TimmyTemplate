using AwesomeProject;

namespace TestAPI.Models.Dtos
{
    public class CoinsDto : EntityDto<string> {

        public string Name { get; set; }
        public string Symbol { get; set; }

    }

}