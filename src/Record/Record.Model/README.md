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

## Fuseki 6.3 compatibility

The Fuseki backend uses the configured `HttpClient` to POST `dbName=record_<guid>`
and `dbType=mem` to `/$/datasets`. Fuseki 6.3 no longer accepts the Turtle
configuration uploads used by older `Record` packages. Upgrade the package and
Fuseki deployment together; the URL and authentication setup do not change.

Stock Fuseki 6.3 does not expose `/shacl` on dynamically created memory datasets.
If you use server-side SHACL, your Fuseki operator must bundle a
[SHACL-enabled memory template](https://github.com/equinor/records/blob/main/src/Record/Record.Test/docker-fuseki/config/config-mem)
at `$FUSEKI_BASE/templates/config-mem` in the image or deployment automation.
Configuring SHACL on a startup dataset alone is not enough. This is a server-side
requirement, not a file each Records client installs. A missing SHACL endpoint
fails validation; the library does not switch validators or skip the check.
