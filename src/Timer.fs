// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open System
open System.Text.RegularExpressions
open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Command2
open WakeLockAPI
open Fermata.ParserCombinators

module Timer' =
    type RunningStatus =
        | NotStarted = 0
        | Running = 1
        | Stopping = 2
        | Finished = 4

    type TimeAcc = { StartTime: DateTime; Acc: TimeSpan }

    type Commands =
        { Input: Command2 list
          Remaining: Command2 list }

    type State =
        { Stop: TimeAcc
          Current: TimeAcc
          IntervalId: int
          Commands: Commands
          WakeLock: JS.Promise<obj> option
          RunningStatus: RunningStatus }

    [<Emit("setInterval($0, $1)")>]
    let setInterval (callback: unit -> unit) (interval: int) : int = jsNative

    [<Emit("clearInterval($0)")>]
    let clearInterval (intervalID: int) : unit = jsNative

    let mutable state =
        { Stop =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          Current =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          IntervalId = -1
          Commands = { Input = []; Remaining = [] }
          WakeLock = None
          RunningStatus = RunningStatus.NotStarted }

    let initState =
        { Stop =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          Current =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          IntervalId = -1
          Commands = { Input = []; Remaining = [] }
          WakeLock = None
          RunningStatus = RunningStatus.NotStarted }

    let timeSpanToDisplay (timeSpan: TimeSpan) =
        $"""%02d{timeSpan.Hours}:%02d{timeSpan.Minutes}:%02d{timeSpan.Seconds}<span class="decimals">.%03d{timeSpan.Milliseconds}</span>"""

    let elapsedTime (command: Command2) (startTime: DateTime) (acc: TimeSpan) (t: DateTime) : TimeSpan =
        let passed = t - startTime + acc

        match command with
        | Command2.Down v -> v.Duration - passed
        | Command2.Up _ -> passed

    let split (input: string) : string array =
        input
        |> fun x -> Regex.Split(x, "(?=down \d|up \d)")
        |> Array.map _.Trim()
        |> Array.filter (String.IsNullOrWhiteSpace >> not)

    let parse (input: string) =
        Parsing.command (Parsers.State(input, 0))

    let validate input =
        let tmp = input |> split |> Array.map parse

        if
            Array.exists
                (fun x ->
                    match x with
                    | Ok _ -> false
                    | Error _ -> true)
                tmp
        then
            Error tmp
        else
            Ok tmp

    let stop () =
        match state.RunningStatus with
        | RunningStatus.Running ->
            clearInterval state.IntervalId

            let now = DateTime.Now

            state <-
                { state with
                    Stop.Acc = state.Stop.Acc + (now - state.Stop.StartTime)
                    Current.Acc = state.Current.Acc + (now - state.Current.StartTime)
                    RunningStatus = RunningStatus.Stopping }
        | _ -> ()

    let start () =
        match state.RunningStatus with
        | RunningStatus.NotStarted
        | RunningStatus.Finished ->
            (document.getElementById "timerArea").classList.remove "finished"
            (document.getElementById "messageArea").classList.remove "finished"

            match (document.getElementById "commandInput" :?> HTMLInputElement).value |> validate with
            | Error xs ->
                let msg =
                    xs
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
            | Ok xs ->
                (document.getElementById "validationArea").innerHTML <- ""

                let commands: Command2 list =
                    xs
                    |> Array.map (fun x ->
                        match x with
                        | Ok(v, _) -> Command2.build' v
                        | Error _ -> Command2.Down(Command2.defaultDown) //never comes here.
                    )
                    |> Array.toList

                state <-
                    { initState with
                        Stop =
                            { initState.Stop with
                                StartTime = DateTime.Now
                                Acc = TimeSpan.Zero }
                        Current =
                            { initState.Current with
                                StartTime = DateTime.Now
                                Acc = TimeSpan.Zero }
                        Commands =
                            { Input = commands
                              Remaining = commands }
                        WakeLock =
                            if WakeLockAPI.isSupported () then
                                printfn $"locking at %s{DateTime.Now.ToString()}"

                                try
                                    WakeLockAPI.lock () |> Some
                                with _ ->
                                    None
                            else
                                printfn "failed to lock..."
                                None
                        RunningStatus = RunningStatus.Running }

                let outputArea = document.getElementById "outputArea"

                match state.WakeLock with
                | Some x ->
                    if not x?released then
                        printfn $"locked at %s{DateTime.Now.ToString()}"
                        outputArea.innerText <- "画面起動ロック API により、タイマー動作中は画面がスリープしません。"
                | None ->
                    printfn "failed to lock ...."
                    outputArea.innerText <- ""

                let intervalId =
                    setInterval
                        (fun _ ->
                            match state.Commands.Remaining with
                            | [] ->
                                (document.getElementById "timerArea").classList.add "finished"
                                (document.getElementById "messageArea").classList.add "finished"

                                match state.WakeLock with
                                | Some x ->
                                    printfn $"releasing at %s{DateTime.Now.ToString()}"
                                    WakeLockAPI.release x
                                | None -> printfn "doing nothing..."

                                state <-
                                    { state with
                                        WakeLock = None
                                        RunningStatus = RunningStatus.Finished }

                                (document.getElementById "outputArea").innerText <- ""

                                clearInterval state.IntervalId
                            | h :: t ->
                                let now = DateTime.Now
                                let passedTime = now - state.Current.StartTime + state.Current.Acc

                                // Checking if the current command has come to its end.
                                if passedTime > Command2.duration h then
                                    // The current command has come to its end.
                                    state <-
                                        { state with
                                            Commands = { state.Commands with Remaining = t }
                                            Current = { StartTime = now; Acc = TimeSpan.Zero } }

                                    state.Commands.Remaining |> (printfn "%A")

                                    // Displaying time.
                                    match h with
                                    | Command2.Down _ -> TimeSpan.Zero
                                    | Command2.Up v -> v.Duration
                                    |> timeSpanToDisplay
                                    |> fun x -> (document.getElementById "timerArea").innerHTML <- x

                                    // Playing alarm.
                                    match alarm h with
                                    | "bell"
                                    | "beep" as x ->
                                        let audio = document.getElementById x :?> HTMLAudioElement
                                        audio.currentTime <- 0
                                        audio.play ()
                                    | _ -> ()

                                    // Pausing.
                                    if Command2.shouldPause h then
                                        // Skipping the pause option with the last command.
                                        if List.length t > 0 then
                                            stop ()
                                else
                                    // The current command has NOT come to its end.
                                    let elapsedTime' = elapsedTime h state.Current.StartTime state.Current.Acc now
                                    (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay elapsedTime'

                                    match h with
                                    | Command2.Down v -> (v.Color, v.Background, v.Message)
                                    | Command2.Up v -> (v.Color, v.Background, v.Message)
                                    |> fun (color, bgcolor, message) ->
                                        document.body.setAttribute (
                                            "style",
                                            (sprintf "color: %s; background-color: %s;" color bgcolor)
                                        )

                                        (document.getElementById "messageArea").innerText <- message)
                        10

                state <- { state with IntervalId = intervalId }
        | RunningStatus.Stopping ->
            state <-
                { state with
                    Stop.StartTime = DateTime.Now
                    Current.StartTime = DateTime.Now
                    RunningStatus = RunningStatus.Running }

            let intervalId =
                setInterval
                    (fun _ ->
                        match state.Commands.Remaining with
                        | [] ->
                            (document.getElementById "timerArea").classList.add "finished"
                            (document.getElementById "messageArea").classList.add "finished"

                            match state.WakeLock with
                            | Some x ->
                                printfn $"releasing at %s{DateTime.Now.ToString()}"
                                WakeLockAPI.release x
                            | None -> printfn "doing nothing..."

                            state <-
                                { state with
                                    WakeLock = None
                                    RunningStatus = RunningStatus.Finished }

                            (document.getElementById "outputArea").innerText <- ""

                            clearInterval state.IntervalId
                        | h :: t ->
                            let now = DateTime.Now
                            let passedTime = now - state.Current.StartTime + state.Current.Acc

                            // Checking if the current command has come to its end.
                            if passedTime > Command2.duration h then
                                // The current command has come to its end.
                                state <-
                                    { state with
                                        Commands = { state.Commands with Remaining = t }
                                        Current = { StartTime = now; Acc = TimeSpan.Zero } }

                                state.Commands.Remaining |> (printfn "%A")

                                // Displaying time.
                                match h with
                                | Command2.Down _ -> TimeSpan.Zero
                                | Command2.Up v -> v.Duration
                                |> timeSpanToDisplay
                                |> fun x -> (document.getElementById "timerArea").innerHTML <- x

                                // Playing alarm.
                                match alarm h with
                                | "bell"
                                | "beep" as x ->
                                    let audio = document.getElementById x :?> HTMLAudioElement
                                    audio.currentTime <- 0
                                    audio.play ()
                                | _ -> ()

                                // Pausing.
                                if Command2.shouldPause h then
                                    // Skipping the pause option with the last command.
                                    if List.length t > 0 then
                                        stop ()
                            else
                                // The current command has NOT come to its end.
                                let elapsedTime' = elapsedTime h state.Current.StartTime state.Current.Acc now
                                (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay elapsedTime'

                                match h with
                                | Command2.Down v -> (v.Color, v.Background, v.Message)
                                | Command2.Up v -> (v.Color, v.Background, v.Message)
                                |> fun (color, bgcolor, message) ->
                                    document.body.setAttribute (
                                        "style",
                                        (sprintf "color: %s; background-color: %s;" color bgcolor)
                                    )

                                    (document.getElementById "messageArea").innerText <- message)
                    10

            state <- { state with IntervalId = intervalId }
        | _ -> ()

    let reset event =
        match state.RunningStatus with
        | RunningStatus.Running -> stop ()
        | RunningStatus.Stopping
        | RunningStatus.Finished ->
            (document.getElementById "timerArea").classList.remove "finished"
            (document.getElementById "messageArea").classList.remove "finished"
            (document.getElementById "timerArea").innerText <- ""
            (document.getElementById "messageArea").innerText <- ""
            document.body.removeAttribute "style"

            match state.WakeLock with
            | Some x ->
                printfn $"releasing at %s{DateTime.Now.ToString()}"
                WakeLockAPI.release x
            | None -> printfn "doing nothing..."

            state <- initState

            (document.getElementById "outputArea").innerText <- ""
        | _ -> ()

    document.addEventListener (
        "visibilitychange",
        fun _ ->
            match state.RunningStatus with
            | RunningStatus.Running
            | RunningStatus.Stopping ->
                if WakeLockAPI.isSupported () then
                    state <-
                        { state with
                            WakeLock =
                                try
                                    printfn $"locking at %s{DateTime.Now.ToString()}"
                                    WakeLockAPI.lock () |> Some
                                with _ ->
                                    None }

                    let outputArea = document.getElementById "outputArea"

                    match state.WakeLock with
                    | Some x ->
                        if not x?released then
                            printfn $"locked at %s{DateTime.Now.ToString()}"
                            outputArea.innerText <- "画面起動ロック API により、タイマー動作中は画面がスリープしません。"
                    | None ->
                        printfn "failed to lock ...."
                        outputArea.innerText <- ""
            | _ -> ()
    )
