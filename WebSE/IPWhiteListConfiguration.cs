
using System.Collections.Generic;

namespace WebSE
{
    public interface IIPWhitelistConfiguration
    {
        IEnumerable<string> AuthorizedIPAddresses { get; }
    }

    public class IPWhitelistConfiguration : IIPWhitelistConfiguration
    {
        public IEnumerable<string> AuthorizedIPAddresses { get; set; }
    }
}
