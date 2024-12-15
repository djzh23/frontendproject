using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ppm_fe.Helpers
{
    class MessageHelper : ValueChangedMessage<string>
    {
        public MessageHelper(string fieldName) : base(fieldName) { }
    }
}

