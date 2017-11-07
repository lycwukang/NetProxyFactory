using Com.Proxy;
using System;
using System.Reflection;

namespace project20171030
{
    class Program
    {
        static void Main(string[] args)
        {
            IPerson person = ProxyFactory.newProxy<IPerson>(new Person(), typeof(IPerson), typeof(PersonProxy));
            person.Run();
            person.Eat("shit");
            Console.Read();
        }
    }

    public interface IPerson
    {
        void Run();

        void Eat(string food);
    }

    public class Person : IPerson
    {
        public void Run()
        {
            Console.WriteLine("runing");
        }

        public void Eat(string food)
        {
            Console.WriteLine("eat " + food);
        }
    }

    public class PersonProxy : IProxyHandler
    {
        public object method(object obj, string name, params object[] parameters)
        {
            Console.WriteLine("method " + name + " begin");
            var result = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance).Invoke(obj, parameters);
            Console.WriteLine("method " + name + " end");
            return result;
        }
    }
}