// See https://aka.ms/new-console-template for more information

using VDS.RDF;
using VDS.RDF.Writing;
using System.Collections;

namespace RecordGenerator;
public class Program
{
    public enum RDFFormat
    {
        NQuads,
        Trig,
        JsonLd,
        CSV
    }

    public static RDFFormat parseRDFFormat(string formatString) =>
        formatString switch
        {
            "n4" => RDFFormat.NQuads,
            "jsonld" => RDFFormat.JsonLd,
            "trig" => RDFFormat.Trig,
            "csv" => RDFFormat.CSV,
            _ => throw new Exception($"Invalid RDF Format {formatString}")
        };

    public static void Main(string[] args)
    {
        if (args.Length != 5)
            throw new InvalidDataException("Usage: dotnet run <nobject> <nscopes> <nrecords> n4|trig|jsonld|csv <outfile>");
        RDFFormat outFormat = parseRDFFormat(args.Reverse().Skip(1).First());
        var outfile = args.Last();
        var nObjects = int.Parse(args[0]);
        var nRecords = int.Parse(args[1]);
        var nScopes = int.Parse(args[2]);

        var recordPrefix = "https://rdf.equinor.com/ontology/record/";
        // var revisionPrefix = "https://rdf.equinor.com/ontology/revision/";
        var melPrefix = "https://rdf.equinor.com/ontology/mel/";
        // var xsdPrefix = "http://www.w3.org/2001/XMLSchema#";
        var rdfPrefix = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        var pcaPrefix = "http://rds.posccaesar.org/ontology/plm/rdl/";
        // var rdfsPrefix = "http://www.w3.org/2000/01/rdf-schema#";
        // var provPrefix = "http://www.w3.org/ns/prov#";
        // var isoPrefix = "http://standards.iso.org/8000#";
        // var dcPrefix = "http://purl.org/dc/terms/";
        var dataPrefix = "http://example.com/data/";

        var store = new TripleStore();
        var objects = Enumerable.Range(1, nObjects).Select(i => $"{dataPrefix}Object{i}");
        var scopes = Enumerable.Range(0, (int)Math.Ceiling(Math.Log2(nScopes))).Select(i => (i, $"{dataPrefix}Scope{i}"));

        for (int scopeNo = 1; scopeNo <= nScopes; scopeNo++)
        {
            var scopeMap = new BitArray(new int[] { scopeNo });
            foreach (var obj in objects)
            {
                for (int i = 0; i < nRecords; i++)
                {
                    var graph = new Graph(new UriNode(new Uri($"{obj}-{scopeNo}-Record{i}")));
                    graph.NamespaceMap.AddNamespace("data:", new Uri(dataPrefix));
                    graph.NamespaceMap.AddNamespace("rdl:", new Uri(pcaPrefix));

                    var recordType = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}Record"));
                    var replacesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}replaces"));
                    var describesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}describes"));
                    var scopesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}isInScope"));
                    var subRecordRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}isSubRecordOf"));
                    var rdfType = graph.CreateUriNode(UriFactory.Create($"{rdfPrefix}type"));
                    var melSystemType = graph.CreateUriNode(UriFactory.Create($"{melPrefix}System"));
                    var lengthType = graph.CreateUriNode(UriFactory.Create($"{pcaPrefix}Length"));
                    var weightType = graph.CreateUriNode(UriFactory.Create($"{pcaPrefix}Weight"));

                    var triples = new List<Triple>();

                    var objectNode = graph.CreateUriNode(UriFactory.Create(obj));
                    graph.BaseUri = new Uri($"{obj}-{scopeNo}-Record{i}");
                    
                    var recordNode = graph.CreateUriNode(graph.BaseUri);

                    triples.Add(new Triple(recordNode, rdfType, recordType));
                    triples.Add(new Triple(recordNode, describesRel, objectNode));

                    triples.AddRange(scopes
                        .Where(i => scopeMap[i.Item1])
                        .Select(i => new Triple(recordNode, scopesRel, graph.CreateUriNode(UriFactory.Create(i.Item2))))
                    );

                    if (i > 0)
                        triples.Add(new Triple(recordNode, replacesRel, graph.CreateUriNode(UriFactory.Create($"{obj}-{scopeNo}-Record{i - 1}"))));

                    triples.Add(new Triple(objectNode, rdfType, melSystemType));
                    var iNode = graph.CreateLiteralNode($"{i}");
                    triples.Add(new Triple(objectNode, lengthType, iNode));
                    triples.Add(new Triple(objectNode, weightType, iNode));

                    graph.Assert(triples);
                    store.Add(graph);
                }
            }

            IStoreWriter writer = outFormat switch
            {
                RDFFormat.JsonLd => new JsonLdWriter(),
                RDFFormat.NQuads => new NQuadsWriter(),
                RDFFormat.Trig => new TriGWriter(),
                RDFFormat.CSV => new CsvStoreWriter(),
                _ => throw new Exception("Invalid RDF Format")
            };

            writer.Save(store, outfile);

        }
    }
}

