using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Schema
{
    public class KeyValueSchemaItem : ISchemaItem
    {
        public string Type { get; set; }

        public string Pattern { get; set; }

        public bool Required { get; set; }
        public bool Unique { get; set; }
    }
}
