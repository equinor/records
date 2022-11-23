// See https://aka.ms/new-console-template for more information

using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace RecordGenerator;
public class Program
{
    public enum RDFFormat {
        NQuads,
        Trig,
        JsonLd,
        CSV
    }

    public static RDFFormat parseRDFFormat(string formatString) =>
        formatString switch {
            "n4" => RDFFormat.NQuads,
            //"ttl" => RDFFormat.Turtle,
            "jsonld" => RDFFormat.JsonLd,
            "trig" => RDFFormat.Trig,
            "csv" => RDFFormat.CSV,
            _ => throw new Exception($"Invalid RDF Format {formatString}")
        };

    public static void Main(string[] args)
{
        if (args.Length != 2)
            throw new InvalidDataException("Usage: dotnet run RecordGenerator n4|trig|jsonld|csv <outfile>");
        var outFormat = parseRDFFormat(args[0]);

        var recordPrefix = "https://rdf.equinor.com/ontology/record/";
        var revisionPrefix = "https://rdf.equinor.com/ontology/revision/";
        var melPrefix = "https://rdf.equinor.com/ontology/mel/";
        var xsdPrefix = "http://www.w3.org/2001/XMLSchema#";
        var rdfPrefix = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        var pcaPrefix = "http://rds.posccaesar.org/ontology/plm/rdl/";
        var rdfsPrefix = "http://www.w3.org/2000/01/rdf-schema#";
        var provPrefix = "http://www.w3.org/ns/prov#";
        var isoPrefix = "http://standards.iso.org/8000#";
        var dcPrefix = "http://purl.org/dc/terms/";
        var tagPrefix = "https://stid.equinor.com/";
        var eqnPrefix = "https://rdf.equinor.com/fam/";
        var commonlibPrefix = "https://rdf.equinor.com/fam/tmp";
        var dataPrefix = "http://example.com/data/";
        
        var store = new TripleStore();

        

        
       var objects = Enumerable.Range(1,100).Select(i => $"{dataPrefix}Object{i}");

        foreach(var obj in objects){
            for(int i = 0; i < 100; i++){
                var graph = new Graph();
       
                var recordType = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}Record"));
                var replacesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}replaces"));
                var describesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}describes"));
                var scopesRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}isInScope"));
                var scopeNode = graph.CreateUriNode(UriFactory.Create($"{dataPrefix}Project"));
                var subRecordRel = graph.CreateUriNode(UriFactory.Create($"{recordPrefix}isSubRecordOf"));
                var rdfType = graph.CreateUriNode(UriFactory.Create($"{rdfPrefix}type"));
                var melSystemType = graph.CreateUriNode(UriFactory.Create($"{melPrefix}System"));
                var lengthType = graph.CreateUriNode(UriFactory.Create($"{pcaPrefix}Length"));
                var weightType = graph.CreateUriNode(UriFactory.Create($"{pcaPrefix}Weight"));


                var triples = new List<Triple>();

                var objectNode = graph.CreateUriNode(UriFactory.Create(obj));
                graph.BaseUri = UriFactory.Create($"{obj}/Record{i}");
                var recordNode = graph.CreateUriNode(graph.BaseUri);

                triples.Add(new Triple(recordNode, rdfType, recordType));
                triples.Add(new Triple(recordNode, describesRel, objectNode));
                triples.Add(new Triple(recordNode, scopesRel, scopeNode));
                if(i > 0)
                    triples.Add(new Triple(recordNode, replacesRel, graph.CreateUriNode(UriFactory.Create($"{obj}/Record{i-1}"))));
                
                triples.Add(new Triple(objectNode, rdfType, melSystemType));
                var iNode = graph.CreateLiteralNode($"{i}");
                triples.Add(new Triple(objectNode, lengthType, iNode));
                triples.Add(new Triple(objectNode, weightType, iNode));

                graph.Assert(triples);
                store.Add(graph);
            }

        IStoreWriter writer = outFormat switch {
            RDFFormat.JsonLd => new JsonLdWriter(),
            RDFFormat.NQuads => new NQuadsWriter(),
            //RDFFormat.Turtle => new CompressingTurtleWriter(),
            RDFFormat.Trig => new TriGWriter(),
            RDFFormat.CSV => new CsvStoreWriter()
        };

        writer.Save(store, args[1]);

        }
    }
}
        
