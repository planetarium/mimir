using Libplanet.Crypto;

namespace Mimir.Validators;

public static class AddressValidator
{
    public static bool TryValidate(
        string addressString,
        out Address address,
        out string? errorMessage)
    {
        try
        {
            address = new Address(addressString);
            errorMessage = null;
            return true;
        }
        catch (ArgumentException e)
        {
            address = default;
            errorMessage = e.Message;
            return false;
        }
    }
}
