#! /usr/bin/env -S dotnet fsi

#r "nuget: FSharp.Data, 8.1.14"

#load "./provider.fsx"

open Provider
open System.IO

let getAllData =
    let json = System.IO.File.ReadAllText "./data.json"
    Provider.Day.Parse json

let getDataForDay day parsed =
    parsed |> Array.filter (fun (x: Provider.Day.Root) -> x.Day = day)

let dayExists a =
    match a with
    | [| _ |] -> true
    | _ -> false

let formatBlock label (data: obj) sym =
    let header = $"{sym} {label}: "

    let lines =
        data.ToString().Split([| "\n"; "\r\n" |], System.StringSplitOptions.None)

    match lines with
    | [| single |] -> header + single
    | many ->
        let indented = many |> Array.map (fun l -> $"{sym}   {l}") |> String.concat "\n"
        header + "\n" + indented

let generateFiles (d: Provider.Day.Root) =
    let extensionData = [ "exs", "#"; "fsx", "//" ]
    let dayStr = $"day-{d.Day}"

    extensionData
    |> List.iter (fun (ext, commentSym) ->
        let dir = $"./{dayStr}"
        let filePath = $"{dir}/main.{ext}"

        let contents =
            $"""{commentSym} Day: {d.Day}
{commentSym} Title: {d.Title}
{commentSym} Description: {d.Description}
{formatBlock "Input" d.Input commentSym}
{formatBlock "Output" d.Output commentSym}"""

        Directory.CreateDirectory dir |> ignore

        if File.Exists filePath then
            printfn $"Skipping {filePath}, nothing to do."
        else
            File.WriteAllText(filePath, contents))

let main () =
    let args = fsi.CommandLineArgs |> Array.tail

    match args with
    | [| arg |] ->
        let _, day = System.Int32.TryParse arg
        let dayData = getAllData |> getDataForDay day

        if dayData |> dayExists then
            dayData |> Array.head |> generateFiles
        else
            printfn "No such day."

    | _ -> printfn "No arguments given."

main ()
