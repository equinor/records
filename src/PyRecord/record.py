from rdflib import *

class RecordBuilder:
    id = None
    scopes = None
    describes = None
    content = None

    def __init__(self):
        self.scopes = list()
        self.describes = list()

    def with_id(self, id):
        if isinstance(id, URIRef): self.id = id
        elif isinstance(id, str): self.id = URIRef(id)
        else: raise ValueError("ID must be string or URIRef.")
        return self
    
    def with_scope(self, scope):
        if isinstance(scope, URIRef): self.scopes.append(scope)
        elif isinstance(scope, str): self.scopes.append(URIRef(scope))
        else: raise ValueError("Scope must be string or URIRef.")
        return self

    
    def build(self):
        record = Dataset()
        record.add((URIRef(self.id), URIRef("http://example.com/a"), URIRef("http://rdf.equinor.com/record/Record"), URIRef(self.id)))

        for scope in self.scopes:
            record.add((URIRef(self.id), URIRef("https://example.com/isInScope"), scope, URIRef(self.id)))

        record_string = record.serialize(format="nquads")
        return Record(record_string)


class Record:
    id = None
    scopes = None
    describes = None
    graph = None

    def __init__(self, rdf):
        self.store = Dataset()
        self.store.parse(data=rdf, format="nquads")
        graphs = list(self.store.graphs())

        if len(graphs) != 2:
            raise ValueError("Found more than one named graph.")
        
        self.graph = [g for g in graphs if g.identifier != URIRef("urn:x-rdflib:default")][0]
        self.id = self.graph.identifier

    def provenance(self):
        return [(str(s), str(p), str(o)) for (s, p, o) in self.graph if s == self.id]
    
    
    def content(self):
        return [(str(s), str(p), str(o)) for (s, p, o) in self.graph if s != self.id]

    def __str__(self):
        return self.store.serialize(format="nquads")
        

def test():
    record = RecordBuilder().with_id("https://example.com/id/1").with_scope("https//example.com/scope/1").with_scope("https//example.com/scope/2").build()
    print(str(record))
    print(record.id)
    print(record.provenance())
    

if __name__ == "__main__":
    test()