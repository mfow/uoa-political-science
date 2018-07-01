// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open System.Collections.Generic
open CS380

[<EntryPoint>]
let main argv = 
    printfn "%A" argv

    let f () =
        new List<float>()

    let a = SimulationEngine()
    
    let b = f()
    
    //a.MyProperty <- f

    0 // return an integer exit code
