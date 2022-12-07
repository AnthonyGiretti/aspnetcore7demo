using DemoDotnet7.Dtos;

namespace DemoDotnet7.Mappers;

file class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class SQLPersonProcessor
{
    public Task Process(PersonDTO personDto)
    {
        var person = new Person
        {
            FirstName = personDto.FirstName,
            LastName = personDto.LastName
        };
        return Task.CompletedTask;
    }
}