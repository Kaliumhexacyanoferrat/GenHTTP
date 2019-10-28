using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules.Templating;

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
            content.AppendLine("    <td>Name</td>");
            content.AppendLine("    <td>Size</td>");
            content.AppendLine("    <td>Last Modified</td>");
            content.AppendLine("  </tr>");
            content.AppendLine("</thead>");

            if (model.HasParent)
            {
                Append(content, "..", null, null);
            }

            foreach (var dir in model.Directories)
            {
                Append(content, dir.Name, null, dir.LastWriteTime);
            }

            foreach (var file in model.Files)
            {
                Append(content, file.Name, file.Length, file.LastWriteTime);
            }

            content.AppendLine("</table>");

            return content.ToString();
        }

        private void Append(StringBuilder builder, string name, long? size, DateTime? modified)
        {
            builder.AppendLine("<tr>");
            builder.AppendLine($"  <td><a href=\"{name}/\">{name}</a></td>");

            if (size != null)
            {
                builder.AppendLine($"  <td>{size}</td>");

            }
            else
            {
                builder.AppendLine("  <td>-</td>");
            }

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
