using VDS.RDF;

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
        public const string HasContent = $"{BaseUrl}hasContent";
        public const string BlankGraph = $"{BaseUrl}BlankGraph#";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Record.BaseUrl);
            public readonly static Uri RecordType = new(Record.RecordType);
            public readonly static Uri Replaces = new(Record.Replaces);
            public readonly static Uri IsInScope = new(Record.IsInScope);
            public readonly static Uri Describes = new(Record.Describes);
            public readonly static Uri IsSubRecordOf = new(Record.IsSubRecordOf);
            public readonly static Uri HasContent = new(Record.HasContent);
        }

        public class UriNodes
        {
            public readonly static UriNode RecordType = new(Uris.RecordType);
            public readonly static UriNode Replaces = new(Uris.Replaces);
            public readonly static UriNode IsInScope = new(Uris.IsInScope);
            public readonly static UriNode Describes = new(Uris.Describes);
            public readonly static UriNode IsSubRecordOf = new(Uris.IsSubRecordOf);
            public readonly static UriNode HasContent = new(Uris.HasContent);
        }
    }

    public struct FileContent
    {
        public const string generatedAtTime = "http://www.w3.org/ns/prov#generatedAtTime";

        public const string Att = "https://rdf.equinor.com/ontology/attachment/";
        public const string Type = $"{Att}File";
        public const string FileExtension = $"{Att}FileExtension";
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

        public static class Uris
        {
            public readonly static Uri generatedAtTime = new(FileContent.generatedAtTime);

            public readonly static Uri Att = new(FileContent.Att);
            public readonly static Uri Type = new(FileContent.Type);
            public readonly static Uri FileExtension = new(FileContent.FileExtension);
            public readonly static Uri ModelType = new(FileContent.ModelType);
            public readonly static Uri DocumentType = new(FileContent.DocumentType);

            public readonly static Uri Dcat = new(FileContent.Dcat);
            public readonly static Uri HasByteSize = new(FileContent.HasByteSize);

            public readonly static Uri Dcterms = new(FileContent.Dcterms);
            public readonly static Uri HasLanguage = new(FileContent.HasLanguage);

            public readonly static Uri Spdx = new(FileContent.Spdx);
            public readonly static Uri HasTitle = new(FileContent.HasTitle);
            public readonly static Uri HasChecksum = new(FileContent.HasChecksum);
            public readonly static Uri HasChecksumValue = new(FileContent.HasChecksumValue);
            public readonly static Uri HasChecksumAlgorithm = new(FileContent.HasChecksumAlgorithm);
        }

        public class UriNodes
        {
            public readonly static UriNode generatedAtTime = new(Uris.generatedAtTime);

            public readonly static UriNode Att = new(Uris.Att);
            public readonly static UriNode Type = new(Uris.Type);
            public readonly static UriNode FileExtension = new(Uris.FileExtension);
            public readonly static UriNode ModelType = new(Uris.ModelType);
            public readonly static UriNode DocumentType = new(Uris.DocumentType);

            public readonly static UriNode Dcat = new(Uris.Dcat);
            public readonly static UriNode HasByteSize = new(Uris.HasByteSize);

            public readonly static UriNode Dcterms = new(Uris.Dcterms);
            public readonly static UriNode HasLanguage = new(Uris.HasLanguage);

            public readonly static UriNode Spdx = new(Uris.Spdx);
            public readonly static UriNode HasTitle = new(Uris.HasTitle);
            public readonly static UriNode HasChecksum = new(Uris.HasChecksum);
            public readonly static UriNode HasChecksumValue = new(Uris.HasChecksumValue);
            public readonly static UriNode HasChecksumAlgorithm = new(Uris.HasChecksumAlgorithm);
        }
    }

    public struct DataType
    {
        internal const string XsdPrefix = "http://www.w3.org/2001/XMLSchema#";
        internal const string String = $"{XsdPrefix}string";
        internal const string Decimal = $"{XsdPrefix}decimal";
        internal const string Date = $"{XsdPrefix}date";
        internal const string HexBinary = $"{XsdPrefix}hexBinary";
        internal const string Language = $"{XsdPrefix}language";

        public static class Uris
        {
            public readonly static Uri XsdPrefix = new(DataType.XsdPrefix);
            public readonly static Uri String = new(DataType.String);
            public readonly static Uri Decimal = new(DataType.Decimal);
            public readonly static Uri Date = new(DataType.Date);
            public readonly static Uri HexBinary = new(DataType.HexBinary);
            public readonly static Uri Language = new(DataType.Language);
        }

        public class UriNodes
        {
            public readonly static UriNode XsdPrefix = new(Uris.XsdPrefix);
            public readonly static UriNode String = new(Uris.String);
            public readonly static UriNode Decimal = new(Uris.Decimal);
            public readonly static UriNode Date = new(Uris.Date);
            public readonly static UriNode HexBinary = new(Uris.HexBinary);
            public readonly static UriNode Language = new(Uris.Language);
        }

    }

    public struct Rdf
    {
        public const string BaseUrl = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public const string Type = $"{BaseUrl}type";
        public const string Nil = $"{BaseUrl}nil";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Rdf.BaseUrl);
            public readonly static Uri Type = new(Rdf.Type);
            public readonly static Uri Nil = new(Rdf.Nil);
        }

        public class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);
            public readonly static UriNode Type = new(Uris.Type);
            public readonly static UriNode Nil = new(Uris.Nil);
        }
    }

    public struct Rdfs
    {
        public const string BaseUrl = "http://www.w3.org/2000/01/rdf-schema#";
        public const string Comment = $"{BaseUrl}comment";
        public const string Label = $"{BaseUrl}label";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Rdfs.BaseUrl);
            public readonly static Uri Comment = new(Rdfs.Comment);
            public readonly static Uri Label = new(Rdfs.Label);
        }

        public class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);
            public readonly static UriNode Comment = new(Uris.Comment);
            public readonly static UriNode Label = new(Uris.Label);
        }
    }

    public struct Shacl
    {
        public const string BaseUrl = "http://www.w3.org/ns/shacl#";
        public const string Conforms = $"{BaseUrl}conforms";
        public const string ResultMessage = $"{BaseUrl}resultMessage";
        public const string ResultSeverity = $"{BaseUrl}resultSeverity";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Shacl.BaseUrl);
            public readonly static Uri Conforms = new(Shacl.Conforms);
            public readonly static Uri ResultMessage = new(Shacl.ResultMessage);
            public readonly static Uri ResultSeverity = new(Shacl.ResultSeverity);
        }

        public class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);
            public readonly static UriNode Conforms = new(Uris.Conforms);
            public readonly static UriNode ResultMessage = new(Uris.ResultMessage);
            public readonly static UriNode ResultSeverity = new(Uris.ResultSeverity);
        }
    }

    public struct Prov
    {
        public const string BaseUrl = "http://www.w3.org/ns/prov#";

        public const string WasDerivedFrom = $"{BaseUrl}wasDerivedFrom";
        public const string WasGeneratedBy = $"{BaseUrl}wasGeneratedBy";
        public const string AtLocation = $"{BaseUrl}atLocation";
        public const string GeneratedAtTime = $"{BaseUrl}generatedAtTime";
        public const string HadMember = $"{BaseUrl}hadMember";
        public const string WasAssociatedWith = $"{BaseUrl}wasAssociatedWith";
        public const string Used = $"{BaseUrl}used";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Prov.BaseUrl);

            public readonly static Uri WasDerivedFrom = new(Prov.WasDerivedFrom);
            public readonly static Uri WasGeneratedBy = new(Prov.WasGeneratedBy);
            public readonly static Uri AtLocation = new(Prov.AtLocation);
            public readonly static Uri GeneratedAtTime = new(Prov.GeneratedAtTime);
            public readonly static Uri HadMember = new(Prov.HadMember);
            public readonly static Uri WasAssociatedWith = new(Prov.WasAssociatedWith);
            public readonly static Uri Used = new(Prov.Used);
        }

        public class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);

            public readonly static UriNode WasDerivedFrom = new(Uris.WasDerivedFrom);
            public readonly static UriNode WasGeneratedBy = new(Uris.WasGeneratedBy);
            public readonly static UriNode AtLocation = new(Uris.AtLocation);
            public readonly static UriNode GeneratedAtTime = new(Uris.GeneratedAtTime);
            public readonly static UriNode HadMember = new(Uris.HadMember);
            public readonly static UriNode WasAssociatedWith = new(Uris.WasAssociatedWith);
            public readonly static UriNode Used = new(Uris.Used);
        }
    }
}

