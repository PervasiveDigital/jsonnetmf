using System;
using Microsoft.SPOT;
using PervasiveDigital.Json;

namespace JsonNetmf.test
{
	public class NestedClass
	{
		public string name;
		public string addr;
	}
    public class TestClass
    {
        public int i;
        public UInt32 ui32;
        public string aString;
        public string ignoreme;
        public string someName;
        public DateTime Timestamp;
	    public NestedClass child;
    }

    public class Program
    {
        public static void Main()
        {
            var test = new TestClass()
            {
                aString = "A string",
                i = 10,
                ignoreme = "who me?",
                someName = "who?",
                Timestamp = DateTime.UtcNow,
				child = new NestedClass()
				{
					name = "John Smith",
					addr = "123 Main St."	
				}
            };
            var result = JsonConverter.Serialize(test);
            Debug.Print("Serialization:");
            Debug.Print(result.ToString());

			// bson tests
	        var bson = result.ToBson();
	        var compare = JsonConverter.FromBson(bson, typeof(TestClass));
        }
    }
}
