using Godot;

// This file provides stub implementations of GUT classes for C# tests
// It allows tests written for GUT to run without the actual GUT framework
// Updated for cross-platform compatibility (Windows/Mac)
namespace GUT
{
    /// <summary>
    /// Base class for tests, similar to GUT's Test class
    /// </summary>
    [GlobalClass]
    public partial class Test : Node
    {
#if !DUPLICATE_METHODS_DEFINED
        // Setup method called before each test
        public virtual void Before()
        {
            // Default implementation does nothing
        }

        // Teardown method called after each test
        public virtual void After()
        {
            // Default implementation does nothing
        }

        // Assertion methods
        public void Assert(bool condition)
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(condition);
        }

        public void AssertTrue(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(condition, message);
        }

        public void AssertFalse(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(condition, message);
        }

        public void AssertEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, actual, message);
        }

        public void AssertNotEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(expected, actual, message);
        }

        public void AssertNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(obj, message);
        }

        public void AssertNotNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(obj, message);
        }

        public void AssertGreater(IComparable actual, IComparable expected, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(actual.CompareTo(expected) > 0, message);
        }

        public void AssertLess(IComparable actual, IComparable expected, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(actual.CompareTo(expected) < 0, message);
        }

        public void Pass(string message = "")
        {
            // Pass is always successful
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true, message);
        }
#endif
    }

#if !DUPLICATE_ATTRIBUTES_DEFINED
    // Attribute for marking test classes
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClassAttribute : Attribute
    {
    }

    // Attribute for marking test methods
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
    }

}

// Static assertion methods for use in non-Test classes
public static class Assert
    {
        public static void IsTrue(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(condition, message);
        }

        public static void IsFalse(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(condition, message);
        }

        public static void AreEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, actual, message);
        }

        public static void AreNotEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(expected, actual, message);
        }

        public static void IsNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(obj, message);
        }

        public static void IsNotNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(obj, message);
        }

        public static void AssertGreater(IComparable actual, IComparable expected, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(actual.CompareTo(expected) > 0, message);
        }

        public static void AssertLess(IComparable actual, IComparable expected, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(actual.CompareTo(expected) < 0, message);
        }

        // Aliases for the test files
        public static void AssertEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(expected, actual, message);
        }

        public static void AssertNotEqual(object expected, object actual, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreNotEqual(expected, actual, message);
        }

        public static void AssertTrue(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(condition, message);
        }

        public static void AssertFalse(bool condition, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(condition, message);
        }

        public static void AssertNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(obj, message);
        }

        public static void AssertNotNull(object obj, string message = "")
        {
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(obj, message);
        }

        public static void Pass(string message = "")
        {
            // Pass is always successful
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(true, message);
        }
    }
#endif
}
