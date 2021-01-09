using System.Collections.Generic;
using Wintellect.PowerCollections;

namespace FlashMemX.Core
{
    public class UniqueCardOrderer : CardOrderer
    {
        private readonly List<BigList<Stack<CardMetadata>>> cardMetadataPool;

        private readonly Dictionary<long, int> schemaIndices;
        private readonly List<Dictionary<long, int>> contentIdIndices;

        private int schemaIterator;
        private List<int> contentIdIterators;

        private int metadataCount;

        public UniqueCardOrderer()
        {
            schemaIndices = new Dictionary<long, int>();
            contentIdIndices = new List<Dictionary<long, int>>();
            cardMetadataPool = new List<BigList<Stack<CardMetadata>>>();
        }

        public override void Add(CardMetadata metadata)
        {
            if(SchemaHasNotBeenInserted(metadata.SchemaId))
            {
                cardMetadataPool.Add(new BigList<Stack<CardMetadata>>());
                schemaIndices.Add(metadata.SchemaId, cardMetadataPool.Count - 1);
                contentIdIndices.Add(new Dictionary<long, int>());
            }

            int schemaIndex = schemaIndices[metadata.SchemaId];

            if(ContentIdHasNotBeenInserted(metadata, schemaIndex))
            {
                cardMetadataPool[schemaIndex].Add(new Stack<CardMetadata>());

                int i = cardMetadataPool[schemaIndex].Count - 1;
                contentIdIndices[schemaIndex].Add(metadata.ContentId, i);
            }

            int contentIdIndex = contentIdIndices[schemaIndex][metadata.ContentId];
            cardMetadataPool[schemaIndex][contentIdIndex].Push(metadata);
            ++metadataCount;
        }

        private bool ContentIdHasNotBeenInserted(CardMetadata metadata, int schemaIndex)
        {
            return !contentIdIndices[schemaIndex].ContainsKey(metadata.ContentId);
        }

        private bool SchemaHasNotBeenInserted(long schemaId)
        {
            return !schemaIndices.ContainsKey(schemaId);
        }

        public override List<CardMetadata> ToList()
        {
            var metadataList = new List<CardMetadata>(metadataCount);
            schemaIterator = 0;
            contentIdIterators = new List<int>(cardMetadataPool.Count);
            for(int i = 0; i < cardMetadataPool.Count; ++i)
                contentIdIterators.Add(0);

            while(cardMetadataPool.Count > 0)
            {
                metadataList.Add(PopNextMetadata());

                if(CurrentStackEmpty())
                {
                    RemoveCurrentStack();

                    if(CurrentListEmpty())
                        RemoveCurrentList();
                    else
                        IncrementSchemaIterator();
                }
                else
                {
                    IncrementIdIterator();
                    IncrementSchemaIterator();
                }
            }

            return metadataList;
        }

        private void IncrementIdIterator()
        {
            int i = contentIdIterators[schemaIterator] + 1;
            contentIdIterators[schemaIterator] = i < cardMetadataPool[schemaIterator].Count ? i : 0;
        }

        private void IncrementSchemaIterator()
        {
            int i = schemaIterator + 1;
            schemaIterator = i < cardMetadataPool.Count ? i : 0;
        }

        private void RemoveCurrentList()
        {
            cardMetadataPool.RemoveAt(schemaIterator);
        }

        private bool CurrentListEmpty()
        {
            return cardMetadataPool[schemaIterator].Count == 0;
        }

        private void RemoveCurrentStack()
        {
            cardMetadataPool[schemaIterator].RemoveAt(contentIdIterators[schemaIterator]);
        }

        private CardMetadata PopNextMetadata()
        {
            return cardMetadataPool[schemaIterator][contentIdIterators[schemaIterator]].Pop();
        }

        private bool CurrentStackEmpty()
        {
            return cardMetadataPool[schemaIterator][contentIdIterators[schemaIterator]].Count == 0;
        }
    }
}
