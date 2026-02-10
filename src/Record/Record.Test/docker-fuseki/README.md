# Fuseki Docker image
The following file [Dockerfile](Dockerfile) is used to build the fuseki image for use in testing the record backend. 
It creates an in memory fuseki store. 

It is also built in the integration tests using dotnet testcontainers. 
Because of this the image, and the files referenced by the image must be included in
the solution context.

For local development and testing, you can build the local image with the following command:

```bash
docker build -f Dockerfile -t record-fuseki:latest .
docker run -p 3030:3030 record-fuseki:latest
```