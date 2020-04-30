using System.Threading.Tasks;
using Externalscaler;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace kaboom_scaler
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private readonly ILogger<ExternalScalerService> _logger;
        public ExternalScalerService(ILogger<ExternalScalerService> logger)
        {
            _logger = logger;
        }

        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }

        public override Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
        {
            return Task.FromResult(new IsActiveResponse(){Result = true});
        }

        public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
        {
            var metricsSpec = new MetricSpec();
            metricsSpec.MetricName = "kaboom";
            metricsSpec.TargetSize = 1;
            var response = new GetMetricSpecResponse();
            response.MetricSpecs.Add(metricsSpec);

            return Task.FromResult<GetMetricSpecResponse>(response);

        }

        public override Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
        {
            var response = new GetMetricsResponse();
            response.MetricValues.Add(new MetricValue{MetricName="kaboom", MetricValue_= 5 });
            return Task.FromResult<GetMetricsResponse>(response);
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            return Task.FromResult<Empty>(new Empty());
        }

    }
}