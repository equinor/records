# Example SPARQL queries
These are queries over the data in [comments.trig]() and [revisions.trig]()

The first two queries assume that all relevant data is put into the default un-named named graph, for example some kind of "head" graph of all not replaced records. 
They also assume that [../schema/comment.ttl]() is loaded into this un-named graph

## Query 1
Get all status updates in the newest revision. Include comment if exists
```sparql
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
```
### Example answers
<table>
<tr><td>status_name	   </td><td> object	      </td><td>      comment_text	                 </td><td>           author
<tr><td>Out of scope	</td><td> ex:tagNo20PG123N</td><td>	Too complicated pump, find smaller model	</td><td>Trude Luth
<tr><td>Resubmit	</td><td>    ex:tagNo20PG123O	</td><td>	                                    </td><td>    Trude Luth
<tr><td>Resubmit	</td><td>    ex:tagNo20PG123P	</td><td>	                                    </td><td>    Trude Luth
</td></tr></table>

## Query 2
 Get all comments on a specific revision

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT distinct ?object ?comment ?author ?property WHERE {
    ?reply a rev:Reply;
        rev:describes exdoc:B123-EX-W-LA-00001.F01;
        rev:hasComment ?c.
    ?c rev:describes ?object ;
        rdfs:label ?comment ;
        rev:issuedBy ?author.
    OPTIONAL {
        ?c rev:aboutProperty ?property.
    }
}
```

### Example answers 
<table>
<tr>
<td>object	     </td><td>           comment	                                  </td><td>                   author	     </td><td>   property
</tr>
<tr>
<td>ex:tagNo20PG123N</td><td>	    Too complicated pump, find smaller model	        </td><td>         Trude Luth
</tr>
<tr>
<td>ex:tagNo20PG123O</td><td>	    This number seems too round, is it really exactly 2000?</td><td>	Kari Nordkvinne	  </td><td>  exRds:weight_in_kgs
</tr>
<tr>
<td>ex:tagNo20PG123O	    </td><td>This number seems too round, is it really exactly 2000?</td><td>	Kari Nordkvinne	 </td><td>   exRds:weight_in_tons
</tr>
<tr>
<td>ex:tagNo20PG124O</td><td>	    This number seems too round, is it really exactly 2000?</td><td>	Kari Nordkvinne	 </td><td>   exRds:weight_in_kgs
</tr>
<tr>
<td>ex:tagNo20PG124O	  </td><td>  This number seems too round, is it really exactly 2000?</td><td>	Kari Nordkvinne	  </td><td>  exRds:weight_in_tons
</tr>
<tr>
<td>ex:tagNo20PG123NMass</td><td>	This is too heavy, please find a smaller version	   </td><td>      Ola Nordmann
</tr>
</table>


## Query 3
This query gets the status of the newest revisions for all documents
Just to show it is possible, this query does not assume materialization into a "head" graph, but in stead assumes the records are in their own named graphs, as in the example files.
It also assumes that the comment ontology is in the un-named graph

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT distinct ?document ?newest_revision_name ?status_name ?reply_name ?reply_date ?comment_responsible WHERE {
    GRAPH ?record1 {
        ?reply a rev:Reply, ?status;
            rev:describes ?revision;
            rdfs:label ?reply_name;
            prov:generatedAtTime ?reply_date;
            rev:issuedBy ?comment_responsible.
    }
    GRAPH ?record2 {
        ?revision rdfs:label ?newest_revision_name;
            rev:describes ?document.
    }
    FILTER NOT EXISTS {
        GRAPH ?record3 {
            ?newRevision rev:replaces ?revision.
        }
    }
    ?status rdfs:subClassOf rev:RevisionState;
        rdfs:label ?status_name.
}
```
### Example answers
<table>
<tr>
<td>document</td>	<td>newest_revision_name</td> 	<td>status_name</td>	<td>reply_name</td><td>reply_date</td>	<td>comment_responsible</td>
</tr>
<tr>
<td> exdoc:B123-EX-W-LA-00001</td>	<td>First delivered revision of MEL</td>	<td>Reviewed</td>	<td>Reply to revision F02</td>	<td>2023-06-15</td>	<td>Turi Skogen</td>
</tr>
</table>


## Query 4
This query gets all comments on an object (from any revisions and replies). Here we again assume all relevant data is materialized to the unnamed graph. 

```sparql
prefix rec: <https://rdf.equinor.com/ontology/record/>
prefix rev: <https://rdf.equinor.com/ontology/revision/>

SELECT distinct ?document ?revision ?comment_text ?author ?comment_responsible ?date WHERE {
    ?reply a rev:Reply;
        rev:issuedBy ?comment_responsible;
        rev:describes ?revision;
        rev:hasComment ?comment.
    ?comment a rev:Comment;
        rdfs:label ?comment_text;
        rev:issuedBy ?author;
        prov:generatedAtTime ?date;
        rev:describes exdata:tagNo20PG123N.
    ?revision rev:describes ?document.
}
```
### Example answers
<table>
<tr>
<td>document</td>	<td>revision</td> 	<td>comment_text</td>	<td>author</td>	<td>comment_responsible</td><td>date</td>
</tr>
<tr>
<td> exdoc:B123-EX-W-LA-00001</td>	<td>exdoc:B123-EX-W-LA-00001.F01</td>	<td>Too complicated pump, find smaller model</td>	<td>Trude Luth</td>		<td>Turi Skogen</td> <td>2023-06-15</td>
</tr>
</table>