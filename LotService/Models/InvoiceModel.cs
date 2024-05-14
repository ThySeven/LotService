using System.Text.RegularExpressions;

namespace LotService.Models
{
    public class InvoiceModel
    {
        private string id = Guid.NewGuid().ToString();
        private bool paidStatus = false;
        private double price;
        private string description;
        private string address;
        private string email;
        private DateTime createdAt = DateTime.Now;

        public string Id
        {
            get => id;
            set
            {
                if (!Guid.TryParse(value, out _))
                {
                    throw new ArgumentException("Invalid GUID format.");
                }
                id = value;
            }
        }

        public bool PaidStatus
        {
            get => paidStatus;
            set => paidStatus = value;
        }

        public double Price
        {
            get => price;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Price cannot be negative.");
                }
                price = value;
            }
        }

        public string Description
        {
            get => description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Description cannot be empty or whitespace.");
                }
                description = value;
            }
        }

        public string Address
        {
            get => address;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Address cannot be empty or whitespace.");
                }
                address = value;
            }
        }

        public string Email
        {
            get => email;
            set
            {
                if (!Regex.IsMatch(value, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                {
                    throw new ArgumentException("Invalid email format.");
                }
                email = value;
            }
        }

        public DateTime CreatedAt
        {
            get => createdAt;
        }
    }
}
