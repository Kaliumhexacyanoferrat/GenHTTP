using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.FileSystem;

public sealed class FileResourceBuilder : IResourceBuilder<FileResourceBuilder>
{
    private FileInfo? _File;

    private string? _Name;

    private FlexibleContentType? _Type;

    #region Functionality

    public FileResourceBuilder File(FileInfo file)
    {
        _File = file;
        return this;
    }

    public FileResourceBuilder Name(string name)
    {
        _Name = name;
        return this;
    }

    public FileResourceBuilder Type(FlexibleContentType contentType)
    {
        _Type = contentType;
        return this;
    }

    public FileResourceBuilder Modified(DateTime modified) => throw new NotSupportedException("Modification date of file resources cannot be changed");

    public IResource Build()
    {
        var file = _File ?? throw new BuilderMissingPropertyException("file");

        if (!file.Exists)
        {
            throw new FileNotFoundException("The given file does not exist", file.FullName);
        }

        return new FileResource(file, _Name, _Type);
    }

    #endregion

}
