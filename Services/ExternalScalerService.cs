using System;
using System.Threading.Tasks;
using Externalscaler;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace kaboom_scaler
{
    public class ExternalScalerService : ExternalScaler.ExternalScalerBase
    {
        private readonly ILogger<ExternalScalerService> _logger;
        private TwitchClient _client ;
        private int state;
        public ExternalScalerService(ILogger<ExternalScalerService> logger)
        {
            _logger = logger;
        }

        public override Task<Empty> New(NewRequest request, ServerCallContext context)
        {
            _logger.LogInformation("New Is Called, the problem is between the screen and the chair!");
            var accessToken = request.Metadata["accessToken"];
            var twitchUserName = request.Metadata["twitchUserName"];
            var channelName = request.Metadata["channelName"];

            _logger.LogInformation($"kaboom scaler new: twitchname:{twitchUserName}, channelName:{channelName}");

            ConnectionCredentials credentials = new ConnectionCredentials(twitchUserName, accessToken);
	        var clientOptions = new ClientOptions
                {
                    MessagesAllowedInPeriod = 750,
                    ThrottlingPeriod = TimeSpan.FromSeconds(30)
                };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials, channelName);

            _client.OnMessageReceived += Client_OnMessageReceived;
            _client.OnLog += Client_OnLog;
            _client.Connect();

            return Task.FromResult(new Empty());
        }

        private void Client_OnLog(object sender, OnLogArgs e)
        {
            _logger.LogInformation($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }
        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            _logger.LogInformation($"Twitch message Received: {e.ChatMessage.Message}");
             if (e.ChatMessage.Message.Contains("kaboom"))
             {
                 state = 10;
             }
             if (e.ChatMessage.Message.Contains("fssst"))
             {
                 state = 2;
             }
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
            _logger.LogInformation($"GetMetrics Called: state: {state}");
            var response = new GetMetricsResponse();
            response.MetricValues.Add(new MetricValue{MetricName="kaboom", MetricValue_= state });
            return Task.FromResult<GetMetricsResponse>(response);
        }

        public override Task<Empty> Close(ScaledObjectRef request, ServerCallContext context)
        {
            return Task.FromResult<Empty>(new Empty());
        }

    }
}