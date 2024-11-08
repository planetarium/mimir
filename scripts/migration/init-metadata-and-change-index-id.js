require('dotenv').config();
const { MongoClient } = require('mongodb');

const uri = process.env.MONGO_CONNECTION_STRING;
const client = new MongoClient(uri, { useNewUrlParser: true });

async function updateDocuments(dbName) {
  console.log(`Starting updates in database: ${dbName}`);
  const db = client.db(dbName);
  const collections = await db.listCollections().toArray();

  for (const collectionInfo of collections) {
    const collection = db.collection(collectionInfo.name);
    console.log(`\nProcessing collection: ${collectionInfo.name}`);

    const cursor = collection.find({ Address: { $exists: true } });
    let count = 0;

    while (await cursor.hasNext()) {
      const doc = await cursor.next();

      if (!doc.Address) {
        console.log(`\nNot found 'Address', skip`);

        continue;
      }

      let newDoc = { ...doc };
      count += 1;

      console.log(`\nProcessing document with original _id: ${doc._id}`);

      if (doc.Address) {
        console.log(` - Address key found. Setting _id to Address value: ${doc.Address}`);
        newDoc._id = doc.Address;
        delete newDoc.Address;
      }

      if (!doc.Metadata) {
        console.log(" - Metadata key not found. Adding default Metadata.");
        newDoc.Metadata = { SchemaVersion: 1, StoredBlockIndex: 1 };
      }

      try {
        console.log(" - Inserting the new document...");
        await collection.insertOne(newDoc);
        console.log(" - Document inserted successfully.");

        console.log(" - Deleting the original document...");
        await collection.deleteOne({ _id: doc._id });
        console.log(" - Original document deleted successfully.");
      } catch (err) {
        console.error(`Error while processing document with _id: ${doc._id} - ${err}`);
      }
    }

    console.log(`\nFinished processing collection: ${collectionInfo.name}. Updated ${count} documents.`);
  }

  console.log(`Completed updates in database: ${dbName}`);
}

async function main() {
  try {
    await client.connect();
    console.log("Connected to MongoDB.");

    await updateDocuments('odin');
    await updateDocuments('heimdall');

    console.log("All updates completed successfully.");
  } catch (err) {
    console.error("An error occurred during the update process:", err);
  } finally {
    await client.close();
    console.log("Disconnected from MongoDB.");
  }
}

main();
