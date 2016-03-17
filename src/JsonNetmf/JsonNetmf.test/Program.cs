using System;
using Microsoft.SPOT;
using PervasiveDigital.Json;

namespace JsonNetmf.test
{

    public class ChildClass
    {
        public int one;
        public int two;
        public int three;
    }

    public class TestClass
    {
        public int i;
        public string aString;
        public string someName;
        public DateTime Timestamp;
        public int[] intArray;
        public string[] stringArray;
        public ChildClass child1;
        public ChildClass Child { get; set; }
    }
    public class Program
    {
        public static void Main()
        {
            var test = new TestClass()
            {
                aString = "A string",
                i = 10,
                someName = "who?",
                Timestamp = DateTime.UtcNow,
                intArray = new [] { 1, 3, 5, 7, 9 },
                stringArray = new [] { "two", "four", "six", "eight" },
                child1 = new ChildClass() { one=1, two = 2, three = 3 },
                Child = new ChildClass() {  one = 100, two = 200, three = 300 }
            };
            var result = JsonConverter.Serialize(test);
            Debug.Print("Serialization:");
            var stringValue = result.ToString();
            Debug.Print(stringValue);

            var dserResult = JsonConverter.Deserialize(stringValue);
            Debug.Print("After deserialization:");
            Debug.Print(dserResult.ToString());

            var newInstance = (TestClass)JsonConverter.DeserializeObject(stringValue, typeof (TestClass), CreateInstance);
            if (test.i!=newInstance.i ||
                test.Timestamp.ToString() != newInstance.Timestamp.ToString() ||
                test.aString != newInstance.aString ||
                test.someName != newInstance.someName ||
                !ArraysAreEqual(test.intArray, newInstance.intArray) ||
                !ArraysAreEqual(test.stringArray, newInstance.stringArray)
                )
                throw new Exception("round-tripping failed");
        }

        private static object CreateInstance(string path, string name, int length)
        {
            if (name == "intArray")
                return new int[length];
            else if (name == "stringArray")
                return new string[length];
            else
                return null;
        }

        private static bool ArraysAreEqual(Array a1, Array a2)
        {
            if (a1 == null && a2 == null)
                return true;
            if (a1 == null || a2 == null)
                return false;
            if (a1.Length != a2.Length)
                return false;
            for (int i = 0; i < a1.Length; ++i)
            {
                if (!a1.GetValue(i).Equals(a2.GetValue(i)))
                    return false;
            }
            return true;
        }

    }
}
