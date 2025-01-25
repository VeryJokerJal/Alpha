using System.Collections;

namespace Alpha.Models
{
    public class OperatorResponse : IGrouping<string, OperatorItemResponse>
    {
        private readonly IEnumerable<OperatorItemResponse> _operators;

        public OperatorResponse(string category, IEnumerable<OperatorItemResponse> operators)
        {
            Key = category;
            _operators = operators;
        }

        public string Key { get; }

        public IEnumerator<OperatorItemResponse> GetEnumerator()
        {
            return _operators.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class OperatorItemResponse
    {
        public string? Name { get; set; }
        public string? Category { get; set; }
        public List<string>? Scope { get; set; }
        public string? Definition { get; set; }
        public string? Description { get; set; }
        public string? Documentation { get; set; }
    }
}
