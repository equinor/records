using VDS.RDF;

namespace Records.Utils;

public static class TimeUtils
{
    public static List<Triple?> CreateHasTimeTriples(INode timeHaver, DateTime dateTime)
    {
        var blankNode = new BlankNode(Guid.NewGuid().ToString());

        var gYear = Namespaces.Time.UriNodes.GetYearLiteralNode(dateTime);
        var gregMonth = Namespaces.Greg.UriNodes.GetGregorianMonthUriNode(dateTime);
        var gDay = Namespaces.Time.UriNodes.GetDayLiteralNode(dateTime);

        return
        [
            new(timeHaver, Namespaces.Time.UriNodes.HasTime, blankNode),
            new(blankNode, Namespaces.Rdf.UriNodes.Type, Namespaces.Time.UriNodes.DateTimeDescription),
            new(blankNode, Namespaces.Time.UriNodes.Year, gYear),
            new(blankNode, Namespaces.Time.UriNodes.Month, gregMonth),
            new(blankNode, Namespaces.Time.UriNodes.Day, gDay)
        ];
    }
}
