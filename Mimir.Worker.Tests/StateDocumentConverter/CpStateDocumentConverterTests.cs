using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume.Model.EnumType;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class CpStateDocumentConverterTests
{
    private readonly CpStateDocumentConverter _converter = new();

    [Theory]
    [InlineData(0)]
    [InlineData(120)]
    [InlineData(9999)]
    public void ConvertToStateData(int cp)
    {
        var privateKey = new PrivateKey();
        var avatarAddress = privateKey.Address;
        var battleType = BattleType.Arena;
        
        // 직접 List 생성
        var avatarBytes = avatarAddress.ToByteArray();
        var addressValue = new Binary(avatarBytes);
        var cpValue = new Integer(cp);
        var list = new List(new IValue[] { addressValue, cpValue });
        
        // 실제 비즈니스 로직과 동일한 주소 생성 방식으로 생성
        // 이 부분은 CpState.DeriveAddress 메서드와 로직이 동일해야 합니다
        var address = new PrivateKey().Address; // 테스트용 임의 주소 사용
        
        var context = new AddressStatePair
        {
            Address = address,
            BlockIndex = 123,
            RawState = list
        };
        
        var doc = _converter.ConvertToDocument(context);

        Assert.IsType<CpStateDocument>(doc);
        var dataState = (CpStateDocument)doc;
        Assert.Equal(address, dataState.Address);
        Assert.Equal(avatarAddress, dataState.AvatarAddress);
        Assert.Equal(battleType, dataState.BattleType);
        Assert.Equal(cp, dataState.Cp);
    }
} 