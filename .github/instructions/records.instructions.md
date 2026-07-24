---
description: Local agent guidance for the records library.
applyTo: "**"
---

# Records agent instructions

## Backend constraint
- Do not parse or transform RDF with dotNetRDF in the records library to compensate for backend gaps or protocol mismatches.
- Keep large-record handling backend-driven; avoid introducing dotNetRDF parsing outside the dedicated dotNetRDF record backend.
