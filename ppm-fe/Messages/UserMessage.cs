using ppm_fe.Models;

namespace ppm_fe.Messages
{
    public class UserMessage
    {
        public User? User { get; }

        public UserMessage(User? user)
        {
            User = user;
        }
    }
}
