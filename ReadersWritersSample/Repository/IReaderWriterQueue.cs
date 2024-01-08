namespace ReadersWritersSample.Repository;

public interface IReaderWriterQueue
{
    void AddReader(Action action);
    void AddWriter(Action action);
    Task ProcessQueueAsync(string mode);
    (int readers, int writers) GetStatus();
}