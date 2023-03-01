# Record 
This is a C# library for handling and creating Records as [described in the official documentation](https://github.com/equinor/records/blob/main/doc/format.md).

The main parts of this library are:
## Immutable.Record
An immutable Record is a record that which loaded can not be changed. It has been verifed to a certain set of rules, and exposes content of the record via helper methods.

## Mutable.Record
A mutable Record may be changed. There is no guarantee that a mutable Record is a valid Record. It is easy to add new content to it via helper methods.
You may turn a mutable Record into an immutable Record which will force it to be validated.

## RecordBuilder
A Record builder helps you in building an immutable Record from scratch. It will not build if it does not have all the content which is required for an immutable Record to be created.
