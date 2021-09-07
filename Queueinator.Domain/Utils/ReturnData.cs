using System.Collections.Generic;

namespace Queueinator.Domain.Utils
{
    public class ReturnData
    {
        public ReturnData(IEnumerable<object> data)
        {
            Data = data;
        }

        public IEnumerable<object> Data { get; set; }

    }
}
