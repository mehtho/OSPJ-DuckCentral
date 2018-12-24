using DuckPond.Models;
using InstantMessenger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DuckDebug
{
    class Program
    {
        static int Main(string[] args)
        {
            Events ev = new Events("Debug000", "The debug message", 2 ,"123.123.123.123", "hhhhhhhh", DateTime.Now);
            String toSend = DoSerialize(ev);

            Console.WriteLine(toSend);

            //Send 1 Debug message
            IMClient im = new IMClient();
            im.setConnParams("localhost", 2000);
            im.SetupConn();
            im.SendSignal((byte)7, toSend);
            return 0;
        }

        public static String DoSerialize(Events ev)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(Events));
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, ev);
                    xml = sww.ToString(); // Your XML
                    return xml;
                }
            }
        }
    }
}
