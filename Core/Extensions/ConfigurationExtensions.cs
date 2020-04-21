using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Cww.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IEnumerable<string> Users(this IConfiguration configuration)
        {
            return configuration.GetSection("Users").Get<string[]>().AsEnumerable();
        }
    }
}
