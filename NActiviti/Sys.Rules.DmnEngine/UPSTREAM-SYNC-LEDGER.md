# Sys.Rules.DmnEngine Upstream Sync Ledger

## Local Baseline

- Upstream project: `ScratchyDisk.DmnEngine`
- Upstream repository: `https://github.com/Scratchydisk/ScratchyDisk.DmnEngine`
- Local upstream cache: `d:\Project\MDD\9.0\nactivity\_upstream\ScratchyDisk.DmnEngine`
- First imported upstream commit: `67bae6927a28f77138aae98dcd35d93b1af39838`
- Local imported root: `NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine\Upstream`
- Namespace rewrite policy: `ScratchyDisk.DmnEngine.* -> Sys.Rules.DmnEngine.*`
- Antlr runtime pin: `Antlr4.Runtime.Standard 4.9.3`

## Latest Observed Upstream Snapshot

- Upstream changelog version seen in local cache: `2.0.1`
- Upstream changelog date seen in local cache: `2026-02-23`
- Source used for inspection:
  - `_upstream\ScratchyDisk.DmnEngine\README.md`
  - `_upstream\ScratchyDisk.DmnEngine\changelog.md`

## Sync Workflow

1. Refresh the local upstream cache manually.
2. Run `NActiviti\scripts\scan-rule-dmn-upstream.ps1`.
3. Review the generated report in `artifacts\reports\rule-dmn-upstream-sync`.
4. Assess breaking changes, DMN compatibility impact, and FEEL behavior changes.
5. If accepted, create a local import batch and rerun:
   - DMN XML compatibility tests
   - key decision table regression tests
   - BusinessRuleTask integration regression tests
6. Update this ledger with the accepted upstream baseline and local decision.

## Current Status

- Status: `Observed`
- Local decision: `No second sync batch imported yet`
- Notes:
  - The local cache shows upstream release `2.0.1`.
  - A formal local sync gate now exists, but the next import batch still requires manual review and regression validation.
