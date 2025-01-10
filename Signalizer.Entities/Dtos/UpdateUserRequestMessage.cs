using Signalizer.Entities.Models;

namespace Signalizer.Entities
{
    public class UpdateUserRequestMessage
    {
        public UserModel UserModel { get; set; }
    }
    public class UpdateUserResponseMessage: BaseResponse
    {
    }
}