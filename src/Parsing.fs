// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open System
open Fermata.ParserCombinators.Parsers

module Parsing =
    type Options =
        | Color of string
        | Background of string
        | Message of string
        | ShouldPause of bool
        | Alarm of string

    type CommandAndOptions =
        | Up of TimeSpan * Options list
        | Down of TimeSpan * Options list

    let oneOrMore (p: Parser<'T>) : Parser<'T list> =
        map' (fun (x, xs) -> x :: xs) (p <&> many p)

    let digit: Parser<char> = [ '0' .. '9' ] |> List.map char' |> List.reduce (<|>)

    let hex: Parser<char> =
        [ '0' .. '9' ] @ [ 'A' .. 'F' ] @ [ 'a' .. 'f' ]
        |> List.map char'
        |> List.reduce (<|>)

    let integer: Parser<int> =
        let f = List.map string >> String.concat "" >> int
        map' (fun (x, y) -> x :: y |> f) (digit <&> many digit)

    let integer2: Parser<int> =
        let f (x, y) = sprintf "%c%c" x y |> int
        map' f (digit <&> digit)

    let integer1: Parser<int> = map' (string >> int) digit

    let integer2or1: Parser<int> = integer2 <|> integer1

    let spaces: Parser<string> =
        map' (List.map string >> String.concat "") (oneOrMore (char' ' '))

    let minute: Parser<TimeSpan> =
        let f (m, s) = TimeSpan(0, m, s)
        map' f (integer2or1 <+&> string' ":" <&> integer2)

    let hour: Parser<TimeSpan> =
        let f ((h, m), s) = TimeSpan(h, m, s)
        map' f (integer2or1 <+&> char' ':' <&> integer2 <+&> char' ':' <&> integer2)

    let time: Parser<TimeSpan> = hour <|> minute

    let hexCode: Parser<string> =
        let f (h, xs) =
            sprintf "%c%s" h ((List.map string >> String.concat "") xs)

        map' f (char' '#' <&> (repeat 6 hex))

    let colorOption: Parser<Options> =
        map'
            Options.Color
            ((string' "--color" <|> string' "-c") <&> spaces <&+> hexCode
             <+&> (pos spaces <|> end'))

    let backgroundOption: Parser<Options> =
        map'
            Options.Background
            ((string' "--background" <|> string' "-bg") <&> spaces <&+> hexCode
             <+&> (pos spaces <|> end'))

    // let messageOption2: Parser<Options> =
    //     let name = string' "--message" <|> string' "-m"

    //     let body =
    //         let names = string' "--color" <|> string' "-c"

    //         many (any <+&> (neg (spaces <&> names) <|> neg end'))
    //         <&> (any <+&> (pos (spaces <&> names) <|> pos end'))

    //     let f (cs, c) =
    //         c :: List.rev cs
    //         |> List.rev
    //         |> List.map string
    //         |> String.concat ""
    //         |> _.Trim()
    //         |> Options.Message

    //     map' f (name <&> spaces <&+> body)

    let messageOption: Parser<Options> =
        let name = string' "--message" <|> string' "-m"

        let body' =
            let names =
                string' "--color"
                <|> string' "-c"
                <|> string' "--background"
                <|> string' "-bg"
                <|> string' "--pause"
                <|> string' "-p"
                <|> string' "--alarm"
                <|> string' "-a"

            let withoutTerminator = neg (spaces <&> names) <&> neg end'
            let withTerminator = pos (spaces <&> names) <|> pos end'
            (many (any <+&> withoutTerminator)) <&> (any <+&> withTerminator)

        let f (cs, c) =
            c :: List.rev cs
            |> List.rev
            |> List.map string
            |> String.concat ""
            |> _.Trim()
            |> Options.Message

        map' f (name <&> spaces <&+> body')

    let pauseOption: Parser<Options> =
        let f _ = true
        map' (f >> Options.ShouldPause) ((string' "--pause" <|> string' "-p") <+&> (pos spaces <|> end'))

    let alarmOption: Parser<Options> =
        map'
            Options.Alarm
            ((string' "--alarm" <|> string' "-a") <&> spaces
             <&+> (string' "bell" <|> string' "beep")
             <+&> (pos spaces <|> end'))

    let options: Parser<Options list> =
        many (
            (spaces <&+> colorOption)
            <|> (spaces <&+> backgroundOption)
            <|> (spaces <&+> messageOption)
            <|> (spaces <&+> pauseOption)
            <|> (spaces <&+> alarmOption)
        )

    let downCommand: Parser<CommandAndOptions> =
        let f ((_, t), l) = CommandAndOptions.Down(t, l)
        map' f (string' "down" <+&> spaces <&> time <&> options <+&> (many spaces <&> end'))

    let upCommand: Parser<CommandAndOptions> =
        let f ((_, t), l) = CommandAndOptions.Up(t, l)
        map' f (string' "up" <+&> spaces <&> time <&> options <+&> (many spaces <&> end'))

    let command: Parser<CommandAndOptions> = downCommand <|> upCommand
