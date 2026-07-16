# Working Knowledge Layer Toolkit Release

Maintainer checklist for publishing the standalone toolkit archive. This is separate from publishing Working Knowledge itself or a generated compatibility layer.

## Current Published Release

- Release: [Working Knowledge Layer Toolkit 1.1.0](https://github.com/abflett/SE-BeneathTheCrust/releases/tag/v1.1.0)
- Direct asset: [WorkingKnowledgeLayerToolkit-1.1.0.zip](https://github.com/abflett/SE-BeneathTheCrust/releases/download/v1.1.0/WorkingKnowledgeLayerToolkit-1.1.0.zip)
- General release history: [SE-BeneathTheCrust Releases](https://github.com/abflett/SE-BeneathTheCrust/releases)

## Release Identity

- Update `tools/WorkingKnowledgeLayerToolkit/VERSION.txt` using `major.minor.patch`.
- Add matching notes to `tools/WorkingKnowledgeLayerToolkit/CHANGELOG.md`.
- Keep the version shown near the top of the toolkit README synchronized.
- Use the archive name `WorkingKnowledgeLayerToolkit-<version>.zip`.

## Validation And Packaging

From the repository root, run:

```powershell
.\tools\package-working-knowledge-layer-toolkit.ps1
```

The script stages a top-level `WorkingKnowledgeLayerToolkit` folder, creates the zip under `.tmp/releases/`, extracts it again, runs the packaged generator self-test, and validates the packaged `ExampleMod`.

Also run the current Working Knowledge release validator. It compiles the mod, tests the source toolkit, validates maintained layers, runs priority-resolution fixtures, and builds the standalone archive:

```powershell
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 0.13.0
```

Open the final archive once and confirm it contains one top-level `WorkingKnowledgeLayerToolkit` folder and no local logs, test worlds, downloaded mods, or repository metadata.

## Publication Gates

- Select and add an explicit repository/toolkit license before the next public toolkit release. The repository currently has no license file; this is a maintainer policy decision and must not be inferred by tooling.
- Commit and push all source changes before creating the release.
- Create or update the GitHub release for the toolkit version.
- Attach the validated `.tmp/releases/WorkingKnowledgeLayerToolkit-<version>.zip` without renaming its internal folder.
- Verify the uploaded archive can be downloaded, extracted, started with `Start.bat`, and validated on a clean path.
- Keep the toolkit README linked to the releases page. Add a version-specific direct link only after the asset exists.
