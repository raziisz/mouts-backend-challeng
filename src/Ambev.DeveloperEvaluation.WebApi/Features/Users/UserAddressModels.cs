namespace Ambev.DeveloperEvaluation.WebApi.Features.Users;

public sealed class UserNameRequest
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
}

public sealed class UserNameResponse
{
    public string Firstname { get; init; } = string.Empty;
    public string Lastname { get; init; } = string.Empty;
}

public sealed class UserGeolocationRequest
{
    public string Lat { get; set; } = string.Empty;
    public string Long { get; set; } = string.Empty;
}

public sealed class UserAddressRequest
{
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public int Number { get; set; }
    public string Zipcode { get; set; } = string.Empty;
    public UserGeolocationRequest Geolocation { get; set; } = new();
}

public sealed class UserGeolocationResponse
{
    public string Lat { get; init; } = string.Empty;
    public string Long { get; init; } = string.Empty;
}

public sealed class UserAddressResponse
{
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public int Number { get; init; }
    public string Zipcode { get; init; } = string.Empty;
    public UserGeolocationResponse Geolocation { get; init; } = new();
}
