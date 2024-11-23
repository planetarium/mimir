using Lib9c.Models.Mails;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Moq;
using Nekoyume.Model;

namespace Mimir.Tests.QueryTests;

public class AvatarQueryTest
{
    [Fact]
    public async Task GraphQL_Query_Avatar_Returns_CorrectValue()
    {
        var mockAddress = new Address("0x0000000000000000000000000000000000000000");

        var stageMap = new CollectionMap();
        var monsterMap = new CollectionMap();
        var itemMap = new CollectionMap();
        var eventMap = new CollectionMap();
        Mail mail = new Mail()
        {
          BlockIndex = 0,
          Id = Guid.Parse("a9320f02-7794-4c3b-8071-701d501f9529"),
          RequiredBlockIndex = 0,
          TypeId = "A"
        };
        
        List<Mail> mails = new();
        mails.Add(mail);
        MailBox mailBox = new MailBox() {Mails = mails};  
        
        stageMap.Add(1, 1);
        monsterMap.Add(1, 1);
        itemMap.Add(1, 1);
        eventMap.Add(1, 1);
        
        var avatar = new AvatarState
        {
          Address = default,
          Version = 1,
          AgentAddress = new Address("0x0000000000000000000000000000000000000001"),
          MailBox = mailBox,
          BlockIndex = 0,
          DailyRewardReceivedIndex = 0,
          ActionPoint = 0,
          StageMap = stageMap,
          MonsterMap = monsterMap,
          ItemMap = itemMap,
          EventMap = eventMap,
          Hair = 0,
          Lens = 0,
          Ear = 0,
          Tail = 0,
          CombinationSlotAddresses = new List<Address>(){new Address()},
          RankingMapAddress = default,
          Name = "TestAvatar",
          CharacterId = 1,
          Level = 0,
          Exp = 0,
          UpdatedAt = 0,



          // Add other mock data as needed
        };

        var mockRepo = new Mock<IAvatarRepository>();
        mockRepo
            .Setup(repo => repo.GetByAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync(new AvatarDocument(0, new Address(), avatar));
        
        var serviceProvider = TestServices.CreateServices(avatarRepositoryMock: mockRepo);
        
        var query = """
                        query {
                          avatar(address: "0x0000000000000000000000000000000000000000") {
                            address
                            agentAddress
                            blockIndex
                            characterId
                            name
                            level
                            eventMap {
                              key
                              value
                            }
                            itemMap {
                              key
                              value
                            }
                            mailBox {
                              mails {
                                blockIndex
                                id
                                requiredBlockIndex
                                typeId
                              }
                            }
                            monsterMap {
                              key
                              value
                            }
                            stageMap {
                              key
                              value
                            }
                          }
                        }
                    """;

        var result = await TestServices.ExecuteRequestAsync(serviceProvider, b => b.SetQuery(query));

        await Verify(result);

    }
}