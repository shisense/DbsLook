using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbsLook.Data
{


    static class Chronometer
    {
        class TimerEntry
        {
            public DateTime DT { get; set; }
            public string Name { get; set; }
        }

        static List<TimerEntry> Records;
        public static void Start(string name)
        {
            Records = new List<TimerEntry>();
            Add(name);
        }


        public static void Add(string name)
        {

            Records.Add(new TimerEntry
            {
                DT = DateTime.Now,
                Name = name
            });
        }

        public static void End()
        {
            Records.Add(new TimerEntry
            {
                DT = DateTime.Now,
                Name = "End"
            });

            TimerEntry prev = null;
            foreach (var r in Records)
            {
                if (prev != null)
                {

                    Console.WriteLine($"[Chronometer] {prev.Name}:{(r.DT - prev.DT).TotalMilliseconds} milliseconds");
                }

                prev = r;
            }
        }


        public static void Run(string name, Action d)
        {
            Start(name);
            var t = Task.Run(() => { d(); });
            t.Wait();
            End();
        }
    }

}