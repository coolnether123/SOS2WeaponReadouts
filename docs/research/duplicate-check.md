# Duplicate check

Checked 2026-07-30 against current Steam Workshop, GitHub, SOS2 source, and
weapon/heat/energy/network synonyms. No maintained equivalent supplies the
full per-weapon heat and pulse readout, connection/capacity comparison, and
placement-warning scope. No verified public original Discord author was found.

Closest adjacent projects:

- [SOS2 Heat Statistics (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=2803774052)
  is unmaintained, supports only 1.1–1.3, and reports radiator/heatsink
  management statistics rather than per-weapon firing costs.
- [Relevant Stats In Description](https://steamcommunity.com/sharedfiles/filedetails/?id=2692669482)
  has no documented SOS2 weapon adapter.
- [WeaponStats](https://steamcommunity.com/sharedfiles/filedetails/?id=974066449)
  does not expose SOS2 network costs or warnings.

Current SOS2 already reports some stored-heat/network and energy-needed text.
The implementation must detect that output and add only missing values.

Decision: proceed with deduplication against current SOS2 UI.

API source: [Bqr1s/SaveOurShip2 stable](https://github.com/Bqr1s/SaveOurShip2/tree/stable),
locally pinned at commit `296ba9a2bec124981cff46e557a07934702a210b`.
