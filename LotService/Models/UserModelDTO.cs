namespace LotService.Models
{
    public class UserModelDTO
    {
        private string? id;
        private string? firstName;
        private string? lastName;
        private string? email;
        private string? userName;
        private string? address;
        private string? phoneNumber;
        private UserType type = UserType.User;
        public enum UserType
        {
            User, Admin
        }

        public string? Id { get => id; set => id = value; }
        public string? FirstName { get => firstName; set => firstName = value; }
        public string? LastName { get => lastName; set => lastName = value; }
        public string? Email { get => email; set => email = value; }
        public string? UserName { get => userName; set => userName = value; }
        public string? Address { get => address; set => address = value; }
        public string? PhoneNumber { get => phoneNumber; set => phoneNumber = value; }
        public UserType Type { get => type; set => type = value; }
    }
}
