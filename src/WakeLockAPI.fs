// ccc Version 0.11.0
// https://github.com/taidalog/ccc
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/ccc/blob/main/LICENSE
namespace Ccc

open System
open Browser.Navigator
open Fable.Core
open Fable.Core.JsInterop

module WakeLockAPI =
    let isSupported () : bool =
        emitJsStatement () """return "wakeLock" in navigator"""

    let lock () : JS.Promise<obj> =
        promise {
            let! x = navigator?wakeLock?request ("screen")
            return x
        }

    let release (x: JS.Promise<obj>) : unit =
        x.``then`` (fun x -> x?release ()) |> ignore
