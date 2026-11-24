using System.Collections.Generic;
using System.Text;

namespace Modules.LocalizationModule.Remote
{
    /// <summary>
    /// The CSV a published sheet tab answers with, read the way RFC 4180 writes it: a quoted cell
    /// may hold a comma, a line break and a doubled quote, rows end in CRLF or LF, and a leading
    /// byte order mark is dropped. A module of its own because the module depends on nothing.
    /// </summary>
    internal class RemoteCsv
    {
        public List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();

            if (string.IsNullOrEmpty(text))
                return rows;

            int start = text[0] == '﻿' ? 1 : 0;
            var row = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            bool any = false;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                any = true;

                if (quoted)
                {
                    if (c != '"')
                    {
                        cell.Append(c);
                        continue;
                    }

                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                        continue;
                    }

                    quoted = false;
                    continue;
                }

                switch (c)
                {
                    case '"':
                        quoted = true;
                        break;

                    case ',':
                        row.Add(cell.ToString());
                        cell.Clear();
                        break;

                    case '\r':
                        break;

                    case '\n':
                        row.Add(cell.ToString());
                        cell.Clear();
                        rows.Add(row);
                        row = new List<string>();
                        any = false;
                        break;

                    default:
                        cell.Append(c);
                        break;
                }
            }

            // A last row with no line break after it.
            if (any || cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                rows.Add(row);
            }

            return rows;
        }
    }
}
