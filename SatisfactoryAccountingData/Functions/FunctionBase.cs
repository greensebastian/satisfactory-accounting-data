using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace SatisfactoryAccountingData.Functions
{
    public abstract class FunctionBase<TChild> where TChild : FunctionBase<TChild>
    {
        protected readonly ILogger<TChild> Log;

        protected FunctionBase(ILogger<TChild> log)
        {
            Log = log;
        }

        protected CancellationToken CombineTokens(HttpRequest req, CancellationToken hostCancellactionToken)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(hostCancellactionToken, req.HttpContext.RequestAborted).Token;
        }
    }
}
