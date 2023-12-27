using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleInvoke
{
    class Program
    {
        // {{## BEGIN threaded-message ##}}
        static void ThreadedMessage(string msg, params object[] args)
        {
            Console.WriteLine("{0}({1}): {2}", 
                Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId, 
                String.Format(msg, args));
        }
        // {{## END threaded-message ##}}

        static void SimpleInvoke()
        {
            // {{## BEGIN simple-invoke ##}}
            Parallel.Invoke(
                () => ThreadedMessage("Hello, from a Task"),
                () => ThreadedMessage("Hello, from another task")
            );
            // {{## END simple-invoke ##}}
        }

        static void ExplicitTask()
        {
            // {{## BEGIN explicit-task ##}}
            Task t = new Task(
                () => ThreadedMessage("Hello")
            );
            t.Start();

            Task t2 = Task.Run(
                () => ThreadedMessage("Hello")
            );

            Task t3 = Task.Factory.StartNew(
                () => ThreadedMessage("Hello")
            );

            ThreadedMessage("Hello");
            t.Wait(); t2.Wait(); t3.Wait();
                // or Task.WaitAll(new Task[] { t, t2, t3 });
            // {{## END explicit-task ##}}
        }

        static void TaskResults()
        {
            // {{## BEGIN task-result ##}}
            var t = Task.Run(() =>
            {
                var random = new Random().NextDouble();
                var ticks = new DateTime().Ticks;
                var result = ticks * random;
                ThreadedMessage("Calculated {0}", result);
                return result;
            });
            Console.WriteLine("completed = {0}", t.IsCompleted);
            var res = t.Result; 
                // this will block until t is completed
            Console.WriteLine("res = {0}, completed = {1}", res, t.IsCompleted);
            // {{## END task-result ##}}
        }

        static void ExplicitTaskWithData()
        {
            /*
            // {{## BEGIN explicit-task-with-data-error ##}}
            string[] messages = { "One", "Two", "Three" };
            List<Task> tasks = new List<Task>();
            for (int i=0; i<messages.Length; i++)
            {
                var t = Task.Run(
                    () =>
                    {
                        ThreadedMessage("We are #{0} in line and our message is {0}",
                            i, messages[i]);
                    }
                );
                tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
            // {{## END explicit-task-with-data-error ##}}
            */

            // {{## BEGIN explicit-task-with-data ##}}
            string[] messages = { "One", "Two", "Three" };
            Task[] tasks = new Task[3];
            for (int i=0; i<messages.Length; i++)
            {
                var t = Task.Factory.StartNew(
                    (Object obj) =>
                    {
                        Tuple<int, String> data = obj as Tuple<int, String>;
                        ThreadedMessage("We are #{0} in line and our message is {1}",
                            data.Item1, data.Item2);
                    },
                    Tuple.Create(i, messages[i])
                );
                tasks[i] = t;
            }
            Task.WaitAll(tasks);
            // {{## END explicit-task-with-data ##}}
        }

        class PlayerCharacter
        {
            public int Strength { get; set; }
            public int Intelligence { get; set; }
            public int Wisdom { get; set; }
            public int Dexterity { get; set; }
            public int Constitution { get; set; }
            public int Charisma { get; set; }
        }
        static void Continuations()
        {
            // {{## BEGIN continuations ##}}
            var generatePC = Task.Factory.StartNew( () => {
                Random rnd = new Random();
                int[] dieRolls = new int[18];
                for (int i = 0; i < dieRolls.Length; i++)
                    dieRolls[i] = rnd.Next(1, 7);
                return dieRolls;
            }).ContinueWith( (task) =>
            {
                int[] dieRolls = task.Result;
                var pc = new PlayerCharacter();
                pc.Strength = dieRolls[0] + dieRolls[1] + dieRolls[2];
                pc.Intelligence = dieRolls[3] + dieRolls[4] + dieRolls[5];
                pc.Wisdom = dieRolls[6] + dieRolls[7] + dieRolls[8];
                pc.Dexterity = dieRolls[9] + dieRolls[10] + dieRolls[11];
                pc.Constitution = dieRolls[12] + dieRolls[13] + dieRolls[14];
                pc.Charisma = dieRolls[15] + dieRolls[16] + dieRolls[17];
                return pc;
            });
            // {{## END continuations ##}}
            var player = generatePC.Result;
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            SimpleInvoke();
            ExplicitTask();
            TaskResults();
            ExplicitTaskWithData();
            Continuations();
        }
    }
}
