namespace Com.Proxy
{
    public interface IProxyHandler
    {
        object method(object obj, string name, params object[] parameters);
    }
}