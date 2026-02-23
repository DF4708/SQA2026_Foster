using System;
using System.Collections.Generic;
using System.Linq;
using Bank4Us.AccountOpening;
using NSubstitute;
using Xunit;

namespace Bank4Us.AccountOpening.Tests.Unit;

public sealed class AccountOpeningAutomationTests
{
    private static ProcessResult AddressApproved => new(ApplicationStatus.Approved, Array.Empty<ValidationError>());

    private static IReadOnlyList<ValidationError> Errors(params string[] messages)
        => messages.Select(m => new ValidationError(m)).ToArray();

    public static IEnumerable<object?[]> DepositCases => new[]
    {
        new object?[] { 199m, ApplicationStatus.Cancelled }, // OFF
        new object?[] { 200m, ApplicationStatus.Approved },  // ON
        new object?[] { 201m, ApplicationStatus.Approved },  // IN
        new object?[] { null, ApplicationStatus.Incomplete } // missing
    };

    [Theory]
    [MemberData(nameof(DepositCases))]
    public void Run_deposit_minimum_boundary_and_missing_values_drive_terminal_status(decimal? depositAmount, ApplicationStatus expected)
    {
        // Arrange
        var applicant = ApplicantFactory.CreateValid() with { OpeningDepositAmount = depositAmount };

        var workflow = Substitute.For<IAccountOpeningWorkflow>();
        workflow.ValidateIdentificationNumber(Arg.Any<Applicant>()).Returns(Array.Empty<ValidationError>());
        workflow.ValidateAddress(Arg.Any<Applicant>()).Returns(AddressApproved);
        workflow.EvaluateCitizenship(Arg.Any<Applicant>()).Returns(ApplicationStatus.Approved);

        workflow.Process(Arg.Any<Applicant>()).Returns(callInfo =>
        {
            var a = callInfo.Arg<Applicant>();
            return a.OpeningDepositAmount switch
            {
                null => new ProcessResult(ApplicationStatus.Incomplete, Errors("Opening deposit is required")),
                < 200m => new ProcessResult(ApplicationStatus.Cancelled, Errors("Opening deposit below minimum")),
                _ => new ProcessResult(ApplicationStatus.Approved, Array.Empty<ValidationError>())
            };
        });

        var sut = new AccountOpeningAutomation(workflow);

        // Act
        var result = sut.Run(applicant);

        // Assert
        Assert.Equal(expected, result.Status);
        workflow.Received(1).Process(Arg.Is<Applicant>(a => ReferenceEquals(a, applicant)));
    }

    public static IEnumerable<object[]> InvalidSsnCases => new[]
    {
        new object[] { "123" },
        new object[] { "123-45-678" },
        new object[] { "ABC-DE-FGHI" }
    };

