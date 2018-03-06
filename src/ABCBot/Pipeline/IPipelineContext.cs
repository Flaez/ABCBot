using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public interface IPipelineContext
    {
        int TaskIdentifier { get; }
        Dictionary<string, object> Data { get; }
    }
}
