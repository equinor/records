using FluentAssertions;
using Records.Utils;
using VDS.RDF;

namespace Records.Tests;

public class C14NTests
{
    [Fact]
    public void BlankNodesAreNotMergedInCanonicalisation()
    {
        var originalGraph = new Graph();
        originalGraph.LoadFromString("""
                                                 _:RK5U8KTX5f20255f025fReport5fActualAccumulatedCost <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/AccumulatedCost> .
                                     _:RK5U8KTX5f20255f025fReport5fActualAccumulatedCost <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "4.253173875E7"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fActualAccumulatedCost <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fActualPeriodicCost <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/PeriodicCost> .
                                     _:RK5U8KTX5f20255f025fReport5fActualPeriodicCost <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "869464.62"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fActualPeriodicCost <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedAdjustments <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedAdjustments <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "1.224510674E7"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedAdjustments <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedVO <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedVO <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "5.772778904E7"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fApprovedVO <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fEAC <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fEAC <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "0.0"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fEAC <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fExecutedOptions <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fExecutedOptions <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "0.0"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fExecutedOptions <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fGrowth <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fGrowth <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "0.0"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fGrowth <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fOriginalContractValue <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fOriginalContractValue <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "8.42497124E7"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fOriginalContractValue <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fPendingVORs <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fPendingVORs <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "0.0"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fPendingVORs <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     _:RK5U8KTX5f20255f025fReport5fPotentialVORs <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Cost> .
                                     _:RK5U8KTX5f20255f025fReport5fPotentialVORs <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount> "4760177.68"^^<http://www.w3.org/2001/XMLSchema#float> .
                                     _:RK5U8KTX5f20255f025fReport5fPotentialVORs <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency> <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .
                                     <https://assetid.equinor.com/contract/12345678910> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/asset/Contract> .
                                     <https://assetid.equinor.com/contract/12345678910> <http://www.w3.org/2000/01/rdf-schema#label> "12345678910" .
                                     <https://assetid.equinor.com/contractorsWbs/EX.01234A.001/NBZ8FRS3RT> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/asset/ContractorsCostWbs> .
                                     <https://assetid.equinor.com/contractorsWbs/EX.01234A.001/NBZ8FRS3RT> <http://www.w3.org/2000/01/rdf-schema#label> "NBZ8FRS3RT" .
                                     <https://assetid.equinor.com/contractorsWbs/EX.01234A.001/NBZ8FRS3RT> <https://rdf.equinor.com/asset/contractorsCostWBSDescription> "It's my WBS, is what it is." .
                                     <https://assetid.equinor.com/project/EX.01234A.001> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/asset/Project> .
                                     <https://assetid.equinor.com/project/EX.01234A.001> <http://www.w3.org/2000/01/rdf-schema#label> "EX.01234A.001" .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/asset/Workpack> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <http://www.w3.org/2000/01/rdf-schema#comment> "RK5U8KTX @ EX.01234A.001!" .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <http://www.w3.org/2000/01/rdf-schema#label> "RK5U8KTX" .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <https://rdf.equinor.com/asset/partOfContract> <https://assetid.equinor.com/contract/12345678910> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <https://rdf.equinor.com/asset/partOfProject> <https://assetid.equinor.com/project/EX.01234A.001> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX> <https://rdf.equinor.com/cost/hasReport> <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/cost/Report> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <http://www.w3.org/2000/01/rdf-schema#comment> "This lineitem is lit" .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <http://www.w3.org/2000/01/rdf-schema#label> "Report for RK5U8KTX" .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <http://www.w3.org/2006/time#inXSDgYearMonth> "2025-02"^^<http://www.w3.org/2001/XMLSchema#gYearMonth> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/hasContractorWbs> <https://assetid.equinor.com/contractorsWbs/EX.01234A.001/NBZ8FRS3RT> .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedActualAccumulatedCost> _:RK5U8KTX5f20255f025fReport5fActualAccumulatedCost .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedActualPeriodicCost> _:RK5U8KTX5f20255f025fReport5fActualPeriodicCost .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedApprovedAdjustments> _:RK5U8KTX5f20255f025fReport5fApprovedAdjustments .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedApprovedVariationOrder> _:RK5U8KTX5f20255f025fReport5fApprovedVO .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedEstimateAtComplete> _:RK5U8KTX5f20255f025fReport5fEAC .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedExecutedOptions> _:RK5U8KTX5f20255f025fReport5fExecutedOptions .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedGrowth> _:RK5U8KTX5f20255f025fReport5fGrowth .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedOriginalContractValue> _:RK5U8KTX5f20255f025fReport5fOriginalContractValue .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedPendingVariationOrderRequests> _:RK5U8KTX5f20255f025fReport5fPendingVORs .
                                     <https://assetid.equinor.com/workpack/EX.01234A.001/RK5U8KTX/Report/2025/02> <https://rdf.equinor.com/cost/reportedPotentialVariationOrderRequests> _:RK5U8KTX5f20255f025fReport5fPotentialVORs .
                                     
                                     """);
        var canonicalized = CanonicalisationExtensions.Canonicalise(originalGraph);

        canonicalized.Triples
            .Select(tr => tr.Subject)
            .Distinct()
            .Count().Should().Be(15);
    }
}