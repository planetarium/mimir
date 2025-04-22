require('dotenv').config();
const { MongoClient } = require('mongodb');

const uri = process.env.MONGO_CONNECTION_STRING;
const client = new MongoClient(uri, { useNewUrlParser: true });

async function updateLastStageClearedId(dbName) {
  console.log(`${dbName} 데이터베이스 업데이트 시작`);
  let db;
  let collection;
  
  if (typeof dbName === 'string') {
    db = client.db(dbName);
    collection = db.collection('world_information');
  } else {
    db = dbName;
    collection = db.collection('world_information');
  }
  
  const cursor = collection.find({ 
    'Object.WorldDictionary': { $exists: true },
    'Metadata.SchemaVersion': 1
  });
  let count = 0;
  
  while (await cursor.hasNext()) {
    const doc = await cursor.next();
    const worldDictionary = doc.Object?.WorldDictionary;
    
    if (!worldDictionary) {
      console.log(`WorldDictionary가 존재하지 않음, 건너뜀: ${doc._id}`);
      continue;
    }
    
    const worldKeys = Object.keys(worldDictionary)
      .filter(key => !isNaN(Number(key)) && Number(key) < 10000) // 10001과 같은 특수 ID 제외
      .map(key => Number(key));
    
    if (worldKeys.length === 0) {
      console.log(`유효한 월드 키가 없음, 건너뜀: ${doc._id}`);
      continue;
    }
    
    // 모든 월드를 순회하며 StageClearedId가 -1이 아닌 가장 높은 값 찾기
    let highestStageClearedId = -1;
    let highestWorldId = -1;
    
    for (const key of worldKeys) {
      const world = worldDictionary[key];
      if (world && world.StageClearedId !== undefined && world.StageClearedId !== -1) {
        if (world.StageClearedId > highestStageClearedId) {
          highestStageClearedId = world.StageClearedId;
          highestWorldId = key;
        }
      }
    }
    
    if (highestStageClearedId === -1) {
      console.log(`StageClearedId가 -1이 아닌 월드가 없음, 건너뜀: ${doc._id}`);
      continue;
    }
    
    console.log(`문서 ID: ${doc._id}, 월드 ID: ${highestWorldId}, 가장 높은 StageClearedId: ${highestStageClearedId}`);
    
    try {
      await collection.updateOne(
        { _id: doc._id },
        { 
          $set: { 
            LastStageClearedId: highestStageClearedId,
            'Metadata.SchemaVersion': 2
          } 
        }
      );
      console.log(`LastStageClearedId 업데이트 완료: ${highestStageClearedId}, SchemaVersion: 2`);
      count++;
    } catch (err) {
      console.error(`문서 업데이트 오류 (ID: ${doc._id}): ${err}`);
    }
  }
  
  console.log(`${typeof dbName === 'string' ? dbName : 'DB'} 데이터베이스 업데이트 완료. ${count}개 문서 수정됨.`);
  return count;
}

async function main() {
  try {
    await client.connect();
    console.log("MongoDB 연결 성공");
    
    const odinCount = await updateLastStageClearedId('odin');
    const heimdallCount = await updateLastStageClearedId('heimdall');
    
    console.log(`모든 업데이트 완료. 총 ${odinCount + heimdallCount}개 문서 수정됨.`);
  } catch (err) {
    console.error("업데이트 과정에서 오류 발생:", err);
  } finally {
    await client.close();
    console.log("MongoDB 연결 종료");
  }
}

if (require.main === module) {
  main();
}

module.exports = updateLastStageClearedId; 