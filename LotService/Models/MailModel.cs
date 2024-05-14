namespace LotService.Models
{

    public class MailModel
    {
        private string? receiverMail;
        private string? header;
        private string? content;

        public string? ReceiverMail { get => receiverMail; set => receiverMail = value; }

        public string? Header { get => header; set => header = value; }

        public string? Content { get => content; set => content = value; }
    }
}
