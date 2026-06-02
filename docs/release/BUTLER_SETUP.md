# Butler Setup - itch.io Upload

> Sprint 9 Lane 4. One-page operator note for getting `butler` working so
> `scripts/build-itch.ps1` can push a Moon 1 build to itch.io end-to-end.

---

## 1. Install butler

The recommended install is via the **itch.io desktop app**:

1. Download and install the itch.io app: https://itch.io/app
2. Open the app, log in with your itch.io account.
3. Settings -> Install butler (the app vendors butler and keeps it up to date).
4. Confirm butler is on `PATH`:

    ```powershell
    butler -V
    # -> butler v15.x.x, ...
    ```

If `butler` is not on `PATH` after the itch app install, add it manually -
the typical path is `%LOCALAPPDATA%\itch\apps\butler\butler.exe`. The
`build-itch.ps1` script falls back to that location automatically, but
having it on `PATH` makes `butler status` etc. work from any shell.

Manual / no-app installs are documented upstream:
**https://itch.io/docs/butler/installing.html**

---

## 2. Generate an API key and log in

1. itch.io -> User menu -> Settings -> **API keys** tab.
2. Click **Generate new API key**, copy the secret.
3. Run once on this machine:

    ```powershell
    butler login
    # Paste the API key when prompted.
    ```

Credentials are stored at `%USERPROFILE%\.config\itch\butler_creds`. The
build chain does NOT pass the key on the command line and does NOT read
the file - it relies on butler's own credential cache.

---

## 3. Create the itch.io project page (one-time)

Butler can only push to a project that already exists on itch.io.

1. itch.io -> Dashboard -> **Create new project**.
2. Title: TARTARIA WORLD OF WONDER - Aether Awakening (Moon 1: Echohaven).
3. URL slug: this becomes the `<project>` half of the target. The build
   script defaults to `resonanceenergy/tartaria-aether-awakening`. Update
   the `-ItchTarget` parameter or the script default if your slug differs.
4. Kind: HTML / Downloadable. Pick **Downloadable** for the Windows zip.
5. Save as **Draft** until first successful upload.

The lore/marketing copy for the page itself lives in
`docs/marketing/itch_page_draft.md` (Sprint 6 Lane 8). Run the political-
risk checklist at the bottom of that draft before publishing.

---

## 4. Verify your install

```powershell
butler status resonanceenergy/tartaria-aether-awakening
```

Expected output before the first push: a single line saying no builds yet
for any channel. After step 5, you'll see one row per channel with size,
upload date, and version.

If butler reports `404 Not Found`, the project page does not exist or the
slug is wrong - re-check step 3.

---

## 5. The push command

`scripts/build-itch.ps1` generates and runs this exact command at step 6:

```powershell
butler push `
    "Builds/itch_assets/TARTARIA_Moon1.zip" `
    "resonanceenergy/tartaria-aether-awakening:moon1-windows" `
    --userversion-file "Builds/itch_assets/build_manifest.txt"
```

Override target/channel at invoke time:

```powershell
.\scripts\build-itch.ps1 `
    -ItchTarget "natrix/tartaria-aether-awakening" `
    -Channel    "moon1-windows-beta"
```

---

## 6. Channel naming convention

One channel per platform per Moon. Use lowercase, hyphenated.

| Channel                | Purpose                                    |
| ---------------------- | ------------------------------------------ |
| `moon1-windows`        | Moon 1 alpha, Windows x64 (current target) |
| `moon1-windows-beta`   | Pre-release Moon 1 testing builds          |
| `moon2-windows`        | Future, when Moon 2 ships                  |
| `moon3-windows`        | Future, etc.                               |

The itch.io download page shows one button per channel - keep them
descriptive so players know which Moon they're getting.

---

## 7. The `--userversion-file` mechanic

Butler stamps each upload with a "user version" string visible on the
itch page (in the download list and in `butler status` output). When
`--userversion-file` is passed, butler reads the **entire file contents**
(stripped) and uses that as the version string.

The build chain writes a single-line version into
`Builds/itch_assets/build_manifest.txt` at step 5, formatted as:

```
0.4.0-moon1-YYYYMMDD-HHMM-<8charGitSha>
```

The original detailed manifest produced by `Moon1ItchBuild.cs` is
preserved alongside as `Builds/itch_assets/build_manifest_detail.txt` -
inspect that file for build target, scenes, errors, warnings, duration.

If you need a different version string scheme, edit step 5 in
`scripts/build-itch.ps1` (look for `$version = "0.4.0-moon1-...`").

---

## 8. Troubleshooting

| Symptom                                       | Likely cause                                                    |
| --------------------------------------------- | --------------------------------------------------------------- |
| `butler: command not found`                   | step 1 incomplete - reinstall itch app, restart shell.          |
| `unauthorized`                                | API key revoked or expired - re-run `butler login`.             |
| `404 not found`                               | Project slug wrong - re-check step 3, fix `-ItchTarget`.        |
| `build-manifest.txt` content shows in version | step 5 was skipped - run without `-SkipBuild`.                  |
| `the file is too large`                       | itch single-file cap is ~30 GB; check the smoke-test zip-size.  |
| Push hangs forever                            | Network or itch outage - check https://itch.io status, retry.   |

---

## 9. Where this fits in the build chain

```
scripts/build-itch.ps1
   step 1 - env capture
   step 2 - Unity batchmode: Moon1ItchBuild.BuildWin64           -> Builds/Win64/, .zip, manifest
   step 3 - Unity batchmode: Moon1ItchScreenshotCapture.Capture* -> shot_00..shot_07_*.png
   step 4 - locate butler.exe (this doc covers install)
   step 5 - rewrite build_manifest.txt with butler-friendly version
   step 6 - butler push <zip> <target>:<channel> --userversion-file <manifest>
```

`scripts/dev/itch-smoke-test.ps1` (Sprint 7 Lane 9) covers steps 2-3 plus
zip+PNG validation and is the right tool for CI / local verification when
you don't want to actually upload. `build-itch.ps1` is the full release
chain that ends with a real itch.io push.

---

*Authored Sprint 9 Lane 4. Update when the build chain or butler CLI semantics change.*
