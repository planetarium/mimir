namespace Mimir.Store.Models;

public class StateResponse
{
    public string Address { get; }
    public string AccountAddress { get; }
    public string Value { get; }

    public StateResponse(string address, string accountAddress, string value)
    {
        Address = address;
        AccountAddress = accountAddress;
        Value = value;
    }
}