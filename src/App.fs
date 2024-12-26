// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Timer'
open System
open System.Text.RegularExpressions
open Fermata.ParserCombinators

module App =
    let keyboardshortcut (e: KeyboardEvent) =
        match document.activeElement.id with
        | "commandInput" ->
            match e.key with
            | "Escape" -> (document.getElementById "commandInput").blur ()
            | _ -> ()
        | _ ->
            let helpWindow = document.getElementById "helpWindow"

            let isHelpWindowActive =
                helpWindow.classList
                |> (fun x -> JS.Constructors.Array?from(x))
                |> Array.contains "active"

            let informationPolicyWindow = document.getElementById "informationPolicyWindow"

            let isInformationPolicyWindowActive =
                informationPolicyWindow.classList
                |> (fun x -> JS.Constructors.Array?from(x))
                |> Array.contains "active"

            match e.key with
            | "Escape" ->
                if isHelpWindowActive then
                    helpWindow.classList.remove "active"

                if isInformationPolicyWindowActive then
                    informationPolicyWindow.classList.remove "active"

                if not isHelpWindowActive && not isInformationPolicyWindowActive then
                    stop ()
            | "Enter" -> start ()
            | "Delete" -> reset ()
            | "Backspace" ->
                if e.altKey then
                    reset ()
            | "?" ->
                if not isHelpWindowActive then
                    helpWindow.classList.add "active"
            | "\\" ->
                (document.getElementById "commandInput").focus ()
                e.preventDefault ()
            | _ -> ()

    window.addEventListener (
        "DOMContentLoaded",
        (fun _ ->
            // timer feature
            let commandInput = document.getElementById "commandInput" :?> HTMLInputElement

            // real-time validation and preview
            commandInput.oninput <-
                fun _ ->
                    if commandInput.value = "" then
                        (document.getElementById "validationArea").innerHTML <- ""
                        document.body.setAttribute ("style", """color: ""; background-color: "";""")
                        (document.getElementById "messageArea").innerText <- ""
                        (document.getElementById "timerArea").innerHTML <- ""

                    // real-time validation
                    let validated = commandInput.value |> split |> Array.map parse

                    if
                        Array.forall
                            (fun x ->
                                match x with
                                | Ok _ -> true
                                | Error _ -> false)
                            validated
                    then
                        // all commands are valid.
                        (document.getElementById "validationArea").innerHTML <- ""

                        validated
                        |> Array.head
                        |> fun x ->
                            match x with
                            | Ok(v, _) ->
                                let v' = Command2.build' v

                                document.body.setAttribute (
                                    "style",
                                    ($"""color: %s{Command2.color v'}; background-color: %s{Command2.background v'};""")
                                )

                                (document.getElementById "messageArea").innerText <- Command2.message v'

                                match v' with
                                | Command2.Down v ->
                                    (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay v.Duration
                                | Command2.Up _ ->
                                    (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay TimeSpan.Zero
                            | Error _ -> ()
                    else if
                        // some of the input commands are invalid.
                        Array.length validated > 1
                    then
                        validated
                        |> Fermata.Array.tryFore
                        |> function
                            | None -> ()
                            | Some v ->
                                let msg =
                                    v
                                    |> Array.indexed
                                    |> Array.filter (fun (_, x) ->
                                        match x with
                                        | Error _ -> true
                                        | Ok _ -> false)
                                    |> Array.map (fun (i, x) ->
                                        match x with
                                        | Error(e, Parsers.State(s, p)) -> $"%d{i + 1} つ目: %s{s}"
                                        //| Error(e, Parsers.State(s, p)) -> $"%d{i + 1}: %s{s}, %s{e} at %d{p + 1}"
                                        | Ok _ -> "")
                                    |> String.concat "<br>"
                                    |> (+) "以下のコマンドに誤りがあります。<br>"

                                printfn "%s" msg
                                (document.getElementById "validationArea").innerHTML <- msg

            (document.getElementById "inputArea").onsubmit <-
                fun e ->
                    e.preventDefault ()
                    commandInput.blur ()

                    let tmp: Result<(Parsing.CommandAndOptions * Parsers.State), (string * Parsers.State)> array =
                        commandInput.value
                        |> fun x -> Regex.Split(x, "(?=down \d|up \d)")
                        |> Array.map _.Trim()
                        |> Array.filter (String.IsNullOrWhiteSpace >> not)
                        |> Array.map (fun x -> Parsers.State(x, 0))
                        |> Array.map Parsing.command

                    if
                        Array.exists
                            (fun x ->
                                match x with
                                | Ok _ -> false
                                | Error _ -> true)
                            tmp
                    then
                        printfn "Input was invalid."

                    tmp
                    |> Array.map (fun x ->
                        match x with
                        | Ok(v, _) -> Command2.build' v
                        | Error _ -> Command2.Down(Command2.defaultDown))
                    |> Array.toList
                    |> List.map string
                    |> List.iter (printfn "%s")

                    //commandInput.value |> splitInput' |> Array.map parse |> Array.toList |> start
                    start ()

            let startButton = (document.getElementById "startButton")
            startButton.title <- "開始 (Enter)"
            startButton.onclick <- fun _ -> start ()
            let pauseButton = (document.getElementById "pauseButton")
            pauseButton.title <- "一時停止 (Escape)"
            pauseButton.onclick <- fun _ -> stop ()
            let stopButton = (document.getElementById "stopButton")
            stopButton.title <- "停止 (Delete)"
            stopButton.onclick <- fun _ -> reset ()
            let helpButton = (document.getElementById "helpButton")
            helpButton.title <- "ヘルプ (?)"

            let bellButton = (document.getElementById "bellButton")

            bellButton.onclick <-
                fun _ ->
                    let audio = document.getElementById "bell" :?> HTMLAudioElement
                    audio.currentTime <- 0
                    audio.play ()

            // help window
            [ "helpButton"; "helpClose" ]
            |> List.iter (fun x ->
                (document.getElementById x :?> HTMLButtonElement).onclick <-
                    fun _ ->
                        (document.getElementById "helpWindow").classList.toggle "active" |> ignore
                        ())

            // information policy window
            (document.getElementById "informationPolicyLink").onclick <-
                fun event ->
                    event.preventDefault ()
                    (document.getElementById "informationPolicyWindow").classList.add "active"

            (document.getElementById "informationPolicyClose").onclick <-
                fun _ -> (document.getElementById "informationPolicyWindow").classList.remove "active"

            // keyboard shortcut
            document.onkeydown <- fun (e: KeyboardEvent) -> keyboardshortcut e)
    )
