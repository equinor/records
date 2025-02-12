# records
Description, schema and libraries for the "record" format for exchanging RDF
testing
* [Format description](doc/format.md)
* [Motivation](doc/motivation.md)
* [Ontologies, schema, etc](schema/)
* [A generator of very simple records](src/RecordGenerator)
* [Queries and code for working with records in fuseki](src/fuseki)
* [Setup, datalog and queries for working with records in RDFOx](src/rdfox)


## CI/CD For Records .NET package
The CI/CD on this repository is done through Github Actions. The action [publish.yml](./.github/workflows/publish.yaml) describes how it works.

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

## Contributing
Please follow these steps to contribute:

Fork the repository on GitHub. Clone your fork and create a new branch for your feature or bugfix. Commit your changes to the new branch. Push your changes to your fork. Open a pull request from your fork to the original repository. Please ensure that your code follows the existing style and structure, and add tests where necessary.
