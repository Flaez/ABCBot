using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Models
{
    public class MerchantDetails
    {
        public Dictionary<string, MerchantDetailsItem> Values { get; }

        public string PlacedImageName { get; set; }

        public bool ShouldStopExecuting { get; set; }

        public MerchantDetails() {
            this.Values = new Dictionary<string, MerchantDetailsItem>();
        }

        public MerchantDetailsItem UpsertValue(string key) {
            if (!Values.TryGetValue(key, out var value)) {
                value = new MerchantDetailsItem();
                Values.Add(key, value);
            }

            return value;
        }

        public Dictionary<string, object> Export() {
            var merchantEntry = new Dictionary<string, object>();
            foreach (var kvp in Values) {
                if (kvp.Value.SchemaItem != null) {
                    AddDetailsToMerchantEntry(merchantEntry, kvp.Key, kvp.Value);
                }
            }

            return merchantEntry;
        }

        private void AddDetailsToMerchantEntry(Dictionary<string, object> merchantEntry, string key, MerchantDetailsItem item) {
            switch (key) {
                case "img": {
                        // Use the placed image path instead of the original image url
                        merchantEntry.Add(key, PlacedImageName);
                    }
                    break;
                default: {
                        merchantEntry.Add(key, item.Value);
                    }
                    break;
            }
        }
    }
}
