using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Rachel.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;

        public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IIpPolicyStore policyStore, IRateLimitCounterStore counterStore, IRateLimitConfiguration config)
        {
            var endpoint = context.Request.Path.ToString().ToLowerInvariant();
            var policy = config.GetPolicy(endpoint);
            var clientIp = context.Connection.RemoteIpAddress.ToString();
            var rules = policy.IpRules;
            var rateLimits = policyStore.GetIpPolicy(clientIp).IpRules;

            foreach (var rule in rules)
            {
                var rateLimit = rateLimits.FirstOrDefault(r => r.Endpoint == rule.Endpoint);
                if (rateLimit == null)
                {
                    rateLimit = new IpRateLimitPolicy
                    {
                        Endpoint = rule.Endpoint,
                        Limit = rule.Limit,
                        Period = rule.Period
                    };
                    rateLimits.Add(rateLimit);
                }
                var counter = await counterStore.GetAsync($"{clientIp}_{endpoint}_{rule.Period}");
                if (counter == null)
                {
                    counter = new RateLimitCounter
                    {
                        Timestamp = DateTime.Now,
                        Count = 1
                    };
                    await counterStore.SetAsync($"{clientIp}_{endpoint}_{rule.Period}", counter, rule.Period);
                }
                else
                {
                    counter.Count++;
                    await counterStore.SetAsync($"{clientIp}_{endpoint}_{rule.Period}", counter);
                }
                context.Response.Headers.Add("X-Rate-Limit-Limit", rule.Limit.ToString());
                context.Response.Headers.Add("X-Rate-Limit-Remaining", (rule.Limit - counter.Count).ToString());
                context.Response.Headers.Add("X-Rate-Limit-Reset", counter.Timestamp.AddSeconds(rule.Period).ToString("O"));
                if (counter.Count > rule.Limit)
                {
                    _logger.LogInformation($"Rate limit exceeded: {clientIp}, Endpoint: {endpoint}, Rule: {rule.Endpoint}");
                    context.Response.StatusCode = policy.HttpStatusCode;
                    await context.Response.WriteAsync($"Rate limit exceeded. Try again in {counter.Timestamp.AddSeconds(rule.Period).Subtract(System.DateTime.Now).Seconds} seconds.");
                    return;
                }
            }
            await _next(context);
        }
    }
}

