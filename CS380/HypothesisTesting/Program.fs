
open CS380
open CS380.VotingRules

open System
open System.Threading.Tasks;

let constrain (x:double, lower:double, upper:double) =
    Math.Min(Math.Max(x, lower), upper);

let r() =
    Rand.NextDouble();

// Hypothesis testing of ENI (Effective Number of Issues)

let numberOfParties = 7
let electorateCount = 120
let voteCount = 500
let simCount = 10

// We set the limits on ENI to between 1 and numberOfParties-1
let sampleENI() =
    1.0 + (r() * (double)(numberOfParties - 2))

let sampleRandVector() =
    let seq1 = Seq.toList(seq { for i in 1 .. numberOfParties do yield r() })
    seq { for x in seq1 do yield x }

let sampleUnitVector() =
    let seq1 = sampleRandVector()
    let sum = Seq.sum(seq1)
    seq { for x in seq1 do yield x / sum }

let sumOfSquares s =
    Seq.sum(seq { for x in s do yield x * x });

let calcEni s =
    1.0 / sumOfSquares(s)

let calcEntropy s =
    Seq.sum(seq { for x in s do yield if x = 0.0 then 0.0 else x * Math.Log(x) });

let normalize s =
    let total = Seq.sum(s)
    seq { for x in s do yield x / total }

let normalizeDistance s =
    let dist = sumOfSquares(s)
    let result = seq { for x in s do yield x / sqrt(dist) } |> Seq.toArray
    let dist2 = sumOfSquares(result)
    result

let abs (x:float) =
    Math.Abs(x)

let sampleVectorWithENIUncertainty = 0.0000001;

let sampleVectorWithProperty eni prFunc normFunc =
    let rec evolve (x:seq<float>) =
        let oldEni = prFunc x
        let currentError = abs(oldEni - eni)
        let slowFactor = 1.0 / currentError
        let transformVector = seq { for x in sampleRandVector() do yield x + slowFactor } |> Seq.toArray
        let candidate = Seq.zip x transformVector |> Seq.map (fun((a,b)) -> a * b) |> normFunc

        let newEni = prFunc candidate

        if (abs(newEni-eni) < abs(oldEni-eni)) then
            if abs(newEni - eni) < sampleVectorWithENIUncertainty then
                candidate
            else
                evolve candidate
        else
            evolve x

    let start = normFunc(sampleRandVector())
    Seq.sort(evolve start)

let sampleVectorWithENI eni =
    sampleVectorWithProperty eni calcEni normalize

let sampleVectorWithEntropy entropy =
    sampleVectorWithProperty entropy calcEntropy normalize

let sampleVectorWithSum sum =
    sampleVectorWithProperty sum Seq.sum normalizeDistance

//let rec sampleVectorWithENI eni =
//    let seq1 = Seq.toList(sampleRandVector())
//    let sum = Seq.sum(seq1);
//    let seqNorm = seq { for x in seq1 do yield x / sum };
//    let sumOfSquares = Seq.sum(seq { for x in seqNorm do yield x * x });
//    let actualENI = 1.0 / sumOfSquares;
//
//    if (Math.Abs(actualENI - eni) < 0.1) then
//        seqNorm
//    else
//        sampleVectorWithENI (eni)

// Transforms the entire set of real numbers to the range 0 to 1
let invLogistic x =
    let y = Math.Exp(x)
    y / (1.0 + y)

// Transforms the entire set of real numbers to valid ENI values.
let transformToENISpace x =
    let y = invLogistic (x)
    1.0 + y * (float)(numberOfParties - 3)

// Given a function float->float, we estimate it's root (i.e. argument that makes it return zero)
// However the function has ***random error***, i.e. so when comparing comparisons with it we can only be sure with some probability that it is correct.
// Use Newton Raphson with slight modifications.

