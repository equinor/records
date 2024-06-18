# Record 
This is a C# library for handling and creating Records as [described in the official documentation](https://github.com/equinor/records/blob/main/doc/format.md).

The main parts of this library are:
## Immutable.Record
An immutable Record is a record that which loaded can not be changed. It has been verifed to a certain set of rules, and exposes content of the record via helper methods.

## Mutable.Record (Obsolete)
A mutable Record may be changed. There is no guarantee that a mutable Record is a valid Record. It is easy to add new content to it via helper methods.
You may turn a mutable Record into an immutable Record which will force it to be validated. This functionality is obsolete, 
as the functionality can be given directly from the Graph endpoint on Immutable.Record

## RecordBuilder
A Record builder helps you in building an immutable Record from scratch. It will not build if it does not have all the content which is required for an immutable Record to be created.

## ProvenanceBuilder
A ProvenanceBuilder helps you in building the provenance on the record content. It is used in conjunction with the RecordBuilder.
