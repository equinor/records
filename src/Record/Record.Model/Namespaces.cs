﻿using System.Globalization;
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
        public const string NotConnected = $"{BaseUrl}notConnected";

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
        public const string XsdPrefix = "http://www.w3.org/2001/XMLSchema#";
        public const string String = $"{XsdPrefix}string";
        public const string Decimal = $"{XsdPrefix}decimal";
        public const string Date = $"{XsdPrefix}date";
        public const string HexBinary = $"{XsdPrefix}hexBinary";
        public const string Language = $"{XsdPrefix}language";
        public const string GYear = $"{XsdPrefix}gYear";
        public const string GDay = $"{XsdPrefix}gDay";

        public static class Uris
        {
            public readonly static Uri XsdPrefix = new(DataType.XsdPrefix);
            public readonly static Uri String = new(DataType.String);
            public readonly static Uri Decimal = new(DataType.Decimal);
            public readonly static Uri Date = new(DataType.Date);
            public readonly static Uri HexBinary = new(DataType.HexBinary);
            public readonly static Uri Language = new(DataType.Language);
            public readonly static Uri GYear = new(DataType.GYear);
            public readonly static Uri GDay = new(DataType.GDay);
        }

        public class UriNodes
        {
            public readonly static UriNode XsdPrefix = new(Uris.XsdPrefix);
            public readonly static UriNode String = new(Uris.String);
            public readonly static UriNode Decimal = new(Uris.Decimal);
            public readonly static UriNode Date = new(Uris.Date);
            public readonly static UriNode HexBinary = new(Uris.HexBinary);
            public readonly static UriNode Language = new(Uris.Language);
            public readonly static UriNode GYear = new(Uris.GYear);
            public readonly static UriNode GDay = new(Uris.GDay);
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

    public struct Time
    {
        public const string BaseUrl = "http://www.w3.org/2006/time#";

        public const string HasTime = $"{BaseUrl}hasTime";
        public const string Year = $"{BaseUrl}year";
        public const string Month = $"{BaseUrl}month";
        public const string Day = $"{BaseUrl}day";

        public const string DateTimeDescription = $"{BaseUrl}DateTimeDescription ";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Time.BaseUrl);

            public readonly static Uri HasTime = new(Time.HasTime);
            public readonly static Uri Year = new(Time.Year);
            public readonly static Uri Month = new(Time.Month);
            public readonly static Uri Day = new(Time.Day);

            public readonly static Uri DateTimeDescription = new(Time.DateTimeDescription);
        }

        public static class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);

            public readonly static UriNode HasTime = new(Uris.HasTime);
            public readonly static UriNode Year = new(Uris.Year);
            public readonly static UriNode Month = new(Uris.Month);
            public readonly static UriNode Day = new(Uris.Day);

            public readonly static UriNode DateTimeDescription = new(Uris.DateTimeDescription);

            public static LiteralNode GetYearLiteralNode(DateTime dateTime)
                => new(
                    literal: dateTime.ToString("yyyy", CultureInfo.InvariantCulture),
                    datatype: DataType.Uris.GYear
                    );

            public static LiteralNode GetDayLiteralNode(DateTime dateTime)
                => new(
                    literal: FormatGregorianDayIso8601String(dateTime),
                    datatype: DataType.Uris.GDay
                    );

            private static string FormatGregorianDayIso8601String(DateTime dateTime)
                => $"---{dateTime:dd}";
        }
    }

    public struct Greg
    {
        public const string BaseUrl = "https://www.w3.org/ns/time/gregorian#";

        public const string January = $"{BaseUrl}January";
        public const string February = $"{BaseUrl}February";
        public const string March = $"{BaseUrl}March";
        public const string April = $"{BaseUrl}April";
        public const string May = $"{BaseUrl}May";
        public const string June = $"{BaseUrl}June";
        public const string July = $"{BaseUrl}July";
        public const string August = $"{BaseUrl}August";
        public const string September = $"{BaseUrl}September";
        public const string October = $"{BaseUrl}October";
        public const string November = $"{BaseUrl}November";
        public const string December = $"{BaseUrl}December";

        public static class Uris
        {
            public readonly static Uri BaseUrl = new(Greg.BaseUrl);

            public readonly static Uri January = new(Greg.January);
            public readonly static Uri February = new(Greg.February);
            public readonly static Uri March = new(Greg.March);
            public readonly static Uri April = new(Greg.April);
            public readonly static Uri May = new(Greg.May);
            public readonly static Uri June = new(Greg.June);
            public readonly static Uri July = new(Greg.July);
            public readonly static Uri August = new(Greg.August);
            public readonly static Uri September = new(Greg.September);
            public readonly static Uri October = new(Greg.October);
            public readonly static Uri November = new(Greg.November);
            public readonly static Uri December = new(Greg.December);
        }

        public static class UriNodes
        {
            public readonly static UriNode BaseUrl = new(Uris.BaseUrl);

            public readonly static UriNode January = new(Uris.January);
            public readonly static UriNode February = new(Uris.February);
            public readonly static UriNode March = new(Uris.March);
            public readonly static UriNode April = new(Uris.April);
            public readonly static UriNode May = new(Uris.May);
            public readonly static UriNode June = new(Uris.June);
            public readonly static UriNode July = new(Uris.July);
            public readonly static UriNode August = new(Uris.August);
            public readonly static UriNode September = new(Uris.September);
            public readonly static UriNode October = new(Uris.October);
            public readonly static UriNode November = new(Uris.November);
            public readonly static UriNode December = new(Uris.December);

            public static UriNode GetGregorianMonthUriNode(DateTime dateTime)
                => GetGregorianMonthUriNode(dateTime.ToString("MMMM", CultureInfo.InvariantCulture));

            public static UriNode GetGregorianMonthUriNode(string monthName)
            {
                return monthName.ToLower() switch
                {
                    "january" => January,
                    "february" => February,
                    "march" => March,
                    "april" => April,
                    "may" => May,
                    "june" => June,
                    "july" => July,
                    "august" => August,
                    "september" => September,
                    "october" => October,
                    "november" => November,
                    "december" => December,
                    _ => throw new ArgumentException("Not a valid month."),
                };
            }
        }
    }
}
