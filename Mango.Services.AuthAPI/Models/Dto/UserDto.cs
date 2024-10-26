namespace Mango.Services.AuthAPI.Models.Dto
{
    public class UserDto
    {
        // user id will be a grid not integer so it will be string
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}
