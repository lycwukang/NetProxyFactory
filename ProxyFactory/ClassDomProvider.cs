using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Proxy.Compile
{
    public class ClassDomProvider
    {
        public string Class { get; set; }
        public string Namespance { get; set; }
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Namespance)) return null;
                if (string.IsNullOrEmpty(Class)) return null;
                return Namespance + "." + Class;
            }
        }
        public bool DefaultConstructor { get; set; }
        public Type Extend { get; set; }
        public IList<Type> Interfaces { get; set; } = new List<Type>();
        public IList<string> Constructors { get; set; } = new List<string>();
        public IList<string> Fields { get; set; } = new List<string>();
        public IList<string> Propertys { get; set; } = new List<string>();
        public IList<string> Methods { get; set; } = new List<string>();
        public IList<string> Assemblies { get; set; } = new List<string>();

        public ClassDomProvider(string namespece, string @class, bool defaultConstructor = false)
        {
            this.Namespance = namespece;
            this.Class = @class;
            this.DefaultConstructor = defaultConstructor;
        }

        public void AddConstructor(VisitType visitType, Type[] paramTypes, string body)
        {
            var builder = new StringBuilder();
            builder.Append(GetVisitCode(visitType)).Append(" $name");
            builder.Append("(");
            if (paramTypes != null)
            {
                for (var i = 0; i < paramTypes.Length; i++)
                {
                    var paramType = paramTypes[i];
                    if (i > 0)
                        builder.Append(",");
                    builder.Append(paramType.FullName).Append(" ").Append("arg" + i);
                }
            }
            builder.Append(")");
            builder.Append("{").Append(body).Append("}");
            this.Constructors.Add(builder.ToString());
        }

        public void AddField(string fieldName, Type fieldType, VisitType visitType, string defaultValue, params AdditionType[] additionTypes)
        {
            var builder = new StringBuilder();
            builder.Append(GetVisitCode(visitType));
            foreach (var additionType in additionTypes)
            {
                builder.Append(" ").Append(GetAdditionType(additionType));
            }
            builder.Append(" ").Append(fieldType.FullName).Append(" ").Append(fieldName);
            if (defaultValue != null)
                builder.Append("=").Append(defaultValue);
            builder.Append(";");
            this.Fields.Add(builder.ToString());
        }

        public void AddProperty(string propertyName, Type propertyType, VisitType visitType, string defaultValue, params AdditionType[] additionTypes)
        {
            var builder = new StringBuilder();
            builder.Append(GetVisitCode(visitType));
            foreach (var additionType in additionTypes)
            {
                builder.Append(" ").Append(GetAdditionType(additionType));
            }
            builder.Append(" ").Append(propertyType.FullName).Append(" ").Append(propertyName);
            builder.Append("{get;set;}");
            if (defaultValue != null)
                builder.Append("=").Append(defaultValue);
            this.Propertys.Add(builder.ToString());
        }

        public void AddMethod(string methodName, Type returnType, VisitType visitType, Type[] paramTypes, string body, params AdditionType[] additionTypes)
        {
            var builder = new StringBuilder();
            builder.Append(GetVisitCode(visitType));
            foreach (var additionType in additionTypes)
            {
                builder.Append(" ").Append(GetAdditionType(additionType));
            }
            builder.Append(" ").Append(returnType.Equals(typeof(void)) ? "void" : returnType.FullName).Append(" ").Append(methodName);
            builder.Append("(");
            if (paramTypes != null)
            {
                for (var i = 0; i < paramTypes.Length; i++)
                {
                    var paramType = paramTypes[i];
                    if (i > 0)
                        builder.Append(",");
                    builder.Append(paramType.FullName).Append(" ").Append("arg" + i);
                }
            }
            builder.Append(")");
            builder.Append("{").Append(body).Append("}");
            this.Methods.Add(builder.ToString());
        }

        private string GetVisitCode(VisitType visitType)
        {
            if (visitType == VisitType.Public) return "public";
            if (visitType == VisitType.Private) return "private";
            if (visitType == VisitType.Protected) return "protected";
            return "public";
        }

        private string GetAdditionType(AdditionType additionType)
        {
            if (additionType == AdditionType.Static) return "static";
            if (additionType == AdditionType.Abstract) return "abstract";
            if (additionType == AdditionType.Readonly) return "readonly";
            if (additionType == AdditionType.Const) return "const";
            return "";
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("namespace ").Append(Namespance).Append("{");
            builder.Append("public class ").Append(Class);
            if (Extend != null) builder.Append(":").Append(Extend.FullName);
            for (var i = 0; i < Interfaces.Count; i++)
            {
                if (Extend == null && i == 0) builder.Append(":");
                else builder.Append(",");
                builder.Append(Interfaces[i].FullName);
            }
            builder.Append("{");
            foreach (var str in Fields) builder.Append(str);
            foreach (var str in Propertys) builder.Append(str);
            if (DefaultConstructor) builder.Append("public $name(){}");
            foreach (var str in Constructors) builder.Append(str);
            foreach (var str in Methods) builder.Append(str);
            builder.Append("}");
            builder.Append("}");

            var source = builder.ToString();
            source = source.Replace("$name", Class);
            return source;
        }

        public enum VisitType
        {
            Public,
            Private,
            Protected
        }

        public enum AdditionType
        {
            Static,
            Abstract,
            Readonly,
            Const
        }
    }
}