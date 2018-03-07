using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Models
{
    public class MerchantDetails
    {
        public string Category { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }
        public string StatusUrl { get; set; }
        public string ImageUrl { get; set; }
        public string EmailAddress { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }

        public string TwitterHandle { get; set; }
        public string FacebookHandle { get; set; }

        public bool AcceptsBCH { get; set; }
        public bool AcceptsBTC { get; set; }
        public bool AcceptsOtherCrypto { get; set; }

        public string Document { get; set; }
        public string Language { get; set; }

        public string PlacedImageName { get; set; }
    }
}
