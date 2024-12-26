// ccc Version 0.11.0
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
          //   Delay = TimeSpan.Zero
          Color = ""
          Background = ""
          Message = ""
          ShouldPause = false
          Alarm = "" }

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
          //   Delay = TimeSpan.Zero
          Color = "#ffffff"
          Background = "#65a2ac"
          Message = "hey hey"
          ShouldPause = false
          Alarm = "" }
        |> Command2.Down

    let actual: Command2 = Command2.build' seed

    Assert.Equal(expected, actual)
