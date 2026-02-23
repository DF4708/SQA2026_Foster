# FIRSTHAND Improvements (Tibi book, Chapter 6)

Documented changes applied while writing the unit tests for `AccountOpeningAutomation.Run()`.

## Improvement 1 — Intention
- What I changed:
  - Named each test as `Run_<condition>_<expected_outcome>` and kept the assertions scoped to one outcome.
- Why it aligns with FIRSTHAND (Tibi Ch. 6):
  - Clear intent makes it obvious which domain/branch is being exercised, and it keeps the tests from drifting into “multi-purpose” checks.
- Evidence (file/test name / commit / snippet):
  - `tests/Bank4Us.AccountOpening.Tests.Unit/AccountOpeningAutomationTests.cs`
  - Examples:
    - `Run_invalid_ssn_format_short_circuits_as_incomplete`
    - `Run_residency_verification_unavailable_returns_pending_verification_and_does_not_process`

## Improvement 2 — Readability
- What I changed:
  - Used a canonical fixture (`ApplicantFactory.CreateValid()`) and only overrode the single field under test using record `with { ... }`.
  - Added tiny helpers (`Errors(...)` and `AddressApproved`) so each test reads like Arrange/Act/Assert instead of setup boilerplate.
- Why it aligns with FIRSTHAND (Tibi Ch. 6):
  - Reduces incidental complexity, so the test’s domain condition (e.g., “postal code missing”, “deposit just below 200”) is visually prominent.
- Evidence:
  - `AccountOpeningAutomationTests.cs`:
    - `private static ProcessResult AddressApproved => ...`
    - `private static IReadOnlyList<ValidationError> Errors(params string[] messages) => ...`

## Improvement 3 — Single-Behavior / No Interdependency / Deterministic
- What I changed:
  - Mocked the workflow seam (`IAccountOpeningWorkflow`) with NSubstitute so the runner is tested in isolation.
  - Added interaction assertions (`DidNotReceive`) to prove short-circuit behavior (no hidden dependencies on later workflow steps).
- Why it aligns with FIRSTHAND (Tibi Ch. 6):
  - Keeps tests repeatable and deterministic by eliminating external services, timing, and shared state.
- Evidence:
  - `Run_residency_verification_unavailable_returns_pending_verification_and_does_not_process` verifies `Process()` is not called.
  - `Run_missing_postal_code_returns_incomplete_and_skips_citizenship_and_process` verifies both `EvaluateCitizenship()` and `Process()` are not called.

## Improvement 4 — Domain Boundary Coverage (ON/OFF points)
- What I changed:
  - Implemented the deposit minimum scenario as a data-driven boundary test: 199 (OFF), 200 (ON), 201 (IN), and `null` (missing).
- Why it aligns with FIRSTHAND (Tibi Ch. 6):
  - Chapter 6 domain testing emphasizes selecting points on/near boundaries instead of “random” interior values. The chosen values target the decision boundary at 200.
- Evidence:
  - `Run_deposit_minimum_boundary_and_missing_values_drive_terminal_status` with `DepositCases` MemberData.
