using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
    public static class ListExtensions
    {
        public static List<List<T>> Split<T>(this IList<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}

namespace BankAccounts
{
    using Extensions;

    // {{## BEGIN account ##}}
    class Account
    {
        static int count = 0;
        public Account(int balance)
        {
            ID = ++count;
            Balance = balance;
        }

        public int ID { get; private set; }
        public int Balance { get; set; }
        public override string ToString()
        {
            return String.Format("Account {0}: {1}", ID, Balance);
        }
    }
    // {{## END account ##}}

    // {{## BEGIN bank ##}}
    class Bank
    {
        public IList<Account> Accounts = new List<Account>();

        public Bank(int numAccounts)
        {
            var amountGenerator = new Random();
            for (int i = 0; i < numAccounts; i++)
                Accounts.Add(new Account(amountGenerator.Next(50000)));
        }
        public override string ToString()
        {
            var result = "Bank Ledger:\n";
            foreach (var acct in Accounts)
                result += String.Format("\tAccount {0}: {1}\n", acct.ID, acct.Balance);
            return result;
        }
    }
    // {{## END bank ##}}


    class Program
    {
        static void Main(string[] args)
        {
            // {{## BEGIN boft ##}}
            var bankOfTed = new Bank(1000000);
                // generate a Bank with a million accounts
            // {{## END boft ##}}

            /*
            // {{## BEGIN serial ##}}
            foreach (var acct in bankOfTed.Accounts)
            {
                Console.WriteLine("Old balance is {0}", acct.Balance);
                acct.Balance += (int)(acct.Balance * 0.05); // 5% interest
                Console.WriteLine("New balance is {0}", acct.Balance);
            }
            // {{## END serial ##}}
            */

            /*
            // {{## BEGIN thread-pooled ##}}
            CountdownEvent latch = new CountdownEvent(bankOfTed.Accounts.Count);
            foreach (var acct in bankOfTed.Accounts)
            {
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    Account account = (Account)obj;
                    Console.WriteLine("Old balance is {0}", acct.Balance);
                    account.Balance += (int)(account.Balance * 0.05); // 5% interest
                    Console.WriteLine("New balance is {0}", acct.Balance);
                    latch.Signal();
                }, acct);
            }
            // {{## END thread-pooled ##}}
            Console.WriteLine("Waiting for accounts to finish calculating...");
            latch.Wait();
            */

            // {{## BEGIN proc-thread-pooled ##}}
            var procs = Environment.ProcessorCount;
            var chunkSize = (bankOfTed.Accounts.Count / procs) + 1;
            var segments = bankOfTed.Accounts.Split(chunkSize);
            CountdownEvent latch = new CountdownEvent(segments.Count);
            foreach (var seg in segments)
            {
                ThreadPool.QueueUserWorkItem(obj =>
                {
                    IEnumerable<Account> segment = (IEnumerable<Account>)obj;
                    foreach (var account in segment)
                    {
                        Console.WriteLine("Old: {0}", account);
                        account.Balance += (int)(account.Balance * 0.05); // 5% interest
                        Console.WriteLine("New: {0}", account);
                    }
                    latch.Signal();
                }, seg);
            }
            latch.Wait();
            // {{## END proc-thread-pooled ##}}
        }
    }
}
