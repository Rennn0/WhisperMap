using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Webhook.Objects;
using XcLib.Data.Abstractions;
using XcLib.Data.ApplicationObjects;
using XcLib.Shared.Reactive.Interfaces;

namespace Webhook;

public static partial class Program
{
    private static void ApiPostWebhook(this RouteGroupBuilder builder)
    {
        builder.MapPost("{idx:int}",
            ([FromRoute] int idx, [FromBody] DockerWebhookRequest request,
                [FromServices] IReactiveBus<DockerWebhookRequest> bus) =>
            {
                if (request is not { Push.Tag: "latest" }) return Results.BadRequest();
                request.Index = (sbyte)idx;
                bus.Publish(request);
                
                return Results.Ok(new
                {
                    success = true,
                    idx,
                    started = true
                });
            });

        builder.MapPost("payment/order/{orderId}",
            async (
                [FromRoute] string orderId,
                HttpContext context,
                [FromServices] ILoggerFactory loggerFactory,
                [FromServices] IProductOrderRepo orderRepo) =>
            {
                ILogger logger = loggerFactory.CreateLogger(orderId);
                using StreamReader reader = new StreamReader(context.Request.Body);
                string json = await reader.ReadToEndAsync();
                logger.LogInformation(new EventId(9), json);

                using JsonDocument document = JsonDocument.Parse(json);
                document.RootElement.GetProperty("payment_id").TryGetInt64(out long paymentId);
                string? orderStatus = document.RootElement.GetProperty("order_status").ToString();

                List<ProductOrder> existingOrder =
                    await orderRepo.GetAsync(new ProductOrder { InternalOrderId = orderId }, 4);
                await Task.WhenAll(existingOrder.Select(eo => orderRepo.UpdateAsync(eo with
                {
                    OrderStatus = orderStatus,
                    ProviderOrderId = paymentId.ToString()
                })));

                return Results.Ok();
            });
    }
}