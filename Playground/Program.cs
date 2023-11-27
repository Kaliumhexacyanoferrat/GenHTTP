using System.Collections.Generic;

using GenHTTP.Engine;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;

var books = new List<Book>()
{
  new Book(1, "Lord of the Rings")
};

var bookApi = Inline.Create()
                    .Get(() => books) // GET http://localhost:8080/books/
                    .Put((Book book) => books.Add(book)); // PUT http://localhost:8080/books/

var content = Layout.Create()
                    .Add("books", bookApi)
                    .Add(CorsPolicy.Permissive());

return Host.Create()
           .Handler(content)
           .Defaults()
           .Console()
//-:cnd:noEmit
#if DEBUG
           .Development()
#endif
//+:cnd:noEmit
           .Run();

record class Book(int ID, string Title);