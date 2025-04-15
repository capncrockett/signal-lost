using Godot;
using System;

namespace GUT
{
    /// <summary>
    /// Base class for all C# tests in the Signal Lost project.
    /// Provides common functionality for test setup, teardown, and assertions.
    /// </summary>
    [GlobalClass]
    public partial class Test : Node
    {
        /// <summary>
        /// Called before each test method is executed.
        /// Override this method to set up test dependencies.
        /// </summary>
        public virtual void Before()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Called after each test method is executed.
        /// Override this method to clean up resources.
        /// </summary>
        public virtual void After()
        {
            // Base implementation does nothing
        }

        #region Assertion Methods

        /// <summary>
        /// Asserts that two values are equal.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <param name="expected">The expected value.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertEqual(object actual, object expected, string message = null)
        {
            if (!Equals(actual, expected))
            {
                string errorMessage = message ?? $"Expected {expected}, but got {actual}";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Asserts that two values are not equal.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <param name="expected">The value that actual should not equal.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertNotEqual(object actual, object expected, string message = null)
        {
            if (Equals(actual, expected))
            {
                string errorMessage = message ?? $"Expected {actual} to not equal {expected}";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is true.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                string errorMessage = message ?? "Expected condition to be true, but it was false";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Asserts that a condition is false.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertFalse(bool condition, string message = null)
        {
            if (condition)
            {
                string errorMessage = message ?? "Expected condition to be false, but it was true";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Asserts that a value is null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertNull(object value, string message = null)
        {
            if (value != null)
            {
                string errorMessage = message ?? $"Expected null, but got {value}";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Asserts that a value is not null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="message">Optional message to display on failure.</param>
        public void AssertNotNull(object value, string message = null)
        {
            if (value == null)
            {
                string errorMessage = message ?? "Expected non-null value, but got null";
                throw new Exception(errorMessage);
            }
        }

        /// <summary>
        /// Marks a test as passed with an optional message.
        /// </summary>
        /// <param name="message">Optional message to display.</param>
        public void Pass(string message = null)
        {
            GD.Print(message ?? "Test passed");
        }

        /// <summary>
        /// Fails a test with the specified message.
        /// </summary>
        /// <param name="message">The failure message.</param>
        public void Fail(string message)
        {
            throw new Exception(message);
        }

        #endregion
    }
}
