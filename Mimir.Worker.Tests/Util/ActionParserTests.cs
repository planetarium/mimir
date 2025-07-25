using Libplanet.Crypto;
using Mimir.Worker.Util;
using MongoDB.Bson;
using Xunit;

namespace Mimir.Worker.Tests.Util;

public class ActionParserTests
{
    [Fact]
    public async Task ParseAction_HackAndSlash22()
    {
        var rawData =
            "6475373a747970655f69647531363a6861636b5f616e645f736c617368323275363a76616c756573647531323a617053746f6e65436f756e7475313a307531333a6176617461724164647265737332303ae06aa76fbc394bd5f0e97179d1d955de324f15be75383a636f7374756d65736c657531303a65717569706d656e74736c31363a89a665068cfc484782326d0a2398b9bd31363a122f0c7e14ce744c988d4c672bf6fea231363a8e4d47d398ab6147b56d6ee156cfd9de31363a6df8fcefbdf251478ef5606d3826a7b131363a4f0d5ffc9d52ca49ab0cc2764a058f956575353a666f6f64736c6575323a696431363abb800221e345034dae37a022e94a349c75313a726c6c75313a3075353a3330303031656575373a7374616765496475333a3234397531343a746f74616c506c6179436f756e7475313a3175373a776f726c64496475313a356565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_BuyProduct3()
    {
        var rawData =
            "6475373a747970655f69647531323a6275795f70726f647563743375363a76616c7565736475313a6132303a0be8f2f79a2084e8668c1d65b71b0a473880d89c75323a696431363a2c72b97cff0aa1419fd174958e8e5adc75313a706c6c31363ae03f7381a432f943b7aee0ba57cc23d26c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353030656532303a83aad26331008485957cc3a28de73bc1ef051da132303a2d48e036efbd06a969c9f927f05868f4c61f77d17531313a4e6f6e46756e6769626c656675363a576561706f6e31363ab72462b09fb2954c9db7b69e6d01c7b8656c31363af25290b22bb99143b14f77e126dd97666c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569373030656532303a93ce72757cf1008f3a69603de43a21bf031b515532303a5f5aa1fe76318c339d5d1636fa27efa1d4b88ff17531313a4e6f6e46756e6769626c656675363a576561706f6e31363abcdf1a237aa2ef48ac101c2e3bfa552f656c31363a7132b6c790cd51499f3c938181dbc0916c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569383030656532303ab0cb7d221ba0a60335df78be42562fc30f477a1832303a10327c4d37f02ce11c1e65f1cd02e5e7ccdf1a377531313a4e6f6e46756e6769626c656675343a52696e6731363a1ddbd334081ec34b8239925827169fa4656c31363ab3ae7ba49d9e9047ba2a1a83780e75856c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569373030656532303ae64edd089728b5a0ce7ff567127da1927795081732303a0451eb66608619f748061b1eb68297402ba0bd9c7531313a4e6f6e46756e6769626c656675343a42656c7431363a7f2b6fd4ac32024fa5c388f30f4772c8656c31363a9ebf241739466f478c6217cba7c8b94e6c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569353030656532303a1068f39f484f37abdb1ddfc256baaa3814ef024d32303ae6f554da1596c1cab7a24bce472806efdb59758f7531313a4e6f6e46756e6769626c656675343a42656c7431363a6db29f7b0ba7a047878a71b527c03a89656c31363a3771e969f22553409f6f836184b4cd636c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656931303030656532303a1068f39f484f37abdb1ddfc256baaa3814ef024d32303ae6f554da1596c1cab7a24bce472806efdb59758f7531313a4e6f6e46756e6769626c656675383a4e65636b6c61636531363aff8fdc2b9984cd43b953d8032cfd580a656c31363af046bd2abe84d04c9c7805e145f1fa186c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656931303030656532303a27d9c6ee76255265b65cf78035f7fc67ae4cb27332303ae0b411a9d8e95ca496f850bf3abfa382983067337531313a4e6f6e46756e6769626c656675343a52696e6731363a1e4637a2073c9d4ca33f61fa5d259b05656c31363a2af9eae00459764a8758b2e0b1a6b6fa6c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656932303030656532303aa57ff2323a5d8f6b855846fd16cd295cbe7b64f932303a33fb7643f66f7ab61c6f9f99ecf42de6d419b1507531313a4e6f6e46756e6769626c656675343a52696e6731363a1ddbd334081ec34b8239925827169fa4656c31363ab3ae7ba49d9e9047ba2a1a83780e75856c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656931303030656532303a850ec603efdae6ca06b64777d395f70efe22b99f32303a6b15f715118adae498e8b69bd2dbe8034ec10cce7531313a4e6f6e46756e6769626c656675343a52696e6731363adcb90dfd74091149be582ed7191f27f1656c31363affea36773a60e04c98572c7dfb26e0cc6c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569383030656532303ab0cb7d221ba0a60335df78be42562fc30f477a1832303a10327c4d37f02ce11c1e65f1cd02e5e7ccdf1a377531313a4e6f6e46756e6769626c656675343a52696e6731363a1609afd350f5bf4a8e752a46307c01e5656c31363a3958f8157ea94e4690f180d3afd1e4006c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656931353030656532303a6e68ad44342a628c30d42737b4983d5848d7a0ab32303a162916fe922a9d963383537c304edd60166f2d1c7531313a4e6f6e46756e6769626c656675363a576561706f6e31363a19022b4334152c49ba180d9ae7d64b82656c31363a6c764a14280dbb4094eaeda2ae1b7f4c6c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569343030656532303a7f292a1add3ed60e553a13b8ebaf8f0b7813ae6132303a4fc0ba67b4e30f33d11c9de8d837f658ac92bcd17531313a4e6f6e46756e6769626c656675363a576561706f6e31363acb84800f6f23444c9da46fc2c5c5fbe6656c31363a69afb2bb8d1af34b9f96b1e85ba39ccd6c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e43476569343030656532303a3b6fd6255a749f0eacc69dd111a0f1b77cb7a77032303adc8a95b0478cc348b9d767d9137099cf563028247531313a4e6f6e46756e6769626c656675343a52696e6731363a383f3532157c8d488bdb9f782cf01ccb656c31363a81964f3c348b574fa3d1c7aeb1d9aec16c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656931353030656532303a0f25977fcc488105fe68c9a833ee06f71be98cd432303ace9833c49ea05ce7b72fe78a85ce284bbeef17d57531313a4e6f6e46756e6769626c65667531313a46756c6c436f7374756d6531363a8cc79fbddc603246b3cb728e9f05e10c65656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_Buy7()
    {
        var rawData = "6475373a747970655f696475343a6275793775363a76616c7565736475323a626132303a47ef9e17fe94c16db8b83c240b1d8c960d059f0e75323a696431363a45e7d99932c0524d9dae84c19d1ceed375333a7069736c6475333a69737475363a576561706f6e75313a706c647531333a646563696d616c506c61636573313a0275373a6d696e746572736c32303a47d082a115c63e7b58b1532d20e631538eafadde6575363a7469636b657275333a4e4347656933393030656575323a706931363ac587d3c2e96ca1408f8b3c9a5a59093475333a73676132303ad7f75b5fb0b7bbf20e7d85e3c68951bc29cd6bb975333a73766132303a237a5fe55d3912b05334a01c2fd527e0bc9f83df65656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_TransferAsset5()
    {
        var rawData = "6475373a747970655f69647531353a7472616e736665725f61737365743575363a76616c7565736475363a616d6f756e746c647531333a646563696d616c506c61636573313a0275373a6d696e746572736e75363a7469636b657275333a4e4347656931656575393a726563697069656e7432303a70e3676822139cc80cfdeeba8acded47fe50f90b75363a73656e64657232303afedb101dc9d6adc060e9256a576be4d5babc865f6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_TransferAssets3()
    {
        var rawData = "6475373a747970655f69647531363a7472616e736665725f6173736574733375363a76616c756573647531303a726563697069656e74736c6c32303acb75c84d76a6f97a2d55882aea4436674c2886736c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f3830303230326569353035303030306565656c32303acb75c84d76a6f97a2d55882aea4436674c2886736c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f36303033303665693130303030306565656575363a73656e64657232303a5340c29b6044d646dc8abe8e2a585f9e8bb88a506565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_DailyReward7()
    {
        var rawData = "6475373a747970655f69647531333a6461696c795f7265776172643775363a76616c7565736475313a6132303acc0b6d014e3ffb4e430189b4d88271767fd6ec2675323a696431363a575667cc4467f141b596d4105eaaa58d6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimStakeReward()
    {
        var rawData = "6475373a747970655f69647531393a636c61696d5f7374616b655f7265776172643975363a76616c7565736475323a616132303a26c0a6f2508b4256bc25529a5d33b33f89554f0d75323a696431363ac0e9abab5369ed4bad1e66734ab332a16565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimWorldBossReward()
    {
        var rawData = "6475373a747970655f69647532333a636c61696d5f776f726c645f626f73735f72657761726475363a76616c7565736475313a6132303aff08107442cd409f3307d0f448fb98a1a7b290a075323a696431363abd3a02c52967134193bd8f67f9f7d4a96565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimRaidReward()
    {
        var rawData = "6475373a747970655f69647531393a636c61696d5f706174726f6c5f72657761726475363a76616c75657332303af33fd309c17cea9371e9beb31dd4ac840b6a6a1265";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimPatrolReward()
    {
        var rawData = "6475373a747970655f69647531393a636c61696d5f706174726f6c5f72657761726475363a76616c75657332303af697896598ae95e5901b3bfc94c956fdc16ab7b065";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimGifts()
    {
        var rawData = "6475373a747970655f69647531313a636c61696d5f676966747375363a76616c7565736475323a616132303abe598d15fc9c8040c9a3c819691353f078a580fa75323a676975313a3175323a696431363a0d845eda01b7964fb4ac9ca2fe4a828d6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimItems()
    {
        var rawData = "6475373a747970655f69647531313a636c61696d5f6974656d7375363a76616c7565736475323a63646c6c32303a2692d73a42627f137b42cc819726f25580a231376c6c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f3830303230316569313065656c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f36303032303165693465656c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f35303030303065693465656c647531333a646563696d616c506c61636573313a0075373a6d696e746572736e75363a7469636b65727531343a4974656d5f4e545f343030303030656931303030656565656575323a696431363a677ec026690011f0bd584a6be86fb7ae75313a6d7537313a7b22696170223a207b22675f736b75223a2022675f706b675f667265656461696c79333530222c2022615f736b75223a2022615f706b675f667265656461696c79333530227d7d6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ItemEnhancement()
    {
        var rawData = "6475373a747970655f69647531383a6974656d5f656e68616e63656d656e74313475363a76616c756573647531333a6176617461724164647265737332303a0cac53afea8c0b69f536b81d718fb0acddb7b79e75323a696431363a29385ba622b6154d8dfffb4dc394be2075363a6974656d496431363a4e260b13f90a7b40a1be5d73910665837531313a6d6174657269616c4964736c31363aacc1c32068f5504d8eabb9d4d19d537c31363ac7e0252b42a6304ea6051b9a56033bd631363abf5688d915633a40a8c6e36f46c6b1b66575393a736c6f74496e64657875313a306565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_CombinationEquipment()
    {
        var rawData = "6475373a747970655f69647532333a636f6d62696e6174696f6e5f65717569706d656e74313775363a76616c7565736475313a6132303a7043db4a56de807717cbcd73a3ef54ba97f964fe75313a686675313a6975393a31303537303030303175323a696431363a0cee5e6b8a531c4fa23dc994808285ba75313a706675333a7069646e75313a7275333a32373875313a7375313a326565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_Raid()
    {
        var rawData = "6475373a747970655f696475353a726169643775363a76616c7565736475313a6132303af637375f78c08298f7580f1d7e7d81185e699b9c75313a636c6575313a656c31363a90957b74013ec14cba3f4bf6ed2ef50531363aa4739f4c26096c4580e2121428bc763531363af5fa3eaef100734b8f1251895d42072731363af026598511586942b7123f215f4243cc31363afed09207f329134f96b64f7f6ca5087d31363a5076c71c69e5bc4abfbf357172d4f08531363ab17bcfeffeb12c44af712a8a0d5fbb0a31363af1119b3eee431345840185c0be04e2206575313a666c6575323a696431363ac21ae45cf0567c458cd58bc2ee8d5c5875313a706675313a726c6c75313a3075353a3130303134656c75313a3375353a3130303033656c75313a3675353a3130303034656c75313a3775353a3130303033656575363a736561736f6e69343732656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_AuraSummon()
    {
        var rawData = "6475373a747970655f69647531313a617572615f73756d6d6f6e75363a76616c7565736475323a616132303ad0a7cf753a9619c25a4ec851aef08c68c529c2b175333a67696475353a313030303175323a696431363a55517ee9b69ada4686cdbe4321a615b375323a736375313a316565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_CostumeSummon()
    {
        var rawData = "6475373a747970655f69647531343a636f7374756d655f73756d6d6f6e75363a76616c7565736475323a616132303a6d87a81051106e8484f562c046612e25cc423c2a75333a6769646935303030316575323a696431363a7c378e19a6d3cc40992efd183f277f3a75323a73636931656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_RuneSummon()
    {
        var rawData = "6475373a747970655f69647531313a72756e655f73756d6d6f6e75363a76616c7565736475323a616132303af5433fdb52c493116b9051196c8746aa1c835acb75333a6769646932303030316575323a696431363a6a42541b86d33147bcf181786b3ba98d75323a7363693130656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_PetEnhancement()
    {
        var rawData = "6475373a747970655f69647531353a7065745f656e68616e63656d656e7475363a76616c7565736475313a6132303a4ce91b9f8542525118981d23e160e55dfc651a6375323a696431363aaa816f82745fec45b25f7bf52a24ed2175313a7075343a3130323475313a7475313a316565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_RuneEnhancement()
    {
        var rawData = "6475373a747970655f69647531363a72756e65456e68616e63656d656e743275363a76616c7565736475313a6132303ae4453fac7fe28f95f970ebe49a27628e8141d4df75323a696431363a1744888ef6a86a47a7abf26d6a9847a675313a7275353a333030303175313a7475313a316565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_UnlockRuneSlot()
    {
        var rawData = "6475373a747970655f69647531363a756e6c6f636b5f72756e655f736c6f7475363a76616c7565736475313a6132303af5433fdb52c493116b9051196c8746aa1c835acb75323a696431363ac0682755f5fc1947a98360128d692ef775313a7375313a376565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_UnlockCombinationSlot()
    {
        var rawData = "6475373a747970655f69647532333a756e6c6f636b5f636f6d62696e6174696f6e5f736c6f7475363a76616c7565736475313a6132303a4076939d01c74de928ed14e59e8280075d0743d075323a696431363a9edc4be0704dc44b917f94cebb28d04875313a736934656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_UnlockEquipmentRecipe()
    {
        var rawData = "6475373a747970655f69647532343a756e6c6f636b5f65717569706d656e745f7265636970653275363a76616c7565736475313a6132303a238da46bddaa99178ec59586b419abf977b6524b75323a696431363ac27d6d2b95ffb94d8e8e6abc3fa8ebc375313a726c75323a3934656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_UnlockWorld()
    {
        var rawData = "6475373a747970655f69647531333a756e6c6f636b5f776f726c643275363a76616c7565736475313a6132303a102fe0cba66e4d8c7543a338321b789aed1255d675323a696431363a8c591f987e5b5b41ba5d1917e1c2465075313a776c75313a35656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_Grinding()
    {
        var rawData = "6475373a747970655f696475393a6772696e64696e673275363a76616c7565736475313a6132303a238da46bddaa99178ec59586b419abf977b6524b75313a637475313a656c31363a9bc451106cb7ac44931d5ac51c3fd6546575323a696431363aa008af0e4b1945409229661326dc07fe6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_Synthesize()
    {
        var rawData = "6475373a747970655f69647531303a73796e74686573697a6575363a76616c7565736475313a6132303a7355c6eb16b02de1dae910cafed9ad8cbb19ebf875313a636675313a6769356575313a696931396575323a696431363ac53215ca8987124e87c1db0e77badb9575313a6d6c31363a864e96482275bb488172cf2fc677830731363a96bcbd4b2552584d83fa2495856dcccd31363acd1db49436882b4fa2bd929eb7da721631363ab2c1c396900b984f8fca1425495dce6331363a2f535c9a536cee4f9c6d4b7d8d05509531363a37a7b4a799129941a2baf6b65f672dad31363a2bad02af9fe5c44cb33b6fd87070039331363a34a22bb4457a24438aef48b90e21d47031363a4f16afb515126b49906bb4f6ae75657731363a93f291bffae97047b03f4c890fc9342031363a89321bc4e393a748858cf1f988e4ba5c31363a678f33cd66c3e046afbb45f51db345d331363abfa123d97afba144b559335d0ca8997731363a1e830be1c4535b43901817ade0850b1431363ac8012ef48d9211448a9a803897d3c92f31363a2f196ff91b2f39468f8ee27d56c2b6f1656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_RapidCombination()
    {
        var rawData = "6475373a747970655f69647531393a72617069645f636f6d62696e6174696f6e313075363a76616c7565736475313a6132303a96f3f92a97cb0d7a7b5bebfb80259a0a9bfd54e675323a696431363a2f7e2e99141a52419ec98838ba4934ab75313a736c693565656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_CombinationConsumable()
    {
        var rawData = "6475373a747970655f69647532333a636f6d62696e6174696f6e5f636f6e73756d61626c653975363a76616c7565736475313a6132303a96f3f92a97cb0d7a7b5bebfb80259a0a9bfd54e675323a696431363ab725b39c5cd8204d8ecb07b8af6959f375313a7275363a31303030323075313a7375313a376565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_WithInvalidRawData_ThrowsException()
    {
        var invalidRawData = "invalid_hex_data";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ActionParser.ExtractActionValue(invalidRawData)
        );
    }

    [Fact]
    public async Task ParseAction_WithNonDictionaryData_ThrowsException()
    {
        var rawData = "6c68656c6c6f"; // "hello" as hex

        Assert.Throws<InvalidOperationException>(() => ActionParser.ExtractActionValue(rawData));
    }

    [Fact]
    public async Task ParseAction_Battle()
    {
        var rawData = "6475373a747970655f696475363a626174746c6575363a76616c7565736475333a6172707531313a504c414e4554415249554d75333a6368616675323a63736c31363a4184872c5fd0c8448614fb6c623f03fe6575333a65616132303afb343d09d075295282f6c40b27cc0d0496020b1e75323a65736c31363aff7c9a0e0b211848960d96dbdb53ebe731363a7707d92bebae6e4aa491bc3073b93a4231363a72d41e59a4c8574fa7b3c661d183d8bd31363aeed4ef9beddd334cab31227e1f6dca5f31363a1d63cca07ba3c441b43e741eb899530a31363ae0b918abe2b01e4d80de3f2b1f58df3f31363a42da5ddaae9eba429e8e791daa7d032631363ab20ba3f22e25d645a7dcd91019d005596575323a696431363aefe286495734244f9a76c6735fb5e74c75313a6d753533323a65794a68624763694f694a53557a49314e694973496e523563434936496b705856434a392e65794a7063334d694f694a77624746755a585268636d6c3162534268636d56755953427a5a584a3261574e6c49697769596d6c6b496a6f314d444d354e7a6373496d6c68644349364d5463314d7a51774f5451794e6977695a586877496a6f784e7a557a4e444d784d4449324c434a68645751694f694a4f6157356c51326879623235705932786c6379426f5a57466b6247567a63794a392e5044735f735561346f4d6854754a7777753158353758786f4132666e366b304d796248655a44706d6d39754965474e70664e37496433786a6776776b6933704c745a7a336a61324d464d4f7154317358397575557848336647746d5a4a7154677173694444506174706a4f6b7139762d7955526a5a566178583875496c72656f314a5f59463139554c476b3741675f4f6677655532417831304964717a75726b435930785f35544b4b3736663647414d397368386f7861545f576f6648377830377674326258784f39397a706f3972656c484b506d6e7049574c51444648306d6566704a6e55466270314930714a366c2d78466a4d4b5a564c746f6d3546457770646f2d6e764e2d36774b7250346d573776344c385031734b795531665f685934664f633935393036314f6e6631786a6a66365562626a674131516b75546d7779684a6b786f626376366d5774623537626642796c7775333a6d616132303abeda0ceb36943f4b53042cb07e613f74b212e33775323a72696c6c75313a3075353a3130303035656c75313a3375353a3130303033656c75313a3675353a3130303034656c75313a3775353a3130303033656575313a7369343732656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ClaimAdventureBossReward()
    {
        var rawData = "6475373a747970655f69647532373a636c61696d5f616476656e747572655f626f73735f72657761726475363a76616c7565736475313a6132303a19326abe59e7115290cf0c69d295c542e2cf973a75323a696431363ae0557dcafe92cd43bee308798d24df7b6565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_ExploreAdventureBoss()
    {
        var rawData = "6475373a747970655f69647532323a6578706c6f72655f616476656e747572655f626f737375363a76616c756573647531333a6176617461724164647265737332303a3dc11702b4e1d41ac165c5412ee4a8551d1155a775383a636f7374756d65736c31363ab25d35312ac18d4788a8cfe4b1d5d413657531303a65717569706d656e74736c31363acd0e9e3380569f4d96dff006b9c24b0831363aefe6b45f3bbbff4a97ba5376c5d11fa631363a56a06898ca37ba40b1815d9e7065264d31363a66f444a04ab7e847b071c2d3c70a2b2331363a5ed073be0b53e0418f985a29e310e95b31363af709d6c5e903f341b8c63ae166fe31a931363ad91596cb3888fc469b74fcd73852b97b31363a155343e33f83864d87cb7d87b0819c806575353a666f6f64736c6575323a696431363a021f10836ff8ca4bae1505e2bfb4729a75313a726c6c75313a3075353a3130303334656c75313a3375353a3130303036656c75313a3675353a3130303034656c75313a3775353a3130303033656575363a736561736f6e69343732656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_SweepAdventureBoss()
    {
        var rawData = "6475373a747970655f69647532303a73776565705f616476656e747572655f626f737375363a76616c7565736475313a6132303a3dc11702b4e1d41ac165c5412ee4a8551d1155a775313a636c31363ab25d35312ac18d4788a8cfe4b1d5d4136575313a656c31363acd0e9e3380569f4d96dff006b9c24b0831363aefe6b45f3bbbff4a97ba5376c5d11fa631363a56a06898ca37ba40b1815d9e7065264d31363a66f444a04ab7e847b071c2d3c70a2b2331363a5ed073be0b53e0418f985a29e310e95b31363af709d6c5e903f341b8c63ae166fe31a931363ad91596cb3888fc469b74fcd73852b97b31363a155343e33f83864d87cb7d87b0819c806575323a696431363a26da17a4c12bda49b2f6974c599bf23875313a726c6c75313a3075353a3130303334656c75313a3375353a3130303036656c75313a3675353a3130303034656c75313a3775353a3130303033656575313a7369343732656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_UnlockFloor()
    {
        var rawData = "6475373a747970655f69647531323a756e6c6f636b5f666c6f6f7275363a76616c7565736475313a6132303a3dc11702b4e1d41ac165c5412ee4a8551d1155a775323a696431363a97051847bc09414facd899b181cf63b375313a73693437326575313a75666565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_Wanted()
    {
        var rawData = "6475373a747970655f696475363a77616e74656475363a76616c7565736475313a6132303ae55bfdec6f59c418029b35c5276e2e9f18c176ed75313a626c647531333a646563696d616c506c61636573313a0275373a6d696e746572736e75363a7469636b657275333a4e43476569353030656575323a696431363acbf176820f05ce41b2b216701e35263975313a7369343732656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    [Fact]
    public async Task ParseAction_CustomEquipmentCraft()
    {
        var rawData = "6475373a747970655f69647532323a637573746f6d5f65717569706d656e745f637261667475363a76616c7565736c32303ab374df06963119738a16e1b08c2052a6979964316c6c69356569306569306565656565";

        var extractedActionValues = ActionParser.ExtractActionValue(rawData);

        await Verify(extractedActionValues);
    }

    // [Fact]
    // public async Task ParseAction_BanGuildMember()
    // {
    //     var rawData = "example_raw_data_for_ban_guild_member";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ClaimGuildReward()
    // {
    //     var rawData = "example_raw_data_for_claim_guild_reward";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ClaimReward()
    // {
    //     var rawData = "example_raw_data_for_claim_reward";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ClaimUnbonded()
    // {
    //     var rawData = "example_raw_data_for_claim_unbonded";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_JoinGuild()
    // {
    //     var rawData = "example_raw_data_for_join_guild";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MakeGuild()
    // {
    //     var rawData = "example_raw_data_for_make_guild";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MoveGuild()
    // {
    //     var rawData = "example_raw_data_for_move_guild";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_QuitGuild()
    // {
    //     var rawData = "example_raw_data_for_quit_guild";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RemoveGuild()
    // {
    //     var rawData = "example_raw_data_for_remove_guild";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_UnbanGuildMember()
    // {
    //     var rawData = "example_raw_data_for_unban_guild_member";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ActivateCollection()
    // {
    //     var rawData = "example_raw_data_for_activate_collection";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_AddRedeemCode()
    // {
    //     var rawData = "example_raw_data_for_add_redeem_code";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ApprovePledge()
    // {
    //     var rawData = "example_raw_data_for_approve_pledge";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_BurnAsset()
    // {
    //     var rawData = "example_raw_data_for_burn_asset";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_CancelProductRegistration()
    // {
    //     var rawData = "example_raw_data_for_cancel_product_registration";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ChargeActionPoint()
    // {
    //     var rawData = "example_raw_data_for_charge_action_point";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ClaimWordBossKillReward()
    // {
    //     var rawData = "example_raw_data_for_claim_word_boss_kill_reward";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_CreatePendingActivation()
    // {
    //     var rawData = "example_raw_data_for_create_pending_activation";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_CreatePendingActivations()
    // {
    //     var rawData = "example_raw_data_for_create_pending_activations";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_CreatePledge()
    // {
    //     var rawData = "example_raw_data_for_create_pledge";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_EndPledge()
    // {
    //     var rawData = "example_raw_data_for_end_pledge";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_EventConsumableItemCrafts()
    // {
    //     var rawData = "example_raw_data_for_event_consumable_item_crafts";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_EventDungeonBattle()
    // {
    //     var rawData = "example_raw_data_for_event_dungeon_battle";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_EventMaterialItemCrafts()
    // {
    //     var rawData = "example_raw_data_for_event_material_item_crafts";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_HackAndSlashRandomBuff()
    // {
    //     var rawData = "example_raw_data_for_hack_and_slash_random_buff";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_JoinArena()
    // {
    //     var rawData = "example_raw_data_for_join_arena";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MigrateAgentAvatar()
    // {
    //     var rawData = "example_raw_data_for_migrate_agent_avatar";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MigrateFee()
    // {
    //     var rawData = "example_raw_data_for_migrate_fee";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MigrateMonsterCollection()
    // {
    //     var rawData = "example_raw_data_for_migrate_monster_collection";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MigrationActivatedAccountsState()
    // {
    //     var rawData = "example_raw_data_for_migration_activated_accounts_state";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_MintAssets()
    // {
    //     var rawData = "example_raw_data_for_mint_assets";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_PatchTableSheet()
    // {
    //     var rawData = "example_raw_data_for_patch_table_sheet";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IRankingBattleV2()
    // {
    //     var rawData = "example_raw_data_for_ranking_battle_v2";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IRedeemCodeV1()
    // {
    //     var rawData = "example_raw_data_for_redeem_code_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RegisterProduct()
    // {
    //     var rawData = "example_raw_data_for_register_product";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RemoveAddressState()
    // {
    //     var rawData = "example_raw_data_for_remove_address_state";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IRenewAdminStateV1()
    // {
    //     var rawData = "example_raw_data_for_renew_admin_state_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RequestPledge()
    // {
    //     var rawData = "example_raw_data_for_request_pledge";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ReRegisterProduct()
    // {
    //     var rawData = "example_raw_data_for_re_register_product";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RetrieveAvatarAssets()
    // {
    //     var rawData = "example_raw_data_for_retrieve_avatar_assets";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_RewardGold()
    // {
    //     var rawData = "example_raw_data_for_reward_gold";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISecureMiningReward()
    // {
    //     var rawData = "example_raw_data_for_secure_mining_reward";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISellV2()
    // {
    //     var rawData = "example_raw_data_for_sell_v2";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISellV1()
    // {
    //     var rawData = "example_raw_data_for_sell_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISellCancellationV3()
    // {
    //     var rawData = "example_raw_data_for_sell_cancellation_v3";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISellCancellationV2()
    // {
    //     var rawData = "example_raw_data_for_sell_cancellation_v2";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ISellCancellationV1()
    // {
    //     var rawData = "example_raw_data_for_sell_cancellation_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_SetAddressState()
    // {
    //     var rawData = "example_raw_data_for_set_address_state";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IStakeV1()
    // {
    //     var rawData = "example_raw_data_for_stake_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_Swap()
    // {
    //     var rawData = "example_raw_data_for_swap";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IUpdateSellV2()
    // {
    //     var rawData = "example_raw_data_for_update_sell_v2";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_IUpdateSellV1()
    // {
    //     var rawData = "example_raw_data_for_update_sell_v1";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }

    // [Fact]
    // public async Task ParseAction_ValidatorSetOperate()
    // {
    //     var rawData = "example_raw_data_for_validator_set_operate";

    //     var extractedActionValues = ActionParser.ExtractActionValue(rawData);

    //     await Verify(extractedActionValues);
    // }
}
