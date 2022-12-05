# Motivation

## Use Cases
[Records](format.md) are intended to make exchange of RDF safer and easier. The design has arised when working with facility design data, but we expect it to be useful for many cases, f.ex.:

* Management of change for heterogenous datasets of arbitrary interconnectedness
* Storage and interchange of data within or between large organizations
* Multi-disciplinary analysis of data across domain verticals

Other cases will probably not benefit, or even be hindered, by using records, f.ex.:

* Performance sensitive handling of large datasets of small, well-known, fixed schema of low interconnectedness
* Matrix data
* Sensor streams
* Ephemeral data (i.e. data which is no longer needed after consumption)
Specifically, use cases with high-velocity append-only data is probably not a good match. 

## Alternatives to Records
Two existing, tried approaches to exchanging RDF are full graph exchange and triple assertion exchange: a (change)set of triples each either inserted or deleted.

### Full Graph Exchange
* Semantically portable: no context is assumed, the exchanged graph can be utilized by anyone at any time, not only the intended recipient at the time of exchange.
* Inflexible: In order to communicate a small change to a large graph, the whole graph must be transmitted. For a given size of graph and rate of change this can prove cost-prohibitive. Access policies may also prevent sharing of the whole graph.
* Concurrent editing of the graph is not possible: Only one process can have the write-lock at the graph at a time.

### Triple Assertion Exchange
* Flexible and small: Changesets only contain the actual changes (however small) to the graph (however large). Changesets are trivially combined and decomposed.
* Less portable: Although semantic technology captures some context, not all of it can be captured, and the remaining context is only available in the intended graph to which it applies, or together with an ordered series of the previous changesets. 
* Concurrent editing of the graph is possible, but very few safety measures are possible unless single-user write-lock is applied.

We present [RDF Record](format.md) as a middle ground between full graph- and triple assertion exchange, providing portability of exchanged sub-graphs (Records) combined with the (somewhat limited) flexibility and prudence of communicating change by transmitting (and thus "replacing") only the changed sub-graphs.

## Design Goals

* Ease of integration with various domain specific information systems at their native level of composition for domain objects (1 updated domain object -> 1 record)
* Immutable history (a Record is never deleted, only ever superseeded by a new Record)
* Transparency of provenance (a Record's history and "chain of custory" can be traced back to the origin)
* Branching and merging of record histories
* Portability of partial graphs (i.e. a Record can be useful in contexts where it's history or sub-/superrecords are neither known nor needed).
  
As a practical matter, since this ontology divides the world into archival objects (Records and their provenance) and domain objects (whatever is in the Record-graphs), we had to formalize a relation between Records and the domain they record. This can be summarized in an additional design goal:

* Provide a framework for placing individual records in their specific domain scope and list the domain objects they describe. This is intended to provide consumers with building blocks to index and retreive records in the language of the domain at hand.