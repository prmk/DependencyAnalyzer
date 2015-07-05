/////////////////////////////////////////////////////////////////////////
// BlockingQueue.cs - A queue for communication                        //
//                                                                     //
//  Language:      C#                                                  //
//  Platform:      Windows 7 Premium                                   //
//  Application:   Dependency Analyzer                                 //
//                                                                     //
// Author: Pramukh Shenoy                                              //
// Original Author: Jim Fawcett                                        //
/////////////////////////////////////////////////////////////////////////

/*
 * Module Operations:
 * ------------------
 * This module implements a Blocking queue
 * Used for interaction between the server and client
 * 
 * 
 * Public Interface
 * ------------------
 * BlockingQueue Q = new BlockingQueue();
 * Q.enQ("msg");
 * Q.deQ();
 * Q.size();
 * Q.clear();
 * analyzer.analyze(8080)
 * analyzer.getDefaultElemData("debug", "8080");
 * analyzer.analyzePartII
 * 
 * 
 * Compiler Command:
 * ----------------
 *   csc /target:exe BlockingQueue.cs
 * 
 *   
 * 
 */

using System;
using System.Collections;
using System.Threading;

namespace DependencyAnalyzer
{
    public class BlockingQueue<T>
    {
        private Queue blockingQ;
        ManualResetEvent ev;

        //----< constructor >--------------------------------------------

        public BlockingQueue()
        {
            Queue Q = new Queue();
            blockingQ = Q;
            ev = new ManualResetEvent(false);
        }
        //----< enqueue a string >---------------------------------------

        public void enQ(T msg)
        {
            lock (blockingQ)
            {
                blockingQ.Enqueue(msg);
                ev.Set();
            }
        }
        //
        //----< dequeue a T >---------------------------------------
        //
        //  This looks more complicated than you might think it needs
        //  to be; however without the second count check:
        //    If a single item is in the queue and a thread
        //    moves toward the deQ but finishes its time allocation
        //    before deQ'ing another thread may get throught the locks
        //    and deQ.  Then the first thread wakes up and since its
        //    waitFlag is false, attempts to deQ the empty queue.
        //  This is the reason for the second count check.

        public T deQ()
        {
            T msg = default(T);
            while (true)
            {
                if (this.size() == 0)
                {
                    ev.Reset();
                    ev.WaitOne();
                }
                lock (blockingQ)
                {
                    if (blockingQ.Count != 0)
                    {
                        msg = (T)blockingQ.Dequeue();
                        break;
                    }
                }
            }
            return msg;
        }
        //----< return number of elements in queue >---------------------

        public int size()
        {
            int count;
            lock (blockingQ) { count = blockingQ.Count; }
            return count;
        }
        //----< purge elements from queue >------------------------------

        public void clear()
        {
            lock (blockingQ) { blockingQ.Clear(); }
        }
    }

    public class test
    {
        public static void Main(string[] args)
        {
            BlockingQueue<string> Q = new BlockingQueue<string>();
            Q.enQ("test msg");
            string msg = Q.deQ();
            Console.WriteLine("Message: {0}", msg);

        }
    }
}
