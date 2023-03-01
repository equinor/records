prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

select *
where{
    graph ?record {
        ?revision rev:describes <IRI of the revisioned thing/document> .
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