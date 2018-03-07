using ABCBot.Models;
using ABCBot.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public interface IPipelineContext
    {
        int TaskIdentifier { get; }
        MerchantDetails MerchantDetails { get; }
        IRepositoryContext RepositoryContext { get; }
        Dictionary<string, object> Data { get; }
    }
}