let newtonRaphsonUncertainty = 0.0001;
let estimateRoot f =
    let rec evolve x count =        // x is the estimation of the parameter. count is how many times recently we "did badly" estimating. The higher count is, the slower we progress
        let delta = r()
        let valueAtX = f(x)
        let valueAtXd = f(x+delta)
        let derivative = valueAtXd - valueAtX
        let slowingCoefficient = 1.0 / (1.0 + Math.Pow((float)count, 2.0))
        let candidateX = x - (slowingCoefficient * (x/derivative))
        let valueAtCandidate = f(candidateX)

        System.Console.Write("Error ")
        System.Console.Write(valueAtX)
        System.Console.Write(" ")
        System.Console.WriteLine(valueAtCandidate)

        if (abs(valueAtCandidate)<abs(valueAtX)) then
            System.Console.WriteLine("Keep")
            let newCount = if valueAtCandidate * valueAtX < 0.0 then count + 1 else 0

            if abs(candidateX) < newtonRaphsonUncertainty then
                candidateX
            else
                evolve candidateX newCount
        else
            System.Console.WriteLine("Discard")
            evolve x (count + 1)
    evolve 1.0 0

let rec doENISimulations rule z =
    let results : SimulationResults array = Array.zeroCreate(simCount)
    let sqr = seq { for x in z do yield x * x }
    let ss = Seq.sum(sqr)
    let effectiveENI = 1.0 / ss;
    let zSum = Seq.sum(z);

    ignore (Parallel.For (0, simCount, (fun (iteration:int) ->
        let society = new SpatialArtificialSociety()
        society.PartyCount <- numberOfParties;
        society.Dimensions <- Seq.toArray(z);
        society.ElectorateCount <- electorateCount;
        society.DistrictMagnitude <- 1;
        society.SetupElection();
        let districts = seq { for index in 1..society.ElectorateCount do yield society.SampleElectorate(voteCount)}
        let situation = new VotingSituation();
        situation.PartyCount <- numberOfParties;

        let processDistrict d =
            let ev = new ElectorateVotes();
            ev.Magnitude <- 1;
            ev.VoteCounts <- d;
            ev;

        situation.SetElectorates(seq { for district in districts do yield processDistrict(district) });
        let simResults = SimulationResults.ComputeSimulation(situation, rule)
        results.[iteration] <- simResults;
        ignore null )))    

    if System.Double.IsNaN(Seq.average((seq { for x in results do yield x.EffectiveNumberOfPCAVars }))) then
        // We got NAN for our answer. Ugh.
        // Try again.
        doENISimulations rule z
    else
        results

// Find the ENI given ENV
let solveForENV env =
    let rule = new STVVotingRule();

    let eniFunc (eni:float) = 
        //Console.Write("Estimate ")
        //Console.WriteLine(eni)

        let z = sampleVectorWithENI eni
        let results = doENISimulations rule z
        Seq.average((seq { for x in results do yield x.EffectiveNumberOfPCAVars })) - eni

    let rootFunc x = 
        x |> transformToENISpace |> eniFunc

    let root = estimateRoot rootFunc
    let eniEstimate = root |> transformToENISpace
    eniEstimate

// Find the ENI given ENV
let solveForProperty env prFun lower upper =
    let rule = new FPPVotingRule();

    let rec solve lowerLimit upperLimit =
        Console.WriteLine (lowerLimit.ToString() + " " + upperLimit.ToString());

        // Generate some ENI values randomly.
        let targetCandidates = seq { for x in 1..50 do yield ((r() * (upperLimit-lowerLimit))+lowerLimit) } |> Seq.toArray

        // Generate values of Z with ENI close to our target ENI
        let zSeq = seq { for eniTarget in targetCandidates do yield sampleVectorWithENI(eniTarget) } |> Seq.toArray

        // Find out the actual ENI of the Z values. (Note we can't quickly generate Z exactly equal to our target)
        let candidateENI = seq { for z in zSeq do yield prFun z } |> Seq.toArray

        // Do many simulations for each ENI value.
        let simResults = seq { for z in zSeq do yield doENISimulations rule z } |> Seq.toArray

        // Calculate mean ENV for each ENI value.
        let candidateENV = seq { for results in simResults do yield Seq.average(seq { for simRun in results do yield simRun.EffectiveNumberOfPCAVars }) }
        let mappedResults = Seq.zip candidateENI candidateENV |> Seq.toArray
        let sortedResults = mappedResults |> Seq.sortBy(fun (a,b) -> abs(b - env))
        let bestResults = sortedResults |> Seq.take 10

        let resultMean = seq { for (a, b) in bestResults do yield a } |> Seq.average
        let resultSD = seq { for (a, b) in bestResults do yield Math.Pow(a-resultMean,2.0) } |> Seq.average |> Math.Sqrt
        let resultSE = resultSD / Math.Sqrt((float)(bestResults |> Seq.length))
        let newLower = constrain(resultMean - (resultSE * 1.96), 1.0, ((float)(numberOfParties-1)))
        let newUpper = constrain(resultMean + (resultSE * 1.96), 1.0, ((float)(numberOfParties-1)))

        if resultSE < 0.1 then
            resultMean
        else
            solve newLower newUpper
    solve lower upper

