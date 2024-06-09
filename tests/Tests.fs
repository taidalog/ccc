// ccc Version 0.7.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
module Tests

open System
open Xunit
open Ccc
open Ccc.Command2

[<Fact>]
let ``Command2.update 1`` () =
    let seed: DownCommand =
        { Duration = TimeSpan(0, 5, 0)
          Delay = TimeSpan.Zero
          Color = ""
          Background = ""
          Message = "" }

    let updated = { seed with Color = "#65a2ac" }
    let expected = Command2.Down(updated)

    let actual =
        Command2.update (Command2.Command2.Down seed) (Parsing.Options.Color "#65a2ac")

    Assert.Equal(expected, actual)

[<Fact>]
let ``Command2.build' 1`` () =
    let seed: Parsing.CommandAndOptions =
        Parsing.CommandAndOptions.Down(
            TimeSpan(0, 5, 0),
            [ Parsing.Options.Color "#ffffff"
              Parsing.Options.Background "#65a2ac"
              Parsing.Options.Message "hey hey" ]
        )

    let expected: Command2 =
        { DownCommand.Duration = TimeSpan(0, 5, 0)
          Delay = TimeSpan.Zero
          Color = "#ffffff"
          Background = "#65a2ac"
          Message = "hey hey" }
        |> Command2.Down

    let actual: Command2 = Command2.build' seed

    Assert.Equal(expected, actual)

[<Fact>]
let ``Command2.withDelay 1`` () =
    let seeds: Parsing.CommandAndOptions list =
        [ Parsing.CommandAndOptions.Down(
              TimeSpan(0, 5, 0),
              [ Parsing.Options.Color "#ffffff"
                Parsing.Options.Background "#65a2ac"
                Parsing.Options.Message "hey hey" ]
          )
          Parsing.CommandAndOptions.Up(
              TimeSpan(0, 2, 0),
              [ Parsing.Options.Color "#333333"
                Parsing.Options.Background "#ffffff"
                Parsing.Options.Message "hey" ]
          ) ]

    let expected: Command2 list =
        [ Command2.Down
              { Duration = TimeSpan(0, 5, 0)
                Delay = TimeSpan.Zero
                Color = "#ffffff"
                Background = "#65a2ac"
                Message = "hey hey" }
          Command2.Up
              { Duration = TimeSpan(0, 2, 0)
                Delay = TimeSpan(0, 5, 0)
                Color = "#333333"
                Background = "#ffffff"
                Message = "hey" } ]

    let actual: Command2 list = seeds |> List.map Command2.build' |> withDelay

    Assert.Equal<Command2 list>(expected, actual)
