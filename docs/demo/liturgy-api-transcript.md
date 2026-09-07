# liturgy-api

Narrator: Microsoft David Desktop (Windows SAPI), rate -1.

## 00:00:00 — Liturgy · API enforcement

This technical demonstration follows real requests to the local Liturgy API. The display shows selected response fields, while authentication credentials remain outside the recording.

## 00:00:14 — Read the workspace

An authenticated request has returned the seeded projects. We select Lantern and inspect its actual board and movement state, using a fresh database for this demonstration.

## 00:00:30 — Inspect the loop

Lantern twenty four has three movements recorded and cannot yet be marked done. The response exposes both the next movement and whether completion is currently allowed.

## 00:00:43 — An incomplete loop is rejected

A premature Done request returned conflict, with the explanation that the five R loop is incomplete. This is server enforcement, independent of the disabled button in the browser.

## 00:00:57 — Record Render

The Render request succeeded. The response now reports four completed movements and Rejoice as the next step. The artifact and change notes accompany the movement.

## 00:01:10 — Record Rejoice

The thanksgiving request succeeded. All five movements are logged, and the API now reports that this card can be marked done.

## 00:01:21 — Complete and read back

The Done request succeeded, and a fresh board request confirms the card remains in Done. This read back verifies the saved result rather than relying only on the command response.

## 00:01:35 — Inspect the gate checklist

The project response shows the Develop gate is blocked by its outstanding demo preparation requirement. The checklist is a recorded team declaration, not an automated assessment of every artifact.

## 00:01:49 — Save and verify the gate

The checklist update succeeded, and another project request confirms the gate is Open. The response gives clients the same persisted gate state.

## 00:02:01 — Shared rules, faithful practice

The API has demonstrated movement ordering, completion enforcement, and a persisted gate update. These shared rules support a team rhythm of prayer, discernment, action, and gratitude.