let findEniForEnv env =
    solveForProperty env calcEni 1.0 ((float)(numberOfParties-1))

//let hypothesisTest1 = 
//    let eni = sampleENI()
//    Console.Write("eni ");
//    Console.WriteLine(eni);
//
//    let strm = System.IO.File.Open("enisim.csv", System.IO.FileMode.OpenOrCreate)
//    strm.SetLength((int64)0)
//    let report = new CSVReportWriter<SimulationResults>(new CSVWriter(strm))
//
//    report.AddColumn("Loosemorehanby", fun a -> a.LoosemoreHanbyIndex.ToString())
//    report.AddColumn("Gallagher", fun a -> a.GallagherIndex.ToString())
//    report.AddColumn("EffectiveNumberOfParties", fun a -> a.EffectiveNumberOfParties.ToString())
//    report.AddColumn("EffectiveNumberOfPCAVars", fun a -> a.EffectiveNumberOfPCAVars.ToString())
//    report.AddColumn("Governability", fun a -> a.Governability.ToString())
//    report.AddColumn("Round", fun a -> a.Properties.["zIndex"])
//    report.AddColumn("ENI", fun a -> a.Properties.["ENI"])
//    report.AddColumn("Entropy", fun a -> a.Properties.["Entropy"])
//    report.AddColumn("SpatialSum", fun a -> a.Properties.["Sum"])
//
//    //let zSeq = seq { for i in 1..10 do yield sampleVectorWithENI (eni) |> Seq.toArray } |> Seq.toArray
//    //let zSeq = seq { for i in 1..10 do yield sampleVectorWithEntropy (-1.4) |> Seq.toArray } |> Seq.toArray
//    let zSeq = seq { for i in 1..10 do yield sampleVectorWithSum (1.1) |> Seq.toArray } |> Seq.toArray
//
//    let rule = new FPPVotingRule();
//    
//    let indicies = seq { for i in 1..Seq.length(zSeq) do yield i } |> Seq.toArray
//    for (z,index) in Seq.zip zSeq indicies do
//        let results = doENISimulations rule z
//        
//        let average:double = Seq.average((seq { for x in results do yield x.EffectiveNumberOfPCAVars }))
//
//        for result in results do
//            result.Properties.Add("zIndex", index.ToString())
//            result.Properties.Add("ENI", (calcEni z).ToString())
//            result.Properties.Add("Entropy", (calcEntropy z).ToString())
//            result.Properties.Add("Sum", (Seq.sum z).ToString())
//            report.WriteLine (result)
//        Console.WriteLine(average)
//
//    report.Close()
//    printfn "Test Complete"


let simulatedElectionCount = 1000

