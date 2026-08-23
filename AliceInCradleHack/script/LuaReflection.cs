using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AliceInCradleHack.script
{
    internal static class LuaReflection
    {
        public static void Register(MoonSharp.Interpreter.Script script)
        {
            UserData.RegisterType(typeof(Assembly), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(Type), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(MemberInfo), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(MethodInfo), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(FieldInfo), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(PropertyInfo), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(EventInfo), InteropAccessMode.Reflection);
            UserData.RegisterType(typeof(ConstructorInfo), InteropAccessMode.Reflection);
            var table = new Table(script);
            table.Set("GetAssemblies", DynValue.NewCallback((c, a) => GetAssemblies(script)));
            table.Set("GetAssembly", DynValue.NewCallback((c, a) => GetAssembly(script, a)));
            table.Set("GetType", DynValue.NewCallback((c, a) => GetType(script, a)));
            table.Set("GetMembers", DynValue.NewCallback((c, a) => GetMembers(script, a, null)));
            table.Set("GetMethods", DynValue.NewCallback((c, a) => GetMembers(script, a, typeof(MethodInfo))));
            table.Set("GetFields", DynValue.NewCallback((c, a) => GetMembers(script, a, typeof(FieldInfo))));
            table.Set("GetProperties", DynValue.NewCallback((c, a) => GetMembers(script, a, typeof(PropertyInfo))));
            table.Set("GetEvents", DynValue.NewCallback((c, a) => GetMembers(script, a, typeof(EventInfo))));
            table.Set("GetConstructors", DynValue.NewCallback((c, a) => GetMembers(script, a, typeof(ConstructorInfo))));
            table.Set("GetMethod", DynValue.NewCallback((c, a) => GetNamedMember(script, a, typeof(MethodInfo))));
            table.Set("GetField", DynValue.NewCallback((c, a) => GetNamedMember(script, a, typeof(FieldInfo))));
            table.Set("GetProperty", DynValue.NewCallback((c, a) => GetNamedMember(script, a, typeof(PropertyInfo))));
            table.Set("GetEvent", DynValue.NewCallback((c, a) => GetNamedMember(script, a, typeof(EventInfo))));
            table.Set("CreateInstance", DynValue.NewCallback((c, a) => CreateInstance(script, a)));
            table.Set("GetValue", DynValue.NewCallback((c, a) => DynValue.FromObject(script, GetValue((MemberInfo)a[0].ToObject(), a.Count > 1 ? a[1].ToObject() : null))));
            table.Set("SetValue", DynValue.NewCallback((c, a) => SetValue(a)));
            table.Set("Invoke", DynValue.NewCallback((c, a) => Invoke(script, a)));
            var flags = new Table(script);
            foreach (BindingFlags value in Enum.GetValues(typeof(BindingFlags))) flags.Set(value.ToString(), DynValue.NewNumber((double)(int)value));
            table.Set("BindingFlags", DynValue.NewTable(flags));
            script.Globals.Set("Reflection", DynValue.NewTable(table));
        }

        private static DynValue GetAssemblies(MoonSharp.Interpreter.Script script)
        {
            var table = new Table(script); int i = 1;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) table.Set(i++, DynValue.FromObject(script, assembly));
            return DynValue.NewTable(table);
        }

        private static DynValue GetAssembly(MoonSharp.Interpreter.Script script, CallbackArguments args)
        {
            string name = args.AsType(0, "name", DataType.String).String;
            return DynValue.FromObject(script, AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }

        private static DynValue GetType(MoonSharp.Interpreter.Script script, CallbackArguments args)
        {
            string name = args.AsType(0, "name", DataType.String).String;
            string assemblyName = args.Count > 1 && args[1].Type == DataType.String ? args[1].String : null;
            Type type = assemblyName == null ? Type.GetType(name) : AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))?.GetType(name);
            type ??= AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name, false)).FirstOrDefault(t => t != null);
            return DynValue.FromObject(script, type);
        }

        private static DynValue GetMembers(MoonSharp.Interpreter.Script script, CallbackArguments args, Type filter)
        {
            var type = (Type)args[0].ToObject();
            var flags = args.Count > 1 ? (BindingFlags)(int)args[1].Number : BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            IEnumerable<MemberInfo> members = type.GetMembers(flags);
            if (filter != null) members = members.Where(m => filter.IsAssignableFrom(m.GetType()));
            var table = new Table(script); int i = 1;
            foreach (var member in members) table.Set(i++, DynValue.FromObject(script, member));
            return DynValue.NewTable(table);
        }

        private static DynValue GetNamedMember(MoonSharp.Interpreter.Script script, CallbackArguments args, Type filter)
        {
            var type = (Type)args[0].ToObject(); string name = args.AsType(1, "name", DataType.String).String;
            var flags = args.Count > 2 ? (BindingFlags)(int)args[2].Number : BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var member = type.GetMember(name, flags).FirstOrDefault(m => filter.IsAssignableFrom(m.GetType()));
            return DynValue.FromObject(script, member);
        }

        private static DynValue CreateInstance(MoonSharp.Interpreter.Script script, CallbackArguments args)
        {
            var type = (Type)args[0].ToObject();
            return DynValue.FromObject(script, Activator.CreateInstance(type, Values(args, 1)));
        }

        private static object GetValue(MemberInfo member, object target)
        {
            if (member is FieldInfo field) return field.GetValue(target);
            if (member is PropertyInfo property) return property.GetValue(target, null);
            throw new ArgumentException("Member is not a field or property");
        }

        private static DynValue SetValue(CallbackArguments args)
        {
            var member = (MemberInfo)args[0].ToObject(); object target = args[1].ToObject(); object value = ToObject(args[2]);
            if (member is FieldInfo field) field.SetValue(target, value);
            else if (member is PropertyInfo property) property.SetValue(target, value, null);
            else throw new ArgumentException("Member is not a field or property");
            return DynValue.NewBoolean(true);
        }

        private static DynValue Invoke(MoonSharp.Interpreter.Script script, CallbackArguments args)
        {
            var method = (MethodBase)args[0].ToObject(); object target = args[1].ToObject();
            return DynValue.FromObject(script, method.Invoke(target, Values(args, 2)));
        }

        private static object ToObject(DynValue value) => value.ToObject();

        private static object[] Values(CallbackArguments args, int start)
        {
            var values = new object[Math.Max(0, args.Count - start)];
            for (int i = start; i < args.Count; i++) values[i - start] = ToObject(args[i]);
            return values;
        }
    }
}
