using System;

namespace Cww.Service.Cache
{
    public interface ICacheTimeoutProvider
    {
        TimeSpan VeryShort();
        TimeSpan Short();
        TimeSpan Medium();
        TimeSpan Long();
        TimeSpan Forever();
    }
}