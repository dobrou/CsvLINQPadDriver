﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsvLINQPadDriver.CodeGen;
using LINQPad;

namespace CsvLINQPadDriver.DataDisplay
{
    internal class CsvRowMemberProvider : ICustomMemberProvider
    {
        protected class ProviderData
        {
            public IList<PropertyInfo> properties;
            public IList<FieldInfo> fields;
            public Func<object,object[]> valuesGet;
        }

        protected static bool IsSupportedType(Type t)
        {
            return typeof(CsvRowBase).IsAssignableFrom(t);
        }

        protected static bool IsMemberVisible(MemberInfo member)
        {
            return
                (MemberTypes.Field | MemberTypes.Property).HasFlag(member.MemberType)
                && (member.GetCustomAttributes(typeof(HideFromDumpAttribute), true).Length == 0)
            ;
        }

        protected static ProviderData GetProviderData(Type objectType)
        {
            var data = new ProviderData()
            {
                properties = objectType.GetProperties().Where(IsMemberVisible).ToList(),
                fields = objectType.GetFields().Where(IsMemberVisible).ToList(),
            };

            var param = Expression.Parameter(typeof(object));
            data.valuesGet = Expression.Lambda<Func<object,object[]>>(
                Expression.NewArrayInit( typeof(object),
                    Enumerable.Concat(
                        data.properties.Select(pi => Expression.Property( Expression.TypeAs(param, objectType), pi)),
                        data.fields.Select(fi => Expression.Field( Expression.TypeAs(param, objectType), fi))
                    )
                )            
            , param).Compile();
            
            return data;
        }

        protected static Dictionary<Type, ProviderData> ProvidersDataCache = new Dictionary<Type, ProviderData>();

        public static ICustomMemberProvider GetCsvRowMemberProvider(object objectToDisplay)
        {
            var objectType = objectToDisplay.GetType();
            if (!IsSupportedType(objectType))
                return null;

            ProviderData providerData;
            if (!ProvidersDataCache.TryGetValue(objectType, out providerData))
            {
                providerData = GetProviderData(objectType);
                ProvidersDataCache.Add(objectType, providerData);
            }
            return new CsvRowMemberProvider(objectToDisplay, providerData);

        }

        private object objectToDisplay;
        private ProviderData providerData;

        protected CsvRowMemberProvider(object objectToDisplay, ProviderData providerData)
        {
            this.objectToDisplay = objectToDisplay;
            this.providerData = providerData;
        }

        public IEnumerable<string> GetNames()
        {
            return Enumerable.Concat(
                providerData.properties.Select(p => p.Name),
                providerData.fields.Select(p => p.Name)
            );
        }

        public IEnumerable<Type> GetTypes()
        {
            return Enumerable.Concat(
                providerData.properties.Select(p => p.PropertyType),
                providerData.fields.Select(p => p.FieldType)
            );
        }

        public IEnumerable<object> GetValues()
        {
            return providerData.valuesGet(objectToDisplay);
        }
    }
}
