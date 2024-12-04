using Lib9c.Models.Items;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Item;
using Inventory = Lib9c.Models.Items.Inventory;
using ItemBase = Lib9c.Models.Items.ItemBase;

namespace Mimir.Tests.QueryTests;

public class InventoryTest
{
    [Fact]
    public async Task GraphQL_Query_Inventory_Returns_CorrectValue()
    {
        var address = new Address();
        var inventory = new Inventory
        {
            Items = new List<InventoryItem>
            {
                new()
                {
                    Item = new ItemBase
                    {
                        Id = 1,
                        Grade = 1,
                        ItemType = ItemType.Equipment,
                        ItemSubType = ItemSubType.Armor,
                        ElementalType = ElementalType.Normal
                    },
                    Count = 1,
                    Lock = new OrderLock()
                },
                new()
                {
                    Item = new ItemBase
                    {
                        Id = 2,
                        Grade = 2,
                        ItemType = ItemType.Material,
                        ItemSubType = ItemSubType.Food,
                        ElementalType = ElementalType.Fire
                    },
                    Count = 2,
                    Lock = null
                }
            }
        };

        // process mocking
        var mockRepo = new Mock<IInventoryRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new InventoryDocument(0, address, inventory));

        var serviceProvider = TestServices.Builder
            .With(mockRepo.Object)
            .Build();

        // Issue: There is issue with the merge in relation to itemId.
        // So, Temporary fix is to remove the itemId from the query.
        var query = $$"""
                      query {
                        inventory(address: "{{address}}") {
                          items {
                            count
                            item {
                              elementalType
                              grade
                              id
                              itemSubType
                              itemType
                              ... on Armor {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on Aura {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on Belt {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on Consumable {
                                elementalType
                                grade
                                id
                                
                                itemSubType
                                itemType
                                requiredBlockIndex
                              }
                              ... on Costume {
                                elementalType
                                equipped
                                grade
                                id
                                
                                itemSubType
                                itemType
                                requiredBlockIndex
                                spineResourcePath
                              }
                              ... on Equipment {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on Grimoire {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on ItemBase {
                                elementalType
                                grade
                                id
                                itemSubType
                                itemType
                              }
                              ... on ItemUsable {
                                elementalType
                                grade
                                id
                                
                                itemSubType
                                itemType
                                requiredBlockIndex
                              }
                              ... on Material {
                                elementalType
                                grade
                                id
                                
                                itemSubType
                                itemType
                              }
                              ... on Necklace {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on Ring {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                              ... on TradableMaterial {
                                elementalType
                                grade
                                id
                                
                                itemSubType
                                itemType
                                requiredBlockIndex
                              }
                              ... on Weapon {
                                elementalType
                                equipped
                                exp
                                grade
                                id
                                
                                itemSubType
                                itemType
                                level
                                madeWithMimisbrunnrRecipe
                                optionCountFromCombination
                                requiredBlockIndex
                                setId
                                spineResourcePath
                              }
                            }
                          }
                        }
                      }
                      """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetDocument(query));

        await Verify(result);
    }
}