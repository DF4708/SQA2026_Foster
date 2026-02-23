## Scenario 1
- FR/Rule: BR-5 / FR-11 Deposit minimum (minDeposit=200) — boundary + missing
- Given: Valid baseline applicant; product requires deposit; vary depositAmount only
- When: Run(applicant) reaches final processing (deposit rule evaluated inside Process)
- Then: depositAmount < 200 ⇒ Status=Cancelled; depositAmount ≥ 200 -> Status=Approved; deposit missing -> Status=Incomplete
- Trace To (Use Case Step / BR / AC): BR-5 / FR-11
- Technique (Control Flow / Data Flow / Domain): Domain
- Source: Bank4Us New Account TDD Completed List.xlsx -> TDD_Template / TechniqueMap

## Scenario 2
- FR/Rule: FR-05 Validate identificationNumber format by identifierType (SSN)
- Given: Valid applicant with IdentifierType=SSN; vary IdentificationNumber only
- When: Run(applicant) performs ValidateIdentificationNumber(applicant)
- Then: Invalid SSN format ⇒ ValidationError("Invalid SSN format") and Status=Incomplete (short-circuit)
- Trace To (Use Case Step / BR / AC): FR-05
- Technique (Control Flow / Data Flow / Domain): Control Flow
- Source: Bank4Us New Account TDD Completed List.xlsx -> TDD_Template / TechniqueMap

## Scenario 3
- FR/Rule: AC-5 / BR-1 Application must be complete (ID required)
- Given: Valid applicant except IdentificationNumber is null/empty
- When: Run(applicant) reaches Process(applicant)
- Then: Status=Cancelled (blocked) and error indicates missing/invalid ID
- Trace To (Use Case Step / BR / AC): AC-5 / BR-1
- Technique (Control Flow / Data Flow / Domain): Data Flow
- Source: Bank4Us New Account TDD Completed List.xlsx -> TDD_Template / TechniqueMap

## Scenario 4
- FR/Rule: BR-3 Residency verification service unavailable → PendingVerification
- Given: Valid applicant w/ residency doc; residency service stub throws Timeout/ServiceUnavailable
- When: Run(applicant) reaches residency verification (modeled as EvaluateCitizenship)
- Then: Status=PendingVerification AND final processing (Process) is not called
- Trace To (Use Case Step / BR / AC): BR-3
- Technique (Control Flow / Data Flow / Domain): Control Flow
- Source: Bank4Us New Account TDD Completed List.xlsx -> TDD_Template / TechniqueMap

## Scenario 5
- FR/Rule: BR-1/AC-5 Address completeness: street, city, state, postalCode required
- Given: Valid applicant except Address.PostalCode missing
- When: Run(applicant) performs ValidateAddress(applicant)
- Then: Status=Incomplete with message mentioning Postal/Zip required
- Trace To (Use Case Step / BR / AC): BR-1 / AC-5
- Technique (Control Flow / Data Flow / Domain): Data Flow
- Source: Bank4Us New Account TDD Completed List.xlsx → TDD_Template / TechniqueMap
