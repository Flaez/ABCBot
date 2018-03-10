using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABCBot.Schema
{
    public interface ISchemaLoader
    {
        ISchemaItem LoadSchema(TextReader schemaTextReader);
    }
}
