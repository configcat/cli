using ConfigCat.Cli.Services.FileSystem.Ignore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO;

public static class FileSystemExtensions
{
    public static bool IsIgnoreFile(this FileInfo info) => IgnoreFile.IgnoreFileNames.Contains(info.Name);

    public static async Task<bool> IsBinaryAsync(this FileStream stream, CancellationToken token)
    {
        const int readLimit = 8000;

        var readBuffer = new byte[readLimit];
        var readBytes = await stream.ReadAsync(readBuffer, token);

        while (readBytes-- > 0)
        {
            if (readBuffer[readBytes] != char.MinValue) continue;
            stream.Seek(0, SeekOrigin.Begin);
            return true;
        }

        stream.Seek(0, SeekOrigin.Begin);
        return false;
    }
}