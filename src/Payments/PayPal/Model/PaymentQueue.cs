using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.PayPal.Model
{
    /// <summary>
    /// Implements a simple First in First out PaymentQueue.
    /// </summary>
    public class PaymentQueue
    {
        private uint count = 0;
        private Node front = null;
        private Node end   = null;
        private Node temp  = null;

        /// 
        /// Test to see if we don't have any payments to make..
        /// 
        public bool empty
        {
            get
            {
                return (count == 0);
            }
        }

        /// 
        /// Number of PaymentRecords in the PaymentQueue.
        /// 
        public uint Count
        {
            get
            {
                return count;
            }
        }
        /// 
        /// This function will append to the end of the PaymentQueue or 
        /// create the first Node instance.
        /// 
        ///  
        public void append(PaymentRecord obj)
        {
            if (count == 0)
            {
                front = end = new Node(obj, front);
            }
            else
            {
                end.Next = new Node(obj, end.Next);
                end = end.Next;
            }
            count++;
        }
        /// 
        /// This function will serve from the front of the PaymentQueue.  Notice
        /// no deallocation for the Node Class, This is now handled by the 
        /// Garbage Collector.
        /// 
        public object serve()
        {
            temp = front;
            if (count == 0)
                throw new Exception("tried to serve from an empty PaymentQueue");
            front = front.Next;
            count--;
            return temp.Value;
        }
        /// 
        /// This function will print everything that is currently in the 
        /// PaymentQueue. 
        /// 
        public void printQueue()
        {
            temp = front;
            while (temp != null)
            {
                Console.WriteLine("{0}", temp.Value.ToString());
                temp = temp.Next;
            }
        }
    }

    /// 
    /// The Node class holds a PaymentRecord object and a pointer to the next
    /// Node class.
    /// 
    class Node
    {
        public Node Next;
        public PaymentRecord Value;

        public Node(PaymentRecord value, Node next)
        {
            Next = next;
            Value = value;
        }
    }
}
