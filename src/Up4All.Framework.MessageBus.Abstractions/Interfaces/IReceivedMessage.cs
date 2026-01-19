using System.Text.Json;

namespace Up4All.Framework.MessageBus.Abstractions.Interfaces
{
    public interface IReceivedMessage : IMessage
    {
        string GetBody();
        T GetBody<T>(JsonSerializerOptions opts = null);
        object GetUserPropertyValue(string key);
        string GetUserPropertyValueAsString(string key, string defaultValue = null);
    }
}
