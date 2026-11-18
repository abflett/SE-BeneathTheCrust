# Working Knowledge Layer Toolkit Release

Maintainer checklist for publishing the standalone toolkit archive. This is separate from publishing Working Knowledge itself or a generated compatibility layer.

## Current Published Release

- Release: [Working Knowledge Layer Toolkit 1.2.0](https://github.com/abflett/SE-BeneathTheCrust/releases/tag/v1.2.0)
- Direct asset: [WorkingKnowledgeLayerToolkit-1.2.0.zip](https://github.com/abflett/SE-BeneathTheCrust/releases/download/v1.2.0/WorkingKnowledgeLayerToolkit-1.2.0.zip)
- General release history: [SE-BeneathTheCrust Releases](https://github.com/abflett/SE-BeneathTheCrust/releases)

## Prepared Next Release

Toolkit `1.3.0` updates the complete 58-family catalog for Working Knowledge 1.2.x / Warfare 3 and generates stat-free custom schematics. Its validated archive is `.tmp/releases/WorkingKnowledgeLayerToolkit-1.3.0.zip`.

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
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.2.0
```

The release validator also compares the generated Working Knowledge research metadata against the Toolkit catalog. It fails if any active built-in schematic group is missing or if a display name or tier differs. Priority/fallback fixtures use temporary fake layers and suppress their expected invalid-case diagnostics; only their pass/fail result appears in normal release output.

Open the final archive once and confirm it contains one top-level `WorkingKnowledgeLayerToolkit` folder and no repository-only `Tests` directory, local logs, test worlds, downloaded mods, or repository metadata.

## Publication Gates

- Include the toolkit's maintainer-approved MIT license (`tools/WorkingKnowledgeLayerToolkit/LICENSE.txt`) in every archive. This license covers the standalone toolkit; it does not declare a license for the rest of the repository.
- Commit and push all source changes before creating the release.
- Create or update the GitHub release for the toolkit version.
- Attach the validated `.tmp/releases/WorkingKnowledgeLayerToolkit-<version>.zip` without renaming its internal folder.
- Verify the uploaded archive can be downloaded, extracted, started with `Start.bat`, and validated on a clean path.
- Keep the toolkit README linked to the releases page. Add a version-specific direct link only after the asset exists.
