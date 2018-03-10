using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ABCBot.Schema
{
    public class SchemaLoader : ISchemaLoader
    {
        public ISchemaItem LoadSchema(TextReader schemaTextReader) {
            var deserializer = new DeserializerBuilder()
                                   .WithNamingConvention(new CamelCaseNamingConvention())
                                   .Build();

            var document = deserializer.Deserialize<Dictionary<object, object>>(schemaTextReader);

            var schema = new Dictionary<string, ISchemaItem>();

            return ReadSchemaItem(document);
        }

        private ISchemaItem ReadSchemaItem(Dictionary<object, object> node) {
            var type = (string)node["type"];

            switch (type) {
                case "map": {
                        return ReadMappingSchemaItem(node);
                    }
                case "seq": {
                        return ReadSequenceSchemaItem(node);
                    }
                case "bool":
                case "str": {
                        return ReadKeyValueSchemaItem(node);
                    }
            }

            throw new InvalidOperationException("Invalid schema item type.");
        }

        private ISchemaItem ReadKeyValueSchemaItem(Dictionary<object, object> node) {
            var schemaItem = new KeyValueSchemaItem();

            if (node.TryGetValue("required", out object required)) {
                schemaItem.Required = (string)required == "yes" ? true : false;
            }
            if (node.TryGetValue("unique", out object unique)) {
                schemaItem.Unique = (string)unique == "yes" ? true : false;
            }
            if (node.TryGetValue("pattern", out object pattern)) {
                schemaItem.Pattern = (string)pattern;
            }

            return schemaItem;
        }

        private ISchemaItem ReadSequenceSchemaItem(Dictionary<object, object> node) {
            var sequenceSchemaItem = new SequenceSchemaItem();

            var sequenceNode = node["sequence"] as List<object>;

            foreach (Dictionary<object, object> childNode in sequenceNode) {
                var childSchemaItem = ReadSchemaItem(childNode);

                sequenceSchemaItem.Items.Add(childSchemaItem);
            }

            return sequenceSchemaItem;
        }

        private ISchemaItem ReadMappingSchemaItem(Dictionary<object, object> node) {
            var mapSchemaItem = new MappingSchemaItem();

            var mappingNode = node["mapping"] as Dictionary<object, object>;
            foreach (var kvp in mappingNode) {
                var key = kvp.Key as string;
                var value = kvp.Value as Dictionary<object, object>;

                var childSchemaItem = ReadSchemaItem(value);

                mapSchemaItem.Mapping.Add(key, childSchemaItem);
            }

            return mapSchemaItem;
        }
    }
}
