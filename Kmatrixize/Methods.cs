using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Kmatrixize
{
    internal enum Method
    {
        PointMax,
        EdgeMax,
        Random
    }

    internal static class Methods
    {
        internal static void PointMax(string input, string output, int lowest_n)
        {
            // read orignal ncc file
            // key: fragments, format <chr1:position1-chr2:postion2> (only re1 position is considered)
            // value: string for storing ncc lines for write
            Dictionary<string, List<(string frag1, string frag2, string edge)>> contacts = new();

            var a = DateTime.Now;

            Console.WriteLine("Step 1: reading input file ...");
            //using (StreamReader sr = new StreamReader(input))
            //{
            //    while (!sr.EndOfStream)
            //    {
            //        string ts = sr.ReadLine();
            //        var items = ts.Split(' ');
            //        string frag1 = (items[0].LastIndexOf("chr") > -1 ? items[0].Substring(items[0].LastIndexOf("chr")) : items[0]) + ":" + items[3];
            //        string frag2 = (items[6].LastIndexOf("chr") > -1 ? items[6].Substring(items[6].LastIndexOf("chr")) : items[6]) + ":" + items[9];

            //        //for frag1
            //        if (!contacts.ContainsKey(frag1)) contacts.Add(frag1, new());
            //        contacts[frag1].Add((frag1, frag2, ts));

            //        //for frag2
            //        if (!contacts.ContainsKey(frag2)) contacts.Add(frag2, new());
            //        contacts[frag2].Add((frag1, frag2, ts));
            //    }
            //}


            foreach (var c in File.ReadLines(input).AsParallel().WithDegreeOfParallelism(8).Select(p => pline(p)))
            {
                if (!contacts.ContainsKey(c.frag1)) contacts.Add(c.frag1, new());
                contacts[c.frag1].Add(c);

                if (!contacts.ContainsKey(c.frag2)) contacts.Add(c.frag2, new());
                contacts[c.frag2].Add(c);
            }



            Console.WriteLine($"Time: {(DateTime.Now - a).TotalSeconds}");

            // sorting contacts
            Console.WriteLine("Step 2: sorting, filtering and randomizing points based on contact number ...");
            Random r = new();
            var sorted_points = contacts.AsParallel().WithDegreeOfParallelism(8)
                                        .Where(p => p.Value.Count >= lowest_n)
                                        .Select(p => (p.Key, p.Value, r.NextDouble()))
                                        .OrderByDescending(p => p.Value.Count)
                                        .ThenBy(p => p.Item3).ToList();
            contacts.Clear();
            GC.Collect();
            //process points
            Console.WriteLine("Step 3: processing points and outputing result file ...");
            int n_points = sorted_points.Count;
            int percent = -1;
            HashSet<string> selected_pos = new();

            using (StreamWriter sw = new StreamWriter(output))
            {

                for (int i = 0; i < sorted_points.Count; i++)
                {
                    var point = sorted_points[i];

                    // if point already in selected, skip the point
                    if (selected_pos.Contains(point.Key)) continue;

                    // get edge for the point
                    var edge = getEdge(point.Value, r, selected_pos);

                    // if null, skip the point
                    if (edge.frag1 is null) continue;

                    // if edge is valid, add to selected pos and write edge
                    selected_pos.Add(edge.frag1);
                    selected_pos.Add(edge.frag2);

                    sw.WriteLine(edge.contact);

                    // write progress
                    if (i * 100 / n_points > percent)
                    {
                        percent = i * 100 / n_points;
                        Console.WriteLine($"{percent}% edges processed... {n_points - i} edges left... {DateTime.Now}");
                    }

                }
            }
        }

        private static (string frag1, string frag2, string ts) pline(string ts)
        {
            var items = ts.Split(' ');
            string frag1 = (items[0].LastIndexOf("chr") > -1 ? items[0].Substring(items[0].LastIndexOf("chr")) : items[0]) + ":" + items[3];
            string frag2 = (items[6].LastIndexOf("chr") > -1 ? items[6].Substring(items[6].LastIndexOf("chr")) : items[6]) + ":" + items[9];
            return (frag1, frag2, ts);
        }

        private static (string frag1, string frag2, string contact) getEdge(List<(string frag1, string frag2, string edge)> contact_list, Random r, HashSet<string> selected_pos)
        {
            // remove contacts if within selected pos
            for (int i = contact_list.Count - 1; i > -1 ; i--)
            {
                if (selected_pos.Contains(contact_list[i].frag1) || selected_pos.Contains(contact_list[i].frag2))
                {
                    contact_list.RemoveAt(i);
                }
            }

            if (contact_list.Count > 0)
            {
                var edge = contact_list[r.Next(contact_list.Count)];
                return edge;
            }
            else
            {
                return (null, null, null);
            }
        }

        internal static void EdgeMax(string input, string output, int lowest_n)
        {
            // store edge for each point pair
            Dictionary<string, int> line_count = new();
            Dictionary<string, string> all_edges = new();
            Random r = new();

            Console.WriteLine("Step 1: reading input file ...");
            using (StreamReader sr = new StreamReader(input))
            {
                while (!sr.EndOfStream)
                {
                    var ts = sr.ReadLine();
                    var items = ts.Split(' ');
                    string frag1 = (items[0].LastIndexOf("chr") > -1 ? items[0].Substring(items[0].LastIndexOf("chr")) : items[0]) + ":" + items[3];
                    string frag2 = (items[6].LastIndexOf("chr") > -1 ? items[6].Substring(items[6].LastIndexOf("chr")) : items[6]) + ":" + items[9];

                    string key = "";
                    if (string.Compare(frag1, frag2) < 0)
                    {
                        key = frag1 + "-" + frag2;
                    }
                    else
                    {
                        key = frag1 + "-" + frag2;
                    }

                    if (!line_count.ContainsKey(key))
                    {
                        line_count.Add(key, 0);
                        all_edges.Add(key, ts);
                    }
                    line_count[key]++;
                }

                Console.WriteLine($"Total edges: {line_count.Count}...");
            }

            Console.WriteLine("Step 2: sorting, filtering and randomizing edges based on numbers ...");
            var sorted_edge = line_count.AsParallel().WithDegreeOfParallelism(8)
                                        .Where(p => p.Value >= lowest_n)
                                        .Select(p => (p.Key, p.Value, r.NextDouble()))
                                        .OrderByDescending(p => p.Value)
                                        .ThenBy(p=>p.Item3).ToList();

            Console.WriteLine("Step 3: processing edges and outputing ...");

            HashSet<string> selected = new HashSet<string>();
            int n_edges = sorted_edge.Count;
            int percent = -1;

            using (StreamWriter sw = new StreamWriter(output))
            {
                for (int i = 0; i < sorted_edge.Count; i++)
                {
                    // print percentage
                    if (i * 100 / n_edges > percent)
                    {
                        percent = i * 100 / n_edges;
                        Console.WriteLine($"{percent}% edges processed... {n_edges - i} edges left... {DateTime.Now}");
                    }

                    // select edge with next max number
                    var edge = sorted_edge[i];

                    // check if points exsit
                    var frags = edge.Key.Split("-");
                    if (!selected.Contains(frags[0]) && !selected.Contains(frags[1]))
                    {
                        sw.WriteLine(all_edges[edge.Key]);
                        selected.Add(frags[0]);
                        selected.Add(frags[1]);
                    }
                }
            }
        }

        internal static void Random(string input, string output)
        {
            // read all the contacts in the ncc file
            Console.WriteLine("Step 1: reading input file ...");
            var contacts = File.ReadAllLines(input).ToList();

            Console.WriteLine($"\tTotal contact number: {contacts.Count}...");
            Console.WriteLine("Step 2: Randomizing reads ...");
            Random r = new();
            var rand_contacts = contacts.AsParallel().WithDegreeOfParallelism(8)
                                        .Select(p => (p, r.NextDouble()))
                                        .OrderBy(p => p.Item2).ToList();


            Console.WriteLine("Step 3: Outputing results ...");
            // save selected fragment names
            HashSet<string> included_pos = new();

            // output file
            var sw = new StreamWriter(output);
            
            // init pool size and percentage of remains
            int cSize = rand_contacts.Count;
            int percent = -1;

            // repeatly random select
            for (int i = 0; i < cSize; i++)
            {
                // print processing progress
                if (i * 100 / cSize > percent)
                {
                    percent = i * 100 / cSize;
                    Console.WriteLine(percent + "% contacts processed... " + (cSize - i) + " contacts lefted...    " + DateTime.Now);
                }

                // select a contact
                var ts = rand_contacts[i].p;

                // get contact fragments
                var items = ts.Split(' ');
                string frag1 = (items[0].LastIndexOf("chr") > -1 ? items[0].Substring(items[0].LastIndexOf("chr")) : items[0]) + ":" + items[3];
                string frag2 = (items[6].LastIndexOf("chr") > -1 ? items[6].Substring(items[6].LastIndexOf("chr")) : items[6]) + ":" + items[9];

                // keep the contact if both fragment not selected already
                if (!included_pos.Contains(frag1) && !included_pos.Contains(frag2))
                {
                    sw.WriteLine(ts);
                    included_pos.Add(frag1);
                    included_pos.Add(frag2);
                }
            }
            sw.Close();
        }
    }
}
