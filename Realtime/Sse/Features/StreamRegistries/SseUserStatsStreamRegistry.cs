using Realtime.Sse.Core.Stream;
using Realtime.Sse.Formatters;

namespace Realtime.Sse.Features.StreamRegistries;

internal class SseUserStatsStreamRegistry : SseStreamRegistry<SseUserStatsFormatter.UserStats>;