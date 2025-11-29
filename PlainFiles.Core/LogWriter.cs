namespace PlainFiles.Core;

public class LogWriter : IDisposable
{
    private readonly StreamWriter _writer;

    public LogWriter(string path)
    {
        _writer = new StreamWriter(path, append: true)
        {
            AutoFlush = true
        };
    }

    public void WriteLog(string level, string message, string username)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _writer.WriteLine($"[{timestamp}] - [{level}] - [User: {username}] - {message}");
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }
}