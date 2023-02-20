select *
where{
    graph ?record {
        ?revision rev:describes ex:DocumentId .
        optional {
            ?revision rev:isNewRevisionOf ?oldRevision .
        }
        ?revision rev:containsRecord ?containedRecord .
    }
    filter not exists{
        graph ?newerRecord {
            ?newerRecord rec:replaces ?record .
        }
    }
    graph ?containedRecord {
        ?s ?p ?o .
    }
}