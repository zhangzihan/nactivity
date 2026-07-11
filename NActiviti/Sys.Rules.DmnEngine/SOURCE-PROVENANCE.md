# Sys.Rules.DmnEngine Source Provenance

- Upstream project: `ScratchyDisk.DmnEngine`
- Upstream repository: `https://github.com/Scratchydisk/ScratchyDisk.DmnEngine`
- Local upstream cache: `d:\Project\MDD\9.0\nactivity\_upstream\ScratchyDisk.DmnEngine`
- First-batch upstream commit:
  - `67bae6927a28f77138aae98dcd35d93b1af39838`
- First-batch imported directories:
  - `engine/`
  - `feel/`
  - `parser/`
  - `utils/`
- Local imported root:
  - `NActiviti\Sys.Rules.DmnEngine\src\Sys.Rules.DmnEngine\Upstream`
- License file copied to:
  - `NActiviti\Sys.Rules.DmnEngine\license-scratchydisk-dmnengine.txt`

## Notes

- First-batch import uses source vendoring rather than direct `NuGet` dependency.
- Imported namespaces are mechanically rewritten from `ScratchyDisk.DmnEngine.*` to `Sys.Rules.DmnEngine.*`.
- `Antlr4.Runtime.Standard` is intentionally pinned to `4.9.3` to match `Sys.Expression`.
- This file should be updated again after tagging the first internal stable baseline derived from the upstream commit above.
- Ongoing sync decisions are tracked in:
  - `NActiviti\Sys.Rules.DmnEngine\UPSTREAM-SYNC-LEDGER.md`
