# Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project intends to use [Semantic Versioning](https://semver.org/spec/v2.0.0.html) when versioned releases begin.

## [Unreleased]

### Added

- Full-stack vertical slice: .NET 9 Clean Architecture API and Angular 21 workspace
- Server-authoritative enforcement engine for 4D phase gates and 5R loop completion
- JWT authentication, EF Core / SQL Server persistence, and SignalR real-time updates
- Develop Kanban board, the 5R co-creation loop, and requirement checklists
- Project lifecycle operations: update (name + tag), close/reopen (soft-hide via `ProjectStatus`, hidden from the default list), and hard delete of a project and its children
- Card lifecycle operations: optional description and story points, cancel/close/reopen (`CardStatus`, independent of the Done 5R gate), and hard delete of a card and its 5R movements; Closed/Cancelled cards leave the active board
- Account membership by invitation: in-app, token-based invites (no email delivery) with a Lead-only create, pending list, anonymous invite-context lookup, accept, and revoke; registration with a valid invitation token joins the inviting account instead of creating a personal one
- Seeded "Lantern" demo project for first-run development
- FaithTech-faithful HTML design mocks under `docs/mocks`
- Project overview and community health documentation (README, contributing, code of conduct, security, support, governance, contributors, changelog)

The repository has not published a versioned release. Earlier development history is available in the [commit log](https://github.com/QuinntyneBrown/Liturgy/commits/main/).
