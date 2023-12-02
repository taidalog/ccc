namespace Ccc

open System
open Browser.Dom
open Fable.Core
open Command
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
          RunningStatus = RunningStatus.NotStarted }

    let initState =
        { Stop =
            { StartTime = DateTime.MinValue
              Acc = TimeSpan.Zero }
          IntervalId = -1
          RunningStatus = RunningStatus.NotStarted }

    let timeSpanToDisplay (timeSpan: TimeSpan) =
        let h = timeSpan.Hours |> string |> String.padLeft 2 '0'
        let m = timeSpan.Minutes |> string |> String.padLeft 2 '0'
        let s = timeSpan.Seconds |> string |> String.padLeft 2 '0'
        let ms = timeSpan.Milliseconds |> string |> String.padLeft 3 '0'
        $"%s{h}:%s{m}:%s{s}.%s{ms}"

    let rec start commands : unit =
        match commands with
        | [] -> ()
        | h :: t ->
            state <-
                { initState with
                    Stop =
                        { initState.Stop with
                            StartTime = DateTime.Now
                            Acc = TimeSpan.Zero }
                    RunningStatus = RunningStatus.Running }

            match h with
            | Command.Invalid -> start t
            | Command.CountDown(time, color, bgcolor, message) ->
                document.body.setAttribute ("style", (sprintf "color: %s; background-color: %s;" color bgcolor))
                (document.getElementById "messageArea").innerText <- message

                let intervalId =
                    setInterval
                        (fun _ ->
                            let elapsedTime = time - (DateTime.Now - state.Stop.StartTime + state.Stop.Acc)

                            if elapsedTime >= TimeSpan.Zero then
                                document.getElementById("timerArea").innerText <- timeSpanToDisplay elapsedTime
                            else
                                document.getElementById("timerArea").innerText <- timeSpanToDisplay TimeSpan.Zero
                                clearInterval state.IntervalId
                                start t)
                        10

                state <- { state with IntervalId = intervalId }
            | Command.CountUp(time, color, bgcolor, message) ->
                document.body.setAttribute ("style", (sprintf "color: %s; background-color: %s;" color bgcolor))
                (document.getElementById "messageArea").innerText <- message

                let intervalId =
                    setInterval
                        (fun _ ->
                            let elapsedTime = DateTime.Now - state.Stop.StartTime + state.Stop.Acc

                            if elapsedTime <= time then
                                document.getElementById("timerArea").innerText <- timeSpanToDisplay elapsedTime
                            else
                                document.getElementById("timerArea").innerText <- timeSpanToDisplay time
                                clearInterval state.IntervalId
                                start t)
                        10

                state <- { state with IntervalId = intervalId }
