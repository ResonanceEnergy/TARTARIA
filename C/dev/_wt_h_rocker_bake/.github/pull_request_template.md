# Pull Request Description

## Summary
<!-- Brief description of changes -->

## Type of Change
<!-- Mark with [x] -->
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to not work as expected)
- [ ] Performance improvement
- [ ] Code refactoring (no functional changes)
- [ ] Documentation update
- [ ] Build/CI pipeline change

## Related Issues
<!-- Link to GitHub issues this PR addresses -->
Fixes #
Closes #

## Changes Made
<!-- Detailed list of changes -->
-
-
-

## Testing Performed
<!-- How were changes tested? -->
- [ ] Ran `.\tartaria-play.ps1` — CS:0, all checks passed
- [ ] Tested in Unity Editor play mode
- [ ] Tested with keyboard + mouse
- [ ] Tested with gamepad (F310/Xbox/PlayStation)
- [ ] Verified no console errors or warnings
- [ ] Unity Profiler checked (no GC allocations in hot paths)
- [ ] Tested on hardware tier: <!-- Minimum / Recommended / High / Ultra -->

## Screenshots / Videos
<!-- If visual changes, attach screenshots or video -->

## Code Quality Checklist
<!-- Confirm all items before submitting PR -->
- [ ] Code compiles with zero errors (CS:0)
- [ ] Code compiles with zero warnings
- [ ] Follows project coding standards (see CONTRIBUTING.md)
- [ ] No per-frame allocations in `Update()` / `FixedUpdate()` / `LateUpdate()`
- [ ] All `GetComponent` calls cached in `Awake()`
- [ ] Used Unity 6 API (`FindFirstObjectByType` not `FindObjectOfType`)
- [ ] Respects assembly boundaries (no Gameplay → Integration refs)
- [ ] Comments on non-obvious logic
- [ ] No `TODO` or `FIXME` comments left unresolved
- [ ] XML documentation on public APIs

## Performance Impact
<!-- How does this affect performance? -->
- [ ] No measurable performance impact
- [ ] Performance improvement: <!-- describe -->
- [ ] Performance regression justified: <!-- explain why necessary -->

## Documentation
<!-- Did you update relevant documentation? -->
- [ ] Updated README.md (if applicable)
- [ ] Updated BUILD_GUIDE.md (if build changes)
- [ ] Updated TROUBLESHOOTING.md (if new issues may arise)
- [ ] Updated CHANGELOG.md
- [ ] Updated GDD docs in `docs/` (if design changes)
- [ ] No documentation needed

## Additional Notes
<!-- Any other context reviewers should know -->
