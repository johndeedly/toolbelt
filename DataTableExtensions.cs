using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.VisualBasic.FileIO;

namespace toolbelt
{
    public static class DataTableExtensions
    {
        public static DataTable FromJson(this DataTable dt, JsonElement elem, string prefix = null)
        {
            InsertDataTableInternal(dt, null, elem, prefix, null);
            return dt;
        }

        public static DataTable ToJson(this DataTable dt, out JsonDocument json)
        {
            var outputBuffer = new ArrayBufferWriter<byte>();
            using (var jsonWriter = new Utf8JsonWriter(outputBuffer))
            {
                jsonWriter.WriteStartArray();

                foreach (DataRow dr in dt.Rows)
                {
                    jsonWriter.WriteStartObject();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        object val = dr[dc];
                        if (val != DBNull.Value && val != null)
                        {
                            if (val is string)
                            {
                                jsonWriter.WriteString(dc.ColumnName, (string)val);
                            }
                            if (val is bool)
                            {
                                jsonWriter.WriteBoolean(dc.ColumnName, (bool)val);
                            }
                            if (val is sbyte
                                || val is byte
                                || val is short
                                || val is ushort
                                || val is int
                                || val is uint
                                || val is long
                                || val is ulong
                                || val is float
                                || val is double
                                || val is decimal)
                            {
                                jsonWriter.WriteNumber(dc.ColumnName, (decimal)val);
                            }
                            if (val is byte[])
                            {
                                jsonWriter.WriteBase64String(dc.ColumnName, (byte[])val);
                            }
                        }
                    }
                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndArray();
            }
            json = JsonDocument.Parse(Encoding.UTF8.GetString(outputBuffer.WrittenSpan));
            return dt;
        }

        private static void InsertDataTableInternal(DataTable dt, DataRow dr, JsonElement elem, string prefix, string column)
        {
            if (elem.ValueKind == JsonValueKind.Array)
            {
                bool firstElem = true;
                foreach (var item in elem.EnumerateArray())
                {
                    if (column == null)
                    {
                        dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        InsertDataTableInternal(dt, dr, item, prefix, column);
                    }
                    else
                    {
                        if (!firstElem)
                        {
                            dr = dt.NewRow();
                            dt.Rows.Add(dr);
                        }
                        firstElem = false;
                        string fullName = string.Concat(prefix, column, "[]");
                        InsertDataTableInternal(dt, dr, item, fullName, null);
                    }
                }
            }
            else if (elem.ValueKind == JsonValueKind.Object)
            {
                if (dr == null)
                {
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }
                foreach (var prop in elem.EnumerateObject())
                {
                    InsertDataTableInternal(dt, dr, prop.Value, prefix, $"{column}.{prop.Name}");
                }
            }
            else if (elem.ValueKind != JsonValueKind.Null && elem.ValueKind != JsonValueKind.Undefined)
            {
                string fullName = string.Concat(prefix, column);
                DataColumn dc = null;
                if (dt.Columns.Contains(fullName))
                    dc = dt.Columns[fullName];
                if (elem.ValueKind == JsonValueKind.False || elem.ValueKind == JsonValueKind.True)
                {
                    if (dc == null)
                        dc = dt.Columns.Add(fullName, typeof(bool));
                    dr[dc] = elem.GetBoolean();
                }
                if (elem.ValueKind == JsonValueKind.Number)
                {
                    if (dc == null)
                        dc = dt.Columns.Add(fullName, typeof(decimal));
                    dr[dc] = elem.GetDecimal();
                }
                if (elem.ValueKind == JsonValueKind.String)
                {
                    if (dc == null)
                        dc = dt.Columns.Add(fullName, typeof(string));
                    dr[dc] = elem.GetString();
                }
            }
        }

        public static DataTable FromCsv(this DataTable dt, Stream s, bool headerLine = false)
        {
            using (TextFieldParser parser = new TextFieldParser(s, new UTF8Encoding(false), false, true))
            {
                parser.Delimiters = new[] { "," };
                parser.CommentTokens = new[] { "#" };
                parser.HasFieldsEnclosedInQuotes = true;
                parser.TrimWhiteSpace = true;

                // find first line with content
                string[] fields;
                while ((fields = parser.ReadFields()) != null)
                {
                    if (fields.Length != 0)
                        break;
                }
                // empty stream
                if (fields == null)
                    return dt;

                // parse header line
                if (headerLine)
                {
                    List<DataColumn> header = new List<DataColumn>();
                    foreach (var field in fields)
                    {
                        DataColumn dc = dt.Columns
                            .OfType<DataColumn>()
                            .FirstOrDefault(x => x.ColumnName == field);
                        if (dc == null)
                        {
                            dc = new DataColumn(field);
                            dt.Columns.Add(dc);
                        }
                        header.Add(dc);
                    }
                }

                // parse content
                while ((fields = parser.ReadFields()) != null)
                {
                    // skip empty lines
                    if (fields.Length == 0)
                        continue;

                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    for (int i = 0; i < fields.Length && i < dt.Columns.Count; i++)
                    {
                        DataColumn dc = dt.Columns[i];
                        decimal tmp;
                        if (dc.DataType == typeof(sbyte)
                            || dc.DataType == typeof(byte)
                            || dc.DataType == typeof(short)
                            || dc.DataType == typeof(ushort)
                            || dc.DataType == typeof(int)
                            || dc.DataType == typeof(uint)
                            || dc.DataType == typeof(long)
                            || dc.DataType == typeof(ulong)
                            || dc.DataType == typeof(float)
                            || dc.DataType == typeof(double)
                            || dc.DataType == typeof(decimal))
                        {
                            if (decimal.TryParse(fields[i], out tmp))
                                dr[dc] = tmp;
                        }
                        else if (dc.DataType == typeof(string))
                        {
                            dr[dc] = fields[i];
                        }
                    }
                }
            }
            return dt;
        }

        public static DataTable ToCsv(this DataTable dt, Stream s)
        {
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0)
            {
                byte[] data = Encoding.UTF8.GetBytes(string.Join(",", dt.Columns.OfType<DataColumn>().Select(x => x.ColumnName)));
                s.Write(data, 0, data.Length);
                s.WriteByte(10);

                foreach (DataRow dr in dt.Rows)
                {
                    data = Encoding.UTF8.GetBytes(string.Join(",", dt.Columns.OfType<DataColumn>().Select(x => GetValue(dr, x))));
                    s.Write(data, 0, data.Length);
                    s.WriteByte(10);
                }
            }
            return dt;
        }

        public static DataTable ToCsvTransposed(this DataTable dt, Stream s)
        {
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Count > 0)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    byte[] data = Encoding.UTF8.GetBytes($"{dc.ColumnName},");
                    s.Write(data, 0, data.Length);
                    data = Encoding.UTF8.GetBytes(string.Join(",", dt.Rows.OfType<DataRow>().Select(x => GetValue(x, dc))));
                    s.Write(data, 0, data.Length);
                    s.WriteByte(10);
                }
            }
            return dt;
        }

        private static string quote = "\"";
        private static string doublequote = "\"\"";

        private static string GetValue(DataRow dr, DataColumn dc)
        {
            string value = dr[dc].ToString();
            if (value.Any(x => x == ',' || x == '"' || x == '\n'))
                value = string.Concat(quote, value.Replace(quote, doublequote), quote);
            return value;
        }
    }
}
