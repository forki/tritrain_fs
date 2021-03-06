﻿namespace TriTrain.Core

open TriTrain.Core
open System

// TODO: internationalization
module Dump =
  let dumpRow =
    function
    | FwdRow -> "前列"
    | BwdRow -> "後列"

  let dumpAmount (var, rate) =
    match var with
    | MaxHP -> "最大HP×" + string rate
    | AT -> "AT×" + string rate
    | AG -> "AG×" + string rate
    | One -> rate |> int |> string

  let dumpScopeSide =
    function
    | Home -> "自陣"
    | Oppo -> "敵陣"
    | Both -> "両陣"

  let dumpScope (name, _) =
    name

  let dumpCond =
    function
    | WhenBoT -> "各ターン開始時"
    | WhenEtB -> "盤面に出たとき"
    | WhenDie -> "死亡したとき"

  let dumpKEffect keff =
    let duration =
      match keff |> KEffect.duration with
      | Some n -> sprintf "(%dT)" n
      | None -> "(永続)"
    let typ =
      match keff |> KEffect.typ with
      | ATInc amount ->
          sprintf "ATが%s点増加する効果"
            (dumpAmount amount)
      | AGInc amount ->
          sprintf "AGが%s点増加する効果"
            (dumpAmount amount)
      | Regenerate amount ->
          sprintf "死亡後に最大HPの%s%%で再生する効果"
            (dumpAmount amount)
    in typ + duration

  let dumpOEffectToUnit (typ, scope) =
    match typ with
    | Damage amount ->
        sprintf "%sに%s点のダメージを与える。"
          (dumpScope scope)
          (dumpAmount amount)
    | Heal amount ->
        sprintf "%sを%s点回復する。"
          (dumpScope scope)
          (dumpAmount amount)
    | Death amount ->
        sprintf "%sを(それぞれ)%s%%の確率で即死させる。"
          (dumpScope scope)
          (dumpAmount amount)
    | Give keff ->
        sprintf "%sに%sを与える。"
          (dumpScope scope)
          (dumpKEffect keff)

  let rec dumpOEffectAtom oeff =
    match oeff with
    | OEffectToUnits (typ, scope) ->
        dumpOEffectToUnit (typ, scope)
    | Swap scope ->
        dumpScope scope + "を交代する。"
    | Rotate scopeSide ->
        dumpScopeSide scopeSide + "の回転を起こす。"
    | GenToken cards ->
        failwith "unimplemented"

  and dumpOEffect (oeffs: OEffect) =
    oeffs |> OEffect.toList
    |> List.map dumpOEffectAtom
    |> String.concat ""

  and dumpStatus st =
    sprintf "HP%d/AT%d/AG%d"
      (st |> Status.hp)
      (st |> Status.at)
      (st |> Status.ag)

  and dumpSkillOf row skills =
    match skills |> Map.tryFind row with
    | Some (name, oeff) ->
        sprintf "{%s「%s」: %s}"
          (row |> dumpRow)
          name
          (oeff |> dumpOEffect)
    | None -> ""

  and dumpCardSpec spec =
    sprintf "《%s》(%s)%s%s"
      (spec |> CardSpec.name)
      (spec |> CardSpec.status |> dumpStatus)
      (spec |> CardSpec.skills |> dumpSkillOf FwdRow)
      (spec |> CardSpec.skills |> dumpSkillOf BwdRow)