let simulateElectionsByModel2 (societyGen:unit->ArtificialSocietyGenerator) (ruleFactory:unit->VotingRule) (filename:string) (districtCount:int) =
    let strm = System.IO.File.Open(filename, System.IO.FileMode.OpenOrCreate)
    strm.SetLength((int64)0)
    let report = new CSVReportWriter<SimulationResults>(new CSVWriter(strm))

    report.AddColumn("lijphart", fun a -> a.LijphartIndex.ToString())
    report.AddColumn("raes", fun a -> a.RaesIndex.ToString())
    report.AddColumn("loosemorehanby", fun a -> a.LoosemoreHanbyIndex.ToString())
    report.AddColumn("gallagher", fun a -> a.GallagherIndex.ToString())
    report.AddColumn("enp", fun a -> a.EffectiveNumberOfParties.ToString())
    report.AddColumn("pca", fun a -> a.EffectiveNumberOfPCAVars.ToString())
    report.AddColumn("governability", fun a -> a.Governability.ToString())
    report.AddColumn("entropy", fun a -> a.EntropyIndex.ToString())
    report.AddColumn("entropyseatprop", fun a -> a.EntropySeatPropIndex.ToString())

    let performSimulation() =
        let society = societyGen()
        society.PartyCount <- numberOfParties;
        society.ElectorateCount <- districtCount;

        society.SetupElection();
        let districts = seq { for index in 1..society.ElectorateCount do yield society.SampleElectorate(voteCount)}
        let situation = new VotingSituation();
        situation.PartyCount <- numberOfParties;

        let processDistrict d =
            let ev = new ElectorateVotes();
            ev.Magnitude <- society.DistrictMagnitude;
            ev.VoteCounts <- d;
            ev;

        situation.SetElectorates(seq { for district in districts do yield processDistrict(district) });

        let rule = ruleFactory()
        let simResults = SimulationResults.ComputeSimulation(situation, rule)
        simResults

    let results : SimulationResults array = Array.zeroCreate(simulatedElectionCount)

    ignore (Parallel.For (0, simulatedElectionCount, (fun (iteration:int) ->
        results.[iteration] <- performSimulation()
        System.Console.WriteLine(iteration)
        )))

    for result in results do
        report.WriteLine(result)

    report.Close()
    System.Console.Write("Test ")
    System.Console.Write(filename)
    System.Console.WriteLine(" Complete")

let simulateElectionByModel (societyGen:unit->ArtificialSocietyGenerator) (ruleFactory:unit->VotingRule) (filename:string) =
    simulateElectionsByModel2 societyGen ruleFactory filename (electorateCount / societyGen().DistrictMagnitude)

let isValidDistrictMagnitude(dm:int) =
    let districtCount:int = electorateCount / dm
    let districtCount2 = districtCount * dm;
    (districtCount2 = electorateCount)

[<EntryPoint>]
let main args = 
    printfn "Hypothesis Testing"
//    //printfn "Test solve for env"
//
//    let ukSpatial = SpatialFile.FromStream("ukspatial.csv")
//    let ukENV = ukSpatial.GetENV()
//    //ukSpatial.DoTest()
//
//    let eniEstimate = findEniForEnv ukENV
//    printfn "Estimate complete"
//    Console.WriteLine(eniEstimate) //6.21 5.60207931966914 6.08108588550783
//    //hypothesisTest1

    //let districtMagnitudes = [| 1; 2; 3; 4; 5; 6; 8; 10; 12; 15; 20; 30; 40; 60; 120 |]

    //let districtMagnitudes = [| 10; 12; 15 |]

    let stv() = new STVVotingRule() :> VotingRule
    let fpp() = new FPPVotingRule() :> VotingRule

    let pd() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.StLague
        rule :> VotingRule

    let nzSpatial = SpatialFile.FromStream("nzspatial.csv")
    let nzPref = PreferenceFile.FromStream("nzpref2.csv");

    //let nzDistricts = seq { for d in nzSpatial.Districts do yield d } |> Seq.toArray

    let districtMagnitudes = seq { 1..electorateCount } |> Seq.where(isValidDistrictMagnitude) |> Seq.toArray;

    for dm in districtMagnitudes do
        let prefSwapSocietyFactory() : ArtificialSocietyGenerator =
            let society = new PreferenceSwappingArtificialSociety()
            society.DistrictMagnitude <- dm;
            society.PreferenceInformation <- nzPref;
            society.SpatialInformation <- nzSpatial;
            society.MaxChanceSwap2 <- 0.18;
            society.MaxChanceSwap3 <- 0.12;
            society :> ArtificialSocietyGenerator

        simulateElectionsByModel2 prefSwapSocietyFactory stv ("nzprefswap_" + dm.ToString() + "_stv.csv") (electorateCount / dm)

