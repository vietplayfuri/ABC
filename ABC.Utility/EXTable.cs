using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABC.Utility
{
    public static class EXTable
    {
        /// <summary>
        /// Fields what not included in real database
        /// </summary>
        public static string GetTableName<T>()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Any())
                return (attributes[0] as TableAttribute).Name;

            Type type = typeof(T);
            return type.Name;
        }

        public static string GetTableName(Type attributeType)
        {
            var attributes = attributeType.GetCustomAttributes(typeof(TableAttribute), true);
            if (attributes.Any())
                return (attributes[0] as TableAttribute).Name;

            return attributeType.Name;
        }
    }
}
