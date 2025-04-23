const fetch = require('node-fetch');
const { sleep } = require('./common');

class HeadlessGqlClient {
  constructor(url, delayMs = 500) {
    this.url = url;
    this.delayMs = delayMs;
  }

  async request(query, variables = {}) {
    try {
      const response = await fetch(this.url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          query,
          variables,
        }),
      });

      if (!response.ok) {
        throw new Error(`HTTP 오류: ${response.status}`);
      }

      const result = await response.json();
      
      if (result.errors) {
        throw new Error(`GraphQL 오류: ${JSON.stringify(result.errors)}`);
      }

      await sleep(this.delayMs);
      return result.data;
    } catch (error) {
      console.error(`GQL 요청 중 오류 발생: ${error.message}`);
      throw error;
    }
  }

  async getAvatar(address) {
    const query = `
      query GetAvatar($address: Address!) {
        stateQuery {
          avatar(avatarAddress: $address) {
            address
            inventory {
              equipments {
                id
                itemSubType
                equipped
              }
              costumes {
                id
                itemSubType
                equipped
              }
            }
          }
        }
    }
    `;

    const result = await this.request(query, { address });

    return result;
  }

  async getAvatarEquipment(address) {
    const avatar = await this.getAvatar(address);
    
    if (!avatar) {
      return { armorId: null, portraitId: null };
    }

    const inventory = avatar.stateQuery.avatar.inventory;
    
    // 방어구 ID 찾기
    const armor = inventory?.equipments?.find(e => e.equipped && e.itemSubType === 'ARMOR');
    const armorId = armor ? parseInt(armor.id, 10) : null;
    
    // 초상화 ID 찾기
    const fullCostume = inventory?.costumes?.find(c => c.itemSubType === 'FULL_COSTUME' && c.equipped);
    // 전신 코스튬이 없으면 방어구 ID를 사용
    const portraitId = fullCostume ? parseInt(fullCostume.id, 10) : armorId;
    
    return { armorId, portraitId };
  }
}

module.exports = HeadlessGqlClient;