
namespace Records;
public struct Namespaces
{
    public struct Record
    {
        public const string BaseUrl = "https://rdf.equinor.com/ontology/record/";
        public const string RecordType = $"{BaseUrl}Record";
        public const string Replaces = $"{BaseUrl}replaces";
        public const string IsInScope = $"{BaseUrl}isInScope";
        public const string Describes = $"{BaseUrl}describes";
        public const string IsSubRecordOf = $"{BaseUrl}isSubRecordOf";
    }
    public struct FileContent
    {
        public const string Type = "https://rdf.equinor.com/ontology/attachment/File";
        public const string generatedAtTime = "http://www.w3.org/ns/prov#generatedAtTime";
        public const string Xsd = "http://www.w3.org/2001/XMLSchema#";

        public const string Dcat = "http://www.w3.org/ns/dcat#";
        public const string HasByteSize = $"{Dcat}byteSize";
        public const string HasMediaType = $"{Dcat}mediaType";
        public const string MediaType = "https://www.iana.org/assignments/media-types/application/";

        public const string Dcterms = "http://purl.org/dc/terms/";
        public const string HasLanguage = $"{Dcterms}language";

        public const string Spdx = "http://spdx.org/rdf/terms#";
        public const string HasTitle = $"{Spdx}fileName";
        public const string HasChecksum = $"{Spdx}checksum";
        public const string HasChecksumValue = $"{Spdx}checksumValue";
        public const string HasChecksumAlgorithm = $"{Spdx}algorithm";
    }

    public struct Rdf
    {
        public const string BaseUrl = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const string Type = $"{BaseUrl}type";
        public const string Nil = $"{BaseUrl}nil";
    }

    public struct Rdfs
    {
        public const string BaseUrl = "http://www.w3.org/2000/01/rdf-schema#";
        public const string Comment = $"{BaseUrl}comment";
    }

    public struct Shacl
    {
        public const string BaseUrl = "http://www.w3.org/ns/shacl#";
        public const string Conforms = $"{BaseUrl}conforms";
        public const string ResultMessage = $"{BaseUrl}resultMessage";
        public const string ResultSeverity = $"{BaseUrl}resultSeverity";
    }
}

