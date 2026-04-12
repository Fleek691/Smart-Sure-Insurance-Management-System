using SmartSure.Shared.Exceptions;

namespace SmartSure.Tests.SharedExceptions;

[TestFixture]
[Category("Shared")]
public class ExceptionTests
{
    [Test]
    [Description("ValidationException should carry the provided message")]
    public void ValidationException_CarriesMessage()
    {
        var ex = new ValidationException("Field is required.");
        Assert.That(ex.Message, Is.EqualTo("Field is required."));
    }

    [Test]
    [Description("NotFoundException should carry the provided message")]
    public void NotFoundException_CarriesMessage()
    {
        var ex = new NotFoundException("Resource not found.");
        Assert.That(ex.Message, Is.EqualTo("Resource not found."));
    }

    [Test]
    [Description("UnauthorizedException should carry the provided message")]
    public void UnauthorizedException_CarriesMessage()
    {
        var ex = new UnauthorizedException("Not authenticated.");
        Assert.That(ex.Message, Is.EqualTo("Not authenticated."));
    }

    [Test]
    [Description("ForbiddenException should carry the provided message")]
    public void ForbiddenException_CarriesMessage()
    {
        var ex = new ForbiddenException("Access denied.");
        Assert.That(ex.Message, Is.EqualTo("Access denied."));
    }

    [Test]
    [Description("ConflictException should carry the provided message")]
    public void ConflictException_CarriesMessage()
    {
        var ex = new ConflictException("Already exists.");
        Assert.That(ex.Message, Is.EqualTo("Already exists."));
    }

    [Test]
    [Description("BusinessRuleException should carry the provided message")]
    public void BusinessRuleException_CarriesMessage()
    {
        var ex = new BusinessRuleException("Business rule violated.");
        Assert.That(ex.Message, Is.EqualTo("Business rule violated."));
    }

    [Test]
    [Description("HttpServiceException should carry message and status code")]
    public void HttpServiceException_CarriesMessageAndStatusCode()
    {
        var ex = new HttpServiceException("Downstream failed.", 502);
        Assert.That(ex.Message, Is.EqualTo("Downstream failed."));
        Assert.That(ex.StatusCode, Is.EqualTo(502));
    }

    [Test]
    [Description("HttpServiceException default status code should be 500")]
    public void HttpServiceException_DefaultStatusCode_Is500()
    {
        var ex = new HttpServiceException("Error");
        Assert.That(ex.StatusCode, Is.EqualTo(500));
    }

    [Test]
    [Description("All custom exceptions should inherit from SmartSureException")]
    public void AllExceptions_InheritFromSmartSureException()
    {
        Assert.That(new ValidationException("x"),   Is.InstanceOf<SmartSureException>());
        Assert.That(new NotFoundException("x"),     Is.InstanceOf<SmartSureException>());
        Assert.That(new UnauthorizedException("x"), Is.InstanceOf<SmartSureException>());
        Assert.That(new ForbiddenException("x"),    Is.InstanceOf<SmartSureException>());
        Assert.That(new ConflictException("x"),     Is.InstanceOf<SmartSureException>());
        Assert.That(new BusinessRuleException("x"), Is.InstanceOf<SmartSureException>());
        Assert.That(new HttpServiceException("x"),  Is.InstanceOf<SmartSureException>());
    }
}
