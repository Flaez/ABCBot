using ABCBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ABCBot.Pipeline.Tasks
{
    public class CategoryYmlAmendmentTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var categoryFileName = $"{context.MerchantDetails.Values["category"].Value.ToLower()}.yml";
            var categoryFilePath = Path.Combine(context.RepositoryContext.RepositoryDirectory, "_data", categoryFileName);

            var deserializer = new DeserializerBuilder()
                                   .WithNamingConvention(new CamelCaseNamingConvention())
                                   .Build();

            Dictionary<string, List<Dictionary<string, object>>> document;
            using (var fileStream = new FileStream(categoryFilePath, FileMode.Open)) {
                using (var streamReader = new StreamReader(fileStream)) {
                    document = deserializer.Deserialize<Dictionary<string, List<Dictionary<string, object>>>>(streamReader);
                }
            }

            var websitesCollection = document["websites"];

            var merchantEntry = new Dictionary<string, object>();
            foreach (var kvp in context.MerchantDetails.Values) {
                if (kvp.Value.SchemaItem != null) {
                    AddDetailsToMerchantEntry(merchantEntry, context.MerchantDetails, kvp.Key, kvp.Value);
                }
            }

            websitesCollection.Add(merchantEntry);

            document["websites"] = websitesCollection.OrderBy(x => x["name"]).ToList();

            var serializer = new SerializerBuilder()
                                .Build();
            var yml = serializer.Serialize(document);

            yml = AddSpacesBetweenListItems(yml);

            File.WriteAllText(categoryFilePath, yml);

            return Task.FromResult(PipelineProcessingResult.Success());
        }

        private void AddDetailsToMerchantEntry(Dictionary<string, object> merchantEntry, MerchantDetails merchantDetails, string key, MerchantDetailsItem item) {
            switch (key) {
                case "img": {
                        // Use the placed image path instead of the original image url
                        merchantEntry.Add(key, merchantDetails.PlacedImageName);
                    }
                    break;
                default: {
                        merchantEntry.Add(key, item.Value);
                    }
                    break;
            }
        }

        // This is a bit of a hack to add spaces between list items in the generated yml
        // The serializer doesn't seem to have an option to do this, and writing a custom emitter is overkill
        // This is done to play nice with the existing hand-generated yml files
        private string AddSpacesBetweenListItems(string yml) {
            var builder = new StringBuilder();

            var lines = yml.NormalizeLineEndings().Split('\n');

            bool firstListItem = true;

            for (var i = 0; i < lines.Length - 1; i++) { // skip the empty line at the end
                if (lines[i].Trim().StartsWith('-')) {
                    if (firstListItem) {
                        firstListItem = false;
                    } else {
                        // Add a new line for all items after the first list item
                        builder.AppendLine("");
                    }
                }

                if (i > 0) {
                    builder.Append("    ");
                }

                builder.AppendLine(lines[i]);
            }

            return builder.ToString();
        }
    }
}
