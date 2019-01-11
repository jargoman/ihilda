using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ripple.Testing.Utils
{
    public class AutoFilled : Attribute
    {
        public static bool IsRequired<T>(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttribute<AutoFilled>() != null && 
                   fieldInfo.FieldType == typeof (T);
        }

        public static List<TFieldType> Set<TFieldType>(object to, 
            Func<string, TFieldType> nameValueMapper)
        {
            return to.GetType().GetFields().Where(IsRequired<TFieldType>).ToList()
                .Select(f =>
                {
                    var value = nameValueMapper(f.Name);
                    f.SetValue(to, value);
                    return value;
                }).ToList();
        }
    }
}