//    for dm in districtMagnitudes do
//        let nzDistricts = nzSpatial.Redistrict(electorateCount / dm) |> Seq.toArray
//        let nzPref = seq { for d in nzDistricts do yield nzPref.InferPreferences(d) } |> Seq.toArray
//        let nzMultisets = seq { for p in nzPref do yield p.ToMultiset(voteCount) } |> Seq.toArray
//
//        let situation = VotingSituation.FromInfo(nzMultisets, nzSpatial.PartyCount, dm)
//
//        let simResults = SimulationResults.ComputeSimulation(situation, stv())
//        ignore 0

//
//    let sm() =
//        let rule = new SupplementaryMemberVotingRule()
//        rule.Apportionment <- ApportionmentMethod.StLague
//        rule.TotalSeats <- electorateCount
//        rule :> VotingRule

//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.EnableMajorLeftRight <- true;
//            society.Dimensions <- [| 1.0; 1.0; |]
//            society.DistrictMagnitude <- 1;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel2 spatialSocietyFactory sm ("spatial2_" + dm.ToString() + "_sm.csv") (electorateCount-dm)

    let pdHamilton() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.Hamilton
        rule :> VotingRule

//    let pdHill() =
//        let rule = new ProportionalByDistrictVotingRule()
//        rule.Apportionment <- ApportionmentMethod.Hill
//        rule :> VotingRule
//
//    let pdJefferson() =
//        let rule = new ProportionalByDistrictVotingRule()
//        rule.Apportionment <- ApportionmentMethod.Jefferson
//        rule :> VotingRule
//
//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.EnableMajorLeftRight <- false;
//            society.Dimensions <- [| 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel spatialSocietyFactory stv ("spatial2_pure_" + dm.ToString() + "_stv.csv")
//
//////
//
//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; 1.0; 1.0; 1.0; 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel2 spatialSocietyFactory stv ("spatial6_" + dm.ToString() + "_stv.csv") (electorateCount / dm)
//

//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel2 spatialSocietyFactory stv ("spatial2_" + dm.ToString() + "_stv.csv") (electorateCount / dm)

//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel spatialSocietyFactory stv ("spatial3_" + dm.ToString() + "_stv.csv")
//
//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel spatialSocietyFactory stv ("spatial1_" + dm.ToString() + "_stv.csv")

//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel spatialSocietyFactory pd ("spatial2_" + dm.ToString() + "_pd.csv")
//
//    for dm in districtMagnitudes do
//        let spatialSocietyFactory() :ArtificialSocietyGenerator =
//            let society = new SpatialArtificialSociety()
//            society.Dimensions <- [| 1.0; 1.0; |]
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//        simulateElectionsByModel2 spatialSocietyFactory pd ("apportionmentStLague_" + dm.ToString() + "_stv.csv") (electorateCount / dm)
//        simulateElectionsByModel2 spatialSocietyFactory pdHamilton ("apportionmentHamilton_" + dm.ToString() + "_stv.csv") (electorateCount / dm)
//        simulateElectionsByModel2 spatialSocietyFactory pdHill ("apportionmentHill_" + dm.ToString() + "_stv.csv") (electorateCount / dm)
//        simulateElectionsByModel2 spatialSocietyFactory pdJefferson ("apportionmentJefferson_" + dm.ToString() + "_stv.csv") (electorateCount / dm)

//    for dm in districtMagnitudes do
//        let urnSocietyFactory() : ArtificialSocietyGenerator =
//            let society = new UrnArtificialSociety()
//            society.AlphaGenerator <- fun () ->
//                let beta = r()
//                beta / (1.0 - beta)
//
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel urnSocietyFactory stv ("urn_" + dm.ToString() + "_stv.csv")

//    for dm in districtMagnitudes do
//        let urnSocietyFactory() : ArtificialSocietyGenerator =
//            let society = new UrnArtificialSociety()
//            society.AlphaGenerator <- fun () ->
//                let beta = r()
//                beta / (1.0 - beta)
//
//            society.DistrictMagnitude <- dm;
//            society :> ArtificialSocietyGenerator
//
//        simulateElectionsByModel urnSocietyFactory pd ("urn_" + dm.ToString() + "_pd.csv")

    Console.WriteLine("Complete")
    let a =  Console.ReadLine()
    0 // return an integer exit code

