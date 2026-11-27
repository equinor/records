# Fuseki Docker image
The following file [Dockerfile](Dockerfile) is used to build the fuseki image for use in testing the record backend. 
It creates an in memory fuseki store. 

It is also built in the integration tests using dotnet testcontainers. 
Because of this the image, and the files referenced by the image must be included in
the solution context.

## Dataset creation and SHACL

`FusekiRecordBackend` creates memory datasets by posting form parameters
`dbName=record_<guid>` and `dbType=mem` to `/$/datasets`. Fuseki 6.3 no longer
accepts uploaded Turtle service configurations, regardless of the former
`fuseki:allowAddByConfigFile` setting.

The image installs [config/config-mem](config/config-mem) at
`$FUSEKI_BASE/templates/config-mem` (`run/templates/config-mem` by default).
Fuseki substitutes `{NAME}` with the requested dataset name. This template
preserves the `sparql`, `update`, `upload`, `data`, and `shacl` endpoints for
every dynamically created dataset.

External Fuseki deployments using this backend must install the same template
before creating datasets. Configuring SHACL on the initial `ds` dataset alone
does not enable it on dynamically created datasets. The stock memory template
does not expose SHACL, and SHACL validation will fail without this customization.

For local development and testing, you can build the local image with the following command:

```bash
docker build -f Dockerfile -t record-fuseki:latest .
docker run -p 3030:3030 record-fuseki:latest
```