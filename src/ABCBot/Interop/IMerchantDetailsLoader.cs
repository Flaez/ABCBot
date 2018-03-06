using ABCBot.Models;
using Optional;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Interop
{
    public interface IMerchantDetailsLoader
    {
        Task<Option<MerchantDetails>> ExtractDetails(int identifier);
    }
}
