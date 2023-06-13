
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
        public const string generatedAtTime = "http://www.w3.org/ns/prov#generatedAtTime";

        public const string Att = "https://rdf.equinor.com/ontology/attachment/";
        public const string Type = $"{Att}File";
        public const string FileType = $"{Att}FileType";
        public const string ModelType = $"{Att}ModelType";
        public const string DocumentType = $"{Att}DocumentType";

        public const string Dcat = "http://www.w3.org/ns/dcat#";
        public const string HasByteSize = $"{Dcat}byteSize";

        public const string Dcterms = "http://purl.org/dc/terms/";
        public const string HasLanguage = $"{Dcterms}language";

        public const string Spdx = "http://spdx.org/rdf/terms#";
        public const string HasTitle = $"{Spdx}fileName";
        public const string HasChecksum = $"{Spdx}checksum";
        public const string HasChecksumValue = $"{Spdx}checksumValue";
        public const string HasChecksumAlgorithm = $"{Spdx}algorithm";
    }

    public struct DataType
    {
        internal const string XsdPrefix = "http://www.w3.org/2001/XMLSchema#";
        internal const string String = $"{XsdPrefix}string";
        internal const string Decimal = $"{XsdPrefix}decimal";
        internal const string Date = $"{XsdPrefix}date";
        internal const string HexBinary = $"{XsdPrefix}hexBinary";
        internal const string Language = $"{XsdPrefix}language";

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

