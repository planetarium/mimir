using System.Globalization;
using System.Text.Json;
using Bencodex;
using Bencodex.Types;
using Lib9c;
using Lib9c.Abstractions;
using Lib9c.Action;
using Libplanet.Action;
using Libplanet.Action.Loader;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Bson;
using Nekoyume.Action;
using Nekoyume.Action.AdventureBoss;
using Nekoyume.Action.Arena;
using Nekoyume.Action.CustomEquipmentCraft;
using Nekoyume.Action.Guild;
using Nekoyume.Action.Loader;

namespace Mimir.Worker.Util;

public static class ActionParser
{
    private static readonly Codec Codec = new();

    public static (string TypeId, BsonDocument Values, BsonDocument ParsedAction) ParseAction(
        string raw
    )
    {
        try
        {
            var decodedAction = Codec.Decode(Convert.FromHexString(raw));

            if (decodedAction is not Dictionary actionDict)
            {
                throw new InvalidCastException(
                    $"Invalid action type. Expected Dictionary, got {decodedAction.GetType().Name}."
                );
            }

            var typeId = "";
            if (actionDict.TryGetValue((Text)"type_id", out var typeIdValue))
            {
                typeId = typeIdValue switch
                {
                    Text text => text.Value,
                    Integer integer => integer.Value.ToString(),
                    _ => "",
                };
            }

            var values = new BsonDocument();
            if (actionDict.TryGetValue((Text)"values", out var valuesValue))
            {
                var parsedValue = ParseBencodexValue(valuesValue);
                if (parsedValue is BsonDocument doc)
                {
                    values = doc;
                }
            }

            var parsedAction =
                ParseBencodexValue(decodedAction) as BsonDocument ?? new BsonDocument();

            return (typeId, values, parsedAction);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse action: {ex.Message}", ex);
        }
    }

    private static BsonValue ParseBencodexValue(IValue value)
    {
        return value switch
        {
            Dictionary dict => ParseDictionary(dict),
            List list => ParseList(list),
            Text text => new BsonString(text.Value),
            Integer integer => new BsonString(integer.Value.ToString(CultureInfo.InvariantCulture)),
            Binary binary => new BsonString(Convert.ToHexString(binary.ToByteArray()).ToLower()),
            Bencodex.Types.Boolean boolean => new BsonBoolean(boolean.Value),
            Null => BsonNull.Value,
            _ => new BsonString(value.ToString()),
        };
    }

    private static BsonDocument ParseDictionary(Dictionary dict)
    {
        var bsonDoc = new BsonDocument();
        foreach (var kvp in dict)
        {
            string key;
            if (kvp.Key is Text text)
            {
                key = text.Value;
            }
            else if (kvp.Key is Integer)
            {
                key = kvp.Key.ToString();
            }
            else if (kvp.Key is Binary binary)
            {
                key = Convert.ToHexString(binary.ToByteArray()).ToLower();
            }
            else
            {
                key = kvp.Key.ToString();
            }

            bsonDoc.Add(key, ParseBencodexValue(kvp.Value));
        }
        return bsonDoc;
    }

    private static BsonArray ParseList(List list)
    {
        var bsonArray = new BsonArray();
        foreach (var item in list)
        {
            bsonArray.Add(ParseBencodexValue(item));
        }
        return bsonArray;
    }

