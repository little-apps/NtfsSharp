using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer
{
    internal class Attributes : ObservableCollection<Attributes.Attribute>
    {
        internal Attributes()
        {
            
        }

        internal Attributes CreateAttributesFromStruct<T>(T s) where T : struct
        {
            var attributes = new Attributes();

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                var fieldName = fieldInfo.Name;
                var fieldValue = fieldInfo.GetValue(s);

                attributes.Add(new Attribute(fieldName, fieldValue as string));
            }

            return attributes;
        }

        internal class Attribute
        {
            public string Name { get; }
            public string Value { get; }

            public Attribute(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
    }
}
