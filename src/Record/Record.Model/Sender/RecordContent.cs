using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Writing;

namespace Records.Sender;

public class RecordContent : StringContent
{
    public RecordContent(string record) : base(record)
    {
        Headers.ContentType = new("application/ld+json");
    }

    public RecordContent(Immutable.Record record) : this(record.ToString<JsonLdWriter>()) { }

}