using System.Linq;
using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Api.Modules;

using GenHTTP.Modules.Webservices;

namespace GenHTTP.Examples.Examples.Webservices
{

    public class BookResource
    {

        #region Get-/Setters

        private Dictionary<int, Book> Books { get; }

        #endregion

        #region Initialization

        public BookResource()
        {
            Books = new Dictionary<int, Book>
            {
                { 1, new Book(1, "Journey to the Center of the Earth") },
                { 2, new Book(2, "Moby Dick") }
            };
        }

        #endregion

        #region Functionality

        [Method]
        public List<Book> GetList(int page = 0, int pageSize = 20)
        {
            return Books.Values.ToList();
        }

        [Method(":id")]
        public Book? GetBook(int id)
        {
            return FindBook(id);
        }

        [Method(RequestMethod.PUT)]
        public Book AddBook(Book book)
        {
            var copy = new Book(Books.Keys.Max() + 1, book.Title);
            Books.Add(copy.ID, copy);

            return copy;
        }

        [Method(RequestMethod.POST)]
        public Book UpdateBook(Book book)
        {
            Books[FindBook(book.ID).ID] = book;
            return book;
        }

        [Method(RequestMethod.DELETE, ":id")]
        public Book DeleteBook(int id)
        {
            var existing = FindBook(id);
            Books.Remove(existing.ID);

            return existing;
        }

        private Book FindBook(int id)
        {
            if (Books.TryGetValue(id, out var existing))
            {
                return existing;
            }

            throw new ProviderException(ResponseStatus.NotFound, $"Book with ID {id} does not exist");
        }

        #endregion

    }

}
