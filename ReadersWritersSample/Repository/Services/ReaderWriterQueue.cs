namespace ReadersWritersSample.Repository.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class ReaderWriterQueue : IReaderWriterQueue
{
    private readonly SemaphoreSlim _resourceSemaphore = new(1, 1);
    private readonly Queue<Action> _readerQueue = new();
    private readonly Queue<Action> _writerQueue = new();
    private int _activeReaders = 0;

    public void AddReader(Action readerAction)
    {
        lock (_readerQueue)
        {
            _readerQueue.Enqueue(readerAction);
        }
    }

    public void AddWriter(Action writerAction)
    {
        lock (_writerQueue)
        {
            _writerQueue.Enqueue(writerAction);
        }
    }

    public async Task ProcessQueueAsync(string mode)
    {
        switch (mode.ToLower())
        {
            case "reader":
                await ProcessReadersFirstAsync();
                break;
            case "writer":
                await ProcessWritersFirstAsync();
                break;
            case "optimized":
                await ProcessOptimizedAsync();
                break;
            default:
                throw new ArgumentException("Invalid mode. Available modes: reader, writer, optimized.");
        }
    }

    public (int readers, int writers) GetStatus()
    {
        lock (_readerQueue)
        {
            lock (_writerQueue)
            {
                return (_readerQueue.Count, _writerQueue.Count);
            }
        }
    }

    public async Task ProcessReadersFirstAsync()
    {
        while (_readerQueue.Count > 0 || _writerQueue.Count > 0)
        {
            while (_readerQueue.Count > 0)
            {
                // Process readers.
                Action readerAction;
                lock (_readerQueue)
                {
                    readerAction = _readerQueue.Dequeue();
                }
            
                // No resource lock needed as readers can run concurrently.
                readerAction.Invoke();

                // We don't increment or decrement _activeReaders in this simplified example.
                // Implementing readers' ability to run concurrently would require more complex handling.
            }

            if (_writerQueue.Count > 0)
            {
                // Process single writer.
                Action writerAction;
                lock (_writerQueue)
                {
                    writerAction = _writerQueue.Dequeue();
                }

                await _resourceSemaphore.WaitAsync(); // Ensure exclusive access
                writerAction.Invoke();
                _resourceSemaphore.Release();
            }
        
            await Task.Delay(10); // Small delay to prevent tight looping.
        }
    }


    public async Task ProcessWritersFirstAsync()
    {
        while (_readerQueue.Count > 0 || _writerQueue.Count > 0)
        {
            while (_writerQueue.Count > 0)
            {
                // Process writers.
                Action writerAction;
                lock (_writerQueue)
                {
                    writerAction = _writerQueue.Dequeue();
                }

                await _resourceSemaphore.WaitAsync(); // Ensure exclusive access
                writerAction.Invoke();
                _resourceSemaphore.Release();
            }

            if (_readerQueue.Count > 0)
            {
                // Process single reader.
                Action readerAction;
                lock (_readerQueue)
                {
                    readerAction = _readerQueue.Dequeue();
                }

                // No resource lock needed as readers can run concurrently.
                readerAction.Invoke();

                // Similar to above, more sophisticated handling would be needed to
                // allow true concurrent reading.
            }
        
            await Task.Delay(10); // Small delay to prevent tight looping.
        }
    }
    
    public async Task ProcessOptimizedAsync()
    {
        while (_readerQueue.Count > 0 || _writerQueue.Count > 0)
        {
            // Check and process a reader if available
            if (_readerQueue.Count > 0)
            {
                Action readerAction = null;
                lock (_readerQueue)
                {
                    if (_readerQueue.TryDequeue(out readerAction))
                    {
                        // Process the reader action immediately without delay
                        readerAction.Invoke();
                    }
                }
            }

            // Check and process a writer if available, ensuring exclusive access
            if (_writerQueue.Count > 0)
            {
                Action writerAction = null;
                lock (_writerQueue)
                {
                    if (_writerQueue.TryDequeue(out writerAction))
                    { 
                        _resourceSemaphore.Wait(); // Exclusive access for writer
                        writerAction.Invoke();
                        _resourceSemaphore.Release();
                    }
                }
            }

            // Since we're not simulating time, we just yield to other awaiting operations, if any
            await Task.Yield();
        }
    }
}