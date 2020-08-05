# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased]

- Nothing!

[Unreleased]: https://github.com//Stamina/compare/v0.2.2...HEAD


## [0.2.2] - 2020-08-05
### Changed

- Fix exception where bots were still iterated in networking code.

[0.2.2]: https://github.com/keyspace/Stamina/compare/v0.2.1..v0.2.2


## [0.2.1] - 2020-08-05
### Changed

- Try not to crash multi-player servers on player log-in if data is not fully loaded.

[0.2.1]: https://github.com/keyspace/Stamina/compare/v0.2...v0.2.1


## [0.2] - 2020-08-01
### Added

- An overlay that fades in as stamina gets low.
- This changelog file.

### Changed

- Fixed low stamina on bots (wolves, spiders, etc.) being applied to the game host.
  Should get less sudded deaths now.

[0.2]: https://github.com/keyspace/Stamina/compare/v0.1.1...v0.2


## [0.1.1] - 2020-06-18
### Changed

- Characters now re-spawn with 50% stamina instead of whatever they had before death.
- Negative stamina to do half as much damage (still not configurable).

[0.1.1]: https://github.com/keyspace/Stamina/compare/v0.1...v0.1.1


## [0.1] - 2020-06-15
### Changed

- Added mod ID to code to avoid networking packet collision with other mods.
- Added thumbnail to repo and Steam Workshop.

[0.1]: https://github.com/keyspace/Stamina/compare/v0.0...v0.1


## [0.0] - 2020-06-15
### Added
- Initial submission to acquire mod Steam ID.
- Stamina gain/cost based on movement state is server-configurable (per save file).
  Nothing else is.
- Player stats are retained between sessions (per save file) based on player Steam ID.

[0.0]: https://github.com/keyspace/Stamina/releases/tag/v0.0

