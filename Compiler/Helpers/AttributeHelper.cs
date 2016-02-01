using System;
using System.Linq;

namespace Compiler.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class AttributeHelper
    {
        /// <summary>
        /// Gets the attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="E"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static T GetAttribute<T, E>(E enumValue) where T : Attribute
        {
            var enumType = typeof(E);
            var attributeType = typeof(T);

            return enumType
                .GetMember(enumValue.ToString())
                .FirstOrDefault()
                .GetCustomAttributes(attributeType, false)
                .Cast<T>()
                .FirstOrDefault();
        }
    }
}