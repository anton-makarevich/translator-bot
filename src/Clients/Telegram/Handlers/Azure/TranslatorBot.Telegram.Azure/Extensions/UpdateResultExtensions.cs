using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;

namespace Sanet.Bots.Telegram.Azure.Extensions;

public static class UpdateResultExtensions
{
    public static IActionResult ToAzureActionResult(this UpdateHandlerResult result, ILogger log)
    {
        if (!string.IsNullOrEmpty(result.Message))
        {
            log.LogError("{Message}",result.Message);
        }

        return result.Status switch
        {
            200 => new OkResult(),
            <= 499 and >= 400 => new BadRequestResult(),
            _ => new InternalServerErrorResult()
        };
    }
}