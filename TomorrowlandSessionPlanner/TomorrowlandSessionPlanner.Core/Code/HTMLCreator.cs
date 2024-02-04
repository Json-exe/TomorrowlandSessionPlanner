using System.Text;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Code;

public class HtmlCreator
{
    public Task<string> CreateHtmlTable(List<Session> sessions, PlannerManager plannerManager)
    {
        var html = new StringBuilder();
        html.Append("<table>");
        html.Append("<thead>");
        html.Append("<tr>");
        html.Append("<th>Stage</th>");
        html.Append("<th>Artist</th>");
        html.Append("<th>Startzeit</th>");
        html.Append("<th>Endzeit</th>");
        html.Append("</tr>");
        html.Append("</thead>");
        html.Append("<tbody>");
        foreach (var session in sessions)
        {
            var stage = plannerManager.StageList.First(s => s.Id == session.StageId);
            var dj = plannerManager.DjList.First(d => d.Id == session.DjId);
            html.Append("<tr>");
            html.Append($"<td>{stage.Name}</td>");
            html.Append($"<td>{dj.Name}</td>");
            html.Append($"<td>{session.StartTime}</td>");
            html.Append($"<td>{session.EndTime}</td>");
            html.Append("</tr>");
        }
        html.Append("</tbody>");
        html.Append("</table>");
        AddStyle(html);
        return Task.FromResult(html.ToString());
    }

    private static void AddStyle(StringBuilder stringBuilder)
    {
      const string style = """
                           table {
                             border-spacing: 1;
                             border-collapse: collapse;
                             background: rgb(255, 231, 231);
                             border-radius: 6px;
                             overflow: hidden;
                             max-width: 800px;
                             width: 100%;
                             margin: 0 auto;
                             position: relative;
                           }
                           table * {
                             position: relative;
                           }
                           table td,
                           table th {
                             padding-left: 8px;
                           }
                           table thead tr {
                             height: 60px;
                             background: #1cf7ff;
                             font-size: 16px;
                           }
                           table tbody tr {
                             height: 48px;
                             border-bottom: 1px solid #e3f1d5;
                           }
                           table tbody tr:last-child {
                             border: 0;
                           }
                           table td,
                           table th {
                             text-align: left;
                           }
                           table td.l,
                           table th.l {
                             text-align: right;
                           }
                           table td.c,
                           table th.c {
                             text-align: center;
                           }
                           table td.r,
                           table th.r {
                             text-align: center;
                           }

                           @media screen and (max-width: 35.5em) {
                             table {
                               display: block;
                             }
                             table > *,
                           table tr,
                           table td,
                           table th {
                               display: block;
                             }
                             table thead {
                               display: none;
                             }
                             table tbody tr {
                               height: auto;
                               padding: 8px 0;
                             }
                             table tbody tr td {
                               padding-left: 45%;
                               margin-bottom: 12px;
                             }
                             table tbody tr td:last-child {
                               margin-bottom: 0;
                             }
                             table tbody tr td:before {
                               position: absolute;
                               font-weight: 700;
                               width: 40%;
                               left: 10px;
                               top: 0;
                             }
                             table tbody tr td:nth-child(1):before {
                               content: "Code";
                             }
                             table tbody tr td:nth-child(2):before {
                               content: "Stock";
                             }
                             table tbody tr td:nth-child(3):before {
                               content: "Cap";
                             }
                             table tbody tr td:nth-child(4):before {
                               content: "Inch";
                             }
                             table tbody tr td:nth-child(5):before {
                               content: "Box Type";
                             }
                           }
                           body {
                             background: #75d2ee;
                             font: 400 14px "Calibri", "Arial";
                             padding: 20px;
                           }
                           """;
      stringBuilder.Append($"<style>{style}</style>");
    }
}