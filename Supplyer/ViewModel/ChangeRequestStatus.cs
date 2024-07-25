using Supplyer.Models.Enums;
using System.Text.Json.Serialization;

namespace Supplyer.ViewModel
{
    public class ChangeRequestStatus
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonRequired]
        public RequestStatus status { get; set; }
        [JsonRequired]
        public string CommentSpec { get; set; }
        [JsonRequired]
        public string CodeWares { get; set; }
        [JsonRequired]
        public string CodeFirm { get; set; }

    }
}
