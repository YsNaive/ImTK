# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Implemented the core dual-tier lifecycle architecture (`ImTKApplication`, `ImTKModule`, `ImTKObject`).
- Introduced a strict `ApplicationState` machine to prevent frame logic reentrancy.
- Added `Time` static utility class to handle unscaled/scaled delta time automatically.
- Added `ImTKSilk.Run()` driver to initialize Silk.NET and integrate ImGui controller with the lifecycle hooks.
- Enabled ImGui Viewports via `ImTKSilkConstant` to natively support multi-window layouts.
- Added foundational documentation structure under `docs/` divided by subsystems (`Core`, `UI`, `Database`, `Log`, `Event`, `Project`).
- Added strict `NamingConventions.md` to establish rules for casing, prefixes, and component suffixes.

### Changed
- Reorganized old `Architecture/` markdown files into their respective subsystem folders.
- Moved `AGENT.md` to the repository root and expanded guidelines.
- Modified `DevelopmentWrapUp.md` to include a mandatory 5-step pre-merge checklist.
