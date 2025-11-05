using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.LoadBalancing;

public delegate Priority PriorityEvaluation(IRequest request);
