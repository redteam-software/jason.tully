using System.Threading.Channels;

namespace RedTeamGoCli;

[RegisterSingleton]
public class FileChangeChannel
{
    public ChannelReader<FileChange> Reader { get; init; }
    public ChannelWriter<FileChange> Writer { get; init; }

    public FileChangeChannel()
    {
        var channel = Channel.CreateUnbounded<FileChange>();

        Reader = channel.Reader;
        Writer = channel.Writer;
    }
}