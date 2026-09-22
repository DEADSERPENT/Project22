# Project22 — Laboratory Order Intake and Input Validation Service

A C# class library that accepts one laboratory order as a JSON string, validates it against the rules in the
assessment specification, and returns a structured result: `Accepted` with the parsed order, `Rejected` with
the full set of validation errors, or `Rejected` with a single `MALFORMED_INPUT` error when the input cannot
be read as an order at all.

## 1. Build and test

Requires the .NET 10 SDK (verified against `10.0.401`).

```
dotnet build
dotnet test
```

Both run cleanly from the repository root against `Project22.slnx`. `dotnet build` reports zero warnings and
zero errors; `dotnet test` runs the full required suite (21 test cases across the six groups defined in the
specification) with zero failures.

## 2. Project layout

```
src/OrderIntake/            Class library — the service under assessment
  Models.cs                   Public result/order types (OrderResult, Order, ValidationError, enums)
  RawOrder.cs                 Internal DTO used only for JSON deserialization
  Validation.cs                Field-level validation rules
  OrderIntakeService.cs        Public entry point: OrderResult Process(string json)

tests/OrderIntake.Tests/    xUnit test project, one file per required test group
```

## 3. Design summary

`Process(string json)` is the single public entry point, and delegates in sequence:

1. **Deserialize.** `System.Text.Json` reads the input into an internal `RawOrder` DTO. Deserialization is
   wrapped in a single `try/catch(JsonException)`. This one boundary handles broken JSON syntax, a top-level
   array/string/number, and any recognized field holding an incompatible JSON type (e.g. `{"orderId": 123}`)
   — all of these are cases `System.Text.Json` already rejects with a `JsonException` by default, so no
   separate hand-rolled type-checking was needed. Null, empty, or whitespace-only input is checked explicitly
   before deserialization is attempted. Every one of these paths returns a single `MALFORMED_INPUT` error with
   field `$`, and the input `{}` is confirmed to fall through to ordinary field validation rather than this
   path, as required.

2. **Validate.** `OrderValidator.Validate` runs one check per field — `orderId`, `patientId`, `specimenId`,
   `specimenType`, `priority`, `collectionDate`, `requestedTests` — unconditionally, so every broken rule is
   collected in a single pass rather than stopping at the first failure. `specimenType` and `priority` are
   matched case-insensitively via `Enum.TryParse` and stored as enums, which also gives the fixed canonical
   form required by the spec for free. `collectionDate` is parsed using `DateOnly.TryParseExact` with the
   required `yyyy-MM-dd` format, rejecting incorrectly formatted and invalid calendar dates (e.g. 30
   February); the future-date check runs only after successful parsing.

3. **Build the result.** If no errors were collected, `Validate` returns the built `Order` (a plain C# record
   holding typed values — a real `DateOnly`, not a string — never the raw JSON or an untyped dictionary) and
   the service returns `Accepted`. Otherwise it returns `Rejected` with the complete error list.

Unrecognized fields (e.g. `senderNote`) are never rejected: `RawOrder` only declares the seven recognized
properties, and `System.Text.Json` ignores unmapped JSON properties by default, so they are silently dropped
before validation ever sees them.

No console input, file access, HTTP, database, UI, dependency-injection framework, or third-party validation
library is used anywhere in the solution, per the assessment's stated scope.

## 4. Assumptions and limitations

- **No trimming.** String values (IDs, enum inputs, test names) are validated and stored exactly as received,
  with no leading/trailing whitespace trimming. This is applied consistently across every field.
- **`requestedTests` type mismatches treated as malformed.** The specification's only worked example of a
  type-incompatible field is a scalar (`{"orderId": 123}`). The same "recognized fields must have compatible
  JSON types" rule was applied consistently to `requestedTests`: if it is not a JSON array of strings (wrong
  shape, or an array containing non-string items), the order is rejected as `MALFORMED_INPUT` rather than as a
  field-level error. This follows directly from typing the field as `List<string>?` in the deserialization
  DTO and required no additional code.
- **Enum values reject numeric strings.** `Enum.TryParse` alone would accept a bare number such as `"0"` as a
  valid `specimenType`, since that is a legal way to represent the underlying enum value. An explicit guard
  rejects any numeric input as `INVALID_VALUE`, since no numeric string is a legitimate specimen type or
  priority under the specification.
- **Single-error validation per field.** Each field reports at most one error per request (e.g. an empty
  `orderId` reports `REQUIRED`, not also `MAX_LENGTH`). This is consistent with the fact that the underlying
  conditions are mutually exclusive for every field in this specification.
- **Not covered, by design:** persistence, authentication, an HTTP surface, and logging — all explicitly out
  of scope for this assessment.

## 5. AI use

AI assistance (Claude Code) was used during development, consistent with the assessment's permitted-tools
policy. One suggestion was specifically evaluated and rejected: a static-analysis pass flagged `Process`
(the public entry point) with `CA1822`, since in its current form it does not read or write any instance
state and could technically be declared `static`. This was reviewed and intentionally kept as an instance
method: the assessment specifies a service with a public processing method, and no static state or
dependency injection is required to satisfy that. The warning is suppressed at the method with an inline
comment recording this reasoning, rather than silently applied or silently ignored.

All test data used throughout (order IDs, patient IDs, specimen IDs, dates) is fictional, per the
specification's requirement.
