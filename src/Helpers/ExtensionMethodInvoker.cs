namespace Nine.Imaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Invokes a series of fluent extension methods based on key value pair parameters.
    /// </summary>
    class ExtensionMethodInvoker
    {
        private readonly Dictionary<string, MethodInfo> methods;
        private readonly Func<Type, string, object> convert;

        public ExtensionMethodInvoker(params Type[] declaredTypes) : this(null, declaredTypes) { }
        public ExtensionMethodInvoker(Func<Type, string, object> convert, params Type[] declaredTypes)
        {
            this.convert = convert;
            this.methods = declaredTypes
                .SelectMany(GetExtensionMethods)
                .ToLookup(method => method.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(method => method.GetParameters().Length).First(), StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<MethodInfo> GetExtensionMethods(Type type)
        {
            return type.GetTypeInfo().DeclaredMethods.Where(m => m.GetCustomAttribute<ExtensionAttribute>() != null);
        }

        public object Invoke(object target, string parameter)
        {
            return Invoke(target, parameter.Split('&').Select(CreateKeyValuePair).Where(kv => kv.Key != null).ToArray());
        }

        private KeyValuePair<string, string> CreateKeyValuePair(string part)
        {
            var parts = part.Split('=');
            if (parts.Length <= 0) return default(KeyValuePair<string, string>);
            return new KeyValuePair<string, string>(parts[0].Trim(), parts.Length > 1 ? parts[1] : null);
        }

        public object Invoke(object target, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            string defaultValue = null;
            MethodInfo method = null, current;
            List<ParameterInfo> methodParams = null;
            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>();

            foreach (var param in parameters)
            {
                if (methods.TryGetValue(param.Key, out current))
                {
                    var existing = methodParams?.FirstOrDefault(p => p != null && string.Equals(p.Name, param.Key, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        args.Add(param);
                        methodParams.Remove(existing);
                    }
                    else
                    {
                        target = Invoke(target, method, defaultValue, args);
                        defaultValue = param.Value;
                        method = current;
                        methodParams = current.GetParameters().ToList();
                        args.Clear();
                    }
                }
                else
                {
                    if (method != null)
                    {
                        args.Add(param);
                    }
                }
            }

            return Invoke(target, method, defaultValue, args);
        }

        private object Invoke(object target, MethodInfo method, string defaultValue, List<KeyValuePair<string, string>> args)
        {
            if (method == null)
            {
                return target;
            }

            var signature = method.GetParameters();
            var parameters = new object[signature.Length];
            parameters[0] = target;

            for (var i = 1; i < signature.Length; i++)
            {
                var arg = args.FirstOrDefault(a => string.Equals(a.Key, signature[i].Name, StringComparison.OrdinalIgnoreCase));
                if (arg.Key != null)
                {
                    parameters[i] = convert?.Invoke(signature[i].ParameterType, arg.Value) ?? ConvertPrimitiveTypes(signature[i].ParameterType, arg.Value);
                }
                else if (i == 1 && defaultValue != null)
                {
                    parameters[i] = convert?.Invoke(signature[i].ParameterType, defaultValue) ?? ConvertPrimitiveTypes(signature[i].ParameterType, defaultValue);
                }
                else
                {
                    parameters[i] = signature[i].HasDefaultValue ? signature[i].DefaultValue : GetDefaultValue(signature[i].ParameterType);
                }
            }

            var result = method.Invoke(null, parameters);

            return method.ReturnType != typeof(void) ? result : target;
        }

        private static object ConvertPrimitiveTypes(Type type, string value)
        {
            if (type == typeof(string)) return value;
            if (type == typeof(int)) return int.Parse(value);
            if (type == typeof(byte)) return byte.Parse(value);
            if (type == typeof(float)) return float.Parse(value);
            if (type == typeof(double)) return double.Parse(value);
            if (type == typeof(Color)) return Color.Parse(value);
            if (type.GetTypeInfo().IsEnum) return Enum.Parse(type, value, true);

            return null;
        }

        private static object GetDefaultValue(Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
