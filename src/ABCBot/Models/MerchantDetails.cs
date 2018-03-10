using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Models
{
    public class MerchantDetails
    {
        public Dictionary<string, MerchantDetailsItem> Values { get; }

        public string PlacedImageName { get; set; }

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
    }
}
