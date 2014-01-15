using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CsvLINQPadDriver.CodeGen;
using LINQPad;

namespace CsvLINQPadDriver.DataDisplay
{
    internal class CsvRowMemberProvider : ICustomMemberProvider
    {
        public static bool IsCsvRowType(Type t)
        {
            while (t != null)
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof (CsvRowBase<>)) return true;
                t = t.BaseType;
            }
            return false;
        }

        public static bool IsMemberVisible(MemberInfo member)
        {
            return
                (MemberTypes.Field | MemberTypes.Property).HasFlag(member.MemberType)
                && (member.GetCustomAttributes(typeof(HideFromDumpAttribute), true).Length == 0)
            ;
        }

        public static ICustomMemberProvider GetCsvRowMemberProvider(object objectToDisplay)
        {
            if (!IsCsvRowType(objectToDisplay.GetType()))
                return null;
            return new CsvRowMemberProvider(objectToDisplay);
        }

        private object objectToDisplay;
        private IList<PropertyInfo> propsToWrite;
        private IList<FieldInfo> fieldsToWrite;

        public CsvRowMemberProvider(object objectToDisplay)
        {
            this.objectToDisplay = objectToDisplay;
            propsToWrite = objectToDisplay.GetType().GetProperties()
                .Where(IsMemberVisible)
                .ToList();
            fieldsToWrite = objectToDisplay.GetType().GetFields()
                .Where(IsMemberVisible)
                .ToList();
        }

        public IEnumerable<string> GetNames()
        {
            return Enumerable.Concat(
                propsToWrite.Select(p => p.Name),
                fieldsToWrite.Select(p => p.Name)
            );
        }

        public IEnumerable<Type> GetTypes()
        {
            return Enumerable.Concat(
                propsToWrite.Select(p => p.PropertyType),
                fieldsToWrite.Select(p => p.FieldType)
            );
        }

        public IEnumerable<object> GetValues()
        {
            return Enumerable.Concat(
                propsToWrite.Select(p => p.GetValue(objectToDisplay, null)),
                fieldsToWrite.Select(p => p.GetValue(objectToDisplay))
            );
        }
    }
}
