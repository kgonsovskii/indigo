namespace Poller.Application;

public interface IFeedConnectionTelemetry
{
    void RecordConnected(string feedName, int laneIndex, string parserName, Uri webSocketUri);

    void RecordDisconnected(string feedName, int laneIndex);

    void RecordConnectionError(string feedName, int laneIndex, Exception exception, TimeSpan reconnectDelay);

    void RecordLaneCrashed(string feedName, int laneIndex, Exception exception, TimeSpan restartDelay);

    IReadOnlyDictionary<string, int> GetActiveLanesSnapshot();
}
