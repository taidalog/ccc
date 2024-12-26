// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
module Parsing.Tests

open System
open Xunit
open Fermata.ParserCombinators.Parsers
open Ccc.Parsing

[<Fact>]
let ``digit 1`` () =
    let expected = Ok('1', State("1:23:45", 1))
    let actual = digit (State("1:23:45", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``hex 1`` () =
    let expected = Ok('6', State("65a2ac", 1))
    let actual = hex (State("65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``hex 2`` () =
    let expected = Ok('a', State("65a2ac", 3))
    let actual = hex (State("65a2ac", 2))
    Assert.Equal(expected, actual)

[<Fact>]
let ``integer 1`` () =
    let expected = Ok(1, State("1:23:45", 1))
    let actual = integer (State("1:23:45", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``integer2 1`` () =
    let expected = Ok(12, State("12:34", 2))
    let actual = integer2 (State("12:34", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``integer1 1`` () =
    let expected = Ok(1, State("12:34", 1))
    let actual = integer1 (State("12:34", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``integer2or1 1`` () =
    let expected = Ok(12, State("12:34", 2))
    let actual = integer2or1 (State("12:34", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``minute 1`` () =
    let expected = Ok(TimeSpan(0, 12, 34), State("12:34", 5))
    let actual = minute (State("12:34", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``hour 1`` () =
    let expected = Ok(TimeSpan(1, 23, 45), State("1:23:45", 7))
    let actual = hour (State("1:23:45", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``time 1`` () =
    let expected = Ok(TimeSpan(0, 12, 34), State("12:34", 5))
    let actual = time (State("12:34", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``time 2`` () =
    let expected = Ok(TimeSpan(1, 23, 45), State("1:23:45", 7))
    let actual = time (State("1:23:45", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``hexCode 1`` () =
    let expected = Ok("#65a2ac", State("#65a2ac", 7))
    let actual = hexCode (State("#65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``spaces 1`` () =
    let expected = Ok(" ", State(" <- a space", 1))
    let actual = spaces (State(" <- a space", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``spaces 2`` () =
    let expected = Ok("  ", State("  <- two spaces", 2))
    let actual = spaces (State("  <- two spaces", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 1`` () =
    let expected = Ok(Options.Color "#65a2ac", State("--color #65a2ac", 15))
    let actual = colorOption (State("--color #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 2`` () =
    let expected = Ok(Options.Color "#65a2ac", State("-c #65a2ac", 10))
    let actual = colorOption (State("-c #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 3`` () =
    let expected = Ok(Options.Color "#65a2ac", State("-c #65a2ac -bg #ffffff", 10))
    let actual = colorOption (State("-c #65a2ac -bg #ffffff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 4`` () =
    let expected = Error("Parsing failed.", State("-c#65a2ac", 0))
    let actual = colorOption (State("-c#65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 5`` () =
    let expected = Error("Parsing failed.", State("-c -bg #ffffff", 0))
    let actual = colorOption (State("-c -bg #ffffff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 6`` () =
    let expected = Error("Parsing failed.", State("-c #xxxxxx", 0))
    let actual = colorOption (State("-c #xxxxxx", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``colorOption 7`` () =
    let expected = Error("Parsing failed.", State("-c #65a2acff", 0))
    let actual = colorOption (State("-c #65a2acff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 1`` () =
    let expected = Ok(Options.Background "#65a2ac", State("--background #65a2ac", 20))
    let actual = backgroundOption (State("--background #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 2`` () =
    let expected = Ok(Options.Background "#65a2ac", State("-bg #65a2ac", 11))
    let actual = backgroundOption (State("-bg #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 3`` () =
    let expected = Ok(Options.Background "#65a2ac", State("-bg #65a2ac -c #ffffff", 11))
    let actual = backgroundOption (State("-bg #65a2ac -c #ffffff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 4`` () =
    let expected = Error("Parsing failed.", State("-bg#65a2ac", 0))
    let actual = backgroundOption (State("-bg#65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 5`` () =
    let expected = Error("Parsing failed.", State("-bg -c #ffffff", 0))
    let actual = backgroundOption (State("-bg -c #ffffff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 6`` () =
    let expected = Error("Parsing failed.", State("-bg #xxxxxx", 0))
    let actual = backgroundOption (State("-bg #xxxxxx", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``backgroundOption 7`` () =
    let expected = Error("Parsing failed.", State("-bg #65a2acff", 0))
    let actual = backgroundOption (State("-bg #65a2acff", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``messageOption 1`` () =
    let expected = Ok(Options.Message "hey hey", State("--message hey hey", 17))
    let actual = messageOption (State("--message hey hey", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``messageOption 2`` () =
    let expected =
        Ok(Options.Message "hey hey", State("--message hey hey --color #65a2ac", 17))

    let actual = messageOption (State("--message hey hey --color #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``messageOption 3`` () =
    let expected = Ok(Options.Message "hey hey", State("--message hey hey --pause", 17))

    let actual = messageOption (State("--message hey hey --pause", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``pauseOption 1`` () =
    let expected = Ok(Options.ShouldPause true, State("--pause --color #65a2ac", 7))
    let actual = pauseOption (State("--pause --color #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``pauseOption 2`` () =
    let expected = Ok(Options.ShouldPause true, State("-p --color #65a2ac", 2))
    let actual = pauseOption (State("-p --color #65a2ac", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 1`` () =
    let expected = Ok(Options.Alarm "beep", State("--alarm beep", 12))
    let actual = alarmOption (State("--alarm beep", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 2`` () =
    let expected = Ok(Options.Alarm "beep", State("-a beep", 7))
    let actual = alarmOption (State("-a beep", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 3`` () =
    let expected = Ok(Options.Alarm "bell", State("--alarm bell", 12))
    let actual = alarmOption (State("--alarm bell", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 4`` () =
    let expected = Ok(Options.Alarm "bell", State("-a bell", 7))
    let actual = alarmOption (State("-a bell", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 5`` () =
    let expected = Error("Parsing failed.", State("-alarm bell", 0))
    let actual = alarmOption (State("-alarm bell", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 6`` () =
    let expected = Error("Parsing failed.", State("--a bell", 0))
    let actual = alarmOption (State("--a bell", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 7`` () =
    let expected = Error("Position exceeded input length.", State("--alarm", 0))
    let actual = alarmOption (State("--alarm", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``alarmOption 8`` () =
    let expected = Error("Position exceeded input length.", State("-a", 0))
    let actual = alarmOption (State("-a", 0))
    Assert.Equal(expected, actual)

[<Fact>]
let ``downCommand 1`` () =
    let expected =
        Ok(CommandAndOptions.Down(TimeSpan(0, 5, 0), [ Options.Message "hey" ]), State("down 5:00 -m hey", 16))

    let actual = downCommand (State("down 5:00 -m hey", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``downCommand 2`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.Message "hey hey" ]
            ),
            State("down 5:00 --color #ffffff --background #65a2ac --message hey hey", 64)
        )

    let actual =
        downCommand (State("down 5:00 --color #ffffff --background #65a2ac --message hey hey", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``downCommand 3`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac" ]
            ),
            State("down 5:00 --message hey hey --color #ffffff --background #65a2ac", 64)
        )

    let actual =
        downCommand (State("down 5:00 --message hey hey --color #ffffff --background #65a2ac", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``downCommand 4`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.ShouldPause true ]
            ),
            State("down 5:00 --message hey hey --color #ffffff --background #65a2ac --pause", 72)
        )

    let actual =
        downCommand (State("down 5:00 --message hey hey --color #ffffff --background #65a2ac --pause", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``downCommand 5`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.ShouldPause true
                  Options.Alarm "bell" ]
            ),
            State("down 5:00 --message hey hey --color #ffffff --background #65a2ac --pause --alarm bell", 85)
        )

    let actual =
        downCommand (State("down 5:00 --message hey hey --color #ffffff --background #65a2ac --pause --alarm bell", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``upCommand 1`` () =
    let expected =
        Ok(CommandAndOptions.Up(TimeSpan(0, 5, 0), [ Options.Message "hey" ]), State("up 5:00 -m hey", 14))

    let actual = upCommand (State("up 5:00 -m hey", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``upCommand 2`` () =
    let expected =
        Ok(
            CommandAndOptions.Up(
                TimeSpan(0, 5, 0),
                [ Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.Message "hey hey" ]
            ),
            State("up 5:00 --color #ffffff --background #65a2ac --message hey hey", 62)
        )

    let actual =
        upCommand (State("up 5:00 --color #ffffff --background #65a2ac --message hey hey", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``upCommand 3`` () =
    let expected =
        Ok(
            CommandAndOptions.Up(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac" ]
            ),
            State("up 5:00 --message hey hey --color #ffffff --background #65a2ac", 62)
        )

    let actual =
        upCommand (State("up 5:00 --message hey hey --color #ffffff --background #65a2ac", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``upCommand 4`` () =
    let expected =
        Ok(
            CommandAndOptions.Up(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.ShouldPause true ]
            ),
            State("up 5:00 --message hey hey --color #ffffff --background #65a2ac --pause", 70)
        )

    let actual =
        upCommand (State("up 5:00 --message hey hey --color #ffffff --background #65a2ac --pause", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``upCommand 5`` () =
    let expected =
        Ok(
            CommandAndOptions.Up(
                TimeSpan(0, 5, 0),
                [ Options.Message "hey hey"
                  Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.ShouldPause true
                  Options.Alarm "bell" ]
            ),
            State("up 5:00 --message hey hey --color #ffffff --background #65a2ac --pause --alarm bell", 83)
        )

    let actual =
        upCommand (State("up 5:00 --message hey hey --color #ffffff --background #65a2ac --pause --alarm bell", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``command 1`` () =
    let expected =
        Ok(
            CommandAndOptions.Down(
                TimeSpan(0, 5, 0),
                [ Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.Message "hey hey" ]
            ),
            State("down 5:00 --color #ffffff --background #65a2ac --message hey hey", 64)
        )

    let actual =
        command (State("down 5:00 --color #ffffff --background #65a2ac --message hey hey", 0))

    Assert.Equal(expected, actual)

[<Fact>]
let ``command 2`` () =
    let expected =
        Ok(
            CommandAndOptions.Up(
                TimeSpan(0, 5, 0),
                [ Options.Color "#ffffff"
                  Options.Background "#65a2ac"
                  Options.Message "hey hey" ]
            ),
            State("up 5:00 --color #ffffff --background #65a2ac --message hey hey", 62)
        )

    let actual =
        command (State("up 5:00 --color #ffffff --background #65a2ac --message hey hey", 0))

    Assert.Equal(expected, actual)
