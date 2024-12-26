// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
module Timter.Tests

open System
open Xunit
open Fermata.ParserCombinators
open Ccc
open Ccc.Parsing
open Ccc.Timer'

[<Fact>]
let ``split 1`` () =
    let expected = [| "down 05:00"; "up 02:00" |]
    let actual = split "down 05:00 up 02:00"
    Assert.Equal<string array>(expected, actual)

[<Fact>]
let ``split 2`` () =
    let expected = [| "down 00:02 -bg #65a2ac -c #ffffff -m hey hey"; "up 00:" |]
    let actual = split "down 00:02 -bg #65a2ac -c #ffffff -m hey hey up 00:"
    Assert.Equal<string array>(expected, actual)

[<Fact>]
let ``split 3`` () =
    let expected =
        [| "down 00:02 -bg #65a2ac -c #ffffff -m hey hey"
           "up 00:02"
           "down 0:02 c #65a2ac -m 3rd"
           "up 00:02" |]

    let actual =
        split "down 00:02 -bg #65a2ac -c #ffffff -m hey hey up 00:02 down 0:02 c #65a2ac -m 3rd up 00:02"

    Assert.Equal<string array>(expected, actual)

[<Fact>]
let ``parse 1`` () =
    let expected =
        Ok(CommandAndOptions.Down(TimeSpan(0, 5, 0), []), Parsers.State("down 05:00", 10))

    let actual = parse "down 05:00"
    Assert.Equal(expected, actual)

[<Fact>]
let ``parse 2`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Background "#65a2ac"
                  Options.Color "#ffffff"
                  Options.Message "hey hey" ]
            ),
            Parsers.State("down 05:00 -bg #65a2ac -c #ffffff -m hey hey", 44)
        )

    let actual = parse "down 05:00 -bg #65a2ac -c #ffffff -m hey hey"
    Assert.Equal(expected, actual)

[<Fact>]
let ``parse 3`` () =
    let expected = Error("Parsing failed.", Parsers.State("down 05:", 0))

    let actual = parse "down 05:"
    Assert.Equal(expected, actual)

[<Fact>]
let ``parse 4`` () =
    let expected =
        Error("Parsing failed.", Parsers.State("down 0:02 c #65a2ac -m 3rd", 0))

    let actual = parse "down 0:02 c #65a2ac -m 3rd"
    Assert.Equal(expected, actual)
