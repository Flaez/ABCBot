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
            var categoryFileName = $"{context.MerchantDetails.Category.ToLower()}.yml";
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
            merchantEntry.Add("name", context.MerchantDetails.Name);
            merchantEntry.Add("url", context.MerchantDetails.Url);

            if (!string.IsNullOrEmpty(context.MerchantDetails.EmailAddress)) {
                merchantEntry.Add("email_address", context.MerchantDetails.EmailAddress);
            }
            if (!string.IsNullOrEmpty(context.MerchantDetails.TwitterHandle)) {
                merchantEntry.Add("twitter", context.MerchantDetails.TwitterHandle);
            }
            if (!string.IsNullOrEmpty(context.MerchantDetails.FacebookHandle)) {
                merchantEntry.Add("facebook", context.MerchantDetails.FacebookHandle);
            }

            merchantEntry.Add("img", context.MerchantDetails.PlacedImageName);
            merchantEntry.Add("bch", context.MerchantDetails.AcceptsBCH ? "Yes" : "No");

            // TODO: Add the other fields here
            if (context.MerchantDetails.AcceptsBTC) {
                merchantEntry.Add("btc", "Yes");
            }
            if (context.MerchantDetails.AcceptsOtherCrypto) {
                merchantEntry.Add("othercrypto", "Yes");
            }
            if (!string.IsNullOrEmpty(context.MerchantDetails.City)) {
                merchantEntry.Add("city", context.MerchantDetails.City);
            }
            if (!string.IsNullOrEmpty(context.MerchantDetails.Language)) {
                merchantEntry.Add("lang", context.MerchantDetails.Language);
            }

            if (!string.IsNullOrEmpty(context.MerchantDetails.Document)) {
                merchantEntry.Add("doc", context.MerchantDetails.Document);
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
