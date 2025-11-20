using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.FileSystem;

public sealed class FileResourceBuilder : IResourceBuilder<FileResourceBuilder>
{
    private FileInfo? _file;

    private string? _name;

    private FlexibleContentType? _type;

    #region Functionality

    public FileResourceBuilder File(FileInfo file)
    {
        _file = file;
        return this;
    }

    public FileResourceBuilder Name(string name)
    {
        _name = name;
        return this;
    }

    public FileResourceBuilder Type(FlexibleContentType contentType)
    {
        _type = contentType;
        return this;
    }

    public FileResourceBuilder Modified(DateTime modified) => throw new NotSupportedException("Modification date of file resources cannot be changed");

    public IResource Build()
    {
        var file = _file ?? throw new BuilderMissingPropertyException("file");

        if (!file.Exists)
        {
            throw new FileNotFoundException("The given file does not exist", file.FullName);
        }

        return new FileResource(file, _name, _type);
    }

    #endregion

}
