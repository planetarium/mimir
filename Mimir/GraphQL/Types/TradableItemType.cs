using Mimir.Models.Item;

namespace Mimir.GraphQL.Types;

public class TradableItemType : UnionType<ItemUsable>
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        descriptor.Type<ArmorType>();
        descriptor.Type<AuraType>();
        descriptor.Type<BeltType>();
        descriptor.Type<NecklaceType>();
        descriptor.Type<WeaponType>();
        descriptor.Type<RingType>();
        descriptor.Type<MaterialType>();
        descriptor.Type<ConsumableType>();
    }
}
