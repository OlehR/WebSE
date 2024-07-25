using Supplyer.Models.Enums;
using System.Text.Json.Serialization;

namespace Supplyer.ViewModel
{
    public class ChangeDiscountRequestStatus
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonRequired]
        public RequestStatus status {  get; set; }
        [JsonRequired]
        public string number { get; set; }
        [JsonRequired]
        public string comment { get; set; }
        [JsonRequired]
        public string codewares { get; set; }
    }
}
