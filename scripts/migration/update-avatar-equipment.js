require('dotenv').config();
const { MongoClient } = require('mongodb');
const HeadlessGqlClient = require('./utils/headlessGqlClient');
const { createProgressTracker, chunkArray } = require('./utils/common');

// 환경변수 확인
const uri = process.env.MONGO_CONNECTION_STRING;
const odinGqlUrl = process.env.ODIN_GQL_URL;
const heimdallGqlUrl = process.env.HEIMDALL_GQL_URL;

if (!uri) {
  console.error('MONGO_CONNECTION_STRING 환경변수가 설정되지 않았습니다.');
  process.exit(1);
}

if (!odinGqlUrl) {
  console.error('ODIN_GQL_URL 환경변수가 설정되지 않았습니다.');
  process.exit(1);
}

if (!heimdallGqlUrl) {
  console.error('HEIMDALL_GQL_URL 환경변수가 설정되지 않았습니다.');
  process.exit(1);
}

const client = new MongoClient(uri);

async function updateAvatarEquipment(dbName, gqlUrl) {
  console.log(`${dbName} 데이터베이스 업데이트 시작 (GQL: ${gqlUrl})`);
  let db;
  let collection;
  
  const gqlClient = new HeadlessGqlClient(gqlUrl, 0.5); // 0.5초 딜레이
  
  if (typeof dbName === 'string') {
    db = client.db(dbName);
    collection = db.collection('avatar');
  } else {
    db = dbName;
    collection = db.collection('avatar');
  }
  
  // SchemaVersion이 1인 문서만 조회
  const cursor = collection.find({ 'Metadata.SchemaVersion': 1 });
  const count = await cursor.count();
  
  if (count === 0) {
    console.log(`${typeof dbName === 'string' ? dbName : 'DB'} 데이터베이스에 업데이트할 아바타 문서가 없습니다.`);
    return 0;
  }
  
  console.log(`${typeof dbName === 'string' ? dbName : 'DB'} 데이터베이스에서 ${count}개의 아바타 문서를 업데이트합니다.`);
  const tracker = createProgressTracker(count);
  
  let updatedCount = 0;
  let errorCount = 0;
  let skipCount = 0;
  
  while (await cursor.hasNext()) {
    const doc = await cursor.next();
    const avatarAddress = doc._id;
    
    try {
      // GQL을 통해 장비 정보 가져오기
      const { armorId, portraitId } = await gqlClient.getAvatarEquipment(avatarAddress);
      
      // 업데이트 수행
      await collection.updateOne(
        { _id: avatarAddress },
        { 
          $set: { 
            'Metadata.SchemaVersion': 2,
            ArmorId: armorId,
            PortraitId: portraitId
          } 
        }
      );
      
      console.log(`아바타 ${avatarAddress} 업데이트 완료: ArmorId=${armorId}, PortraitId=${portraitId}`);
      updatedCount++;
    } catch (err) {
      console.error(`아바타 ${avatarAddress} 업데이트 실패: ${err.message}`);
      errorCount++;
    }
    
    tracker.increment();
  }
  
  console.log(`${typeof dbName === 'string' ? dbName : 'DB'} 데이터베이스 업데이트 완료`);
  console.log(`- 업데이트 성공: ${updatedCount}개`);
  console.log(`- 에러 발생: ${errorCount}개`);
  console.log(`- 건너뜀: ${skipCount}개`);
  
  return updatedCount;
}

async function main() {
  try {
    await client.connect();
    console.log("MongoDB 연결 성공");
    
    const odinCount = await updateAvatarEquipment('odin', odinGqlUrl);
    const heimdallCount = await updateAvatarEquipment('heimdall', heimdallGqlUrl);
    
    console.log(`모든 업데이트 완료. 총 ${odinCount + heimdallCount}개 아바타 문서 업데이트됨.`);
  } catch (err) {
    console.error("업데이트 과정에서 오류 발생:", err);
  } finally {
    await client.close();
    console.log("MongoDB 연결 종료");
  }
}

// 스크립트 실행
if (require.main === module) {
  main();
}

module.exports = updateAvatarEquipment; 