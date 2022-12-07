using ExpectedObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using NSubstitute;
using WebAppDotnet7.MinimalEndpoints;
using WebAppDotnet7.Models;

namespace TestDotnet7;

public class MinimalTests
{
    private readonly IHttpContextAccessor _mockedHttpContextAccessor;

    public MinimalTests()
    {
        _mockedHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    [Fact]
    public void TestGetPerson()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items.Add("firstname", "Anthony");
        context.Items.Add("lastname", "Giretti");

        _mockedHttpContextAccessor.HttpContext.Returns(context);

        var expectedResult = new Person
        {
            FirstName = "Anthony",
            LastName = "Giretti"
        }.ToExpectedObject();

        // Act
        var result = UnitTestableEndpoints.GetPerson(_mockedHttpContextAccessor) as Ok<Person>;

        // Assert
        expectedResult.ShouldEqual(result.Value);
    }
}