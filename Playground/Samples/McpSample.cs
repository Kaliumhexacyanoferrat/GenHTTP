using System.ComponentModel.DataAnnotations;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Mcp;

namespace GenHTTP.Playground.Samples;

public static class McpSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shows how to provide tools to AI agents such as Claude via the
         * Model Context Protocol (MCP).
         *
         * Add the endpoint to Claude Code via
         *
         *   claude mcp add --transport http library http://localhost:8080/mcp
         *
         * and ask, for example, "Which books about Rome are in the library?".
         *
         * See https://genhttp.org/documentation/content/frameworks/mcp/
         *
         */

        var tools = Tools.Create()
                         .Server("library", "1.0.0", "Library")
                         .Instructions("Search for books before lending them to a reader.")
                         .Add<LibraryTools>()
                         .Add("get_time", "Returns the current time of the library (UTC)", [Tool(ReadOnly = true)] () => DateTime.UtcNow);

        return Layout.Create()
                     .Add("mcp", tools);
    }

    public record Book(int Id, string Title, string? LentTo);

    public class LibraryTools
    {
        private static readonly List<Book> Books =
        [
            new(1, "SPQR: A History of Ancient Rome", null),
            new(2, "The Lord of the Rings", null)
        ];

        [Tool(ReadOnly = true)]
        [Summary("Searches the books of the library by title")]
        public List<Book> FindBooks([Summary("Part of the title, e.g. 'Rome'")] string query, [Range(1, 50)] int limit = 10)
            => Books.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase)).Take(limit).ToList();

        [Tool(Destructive = false)]
        [Summary("Lends a book to a reader")]
        public Book LendBook([Summary("The id of the book, as returned by find_books")] int bookId, [Summary("The name of the reader")] string reader)
        {
            var book = Books.FirstOrDefault(b => b.Id == bookId)
                       ?? throw new ProviderException(ResponseStatus.NotFound, $"There is no book with id {bookId}.");

            if (book.LentTo != null)
            {
                throw new ProviderException(ResponseStatus.Conflict, $"'{book.Title}' is already lent to {book.LentTo}.");
            }

            var lent = book with { LentTo = reader };

            Books[Books.IndexOf(book)] = lent;

            return lent;
        }

    }

}
