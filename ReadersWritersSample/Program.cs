using ReadersWritersSample.Repository.Services;
using ReadersWritersSample.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IReaderWriterQueue, ReaderWriterQueue>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
