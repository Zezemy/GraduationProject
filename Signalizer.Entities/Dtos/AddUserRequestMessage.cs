using Signalizer.Entities.Dtos;
using Signalizer.Entities.Models;

namespace Signalizer.Entities
{
    public class AddUserRequestMessage
    {
        public UserModel UserModel { get; set; }
    }
    public class AddUserResponseMessage: BaseResponse
    {
    }
}