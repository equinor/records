// Get all status updates in the newest revision
// Include comment if exists

prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT ?status_name ?comment_text ?author WHERE {
    ?r a rev:Revision.
    FILTER NOT EXISTS {
        ?new rev:replaces ?r.
    }
    ?reply a rev:Reply;
        rev:describes ?r;
        rev:hasStatus ?s.
    ?s a ?status;
        rev:describes ?object;
        rev:issuedBy ?author.
    ?status rdfs:subClassOf rev:RevisionState;
            rdfs:label ?status_name .
    OPTIONAL {
        ?s rev:hasComment ?comment.
        ?comment rdfs:label ?comment_text.
    }
}

//Example answers
status_name	    object	                                comment_text	                            author
Out of scope	ex:tagNo20PG123N	Too complicated pump, find smaller model	Trude Luth
Resubmit	    ex:tagNo20PG123O		                                        Trude Luth
Resubmit	    ex:tagNo20PG123P		                                        Trude Luth



// Get all comments on a specific revision

prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT distinct ?object ?comment ?author ?property WHERE {
    ?reply a rev:Reply;
        rev:describes document:B123-EX-W-LA-00001.F01;
        rev:hasComment ?c.
    ?c rev:describes ?object ;
        rdfs:label ?comment ;
        rev:issuedBy ?author.
    ?status rdfs:subClassOf rev:RevisionState.
    OPTIONAL {
        ?c rev:aboutProperty ?property.
    }
}

//Example answers
object	                comment	                                                    author	        property
ex:tagNo20PG123N	    Too complicated pump, find smaller model	                Trude Luth	
ex:tagNo20PG123O	    This number seems too round, is it reaally exactly 2000?	Kari Nordkvinne	exRds:weight_in_kgs
ex:tagNo20PG123O	    This number seems too round, is it reaally exactly 2000?	Kari Nordkvinne	exRds:weight_in_tons
ex:tagNo20PG124O	    This number seems too round, is it reaally exactly 2000?	Kari Nordkvinne	exRds:weight_in_kgs
ex:tagNo20PG124O	    This number seems too round, is it reaally exactly 2000?	Kari Nordkvinne	exRds:weight_in_tons
ex:tagNo20PG123NMass	This is too heavy, please find a smaller version	        Ola Nordmann	

