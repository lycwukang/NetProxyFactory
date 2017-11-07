using Com.Proxy.Compile;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Com.Proxy
{
    public class ProxyFactory
    {
        private static ConcurrentDictionary<string, Type> compileClassCache = new ConcurrentDictionary<string, Type>();
        private static int count = 0;

        public static T newProxy<T>(object obj, Type interfaceType, Type proxyType) where T : class
        {
            var type = default(Type);
            if (compileClassCache.ContainsKey(interfaceType.FullName)) type = compileClassCache[interfaceType.FullName];
            else
            {
                var number = Interlocked.Increment(ref count);
                var provider = new ClassDomProvider($@"{interfaceType.Namespace}.Proxy", $@"Proxy{number}", false);
                provider.Interfaces.Add(interfaceType);
                provider.Assemblies.Add("System.dll");
                provider.Assemblies.Add(interfaceType.Assembly.ManifestModule.Name);
                foreach (var typ2 in interfaceType.GetInterfaces())
                    provider.Assemblies.Add(typ2.Assembly.ManifestModule.Name);
                provider.Assemblies.Add(proxyType.Assembly.ManifestModule.Name);
                foreach (var typ2 in proxyType.GetInterfaces())
                    provider.Assemblies.Add(typ2.Assembly.ManifestModule.Name);
                provider.AddField("proxy", proxyType, ClassDomProvider.VisitType.Private, null);
                provider.AddField("obj", interfaceType, ClassDomProvider.VisitType.Private, null);
                provider.AddConstructor(ClassDomProvider.VisitType.Public, new Type[] { proxyType, interfaceType }, "this.proxy=arg0;this.obj=arg1;");
                foreach (var method in interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
                    var methodParams = new StringBuilder();
                    for (var i = 0; i < paramTypes.Length; i++) methodParams.Append(",arg" + i);
                    provider.AddMethod(method.Name, method.ReturnType, ClassDomProvider.VisitType.Public, paramTypes, $@"proxy.method(obj, ""{method.Name}""{methodParams.ToString()});");
                }
                type = ClassDomFactory.Compile(provider);
                compileClassCache.TryAdd(interfaceType.FullName, type);
            }
            var proxy = proxyType.Assembly.CreateInstance(proxyType.FullName);
            return (T)type.Assembly.CreateInstance(type.FullName, true, BindingFlags.Default, null, new object[] { proxy, obj }, null, null);
        }
    }
}