using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ragnarok
{
    public class NameTable
    {
        private static readonly Encoding s_KoreanEncoding = Encoding.GetEncoding(949);

        private string m_Filename;
        private Dictionary<int, string> m_Items;

        public string this[int id]
        {
            get { return m_Items[id]; }
        }

        public NameTable(string filename)
        {
            m_Filename = filename;
            m_Items = new Dictionary<int, string>();
            string[] lines = new string[0];
            {
                byte[] b = File.ReadAllBytes(filename);
                lines = s_KoreanEncoding.GetString(b).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }

            int j = 0, k;
            string line, id = null;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                line = lines[i].Trim();

                if (line.StartsWith("//"))
                    continue;

                if (id == null)
                {
                    j = line.IndexOf('#');
                    id = line.Substring(0, j);
                    k = line.IndexOf('#', ++j);
                }
                else
                {
                    k = line.IndexOf('#');
                }

                if (k >= 0)
                {
                    sb.Append(line.Substring(j, k - j));
                    m_Items.Add(Convert.ToInt32(id), sb.ToString());
                    sb.Remove(0, sb.Length);
                    id = null;
                    j = 0;
                }
                else
                {
                    sb.AppendLine(line.Substring(j));
                    j = 0;
                }
            }
        }

        public void Duplicate(int id, int newId)
        {
            if (!m_Items.ContainsKey(id))
                throw new ArgumentOutOfRangeException("index");

            if (m_Items.ContainsKey(newId))
                m_Items[newId] = m_Items[id];
            else
                m_Items.Add(newId, m_Items[id]);
        }

        public void Add(int id, string value)
        {
            if (m_Items.ContainsKey(id))
                m_Items[id] = value;
            else
                m_Items.Add(id, value);
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<int, string> kvp in m_Items)
                sb.AppendFormat("{0}#{1}#\r\n", kvp.Key, kvp.Value);

            File.WriteAllText(m_Filename + "1", sb.ToString(), s_KoreanEncoding);
        }
    }
}
