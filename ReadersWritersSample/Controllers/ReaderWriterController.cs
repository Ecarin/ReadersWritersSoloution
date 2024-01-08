using Microsoft.AspNetCore.Mvc;
using ReadersWritersSample.Repository;

namespace ReadersWritersSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReaderWriterController(IReaderWriterQueue queue) : ControllerBase
{

    [HttpPost("addReader")]
    public IActionResult AddReader()
    {
        queue.AddReader(() =>
        {
            var status = queue.GetStatus();
            var result = new 
            { 
                readersInQueue = status.readers,
                writersInQueue = status.writers
            };
            Console.Clear();
            Console.WriteLine(result);
            Console.WriteLine("Reader action done!");
            Thread.Sleep(100);
        });
        return Ok("Reader added to queue.");
    }

    [HttpPost("addWriter")]
    public IActionResult AddWriter()
    {
        queue.AddWriter(() =>
        {
            var status = queue.GetStatus();
            var result = new 
            { 
                readersInQueue = status.readers,
                writersInQueue = status.writers
            };
            Console.Clear();
            Console.WriteLine(result);
            Console.WriteLine("Writer action done!");
            Thread.Sleep(2000);
        }); 
        return Ok("Writer added to queue.");
    }

    [HttpPost("processQueue/{mode}")]
    public async Task<IActionResult> ProcessQueue(string mode)
    {
        await queue.ProcessQueueAsync(mode);
        return Ok($"Processed queue in {mode} mode.");
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var status = queue.GetStatus();
        var result = new 
            { 
                readersInQueue = status.readers,
                writersInQueue = status.writers
            };
        return Ok(result);
    }
}
