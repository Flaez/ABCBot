using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Schema
{
    public class SequenceSchemaItem : ISchemaItem
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public List<ISchemaItem> Items { get; }

        public SequenceSchemaItem() {
            this.Items = new List<ISchemaItem>();
        }
    }
}
