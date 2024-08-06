using System.Collections.Generic;

namespace Mimir.Worker.Client
{
    public class GetAccountDiffsRequest
    {
        public long BaseIndex { get; set; }
        public long ChangedIndex { get; set; }
        public string AccountAddress { get; set; }
    }

    public class StateDiff
    {
        public string path { get; set; }
        public string baseState { get; set; }
        public string changedState { get; set; }
    }

    public class GetAccountDiffsResponse
    {
        public List<StateDiff> accountDiffs { get; set; }
    }

    public class GraphQLRequest
    {
        public string query { get; set; }
        public Dictionary<string, object> variables { get; set; }
    }

    public class GraphQLResponse<T>
    {
        public T data { get; set; }
    }
}
