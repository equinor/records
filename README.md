# records
Description, schema and libraries for the "record" format for exchanging RDF

* [Format description](doc/format.md)
* [Motivation](doc/motivation.md)
* [Ontologies, schema, etc](schema/)
* [A generator of very simple records](src/RecordGenerator)
* [Queries and code for working with records in fuseki](src/fuseki)
* [Setup, datalog and queries for working with records in RDFOx](src/rdfox)


## CI/CD For Records .NET package
The CI/CD on this repository is done through Github Actions. The action [publish.yml](./.github/workflows/publish.yml) describes how it works.

Every pull request trigger a myriad of actions that check the quality of the code being pushed. Unit tests, integration tests, formatting, linting, and SQL migration are amongst the things being run / tested. You may find a full list in the [workflows](./.github/workflows/) folder.

Every push to the `main` branch result in a publication of the records nuget package.

## Commits
Commits into the `main` branch contain the message of the pull request it originated from.

## Issues
We handle issues using Azure DevOps. You may find [our backlog here](https://dev.azure.com/EquinorASA/Spine/_backlogs/backlog/Semantic%20Infrastucture/Stories) if you have the proper access.

## Releases
Our releases uses [semver](https://semver.org/).

## Tooling
For the code base we use .NET in C#. 

For testing we use `XUnit`.