    [Theory]
    [MemberData(nameof(InvalidSsnCases))]
    public void Run_invalid_ssn_format_short_circuits_as_incomplete(string invalidIdentificationNumber)
    {
        // Arrange
        var applicant = ApplicantFactory.CreateValid() with
        {
            IdentifierType = IdentifierType.SSN,
            IdentificationNumber = invalidIdentificationNumber
        };

        var workflow = Substitute.For<IAccountOpeningWorkflow>();
        workflow.ValidateIdentificationNumber(Arg.Is<Applicant>(a => a.IdentificationNumber == invalidIdentificationNumber))
            .Returns(Errors("Invalid SSN format"));

        var sut = new AccountOpeningAutomation(workflow);

        // Act
        var result = sut.Run(applicant);

        // Assert
        Assert.Equal(ApplicationStatus.Incomplete, result.Status);
        Assert.Contains(result.Errors, e => e.Message.Contains("Invalid SSN format", StringComparison.OrdinalIgnoreCase));

        workflow.Received(1).ValidateIdentificationNumber(Arg.Any<Applicant>());
        workflow.DidNotReceive().ValidateAddress(Arg.Any<Applicant>());
        workflow.DidNotReceive().EvaluateCitizenship(Arg.Any<Applicant>());
        workflow.DidNotReceive().Process(Arg.Any<Applicant>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Run_missing_identificationNumber_is_cancelled_by_process(string? id)
    {
        // Arrange
        var applicant = ApplicantFactory.CreateValid() with { IdentificationNumber = id };

        var workflow = Substitute.For<IAccountOpeningWorkflow>();

        // In this scenario, syntactic validation does not block. Completeness is enforced in Process().
        workflow.ValidateIdentificationNumber(Arg.Any<Applicant>()).Returns(Array.Empty<ValidationError>());
        workflow.ValidateAddress(Arg.Any<Applicant>()).Returns(AddressApproved);
        workflow.EvaluateCitizenship(Arg.Any<Applicant>()).Returns(ApplicationStatus.Approved);

        workflow.Process(Arg.Any<Applicant>()).Returns(callInfo =>
        {
            var a = callInfo.Arg<Applicant>();
            return string.IsNullOrWhiteSpace(a.IdentificationNumber)
                ? new ProcessResult(ApplicationStatus.Cancelled, Errors("ID is required"))
                : new ProcessResult(ApplicationStatus.Approved, Array.Empty<ValidationError>());
        });

        var sut = new AccountOpeningAutomation(workflow);

        // Act
        var result = sut.Run(applicant);

        // Assert
        Assert.Equal(ApplicationStatus.Cancelled, result.Status);
        Assert.Contains(result.Errors, e => e.Message.Contains("ID", StringComparison.OrdinalIgnoreCase));
        workflow.Received(1).Process(Arg.Is<Applicant>(a => ReferenceEquals(a, applicant)));
    }

    [Fact]
    public void Run_residency_verification_unavailable_returns_pending_verification_and_does_not_process()
    {
        // Arrange
        var applicant = ApplicantFactory.CreateValid();

        var workflow = Substitute.For<IAccountOpeningWorkflow>();
        workflow.ValidateIdentificationNumber(Arg.Any<Applicant>()).Returns(Array.Empty<ValidationError>());
        workflow.ValidateAddress(Arg.Any<Applicant>()).Returns(AddressApproved);

        // Represents the alternate flow when an external residency service fails.
        workflow.EvaluateCitizenship(Arg.Any<Applicant>()).Returns(ApplicationStatus.PendingVerification);

        var sut = new AccountOpeningAutomation(workflow);

        // Act
        var result = sut.Run(applicant);

        // Assert
        Assert.Equal(ApplicationStatus.PendingVerification, result.Status);
        Assert.Empty(result.Errors);

        workflow.Received(1).EvaluateCitizenship(Arg.Any<Applicant>());
        workflow.DidNotReceive().Process(Arg.Any<Applicant>());
    }

    [Fact]
    public void Run_missing_postal_code_returns_incomplete_and_skips_citizenship_and_process()
    {
        // Arrange
        var applicant = ApplicantFactory.CreateValid() with
        {
            Address = new Address("123 Main St", "Milwaukee", "WI", "")
        };

        var workflow = Substitute.For<IAccountOpeningWorkflow>();
        workflow.ValidateIdentificationNumber(Arg.Any<Applicant>()).Returns(Array.Empty<ValidationError>());

        var expected = new ProcessResult(
            ApplicationStatus.Incomplete,
            Errors("Postal/Zip required")
        );
        // Lesson learned, NSubstitute's Arg.Is uses an expression tree; so it can't handle null-propagation.
        workflow.ValidateAddress(Arg.Is<Applicant>(a => a.Address != null && a.Address.PostalCode == "")).Returns(expected);

        var sut = new AccountOpeningAutomation(workflow);

        // Act
        var result = sut.Run(applicant);

        // Assert
        Assert.Equal(ApplicationStatus.Incomplete, result.Status);
        Assert.Contains(result.Errors, e => e.Message.Contains("Postal", StringComparison.OrdinalIgnoreCase));

        workflow.Received(1).ValidateAddress(Arg.Any<Applicant>());
        workflow.DidNotReceive().EvaluateCitizenship(Arg.Any<Applicant>());
        workflow.DidNotReceive().Process(Arg.Any<Applicant>());
    }
}
