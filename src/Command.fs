// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open System

module Command2 =
    type DownCommand =
        { Duration: TimeSpan
          Color: string
          Background: string
          Message: string
          ShouldPause: bool
          Alarm: string }

    type UpCommand =
        { Duration: TimeSpan
          Color: string
          Background: string
          Message: string
          ShouldPause: bool
          Alarm: string }

    [<StructuredFormatDisplay("{DisplayText}")>]
    type Command2 =
        | Down of DownCommand
        | Up of UpCommand

        override this.ToString() =
            match this with
            | Command2.Down v ->
                $"Countdown for %s{string v.Duration}, color: %s{v.Color}; background-color: %s{v.Background}; message: %s{v.Message}; ShouldPause: %b{v.ShouldPause}; Alarm: %s{v.Alarm}"
            | Command2.Up v ->
                $"Countup for %s{string v.Duration}, color: %s{v.Color}; background-color: %s{v.Background}; message: %s{v.Message}; ShouldPause: %b{v.ShouldPause}; Alarm: %s{v.Alarm}"

        member this.DisplayText = this.ToString()

    let duration (x: Command2) : TimeSpan =
        match x with
        | Command2.Down v -> v.Duration
        | Command2.Up v -> v.Duration

    let color (x: Command2) : string =
        match x with
        | Command2.Down v -> v.Color
        | Command2.Up v -> v.Color

    let background (x: Command2) : string =
        match x with
        | Command2.Down v -> v.Background
        | Command2.Up v -> v.Background

    let message (x: Command2) : string =
        match x with
        | Command2.Down v -> v.Message
        | Command2.Up v -> v.Message

    let shouldPause (x: Command2) : bool =
        match x with
        | Command2.Down v -> v.ShouldPause
        | Command2.Up v -> v.ShouldPause

    let alarm (x: Command2) : string =
        match x with
        | Command2.Down v -> v.Alarm
        | Command2.Up v -> v.Alarm

    let defaultDown: DownCommand =
        { Duration = TimeSpan.Zero
          Color = ""
          Background = ""
          Message = ""
          ShouldPause = false
          Alarm = "" }

    let defaultUp: UpCommand =
        { Duration = TimeSpan.Zero
          Color = ""
          Background = ""
          Message = ""
          ShouldPause = false
          Alarm = "" }

    let update (command: Command2) (options: Parsing.Options) : Command2 =
        match command with
        | Command2.Down d ->
            match options with
            | Parsing.Options.Color v -> { d with Color = v }
            | Parsing.Options.Background v -> { d with Background = v }
            | Parsing.Options.Message v -> { d with Message = v }
            | Parsing.Options.ShouldPause v -> { d with ShouldPause = v }
            | Parsing.Options.Alarm v -> { d with Alarm = v }
            |> Command2.Down
        | Command2.Up u ->
            match options with
            | Parsing.Options.Color v -> { u with Color = v }
            | Parsing.Options.Background v -> { u with Background = v }
            | Parsing.Options.Message v -> { u with Message = v }
            | Parsing.Options.ShouldPause v -> { u with ShouldPause = v }
            | Parsing.Options.Alarm v -> { u with Alarm = v }
            |> Command2.Up

    let build' (x: Parsing.CommandAndOptions) : Command2 =
        match x with
        | Parsing.CommandAndOptions.Down(t, vs) -> List.fold update (Command2.Down { defaultDown with Duration = t }) vs
        | Parsing.CommandAndOptions.Up(t, vs) -> List.fold update (Command2.Up { defaultUp with Duration = t }) vs
