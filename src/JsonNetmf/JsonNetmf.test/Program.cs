using System;
using Microsoft.SPOT;
using PervasiveDigital.Json;

namespace JsonNetmf.test
{
    public class TestClass
    {
        public int i;
        public UInt32 ui32;
        public string aString;
        public string ignoreme;
        public string someName;
        public DateTime Timestamp;
        public int[] intArray;
        public string[] stringArray;
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
                intArray = new [] { 1, 3, 5, 7, 9 },
                stringArray = new [] { "two", "four", "six", "eight" }
            };
            var result = JsonConverter.Serialize(test);
            Debug.Print("Serialization:");
            var stringValue = result.ToString();
            Debug.Print(stringValue);

            var dserResult = JsonConverter.Deserialize(stringValue);
            Debug.Print("After deserialization:");
            Debug.Print(dserResult.ToString());
        }
    }
}
