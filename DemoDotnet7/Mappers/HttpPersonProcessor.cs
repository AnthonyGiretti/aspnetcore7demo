using DemoDotnet7.Dtos;

namespace DemoDotnet7.Mappers;

file class Person
{
    public string FirstNameLastName { get; set; }
}

public class HttpPersonProcessor
{
    public Task Process(PersonDTO personDto)
    {
        var person = new Person
        {
            FirstNameLastName = $"{personDto.FirstName} {personDto.LastName}"
        };
        return Task.CompletedTask;
    }
}