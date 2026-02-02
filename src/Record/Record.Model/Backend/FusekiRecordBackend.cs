using VDS.RDF;
using VDS.RDF.Query;

namespace Records.Backend;

public class FusekiRecordBackend : RecordBackendBase
{
    private readonly FusekiClient _fusekiClient;
    public FusekiRecordBackend(FusekiClient fusekiClient)
    {
        _fusekiClient = fusekiClient;
    }
    
    public override ITripleStore TripleStore()
    {
        throw new NotImplementedException();
    }

    public override string ToString(IStoreWriter writer)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<INode> SubjectWithType(UriNode type)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<string> LabelsOfSubject(UriNode subject)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithSubject(UriNode subject)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithPredicate(UriNode predicate)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithObject(INode @object)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithPredicateAndObject(UriNode predicate, INode @object)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithSubjectObject(UriNode subject, INode @object)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
    {
        throw new NotImplementedException();
    }

    public override IGraph ConstructQuery(SparqlQuery query)
    {
        throw new NotImplementedException();
    }

    public override SparqlResultSet Query(SparqlQuery query)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<string> Sparql(string queryString)
    {
        throw new NotImplementedException();
    }

    public override IGraph GetMergedGraphs()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IGraph> GetContentGraphs()
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Triple> Triples()
    {
        throw new NotImplementedException();
    }

    public override bool ContainsTriple(Triple triple)
    {
        throw new NotImplementedException();
    }

    public override string ToCanonString()
    {
        throw new NotImplementedException();
    }
}