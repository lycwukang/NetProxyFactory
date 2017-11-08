using Com.Proxy.Compile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Com.Proxy
{
    public class ProxyFactory
    {
        private static Dictionary<string, Type> compileClassCache = new Dictionary<string, Type>();
        private static Dictionary<string, object> classLocker = new Dictionary<string, object>();
        private static int count = 0;

        public static T NewProxy<T>(object obj, Type interfaceType, Type proxyType) where T : class
        {
            var type = default(Type);
            if (compileClassCache.ContainsKey(interfaceType.FullName))
                type = compileClassCache[interfaceType.FullName];
            else
            {
                lock (GetProxyLocker(interfaceType.FullName))
                {
                    if (compileClassCache.ContainsKey(interfaceType.FullName))
                        type = compileClassCache[interfaceType.FullName];
                    else
                        type = CreateProxyType(obj, interfaceType, proxyType);
                }
            }
            var proxy = proxyType.Assembly.CreateInstance(proxyType.FullName);
            return (T)type.Assembly.CreateInstance(type.FullName, true, BindingFlags.Default, null, new object[] { proxy, obj }, null, null);
        }

        private static object GetProxyLocker(string key)
        {
            object locker;
            lock (compileClassCache)
            {
                if (classLocker.ContainsKey(key)) locker = classLocker[key];
                else
                {
                    locker = new object();
                    classLocker.Add(key, locker);
                }
            }
            return locker;
        }

        private static Type CreateProxyType(object obj, Type interfaceType, Type proxyType)
        {
            var number = count++;
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
            Type type = ClassDomFactory.Compile(provider);
            compileClassCache.Add(interfaceType.FullName, type);
            return type;
        }
    }
}