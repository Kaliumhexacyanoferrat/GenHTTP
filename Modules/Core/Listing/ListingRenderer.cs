using System;
using System.Web;
using System.Linq;
using System.Text;

using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Core.Listing
{

    public class ListingRenderer : IRenderer<ListingModel>
    {

        public string Render(ListingModel model)
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

            foreach (var dir in model.Directories.OrderBy(d => d.Name))
            {
                Append(content, $"./{HttpUtility.UrlPathEncode(dir.Name)}/", dir.Name, null, dir.LastWriteTime);
            }

            foreach (var file in model.Files.OrderBy(f => f.Name))
            {
                Append(content, $"./{HttpUtility.UrlPathEncode(file.Name)}", file.Name, file.Length, file.LastWriteTime);
            }

            content.AppendLine("</table>");

            return content.ToString();
        }

        private void Append(StringBuilder builder, string path, string name, long? size, DateTime? modified)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"  <td><a href=\"{path}\">{name}</a></td>");
            builder.AppendLine($"  <td>{FileSizeFormatter.Format(size)}</td>");

            if (modified != null)
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
