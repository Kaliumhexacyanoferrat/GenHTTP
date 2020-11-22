using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public class ListingRenderer : IRenderer<ListingModel>
    {

        public ValueTask<string> RenderAsync(ListingModel model)
        {
            var content = new StringBuilder();

            content.AppendLine("<table>");

            content.AppendLine("<thead>");
            content.AppendLine("  <tr>");
            content.AppendLine("    <th>Name</th>");
            content.AppendLine("    <th>Size</th>");
            content.AppendLine("    <th>Last Modified</th>");
            content.AppendLine("  </tr>");
            content.AppendLine("</thead>");

            if (model.HasParent)
            {
                Append(content, "../", "..", null, null);
            }

            foreach (var dir in model.Container.GetNodes().OrderBy(d => d.Name))
            {
                Append(content, $"./{HttpUtility.UrlPathEncode(dir.Name)}/", dir.Name, null, null);
            }

            foreach (var file in model.Container.GetResources().Where(f => f.Name is not null).OrderBy(f => f.Name))
            {
                Append(content, $"./{HttpUtility.UrlPathEncode(file.Name)}", file.Name!, file.Length, file.Modified);
            }

            content.AppendLine("</table>");

            return new ValueTask<string>(content.ToString());
        }

        private static void Append(StringBuilder builder, string path, string name, ulong? size, DateTime? modified)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"  <td><a href=\"{path}\">{name}</a></td>");
            builder.AppendLine($"  <td>{FileSizeFormatter.Format(size)}</td>");

            if (modified is not null)
            {
                builder.AppendLine($"  <td>{modified}</td>");
            }
            else
            {
                builder.AppendLine("  <td>-</td>");
            }

            builder.AppendLine("</tr>");
        }

    }


}
