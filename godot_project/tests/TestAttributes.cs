using System;

// This file provides test attributes for C# tests
// It allows tests to run without requiring the Microsoft.VisualStudio.TestTools.UnitTesting assembly
namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    // Attribute for marking test classes
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClassAttribute : Attribute
    {
    }

    // Attribute for marking test methods
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : Attribute
    {
    }

    // Static assertion methods for use in tests
    public static class Assert
    {
        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception(message ?? "Assert.IsTrue failed.");
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new Exception(message ?? "Assert.IsFalse failed.");
            }
        }

        public static void AreEqual(object expected, object actual, string message = "")
        {
            if (!object.Equals(expected, actual))
            {
                throw new Exception(message ?? $"Assert.AreEqual failed. Expected:<{expected}>. Actual:<{actual}>.");
            }
        }

        public static void AreNotEqual(object expected, object actual, string message = "")
        {
            if (object.Equals(expected, actual))
            {
                throw new Exception(message ?? $"Assert.AreNotEqual failed. Value:<{actual}>.");
            }
        }

        public static void IsNull(object obj, string message = "")
        {
            if (obj != null)
            {
                throw new Exception(message ?? "Assert.IsNull failed.");
            }
        }

        public static void IsNotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                throw new Exception(message ?? "Assert.IsNotNull failed.");
            }
        }
    }
}
