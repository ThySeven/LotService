using System.Net.Mail;
using System.Text.RegularExpressions;

namespace LotService.Models
{
    public class Notification
    {
        private string lotId;
        private string receiverMail;
        private DateTime timeStamp;
        private string lotName;
        private int newLotPrice;
        private string newBidLink;

        public string LotId
        {
            get => lotId;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Lot name cannot be null or empty.");
                lotId = value;
            }
        }

        public string ReceiverMail
        {
            get => receiverMail;
            set
            {
                try
                {
                    var mailAddress = new MailAddress(value);
                    receiverMail = value;
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid email address.");
                }
            }
        }

        public DateTime TimeStamp
        {
            get => timeStamp;
            set
            {
                if (value > DateTime.Now)
                    throw new ArgumentException("Timestamp cannot be a future date.");
                timeStamp = value;
            }
        }

        public string LotName
        {
            get => lotName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Lot name cannot be null or empty.");
                lotName = value;
            }
        }

        public int NewLotPrice
        {
            get => newLotPrice;
            set
            {
                if (value < 0)
                    throw new ArgumentException("New lot price must be a valid decimal number.");
                newLotPrice = value;
            }
        }

        public string NewBidLink
        {
            get => newBidLink;
            set
            {
                string pattern = @"^(http|https)://";
                if (!Regex.IsMatch(value, pattern))
                    throw new ArgumentException("New bid link must be a valid URL.");
                newBidLink = value;
            }
        }
    }
}
