# Record format
Records are intended to make exchange of RDF safer and easier. The design has arised when working with facility design data, but we expect it to be useful for many cases. Specifically, use cases with high-velocity append-only data is probably not a good match. 

Two existing, safe, approaches to exchanging RDF is to either exchange lists of individual triples to deleted and inserted, or to always exchange the full graph. Both these approaches have valid use cases, but have major disadvantages in those we work with. We claim that records are useful for any situation that is not handled well by these two extremes.

## Record Format Summary  
Records encapsulate an immutable list of triples (an RDF graph). We call this graph the 'content' of the record.
Records are implemented as a named graph, and the identity of the record is the IRI of the named graph.
The contents of a record are immutable by agreement and specification. The triples in the named graph in a record are of two types, content and the 'provenance'. 
The schema is formalized in [../schema/record.ttl](record.ttl) and [../schema/record.shacl](record.shacl).

## Namespaces
* rec: https://rdf.equinor.com/ontology/record/
* prov: http://www.w3.org/ns/prov#
## Provenance
The provenance graph is the subgraph of the record named graph which is reachable from the IRI of the named graph which stops whenever a resource which is not of type Record is encountered. The rest of the graph is the content.
We require these triples in the provenance graph to have a special meaning and only be used in the provenance
    * The record is of type rec:Record
    * The record is related to at most one other record with the relation rec:isSubRecordOf. (But any number of records in the other direction) 
    * * The record is related to at least one resource with rec:isInScope (possibly indirectly through rec:isSubRecordOf). These resources are called scopes.
    * The record is related to any number of resources with rec:describes. 

### Scopes
The intention of the 'scopes' is to make explicit the scope in which the content of the record is valid. If there are several scopes, the content is valid in all of them. This implies an intersection-type semantics for scope. 

### Describes
The intention of 'describes' is an inventory of what the content of the record is describing. This may be the same as the subjects in many of the triples in the content, but the exact mapping between describes and content is not specified by this format (and does not need to be specified, more detail below)

### Example in trig
For example, assuming :Object/Record0 already exists, the trig below represents a valid record:
```
@prefix : <http://example.com/data/> .
@prefix vocab: <http://example.com/vocab/> .
@prefix rec: <https://rdf.equinor.com/ontology/record/> .

:Object1/Record1 {
    :Object1 a vocab:System.
    :Object1/Record1 a rec:Record;
        rec:describes :Object1;
        rec:isInScope :Project;
        rec:replaces :Object1/Record0.
}
 ```
### Splitting up records
An important functionality of records is the ability to change the size, or granularity of the records. For example, assume that the Object1 has been split up into two objects, that we wish to keep in separate records, then the following trig file is a valid as a transaction into the triplestore with the record above: 

```
@prefix : <http://example.com/data/> .
@prefix vocab: <http://example.com/vocab/> .
@prefix rec: <https://rdf.equinor.com/ontology/record/> .

:Object1/Record2 {
    :Object1 a vocab:System.
    :Object1/Record2 a rec:Record;
        rec:describes :Object1;
        rec:isInScope :Project;
        rec:replaces :Object1/Record1.
}

:Object2/Record0 {
    :Object2 a vocab:System.
    :Object2/Record0 a rec:Record;
        rec:describes :Object2;
        rec:isInScope :Project;
        rec:replaces :Object1/Record0.
}
 ```
 ### Transactions
 The fact that both these records are related with rec:replaces to the same record is fine if they are sent together. The format does not specify further how to specify that objects are sent together, as all cases will need a solid transaction concept anyway to ensure correct transfer.

 If the records above arrived separately, this would represents an error in the record history that cannot be fixed. We will call this type of error a merge conflict.

### Subrecords
Records can be related via the relation rec:isSubRecordOf. Scopes are inherited by subrecords. That is, if record 1 is a subrecord of record 2, then any member of rec:isInScope of record 2 is also the scope of record 1.

### Head and describe
For any set of records, we define the 'head' to be those records that are not the object of any rec:replaces triple. The intention is that the 'head' is the set of triples that have not been replaces by other (usually newer)  records.
In any store of records, there can in the head never be any two records that have identical set of scopes and overlapping describes. It is allowed, and even useful in most use cases, to have overlapping describes in different sets of scopes. But for a given set of scopes, if two records in the head overlap in the describes, this indicates a type of merge conflict that is not allowed.

This is the only rule that the record format puts on the describe elements. It is, for example, allowed that two records related with rec:replaces have completely different set of describes.
