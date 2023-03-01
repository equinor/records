# Record format
Records are intended to make exchange of RDF safer and easier. More details on motivation and background in [motivation.md](motivation.md)

## Record Format Summary  
Records encapsulate an immutable list of triples (an RDF graph). We call this graph the 'content' of the record.
Records are implemented as a named graph, and the identity of the record is the IRI of the named graph.
The contents of a record are immutable by agreement and specification. The triples in the named graph in a record are of two types, content and the 'provenance'. 
The schema is formalized in [record-syntax.ttl](../schema/record-syntax.ttl) and [record-syntax.shacl](../schema/record-syntax.shacl). Further, experimental, axioms are in [record-rules.ttl](../schema/record-rules.ttl)

## Namespaces
* rec: https://rdf.equinor.com/ontology/record/
* prov: http://www.w3.org/ns/prov#
* ex: http://example.com/data/ 

## Provenance
The provenance graph is the subgraph of the record named graph which is reachable from the IRI of the named graph which stops whenever a resource which is not of type Record is encountered. The rest of the graph is the content.
We require these triples in the provenance graph to have a special meaning and only be used in the provenance
* The record is of type rec:Record
* The record is related to at most one other record with the relation rec:isSubRecordOf. That is, rec:isSubRecordOf is functional, but not inverse functional. The subrecord relation is used to avoid duplication.
* The record is related to at least one resource with rec:isInScope (possibly indirectly through rec:isSubRecordOf). These resources are called scopes.
* The record is related to any number of resources with rec:describes. 
* The record is related to any number of records with rec:replaces. In other words it is a many-to-many relation.

### Scopes
The intention of the 'scopes' is to make explicit the scope in which the content of the record is valid. If there are several scopes, the content is valid in all of them. This implies an intersection-type semantics for scope. 

### Describes
The intention of 'describes' is an inventory of what the content of the record is describing. The minimum requirement is that any IRI in describes is also mentioned in the content of the record, and that the any resource in the content graph is reachable from at least one element in describes using only edges in the content. Most stores will probably have stronger rules on describes, f.ex. that the elements in describes are the roots of certain trees in the content.

### Example in trig
For example, this is a valid record with exactly one triple in the content:
```ttl
ex:Object1/Record0 {
    ex:Object1 a ex:System.
    ex:Object1/Record0 a rec:Record;
        rec:describes ex:Object1;
        rec:isInScope ex:Project.
}
 ```
and after the record above is sent, this is a new valid record
 ```ttl
ex:Object1/Record1 {
    ex:Object1 a ex:System;
                rdfs:label "System 1";
                ex:hasSubSystem ex:Object2, ex:Object3.
    ex:Object2 a ex:SubSystem.
    ex:Object3 a ex:SubSystem.
    ex:Object1/Record1 a rec:Record;
        rec:describes ex:Object1;
        rec:isInScope ex:Project.
        rec:replaces ex:Object1/Record0.
}
 ```
## Subrecords
Records can be related via the relation rec:isSubRecordOf. Scopes are inherited by subrecords. That is, if record 1 is a subrecord of record 2, then any member of rec:isInScope of record 2 is also the scope of record 1.

### Example: Using subrecord to split up records
An important functionality of records is the ability to change the size, or granularity of the records. For example, assume that the Object1 has been split up into two objects, that we wish to keep in separate records, then the following records are valid into the triplestore with the records above if they come in the same transaction:

```ttl
:Object1/Record2 {
    ex:Object1 a ex:System ;
                rdfs:label "System 1" .
    :Object1/Record2 a rec:Record ;
        rec:describes ex:Object1 ;
        rec:isInScope ex:Project ;
        rec:replaces ex:Object1/Record1.
}
```
```ttl
ex:Object2/Record0 {
    ex:Object2 a ex:SubSystem;
                rdfs:label "Subsystem 2" .
    ex:Object1 ex:hasSubSystem ex:Object2 .
    :Object2/Record0 a rec:Record ;
        rec:describes ex:Object2 ;
        rec:isSubRecordOf ex:Object1/Record2 .
}
 ```
 
```ttl
ex:Object3/Record0 {
    ex:Object3 a ex:SubSystem ;
                rdfs:label "Subsystem 3" .
    ex:Object1 ex:hasSubSystem ex:Object3 .
    :Object3/Record0 a rec:Record ;
        rec:describes ex:Object3 ;
        rec:isSubRecordOf ex:Object1/Record2 .
}
 ```
### Head and describe
For any set of records, we define the 'head' to be those records that are not the object of any rec:replaces triple. The intention is that the 'head' is the set of records that have not been replaced by other (usually newer)  records.
In any store of records, there can in the head never be any two records that have identical set of scopes and overlapping describes. It is allowed, and even useful in most use cases, to have overlapping describes in different sets of scopes. But for a given set of scopes, if two records in the head overlap in the describes, this indicates a type of merge conflict that is not allowed.

As long as the rule above is followed, it is allowed that two records related with rec:replaces have completely different set of describes.

### Concurrency and Subrecords
We assume that records are sent via a medium that has transaction support, f.ex. TCP. The subrecord relation can only be stated explicitly for records that are received in the same transaction.

Scope inheritance from the superrecord propagates with replaces. That is, if ex:SubSystem subrecord of ex:System and ex:System2 replaces ex:System, and ex:System2 and ex:Subsystem are both in head, then the scopes of ex:System2 are also scopes of ex:Subsystem. 
This is useful to change the scopes of large numbers of records.
### Querying on scopes
When querying over a set of records, it will usually be with a filter over scopes. There are two different ways to filter over scopes. We expect the domain-oriented queries to be using an inclusive filtering, that is, include any record that has at least the given scopes. For exploring the history or provenance it will also be beneficial with queries that use precise scope queries, that is, give me all the records that have exactly these scopes.

### Modelling pattern for revisions and versions
Some work processes have their own concepts of revisions and versions. These patterns might in some cases be modelled directly by records, but only in the cases where such a revision or version can never be changed. In the cases where revisions and versions can be changed, it is better to model the revisions as a separate concept. 
`rev:Revision` is a quite general concept for any revision/edition of data. It must have at least one `rev:containsRecord` relation to a record, and at most one `rev:isNewRevisionOf` relation to a previous revision. Note that revisions are objects in the content, even though they have relations to records. Files:

*  [Revision ontology](../schema/revision.ttl)
*  [Shacl rules](../schema/revision.shacl)
*  [Example](../example/revisions.trig)
  
There is currently only one subclass of `rev:Revision` for revisions of documents. Revisions of documents have author and dates. The can also have the relation `rev:describes` to the document it revises. Note that this is not the same relation as `rec:describes` in the record provenance. Document revisions are intended to support existing workflows where documents have editions/revisions/versions that are issued at certain times.
