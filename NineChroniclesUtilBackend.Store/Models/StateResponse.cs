using Bencodex.Types;

namespace NineChroniclesUtilBackend.Store.Models;

public class StateResponse
{
    public string Address { get; }
    public string AccountAddress { get; }
    public IValue Value { get; }

    public StateResponse(string address, string accountAddress, IValue value)
    {
        Address = address;
        AccountAddress = accountAddress;
        Value = value;
    }
}