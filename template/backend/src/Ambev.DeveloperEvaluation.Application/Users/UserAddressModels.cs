namespace Ambev.DeveloperEvaluation.Application.Users;

public sealed class UserGeolocationModel
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}

public sealed class UserAddressModel
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public UserGeolocationModel Geolocation { get; set; } = new();
}
