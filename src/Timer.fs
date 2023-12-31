// ccc Version 0.4.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open System
open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Command
open WakeLockAPI
open Fermata

module Timer' =
    type RunningStatus =
        | NotStarted = 0
        | Running = 1
        | Stopping = 2
        | Finished = 4

    type TimeAcc = { StartTime: DateTime; Acc: TimeSpan }

    type State =
        { Stop: TimeAcc
          IntervalId: int
          Commands: Command list
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
          IntervalId = -1
          Commands = []
          WakeLock = None
          RunningStatus = RunningStatus.NotStarted }

    let initState =
        { Stop =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          IntervalId = -1
          Commands = []
          WakeLock = None
          RunningStatus = RunningStatus.NotStarted }

    let timeSpanToDisplay (timeSpan: TimeSpan) =
        let h = timeSpan.Hours |> string |> String.padLeft 2 '0'
        let m = timeSpan.Minutes |> string |> String.padLeft 2 '0'
        let s = timeSpan.Seconds |> string |> String.padLeft 2 '0'
        let ms = timeSpan.Milliseconds |> string |> String.padLeft 3 '0'
        $"""%s{h}:%s{m}:%s{s}<span class="decimals">.%s{ms}</span>"""

    let currentCommand (commands: Command list) (startTime: DateTime) (acc: TimeSpan) (t: DateTime) : Command =
        let passed = t - startTime + acc
        commands |> List.findBack (fun x -> Command.delay x <= passed)

    let f (commands: Command list) (startTime: DateTime) (acc: TimeSpan) (t: DateTime) : TimeSpan =
        let passed = t - startTime + acc
        let c = currentCommand commands startTime acc t

        match c with
        | Command.CountDown(duration, delay, _, _, _) -> duration - (passed - delay)
        | Command.CountUp(_, delay, _, _, _) -> passed - delay

    let start () =
        match state.RunningStatus with
        | RunningStatus.NotStarted
        | RunningStatus.Finished ->
            (document.getElementById "timerArea").classList.remove "finished"
            (document.getElementById "messageArea").classList.remove "finished"

            let commands =
                (document.getElementById "commandInput" :?> HTMLInputElement).value
                |> Command.ofString

            state <-
                { initState with
                    Stop =
                        { initState.Stop with
                            StartTime = DateTime.Now
                            Acc = TimeSpan.Zero }
                    Commands = commands
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

            let f' = f state.Commands state.Stop.StartTime

            let totalDuration =
                state.Commands |> List.map Command.duration |> List.fold (+) TimeSpan.Zero

            let intervalId =
                setInterval
                    (fun _ ->
                        let elapsedTime = f' state.Stop.Acc DateTime.Now
                        (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay elapsedTime

                        currentCommand state.Commands state.Stop.StartTime state.Stop.Acc DateTime.Now
                        |> function
                            | Command.CountDown(_, _, color, bgcolor, message) -> (color, bgcolor, message)
                            | Command.CountUp(_, _, color, bgcolor, message) -> (color, bgcolor, message)
                        |> fun (color, bgcolor, message) ->
                            document.body.setAttribute (
                                "style",
                                (sprintf "color: %s; background-color: %s;" color bgcolor)
                            )

                            (document.getElementById "messageArea").innerText <- message

                        if (DateTime.Now - state.Stop.StartTime + state.Stop.Acc) > totalDuration then
                            match state.Commands |> List.last with
                            | Command.CountDown(_, _, _, _, _) -> TimeSpan.Zero
                            | Command.CountUp(duration, _, _, _, _) -> duration
                            |> fun x -> (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay x

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

                            clearInterval state.IntervalId)
                    10

            state <- { state with IntervalId = intervalId }
        | RunningStatus.Stopping ->
            state <-
                { state with
                    Stop =
                        { state.Stop with
                            StartTime = DateTime.Now }
                    RunningStatus = RunningStatus.Running }

            let f' = f state.Commands state.Stop.StartTime

            let totalDuration =
                state.Commands |> List.map Command.duration |> List.fold (+) TimeSpan.Zero

            let intervalId =
                setInterval
                    (fun _ ->
                        let elapsedTime = f' state.Stop.Acc DateTime.Now
                        (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay elapsedTime

                        currentCommand state.Commands state.Stop.StartTime state.Stop.Acc DateTime.Now
                        |> function
                            | Command.CountDown(_, _, color, bgcolor, message) -> (color, bgcolor, message)
                            | Command.CountUp(_, _, color, bgcolor, message) -> (color, bgcolor, message)
                        |> fun (color, bgcolor, message) ->
                            document.body.setAttribute (
                                "style",
                                (sprintf "color: %s; background-color: %s;" color bgcolor)
                            )

                            (document.getElementById "messageArea").innerText <- message

                        if (DateTime.Now - state.Stop.StartTime + state.Stop.Acc) > totalDuration then
                            match state.Commands |> List.last with
                            | Command.CountDown(_, _, _, _, _) -> TimeSpan.Zero
                            | Command.CountUp(duration, _, _, _, _) -> duration
                            |> fun x -> (document.getElementById "timerArea").innerHTML <- timeSpanToDisplay x

                            (document.getElementById "timerArea").classList.add "finished"
                            (document.getElementById "messageArea").classList.add "finished"

                            state <-
                                { state with
                                    RunningStatus = RunningStatus.Finished }

                            clearInterval state.IntervalId)
                    10

            state <- { state with IntervalId = intervalId }
        | _ -> ()

    let stop () =
        match state.RunningStatus with
        | RunningStatus.Running ->
            clearInterval state.IntervalId

            state <-
                { state with
                    Stop =
                        { state.Stop with
                            Acc = state.Stop.Acc + (DateTime.Now - state.Stop.StartTime) }
                    RunningStatus = RunningStatus.Stopping }
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
