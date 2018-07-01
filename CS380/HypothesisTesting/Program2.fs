
open CS380
open CS380.VotingRules

open System
open System.Threading.Tasks;

let constrain (x:double, lower:double, upper:double) =
    Math.Min(Math.Max(x, lower), upper);

let r() =
    Rand.NextDouble();

let numberOfParties = 8
let electorateCount = 120
let voteCount = 500
let simulatedElectionCount = 2000

let sampleRandVector() =
    let seq1 = Seq.toList(seq { for i in 1 .. numberOfParties do yield r() })
    seq { for x in seq1 do yield x }

let sampleUnitVector() =
    let seq1 = sampleRandVector()
    let sum = Seq.sum(seq1)
    seq { for x in seq1 do yield x / sum }

let sumOfSquares s =
    Seq.sum(seq { for x in s do yield x * x });

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
    printfn "CS380 Hypothesis Testing"

    let stv() = new STVVotingRule() :> VotingRule
    let fpp() = new FPPVotingRule() :> VotingRule

    let pdHamilton() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.Hamilton
        rule :> VotingRule

    let pdHill() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.Hill
        rule :> VotingRule

    let pdJefferson() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.Jefferson
        rule :> VotingRule

    let pd() =
        let rule = new ProportionalByDistrictVotingRule()
        rule.Apportionment <- ApportionmentMethod.StLague
        rule :> VotingRule

    
    let sm() =
        let rule = new SupplementaryMemberVotingRule()
        rule.Apportionment <- ApportionmentMethod.StLague
        rule.TotalSeats <- electorateCount
        rule :> VotingRule

    let districtMagnitudes = seq { 1..electorateCount } |> Seq.where(isValidDistrictMagnitude) |> Seq.toArray;

    // Spatial Model

    for dm in districtMagnitudes do
        let spatialSocietyFactory() :ArtificialSocietyGenerator =
            let society = new SpatialArtificialSociety()
            society.EnableMajorLeftRight <- true;
            society.Dimensions <- [| 1.0; 1.0; |]
            society.DistrictMagnitude <- 1;
            society :> ArtificialSocietyGenerator

        simulateElectionsByModel2 spatialSocietyFactory sm ("spatial2_" + dm.ToString() + "_sm.csv") (electorateCount-dm)

    for dm in districtMagnitudes do
        let spatialSocietyFactory() :ArtificialSocietyGenerator =
            let society = new SpatialArtificialSociety()
            society.EnableMajorLeftRight <- true;
            society.Dimensions <- [| 1.0; 1.0; |]
            society.DistrictMagnitude <- 1;
            society :> ArtificialSocietyGenerator

        simulateElectionsByModel2 spatialSocietyFactory stv ("spatial2_" + dm.ToString() + "_stv.csv") (electorateCount-dm)

    // Urn Model

    for dm in districtMagnitudes do
        let urnSocietyFactory() : ArtificialSocietyGenerator =
            let society = new UrnArtificialSociety()
            society.AlphaGenerator <- fun () ->
                let beta = r()
                beta / (1.0 - beta)

            society.DistrictMagnitude <- dm;
            society :> ArtificialSocietyGenerator

        simulateElectionsByModel2 urnSocietyFactory stv ("urn_" + dm.ToString() + "_stv.csv") (electorateCount-dm)

    // Preference Swapping Model
    let nzSpatial = SpatialFile.FromStream("nzspatial.csv")
    let nzPref = PreferenceFile.FromStream("nzpref2.csv");

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

    Console.WriteLine("Simulation Complete")

    0 // return an integer exit code

