namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Address
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public Geolocation Geolocation { get; set; } = new();
}
