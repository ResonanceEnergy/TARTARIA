# Butler Credentials Setup - SECURE workflow

> Sprint 10 Lane 9. One-page operator note for getting an itch.io API key
> onto this machine in a way `scripts/build-itch.ps1` can pick up
> **without ever committing the key to the repo.**
>
> Sibling doc: `docs/release/BUTLER_SETUP.md` (Sprint 9 Lane 4) covers the
> butler CLI install + project page + push command. This doc is strictly
> about the credentials.

---

## STRONG WARNING - read first

The itch.io API key is the equivalent of your account password for
upload purposes. Treat it like one.

- **NEVER commit the key.** Not to this repo, not to any repo, not to a
  gist, not to a Notion page that syncs to GitHub.
- **NEVER paste the key into chat.** Not into Slack, not into a Claude
  conversation, not into a screenshot. If you paste it, rotate it.
- **NEVER put the key in `CLAUDE.md`, `STATUS.md`, or any `.md` file in
  the repo.** Future agents will dump those into context windows.
- **NEVER log the key value.** `scripts/build-itch.ps1` logs the *source*
  of the key (`env var` vs `.local-secrets/itch_key.txt`) but never the
  key itself. Keep it that way.

If you suspect the key has leaked: itch.io -> User menu -> Settings ->
API keys -> **Revoke** -> generate a new one. Cheap to rotate, expensive
to recall a leak.

---

## Three ways to store the key (pick one)

### Option 1 - `butler login` (recommended for desktop dev)

One-time interactive flow. Opens a browser, you click "authorize", the
key is saved to butler's own credential cache (`%USERPROFILE%\.config\itch\butler_creds`).

```powershell
butler login
# Browser opens. Approve the request. Close the tab when it says "you can close this window".
```

The build script does NOT read this file directly - butler picks it up
automatically when it runs. This is the cleanest option because the key
never touches your shell history or environment.

**Verify:**

```powershell
butler status resonanceenergy/tartaria-aether-awakening
```

Should list channels (empty before the first push, populated after).

### Option 2 - `ITCH_API_KEY` env var (recommended for headless / CI / repeated runs)

Get a key from itch.io directly:

1. https://itch.io/user/settings/api-keys
2. Click **Generate new API key**.
3. Copy the key (a long hex/base64 string).

Store it as a **User-scoped** Windows env var (NOT System - keeps it off
shared admin processes):

```powershell
[Environment]::SetEnvironmentVariable("ITCH_API_KEY", "your_key_here", "User")
# Close + reopen your terminal so the new env var is visible.
```

Verify:

```powershell
# Print only that it's set, never the value:
if ($env:ITCH_API_KEY) { "ITCH_API_KEY is set ($(($env:ITCH_API_KEY).Length) chars)" } else { "NOT set" }
```

`scripts/build-itch.ps1` reads `$env:ITCH_API_KEY` first. When found, it
copies the value into `$env:BUTLER_API_KEY` (butler's own documented env
var) for the duration of the push, then proceeds. The value is never
written to a log or a file.

### Option 3 - `.local-secrets/itch_key.txt` (fallback - laptop without env vars)

Same pattern as the existing `.local-secrets/github_pat.txt` workflow.

```powershell
New-Item -ItemType Directory -Force -Path C:\dev\TARTARIA_new\.local-secrets | Out-Null
# Paste the key (no trailing newline, no quotes):
Set-Content -Path C:\dev\TARTARIA_new\.local-secrets\itch_key.txt -Value "your_key_here" -NoNewline -Encoding ascii
# Lock it down to the current user:
icacls C:\dev\TARTARIA_new\.local-secrets\itch_key.txt /inheritance:r /grant:r "$($env:USERNAME):(R,W)"
```

The folder `.local-secrets/` is already gitignored (verified - see the
last line of `.gitignore`). Do NOT git-add anything inside it.

`scripts/build-itch.ps1` reads this file only if `ITCH_API_KEY` env var
is not set.

---

## How the build script picks the key

Lookup order, top wins (see `Get-ItchApiKey` in `scripts/build-itch.ps1`):

1. `$env:ITCH_API_KEY`                        -> source = `env:ITCH_API_KEY`
2. `C:\dev\TARTARIA_new\.local-secrets\itch_key.txt`  -> source = `file:.local-secrets/itch_key.txt`
3. Neither -> assume `butler login` was used. If butler has no cached
   creds either, butler itself will fail with `unauthorized` and the
   script exits 4 with a pointer to this doc.

Once found, the key is exported to `$env:BUTLER_API_KEY` for the child
butler process. Butler reads `BUTLER_API_KEY` natively (see
https://itch.io/docs/butler/login.html). The script logs the *source*
only:

```
[BuildItch] Loaded ITCH_API_KEY from env:ITCH_API_KEY
# or
[BuildItch] Loaded ITCH_API_KEY from file:.local-secrets/itch_key.txt
# or (when neither is set)
[BuildItch] No ITCH_API_KEY in env or .local-secrets - relying on butler login cache
```

---

## Verification - the full handshake

After picking an option above, smoke-test:

```powershell
# (1) Confirm the key is reachable to butler:
butler status resonanceenergy/tartaria-aether-awakening
# Expected: a channel listing, or "no builds yet". NOT "unauthorized".

# (2) Confirm the build script picks it up:
cd C:\dev\TARTARIA_new
.\scripts\build-itch.ps1 -DryRun -SkipBuild -SkipCapture
# Look for: "[BuildItch] Loaded ITCH_API_KEY from <source>"
# Then:     "DRY RUN - would invoke: butler push ..."
# Exit 0.
```

If step (1) returns `unauthorized`, the key is wrong or revoked - go
back to option 1/2/3 and re-store.

If step (2) doesn't print the `Loaded ITCH_API_KEY from ...` line, the
script is older than this doc - re-pull from `agent/release/butler-creds-doc`.

---

## What lives where

| Location                                     | What                                                  | Gitignored? |
| -------------------------------------------- | ----------------------------------------------------- | ----------- |
| `$env:ITCH_API_KEY` (User scope)             | Preferred for headless / repeated runs                | n/a (env)   |
| `%USERPROFILE%\.config\itch\butler_creds`    | Butler's own cache from `butler login`                | n/a (home)  |
| `.local-secrets\itch_key.txt`                | Fallback file the build script reads                  | YES         |
| `.local-secrets\github_pat.txt`              | Existing GitHub PAT (same pattern, different service) | YES         |
| `scripts\build-itch.ps1`                     | Reads (1) env, (2) file. NEVER logs the value.        | (committed) |
| `docs\release\BUTLER_SETUP.md`               | butler CLI install + push command                     | (committed) |
| `docs\release\BUTLER_CREDS_SETUP.md` (this)  | Credentials policy + storage                          | (committed) |

---

## If a key leaks

1. itch.io -> Settings -> API keys -> **Revoke** the leaked key.
2. Generate a fresh one, store it via option 1/2/3.
3. `git log -p` the leak commit if it ever hit a repo -> scrub via
   `git filter-repo` -> force-push -> notify any clones.
4. Note the rotation in `CHANGELOG.md` (date + "rotated itch API key",
   without details).

---

*Authored Sprint 10 Lane 9. Update when butler env-var semantics or the
.local-secrets/ pattern changes.*
