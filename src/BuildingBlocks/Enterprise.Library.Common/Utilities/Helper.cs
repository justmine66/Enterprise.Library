using System;
using System.Collections.Generic;
using System.Text;

namespace Enterprise.Library.Common.Utilities
{
    /// <summary>
    /// Represents a helper class
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// eat exception
        /// </summary>
        /// <param name="action">the action.</param>
        public static void EatException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// eat exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T EatException<T>(Func<T> action, T defaultValue = default(T))
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }
}
