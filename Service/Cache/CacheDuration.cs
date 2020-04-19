namespace Cww.Service.Cache
{
    public enum CacheDuration
    {
        VeryShort = 10,
        Short = 300,
        Medium = 1800,
        Long = 86400,
        Forever = int.MaxValue
    }
}