using Bencodex.Types;
using Libplanet.Action;
using Libplanet.Action.State;

namespace Mimir.Initializer.Util;

/// <summary>
/// A mock <see cref="IAction"/>.
/// </summary>
public class MockAction : IAction
{
    /// <inheritdoc/>
    public IValue PlainValue { get; private set; }

    public MockAction()
    {
        PlainValue = Null.Value;
    }

    /// <inheritdoc cref="IAction.LoadPlainValue"/>
    public void LoadPlainValue(IValue plainValue)
    {
        PlainValue = plainValue;
    }

    /// <inheritdoc cref="IAction.Execute/>
    public IWorld Execute(IActionContext context)
    {
        throw new NotSupportedException("Execution is not allowed.");
    }
}
