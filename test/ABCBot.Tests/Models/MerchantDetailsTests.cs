using ABCBot.Models;
using ABCBot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ABCBot.Tests.Models
{
    public class MerchantDetailsTests
    {
        [Fact]
        public void ItShouldUpsertValue() {
            var merchantDetails = new MerchantDetails();

            var result = merchantDetails.UpsertValue("test");

            Assert.Single(merchantDetails.Values);
            Assert.NotNull(result);
        }

        [Fact]
        public void ItShouldExportToAMerchantEntryAndExcludeItemsWithoutASchema() {
            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "name", new MerchantDetailsItem() { Value = "test", SchemaItem = new KeyValueSchemaItem() { Type = "str" } } },
                    { "category", new MerchantDetailsItem() { Value = "category" } },
                    { "url", new MerchantDetailsItem() { Value = "https://merchant.url", SchemaItem = new KeyValueSchemaItem() { Type = "str" } } },
                    { "bch", new MerchantDetailsItem() { Value = "yes", SchemaItem = new KeyValueSchemaItem() { Type = "bool" } } },
                }
            };

            var result = merchantDetails.Export();

            Assert.Equal("test", result["name"]);
            Assert.False(result.ContainsKey("category"));
            Assert.Equal("https://merchant.url", result["url"]);
            Assert.Equal("yes", result["bch"]);
        }

        [Fact]
        public void ItShouldExportToAMerchantEntryAndRewriteImgTagToBeThePlacedImagePath() {
            var merchantDetails = new MerchantDetails()
            {
                PlacedImageName = "img.png",
                Values =
                {
                    { "img", new MerchantDetailsItem() { Value = "https://image.url", SchemaItem = new KeyValueSchemaItem() { Type = "str" } } },
                }
            };

            var result = merchantDetails.Export();

            Assert.Equal(merchantDetails.PlacedImageName, result["img"]);
        }
    }
}