    public static ExtractedActionValues ExtractActionValue(string raw)
    {
        string? typeId = null;
        IValue decodedAction;
        try
        {
            decodedAction = Codec.Decode(Convert.FromHexString(raw));
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Invalid action data: {e.Message}");
        }

        if (decodedAction is not Dictionary actionDict)
        {
            throw new InvalidCastException(
                $"Invalid action type. Expected Dictionary, got {decodedAction.GetType().Name}."
            );
        }

        if (actionDict.TryGetValue((Text)"type_id", out var typeIdValue))
        {
            typeId = typeIdValue switch
            {
                Text text => text.Value,
                Integer integer => integer.Value.ToString(),
                _ => throw new InvalidCastException(
                    $"Invalid action type. Expected Text or Integer, got {typeIdValue.GetType().Name}."
                ),
            };
        }

        NCActionLoader actionLoader = new NCActionLoader();
        try
        {
            var action = actionLoader.LoadAction(0, decodedAction);
            Address? avatarAddress = null;
            Address? sender = null;
            List<RecipientInfo> recipients = [];
            HashSet<Address> involvedAvatarAddresses = [];
            HashSet<Address> involvedAddresses = [];
            List<string> fungibleAssetValues = [];

            switch (action)
            {
                case ITransferAsset transferAsset:
                    recipients = new List<RecipientInfo>
                    {
                        new RecipientInfo(transferAsset.Recipient, transferAsset.Amount.ToString()),
                    };
                    sender = transferAsset.Sender;
                    involvedAddresses.Add(transferAsset.Recipient);
                    break;
                case ITransferAssets transferAssets:
                    sender = transferAssets.Sender;
                    recipients = transferAssets
                        .Recipients.Select(r => new RecipientInfo(r.recipient, r.amount.ToString()))
                        .ToList();
                    foreach (var recipient in transferAssets.Recipients)
                    {
                        involvedAddresses.Add(recipient.recipient);
                    }
                    break;
                case Battle battle:
                    avatarAddress = battle.myAvatarAddress;
                    involvedAvatarAddresses.Add(battle.enemyAvatarAddress);
                    break;
                case IBuy0 buy0:
                    avatarAddress = buy0.buyerAvatarAddress;
                    involvedAvatarAddresses.Add(buy0.sellerAvatarAddress);
                    break;
                case IBuy5 buy5:
                    avatarAddress = buy5.buyerAvatarAddress;
                    break;
                case BuyProduct buyProduct:
                    avatarAddress = buyProduct.AvatarAddress;
                    foreach (var productInfo in buyProduct.ProductInfos)
                    {
                        fungibleAssetValues.Add(productInfo.Price.ToString());
                    }
                    break;
                case IHackAndSlashV1 hackAndSlashV1:
                    avatarAddress = hackAndSlashV1.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV1.AvatarAddress);
                    break;
                case IHackAndSlashV2 hackAndSlashV2:
                    avatarAddress = hackAndSlashV2.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV2.AvatarAddress);
                    break;
                case IHackAndSlashV3 hackAndSlashV3:
                    avatarAddress = hackAndSlashV3.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV3.AvatarAddress);
                    break;
                case IHackAndSlashV4 hackAndSlashV4:
                    avatarAddress = hackAndSlashV4.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV4.AvatarAddress);
                    break;
                case IHackAndSlashV5 hackAndSlashV5:
                    avatarAddress = hackAndSlashV5.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV5.AvatarAddress);
                    break;
                case IHackAndSlashV6 hackAndSlashV6:
                    avatarAddress = hackAndSlashV6.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV6.AvatarAddress);
                    break;
                case IHackAndSlashV7 hackAndSlashV7:
                    avatarAddress = hackAndSlashV7.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV7.AvatarAddress);
                    break;
                case IHackAndSlashV8 hackAndSlashV8:
                    avatarAddress = hackAndSlashV8.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV8.AvatarAddress);
                    break;
                case IHackAndSlashV9 hackAndSlashV9:
                    avatarAddress = hackAndSlashV9.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV9.AvatarAddress);
                    break;
                case IHackAndSlashV10 hackAndSlashV10:
                    avatarAddress = hackAndSlashV10.AvatarAddress;
                    involvedAvatarAddresses.Add(hackAndSlashV10.AvatarAddress);
                    break;
                case BuyMultiple buyMultiple:
                    avatarAddress = buyMultiple.buyerAvatarAddress;
                    foreach (var purchaseInfo in buyMultiple.purchaseInfos)
                    {
                        involvedAvatarAddresses.Add(purchaseInfo.sellerAvatarAddress);
                        involvedAddresses.Add(purchaseInfo.sellerAgentAddress);
                    }
                    break;
                case IDailyRewardV1 dailyRewardV1:
                    avatarAddress = dailyRewardV1.AvatarAddress;
                    break;
                case IClaimStakeRewardV1 claimStakeReward:
                    avatarAddress = claimStakeReward.AvatarAddress;
                    break;
                case ClaimWorldBossReward claimWorldBossReward:
                    avatarAddress = claimWorldBossReward.AvatarAddress;
                    break;
                case IClaimRaidRewardV1 claimRaidReward:
                    avatarAddress = claimRaidReward.AvatarAddress;
                    break;
                case ClaimPatrolReward claimPatrolReward:
                    avatarAddress = claimPatrolReward.AvatarAddress;
                    break;
                case ClaimGifts claimGifts:
                    avatarAddress = claimGifts.AvatarAddress;
                    break;
                case IClaimItems claimItems:
                    foreach (var (aa, favs) in claimItems.ClaimData)
                    {
                        involvedAvatarAddresses.Add(aa);
                        foreach (var fav in favs)
                        {
                            fungibleAssetValues.Add(fav.ToString());
                        }
                    }
                    break;
                case IItemEnhancementV1 itemEnhancement1:
                    avatarAddress = itemEnhancement1.AvatarAddress;
                    break;
                case IItemEnhancementV2 itemEnhancement2:
                    avatarAddress = itemEnhancement2.AvatarAddress;
                    break;
                case IItemEnhancementV3 itemEnhancement3:
                    avatarAddress = itemEnhancement3.AvatarAddress;
                    break;
                case IItemEnhancementV4 itemEnhancement4:
                    avatarAddress = itemEnhancement4.AvatarAddress;
                    break;
                case IItemEnhancementV5 itemEnhancementV5:
                    avatarAddress = itemEnhancementV5.AvatarAddress;
                    break;
                case ICombinationEquipmentV1 combinationEquipmentV1:
                    avatarAddress = combinationEquipmentV1.AvatarAddress;
                    break;
                case ICombinationEquipmentV2 combinationEquipmentV2:
                    avatarAddress = combinationEquipmentV2.AvatarAddress;
                    break;
                case ICombinationEquipmentV3 combinationEquipmentV3:
                    avatarAddress = combinationEquipmentV3.AvatarAddress;
                    break;
                case ICombinationEquipmentV4 combinationEquipmentV4:
                    avatarAddress = combinationEquipmentV4.AvatarAddress;
                    break;
                case IRaidV1 raidV1:
                    avatarAddress = raidV1.AvatarAddress;
                    break;
                case IRaidV2 raidV2:
                    avatarAddress = raidV2.AvatarAddress;
                    break;
                case BattleArena battleArena:
                    avatarAddress = battleArena.myAvatarAddress;
                    involvedAvatarAddresses.Add(battleArena.enemyAvatarAddress);
                    break;
                case IBattleArenaV1 battleArenaV1:
                    avatarAddress = battleArenaV1.MyAvatarAddress;
                    involvedAvatarAddresses.Add(battleArenaV1.EnemyAvatarAddress);
                    break;
                case IAuraSummonV1 auraSummon:
                    avatarAddress = auraSummon.AvatarAddress;
                    break;
                case CostumeSummon costumeSummon:
                    avatarAddress = costumeSummon.AvatarAddress;
                    break;
                case IRuneSummonV1 runeSummon:
                    avatarAddress = runeSummon.AvatarAddress;
                    break;
                case PetEnhancement petEnhancement:
                    avatarAddress = petEnhancement.AvatarAddress;
                    break;
                case IRuneEnhancementV1 runeEnhancement:
                    avatarAddress = runeEnhancement.AvatarAddress;
                    break;
                case IUnlockRuneSlotV1 unlockRuneSlot:
                    avatarAddress = unlockRuneSlot.AvatarAddress;
                    break;
                case UnlockCombinationSlot unlockCombinationSlot:
                    avatarAddress = unlockCombinationSlot.AvatarAddress;
                    break;
                case IUnlockEquipmentRecipeV1 unlockEquipmentRecipe:
                    avatarAddress = unlockEquipmentRecipe.AvatarAddress;
                    break;
                case IUnlockWorldV1 unlockWorld:
                    avatarAddress = unlockWorld.AvatarAddress;
                    break;
                case IGrindingV1 grinding:
                    avatarAddress = grinding.AvatarAddress;
                    break;
                case Synthesize synthesize:
                    avatarAddress = synthesize.AvatarAddress;
                    break;
                case IRapidCombinationV2 rapidCombination:
                    avatarAddress = rapidCombination.AvatarAddress;
                    break;
                case IRapidCombinationV1 rapidCombination:
                    avatarAddress = rapidCombination.AvatarAddress;
                    break;
                case ICombinationConsumableV1 combinationConsumable:
                    avatarAddress = combinationConsumable.AvatarAddress;
                    break;
                case ClaimAdventureBossReward claimAdventureBossReward:
                    avatarAddress = claimAdventureBossReward.AvatarAddress;
                    break;
                case ExploreAdventureBoss exploreAdventureBoss:
                    avatarAddress = exploreAdventureBoss.AvatarAddress;
                    break;
                case SweepAdventureBoss sweepAdventureBoss:
                    avatarAddress = sweepAdventureBoss.AvatarAddress;
                    break;
                case IBulkUnloadFromGaragesV1 bulkUnloadFromGaragesV1:
                    foreach (var bulkUnloadFromGarage in bulkUnloadFromGaragesV1.UnloadData)
                    {
                        avatarAddress = bulkUnloadFromGarage.recipientAvatarAddress;
                        involvedAvatarAddresses.Add(bulkUnloadFromGarage.recipientAvatarAddress);
                        if (bulkUnloadFromGarage.fungibleAssetValues is not null)
                        {
                            foreach (var fav in bulkUnloadFromGarage.fungibleAssetValues)
                            {
                                fungibleAssetValues.Add(fav.ToString());
                            }
                        }
                    }
                    break;
                case IDeliverToOthersGaragesV1 deliverToOthersGaragesV1:
                    involvedAddresses.Add(deliverToOthersGaragesV1.RecipientAgentAddr);

                    if (deliverToOthersGaragesV1.FungibleAssetValues is not null)
                    {
                        foreach (var fav in deliverToOthersGaragesV1.FungibleAssetValues)
                        {
                            fungibleAssetValues.Add(fav.ToString());
                        }
                    }
                    break;
                case ILoadIntoMyGaragesV1 loadIntoMyGaragesV1:
                    if (loadIntoMyGaragesV1.AvatarAddr is not null)
                    {
                        avatarAddress = loadIntoMyGaragesV1!.AvatarAddr;
                        involvedAvatarAddresses.Add(loadIntoMyGaragesV1.AvatarAddr.Value);
                    }

                    if (loadIntoMyGaragesV1.FungibleAssetValues is not null)
                    {
                        foreach (var fav in loadIntoMyGaragesV1.FungibleAssetValues)
                        {
                            fungibleAssetValues.Add(fav.ToString());
                        }
                    }
                    break;
                case UnlockFloor unlockFloor:
                    avatarAddress = unlockFloor.AvatarAddress;
                    break;
                case Wanted wanted:
                    avatarAddress = wanted.AvatarAddress;
                    break;
                case CustomEquipmentCraft customEquipmentCraft:
                    avatarAddress = customEquipmentCraft.AvatarAddress;
                    break;
                case BanGuildMember banGuildMember:
                    involvedAddresses.Add(banGuildMember.Target);
                    break;
                case MakeGuild makeGuild:
                    involvedAddresses.Add(makeGuild.ValidatorAddress);
                    break;
                case UnbanGuildMember unbanGuildMember:
                    involvedAddresses.Add(unbanGuildMember.Target);
                    break;
                case ActivateCollection activateCollection:
                    avatarAddress = activateCollection.AvatarAddress;
                    break;
                case BurnAsset burnAsset:
                    involvedAddresses.Add(burnAsset.Owner);
                    fungibleAssetValues.Add(burnAsset.Amount.ToString());
                    break;
                case CancelProductRegistration cancelProductRegistration:
                    avatarAddress = cancelProductRegistration.AvatarAddress;
                    foreach (var productInfo in cancelProductRegistration.ProductInfos)
                    {
                        fungibleAssetValues.Add(productInfo.Price.ToString());
                        involvedAvatarAddresses.Add(productInfo.AvatarAddress);
                        involvedAddresses.Add(productInfo.AgentAddress);
                    }
                    break;
                case IChargeActionPointV1 chargeActionPoint:
                    avatarAddress = chargeActionPoint.AvatarAddress;
                    break;
                case IClaimWordBossKillRewardV1 claimWordBossKillReward:
                    avatarAddress = claimWordBossKillReward.AvatarAddress;
                    break;
                case CreatePledge createPledge:
                    foreach (var agentAddressTuple in createPledge.AgentAddresses)
                    {
                        involvedAddresses.Add(agentAddressTuple.Item1);
                    }
                    break;
                case EndPledge endPledge:
                    involvedAddresses.Add(endPledge.AgentAddress);
                    break;
                case IEventConsumableItemCraftsV1 eventConsumableItemCrafts:
                    avatarAddress = eventConsumableItemCrafts.AvatarAddress;
                    break;
                case IEventDungeonBattleV1 eventDungeonBattle1:
                    avatarAddress = eventDungeonBattle1.AvatarAddress;
                    break;
                case IEventDungeonBattleV2 eventDungeonBattle2:
                    avatarAddress = eventDungeonBattle2.AvatarAddress;
                    break;
                case IEventMaterialItemCraftsV1 eventMaterialItemCrafts:
                    avatarAddress = eventMaterialItemCrafts.AvatarAddress;
                    break;
                case IHackAndSlashRandomBuffV1 hackAndSlashRandomBuff:
                    avatarAddress = hackAndSlashRandomBuff.AvatarAddress;
                    break;
                case IJoinArenaV1 joinArena:
                    avatarAddress = joinArena.AvatarAddress;
                    break;
                case MigrateAgentAvatar migrateAgentAvatar:
                    foreach (var agentAddress in migrateAgentAvatar.AgentAddresses)
                    {
                        involvedAddresses.Add(agentAddress);
                    }
                    break;
                case MigrateFee migrateFee:
                    foreach (var (s, r, a) in migrateFee.TransferData)
                    {
                        involvedAddresses.Add(s);
                        involvedAddresses.Add(r);
                    }
                    break;
                case IMigrateMonsterCollectionV1 migrateMonsterCollection:
                    avatarAddress = migrateMonsterCollection.AvatarAddress;
                    break;
                case MintAssets mintAssets:
                    if (mintAssets.MintSpecs is not null)
                    {
                        foreach (var mintSpec in mintAssets.MintSpecs)
                        {
                            involvedAvatarAddresses.Add(mintSpec.Recipient);
                            if (mintSpec.Assets is not null)
                            {
                                fungibleAssetValues.Add(mintSpec.Assets.Value.ToString());
                            }
                            if (mintSpec.Items is not null)
                            {
                                fungibleAssetValues.Add(mintSpec.Items.Value.ToString());
                            }
                        }
                    }
                    break;
                case IRankingBattleV1 rankingBattleV1:
                    avatarAddress = rankingBattleV1.AvatarAddress;
                    involvedAvatarAddresses.Add(rankingBattleV1.EnemyAddress);
                    break;
                case IRankingBattleV2 rankingBattleV2:
                    avatarAddress = rankingBattleV2.AvatarAddress;
                    involvedAvatarAddresses.Add(rankingBattleV2.EnemyAddress);
                    break;
                case IRedeemCodeV1 redeemCodeV1:
                    avatarAddress = redeemCodeV1.AvatarAddress;
                    break;
                case RegisterProduct registerProduct:
                    avatarAddress = registerProduct.AvatarAddress;
                    foreach (var ri in registerProduct.RegisterInfos)
                    {
                        involvedAvatarAddresses.Add(ri.AvatarAddress);
                        fungibleAssetValues.Add(ri.Price.ToString());
                    }
                    break;
                case RemoveAddressState removeAddressState:
                    foreach (var removal in removeAddressState.Removals)
                    {
                        involvedAddresses.Add(removal.accountAddress);
                        involvedAddresses.Add(removal.targetAddress);
                    }
                    break;
                case RequestPledge requestPledge:
                    involvedAddresses.Add(requestPledge.AgentAddress);
                    break;
                case ReRegisterProduct reRegisterProduct:
                    avatarAddress = reRegisterProduct.AvatarAddress;
                    foreach (var (pi, ri) in reRegisterProduct.ReRegisterInfos)
                    {
                        fungibleAssetValues.Add(pi.Price.ToString());
                        involvedAvatarAddresses.Add(pi.AvatarAddress);
                        involvedAddresses.Add(pi.AgentAddress);

                        involvedAvatarAddresses.Add(ri.AvatarAddress);
                        fungibleAssetValues.Add(ri.Price.ToString());
                    }
                    break;
                case RetrieveAvatarAssets retrieveAvatarAssets:
                    avatarAddress = retrieveAvatarAssets.AvatarAddress;
                    break;
                case ISecureMiningReward secureMiningReward:
                    involvedAddresses.Add(secureMiningReward.Recipient);
                    break;
                case ISellV2 sellV2:
                    avatarAddress = sellV2.SellerAvatarAddress;
                    fungibleAssetValues.Add(sellV2.Price.ToString());
                    break;
                case ISellV1 sellV1:
                    avatarAddress = sellV1.SellerAvatarAddress;
                    fungibleAssetValues.Add(sellV1.Price.ToString());
                    break;
                case ISellCancellationV3 sellCancellationV3:
                    avatarAddress = sellCancellationV3.SellerAvatarAddress;
                    break;
                case ISellCancellationV2 sellCancellationV2:
                    avatarAddress = sellCancellationV2.SellerAvatarAddress;
                    break;
                case ISellCancellationV1 sellCancellationV1:
                    avatarAddress = sellCancellationV1.SellerAvatarAddress;
                    break;
                case IUpdateSellV2 updateSellV2:
                    avatarAddress = updateSellV2.SellerAvatarAddress;
                    break;
                case IUpdateSellV1 updateSellV1:
                    avatarAddress = updateSellV1.SellerAvatarAddress;
                    break;
                default:
                    break;
            }

            return new ExtractedActionValues(
                typeId!,
                avatarAddress,
                sender,
                recipients,
                fungibleAssetValues,
                involvedAvatarAddresses.ToList(),
                involvedAddresses.ToList()
            );
        }
        catch (InvalidActionException)
        {
            return new ExtractedActionValues(typeId!, null, null, null, null, null, null);
        }
    }
}
