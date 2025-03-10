namespace UserAPI.Models.Dtos
{
    public class SendEmailDTO
    {
        public string ReceiverEmail { get; set; }
        public string ReceiverName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
