using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Schema
{
    public class MappingSchemaItem : ISchemaItem
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public Dictionary<string, ISchemaItem> Mapping { get; }

        public MappingSchemaItem() {
            this.Mapping = new Dictionary<string, ISchemaItem>();
        }
    }
}
